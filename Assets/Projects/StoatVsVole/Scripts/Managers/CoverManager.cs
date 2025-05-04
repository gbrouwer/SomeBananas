using System.Collections.Generic;
using UnityEngine;

namespace StoatVsVole
{
    /// <summary>
    /// Manages the grid-based positioning system for static agents (e.g., flowers).
    /// Uses Perlin noise to create naturalistic clustering patterns.
    /// </summary>
    public class CoverManager : MonoBehaviour
    {
        #region Fields and Settings

        [Header("Ground Settings")]
        public GlobalSettings globalSettings;
        public GameObject ground;
        private List<Vector2Int> availableCells;

        [Range(0f, 1f)]
        public float spawnLowerBound = 0.25f;
        [Range(0f, 1f)]
        public float spawnUpperBound = 0.75f;

        public float cellSize = 2.0f;

        [Header("Perlin Noise Settings")]
        public float noiseScale = 10f;
        public float noiseOffsetX = 0f;
        public float noiseOffsetY = 0f;
        public int octaves = 1;
        public float persistence = 0.5f;
        public float lacunarity = 2.0f;
        public float spawnThreshold = 0.5f;

        [Header("Neighbor Placement Settings")]
        public int goodEnoughNeighborCount = 4; // Replaces old minNeighborCount

        public float jitterFraction = 0.05f;

        [Header("Randomization Settings")]
        public bool useRandomSeed = true;
        public int randomSeed = 0;

        private int gridWidth;
        private int gridHeight;

        private float[,] noiseMap;
        private string[,] agentGrid; // Holds agentIDs (null = empty)
        public bool showNoiseTexture = false;

        #endregion

        #region Unity Lifecycle

        /// <summary>
        /// Initializes the cover grid and places static objects according to noise patterns.
        /// </summary>
        public void CreateCoverGrid()
        {
            if (useRandomSeed)
            {
                randomSeed = Random.Range(int.MinValue, int.MaxValue);
            }
            Random.InitState(randomSeed);

            if (ground == null)
            {

                Debug.LogError("GroundCoverManager: Ground reference is missing!");
                return;
            }
            SetupGridBasedOnWorldSize();
            GeneratePerlinNoiseMap();
            GenerateDebugTexture();
            InitializeAvailablePositions();
        }

        #endregion

        #region Grid and Noise Setup

        private void SetupGridBasedOnWorldSize()
        {
            if (globalSettings == null)
            {
                Debug.LogError("CoverManager: Missing reference to GlobalSettings!");
                return;
            }

            float worldSize = globalSettings.worldSize;
            gridWidth = Mathf.CeilToInt(worldSize / cellSize);
            gridHeight = Mathf.CeilToInt(worldSize / cellSize);

            noiseMap = new float[gridWidth, gridHeight];
            agentGrid = new string[gridWidth, gridHeight];
        }

        private void GeneratePerlinNoiseMap()
        {
            noiseOffsetX = Random.Range(0f, 10000f);
            noiseOffsetY = Random.Range(0f, 10000f);

            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    float amplitude = 1f;
                    float frequency = 1f;
                    float noiseHeight = 0f;
                    float maxPossibleHeight = 0f;

                    for (int i = 0; i < octaves; i++)
                    {
                        float perlinX = ((float)x / gridWidth * noiseScale + noiseOffsetX) * frequency;
                        float perlinY = ((float)y / gridHeight * noiseScale + noiseOffsetY) * frequency;

                        float perlinValue = Mathf.PerlinNoise(perlinX, perlinY) * 2f - 1f;
                        noiseHeight += perlinValue * amplitude;
                        maxPossibleHeight += amplitude;

                        amplitude *= persistence;
                        frequency *= lacunarity;
                    }

                    noiseMap[x, y] = Mathf.Clamp01((noiseHeight / maxPossibleHeight + 1f) / 2f);
                }
            }
        }

        private void GenerateDebugTexture()
        {
            Texture2D texture = new Texture2D(gridWidth, gridHeight);
            texture.filterMode = FilterMode.Point;

            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    float value = noiseMap[x, y];
                    Color color = new Color(value, value, value);
                    texture.SetPixel(x, y, color);
                }
            }

            texture.Apply();

            Renderer renderer = ground.GetComponent<Renderer>();
            if (renderer != null && showNoiseTexture)
            {
                renderer.material.mainTexture = texture;
            }
            else
            {
                renderer.material = Resources.Load<Material>("StoatVsVole/Materials/Ground");
            }

        }

        #endregion

        #region Placement Logic

        public void InitializeAvailablePositions()
        {
            availableCells = new List<Vector2Int>();

            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    if (noiseMap[x, y] > spawnThreshold)
                    {
                        Vector3 worldPos = GridToWorldPosition(x, y);

                        if (IsWithinSpawnBounds(worldPos))
                        {
                            agentGrid[x, y] = null;
                            availableCells.Add(new Vector2Int(x, y));
                        }
                    }
                }
            }
        }

        public bool TrySpawnAgent(string agentID, AgentDefinition agentDefinition, out Vector3 spawnPos)
        {
            if (agentDefinition.agentType.ToLower() == "dynamic")
            {
                spawnPos = TrySpawnDynamicAgent();
                return true;
            }
            else
            {
                return TrySpawnStaticAgent(agentID, out spawnPos);
            }
        }

        public bool TrySpawnStaticAgent(string agentID, out Vector3 spawnPos)
        {

            if (availableCells.Count == 0)
            {
                spawnPos = Vector3.zero;
                return false;
            }

            Vector2Int selectedCell = availableCells[Random.Range(0, availableCells.Count)];
            agentGrid[selectedCell.x, selectedCell.y] = agentID;
            availableCells.Remove(selectedCell);

            spawnPos = JitteredWorldPosition(selectedCell);
            return true;
        }

        private Vector3 TrySpawnDynamicAgent()
        {
            float worldSize = globalSettings.worldSize;
            float halfWorld = worldSize * 0.5f;

            int maxAttempts = 10;
            float checkRadius = 0.5f; // Depends on agent size
            int layerMask = Physics.DefaultRaycastLayers; // Includes Default layer (agents + ground)

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                Vector3 randomPosition = new Vector3(
                    Random.Range(-halfWorld, halfWorld),
                    0f, // No need to lift up now
                    Random.Range(-halfWorld, halfWorld)
                );

                Collider[] overlaps = Physics.OverlapSphere(randomPosition, checkRadius, layerMask);

                bool agentFound = false;
                foreach (var col in overlaps)
                {
                    if (col.CompareTag("agent")) // Only care about collisions with other agents
                    {
                        agentFound = true;
                        break;
                    }
                }

                if (!agentFound)
                {
                    return randomPosition;
                }
            }

            Debug.LogWarning("CoverManager: Could not find a free spawn position after max attempts.");
            return Vector3.zero;
        }

        public bool TryRespawnAgent(string agentID, AgentDefinition agentDefinition, out Vector3 spawnPos)
        {
            if (agentDefinition.agentType.ToLower() == "dynamic")
            {
                return TrySpawnAgent(agentID, agentDefinition, out spawnPos); // same as normal
            }
            else
            {
                return TrySpawnNewAgentByNeighboringClustering(agentID, out spawnPos); // clustering
            }
        }

        public bool TrySpawnNewAgentByNeighboringClustering(string agentID, out Vector3 spawnPos)
        {
            List<Vector2Int> emptyCells = new List<Vector2Int>();

            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    if (agentGrid[x, y] == null)
                        emptyCells.Add(new Vector2Int(x, y));
                }
            }

            Utils.Shuffle(emptyCells); // Randomize the traversal order

            Vector2Int? bestCell = null;
            int maxNeighbors = -1;

            foreach (var cell in emptyCells)
            {
                int neighborCount = CountAgentNeighbors(cell.x, cell.y);

                if (neighborCount > maxNeighbors)
                {
                    maxNeighbors = neighborCount;
                    bestCell = cell;

                    if (neighborCount >= goodEnoughNeighborCount)
                        break; // ✅ Early exit
                }
            }

            if (bestCell.HasValue)
            {
                Vector2Int selected = bestCell.Value;
                agentGrid[selected.x, selected.y] = agentID;
                spawnPos = JitteredWorldPosition(selected);
                return true;
            }

            spawnPos = Vector3.zero;
            Debug.LogWarning("CoverManager: No valid grid cell found for clustered flower placement!");
            return false;
        }

        public void RemoveAgent(string agentID)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    if (agentGrid[x, y] == agentID)
                    {
                        agentGrid[x, y] = null;

                        // 🛠️ Fix: re-add the cell to availableCells
                        availableCells.Add(new Vector2Int(x, y));

                        return;
                    }
                }
            }
            Debug.LogWarning($"GroundCoverManager: Tried to remove agent {agentID} but it wasn't found.");
        }

        #endregion

        #region Utility

        private int CountAgentNeighbors(int x, int y)
        {
            int count = 0;
            Vector2Int[] neighborOffsets = {
                new Vector2Int(-1, 0), new Vector2Int(1, 0),
                new Vector2Int(0, -1), new Vector2Int(0, 1),
                new Vector2Int(-1, -1), new Vector2Int(-1, 1),
                new Vector2Int(1, -1), new Vector2Int(1, 1)
            };

            foreach (var offset in neighborOffsets)
            {
                int nx = x + offset.x;
                int ny = y + offset.y;

                if (nx >= 0 && nx < gridWidth && ny >= 0 && ny < gridHeight)
                {
                    if (agentGrid[nx, ny] != null)
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        private Vector3 GridToWorldPosition(int gridX, int gridY)
        {
            float halfWorldSize = globalSettings.worldSize / 2f;

            float cellWidth = globalSettings.worldSize / gridWidth;
            float cellHeight = globalSettings.worldSize / gridHeight;

            float worldX = -halfWorldSize + (gridX * cellWidth) + (cellWidth / 2f);
            float worldZ = -halfWorldSize + (gridY * cellHeight) + (cellHeight / 2f);

            return new Vector3(worldX, 0f, worldZ);
        }

        private Vector3 JitteredWorldPosition(Vector2Int cell)
        {
            Vector3 basePosition = GridToWorldPosition(cell.x, cell.y);

            float cellWidth = globalSettings.worldSize / gridWidth;
            float cellHeight = globalSettings.worldSize / gridHeight;

            float maxJitterX = (cellWidth / 2f) * jitterFraction;
            float maxJitterZ = (cellHeight / 2f) * jitterFraction;

            float jitterX = 0;//Random.Range(-maxJitterX, maxJitterX);
            float jitterZ = 0;//Random.Range(-maxJitterZ, maxJitterZ);

            return new Vector3(basePosition.x + jitterX, basePosition.y, basePosition.z + jitterZ);
        }

        private bool IsWithinSpawnBounds(Vector3 worldPos)
        {
            float halfWorldSize = globalSettings.worldSize / 2f;

            float lowerX = Mathf.Lerp(-halfWorldSize, halfWorldSize, spawnLowerBound);
            float upperX = Mathf.Lerp(-halfWorldSize, halfWorldSize, spawnUpperBound);
            float lowerZ = Mathf.Lerp(-halfWorldSize, halfWorldSize, spawnLowerBound);
            float upperZ = Mathf.Lerp(-halfWorldSize, halfWorldSize, spawnUpperBound);

            return worldPos.x >= lowerX && worldPos.x <= upperX &&
                   worldPos.z >= lowerZ && worldPos.z <= upperZ;
        }

        #endregion
    }
}

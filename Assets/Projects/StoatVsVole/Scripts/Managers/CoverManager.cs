using System.Collections.Generic;
using UnityEngine;

namespace StoatVsVole
{
    public class CoverManager : MonoBehaviour
    {
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
        public int minNeighborCount = 3;
        public float jitterFraction = 0.05f;

        [Header("Randomization Settings")]
        public bool useRandomSeed = true;
        public int randomSeed = 0;

        private int gridWidth;
        private int gridHeight;

        private float[,] noiseMap;
        private string[,] agentGrid; // Grid holding agentIDs (null = empty)

        public void CreateCoverGrid(float agentSize)
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
            InitializeAvailablePositions(agentSize);
        }

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
            texture.filterMode = FilterMode.Point; // Pixel perfect

            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    float value = noiseMap[x, y];
                    Color color = new Color(value, value, value);
                    texture.SetPixel(x, y, color); // Flip vertically
                }
            }

            texture.Apply();

            Renderer renderer = ground.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.mainTexture = texture;
            }
        }
        public void InitializeAvailablePositions(float agentSize)
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

  public bool TryPlaceAgent(string agentID, out Vector3 spawnPos)
{
    if (availableCells.Count == 0)
    {
        spawnPos = Vector3.zero;
        return false;
    }

    Vector2Int selectedCell = availableCells[Random.Range(0, availableCells.Count)];
    agentGrid[selectedCell.x, selectedCell.y] = agentID;
    availableCells.Remove(selectedCell);

    Vector3 basePosition = GridToWorldPosition(selectedCell.x, selectedCell.y);

    float cellWidth = globalSettings.worldSize / gridWidth;
    float cellHeight = globalSettings.worldSize / gridHeight;

    float maxJitterX = (cellWidth / 2f) * jitterFraction;
    float maxJitterZ = (cellHeight / 2f) * jitterFraction;

    float jitterX = UnityEngine.Random.Range(-maxJitterX, maxJitterX);
    float jitterZ = UnityEngine.Random.Range(-maxJitterZ, maxJitterZ);

    spawnPos = new Vector3(
        basePosition.x + jitterX,
        basePosition.y,
        basePosition.z + jitterZ
    );

    return true;
}

public bool TryPlaceNewAgentByNeighboringClustering(string agentID, out Vector3 spawnPos)
{
    List<Vector2Int> candidateCells = new List<Vector2Int>();

    for (int x = 0; x < gridWidth; x++)
    {
        for (int y = 0; y < gridHeight; y++)
        {
            if (agentGrid[x, y] != null)
                continue; // Already occupied

            int neighborCount = 0;

            Vector2Int[] neighborOffsets = new Vector2Int[]
            {
                new Vector2Int(-1, 0),  // Left
                new Vector2Int(1, 0),   // Right
                new Vector2Int(0, -1),  // Down
                new Vector2Int(0, 1),   // Up
                new Vector2Int(-1, -1), // Down-Left
                new Vector2Int(-1, 1),  // Up-Left
                new Vector2Int(1, -1),  // Down-Right
                new Vector2Int(1, 1)    // Up-Right
            };

            foreach (var offset in neighborOffsets)
            {
                int nx = x + offset.x;
                int ny = y + offset.y;

                if (nx >= 0 && nx < gridWidth && ny >= 0 && ny < gridHeight)
                {
                    if (agentGrid[nx, ny] != null)
                    {
                        neighborCount++;

                        if (neighborCount >= minNeighborCount)
                        {
                            candidateCells.Add(new Vector2Int(x, y));
                            break; // Early exit
                        }
                    }
                }
            }
        }
    }

    if (candidateCells.Count == 0)
    {
        Debug.LogWarning("CoverManager: No candidate cells found for neighbor-based placement!");
        spawnPos = Vector3.zero;
        return false;
    }

    Vector2Int selectedCell = candidateCells[Random.Range(0, candidateCells.Count)];

    agentGrid[selectedCell.x, selectedCell.y] = agentID;

    Vector3 basePosition = GridToWorldPosition(selectedCell.x, selectedCell.y);

    float cellWidth = globalSettings.worldSize / gridWidth;
    float cellHeight = globalSettings.worldSize / gridHeight;

    float maxJitterX = (cellWidth / 2f) * jitterFraction;
    float maxJitterZ = (cellHeight / 2f) * jitterFraction;

    float jitterX = UnityEngine.Random.Range(-maxJitterX, maxJitterX);
    float jitterZ = UnityEngine.Random.Range(-maxJitterZ, maxJitterZ);

    spawnPos = new Vector3(
        basePosition.x + jitterX,
        basePosition.y,
        basePosition.z + jitterZ
    );

    return true;
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
                        return;
                    }
                }
            }

            Debug.LogWarning($"GroundCoverManager: Tried to remove agent {agentID} but it wasn't found.");
        }

        private Vector3 GridCellToWorldPosition(Vector2Int cell, float size)
        {

            float jitterX = Random.Range(-cellSize * 0.04f, cellSize * 0.04f);
            float jitterZ = Random.Range(-cellSize * 0.04f, cellSize * 0.04f);

            Vector3 worldPos = new Vector3(
                (cell.x + 0.5f) * cellSize + jitterX + ground.transform.position.x - (gridWidth * cellSize * 0.5f) - size * 0.5f,
                ground.transform.position.y,
                (cell.y + 0.5f) * cellSize + jitterZ + ground.transform.position.z - (gridHeight * cellSize * 0.5f) - size * 0.5f
            );

            return worldPos;
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


        // private bool IsPositionWithinSpawnBounds(Vector3 position)
        // {
        //     Vector3 planeCenter = groundPlaneTransform.position;
        //     Vector3 planeSize = new Vector3(
        //         groundPlaneTransform.localScale.x * 10f,
        //         0f,
        //         groundPlaneTransform.localScale.z * 10f
        //     );

        //     float halfSizeX = planeSize.x / 2f;
        //     float halfSizeZ = planeSize.z / 2f;

        //     float xMin = planeCenter.x - halfSizeX + (planeSize.x * spawnLowerBound);
        //     float xMax = planeCenter.x - halfSizeX + (planeSize.x * spawnUpperBound);
        //     print(xMin);
        //     float zMin = planeCenter.z - halfSizeZ + (planeSize.z * spawnLowerBound);
        //     float zMax = planeCenter.z - halfSizeZ + (planeSize.z * spawnUpperBound);

        //     return (position.x >= xMin && position.x <= xMax) &&
        //            (position.z >= zMin && position.z <= zMax);
        // }        
    }

}

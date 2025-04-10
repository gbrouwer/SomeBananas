using UnityEngine;

namespace StoatVsVole
{
    /// <summary>
    /// Spawns decorative, non-interactive environment elements across a plane or terrain surface.
    /// Placement can be randomized, bounded, and aligned to procedural surfaces.
    /// </summary>
    public class EnvironmentElementSpawner : MonoBehaviour
    {
        #region Fields and Settings

        [Header("Spawn Settings")]
        public string resourceFolderPath;
        public int resolution = 32;
        public float spawnProbability = 1f;
        public float verticalRangeMin = 5f;
        public float verticalRangeMax = 10f;
        public float randomRotation = 360f;
        public float randomScaleMin = 1f;
        public float randomScaleMax = 1f;
        public float extent = 1f;
        public bool invertExtent = false;
        public bool useCircularBounds = false;
        public bool alignToSurface = true;
        public float surfaceOffet = 1.0f;
        public Transform PlaneTransform;
        public string tagToAssign = "Untagged";
        public int layerToAssign = 0;

        private GameObject[] prefabs;
        private Bounds bounds;

        #endregion

        #region Public Methods

        /// <summary>
        /// Loads all prefabs from the specified resource folder path.
        /// </summary>
        public void Prepare()
        {
            prefabs = Resources.LoadAll<GameObject>(resourceFolderPath);
            if (prefabs == null || prefabs.Length == 0)
            {
                Debug.LogWarning($"No prefabs found in Resources/{resourceFolderPath}");
            }
        }

        /// <summary>
        /// Spawns environment elements based on noise, probability, and optional surface alignment.
        /// </summary>
        /// <param name="ground">Ground generator providing height information for alignment.</param>
        public void Spawn(EnvironmentGroundSpawner ground)
        {
            if (ground == null)
            {
                Debug.LogWarning("No EnvironmentGroundSpawner provided.");
                return;
            }

            bounds = PlaneTransform.GetComponent<MeshRenderer>().bounds;

            float stepX = bounds.size.x / resolution;
            float stepZ = bounds.size.z / resolution;
            float radius = Mathf.Min(bounds.size.x, bounds.size.z) * extent * 0.5f;
            Vector2 center = new Vector2(bounds.center.x, bounds.center.z);

            for (int x = 0; x < resolution; x++)
            {
                for (int z = 0; z < resolution; z++)
                {
                    float px = bounds.min.x + stepX * x + Random.Range(0, stepX);
                    float pz = bounds.min.z + stepZ * z + Random.Range(0, stepZ);

                    Vector3 spawnPosition = new Vector3(px, 0f, pz);

                    float noiseX = (float)x / resolution;
                    float noiseZ = (float)z / resolution;
                    float sample = Mathf.PerlinNoise(noiseX, noiseZ);
                    float randomThreshold = Random.value;

                    if (randomThreshold > spawnProbability * sample)
                        continue;

                    if (extent < 1f)
                    {
                        Vector2 pos2D = new Vector2(px, pz);
                        float distFromCenter = Vector2.Distance(pos2D, center);
                        if ((!invertExtent && distFromCenter > radius) || (invertExtent && distFromCenter < radius))
                            continue;
                    }

                    float py = alignToSurface
                        ? ground.GetHeightAtWorldPosition(spawnPosition)
                        : Random.Range(verticalRangeMin, verticalRangeMax);

                    py += surfaceOffet;
                    spawnPosition.y = py;

                    GameObject prefab = prefabs[Random.Range(0, prefabs.Length)];

                    if (IsDebugOrOverlayPrefab(prefab))
                        continue;

                    GameObject instance = Instantiate(prefab, spawnPosition, Quaternion.identity, transform);

                    float scale = Random.Range(randomScaleMin, randomScaleMax);
                    instance.transform.localScale = Vector3.one * scale;
                    instance.transform.eulerAngles = new Vector3(
                        0f,
                        Random.Range(0, randomRotation),
                        0f
                    );

                    instance.tag = tagToAssign;
                    instance.layer = layerToAssign;
                }
            }
        }

        #endregion

        #region Private Helpers

        /// <summary>
        /// Determines if the prefab should be skipped because it's a debug, UI, or stats element.
        /// </summary>
        private bool IsDebugOrOverlayPrefab(GameObject prefab)
        {
            if (prefab.GetComponentInChildren<UnityEngine.Canvas>() != null)
                return true;

            string lowerName = prefab.name.ToLower();
            return lowerName.Contains("debug") || lowerName.Contains("overlay") || lowerName.Contains("stats");
        }

        #endregion
    }
}

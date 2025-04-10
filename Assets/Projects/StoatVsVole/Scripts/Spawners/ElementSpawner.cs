using UnityEngine;

namespace StoatVsVole
{
    public class EnvironmentElementSpawner : MonoBehaviour
    {
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

        public void Prepare()
        {
            prefabs = Resources.LoadAll<GameObject>(resourceFolderPath);
            if (prefabs == null || prefabs.Length == 0)
            {
                Debug.LogWarning($"No prefabs found in Resources/{resourceFolderPath}");
            }
        }

        public void Spawn(EnvironmentGroundSpawner ground)
        {
            if (ground == null)
            {
                Debug.LogWarning("No EnvironmentGroundSpawner provided.");
                return;
            }

            bounds = PlaneTransform.GetComponent<MeshRenderer>().bounds;

            // bounds = PlaneTransform.GetComponent<MeshRenderer>().bounds;
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
                    if (randomThreshold > spawnProbability * sample) continue;

                    if (extent < 1f)
                    {
                        Vector2 pos2D = new Vector2(px, pz);
                        float distFromCenter = Vector2.Distance(pos2D, center);
                        if ((!invertExtent && distFromCenter > radius) || (invertExtent && distFromCenter < radius))
                            continue;
                    }

                    float py = alignToSurface ? ground.GetHeightAtWorldPosition(spawnPosition) : Random.Range(verticalRangeMin, verticalRangeMax);
                    py = py + surfaceOffet;
                    spawnPosition.y = py;

                    GameObject prefab = prefabs[Random.Range(0, prefabs.Length)];

                    if (prefab.GetComponentInChildren<UnityEngine.Canvas>() != null ||
                        prefab.name.ToLower().Contains("debug") ||
                        prefab.name.ToLower().Contains("overlay") ||
                        prefab.name.ToLower().Contains("stats"))
                    {
                        // Debug.LogWarning($"[Spawner] Skipping debug-related prefab: {prefab.name}");
                        continue;
                    }

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
    }
}

using UnityEngine;

namespace StoatVsVole
{
    public class EnvironmentWallSpawner : MonoBehaviour
    {
        [Header("Wall Settings")]
        public Material wallMaterial;
        public float wallHeight = 10f;
        public float wallThickness = 1f;
        public string wallTag = "Wall";
        [Tooltip("Layer to assign to wall objects")] public int wallLayer = 0;

        [Header("Global Settings")]
        public GlobalSettings globalSettings;

        public void SpawnWalls()
        {
            SpawnBoxWalls();
        }

        private void SpawnBoxWalls()
        {
            float halfWorldSize = globalSettings.worldSize / 2f;

            float x = 0f;
            float z = 0f;
            float width = globalSettings.worldSize;
            float depth = globalSettings.worldSize;

            Vector3[] positions = new Vector3[]
            {
                new Vector3(x, 0, z - depth / 2 - wallThickness / 2), // back
                new Vector3(x, 0, z + depth / 2 + wallThickness / 2), // front
                new Vector3(x - width / 2 - wallThickness / 2, 0, z), // left
                new Vector3(x + width / 2 + wallThickness / 2, 0, z), // right
            };

            Vector3[] scales = new Vector3[]
            {
                new Vector3(width + 2 * wallThickness, wallHeight, wallThickness),
                new Vector3(width + 2 * wallThickness, wallHeight, wallThickness),
                new Vector3(wallThickness, wallHeight, depth + 2 * wallThickness),
                new Vector3(wallThickness, wallHeight, depth + 2 * wallThickness),
            };

            for (int i = 0; i < positions.Length; i++)
            {
                GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                wall.transform.position = positions[i];
                wall.transform.localScale = scales[i];
                wall.GetComponent<Renderer>().material = wallMaterial;
                wall.tag = wallTag;
                wall.layer = wallLayer;
                wall.transform.parent = transform;
            }
        }
    }
}

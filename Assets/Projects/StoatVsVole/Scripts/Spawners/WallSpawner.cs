using UnityEngine;

namespace StoatVsVole
{
    /// <summary>
    /// Spawns invisible or visible walls around the simulation area, 
    /// based on the GlobalSettings world size.
    /// Used to constrain dynamic agents within the environment.
    /// </summary>
    public class EnvironmentWallSpawner : MonoBehaviour
    {
        #region Wall Settings

        [Header("Wall Settings")]
        public Material wallMaterial;
        public float wallHeight = 10f;
        public float wallThickness = 1f;
        public string wallTag = "Wall";
        [Tooltip("Layer to assign to wall objects")] 
        public int wallLayer = 0;

        [Header("Global Settings")]
        public GlobalSettings globalSettings;

        #endregion

        #region Public Methods

        /// <summary>
        /// Entry point to spawn the simulation boundary walls.
        /// </summary>
        public void SpawnWalls()
        {
            SpawnBoxWalls();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Spawns four box walls around the simulation area, forming a closed boundary.
        /// </summary>
        private void SpawnBoxWalls()
        {
            float halfWorldSize = globalSettings.worldSize / 2f;
            float x = 0f;
            float z = 0f;
            float width = globalSettings.worldSize;
            float depth = globalSettings.worldSize;

            Vector3[] positions = new Vector3[]
            {
                new Vector3(x, 0, z - depth / 2 - wallThickness / 2), // Back wall
                new Vector3(x, 0, z + depth / 2 + wallThickness / 2), // Front wall
                new Vector3(x - width / 2 - wallThickness / 2, 0, z), // Left wall
                new Vector3(x + width / 2 + wallThickness / 2, 0, z), // Right wall
            };

            Vector3[] scales = new Vector3[]
            {
                new Vector3(width + 2 * wallThickness, wallHeight, wallThickness), // Back wall
                new Vector3(width + 2 * wallThickness, wallHeight, wallThickness), // Front wall
                new Vector3(wallThickness, wallHeight, depth + 2 * wallThickness), // Left wall
                new Vector3(wallThickness, wallHeight, depth + 2 * wallThickness), // Right wall
            };

            for (int i = 0; i < positions.Length; i++)
            {
                GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                wall.transform.position = positions[i];
                wall.transform.localScale = scales[i];

                Renderer wallRenderer = wall.GetComponent<Renderer>();
                if (wallRenderer != null)
                {
                    wallRenderer.material = wallMaterial;
                }

                wall.tag = wallTag;
                wall.layer = wallLayer;
                wall.transform.parent = transform;
            }
        }

        #endregion
    }
}

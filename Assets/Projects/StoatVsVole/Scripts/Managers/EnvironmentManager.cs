using UnityEngine;
using System.Collections;

namespace StoatVsVole
{
    /// <summary>
    /// Orchestrates the initialization of environment components: ground, walls, and decorative elements.
    /// Ensures the ground is generated before spawning dependent elements.
    /// </summary>
    public class EnvironmentManager : MonoBehaviour
    {
        #region Private Fields

        private EnvironmentGroundSpawner groundSpawner;
        private EnvironmentWallSpawner wallSpawner;
        private EnvironmentElementSpawner[] spawners;

        #endregion

        #region Unity Lifecycle

        /// <summary>
        /// Coroutine that sequentially initializes the environment at startup.
        /// </summary>
        private IEnumerator Start()
        {
            groundSpawner = GetComponentInChildren<EnvironmentGroundSpawner>();
            wallSpawner = GetComponentInChildren<EnvironmentWallSpawner>();
            spawners = GetComponentsInChildren<EnvironmentElementSpawner>();

            if (groundSpawner != null)
            {
                groundSpawner.GenerateGround();
                yield return null; // Yield to ensure mesh and heightmap data are ready
            }

            if (wallSpawner != null)
            {
                wallSpawner.SpawnWalls();
            }

            foreach (var spawner in spawners)
            {
                spawner.Prepare();
                spawner.Spawn(groundSpawner);
            }
        }

        #endregion
    }
}

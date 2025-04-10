using UnityEngine;
using System.Collections;

namespace StoatVsVole
{
    public class EnvironmentManager : MonoBehaviour
    {
        private EnvironmentGroundSpawner groundSpawner;
        private EnvironmentWallSpawner wallSpawner;
        private EnvironmentElementSpawner[] spawners;

        private IEnumerator Start()
        {
            groundSpawner = GetComponentInChildren<EnvironmentGroundSpawner>();
            wallSpawner = GetComponentInChildren<EnvironmentWallSpawner>();
            spawners = GetComponentsInChildren<EnvironmentElementSpawner>();

            if (groundSpawner != null)
            {
                groundSpawner.GenerateGround();
                yield return null; // Ensure mesh and height data are ready
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
    }
}

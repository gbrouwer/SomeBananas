// using UnityEngine;
// using System.Collections;
// using StoatVsVole;

// namespace StoatVsVole
// {
//     public class EpisodeManager : MonoBehaviour
//     {
//         public AgentManager agentManager;
//         public EnvironmentManager environmentManager;

//         [Header("Episode Timing")]
//         public float episodeDuration = 60f; // seconds
//         private float timer = 0f;

//         private bool episodeRunning = false;

//         void Start()
//         {
//             StartEpisode();
//         }

//         void Update()
//         {
//             if (!episodeRunning) return;

//             timer += Time.deltaTime;

//             if (timer >= episodeDuration)
//             {
//                 EndEpisode();
//             }
//         }

//         public void StartEpisode()
//         {
//             timer = 0f;
//             episodeRunning = true;

//             // environmentManager.ResetObstacles();
//             agentManager.ResetAgents();
//         }

//         public void EndEpisode()
//         {
//             episodeRunning = false;
//             Debug.Log("Episode ended. Resetting...");
//             StartEpisode();
//         }
//     }
// }

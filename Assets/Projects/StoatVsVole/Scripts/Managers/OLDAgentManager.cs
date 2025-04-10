// using System.Collections.Generic;
// using UnityEngine;
// using System.Linq;
// using Unity.MLAgents;
// using StoatVsVole;
// using UnityEngine.UIElements;

// namespace StoatVsVole
// {
//     public class AgentManager : MonoBehaviour
//     {
//         [Header("Agent Settings")]
//         public GameObject preyPrefab;
//         public GameObject predatorPrefab;
//         public int numberOfPrey = 5;
//         public int numberOfPredators = 2;
//         public Transform spawnArea;

//         private List<GameObject> agents = new List<GameObject>();
//         private List<Vector3> spawnPositions;

//         void Start()
//         {
//             CreateSpawnPositions();
//             SpawnAgents();
//         }

//         private void CreateSpawnPositions()
//         {
//             int totalAgents = numberOfPrey + numberOfPredators;
//             int gridSize = Mathf.CeilToInt(Mathf.Sqrt(totalAgents));
//             float spacing = 2f;
//             spawnPositions = new List<Vector3>();

//             Vector3 center = spawnArea.position;
//             for (int i = 0; i < gridSize; i++)
//             {
//                 for (int j = 0; j < gridSize; j++)
//                 {
//                     float x = center.x + (i - gridSize / 2f) * spacing;
//                     float z = center.z + (j - gridSize / 2f) * spacing;
//                     float y = center.y + 0.5f;
//                     spawnPositions.Add(new Vector3(x, y, z));
//                 }
//             }
//         }

//         public void SpawnAgents()
//         {
//             ClearAgents();
//             List<Vector3> selectedPositions = spawnPositions.OrderBy(x => Random.value).Take(numberOfPrey + numberOfPredators).ToList();

//             for (int i = 0; i < numberOfPrey; i++)
//             {
//                 Vector3 position = selectedPositions[i];
//                 Quaternion rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
//                 GameObject agent = Instantiate(preyPrefab, position, rotation);
//                 agents.Add(agent);
//             }

//             for (int i = 0; i < numberOfPredators; i++)
//             {
//                 Vector3 position = selectedPositions[numberOfPrey + i];
//                 Quaternion rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
//                 GameObject agent = Instantiate(predatorPrefab, position, rotation);
//                 agents.Add(agent);
//             }
//         }

//         public void ResetAgents()
//         {
//             List<Vector3> selectedPositions = spawnPositions.OrderBy(x => Random.value).Take(numberOfPrey + numberOfPredators).ToList();

//             for (int i = 0; i < agents.Count; i++)
//             {
//                 GameObject agent = agents[i];
//                 Vector3 position = selectedPositions[i];
//                 agent.transform.position = position;
//                 agent.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

//                 var agentComponent = agent.GetComponent<AgentBase>();
//                 if (agentComponent != null)
//                 {
//                     //agentComponent.Reset();
//                 }
//             }
//         }

//         public void ClearAgents()
//         {
//             foreach (var agent in agents)
//             {
//                 Destroy(agent);
//             }
//             agents.Clear();
//         }

//         public List<GameObject> GetAllAgents() => agents;
//     }
// }

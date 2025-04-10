using UnityEngine;
using System.Collections.Generic;
using System.IO;

namespace StoatVsVole
{
    public class Manager : MonoBehaviour
    {
        [Header(" Manager Settings")]
        [Tooltip("Path to the agent preset JSON.")]
        private const string presetBasePath = "Assets/SomeBananas/Projects/StoatVsVole/Data/Presets/";
        public string PresetFile = "FlowerConfig.json";

        // public GameObject groundObject;
        public int initialAgentCount = 100;
        public int poolSize = 200;

        private Transform agentParent;
        private List<GameObject> agentPool = new List<GameObject>();
        private List<IAgentLifecycle> activeAgents = new List<IAgentLifecycle>();
        private AgentDefinition agentDefinition;

        public int PotentialPopulationCount { get; private set; }
        public int ActiveCount => activeAgents.Count;
        public float ReplicationFraction { get; private set; }
        public float MeanAge { get; private set; }
        public float agentSize = 1.0f;

        string newAgentID;

        [SerializeField]
        private AgentSpawner agentSpawner;

        [SerializeField]
        private CoverManager coverManager;

        private string GenerateUniqueAgentID()
        {
            return System.Guid.NewGuid().ToString();
        }

        private void Awake()
        {
            agentParent = new GameObject("Agents").transform;
            agentParent.SetParent(this.transform);
            PotentialPopulationCount = poolSize;
        }

        private void Start()
        {

            coverManager.CreateCoverGrid(agentSize);
            LoadAgentDefinition();
            CreateAgentPool();
            SpawnInitialAgents();
        }

        private void Update()
        {
            UpdateMeanAge();
        }

        private void LoadAgentDefinition()
        {
            string path = Path.Combine(presetBasePath, PresetFile);
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                agentDefinition = JsonUtility.FromJson<AgentDefinition>(json);
            }
            else
            {
                Debug.LogError("Agent preset file not found at: " + path);
            }
        }

        private void CreateAgentPool()
        {
            for (int i = 0; i < poolSize; i++)
            {
                Vector3 dummyPosition = Vector3.zero;
                Quaternion dummyRotation = Quaternion.identity;
                GameObject agentObject = agentSpawner.SpawnAgentFromDefinition(agentDefinition, dummyPosition, dummyRotation);
                if (agentObject != null)
                {
                    agentObject.transform.SetParent(agentParent);
                    agentObject.SetActive(false);
                    agentPool.Add(agentObject);
                }
            }
        }

        private void SpawnInitialAgents()
        {
            for (int i = 0; i < initialAgentCount; i++)
            {
                newAgentID = GenerateUniqueAgentID();
                SpawnAgent(newAgentID);
            }
        }

        private void SpawnAgent(string id)
        {
            GameObject agentObject = GetPooledAgent();
            if (agentObject != null)
            {
                IAgentLifecycle agentLifecycle = agentObject.GetComponent<IAgentLifecycle>();
                if (agentLifecycle is AgentController Agent)
                {
                    Agent.InitializeFromDefinition(agentDefinition); // attaches definition + resets once
                    Agent.SetAgentID(id);
                    Agent.SetManager(this);
                    Agent.RandomizeMaxAge(100);
                    Agent.RandomizeReplicationAge(25);
                }


                Vector3 spawnPos;
                if (coverManager.TryPlaceAgent(id, out spawnPos))
                {
                    spawnPos = new Vector3(spawnPos.x, agentSize, spawnPos.z);
                    Quaternion spawnRot = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                    agentObject.transform.position = spawnPos;
                    agentObject.transform.rotation = spawnRot;
                    agentObject.SetActive(true);
                    activeAgents.Add(agentLifecycle);
                }
                else
                {
                    Debug.LogWarning($"AgentManager: Could not place agent {id}, no available positions!");
                    Destroy(agentObject);
                }
            }


        }

        private GameObject GetPooledAgent()
        {
            foreach (GameObject agentObject in agentPool)
            {
                if (!agentObject.activeInHierarchy)
                {
                    return agentObject;
                }
            }
            Debug.LogWarning("No pooled agents available!");
            return null;
        }

        private void UpdateMeanAge()
        {
            if (activeAgents.Count == 0)
            {
                MeanAge = 0f;
                return;
            }

            float totalAge = 0f;
            int replicationCount = 0;
            foreach (var agent in activeAgents)
            {
                totalAge += agent.GetAge();
                if (agent.HasReplicated())
                    replicationCount++;
            }

            MeanAge = totalAge / activeAgents.Count;
            ReplicationFraction = (float) replicationCount / (float)activeAgents.Count;
        }

        public void OnExpired(IAgentLifecycle agent)
        {
            if (activeAgents.Contains(agent))
            {
                // Remove Agent
                MonoBehaviour agentMono = agent as MonoBehaviour;
                GameObject agentObject = agentMono.gameObject;
                activeAgents.Remove(agent);
                coverManager.RemoveAgent(agent.GetAgentID());
                agentObject.SetActive(false);

                // Replace it only when it has replicated
                if (agent.HasReplicated())
                {
                    agent.ResetState();
                    Vector3 spawnPos;
                    if (coverManager.TryPlaceNewAgentByNeighboringClustering(agent.GetAgentID(), out spawnPos))
                    {
                        spawnPos = new Vector3(spawnPos.x, agentSize, spawnPos.z);
                        Quaternion spawnRot = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                        agentObject.transform.position = spawnPos;
                        agentObject.transform.rotation = spawnRot;
                        agentObject.SetActive(true);
                        activeAgents.Add(agent);
                    }
                    else
                    {
                        Debug.LogWarning($"AgentManager: Could not place new agent {agent.GetAgentID()} by clustering!");
                        Destroy(agentObject);
                    }
                }

            }
            else
            {
                Debug.LogWarning("Manager: Tried to expire an agent not in active list!");
            }
        }

        public void OnReplicated(IAgentLifecycle agent)
        {
        }
    }
}

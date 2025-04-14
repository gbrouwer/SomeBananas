using UnityEngine;
using System.Collections.Generic;
using System.IO;

namespace StoatVsVole
{
    /// <summary>
    /// Manager class responsible for pooling, spawning, recycling, and overseeing the lifecycle of all agents.
    /// Agents are autonomous; this manager only handles creation and placement logistics.
    /// </summary>
    public class AgentManager : MonoBehaviour
    {
        #region Fields and Settings

        [Header("Manager Settings")]
        [Tooltip("Path to the agent preset JSON.")]
        private const string presetBasePath = "Assets/Projects/StoatVsVole/Data/Presets/";
        public string PresetFile = "FlowerConfig.json";

        public int initialAgentCount = 100;
        public int poolSize = 200;
        public float agentSize = 1.0f;

        private Transform agentParent;
        private List<GameObject> agentPool = new List<GameObject>();
        private List<IAgentLifecycle> activeAgents = new List<IAgentLifecycle>();
        private AgentDefinition agentDefinition;
        private string newAgentID;

        [SerializeField]
        private AgentInstantiator agentInstantiator;

        [SerializeField]
        private CoverManager coverManager;

        public int PotentialPopulationCount { get; private set; }
        public int ActiveCount => activeAgents.Count;
        public float ReplicationFraction { get; private set; }
        public float MeanAge { get; private set; }

        #endregion

        #region Unity Lifecycle

        /// <summary>
        /// Unity Awake. Initializes agent parent container and sets potential population.
        /// </summary>
        private void Awake()
        {
            agentParent = new GameObject("Agents").transform;
            agentParent.SetParent(this.transform);
            PotentialPopulationCount = poolSize;
        }

        /// <summary>
        /// Unity Start. Loads agent definitions, creates the pool, and spawns initial agents.
        /// </summary>
        private void Start()
        {
            coverManager.CreateCoverGrid(agentSize);
            LoadAgentDefinition();
            CreateAgentPool();
            SpawnInitialAgents();
        }

        /// <summary>
        /// Unity Update. Continuously updates mean age and replication metrics.
        /// </summary>
        private void Update()
        {
            UpdateMeanAge();
        }

        #endregion

        #region Agent Pooling and Spawning

        /// <summary>
        /// Loads agent settings from a JSON definition file.
        /// </summary>
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

        /// <summary>
        /// Creates an inactive pool of agent instances ready for spawning.
        /// </summary>
        private void CreateAgentPool()
        {
            for (int i = 0; i < poolSize; i++)
            {
                Vector3 dummyPosition = Vector3.zero;
                Quaternion dummyRotation = Quaternion.identity;
                GameObject agentObject = agentInstantiator.InstantiateAgentFromDefinition(agentDefinition, dummyPosition, dummyRotation);
                if (agentObject != null)
                {
                    agentObject.transform.SetParent(agentParent);
                    agentObject.SetActive(false);
                    agentPool.Add(agentObject);
                }
            }
        }

        /// <summary>
        /// Spawns the initial batch of agents into the environment.
        /// </summary>
        private void SpawnInitialAgents()
        {
            for (int i = 0; i < initialAgentCount; i++)
            {
                newAgentID = GenerateUniqueAgentID();
                SpawnAgent(newAgentID);
            }
        }

        /// <summary>
        /// Spawns an individual agent using an available pooled object.
        /// </summary>
        private void SpawnAgent(string id)
        {
            GameObject agentObject = GetPooledAgent();
            Bounds totalBound = Utils.CalculateTotalBounds(agentObject);
            if (agentObject != null)
            {
                IAgentLifecycle agentLifecycle = agentObject.GetComponent<IAgentLifecycle>();
                if (agentLifecycle is AgentController Agent)
                {
                    Agent.InitializeFromDefinition(agentDefinition);
                    Agent.SetAgentID(id);
                    Agent.SetManager(this);
                    Agent.RandomizeMaxAge(100);
                    Agent.RandomizeReplicationAge(25);
                }

                Vector3 spawnPos;

                if (coverManager.TrySpawnAgent(id, agentDefinition, out spawnPos))
                {
                    spawnPos = new Vector3(spawnPos.x, totalBound.extents.y, spawnPos.z);
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

        /// <summary>
        /// Retrieves a free, inactive agent from the pool.
        /// </summary>
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

        /// <summary>
        /// Generates a unique ID for each agent (GUID-based).
        /// </summary>
        private string GenerateUniqueAgentID()
        {
            return System.Guid.NewGuid().ToString();
        }

        #endregion

        #region Metrics and Maintenance

        /// <summary>
        /// Updates mean age and replication fraction of currently active agents.
        /// </summary>
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
            ReplicationFraction = (float)replicationCount / (float)activeAgents.Count;
        }

        #endregion

        #region Agent Lifecycle Management

        /// <summary>
        /// Handles agent expiration, including potential respawn after replication.
        /// </summary>
        public void OnExpired(IAgentLifecycle agent)
        {
            if (activeAgents.Contains(agent))
            {
                // Remove agent
                MonoBehaviour agentMono = agent as MonoBehaviour;
                GameObject agentObject = agentMono.gameObject;
                activeAgents.Remove(agent);
                coverManager.RemoveAgent(agent.GetAgentID());
                agentObject.SetActive(false);

                // Respawn if replicated
                if (agent.HasReplicated())
                {
                    agent.ResetState();
                    Vector3 spawnPos;
                    if (coverManager.TryRespawnAgent(agent.GetAgentID(), agentDefinition, out spawnPos))
                    {
                        spawnPos = new Vector3(spawnPos.x, agentObject.transform.position.y, spawnPos.z);
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

        /// <summary>
        /// Called when an agent replicates. (Currently empty.)
        /// </summary>
        public void OnReplicated(IAgentLifecycle agent)
        {
            // TODO: Implement future replication effects here (e.g., mutation, new agent properties).
        }

        #endregion
    }
}

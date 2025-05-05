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
        public GlobalSettings globalSettings;

        public int initialAgentCount = 100;
        public int poolSize = 200;
        public int SpawnBurst = 5; // How many to add when pool is empty
        public int suspendedCount = 0;

        private Transform agentParent;
        public List<GameObject> agentPool = new List<GameObject>();
        public List<IAgentLifecycle> activeAgents = new List<IAgentLifecycle>();
        private AgentDefinition agentDefinition;

        private string newAgentID;

        [SerializeField]
        private AgentInstantiator agentInstantiator;

        [SerializeField]
        private CoverManager coverManager;

        public int PotentialPopulationCount { get; private set; }
        public int ActiveCount => activeAgents.Count;
        public int PoolCount => agentPool.Count;
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
            globalSettings = FindAnyObjectByType<GlobalSettings>();
            coverManager.CreateCoverGrid();
            LoadAgentDefinition();
            CreateAgentPool();
            SpawnInitialAgents();
            Time.timeScale = globalSettings.timeScale;
            Time.fixedDeltaTime = 0.02f * globalSettings.timeScale;

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
                // Debug.LogError("Agent preset file not found at: " + path);
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
                agentPool.Remove(agentObject);
                IAgentLifecycle agentLifecycle = agentObject.GetComponent<IAgentLifecycle>();
                if (agentLifecycle is AgentController Agent)
                {
                    Agent.InitializeFromDefinition(agentDefinition);
                    Agent.SetAgentID(id);
                    Agent.SetManager(this);
                    Agent.RandomizeMaxAge(50);
                    Agent.RandomizeReplicationAge(50);
                }

                Vector3 spawnPos;
                if (coverManager.TrySpawnAgent(id, agentDefinition, out spawnPos))
                {
                    Quaternion spawnRot = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                    agentObject.transform.position = spawnPos;
                    Utils.PositionAgentAtGroundLevel(agentObject);
                    agentObject.transform.rotation = spawnRot;
                    agentObject.SetActive(true);
                    activeAgents.Add(agentLifecycle);
                }
                else
                {
                    // Debug.LogWarning($"AgentManager: Could not place agent {id}, no available positions!");
                }
            }
        }


        private void RespawnAgent(string id)
        {
            GameObject agentObject = GetPooledAgent();
            Bounds totalBound = Utils.CalculateTotalBounds(agentObject);
            if (agentObject != null)
            {
                agentPool.Remove(agentObject);
                IAgentLifecycle agentLifecycle = agentObject.GetComponent<IAgentLifecycle>();
                if (agentLifecycle is AgentController Agent)
                {
                    Agent.InitializeFromDefinition(agentDefinition);
                    Agent.SetAgentID(id);
                    Agent.SetManager(this);
                    Agent.RandomizeMaxAge(50);
                    Agent.RandomizeReplicationAge(50);
                }

                Vector3 spawnPos;
                if (coverManager.TrySpawnAgent(id, agentDefinition, out spawnPos))
                {
                    agentObject.transform.position = spawnPos;
                    Utils.PositionAgentAtGroundLevel(agentObject);
                    Quaternion spawnRot = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                    agentObject.transform.rotation = spawnRot;
                    agentObject.SetActive(true);
                    activeAgents.Add(agentLifecycle);

                }
                else
                {
                    Debug.LogWarning($"AgentManager: Could not place agent {id}, no available positions!");
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
            // Debug.LogWarning("No pooled agents available!");
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
                // Remove agent and place it back in the pool
                MonoBehaviour agentMono = agent as MonoBehaviour;
                GameObject agentObject = agentMono.gameObject;
                coverManager.RemoveAgent(agent.GetAgentID());
                activeAgents.Remove(agent);
                agentObject.SetActive(false);
                agentPool.Add(agentObject);
            }
            else
            {
                // Debug.LogWarning("Manager: Tried to expire an agent not in active list!");
            }
        }

        /// <summary>
        /// Called when an agent replicates. (Currently empty.)
        /// </summary>
        public void OnReplicated(IAgentLifecycle parentAgent)
        {

            // Get an agent from the pool
            if (agentPool.Count <= 0)
            {
                // Debug.Log("Pool empty. adding to pool...");
                for (int i = 0; i < 1; i++)
                {
                    Vector3 dummyPosition = Vector3.zero;
                    Quaternion dummyRotation = Quaternion.identity;

                    GameObject agentObject = agentInstantiator.InstantiateAgentFromDefinition(agentDefinition, dummyPosition, dummyRotation);
                    agentObject.SetActive(false);
                    agentPool.Add(agentObject);
                }
            }
            string newID = GenerateUniqueAgentID();
            RespawnAgent(newID);
        }

        #endregion

public bool HasActiveDynamicAgents()
{
    foreach (var agent in activeAgents)
    {
    if (agent.IsDynamic() && agent.IsActive())
        {
            return true;
        }
    }
    return false;
}
        public void Restart()
        {
            // Clear all active agents
            foreach (var agent in activeAgents)
            {
                var agentMono = agent as MonoBehaviour;
                if (agentMono != null)
                {
                    Destroy(agentMono.gameObject);
                }
            }
            activeAgents.Clear();

            // Clear all pooled agents
            foreach (var pooledGO in agentPool)
            {
                if (pooledGO != null)
                {
                    Destroy(pooledGO);
                }
            }
            agentPool.Clear();

            // Reset cover manager and definition if needed
            coverManager.CreateCoverGrid();  // Rebuild positions
            LoadAgentDefinition();  // Reload JSON in case it changed

            // Recreate pool and spawn agents
            CreateAgentPool();
            SpawnInitialAgents();

            Debug.Log($"{gameObject.name}: Restarted with {PotentialPopulationCount} agents.");
        }
    }
}

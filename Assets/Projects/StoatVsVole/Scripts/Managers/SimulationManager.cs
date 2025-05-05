using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents.Policies;

namespace StoatVsVole
{
    public class SimulationManager : MonoBehaviour
    {
        [Header("Dynamic Agent Managers")]
        public List<AgentManager> dynamicManagers;
        public GlobalSettings globalSettings;

        [Header("Reset Options")]
        public float checkInterval = 1.0f;
        private float timeSinceLastCheck = 0f;
        private int iterationStep = 0;
        void Start() {

            globalSettings = FindAnyObjectByType<GlobalSettings>();
            iterationStep = 0;

        }
        private void Update()
        {
            if (dynamicManagers == null || dynamicManagers.Count == 0)
                return;  // 🔒 Skip extinction logic until managers are registered

            iterationStep++;
            if (iterationStep >= globalSettings.nIterationsPerEpisode)
            {
                print("SimulationManager: Out of Iterations. Resetting simulation.");
                EndEpisodeForAll();
                iterationStep = 0;
                RestartAllManagers();
            }

            timeSinceLastCheck += Time.deltaTime;
            if (timeSinceLastCheck >= checkInterval)
            {
                timeSinceLastCheck = 0f;
                if (AllDynamicAgentsAreDead())
                {
                    print("SimulationManager: All dynamic agents are extinct. Resetting simulation.");
                    EndEpisodeForAll();
                    RestartAllManagers();
                }
                else
                {
                }
            }
        }
        private bool AllDynamicAgentsAreDead()
        {
            foreach (var manager in dynamicManagers)
            {
                // print($"SimulationManager: Checking manager {manager.name}...");
                if (manager.gameObject.activeInHierarchy && manager.HasActiveDynamicAgents())
                {
                    // print($"SimulationManager: Manager {manager.name} has active dynamic agents.");
                    return false;
                }
            }
            return true;
        }

        private void EndEpisodeForAll()
        {
            print("SimulationManager: Ending episodes for all agents.");
            foreach (var manager in dynamicManagers)
            {
                foreach (var agent in manager.activeAgents)
                {
                    var agentController = agent as AgentController;
                    if (agentController != null)
                    {
                        agentController.EndEpisode();
                    }
                }

                foreach (var pooledGO in manager.agentPool)
                {
                    var pooledAgent = pooledGO.GetComponent<AgentController>();
                    if (pooledAgent != null)
                    {
                        // print($"SimulationManager: Ending episode for pooled agent {pooledAgent.GetAgentID()}.");
                        pooledAgent.EndEpisode();
                    }
                }
            }
        }

        private void RestartAllManagers()
        {
            print("SimulationManager: Restarting all dynamic agent managers.");
            foreach (var manager in dynamicManagers)
            {
                manager.Restart();
            }
        }
    }
}
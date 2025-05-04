using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents.Policies;

namespace StoatVsVole
{
    public class SimulationManager : MonoBehaviour
    {
        [Header("Dynamic Agent Managers")]
        public List<AgentManager> dynamicManagers;

        [Header("Reset Options")]
        public float checkInterval = 1.0f;
        private float timeSinceLastCheck = 0f;

    private void Start()
    {
        print("SimulationManager: Scanning for active ML-Agents at startup...");

        var agents = FindObjectsByType<Unity.MLAgents.Agent>(FindObjectsSortMode.None);

        foreach (var agent in agents)
        {
            if (!agent.gameObject.activeInHierarchy)
                continue;

            var bp = agent.GetComponent<BehaviorParameters>();
            if (bp != null)
            {
                print($"[Startup] Found ACTIVE agent: {agent.name}, behavior = {bp.BehaviorName}");
            }
        }
    }

private void Update()
{
    if (dynamicManagers == null || dynamicManagers.Count == 0)
        return;  // 🔒 Skip extinction logic until managers are registered

    timeSinceLastCheck += Time.deltaTime;

    if (timeSinceLastCheck >= checkInterval)
    {
        timeSinceLastCheck = 0f;
        print("SimulationManager: Checking for extinction...");

        if (AllDynamicAgentsAreDead())
        {
            print("SimulationManager: All dynamic agents are extinct. Resetting simulation.");
            EndEpisodeForAll();
            RestartAllManagers();
        }
        else
        {
            print("SimulationManager: Dynamic agents still active.");
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
            print("SimulationManager: No active dynamic agents found.");
            return true;
        }

        private void EndEpisodeForAll()
        {
            print("SimulationManager: Ending episodes for all agents.");
            foreach (var manager in dynamicManagers)
            {
                foreach (var agent in manager.activeAgents)
                {
                    print($"SimulationManager: Ending episode for active agent {agent.GetAgentID()}.");
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
                print($"SimulationManager: Restarting manager {manager.name}.");
                manager.Restart();
            }
        }
    }
}
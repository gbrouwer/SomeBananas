using UnityEngine;
using TMPro;

namespace Halverson
{
    public class HalversonMetrics : MonoBehaviour
    {
        [Header("UI Elements")]
        public TextMeshProUGUI episodeText;
        public TextMeshProUGUI escapedAgentsText;
        public TextMeshProUGUI outOfBouadsAgentsText;
        public TextMeshProUGUI totalRewardText;
        public TextMeshProUGUI timeStepsText;

        [Header("Simulation Manager Reference")]
        public HalversonManager simulationManager; // Assign this in Inspector

        private int episodeCounter = 0;

        void Start()
        {
            if (simulationManager == null)
            {
                Debug.LogError("SimulationManager is not assigned to SimulationUIManager!");
            }
        }

        void Update()
        {
            if (simulationManager == null) return;

            // Increment episode count if a new episode starts
            if (simulationManager.GetCurrentTimeSteps() == 0)
            {
                episodeCounter++;
            }

            // Fetch data from HalversonManager
            int escapedAgents = simulationManager.GetEscapedAgentsCount();
            int outOfBouadsAgents = simulationManager.GetOutOfBoundsAgentsCount();
            float totalRewards = simulationManager.GetTotalAgentRewards();
            int remainingSteps = simulationManager.GetMaxEnvironmentSteps() - simulationManager.GetCurrentTimeSteps();

            // Update UI
            episodeText.text = $"Episode: {episodeCounter}";
            escapedAgentsText.text = $"Escaped Agents: {escapedAgents}/{simulationManager.Agents.Count}";
            outOfBouadsAgentsText.text = $"Escaped Agents: {outOfBouadsAgents}/{simulationManager.Agents.Count}";
            totalRewardText.text = $"Total Reward: {totalRewards:F2}";
            timeStepsText.text = $"Remaining Steps: {remainingSteps}";
        }
    }
}

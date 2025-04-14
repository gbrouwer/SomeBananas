using UnityEngine;
using TMPro;
using Unity.VisualScripting;

namespace TrafficJam
{
    public class TrafficJamMetrics : MonoBehaviour
    {
        [Header("UI Elements")]
        public TextMeshProUGUI currentSpeedText;
        public TextMeshProUGUI currentEpisodeText;
        public TextMeshProUGUI currentTickText;
        public TextMeshProUGUI currentRewardText;
        public TextMeshProUGUI currentPositionText;
        public TextMeshProUGUI cumulativeDistanceText;

        private TrafficJamManager simulationManager; // Assign this in Inspector
        void Start()
        {
            GameObject Env = GameObject.Find("TrafficJamManager");
            simulationManager = Env.GetComponent<TrafficJamManager>();
            if (simulationManager == null)
            {
                Debug.LogError("SimulationManager is not assigned to SimulationUIManager!");
            }
        }

        void Update()
        {
            if (simulationManager == null) return;

            // Increment episode count if a new episode starts
            int stepCounter = simulationManager.currentStepCount;
            currentTickText.text = $"Step: {stepCounter}";

            float cumulativeDistance = simulationManager.cumulativeDistance;
            cumulativeDistanceText.text = $"Cumulative Distance: {cumulativeDistance}";

            int episodeCounter = simulationManager.currentEpisode;
            currentEpisodeText.text = $"Episode: {episodeCounter}";

            Vector3 currentPos = simulationManager.currentPos;
            currentPositionText.text = $"Current position: {currentPos.x:F2} {currentPos.y:F2} {currentPos.z:F2}";


            float speed = simulationManager.currentSpeed;
            currentSpeedText.text = $"Current position: {speed:F2}";

            float reward = simulationManager.currentRewardLevel;
            currentRewardText.text = $"Cumulative Reward: {reward:F2}";
        //     // Fetch data from TrafficJamManager
        //     int escapedAgents = simulationManager.GetEscapedAgentsCount();
        //     int outOfBouadsAgents = simulationManager.GetOutOfBoundsAgentsCount();
        //     float totalRewards = simulationManager.GetTotalAgentRewards();
        //     int remainingSteps = simulationManager.GetMaxEnvironmentSteps() - simulationManager.GetCurrentTimeSteps();

        //     // Update UI
        //     episodeText.text = $"Episode: {episodeCounter}";
        //     escapedAgentsText.text = $"Escaped Agents: {escapedAgents}/{simulationManager.Agents.Count}";
        //     outOfBouadsAgentsText.text = $"Escaped Agents: {outOfBouadsAgents}/{simulationManager.Agents.Count}";
        //     totalRewardText.text = $"Total Reward: {totalRewards:F2}";
        //     timeStepsText.text = $"Remaining Steps: {remainingSteps}";
        }
    }
}

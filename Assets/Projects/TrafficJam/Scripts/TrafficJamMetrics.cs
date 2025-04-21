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
        public TextMeshProUGUI timeScaleText;
        public TextMeshProUGUI stepLimitText;
        public TextMeshProUGUI moveSpeedText;

        [Header("Settings")]
        public TrafficJamSettings settings;

        [Header("Manager")]
        private TrafficJamManager simulationManager; // Assign this in Inspector
        void Start()
        {
            GameObject Env = GameObject.Find("TrafficJamManager");
            GameObject settingsObj = GameObject.Find("TrafficJamSettings");
            simulationManager = Env.GetComponent<TrafficJamManager>();
            settings = settingsObj.GetComponent<TrafficJamSettings>();

                        if (simulationManager == null)
            {
                Debug.LogError("SimulationManager is not assigned to SimulationUIManager!");
            }

            
        }

        void Update()
        {

            if (settings != null)
            {
                timeScaleText.text = $"Time Scale: {Time.timeScale:F2}";
                stepLimitText.text = $"Max Steps: {settings.maxSteps}";
                moveSpeedText.text = $"Move Speed: {settings.moveSpeed:F1}";
            }

            if (simulationManager == null) return;

            // Increment episode count if a new episode starts
            int stepCounter = simulationManager.currentStepCount;
            currentTickText.text = $"Step: {stepCounter}";

            float cumulativeDistance = simulationManager.cumulativeDistance;
            cumulativeDistanceText.text = $"Cumulative Distance: {cumulativeDistance:F2}";

            float speed = simulationManager.currentSpeed;
            currentSpeedText.text = $"Current Speed: {speed:F2}";

            int episodeCounter = simulationManager.currentEpisode;
            currentEpisodeText.text = $"Episode: {episodeCounter}";

            Vector3 currentPos = simulationManager.currentPos;
            currentPositionText.text = $"Current position: {currentPos.x:F2} {currentPos.y:F2} {currentPos.z:F2}";

            float reward = simulationManager.currentRewardLevel;
            currentRewardText.text = $"Cumulative Reward: {reward:F2}";
        }
    }
}

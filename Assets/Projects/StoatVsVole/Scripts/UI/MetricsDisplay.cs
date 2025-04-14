using UnityEngine;
using TMPro;

namespace StoatVsVole
{
    public class ManagerMetricsDisplay : MonoBehaviour
    {
        [Header("References")]
        public AgentManager manager;

        [Header("UI Elements")]
        public TextMeshProUGUI aliveText;
        public TextMeshProUGUI suspendedText;
        public TextMeshProUGUI meanAgeText;
        public TextMeshProUGUI replicationCountText;

        private void Update()
        {
            if (manager == null)
                return;

            aliveText.text = "IsActive: " + manager.ActiveCount;
            replicationCountText.text = "Replication Rate: " + manager.ReplicationFraction.ToString("F2");
            meanAgeText.text = "Mean Age: " + manager.MeanAge.ToString("F2");

        }
    }
}

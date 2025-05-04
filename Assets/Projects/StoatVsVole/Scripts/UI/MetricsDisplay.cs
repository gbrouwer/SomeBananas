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
        public TextMeshProUGUI poolSizeText;
        public TextMeshProUGUI meanAgeText;
        public TextMeshProUGUI replicationCountText;
        public TextMeshProUGUI suspendedText;

        private void Update()
        {
            if (manager == null)
                return;

            aliveText.text = "IsActive: " + manager.ActiveCount;
            poolSizeText.text = "Pool Size: " + manager.PoolCount;

            replicationCountText.text = "Replication Rate: " + manager.ReplicationFraction.ToString("F2");
            meanAgeText.text = "Mean Age: " + manager.MeanAge.ToString("F2");

        }
    }
}

using UnityEngine;
using System.Collections.Generic;
using System.IO;

namespace StoatVsVole
{
    /// <summary>
    /// Stores global, simulation-wide parameters accessible by multiple systems.
    /// Primarily defines the world size, which determines terrain and wall boundaries.
    /// </summary>
    public class GlobalSettings : MonoBehaviour
    {
        [Header("Episode Settings")]
        public int nIterationsPerEpisode = 5000;

        [Header("World Settings")]
        [Tooltip("Length of one side of the square world (units). Affects ground, wall, and grid scaling.")]
        public float worldSize = 10f;

        [Header("Vole Settings")]
        public List<string> labelList = new List<string>();
        
        [Header("Timing Settings")]
        [Range(0.1f, 5f)]
        public float timeScale = 1f;

        private void Awake()
        {
            ApplyTimeScale();
        }

        private void OnValidate()
        {
            // Update TimeScale in the Editor when values are changed
            ApplyTimeScale();
        }

        private void ApplyTimeScale()
        {
            Time.timeScale = timeScale;
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
        }        

        void Start() {
            labelList.Clear();
            labelList.Add("vole");
        }

        public void ToggleLabels(string agentClass)
        {
            print("Toggling labels for: " + agentClass);
            if (labelList.Contains(agentClass)) {
                labelList.Remove(agentClass);
            }
            else {
                labelList.Add(agentClass);
            }
        }
    }
}

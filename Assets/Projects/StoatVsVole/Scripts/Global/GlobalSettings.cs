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
        [Header("World Settings")]
        [Tooltip("Length of one side of the square world (units). Affects ground, wall, and grid scaling.")]
        public float worldSize = 10f;

        [Header("Vole Settings")]
        public List<string> labelList = new List<string>();

        void Start() {
            labelList.Clear();
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

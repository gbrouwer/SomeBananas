using System;
using UnityEngine;

namespace TrafficJam
{
    
    public class TrafficJamSettings : MonoBehaviour
    {
        [Header("Simulation Settings")]
        public int maxSteps = 5000;

        [Header("Agent Movement Settings")]
        public float moveSpeed = 100f;
        public float turnSpeed = 50f;
        public float targetSpeed = 25f;
        public float lateralFrictionStrength = 3f;
        public float brakeForce = 10f;

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
    }
}

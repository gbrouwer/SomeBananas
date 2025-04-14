using System;
using UnityEngine;

namespace TrafficJam

{
    public class TrafficJamSettings : MonoBehaviour
    {

        [Header("Stimulation Settings")]
        public int maxSteps = 5000;

        [Header("Agent Movement Settings")]
        public float moveSpeed = 100f;
        public float turnSpeed = 50f;
        public float targetSpeed = 100f; 
        public float lateralFrictionStrength = 3f;
        public float brakeForce = 10f; 
    }
}

using System;
using UnityEngine;

namespace CubeEscape

{
    public class CubeEscapeSettings : MonoBehaviour
    {



        [Header("🔧 Environment Settings")]
        public float spawnAreaMarginMultiplier;
        public float agentSpacing = 0.5f;
        public int nAgents = 16;
        public int maxSteps = 5000;

        [Header("🔧 Agent Detectable Objects")]
        public bool detectGoal;
        public bool detectWall;
        public bool detectAgent;

        [Header("🔧 Agent Movement Settings")]
        public float agentRunSpeed = 2.0f;
        public float agentRotationSpeed = 10.0f;
        public float maxSpeed = 4.0f;

        [Header("🔧 Agent Sensor Settings")]
        public float RayPerceptionSensor_length = 12.0f;
        public float PerceptionSensor_angle = 90.0f;
        public float RayPerceptionSensor_radius = 0.5f;

        [Header("🔧 Agent Reward Settings")]
        public float EscapingBaseReward = 1.0f;
        public float AltruismBonus = 0.0f;
        public float SelfishnessPenalty = 0.0f;
        public float OtherGoalReward = 1.0f;

        [Header("🔧 Agent Penalty Settings")]
        public float CollisionPenalty = -0.2f;
        public float FailurePenalty = -3.0f;
        public float IterationPenalty = -0.1f;
        public float IterationMultiplier = 1.00f;

    }
}

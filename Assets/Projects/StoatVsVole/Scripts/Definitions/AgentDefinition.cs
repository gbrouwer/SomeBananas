using UnityEngine;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using System;
using System.Collections.Generic;

namespace StoatVsVole
{
    /// <summary>
    /// Serializable class defining all parameters required to configure an agent.
    /// Used to instantiate agents dynamically from config files.
    /// </summary>
    [Serializable]
    public class AgentDefinition
    {
        // General Agent Settings
        public string agentType;
        public string agentClass;
        public string agentPrefabName;
        public string bodyPrefabName;
        public string sensorPrefabName;

        // Lifecycle Settings
        public float maxAge;
        public float replicationAge;
        public List<string> expirationCauses;
        public bool canExpire => expirationCauses != null && expirationCauses.Count > 0;
        public bool canExpireFromAge => expirationCauses.Contains("age");
        public bool canExpireFromEnergy => expirationCauses.Contains("energy");

        // Energy Settings
        public float maxEnergy;
        public float energyExchangeRate;
        public float initialEnergy;

        // Tagging and Sensing
        public string agentTag;
        public List<string> detectableTags;
        public List<string> energySources;
        public List<string> energySinks;

        // Components and Behavior Settings
        public RigidbodySettings rigidbodySettings;
        public ColliderSettings colliderSettings;
        public BodySettings bodySettings;
        public BehaviorParameterSettings behaviorParameterSettings;
        public DecisionRequesterSettings decisionRequesterSettings;
        public RaySensorSettings raySensorSettings;
        public RewardSettings rewardSettings;
        public MotionSettings motionSettings;
        public ModelOverriderSettings modelOverriderSettings;
    }

    /// <summary>
    /// Settings for the agent's Rigidbody component.
    /// </summary>
    [Serializable]
    public class RigidbodySettings
    {
        public float mass;
        public float linearDamping;
        public float angularDamping;
        public RigidbodyConstraints constraints;
    }

    /// <summary>
    /// Settings for the agent's Collider component.
    /// </summary>
    [Serializable]
    public class ColliderSettings
    {
        public Vector3 center;
        public float radius;
        public float height;
        public Vector3 size;
        public bool isTrigger;
        public bool providesContacts;
    }

    /// <summary>
    /// Settings for the body visual (mesh) of the agent.
    /// </summary>
    [Serializable]
    public class BodySettings
    {
        public float scaleX = 0.5f;
        public float scaleY = 0.5f;
        public float scaleZ = 0.5f;
        public float offsetX = 0.0f;
        public float offsetY = 0.0f;
        public float offsetZ = 0.0f;
        public string materialName;
    }

    /// <summary>
    /// Settings for ML-Agents BehaviorParameters component.
    /// </summary>
    [Serializable]
    public class BehaviorParameterSettings
    {
        public bool enabled = false;
        public string behaviorName;
        public string behaviorType;
        public int teamID;
        public string inferenceDevice;
    }

    /// <summary>
    /// Settings for ML-Agents DecisionRequester component.
    /// </summary>
    [Serializable]
    public class DecisionRequesterSettings
    {
        public bool enabled = false;
        public int decisionPeriod = 5;
        public bool takeActionsBetweenDecisions = true;
    }

    /// <summary>
    /// Settings for RayPerceptionSensorComponent3D attached to the agent.
    /// </summary>
    [Serializable]
    public class RaySensorSettings
    {
        public bool enabled = false;
        public string sensorName = "RaySensor";
        public int raysPerDirection = 3;
        public float maxRayDegrees = 70f;
        public float rayLength = 20f;
        public string[] detectableTags;
        public float scaleX = 1.0f;
        public float scaleY = 1.0f;
        public float scaleZ = 1.0f;
        public float startVerticalOffset;
        public float endVerticalOffset;
    }

    /// <summary>
    /// Settings for the agent's basic movement capabilities.
    /// </summary>
    [Serializable]
    public class MotionSettings
    {
        public float agentRunSpeed;
        public float agentRotationSpeed;
        public float maxSpeed;
    }

    /// <summary>
    /// Settings for reinforcement learning reward shaping.
    /// </summary>
    [Serializable]
    public class RewardSettings
    {
        public float longevityRewardPerStep = 0.01f;
        public float expirationWithoutReplicationPenalty = -1.0f;
        public float replicationAward = 2.0f;
    }

    /// <summary>
    /// Settings for overriding trained models via the ModelOverrider utility.
    /// </summary>
    [Serializable]
    public class ModelOverriderSettings
    {
        public string debugCommandLineOverride;
        public bool hasOverrides;
        public string originalBehaviorName;
    }
}

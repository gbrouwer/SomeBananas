using UnityEngine;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using System;
using System.Collections.Generic;

namespace StoatVsVole
{
    [Serializable]
    public class AgentDefinition
    {
        public string agentType;
        public string agentClass;
        public string agentPrefabName;
        public string bodyPrefabName;
        public string sensorPrefabName;
        public float initialEnergy;
        public float maxEnergy;
        public float energyExchangeRate;
        public bool canExpire = true;
        public float maxAge;
        public float replicationAge;
        public string agentTag;
        public List<string> detectableTags;
        public List<string> energySources;
        public List<string> energySinks;

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

    [Serializable]
    public class RigidbodySettings
    {
        public float mass;
        public float linearDamping;
        public float angularDamping;
        public RigidbodyConstraints constraints;
    }
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

    [Serializable]
    public class BehaviorParameterSettings
    {
        public bool enabled = false;
        public string behaviorName;
        public string behaviorType;
        public int teamID;
        public string inferenceDevice;
    }

    [Serializable]
    public class DecisionRequesterSettings
    {
        public bool enabled = false;
        public int decisionPeriod = 5;
        public bool takeActionsBetweenDecisions = true;
    }

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

    [Serializable]
    public class MotionSettings
    {
        public float agentRunSpeed;
        public float agentRotationSpeed;
        public float maxSpeed;
    }

    [Serializable]
    public class RewardSettings
    {
        public float longevityRewardPerStep = 0.01f;
        public float expirationWithoutReplicationPenalty = -1.0f;
        public float replicationAward = 2.0f;
    }

    [Serializable]
    public class ModelOverriderSettings
    {
        public string debugCommandLineOverride;
        public bool hasOverrides;
        public string originalBehaviorName;
    }
}

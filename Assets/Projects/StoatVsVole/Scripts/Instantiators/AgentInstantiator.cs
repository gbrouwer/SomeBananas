using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using System.Collections.Generic;
using Unity.MLAgentsExamples;

namespace StoatVsVole
{
    /// <summary>
    /// Dynamically spawns agents in the simulation using settings defined in AgentDefinition files.
    /// Handles prefab instantiation, body setup, colliders, sensors, ML-Agents integration, and behavior overrides.
    /// </summary>
    public class AgentInstantiator  : MonoBehaviour
    {
        #region Public Methods

        /// <summary>
        /// Spawns an agent GameObject using the provided AgentDefinition configuration.
        /// </summary>
        /// <param name="config">The agent definition containing prefab, body, and ML-Agents parameters.</param>
        /// <param name="position">World position to spawn at.</param>
        /// <param name="rotation">World rotation to spawn with.</param>
        /// <returns>Instantiated agent GameObject, or null if spawning fails.</returns>
        public GameObject InstantiateAgentFromDefinition(AgentDefinition config, Vector3 position, Quaternion rotation)
        {
            if (!TagExists(config.agentTag))
            {
                Debug.LogWarning($"AgentSpawner: Cannot spawn agent because tag '{config.agentTag}' does not exist. Skipping instantiation.");
                return null;
            }

            GameObject agentPrefab = Resources.Load<GameObject>(config.agentPrefabName);
            if (agentPrefab == null)
            {
                Debug.LogError("Agent prefab not found: " + config.agentPrefabName);
                return null;
            }

            GameObject agentGO = Instantiate(agentPrefab, position, rotation);

            SetupBody(config, agentGO);
            SetupCollider(config, agentGO);
            SetupSensors(config, agentGO);
            SetupAgentController(config, agentGO);
            SetupRigidbody(config, agentGO);
            SetupBehaviorParameters(config, agentGO);
            SetupDecisionRequester(config, agentGO);
            SetupModelOverrider(config, agentGO);

            agentGO.transform.position = new Vector3(0, config.bodySettings.scaleY * 0.5f, 0); // TODO: Consider generalizing this ground offset.
            return agentGO;
        }

        #endregion

        #region Private Helpers

        /// <summary>
        /// Verifies if a given tag exists in the project.
        /// </summary>
        private bool TagExists(string tag)
        {
            try
            {
                GameObject temp = new GameObject();
                temp.tag = tag;
                Destroy(temp);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void SetupBody(AgentDefinition config, GameObject agentGO)
        {
            GameObject bodyPrefab = Resources.Load<GameObject>(config.bodyPrefabName);
            if (bodyPrefab != null)
            {
                GameObject bodyGO = Instantiate(bodyPrefab, agentGO.transform);
                bodyGO.transform.localPosition = new Vector3(
                    config.bodySettings.offsetX,
                    config.bodySettings.offsetY,
                    config.bodySettings.offsetZ
                );
                bodyGO.transform.localRotation = Quaternion.identity;
                bodyGO.transform.localScale = new Vector3(
                    config.bodySettings.scaleX,
                    config.bodySettings.scaleY,
                    config.bodySettings.scaleZ
                );

                if (config.bodySettings != null)
                {
                    var renderer = bodyGO.GetComponent<MeshRenderer>();
                    if (renderer != null)
                    {
                        Material mat = Resources.Load<Material>(config.bodySettings.materialName);
                        if (mat != null)
                        {
                            renderer.material = mat;
                        }
                    }
                }
            }
        }

        private void SetupCollider(AgentDefinition config, GameObject agentGO)
        {
            var collider = agentGO.GetComponent<Collider>();
            if (collider != null && config.colliderSettings != null)
            {
                if (collider is CapsuleCollider capsuleCollider)
                {
                    capsuleCollider.height = config.colliderSettings.height;
                    capsuleCollider.radius = config.colliderSettings.radius;
                    capsuleCollider.center = config.colliderSettings.center;
                }
                else if (collider is BoxCollider boxCollider)
                {
                    boxCollider.size = config.colliderSettings.size;
                    boxCollider.center = config.colliderSettings.center;
                }
                else if (collider is SphereCollider sphereCollider)
                {
                    sphereCollider.radius = config.colliderSettings.radius;
                    sphereCollider.center = config.colliderSettings.center;
                }

                collider.isTrigger = config.colliderSettings.isTrigger;
                collider.providesContacts = config.colliderSettings.providesContacts;
            }
        }

        private void SetupSensors(AgentDefinition config, GameObject agentGO)
        {
            if (config.raySensorSettings != null && config.raySensorSettings.enabled &&
                !string.IsNullOrEmpty(config.sensorPrefabName) && config.sensorPrefabName != "None")
            {
                GameObject sensorPrefab = Resources.Load<GameObject>(config.sensorPrefabName);
                if (sensorPrefab != null)
                {
                    GameObject sensorGO = Instantiate(sensorPrefab, agentGO.transform);
                    sensorGO.transform.localPosition = new Vector3(
                        config.bodySettings.offsetX,
                        config.bodySettings.offsetY,
                        config.bodySettings.offsetZ
                    );
                    sensorGO.transform.localRotation = Quaternion.identity;

                    var raySensor = sensorGO.GetComponent<RayPerceptionSensorComponent3D>();
                    if (raySensor != null)
                    {
                        raySensor.SensorName = config.raySensorSettings.sensorName;
                        raySensor.RaysPerDirection = config.raySensorSettings.raysPerDirection;
                        raySensor.MaxRayDegrees = config.raySensorSettings.maxRayDegrees;
                        raySensor.RayLength = config.raySensorSettings.rayLength;
                        raySensor.DetectableTags = new List<string>(config.raySensorSettings.detectableTags);
                        raySensor.StartVerticalOffset = config.raySensorSettings.startVerticalOffset;
                        raySensor.EndVerticalOffset = config.raySensorSettings.endVerticalOffset;
                    }
                }
            }
        }

        private void SetupAgentController(AgentDefinition config, GameObject agentGO)
        {
            AgentController agentController = agentGO.GetComponent<AgentController>();
            if (agentController != null)
            {
                agentController.energy = config.initialEnergy;
                agentController.maxAge = config.maxAge;
                agentController.replicationAge = config.replicationAge;
                agentController.agentTag = config.agentTag;
                agentController.detectableTags = config.detectableTags;
                agentController.energySinks = config.energySinks;
                agentController.energySources = config.energySources;
                agentController.agentType = config.agentType;

                if (config.rewardSettings != null)
                {
                    agentController.longevityRewardPerStep = config.rewardSettings.longevityRewardPerStep;
                    agentController.expirationWithoutReplicationPenalty = config.rewardSettings.expirationWithoutReplicationPenalty;
                    agentController.replicationAward = config.rewardSettings.replicationAward;
                }

                if (config.motionSettings != null)
                {
                    agentController.agentRunSpeed = config.motionSettings.agentRunSpeed;
                    agentController.agentRotationSpeed = config.motionSettings.agentRotationSpeed;
                    agentController.maxSpeed = config.motionSettings.maxSpeed;
                }
            }
        }

        private void SetupRigidbody(AgentDefinition config, GameObject agentGO)
        {
            Rigidbody rb = agentGO.GetComponent<Rigidbody>();
            if (rb != null && config.rigidbodySettings != null)
            {
                if (config.rigidbodySettings.mass <= 0f)
                {
                    Destroy(rb); // No Rigidbody needed for static agents
                }
                else
                {
                    rb.mass = config.rigidbodySettings.mass;
                    rb.linearDamping = config.rigidbodySettings.linearDamping;
                    rb.angularDamping = config.rigidbodySettings.angularDamping;
                    rb.constraints = config.rigidbodySettings.constraints;

                    rb.isKinematic = false;
                    rb.useGravity = true;
                }
            }
        }

        private void SetupBehaviorParameters(AgentDefinition config, GameObject agentGO)
        {
            BehaviorParameters behaviorParameters = agentGO.GetComponent<BehaviorParameters>();
            if (behaviorParameters != null && config.behaviorParameterSettings != null)
            {
                if (!config.behaviorParameterSettings.enabled)
                {
                    behaviorParameters.enabled = false;
                }
                else
                {
                    behaviorParameters.enabled = true;
                    behaviorParameters.BehaviorName = config.behaviorParameterSettings.behaviorName;
                    behaviorParameters.BehaviorType = (BehaviorType)System.Enum.Parse(typeof(BehaviorType), config.behaviorParameterSettings.behaviorType);
                    behaviorParameters.TeamId = config.behaviorParameterSettings.teamID;
                    behaviorParameters.InferenceDevice = (InferenceDevice)System.Enum.Parse(typeof(InferenceDevice), config.behaviorParameterSettings.inferenceDevice);
                }
            }
        }

        private void SetupDecisionRequester(AgentDefinition config, GameObject agentGO)
        {
            DecisionRequester decisionRequester = agentGO.GetComponent<DecisionRequester>();
            if (decisionRequester != null && config.decisionRequesterSettings != null)
            {
                if (!config.decisionRequesterSettings.enabled)
                {
                    decisionRequester.enabled = false;
                }
                else
                {
                    decisionRequester.enabled = true;
                    decisionRequester.DecisionPeriod = config.decisionRequesterSettings.decisionPeriod;
                    decisionRequester.TakeActionsBetweenDecisions = config.decisionRequesterSettings.takeActionsBetweenDecisions;
                }
            }
        }

        private void SetupModelOverrider(AgentDefinition config, GameObject agentGO)
        {
            ModelOverrider modelOverrider = agentGO.GetComponent<ModelOverrider>();
            if (modelOverrider != null && config.modelOverriderSettings != null && config.modelOverriderSettings.hasOverrides)
            {
                modelOverrider.enabled = true;
                modelOverrider.debugCommandLineOverride = config.modelOverriderSettings.debugCommandLineOverride;
            }
        }

        #endregion
    }
}

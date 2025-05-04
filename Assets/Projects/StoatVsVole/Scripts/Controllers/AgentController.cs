using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Collections.Generic;

namespace StoatVsVole
{
    /// <summary>
    /// Interface for all lifecycle-based agents (Static or Dynamic).
    /// </summary>
    public interface IAgentLifecycle
    {
        void SetAgentID(string id);
        void RandomizeMaxAge(float standardDeviation);
        void RandomizeReplicationAge(float standardDeviation);
        string GetAgentID();
        string GetAgentClass();
        float GetAge();
        bool IsActive();
        bool IsExpired();
        bool IsSuspended();
        bool HasReplicated();
        bool IsDynamic();
        void ResetState();
    }

    /// <summary>
    /// Controls the lifecycle, energy management, and behavior of a single agent.
    /// Integrates with Unity ML-Agents for training, and manages resource exchange logic.
    /// </summary>
    public class AgentController : Agent, IAgentLifecycle
    {

        [Header("Read Only Parameters for debugging")]
        [SerializeField] private float debugEnergy;
        [SerializeField] private float debugAge;
        [SerializeField] private float debugNaxAge;
        [SerializeField] private float debugMaxEnergy;
        [SerializeField] private bool debugCanExpire;
        [SerializeField] private bool debugCanExpireFromAge;
        [SerializeField] private bool debugCanExpireFromEnergy;
        [SerializeField] private bool debugIsActive;
        [SerializeField] private bool debugIsExpired;
        [SerializeField] private bool debugHasReplicated;
        [SerializeField] private bool debugIsSuspended;
        [SerializeField] private bool debugIsDynamic;

        #region Fields and Settings

        [Header("Lifecycle Settings")]
        public AgentDefinition agentDefinition;
        public float energy;
        public float maxAge;
        public float replicationAge;
        public float age;

        [Header("Expiration Settings")]
        private bool canExpireFromAge = false;
        private bool canExpireFromEnergy = false;

        [Header("Energy Settings")]
        private float maxEnergy;
        private float energyExchangeRate;
        public List<string> detectableTags;
        public List<string> energySinks;
        public List<string> energySources;
        private bool withEnergySource = false;
        private bool withEnergySink = false;
        private Queue<EnergyRequest> incomingRequests = new Queue<EnergyRequest>();
        private Queue<EnergyRequest> outgoingRequests = new Queue<EnergyRequest>();
        private EnergyRequest activeIncomingRequest = null;
        private EnergyRequest activeOutgoingRequest = null;
        private AgentController targetResourceAgent;
        private Gradient energyGradient;
        private MaterialPropertyBlock propertyBlock;

        [Header("Agent Settings")]
        public string agentType;
        public string agentClass;
        public string agentTag;
        public string agentID;

        private Rigidbody rb;
        private bool isActive;
        private bool isExpired;
        private bool hasReplicated;
        private bool isSuspended = false;
        private bool isDynamic = false;
        private Collider agentCollider;
        private RayPerceptionSensorComponent3D[] sensors;
        private AgentManager manager;
        private Renderer bodyRenderer;
        private Material agentMaterialInstance;

        [Header("Debug Settings")]
        private Color originalColor;
        private Color triggeredColor = Color.yellow;
        private FloatingLabel floatingLabel;
        private string lastLabelText = "Test"; // Add this at the top inside Debugging region

        [Header("Motion Settings")]
        public float agentRunSpeed;
        public float agentRotationSpeed;
        public float maxSpeed;

        [Header("Reward Settings")]
        public float longevityRewardPerStep;
        public float expirationWithoutReplicationPenalty;
        public float replicationAward;
        public float energyDrainRate;
        public float lowEnergyPenaltyFactor;


        #endregion

        #region Unity Lifecycle Methods

        /// <summary>
        /// Unity Awake. Initializes core components.
        /// </summary>
        protected override void OnEnable()
        {
            if (agentType != "dynamic")
            {
                print($"[OnEnable] Skipping ML-Agents registration for static agent: {name}");
                return;  // Prevents base.Agent.OnEnable() from being called
            }

            base.OnEnable();
        }

        protected override void OnDisable()
        {
            if (agentType != "dynamic")
            {
                print($"[OnDisable] Skipping ML cleanup for static agent: {name}");
                return;
            }

            base.OnDisable();
        }
        protected new void Awake()
        {
            rb = GetComponent<Rigidbody>();
            agentCollider = GetComponent<Collider>();
            sensors = GetComponentsInChildren<RayPerceptionSensorComponent3D>();
        }

        // public override void Initialize()
        // {
        //     if (agentType != "dynamic")
        //     {
        //         print("Turning flower inactive");
        //         // Prevent non-learning agents from initializing with ML-Agents
        //         enabled = false;
        //         return;
        //     }

        //     base.Initialize(); // only for voles, etc.
        // }

        /// <summary>
        /// Unity Start. Initializes debug label.
        /// </summary>
        void Start()
        {
            floatingLabel = GetComponentInChildren<FloatingLabel>();
            bodyRenderer = GetComponentInChildren<Renderer>();
            bodyRenderer.material = new Material(bodyRenderer.material);

        }

        /// <summary>
        /// Unity FixedUpdate. Main simulation step: handles aging, expiration, energy interactions.
        /// </summary>
        private void FixedUpdate()
        {
            if (!isActive)
                return;

            HandleAging();
            HandleEnergy();
            HandleLongevityReward();

            if (targetResourceAgent == null || targetResourceAgent.IsExpired())
            {
                withEnergySource = false;
                targetResourceAgent = null;
                if (agentMaterialInstance != null)
                    agentMaterialInstance.color = originalColor;
            }

            if (CheckExpirationConditions())
                Expire();

            if (IsExpired())
                return;

            HandleOutgoingRequests();
            HandleIncomingRequests();
        }

        /// <summary>
        /// Unity Update. (Currently unused.)
        /// </summary>
        private void Update()
        {
            UpdateLabel();
            UpdateVisualState();
            UpdateDebugValues();

        }

        #endregion

        #region Initialization and Reset

        /// <summary>
        /// Initializes agent from a definition (typically loaded from JSON).
        /// </summary>
        public void InitializeFromDefinition(AgentDefinition definition)
        {
            agentDefinition = definition;
            isDynamic = agentDefinition.agentType.ToLower() == "dynamic";
            ResetState();

            Renderer renderer = GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                agentMaterialInstance = renderer.material;
                originalColor = agentMaterialInstance.color;
            }

        }

        /// <summary>
        /// Resets internal state for respawn/reuse.
        /// </summary>
        public void ResetState()
        {
            if (agentDefinition != null)
            {
                // Energy Settings
                energy = agentDefinition.initialEnergy;
                maxEnergy = agentDefinition.maxEnergy;
                energyExchangeRate = agentDefinition.energyExchangeRate;
                energySinks = agentDefinition.energySinks;
                energySources = agentDefinition.energySources;

                // Lifecycle Settings
                maxAge = agentDefinition.maxAge;

                // lowEnergyPenaltyFactor = agentDefinition
                replicationAge = agentDefinition.replicationAge;
                canExpireFromAge = agentDefinition.canExpireFromAge;
                canExpireFromEnergy = agentDefinition.canExpireFromEnergy;

                // Motion Settings
                agentTag = agentDefinition.agentTag;
                agentClass = agentDefinition.agentClass;
                detectableTags = agentDefinition.detectableTags;
                agentRunSpeed = agentDefinition.motionSettings.agentRunSpeed;
                agentRotationSpeed = agentDefinition.motionSettings.agentRotationSpeed;
                maxSpeed = agentDefinition.motionSettings.maxSpeed;

                // Reward Settings
                longevityRewardPerStep = agentDefinition.rewardSettings.longevityRewardPerStep;
                expirationWithoutReplicationPenalty = agentDefinition.rewardSettings.expirationWithoutReplicationPenalty;
                replicationAward = agentDefinition.rewardSettings.replicationAward;
                energyDrainRate = agentDefinition.rewardSettings.energyDrainRate;
                lowEnergyPenaltyFactor = agentDefinition.rewardSettings.lowEnergyPenaltyFactor;
            }

            age = 0f;
            hasReplicated = false;
            isSuspended = false;
            isActive = true;

            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.WakeUp();
            }

            if (agentCollider != null)
            {
                agentCollider.enabled = true;
            }

            gameObject.tag = agentTag;
        }

        /// <summary>
        /// Sets manager reference for callbacks.
        /// </summary>
        public void SetManager(AgentManager managerReference)
        {
            manager = managerReference;
        }

        #endregion

        #region Energy Management

        private void HandleOutgoingRequests()
        {
            if (activeOutgoingRequest == null && withEnergySource)
            {
                if (targetResourceAgent != null && !targetResourceAgent.IsExpired() && energy < maxEnergy)
                {
                    activeOutgoingRequest = new EnergyRequest(this, targetResourceAgent, energyExchangeRate);
                    outgoingRequests.Enqueue(activeOutgoingRequest);
                }
            }

            if (activeOutgoingRequest != null)
            {
                float amountRequested = activeOutgoingRequest.amountRequested;
                float amountReceived = activeOutgoingRequest.provider.RequestEnergy(amountRequested);

                if (amountReceived > 0f)
                {
                    energy += amountReceived;
                    energy = Mathf.Min(energy, maxEnergy);

                    // ✅ Reward for successfully gaining energy
                    AddReward(amountReceived * 0.5f);

                    activeOutgoingRequest.provider.ConfirmEnergyTransfer(amountReceived);
                    activeOutgoingRequest.isCompleted = true;
                }
                else
                {
                    activeOutgoingRequest.isCancelled = true;
                }

                activeOutgoingRequest = null;
            }
        }

        private void HandleIncomingRequests()
        {
            if (activeIncomingRequest == null && incomingRequests.Count > 0)
            {
                activeIncomingRequest = incomingRequests.Dequeue();
            }

            if (activeIncomingRequest != null)
            {
                if (activeIncomingRequest.isCancelled)
                {
                    activeIncomingRequest = null;
                    return;
                }

                // Note: Actual deduction occurs when requester confirms energy transfer.
                activeIncomingRequest = null;
            }
        }

        /// <summary>
        /// Request available energy (non-destructive until confirmed).
        /// </summary>
        public float RequestEnergy(float amountRequested)
        {
            if (IsExpired())
                return 0f;

            return Mathf.Min(amountRequested, energy);
        }

        /// <summary>
        /// Confirm actual energy transfer (deducts energy).
        /// </summary>
        public void ConfirmEnergyTransfer(float amountTransferred)
        {
            if (IsExpired())
                return;

            energy -= amountTransferred;

            if (energy <= 0f & canExpireFromEnergy == true)
            {
                Expire();
            }
        }

        #endregion

        #region Collision Handling

        private void OnTriggerEnter(Collider other)
        {
            AgentController otherAgent = other.GetComponent<AgentController>();
            if (otherAgent == null)
                return;

            if (detectableTags.Contains(otherAgent.agentTag))
            {
                if (energySources.Contains(otherAgent.agentTag))
                {
                    withEnergySource = true;
                    targetResourceAgent = otherAgent;
                }

                if (energySinks.Contains(otherAgent.agentTag))
                {
                    withEnergySink = true;
                }

                if (agentMaterialInstance != null)
                {
                    agentMaterialInstance.color = triggeredColor;
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            AgentController otherAgent = other.GetComponent<AgentController>();
            if (otherAgent == null)
                return;

            if (detectableTags.Contains(otherAgent.agentTag))
            {
                if (energySources.Contains(otherAgent.agentTag))
                {
                    withEnergySource = false;
                    targetResourceAgent = null;
                }

                if (energySinks.Contains(otherAgent.agentTag))
                {
                    withEnergySink = false;
                }

                if (agentMaterialInstance != null)
                {
                    agentMaterialInstance.color = originalColor;
                }
            }
        }

        #endregion

        #region ML-Agents Integration

        public override void OnActionReceived(ActionBuffers actionBuffers)
        {
            if (!isDynamic)
                return;

            if (!isSuspended)
            {
                print(actionBuffers.DiscreteActions);
                MoveAgent(actionBuffers.DiscreteActions);
            }
        }

        public override void Heuristic(in ActionBuffers actionsOut)
        {
            if (!isDynamic)
                return;

            var discreteActionsOut = actionsOut.DiscreteActions;
            if (actionsOut.DiscreteActions.Length == 0) return;

            if (Input.GetKey(KeyCode.W))
                discreteActionsOut[0] = 1;
            else if (Input.GetKey(KeyCode.S))
                discreteActionsOut[0] = 2;
            else if (Input.GetKey(KeyCode.A))
                discreteActionsOut[0] = 3;
            else if (Input.GetKey(KeyCode.D))
                discreteActionsOut[0] = 4;
            else
                discreteActionsOut[0] = 0;
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            // if (!isDynamic)
            // {
            //     // Static agents (e.g., flowers) — no observations needed
            //     sensor.AddObservation(0f);
            //     return;
            // }

            // if (sensors == null || sensors.Length == 0)
            // {
            //     // Dynamic but no Ray Sensors found — fallback to dummy observation
            //     sensor.AddObservation(0f);
            //     Debug.LogWarning($"{gameObject.name}: Dynamic agent has no Ray Sensors attached! Added dummy observation.");
            // }
            // else
            // {
            //     // Dynamic agents with Ray Sensors — no manual observations needed
            //     // Ray sensors automatically send their observations
            // }
        }

        public override void OnEpisodeBegin()
        {
            // TODO: Handle environment resets, agent reset, etc.
        }

        private void MoveAgent(ActionSegment<int> act)
        {
            var dirToGo = Vector3.zero;
            var rotateDir = Vector3.zero;
            var action = act[0];
            switch (action)
            {
                case 1:
                    dirToGo = transform.forward;
                    break;
                case 2:
                    dirToGo = transform.forward * -1f;
                    break;
                case 3:
                    rotateDir = transform.up;
                    break;
                case 4:
                    rotateDir = transform.up * -1f;
                    break;
                case 5:
                    dirToGo = transform.right * -0.75f;
                    break;
                case 6:
                    dirToGo = transform.right * 0.75f;
                    break;
            }

            transform.Rotate(rotateDir, Time.fixedDeltaTime * agentRotationSpeed);
            rb.AddForce(dirToGo * agentRunSpeed, ForceMode.VelocityChange);

            if (rb.linearVelocity.magnitude > maxSpeed)
            {
                rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
            }
        }

        #endregion

        #region Lifecycle Management

        private void HandleAging()
        {
            age += Time.fixedDeltaTime * 30f;

            if (!hasReplicated && age >= replicationAge)
            {
                Replicate();
            }
        }

        private void HandleEnergy()
        {
            // Only dynamic agents (e.g., voles) lose energy passively
            if (isDynamic)
            {
                energy -= Time.fixedDeltaTime * energyDrainRate;
                energy = Mathf.Max(0f, energy);

                float energyRatio = energy / maxEnergy;

                if (energyRatio < 0.5f)
                {
                    float penalty = lowEnergyPenaltyFactor;// (0.5f - energyRatio) * lowEnergyPenaltyFactor;
                    AddReward(-penalty);
                }

                HandleLongevityReward();  // still dynamic-only
            }

            // Debug info
            debugEnergy = energy;
            debugMaxEnergy = maxEnergy;
        }

        protected virtual bool CheckExpirationConditions()
        {

            if (canExpireFromAge == true)
            {
                if (age >= maxAge)
                    return true;
            }

            if (canExpireFromEnergy == true)
            {
                if (energy <= 0f)
                    return true;
            }

            return false;
        }

        private void Expire()
        {
            isActive = false;
            isExpired = true;

            if (!hasReplicated)
            {
                AddReward(expirationWithoutReplicationPenalty);
            }

            manager.OnExpired(this);
        }

        public void Replicate()
        {
            hasReplicated = true;
            AddReward(replicationAward);  // ✅ Add this
            manager.OnReplicated(this);
        }

        protected virtual void HandleLongevityReward()
        {
            AddReward(longevityRewardPerStep * Time.fixedDeltaTime);
        }

        protected void Suspend()
        {
            isSuspended = true;

            if (rb != null)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.Sleep();
            }

            if (agentCollider != null)
            {
                agentCollider.enabled = false;
            }

            foreach (var sensor in sensors)
            {
                sensor.enabled = false;
            }

            gameObject.tag = "Untagged";
        }

        #endregion

        #region IAgentLifecycle API

        public void SetAgentID(string id) => agentID = id;
        public string GetAgentID() => agentID;
        public string GetAgentClass() => agentClass;
        public float GetAge() => age;
        public bool IsActive() => isActive && !isSuspended;
        public bool IsExpired() => isExpired;
        public bool IsSuspended() => isSuspended;
        public bool IsDynamic() => isDynamic;
        public bool HasReplicated() => hasReplicated;

        public void RandomizeMaxAge(float standardDeviation)
        {
            float randomized = Utils.RandomNormal(maxAge, standardDeviation);
            maxAge = Mathf.Max(1f, randomized);
        }

        public void RandomizeReplicationAge(float standardDeviation)
        {
            float randomized = Utils.RandomNormal(replicationAge, standardDeviation);
            replicationAge = Mathf.Max(1f, randomized);
        }

        #endregion

        #region Debugging

        private void UpdateVisualState()
        {
            if (bodyRenderer == null)
                return;

            float ratio = Mathf.Clamp01(energy / maxEnergy);
            Color energyColor;

            if (CompareTag("vole"))
            {
                if (ratio < 0.5f)
                {
                    energyColor = Color.Lerp(Color.red, Color.green, ratio * 2f);
                }
                else
                {
                    energyColor = Color.Lerp(Color.green, Color.yellow, (ratio - 0.5f) * 2f);
                }
            }
            else if (CompareTag("flower"))
            {
                energyColor = Color.Lerp(Color.red, Color.white, ratio);
            }
            else
            {
                energyColor = Color.Lerp(Color.gray, Color.white, ratio);
            }

            if (propertyBlock == null)
                propertyBlock = new MaterialPropertyBlock();

            bodyRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor("_BaseColor", energyColor);  // ✅ Works with URP Lit
            bodyRenderer.SetPropertyBlock(propertyBlock);
        }


        private void UpdateLabel()
        {
            if (floatingLabel != null && manager.globalSettings.labelList.Contains(agentTag))
            {
                string labelText = $"{agentTag}\nEnergy: {energy:F1}";

                if (withEnergySource)
                    labelText += "\nExtracting";

                if (withEnergySink)
                    labelText += "\nBeing Drained";

                // ➕ Add reward display
                labelText += $"\nReward: {GetCumulativeReward():F2}";

                floatingLabel.SetLabel(
                    labelText,
                    Color.white,
                    4.0f,
                    new Vector3(0, agentDefinition.bodySettings.scaleY + 0.25f, 0)
                );

                lastLabelText = labelText;
            }
            else
            {
                floatingLabel.SetLabel("", Color.white, 4.0f, new Vector3(0, agentDefinition.bodySettings.scaleY + 0.25f, 0));
            }

        }
        #endregion



        private void UpdateDebugValues()
        {
            debugEnergy = energy;
            debugAge = age;
            debugHasReplicated = hasReplicated;
        }


        private void SetupEnergyGradientBasedOnTag()
        {
            energyGradient = new Gradient(); // ← This must happen per-agent

            GradientColorKey[] colorKeys;
            GradientAlphaKey[] alphaKeys;

            if (CompareTag("vole"))
            {
                colorKeys = new GradientColorKey[]
                {
                new GradientColorKey(Color.blue, 0f),
                new GradientColorKey(Color.magenta, 1f)
                };
            }
            else if (CompareTag("flower"))
            {
                colorKeys = new GradientColorKey[]
                {
                    new GradientColorKey(Color.blue, 0f),
                    new GradientColorKey(Color.yellow, 1f)
                };
            }
            else
            {
                colorKeys = new GradientColorKey[]
                {
                    new GradientColorKey(Color.gray, 0f),
                    new GradientColorKey(Color.white, 1f)
                };
            }

            alphaKeys = new GradientAlphaKey[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 1f)
            };

            energyGradient.SetKeys(colorKeys, alphaKeys);
        }

        public void PrepareMaterialAndGradient()
        {
            bodyRenderer = GetComponentInChildren<Renderer>();

            if (bodyRenderer != null)
            {
                if (propertyBlock == null)
                    propertyBlock = new MaterialPropertyBlock();

                SetupEnergyGradientBasedOnTag();
                UpdateVisualState();  // This now uses the property block
            }
        }

    }
}

using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Collections.Generic;

namespace StoatVsVole
{

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

    public class AgentController : Agent, IAgentLifecycle
    {
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
        public float maxEnergy;
        private float energyExchangeRate;
        public List<string> detectableTags;
        public List<string> energySinks;
        public List<string> energySources;
        public bool withEnergySource = false;
        public bool withEnergySink = false;
        private Queue<EnergyRequest> incomingRequests = new Queue<EnergyRequest>();
        private Queue<EnergyRequest> outgoingRequests = new Queue<EnergyRequest>();
        private EnergyRequest activeIncomingRequest = null;
        private EnergyRequest activeOutgoingRequest = null;
        private AgentController targetResourceAgent;

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
        public AgentManager manager;
        public Renderer bodyRenderer;
        private Material agentMaterialInstance;
        public bool detectEnergyTransfer;

        [Header("Rendering Settings")]
        public Gradient energyGradient;
        public MaterialPropertyBlock propertyBlock;
        private Color originalColor;
        private Color triggeredColor = Color.yellow;
        public FloatingLabel floatingLabel;
        public string lastLabelText;

        [Header("Motion Settings")]
        public float agentRunSpeed;
        public float agentRotationSpeed;
        public float maxSpeed;

        [Header("Reward Settings")]
        public float longevityReward;
        public float expirationWithoutReplicationPenalty;
        public float energyExpirationPenalty;
        public float energyExtractionAward;
        public float replicationAward;
        public float energyDrainRate;
        public float lowEnergyPenalty;

        public void Reward(string reason = "",float value = 0.0f)
        {
            switch (reason)
            {
                case "Longevity":
                    AddReward(longevityReward * Time.fixedDeltaTime);
                    break;

                case "LowEnergyPenalty":
                    AddReward(-lowEnergyPenalty);
                    break;

                case "Replication":
                    AddReward(replicationAward);
                    break;

                case "ExpirationWithoutReplication":
                    AddReward(expirationWithoutReplicationPenalty);
                    break;

                case "EnergyExtraction":
                    AddReward(value*energyExtractionAward); // could scale up with efficiency later
                    break;

                default:
                    print("Unknown Reward");
                    break;
            }
        }

        protected override void OnEnable()
        {
            if (agentType != "dynamic")
            {
                return;
            }
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            if (agentType != "dynamic")
            {
                return;
            }

            base.OnDisable();
        }

        public void SafeEndEpisode()
        {
            if (agentType != "dynamic") return;
            if (!isActive) return; 
            EndEpisode();
        }

        protected new void Awake()
        {
            rb = GetComponent<Rigidbody>();
            agentCollider = GetComponent<Collider>();
            sensors = GetComponentsInChildren<RayPerceptionSensorComponent3D>();
        }

        void Start()
        {
            floatingLabel = GetComponentInChildren<FloatingLabel>();
            bodyRenderer = GetComponentInChildren<Renderer>();
            bodyRenderer.material = new Material(bodyRenderer.material);

        }

        private void FixedUpdate()
        {
            if (!isActive)
                return;

            HandleAging();
            HandleEnergy();
            Reward("Longevity",0.0f);

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

        private void Update()
        {
            AgentRenderer.UpdateLabel(this);
            AgentRenderer.UpdateVisualState(this);
        }

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
                replicationAge = agentDefinition.replicationAge;
                canExpireFromAge = agentDefinition.canExpireFromAge;
                canExpireFromEnergy = agentDefinition.canExpireFromEnergy;
                energyDrainRate = agentDefinition.energyDrainRate;

                // Motion Settings
                agentTag = agentDefinition.agentTag;
                agentClass = agentDefinition.agentClass;
                detectableTags = agentDefinition.detectableTags;
                agentRunSpeed = agentDefinition.motionSettings.agentRunSpeed;
                agentRotationSpeed = agentDefinition.motionSettings.agentRotationSpeed;
                maxSpeed = agentDefinition.motionSettings.maxSpeed;

                // Reward Settings
                longevityReward = agentDefinition.rewardSettings.longevityReward;
                expirationWithoutReplicationPenalty = agentDefinition.rewardSettings.expirationWithoutReplicationPenalty;
                energyExpirationPenalty = agentDefinition.rewardSettings.energyExpirationPenalty;
                replicationAward = agentDefinition.rewardSettings.replicationAward;
                lowEnergyPenalty = agentDefinition.rewardSettings.lowEnergyPenalty;
                energyExtractionAward = agentDefinition.rewardSettings.energyExtractionAward;
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

        public void SetManager(AgentManager managerReference)
        {
            manager = managerReference;
        }

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
                    Reward("EnergyExtraction",amountReceived);
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

        public float RequestEnergy(float amountRequested)
        {
            if (IsExpired())
                return 0f;

            return Mathf.Min(amountRequested, energy);
        }

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

        public override void OnActionReceived(ActionBuffers actionBuffers)
        {
            if (!isDynamic)
                return;

            if (!isSuspended)
            {
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
            if (sensor == null)
                return;

            if (isDynamic == false)
                return;

            if (detectEnergyTransfer == true) {
                print("Detection ON");
            }

        }

        public override void OnEpisodeBegin()
        {
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
                    Reward("LowEnergyPenalty",0.0f);
                }
            }
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
                Reward("ExpirationWithoutReplication",0.0f);
            }
            SafeEndEpisode();
            manager.OnExpired(this);
        }

        public void Replicate()
        {
            hasReplicated = true;
            Reward("Replication",0.0f);  // ✅ Add this
            manager.OnReplicated(this);
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

    }
}

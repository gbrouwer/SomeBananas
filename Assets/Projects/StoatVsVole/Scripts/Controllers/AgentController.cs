
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Collections.Generic;

namespace StoatVsVole
{
    // Interface for all lifecycle-based agents (Static or Dynamic)
    public interface IAgentLifecycle
    {
        void SetAgentID(string id);
        void RandomizeMaxAge(float standardDeviation);
        void RandomizeReplicationAge(float standardDeviation);
        string GetAgentID();
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
        public bool canExpire;
        public float age;
        
        private float maxEnergy;
        private float energyExchangeRate;


        [Header("Energy Transfer Settings")]
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
        private Manager manager;

        [Header("Debug Settings")]
        private Material agentMaterialInstance;
        private Color originalColor;
        private Color triggeredColor = Color.yellow;
        private FloatingLabel floatingLabel;

        [Header("Motion Settings")]
        public float agentRunSpeed;
        public float agentRotationSpeed;
        public float maxSpeed;

        [Header("Reward Settings")]
        public float longevityRewardPerStep;
        public float expirationWithoutReplicationPenalty;
        public float replicationAward;

        protected new void Awake()
        {
            rb = GetComponent<Rigidbody>();
            agentCollider = GetComponent<Collider>();
            sensors = GetComponentsInChildren<RayPerceptionSensorComponent3D>();
        }

        void Start()
        {
            floatingLabel = GetComponentInChildren<FloatingLabel>();
        }

        public void InitializeFromDefinition(AgentDefinition definition)
        {
            agentDefinition = definition;

            isDynamic = agentDefinition.agentType.ToLower() == "dynamic";

            ResetState();

            // Materials
            Renderer renderer = GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                agentMaterialInstance = renderer.material; // This creates an instance
                originalColor = agentMaterialInstance.color;
            }
        }

        public void ResetState()
        {
            if (agentDefinition != null)
            {
                energy = agentDefinition.initialEnergy;
                maxAge = agentDefinition.maxAge;
                replicationAge = agentDefinition.replicationAge;
                maxEnergy = agentDefinition.maxEnergy;
                canExpire = agentDefinition.canExpire;
                energyExchangeRate = agentDefinition.energyExchangeRate;
                agentTag = agentDefinition.agentTag;
                detectableTags = agentDefinition.detectableTags;
                energySinks = agentDefinition.energySinks;
                energySources = agentDefinition.energySources;
                agentRunSpeed = agentDefinition.motionSettings.agentRunSpeed;
                agentRotationSpeed = agentDefinition.motionSettings.agentRotationSpeed;
                maxSpeed = agentDefinition.motionSettings.maxSpeed;
                longevityRewardPerStep = agentDefinition.rewardSettings.longevityRewardPerStep;
                expirationWithoutReplicationPenalty = agentDefinition.rewardSettings.expirationWithoutReplicationPenalty;
                replicationAward = agentDefinition.rewardSettings.replicationAward;
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

        private void FixedUpdate()
        {
            if (!isActive)
                return;

            HandleAging();

            if (CheckExpirationConditions())
            {
                Expire();
            }

            if (IsExpired())
                return;

            if (targetResourceAgent == null || targetResourceAgent.IsExpired())
            {
                withEnergySource = false;
                targetResourceAgent = null;
                if (agentMaterialInstance != null)
                {
                    agentMaterialInstance.color = originalColor;
                }
            }

            HandleOutgoingRequests();
            HandleIncomingRequests();
            UpdateLabel();
        }

        private void Update()
        {
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

                // In this basic version, we don't need to actively process incoming requests.
                // Energy is only deducted when a confirmed RequestEnergy() is called by the requester.

                activeIncomingRequest = null;
            }
        }


        public void SetManager(Manager managerReference)
        {
            manager = managerReference;
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

        public void MoveAgent(ActionSegment<int> act)
        {
            var dirToGo = Vector3.zero;
            var rotateDir = Vector3.zero;
            var action = act[0];

            switch (action)
            {
                case 1:
                    dirToGo = transform.forward * 1f;
                    break;
                case 2:
                    dirToGo = transform.forward * -1f;
                    break;
                case 3:
                    rotateDir = transform.up * 1f;
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

        public override void OnEpisodeBegin()
        {
        }

        public override void CollectObservations(VectorSensor sensor)
        {

            if (!isDynamic)
            {

                // Static agents provide 1 dummy observation to satisfy ML-Agents
                sensor.AddObservation(0f);
                return;
            }

            // Dynamic agents - real observations (if needed)

        }

        public override void Heuristic(in ActionBuffers actionsOut)
        {
            if (!isDynamic)
                return;

            var discreteActionsOut = actionsOut.DiscreteActions;
            if (actionsOut.DiscreteActions.Length == 0) return;

            if (Input.GetKey(KeyCode.W))
                discreteActionsOut[0] = 1; // Move Forward
            else if (Input.GetKey(KeyCode.S))
                discreteActionsOut[0] = 2; // Move Backward
            else if (Input.GetKey(KeyCode.A))
                discreteActionsOut[0] = 3; // Rotate Left
            else if (Input.GetKey(KeyCode.D))
                discreteActionsOut[0] = 4; // Rotate Right
            else
                discreteActionsOut[0] = 0; // Do nothing

        }

        public float RequestEnergy(float amountRequested)
        {
            if (IsExpired())
                return 0f;

            // No deduction yet; just offer what is available
            return Mathf.Min(amountRequested, energy);
        }

        public void ConfirmEnergyTransfer(float amountTransferred)
        {
            if (IsExpired())
                return;

            energy -= amountTransferred;

            if (energy <= 0f)
            {
                Expire();
            }
        }

        private void HandleAging()
        {
            age += Time.fixedDeltaTime * 30f;

            if (!hasReplicated && age >= replicationAge)
            {
                hasReplicated = true;
            }
        }

        protected virtual void HandleLongevityReward()
        {
            AddReward(longevityRewardPerStep * Time.fixedDeltaTime);
        }

        protected virtual bool CheckExpirationConditions()
        {

            if (!canExpire)
                return false;

            return age >= maxAge || energy <= 0f;
        }

        private void Expire()
        {
            isActive = false;
            isExpired = true;
            if (!hasReplicated)
            {
                AddReward(expirationWithoutReplicationPenalty);
            }
            // Suspend();
            manager.OnExpired(this);
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

            // flowerManager?.OnFlowerSuspended(this.gameObject);

        }

        public void SetAgentID(string id)
        {
            agentID = id;
        }

        public string GetAgentID()
        {
            return agentID;

        }

        public void RandomizeMaxAge(float standardDeviation)
        {
            float randomized = Utils.RandomNormal(maxAge*0.5f, standardDeviation);
            maxAge = Mathf.Max(1f, randomized);
            print(maxAge);
        }

        public void RandomizeReplicationAge(float standardDeviation)
        {
            float randomized = Utils.RandomNormal(replicationAge, standardDeviation);
            replicationAge = Mathf.Max(1f, randomized);
            print(replicationAge);
        }


        public void Replicate()
        {
            if (!hasReplicated)
            {
                hasReplicated = true;
                manager.OnReplicated(this);
            }
        }

        public float GetAge()
        {
            return age;
        }

        public bool IsExpired()
        {
            return isExpired;
        }

        public bool IsSuspended()
        {
            return isSuspended;
        }

        public bool IsActive()
        {
            return isActive && !isSuspended;
        }

        public bool IsDynamic()
        {
            return isActive && !isSuspended;
        }

        public bool HasReplicated()
        {
            return hasReplicated;
        }

        private void UpdateLabel()
        {
            if (floatingLabel != null)
            {
                string labelText = $"{agentTag}\nEnergy: {energy:F1}";
                if (withEnergySource)
                {
                    labelText += "\nExtracting";
                }
                if (withEnergySink)
                {
                    labelText += "\nBeing Drained";
                }

                floatingLabel.SetLabelText(labelText);
            }
        }
    }
}

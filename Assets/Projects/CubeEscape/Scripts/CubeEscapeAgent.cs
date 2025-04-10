//Put this script on your blue cube.

using System.Collections;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.Transforms;
using Unity.VisualScripting;
using Unity.MLAgents.Sensors;
namespace CubeEscape
{
    public class CubeEscapeAgent : Agent
    {
        private GameObject ground;
        private GameObject area;
        public CubeEscapeManager ceManager;
        public CubeEscapeSettings ceSettings;
        private Bounds areaBounds;
       
        public bool hasEscaped = false;
        public bool isOutOfBounds = false;
        public bool isActive = false;

        [HideInInspector]
        private Collider agentCollider;
        private RayPerceptionSensorComponent3D[] sensors;
        public bool useVectorObs;
        Rigidbody m_AgentRb;  //cached on initialization
        EnvironmentParameters m_ResetParams;

        protected override void Awake()
        {

        }

        public override void Initialize()
        {
            this.isActive = true;
            this.hasEscaped = false;
            this.isOutOfBounds = false;
            m_AgentRb = GetComponent<Rigidbody>();
            m_ResetParams = Academy.Instance.EnvironmentParameters;

        }

        void FixedUpdate()
        {
            //LimitAgentSpeed();
        }

        void LimitAgentSpeed()
        {
            float maxSpeed = 5f;  // Set your preferred speed limit
            if (m_AgentRb.linearVelocity.magnitude > maxSpeed)
            {
                m_AgentRb.linearVelocity = m_AgentRb.linearVelocity.normalized * maxSpeed;
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
            transform.Rotate(rotateDir, Time.fixedDeltaTime * 200f);
            m_AgentRb.AddForce(dirToGo * ceSettings.agentRunSpeed,ForceMode.VelocityChange);
        }

        public override void OnActionReceived(ActionBuffers actionBuffers)
        {
            if (!isActive || actionBuffers.DiscreteActions.Length == 0) return; // ✅ Check action buffer size
            // Move the agent using the action.
            MoveAgent(actionBuffers.DiscreteActions);
        }

        public override void Heuristic(in ActionBuffers actionsOut)
        {
            if (actionsOut.DiscreteActions.Length == 0) return;
            var discreteActionsOut = actionsOut.DiscreteActions;
            if (Input.GetKey(KeyCode.D))
            {
                discreteActionsOut[0] = 3;
            }
            else if (Input.GetKey(KeyCode.W))
            {
                discreteActionsOut[0] = 1;
            }
            else if (Input.GetKey(KeyCode.A))
            {
                discreteActionsOut[0] = 4;
            }
            else if (Input.GetKey(KeyCode.S))
            {
                discreteActionsOut[0] = 2;
            }
        }
        void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("agent"))
            {
                ceManager.GetComponent<CubeEscapeManager>().ReportCollisionWithOtherAgent(this,other);
            }
            if (other.gameObject.CompareTag("goal"))
            {
                ceManager.GetComponent<CubeEscapeManager>().ReportGoalReached(this);
            }
            if (other.gameObject.CompareTag("bounds"))
            {
                ceManager.GetComponent<CubeEscapeManager>().ReportOutOfBounds(this);
            }
            if (other.gameObject.CompareTag("wall"))
            {
                ceManager.GetComponent<CubeEscapeManager>().ReportCollisionWithOtherAgent(this,other);
            }
        }

        public void AddTagsToRaySensors()
        {
            // Get all RayPerceptionSensor3D components attached to the GameObject
            RayPerceptionSensorComponent3D[] raySensors = GetComponents<RayPerceptionSensorComponent3D>();

            // Loop through each sensor and update detectable tags based on the options
            foreach (var sensor in raySensors)
            {
                // Make sure the detectable tags list is initialized
                if (sensor.DetectableTags == null)
                {
                    sensor.DetectableTags = new System.Collections.Generic.List<string>();
                }
                // Add tags based on the public booleans
                if (ceSettings.detectGoal && !sensor.DetectableTags.Contains("goal"))
                {
                    sensor.DetectableTags.Add("goal");
                }
                if (ceSettings.detectWall && !sensor.DetectableTags.Contains("wall"))
                {
                    sensor.DetectableTags.Add("wall");
                }
                if (ceSettings.detectAgent && !sensor.DetectableTags.Contains("agent"))
                {
                    sensor.DetectableTags.Add("agent");
                }
            }
        }

        public void BecomeSpectator()
        {
            isActive = false; // Mark agent as inactive

            // Disable Physics Movement
            m_AgentRb.isKinematic = true;  // Stop reacting to forces
            m_AgentRb.useGravity = false;  // Prevent falling (optional)
            m_AgentRb.linearVelocity = Vector3.zero; // Stop all motion
            m_AgentRb.angularVelocity = Vector3.zero; // Stop rotation
            m_AgentRb.Sleep(); // Force the Rigidbody to enter sleep mode

            // Remove from Perception
            gameObject.tag = "Untagged"; // Makes it invisible to Ray Sensors
            foreach (var sensor in sensors)
            {
                sensor.enabled = false; // Disable observations
            }

            // Disable Collision Interactions
            agentCollider.enabled = false; // Prevents bumping into other agents

            // Stop contributing to training
            RequestDecision();
            RequestAction();
        }

        public void ResetAgent(Vector3 startPosition)
        {
            isActive = true; // Reactivate the agent
            hasEscaped = false; // Reset escape status
            isOutOfBounds = false; // Reset out-of-bounds flag
            gameObject.tag = "Agent"; // Restore detectability
            transform.position = startPosition; // Move to new position

            // Reactivate Physics
            m_AgentRb.isKinematic = false;
            m_AgentRb.linearVelocity = Vector3.zero;
            m_AgentRb.angularVelocity = Vector3.zero;
            m_AgentRb.WakeUp();
            agentCollider.enabled = true;

            // Reactivate Sensors
            foreach (var sensor in sensors)
            {
                sensor.enabled = true;
            }

            // Ensure agent starts making decisions again
            RequestDecision();
        }
    }
}


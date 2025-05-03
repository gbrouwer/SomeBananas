using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using UnityEngine.InputSystem; // 👈 Add at top
using System.Collections.Generic;

namespace TrafficJam
{

    [RequireComponent(typeof(Rigidbody))]
    public class TrafficJamAgent : Agent
    {
        [Header("Movement Settings")]
        public TrafficJamManager manager;
        public TrafficJamSettings settings;
        private Vector3 previousPosition;
        float cumulativeDistance = 0f;
        private float currentSpeed;
        private float moveSpeed;
        private float turnSpeed;
        private float targetSpeed;
        private TrafficJamActionAsset trafficJamActionAsset;
        private InputAction moveAction;
        private InputAction steerAction;
        private Rigidbody rb;
        private float lateralFrictionStrength;
        private float brakeForce;
        EnvironmentParameters m_ResetParams;
        List<int> visitedWaypoints = new List<int>();
        protected override void Awake()
        {
            trafficJamActionAsset = new TrafficJamActionAsset(); // <- Important! create a new instance
            moveAction = trafficJamActionAsset.Car.Move;
            steerAction = trafficJamActionAsset.Car.Steer;
            moveAction.Enable();
            steerAction.Enable();
            manager = GameObject.Find("TrafficJamManager").GetComponent<TrafficJamManager>();
        }


        public void ResetAgent()
        {
            Vector3 environmentOffset = manager.environmentOffset;
            visitedWaypoints = new List<int>();
            moveSpeed = settings.moveSpeed;
            turnSpeed = settings.turnSpeed;
            targetSpeed = settings.targetSpeed;
            lateralFrictionStrength = settings.lateralFrictionStrength;
            brakeForce = settings.brakeForce;
            transform.position = new Vector3(0, 1, 0) + environmentOffset;
            transform.rotation = Quaternion.identity;
            cumulativeDistance = 0;

        }

        public override void Initialize()
        {
            Vector3 environmentOffset = manager.environmentOffset;
            rb = GetComponent<Rigidbody>();
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            m_ResetParams = Academy.Instance.EnvironmentParameters;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            transform.position = new Vector3(0, 1, 0) + environmentOffset;
            transform.rotation = Quaternion.identity;
            previousPosition = transform.position;
            cumulativeDistance = 0;
        }

        void FixedUpdate()
        {

            currentSpeed = rb.linearVelocity.magnitude;

            float stepDistance = Vector3.Distance(previousPosition, transform.position);
            cumulativeDistance += stepDistance;
            previousPosition = transform.position;

            float speedReward = CalculateSpeedReward(currentSpeed, targetSpeed, 10, 0.01f);
            AddReward(speedReward * 20.0f);
            AddReward(-0.02f); // small living penalty
        }

        private void ApplyLateralFriction()
        {
            // Calculate the sideways (lateral) velocity
            Vector3 right = transform.right;
            Vector3 lateralVelocity = Vector3.Dot(rb.linearVelocity, right) * right;

            // Apply a force opposite to the sideways velocity
            Vector3 lateralFrictionForce = -lateralVelocity * lateralFrictionStrength;

            rb.AddForce(lateralFrictionForce, ForceMode.Acceleration);
        }


        public override void OnEpisodeBegin()
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            float throttleInput = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
            float steeringInput = Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f);

            // Determine if braking or accelerating
            if (throttleInput > 0f)
            {
                // Accelerate forward
                Vector3 move = transform.forward * throttleInput * moveSpeed;
                rb.AddForce(move, ForceMode.Acceleration);
            }
            else if (throttleInput < 0f)
            {
                // Apply braking force (opposite to current velocity direction)
                Vector3 brakingForce = -rb.linearVelocity.normalized * brakeForce;
                rb.AddForce(brakingForce, ForceMode.Acceleration);
                AddReward(brakingForce.magnitude * -0.001f);

            }

            // Apply turning
            float turn = steeringInput * turnSpeed * Time.fixedDeltaTime;
            Quaternion turnRotation = Quaternion.Euler(0f, turn, 0f);
            rb.MoveRotation(rb.rotation * turnRotation);

            // Apply lateral friction to prevent sliding
            ApplyLateralFriction();

            // (Optional) Debug speed
            currentSpeed = rb.linearVelocity.magnitude; // assuming you already have Rigidbody 'rb'

        }

        private float CalculateSpeedReward(float currentSpeed, float targetSpeed, float sigma, float peakReward)
        {
            float diff = currentSpeed - targetSpeed;
            float exponent = -(diff * diff) / (2f * sigma * sigma);
            return peakReward * Mathf.Exp(exponent);
        }

        public override void Heuristic(in ActionBuffers actionsOut)
        {
            var continuousActions = actionsOut.ContinuousActions;
            if (continuousActions.Length >= 2)
            {
                float moveInput = moveAction != null ? moveAction.ReadValue<float>() : 0f;
                float steerInput = steerAction != null ? steerAction.ReadValue<float>() : 0f;
                continuousActions[0] = Mathf.Clamp(moveInput, -1f, 1f);
                continuousActions[1] = Mathf.Clamp(steerInput, -1f, 1f);
            }
        }

        private void OnTriggerEnter(Collider waypoint)
        {
            int waypoint_id = waypoint.GetInstanceID();
            if (visitedWaypoints.Contains(waypoint_id)) 
            {
                visitedWaypoints.Add(waypoint_id);
                SetReward(-10.0f);
            } 
            else
            {
                visitedWaypoints.Add(waypoint_id);
                AddReward(10.0f);
            }

        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("ground"))
            {
                SetReward(-5.0f);
                manager.EndEpisode();
            }
        }

        public float GetCurrentSpeed()
        {
            return currentSpeed;
        }
        public Quaternion GetCurrentRotation()
        {
            return transform.rotation;
        }
        public Vector3 GetCurrentPosition()
        {
            return transform.position;
        }
        public float GetCurrentDistance()
        {
            return cumulativeDistance;
        }
    }
}

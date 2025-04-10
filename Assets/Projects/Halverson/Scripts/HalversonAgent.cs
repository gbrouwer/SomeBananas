//Put this script on your blue cube.

using System.Collections;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
namespace Halverson
{
    public class HalversonAgent : Agent
    {
        public HalversonManager ceManager;
        public HalversonSettings ceSettings;
       
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
            m_ResetParams = Academy.Instance.EnvironmentParameters;

        }

        void FixedUpdate()
        {
            //LimitAgentSpeed();
        }

        public void MoveAgent(ActionSegment<int> act)
        {
            var dirToGo = Vector3.zero;
            var rotateDir = Vector3.zero;
            var action = act[0];

            // switch (action)
            // {
            //     case 1:
            //         dirToGo = transform.forward * 1f;
            //         break;
            //     case 2:
            //         dirToGo = transform.forward * -1f;
            //         break;
            //     case 3:
            //         rotateDir = transform.up * 1f;
            //         break;
            //     case 4:
            //         rotateDir = transform.up * -1f;
            //         break;
            //     case 5:
            //         dirToGo = transform.right * -0.75f;
            //         break;
            //     case 6:
            //         dirToGo = transform.right * 0.75f;
            //         break;
            // }
            // transform.Rotate(rotateDir, Time.fixedDeltaTime * 200f);
            // m_AgentRb.AddForce(dirToGo * ceSettings.agentRunSpeed,ForceMode.VelocityChange);
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            float delta = actions.ContinuousActions[0]; // sigma-like
            float beta = actions.ContinuousActions[1];
            float rho = actions.ContinuousActions[2];

            Vector3 pos = transform.position;

            float dx = delta * (pos.y - pos.x);
            float dy = pos.x * (rho - pos.z) - pos.y;
            float dz = pos.x * pos.y - beta * pos.z;

            Vector3 newPos = pos + new Vector3(dx, dy, dz);
            transform.position = newPos;
        }

        public override void Heuristic(in ActionBuffers actionsOut)
        {

        }
    }
}


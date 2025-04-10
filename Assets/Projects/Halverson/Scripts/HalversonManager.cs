using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.MLAgents;
using Unity.Transforms;
using System.Collections;
using Unity.MLAgents.Actuators;
using Halverson;
using UnityEditor;
using Unity.VisualScripting;
using Unity.MLAgents.Sensors;

namespace Halverson
{
    public class HalversonManager : MonoBehaviour
    {
        EnvironmentParameters m_ResetParams;
        HalversonGround Ground;
        HalversonWallSegments WallSegments;
        HalversonSettings Settings;

        public GameObject Agent;
        [HideInInspector]
        public List<GameObject> Agents = new List<GameObject>();
        int m_ResetTimer;
        int MaxEnvironmentSteps;
        int nAgents;
        int randomIndex = 10;
        int placedAgents = 0;
        private int escapedAgents = 0;
        private int outOfBoundsAgents = 0;
        public GameObject HalversonSideLine;
        List<Vector3> cubeStartPositions;
        List<Vector3> sideLinePositions;
        List<HalversonAgent> escapedAgentsList;

        public int GetOutOfBoundsAgentsCount() => outOfBoundsAgents;
        public int GetEscapedAgentsCount() => escapedAgents;
        public int GetCurrentTimeSteps() => m_ResetTimer;
        public int GetMaxEnvironmentSteps() => MaxEnvironmentSteps;
        public Vector3 GetExitPosition() => Ground.transform.position;
        public float GetTotalAgentRewards()
        {
            float totalReward = 0f;
            foreach (var Agent in Agents)
            {
                HalversonAgent agent = Agent.GetComponent<HalversonAgent>();
                totalReward += agent.GetCumulativeReward();
            }
            return totalReward;
        }

        void Start()
        {

            m_ResetTimer = 0;
            outOfBoundsAgents = 0;
            escapedAgents = 0;
            placedAgents = 0;

            Settings = GetComponentInChildren<HalversonSettings>();
            Ground = GetComponentInChildren<HalversonGround>();
            WallSegments = GetComponentInChildren<HalversonWallSegments>();
            nAgents = Settings.nAgents;
            MaxEnvironmentSteps = Settings.maxSteps;
            m_ResetParams = Academy.Instance.EnvironmentParameters;

            CalculateSideLinePositions();
            ResetWallSgements();
            AssignRandomExit();
            CreateStartingPositions();
            InstantiateAgents();
            print("Done Setting up");
        }


        void CreateStartingPositions()
        {

            int gridSize = 8;
            Vector2 center = new Vector2(0f, 0f); // Center (x, z) of the grid
            float objectSize = 2f; // The width and depth of each cube
            float totalExtent = gridSize * objectSize; // The total extent covered by the grid
            float startX = center.x - (totalExtent / 2) + (objectSize / 2);
            float startZ = center.y - (totalExtent / 2) + (objectSize / 2);
            cubeStartPositions = new List<Vector3>();
            for (int i = 0; i < gridSize; i++)
            {
                for (int j = 0; j < gridSize; j++)
                {
                    float posX = startX + i * objectSize;
                    float posZ = startZ + j * objectSize;
                    float posY = 0;
                    cubeStartPositions.Add(new Vector3(posX, posY + 1f, posZ));
                }
            }
        }

        void InstantiateAgents()
        {
            placedAgents = 0;
            outOfBoundsAgents = 0;
            escapedAgents = 0;
            escapedAgentsList = new List<HalversonAgent>();

            List<Vector3> randomSubset = cubeStartPositions.TakeRandomSubset(nAgents);
            foreach (Vector3 position in randomSubset)
            {
                Quaternion randomRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
                GameObject newAgent = Instantiate(Agent, position, randomRotation);
                HalversonAgent agent = newAgent.GetComponent<HalversonAgent>();
                agent.ceManager = this;
                agent.ceSettings = Settings;
                // agent.AddTagsToRaySensors();
                agent.Initialize();
                Agents.Add(newAgent);
            }
        }

        void ResetAgents()
        {
            // placedAgents = 0;
            // outOfBoundsAgents = 0;
            // escapedAgents = 0;
            // escapedAgentsList = new List<HalversonAgent>();
            // List<Vector3> randomSubset = cubeStartPositions.TakeRandomSubset(nAgents);
            // for (int i = 0; i < nAgents; i++)
            // {
            //     Vector3 position = randomSubset[i];
            //     Agent = Agents[i];
            //     HalversonAgent agent = Agent.GetComponent<HalversonAgent>();
            //     // Vector3 newStartPosition = position;
            //     // agent.ResetAgent(newStartPosition);
            //    }
        }

        void ResetWallSgements()
        {
            foreach (Transform child in WallSegments.transform)
            {
                HalversonStateChanger stateChanger = child.GetComponent<HalversonStateChanger>();
                if (stateChanger != null)
                {
                    stateChanger.Reset();
                }
            }
        }

        void AssignRandomExit()
        {
            randomIndex = Random.Range(0, WallSegments.transform.childCount);
            Transform child = WallSegments.transform.GetChild(randomIndex);
            child.transform.GetComponent<HalversonStateChanger>().SetToExit();
        }

        public void ResetEnvironment()
        {

            m_ResetTimer = 0;
            ResetWallSgements();
            AssignRandomExit();
            ResetAgents();

        }

        void FixedUpdate()
        {
        }

        void EndEpisode()
        {
            foreach (var Agent in Agents)
            {
                HalversonAgent agent = Agent.GetComponent<HalversonAgent>();
                agent.GetComponent<Rigidbody>().Sleep();
                agent.EndEpisode();
            }
            m_ResetTimer = 0;
            ResetWallSgements();
            AssignRandomExit();
            ResetAgents();
        }

        private void MoveAgentToNextGridPosition(HalversonAgent agent)
        {
            agent.transform.localRotation = Quaternion.identity;
            if (placedAgents >= sideLinePositions.Count)
            {
                Debug.LogWarning("All grid positions are occupied!");
                return;
            }

            agent.transform.position = sideLinePositions[placedAgents];
            placedAgents++;
        }

        private void CalculateSideLinePositions()
        {
            sideLinePositions = new List<Vector3>();
            Renderer areaRenderer = HalversonSideLine.GetComponent<Renderer>();
            Bounds bounds = areaRenderer.bounds;
            float width = bounds.size.x;
            float depth = bounds.size.z;

            int gridSize = Mathf.CeilToInt(Mathf.Sqrt(nAgents));
            float cellWidth = width / gridSize;
            float cellDepth = depth / gridSize;
            for (int row = 0; row < gridSize; row++)
            {
                for (int col = 0; col < gridSize; col++)
                {
                    if (sideLinePositions.Count >= nAgents) break;
                    float posX = bounds.min.x + (col * cellWidth) + (cellWidth / 2);
                    float posZ = bounds.min.z + (row * cellDepth) + (cellDepth / 2);
                    sideLinePositions.Add(new Vector3(posX, bounds.center.y, posZ));
                }
            }
        }
        public void GiveOutFinalRewards(List<HalversonAgent> escapedAgents)
        {
            int totalAgents = Agents.Count;
            int numEscaped = escapedAgentsList.Count;
            float baseReward = Settings.EscapingBaseReward;
            float altruismBonus = Settings.AltruismBonus;
            float selfishnessPenalty = Settings.SelfishnessPenalty;
            for (int i = 0; i < numEscaped; i++)
            {
                HalversonAgent escapedagent = escapedAgentsList[i];
                int escapedAfter = numEscaped - (i + 1); // How many agents escaped AFTER this one
                float escapeOrder = (float)(i + 1) / totalAgents; // Normalize escape order
                float reward = baseReward
                             + altruismBonus * ((float)escapedAfter / (totalAgents - 1))
                             - selfishnessPenalty * escapeOrder;
                escapedagent.AddReward(reward);
            }
        }

    }
}



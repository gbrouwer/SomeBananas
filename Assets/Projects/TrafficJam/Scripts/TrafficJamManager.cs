using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Unity.MLAgents;
using Unity.Transforms;
using System.Collections;
using Unity.MLAgents.Actuators;
using CubeEscape;
using UnityEditor;
using Unity.VisualScripting;
using Unity.MLAgents.Sensors;

namespace TrafficJam
{

    public class TrafficJamManager : MonoBehaviour
    {

        EnvironmentParameters m_ResetParams;
        // int m_ResetTimer;

        [Header("Tracks")]
        public GameObject tracks;
        private List<GameObject> trackList = new List<GameObject>();
        public int activeTrack = -1;
        private int nTracks;
        public int currentEpisode = 0;
        public float cumulativeDistance;
        public float currentSpeed;
        public Vector3 currentPos;
        public Quaternion currentRot;
        public float currentRewardLevel;

        [Header("Agent")]
        public GameObject agentPrefab;
        public int nAgents;

        [Header("Settings")]
        private TrafficJamSettings settings;
        private List<GameObject> Agents = new List<GameObject>();
        public Camera mainCamera;

        [Header("Episode Settings")]
        int MaxEnvironmentSteps;
        public int currentStepCount;

        void Start()
        {
            settings = GameObject.Find("TrafficJamSettings").GetComponent<TrafficJamSettings>();
            MaxEnvironmentSteps = settings.maxSteps;
            m_ResetParams = Academy.Instance.EnvironmentParameters;
            GetListOfTracks();
            PickTrack();
            SpawnAgents();
        }

        public void SpawnAgents()
        {

            Agents = new List<GameObject>();
            for (int i = 0; i < nAgents; i++)
            {
                Vector3 SpawnPos = new Vector3(0, 0, 0);
                Quaternion SpawnRot = Quaternion.identity;
                GameObject newAgent = Instantiate(agentPrefab, SpawnPos, SpawnRot);
                TrafficJamAgent agent = newAgent.GetComponent<TrafficJamAgent>();
                agent.manager = this;
                agent.settings = settings;
                agent.ResetAgent();
                Agents.Add(newAgent);
            }
        }

        public void RestartEpisode()
        {

            PickTrack();
            currentStepCount = 0;
            currentEpisode++; ;
            for (int i = 0; i < nAgents; i++)
            {
                TrafficJamAgent agent = Agents[i].GetComponent<TrafficJamAgent>();
                agent.manager = this;
                agent.settings = settings;
                agent.ResetAgent();
                Agents[i].SetActive(true);
                mainCamera.GetComponent<FollowCamera>().target = agent.transform;
            }
        }

        void PickTrack()
        {
            if (activeTrack > -1)
            {
                trackList[activeTrack].SetActive(false);
            }

            //Pick Track
            int randomIndex = Random.Range(0, nTracks);
            trackList[randomIndex].SetActive(true);
            activeTrack = randomIndex;

        }

        void GetListOfTracks()
        {
            foreach (Transform child in tracks.transform)
            {
                trackList.Add(child.gameObject);
            }
            nTracks = trackList.Count;
        }

        private void FixedUpdate()
        {
            currentStepCount++;
            if (currentStepCount >= MaxEnvironmentSteps)
            {
                currentEpisode++;
                TrafficJamAgent agent = Agents[0].GetComponent<TrafficJamAgent>();
                EndEpisode();
            }
            if (Agents.Count > 0)
            {
                TrafficJamAgent agent = Agents[0].GetComponent<TrafficJamAgent>();
                currentSpeed = agent.GetCurrentSpeed();
                currentPos = agent.GetCurrentPosition();
                currentRot = agent.GetCurrentRotation();
                currentRewardLevel = agent.GetCumulativeReward();
                cumulativeDistance = agent.GetCurrentDistance();
            }
        }

        public void EndEpisode()
        {
            TrafficJamAgent agent = Agents[0].GetComponent<TrafficJamAgent>();
            agent.EndEpisode();
            Agents[0].SetActive(false);
            StartCoroutine(ResetSimulationAfterDelay(2.0f));
        }

        private IEnumerator ResetSimulationAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            RestartEpisode();
        }
    }
}

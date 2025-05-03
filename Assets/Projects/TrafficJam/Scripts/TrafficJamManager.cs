using UnityEngine;
using System.Collections.Generic;
using Unity.MLAgents;
using System.Collections;

namespace TrafficJam
{
    public class TrafficJamManager : MonoBehaviour
    {
        EnvironmentParameters m_ResetParams;

        [Header("Track Textures (from Resources/Tracks)")]
        public bool testing = false;
        public int activeTrack = -1;
        public int currentEpisode = 0;

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

        [Header("Environment Instance Offset")]
        public Vector3 environmentOffset = Vector3.zero;

        [Header("Step Randomization")]
        public int stepOffsetRange = 1000;
        private int localStepOffset = 0;

        [Header("Runtime Stats")]
        public float cumulativeDistance;
        public float currentSpeed;
        public Vector3 currentPos;
        public Quaternion currentRot;
        public float currentRewardLevel;

        void Start()
        {
            settings = GameObject.Find("TrafficJamSettings").GetComponent<TrafficJamSettings>();
            MaxEnvironmentSteps = settings.maxSteps;
            localStepOffset = Random.Range(0, stepOffsetRange);
            m_ResetParams = Academy.Instance.EnvironmentParameters;
            agentPrefab = Resources.Load<GameObject>("TrafficJam/Agents/TrafficJamAgent");
            print("Done Loading Preab: " + agentPrefab.name);
            PickTrack();
            SpawnAgents();
            Time.timeScale = settings.timeScale;
            Time.fixedDeltaTime = 0.02f * settings.timeScale;
        }



        public void SpawnAgents()
        {
            Agents = new List<GameObject>();
            for (int i = 0; i < nAgents; i++)
            {
                Vector3 SpawnPos = transform.position;
                Quaternion SpawnRot = Quaternion.identity;
                SpawnPos = new Vector3(0, 0.0f, 0) + environmentOffset;
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
            currentEpisode++;

            for (int i = 0; i < nAgents; i++)
            {
                TrafficJamAgent agent = Agents[i].GetComponent<TrafficJamAgent>();
                agent.manager = this;
                agent.settings = settings;
                agent.ResetAgent();
                Agents[i].SetActive(true);
                if (mainCamera != null) {
                   mainCamera.GetComponent<FollowCamera>().target = agent.transform;
                }
            }
        }

        void PickTrack()
        {
            Texture2D[] trackTextures = Resources.LoadAll<Texture2D>("TrafficJam/Tracks");

            if (trackTextures.Length == 0)
            {
                Debug.LogError("No track textures found in Resources/Tracks/");
                return;
            }

            int randomIndex = Random.Range(0, trackTextures.Length);
            Texture2D selectedTexture = trackTextures[randomIndex];

            TrafficJamTrackMaker trackMaker = gameObject.GetSiblingComponent<TrafficJamTrackMaker>();
            if (trackMaker == null)
            {
                Debug.LogError("No ImageToRoadPlacer found in the scene!");
                return;
            }

            foreach (Transform child in trackMaker.transform)
            {
                Destroy(child.gameObject);
            }

            trackMaker.inputImage = selectedTexture;
            trackMaker.Generate(environmentOffset);

            activeTrack = randomIndex;
        }

        private void FixedUpdate()
        {
            currentStepCount++;
            if (currentStepCount >= MaxEnvironmentSteps + localStepOffset)
            {
                currentEpisode++;
                TrafficJamAgent agent = Agents[0].GetComponent<TrafficJamAgent>();
                agent.EndEpisode();
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
            StartCoroutine(ResetSimulationAfterDelay(0.0f));
        }

        private IEnumerator ResetSimulationAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            RestartEpisode();
        }
    }
}

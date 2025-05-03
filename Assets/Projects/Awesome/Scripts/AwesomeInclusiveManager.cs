// AwesomeInclusiveManager.cs
using System.Collections.Generic;
using UnityEngine;

public class AwesomeInclusiveManager : MonoBehaviour
{
    public GameObject agentPrefab;
    public int numAgents = 100;
    public float spawnHeight = 0.5f;
    public float stdDev = 10f;
    public float leftMeanX = -50f;
    public float rightMeanX = 50f;
    public float meanZ = 0f;
    public int splitHalf = 50; // Number of agents in each group

    private List<Vector3> spawnedPositions = new List<Vector3>();
    private float agentRadius = 0.5f; // Assuming agent diameter ~1 unit

    private void Start()
    {
        for (int i = 0; i < numAgents; i++)
        {
            Vector3 spawnPos = GenerateSpawnPosition(i < splitHalf);
            GameObject agent = Instantiate(agentPrefab, spawnPos, Quaternion.identity);

            // Color based on spawn position
            float normalizedX = Mathf.InverseLerp(-100f, 100f, spawnPos.x);
            float normalizedZ = Mathf.InverseLerp(-100f, 100f, spawnPos.z);
            Color agentColor = new Color(normalizedX, 0.5f, normalizedZ);

            agent.GetComponent<Renderer>().material.color = agentColor;

            // // Pass color to agent script
            // AwesomeInclusiveAgent agentScript = agent.GetComponent<AwesomeInclusiveAgent>();
            // agentScript.agentColor = agentColor;
        }
    }

    private Vector3 GenerateSpawnPosition(bool isLeftGroup)
    {
        Vector3 pos;
        int tries = 0;
        bool positionValid;

        do
        {
            positionValid = true;
            float meanX = isLeftGroup ? leftMeanX : rightMeanX;
            float x = RandomGaussian(meanX, stdDev);
            float z = RandomGaussian(meanZ, stdDev);
            pos = new Vector3(x, spawnHeight, z);

            foreach (var existingPos in spawnedPositions)
            {
                if (Vector3.Distance(pos, existingPos) < agentRadius * 2f)
                {
                    positionValid = false;
                    break;
                }
            }

            tries++;

        } while (!positionValid && tries < 10);

        spawnedPositions.Add(pos);
        return pos;
    }

    // Box-Muller Transform for normal distribution
    private float RandomGaussian(float mean, float stdDev)
    {
        float u1 = 1.0f - Random.value;
        float u2 = 1.0f - Random.value;
        float randStdNormal = Mathf.Sqrt(-2.0f * Mathf.Log(u1)) * Mathf.Sin(2.0f * Mathf.PI * u2);
        return mean + stdDev * randStdNormal;
    }
}

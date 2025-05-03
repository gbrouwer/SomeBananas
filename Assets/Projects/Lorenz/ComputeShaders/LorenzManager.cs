using UnityEngine;

public class LorenzManager : MonoBehaviour
{
    [Header("Simulation Settings")]
    public int numSpheres = 1000;
    public GameObject spherePrefab;
    public ComputeShader lorenzShader;

    [Header("Initial Conditions")]
    public Vector3 baseStartPosition = new Vector3(10f, 10f, 10f);
    public float noiseAmount = 0.01f;

    [Header("Simulation Parameters")]
    public float dt = 0.01f;
    public float spawnInterval = 0.01f;

    [Header("Display Settings")]
    public float positionScale = 1.0f;

    private GameObject[] spheres;
    private Vector3[] positions;
    private ComputeBuffer positionBuffer;

    private int releasedCount = 0;
    private float spawnTimer = 0f;

    void Start()
    {
        spheres = new GameObject[numSpheres];
        positions = new Vector3[numSpheres];

        // Initially place all spheres at the attractor basin (0,0,0)
        for (int i = 0; i < numSpheres; i++)
        {
            positions[i] = Vector3.zero;
            spheres[i] = Instantiate(spherePrefab, Vector3.zero, Quaternion.identity, this.transform);
        }

        positionBuffer = new ComputeBuffer(numSpheres, sizeof(float) * 3);
        positionBuffer.SetData(positions);
    }

    void Update()
    {
        if (releasedCount < numSpheres)
        {
            spawnTimer += Time.deltaTime;
            while (spawnTimer >= spawnInterval && releasedCount < numSpheres)
            {
                spawnTimer -= spawnInterval;

                // Apply noise to release a new sphere
                Vector3 noise = new Vector3(
                    Random.Range(-noiseAmount, noiseAmount),
                    Random.Range(-noiseAmount, noiseAmount),
                    Random.Range(-noiseAmount, noiseAmount)
                );
                positions[releasedCount] = baseStartPosition + noise;

                // Update buffer only at released index
                positionBuffer.SetData(positions, releasedCount, releasedCount, 1);

                releasedCount++;
            }
        }
    }

    void FixedUpdate()
    {
        if (releasedCount == 0 || positionBuffer == null)
            return;

        int kernel = lorenzShader.FindKernel("CSMain");

        lorenzShader.SetFloat("dt", dt);
        lorenzShader.SetBuffer(kernel, "positions", positionBuffer);

        int threadGroups = Mathf.CeilToInt(releasedCount / 64f);
        lorenzShader.Dispatch(kernel, threadGroups, 1, 1);

        positionBuffer.GetData(positions, 0, 0, releasedCount);

        for (int i = 0; i < releasedCount; i++)
        {
            spheres[i].transform.position = positions[i] * positionScale;
        }
    }

    void OnDestroy()
    {
        if (positionBuffer != null)
            positionBuffer.Release();
    }
}

using UnityEngine;

public class LeadMovement : MonoBehaviour
{
    [Header("Lorenz Parameters")]
    public float delta = 10f;  // σ
    public float beta = 8f / 3f;
    public float rho = 28f;

    [Header("Motion Settings")]
    public float timeScale = 0.001f;
    public float positionScale = 8f;
    public Vector3 initialPosition = new Vector3(1f, 1f, 1f);

    private Vector3 currentPos;

    void Start()
    {
        currentPos = initialPosition;
    }

    void Update()
    {
        float x = currentPos.x;
        float y = currentPos.y;
        float z = currentPos.z;

        // Lorenz system update
        float dx = delta * (y - x);
        float dy = x * (rho - z) - y;
        float dz = x * y - beta * z;

        currentPos += new Vector3(dx, dy, dz) * timeScale;

        // Scale and apply to world position
        transform.position = currentPos * positionScale;
    }
}

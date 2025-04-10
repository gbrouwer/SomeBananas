using UnityEngine;

public class GridExplosionCameraOrbit : MonoBehaviour
{
    public Transform target;  // The point to orbit around
    public float radius = 5f; // Distance from the target
    public float speed = 10f; // Rotation speed in degrees per second

    private float angle = 0f;

    void Update()
    {
        if (target == null)
        {
            Debug.LogWarning("CameraOrbit: No target assigned!");
            return;
        }

        // Calculate new angle
        angle += speed * Time.deltaTime;

        // Convert angle to radians
        float radians = angle * Mathf.Deg2Rad;

        // Calculate new position
        Vector3 newPosition = new Vector3(
            target.position.x + Mathf.Cos(radians) * radius,
            transform.position.y, // Maintain current height
            target.position.z + Mathf.Sin(radians) * radius
        );

        // Update camera position
        transform.position = newPosition;

        // Look at the target
        transform.LookAt(target);
    }
}

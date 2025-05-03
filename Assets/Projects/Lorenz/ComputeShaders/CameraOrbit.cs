using UnityEngine;

public class OrbitCamera : MonoBehaviour
{
    public Transform target;           // The point to orbit around
    public float distance = 50f;       // Initial distance from target
    public float zoomSpeed = 10f;      // Scroll wheel zoom
    public float minDistance = 5f;
    public float maxDistance = 200f;

    public float xSpeed = 120f;        // Horizontal mouse rotation speed
    public float ySpeed = 80f;         // Vertical mouse rotation speed
    public float yMinLimit = -20f;     // Min vertical angle
    public float yMaxLimit = 80f;      // Max vertical angle

    private float x = 0f;
    private float y = 0f;

    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;

        if (target == null)
        {
            GameObject origin = new GameObject("Camera Target");
            origin.transform.position = Vector3.zero;
            target = origin.transform;
        }
    }

    void LateUpdate()
    {
        if (Input.GetMouseButton(1)) // Right mouse button
        {
            x += Input.GetAxis("Mouse X") * xSpeed * Time.deltaTime;
            y -= Input.GetAxis("Mouse Y") * ySpeed * Time.deltaTime;
            y = Mathf.Clamp(y, yMinLimit, yMaxLimit);
        }

        // Scroll wheel zoom
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        distance -= scroll * zoomSpeed;
        distance = Mathf.Clamp(distance, minDistance, maxDistance);

        Quaternion rotation = Quaternion.Euler(y, x, 0);
        Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
        Vector3 position = rotation * negDistance + target.position;

        transform.rotation = rotation;
        transform.position = position;
    }
}

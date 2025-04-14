using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public Transform target; // The object the camera will follow
    public Vector3 offset = new Vector3(0f, 5f, -10f); // Default offset behind and above the target
    public float smoothSpeed = 5f; // How smooth the camera follows the target

    private void LateUpdate()
    {
        if (target == null)
            return;

        // Desired position based on the target's position + offset
        Vector3 desiredPosition = target.position + target.rotation * offset;

        // Smoothly interpolate to the desired position
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        // Always look at the target
        transform.LookAt(target);
    }
}

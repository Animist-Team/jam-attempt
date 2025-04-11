using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;      // Reference to the player
    public Vector3 offset;       // Offset distance between camera and player
    public float smoothSpeed = 0.125f;  // Smoothness of the camera follow
    public float distanceFromPlayer = 5f; // Distance of the camera from the player
    public float rotationSmoothness = 0.1f; // Smoothness of camera rotation follow

    private Vector3 velocity = Vector3.zero; // Used for smooth damping
    private Quaternion desiredRotation; // Desired rotation to follow player

    void Start()
    {
        // Initialize the desired rotation to the current camera rotation
        desiredRotation = transform.rotation;
    }

    void FixedUpdate()
    {
        // Calculate the desired position of the camera
        Vector3 desiredPosition = player.position + player.forward * -distanceFromPlayer + player.up * 2f;

        // Smoothly interpolate the position
        Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothSpeed);

        // Update the camera's position
        transform.position = smoothedPosition;

        // Smoothly interpolate the rotation of the camera to follow the player's rotation
        desiredRotation = Quaternion.LookRotation(player.position - transform.position, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationSmoothness);
    }
}

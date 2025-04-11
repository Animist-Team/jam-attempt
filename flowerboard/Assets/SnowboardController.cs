using UnityEngine;

public class SnowboardController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveForce = 10f;
    public float turnTorque = 5f;
    public float jumpForce = 10f;
    public float airRotationSpeed = 5f;
    public float maxForwardSpeed = 20f;

    [Header("Ground & Reset Settings")]
    public float groundCheckDistance = 1.5f;
    public LayerMask groundLayer;
    public float uprightThreshold = 10f;
    public float uprightResetTime = 1.5f;
    public float rotationSmoothTime = 0.2f;  // Time to smooth the rotation to match the slope

    private Rigidbody rb;
    private bool isGrounded;
    private bool isJumping = false;
    private bool isUpsideDown = false;
    private float timeOffUpright = 0f;
    private Quaternion originalRotation;
    private RaycastHit groundHit;
    private Quaternion targetRotation;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        originalRotation = transform.rotation;
    }

    void FixedUpdate()
    {
        CheckGrounded();
        CheckUpsideDown();
        HandleMovement();
        HandleUprightReset();
    }

    void CheckGrounded()
    {
        // Raycast to check if the snowboard is grounded and get the ground normal
        isGrounded = Physics.Raycast(transform.position, Vector3.down, out groundHit, groundCheckDistance, groundLayer);
    }

    void CheckUpsideDown()
    {
        // Determine if the snowboard is upside down
        float tiltAngle = Vector3.Angle(transform.up, Vector3.up);
        isUpsideDown = tiltAngle > 120f; // Consider it upside down only if flipped badly
    }

    void HandleMovement()
    {
        // Getting the forward direction on a plane (flattened movement)
        Vector3 forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        float speed = rb.velocity.magnitude;
        float turnInput = Input.GetAxis("Horizontal");

        if (isGrounded)
        {
            // Adjusting movement based on ground normal (slope)
            Vector3 groundNormal = groundHit.normal;
            Vector3 adjustedForward = Vector3.ProjectOnPlane(transform.forward, groundNormal).normalized;

            // Add force based on adjusted forward vector and the slope of the ground
            float slopeFactor = Vector3.Dot(groundNormal, Vector3.up);
            float slopeBoost = Mathf.Clamp01(slopeFactor); // The greater the slope factor, the easier it is to go up
            rb.AddForce(adjustedForward * moveForce * (1f + slopeBoost), ForceMode.Acceleration);

            // Turning torque (stronger based on speed)
            float turnStrength = Mathf.Clamp(speed / 10f, 0.5f, 1.5f);
            rb.AddTorque(transform.up * turnInput * turnTorque * turnStrength);

            // Smoothly align the snowboard's rotation with the ground normal
            targetRotation = Quaternion.FromToRotation(transform.up, groundNormal) * transform.rotation;
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSmoothTime);
        }
        else
        {
            // Air control (turning and pitch)
            float airTurn = Mathf.Clamp(speed / 5f, 1f, 3f);
            rb.AddTorque(transform.up * turnInput * turnTorque * airTurn);

            float pitchInput = Input.GetKey(KeyCode.W) ? -1f : Input.GetKey(KeyCode.S) ? 1f : 0f;
            rb.AddTorque(transform.right * pitchInput * airRotationSpeed);
        }

        // Jumping logic
        if (Input.GetButtonDown("Jump") && isGrounded && !isJumping)
        {
            isJumping = true;
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        if (isGrounded && rb.velocity.y <= 0f)
            isJumping = false;

        // Clamp forward speed to max value
        Vector3 flatVelocity = Vector3.ProjectOnPlane(rb.velocity, Vector3.up);
        float forwardSpeed = Vector3.Dot(flatVelocity, forward);

        if (forwardSpeed > maxForwardSpeed)
        {
            Vector3 clampedVelocity = forward * maxForwardSpeed;
            Vector3 lateral = rb.velocity - flatVelocity;
            rb.velocity = clampedVelocity + lateral + Vector3.up * rb.velocity.y;
        }
    }

    void HandleUprightReset()
    {
        // Handle upright reset logic if snowboard is flipped too far
        float tiltAngle = Vector3.Angle(transform.up, Vector3.up);

        if (tiltAngle > uprightThreshold)
            timeOffUpright += Time.deltaTime;
        else
            timeOffUpright = 0f;

        if (timeOffUpright >= uprightResetTime)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, originalRotation, Time.deltaTime * 2f);
            rb.angularVelocity = Vector3.Lerp(rb.angularVelocity, Vector3.zero, Time.deltaTime * 5f);

            if (tiltAngle > 45f)
                rb.angularVelocity = Vector3.zero;
        }
    }
}

using UnityEngine;

/// <summary>
/// Controls boat movement, rotation, and physics.
/// Handles WASD/controller input for movement and mouse/stick for rotation.
/// Integrates with EventSystem to publish OnPlayerMoved events.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class BoatController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float maxSpeed = 15f;
    [SerializeField] private float acceleration = 3f;
    [SerializeField] private float deceleration = 2f;

    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 80f;
    [SerializeField] private bool useMouseForRotation = false;
    [SerializeField] private float mouseSensitivity = 2f;

    [Header("Physics Settings")]
    [SerializeField] private float drag = 1f;
    [SerializeField] private float angularDrag = 3f;

    [Header("References")]
    private Rigidbody rb;
    private WaterPhysics waterPhysics;

    // Movement state
    private Vector3 currentVelocity = Vector3.zero;
    private float currentSpeed = 0f;
    private Vector3 lastPosition;

    // Input state
    private float horizontalInput;
    private float verticalInput;
    private float mouseX;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        waterPhysics = GetComponent<WaterPhysics>();

        // Configure rigidbody
        rb.useGravity = false;
        rb.drag = drag;
        rb.angularDrag = angularDrag;
        rb.constraints = RigidbodyConstraints.FreezePositionY |
                        RigidbodyConstraints.FreezeRotationX |
                        RigidbodyConstraints.FreezeRotationZ;

        lastPosition = transform.position;
    }

    private void Update()
    {
        // Gather input
        GatherInput();

        // Handle rotation in Update for smoother camera-relative controls
        HandleRotation();
    }

    private void FixedUpdate()
    {
        // Handle movement in FixedUpdate for consistent physics
        HandleMovement();

        // Publish movement event if position changed significantly
        if (Vector3.Distance(transform.position, lastPosition) > 0.01f)
        {
            PublishMovementEvent();
            lastPosition = transform.position;
        }
    }

    private void GatherInput()
    {
        // Get movement input
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");

        // Get rotation input
        if (useMouseForRotation)
        {
            mouseX = Input.GetAxis("Mouse X");
        }
    }

    private void HandleMovement()
    {
        // Calculate target speed based on input
        float targetSpeed = verticalInput * moveSpeed;

        // Smoothly interpolate current speed
        if (Mathf.Abs(verticalInput) > 0.1f)
        {
            currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, acceleration * Time.fixedDeltaTime);
        }
        else
        {
            currentSpeed = Mathf.Lerp(currentSpeed, 0f, deceleration * Time.fixedDeltaTime);
        }

        // Clamp to max speed
        currentSpeed = Mathf.Clamp(currentSpeed, -maxSpeed, maxSpeed);

        // Calculate movement direction (forward/backward along boat's forward axis)
        Vector3 moveDirection = transform.forward * currentSpeed;

        // Apply movement using Rigidbody
        rb.velocity = new Vector3(moveDirection.x, rb.velocity.y, moveDirection.z);

        currentVelocity = rb.velocity;
    }

    private void HandleRotation()
    {
        float rotationInput = 0f;

        if (useMouseForRotation)
        {
            // Mouse rotation
            rotationInput = mouseX * mouseSensitivity;
        }
        else
        {
            // Keyboard/controller rotation
            rotationInput = horizontalInput;
        }

        // Apply rotation
        if (Mathf.Abs(rotationInput) > 0.01f)
        {
            float rotationAmount = rotationInput * rotationSpeed * Time.deltaTime;
            transform.Rotate(Vector3.up, rotationAmount);
        }
    }

    private void PublishMovementEvent()
    {
        PlayerMovedEventData eventData = new PlayerMovedEventData(
            transform.position,
            currentVelocity,
            currentSpeed
        );

        EventSystem.Publish("OnPlayerMoved", eventData);
    }

    #region Public API

    /// <summary>
    /// Get current boat position
    /// </summary>
    public Vector3 GetPosition() => transform.position;

    /// <summary>
    /// Get current boat velocity
    /// </summary>
    public Vector3 GetVelocity() => currentVelocity;

    /// <summary>
    /// Get current boat speed (magnitude)
    /// </summary>
    public float GetSpeed() => currentSpeed;

    /// <summary>
    /// Get current boat forward direction
    /// </summary>
    public Vector3 GetForward() => transform.forward;

    /// <summary>
    /// Set boat position (used by save/load system)
    /// </summary>
    public void SetPosition(Vector3 position)
    {
        transform.position = position;
        lastPosition = position;
    }

    /// <summary>
    /// Enable or disable movement controls
    /// </summary>
    public void SetMovementEnabled(bool enabled)
    {
        this.enabled = enabled;
        if (!enabled)
        {
            currentSpeed = 0f;
            rb.velocity = Vector3.zero;
        }
    }

    #endregion

    private void OnDrawGizmos()
    {
        // Visualize movement direction
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, transform.forward * 3f);

        // Visualize velocity
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, currentVelocity);
    }
}

using UnityEngine;

/// <summary>
/// Controls camera movement and positioning to follow the boat.
/// Supports single camera view (Phase 1) and split-screen above/below water (Phase 2).
/// Provides smooth follow, configurable distance/angle, and camera shake effects.
/// </summary>
public class CameraController : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;
    [SerializeField] private bool autoFindTarget = true;

    [Header("Camera Settings")]
    [SerializeField] private float distance = 15f;
    [SerializeField] private float height = 8f;
    [SerializeField] private float angle = 30f;

    [Header("Follow Settings")]
    [SerializeField] private float followSpeed = 5f;
    [SerializeField] private float rotationSpeed = 3f;
    [SerializeField] private bool smoothFollow = true;

    [Header("Offset")]
    [SerializeField] private Vector3 positionOffset = Vector3.zero;
    [SerializeField] private Vector3 lookOffset = Vector3.zero;

    [Header("Camera Shake")]
    [SerializeField] private float shakeIntensity = 0.1f;
    [SerializeField] private float shakeDuration = 0.5f;
    private float shakeTimer = 0f;
    private Vector3 shakeOffset = Vector3.zero;

    [Header("Split Screen (Phase 2)")]
    [SerializeField] private bool enableSplitScreen = false;
    [SerializeField] private Camera underwaterCamera;
    [SerializeField] private float waterLevel = 0f;

    private Camera mainCamera;
    private Vector3 desiredPosition;
    private Quaternion desiredRotation;

    private void Awake()
    {
        mainCamera = GetComponent<Camera>();
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    private void Start()
    {
        if (autoFindTarget && target == null)
        {
            BoatController boat = FindObjectOfType<BoatController>();
            if (boat != null)
            {
                target = boat.transform;
            }
        }

        if (target == null)
        {
            Debug.LogWarning("CameraController: No target assigned!");
        }

        // Initialize split screen camera if enabled (Phase 2)
        if (enableSplitScreen && underwaterCamera == null)
        {
            SetupSplitScreen();
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        UpdateCameraShake();
        CalculateDesiredTransform();
        ApplyCameraTransform();

        if (enableSplitScreen && underwaterCamera != null)
        {
            UpdateUnderwaterCamera();
        }
    }

    private void CalculateDesiredTransform()
    {
        // Calculate position behind and above target
        Vector3 targetForward = target.forward;
        Vector3 targetPosition = target.position;

        // Calculate camera position based on distance and height
        Vector3 direction = -targetForward;
        direction.y = 0;
        direction.Normalize();

        desiredPosition = targetPosition +
                         (direction * distance) +
                         (Vector3.up * height) +
                         positionOffset +
                         shakeOffset;

        // Calculate rotation to look at target
        Vector3 lookAtPosition = targetPosition + lookOffset;
        desiredRotation = Quaternion.LookRotation(lookAtPosition - desiredPosition);

        // Apply angle adjustment
        desiredRotation *= Quaternion.Euler(angle, 0, 0);
    }

    private void ApplyCameraTransform()
    {
        if (smoothFollow)
        {
            // Smooth follow
            transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationSpeed * Time.deltaTime);
        }
        else
        {
            // Instant follow
            transform.position = desiredPosition;
            transform.rotation = desiredRotation;
        }
    }

    #region Camera Shake

    private void UpdateCameraShake()
    {
        if (shakeTimer > 0)
        {
            shakeTimer -= Time.deltaTime;

            // Generate random shake offset
            shakeOffset = new Vector3(
                Random.Range(-1f, 1f),
                Random.Range(-1f, 1f),
                Random.Range(-1f, 1f)
            ) * shakeIntensity * (shakeTimer / shakeDuration);
        }
        else
        {
            shakeOffset = Vector3.zero;
        }
    }

    /// <summary>
    /// Trigger camera shake effect
    /// </summary>
    public void Shake(float intensity, float duration)
    {
        shakeIntensity = intensity;
        shakeDuration = duration;
        shakeTimer = duration;
    }

    /// <summary>
    /// Trigger default camera shake
    /// </summary>
    public void Shake()
    {
        shakeTimer = shakeDuration;
    }

    #endregion

    #region Split Screen (Phase 2)

    private void SetupSplitScreen()
    {
        // Create underwater camera
        GameObject underwaterCamGO = new GameObject("UnderwaterCamera");
        underwaterCamera = underwaterCamGO.AddComponent<Camera>();
        underwaterCamera.transform.SetParent(transform);

        // Configure viewport for split screen
        mainCamera.rect = new Rect(0, 0.5f, 1, 0.5f); // Top half
        underwaterCamera.rect = new Rect(0, 0, 1, 0.5f); // Bottom half

        // Copy settings from main camera
        underwaterCamera.fieldOfView = mainCamera.fieldOfView;
        underwaterCamera.nearClipPlane = mainCamera.nearClipPlane;
        underwaterCamera.farClipPlane = mainCamera.farClipPlane;

        Debug.Log("Split screen camera setup complete");
    }

    private void UpdateUnderwaterCamera()
    {
        if (underwaterCamera == null) return;

        // Position underwater camera below water level
        Vector3 underwaterPos = transform.position;
        underwaterPos.y = waterLevel - height;
        underwaterCamera.transform.position = underwaterPos;

        // Look at same target from below
        Vector3 targetPos = target.position;
        targetPos.y = waterLevel - 2f;
        underwaterCamera.transform.LookAt(targetPos);
    }

    /// <summary>
    /// Toggle split screen mode
    /// </summary>
    public void SetSplitScreenEnabled(bool enabled)
    {
        enableSplitScreen = enabled;

        if (enabled && underwaterCamera == null)
        {
            SetupSplitScreen();
        }
        else if (!enabled && underwaterCamera != null)
        {
            mainCamera.rect = new Rect(0, 0, 1, 1); // Full screen
            underwaterCamera.enabled = false;
        }
        else if (enabled && underwaterCamera != null)
        {
            mainCamera.rect = new Rect(0, 0.5f, 1, 0.5f);
            underwaterCamera.enabled = true;
        }
    }

    #endregion

    #region Public API

    /// <summary>
    /// Set the camera target
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    /// <summary>
    /// Set camera distance from target
    /// </summary>
    public void SetDistance(float newDistance)
    {
        distance = newDistance;
    }

    /// <summary>
    /// Set camera height above target
    /// </summary>
    public void SetHeight(float newHeight)
    {
        height = newHeight;
    }

    /// <summary>
    /// Set camera angle
    /// </summary>
    public void SetAngle(float newAngle)
    {
        angle = newAngle;
    }

    /// <summary>
    /// Set follow speed
    /// </summary>
    public void SetFollowSpeed(float speed)
    {
        followSpeed = speed;
    }

    /// <summary>
    /// Enable or disable smooth following
    /// </summary>
    public void SetSmoothFollow(bool smooth)
    {
        smoothFollow = smooth;
    }

    /// <summary>
    /// Get main camera
    /// </summary>
    public Camera GetMainCamera()
    {
        return mainCamera;
    }

    /// <summary>
    /// Get underwater camera (if split screen enabled)
    /// </summary>
    public Camera GetUnderwaterCamera()
    {
        return underwaterCamera;
    }

    #endregion

    private void OnDrawGizmosSelected()
    {
        if (target == null) return;

        // Draw desired camera position
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(desiredPosition, 0.5f);

        // Draw line from camera to target
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, target.position + lookOffset);

        // Draw camera frustum
        Gizmos.color = Color.green;
        Gizmos.matrix = Matrix4x4.TRS(desiredPosition, desiredRotation, Vector3.one);
        Gizmos.DrawFrustum(Vector3.zero, mainCamera != null ? mainCamera.fieldOfView : 60f, 2f, 0.1f, mainCamera != null ? mainCamera.aspect : 1.7f);
    }
}

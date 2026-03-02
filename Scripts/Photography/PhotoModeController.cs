using UnityEngine;

/// <summary>
/// Agent 18: Photography Mode Specialist - PhotoModeController.cs
/// Manages photo mode activation, free camera movement, and game state during photography.
/// Allows players to enter a dedicated photo mode with full camera control and time pause.
/// </summary>
public class PhotoModeController : MonoBehaviour
{
    public static PhotoModeController Instance { get; private set; }

    [Header("Photo Mode Settings")]
    [SerializeField] private KeyCode photoModeKey = KeyCode.P;
    [SerializeField] private bool pauseTimeInPhotoMode = true;
    [SerializeField] private float freeCameraSpeed = 10f;
    [SerializeField] private float freeCameraSprintMultiplier = 2f;
    [SerializeField] private float freeCameraSlowMultiplier = 0.5f;
    [SerializeField] private float freeCameraMouseSensitivity = 2f;

    [Header("Camera Limits")]
    [SerializeField] private float maxDistanceFromPlayer = 100f;
    [SerializeField] private float minFOV = 30f;
    [SerializeField] private float maxFOV = 90f;
    [SerializeField] private float fovChangeSpeed = 10f;

    [Header("References")]
    [SerializeField] private CameraController mainCameraController;
    [SerializeField] private Camera photoCamera;

    // State
    private bool isInPhotoMode = false;
    private Vector3 photoCameraStartPosition;
    private Quaternion photoCameraStartRotation;
    private float photoCameraFOV = 60f;
    private float photoCameraTilt = 0f;

    // Camera settings
    private float exposure = 0f;
    private float contrast = 1f;
    private float saturation = 1f;
    private float brightness = 0f;
    private float focusDistance = 10f;
    private float dofBlurAmount = 0f;

    // Original state
    private float originalTimeScale;
    private bool originalCameraControllerState;
    private Vector3 playerPosition;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // Auto-find references if not set
        if (mainCameraController == null)
        {
            mainCameraController = FindObjectOfType<CameraController>();
        }

        if (photoCamera == null)
        {
            photoCamera = Camera.main;
        }

        // Initialize photo camera settings
        if (photoCamera != null)
        {
            photoCameraFOV = photoCamera.fieldOfView;
        }
    }

    private void Update()
    {
        // Toggle photo mode
        if (Input.GetKeyDown(photoModeKey))
        {
            TogglePhotoMode();
        }

        // Photo mode controls
        if (isInPhotoMode)
        {
            HandlePhotoModeInput();
        }
    }

    /// <summary>
    /// Toggles photo mode on/off.
    /// </summary>
    public void TogglePhotoMode()
    {
        if (isInPhotoMode)
        {
            ExitPhotoMode();
        }
        else
        {
            EnterPhotoMode();
        }
    }

    /// <summary>
    /// Enters photo mode with free camera control.
    /// </summary>
    public void EnterPhotoMode()
    {
        if (isInPhotoMode) return;

        isInPhotoMode = true;

        // Store original state
        originalTimeScale = Time.timeScale;
        if (pauseTimeInPhotoMode)
        {
            Time.timeScale = 0f;
        }

        // Store player position for distance limiting
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerPosition = player.transform.position;
        }

        // Store camera state
        if (photoCamera != null)
        {
            photoCameraStartPosition = photoCamera.transform.position;
            photoCameraStartRotation = photoCamera.transform.rotation;
            photoCameraFOV = photoCamera.fieldOfView;
        }

        // Disable main camera controller
        if (mainCameraController != null)
        {
            originalCameraControllerState = mainCameraController.enabled;
            mainCameraController.enabled = false;
        }

        // Unlock and show cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Publish event
        EventSystem.Publish("PhotoModeEntered");

        Debug.Log("[PhotoModeController] Entered photo mode");
    }

    /// <summary>
    /// Exits photo mode and resumes normal gameplay.
    /// </summary>
    public void ExitPhotoMode()
    {
        if (!isInPhotoMode) return;

        isInPhotoMode = false;

        // Restore time scale
        Time.timeScale = originalTimeScale;

        // Restore camera controller
        if (mainCameraController != null)
        {
            mainCameraController.enabled = originalCameraControllerState;
        }

        // Reset camera settings
        if (photoCamera != null)
        {
            photoCamera.fieldOfView = 60f;
        }

        // Lock cursor for gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Publish event
        EventSystem.Publish("PhotoModeExited");

        Debug.Log("[PhotoModeController] Exited photo mode");
    }

    /// <summary>
    /// Handles input during photo mode.
    /// </summary>
    private void HandlePhotoModeInput()
    {
        if (photoCamera == null) return;

        // Use unscaled delta time for movement during pause
        float deltaTime = Time.unscaledDeltaTime;

        // Camera movement (WASD)
        Vector3 movement = Vector3.zero;

        if (Input.GetKey(KeyCode.W)) movement += photoCamera.transform.forward;
        if (Input.GetKey(KeyCode.S)) movement -= photoCamera.transform.forward;
        if (Input.GetKey(KeyCode.A)) movement -= photoCamera.transform.right;
        if (Input.GetKey(KeyCode.D)) movement += photoCamera.transform.right;
        if (Input.GetKey(KeyCode.Q)) movement -= photoCamera.transform.up;
        if (Input.GetKey(KeyCode.E)) movement += photoCamera.transform.up;

        // Apply speed multipliers
        float currentSpeed = freeCameraSpeed;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            currentSpeed *= freeCameraSprintMultiplier;
        }
        else if (Input.GetKey(KeyCode.LeftControl))
        {
            currentSpeed *= freeCameraSlowMultiplier;
        }

        // Move camera
        if (movement != Vector3.zero)
        {
            movement = movement.normalized * currentSpeed * deltaTime;
            Vector3 newPosition = photoCamera.transform.position + movement;

            // Limit distance from player
            if (Vector3.Distance(newPosition, playerPosition) <= maxDistanceFromPlayer)
            {
                photoCamera.transform.position = newPosition;
            }
        }

        // Mouse look (hold right mouse button)
        if (Input.GetMouseButton(1))
        {
            float mouseX = Input.GetAxis("Mouse X") * freeCameraMouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * freeCameraMouseSensitivity;

            photoCamera.transform.Rotate(Vector3.up, mouseX, Space.World);
            photoCamera.transform.Rotate(Vector3.right, -mouseY, Space.Self);
        }

        // FOV adjustment (scroll wheel)
        float scrollDelta = Input.GetAxis("Mouse ScrollWheel");
        if (scrollDelta != 0f)
        {
            photoCameraFOV -= scrollDelta * fovChangeSpeed;
            photoCameraFOV = Mathf.Clamp(photoCameraFOV, minFOV, maxFOV);
            photoCamera.fieldOfView = photoCameraFOV;
        }

        // Reset camera position (R key)
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetCameraPosition();
        }

        // Take photo (Space or Left Mouse)
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Mouse0))
        {
            TakePhoto();
        }
    }

    /// <summary>
    /// Resets camera to starting position.
    /// </summary>
    public void ResetCameraPosition()
    {
        if (photoCamera != null)
        {
            photoCamera.transform.position = photoCameraStartPosition;
            photoCamera.transform.rotation = photoCameraStartRotation;
            photoCameraFOV = 60f;
            photoCamera.fieldOfView = photoCameraFOV;
        }
    }

    /// <summary>
    /// Captures a photo.
    /// </summary>
    private void TakePhoto()
    {
        if (PhotoStorage.Instance != null)
        {
            PhotoStorage.Instance.CapturePhoto(photoCamera);
        }
        else
        {
            Debug.LogWarning("[PhotoModeController] PhotoStorage instance not found!");
        }
    }

    #region Camera Settings

    /// <summary>
    /// Sets the field of view of the photo camera.
    /// </summary>
    public void SetFOV(float fov)
    {
        photoCameraFOV = Mathf.Clamp(fov, minFOV, maxFOV);
        if (photoCamera != null)
        {
            photoCamera.fieldOfView = photoCameraFOV;
        }
    }

    /// <summary>
    /// Sets the camera tilt (Dutch angle).
    /// </summary>
    public void SetTilt(float tilt)
    {
        photoCameraTilt = Mathf.Clamp(tilt, -45f, 45f);
        if (photoCamera != null)
        {
            Vector3 eulerAngles = photoCamera.transform.eulerAngles;
            eulerAngles.z = photoCameraTilt;
            photoCamera.transform.eulerAngles = eulerAngles;
        }
    }

    /// <summary>
    /// Sets camera exposure.
    /// </summary>
    public void SetExposure(float value)
    {
        exposure = Mathf.Clamp(value, -2f, 2f);
    }

    /// <summary>
    /// Sets camera contrast.
    /// </summary>
    public void SetContrast(float value)
    {
        contrast = Mathf.Clamp(value, 0f, 2f);
    }

    /// <summary>
    /// Sets camera saturation.
    /// </summary>
    public void SetSaturation(float value)
    {
        saturation = Mathf.Clamp(value, 0f, 2f);
    }

    /// <summary>
    /// Sets camera brightness.
    /// </summary>
    public void SetBrightness(float value)
    {
        brightness = Mathf.Clamp(value, -50f, 50f);
    }

    /// <summary>
    /// Sets depth of field focus distance.
    /// </summary>
    public void SetFocusDistance(float distance)
    {
        focusDistance = Mathf.Max(0f, distance);
    }

    /// <summary>
    /// Sets depth of field blur amount.
    /// </summary>
    public void SetDepthOfFieldBlur(float amount)
    {
        dofBlurAmount = Mathf.Clamp01(amount);
    }

    #endregion

    #region Public API

    /// <summary>
    /// Gets whether photo mode is currently active.
    /// </summary>
    public bool IsInPhotoMode()
    {
        return isInPhotoMode;
    }

    /// <summary>
    /// Gets the photo camera.
    /// </summary>
    public Camera GetPhotoCamera()
    {
        return photoCamera;
    }

    /// <summary>
    /// Gets current FOV.
    /// </summary>
    public float GetFOV()
    {
        return photoCameraFOV;
    }

    /// <summary>
    /// Gets current tilt.
    /// </summary>
    public float GetTilt()
    {
        return photoCameraTilt;
    }

    /// <summary>
    /// Gets current exposure.
    /// </summary>
    public float GetExposure()
    {
        return exposure;
    }

    /// <summary>
    /// Gets current contrast.
    /// </summary>
    public float GetContrast()
    {
        return contrast;
    }

    /// <summary>
    /// Gets current saturation.
    /// </summary>
    public float GetSaturation()
    {
        return saturation;
    }

    /// <summary>
    /// Gets current brightness.
    /// </summary>
    public float GetBrightness()
    {
        return brightness;
    }

    /// <summary>
    /// Gets current focus distance.
    /// </summary>
    public float GetFocusDistance()
    {
        return focusDistance;
    }

    /// <summary>
    /// Gets current depth of field blur amount.
    /// </summary>
    public float GetDepthOfFieldBlur()
    {
        return dofBlurAmount;
    }

    /// <summary>
    /// Gets the player position (for distance calculations).
    /// </summary>
    public Vector3 GetPlayerPosition()
    {
        return playerPosition;
    }

    #endregion
}

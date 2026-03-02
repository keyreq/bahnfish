using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Displays fishing line tension during fishing.
/// Shows horizontal or vertical bar with danger zone at high tension.
/// Pulses and shakes when near breaking point.
/// Only visible during active fishing.
/// Position: Bottom-center during fishing.
/// </summary>
public class TensionMeter : MonoBehaviour
{
    #region Inspector References
    [Header("Display Type")]
    [Tooltip("Orientation of the tension bar")]
    public TensionBarOrientation orientation = TensionBarOrientation.Horizontal;

    [Header("UI Elements")]
    [Tooltip("Fill image for tension bar")]
    public Image tensionFillBar;

    [Tooltip("Background image for the meter")]
    public Image backgroundImage;

    [Tooltip("Danger zone overlay (red at high tension)")]
    public Image dangerZoneOverlay;

    [Tooltip("Optional text showing tension percentage")]
    public TextMeshProUGUI tensionText;

    [Header("Tension Thresholds")]
    [Tooltip("Tension level considered dangerous (0-100)")]
    [Range(0f, 100f)]
    public float dangerThreshold = 75f;

    [Tooltip("Tension level that breaks the line (0-100)")]
    [Range(0f, 100f)]
    public float breakThreshold = 95f;

    [Header("Colors")]
    [Tooltip("Color at low tension (safe)")]
    public Color safeTensionColor = new Color(0.2f, 1f, 0.2f); // Green

    [Tooltip("Color at medium tension (warning)")]
    public Color warningTensionColor = new Color(1f, 1f, 0f); // Yellow

    [Tooltip("Color at high tension (danger)")]
    public Color dangerTensionColor = new Color(1f, 0.2f, 0.2f); // Red

    [Tooltip("Color when about to break")]
    public Color criticalTensionColor = new Color(1f, 0f, 0f); // Bright Red

    [Header("Visual Effects")]
    [Tooltip("Enable pulsing at high tension")]
    public bool enablePulsing = true;

    [Tooltip("Pulse speed when in danger zone")]
    [Range(1f, 10f)]
    public float pulseSpeed = 3f;

    [Tooltip("Pulse intensity")]
    [Range(0f, 0.5f)]
    public float pulseIntensity = 0.2f;

    [Tooltip("Enable shake effect near break")]
    public bool enableShaking = true;

    [Tooltip("Shake intensity")]
    [Range(0f, 10f)]
    public float shakeIntensity = 3f;

    [Header("Display Settings")]
    [Tooltip("Show numerical tension percentage")]
    public bool showPercentage = false;

    [Tooltip("Smooth tension value changes")]
    public bool smoothTransition = true;

    [Tooltip("Transition speed")]
    [Range(1f, 20f)]
    public float transitionSpeed = 10f;

    [Header("Visibility")]
    [Tooltip("Auto-hide when not fishing")]
    public bool autoHide = true;

    [Header("Debug")]
    [Tooltip("Enable debug logging")]
    public bool enableDebugLogging = false;
    #endregion

    #region Private Variables
    private float currentTension = 0f;
    private float displayTension = 0f;
    private bool isFishing = false;
    private Vector3 originalPosition;
    private CanvasGroup canvasGroup;
    #endregion

    #region Enums
    public enum TensionBarOrientation
    {
        Horizontal,
        Vertical
    }
    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        // Get or add canvas group
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // Store original position
        originalPosition = transform.localPosition;
    }

    private void Start()
    {
        // Subscribe to fishing events (will be published by Agent 5)
        EventSystem.Subscribe("FishingStarted", OnFishingStarted);
        EventSystem.Subscribe("FishingEnded", OnFishingEnded);
        EventSystem.Subscribe<float>("OnTensionChanged", OnTensionChanged);

        // Hide initially if auto-hide enabled
        if (autoHide)
        {
            Hide();
        }

        if (enableDebugLogging)
        {
            Debug.Log("[TensionMeter] Initialized");
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        EventSystem.Unsubscribe("FishingStarted", OnFishingStarted);
        EventSystem.Unsubscribe("FishingEnded", OnFishingEnded);
        EventSystem.Unsubscribe<float>("OnTensionChanged", OnTensionChanged);
    }

    private void Update()
    {
        // Smooth transition to current tension
        if (smoothTransition && Mathf.Abs(displayTension - currentTension) > 0.1f)
        {
            displayTension = Mathf.Lerp(displayTension, currentTension, Time.deltaTime * transitionSpeed);
            UpdateVisuals();
        }

        // Apply effects when fishing
        if (isFishing)
        {
            ApplyVisualEffects();
        }
    }

    #endregion

    #region Event Handlers

    /// <summary>
    /// Called when fishing starts
    /// </summary>
    private void OnFishingStarted()
    {
        isFishing = true;
        if (autoHide)
        {
            Show();
        }

        if (enableDebugLogging)
        {
            Debug.Log("[TensionMeter] Fishing started - showing meter");
        }
    }

    /// <summary>
    /// Called when fishing ends
    /// </summary>
    private void OnFishingEnded()
    {
        isFishing = false;
        currentTension = 0f;
        displayTension = 0f;
        UpdateVisuals();

        if (autoHide)
        {
            Hide();
        }

        if (enableDebugLogging)
        {
            Debug.Log("[TensionMeter] Fishing ended - hiding meter");
        }
    }

    /// <summary>
    /// Called when tension changes during fishing
    /// </summary>
    private void OnTensionChanged(float newTension)
    {
        SetTension(newTension);
    }

    #endregion

    #region Tension Updates

    /// <summary>
    /// Set the tension value (0-100)
    /// </summary>
    public void SetTension(float tension)
    {
        currentTension = Mathf.Clamp(tension, 0f, 100f);

        if (!smoothTransition)
        {
            displayTension = currentTension;
            UpdateVisuals();
        }

        if (enableDebugLogging && Mathf.Abs(tension - currentTension) > 10f)
        {
            Debug.Log($"[TensionMeter] Tension: {currentTension:F1}%");
        }
    }

    /// <summary>
    /// Update all visual elements
    /// </summary>
    private void UpdateVisuals()
    {
        UpdateFillBar();
        UpdateColor();
        UpdateText();
        UpdateDangerZone();
    }

    /// <summary>
    /// Update the fill bar
    /// </summary>
    private void UpdateFillBar()
    {
        if (tensionFillBar == null)
            return;

        float fillAmount = displayTension / 100f;

        tensionFillBar.fillAmount = fillAmount;
        tensionFillBar.type = Image.Type.Filled;

        if (orientation == TensionBarOrientation.Horizontal)
        {
            tensionFillBar.fillMethod = Image.FillMethod.Horizontal;
            tensionFillBar.fillOrigin = (int)Image.OriginHorizontal.Left;
        }
        else
        {
            tensionFillBar.fillMethod = Image.FillMethod.Vertical;
            tensionFillBar.fillOrigin = (int)Image.OriginVertical.Bottom;
        }
    }

    /// <summary>
    /// Update color based on tension level
    /// </summary>
    private void UpdateColor()
    {
        if (tensionFillBar == null)
            return;

        Color targetColor = GetColorForTension(displayTension);
        tensionFillBar.color = targetColor;
    }

    /// <summary>
    /// Update text display
    /// </summary>
    private void UpdateText()
    {
        if (tensionText != null && showPercentage)
        {
            tensionText.text = $"{Mathf.RoundToInt(displayTension)}%";
            tensionText.enabled = true;
        }
        else if (tensionText != null)
        {
            tensionText.enabled = false;
        }
    }

    /// <summary>
    /// Update danger zone overlay
    /// </summary>
    private void UpdateDangerZone()
    {
        if (dangerZoneOverlay == null)
            return;

        // Show danger zone when above danger threshold
        if (displayTension >= dangerThreshold)
        {
            float alpha = Mathf.Clamp01((displayTension - dangerThreshold) / (100f - dangerThreshold));
            Color dangerColor = dangerZoneOverlay.color;
            dangerColor.a = alpha * 0.5f;
            dangerZoneOverlay.color = dangerColor;
            dangerZoneOverlay.enabled = true;
        }
        else
        {
            dangerZoneOverlay.enabled = false;
        }
    }

    #endregion

    #region Visual Effects

    /// <summary>
    /// Get color based on tension value
    /// </summary>
    private Color GetColorForTension(float tension)
    {
        if (tension >= breakThreshold)
        {
            return criticalTensionColor;
        }
        else if (tension >= dangerThreshold)
        {
            // Interpolate between warning and danger
            float t = (tension - dangerThreshold) / (breakThreshold - dangerThreshold);
            return Color.Lerp(warningTensionColor, dangerTensionColor, t);
        }
        else if (tension >= 50f)
        {
            // Interpolate between safe and warning
            float t = (tension - 50f) / (dangerThreshold - 50f);
            return Color.Lerp(safeTensionColor, warningTensionColor, t);
        }
        else
        {
            return safeTensionColor;
        }
    }

    /// <summary>
    /// Apply visual effects (pulse and shake)
    /// </summary>
    private void ApplyVisualEffects()
    {
        // Pulse effect at high tension
        if (enablePulsing && displayTension >= dangerThreshold)
        {
            float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed * Mathf.PI) * pulseIntensity;
            transform.localScale = Vector3.one * pulse;
        }
        else
        {
            transform.localScale = Vector3.one;
        }

        // Shake effect near break
        if (enableShaking && displayTension >= breakThreshold)
        {
            float shake = shakeIntensity * ((displayTension - breakThreshold) / (100f - breakThreshold));
            Vector3 randomOffset = new Vector3(
                Random.Range(-shake, shake),
                Random.Range(-shake, shake),
                0f
            );
            transform.localPosition = originalPosition + randomOffset;
        }
        else
        {
            transform.localPosition = originalPosition;
        }
    }

    #endregion

    #region Visibility

    /// <summary>
    /// Show the tension meter
    /// </summary>
    public void Show()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Hide the tension meter
    /// </summary>
    public void Hide()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        // Don't deactivate, just make invisible for smoother transitions
    }

    #endregion

    #region Public Interface

    /// <summary>
    /// Get current tension value
    /// </summary>
    public float GetCurrentTension()
    {
        return currentTension;
    }

    /// <summary>
    /// Check if tension is in danger zone
    /// </summary>
    public bool IsInDangerZone()
    {
        return currentTension >= dangerThreshold;
    }

    /// <summary>
    /// Check if tension is at breaking point
    /// </summary>
    public bool IsAtBreakingPoint()
    {
        return currentTension >= breakThreshold;
    }

    /// <summary>
    /// Set orientation
    /// </summary>
    public void SetOrientation(TensionBarOrientation newOrientation)
    {
        orientation = newOrientation;
        UpdateFillBar();
    }

    #endregion

    #region Debug Methods

    /// <summary>
    /// Test different tension levels
    /// </summary>
    [ContextMenu("Test Low Tension (25%)")]
    private void TestLowTension()
    {
        isFishing = true;
        SetTension(25f);
        Show();
    }

    [ContextMenu("Test Medium Tension (50%)")]
    private void TestMediumTension()
    {
        isFishing = true;
        SetTension(50f);
        Show();
    }

    [ContextMenu("Test High Tension (80%)")]
    private void TestHighTension()
    {
        isFishing = true;
        SetTension(80f);
        Show();
    }

    [ContextMenu("Test Critical Tension (98%)")]
    private void TestCriticalTension()
    {
        isFishing = true;
        SetTension(98f);
        Show();
    }

    [ContextMenu("Simulate Fishing")]
    private void SimulateFishing()
    {
        StartCoroutine(SimulateFishingCoroutine());
    }

    private System.Collections.IEnumerator SimulateFishingCoroutine()
    {
        OnFishingStarted();

        for (int i = 0; i <= 100; i += 5)
        {
            SetTension(i);
            yield return new WaitForSeconds(0.2f);
        }

        yield return new WaitForSeconds(0.5f);

        for (int i = 100; i >= 0; i -= 5)
        {
            SetTension(i);
            yield return new WaitForSeconds(0.2f);
        }

        OnFishingEnded();
    }

    #endregion
}

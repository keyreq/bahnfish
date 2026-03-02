using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Visual sanity indicator for the HUD.
/// Displays sanity level with color gradient (Green → Yellow → Red).
/// Pulses and shakes at low sanity levels.
/// Subscribes to Agent 7's OnSanityChanged event.
/// Position: Top-left or top-right corner.
/// </summary>
public class SanityMeter : MonoBehaviour
{
    #region Inspector References
    [Header("Display Type")]
    [Tooltip("Type of sanity meter visualization")]
    public SanityMeterType meterType = SanityMeterType.CircularMeter;

    [Header("UI Elements")]
    [Tooltip("Image component for the sanity fill (bar or circular)")]
    public Image fillImage;

    [Tooltip("Background image for the meter")]
    public Image backgroundImage;

    [Tooltip("Optional icon image (e.g., brain or eye icon)")]
    public Image iconImage;

    [Tooltip("Optional text showing sanity percentage")]
    public TextMeshProUGUI sanityText;

    [Header("Colors")]
    [Tooltip("Color at 100% sanity (healthy)")]
    public Color highSanityColor = new Color(0.2f, 1f, 0.2f); // Green

    [Tooltip("Color at 50% sanity (warning)")]
    public Color mediumSanityColor = new Color(1f, 1f, 0f); // Yellow

    [Tooltip("Color at 0% sanity (critical)")]
    public Color lowSanityColor = new Color(1f, 0.2f, 0.2f); // Red

    [Header("Thresholds")]
    [Tooltip("Sanity level below which to show warning effects (0-100)")]
    [Range(0f, 100f)]
    public float warningThreshold = 50f;

    [Tooltip("Sanity level below which to show critical effects (0-100)")]
    [Range(0f, 100f)]
    public float criticalThreshold = 25f;

    [Header("Visual Effects")]
    [Tooltip("Enable pulsing effect at low sanity")]
    public bool enablePulsing = true;

    [Tooltip("Pulse speed multiplier")]
    [Range(0.5f, 5f)]
    public float pulseSpeed = 2f;

    [Tooltip("Pulse intensity (scale change)")]
    [Range(0f, 0.5f)]
    public float pulseIntensity = 0.15f;

    [Tooltip("Enable shake effect at critical sanity")]
    public bool enableShaking = true;

    [Tooltip("Shake intensity")]
    [Range(0f, 20f)]
    public float shakeIntensity = 5f;

    [Header("Display Settings")]
    [Tooltip("Show numerical percentage")]
    public bool showPercentage = false;

    [Tooltip("Smooth sanity value changes")]
    public bool smoothTransition = true;

    [Tooltip("Transition speed")]
    [Range(0.1f, 10f)]
    public float transitionSpeed = 3f;

    [Header("Visibility")]
    [Tooltip("Reduce opacity at high sanity (subtle)")]
    public bool subtleAtHighSanity = true;

    [Tooltip("Alpha value at high sanity")]
    [Range(0.3f, 1f)]
    public float highSanityAlpha = 0.7f;

    [Tooltip("Alpha value at low sanity")]
    [Range(0.8f, 1f)]
    public float lowSanityAlpha = 1f;

    [Header("Debug")]
    [Tooltip("Enable debug logging")]
    public bool enableDebugLogging = false;
    #endregion

    #region Private Variables
    private float currentSanity = 100f;
    private float targetSanity = 100f;
    private float displaySanity = 100f;
    private Vector3 originalPosition;
    private Vector3 originalScale;
    private CanvasGroup canvasGroup;
    private Coroutine pulseCoroutine;
    private bool isPulsing = false;
    #endregion

    #region Enums
    public enum SanityMeterType
    {
        HorizontalBar,
        VerticalBar,
        CircularMeter,
        Icon
    }
    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        // Get or add canvas group for alpha control
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // Store original transform values
        originalPosition = transform.localPosition;
        originalScale = transform.localScale;
    }

    private void Start()
    {
        // Subscribe to sanity changed event (will be published by Agent 7)
        EventSystem.Subscribe<float>("OnSanityChanged", OnSanityChanged);
        EventSystem.Subscribe<GameState>("GameStateUpdated", OnGameStateUpdated);

        // Initialize with current sanity from GameState
        if (GameManager.Instance != null)
        {
            currentSanity = GameManager.Instance.CurrentGameState.sanity;
            targetSanity = currentSanity;
            displaySanity = currentSanity;
            UpdateVisuals();
        }

        if (enableDebugLogging)
        {
            Debug.Log("[SanityMeter] Initialized");
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        EventSystem.Unsubscribe<float>("OnSanityChanged", OnSanityChanged);
        EventSystem.Unsubscribe<GameState>("GameStateUpdated", OnGameStateUpdated);
    }

    private void Update()
    {
        // Smooth transition to target sanity
        if (smoothTransition && Mathf.Abs(displaySanity - targetSanity) > 0.01f)
        {
            displaySanity = Mathf.Lerp(displaySanity, targetSanity, Time.deltaTime * transitionSpeed);
            UpdateVisuals();
        }

        // Apply shake effect if critical
        if (enableShaking && displaySanity <= criticalThreshold)
        {
            ApplyShake();
        }
        else
        {
            // Reset to original position when not shaking
            transform.localPosition = originalPosition;
        }
    }

    #endregion

    #region Event Handlers

    /// <summary>
    /// Called when sanity changes (from Agent 7)
    /// </summary>
    private void OnSanityChanged(float newSanity)
    {
        SetSanity(newSanity);

        if (enableDebugLogging)
        {
            Debug.Log($"[SanityMeter] Sanity changed: {newSanity:F1}%");
        }
    }

    /// <summary>
    /// Called when game state updates
    /// </summary>
    private void OnGameStateUpdated(GameState state)
    {
        if (state != null)
        {
            SetSanity(state.sanity);
        }
    }

    #endregion

    #region Sanity Updates

    /// <summary>
    /// Set the sanity value (0-100)
    /// </summary>
    public void SetSanity(float sanity)
    {
        targetSanity = Mathf.Clamp(sanity, 0f, 100f);

        if (!smoothTransition)
        {
            displaySanity = targetSanity;
            UpdateVisuals();
        }

        // Start or stop pulsing based on threshold
        UpdatePulsingState();
    }

    /// <summary>
    /// Update all visual elements
    /// </summary>
    private void UpdateVisuals()
    {
        UpdateFill();
        UpdateColor();
        UpdateText();
        UpdateAlpha();
    }

    /// <summary>
    /// Update the fill amount
    /// </summary>
    private void UpdateFill()
    {
        if (fillImage == null)
            return;

        float fillAmount = displaySanity / 100f;

        switch (meterType)
        {
            case SanityMeterType.CircularMeter:
                fillImage.fillAmount = fillAmount;
                fillImage.type = Image.Type.Filled;
                fillImage.fillMethod = Image.FillMethod.Radial360;
                break;

            case SanityMeterType.HorizontalBar:
                fillImage.fillAmount = fillAmount;
                fillImage.type = Image.Type.Filled;
                fillImage.fillMethod = Image.FillMethod.Horizontal;
                break;

            case SanityMeterType.VerticalBar:
                fillImage.fillAmount = fillAmount;
                fillImage.type = Image.Type.Filled;
                fillImage.fillMethod = Image.FillMethod.Vertical;
                break;

            case SanityMeterType.Icon:
                // For icon type, just change color/alpha
                break;
        }
    }

    /// <summary>
    /// Update color based on sanity level
    /// </summary>
    private void UpdateColor()
    {
        Color targetColor = GetColorForSanity(displaySanity);

        if (fillImage != null)
        {
            fillImage.color = targetColor;
        }

        if (iconImage != null)
        {
            iconImage.color = targetColor;
        }
    }

    /// <summary>
    /// Update text display
    /// </summary>
    private void UpdateText()
    {
        if (sanityText != null && showPercentage)
        {
            sanityText.text = $"{Mathf.RoundToInt(displaySanity)}%";
            sanityText.enabled = true;
        }
        else if (sanityText != null)
        {
            sanityText.enabled = false;
        }
    }

    /// <summary>
    /// Update alpha for subtle high sanity display
    /// </summary>
    private void UpdateAlpha()
    {
        if (!subtleAtHighSanity || canvasGroup == null)
            return;

        float targetAlpha = displaySanity > warningThreshold ? highSanityAlpha : lowSanityAlpha;
        canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetAlpha, Time.deltaTime * 2f);
    }

    #endregion

    #region Visual Effects

    /// <summary>
    /// Get color based on sanity value (gradient)
    /// </summary>
    private Color GetColorForSanity(float sanity)
    {
        if (sanity >= 50f)
        {
            // High to medium (green to yellow)
            float t = (sanity - 50f) / 50f;
            return Color.Lerp(mediumSanityColor, highSanityColor, t);
        }
        else
        {
            // Medium to low (yellow to red)
            float t = sanity / 50f;
            return Color.Lerp(lowSanityColor, mediumSanityColor, t);
        }
    }

    /// <summary>
    /// Update pulsing state based on sanity
    /// </summary>
    private void UpdatePulsingState()
    {
        bool shouldPulse = enablePulsing && targetSanity <= warningThreshold;

        if (shouldPulse && !isPulsing)
        {
            StartPulsing();
        }
        else if (!shouldPulse && isPulsing)
        {
            StopPulsing();
        }
    }

    /// <summary>
    /// Start pulsing animation
    /// </summary>
    private void StartPulsing()
    {
        if (pulseCoroutine != null)
        {
            StopCoroutine(pulseCoroutine);
        }

        isPulsing = true;
        pulseCoroutine = StartCoroutine(PulseCoroutine());

        if (enableDebugLogging)
        {
            Debug.Log("[SanityMeter] Started pulsing");
        }
    }

    /// <summary>
    /// Stop pulsing animation
    /// </summary>
    private void StopPulsing()
    {
        if (pulseCoroutine != null)
        {
            StopCoroutine(pulseCoroutine);
            pulseCoroutine = null;
        }

        isPulsing = false;
        transform.localScale = originalScale;

        if (enableDebugLogging)
        {
            Debug.Log("[SanityMeter] Stopped pulsing");
        }
    }

    /// <summary>
    /// Pulsing animation coroutine
    /// </summary>
    private IEnumerator PulseCoroutine()
    {
        while (isPulsing)
        {
            float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed * Mathf.PI) * pulseIntensity;
            transform.localScale = originalScale * pulse;
            yield return null;
        }
    }

    /// <summary>
    /// Apply shake effect for critical sanity
    /// </summary>
    private void ApplyShake()
    {
        float shakeAmount = shakeIntensity * (1f - (displaySanity / criticalThreshold));
        Vector3 randomOffset = new Vector3(
            Random.Range(-shakeAmount, shakeAmount),
            Random.Range(-shakeAmount, shakeAmount),
            0f
        );
        transform.localPosition = originalPosition + randomOffset;
    }

    #endregion

    #region Public Interface

    /// <summary>
    /// Get current displayed sanity value
    /// </summary>
    public float GetCurrentSanity()
    {
        return displaySanity;
    }

    /// <summary>
    /// Set meter type
    /// </summary>
    public void SetMeterType(SanityMeterType type)
    {
        meterType = type;
        UpdateVisuals();
    }

    /// <summary>
    /// Toggle percentage display
    /// </summary>
    public void SetShowPercentage(bool show)
    {
        showPercentage = show;
        UpdateText();
    }

    #endregion

    #region Debug Methods

    /// <summary>
    /// Test different sanity levels
    /// </summary>
    [ContextMenu("Test High Sanity (100%)")]
    private void TestHighSanity()
    {
        SetSanity(100f);
    }

    [ContextMenu("Test Medium Sanity (50%)")]
    private void TestMediumSanity()
    {
        SetSanity(50f);
    }

    [ContextMenu("Test Low Sanity (25%)")]
    private void TestLowSanity()
    {
        SetSanity(25f);
    }

    [ContextMenu("Test Critical Sanity (5%)")]
    private void TestCriticalSanity()
    {
        SetSanity(5f);
    }

    [ContextMenu("Animate Sanity Loss")]
    private void AnimateSanityLoss()
    {
        StartCoroutine(AnimateSanityCoroutine(100f, 0f, 5f));
    }

    private IEnumerator AnimateSanityCoroutine(float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            SetSanity(Mathf.Lerp(from, to, t));
            yield return null;
        }
        SetSanity(to);
    }

    #endregion
}

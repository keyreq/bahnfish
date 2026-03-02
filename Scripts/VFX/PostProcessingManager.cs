using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;

/// <summary>
/// Agent 13: Visual Effects & Particles Specialist - PostProcessingManager.cs
/// Manages all post-processing effects including bloom, vignette, chromatic aberration, color grading, DOF, and motion blur.
/// Creates cinematic visual polish for the game with quality-based presets.
/// NOTE: This uses placeholder code for Unity's post-processing. In production, use URP/HDRP PostProcessing volumes.
/// </summary>
public class PostProcessingManager : MonoBehaviour
{
    #region Configuration
    [Header("Post-Processing Volumes")]
    [SerializeField] private Volume globalVolume;
    [SerializeField] private bool enablePostProcessing = true;

    [Header("Bloom Settings")]
    [SerializeField] private bool enableBloom = true;
    [SerializeField] private float bloomIntensity = 0.5f;
    [SerializeField] private float bloomThreshold = 1f;

    [Header("Vignette Settings")]
    [SerializeField] private bool enableVignette = false;
    [SerializeField] private float vignetteIntensity = 0f;
    [SerializeField] private float vignetteSmoothness = 0.4f;
    [SerializeField] private Color vignetteColor = Color.black;

    [Header("Chromatic Aberration")]
    [SerializeField] private bool enableChromaticAberration = false;
    [SerializeField] private float chromaticAberrationIntensity = 0f;

    [Header("Color Grading")]
    [SerializeField] private bool enableColorGrading = true;
    [SerializeField] private float colorSaturation = 1f;
    [SerializeField] private float colorContrast = 1f;
    [SerializeField] private float colorExposure = 0f;
    [SerializeField] private Color colorFilter = Color.white;

    [Header("Depth of Field")]
    [SerializeField] private bool enableDepthOfField = false;
    [SerializeField] private float dofFocusDistance = 10f;
    [SerializeField] private float dofAperture = 5.6f;
    [SerializeField] private float dofFocalLength = 50f;

    [Header("Motion Blur")]
    [SerializeField] private bool enableMotionBlur = false;
    [SerializeField] private float motionBlurIntensity = 0.5f;

    [Header("Screen Space Reflections")]
    [SerializeField] private bool enableSSR = true;

    [Header("Ambient Occlusion")]
    [SerializeField] private bool enableAO = true;
    [SerializeField] private float aoIntensity = 1f;
    #endregion

    #region Private Fields
    private VFXQuality currentQuality = VFXQuality.High;
    private TimeOfDay currentTimeOfDay = TimeOfDay.Day;
    private bool isDynamicGradingEnabled = true;

    // Cached values for smooth transitions
    private float targetVignetteIntensity = 0f;
    private float targetChromaticAberration = 0f;
    private float targetSaturation = 1f;

    // Placeholder: In production, these would be references to actual post-processing components
    // from Unity's URP/HDRP PostProcessing stack
    #endregion

    #region Initialization
    private void Start()
    {
        InitializePostProcessing();

        // Subscribe to events
        SubscribeToEvents();

        Debug.Log("[PostProcessingManager] Initialized.");
    }

    /// <summary>
    /// Initializes post-processing volumes and effects.
    /// </summary>
    private void InitializePostProcessing()
    {
        // Create global volume if it doesn't exist
        if (globalVolume == null)
        {
            GameObject volumeObject = new GameObject("GlobalPostProcessVolume");
            volumeObject.transform.SetParent(transform);
            globalVolume = volumeObject.AddComponent<Volume>();
            globalVolume.isGlobal = true;
            globalVolume.priority = 1;
        }

        // Initialize with default quality settings
        ApplyQualityPreset(currentQuality);
    }

    /// <summary>
    /// Subscribes to game events.
    /// </summary>
    private void SubscribeToEvents()
    {
        EventSystem.Subscribe<TimeChangedEventData>("TimeChanged", OnTimeChanged);
        EventSystem.Subscribe<float>("SanityChanged", OnSanityChanged);
        EventSystem.Subscribe<string>("DynamicEventStarted", OnDynamicEventStarted);
        EventSystem.Subscribe<bool>("PhotographyModeToggled", OnPhotographyModeToggled);
    }

    private void OnDestroy()
    {
        EventSystem.Unsubscribe<TimeChangedEventData>("TimeChanged", OnTimeChanged);
        EventSystem.Unsubscribe<float>("SanityChanged", OnSanityChanged);
        EventSystem.Unsubscribe<string>("DynamicEventStarted", OnDynamicEventStarted);
        EventSystem.Unsubscribe<bool>("PhotographyModeToggled", OnPhotographyModeToggled);
    }
    #endregion

    #region Update Loop
    private void Update()
    {
        if (!enablePostProcessing) return;

        // Smooth interpolation for dynamic effects
        InterpolateEffects();

        // Dynamic color grading based on time of day
        if (isDynamicGradingEnabled)
        {
            UpdateDynamicColorGrading();
        }
    }

    /// <summary>
    /// Interpolates post-processing effects smoothly over time.
    /// </summary>
    private void InterpolateEffects()
    {
        float lerpSpeed = 2f * Time.deltaTime;

        vignetteIntensity = Mathf.Lerp(vignetteIntensity, targetVignetteIntensity, lerpSpeed);
        chromaticAberrationIntensity = Mathf.Lerp(chromaticAberrationIntensity, targetChromaticAberration, lerpSpeed);
        colorSaturation = Mathf.Lerp(colorSaturation, targetSaturation, lerpSpeed);

        ApplyEffects();
    }

    /// <summary>
    /// Updates color grading dynamically based on time of day.
    /// </summary>
    private void UpdateDynamicColorGrading()
    {
        switch (currentTimeOfDay)
        {
            case TimeOfDay.Dawn:
                colorFilter = Color.Lerp(colorFilter, new Color(1f, 0.9f, 0.8f), Time.deltaTime * 0.5f);
                colorExposure = Mathf.Lerp(colorExposure, 0.1f, Time.deltaTime * 0.5f);
                break;

            case TimeOfDay.Day:
                colorFilter = Color.Lerp(colorFilter, Color.white, Time.deltaTime * 0.5f);
                colorExposure = Mathf.Lerp(colorExposure, 0f, Time.deltaTime * 0.5f);
                break;

            case TimeOfDay.Dusk:
                colorFilter = Color.Lerp(colorFilter, new Color(1f, 0.7f, 0.5f), Time.deltaTime * 0.5f);
                colorExposure = Mathf.Lerp(colorExposure, -0.1f, Time.deltaTime * 0.5f);
                break;

            case TimeOfDay.Night:
                colorFilter = Color.Lerp(colorFilter, new Color(0.5f, 0.5f, 0.7f), Time.deltaTime * 0.5f);
                colorExposure = Mathf.Lerp(colorExposure, -0.3f, Time.deltaTime * 0.5f);
                break;
        }
    }

    /// <summary>
    /// Applies all post-processing effects.
    /// NOTE: This is placeholder code. In production, update actual Volume Profile components.
    /// </summary>
    private void ApplyEffects()
    {
        if (globalVolume == null || !enablePostProcessing) return;

        // In production, you would do something like:
        // if (globalVolume.profile.TryGet<Bloom>(out var bloom))
        // {
        //     bloom.intensity.value = bloomIntensity;
        //     bloom.threshold.value = bloomThreshold;
        // }

        // For now, we'll use RenderSettings for basic effects
        if (enableBloom)
        {
            // Bloom would be applied via Volume Profile
        }
    }
    #endregion

    #region Quality Presets
    /// <summary>
    /// Sets the post-processing quality level.
    /// </summary>
    public void SetQuality(VFXQuality quality)
    {
        currentQuality = quality;
        ApplyQualityPreset(quality);
        Debug.Log($"[PostProcessingManager] Quality set to {quality}");
    }

    /// <summary>
    /// Applies quality-specific post-processing settings.
    /// </summary>
    private void ApplyQualityPreset(VFXQuality quality)
    {
        switch (quality)
        {
            case VFXQuality.Low:
                enableBloom = false;
                enableSSR = false;
                enableAO = false;
                enableMotionBlur = false;
                enableDepthOfField = false;
                break;

            case VFXQuality.Medium:
                enableBloom = true;
                bloomIntensity = 0.3f;
                enableSSR = false;
                enableAO = true;
                aoIntensity = 0.5f;
                enableMotionBlur = false;
                enableDepthOfField = false;
                break;

            case VFXQuality.High:
                enableBloom = true;
                bloomIntensity = 0.5f;
                enableSSR = true;
                enableAO = true;
                aoIntensity = 1f;
                enableMotionBlur = true;
                motionBlurIntensity = 0.3f;
                enableDepthOfField = false;
                break;

            case VFXQuality.Ultra:
                enableBloom = true;
                bloomIntensity = 0.7f;
                enableSSR = true;
                enableAO = true;
                aoIntensity = 1.5f;
                enableMotionBlur = true;
                motionBlurIntensity = 0.5f;
                enableDepthOfField = true;
                break;
        }

        ApplyEffects();
    }
    #endregion

    #region Public API - Effect Setters
    /// <summary>
    /// Enables or disables all post-processing.
    /// </summary>
    public void SetEnabled(bool enabled)
    {
        enablePostProcessing = enabled;
        if (globalVolume != null)
        {
            globalVolume.enabled = enabled;
        }
    }

    /// <summary>
    /// Sets vignette intensity.
    /// </summary>
    public void SetVignetteIntensity(float intensity)
    {
        targetVignetteIntensity = Mathf.Clamp01(intensity);
        enableVignette = intensity > 0.01f;
    }

    /// <summary>
    /// Sets chromatic aberration intensity.
    /// </summary>
    public void SetChromaticAberration(float intensity)
    {
        targetChromaticAberration = Mathf.Clamp01(intensity);
        enableChromaticAberration = intensity > 0.01f;
    }

    /// <summary>
    /// Sets color saturation.
    /// </summary>
    public void SetColorSaturation(float saturation)
    {
        targetSaturation = Mathf.Clamp(saturation, 0f, 2f);
    }

    /// <summary>
    /// Sets bloom intensity.
    /// </summary>
    public void SetBloomIntensity(float intensity)
    {
        bloomIntensity = Mathf.Clamp(intensity, 0f, 2f);
        ApplyEffects();
    }

    /// <summary>
    /// Sets exposure.
    /// </summary>
    public void SetExposure(float exposure)
    {
        colorExposure = Mathf.Clamp(exposure, -2f, 2f);
    }

    /// <summary>
    /// Sets color filter tint.
    /// </summary>
    public void SetColorFilter(Color color)
    {
        colorFilter = color;
    }

    /// <summary>
    /// Enables depth of field with specified parameters.
    /// </summary>
    public void SetDepthOfField(bool enabled, float focusDistance = 10f, float aperture = 5.6f)
    {
        enableDepthOfField = enabled;
        dofFocusDistance = focusDistance;
        dofAperture = aperture;
        ApplyEffects();
    }
    #endregion

    #region Special Effects
    /// <summary>
    /// Creates a camera flash effect.
    /// </summary>
    public void FlashScreen(Color flashColor, float duration = 0.2f)
    {
        StartCoroutine(FlashRoutine(flashColor, duration));
    }

    /// <summary>
    /// Flash coroutine.
    /// </summary>
    private IEnumerator FlashRoutine(Color flashColor, float duration)
    {
        Color originalFilter = colorFilter;
        float originalExposure = colorExposure;

        colorFilter = flashColor;
        colorExposure = 1f;
        ApplyEffects();

        yield return new WaitForSeconds(duration);

        colorFilter = originalFilter;
        colorExposure = originalExposure;
        ApplyEffects();
    }

    /// <summary>
    /// Applies a damage/hit screen effect.
    /// </summary>
    public void ApplyDamageEffect()
    {
        StartCoroutine(DamageEffectRoutine());
    }

    /// <summary>
    /// Damage effect coroutine.
    /// </summary>
    private IEnumerator DamageEffectRoutine()
    {
        float originalVignette = vignetteIntensity;
        vignetteColor = new Color(1f, 0f, 0f, 1f); // Red
        SetVignetteIntensity(0.8f);

        yield return new WaitForSeconds(0.2f);

        vignetteColor = Color.black;
        SetVignetteIntensity(originalVignette);
    }
    #endregion

    #region Event Handlers
    /// <summary>
    /// Handles time of day changes.
    /// </summary>
    private void OnTimeChanged(TimeChangedEventData data)
    {
        currentTimeOfDay = data.timeOfDay;
    }

    /// <summary>
    /// Handles sanity changes (applied by HorrorVFX).
    /// </summary>
    private void OnSanityChanged(float newSanity)
    {
        // Sanity effects are handled by HorrorVFX which calls our public API methods
    }

    /// <summary>
    /// Handles dynamic event starts (like Blood Moon).
    /// </summary>
    private void OnDynamicEventStarted(string eventName)
    {
        switch (eventName)
        {
            case "BloodMoon":
                isDynamicGradingEnabled = false;
                SetColorFilter(new Color(1f, 0.5f, 0.5f)); // Red tint
                SetBloomIntensity(0.8f);
                break;
        }
    }

    /// <summary>
    /// Handles photography mode toggle.
    /// </summary>
    private void OnPhotographyModeToggled(bool enabled)
    {
        if (enabled)
        {
            // Enable depth of field for photography
            SetDepthOfField(true, 10f, 2.8f);
        }
        else
        {
            // Disable depth of field when exiting photography mode
            SetDepthOfField(false);
        }
    }
    #endregion

    #region Save/Load
    /// <summary>
    /// Gets post-processing settings for saving.
    /// </summary>
    public PostProcessingData GetPostProcessingData()
    {
        return new PostProcessingData
        {
            enabled = enablePostProcessing,
            quality = currentQuality,
            bloomEnabled = enableBloom,
            ssrEnabled = enableSSR,
            aoEnabled = enableAO,
            motionBlurEnabled = enableMotionBlur
        };
    }

    /// <summary>
    /// Loads post-processing settings.
    /// </summary>
    public void LoadPostProcessingData(PostProcessingData data)
    {
        if (data == null) return;

        enablePostProcessing = data.enabled;
        currentQuality = data.quality;
        enableBloom = data.bloomEnabled;
        enableSSR = data.ssrEnabled;
        enableAO = data.aoEnabled;
        enableMotionBlur = data.motionBlurEnabled;

        SetEnabled(enablePostProcessing);
        ApplyQualityPreset(currentQuality);

        Debug.Log("[PostProcessingManager] Loaded post-processing settings.");
    }
    #endregion
}

/// <summary>
/// Post-processing save data.
/// </summary>
[System.Serializable]
public class PostProcessingData
{
    public bool enabled = true;
    public VFXQuality quality = VFXQuality.High;
    public bool bloomEnabled = true;
    public bool ssrEnabled = true;
    public bool aoEnabled = true;
    public bool motionBlurEnabled = true;
}

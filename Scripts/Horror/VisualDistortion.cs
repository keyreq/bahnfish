using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

/// <summary>
/// Manages post-processing visual distortion effects based on sanity.
/// Creates unsettling atmosphere at low sanity.
/// Agent 7: Sanity & Horror System
/// </summary>
public class VisualDistortion : MonoBehaviour
{
    private static VisualDistortion _instance;
    public static VisualDistortion Instance => _instance;

    [Header("Post-Processing")]
    [SerializeField] private PostProcessVolume postProcessVolume;
    [SerializeField] private PostProcessProfile normalProfile;
    [SerializeField] private PostProcessProfile horrorProfile;

    [Header("Chromatic Aberration")]
    [SerializeField] private bool enableChromaticAberration = true;
    [SerializeField] private float maxChromaticAberration = 1f;
    private ChromaticAberration chromaticAberration;

    [Header("Vignette")]
    [SerializeField] private bool enableVignette = true;
    [SerializeField] private float maxVignetteIntensity = 0.5f;
    [SerializeField] private Color vignetteColor = Color.black;
    private Vignette vignette;

    [Header("Color Grading")]
    [SerializeField] private bool enableColorGrading = true;
    [SerializeField] private float minSaturation = -50f;
    [SerializeField] private float maxContrast = 20f;
    [SerializeField] private Color horrorTint = new Color(0.5f, 0.6f, 0.8f);
    private ColorGrading colorGrading;

    [Header("Lens Distortion")]
    [SerializeField] private bool enableLensDistortion = true;
    [SerializeField] private float maxLensDistortion = -30f;
    private LensDistortion lensDistortion;

    [Header("Grain")]
    [SerializeField] private bool enableGrain = true;
    [SerializeField] private float maxGrainIntensity = 0.5f;
    private Grain grain;

    [Header("Screen Shake")]
    [SerializeField] private bool enableScreenShake = true;
    [SerializeField] private float maxShakeIntensity = 0.2f;
    [SerializeField] private float shakeFrequency = 20f;
    private Camera mainCamera;
    private Vector3 originalCameraPosition;

    [Header("Status")]
    [SerializeField] private float currentSanity = 100f;
    [SerializeField] private float effectIntensity = 0f;

    [Header("Pulse Effect")]
    [SerializeField] private bool enablePulse = true;
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float pulseAmount = 0.2f;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogging = false;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        InitializePostProcessing();
        mainCamera = Camera.main;
    }

    private void Start()
    {
        // Subscribe to sanity events
        EventSystem.Subscribe<float>("SanityChanged", OnSanityChanged);

        if (SanityManager.Instance != null)
        {
            currentSanity = SanityManager.Instance.GetCurrentSanity();
            UpdateEffectIntensity();
        }

        if (mainCamera != null)
        {
            originalCameraPosition = mainCamera.transform.localPosition;
        }
    }

    private void InitializePostProcessing()
    {
        if (postProcessVolume == null)
        {
            postProcessVolume = FindObjectOfType<PostProcessVolume>();

            if (postProcessVolume == null)
            {
                if (enableDebugLogging)
                {
                    Debug.LogWarning("[VisualDistortion] No PostProcessVolume found. Creating one.");
                }

                GameObject ppGo = new GameObject("PostProcessVolume");
                postProcessVolume = ppGo.AddComponent<PostProcessVolume>();
                postProcessVolume.isGlobal = true;
                postProcessVolume.priority = 1;
            }
        }

        // Create profile if needed
        if (postProcessVolume.profile == null)
        {
            postProcessVolume.profile = ScriptableObject.CreateInstance<PostProcessProfile>();
        }

        // Get or add effects
        if (!postProcessVolume.profile.TryGetSettings(out chromaticAberration))
        {
            chromaticAberration = postProcessVolume.profile.AddSettings<ChromaticAberration>();
        }

        if (!postProcessVolume.profile.TryGetSettings(out vignette))
        {
            vignette = postProcessVolume.profile.AddSettings<Vignette>();
        }

        if (!postProcessVolume.profile.TryGetSettings(out colorGrading))
        {
            colorGrading = postProcessVolume.profile.AddSettings<ColorGrading>();
        }

        if (!postProcessVolume.profile.TryGetSettings(out lensDistortion))
        {
            lensDistortion = postProcessVolume.profile.AddSettings<LensDistortion>();
        }

        if (!postProcessVolume.profile.TryGetSettings(out grain))
        {
            grain = postProcessVolume.profile.AddSettings<Grain>();
        }

        // Initialize to normal state
        ResetEffects();
    }

    private void Update()
    {
        UpdateVisualEffects();

        if (enableScreenShake && effectIntensity > 0.7f)
        {
            ApplyScreenShake();
        }
        else if (mainCamera != null)
        {
            // Reset camera position
            mainCamera.transform.localPosition = originalCameraPosition;
        }
    }

    private void UpdateVisualEffects()
    {
        // Add pulse effect
        float pulseIntensity = effectIntensity;
        if (enablePulse && effectIntensity > 0.5f)
        {
            float pulse = Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
            pulseIntensity += pulse * (effectIntensity - 0.5f);
            pulseIntensity = Mathf.Clamp01(pulseIntensity);
        }

        // Chromatic Aberration
        if (enableChromaticAberration && chromaticAberration != null)
        {
            chromaticAberration.intensity.value = maxChromaticAberration * pulseIntensity;
            chromaticAberration.enabled.value = pulseIntensity > 0.01f;
        }

        // Vignette
        if (enableVignette && vignette != null)
        {
            vignette.intensity.value = maxVignetteIntensity * effectIntensity;
            vignette.color.value = vignetteColor;
            vignette.enabled.value = effectIntensity > 0.01f;
        }

        // Color Grading
        if (enableColorGrading && colorGrading != null)
        {
            colorGrading.saturation.value = minSaturation * effectIntensity;
            colorGrading.contrast.value = maxContrast * effectIntensity;
            colorGrading.colorFilter.value = Color.Lerp(Color.white, horrorTint, effectIntensity);
            colorGrading.enabled.value = effectIntensity > 0.01f;
        }

        // Lens Distortion
        if (enableLensDistortion && lensDistortion != null)
        {
            lensDistortion.intensity.value = maxLensDistortion * effectIntensity;
            lensDistortion.enabled.value = effectIntensity > 0.2f;
        }

        // Grain
        if (enableGrain && grain != null)
        {
            grain.intensity.value = maxGrainIntensity * effectIntensity;
            grain.enabled.value = effectIntensity > 0.3f;
        }
    }

    private void ApplyScreenShake()
    {
        if (mainCamera == null) return;

        float shakeAmount = maxShakeIntensity * (effectIntensity - 0.7f) / 0.3f;

        float x = Mathf.PerlinNoise(Time.time * shakeFrequency, 0f) * 2f - 1f;
        float y = Mathf.PerlinNoise(0f, Time.time * shakeFrequency) * 2f - 1f;

        Vector3 shakeOffset = new Vector3(x, y, 0f) * shakeAmount;
        mainCamera.transform.localPosition = originalCameraPosition + shakeOffset;
    }

    private void OnSanityChanged(float sanity)
    {
        currentSanity = sanity;
        UpdateEffectIntensity();
    }

    private void UpdateEffectIntensity()
    {
        // Effect intensity is inverse of sanity
        effectIntensity = 1f - (currentSanity / 100f);

        if (enableDebugLogging && effectIntensity > 0.5f)
        {
            Debug.Log($"[VisualDistortion] Effect intensity: {effectIntensity:F2}");
        }
    }

    private void ResetEffects()
    {
        effectIntensity = 0f;

        if (chromaticAberration != null)
        {
            chromaticAberration.intensity.value = 0f;
            chromaticAberration.enabled.value = false;
        }

        if (vignette != null)
        {
            vignette.intensity.value = 0f;
            vignette.enabled.value = false;
        }

        if (colorGrading != null)
        {
            colorGrading.saturation.value = 0f;
            colorGrading.contrast.value = 0f;
            colorGrading.colorFilter.value = Color.white;
            colorGrading.enabled.value = true;
        }

        if (lensDistortion != null)
        {
            lensDistortion.intensity.value = 0f;
            lensDistortion.enabled.value = false;
        }

        if (grain != null)
        {
            grain.intensity.value = 0f;
            grain.enabled.value = false;
        }
    }

    /// <summary>
    /// Get current effect intensity
    /// </summary>
    public float GetEffectIntensity()
    {
        return effectIntensity;
    }

    /// <summary>
    /// Manually trigger a screen shake
    /// </summary>
    public void TriggerScreenShake(float intensity = 1f, float duration = 0.5f)
    {
        if (mainCamera != null)
        {
            StartCoroutine(ScreenShakeCoroutine(intensity, duration));
        }
    }

    private System.Collections.IEnumerator ScreenShakeCoroutine(float intensity, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * intensity * maxShakeIntensity;
            float y = Random.Range(-1f, 1f) * intensity * maxShakeIntensity;

            mainCamera.transform.localPosition = originalCameraPosition + new Vector3(x, y, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        mainCamera.transform.localPosition = originalCameraPosition;
    }

    /// <summary>
    /// Force set effect intensity (for testing)
    /// </summary>
    public void SetEffectIntensity(float intensity)
    {
        effectIntensity = Mathf.Clamp01(intensity);
    }

    /// <summary>
    /// Toggle specific effects on/off
    /// </summary>
    public void SetEffectEnabled(string effectName, bool enabled)
    {
        switch (effectName.ToLower())
        {
            case "chromatic":
                enableChromaticAberration = enabled;
                break;
            case "vignette":
                enableVignette = enabled;
                break;
            case "colorgrading":
                enableColorGrading = enabled;
                break;
            case "lens":
                enableLensDistortion = enabled;
                break;
            case "grain":
                enableGrain = enabled;
                break;
            case "shake":
                enableScreenShake = enabled;
                break;
        }
    }

    private void OnDestroy()
    {
        EventSystem.Unsubscribe<float>("SanityChanged", OnSanityChanged);

        if (_instance == this)
        {
            _instance = null;
        }
    }
}

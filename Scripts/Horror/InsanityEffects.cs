using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

/// <summary>
/// Manages visual and audio distortions based on sanity level.
/// Creates hallucinations and equipment malfunctions.
/// Agent 7: Sanity & Horror System
/// </summary>
public class InsanityEffects : MonoBehaviour
{
    private static InsanityEffects _instance;
    public static InsanityEffects Instance => _instance;

    [Header("References")]
    [SerializeField] private PostProcessVolume postProcessVolume;
    [SerializeField] private AudioSource audioDistortionSource;

    [Header("Visual Effects Configuration")]
    [SerializeField] private bool enableVisualEffects = true;
    [SerializeField] private float chromaticAberrationMax = 1f;
    [SerializeField] private float vignetteMax = 0.5f;
    [SerializeField] private float colorSaturationMin = -50f;
    [SerializeField] private float screenShakeIntensity = 0.1f;

    [Header("Audio Distortion")]
    [SerializeField] private bool enableAudioDistortion = true;
    [SerializeField] private float minPitch = 0.7f;
    [SerializeField] private float maxPitch = 1.3f;
    [SerializeField] private AudioReverbPreset reverbPreset = AudioReverbPreset.Cave;
    [SerializeField] private float reverbIntensity = 0.8f;

    [Header("Hallucination Settings")]
    [SerializeField] private bool enableHallucinations = true;
    [SerializeField] private float hallucinationChance = 0.1f; // 10% per check
    [SerializeField] private float hallucinationCheckInterval = 5f; // Check every 5 seconds
    private float hallucinationTimer = 0f;

    [Header("Equipment Malfunction")]
    [SerializeField] private bool enableMalfunction = true;
    [SerializeField] private float malfunctionChance = 0.05f; // 5% per check
    [SerializeField] private float malfunctionCheckInterval = 10f;
    private float malfunctionTimer = 0f;

    [Header("Status")]
    [SerializeField] private float currentSanity = 100f;
    [SerializeField] private float effectIntensity = 0f; // 0 = no effects, 1 = max effects

    // Post-processing effects
    private ChromaticAberration chromaticAberration;
    private Vignette vignette;
    private ColorGrading colorGrading;

    // Audio effects
    private AudioReverbFilter reverbFilter;
    private AudioDistortionFilter distortionFilter;

    // Camera reference
    private Camera mainCamera;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogging = true;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeEffects();
    }

    private void Start()
    {
        // Subscribe to sanity events
        if (SanityManager.Instance != null)
        {
            SanityManager.Instance.OnSanityChanged += OnSanityChanged;
        }

        EventSystem.Subscribe<float>("SanityChanged", OnSanityChanged);

        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (effectIntensity > 0f)
        {
            UpdateVisualEffects();
            UpdateAudioEffects();
            CheckForHallucinations();
            CheckForMalfunction();
        }
    }

    private void InitializeEffects()
    {
        // Try to find or create post-process volume
        if (postProcessVolume == null)
        {
            postProcessVolume = FindObjectOfType<PostProcessVolume>();

            if (postProcessVolume == null && enableDebugLogging)
            {
                Debug.LogWarning("[InsanityEffects] No PostProcessVolume found. Visual effects will be limited.");
            }
        }

        // Get post-processing effects from profile
        if (postProcessVolume != null && postProcessVolume.profile != null)
        {
            postProcessVolume.profile.TryGetSettings(out chromaticAberration);
            postProcessVolume.profile.TryGetSettings(out vignette);
            postProcessVolume.profile.TryGetSettings(out colorGrading);

            if (enableDebugLogging)
            {
                Debug.Log("[InsanityEffects] Post-processing effects initialized");
            }
        }

        // Setup audio distortion
        if (audioDistortionSource == null)
        {
            audioDistortionSource = gameObject.AddComponent<AudioSource>();
            audioDistortionSource.playOnAwake = false;
            audioDistortionSource.loop = true;
            audioDistortionSource.spatialBlend = 0f; // 2D sound
            audioDistortionSource.volume = 0f;
        }

        reverbFilter = gameObject.GetComponent<AudioReverbFilter>();
        if (reverbFilter == null && enableAudioDistortion)
        {
            reverbFilter = gameObject.AddComponent<AudioReverbFilter>();
            reverbFilter.reverbPreset = AudioReverbPreset.Off;
        }

        distortionFilter = gameObject.GetComponent<AudioDistortionFilter>();
        if (distortionFilter == null && enableAudioDistortion)
        {
            distortionFilter = gameObject.AddComponent<AudioDistortionFilter>();
            distortionFilter.distortionLevel = 0f;
        }
    }

    private void OnSanityChanged(float sanity)
    {
        currentSanity = sanity;

        // Calculate effect intensity (inverse of sanity)
        // 100 sanity = 0 intensity, 0 sanity = 1 intensity
        effectIntensity = 1f - (sanity / 100f);

        if (enableDebugLogging && effectIntensity > 0.5f)
        {
            Debug.Log($"[InsanityEffects] Effect intensity: {effectIntensity:F2}");
        }
    }

    private void UpdateVisualEffects()
    {
        if (!enableVisualEffects) return;

        // Chromatic aberration increases with low sanity
        if (chromaticAberration != null)
        {
            chromaticAberration.intensity.value = chromaticAberrationMax * effectIntensity;
            chromaticAberration.enabled.value = effectIntensity > 0.1f;
        }

        // Vignette darkens edges
        if (vignette != null)
        {
            vignette.intensity.value = vignetteMax * effectIntensity;
            vignette.enabled.value = effectIntensity > 0.1f;
        }

        // Desaturate colors at low sanity
        if (colorGrading != null)
        {
            colorGrading.saturation.value = colorSaturationMin * effectIntensity;
        }

        // Screen shake at very low sanity
        if (effectIntensity > 0.7f && mainCamera != null)
        {
            float shakeAmount = screenShakeIntensity * (effectIntensity - 0.7f) / 0.3f;
            Vector3 shakeOffset = Random.insideUnitSphere * shakeAmount;
            mainCamera.transform.localPosition += shakeOffset * Time.deltaTime;
        }
    }

    private void UpdateAudioEffects()
    {
        if (!enableAudioDistortion) return;

        // Pitch shift based on sanity
        float pitchShift = Mathf.Lerp(maxPitch, minPitch, effectIntensity);
        if (audioDistortionSource != null)
        {
            audioDistortionSource.pitch = pitchShift;
        }

        // Reverb increases with insanity
        if (reverbFilter != null)
        {
            if (effectIntensity > 0.3f)
            {
                reverbFilter.reverbPreset = reverbPreset;
                reverbFilter.reverbLevel = Mathf.Lerp(-10000f, 0f, effectIntensity * reverbIntensity);
            }
            else
            {
                reverbFilter.reverbPreset = AudioReverbPreset.Off;
            }
        }

        // Audio distortion at critical sanity
        if (distortionFilter != null && effectIntensity > 0.5f)
        {
            distortionFilter.distortionLevel = Mathf.Lerp(0f, 0.5f, (effectIntensity - 0.5f) / 0.5f);
        }
        else if (distortionFilter != null)
        {
            distortionFilter.distortionLevel = 0f;
        }
    }

    private void CheckForHallucinations()
    {
        if (!enableHallucinations || effectIntensity < 0.3f) return;

        hallucinationTimer += Time.deltaTime;

        if (hallucinationTimer >= hallucinationCheckInterval)
        {
            hallucinationTimer = 0f;

            // Chance increases with low sanity
            float adjustedChance = hallucinationChance * effectIntensity;

            if (Random.value < adjustedChance)
            {
                TriggerHallucination();
            }
        }
    }

    private void TriggerHallucination()
    {
        int hallucinationType = Random.Range(0, 3);

        switch (hallucinationType)
        {
            case 0:
                TriggerFalseFishSignal();
                break;
            case 1:
                TriggerEquipmentGlitch();
                break;
            case 2:
                TriggerCompassDistortion();
                break;
        }

        EventSystem.Publish("HallucinationTriggered", hallucinationType);

        if (enableDebugLogging)
        {
            Debug.Log($"[InsanityEffects] Hallucination triggered: Type {hallucinationType}");
        }
    }

    private void TriggerFalseFishSignal()
    {
        // Publish false fish detection
        EventSystem.Publish("FalseFishDetected", Random.insideUnitSphere * 20f);

        if (enableDebugLogging)
        {
            Debug.Log("[InsanityEffects] FALSE FISH SIGNAL!");
        }
    }

    private void TriggerEquipmentGlitch()
    {
        // Publish equipment malfunction event
        EventSystem.Publish("EquipmentMalfunction", Random.Range(1f, 3f)); // Duration

        if (enableDebugLogging)
        {
            Debug.Log("[InsanityEffects] EQUIPMENT GLITCH!");
        }
    }

    private void TriggerCompassDistortion()
    {
        // Publish compass distortion event
        EventSystem.Publish("CompassDistorted", Random.Range(30f, 90f)); // Rotation angle

        if (enableDebugLogging)
        {
            Debug.Log("[InsanityEffects] COMPASS DISTORTED!");
        }
    }

    private void CheckForMalfunction()
    {
        if (!enableMalfunction || effectIntensity < 0.5f) return;

        malfunctionTimer += Time.deltaTime;

        if (malfunctionTimer >= malfunctionCheckInterval)
        {
            malfunctionTimer = 0f;

            float adjustedChance = malfunctionChance * (effectIntensity - 0.5f) / 0.5f;

            if (Random.value < adjustedChance)
            {
                TriggerMalfunction();
            }
        }
    }

    private void TriggerMalfunction()
    {
        // Temporary equipment failure
        EventSystem.Publish("EquipmentMalfunction", Random.Range(3f, 8f));

        if (enableDebugLogging)
        {
            Debug.LogWarning("[InsanityEffects] EQUIPMENT MALFUNCTION!");
        }
    }

    /// <summary>
    /// Manually trigger a screen shake effect
    /// </summary>
    public void TriggerScreenShake(float intensity = 1f, float duration = 0.5f)
    {
        StartCoroutine(ScreenShakeCoroutine(intensity, duration));
    }

    private System.Collections.IEnumerator ScreenShakeCoroutine(float intensity, float duration)
    {
        if (mainCamera == null) yield break;

        Vector3 originalPosition = mainCamera.transform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * intensity;
            float y = Random.Range(-1f, 1f) * intensity;

            mainCamera.transform.localPosition = originalPosition + new Vector3(x, y, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        mainCamera.transform.localPosition = originalPosition;
    }

    /// <summary>
    /// Get current effect intensity (0-1)
    /// </summary>
    public float GetEffectIntensity()
    {
        return effectIntensity;
    }

    /// <summary>
    /// Force effect intensity (for testing)
    /// </summary>
    public void SetEffectIntensity(float intensity)
    {
        effectIntensity = Mathf.Clamp01(intensity);
    }

    private void OnDestroy()
    {
        if (SanityManager.Instance != null)
        {
            SanityManager.Instance.OnSanityChanged -= OnSanityChanged;
        }

        EventSystem.Unsubscribe<float>("SanityChanged", OnSanityChanged);

        if (_instance == this)
        {
            _instance = null;
        }
    }
}

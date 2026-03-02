using UnityEngine;

/// <summary>
/// Handles visual transitions for the day/night cycle
/// Manages ambient lighting, fog, and atmospheric effects
/// Uses color palettes from VIDEO_ANALYSIS.md
/// </summary>
[RequireComponent(typeof(TimeManager))]
public class DayNightCycle : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Main directional light (sun/moon)")]
    public Light directionalLight;

    [Header("Day Colors (from VIDEO_ANALYSIS.md)")]
    [SerializeField] private Color daySkyColor = new Color(0.53f, 0.81f, 0.92f); // #87CEEB
    [SerializeField] private Color dayHorizonColor = new Color(0.69f, 0.88f, 0.9f); // #B0E0E6
    [SerializeField] private Color dayFogColor = new Color(0.75f, 0.85f, 0.95f);
    [SerializeField] private Color dayAmbientColor = new Color(0.7f, 0.75f, 0.8f);

    [Header("Dusk/Dawn Colors")]
    [SerializeField] private Color duskSkyTopColor = new Color(0.29f, 0f, 0.51f); // #4B0082 (deep purple)
    [SerializeField] private Color duskSkyBottomColor = new Color(1f, 0.55f, 0f); // #FF8C00 (orange)
    [SerializeField] private Color duskFogColor = new Color(0.55f, 0.35f, 0.5f);
    [SerializeField] private Color duskAmbientColor = new Color(0.5f, 0.4f, 0.5f);

    [Header("Night Colors (from VIDEO_ANALYSIS.md)")]
    [SerializeField] private Color nightSkyColor = new Color(0.1f, 0.1f, 0.18f); // #1a1a2e
    [SerializeField] private Color nightHorizonColor = new Color(0.06f, 0.2f, 0.38f); // #0f3460
    [SerializeField] private Color nightFogColor = new Color(0.09f, 0.13f, 0.24f, 0.6f); // #16213e at 60% opacity
    [SerializeField] private Color nightAmbientColor = new Color(0.15f, 0.2f, 0.3f);

    [Header("Light Intensity Settings")]
    [SerializeField] private float dayLightIntensity = 1.5f;
    [SerializeField] private float duskLightIntensity = 0.8f;
    [SerializeField] private float nightLightIntensity = 0.3f;

    [Header("Fog Settings")]
    [SerializeField] private bool useFog = true;
    [SerializeField] private float dayFogDensity = 0.001f;
    [SerializeField] private float duskFogDensity = 0.005f;
    [SerializeField] private float nightFogDensity = 0.015f;

    [Header("Transition Settings")]
    [Tooltip("How smooth the transitions are (higher = smoother)")]
    [Range(0.1f, 5f)]
    public float transitionSpeed = 1f;

    [Header("Advanced Settings")]
    [SerializeField] private bool updateSkybox = true;
    [SerializeField] private bool updateFog = true;
    [SerializeField] private bool updateAmbientLight = true;
    [SerializeField] private bool rotateSun = true;

    // Private variables
    private TimeManager timeManager;
    private Material skyboxMaterial;

    // Current interpolation values
    private Color currentSkyColor;
    private Color currentHorizonColor;
    private Color currentFogColor;
    private Color currentAmbientColor;
    private float currentLightIntensity;
    private float currentFogDensity;

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        // Get TimeManager reference
        timeManager = TimeManager.Instance;
        if (timeManager == null)
        {
            Debug.LogError("DayNightCycle: TimeManager not found!");
            return;
        }

        // Get or find directional light (sun)
        if (directionalLight == null)
        {
            directionalLight = FindDirectionalLight();
        }

        // Setup skybox material if we're updating it
        if (updateSkybox)
        {
            skyboxMaterial = RenderSettings.skybox;
            if (skyboxMaterial == null)
            {
                Debug.LogWarning("DayNightCycle: No skybox material found. Skybox updates disabled.");
                updateSkybox = false;
            }
        }

        // Enable fog if we're using it
        if (updateFog)
        {
            RenderSettings.fog = useFog;
        }

        // Subscribe to time events
        EventSystem.Subscribe<TimeOfDay>("TimeOfDayChanged", OnTimeOfDayChanged);

        // Set initial state
        UpdateCycleImmediate();

        Debug.Log("DayNightCycle initialized");
    }

    private Light FindDirectionalLight()
    {
        // Find the main directional light in the scene
        Light[] lights = FindObjectsOfType<Light>();
        foreach (Light light in lights)
        {
            if (light.type == LightType.Directional)
            {
                return light;
            }
        }

        // If no directional light found, create one
        GameObject sunObj = new GameObject("Sun (Directional Light)");
        Light sun = sunObj.AddComponent<Light>();
        sun.type = LightType.Directional;
        sun.shadows = LightShadows.Soft;
        Debug.LogWarning("DayNightCycle: No directional light found. Created new one.");
        return sun;
    }

    private void Update()
    {
        if (timeManager != null)
        {
            UpdateCycleSmooth();
        }
    }

    /// <summary>
    /// Smoothly update all visual elements based on current time
    /// </summary>
    private void UpdateCycleSmooth()
    {
        float normalizedTime = timeManager.GetNormalizedTime();
        TimeOfDay currentTimeOfDay = timeManager.GetCurrentTimeOfDay();

        // Get target colors based on time of day
        Color targetSkyColor, targetHorizonColor, targetFogColor, targetAmbientColor;
        float targetIntensity, targetFogDensity;

        GetTargetColorsForTime(currentTimeOfDay, normalizedTime,
            out targetSkyColor, out targetHorizonColor, out targetFogColor,
            out targetAmbientColor, out targetIntensity, out targetFogDensity);

        // Smoothly interpolate to target values
        float lerpSpeed = transitionSpeed * Time.deltaTime;
        currentSkyColor = Color.Lerp(currentSkyColor, targetSkyColor, lerpSpeed);
        currentHorizonColor = Color.Lerp(currentHorizonColor, targetHorizonColor, lerpSpeed);
        currentFogColor = Color.Lerp(currentFogColor, targetFogColor, lerpSpeed);
        currentAmbientColor = Color.Lerp(currentAmbientColor, targetAmbientColor, lerpSpeed);
        currentLightIntensity = Mathf.Lerp(currentLightIntensity, targetIntensity, lerpSpeed);
        currentFogDensity = Mathf.Lerp(currentFogDensity, targetFogDensity, lerpSpeed);

        // Apply values
        ApplyVisualSettings();

        // Rotate sun based on time
        if (rotateSun && directionalLight != null)
        {
            UpdateSunRotation(normalizedTime);
        }
    }

    /// <summary>
    /// Immediately update visual elements (no smooth transition)
    /// </summary>
    private void UpdateCycleImmediate()
    {
        float normalizedTime = timeManager.GetNormalizedTime();
        TimeOfDay currentTimeOfDay = timeManager.GetCurrentTimeOfDay();

        GetTargetColorsForTime(currentTimeOfDay, normalizedTime,
            out currentSkyColor, out currentHorizonColor, out currentFogColor,
            out currentAmbientColor, out currentLightIntensity, out currentFogDensity);

        ApplyVisualSettings();

        if (rotateSun && directionalLight != null)
        {
            UpdateSunRotation(normalizedTime);
        }
    }

    /// <summary>
    /// Determine target colors based on time of day
    /// </summary>
    private void GetTargetColorsForTime(TimeOfDay timeOfDay, float normalizedTime,
        out Color skyColor, out Color horizonColor, out Color fogColor,
        out Color ambientColor, out float intensity, out float fogDensity)
    {
        switch (timeOfDay)
        {
            case TimeOfDay.Day:
                skyColor = daySkyColor;
                horizonColor = dayHorizonColor;
                fogColor = dayFogColor;
                ambientColor = dayAmbientColor;
                intensity = dayLightIntensity;
                fogDensity = dayFogDensity;
                break;

            case TimeOfDay.Dawn:
            case TimeOfDay.Dusk:
                // Blend between day and night colors with orange/purple gradient
                float transitionProgress = GetTransitionProgress(timeOfDay, normalizedTime);

                if (timeOfDay == TimeOfDay.Dawn)
                {
                    // Dawn: Night -> Dusk colors -> Day
                    skyColor = Color.Lerp(nightSkyColor, duskSkyTopColor, transitionProgress * 0.5f);
                    skyColor = Color.Lerp(skyColor, daySkyColor, Mathf.Max(0, (transitionProgress - 0.5f) * 2f));
                    horizonColor = Color.Lerp(nightHorizonColor, duskSkyBottomColor, transitionProgress);
                }
                else // Dusk
                {
                    // Dusk: Day -> Dusk colors -> Night
                    skyColor = Color.Lerp(daySkyColor, duskSkyTopColor, transitionProgress * 0.5f);
                    skyColor = Color.Lerp(skyColor, nightSkyColor, Mathf.Max(0, (transitionProgress - 0.5f) * 2f));
                    horizonColor = Color.Lerp(dayHorizonColor, duskSkyBottomColor, transitionProgress);
                }

                fogColor = duskFogColor;
                ambientColor = duskAmbientColor;
                intensity = duskLightIntensity;
                fogDensity = duskFogDensity;
                break;

            case TimeOfDay.Night:
            default:
                skyColor = nightSkyColor;
                horizonColor = nightHorizonColor;
                fogColor = nightFogColor;
                ambientColor = nightAmbientColor;
                intensity = nightLightIntensity;
                fogDensity = nightFogDensity;
                break;
        }
    }

    /// <summary>
    /// Get transition progress for dawn/dusk (0 to 1)
    /// </summary>
    private float GetTransitionProgress(TimeOfDay period, float normalizedTime)
    {
        float currentHour = timeManager.GetCurrentTime();

        if (period == TimeOfDay.Dawn)
        {
            // Dawn period progress
            float dawnStart = timeManager.dawnStartHour;
            float dawnEnd = timeManager.dayStartHour;
            return Mathf.InverseLerp(dawnStart, dawnEnd, currentHour);
        }
        else // Dusk
        {
            // Dusk period progress
            float duskStart = timeManager.duskStartHour;
            float duskEnd = timeManager.nightStartHour;
            return Mathf.InverseLerp(duskStart, duskEnd, currentHour);
        }
    }

    /// <summary>
    /// Apply all visual settings to the scene
    /// </summary>
    private void ApplyVisualSettings()
    {
        // Update ambient lighting
        if (updateAmbientLight)
        {
            RenderSettings.ambientLight = currentAmbientColor;
        }

        // Update fog
        if (updateFog && useFog)
        {
            RenderSettings.fog = true;
            RenderSettings.fogColor = currentFogColor;
            RenderSettings.fogDensity = currentFogDensity;
        }

        // Update directional light
        if (directionalLight != null)
        {
            directionalLight.color = currentHorizonColor;
            directionalLight.intensity = currentLightIntensity;
        }

        // Update skybox (if using a procedural skybox material)
        if (updateSkybox && skyboxMaterial != null)
        {
            if (skyboxMaterial.HasProperty("_SkyTint"))
            {
                skyboxMaterial.SetColor("_SkyTint", currentSkyColor);
            }
            if (skyboxMaterial.HasProperty("_GroundColor"))
            {
                skyboxMaterial.SetColor("_GroundColor", currentHorizonColor);
            }
        }
    }

    /// <summary>
    /// Rotate the sun based on time of day
    /// </summary>
    private void UpdateSunRotation(float normalizedTime)
    {
        // Rotate sun around the X-axis (0° at sunrise, 180° at sunset)
        // Subtract 90 to start at horizon at midnight
        float sunAngle = (normalizedTime * 360f) - 90f;
        directionalLight.transform.rotation = Quaternion.Euler(sunAngle, 170f, 0f);
    }

    /// <summary>
    /// Called when time of day changes
    /// </summary>
    private void OnTimeOfDayChanged(TimeOfDay newTimeOfDay)
    {
        Debug.Log($"DayNightCycle: Time period changed to {newTimeOfDay}");
        // Could trigger specific effects here (birds at dawn, owls at night, etc.)
    }

    /// <summary>
    /// Public method to force an immediate update
    /// </summary>
    public void ForceUpdate()
    {
        UpdateCycleImmediate();
    }

    /// <summary>
    /// Get current lighting intensity (useful for other systems)
    /// </summary>
    public float GetCurrentLightIntensity()
    {
        return currentLightIntensity;
    }

    /// <summary>
    /// Get current ambient color
    /// </summary>
    public Color GetCurrentAmbientColor()
    {
        return currentAmbientColor;
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        EventSystem.Unsubscribe<TimeOfDay>("TimeOfDayChanged", OnTimeOfDayChanged);
    }
}

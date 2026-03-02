using UnityEngine;

/// <summary>
/// Manages fog density based on time, weather, and sanity.
/// Reduces visibility and creates atmospheric tension.
/// Agent 7: Sanity & Horror System
/// </summary>
public class FogSystem : MonoBehaviour
{
    private static FogSystem _instance;
    public static FogSystem Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<FogSystem>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("FogSystem");
                    _instance = go.AddComponent<FogSystem>();
                }
            }
            return _instance;
        }
    }

    [Header("Fog Configuration")]
    [SerializeField] private bool enableFog = true;
    [SerializeField] private FogMode fogMode = FogMode.ExponentialSquared;
    [SerializeField] private Color fogColor = new Color(0.5f, 0.5f, 0.6f);

    [Header("Time-Based Fog Density")]
    [SerializeField] private float dayFogDensity = 0.0f;
    [SerializeField] private float duskFogDensity = 0.005f;
    [SerializeField] private float nightFogDensity = 0.01f;
    [SerializeField] private float dawnFogDensity = 0.008f;

    [Header("Weather Fog Multipliers")]
    [SerializeField] private float clearMultiplier = 1f;
    [SerializeField] private float rainMultiplier = 2f;
    [SerializeField] private float stormMultiplier = 1.5f;
    [SerializeField] private float fogWeatherMultiplier = 5f; // Heavy fog in fog weather

    [Header("Sanity-Based Fog")]
    [SerializeField] private bool enableSanityFog = true;
    [SerializeField] private float sanityFogThreshold = 30f; // Below 30% sanity
    [SerializeField] private float maxSanityFogMultiplier = 2f;

    [Header("Transition")]
    [SerializeField] private float transitionSpeed = 0.5f;

    [Header("Status")]
    [SerializeField] private float currentFogDensity = 0f;
    [SerializeField] private float targetFogDensity = 0f;
    [SerializeField] private TimeOfDay currentTimeOfDay = TimeOfDay.Day;
    [SerializeField] private WeatherType currentWeather = WeatherType.Clear;
    [SerializeField] private float currentSanity = 100f;

    [Header("Visibility")]
    [SerializeField] private float baseFarClipPlane = 1000f;
    [SerializeField] private float minFarClipPlane = 50f;
    private Camera mainCamera;

    [Header("Color Tints")]
    [SerializeField] private Color dayFogColor = new Color(0.8f, 0.85f, 0.9f);
    [SerializeField] private Color nightFogColor = new Color(0.2f, 0.25f, 0.35f);
    [SerializeField] private Color horrorFogColor = new Color(0.3f, 0.2f, 0.4f);

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogging = false;
    [SerializeField] private bool showFogDebug = false;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        mainCamera = Camera.main;

        // Enable Unity fog
        RenderSettings.fog = enableFog;
        RenderSettings.fogMode = fogMode;
    }

    private void Start()
    {
        // Subscribe to events
        EventSystem.Subscribe<TimeOfDay>("TimeOfDayChanged", OnTimeOfDayChanged);
        EventSystem.Subscribe<WeatherType>("WeatherChanged", OnWeatherChanged);
        EventSystem.Subscribe<float>("SanityChanged", OnSanityChanged);

        // Initialize state
        if (TimeManager.Instance != null)
        {
            currentTimeOfDay = TimeManager.Instance.GetCurrentTimeOfDay();
        }

        if (WeatherSystem.Instance != null)
        {
            currentWeather = WeatherSystem.Instance.GetCurrentWeather();
        }

        if (SanityManager.Instance != null)
        {
            currentSanity = SanityManager.Instance.GetCurrentSanity();
        }

        UpdateTargetFogDensity();

        if (enableDebugLogging)
        {
            Debug.Log("[FogSystem] Initialized");
        }
    }

    private void Update()
    {
        // Smoothly transition fog density
        if (Mathf.Abs(currentFogDensity - targetFogDensity) > 0.0001f)
        {
            currentFogDensity = Mathf.Lerp(currentFogDensity, targetFogDensity, transitionSpeed * Time.deltaTime);
            ApplyFogDensity();
        }

        // Update fog color based on time
        UpdateFogColor();

        // Update camera far clip plane for visibility
        UpdateCameraVisibility();
    }

    private void UpdateTargetFogDensity()
    {
        // Base density from time of day
        float baseDensity = GetTimeFogDensity();

        // Weather multiplier
        float weatherMultiplier = GetWeatherMultiplier();

        // Sanity multiplier
        float sanityMultiplier = GetSanityMultiplier();

        // Calculate target
        targetFogDensity = baseDensity * weatherMultiplier * sanityMultiplier;

        if (enableDebugLogging)
        {
            Debug.Log($"[FogSystem] Target fog density: {targetFogDensity:F4} " +
                     $"(base: {baseDensity:F4}, weather: {weatherMultiplier:F2}, sanity: {sanityMultiplier:F2})");
        }
    }

    private float GetTimeFogDensity()
    {
        switch (currentTimeOfDay)
        {
            case TimeOfDay.Day:
                return dayFogDensity;
            case TimeOfDay.Dusk:
                return duskFogDensity;
            case TimeOfDay.Night:
                return nightFogDensity;
            case TimeOfDay.Dawn:
                return dawnFogDensity;
            default:
                return dayFogDensity;
        }
    }

    private float GetWeatherMultiplier()
    {
        switch (currentWeather)
        {
            case WeatherType.Clear:
                return clearMultiplier;
            case WeatherType.Rain:
                return rainMultiplier;
            case WeatherType.Storm:
                return stormMultiplier;
            case WeatherType.Fog:
                return fogWeatherMultiplier;
            default:
                return clearMultiplier;
        }
    }

    private float GetSanityMultiplier()
    {
        if (!enableSanityFog) return 1f;

        if (currentSanity >= sanityFogThreshold) return 1f;

        // Increase fog as sanity decreases
        float sanityFactor = 1f - (currentSanity / sanityFogThreshold);
        return 1f + (sanityFactor * (maxSanityFogMultiplier - 1f));
    }

    private void ApplyFogDensity()
    {
        if (fogMode == FogMode.Linear)
        {
            RenderSettings.fogStartDistance = 10f;
            RenderSettings.fogEndDistance = 100f / currentFogDensity;
        }
        else
        {
            RenderSettings.fogDensity = currentFogDensity;
        }
    }

    private void UpdateFogColor()
    {
        Color targetColor = dayFogColor;

        // Base color from time of day
        switch (currentTimeOfDay)
        {
            case TimeOfDay.Day:
            case TimeOfDay.Dawn:
                targetColor = dayFogColor;
                break;
            case TimeOfDay.Dusk:
                targetColor = Color.Lerp(dayFogColor, nightFogColor, 0.5f);
                break;
            case TimeOfDay.Night:
                targetColor = nightFogColor;
                break;
        }

        // Tint toward horror color at low sanity
        if (enableSanityFog && currentSanity < sanityFogThreshold)
        {
            float horrorBlend = 1f - (currentSanity / sanityFogThreshold);
            targetColor = Color.Lerp(targetColor, horrorFogColor, horrorBlend * 0.5f);
        }

        // Smooth transition
        RenderSettings.fogColor = Color.Lerp(RenderSettings.fogColor, targetColor, transitionSpeed * Time.deltaTime);
    }

    private void UpdateCameraVisibility()
    {
        if (mainCamera == null) return;

        // Reduce far clip plane in heavy fog
        float visibilityFactor = 1f - Mathf.Clamp01(currentFogDensity * 50f);
        float targetFarClip = Mathf.Lerp(minFarClipPlane, baseFarClipPlane, visibilityFactor);

        mainCamera.farClipPlane = Mathf.Lerp(mainCamera.farClipPlane, targetFarClip, transitionSpeed * Time.deltaTime);
    }

    private void OnTimeOfDayChanged(TimeOfDay newTime)
    {
        currentTimeOfDay = newTime;
        UpdateTargetFogDensity();

        if (enableDebugLogging)
        {
            Debug.Log($"[FogSystem] Time changed to {newTime}. Updating fog.");
        }
    }

    private void OnWeatherChanged(WeatherType newWeather)
    {
        currentWeather = newWeather;
        UpdateTargetFogDensity();

        if (enableDebugLogging)
        {
            Debug.Log($"[FogSystem] Weather changed to {newWeather}. Updating fog.");
        }
    }

    private void OnSanityChanged(float sanity)
    {
        currentSanity = sanity;

        if (enableSanityFog)
        {
            UpdateTargetFogDensity();
        }
    }

    /// <summary>
    /// Get current fog density
    /// </summary>
    public float GetCurrentFogDensity()
    {
        return currentFogDensity;
    }

    /// <summary>
    /// Get current visibility range (approximate)
    /// </summary>
    public float GetVisibilityRange()
    {
        if (currentFogDensity <= 0.0001f) return baseFarClipPlane;

        // Approximate visibility based on fog density
        return Mathf.Clamp(100f / currentFogDensity, minFarClipPlane, baseFarClipPlane);
    }

    /// <summary>
    /// Check if visibility is reduced
    /// </summary>
    public bool IsReducedVisibility()
    {
        return currentFogDensity > 0.005f;
    }

    /// <summary>
    /// Enable or disable fog system
    /// </summary>
    public void SetFogEnabled(bool enabled)
    {
        enableFog = enabled;
        RenderSettings.fog = enabled;

        if (enableDebugLogging)
        {
            Debug.Log($"[FogSystem] Fog {(enabled ? "enabled" : "disabled")}");
        }
    }

    /// <summary>
    /// Set fog density manually (for testing)
    /// </summary>
    public void SetFogDensity(float density)
    {
        targetFogDensity = density;
        currentFogDensity = density;
        ApplyFogDensity();
    }

    private void OnGUI()
    {
        if (!showFogDebug) return;

        GUILayout.BeginArea(new Rect(10, 200, 300, 200));
        GUILayout.Box("=== FOG SYSTEM DEBUG ===");
        GUILayout.Label($"Current Density: {currentFogDensity:F4}");
        GUILayout.Label($"Target Density: {targetFogDensity:F4}");
        GUILayout.Label($"Visibility Range: {GetVisibilityRange():F1}m");
        GUILayout.Label($"Time: {currentTimeOfDay}");
        GUILayout.Label($"Weather: {currentWeather}");
        GUILayout.Label($"Sanity: {currentSanity:F1}%");
        GUILayout.Label($"Fog Color: {RenderSettings.fogColor}");
        GUILayout.EndArea();
    }

    private void OnDestroy()
    {
        EventSystem.Unsubscribe<TimeOfDay>("TimeOfDayChanged", OnTimeOfDayChanged);
        EventSystem.Unsubscribe<WeatherType>("WeatherChanged", OnWeatherChanged);
        EventSystem.Unsubscribe<float>("SanityChanged", OnSanityChanged);

        if (_instance == this)
        {
            _instance = null;
        }
    }
}

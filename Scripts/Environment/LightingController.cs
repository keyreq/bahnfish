using UnityEngine;

/// <summary>
/// Advanced lighting controller that manages sun position, intensity curves, and color gradients
/// Works in conjunction with DayNightCycle for enhanced visual quality
/// Uses VIDEO_ANALYSIS.md color specifications
/// </summary>
public class LightingController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Main directional light (sun)")]
    public Light sunLight;

    [Tooltip("Optional moon light for nighttime")]
    public Light moonLight;

    [Header("Sun Rotation Settings")]
    [Tooltip("Angle offset for sun rotation (adjust based on your scene)")]
    [Range(-180f, 180f)]
    public float sunRotationOffset = -90f;

    [Tooltip("Sun Y-axis rotation (determines east/west direction)")]
    [Range(0f, 360f)]
    public float sunYRotation = 170f;

    [Header("Light Intensity Curves")]
    [Tooltip("Light intensity curve over 24 hours (X=normalized time 0-1, Y=intensity)")]
    public AnimationCurve intensityCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1.5f);

    [Tooltip("Moon light intensity multiplier")]
    [Range(0f, 1f)]
    public float moonIntensityMultiplier = 0.3f;

    [Header("Color Gradients")]
    [Tooltip("Light color gradient over 24-hour cycle")]
    public Gradient lightColorGradient;

    [Header("Shadow Settings")]
    [Tooltip("Enable dynamic shadow distance based on time")]
    public bool dynamicShadowDistance = true;

    [Range(10f, 500f)]
    public float dayShadowDistance = 200f;

    [Range(10f, 200f)]
    public float nightShadowDistance = 50f;

    [Header("Advanced Settings")]
    [Tooltip("Enable lens flare during day")]
    public bool useLensFlare = true;

    [Tooltip("Lens flare intensity curve")]
    public AnimationCurve lensFlareCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Tooltip("Enable light bounces (GI)")]
    public bool enableGlobalIllumination = true;

    [Range(0f, 5f)]
    public float giIntensityMultiplier = 1f;

    // Private variables
    private TimeManager timeManager;
    private WeatherSystem weatherSystem;
    private LensFlare sunLensFlare;
    private float currentIntensity;
    private Color currentLightColor;

    // Cached values
    private bool hasWeatherSystem = false;

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
            Debug.LogError("LightingController: TimeManager not found!");
            return;
        }

        // Get WeatherSystem reference (optional)
        weatherSystem = WeatherSystem.Instance;
        hasWeatherSystem = (weatherSystem != null);

        // Setup sun light
        if (sunLight == null)
        {
            sunLight = FindSunLight();
        }

        // Setup lens flare
        if (useLensFlare && sunLight != null)
        {
            sunLensFlare = sunLight.GetComponent<LensFlare>();
            if (sunLensFlare == null)
            {
                sunLensFlare = sunLight.gameObject.AddComponent<LensFlare>();
            }
        }

        // Setup moon light if provided
        if (moonLight != null)
        {
            moonLight.enabled = false; // Start disabled
        }

        // Initialize default gradient if not set
        if (lightColorGradient == null || lightColorGradient.colorKeys.Length == 0)
        {
            SetupDefaultGradient();
        }

        // Initialize default intensity curve if not set
        if (intensityCurve == null || intensityCurve.length == 0)
        {
            SetupDefaultIntensityCurve();
        }

        // Setup global illumination
        if (enableGlobalIllumination)
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
            RenderSettings.ambientIntensity = giIntensityMultiplier;
        }

        Debug.Log("LightingController initialized");
    }

    private Light FindSunLight()
    {
        Light[] lights = FindObjectsOfType<Light>();
        foreach (Light light in lights)
        {
            if (light.type == LightType.Directional)
            {
                return light;
            }
        }

        // Create sun if not found
        GameObject sunObj = new GameObject("Sun");
        Light sun = sunObj.AddComponent<Light>();
        sun.type = LightType.Directional;
        sun.shadows = LightShadows.Soft;
        Debug.LogWarning("LightingController: No sun light found. Created new one.");
        return sun;
    }

    private void SetupDefaultGradient()
    {
        lightColorGradient = new Gradient();

        // Setup gradient keys based on VIDEO_ANALYSIS.md
        GradientColorKey[] colorKeys = new GradientColorKey[6];
        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];

        // Midnight - deep blue
        colorKeys[0] = new GradientColorKey(new Color(0.15f, 0.2f, 0.3f), 0f);

        // Dawn - orange/pink
        colorKeys[1] = new GradientColorKey(new Color(1f, 0.6f, 0.4f), 0.25f);

        // Midday - bright white/yellow
        colorKeys[2] = new GradientColorKey(new Color(1f, 0.95f, 0.85f), 0.5f);

        // Dusk - orange/purple
        colorKeys[3] = new GradientColorKey(new Color(1f, 0.5f, 0.3f), 0.75f);

        // Night - deep blue
        colorKeys[4] = new GradientColorKey(new Color(0.2f, 0.25f, 0.4f), 0.85f);

        // Midnight again
        colorKeys[5] = new GradientColorKey(new Color(0.15f, 0.2f, 0.3f), 1f);

        alphaKeys[0] = new GradientAlphaKey(1f, 0f);
        alphaKeys[1] = new GradientAlphaKey(1f, 1f);

        lightColorGradient.SetKeys(colorKeys, alphaKeys);
    }

    private void SetupDefaultIntensityCurve()
    {
        intensityCurve = new AnimationCurve();

        // Midnight - very low
        intensityCurve.AddKey(0f, 0.1f);

        // Dawn - rising
        intensityCurve.AddKey(0.25f, 0.8f);

        // Midday - peak
        intensityCurve.AddKey(0.5f, 1.5f);

        // Dusk - lowering
        intensityCurve.AddKey(0.75f, 0.6f);

        // Night - low
        intensityCurve.AddKey(0.85f, 0.2f);

        // Midnight - very low
        intensityCurve.AddKey(1f, 0.1f);

        // Smooth the curve
        for (int i = 0; i < intensityCurve.length; i++)
        {
            intensityCurve.SmoothTangents(i, 0.5f);
        }
    }

    private void Update()
    {
        if (timeManager != null)
        {
            UpdateLighting();
        }
    }

    /// <summary>
    /// Main lighting update loop
    /// </summary>
    private void UpdateLighting()
    {
        float normalizedTime = timeManager.GetNormalizedTime();

        // Update sun rotation
        UpdateSunRotation(normalizedTime);

        // Update light intensity
        UpdateLightIntensity(normalizedTime);

        // Update light color
        UpdateLightColor(normalizedTime);

        // Update shadows
        if (dynamicShadowDistance)
        {
            UpdateShadowDistance(normalizedTime);
        }

        // Update lens flare
        if (useLensFlare && sunLensFlare != null)
        {
            UpdateLensFlare(normalizedTime);
        }

        // Update moon (if present)
        if (moonLight != null)
        {
            UpdateMoonLight(normalizedTime);
        }
    }

    /// <summary>
    /// Update sun rotation based on time of day
    /// </summary>
    private void UpdateSunRotation(float normalizedTime)
    {
        if (sunLight == null) return;

        // Calculate sun angle (0° at sunrise, 180° at sunset)
        float sunAngle = (normalizedTime * 360f) + sunRotationOffset;

        // Apply rotation
        sunLight.transform.rotation = Quaternion.Euler(sunAngle, sunYRotation, 0f);
    }

    /// <summary>
    /// Update light intensity based on time and weather
    /// </summary>
    private void UpdateLightIntensity(float normalizedTime)
    {
        if (sunLight == null) return;

        // Get base intensity from curve
        currentIntensity = intensityCurve.Evaluate(normalizedTime);

        // Apply weather multiplier if weather system exists
        if (hasWeatherSystem)
        {
            float weatherMultiplier = GetWeatherIntensityMultiplier();
            currentIntensity *= weatherMultiplier;
        }

        // Apply to sun light
        sunLight.intensity = currentIntensity;
    }

    /// <summary>
    /// Update light color based on time gradient
    /// </summary>
    private void UpdateLightColor(float normalizedTime)
    {
        if (sunLight == null) return;

        // Get color from gradient
        currentLightColor = lightColorGradient.Evaluate(normalizedTime);

        // Apply weather tint if weather system exists
        if (hasWeatherSystem)
        {
            Color weatherTint = GetWeatherColorTint();
            currentLightColor *= weatherTint;
        }

        // Apply to sun light
        sunLight.color = currentLightColor;
    }

    /// <summary>
    /// Update shadow distance based on time
    /// </summary>
    private void UpdateShadowDistance(float normalizedTime)
    {
        // More shadows during day, fewer at night (performance optimization)
        float shadowDistance = Mathf.Lerp(nightShadowDistance, dayShadowDistance,
            Mathf.Sin(normalizedTime * Mathf.PI));

        QualitySettings.shadowDistance = shadowDistance;
    }

    /// <summary>
    /// Update lens flare intensity
    /// </summary>
    private void UpdateLensFlare(float normalizedTime)
    {
        if (sunLensFlare == null) return;

        // Lens flare only visible when sun is above horizon
        float flareIntensity = lensFlareCurve.Evaluate(normalizedTime);

        // Reduce flare in bad weather
        if (hasWeatherSystem)
        {
            if (weatherSystem.IsRaining() || weatherSystem.IsFoggy())
            {
                flareIntensity *= 0.2f;
            }
        }

        sunLensFlare.brightness = flareIntensity;
    }

    /// <summary>
    /// Update moon light (opposite of sun)
    /// </summary>
    private void UpdateMoonLight(float normalizedTime)
    {
        // Moon is opposite the sun
        float moonAngle = (normalizedTime * 360f) + sunRotationOffset + 180f;
        moonLight.transform.rotation = Quaternion.Euler(moonAngle, sunYRotation + 180f, 0f);

        // Moon intensity (inverse of sun, with multiplier)
        float moonIntensity = (1f - intensityCurve.Evaluate(normalizedTime)) * moonIntensityMultiplier;

        // Enable/disable moon based on intensity
        if (moonIntensity > 0.05f)
        {
            moonLight.enabled = true;
            moonLight.intensity = moonIntensity;
        }
        else
        {
            moonLight.enabled = false;
        }
    }

    /// <summary>
    /// Get intensity multiplier based on weather
    /// </summary>
    private float GetWeatherIntensityMultiplier()
    {
        if (!hasWeatherSystem) return 1f;

        WeatherType weather = weatherSystem.GetCurrentWeather();
        float intensity = weatherSystem.GetWeatherIntensity();

        switch (weather)
        {
            case WeatherType.Clear:
                return 1f;
            case WeatherType.Rain:
                return Mathf.Lerp(1f, 0.7f, intensity);
            case WeatherType.Storm:
                return Mathf.Lerp(1f, 0.5f, intensity);
            case WeatherType.Fog:
                return Mathf.Lerp(1f, 0.8f, intensity);
            default:
                return 1f;
        }
    }

    /// <summary>
    /// Get color tint based on weather
    /// </summary>
    private Color GetWeatherColorTint()
    {
        if (!hasWeatherSystem) return Color.white;

        WeatherType weather = weatherSystem.GetCurrentWeather();
        float intensity = weatherSystem.GetWeatherIntensity();

        switch (weather)
        {
            case WeatherType.Clear:
                return Color.white;
            case WeatherType.Rain:
                return Color.Lerp(Color.white, new Color(0.8f, 0.85f, 0.9f), intensity);
            case WeatherType.Storm:
                return Color.Lerp(Color.white, new Color(0.6f, 0.65f, 0.7f), intensity);
            case WeatherType.Fog:
                return Color.Lerp(Color.white, new Color(0.9f, 0.9f, 0.95f), intensity);
            default:
                return Color.white;
        }
    }

    #region Public Interface

    /// <summary>
    /// Get current light intensity
    /// </summary>
    public float GetCurrentIntensity()
    {
        return currentIntensity;
    }

    /// <summary>
    /// Get current light color
    /// </summary>
    public Color GetCurrentLightColor()
    {
        return currentLightColor;
    }

    /// <summary>
    /// Get sun direction (useful for shaders and effects)
    /// </summary>
    public Vector3 GetSunDirection()
    {
        if (sunLight != null)
            return -sunLight.transform.forward;
        return Vector3.down;
    }

    /// <summary>
    /// Force update lighting (useful after changing settings)
    /// </summary>
    public void ForceUpdate()
    {
        if (timeManager != null)
        {
            UpdateLighting();
        }
    }

    #endregion
}

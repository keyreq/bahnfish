using UnityEngine;
using System.Collections;

/// <summary>
/// Agent 13: Visual Effects & Particles Specialist - WeatherParticles.cs
/// Manages all weather particle effects including rain, snow, fog, lightning, and wind debris.
/// Creates immersive atmospheric conditions that affect gameplay visibility and mood.
/// </summary>
public class WeatherParticles : MonoBehaviour
{
    #region Configuration
    [Header("Rain Settings")]
    [SerializeField] private GameObject rainPrefab;
    [SerializeField] private ParticleSystem rainParticles;
    [SerializeField] private float lightRainDensity = 0.2f;
    [SerializeField] private float mediumRainDensity = 0.6f;
    [SerializeField] private float heavyRainDensity = 1.0f;

    [Header("Snow Settings")]
    [SerializeField] private GameObject snowPrefab;
    [SerializeField] private ParticleSystem snowParticles;
    [SerializeField] private float gentleSnowDensity = 0.3f;
    [SerializeField] private float blizzardDensity = 1.0f;

    [Header("Fog Settings")]
    [SerializeField] private bool useFog = true;
    [SerializeField] private Color fogColorDay = new Color(0.7f, 0.7f, 0.7f);
    [SerializeField] private Color fogColorNight = new Color(0.2f, 0.2f, 0.3f);
    [SerializeField] private float fogDensityClear = 0.001f;
    [SerializeField] private float fogDensityFog = 0.05f;

    [Header("Storm Effects")]
    [SerializeField] private GameObject lightningPrefab;
    [SerializeField] private float lightningMinInterval = 3f;
    [SerializeField] private float lightningMaxInterval = 8f;
    [SerializeField] private float lightningFlashDuration = 0.1f;
    [SerializeField] private Color lightningFlashColor = Color.white;

    [Header("Wind Effects")]
    [SerializeField] private GameObject windDebrisPrefab;
    [SerializeField] private ParticleSystem windParticles;
    [SerializeField] private float windStrength = 1f;

    [Header("Mist Effects")]
    [SerializeField] private GameObject mistPrefab;
    [SerializeField] private ParticleSystem mistParticles;
    #endregion

    #region Private Fields
    private VFXQuality currentQuality = VFXQuality.High;
    private WeatherType currentWeather = WeatherType.Clear;
    private WeatherType targetWeather = WeatherType.Clear;
    private float transitionProgress = 1f;
    private float transitionDuration = 10f;
    private VFXManager vfxManager;
    private bool isLightningActive = false;
    private Coroutine lightningCoroutine;
    private Light directionalLight;
    private Camera mainCamera;
    #endregion

    #region Initialization
    private void Start()
    {
        vfxManager = VFXManager.Instance;
        mainCamera = Camera.main;
        directionalLight = FindObjectOfType<Light>();

        // Subscribe to events
        SubscribeToEvents();

        // Register particle prefabs
        RegisterParticlePrefabs();

        // Setup fog
        SetupFog();

        Debug.Log("[WeatherParticles] Initialized.");
    }

    /// <summary>
    /// Subscribes to game events.
    /// </summary>
    private void SubscribeToEvents()
    {
        EventSystem.Subscribe<WeatherType>("WeatherChanged", OnWeatherChanged);
        EventSystem.Subscribe<TimeChangedEventData>("TimeChanged", OnTimeChanged);
    }

    /// <summary>
    /// Registers particle prefabs with the VFX Manager.
    /// </summary>
    private void RegisterParticlePrefabs()
    {
        if (vfxManager == null) return;

        if (rainPrefab != null) vfxManager.RegisterParticlePrefab("weather_rain", rainPrefab);
        if (snowPrefab != null) vfxManager.RegisterParticlePrefab("weather_snow", snowPrefab);
        if (lightningPrefab != null) vfxManager.RegisterParticlePrefab("weather_lightning", lightningPrefab);
        if (windDebrisPrefab != null) vfxManager.RegisterParticlePrefab("weather_wind_debris", windDebrisPrefab);
        if (mistPrefab != null) vfxManager.RegisterParticlePrefab("weather_mist", mistPrefab);
    }

    /// <summary>
    /// Sets up fog rendering.
    /// </summary>
    private void SetupFog()
    {
        if (useFog)
        {
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = fogDensityClear;
            RenderSettings.fogColor = fogColorDay;
        }
    }

    private void OnDestroy()
    {
        EventSystem.Unsubscribe<WeatherType>("WeatherChanged", OnWeatherChanged);
        EventSystem.Unsubscribe<TimeChangedEventData>("TimeChanged", OnTimeChanged);
    }
    #endregion

    #region Update Loop
    private void Update()
    {
        UpdateWeatherTransition();
        UpdateParticlePositions();
    }

    /// <summary>
    /// Updates weather transition between states.
    /// </summary>
    private void UpdateWeatherTransition()
    {
        if (transitionProgress < 1f)
        {
            transitionProgress += Time.deltaTime / transitionDuration;
            transitionProgress = Mathf.Clamp01(transitionProgress);

            // Interpolate particle intensities
            UpdateParticleIntensities(transitionProgress);
        }
    }

    /// <summary>
    /// Updates particle system positions to follow the player.
    /// </summary>
    private void UpdateParticlePositions()
    {
        if (mainCamera == null) return;

        Vector3 cameraPos = mainCamera.transform.position;

        if (rainParticles != null && rainParticles.gameObject.activeSelf)
        {
            rainParticles.transform.position = new Vector3(cameraPos.x, cameraPos.y + 20f, cameraPos.z);
        }

        if (snowParticles != null && snowParticles.gameObject.activeSelf)
        {
            snowParticles.transform.position = new Vector3(cameraPos.x, cameraPos.y + 20f, cameraPos.z);
        }

        if (mistParticles != null && mistParticles.gameObject.activeSelf)
        {
            mistParticles.transform.position = new Vector3(cameraPos.x, cameraPos.y, cameraPos.z);
        }

        if (windParticles != null && windParticles.gameObject.activeSelf)
        {
            windParticles.transform.position = new Vector3(cameraPos.x, cameraPos.y, cameraPos.z);
        }
    }
    #endregion

    #region Rain Effects
    /// <summary>
    /// Starts rain particles with specified intensity.
    /// </summary>
    private void StartRain(RainIntensity intensity)
    {
        if (rainParticles == null) return;

        rainParticles.gameObject.SetActive(true);

        var emission = rainParticles.emission;
        float densityMultiplier = GetDensityMultiplier();

        switch (intensity)
        {
            case RainIntensity.Light:
                emission.rateOverTime = 500f * lightRainDensity * densityMultiplier;
                break;
            case RainIntensity.Medium:
                emission.rateOverTime = 1000f * mediumRainDensity * densityMultiplier;
                break;
            case RainIntensity.Heavy:
                emission.rateOverTime = 2000f * heavyRainDensity * densityMultiplier;
                break;
        }

        rainParticles.Play();

        // Start mist for medium/heavy rain
        if (intensity >= RainIntensity.Medium && currentQuality >= VFXQuality.Medium)
        {
            StartMist(0.5f);
        }

        Debug.Log($"[WeatherParticles] Started {intensity} rain");
    }

    /// <summary>
    /// Stops rain particles.
    /// </summary>
    private void StopRain()
    {
        if (rainParticles != null)
        {
            rainParticles.Stop();
        }
        StopMist();
    }
    #endregion

    #region Snow Effects
    /// <summary>
    /// Starts snow particles.
    /// </summary>
    private void StartSnow(bool blizzard = false)
    {
        if (snowParticles == null) return;

        snowParticles.gameObject.SetActive(true);

        var emission = snowParticles.emission;
        float densityMultiplier = GetDensityMultiplier();

        if (blizzard)
        {
            emission.rateOverTime = 1500f * blizzardDensity * densityMultiplier;

            // Add wind
            if (windParticles != null && currentQuality >= VFXQuality.High)
            {
                StartWind(2f);
            }
        }
        else
        {
            emission.rateOverTime = 500f * gentleSnowDensity * densityMultiplier;
        }

        snowParticles.Play();

        Debug.Log($"[WeatherParticles] Started {(blizzard ? "blizzard" : "snow")}");
    }

    /// <summary>
    /// Stops snow particles.
    /// </summary>
    private void StopSnow()
    {
        if (snowParticles != null)
        {
            snowParticles.Stop();
        }
        StopWind();
    }
    #endregion

    #region Storm Effects
    /// <summary>
    /// Starts storm effects including heavy rain and lightning.
    /// </summary>
    private void StartStorm()
    {
        StartRain(RainIntensity.Heavy);
        StartLightning();
        StartWind(1.5f);
        UpdateFog(fogDensityFog * 0.5f);

        Debug.Log("[WeatherParticles] Started storm");
    }

    /// <summary>
    /// Stops storm effects.
    /// </summary>
    private void StopStorm()
    {
        StopRain();
        StopLightning();
        StopWind();
    }

    /// <summary>
    /// Starts lightning effects.
    /// </summary>
    private void StartLightning()
    {
        if (isLightningActive) return;
        if (currentQuality < VFXQuality.Medium) return;

        isLightningActive = true;
        lightningCoroutine = StartCoroutine(LightningRoutine());
    }

    /// <summary>
    /// Stops lightning effects.
    /// </summary>
    private void StopLightning()
    {
        if (!isLightningActive) return;

        isLightningActive = false;
        if (lightningCoroutine != null)
        {
            StopCoroutine(lightningCoroutine);
            lightningCoroutine = null;
        }
    }

    /// <summary>
    /// Lightning routine that flashes periodically.
    /// </summary>
    private IEnumerator LightningRoutine()
    {
        while (isLightningActive)
        {
            // Wait random interval
            float interval = Random.Range(lightningMinInterval, lightningMaxInterval);
            yield return new WaitForSeconds(interval);

            // Lightning flash
            if (isLightningActive)
            {
                yield return StartCoroutine(LightningFlash());
            }
        }
    }

    /// <summary>
    /// Creates a single lightning flash.
    /// </summary>
    private IEnumerator LightningFlash()
    {
        // Store original light intensity
        float originalIntensity = directionalLight != null ? directionalLight.intensity : 1f;
        Color originalAmbient = RenderSettings.ambientLight;

        // Flash
        if (directionalLight != null)
        {
            directionalLight.intensity = originalIntensity * 3f;
        }
        RenderSettings.ambientLight = lightningFlashColor;

        // Spawn lightning VFX
        if (mainCamera != null && currentQuality >= VFXQuality.High)
        {
            Vector3 lightningPos = mainCamera.transform.position + mainCamera.transform.forward * 50f;
            lightningPos.y = 30f;
            vfxManager.SpawnEffect("weather_lightning", lightningPos);
        }

        // Publish event for thunder sound
        EventSystem.Publish("LightningFlash", mainCamera != null ? mainCamera.transform.position : Vector3.zero);

        yield return new WaitForSeconds(lightningFlashDuration);

        // Restore original lighting
        if (directionalLight != null)
        {
            directionalLight.intensity = originalIntensity;
        }
        RenderSettings.ambientLight = originalAmbient;
    }
    #endregion

    #region Fog Effects
    /// <summary>
    /// Updates fog settings.
    /// </summary>
    private void UpdateFog(float targetDensity)
    {
        if (!useFog) return;

        RenderSettings.fogDensity = Mathf.Lerp(RenderSettings.fogDensity, targetDensity, Time.deltaTime * 0.5f);
    }

    /// <summary>
    /// Starts heavy fog.
    /// </summary>
    private void StartFog()
    {
        UpdateFog(fogDensityFog);
        Debug.Log("[WeatherParticles] Started fog");
    }

    /// <summary>
    /// Clears fog.
    /// </summary>
    private void StopFog()
    {
        UpdateFog(fogDensityClear);
    }
    #endregion

    #region Wind & Mist Effects
    /// <summary>
    /// Starts wind particle effects.
    /// </summary>
    private void StartWind(float strength)
    {
        if (windParticles == null || currentQuality < VFXQuality.High) return;

        windParticles.gameObject.SetActive(true);

        var emission = windParticles.emission;
        emission.rateOverTime = 200f * strength * GetDensityMultiplier();

        windParticles.Play();

        Debug.Log($"[WeatherParticles] Started wind (strength: {strength})");
    }

    /// <summary>
    /// Stops wind particles.
    /// </summary>
    private void StopWind()
    {
        if (windParticles != null)
        {
            windParticles.Stop();
        }
    }

    /// <summary>
    /// Starts mist particles.
    /// </summary>
    private void StartMist(float density)
    {
        if (mistParticles == null || currentQuality < VFXQuality.Medium) return;

        mistParticles.gameObject.SetActive(true);

        var emission = mistParticles.emission;
        emission.rateOverTime = 100f * density * GetDensityMultiplier();

        mistParticles.Play();
    }

    /// <summary>
    /// Stops mist particles.
    /// </summary>
    private void StopMist()
    {
        if (mistParticles != null)
        {
            mistParticles.Stop();
        }
    }
    #endregion

    #region Weather Transitions
    /// <summary>
    /// Transitions to a new weather type.
    /// </summary>
    private void TransitionWeather(WeatherType newWeather)
    {
        if (currentWeather == newWeather) return;

        targetWeather = newWeather;
        transitionProgress = 0f;

        // Stop old weather
        StopCurrentWeather();

        // Start new weather
        StartWeatherType(newWeather);

        currentWeather = newWeather;
    }

    /// <summary>
    /// Stops all current weather effects.
    /// </summary>
    private void StopCurrentWeather()
    {
        StopRain();
        StopSnow();
        StopStorm();
        StopFog();
        StopWind();
        StopMist();
    }

    /// <summary>
    /// Starts weather effects for a specific type.
    /// </summary>
    private void StartWeatherType(WeatherType weather)
    {
        switch (weather)
        {
            case WeatherType.Clear:
                StopFog();
                break;

            case WeatherType.Rain:
                StartRain(RainIntensity.Medium);
                break;

            case WeatherType.Storm:
                StartStorm();
                break;

            case WeatherType.Fog:
                StartFog();
                StartMist(1f);
                break;
        }
    }

    /// <summary>
    /// Updates particle intensities during weather transition.
    /// </summary>
    private void UpdateParticleIntensities(float progress)
    {
        // Smooth transition curve
        float t = Mathf.SmoothStep(0f, 1f, progress);

        // Update fog
        if (useFog)
        {
            float targetFogDensity = GetFogDensityForWeather(targetWeather);
            RenderSettings.fogDensity = Mathf.Lerp(RenderSettings.fogDensity, targetFogDensity, t);
        }

        // Particle emissions will be handled by individual Start/Stop methods
    }

    /// <summary>
    /// Gets fog density for a weather type.
    /// </summary>
    private float GetFogDensityForWeather(WeatherType weather)
    {
        switch (weather)
        {
            case WeatherType.Fog: return fogDensityFog;
            case WeatherType.Storm: return fogDensityFog * 0.5f;
            case WeatherType.Rain: return fogDensityClear * 2f;
            default: return fogDensityClear;
        }
    }
    #endregion

    #region Event Handlers
    /// <summary>
    /// Handles weather change events.
    /// </summary>
    private void OnWeatherChanged(WeatherType weatherType)
    {
        TransitionWeather(weatherType);
    }

    /// <summary>
    /// Handles time of day changes.
    /// </summary>
    private void OnTimeChanged(TimeChangedEventData data)
    {
        // Update fog color based on time of day
        if (useFog)
        {
            Color targetFogColor = data.timeOfDay == TimeOfDay.Day ? fogColorDay : fogColorNight;
            RenderSettings.fogColor = Color.Lerp(RenderSettings.fogColor, targetFogColor, Time.deltaTime * 0.5f);
        }
    }
    #endregion

    #region Quality Settings
    /// <summary>
    /// Sets the VFX quality level for weather effects.
    /// </summary>
    public void SetQuality(VFXQuality quality)
    {
        currentQuality = quality;

        // Restart current weather with new quality settings
        WeatherType temp = currentWeather;
        StopCurrentWeather();
        StartWeatherType(temp);

        Debug.Log($"[WeatherParticles] Quality set to {quality}");
    }

    /// <summary>
    /// Gets density multiplier based on quality.
    /// </summary>
    private float GetDensityMultiplier()
    {
        return vfxManager != null ? vfxManager.GetParticleDensity() : 1f;
    }
    #endregion
}

#region Enums
/// <summary>
/// Rain intensity levels.
/// </summary>
public enum RainIntensity
{
    Light,
    Medium,
    Heavy
}
#endregion

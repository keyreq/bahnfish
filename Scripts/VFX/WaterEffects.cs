using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Agent 13: Visual Effects & Particles Specialist - WaterEffects.cs
/// Manages all water-related visual effects including splashes, wake trails, ripples, foam, and caustics.
/// Creates beautiful and immersive water interactions for the fishing game.
/// </summary>
public class WaterEffects : MonoBehaviour
{
    #region Configuration
    [Header("Water Surface")]
    [SerializeField] private Material waterSurfaceMaterial;
    [SerializeField] private float waterLevel = 0f;
    [SerializeField] private bool enableCaustics = true;
    [SerializeField] private float causticsStrength = 0.5f;

    [Header("Splash Effects")]
    [SerializeField] private GameObject splashPrefab;
    [SerializeField] private GameObject smallSplashPrefab;
    [SerializeField] private GameObject largeSplashPrefab;
    [SerializeField] private float splashCooldown = 0.1f;

    [Header("Wake Trail")]
    [SerializeField] private GameObject wakeTrailPrefab;
    [SerializeField] private float wakeLifetime = 15f;
    [SerializeField] private float wakeUpdateInterval = 0.2f;
    [SerializeField] private float wakeMinSpeed = 1f;

    [Header("Ripples")]
    [SerializeField] private GameObject ripplePrefab;
    [SerializeField] private float rippleMaxRadius = 5f;
    [SerializeField] private float rippleExpandSpeed = 2f;
    [SerializeField] private float rippleLifetime = 3f;

    [Header("Foam")]
    [SerializeField] private GameObject foamPrefab;
    [SerializeField] private bool enableFoam = true;
    [SerializeField] private float foamDensity = 0.5f;

    [Header("Underwater Effects")]
    [SerializeField] private GameObject bubblesPrefab;
    [SerializeField] private float bubbleSpawnRate = 0.5f;
    #endregion

    #region Private Fields
    private VFXQuality currentQuality = VFXQuality.High;
    private List<WakeTrailSegment> activeWakeTrails = new List<WakeTrailSegment>();
    private List<Ripple> activeRipples = new List<Ripple>();
    private float lastSplashTime = 0f;
    private float lastWakeUpdate = 0f;
    private VFXManager vfxManager;
    #endregion

    #region Initialization
    private void Start()
    {
        vfxManager = VFXManager.Instance;

        // Subscribe to events
        SubscribeToEvents();

        // Register particle prefabs
        RegisterParticlePrefabs();

        Debug.Log("[WaterEffects] Initialized.");
    }

    /// <summary>
    /// Subscribes to game events.
    /// </summary>
    private void SubscribeToEvents()
    {
        // Player movement for wake trails
        EventSystem.Subscribe<PlayerMovedEventData>("PlayerMoved", OnPlayerMoved);

        // Fish events for splashes
        EventSystem.Subscribe<FishJumpData>("FishJumped", OnFishJumped);

        // Fishing events
        EventSystem.Subscribe<CastingData>("FishingCastStarted", OnFishingCast);
        EventSystem.Subscribe<Vector3>("FishingBobberLanded", OnBobberLanded);

        // Weather events
        EventSystem.Subscribe<WeatherType>("WeatherChanged", OnWeatherChanged);
    }

    /// <summary>
    /// Registers particle prefabs with the VFX Manager.
    /// </summary>
    private void RegisterParticlePrefabs()
    {
        if (vfxManager == null) return;

        if (splashPrefab != null) vfxManager.RegisterParticlePrefab("water_splash", splashPrefab);
        if (smallSplashPrefab != null) vfxManager.RegisterParticlePrefab("water_splash_small", smallSplashPrefab);
        if (largeSplashPrefab != null) vfxManager.RegisterParticlePrefab("water_splash_large", largeSplashPrefab);
        if (ripplePrefab != null) vfxManager.RegisterParticlePrefab("water_ripple", ripplePrefab);
        if (foamPrefab != null) vfxManager.RegisterParticlePrefab("water_foam", foamPrefab);
        if (bubblesPrefab != null) vfxManager.RegisterParticlePrefab("water_bubbles", bubblesPrefab);
    }

    private void OnDestroy()
    {
        EventSystem.Unsubscribe<PlayerMovedEventData>("PlayerMoved", OnPlayerMoved);
        EventSystem.Unsubscribe<FishJumpData>("FishJumped", OnFishJumped);
        EventSystem.Unsubscribe<CastingData>("FishingCastStarted", OnFishingCast);
        EventSystem.Unsubscribe<Vector3>("FishingBobberLanded", OnBobberLanded);
        EventSystem.Unsubscribe<WeatherType>("WeatherChanged", OnWeatherChanged);
    }
    #endregion

    #region Update Loop
    private void Update()
    {
        UpdateRipples();
        UpdateWakeTrails();
        UpdateWaterSurface();
    }

    /// <summary>
    /// Updates all active ripples.
    /// </summary>
    private void UpdateRipples()
    {
        for (int i = activeRipples.Count - 1; i >= 0; i--)
        {
            Ripple ripple = activeRipples[i];
            ripple.age += Time.deltaTime;
            ripple.radius += rippleExpandSpeed * Time.deltaTime;

            if (ripple.age >= rippleLifetime || ripple.radius >= rippleMaxRadius)
            {
                if (ripple.gameObject != null)
                {
                    Destroy(ripple.gameObject);
                }
                activeRipples.RemoveAt(i);
            }
            else
            {
                UpdateRippleVisuals(ripple);
            }
        }
    }

    /// <summary>
    /// Updates ripple visuals.
    /// </summary>
    private void UpdateRippleVisuals(Ripple ripple)
    {
        if (ripple.gameObject == null) return;

        // Scale ripple
        float scale = ripple.radius;
        ripple.gameObject.transform.localScale = new Vector3(scale, 1f, scale);

        // Fade out over lifetime
        float alpha = 1f - (ripple.age / rippleLifetime);
        Renderer renderer = ripple.gameObject.GetComponent<Renderer>();
        if (renderer != null && renderer.material != null)
        {
            Color color = renderer.material.color;
            color.a = alpha;
            renderer.material.color = color;
        }
    }

    /// <summary>
    /// Updates all active wake trails.
    /// </summary>
    private void UpdateWakeTrails()
    {
        for (int i = activeWakeTrails.Count - 1; i >= 0; i--)
        {
            WakeTrailSegment wake = activeWakeTrails[i];
            wake.age += Time.deltaTime;

            if (wake.age >= wakeLifetime)
            {
                if (wake.gameObject != null)
                {
                    Destroy(wake.gameObject);
                }
                activeWakeTrails.RemoveAt(i);
            }
            else
            {
                UpdateWakeTrailVisuals(wake);
            }
        }
    }

    /// <summary>
    /// Updates wake trail visuals.
    /// </summary>
    private void UpdateWakeTrailVisuals(WakeTrailSegment wake)
    {
        if (wake.gameObject == null) return;

        // Fade out over lifetime
        float alpha = 1f - (wake.age / wakeLifetime);
        Renderer renderer = wake.gameObject.GetComponent<Renderer>();
        if (renderer != null && renderer.material != null)
        {
            Color color = renderer.material.color;
            color.a = alpha * 0.5f; // Wake is semi-transparent
            renderer.material.color = color;
        }
    }

    /// <summary>
    /// Updates water surface shader properties.
    /// </summary>
    private void UpdateWaterSurface()
    {
        if (waterSurfaceMaterial == null) return;

        // Update caustics
        if (enableCaustics)
        {
            float causticsOffset = Time.time * 0.1f;
            waterSurfaceMaterial.SetFloat("_CausticsOffset", causticsOffset);
            waterSurfaceMaterial.SetFloat("_CausticsStrength", causticsStrength);
        }

        // Update water level
        waterSurfaceMaterial.SetFloat("_WaterLevel", waterLevel);
    }
    #endregion

    #region Splash Effects
    /// <summary>
    /// Creates a water splash at the specified position.
    /// </summary>
    public void CreateSplash(Vector3 position, SplashSize size = SplashSize.Medium, float intensity = 1f)
    {
        if (Time.time - lastSplashTime < splashCooldown) return;
        lastSplashTime = Time.time;

        string prefabID = GetSplashPrefabID(size);
        ParticleSystem splash = vfxManager.SpawnEffect(prefabID, position);

        if (splash != null)
        {
            var main = splash.main;
            main.startSizeMultiplier *= intensity;

            // Create ripple
            CreateRipple(position, intensity);

            // Create foam if enabled
            if (enableFoam && currentQuality >= VFXQuality.Medium)
            {
                CreateFoam(position, size);
            }

            Debug.Log($"[WaterEffects] Created {size} splash at {position}");
        }
    }

    /// <summary>
    /// Gets the prefab ID for a splash size.
    /// </summary>
    private string GetSplashPrefabID(SplashSize size)
    {
        switch (size)
        {
            case SplashSize.Small: return "water_splash_small";
            case SplashSize.Large: return "water_splash_large";
            default: return "water_splash";
        }
    }

    /// <summary>
    /// Creates a splash from boat movement.
    /// </summary>
    public void CreateBoatSplash(Vector3 position, float speed)
    {
        if (speed < wakeMinSpeed) return;

        SplashSize size = speed > 10f ? SplashSize.Large : SplashSize.Medium;
        float intensity = Mathf.Clamp01(speed / 20f);
        CreateSplash(position, size, intensity);
    }
    #endregion

    #region Wake Trails
    /// <summary>
    /// Creates a wake trail segment at the specified position.
    /// </summary>
    private void CreateWakeTrail(Vector3 position, Vector3 velocity)
    {
        if (currentQuality < VFXQuality.Medium) return;
        if (Time.time - lastWakeUpdate < wakeUpdateInterval) return;
        if (velocity.magnitude < wakeMinSpeed) return;

        lastWakeUpdate = Time.time;

        GameObject wakeObj = null;

        // Quality-based wake trail
        switch (currentQuality)
        {
            case VFXQuality.Medium:
                // Particle trail
                ParticleSystem ps = vfxManager.SpawnEffect("water_splash_small", position);
                if (ps != null) wakeObj = ps.gameObject;
                break;

            case VFXQuality.High:
            case VFXQuality.Ultra:
                // Full mesh wake
                if (wakeTrailPrefab != null)
                {
                    wakeObj = Instantiate(wakeTrailPrefab, position, Quaternion.LookRotation(velocity));
                }
                break;
        }

        if (wakeObj != null)
        {
            WakeTrailSegment wake = new WakeTrailSegment
            {
                gameObject = wakeObj,
                position = position,
                age = 0f
            };
            activeWakeTrails.Add(wake);
        }
    }
    #endregion

    #region Ripples
    /// <summary>
    /// Creates an expanding ripple at the specified position.
    /// </summary>
    public void CreateRipple(Vector3 position, float intensity = 1f)
    {
        GameObject rippleObj = null;

        if (currentQuality >= VFXQuality.Medium && ripplePrefab != null)
        {
            rippleObj = Instantiate(ripplePrefab, new Vector3(position.x, waterLevel, position.z), Quaternion.Euler(90, 0, 0));
        }

        if (rippleObj != null)
        {
            Ripple ripple = new Ripple
            {
                gameObject = rippleObj,
                position = position,
                radius = 0.5f,
                age = 0f,
                intensity = intensity
            };
            activeRipples.Add(ripple);
        }
    }

    /// <summary>
    /// Creates multiple ripples at the specified position for rain effect.
    /// </summary>
    public void CreateRainRipples(Vector3 position, int count = 5)
    {
        if (currentQuality < VFXQuality.Medium) return;

        for (int i = 0; i < count; i++)
        {
            Vector2 randomOffset = Random.insideUnitCircle * 2f;
            Vector3 ripplePos = position + new Vector3(randomOffset.x, 0, randomOffset.y);
            CreateRipple(ripplePos, 0.3f);
        }
    }
    #endregion

    #region Foam
    /// <summary>
    /// Creates foam particles at the specified position.
    /// </summary>
    private void CreateFoam(Vector3 position, SplashSize size)
    {
        if (!enableFoam || foamPrefab == null) return;
        if (currentQuality < VFXQuality.High) return;

        ParticleSystem foam = vfxManager.SpawnEffect("water_foam", position);
        if (foam != null)
        {
            var main = foam.main;
            float sizeMultiplier = size == SplashSize.Large ? 2f : (size == SplashSize.Small ? 0.5f : 1f);
            main.startSizeMultiplier *= sizeMultiplier * foamDensity;
        }
    }

    /// <summary>
    /// Creates persistent foam around an object (like rocks).
    /// </summary>
    public void CreatePersistentFoam(Vector3 position, float radius)
    {
        if (!enableFoam || currentQuality < VFXQuality.High) return;

        // TODO: Implement persistent foam system for rocks/coastline
        // This would use a different particle system that continuously emits
    }
    #endregion

    #region Underwater Effects
    /// <summary>
    /// Creates underwater bubble particles.
    /// </summary>
    public void CreateBubbles(Vector3 position, int count = 10)
    {
        if (currentQuality < VFXQuality.Medium) return;

        ParticleSystem bubbles = vfxManager.SpawnEffect("water_bubbles", position);
        if (bubbles != null)
        {
            var emission = bubbles.emission;
            emission.SetBurst(0, new ParticleSystem.Burst(0f, (short)count));
        }
    }
    #endregion

    #region Event Handlers
    /// <summary>
    /// Handles player movement for wake trails.
    /// </summary>
    private void OnPlayerMoved(PlayerMovedEventData data)
    {
        // Check if player is on water
        if (data.position.y <= waterLevel + 1f)
        {
            CreateWakeTrail(data.position, data.velocity);

            // Create bow splash if moving fast enough
            if (data.speed > 5f)
            {
                CreateBoatSplash(data.position + data.velocity.normalized * 2f, data.speed);
            }
        }
    }

    /// <summary>
    /// Handles fish jump events.
    /// </summary>
    private void OnFishJumped(FishJumpData data)
    {
        // Entry splash
        CreateSplash(data.position, GetSplashSizeForFish(data.fishSize), 1f);

        // Exit splash (delayed)
        StartCoroutine(DelayedSplash(data.position, data.airTime, data.fishSize));
    }

    /// <summary>
    /// Creates a delayed splash for fish re-entry.
    /// </summary>
    private System.Collections.IEnumerator DelayedSplash(Vector3 position, float delay, float fishSize)
    {
        yield return new System.WaitForSeconds(delay);
        CreateSplash(position, GetSplashSizeForFish(fishSize), 1f);
    }

    /// <summary>
    /// Gets splash size based on fish size.
    /// </summary>
    private SplashSize GetSplashSizeForFish(float fishSize)
    {
        if (fishSize < 0.5f) return SplashSize.Small;
        if (fishSize > 2f) return SplashSize.Large;
        return SplashSize.Medium;
    }

    /// <summary>
    /// Handles fishing cast events.
    /// </summary>
    private void OnFishingCast(CastingData data)
    {
        // No splash on cast, only on bobber landing
    }

    /// <summary>
    /// Handles bobber landing.
    /// </summary>
    private void OnBobberLanded(Vector3 position)
    {
        CreateSplash(position, SplashSize.Small, 0.5f);
    }

    /// <summary>
    /// Handles weather changes.
    /// </summary>
    private void OnWeatherChanged(WeatherType weatherType)
    {
        if (weatherType == WeatherType.Rain || weatherType == WeatherType.Storm)
        {
            // Start rain ripple generation
            StartCoroutine(GenerateRainRipples());
        }
    }

    /// <summary>
    /// Generates continuous rain ripples.
    /// </summary>
    private System.Collections.IEnumerator GenerateRainRipples()
    {
        while (true)
        {
            // Get random position around player
            // TODO: Get actual player position
            Vector3 playerPos = Vector3.zero; // Placeholder
            Vector2 randomOffset = Random.insideUnitCircle * 20f;
            Vector3 ripplePos = playerPos + new Vector3(randomOffset.x, waterLevel, randomOffset.y);

            CreateRipple(ripplePos, 0.2f);

            yield return new WaitForSeconds(0.1f);
        }
    }
    #endregion

    #region Quality Settings
    /// <summary>
    /// Sets the VFX quality level for water effects.
    /// </summary>
    public void SetQuality(VFXQuality quality)
    {
        currentQuality = quality;

        // Adjust settings based on quality
        switch (quality)
        {
            case VFXQuality.Low:
                enableFoam = false;
                enableCaustics = false;
                rippleMaxRadius = 3f;
                break;

            case VFXQuality.Medium:
                enableFoam = false;
                enableCaustics = true;
                causticsStrength = 0.3f;
                rippleMaxRadius = 4f;
                break;

            case VFXQuality.High:
                enableFoam = true;
                foamDensity = 0.5f;
                enableCaustics = true;
                causticsStrength = 0.5f;
                rippleMaxRadius = 5f;
                break;

            case VFXQuality.Ultra:
                enableFoam = true;
                foamDensity = 1f;
                enableCaustics = true;
                causticsStrength = 0.7f;
                rippleMaxRadius = 6f;
                break;
        }

        Debug.Log($"[WaterEffects] Quality set to {quality}");
    }
    #endregion
}

#region Data Structures
/// <summary>
/// Represents a wake trail segment.
/// </summary>
public class WakeTrailSegment
{
    public GameObject gameObject;
    public Vector3 position;
    public float age;
}

/// <summary>
/// Represents a water ripple.
/// </summary>
public class Ripple
{
    public GameObject gameObject;
    public Vector3 position;
    public float radius;
    public float age;
    public float intensity;
}

/// <summary>
/// Splash size enum.
/// </summary>
public enum SplashSize
{
    Small,
    Medium,
    Large
}

/// <summary>
/// Fish jump event data.
/// </summary>
[System.Serializable]
public struct FishJumpData
{
    public Vector3 position;
    public float fishSize;
    public float airTime;
    public FishRarity rarity;

    public FishJumpData(Vector3 pos, float size, float time, FishRarity rarity)
    {
        position = pos;
        fishSize = size;
        airTime = time;
        this.rarity = rarity;
    }
}

/// <summary>
/// Casting event data.
/// </summary>
[System.Serializable]
public struct CastingData
{
    public Vector3 startPosition;
    public Vector3 targetPosition;
    public float castPower;

    public CastingData(Vector3 start, Vector3 target, float power)
    {
        startPosition = start;
        targetPosition = target;
        castPower = power;
    }
}
#endregion

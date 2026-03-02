using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Agent 13: Visual Effects & Particles Specialist - VFXManager.cs
/// Central VFX system manager with particle pooling, quality settings, and performance optimization.
/// Manages all visual effects in Bahnfish including water, weather, fishing, horror, and event effects.
/// </summary>
public class VFXManager : MonoBehaviour
{
    #region Singleton
    private static VFXManager _instance;
    public static VFXManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<VFXManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("VFXManager");
                    _instance = go.AddComponent<VFXManager>();
                }
            }
            return _instance;
        }
    }
    #endregion

    #region Configuration
    [Header("VFX Configuration")]
    [SerializeField] private VFXQuality currentQuality = VFXQuality.High;
    [SerializeField] private bool particlesEnabled = true;
    [SerializeField] private bool postProcessingEnabled = true;
    [SerializeField] private float particleDensity = 1.0f;

    [Header("Performance Settings")]
    [SerializeField] private int maxSimultaneousParticles = 10000;
    [SerializeField] private float cullingDistance = 100f;
    [SerializeField] private bool autoQualityAdjust = true;
    [SerializeField] private int targetFPS = 60;

    [Header("Particle Pool Settings")]
    [SerializeField] private int defaultPoolSize = 20;
    [SerializeField] private int maxPoolSize = 100;
    [SerializeField] private float poolCleanupInterval = 30f;

    [Header("VFX Systems")]
    [SerializeField] private WaterEffects waterEffects;
    [SerializeField] private WeatherParticles weatherParticles;
    [SerializeField] private FishingVFX fishingVFX;
    [SerializeField] private HorrorVFX horrorVFX;
    [SerializeField] private EventVFX eventVFX;
    [SerializeField] private FishAIVisuals fishAIVisuals;
    [SerializeField] private CompanionVFX companionVFX;
    [SerializeField] private InventoryVFX inventoryVFX;
    [SerializeField] private UIParticleEffects uiParticleEffects;
    [SerializeField] private PostProcessingManager postProcessingManager;
    #endregion

    #region Particle Pooling
    private Dictionary<string, Queue<ParticleSystem>> particlePools = new Dictionary<string, Queue<ParticleSystem>>();
    private Dictionary<string, GameObject> particlePrefabs = new Dictionary<string, GameObject>();
    private List<ParticleSystem> activeParticles = new List<ParticleSystem>();
    private Transform poolContainer;
    private float lastCleanupTime;
    #endregion

    #region Performance Tracking
    private int currentParticleCount = 0;
    private float averageFPS = 60f;
    private float fpsCheckInterval = 1f;
    private float lastFPSCheck = 0f;
    private List<float> fpsHistory = new List<float>();
    #endregion

    #region Initialization
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        Initialize();
    }

    /// <summary>
    /// Initializes the VFX Manager and all subsystems.
    /// </summary>
    private void Initialize()
    {
        Debug.Log("[VFXManager] Initializing VFX system...");

        // Create pool container
        poolContainer = new GameObject("ParticlePool").transform;
        poolContainer.SetParent(transform);

        // Initialize subsystems
        InitializeSubsystems();

        // Load saved settings
        LoadVFXSettings();

        // Subscribe to events
        SubscribeToEvents();

        Debug.Log($"[VFXManager] Initialized with quality: {currentQuality}");
    }

    /// <summary>
    /// Initializes all VFX subsystems.
    /// </summary>
    private void InitializeSubsystems()
    {
        // Get or create subsystems
        if (waterEffects == null) waterEffects = GetOrCreateSubsystem<WaterEffects>();
        if (weatherParticles == null) weatherParticles = GetOrCreateSubsystem<WeatherParticles>();
        if (fishingVFX == null) fishingVFX = GetOrCreateSubsystem<FishingVFX>();
        if (horrorVFX == null) horrorVFX = GetOrCreateSubsystem<HorrorVFX>();
        if (eventVFX == null) eventVFX = GetOrCreateSubsystem<EventVFX>();
        if (fishAIVisuals == null) fishAIVisuals = GetOrCreateSubsystem<FishAIVisuals>();
        if (companionVFX == null) companionVFX = GetOrCreateSubsystem<CompanionVFX>();
        if (inventoryVFX == null) inventoryVFX = GetOrCreateSubsystem<InventoryVFX>();
        if (uiParticleEffects == null) uiParticleEffects = GetOrCreateSubsystem<UIParticleEffects>();
        if (postProcessingManager == null) postProcessingManager = GetOrCreateSubsystem<PostProcessingManager>();
    }

    /// <summary>
    /// Gets or creates a VFX subsystem component.
    /// </summary>
    private T GetOrCreateSubsystem<T>() where T : Component
    {
        T subsystem = GetComponentInChildren<T>();
        if (subsystem == null)
        {
            GameObject go = new GameObject(typeof(T).Name);
            go.transform.SetParent(transform);
            subsystem = go.AddComponent<T>();
        }
        return subsystem;
    }

    /// <summary>
    /// Subscribes to game events.
    /// </summary>
    private void SubscribeToEvents()
    {
        EventSystem.Subscribe<VFXQuality>("VFXQualityChanged", OnQualityChanged);
        EventSystem.Subscribe<bool>("ParticlesToggled", OnParticlesToggled);
        EventSystem.Subscribe<bool>("PostProcessingToggled", OnPostProcessingToggled);
    }

    private void OnDestroy()
    {
        EventSystem.Unsubscribe<VFXQuality>("VFXQualityChanged", OnQualityChanged);
        EventSystem.Unsubscribe<bool>("ParticlesToggled", OnParticlesToggled);
        EventSystem.Unsubscribe<bool>("PostProcessingToggled", OnPostProcessingToggled);
    }
    #endregion

    #region Update Loop
    private void Update()
    {
        // Performance tracking
        TrackPerformance();

        // Auto quality adjustment
        if (autoQualityAdjust)
        {
            AutoAdjustQuality();
        }

        // Cleanup inactive particles
        if (Time.time - lastCleanupTime > poolCleanupInterval)
        {
            CleanupPools();
            lastCleanupTime = Time.time;
        }

        // Update active particles
        UpdateActiveParticles();
    }

    /// <summary>
    /// Tracks FPS and particle performance.
    /// </summary>
    private void TrackPerformance()
    {
        if (Time.time - lastFPSCheck > fpsCheckInterval)
        {
            float currentFPS = 1f / Time.deltaTime;
            fpsHistory.Add(currentFPS);
            if (fpsHistory.Count > 30) fpsHistory.RemoveAt(0);

            averageFPS = 0f;
            foreach (float fps in fpsHistory)
            {
                averageFPS += fps;
            }
            averageFPS /= fpsHistory.Count;

            lastFPSCheck = Time.time;
        }

        // Count active particles
        currentParticleCount = 0;
        foreach (ParticleSystem ps in activeParticles)
        {
            if (ps != null && ps.isPlaying)
            {
                currentParticleCount += ps.particleCount;
            }
        }
    }

    /// <summary>
    /// Automatically adjusts quality based on performance.
    /// </summary>
    private void AutoAdjustQuality()
    {
        if (averageFPS < targetFPS - 10 && currentQuality > VFXQuality.Low)
        {
            // Decrease quality
            VFXQuality newQuality = (VFXQuality)((int)currentQuality - 1);
            SetQuality(newQuality);
            Debug.Log($"[VFXManager] Auto-decreased quality to {newQuality} (FPS: {averageFPS:F1})");
        }
        else if (averageFPS > targetFPS + 20 && currentQuality < VFXQuality.Ultra)
        {
            // Increase quality
            VFXQuality newQuality = (VFXQuality)((int)currentQuality + 1);
            SetQuality(newQuality);
            Debug.Log($"[VFXManager] Auto-increased quality to {newQuality} (FPS: {averageFPS:F1})");
        }
    }

    /// <summary>
    /// Updates active particles and removes stopped ones.
    /// </summary>
    private void UpdateActiveParticles()
    {
        for (int i = activeParticles.Count - 1; i >= 0; i--)
        {
            if (activeParticles[i] == null || !activeParticles[i].isPlaying)
            {
                if (activeParticles[i] != null)
                {
                    ReturnToPool(activeParticles[i]);
                }
                activeParticles.RemoveAt(i);
            }
        }
    }
    #endregion

    #region Particle Pooling
    /// <summary>
    /// Registers a particle prefab for pooling.
    /// </summary>
    public void RegisterParticlePrefab(string particleID, GameObject prefab)
    {
        if (!particlePrefabs.ContainsKey(particleID))
        {
            particlePrefabs[particleID] = prefab;
            particlePools[particleID] = new Queue<ParticleSystem>();
            Debug.Log($"[VFXManager] Registered particle prefab: {particleID}");
        }
    }

    /// <summary>
    /// Spawns a particle effect at the specified position and rotation.
    /// </summary>
    public ParticleSystem SpawnEffect(string particleID, Vector3 position, Quaternion rotation)
    {
        if (!particlesEnabled) return null;

        if (currentParticleCount >= maxSimultaneousParticles)
        {
            Debug.LogWarning("[VFXManager] Max simultaneous particles reached. Cannot spawn more.");
            return null;
        }

        ParticleSystem ps = GetFromPool(particleID);
        if (ps != null)
        {
            ps.transform.position = position;
            ps.transform.rotation = rotation;
            ps.gameObject.SetActive(true);
            ps.Play();
            activeParticles.Add(ps);

            EventSystem.Publish("VFXSpawned", particleID);
            return ps;
        }

        Debug.LogWarning($"[VFXManager] Particle prefab not found: {particleID}");
        return null;
    }

    /// <summary>
    /// Spawns a particle effect with automatic cleanup.
    /// </summary>
    public ParticleSystem SpawnEffect(string particleID, Vector3 position)
    {
        return SpawnEffect(particleID, position, Quaternion.identity);
    }

    /// <summary>
    /// Gets a particle system from the pool or creates a new one.
    /// </summary>
    private ParticleSystem GetFromPool(string particleID)
    {
        if (!particlePools.ContainsKey(particleID) || !particlePrefabs.ContainsKey(particleID))
        {
            return null;
        }

        ParticleSystem ps;
        if (particlePools[particleID].Count > 0)
        {
            ps = particlePools[particleID].Dequeue();
        }
        else
        {
            GameObject instance = Instantiate(particlePrefabs[particleID], poolContainer);
            ps = instance.GetComponent<ParticleSystem>();
            if (ps == null)
            {
                Debug.LogError($"[VFXManager] Prefab {particleID} does not have a ParticleSystem component!");
                Destroy(instance);
                return null;
            }
        }

        return ps;
    }

    /// <summary>
    /// Returns a particle system to the pool.
    /// </summary>
    private void ReturnToPool(ParticleSystem ps)
    {
        if (ps == null) return;

        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        ps.gameObject.SetActive(false);
        ps.transform.SetParent(poolContainer);

        // Find which pool this belongs to
        foreach (var kvp in particlePrefabs)
        {
            if (ps.gameObject.name.Contains(kvp.Key))
            {
                if (particlePools[kvp.Key].Count < maxPoolSize)
                {
                    particlePools[kvp.Key].Enqueue(ps);
                }
                else
                {
                    Destroy(ps.gameObject);
                }
                return;
            }
        }
    }

    /// <summary>
    /// Cleans up unused particle systems in the pools.
    /// </summary>
    private void CleanupPools()
    {
        foreach (var kvp in particlePools)
        {
            while (kvp.Value.Count > defaultPoolSize)
            {
                ParticleSystem ps = kvp.Value.Dequeue();
                if (ps != null)
                {
                    Destroy(ps.gameObject);
                }
            }
        }
    }

    /// <summary>
    /// Clears all particle pools and destroys all particles.
    /// </summary>
    public void ClearAllPools()
    {
        foreach (var kvp in particlePools)
        {
            while (kvp.Value.Count > 0)
            {
                ParticleSystem ps = kvp.Value.Dequeue();
                if (ps != null)
                {
                    Destroy(ps.gameObject);
                }
            }
        }
        particlePools.Clear();

        foreach (ParticleSystem ps in activeParticles)
        {
            if (ps != null)
            {
                Destroy(ps.gameObject);
            }
        }
        activeParticles.Clear();

        Debug.Log("[VFXManager] All particle pools cleared.");
    }
    #endregion

    #region Quality Settings
    /// <summary>
    /// Sets the VFX quality level.
    /// </summary>
    public void SetQuality(VFXQuality quality)
    {
        currentQuality = quality;
        particleDensity = GetParticleDensityForQuality(quality);

        // Apply quality to all subsystems
        ApplyQualitySettings();

        // Publish event
        EventSystem.Publish("VFXQualityChanged", quality);

        Debug.Log($"[VFXManager] Quality set to {quality} (Density: {particleDensity})");
    }

    /// <summary>
    /// Gets the particle density multiplier for a quality level.
    /// </summary>
    private float GetParticleDensityForQuality(VFXQuality quality)
    {
        switch (quality)
        {
            case VFXQuality.Low: return 0.2f;
            case VFXQuality.Medium: return 0.4f;
            case VFXQuality.High: return 0.7f;
            case VFXQuality.Ultra: return 1.0f;
            default: return 0.7f;
        }
    }

    /// <summary>
    /// Applies quality settings to all VFX subsystems.
    /// </summary>
    private void ApplyQualitySettings()
    {
        if (waterEffects != null) waterEffects.SetQuality(currentQuality);
        if (weatherParticles != null) weatherParticles.SetQuality(currentQuality);
        if (fishingVFX != null) fishingVFX.SetQuality(currentQuality);
        if (horrorVFX != null) horrorVFX.SetQuality(currentQuality);
        if (eventVFX != null) eventVFX.SetQuality(currentQuality);
        if (fishAIVisuals != null) fishAIVisuals.SetQuality(currentQuality);
        if (companionVFX != null) companionVFX.SetQuality(currentQuality);
        if (inventoryVFX != null) inventoryVFX.SetQuality(currentQuality);
        if (uiParticleEffects != null) uiParticleEffects.SetQuality(currentQuality);
        if (postProcessingManager != null) postProcessingManager.SetQuality(currentQuality);
    }

    /// <summary>
    /// Gets the current VFX quality level.
    /// </summary>
    public VFXQuality GetQuality()
    {
        return currentQuality;
    }

    /// <summary>
    /// Gets the current particle density multiplier.
    /// </summary>
    public float GetParticleDensity()
    {
        return particleDensity;
    }
    #endregion

    #region Enable/Disable Features
    /// <summary>
    /// Enables or disables all particle effects.
    /// </summary>
    public void SetParticlesEnabled(bool enabled)
    {
        particlesEnabled = enabled;
        if (!enabled)
        {
            // Stop all active particles
            foreach (ParticleSystem ps in activeParticles)
            {
                if (ps != null)
                {
                    ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                }
            }
        }
        EventSystem.Publish("ParticlesToggled", enabled);
    }

    /// <summary>
    /// Enables or disables post-processing effects.
    /// </summary>
    public void SetPostProcessingEnabled(bool enabled)
    {
        postProcessingEnabled = enabled;
        if (postProcessingManager != null)
        {
            postProcessingManager.SetEnabled(enabled);
        }
        EventSystem.Publish("PostProcessingToggled", enabled);
    }

    /// <summary>
    /// Gets whether particles are enabled.
    /// </summary>
    public bool AreParticlesEnabled()
    {
        return particlesEnabled;
    }

    /// <summary>
    /// Gets whether post-processing is enabled.
    /// </summary>
    public bool IsPostProcessingEnabled()
    {
        return postProcessingEnabled;
    }
    #endregion

    #region Event Handlers
    private void OnQualityChanged(VFXQuality quality)
    {
        Debug.Log($"[VFXManager] Quality changed to: {quality}");
    }

    private void OnParticlesToggled(bool enabled)
    {
        Debug.Log($"[VFXManager] Particles {(enabled ? "enabled" : "disabled")}");
    }

    private void OnPostProcessingToggled(bool enabled)
    {
        Debug.Log($"[VFXManager] Post-processing {(enabled ? "enabled" : "disabled")}");
    }
    #endregion

    #region Save/Load
    /// <summary>
    /// Loads VFX settings from save data.
    /// </summary>
    private void LoadVFXSettings()
    {
        // This will be called by SaveSystem when loading a game
        // For now, use default settings
        Debug.Log("[VFXManager] VFX settings loaded.");
    }

    /// <summary>
    /// Gets VFX data for saving.
    /// </summary>
    public VFXData GetVFXData()
    {
        return new VFXData
        {
            quality = currentQuality,
            particlesEnabled = particlesEnabled,
            postProcessingEnabled = postProcessingEnabled,
            particleDensity = particleDensity
        };
    }

    /// <summary>
    /// Loads VFX data from save.
    /// </summary>
    public void LoadVFXData(VFXData data)
    {
        if (data == null)
        {
            Debug.LogWarning("[VFXManager] No VFX data to load, using defaults.");
            return;
        }

        currentQuality = data.quality;
        particlesEnabled = data.particlesEnabled;
        postProcessingEnabled = data.postProcessingEnabled;
        particleDensity = data.particleDensity;

        ApplyQualitySettings();
        SetParticlesEnabled(particlesEnabled);
        SetPostProcessingEnabled(postProcessingEnabled);

        Debug.Log($"[VFXManager] Loaded VFX data: Quality={currentQuality}, Particles={particlesEnabled}");
    }
    #endregion

    #region Public API - Accessors
    /// <summary>
    /// Gets the water effects subsystem.
    /// </summary>
    public WaterEffects GetWaterEffects() => waterEffects;

    /// <summary>
    /// Gets the weather particles subsystem.
    /// </summary>
    public WeatherParticles GetWeatherParticles() => weatherParticles;

    /// <summary>
    /// Gets the fishing VFX subsystem.
    /// </summary>
    public FishingVFX GetFishingVFX() => fishingVFX;

    /// <summary>
    /// Gets the horror VFX subsystem.
    /// </summary>
    public HorrorVFX GetHorrorVFX() => horrorVFX;

    /// <summary>
    /// Gets the event VFX subsystem.
    /// </summary>
    public EventVFX GetEventVFX() => eventVFX;

    /// <summary>
    /// Gets the fish AI visuals subsystem.
    /// </summary>
    public FishAIVisuals GetFishAIVisuals() => fishAIVisuals;

    /// <summary>
    /// Gets the companion VFX subsystem.
    /// </summary>
    public CompanionVFX GetCompanionVFX() => companionVFX;

    /// <summary>
    /// Gets the inventory VFX subsystem.
    /// </summary>
    public InventoryVFX GetInventoryVFX() => inventoryVFX;

    /// <summary>
    /// Gets the UI particle effects subsystem.
    /// </summary>
    public UIParticleEffects GetUIParticleEffects() => uiParticleEffects;

    /// <summary>
    /// Gets the post-processing manager.
    /// </summary>
    public PostProcessingManager GetPostProcessingManager() => postProcessingManager;
    #endregion

    #region Debug
    /// <summary>
    /// Gets performance statistics for debugging.
    /// </summary>
    public string GetPerformanceStats()
    {
        return $"FPS: {averageFPS:F1} | Particles: {currentParticleCount}/{maxSimultaneousParticles} | Quality: {currentQuality}";
    }

    private void OnGUI()
    {
        if (Debug.isDebugBuild)
        {
            GUI.Label(new Rect(10, 10, 400, 20), GetPerformanceStats());
        }
    }
    #endregion
}

/// <summary>
/// VFX quality levels that affect particle density and complexity.
/// </summary>
[System.Serializable]
public enum VFXQuality
{
    Low = 0,
    Medium = 1,
    High = 2,
    Ultra = 3
}

/// <summary>
/// VFX save data structure for persistent settings.
/// </summary>
[System.Serializable]
public class VFXData
{
    public VFXQuality quality = VFXQuality.High;
    public bool particlesEnabled = true;
    public bool postProcessingEnabled = true;
    public float particleDensity = 1.0f;
}

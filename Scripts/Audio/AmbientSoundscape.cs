using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Agent 12: Audio System Specialist - AmbientSoundscape.cs
/// Manages layered ambient audio for locations, weather, and time of day.
/// Creates immersive soundscapes that blend multiple audio layers dynamically.
/// </summary>
public class AmbientSoundscape : MonoBehaviour
{
    #region Inspector Settings
    [Header("Ambient Layers")]
    [Tooltip("All ambient audio layers")]
    [SerializeField] private List<AmbientLayer> ambientLayers = new List<AmbientLayer>();

    [Header("Current State")]
    [SerializeField] private string currentLocation = "calm_lake";
    [SerializeField] private WeatherType currentWeather = WeatherType.Clear;
    [SerializeField] private TimeOfDay currentTimeOfDay = TimeOfDay.Day;

    [Header("Transition Settings")]
    [Tooltip("Fade duration when switching soundscapes")]
    [Range(0f, 10f)]
    [SerializeField] private float transitionDuration = 3f;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogging = true;
    #endregion

    #region Private Variables
    private AudioManager audioManager;
    private TimeManager timeManager;

    // Active layers (currently playing)
    private List<AmbientLayer> activeLayers = new List<AmbientLayer>();

    // Transition state
    private bool isTransitioning = false;
    #endregion

    #region Initialization
    private void Start()
    {
        audioManager = AudioManager.Instance;
        timeManager = TimeManager.Instance;

        // Subscribe to game events
        SubscribeToEvents();

        // Create default ambient layers if none assigned
        if (ambientLayers.Count == 0)
        {
            CreateDefaultAmbientLayers();
        }

        // Initialize with current state
        UpdateAmbientSoundscape();

        if (enableDebugLogging)
        {
            Debug.Log($"[AmbientSoundscape] Initialized with {ambientLayers.Count} ambient layers");
        }
    }

    /// <summary>
    /// Subscribes to relevant game events.
    /// </summary>
    private void SubscribeToEvents()
    {
        EventSystem.Subscribe<string>("LocationChanged", OnLocationChanged);
        EventSystem.Subscribe<string>("WeatherChanged", OnWeatherChanged);
        EventSystem.Subscribe<TimeOfDay>("TimeOfDayChanged", OnTimeOfDayChanged);
        EventSystem.Subscribe<string>("EnterAudioZone", OnEnterAudioZone);
        EventSystem.Subscribe<string>("ExitAudioZone", OnExitAudioZone);
    }
    #endregion

    #region Update
    private void Update()
    {
        UpdateLayerVolumes();
    }

    /// <summary>
    /// Updates volumes for active layers based on distance and conditions.
    /// </summary>
    private void UpdateLayerVolumes()
    {
        if (audioManager == null) return;

        Vector3 listenerPos = audioManager.GetListenerPosition();

        foreach (var layer in activeLayers)
        {
            if (layer.audioSource == null || !layer.is3D) continue;

            // Calculate distance-based volume for 3D layers
            float distance = Vector3.Distance(listenerPos, layer.worldPosition);
            float volumeMultiplier = Mathf.Clamp01(1f - (distance - layer.minDistance) / (layer.maxDistance - layer.minDistance));

            float targetVolume = GetEffectiveVolume() * layer.volume * volumeMultiplier;
            layer.audioSource.volume = targetVolume;
        }
    }
    #endregion

    #region Soundscape Management
    /// <summary>
    /// Updates the entire ambient soundscape based on current game state.
    /// </summary>
    private void UpdateAmbientSoundscape()
    {
        // Determine which layers should be active
        List<AmbientLayer> layersToActivate = new List<AmbientLayer>();

        foreach (var layer in ambientLayers)
        {
            if (ShouldLayerBeActive(layer))
            {
                layersToActivate.Add(layer);
            }
        }

        // Transition to new soundscape
        TransitionToSoundscape(layersToActivate);
    }

    /// <summary>
    /// Checks if an ambient layer should be active based on its condition.
    /// </summary>
    private bool ShouldLayerBeActive(AmbientLayer layer)
    {
        if (string.IsNullOrEmpty(layer.condition))
            return false;

        string condition = layer.condition.ToLower();

        // Location conditions
        if (condition.Contains("location"))
        {
            if (condition.Contains(currentLocation.ToLower()))
                return true;
        }

        // Weather conditions
        if (condition.Contains("weather"))
        {
            if (condition.Contains("clear") && currentWeather == WeatherType.Clear)
                return true;
            if (condition.Contains("rain") && currentWeather == WeatherType.Rain)
                return true;
            if (condition.Contains("storm") && currentWeather == WeatherType.Storm)
                return true;
            if (condition.Contains("fog") && currentWeather == WeatherType.Fog)
                return true;
        }

        // Time of day conditions
        if (condition.Contains("time") || condition.Contains("day") || condition.Contains("night"))
        {
            if (condition.Contains("day") && currentTimeOfDay == TimeOfDay.Day)
                return true;
            if (condition.Contains("night") && currentTimeOfDay == TimeOfDay.Night)
                return true;
            if (condition.Contains("dusk") && currentTimeOfDay == TimeOfDay.Dusk)
                return true;
            if (condition.Contains("dawn") && currentTimeOfDay == TimeOfDay.Dawn)
                return true;
        }

        // Base/Always conditions
        if (condition.Contains("base") || condition.Contains("always"))
            return true;

        return false;
    }

    /// <summary>
    /// Transitions from current soundscape to a new one.
    /// </summary>
    private void TransitionToSoundscape(List<AmbientLayer> targetLayers)
    {
        // Fade out layers that should no longer be active
        foreach (var layer in activeLayers.ToList())
        {
            if (!targetLayers.Contains(layer))
            {
                DeactivateLayer(layer);
            }
        }

        // Fade in new layers
        foreach (var layer in targetLayers)
        {
            if (!activeLayers.Contains(layer))
            {
                ActivateLayer(layer);
            }
        }
    }

    /// <summary>
    /// Activates an ambient layer with fade in.
    /// </summary>
    private void ActivateLayer(AmbientLayer layer)
    {
        if (layer.audioSource == null)
        {
            // Create audio source for this layer
            GameObject layerObj = new GameObject($"AmbientLayer_{layer.layerName}");
            layerObj.transform.SetParent(transform);
            layer.audioSource = layerObj.AddComponent<AudioSource>();
            layer.audioSource.clip = layer.clip;
            layer.audioSource.loop = layer.loop;
            layer.audioSource.playOnAwake = false;
            layer.audioSource.volume = 0f;

            // 3D audio setup
            if (layer.is3D)
            {
                layer.audioSource.spatialBlend = 1f;
                layer.audioSource.minDistance = layer.minDistance;
                layer.audioSource.maxDistance = layer.maxDistance;
                layer.audioSource.rolloffMode = AudioRolloffMode.Linear;
                layerObj.transform.position = layer.worldPosition;
            }
            else
            {
                layer.audioSource.spatialBlend = 0f;
            }

            layer.audioSource.Play();
        }

        activeLayers.Add(layer);

        // Fade in
        float targetVolume = GetEffectiveVolume() * layer.volume;
        audioManager.FadeAudioSource(layer.audioSource, targetVolume, transitionDuration);

        if (enableDebugLogging)
        {
            Debug.Log($"[AmbientSoundscape] Activated layer: {layer.layerName}");
        }
    }

    /// <summary>
    /// Deactivates an ambient layer with fade out.
    /// </summary>
    private void DeactivateLayer(AmbientLayer layer)
    {
        if (layer.audioSource == null) return;

        activeLayers.Remove(layer);

        // Fade out
        audioManager.FadeAudioSource(layer.audioSource, 0f, transitionDuration, () =>
        {
            if (layer.audioSource != null)
            {
                layer.audioSource.Stop();
                Destroy(layer.audioSource.gameObject);
                layer.audioSource = null;
            }
        });

        if (enableDebugLogging)
        {
            Debug.Log($"[AmbientSoundscape] Deactivated layer: {layer.layerName}");
        }
    }
    #endregion

    #region Volume Control
    /// <summary>
    /// Updates all ambient volumes (called when volume settings change).
    /// </summary>
    public void UpdateVolume()
    {
        float effectiveVolume = GetEffectiveVolume();

        foreach (var layer in activeLayers)
        {
            if (layer.audioSource != null)
            {
                float targetVolume = effectiveVolume * layer.volume;
                layer.audioSource.volume = targetVolume;
            }
        }
    }

    /// <summary>
    /// Gets the effective volume for ambient audio.
    /// </summary>
    private float GetEffectiveVolume()
    {
        if (audioManager != null)
        {
            return audioManager.masterVolume * audioManager.ambientVolume;
        }
        return 1f;
    }
    #endregion

    #region Event Handlers
    private void OnLocationChanged(string newLocation)
    {
        currentLocation = newLocation;
        UpdateAmbientSoundscape();

        if (enableDebugLogging)
        {
            Debug.Log($"[AmbientSoundscape] Location changed to: {newLocation}");
        }
    }

    private void OnWeatherChanged(string weatherString)
    {
        // Parse weather string to WeatherType
        if (System.Enum.TryParse(weatherString, true, out WeatherType weather))
        {
            currentWeather = weather;
            UpdateAmbientSoundscape();

            if (enableDebugLogging)
            {
                Debug.Log($"[AmbientSoundscape] Weather changed to: {weather}");
            }
        }
    }

    private void OnTimeOfDayChanged(TimeOfDay timeOfDay)
    {
        currentTimeOfDay = timeOfDay;
        UpdateAmbientSoundscape();

        if (enableDebugLogging)
        {
            Debug.Log($"[AmbientSoundscape] Time of day changed to: {timeOfDay}");
        }
    }

    private void OnEnterAudioZone(string zoneName)
    {
        // Audio zones can trigger specific ambient layers
        // Implementation depends on AudioZone system
    }

    private void OnExitAudioZone(string zoneName)
    {
        // Restore default ambient for location
    }
    #endregion

    #region Layer Creation
    /// <summary>
    /// Creates default ambient layers for all 13 locations + weather + time.
    /// </summary>
    private void CreateDefaultAmbientLayers()
    {
        // CALM LAKE LAYERS
        AddLayer("calm_lake_base", "Calm Lake - Gentle Waves", "location:calm_lake", 0.5f, false);
        AddLayer("calm_lake_birds", "Calm Lake - Birds", "location:calm_lake && time:day", 0.4f, false);
        AddLayer("calm_lake_ducks", "Calm Lake - Ducks", "location:calm_lake && time:day", 0.3f, true, new Vector3(20, 0, 10));
        AddLayer("calm_lake_night", "Calm Lake - Night Crickets", "location:calm_lake && time:night", 0.4f, false);

        // ROCKY COASTLINE LAYERS
        AddLayer("coastline_waves", "Coastline - Crashing Waves", "location:rocky_coastline", 0.7f, false);
        AddLayer("coastline_seagulls", "Coastline - Seagulls", "location:rocky_coastline && time:day", 0.5f, true, new Vector3(15, 5, 0));
        AddLayer("coastline_wind", "Coastline - Wind", "location:rocky_coastline", 0.4f, false);
        AddLayer("coastline_foghorn", "Coastline - Distant Foghorn", "location:rocky_coastline && weather:fog", 0.6f, true, new Vector3(100, 0, 0));

        // DEEP OCEAN LAYERS
        AddLayer("deep_ocean_base", "Deep Ocean - Rumble", "location:deep_ocean", 0.6f, false);
        AddLayer("deep_ocean_pressure", "Deep Ocean - Pressure", "location:deep_ocean", 0.4f, false);
        AddLayer("deep_ocean_whale", "Deep Ocean - Whale Song", "location:deep_ocean", 0.5f, true, new Vector3(50, -20, 30), 20f, 100f);
        AddLayer("deep_ocean_wind", "Deep Ocean - Heavy Wind", "location:deep_ocean", 0.5f, false);

        // BIOLUMINESCENT CAVERN LAYERS
        AddLayer("cavern_drips", "Cavern - Water Dripping", "location:biolum_cavern", 0.4f, true, new Vector3(5, 3, 8));
        AddLayer("cavern_echo", "Cavern - Echo Ambience", "location:biolum_cavern", 0.5f, false);
        AddLayer("cavern_bats", "Cavern - Bats", "location:biolum_cavern", 0.3f, true, new Vector3(0, 5, 0));
        AddLayer("cavern_hum", "Cavern - Bioluminescent Hum", "location:biolum_cavern", 0.3f, false);

        // ABYSSAL TRENCH LAYERS
        AddLayer("abyss_pressure", "Abyss - Deep Pressure", "location:abyssal_trench", 0.7f, false);
        AddLayer("abyss_creaking", "Abyss - Creaking", "location:abyssal_trench", 0.5f, false);
        AddLayer("abyss_entity", "Abyss - Unknown Sounds", "location:abyssal_trench", 0.4f, true, new Vector3(30, -50, 20));
        AddLayer("abyss_reverb", "Abyss - Massive Echo", "location:abyssal_trench", 0.6f, false);

        // GHOST SHIP GRAVEYARD LAYERS
        AddLayer("graveyard_wind", "Graveyard - Eerie Wind", "location:ghost_ship_graveyard", 0.5f, false);
        AddLayer("graveyard_creaking", "Graveyard - Ship Creaking", "location:ghost_ship_graveyard", 0.6f, true, new Vector3(25, 0, 15));
        AddLayer("graveyard_chains", "Graveyard - Chains", "location:ghost_ship_graveyard", 0.4f, true, new Vector3(30, 3, 10));
        AddLayer("graveyard_whispers", "Graveyard - Whispers", "location:ghost_ship_graveyard && time:night", 0.3f, false);

        // MANGROVE SWAMP LAYERS
        AddLayer("mangrove_water", "Mangrove - Water Lapping", "location:mangrove_swamp", 0.4f, false);
        AddLayer("mangrove_insects", "Mangrove - Insects", "location:mangrove_swamp", 0.5f, false);
        AddLayer("mangrove_birds", "Mangrove - Tropical Birds", "location:mangrove_swamp && time:day", 0.4f, true, new Vector3(10, 4, 12));
        AddLayer("mangrove_frogs", "Mangrove - Frogs", "location:mangrove_swamp && time:night", 0.5f, false);

        // FROZEN WATERS LAYERS
        AddLayer("frozen_wind", "Frozen - Arctic Wind", "location:frozen_waters", 0.6f, false);
        AddLayer("frozen_ice_crack", "Frozen - Ice Cracking", "location:frozen_waters", 0.5f, true, new Vector3(20, 0, 20));
        AddLayer("frozen_silence", "Frozen - Eerie Silence", "location:frozen_waters", 0.3f, false);

        // CORAL REEF LAYERS
        AddLayer("reef_waves", "Reef - Gentle Waves", "location:coral_reef", 0.4f, false);
        AddLayer("reef_bubbles", "Reef - Underwater Bubbles", "location:coral_reef", 0.3f, false);
        AddLayer("reef_fish", "Reef - Fish Sounds", "location:coral_reef", 0.3f, true, new Vector3(8, -2, 10));
        AddLayer("reef_birds", "Reef - Seabirds", "location:coral_reef && time:day", 0.4f, true, new Vector3(15, 5, 5));

        // VOLCANIC VENT LAYERS
        AddLayer("volcanic_rumble", "Volcanic - Rumble", "location:volcanic_vent", 0.7f, false);
        AddLayer("volcanic_bubbles", "Volcanic - Bubbling", "location:volcanic_vent", 0.6f, true, new Vector3(10, -5, 8));
        AddLayer("volcanic_hiss", "Volcanic - Steam Hiss", "location:volcanic_vent", 0.5f, true, new Vector3(5, 0, 5));

        // KELP FOREST LAYERS
        AddLayer("kelp_underwater", "Kelp - Underwater Ambience", "location:kelp_forest", 0.5f, false);
        AddLayer("kelp_sway", "Kelp - Swaying", "location:kelp_forest", 0.3f, false);
        AddLayer("kelp_fish", "Kelp - Fish Movement", "location:kelp_forest", 0.3f, true, new Vector3(7, -3, 9));

        // SHIPWRECK COVE LAYERS
        AddLayer("shipwreck_waves", "Shipwreck - Waves", "location:shipwreck_cove", 0.5f, false);
        AddLayer("shipwreck_creaking", "Shipwreck - Wood Creaking", "location:shipwreck_cove", 0.4f, true, new Vector3(12, 0, 8));
        AddLayer("shipwreck_wind", "Shipwreck - Wind Through Hull", "location:shipwreck_cove", 0.4f, false);

        // RIVER DELTA LAYERS
        AddLayer("delta_water_flow", "Delta - Water Flow", "location:river_delta", 0.6f, false);
        AddLayer("delta_birds", "Delta - Birds", "location:river_delta && time:day", 0.5f, true, new Vector3(15, 3, 10));
        AddLayer("delta_insects", "Delta - Insects", "location:river_delta", 0.4f, false);

        // WEATHER LAYERS (applied regardless of location)
        AddLayer("weather_rain", "Weather - Rain", "weather:rain", 0.7f, false);
        AddLayer("weather_storm", "Weather - Storm", "weather:storm", 0.8f, false);
        AddLayer("weather_thunder", "Weather - Thunder", "weather:storm", 0.6f, false);
        AddLayer("weather_heavy_wind", "Weather - Heavy Wind", "weather:storm", 0.7f, false);
        AddLayer("weather_fog", "Weather - Fog Ambience", "weather:fog", 0.4f, false);

        // TIME-BASED LAYERS
        AddLayer("time_dawn_birds", "Dawn - First Birds", "time:dawn", 0.4f, false);
        AddLayer("time_night_crickets", "Night - Crickets", "time:night", 0.4f, false);
        AddLayer("time_night_owls", "Night - Owls", "time:night", 0.3f, true, new Vector3(25, 4, 15));

        if (enableDebugLogging)
        {
            Debug.Log($"[AmbientSoundscape] Created {ambientLayers.Count} default ambient layers");
        }
    }

    /// <summary>
    /// Helper method to add an ambient layer.
    /// </summary>
    private void AddLayer(string id, string name, string condition, float volume, bool is3D,
                         Vector3 position = default, float minDist = 10f, float maxDist = 50f)
    {
        AmbientLayer layer = new AmbientLayer
        {
            layerName = id,
            condition = condition,
            volume = volume,
            loop = true,
            is3D = is3D,
            worldPosition = position,
            minDistance = minDist,
            maxDistance = maxDist
        };

        ambientLayers.Add(layer);
    }
    #endregion

    #region Public API
    /// <summary>
    /// Manually sets the current location (in case not using LocationManager).
    /// </summary>
    public void SetLocation(string locationID)
    {
        currentLocation = locationID;
        UpdateAmbientSoundscape();
    }

    /// <summary>
    /// Registers a new ambient layer.
    /// </summary>
    public void RegisterAmbientLayer(AmbientLayer layer)
    {
        if (layer != null && !ambientLayers.Contains(layer))
        {
            ambientLayers.Add(layer);
        }
    }

    /// <summary>
    /// Gets all currently active layers.
    /// </summary>
    public List<AmbientLayer> GetActiveLayers()
    {
        return new List<AmbientLayer>(activeLayers);
    }
    #endregion

    #region Cleanup
    private void OnDestroy()
    {
        // Unsubscribe from events
        EventSystem.Unsubscribe<string>("LocationChanged", OnLocationChanged);
        EventSystem.Unsubscribe<string>("WeatherChanged", OnWeatherChanged);
        EventSystem.Unsubscribe<TimeOfDay>("TimeOfDayChanged", OnTimeOfDayChanged);
        EventSystem.Unsubscribe<string>("EnterAudioZone", OnEnterAudioZone);
        EventSystem.Unsubscribe<string>("ExitAudioZone", OnExitAudioZone);

        // Stop all ambient layers
        foreach (var layer in activeLayers.ToList())
        {
            if (layer.audioSource != null)
            {
                layer.audioSource.Stop();
                Destroy(layer.audioSource.gameObject);
            }
        }
        activeLayers.Clear();
    }
    #endregion

    #region Editor Utilities
#if UNITY_EDITOR
    [ContextMenu("Print Ambient Status")]
    private void PrintAmbientStatus()
    {
        Debug.Log("=== Ambient Soundscape Status ===");
        Debug.Log($"Current Location: {currentLocation}");
        Debug.Log($"Current Weather: {currentWeather}");
        Debug.Log($"Current Time: {currentTimeOfDay}");
        Debug.Log($"Active Layers: {activeLayers.Count}/{ambientLayers.Count}");

        foreach (var layer in activeLayers)
        {
            Debug.Log($"  - {layer.layerName} (Volume: {layer.volume:F2})");
        }
    }

    [ContextMenu("Force Update Soundscape")]
    private void ForceUpdateSoundscape()
    {
        UpdateAmbientSoundscape();
    }
#endif
    #endregion
}

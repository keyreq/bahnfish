using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Agent 12: Audio System Specialist - MusicSystem.cs
/// Dynamic music system with layering, seamless transitions, and adaptive music.
/// Responds to game state (time of day, sanity, weather, events) to create immersive atmosphere.
/// </summary>
public class MusicSystem : MonoBehaviour
{
    #region Inspector Settings
    [Header("Music Tracks")]
    [Tooltip("All available music tracks")]
    [SerializeField] private List<MusicTrack> musicTracks = new List<MusicTrack>();

    [Header("Current State")]
    [SerializeField] private MusicTrack currentTrack;
    [SerializeField] private MusicTrackType currentTrackType = MusicTrackType.Menu;

    [Header("Transition Settings")]
    [Tooltip("Default crossfade duration for track transitions")]
    [Range(0f, 10f)]
    [SerializeField] private float defaultCrossfadeDuration = 3f;

    [Header("Layer Management")]
    [Tooltip("Auto-manage layers based on game state")]
    [SerializeField] private bool autoManageLayers = true;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogging = true;
    #endregion

    #region Private Variables
    private AudioManager audioManager;
    private TimeManager timeManager;
    private SanityManager sanityManager;
    private float normalVolume = 1f;
    private bool isDucked = false;

    // Track transition
    private bool isTransitioning = false;
    private MusicTrack transitioningFrom;
    private float transitionProgress = 0f;
    private float transitionDuration = 0f;
    #endregion

    #region Initialization
    private void Start()
    {
        audioManager = AudioManager.Instance;
        timeManager = TimeManager.Instance;
        sanityManager = SanityManager.Instance;

        // Subscribe to game events
        SubscribeToEvents();

        // Initialize default music tracks if none assigned
        if (musicTracks.Count == 0)
        {
            CreateDefaultMusicTracks();
        }

        // Start with menu music
        if (musicTracks.Count > 0)
        {
            PlayTrack(MusicTrackType.Menu);
        }

        if (enableDebugLogging)
        {
            Debug.Log($"[MusicSystem] Initialized with {musicTracks.Count} music tracks");
        }
    }

    /// <summary>
    /// Subscribes to all relevant game events for adaptive music.
    /// </summary>
    private void SubscribeToEvents()
    {
        EventSystem.Subscribe<TimeOfDay>("TimeOfDayChanged", OnTimeOfDayChanged);
        EventSystem.Subscribe<float>("SanityChanged", OnSanityChanged);
        EventSystem.Subscribe<string>("WeatherChanged", OnWeatherChanged);
        EventSystem.Subscribe("FishingStarted", OnFishingStarted);
        EventSystem.Subscribe("FishHooked", OnFishHooked);
        EventSystem.Subscribe("FishCaught", OnFishCaught);
        EventSystem.Subscribe("FishLost", OnFishLost);
        EventSystem.Subscribe<GameEvent>("EventStarted", OnDynamicEventStarted);
        EventSystem.Subscribe("EventEnded", OnDynamicEventEnded);
        EventSystem.Subscribe("EnterShop", OnEnterShop);
        EventSystem.Subscribe("ExitShop", OnExitShop);
        EventSystem.Subscribe<string>("PlayEventMusic", OnPlayEventMusic);
        EventSystem.Subscribe("StopEventMusic", OnStopEventMusic);
    }
    #endregion

    #region Update
    private void Update()
    {
        if (currentTrack != null && autoManageLayers)
        {
            UpdateMusicLayers();
        }

        UpdateTransition();
    }

    /// <summary>
    /// Updates music layers based on current game state.
    /// </summary>
    private void UpdateMusicLayers()
    {
        if (currentTrack == null || currentTrack.layers == null) return;

        foreach (var layer in currentTrack.layers)
        {
            bool shouldBeActive = ShouldLayerBeActive(layer);

            if (shouldBeActive && !layer.isActive)
            {
                ActivateLayer(layer);
            }
            else if (!shouldBeActive && layer.isActive)
            {
                DeactivateLayer(layer);
            }
        }
    }

    /// <summary>
    /// Checks if a music layer should be active based on its condition.
    /// </summary>
    private bool ShouldLayerBeActive(MusicLayer layer)
    {
        // Base layers are always active
        if (layer.isBaseLayer)
            return true;

        // Parse condition string (simplified - in production use more robust parsing)
        if (string.IsNullOrEmpty(layer.activationCondition))
            return false;

        string condition = layer.activationCondition.ToLower();

        // Sanity conditions
        if (condition.Contains("sanity"))
        {
            if (sanityManager == null) return false;

            float sanity = sanityManager.GetCurrentSanity();
            if (condition.Contains("<") && condition.Contains("50"))
                return sanity < 50f;
            if (condition.Contains("<") && condition.Contains("30"))
                return sanity < 30f;
            if (condition.Contains(">") && condition.Contains("70"))
                return sanity > 70f;
        }

        // Time conditions
        if (condition.Contains("night"))
        {
            if (timeManager == null) return false;
            return timeManager.IsNighttime();
        }

        // Hazard conditions
        if (condition.Contains("hazard"))
        {
            // Check if hazards are nearby (would need HazardManager reference)
            return false; // Placeholder
        }

        return false;
    }

    /// <summary>
    /// Activates a music layer with fade in.
    /// </summary>
    private void ActivateLayer(MusicLayer layer)
    {
        if (layer.audioSource == null)
        {
            // Create audio source for this layer
            GameObject layerObj = new GameObject($"MusicLayer_{layer.layerName}");
            layerObj.transform.SetParent(transform);
            layer.audioSource = layerObj.AddComponent<AudioSource>();
            layer.audioSource.clip = layer.clip;
            layer.audioSource.loop = true;
            layer.audioSource.playOnAwake = false;
            layer.audioSource.volume = 0f;

            // Sync with current track position if possible
            if (currentTrack != null && currentTrack.layers.Length > 0)
            {
                AudioSource baseSource = currentTrack.layers[0].audioSource;
                if (baseSource != null && baseSource.isPlaying)
                {
                    layer.audioSource.time = baseSource.time;
                }
            }

            layer.audioSource.Play();
        }

        layer.isActive = true;

        // Fade in
        float targetVolume = GetEffectiveVolume() * layer.volume;
        audioManager.FadeAudioSource(layer.audioSource, targetVolume, 1.5f);

        if (enableDebugLogging)
        {
            Debug.Log($"[MusicSystem] Activated layer: {layer.layerName}");
        }
    }

    /// <summary>
    /// Deactivates a music layer with fade out.
    /// </summary>
    private void DeactivateLayer(MusicLayer layer)
    {
        if (layer.audioSource == null) return;

        layer.isActive = false;

        // Fade out
        audioManager.FadeAudioSource(layer.audioSource, 0f, 1.5f, () =>
        {
            if (layer.audioSource != null)
            {
                layer.audioSource.Stop();
            }
        });

        if (enableDebugLogging)
        {
            Debug.Log($"[MusicSystem] Deactivated layer: {layer.layerName}");
        }
    }

    /// <summary>
    /// Updates smooth track transition.
    /// </summary>
    private void UpdateTransition()
    {
        if (!isTransitioning) return;

        transitionProgress += Time.deltaTime;
        float t = Mathf.Clamp01(transitionProgress / transitionDuration);

        if (t >= 1f)
        {
            // Transition complete
            isTransitioning = false;
            if (transitioningFrom != null)
            {
                StopTrack(transitioningFrom);
                transitioningFrom = null;
            }
        }
    }
    #endregion

    #region Track Playback
    /// <summary>
    /// Plays a specific music track type.
    /// </summary>
    public void PlayTrack(MusicTrackType trackType, bool forceRestart = false)
    {
        MusicTrack track = GetTrackByType(trackType);
        if (track == null)
        {
            if (enableDebugLogging)
                Debug.LogWarning($"[MusicSystem] No track found for type: {trackType}");
            return;
        }

        PlayTrack(track, forceRestart);
    }

    /// <summary>
    /// Plays a specific music track.
    /// </summary>
    public void PlayTrack(MusicTrack track, bool forceRestart = false)
    {
        if (track == null) return;

        // If same track already playing, don't restart unless forced
        if (currentTrack == track && !forceRestart)
            return;

        // Store previous track for crossfade
        transitioningFrom = currentTrack;
        currentTrack = track;
        currentTrackType = track.trackType;

        // Initialize track layers
        InitializeTrackLayers(track);

        // Start crossfade transition
        float crossfade = track.crossfadeDuration > 0f ? track.crossfadeDuration : defaultCrossfadeDuration;
        StartTransition(transitioningFrom, track, crossfade);

        // Publish event
        EventSystem.Publish("MusicTrackChanged", track.trackType);

        if (enableDebugLogging)
        {
            Debug.Log($"[MusicSystem] Now playing: {track.trackName}");
        }
    }

    /// <summary>
    /// Initializes all layers for a track.
    /// </summary>
    private void InitializeTrackLayers(MusicTrack track)
    {
        if (track.layers == null || track.layers.Length == 0) return;

        foreach (var layer in track.layers)
        {
            // Create audio source for layer
            GameObject layerObj = new GameObject($"MusicLayer_{layer.layerName}");
            layerObj.transform.SetParent(transform);
            layer.audioSource = layerObj.AddComponent<AudioSource>();
            layer.audioSource.clip = layer.clip;
            layer.audioSource.loop = track.loop;
            layer.audioSource.playOnAwake = false;
            layer.audioSource.volume = 0f;

            // Start playing all layers (even if not active yet)
            layer.audioSource.Play();

            // Base layers start active, others wait for conditions
            if (layer.isBaseLayer)
            {
                layer.isActive = true;
                float targetVolume = GetEffectiveVolume() * layer.volume * track.baseVolume;
                audioManager.FadeAudioSource(layer.audioSource, targetVolume, 0.1f);
            }
            else
            {
                layer.isActive = false;
            }
        }
    }

    /// <summary>
    /// Starts a crossfade transition between tracks.
    /// </summary>
    private void StartTransition(MusicTrack fromTrack, MusicTrack toTrack, float duration)
    {
        isTransitioning = true;
        transitionDuration = duration;
        transitionProgress = 0f;

        // Fade out old track
        if (fromTrack != null)
        {
            FadeOutTrack(fromTrack, duration);
        }

        // Fade in new track
        if (toTrack != null)
        {
            // Fade in is handled by layer initialization
        }
    }

    /// <summary>
    /// Fades out all layers of a track.
    /// </summary>
    private void FadeOutTrack(MusicTrack track, float duration)
    {
        if (track.layers == null) return;

        foreach (var layer in track.layers)
        {
            if (layer.audioSource != null)
            {
                audioManager.FadeAudioSource(layer.audioSource, 0f, duration);
            }
        }
    }

    /// <summary>
    /// Stops a track completely.
    /// </summary>
    private void StopTrack(MusicTrack track)
    {
        if (track == null || track.layers == null) return;

        foreach (var layer in track.layers)
        {
            if (layer.audioSource != null)
            {
                layer.audioSource.Stop();
                Destroy(layer.audioSource.gameObject);
                layer.audioSource = null;
            }
            layer.isActive = false;
        }
    }

    /// <summary>
    /// Stops all music.
    /// </summary>
    public void StopMusic(float fadeOutTime = 2f)
    {
        if (currentTrack != null)
        {
            FadeOutTrack(currentTrack, fadeOutTime);
            StartCoroutine(StopTrackDelayed(currentTrack, fadeOutTime));
            currentTrack = null;
        }
    }

    private System.Collections.IEnumerator StopTrackDelayed(MusicTrack track, float delay)
    {
        yield return new WaitForSeconds(delay);
        StopTrack(track);
    }
    #endregion

    #region Volume Control
    /// <summary>
    /// Updates all music volumes (called when master/music volume changes).
    /// </summary>
    public void UpdateVolume()
    {
        if (currentTrack == null || currentTrack.layers == null) return;

        float effectiveVolume = GetEffectiveVolume();

        foreach (var layer in currentTrack.layers)
        {
            if (layer.audioSource != null && layer.isActive)
            {
                float targetVolume = effectiveVolume * layer.volume * currentTrack.baseVolume;
                layer.audioSource.volume = targetVolume;
            }
        }
    }

    /// <summary>
    /// Gets the effective volume for music (considering master, music, and ducking).
    /// </summary>
    private float GetEffectiveVolume()
    {
        float volume = normalVolume;
        if (audioManager != null)
        {
            volume = audioManager.masterVolume * audioManager.musicVolume;
        }
        return volume;
    }

    /// <summary>
    /// Ducks the music volume (for important sounds/dialog).
    /// </summary>
    public void DuckVolume(float targetMultiplier, float fadeTime)
    {
        isDucked = true;
        normalVolume = targetMultiplier;
        UpdateVolume();
    }

    /// <summary>
    /// Restores normal music volume after ducking.
    /// </summary>
    public void RestoreVolume(float fadeTime)
    {
        isDucked = false;
        normalVolume = 1f;
        UpdateVolume();
    }
    #endregion

    #region Event Handlers
    /// <summary>
    /// Handles time of day changes to switch music tracks.
    /// </summary>
    private void OnTimeOfDayChanged(TimeOfDay timeOfDay)
    {
        MusicTrackType newTrackType = currentTrackType;

        switch (timeOfDay)
        {
            case TimeOfDay.Dawn:
                newTrackType = MusicTrackType.Dawn;
                break;
            case TimeOfDay.Day:
                newTrackType = MusicTrackType.Day;
                break;
            case TimeOfDay.Dusk:
                newTrackType = MusicTrackType.Dusk;
                break;
            case TimeOfDay.Night:
                newTrackType = MusicTrackType.Night;
                break;
        }

        if (newTrackType != currentTrackType && currentTrackType != MusicTrackType.Fishing)
        {
            PlayTrack(newTrackType);
        }
    }

    private void OnSanityChanged(float sanity)
    {
        // Music layers will auto-update based on sanity
        // Additional logic can be added here for dramatic changes
    }

    private void OnWeatherChanged(string weather)
    {
        // Could add weather-specific music layers here
    }

    private void OnFishingStarted()
    {
        PlayTrack(MusicTrackType.Fishing);
    }

    private void OnFishHooked()
    {
        // Increase music intensity (handled by layers)
        if (currentTrack != null && currentTrack.trackType == MusicTrackType.Fishing)
        {
            // Could add tempo increase here
        }
    }

    private void OnFishCaught()
    {
        // Return to normal fishing music
    }

    private void OnFishLost()
    {
        // Return to calm music
        if (timeManager != null)
        {
            OnTimeOfDayChanged(timeManager.GetCurrentTimeOfDay());
        }
    }

    private void OnDynamicEventStarted(GameEvent gameEvent)
    {
        // Event-specific music could be triggered here
        if (gameEvent.data.eventMusic != null)
        {
            // Play event music (would need to create a track for it)
        }
    }

    private void OnDynamicEventEnded()
    {
        // Return to time-appropriate music
        if (timeManager != null)
        {
            OnTimeOfDayChanged(timeManager.GetCurrentTimeOfDay());
        }
    }

    private void OnEnterShop()
    {
        PlayTrack(MusicTrackType.Shop);
    }

    private void OnExitShop()
    {
        if (timeManager != null)
        {
            OnTimeOfDayChanged(timeManager.GetCurrentTimeOfDay());
        }
    }

    private void OnPlayEventMusic(string musicClipName)
    {
        // Custom event music playback
        PlayTrack(MusicTrackType.Event);
    }

    private void OnStopEventMusic()
    {
        if (timeManager != null)
        {
            OnTimeOfDayChanged(timeManager.GetCurrentTimeOfDay());
        }
    }
    #endregion

    #region Track Management
    /// <summary>
    /// Gets a track by its type.
    /// </summary>
    private MusicTrack GetTrackByType(MusicTrackType type)
    {
        return musicTracks.FirstOrDefault(t => t.trackType == type);
    }

    /// <summary>
    /// Registers a new music track.
    /// </summary>
    public void RegisterTrack(MusicTrack track)
    {
        if (track != null && !musicTracks.Contains(track))
        {
            musicTracks.Add(track);
        }
    }

    /// <summary>
    /// Creates default placeholder music tracks.
    /// </summary>
    private void CreateDefaultMusicTracks()
    {
        // Create placeholder tracks (actual audio clips assigned in Unity)
        var trackTypes = System.Enum.GetValues(typeof(MusicTrackType));
        foreach (MusicTrackType type in trackTypes)
        {
            MusicTrack track = new MusicTrack
            {
                trackID = type.ToString().ToLower(),
                trackName = type.ToString(),
                trackType = type,
                layers = new MusicLayer[0],
                crossfadeDuration = defaultCrossfadeDuration,
                loop = true,
                baseVolume = 0.7f
            };
            musicTracks.Add(track);
        }
    }
    #endregion

    #region Cleanup
    private void OnDestroy()
    {
        // Unsubscribe from all events
        EventSystem.Unsubscribe<TimeOfDay>("TimeOfDayChanged", OnTimeOfDayChanged);
        EventSystem.Unsubscribe<float>("SanityChanged", OnSanityChanged);
        EventSystem.Unsubscribe<string>("WeatherChanged", OnWeatherChanged);
        EventSystem.Unsubscribe("FishingStarted", OnFishingStarted);
        EventSystem.Unsubscribe("FishHooked", OnFishHooked);
        EventSystem.Unsubscribe("FishCaught", OnFishCaught);
        EventSystem.Unsubscribe("FishLost", OnFishLost);
        EventSystem.Unsubscribe<GameEvent>("EventStarted", OnDynamicEventStarted);
        EventSystem.Unsubscribe("EventEnded", OnDynamicEventEnded);
        EventSystem.Unsubscribe("EnterShop", OnEnterShop);
        EventSystem.Unsubscribe("ExitShop", OnExitShop);
        EventSystem.Unsubscribe<string>("PlayEventMusic", OnPlayEventMusic);
        EventSystem.Unsubscribe("StopEventMusic", OnStopEventMusic);

        // Stop all music
        if (currentTrack != null)
        {
            StopTrack(currentTrack);
        }
    }
    #endregion

    #region Editor Utilities
#if UNITY_EDITOR
    [ContextMenu("Print Music Status")]
    private void PrintMusicStatus()
    {
        Debug.Log("=== Music System Status ===");
        Debug.Log($"Current Track: {(currentTrack != null ? currentTrack.trackName : "None")}");
        Debug.Log($"Track Type: {currentTrackType}");
        Debug.Log($"Is Transitioning: {isTransitioning}");
        Debug.Log($"Is Ducked: {isDucked}");
        Debug.Log($"Registered Tracks: {musicTracks.Count}");

        if (currentTrack != null && currentTrack.layers != null)
        {
            Debug.Log($"Active Layers: {currentTrack.layers.Count(l => l.isActive)}/{currentTrack.layers.Length}");
            foreach (var layer in currentTrack.layers)
            {
                Debug.Log($"  - {layer.layerName}: {(layer.isActive ? "ACTIVE" : "Inactive")}");
            }
        }
    }
#endif
    #endregion
}

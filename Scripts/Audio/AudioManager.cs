using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Agent 12: Audio System Specialist - AudioManager.cs
/// Central audio system manager handling all audio playback, mixing, and pooling.
/// Manages audio channels (Music, SFX, Ambient, UI), volume controls, ducking, and fade operations.
/// </summary>
public class AudioManager : MonoBehaviour
{
    #region Singleton
    private static AudioManager _instance;
    public static AudioManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<AudioManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("AudioManager");
                    _instance = go.AddComponent<AudioManager>();
                }
            }
            return _instance;
        }
    }
    #endregion

    #region Inspector Settings
    [Header("Audio Mixer")]
    [Tooltip("Unity Audio Mixer for advanced mixing (optional)")]
    [SerializeField] private AudioMixerGroup masterMixerGroup;
    [SerializeField] private AudioMixerGroup musicMixerGroup;
    [SerializeField] private AudioMixerGroup sfxMixerGroup;
    [SerializeField] private AudioMixerGroup ambientMixerGroup;
    [SerializeField] private AudioMixerGroup uiMixerGroup;

    [Header("Volume Settings")]
    [Range(0f, 1f)] public float masterVolume = 1.0f;
    [Range(0f, 1f)] public float musicVolume = 0.7f;
    [Range(0f, 1f)] public float sfxVolume = 0.8f;
    [Range(0f, 1f)] public float ambientVolume = 0.6f;
    [Range(0f, 1f)] public float uiVolume = 1.0f;

    [Header("Mute Controls")]
    public bool musicMuted = false;
    public bool sfxMuted = false;
    public bool ambientMuted = false;
    public bool uiMuted = false;

    [Header("Audio Pool Settings")]
    [Tooltip("Number of AudioSource components to pre-create")]
    [SerializeField] private int poolSize = 32;

    [Tooltip("Maximum concurrent sounds playing at once")]
    [SerializeField] private int maxConcurrentSounds = 32;

    [Header("Ducking Settings")]
    [Tooltip("Enable automatic music ducking during important sounds")]
    [SerializeField] private bool enableDucking = true;

    [Tooltip("Target volume for music during ducking (multiplier)")]
    [Range(0f, 1f)]
    [SerializeField] private float duckingVolume = 0.4f;

    [Tooltip("Fade time for ducking transitions")]
    [SerializeField] private float duckingFadeTime = 0.5f;

    [Header("Performance")]
    [Tooltip("Maximum audible distance for 3D sounds (culling)")]
    [SerializeField] private float maxAudibleDistance = 100f;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogging = true;
    #endregion

    #region Private Variables
    // Audio source pool
    private List<AudioSource> audioSourcePool = new List<AudioSource>();
    private List<AudioSource> activeAudioSources = new List<AudioSource>();

    // Fade operations
    private List<AudioFade> activeFades = new List<AudioFade>();

    // Ducking state
    private bool isDucking = false;
    private int duckingCount = 0;

    // Component references
    private MusicSystem musicSystem;
    private SoundEffectManager sfxManager;
    private AmbientSoundscape ambientSoundscape;
    private UIAudioController uiAudioController;

    // Camera reference for 3D audio
    private Transform audioListener;
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

        InitializeAudioPool();
    }

    private void Start()
    {
        // Find audio listener (usually on camera)
        audioListener = Camera.main?.transform;
        if (audioListener == null)
        {
            AudioListener listener = FindObjectOfType<AudioListener>();
            if (listener != null)
                audioListener = listener.transform;
        }

        // Get component references
        musicSystem = GetComponent<MusicSystem>();
        sfxManager = GetComponent<SoundEffectManager>();
        ambientSoundscape = GetComponent<AmbientSoundscape>();
        uiAudioController = GetComponent<UIAudioController>();

        // Subscribe to save system events
        EventSystem.Subscribe<SaveData>("GatheringSaveData", OnGatheringSaveData);
        EventSystem.Subscribe<SaveData>("ApplyingSaveData", OnApplyingSaveData);

        // Load audio settings from save data if available
        LoadAudioSettings();

        if (enableDebugLogging)
        {
            Debug.Log($"[AudioManager] Initialized with {poolSize} audio sources in pool");
        }
    }

    /// <summary>
    /// Creates a pool of AudioSource components for efficient audio playback.
    /// </summary>
    private void InitializeAudioPool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject audioObject = new GameObject($"AudioSource_Pool_{i}");
            audioObject.transform.SetParent(transform);
            AudioSource source = audioObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            audioSourcePool.Add(source);
        }
    }
    #endregion

    #region Update
    private void Update()
    {
        UpdateActiveFades();
        UpdateActiveAudioSources();
    }

    /// <summary>
    /// Updates all active audio fade operations.
    /// </summary>
    private void UpdateActiveFades()
    {
        for (int i = activeFades.Count - 1; i >= 0; i--)
        {
            if (activeFades[i].Update(Time.deltaTime))
            {
                activeFades.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// Updates and cleans up finished audio sources.
    /// </summary>
    private void UpdateActiveAudioSources()
    {
        for (int i = activeAudioSources.Count - 1; i >= 0; i--)
        {
            AudioSource source = activeAudioSources[i];
            if (source != null && !source.isPlaying && !source.loop)
            {
                ReturnToPool(source);
                activeAudioSources.RemoveAt(i);
            }
        }
    }
    #endregion

    #region Audio Playback
    /// <summary>
    /// Plays a sound effect at a specific world position.
    /// </summary>
    /// <param name="clipData">Audio clip data to play</param>
    /// <param name="position">World position for 3D audio</param>
    /// <param name="parent">Optional parent transform to attach audio source to</param>
    /// <returns>The AudioSource playing the sound (or null if failed)</returns>
    public AudioSource PlaySoundAtPosition(AudioClipData clipData, Vector3 position, Transform parent = null)
    {
        if (clipData == null || clipData.clip == null)
        {
            Debug.LogWarning("[AudioManager] Attempted to play null audio clip!");
            return null;
        }

        // Check cooldown
        if (!clipData.IsAvailable())
        {
            if (enableDebugLogging)
                Debug.Log($"[AudioManager] Sound '{clipData.displayName}' is on cooldown");
            return null;
        }

        // Check if muted
        if (IsCategoryMuted(clipData.category))
            return null;

        // Check max concurrent sounds
        if (activeAudioSources.Count >= maxConcurrentSounds)
        {
            // Try to cull lowest priority sound
            if (!CullLowestPrioritySound(clipData.priority))
                return null;
        }

        // Get audio source from pool
        AudioSource source = GetAudioSourceFromPool();
        if (source == null)
        {
            Debug.LogWarning("[AudioManager] Audio source pool exhausted!");
            return null;
        }

        // Configure audio source
        ConfigureAudioSource(source, clipData, position, parent);

        // Apply delay if specified
        float delay = clipData.GetRandomDelay();
        if (delay > 0f)
        {
            source.PlayDelayed(delay);
        }
        else
        {
            source.Play();
        }

        // Mark as played (for cooldown)
        clipData.MarkPlayed();

        // Add to active sources
        activeAudioSources.Add(source);

        // Apply fade in if specified
        if (clipData.fadeInTime > 0f)
        {
            source.volume = 0f;
            FadeAudioSource(source, GetCategoryVolume(clipData.category) * clipData.GetRandomVolume(), clipData.fadeInTime);
        }

        if (enableDebugLogging)
        {
            Debug.Log($"[AudioManager] Playing sound '{clipData.displayName}' at {position}");
        }

        // Publish event
        EventSystem.Publish("SoundEffectPlayed", new AudioEventData(clipData.id, clipData.category, position, source.volume));

        return source;
    }

    /// <summary>
    /// Plays a 2D sound (not positional).
    /// </summary>
    public AudioSource PlaySound2D(AudioClipData clipData)
    {
        return PlaySoundAtPosition(clipData, Vector3.zero, null);
    }

    /// <summary>
    /// Stops a specific audio source with optional fade out.
    /// </summary>
    public void StopAudioSource(AudioSource source, float fadeOutTime = 0f)
    {
        if (source == null) return;

        if (fadeOutTime > 0f)
        {
            FadeAudioSource(source, 0f, fadeOutTime, () =>
            {
                source.Stop();
                ReturnToPool(source);
            });
        }
        else
        {
            source.Stop();
            ReturnToPool(source);
        }
    }

    /// <summary>
    /// Stops all sounds in a specific category.
    /// </summary>
    public void StopCategory(AudioCategory category, float fadeOutTime = 0f)
    {
        var sourcesToStop = activeAudioSources.Where(s => GetSourceCategory(s) == category).ToList();
        foreach (var source in sourcesToStop)
        {
            StopAudioSource(source, fadeOutTime);
        }
    }

    /// <summary>
    /// Stops all currently playing audio.
    /// </summary>
    public void StopAllAudio(float fadeOutTime = 0f)
    {
        var sources = new List<AudioSource>(activeAudioSources);
        foreach (var source in sources)
        {
            StopAudioSource(source, fadeOutTime);
        }
    }
    #endregion

    #region Audio Source Pool Management
    /// <summary>
    /// Gets an available AudioSource from the pool.
    /// </summary>
    private AudioSource GetAudioSourceFromPool()
    {
        foreach (var source in audioSourcePool)
        {
            if (!source.isPlaying)
            {
                return source;
            }
        }

        // If pool exhausted, try to expand (if under max concurrent)
        if (audioSourcePool.Count < maxConcurrentSounds)
        {
            GameObject audioObject = new GameObject($"AudioSource_Pool_{audioSourcePool.Count}");
            audioObject.transform.SetParent(transform);
            AudioSource newSource = audioObject.AddComponent<AudioSource>();
            newSource.playOnAwake = false;
            audioSourcePool.Add(newSource);
            return newSource;
        }

        return null;
    }

    /// <summary>
    /// Returns an AudioSource to the pool for reuse.
    /// </summary>
    private void ReturnToPool(AudioSource source)
    {
        if (source == null) return;

        source.Stop();
        source.clip = null;
        source.transform.SetParent(transform);
        source.transform.localPosition = Vector3.zero;
        source.spatialBlend = 0f;
        source.loop = false;

        activeAudioSources.Remove(source);
    }

    /// <summary>
    /// Configures an AudioSource with the given clip data.
    /// </summary>
    private void ConfigureAudioSource(AudioClipData clipData, AudioSource source, Vector3 position, Transform parent)
    {
        source.clip = clipData.clip;
        source.volume = GetCategoryVolume(clipData.category) * clipData.GetRandomVolume();
        source.pitch = clipData.GetRandomPitch();
        source.loop = clipData.loop;
        source.priority = (int)clipData.priority;

        // 3D audio settings
        if (clipData.is3D)
        {
            source.spatialBlend = 1f;
            source.minDistance = clipData.minDistance;
            source.maxDistance = clipData.maxDistance;
            source.dopplerLevel = clipData.dopplerLevel;
            source.rolloffMode = AudioRolloffMode.Linear;

            // Position audio source
            source.transform.position = position;
            if (parent != null)
            {
                source.transform.SetParent(parent);
            }
        }
        else
        {
            source.spatialBlend = 0f;
        }

        // Set mixer group
        source.outputAudioMixerGroup = GetMixerGroupForCategory(clipData.category);
    }
    #endregion

    #region Volume and Mixing
    /// <summary>
    /// Gets the effective volume for a specific audio category.
    /// </summary>
    private float GetCategoryVolume(AudioCategory category)
    {
        float categoryVol = 1f;

        switch (category)
        {
            case AudioCategory.Music:
                categoryVol = musicMuted ? 0f : musicVolume;
                break;
            case AudioCategory.SFX_Fishing:
            case AudioCategory.SFX_Boat:
            case AudioCategory.SFX_Horror:
            case AudioCategory.SFX_Companion:
            case AudioCategory.SFX_Environment:
                categoryVol = sfxMuted ? 0f : sfxVolume;
                break;
            case AudioCategory.Ambient_Weather:
            case AudioCategory.Ambient_Location:
            case AudioCategory.Ambient_Time:
                categoryVol = ambientMuted ? 0f : ambientVolume;
                break;
            case AudioCategory.SFX_UI:
                categoryVol = uiMuted ? 0f : uiVolume;
                break;
        }

        return masterVolume * categoryVol;
    }

    /// <summary>
    /// Sets the master volume.
    /// </summary>
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        EventSystem.Publish("AudioVolumeChanged", "Master");
    }

    /// <summary>
    /// Sets the music volume.
    /// </summary>
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSystem != null)
            musicSystem.UpdateVolume();
        EventSystem.Publish("AudioVolumeChanged", "Music");
    }

    /// <summary>
    /// Sets the SFX volume.
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        EventSystem.Publish("AudioVolumeChanged", "SFX");
    }

    /// <summary>
    /// Sets the ambient volume.
    /// </summary>
    public void SetAmbientVolume(float volume)
    {
        ambientVolume = Mathf.Clamp01(volume);
        if (ambientSoundscape != null)
            ambientSoundscape.UpdateVolume();
        EventSystem.Publish("AudioVolumeChanged", "Ambient");
    }

    /// <summary>
    /// Sets the UI volume.
    /// </summary>
    public void SetUIVolume(float volume)
    {
        uiVolume = Mathf.Clamp01(volume);
        EventSystem.Publish("AudioVolumeChanged", "UI");
    }

    /// <summary>
    /// Toggles music mute.
    /// </summary>
    public void ToggleMusicMute()
    {
        musicMuted = !musicMuted;
        if (musicSystem != null)
            musicSystem.UpdateVolume();
        EventSystem.Publish("AudioMuted", "Music");
    }

    /// <summary>
    /// Toggles SFX mute.
    /// </summary>
    public void ToggleSFXMute()
    {
        sfxMuted = !sfxMuted;
        EventSystem.Publish("AudioMuted", "SFX");
    }

    /// <summary>
    /// Checks if a category is currently muted.
    /// </summary>
    private bool IsCategoryMuted(AudioCategory category)
    {
        switch (category)
        {
            case AudioCategory.Music:
                return musicMuted;
            case AudioCategory.SFX_Fishing:
            case AudioCategory.SFX_Boat:
            case AudioCategory.SFX_Horror:
            case AudioCategory.SFX_Companion:
            case AudioCategory.SFX_Environment:
                return sfxMuted;
            case AudioCategory.Ambient_Weather:
            case AudioCategory.Ambient_Location:
            case AudioCategory.Ambient_Time:
                return ambientMuted;
            case AudioCategory.SFX_UI:
                return uiMuted;
            default:
                return false;
        }
    }

    /// <summary>
    /// Gets the appropriate mixer group for a category.
    /// </summary>
    private AudioMixerGroup GetMixerGroupForCategory(AudioCategory category)
    {
        switch (category)
        {
            case AudioCategory.Music:
                return musicMixerGroup;
            case AudioCategory.SFX_Fishing:
            case AudioCategory.SFX_Boat:
            case AudioCategory.SFX_Horror:
            case AudioCategory.SFX_Companion:
            case AudioCategory.SFX_Environment:
                return sfxMixerGroup;
            case AudioCategory.Ambient_Weather:
            case AudioCategory.Ambient_Location:
            case AudioCategory.Ambient_Time:
                return ambientMixerGroup;
            case AudioCategory.SFX_UI:
                return uiMixerGroup;
            default:
                return masterMixerGroup;
        }
    }
    #endregion

    #region Audio Ducking
    /// <summary>
    /// Starts audio ducking (lower music volume for important sounds).
    /// </summary>
    public void StartDucking()
    {
        if (!enableDucking) return;

        duckingCount++;
        if (isDucking) return;

        isDucking = true;
        if (musicSystem != null)
        {
            musicSystem.DuckVolume(duckingVolume, duckingFadeTime);
        }

        if (enableDebugLogging)
            Debug.Log("[AudioManager] Started audio ducking");
    }

    /// <summary>
    /// Stops audio ducking (restore music volume).
    /// </summary>
    public void StopDucking()
    {
        if (!enableDucking) return;

        duckingCount--;
        if (duckingCount > 0) return;

        duckingCount = 0;
        isDucking = false;

        if (musicSystem != null)
        {
            musicSystem.RestoreVolume(duckingFadeTime);
        }

        if (enableDebugLogging)
            Debug.Log("[AudioManager] Stopped audio ducking");
    }
    #endregion

    #region Fade Operations
    /// <summary>
    /// Fades an AudioSource to a target volume over time.
    /// </summary>
    public void FadeAudioSource(AudioSource source, float targetVolume, float duration, System.Action onComplete = null)
    {
        if (source == null || duration <= 0f)
        {
            source?.SetVolume(targetVolume);
            onComplete?.Invoke();
            return;
        }

        AudioFade fade = new AudioFade(source, targetVolume, duration, onComplete);
        activeFades.Add(fade);
    }
    #endregion

    #region Priority and Culling
    /// <summary>
    /// Attempts to cull the lowest priority sound to make room for a higher priority one.
    /// </summary>
    private bool CullLowestPrioritySound(AudioPriority newSoundPriority)
    {
        AudioSource lowestPrioritySource = null;
        int lowestPriority = (int)newSoundPriority;

        foreach (var source in activeAudioSources)
        {
            if (source.priority > lowestPriority)
            {
                lowestPriority = source.priority;
                lowestPrioritySource = source;
            }
        }

        if (lowestPrioritySource != null)
        {
            StopAudioSource(lowestPrioritySource);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Gets the category of an audio source (stored in tag temporarily).
    /// This is a helper for managing sounds by category.
    /// </summary>
    private AudioCategory GetSourceCategory(AudioSource source)
    {
        // This is a simplification - in production, you might store this differently
        // For now, we'll use the mixer group to infer category
        if (source.outputAudioMixerGroup == musicMixerGroup)
            return AudioCategory.Music;
        if (source.outputAudioMixerGroup == sfxMixerGroup)
            return AudioCategory.SFX_Environment;
        if (source.outputAudioMixerGroup == ambientMixerGroup)
            return AudioCategory.Ambient_Location;
        if (source.outputAudioMixerGroup == uiMixerGroup)
            return AudioCategory.SFX_UI;

        return AudioCategory.SFX_Environment;
    }
    #endregion

    #region Save/Load Integration
    /// <summary>
    /// Saves current audio settings to save data.
    /// </summary>
    private void OnGatheringSaveData(SaveData saveData)
    {
        if (saveData.audioData == null)
            saveData.audioData = new AudioData();

        saveData.audioData.masterVolume = masterVolume;
        saveData.audioData.musicVolume = musicVolume;
        saveData.audioData.sfxVolume = sfxVolume;
        saveData.audioData.ambientVolume = ambientVolume;
        saveData.audioData.uiVolume = uiVolume;
        saveData.audioData.musicMuted = musicMuted;
        saveData.audioData.sfxMuted = sfxMuted;

        if (enableDebugLogging)
            Debug.Log("[AudioManager] Audio settings saved");
    }

    /// <summary>
    /// Loads audio settings from save data.
    /// </summary>
    private void OnApplyingSaveData(SaveData saveData)
    {
        if (saveData.audioData == null) return;

        masterVolume = saveData.audioData.masterVolume;
        musicVolume = saveData.audioData.musicVolume;
        sfxVolume = saveData.audioData.sfxVolume;
        ambientVolume = saveData.audioData.ambientVolume;
        uiVolume = saveData.audioData.uiVolume;
        musicMuted = saveData.audioData.musicMuted;
        sfxMuted = saveData.audioData.sfxMuted;

        // Update all systems with new volumes
        if (musicSystem != null)
            musicSystem.UpdateVolume();
        if (ambientSoundscape != null)
            ambientSoundscape.UpdateVolume();

        if (enableDebugLogging)
            Debug.Log("[AudioManager] Audio settings loaded");
    }

    /// <summary>
    /// Loads audio settings (called on start if no save data available).
    /// </summary>
    private void LoadAudioSettings()
    {
        // Settings will be loaded via save system
        // This is a placeholder for PlayerPrefs fallback if needed
    }
    #endregion

    #region Utility
    /// <summary>
    /// Gets the current audio listener position (usually camera).
    /// </summary>
    public Vector3 GetListenerPosition()
    {
        return audioListener != null ? audioListener.position : Vector3.zero;
    }

    /// <summary>
    /// Gets the number of active audio sources.
    /// </summary>
    public int GetActiveAudioSourceCount()
    {
        return activeAudioSources.Count;
    }
    #endregion

    #region Cleanup
    private void OnDestroy()
    {
        EventSystem.Unsubscribe<SaveData>("GatheringSaveData", OnGatheringSaveData);
        EventSystem.Unsubscribe<SaveData>("ApplyingSaveData", OnApplyingSaveData);

        if (_instance == this)
            _instance = null;
    }
    #endregion

    #region Editor Utilities
#if UNITY_EDITOR
    [ContextMenu("Print Audio Status")]
    private void PrintAudioStatus()
    {
        Debug.Log("=== Audio Manager Status ===");
        Debug.Log($"Active Audio Sources: {activeAudioSources.Count}/{maxConcurrentSounds}");
        Debug.Log($"Pool Size: {audioSourcePool.Count}");
        Debug.Log($"Active Fades: {activeFades.Count}");
        Debug.Log($"Master Volume: {masterVolume:F2}");
        Debug.Log($"Music Volume: {musicVolume:F2} (Muted: {musicMuted})");
        Debug.Log($"SFX Volume: {sfxVolume:F2} (Muted: {sfxMuted})");
        Debug.Log($"Ambient Volume: {ambientVolume:F2}");
        Debug.Log($"Ducking Active: {isDucking}");
    }

    [ContextMenu("Stop All Audio")]
    private void EditorStopAllAudio()
    {
        StopAllAudio(0.5f);
    }
#endif
    #endregion
}

/// <summary>
/// Extension methods for AudioSource.
/// </summary>
public static class AudioSourceExtensions
{
    public static void SetVolume(this AudioSource source, float volume)
    {
        if (source != null)
            source.volume = volume;
    }
}

using UnityEngine;
using System;

/// <summary>
/// Agent 12: Audio System Specialist - Core Audio Types and Data Structures
/// Defines all audio categories, enums, and data structures used across the audio system.
/// </summary>

/// <summary>
/// Audio category classification for organization and mixing purposes.
/// </summary>
public enum AudioCategory
{
    Music,
    SFX_Fishing,
    SFX_Boat,
    SFX_Horror,
    SFX_Companion,
    SFX_UI,
    SFX_Environment,
    Ambient_Weather,
    Ambient_Location,
    Ambient_Time
}

/// <summary>
/// Music track types for different game contexts.
/// </summary>
public enum MusicTrackType
{
    Menu,
    Day,
    Dusk,
    Night,
    Dawn,
    Fishing,
    Shop,
    Boss,
    Event
}

/// <summary>
/// Audio priority levels for sound mixing and culling.
/// Higher priority sounds won't be culled when max concurrent sounds reached.
/// </summary>
public enum AudioPriority
{
    Critical = 0,   // UI sounds, important events (never culled)
    High = 64,      // Player actions, fishing, companion sounds
    Medium = 128,   // Ambient effects, environment
    Low = 192,      // Background, decorative sounds
    VeryLow = 255   // Can be culled anytime
}

/// <summary>
/// Comprehensive data structure for audio clips with all necessary metadata.
/// </summary>
[Serializable]
public class AudioClipData
{
    [Header("Identity")]
    [Tooltip("Unique identifier for this audio clip")]
    public string id;

    [Tooltip("Display name for editor/debugging")]
    public string displayName;

    [Tooltip("The actual audio clip asset (assigned in Unity Inspector)")]
    public AudioClip clip;

    [Header("Classification")]
    [Tooltip("Category for organization and mixing")]
    public AudioCategory category;

    [Tooltip("Priority level for sound culling")]
    public AudioPriority priority = AudioPriority.Medium;

    [Header("Playback Settings")]
    [Tooltip("Base volume multiplier (0.0 - 1.0)")]
    [Range(0f, 1f)]
    public float baseVolume = 1.0f;

    [Tooltip("Random pitch variation range (e.g., 0.1 = ±10% pitch)")]
    [Range(0f, 0.5f)]
    public float pitchVariation = 0.1f;

    [Tooltip("Random volume variation range")]
    [Range(0f, 0.3f)]
    public float volumeVariation = 0.05f;

    [Tooltip("Loop this audio clip")]
    public bool loop = false;

    [Header("Fade Settings")]
    [Tooltip("Fade in duration in seconds")]
    public float fadeInTime = 0f;

    [Tooltip("Fade out duration in seconds")]
    public float fadeOutTime = 0f;

    [Header("3D Audio Settings")]
    [Tooltip("Use 3D spatial audio")]
    public bool is3D = false;

    [Tooltip("Minimum distance for 3D audio attenuation")]
    public float minDistance = 1f;

    [Tooltip("Maximum distance for 3D audio (sound inaudible beyond this)")]
    public float maxDistance = 50f;

    [Tooltip("Doppler effect intensity (0 = none, 1 = realistic)")]
    [Range(0f, 1f)]
    public float dopplerLevel = 0.5f;

    [Header("Advanced")]
    [Tooltip("Random delay before playing (min-max in seconds)")]
    public Vector2 randomDelay = Vector2.zero;

    [Tooltip("Cooldown time before this sound can play again (prevents spam)")]
    public float cooldown = 0f;

    [Tooltip("Last time this sound was played (runtime)")]
    [NonSerialized]
    public float lastPlayedTime = -999f;

    /// <summary>
    /// Gets a randomized pitch value within the variation range.
    /// </summary>
    public float GetRandomPitch()
    {
        return 1f + UnityEngine.Random.Range(-pitchVariation, pitchVariation);
    }

    /// <summary>
    /// Gets a randomized volume value within the variation range.
    /// </summary>
    public float GetRandomVolume()
    {
        return baseVolume + UnityEngine.Random.Range(-volumeVariation, volumeVariation);
    }

    /// <summary>
    /// Gets a random delay within the specified range.
    /// </summary>
    public float GetRandomDelay()
    {
        if (randomDelay == Vector2.zero) return 0f;
        return UnityEngine.Random.Range(randomDelay.x, randomDelay.y);
    }

    /// <summary>
    /// Check if this sound is off cooldown and can be played.
    /// </summary>
    public bool IsAvailable()
    {
        if (cooldown <= 0f) return true;
        return Time.time - lastPlayedTime >= cooldown;
    }

    /// <summary>
    /// Mark this sound as played (updates cooldown timer).
    /// </summary>
    public void MarkPlayed()
    {
        lastPlayedTime = Time.time;
    }
}

/// <summary>
/// Music layer data for dynamic music system.
/// Layers can be added/removed based on game state for adaptive music.
/// </summary>
[Serializable]
public class MusicLayer
{
    [Tooltip("Layer name for identification")]
    public string layerName;

    [Tooltip("Audio clip for this layer")]
    public AudioClip clip;

    [Tooltip("Volume of this layer (0.0 - 1.0)")]
    [Range(0f, 1f)]
    public float volume = 1.0f;

    [Tooltip("Is this the base layer (always playing)")]
    public bool isBaseLayer = false;

    [Tooltip("Condition for this layer to be active (e.g., 'sanity < 50')")]
    public string activationCondition;

    [Tooltip("Current state of this layer (runtime)")]
    [NonSerialized]
    public bool isActive = false;

    [Tooltip("Audio source for this layer (runtime)")]
    [NonSerialized]
    public AudioSource audioSource;
}

/// <summary>
/// Complete music track definition with multiple layers.
/// </summary>
[Serializable]
public class MusicTrack
{
    [Header("Track Identity")]
    public string trackID;
    public string trackName;
    public MusicTrackType trackType;

    [Header("Layers")]
    [Tooltip("All layers for this track (base + conditional layers)")]
    public MusicLayer[] layers;

    [Header("Transition Settings")]
    [Tooltip("Crossfade duration when transitioning to/from this track")]
    [Range(0f, 10f)]
    public float crossfadeDuration = 2f;

    [Tooltip("Beat-match transitions (align beats when switching)")]
    public bool beatMatch = false;

    [Tooltip("Beats per minute (for beat matching)")]
    public float bpm = 120f;

    [Header("Playback")]
    [Tooltip("Should this track loop")]
    public bool loop = true;

    [Tooltip("Base volume for entire track")]
    [Range(0f, 1f)]
    public float baseVolume = 0.7f;
}

/// <summary>
/// Ambient soundscape layer definition.
/// Multiple layers combine to create rich environmental audio.
/// </summary>
[Serializable]
public class AmbientLayer
{
    public string layerName;
    public AudioClip clip;

    [Range(0f, 1f)]
    public float volume = 0.5f;

    public bool loop = true;

    [Tooltip("Is this layer 3D (has a position in world)")]
    public bool is3D = false;

    [Tooltip("If 3D, the world position of this layer")]
    public Vector3 worldPosition;

    [Tooltip("Minimum distance for 3D audio")]
    public float minDistance = 10f;

    [Tooltip("Maximum distance for 3D audio")]
    public float maxDistance = 100f;

    [Tooltip("Condition for this layer (e.g., 'weather == Rain')")]
    public string condition;

    [NonSerialized]
    public AudioSource audioSource;
}

/// <summary>
/// Audio fade operation data.
/// </summary>
public class AudioFade
{
    public AudioSource source;
    public float startVolume;
    public float targetVolume;
    public float duration;
    public float elapsed;
    public Action onComplete;

    public AudioFade(AudioSource src, float target, float dur, Action callback = null)
    {
        source = src;
        startVolume = src.volume;
        targetVolume = target;
        duration = dur;
        elapsed = 0f;
        onComplete = callback;
    }

    /// <summary>
    /// Update the fade (called each frame).
    /// Returns true when complete.
    /// </summary>
    public bool Update(float deltaTime)
    {
        elapsed += deltaTime;
        float t = Mathf.Clamp01(elapsed / duration);
        source.volume = Mathf.Lerp(startVolume, targetVolume, t);

        if (t >= 1f)
        {
            onComplete?.Invoke();
            return true;
        }
        return false;
    }
}

/// <summary>
/// Audio event data for publishing audio-related events.
/// </summary>
public class AudioEventData
{
    public string soundID;
    public AudioCategory category;
    public Vector3 position;
    public float volume;

    public AudioEventData(string id, AudioCategory cat, Vector3 pos, float vol)
    {
        soundID = id;
        category = cat;
        position = pos;
        volume = vol;
    }
}

/// <summary>
/// Reverb preset definitions for different environments.
/// </summary>
public enum ReverbPreset
{
    None,
    Cave,
    Underwater,
    Forest,
    Mountains,
    City,
    Room,
    LargeHall,
    Cathedral
}

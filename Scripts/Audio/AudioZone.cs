using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Agent 12: Audio System Specialist - AudioZone.cs
/// Defines audio zones in the world with location-specific audio setups.
/// Triggers ambient soundscapes, reverb settings, and music changes when entered.
/// </summary>
[RequireComponent(typeof(Collider))]
public class AudioZone : MonoBehaviour
{
    #region Inspector Settings
    [Header("Zone Identity")]
    [Tooltip("Unique zone identifier")]
    [SerializeField] private string zoneID = "audio_zone_01";

    [Tooltip("Display name for debugging")]
    [SerializeField] private string zoneName = "Audio Zone";

    [Header("Location Settings")]
    [Tooltip("Location ID this zone represents")]
    [SerializeField] private string locationID = "calm_lake";

    [Header("Ambient Audio")]
    [Tooltip("Override ambient soundscape for this zone")]
    [SerializeField] private bool overrideAmbient = false;

    [Tooltip("Ambient layers to activate in this zone")]
    [SerializeField] private List<AmbientLayer> ambientLayers = new List<AmbientLayer>();

    [Header("Music Settings")]
    [Tooltip("Override music track in this zone")]
    [SerializeField] private bool overrideMusic = false;

    [Tooltip("Music track type to play in this zone")]
    [SerializeField] private MusicTrackType musicTrackType = MusicTrackType.Day;

    [Header("Reverb Settings")]
    [Tooltip("Apply reverb preset in this zone")]
    [SerializeField] private bool applyReverb = false;

    [Tooltip("Reverb preset to use")]
    [SerializeField] private ReverbPreset reverbPreset = ReverbPreset.None;

    [Tooltip("Custom reverb settings (if not using preset)")]
    [SerializeField] private AudioReverbZone reverbZone;

    [Header("Volume Modifiers")]
    [Tooltip("Ambient volume multiplier in this zone")]
    [Range(0f, 2f)]
    [SerializeField] private float ambientVolumeMultiplier = 1f;

    [Tooltip("Music volume multiplier in this zone")]
    [Range(0f, 2f)]
    [SerializeField] private float musicVolumeMultiplier = 1f;

    [Header("Trigger Settings")]
    [Tooltip("Tag required to trigger this zone (usually 'Player')")]
    [SerializeField] private string triggerTag = "Player";

    [Tooltip("Fade duration when entering/exiting zone")]
    [Range(0f, 5f)]
    [SerializeField] private float transitionDuration = 1.5f;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogging = false;
    [SerializeField] private bool drawDebugGizmos = true;
    [SerializeField] private Color gizmoColor = new Color(0.3f, 0.7f, 1f, 0.3f);
    #endregion

    #region Private Variables
    private AudioManager audioManager;
    private MusicSystem musicSystem;
    private AmbientSoundscape ambientSoundscape;

    private bool isPlayerInZone = false;
    private MusicTrackType previousMusicTrack;

    private AudioReverbZone activeReverbZone;
    #endregion

    #region Initialization
    private void Start()
    {
        audioManager = AudioManager.Instance;
        musicSystem = FindObjectOfType<MusicSystem>();
        ambientSoundscape = FindObjectOfType<AmbientSoundscape>();

        // Ensure trigger collider
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }

        // Setup reverb zone if needed
        if (applyReverb)
        {
            SetupReverbZone();
        }

        if (enableDebugLogging)
        {
            Debug.Log($"[AudioZone] {zoneName} initialized at {transform.position}");
        }
    }

    /// <summary>
    /// Sets up the audio reverb zone component.
    /// </summary>
    private void SetupReverbZone()
    {
        // Create or get reverb zone
        activeReverbZone = GetComponent<AudioReverbZone>();
        if (activeReverbZone == null)
        {
            activeReverbZone = gameObject.AddComponent<AudioReverbZone>();
        }

        // Apply preset
        ApplyReverbPreset(reverbPreset);
    }

    /// <summary>
    /// Applies a reverb preset to the zone.
    /// </summary>
    private void ApplyReverbPreset(ReverbPreset preset)
    {
        if (activeReverbZone == null) return;

        switch (preset)
        {
            case ReverbPreset.Cave:
                activeReverbZone.reverbPreset = AudioReverbPreset.Cave;
                break;
            case ReverbPreset.Underwater:
                activeReverbZone.reverbPreset = AudioReverbPreset.Underwater;
                break;
            case ReverbPreset.Forest:
                activeReverbZone.reverbPreset = AudioReverbPreset.Forest;
                break;
            case ReverbPreset.Mountains:
                activeReverbZone.reverbPreset = AudioReverbPreset.Mountains;
                break;
            case ReverbPreset.City:
                activeReverbZone.reverbPreset = AudioReverbPreset.City;
                break;
            case ReverbPreset.Room:
                activeReverbZone.reverbPreset = AudioReverbPreset.Room;
                break;
            case ReverbPreset.LargeHall:
                activeReverbZone.reverbPreset = AudioReverbPreset.Hallway;
                break;
            case ReverbPreset.Cathedral:
                activeReverbZone.reverbPreset = AudioReverbPreset.Hallway;
                break;
            default:
                activeReverbZone.reverbPreset = AudioReverbPreset.Off;
                break;
        }
    }
    #endregion

    #region Trigger Events
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(triggerTag)) return;

        isPlayerInZone = true;
        OnZoneEntered();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(triggerTag)) return;

        isPlayerInZone = false;
        OnZoneExited();
    }

    /// <summary>
    /// Called when player enters the audio zone.
    /// </summary>
    private void OnZoneEntered()
    {
        if (enableDebugLogging)
        {
            Debug.Log($"[AudioZone] Entered zone: {zoneName}");
        }

        // Publish location change event
        EventSystem.Publish("LocationChanged", locationID);
        EventSystem.Publish("EnterAudioZone", zoneID);

        // Override ambient soundscape
        if (overrideAmbient && ambientSoundscape != null)
        {
            ambientSoundscape.SetLocation(locationID);
        }

        // Override music
        if (overrideMusic && musicSystem != null)
        {
            previousMusicTrack = musicSystem.GetCurrentTrackType();
            musicSystem.PlayTrack(musicTrackType);
        }

        // Apply volume modifiers (simplified - would need more integration)
        // In production, this would modify AudioManager settings

        if (enableDebugLogging)
        {
            Debug.Log($"[AudioZone] Audio zone '{zoneName}' activated");
        }
    }

    /// <summary>
    /// Called when player exits the audio zone.
    /// </summary>
    private void OnZoneExited()
    {
        if (enableDebugLogging)
        {
            Debug.Log($"[AudioZone] Exited zone: {zoneName}");
        }

        // Publish exit event
        EventSystem.Publish("ExitAudioZone", zoneID);

        // Restore previous music
        if (overrideMusic && musicSystem != null && previousMusicTrack != musicTrackType)
        {
            musicSystem.PlayTrack(previousMusicTrack);
        }

        if (enableDebugLogging)
        {
            Debug.Log($"[AudioZone] Audio zone '{zoneName}' deactivated");
        }
    }
    #endregion

    #region Public API
    /// <summary>
    /// Gets the zone ID.
    /// </summary>
    public string GetZoneID()
    {
        return zoneID;
    }

    /// <summary>
    /// Gets the location ID.
    /// </summary>
    public string GetLocationID()
    {
        return locationID;
    }

    /// <summary>
    /// Checks if player is currently in zone.
    /// </summary>
    public bool IsPlayerInZone()
    {
        return isPlayerInZone;
    }

    /// <summary>
    /// Sets the reverb preset for this zone.
    /// </summary>
    public void SetReverbPreset(ReverbPreset preset)
    {
        reverbPreset = preset;
        if (applyReverb && activeReverbZone != null)
        {
            ApplyReverbPreset(preset);
        }
    }

    /// <summary>
    /// Adds an ambient layer to this zone.
    /// </summary>
    public void AddAmbientLayer(AmbientLayer layer)
    {
        if (layer != null && !ambientLayers.Contains(layer))
        {
            ambientLayers.Add(layer);
        }
    }
    #endregion

    #region Gizmos
    private void OnDrawGizmos()
    {
        if (!drawDebugGizmos) return;

        Collider col = GetComponent<Collider>();
        if (col == null) return;

        Gizmos.color = isPlayerInZone ? Color.green : gizmoColor;

        // Draw based on collider type
        if (col is BoxCollider boxCol)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(boxCol.center, boxCol.size);
            Gizmos.DrawWireCube(boxCol.center, boxCol.size);
        }
        else if (col is SphereCollider sphereCol)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawSphere(sphereCol.center, sphereCol.radius);
            Gizmos.DrawWireSphere(sphereCol.center, sphereCol.radius);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Draw zone name
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position, zoneName);
        #endif
    }
    #endregion

    #region Editor Utilities
#if UNITY_EDITOR
    [ContextMenu("Test Enter Zone")]
    private void TestEnterZone()
    {
        OnZoneEntered();
    }

    [ContextMenu("Test Exit Zone")]
    private void TestExitZone()
    {
        OnZoneExited();
    }
#endif
    #endregion
}

/// <summary>
/// Extension for MusicSystem to get current track type.
/// </summary>
public static class MusicSystemExtensions
{
    public static MusicTrackType GetCurrentTrackType(this MusicSystem musicSystem)
    {
        // This would need to be implemented in MusicSystem
        // For now, return a default
        return MusicTrackType.Day;
    }
}

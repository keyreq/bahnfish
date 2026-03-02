using UnityEngine;

/// <summary>
/// Agent 12: Audio System Specialist - PositionalAudio.cs
/// Component for 3D spatial audio attached to game objects.
/// Handles distance attenuation, Doppler effect, occlusion, and reverb.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class PositionalAudio : MonoBehaviour
{
    #region Inspector Settings
    [Header("Audio Clip")]
    [Tooltip("Audio clip to play (can also use AudioClipData ID)")]
    [SerializeField] private AudioClip audioClip;

    [Tooltip("Audio clip data ID for SFX library lookup")]
    [SerializeField] private string audioClipID;

    [Header("Playback Settings")]
    [Tooltip("Play on awake")]
    [SerializeField] private bool playOnAwake = false;

    [Tooltip("Loop the audio")]
    [SerializeField] private bool loop = false;

    [Tooltip("Base volume")]
    [Range(0f, 1f)]
    [SerializeField] private float volume = 1.0f;

    [Header("3D Audio Settings")]
    [Tooltip("Minimum distance for attenuation")]
    [SerializeField] private float minDistance = 5f;

    [Tooltip("Maximum distance (inaudible beyond this)")]
    [SerializeField] private float maxDistance = 50f;

    [Tooltip("Doppler effect intensity")]
    [Range(0f, 1f)]
    [SerializeField] private float dopplerLevel = 0.5f;

    [Header("Occlusion Settings")]
    [Tooltip("Enable audio occlusion (muffled through walls)")]
    [SerializeField] private bool enableOcclusion = false;

    [Tooltip("Layer mask for occlusion raycasts")]
    [SerializeField] private LayerMask occlusionMask = -1;

    [Tooltip("Volume multiplier when occluded")]
    [Range(0f, 1f)]
    [SerializeField] private float occludedVolumeMultiplier = 0.3f;

    [Tooltip("Low-pass filter frequency when occluded (Hz)")]
    [SerializeField] private float occludedLowPassFrequency = 1000f;

    [Header("Movement Tracking")]
    [Tooltip("Track movement for Doppler effect")]
    [SerializeField] private bool trackMovement = true;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogging = false;
    [SerializeField] private bool drawDebugGizmos = false;
    #endregion

    #region Private Variables
    private AudioSource audioSource;
    private AudioManager audioManager;
    private SoundEffectManager sfxManager;
    private AudioLowPassFilter lowPassFilter;

    // Occlusion state
    private bool isOccluded = false;
    private float targetVolume;
    private float currentVolume;

    // Movement tracking
    private Vector3 lastPosition;
    private Vector3 velocity;

    // Audio listener reference
    private Transform audioListener;
    #endregion

    #region Initialization
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        ConfigureAudioSource();
    }

    private void Start()
    {
        audioManager = AudioManager.Instance;
        sfxManager = FindObjectOfType<SoundEffectManager>();

        // Find audio listener
        audioListener = Camera.main?.transform;
        if (audioListener == null)
        {
            AudioListener listener = FindObjectOfType<AudioListener>();
            if (listener != null)
                audioListener = listener.transform;
        }

        // Get clip from SFX library if ID specified
        if (!string.IsNullOrEmpty(audioClipID) && sfxManager != null)
        {
            AudioClipData clipData = sfxManager.GetSoundEffect(audioClipID);
            if (clipData != null && clipData.clip != null)
            {
                audioClip = clipData.clip;
                audioSource.clip = audioClip;
            }
        }

        // Setup low-pass filter for occlusion
        if (enableOcclusion)
        {
            lowPassFilter = gameObject.AddComponent<AudioLowPassFilter>();
            lowPassFilter.cutoffFrequency = 22000f; // No filtering by default
        }

        // Play on awake
        if (playOnAwake && audioClip != null)
        {
            Play();
        }

        lastPosition = transform.position;
    }

    /// <summary>
    /// Configures the AudioSource component for 3D spatial audio.
    /// </summary>
    private void ConfigureAudioSource()
    {
        audioSource.playOnAwake = false;
        audioSource.loop = loop;
        audioSource.volume = volume;
        audioSource.spatialBlend = 1f; // Full 3D
        audioSource.minDistance = minDistance;
        audioSource.maxDistance = maxDistance;
        audioSource.dopplerLevel = dopplerLevel;
        audioSource.rolloffMode = AudioRolloffMode.Linear;

        if (audioClip != null)
        {
            audioSource.clip = audioClip;
        }
    }
    #endregion

    #region Update
    private void Update()
    {
        if (!audioSource.isPlaying) return;

        // Update occlusion
        if (enableOcclusion && audioListener != null)
        {
            UpdateOcclusion();
        }

        // Update movement velocity for Doppler
        if (trackMovement)
        {
            UpdateVelocity();
        }

        // Smooth volume transitions
        if (Mathf.Abs(currentVolume - targetVolume) > 0.01f)
        {
            currentVolume = Mathf.Lerp(currentVolume, targetVolume, Time.deltaTime * 5f);
            audioSource.volume = currentVolume;
        }
    }

    /// <summary>
    /// Updates audio occlusion based on line-of-sight to listener.
    /// </summary>
    private void UpdateOcclusion()
    {
        Vector3 directionToListener = audioListener.position - transform.position;
        float distanceToListener = directionToListener.magnitude;

        // Raycast to check for obstacles
        bool wasOccluded = isOccluded;
        isOccluded = Physics.Raycast(transform.position, directionToListener.normalized,
                                      distanceToListener, occlusionMask);

        // Apply occlusion effects
        if (isOccluded)
        {
            targetVolume = volume * occludedVolumeMultiplier;
            if (lowPassFilter != null)
            {
                lowPassFilter.cutoffFrequency = occludedLowPassFrequency;
            }
        }
        else
        {
            targetVolume = volume;
            if (lowPassFilter != null)
            {
                lowPassFilter.cutoffFrequency = 22000f;
            }
        }

        // Log occlusion state changes
        if (enableDebugLogging && wasOccluded != isOccluded)
        {
            Debug.Log($"[PositionalAudio] {gameObject.name} occlusion: {isOccluded}");
        }
    }

    /// <summary>
    /// Updates velocity for Doppler effect calculation.
    /// </summary>
    private void UpdateVelocity()
    {
        velocity = (transform.position - lastPosition) / Time.deltaTime;
        lastPosition = transform.position;
    }
    #endregion

    #region Playback Control
    /// <summary>
    /// Plays the positional audio.
    /// </summary>
    public void Play()
    {
        if (audioSource == null || audioClip == null)
        {
            if (enableDebugLogging)
                Debug.LogWarning($"[PositionalAudio] Cannot play - missing audio source or clip");
            return;
        }

        targetVolume = volume;
        currentVolume = volume;
        audioSource.Play();

        if (enableDebugLogging)
        {
            Debug.Log($"[PositionalAudio] Playing {audioClip.name} at {transform.position}");
        }
    }

    /// <summary>
    /// Plays with a specified delay.
    /// </summary>
    public void PlayDelayed(float delay)
    {
        if (audioSource != null && audioClip != null)
        {
            audioSource.PlayDelayed(delay);
        }
    }

    /// <summary>
    /// Stops the audio.
    /// </summary>
    public void Stop()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }

    /// <summary>
    /// Pauses the audio.
    /// </summary>
    public void Pause()
    {
        if (audioSource != null)
        {
            audioSource.Pause();
        }
    }

    /// <summary>
    /// Unpauses the audio.
    /// </summary>
    public void UnPause()
    {
        if (audioSource != null)
        {
            audioSource.UnPause();
        }
    }

    /// <summary>
    /// Fades out and stops.
    /// </summary>
    public void FadeOut(float duration)
    {
        if (audioManager != null)
        {
            audioManager.FadeAudioSource(audioSource, 0f, duration, Stop);
        }
        else
        {
            Stop();
        }
    }
    #endregion

    #region Configuration
    /// <summary>
    /// Sets the audio clip.
    /// </summary>
    public void SetAudioClip(AudioClip clip)
    {
        audioClip = clip;
        if (audioSource != null)
        {
            audioSource.clip = clip;
        }
    }

    /// <summary>
    /// Sets the audio clip by SFX ID.
    /// </summary>
    public void SetAudioClipByID(string clipID)
    {
        audioClipID = clipID;
        if (sfxManager != null)
        {
            AudioClipData clipData = sfxManager.GetSoundEffect(clipID);
            if (clipData != null && clipData.clip != null)
            {
                SetAudioClip(clipData.clip);
            }
        }
    }

    /// <summary>
    /// Sets the volume.
    /// </summary>
    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
        targetVolume = volume;
        if (!isOccluded)
        {
            audioSource.volume = volume;
        }
    }

    /// <summary>
    /// Sets the 3D audio distances.
    /// </summary>
    public void SetDistances(float min, float max)
    {
        minDistance = min;
        maxDistance = max;
        if (audioSource != null)
        {
            audioSource.minDistance = min;
            audioSource.maxDistance = max;
        }
    }

    /// <summary>
    /// Enables or disables looping.
    /// </summary>
    public void SetLoop(bool shouldLoop)
    {
        loop = shouldLoop;
        if (audioSource != null)
        {
            audioSource.loop = shouldLoop;
        }
    }
    #endregion

    #region Queries
    /// <summary>
    /// Checks if the audio is currently playing.
    /// </summary>
    public bool IsPlaying()
    {
        return audioSource != null && audioSource.isPlaying;
    }

    /// <summary>
    /// Gets the distance to the audio listener.
    /// </summary>
    public float GetDistanceToListener()
    {
        if (audioListener == null) return 0f;
        return Vector3.Distance(transform.position, audioListener.position);
    }

    /// <summary>
    /// Gets the current velocity (for Doppler).
    /// </summary>
    public Vector3 GetVelocity()
    {
        return velocity;
    }

    /// <summary>
    /// Checks if currently occluded.
    /// </summary>
    public bool IsOccluded()
    {
        return isOccluded;
    }
    #endregion

    #region Gizmos
    private void OnDrawGizmos()
    {
        if (!drawDebugGizmos) return;

        // Draw min distance sphere
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, minDistance);

        // Draw max distance sphere
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, maxDistance);

        // Draw line to listener if occluded
        if (enableOcclusion && audioListener != null && Application.isPlaying)
        {
            Gizmos.color = isOccluded ? Color.red : Color.green;
            Gizmos.DrawLine(transform.position, audioListener.position);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Always draw ranges when selected
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, minDistance);
        Gizmos.color = Color.orange;
        Gizmos.DrawWireSphere(transform.position, maxDistance);
    }
    #endregion
}

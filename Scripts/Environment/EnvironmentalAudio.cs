using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages ambient environmental audio based on time of day and weather conditions
/// Implements smooth transitions and layered soundscapes
/// Audio design based on VIDEO_ANALYSIS.md specifications
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class EnvironmentalAudio : MonoBehaviour
{
    [Header("Audio Sources")]
    [Tooltip("Primary ambient audio source")]
    public AudioSource ambientSource;

    [Tooltip("Secondary layer for weather sounds")]
    public AudioSource weatherSource;

    [Tooltip("Tertiary layer for special effects")]
    public AudioSource effectsSource;

    [Header("Day Audio Clips")]
    [Tooltip("Peaceful daytime ambient (water, seagulls, gentle waves)")]
    public AudioClip dayAmbient;

    [Tooltip("Bird chirping sounds for day")]
    public AudioClip[] birdSounds;

    [Tooltip("Gentle wave sounds")]
    public AudioClip gentleWaves;

    [Header("Dusk/Dawn Audio Clips")]
    [Tooltip("Transitional ambient sounds")]
    public AudioClip duskAmbient;

    [Tooltip("Evening birds/crickets")]
    public AudioClip[] eveningSounds;

    [Header("Night Audio Clips")]
    [Tooltip("Eerie night ambient with distortion")]
    public AudioClip nightAmbient;

    [Tooltip("Distant eerie sounds")]
    public AudioClip[] eerySounds;

    [Tooltip("Deep bass rumbles for night")]
    public AudioClip deepRumble;

    [Header("Weather Audio Clips")]
    [Tooltip("Rain sound loop")]
    public AudioClip rainSound;

    [Tooltip("Storm with thunder")]
    public AudioClip stormSound;

    [Tooltip("Foghorn and muffled sounds")]
    public AudioClip fogSound;

    [Header("Volume Settings")]
    [Range(0f, 1f)] public float masterVolume = 0.7f;
    [Range(0f, 1f)] public float ambientVolume = 0.8f;
    [Range(0f, 1f)] public float weatherVolume = 0.6f;
    [Range(0f, 1f)] public float effectsVolume = 0.5f;

    [Header("Transition Settings")]
    [Tooltip("Crossfade duration in seconds")]
    [Range(0.5f, 10f)]
    public float crossfadeDuration = 3f;

    [Header("Audio Effects")]
    [Tooltip("Enable pitch shifting at night")]
    public bool enableNightPitchShift = true;

    [Range(0.5f, 1f)]
    public float nightPitchMin = 0.85f;

    [Tooltip("Enable reverb at night")]
    public bool enableNightReverb = true;

    [Tooltip("Enable low-pass filter in fog")]
    public bool enableFogFilter = true;

    [Header("Random Ambient Sounds")]
    [Tooltip("Enable random ambient sound effects")]
    public bool playRandomSounds = true;

    [Range(5f, 60f)]
    public float minTimeBetweenSounds = 10f;

    [Range(10f, 120f)]
    public float maxTimeBetweenSounds = 30f;

    // Private variables
    private TimeManager timeManager;
    private WeatherSystem weatherSystem;
    private TimeOfDay currentTimeOfDay;
    private WeatherType currentWeather;

    // Audio filter effects
    private AudioReverbFilter reverbFilter;
    private AudioLowPassFilter lowPassFilter;

    // Crossfade tracking
    private bool isCrossfading = false;
    private float crossfadeProgress = 0f;
    private AudioClip targetClip;
    private float targetVolume;

    // Random sound timer
    private float nextRandomSoundTime;

    // Singleton pattern
    private static EnvironmentalAudio _instance;
    public static EnvironmentalAudio Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<EnvironmentalAudio>();
            }
            return _instance;
        }
    }

    private void Awake()
    {
        // Singleton setup
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        // Setup audio sources
        if (ambientSource == null)
        {
            ambientSource = GetComponent<AudioSource>();
        }

        if (weatherSource == null)
        {
            weatherSource = gameObject.AddComponent<AudioSource>();
        }

        if (effectsSource == null)
        {
            effectsSource = gameObject.AddComponent<AudioSource>();
        }

        // Configure audio sources
        ConfigureAudioSource(ambientSource, true, ambientVolume);
        ConfigureAudioSource(weatherSource, true, weatherVolume);
        ConfigureAudioSource(effectsSource, false, effectsVolume);

        // Setup audio filters
        SetupAudioFilters();

        // Get manager references
        timeManager = TimeManager.Instance;
        weatherSystem = WeatherSystem.Instance;

        if (timeManager != null)
        {
            currentTimeOfDay = timeManager.GetCurrentTimeOfDay();
        }

        if (weatherSystem != null)
        {
            currentWeather = weatherSystem.GetCurrentWeather();
        }

        // Subscribe to events
        EventSystem.Subscribe<TimeOfDay>("TimeOfDayChanged", OnTimeOfDayChanged);
        EventSystem.Subscribe<WeatherChangedEventData>("OnWeatherChanged", OnWeatherChanged);

        // Play initial ambient
        PlayAmbientForTimeOfDay(currentTimeOfDay, immediate: true);
        PlayWeatherSound(currentWeather, immediate: true);

        // Schedule first random sound
        ScheduleNextRandomSound();

        Debug.Log("EnvironmentalAudio initialized");
    }

    private void ConfigureAudioSource(AudioSource source, bool loop, float volume)
    {
        source.loop = loop;
        source.playOnAwake = false;
        source.volume = volume * masterVolume;
        source.spatialBlend = 0f; // 2D audio for ambient
    }

    private void SetupAudioFilters()
    {
        // Add reverb filter for night atmosphere
        reverbFilter = gameObject.AddComponent<AudioReverbFilter>();
        reverbFilter.enabled = false;
        reverbFilter.reverbPreset = AudioReverbPreset.Cave;
        reverbFilter.dryLevel = -1000f;
        reverbFilter.room = -1000f;
        reverbFilter.roomHF = -100f;
        reverbFilter.roomLF = 0f;
        reverbFilter.decayTime = 2.5f;

        // Add low-pass filter for fog
        lowPassFilter = gameObject.AddComponent<AudioLowPassFilter>();
        lowPassFilter.enabled = false;
        lowPassFilter.cutoffFrequency = 5000f;
    }

    private void Update()
    {
        UpdateCrossfade();
        UpdateAudioEffects();
        UpdateRandomSounds();
    }

    /// <summary>
    /// Update smooth crossfade between audio clips
    /// </summary>
    private void UpdateCrossfade()
    {
        if (isCrossfading)
        {
            crossfadeProgress += Time.deltaTime / crossfadeDuration;

            if (crossfadeProgress >= 1f)
            {
                // Crossfade complete
                crossfadeProgress = 1f;
                isCrossfading = false;
                ambientSource.volume = targetVolume * masterVolume;
            }
            else
            {
                // Update volume during crossfade
                ambientSource.volume = Mathf.Lerp(0f, targetVolume, crossfadeProgress) * masterVolume;
            }
        }
    }

    /// <summary>
    /// Update audio effects based on time and weather
    /// </summary>
    private void UpdateAudioEffects()
    {
        if (timeManager == null) return;

        TimeOfDay tod = timeManager.GetCurrentTimeOfDay();

        // Night pitch shift
        if (enableNightPitchShift)
        {
            if (tod == TimeOfDay.Night)
            {
                ambientSource.pitch = nightPitchMin;
            }
            else if (tod == TimeOfDay.Dusk)
            {
                // Gradually lower pitch
                ambientSource.pitch = Mathf.Lerp(1f, nightPitchMin, 0.5f);
            }
            else
            {
                ambientSource.pitch = 1f;
            }
        }

        // Night reverb
        if (enableNightReverb && reverbFilter != null)
        {
            reverbFilter.enabled = (tod == TimeOfDay.Night);
        }

        // Fog low-pass filter
        if (enableFogFilter && lowPassFilter != null && weatherSystem != null)
        {
            bool isFoggy = weatherSystem.IsFoggy();
            lowPassFilter.enabled = isFoggy;

            if (isFoggy)
            {
                float intensity = weatherSystem.GetWeatherIntensity();
                lowPassFilter.cutoffFrequency = Mathf.Lerp(22000f, 3000f, intensity);
            }
        }
    }

    /// <summary>
    /// Play random ambient sounds at intervals
    /// </summary>
    private void UpdateRandomSounds()
    {
        if (!playRandomSounds || effectsSource == null) return;

        if (Time.time >= nextRandomSoundTime)
        {
            PlayRandomAmbientSound();
            ScheduleNextRandomSound();
        }
    }

    private void ScheduleNextRandomSound()
    {
        nextRandomSoundTime = Time.time + Random.Range(minTimeBetweenSounds, maxTimeBetweenSounds);
    }

    /// <summary>
    /// Play a random ambient sound based on current time
    /// </summary>
    private void PlayRandomAmbientSound()
    {
        AudioClip clip = null;

        switch (currentTimeOfDay)
        {
            case TimeOfDay.Day:
            case TimeOfDay.Dawn:
                if (birdSounds != null && birdSounds.Length > 0)
                    clip = birdSounds[Random.Range(0, birdSounds.Length)];
                break;

            case TimeOfDay.Dusk:
                if (eveningSounds != null && eveningSounds.Length > 0)
                    clip = eveningSounds[Random.Range(0, eveningSounds.Length)];
                break;

            case TimeOfDay.Night:
                if (eerySounds != null && eerySounds.Length > 0)
                    clip = eerySounds[Random.Range(0, eerySounds.Length)];
                break;
        }

        if (clip != null && effectsSource != null)
        {
            effectsSource.PlayOneShot(clip, effectsVolume * masterVolume);
        }
    }

    /// <summary>
    /// Called when time of day changes
    /// </summary>
    private void OnTimeOfDayChanged(TimeOfDay newTimeOfDay)
    {
        currentTimeOfDay = newTimeOfDay;
        PlayAmbientForTimeOfDay(newTimeOfDay, immediate: false);
        Debug.Log($"EnvironmentalAudio: Time changed to {newTimeOfDay}");
    }

    /// <summary>
    /// Called when weather changes
    /// </summary>
    private void OnWeatherChanged(WeatherChangedEventData eventData)
    {
        currentWeather = eventData.currentWeather;
        PlayWeatherSound(eventData.currentWeather, immediate: false);
        Debug.Log($"EnvironmentalAudio: Weather changed to {eventData.currentWeather}");
    }

    /// <summary>
    /// Play ambient sound for specific time of day
    /// </summary>
    private void PlayAmbientForTimeOfDay(TimeOfDay timeOfDay, bool immediate)
    {
        AudioClip clipToPlay = null;
        float volume = ambientVolume;

        switch (timeOfDay)
        {
            case TimeOfDay.Day:
                clipToPlay = dayAmbient;
                volume = ambientVolume;
                break;

            case TimeOfDay.Dawn:
            case TimeOfDay.Dusk:
                clipToPlay = duskAmbient;
                volume = ambientVolume * 0.9f;
                break;

            case TimeOfDay.Night:
                clipToPlay = nightAmbient;
                volume = ambientVolume * 0.8f;
                break;
        }

        if (clipToPlay != null)
        {
            PlayAmbientClip(clipToPlay, volume, immediate);
        }
    }

    /// <summary>
    /// Play weather-specific sound
    /// </summary>
    private void PlayWeatherSound(WeatherType weather, bool immediate)
    {
        AudioClip clipToPlay = null;
        float volume = weatherVolume;

        switch (weather)
        {
            case WeatherType.Clear:
                // Stop weather sounds for clear weather
                if (weatherSource.isPlaying)
                {
                    FadeOutAudioSource(weatherSource, immediate ? 0f : 2f);
                }
                return;

            case WeatherType.Rain:
                clipToPlay = rainSound;
                volume = weatherVolume * 0.6f;
                break;

            case WeatherType.Storm:
                clipToPlay = stormSound;
                volume = weatherVolume * 0.8f;
                break;

            case WeatherType.Fog:
                clipToPlay = fogSound;
                volume = weatherVolume * 0.4f;
                break;
        }

        if (clipToPlay != null && weatherSource != null)
        {
            if (immediate)
            {
                weatherSource.clip = clipToPlay;
                weatherSource.volume = volume * masterVolume;
                weatherSource.Play();
            }
            else
            {
                StartCoroutine(CrossfadeAudioSource(weatherSource, clipToPlay, volume * masterVolume, crossfadeDuration));
            }
        }
    }

    /// <summary>
    /// Play an ambient clip with optional crossfade
    /// </summary>
    private void PlayAmbientClip(AudioClip clip, float volume, bool immediate)
    {
        if (ambientSource == null || clip == null) return;

        if (immediate)
        {
            ambientSource.clip = clip;
            ambientSource.volume = volume * masterVolume;
            ambientSource.Play();
            isCrossfading = false;
        }
        else
        {
            // Start crossfade
            targetClip = clip;
            targetVolume = volume;
            crossfadeProgress = 0f;
            isCrossfading = true;

            // Fade out current, then start new
            StartCoroutine(CrossfadeAmbient(clip, volume));
        }
    }

    private System.Collections.IEnumerator CrossfadeAmbient(AudioClip newClip, float newVolume)
    {
        // Fade out
        float startVolume = ambientSource.volume;
        float elapsed = 0f;

        while (elapsed < crossfadeDuration / 2f)
        {
            elapsed += Time.deltaTime;
            ambientSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / (crossfadeDuration / 2f));
            yield return null;
        }

        // Switch clip
        ambientSource.clip = newClip;
        ambientSource.Play();

        // Fade in
        elapsed = 0f;
        while (elapsed < crossfadeDuration / 2f)
        {
            elapsed += Time.deltaTime;
            ambientSource.volume = Mathf.Lerp(0f, newVolume * masterVolume, elapsed / (crossfadeDuration / 2f));
            yield return null;
        }

        ambientSource.volume = newVolume * masterVolume;
        isCrossfading = false;
    }

    private System.Collections.IEnumerator CrossfadeAudioSource(AudioSource source, AudioClip newClip, float newVolume, float duration)
    {
        float startVolume = source.volume;
        float elapsed = 0f;

        // Fade out
        while (elapsed < duration / 2f)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, 0f, elapsed / (duration / 2f));
            yield return null;
        }

        // Switch clip
        source.clip = newClip;
        source.Play();

        // Fade in
        elapsed = 0f;
        while (elapsed < duration / 2f)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(0f, newVolume, elapsed / (duration / 2f));
            yield return null;
        }

        source.volume = newVolume;
    }

    private void FadeOutAudioSource(AudioSource source, float duration)
    {
        StartCoroutine(FadeOutCoroutine(source, duration));
    }

    private System.Collections.IEnumerator FadeOutCoroutine(AudioSource source, float duration)
    {
        float startVolume = source.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            yield return null;
        }

        source.Stop();
        source.volume = startVolume;
    }

    #region Public Interface

    /// <summary>
    /// Set master volume
    /// </summary>
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        UpdateAllVolumes();
    }

    /// <summary>
    /// Set ambient volume
    /// </summary>
    public void SetAmbientVolume(float volume)
    {
        ambientVolume = Mathf.Clamp01(volume);
        if (ambientSource != null)
            ambientSource.volume = ambientVolume * masterVolume;
    }

    /// <summary>
    /// Set weather volume
    /// </summary>
    public void SetWeatherVolume(float volume)
    {
        weatherVolume = Mathf.Clamp01(volume);
        if (weatherSource != null)
            weatherSource.volume = weatherVolume * masterVolume;
    }

    /// <summary>
    /// Mute/unmute all audio
    /// </summary>
    public void SetMuted(bool muted)
    {
        if (ambientSource != null) ambientSource.mute = muted;
        if (weatherSource != null) weatherSource.mute = muted;
        if (effectsSource != null) effectsSource.mute = muted;
    }

    private void UpdateAllVolumes()
    {
        if (ambientSource != null)
            ambientSource.volume = ambientVolume * masterVolume;
        if (weatherSource != null)
            weatherSource.volume = weatherVolume * masterVolume;
        if (effectsSource != null)
            effectsSource.volume = effectsVolume * masterVolume;
    }

    #endregion

    private void OnDestroy()
    {
        // Unsubscribe from events
        EventSystem.Unsubscribe<TimeOfDay>("TimeOfDayChanged", OnTimeOfDayChanged);
        EventSystem.Unsubscribe<WeatherChangedEventData>("OnWeatherChanged", OnWeatherChanged);
    }
}

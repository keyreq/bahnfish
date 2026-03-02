using UnityEngine;
using System;

/// <summary>
/// Manages audio settings.
/// Features:
/// - Master, Music, SFX, Ambient, UI volume controls (0-100%)
/// - Mute toggles (global and per-channel)
/// - Audio device selection
/// - Subtitle settings (on/off, size, background opacity)
/// - Real-time application
/// </summary>
public class AudioSettings : MonoBehaviour
{
    #region Enums

    public enum SubtitleSize
    {
        Small,
        Medium,
        Large
    }

    #endregion

    #region Settings Variables

    [Header("Volume (0-1)")]
    [Range(0f, 1f)]
    public float masterVolume = 1f;
    [Range(0f, 1f)]
    public float musicVolume = 0.7f;
    [Range(0f, 1f)]
    public float sfxVolume = 0.8f;
    [Range(0f, 1f)]
    public float ambientVolume = 0.6f;
    [Range(0f, 1f)]
    public float uiVolume = 0.5f;

    [Header("Mute Toggles")]
    public bool muteAll = false;
    public bool muteMusic = false;
    public bool muteSFX = false;
    public bool muteAmbient = false;
    public bool muteUI = false;

    [Header("Subtitles")]
    public bool subtitlesEnabled = true;
    public SubtitleSize subtitleSize = SubtitleSize.Medium;
    [Range(0f, 1f)]
    public float subtitleBackgroundOpacity = 0.7f;

    [Header("Audio Device")]
    public int selectedAudioDevice = 0; // Index in available devices

    #endregion

    private bool isInitialized = false;

    /// <summary>
    /// Initialize audio settings.
    /// </summary>
    public void Initialize()
    {
        if (isInitialized) return;

        isInitialized = true;
        Debug.Log("[AudioSettings] Initialized");
    }

    #region Apply Settings

    /// <summary>
    /// Apply all audio settings.
    /// </summary>
    public void ApplySettings()
    {
        ApplyVolumeSettings();
        ApplySubtitleSettings();
        ApplyAudioDevice();

        Debug.Log("[AudioSettings] Settings applied");
        EventSystem.Publish("AudioSettingsApplied", true);
    }

    /// <summary>
    /// Apply volume settings to audio mixer.
    /// </summary>
    private void ApplyVolumeSettings()
    {
        // Calculate effective volumes (considering mutes)
        float effectiveMaster = muteAll ? 0f : masterVolume;
        float effectiveMusic = (muteAll || muteMusic) ? 0f : musicVolume * masterVolume;
        float effectiveSFX = (muteAll || muteSFX) ? 0f : sfxVolume * masterVolume;
        float effectiveAmbient = (muteAll || muteAmbient) ? 0f : ambientVolume * masterVolume;
        float effectiveUI = (muteAll || muteUI) ? 0f : uiVolume * masterVolume;

        // Publish volume changes for audio system to handle
        EventSystem.Publish("SetMasterVolume", effectiveMaster);
        EventSystem.Publish("SetMusicVolume", effectiveMusic);
        EventSystem.Publish("SetSFXVolume", effectiveSFX);
        EventSystem.Publish("SetAmbientVolume", effectiveAmbient);
        EventSystem.Publish("SetUIVolume", effectiveUI);

        // Also set Unity's global volume
        AudioListener.volume = effectiveMaster;
    }

    /// <summary>
    /// Apply subtitle settings.
    /// </summary>
    private void ApplySubtitleSettings()
    {
        EventSystem.Publish("SetSubtitlesEnabled", subtitlesEnabled);
        EventSystem.Publish("SetSubtitleSize", subtitleSize);
        EventSystem.Publish("SetSubtitleBackgroundOpacity", subtitleBackgroundOpacity);
    }

    /// <summary>
    /// Apply audio device selection.
    /// </summary>
    private void ApplyAudioDevice()
    {
        // Audio device selection is handled by Unity's AudioSettings
        // This would require Unity's Audio Settings API
        EventSystem.Publish("SetAudioDevice", selectedAudioDevice);
    }

    #endregion

    #region Save/Load

    /// <summary>
    /// Save settings to PlayerPrefs.
    /// </summary>
    public void SaveSettings()
    {
        PlayerPrefs.SetFloat("Audio_MasterVolume", masterVolume);
        PlayerPrefs.SetFloat("Audio_MusicVolume", musicVolume);
        PlayerPrefs.SetFloat("Audio_SFXVolume", sfxVolume);
        PlayerPrefs.SetFloat("Audio_AmbientVolume", ambientVolume);
        PlayerPrefs.SetFloat("Audio_UIVolume", uiVolume);

        PlayerPrefs.SetInt("Audio_MuteAll", muteAll ? 1 : 0);
        PlayerPrefs.SetInt("Audio_MuteMusic", muteMusic ? 1 : 0);
        PlayerPrefs.SetInt("Audio_MuteSFX", muteSFX ? 1 : 0);
        PlayerPrefs.SetInt("Audio_MuteAmbient", muteAmbient ? 1 : 0);
        PlayerPrefs.SetInt("Audio_MuteUI", muteUI ? 1 : 0);

        PlayerPrefs.SetInt("Audio_SubtitlesEnabled", subtitlesEnabled ? 1 : 0);
        PlayerPrefs.SetInt("Audio_SubtitleSize", (int)subtitleSize);
        PlayerPrefs.SetFloat("Audio_SubtitleBackgroundOpacity", subtitleBackgroundOpacity);

        PlayerPrefs.SetInt("Audio_SelectedDevice", selectedAudioDevice);
    }

    /// <summary>
    /// Load settings from PlayerPrefs.
    /// </summary>
    public void LoadSettings()
    {
        masterVolume = PlayerPrefs.GetFloat("Audio_MasterVolume", 1f);
        musicVolume = PlayerPrefs.GetFloat("Audio_MusicVolume", 0.7f);
        sfxVolume = PlayerPrefs.GetFloat("Audio_SFXVolume", 0.8f);
        ambientVolume = PlayerPrefs.GetFloat("Audio_AmbientVolume", 0.6f);
        uiVolume = PlayerPrefs.GetFloat("Audio_UIVolume", 0.5f);

        muteAll = PlayerPrefs.GetInt("Audio_MuteAll", 0) == 1;
        muteMusic = PlayerPrefs.GetInt("Audio_MuteMusic", 0) == 1;
        muteSFX = PlayerPrefs.GetInt("Audio_MuteSFX", 0) == 1;
        muteAmbient = PlayerPrefs.GetInt("Audio_MuteAmbient", 0) == 1;
        muteUI = PlayerPrefs.GetInt("Audio_MuteUI", 0) == 1;

        subtitlesEnabled = PlayerPrefs.GetInt("Audio_SubtitlesEnabled", 1) == 1;
        subtitleSize = (SubtitleSize)PlayerPrefs.GetInt("Audio_SubtitleSize", 1);
        subtitleBackgroundOpacity = PlayerPrefs.GetFloat("Audio_SubtitleBackgroundOpacity", 0.7f);

        selectedAudioDevice = PlayerPrefs.GetInt("Audio_SelectedDevice", 0);
    }

    #endregion

    #region Reset

    /// <summary>
    /// Reset to default settings.
    /// </summary>
    public void ResetToDefaults()
    {
        masterVolume = 1f;
        musicVolume = 0.7f;
        sfxVolume = 0.8f;
        ambientVolume = 0.6f;
        uiVolume = 0.5f;

        muteAll = false;
        muteMusic = false;
        muteSFX = false;
        muteAmbient = false;
        muteUI = false;

        subtitlesEnabled = true;
        subtitleSize = SubtitleSize.Medium;
        subtitleBackgroundOpacity = 0.7f;

        selectedAudioDevice = 0;
    }

    #endregion

    #region Data Transfer

    /// <summary>
    /// Get settings data for save system integration.
    /// </summary>
    public AudioSettingsData GetData()
    {
        return new AudioSettingsData
        {
            masterVolume = masterVolume,
            musicVolume = musicVolume,
            sfxVolume = sfxVolume,
            ambientVolume = ambientVolume,
            uiVolume = uiVolume,
            muteAll = muteAll,
            muteMusic = muteMusic,
            muteSFX = muteSFX,
            muteAmbient = muteAmbient,
            muteUI = muteUI,
            subtitlesEnabled = subtitlesEnabled,
            subtitleSize = (int)subtitleSize,
            subtitleBackgroundOpacity = subtitleBackgroundOpacity,
            selectedAudioDevice = selectedAudioDevice
        };
    }

    /// <summary>
    /// Set settings from data structure.
    /// </summary>
    public void SetData(AudioSettingsData data)
    {
        if (data == null) return;

        masterVolume = data.masterVolume;
        musicVolume = data.musicVolume;
        sfxVolume = data.sfxVolume;
        ambientVolume = data.ambientVolume;
        uiVolume = data.uiVolume;

        muteAll = data.muteAll;
        muteMusic = data.muteMusic;
        muteSFX = data.muteSFX;
        muteAmbient = data.muteAmbient;
        muteUI = data.muteUI;

        subtitlesEnabled = data.subtitlesEnabled;
        subtitleSize = (SubtitleSize)data.subtitleSize;
        subtitleBackgroundOpacity = data.subtitleBackgroundOpacity;

        selectedAudioDevice = data.selectedAudioDevice;
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Set master volume and apply.
    /// </summary>
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        ApplyVolumeSettings();
        EventSystem.Publish("SettingChanged", new SettingChangedEvent("MasterVolume", masterVolume));
    }

    /// <summary>
    /// Set music volume and apply.
    /// </summary>
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        ApplyVolumeSettings();
        EventSystem.Publish("SettingChanged", new SettingChangedEvent("MusicVolume", musicVolume));
    }

    /// <summary>
    /// Set SFX volume and apply.
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        ApplyVolumeSettings();
        EventSystem.Publish("SettingChanged", new SettingChangedEvent("SFXVolume", sfxVolume));
    }

    /// <summary>
    /// Toggle mute all.
    /// </summary>
    public void ToggleMuteAll()
    {
        muteAll = !muteAll;
        ApplyVolumeSettings();
        EventSystem.Publish("SettingChanged", new SettingChangedEvent("MuteAll", muteAll));
    }

    #endregion
}

/// <summary>
/// Serializable audio settings data.
/// </summary>
[Serializable]
public class AudioSettingsData
{
    public float masterVolume = 1f;
    public float musicVolume = 0.7f;
    public float sfxVolume = 0.8f;
    public float ambientVolume = 0.6f;
    public float uiVolume = 0.5f;

    public bool muteAll = false;
    public bool muteMusic = false;
    public bool muteSFX = false;
    public bool muteAmbient = false;
    public bool muteUI = false;

    public bool subtitlesEnabled = true;
    public int subtitleSize = 1;
    public float subtitleBackgroundOpacity = 0.7f;

    public int selectedAudioDevice = 0;
}

/// <summary>
/// Setting changed event data.
/// </summary>
[Serializable]
public class SettingChangedEvent
{
    public string settingName;
    public object value;

    public SettingChangedEvent(string name, object val)
    {
        settingName = name;
        value = val;
    }
}

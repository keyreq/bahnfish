using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Central settings system manager.
/// Manages all game settings across categories (Video, Audio, Controls, Gameplay, Accessibility).
/// Features:
/// - Settings persistence (save/load via PlayerPrefs and SaveManager integration)
/// - Apply settings at runtime without restart
/// - Reset to defaults per category or all
/// - Event-driven architecture for settings changes
/// - Platform-specific defaults
/// </summary>
public class SettingsManager : MonoBehaviour
{
    private static SettingsManager _instance;

    /// <summary>
    /// Singleton instance accessor.
    /// </summary>
    public static SettingsManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<SettingsManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("SettingsManager");
                    _instance = go.AddComponent<SettingsManager>();
                }
            }
            return _instance;
        }
    }

    [Header("Settings Components")]
    [SerializeField] private VideoSettings videoSettings;
    [SerializeField] private AudioSettings audioSettings;
    [SerializeField] private ControlSettings controlSettings;
    [SerializeField] private GameplaySettings gameplaySettings;
    [SerializeField] private AccessibilitySettings accessibilitySettings;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogging = true;

    private bool isInitialized = false;

    /// <summary>
    /// Public accessors for settings components.
    /// </summary>
    public VideoSettings Video => videoSettings;
    public AudioSettings Audio => audioSettings;
    public ControlSettings Controls => controlSettings;
    public GameplaySettings Gameplay => gameplaySettings;
    public AccessibilitySettings Accessibility => accessibilitySettings;

    /// <summary>
    /// Initialize settings manager singleton.
    /// </summary>
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeSettings();
    }

    /// <summary>
    /// Initialize all settings components.
    /// </summary>
    private void InitializeSettings()
    {
        if (isInitialized) return;

        LogDebug("Initializing Settings Manager...");

        // Create settings components if not assigned
        if (videoSettings == null)
        {
            videoSettings = gameObject.AddComponent<VideoSettings>();
        }
        if (audioSettings == null)
        {
            audioSettings = gameObject.AddComponent<AudioSettings>();
        }
        if (controlSettings == null)
        {
            controlSettings = gameObject.AddComponent<ControlSettings>();
        }
        if (gameplaySettings == null)
        {
            gameplaySettings = gameObject.AddComponent<GameplaySettings>();
        }
        if (accessibilitySettings == null)
        {
            accessibilitySettings = gameObject.AddComponent<AccessibilitySettings>();
        }

        // Initialize all components
        videoSettings.Initialize();
        audioSettings.Initialize();
        controlSettings.Initialize();
        gameplaySettings.Initialize();
        accessibilitySettings.Initialize();

        // Load saved settings
        LoadAllSettings();

        // Apply settings
        ApplyAllSettings();

        isInitialized = true;
        LogDebug("Settings Manager initialized successfully");

        // Publish initialization event
        EventSystem.Publish("SettingsInitialized", true);
    }

    private void Start()
    {
        // Subscribe to save/load events
        EventSystem.Subscribe<SaveData>("GatheringSaveData", OnGatheringSaveData);
        EventSystem.Subscribe<SaveData>("ApplyingSaveData", OnApplyingSaveData);
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        EventSystem.Unsubscribe<SaveData>("GatheringSaveData", OnGatheringSaveData);
        EventSystem.Unsubscribe<SaveData>("ApplyingSaveData", OnApplyingSaveData);

        if (_instance == this)
        {
            _instance = null;
        }
    }

    #region Save/Load

    /// <summary>
    /// Load all settings from PlayerPrefs.
    /// </summary>
    public void LoadAllSettings()
    {
        LogDebug("Loading all settings...");

        videoSettings.LoadSettings();
        audioSettings.LoadSettings();
        controlSettings.LoadSettings();
        gameplaySettings.LoadSettings();
        accessibilitySettings.LoadSettings();

        LogDebug("All settings loaded");
    }

    /// <summary>
    /// Save all settings to PlayerPrefs.
    /// </summary>
    public void SaveAllSettings()
    {
        LogDebug("Saving all settings...");

        videoSettings.SaveSettings();
        audioSettings.SaveSettings();
        controlSettings.SaveSettings();
        gameplaySettings.SaveSettings();
        accessibilitySettings.SaveSettings();

        PlayerPrefs.Save();
        LogDebug("All settings saved");

        EventSystem.Publish("SettingsSaved", true);
    }

    /// <summary>
    /// Apply all settings to the game.
    /// </summary>
    public void ApplyAllSettings()
    {
        LogDebug("Applying all settings...");

        videoSettings.ApplySettings();
        audioSettings.ApplySettings();
        controlSettings.ApplySettings();
        gameplaySettings.ApplySettings();
        accessibilitySettings.ApplySettings();

        LogDebug("All settings applied");
        EventSystem.Publish("SettingsApplied", true);
    }

    #endregion

    #region Reset

    /// <summary>
    /// Reset all settings to platform-specific defaults.
    /// </summary>
    public void ResetAllToDefaults()
    {
        LogDebug("Resetting all settings to defaults...");

        videoSettings.ResetToDefaults();
        audioSettings.ResetToDefaults();
        controlSettings.ResetToDefaults();
        gameplaySettings.ResetToDefaults();
        accessibilitySettings.ResetToDefaults();

        SaveAllSettings();
        ApplyAllSettings();

        LogDebug("All settings reset to defaults");
        EventSystem.Publish("SettingsReset", "All");
    }

    /// <summary>
    /// Reset a specific category to defaults.
    /// </summary>
    /// <param name="category">Category name (Video, Audio, Controls, Gameplay, Accessibility)</param>
    public void ResetCategoryToDefaults(string category)
    {
        LogDebug($"Resetting {category} settings to defaults...");

        switch (category.ToLower())
        {
            case "video":
                videoSettings.ResetToDefaults();
                videoSettings.SaveSettings();
                videoSettings.ApplySettings();
                break;
            case "audio":
                audioSettings.ResetToDefaults();
                audioSettings.SaveSettings();
                audioSettings.ApplySettings();
                break;
            case "controls":
                controlSettings.ResetToDefaults();
                controlSettings.SaveSettings();
                controlSettings.ApplySettings();
                break;
            case "gameplay":
                gameplaySettings.ResetToDefaults();
                gameplaySettings.SaveSettings();
                gameplaySettings.ApplySettings();
                break;
            case "accessibility":
                accessibilitySettings.ResetToDefaults();
                accessibilitySettings.SaveSettings();
                accessibilitySettings.ApplySettings();
                break;
            default:
                Debug.LogWarning($"[SettingsManager] Unknown category: {category}");
                return;
        }

        LogDebug($"{category} settings reset to defaults");
        EventSystem.Publish("SettingsReset", category);
    }

    #endregion

    #region SaveManager Integration

    /// <summary>
    /// Save settings data to SaveData structure.
    /// </summary>
    private void OnGatheringSaveData(SaveData saveData)
    {
        // Settings are saved to PlayerPrefs separately, but we can include them in save data too
        saveData.settingsData = new SettingsData
        {
            video = videoSettings.GetData(),
            audio = audioSettings.GetData(),
            controls = controlSettings.GetData(),
            gameplay = gameplaySettings.GetData(),
            accessibility = accessibilitySettings.GetData()
        };

        LogDebug("Settings data gathered for save");
    }

    /// <summary>
    /// Load settings from SaveData structure.
    /// </summary>
    private void OnApplyingSaveData(SaveData saveData)
    {
        if (saveData.settingsData != null)
        {
            LogDebug("Applying settings from save data...");

            videoSettings.SetData(saveData.settingsData.video);
            audioSettings.SetData(saveData.settingsData.audio);
            controlSettings.SetData(saveData.settingsData.controls);
            gameplaySettings.SetData(saveData.settingsData.gameplay);
            accessibilitySettings.SetData(saveData.settingsData.accessibility);

            ApplyAllSettings();
            LogDebug("Settings applied from save data");
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Logs debug messages if debugging is enabled.
    /// </summary>
    private void LogDebug(string message)
    {
        if (enableDebugLogging)
        {
            Debug.Log($"[SettingsManager] {message}");
        }
    }

    /// <summary>
    /// Check if settings manager is initialized.
    /// </summary>
    public bool IsInitialized => isInitialized;

    #endregion
}

/// <summary>
/// Container for all settings data.
/// Used for save/load integration.
/// </summary>
[Serializable]
public class SettingsData
{
    public VideoSettingsData video;
    public AudioSettingsData audio;
    public ControlSettingsData controls;
    public GameplaySettingsData gameplay;
    public AccessibilitySettingsData accessibility;

    public SettingsData()
    {
        video = new VideoSettingsData();
        audio = new AudioSettingsData();
        controls = new ControlSettingsData();
        gameplay = new GameplaySettingsData();
        accessibility = new AccessibilitySettingsData();
    }
}

/// <summary>
/// Enum for preservation methods (required by PreservationData in SaveData.cs)
/// </summary>
public enum PreservationMethod
{
    None,
    IceBox,
    Salting,
    Smoking,
    Freezing
}

/// <summary>
/// Enum for active buff serialization (required by SaveData.cs)
/// </summary>
[Serializable]
public class SerializedBuff
{
    public string buffID;
    public float remainingDuration;
    public float effectStrength;
    public string buffType;

    public SerializedBuff()
    {
        buffID = "";
        remainingDuration = 0f;
        effectStrength = 1f;
        buffType = "";
    }
}

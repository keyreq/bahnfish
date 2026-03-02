using UnityEngine;
using System;

/// <summary>
/// Manages gameplay settings.
/// Features:
/// - Difficulty presets (Story/Normal/Hard/Custom)
/// - AI aggression levels
/// - Mini-game difficulty
/// - Sanity drain rate (adjustable or OFF)
/// - Auto-save frequency
/// - Permadeath toggle
/// - Enemy damage multiplier
/// - Time scale adjustment
/// - Tutorial hints and quest markers toggles
/// </summary>
public class GameplaySettings : MonoBehaviour
{
    #region Enums

    public enum DifficultyLevel
    {
        Story,
        Normal,
        Hard,
        Custom
    }

    public enum AIAggressionLevel
    {
        Easy,
        Normal,
        Hard
    }

    public enum MinigameDifficulty
    {
        Easy,
        Normal,
        Hard
    }

    public enum SanityDrainRate
    {
        Off,
        Half,      // 50%
        Normal,    // 100%
        Increased, // 150%
        Double     // 200%
    }

    public enum AutoSaveFrequency
    {
        Never,
        OneMinute,
        FiveMinutes,
        TenMinutes
    }

    public enum TimeScale
    {
        Half,      // 0.5x
        Normal,    // 1x
        OneFiveX,  // 1.5x
        Double     // 2x
    }

    #endregion

    #region Settings Variables

    [Header("Difficulty")]
    public DifficultyLevel difficulty = DifficultyLevel.Normal;
    public AIAggressionLevel fishAIAggression = AIAggressionLevel.Normal;
    public MinigameDifficulty fishingMinigameDifficulty = MinigameDifficulty.Normal;

    [Header("Gameplay Modifiers")]
    public SanityDrainRate sanityDrainRate = SanityDrainRate.Normal;
    public float enemyDamageMultiplier = 1f; // 0.5, 1.0, 1.5, 2.0
    public TimeScale timeScale = TimeScale.Normal;

    [Header("Progression")]
    public AutoSaveFrequency autoSaveFrequency = AutoSaveFrequency.FiveMinutes;
    public bool permadeathEnabled = false;

    [Header("UI Assistance")]
    public bool tutorialHintsEnabled = true;
    public bool questMarkersEnabled = true;

    #endregion

    private bool isInitialized = false;

    /// <summary>
    /// Initialize gameplay settings.
    /// </summary>
    public void Initialize()
    {
        if (isInitialized) return;

        isInitialized = true;
        Debug.Log("[GameplaySettings] Initialized");
    }

    #region Difficulty Presets

    /// <summary>
    /// Apply difficulty preset.
    /// </summary>
    public void ApplyDifficultyPreset(DifficultyLevel preset)
    {
        difficulty = preset;

        switch (preset)
        {
            case DifficultyLevel.Story:
                ApplyStoryDifficulty();
                break;
            case DifficultyLevel.Normal:
                ApplyNormalDifficulty();
                break;
            case DifficultyLevel.Hard:
                ApplyHardDifficulty();
                break;
            case DifficultyLevel.Custom:
                // Keep current settings
                break;
        }

        EventSystem.Publish("DifficultyChanged", difficulty);
    }

    /// <summary>
    /// Apply Story difficulty (easy, narrative-focused).
    /// </summary>
    private void ApplyStoryDifficulty()
    {
        fishAIAggression = AIAggressionLevel.Easy;
        fishingMinigameDifficulty = MinigameDifficulty.Easy;
        sanityDrainRate = SanityDrainRate.Half;
        enemyDamageMultiplier = 0.5f;
        permadeathEnabled = false;
        tutorialHintsEnabled = true;
        questMarkersEnabled = true;
    }

    /// <summary>
    /// Apply Normal difficulty (balanced challenge).
    /// </summary>
    private void ApplyNormalDifficulty()
    {
        fishAIAggression = AIAggressionLevel.Normal;
        fishingMinigameDifficulty = MinigameDifficulty.Normal;
        sanityDrainRate = SanityDrainRate.Normal;
        enemyDamageMultiplier = 1f;
        permadeathEnabled = false;
        tutorialHintsEnabled = true;
        questMarkersEnabled = true;
    }

    /// <summary>
    /// Apply Hard difficulty (challenging, punishing).
    /// </summary>
    private void ApplyHardDifficulty()
    {
        fishAIAggression = AIAggressionLevel.Hard;
        fishingMinigameDifficulty = MinigameDifficulty.Hard;
        sanityDrainRate = SanityDrainRate.Double;
        enemyDamageMultiplier = 2f;
        permadeathEnabled = false; // Player can still opt-in
        tutorialHintsEnabled = false;
        questMarkersEnabled = false;
    }

    #endregion

    #region Apply Settings

    /// <summary>
    /// Apply all gameplay settings.
    /// </summary>
    public void ApplySettings()
    {
        ApplyFishAIAggression();
        ApplyMinigameDifficulty();
        ApplySanityDrainRate();
        ApplyEnemyDamageMultiplier();
        ApplyTimeScale();
        ApplyAutoSave();
        ApplyUIAssistance();

        Debug.Log("[GameplaySettings] Settings applied");
        EventSystem.Publish("GameplaySettingsApplied", true);
    }

    /// <summary>
    /// Apply fish AI aggression level.
    /// </summary>
    private void ApplyFishAIAggression()
    {
        EventSystem.Publish("SetFishAIAggression", fishAIAggression);
    }

    /// <summary>
    /// Apply fishing mini-game difficulty.
    /// </summary>
    private void ApplyMinigameDifficulty()
    {
        EventSystem.Publish("SetMinigameDifficulty", fishingMinigameDifficulty);
    }

    /// <summary>
    /// Apply sanity drain rate.
    /// </summary>
    private void ApplySanityDrainRate()
    {
        float drainMultiplier = GetSanityDrainMultiplier();
        EventSystem.Publish("SetSanityDrainRate", drainMultiplier);
    }

    /// <summary>
    /// Get sanity drain multiplier from enum.
    /// </summary>
    private float GetSanityDrainMultiplier()
    {
        switch (sanityDrainRate)
        {
            case SanityDrainRate.Off:
                return 0f;
            case SanityDrainRate.Half:
                return 0.5f;
            case SanityDrainRate.Normal:
                return 1f;
            case SanityDrainRate.Increased:
                return 1.5f;
            case SanityDrainRate.Double:
                return 2f;
            default:
                return 1f;
        }
    }

    /// <summary>
    /// Apply enemy damage multiplier.
    /// </summary>
    private void ApplyEnemyDamageMultiplier()
    {
        EventSystem.Publish("SetEnemyDamageMultiplier", enemyDamageMultiplier);
    }

    /// <summary>
    /// Apply time scale.
    /// </summary>
    private void ApplyTimeScale()
    {
        float scale = GetTimeScaleValue();
        EventSystem.Publish("SetGameTimeScale", scale);
    }

    /// <summary>
    /// Get time scale value from enum.
    /// </summary>
    private float GetTimeScaleValue()
    {
        switch (timeScale)
        {
            case TimeScale.Half:
                return 0.5f;
            case TimeScale.Normal:
                return 1f;
            case TimeScale.OneFiveX:
                return 1.5f;
            case TimeScale.Double:
                return 2f;
            default:
                return 1f;
        }
    }

    /// <summary>
    /// Apply auto-save frequency.
    /// </summary>
    private void ApplyAutoSave()
    {
        float intervalSeconds = GetAutoSaveInterval();
        EventSystem.Publish("SetAutoSaveInterval", intervalSeconds);
    }

    /// <summary>
    /// Get auto-save interval in seconds.
    /// </summary>
    private float GetAutoSaveInterval()
    {
        switch (autoSaveFrequency)
        {
            case AutoSaveFrequency.Never:
                return 0f; // 0 = disabled
            case AutoSaveFrequency.OneMinute:
                return 60f;
            case AutoSaveFrequency.FiveMinutes:
                return 300f;
            case AutoSaveFrequency.TenMinutes:
                return 600f;
            default:
                return 300f;
        }
    }

    /// <summary>
    /// Apply UI assistance settings.
    /// </summary>
    private void ApplyUIAssistance()
    {
        EventSystem.Publish("SetTutorialHints", tutorialHintsEnabled);
        EventSystem.Publish("SetQuestMarkers", questMarkersEnabled);
        EventSystem.Publish("SetPermadeath", permadeathEnabled);
    }

    #endregion

    #region Save/Load

    /// <summary>
    /// Save settings to PlayerPrefs.
    /// </summary>
    public void SaveSettings()
    {
        PlayerPrefs.SetInt("Gameplay_Difficulty", (int)difficulty);
        PlayerPrefs.SetInt("Gameplay_FishAIAggression", (int)fishAIAggression);
        PlayerPrefs.SetInt("Gameplay_MinigameDifficulty", (int)fishingMinigameDifficulty);
        PlayerPrefs.SetInt("Gameplay_SanityDrainRate", (int)sanityDrainRate);
        PlayerPrefs.SetFloat("Gameplay_EnemyDamageMultiplier", enemyDamageMultiplier);
        PlayerPrefs.SetInt("Gameplay_TimeScale", (int)timeScale);
        PlayerPrefs.SetInt("Gameplay_AutoSaveFrequency", (int)autoSaveFrequency);
        PlayerPrefs.SetInt("Gameplay_Permadeath", permadeathEnabled ? 1 : 0);
        PlayerPrefs.SetInt("Gameplay_TutorialHints", tutorialHintsEnabled ? 1 : 0);
        PlayerPrefs.SetInt("Gameplay_QuestMarkers", questMarkersEnabled ? 1 : 0);
    }

    /// <summary>
    /// Load settings from PlayerPrefs.
    /// </summary>
    public void LoadSettings()
    {
        difficulty = (DifficultyLevel)PlayerPrefs.GetInt("Gameplay_Difficulty", 1);
        fishAIAggression = (AIAggressionLevel)PlayerPrefs.GetInt("Gameplay_FishAIAggression", 1);
        fishingMinigameDifficulty = (MinigameDifficulty)PlayerPrefs.GetInt("Gameplay_MinigameDifficulty", 1);
        sanityDrainRate = (SanityDrainRate)PlayerPrefs.GetInt("Gameplay_SanityDrainRate", 2);
        enemyDamageMultiplier = PlayerPrefs.GetFloat("Gameplay_EnemyDamageMultiplier", 1f);
        timeScale = (TimeScale)PlayerPrefs.GetInt("Gameplay_TimeScale", 1);
        autoSaveFrequency = (AutoSaveFrequency)PlayerPrefs.GetInt("Gameplay_AutoSaveFrequency", 2);
        permadeathEnabled = PlayerPrefs.GetInt("Gameplay_Permadeath", 0) == 1;
        tutorialHintsEnabled = PlayerPrefs.GetInt("Gameplay_TutorialHints", 1) == 1;
        questMarkersEnabled = PlayerPrefs.GetInt("Gameplay_QuestMarkers", 1) == 1;
    }

    #endregion

    #region Reset

    /// <summary>
    /// Reset to default settings.
    /// </summary>
    public void ResetToDefaults()
    {
        difficulty = DifficultyLevel.Normal;
        ApplyNormalDifficulty();
        autoSaveFrequency = AutoSaveFrequency.FiveMinutes;
        timeScale = TimeScale.Normal;
    }

    #endregion

    #region Data Transfer

    /// <summary>
    /// Get settings data for save system integration.
    /// </summary>
    public GameplaySettingsData GetData()
    {
        return new GameplaySettingsData
        {
            difficulty = (int)difficulty,
            fishAIAggression = (int)fishAIAggression,
            fishingMinigameDifficulty = (int)fishingMinigameDifficulty,
            sanityDrainRate = (int)sanityDrainRate,
            enemyDamageMultiplier = enemyDamageMultiplier,
            timeScale = (int)timeScale,
            autoSaveFrequency = (int)autoSaveFrequency,
            permadeathEnabled = permadeathEnabled,
            tutorialHintsEnabled = tutorialHintsEnabled,
            questMarkersEnabled = questMarkersEnabled
        };
    }

    /// <summary>
    /// Set settings from data structure.
    /// </summary>
    public void SetData(GameplaySettingsData data)
    {
        if (data == null) return;

        difficulty = (DifficultyLevel)data.difficulty;
        fishAIAggression = (AIAggressionLevel)data.fishAIAggression;
        fishingMinigameDifficulty = (MinigameDifficulty)data.fishingMinigameDifficulty;
        sanityDrainRate = (SanityDrainRate)data.sanityDrainRate;
        enemyDamageMultiplier = data.enemyDamageMultiplier;
        timeScale = (TimeScale)data.timeScale;
        autoSaveFrequency = (AutoSaveFrequency)data.autoSaveFrequency;
        permadeathEnabled = data.permadeathEnabled;
        tutorialHintsEnabled = data.tutorialHintsEnabled;
        questMarkersEnabled = data.questMarkersEnabled;
    }

    #endregion
}

/// <summary>
/// Serializable gameplay settings data.
/// </summary>
[Serializable]
public class GameplaySettingsData
{
    public int difficulty = 1;
    public int fishAIAggression = 1;
    public int fishingMinigameDifficulty = 1;
    public int sanityDrainRate = 2;
    public float enemyDamageMultiplier = 1f;
    public int timeScale = 1;
    public int autoSaveFrequency = 2;
    public bool permadeathEnabled = false;
    public bool tutorialHintsEnabled = true;
    public bool questMarkersEnabled = true;
}

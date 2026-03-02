using UnityEngine;

/// <summary>
/// Agent 13: Visual Effects & Particles Specialist - UIParticleEffects.cs
/// Manages visual effects for UI elements including achievements, notifications, and level ups.
/// </summary>
public class UIParticleEffects : MonoBehaviour
{
    [Header("Achievement Effects")]
    [SerializeField] private GameObject achievementFanfarePrefab;
    [SerializeField] private GameObject achievementStarsPrefab;

    [Header("Notification Effects")]
    [SerializeField] private GameObject infoGlowPrefab;
    [SerializeField] private GameObject warningPulsePrefab;
    [SerializeField] private GameObject errorFlashPrefab;

    [Header("Progress Effects")]
    [SerializeField] private GameObject questCompleteConfettiPrefab;
    [SerializeField] private GameObject levelUpBurstPrefab;
    [SerializeField] private GameObject levelUpNumbersPrefab;

    [Header("Button Effects")]
    [SerializeField] private GameObject buttonHoverGlowPrefab;

    private VFXQuality currentQuality = VFXQuality.High;
    private VFXManager vfxManager;

    private void Start()
    {
        vfxManager = VFXManager.Instance;
        SubscribeToEvents();
        RegisterParticlePrefabs();
        Debug.Log("[UIParticleEffects] Initialized.");
    }

    private void SubscribeToEvents()
    {
        EventSystem.Subscribe<string>("AchievementUnlocked", OnAchievementUnlocked);
        EventSystem.Subscribe<string>("QuestCompleted", OnQuestCompleted);
        EventSystem.Subscribe<int>("PlayerLevelUp", OnLevelUp);
    }

    private void RegisterParticlePrefabs()
    {
        if (vfxManager == null) return;
        if (achievementFanfarePrefab != null) vfxManager.RegisterParticlePrefab("ui_achievement_fanfare", achievementFanfarePrefab);
        if (achievementStarsPrefab != null) vfxManager.RegisterParticlePrefab("ui_achievement_stars", achievementStarsPrefab);
        if (infoGlowPrefab != null) vfxManager.RegisterParticlePrefab("ui_info_glow", infoGlowPrefab);
        if (warningPulsePrefab != null) vfxManager.RegisterParticlePrefab("ui_warning_pulse", warningPulsePrefab);
        if (errorFlashPrefab != null) vfxManager.RegisterParticlePrefab("ui_error_flash", errorFlashPrefab);
        if (questCompleteConfettiPrefab != null) vfxManager.RegisterParticlePrefab("ui_quest_confetti", questCompleteConfettiPrefab);
        if (levelUpBurstPrefab != null) vfxManager.RegisterParticlePrefab("ui_level_up_burst", levelUpBurstPrefab);
        if (levelUpNumbersPrefab != null) vfxManager.RegisterParticlePrefab("ui_level_up_numbers", levelUpNumbersPrefab);
        if (buttonHoverGlowPrefab != null) vfxManager.RegisterParticlePrefab("ui_button_hover", buttonHoverGlowPrefab);
    }

    private void OnDestroy()
    {
        EventSystem.Unsubscribe<string>("AchievementUnlocked", OnAchievementUnlocked);
        EventSystem.Unsubscribe<string>("QuestCompleted", OnQuestCompleted);
        EventSystem.Unsubscribe<int>("PlayerLevelUp", OnLevelUp);
    }

    public void CreateAchievementEffect(Vector3 screenPosition)
    {
        if (currentQuality < VFXQuality.Medium) return;

        vfxManager.SpawnEffect("ui_achievement_fanfare", screenPosition);
        if (currentQuality >= VFXQuality.High)
        {
            vfxManager.SpawnEffect("ui_achievement_stars", screenPosition);
        }
    }

    public void CreateNotificationEffect(Vector3 screenPosition, NotificationType type)
    {
        if (currentQuality < VFXQuality.Low) return;

        string effectID = type switch
        {
            NotificationType.Info => "ui_info_glow",
            NotificationType.Warning => "ui_warning_pulse",
            NotificationType.Error => "ui_error_flash",
            _ => "ui_info_glow"
        };

        vfxManager.SpawnEffect(effectID, screenPosition);
    }

    public void CreateQuestCompleteEffect(Vector3 screenPosition)
    {
        if (currentQuality < VFXQuality.Medium) return;
        vfxManager.SpawnEffect("ui_quest_confetti", screenPosition);
    }

    public void CreateLevelUpEffect(Vector3 screenPosition, int newLevel)
    {
        if (currentQuality < VFXQuality.Low) return;

        vfxManager.SpawnEffect("ui_level_up_burst", screenPosition);

        if (currentQuality >= VFXQuality.High)
        {
            ParticleSystem numbers = vfxManager.SpawnEffect("ui_level_up_numbers", screenPosition);
            // Numbers would display the new level
        }
    }

    public void CreateButtonHoverEffect(Vector3 buttonPosition)
    {
        if (currentQuality < VFXQuality.High) return;
        vfxManager.SpawnEffect("ui_button_hover", buttonPosition);
    }

    private void OnAchievementUnlocked(string achievementID)
    {
        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 10f);
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenCenter);
        CreateAchievementEffect(worldPos);
    }

    private void OnQuestCompleted(string questID)
    {
        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 10f);
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenCenter);
        CreateQuestCompleteEffect(worldPos);
    }

    private void OnLevelUp(int newLevel)
    {
        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 10f);
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenCenter);
        CreateLevelUpEffect(worldPos, newLevel);
    }

    public void SetQuality(VFXQuality quality)
    {
        currentQuality = quality;
    }
}

public enum NotificationType
{
    Info,
    Warning,
    Error
}

using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Agent 20: Idle/AFK Progression System - IdleManager.cs
/// Central manager for idle progression and offline time tracking.
/// Coordinates all passive income systems and offline rewards.
/// </summary>
public class IdleManager : MonoBehaviour
{
    private static IdleManager _instance;
    public static IdleManager Instance => _instance;

    [Header("Idle Configuration")]
    [Tooltip("Maximum offline time that grants rewards (in hours)")]
    [SerializeField] private float maxOfflineHours = 24f;

    [Tooltip("Enable idle progression (requires Auto-Fisher upgrade)")]
    [SerializeField] private bool idleProgressionEnabled = false;

    [Header("Current Session")]
    [SerializeField] private DateTime sessionStartTime;
    [SerializeField] private DateTime lastLogoutTime;
    [SerializeField] private bool hasOfflineRewards = false;

    [Header("Offline Progress")]
    [SerializeField] private float offlineTimeElapsed = 0f; // In hours
    [SerializeField] private float cappedOfflineTime = 0f;  // After applying 24h cap
    [SerializeField] private float offlineMoneyEarned = 0f;
    [SerializeField] private int offlineFishCaught = 0;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogging = true;
    [SerializeField] private bool showOfflineDebugInfo = true;

    // Component references
    private IdleProgressionCalculator progressionCalculator;
    private AutoFishingSystem autoFishingSystem;
    private PassiveIncomeSystem passiveIncomeSystem;
    private WelcomeBackSystem welcomeBackSystem;
    private IdleUpgradeSystem upgradeSystem;
    private IdleResourceGenerator resourceGenerator;
    private OfflineEventSimulator eventSimulator;

    // Idle data
    private IdleData idleData;

    private void Awake()
    {
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

    /// <summary>
    /// Initializes the idle system and all subsystems.
    /// </summary>
    private void Initialize()
    {
        // Initialize idle data
        idleData = new IdleData();
        sessionStartTime = DateTime.UtcNow;

        // Get or create subsystems
        progressionCalculator = GetOrCreateComponent<IdleProgressionCalculator>();
        autoFishingSystem = GetOrCreateComponent<AutoFishingSystem>();
        passiveIncomeSystem = GetOrCreateComponent<PassiveIncomeSystem>();
        welcomeBackSystem = GetOrCreateComponent<WelcomeBackSystem>();
        upgradeSystem = GetOrCreateComponent<IdleUpgradeSystem>();
        resourceGenerator = GetOrCreateComponent<IdleResourceGenerator>();
        eventSimulator = GetOrCreateComponent<OfflineEventSimulator>();

        // Subscribe to events
        EventSystem.Subscribe("GameInitialized", OnGameInitialized);
        EventSystem.Subscribe("GameQuitting", OnGameQuitting);
        EventSystem.Subscribe<SaveData>("GatheringSaveData", OnGatheringSaveData);
        EventSystem.Subscribe<SaveData>("ApplyingSaveData", OnApplyingSaveData);
        EventSystem.Subscribe<string>("IdleUpgradePurchased", OnIdleUpgradePurchased);

        if (enableDebugLogging)
        {
            Debug.Log("[IdleManager] Initialized successfully");
        }
    }

    /// <summary>
    /// Gets or creates a component on this GameObject.
    /// </summary>
    private T GetOrCreateComponent<T>() where T : Component
    {
        T component = GetComponent<T>();
        if (component == null)
        {
            component = gameObject.AddComponent<T>();
        }
        return component;
    }

    /// <summary>
    /// Called when the game is initialized.
    /// Checks for offline time and calculates rewards.
    /// </summary>
    private void OnGameInitialized()
    {
        CheckOfflineProgress();
    }

    /// <summary>
    /// Checks if the player has been offline and calculates rewards.
    /// </summary>
    private void CheckOfflineProgress()
    {
        // Check if we have a valid logout time
        if (string.IsNullOrEmpty(idleData.lastLogoutTime))
        {
            if (enableDebugLogging)
            {
                Debug.Log("[IdleManager] No previous logout time found (new game)");
            }
            return;
        }

        // Parse logout time
        DateTime logoutTime;
        if (!DateTime.TryParse(idleData.lastLogoutTime, out logoutTime))
        {
            Debug.LogError("[IdleManager] Failed to parse logout time!");
            return;
        }

        // Calculate offline time
        DateTime now = DateTime.UtcNow;
        TimeSpan offlineTimeSpan = now - logoutTime;
        offlineTimeElapsed = (float)offlineTimeSpan.TotalHours;

        // Check if player has Auto-Fisher upgrade (required for idle progression)
        if (!upgradeSystem.HasUpgrade("auto_fisher"))
        {
            if (enableDebugLogging)
            {
                Debug.Log("[IdleManager] Auto-Fisher not purchased. No offline rewards.");
            }
            return;
        }

        // Check if offline time is significant (at least 5 minutes)
        if (offlineTimeElapsed < 0.083f) // 5 minutes
        {
            if (enableDebugLogging)
            {
                Debug.Log($"[IdleManager] Offline time too short: {offlineTimeElapsed * 60f} minutes");
            }
            return;
        }

        // Apply 24-hour cap
        cappedOfflineTime = Mathf.Min(offlineTimeElapsed, maxOfflineHours);

        if (enableDebugLogging)
        {
            Debug.Log($"[IdleManager] Player was offline for {offlineTimeElapsed:F2} hours (capped: {cappedOfflineTime:F2} hours)");
        }

        // Calculate offline rewards
        CalculateOfflineRewards();

        // Mark that we have offline rewards to show
        hasOfflineRewards = true;
        idleData.hasCollectedOfflineRewards = false;

        // Show welcome back UI
        if (welcomeBackSystem != null)
        {
            welcomeBackSystem.ShowWelcomeBackUI(CreateOfflineRewardsSummary());
        }
    }

    /// <summary>
    /// Calculates all offline rewards based on offline time.
    /// </summary>
    private void CalculateOfflineRewards()
    {
        if (progressionCalculator == null)
        {
            Debug.LogError("[IdleManager] ProgressionCalculator is null!");
            return;
        }

        // Calculate offline fishing
        if (autoFishingSystem != null)
        {
            var fishingResults = autoFishingSystem.SimulateOfflineFishing(cappedOfflineTime);
            offlineFishCaught = fishingResults.fishCaught;
            offlineMoneyEarned += fishingResults.moneyEarned;
        }

        // Calculate passive income
        if (passiveIncomeSystem != null)
        {
            float passiveEarnings = passiveIncomeSystem.CalculateOfflinePassiveIncome(cappedOfflineTime);
            offlineMoneyEarned += passiveEarnings;
        }

        // Generate resources
        if (resourceGenerator != null)
        {
            idleData.offlineMaterialsGathered = resourceGenerator.GenerateOfflineResources(cappedOfflineTime);
        }

        // Simulate events
        if (eventSimulator != null)
        {
            idleData.offlineEventsOccurred = eventSimulator.SimulateOfflineEvents(cappedOfflineTime);
        }

        // Apply storage cap
        float maxStorage = upgradeSystem.GetMaxOfflineStorage();
        if (offlineMoneyEarned > maxStorage)
        {
            if (enableDebugLogging)
            {
                Debug.LogWarning($"[IdleManager] Offline earnings capped at ${maxStorage} (earned ${offlineMoneyEarned})");
            }
            offlineMoneyEarned = maxStorage;
        }

        // Store in idle data
        idleData.accumulatedOfflineMoney = offlineMoneyEarned;
        idleData.offlineFishCaught = offlineFishCaught;

        // Update totals
        idleData.totalIdleEarnings += offlineMoneyEarned;
        idleData.totalIdleFishCaught += offlineFishCaught;

        if (enableDebugLogging)
        {
            Debug.Log($"[IdleManager] Offline Rewards Calculated:");
            Debug.Log($"  Money: ${offlineMoneyEarned:F2}");
            Debug.Log($"  Fish Caught: {offlineFishCaught}");
            Debug.Log($"  Materials: {idleData.offlineMaterialsGathered.Count} types");
            Debug.Log($"  Events: {idleData.offlineEventsOccurred.Count}");
        }
    }

    /// <summary>
    /// Creates a summary of offline rewards for the Welcome Back UI.
    /// </summary>
    private OfflineRewardsSummary CreateOfflineRewardsSummary()
    {
        return new OfflineRewardsSummary
        {
            offlineTimeHours = offlineTimeElapsed,
            cappedTimeHours = cappedOfflineTime,
            moneyEarned = offlineMoneyEarned,
            fishCaught = offlineFishCaught,
            materialsGathered = new Dictionary<string, int>(idleData.offlineMaterialsGathered),
            eventsOccurred = new List<string>(idleData.offlineEventsOccurred),
            comebackBonus = CalculateComebackBonus(),
            wasCapped = offlineTimeElapsed > maxOfflineHours
        };
    }

    /// <summary>
    /// Calculates comeback bonus based on time away.
    /// </summary>
    private ComebackBonus CalculateComebackBonus()
    {
        ComebackBonus bonus = new ComebackBonus();

        // Check last comeback bonus time
        DateTime lastBonus;
        if (DateTime.TryParse(idleData.lastComebackBonusTime, out lastBonus))
        {
            TimeSpan timeSinceBonus = DateTime.UtcNow - lastBonus;

            // 24+ hours: $500 bonus
            if (timeSinceBonus.TotalHours >= 24f)
            {
                bonus.moneyBonus = 500f;
                bonus.bonusDescription = "Daily Comeback Bonus";
            }

            // 48+ hours: $1,000 bonus + rare bait
            if (timeSinceBonus.TotalHours >= 48f)
            {
                bonus.moneyBonus = 1000f;
                bonus.rareBaitCount = 1;
                bonus.bonusDescription = "2-Day Comeback Bonus";
            }

            // 72+ hours: $2,000 bonus + relic
            if (timeSinceBonus.TotalHours >= 72f)
            {
                bonus.moneyBonus = 2000f;
                bonus.rareBaitCount = 1;
                bonus.relicCount = 1;
                bonus.bonusDescription = "3-Day Comeback Bonus";
            }
        }
        else
        {
            // First time, give small bonus
            bonus.moneyBonus = 500f;
            bonus.bonusDescription = "Welcome Bonus";
        }

        return bonus;
    }

    /// <summary>
    /// Collects offline rewards and adds them to the player's inventory.
    /// </summary>
    public void CollectOfflineRewards()
    {
        if (!hasOfflineRewards)
        {
            Debug.LogWarning("[IdleManager] No offline rewards to collect!");
            return;
        }

        // Apply money
        EventSystem.Publish("AddMoney", offlineMoneyEarned);

        // Apply materials
        foreach (var material in idleData.offlineMaterialsGathered)
        {
            EventSystem.Publish("AddMaterial", new MaterialReward { materialID = material.Key, quantity = material.Value });
        }

        // Apply comeback bonus
        ComebackBonus bonus = CalculateComebackBonus();
        if (bonus.moneyBonus > 0f)
        {
            EventSystem.Publish("AddMoney", bonus.moneyBonus);
            idleData.lastComebackBonusTime = DateTime.UtcNow.ToString("o");
        }
        if (bonus.rareBaitCount > 0)
        {
            EventSystem.Publish("AddRareBait", bonus.rareBaitCount);
        }
        if (bonus.relicCount > 0)
        {
            EventSystem.Publish("AddRelics", bonus.relicCount);
        }

        // Publish offline rewards collected event
        EventSystem.Publish("OfflineRewardsCollected", CreateOfflineRewardsSummary());

        // Clear offline rewards
        hasOfflineRewards = false;
        idleData.hasCollectedOfflineRewards = true;
        offlineMoneyEarned = 0f;
        offlineFishCaught = 0;
        idleData.offlineMaterialsGathered.Clear();
        idleData.offlineEventsOccurred.Clear();

        if (enableDebugLogging)
        {
            Debug.Log("[IdleManager] Offline rewards collected!");
        }
    }

    /// <summary>
    /// Called when the game is quitting. Records logout time.
    /// </summary>
    private void OnGameQuitting()
    {
        RecordLogoutTime();
    }

    /// <summary>
    /// Records the current time as logout time.
    /// </summary>
    private void RecordLogoutTime()
    {
        lastLogoutTime = DateTime.UtcNow;
        idleData.lastLogoutTime = lastLogoutTime.ToString("o");

        if (enableDebugLogging)
        {
            Debug.Log($"[IdleManager] Logout time recorded: {lastLogoutTime}");
        }
    }

    /// <summary>
    /// Called when an idle upgrade is purchased.
    /// </summary>
    private void OnIdleUpgradePurchased(string upgradeID)
    {
        if (upgradeID == "auto_fisher")
        {
            idleProgressionEnabled = true;
            if (enableDebugLogging)
            {
                Debug.Log("[IdleManager] Idle progression enabled (Auto-Fisher purchased)");
            }
        }

        // Recalculate idle efficiency
        EventSystem.Publish("IdleEfficiencyChanged", GetIdleEfficiency());
    }

    /// <summary>
    /// Gets the current idle efficiency multiplier.
    /// </summary>
    public float GetIdleEfficiency()
    {
        if (upgradeSystem == null) return 0f;
        return upgradeSystem.GetIdleEfficiencyMultiplier();
    }

    /// <summary>
    /// Gets whether idle progression is enabled.
    /// </summary>
    public bool IsIdleProgressionEnabled()
    {
        return idleProgressionEnabled && upgradeSystem.HasUpgrade("auto_fisher");
    }

    /// <summary>
    /// Gets the offline time elapsed (in hours).
    /// </summary>
    public float GetOfflineTimeElapsed()
    {
        return offlineTimeElapsed;
    }

    /// <summary>
    /// Gets the capped offline time (in hours).
    /// </summary>
    public float GetCappedOfflineTime()
    {
        return cappedOfflineTime;
    }

    /// <summary>
    /// Gets whether there are offline rewards to collect.
    /// </summary>
    public bool HasOfflineRewards()
    {
        return hasOfflineRewards;
    }

    #region Save/Load Integration

    private void OnGatheringSaveData(SaveData data)
    {
        // Record logout time when saving
        RecordLogoutTime();

        // Serialize idle data
        data.idleData = JsonUtility.ToJson(idleData);

        if (enableDebugLogging)
        {
            Debug.Log("[IdleManager] Idle data saved");
        }
    }

    private void OnApplyingSaveData(SaveData data)
    {
        if (string.IsNullOrEmpty(data.idleData))
        {
            idleData = new IdleData();
            return;
        }

        // Deserialize idle data
        idleData = JsonUtility.FromJson<IdleData>(data.idleData);

        // Check if Auto-Fisher is owned
        if (upgradeSystem != null)
        {
            idleProgressionEnabled = upgradeSystem.HasUpgrade("auto_fisher");
        }

        if (enableDebugLogging)
        {
            Debug.Log("[IdleManager] Idle data loaded");
        }
    }

    #endregion

    private void OnDestroy()
    {
        EventSystem.Unsubscribe("GameInitialized", OnGameInitialized);
        EventSystem.Unsubscribe("GameQuitting", OnGameQuitting);
        EventSystem.Unsubscribe<SaveData>("GatheringSaveData", OnGatheringSaveData);
        EventSystem.Unsubscribe<SaveData>("ApplyingSaveData", OnApplyingSaveData);
        EventSystem.Unsubscribe<string>("IdleUpgradePurchased", OnIdleUpgradePurchased);

        if (_instance == this)
        {
            _instance = null;
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Simulate 1 Hour Offline")]
    private void SimulateOneHourOffline()
    {
        SimulateOfflineTime(1f);
    }

    [ContextMenu("Simulate 8 Hours Offline")]
    private void SimulateEightHoursOffline()
    {
        SimulateOfflineTime(8f);
    }

    [ContextMenu("Simulate 24 Hours Offline")]
    private void SimulateTwentyFourHoursOffline()
    {
        SimulateOfflineTime(24f);
    }

    [ContextMenu("Simulate 48 Hours Offline (Capped)")]
    private void SimulateFortyEightHoursOffline()
    {
        SimulateOfflineTime(48f);
    }

    private void SimulateOfflineTime(float hours)
    {
        DateTime fakeLogout = DateTime.UtcNow.AddHours(-hours);
        idleData.lastLogoutTime = fakeLogout.ToString("o");
        CheckOfflineProgress();
        Debug.Log($"[IdleManager] Simulated {hours} hours offline");
    }

    [ContextMenu("Print Idle Stats")]
    private void PrintIdleStats()
    {
        Debug.Log($"=== Idle Manager Stats ===");
        Debug.Log($"Idle Progression Enabled: {idleProgressionEnabled}");
        Debug.Log($"Total Idle Earnings: ${idleData.totalIdleEarnings:F2}");
        Debug.Log($"Total Idle Fish Caught: {idleData.totalIdleFishCaught}");
        Debug.Log($"Has Offline Rewards: {hasOfflineRewards}");
        Debug.Log($"Offline Time Elapsed: {offlineTimeElapsed:F2} hours");
        Debug.Log($"Idle Efficiency: {GetIdleEfficiency():F2}x");
    }
#endif
}

/// <summary>
/// Summary of offline rewards for display in Welcome Back UI.
/// </summary>
[System.Serializable]
public struct OfflineRewardsSummary
{
    public float offlineTimeHours;
    public float cappedTimeHours;
    public float moneyEarned;
    public int fishCaught;
    public Dictionary<string, int> materialsGathered;
    public List<string> eventsOccurred;
    public ComebackBonus comebackBonus;
    public bool wasCapped;
}

/// <summary>
/// Comeback bonus for returning after extended absence.
/// </summary>
[System.Serializable]
public struct ComebackBonus
{
    public float moneyBonus;
    public int rareBaitCount;
    public int relicCount;
    public string bonusDescription;
}

/// <summary>
/// Material reward data for event publishing.
/// </summary>
public struct MaterialReward
{
    public string materialID;
    public int quantity;
}

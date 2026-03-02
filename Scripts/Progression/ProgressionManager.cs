using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Agent 9: Progression & Economy Agent - ProgressionManager.cs
/// Main progression system that coordinates all economy and progression subsystems.
/// Central point for tracking player advancement and providing progression UI data.
/// </summary>
public class ProgressionManager : MonoBehaviour
{
    public static ProgressionManager Instance { get; private set; }

    [Header("Subsystem References")]
    [SerializeField] private EconomySystem _economySystem;
    [SerializeField] private PricingSystem _pricingSystem;
    [SerializeField] private ShopManager _shopManager;
    [SerializeField] private UpgradeSystem _upgradeSystem;
    [SerializeField] private LocationLicenses _locationLicenses;
    [SerializeField] private DarkAbilities _darkAbilities;

    [Header("Progression Tracking")]
    [SerializeField] private int _totalFishCaught = 0;
    [SerializeField] private float _totalMoneyEarned = 0f;
    [SerializeField] private float _totalMoneySpent = 0f;
    [SerializeField] private int _upgradesPurchased = 0;
    [SerializeField] private int _locationsUnlocked = 1; // Start with 1 (starter lake)

    [Header("Milestones")]
    [SerializeField] private List<MilestoneData> _milestones = new List<MilestoneData>();
    private HashSet<string> _completedMilestones = new HashSet<string>();

    [Header("Debug")]
    [SerializeField] private bool _enableDebugLogs = true;

    // Events
    public event System.Action<MilestoneData> OnMilestoneCompleted;
    public event System.Action<int> OnPlayerLevelUp;
    public event System.Action OnProgressionChanged;

    // Properties
    public int PlayerLevel => CalculatePlayerLevel();
    public float ProgressionScore => CalculateProgressionScore();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        InitializeSubsystems();
        InitializeMilestones();
        SubscribeToEvents();

        if (_enableDebugLogs)
        {
            Debug.Log("[ProgressionManager] Initialized - All systems ready");
        }
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    #region Initialization

    /// <summary>
    /// Ensures all subsystems are initialized.
    /// </summary>
    private void InitializeSubsystems()
    {
        // Get or create subsystems
        if (_economySystem == null)
            _economySystem = EconomySystem.Instance;

        if (_pricingSystem == null)
            _pricingSystem = PricingSystem.Instance;

        if (_shopManager == null)
            _shopManager = ShopManager.Instance;

        if (_upgradeSystem == null)
            _upgradeSystem = UpgradeSystem.Instance;

        if (_locationLicenses == null)
            _locationLicenses = LocationLicenses.Instance;

        if (_darkAbilities == null)
            _darkAbilities = DarkAbilities.Instance;

        if (_enableDebugLogs)
        {
            Debug.Log("[ProgressionManager] All subsystems linked");
        }
    }

    /// <summary>
    /// Initializes progression milestones.
    /// </summary>
    private void InitializeMilestones()
    {
        if (_milestones.Count == 0)
        {
            CreateDefaultMilestones();
        }
    }

    /// <summary>
    /// Creates default progression milestones.
    /// </summary>
    private void CreateDefaultMilestones()
    {
        // Early game milestones
        _milestones.Add(new MilestoneData
        {
            milestoneID = "first_catch",
            milestoneName = "First Catch",
            description = "Catch your first fish",
            requirement = new MilestoneRequirement { type = RequirementType.FishCaught, value = 1 },
            reward = new MilestoneReward { money = 50f }
        });

        _milestones.Add(new MilestoneData
        {
            milestoneID = "earn_100",
            milestoneName = "Fisherman's Start",
            description = "Earn your first $100",
            requirement = new MilestoneRequirement { type = RequirementType.MoneyEarned, value = 100 },
            reward = new MilestoneReward { money = 50f }
        });

        _milestones.Add(new MilestoneData
        {
            milestoneID = "first_upgrade",
            milestoneName = "First Upgrade",
            description = "Purchase your first upgrade",
            requirement = new MilestoneRequirement { type = RequirementType.UpgradesPurchased, value = 1 },
            reward = new MilestoneReward { money = 100f }
        });

        // Mid game milestones
        _milestones.Add(new MilestoneData
        {
            milestoneID = "catch_50",
            milestoneName = "Experienced Angler",
            description = "Catch 50 fish",
            requirement = new MilestoneRequirement { type = RequirementType.FishCaught, value = 50 },
            reward = new MilestoneReward { money = 200f, scrap = 50f }
        });

        _milestones.Add(new MilestoneData
        {
            milestoneID = "unlock_3_locations",
            milestoneName = "Explorer",
            description = "Unlock 3 fishing locations",
            requirement = new MilestoneRequirement { type = RequirementType.LocationsUnlocked, value = 3 },
            reward = new MilestoneReward { money = 500f }
        });

        _milestones.Add(new MilestoneData
        {
            milestoneID = "first_relic",
            milestoneName = "Touched by Darkness",
            description = "Find your first relic",
            requirement = new MilestoneRequirement { type = RequirementType.RelicsOwned, value = 1 },
            reward = new MilestoneReward { relics = 1 }
        });

        // Late game milestones
        _milestones.Add(new MilestoneData
        {
            milestoneID = "catch_500",
            milestoneName = "Master Fisherman",
            description = "Catch 500 fish",
            requirement = new MilestoneRequirement { type = RequirementType.FishCaught, value = 500 },
            reward = new MilestoneReward { money = 1000f, relics = 5 }
        });

        _milestones.Add(new MilestoneData
        {
            milestoneID = "unlock_all_locations",
            milestoneName = "Cartographer",
            description = "Unlock all 13 fishing locations",
            requirement = new MilestoneRequirement { type = RequirementType.LocationsUnlocked, value = 13 },
            reward = new MilestoneReward { money = 5000f, relics = 10 }
        });

        _milestones.Add(new MilestoneData
        {
            milestoneID = "unlock_all_abilities",
            milestoneName = "Eldritch Master",
            description = "Unlock all 6 dark abilities",
            requirement = new MilestoneRequirement { type = RequirementType.AbilitiesUnlocked, value = 6 },
            reward = new MilestoneReward { money = 10000f }
        });

        _milestones.Add(new MilestoneData
        {
            milestoneID = "earn_10000",
            milestoneName = "Tycoon",
            description = "Earn $10,000 total",
            requirement = new MilestoneRequirement { type = RequirementType.MoneyEarned, value = 10000 },
            reward = new MilestoneReward { relics = 20 }
        });
    }

    #endregion

    #region Event Subscriptions

    /// <summary>
    /// Subscribes to all relevant progression events.
    /// </summary>
    private void SubscribeToEvents()
    {
        // Economy events
        EventSystem.Subscribe<FishSoldData>("FishSold", OnFishSold);
        EventSystem.Subscribe<BulkSaleData>("BulkFishSold", OnBulkFishSold);
        EventSystem.Subscribe<CurrencyChangeData>("MoneyChanged", OnMoneyChanged);

        // Upgrade events
        EventSystem.Subscribe<UpgradePurchasedData>("UpgradePurchased", OnUpgradePurchased);

        // Location events
        EventSystem.Subscribe<LocationLicenseData>("LocationUnlocked", OnLocationUnlocked);

        // Ability events
        EventSystem.Subscribe<DarkAbilityData>("DarkAbilityUnlocked", OnDarkAbilityUnlocked);

        // Fish caught events
        EventSystem.Subscribe<Fish>("FishCaught", OnFishCaught);
    }

    /// <summary>
    /// Unsubscribes from all events.
    /// </summary>
    private void UnsubscribeFromEvents()
    {
        EventSystem.Unsubscribe<FishSoldData>("FishSold", OnFishSold);
        EventSystem.Unsubscribe<BulkSaleData>("BulkFishSold", OnBulkFishSold);
        EventSystem.Unsubscribe<CurrencyChangeData>("MoneyChanged", OnMoneyChanged);
        EventSystem.Unsubscribe<UpgradePurchasedData>("UpgradePurchased", OnUpgradePurchased);
        EventSystem.Unsubscribe<LocationLicenseData>("LocationUnlocked", OnLocationUnlocked);
        EventSystem.Unsubscribe<DarkAbilityData>("DarkAbilityUnlocked", OnDarkAbilityUnlocked);
        EventSystem.Unsubscribe<Fish>("FishCaught", OnFishCaught);
    }

    #endregion

    #region Event Handlers

    private void OnFishCaught(Fish fish)
    {
        _totalFishCaught++;
        CheckMilestones();
        OnProgressionChanged?.Invoke();
    }

    private void OnFishSold(FishSoldData data)
    {
        _totalMoneyEarned += data.amountEarned;
        CheckMilestones();
        OnProgressionChanged?.Invoke();
    }

    private void OnBulkFishSold(BulkSaleData data)
    {
        _totalMoneyEarned += data.totalEarned;
        CheckMilestones();
        OnProgressionChanged?.Invoke();
    }

    private void OnMoneyChanged(CurrencyChangeData data)
    {
        if (data.changeAmount < 0)
        {
            _totalMoneySpent += Mathf.Abs(data.changeAmount);
        }
        CheckMilestones();
    }

    private void OnUpgradePurchased(UpgradePurchasedData data)
    {
        _upgradesPurchased++;
        CheckMilestones();
        OnProgressionChanged?.Invoke();

        if (_enableDebugLogs)
        {
            Debug.Log($"[ProgressionManager] Upgrade purchased: {data.upgrade.upgradeName} Level {data.newLevel}");
        }
    }

    private void OnLocationUnlocked(LocationLicenseData location)
    {
        _locationsUnlocked++;
        CheckMilestones();
        OnProgressionChanged?.Invoke();

        if (_enableDebugLogs)
        {
            Debug.Log($"[ProgressionManager] Location unlocked: {location.locationName}");
        }
    }

    private void OnDarkAbilityUnlocked(DarkAbilityData ability)
    {
        CheckMilestones();
        OnProgressionChanged?.Invoke();

        if (_enableDebugLogs)
        {
            Debug.Log($"[ProgressionManager] Dark ability unlocked: {ability.abilityName}");
        }
    }

    #endregion

    #region Milestone System

    /// <summary>
    /// Checks all milestones for completion.
    /// </summary>
    private void CheckMilestones()
    {
        foreach (var milestone in _milestones)
        {
            if (!_completedMilestones.Contains(milestone.milestoneID))
            {
                if (IsMilestoneCompleted(milestone))
                {
                    CompleteMilestone(milestone);
                }
            }
        }
    }

    /// <summary>
    /// Checks if a specific milestone is completed.
    /// </summary>
    private bool IsMilestoneCompleted(MilestoneData milestone)
    {
        switch (milestone.requirement.type)
        {
            case RequirementType.FishCaught:
                return _totalFishCaught >= milestone.requirement.value;

            case RequirementType.MoneyEarned:
                return _totalMoneyEarned >= milestone.requirement.value;

            case RequirementType.UpgradesPurchased:
                return _upgradesPurchased >= milestone.requirement.value;

            case RequirementType.LocationsUnlocked:
                return _locationsUnlocked >= milestone.requirement.value;

            case RequirementType.RelicsOwned:
                return _economySystem.GetRelics() >= milestone.requirement.value;

            case RequirementType.AbilitiesUnlocked:
                return _darkAbilities.GetUnlockedAbilities().Count >= milestone.requirement.value;

            default:
                return false;
        }
    }

    /// <summary>
    /// Completes a milestone and grants rewards.
    /// </summary>
    private void CompleteMilestone(MilestoneData milestone)
    {
        _completedMilestones.Add(milestone.milestoneID);

        // Grant rewards
        if (milestone.reward.money > 0)
        {
            _economySystem.AddMoney(milestone.reward.money, $"Milestone: {milestone.milestoneName}");
        }

        if (milestone.reward.scrap > 0)
        {
            _economySystem.AddScrap(milestone.reward.scrap, $"Milestone: {milestone.milestoneName}");
        }

        if (milestone.reward.relics > 0)
        {
            _economySystem.AddRelics(milestone.reward.relics, $"Milestone: {milestone.milestoneName}");
        }

        // Fire events
        OnMilestoneCompleted?.Invoke(milestone);
        EventSystem.Publish("MilestoneCompleted", milestone);

        if (_enableDebugLogs)
        {
            Debug.Log($"[ProgressionManager] Milestone completed: {milestone.milestoneName}");
        }
    }

    /// <summary>
    /// Gets all completed milestones.
    /// </summary>
    public List<MilestoneData> GetCompletedMilestones()
    {
        List<MilestoneData> completed = new List<MilestoneData>();
        foreach (var milestone in _milestones)
        {
            if (_completedMilestones.Contains(milestone.milestoneID))
            {
                completed.Add(milestone);
            }
        }
        return completed;
    }

    /// <summary>
    /// Gets milestones that can be completed now.
    /// </summary>
    public List<MilestoneData> GetAvailableMilestones()
    {
        List<MilestoneData> available = new List<MilestoneData>();
        foreach (var milestone in _milestones)
        {
            if (!_completedMilestones.Contains(milestone.milestoneID) && IsMilestoneCompleted(milestone))
            {
                available.Add(milestone);
            }
        }
        return available;
    }

    #endregion

    #region Progression Calculation

    /// <summary>
    /// Calculates the player's level based on progression.
    /// </summary>
    private int CalculatePlayerLevel()
    {
        // Level is based on overall progression score
        float score = CalculateProgressionScore();
        return Mathf.FloorToInt(score / 100f) + 1; // Every 100 points = 1 level
    }

    /// <summary>
    /// Calculates overall progression score.
    /// </summary>
    private float CalculateProgressionScore()
    {
        float score = 0f;

        // Fish caught (1 point each)
        score += _totalFishCaught;

        // Money earned (1 point per $10)
        score += _totalMoneyEarned / 10f;

        // Upgrades purchased (20 points each)
        score += _upgradesPurchased * 20f;

        // Locations unlocked (50 points each)
        score += _locationsUnlocked * 50f;

        // Abilities unlocked (100 points each)
        if (_darkAbilities != null)
        {
            score += _darkAbilities.GetUnlockedAbilities().Count * 100f;
        }

        // Milestones completed (30 points each)
        score += _completedMilestones.Count * 30f;

        return score;
    }

    #endregion

    #region Public API

    /// <summary>
    /// Gets overall progression statistics.
    /// </summary>
    public ProgressionStats GetProgressionStats()
    {
        return new ProgressionStats
        {
            playerLevel = PlayerLevel,
            progressionScore = ProgressionScore,
            totalFishCaught = _totalFishCaught,
            totalMoneyEarned = _totalMoneyEarned,
            totalMoneySpent = _totalMoneySpent,
            upgradesPurchased = _upgradesPurchased,
            locationsUnlocked = _locationsUnlocked,
            abilitiesUnlocked = _darkAbilities != null ? _darkAbilities.GetUnlockedAbilities().Count : 0,
            milestonesCompleted = _completedMilestones.Count,
            totalMilestones = _milestones.Count
        };
    }

    /// <summary>
    /// Gets a formatted progression summary for UI.
    /// </summary>
    public string GetProgressionSummary()
    {
        var stats = GetProgressionStats();
        return $"Level {stats.playerLevel}\n" +
               $"Fish Caught: {stats.totalFishCaught}\n" +
               $"Money Earned: ${stats.totalMoneyEarned:F2}\n" +
               $"Locations: {stats.locationsUnlocked}/13\n" +
               $"Abilities: {stats.abilitiesUnlocked}/6\n" +
               $"Milestones: {stats.milestonesCompleted}/{stats.totalMilestones}";
    }

    #endregion

    #region Debug Methods

    [ContextMenu("Print Progression Stats")]
    public void PrintProgressionStats()
    {
        var stats = GetProgressionStats();
        Debug.Log("=== Progression Stats ===");
        Debug.Log($"Player Level: {stats.playerLevel}");
        Debug.Log($"Progression Score: {stats.progressionScore:F1}");
        Debug.Log($"Fish Caught: {stats.totalFishCaught}");
        Debug.Log($"Money Earned: ${stats.totalMoneyEarned:F2}");
        Debug.Log($"Money Spent: ${stats.totalMoneySpent:F2}");
        Debug.Log($"Upgrades: {stats.upgradesPurchased}");
        Debug.Log($"Locations: {stats.locationsUnlocked}/13");
        Debug.Log($"Abilities: {stats.abilitiesUnlocked}/6");
        Debug.Log($"Milestones: {stats.milestonesCompleted}/{stats.totalMilestones}");
        Debug.Log("========================");
    }

    [ContextMenu("Complete All Milestones (Debug)")]
    private void DebugCompleteAllMilestones()
    {
        foreach (var milestone in _milestones)
        {
            if (!_completedMilestones.Contains(milestone.milestoneID))
            {
                CompleteMilestone(milestone);
            }
        }
        Debug.Log("[ProgressionManager] All milestones completed (DEBUG)");
    }

    #endregion
}

#region Data Structures

/// <summary>
/// Milestone data.
/// </summary>
[System.Serializable]
public class MilestoneData
{
    public string milestoneID;
    public string milestoneName;
    public string description;
    public MilestoneRequirement requirement;
    public MilestoneReward reward;
}

/// <summary>
/// Milestone requirement.
/// </summary>
[System.Serializable]
public struct MilestoneRequirement
{
    public RequirementType type;
    public float value;
}

/// <summary>
/// Milestone reward.
/// </summary>
[System.Serializable]
public struct MilestoneReward
{
    public float money;
    public float scrap;
    public int relics;
}

/// <summary>
/// Requirement types.
/// </summary>
public enum RequirementType
{
    FishCaught,
    MoneyEarned,
    UpgradesPurchased,
    LocationsUnlocked,
    RelicsOwned,
    AbilitiesUnlocked
}

/// <summary>
/// Progression statistics.
/// </summary>
[System.Serializable]
public struct ProgressionStats
{
    public int playerLevel;
    public float progressionScore;
    public int totalFishCaught;
    public float totalMoneyEarned;
    public float totalMoneySpent;
    public int upgradesPurchased;
    public int locationsUnlocked;
    public int abilitiesUnlocked;
    public int milestonesCompleted;
    public int totalMilestones;
}

#endregion

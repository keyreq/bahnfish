using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Agent 9: Progression & Economy Agent - UpgradeSystem.cs
/// Manages all upgrade logic: ship upgrades, tool upgrades, and equipment.
/// Implements IUpgradeable interface and handles upgrade trees with dependencies.
/// </summary>
public class UpgradeSystem : MonoBehaviour
{
    public static UpgradeSystem Instance { get; private set; }

    [Header("Upgrade Data")]
    [SerializeField] private List<UpgradeData> _allUpgrades = new List<UpgradeData>();
    private Dictionary<string, UpgradeData> _upgradesByID = new Dictionary<string, UpgradeData>();
    private Dictionary<string, int> _currentLevels = new Dictionary<string, int>();

    [Header("Debug")]
    [SerializeField] private bool _enableDebugLogs = true;

    // Events
    public event System.Action<UpgradeData> OnUpgradePurchased;
    public event System.Action<UpgradeData, int> OnUpgradeLevelChanged;
    public event System.Action<string> OnUpgradeAvailable;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeUpgrades();
    }

    private void Start()
    {
        // Subscribe to save/load events
        EventSystem.Subscribe<SaveData>("GatheringSaveData", OnGatheringSaveData);
        EventSystem.Subscribe<SaveData>("ApplyingSaveData", OnApplyingSaveData);

        if (_enableDebugLogs)
        {
            Debug.Log($"[UpgradeSystem] Initialized with {_allUpgrades.Count} upgrades");
        }
    }

    private void OnDestroy()
    {
        EventSystem.Unsubscribe<SaveData>("GatheringSaveData", OnGatheringSaveData);
        EventSystem.Unsubscribe<SaveData>("ApplyingSaveData", OnApplyingSaveData);
    }

    #region Initialization

    /// <summary>
    /// Initializes all upgrade definitions.
    /// </summary>
    private void InitializeUpgrades()
    {
        if (_allUpgrades.Count == 0)
        {
            CreateDefaultUpgrades();
        }

        // Build lookup dictionary
        _upgradesByID.Clear();
        foreach (var upgrade in _allUpgrades)
        {
            if (!_upgradesByID.ContainsKey(upgrade.upgradeID))
            {
                _upgradesByID[upgrade.upgradeID] = upgrade;
                _currentLevels[upgrade.upgradeID] = 0; // Start at level 0
            }
        }
    }

    /// <summary>
    /// Creates default upgrade definitions from design document.
    /// </summary>
    private void CreateDefaultUpgrades()
    {
        // SHIP UPGRADES

        // Hull Upgrades (Storage Capacity)
        _allUpgrades.Add(new UpgradeData
        {
            upgradeID = "hull_upgrade",
            upgradeName = "Hull Storage",
            upgradeCategory = UpgradeCategory.Ship,
            upgradeType = UpgradeType.Hull,
            description = "Increase cargo hold capacity",
            maxLevel = 3,
            costs = new float[] { 500f, 1500f, 3000f },
            effects = new UpgradeEffect[]
            {
                new UpgradeEffect { effectType = EffectType.StorageCapacity, values = new float[] { 120, 144, 225 } } // 10x10 → 10x12 → 12x12 → 15x15
            }
        });

        // Engine Upgrades (Speed)
        _allUpgrades.Add(new UpgradeData
        {
            upgradeID = "engine_upgrade",
            upgradeName = "Engine Power",
            upgradeCategory = UpgradeCategory.Ship,
            upgradeType = UpgradeType.Engine,
            description = "Increase boat speed",
            maxLevel = 3,
            costs = new float[] { 800f, 2000f, 5000f },
            effects = new UpgradeEffect[]
            {
                new UpgradeEffect { effectType = EffectType.SpeedBonus, values = new float[] { 0.10f, 0.25f, 0.50f } } // +10%, +25%, +50%
            }
        });

        // Light Upgrades (Sanity Drain Reduction)
        _allUpgrades.Add(new UpgradeData
        {
            upgradeID = "lights_upgrade",
            upgradeName = "Boat Lights",
            upgradeCategory = UpgradeCategory.Ship,
            upgradeType = UpgradeType.Lights,
            description = "Reduce sanity drain at night",
            maxLevel = 3,
            costs = new float[] { 300f, 700f, 1500f },
            effects = new UpgradeEffect[]
            {
                new UpgradeEffect { effectType = EffectType.SanityDrainReduction, values = new float[] { 0.25f, 0.50f, 0.75f } } // -25%, -50%, -75%
            }
        });

        // Armor Upgrades (Damage Resistance)
        _allUpgrades.Add(new UpgradeData
        {
            upgradeID = "armor_upgrade",
            upgradeName = "Hull Armor",
            upgradeCategory = UpgradeCategory.Ship,
            upgradeType = UpgradeType.Armor,
            description = "Reduce damage from hazards",
            maxLevel = 3,
            costs = new float[] { 600f, 1800f, 4000f },
            effects = new UpgradeEffect[]
            {
                new UpgradeEffect { effectType = EffectType.DamageResistance, values = new float[] { 0.10f, 0.25f, 0.50f } } // 10%, 25%, 50%
            }
        });

        // Fuel Tank Upgrades
        _allUpgrades.Add(new UpgradeData
        {
            upgradeID = "fuel_tank_upgrade",
            upgradeName = "Fuel Tank",
            upgradeCategory = UpgradeCategory.Ship,
            upgradeType = UpgradeType.FuelTank,
            description = "Increase maximum fuel capacity",
            maxLevel = 3,
            costs = new float[] { 400f, 1000f, 2500f },
            effects = new UpgradeEffect[]
            {
                new UpgradeEffect { effectType = EffectType.MaxFuel, values = new float[] { 150f, 200f, 250f } } // 100 → 150 → 200 → 250
            }
        });

        // TOOL UPGRADES

        // Fishing Rod Upgrades
        _allUpgrades.Add(new UpgradeData
        {
            upgradeID = "fishing_rod_upgrade",
            upgradeName = "Fishing Rod",
            upgradeCategory = UpgradeCategory.Tools,
            upgradeType = UpgradeType.FishingRod,
            description = "Increase line strength and reel speed",
            maxLevel = 5,
            costs = new float[] { 100f, 250f, 500f, 1000f, 2000f },
            effects = new UpgradeEffect[]
            {
                new UpgradeEffect { effectType = EffectType.LineStrength, values = new float[] { 1.2f, 1.4f, 1.7f, 2.0f, 2.5f } },
                new UpgradeEffect { effectType = EffectType.ReelSpeed, values = new float[] { 1.1f, 1.2f, 1.4f, 1.6f, 2.0f } }
            }
        });

        // Harpoon Upgrades
        _allUpgrades.Add(new UpgradeData
        {
            upgradeID = "harpoon_upgrade",
            upgradeName = "Harpoon",
            upgradeCategory = UpgradeCategory.Tools,
            upgradeType = UpgradeType.Harpoon,
            description = "Improve accuracy and ammo capacity",
            maxLevel = 3,
            costs = new float[] { 200f, 400f, 800f },
            effects = new UpgradeEffect[]
            {
                new UpgradeEffect { effectType = EffectType.Accuracy, values = new float[] { 1.15f, 1.30f, 1.50f } },
                new UpgradeEffect { effectType = EffectType.AmmoCapacity, values = new float[] { 5f, 8f, 12f } }
            }
        });

        // Net/Pot Upgrades
        _allUpgrades.Add(new UpgradeData
        {
            upgradeID = "net_upgrade",
            upgradeName = "Nets & Pots",
            upgradeCategory = UpgradeCategory.Tools,
            upgradeType = UpgradeType.Nets,
            description = "Increase capacity and catch rate",
            maxLevel = 4,
            costs = new float[] { 150f, 350f, 600f, 1000f },
            effects = new UpgradeEffect[]
            {
                new UpgradeEffect { effectType = EffectType.NetCapacity, values = new float[] { 3f, 5f, 7f, 10f } },
                new UpgradeEffect { effectType = EffectType.CatchRate, values = new float[] { 1.1f, 1.25f, 1.4f, 1.6f } }
            }
        });

        // Dredge Crane Upgrades
        _allUpgrades.Add(new UpgradeData
        {
            upgradeID = "dredge_upgrade",
            upgradeName = "Dredge Crane",
            upgradeCategory = UpgradeCategory.Tools,
            upgradeType = UpgradeType.Dredge,
            description = "Faster dredging and deeper reach",
            maxLevel = 3,
            costs = new float[] { 300f, 700f, 1500f },
            effects = new UpgradeEffect[]
            {
                new UpgradeEffect { effectType = EffectType.DredgeSpeed, values = new float[] { 1.2f, 1.5f, 2.0f } },
                new UpgradeEffect { effectType = EffectType.MaxDepth, values = new float[] { 30f, 50f, 80f } }
            }
        });
    }

    #endregion

    #region Purchase & Upgrade

    /// <summary>
    /// Attempts to purchase an upgrade level.
    /// </summary>
    public bool PurchaseUpgrade(string upgradeID)
    {
        if (!_upgradesByID.TryGetValue(upgradeID, out UpgradeData upgrade))
        {
            Debug.LogWarning($"[UpgradeSystem] Unknown upgrade ID: {upgradeID}");
            return false;
        }

        int currentLevel = GetUpgradeLevel(upgradeID);

        // Check if already at max level
        if (currentLevel >= upgrade.maxLevel)
        {
            if (_enableDebugLogs)
            {
                Debug.Log($"[UpgradeSystem] {upgrade.upgradeName} already at max level");
            }
            return false;
        }

        // Check prerequisites
        if (!CheckPrerequisites(upgrade, currentLevel))
        {
            if (_enableDebugLogs)
            {
                Debug.Log($"[UpgradeSystem] Prerequisites not met for {upgrade.upgradeName}");
            }
            return false;
        }

        // Get cost for next level
        float cost = upgrade.GetCostForLevel(currentLevel);

        // Check currency type and spend
        bool success = false;
        if (upgrade.currencyType == CurrencyType.Money)
        {
            success = EconomySystem.Instance.SpendMoney(cost, $"{upgrade.upgradeName} Level {currentLevel + 1}");
        }
        else if (upgrade.currencyType == CurrencyType.Scrap)
        {
            success = EconomySystem.Instance.SpendScrap(cost, $"{upgrade.upgradeName} Level {currentLevel + 1}");
        }

        if (success)
        {
            // Increase level
            _currentLevels[upgradeID] = currentLevel + 1;

            // Apply upgrade effects
            ApplyUpgradeEffects(upgrade, currentLevel + 1);

            // Fire events
            OnUpgradePurchased?.Invoke(upgrade);
            OnUpgradeLevelChanged?.Invoke(upgrade, currentLevel + 1);
            EventSystem.Publish("UpgradePurchased", new UpgradePurchasedData(upgrade, currentLevel + 1, cost));

            if (_enableDebugLogs)
            {
                Debug.Log($"[UpgradeSystem] Purchased {upgrade.upgradeName} Level {currentLevel + 1} for ${cost:F2}");
            }

            // Check for newly available upgrades
            CheckNewlyAvailableUpgrades();

            return true;
        }

        return false;
    }

    /// <summary>
    /// Checks if prerequisites are met for an upgrade.
    /// </summary>
    private bool CheckPrerequisites(UpgradeData upgrade, int currentLevel)
    {
        if (upgrade.prerequisites == null || upgrade.prerequisites.Length == 0)
        {
            return true;
        }

        foreach (var prereq in upgrade.prerequisites)
        {
            int prereqLevel = GetUpgradeLevel(prereq.requiredUpgradeID);
            if (prereqLevel < prereq.requiredLevel)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Applies the effects of an upgrade.
    /// </summary>
    private void ApplyUpgradeEffects(UpgradeData upgrade, int level)
    {
        foreach (var effect in upgrade.effects)
        {
            float value = effect.GetValueForLevel(level);

            switch (effect.effectType)
            {
                case EffectType.StorageCapacity:
                    ApplyStorageCapacityUpgrade((int)value);
                    break;

                case EffectType.SpeedBonus:
                    ApplySpeedBonus(value);
                    break;

                case EffectType.SanityDrainReduction:
                    ApplySanityDrainReduction(value);
                    break;

                case EffectType.DamageResistance:
                    ApplyDamageResistance(value);
                    break;

                case EffectType.MaxFuel:
                    ApplyMaxFuel(value);
                    break;

                case EffectType.LineStrength:
                case EffectType.ReelSpeed:
                case EffectType.Accuracy:
                case EffectType.AmmoCapacity:
                case EffectType.NetCapacity:
                case EffectType.CatchRate:
                case EffectType.DredgeSpeed:
                case EffectType.MaxDepth:
                    // These are applied directly by the relevant systems
                    EventSystem.Publish("UpgradeEffectApplied", new UpgradeEffectData(effect.effectType, value));
                    break;
            }
        }
    }

    #endregion

    #region Effect Application

    private void ApplyStorageCapacityUpgrade(int newGridSize)
    {
        // Notify InventoryManager to resize grid
        EventSystem.Publish("StorageCapacityUpgraded", newGridSize);
        if (_enableDebugLogs)
        {
            Debug.Log($"[UpgradeSystem] Storage capacity upgraded to {newGridSize} cells");
        }
    }

    private void ApplySpeedBonus(float bonus)
    {
        // Notify BoatController
        EventSystem.Publish("SpeedBonusApplied", bonus);
        if (_enableDebugLogs)
        {
            Debug.Log($"[UpgradeSystem] Speed bonus applied: +{bonus * 100f:F0}%");
        }
    }

    private void ApplySanityDrainReduction(float reduction)
    {
        // Notify SanityManager
        EventSystem.Publish("SanityDrainReductionApplied", reduction);
        if (_enableDebugLogs)
        {
            Debug.Log($"[UpgradeSystem] Sanity drain reduction applied: -{reduction * 100f:F0}%");
        }
    }

    private void ApplyDamageResistance(float resistance)
    {
        // Notify damage system
        EventSystem.Publish("DamageResistanceApplied", resistance);
        if (_enableDebugLogs)
        {
            Debug.Log($"[UpgradeSystem] Damage resistance applied: {resistance * 100f:F0}%");
        }
    }

    private void ApplyMaxFuel(float maxFuel)
    {
        // Update GameState
        if (GameManager.Instance != null)
        {
            GameState state = GameManager.Instance.CurrentGameState;
            state.fuel = Mathf.Min(state.fuel, maxFuel);
            GameManager.Instance.UpdateGameState(state);
        }
        EventSystem.Publish("MaxFuelUpgraded", maxFuel);
        if (_enableDebugLogs)
        {
            Debug.Log($"[UpgradeSystem] Max fuel upgraded to {maxFuel}");
        }
    }

    #endregion

    #region Query Methods

    /// <summary>
    /// Gets the current level of an upgrade.
    /// </summary>
    public int GetUpgradeLevel(string upgradeID)
    {
        return _currentLevels.TryGetValue(upgradeID, out int level) ? level : 0;
    }

    /// <summary>
    /// Gets an upgrade by ID.
    /// </summary>
    public UpgradeData GetUpgrade(string upgradeID)
    {
        return _upgradesByID.TryGetValue(upgradeID, out UpgradeData upgrade) ? upgrade : null;
    }

    /// <summary>
    /// Checks if an upgrade can be purchased.
    /// </summary>
    public bool CanPurchaseUpgrade(string upgradeID)
    {
        if (!_upgradesByID.TryGetValue(upgradeID, out UpgradeData upgrade))
        {
            return false;
        }

        int currentLevel = GetUpgradeLevel(upgradeID);

        // Check max level
        if (currentLevel >= upgrade.maxLevel) return false;

        // Check prerequisites
        if (!CheckPrerequisites(upgrade, currentLevel)) return false;

        // Check if can afford
        float cost = upgrade.GetCostForLevel(currentLevel);
        if (upgrade.currencyType == CurrencyType.Money)
        {
            return EconomySystem.Instance.CanAffordMoney(cost);
        }
        else if (upgrade.currencyType == CurrencyType.Scrap)
        {
            return EconomySystem.Instance.CanAffordScrap(cost);
        }

        return false;
    }

    /// <summary>
    /// Gets all upgrades of a specific category.
    /// </summary>
    public List<UpgradeData> GetUpgradesByCategory(UpgradeCategory category)
    {
        return _allUpgrades.Where(u => u.upgradeCategory == category).ToList();
    }

    /// <summary>
    /// Gets all available upgrades (purchasable right now).
    /// </summary>
    public List<UpgradeData> GetAvailableUpgrades()
    {
        return _allUpgrades.Where(u => CanPurchaseUpgrade(u.upgradeID)).ToList();
    }

    /// <summary>
    /// Checks for newly available upgrades and fires events.
    /// </summary>
    private void CheckNewlyAvailableUpgrades()
    {
        var available = GetAvailableUpgrades();
        foreach (var upgrade in available)
        {
            OnUpgradeAvailable?.Invoke(upgrade.upgradeID);
        }
    }

    /// <summary>
    /// Gets a specific upgrade effect value.
    /// </summary>
    public float GetUpgradeEffectValue(string upgradeID, EffectType effectType)
    {
        if (!_upgradesByID.TryGetValue(upgradeID, out UpgradeData upgrade))
        {
            return 0f;
        }

        int currentLevel = GetUpgradeLevel(upgradeID);
        if (currentLevel == 0) return 0f;

        var effect = upgrade.effects.FirstOrDefault(e => e.effectType == effectType);
        if (effect != null)
        {
            return effect.GetValueForLevel(currentLevel);
        }

        return 0f;
    }

    #endregion

    #region Save/Load

    private void OnGatheringSaveData(SaveData data)
    {
        data.purchasedUpgrades.Clear();
        data.upgradelevels.Clear();

        foreach (var kvp in _currentLevels)
        {
            if (kvp.Value > 0)
            {
                data.purchasedUpgrades.Add(kvp.Key);
                data.upgradelevels[kvp.Key] = kvp.Value;
            }
        }

        if (_enableDebugLogs)
        {
            Debug.Log($"[UpgradeSystem] Saved {data.purchasedUpgrades.Count} purchased upgrades");
        }
    }

    private void OnApplyingSaveData(SaveData data)
    {
        // Restore upgrade levels
        _currentLevels.Clear();
        foreach (var upgradeID in _upgradesByID.Keys)
        {
            _currentLevels[upgradeID] = 0;
        }

        foreach (var kvp in data.upgradelevels)
        {
            _currentLevels[kvp.Key] = kvp.Value;

            // Reapply upgrade effects
            if (_upgradesByID.TryGetValue(kvp.Key, out UpgradeData upgrade))
            {
                ApplyUpgradeEffects(upgrade, kvp.Value);
            }
        }

        if (_enableDebugLogs)
        {
            Debug.Log($"[UpgradeSystem] Loaded {data.upgradelevels.Count} upgrade levels");
        }
    }

    #endregion

    #region Debug Methods

    [ContextMenu("Print All Upgrades")]
    public void PrintAllUpgrades()
    {
        Debug.Log($"=== All Upgrades ({_allUpgrades.Count}) ===");
        foreach (var upgrade in _allUpgrades)
        {
            int level = GetUpgradeLevel(upgrade.upgradeID);
            Debug.Log($"{upgrade.upgradeName}: Level {level}/{upgrade.maxLevel}");
        }
    }

    [ContextMenu("Purchase Fishing Rod Upgrade")]
    private void DebugPurchaseFishingRod()
    {
        PurchaseUpgrade("fishing_rod_upgrade");
    }

    #endregion
}

#region Data Structures

[System.Serializable]
public class UpgradeData
{
    public string upgradeID;
    public string upgradeName;
    public UpgradeCategory upgradeCategory;
    public UpgradeType upgradeType;
    public string description;
    public int maxLevel;
    public float[] costs;
    public CurrencyType currencyType = CurrencyType.Money;
    public UpgradeEffect[] effects;
    public UpgradePrerequisite[] prerequisites;

    public float GetCostForLevel(int level)
    {
        if (level >= 0 && level < costs.Length)
        {
            return costs[level];
        }
        return 0f;
    }
}

[System.Serializable]
public class UpgradeEffect
{
    public EffectType effectType;
    public float[] values; // Values per level

    public float GetValueForLevel(int level)
    {
        if (level > 0 && level <= values.Length)
        {
            return values[level - 1];
        }
        return 0f;
    }
}

[System.Serializable]
public struct UpgradePrerequisite
{
    public string requiredUpgradeID;
    public int requiredLevel;
}

public enum UpgradeCategory
{
    Ship,
    Tools,
    Equipment,
    Special
}

public enum UpgradeType
{
    Hull,
    Engine,
    Lights,
    Armor,
    FuelTank,
    FishingRod,
    Harpoon,
    Nets,
    Dredge
}

public enum EffectType
{
    StorageCapacity,
    SpeedBonus,
    SanityDrainReduction,
    DamageResistance,
    MaxFuel,
    LineStrength,
    ReelSpeed,
    Accuracy,
    AmmoCapacity,
    NetCapacity,
    CatchRate,
    DredgeSpeed,
    MaxDepth
}

public enum CurrencyType
{
    Money,
    Scrap,
    Relics
}

[System.Serializable]
public struct UpgradePurchasedData
{
    public UpgradeData upgrade;
    public int newLevel;
    public float cost;

    public UpgradePurchasedData(UpgradeData upgrade, int newLevel, float cost)
    {
        this.upgrade = upgrade;
        this.newLevel = newLevel;
        this.cost = cost;
    }
}

[System.Serializable]
public struct UpgradeEffectData
{
    public EffectType effectType;
    public float value;

    public UpgradeEffectData(EffectType effectType, float value)
    {
        this.effectType = effectType;
        this.value = value;
    }
}

#endregion

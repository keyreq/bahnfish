using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Agent 20: Idle/AFK Progression System - IdleUpgradeSystem.cs
/// Manages all idle-specific upgrades and their effects.
/// Includes Auto-Fisher, Quality Rod Holder, Auto-Sell, Efficiency Boost,
/// Time Compression, Storage Expansion, Crew Autonomy, and Breeding Automation.
/// </summary>
public class IdleUpgradeSystem : MonoBehaviour
{
    [Header("Owned Upgrades")]
    [SerializeField] private List<string> ownedUpgrades = new List<string>();
    [SerializeField] private Dictionary<string, int> upgradeLevels = new Dictionary<string, int>();

    [Header("Storage Caps")]
    [SerializeField] private float baseStorageCap = 10000f;
    [SerializeField] private float[] storageCapLevels = { 25000f, 50000f, 100000f };

    [Header("Time Compression Multipliers")]
    [SerializeField] private float[] timeCompressionLevels = { 1.5f, 2f, 2.5f, 3f };

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogging = true;

    // Upgrade database
    private Dictionary<string, IdleUpgrade> upgradeDatabase;

    private void Awake()
    {
        InitializeUpgradeDatabase();
    }

    /// <summary>
    /// Initializes the upgrade database with all idle upgrades.
    /// </summary>
    private void InitializeUpgradeDatabase()
    {
        upgradeDatabase = new Dictionary<string, IdleUpgrade>();

        // Tier 1 - Basic Idle
        upgradeDatabase["auto_fisher"] = new IdleUpgrade
        {
            id = "auto_fisher",
            name = "Auto-Fisher",
            description = "Enables basic idle fishing while offline",
            baseCost = 5000f,
            tier = 1,
            maxLevel = 1,
            unlockRequirement = ""
        };

        upgradeDatabase["auto_sell"] = new IdleUpgrade
        {
            id = "auto_sell",
            name = "Auto-Sell",
            description = "Automatically sells fish when inventory is full",
            baseCost = 2000f,
            tier = 1,
            maxLevel = 1,
            unlockRequirement = "auto_fisher"
        };

        // Tier 2 - Efficiency (Levels 1-3)
        upgradeDatabase["quality_rod_holder"] = new IdleUpgrade
        {
            id = "quality_rod_holder",
            name = "Quality Rod Holder",
            description = "Increases idle catch rate by 20% per level",
            baseCost = 3000f,
            costIncreasePerLevel = 0f,
            tier = 2,
            maxLevel = 5,
            unlockRequirement = "auto_fisher"
        };

        upgradeDatabase["efficiency_boost"] = new IdleUpgrade
        {
            id = "efficiency_boost",
            name = "Efficiency Boost",
            description = "Increases idle income by 10% per level",
            baseCost = 2000f,
            costIncreasePerLevel = 2000f,
            tier = 2,
            maxLevel = 10,
            unlockRequirement = "auto_fisher"
        };

        // Tier 3 - Automation
        upgradeDatabase["crew_autonomy"] = new IdleUpgrade
        {
            id = "crew_autonomy",
            name = "Crew Autonomy",
            description = "Crew members work while you're offline",
            baseCost = 10000f,
            tier = 3,
            maxLevel = 1,
            unlockRequirement = "auto_fisher"
        };

        upgradeDatabase["breeding_automation"] = new IdleUpgrade
        {
            id = "breeding_automation",
            name = "Breeding Automation",
            description = "Automatically breeds compatible fish pairs",
            baseCost = 8000f,
            tier = 3,
            maxLevel = 1,
            unlockRequirement = "auto_fisher"
        };

        upgradeDatabase["storage_expansion"] = new IdleUpgrade
        {
            id = "storage_expansion",
            name = "Storage Expansion",
            description = "Increases offline earnings cap (25k, 50k, 100k)",
            baseCost = 10000f,
            costIncreasePerLevel = 10000f,
            tier = 3,
            maxLevel = 3,
            unlockRequirement = "auto_fisher"
        };

        // Tier 4 - Advanced
        upgradeDatabase["time_compression"] = new IdleUpgrade
        {
            id = "time_compression",
            name = "Time Compression",
            description = "Faster offline time progression (1.5x to 3x)",
            baseCost = 5000f,
            costIncreasePerLevel = 10000f,
            tier = 4,
            maxLevel = 4,
            unlockRequirement = "efficiency_boost"
        };
    }

    /// <summary>
    /// Checks if player owns a specific upgrade.
    /// </summary>
    /// <param name="upgradeID">Upgrade identifier</param>
    /// <returns>True if owned</returns>
    public bool HasUpgrade(string upgradeID)
    {
        return ownedUpgrades.Contains(upgradeID);
    }

    /// <summary>
    /// Gets the current level of an upgrade.
    /// </summary>
    /// <param name="upgradeID">Upgrade identifier</param>
    /// <returns>Current level (0 if not owned)</returns>
    public int GetUpgradeLevel(string upgradeID)
    {
        if (!upgradeLevels.ContainsKey(upgradeID))
        {
            return 0;
        }
        return upgradeLevels[upgradeID];
    }

    /// <summary>
    /// Purchases an idle upgrade.
    /// </summary>
    /// <param name="upgradeID">Upgrade identifier</param>
    /// <returns>True if purchase was successful</returns>
    public bool PurchaseUpgrade(string upgradeID)
    {
        if (!upgradeDatabase.ContainsKey(upgradeID))
        {
            Debug.LogError($"[IdleUpgradeSystem] Upgrade {upgradeID} not found in database!");
            return false;
        }

        IdleUpgrade upgrade = upgradeDatabase[upgradeID];

        // Check unlock requirement
        if (!string.IsNullOrEmpty(upgrade.unlockRequirement) && !HasUpgrade(upgrade.unlockRequirement))
        {
            if (enableDebugLogging)
            {
                Debug.LogWarning($"[IdleUpgradeSystem] Cannot purchase {upgradeID} - requirement not met: {upgrade.unlockRequirement}");
            }
            return false;
        }

        // Get current level
        int currentLevel = GetUpgradeLevel(upgradeID);

        // Check max level
        if (currentLevel >= upgrade.maxLevel)
        {
            if (enableDebugLogging)
            {
                Debug.LogWarning($"[IdleUpgradeSystem] {upgradeID} is already at max level ({upgrade.maxLevel})");
            }
            return false;
        }

        // Calculate cost
        float cost = CalculateUpgradeCost(upgradeID, currentLevel + 1);

        // Check if player can afford (publish event to query money)
        // TODO: Integrate with economy system
        // For now, assume purchase is valid

        // Add to owned upgrades (if first level)
        if (currentLevel == 0)
        {
            ownedUpgrades.Add(upgradeID);
        }

        // Increment level
        if (!upgradeLevels.ContainsKey(upgradeID))
        {
            upgradeLevels[upgradeID] = 1;
        }
        else
        {
            upgradeLevels[upgradeID]++;
        }

        // Deduct money
        EventSystem.Publish("SpendMoney", cost);

        // Publish upgrade purchased event
        EventSystem.Publish("IdleUpgradePurchased", upgradeID);

        if (enableDebugLogging)
        {
            Debug.Log($"[IdleUpgradeSystem] Purchased {upgrade.name} (Level {upgradeLevels[upgradeID]}) for ${cost:F2}");
        }

        return true;
    }

    /// <summary>
    /// Calculates the cost of an upgrade at a specific level.
    /// </summary>
    /// <param name="upgradeID">Upgrade identifier</param>
    /// <param name="level">Target level</param>
    /// <returns>Cost in currency</returns>
    public float CalculateUpgradeCost(string upgradeID, int level)
    {
        if (!upgradeDatabase.ContainsKey(upgradeID))
        {
            return 0f;
        }

        IdleUpgrade upgrade = upgradeDatabase[upgradeID];

        if (level <= 0 || level > upgrade.maxLevel)
        {
            return 0f;
        }

        // Calculate cost based on level
        return upgrade.baseCost + ((level - 1) * upgrade.costIncreasePerLevel);
    }

    /// <summary>
    /// Gets the maximum offline storage cap based on Storage Expansion level.
    /// </summary>
    /// <returns>Maximum offline earnings cap</returns>
    public float GetMaxOfflineStorage()
    {
        int storageLevel = GetUpgradeLevel("storage_expansion");

        if (storageLevel == 0)
        {
            return baseStorageCap;
        }

        return storageCapLevels[Mathf.Min(storageLevel - 1, storageCapLevels.Length - 1)];
    }

    /// <summary>
    /// Gets the time compression multiplier based on Time Compression level.
    /// </summary>
    /// <returns>Time compression multiplier</returns>
    public float GetTimeCompressionMultiplier()
    {
        int compressionLevel = GetUpgradeLevel("time_compression");

        if (compressionLevel == 0)
        {
            return 1f; // No compression
        }

        return timeCompressionLevels[Mathf.Min(compressionLevel - 1, timeCompressionLevels.Length - 1)];
    }

    /// <summary>
    /// Gets the overall idle efficiency multiplier.
    /// </summary>
    /// <returns>Idle efficiency multiplier</returns>
    public float GetIdleEfficiencyMultiplier()
    {
        if (!HasUpgrade("auto_fisher"))
        {
            return 0f; // Idle progression disabled
        }

        float efficiency = 1f;

        // Quality Rod Holder: +20% per level
        int rodHolderLevel = GetUpgradeLevel("quality_rod_holder");
        efficiency += rodHolderLevel * 0.2f;

        // Efficiency Boost: +10% per level
        int efficiencyLevel = GetUpgradeLevel("efficiency_boost");
        efficiency += efficiencyLevel * 0.1f;

        // Time Compression
        efficiency *= GetTimeCompressionMultiplier();

        return efficiency;
    }

    /// <summary>
    /// Gets all available upgrades.
    /// </summary>
    /// <returns>List of all upgrades</returns>
    public List<IdleUpgrade> GetAllUpgrades()
    {
        return new List<IdleUpgrade>(upgradeDatabase.Values);
    }

    /// <summary>
    /// Gets all upgrades that can currently be purchased.
    /// </summary>
    /// <returns>List of purchasable upgrades</returns>
    public List<IdleUpgrade> GetAvailableUpgrades()
    {
        List<IdleUpgrade> available = new List<IdleUpgrade>();

        foreach (var upgrade in upgradeDatabase.Values)
        {
            // Check unlock requirement
            if (!string.IsNullOrEmpty(upgrade.unlockRequirement) && !HasUpgrade(upgrade.unlockRequirement))
            {
                continue;
            }

            // Check if not at max level
            int currentLevel = GetUpgradeLevel(upgrade.id);
            if (currentLevel < upgrade.maxLevel)
            {
                available.Add(upgrade);
            }
        }

        return available;
    }

    /// <summary>
    /// Gets upgrade info by ID.
    /// </summary>
    /// <param name="upgradeID">Upgrade identifier</param>
    /// <returns>Upgrade data or null if not found</returns>
    public IdleUpgrade GetUpgrade(string upgradeID)
    {
        if (upgradeDatabase.ContainsKey(upgradeID))
        {
            return upgradeDatabase[upgradeID];
        }
        return null;
    }

    /// <summary>
    /// Loads upgrade state from save data.
    /// </summary>
    /// <param name="ownedUpgradesList">List of owned upgrades</param>
    /// <param name="levelsDict">Dictionary of upgrade levels</param>
    public void LoadUpgradeState(List<string> ownedUpgradesList, Dictionary<string, int> levelsDict)
    {
        ownedUpgrades = new List<string>(ownedUpgradesList);
        upgradeLevels = new Dictionary<string, int>(levelsDict);

        if (enableDebugLogging)
        {
            Debug.Log($"[IdleUpgradeSystem] Loaded {ownedUpgrades.Count} owned upgrades");
        }
    }

    /// <summary>
    /// Gets the current upgrade state for saving.
    /// </summary>
    /// <param name="ownedUpgradesList">Output list of owned upgrades</param>
    /// <param name="levelsDict">Output dictionary of upgrade levels</param>
    public void GetUpgradeState(out List<string> ownedUpgradesList, out Dictionary<string, int> levelsDict)
    {
        ownedUpgradesList = new List<string>(ownedUpgrades);
        levelsDict = new Dictionary<string, int>(upgradeLevels);
    }

#if UNITY_EDITOR
    [ContextMenu("Print All Upgrades")]
    private void PrintAllUpgrades()
    {
        Debug.Log("=== Idle Upgrades Database ===");
        foreach (var upgrade in upgradeDatabase.Values)
        {
            int currentLevel = GetUpgradeLevel(upgrade.id);
            bool owned = HasUpgrade(upgrade.id);
            Debug.Log($"{upgrade.name} (Tier {upgrade.tier}):");
            Debug.Log($"  ID: {upgrade.id}");
            Debug.Log($"  Description: {upgrade.description}");
            Debug.Log($"  Base Cost: ${upgrade.baseCost:F2}");
            Debug.Log($"  Max Level: {upgrade.maxLevel}");
            Debug.Log($"  Current Level: {currentLevel}");
            Debug.Log($"  Owned: {owned}");
            Debug.Log($"  Unlock Requirement: {(string.IsNullOrEmpty(upgrade.unlockRequirement) ? "None" : upgrade.unlockRequirement)}");
            Debug.Log("");
        }
    }

    [ContextMenu("Print Owned Upgrades")]
    private void PrintOwnedUpgrades()
    {
        Debug.Log($"=== Owned Idle Upgrades ({ownedUpgrades.Count}) ===");
        foreach (string upgradeID in ownedUpgrades)
        {
            if (upgradeDatabase.ContainsKey(upgradeID))
            {
                IdleUpgrade upgrade = upgradeDatabase[upgradeID];
                int level = GetUpgradeLevel(upgradeID);
                Debug.Log($"{upgrade.name} - Level {level}/{upgrade.maxLevel}");
            }
        }
        Debug.Log($"");
        Debug.Log($"Idle Efficiency: {GetIdleEfficiencyMultiplier():F2}x");
        Debug.Log($"Max Offline Storage: ${GetMaxOfflineStorage():F2}");
        Debug.Log($"Time Compression: {GetTimeCompressionMultiplier():F2}x");
    }

    [ContextMenu("Test Purchase - Auto-Fisher")]
    private void TestPurchaseAutoFisher()
    {
        bool success = PurchaseUpgrade("auto_fisher");
        Debug.Log($"Auto-Fisher purchase: {(success ? "SUCCESS" : "FAILED")}");
    }

    [ContextMenu("Grant All Basic Upgrades")]
    private void GrantAllBasicUpgrades()
    {
        PurchaseUpgrade("auto_fisher");
        PurchaseUpgrade("auto_sell");
        Debug.Log("[IdleUpgradeSystem] Granted all basic upgrades");
    }

    [ContextMenu("Grant All Upgrades (Max Level)")]
    private void GrantAllUpgradesMaxLevel()
    {
        foreach (var upgrade in upgradeDatabase.Values)
        {
            for (int i = 0; i < upgrade.maxLevel; i++)
            {
                PurchaseUpgrade(upgrade.id);
            }
        }
        Debug.Log("[IdleUpgradeSystem] Granted all upgrades at max level");
        PrintOwnedUpgrades();
    }
#endif
}

/// <summary>
/// Data structure for an idle upgrade.
/// </summary>
[System.Serializable]
public class IdleUpgrade
{
    public string id;
    public string name;
    public string description;
    public float baseCost;
    public float costIncreasePerLevel;
    public int tier;
    public int maxLevel;
    public string unlockRequirement; // ID of required upgrade
}

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Agent 16: Aquarium & Breeding Specialist - FishTankData.cs
/// ScriptableObject that defines tank specifications and properties.
/// Used by AquariumManager to manage tank data.
/// </summary>
[CreateAssetMenu(fileName = "NewFishTank", menuName = "Bahnfish/Fish Tank", order = 2)]
public class FishTankData : ScriptableObject
{
    [Header("Basic Info")]
    [Tooltip("Unique identifier for this tank type")]
    public string tankID = "";

    [Tooltip("Display name of the tank")]
    public string tankName = "Basic Tank";

    [Tooltip("Size category of the tank")]
    public TankSize size = TankSize.Small;

    [TextArea(3, 5)]
    [Tooltip("Description of the tank")]
    public string description = "";

    [Header("Capacity")]
    [Tooltip("Maximum number of fish this tank can hold")]
    public int baseCapacity = 5;

    [Tooltip("Additional capacity per upgrade level")]
    public int capacityPerUpgrade = 5;

    [Tooltip("Maximum upgrade level for capacity")]
    public int maxCapacityUpgrades = 4;

    [Header("Environment")]
    [Tooltip("Type of environment this tank supports")]
    public EnvironmentType environmentType = EnvironmentType.Freshwater;

    [Tooltip("Can this tank hold aberrant fish?")]
    public bool supportsAberrant = false;

    [Tooltip("Environment quality (affects fish happiness)")]
    [Range(0f, 1f)]
    public float baseEnvironmentQuality = 0.7f;

    [Header("Economics")]
    [Tooltip("Cost to purchase this tank")]
    public float purchaseCost = 500f;

    [Tooltip("Daily maintenance cost")]
    public float dailyMaintenanceCost = 5f;

    [Tooltip("Base daily exhibition income per fish")]
    public float baseIncomePerFish = 2f;

    [Tooltip("Multiplier for rare fish exhibition income")]
    public float rareIncomeMultiplier = 5f;

    [Header("Visual & Theme")]
    [Tooltip("Icon for tank in UI")]
    public Sprite tankIcon;

    [Tooltip("Visual theme/decoration style")]
    public TankTheme theme = TankTheme.Natural;

    [Tooltip("Background color for tank display")]
    public Color backgroundColor = new Color(0.1f, 0.3f, 0.5f);

    [Tooltip("Available decoration prefabs")]
    public List<GameObject> decorationPrefabs = new List<GameObject>();

    [Header("Unlocking")]
    [Tooltip("Required player level to unlock")]
    public int requiredLevel = 1;

    [Tooltip("Previous tank that must be owned (leave empty for starter tanks)")]
    public string prerequisiteTankID = "";

    [Tooltip("Required fish species discovered count")]
    public int requiredSpeciesCount = 0;

    [Header("Special Features")]
    [Tooltip("Has built-in breeding chamber")]
    public bool hasBreedingChamber = false;

    [Tooltip("Breeding speed multiplier (if has chamber)")]
    [Range(1f, 2f)]
    public float breedingSpeedMultiplier = 1f;

    [Tooltip("Has genetics lab features")]
    public bool hasGeneticsLab = false;

    [Tooltip("Bonus mutation chance (if has genetics lab)")]
    [Range(0f, 0.02f)]
    public float bonusMutationChance = 0f;

    /// <summary>
    /// Calculates maximum capacity with upgrades.
    /// </summary>
    public int GetMaxCapacity(int upgradeLevel)
    {
        int level = Mathf.Min(upgradeLevel, maxCapacityUpgrades);
        return baseCapacity + (level * capacityPerUpgrade);
    }

    /// <summary>
    /// Calculates daily exhibition income for a specific fish rarity.
    /// </summary>
    public float GetDailyIncome(FishRarity rarity, int fishCount)
    {
        float baseIncome = baseIncomePerFish * fishCount;

        // Apply rarity multiplier
        float rarityMult = 1f;
        switch (rarity)
        {
            case FishRarity.Common:
                rarityMult = 1f;
                break;
            case FishRarity.Uncommon:
                rarityMult = 2f;
                break;
            case FishRarity.Rare:
                rarityMult = rareIncomeMultiplier;
                break;
            case FishRarity.Legendary:
                rarityMult = rareIncomeMultiplier * 3f;
                break;
            case FishRarity.Aberrant:
                rarityMult = rareIncomeMultiplier * 2f;
                break;
        }

        return baseIncome * rarityMult;
    }

    /// <summary>
    /// Gets the total cost to upgrade capacity to a specific level.
    /// </summary>
    public float GetCapacityUpgradeCost(int targetLevel)
    {
        if (targetLevel <= 0 || targetLevel > maxCapacityUpgrades)
        {
            return 0f;
        }

        // Progressive cost: 100, 250, 500, 1000
        float baseCost = 100f;
        float costMultiplier = Mathf.Pow(2.5f, targetLevel - 1);
        return baseCost * costMultiplier;
    }

    /// <summary>
    /// Checks if a fish species can be placed in this tank.
    /// </summary>
    public bool CanHoldFish(FishSpeciesData fishSpecies)
    {
        if (fishSpecies == null)
        {
            return false;
        }

        // Check aberrant compatibility
        if (fishSpecies.isAberrant && !supportsAberrant)
        {
            return false;
        }

        // Check environment type
        return IsCompatibleEnvironment(fishSpecies);
    }

    /// <summary>
    /// Checks if the fish's environment matches this tank.
    /// </summary>
    private bool IsCompatibleEnvironment(FishSpeciesData fishSpecies)
    {
        // This is a simplified check - in full implementation, you'd check depth ranges
        // For now, we'll use location-based logic
        bool isFreshwater = fishSpecies.allowedLocations.Contains("starter_lake") ||
                           fishSpecies.allowedLocations.Contains("misty_river");

        bool isSaltwater = fishSpecies.allowedLocations.Contains("coral_reef") ||
                          fishSpecies.allowedLocations.Contains("open_ocean");

        bool isDeepSea = fishSpecies.maxDepth > 100f;

        switch (environmentType)
        {
            case EnvironmentType.Freshwater:
                return isFreshwater;
            case EnvironmentType.Saltwater:
                return isSaltwater;
            case EnvironmentType.DeepSea:
                return isDeepSea;
            case EnvironmentType.Mixed:
                return true; // Mixed tanks can hold anything
            default:
                return false;
        }
    }

    /// <summary>
    /// Gets environment quality with upgrades.
    /// </summary>
    public float GetEnvironmentQuality(int filtrationLevel)
    {
        float quality = baseEnvironmentQuality;
        quality += filtrationLevel * 0.075f; // +7.5% per level
        return Mathf.Clamp01(quality);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Auto-generate ID from name if empty
        if (string.IsNullOrEmpty(tankID) && !string.IsNullOrEmpty(tankName))
        {
            tankID = tankName.ToLower().Replace(" ", "_");
        }

        // Ensure capacity values are valid
        if (baseCapacity < 1)
        {
            baseCapacity = 1;
        }

        if (capacityPerUpgrade < 1)
        {
            capacityPerUpgrade = 1;
        }

        // Ensure costs are valid
        if (purchaseCost < 0)
        {
            purchaseCost = 0;
        }

        if (dailyMaintenanceCost < 0)
        {
            dailyMaintenanceCost = 0;
        }

        // Aberrant tanks should support aberrants
        if (environmentType == EnvironmentType.Aberrant && !supportsAberrant)
        {
            Debug.LogWarning($"[{tankName}] Aberrant environment should support aberrant fish!");
            supportsAberrant = true;
        }

        // Validate upgrade counts
        if (maxCapacityUpgrades < 0)
        {
            maxCapacityUpgrades = 0;
        }
    }
#endif
}

/// <summary>
/// Tank size categories.
/// </summary>
[System.Serializable]
public enum TankSize
{
    Small,      // 5 fish capacity, $500
    Medium,     // 10 fish capacity, $2,000
    Large,      // 20 fish capacity, $5,000
    Massive     // 50 fish capacity, $15,000
}

/// <summary>
/// Environment types for tanks.
/// </summary>
[System.Serializable]
public enum EnvironmentType
{
    Freshwater, // Lakes, rivers
    Saltwater,  // Ocean, reef
    DeepSea,    // Deep ocean zones
    Aberrant,   // Cursed/supernatural fish
    Mixed       // Can hold any type (expensive)
}

/// <summary>
/// Visual themes for tank decoration.
/// </summary>
[System.Serializable]
public enum TankTheme
{
    Natural,        // Rocks, plants
    Tropical,       // Colorful coral
    DeepOcean,      // Dark, mysterious
    Haunted,        // Aberrant theme
    Minimalist,     // Clean, modern
    Reef,           // Coral reef
    Sunken,         // Shipwreck theme
    Laboratory      // Scientific/clinical
}

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Agent 20: Idle/AFK Progression System - PassiveIncomeSystem.cs
/// Manages all passive income sources including aquarium exhibitions,
/// crew assignments, breeding automation, and material gathering.
/// </summary>
public class PassiveIncomeSystem : MonoBehaviour
{
    [Header("Aquarium Income")]
    [Tooltip("Base income per fish in aquarium per day")]
    [SerializeField] private float baseAquariumIncomePerDay = 10f;

    [Tooltip("Income multipliers by fish rarity")]
    [SerializeField] private float commonMultiplier = 1f;
    [SerializeField] private float uncommonMultiplier = 5f;
    [SerializeField] private float rareMultiplier = 20f;
    [SerializeField] private float legendaryMultiplier = 50f;

    [Header("Crew Income")]
    [Tooltip("Base income per crew member per hour")]
    [SerializeField] private float baseCrewIncomePerHour = 10f;

    [Tooltip("Crew efficiency multiplier")]
    [SerializeField] private float crewEfficiencyMultiplier = 1.2f;

    [Header("Breeding Income")]
    [Tooltip("Base income from automated breeding per day")]
    [SerializeField] private float baseBreedingIncomePerDay = 200f;

    [Tooltip("Maximum breeding pairs that generate income")]
    [SerializeField] private int maxBreedingPairs = 5;

    [Header("Configuration")]
    [Tooltip("Enable debug logging")]
    [SerializeField] private bool enableDebugLogging = false;

    // Component references
    private IdleUpgradeSystem upgradeSystem;

    private void Awake()
    {
        upgradeSystem = GetComponent<IdleUpgradeSystem>();
        if (upgradeSystem == null)
        {
            Debug.LogWarning("[PassiveIncomeSystem] IdleUpgradeSystem not found!");
        }
    }

    /// <summary>
    /// Calculates total passive income for offline time period.
    /// </summary>
    /// <param name="hoursOffline">Hours player was offline</param>
    /// <returns>Total passive income earned</returns>
    public float CalculateOfflinePassiveIncome(float hoursOffline)
    {
        float totalIncome = 0f;

        // Aquarium income
        totalIncome += CalculateAquariumIncome(hoursOffline);

        // Crew income (if Crew Autonomy upgrade owned)
        if (upgradeSystem != null && upgradeSystem.HasUpgrade("crew_autonomy"))
        {
            totalIncome += CalculateCrewIncome(hoursOffline);
        }

        // Breeding income (if Breeding Automation upgrade owned)
        if (upgradeSystem != null && upgradeSystem.HasUpgrade("breeding_automation"))
        {
            totalIncome += CalculateBreedingIncome(hoursOffline);
        }

        if (enableDebugLogging)
        {
            Debug.Log($"[PassiveIncomeSystem] Total passive income for {hoursOffline:F2}h: ${totalIncome:F2}");
        }

        return totalIncome;
    }

    /// <summary>
    /// Calculates income from aquarium exhibitions.
    /// </summary>
    /// <param name="hoursOffline">Hours offline</param>
    /// <returns>Aquarium income</returns>
    private float CalculateAquariumIncome(float hoursOffline)
    {
        // TODO: Integrate with Aquarium system (Agent 16)
        // For now, simulate with placeholder values

        // Get number of fish in aquarium by rarity
        Dictionary<FishRarity, int> aquariumFish = GetAquariumFishCount();

        float totalIncome = 0f;
        float hoursPerDay = 24f;
        float days = hoursOffline / hoursPerDay;

        // Calculate income per rarity
        totalIncome += aquariumFish[FishRarity.Common] * baseAquariumIncomePerDay * commonMultiplier * days;
        totalIncome += aquariumFish[FishRarity.Uncommon] * baseAquariumIncomePerDay * uncommonMultiplier * days;
        totalIncome += aquariumFish[FishRarity.Rare] * baseAquariumIncomePerDay * rareMultiplier * days;
        totalIncome += aquariumFish[FishRarity.Legendary] * baseAquariumIncomePerDay * legendaryMultiplier * days;

        if (enableDebugLogging && totalIncome > 0f)
        {
            Debug.Log($"[PassiveIncomeSystem] Aquarium income: ${totalIncome:F2}");
        }

        return totalIncome;
    }

    /// <summary>
    /// Calculates income from crew working while offline.
    /// </summary>
    /// <param name="hoursOffline">Hours offline</param>
    /// <returns>Crew income</returns>
    private float CalculateCrewIncome(float hoursOffline)
    {
        // TODO: Integrate with Crew system (Agent 17)
        // For now, simulate with placeholder values

        int crewCount = GetActiveCrewCount();
        if (crewCount == 0) return 0f;

        float income = crewCount * baseCrewIncomePerHour * crewEfficiencyMultiplier * hoursOffline;

        if (enableDebugLogging && income > 0f)
        {
            Debug.Log($"[PassiveIncomeSystem] Crew income ({crewCount} crew): ${income:F2}");
        }

        return income;
    }

    /// <summary>
    /// Calculates income from automated fish breeding.
    /// </summary>
    /// <param name="hoursOffline">Hours offline</param>
    /// <returns>Breeding income</returns>
    private float CalculateBreedingIncome(float hoursOffline)
    {
        // TODO: Integrate with Breeding system (Agent 16)
        // For now, simulate with placeholder values

        int breedingPairs = GetActiveBreedingPairs();
        if (breedingPairs == 0) return 0f;

        // Cap breeding pairs
        breedingPairs = Mathf.Min(breedingPairs, maxBreedingPairs);

        float hoursPerDay = 24f;
        float days = hoursOffline / hoursPerDay;

        float income = breedingPairs * baseBreedingIncomePerDay * days;

        if (enableDebugLogging && income > 0f)
        {
            Debug.Log($"[PassiveIncomeSystem] Breeding income ({breedingPairs} pairs): ${income:F2}");
        }

        return income;
    }

    /// <summary>
    /// Gets the count of fish in aquarium by rarity.
    /// </summary>
    /// <returns>Dictionary of fish counts by rarity</returns>
    private Dictionary<FishRarity, int> GetAquariumFishCount()
    {
        // TODO: Query actual aquarium system
        // For now, return placeholder values

        return new Dictionary<FishRarity, int>
        {
            { FishRarity.Common, 0 },
            { FishRarity.Uncommon, 0 },
            { FishRarity.Rare, 0 },
            { FishRarity.Legendary, 0 }
        };
    }

    /// <summary>
    /// Gets the number of active crew members.
    /// </summary>
    /// <returns>Active crew count</returns>
    private int GetActiveCrewCount()
    {
        // TODO: Query actual crew system
        // For now, return placeholder value
        return 0;
    }

    /// <summary>
    /// Gets the number of active breeding pairs.
    /// </summary>
    /// <returns>Active breeding pairs count</returns>
    private int GetActiveBreedingPairs()
    {
        // TODO: Query actual breeding system
        // For now, return placeholder value
        return 0;
    }

    /// <summary>
    /// Estimates passive income for a given time period.
    /// </summary>
    /// <param name="hours">Number of hours</param>
    /// <returns>Estimated passive income</returns>
    public float EstimatePassiveIncome(float hours)
    {
        return CalculateOfflinePassiveIncome(hours);
    }

    /// <summary>
    /// Gets breakdown of passive income sources.
    /// </summary>
    /// <param name="hoursOffline">Hours offline</param>
    /// <returns>Breakdown of income by source</returns>
    public PassiveIncomeBreakdown GetIncomeBreakdown(float hoursOffline)
    {
        PassiveIncomeBreakdown breakdown = new PassiveIncomeBreakdown();

        breakdown.aquariumIncome = CalculateAquariumIncome(hoursOffline);
        breakdown.crewIncome = 0f;
        breakdown.breedingIncome = 0f;

        if (upgradeSystem != null && upgradeSystem.HasUpgrade("crew_autonomy"))
        {
            breakdown.crewIncome = CalculateCrewIncome(hoursOffline);
        }

        if (upgradeSystem != null && upgradeSystem.HasUpgrade("breeding_automation"))
        {
            breakdown.breedingIncome = CalculateBreedingIncome(hoursOffline);
        }

        breakdown.totalIncome = breakdown.aquariumIncome + breakdown.crewIncome + breakdown.breedingIncome;

        return breakdown;
    }

#if UNITY_EDITOR
    [ContextMenu("Test 1 Hour Passive Income")]
    private void TestOneHour()
    {
        float income = CalculateOfflinePassiveIncome(1f);
        Debug.Log($"[PassiveIncomeSystem] 1 Hour Passive Income: ${income:F2}");
        PrintBreakdown(1f);
    }

    [ContextMenu("Test 8 Hours Passive Income")]
    private void TestEightHours()
    {
        float income = CalculateOfflinePassiveIncome(8f);
        Debug.Log($"[PassiveIncomeSystem] 8 Hours Passive Income: ${income:F2}");
        PrintBreakdown(8f);
    }

    [ContextMenu("Test 24 Hours Passive Income")]
    private void TestTwentyFourHours()
    {
        float income = CalculateOfflinePassiveIncome(24f);
        Debug.Log($"[PassiveIncomeSystem] 24 Hours Passive Income: ${income:F2}");
        PrintBreakdown(24f);
    }

    private void PrintBreakdown(float hours)
    {
        PassiveIncomeBreakdown breakdown = GetIncomeBreakdown(hours);
        Debug.Log($"=== Passive Income Breakdown ({hours}h) ===");
        Debug.Log($"Aquarium: ${breakdown.aquariumIncome:F2}");
        Debug.Log($"Crew: ${breakdown.crewIncome:F2}");
        Debug.Log($"Breeding: ${breakdown.breedingIncome:F2}");
        Debug.Log($"Total: ${breakdown.totalIncome:F2}");
    }
#endif
}

/// <summary>
/// Breakdown of passive income by source.
/// </summary>
[System.Serializable]
public struct PassiveIncomeBreakdown
{
    public float aquariumIncome;
    public float crewIncome;
    public float breedingIncome;
    public float totalIncome;
}

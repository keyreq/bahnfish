using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Agent 20: Idle/AFK Progression System - AutoFishingSystem.cs
/// Simulates passive fishing while player is offline or AFK.
/// Calculates fish catches based on rod quality, location, and idle upgrades.
/// </summary>
public class AutoFishingSystem : MonoBehaviour
{
    [Header("Base Idle Fishing Rates")]
    [Tooltip("Base number of fish caught per hour (before modifiers)")]
    [SerializeField] private float baseFishPerHour = 6f;

    [Tooltip("Percentage of fish that auto-sell when inventory is full")]
    [SerializeField] private bool autoSellEnabled = false;

    [Header("Fish Rarity Weights (Idle Fishing)")]
    [Tooltip("Aberrant fish cannot be caught while idle (active play only)")]
    [SerializeField] private float commonWeight = 70f;
    [SerializeField] private float uncommonWeight = 20f;
    [SerializeField] private float rareWeight = 8f;
    [SerializeField] private float legendaryWeight = 2f;

    [Header("Configuration")]
    [Tooltip("Average fish value for auto-selling")]
    [SerializeField] private float averageFishValue = 50f;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogging = false;

    // Component references
    private IdleUpgradeSystem upgradeSystem;

    private void Awake()
    {
        upgradeSystem = GetComponent<IdleUpgradeSystem>();
        if (upgradeSystem == null)
        {
            Debug.LogWarning("[AutoFishingSystem] IdleUpgradeSystem not found!");
        }
    }

    private void Start()
    {
        // Subscribe to upgrade events
        EventSystem.Subscribe<string>("IdleUpgradePurchased", OnIdleUpgradePurchased);
    }

    /// <summary>
    /// Simulates offline fishing for the given time period.
    /// </summary>
    /// <param name="hoursOffline">Number of hours player was offline</param>
    /// <returns>Fishing results including fish caught and money earned</returns>
    public OfflineFishingResults SimulateOfflineFishing(float hoursOffline)
    {
        OfflineFishingResults results = new OfflineFishingResults();

        // Check if Auto-Fisher is owned
        if (upgradeSystem == null || !upgradeSystem.HasUpgrade("auto_fisher"))
        {
            if (enableDebugLogging)
            {
                Debug.Log("[AutoFishingSystem] Auto-Fisher not purchased. No idle fishing.");
            }
            return results;
        }

        // Calculate total fish catch rate
        float catchRate = CalculateIdleCatchRate();

        // Calculate total fish caught
        int totalFishCaught = Mathf.FloorToInt(catchRate * hoursOffline);

        // Generate fish by rarity
        Dictionary<FishRarity, int> fishByRarity = GenerateFishByRarity(totalFishCaught);

        // Calculate money earned (auto-sell if enabled)
        float moneyEarned = 0f;
        if (upgradeSystem.HasUpgrade("auto_sell"))
        {
            moneyEarned = CalculateFishValue(fishByRarity);
            autoSellEnabled = true;
        }

        // Store results
        results.fishCaught = totalFishCaught;
        results.fishByRarity = fishByRarity;
        results.moneyEarned = moneyEarned;
        results.autoSoldFish = autoSellEnabled;

        if (enableDebugLogging)
        {
            Debug.Log($"[AutoFishingSystem] Offline Fishing Results:");
            Debug.Log($"  Hours Offline: {hoursOffline:F2}");
            Debug.Log($"  Catch Rate: {catchRate:F2} fish/hour");
            Debug.Log($"  Total Fish Caught: {totalFishCaught}");
            Debug.Log($"  Money Earned: ${moneyEarned:F2}");
            Debug.Log($"  Common: {fishByRarity[FishRarity.Common]}, Uncommon: {fishByRarity[FishRarity.Uncommon]}, Rare: {fishByRarity[FishRarity.Rare]}, Legendary: {fishByRarity[FishRarity.Legendary]}");
        }

        return results;
    }

    /// <summary>
    /// Calculates the idle catch rate (fish per hour) based on upgrades.
    /// </summary>
    /// <returns>Fish per hour</returns>
    private float CalculateIdleCatchRate()
    {
        float catchRate = baseFishPerHour;

        if (upgradeSystem == null) return catchRate;

        // Apply rod quality multiplier (1.0 - 2.0)
        float rodQualityMultiplier = GetRodQualityMultiplier();
        catchRate *= rodQualityMultiplier;

        // Apply location efficiency (0.8 - 1.5)
        float locationEfficiency = GetLocationEfficiency();
        catchRate *= locationEfficiency;

        // Apply Quality Rod Holder bonus (+20% per level, max 5 levels)
        int rodHolderLevel = upgradeSystem.GetUpgradeLevel("quality_rod_holder");
        float rodHolderBonus = 1f + (rodHolderLevel * 0.2f);
        catchRate *= rodHolderBonus;

        // Apply Efficiency Boost bonus (+10% per level, max 10 levels)
        int efficiencyLevel = upgradeSystem.GetUpgradeLevel("efficiency_boost");
        float efficiencyBonus = 1f + (efficiencyLevel * 0.1f);
        catchRate *= efficiencyBonus;

        // Apply Time Compression (1.5x - 3x)
        float timeCompression = upgradeSystem.GetTimeCompressionMultiplier();
        catchRate *= timeCompression;

        return catchRate;
    }

    /// <summary>
    /// Gets the rod quality multiplier based on equipped rod.
    /// </summary>
    /// <returns>Multiplier between 1.0 and 2.0</returns>
    private float GetRodQualityMultiplier()
    {
        // TODO: Integrate with actual rod quality from inventory system
        // For now, return a default value
        return 1.2f;
    }

    /// <summary>
    /// Gets the location efficiency multiplier.
    /// </summary>
    /// <returns>Multiplier between 0.8 and 1.5</returns>
    private float GetLocationEfficiency()
    {
        // TODO: Integrate with location system
        // Different locations have different idle efficiency
        // Starter Lake: 1.0
        // Deep Ocean: 1.5
        // Cursed Waters: 0.8
        return 1.0f;
    }

    /// <summary>
    /// Generates fish distribution by rarity.
    /// </summary>
    /// <param name="totalFish">Total number of fish to generate</param>
    /// <returns>Dictionary mapping rarity to count</returns>
    private Dictionary<FishRarity, int> GenerateFishByRarity(int totalFish)
    {
        Dictionary<FishRarity, int> fishByRarity = new Dictionary<FishRarity, int>
        {
            { FishRarity.Common, 0 },
            { FishRarity.Uncommon, 0 },
            { FishRarity.Rare, 0 },
            { FishRarity.Legendary, 0 }
        };

        float totalWeight = commonWeight + uncommonWeight + rareWeight + legendaryWeight;

        for (int i = 0; i < totalFish; i++)
        {
            float roll = Random.value * totalWeight;

            if (roll < commonWeight)
            {
                fishByRarity[FishRarity.Common]++;
            }
            else if (roll < commonWeight + uncommonWeight)
            {
                fishByRarity[FishRarity.Uncommon]++;
            }
            else if (roll < commonWeight + uncommonWeight + rareWeight)
            {
                fishByRarity[FishRarity.Rare]++;
            }
            else
            {
                fishByRarity[FishRarity.Legendary]++;
            }
        }

        return fishByRarity;
    }

    /// <summary>
    /// Calculates the total value of fish caught.
    /// </summary>
    /// <param name="fishByRarity">Fish distribution by rarity</param>
    /// <returns>Total value in currency</returns>
    private float CalculateFishValue(Dictionary<FishRarity, int> fishByRarity)
    {
        float totalValue = 0f;

        // Base values per rarity (can be modified by events)
        float commonValue = 30f;
        float uncommonValue = 80f;
        float rareValue = 200f;
        float legendaryValue = 800f;

        totalValue += fishByRarity[FishRarity.Common] * commonValue;
        totalValue += fishByRarity[FishRarity.Uncommon] * uncommonValue;
        totalValue += fishByRarity[FishRarity.Rare] * rareValue;
        totalValue += fishByRarity[FishRarity.Legendary] * legendaryValue;

        return totalValue;
    }

    /// <summary>
    /// Called when an idle upgrade is purchased.
    /// </summary>
    private void OnIdleUpgradePurchased(string upgradeID)
    {
        if (upgradeID == "auto_sell")
        {
            autoSellEnabled = true;
            if (enableDebugLogging)
            {
                Debug.Log("[AutoFishingSystem] Auto-Sell enabled");
            }
        }
    }

    /// <summary>
    /// Gets the current idle catch rate.
    /// </summary>
    public float GetCurrentCatchRate()
    {
        return CalculateIdleCatchRate();
    }

    /// <summary>
    /// Estimates earnings for a given time period.
    /// </summary>
    /// <param name="hours">Number of hours</param>
    /// <returns>Estimated earnings</returns>
    public float EstimateEarnings(float hours)
    {
        if (!autoSellEnabled) return 0f;

        float catchRate = CalculateIdleCatchRate();
        int fishCount = Mathf.FloorToInt(catchRate * hours);
        Dictionary<FishRarity, int> fishByRarity = GenerateFishByRarity(fishCount);
        return CalculateFishValue(fishByRarity);
    }

    private void OnDestroy()
    {
        EventSystem.Unsubscribe<string>("IdleUpgradePurchased", OnIdleUpgradePurchased);
    }

#if UNITY_EDITOR
    [ContextMenu("Test 1 Hour Idle Fishing")]
    private void TestOneHourFishing()
    {
        var results = SimulateOfflineFishing(1f);
        Debug.Log($"[AutoFishingSystem Test] 1 Hour Results:");
        Debug.Log($"  Fish Caught: {results.fishCaught}");
        Debug.Log($"  Money Earned: ${results.moneyEarned:F2}");
    }

    [ContextMenu("Test 8 Hours Idle Fishing")]
    private void TestEightHoursFishing()
    {
        var results = SimulateOfflineFishing(8f);
        Debug.Log($"[AutoFishingSystem Test] 8 Hours Results:");
        Debug.Log($"  Fish Caught: {results.fishCaught}");
        Debug.Log($"  Money Earned: ${results.moneyEarned:F2}");
    }

    [ContextMenu("Test 24 Hours Idle Fishing")]
    private void TestTwentyFourHoursFishing()
    {
        var results = SimulateOfflineFishing(24f);
        Debug.Log($"[AutoFishingSystem Test] 24 Hours Results:");
        Debug.Log($"  Fish Caught: {results.fishCaught}");
        Debug.Log($"  Money Earned: ${results.moneyEarned:F2}");
    }

    [ContextMenu("Print Current Catch Rate")]
    private void PrintCatchRate()
    {
        float catchRate = CalculateIdleCatchRate();
        Debug.Log($"[AutoFishingSystem] Current catch rate: {catchRate:F2} fish/hour");
        Debug.Log($"  Estimated 1h earnings: ${EstimateEarnings(1f):F2}");
        Debug.Log($"  Estimated 8h earnings: ${EstimateEarnings(8f):F2}");
        Debug.Log($"  Estimated 24h earnings: ${EstimateEarnings(24f):F2}");
    }
#endif
}

/// <summary>
/// Results from offline fishing simulation.
/// </summary>
[System.Serializable]
public struct OfflineFishingResults
{
    public int fishCaught;
    public Dictionary<FishRarity, int> fishByRarity;
    public float moneyEarned;
    public bool autoSoldFish;
}

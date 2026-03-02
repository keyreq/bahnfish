using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Agent 9: Progression & Economy Agent - PricingSystem.cs
/// Handles dynamic pricing for fish, items, and upgrades.
/// Implements night premium, bulk bonuses, and market fluctuations.
/// </summary>
public class PricingSystem : MonoBehaviour
{
    public static PricingSystem Instance { get; private set; }

    [Header("Fish Pricing Multipliers")]
    [SerializeField] private float _nightPremiumMin = 3f;
    [SerializeField] private float _nightPremiumMax = 5f;
    [SerializeField] private float _aberrantBonus = 1.5f;
    [SerializeField] private float _freshBonusMultiplier = 1.2f;

    [Header("Bulk Sell Bonuses")]
    [SerializeField] private int _bulkThreshold1 = 10; // 10+ fish
    [SerializeField] private float _bulkBonus1 = 1.05f; // +5%
    [SerializeField] private int _bulkThreshold2 = 20; // 20+ fish
    [SerializeField] private float _bulkBonus2 = 1.10f; // +10%
    [SerializeField] private int _bulkThreshold3 = 30; // 30+ fish
    [SerializeField] private float _bulkBonus3 = 1.15f; // +15%

    [Header("Market Fluctuations")]
    [SerializeField] private bool _enableDynamicPricing = false;
    [SerializeField] private float _marketFluctuationRange = 0.15f; // ±15%
    [SerializeField] private float _rarityDemandMultiplier = 1.2f;

    [Header("Debug")]
    [SerializeField] private bool _enableDebugLogs = true;

    // Market demand tracking
    private Dictionary<string, int> _recentlySoldCount = new Dictionary<string, int>();
    private Dictionary<FishRarity, float> _rarityPriceModifiers = new Dictionary<FishRarity, float>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializePriceModifiers();
    }

    private void Start()
    {
        if (_enableDebugLogs)
        {
            Debug.Log($"[PricingSystem] Initialized. Night premium: {_nightPremiumMin}x-{_nightPremiumMax}x");
        }
    }

    /// <summary>
    /// Initializes rarity price modifiers.
    /// </summary>
    private void InitializePriceModifiers()
    {
        _rarityPriceModifiers[FishRarity.Common] = 1.0f;
        _rarityPriceModifiers[FishRarity.Uncommon] = 1.0f;
        _rarityPriceModifiers[FishRarity.Rare] = 1.0f;
        _rarityPriceModifiers[FishRarity.Legendary] = 1.0f;
        _rarityPriceModifiers[FishRarity.Aberrant] = 1.0f;
    }

    #region Fish Pricing

    /// <summary>
    /// Calculates the sell value of a fish with all applicable modifiers.
    /// </summary>
    /// <param name="fish">The fish to price</param>
    /// <param name="isFresh">Is the fish fresh (not spoiled)?</param>
    /// <param name="caughtAtNight">Was the fish caught at night?</param>
    /// <param name="qualityMultiplier">Additional quality multiplier (1.0 = normal)</param>
    /// <returns>Final sell price</returns>
    public float GetFishSellValue(Fish fish, bool isFresh = true, bool caughtAtNight = false, float qualityMultiplier = 1f)
    {
        if (fish == null)
        {
            Debug.LogWarning("[PricingSystem] Cannot price null fish");
            return 0f;
        }

        float baseValue = fish.baseValue;
        float finalValue = baseValue;

        // Apply night premium (3-5x for night catches)
        if (caughtAtNight)
        {
            float nightBonus = Random.Range(_nightPremiumMin, _nightPremiumMax);
            finalValue *= nightBonus;

            if (_enableDebugLogs)
            {
                Debug.Log($"[PricingSystem] Night premium applied: {nightBonus:F2}x (${baseValue:F2} → ${finalValue:F2})");
            }
        }

        // Apply aberrant bonus
        if (fish.isAberrant)
        {
            finalValue *= _aberrantBonus;

            if (_enableDebugLogs)
            {
                Debug.Log($"[PricingSystem] Aberrant bonus applied: {_aberrantBonus}x");
            }
        }

        // Apply freshness bonus
        if (isFresh)
        {
            finalValue *= _freshBonusMultiplier;
        }
        else
        {
            finalValue *= 0.5f; // Spoiled fish worth 50% less
        }

        // Apply quality multiplier
        finalValue *= qualityMultiplier;

        // Apply dynamic market pricing (if enabled)
        if (_enableDynamicPricing)
        {
            float marketModifier = GetMarketModifier(fish);
            finalValue *= marketModifier;
        }

        return Mathf.Round(finalValue * 100f) / 100f; // Round to 2 decimal places
    }

    /// <summary>
    /// Calculates the total value of multiple fish with bulk bonuses.
    /// </summary>
    /// <param name="fishList">List of fish to sell</param>
    /// <param name="caughtAtNight">Were these caught at night?</param>
    /// <returns>Total sell value including bulk bonuses</returns>
    public float CalculateBulkSellValue(List<Fish> fishList, bool caughtAtNight = false)
    {
        if (fishList == null || fishList.Count == 0)
        {
            return 0f;
        }

        float totalValue = 0f;

        // Calculate individual fish values
        foreach (var fish in fishList)
        {
            totalValue += GetFishSellValue(fish, true, caughtAtNight);
        }

        // Apply bulk bonuses
        float bulkBonus = GetBulkBonus(fishList.Count);
        float finalValue = totalValue * bulkBonus;

        if (_enableDebugLogs && bulkBonus > 1f)
        {
            Debug.Log($"[PricingSystem] Bulk bonus applied: {bulkBonus:F2}x for {fishList.Count} fish (${totalValue:F2} → ${finalValue:F2})");
        }

        return Mathf.Round(finalValue * 100f) / 100f;
    }

    /// <summary>
    /// Gets the bulk sell bonus multiplier based on quantity.
    /// </summary>
    private float GetBulkBonus(int fishCount)
    {
        if (fishCount >= _bulkThreshold3) return _bulkBonus3;
        if (fishCount >= _bulkThreshold2) return _bulkBonus2;
        if (fishCount >= _bulkThreshold1) return _bulkBonus1;
        return 1f; // No bonus
    }

    /// <summary>
    /// Gets the market demand modifier for a specific fish.
    /// </summary>
    private float GetMarketModifier(Fish fish)
    {
        // If we haven't sold many of this species recently, price is higher (demand)
        int recentSales = _recentlySoldCount.ContainsKey(fish.id) ? _recentlySoldCount[fish.id] : 0;

        // More sales = lower price (market saturation)
        float demandModifier = 1f - (recentSales * 0.02f); // -2% per recent sale
        demandModifier = Mathf.Clamp(demandModifier, 1f - _marketFluctuationRange, 1f + _marketFluctuationRange);

        // Apply rarity-based demand
        if (_rarityPriceModifiers.TryGetValue(fish.rarity, out float rarityMod))
        {
            demandModifier *= rarityMod;
        }

        return demandModifier;
    }

    /// <summary>
    /// Records a fish sale for market tracking.
    /// </summary>
    public void RecordFishSale(Fish fish, int quantity = 1)
    {
        if (fish == null) return;

        if (!_recentlySoldCount.ContainsKey(fish.id))
        {
            _recentlySoldCount[fish.id] = 0;
        }

        _recentlySoldCount[fish.id] += quantity;

        if (_enableDebugLogs)
        {
            Debug.Log($"[PricingSystem] Recorded sale of {quantity}x {fish.name}. Total recent sales: {_recentlySoldCount[fish.id]}");
        }
    }

    /// <summary>
    /// Resets market tracking (call daily or on location change).
    /// </summary>
    public void ResetMarketTracking()
    {
        _recentlySoldCount.Clear();
        InitializePriceModifiers();

        if (_enableDebugLogs)
        {
            Debug.Log("[PricingSystem] Market tracking reset");
        }
    }

    #endregion

    #region Upgrade Pricing

    /// <summary>
    /// Calculates the cost of an upgrade based on level.
    /// Uses exponential scaling: baseCost × (scaleFactor ^ currentLevel)
    /// </summary>
    /// <param name="baseCost">Starting cost at level 1</param>
    /// <param name="currentLevel">Current level (0-indexed)</param>
    /// <param name="scaleFactor">Cost multiplier per level (default: 2.5)</param>
    /// <returns>Cost for next upgrade</returns>
    public float CalculateUpgradeCost(float baseCost, int currentLevel, float scaleFactor = 2.5f)
    {
        float cost = baseCost * Mathf.Pow(scaleFactor, currentLevel);
        return Mathf.Round(cost);
    }

    /// <summary>
    /// Gets the fishing rod upgrade costs (predefined from design doc).
    /// </summary>
    public float GetFishingRodUpgradeCost(int level)
    {
        // From design: $100, $250, $500, $1000, $2000
        float[] costs = { 100f, 250f, 500f, 1000f, 2000f };
        if (level >= 0 && level < costs.Length)
        {
            return costs[level];
        }
        return 0f;
    }

    /// <summary>
    /// Gets the hull upgrade costs.
    /// </summary>
    public float GetHullUpgradeCost(int level)
    {
        // From design: $500, $1500, $3000
        float[] costs = { 500f, 1500f, 3000f };
        if (level >= 0 && level < costs.Length)
        {
            return costs[level];
        }
        return 0f;
    }

    /// <summary>
    /// Gets the engine upgrade costs.
    /// </summary>
    public float GetEngineUpgradeCost(int level)
    {
        // From design: $800, $2000, $5000
        float[] costs = { 800f, 2000f, 5000f };
        if (level >= 0 && level < costs.Length)
        {
            return costs[level];
        }
        return 0f;
    }

    /// <summary>
    /// Gets the lights upgrade costs.
    /// </summary>
    public float GetLightsUpgradeCost(int level)
    {
        // From design: $300, $700, $1500
        float[] costs = { 300f, 700f, 1500f };
        if (level >= 0 && level < costs.Length)
        {
            return costs[level];
        }
        return 0f;
    }

    /// <summary>
    /// Gets harpoon upgrade costs.
    /// </summary>
    public float GetHarpoonUpgradeCost(int level)
    {
        // From design: $200, $400, $800 (3 levels)
        float[] costs = { 200f, 400f, 800f };
        if (level >= 0 && level < costs.Length)
        {
            return costs[level];
        }
        return 0f;
    }

    /// <summary>
    /// Gets net/pot upgrade costs.
    /// </summary>
    public float GetNetUpgradeCost(int level)
    {
        // From design: $150, $350, $600, $1000 (4 levels)
        float[] costs = { 150f, 350f, 600f, 1000f };
        if (level >= 0 && level < costs.Length)
        {
            return costs[level];
        }
        return 0f;
    }

    #endregion

    #region Location Pricing

    /// <summary>
    /// Gets the cost to unlock a specific location by ID.
    /// </summary>
    public float GetLocationUnlockCost(string locationID)
    {
        // Predefined costs from design doc
        Dictionary<string, float> locationCosts = new Dictionary<string, float>
        {
            { "starter_lake", 0f },            // Free (starting location)
            { "rocky_coastline", 500f },
            { "tidal_pools", 1500f },
            { "deep_ocean", 1500f },
            { "fog_swamp", 2000f },
            { "mangrove_forest", 2000f },
            { "coral_reef", 2500f },
            { "arctic_waters", 3000f },
            { "shipwreck_graveyard", 3500f },
            { "underground_cavern", 4000f },
            { "bioluminescent_bay", 4500f },
            { "volcanic_vent", 5000f },
            { "abyssal_trench", 10000f }       // Endgame
        };

        if (locationCosts.TryGetValue(locationID, out float cost))
        {
            return cost;
        }

        Debug.LogWarning($"[PricingSystem] Unknown location ID: {locationID}");
        return 0f;
    }

    #endregion

    #region Debug Methods

    /// <summary>
    /// Prints pricing examples for testing.
    /// </summary>
    [ContextMenu("Print Pricing Examples")]
    public void PrintPricingExamples()
    {
        Debug.Log("=== Pricing Examples ===");

        // Create test fish
        Fish testFish = new Fish
        {
            id = "test_bass",
            name = "Test Bass",
            rarity = FishRarity.Common,
            baseValue = 20f,
            isAberrant = false
        };

        Debug.Log($"Base fish value: ${testFish.baseValue:F2}");
        Debug.Log($"Day catch: ${GetFishSellValue(testFish, true, false):F2}");
        Debug.Log($"Night catch: ${GetFishSellValue(testFish, true, true):F2}");
        Debug.Log($"Spoiled: ${GetFishSellValue(testFish, false, false):F2}");

        Debug.Log("\nUpgrade Costs:");
        Debug.Log($"Fishing Rod Level 1: ${GetFishingRodUpgradeCost(0):F2}");
        Debug.Log($"Hull Level 1: ${GetHullUpgradeCost(0):F2}");
        Debug.Log($"Engine Level 1: ${GetEngineUpgradeCost(0):F2}");

        Debug.Log("\nLocation Costs:");
        Debug.Log($"Rocky Coastline: ${GetLocationUnlockCost("rocky_coastline"):F2}");
        Debug.Log($"Abyssal Trench: ${GetLocationUnlockCost("abyssal_trench"):F2}");
        Debug.Log("========================");
    }

    #endregion
}

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Agent 20: Idle/AFK Progression System - IdleResourceGenerator.cs
/// Generates materials and resources while player is offline.
/// Produces fish scales, bones, oil, and scrap based on idle time and upgrades.
/// </summary>
public class IdleResourceGenerator : MonoBehaviour
{
    [Header("Base Generation Rates (per hour)")]
    [SerializeField] private int baseFishScalesPerHour = 5;
    [SerializeField] private int baseFishBonesPerHour = 3;
    [SerializeField] private int baseFishOilPerHour = 2;
    [SerializeField] private int baseScrapPerHour = 1;

    [Header("Maximum Generation Rates (per hour)")]
    [SerializeField] private int maxFishScalesPerHour = 20;
    [SerializeField] private int maxFishBonesPerHour = 15;
    [SerializeField] private int maxFishOilPerHour = 10;
    [SerializeField] private int maxScrapPerHour = 5;

    [Header("Configuration")]
    [Tooltip("Location affects resource generation rates")]
    [SerializeField] private bool useLocationModifiers = true;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogging = false;

    // Component references
    private IdleUpgradeSystem upgradeSystem;

    private void Awake()
    {
        upgradeSystem = GetComponent<IdleUpgradeSystem>();
        if (upgradeSystem == null)
        {
            Debug.LogWarning("[IdleResourceGenerator] IdleUpgradeSystem not found!");
        }
    }

    /// <summary>
    /// Generates offline resources based on time offline.
    /// </summary>
    /// <param name="hoursOffline">Hours player was offline</param>
    /// <returns>Dictionary of materials generated</returns>
    public Dictionary<string, int> GenerateOfflineResources(float hoursOffline)
    {
        Dictionary<string, int> resources = new Dictionary<string, int>();

        // Check if Auto-Fisher is owned (required for resource generation)
        if (upgradeSystem == null || !upgradeSystem.HasUpgrade("auto_fisher"))
        {
            return resources;
        }

        // Calculate generation rates
        float efficiencyMultiplier = GetEfficiencyMultiplier();
        float locationMultiplier = GetLocationMultiplier();
        float totalMultiplier = efficiencyMultiplier * locationMultiplier;

        // Generate fish scales
        int fishScales = CalculateResourceAmount(baseFishScalesPerHour, maxFishScalesPerHour, hoursOffline, totalMultiplier);
        if (fishScales > 0)
        {
            resources["fish_scales"] = fishScales;
        }

        // Generate fish bones
        int fishBones = CalculateResourceAmount(baseFishBonesPerHour, maxFishBonesPerHour, hoursOffline, totalMultiplier);
        if (fishBones > 0)
        {
            resources["fish_bones"] = fishBones;
        }

        // Generate fish oil
        int fishOil = CalculateResourceAmount(baseFishOilPerHour, maxFishOilPerHour, hoursOffline, totalMultiplier);
        if (fishOil > 0)
        {
            resources["fish_oil"] = fishOil;
        }

        // Generate scrap
        int scrap = CalculateResourceAmount(baseScrapPerHour, maxScrapPerHour, hoursOffline, totalMultiplier);
        if (scrap > 0)
        {
            resources["scrap"] = scrap;
        }

        if (enableDebugLogging)
        {
            Debug.Log($"[IdleResourceGenerator] Generated resources for {hoursOffline:F2}h:");
            Debug.Log($"  Fish Scales: {fishScales}");
            Debug.Log($"  Fish Bones: {fishBones}");
            Debug.Log($"  Fish Oil: {fishOil}");
            Debug.Log($"  Scrap: {scrap}");
            Debug.Log($"  Efficiency Multiplier: {efficiencyMultiplier:F2}x");
            Debug.Log($"  Location Multiplier: {locationMultiplier:F2}x");
        }

        return resources;
    }

    /// <summary>
    /// Calculates the amount of a resource generated.
    /// </summary>
    /// <param name="baseRate">Base generation rate per hour</param>
    /// <param name="maxRate">Maximum generation rate per hour</param>
    /// <param name="hours">Hours offline</param>
    /// <param name="multiplier">Total multiplier</param>
    /// <returns>Amount generated</returns>
    private int CalculateResourceAmount(int baseRate, int maxRate, float hours, float multiplier)
    {
        // Calculate rate with multiplier
        float ratePerHour = Mathf.Min(baseRate * multiplier, maxRate);

        // Add randomness (±20%)
        float randomFactor = Random.Range(0.8f, 1.2f);
        ratePerHour *= randomFactor;

        // Calculate total amount
        int amount = Mathf.FloorToInt(ratePerHour * hours);

        return amount;
    }

    /// <summary>
    /// Gets the efficiency multiplier based on upgrades.
    /// </summary>
    /// <returns>Efficiency multiplier</returns>
    private float GetEfficiencyMultiplier()
    {
        if (upgradeSystem == null) return 1f;

        float multiplier = 1f;

        // Efficiency Boost upgrade (+10% per level)
        int efficiencyLevel = upgradeSystem.GetUpgradeLevel("efficiency_boost");
        multiplier += efficiencyLevel * 0.1f;

        // Quality Rod Holder upgrade (+5% resource generation per level)
        int rodHolderLevel = upgradeSystem.GetUpgradeLevel("quality_rod_holder");
        multiplier += rodHolderLevel * 0.05f;

        return multiplier;
    }

    /// <summary>
    /// Gets the location multiplier for resource generation.
    /// </summary>
    /// <returns>Location multiplier</returns>
    private float GetLocationMultiplier()
    {
        if (!useLocationModifiers) return 1f;

        // TODO: Integrate with location system (Agent 14)
        // Different locations yield different resources:
        // - Starter Lake: 1.0x (balanced)
        // - Deep Ocean: 1.3x fish scales, 0.8x scrap
        // - Swamp: 1.5x fish oil, 0.7x fish scales
        // - Industrial River: 1.8x scrap, 0.6x fish oil
        // - Cursed Waters: 1.2x all resources (high risk, high reward)

        return 1.0f; // Default for now
    }

    /// <summary>
    /// Estimates resource generation for a given time period.
    /// </summary>
    /// <param name="hours">Number of hours</param>
    /// <returns>Estimated resources</returns>
    public Dictionary<string, int> EstimateResourceGeneration(float hours)
    {
        return GenerateOfflineResources(hours);
    }

    /// <summary>
    /// Gets the current generation rates (per hour).
    /// </summary>
    /// <returns>Dictionary of resource generation rates</returns>
    public Dictionary<string, float> GetCurrentGenerationRates()
    {
        Dictionary<string, float> rates = new Dictionary<string, float>();

        float efficiencyMultiplier = GetEfficiencyMultiplier();
        float locationMultiplier = GetLocationMultiplier();
        float totalMultiplier = efficiencyMultiplier * locationMultiplier;

        rates["fish_scales"] = Mathf.Min(baseFishScalesPerHour * totalMultiplier, maxFishScalesPerHour);
        rates["fish_bones"] = Mathf.Min(baseFishBonesPerHour * totalMultiplier, maxFishBonesPerHour);
        rates["fish_oil"] = Mathf.Min(baseFishOilPerHour * totalMultiplier, maxFishOilPerHour);
        rates["scrap"] = Mathf.Min(baseScrapPerHour * totalMultiplier, maxScrapPerHour);

        return rates;
    }

    /// <summary>
    /// Gets the maximum possible generation rates (per hour).
    /// </summary>
    /// <returns>Dictionary of maximum rates</returns>
    public Dictionary<string, int> GetMaximumGenerationRates()
    {
        return new Dictionary<string, int>
        {
            { "fish_scales", maxFishScalesPerHour },
            { "fish_bones", maxFishBonesPerHour },
            { "fish_oil", maxFishOilPerHour },
            { "scrap", maxScrapPerHour }
        };
    }

#if UNITY_EDITOR
    [ContextMenu("Test 1 Hour Generation")]
    private void TestOneHour()
    {
        var resources = GenerateOfflineResources(1f);
        Debug.Log("[IdleResourceGenerator] 1 Hour Generation:");
        PrintResources(resources);
    }

    [ContextMenu("Test 8 Hours Generation")]
    private void TestEightHours()
    {
        var resources = GenerateOfflineResources(8f);
        Debug.Log("[IdleResourceGenerator] 8 Hours Generation:");
        PrintResources(resources);
    }

    [ContextMenu("Test 24 Hours Generation")]
    private void TestTwentyFourHours()
    {
        var resources = GenerateOfflineResources(24f);
        Debug.Log("[IdleResourceGenerator] 24 Hours Generation:");
        PrintResources(resources);
    }

    [ContextMenu("Print Current Generation Rates")]
    private void PrintCurrentRates()
    {
        var rates = GetCurrentGenerationRates();
        Debug.Log("=== Current Resource Generation Rates (per hour) ===");
        foreach (var rate in rates)
        {
            Debug.Log($"{FormatResourceName(rate.Key)}: {rate.Value:F1}");
        }

        Debug.Log("\n=== Maximum Rates ===");
        var maxRates = GetMaximumGenerationRates();
        foreach (var rate in maxRates)
        {
            Debug.Log($"{FormatResourceName(rate.Key)}: {rate.Value}");
        }

        Debug.Log($"\nEfficiency Multiplier: {GetEfficiencyMultiplier():F2}x");
        Debug.Log($"Location Multiplier: {GetLocationMultiplier():F2}x");
    }

    [ContextMenu("Simulate Full Day (24h) with Breakdown")]
    private void SimulateFullDay()
    {
        Debug.Log("=== 24-Hour Resource Generation Simulation ===");
        Debug.Log("Checking generation every 4 hours:");

        for (int hour = 4; hour <= 24; hour += 4)
        {
            var resources = EstimateResourceGeneration(hour);
            Debug.Log($"\n{hour} hours:");
            PrintResources(resources);
        }
    }

    private void PrintResources(Dictionary<string, int> resources)
    {
        foreach (var resource in resources)
        {
            Debug.Log($"  {FormatResourceName(resource.Key)}: {resource.Value}");
        }
    }

    private string FormatResourceName(string resourceID)
    {
        // Convert snake_case to Title Case
        string formatted = resourceID.Replace("_", " ");
        System.Text.StringBuilder result = new System.Text.StringBuilder();
        bool capitalizeNext = true;

        foreach (char c in formatted)
        {
            if (c == ' ')
            {
                capitalizeNext = true;
                result.Append(c);
            }
            else if (capitalizeNext)
            {
                result.Append(char.ToUpper(c));
                capitalizeNext = false;
            }
            else
            {
                result.Append(c);
            }
        }

        return result.ToString();
    }
#endif
}

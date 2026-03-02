using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// Agent 16: Aquarium & Breeding Specialist - FishCareSystem.cs
/// Manages fish health, happiness, feeding mechanics, and disease prevention.
/// Integrates with AquariumManager for daily care routines.
/// </summary>
public class FishCareSystem : MonoBehaviour
{
    private static FishCareSystem _instance;
    public static FishCareSystem Instance => _instance;

    [Header("Care Configuration")]
    [SerializeField] private float feedingCostPerFish = 1f;
    [SerializeField] private float treatmentCost = 25f;
    [SerializeField] private float diseaseChance = 0.02f; // 2% daily chance if unhealthy

    [Header("Happiness Modifiers")]
    [SerializeField] private float cleanWaterHappinessBonus = 0.1f;
    [SerializeField] private float overPopulationPenalty = 0.15f;
    [SerializeField] private float compatibleTankBonus = 0.05f;

    [Header("Auto-Care")]
    [SerializeField] private bool autoFeedingEnabled = false;
    [SerializeField] private bool autoTreatmentEnabled = false;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;

    // Diseased fish tracking
    private HashSet<string> diseasedFish = new HashSet<string>();

    // Care statistics
    private int totalFeedings = 0;
    private int totalTreatments = 0;
    private float totalCareCost = 0f;

    // Events
    public event Action<DisplayFish> OnFishBecameSick;
    public event Action<DisplayFish> OnFishTreated;
    public event Action<string> OnTankCleaned;

    #region Unity Lifecycle

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
        // Subscribe to events
        EventSystem.Subscribe<TimeChangedEventData>("TimeChanged", OnTimeChanged);
        EventSystem.Subscribe<FishDeathData>("FishDied", OnFishDied);

        // Subscribe to save/load
        EventSystem.Subscribe<SaveData>("GatheringSaveData", OnGatheringSaveData);
        EventSystem.Subscribe<SaveData>("ApplyingSaveData", OnApplyingSaveData);

        if (enableDebugLogs)
        {
            Debug.Log("[FishCareSystem] Initialized");
        }
    }

    private void OnDestroy()
    {
        EventSystem.Unsubscribe<TimeChangedEventData>("TimeChanged", OnTimeChanged);
        EventSystem.Unsubscribe<FishDeathData>("FishDied", OnFishDied);
        EventSystem.Unsubscribe<SaveData>("GatheringSaveData", OnGatheringSaveData);
        EventSystem.Unsubscribe<SaveData>("ApplyingSaveData", OnApplyingSaveData);
    }

    #endregion

    #region Feeding

    /// <summary>
    /// Feeds a specific fish.
    /// </summary>
    public bool FeedFish(DisplayFish fish)
    {
        if (fish == null || !fish.isAlive)
        {
            return false;
        }

        // Check cost
        if (EconomySystem.Instance != null && !EconomySystem.Instance.CanAffordMoney(feedingCostPerFish))
        {
            if (enableDebugLogs)
            {
                Debug.Log("[FishCareSystem] Cannot afford fish food");
            }
            return false;
        }

        // Deduct cost
        if (EconomySystem.Instance != null)
        {
            EconomySystem.Instance.SpendMoney(feedingCostPerFish, "Fish Food");
        }

        // Feed the fish
        fish.Feed();
        totalFeedings++;
        totalCareCost += feedingCostPerFish;

        EventSystem.Publish("FishFed", fish);

        if (enableDebugLogs)
        {
            Debug.Log($"[FishCareSystem] Fed {fish.speciesName}");
        }

        return true;
    }

    /// <summary>
    /// Feeds all fish in a tank.
    /// </summary>
    public bool FeedTank(string tankID)
    {
        if (AquariumManager.Instance == null)
        {
            return false;
        }

        List<DisplayFish> fishList = AquariumManager.Instance.GetTankFish(tankID);

        if (fishList.Count == 0)
        {
            return false;
        }

        float totalCost = fishList.Count * feedingCostPerFish;

        // Check affordability
        if (EconomySystem.Instance != null && !EconomySystem.Instance.CanAffordMoney(totalCost))
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[FishCareSystem] Cannot afford to feed tank (${totalCost})");
            }
            return false;
        }

        // Deduct cost
        if (EconomySystem.Instance != null)
        {
            EconomySystem.Instance.SpendMoney(totalCost, $"Feed Tank {tankID}");
        }

        // Feed all fish
        foreach (var fish in fishList)
        {
            fish.Feed();
            totalFeedings++;
        }

        totalCareCost += totalCost;

        EventSystem.Publish("TankFed", tankID);

        if (enableDebugLogs)
        {
            Debug.Log($"[FishCareSystem] Fed {fishList.Count} fish in tank {tankID}");
        }

        return true;
    }

    /// <summary>
    /// Feeds all fish in all tanks.
    /// </summary>
    public bool FeedAllTanks()
    {
        if (AquariumManager.Instance == null)
        {
            return false;
        }

        List<DisplayFish> allFish = AquariumManager.Instance.GetAllDisplayFish();
        int aliveFish = allFish.Count(f => f.isAlive);

        if (aliveFish == 0)
        {
            return false;
        }

        float totalCost = aliveFish * feedingCostPerFish;

        // Check affordability
        if (EconomySystem.Instance != null && !EconomySystem.Instance.CanAffordMoney(totalCost))
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[FishCareSystem] Cannot afford to feed all fish (${totalCost})");
            }
            return false;
        }

        // Deduct cost
        if (EconomySystem.Instance != null)
        {
            EconomySystem.Instance.SpendMoney(totalCost, "Feed All Fish");
        }

        // Feed all fish
        foreach (var fish in allFish)
        {
            if (fish.isAlive)
            {
                fish.Feed();
                totalFeedings++;
            }
        }

        totalCareCost += totalCost;

        EventSystem.Publish("AllTanksFed", aliveFish);

        if (enableDebugLogs)
        {
            Debug.Log($"[FishCareSystem] Fed {aliveFish} fish across all tanks");
        }

        return true;
    }

    /// <summary>
    /// Enables or disables auto-feeding.
    /// </summary>
    public void SetAutoFeeding(bool enabled)
    {
        autoFeedingEnabled = enabled;

        if (enableDebugLogs)
        {
            Debug.Log($"[FishCareSystem] Auto-feeding {(enabled ? "enabled" : "disabled")}");
        }
    }

    #endregion

    #region Health & Disease

    /// <summary>
    /// Checks fish for disease and applies status.
    /// </summary>
    public void CheckForDisease(DisplayFish fish)
    {
        if (fish == null || !fish.isAlive || diseasedFish.Contains(fish.uniqueID))
        {
            return;
        }

        // Disease chance increases with poor health and happiness
        float baseDiseaseChance = diseaseChance;

        if (fish.health < 0.5f)
        {
            baseDiseaseChance *= 2f;
        }

        if (fish.happiness < 0.3f)
        {
            baseDiseaseChance *= 1.5f;
        }

        // Roll for disease
        if (UnityEngine.Random.value < baseDiseaseChance)
        {
            fish.health -= 0.2f; // Disease damages health
            diseasedFish.Add(fish.uniqueID);

            OnFishBecameSick?.Invoke(fish);
            EventSystem.Publish("FishBecameSick", fish);

            if (enableDebugLogs)
            {
                Debug.Log($"[FishCareSystem] {fish.speciesName} became sick!");
            }
        }
    }

    /// <summary>
    /// Treats a sick fish.
    /// </summary>
    public bool TreatFish(DisplayFish fish)
    {
        if (fish == null || !diseasedFish.Contains(fish.uniqueID))
        {
            return false;
        }

        // Check affordability
        if (EconomySystem.Instance != null && !EconomySystem.Instance.CanAffordMoney(treatmentCost))
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[FishCareSystem] Cannot afford treatment (${treatmentCost})");
            }
            return false;
        }

        // Deduct cost
        if (EconomySystem.Instance != null)
        {
            EconomySystem.Instance.SpendMoney(treatmentCost, "Fish Treatment");
        }

        // Cure the fish
        diseasedFish.Remove(fish.uniqueID);
        fish.health = Mathf.Min(fish.health + 0.3f, 1f); // Restore 30% health
        totalTreatments++;
        totalCareCost += treatmentCost;

        OnFishTreated?.Invoke(fish);
        EventSystem.Publish("FishTreated", fish);

        if (enableDebugLogs)
        {
            Debug.Log($"[FishCareSystem] Treated {fish.speciesName}");
        }

        return true;
    }

    /// <summary>
    /// Checks if a fish is diseased.
    /// </summary>
    public bool IsDiseased(DisplayFish fish)
    {
        return fish != null && diseasedFish.Contains(fish.uniqueID);
    }

    /// <summary>
    /// Gets count of diseased fish.
    /// </summary>
    public int GetDiseasedFishCount()
    {
        return diseasedFish.Count;
    }

    /// <summary>
    /// Enables or disables auto-treatment.
    /// </summary>
    public void SetAutoTreatment(bool enabled)
    {
        autoTreatmentEnabled = enabled;

        if (enableDebugLogs)
        {
            Debug.Log($"[FishCareSystem] Auto-treatment {(enabled ? "enabled" : "disabled")}");
        }
    }

    #endregion

    #region Tank Maintenance

    /// <summary>
    /// Cleans a tank, improving environment quality.
    /// </summary>
    public bool CleanTank(string tankID)
    {
        if (AquariumManager.Instance == null)
        {
            return false;
        }

        TankInstance tank = AquariumManager.Instance.GetTank(tankID);

        if (tank == null)
        {
            return false;
        }

        float cleaningCost = 20f; // Base cleaning cost

        // Check affordability
        if (EconomySystem.Instance != null && !EconomySystem.Instance.CanAffordMoney(cleaningCost))
        {
            return false;
        }

        // Deduct cost
        if (EconomySystem.Instance != null)
        {
            EconomySystem.Instance.SpendMoney(cleaningCost, $"Clean Tank {tankID}");
        }

        // Improve happiness of all fish in tank
        List<DisplayFish> fishList = AquariumManager.Instance.GetTankFish(tankID);

        foreach (var fish in fishList)
        {
            fish.happiness = Mathf.Min(fish.happiness + cleanWaterHappinessBonus, 1f);
        }

        totalCareCost += cleaningCost;

        OnTankCleaned?.Invoke(tankID);
        EventSystem.Publish("TankCleaned", tankID);

        if (enableDebugLogs)
        {
            Debug.Log($"[FishCareSystem] Cleaned tank {tankID}");
        }

        return true;
    }

    /// <summary>
    /// Calculates happiness modifiers for a tank.
    /// </summary>
    public float CalculateTankHappinessModifier(string tankID)
    {
        if (AquariumManager.Instance == null)
        {
            return 0f;
        }

        TankInstance tank = AquariumManager.Instance.GetTank(tankID);

        if (tank == null)
        {
            return 0f;
        }

        float modifier = 0f;

        // Environment quality bonus
        float environmentQuality = tank.GetEnvironmentQuality();
        modifier += (environmentQuality - 0.7f) * cleanWaterHappinessBonus;

        // Population check
        int fishCount = tank.GetFishCount();
        int maxCapacity = tank.GetMaxCapacity();

        if (fishCount > maxCapacity * 0.8f) // Over 80% capacity
        {
            modifier -= overPopulationPenalty;
        }

        return modifier;
    }

    #endregion

    #region Daily Care Routine

    /// <summary>
    /// Performs daily care routines (called by AquariumManager).
    /// </summary>
    public void PerformDailyCare()
    {
        if (AquariumManager.Instance == null)
        {
            return;
        }

        List<DisplayFish> allFish = AquariumManager.Instance.GetAllDisplayFish();

        // Auto-feeding
        if (autoFeedingEnabled)
        {
            FeedAllTanks();
        }

        // Check for disease
        foreach (var fish in allFish)
        {
            if (fish.isAlive)
            {
                CheckForDisease(fish);
            }
        }

        // Auto-treatment
        if (autoTreatmentEnabled)
        {
            foreach (var fish in allFish)
            {
                if (IsDiseased(fish))
                {
                    TreatFish(fish);
                }
            }
        }

        if (enableDebugLogs)
        {
            Debug.Log("[FishCareSystem] Completed daily care routine");
        }
    }

    #endregion

    #region Statistics

    /// <summary>
    /// Gets care statistics.
    /// </summary>
    public CareStatistics GetStatistics()
    {
        return new CareStatistics
        {
            totalFeedings = totalFeedings,
            totalTreatments = totalTreatments,
            totalCareCost = totalCareCost,
            diseasedFishCount = diseasedFish.Count,
            autoFeedingEnabled = autoFeedingEnabled,
            autoTreatmentEnabled = autoTreatmentEnabled
        };
    }

    /// <summary>
    /// Resets care statistics.
    /// </summary>
    public void ResetStatistics()
    {
        totalFeedings = 0;
        totalTreatments = 0;
        totalCareCost = 0f;
    }

    #endregion

    #region Event Handlers

    private void OnTimeChanged(TimeChangedEventData timeData)
    {
        // Daily care is handled by AquariumManager
    }

    private void OnFishDied(FishDeathData data)
    {
        // Remove from diseased list
        diseasedFish.Remove(data.fish.uniqueID);
    }

    #endregion

    #region Save/Load Integration

    private void OnGatheringSaveData(SaveData data)
    {
        // Save care statistics and settings
        // In full implementation, would save to custom data structure
        if (enableDebugLogs)
        {
            Debug.Log($"[FishCareSystem] Saved care data: {totalFeedings} feedings, {totalTreatments} treatments");
        }
    }

    private void OnApplyingSaveData(SaveData data)
    {
        // Load care settings
        if (enableDebugLogs)
        {
            Debug.Log("[FishCareSystem] Loaded care settings");
        }
    }

    #endregion

    #region Debug Methods

    [ContextMenu("Print Care Statistics")]
    private void DebugPrintStats()
    {
        CareStatistics stats = GetStatistics();

        Debug.Log($"=== Fish Care Statistics ===\n" +
                 $"Total Feedings: {stats.totalFeedings}\n" +
                 $"Total Treatments: {stats.totalTreatments}\n" +
                 $"Total Care Cost: ${stats.totalCareCost:F2}\n" +
                 $"Diseased Fish: {stats.diseasedFishCount}\n" +
                 $"Auto-Feeding: {(stats.autoFeedingEnabled ? "ON" : "OFF")}\n" +
                 $"Auto-Treatment: {(stats.autoTreatmentEnabled ? "ON" : "OFF")}\n" +
                 $"============================");
    }

    [ContextMenu("Perform Daily Care")]
    private void DebugDailyCare()
    {
        PerformDailyCare();
    }

    #endregion
}

/// <summary>
/// Care statistics structure.
/// </summary>
[Serializable]
public struct CareStatistics
{
    public int totalFeedings;
    public int totalTreatments;
    public float totalCareCost;
    public int diseasedFishCount;
    public bool autoFeedingEnabled;
    public bool autoTreatmentEnabled;
}

using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Agent 16: Aquarium & Breeding Specialist - BreedingSystem.cs
/// Manages fish breeding mechanics, timing, and offspring generation.
/// Integrates with GeneticsSystem for trait inheritance.
/// </summary>
public class BreedingSystem : MonoBehaviour
{
    private static BreedingSystem _instance;
    public static BreedingSystem Instance => _instance;

    [Header("Breeding Configuration")]
    [SerializeField] private float baseBreedingSuccessChance = 0.6f; // 60% base success
    [SerializeField] private float minHappinessRequired = 0.6f; // 60% happiness minimum
    [SerializeField] private float baseIncubationHours = 24f; // Real-time hours
    [SerializeField] private float breedingCooldownHours = 24f; // Real-time hours

    [Header("Costs")]
    [SerializeField] private float baseBreedingCost = 50f;
    [SerializeField] private float rareBreedingCost = 100f;
    [SerializeField] private float legendaryBreedingCost = 200f;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;
    [SerializeField] private bool fastBreeding = false; // For testing: 1 minute instead of hours

    // Active breeding pairs
    private List<BreedingPair> activeBreedingPairs = new List<BreedingPair>();

    // Events
    public event Action<BreedingPair> OnBreedingStarted;
    public event Action<BreedingPair, DisplayFish> OnOffspringBorn;
    public event Action<BreedingPair> OnBreedingFailed;

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
        // Subscribe to time updates for breeding timers
        EventSystem.Subscribe<TimeChangedEventData>("TimeChanged", OnTimeChanged);

        // Subscribe to save/load events
        EventSystem.Subscribe<SaveData>("GatheringSaveData", OnGatheringSaveData);
        EventSystem.Subscribe<SaveData>("ApplyingSaveData", OnApplyingSaveData);

        if (enableDebugLogs)
        {
            Debug.Log("[BreedingSystem] Initialized");
        }
    }

    private void OnDestroy()
    {
        EventSystem.Unsubscribe<TimeChangedEventData>("TimeChanged", OnTimeChanged);
        EventSystem.Unsubscribe<SaveData>("GatheringSaveData", OnGatheringSaveData);
        EventSystem.Unsubscribe<SaveData>("ApplyingSaveData", OnApplyingSaveData);
    }

    private void Update()
    {
        // Check breeding timers
        UpdateBreedingTimers();
    }

    #endregion

    /// <summary>
    /// Attempts to start breeding between two fish.
    /// </summary>
    public bool StartBreeding(DisplayFish parent1, DisplayFish parent2, string tankID, int geneticsLabLevel = 0)
    {
        // Validate breeding attempt
        string validationError = ValidateBreeding(parent1, parent2);
        if (!string.IsNullOrEmpty(validationError))
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[BreedingSystem] Breeding validation failed: {validationError}");
            }

            EventSystem.Publish("BreedingValidationFailed", validationError);
            return false;
        }

        // Calculate breeding cost
        float cost = CalculateBreedingCost(parent1, parent2);

        // Check if player can afford
        if (EconomySystem.Instance != null && !EconomySystem.Instance.CanAffordMoney(cost))
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[BreedingSystem] Cannot afford breeding cost: ${cost}");
            }

            EventSystem.Publish("InsufficientFunds", new InsufficientFundsData("money", cost, EconomySystem.Instance.Money, "Fish Breeding"));
            return false;
        }

        // Deduct cost
        if (EconomySystem.Instance != null)
        {
            EconomySystem.Instance.SpendMoney(cost, $"Breeding: {parent1.speciesName} x {parent2.speciesName}");
        }

        // Roll for breeding success
        float successChance = CalculateBreedingSuccess(parent1, parent2);
        bool success = UnityEngine.Random.value < successChance;

        if (!success)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[BreedingSystem] Breeding attempt failed (chance was {successChance:P0})");
            }

            BreedingPair failedPair = new BreedingPair
            {
                parent1 = parent1,
                parent2 = parent2,
                tankID = tankID
            };

            OnBreedingFailed?.Invoke(failedPair);
            EventSystem.Publish("BreedingFailed", failedPair);

            // Set cooldown even on failure
            parent1.lastBreedingTime = DateTime.Now;
            parent2.lastBreedingTime = DateTime.Now;

            return false;
        }

        // Create breeding pair
        BreedingPair pair = new BreedingPair
        {
            pairID = Guid.NewGuid().ToString(),
            parent1 = parent1,
            parent2 = parent2,
            tankID = tankID,
            geneticsLabLevel = geneticsLabLevel,
            startTime = DateTime.Now,
            estimatedCompletionTime = DateTime.Now.AddHours(fastBreeding ? 0.017 : baseIncubationHours), // 1 min or 24 hours
            isComplete = false
        };

        activeBreedingPairs.Add(pair);

        // Update parent breeding times
        parent1.lastBreedingTime = DateTime.Now;
        parent2.lastBreedingTime = DateTime.Now;

        // Fire events
        OnBreedingStarted?.Invoke(pair);
        EventSystem.Publish("BreedingStarted", pair);

        if (enableDebugLogs)
        {
            Debug.Log($"[BreedingSystem] Breeding started: {parent1.speciesName} x {parent2.speciesName}. " +
                     $"Expected completion: {pair.estimatedCompletionTime:g}");
        }

        return true;
    }

    /// <summary>
    /// Validates if two fish can breed together.
    /// </summary>
    public string ValidateBreeding(DisplayFish parent1, DisplayFish parent2)
    {
        if (parent1 == null || parent2 == null)
        {
            return "One or both fish are null";
        }

        // Must be same species
        if (parent1.speciesID != parent2.speciesID)
        {
            return "Fish must be the same species to breed";
        }

        // Must be mature
        if (!parent1.IsMature() || !parent2.IsMature())
        {
            return "Both fish must be mature (3-7 days old)";
        }

        // Must be happy enough
        if (parent1.happiness < minHappinessRequired || parent2.happiness < minHappinessRequired)
        {
            return $"Both fish must be at least {minHappinessRequired:P0} happy";
        }

        // Check breeding cooldown
        if (IsOnCooldown(parent1) || IsOnCooldown(parent2))
        {
            return $"One or both fish are on breeding cooldown ({breedingCooldownHours}h)";
        }

        // Check if already breeding
        if (IsCurrentlyBreeding(parent1.uniqueID) || IsCurrentlyBreeding(parent2.uniqueID))
        {
            return "One or both fish are already breeding";
        }

        return null; // Validation passed
    }

    /// <summary>
    /// Checks if a fish is currently in a breeding pair.
    /// </summary>
    public bool IsCurrentlyBreeding(string fishID)
    {
        foreach (var pair in activeBreedingPairs)
        {
            if (!pair.isComplete && (pair.parent1.uniqueID == fishID || pair.parent2.uniqueID == fishID))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Checks if a fish is on breeding cooldown.
    /// </summary>
    public bool IsOnCooldown(DisplayFish fish)
    {
        if (fish.lastBreedingTime == DateTime.MinValue)
        {
            return false;
        }

        TimeSpan timeSince = DateTime.Now - fish.lastBreedingTime;
        return timeSince.TotalHours < breedingCooldownHours;
    }

    /// <summary>
    /// Gets remaining cooldown time for a fish.
    /// </summary>
    public TimeSpan GetRemainingCooldown(DisplayFish fish)
    {
        if (!IsOnCooldown(fish))
        {
            return TimeSpan.Zero;
        }

        TimeSpan elapsed = DateTime.Now - fish.lastBreedingTime;
        TimeSpan total = TimeSpan.FromHours(breedingCooldownHours);
        return total - elapsed;
    }

    /// <summary>
    /// Calculates breeding success chance based on compatibility and happiness.
    /// </summary>
    private float CalculateBreedingSuccess(DisplayFish parent1, DisplayFish parent2)
    {
        float successChance = baseBreedingSuccessChance;

        // Genetic compatibility bonus
        float compatibility = GeneticsSystem.CalculateCompatibility(parent1.traits, parent2.traits);
        successChance += compatibility * 0.2f; // Up to +20% for perfect compatibility

        // Happiness bonus
        float avgHappiness = (parent1.happiness + parent2.happiness) / 2f;
        successChance += (avgHappiness - minHappinessRequired) * 0.3f;

        // Rarity penalty (harder to breed rare fish)
        if (parent1.rarity >= FishRarity.Rare || parent2.rarity >= FishRarity.Rare)
        {
            successChance -= 0.1f;
        }

        if (parent1.rarity == FishRarity.Legendary || parent2.rarity == FishRarity.Legendary)
        {
            successChance -= 0.2f;
        }

        return Mathf.Clamp01(successChance);
    }

    /// <summary>
    /// Calculates breeding cost based on fish rarity.
    /// </summary>
    private float CalculateBreedingCost(DisplayFish parent1, DisplayFish parent2)
    {
        FishRarity highestRarity = (FishRarity)Mathf.Max((int)parent1.rarity, (int)parent2.rarity);

        switch (highestRarity)
        {
            case FishRarity.Common:
            case FishRarity.Uncommon:
                return baseBreedingCost;
            case FishRarity.Rare:
            case FishRarity.Aberrant:
                return rareBreedingCost;
            case FishRarity.Legendary:
                return legendaryBreedingCost;
            default:
                return baseBreedingCost;
        }
    }

    /// <summary>
    /// Updates all active breeding timers.
    /// </summary>
    private void UpdateBreedingTimers()
    {
        for (int i = activeBreedingPairs.Count - 1; i >= 0; i--)
        {
            BreedingPair pair = activeBreedingPairs[i];

            if (pair.isComplete)
            {
                continue;
            }

            // Check if breeding is complete
            if (DateTime.Now >= pair.estimatedCompletionTime)
            {
                CompleteBreeding(pair);
            }
        }
    }

    /// <summary>
    /// Completes a breeding pair and generates offspring.
    /// </summary>
    private void CompleteBreeding(BreedingPair pair)
    {
        // Generate offspring genetics
        GeneticTraits offspringTraits = GeneticsSystem.GenerateOffspring(
            pair.parent1.traits,
            pair.parent2.traits,
            pair.geneticsLabLevel
        );

        // Create offspring fish
        DisplayFish offspring = new DisplayFish
        {
            uniqueID = offspringTraits.uniqueID,
            speciesID = pair.parent1.speciesID,
            speciesName = pair.parent1.speciesName,
            rarity = DetermineOffspringRarity(pair.parent1.rarity, pair.parent2.rarity, offspringTraits),
            traits = offspringTraits,
            age = 0f,
            happiness = 0.8f, // Start happy
            tankID = pair.tankID,
            birthDate = DateTime.Now,
            isBred = true
        };

        // Mark pair as complete
        pair.isComplete = true;
        pair.offspring = offspring;

        // Fire events
        OnOffspringBorn?.Invoke(pair, offspring);
        EventSystem.Publish("OffspringBorn", new OffspringBornData(pair, offspring));

        // Check for mutation discovery
        if (offspringTraits.mutationChance > 0.02f || offspringTraits.hasBioluminescence ||
            offspringTraits.colorVariant == FishColor.Rainbow)
        {
            EventSystem.Publish("MutationDiscovered", offspring);
        }

        if (enableDebugLogs)
        {
            Debug.Log($"[BreedingSystem] Offspring born! {offspring.speciesName} (Gen {offspring.traits.generation})");
        }

        // Clean up completed pair after notification
        activeBreedingPairs.Remove(pair);
    }

    /// <summary>
    /// Determines offspring rarity based on parents and traits.
    /// </summary>
    private FishRarity DetermineOffspringRarity(FishRarity parent1Rarity, FishRarity parent2Rarity, GeneticTraits traits)
    {
        FishRarity baseRarity = (FishRarity)Mathf.Max((int)parent1Rarity, (int)parent2Rarity);

        // Aberrant overrides
        if (traits.isAberrant)
        {
            return FishRarity.Aberrant;
        }

        // Chance to upgrade rarity
        float upgradeChance = traits.rarityBonus;
        if (UnityEngine.Random.value < upgradeChance && baseRarity < FishRarity.Legendary)
        {
            baseRarity = (FishRarity)((int)baseRarity + 1);

            if (enableDebugLogs)
            {
                Debug.Log($"[BreedingSystem] Rarity upgraded to {baseRarity}!");
            }
        }

        return baseRarity;
    }

    /// <summary>
    /// Gets all active breeding pairs.
    /// </summary>
    public List<BreedingPair> GetActiveBreedingPairs()
    {
        return new List<BreedingPair>(activeBreedingPairs);
    }

    /// <summary>
    /// Cancels a breeding pair (does not refund cost).
    /// </summary>
    public void CancelBreeding(string pairID)
    {
        for (int i = 0; i < activeBreedingPairs.Count; i++)
        {
            if (activeBreedingPairs[i].pairID == pairID)
            {
                activeBreedingPairs.RemoveAt(i);

                if (enableDebugLogs)
                {
                    Debug.Log($"[BreedingSystem] Breeding pair {pairID} cancelled");
                }

                return;
            }
        }
    }

    #region Time & Save/Load Integration

    private void OnTimeChanged(TimeChangedEventData timeData)
    {
        // Breeding uses real-time, not game time
        // This is here for potential future use
    }

    private void OnGatheringSaveData(SaveData data)
    {
        // Save active breeding pairs
        List<SerializedBreedingPair> serializedPairs = new List<SerializedBreedingPair>();

        foreach (var pair in activeBreedingPairs)
        {
            serializedPairs.Add(new SerializedBreedingPair(pair));
        }

        data.breedingPairs = new Dictionary<string, int>();
        // Store count for compatibility (full data would be in system-specific storage)
        data.breedingPairs["activeCount"] = activeBreedingPairs.Count;
    }

    private void OnApplyingSaveData(SaveData data)
    {
        // Load breeding pairs
        // In full implementation, this would restore active breeding timers
        if (enableDebugLogs)
        {
            Debug.Log($"[BreedingSystem] Loaded save data");
        }
    }

    #endregion

    #region Debug Methods

    [ContextMenu("Force Complete All Breeding")]
    private void DebugForceCompleteAll()
    {
        foreach (var pair in activeBreedingPairs.ToArray())
        {
            if (!pair.isComplete)
            {
                CompleteBreeding(pair);
            }
        }
    }

    #endregion
}

/// <summary>
/// Represents an active breeding pair.
/// </summary>
[Serializable]
public class BreedingPair
{
    public string pairID;
    public DisplayFish parent1;
    public DisplayFish parent2;
    public string tankID;
    public int geneticsLabLevel;
    public DateTime startTime;
    public DateTime estimatedCompletionTime;
    public bool isComplete;
    public DisplayFish offspring;

    public float GetProgressPercentage()
    {
        if (isComplete)
        {
            return 1f;
        }

        TimeSpan total = estimatedCompletionTime - startTime;
        TimeSpan elapsed = DateTime.Now - startTime;

        return Mathf.Clamp01((float)(elapsed.TotalSeconds / total.TotalSeconds));
    }

    public TimeSpan GetRemainingTime()
    {
        if (isComplete)
        {
            return TimeSpan.Zero;
        }

        TimeSpan remaining = estimatedCompletionTime - DateTime.Now;
        return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
    }
}

/// <summary>
/// Serializable version of breeding pair for save/load.
/// </summary>
[Serializable]
public class SerializedBreedingPair
{
    public string pairID;
    public string parent1ID;
    public string parent2ID;
    public string tankID;
    public int geneticsLabLevel;
    public string startTime;
    public string estimatedCompletionTime;
    public bool isComplete;

    public SerializedBreedingPair(BreedingPair pair)
    {
        pairID = pair.pairID;
        parent1ID = pair.parent1.uniqueID;
        parent2ID = pair.parent2.uniqueID;
        tankID = pair.tankID;
        geneticsLabLevel = pair.geneticsLabLevel;
        startTime = pair.startTime.ToString("o");
        estimatedCompletionTime = pair.estimatedCompletionTime.ToString("o");
        isComplete = pair.isComplete;
    }
}

/// <summary>
/// Event data for offspring birth.
/// </summary>
[Serializable]
public struct OffspringBornData
{
    public BreedingPair pair;
    public DisplayFish offspring;

    public OffspringBornData(BreedingPair pair, DisplayFish offspring)
    {
        this.pair = pair;
        this.offspring = offspring;
    }
}

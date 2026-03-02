using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Agent 17: Crew & Companion Specialist - LoyaltySystem.cs
/// Manages pet loyalty progression, decay, and interaction tracking.
/// THE CORE PETTING MECHANIC from Cast n Chill inspiration!
/// </summary>
public class LoyaltySystem : MonoBehaviour
{
    private static LoyaltySystem _instance;
    public static LoyaltySystem Instance => _instance;

    [Header("Loyalty Tracking")]
    [SerializeField] private Dictionary<string, PetLoyaltyData> petLoyaltyData = new Dictionary<string, PetLoyaltyData>();

    [Header("Interaction Cooldowns")]
    [SerializeField] private Dictionary<string, float> pettingCooldowns = new Dictionary<string, float>();
    [SerializeField] private Dictionary<string, float> feedingTimestamps = new Dictionary<string, float>();
    [SerializeField] private Dictionary<string, float> playingCooldowns = new Dictionary<string, float>();

    [Header("Settings")]
    [SerializeField] private float dailyDecayCheckInterval = 3600f; // 1 hour in real-time
    [SerializeField] private bool enableDebugLogging = true;

    [Header("Status")]
    [SerializeField] private float lastDecayCheckTime = 0f;

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
        Initialize();
    }

    private void Initialize()
    {
        lastDecayCheckTime = Time.time;

        // Subscribe to save/load events
        EventSystem.Subscribe<SaveData>("GatheringSaveData", OnGatheringSaveData);
        EventSystem.Subscribe<SaveData>("ApplyingSaveData", OnApplyingSaveData);

        if (enableDebugLogging)
        {
            Debug.Log("[LoyaltySystem] Initialized");
        }
    }

    private void Update()
    {
        // Check for loyalty decay periodically
        if (Time.time >= lastDecayCheckTime + dailyDecayCheckInterval)
        {
            ProcessLoyaltyDecay();
            lastDecayCheckTime = Time.time;
        }
    }

    /// <summary>
    /// Registers a new pet in the loyalty system
    /// </summary>
    /// <param name="petData">Pet data to register</param>
    public void RegisterPet(PetData petData)
    {
        if (petData == null || petLoyaltyData.ContainsKey(petData.petID))
        {
            return;
        }

        PetLoyaltyData loyaltyData = new PetLoyaltyData
        {
            petID = petData.petID,
            currentLoyalty = petData.startingLoyalty,
            totalPettings = 0,
            totalFeedings = 0,
            totalPlaySessions = 0,
            lastPettingTime = Time.time,
            lastFeedingTime = Time.time,
            lastPlayTime = Time.time,
            acquisitionTime = Time.time
        };

        petLoyaltyData[petData.petID] = loyaltyData;

        if (enableDebugLogging)
        {
            Debug.Log($"[LoyaltySystem] Registered pet: {petData.petName} (ID: {petData.petID})");
        }

        EventSystem.Publish("PetRegistered", petData.petID);
    }

    /// <summary>
    /// Handles petting interaction - THE KEY FEATURE!
    /// </summary>
    /// <param name="petID">ID of pet being petted</param>
    /// <param name="petData">Pet data reference</param>
    /// <returns>True if petting was successful</returns>
    public bool PetCompanion(string petID, PetData petData)
    {
        if (!petLoyaltyData.ContainsKey(petID) || petData == null)
        {
            return false;
        }

        // Check cooldown
        if (pettingCooldowns.ContainsKey(petID))
        {
            float timeSinceLastPet = Time.time - pettingCooldowns[petID];
            if (timeSinceLastPet < petData.pettingCooldown)
            {
                float remainingCooldown = petData.pettingCooldown - timeSinceLastPet;
                EventSystem.Publish("ShowNotification", $"Wait {remainingCooldown:F0}s before petting again");
                return false;
            }
        }

        // Perform petting
        PetLoyaltyData loyaltyData = petLoyaltyData[petID];
        float loyaltyGain = petData.loyaltyPerPet;

        // Affection modifier
        loyaltyGain *= (petData.affectionLevel / 5f);

        // Apply loyalty gain
        loyaltyData.currentLoyalty = Mathf.Clamp(loyaltyData.currentLoyalty + loyaltyGain, 0f, 100f);
        loyaltyData.totalPettings++;
        loyaltyData.lastPettingTime = Time.time;

        pettingCooldowns[petID] = Time.time;
        petLoyaltyData[petID] = loyaltyData;

        // Sanity boost if low
        GameState state = GameManager.Instance.CurrentGameState;
        if (state.sanity < 50f)
        {
            state.sanity = Mathf.Min(state.sanity + petData.sanityBoost, 100f);
            GameManager.Instance.UpdateGameState(state);
        }

        // Publish events
        EventSystem.Publish("PetPetted", petID);
        EventSystem.Publish("LoyaltyChanged", new LoyaltyChangedEventData
        {
            petID = petID,
            oldLoyalty = loyaltyData.currentLoyalty - loyaltyGain,
            newLoyalty = loyaltyData.currentLoyalty,
            changeAmount = loyaltyGain
        });

        if (enableDebugLogging)
        {
            Debug.Log($"[LoyaltySystem] Pet {petID} petted! Loyalty: {loyaltyData.currentLoyalty:F1}% (+{loyaltyGain:F1})");
        }

        return true;
    }

    /// <summary>
    /// Handles feeding interaction
    /// </summary>
    /// <param name="petID">ID of pet being fed</param>
    /// <param name="petData">Pet data reference</param>
    /// <returns>True if feeding was successful</returns>
    public bool FeedPet(string petID, PetData petData)
    {
        if (!petLoyaltyData.ContainsKey(petID) || petData == null)
        {
            return false;
        }

        // Check if pet needs feeding
        float timeSinceLastFeed = Time.time - feedingTimestamps.GetValueOrDefault(petID, 0f);
        float feedingIntervalSeconds = petData.feedingIntervalHours * 3600f;

        if (timeSinceLastFeed < feedingIntervalSeconds)
        {
            EventSystem.Publish("ShowNotification", $"{petData.petName} is not hungry yet");
            return false;
        }

        // Perform feeding
        PetLoyaltyData loyaltyData = petLoyaltyData[petID];
        float loyaltyGain = petData.loyaltyPerFeed;

        loyaltyData.currentLoyalty = Mathf.Clamp(loyaltyData.currentLoyalty + loyaltyGain, 0f, 100f);
        loyaltyData.totalFeedings++;
        loyaltyData.lastFeedingTime = Time.time;

        feedingTimestamps[petID] = Time.time;
        petLoyaltyData[petID] = loyaltyData;

        EventSystem.Publish("PetFed", petID);
        EventSystem.Publish("LoyaltyChanged", new LoyaltyChangedEventData
        {
            petID = petID,
            oldLoyalty = loyaltyData.currentLoyalty - loyaltyGain,
            newLoyalty = loyaltyData.currentLoyalty,
            changeAmount = loyaltyGain
        });

        if (enableDebugLogging)
        {
            Debug.Log($"[LoyaltySystem] Pet {petID} fed! Loyalty: {loyaltyData.currentLoyalty:F1}% (+{loyaltyGain:F1})");
        }

        return true;
    }

    /// <summary>
    /// Handles play interaction
    /// </summary>
    /// <param name="petID">ID of pet playing</param>
    /// <param name="petData">Pet data reference</param>
    /// <returns>True if play was successful</returns>
    public bool PlayWithPet(string petID, PetData petData)
    {
        if (!petLoyaltyData.ContainsKey(petID) || petData == null)
        {
            return false;
        }

        // Check cooldown (2 minutes)
        if (playingCooldowns.ContainsKey(petID))
        {
            float timeSinceLastPlay = Time.time - playingCooldowns[petID];
            if (timeSinceLastPlay < 120f)
            {
                float remainingCooldown = 120f - timeSinceLastPlay;
                EventSystem.Publish("ShowNotification", $"Wait {remainingCooldown:F0}s before playing again");
                return false;
            }
        }

        // Perform play
        PetLoyaltyData loyaltyData = petLoyaltyData[petID];
        float loyaltyGain = petData.loyaltyPerPlay;

        // Energy modifier
        loyaltyGain *= (petData.energyLevel / 5f);

        loyaltyData.currentLoyalty = Mathf.Clamp(loyaltyData.currentLoyalty + loyaltyGain, 0f, 100f);
        loyaltyData.totalPlaySessions++;
        loyaltyData.lastPlayTime = Time.time;

        playingCooldowns[petID] = Time.time;
        petLoyaltyData[petID] = loyaltyData;

        EventSystem.Publish("PetPlayed", petID);
        EventSystem.Publish("LoyaltyChanged", new LoyaltyChangedEventData
        {
            petID = petID,
            oldLoyalty = loyaltyData.currentLoyalty - loyaltyGain,
            newLoyalty = loyaltyData.currentLoyalty,
            changeAmount = loyaltyGain
        });

        if (enableDebugLogging)
        {
            Debug.Log($"[LoyaltySystem] Played with pet {petID}! Loyalty: {loyaltyData.currentLoyalty:F1}% (+{loyaltyGain:F1})");
        }

        return true;
    }

    /// <summary>
    /// Processes daily loyalty decay for all pets
    /// </summary>
    private void ProcessLoyaltyDecay()
    {
        List<string> petsToRemove = new List<string>();

        foreach (var kvp in petLoyaltyData)
        {
            string petID = kvp.Key;
            PetLoyaltyData loyaltyData = kvp.Value;

            // Get pet data (would need reference to CompanionManager)
            // For now, apply generic decay
            float timeSinceLastPet = Time.time - loyaltyData.lastPettingTime;
            float daysSinceLastPet = timeSinceLastPet / 86400f; // Convert to days

            if (daysSinceLastPet >= 1f)
            {
                float decayAmount = 1f * Mathf.Floor(daysSinceLastPet);
                loyaltyData.currentLoyalty = Mathf.Max(loyaltyData.currentLoyalty - decayAmount, 0f);

                // Check if pet runs away
                if (loyaltyData.currentLoyalty < 20f)
                {
                    EventSystem.Publish("PetLowLoyalty", petID);

                    if (loyaltyData.currentLoyalty <= 0f)
                    {
                        EventSystem.Publish("PetRunAway", petID);
                        petsToRemove.Add(petID);
                        continue;
                    }
                }

                petLoyaltyData[petID] = loyaltyData;

                if (enableDebugLogging)
                {
                    Debug.Log($"[LoyaltySystem] Pet {petID} loyalty decayed: {loyaltyData.currentLoyalty:F1}% (-{decayAmount:F1})");
                }
            }
        }

        // Remove pets that ran away
        foreach (string petID in petsToRemove)
        {
            petLoyaltyData.Remove(petID);
        }
    }

    /// <summary>
    /// Gets current loyalty for a pet
    /// </summary>
    /// <param name="petID">Pet ID</param>
    /// <returns>Loyalty percentage (0-100)</returns>
    public float GetLoyalty(string petID)
    {
        if (petLoyaltyData.ContainsKey(petID))
        {
            return petLoyaltyData[petID].currentLoyalty;
        }
        return 0f;
    }

    /// <summary>
    /// Gets full loyalty data for a pet
    /// </summary>
    public PetLoyaltyData GetLoyaltyData(string petID)
    {
        return petLoyaltyData.GetValueOrDefault(petID, null);
    }

    /// <summary>
    /// Checks if pet can be petted (not on cooldown)
    /// </summary>
    public bool CanPetNow(string petID, PetData petData)
    {
        if (!pettingCooldowns.ContainsKey(petID))
        {
            return true;
        }

        float timeSinceLastPet = Time.time - pettingCooldowns[petID];
        return timeSinceLastPet >= petData.pettingCooldown;
    }

    /// <summary>
    /// Checks if pet needs feeding
    /// </summary>
    public bool NeedsFeeding(string petID, PetData petData)
    {
        if (!feedingTimestamps.ContainsKey(petID))
        {
            return true;
        }

        float timeSinceLastFeed = Time.time - feedingTimestamps[petID];
        float feedingIntervalSeconds = petData.feedingIntervalHours * 3600f;
        return timeSinceLastFeed >= feedingIntervalSeconds;
    }

    #region Save/Load Integration

    private void OnGatheringSaveData(SaveData data)
    {
        LoyaltySystemSaveData saveData = new LoyaltySystemSaveData
        {
            petLoyaltyDataList = new List<PetLoyaltyData>(petLoyaltyData.Values),
            lastDecayCheckTime = this.lastDecayCheckTime
        };

        // Convert cooldown dictionaries to lists
        saveData.pettingCooldownsList = new List<CooldownEntry>();
        foreach (var kvp in pettingCooldowns)
        {
            saveData.pettingCooldownsList.Add(new CooldownEntry { key = kvp.Key, timestamp = kvp.Value });
        }

        saveData.feedingTimestampsList = new List<CooldownEntry>();
        foreach (var kvp in feedingTimestamps)
        {
            saveData.feedingTimestampsList.Add(new CooldownEntry { key = kvp.Key, timestamp = kvp.Value });
        }

        data.loyaltySystemData = JsonUtility.ToJson(saveData);
    }

    private void OnApplyingSaveData(SaveData data)
    {
        if (string.IsNullOrEmpty(data.loyaltySystemData)) return;

        LoyaltySystemSaveData saveData = JsonUtility.FromJson<LoyaltySystemSaveData>(data.loyaltySystemData);

        // Restore loyalty data
        petLoyaltyData.Clear();
        foreach (PetLoyaltyData loyaltyData in saveData.petLoyaltyDataList)
        {
            petLoyaltyData[loyaltyData.petID] = loyaltyData;
        }

        // Restore cooldowns
        pettingCooldowns.Clear();
        foreach (CooldownEntry entry in saveData.pettingCooldownsList)
        {
            pettingCooldowns[entry.key] = entry.timestamp;
        }

        feedingTimestamps.Clear();
        foreach (CooldownEntry entry in saveData.feedingTimestampsList)
        {
            feedingTimestamps[entry.key] = entry.timestamp;
        }

        lastDecayCheckTime = saveData.lastDecayCheckTime;

        if (enableDebugLogging)
        {
            Debug.Log($"[LoyaltySystem] Loaded loyalty data for {petLoyaltyData.Count} pets");
        }
    }

    #endregion

    private void OnDestroy()
    {
        EventSystem.Unsubscribe<SaveData>("GatheringSaveData", OnGatheringSaveData);
        EventSystem.Unsubscribe<SaveData>("ApplyingSaveData", OnApplyingSaveData);

        if (_instance == this)
        {
            _instance = null;
        }
    }
}

/// <summary>
/// Pet loyalty data structure
/// </summary>
[Serializable]
public class PetLoyaltyData
{
    public string petID;
    public float currentLoyalty;
    public int totalPettings;
    public int totalFeedings;
    public int totalPlaySessions;
    public float lastPettingTime;
    public float lastFeedingTime;
    public float lastPlayTime;
    public float acquisitionTime;
}

/// <summary>
/// Loyalty changed event data
/// </summary>
[Serializable]
public struct LoyaltyChangedEventData
{
    public string petID;
    public float oldLoyalty;
    public float newLoyalty;
    public float changeAmount;
}

/// <summary>
/// Save data for loyalty system
/// </summary>
[Serializable]
public class LoyaltySystemSaveData
{
    public List<PetLoyaltyData> petLoyaltyDataList;
    public List<CooldownEntry> pettingCooldownsList;
    public List<CooldownEntry> feedingTimestampsList;
    public float lastDecayCheckTime;
}

/// <summary>
/// Helper for saving dictionary entries
/// </summary>
[Serializable]
public class CooldownEntry
{
    public string key;
    public float timestamp;
}

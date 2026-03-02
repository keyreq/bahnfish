using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages fish preservation mechanics and quality degradation.
/// Provides 4 preservation methods: Ice Box, Salting, Smoking, and Freezing.
/// Tracks decay timers for all fish in inventory and applies quality penalties.
/// </summary>
public class PreservationSystem : MonoBehaviour
{
    public static PreservationSystem Instance { get; private set; }

    [Header("Preservation Settings")]
    [SerializeField] private PreservationConfig config = new PreservationConfig();

    [Header("Decay Tracking")]
    [SerializeField] private Dictionary<string, FishDecayData> decayTimers = new Dictionary<string, FishDecayData>();

    [Header("Available Preservation Methods")]
    [SerializeField] private bool iceBoxUnlocked = true;
    [SerializeField] private bool saltingUnlocked = false;
    [SerializeField] private bool smokingUnlocked = false;
    [SerializeField] private bool freezingUnlocked = false;

    [Header("Capacity")]
    [SerializeField] private int iceBoxCapacity = 4;
    [SerializeField] private int smokehouseCapacity = 6;
    [SerializeField] private int freezerCapacity = 10;

    [Header("Settings")]
    [Tooltip("Show debug logs")]
    [SerializeField] private bool debugMode = false;

    // Tracked preserved fish
    private List<string> iceBoxFish = new List<string>();
    private List<string> saltedFish = new List<string>();
    private List<string> smokedFish = new List<string>();
    private List<string> frozenFish = new List<string>();

    // Events
    public event System.Action<string, PreservationMethod> OnFishPreserved;
    public event System.Action<string> OnFishDecayed;
    public event System.Action<string, float> OnQualityDegraded;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Subscribe to events
        EventSystem.Subscribe<SaveData>("GatheringSaveData", OnGatheringSaveData);
        EventSystem.Subscribe<SaveData>("ApplyingSaveData", OnApplyingSaveData);
        EventSystem.Subscribe<InventoryItem>("ItemAddedToInventory", OnFishAdded);
        EventSystem.Subscribe<InventoryItem>("ItemRemovedFromInventory", OnFishRemoved);
    }

    private void Update()
    {
        UpdateDecayTimers();
    }

    private void OnDestroy()
    {
        EventSystem.Unsubscribe<SaveData>("GatheringSaveData", OnGatheringSaveData);
        EventSystem.Unsubscribe<SaveData>("ApplyingSaveData", OnApplyingSaveData);
        EventSystem.Unsubscribe<InventoryItem>("ItemAddedToInventory", OnFishAdded);
        EventSystem.Unsubscribe<InventoryItem>("ItemRemovedFromInventory", OnFishRemoved);
    }

    /// <summary>
    /// Updates decay timers for all tracked fish.
    /// </summary>
    private void UpdateDecayTimers()
    {
        List<string> toRemove = new List<string>();

        foreach (var kvp in decayTimers.ToList())
        {
            string fishID = kvp.Key;
            FishDecayData decay = kvp.Value;

            // Skip if frozen (infinite preservation)
            if (decay.preservationMethod == PreservationMethod.Freezing)
                continue;

            // Update timer
            decay.elapsedTime += Time.deltaTime;

            // Check for quality degradation thresholds
            float decayPercent = decay.elapsedTime / decay.maxPreservationTime;

            if (decayPercent >= 0.5f && !decay.halfwayWarned)
            {
                decay.halfwayWarned = true;
                EventSystem.Publish("FishHalfwayDecayed", fishID);

                if (debugMode)
                    Debug.Log($"[PreservationSystem] Fish {fishID} is halfway decayed");
            }

            if (decayPercent >= 0.75f && !decay.almostDecayedWarned)
            {
                decay.almostDecayedWarned = true;
                EventSystem.Publish("FishAlmostDecayed", fishID);

                if (debugMode)
                    Debug.Log($"[PreservationSystem] Fish {fishID} is almost decayed");
            }

            // Check if fully decayed
            if (decay.elapsedTime >= decay.maxPreservationTime)
            {
                HandleFishDecay(fishID);
                toRemove.Add(fishID);
            }
        }

        // Remove decayed fish
        foreach (var fishID in toRemove)
        {
            decayTimers.Remove(fishID);
        }
    }

    /// <summary>
    /// Handles when a fish fully decays.
    /// </summary>
    private void HandleFishDecay(string fishID)
    {
        // Remove from inventory
        if (InventoryManager.Instance != null)
        {
            var items = InventoryManager.Instance.GetAllItems();
            var fishItem = items.FirstOrDefault(i => i.ItemID == fishID);

            if (fishItem != null)
            {
                InventoryManager.Instance.RemoveItem(fishItem);
            }
        }

        OnFishDecayed?.Invoke(fishID);
        EventSystem.Publish("FishDecayed", fishID);

        if (debugMode)
            Debug.Log($"[PreservationSystem] Fish {fishID} has fully decayed and been removed");
    }

    /// <summary>
    /// Applies a preservation method to a fish.
    /// </summary>
    public bool PreserveFish(string fishID, PreservationMethod method)
    {
        if (string.IsNullOrEmpty(fishID))
            return false;

        // Check if method is unlocked
        if (!IsMethodUnlocked(method))
        {
            Debug.LogWarning($"[PreservationSystem] {method} is not unlocked");
            return false;
        }

        // Check capacity
        if (!HasCapacity(method))
        {
            Debug.LogWarning($"[PreservationSystem] {method} is at full capacity");
            return false;
        }

        // Get or create decay data
        if (!decayTimers.ContainsKey(fishID))
        {
            decayTimers[fishID] = new FishDecayData
            {
                fishID = fishID,
                startTime = Time.time,
                preservationMethod = PreservationMethod.None,
                baseDecayTime = config.baseDecayTime
            };
        }

        FishDecayData decay = decayTimers[fishID];

        // Remove from old preservation list
        RemoveFromPreservationList(fishID, decay.preservationMethod);

        // Apply new preservation method
        decay.preservationMethod = method;
        decay.maxPreservationTime = GetPreservationTime(method);
        decay.elapsedTime = 0f; // Reset timer
        decay.halfwayWarned = false;
        decay.almostDecayedWarned = false;

        // Add to new preservation list
        AddToPreservationList(fishID, method);

        OnFishPreserved?.Invoke(fishID, method);
        EventSystem.Publish("FishPreserved", new FishPreservationData
        {
            fishID = fishID,
            method = method,
            preservationTime = decay.maxPreservationTime
        });

        if (debugMode)
            Debug.Log($"[PreservationSystem] Applied {method} to fish {fishID} (preservation time: {decay.maxPreservationTime}s)");

        return true;
    }

    /// <summary>
    /// Gets the preservation time for a method in seconds.
    /// </summary>
    private float GetPreservationTime(PreservationMethod method)
    {
        switch (method)
        {
            case PreservationMethod.IceBox:
                return config.iceBoxDuration;
            case PreservationMethod.Salting:
                return config.saltingDuration;
            case PreservationMethod.Smoking:
                return config.smokingDuration;
            case PreservationMethod.Freezing:
                return float.MaxValue; // Indefinite
            default:
                return config.baseDecayTime;
        }
    }

    /// <summary>
    /// Checks if a preservation method is unlocked.
    /// </summary>
    public bool IsMethodUnlocked(PreservationMethod method)
    {
        switch (method)
        {
            case PreservationMethod.None:
                return true;
            case PreservationMethod.IceBox:
                return iceBoxUnlocked;
            case PreservationMethod.Salting:
                return saltingUnlocked;
            case PreservationMethod.Smoking:
                return smokingUnlocked;
            case PreservationMethod.Freezing:
                return freezingUnlocked;
            default:
                return false;
        }
    }

    /// <summary>
    /// Unlocks a preservation method.
    /// </summary>
    public void UnlockMethod(PreservationMethod method)
    {
        switch (method)
        {
            case PreservationMethod.IceBox:
                iceBoxUnlocked = true;
                break;
            case PreservationMethod.Salting:
                saltingUnlocked = true;
                break;
            case PreservationMethod.Smoking:
                smokingUnlocked = true;
                break;
            case PreservationMethod.Freezing:
                freezingUnlocked = true;
                break;
        }

        EventSystem.Publish("PreservationMethodUnlocked", method);

        if (debugMode)
            Debug.Log($"[PreservationSystem] Unlocked {method}");
    }

    /// <summary>
    /// Checks if there's capacity for a preservation method.
    /// </summary>
    private bool HasCapacity(PreservationMethod method)
    {
        switch (method)
        {
            case PreservationMethod.IceBox:
                return iceBoxFish.Count < iceBoxCapacity;
            case PreservationMethod.Smoking:
                return smokedFish.Count < smokehouseCapacity;
            case PreservationMethod.Freezing:
                return frozenFish.Count < freezerCapacity;
            case PreservationMethod.Salting:
                return true; // No limit on salting
            default:
                return true;
        }
    }

    /// <summary>
    /// Adds fish to preservation tracking list.
    /// </summary>
    private void AddToPreservationList(string fishID, PreservationMethod method)
    {
        switch (method)
        {
            case PreservationMethod.IceBox:
                if (!iceBoxFish.Contains(fishID))
                    iceBoxFish.Add(fishID);
                break;
            case PreservationMethod.Salting:
                if (!saltedFish.Contains(fishID))
                    saltedFish.Add(fishID);
                break;
            case PreservationMethod.Smoking:
                if (!smokedFish.Contains(fishID))
                    smokedFish.Add(fishID);
                break;
            case PreservationMethod.Freezing:
                if (!frozenFish.Contains(fishID))
                    frozenFish.Add(fishID);
                break;
        }
    }

    /// <summary>
    /// Removes fish from preservation tracking list.
    /// </summary>
    private void RemoveFromPreservationList(string fishID, PreservationMethod method)
    {
        switch (method)
        {
            case PreservationMethod.IceBox:
                iceBoxFish.Remove(fishID);
                break;
            case PreservationMethod.Salting:
                saltedFish.Remove(fishID);
                break;
            case PreservationMethod.Smoking:
                smokedFish.Remove(fishID);
                break;
            case PreservationMethod.Freezing:
                frozenFish.Remove(fishID);
                break;
        }
    }

    /// <summary>
    /// Gets the current quality multiplier for a fish (0-1).
    /// </summary>
    public float GetQualityMultiplier(string fishID)
    {
        if (!decayTimers.ContainsKey(fishID))
            return 1.0f;

        FishDecayData decay = decayTimers[fishID];

        // Frozen fish maintain perfect quality
        if (decay.preservationMethod == PreservationMethod.Freezing)
            return 1.0f;

        // Calculate quality based on decay percentage
        float decayPercent = decay.elapsedTime / decay.maxPreservationTime;
        return Mathf.Lerp(1.0f, 0.1f, decayPercent); // Quality drops from 100% to 10%
    }

    /// <summary>
    /// Gets remaining preservation time for a fish.
    /// </summary>
    public float GetRemainingTime(string fishID)
    {
        if (!decayTimers.ContainsKey(fishID))
            return 0f;

        FishDecayData decay = decayTimers[fishID];

        if (decay.preservationMethod == PreservationMethod.Freezing)
            return float.MaxValue;

        return Mathf.Max(0f, decay.maxPreservationTime - decay.elapsedTime);
    }

    /// <summary>
    /// Gets the preservation method applied to a fish.
    /// </summary>
    public PreservationMethod GetPreservationMethod(string fishID)
    {
        if (!decayTimers.ContainsKey(fishID))
            return PreservationMethod.None;

        return decayTimers[fishID].preservationMethod;
    }

    // ===== Event Handlers =====

    private void OnFishAdded(InventoryItem item)
    {
        // Start decay tracking for fish items
        if (item.ItemData is FishInventoryItem)
        {
            if (!decayTimers.ContainsKey(item.ItemID))
            {
                decayTimers[item.ItemID] = new FishDecayData
                {
                    fishID = item.ItemID,
                    startTime = Time.time,
                    preservationMethod = PreservationMethod.None,
                    baseDecayTime = config.baseDecayTime,
                    maxPreservationTime = config.baseDecayTime,
                    elapsedTime = 0f
                };

                if (debugMode)
                    Debug.Log($"[PreservationSystem] Started decay tracking for {item.ItemID}");
            }
        }
    }

    private void OnFishRemoved(InventoryItem item)
    {
        // Stop decay tracking
        if (decayTimers.ContainsKey(item.ItemID))
        {
            FishDecayData decay = decayTimers[item.ItemID];
            RemoveFromPreservationList(item.ItemID, decay.preservationMethod);
            decayTimers.Remove(item.ItemID);

            if (debugMode)
                Debug.Log($"[PreservationSystem] Stopped decay tracking for {item.ItemID}");
        }
    }

    // ===== Save/Load Integration =====

    private void OnGatheringSaveData(SaveData saveData)
    {
        saveData.preservationData = new PreservationData
        {
            decayTimers = new Dictionary<string, float>(),
            preservationMethods = new Dictionary<string, PreservationMethod>(),
            iceBoxUnlocked = this.iceBoxUnlocked,
            saltingUnlocked = this.saltingUnlocked,
            smokingUnlocked = this.smokingUnlocked,
            freezingUnlocked = this.freezingUnlocked
        };

        // Save decay data
        foreach (var kvp in decayTimers)
        {
            saveData.preservationData.decayTimers[kvp.Key] = kvp.Value.elapsedTime;
            saveData.preservationData.preservationMethods[kvp.Key] = kvp.Value.preservationMethod;
        }

        if (debugMode)
            Debug.Log($"[PreservationSystem] Saved {decayTimers.Count} decay timers");
    }

    private void OnApplyingSaveData(SaveData saveData)
    {
        decayTimers.Clear();
        iceBoxFish.Clear();
        saltedFish.Clear();
        smokedFish.Clear();
        frozenFish.Clear();

        if (saveData.preservationData != null)
        {
            // Load unlocks
            iceBoxUnlocked = saveData.preservationData.iceBoxUnlocked;
            saltingUnlocked = saveData.preservationData.saltingUnlocked;
            smokingUnlocked = saveData.preservationData.smokingUnlocked;
            freezingUnlocked = saveData.preservationData.freezingUnlocked;

            // Load decay data
            foreach (var kvp in saveData.preservationData.decayTimers)
            {
                string fishID = kvp.Key;
                float elapsedTime = kvp.Value;
                PreservationMethod method = saveData.preservationData.preservationMethods.TryGetValue(fishID, out PreservationMethod m)
                    ? m
                    : PreservationMethod.None;

                decayTimers[fishID] = new FishDecayData
                {
                    fishID = fishID,
                    startTime = Time.time - elapsedTime,
                    elapsedTime = elapsedTime,
                    preservationMethod = method,
                    baseDecayTime = config.baseDecayTime,
                    maxPreservationTime = GetPreservationTime(method)
                };

                AddToPreservationList(fishID, method);
            }

            if (debugMode)
                Debug.Log($"[PreservationSystem] Loaded {decayTimers.Count} decay timers");
        }
    }

    // ===== Debug Methods =====

    [ContextMenu("Debug: Unlock All Methods")]
    public void DebugUnlockAllMethods()
    {
        UnlockMethod(PreservationMethod.IceBox);
        UnlockMethod(PreservationMethod.Salting);
        UnlockMethod(PreservationMethod.Smoking);
        UnlockMethod(PreservationMethod.Freezing);

        Debug.Log("[PreservationSystem] Unlocked all preservation methods");
    }

    [ContextMenu("Debug: Print Decay Status")]
    public void DebugPrintDecayStatus()
    {
        Debug.Log("=== PRESERVATION STATUS ===");
        Debug.Log($"Ice Box: {iceBoxFish.Count}/{iceBoxCapacity}");
        Debug.Log($"Salted: {saltedFish.Count}");
        Debug.Log($"Smoked: {smokedFish.Count}/{smokehouseCapacity}");
        Debug.Log($"Frozen: {frozenFish.Count}/{freezerCapacity}");
        Debug.Log($"Total tracked fish: {decayTimers.Count}");
    }
}

/// <summary>
/// Preservation method types.
/// </summary>
[System.Serializable]
public enum PreservationMethod
{
    None,       // No preservation (48 hour base decay)
    IceBox,     // Ice box storage (7 days)
    Salting,    // Salt preservation (14 days)
    Smoking,    // Smoke preservation (30 days)
    Freezing    // Freezer storage (indefinite)
}

/// <summary>
/// Configuration for preservation times.
/// </summary>
[System.Serializable]
public class PreservationConfig
{
    [Tooltip("Base decay time without preservation (in seconds)")]
    public float baseDecayTime = 172800f; // 48 hours

    [Tooltip("Ice box preservation time (in seconds)")]
    public float iceBoxDuration = 604800f; // 7 days

    [Tooltip("Salting preservation time (in seconds)")]
    public float saltingDuration = 1209600f; // 14 days

    [Tooltip("Smoking preservation time (in seconds)")]
    public float smokingDuration = 2592000f; // 30 days
}

/// <summary>
/// Decay tracking data for a fish.
/// </summary>
[System.Serializable]
public class FishDecayData
{
    public string fishID;
    public float startTime;
    public float elapsedTime;
    public PreservationMethod preservationMethod;
    public float baseDecayTime;
    public float maxPreservationTime;
    public bool halfwayWarned;
    public bool almostDecayedWarned;
}

/// <summary>
/// Data for fish preservation events.
/// </summary>
[System.Serializable]
public class FishPreservationData
{
    public string fishID;
    public PreservationMethod method;
    public float preservationTime;
}

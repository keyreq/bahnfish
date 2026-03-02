using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Types of environmental storytelling elements
/// </summary>
public enum ClueType
{
    MessageInBottle,    // Floating bottles with previous fisher's logs
    RockCarving,        // Ancient markings on rocks
    SunkenJournal,      // Recovered via dredging
    GhostWhisper,       // Audio clues at low sanity
    AbandonedBoat,      // Wrecked vessels
    BrokenEquipment,    // Scattered fishing gear
    StrangeMarking,     // Mysterious symbols
    EerieStatue        // Monuments to the entity
}

/// <summary>
/// Individual environmental clue data
/// </summary>
[System.Serializable]
public class EnvironmentalClue
{
    [Header("Clue Identity")]
    public string clueID;
    public string clueName;
    public ClueType clueType;

    [Header("Content")]
    [TextArea(4, 8)]
    public string clueText; // What the clue reveals
    public Sprite clueImage; // Visual representation

    [Header("Discovery Requirements")]
    public string requiredLocationID; // Must be in this location
    public TimeOfDay requiredTimeOfDay = TimeOfDay.Day;
    public float requiredMaxSanity = 100f; // Must be below this sanity (for ghost whispers)
    public int requiredStoryAct = 0; // Minimum story progression

    [Header("Spawn Settings")]
    public Vector3 spawnPosition;
    public float spawnRadius = 5f; // Random offset within radius
    public bool isRandom = false; // Spawns randomly in location
    public float spawnChance = 1f; // 0-1 probability if random

    [Header("Rewards")]
    public List<string> journalEntriesToUnlock = new List<string>();
    public int relicReward = 0;
    public float moneyReward = 0f;
}

/// <summary>
/// Spawns and manages environmental storytelling elements.
/// Handles messages in bottles, carvings, journal entries, and atmospheric clues.
/// </summary>
public class EnvironmentalClues : MonoBehaviour
{
    private static EnvironmentalClues _instance;
    public static EnvironmentalClues Instance => _instance;

    [Header("Clue Database")]
    [SerializeField] private List<EnvironmentalClue> allClues = new List<EnvironmentalClue>();

    [Header("Spawn State")]
    private List<string> discoveredClueIDs = new List<string>();
    private List<GameObject> activeClueObjects = new List<GameObject>();

    [Header("Prefabs")]
    [SerializeField] private GameObject messageBottlePrefab;
    [SerializeField] private GameObject rockCarvingPrefab;
    [SerializeField] private GameObject journalPrefab;
    [SerializeField] private GameObject markedBoatPrefab;
    [SerializeField] private GameObject brokenEquipmentPrefab;
    [SerializeField] private GameObject strangeMarkingPrefab;
    [SerializeField] private GameObject eerieStatuePrefab;

    [Header("Audio")]
    [SerializeField] private List<AudioClip> ghostWhispers = new List<AudioClip>();

    [Header("Debug")]
    [SerializeField] private bool debugMode = false;

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
        EventSystem.Subscribe<string>("LocationChanged", OnLocationChanged);
        EventSystem.Subscribe<TimeChangedEventData>("TimeChanged", OnTimeChanged);
        EventSystem.Subscribe<float>("SanityChanged", OnSanityChanged);
        EventSystem.Subscribe<SaveData>("GatheringSaveData", OnGatheringSave);
        EventSystem.Subscribe<SaveData>("ApplyingSaveData", OnApplyingSave);

        // Initial spawn
        SpawnCluesForCurrentLocation();
    }

    private void OnDestroy()
    {
        EventSystem.Unsubscribe<string>("LocationChanged", OnLocationChanged);
        EventSystem.Unsubscribe<TimeChangedEventData>("TimeChanged", OnTimeChanged);
        EventSystem.Unsubscribe<float>("SanityChanged", OnSanityChanged);
        EventSystem.Unsubscribe<SaveData>("GatheringSaveData", OnGatheringSave);
        EventSystem.Unsubscribe<SaveData>("ApplyingSaveData", OnApplyingSave);
    }

    #region Clue Spawning

    /// <summary>
    /// Spawn clues for current location
    /// </summary>
    private void SpawnCluesForCurrentLocation()
    {
        // Clear existing clues
        ClearActiveClues();

        GameState gameState = GameManager.Instance?.CurrentGameState;
        if (gameState == null)
            return;

        // Get available clues for this location
        List<EnvironmentalClue> availableClues = GetAvailableClues(
            gameState.currentLocationID,
            gameState.timeOfDay,
            gameState.sanity
        );

        // Spawn each available clue
        foreach (var clue in availableClues)
        {
            if (!discoveredClueIDs.Contains(clue.clueID))
            {
                SpawnClue(clue);
            }
        }
    }

    /// <summary>
    /// Spawn a single clue
    /// </summary>
    private void SpawnClue(EnvironmentalClue clue)
    {
        // Check spawn chance for random clues
        if (clue.isRandom && Random.value > clue.spawnChance)
            return;

        // Get appropriate prefab
        GameObject prefab = GetPrefabForClueType(clue.clueType);
        if (prefab == null)
        {
            Debug.LogWarning($"No prefab for clue type: {clue.clueType}");
            return;
        }

        // Calculate spawn position
        Vector3 spawnPos = clue.spawnPosition;
        if (clue.spawnRadius > 0f)
        {
            Vector2 randomOffset = Random.insideUnitCircle * clue.spawnRadius;
            spawnPos += new Vector3(randomOffset.x, 0f, randomOffset.y);
        }

        // Spawn clue object
        GameObject clueObject = Instantiate(prefab, spawnPos, Quaternion.identity);
        clueObject.name = $"Clue_{clue.clueID}";

        // Add clue interaction component
        ClueInteraction interaction = clueObject.AddComponent<ClueInteraction>();
        interaction.Initialize(clue);

        activeClueObjects.Add(clueObject);

        if (debugMode)
            Debug.Log($"Spawned clue: {clue.clueID} at {spawnPos}");
    }

    /// <summary>
    /// Clear all active clue objects
    /// </summary>
    private void ClearActiveClues()
    {
        foreach (var clueObj in activeClueObjects)
        {
            if (clueObj != null)
                Destroy(clueObj);
        }
        activeClueObjects.Clear();
    }

    /// <summary>
    /// Get prefab for clue type
    /// </summary>
    private GameObject GetPrefabForClueType(ClueType type)
    {
        switch (type)
        {
            case ClueType.MessageInBottle: return messageBottlePrefab;
            case ClueType.RockCarving: return rockCarvingPrefab;
            case ClueType.SunkenJournal: return journalPrefab;
            case ClueType.AbandonedBoat: return markedBoatPrefab;
            case ClueType.BrokenEquipment: return brokenEquipmentPrefab;
            case ClueType.StrangeMarking: return strangeMarkingPrefab;
            case ClueType.EerieStatue: return eerieStatuePrefab;
            default: return null;
        }
    }

    #endregion

    #region Clue Discovery

    /// <summary>
    /// Discover a clue (called by ClueInteraction)
    /// </summary>
    public void DiscoverClue(EnvironmentalClue clue)
    {
        if (discoveredClueIDs.Contains(clue.clueID))
            return;

        discoveredClueIDs.Add(clue.clueID);

        Debug.Log($"Clue discovered: {clue.clueName}");

        // Publish discovery event
        EventSystem.Publish("ClueDiscovered", clue.clueID);

        // Special handling for messages in bottles
        if (clue.clueType == ClueType.MessageInBottle)
        {
            EventSystem.Publish("MessageInBottleFound", clue.clueID);
        }

        // Unlock journal entries
        foreach (var entryID in clue.journalEntriesToUnlock)
        {
            EventSystem.Publish("UnlockJournalEntry", entryID);
        }

        // Grant rewards
        if (clue.relicReward > 0)
            EventSystem.Publish("AddRelics", clue.relicReward);
        if (clue.moneyReward > 0f)
            EventSystem.Publish("AddMoney", clue.moneyReward);

        // Show clue UI
        EventSystem.Publish("ShowClueDiscovery", clue);
    }

    #endregion

    #region Ghost Whispers

    /// <summary>
    /// Play ghost whisper audio at low sanity
    /// </summary>
    private void PlayGhostWhisper()
    {
        if (ghostWhispers.Count == 0)
            return;

        AudioClip whisper = ghostWhispers[Random.Range(0, ghostWhispers.Count)];
        EventSystem.Publish("PlaySpatialSound", new { clip = whisper, position = transform.position, volume = 0.5f });

        // Maybe spawn a whisper clue
        var whisperClues = allClues.FindAll(c => c.clueType == ClueType.GhostWhisper);
        if (whisperClues.Count > 0 && Random.value < 0.3f) // 30% chance
        {
            var randomWhisper = whisperClues[Random.Range(0, whisperClues.Count)];
            if (!discoveredClueIDs.Contains(randomWhisper.clueID))
            {
                DiscoverClue(randomWhisper);
            }
        }
    }

    #endregion

    #region Query Methods

    /// <summary>
    /// Get available clues for location and conditions
    /// </summary>
    private List<EnvironmentalClue> GetAvailableClues(string locationID, TimeOfDay timeOfDay, float sanity)
    {
        List<EnvironmentalClue> available = new List<EnvironmentalClue>();
        int currentStoryAct = QuestManager.Instance.GetCurrentStoryAct();

        foreach (var clue in allClues)
        {
            // Skip if already discovered
            if (discoveredClueIDs.Contains(clue.clueID))
                continue;

            // Check location
            if (!string.IsNullOrEmpty(clue.requiredLocationID) && clue.requiredLocationID != locationID)
                continue;

            // Check time of day
            if (clue.requiredTimeOfDay != timeOfDay && clue.requiredTimeOfDay != TimeOfDay.Day) // Day = always
                continue;

            // Check sanity requirement
            if (sanity > clue.requiredMaxSanity)
                continue;

            // Check story progression
            if (currentStoryAct < clue.requiredStoryAct)
                continue;

            available.Add(clue);
        }

        return available;
    }

    public bool IsClueDiscovered(string clueID) => discoveredClueIDs.Contains(clueID);
    public int GetDiscoveredClueCount() => discoveredClueIDs.Count;
    public int GetTotalClueCount() => allClues.Count;

    #endregion

    #region Event Handlers

    private void OnLocationChanged(string newLocationID)
    {
        SpawnCluesForCurrentLocation();
    }

    private void OnTimeChanged(TimeChangedEventData data)
    {
        SpawnCluesForCurrentLocation();
    }

    private void OnSanityChanged(float newSanity)
    {
        // Play ghost whispers at very low sanity
        if (newSanity <= 10f && Random.value < 0.05f) // 5% chance each frame
        {
            PlayGhostWhisper();
        }

        // Refresh clues (some only appear at low sanity)
        SpawnCluesForCurrentLocation();
    }

    #endregion

    #region Save/Load

    private void OnGatheringSave(SaveData data)
    {
        // Save discovered clues in completedQuests (repurposing)
        foreach (var clueID in discoveredClueIDs)
        {
            if (!data.completedQuests.Contains($"clue_{clueID}"))
                data.completedQuests.Add($"clue_{clueID}");
        }

        if (debugMode)
            Debug.Log($"Saved {discoveredClueIDs.Count} discovered clues");
    }

    private void OnApplyingSave(SaveData data)
    {
        // Restore discovered clues
        discoveredClueIDs.Clear();
        foreach (var questID in data.completedQuests)
        {
            if (questID.StartsWith("clue_"))
            {
                string clueID = questID.Substring(5); // Remove "clue_" prefix
                discoveredClueIDs.Add(clueID);
            }
        }

        if (debugMode)
            Debug.Log($"Loaded {discoveredClueIDs.Count} discovered clues");

        // Respawn clues
        SpawnCluesForCurrentLocation();
    }

    #endregion
}

/// <summary>
/// Component attached to clue objects for player interaction
/// </summary>
public class ClueInteraction : MonoBehaviour, IInteractable
{
    private EnvironmentalClue clueData;
    private float interactionRange = 2f;

    public void Initialize(EnvironmentalClue data)
    {
        clueData = data;
    }

    public void Interact()
    {
        if (clueData != null)
        {
            EnvironmentalClues.Instance.DiscoverClue(clueData);
            Destroy(gameObject); // Remove after discovery
        }
    }

    public string GetInteractionPrompt()
    {
        return clueData != null ? $"[E] Examine {clueData.clueName}" : "[E] Examine";
    }

    public bool CanInteract() => true;
    public float GetInteractionRange() => interactionRange;
}

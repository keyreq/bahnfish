using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Categories for journal entries
/// </summary>
public enum JournalCategory
{
    Fish,           // Fish species descriptions
    Locations,      // Location histories
    NPCs,           // NPC backstories
    Aberrations,    // Aberrant fish origins
    Lore,           // Ancient entity and mystery lore
    Quests,         // Quest logs
    Clues,          // Discovered clues
    Story          // Main story progression
}

/// <summary>
/// Individual journal entry
/// </summary>
[System.Serializable]
public class JournalEntry
{
    [Header("Entry Identity")]
    public string entryID;
    public string title;
    public JournalCategory category;

    [Header("Content")]
    [TextArea(6, 12)]
    public string content;
    public List<Sprite> images = new List<Sprite>();

    [Header("Metadata")]
    public string author = "Unknown"; // Who wrote this entry
    public string dateDiscovered;
    public bool isLocked = true; // Starts locked until discovered

    [Header("Related Entries")]
    public List<string> relatedEntryIDs = new List<string>(); // Links to other entries

    /// <summary>
    /// Unlock this journal entry
    /// </summary>
    public void Unlock()
    {
        if (!isLocked)
            return;

        isLocked = false;
        dateDiscovered = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        Debug.Log($"Journal entry unlocked: {title}");
    }
}

/// <summary>
/// Manages the player's journal/log book.
/// Tracks quest logs, lore entries, bestiary, and discovered clues.
/// Provides data for Journal UI.
/// </summary>
public class JournalSystem : MonoBehaviour
{
    private static JournalSystem _instance;
    public static JournalSystem Instance => _instance;

    [Header("Journal Database")]
    [SerializeField] private List<JournalEntry> allEntries = new List<JournalEntry>();

    [Header("Unlocked Entries")]
    private List<string> unlockedEntryIDs = new List<string>();

    [Header("Statistics")]
    private Dictionary<JournalCategory, int> categoryProgress = new Dictionary<JournalCategory, int>();

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

        InitializeCategoryProgress();
    }

    private void Start()
    {
        // Subscribe to unlock events
        EventSystem.Subscribe<string>("UnlockJournalEntry", OnUnlockJournalEntry);
        EventSystem.Subscribe<Fish>("FishCaught", OnFishCaught);
        EventSystem.Subscribe<string>("LocationVisited", OnLocationVisited);
        EventSystem.Subscribe<string>("NPCFirstMet", OnNPCFirstMet);
        EventSystem.Subscribe<Quest>("QuestStarted", OnQuestStarted);
        EventSystem.Subscribe<Quest>("QuestCompleted", OnQuestCompleted);

        // Subscribe to save/load
        EventSystem.Subscribe<SaveData>("GatheringSaveData", OnGatheringSave);
        EventSystem.Subscribe<SaveData>("ApplyingSaveData", OnApplyingSave);

        // Unlock starting entries
        UnlockEntry("journal_start");
        UnlockEntry("how_to_fish");
    }

    private void OnDestroy()
    {
        EventSystem.Unsubscribe<string>("UnlockJournalEntry", OnUnlockJournalEntry);
        EventSystem.Unsubscribe<Fish>("FishCaught", OnFishCaught);
        EventSystem.Unsubscribe<string>("LocationVisited", OnLocationVisited);
        EventSystem.Unsubscribe<string>("NPCFirstMet", OnNPCFirstMet);
        EventSystem.Unsubscribe<Quest>("QuestStarted", OnQuestStarted);
        EventSystem.Unsubscribe<Quest>("QuestCompleted", OnQuestCompleted);
        EventSystem.Unsubscribe<SaveData>("GatheringSaveData", OnGatheringSave);
        EventSystem.Unsubscribe<SaveData>("ApplyingSaveData", OnApplyingSave);
    }

    private void InitializeCategoryProgress()
    {
        foreach (JournalCategory category in System.Enum.GetValues(typeof(JournalCategory)))
        {
            categoryProgress[category] = 0;
        }
    }

    #region Entry Management

    /// <summary>
    /// Unlock a journal entry by ID
    /// </summary>
    public void UnlockEntry(string entryID)
    {
        if (unlockedEntryIDs.Contains(entryID))
            return;

        JournalEntry entry = GetEntryByID(entryID);
        if (entry == null)
        {
            if (debugMode)
                Debug.LogWarning($"Journal entry not found: {entryID}");
            return;
        }

        // Unlock the entry
        entry.Unlock();
        unlockedEntryIDs.Add(entryID);

        // Update category progress
        if (categoryProgress.ContainsKey(entry.category))
            categoryProgress[entry.category]++;

        Debug.Log($"Journal entry unlocked: {entry.title} ({entry.category})");

        // Notify UI
        EventSystem.Publish("JournalEntryUnlocked", entry);

        // Show notification
        EventSystem.Publish("ShowNotification", $"New Journal Entry: {entry.title}");
    }

    /// <summary>
    /// Get journal entry by ID
    /// </summary>
    private JournalEntry GetEntryByID(string entryID)
    {
        return allEntries.FirstOrDefault(e => e.entryID == entryID);
    }

    /// <summary>
    /// Get all unlocked entries
    /// </summary>
    public List<JournalEntry> GetUnlockedEntries()
    {
        return allEntries.Where(e => unlockedEntryIDs.Contains(e.entryID)).ToList();
    }

    /// <summary>
    /// Get unlocked entries by category
    /// </summary>
    public List<JournalEntry> GetEntriesByCategory(JournalCategory category)
    {
        return allEntries.Where(e =>
            e.category == category &&
            unlockedEntryIDs.Contains(e.entryID)
        ).ToList();
    }

    /// <summary>
    /// Get all entries in category (including locked)
    /// </summary>
    public List<JournalEntry> GetAllEntriesInCategory(JournalCategory category)
    {
        return allEntries.Where(e => e.category == category).ToList();
    }

    #endregion

    #region Auto-Unlock Events

    private void OnUnlockJournalEntry(string entryID)
    {
        UnlockEntry(entryID);
    }

    private void OnFishCaught(Fish fish)
    {
        // Unlock bestiary entry for this fish
        string fishEntryID = $"fish_{fish.id}";
        UnlockEntry(fishEntryID);

        // Unlock aberrant fish lore if aberrant
        if (fish.isAberrant)
        {
            UnlockEntry("aberrant_discovery");
        }
    }

    private void OnLocationVisited(string locationID)
    {
        // Unlock location history entry
        string locationEntryID = $"location_{locationID}";
        UnlockEntry(locationEntryID);
    }

    private void OnNPCFirstMet(string npcID)
    {
        // Unlock NPC backstory entry
        string npcEntryID = $"npc_{npcID}";
        UnlockEntry(npcEntryID);
    }

    private void OnQuestStarted(Quest quest)
    {
        // Unlock quest log entry
        string questEntryID = $"quest_{quest.questID}";
        UnlockEntry(questEntryID);
    }

    private void OnQuestCompleted(Quest quest)
    {
        // Unlock quest completion lore (if any)
        string completionEntryID = $"quest_{quest.questID}_complete";
        if (GetEntryByID(completionEntryID) != null)
        {
            UnlockEntry(completionEntryID);
        }
    }

    #endregion

    #region Statistics & Progress

    /// <summary>
    /// Get progress in a category (unlocked / total)
    /// </summary>
    public (int unlocked, int total) GetCategoryProgress(JournalCategory category)
    {
        var allInCategory = allEntries.Where(e => e.category == category).ToList();
        int total = allInCategory.Count;
        int unlocked = allInCategory.Count(e => unlockedEntryIDs.Contains(e.entryID));
        return (unlocked, total);
    }

    /// <summary>
    /// Get overall journal completion percentage
    /// </summary>
    public float GetOverallCompletion()
    {
        if (allEntries.Count == 0)
            return 0f;

        return (float)unlockedEntryIDs.Count / allEntries.Count;
    }

    /// <summary>
    /// Get completion percentage for category
    /// </summary>
    public float GetCategoryCompletion(JournalCategory category)
    {
        var (unlocked, total) = GetCategoryProgress(category);
        if (total == 0)
            return 0f;

        return (float)unlocked / total;
    }

    /// <summary>
    /// Check if entry is unlocked
    /// </summary>
    public bool IsEntryUnlocked(string entryID)
    {
        return unlockedEntryIDs.Contains(entryID);
    }

    #endregion

    #region Bestiary Integration

    /// <summary>
    /// Get fish bestiary entries
    /// </summary>
    public List<JournalEntry> GetBestiary()
    {
        return GetEntriesByCategory(JournalCategory.Fish);
    }

    /// <summary>
    /// Get aberrant fish entries
    /// </summary>
    public List<JournalEntry> GetAberrantBestiary()
    {
        return GetEntriesByCategory(JournalCategory.Aberrations);
    }

    #endregion

    #region Quest Log

    /// <summary>
    /// Get active quest entries
    /// </summary>
    public List<JournalEntry> GetActiveQuestLog()
    {
        var activeQuests = QuestManager.Instance.GetActiveQuests();
        List<JournalEntry> questEntries = new List<JournalEntry>();

        foreach (var quest in activeQuests)
        {
            string questEntryID = $"quest_{quest.questID}";
            JournalEntry entry = GetEntryByID(questEntryID);
            if (entry != null && IsEntryUnlocked(questEntryID))
            {
                questEntries.Add(entry);
            }
        }

        return questEntries;
    }

    #endregion

    #region Save/Load

    private void OnGatheringSave(SaveData data)
    {
        // Save unlocked journal entries
        foreach (var entryID in unlockedEntryIDs)
        {
            if (!data.completedQuests.Contains($"journal_{entryID}"))
                data.completedQuests.Add($"journal_{entryID}");
        }

        if (debugMode)
            Debug.Log($"Saved {unlockedEntryIDs.Count} journal entries");
    }

    private void OnApplyingSave(SaveData data)
    {
        // Restore unlocked journal entries
        unlockedEntryIDs.Clear();
        foreach (var questID in data.completedQuests)
        {
            if (questID.StartsWith("journal_"))
            {
                string entryID = questID.Substring(8); // Remove "journal_" prefix
                unlockedEntryIDs.Add(entryID);

                // Update entry unlock state
                JournalEntry entry = GetEntryByID(entryID);
                if (entry != null)
                {
                    entry.isLocked = false;
                }
            }
        }

        // Recalculate category progress
        InitializeCategoryProgress();
        foreach (var entryID in unlockedEntryIDs)
        {
            JournalEntry entry = GetEntryByID(entryID);
            if (entry != null && categoryProgress.ContainsKey(entry.category))
            {
                categoryProgress[entry.category]++;
            }
        }

        if (debugMode)
            Debug.Log($"Loaded {unlockedEntryIDs.Count} journal entries");
    }

    #endregion

    #region Debug

    /// <summary>
    /// Debug: Unlock all journal entries
    /// </summary>
    public void DebugUnlockAll()
    {
        foreach (var entry in allEntries)
        {
            UnlockEntry(entry.entryID);
        }
        Debug.Log("DEBUG: Unlocked all journal entries");
    }

    /// <summary>
    /// Debug: Print journal statistics
    /// </summary>
    public void DebugPrintStats()
    {
        Debug.Log($"=== JOURNAL STATISTICS ===");
        Debug.Log($"Overall: {unlockedEntryIDs.Count}/{allEntries.Count} ({GetOverallCompletion() * 100f:F1}%)");

        foreach (JournalCategory category in System.Enum.GetValues(typeof(JournalCategory)))
        {
            var (unlocked, total) = GetCategoryProgress(category);
            Debug.Log($"{category}: {unlocked}/{total} ({GetCategoryCompletion(category) * 100f:F1}%)");
        }
    }

    #endregion
}

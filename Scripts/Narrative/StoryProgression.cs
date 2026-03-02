using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Story acts in Bahnfish main mystery
/// </summary>
public enum StoryAct
{
    Prologue = 0,       // Tutorial, arrival at dock
    Act1_Arrival = 1,   // Learn basics, first hints
    Act2_Discovery = 2, // Find messages, aberrant fish appear
    Act3_Investigation = 3, // Unlock cavern, meet ghost
    Act4_Truth = 4,     // Learn full story, make choice
    Act5_Resolution = 5 // Ending based on player choice
}

/// <summary>
/// Possible story endings
/// </summary>
public enum StoryEnding
{
    None,           // Not reached ending yet
    SealEntity,     // Good ending: seal the ancient entity
    EmbraceDark,    // Dark ending: gain ultimate power
    IgnoreMystery   // Neutral ending: keep fishing, ignore truth
}

/// <summary>
/// Manages the main mystery storyline progression.
/// Tracks story acts, key discoveries, and multiple endings.
/// </summary>
public class StoryProgression : MonoBehaviour
{
    private static StoryProgression _instance;
    public static StoryProgression Instance => _instance;

    [Header("Story State")]
    [SerializeField] private StoryAct currentAct = StoryAct.Prologue;
    [SerializeField] private StoryEnding chosenEnding = StoryEnding.None;

    [Header("Key Discoveries")]
    [SerializeField] private List<string> discoveredClues = new List<string>();
    [SerializeField] private List<string> messagesInBottlesFound = new List<string>();
    [SerializeField] private List<string> altarsDiscovered = new List<string>();

    [Header("Story Milestones")]
    [SerializeField] private bool hasMetGhostFisher = false;
    [SerializeField] private bool hasFoundAllAltars = false;
    [SerializeField] private bool hasLearnedFullTruth = false;
    [SerializeField] private bool hasCompletedRitual = false;

    [Header("Debug")]
    [SerializeField] private bool debugMode = false;

    // Story constants
    private const int TOTAL_MESSAGES = 5;
    private const int TOTAL_ALTARS = 3;
    private const int TOTAL_LORE_ENTRIES = 50;

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
        // Subscribe to discovery events
        EventSystem.Subscribe<string>("ClueDiscovered", OnClueDiscovered);
        EventSystem.Subscribe<string>("MessageInBottleFound", OnMessageInBottleFound);
        EventSystem.Subscribe<string>("AltarDiscovered", OnAltarDiscovered);
        EventSystem.Subscribe<string>("NPCTalkedTo", OnNPCTalkedTo);
        EventSystem.Subscribe<Quest>("QuestCompleted", OnQuestCompleted);

        // Subscribe to save/load
        EventSystem.Subscribe<SaveData>("GatheringSaveData", OnGatheringSave);
        EventSystem.Subscribe<SaveData>("ApplyingSaveData", OnApplyingSave);
    }

    private void OnDestroy()
    {
        EventSystem.Unsubscribe<string>("ClueDiscovered", OnClueDiscovered);
        EventSystem.Unsubscribe<string>("MessageInBottleFound", OnMessageInBottleFound);
        EventSystem.Unsubscribe<string>("AltarDiscovered", OnAltarDiscovered);
        EventSystem.Unsubscribe<string>("NPCTalkedTo", OnNPCTalkedTo);
        EventSystem.Unsubscribe<Quest>("QuestCompleted", OnQuestCompleted);
        EventSystem.Unsubscribe<SaveData>("GatheringSaveData", OnGatheringSave);
        EventSystem.Unsubscribe<SaveData>("ApplyingSaveData", OnApplyingSave);
    }

    #region Story Progression

    /// <summary>
    /// Advance to the next story act
    /// </summary>
    public void AdvanceToAct(StoryAct act)
    {
        if (act <= currentAct)
        {
            Debug.LogWarning($"Cannot advance to earlier act: {act}");
            return;
        }

        StoryAct previousAct = currentAct;
        currentAct = act;

        Debug.Log($"Story progressed: {previousAct} -> {currentAct}");
        EventSystem.Publish("StoryActChanged", currentAct);

        // Trigger act-specific events
        OnActChanged(currentAct);

        // Update quest manager
        QuestManager.Instance.AdvanceStoryProgression((int)currentAct);
    }

    /// <summary>
    /// Handle act-specific triggers
    /// </summary>
    private void OnActChanged(StoryAct newAct)
    {
        switch (newAct)
        {
            case StoryAct.Act1_Arrival:
                // First night experience
                Debug.Log("ACT 1: Something's wrong with these waters...");
                EventSystem.Publish("UnlockJournalEntry", "act1_intro");
                break;

            case StoryAct.Act2_Discovery:
                // Aberrant fish start appearing
                Debug.Log("ACT 2: The fish... they're changing...");
                EventSystem.Publish("UnlockJournalEntry", "act2_aberrant_discovery");
                EventSystem.Publish("EnableAberrantFish", true);
                break;

            case StoryAct.Act3_Investigation:
                // Underground cavern unlocks
                Debug.Log("ACT 3: We should never have disturbed it...");
                EventSystem.Publish("UnlockLocation", "underground_cavern");
                EventSystem.Publish("UnlockJournalEntry", "act3_cavern");
                break;

            case StoryAct.Act4_Truth:
                // Full story revealed
                Debug.Log("ACT 4: The truth is darker than imagined...");
                EventSystem.Publish("UnlockJournalEntry", "act4_full_truth");
                hasLearnedFullTruth = true;
                break;

            case StoryAct.Act5_Resolution:
                // Ending paths available
                Debug.Log("ACT 5: Choose your fate...");
                EventSystem.Publish("UnlockJournalEntry", "act5_choice");
                break;
        }
    }

    #endregion

    #region Discoveries

    private void OnClueDiscovered(string clueID)
    {
        if (discoveredClues.Contains(clueID))
            return;

        discoveredClues.Add(clueID);
        Debug.Log($"Clue discovered: {clueID}");

        // Check for story progression triggers
        CheckStoryProgression();
    }

    private void OnMessageInBottleFound(string messageID)
    {
        if (messagesInBottlesFound.Contains(messageID))
            return;

        messagesInBottlesFound.Add(messageID);
        Debug.Log($"Message in bottle found ({messagesInBottlesFound.Count}/{TOTAL_MESSAGES}): {messageID}");

        EventSystem.Publish("UnlockJournalEntry", $"message_{messageID}");

        // Progress to Act 2 if first message found
        if (messagesInBottlesFound.Count == 1 && currentAct < StoryAct.Act2_Discovery)
        {
            AdvanceToAct(StoryAct.Act2_Discovery);
        }

        CheckStoryProgression();
    }

    private void OnAltarDiscovered(string altarID)
    {
        if (altarsDiscovered.Contains(altarID))
            return;

        altarsDiscovered.Add(altarID);
        Debug.Log($"Altar discovered ({altarsDiscovered.Count}/{TOTAL_ALTARS}): {altarID}");

        EventSystem.Publish("UnlockJournalEntry", $"altar_{altarID}");

        // All altars found
        if (altarsDiscovered.Count >= TOTAL_ALTARS)
        {
            hasFoundAllAltars = true;
            EventSystem.Publish("AllAltarsDiscovered");

            // Progress to Act 3
            if (currentAct < StoryAct.Act3_Investigation)
            {
                AdvanceToAct(StoryAct.Act3_Investigation);
            }
        }

        CheckStoryProgression();
    }

    private void OnNPCTalkedTo(string npcID)
    {
        // Ghost fisher appears at 0 sanity
        if (npcID == "ghost_fisher" && !hasMetGhostFisher)
        {
            hasMetGhostFisher = true;
            Debug.Log("Ghost of previous fisher encountered...");
            EventSystem.Publish("UnlockJournalEntry", "ghost_fisher_met");

            // Progress to Act 3 if not already there
            if (currentAct < StoryAct.Act3_Investigation)
            {
                AdvanceToAct(StoryAct.Act3_Investigation);
            }
        }
    }

    private void OnQuestCompleted(Quest quest)
    {
        // Check for key story quests
        if (quest.questType == QuestType.Story)
        {
            CheckStoryProgression();
        }
    }

    /// <summary>
    /// Check if story should progress based on discoveries
    /// </summary>
    private void CheckStoryProgression()
    {
        // Act 1 -> Act 2: First message found
        if (currentAct == StoryAct.Act1_Arrival && messagesInBottlesFound.Count > 0)
        {
            AdvanceToAct(StoryAct.Act2_Discovery);
        }

        // Act 2 -> Act 3: All altars found OR ghost met
        if (currentAct == StoryAct.Act2_Discovery && (hasFoundAllAltars || hasMetGhostFisher))
        {
            AdvanceToAct(StoryAct.Act3_Investigation);
        }

        // Act 3 -> Act 4: All messages found AND ghost met AND all altars found
        if (currentAct == StoryAct.Act3_Investigation &&
            messagesInBottlesFound.Count >= TOTAL_MESSAGES &&
            hasMetGhostFisher &&
            hasFoundAllAltars)
        {
            AdvanceToAct(StoryAct.Act4_Truth);
        }
    }

    #endregion

    #region Endings

    /// <summary>
    /// Choose the "Seal Entity" ending
    /// </summary>
    public void ChooseSealEntityEnding()
    {
        if (currentAct < StoryAct.Act4_Truth)
        {
            Debug.LogWarning("Cannot choose ending - story not advanced enough");
            return;
        }

        chosenEnding = StoryEnding.SealEntity;
        hasCompletedRitual = true;
        AdvanceToAct(StoryAct.Act5_Resolution);

        Debug.Log("ENDING: Seal the Deep One (Good Ending)");
        EventSystem.Publish("EndingChosen", StoryEnding.SealEntity);
        EventSystem.Publish("UnlockJournalEntry", "ending_seal");

        // Remove night horror permanently
        EventSystem.Publish("DisableNightHorror", true);
    }

    /// <summary>
    /// Choose the "Embrace Darkness" ending
    /// </summary>
    public void ChooseEmbraceDarknessEnding()
    {
        if (currentAct < StoryAct.Act4_Truth)
        {
            Debug.LogWarning("Cannot choose ending - story not advanced enough");
            return;
        }

        chosenEnding = StoryEnding.EmbraceDark;
        hasCompletedRitual = true;
        AdvanceToAct(StoryAct.Act5_Resolution);

        Debug.Log("ENDING: Embrace the Darkness (Dark Ending)");
        EventSystem.Publish("EndingChosen", StoryEnding.EmbraceDark);
        EventSystem.Publish("UnlockJournalEntry", "ending_darkness");

        // Grant ultimate dark power
        EventSystem.Publish("UnlockAbility", "ultimate_dark_power");
    }

    /// <summary>
    /// Choose the "Ignore Mystery" ending
    /// </summary>
    public void ChooseIgnoreMysteryEnding()
    {
        chosenEnding = StoryEnding.IgnoreMystery;
        AdvanceToAct(StoryAct.Act5_Resolution);

        Debug.Log("ENDING: Just Keep Fishing (Neutral Ending)");
        EventSystem.Publish("EndingChosen", StoryEnding.IgnoreMystery);
        EventSystem.Publish("UnlockJournalEntry", "ending_neutral");
    }

    #endregion

    #region Query Methods

    public StoryAct GetCurrentAct() => currentAct;
    public StoryEnding GetChosenEnding() => chosenEnding;
    public bool HasMetGhostFisher() => hasMetGhostFisher;
    public bool HasFoundAllAltars() => hasFoundAllAltars;
    public bool HasLearnedFullTruth() => hasLearnedFullTruth;
    public int GetMessagesFound() => messagesInBottlesFound.Count;
    public int GetAltarsFound() => altarsDiscovered.Count;
    public int GetCluesFound() => discoveredClues.Count;

    #endregion

    #region Save/Load

    private void OnGatheringSave(SaveData data)
    {
        // Save story progression in unused fields or add to additionalData
        // For now, use quest progress dictionary
        data.questProgress["story_act"] = (int)currentAct;
        data.questProgress["story_ending"] = (int)chosenEnding;
        data.questProgress["messages_found"] = messagesInBottlesFound.Count;
        data.questProgress["altars_found"] = altarsDiscovered.Count;
        data.questProgress["clues_found"] = discoveredClues.Count;

        if (debugMode)
            Debug.Log($"Saved story progression: Act {currentAct}");
    }

    private void OnApplyingSave(SaveData data)
    {
        // Restore story progression
        if (data.questProgress.ContainsKey("story_act"))
            currentAct = (StoryAct)data.questProgress["story_act"];

        if (data.questProgress.ContainsKey("story_ending"))
            chosenEnding = (StoryEnding)data.questProgress["story_ending"];

        if (debugMode)
            Debug.Log($"Loaded story progression: Act {currentAct}");
    }

    #endregion
}

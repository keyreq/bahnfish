using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Example integration code showing how other systems interact with the Narrative system.
/// This demonstrates proper usage patterns for quests, NPCs, dialogue, and story progression.
/// </summary>
public class NarrativeIntegrationExample : MonoBehaviour
{
    // ============================================================
    // EXAMPLE 1: Starting and Completing Quests
    // ============================================================

    public void Example_StartAndCompleteQuest()
    {
        // Start a quest by ID
        bool questStarted = QuestManager.Instance.StartQuest("tutorial_first_catch");

        if (questStarted)
        {
            Debug.Log("Quest started successfully!");
        }
        else
        {
            Debug.Log("Quest failed to start - check prerequisites or quest limit");
        }

        // Quest objectives are tracked automatically through events
        // For example, when a fish is caught:
        Fish caughtFish = new Fish { id = "bass", name = "Bass", baseValue = 25f };
        EventSystem.Publish("FishCaught", caughtFish);
        // -> QuestManager automatically updates "CatchFish" objectives

        // Check if quest can be completed
        Quest quest = QuestManager.Instance.GetActiveQuests().Find(q => q.questID == "tutorial_first_catch");
        if (quest != null && quest.AreAllObjectivesComplete())
        {
            QuestManager.Instance.CompleteQuest(quest);
            // -> Rewards automatically granted
            // -> Quest moved to completed list
            // -> Events published for UI updates
        }
    }

    // ============================================================
    // EXAMPLE 2: NPC Interaction Flow
    // ============================================================

    public void Example_NPCInteraction()
    {
        // Player approaches NPC (handled by NPCController automatically)
        // When player presses interact key:

        // 1. NPCController.Interact() is called
        // 2. NPC publishes dialogue event
        // This is handled automatically, but here's what happens internally:

        // Example of what NPCController does:
        var dialogueData = new NPCDialogueData
        {
            npcID = "old_fisher",
            npcName = "Old Fisher Tom",
            dialogue = "Welcome to the waters, newcomer!",
            availableQuests = new string[] { "tutorial_first_catch" }
        };

        EventSystem.Publish("NPCDialogueStarted", dialogueData);

        // -> DialogueSystem picks this up
        // -> UI displays dialogue
        // -> Player can accept quests
    }

    // ============================================================
    // EXAMPLE 3: Story Progression Tracking
    // ============================================================

    public void Example_StoryProgression()
    {
        // Check current story act
        StoryAct currentAct = StoryProgression.Instance.GetCurrentAct();
        Debug.Log($"Current story act: {currentAct}");

        // Story progresses automatically based on discoveries
        // Example: Player finds a message in a bottle
        EventSystem.Publish("MessageInBottleFound", "message_1");
        // -> StoryProgression tracks this
        // -> After 1st message, advances to Act 2

        // Check if player has met key story milestones
        bool metGhost = StoryProgression.Instance.HasMetGhostFisher();
        bool foundAllAltars = StoryProgression.Instance.HasFoundAllAltars();

        // Choose an ending (late game)
        if (StoryProgression.Instance.GetCurrentAct() >= StoryAct.Act4_Truth)
        {
            // Player chooses good ending
            StoryProgression.Instance.ChooseSealEntityEnding();
            // -> Night horror disabled
            // -> Journal entries unlocked
            // -> Story ends
        }
    }

    // ============================================================
    // EXAMPLE 4: Journal Entry Management
    // ============================================================

    public void Example_JournalEntries()
    {
        // Unlock a specific journal entry
        JournalSystem.Instance.UnlockEntry("lore_entity_origin");

        // Journal entries auto-unlock based on events:
        Fish rareFish = new Fish { id = "aberrant_bass", isAberrant = true };
        EventSystem.Publish("FishCaught", rareFish);
        // -> Unlocks "fish_aberrant_bass" entry
        // -> Unlocks "aberrant_discovery" entry (first aberrant)

        // Get all unlocked entries in a category
        List<JournalEntry> fishEntries = JournalSystem.Instance.GetEntriesByCategory(JournalCategory.Fish);
        Debug.Log($"Discovered {fishEntries.Count} fish species");

        // Check journal completion
        float overallCompletion = JournalSystem.Instance.GetOverallCompletion();
        Debug.Log($"Journal {overallCompletion * 100f:F1}% complete");

        // Get category-specific completion
        var (unlockedFish, totalFish) = JournalSystem.Instance.GetCategoryProgress(JournalCategory.Fish);
        Debug.Log($"Fish Bestiary: {unlockedFish}/{totalFish}");
    }

    // ============================================================
    // EXAMPLE 5: Environmental Clue Discovery
    // ============================================================

    public void Example_EnvironmentalClues()
    {
        // Environmental clues spawn automatically based on:
        // - Current location
        // - Time of day
        // - Player sanity
        // - Story progression

        // When player interacts with a clue object:
        // ClueInteraction.Interact() is called automatically

        // This is what happens internally:
        var clue = new EnvironmentalClue
        {
            clueID = "message_bottle_1",
            clueName = "Weathered Bottle",
            clueType = ClueType.MessageInBottle,
            clueText = "Day 1: I've found the most incredible fishing spot...",
            journalEntriesToUnlock = new List<string> { "message_1" },
            relicReward = 1
        };

        EnvironmentalClues.Instance.DiscoverClue(clue);
        // -> Clue marked as discovered
        // -> Journal entries unlocked
        // -> Rewards granted
        // -> Events published
        // -> UI notification shown

        // Check how many clues discovered
        int cluesFound = EnvironmentalClues.Instance.GetDiscoveredClueCount();
        int totalClues = EnvironmentalClues.Instance.GetTotalClueCount();
        Debug.Log($"Clues: {cluesFound}/{totalClues}");
    }

    // ============================================================
    // EXAMPLE 6: Quest Objective Tracking (Manual)
    // ============================================================

    public void Example_ManualQuestObjectiveUpdate()
    {
        // Most objectives track automatically, but you can manually update if needed

        // Example: Player visits a location
        QuestManager.Instance.UpdateObjective(
            ObjectiveType.VisitLocation,
            targetID: "deep_ocean",
            incrementCount: 1
        );

        // Example: Player spends money
        QuestManager.Instance.UpdateObjective(
            ObjectiveType.SpendMoney,
            targetID: "",
            incrementCount: 0,
            incrementValue: 150f // Spent $150
        );

        // Example: Player completes a minigame
        QuestManager.Instance.UpdateObjective(
            ObjectiveType.CompleteMinigame,
            targetID: "harpoon",
            incrementCount: 1
        );
    }

    // ============================================================
    // EXAMPLE 7: Listening to Narrative Events
    // ============================================================

    private void Start()
    {
        // Subscribe to quest events
        EventSystem.Subscribe<Quest>("QuestStarted", OnQuestStarted);
        EventSystem.Subscribe<Quest>("QuestCompleted", OnQuestCompleted);
        EventSystem.Subscribe<QuestObjective>("QuestObjectiveCompleted", OnObjectiveCompleted);

        // Subscribe to story events
        EventSystem.Subscribe<StoryAct>("StoryActChanged", OnStoryActChanged);
        EventSystem.Subscribe<StoryEnding>("EndingChosen", OnEndingChosen);

        // Subscribe to NPC events
        EventSystem.Subscribe<NPCDialogueData>("NPCDialogueStarted", OnNPCDialogue);
        EventSystem.Subscribe<string>("NPCFirstMet", OnNPCFirstMet);

        // Subscribe to discovery events
        EventSystem.Subscribe<string>("ClueDiscovered", OnClueDiscovered);
        EventSystem.Subscribe<string>("MessageInBottleFound", OnMessageFound);
        EventSystem.Subscribe<JournalEntry>("JournalEntryUnlocked", OnJournalEntryUnlocked);
    }

    private void OnDestroy()
    {
        // CRITICAL: Always unsubscribe!
        EventSystem.Unsubscribe<Quest>("QuestStarted", OnQuestStarted);
        EventSystem.Unsubscribe<Quest>("QuestCompleted", OnQuestCompleted);
        EventSystem.Unsubscribe<QuestObjective>("QuestObjectiveCompleted", OnObjectiveCompleted);
        EventSystem.Unsubscribe<StoryAct>("StoryActChanged", OnStoryActChanged);
        EventSystem.Unsubscribe<StoryEnding>("EndingChosen", OnEndingChosen);
        EventSystem.Unsubscribe<NPCDialogueData>("NPCDialogueStarted", OnNPCDialogue);
        EventSystem.Unsubscribe<string>("NPCFirstMet", OnNPCFirstMet);
        EventSystem.Unsubscribe<string>("ClueDiscovered", OnClueDiscovered);
        EventSystem.Unsubscribe<string>("MessageInBottleFound", OnMessageFound);
        EventSystem.Unsubscribe<JournalEntry>("JournalEntryUnlocked", OnJournalEntryUnlocked);
    }

    // Event handlers
    private void OnQuestStarted(Quest quest)
    {
        Debug.Log($"New quest started: {quest.title}");
        // Update UI, play sound, show notification, etc.
    }

    private void OnQuestCompleted(Quest quest)
    {
        Debug.Log($"Quest completed: {quest.title}");
        Debug.Log($"Rewards: {quest.reward.GetRewardSummary()}");
        // Update UI, play fanfare, grant rewards, etc.
    }

    private void OnObjectiveCompleted(QuestObjective objective)
    {
        Debug.Log($"Objective complete: {objective.description}");
        // Update UI, play sound, etc.
    }

    private void OnStoryActChanged(StoryAct newAct)
    {
        Debug.Log($"Story progressed to: {newAct}");
        // Update UI, trigger cutscene, unlock content, etc.
    }

    private void OnEndingChosen(StoryEnding ending)
    {
        Debug.Log($"Player chose ending: {ending}");
        // Trigger ending sequence, credits, etc.
    }

    private void OnNPCDialogue(NPCDialogueData data)
    {
        Debug.Log($"{data.npcName}: {data.dialogue}");
        // Display dialogue UI
    }

    private void OnNPCFirstMet(string npcID)
    {
        Debug.Log($"First meeting with: {npcID}");
        // Unlock journal entry, show intro, etc.
    }

    private void OnClueDiscovered(string clueID)
    {
        Debug.Log($"Clue discovered: {clueID}");
        // Show discovery UI, play sound, etc.
    }

    private void OnMessageFound(string messageID)
    {
        Debug.Log($"Message in bottle found: {messageID}");
        // Show message UI, track progress, etc.
    }

    private void OnJournalEntryUnlocked(JournalEntry entry)
    {
        Debug.Log($"Journal entry unlocked: {entry.title}");
        // Show notification, update journal UI, etc.
    }

    // ============================================================
    // EXAMPLE 8: Publishing Events for Quest Tracking
    // ============================================================

    public void Example_PublishingGameplayEvents()
    {
        // When your system does something quest-relevant, publish an event

        // Fish caught (from Fishing System)
        Fish fish = new Fish { id = "bass", baseValue = 25f };
        EventSystem.Publish("FishCaught", fish);

        // Fish sold (from Economy System)
        EventSystem.Publish("FishSold", fish);

        // Location visited (from World System)
        EventSystem.Publish("LocationVisited", "deep_ocean");

        // Location unlocked (from Progression System)
        EventSystem.Publish("LocationUnlocked", "underground_cavern");

        // NPC talked to (from Dialogue System)
        EventSystem.Publish("NPCTalkedTo", "old_fisher");

        // Item collected (from Inventory System)
        EventSystem.Publish("ItemCollected", "ancient_relic");

        // Dark ability used (from Horror System)
        EventSystem.Publish("DarkAbilityUsed", "abyssal_sprint");

        // Night survived (from Time System)
        EventSystem.Publish("NightSurvived");

        // Item dredged (from Fishing System)
        EventSystem.Publish("ItemDredged", "sunken_treasure");

        // Photo taken (from Photography System)
        EventSystem.Publish("PhotoTaken", "legendary_fish");

        // Recipe cooked (from Cooking System)
        EventSystem.Publish("RecipeCooked", "fish_stew");
    }

    // ============================================================
    // EXAMPLE 9: Checking Quest State
    // ============================================================

    public void Example_QueryQuestState()
    {
        // Check if quest is active
        bool isActive = QuestManager.Instance.IsQuestActive("tutorial_first_catch");

        // Check if quest is completed
        bool isCompleted = QuestManager.Instance.IsQuestCompleted("tutorial_first_catch");

        // Get all active quests
        List<Quest> activeQuests = QuestManager.Instance.GetActiveQuests();
        Debug.Log($"You have {activeQuests.Count} active quests");

        // Get all completed quests
        List<string> completedQuests = QuestManager.Instance.GetCompletedQuestIDs();
        Debug.Log($"You've completed {completedQuests.Count} quests");

        // Get available quests (can be started)
        List<Quest> availableQuests = QuestManager.Instance.GetAvailableQuests();
        Debug.Log($"{availableQuests.Count} quests are available to start");

        // Get current story act
        int storyAct = QuestManager.Instance.GetCurrentStoryAct();
        Debug.Log($"Current story act: {storyAct}");
    }

    // ============================================================
    // EXAMPLE 10: NPC Relationships (Future Feature)
    // ============================================================

    public void Example_NPCRelationships()
    {
        // Modify NPC relationship (called by quest completion, dialogue choices, etc.)
        // This would be handled by NPCController, shown here for reference

        // Example: NPC relationship increases after quest
        EventSystem.Publish("NPCRelationshipChanged", new { npcID = "old_fisher", relationship = 50 });

        // Example: Negative relationship from dialogue choice
        EventSystem.Publish("NPCRelationshipChanged", new { npcID = "captain", relationship = -10 });
    }
}

/// <summary>
/// Example of a fishing system publishing events for quest tracking
/// </summary>
public class FishingSystemExample : MonoBehaviour
{
    public void OnFishCaught(Fish caughtFish)
    {
        // Add fish to inventory
        // ... inventory code ...

        // Publish event for quest tracking
        EventSystem.Publish("FishCaught", caughtFish);
        // -> QuestManager automatically tracks this for all relevant objectives
    }

    public void OnFishSold(Fish soldFish)
    {
        // Add money to player
        // ... economy code ...

        // Publish event for quest tracking
        EventSystem.Publish("FishSold", soldFish);
        // -> Tracks "SellFish" objectives
    }
}

/// <summary>
/// Example of a location system publishing events
/// </summary>
public class LocationSystemExample : MonoBehaviour
{
    public void OnPlayerEnterLocation(string locationID)
    {
        // Load location assets
        // ... loading code ...

        // Publish event for quest tracking
        EventSystem.Publish("LocationVisited", locationID);
        // -> Tracks "VisitLocation" objectives
        // -> Unlocks journal entry for location
    }

    public void OnLocationUnlocked(string locationID)
    {
        // Mark location as accessible
        // ... unlock code ...

        // Publish event
        EventSystem.Publish("LocationUnlocked", locationID);
        // -> Tracks "UnlockLocation" objectives
    }
}

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Central quest management system.
/// Tracks active quests, completed quests, and quest progression.
/// Integrates with SaveSystem and other game systems.
/// </summary>
public class QuestManager : MonoBehaviour
{
    private static QuestManager _instance;
    public static QuestManager Instance => _instance;

    [Header("Quest Settings")]
    [SerializeField] private int maxActiveQuests = 5;
    [SerializeField] private List<Quest> allQuests = new List<Quest>(); // All available quests in game

    [Header("Quest State")]
    private List<Quest> activeQuests = new List<Quest>();
    private List<string> completedQuestIDs = new List<string>();
    private int currentStoryProgression = 0; // 0 = not started, 1-5 = acts

    [Header("Debug")]
    [SerializeField] private bool debugMode = false;

    private void Awake()
    {
        // Singleton pattern
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
        // Subscribe to game events
        EventSystem.Subscribe<SaveData>("GatheringSaveData", OnGatheringSave);
        EventSystem.Subscribe<SaveData>("ApplyingSaveData", OnApplyingSave);
        EventSystem.Subscribe("GameInitialized", OnGameInitialized);

        // Subscribe to gameplay events for quest objectives
        SubscribeToObjectiveEvents();
    }

    private void OnDestroy()
    {
        // Unsubscribe from all events
        EventSystem.Unsubscribe<SaveData>("GatheringSaveData", OnGatheringSave);
        EventSystem.Unsubscribe<SaveData>("ApplyingSaveData", OnApplyingSave);
        EventSystem.Unsubscribe("GameInitialized", OnGameInitialized);
        UnsubscribeFromObjectiveEvents();
    }

    private void OnGameInitialized()
    {
        Debug.Log("QuestManager initialized");
    }

    #region Quest Management

    /// <summary>
    /// Start a new quest
    /// </summary>
    public bool StartQuest(string questID)
    {
        Quest quest = FindQuestByID(questID);
        if (quest == null)
        {
            Debug.LogWarning($"Quest not found: {questID}");
            return false;
        }

        return StartQuest(quest);
    }

    /// <summary>
    /// Start a new quest
    /// </summary>
    public bool StartQuest(Quest quest)
    {
        // Validation checks
        if (quest == null)
        {
            Debug.LogWarning("Cannot start null quest");
            return false;
        }

        if (activeQuests.Count >= maxActiveQuests)
        {
            Debug.LogWarning("Too many active quests!");
            EventSystem.Publish("QuestLimitReached", maxActiveQuests);
            return false;
        }

        if (activeQuests.Contains(quest))
        {
            Debug.LogWarning($"Quest already active: {quest.title}");
            return false;
        }

        if (completedQuestIDs.Contains(quest.questID) && !quest.isRepeatable)
        {
            Debug.LogWarning($"Quest already completed: {quest.title}");
            return false;
        }

        if (!quest.CanStart(completedQuestIDs, currentStoryProgression))
        {
            Debug.LogWarning($"Prerequisites not met for quest: {quest.title}");
            EventSystem.Publish("QuestPrerequisitesNotMet", quest);
            return false;
        }

        // Start the quest
        quest.Reset(); // Reset in case it's repeatable
        quest.isActive = true;
        activeQuests.Add(quest);

        Debug.Log($"Quest started: {quest.title}");
        EventSystem.Publish("QuestStarted", quest);

        return true;
    }

    /// <summary>
    /// Complete a quest and grant rewards
    /// </summary>
    public bool CompleteQuest(string questID)
    {
        Quest quest = activeQuests.FirstOrDefault(q => q.questID == questID);
        if (quest == null)
        {
            Debug.LogWarning($"Cannot complete inactive quest: {questID}");
            return false;
        }

        return CompleteQuest(quest);
    }

    /// <summary>
    /// Complete a quest and grant rewards
    /// </summary>
    public bool CompleteQuest(Quest quest)
    {
        if (quest == null || !activeQuests.Contains(quest))
        {
            Debug.LogWarning("Cannot complete quest - not active");
            return false;
        }

        if (!quest.AreAllObjectivesComplete())
        {
            Debug.LogWarning($"Cannot complete quest - objectives incomplete: {quest.title}");
            return false;
        }

        // Mark as complete
        quest.isCompleted = true;
        quest.isActive = false;
        activeQuests.Remove(quest);

        if (!completedQuestIDs.Contains(quest.questID))
        {
            completedQuestIDs.Add(quest.questID);
        }

        // Grant rewards
        GrantRewards(quest.reward);

        Debug.Log($"Quest completed: {quest.title}");
        EventSystem.Publish("QuestCompleted", quest);

        return true;
    }

    /// <summary>
    /// Fail a quest (removes from active quests)
    /// </summary>
    public void FailQuest(string questID)
    {
        Quest quest = activeQuests.FirstOrDefault(q => q.questID == questID);
        if (quest != null)
        {
            FailQuest(quest);
        }
    }

    /// <summary>
    /// Fail a quest (removes from active quests)
    /// </summary>
    public void FailQuest(Quest quest)
    {
        if (quest == null || !activeQuests.Contains(quest))
            return;

        quest.isFailed = true;
        quest.isActive = false;
        activeQuests.Remove(quest);

        Debug.Log($"Quest failed: {quest.title}");
        EventSystem.Publish("QuestFailed", quest);
    }

    /// <summary>
    /// Abandon an active quest
    /// </summary>
    public void AbandonQuest(string questID)
    {
        Quest quest = activeQuests.FirstOrDefault(q => q.questID == questID);
        if (quest != null)
        {
            quest.isActive = false;
            activeQuests.Remove(quest);
            Debug.Log($"Quest abandoned: {quest.title}");
            EventSystem.Publish("QuestAbandoned", quest);
        }
    }

    #endregion

    #region Quest Objectives

    /// <summary>
    /// Update objective progress for active quests
    /// </summary>
    public void UpdateObjective(ObjectiveType objectiveType, string targetID = "", int incrementCount = 1, float incrementValue = 0f)
    {
        foreach (var quest in activeQuests)
        {
            foreach (var objective in quest.objectives)
            {
                if (objective.objectiveType == objectiveType &&
                    objective.MatchesTarget(targetID) &&
                    !objective.isComplete)
                {
                    objective.UpdateProgress(incrementCount, incrementValue);

                    if (objective.isComplete)
                    {
                        Debug.Log($"Objective completed: {objective.description}");
                        EventSystem.Publish("QuestObjectiveCompleted", objective);
                    }

                    // Auto-complete quest if all objectives are done
                    if (quest.autoComplete && quest.AreAllObjectivesComplete())
                    {
                        CompleteQuest(quest);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Subscribe to gameplay events for automatic objective tracking
    /// </summary>
    private void SubscribeToObjectiveEvents()
    {
        EventSystem.Subscribe<Fish>("FishCaught", OnFishCaught);
        EventSystem.Subscribe<Fish>("FishSold", OnFishSold);
        EventSystem.Subscribe<string>("LocationVisited", OnLocationVisited);
        EventSystem.Subscribe<string>("LocationUnlocked", OnLocationUnlocked);
        EventSystem.Subscribe<string>("NPCTalkedTo", OnNPCTalkedTo);
        EventSystem.Subscribe<string>("ItemCollected", OnItemCollected);
        EventSystem.Subscribe<string>("DarkAbilityUsed", OnDarkAbilityUsed);
        EventSystem.Subscribe("NightSurvived", OnNightSurvived);
        EventSystem.Subscribe<string>("ItemDredged", OnItemDredged);
        EventSystem.Subscribe<string>("PhotoTaken", OnPhotoTaken);
        EventSystem.Subscribe<string>("RecipeCooked", OnRecipeCooked);
        EventSystem.Subscribe<string>("ClueDiscovered", OnClueDiscovered);
    }

    private void UnsubscribeFromObjectiveEvents()
    {
        EventSystem.Unsubscribe<Fish>("FishCaught", OnFishCaught);
        EventSystem.Unsubscribe<Fish>("FishSold", OnFishSold);
        EventSystem.Unsubscribe<string>("LocationVisited", OnLocationVisited);
        EventSystem.Unsubscribe<string>("LocationUnlocked", OnLocationUnlocked);
        EventSystem.Unsubscribe<string>("NPCTalkedTo", OnNPCTalkedTo);
        EventSystem.Unsubscribe<string>("ItemCollected", OnItemCollected);
        EventSystem.Unsubscribe<string>("DarkAbilityUsed", OnDarkAbilityUsed);
        EventSystem.Unsubscribe("NightSurvived", OnNightSurvived);
        EventSystem.Unsubscribe<string>("ItemDredged", OnItemDredged);
        EventSystem.Unsubscribe<string>("PhotoTaken", OnPhotoTaken);
        EventSystem.Unsubscribe<string>("RecipeCooked", OnRecipeCooked);
        EventSystem.Unsubscribe<string>("ClueDiscovered", OnClueDiscovered);
    }

    // Event handlers for objective tracking
    private void OnFishCaught(Fish fish)
    {
        UpdateObjective(ObjectiveType.CatchFish, fish.id, 1, fish.baseValue);
        UpdateObjective(ObjectiveType.CatchAnyFish, "", 1, fish.baseValue);
        UpdateObjective(ObjectiveType.CatchRarity, fish.rarity.ToString(), 1);
    }

    private void OnFishSold(Fish fish)
    {
        UpdateObjective(ObjectiveType.SellFish, "", 0, fish.baseValue);
    }

    private void OnLocationVisited(string locationID)
    {
        UpdateObjective(ObjectiveType.VisitLocation, locationID, 1);
    }

    private void OnLocationUnlocked(string locationID)
    {
        UpdateObjective(ObjectiveType.UnlockLocation, locationID, 1);
    }

    private void OnNPCTalkedTo(string npcID)
    {
        UpdateObjective(ObjectiveType.TalkToNPC, npcID, 1);
    }

    private void OnItemCollected(string itemID)
    {
        UpdateObjective(ObjectiveType.CollectItem, itemID, 1);
    }

    private void OnDarkAbilityUsed(string abilityID)
    {
        UpdateObjective(ObjectiveType.UseDarkAbility, abilityID, 1);
    }

    private void OnNightSurvived()
    {
        UpdateObjective(ObjectiveType.SurviveNight, "", 1);
    }

    private void OnItemDredged(string itemID)
    {
        UpdateObjective(ObjectiveType.DredgeItem, itemID, 1);
    }

    private void OnPhotoTaken(string subjectID)
    {
        UpdateObjective(ObjectiveType.TakePhoto, subjectID, 1);
    }

    private void OnRecipeCooked(string recipeID)
    {
        UpdateObjective(ObjectiveType.CookRecipe, recipeID, 1);
    }

    private void OnClueDiscovered(string clueID)
    {
        UpdateObjective(ObjectiveType.FindClue, clueID, 1);
    }

    #endregion

    #region Rewards

    /// <summary>
    /// Grant quest rewards to player
    /// </summary>
    private void GrantRewards(QuestReward reward)
    {
        if (reward == null)
            return;

        // Grant currency
        if (reward.money > 0f)
            EventSystem.Publish("AddMoney", reward.money);
        if (reward.scrap > 0f)
            EventSystem.Publish("AddScrap", reward.scrap);
        if (reward.relics > 0)
            EventSystem.Publish("AddRelics", reward.relics);

        // Grant items
        for (int i = 0; i < reward.itemIDs.Count; i++)
        {
            string itemID = reward.itemIDs[i];
            int quantity = i < reward.itemQuantities.Count ? reward.itemQuantities[i] : 1;
            EventSystem.Publish("AddItem", new { itemID, quantity });
        }

        // Unlock locations
        foreach (var locationID in reward.unlockedLocations)
        {
            EventSystem.Publish("UnlockLocation", locationID);
        }

        // Unlock dark abilities
        foreach (var abilityID in reward.unlockedAbilities)
        {
            EventSystem.Publish("UnlockAbility", abilityID);
        }

        // Story progression
        if (reward.storyProgressionIncrease > 0)
        {
            AdvanceStoryProgression(reward.storyProgressionIncrease);
        }

        // Unlock journal entries
        foreach (var entryID in reward.unlockedJournalEntries)
        {
            EventSystem.Publish("UnlockJournalEntry", entryID);
        }

        Debug.Log($"Rewards granted: {reward.GetRewardSummary()}");
    }

    #endregion

    #region Story Progression

    /// <summary>
    /// Advance the main story progression
    /// </summary>
    public void AdvanceStoryProgression(int amount = 1)
    {
        int oldProgression = currentStoryProgression;
        currentStoryProgression = Mathf.Clamp(currentStoryProgression + amount, 0, 5);

        if (currentStoryProgression != oldProgression)
        {
            Debug.Log($"Story progressed: Act {currentStoryProgression}");
            EventSystem.Publish("StoryProgressionChanged", currentStoryProgression);
        }
    }

    /// <summary>
    /// Get current story act (0-5)
    /// </summary>
    public int GetCurrentStoryAct()
    {
        return currentStoryProgression;
    }

    #endregion

    #region Query Methods

    /// <summary>
    /// Get all active quests
    /// </summary>
    public List<Quest> GetActiveQuests()
    {
        return new List<Quest>(activeQuests);
    }

    /// <summary>
    /// Get all completed quest IDs
    /// </summary>
    public List<string> GetCompletedQuestIDs()
    {
        return new List<string>(completedQuestIDs);
    }

    /// <summary>
    /// Check if a quest is completed
    /// </summary>
    public bool IsQuestCompleted(string questID)
    {
        return completedQuestIDs.Contains(questID);
    }

    /// <summary>
    /// Check if a quest is active
    /// </summary>
    public bool IsQuestActive(string questID)
    {
        return activeQuests.Any(q => q.questID == questID);
    }

    /// <summary>
    /// Find quest by ID
    /// </summary>
    private Quest FindQuestByID(string questID)
    {
        return allQuests.FirstOrDefault(q => q.questID == questID);
    }

    /// <summary>
    /// Get all available quests that can be started
    /// </summary>
    public List<Quest> GetAvailableQuests()
    {
        return allQuests.Where(q =>
            !IsQuestActive(q.questID) &&
            !IsQuestCompleted(q.questID) &&
            q.CanStart(completedQuestIDs, currentStoryProgression) &&
            !q.isHidden
        ).ToList();
    }

    #endregion

    #region Save/Load Integration

    private void OnGatheringSave(SaveData data)
    {
        // Save active quest IDs
        data.activeQuests.Clear();
        foreach (var quest in activeQuests)
        {
            data.activeQuests.Add(quest.questID);
        }

        // Save completed quest IDs
        data.completedQuests.Clear();
        data.completedQuests.AddRange(completedQuestIDs);

        // Save quest objective progress
        data.questProgress.Clear();
        foreach (var quest in activeQuests)
        {
            for (int i = 0; i < quest.objectives.Count; i++)
            {
                var objective = quest.objectives[i];
                string progressKey = $"{quest.questID}_{i}";
                data.questProgress[progressKey] = objective.currentCount;
            }
        }

        if (debugMode)
            Debug.Log($"Saved {activeQuests.Count} active quests, {completedQuestIDs.Count} completed quests");
    }

    private void OnApplyingSave(SaveData data)
    {
        // Restore completed quests
        completedQuestIDs.Clear();
        completedQuestIDs.AddRange(data.completedQuests);

        // Restore active quests
        activeQuests.Clear();
        foreach (var questID in data.activeQuests)
        {
            Quest quest = FindQuestByID(questID);
            if (quest != null)
            {
                quest.isActive = true;
                activeQuests.Add(quest);

                // Restore objective progress
                for (int i = 0; i < quest.objectives.Count; i++)
                {
                    string progressKey = $"{questID}_{i}";
                    if (data.questProgress.ContainsKey(progressKey))
                    {
                        quest.objectives[i].currentCount = data.questProgress[progressKey];
                        quest.objectives[i].CheckCompletion();
                    }
                }
            }
        }

        if (debugMode)
            Debug.Log($"Loaded {activeQuests.Count} active quests, {completedQuestIDs.Count} completed quests");
    }

    #endregion

    #region Debug

    /// <summary>
    /// Debug: Complete all objectives for a quest
    /// </summary>
    public void DebugCompleteAllObjectives(string questID)
    {
        Quest quest = activeQuests.FirstOrDefault(q => q.questID == questID);
        if (quest != null)
        {
            foreach (var objective in quest.objectives)
            {
                objective.SetProgress(objective.targetCount, objective.targetValue);
            }
            Debug.Log($"DEBUG: Completed all objectives for {quest.title}");
        }
    }

    #endregion
}

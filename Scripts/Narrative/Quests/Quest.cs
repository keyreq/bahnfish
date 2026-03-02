using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Quest types in Bahnfish
/// </summary>
public enum QuestType
{
    Tutorial,   // Starter quests
    Story,      // Main mystery storyline
    Fetch,      // Catch/collect specific items
    Side,       // Optional challenges
    Repeatable  // Daily/weekly quests
}

/// <summary>
/// Represents a quest with objectives, rewards, and prerequisites.
/// </summary>
[System.Serializable]
public class Quest
{
    [Header("Quest Identity")]
    public string questID;
    public string title;
    [TextArea(3, 6)]
    public string description;
    public QuestType questType;

    [Header("Quest Progression")]
    public List<QuestObjective> objectives = new List<QuestObjective>();
    public QuestReward reward;
    public List<string> prerequisiteQuestIDs = new List<string>(); // Must complete these first

    [Header("Quest Behavior")]
    public bool isRepeatable = false;
    public bool isHidden = false; // Discovered through exploration
    public bool autoComplete = false; // Completes when all objectives done

    [Header("Quest Availability")]
    public string questGiverNPCID; // NPC who gives this quest
    public TimeOfDay availableTimeOfDay = TimeOfDay.Day; // When quest can be started
    public int storyProgressionRequired = 0; // Story act requirement (0-5)

    [Header("Quest Flavor")]
    [TextArea(2, 4)]
    public string startDialogue;
    [TextArea(2, 4)]
    public string completeDialogue;
    [TextArea(2, 4)]
    public string failDialogue;

    // Runtime data (not serialized)
    [System.NonSerialized]
    public bool isActive = false;
    [System.NonSerialized]
    public bool isCompleted = false;
    [System.NonSerialized]
    public bool isFailed = false;

    /// <summary>
    /// Check if all objectives are complete
    /// </summary>
    public bool AreAllObjectivesComplete()
    {
        foreach (var objective in objectives)
        {
            if (!objective.isComplete)
                return false;
        }
        return objectives.Count > 0; // At least one objective must exist
    }

    /// <summary>
    /// Get overall quest progress (0-1)
    /// </summary>
    public float GetProgress()
    {
        if (objectives.Count == 0)
            return 0f;

        float totalProgress = 0f;
        foreach (var objective in objectives)
        {
            totalProgress += objective.GetProgress();
        }
        return totalProgress / objectives.Count;
    }

    /// <summary>
    /// Check if quest can be started based on prerequisites
    /// </summary>
    public bool CanStart(List<string> completedQuestIDs, int currentStoryProgression)
    {
        // Check story progression requirement
        if (currentStoryProgression < storyProgressionRequired)
            return false;

        // Check prerequisites
        foreach (var prereqID in prerequisiteQuestIDs)
        {
            if (!completedQuestIDs.Contains(prereqID))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Reset quest state (for repeatable quests)
    /// </summary>
    public void Reset()
    {
        isActive = false;
        isCompleted = false;
        isFailed = false;

        foreach (var objective in objectives)
        {
            objective.Reset();
        }
    }
}

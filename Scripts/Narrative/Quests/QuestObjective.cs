using UnityEngine;

/// <summary>
/// Types of quest objectives
/// </summary>
public enum ObjectiveType
{
    CatchFish,          // Catch X fish of species Y
    CatchAnyFish,       // Catch X fish of any type
    CatchRarity,        // Catch X fish of rarity Y
    VisitLocation,      // Travel to location X
    TalkToNPC,          // Speak with NPC X
    CollectItem,        // Find/collect item X
    SellFish,           // Sell fish worth $X
    SpendMoney,         // Purchase items worth $X
    SurviveNight,       // Survive X nights at sea
    ReachSanity,        // Reach sanity level X (can be low or high)
    UnlockLocation,     // Unlock location X
    UpgradeItem,        // Upgrade item X to level Y
    UseDarkAbility,     // Use dark ability X times
    DredgeItem,         // Dredge up X items
    CompleteMinigame,   // Complete fishing minigame X times
    TakePhoto,          // Take photo of X
    BreedFish,          // Breed X fish
    CookRecipe,         // Cook recipe X times
    FindClue           // Discover environmental clue X
}

/// <summary>
/// Individual quest objective that tracks progress toward completion.
/// </summary>
[System.Serializable]
public class QuestObjective
{
    [Header("Objective Definition")]
    public string objectiveID;
    [TextArea(2, 3)]
    public string description;
    public ObjectiveType objectiveType;

    [Header("Objective Target")]
    public string targetID; // Fish species, NPC ID, location ID, etc.
    public int targetCount = 1; // How many times to complete
    public float targetValue = 0f; // For money-based or numeric objectives

    [Header("Objective Progress")]
    public int currentCount = 0;
    public float currentValue = 0f;
    public bool isComplete = false;
    public bool isOptional = false; // Quest can be completed without this

    [Header("Visibility")]
    public bool showInJournal = true;
    public bool trackOnHUD = true;

    /// <summary>
    /// Update objective progress
    /// </summary>
    public void UpdateProgress(int incrementCount = 1, float incrementValue = 0f)
    {
        if (isComplete)
            return;

        currentCount += incrementCount;
        currentValue += incrementValue;

        CheckCompletion();
    }

    /// <summary>
    /// Set objective progress directly
    /// </summary>
    public void SetProgress(int count, float value)
    {
        if (isComplete)
            return;

        currentCount = count;
        currentValue = value;

        CheckCompletion();
    }

    /// <summary>
    /// Check if objective is complete and update status
    /// </summary>
    private void CheckCompletion()
    {
        bool countMet = currentCount >= targetCount;
        bool valueMet = currentValue >= targetValue || targetValue == 0f;

        if (countMet && valueMet)
        {
            isComplete = true;
            Debug.Log($"Objective completed: {description}");
        }
    }

    /// <summary>
    /// Get progress as 0-1 value
    /// </summary>
    public float GetProgress()
    {
        if (isComplete)
            return 1f;

        if (targetCount > 0 && targetValue > 0f)
        {
            // Both count and value matter, use average
            float countProgress = Mathf.Clamp01((float)currentCount / targetCount);
            float valueProgress = Mathf.Clamp01(currentValue / targetValue);
            return (countProgress + valueProgress) / 2f;
        }
        else if (targetCount > 0)
        {
            return Mathf.Clamp01((float)currentCount / targetCount);
        }
        else if (targetValue > 0f)
        {
            return Mathf.Clamp01(currentValue / targetValue);
        }

        return 0f;
    }

    /// <summary>
    /// Get progress text for UI display
    /// </summary>
    public string GetProgressText()
    {
        if (isComplete)
            return "Complete!";

        if (targetCount > 0 && targetValue > 0f)
        {
            return $"{currentCount}/{targetCount} (${currentValue:F0}/${targetValue:F0})";
        }
        else if (targetCount > 0)
        {
            return $"{currentCount}/{targetCount}";
        }
        else if (targetValue > 0f)
        {
            return $"${currentValue:F0}/${targetValue:F0}";
        }

        return "In Progress";
    }

    /// <summary>
    /// Reset objective to initial state
    /// </summary>
    public void Reset()
    {
        currentCount = 0;
        currentValue = 0f;
        isComplete = false;
    }

    /// <summary>
    /// Check if a given target matches this objective
    /// </summary>
    public bool MatchesTarget(string checkTargetID)
    {
        return targetID == checkTargetID || string.IsNullOrEmpty(targetID);
    }
}

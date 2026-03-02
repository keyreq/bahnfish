using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Rewards given upon quest completion
/// </summary>
[System.Serializable]
public class QuestReward
{
    [Header("Currency Rewards")]
    public float money = 0f;
    public float scrap = 0f;
    public int relics = 0;

    [Header("Item Rewards")]
    public List<string> itemIDs = new List<string>(); // Items to add to inventory
    public List<int> itemQuantities = new List<int>(); // Corresponding quantities

    [Header("Progression Rewards")]
    public List<string> unlockedLocations = new List<string>(); // Locations to unlock
    public List<string> unlockedAbilities = new List<string>(); // Dark abilities to unlock
    public List<string> unlockedUpgrades = new List<string>(); // Upgrades to make available
    public List<string> unlockedRecipes = new List<string>(); // Cooking recipes to unlock

    [Header("Story Rewards")]
    public int storyProgressionIncrease = 0; // Advance story act
    public List<string> unlockedJournalEntries = new List<string>(); // Lore entries
    public List<string> unlockedDialogue = new List<string>(); // New NPC dialogue options

    [Header("Reputation Rewards")]
    public Dictionary<string, int> npcRelationshipChanges = new Dictionary<string, int>(); // NPC ID -> reputation delta

    /// <summary>
    /// Check if reward has any currency
    /// </summary>
    public bool HasCurrency()
    {
        return money > 0f || scrap > 0f || relics > 0;
    }

    /// <summary>
    /// Check if reward has any items
    /// </summary>
    public bool HasItems()
    {
        return itemIDs.Count > 0;
    }

    /// <summary>
    /// Check if reward has any unlocks
    /// </summary>
    public bool HasUnlocks()
    {
        return unlockedLocations.Count > 0 ||
               unlockedAbilities.Count > 0 ||
               unlockedUpgrades.Count > 0 ||
               unlockedRecipes.Count > 0;
    }

    /// <summary>
    /// Get reward summary text for UI
    /// </summary>
    public string GetRewardSummary()
    {
        List<string> rewards = new List<string>();

        if (money > 0f)
            rewards.Add($"${money:F0}");
        if (scrap > 0f)
            rewards.Add($"{scrap:F0} Scrap");
        if (relics > 0)
            rewards.Add($"{relics} Relic{(relics > 1 ? "s" : "")}");

        if (itemIDs.Count > 0)
            rewards.Add($"{itemIDs.Count} Item{(itemIDs.Count > 1 ? "s" : "")}");

        if (unlockedLocations.Count > 0)
            rewards.Add($"Unlock {unlockedLocations.Count} Location{(unlockedLocations.Count > 1 ? "s" : "")}");

        if (unlockedAbilities.Count > 0)
            rewards.Add($"{unlockedAbilities.Count} Dark Abilit{(unlockedAbilities.Count > 1 ? "ies" : "y")}");

        if (storyProgressionIncrease > 0)
            rewards.Add("Story Progression");

        if (rewards.Count == 0)
            return "Experience";

        return string.Join(", ", rewards);
    }
}

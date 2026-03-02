using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// NPC personality traits that affect dialogue and behavior
/// </summary>
public enum NPCPersonality
{
    Friendly,       // Welcoming, helpful
    Mysterious,     // Cryptic, secretive
    Hostile,        // Unfriendly, suspicious
    Cheerful,       // Upbeat, optimistic
    Gloomy,         // Pessimistic, dark
    Wise,           // Knowledgeable, mentor-like
    Eccentric,      // Quirky, unusual
    Traumatized,    // Damaged, haunted
    Greedy,         // Profit-focused
    Spiritual       // Mystic, otherworldly
}

/// <summary>
/// NPC availability based on time of day
/// </summary>
public enum NPCAvailability
{
    Always,         // Available anytime
    DayOnly,        // Only during day
    NightOnly,      // Only at night
    DuskDawn,       // Only during transitions
    SpecialEvent    // Only during specific events
}

/// <summary>
/// Complete NPC definition including personality, dialogue, and quests.
/// </summary>
[CreateAssetMenu(fileName = "NPC_", menuName = "Bahnfish/NPC Data")]
public class NPCData : ScriptableObject
{
    [Header("NPC Identity")]
    public string npcID;
    public string npcName;
    [TextArea(3, 5)]
    public string description;
    public Sprite portrait;
    public GameObject npcPrefab;

    [Header("Personality")]
    public NPCPersonality personality;
    public NPCAvailability availability;
    [Range(0f, 1f)]
    public float friendliness = 0.5f; // How friendly they are (0 = hostile, 1 = very friendly)

    [Header("Location")]
    public string homeLocationID; // Where NPC is usually found
    public Vector3 defaultPosition; // Position in location
    public bool canMove = false; // Whether NPC moves around

    [Header("Dialogue")]
    public List<string> greetings = new List<string>(); // First interaction dialogue
    public List<string> idleDialogue = new List<string>(); // Repeated interaction dialogue
    public List<string> farewells = new List<string>(); // Leaving dialogue
    public DialogueTree dialogueTree; // Full dialogue tree

    [Header("Quests")]
    public List<string> questsToGive = new List<string>(); // Quest IDs this NPC can give

    [Header("Relationships")]
    public int baseRelationship = 0; // Starting reputation (-100 to 100)
    public List<string> relatedNPCs = new List<string>(); // NPCs with connected stories
    public bool isGhost = false; // Only appears at low sanity

    [Header("Shop (if applicable)")]
    public bool isShopkeeper = false;
    public List<string> itemsForSale = new List<string>();
    public List<float> itemPrices = new List<float>();

    [Header("Special Behavior")]
    public bool isMystic = false; // Can cleanse curses
    public bool disappearsAfterStoryAct = -1; // Disappears after story act X (-1 = never)
    public bool requiresSanityLevel = -1; // Requires sanity below X to appear (-1 = no requirement)

    /// <summary>
    /// Get random greeting dialogue
    /// </summary>
    public string GetRandomGreeting()
    {
        if (greetings.Count == 0)
            return $"Hello, I'm {npcName}.";
        return greetings[Random.Range(0, greetings.Count)];
    }

    /// <summary>
    /// Get random idle dialogue
    /// </summary>
    public string GetRandomIdleDialogue()
    {
        if (idleDialogue.Count == 0)
            return "...";
        return idleDialogue[Random.Range(0, idleDialogue.Count)];
    }

    /// <summary>
    /// Get random farewell
    /// </summary>
    public string GetRandomFarewell()
    {
        if (farewells.Count == 0)
            return "Goodbye.";
        return farewells[Random.Range(0, farewells.Count)];
    }

    /// <summary>
    /// Check if NPC is available at current time
    /// </summary>
    public bool IsAvailableAtTime(TimeOfDay currentTime)
    {
        switch (availability)
        {
            case NPCAvailability.DayOnly:
                return currentTime == TimeOfDay.Day;
            case NPCAvailability.NightOnly:
                return currentTime == TimeOfDay.Night;
            case NPCAvailability.DuskDawn:
                return currentTime == TimeOfDay.Dusk || currentTime == TimeOfDay.Dawn;
            case NPCAvailability.Always:
                return true;
            default:
                return false;
        }
    }

    /// <summary>
    /// Check if NPC should appear based on sanity
    /// </summary>
    public bool IsAvailableAtSanity(float currentSanity)
    {
        if (requiresSanityLevel < 0)
            return true;
        return currentSanity <= requiresSanityLevel;
    }
}

/// <summary>
/// Relationship data for tracking NPC friendship/reputation.
/// </summary>
[System.Serializable]
public class NPCRelationship
{
    public string npcID;
    public int relationshipLevel = 0; // -100 (hated) to 100 (beloved)
    public List<string> unlockedDialogue = new List<string>();
    public bool hasMetBefore = false;

    /// <summary>
    /// Get relationship tier as string
    /// </summary>
    public string GetRelationshipTier()
    {
        if (relationshipLevel >= 80) return "Beloved";
        if (relationshipLevel >= 50) return "Friend";
        if (relationshipLevel >= 20) return "Friendly";
        if (relationshipLevel >= -20) return "Neutral";
        if (relationshipLevel >= -50) return "Disliked";
        if (relationshipLevel >= -80) return "Hostile";
        return "Hated";
    }

    /// <summary>
    /// Modify relationship level
    /// </summary>
    public void ModifyRelationship(int amount)
    {
        relationshipLevel = Mathf.Clamp(relationshipLevel + amount, -100, 100);
    }
}

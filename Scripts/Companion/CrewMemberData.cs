using UnityEngine;

/// <summary>
/// Agent 17: Crew & Companion Specialist - CrewMemberData.cs
/// ScriptableObject defining crew member specifications, skills, and personalities.
/// </summary>
[CreateAssetMenu(fileName = "NewCrewMember", menuName = "Bahnfish/Companions/Crew Member Data")]
public class CrewMemberData : ScriptableObject
{
    [Header("Basic Information")]
    [Tooltip("Unique identifier for this crew member")]
    public string crewID;

    [Tooltip("Crew member's name")]
    public string crewName;

    [Tooltip("Crew member specialization")]
    public CrewSpecialization specialization;

    [Tooltip("Portrait/avatar sprite")]
    public Sprite portrait;

    [TextArea(3, 5)]
    [Tooltip("Backstory and personality description")]
    public string backstory;

    [Header("Personality Traits")]
    [Tooltip("Personality type (Gruff, Cheerful, Professional, etc.)")]
    public string personality;

    [Tooltip("Compatibility tags (for crew synergy)")]
    public string[] compatibilityTags;

    [Tooltip("Conflict tags (crew that don't get along)")]
    public string[] conflictTags;

    [Header("Skills")]
    [Range(1, 10)]
    [Tooltip("Skill level (1 = novice, 10 = master)")]
    public int skillLevel = 5;

    [Tooltip("Primary skill bonus description")]
    [TextArea(2, 3)]
    public string primarySkillDescription;

    [Tooltip("Primary skill bonus value")]
    public float primarySkillBonus = 20f;

    [Tooltip("Secondary skill bonus description")]
    [TextArea(2, 3)]
    public string secondarySkillDescription;

    [Tooltip("Secondary skill bonus value")]
    public float secondarySkillBonus = 10f;

    [Header("Employment")]
    [Tooltip("Daily salary in dollars")]
    public float dailySalary = 150f;

    [Tooltip("Minimum morale to avoid quitting")]
    [Range(0, 100)]
    public float minimumMorale = 10f;

    [Tooltip("Starting morale when hired")]
    [Range(0, 100)]
    public float startingMorale = 50f;

    [Header("Unlock Requirements")]
    [Tooltip("Location where crew member can be hired")]
    public string hirableAtLocation = "marina";

    [Tooltip("Minimum player level required to hire")]
    public int minimumPlayerLevel = 1;

    [Tooltip("Required quest completion to unlock")]
    public string requiredQuestID = "";

    [Tooltip("Is this crew member available from start?")]
    public bool availableFromStart = true;

    [Header("Station Assignment")]
    [Tooltip("Which station this crew member works at")]
    public CrewStation assignedStation;

    [Tooltip("Prefab for crew member at their station")]
    public GameObject stationPrefab;

    [Header("Voice & Dialogue")]
    [Tooltip("Voice lines for hiring")]
    public string[] hiringDialogue;

    [Tooltip("Voice lines for happy mood")]
    public string[] happyDialogue;

    [Tooltip("Voice lines for unhappy mood")]
    public string[] unhappyDialogue;

    [Tooltip("Voice lines for quitting")]
    public string[] quittingDialogue;

    [Header("Morale Factors")]
    [Range(-10f, 10f)]
    [Tooltip("Morale change per day with on-time payment")]
    public float paymentMoraleBonus = 5f;

    [Range(-20f, -5f)]
    [Tooltip("Morale change per day with late payment")]
    public float latePaymentMoralePenalty = -10f;

    [Range(-15f, -5f)]
    [Tooltip("Morale change when working in storm")]
    public float stormMoralePenalty = -10f;

    [Range(1f, 10f)]
    [Tooltip("Morale gain from bonus payment")]
    public float bonusMoraleGain = 15f;

    [Header("Skill Modifiers")]
    [Tooltip("Skill bonus at high morale (>70%)")]
    public float highMoraleMultiplier = 1.0f;

    [Tooltip("Skill bonus at medium morale (40-70%)")]
    public float mediumMoraleMultiplier = 0.5f;

    [Tooltip("Skill penalty at low morale (<40%)")]
    public float lowMoralePenalty = -0.1f;

    [Header("Special Abilities")]
    [Tooltip("Special crew ability description")]
    [TextArea(2, 3)]
    public string specialAbilityDescription;

    [Tooltip("Has a special once-per-day ability?")]
    public bool hasSpecialAbility = false;

    [Tooltip("Special ability cooldown (hours)")]
    public float specialAbilityCooldown = 24f;

    /// <summary>
    /// Calculates effective skill bonus based on morale level
    /// </summary>
    /// <param name="morale">Current morale percentage (0-100)</param>
    /// <returns>Effective skill multiplier</returns>
    public float GetEffectiveSkillBonus(float morale)
    {
        if (morale >= 70f)
        {
            return primarySkillBonus * highMoraleMultiplier;
        }
        else if (morale >= 40f)
        {
            return primarySkillBonus * mediumMoraleMultiplier;
        }
        else
        {
            // Low morale applies penalty
            return lowMoralePenalty * 100f; // Convert to percentage
        }
    }

    /// <summary>
    /// Checks if this crew member is compatible with another
    /// </summary>
    /// <param name="otherCrew">Other crew member to check</param>
    /// <returns>True if compatible</returns>
    public bool IsCompatibleWith(CrewMemberData otherCrew)
    {
        if (otherCrew == null) return true;

        // Check for conflicts
        foreach (string conflictTag in conflictTags)
        {
            if (otherCrew.crewID == conflictTag)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Checks if this crew member has synergy with another
    /// </summary>
    /// <param name="otherCrew">Other crew member to check</param>
    /// <returns>True if has synergy bonus</returns>
    public bool HasSynergyWith(CrewMemberData otherCrew)
    {
        if (otherCrew == null) return false;

        foreach (string compatTag in compatibilityTags)
        {
            if (otherCrew.crewID == compatTag)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Gets a random dialogue line based on mood
    /// </summary>
    /// <param name="morale">Current morale level</param>
    /// <returns>Dialogue string</returns>
    public string GetRandomDialogue(float morale)
    {
        string[] dialoguePool;

        if (morale >= 70f && happyDialogue.Length > 0)
        {
            dialoguePool = happyDialogue;
        }
        else if (morale < 40f && unhappyDialogue.Length > 0)
        {
            dialoguePool = unhappyDialogue;
        }
        else
        {
            // Default to hiring dialogue
            dialoguePool = hiringDialogue.Length > 0 ? hiringDialogue : new string[] { "..." };
        }

        if (dialoguePool.Length == 0)
        {
            return "...";
        }

        return dialoguePool[Random.Range(0, dialoguePool.Length)];
    }

    /// <summary>
    /// Validates crew data integrity
    /// </summary>
    public void OnValidate()
    {
        if (string.IsNullOrEmpty(crewID))
        {
            crewID = name.ToLower().Replace(" ", "_");
        }

        if (string.IsNullOrEmpty(crewName))
        {
            crewName = name;
        }

        // Ensure salary is reasonable
        dailySalary = Mathf.Clamp(dailySalary, 50f, 1000f);

        // Ensure skill level is valid
        skillLevel = Mathf.Clamp(skillLevel, 1, 10);
    }
}

/// <summary>
/// Crew member specializations
/// </summary>
public enum CrewSpecialization
{
    Fisherman,          // Fishing bonuses
    Navigator,          // Travel and fuel efficiency
    MaintenanceEngineer,// Durability and repair
    Cook,               // Cooking buffs
    Defender            // Protection and combat
}

/// <summary>
/// Stations where crew members work
/// </summary>
public enum CrewStation
{
    FishingDeck,        // Assists with fishing
    Helm,               // Controls navigation
    EngineRoom,         // Maintains boat
    Galley,             // Prepares food
    DeckPatrol          // Patrols and defends
}

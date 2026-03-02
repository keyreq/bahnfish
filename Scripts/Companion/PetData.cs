using UnityEngine;

/// <summary>
/// Agent 17: Crew & Companion Specialist - PetData.cs
/// ScriptableObject defining pet companion specifications, abilities, and properties.
/// Inspired by Cast n Chill's dog companion system with petting mechanics.
/// </summary>
[CreateAssetMenu(fileName = "NewPet", menuName = "Bahnfish/Companions/Pet Data")]
public class PetData : ScriptableObject
{
    [Header("Basic Information")]
    [Tooltip("Unique identifier for this pet type")]
    public string petID;

    [Tooltip("Display name of the pet")]
    public string petName;

    [Tooltip("Type of pet (dog, cat, seabird, otter, crab, ghost)")]
    public PetType petType;

    [TextArea(3, 5)]
    [Tooltip("Description of the pet")]
    public string description;

    [Header("Visual & Audio")]
    [Tooltip("Sprite/icon for UI display")]
    public Sprite icon;

    [Tooltip("Prefab for the pet in the world")]
    public GameObject petPrefab;

    [Tooltip("Pet bark/meow/chirp sounds")]
    public AudioClip[] vocalSounds;

    [Tooltip("Petting interaction sound")]
    public AudioClip pettingSound;

    [Header("Personality")]
    [Tooltip("Personality trait (Loyal, Independent, Playful, etc.)")]
    public string personality;

    [Range(1f, 10f)]
    [Tooltip("How affectionate the pet is (affects loyalty gain)")]
    public float affectionLevel = 5f;

    [Range(1f, 10f)]
    [Tooltip("How energetic the pet is (affects animations)")]
    public float energyLevel = 5f;

    [Header("Unlock Requirements")]
    [Tooltip("How the pet is unlocked")]
    public PetUnlockType unlockType;

    [Tooltip("Cost to unlock (if purchased)")]
    public float unlockCost = 0f;

    [Tooltip("Required quest ID (if quest reward)")]
    public string requiredQuestID = "";

    [Tooltip("Is this the starter pet?")]
    public bool isStarterPet = false;

    [Header("Loyalty System")]
    [Range(0f, 100f)]
    [Tooltip("Starting loyalty when acquired")]
    public float startingLoyalty = 50f;

    [Range(1f, 20f)]
    [Tooltip("Loyalty gained per petting interaction")]
    public float loyaltyPerPet = 5f;

    [Range(1f, 10f)]
    [Tooltip("Loyalty gained per feeding")]
    public float loyaltyPerFeed = 3f;

    [Range(5f, 20f)]
    [Tooltip("Loyalty gained per play session")]
    public float loyaltyPerPlay = 10f;

    [Range(0.1f, 2f)]
    [Tooltip("Daily loyalty decay if neglected")]
    public float dailyLoyaltyDecay = 1f;

    [Header("Petting Mechanics")]
    [Range(10f, 120f)]
    [Tooltip("Cooldown between petting interactions (seconds)")]
    public float pettingCooldown = 30f;

    [Tooltip("Animation to play when petted")]
    public string pettingAnimationName = "Pet_Happy";

    [Tooltip("Particle effect when petted")]
    public GameObject pettingParticles;

    [Range(1f, 10f)]
    [Tooltip("Sanity boost when petting at low sanity")]
    public float sanityBoost = 5f;

    [Header("Feeding Requirements")]
    [Tooltip("Needs feeding daily?")]
    public bool requiresFeeding = true;

    [Tooltip("Hours between feedings (real-time)")]
    public float feedingIntervalHours = 24f;

    [Tooltip("Cost to buy pet food if no fish available")]
    public float petFoodCost = 10f;

    [Header("Abilities - Passive")]
    [Tooltip("Passive ability description")]
    [TextArea(2, 3)]
    public string passiveAbilityDescription;

    [Tooltip("Passive ability multiplier (e.g., 0.8 = -20% hazard damage)")]
    public float passiveAbilityMultiplier = 1f;

    [Tooltip("Type of passive ability")]
    public PetPassiveAbility passiveAbility;

    [Header("Abilities - Active")]
    [Tooltip("Active ability description")]
    [TextArea(2, 3)]
    public string activeAbilityDescription;

    [Tooltip("Active ability cooldown (minutes)")]
    public float activeAbilityCooldown = 5f;

    [Tooltip("Type of active ability")]
    public PetActiveAbility activeAbility;

    [Header("Following Behavior")]
    [Range(1f, 10f)]
    [Tooltip("Distance pet stays from player boat")]
    public float followDistance = 5f;

    [Range(0.5f, 3f)]
    [Tooltip("Movement speed multiplier")]
    public float moveSpeed = 1f;

    [Tooltip("Idle animation when boat is stationary")]
    public string idleAnimationName = "Idle";

    [Tooltip("Swimming/moving animation")]
    public string movingAnimationName = "Swim";

    [Header("Special Features")]
    [Tooltip("Special feature description")]
    [TextArea(2, 3)]
    public string specialFeatureDescription;

    [Tooltip("Can pet fetch thrown items?")]
    public bool canFetch = false;

    [Tooltip("Can pet detect secrets/aberrations?")]
    public bool canDetectSecrets = false;

    [Tooltip("Can pet dive for items?")]
    public bool canDive = false;

    [Tooltip("Extra inventory space provided")]
    public int bonusInventorySpace = 0;

    [Header("High Loyalty Bonuses")]
    [Tooltip("At 80%+ loyalty, ability effectiveness multiplier")]
    public float highLoyaltyBonus = 1.5f;

    [Header("Low Loyalty Penalties")]
    [Tooltip("At <30% loyalty, ability effectiveness multiplier")]
    public float lowLoyaltyPenalty = 0.7f;

    [Tooltip("At <20% loyalty, pet may run away?")]
    public bool canRunAway = true;

    /// <summary>
    /// Calculates ability effectiveness based on loyalty level
    /// </summary>
    /// <param name="loyalty">Current loyalty percentage (0-100)</param>
    /// <returns>Effectiveness multiplier</returns>
    public float GetAbilityEffectiveness(float loyalty)
    {
        if (loyalty >= 80f)
        {
            return highLoyaltyBonus;
        }
        else if (loyalty < 30f)
        {
            return lowLoyaltyPenalty;
        }
        else
        {
            return 1f;
        }
    }

    /// <summary>
    /// Gets the effective passive ability value based on loyalty
    /// </summary>
    /// <param name="loyalty">Current loyalty percentage</param>
    /// <returns>Effective multiplier</returns>
    public float GetEffectivePassiveValue(float loyalty)
    {
        float baseValue = passiveAbilityMultiplier;
        float effectiveness = GetAbilityEffectiveness(loyalty);

        // For reduction multipliers (< 1.0), we want higher effectiveness to reduce more
        if (baseValue < 1f)
        {
            float reduction = 1f - baseValue;
            return 1f - (reduction * effectiveness);
        }
        else
        {
            // For bonus multipliers (> 1.0)
            return baseValue * effectiveness;
        }
    }

    /// <summary>
    /// Validates pet data integrity
    /// </summary>
    public void OnValidate()
    {
        if (string.IsNullOrEmpty(petID))
        {
            petID = name.ToLower().Replace(" ", "_");
        }

        if (string.IsNullOrEmpty(petName))
        {
            petName = name;
        }

        // Ensure starter pet has no cost
        if (isStarterPet)
        {
            unlockCost = 0f;
            unlockType = PetUnlockType.Starter;
        }
    }
}

/// <summary>
/// Types of pet companions available
/// </summary>
public enum PetType
{
    Dog,
    Cat,
    Seabird,
    Otter,
    HermitCrab,
    Ghost
}

/// <summary>
/// How a pet is unlocked
/// </summary>
public enum PetUnlockType
{
    Starter,        // Given at game start
    Purchase,       // Buy from shop
    QuestReward,    // Complete quest
    RareFind,       // Discover in world
    StoryUnlock     // Progress-based unlock
}

/// <summary>
/// Passive abilities pets can have
/// </summary>
public enum PetPassiveAbility
{
    None,
    HazardAlert,        // Dog: -20% hazard damage
    NightVision,        // Cat: +30% night vision, -15% sanity drain
    FishSpotter,        // Seabird: +25% rare fish spawn
    DiveRetrieval,      // Otter: +10% catch rate
    ShellGuard,         // Hermit Crab: +15% inventory space
    VoidSense           // Ghost: Reveal secrets, +50% aberrant detection
}

/// <summary>
/// Active abilities pets can use
/// </summary>
public enum PetActiveAbility
{
    None,
    Fetch,              // Dog: Retrieve dropped items
    Stealth,            // Cat: Hide from one hazard
    Scout,              // Seabird: Reveal nearby fish schools
    TreasureDive,       // Otter: Find 1 relic
    ShellShield,        // Hermit Crab: Protect all fish from theft
    EtherealPhase       // Ghost: Immune to hazards for 30s
}

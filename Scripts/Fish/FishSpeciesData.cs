using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Agent 8: Fish AI & Behavior Agent - FishSpeciesData.cs
/// ScriptableObject that defines a fish species with all its properties.
/// Used by FishDatabase to manage fish data.
/// </summary>
[CreateAssetMenu(fileName = "NewFishSpecies", menuName = "Bahnfish/Fish Species", order = 1)]
public class FishSpeciesData : ScriptableObject
{
    [Header("Basic Info")]
    [Tooltip("Unique identifier for this fish species")]
    public string fishID = "";

    [Tooltip("Display name of the fish")]
    public string fishName = "Unknown Fish";

    [Tooltip("Rarity tier of this fish")]
    public FishRarity rarity = FishRarity.Common;

    [TextArea(3, 6)]
    [Tooltip("Description shown in the angler's journal")]
    public string description = "";

    [Header("Economic Value")]
    [Tooltip("Base selling price of this fish")]
    public float baseValue = 10f;

    [Tooltip("Grid size in inventory (width x height)")]
    public Vector2Int inventorySize = new Vector2Int(1, 1);

    [Header("Physical Properties")]
    [Tooltip("Weight range in kg")]
    public Vector2 weightRange = new Vector2(0.5f, 2f);

    [Tooltip("Length range in cm")]
    public Vector2 lengthRange = new Vector2(10f, 30f);

    [Header("Spawning Conditions")]
    [Tooltip("Minimum depth in meters where this fish spawns")]
    public float minDepth = 0f;

    [Tooltip("Maximum depth in meters where this fish spawns")]
    public float maxDepth = 10f;

    [Tooltip("Preferred time of day for spawning")]
    public TimeOfDay preferredTime = TimeOfDay.Day;

    [Tooltip("Weather types that boost spawn rate")]
    public List<WeatherType> preferredWeather = new List<WeatherType> { WeatherType.Clear };

    [Tooltip("Location IDs where this fish can spawn")]
    public List<string> allowedLocations = new List<string> { "starter_lake" };

    [Header("Behavior")]
    [Tooltip("Type of movement behavior this fish uses")]
    public FishBehaviorType behaviorType = FishBehaviorType.Normal;

    [Tooltip("Movement speed multiplier")]
    [Range(0.5f, 3f)]
    public float speedMultiplier = 1f;

    [Tooltip("How aggressive the fish is when hooked (affects difficulty)")]
    [Range(0f, 1f)]
    public float aggression = 0.5f;

    [Tooltip("How long the fish fights before tiring (in seconds)")]
    [Range(5f, 120f)]
    public float staminaDuration = 30f;

    [Header("Bait Preferences")]
    [Tooltip("Types of bait this fish prefers (affects bite chance)")]
    public List<BaitType> preferredBait = new List<BaitType> { BaitType.Worms };

    [Tooltip("Multiplier for bite chance when using preferred bait")]
    [Range(1f, 5f)]
    public float baitPreferenceMultiplier = 2f;

    [Header("Special Properties")]
    [Tooltip("Is this an aberrant (mutated) variant?")]
    public bool isAberrant = false;

    [Tooltip("Does this fish have unique boss mechanics?")]
    public bool isLegendary = false;

    [Tooltip("Special abilities or effects this fish has")]
    public List<FishAbility> specialAbilities = new List<FishAbility>();

    [Header("Visual & Audio")]
    [Tooltip("Icon shown in inventory and journal")]
    public Sprite icon;

    [Tooltip("3D model prefab for this fish")]
    public GameObject fishPrefab;

    [Tooltip("Color tint for aberrant variants")]
    public Color aberrantGlowColor = Color.green;

    [Tooltip("Sound effect when this fish bites")]
    public AudioClip biteSound;

    [Tooltip("Sound effect when this fish is caught")]
    public AudioClip catchSound;

    /// <summary>
    /// Generates a random weight within the weight range.
    /// </summary>
    public float GetRandomWeight()
    {
        return Random.Range(weightRange.x, weightRange.y);
    }

    /// <summary>
    /// Generates a random length within the length range.
    /// </summary>
    public float GetRandomLength()
    {
        return Random.Range(lengthRange.x, lengthRange.y);
    }

    /// <summary>
    /// Calculates the sell value with quality multiplier.
    /// </summary>
    public float GetSellValue(float qualityMultiplier = 1f)
    {
        return baseValue * qualityMultiplier;
    }

    /// <summary>
    /// Checks if this fish can spawn at the given depth.
    /// </summary>
    public bool CanSpawnAtDepth(float depth)
    {
        return depth >= minDepth && depth <= maxDepth;
    }

    /// <summary>
    /// Checks if this fish can spawn at the given location.
    /// </summary>
    public bool CanSpawnAtLocation(string locationID)
    {
        return allowedLocations.Contains(locationID);
    }

    /// <summary>
    /// Checks if this fish prefers the given weather.
    /// </summary>
    public bool PrefersWeather(WeatherType weather)
    {
        return preferredWeather.Contains(weather);
    }

    /// <summary>
    /// Checks if the given bait is preferred by this fish.
    /// </summary>
    public bool PrefersBait(BaitType bait)
    {
        return preferredBait.Contains(bait);
    }

    /// <summary>
    /// Converts this ScriptableObject data to a Fish instance.
    /// </summary>
    public Fish ToFishInstance()
    {
        Fish fish = new Fish
        {
            id = this.fishID,
            name = this.fishName,
            rarity = this.rarity,
            baseValue = this.baseValue,
            inventorySize = this.inventorySize,
            isAberrant = this.isAberrant,
            icon = this.icon,
            depthRange = new Vector2(this.minDepth, this.maxDepth),
            preferredTime = this.preferredTime,
            weight = GetRandomWeight(),
            length = GetRandomLength(),
            description = this.description
        };

        return fish;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Auto-generate ID from name if empty
        if (string.IsNullOrEmpty(fishID) && !string.IsNullOrEmpty(fishName))
        {
            fishID = fishName.ToLower().Replace(" ", "_");
        }

        // Ensure depth values are valid
        if (minDepth > maxDepth)
        {
            maxDepth = minDepth;
        }

        // Ensure weight values are valid
        if (weightRange.x > weightRange.y)
        {
            weightRange.y = weightRange.x;
        }

        // Ensure length values are valid
        if (lengthRange.x > lengthRange.y)
        {
            lengthRange.y = lengthRange.x;
        }

        // Legendary fish should have higher values
        if (isLegendary && rarity != FishRarity.Legendary)
        {
            Debug.LogWarning($"[{fishName}] Legendary fish should have Legendary rarity!");
        }

        // Aberrant fish should have Aberrant rarity (unless they're legendary aberrants)
        if (isAberrant && !isLegendary && rarity != FishRarity.Aberrant)
        {
            Debug.LogWarning($"[{fishName}] Aberrant fish should have Aberrant rarity!");
        }
    }
#endif
}

/// <summary>
/// Types of fish movement behavior.
/// </summary>
[System.Serializable]
public enum FishBehaviorType
{
    Normal,      // Standard swimming
    Aberrant,    // Erratic, supernatural movement
    Legendary    // Boss-level mechanics
}

/// <summary>
/// Types of bait available in the game.
/// </summary>
[System.Serializable]
public enum BaitType
{
    Worms,
    Lures,
    Chum,
    Specialized,
    Rare,
    Legendary
}

/// <summary>
/// Special abilities that fish can have.
/// </summary>
[System.Serializable]
public enum FishAbility
{
    None,
    FastSwimmer,       // Moves very quickly
    LineBreaker,       // Increased chance to break line
    Camouflage,        // Hard to detect
    Teleport,          // Can phase/teleport (aberrants)
    Regeneration,      // Heals stamina during fight
    Curse,             // Can curse player's equipment
    WeatherControl,    // Affects weather when present
    Summon,            // Calls other fish to help (legendary)
    EnrageNearby       // Makes nearby fish more aggressive
}

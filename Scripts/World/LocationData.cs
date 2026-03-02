using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Agent 14: Location & World Generation Agent - LocationData.cs
/// ScriptableObject that defines a fishing location with all its properties.
/// Used by LocationManager to manage location data.
/// </summary>
[CreateAssetMenu(fileName = "NewLocation", menuName = "Bahnfish/Location", order = 2)]
public class LocationData : ScriptableObject
{
    [Header("Basic Info")]
    [Tooltip("Unique identifier for this location")]
    public string locationID = "";

    [Tooltip("Display name of the location")]
    public string locationName = "Unknown Location";

    [Tooltip("Description shown in location selection")]
    [TextArea(3, 6)]
    public string description = "";

    [Header("Progression")]
    [Tooltip("Cost to unlock this location (0 = free/starter)")]
    public int licenseCost = 0;

    [Tooltip("Difficulty rating of this location")]
    public LocationDifficulty difficulty = LocationDifficulty.Beginner;

    [Tooltip("Biome type for visual/audio theming")]
    public BiomeType biomeType = BiomeType.Freshwater;

    [Header("Spawn Configuration")]
    [Tooltip("Fish species IDs that can spawn here")]
    public List<string> fishSpeciesIDs = new List<string>();

    [Tooltip("Spawn rate multiplier for rare fish (1.0 = normal)")]
    [Range(0.1f, 10f)]
    public float rareFishSpawnMultiplier = 1f;

    [Tooltip("Spawn rate multiplier for legendary fish (1.0 = normal)")]
    [Range(0.1f, 10f)]
    public float legendaryFishSpawnMultiplier = 1f;

    [Tooltip("Spawn rate multiplier for aberrant fish at night (1.0 = normal)")]
    [Range(0.1f, 10f)]
    public float aberrantSpawnMultiplier = 1f;

    [Header("Environment")]
    [Tooltip("Allowed weather types for this location")]
    public List<WeatherType> allowedWeather = new List<WeatherType>
    {
        WeatherType.Clear,
        WeatherType.Rain
    };

    [Tooltip("Sanity drain rate modifier (1.0 = normal, 2.0 = double drain)")]
    [Range(0f, 5f)]
    public float sanityDrainModifier = 1f;

    [Tooltip("Hazard spawn rate modifier (1.0 = normal, 2.0 = double spawns)")]
    [Range(0f, 5f)]
    public float hazardSpawnRateModifier = 1f;

    [Tooltip("Does this location have permanent fog?")]
    public bool hasPermanentFog = false;

    [Tooltip("Temperature modifier (affects cold damage in Arctic)")]
    [Range(-50f, 50f)]
    public float temperatureModifier = 0f;

    [Header("World Position")]
    [Tooltip("Dock position where player boat spawns")]
    public Vector3 dockPosition = Vector3.zero;

    [Tooltip("Spawn bounds for fish (min corner)")]
    public Vector3 spawnBoundsMin = new Vector3(-100, -50, -100);

    [Tooltip("Spawn bounds for fish (max corner)")]
    public Vector3 spawnBoundsMax = new Vector3(100, 0, 100);

    [Tooltip("Scene name to load for this location")]
    public string sceneName = "";

    [Header("Special Features")]
    [Tooltip("Secret area IDs within this location")]
    public List<string> secretAreaIDs = new List<string>();

    [Tooltip("NPC IDs that appear at this location")]
    public List<string> npcIDs = new List<string>();

    [Tooltip("Does this location have special story significance?")]
    public bool isStoryLocation = false;

    [Tooltip("Special mechanics or features unique to this location")]
    [TextArea(2, 4)]
    public string specialMechanics = "";

    [Header("Visual & Audio")]
    [Tooltip("Location thumbnail for UI")]
    public Sprite locationThumbnail;

    [Tooltip("Background music track for this location")]
    public AudioClip ambientMusic;

    [Tooltip("Ambient sound effects (waves, wind, etc)")]
    public AudioClip ambientSounds;

    /// <summary>
    /// Checks if this location allows a specific weather type.
    /// </summary>
    public bool AllowsWeather(WeatherType weather)
    {
        return allowedWeather.Contains(weather);
    }

    /// <summary>
    /// Checks if a fish species can spawn at this location.
    /// </summary>
    public bool CanSpawnFish(string fishID)
    {
        return fishSpeciesIDs.Contains(fishID);
    }

    /// <summary>
    /// Gets the spawn bounds as a Bounds object.
    /// </summary>
    public Bounds GetSpawnBounds()
    {
        Vector3 center = (spawnBoundsMin + spawnBoundsMax) / 2f;
        Vector3 size = spawnBoundsMax - spawnBoundsMin;
        return new Bounds(center, size);
    }

    /// <summary>
    /// Gets a random position within the spawn bounds.
    /// </summary>
    public Vector3 GetRandomSpawnPosition()
    {
        return new Vector3(
            Random.Range(spawnBoundsMin.x, spawnBoundsMax.x),
            Random.Range(spawnBoundsMin.y, spawnBoundsMax.y),
            Random.Range(spawnBoundsMin.z, spawnBoundsMax.z)
        );
    }

    /// <summary>
    /// Converts this ScriptableObject to a Location instance.
    /// </summary>
    public Location ToLocationInstance(bool unlocked)
    {
        Location location = new Location
        {
            id = this.locationID,
            name = this.locationName,
            description = this.description,
            isUnlocked = unlocked,
            unlockCost = this.licenseCost,
            worldPosition = this.dockPosition,
            sceneName = this.sceneName
        };

        return location;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Auto-generate ID from name if empty
        if (string.IsNullOrEmpty(locationID) && !string.IsNullOrEmpty(locationName))
        {
            locationID = locationName.ToLower().Replace(" ", "_");
        }

        // Ensure spawn bounds are valid
        if (spawnBoundsMin.x > spawnBoundsMax.x)
        {
            spawnBoundsMax.x = spawnBoundsMin.x;
        }
        if (spawnBoundsMin.y > spawnBoundsMax.y)
        {
            spawnBoundsMax.y = spawnBoundsMin.y;
        }
        if (spawnBoundsMin.z > spawnBoundsMax.z)
        {
            spawnBoundsMax.z = spawnBoundsMin.z;
        }

        // Starter location should be free
        if (locationID == "calm_lake" && licenseCost != 0)
        {
            Debug.LogWarning($"[{locationName}] Starter location should have 0 license cost!");
        }

        // Scene name should match location ID convention
        if (string.IsNullOrEmpty(sceneName))
        {
            sceneName = $"Location_{locationID}";
        }
    }
#endif
}

/// <summary>
/// Difficulty levels for fishing locations.
/// </summary>
[System.Serializable]
public enum LocationDifficulty
{
    Beginner,
    Easy,
    Medium,
    Hard,
    Extreme
}

/// <summary>
/// Biome types for location theming.
/// </summary>
[System.Serializable]
public enum BiomeType
{
    Freshwater,      // Lakes, rivers
    Coastal,         // Rocky shores, beaches
    Ocean,           // Deep ocean, open water
    Swamp,           // Murky, fog-shrouded
    Arctic,          // Icy, cold waters
    Tropical,        // Coral reefs, warm waters
    Underground,     // Caves, caverns
    Volcanic,        // Geothermal vents, lava
    Bioluminescent,  // Glowing, magical
    Abyssal,         // Extreme depths, darkness
    Mangrove         // Tangled roots, brackish
}

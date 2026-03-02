using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Comprehensive data structure for all persistent game state.
/// This class contains all data that needs to be saved and loaded.
/// </summary>
[System.Serializable]
public class SaveData
{
    // ===== META INFORMATION =====
    [Header("Meta Information")]
    public string saveVersion = "1.0.0";
    public string saveTimestamp;
    public string gameVersion;
    public float totalPlayTime; // in seconds

    // ===== PLAYER STATE =====
    [Header("Player State")]
    public SerializableVector3 playerPosition;
    public SerializableQuaternion playerRotation;

    // ===== TIME & ENVIRONMENT =====
    [Header("Time & Environment")]
    public float currentTime; // 0-24 hour format
    public TimeOfDay timeOfDay;
    public WeatherType currentWeather;

    // ===== RESOURCES =====
    [Header("Resources")]
    public float money;
    public float scrap;
    public int relics;
    public float fuel;
    public float sanity;

    // ===== LOCATION =====
    [Header("Location")]
    public string currentLocationID;
    public List<string> unlockedLocations = new List<string>();

    // ===== INVENTORY =====
    [Header("Inventory")]
    public List<SerializedItem> inventoryItems = new List<SerializedItem>();
    public int inventoryCapacity = 20;

    // ===== UPGRADES =====
    [Header("Upgrades")]
    public List<string> purchasedUpgrades = new List<string>();
    public Dictionary<string, int> upgradelevels = new Dictionary<string, int>();

    // ===== FISH COLLECTION =====
    [Header("Fish Collection")]
    public Dictionary<string, int> fishCaughtCount = new Dictionary<string, int>();
    public List<string> discoveredFishSpecies = new List<string>();
    public Dictionary<string, float> fishRecordWeights = new Dictionary<string, float>();

    // ===== QUESTS =====
    [Header("Quests")]
    public List<string> completedQuests = new List<string>();
    public List<string> activeQuests = new List<string>();
    public Dictionary<string, int> questProgress = new Dictionary<string, int>();

    // ===== DARK ABILITIES =====
    [Header("Dark Abilities")]
    public List<string> unlockedAbilities = new List<string>();

    // ===== AQUARIUM =====
    [Header("Aquarium")]
    public List<string> aquariumFish = new List<string>();
    public Dictionary<string, int> breedingPairs = new Dictionary<string, int>();

    // ===== STATISTICS =====
    [Header("Statistics")]
    public int totalFishCaught;
    public float totalMoneyEarned;
    public int nightsSurvived;
    public float lowestSanity;
    public int deathCount;

    // ===== SYSTEM DATA (JSON STRINGS) =====
    [Header("System Data")]
    public string locationData;        // LocationManager data (LocationSaveData)
    public string fastTravelData;      // FastTravelSystem data
    public string secretAreaData;      // SecretAreaManager data
    public string eventManagerData;    // EventManager data (Agent 19)
    public string eventCalendarData;   // EventCalendar data (Agent 19)
    public string migrationData;       // MigrationSystem data (Agent 19)

    /// <summary>
    /// Creates a new SaveData instance with default values.
    /// </summary>
    public SaveData()
    {
        saveVersion = "1.0.0";
        saveTimestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        gameVersion = Application.version;
        totalPlayTime = 0f;

        playerPosition = new SerializableVector3(Vector3.zero);
        playerRotation = new SerializableQuaternion(Quaternion.identity);

        currentTime = 8f; // Start at 8 AM
        timeOfDay = TimeOfDay.Day;
        currentWeather = WeatherType.Clear;

        money = 100f;
        scrap = 0f;
        relics = 0;
        fuel = 100f;
        sanity = 100f;

        currentLocationID = "starter_lake";
        unlockedLocations.Add("starter_lake");

        inventoryCapacity = 20;

        totalFishCaught = 0;
        totalMoneyEarned = 0f;
        nightsSurvived = 0;
        lowestSanity = 100f;
        deathCount = 0;
    }
}

/// <summary>
/// Serializable representation of an inventory item.
/// Contains all necessary data to reconstruct the item.
/// </summary>
[System.Serializable]
public class SerializedItem
{
    public string itemID;
    public string itemName;
    public string itemType; // "fish", "upgrade", "bait", "tool", etc.
    public int quantity;
    public float value;

    // Grid-based inventory properties
    public int gridX;
    public int gridY;
    public int width;
    public int height;
    public int rotation; // 0, 90, 180, 270

    // Fish-specific properties
    public string fishSpecies;
    public float fishWeight;
    public bool isAberrant;
    public FishRarity rarity;

    // Item condition/durability
    public float durability = 100f;
    public bool isFresh = true;

    // Additional properties stored as JSON
    public string additionalData;

    public SerializedItem()
    {
        quantity = 1;
        value = 0f;
        gridX = 0;
        gridY = 0;
        width = 1;
        height = 1;
        rotation = 0;
        durability = 100f;
        isFresh = true;
        additionalData = "";
    }

    public SerializedItem(string id, string name, string type)
    {
        itemID = id;
        itemName = name;
        itemType = type;
        quantity = 1;
        value = 0f;
        gridX = 0;
        gridY = 0;
        width = 1;
        height = 1;
        rotation = 0;
        durability = 100f;
        isFresh = true;
        additionalData = "";
    }
}

/// <summary>
/// Serializable Vector3 wrapper for JSON serialization.
/// JsonUtility doesn't handle Vector3 well in all cases.
/// </summary>
[System.Serializable]
public struct SerializableVector3
{
    public float x;
    public float y;
    public float z;

    public SerializableVector3(Vector3 vector)
    {
        x = vector.x;
        y = vector.y;
        z = vector.z;
    }

    public Vector3 ToVector3()
    {
        return new Vector3(x, y, z);
    }

    public static implicit operator Vector3(SerializableVector3 sv)
    {
        return new Vector3(sv.x, sv.y, sv.z);
    }

    public static implicit operator SerializableVector3(Vector3 v)
    {
        return new SerializableVector3(v);
    }
}

/// <summary>
/// Serializable Quaternion wrapper for JSON serialization.
/// </summary>
[System.Serializable]
public struct SerializableQuaternion
{
    public float x;
    public float y;
    public float z;
    public float w;

    public SerializableQuaternion(Quaternion quaternion)
    {
        x = quaternion.x;
        y = quaternion.y;
        z = quaternion.z;
        w = quaternion.w;
    }

    public Quaternion ToQuaternion()
    {
        return new Quaternion(x, y, z, w);
    }

    public static implicit operator Quaternion(SerializableQuaternion sq)
    {
        return new Quaternion(sq.x, sq.y, sq.z, sq.w);
    }

    public static implicit operator SerializableQuaternion(Quaternion q)
    {
        return new SerializableQuaternion(q);
    }
}

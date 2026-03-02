using UnityEngine;

/// <summary>
/// Common data types and enums used across the game.
/// Shared by all agents to maintain consistency.
/// </summary>

[System.Serializable]
public enum TimeOfDay
{
    Day,
    Dusk,
    Night,
    Dawn
}

[System.Serializable]
public enum WeatherType
{
    Clear,
    Rain,
    Storm,
    Fog
}

[System.Serializable]
public enum FishRarity
{
    Common,
    Uncommon,
    Rare,
    Legendary,
    Aberrant
}

/// <summary>
/// Player movement event data
/// </summary>
[System.Serializable]
public struct PlayerMovedEventData
{
    public Vector3 position;
    public Vector3 velocity;
    public float speed;

    public PlayerMovedEventData(Vector3 position, Vector3 velocity, float speed)
    {
        this.position = position;
        this.velocity = velocity;
        this.speed = speed;
    }
}

/// <summary>
/// Interaction event data
/// </summary>
[System.Serializable]
public struct InteractionEventData
{
    public GameObject interactable;
    public float distance;
    public Vector3 position;

    public InteractionEventData(GameObject interactable, float distance, Vector3 position)
    {
        this.interactable = interactable;
        this.distance = distance;
        this.position = position;
    }
}

/// <summary>
/// Time changed event data
/// </summary>
[System.Serializable]
public struct TimeChangedEventData
{
    public float currentTime;
    public TimeOfDay timeOfDay;

    public TimeChangedEventData(float currentTime, TimeOfDay timeOfDay)
    {
        this.currentTime = currentTime;
        this.timeOfDay = timeOfDay;
    }
}

/// <summary>
/// Core game state structure that holds all essential runtime data.
/// Used for save/load operations and state management.
/// </summary>
[System.Serializable]
public class GameState
{
    public Vector3 playerPosition;
    public float currentTime;
    public TimeOfDay timeOfDay;
    public WeatherType weather;
    public float sanity;
    public float money;
    public float fuel;
    public string currentLocationID;

    public GameState()
    {
        playerPosition = Vector3.zero;
        currentTime = 8f;
        timeOfDay = TimeOfDay.Day;
        weather = WeatherType.Clear;
        sanity = 100f;
        money = 100f;
        fuel = 100f;
        currentLocationID = "starter_lake";
    }

    public GameState Clone()
    {
        return new GameState
        {
            playerPosition = this.playerPosition,
            currentTime = this.currentTime,
            timeOfDay = this.timeOfDay,
            weather = this.weather,
            sanity = this.sanity,
            money = this.money,
            fuel = this.fuel,
            currentLocationID = this.currentLocationID
        };
    }
}

/// <summary>
/// Represents a fish species with all its properties.
/// </summary>
[System.Serializable]
public class Fish
{
    public string id;
    public string name;
    public FishRarity rarity;
    public float baseValue;
    public Vector2Int inventorySize;
    public bool isAberrant;
    public Sprite icon;
    public Vector2 depthRange;
    public TimeOfDay preferredTime;
    public float weight;
    public float length;
    public string description;

    public Fish()
    {
        id = "";
        name = "Unknown Fish";
        rarity = FishRarity.Common;
        baseValue = 10f;
        inventorySize = new Vector2Int(1, 1);
        isAberrant = false;
        depthRange = new Vector2(0f, 10f);
        preferredTime = TimeOfDay.Day;
        weight = 1f;
        length = 20f;
        description = "";
    }

    public float GetSellValue(float qualityMultiplier = 1f)
    {
        return baseValue * qualityMultiplier;
    }
}

/// <summary>
/// Represents a fishing location in the game world.
/// </summary>
[System.Serializable]
public class Location
{
    public string id;
    public string name;
    public string description;
    public bool isUnlocked;
    public float unlockCost;
    public Vector3 worldPosition;
    public string sceneName;

    public Location()
    {
        id = "";
        name = "Unknown Location";
        description = "";
        isUnlocked = false;
        unlockCost = 0f;
        worldPosition = Vector3.zero;
        sceneName = "";
    }
}

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Agent 19: Dynamic Events Agent - EventData.cs
/// ScriptableObject that defines a dynamic event with all its properties.
/// Used to configure events in the Unity Editor.
/// </summary>
[CreateAssetMenu(fileName = "New Event", menuName = "Bahnfish/Events/Event Data")]
public class EventData : ScriptableObject
{
    [Header("Event Identity")]
    [Tooltip("Unique identifier for this event")]
    public string eventID;

    [Tooltip("Display name for the event")]
    public string eventName;

    [Tooltip("Type of event")]
    public EventType eventType;

    [Header("Scheduling")]
    [Tooltip("Probability of this event occurring per day (0-1)")]
    [Range(0f, 1f)]
    public float probability = 0.1f;

    [Tooltip("Minimum days between occurrences")]
    public int minimumDaysBetween = 0;

    [Tooltip("Is this event triggered by story progression?")]
    public bool isStoryEvent = false;

    [Header("Duration")]
    [Tooltip("Duration of the event in real-time minutes")]
    public float durationMinutes = 5f;

    [Tooltip("Can this event be ended early?")]
    public bool canEndEarly = false;

    [Header("Event Effects")]
    [Tooltip("Fish spawn rate multiplier during event")]
    public float fishSpawnMultiplier = 1f;

    [Tooltip("Rare fish spawn rate multiplier")]
    public float rareFishMultiplier = 1f;

    [Tooltip("Legendary fish spawn chance multiplier")]
    public float legendaryFishMultiplier = 1f;

    [Tooltip("Aberrant fish spawn chance multiplier")]
    public float aberrantFishMultiplier = 1f;

    [Tooltip("Hazard spawn rate multiplier")]
    public float hazardSpawnMultiplier = 1f;

    [Tooltip("Sanity drain multiplier")]
    public float sanityDrainMultiplier = 1f;

    [Tooltip("Catch success rate modifier (-1 to 1)")]
    [Range(-1f, 1f)]
    public float catchSuccessModifier = 0f;

    [Tooltip("Sell price multiplier")]
    public float sellPriceMultiplier = 1f;

    [Header("Conditional Effects")]
    [Tooltip("Force all fish spawns to be aberrant?")]
    public bool forceAberrantFish = false;

    [Tooltip("Override weather during event")]
    public bool overrideWeather = false;

    [Tooltip("Weather to set if overriding")]
    public WeatherType eventWeather = WeatherType.Clear;

    [Tooltip("Override time progression")]
    public bool overrideTime = false;

    [Tooltip("Time speed multiplier (0 = frozen, 1 = normal)")]
    [Range(0f, 2f)]
    public float timeSpeedMultiplier = 1f;

    [Header("Visual Effects")]
    [Tooltip("Sky color tint during event")]
    public Color skyTint = Color.white;

    [Tooltip("Water color tint during event")]
    public Color waterTint = Color.white;

    [Tooltip("Post-processing intensity")]
    [Range(0f, 1f)]
    public float postProcessingIntensity = 0f;

    [Tooltip("Vignette color")]
    public Color vignetteColor = Color.black;

    [Header("Audio")]
    [Tooltip("Music track to play during event")]
    public AudioClip eventMusic;

    [Tooltip("Ambient sounds during event")]
    public List<AudioClip> ambientSounds = new List<AudioClip>();

    [Header("NPC Messages")]
    [Tooltip("Warning message NPCs say before event")]
    [TextArea(2, 4)]
    public string warningMessage = "";

    [Tooltip("Message during event")]
    [TextArea(2, 4)]
    public string activeMessage = "";

    [Tooltip("Days before event that warning is given")]
    public int warningDaysBefore = 1;

    [Header("Rewards")]
    [Tooltip("Guaranteed legendary fish spawn?")]
    public bool guaranteedLegendarySpawn = false;

    [Tooltip("Relic drop multiplier")]
    public float relicMultiplier = 1f;

    [Tooltip("Special items that can only drop during this event")]
    public List<string> exclusiveItems = new List<string>();

    [Header("Requirements")]
    [Tooltip("Required time of day for event to trigger")]
    public TimeOfDay requiredTimeOfDay = TimeOfDay.Day;

    [Tooltip("Can trigger at any time?")]
    public bool anyTimeOfDay = true;

    [Tooltip("Required locations (empty = any location)")]
    public List<string> requiredLocations = new List<string>();

    [Header("Display")]
    [Tooltip("Icon for event notifications")]
    public Sprite eventIcon;

    [Tooltip("Description shown in event log")]
    [TextArea(3, 6)]
    public string description = "";
}

/// <summary>
/// Types of dynamic events in the game
/// </summary>
[System.Serializable]
public enum EventType
{
    BloodMoon,
    MeteorShower,
    FogBank,
    Festival,
    Migration,
    Storm,
    SolarEclipse,
    FishSchool,  // Random spawn boost
    Custom
}

/// <summary>
/// Runtime instance of an event
/// </summary>
[System.Serializable]
public class GameEvent
{
    public EventData data;
    public float startTime;
    public float endTime;
    public bool isActive;
    public int dayStarted;

    public GameEvent(EventData eventData, float startTime, float durationMinutes, int currentDay)
    {
        this.data = eventData;
        this.startTime = startTime;
        this.endTime = startTime + (durationMinutes * 60f);
        this.isActive = true;
        this.dayStarted = currentDay;
    }

    public float GetRemainingTime()
    {
        return Mathf.Max(0f, endTime - Time.time);
    }

    public float GetProgress()
    {
        float duration = endTime - startTime;
        float elapsed = Time.time - startTime;
        return Mathf.Clamp01(elapsed / duration);
    }

    public bool HasExpired()
    {
        return Time.time >= endTime;
    }
}

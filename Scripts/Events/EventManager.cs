using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Agent 19: Dynamic Events Agent - EventManager.cs
/// Central manager for all dynamic events in the game.
/// Handles event scheduling, activation, and coordination with other systems.
/// </summary>
public class EventManager : MonoBehaviour
{
    private static EventManager _instance;
    public static EventManager Instance => _instance;

    [Header("Event Database")]
    [Tooltip("All available events in the game")]
    [SerializeField] private List<EventData> allEvents = new List<EventData>();

    [Header("Active Event")]
    [SerializeField] private GameEvent currentEvent = null;
    [SerializeField] private bool hasActiveEvent = false;

    [Header("Scheduling Configuration")]
    [Tooltip("Only one major event can be active at a time")]
    [SerializeField] private bool allowOverlappingEvents = false;

    [Tooltip("Check for events every X hours")]
    [SerializeField] private float eventCheckInterval = 1f; // 1 hour

    [Header("Daily Event Roll")]
    [SerializeField] private bool dailyRollComplete = false;
    [SerializeField] private int lastRollDay = -1;

    [Header("Status")]
    [SerializeField] private float nextEventCheckTime = 0f;
    [SerializeField] private int currentDay = 0;

    [Header("Event Modifiers (Runtime)")]
    [SerializeField] private float globalSpawnMultiplier = 1f;
    [SerializeField] private float globalRareMultiplier = 1f;
    [SerializeField] private float globalHazardMultiplier = 1f;
    [SerializeField] private float globalSanityMultiplier = 1f;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogging = true;
    [SerializeField] private bool showEventUI = true;

    // Component references
    private FishManager fishManager;
    private EventCalendar eventCalendar;
    private TimeManager timeManager;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        // Get component references
        fishManager = FishManager.Instance;
        eventCalendar = EventCalendar.Instance;
        timeManager = TimeManager.Instance;

        // Subscribe to time events
        EventSystem.Subscribe("DayCompleted", OnDayCompleted);
        EventSystem.Subscribe<SaveData>("GatheringSaveData", OnGatheringSaveData);
        EventSystem.Subscribe<SaveData>("ApplyingSaveData", OnApplyingSaveData);

        // Load default events if none assigned
        if (allEvents.Count == 0)
        {
            LoadDefaultEvents();
        }

        if (enableDebugLogging)
        {
            Debug.Log($"[EventManager] Initialized with {allEvents.Count} events");
        }
    }

    private void Update()
    {
        // Update active event
        if (hasActiveEvent && currentEvent != null)
        {
            UpdateCurrentEvent();
        }

        // Check for new events
        if (Time.time >= nextEventCheckTime)
        {
            CheckForEvents();
            nextEventCheckTime = Time.time + (eventCheckInterval * 3600f); // Convert hours to seconds
        }
    }

    /// <summary>
    /// Performs daily event roll at midnight
    /// </summary>
    private void PerformDailyRoll()
    {
        if (dailyRollComplete) return;

        if (enableDebugLogging)
        {
            Debug.Log($"[EventManager] Performing daily event roll for day {currentDay}");
        }

        // Blood Moon check (every 10 days)
        EventData bloodMoon = GetEventByType(EventType.BloodMoon);
        if (bloodMoon != null)
        {
            int daysSinceLastBloodMoon = eventCalendar.GetDaysSinceLastOccurrence(bloodMoon.eventID);
            if (daysSinceLastBloodMoon >= 10)
            {
                float roll = Random.value;
                if (roll < 0.1f) // 10% chance
                {
                    ScheduleEvent(bloodMoon, TimeOfDay.Night);
                    dailyRollComplete = true;
                    return;
                }
            }
        }

        // Meteor Shower check (every 3 days)
        EventData meteorShower = GetEventByType(EventType.MeteorShower);
        if (meteorShower != null)
        {
            int daysSinceLastMeteor = eventCalendar.GetDaysSinceLastOccurrence(meteorShower.eventID);
            if (daysSinceLastMeteor >= 3)
            {
                float roll = Random.value;
                if (roll < 0.3f) // 30% chance
                {
                    ScheduleEvent(meteorShower);
                    dailyRollComplete = true;
                    return;
                }
            }
        }

        // Festival check (every 7 days, scheduled)
        EventData festival = GetEventByType(EventType.Festival);
        if (festival != null && currentDay % 7 == 0)
        {
            ScheduleEvent(festival, TimeOfDay.Day);
            dailyRollComplete = true;
            return;
        }

        // Storm check (15% chance)
        EventData storm = GetEventByType(EventType.Storm);
        if (storm != null)
        {
            float roll = Random.value;
            if (roll < 0.15f)
            {
                ScheduleEvent(storm);
                dailyRollComplete = true;
                return;
            }
        }

        dailyRollComplete = true;
    }

    /// <summary>
    /// Checks for hourly events (fog banks, fish schools)
    /// </summary>
    private void CheckForEvents()
    {
        if (hasActiveEvent && !allowOverlappingEvents) return;

        // Fog bank check (5% per hour at sea)
        EventData fogBank = GetEventByType(EventType.FogBank);
        if (fogBank != null && !hasActiveEvent)
        {
            float roll = Random.value;
            if (roll < 0.05f)
            {
                TriggerEvent(fogBank);
            }
        }

        // Fish school check (20% per hour)
        EventData fishSchool = GetEventByType(EventType.FishSchool);
        if (fishSchool != null && !hasActiveEvent)
        {
            float roll = Random.value;
            if (roll < 0.2f)
            {
                TriggerEvent(fishSchool);
            }
        }
    }

    /// <summary>
    /// Schedules an event to start at a specific time
    /// </summary>
    public void ScheduleEvent(EventData eventData, TimeOfDay? targetTime = null)
    {
        if (eventData == null) return;

        if (enableDebugLogging)
        {
            Debug.Log($"[EventManager] Scheduling event: {eventData.eventName}");
        }

        // If target time specified, wait for that time
        if (targetTime.HasValue && timeManager != null)
        {
            TimeOfDay currentTime = timeManager.GetCurrentTimeOfDay();
            if (currentTime != targetTime.Value)
            {
                // Wait until target time (handled by Update checking)
                return;
            }
        }

        TriggerEvent(eventData);
    }

    /// <summary>
    /// Immediately triggers an event
    /// </summary>
    public void TriggerEvent(EventData eventData)
    {
        if (eventData == null)
        {
            Debug.LogError("[EventManager] Attempted to trigger null event!");
            return;
        }

        if (hasActiveEvent && !allowOverlappingEvents)
        {
            if (enableDebugLogging)
            {
                Debug.LogWarning($"[EventManager] Cannot trigger {eventData.eventName} - event already active");
            }
            return;
        }

        // Check requirements
        if (!CheckEventRequirements(eventData))
        {
            if (enableDebugLogging)
            {
                Debug.LogWarning($"[EventManager] Event {eventData.eventName} requirements not met");
            }
            return;
        }

        // Create game event instance
        currentEvent = new GameEvent(eventData, Time.time, eventData.durationMinutes, currentDay);
        hasActiveEvent = true;

        // Apply event effects
        ApplyEventEffects(eventData);

        // Publish event started
        EventSystem.Publish("EventStarted", currentEvent);

        // Notification
        if (showEventUI)
        {
            EventSystem.Publish("ShowNotification", $"{eventData.eventName} has begun!");
        }

        if (enableDebugLogging)
        {
            Debug.Log($"[EventManager] EVENT STARTED: {eventData.eventName} (Duration: {eventData.durationMinutes} min)");
        }
    }

    /// <summary>
    /// Updates the currently active event
    /// </summary>
    private void UpdateCurrentEvent()
    {
        if (currentEvent == null) return;

        // Check if event has expired
        if (currentEvent.HasExpired())
        {
            EndCurrentEvent();
        }
    }

    /// <summary>
    /// Ends the current event
    /// </summary>
    public void EndCurrentEvent()
    {
        if (currentEvent == null || !hasActiveEvent) return;

        EventData eventData = currentEvent.data;

        // Remove event effects
        RemoveEventEffects(eventData);

        // Publish event ended
        EventSystem.Publish("EventEnded", currentEvent);

        if (showEventUI)
        {
            EventSystem.Publish("ShowNotification", $"{eventData.eventName} has ended.");
        }

        if (enableDebugLogging)
        {
            Debug.Log($"[EventManager] EVENT ENDED: {eventData.eventName}");
        }

        currentEvent = null;
        hasActiveEvent = false;
    }

    /// <summary>
    /// Applies all effects from an event
    /// </summary>
    private void ApplyEventEffects(EventData eventData)
    {
        // Update global multipliers
        globalSpawnMultiplier = eventData.fishSpawnMultiplier;
        globalRareMultiplier = eventData.rareFishMultiplier;
        globalHazardMultiplier = eventData.hazardSpawnMultiplier;
        globalSanityMultiplier = eventData.sanityDrainMultiplier;

        // Publish multiplier changes
        EventSystem.Publish("EventSpawnMultiplierChanged", globalSpawnMultiplier);
        EventSystem.Publish("EventRareMultiplierChanged", globalRareMultiplier);
        EventSystem.Publish("EventHazardMultiplierChanged", globalHazardMultiplier);
        EventSystem.Publish("EventSanityMultiplierChanged", globalSanityMultiplier);

        // Force aberrant fish if needed
        if (eventData.forceAberrantFish)
        {
            EventSystem.Publish("ForceAberrantFish", true);
        }

        // Override weather if specified
        if (eventData.overrideWeather)
        {
            EventSystem.Publish("OverrideWeather", eventData.eventWeather);
        }

        // Override time speed if specified
        if (eventData.overrideTime && timeManager != null)
        {
            timeManager.SetTimeSpeed(eventData.timeSpeedMultiplier);
        }

        // Guaranteed legendary spawn
        if (eventData.guaranteedLegendarySpawn && fishManager != null)
        {
            EventSystem.Publish("GuaranteedLegendarySpawn", true);
        }

        // Play event music
        if (eventData.eventMusic != null)
        {
            EventSystem.Publish("PlayEventMusic", eventData.eventMusic);
        }

        // Visual effects
        EventSystem.Publish("ApplyEventVisuals", eventData);
    }

    /// <summary>
    /// Removes all effects from an event
    /// </summary>
    private void RemoveEventEffects(EventData eventData)
    {
        // Reset multipliers
        globalSpawnMultiplier = 1f;
        globalRareMultiplier = 1f;
        globalHazardMultiplier = 1f;
        globalSanityMultiplier = 1f;

        EventSystem.Publish("EventSpawnMultiplierChanged", 1f);
        EventSystem.Publish("EventRareMultiplierChanged", 1f);
        EventSystem.Publish("EventHazardMultiplierChanged", 1f);
        EventSystem.Publish("EventSanityMultiplierChanged", 1f);

        // Stop forcing aberrant fish
        if (eventData.forceAberrantFish)
        {
            EventSystem.Publish("ForceAberrantFish", false);
        }

        // Restore weather
        if (eventData.overrideWeather)
        {
            EventSystem.Publish("RestoreWeather", true);
        }

        // Restore time speed
        if (eventData.overrideTime && timeManager != null)
        {
            timeManager.SetTimeSpeed(1f);
        }

        // Stop event music
        if (eventData.eventMusic != null)
        {
            EventSystem.Publish("StopEventMusic", true);
        }

        // Remove visual effects
        EventSystem.Publish("RemoveEventVisuals", true);
    }

    /// <summary>
    /// Checks if event requirements are met
    /// </summary>
    private bool CheckEventRequirements(EventData eventData)
    {
        // Check time of day requirement
        if (!eventData.anyTimeOfDay && timeManager != null)
        {
            TimeOfDay currentTime = timeManager.GetCurrentTimeOfDay();
            if (currentTime != eventData.requiredTimeOfDay)
            {
                return false;
            }
        }

        // Check location requirements
        if (eventData.requiredLocations.Count > 0)
        {
            // TODO: Check current location when Agent 14 is integrated
            // For now, assume requirement is met
        }

        // Check minimum days between occurrences
        if (eventCalendar != null)
        {
            int daysSinceLast = eventCalendar.GetDaysSinceLastOccurrence(eventData.eventID);
            if (daysSinceLast < eventData.minimumDaysBetween)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Gets the currently active event
    /// </summary>
    public GameEvent GetCurrentEvent()
    {
        return hasActiveEvent ? currentEvent : null;
    }

    /// <summary>
    /// Gets all active events
    /// </summary>
    public List<GameEvent> GetActiveEvents()
    {
        List<GameEvent> activeEvents = new List<GameEvent>();
        if (hasActiveEvent && currentEvent != null)
        {
            activeEvents.Add(currentEvent);
        }
        return activeEvents;
    }

    /// <summary>
    /// Checks if a specific event is active
    /// </summary>
    public bool IsEventActive(string eventID)
    {
        return hasActiveEvent && currentEvent != null && currentEvent.data.eventID == eventID;
    }

    /// <summary>
    /// Gets event by type
    /// </summary>
    private EventData GetEventByType(EventType type)
    {
        return allEvents.FirstOrDefault(e => e.eventType == type);
    }

    /// <summary>
    /// Gets current spawn multiplier
    /// </summary>
    public float GetSpawnMultiplier()
    {
        return globalSpawnMultiplier;
    }

    /// <summary>
    /// Gets current rare fish multiplier
    /// </summary>
    public float GetRareMultiplier()
    {
        return globalRareMultiplier;
    }

    /// <summary>
    /// Gets current hazard multiplier
    /// </summary>
    public float GetHazardMultiplier()
    {
        return globalHazardMultiplier;
    }

    /// <summary>
    /// Gets current sanity drain multiplier
    /// </summary>
    public float GetSanityMultiplier()
    {
        return globalSanityMultiplier;
    }

    /// <summary>
    /// Adds an event to the database
    /// </summary>
    public void RegisterEvent(EventData eventData)
    {
        if (!allEvents.Contains(eventData))
        {
            allEvents.Add(eventData);
        }
    }

    /// <summary>
    /// Force triggers a specific event by ID (for testing/story)
    /// </summary>
    public void ForceTriggerEvent(string eventID)
    {
        EventData eventData = allEvents.FirstOrDefault(e => e.eventID == eventID);
        if (eventData != null)
        {
            TriggerEvent(eventData);
        }
        else
        {
            Debug.LogError($"[EventManager] Event {eventID} not found!");
        }
    }

    private void OnDayCompleted()
    {
        currentDay++;
        dailyRollComplete = false;
        lastRollDay = currentDay;

        // Perform daily event roll
        PerformDailyRoll();

        // Update event predictions
        if (eventCalendar != null)
        {
            eventCalendar.UpdatePredictions(allEvents);
        }
    }

    /// <summary>
    /// Loads default events (placeholder for actual event setup)
    /// </summary>
    private void LoadDefaultEvents()
    {
        // Events should be created as ScriptableObjects in Unity Editor
        // This is just a placeholder
        if (enableDebugLogging)
        {
            Debug.LogWarning("[EventManager] No events configured. Create EventData ScriptableObjects!");
        }
    }

    #region Save/Load Integration

    private void OnGatheringSaveData(SaveData data)
    {
        EventManagerSaveData eventData = new EventManagerSaveData
        {
            currentDay = this.currentDay,
            hasActiveEvent = this.hasActiveEvent,
            lastRollDay = this.lastRollDay
        };

        // Save active event if any
        if (hasActiveEvent && currentEvent != null)
        {
            eventData.activeEventID = currentEvent.data.eventID;
            eventData.activeEventStartTime = currentEvent.startTime;
            eventData.activeEventEndTime = currentEvent.endTime;
        }

        data.eventManagerData = JsonUtility.ToJson(eventData);
    }

    private void OnApplyingSaveData(SaveData data)
    {
        if (string.IsNullOrEmpty(data.eventManagerData)) return;

        EventManagerSaveData eventData = JsonUtility.FromJson<EventManagerSaveData>(data.eventManagerData);

        this.currentDay = eventData.currentDay;
        this.lastRollDay = eventData.lastRollDay;

        // Restore active event if it was saved
        if (eventData.hasActiveEvent && !string.IsNullOrEmpty(eventData.activeEventID))
        {
            EventData savedEvent = allEvents.FirstOrDefault(e => e.eventID == eventData.activeEventID);
            if (savedEvent != null)
            {
                // Check if event should still be active
                if (Time.time < eventData.activeEventEndTime)
                {
                    TriggerEvent(savedEvent);
                    currentEvent.startTime = eventData.activeEventStartTime;
                    currentEvent.endTime = eventData.activeEventEndTime;
                }
            }
        }

        if (enableDebugLogging)
        {
            Debug.Log($"[EventManager] Loaded event data. Current day: {currentDay}");
        }
    }

    #endregion

    private void OnDestroy()
    {
        EventSystem.Unsubscribe("DayCompleted", OnDayCompleted);
        EventSystem.Unsubscribe<SaveData>("GatheringSaveData", OnGatheringSaveData);
        EventSystem.Unsubscribe<SaveData>("ApplyingSaveData", OnApplyingSaveData);

        if (_instance == this)
        {
            _instance = null;
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Trigger Test Event")]
    private void TriggerTestEvent()
    {
        if (allEvents.Count > 0)
        {
            TriggerEvent(allEvents[0]);
        }
    }

    [ContextMenu("End Current Event")]
    private void EndEventEditor()
    {
        EndCurrentEvent();
    }

    [ContextMenu("Print Event Status")]
    private void PrintStatus()
    {
        Debug.Log($"=== Event Manager Status ===");
        Debug.Log($"Current Day: {currentDay}");
        Debug.Log($"Active Event: {(hasActiveEvent ? currentEvent.data.eventName : "None")}");
        Debug.Log($"Registered Events: {allEvents.Count}");
        Debug.Log($"Spawn Multiplier: {globalSpawnMultiplier}x");
        Debug.Log($"Rare Multiplier: {globalRareMultiplier}x");
        Debug.Log($"Hazard Multiplier: {globalHazardMultiplier}x");
    }
#endif
}

/// <summary>
/// Save data for EventManager
/// </summary>
[System.Serializable]
public class EventManagerSaveData
{
    public int currentDay;
    public bool hasActiveEvent;
    public string activeEventID;
    public float activeEventStartTime;
    public float activeEventEndTime;
    public int lastRollDay;
}

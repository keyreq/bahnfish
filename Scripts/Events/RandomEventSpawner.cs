using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Agent 19: Dynamic Events Agent - RandomEventSpawner.cs
/// Schedules and triggers random events based on conditions and probability.
/// Ensures balanced event frequency and prevents overwhelming overlap.
///
/// EVENT PROBABILITY SYSTEM:
/// Daily Roll (each in-game day at midnight):
/// - Blood Moon: 10% if it's been 10+ days since last one
/// - Meteor Shower: 30% if it's been 3+ days
/// - Festival: Scheduled (every 7 days)
/// - Storm: 15% (unless weather override)
/// - Solar Eclipse: Story-triggered only
///
/// Hourly Roll (while at sea):
/// - Fog Bank: 5% per hour
/// - Random fish school: 20% (temporary spawn boost)
/// </summary>
public class RandomEventSpawner : MonoBehaviour
{
    private static RandomEventSpawner _instance;
    public static RandomEventSpawner Instance => _instance;

    [Header("Event References")]
    [SerializeField] private EventManager eventManager;
    [SerializeField] private EventCalendar eventCalendar;
    [SerializeField] private TimeManager timeManager;

    [Header("Spawning Configuration")]
    [SerializeField] private bool enableRandomEvents = true;
    [SerializeField] private bool enableDailyRolls = true;
    [SerializeField] private bool enableHourlyRolls = true;

    [Header("Probability Overrides")]
    [Tooltip("Override probabilities for testing")]
    [SerializeField] private bool useProbabilityOverrides = false;
    [SerializeField] private float bloodMoonChanceOverride = 0.1f;
    [SerializeField] private float meteorShowerChanceOverride = 0.3f;
    [SerializeField] private float stormChanceOverride = 0.15f;
    [SerializeField] private float fogBankChanceOverride = 0.05f;

    [Header("Event Cooldowns")]
    [SerializeField] private Dictionary<string, float> eventCooldowns = new Dictionary<string, float>();
    [SerializeField] private float globalEventCooldown = 60f; // Minimum 60 seconds between any events

    [Header("Scheduling")]
    [SerializeField] private int currentDay = 0;
    [SerializeField] private int currentHour = 0;
    [SerializeField] private float lastEventTime = 0f;
    [SerializeField] private float nextHourlyCheck = 0f;

    [Header("Forecast")]
    [Tooltip("Show predictions to player")]
    [SerializeField] private bool enableForecast = true;
    [SerializeField] private List<EventPrediction> forecast = new List<EventPrediction>();

    [Header("Balancing")]
    [Tooltip("Maximum events per day")]
    [SerializeField] private int maxEventsPerDay = 3;
    [SerializeField] private int eventsToday = 0;

    [Header("Statistics")]
    [SerializeField] private int totalEventsTriggered = 0;
    [SerializeField] private int dailyRollsPerformed = 0;
    [SerializeField] private int hourlyRollsPerformed = 0;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogging = true;
    [SerializeField] private bool logAllRolls = false;

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
        if (eventManager == null)
            eventManager = EventManager.Instance;

        if (eventCalendar == null)
            eventCalendar = EventCalendar.Instance;

        if (timeManager == null)
            timeManager = TimeManager.Instance;

        // Subscribe to time events
        EventSystem.Subscribe("DayCompleted", OnDayCompleted);
        EventSystem.Subscribe<float>("TimeUpdated", OnTimeUpdated);

        // Initialize next check time
        nextHourlyCheck = Time.time + 3600f; // 1 hour

        if (enableDebugLogging)
        {
            Debug.Log("[RandomEventSpawner] Initialized and ready to spawn events");
        }
    }

    private void Update()
    {
        if (!enableRandomEvents) return;

        // Check for hourly events
        if (enableHourlyRolls && Time.time >= nextHourlyCheck)
        {
            PerformHourlyRoll();
            nextHourlyCheck = Time.time + 3600f;
        }
    }

    /// <summary>
    /// Performs daily event roll at midnight
    /// </summary>
    private void PerformDailyRoll()
    {
        if (!enableDailyRolls) return;
        if (eventsToday >= maxEventsPerDay) return;

        dailyRollsPerformed++;

        if (enableDebugLogging)
        {
            Debug.Log($"[RandomEventSpawner] === DAILY ROLL for Day {currentDay} ===");
        }

        // Check Blood Moon (highest priority, rarest)
        if (RollForBloodMoon())
        {
            TriggerEvent("blood_moon");
            return;
        }

        // Check Meteor Shower
        if (RollForMeteorShower())
        {
            TriggerEvent("meteor_shower");
            return;
        }

        // Check Festival (scheduled, not random)
        if (ShouldTriggerFestival())
        {
            TriggerEvent("festival");
            return;
        }

        // Check Storm
        if (RollForStorm())
        {
            TriggerEvent("storm");
            return;
        }

        if (logAllRolls)
        {
            Debug.Log("[RandomEventSpawner] Daily roll complete - no events triggered");
        }
    }

    /// <summary>
    /// Performs hourly event roll while at sea
    /// </summary>
    private void PerformHourlyRoll()
    {
        if (!enableHourlyRolls) return;
        if (eventsToday >= maxEventsPerDay) return;

        hourlyRollsPerformed++;
        currentHour++;

        if (logAllRolls)
        {
            Debug.Log($"[RandomEventSpawner] Hourly roll at hour {currentHour}");
        }

        // Check if player is at sea
        if (!IsPlayerAtSea())
        {
            return;
        }

        // Check Fog Bank
        if (RollForFogBank())
        {
            TriggerEvent("fog_bank");
            return;
        }

        // Check Fish School (minor event)
        if (RollForFishSchool())
        {
            TriggerEvent("fish_school");
            return;
        }
    }

    #region Event Roll Methods

    /// <summary>
    /// Rolls for Blood Moon event
    /// </summary>
    private bool RollForBloodMoon()
    {
        if (eventManager == null || eventCalendar == null) return false;

        // Check if 10+ days have passed
        int daysSinceLast = eventCalendar.GetDaysSinceLastOccurrence("blood_moon");
        if (daysSinceLast < 10) return false;

        // Check cooldown
        if (IsEventOnCooldown("blood_moon")) return false;

        // Roll for 10% chance
        float chance = useProbabilityOverrides ? bloodMoonChanceOverride : 0.1f;
        float roll = Random.value;

        bool triggered = roll < chance;

        if (logAllRolls)
        {
            Debug.Log($"[RandomEventSpawner] Blood Moon roll: {roll:F3} < {chance:F3} = {triggered}");
        }

        return triggered;
    }

    /// <summary>
    /// Rolls for Meteor Shower event
    /// </summary>
    private bool RollForMeteorShower()
    {
        if (eventManager == null || eventCalendar == null) return false;

        // Check if 3+ days have passed
        int daysSinceLast = eventCalendar.GetDaysSinceLastOccurrence("meteor_shower");
        if (daysSinceLast < 3) return false;

        // Check cooldown
        if (IsEventOnCooldown("meteor_shower")) return false;

        // Roll for 30% chance
        float chance = useProbabilityOverrides ? meteorShowerChanceOverride : 0.3f;
        float roll = Random.value;

        bool triggered = roll < chance;

        if (logAllRolls)
        {
            Debug.Log($"[RandomEventSpawner] Meteor Shower roll: {roll:F3} < {chance:F3} = {triggered}");
        }

        return triggered;
    }

    /// <summary>
    /// Checks if Festival should trigger (scheduled, not random)
    /// </summary>
    private bool ShouldTriggerFestival()
    {
        // Festivals are scheduled every 7 days
        return currentDay % 7 == 0;
    }

    /// <summary>
    /// Rolls for Storm event
    /// </summary>
    private bool RollForStorm()
    {
        if (IsEventOnCooldown("storm")) return false;

        // Roll for 15% chance
        float chance = useProbabilityOverrides ? stormChanceOverride : 0.15f;
        float roll = Random.value;

        bool triggered = roll < chance;

        if (logAllRolls)
        {
            Debug.Log($"[RandomEventSpawner] Storm roll: {roll:F3} < {chance:F3} = {triggered}");
        }

        return triggered;
    }

    /// <summary>
    /// Rolls for Fog Bank event (hourly)
    /// </summary>
    private bool RollForFogBank()
    {
        if (IsEventOnCooldown("fog_bank")) return false;

        // Roll for 5% chance
        float chance = useProbabilityOverrides ? fogBankChanceOverride : 0.05f;
        float roll = Random.value;

        bool triggered = roll < chance;

        if (logAllRolls)
        {
            Debug.Log($"[RandomEventSpawner] Fog Bank roll: {roll:F3} < {chance:F3} = {triggered}");
        }

        return triggered;
    }

    /// <summary>
    /// Rolls for Fish School event (hourly, minor)
    /// </summary>
    private bool RollForFishSchool()
    {
        if (IsEventOnCooldown("fish_school")) return false;

        // Roll for 20% chance
        float chance = 0.2f;
        float roll = Random.value;

        return roll < chance;
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Triggers a specific event by ID
    /// </summary>
    private void TriggerEvent(string eventID)
    {
        if (eventManager == null)
        {
            Debug.LogError("[RandomEventSpawner] EventManager is null!");
            return;
        }

        // Check global cooldown
        if (Time.time - lastEventTime < globalEventCooldown)
        {
            if (enableDebugLogging)
            {
                Debug.Log($"[RandomEventSpawner] Event {eventID} blocked by global cooldown");
            }
            return;
        }

        // Trigger event
        eventManager.ForceTriggerEvent(eventID);

        // Update tracking
        eventsToday++;
        totalEventsTriggered++;
        lastEventTime = Time.time;
        SetEventCooldown(eventID);

        if (enableDebugLogging)
        {
            Debug.Log($"[RandomEventSpawner] *** TRIGGERED EVENT: {eventID} ***");
        }
    }

    /// <summary>
    /// Checks if player is at sea (not docked)
    /// </summary>
    private bool IsPlayerAtSea()
    {
        // TODO: Integrate with dock/location system from Agent 14
        // For now, assume player is at sea
        return true;
    }

    /// <summary>
    /// Checks if event is on cooldown
    /// </summary>
    private bool IsEventOnCooldown(string eventID)
    {
        if (!eventCooldowns.ContainsKey(eventID)) return false;

        return Time.time < eventCooldowns[eventID];
    }

    /// <summary>
    /// Sets cooldown for an event
    /// </summary>
    private void SetEventCooldown(string eventID)
    {
        float cooldownDuration = GetEventCooldownDuration(eventID);
        eventCooldowns[eventID] = Time.time + cooldownDuration;
    }

    /// <summary>
    /// Gets cooldown duration for specific event
    /// </summary>
    private float GetEventCooldownDuration(string eventID)
    {
        return eventID switch
        {
            "blood_moon" => 86400f, // 1 day minimum
            "meteor_shower" => 43200f, // 12 hours
            "fog_bank" => 3600f, // 1 hour
            "fish_school" => 1800f, // 30 minutes
            _ => globalEventCooldown
        };
    }

    /// <summary>
    /// Updates forecast based on event calendar predictions
    /// </summary>
    private void UpdateForecast()
    {
        if (!enableForecast || eventCalendar == null) return;

        forecast = eventCalendar.GetUpcomingEvents();

        EventSystem.Publish("ForecastUpdated", forecast);
    }

    #endregion

    #region Event Handlers

    private void OnDayCompleted()
    {
        currentDay++;
        eventsToday = 0;
        currentHour = 0;

        // Perform daily roll
        PerformDailyRoll();

        // Update forecast
        UpdateForecast();
    }

    private void OnTimeUpdated(float currentTime)
    {
        // Could be used for more precise timing if needed
    }

    #endregion

    #region Public Interface

    /// <summary>
    /// Gets current event statistics
    /// </summary>
    public EventSpawnerStats GetStats()
    {
        return new EventSpawnerStats
        {
            totalEventsTriggered = totalEventsTriggered,
            eventsToday = eventsToday,
            dailyRollsPerformed = dailyRollsPerformed,
            hourlyRollsPerformed = hourlyRollsPerformed,
            currentDay = currentDay
        };
    }

    /// <summary>
    /// Gets forecast of upcoming events
    /// </summary>
    public List<EventPrediction> GetForecast()
    {
        return new List<EventPrediction>(forecast);
    }

    /// <summary>
    /// Enables or disables random events
    /// </summary>
    public void SetRandomEventsEnabled(bool enabled)
    {
        enableRandomEvents = enabled;

        if (enableDebugLogging)
        {
            Debug.Log($"[RandomEventSpawner] Random events {(enabled ? "enabled" : "disabled")}");
        }
    }

    /// <summary>
    /// Forces an immediate daily roll (for testing)
    /// </summary>
    public void ForceDailyRoll()
    {
        PerformDailyRoll();
    }

    /// <summary>
    /// Forces an immediate hourly roll (for testing)
    /// </summary>
    public void ForceHourlyRoll()
    {
        PerformHourlyRoll();
    }

    #endregion

    private void OnDestroy()
    {
        EventSystem.Unsubscribe("DayCompleted", OnDayCompleted);
        EventSystem.Unsubscribe<float>("TimeUpdated", OnTimeUpdated);

        if (_instance == this)
        {
            _instance = null;
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Force Daily Roll")]
    private void ForceDailyRollEditor()
    {
        ForceDailyRoll();
    }

    [ContextMenu("Force Hourly Roll")]
    private void ForceHourlyRollEditor()
    {
        ForceHourlyRoll();
    }

    [ContextMenu("Print Statistics")]
    private void PrintStatistics()
    {
        Debug.Log($"=== Random Event Spawner Statistics ===");
        Debug.Log($"Current Day: {currentDay}");
        Debug.Log($"Events Today: {eventsToday} / {maxEventsPerDay}");
        Debug.Log($"Total Events Triggered: {totalEventsTriggered}");
        Debug.Log($"Daily Rolls: {dailyRollsPerformed}");
        Debug.Log($"Hourly Rolls: {hourlyRollsPerformed}");
        Debug.Log($"Forecast Events: {forecast.Count}");
    }

    [ContextMenu("Clear All Cooldowns")]
    private void ClearCooldowns()
    {
        eventCooldowns.Clear();
        Debug.Log("[RandomEventSpawner] All event cooldowns cleared");
    }
#endif
}

/// <summary>
/// Event spawner statistics
/// </summary>
[System.Serializable]
public struct EventSpawnerStats
{
    public int totalEventsTriggered;
    public int eventsToday;
    public int dailyRollsPerformed;
    public int hourlyRollsPerformed;
    public int currentDay;
}

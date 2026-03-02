using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Agent 19: Dynamic Events Agent - EventCalendar.cs
/// Tracks event history and predicts upcoming events.
/// Integrates with journal system for player information.
/// </summary>
[System.Serializable]
public class EventHistory
{
    public string eventID;
    public int dayOccurred;
    public float realTimeOccurred;
    public bool wasCompleted;
    public int fishCaughtDuring;
    public int rareFishCaught;

    public EventHistory(string id, int day)
    {
        eventID = id;
        dayOccurred = day;
        realTimeOccurred = Time.time;
        wasCompleted = false;
        fishCaughtDuring = 0;
        rareFishCaught = 0;
    }
}

public class EventCalendar : MonoBehaviour
{
    private static EventCalendar _instance;
    public static EventCalendar Instance => _instance;

    [Header("Event History")]
    [SerializeField] private List<EventHistory> eventHistory = new List<EventHistory>();

    [Header("Upcoming Events")]
    [SerializeField] private List<EventPrediction> predictedEvents = new List<EventPrediction>();

    [Header("Statistics")]
    [SerializeField] private int totalEventsExperienced = 0;
    [SerializeField] private int bloodMoonsExperienced = 0;
    [SerializeField] private int meteorsExperienced = 0;
    [SerializeField] private int festivalsAttended = 0;
    [SerializeField] private int specialCatchesDuringEvents = 0;

    [Header("Configuration")]
    [Tooltip("How many days ahead to predict events")]
    [SerializeField] private int predictionDays = 3;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogging = true;

    private int currentDay = 0;

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
        // Subscribe to day completion events
        EventSystem.Subscribe("DayCompleted", OnDayCompleted);

        // Subscribe to event lifecycle events
        EventSystem.Subscribe<GameEvent>("EventStarted", OnEventStarted);
        EventSystem.Subscribe<GameEvent>("EventEnded", OnEventEnded);

        // Subscribe to fishing events during events
        EventSystem.Subscribe<Fish>("FishCaught", OnFishCaught);

        // Subscribe to save/load events
        EventSystem.Subscribe<SaveData>("GatheringSaveData", OnGatheringSaveData);
        EventSystem.Subscribe<SaveData>("ApplyingSaveData", OnApplyingSaveData);
    }

    private void OnDestroy()
    {
        EventSystem.Unsubscribe("DayCompleted", OnDayCompleted);
        EventSystem.Unsubscribe<GameEvent>("EventStarted", OnEventStarted);
        EventSystem.Unsubscribe<GameEvent>("EventEnded", OnEventEnded);
        EventSystem.Unsubscribe<Fish>("FishCaught", OnFishCaught);
        EventSystem.Unsubscribe<SaveData>("GatheringSaveData", OnGatheringSaveData);
        EventSystem.Unsubscribe<SaveData>("ApplyingSaveData", OnApplyingSaveData);

        if (_instance == this)
        {
            _instance = null;
        }
    }

    /// <summary>
    /// Records an event in the history
    /// </summary>
    public void RecordEvent(string eventID, int day)
    {
        EventHistory record = new EventHistory(eventID, day);
        eventHistory.Add(record);

        totalEventsExperienced++;

        if (enableDebugLogging)
        {
            Debug.Log($"[EventCalendar] Recorded event: {eventID} on day {day}");
        }
    }

    /// <summary>
    /// Gets the last time an event occurred
    /// </summary>
    public int GetDaysSinceLastOccurrence(string eventID)
    {
        EventHistory lastOccurrence = eventHistory
            .Where(e => e.eventID == eventID)
            .OrderByDescending(e => e.dayOccurred)
            .FirstOrDefault();

        if (lastOccurrence != null)
        {
            return currentDay - lastOccurrence.dayOccurred;
        }

        return int.MaxValue; // Never occurred
    }

    /// <summary>
    /// Gets the number of times an event has occurred
    /// </summary>
    public int GetEventOccurrenceCount(string eventID)
    {
        return eventHistory.Count(e => e.eventID == eventID);
    }

    /// <summary>
    /// Predicts when the next event of a type will occur
    /// </summary>
    public int PredictNextOccurrence(EventData eventData)
    {
        if (eventData.isStoryEvent)
        {
            return -1; // Story events can't be predicted
        }

        int daysSinceLast = GetDaysSinceLastOccurrence(eventData.eventID);

        // If minimum days haven't passed, return that day
        if (daysSinceLast < eventData.minimumDaysBetween)
        {
            int daysUntilEligible = eventData.minimumDaysBetween - daysSinceLast;
            return currentDay + daysUntilEligible;
        }

        // Otherwise, it's based on probability
        // Expected days = 1 / probability
        float expectedDays = 1f / Mathf.Max(0.01f, eventData.probability);
        return currentDay + Mathf.RoundToInt(expectedDays);
    }

    /// <summary>
    /// Updates predictions for upcoming events
    /// </summary>
    public void UpdatePredictions(List<EventData> allEvents)
    {
        predictedEvents.Clear();

        foreach (EventData eventData in allEvents)
        {
            if (eventData.isStoryEvent) continue;

            int predictedDay = PredictNextOccurrence(eventData);

            if (predictedDay <= currentDay + predictionDays && predictedDay >= currentDay)
            {
                EventPrediction prediction = new EventPrediction
                {
                    eventData = eventData,
                    predictedDay = predictedDay,
                    daysUntil = predictedDay - currentDay,
                    confidence = CalculatePredictionConfidence(eventData)
                };

                predictedEvents.Add(prediction);
            }
        }

        predictedEvents = predictedEvents.OrderBy(p => p.daysUntil).ToList();

        if (enableDebugLogging)
        {
            Debug.Log($"[EventCalendar] Updated predictions. {predictedEvents.Count} events predicted in next {predictionDays} days");
        }
    }

    /// <summary>
    /// Calculates how confident the prediction is
    /// </summary>
    private float CalculatePredictionConfidence(EventData eventData)
    {
        int occurrences = GetEventOccurrenceCount(eventData.eventID);

        if (occurrences == 0)
        {
            return 0.3f; // Low confidence, never seen before
        }
        else if (occurrences < 3)
        {
            return 0.6f; // Medium confidence
        }
        else
        {
            return 0.9f; // High confidence, seen multiple times
        }
    }

    /// <summary>
    /// Gets all upcoming predicted events
    /// </summary>
    public List<EventPrediction> GetUpcomingEvents()
    {
        return new List<EventPrediction>(predictedEvents);
    }

    /// <summary>
    /// Gets events predicted for today
    /// </summary>
    public List<EventPrediction> GetTodaysEvents()
    {
        return predictedEvents.Where(p => p.daysUntil == 0).ToList();
    }

    /// <summary>
    /// Gets total events experienced
    /// </summary>
    public int GetTotalEventsExperienced()
    {
        return totalEventsExperienced;
    }

    /// <summary>
    /// Gets special catches made during events
    /// </summary>
    public int GetSpecialCatchesDuringEvents()
    {
        return specialCatchesDuringEvents;
    }

    /// <summary>
    /// Gets statistics for specific event type
    /// </summary>
    public EventStatistics GetEventStatistics(string eventID)
    {
        List<EventHistory> eventRecords = eventHistory.Where(e => e.eventID == eventID).ToList();

        EventStatistics stats = new EventStatistics
        {
            eventID = eventID,
            timesOccurred = eventRecords.Count,
            totalFishCaught = eventRecords.Sum(e => e.fishCaughtDuring),
            totalRareFishCaught = eventRecords.Sum(e => e.rareFishCaught),
            lastOccurrence = eventRecords.OrderByDescending(e => e.dayOccurred).FirstOrDefault()?.dayOccurred ?? -1
        };

        return stats;
    }

    /// <summary>
    /// Marks current event record as completed
    /// </summary>
    public void MarkEventCompleted(string eventID)
    {
        EventHistory record = eventHistory
            .Where(e => e.eventID == eventID)
            .OrderByDescending(e => e.dayOccurred)
            .FirstOrDefault();

        if (record != null)
        {
            record.wasCompleted = true;
        }
    }

    /// <summary>
    /// Increments fish caught during current event
    /// </summary>
    private void OnFishCaught(Fish fish)
    {
        if (EventManager.Instance == null) return;

        GameEvent currentEvent = EventManager.Instance.GetCurrentEvent();
        if (currentEvent == null) return;

        EventHistory record = eventHistory
            .Where(e => e.eventID == currentEvent.data.eventID)
            .OrderByDescending(e => e.dayOccurred)
            .FirstOrDefault();

        if (record != null)
        {
            record.fishCaughtDuring++;

            if (fish.rarity == FishRarity.Rare || fish.rarity == FishRarity.Legendary || fish.isAberrant)
            {
                record.rareFishCaught++;
                specialCatchesDuringEvents++;
            }
        }
    }

    private void OnDayCompleted()
    {
        currentDay++;

        if (enableDebugLogging)
        {
            Debug.Log($"[EventCalendar] Day {currentDay} completed");
        }
    }

    private void OnEventStarted(GameEvent gameEvent)
    {
        RecordEvent(gameEvent.data.eventID, currentDay);

        // Update specific counters
        switch (gameEvent.data.eventType)
        {
            case EventType.BloodMoon:
                bloodMoonsExperienced++;
                break;
            case EventType.MeteorShower:
                meteorsExperienced++;
                break;
            case EventType.Festival:
                festivalsAttended++;
                break;
        }
    }

    private void OnEventEnded(GameEvent gameEvent)
    {
        MarkEventCompleted(gameEvent.data.eventID);
    }

    #region Save/Load Integration

    private void OnGatheringSaveData(SaveData data)
    {
        EventCalendarSaveData calendarData = new EventCalendarSaveData
        {
            currentDay = this.currentDay,
            totalEventsExperienced = this.totalEventsExperienced,
            bloodMoonsExperienced = this.bloodMoonsExperienced,
            meteorsExperienced = this.meteorsExperienced,
            festivalsAttended = this.festivalsAttended,
            specialCatchesDuringEvents = this.specialCatchesDuringEvents,
            eventHistory = this.eventHistory
        };

        data.eventCalendarData = JsonUtility.ToJson(calendarData);
    }

    private void OnApplyingSaveData(SaveData data)
    {
        if (string.IsNullOrEmpty(data.eventCalendarData)) return;

        EventCalendarSaveData calendarData = JsonUtility.FromJson<EventCalendarSaveData>(data.eventCalendarData);

        this.currentDay = calendarData.currentDay;
        this.totalEventsExperienced = calendarData.totalEventsExperienced;
        this.bloodMoonsExperienced = calendarData.bloodMoonsExperienced;
        this.meteorsExperienced = calendarData.meteorsExperienced;
        this.festivalsAttended = calendarData.festivalsAttended;
        this.specialCatchesDuringEvents = calendarData.specialCatchesDuringEvents;
        this.eventHistory = calendarData.eventHistory ?? new List<EventHistory>();

        if (enableDebugLogging)
        {
            Debug.Log($"[EventCalendar] Loaded calendar data. Current day: {currentDay}");
        }
    }

    #endregion
}

/// <summary>
/// Prediction for an upcoming event
/// </summary>
[System.Serializable]
public class EventPrediction
{
    public EventData eventData;
    public int predictedDay;
    public int daysUntil;
    public float confidence; // 0-1
}

/// <summary>
/// Statistics for a specific event
/// </summary>
[System.Serializable]
public class EventStatistics
{
    public string eventID;
    public int timesOccurred;
    public int totalFishCaught;
    public int totalRareFishCaught;
    public int lastOccurrence;
}

/// <summary>
/// Save data structure for event calendar
/// </summary>
[System.Serializable]
public class EventCalendarSaveData
{
    public int currentDay;
    public int totalEventsExperienced;
    public int bloodMoonsExperienced;
    public int meteorsExperienced;
    public int festivalsAttended;
    public int specialCatchesDuringEvents;
    public List<EventHistory> eventHistory;
}

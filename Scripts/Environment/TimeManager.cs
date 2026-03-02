using UnityEngine;

/// <summary>
/// Manages the in-game time system with configurable day/night cycle speed
/// Default: 15 minutes for full 24-hour cycle
/// Tracks time periods (Day, Dusk, Night, Dawn) and publishes time change events
/// </summary>
public class TimeManager : MonoBehaviour
{
    [Header("Time Settings")]
    [Tooltip("Real-time minutes for a full 24-hour game cycle")]
    public float dayLengthMinutes = 15f; // Default from VIDEO_ANALYSIS.md

    [Tooltip("Starting time in 24-hour format (0-24)")]
    [Range(0f, 24f)]
    public float startingTime = 6f; // Start at dawn

    [Header("Time Configuration Presets")]
    public bool usePreset = false;
    public enum CycleSpeed { Fast10Min, Balanced15Min, Slow20Min }
    public CycleSpeed cyclePreset = CycleSpeed.Balanced15Min;

    [Header("Current Time (Runtime)")]
    [SerializeField] private float currentTime = 0f; // 0-24 hour format
    [SerializeField] private TimeOfDay currentTimeOfDay = TimeOfDay.Dawn;

    [Header("Time Period Thresholds")]
    [Tooltip("Hour when dawn begins (default: 5 AM)")]
    public float dawnStartHour = 5f;

    [Tooltip("Hour when day begins (default: 8 AM)")]
    public float dayStartHour = 8f;

    [Tooltip("Hour when dusk begins (default: 18 / 6 PM)")]
    public float duskStartHour = 18f;

    [Tooltip("Hour when night begins (default: 20 / 8 PM)")]
    public float nightStartHour = 20f;

    [Header("Time Control")]
    [Tooltip("Pause time progression")]
    public bool timePaused = false;

    [Tooltip("Time speed multiplier (1.0 = normal)")]
    [Range(0.1f, 10f)]
    public float timeSpeedMultiplier = 1f;

    // Private variables
    private float timeScale; // How fast game time progresses relative to real time
    private TimeOfDay previousTimeOfDay;

    // Singleton pattern
    private static TimeManager _instance;
    public static TimeManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<TimeManager>();
            }
            return _instance;
        }
    }

    private void Awake()
    {
        // Singleton setup
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
        // Apply preset if enabled
        if (usePreset)
        {
            ApplyPreset();
        }

        // Calculate time scale: how many game hours pass per real second
        // Formula: 24 game hours / (dayLengthMinutes * 60 seconds)
        timeScale = 24f / (dayLengthMinutes * 60f);

        // Set starting time
        currentTime = startingTime;
        previousTimeOfDay = currentTimeOfDay;

        // Determine initial time of day
        UpdateTimeOfDay();

        Debug.Log($"TimeManager initialized: {dayLengthMinutes} min cycle, starting at {currentTime:F1}:00");
    }

    private void ApplyPreset()
    {
        switch (cyclePreset)
        {
            case CycleSpeed.Fast10Min:
                dayLengthMinutes = 10f;
                break;
            case CycleSpeed.Balanced15Min:
                dayLengthMinutes = 15f;
                break;
            case CycleSpeed.Slow20Min:
                dayLengthMinutes = 20f;
                break;
        }
    }

    private void Update()
    {
        if (!timePaused)
        {
            UpdateTime();
            UpdateTimeOfDay();
        }
    }

    /// <summary>
    /// Progress time forward based on real-time delta
    /// </summary>
    private void UpdateTime()
    {
        // Advance time with speed multiplier
        currentTime += Time.deltaTime * timeScale * timeSpeedMultiplier;

        // Wrap around at 24 hours
        if (currentTime >= 24f)
        {
            currentTime -= 24f;
            OnDayCompleted();
        }

        // Publish continuous time update event
        EventSystem.Publish("TimeUpdated", currentTime);
    }

    /// <summary>
    /// Check if time of day period has changed and update accordingly
    /// </summary>
    private void UpdateTimeOfDay()
    {
        TimeOfDay newTimeOfDay = DetermineTimeOfDay(currentTime);

        // If time period changed, publish event
        if (newTimeOfDay != currentTimeOfDay)
        {
            previousTimeOfDay = currentTimeOfDay;
            currentTimeOfDay = newTimeOfDay;
            OnTimeOfDayChanged();
        }
    }

    /// <summary>
    /// Determine which time period the current time falls into
    /// </summary>
    private TimeOfDay DetermineTimeOfDay(float time)
    {
        if (time >= dawnStartHour && time < dayStartHour)
            return TimeOfDay.Dawn;
        else if (time >= dayStartHour && time < duskStartHour)
            return TimeOfDay.Day;
        else if (time >= duskStartHour && time < nightStartHour)
            return TimeOfDay.Dusk;
        else
            return TimeOfDay.Night;
    }

    /// <summary>
    /// Called when time of day period changes
    /// </summary>
    private void OnTimeOfDayChanged()
    {
        Debug.Log($"Time of Day changed: {previousTimeOfDay} -> {currentTimeOfDay} at {currentTime:F1}:00");

        // Create event data
        TimeChangedEventData eventData = new TimeChangedEventData(currentTime, currentTimeOfDay);

        // Publish events
        EventSystem.Publish("TimeOfDayChanged", currentTimeOfDay);
        EventSystem.Publish("OnTimeChanged", eventData);
    }

    /// <summary>
    /// Called when a full 24-hour day completes
    /// </summary>
    private void OnDayCompleted()
    {
        Debug.Log("Day completed - New day beginning");
        EventSystem.Publish("DayCompleted");
    }

    #region Public Interface Methods

    /// <summary>
    /// Get current time of day period
    /// </summary>
    public TimeOfDay GetCurrentTimeOfDay()
    {
        return currentTimeOfDay;
    }

    /// <summary>
    /// Get current time in 24-hour format (0-24)
    /// </summary>
    public float GetCurrentTime()
    {
        return currentTime;
    }

    /// <summary>
    /// Get current hour (0-23)
    /// </summary>
    public int GetCurrentHour()
    {
        return Mathf.FloorToInt(currentTime);
    }

    /// <summary>
    /// Get current minute (0-59)
    /// </summary>
    public int GetCurrentMinute()
    {
        return Mathf.FloorToInt((currentTime - GetCurrentHour()) * 60f);
    }

    /// <summary>
    /// Get time as formatted string (HH:MM)
    /// </summary>
    public string GetTimeString()
    {
        int hour = GetCurrentHour();
        int minute = GetCurrentMinute();
        return $"{hour:D2}:{minute:D2}";
    }

    /// <summary>
    /// Get time as 12-hour format with AM/PM
    /// </summary>
    public string GetTime12HourString()
    {
        int hour = GetCurrentHour();
        int minute = GetCurrentMinute();
        string period = hour >= 12 ? "PM" : "AM";
        int hour12 = hour % 12;
        if (hour12 == 0) hour12 = 12;
        return $"{hour12}:{minute:D2} {period}";
    }

    /// <summary>
    /// Set time to a specific hour (0-24)
    /// </summary>
    public void SetTime(float time)
    {
        currentTime = Mathf.Clamp(time, 0f, 24f);
        UpdateTimeOfDay();
        Debug.Log($"Time set to: {GetTimeString()}");
    }

    /// <summary>
    /// Advance time by a specific number of hours
    /// </summary>
    public void AdvanceTime(float hours)
    {
        currentTime += hours;
        if (currentTime >= 24f)
        {
            currentTime -= 24f;
            OnDayCompleted();
        }
        UpdateTimeOfDay();
        Debug.Log($"Time advanced by {hours} hours to: {GetTimeString()}");
    }

    /// <summary>
    /// Check if it's currently daytime (safe period)
    /// </summary>
    public bool IsDaytime()
    {
        return currentTimeOfDay == TimeOfDay.Day || currentTimeOfDay == TimeOfDay.Dawn;
    }

    /// <summary>
    /// Check if it's currently nighttime (dangerous period)
    /// </summary>
    public bool IsNighttime()
    {
        return currentTimeOfDay == TimeOfDay.Night;
    }

    /// <summary>
    /// Check if it's a transition period (dusk or dawn)
    /// </summary>
    public bool IsTransitionTime()
    {
        return currentTimeOfDay == TimeOfDay.Dusk || currentTimeOfDay == TimeOfDay.Dawn;
    }

    /// <summary>
    /// Get normalized time (0.0 to 1.0) for the current day
    /// Useful for interpolating colors, light intensity, etc.
    /// </summary>
    public float GetNormalizedTime()
    {
        return currentTime / 24f;
    }

    /// <summary>
    /// Pause or resume time progression
    /// </summary>
    public void SetTimePaused(bool paused)
    {
        timePaused = paused;
        EventSystem.Publish("TimePausedChanged", paused);
    }

    /// <summary>
    /// Set the time speed multiplier
    /// </summary>
    public void SetTimeSpeed(float speedMultiplier)
    {
        timeSpeedMultiplier = Mathf.Max(0.1f, speedMultiplier);
        Debug.Log($"Time speed set to: {timeSpeedMultiplier}x");
    }

    /// <summary>
    /// Change the day length and recalculate time scale
    /// </summary>
    public void SetDayLength(float minutes)
    {
        dayLengthMinutes = Mathf.Max(1f, minutes);
        timeScale = 24f / (dayLengthMinutes * 60f);
        Debug.Log($"Day length set to: {dayLengthMinutes} minutes");
    }

    #endregion

    #region Debug and Testing

    /// <summary>
    /// Skip to a specific time of day (for testing)
    /// </summary>
    public void SkipToTimeOfDay(TimeOfDay targetTimeOfDay)
    {
        switch (targetTimeOfDay)
        {
            case TimeOfDay.Dawn:
                SetTime(dawnStartHour);
                break;
            case TimeOfDay.Day:
                SetTime(dayStartHour);
                break;
            case TimeOfDay.Dusk:
                SetTime(duskStartHour);
                break;
            case TimeOfDay.Night:
                SetTime(nightStartHour);
                break;
        }
    }

    private void OnValidate()
    {
        // Ensure time thresholds are in logical order
        if (dawnStartHour >= dayStartHour) dawnStartHour = dayStartHour - 1f;
        if (dayStartHour >= duskStartHour) dayStartHour = duskStartHour - 1f;
        if (duskStartHour >= nightStartHour) duskStartHour = nightStartHour - 1f;
    }

    #endregion
}

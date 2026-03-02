using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Displays current in-game time with day/night indicator.
/// Shows time in 12-hour or 24-hour format with sun/moon icon.
/// Position: Top-center of screen.
/// </summary>
public class TimeDisplay : MonoBehaviour
{
    #region Inspector References
    [Header("UI Elements")]
    [Tooltip("Text component showing the current time")]
    public TextMeshProUGUI timeText;

    [Tooltip("Text component showing the time period label (Day, Dusk, Night, Dawn)")]
    public TextMeshProUGUI periodLabel;

    [Tooltip("Image component for day/night icon")]
    public Image timeIcon;

    [Header("Icons")]
    [Tooltip("Sun icon for daytime")]
    public Sprite sunIcon;

    [Tooltip("Moon icon for nighttime")]
    public Sprite moonIcon;

    [Tooltip("Sunrise icon for dawn")]
    public Sprite sunriseIcon;

    [Tooltip("Sunset icon for dusk")]
    public Sprite sunsetIcon;

    [Header("Settings")]
    [Tooltip("Use 12-hour format (AM/PM) instead of 24-hour")]
    public bool use12HourFormat = true;

    [Tooltip("Show the time period label")]
    public bool showPeriodLabel = true;

    [Tooltip("Show the day/night icon")]
    public bool showIcon = true;

    [Header("Colors")]
    [Tooltip("Text color during daytime")]
    public Color dayColor = Color.white;

    [Tooltip("Text color during nighttime")]
    public Color nightColor = new Color(0.7f, 0.8f, 1f, 1f); // Light blue

    [Tooltip("Text color during transition periods")]
    public Color transitionColor = new Color(1f, 0.9f, 0.6f, 1f); // Golden

    [Header("Animation")]
    [Tooltip("Smoothly transition colors")]
    public bool smoothColorTransition = true;

    [Tooltip("Color transition speed")]
    [Range(0.1f, 5f)]
    public float colorTransitionSpeed = 1f;

    [Header("Debug")]
    [Tooltip("Enable debug logging")]
    public bool enableDebugLogging = false;
    #endregion

    #region Private Variables
    private TimeOfDay currentTimeOfDay;
    private Color targetTextColor;
    private Color currentTextColor;
    #endregion

    #region Unity Lifecycle

    private void Start()
    {
        // Subscribe to time events
        EventSystem.Subscribe<float>("TimeUpdated", OnTimeUpdated);
        EventSystem.Subscribe<TimeOfDay>("TimeOfDayChanged", OnTimeOfDayChanged);

        // Initialize with current time
        if (TimeManager.Instance != null)
        {
            UpdateTimeDisplay(TimeManager.Instance.GetCurrentTime());
            UpdateTimeOfDayVisuals(TimeManager.Instance.GetCurrentTimeOfDay());
        }

        if (enableDebugLogging)
        {
            Debug.Log("[TimeDisplay] Initialized");
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        EventSystem.Unsubscribe<float>("TimeUpdated", OnTimeUpdated);
        EventSystem.Unsubscribe<TimeOfDay>("TimeOfDayChanged", OnTimeOfDayChanged);
    }

    private void Update()
    {
        // Smooth color transition
        if (smoothColorTransition && currentTextColor != targetTextColor)
        {
            currentTextColor = Color.Lerp(currentTextColor, targetTextColor, Time.deltaTime * colorTransitionSpeed);
            ApplyTextColor(currentTextColor);
        }
    }

    #endregion

    #region Event Handlers

    /// <summary>
    /// Called every frame when time updates
    /// </summary>
    private void OnTimeUpdated(float currentTime)
    {
        UpdateTimeDisplay(currentTime);
    }

    /// <summary>
    /// Called when time of day period changes
    /// </summary>
    private void OnTimeOfDayChanged(TimeOfDay newTimeOfDay)
    {
        UpdateTimeOfDayVisuals(newTimeOfDay);

        if (enableDebugLogging)
        {
            Debug.Log($"[TimeDisplay] Time of day changed to: {newTimeOfDay}");
        }
    }

    #endregion

    #region Display Updates

    /// <summary>
    /// Update the time text display
    /// </summary>
    private void UpdateTimeDisplay(float currentTime)
    {
        if (timeText == null)
            return;

        string timeString = use12HourFormat ?
            FormatTime12Hour(currentTime) :
            FormatTime24Hour(currentTime);

        timeText.text = timeString;
    }

    /// <summary>
    /// Update visuals based on time of day (icon, colors, label)
    /// </summary>
    private void UpdateTimeOfDayVisuals(TimeOfDay timeOfDay)
    {
        currentTimeOfDay = timeOfDay;

        // Update icon
        if (showIcon && timeIcon != null)
        {
            timeIcon.sprite = GetIconForTimeOfDay(timeOfDay);
            timeIcon.enabled = timeIcon.sprite != null;
        }

        // Update period label
        if (showPeriodLabel && periodLabel != null)
        {
            periodLabel.text = GetPeriodLabel(timeOfDay);
            periodLabel.enabled = true;
        }

        // Update colors
        targetTextColor = GetColorForTimeOfDay(timeOfDay);
        if (!smoothColorTransition)
        {
            currentTextColor = targetTextColor;
            ApplyTextColor(currentTextColor);
        }
    }

    #endregion

    #region Time Formatting

    /// <summary>
    /// Format time in 24-hour format (HH:MM)
    /// </summary>
    private string FormatTime24Hour(float time)
    {
        int hour = Mathf.FloorToInt(time);
        int minute = Mathf.FloorToInt((time - hour) * 60f);
        return $"{hour:D2}:{minute:D2}";
    }

    /// <summary>
    /// Format time in 12-hour format (H:MM AM/PM)
    /// </summary>
    private string FormatTime12Hour(float time)
    {
        int hour = Mathf.FloorToInt(time);
        int minute = Mathf.FloorToInt((time - hour) * 60f);

        string period = hour >= 12 ? "PM" : "AM";
        int hour12 = hour % 12;
        if (hour12 == 0) hour12 = 12;

        return $"{hour12}:{minute:D2} {period}";
    }

    #endregion

    #region Visual Helpers

    /// <summary>
    /// Get the appropriate icon for time of day
    /// </summary>
    private Sprite GetIconForTimeOfDay(TimeOfDay timeOfDay)
    {
        switch (timeOfDay)
        {
            case TimeOfDay.Day:
                return sunIcon;
            case TimeOfDay.Night:
                return moonIcon;
            case TimeOfDay.Dawn:
                return sunriseIcon != null ? sunriseIcon : sunIcon;
            case TimeOfDay.Dusk:
                return sunsetIcon != null ? sunsetIcon : moonIcon;
            default:
                return sunIcon;
        }
    }

    /// <summary>
    /// Get the period label text
    /// </summary>
    private string GetPeriodLabel(TimeOfDay timeOfDay)
    {
        switch (timeOfDay)
        {
            case TimeOfDay.Day:
                return "Day";
            case TimeOfDay.Night:
                return "Night";
            case TimeOfDay.Dawn:
                return "Dawn";
            case TimeOfDay.Dusk:
                return "Dusk";
            default:
                return "";
        }
    }

    /// <summary>
    /// Get the appropriate color for time of day
    /// </summary>
    private Color GetColorForTimeOfDay(TimeOfDay timeOfDay)
    {
        switch (timeOfDay)
        {
            case TimeOfDay.Day:
                return dayColor;
            case TimeOfDay.Night:
                return nightColor;
            case TimeOfDay.Dawn:
            case TimeOfDay.Dusk:
                return transitionColor;
            default:
                return dayColor;
        }
    }

    /// <summary>
    /// Apply color to all text elements
    /// </summary>
    private void ApplyTextColor(Color color)
    {
        if (timeText != null)
        {
            timeText.color = color;
        }

        if (periodLabel != null)
        {
            periodLabel.color = color;
        }

        if (timeIcon != null)
        {
            timeIcon.color = color;
        }
    }

    #endregion

    #region Public Interface

    /// <summary>
    /// Set whether to use 12-hour or 24-hour format
    /// </summary>
    public void SetTimeFormat(bool use12Hour)
    {
        use12HourFormat = use12Hour;
        if (TimeManager.Instance != null)
        {
            UpdateTimeDisplay(TimeManager.Instance.GetCurrentTime());
        }
    }

    /// <summary>
    /// Toggle period label visibility
    /// </summary>
    public void SetPeriodLabelVisible(bool visible)
    {
        showPeriodLabel = visible;
        if (periodLabel != null)
        {
            periodLabel.enabled = visible;
        }
    }

    /// <summary>
    /// Toggle icon visibility
    /// </summary>
    public void SetIconVisible(bool visible)
    {
        showIcon = visible;
        if (timeIcon != null)
        {
            timeIcon.enabled = visible && timeIcon.sprite != null;
        }
    }

    #endregion

    #region Debug Methods

    /// <summary>
    /// Test different time periods
    /// </summary>
    [ContextMenu("Test Dawn")]
    private void TestDawn()
    {
        UpdateTimeOfDayVisuals(TimeOfDay.Dawn);
        UpdateTimeDisplay(6f);
    }

    [ContextMenu("Test Day")]
    private void TestDay()
    {
        UpdateTimeOfDayVisuals(TimeOfDay.Day);
        UpdateTimeDisplay(12f);
    }

    [ContextMenu("Test Dusk")]
    private void TestDusk()
    {
        UpdateTimeOfDayVisuals(TimeOfDay.Dusk);
        UpdateTimeDisplay(19f);
    }

    [ContextMenu("Test Night")]
    private void TestNight()
    {
        UpdateTimeOfDayVisuals(TimeOfDay.Night);
        UpdateTimeDisplay(23f);
    }

    #endregion
}

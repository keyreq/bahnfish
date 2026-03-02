using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages dynamic weather system with transitions and 3-day forecast
/// Influences gameplay: rain attracts fish, storms increase danger, fog reduces visibility
/// </summary>
public class WeatherSystem : MonoBehaviour
{
    [Header("Current Weather")]
    [SerializeField] private WeatherType currentWeather = WeatherType.Clear;
    [SerializeField] private float weatherIntensity = 0f; // 0.0 to 1.0

    [Header("Weather Transition")]
    [Tooltip("Time in seconds to transition between weather types")]
    [Range(1f, 60f)]
    public float transitionDuration = 10f;

    [Tooltip("Minimum duration for each weather state (in game hours)")]
    [Range(0.5f, 12f)]
    public float minWeatherDuration = 2f;

    [Tooltip("Maximum duration for each weather state (in game hours)")]
    [Range(1f, 24f)]
    public float maxWeatherDuration = 6f;

    [Header("Weather Probabilities")]
    [Range(0f, 1f)] public float clearProbability = 0.5f;
    [Range(0f, 1f)] public float rainProbability = 0.3f;
    [Range(0f, 1f)] public float stormProbability = 0.1f;
    [Range(0f, 1f)] public float fogProbability = 0.1f;

    [Header("3-Day Forecast")]
    [SerializeField] private List<WeatherForecast> forecast = new List<WeatherForecast>();

    [Tooltip("How many days ahead to forecast")]
    [Range(1, 7)]
    public int forecastDays = 3;

    [Header("Weather Effects Impact")]
    [Tooltip("Rain fish spawn rate multiplier")]
    [Range(0.5f, 3f)]
    public float rainFishMultiplier = 1.5f;

    [Tooltip("Storm rare fish spawn multiplier")]
    [Range(1f, 5f)]
    public float stormRareFishMultiplier = 2.5f;

    [Tooltip("Fog visibility range (meters)")]
    [Range(10f, 200f)]
    public float fogVisibilityRange = 50f;

    // Private variables
    private WeatherType targetWeather;
    private float transitionProgress = 1f; // 1 = fully transitioned
    private float weatherChangeTimer = 0f;
    private float nextWeatherChangeTime;
    private TimeManager timeManager;
    private bool isTransitioning = false;

    // Random seed for weather generation
    private int weatherSeed;

    // Singleton pattern
    private static WeatherSystem _instance;
    public static WeatherSystem Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<WeatherSystem>();
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
        // Get TimeManager reference
        timeManager = TimeManager.Instance;
        if (timeManager == null)
        {
            Debug.LogWarning("WeatherSystem: TimeManager not found. Some features may not work.");
        }

        // Initialize random seed (could be based on world seed in full game)
        weatherSeed = System.DateTime.Now.Millisecond;
        Random.InitState(weatherSeed);

        // Set initial weather
        currentWeather = WeatherType.Clear;
        targetWeather = currentWeather;
        weatherIntensity = 1f;
        transitionProgress = 1f;

        // Schedule first weather change
        ScheduleNextWeatherChange();

        // Generate initial forecast
        GenerateForecast();

        // Subscribe to day completion event to update forecast
        EventSystem.Subscribe("DayCompleted", OnDayCompleted);

        Debug.Log($"WeatherSystem initialized. Current weather: {currentWeather}");
    }

    private void Update()
    {
        UpdateWeatherTransition();
        UpdateWeatherTimer();
    }

    /// <summary>
    /// Update smooth transition between weather states
    /// </summary>
    private void UpdateWeatherTransition()
    {
        if (isTransitioning)
        {
            transitionProgress += Time.deltaTime / transitionDuration;

            if (transitionProgress >= 1f)
            {
                // Transition complete
                transitionProgress = 1f;
                isTransitioning = false;
                currentWeather = targetWeather;
                weatherIntensity = 1f;

                Debug.Log($"Weather transition complete. Current weather: {currentWeather}");
            }
            else
            {
                // Update intensity during transition
                weatherIntensity = transitionProgress;
            }
        }
    }

    /// <summary>
    /// Update timer for next weather change
    /// </summary>
    private void UpdateWeatherTimer()
    {
        if (timeManager != null)
        {
            weatherChangeTimer += Time.deltaTime * (24f / (timeManager.dayLengthMinutes * 60f));

            // Check if it's time to change weather
            if (weatherChangeTimer >= nextWeatherChangeTime)
            {
                ChangeToRandomWeather();
                ScheduleNextWeatherChange();
            }
        }
    }

    /// <summary>
    /// Schedule the next weather change
    /// </summary>
    private void ScheduleNextWeatherChange()
    {
        weatherChangeTimer = 0f;
        nextWeatherChangeTime = Random.Range(minWeatherDuration, maxWeatherDuration);
        Debug.Log($"Next weather change scheduled in {nextWeatherChangeTime:F1} game hours");
    }

    /// <summary>
    /// Change to a random weather type based on probabilities
    /// </summary>
    private void ChangeToRandomWeather()
    {
        // Normalize probabilities
        float totalProbability = clearProbability + rainProbability + stormProbability + fogProbability;
        float roll = Random.Range(0f, totalProbability);

        WeatherType newWeather;

        if (roll < clearProbability)
            newWeather = WeatherType.Clear;
        else if (roll < clearProbability + rainProbability)
            newWeather = WeatherType.Rain;
        else if (roll < clearProbability + rainProbability + stormProbability)
            newWeather = WeatherType.Storm;
        else
            newWeather = WeatherType.Fog;

        SetWeather(newWeather);
    }

    /// <summary>
    /// Set weather to a specific type with transition
    /// </summary>
    public void SetWeather(WeatherType newWeather, bool immediate = false)
    {
        if (newWeather == currentWeather && !immediate)
            return;

        WeatherType previousWeather = currentWeather;
        targetWeather = newWeather;

        if (immediate)
        {
            currentWeather = newWeather;
            weatherIntensity = 1f;
            transitionProgress = 1f;
            isTransitioning = false;
        }
        else
        {
            transitionProgress = 0f;
            isTransitioning = true;
        }

        // Publish weather change event
        WeatherChangedEventData eventData = new WeatherChangedEventData(previousWeather, newWeather);
        EventSystem.Publish("OnWeatherChanged", eventData);
        EventSystem.Publish("WeatherChanged", newWeather);

        Debug.Log($"Weather changing: {previousWeather} -> {newWeather}");
    }

    #region 3-Day Forecast System

    /// <summary>
    /// Generate weather forecast for the next N days
    /// </summary>
    private void GenerateForecast()
    {
        forecast.Clear();

        // Use current weather seed for consistency
        Random.State originalState = Random.state;
        Random.InitState(weatherSeed + GetCurrentDay());

        for (int day = 1; day <= forecastDays; day++)
        {
            WeatherType predictedWeather = PredictWeatherForDay(day);

            // Forecast confidence decreases with distance
            float confidence = 1f - (day * 0.15f);
            confidence = Mathf.Max(confidence, 0.5f);

            forecast.Add(new WeatherForecast(day, predictedWeather, confidence));
        }

        Random.state = originalState;

        Debug.Log($"Generated {forecastDays}-day weather forecast");
    }

    /// <summary>
    /// Predict weather for a specific day in the future
    /// </summary>
    private WeatherType PredictWeatherForDay(int daysAhead)
    {
        // Simple prediction based on random with weighted probabilities
        float totalProbability = clearProbability + rainProbability + stormProbability + fogProbability;
        float roll = Random.Range(0f, totalProbability);

        if (roll < clearProbability)
            return WeatherType.Clear;
        else if (roll < clearProbability + rainProbability)
            return WeatherType.Rain;
        else if (roll < clearProbability + rainProbability + stormProbability)
            return WeatherType.Storm;
        else
            return WeatherType.Fog;
    }

    /// <summary>
    /// Get the forecast list
    /// </summary>
    public List<WeatherForecast> GetForecast()
    {
        return new List<WeatherForecast>(forecast);
    }

    /// <summary>
    /// Get forecast for a specific day
    /// </summary>
    public WeatherForecast GetForecastForDay(int day)
    {
        if (day >= 1 && day <= forecast.Count)
        {
            return forecast[day - 1];
        }
        return null;
    }

    /// <summary>
    /// Called when a day completes - update forecast
    /// </summary>
    private void OnDayCompleted()
    {
        weatherSeed++; // Advance seed for next day
        GenerateForecast();
        Debug.Log("Forecast updated for new day");
    }

    /// <summary>
    /// Get current day number (for forecast seeding)
    /// </summary>
    private int GetCurrentDay()
    {
        if (timeManager != null)
        {
            // Could track actual day count in TimeManager
            // For now, use a simple calculation
            return Mathf.FloorToInt(Time.time / (timeManager.dayLengthMinutes * 60f));
        }
        return 0;
    }

    #endregion

    #region Public Interface Methods

    /// <summary>
    /// Get current weather type
    /// </summary>
    public WeatherType GetCurrentWeather()
    {
        return currentWeather;
    }

    /// <summary>
    /// Get weather intensity (0-1, useful for transitions)
    /// </summary>
    public float GetWeatherIntensity()
    {
        return weatherIntensity;
    }

    /// <summary>
    /// Check if weather is currently transitioning
    /// </summary>
    public bool IsTransitioning()
    {
        return isTransitioning;
    }

    /// <summary>
    /// Get the target weather (what we're transitioning to)
    /// </summary>
    public WeatherType GetTargetWeather()
    {
        return targetWeather;
    }

    /// <summary>
    /// Get transition progress (0-1)
    /// </summary>
    public float GetTransitionProgress()
    {
        return transitionProgress;
    }

    /// <summary>
    /// Get fish spawn rate multiplier based on current weather
    /// </summary>
    public float GetFishSpawnMultiplier()
    {
        switch (currentWeather)
        {
            case WeatherType.Rain:
                return rainFishMultiplier;
            case WeatherType.Storm:
                return stormRareFishMultiplier;
            case WeatherType.Fog:
                return 1.2f; // Slight increase in fog
            case WeatherType.Clear:
            default:
                return 1f;
        }
    }

    /// <summary>
    /// Get visibility range based on current weather (for fog system)
    /// </summary>
    public float GetVisibilityRange()
    {
        switch (currentWeather)
        {
            case WeatherType.Fog:
                return fogVisibilityRange * weatherIntensity;
            case WeatherType.Storm:
                return 100f; // Reduced visibility in storms
            case WeatherType.Rain:
                return 150f; // Slightly reduced
            case WeatherType.Clear:
            default:
                return 1000f; // Maximum visibility
        }
    }

    /// <summary>
    /// Check if it's currently raining (rain or storm)
    /// </summary>
    public bool IsRaining()
    {
        return currentWeather == WeatherType.Rain || currentWeather == WeatherType.Storm;
    }

    /// <summary>
    /// Check if there's a storm
    /// </summary>
    public bool IsStormy()
    {
        return currentWeather == WeatherType.Storm;
    }

    /// <summary>
    /// Check if it's foggy
    /// </summary>
    public bool IsFoggy()
    {
        return currentWeather == WeatherType.Fog;
    }

    /// <summary>
    /// Check if weather is clear
    /// </summary>
    public bool IsClear()
    {
        return currentWeather == WeatherType.Clear;
    }

    /// <summary>
    /// Force a specific weather for testing
    /// </summary>
    public void ForceWeather(WeatherType weather)
    {
        SetWeather(weather, immediate: true);
        Debug.Log($"Weather forced to: {weather}");
    }

    #endregion

    private void OnDestroy()
    {
        // Unsubscribe from events
        EventSystem.Unsubscribe("DayCompleted", OnDayCompleted);
    }

    #region Debug

    private void OnValidate()
    {
        // Normalize probabilities warning
        float total = clearProbability + rainProbability + stormProbability + fogProbability;
        if (Mathf.Abs(total - 1f) > 0.01f && total > 0)
        {
            // Auto-normalize
            float normalizer = 1f / total;
            clearProbability *= normalizer;
            rainProbability *= normalizer;
            stormProbability *= normalizer;
            fogProbability *= normalizer;
        }
    }

    #endregion
}

using UnityEngine;

/// <summary>
/// Example integration showing how other systems should interact with the Events system.
/// This demonstrates best practices for subscribing to events, reacting to changes,
/// and coordinating with the event managers.
///
/// Agent 19: Dynamic Events Agent
/// </summary>
public class EventsIntegrationExample : MonoBehaviour
{
    [Header("Component References")]
    private EventManager eventManager;
    private EventCalendar eventCalendar;
    private MigrationSystem migrationSystem;
    private FishManager fishManager;

    [Header("Runtime State")]
    private float currentSpawnMultiplier = 1f;
    private float currentRareMultiplier = 1f;
    private bool aberrantFishForced = false;

    private void Start()
    {
        // Get references to event systems
        eventManager = EventManager.Instance;
        eventCalendar = EventCalendar.Instance;
        migrationSystem = MigrationSystem.Instance;
        fishManager = FishManager.Instance;

        // Subscribe to event lifecycle
        SubscribeToEventLifecycle();

        // Subscribe to specific event effects
        SubscribeToEventEffects();

        // Subscribe to calendar updates
        SubscribeToCalendarEvents();
    }

    #region Event Lifecycle Subscriptions

    private void SubscribeToEventLifecycle()
    {
        // Called when any event starts
        EventSystem.Subscribe<GameEvent>("EventStarted", OnEventStarted);

        // Called when any event ends
        EventSystem.Subscribe<GameEvent>("EventEnded", OnEventEnded);
    }

    private void OnEventStarted(GameEvent gameEvent)
    {
        Debug.Log($"[Integration] Event started: {gameEvent.data.eventName}");

        // React based on event type
        switch (gameEvent.data.eventType)
        {
            case EventType.BloodMoon:
                HandleBloodMoonStart();
                break;

            case EventType.MeteorShower:
                HandleMeteorShowerStart();
                break;

            case EventType.FogBank:
                HandleFogBankStart();
                break;

            case EventType.Festival:
                HandleFestivalStart();
                break;

            case EventType.Storm:
                HandleStormStart();
                break;
        }
    }

    private void OnEventEnded(GameEvent gameEvent)
    {
        Debug.Log($"[Integration] Event ended: {gameEvent.data.eventName}");

        // Clean up event-specific state
        switch (gameEvent.data.eventType)
        {
            case EventType.BloodMoon:
                HandleBloodMoonEnd();
                break;

            case EventType.MeteorShower:
                HandleMeteorShowerEnd();
                break;

            case EventType.FogBank:
                HandleFogBankEnd();
                break;

            case EventType.Festival:
                HandleFestivalEnd();
                break;
        }
    }

    #endregion

    #region Event Effect Subscriptions

    private void SubscribeToEventEffects()
    {
        // Spawn rate changes
        EventSystem.Subscribe<float>("EventSpawnMultiplierChanged", OnSpawnMultiplierChanged);
        EventSystem.Subscribe<float>("EventRareMultiplierChanged", OnRareMultiplierChanged);

        // Forced aberrant fish
        EventSystem.Subscribe<bool>("ForceAberrantFish", OnForceAberrantFish);

        // Weather overrides
        EventSystem.Subscribe<WeatherType>("OverrideWeather", OnWeatherOverride);

        // Notifications
        EventSystem.Subscribe<string>("ShowNotification", OnNotification);
    }

    private void OnSpawnMultiplierChanged(float multiplier)
    {
        currentSpawnMultiplier = multiplier;
        Debug.Log($"[Integration] Spawn multiplier changed to: {multiplier}x");

        // Example: Update your fish spawner
        if (fishManager != null)
        {
            // fishManager.SetSpawnMultiplier(multiplier);
        }
    }

    private void OnRareMultiplierChanged(float multiplier)
    {
        currentRareMultiplier = multiplier;
        Debug.Log($"[Integration] Rare fish multiplier changed to: {multiplier}x");
    }

    private void OnForceAberrantFish(bool forced)
    {
        aberrantFishForced = forced;
        Debug.Log($"[Integration] Force aberrant fish: {forced}");

        // Example: Make all fish spawns aberrant
        if (forced && fishManager != null)
        {
            // fishManager.SetForcedAberrantMode(true);
        }
        else if (fishManager != null)
        {
            // fishManager.SetForcedAberrantMode(false);
        }
    }

    private void OnWeatherOverride(WeatherType weather)
    {
        Debug.Log($"[Integration] Weather override: {weather}");

        // Example: Change weather system
        // WeatherSystem.Instance.ForceWeather(weather);
    }

    private void OnNotification(string message)
    {
        Debug.Log($"[Integration] Notification: {message}");

        // Example: Show in UI
        // NotificationManager.Instance.Show(message);
    }

    #endregion

    #region Calendar Event Subscriptions

    private void SubscribeToCalendarEvents()
    {
        // Day completion
        EventSystem.Subscribe("DayCompleted", OnDayCompleted);

        // Forecast updates
        EventSystem.Subscribe<System.Collections.Generic.List<EventPrediction>>("ForecastUpdated", OnForecastUpdated);
    }

    private void OnDayCompleted()
    {
        Debug.Log("[Integration] Day completed - checking for upcoming events");

        if (eventCalendar != null)
        {
            // Check today's predictions
            var todaysEvents = eventCalendar.GetTodaysEvents();
            foreach (var prediction in todaysEvents)
            {
                Debug.Log($"[Integration] Event predicted today: {prediction.eventData.eventName} (confidence: {prediction.confidence * 100}%)");
            }
        }
    }

    private void OnForecastUpdated(System.Collections.Generic.List<EventPrediction> forecast)
    {
        Debug.Log($"[Integration] Forecast updated: {forecast.Count} events predicted");

        // Example: Update journal UI with forecast
        foreach (var prediction in forecast)
        {
            Debug.Log($"  - {prediction.eventData.eventName} in {prediction.daysUntil} days ({prediction.confidence * 100}% confidence)");
        }
    }

    #endregion

    #region Specific Event Handlers

    private void HandleBloodMoonStart()
    {
        Debug.Log("[Integration] Blood Moon started - preparing for aberrant fish onslaught!");

        // Example actions:
        // - Increase sanity drain display
        // - Apply red post-processing
        // - Play ominous music
        // - Show warning to player
    }

    private void HandleBloodMoonEnd()
    {
        Debug.Log("[Integration] Blood Moon ended - returning to normal");

        // Example cleanup:
        // - Restore normal post-processing
        // - Resume normal music
        // - Show statistics to player
    }

    private void HandleMeteorShowerStart()
    {
        Debug.Log("[Integration] Meteor Shower started - rare fish incoming!");

        // Example actions:
        // - Spawn meteor particle effects
        // - Play uplifting music
        // - Show bonus notification
    }

    private void HandleMeteorShowerEnd()
    {
        Debug.Log("[Integration] Meteor Shower ended");

        // Example cleanup:
        // - Stop meteor effects
        // - Show collection summary
    }

    private void HandleFogBankStart()
    {
        Debug.Log("[Integration] Fog Bank appeared - limited visibility!");

        // Example actions:
        // - Reduce camera far clip plane
        // - Apply fog post-processing
        // - Muffle audio
        // - Enable fog-only fish
    }

    private void HandleFogBankEnd()
    {
        Debug.Log("[Integration] Fog Bank cleared");

        // Example cleanup:
        // - Restore camera settings
        // - Remove fog effects
        // - Restore audio
    }

    private void HandleFestivalStart()
    {
        Debug.Log("[Integration] Festival started!");

        // Example actions:
        // - Change NPC dialogues
        // - Activate decorations
        // - Apply shop discounts
        // - Play festival music
    }

    private void HandleFestivalEnd()
    {
        Debug.Log("[Integration] Festival ended");

        // Example cleanup:
        // - Restore NPC dialogues
        // - Remove decorations
        // - Remove shop bonuses
    }

    private void HandleStormStart()
    {
        Debug.Log("[Integration] Storm started!");

        // Example actions:
        // - Enable rain particles
        // - Add wave motion to boat
        // - Spawn lightning
    }

    #endregion

    #region Public API Examples

    /// <summary>
    /// Example: Check if specific event is active before doing something
    /// </summary>
    public void ExampleCheckActiveEvent()
    {
        // Check via EventManager
        if (eventManager != null && eventManager.IsEventActive("blood_moon"))
        {
            Debug.Log("Blood Moon is active!");
        }

        // Check via specific event system
        if (BloodMoonEvent.Instance?.IsActive() ?? false)
        {
            Debug.Log("Blood Moon confirmed active!");
        }
    }

    /// <summary>
    /// Example: Get current multipliers to apply to your system
    /// </summary>
    public float ExampleGetSpawnRate()
    {
        float baseRate = 1.0f;

        // Get event multiplier
        if (eventManager != null)
        {
            baseRate *= eventManager.GetSpawnMultiplier();
            baseRate *= eventManager.GetRareMultiplier();
        }

        // Get seasonal multiplier
        if (migrationSystem != null)
        {
            baseRate *= migrationSystem.GetSeasonalSpawnModifier("salmon");
        }

        return baseRate;
    }

    /// <summary>
    /// Example: Check fish availability with migrations
    /// </summary>
    public bool ExampleIsFishAvailable(string fishID)
    {
        if (migrationSystem == null) return true;

        return migrationSystem.IsFishAvailable(fishID);
    }

    /// <summary>
    /// Example: Get event statistics for display
    /// </summary>
    public void ExampleGetEventStats()
    {
        if (eventCalendar == null) return;

        int totalEvents = eventCalendar.GetTotalEventsExperienced();
        int specialCatches = eventCalendar.GetSpecialCatchesDuringEvents();

        Debug.Log($"Player has experienced {totalEvents} events");
        Debug.Log($"Player caught {specialCatches} special fish during events");

        // Get specific event stats
        var bloodMoonStats = eventCalendar.GetEventStatistics("blood_moon");
        Debug.Log($"Blood Moons: {bloodMoonStats.timesOccurred} times, {bloodMoonStats.totalRareFishCaught} rare fish");
    }

    /// <summary>
    /// Example: Query upcoming events for UI display
    /// </summary>
    public void ExampleGetUpcomingEvents()
    {
        if (eventCalendar == null) return;

        var upcoming = eventCalendar.GetUpcomingEvents();

        foreach (var prediction in upcoming)
        {
            string eventName = prediction.eventData.eventName;
            int daysUntil = prediction.daysUntil;
            float confidence = prediction.confidence * 100f;

            Debug.Log($"{eventName} predicted in {daysUntil} days ({confidence:F0}% confidence)");
        }
    }

    /// <summary>
    /// Example: Spawn fish with event modifiers applied
    /// </summary>
    public void ExampleSpawnFish(Vector3 position)
    {
        if (fishManager == null) return;

        // FishManager automatically applies event modifiers
        GameObject fish = fishManager.SpawnFish(position);

        // But you can check what modifiers are active
        float spawnMult = eventManager?.GetSpawnMultiplier() ?? 1f;
        float rareMult = eventManager?.GetRareMultiplier() ?? 1f;

        Debug.Log($"Spawned fish with modifiers: Spawn {spawnMult}x, Rare {rareMult}x");
    }

    #endregion

    #region Testing Helpers

    [ContextMenu("Test: Trigger Blood Moon")]
    private void TestTriggerBloodMoon()
    {
        eventManager?.ForceTriggerEvent("blood_moon");
    }

    [ContextMenu("Test: Trigger Meteor Shower")]
    private void TestTriggerMeteorShower()
    {
        eventManager?.ForceTriggerEvent("meteor_shower");
    }

    [ContextMenu("Test: Change to Winter")]
    private void TestChangeToWinter()
    {
        migrationSystem?.ForceSeasonChange(Season.Winter);
    }

    [ContextMenu("Test: Print Current Event Status")]
    private void TestPrintEventStatus()
    {
        if (eventManager == null)
        {
            Debug.Log("No EventManager found");
            return;
        }

        var currentEvent = eventManager.GetCurrentEvent();
        if (currentEvent != null)
        {
            Debug.Log($"Active Event: {currentEvent.data.eventName}");
            Debug.Log($"Time Remaining: {currentEvent.GetRemainingTime():F0} seconds");
            Debug.Log($"Progress: {currentEvent.GetProgress() * 100:F0}%");
        }
        else
        {
            Debug.Log("No active events");
        }

        Debug.Log($"Current Multipliers:");
        Debug.Log($"  Spawn: {eventManager.GetSpawnMultiplier()}x");
        Debug.Log($"  Rare: {eventManager.GetRareMultiplier()}x");
        Debug.Log($"  Hazard: {eventManager.GetHazardMultiplier()}x");
        Debug.Log($"  Sanity: {eventManager.GetSanityMultiplier()}x");
    }

    #endregion

    private void OnDestroy()
    {
        // CRITICAL: Always unsubscribe to prevent memory leaks!
        EventSystem.Unsubscribe<GameEvent>("EventStarted", OnEventStarted);
        EventSystem.Unsubscribe<GameEvent>("EventEnded", OnEventEnded);
        EventSystem.Unsubscribe<float>("EventSpawnMultiplierChanged", OnSpawnMultiplierChanged);
        EventSystem.Unsubscribe<float>("EventRareMultiplierChanged", OnRareMultiplierChanged);
        EventSystem.Unsubscribe<bool>("ForceAberrantFish", OnForceAberrantFish);
        EventSystem.Unsubscribe<WeatherType>("OverrideWeather", OnWeatherOverride);
        EventSystem.Unsubscribe<string>("ShowNotification", OnNotification);
        EventSystem.Unsubscribe("DayCompleted", OnDayCompleted);
        EventSystem.Unsubscribe<System.Collections.Generic.List<EventPrediction>>("ForecastUpdated", OnForecastUpdated);
    }
}

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Agent 20: Idle/AFK Progression System - OfflineEventSimulator.cs
/// Simulates random events that occur while player is offline.
/// Tracks weather changes, fish migrations, random events, and special occurrences.
/// Only positive or neutral events occur - no negative consequences for being offline.
/// </summary>
public class OfflineEventSimulator : MonoBehaviour
{
    [Header("Event Probabilities (per hour offline)")]
    [SerializeField] private float weatherChangeChance = 0.3f;      // 30% per hour
    [SerializeField] private float fishMigrationChance = 0.1f;      // 10% per hour
    [SerializeField] private float meteorShowerChance = 0.05f;      // 5% per hour
    [SerializeField] private float bloodMoonChance = 0.02f;         // 2% per hour
    [SerializeField] private float festivalChance = 0.08f;          // 8% per hour
    [SerializeField] private float fishSchoolChance = 0.2f;         // 20% per hour

    [Header("Event Bonuses")]
    [SerializeField] private float bloodMoonValueMultiplier = 10f;  // 10x fish value during Blood Moon
    [SerializeField] private float meteorShowerRelicChance = 0.3f;  // 30% chance per meteor shower
    [SerializeField] private int maxMeteorsPerShower = 3;

    [Header("Configuration")]
    [Tooltip("Maximum number of events to simulate (prevents spam for very long offline periods)")]
    [SerializeField] private int maxEventsToSimulate = 10;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogging = false;

    // Component references
    private IdleUpgradeSystem upgradeSystem;

    private void Awake()
    {
        upgradeSystem = GetComponent<IdleUpgradeSystem>();
        if (upgradeSystem == null)
        {
            Debug.LogWarning("[OfflineEventSimulator] IdleUpgradeSystem not found!");
        }
    }

    /// <summary>
    /// Simulates events that occurred during offline time.
    /// </summary>
    /// <param name="hoursOffline">Hours player was offline</param>
    /// <returns>List of event descriptions</returns>
    public List<string> SimulateOfflineEvents(float hoursOffline)
    {
        List<string> events = new List<string>();

        // Simulate hourly events
        int hoursToSimulate = Mathf.Min(Mathf.FloorToInt(hoursOffline), 48); // Cap at 48 hours for performance

        int weatherChanges = 0;
        int fishMigrations = 0;
        int meteorShowers = 0;
        int bloodMoons = 0;
        int festivals = 0;
        int fishSchools = 0;

        // Simulate each hour
        for (int hour = 0; hour < hoursToSimulate; hour++)
        {
            // Weather changes
            if (Random.value < weatherChangeChance)
            {
                weatherChanges++;
            }

            // Fish migrations
            if (Random.value < fishMigrationChance)
            {
                fishMigrations++;
            }

            // Meteor showers
            if (Random.value < meteorShowerChance)
            {
                meteorShowers++;
            }

            // Blood Moon (rare)
            if (Random.value < bloodMoonChance)
            {
                bloodMoons++;
            }

            // Festival
            if (Random.value < festivalChance)
            {
                festivals++;
            }

            // Fish school
            if (Random.value < fishSchoolChance)
            {
                fishSchools++;
            }

            // Stop if we've hit max events
            int totalEvents = weatherChanges + fishMigrations + meteorShowers + bloodMoons + festivals + fishSchools;
            if (totalEvents >= maxEventsToSimulate)
            {
                break;
            }
        }

        // Generate event descriptions (only for significant events)

        // Blood Moon (most significant)
        if (bloodMoons > 0)
        {
            if (bloodMoons == 1)
            {
                events.Add($"Blood Moon occurred ({GetRandomTimeAgo(hoursOffline)}) - Extra fish sold for {bloodMoonValueMultiplier}x value!");
            }
            else
            {
                events.Add($"{bloodMoons} Blood Moons occurred - Massive earnings boost!");
            }
        }

        // Meteor Shower
        if (meteorShowers > 0)
        {
            int meteorsCollected = 0;
            for (int i = 0; i < meteorShowers; i++)
            {
                if (Random.value < meteorShowerRelicChance)
                {
                    meteorsCollected += Random.Range(1, maxMeteorsPerShower + 1);
                }
            }

            if (meteorsCollected > 0)
            {
                events.Add($"Meteor Shower occurred - Would have collected {meteorsCollected} meteorite{(meteorsCollected > 1 ? "s" : "")} (active play only)");
            }
            else
            {
                events.Add($"Meteor Shower occurred ({GetRandomTimeAgo(hoursOffline)})");
            }
        }

        // Festival
        if (festivals > 0)
        {
            if (festivals == 1)
            {
                // Check if festival is still active
                if (hoursOffline < 12f)
                {
                    events.Add($"Festival started ({GetRandomTimeAgo(hoursOffline)}) - Still active!");
                }
                else
                {
                    events.Add($"Festival occurred while you were away");
                }
            }
            else
            {
                events.Add($"{festivals} Festivals occurred during your absence");
            }
        }

        // Fish Migration
        if (fishMigrations > 0)
        {
            if (fishMigrations == 1)
            {
                events.Add($"Fish migration event ({GetRandomTimeAgo(hoursOffline)}) - New species available");
            }
            else
            {
                events.Add($"{fishMigrations} Fish migration events occurred");
            }
        }

        // Fish Schools
        if (fishSchools > 0 && fishSchools <= 2)
        {
            events.Add($"Fish school appeared ({GetRandomTimeAgo(hoursOffline)})");
        }

        // Weather changes (only mention if significant)
        if (weatherChanges >= 3)
        {
            events.Add($"Weather changed {weatherChanges} times during your absence");
        }

        // Breeding completions (if breeding automation is enabled)
        if (upgradeSystem != null && upgradeSystem.HasUpgrade("breeding_automation"))
        {
            int breedingCompletions = SimulateBreedingCompletions(hoursOffline);
            if (breedingCompletions > 0)
            {
                events.Add($"{breedingCompletions} fish breeding pair{(breedingCompletions > 1 ? "s" : "")} completed");
            }
        }

        // Crew activities (if crew autonomy is enabled)
        if (upgradeSystem != null && upgradeSystem.HasUpgrade("crew_autonomy"))
        {
            events.Add("Crew worked diligently while you were away");
        }

        if (enableDebugLogging)
        {
            Debug.Log($"[OfflineEventSimulator] Simulated {hoursOffline:F2}h offline:");
            Debug.Log($"  Weather Changes: {weatherChanges}");
            Debug.Log($"  Fish Migrations: {fishMigrations}");
            Debug.Log($"  Meteor Showers: {meteorShowers}");
            Debug.Log($"  Blood Moons: {bloodMoons}");
            Debug.Log($"  Festivals: {festivals}");
            Debug.Log($"  Fish Schools: {fishSchools}");
            Debug.Log($"  Event Messages: {events.Count}");
        }

        return events;
    }

    /// <summary>
    /// Simulates breeding completions during offline time.
    /// </summary>
    /// <param name="hoursOffline">Hours offline</param>
    /// <returns>Number of breeding pairs completed</returns>
    private int SimulateBreedingCompletions(float hoursOffline)
    {
        // Breeding takes ~24 hours per pair
        float breedingTimeHours = 24f;
        int completions = Mathf.FloorToInt(hoursOffline / breedingTimeHours);

        // TODO: Integrate with actual breeding system
        // For now, simulate 0-2 completions per 24 hours
        int simulatedPairs = Random.Range(0, 3);
        return Mathf.Min(completions * simulatedPairs, 5); // Cap at 5
    }

    /// <summary>
    /// Generates a random "time ago" description.
    /// </summary>
    /// <param name="maxHours">Maximum hours offline</param>
    /// <returns>Time ago string</returns>
    private string GetRandomTimeAgo(float maxHours)
    {
        float hoursAgo = Random.Range(0.5f, maxHours);

        if (hoursAgo < 1f)
        {
            int minutes = Mathf.FloorToInt(hoursAgo * 60f);
            return $"{minutes} minutes ago";
        }
        else if (hoursAgo < 24f)
        {
            int hours = Mathf.FloorToInt(hoursAgo);
            return $"{hours} hour{(hours > 1 ? "s" : "")} ago";
        }
        else
        {
            int days = Mathf.FloorToInt(hoursAgo / 24f);
            return $"{days} day{(days > 1 ? "s" : "")} ago";
        }
    }

    /// <summary>
    /// Checks if a specific event type occurred during offline time.
    /// </summary>
    /// <param name="eventType">Event type to check</param>
    /// <param name="hoursOffline">Hours offline</param>
    /// <returns>True if event likely occurred</returns>
    public bool DidEventOccur(string eventType, float hoursOffline)
    {
        float chance = 0f;

        switch (eventType.ToLower())
        {
            case "blood_moon":
                chance = bloodMoonChance;
                break;
            case "meteor_shower":
                chance = meteorShowerChance;
                break;
            case "festival":
                chance = festivalChance;
                break;
            case "fish_migration":
                chance = fishMigrationChance;
                break;
            case "fish_school":
                chance = fishSchoolChance;
                break;
            case "weather_change":
                chance = weatherChangeChance;
                break;
            default:
                return false;
        }

        // Calculate probability over entire offline period
        float totalProbability = 1f - Mathf.Pow(1f - chance, hoursOffline);
        return Random.value < totalProbability;
    }

    /// <summary>
    /// Estimates the number of events for a given time period.
    /// </summary>
    /// <param name="hours">Hours to estimate</param>
    /// <returns>Estimated number of events</returns>
    public int EstimateEventCount(float hours)
    {
        int hoursToCheck = Mathf.FloorToInt(hours);
        int estimatedEvents = 0;

        estimatedEvents += Mathf.FloorToInt(hoursToCheck * weatherChangeChance);
        estimatedEvents += Mathf.FloorToInt(hoursToCheck * fishMigrationChance);
        estimatedEvents += Mathf.FloorToInt(hoursToCheck * meteorShowerChance);
        estimatedEvents += Mathf.FloorToInt(hoursToCheck * bloodMoonChance);
        estimatedEvents += Mathf.FloorToInt(hoursToCheck * festivalChance);
        estimatedEvents += Mathf.FloorToInt(hoursToCheck * fishSchoolChance);

        return Mathf.Min(estimatedEvents, maxEventsToSimulate);
    }

#if UNITY_EDITOR
    [ContextMenu("Test 1 Hour Events")]
    private void TestOneHour()
    {
        var events = SimulateOfflineEvents(1f);
        Debug.Log($"[OfflineEventSimulator] 1 Hour Events ({events.Count}):");
        PrintEvents(events);
    }

    [ContextMenu("Test 8 Hours Events")]
    private void TestEightHours()
    {
        var events = SimulateOfflineEvents(8f);
        Debug.Log($"[OfflineEventSimulator] 8 Hours Events ({events.Count}):");
        PrintEvents(events);
    }

    [ContextMenu("Test 24 Hours Events")]
    private void TestTwentyFourHours()
    {
        var events = SimulateOfflineEvents(24f);
        Debug.Log($"[OfflineEventSimulator] 24 Hours Events ({events.Count}):");
        PrintEvents(events);
    }

    [ContextMenu("Test 48 Hours Events")]
    private void TestFortyEightHours()
    {
        var events = SimulateOfflineEvents(48f);
        Debug.Log($"[OfflineEventSimulator] 48 Hours Events ({events.Count}):");
        PrintEvents(events);
    }

    [ContextMenu("Estimate Events for Different Durations")]
    private void EstimateEvents()
    {
        Debug.Log("=== Event Occurrence Estimates ===");
        float[] durations = { 1f, 4f, 8f, 12f, 24f, 48f };

        foreach (float duration in durations)
        {
            int estimate = EstimateEventCount(duration);
            Debug.Log($"{duration}h → ~{estimate} events");
        }
    }

    [ContextMenu("Test Event Probabilities")]
    private void TestEventProbabilities()
    {
        Debug.Log("=== Event Probability Test (100 simulations, 24h each) ===");

        Dictionary<string, int> eventCounts = new Dictionary<string, int>
        {
            { "Blood Moon", 0 },
            { "Meteor Shower", 0 },
            { "Festival", 0 },
            { "Fish Migration", 0 },
            { "Breeding", 0 }
        };

        int simulations = 100;
        for (int i = 0; i < simulations; i++)
        {
            var events = SimulateOfflineEvents(24f);

            foreach (string eventMsg in events)
            {
                if (eventMsg.Contains("Blood Moon")) eventCounts["Blood Moon"]++;
                if (eventMsg.Contains("Meteor Shower")) eventCounts["Meteor Shower"]++;
                if (eventMsg.Contains("Festival")) eventCounts["Festival"]++;
                if (eventMsg.Contains("migration")) eventCounts["Fish Migration"]++;
                if (eventMsg.Contains("breeding")) eventCounts["Breeding"]++;
            }
        }

        Debug.Log($"Results from {simulations} simulations:");
        foreach (var kvp in eventCounts)
        {
            float percentage = (kvp.Value / (float)simulations) * 100f;
            Debug.Log($"{kvp.Key}: {kvp.Value}/{simulations} ({percentage:F1}%)");
        }
    }

    private void PrintEvents(List<string> events)
    {
        if (events.Count == 0)
        {
            Debug.Log("  No significant events occurred");
        }
        else
        {
            foreach (string eventMsg in events)
            {
                Debug.Log($"  • {eventMsg}");
            }
        }
    }
#endif
}

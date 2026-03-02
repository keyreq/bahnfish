using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Agent 19: Dynamic Events Agent - MigrationSystem.cs
/// Manages seasonal fish migrations and availability changes.
///
/// MIGRATION EFFECTS:
/// - Occurs every season change (14 in-game days)
/// - Duration: Entire season
/// - Certain fish species move to different locations
/// - Some fish become unavailable (seasonal)
/// - New fish appear (migrants)
/// - Spawn rates change by location
///
/// SEASONS:
/// - Spring: Salmon run (salmon abundant in rivers)
/// - Summer: Tropical influx (warm-water fish everywhere)
/// - Autumn: Preparation (fish feed heavily, larger catches)
/// - Winter: Deep dwellers (rare deep-sea fish surface)
/// </summary>
public class MigrationSystem : MonoBehaviour
{
    private static MigrationSystem _instance;
    public static MigrationSystem Instance => _instance;

    [Header("Season Configuration")]
    [SerializeField] private Season currentSeason = Season.Spring;
    [SerializeField] private int daysSinceMigration = 0;
    [SerializeField] private int daysPerSeason = 14;

    [Header("Migration Data")]
    [SerializeField] private List<SeasonalFishData> seasonalFish = new List<SeasonalFishData>();
    [SerializeField] private Dictionary<string, float> locationModifiers = new Dictionary<string, float>();

    [Header("Active Migrations")]
    [SerializeField] private bool migrationActive = false;
    [SerializeField] private List<string> currentMigrants = new List<string>();
    [SerializeField] private List<string> unavailableFish = new List<string>();

    [Header("Weather Patterns")]
    [SerializeField] private WeatherType seasonalWeatherPreference;

    [Header("Visual Changes")]
    [SerializeField] private Color springLighting = new Color(1f, 1f, 0.9f);
    [SerializeField] private Color summerLighting = new Color(1f, 0.95f, 0.85f);
    [SerializeField] private Color autumnLighting = new Color(1f, 0.9f, 0.8f);
    [SerializeField] private Color winterLighting = new Color(0.85f, 0.9f, 1f);

    [Header("Statistics")]
    [SerializeField] private int totalMigrations = 0;
    [SerializeField] private int migrantsCaught = 0;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogging = true;

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
        EventSystem.Subscribe("DayCompleted", OnDayCompleted);
        EventSystem.Subscribe<Fish>("FishCaught", OnFishCaught);
        EventSystem.Subscribe<SaveData>("GatheringSaveData", OnGatheringSaveData);
        EventSystem.Subscribe<SaveData>("ApplyingSaveData", OnApplyingSaveData);

        // Initialize with current season
        InitializeSeasonalData();
        ApplySeasonalEffects(currentSeason);
    }

    /// <summary>
    /// Initializes seasonal fish data
    /// </summary>
    private void InitializeSeasonalData()
    {
        // This would typically load from database or ScriptableObjects
        // For now, create example data
        if (seasonalFish.Count == 0)
        {
            CreateDefaultSeasonalData();
        }
    }

    /// <summary>
    /// Creates default seasonal fish migration patterns
    /// </summary>
    private void CreateDefaultSeasonalData()
    {
        // Spring migrations
        seasonalFish.Add(new SeasonalFishData
        {
            fishID = "salmon",
            availableSeasons = new List<Season> { Season.Spring, Season.Autumn },
            preferredLocations = new List<string> { "river", "coastal" },
            spawnRateMultiplier = 3f
        });

        // Summer migrations
        seasonalFish.Add(new SeasonalFishData
        {
            fishID = "tropical_fish",
            availableSeasons = new List<Season> { Season.Summer },
            preferredLocations = new List<string> { "reef", "shallow" },
            spawnRateMultiplier = 2f
        });

        // Winter deep-dwellers
        seasonalFish.Add(new SeasonalFishData
        {
            fishID = "deep_sea_fish",
            availableSeasons = new List<Season> { Season.Winter },
            preferredLocations = new List<string> { "deep_ocean", "abyss" },
            spawnRateMultiplier = 1.5f
        });

        if (enableDebugLogging)
        {
            Debug.Log($"[MigrationSystem] Created {seasonalFish.Count} default seasonal fish patterns");
        }
    }

    /// <summary>
    /// Called when a day completes
    /// </summary>
    private void OnDayCompleted()
    {
        daysSinceMigration++;

        // Check if season should change
        if (daysSinceMigration >= daysPerSeason)
        {
            ChangeSeason();
        }

        // Occasional announcements about migrations
        if (daysSinceMigration == daysPerSeason - 1)
        {
            AnnounceUpcomingMigration();
        }
    }

    /// <summary>
    /// Changes to the next season
    /// </summary>
    private void ChangeSeason()
    {
        Season previousSeason = currentSeason;

        // Cycle to next season
        currentSeason = (Season)(((int)currentSeason + 1) % 4);

        daysSinceMigration = 0;
        totalMigrations++;

        // Apply seasonal effects
        ApplySeasonalEffects(currentSeason);

        // Update fish availability
        UpdateFishAvailability();

        // Show notification
        string message = GetSeasonChangeMessage(previousSeason, currentSeason);
        EventSystem.Publish("ShowNotification", message);

        if (enableDebugLogging)
        {
            Debug.Log($"[MigrationSystem] Season changed: {previousSeason} -> {currentSeason}");
        }
    }

    /// <summary>
    /// Applies effects for the current season
    /// </summary>
    private void ApplySeasonalEffects(Season season)
    {
        migrationActive = true;

        // Set lighting based on season
        Color seasonalLight = season switch
        {
            Season.Spring => springLighting,
            Season.Summer => summerLighting,
            Season.Autumn => autumnLighting,
            Season.Winter => winterLighting,
            _ => Color.white
        };

        RenderSettings.ambientLight = seasonalLight;

        // Set weather preference
        seasonalWeatherPreference = season switch
        {
            Season.Spring => WeatherType.Rain,
            Season.Summer => WeatherType.Clear,
            Season.Autumn => WeatherType.Fog,
            Season.Winter => WeatherType.Storm,
            _ => WeatherType.Clear
        };

        EventSystem.Publish("SetSeasonalWeatherPreference", seasonalWeatherPreference);

        // Apply season-specific modifiers
        ApplySeasonModifiers(season);

        // Update decorations
        EventSystem.Publish("UpdateSeasonalDecorations", season.ToString());
    }

    /// <summary>
    /// Applies season-specific gameplay modifiers
    /// </summary>
    private void ApplySeasonModifiers(Season season)
    {
        switch (season)
        {
            case Season.Spring:
                // Salmon run - abundant in rivers
                EventSystem.Publish("LocationSpawnBonus", new KeyValuePair<string, float>("river", 3f));
                break;

            case Season.Summer:
                // Tropical fish everywhere
                EventSystem.Publish("GlobalFishSpawnBonus", 1.5f);
                break;

            case Season.Autumn:
                // Fish feed heavily - larger catches
                EventSystem.Publish("FishSizeMultiplier", 1.3f);
                EventSystem.Publish("FishWeightMultiplier", 1.3f);
                break;

            case Season.Winter:
                // Deep dwellers surface
                EventSystem.Publish("DeepSeaFishSpawnBonus", 2f);
                EventSystem.Publish("RareFishSpawnBonus", 1.2f);
                break;
        }
    }

    /// <summary>
    /// Updates which fish are available based on season
    /// </summary>
    private void UpdateFishAvailability()
    {
        currentMigrants.Clear();
        unavailableFish.Clear();

        foreach (SeasonalFishData fishData in seasonalFish)
        {
            bool availableThisSeason = fishData.availableSeasons.Contains(currentSeason);

            if (availableThisSeason)
            {
                // Fish is migrating in
                currentMigrants.Add(fishData.fishID);
                EventSystem.Publish("EnableFish", fishData.fishID);
            }
            else
            {
                // Fish is migrating out
                unavailableFish.Add(fishData.fishID);
                EventSystem.Publish("DisableFish", fishData.fishID);
            }
        }

        if (enableDebugLogging)
        {
            Debug.Log($"[MigrationSystem] Fish availability updated: {currentMigrants.Count} migrants, {unavailableFish.Count} unavailable");
        }
    }

    /// <summary>
    /// Announces upcoming migration to NPCs
    /// </summary>
    private void AnnounceUpcomingMigration()
    {
        Season nextSeason = (Season)(((int)currentSeason + 1) % 4);
        string announcement = GetMigrationWarning(nextSeason);

        EventSystem.Publish("NPCAnnouncement", announcement);

        if (enableDebugLogging)
        {
            Debug.Log($"[MigrationSystem] Migration announcement: {announcement}");
        }
    }

    /// <summary>
    /// Gets warning message for upcoming migration
    /// </summary>
    private string GetMigrationWarning(Season upcomingSeason)
    {
        return upcomingSeason switch
        {
            Season.Spring => "The salmon will be running soon! Head to the rivers!",
            Season.Summer => "Warm waters are coming - tropical fish will arrive!",
            Season.Autumn => "The fish are preparing for winter - they'll be feeding heavily!",
            Season.Winter => "Deep sea dwellers will surface in the cold waters...",
            _ => "The fish are migrating!"
        };
    }

    /// <summary>
    /// Gets season change message
    /// </summary>
    private string GetSeasonChangeMessage(Season from, Season to)
    {
        return to switch
        {
            Season.Spring => "Spring arrives! The salmon run begins in the rivers!",
            Season.Summer => "Summer is here! Tropical fish fill the warm waters!",
            Season.Autumn => "Autumn approaches. Fish are feeding heavily for winter!",
            Season.Winter => "Winter has come. Rare deep-sea fish surface in the cold!",
            _ => $"Season changed from {from} to {to}"
        };
    }

    /// <summary>
    /// Checks if a fish is available in current season
    /// </summary>
    public bool IsFishAvailable(string fishID)
    {
        SeasonalFishData fishData = seasonalFish.FirstOrDefault(f => f.fishID == fishID);

        if (fishData == null)
        {
            return true; // Non-seasonal fish always available
        }

        return fishData.availableSeasons.Contains(currentSeason);
    }

    /// <summary>
    /// Gets spawn rate modifier for a fish in current season
    /// </summary>
    public float GetSeasonalSpawnModifier(string fishID)
    {
        SeasonalFishData fishData = seasonalFish.FirstOrDefault(f => f.fishID == fishID);

        if (fishData == null || !fishData.availableSeasons.Contains(currentSeason))
        {
            return 1f;
        }

        return fishData.spawnRateMultiplier;
    }

    /// <summary>
    /// Gets current season
    /// </summary>
    public Season GetCurrentSeason()
    {
        return currentSeason;
    }

    /// <summary>
    /// Gets days until next migration
    /// </summary>
    public int GetDaysUntilNextMigration()
    {
        return daysPerSeason - daysSinceMigration;
    }

    /// <summary>
    /// Force triggers a season change (for testing)
    /// </summary>
    public void ForceSeasonChange(Season targetSeason)
    {
        currentSeason = targetSeason;
        daysSinceMigration = 0;
        ApplySeasonalEffects(currentSeason);
        UpdateFishAvailability();
    }

    private void OnFishCaught(Fish fish)
    {
        if (currentMigrants.Contains(fish.id))
        {
            migrantsCaught++;
        }
    }

    #region Save/Load Integration

    private void OnGatheringSaveData(SaveData data)
    {
        MigrationSaveData migrationData = new MigrationSaveData
        {
            currentSeason = (int)currentSeason,
            daysSinceMigration = daysSinceMigration,
            totalMigrations = totalMigrations,
            migrantsCaught = migrantsCaught
        };

        data.migrationData = JsonUtility.ToJson(migrationData);
    }

    private void OnApplyingSaveData(SaveData data)
    {
        if (string.IsNullOrEmpty(data.migrationData)) return;

        MigrationSaveData migrationData = JsonUtility.FromJson<MigrationSaveData>(data.migrationData);

        currentSeason = (Season)migrationData.currentSeason;
        daysSinceMigration = migrationData.daysSinceMigration;
        totalMigrations = migrationData.totalMigrations;
        migrantsCaught = migrationData.migrantsCaught;

        ApplySeasonalEffects(currentSeason);
        UpdateFishAvailability();

        if (enableDebugLogging)
        {
            Debug.Log($"[MigrationSystem] Loaded migration data. Season: {currentSeason}, Days: {daysSinceMigration}");
        }
    }

    #endregion

    private void OnDestroy()
    {
        EventSystem.Unsubscribe("DayCompleted", OnDayCompleted);
        EventSystem.Unsubscribe<Fish>("FishCaught", OnFishCaught);
        EventSystem.Unsubscribe<SaveData>("GatheringSaveData", OnGatheringSaveData);
        EventSystem.Unsubscribe<SaveData>("ApplyingSaveData", OnApplyingSaveData);

        if (_instance == this)
        {
            _instance = null;
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Change to Spring")]
    private void ChangeToSpring() => ForceSeasonChange(Season.Spring);

    [ContextMenu("Change to Summer")]
    private void ChangeToSummer() => ForceSeasonChange(Season.Summer);

    [ContextMenu("Change to Autumn")]
    private void ChangeToAutumn() => ForceSeasonChange(Season.Autumn);

    [ContextMenu("Change to Winter")]
    private void ChangeToWinter() => ForceSeasonChange(Season.Winter);

    [ContextMenu("Print Migration Status")]
    private void PrintStatus()
    {
        Debug.Log($"=== Migration System Status ===");
        Debug.Log($"Current Season: {currentSeason}");
        Debug.Log($"Days Since Migration: {daysSinceMigration}");
        Debug.Log($"Days Until Next: {GetDaysUntilNextMigration()}");
        Debug.Log($"Active Migrants: {currentMigrants.Count}");
        Debug.Log($"Unavailable Fish: {unavailableFish.Count}");
    }
#endif
}

/// <summary>
/// Seasons in the game
/// </summary>
[System.Serializable]
public enum Season
{
    Spring = 0,
    Summer = 1,
    Autumn = 2,
    Winter = 3
}

/// <summary>
/// Data for seasonal fish migrations
/// </summary>
[System.Serializable]
public class SeasonalFishData
{
    public string fishID;
    public List<Season> availableSeasons;
    public List<string> preferredLocations;
    public float spawnRateMultiplier;
}

/// <summary>
/// Save data for migration system
/// </summary>
[System.Serializable]
public class MigrationSaveData
{
    public int currentSeason;
    public int daysSinceMigration;
    public int totalMigrations;
    public int migrantsCaught;
}

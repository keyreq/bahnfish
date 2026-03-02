using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Agent 14: Location & World Generation Agent - LocationManager.cs
/// Singleton that manages all fishing locations, handles location loading,
/// and tracks which locations are unlocked.
/// </summary>
public class LocationManager : MonoBehaviour
{
    public static LocationManager Instance { get; private set; }

    [Header("Location Database")]
    [SerializeField] private List<LocationData> allLocations = new List<LocationData>();

    [Header("Current State")]
    [SerializeField] private LocationData currentLocation;
    [SerializeField] private HashSet<string> unlockedLocationIDs = new HashSet<string>();

    [Header("Settings")]
    [SerializeField] private bool autoLoadLocationsFromResources = true;
    [SerializeField] private float locationLoadDelay = 1f; // Simulated loading time

    private Dictionary<string, LocationData> locationsByID = new Dictionary<string, LocationData>();
    private bool isLoadingLocation = false;

    #region Initialization

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeLocationDatabase();
    }

    private void Start()
    {
        // Subscribe to save/load events
        EventSystem.Subscribe<SaveData>("GatheringSaveData", OnGatheringSave);
        EventSystem.Subscribe<SaveData>("ApplyingSaveData", OnApplyingSave);

        // Set starter location as default
        SetStarterLocation();

        Debug.Log($"[LocationManager] Initialized with {allLocations.Count} locations.");
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        EventSystem.Unsubscribe<SaveData>("GatheringSaveData", OnGatheringSave);
        EventSystem.Unsubscribe<SaveData>("ApplyingSaveData", OnApplyingSave);
    }

    /// <summary>
    /// Loads all locations from Resources folder and builds lookup tables.
    /// </summary>
    private void InitializeLocationDatabase()
    {
        if (autoLoadLocationsFromResources)
        {
            LocationData[] loadedLocations = Resources.LoadAll<LocationData>("Locations");

            if (loadedLocations.Length > 0)
            {
                allLocations.Clear();
                allLocations.AddRange(loadedLocations);
            }
        }

        // Build lookup dictionary
        locationsByID.Clear();
        foreach (var location in allLocations)
        {
            if (location == null) continue;

            if (!locationsByID.ContainsKey(location.locationID))
            {
                locationsByID[location.locationID] = location;
            }
            else
            {
                Debug.LogWarning($"[LocationManager] Duplicate location ID: {location.locationID}");
            }
        }
    }

    /// <summary>
    /// Sets the starter location (Calm Lake) as current and unlocks it.
    /// </summary>
    private void SetStarterLocation()
    {
        LocationData starterLocation = GetLocationByID("calm_lake");

        if (starterLocation != null)
        {
            UnlockLocation(starterLocation.locationID);
            currentLocation = starterLocation;
            Debug.Log($"[LocationManager] Starter location set: {starterLocation.locationName}");
        }
        else
        {
            Debug.LogError("[LocationManager] Starter location 'calm_lake' not found!");
        }
    }

    #endregion

    #region Location Queries

    /// <summary>
    /// Gets a location by its unique ID.
    /// </summary>
    public LocationData GetLocationByID(string locationID)
    {
        if (locationsByID.TryGetValue(locationID, out LocationData location))
        {
            return location;
        }

        Debug.LogWarning($"[LocationManager] Location ID not found: {locationID}");
        return null;
    }

    /// <summary>
    /// Gets the currently active location.
    /// </summary>
    public LocationData GetCurrentLocation()
    {
        return currentLocation;
    }

    /// <summary>
    /// Gets all unlocked locations.
    /// </summary>
    public List<LocationData> GetUnlockedLocations()
    {
        return allLocations.Where(loc => IsLocationUnlocked(loc.locationID)).ToList();
    }

    /// <summary>
    /// Gets all locked locations.
    /// </summary>
    public List<LocationData> GetLockedLocations()
    {
        return allLocations.Where(loc => !IsLocationUnlocked(loc.locationID)).ToList();
    }

    /// <summary>
    /// Gets all locations (unlocked and locked).
    /// </summary>
    public List<LocationData> GetAllLocations()
    {
        return new List<LocationData>(allLocations);
    }

    /// <summary>
    /// Checks if a location is unlocked.
    /// </summary>
    public bool IsLocationUnlocked(string locationID)
    {
        return unlockedLocationIDs.Contains(locationID);
    }

    /// <summary>
    /// Gets fish species that can spawn at the current location.
    /// </summary>
    public List<string> GetFishPoolForLocation(string locationID)
    {
        LocationData location = GetLocationByID(locationID);
        return location != null ? location.fishSpeciesIDs : new List<string>();
    }

    /// <summary>
    /// Gets fish species that can spawn at the current location.
    /// </summary>
    public List<string> GetCurrentLocationFishPool()
    {
        return currentLocation != null ? currentLocation.fishSpeciesIDs : new List<string>();
    }

    #endregion

    #region Location Management

    /// <summary>
    /// Unlocks a location (usually after purchasing license).
    /// </summary>
    public bool UnlockLocation(string locationID)
    {
        if (IsLocationUnlocked(locationID))
        {
            Debug.Log($"[LocationManager] Location already unlocked: {locationID}");
            return false;
        }

        unlockedLocationIDs.Add(locationID);
        Debug.Log($"[LocationManager] Location unlocked: {locationID}");

        // Publish event
        EventSystem.Publish("LocationUnlocked", locationID);

        return true;
    }

    /// <summary>
    /// Loads a location and makes it the current location.
    /// </summary>
    public void LoadLocation(string locationID)
    {
        if (isLoadingLocation)
        {
            Debug.LogWarning("[LocationManager] Already loading a location!");
            return;
        }

        LocationData targetLocation = GetLocationByID(locationID);

        if (targetLocation == null)
        {
            Debug.LogError($"[LocationManager] Cannot load location: {locationID} not found!");
            return;
        }

        if (!IsLocationUnlocked(locationID))
        {
            Debug.LogError($"[LocationManager] Cannot load location: {locationID} is locked!");
            return;
        }

        StartCoroutine(LoadLocationCoroutine(targetLocation));
    }

    /// <summary>
    /// Coroutine that handles location loading with delay/transition.
    /// </summary>
    private System.Collections.IEnumerator LoadLocationCoroutine(LocationData targetLocation)
    {
        isLoadingLocation = true;

        // Publish loading started event
        EventSystem.Publish("LocationLoadingStarted", targetLocation.locationID);

        Debug.Log($"[LocationManager] Loading location: {targetLocation.locationName}");

        // Simulate loading time (in real game, this would load scene)
        yield return new WaitForSeconds(locationLoadDelay);

        // Store previous location
        LocationData previousLocation = currentLocation;

        // Set new current location
        currentLocation = targetLocation;

        // Update game state
        if (GameManager.Instance != null)
        {
            GameState state = GameManager.Instance.CurrentGameState;
            state.currentLocationID = targetLocation.locationID;
            state.playerPosition = targetLocation.dockPosition;
            GameManager.Instance.UpdateGameState(state);
        }

        // Apply location-specific settings
        ApplyLocationSettings(targetLocation);

        // Publish location changed event
        EventSystem.Publish("LocationChanged", targetLocation);

        Debug.Log($"[LocationManager] Location loaded: {targetLocation.locationName}");

        isLoadingLocation = false;

        // Publish loading complete event
        EventSystem.Publish("LocationLoadingComplete", targetLocation.locationID);
    }

    /// <summary>
    /// Applies location-specific settings to other game systems.
    /// </summary>
    private void ApplyLocationSettings(LocationData location)
    {
        // Update weather system (Agent 3)
        if (WeatherSystem.Instance != null)
        {
            WeatherSystem.Instance.SetAllowedWeather(location.allowedWeather);
        }

        // Update sanity manager (Agent 7)
        if (SanityManager.Instance != null)
        {
            SanityManager.Instance.SetSanityDrainModifier(location.sanityDrainModifier);
        }

        // Update fish spawner (Agent 8)
        if (FishSpawner.Instance != null)
        {
            FishSpawner.Instance.SetLocationFishPool(location.fishSpeciesIDs);
            FishSpawner.Instance.SetSpawnRateModifiers(
                location.rareFishSpawnMultiplier,
                location.legendaryFishSpawnMultiplier,
                location.aberrantSpawnMultiplier
            );
        }

        // Apply fog if location has permanent fog
        if (location.hasPermanentFog && FogSystem.Instance != null)
        {
            FogSystem.Instance.EnablePermanentFog(true);
        }

        Debug.Log($"[LocationManager] Applied settings for location: {location.locationName}");
    }

    #endregion

    #region Save/Load Integration

    /// <summary>
    /// Called when game is saving - adds location data to save file.
    /// </summary>
    private void OnGatheringSave(SaveData data)
    {
        LocationSaveData locationData = new LocationSaveData
        {
            currentLocationID = currentLocation != null ? currentLocation.locationID : "calm_lake",
            unlockedLocationIDs = new List<string>(unlockedLocationIDs)
        };

        data.locationData = JsonUtility.ToJson(locationData);
        Debug.Log("[LocationManager] Location data saved.");
    }

    /// <summary>
    /// Called when game is loading - restores location data from save file.
    /// </summary>
    private void OnApplyingSave(SaveData data)
    {
        if (string.IsNullOrEmpty(data.locationData))
        {
            Debug.LogWarning("[LocationManager] No location data in save file, using defaults.");
            return;
        }

        LocationSaveData locationData = JsonUtility.FromJson<LocationSaveData>(data.locationData);

        // Restore unlocked locations
        unlockedLocationIDs.Clear();
        foreach (string locID in locationData.unlockedLocationIDs)
        {
            unlockedLocationIDs.Add(locID);
        }

        // Restore current location
        LocationData loadedLocation = GetLocationByID(locationData.currentLocationID);
        if (loadedLocation != null)
        {
            currentLocation = loadedLocation;
            ApplyLocationSettings(currentLocation);
        }

        Debug.Log($"[LocationManager] Location data loaded: {locationData.unlockedLocationIDs.Count} locations unlocked.");
    }

    #endregion

    #region Debug & Utilities

    /// <summary>
    /// Gets statistics about the location database.
    /// </summary>
    public string GetLocationStats()
    {
        System.Text.StringBuilder stats = new System.Text.StringBuilder();
        stats.AppendLine("=== Location Manager Statistics ===");
        stats.AppendLine($"Total Locations: {allLocations.Count}");
        stats.AppendLine($"Unlocked Locations: {unlockedLocationIDs.Count}");
        stats.AppendLine($"Current Location: {(currentLocation != null ? currentLocation.locationName : "None")}");

        if (currentLocation != null)
        {
            stats.AppendLine($"\nCurrent Location Details:");
            stats.AppendLine($"  - ID: {currentLocation.locationID}");
            stats.AppendLine($"  - Difficulty: {currentLocation.difficulty}");
            stats.AppendLine($"  - Fish Species: {currentLocation.fishSpeciesIDs.Count}");
            stats.AppendLine($"  - Biome: {currentLocation.biomeType}");
            stats.AppendLine($"  - Sanity Drain: {currentLocation.sanityDrainModifier}x");
        }

        return stats.ToString();
    }

#if UNITY_EDITOR
    [ContextMenu("Print Location Stats")]
    private void PrintLocationStats()
    {
        Debug.Log(GetLocationStats());
    }

    [ContextMenu("Unlock All Locations (Debug)")]
    private void DebugUnlockAllLocations()
    {
        foreach (var location in allLocations)
        {
            UnlockLocation(location.locationID);
        }
        Debug.Log("[LocationManager] All locations unlocked (debug mode).");
    }

    [ContextMenu("Reset Unlocked Locations")]
    private void DebugResetUnlockedLocations()
    {
        unlockedLocationIDs.Clear();
        SetStarterLocation();
        Debug.Log("[LocationManager] Unlocked locations reset.");
    }
#endif

    #endregion
}

/// <summary>
/// Save data structure for location system.
/// </summary>
[System.Serializable]
public class LocationSaveData
{
    public string currentLocationID;
    public List<string> unlockedLocationIDs;
}

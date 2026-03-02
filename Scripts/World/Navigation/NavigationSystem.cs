using UnityEngine;
using System.Collections;

/// <summary>
/// Agent 14: Location & World Generation Agent - NavigationSystem.cs
/// Handles travel between locations, fuel consumption, and travel time.
/// </summary>
public class NavigationSystem : MonoBehaviour
{
    public static NavigationSystem Instance { get; private set; }

    [Header("Travel Settings")]
    [SerializeField] private float baseSpeedKnots = 10f; // Boat speed in knots
    [SerializeField] private float fuelConsumptionRate = 0.5f; // Fuel per km
    [SerializeField] private float kmPerDegree = 111f; // Rough conversion for map distances

    [Header("Current State")]
    [SerializeField] private bool isTraveling = false;
    [SerializeField] private LocationData currentDestination;
    [SerializeField] private float travelProgress = 0f;

    [Header("Settings")]
    [SerializeField] private bool canTravelWhileFishing = false;
    [SerializeField] private bool showTravelUI = true;

    private float engineSpeedMultiplier = 1f; // From upgrades

    #region Initialization

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        Debug.Log("[NavigationSystem] Initialized.");
    }

    #endregion

    #region Travel Methods

    /// <summary>
    /// Attempts to travel to a target location.
    /// </summary>
    public bool TravelToLocation(string targetLocationID)
    {
        // Check if already traveling
        if (isTraveling)
        {
            Debug.LogWarning("[NavigationSystem] Already traveling to a location!");
            return false;
        }

        // Check if currently fishing
        if (!canTravelWhileFishing && IsFishing())
        {
            Debug.LogWarning("[NavigationSystem] Cannot travel while fishing!");
            EventSystem.Publish("TravelBlocked", "Cannot travel while fishing");
            return false;
        }

        // Get target location
        LocationData targetLocation = LocationManager.Instance.GetLocationByID(targetLocationID);
        if (targetLocation == null)
        {
            Debug.LogError($"[NavigationSystem] Target location not found: {targetLocationID}");
            return false;
        }

        // Check if location is unlocked
        if (!LocationManager.Instance.IsLocationUnlocked(targetLocationID))
        {
            Debug.LogWarning($"[NavigationSystem] Location is locked: {targetLocation.locationName}");
            EventSystem.Publish("TravelBlocked", "Location is locked");
            return false;
        }

        // Get current location
        LocationData currentLocation = LocationManager.Instance.GetCurrentLocation();
        if (currentLocation == null)
        {
            Debug.LogError("[NavigationSystem] No current location set!");
            return false;
        }

        // Check if already at target
        if (currentLocation.locationID == targetLocationID)
        {
            Debug.Log($"[NavigationSystem] Already at location: {targetLocation.locationName}");
            return false;
        }

        // Calculate travel requirements
        float distance = CalculateDistance(currentLocation, targetLocation);
        float fuelRequired = CalculateFuelCost(distance);
        float travelTime = CalculateTravelTime(distance);

        // Check if player has enough fuel
        GameState gameState = GameManager.Instance.CurrentGameState;
        if (gameState.fuel < fuelRequired)
        {
            Debug.LogWarning($"[NavigationSystem] Not enough fuel! Need {fuelRequired:F1}, have {gameState.fuel:F1}");
            EventSystem.Publish("TravelBlocked", "Not enough fuel");
            return false;
        }

        // Start travel
        StartCoroutine(TravelCoroutine(targetLocation, distance, fuelRequired, travelTime));

        return true;
    }

    /// <summary>
    /// Coroutine that handles the travel process.
    /// </summary>
    private IEnumerator TravelCoroutine(LocationData destination, float distance, float fuelCost, float travelTime)
    {
        isTraveling = true;
        currentDestination = destination;
        travelProgress = 0f;

        // Publish travel started event
        TravelStartedEventData travelData = new TravelStartedEventData
        {
            destinationID = destination.locationID,
            destinationName = destination.locationName,
            distance = distance,
            fuelCost = fuelCost,
            estimatedTime = travelTime
        };
        EventSystem.Publish("TravelStarted", travelData);

        Debug.Log($"[NavigationSystem] Traveling to {destination.locationName} ({distance:F1} km, {travelTime:F1}s, {fuelCost:F1} fuel)");

        // Consume fuel gradually during travel
        float fuelPerSecond = fuelCost / travelTime;
        float elapsedTime = 0f;

        while (elapsedTime < travelTime)
        {
            elapsedTime += Time.deltaTime;
            travelProgress = Mathf.Clamp01(elapsedTime / travelTime);

            // Consume fuel
            if (GameManager.Instance != null)
            {
                GameState state = GameManager.Instance.CurrentGameState;
                state.fuel -= fuelPerSecond * Time.deltaTime;
                state.fuel = Mathf.Max(0, state.fuel);
                GameManager.Instance.UpdateGameState(state);
            }

            // Publish progress update
            EventSystem.Publish("TravelProgress", travelProgress);

            yield return null;
        }

        // Travel complete - load the destination
        travelProgress = 1f;
        isTraveling = false;

        LocationManager.Instance.LoadLocation(destination.locationID);

        // Publish travel complete event
        EventSystem.Publish("TravelComplete", destination.locationID);

        Debug.Log($"[NavigationSystem] Arrived at {destination.locationName}");

        currentDestination = null;
    }

    /// <summary>
    /// Cancels current travel (emergency return, etc).
    /// </summary>
    public void CancelTravel()
    {
        if (!isTraveling)
        {
            return;
        }

        StopAllCoroutines();
        isTraveling = false;
        travelProgress = 0f;

        EventSystem.Publish("TravelCancelled", currentDestination?.locationID);
        Debug.Log("[NavigationSystem] Travel cancelled.");

        currentDestination = null;
    }

    #endregion

    #region Calculation Methods

    /// <summary>
    /// Calculates distance between two locations in kilometers.
    /// </summary>
    public float CalculateDistance(LocationData from, LocationData to)
    {
        if (from == null || to == null) return 0f;

        // Simple Euclidean distance (in game, this would use world coordinates)
        float distance = Vector3.Distance(from.dockPosition, to.dockPosition);

        // Convert to km (assuming Unity units are meters)
        return distance / 1000f;
    }

    /// <summary>
    /// Calculates fuel cost for a given distance.
    /// </summary>
    public float CalculateFuelCost(float distanceKm)
    {
        // Base fuel cost modified by engine efficiency (from upgrades)
        float efficiencyMultiplier = 1f / engineSpeedMultiplier;
        return distanceKm * fuelConsumptionRate * efficiencyMultiplier;
    }

    /// <summary>
    /// Calculates travel time for a given distance (in seconds).
    /// </summary>
    public float CalculateTravelTime(float distanceKm)
    {
        // Speed in km/h
        float speedKmh = baseSpeedKnots * 1.852f * engineSpeedMultiplier;

        // Time in hours, converted to seconds
        float timeHours = distanceKm / speedKmh;
        return timeHours * 3600f;
    }

    /// <summary>
    /// Calculates travel requirements for a destination.
    /// </summary>
    public TravelRequirements GetTravelRequirements(string targetLocationID)
    {
        LocationData currentLocation = LocationManager.Instance.GetCurrentLocation();
        LocationData targetLocation = LocationManager.Instance.GetLocationByID(targetLocationID);

        if (currentLocation == null || targetLocation == null)
        {
            return new TravelRequirements();
        }

        float distance = CalculateDistance(currentLocation, targetLocation);

        return new TravelRequirements
        {
            distance = distance,
            fuelCost = CalculateFuelCost(distance),
            travelTime = CalculateTravelTime(distance),
            canAfford = GameManager.Instance.CurrentGameState.fuel >= CalculateFuelCost(distance)
        };
    }

    #endregion

    #region Upgrade Integration

    /// <summary>
    /// Sets engine speed multiplier from upgrades (Agent 9).
    /// </summary>
    public void SetEngineSpeedMultiplier(float multiplier)
    {
        engineSpeedMultiplier = Mathf.Max(0.1f, multiplier);
        Debug.Log($"[NavigationSystem] Engine speed multiplier set to {engineSpeedMultiplier}x");
    }

    /// <summary>
    /// Sets fuel consumption rate from upgrades.
    /// </summary>
    public void SetFuelConsumptionRate(float rate)
    {
        fuelConsumptionRate = Mathf.Max(0.1f, rate);
        Debug.Log($"[NavigationSystem] Fuel consumption rate set to {fuelConsumptionRate}");
    }

    #endregion

    #region Queries

    /// <summary>
    /// Checks if currently traveling.
    /// </summary>
    public bool IsTraveling()
    {
        return isTraveling;
    }

    /// <summary>
    /// Gets current travel progress (0-1).
    /// </summary>
    public float GetTravelProgress()
    {
        return travelProgress;
    }

    /// <summary>
    /// Gets current travel destination.
    /// </summary>
    public LocationData GetCurrentDestination()
    {
        return currentDestination;
    }

    /// <summary>
    /// Checks if player is currently fishing.
    /// </summary>
    private bool IsFishing()
    {
        if (FishingController.Instance != null)
        {
            return FishingController.Instance.IsFishing();
        }
        return false;
    }

    #endregion

#if UNITY_EDITOR
    [ContextMenu("Simulate Quick Travel")]
    private void DebugQuickTravel()
    {
        // For testing - instant travel
        var unlockedLocations = LocationManager.Instance.GetUnlockedLocations();
        if (unlockedLocations.Count > 1)
        {
            LocationData current = LocationManager.Instance.GetCurrentLocation();
            LocationData target = unlockedLocations.Find(loc => loc.locationID != current.locationID);

            if (target != null)
            {
                LocationManager.Instance.LoadLocation(target.locationID);
                Debug.Log($"[NavigationSystem] Debug: Instant travel to {target.locationName}");
            }
        }
    }
#endif
}

/// <summary>
/// Data structure for travel requirements.
/// </summary>
[System.Serializable]
public struct TravelRequirements
{
    public float distance;
    public float fuelCost;
    public float travelTime;
    public bool canAfford;
}

/// <summary>
/// Event data for travel started.
/// </summary>
[System.Serializable]
public struct TravelStartedEventData
{
    public string destinationID;
    public string destinationName;
    public float distance;
    public float fuelCost;
    public float estimatedTime;
}

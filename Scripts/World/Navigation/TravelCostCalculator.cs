using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Agent 14: Location & World Generation Agent - TravelCostCalculator.cs
/// Calculates fuel costs and efficiency for travel between locations.
/// Integrates with upgrade system for fuel efficiency improvements.
/// </summary>
public class TravelCostCalculator : MonoBehaviour
{
    public static TravelCostCalculator Instance { get; private set; }

    [Header("Base Fuel Costs")]
    [SerializeField] private float baseFuelCostPerKm = 0.5f;
    [SerializeField] private float minimumFuelReserve = 5f; // Minimum fuel to keep in tank

    [Header("Efficiency Modifiers")]
    [SerializeField] private float engineEfficiencyMultiplier = 1f; // From upgrades
    [SerializeField] private float weatherEfficiencyModifier = 1f; // Storm = more fuel
    [SerializeField] private float nightEfficiencyModifier = 1f; // Night travel penalty

    [Header("Warnings")]
    [SerializeField] private float lowFuelWarningThreshold = 20f;
    [SerializeField] private float criticalFuelThreshold = 10f;

    private Dictionary<string, float> locationDistanceCache = new Dictionary<string, float>();

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
        // Subscribe to weather changes to update efficiency
        EventSystem.Subscribe<WeatherType>("WeatherChanged", OnWeatherChanged);

        // Subscribe to time changes for night modifier
        EventSystem.Subscribe<TimeChangedEventData>("TimeChanged", OnTimeChanged);

        Debug.Log("[TravelCostCalculator] Initialized.");
    }

    private void OnDestroy()
    {
        EventSystem.Unsubscribe<WeatherType>("WeatherChanged", OnWeatherChanged);
        EventSystem.Unsubscribe<TimeChangedEventData>("TimeChanged", OnTimeChanged);
    }

    #endregion

    #region Fuel Cost Calculations

    /// <summary>
    /// Calculates fuel cost to travel from current location to target.
    /// </summary>
    public float CalculateFuelCost(string targetLocationID)
    {
        LocationData currentLocation = LocationManager.Instance.GetCurrentLocation();
        LocationData targetLocation = LocationManager.Instance.GetLocationByID(targetLocationID);

        if (currentLocation == null || targetLocation == null)
        {
            return 0f;
        }

        return CalculateFuelCost(currentLocation, targetLocation);
    }

    /// <summary>
    /// Calculates fuel cost between two locations.
    /// </summary>
    public float CalculateFuelCost(LocationData from, LocationData to)
    {
        float distance = GetDistance(from, to);
        return CalculateFuelCostForDistance(distance);
    }

    /// <summary>
    /// Calculates fuel cost for a specific distance.
    /// </summary>
    public float CalculateFuelCostForDistance(float distanceKm)
    {
        float baseCost = distanceKm * baseFuelCostPerKm;

        // Apply efficiency modifiers
        baseCost /= engineEfficiencyMultiplier; // Better engine = less fuel
        baseCost *= weatherEfficiencyModifier; // Bad weather = more fuel
        baseCost *= nightEfficiencyModifier; // Night = slightly more fuel

        return Mathf.Max(0f, baseCost);
    }

    /// <summary>
    /// Gets the distance between two locations (with caching).
    /// </summary>
    private float GetDistance(LocationData from, LocationData to)
    {
        string cacheKey = $"{from.locationID}_to_{to.locationID}";

        // Check cache
        if (locationDistanceCache.TryGetValue(cacheKey, out float cachedDistance))
        {
            return cachedDistance;
        }

        // Calculate and cache
        float distance = Vector3.Distance(from.dockPosition, to.dockPosition) / 1000f; // Convert to km
        locationDistanceCache[cacheKey] = distance;

        return distance;
    }

    #endregion

    #region Fuel Checks & Warnings

    /// <summary>
    /// Checks if player has enough fuel to travel to target location.
    /// </summary>
    public bool CanAffordTravel(string targetLocationID)
    {
        float fuelCost = CalculateFuelCost(targetLocationID);
        float currentFuel = GetCurrentFuel();

        return currentFuel >= fuelCost + minimumFuelReserve;
    }

    /// <summary>
    /// Gets current fuel level from game state.
    /// </summary>
    public float GetCurrentFuel()
    {
        if (GameManager.Instance != null)
        {
            return GameManager.Instance.CurrentGameState.fuel;
        }
        return 0f;
    }

    /// <summary>
    /// Checks if fuel is low and returns warning message.
    /// </summary>
    public FuelWarning GetFuelWarningStatus()
    {
        float currentFuel = GetCurrentFuel();

        if (currentFuel <= criticalFuelThreshold)
        {
            return new FuelWarning
            {
                level = FuelWarningLevel.Critical,
                message = $"CRITICAL: Only {currentFuel:F1} fuel remaining!",
                fuelRemaining = currentFuel
            };
        }
        else if (currentFuel <= lowFuelWarningThreshold)
        {
            return new FuelWarning
            {
                level = FuelWarningLevel.Low,
                message = $"Low fuel: {currentFuel:F1} remaining",
                fuelRemaining = currentFuel
            };
        }
        else
        {
            return new FuelWarning
            {
                level = FuelWarningLevel.Normal,
                message = $"Fuel: {currentFuel:F1}",
                fuelRemaining = currentFuel
            };
        }
    }

    /// <summary>
    /// Gets the maximum range player can travel with current fuel.
    /// </summary>
    public float GetMaximumRange()
    {
        float currentFuel = GetCurrentFuel();
        float usableFuel = Mathf.Max(0f, currentFuel - minimumFuelReserve);

        return usableFuel / (baseFuelCostPerKm / engineEfficiencyMultiplier);
    }

    /// <summary>
    /// Gets all locations reachable with current fuel.
    /// </summary>
    public List<LocationData> GetReachableLocations()
    {
        List<LocationData> reachable = new List<LocationData>();
        List<LocationData> unlockedLocations = LocationManager.Instance.GetUnlockedLocations();
        LocationData currentLocation = LocationManager.Instance.GetCurrentLocation();

        foreach (var location in unlockedLocations)
        {
            if (location.locationID == currentLocation?.locationID)
                continue;

            if (CanAffordTravel(location.locationID))
            {
                reachable.Add(location);
            }
        }

        return reachable;
    }

    #endregion

    #region Upgrade Integration

    /// <summary>
    /// Sets engine efficiency from upgrades (Agent 9).
    /// Higher value = better efficiency = less fuel consumption.
    /// </summary>
    public void SetEngineEfficiency(float efficiency)
    {
        engineEfficiencyMultiplier = Mathf.Max(0.1f, efficiency);
        Debug.Log($"[TravelCostCalculator] Engine efficiency set to {engineEfficiencyMultiplier}x");

        // Clear distance cache as costs have changed
        locationDistanceCache.Clear();
    }

    /// <summary>
    /// Sets base fuel cost per km (for balance tweaks).
    /// </summary>
    public void SetBaseFuelCost(float costPerKm)
    {
        baseFuelCostPerKm = Mathf.Max(0.01f, costPerKm);
        Debug.Log($"[TravelCostCalculator] Base fuel cost set to {baseFuelCostPerKm}/km");

        locationDistanceCache.Clear();
    }

    #endregion

    #region Weather & Time Effects

    /// <summary>
    /// Updates efficiency modifier based on weather.
    /// </summary>
    private void OnWeatherChanged(WeatherType weather)
    {
        switch (weather)
        {
            case WeatherType.Clear:
                weatherEfficiencyModifier = 1f;
                break;
            case WeatherType.Rain:
                weatherEfficiencyModifier = 1.1f; // 10% more fuel
                break;
            case WeatherType.Storm:
                weatherEfficiencyModifier = 1.5f; // 50% more fuel
                break;
            case WeatherType.Fog:
                weatherEfficiencyModifier = 1.2f; // 20% more fuel (slower speed)
                break;
        }

        Debug.Log($"[TravelCostCalculator] Weather efficiency modifier: {weatherEfficiencyModifier}x");
    }

    /// <summary>
    /// Updates efficiency modifier based on time of day.
    /// </summary>
    private void OnTimeChanged(TimeChangedEventData timeData)
    {
        switch (timeData.timeOfDay)
        {
            case TimeOfDay.Day:
                nightEfficiencyModifier = 1f;
                break;
            case TimeOfDay.Dusk:
                nightEfficiencyModifier = 1.05f;
                break;
            case TimeOfDay.Night:
                nightEfficiencyModifier = 1.1f; // 10% more fuel (slower, cautious navigation)
                break;
            case TimeOfDay.Dawn:
                nightEfficiencyModifier = 1.05f;
                break;
        }
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Gets detailed travel cost breakdown for UI display.
    /// </summary>
    public TravelCostBreakdown GetTravelCostBreakdown(string targetLocationID)
    {
        LocationData currentLocation = LocationManager.Instance.GetCurrentLocation();
        LocationData targetLocation = LocationManager.Instance.GetLocationByID(targetLocationID);

        if (currentLocation == null || targetLocation == null)
        {
            return new TravelCostBreakdown();
        }

        float distance = GetDistance(currentLocation, targetLocation);
        float baseCost = distance * baseFuelCostPerKm;

        return new TravelCostBreakdown
        {
            distance = distance,
            baseFuelCost = baseCost,
            engineEfficiency = engineEfficiencyMultiplier,
            weatherModifier = weatherEfficiencyModifier,
            nightModifier = nightEfficiencyModifier,
            totalFuelCost = CalculateFuelCostForDistance(distance),
            canAfford = CanAffordTravel(targetLocationID)
        };
    }

    /// <summary>
    /// Clears the distance cache (useful when locations change).
    /// </summary>
    public void ClearDistanceCache()
    {
        locationDistanceCache.Clear();
        Debug.Log("[TravelCostCalculator] Distance cache cleared.");
    }

    #endregion

#if UNITY_EDITOR
    [ContextMenu("Print Fuel Status")]
    private void PrintFuelStatus()
    {
        FuelWarning warning = GetFuelWarningStatus();
        Debug.Log($"=== Fuel Status ===\n{warning.message}\nMax Range: {GetMaximumRange():F1} km");
    }

    [ContextMenu("Print Reachable Locations")]
    private void PrintReachableLocations()
    {
        List<LocationData> reachable = GetReachableLocations();
        Debug.Log($"=== Reachable Locations ({reachable.Count}) ===");
        foreach (var loc in reachable)
        {
            float cost = CalculateFuelCost(loc.locationID);
            Debug.Log($"  - {loc.locationName}: {cost:F1} fuel");
        }
    }
#endif
}

/// <summary>
/// Fuel warning levels.
/// </summary>
public enum FuelWarningLevel
{
    Normal,
    Low,
    Critical
}

/// <summary>
/// Fuel warning data structure.
/// </summary>
[System.Serializable]
public struct FuelWarning
{
    public FuelWarningLevel level;
    public string message;
    public float fuelRemaining;
}

/// <summary>
/// Detailed breakdown of travel costs.
/// </summary>
[System.Serializable]
public struct TravelCostBreakdown
{
    public float distance;
    public float baseFuelCost;
    public float engineEfficiency;
    public float weatherModifier;
    public float nightModifier;
    public float totalFuelCost;
    public bool canAfford;
}

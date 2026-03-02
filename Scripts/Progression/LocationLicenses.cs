using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Agent 9: Progression & Economy Agent - LocationLicenses.cs
/// Manages unlocking of fishing locations through purchase.
/// 13 distinct locations with progressive costs and prerequisites.
/// </summary>
public class LocationLicenses : MonoBehaviour
{
    public static LocationLicenses Instance { get; private set; }

    [Header("Location Data")]
    [SerializeField] private List<LocationLicenseData> _allLocations = new List<LocationLicenseData>();
    private Dictionary<string, LocationLicenseData> _locationsByID = new Dictionary<string, LocationLicenseData>();
    private HashSet<string> _unlockedLocations = new HashSet<string>();

    [Header("Debug")]
    [SerializeField] private bool _enableDebugLogs = true;

    // Events
    public event System.Action<LocationLicenseData> OnLocationUnlocked;
    public event System.Action<LocationLicenseData> OnLocationAvailable;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeLocations();
    }

    private void Start()
    {
        // Subscribe to save/load events
        EventSystem.Subscribe<SaveData>("GatheringSaveData", OnGatheringSaveData);
        EventSystem.Subscribe<SaveData>("ApplyingSaveData", OnApplyingSaveData);

        // Ensure starter location is unlocked
        UnlockLocationInternal("starter_lake", true);

        if (_enableDebugLogs)
        {
            Debug.Log($"[LocationLicenses] Initialized with {_allLocations.Count} locations");
        }
    }

    private void OnDestroy()
    {
        EventSystem.Unsubscribe<SaveData>("GatheringSaveData", OnGatheringSaveData);
        EventSystem.Unsubscribe<SaveData>("ApplyingSaveData", OnApplyingSaveData);
    }

    #region Initialization

    /// <summary>
    /// Initializes all 13 fishing location licenses from design document.
    /// </summary>
    private void InitializeLocations()
    {
        if (_allLocations.Count == 0)
        {
            CreateDefaultLocations();
        }

        // Build lookup dictionary
        _locationsByID.Clear();
        foreach (var location in _allLocations)
        {
            _locationsByID[location.locationID] = location;
        }
    }

    /// <summary>
    /// Creates all 13 locations from game design document.
    /// </summary>
    private void CreateDefaultLocations()
    {
        // 1. Starter Location (FREE)
        _allLocations.Add(new LocationLicenseData
        {
            locationID = "starter_lake",
            locationName = "Calm Lake",
            description = "A peaceful lake perfect for beginners. Calm waters, abundant common fish.",
            cost = 0f,
            tier = 1,
            isStarterLocation = true,
            difficulty = LocationDifficulty.Easy,
            dominantFishRarities = new FishRarity[] { FishRarity.Common, FishRarity.Uncommon },
            hazardLevel = 0f
        });

        // 2-3. Tier 1 Locations ($500-1500)
        _allLocations.Add(new LocationLicenseData
        {
            locationID = "rocky_coastline",
            locationName = "Rocky Coastline",
            description = "Rough waters with rocky outcrops. Medium-sized fish hide in crevices.",
            cost = 500f,
            tier = 1,
            difficulty = LocationDifficulty.Easy,
            prerequisites = new string[] { "starter_lake" },
            dominantFishRarities = new FishRarity[] { FishRarity.Common, FishRarity.Uncommon },
            hazardLevel = 0.2f
        });

        _allLocations.Add(new LocationLicenseData
        {
            locationID = "tidal_pools",
            locationName = "Tidal Pools",
            description = "Shallow pools filled with unique tidal species. Shellfish and crabs abundant.",
            cost = 1500f,
            tier = 1,
            difficulty = LocationDifficulty.Easy,
            prerequisites = new string[] { "rocky_coastline" },
            dominantFishRarities = new FishRarity[] { FishRarity.Uncommon },
            hazardLevel = 0.1f
        });

        // 4-6. Tier 2 Locations ($1500-2500)
        _allLocations.Add(new LocationLicenseData
        {
            locationID = "deep_ocean",
            locationName = "Deep Ocean",
            description = "Vast open waters where large predators hunt. Rare fish lurk in the depths.",
            cost = 1500f,
            tier = 2,
            difficulty = LocationDifficulty.Medium,
            prerequisites = new string[] { "rocky_coastline" },
            dominantFishRarities = new FishRarity[] { FishRarity.Uncommon, FishRarity.Rare },
            hazardLevel = 0.4f
        });

        _allLocations.Add(new LocationLicenseData
        {
            locationID = "fog_swamp",
            locationName = "Fog Swamp",
            description = "Murky waters shrouded in perpetual mist. High aberrant fish spawn rate.",
            cost = 2000f,
            tier = 2,
            difficulty = LocationDifficulty.Medium,
            prerequisites = new string[] { "tidal_pools" },
            dominantFishRarities = new FishRarity[] { FishRarity.Uncommon, FishRarity.Aberrant },
            hazardLevel = 0.6f
        });

        _allLocations.Add(new LocationLicenseData
        {
            locationID = "mangrove_forest",
            locationName = "Mangrove Forest",
            description = "Dense underwater roots create a labyrinth. Unique ecosystem with hidden species.",
            cost = 2000f,
            tier = 2,
            difficulty = LocationDifficulty.Medium,
            prerequisites = new string[] { "tidal_pools" },
            dominantFishRarities = new FishRarity[] { FishRarity.Uncommon, FishRarity.Rare },
            hazardLevel = 0.3f
        });

        _allLocations.Add(new LocationLicenseData
        {
            locationID = "coral_reef",
            locationName = "Coral Reef",
            description = "Vibrant underwater paradise. Highest diversity of species.",
            cost = 2500f,
            tier = 2,
            difficulty = LocationDifficulty.Medium,
            prerequisites = new string[] { "deep_ocean" },
            dominantFishRarities = new FishRarity[] { FishRarity.Uncommon, FishRarity.Rare },
            hazardLevel = 0.2f
        });

        // 7-10. Tier 3 Locations ($3000-5000)
        _allLocations.Add(new LocationLicenseData
        {
            locationID = "arctic_waters",
            locationName = "Arctic Waters",
            description = "Freezing temperatures and icebergs. Hardy species and occasional polar predators.",
            cost = 3000f,
            tier = 3,
            difficulty = LocationDifficulty.Hard,
            prerequisites = new string[] { "deep_ocean" },
            dominantFishRarities = new FishRarity[] { FishRarity.Rare },
            hazardLevel = 0.5f
        });

        _allLocations.Add(new LocationLicenseData
        {
            locationID = "shipwreck_graveyard",
            locationName = "Shipwreck Graveyard",
            description = "Sunken vessels litter the seabed. Perfect for dredging ancient artifacts.",
            cost = 3500f,
            tier = 3,
            difficulty = LocationDifficulty.Hard,
            prerequisites = new string[] { "deep_ocean", "fog_swamp" },
            dominantFishRarities = new FishRarity[] { FishRarity.Rare, FishRarity.Aberrant },
            hazardLevel = 0.7f
        });

        _allLocations.Add(new LocationLicenseData
        {
            locationID = "underground_cavern",
            locationName = "Underground Cavern",
            description = "Dark subterranean waters. Pure mystery and horror. Relics found here.",
            cost = 4000f,
            tier = 3,
            difficulty = LocationDifficulty.Hard,
            prerequisites = new string[] { "fog_swamp" },
            dominantFishRarities = new FishRarity[] { FishRarity.Rare, FishRarity.Aberrant },
            hazardLevel = 0.8f
        });

        _allLocations.Add(new LocationLicenseData
        {
            locationID = "bioluminescent_bay",
            locationName = "Bioluminescent Bay",
            description = "Waters glow with ethereal light. Unique bioluminescent fish species.",
            cost = 4500f,
            tier = 3,
            difficulty = LocationDifficulty.Hard,
            prerequisites = new string[] { "coral_reef" },
            dominantFishRarities = new FishRarity[] { FishRarity.Rare, FishRarity.Legendary },
            hazardLevel = 0.4f
        });

        _allLocations.Add(new LocationLicenseData
        {
            locationID = "volcanic_vent",
            locationName = "Volcanic Vent",
            description = "Superheated waters near underwater volcanoes. Extreme conditions, extreme fish.",
            cost = 5000f,
            tier = 3,
            difficulty = LocationDifficulty.Hard,
            prerequisites = new string[] { "arctic_waters" },
            dominantFishRarities = new FishRarity[] { FishRarity.Rare, FishRarity.Legendary },
            hazardLevel = 0.6f
        });

        // 11. Endgame Location ($10,000)
        _allLocations.Add(new LocationLicenseData
        {
            locationID = "abyssal_trench",
            locationName = "Abyssal Trench",
            description = "The deepest, darkest waters. Home to legendary creatures and cosmic horrors.",
            cost = 10000f,
            tier = 4,
            difficulty = LocationDifficulty.Extreme,
            prerequisites = new string[] { "underground_cavern", "volcanic_vent", "shipwreck_graveyard" },
            dominantFishRarities = new FishRarity[] { FishRarity.Legendary, FishRarity.Aberrant },
            hazardLevel = 1.0f
        });
    }

    #endregion

    #region Purchase License

    /// <summary>
    /// Attempts to purchase a location license.
    /// </summary>
    public bool PurchaseLicense(string locationID)
    {
        if (!_locationsByID.TryGetValue(locationID, out LocationLicenseData location))
        {
            Debug.LogWarning($"[LocationLicenses] Unknown location ID: {locationID}");
            return false;
        }

        // Check if already unlocked
        if (IsLocationUnlocked(locationID))
        {
            if (_enableDebugLogs)
            {
                Debug.Log($"[LocationLicenses] {location.locationName} already unlocked");
            }
            return false;
        }

        // Check prerequisites
        if (!CheckPrerequisites(location))
        {
            if (_enableDebugLogs)
            {
                Debug.Log($"[LocationLicenses] Prerequisites not met for {location.locationName}");
            }
            EventSystem.Publish("LocationPrerequisitesNotMet", location);
            return false;
        }

        // Try to spend money
        if (EconomySystem.Instance.SpendMoney(location.cost, $"Unlock {location.locationName}"))
        {
            UnlockLocationInternal(locationID, false);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Internal method to unlock a location.
    /// </summary>
    private void UnlockLocationInternal(string locationID, bool isFree)
    {
        if (_unlockedLocations.Contains(locationID))
        {
            return; // Already unlocked
        }

        _unlockedLocations.Add(locationID);

        if (_locationsByID.TryGetValue(locationID, out LocationLicenseData location))
        {
            // Fire events
            OnLocationUnlocked?.Invoke(location);
            EventSystem.Publish("LocationUnlocked", location);

            if (_enableDebugLogs)
            {
                string costText = isFree ? "FREE" : $"${location.cost:F2}";
                Debug.Log($"[LocationLicenses] Unlocked {location.locationName} ({costText})");
            }

            // Check for newly available locations
            CheckNewlyAvailableLocations();
        }
    }

    /// <summary>
    /// Checks if prerequisites are met for a location.
    /// </summary>
    private bool CheckPrerequisites(LocationLicenseData location)
    {
        if (location.prerequisites == null || location.prerequisites.Length == 0)
        {
            return true;
        }

        foreach (var prereqID in location.prerequisites)
        {
            if (!IsLocationUnlocked(prereqID))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Checks for newly available locations and fires events.
    /// </summary>
    private void CheckNewlyAvailableLocations()
    {
        foreach (var location in _allLocations)
        {
            if (!IsLocationUnlocked(location.locationID) && CanPurchaseLocation(location.locationID))
            {
                OnLocationAvailable?.Invoke(location);
            }
        }
    }

    #endregion

    #region Query Methods

    /// <summary>
    /// Checks if a location is unlocked.
    /// </summary>
    public bool IsLocationUnlocked(string locationID)
    {
        return _unlockedLocations.Contains(locationID);
    }

    /// <summary>
    /// Checks if a location can be purchased right now.
    /// </summary>
    public bool CanPurchaseLocation(string locationID)
    {
        if (!_locationsByID.TryGetValue(locationID, out LocationLicenseData location))
        {
            return false;
        }

        // Already unlocked?
        if (IsLocationUnlocked(locationID)) return false;

        // Prerequisites met?
        if (!CheckPrerequisites(location)) return false;

        // Can afford?
        return EconomySystem.Instance.CanAffordMoney(location.cost);
    }

    /// <summary>
    /// Gets a location by ID.
    /// </summary>
    public LocationLicenseData GetLocation(string locationID)
    {
        return _locationsByID.TryGetValue(locationID, out LocationLicenseData location) ? location : null;
    }

    /// <summary>
    /// Gets all unlocked locations.
    /// </summary>
    public List<LocationLicenseData> GetUnlockedLocations()
    {
        return _allLocations.Where(l => IsLocationUnlocked(l.locationID)).ToList();
    }

    /// <summary>
    /// Gets all locations available for purchase.
    /// </summary>
    public List<LocationLicenseData> GetAvailableLocations()
    {
        return _allLocations.Where(l => CanPurchaseLocation(l.locationID)).ToList();
    }

    /// <summary>
    /// Gets all locations by tier.
    /// </summary>
    public List<LocationLicenseData> GetLocationsByTier(int tier)
    {
        return _allLocations.Where(l => l.tier == tier).ToList();
    }

    /// <summary>
    /// Gets progression percentage (locations unlocked / total).
    /// </summary>
    public float GetProgressionPercentage()
    {
        return (_unlockedLocations.Count / (float)_allLocations.Count) * 100f;
    }

    #endregion

    #region Save/Load

    private void OnGatheringSaveData(SaveData data)
    {
        data.unlockedLocations.Clear();
        data.unlockedLocations.AddRange(_unlockedLocations);

        if (_enableDebugLogs)
        {
            Debug.Log($"[LocationLicenses] Saved {_unlockedLocations.Count} unlocked locations");
        }
    }

    private void OnApplyingSaveData(SaveData data)
    {
        _unlockedLocations.Clear();
        foreach (var locationID in data.unlockedLocations)
        {
            _unlockedLocations.Add(locationID);
        }

        // Ensure starter location is always unlocked
        if (!_unlockedLocations.Contains("starter_lake"))
        {
            _unlockedLocations.Add("starter_lake");
        }

        if (_enableDebugLogs)
        {
            Debug.Log($"[LocationLicenses] Loaded {_unlockedLocations.Count} unlocked locations");
        }
    }

    #endregion

    #region Debug Methods

    [ContextMenu("Print All Locations")]
    public void PrintAllLocations()
    {
        Debug.Log($"=== All Locations ({_allLocations.Count}) ===");
        foreach (var location in _allLocations.OrderBy(l => l.tier).ThenBy(l => l.cost))
        {
            string status = IsLocationUnlocked(location.locationID) ? "UNLOCKED" : "LOCKED";
            Debug.Log($"[{status}] Tier {location.tier}: {location.locationName} (${location.cost:F2})");
        }
        Debug.Log($"Progression: {GetProgressionPercentage():F1}% complete");
    }

    [ContextMenu("Unlock All Locations (Debug)")]
    private void DebugUnlockAll()
    {
        foreach (var location in _allLocations)
        {
            UnlockLocationInternal(location.locationID, true);
        }
        Debug.Log("[LocationLicenses] All locations unlocked (DEBUG)");
    }

    [ContextMenu("Purchase Rocky Coastline")]
    private void DebugPurchaseRocky()
    {
        PurchaseLicense("rocky_coastline");
    }

    #endregion
}

#region Data Structures

/// <summary>
/// Location license data.
/// </summary>
[System.Serializable]
public class LocationLicenseData
{
    public string locationID;
    public string locationName;
    public string description;
    public float cost;
    public int tier; // 1 = starter, 2 = mid, 3 = late, 4 = endgame
    public LocationDifficulty difficulty;
    public bool isStarterLocation = false;
    public string[] prerequisites;
    public FishRarity[] dominantFishRarities;
    public float hazardLevel; // 0.0 to 1.0
    public Vector3 worldPosition;
    public string sceneName;
}

/// <summary>
/// Location difficulty rating.
/// </summary>
public enum LocationDifficulty
{
    Easy,
    Medium,
    Hard,
    Extreme
}

#endregion

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Agent 8: Fish AI & Behavior Agent - FishManager.cs
/// Singleton that manages all active fish in the game world.
/// Handles spawn rate modifiers based on time, weather, and location.
/// Integrates with Agent 3 (Time/Environment) and Agent 5 (Fishing).
/// </summary>
public class FishManager : MonoBehaviour
{
    public static FishManager Instance { get; private set; }

    [Header("Spawn Rate Configuration")]
    [Tooltip("Base spawn rate multiplier (1.0 = normal)")]
    [SerializeField] private float baseSpawnRate = 1.0f;

    [Header("Time Modifiers")]
    [SerializeField] private float daySpawnModifier = 1.0f;
    [SerializeField] private float duskSpawnModifier = 1.5f;
    [SerializeField] private float nightSpawnModifier = 2.0f;
    [SerializeField] private float dawnSpawnModifier = 1.5f;

    [Header("Weather Modifiers")]
    [SerializeField] private float clearWeatherModifier = 1.0f;
    [SerializeField] private float rainWeatherModifier = 1.5f;   // +50% as per spec
    [SerializeField] private float stormWeatherModifier = 2.5f;  // +150% rare fish as per spec
    [SerializeField] private float fogWeatherModifier = 1.3f;

    [Header("Rare Fish Spawn Boost")]
    [Tooltip("Additional multiplier for rare fish during storms")]
    [SerializeField] private float stormRareFishBoost = 2.5f;

    [Tooltip("Additional multiplier for rare fish at night")]
    [SerializeField] private float nightRareFishBoost = 2.0f;

    [Header("Active Fish Management")]
    [SerializeField] private List<GameObject> activeFish = new List<GameObject>();

    [Tooltip("Maximum number of active fish at once")]
    [SerializeField] private int maxActiveFish = 50;

    [Tooltip("Distance at which fish are despawned")]
    [SerializeField] private float despawnDistance = 100f;

    [Header("Events")]
    public System.Action<Fish> OnRareFishSpawned;
    public System.Action<FishSpeciesData> OnFishSpawned;
    public System.Action<GameObject> OnFishDespawned;

    // Current game state (updated by Time/Environment system)
    private TimeOfDay currentTime = TimeOfDay.Day;
    private WeatherType currentWeather = WeatherType.Clear;
    private string currentLocationID = "starter_lake";
    private Vector3 playerPosition = Vector3.zero;

    private FishDatabase fishDatabase;

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
        fishDatabase = FishDatabase.Instance;

        if (fishDatabase == null)
        {
            Debug.LogError("[FishManager] FishDatabase not found! Fish spawning will not work.");
        }

        SubscribeToEvents();
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    /// <summary>
    /// Subscribes to time and weather events from Agent 3.
    /// </summary>
    private void SubscribeToEvents()
    {
        // Subscribe to Time/Environment events (Agent 3)
        // EventSystem.Subscribe("TimeChanged", OnTimeChanged);
        // EventSystem.Subscribe("WeatherChanged", OnWeatherChanged);
        // EventSystem.Subscribe("PlayerMoved", OnPlayerMoved);
        // EventSystem.Subscribe("LocationChanged", OnLocationChanged);

        // TODO: Replace with actual event system when Agent 3 integration is complete
        Debug.Log("[FishManager] Event subscriptions ready (waiting for Agent 3 integration).");
    }

    /// <summary>
    /// Unsubscribes from events.
    /// </summary>
    private void UnsubscribeFromEvents()
    {
        // EventSystem.Unsubscribe("TimeChanged", OnTimeChanged);
        // EventSystem.Unsubscribe("WeatherChanged", OnWeatherChanged);
        // EventSystem.Unsubscribe("PlayerMoved", OnPlayerMoved);
        // EventSystem.Unsubscribe("LocationChanged", OnLocationChanged);
    }

    /// <summary>
    /// Spawns a fish based on current conditions.
    /// Main method called by Agent 5 (Fishing) or FishSpawner.
    /// </summary>
    public GameObject SpawnFish(Vector3 position, string locationID = null, TimeOfDay? timeOverride = null, WeatherType? weatherOverride = null)
    {
        if (fishDatabase == null)
        {
            Debug.LogError("[FishManager] Cannot spawn fish - FishDatabase not initialized!");
            return null;
        }

        if (activeFish.Count >= maxActiveFish)
        {
            Debug.LogWarning("[FishManager] Max active fish reached. Despawning oldest fish.");
            DespawnOldestFish();
        }

        // Use provided values or current state
        TimeOfDay spawnTime = timeOverride ?? currentTime;
        WeatherType spawnWeather = weatherOverride ?? currentWeather;
        string spawnLocation = locationID ?? currentLocationID;

        // Get possible fish for current conditions
        List<FishSpeciesData> possibleFish = fishDatabase.GetFilteredFish(
            locationID: spawnLocation,
            time: spawnTime
        );

        if (possibleFish.Count == 0)
        {
            Debug.LogWarning($"[FishManager] No fish available for location: {spawnLocation}, time: {spawnTime}");
            return null;
        }

        // Roll for fish rarity with modifiers
        FishRarity selectedRarity = RollFishRarity(spawnTime, spawnWeather);

        // Filter by rarity
        List<FishSpeciesData> fishOfRarity = possibleFish.Where(f => f.rarity == selectedRarity).ToList();

        // If no fish of that rarity, fall back to all possible fish
        if (fishOfRarity.Count == 0)
        {
            fishOfRarity = possibleFish;
        }

        // Select random fish
        FishSpeciesData selectedFish = fishOfRarity[Random.Range(0, fishOfRarity.Count)];

        // Instantiate fish
        GameObject fishObject = InstantiateFish(selectedFish, position);

        if (fishObject != null)
        {
            activeFish.Add(fishObject);
            OnFishSpawned?.Invoke(selectedFish);

            // Trigger rare fish event
            if (selectedFish.rarity == FishRarity.Rare || selectedFish.rarity == FishRarity.Legendary || selectedFish.isAberrant)
            {
                Fish fishInstance = selectedFish.ToFishInstance();
                OnRareFishSpawned?.Invoke(fishInstance);
                Debug.Log($"[FishManager] Rare fish spawned: {selectedFish.fishName} ({selectedFish.rarity})");
            }
        }

        return fishObject;
    }

    /// <summary>
    /// Rolls for fish rarity based on current conditions.
    /// Implements spawn rate formula: Base × Time × Weather × Location
    /// </summary>
    private FishRarity RollFishRarity(TimeOfDay time, WeatherType weather)
    {
        float rarityRoll = Random.value; // 0.0 to 1.0

        // Calculate spawn rate modifier
        float timeModifier = GetTimeModifier(time);
        float weatherModifier = GetWeatherModifier(weather);

        // Apply rare fish boosts
        float rareFishBoost = 1.0f;
        if (time == TimeOfDay.Night)
        {
            rareFishBoost *= nightRareFishBoost;
        }
        if (weather == WeatherType.Storm)
        {
            rareFishBoost *= stormRareFishBoost;
        }

        float totalModifier = baseSpawnRate * timeModifier * weatherModifier * rareFishBoost;

        // Adjust rarity thresholds based on modifiers
        // Base: 60% Common, 25% Uncommon, 10% Rare, 4% Legendary, 1% Aberrant
        float commonThreshold = 0.60f;
        float uncommonThreshold = 0.85f;
        float rareThreshold = 0.95f;
        float legendaryThreshold = 0.99f;

        // Shift thresholds based on modifiers (higher modifiers = more rare fish)
        float shift = (totalModifier - 1.0f) * 0.1f;
        commonThreshold = Mathf.Max(0.3f, commonThreshold - shift);
        uncommonThreshold = Mathf.Max(commonThreshold + 0.1f, uncommonThreshold - shift * 0.5f);
        rareThreshold = Mathf.Max(uncommonThreshold + 0.05f, rareThreshold - shift * 0.3f);
        legendaryThreshold = Mathf.Max(rareThreshold + 0.02f, legendaryThreshold - shift * 0.2f);

        // Determine rarity
        if (rarityRoll < commonThreshold)
            return FishRarity.Common;
        else if (rarityRoll < uncommonThreshold)
            return FishRarity.Uncommon;
        else if (rarityRoll < rareThreshold)
            return FishRarity.Rare;
        else if (rarityRoll < legendaryThreshold)
            return FishRarity.Legendary;
        else
            return FishRarity.Aberrant;
    }

    /// <summary>
    /// Gets the spawn rate modifier for the given time of day.
    /// </summary>
    private float GetTimeModifier(TimeOfDay time)
    {
        return time switch
        {
            TimeOfDay.Day => daySpawnModifier,
            TimeOfDay.Dusk => duskSpawnModifier,
            TimeOfDay.Night => nightSpawnModifier,
            TimeOfDay.Dawn => dawnSpawnModifier,
            _ => 1.0f
        };
    }

    /// <summary>
    /// Gets the spawn rate modifier for the given weather.
    /// </summary>
    private float GetWeatherModifier(WeatherType weather)
    {
        return weather switch
        {
            WeatherType.Clear => clearWeatherModifier,
            WeatherType.Rain => rainWeatherModifier,
            WeatherType.Storm => stormWeatherModifier,
            WeatherType.Fog => fogWeatherModifier,
            _ => 1.0f
        };
    }

    /// <summary>
    /// Instantiates a fish GameObject from species data.
    /// </summary>
    private GameObject InstantiateFish(FishSpeciesData species, Vector3 position)
    {
        GameObject fishPrefab = species.fishPrefab;

        if (fishPrefab == null)
        {
            Debug.LogWarning($"[FishManager] No prefab assigned for {species.fishName}. Creating empty GameObject.");
            fishPrefab = new GameObject($"Fish_{species.fishName}");
        }

        GameObject fishObject = Instantiate(fishPrefab, position, Quaternion.identity);
        fishObject.name = $"{species.fishName}_{activeFish.Count}";

        // Add FishAI component if not present
        FishAI fishAI = fishObject.GetComponent<FishAI>();
        if (fishAI == null)
        {
            fishAI = fishObject.AddComponent<FishAI>();
        }

        // Initialize fish with species data
        fishAI.Initialize(species);

        return fishObject;
    }

    /// <summary>
    /// Gets all fish within a radius of a position.
    /// Used by Agent 5 (Fishing) to detect nearby fish.
    /// </summary>
    public List<GameObject> GetFishInArea(Vector3 position, float radius)
    {
        return activeFish.Where(fish =>
            fish != null && Vector3.Distance(fish.transform.position, position) <= radius
        ).ToList();
    }

    /// <summary>
    /// Despawns a specific fish.
    /// </summary>
    public void DespawnFish(GameObject fish)
    {
        if (activeFish.Contains(fish))
        {
            activeFish.Remove(fish);
            OnFishDespawned?.Invoke(fish);
            Destroy(fish);
        }
    }

    /// <summary>
    /// Despawns the oldest fish.
    /// </summary>
    private void DespawnOldestFish()
    {
        if (activeFish.Count > 0)
        {
            GameObject oldestFish = activeFish[0];
            DespawnFish(oldestFish);
        }
    }

    /// <summary>
    /// Despawns fish that are too far from the player.
    /// </summary>
    public void DespawnDistantFish()
    {
        for (int i = activeFish.Count - 1; i >= 0; i--)
        {
            GameObject fish = activeFish[i];
            if (fish == null)
            {
                activeFish.RemoveAt(i);
                continue;
            }

            float distance = Vector3.Distance(fish.transform.position, playerPosition);
            if (distance > despawnDistance)
            {
                DespawnFish(fish);
            }
        }
    }

    /// <summary>
    /// Updates current time (called by Agent 3).
    /// </summary>
    public void UpdateTime(TimeOfDay newTime)
    {
        currentTime = newTime;
        Debug.Log($"[FishManager] Time updated to: {newTime}");
    }

    /// <summary>
    /// Updates current weather (called by Agent 3).
    /// </summary>
    public void UpdateWeather(WeatherType newWeather)
    {
        currentWeather = newWeather;
        Debug.Log($"[FishManager] Weather updated to: {newWeather}");
    }

    /// <summary>
    /// Updates current location (called by Agent 14).
    /// </summary>
    public void UpdateLocation(string newLocationID)
    {
        currentLocationID = newLocationID;
        Debug.Log($"[FishManager] Location updated to: {newLocationID}");
    }

    /// <summary>
    /// Updates player position.
    /// </summary>
    public void UpdatePlayerPosition(Vector3 newPosition)
    {
        playerPosition = newPosition;
    }

    /// <summary>
    /// Gets the current spawn rate multiplier.
    /// </summary>
    public float GetCurrentSpawnRate()
    {
        float timeModifier = GetTimeModifier(currentTime);
        float weatherModifier = GetWeatherModifier(currentWeather);
        return baseSpawnRate * timeModifier * weatherModifier;
    }

    /// <summary>
    /// Gets the number of active fish.
    /// </summary>
    public int GetActiveFishCount()
    {
        return activeFish.Count(f => f != null);
    }

    /// <summary>
    /// Clears all active fish.
    /// </summary>
    public void ClearAllFish()
    {
        foreach (var fish in activeFish)
        {
            if (fish != null)
            {
                Destroy(fish);
            }
        }
        activeFish.Clear();
        Debug.Log("[FishManager] All fish cleared.");
    }

#if UNITY_EDITOR
    [ContextMenu("Print Fish Stats")]
    private void PrintFishStats()
    {
        Debug.Log($"=== Fish Manager Stats ===");
        Debug.Log($"Active Fish: {GetActiveFishCount()} / {maxActiveFish}");
        Debug.Log($"Current Time: {currentTime}");
        Debug.Log($"Current Weather: {currentWeather}");
        Debug.Log($"Current Location: {currentLocationID}");
        Debug.Log($"Current Spawn Rate: {GetCurrentSpawnRate():F2}x");
    }

    [ContextMenu("Spawn Test Fish")]
    private void SpawnTestFish()
    {
        SpawnFish(playerPosition);
    }

    [ContextMenu("Clear All Fish")]
    private void ClearFishEditor()
    {
        ClearAllFish();
    }
#endif
}

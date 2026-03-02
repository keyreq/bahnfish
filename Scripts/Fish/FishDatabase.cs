using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Agent 8: Fish AI & Behavior Agent - FishDatabase.cs
/// Manages all fish species data and provides filtering/query methods.
/// Loads fish species from ScriptableObjects in Resources folder.
/// </summary>
public class FishDatabase : MonoBehaviour
{
    public static FishDatabase Instance { get; private set; }

    [Header("Fish Species")]
    [SerializeField] private List<FishSpeciesData> allFishSpecies = new List<FishSpeciesData>();

    private Dictionary<string, FishSpeciesData> fishByID = new Dictionary<string, FishSpeciesData>();
    private Dictionary<FishRarity, List<FishSpeciesData>> fishByRarity = new Dictionary<FishRarity, List<FishSpeciesData>>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadAllFishSpecies();
        BuildLookupTables();
    }

    /// <summary>
    /// Loads all fish species from Resources/FishSpecies folder.
    /// </summary>
    private void LoadAllFishSpecies()
    {
        FishSpeciesData[] loadedFish = Resources.LoadAll<FishSpeciesData>("FishSpecies");

        if (loadedFish.Length == 0)
        {
            Debug.LogWarning("[FishDatabase] No fish species found in Resources/FishSpecies folder!");
            return;
        }

        allFishSpecies.Clear();
        allFishSpecies.AddRange(loadedFish);

        Debug.Log($"[FishDatabase] Loaded {allFishSpecies.Count} fish species.");
    }

    /// <summary>
    /// Builds lookup dictionaries for fast queries.
    /// </summary>
    private void BuildLookupTables()
    {
        fishByID.Clear();
        fishByRarity.Clear();

        // Initialize rarity lists
        foreach (FishRarity rarity in System.Enum.GetValues(typeof(FishRarity)))
        {
            fishByRarity[rarity] = new List<FishSpeciesData>();
        }

        // Populate lookup tables
        foreach (var fish in allFishSpecies)
        {
            if (fish == null) continue;

            // ID lookup
            if (!fishByID.ContainsKey(fish.fishID))
            {
                fishByID[fish.fishID] = fish;
            }
            else
            {
                Debug.LogWarning($"[FishDatabase] Duplicate fish ID found: {fish.fishID}");
            }

            // Rarity lookup
            fishByRarity[fish.rarity].Add(fish);
        }
    }

    /// <summary>
    /// Gets a fish species by its unique ID.
    /// </summary>
    public FishSpeciesData GetFishByID(string id)
    {
        if (fishByID.TryGetValue(id, out FishSpeciesData fish))
        {
            return fish;
        }

        Debug.LogWarning($"[FishDatabase] Fish ID not found: {id}");
        return null;
    }

    /// <summary>
    /// Gets all fish species of a specific rarity.
    /// </summary>
    public List<FishSpeciesData> GetFishByRarity(FishRarity rarity)
    {
        if (fishByRarity.TryGetValue(rarity, out List<FishSpeciesData> fishList))
        {
            return new List<FishSpeciesData>(fishList); // Return a copy
        }

        return new List<FishSpeciesData>();
    }

    /// <summary>
    /// Gets fish species that can spawn at a specific depth.
    /// </summary>
    public List<FishSpeciesData> GetFishByDepth(float depth)
    {
        return allFishSpecies.Where(f =>
            depth >= f.minDepth && depth <= f.maxDepth
        ).ToList();
    }

    /// <summary>
    /// Gets fish species that prefer a specific time of day.
    /// </summary>
    public List<FishSpeciesData> GetFishByTime(TimeOfDay time)
    {
        return allFishSpecies.Where(f => f.preferredTime == time).ToList();
    }

    /// <summary>
    /// Gets fish species that can spawn in a specific location.
    /// </summary>
    public List<FishSpeciesData> GetFishByLocation(string locationID)
    {
        return allFishSpecies.Where(f =>
            f.allowedLocations.Contains(locationID)
        ).ToList();
    }

    /// <summary>
    /// Gets fish species that match multiple filters.
    /// </summary>
    public List<FishSpeciesData> GetFilteredFish(
        string locationID = null,
        TimeOfDay? time = null,
        float? depth = null,
        FishRarity? rarity = null,
        bool? aberrantOnly = null)
    {
        IEnumerable<FishSpeciesData> filtered = allFishSpecies;

        if (!string.IsNullOrEmpty(locationID))
        {
            filtered = filtered.Where(f => f.allowedLocations.Contains(locationID));
        }

        if (time.HasValue)
        {
            filtered = filtered.Where(f => f.preferredTime == time.Value);
        }

        if (depth.HasValue)
        {
            filtered = filtered.Where(f =>
                depth.Value >= f.minDepth && depth.Value <= f.maxDepth
            );
        }

        if (rarity.HasValue)
        {
            filtered = filtered.Where(f => f.rarity == rarity.Value);
        }

        if (aberrantOnly.HasValue)
        {
            filtered = filtered.Where(f => f.isAberrant == aberrantOnly.Value);
        }

        return filtered.ToList();
    }

    /// <summary>
    /// Gets a random fish species from the filtered results.
    /// </summary>
    public FishSpeciesData GetRandomFish(
        string locationID = null,
        TimeOfDay? time = null,
        float? depth = null,
        FishRarity? rarity = null,
        bool? aberrantOnly = null)
    {
        List<FishSpeciesData> filtered = GetFilteredFish(locationID, time, depth, rarity, aberrantOnly);

        if (filtered.Count == 0)
        {
            Debug.LogWarning("[FishDatabase] No fish match the given filters!");
            return null;
        }

        return filtered[Random.Range(0, filtered.Count)];
    }

    /// <summary>
    /// Gets all fish species.
    /// </summary>
    public List<FishSpeciesData> GetAllFish()
    {
        return new List<FishSpeciesData>(allFishSpecies);
    }

    /// <summary>
    /// Gets the total count of fish species.
    /// </summary>
    public int GetTotalFishCount()
    {
        return allFishSpecies.Count;
    }

    /// <summary>
    /// Checks if a fish species exists by ID.
    /// </summary>
    public bool FishExists(string id)
    {
        return fishByID.ContainsKey(id);
    }

    /// <summary>
    /// Reloads all fish species from Resources (useful for editor tools).
    /// </summary>
    public void ReloadDatabase()
    {
        LoadAllFishSpecies();
        BuildLookupTables();
        Debug.Log("[FishDatabase] Database reloaded.");
    }

    /// <summary>
    /// Gets statistics about the fish database.
    /// </summary>
    public string GetDatabaseStats()
    {
        System.Text.StringBuilder stats = new System.Text.StringBuilder();
        stats.AppendLine("=== Fish Database Statistics ===");
        stats.AppendLine($"Total Fish Species: {allFishSpecies.Count}");

        foreach (FishRarity rarity in System.Enum.GetValues(typeof(FishRarity)))
        {
            int count = fishByRarity[rarity].Count;
            stats.AppendLine($"{rarity}: {count}");
        }

        int aberrantCount = allFishSpecies.Count(f => f.isAberrant);
        stats.AppendLine($"Aberrant Variants: {aberrantCount}");

        return stats.ToString();
    }

#if UNITY_EDITOR
    [ContextMenu("Print Database Stats")]
    private void PrintDatabaseStats()
    {
        Debug.Log(GetDatabaseStats());
    }

    [ContextMenu("Reload Database")]
    private void EditorReloadDatabase()
    {
        ReloadDatabase();
    }
#endif
}

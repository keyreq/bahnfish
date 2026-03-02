using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

/// <summary>
/// Agent 16: Aquarium & Breeding Specialist - AquariumManager.cs
/// Central manager for the aquarium system.
/// Handles tank management, fish display, and integration with other systems.
/// </summary>
public class AquariumManager : MonoBehaviour
{
    private static AquariumManager _instance;
    public static AquariumManager Instance => _instance;

    [Header("Tank Database")]
    [SerializeField] private List<FishTankData> availableTanks = new List<FishTankData>();

    [Header("Configuration")]
    [SerializeField] private float dailyUpdateHour = 8f; // 8 AM game time
    [SerializeField] private bool autoFeed = false;
    [SerializeField] private float autoFeederCostPerDay = 10f;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;

    // Player's tanks
    private Dictionary<string, TankInstance> ownedTanks = new Dictionary<string, TankInstance>();

    // All display fish across all tanks
    private Dictionary<string, DisplayFish> allDisplayFish = new Dictionary<string, DisplayFish>();

    // Daily update tracking
    private float lastUpdateTime = -1f;

    // Events
    public event Action<TankInstance> OnTankPurchased;
    public event Action<TankInstance> OnTankUpgraded;
    public event Action<DisplayFish, string> OnFishAddedToTank;
    public event Action<DisplayFish, string> OnFishRemovedFromTank;
    public event Action<float> OnExhibitionIncomeEarned;

    #region Unity Lifecycle

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        LoadTankDatabase();
    }

    private void Start()
    {
        // Subscribe to events
        EventSystem.Subscribe<Fish>("FishCaught", OnFishCaught);
        EventSystem.Subscribe<TimeChangedEventData>("TimeChanged", OnTimeChanged);
        EventSystem.Subscribe<OffspringBornData>("OffspringBorn", OnOffspringBorn);
        EventSystem.Subscribe<FishDeathData>("FishDied", OnFishDied);

        // Subscribe to save/load
        EventSystem.Subscribe<SaveData>("GatheringSaveData", OnGatheringSaveData);
        EventSystem.Subscribe<SaveData>("ApplyingSaveData", OnApplyingSaveData);

        if (enableDebugLogs)
        {
            Debug.Log("[AquariumManager] Initialized");
        }
    }

    private void OnDestroy()
    {
        EventSystem.Unsubscribe<Fish>("FishCaught", OnFishCaught);
        EventSystem.Unsubscribe<TimeChangedEventData>("TimeChanged", OnTimeChanged);
        EventSystem.Unsubscribe<OffspringBornData>("OffspringBorn", OnOffspringBorn);
        EventSystem.Unsubscribe<FishDeathData>("FishDied", OnFishDied);
        EventSystem.Unsubscribe<SaveData>("GatheringSaveData", OnGatheringSaveData);
        EventSystem.Unsubscribe<SaveData>("ApplyingSaveData", OnApplyingSaveData);
    }

    #endregion

    #region Tank Database

    /// <summary>
    /// Loads all tank types from Resources.
    /// </summary>
    private void LoadTankDatabase()
    {
        FishTankData[] tanks = Resources.LoadAll<FishTankData>("Tanks");

        if (tanks.Length == 0)
        {
            Debug.LogWarning("[AquariumManager] No tank types found in Resources/Tanks!");
            return;
        }

        availableTanks.Clear();
        availableTanks.AddRange(tanks);

        if (enableDebugLogs)
        {
            Debug.Log($"[AquariumManager] Loaded {availableTanks.Count} tank types");
        }
    }

    /// <summary>
    /// Gets a tank definition by ID.
    /// </summary>
    public FishTankData GetTankData(string tankID)
    {
        return availableTanks.FirstOrDefault(t => t.tankID == tankID);
    }

    /// <summary>
    /// Gets all available tank types.
    /// </summary>
    public List<FishTankData> GetAvailableTankTypes()
    {
        return new List<FishTankData>(availableTanks);
    }

    #endregion

    #region Tank Management

    /// <summary>
    /// Purchases a new tank.
    /// </summary>
    public bool PurchaseTank(string tankID)
    {
        FishTankData tankData = GetTankData(tankID);

        if (tankData == null)
        {
            Debug.LogError($"[AquariumManager] Tank ID not found: {tankID}");
            return false;
        }

        // Check if already owned
        if (ownedTanks.ContainsKey(tankID))
        {
            Debug.LogWarning($"[AquariumManager] Already own tank: {tankData.tankName}");
            return false;
        }

        // Check prerequisites
        if (!string.IsNullOrEmpty(tankData.prerequisiteTankID) && !ownedTanks.ContainsKey(tankData.prerequisiteTankID))
        {
            Debug.LogWarning($"[AquariumManager] Must own {tankData.prerequisiteTankID} first");
            return false;
        }

        // Check affordability
        if (EconomySystem.Instance != null && !EconomySystem.Instance.CanAffordMoney(tankData.purchaseCost))
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[AquariumManager] Cannot afford tank: ${tankData.purchaseCost}");
            }
            return false;
        }

        // Purchase tank
        if (EconomySystem.Instance != null)
        {
            EconomySystem.Instance.SpendMoney(tankData.purchaseCost, $"Tank Purchase: {tankData.tankName}");
        }

        // Create tank instance
        TankInstance tank = new TankInstance(tankData);
        ownedTanks[tankID] = tank;

        // Fire events
        OnTankPurchased?.Invoke(tank);
        EventSystem.Publish("TankPurchased", tank);

        if (enableDebugLogs)
        {
            Debug.Log($"[AquariumManager] Purchased tank: {tankData.tankName}");
        }

        return true;
    }

    /// <summary>
    /// Upgrades a tank's capacity.
    /// </summary>
    public bool UpgradeTankCapacity(string tankID)
    {
        if (!ownedTanks.TryGetValue(tankID, out TankInstance tank))
        {
            return false;
        }

        if (tank.capacityLevel >= tank.tankData.maxCapacityUpgrades)
        {
            Debug.LogWarning("[AquariumManager] Tank capacity already at max");
            return false;
        }

        float cost = tank.tankData.GetCapacityUpgradeCost(tank.capacityLevel + 1);

        if (EconomySystem.Instance != null && !EconomySystem.Instance.CanAffordMoney(cost))
        {
            return false;
        }

        if (EconomySystem.Instance != null)
        {
            EconomySystem.Instance.SpendMoney(cost, $"Tank Capacity Upgrade: {tank.tankData.tankName}");
        }

        tank.capacityLevel++;

        OnTankUpgraded?.Invoke(tank);
        EventSystem.Publish("TankUpgraded", tank);

        if (enableDebugLogs)
        {
            Debug.Log($"[AquariumManager] Upgraded {tank.tankData.tankName} capacity to level {tank.capacityLevel}");
        }

        return true;
    }

    /// <summary>
    /// Upgrades other tank features.
    /// </summary>
    public bool UpgradeTank(string tankID, TankUpgradeType upgradeType)
    {
        if (!ownedTanks.TryGetValue(tankID, out TankInstance tank))
        {
            return false;
        }

        int currentLevel = tank.GetUpgradeLevel(upgradeType);
        int maxLevel = GetMaxUpgradeLevel(upgradeType);

        if (currentLevel >= maxLevel)
        {
            Debug.LogWarning($"[AquariumManager] {upgradeType} already at max level");
            return false;
        }

        float cost = GetUpgradeCost(upgradeType, currentLevel + 1);

        if (EconomySystem.Instance != null && !EconomySystem.Instance.CanAffordMoney(cost))
        {
            return false;
        }

        if (EconomySystem.Instance != null)
        {
            EconomySystem.Instance.SpendMoney(cost, $"Tank Upgrade: {upgradeType}");
        }

        tank.SetUpgradeLevel(upgradeType, currentLevel + 1);

        OnTankUpgraded?.Invoke(tank);
        EventSystem.Publish("TankUpgraded", new TankUpgradeData(tank, upgradeType, currentLevel + 1));

        if (enableDebugLogs)
        {
            Debug.Log($"[AquariumManager] Upgraded {upgradeType} to level {currentLevel + 1}");
        }

        return true;
    }

    /// <summary>
    /// Gets all owned tanks.
    /// </summary>
    public List<TankInstance> GetOwnedTanks()
    {
        return ownedTanks.Values.ToList();
    }

    /// <summary>
    /// Gets a specific tank instance.
    /// </summary>
    public TankInstance GetTank(string tankID)
    {
        ownedTanks.TryGetValue(tankID, out TankInstance tank);
        return tank;
    }

    /// <summary>
    /// Checks if player owns a tank.
    /// </summary>
    public bool OwnsTank(string tankID)
    {
        return ownedTanks.ContainsKey(tankID);
    }

    #endregion

    #region Fish Management

    /// <summary>
    /// Adds a fish to a tank.
    /// </summary>
    public bool AddFishToTank(DisplayFish fish, string tankID)
    {
        if (fish == null)
        {
            return false;
        }

        if (!ownedTanks.TryGetValue(tankID, out TankInstance tank))
        {
            Debug.LogWarning($"[AquariumManager] Tank not found: {tankID}");
            return false;
        }

        // Check capacity
        if (tank.GetFishCount() >= tank.GetMaxCapacity())
        {
            Debug.LogWarning("[AquariumManager] Tank is at capacity");
            return false;
        }

        // Check compatibility
        FishSpeciesData speciesData = FishDatabase.Instance?.GetFishByID(fish.speciesID);
        if (speciesData != null && !tank.tankData.CanHoldFish(speciesData))
        {
            Debug.LogWarning($"[AquariumManager] Tank cannot hold {fish.speciesName}");
            return false;
        }

        // Add fish
        fish.tankID = tankID;
        tank.fishIDs.Add(fish.uniqueID);
        allDisplayFish[fish.uniqueID] = fish;

        // Fire events
        OnFishAddedToTank?.Invoke(fish, tankID);
        EventSystem.Publish("FishAddedToTank", new FishTankChangeData(fish, tankID));

        if (enableDebugLogs)
        {
            Debug.Log($"[AquariumManager] Added {fish.speciesName} to {tank.tankData.tankName}");
        }

        return true;
    }

    /// <summary>
    /// Removes a fish from its tank.
    /// </summary>
    public bool RemoveFishFromTank(string fishID)
    {
        if (!allDisplayFish.TryGetValue(fishID, out DisplayFish fish))
        {
            return false;
        }

        string tankID = fish.tankID;

        if (ownedTanks.TryGetValue(tankID, out TankInstance tank))
        {
            tank.fishIDs.Remove(fishID);
        }

        fish.tankID = null;

        OnFishRemovedFromTank?.Invoke(fish, tankID);
        EventSystem.Publish("FishRemovedFromTank", new FishTankChangeData(fish, tankID));

        if (enableDebugLogs)
        {
            Debug.Log($"[AquariumManager] Removed {fish.speciesName} from tank");
        }

        return true;
    }

    /// <summary>
    /// Gets all fish in a specific tank.
    /// </summary>
    public List<DisplayFish> GetTankFish(string tankID)
    {
        if (!ownedTanks.TryGetValue(tankID, out TankInstance tank))
        {
            return new List<DisplayFish>();
        }

        List<DisplayFish> fishList = new List<DisplayFish>();

        foreach (string fishID in tank.fishIDs)
        {
            if (allDisplayFish.TryGetValue(fishID, out DisplayFish fish))
            {
                fishList.Add(fish);
            }
        }

        return fishList;
    }

    /// <summary>
    /// Gets a specific display fish.
    /// </summary>
    public DisplayFish GetDisplayFish(string fishID)
    {
        allDisplayFish.TryGetValue(fishID, out DisplayFish fish);
        return fish;
    }

    /// <summary>
    /// Gets all display fish across all tanks.
    /// </summary>
    public List<DisplayFish> GetAllDisplayFish()
    {
        return allDisplayFish.Values.ToList();
    }

    /// <summary>
    /// Feeds all fish in a tank.
    /// </summary>
    public void FeedTank(string tankID)
    {
        List<DisplayFish> fishList = GetTankFish(tankID);

        foreach (var fish in fishList)
        {
            fish.Feed();
        }

        if (enableDebugLogs)
        {
            Debug.Log($"[AquariumManager] Fed {fishList.Count} fish in tank {tankID}");
        }
    }

    /// <summary>
    /// Feeds all fish in all tanks.
    /// </summary>
    public void FeedAllTanks()
    {
        foreach (var fish in allDisplayFish.Values)
        {
            fish.Feed();
        }

        if (enableDebugLogs)
        {
            Debug.Log($"[AquariumManager] Fed all {allDisplayFish.Count} fish");
        }
    }

    #endregion

    #region Daily Updates & Exhibition Income

    /// <summary>
    /// Performs daily update for all tanks and fish.
    /// </summary>
    private void DailyUpdate(float currentTime)
    {
        if (enableDebugLogs)
        {
            Debug.Log("[AquariumManager] Performing daily update");
        }

        // Auto-feed if enabled
        if (autoFeed)
        {
            FeedAllTanks();

            if (EconomySystem.Instance != null)
            {
                float totalCost = ownedTanks.Count * autoFeederCostPerDay;
                EconomySystem.Instance.SpendMoney(totalCost, "Auto-Feeder Daily Cost");
            }
        }

        // Update all fish
        foreach (var tank in ownedTanks.Values)
        {
            float environmentQuality = tank.GetEnvironmentQuality();

            List<DisplayFish> tankFish = GetTankFish(tank.tankData.tankID);

            foreach (var fish in tankFish)
            {
                fish.UpdateDaily(environmentQuality);
            }

            // Calculate and pay exhibition income
            float dailyIncome = CalculateTankDailyIncome(tank);

            if (dailyIncome > 0 && EconomySystem.Instance != null)
            {
                EconomySystem.Instance.AddMoney(dailyIncome, $"Exhibition Income: {tank.tankData.tankName}");
            }
        }

        // Calculate total exhibition income
        float totalIncome = CalculateTotalDailyIncome();

        if (totalIncome > 0)
        {
            OnExhibitionIncomeEarned?.Invoke(totalIncome);
            EventSystem.Publish("ExhibitionIncomeEarned", totalIncome);
        }

        // Pay maintenance costs
        float maintenanceCost = CalculateTotalMaintenanceCost();

        if (maintenanceCost > 0 && EconomySystem.Instance != null)
        {
            EconomySystem.Instance.SpendMoney(maintenanceCost, "Tank Maintenance");
        }

        lastUpdateTime = currentTime;
    }

    /// <summary>
    /// Calculates daily exhibition income for a tank.
    /// </summary>
    private float CalculateTankDailyIncome(TankInstance tank)
    {
        List<DisplayFish> fishList = GetTankFish(tank.tankData.tankID);
        float income = 0f;

        foreach (var fish in fishList)
        {
            if (!fish.isAlive)
            {
                continue;
            }

            income += tank.tankData.GetDailyIncome(fish.rarity, 1);

            // Happiness bonus
            income *= (0.5f + fish.happiness * 0.5f);
        }

        return income;
    }

    /// <summary>
    /// Calculates total daily exhibition income.
    /// </summary>
    public float CalculateTotalDailyIncome()
    {
        float totalIncome = 0f;

        foreach (var tank in ownedTanks.Values)
        {
            totalIncome += CalculateTankDailyIncome(tank);
        }

        return totalIncome;
    }

    /// <summary>
    /// Calculates total daily maintenance cost.
    /// </summary>
    public float CalculateTotalMaintenanceCost()
    {
        float totalCost = 0f;

        foreach (var tank in ownedTanks.Values)
        {
            totalCost += tank.tankData.dailyMaintenanceCost;
        }

        return totalCost;
    }

    #endregion

    #region Event Handlers

    private void OnFishCaught(Fish fish)
    {
        // Optionally prompt player to add to aquarium
        // This would be handled by UI in full implementation
        if (enableDebugLogs)
        {
            Debug.Log($"[AquariumManager] Fish caught: {fish.name}. Player can add to aquarium.");
        }
    }

    private void OnTimeChanged(TimeChangedEventData timeData)
    {
        // Check for daily update
        if (lastUpdateTime < 0f)
        {
            lastUpdateTime = timeData.currentTime;
            return;
        }

        // Daily update at 8 AM
        if (lastUpdateTime < dailyUpdateHour && timeData.currentTime >= dailyUpdateHour)
        {
            DailyUpdate(timeData.currentTime);
        }

        // Handle day wrap-around
        if (timeData.currentTime < lastUpdateTime)
        {
            DailyUpdate(timeData.currentTime);
        }
    }

    private void OnOffspringBorn(OffspringBornData data)
    {
        // Add offspring to tank automatically
        AddFishToTank(data.offspring, data.pair.tankID);

        if (enableDebugLogs)
        {
            Debug.Log($"[AquariumManager] Offspring born and added to tank: {data.offspring.speciesName}");
        }
    }

    private void OnFishDied(FishDeathData data)
    {
        // Remove dead fish from tank
        RemoveFishFromTank(data.fish.uniqueID);

        if (enableDebugLogs)
        {
            Debug.Log($"[AquariumManager] Fish died: {data.fish.speciesName}");
        }
    }

    #endregion

    #region Save/Load Integration

    private void OnGatheringSaveData(SaveData data)
    {
        // Save aquarium data
        AquariumSaveData aquariumData = new AquariumSaveData
        {
            ownedTankIDs = ownedTanks.Keys.ToList(),
            tankInstances = ownedTanks.Values.ToList(),
            allDisplayFish = allDisplayFish.Values.ToList(),
            autoFeedEnabled = autoFeed,
            lastUpdateTime = lastUpdateTime
        };

        data.aquariumFish = allDisplayFish.Keys.ToList();

        if (enableDebugLogs)
        {
            Debug.Log($"[AquariumManager] Saved {ownedTanks.Count} tanks and {allDisplayFish.Count} fish");
        }
    }

    private void OnApplyingSaveData(SaveData data)
    {
        // In full implementation, restore aquarium state
        if (enableDebugLogs)
        {
            Debug.Log("[AquariumManager] Loaded aquarium save data");
        }
    }

    #endregion

    #region Upgrade Helpers

    private int GetMaxUpgradeLevel(TankUpgradeType upgradeType)
    {
        switch (upgradeType)
        {
            case TankUpgradeType.AutoFeeder:
                return 3;
            case TankUpgradeType.Filtration:
                return 3;
            case TankUpgradeType.Lighting:
                return 4;
            case TankUpgradeType.BreedingChamber:
                return 3;
            case TankUpgradeType.GeneticsLab:
                return 3;
            default:
                return 0;
        }
    }

    private float GetUpgradeCost(TankUpgradeType upgradeType, int level)
    {
        float baseCost = 200f;

        switch (upgradeType)
        {
            case TankUpgradeType.AutoFeeder:
                baseCost = 150f;
                break;
            case TankUpgradeType.Filtration:
                baseCost = 250f;
                break;
            case TankUpgradeType.Lighting:
                baseCost = 300f;
                break;
            case TankUpgradeType.BreedingChamber:
                baseCost = 500f;
                break;
            case TankUpgradeType.GeneticsLab:
                baseCost = 800f;
                break;
        }

        return baseCost * Mathf.Pow(2f, level - 1);
    }

    #endregion

    #region Debug Methods

    [ContextMenu("Print Aquarium Status")]
    private void DebugPrintStatus()
    {
        Debug.Log($"=== Aquarium Status ===\n" +
                 $"Owned Tanks: {ownedTanks.Count}\n" +
                 $"Total Fish: {allDisplayFish.Count}\n" +
                 $"Daily Income: ${CalculateTotalDailyIncome():F2}\n" +
                 $"Daily Costs: ${CalculateTotalMaintenanceCost():F2}\n" +
                 $"======================");
    }

    [ContextMenu("Force Daily Update")]
    private void DebugForceDailyUpdate()
    {
        DailyUpdate(8f);
    }

    #endregion
}

/// <summary>
/// Represents an owned tank instance.
/// </summary>
[Serializable]
public class TankInstance
{
    public string instanceID;
    public FishTankData tankData;
    public List<string> fishIDs;
    public DateTime purchaseDate;

    // Upgrade levels
    public int capacityLevel;
    public int autoFeederLevel;
    public int filtrationLevel;
    public int lightingLevel;
    public int breedingChamberLevel;
    public int geneticsLabLevel;

    public TankInstance(FishTankData data)
    {
        instanceID = Guid.NewGuid().ToString();
        tankData = data;
        fishIDs = new List<string>();
        purchaseDate = DateTime.Now;
        capacityLevel = 0;
        autoFeederLevel = 0;
        filtrationLevel = 0;
        lightingLevel = 0;
        breedingChamberLevel = 0;
        geneticsLabLevel = 0;
    }

    public int GetMaxCapacity()
    {
        return tankData.GetMaxCapacity(capacityLevel);
    }

    public int GetFishCount()
    {
        return fishIDs.Count;
    }

    public float GetEnvironmentQuality()
    {
        return tankData.GetEnvironmentQuality(filtrationLevel);
    }

    public int GetUpgradeLevel(TankUpgradeType upgradeType)
    {
        switch (upgradeType)
        {
            case TankUpgradeType.AutoFeeder:
                return autoFeederLevel;
            case TankUpgradeType.Filtration:
                return filtrationLevel;
            case TankUpgradeType.Lighting:
                return lightingLevel;
            case TankUpgradeType.BreedingChamber:
                return breedingChamberLevel;
            case TankUpgradeType.GeneticsLab:
                return geneticsLabLevel;
            default:
                return 0;
        }
    }

    public void SetUpgradeLevel(TankUpgradeType upgradeType, int level)
    {
        switch (upgradeType)
        {
            case TankUpgradeType.AutoFeeder:
                autoFeederLevel = level;
                break;
            case TankUpgradeType.Filtration:
                filtrationLevel = level;
                break;
            case TankUpgradeType.Lighting:
                lightingLevel = level;
                break;
            case TankUpgradeType.BreedingChamber:
                breedingChamberLevel = level;
                break;
            case TankUpgradeType.GeneticsLab:
                geneticsLabLevel = level;
                break;
        }
    }
}

/// <summary>
/// Tank upgrade types.
/// </summary>
[Serializable]
public enum TankUpgradeType
{
    AutoFeeder,
    Filtration,
    Lighting,
    BreedingChamber,
    GeneticsLab
}

/// <summary>
/// Save data structure for aquarium.
/// </summary>
[Serializable]
public class AquariumSaveData
{
    public List<string> ownedTankIDs;
    public List<TankInstance> tankInstances;
    public List<DisplayFish> allDisplayFish;
    public bool autoFeedEnabled;
    public float lastUpdateTime;
}

/// <summary>
/// Event data for fish added/removed from tank.
/// </summary>
[Serializable]
public struct FishTankChangeData
{
    public DisplayFish fish;
    public string tankID;

    public FishTankChangeData(DisplayFish fish, string tankID)
    {
        this.fish = fish;
        this.tankID = tankID;
    }
}

/// <summary>
/// Event data for tank upgrades.
/// </summary>
[Serializable]
public struct TankUpgradeData
{
    public TankInstance tank;
    public TankUpgradeType upgradeType;
    public int newLevel;

    public TankUpgradeData(TankInstance tank, TankUpgradeType upgradeType, int newLevel)
    {
        this.tank = tank;
        this.upgradeType = upgradeType;
        this.newLevel = newLevel;
    }
}

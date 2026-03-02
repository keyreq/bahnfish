using UnityEngine;

/// <summary>
/// Agent 14: Location & World Generation Agent - FastTravelSystem.cs
/// Handles instant dock-to-dock teleportation using relics.
/// Unlocked via Tidal Gate ability from progression system.
/// </summary>
public class FastTravelSystem : MonoBehaviour
{
    public static FastTravelSystem Instance { get; private set; }

    [Header("Fast Travel Settings")]
    [SerializeField] private bool isFastTravelUnlocked = false;
    [SerializeField] private int relicCostPerTravel = 1;
    [SerializeField] private bool requiresDockLocation = true;

    [Header("Visual Settings")]
    [SerializeField] private float teleportDuration = 0.5f;
    [SerializeField] private bool playTeleportEffect = true;

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
        // Subscribe to save/load events
        EventSystem.Subscribe<SaveData>("GatheringSaveData", OnGatheringSave);
        EventSystem.Subscribe<SaveData>("ApplyingSaveData", OnApplyingSave);

        Debug.Log("[FastTravelSystem] Initialized.");
    }

    private void OnDestroy()
    {
        EventSystem.Unsubscribe<SaveData>("GatheringSaveData", OnGatheringSave);
        EventSystem.Unsubscribe<SaveData>("ApplyingSaveData", OnApplyingSave);
    }

    #endregion

    #region Fast Travel Methods

    /// <summary>
    /// Attempts to fast travel to a target location.
    /// </summary>
    public bool FastTravel(string targetLocationID)
    {
        // Check if fast travel is unlocked
        if (!isFastTravelUnlocked)
        {
            Debug.LogWarning("[FastTravelSystem] Fast travel not unlocked yet!");
            EventSystem.Publish("FastTravelBlocked", "Fast travel ability not unlocked");
            return false;
        }

        // Check if player is at dock (if required)
        if (requiresDockLocation && !IsPlayerAtDock())
        {
            Debug.LogWarning("[FastTravelSystem] Must be at dock to fast travel!");
            EventSystem.Publish("FastTravelBlocked", "Must be at dock to fast travel");
            return false;
        }

        // Check if currently fishing or traveling
        if (IsFishing() || IsTraveling())
        {
            Debug.LogWarning("[FastTravelSystem] Cannot fast travel while fishing or traveling!");
            EventSystem.Publish("FastTravelBlocked", "Cannot fast travel now");
            return false;
        }

        // Get target location
        LocationData targetLocation = LocationManager.Instance.GetLocationByID(targetLocationID);
        if (targetLocation == null)
        {
            Debug.LogError($"[FastTravelSystem] Target location not found: {targetLocationID}");
            return false;
        }

        // Check if location is unlocked
        if (!LocationManager.Instance.IsLocationUnlocked(targetLocationID))
        {
            Debug.LogWarning($"[FastTravelSystem] Location is locked: {targetLocation.locationName}");
            EventSystem.Publish("FastTravelBlocked", "Location is locked");
            return false;
        }

        // Get current location
        LocationData currentLocation = LocationManager.Instance.GetCurrentLocation();
        if (currentLocation == null)
        {
            Debug.LogError("[FastTravelSystem] No current location set!");
            return false;
        }

        // Check if already at target
        if (currentLocation.locationID == targetLocationID)
        {
            Debug.Log($"[FastTravelSystem] Already at location: {targetLocation.locationName}");
            return false;
        }

        // Check if player has enough relics
        GameState gameState = GameManager.Instance.CurrentGameState;
        // Note: Relics are managed by progression system (Agent 9)
        // For now, we'll use a placeholder check
        // TODO: Integrate with ProgressionManager for actual relic count

        // Consume relics (placeholder - will be handled by Agent 9)
        // ProgressionManager.Instance.ConsumeRelics(relicCostPerTravel);

        // Perform instant travel
        PerformFastTravel(targetLocation);

        return true;
    }

    /// <summary>
    /// Performs the instant teleportation to target location.
    /// </summary>
    private void PerformFastTravel(LocationData destination)
    {
        // Publish fast travel started event
        EventSystem.Publish("FastTravelStarted", destination.locationID);

        Debug.Log($"[FastTravelSystem] Fast traveling to {destination.locationName}");

        // Play teleport effect
        if (playTeleportEffect)
        {
            StartCoroutine(PlayTeleportEffectCoroutine(destination));
        }
        else
        {
            // Instant travel
            CompleteFastTravel(destination);
        }
    }

    /// <summary>
    /// Plays visual effect and then completes travel.
    /// </summary>
    private System.Collections.IEnumerator PlayTeleportEffectCoroutine(LocationData destination)
    {
        // TODO: Trigger visual effect (particles, screen flash, etc)
        // This would integrate with Agent 13 (VFX)
        EventSystem.Publish("PlayTeleportEffect", destination.dockPosition);

        yield return new WaitForSeconds(teleportDuration);

        CompleteFastTravel(destination);
    }

    /// <summary>
    /// Completes the fast travel by loading the destination.
    /// </summary>
    private void CompleteFastTravel(LocationData destination)
    {
        // Load the destination location
        LocationManager.Instance.LoadLocation(destination.locationID);

        // Publish fast travel complete event
        EventSystem.Publish("FastTravelComplete", destination.locationID);

        Debug.Log($"[FastTravelSystem] Fast travel complete: {destination.locationName}");
    }

    #endregion

    #region Unlock & Upgrade Methods

    /// <summary>
    /// Unlocks fast travel ability (called by progression system).
    /// </summary>
    public void UnlockFastTravel()
    {
        if (isFastTravelUnlocked)
        {
            Debug.Log("[FastTravelSystem] Fast travel already unlocked.");
            return;
        }

        isFastTravelUnlocked = true;
        EventSystem.Publish("FastTravelUnlocked", true);
        Debug.Log("[FastTravelSystem] Fast travel unlocked! You can now teleport between docks using relics.");
    }

    /// <summary>
    /// Sets the relic cost per fast travel (for upgrades/balance).
    /// </summary>
    public void SetRelicCost(int cost)
    {
        relicCostPerTravel = Mathf.Max(0, cost);
        Debug.Log($"[FastTravelSystem] Relic cost set to {relicCostPerTravel}");
    }

    /// <summary>
    /// Sets whether dock requirement is enforced.
    /// </summary>
    public void SetDockRequirement(bool required)
    {
        requiresDockLocation = required;
        Debug.Log($"[FastTravelSystem] Dock requirement: {required}");
    }

    #endregion

    #region Query Methods

    /// <summary>
    /// Checks if fast travel is unlocked.
    /// </summary>
    public bool IsFastTravelUnlocked()
    {
        return isFastTravelUnlocked;
    }

    /// <summary>
    /// Gets the relic cost for fast travel.
    /// </summary>
    public int GetRelicCost()
    {
        return relicCostPerTravel;
    }

    /// <summary>
    /// Checks if player can fast travel to a location.
    /// </summary>
    public bool CanFastTravelTo(string locationID)
    {
        if (!isFastTravelUnlocked) return false;
        if (!LocationManager.Instance.IsLocationUnlocked(locationID)) return false;
        if (LocationManager.Instance.GetCurrentLocation()?.locationID == locationID) return false;

        // Check if player has enough relics (TODO: integrate with Agent 9)
        // return ProgressionManager.Instance.GetRelicCount() >= relicCostPerTravel;

        return true; // Placeholder
    }

    /// <summary>
    /// Checks if player is at dock.
    /// </summary>
    private bool IsPlayerAtDock()
    {
        // TODO: Check actual player position vs dock position
        // For now, we'll assume player is at dock if not fishing/traveling
        return !IsFishing() && !IsTraveling();
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

    /// <summary>
    /// Checks if player is currently traveling.
    /// </summary>
    private bool IsTraveling()
    {
        if (NavigationSystem.Instance != null)
        {
            return NavigationSystem.Instance.IsTraveling();
        }
        return false;
    }

    #endregion

    #region Save/Load Integration

    /// <summary>
    /// Saves fast travel unlock state.
    /// </summary>
    private void OnGatheringSave(SaveData data)
    {
        FastTravelSaveData fastTravelData = new FastTravelSaveData
        {
            isFastTravelUnlocked = this.isFastTravelUnlocked
        };

        data.fastTravelData = JsonUtility.ToJson(fastTravelData);
    }

    /// <summary>
    /// Loads fast travel unlock state.
    /// </summary>
    private void OnApplyingSave(SaveData data)
    {
        if (string.IsNullOrEmpty(data.fastTravelData))
        {
            return;
        }

        FastTravelSaveData fastTravelData = JsonUtility.FromJson<FastTravelSaveData>(data.fastTravelData);
        isFastTravelUnlocked = fastTravelData.isFastTravelUnlocked;

        Debug.Log($"[FastTravelSystem] Loaded: Fast travel unlocked = {isFastTravelUnlocked}");
    }

    #endregion

#if UNITY_EDITOR
    [ContextMenu("Unlock Fast Travel (Debug)")]
    private void DebugUnlockFastTravel()
    {
        UnlockFastTravel();
    }

    [ContextMenu("Test Fast Travel")]
    private void DebugTestFastTravel()
    {
        var unlockedLocations = LocationManager.Instance.GetUnlockedLocations();
        if (unlockedLocations.Count > 1)
        {
            LocationData current = LocationManager.Instance.GetCurrentLocation();
            LocationData target = unlockedLocations.Find(loc => loc.locationID != current.locationID);

            if (target != null)
            {
                FastTravel(target.locationID);
            }
        }
    }
#endif
}

/// <summary>
/// Save data for fast travel system.
/// </summary>
[System.Serializable]
public class FastTravelSaveData
{
    public bool isFastTravelUnlocked;
}

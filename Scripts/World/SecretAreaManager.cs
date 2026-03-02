using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Agent 14: Location & World Generation Agent - SecretAreaManager.cs
/// Manages hidden fishing spots within locations and their discovery mechanics.
/// </summary>
public class SecretAreaManager : MonoBehaviour
{
    public static SecretAreaManager Instance { get; private set; }

    [Header("Secret Areas")]
    [SerializeField] private List<SecretAreaData> allSecretAreas = new List<SecretAreaData>();

    [Header("Discovery Settings")]
    [SerializeField] private float discoveryRadius = 10f;
    [SerializeField] private bool requiresElditchEye = false; // Dark ability unlock
    [SerializeField] private bool showDiscoveryHints = true;

    [Header("Rewards")]
    [SerializeField] private float rareFishBonusMultiplier = 2f;
    [SerializeField] private float legendarySpawnChanceBonus = 0.05f; // +5% chance
    [SerializeField] private int relicRewardOnDiscovery = 1;

    private HashSet<string> discoveredSecretIDs = new HashSet<string>();
    private Dictionary<string, SecretAreaData> secretsByID = new Dictionary<string, SecretAreaData>();

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

        BuildSecretAreaLookup();
    }

    private void Start()
    {
        // Subscribe to save/load events
        EventSystem.Subscribe<SaveData>("GatheringSaveData", OnGatheringSave);
        EventSystem.Subscribe<SaveData>("ApplyingSaveData", OnApplyingSave);

        // Subscribe to player movement to check for discoveries
        EventSystem.Subscribe<PlayerMovedEventData>("PlayerMoved", OnPlayerMoved);

        Debug.Log($"[SecretAreaManager] Initialized with {allSecretAreas.Count} secret areas.");
    }

    private void OnDestroy()
    {
        EventSystem.Unsubscribe<SaveData>("GatheringSaveData", OnGatheringSave);
        EventSystem.Unsubscribe<SaveData>("ApplyingSaveData", OnApplyingSave);
        EventSystem.Unsubscribe<PlayerMovedEventData>("PlayerMoved", OnPlayerMoved);
    }

    /// <summary>
    /// Builds lookup dictionary for secret areas.
    /// </summary>
    private void BuildSecretAreaLookup()
    {
        secretsByID.Clear();

        foreach (var secret in allSecretAreas)
        {
            if (secret == null) continue;

            if (!secretsByID.ContainsKey(secret.secretID))
            {
                secretsByID[secret.secretID] = secret;
            }
            else
            {
                Debug.LogWarning($"[SecretAreaManager] Duplicate secret ID: {secret.secretID}");
            }
        }
    }

    #endregion

    #region Discovery System

    /// <summary>
    /// Called when player moves - checks for nearby secret areas.
    /// </summary>
    private void OnPlayerMoved(PlayerMovedEventData moveData)
    {
        CheckForNearbySecrets(moveData.position);
    }

    /// <summary>
    /// Checks for undiscovered secrets near the player.
    /// </summary>
    private void CheckForNearbySecrets(Vector3 playerPosition)
    {
        LocationData currentLocation = LocationManager.Instance.GetCurrentLocation();
        if (currentLocation == null) return;

        // Get secrets in current location
        List<SecretAreaData> locationSecrets = GetSecretsInLocation(currentLocation.locationID);

        foreach (var secret in locationSecrets)
        {
            // Skip if already discovered
            if (IsSecretDiscovered(secret.secretID))
                continue;

            // Check if player is within discovery radius
            float distance = Vector3.Distance(playerPosition, secret.position);

            if (distance <= discoveryRadius)
            {
                // Check if requires Eldritch Eye ability
                if (secret.requiresElditchEye && !HasElditchEye())
                {
                    if (showDiscoveryHints && distance <= discoveryRadius * 0.5f)
                    {
                        ShowElditchEyeHint(secret);
                    }
                    continue;
                }

                // Discover the secret!
                DiscoverSecret(secret);
            }
        }
    }

    /// <summary>
    /// Discovers a secret area.
    /// </summary>
    private void DiscoverSecret(SecretAreaData secret)
    {
        discoveredSecretIDs.Add(secret.secretID);

        Debug.Log($"[SecretAreaManager] Secret discovered: {secret.secretName}!");

        // Award relics
        if (relicRewardOnDiscovery > 0)
        {
            // TODO: Integrate with ProgressionManager (Agent 9)
            // ProgressionManager.Instance.AddRelics(relicRewardOnDiscovery);
        }

        // Publish discovery event
        EventSystem.Publish("SecretAreaDiscovered", secret);

        // Show UI notification
        if (NotificationManager.Instance != null)
        {
            NotificationManager.Instance.ShowNotification(
                $"Secret Area Discovered: {secret.secretName}",
                NotificationType.Discovery
            );
        }
    }

    /// <summary>
    /// Shows a hint that Eldritch Eye is needed.
    /// </summary>
    private void ShowElditchEyeHint(SecretAreaData secret)
    {
        if (NotificationManager.Instance != null)
        {
            NotificationManager.Instance.ShowNotification(
                "You sense something hidden nearby... (Requires Eldritch Eye ability)",
                NotificationType.Hint
            );
        }
    }

    /// <summary>
    /// Manually discovers a secret (for quest rewards, etc).
    /// </summary>
    public void DiscoverSecretByID(string secretID)
    {
        if (IsSecretDiscovered(secretID))
        {
            Debug.Log($"[SecretAreaManager] Secret already discovered: {secretID}");
            return;
        }

        SecretAreaData secret = GetSecretByID(secretID);
        if (secret != null)
        {
            DiscoverSecret(secret);
        }
    }

    #endregion

    #region Query Methods

    /// <summary>
    /// Gets a secret area by ID.
    /// </summary>
    public SecretAreaData GetSecretByID(string secretID)
    {
        if (secretsByID.TryGetValue(secretID, out SecretAreaData secret))
        {
            return secret;
        }

        Debug.LogWarning($"[SecretAreaManager] Secret ID not found: {secretID}");
        return null;
    }

    /// <summary>
    /// Checks if a secret has been discovered.
    /// </summary>
    public bool IsSecretDiscovered(string secretID)
    {
        return discoveredSecretIDs.Contains(secretID);
    }

    /// <summary>
    /// Gets all secrets in a specific location.
    /// </summary>
    public List<SecretAreaData> GetSecretsInLocation(string locationID)
    {
        return allSecretAreas.Where(s => s.locationID == locationID).ToList();
    }

    /// <summary>
    /// Gets all discovered secrets.
    /// </summary>
    public List<SecretAreaData> GetDiscoveredSecrets()
    {
        return allSecretAreas.Where(s => IsSecretDiscovered(s.secretID)).ToList();
    }

    /// <summary>
    /// Gets total secret count and discovered count.
    /// </summary>
    public (int total, int discovered) GetSecretStats()
    {
        return (allSecretAreas.Count, discoveredSecretIDs.Count);
    }

    /// <summary>
    /// Gets rare fish bonus multiplier for current position.
    /// </summary>
    public float GetRareFishBonusAtPosition(Vector3 position)
    {
        LocationData currentLocation = LocationManager.Instance.GetCurrentLocation();
        if (currentLocation == null) return 1f;

        // Check if player is in a discovered secret area
        List<SecretAreaData> locationSecrets = GetSecretsInLocation(currentLocation.locationID);

        foreach (var secret in locationSecrets)
        {
            if (!IsSecretDiscovered(secret.secretID))
                continue;

            float distance = Vector3.Distance(position, secret.position);
            if (distance <= secret.radius)
            {
                return rareFishBonusMultiplier;
            }
        }

        return 1f; // No bonus
    }

    /// <summary>
    /// Gets legendary spawn chance bonus for current position.
    /// </summary>
    public float GetLegendaryBonusAtPosition(Vector3 position)
    {
        LocationData currentLocation = LocationManager.Instance.GetCurrentLocation();
        if (currentLocation == null) return 0f;

        List<SecretAreaData> locationSecrets = GetSecretsInLocation(currentLocation.locationID);

        foreach (var secret in locationSecrets)
        {
            if (!IsSecretDiscovered(secret.secretID))
                continue;

            float distance = Vector3.Distance(position, secret.position);
            if (distance <= secret.radius)
            {
                return legendarySpawnChanceBonus;
            }
        }

        return 0f;
    }

    /// <summary>
    /// Checks if player has Eldritch Eye ability unlocked.
    /// </summary>
    private bool HasElditchEye()
    {
        // TODO: Integrate with ProgressionManager (Agent 9)
        // return ProgressionManager.Instance.HasAbility("eldritch_eye");
        return !requiresElditchEye; // Placeholder
    }

    #endregion

    #region Save/Load Integration

    /// <summary>
    /// Saves discovered secrets.
    /// </summary>
    private void OnGatheringSave(SaveData data)
    {
        SecretAreaSaveData secretData = new SecretAreaSaveData
        {
            discoveredSecretIDs = new List<string>(discoveredSecretIDs)
        };

        data.secretAreaData = JsonUtility.ToJson(secretData);
    }

    /// <summary>
    /// Loads discovered secrets.
    /// </summary>
    private void OnApplyingSave(SaveData data)
    {
        if (string.IsNullOrEmpty(data.secretAreaData))
        {
            return;
        }

        SecretAreaSaveData secretData = JsonUtility.FromJson<SecretAreaSaveData>(data.secretAreaData);

        discoveredSecretIDs.Clear();
        foreach (string secretID in secretData.discoveredSecretIDs)
        {
            discoveredSecretIDs.Add(secretID);
        }

        Debug.Log($"[SecretAreaManager] Loaded {discoveredSecretIDs.Count} discovered secrets.");
    }

    #endregion

#if UNITY_EDITOR
    [ContextMenu("Print Secret Stats")]
    private void PrintSecretStats()
    {
        var stats = GetSecretStats();
        Debug.Log($"=== Secret Area Statistics ===\nTotal: {stats.total}\nDiscovered: {stats.discovered}\nRemaining: {stats.total - stats.discovered}");
    }

    [ContextMenu("Discover All Secrets (Debug)")]
    private void DebugDiscoverAllSecrets()
    {
        foreach (var secret in allSecretAreas)
        {
            DiscoverSecretByID(secret.secretID);
        }
    }

    [ContextMenu("Reset All Secrets")]
    private void DebugResetSecrets()
    {
        discoveredSecretIDs.Clear();
        Debug.Log("[SecretAreaManager] All secrets reset.");
    }
#endif
}

/// <summary>
/// Data structure for a secret fishing area.
/// </summary>
[System.Serializable]
public class SecretAreaData
{
    public string secretID;
    public string secretName;
    public string locationID; // Which location this secret is in
    public Vector3 position;
    public float radius = 15f;
    public string description;
    public bool requiresElditchEye = false;
    public string unlockHint; // Hint for discovery
}

/// <summary>
/// Save data for secret areas.
/// </summary>
[System.Serializable]
public class SecretAreaSaveData
{
    public List<string> discoveredSecretIDs;
}

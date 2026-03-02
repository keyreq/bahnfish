using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Drift net that passively catches fish while sailing.
/// Automatically collects fish as you travel between locations.
/// Larger nets catch more but create drag on boat speed.
/// </summary>
public class DriftNet : BaseFishingTool
{
    [Header("Net Stats")]
    [SerializeField] private int maxCapacity = 8;
    [SerializeField] private float catchRadius = 5f; // Meters around boat
    [SerializeField] private float catchInterval = 15f; // Time between catch attempts
    [SerializeField] private float speedPenalty = 0.1f; // Reduces boat speed

    [Header("Deployment")]
    [SerializeField] private bool isDeployed = false;
    [SerializeField] private float totalDistanceTraveled = 0f;
    [SerializeField] private Vector3 lastPosition;

    [Header("Catches")]
    private List<Fish> caughtFish = new List<Fish>();
    private float timeSinceLastCatch = 0f;

    [Header("Upgrade Stats")]
    [SerializeField] private int upgradeLevel = 1;
    [SerializeField] private bool hasAutoDeploy = false;

    private BoatController boatController;

    private void Awake()
    {
        toolName = "Drift Net";
        durability = 100f;
        power = 0.7f;

        UpdateStats();
    }

    private void Start()
    {
        // Find boat controller
        boatController = FindObjectOfType<BoatController>();

        // Subscribe to movement events
        EventSystem.Subscribe<PlayerMovedEventData>("OnPlayerMoved", OnPlayerMoved);
    }

    private void OnDestroy()
    {
        EventSystem.Unsubscribe<PlayerMovedEventData>("OnPlayerMoved", OnPlayerMoved);
    }

    private void Update()
    {
        if (isDeployed && Application.isPlaying)
        {
            UpdatePassiveFishing();
        }

        // Auto-deploy if enabled and moving
        if (hasAutoDeploy && !isDeployed && IsBoatMoving())
        {
            Deploy();
        }
    }

    public override bool CanCatchFish(Fish fish)
    {
        // Drift nets catch surface and mid-depth fish
        // Not effective for deep-sea or bottom-dwellers

        // Can't catch legendary fish with net
        if (fish.rarity == FishRarity.Legendary)
            return false;

        // Large fish break through
        if (fish.weight > 20f)
            return false;

        return true;
    }

    #region Deployment

    public void Deploy()
    {
        if (isDeployed)
        {
            Debug.LogWarning("Net already deployed!");
            return;
        }

        if (durability <= 10f)
        {
            Debug.LogWarning("Net too damaged to deploy!");
            return;
        }

        isDeployed = true;
        lastPosition = boatController != null ? boatController.GetPosition() : Vector3.zero;
        totalDistanceTraveled = 0f;
        timeSinceLastCatch = 0f;

        // Apply speed penalty to boat
        if (boatController != null)
        {
            // This would integrate with BoatController to reduce max speed
            Debug.Log($"Drift net deployed - boat speed reduced by {speedPenalty * 100f}%");
        }

        EventSystem.Publish("OnDriftNetDeployed", this);
    }

    public void Retrieve()
    {
        if (!isDeployed)
        {
            Debug.LogWarning("Net not deployed!");
            return;
        }

        isDeployed = false;

        Debug.Log($"Drift net retrieved after {totalDistanceTraveled:F1}m. Caught {caughtFish.Count} fish.");
        EventSystem.Publish("OnDriftNetRetrieved", this);

        // Decrease durability based on distance traveled
        float durabilityLoss = totalDistanceTraveled * 0.01f;
        durability -= durabilityLoss;

        if (durability <= 0f)
        {
            OnToolBroken();
        }

        totalDistanceTraveled = 0f;
    }

    #endregion

    #region Passive Fishing

    private void OnPlayerMoved(PlayerMovedEventData data)
    {
        if (!isDeployed)
            return;

        // Track distance
        float distance = Vector3.Distance(lastPosition, data.position);
        totalDistanceTraveled += distance;
        lastPosition = data.position;

        // Only catch fish if actually moving
        if (data.speed < 1f)
            return;

        // Faster movement = more chance to catch
        float speedBonus = Mathf.Clamp01(data.speed / 10f);
        timeSinceLastCatch += Time.deltaTime * (1f + speedBonus);
    }

    private void UpdatePassiveFishing()
    {
        if (caughtFish.Count >= maxCapacity)
        {
            // Net is full - need to retrieve
            if (!hasAutoDeploy) // Only stop if not auto-deploying
            {
                Debug.LogWarning("Drift net is full! Retrieve to collect fish.");
            }
            return;
        }

        if (timeSinceLastCatch >= catchInterval)
        {
            CheckForCatch();
            timeSinceLastCatch = 0f;
        }
    }

    private void CheckForCatch()
    {
        if (caughtFish.Count >= maxCapacity)
            return;

        // Higher chance while moving
        bool isMoving = IsBoatMoving();
        float catchChance = isMoving ? 0.6f : 0.2f;
        catchChance += upgradeLevel * 0.1f;

        if (Random.value < catchChance)
        {
            Fish caught = GenerateRandomCatch();

            if (caught != null && CanCatchFish(caught))
            {
                caughtFish.Add(caught);
                Debug.Log($"Drift net caught: {caught.name}");
                EventSystem.Publish("OnDriftNetCatch", caught);

                // Net durability decreases with each catch
                durability -= 0.2f;

                if (durability <= 0f)
                {
                    OnToolBroken();
                }
            }
        }
    }

    private Fish GenerateRandomCatch()
    {
        // MOCK DATA - Will integrate with Agent 8
        string[] netFish = { "Mackerel", "Herring", "Sardine", "Tuna", "Salmon", "Trout" };
        string fishName = netFish[Random.Range(0, netFish.Length)];

        Fish caught = new Fish
        {
            id = $"netfish_{Random.Range(1000, 9999)}",
            name = fishName,
            rarity = Random.value < 0.7f ? FishRarity.Common : FishRarity.Uncommon,
            baseValue = Random.Range(8f, 20f),
            inventorySize = new Vector2Int(2, 1),
            weight = Random.Range(1f, 5f),
            length = Random.Range(20f, 50f)
        };

        // Rare chance of rare fish
        if (Random.value < 0.05f)
        {
            caught.rarity = FishRarity.Rare;
            caught.baseValue *= 2f;
        }

        return caught;
    }

    private bool IsBoatMoving()
    {
        if (boatController == null)
            return false;

        return boatController.GetSpeed() > 1f;
    }

    #endregion

    #region Collection

    public List<Fish> CollectCatch()
    {
        List<Fish> collected = new List<Fish>(caughtFish);
        caughtFish.Clear();

        Debug.Log($"Collected {collected.Count} fish from drift net");
        return collected;
    }

    public int GetCatchCount()
    {
        return caughtFish.Count;
    }

    public bool IsFull()
    {
        return caughtFish.Count >= maxCapacity;
    }

    #endregion

    #region Upgrades

    public void UpgradeCapacity(int increase)
    {
        maxCapacity += increase;
        Debug.Log($"Drift net capacity increased to {maxCapacity}");
    }

    public void UpgradeCatchRate(float decrease)
    {
        catchInterval = Mathf.Max(5f, catchInterval - decrease);
        Debug.Log($"Drift net catch rate improved to {catchInterval}s");
    }

    public void ReduceSpeedPenalty(float reduction)
    {
        speedPenalty = Mathf.Max(0.05f, speedPenalty - reduction);
        Debug.Log($"Drift net speed penalty reduced to {speedPenalty * 100f}%");
    }

    public void EnableAutoDeploy()
    {
        hasAutoDeploy = true;
        Debug.Log("Auto-deploy enabled - net will deploy automatically when moving!");
    }

    private void UpdateStats()
    {
        maxCapacity = 8 + (upgradeLevel * 3);
        catchInterval = 15f - (upgradeLevel * 2f);
        power = 0.7f + (upgradeLevel * 0.15f);
        catchRadius = 5f + (upgradeLevel * 1f);
    }

    #endregion

    #region Tool Maintenance

    public void Repair(float amount = 100f)
    {
        durability = Mathf.Min(durability + amount, 100f);
        Debug.Log($"Drift net repaired to {durability:F1} durability");
    }

    private void OnToolBroken()
    {
        Debug.LogWarning("Drift net tore!");
        EventSystem.Publish("OnToolBroken", this);

        // Auto-retrieve when broken
        if (isDeployed)
        {
            Retrieve();
        }

        // Lose half the catch
        if (caughtFish.Count > 0)
        {
            int lost = Mathf.CeilToInt(caughtFish.Count * 0.5f);
            caughtFish.RemoveRange(0, lost);
            Debug.Log($"Net tore! Lost {lost} fish!");
        }

        durability = 1f;
    }

    #endregion

    #region Public API

    public bool IsDeployed() => isDeployed;
    public float GetTotalDistance() => totalDistanceTraveled;
    public float GetSpeedPenalty() => speedPenalty;
    public int GetUpgradeLevel() => upgradeLevel;
    public bool HasAutoDeploy() => hasAutoDeploy;

    #endregion

    #region Visual Feedback

    private void OnDrawGizmos()
    {
        if (isDeployed && boatController != null)
        {
            Vector3 boatPos = boatController.GetPosition();

            // Draw net area
            Gizmos.color = IsFull() ? Color.red : Color.cyan;
            Gizmos.DrawWireSphere(boatPos, catchRadius);

            // Draw net trail behind boat
            Gizmos.color = new Color(0f, 1f, 1f, 0.3f);
            Vector3 netPos = boatPos - boatController.GetForward() * 5f;
            Gizmos.DrawLine(boatPos, netPos);
            Gizmos.DrawWireSphere(netPos, 1f);
        }
    }

    #endregion
}

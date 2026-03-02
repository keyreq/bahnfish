using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Passive fishing tool that catches shellfish over time.
/// Place in water, return later to collect catches.
/// Multiple pots can be deployed simultaneously.
/// </summary>
public class CrabPot : BaseFishingTool
{
    [Header("Crab Pot Stats")]
    [SerializeField] private int maxCapacity = 5; // Max fish per pot
    [SerializeField] private float catchInterval = 120f; // Time between catches (seconds)
    [SerializeField] private int upgradeLevel = 1;

    [Header("Deployment")]
    [SerializeField] private bool isDeployed = false;
    [SerializeField] private float deployTime = 0f;
    [SerializeField] private Vector3 deployPosition;
    [SerializeField] private string deployedLocationID;

    [Header("Catches")]
    private List<Fish> caughtFish = new List<Fish>();
    private float timeSinceLastCatch = 0f;

    [Header("Settings")]
    [SerializeField] private bool canCatchAberrant = false;
    [SerializeField] private float aberrantChance = 0.05f;

    private void Awake()
    {
        toolName = "Crab Pot";
        durability = 100f;
        power = 0.5f; // Passive tool, lower power

        UpdateStats();
    }

    private void Update()
    {
        if (isDeployed && Application.isPlaying)
        {
            UpdatePassiveFishing();
        }
    }

    public override bool CanCatchFish(Fish fish)
    {
        // Crab pots only catch shellfish and bottom-feeders
        // This will be defined by fish type (Agent 8 integration)

        // For now, only catch common and uncommon fish
        if (fish.rarity == FishRarity.Rare || fish.rarity == FishRarity.Legendary)
            return false;

        // Can catch aberrant only if upgraded
        if (fish.isAberrant && !canCatchAberrant)
            return false;

        return true;
    }

    #region Deployment

    public bool Deploy(Vector3 position, string locationID)
    {
        if (isDeployed)
        {
            Debug.LogWarning("Crab pot already deployed!");
            return false;
        }

        if (durability <= 10f)
        {
            Debug.LogWarning("Crab pot too damaged to deploy!");
            return false;
        }

        isDeployed = true;
        deployPosition = position;
        deployedLocationID = locationID;
        deployTime = Time.time;
        timeSinceLastCatch = 0f;

        Debug.Log($"Crab pot deployed at {locationID}");
        EventSystem.Publish("OnCrabPotDeployed", this);

        return true;
    }

    public bool Retrieve()
    {
        if (!isDeployed)
        {
            Debug.LogWarning("Crab pot not deployed!");
            return false;
        }

        isDeployed = false;

        // Final check for any catches before retrieval
        CheckForCatch();

        Debug.Log($"Crab pot retrieved with {caughtFish.Count} fish");
        EventSystem.Publish("OnCrabPotRetrieved", this);

        // Decrease durability based on deployment time
        float hoursDeployed = (Time.time - deployTime) / 3600f;
        durability -= hoursDeployed * 2f;

        if (durability <= 0f)
        {
            OnToolBroken();
        }

        return true;
    }

    #endregion

    #region Passive Fishing

    private void UpdatePassiveFishing()
    {
        if (caughtFish.Count >= maxCapacity)
        {
            // Pot is full
            return;
        }

        timeSinceLastCatch += Time.deltaTime;

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

        // Chance to catch something
        float catchChance = 0.7f + (upgradeLevel * 0.1f);

        if (Random.value < catchChance)
        {
            // Generate a random catch
            Fish caught = GenerateRandomCatch();

            if (caught != null && CanCatchFish(caught))
            {
                caughtFish.Add(caught);
                Debug.Log($"Crab pot caught: {caught.name}");
                EventSystem.Publish("OnCrabPotCatch", caught);
            }
        }
    }

    private Fish GenerateRandomCatch()
    {
        // MOCK DATA - Will integrate with Agent 8 (Fish AI)
        // For now, generate simple mock fish

        string[] crabPotFish = { "Crab", "Lobster", "Clam", "Oyster", "Mussel", "Shrimp" };
        string fishName = crabPotFish[Random.Range(0, crabPotFish.Length)];

        Fish caught = new Fish
        {
            id = $"shellfish_{Random.Range(1000, 9999)}",
            name = fishName,
            rarity = Random.value < 0.8f ? FishRarity.Common : FishRarity.Uncommon,
            baseValue = Random.Range(5f, 15f),
            inventorySize = new Vector2Int(1, 1),
            weight = Random.Range(0.2f, 1.5f),
            length = Random.Range(5f, 15f)
        };

        // Small chance of aberrant catch if upgraded
        if (canCatchAberrant && Random.value < aberrantChance)
        {
            caught.isAberrant = true;
            caught.rarity = FishRarity.Aberrant;
            caught.baseValue *= 3f;
            caught.name = $"Aberrant {fishName}";
        }

        return caught;
    }

    #endregion

    #region Collection

    public List<Fish> CollectCatch()
    {
        List<Fish> collected = new List<Fish>(caughtFish);
        caughtFish.Clear();

        Debug.Log($"Collected {collected.Count} fish from crab pot");
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
        Debug.Log($"Crab pot capacity increased to {maxCapacity}");
    }

    public void UpgradeCatchRate(float decrease)
    {
        catchInterval = Mathf.Max(60f, catchInterval - decrease);
        Debug.Log($"Crab pot catch rate improved to {catchInterval}s");
    }

    public void UnlockAberrantCatching()
    {
        canCatchAberrant = true;
        Debug.Log("Crab pot can now catch aberrant fish!");
    }

    private void UpdateStats()
    {
        maxCapacity = 5 + (upgradeLevel * 2);
        catchInterval = 120f - (upgradeLevel * 10f);
        power = 0.5f + (upgradeLevel * 0.1f);
    }

    #endregion

    #region Tool Maintenance

    public void Repair(float amount = 100f)
    {
        durability = Mathf.Min(durability + amount, 100f);
        Debug.Log($"Crab pot repaired to {durability:F1} durability");
    }

    private void OnToolBroken()
    {
        Debug.LogWarning("Crab pot broke!");
        EventSystem.Publish("OnToolBroken", this);

        // Lost some catches when it broke
        if (caughtFish.Count > 0)
        {
            int lost = Mathf.CeilToInt(caughtFish.Count * 0.5f);
            caughtFish.RemoveRange(0, lost);
            Debug.Log($"Lost {lost} fish when pot broke!");
        }

        durability = 1f;
    }

    #endregion

    #region Public API

    public bool IsDeployed() => isDeployed;
    public Vector3 GetDeployPosition() => deployPosition;
    public string GetDeployLocation() => deployedLocationID;
    public float GetTimeDeployed() => isDeployed ? Time.time - deployTime : 0f;
    public int GetUpgradeLevel() => upgradeLevel;

    #endregion

    #region Visual Feedback

    private void OnDrawGizmos()
    {
        if (isDeployed)
        {
            Gizmos.color = IsFull() ? Color.green : Color.yellow;
            Gizmos.DrawWireSphere(deployPosition, 1f);

            // Draw line to surface
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(deployPosition, deployPosition + Vector3.up * 3f);
        }
    }

    #endregion
}

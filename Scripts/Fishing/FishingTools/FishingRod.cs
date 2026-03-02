using UnityEngine;

/// <summary>
/// Standard fishing rod tool for catching most fish.
/// Uses the reel minigame for active tension management.
/// Can be upgraded for better performance.
/// </summary>
public class FishingRod : BaseFishingTool
{
    [Header("Rod Stats")]
    [SerializeField] private float lineStrength = 50f; // Affects how much tension before break
    [SerializeField] private float reelSpeed = 1f; // Affects catch progress rate
    [SerializeField] private int upgradeLevel = 1;

    [Header("Upgrade Costs")]
    [SerializeField] private float[] upgradeCosts = { 0f, 100f, 250f, 500f, 1000f, 2000f };
    [SerializeField] private int maxUpgradeLevel = 5;

    private void Awake()
    {
        toolName = "Fishing Rod";
        durability = 100f;
        UpdateStats();
    }

    public override bool CanCatchFish(Fish fish)
    {
        // Basic rod can catch most fish except legendary and large aberrant
        if (fish.rarity == FishRarity.Legendary)
        {
            // Need upgraded rod
            return upgradeLevel >= 3;
        }

        if (fish.isAberrant && fish.weight > 15f)
        {
            // Large aberrant fish need strong rod
            return upgradeLevel >= 4;
        }

        // Can catch everything else
        return true;
    }

    #region Upgrades

    public bool CanUpgrade()
    {
        return upgradeLevel < maxUpgradeLevel;
    }

    public float GetUpgradeCost()
    {
        if (!CanUpgrade())
            return 0f;

        return upgradeCosts[upgradeLevel];
    }

    public bool TryUpgrade(float playerMoney)
    {
        if (!CanUpgrade())
        {
            Debug.Log("Rod is already max level!");
            return false;
        }

        float cost = GetUpgradeCost();
        if (playerMoney < cost)
        {
            Debug.Log($"Not enough money! Need ${cost}");
            return false;
        }

        // Perform upgrade
        upgradeLevel++;
        UpdateStats();

        Debug.Log($"Fishing Rod upgraded to level {upgradeLevel}!");
        return true;
    }

    private void UpdateStats()
    {
        // Scale stats based on upgrade level
        lineStrength = 50f + (upgradeLevel * 20f);
        reelSpeed = 1f + (upgradeLevel * 0.2f);
        power = 1f + (upgradeLevel * 0.3f);
        durability = 100f + (upgradeLevel * 20f);

        toolName = upgradeLevel > 1 ? $"Fishing Rod +{upgradeLevel - 1}" : "Fishing Rod";
    }

    #endregion

    #region Tool Usage

    public void UseTool()
    {
        // Decrease durability slightly on each use
        durability -= 0.5f;

        if (durability <= 0f)
        {
            OnToolBroken();
        }
    }

    public void Repair(float amount = 100f)
    {
        durability = Mathf.Min(durability + amount, 100f + (upgradeLevel * 20f));
        Debug.Log($"Rod repaired to {durability:F1} durability");
    }

    private void OnToolBroken()
    {
        Debug.LogWarning("Fishing rod broke! Need to repair.");
        EventSystem.Publish("OnToolBroken", this);

        // Set to minimal durability instead of 0 (can still use but very weak)
        durability = 1f;
    }

    #endregion

    #region Public API

    public float GetLineStrength() => lineStrength;
    public float GetReelSpeed() => reelSpeed;
    public int GetUpgradeLevel() => upgradeLevel;
    public float GetDurability() => durability;
    public float GetDurabilityPercentage() => (durability / (100f + upgradeLevel * 20f)) * 100f;

    #endregion

    #region Visual Feedback

    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            // Draw rod stats in scene view
            Gizmos.color = Color.cyan;
            // Could visualize line strength, etc.
        }
    }

    #endregion
}

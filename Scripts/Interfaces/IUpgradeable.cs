/// <summary>
/// Interface for objects that can be upgraded through the progression system.
/// Implement this on ship components, equipment, and tools that support upgrades.
/// </summary>
public interface IUpgradeable
{
    /// <summary>
    /// Unique identifier for this upgradeable item.
    /// </summary>
    string UpgradeID { get; }

    /// <summary>
    /// Display name of the upgradeable item.
    /// </summary>
    string UpgradeName { get; }

    /// <summary>
    /// Current upgrade level (0 = base, 1+ = upgraded tiers).
    /// </summary>
    int CurrentLevel { get; }

    /// <summary>
    /// Maximum level this item can be upgraded to.
    /// </summary>
    int MaxLevel { get; }

    /// <summary>
    /// Checks if the item can currently be upgraded.
    /// </summary>
    /// <returns>True if upgrade is possible, false otherwise</returns>
    bool CanUpgrade();

    /// <summary>
    /// Attempts to upgrade the item to the next level.
    /// </summary>
    /// <returns>True if upgrade succeeded, false if failed</returns>
    bool Upgrade();

    /// <summary>
    /// Gets the cost to upgrade to the next level.
    /// </summary>
    /// <returns>Cost in the game's currency, or -1 if at max level</returns>
    float GetUpgradeCost();

    /// <summary>
    /// Gets the requirements for the next upgrade.
    /// </summary>
    /// <returns>Description of requirements (e.g., "Level 5, 500 coins")</returns>
    string GetUpgradeRequirements();

    /// <summary>
    /// Gets a description of what the next upgrade provides.
    /// </summary>
    /// <returns>Description of upgrade benefits</returns>
    string GetUpgradeDescription();
}

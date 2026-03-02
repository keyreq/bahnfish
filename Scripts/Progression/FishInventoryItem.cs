using UnityEngine;

/// <summary>
/// Agent 9: Progression & Economy Agent - FishInventoryItem.cs
/// Wrapper class for Fish to work with Agent 6's inventory system.
/// Allows ShopManager to sell fish from inventory.
/// </summary>
public class FishInventoryItem : InventoryItem
{
    private Fish _fishData;
    private bool _isFresh = true;
    private bool _caughtAtNight = false;

    public Fish FishData => _fishData;
    public bool IsFresh => _isFresh;
    public bool CaughtAtNight => _caughtAtNight;

    /// <summary>
    /// Creates a new fish inventory item.
    /// </summary>
    public FishInventoryItem(Fish fish, bool isFresh = true, bool caughtAtNight = false)
    {
        _fishData = fish;
        _isFresh = isFresh;
        _caughtAtNight = caughtAtNight;
    }

    #region IInventoryItem Implementation

    public override string ItemID => _fishData.id;
    public override string ItemName => _fishData.name;
    public override int Width => _fishData.inventorySize.x;
    public override int Height => _fishData.inventorySize.y;
    public override Sprite Icon => _fishData.icon;

    public override string GetDescription()
    {
        float sellValue = PricingSystem.Instance.GetFishSellValue(_fishData, _isFresh, _caughtAtNight);

        string description = $"{_fishData.description}\n\n";
        description += $"Rarity: {_fishData.rarity}\n";
        description += $"Weight: {_fishData.weight:F1} lbs\n";
        description += $"Sell Value: ${sellValue:F2}\n";

        if (_caughtAtNight)
        {
            description += "\n[Night Catch - Premium Price!]";
        }

        if (!_isFresh)
        {
            description += "\n[SPOILED - Reduced Value]";
        }

        if (_fishData.isAberrant)
        {
            description += "\n[ABERRANT - Bonus Value]";
        }

        return description;
    }

    public override float GetValue()
    {
        return PricingSystem.Instance.GetFishSellValue(_fishData, _isFresh, _caughtAtNight);
    }

    public override bool CanRotate()
    {
        // Fish can be rotated if they're not square
        return Width != Height;
    }

    public override bool CanStackWith(InventoryItem other)
    {
        // Fish don't stack (each is unique)
        return false;
    }

    #endregion

    /// <summary>
    /// Sets the freshness state of the fish.
    /// </summary>
    public void SetFreshness(bool isFresh)
    {
        _isFresh = isFresh;
    }

    /// <summary>
    /// Marks fish as spoiled (reduces value).
    /// </summary>
    public void Spoil()
    {
        _isFresh = false;
    }

    /// <summary>
    /// Gets the base value before modifiers.
    /// </summary>
    public float GetBaseValue()
    {
        return _fishData.baseValue;
    }

    /// <summary>
    /// Gets the night premium multiplier if caught at night.
    /// </summary>
    public float GetNightMultiplier()
    {
        return _caughtAtNight ? Random.Range(3f, 5f) : 1f;
    }
}

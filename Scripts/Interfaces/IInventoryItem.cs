using UnityEngine;

/// <summary>
/// Interface for items that can be stored in the inventory system.
/// Implement this on any object that should be placeable in the grid-based inventory.
/// </summary>
public interface IInventoryItem
{
    /// <summary>
    /// Unique identifier for this item.
    /// </summary>
    string ItemID { get; }

    /// <summary>
    /// Display name of the item.
    /// </summary>
    string ItemName { get; }

    /// <summary>
    /// Width of the item in inventory grid cells.
    /// </summary>
    int Width { get; }

    /// <summary>
    /// Height of the item in inventory grid cells.
    /// </summary>
    int Height { get; }

    /// <summary>
    /// Icon sprite for inventory display.
    /// </summary>
    Sprite Icon { get; }

    /// <summary>
    /// Gets the item's description for tooltip display.
    /// </summary>
    /// <returns>Description text</returns>
    string GetDescription();

    /// <summary>
    /// Gets the sell value of this item.
    /// </summary>
    /// <returns>The monetary value when sold</returns>
    float GetValue();

    /// <summary>
    /// Checks if the item can be rotated in inventory.
    /// </summary>
    /// <returns>True if rotatable, false otherwise</returns>
    bool CanRotate();

    /// <summary>
    /// Checks if the item can stack with another.
    /// </summary>
    /// <param name="other">The other item to check stacking with</param>
    /// <returns>True if stackable, false otherwise</returns>
    bool CanStackWith(IInventoryItem other);
}

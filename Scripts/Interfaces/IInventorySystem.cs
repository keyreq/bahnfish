using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Interface for the inventory management system.
/// Defines the contract that the InventoryManager must implement.
/// </summary>
public interface IInventorySystem
{
    /// <summary>
    /// Attempts to add an item to the inventory.
    /// </summary>
    /// <param name="item">The item to add</param>
    /// <returns>True if item was successfully added, false if inventory is full</returns>
    bool AddItem(InventoryItem item);

    /// <summary>
    /// Attempts to add an item to a specific grid position.
    /// </summary>
    /// <param name="item">The item to add</param>
    /// <param name="gridX">X position in grid</param>
    /// <param name="gridY">Y position in grid</param>
    /// <returns>True if item was successfully placed at that position</returns>
    bool AddItemAt(InventoryItem item, int gridX, int gridY);

    /// <summary>
    /// Removes an item from the inventory.
    /// </summary>
    /// <param name="item">The item to remove</param>
    /// <returns>True if item was found and removed</returns>
    bool RemoveItem(InventoryItem item);

    /// <summary>
    /// Gets the total number of available grid cells.
    /// </summary>
    /// <returns>Number of empty cells</returns>
    int GetAvailableSpace();

    /// <summary>
    /// Gets all items currently in the inventory.
    /// </summary>
    /// <returns>List of inventory items</returns>
    List<InventoryItem> GetAllItems();

    /// <summary>
    /// Checks if an item can fit in the inventory.
    /// </summary>
    /// <param name="item">The item to check</param>
    /// <returns>True if item can fit somewhere in the grid</returns>
    bool CanFitItem(InventoryItem item);

    /// <summary>
    /// Clears all items from the inventory.
    /// </summary>
    void ClearInventory();

    /// <summary>
    /// Gets the total capacity of the inventory grid.
    /// </summary>
    /// <returns>Total number of grid cells</returns>
    int GetTotalCapacity();

    /// <summary>
    /// Gets the number of cells currently occupied.
    /// </summary>
    /// <returns>Number of occupied cells</returns>
    int GetOccupiedSpace();
}

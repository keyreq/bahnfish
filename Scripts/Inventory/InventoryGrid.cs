using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages the 2D grid system for Tetris-style inventory.
/// Handles cell occupation tracking, item placement, and collision detection.
/// </summary>
[System.Serializable]
public class InventoryGrid
{
    [SerializeField] private int _width = 10;
    [SerializeField] private int _height = 10;
    [SerializeField] private InventoryItem[,] _grid;

    /// <summary>
    /// Width of the grid.
    /// </summary>
    public int Width => _width;

    /// <summary>
    /// Height of the grid.
    /// </summary>
    public int Height => _height;

    /// <summary>
    /// Total capacity in cells.
    /// </summary>
    public int TotalCapacity => _width * _height;

    /// <summary>
    /// Creates a new inventory grid with specified dimensions.
    /// </summary>
    public InventoryGrid(int width, int height)
    {
        _width = width;
        _height = height;
        _grid = new InventoryItem[width, height];
    }

    /// <summary>
    /// Checks if an item can be placed at the specified position.
    /// </summary>
    public bool CanPlaceItem(InventoryItem item, int gridX, int gridY)
    {
        if (item == null || item.Shape == null)
            return false;

        // Check bounds
        if (gridX < 0 || gridY < 0)
            return false;

        if (gridX + item.GetWidth() > _width || gridY + item.GetHeight() > _height)
            return false;

        // Check each cell the item would occupy
        List<Vector2Int> occupiedCells = item.Shape.GetOccupiedCells();
        foreach (Vector2Int cell in occupiedCells)
        {
            int checkX = gridX + cell.x;
            int checkY = gridY + cell.y;

            // Out of bounds
            if (checkX < 0 || checkX >= _width || checkY < 0 || checkY >= _height)
                return false;

            // Cell already occupied by another item
            if (_grid[checkX, checkY] != null && _grid[checkX, checkY] != item)
                return false;
        }

        return true;
    }

    /// <summary>
    /// Places an item at the specified position.
    /// </summary>
    /// <returns>True if successfully placed</returns>
    public bool PlaceItem(InventoryItem item, int gridX, int gridY)
    {
        if (!CanPlaceItem(item, gridX, gridY))
            return false;

        // Remove item from its current position first
        RemoveItem(item);

        // Set the item's position
        item.GridX = gridX;
        item.GridY = gridY;

        // Mark all cells as occupied by this item
        List<Vector2Int> occupiedCells = item.Shape.GetOccupiedCells();
        foreach (Vector2Int cell in occupiedCells)
        {
            int x = gridX + cell.x;
            int y = gridY + cell.y;
            _grid[x, y] = item;
        }

        return true;
    }

    /// <summary>
    /// Removes an item from the grid.
    /// </summary>
    public bool RemoveItem(InventoryItem item)
    {
        if (item == null)
            return false;

        bool removed = false;

        // Clear all cells occupied by this item
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                if (_grid[x, y] == item)
                {
                    _grid[x, y] = null;
                    removed = true;
                }
            }
        }

        return removed;
    }

    /// <summary>
    /// Finds the first available position for an item.
    /// Uses left-to-right, top-to-bottom search.
    /// </summary>
    public bool FindEmptyPosition(InventoryItem item, out int gridX, out int gridY)
    {
        gridX = -1;
        gridY = -1;

        if (item == null)
            return false;

        // Try all possible positions
        for (int y = 0; y < _height; y++)
        {
            for (int x = 0; x < _width; x++)
            {
                if (CanPlaceItem(item, x, y))
                {
                    gridX = x;
                    gridY = y;
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Finds the first available position, trying all rotations.
    /// </summary>
    public bool FindEmptyPositionWithRotation(InventoryItem item, out int gridX, out int gridY, out int rotation)
    {
        gridX = -1;
        gridY = -1;
        rotation = 0;

        if (item == null)
            return false;

        int originalRotation = item.GetRotation();

        // Try all 4 rotations
        for (int r = 0; r < 4; r++)
        {
            item.SetRotation(r * 90);

            if (FindEmptyPosition(item, out gridX, out gridY))
            {
                rotation = r * 90;
                return true;
            }
        }

        // Restore original rotation if no position found
        item.SetRotation(originalRotation);
        return false;
    }

    /// <summary>
    /// Gets the item at a specific grid position.
    /// </summary>
    public InventoryItem GetItemAt(int gridX, int gridY)
    {
        if (gridX < 0 || gridX >= _width || gridY < 0 || gridY >= _height)
            return null;

        return _grid[gridX, gridY];
    }

    /// <summary>
    /// Gets all unique items in the grid.
    /// </summary>
    public List<InventoryItem> GetAllItems()
    {
        HashSet<InventoryItem> uniqueItems = new HashSet<InventoryItem>();

        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                if (_grid[x, y] != null)
                {
                    uniqueItems.Add(_grid[x, y]);
                }
            }
        }

        return new List<InventoryItem>(uniqueItems);
    }

    /// <summary>
    /// Gets the number of occupied cells.
    /// </summary>
    public int GetOccupiedCellCount()
    {
        int count = 0;

        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                if (_grid[x, y] != null)
                {
                    count++;
                }
            }
        }

        return count;
    }

    /// <summary>
    /// Gets the number of empty cells.
    /// </summary>
    public int GetEmptyCellCount()
    {
        return TotalCapacity - GetOccupiedCellCount();
    }

    /// <summary>
    /// Checks if a cell is occupied.
    /// </summary>
    public bool IsCellOccupied(int gridX, int gridY)
    {
        if (gridX < 0 || gridX >= _width || gridY < 0 || gridY >= _height)
            return true; // Out of bounds = occupied

        return _grid[gridX, gridY] != null;
    }

    /// <summary>
    /// Gets the grid state for visualization (for UI).
    /// Returns a 2D array where true = occupied.
    /// </summary>
    public bool[,] GetGridState()
    {
        bool[,] state = new bool[_width, _height];

        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                state[x, y] = _grid[x, y] != null;
            }
        }

        return state;
    }

    /// <summary>
    /// Clears all items from the grid.
    /// </summary>
    public void Clear()
    {
        _grid = new InventoryItem[_width, _height];
    }

    /// <summary>
    /// Resizes the grid. Warning: This clears all items!
    /// </summary>
    public void Resize(int newWidth, int newHeight)
    {
        _width = newWidth;
        _height = newHeight;
        _grid = new InventoryItem[newWidth, newHeight];
    }

    /// <summary>
    /// Gets all cells occupied by a specific item.
    /// </summary>
    public List<Vector2Int> GetItemCells(InventoryItem item)
    {
        List<Vector2Int> cells = new List<Vector2Int>();

        if (item == null)
            return cells;

        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                if (_grid[x, y] == item)
                {
                    cells.Add(new Vector2Int(x, y));
                }
            }
        }

        return cells;
    }

    /// <summary>
    /// Checks if there's enough contiguous space for an item of given dimensions.
    /// </summary>
    public bool HasContiguousSpace(int requiredWidth, int requiredHeight)
    {
        for (int y = 0; y <= _height - requiredHeight; y++)
        {
            for (int x = 0; x <= _width - requiredWidth; x++)
            {
                bool hasSpace = true;

                // Check if this rectangular area is empty
                for (int dy = 0; dy < requiredHeight && hasSpace; dy++)
                {
                    for (int dx = 0; dx < requiredWidth && hasSpace; dx++)
                    {
                        if (_grid[x + dx, y + dy] != null)
                        {
                            hasSpace = false;
                        }
                    }
                }

                if (hasSpace)
                    return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Gets the largest contiguous empty rectangle in the grid.
    /// Useful for optimization suggestions.
    /// </summary>
    public Vector2Int GetLargestEmptyArea()
    {
        int maxArea = 0;
        Vector2Int bestSize = Vector2Int.zero;

        // Try all possible rectangle sizes
        for (int h = 1; h <= _height; h++)
        {
            for (int w = 1; w <= _width; w++)
            {
                if (HasContiguousSpace(w, h))
                {
                    int area = w * h;
                    if (area > maxArea)
                    {
                        maxArea = area;
                        bestSize = new Vector2Int(w, h);
                    }
                }
            }
        }

        return bestSize;
    }
}

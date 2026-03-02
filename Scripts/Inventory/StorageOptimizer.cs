using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Provides suggestions for optimal item placement in the inventory grid.
/// Calculates wasted space and provides visual hints for efficient packing.
/// </summary>
public class StorageOptimizer
{
    private InventoryGrid _grid;

    /// <summary>
    /// Optimization hint for item placement.
    /// </summary>
    public struct PlacementHint
    {
        public int gridX;
        public int gridY;
        public int rotation;
        public float efficiencyScore; // 0-1, higher is better
        public string reason;

        public PlacementHint(int x, int y, int rot, float score, string reason)
        {
            this.gridX = x;
            this.gridY = y;
            this.rotation = rot;
            this.efficiencyScore = score;
            this.reason = reason;
        }
    }

    /// <summary>
    /// Storage efficiency statistics.
    /// </summary>
    public struct EfficiencyStats
    {
        public int totalCells;
        public int occupiedCells;
        public int wastedCells;
        public float utilizationPercent;
        public float fragmentationScore; // 0-1, higher = more fragmented
        public int largestContiguousArea;

        public EfficiencyStats(int total, int occupied, int wasted, float utilization, float fragmentation, int largestArea)
        {
            this.totalCells = total;
            this.occupiedCells = occupied;
            this.wastedCells = wasted;
            this.utilizationPercent = utilization;
            this.fragmentationScore = fragmentation;
            this.largestContiguousArea = largestArea;
        }
    }

    public StorageOptimizer(InventoryGrid grid)
    {
        _grid = grid;
    }

    /// <summary>
    /// Gets the best placement position for an item.
    /// Considers efficiency, compactness, and future flexibility.
    /// </summary>
    public PlacementHint GetOptimalPlacement(InventoryItem item)
    {
        List<PlacementHint> hints = GetAllPlacementHints(item);

        if (hints.Count == 0)
        {
            return new PlacementHint(-1, -1, 0, 0f, "No space available");
        }

        // Return the hint with the highest efficiency score
        return hints.OrderByDescending(h => h.efficiencyScore).First();
    }

    /// <summary>
    /// Gets all possible placement hints for an item, sorted by efficiency.
    /// </summary>
    public List<PlacementHint> GetAllPlacementHints(InventoryItem item)
    {
        List<PlacementHint> hints = new List<PlacementHint>();

        if (item == null)
            return hints;

        int originalRotation = item.GetRotation();

        // Try all rotations
        for (int r = 0; r < 4; r++)
        {
            int rotation = r * 90;
            item.SetRotation(rotation);

            // Try all positions
            for (int y = 0; y < _grid.Height; y++)
            {
                for (int x = 0; x < _grid.Width; x++)
                {
                    if (_grid.CanPlaceItem(item, x, y))
                    {
                        float score = CalculatePlacementScore(item, x, y);
                        string reason = GetPlacementReason(score);

                        hints.Add(new PlacementHint(x, y, rotation, score, reason));
                    }
                }
            }
        }

        // Restore original rotation
        item.SetRotation(originalRotation);

        return hints.OrderByDescending(h => h.efficiencyScore).ToList();
    }

    /// <summary>
    /// Calculates a score for placing an item at a position (0-1, higher is better).
    /// </summary>
    private float CalculatePlacementScore(InventoryItem item, int x, int y)
    {
        float score = 0f;

        // 1. Corner preference (items near edges are more efficient)
        float cornerScore = CalculateCornerScore(x, y, item.GetWidth(), item.GetHeight());
        score += cornerScore * 0.4f;

        // 2. Adjacency score (placing near other items reduces fragmentation)
        float adjacencyScore = CalculateAdjacencyScore(item, x, y);
        score += adjacencyScore * 0.3f;

        // 3. Compactness (prefer placements that keep items together)
        float compactnessScore = CalculateCompactnessScore(x, y);
        score += compactnessScore * 0.2f;

        // 4. Future flexibility (leave large contiguous spaces)
        float flexibilityScore = CalculateFlexibilityScore(item, x, y);
        score += flexibilityScore * 0.1f;

        return Mathf.Clamp01(score);
    }

    /// <summary>
    /// Prefers corners and edges.
    /// </summary>
    private float CalculateCornerScore(int x, int y, int width, int height)
    {
        float score = 0f;

        // Distance from top-left corner (closer is better)
        float distanceFromTopLeft = Mathf.Sqrt(x * x + y * y);
        float maxDistance = Mathf.Sqrt(_grid.Width * _grid.Width + _grid.Height * _grid.Height);
        score = 1f - (distanceFromTopLeft / maxDistance);

        return score;
    }

    /// <summary>
    /// Prefers placements adjacent to existing items.
    /// </summary>
    private float CalculateAdjacencyScore(InventoryItem item, int x, int y)
    {
        int adjacentSides = 0;

        // Check left
        if (x == 0 || _grid.IsCellOccupied(x - 1, y))
            adjacentSides++;

        // Check right
        if (x + item.GetWidth() >= _grid.Width || _grid.IsCellOccupied(x + item.GetWidth(), y))
            adjacentSides++;

        // Check top
        if (y == 0 || _grid.IsCellOccupied(x, y - 1))
            adjacentSides++;

        // Check bottom
        if (y + item.GetHeight() >= _grid.Height || _grid.IsCellOccupied(x, y + item.GetHeight()))
            adjacentSides++;

        return adjacentSides / 4f;
    }

    /// <summary>
    /// Prefers placements that keep items grouped together.
    /// </summary>
    private float CalculateCompactnessScore(int x, int y)
    {
        // Find the center of mass of all items
        Vector2 centerOfMass = CalculateCenterOfMass();

        // Distance from this position to center of mass
        float distance = Vector2.Distance(new Vector2(x, y), centerOfMass);
        float maxDistance = Mathf.Sqrt(_grid.Width * _grid.Width + _grid.Height * _grid.Height);

        return 1f - (distance / maxDistance);
    }

    /// <summary>
    /// Prefers placements that don't fragment the remaining space.
    /// </summary>
    private float CalculateFlexibilityScore(InventoryItem item, int x, int y)
    {
        // Temporarily place the item
        int originalX = item.GridX;
        int originalY = item.GridY;

        _grid.PlaceItem(item, x, y);

        // Check the largest remaining contiguous area
        Vector2Int largestArea = _grid.GetLargestEmptyArea();
        int areaSize = largestArea.x * largestArea.y;

        // Remove the item
        _grid.RemoveItem(item);

        if (originalX >= 0 && originalY >= 0)
        {
            _grid.PlaceItem(item, originalX, originalY);
        }

        // Normalize by total grid size
        float score = (float)areaSize / _grid.TotalCapacity;

        return score;
    }

    /// <summary>
    /// Calculates the center of mass of all items in the grid.
    /// </summary>
    private Vector2 CalculateCenterOfMass()
    {
        List<InventoryItem> items = _grid.GetAllItems();

        if (items.Count == 0)
            return new Vector2(_grid.Width / 2f, _grid.Height / 2f);

        float sumX = 0f;
        float sumY = 0f;

        foreach (var item in items)
        {
            // Use the item's center position
            float centerX = item.GridX + item.GetWidth() / 2f;
            float centerY = item.GridY + item.GetHeight() / 2f;

            sumX += centerX;
            sumY += centerY;
        }

        return new Vector2(sumX / items.Count, sumY / items.Count);
    }

    /// <summary>
    /// Gets a human-readable reason for the placement score.
    /// </summary>
    private string GetPlacementReason(float score)
    {
        if (score >= 0.8f)
            return "Excellent - Very efficient placement";
        else if (score >= 0.6f)
            return "Good - Solid placement";
        else if (score >= 0.4f)
            return "Okay - Acceptable but not optimal";
        else if (score >= 0.2f)
            return "Poor - Creates fragmentation";
        else
            return "Bad - Highly inefficient";
    }

    /// <summary>
    /// Calculates overall storage efficiency statistics.
    /// </summary>
    public EfficiencyStats CalculateEfficiency()
    {
        int totalCells = _grid.TotalCapacity;
        int occupiedCells = _grid.GetOccupiedCellCount();
        int emptyCells = _grid.GetEmptyCellCount();

        // Calculate wasted space (fragmented cells that can't fit typical items)
        int wastedCells = CalculateWastedCells();

        // Utilization percentage
        float utilization = (occupiedCells / (float)totalCells) * 100f;

        // Fragmentation score
        float fragmentation = CalculateFragmentation();

        // Largest contiguous area
        Vector2Int largestArea = _grid.GetLargestEmptyArea();
        int largestAreaSize = largestArea.x * largestArea.y;

        return new EfficiencyStats(
            totalCells,
            occupiedCells,
            wastedCells,
            utilization,
            fragmentation,
            largestAreaSize
        );
    }

    /// <summary>
    /// Calculates wasted cells (small gaps that can't fit typical items).
    /// </summary>
    private int CalculateWastedCells()
    {
        int wastedCells = 0;

        // Consider a cell "wasted" if it's part of a gap smaller than 2x2
        bool[,] visited = new bool[_grid.Width, _grid.Height];

        for (int y = 0; y < _grid.Height; y++)
        {
            for (int x = 0; x < _grid.Width; x++)
            {
                if (!_grid.IsCellOccupied(x, y) && !visited[x, y])
                {
                    int gapSize = FloodFillCount(x, y, visited);

                    // Gaps smaller than 4 cells are considered wasted
                    if (gapSize < 4)
                    {
                        wastedCells += gapSize;
                    }
                }
            }
        }

        return wastedCells;
    }

    /// <summary>
    /// Flood fill to count contiguous empty cells.
    /// </summary>
    private int FloodFillCount(int startX, int startY, bool[,] visited)
    {
        if (startX < 0 || startX >= _grid.Width || startY < 0 || startY >= _grid.Height)
            return 0;

        if (visited[startX, startY] || _grid.IsCellOccupied(startX, startY))
            return 0;

        visited[startX, startY] = true;
        int count = 1;

        // Check 4 directions
        count += FloodFillCount(startX + 1, startY, visited);
        count += FloodFillCount(startX - 1, startY, visited);
        count += FloodFillCount(startX, startY + 1, visited);
        count += FloodFillCount(startX, startY - 1, visited);

        return count;
    }

    /// <summary>
    /// Calculates fragmentation score (0 = no fragmentation, 1 = highly fragmented).
    /// </summary>
    private float CalculateFragmentation()
    {
        int emptyCells = _grid.GetEmptyCellCount();

        if (emptyCells == 0)
            return 0f;

        Vector2Int largestArea = _grid.GetLargestEmptyArea();
        int largestAreaSize = largestArea.x * largestArea.y;

        // Fragmentation = (empty cells - largest contiguous area) / empty cells
        float fragmentation = (emptyCells - largestAreaSize) / (float)emptyCells;

        return Mathf.Clamp01(fragmentation);
    }

    /// <summary>
    /// Suggests reorganization to improve efficiency.
    /// Returns a message with suggestions.
    /// </summary>
    public string GetReorganizationSuggestion()
    {
        EfficiencyStats stats = CalculateEfficiency();

        if (stats.fragmentationScore < 0.3f)
        {
            return "Your inventory is well organized!";
        }
        else if (stats.fragmentationScore < 0.6f)
        {
            return "Consider rotating items to reduce gaps.";
        }
        else
        {
            return "Your inventory is fragmented. Try reorganizing items to create larger spaces.";
        }
    }

    /// <summary>
    /// Calculates bonus capacity earned from efficient packing.
    /// </summary>
    public int CalculateBonusCapacity()
    {
        EfficiencyStats stats = CalculateEfficiency();

        // Award bonus cells based on low fragmentation
        if (stats.fragmentationScore < 0.2f)
        {
            return 5; // Excellent packing
        }
        else if (stats.fragmentationScore < 0.4f)
        {
            return 3; // Good packing
        }
        else if (stats.fragmentationScore < 0.6f)
        {
            return 1; // Decent packing
        }

        return 0; // No bonus for poor packing
    }
}

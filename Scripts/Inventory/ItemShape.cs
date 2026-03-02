using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Defines the 2D shape of an item in the inventory grid.
/// Supports rotation and provides common shape presets.
/// </summary>
[System.Serializable]
public class ItemShape
{
    [SerializeField] private bool[,] _shape;
    [SerializeField] private int _width;
    [SerializeField] private int _height;
    [SerializeField] private int _currentRotation = 0; // 0, 90, 180, 270

    /// <summary>
    /// Gets the current width of the shape (accounts for rotation).
    /// </summary>
    public int Width => _currentRotation == 90 || _currentRotation == 270 ? _height : _width;

    /// <summary>
    /// Gets the current height of the shape (accounts for rotation).
    /// </summary>
    public int Height => _currentRotation == 90 || _currentRotation == 270 ? _width : _height;

    /// <summary>
    /// Gets the base width (without rotation).
    /// </summary>
    public int BaseWidth => _width;

    /// <summary>
    /// Gets the base height (without rotation).
    /// </summary>
    public int BaseHeight => _height;

    /// <summary>
    /// Gets the current rotation in degrees (0, 90, 180, 270).
    /// </summary>
    public int CurrentRotation => _currentRotation;

    /// <summary>
    /// Creates a new ItemShape from a 2D boolean array.
    /// </summary>
    /// <param name="shape">2D array where true = occupied cell</param>
    public ItemShape(bool[,] shape)
    {
        _width = shape.GetLength(0);
        _height = shape.GetLength(1);
        _shape = shape;
        _currentRotation = 0;
    }

    /// <summary>
    /// Creates a rectangular shape of given dimensions.
    /// </summary>
    public ItemShape(int width, int height)
    {
        _width = width;
        _height = height;
        _shape = new bool[width, height];

        // Fill entire rectangle
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                _shape[x, y] = true;
            }
        }
        _currentRotation = 0;
    }

    /// <summary>
    /// Checks if a cell is occupied at the given local coordinates (accounting for rotation).
    /// </summary>
    public bool IsOccupied(int x, int y)
    {
        if (x < 0 || y < 0 || x >= Width || y >= Height)
            return false;

        // Transform coordinates based on rotation
        int srcX = x;
        int srcY = y;

        switch (_currentRotation)
        {
            case 90:
                srcX = y;
                srcY = _width - 1 - x;
                break;
            case 180:
                srcX = _width - 1 - x;
                srcY = _height - 1 - y;
                break;
            case 270:
                srcX = _height - 1 - y;
                srcY = x;
                break;
        }

        if (srcX < 0 || srcY < 0 || srcX >= _width || srcY >= _height)
            return false;

        return _shape[srcX, srcY];
    }

    /// <summary>
    /// Rotates the shape 90 degrees clockwise.
    /// </summary>
    public void Rotate()
    {
        _currentRotation = (_currentRotation + 90) % 360;
    }

    /// <summary>
    /// Sets the rotation to a specific angle.
    /// </summary>
    public void SetRotation(int degrees)
    {
        _currentRotation = Mathf.Clamp(degrees, 0, 270);
        _currentRotation = (_currentRotation / 90) * 90; // Snap to 90 degree increments
    }

    /// <summary>
    /// Resets rotation to 0 degrees.
    /// </summary>
    public void ResetRotation()
    {
        _currentRotation = 0;
    }

    /// <summary>
    /// Gets all occupied cells in local coordinates (accounting for rotation).
    /// </summary>
    public List<Vector2Int> GetOccupiedCells()
    {
        List<Vector2Int> cells = new List<Vector2Int>();

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                if (IsOccupied(x, y))
                {
                    cells.Add(new Vector2Int(x, y));
                }
            }
        }

        return cells;
    }

    /// <summary>
    /// Creates a deep copy of this shape.
    /// </summary>
    public ItemShape Clone()
    {
        bool[,] shapeCopy = new bool[_width, _height];
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                shapeCopy[x, y] = _shape[x, y];
            }
        }

        ItemShape clone = new ItemShape(shapeCopy);
        clone._currentRotation = _currentRotation;
        return clone;
    }

    // ===== COMMON SHAPE PRESETS =====

    /// <summary>
    /// Creates a 1x1 square shape.
    /// </summary>
    public static ItemShape Square1x1()
    {
        return new ItemShape(1, 1);
    }

    /// <summary>
    /// Creates a 2x1 horizontal bar.
    /// </summary>
    public static ItemShape Bar2x1()
    {
        return new ItemShape(2, 1);
    }

    /// <summary>
    /// Creates a 3x1 horizontal bar.
    /// </summary>
    public static ItemShape Bar3x1()
    {
        return new ItemShape(3, 1);
    }

    /// <summary>
    /// Creates a 4x1 horizontal bar.
    /// </summary>
    public static ItemShape Bar4x1()
    {
        return new ItemShape(4, 1);
    }

    /// <summary>
    /// Creates a 2x2 square.
    /// </summary>
    public static ItemShape Square2x2()
    {
        return new ItemShape(2, 2);
    }

    /// <summary>
    /// Creates a 3x2 rectangle.
    /// </summary>
    public static ItemShape Rectangle3x2()
    {
        return new ItemShape(3, 2);
    }

    /// <summary>
    /// Creates an L-shape (3x2 with one corner missing).
    /// </summary>
    public static ItemShape LShape()
    {
        bool[,] shape = new bool[,]
        {
            { true, true },
            { true, false },
            { true, false }
        };
        return new ItemShape(shape);
    }

    /// <summary>
    /// Creates a T-shape (3x2 with corners missing).
    /// </summary>
    public static ItemShape TShape()
    {
        bool[,] shape = new bool[,]
        {
            { false, true },
            { true, true },
            { false, true }
        };
        return new ItemShape(shape);
    }

    /// <summary>
    /// Creates a Z-shape (3x2 zigzag).
    /// </summary>
    public static ItemShape ZShape()
    {
        bool[,] shape = new bool[,]
        {
            { true, false },
            { true, true },
            { false, true }
        };
        return new ItemShape(shape);
    }

    /// <summary>
    /// Creates an S-shape (3x2 zigzag, opposite of Z).
    /// </summary>
    public static ItemShape SShape()
    {
        bool[,] shape = new bool[,]
        {
            { false, true },
            { true, true },
            { true, false }
        };
        return new ItemShape(shape);
    }

    /// <summary>
    /// Creates a cross/plus shape (3x3 with corners missing).
    /// </summary>
    public static ItemShape CrossShape()
    {
        bool[,] shape = new bool[,]
        {
            { false, true, false },
            { true, true, true },
            { false, true, false }
        };
        return new ItemShape(shape);
    }

    /// <summary>
    /// Creates a fish-like shape for common fish (2x3 elongated).
    /// </summary>
    public static ItemShape FishSmall()
    {
        return new ItemShape(2, 3);
    }

    /// <summary>
    /// Creates a large fish shape (3x4).
    /// </summary>
    public static ItemShape FishMedium()
    {
        return new ItemShape(3, 4);
    }

    /// <summary>
    /// Creates a huge fish shape (4x5 with tapered tail).
    /// </summary>
    public static ItemShape FishLarge()
    {
        bool[,] shape = new bool[,]
        {
            { true, true, true, true, false },
            { true, true, true, true, true },
            { true, true, true, true, true },
            { true, true, true, true, false }
        };
        return new ItemShape(shape);
    }

    /// <summary>
    /// Creates an eel shape (1x5 very long and thin).
    /// </summary>
    public static ItemShape Eel()
    {
        return new ItemShape(1, 5);
    }

    /// <summary>
    /// Creates a round pufferfish shape (3x3 with corners cut).
    /// </summary>
    public static ItemShape Pufferfish()
    {
        bool[,] shape = new bool[,]
        {
            { false, true, false },
            { true, true, true },
            { false, true, false }
        };
        return new ItemShape(shape);
    }

    /// <summary>
    /// Creates an octopus/aberrant shape (3x3 irregular).
    /// </summary>
    public static ItemShape Aberrant()
    {
        bool[,] shape = new bool[,]
        {
            { true, false, true },
            { true, true, true },
            { true, true, true }
        };
        return new ItemShape(shape);
    }
}

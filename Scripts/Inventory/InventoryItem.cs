using UnityEngine;

/// <summary>
/// Wrapper class for items stored in the inventory grid.
/// Combines IInventoryItem data with grid placement information.
/// </summary>
[System.Serializable]
public class InventoryItem
{
    // Reference to the underlying item data
    [SerializeField] private IInventoryItem _itemData;

    // Grid placement data
    [SerializeField] private int _gridX;
    [SerializeField] private int _gridY;
    [SerializeField] private ItemShape _shape;

    // Visual state
    [SerializeField] private bool _isFresh = true;
    [SerializeField] private float _durability = 100f;

    /// <summary>
    /// The underlying item implementing IInventoryItem.
    /// </summary>
    public IInventoryItem ItemData => _itemData;

    /// <summary>
    /// Grid X position.
    /// </summary>
    public int GridX
    {
        get => _gridX;
        set => _gridX = value;
    }

    /// <summary>
    /// Grid Y position.
    /// </summary>
    public int GridY
    {
        get => _gridY;
        set => _gridY = value;
    }

    /// <summary>
    /// The shape of this item in the grid.
    /// </summary>
    public ItemShape Shape => _shape;

    /// <summary>
    /// Whether the item is fresh (for fish).
    /// </summary>
    public bool IsFresh
    {
        get => _isFresh;
        set => _isFresh = value;
    }

    /// <summary>
    /// Item durability/condition (0-100).
    /// </summary>
    public float Durability
    {
        get => _durability;
        set => _durability = Mathf.Clamp(value, 0f, 100f);
    }

    /// <summary>
    /// Quick access to item ID.
    /// </summary>
    public string ItemID => _itemData?.ItemID ?? "";

    /// <summary>
    /// Quick access to item name.
    /// </summary>
    public string ItemName => _itemData?.ItemName ?? "Unknown";

    /// <summary>
    /// Quick access to item icon.
    /// </summary>
    public Sprite Icon => _itemData?.Icon;

    /// <summary>
    /// Creates a new inventory item wrapper.
    /// </summary>
    /// <param name="itemData">The item implementing IInventoryItem</param>
    /// <param name="shape">The shape of the item in the grid</param>
    public InventoryItem(IInventoryItem itemData, ItemShape shape)
    {
        _itemData = itemData;
        _shape = shape ?? ItemShape.Square1x1();
        _gridX = -1;
        _gridY = -1;
        _isFresh = true;
        _durability = 100f;
    }

    /// <summary>
    /// Creates an inventory item from a fish.
    /// </summary>
    public static InventoryItem FromFish(Fish fish)
    {
        // Create appropriate shape based on fish size
        ItemShape shape = GetShapeForFish(fish);

        // Wrap the fish as an IInventoryItem
        FishInventoryItem fishItem = new FishInventoryItem(fish);

        return new InventoryItem(fishItem, shape);
    }

    /// <summary>
    /// Determines the appropriate shape for a fish based on its properties.
    /// </summary>
    private static ItemShape GetShapeForFish(Fish fish)
    {
        // Aberrant fish have irregular shapes
        if (fish.isAberrant)
        {
            return ItemShape.Aberrant();
        }

        // Use the inventory size from fish data
        Vector2Int size = fish.inventorySize;
        int width = size.x;
        int height = size.y;

        // If fish has specific dimensions, use them
        if (width > 0 && height > 0)
        {
            return new ItemShape(width, height);
        }

        // Otherwise, base on rarity
        switch (fish.rarity)
        {
            case FishRarity.Common:
                return ItemShape.FishSmall();
            case FishRarity.Uncommon:
                return ItemShape.FishMedium();
            case FishRarity.Rare:
                return ItemShape.FishMedium();
            case FishRarity.Legendary:
                return ItemShape.FishLarge();
            default:
                return ItemShape.Square1x1();
        }
    }

    /// <summary>
    /// Rotates the item's shape 90 degrees.
    /// </summary>
    public void Rotate()
    {
        if (_itemData?.CanRotate() ?? false)
        {
            _shape.Rotate();
        }
    }

    /// <summary>
    /// Sets the item's rotation.
    /// </summary>
    public void SetRotation(int degrees)
    {
        if (_itemData?.CanRotate() ?? false)
        {
            _shape.SetRotation(degrees);
        }
    }

    /// <summary>
    /// Gets the current rotation in degrees.
    /// </summary>
    public int GetRotation()
    {
        return _shape.CurrentRotation;
    }

    /// <summary>
    /// Gets the current width (accounting for rotation).
    /// </summary>
    public int GetWidth()
    {
        return _shape.Width;
    }

    /// <summary>
    /// Gets the current height (accounting for rotation).
    /// </summary>
    public int GetHeight()
    {
        return _shape.Height;
    }

    /// <summary>
    /// Gets the item's value considering condition.
    /// </summary>
    public float GetValue()
    {
        float baseValue = _itemData?.GetValue() ?? 0f;

        // Reduce value if not fresh or damaged
        float freshnessMultiplier = _isFresh ? 1f : 0.5f;
        float durabilityMultiplier = _durability / 100f;

        return baseValue * freshnessMultiplier * durabilityMultiplier;
    }

    /// <summary>
    /// Converts this inventory item to a serialized format for saving.
    /// </summary>
    public SerializedItem ToSerializedItem()
    {
        SerializedItem serialized = new SerializedItem
        {
            itemID = ItemID,
            itemName = ItemName,
            gridX = _gridX,
            gridY = _gridY,
            width = _shape.BaseWidth,
            height = _shape.BaseHeight,
            rotation = _shape.CurrentRotation,
            durability = _durability,
            isFresh = _isFresh,
            value = GetValue()
        };

        // Add fish-specific data if this is a fish
        if (_itemData is FishInventoryItem fishItem)
        {
            serialized.itemType = "fish";
            serialized.fishSpecies = fishItem.Fish.id;
            serialized.fishWeight = fishItem.Fish.weight;
            serialized.isAberrant = fishItem.Fish.isAberrant;
            serialized.rarity = fishItem.Fish.rarity;
        }
        else
        {
            serialized.itemType = "generic";
        }

        return serialized;
    }
}

/// <summary>
/// Adapter class that wraps a Fish object to implement IInventoryItem.
/// </summary>
public class FishInventoryItem : IInventoryItem
{
    private Fish _fish;

    public Fish Fish => _fish;

    public string ItemID => _fish.id;
    public string ItemName => _fish.name;
    public int Width => _fish.inventorySize.x;
    public int Height => _fish.inventorySize.y;
    public Sprite Icon => _fish.icon;

    public FishInventoryItem(Fish fish)
    {
        _fish = fish;
    }

    public string GetDescription()
    {
        string desc = _fish.description;
        desc += $"\n\nWeight: {_fish.weight:F2} kg";
        desc += $"\nLength: {_fish.length:F1} cm";
        desc += $"\nRarity: {_fish.rarity}";
        if (_fish.isAberrant)
        {
            desc += "\n<color=purple>[ABERRANT]</color>";
        }
        return desc;
    }

    public float GetValue()
    {
        return _fish.GetSellValue();
    }

    public bool CanRotate()
    {
        // Fish can be rotated to fit better
        return true;
    }

    public bool CanStackWith(IInventoryItem other)
    {
        // Fish don't stack
        return false;
    }
}

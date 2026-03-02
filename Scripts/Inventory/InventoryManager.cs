using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Main inventory management system for Bahnfish.
/// Implements the IInventorySystem interface and manages the grid-based Tetris inventory.
/// Singleton pattern ensures only one inventory exists.
/// </summary>
public class InventoryManager : MonoBehaviour, IInventorySystem
{
    // Singleton instance
    private static InventoryManager _instance;
    public static InventoryManager Instance => _instance;

    [Header("Grid Configuration")]
    [SerializeField] private int _gridWidth = 10;
    [SerializeField] private int _gridHeight = 10;

    [Header("Cooler Slots")]
    [SerializeField] private int _coolerSlots = 4;
    [SerializeField] private List<InventoryItem> _coolerItems = new List<InventoryItem>();

    [Header("Equipment Slots")]
    [SerializeField] private int _equipmentSlots = 6;
    [SerializeField] private List<InventoryItem> _equipmentItems = new List<InventoryItem>();

    // Core systems
    private InventoryGrid _mainGrid;
    private StorageOptimizer _optimizer;

    // Events
    public event System.Action OnInventoryChanged;
    public event System.Action OnInventoryFull;
    public event System.Action<InventoryItem> OnItemAdded;
    public event System.Action<InventoryItem> OnItemRemoved;

    // Properties
    public InventoryGrid MainGrid => _mainGrid;
    public StorageOptimizer Optimizer => _optimizer;
    public int CoolerSlotsAvailable => _coolerSlots - _coolerItems.Count;
    public int EquipmentSlotsAvailable => _equipmentSlots - _equipmentItems.Count;

    private void Awake()
    {
        // Singleton pattern
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        // Initialize grid and optimizer
        _mainGrid = new InventoryGrid(_gridWidth, _gridHeight);
        _optimizer = new StorageOptimizer(_mainGrid);
    }

    private void Start()
    {
        // Subscribe to save/load events
        EventSystem.Subscribe<SaveData>("GatheringSaveData", OnGatheringSaveData);
        EventSystem.Subscribe<SaveData>("ApplyingSaveData", OnApplyingSaveData);

        Debug.Log($"InventoryManager initialized with {_gridWidth}x{_gridHeight} grid ({_mainGrid.TotalCapacity} cells)");
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        EventSystem.Unsubscribe<SaveData>("GatheringSaveData", OnGatheringSaveData);
        EventSystem.Unsubscribe<SaveData>("ApplyingSaveData", OnApplyingSaveData);
    }

    // ===== IInventorySystem Implementation =====

    /// <summary>
    /// Attempts to add an item to the inventory, finding the best placement automatically.
    /// </summary>
    public bool AddItem(InventoryItem item)
    {
        if (item == null)
        {
            Debug.LogWarning("Cannot add null item to inventory");
            return false;
        }

        // Try to find optimal placement with rotation
        if (_mainGrid.FindEmptyPositionWithRotation(item, out int x, out int y, out int rotation))
        {
            item.SetRotation(rotation);
            return AddItemAt(item, x, y);
        }

        // Inventory full
        Debug.Log($"Cannot add {item.ItemName} - inventory full");
        OnInventoryFull?.Invoke();
        EventSystem.Publish("InventoryFull", this);
        return false;
    }

    /// <summary>
    /// Attempts to add an item at a specific grid position.
    /// </summary>
    public bool AddItemAt(InventoryItem item, int gridX, int gridY)
    {
        if (item == null)
        {
            Debug.LogWarning("Cannot add null item to inventory");
            return false;
        }

        if (_mainGrid.PlaceItem(item, gridX, gridY))
        {
            Debug.Log($"Added {item.ItemName} to inventory at ({gridX}, {gridY})");

            // Trigger events
            OnItemAdded?.Invoke(item);
            OnInventoryChanged?.Invoke();

            // Publish to EventSystem
            EventSystem.Publish("ItemAddedToInventory", item);
            EventSystem.Publish("InventoryChanged", this);

            return true;
        }

        Debug.LogWarning($"Cannot place {item.ItemName} at ({gridX}, {gridY}) - space occupied or out of bounds");
        return false;
    }

    /// <summary>
    /// Removes an item from the inventory.
    /// </summary>
    public bool RemoveItem(InventoryItem item)
    {
        if (item == null)
            return false;

        // Check cooler slots first
        if (_coolerItems.Contains(item))
        {
            _coolerItems.Remove(item);
            OnItemRemoved?.Invoke(item);
            OnInventoryChanged?.Invoke();
            EventSystem.Publish("ItemRemovedFromInventory", item);
            EventSystem.Publish("InventoryChanged", this);
            return true;
        }

        // Check equipment slots
        if (_equipmentItems.Contains(item))
        {
            _equipmentItems.Remove(item);
            OnItemRemoved?.Invoke(item);
            OnInventoryChanged?.Invoke();
            EventSystem.Publish("ItemRemovedFromInventory", item);
            EventSystem.Publish("InventoryChanged", this);
            return true;
        }

        // Remove from main grid
        if (_mainGrid.RemoveItem(item))
        {
            Debug.Log($"Removed {item.ItemName} from inventory");

            OnItemRemoved?.Invoke(item);
            OnInventoryChanged?.Invoke();

            EventSystem.Publish("ItemRemovedFromInventory", item);
            EventSystem.Publish("InventoryChanged", this);

            return true;
        }

        return false;
    }

    /// <summary>
    /// Gets the number of available (empty) grid cells.
    /// </summary>
    public int GetAvailableSpace()
    {
        return _mainGrid.GetEmptyCellCount();
    }

    /// <summary>
    /// Gets all items in the main inventory grid.
    /// </summary>
    public List<InventoryItem> GetAllItems()
    {
        return _mainGrid.GetAllItems();
    }

    /// <summary>
    /// Checks if an item can fit anywhere in the inventory.
    /// </summary>
    public bool CanFitItem(InventoryItem item)
    {
        if (item == null)
            return false;

        return _mainGrid.FindEmptyPositionWithRotation(item, out int _, out int _, out int _);
    }

    /// <summary>
    /// Clears all items from the inventory.
    /// </summary>
    public void ClearInventory()
    {
        _mainGrid.Clear();
        _coolerItems.Clear();
        _equipmentItems.Clear();

        OnInventoryChanged?.Invoke();
        EventSystem.Publish("InventoryCleared", this);
        EventSystem.Publish("InventoryChanged", this);

        Debug.Log("Inventory cleared");
    }

    /// <summary>
    /// Gets the total capacity of the main grid.
    /// </summary>
    public int GetTotalCapacity()
    {
        return _mainGrid.TotalCapacity;
    }

    /// <summary>
    /// Gets the number of occupied cells.
    /// </summary>
    public int GetOccupiedSpace()
    {
        return _mainGrid.GetOccupiedCellCount();
    }

    // ===== Cooler System =====

    /// <summary>
    /// Adds an item to a cooler slot (premium storage that keeps fish fresh).
    /// </summary>
    public bool AddToCooler(InventoryItem item)
    {
        if (item == null)
            return false;

        if (_coolerItems.Count >= _coolerSlots)
        {
            Debug.LogWarning("Cooler is full");
            return false;
        }

        _coolerItems.Add(item);
        item.IsFresh = true; // Cooler keeps items fresh

        OnItemAdded?.Invoke(item);
        OnInventoryChanged?.Invoke();

        Debug.Log($"Added {item.ItemName} to cooler");
        return true;
    }

    /// <summary>
    /// Gets all items in cooler storage.
    /// </summary>
    public List<InventoryItem> GetCoolerItems()
    {
        return new List<InventoryItem>(_coolerItems);
    }

    // ===== Equipment System =====

    /// <summary>
    /// Adds an item to an equipment slot.
    /// </summary>
    public bool AddToEquipment(InventoryItem item)
    {
        if (item == null)
            return false;

        if (_equipmentItems.Count >= _equipmentSlots)
        {
            Debug.LogWarning("Equipment slots full");
            return false;
        }

        _equipmentItems.Add(item);

        OnItemAdded?.Invoke(item);
        OnInventoryChanged?.Invoke();

        Debug.Log($"Added {item.ItemName} to equipment");
        return true;
    }

    /// <summary>
    /// Gets all items in equipment slots.
    /// </summary>
    public List<InventoryItem> GetEquipmentItems()
    {
        return new List<InventoryItem>(_equipmentItems);
    }

    // ===== Fish-Specific Methods =====

    /// <summary>
    /// Adds a caught fish to the inventory.
    /// </summary>
    public bool AddFish(Fish fish)
    {
        if (fish == null)
        {
            Debug.LogWarning("Cannot add null fish");
            return false;
        }

        // Create inventory item from fish
        InventoryItem fishItem = InventoryItem.FromFish(fish);

        // Try cooler first for rare fish
        if (fish.rarity >= FishRarity.Rare && CoolerSlotsAvailable > 0)
        {
            if (AddToCooler(fishItem))
            {
                Debug.Log($"Added rare fish {fish.name} to cooler");
                return true;
            }
        }

        // Otherwise add to main grid
        return AddItem(fishItem);
    }

    /// <summary>
    /// Gets all fish items in the inventory.
    /// </summary>
    public List<InventoryItem> GetAllFish()
    {
        List<InventoryItem> allFish = new List<InventoryItem>();

        // Main grid fish
        foreach (var item in GetAllItems())
        {
            if (item.ItemData is FishInventoryItem)
            {
                allFish.Add(item);
            }
        }

        // Cooler fish
        foreach (var item in _coolerItems)
        {
            if (item.ItemData is FishInventoryItem)
            {
                allFish.Add(item);
            }
        }

        return allFish;
    }

    /// <summary>
    /// Calculates total value of all fish in inventory.
    /// </summary>
    public float GetTotalFishValue()
    {
        float total = 0f;

        foreach (var fish in GetAllFish())
        {
            total += fish.GetValue();
        }

        return total;
    }

    // ===== Optimization Methods =====

    /// <summary>
    /// Gets optimization hints for placing an item.
    /// </summary>
    public List<StorageOptimizer.PlacementHint> GetPlacementHints(InventoryItem item)
    {
        return _optimizer.GetAllPlacementHints(item);
    }

    /// <summary>
    /// Gets the optimal placement for an item.
    /// </summary>
    public StorageOptimizer.PlacementHint GetOptimalPlacement(InventoryItem item)
    {
        return _optimizer.GetOptimalPlacement(item);
    }

    /// <summary>
    /// Gets efficiency statistics for the current inventory state.
    /// </summary>
    public StorageOptimizer.EfficiencyStats GetEfficiencyStats()
    {
        return _optimizer.CalculateEfficiency();
    }

    /// <summary>
    /// Gets a suggestion for reorganizing the inventory.
    /// </summary>
    public string GetReorganizationSuggestion()
    {
        return _optimizer.GetReorganizationSuggestion();
    }

    // ===== Save/Load Integration =====

    private void OnGatheringSaveData(SaveData saveData)
    {
        // Clear existing inventory data
        saveData.inventoryItems.Clear();

        // Save main grid items
        foreach (var item in GetAllItems())
        {
            saveData.inventoryItems.Add(item.ToSerializedItem());
        }

        // Save cooler items
        foreach (var item in _coolerItems)
        {
            SerializedItem serialized = item.ToSerializedItem();
            serialized.additionalData = "cooler";
            saveData.inventoryItems.Add(serialized);
        }

        // Save equipment items
        foreach (var item in _equipmentItems)
        {
            SerializedItem serialized = item.ToSerializedItem();
            serialized.additionalData = "equipment";
            saveData.inventoryItems.Add(serialized);
        }

        saveData.inventoryCapacity = GetTotalCapacity();

        Debug.Log($"Saved {saveData.inventoryItems.Count} inventory items");
    }

    private void OnApplyingSaveData(SaveData saveData)
    {
        // Clear current inventory
        ClearInventory();

        // Resize grid if needed
        if (saveData.inventoryCapacity != GetTotalCapacity())
        {
            int newSize = Mathf.RoundToInt(Mathf.Sqrt(saveData.inventoryCapacity));
            _mainGrid.Resize(newSize, newSize);
            Debug.Log($"Resized inventory grid to {newSize}x{newSize}");
        }

        // Load items
        foreach (var serializedItem in saveData.inventoryItems)
        {
            InventoryItem item = DeserializeItem(serializedItem);

            if (item != null)
            {
                // Check storage location
                if (serializedItem.additionalData == "cooler")
                {
                    AddToCooler(item);
                }
                else if (serializedItem.additionalData == "equipment")
                {
                    AddToEquipment(item);
                }
                else
                {
                    // Main grid item - restore exact position and rotation
                    item.SetRotation(serializedItem.rotation);
                    AddItemAt(item, serializedItem.gridX, serializedItem.gridY);
                }
            }
        }

        Debug.Log($"Loaded {saveData.inventoryItems.Count} inventory items");
    }

    /// <summary>
    /// Deserializes a saved item back into an InventoryItem.
    /// </summary>
    private InventoryItem DeserializeItem(SerializedItem serialized)
    {
        // For now, we only support fish items
        // Other item types would need their own deserialization logic

        if (serialized.itemType == "fish")
        {
            // Recreate the fish
            Fish fish = new Fish
            {
                id = serialized.fishSpecies,
                name = serialized.itemName,
                rarity = serialized.rarity,
                baseValue = serialized.value,
                inventorySize = new Vector2Int(serialized.width, serialized.height),
                isAberrant = serialized.isAberrant,
                weight = serialized.fishWeight
            };

            InventoryItem item = InventoryItem.FromFish(fish);
            item.IsFresh = serialized.isFresh;
            item.Durability = serialized.durability;
            item.SetRotation(serialized.rotation);

            return item;
        }

        Debug.LogWarning($"Unknown item type: {serialized.itemType}");
        return null;
    }

    // ===== Debug Methods =====

    /// <summary>
    /// Logs current inventory state for debugging.
    /// </summary>
    public void DebugPrintInventory()
    {
        Debug.Log("=== INVENTORY STATE ===");
        Debug.Log($"Grid: {_gridWidth}x{_gridHeight} ({GetOccupiedSpace()}/{GetTotalCapacity()} cells used)");
        Debug.Log($"Items in main grid: {GetAllItems().Count}");
        Debug.Log($"Items in cooler: {_coolerItems.Count}/{_coolerSlots}");
        Debug.Log($"Items in equipment: {_equipmentItems.Count}/{_equipmentSlots}");

        var stats = GetEfficiencyStats();
        Debug.Log($"Efficiency: {stats.utilizationPercent:F1}%");
        Debug.Log($"Fragmentation: {stats.fragmentationScore:F2}");
        Debug.Log($"Wasted cells: {stats.wastedCells}");
    }
}

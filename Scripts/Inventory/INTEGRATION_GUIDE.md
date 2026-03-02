# Inventory System Integration Guide

## Quick Start

### Step 1: Add InventoryManager to Scene

1. Create an empty GameObject in your main scene
2. Name it "InventoryManager"
3. Add the `InventoryManager.cs` component
4. Configure grid size (default: 10x10)
5. Configure cooler slots (default: 4)
6. Configure equipment slots (default: 6)

**The InventoryManager is a singleton - it will persist across scenes automatically.**

### Step 2: Set Up UI

1. Create a Canvas for the inventory UI
2. Create an empty GameObject under Canvas, name it "InventoryPanel"
3. Add the `InventoryUI.cs` component to InventoryPanel
4. Create child objects:
   - **GridContainer** - RectTransform for grid cells
   - **CoolerContainer** - RectTransform for cooler slots
   - **EquipmentContainer** - RectTransform for equipment slots

5. Create prefabs:
   - **GridCellPrefab** - Contains:
     - Image component (background)
     - Image component (item icon)
     - Image component (highlight)
     - Button component (for clicks)
     - GridCell.cs script

   - **CoolerSlotPrefab** - Similar to GridCell but styled differently
   - **EquipmentSlotPrefab** - Similar structure

6. Assign prefab references in InventoryUI inspector

7. Create DragDropHandler:
   - Add empty GameObject "DragDropHandler" to InventoryPanel
   - Add `DragDropHandler.cs` component
   - Assign InventoryUI reference
   - Assign Canvas reference
   - Create drag preview prefab (optional)

### Step 3: Test Basic Functionality

```csharp
// In your test script
void Start()
{
    // Create a test fish
    Fish testFish = new Fish
    {
        id = "bass_001",
        name = "Largemouth Bass",
        rarity = FishRarity.Common,
        baseValue = 25f,
        inventorySize = new Vector2Int(2, 3),
        icon = Resources.Load<Sprite>("Fish/Bass")
    };

    // Add to inventory
    bool success = InventoryManager.Instance.AddFish(testFish);

    if (success)
    {
        Debug.Log("Fish added successfully!");
    }

    // Open inventory UI
    InventoryUI inventoryUI = FindObjectOfType<InventoryUI>();
    inventoryUI.OpenInventory();
}
```

## Integration with Other Systems

### Fishing System (Agent 5)

When a fish is caught, add it to inventory:

```csharp
public class FishingController : MonoBehaviour
{
    // In your existing fishing code
    void OnFishCaught(Fish fish)
    {
        // Try to add fish to inventory
        bool added = InventoryManager.Instance.AddFish(fish);

        if (added)
        {
            // Success - play sound, show notification
            Debug.Log($"Caught {fish.name}!");
            EventSystem.Publish("FishStored", fish);
        }
        else
        {
            // Inventory full - handle it
            Debug.LogWarning("Inventory full! Fish escaped.");
            EventSystem.Publish("InventoryFullFishLost", fish);

            // Optional: Show UI message
            // UIManager.Instance.ShowNotification("Inventory Full!");
        }
    }
}
```

### Save/Load System (Agent 4)

**No action needed!** The inventory system automatically saves and loads via EventSystem:

```csharp
// Inventory automatically responds to these events:
// - "GatheringSaveData" - Adds inventory data to SaveData
// - "ApplyingSaveData" - Restores inventory from SaveData

// Just call SaveManager.Instance.SaveGame() and inventory is included!
```

### Progression System (Agent 9)

Selling fish and managing currency:

```csharp
public class FishMarket : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        // Get all fish
        List<InventoryItem> allFish = InventoryManager.Instance.GetAllFish();

        if (allFish.Count == 0)
        {
            Debug.Log("No fish to sell!");
            return;
        }

        // Calculate total value
        float totalValue = InventoryManager.Instance.GetTotalFishValue();

        // Remove all fish
        foreach (var fish in allFish)
        {
            InventoryManager.Instance.RemoveItem(fish);
        }

        // Add money via progression system
        ProgressionManager.Instance.AddMoney(totalValue);

        Debug.Log($"Sold {allFish.Count} fish for ${totalValue:F2}");

        // Show UI feedback
        // UIManager.Instance.ShowNotification($"Sold for ${totalValue:F2}");
    }
}
```

### UI System (Agent 11)

Accessing inventory UI from other systems:

```csharp
// Open inventory
public void OnInventoryButtonClicked()
{
    InventoryUI inventoryUI = FindObjectOfType<InventoryUI>();
    inventoryUI.OpenInventory();
}

// Or use a singleton if you make InventoryUI one:
InventoryUI.Instance.OpenInventory();

// Subscribe to inventory events for UI updates
void Start()
{
    InventoryManager.Instance.OnInventoryChanged += UpdateInventoryIndicator;
    InventoryManager.Instance.OnInventoryFull += ShowInventoryFullWarning;
}

void UpdateInventoryIndicator()
{
    int occupied = InventoryManager.Instance.GetOccupiedSpace();
    int total = InventoryManager.Instance.GetTotalCapacity();

    inventoryCapacityText.text = $"{occupied}/{total}";

    // Change color if nearly full
    if (occupied >= total * 0.9f)
    {
        inventoryCapacityText.color = Color.red;
    }
}

void ShowInventoryFullWarning()
{
    // Show warning message
    UIManager.Instance.ShowNotification("Inventory Full!", Color.red);
}
```

## EventSystem Integration

The inventory system publishes these events:

```csharp
// Item events
EventSystem.Publish("ItemAddedToInventory", item);    // InventoryItem
EventSystem.Publish("ItemRemovedFromInventory", item); // InventoryItem

// Inventory state events
EventSystem.Publish("InventoryChanged", this);         // InventoryManager
EventSystem.Publish("InventoryFull", this);            // InventoryManager
EventSystem.Publish("InventoryCleared", this);         // InventoryManager

// Save/load events (automatic)
EventSystem.Subscribe("GatheringSaveData", OnGatheringSaveData);
EventSystem.Subscribe("ApplyingSaveData", OnApplyingSaveData);
```

Subscribe to these events in your systems:

```csharp
void Start()
{
    EventSystem.Subscribe<InventoryItem>("ItemAddedToInventory", OnItemAdded);
    EventSystem.Subscribe<InventoryManager>("InventoryFull", OnInventoryFull);
}

void OnDestroy()
{
    EventSystem.Unsubscribe<InventoryItem>("ItemAddedToInventory", OnItemAdded);
    EventSystem.Unsubscribe<InventoryManager>("InventoryFull", OnInventoryFull);
}

void OnItemAdded(InventoryItem item)
{
    Debug.Log($"Item added: {item.ItemName}");
    UpdateQuestProgress("collect_fish", 1);
}

void OnInventoryFull(InventoryManager manager)
{
    Debug.Log("Inventory is full!");
    ShowWarningMessage();
}
```

## Creating Fish Shapes

When adding new fish species, define appropriate shapes:

```csharp
// In your fish database or scriptable object
public class FishDatabase : MonoBehaviour
{
    void CreateFishSpecies()
    {
        // Small fish (common)
        Fish smallFish = new Fish
        {
            id = "sardine",
            name = "Sardine",
            rarity = FishRarity.Common,
            inventorySize = new Vector2Int(1, 2) // Small bar
        };

        // Medium fish (uncommon)
        Fish mediumFish = new Fish
        {
            id = "trout",
            name = "Rainbow Trout",
            rarity = FishRarity.Uncommon,
            inventorySize = new Vector2Int(2, 3) // Medium rectangle
        };

        // Large fish (rare)
        Fish largeFish = new Fish
        {
            id = "tuna",
            name = "Bluefin Tuna",
            rarity = FishRarity.Rare,
            inventorySize = new Vector2Int(3, 4) // Large rectangle
        };

        // Eel (special shape)
        Fish eel = new Fish
        {
            id = "eel",
            name = "Electric Eel",
            rarity = FishRarity.Uncommon,
            inventorySize = new Vector2Int(1, 5) // Very long
        };

        // Aberrant fish (irregular shape)
        Fish aberrant = new Fish
        {
            id = "void_bass",
            name = "Void Bass",
            rarity = FishRarity.Aberrant,
            isAberrant = true,
            inventorySize = new Vector2Int(3, 3) // Will use Aberrant() shape
        };
    }
}
```

## Upgrading Inventory Capacity

For ship upgrades that increase storage:

```csharp
public class ShipUpgradeManager : MonoBehaviour
{
    public void UpgradeCargoHold(int newWidth, int newHeight)
    {
        // Save current items
        List<InventoryItem> currentItems = InventoryManager.Instance.GetAllItems();

        // Resize grid (this clears it)
        InventoryManager.Instance.MainGrid.Resize(newWidth, newHeight);

        // Try to re-add all items
        int failedItems = 0;
        foreach (var item in currentItems)
        {
            bool added = InventoryManager.Instance.AddItem(item);
            if (!added)
            {
                failedItems++;
                Debug.LogWarning($"Could not re-add {item.ItemName} after resize");
            }
        }

        if (failedItems > 0)
        {
            Debug.LogWarning($"{failedItems} items couldn't fit in new grid");
            // Could offer to store overflow in a temporary chest or drop items
        }

        Debug.Log($"Cargo hold upgraded to {newWidth}x{newHeight}!");
    }
}
```

## Custom Item Types

To create non-fish inventory items:

```csharp
// 1. Create a class implementing IInventoryItem
public class BaitItem : MonoBehaviour, IInventoryItem
{
    [SerializeField] private string _id;
    [SerializeField] private string _name;
    [SerializeField] private Sprite _icon;
    [SerializeField] private float _value;

    public string ItemID => _id;
    public string ItemName => _name;
    public int Width => 1;
    public int Height => 1;
    public Sprite Icon => _icon;

    public string GetDescription()
    {
        return "Fresh bait for catching fish.";
    }

    public float GetValue() => _value;
    public bool CanRotate() => false; // Bait doesn't need rotation
    public bool CanStackWith(IInventoryItem other)
    {
        // Bait can stack with same type
        return other.ItemID == this.ItemID;
    }
}

// 2. Create InventoryItem wrapper
BaitItem bait = new BaitItem();
ItemShape shape = ItemShape.Square1x1();
InventoryItem item = new InventoryItem(bait, shape);

// 3. Add to inventory
InventoryManager.Instance.AddItem(item);
```

## Optimization Hints for Players

Show packing suggestions to players:

```csharp
public class InventoryOptimizationUI : MonoBehaviour
{
    public void ShowPackingTips()
    {
        var stats = InventoryManager.Instance.GetEfficiencyStats();
        string suggestion = InventoryManager.Instance.GetReorganizationSuggestion();

        string message = $"Storage Efficiency: {stats.utilizationPercent:F1}%\n";
        message += $"Fragmentation: {stats.fragmentationScore:F2}\n\n";
        message += suggestion;

        // Display in UI
        tooltipText.text = message;
    }

    public void HighlightOptimalPlacement(InventoryItem item)
    {
        var hint = InventoryManager.Instance.GetOptimalPlacement(item);

        Debug.Log($"Best position: ({hint.gridX}, {hint.gridY})");
        Debug.Log($"Rotation: {hint.rotation}°");
        Debug.Log($"Score: {hint.efficiencyScore:F2}");
        Debug.Log($"Reason: {hint.reason}");

        // Could highlight cells in UI
        InventoryUI.Instance.ShowOptimalPlacement(item);
    }
}
```

## Debugging

### Enable Debug Logging

```csharp
void Start()
{
    // Enable EventSystem debug logging
    EventSystem.SetDebugLogging(true);

    // Print inventory state
    InventoryManager.Instance.DebugPrintInventory();
}
```

### Test Scenarios

```csharp
// Test 1: Fill inventory with random fish
[ContextMenu("Fill Inventory")]
void TestFillInventory()
{
    for (int i = 0; i < 30; i++)
    {
        Fish fish = CreateRandomFish();
        bool added = InventoryManager.Instance.AddFish(fish);
        Debug.Log($"Fish {i}: {(added ? "Added" : "Failed - inventory full")}");
    }
}

// Test 2: Save and load
[ContextMenu("Test Save/Load")]
void TestSaveLoad()
{
    // Add some items
    TestFillInventory();

    // Save
    SaveManager.Instance.SaveGame();

    // Clear
    InventoryManager.Instance.ClearInventory();
    Debug.Log("Inventory cleared");

    // Load
    SaveManager.Instance.LoadGame();
    Debug.Log("Inventory restored");

    // Verify
    InventoryManager.Instance.DebugPrintInventory();
}

// Test 3: Efficiency
[ContextMenu("Test Efficiency")]
void TestEfficiency()
{
    var stats = InventoryManager.Instance.GetEfficiencyStats();
    Debug.Log("=== EFFICIENCY STATS ===");
    Debug.Log($"Total Cells: {stats.totalCells}");
    Debug.Log($"Occupied: {stats.occupiedCells}");
    Debug.Log($"Wasted: {stats.wastedCells}");
    Debug.Log($"Utilization: {stats.utilizationPercent:F1}%");
    Debug.Log($"Fragmentation: {stats.fragmentationScore:F2}");
    Debug.Log($"Largest Area: {stats.largestContiguousArea}");
}
```

## Common Pitfalls

### 1. Forgetting to Set inventorySize on Fish

```csharp
// BAD - inventorySize is Vector2Int.zero
Fish badFish = new Fish
{
    id = "fish_001",
    name = "Mystery Fish"
    // Missing inventorySize!
};

// GOOD - Always set inventorySize
Fish goodFish = new Fish
{
    id = "fish_001",
    name = "Bass",
    inventorySize = new Vector2Int(2, 3) // Required!
};
```

### 2. Not Refreshing UI After Changes

```csharp
// If you manually modify the grid, refresh the UI:
InventoryManager.Instance.AddItem(item);
InventoryUI.Instance.RefreshGrid(); // Update visuals
```

### 3. Forgetting to Unsubscribe from Events

```csharp
void OnDestroy()
{
    // ALWAYS unsubscribe to prevent memory leaks!
    if (InventoryManager.Instance != null)
    {
        InventoryManager.Instance.OnInventoryChanged -= RefreshUI;
    }

    EventSystem.Unsubscribe<InventoryItem>("ItemAdded", OnItemAdded);
}
```

## Performance Tips

1. **Batch Operations**: When adding/removing many items, disable events temporarily:
   ```csharp
   // Not implemented yet, but could be added:
   // InventoryManager.Instance.SuspendEvents();
   // ... add many items ...
   // InventoryManager.Instance.ResumeEvents();
   ```

2. **Cache References**: Don't call `InventoryManager.Instance` repeatedly
   ```csharp
   // BAD
   for (int i = 0; i < 100; i++)
   {
       InventoryManager.Instance.AddItem(items[i]);
   }

   // GOOD
   var inventory = InventoryManager.Instance;
   for (int i = 0; i < 100; i++)
   {
       inventory.AddItem(items[i]);
   }
   ```

3. **Optimize Grid Size**: Larger grids = more cells to check
   - 10x10 = 100 cells (fast)
   - 15x15 = 225 cells (still fast)
   - 20x20 = 400 cells (slower)

## Next Steps

1. ✅ Read the main README.md for detailed API documentation
2. ✅ Set up InventoryManager GameObject in your scene
3. ✅ Create UI prefabs for GridCell, Cooler, Equipment
4. ✅ Test adding fish to inventory
5. ✅ Integrate with fishing system (Agent 5)
6. ✅ Test save/load functionality
7. ✅ Add fish shape definitions to your fish database
8. ✅ Implement ship upgrades for inventory expansion
9. ✅ Polish UI with animations and sounds

## Support

If you encounter issues:
1. Check the console for error messages
2. Call `InventoryManager.Instance.DebugPrintInventory()`
3. Verify all references are set in Inspector
4. Check that fish have valid inventorySize values
5. Ensure EventSystem is initialized (Agent 1)
6. Post in #inventory-system Discord channel

---

**Happy fishing! May your inventory always have room for one more catch!** 🎣

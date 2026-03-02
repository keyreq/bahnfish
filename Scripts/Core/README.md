# Bahnfish Core Architecture

## Overview

This directory contains the foundational systems that power the Bahnfish game. These core components provide the base architecture that all other game systems build upon.

## Architecture Pattern

The core architecture follows these key design patterns:

- **Singleton Pattern**: GameManager and SaveManager use thread-safe singletons to ensure single instances
- **Observer Pattern**: EventSystem implements pub/sub for loosely-coupled communication
- **Interface Segregation**: Clean interfaces define contracts between systems
- **Dependency Inversion**: Systems depend on interfaces, not concrete implementations

## Core Components

### GameManager.cs

**Purpose**: Central game state manager and lifecycle controller

**Responsibilities**:
- Initializes all game systems in proper order
- Maintains current game state (time, weather, sanity, etc.)
- Manages game lifecycle (pause, resume, quit)
- Broadcasts game state changes via EventSystem

**Usage Example**:
```csharp
// Access singleton instance
GameManager.Instance.PauseGame();

// Get current game state
GameState state = GameManager.Instance.CurrentGameState;

// Update game state
state.sanity -= 10f;
GameManager.Instance.UpdateGameState(state);
```

**Key Events**:
- `GameInitialized` - Fired when GameManager finishes initialization
- `GameStateUpdated` - Fired when game state changes
- `GamePaused` - Fired when game is paused/unpaused

### EventSystem.cs

**Purpose**: Centralized publish-subscribe event bus for inter-system communication

**Responsibilities**:
- Allows systems to subscribe to events without direct dependencies
- Publishes events with type-safe data
- Manages subscription lifecycle (subscribe/unsubscribe)
- Provides error handling for event callbacks

**Usage Example**:
```csharp
// Subscribe to an event
EventSystem.Subscribe<TimeOfDay>("TimeOfDayChanged", OnTimeChanged);

// Publish an event
EventSystem.Publish("FishCaught", caughtFish);

// Unsubscribe (important for cleanup!)
EventSystem.Unsubscribe<TimeOfDay>("TimeOfDayChanged", OnTimeChanged);

// Callback method
private void OnTimeChanged(TimeOfDay newTime)
{
    Debug.Log($"Time changed to: {newTime}");
}
```

**Important Notes**:
- Always unsubscribe in `OnDestroy()` to prevent memory leaks
- Event names should be consistent across the codebase
- Use typed events (`Publish<T>`) when passing data
- Enable debug logging with `EventSystem.SetDebugLogging(true)` for troubleshooting

### DataTypes.cs

**Purpose**: Shared data structures and enums used across all systems

**Key Types**:

#### Enumerations
- `TimeOfDay`: Day, Dusk, Night, Dawn
- `WeatherType`: Clear, Rain, Storm, Fog
- `FishRarity`: Common, Uncommon, Rare, Legendary, Aberrant

#### Classes
- `Fish`: Complete fish data including stats, rarity, and behavior
- `GameState`: Current game world and player state snapshot
- `Location`: Fishing location data and unlock status

**Usage Example**:
```csharp
// Create a new fish
Fish bass = new Fish
{
    id = "bass_001",
    name = "Largemouth Bass",
    rarity = FishRarity.Common,
    baseValue = 25f,
    inventorySize = new Vector2Int(2, 1)
};

// Check current time of day
if (gameState.timeOfDay == TimeOfDay.Night)
{
    // Spawn rare fish
}
```

## Interfaces

All interface definitions are in `Scripts/Interfaces/`. These define contracts that systems must implement:

### IFishable
For objects that can perform fishing (player, NPCs, etc.)

**Required Methods**:
- `StartFishing()` - Begin fishing action
- `StopFishing()` - End fishing action
- `IsFishing()` - Check current fishing state
- `GetCurrentTool()` - Get active fishing tool

### IInventoryItem
For items that can be stored in inventory (fish, equipment, consumables)

**Required Properties**:
- `ItemID`, `ItemName` - Identification
- `Width`, `Height` - Grid size
- `Icon` - Display sprite

**Required Methods**:
- `GetDescription()` - Tooltip text
- `GetValue()` - Sell price
- `CanRotate()` - Rotation support
- `CanStackWith()` - Stacking logic

### IUpgradeable
For systems that support upgrades (ship, equipment, tools)

**Required Properties**:
- `UpgradeID`, `UpgradeName` - Identification
- `CurrentLevel`, `MaxLevel` - Upgrade tiers

**Required Methods**:
- `CanUpgrade()` - Check if upgradeable
- `Upgrade()` - Perform upgrade
- `GetUpgradeCost()` - Cost to upgrade
- `GetUpgradeRequirements()` - Upgrade prerequisites
- `GetUpgradeDescription()` - Benefit description

### IInteractable
For world objects that can be interacted with (NPCs, docks, objects)

**Required Methods**:
- `Interact()` - Perform interaction
- `GetInteractionPrompt()` - Display prompt
- `CanInteract()` - Check if available
- `GetInteractionRange()` - Interaction distance
- `OnPlayerEnterRange()` - Highlight/show UI
- `OnPlayerExitRange()` - Remove highlight

### ISaveSystem
For save/load system implementations

**Required Methods**:
- `SaveGame()` - Write save to disk
- `LoadGame()` - Read save from disk
- `HasSaveData()` - Check save existence
- `DeleteSaveData()` - Remove save files
- `GetSavePath()` - Get save location
- `AutoSave()` - Automatic save

## Save System

### SaveManager.cs

**Purpose**: Handles all save/load operations with automatic backups

**Features**:
- JSON serialization for human-readable saves
- Automatic backup before each save
- Save data validation and corruption detection
- Auto-save on configurable interval (default: 5 minutes)
- Cloud save support (ready for Steam Cloud integration)

**Save Data Structure**:
```csharp
SaveData {
    // Meta
    string saveTimestamp
    string gameVersion

    // Game State
    Vector3 playerPosition
    float currentTime
    TimeOfDay timeOfDay
    WeatherType weather
    float sanity, money, fuel
    string currentLocationID

    // System Data (JSON strings)
    string inventoryData
    string progressionData
    string questData
    string upgradesData
}
```

**Integration with Other Systems**:

Other game systems can hook into save/load via events:

```csharp
// Subscribe to save/load events
EventSystem.Subscribe<SaveData>("GatheringSaveData", OnGatheringSave);
EventSystem.Subscribe<SaveData>("ApplyingSaveData", OnApplyingSave);

// When saving, add your data
private void OnGatheringSave(SaveData data)
{
    data.inventoryData = JsonUtility.ToJson(myInventoryData);
}

// When loading, restore your data
private void OnApplyingSave(SaveData data)
{
    myInventoryData = JsonUtility.FromJson<InventoryData>(data.inventoryData);
}
```

**Save Location**:
- Windows: `%AppData%\..\LocalLow\[CompanyName]\Bahnfish\savegame.json`
- Mac: `~/Library/Application Support/[CompanyName]/Bahnfish/savegame.json`
- Linux: `~/.config/unity3d/[CompanyName]/Bahnfish/savegame.json`

## Communication Flow

### System Initialization
```
Game Start
    ↓
GameManager.Awake()
    ↓
GameManager.Initialize()
    ↓
EventSystem.Publish("GameInitialized")
    ↓
All Systems Initialize
```

### Save Process
```
SaveManager.SaveGame()
    ↓
GatherSaveData()
    ↓
EventSystem.Publish("GatheringSaveData") → All systems add data
    ↓
Serialize to JSON
    ↓
Write to disk
    ↓
EventSystem.Publish("SaveComplete")
```

### Load Process
```
SaveManager.LoadGame()
    ↓
Read from disk
    ↓
Deserialize JSON
    ↓
ValidateSaveData()
    ↓
ApplySaveData()
    ↓
EventSystem.Publish("ApplyingSaveData") → All systems restore data
    ↓
EventSystem.Publish("LoadComplete")
```

## Event Reference

### Core Events

| Event Name | Data Type | Publisher | Description |
|------------|-----------|-----------|-------------|
| `GameInitialized` | `GameManager` | GameManager | Game finished initializing |
| `GameStateUpdated` | `GameState` | GameManager | Game state changed |
| `GamePaused` | `bool` | GameManager | Game paused (true) or resumed (false) |
| `GameQuitting` | `GameManager` | GameManager | Game is about to quit |
| `SaveComplete` | `SaveData` | SaveManager | Save operation finished |
| `LoadComplete` | `SaveData` | SaveManager | Load operation finished |
| `SaveFailed` | `string` | SaveManager | Save failed with error message |
| `LoadFailed` | `string` | SaveManager | Load failed with error message |
| `GatheringSaveData` | `SaveData` | SaveManager | Systems should add their save data |
| `ApplyingSaveData` | `SaveData` | SaveManager | Systems should restore from save data |
| `SaveDataDeleted` | `bool` | SaveManager | Save file was deleted |

## Best Practices

### For New Systems

1. **Always use EventSystem for cross-system communication**
   - Prevents tight coupling
   - Makes systems easier to test
   - Allows parallel development

2. **Subscribe to events in `Start()` or `OnEnable()`**
   - Don't subscribe in `Awake()` - timing issues may occur

3. **Always unsubscribe in `OnDestroy()` or `OnDisable()`**
   - Prevents memory leaks
   - Avoids null reference errors

4. **Implement save/load via events**
   - Listen for `GatheringSaveData` and `ApplyingSaveData`
   - Serialize your data to JSON strings
   - Store in the appropriate SaveData field

5. **Use interfaces to define contracts**
   - Makes systems swappable
   - Enables mock implementations for testing
   - Documents expected behavior

### Code Organization

```csharp
public class YourSystem : MonoBehaviour
{
    // Singleton pattern (if needed)
    private static YourSystem _instance;
    public static YourSystem Instance => _instance;

    private void Awake()
    {
        // Initialize singleton
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Subscribe to events
        EventSystem.Subscribe<TimeOfDay>("TimeOfDayChanged", OnTimeChanged);
        EventSystem.Subscribe<SaveData>("GatheringSaveData", OnGatheringSave);
        EventSystem.Subscribe<SaveData>("ApplyingSaveData", OnApplyingSave);
    }

    private void OnDestroy()
    {
        // CRITICAL: Always unsubscribe!
        EventSystem.Unsubscribe<TimeOfDay>("TimeOfDayChanged", OnTimeChanged);
        EventSystem.Unsubscribe<SaveData>("GatheringSaveData", OnGatheringSave);
        EventSystem.Unsubscribe<SaveData>("ApplyingSaveData", OnApplyingSave);
    }

    // Event handlers
    private void OnTimeChanged(TimeOfDay newTime) { }
    private void OnGatheringSave(SaveData data) { }
    private void OnApplyingSave(SaveData data) { }
}
```

## Testing

### Manual Testing

1. **Test GameManager initialization**:
   - Create empty scene
   - Verify GameManager auto-creates on play
   - Check console for initialization message

2. **Test EventSystem**:
   - Subscribe to test event
   - Publish test event
   - Verify callback is invoked
   - Unsubscribe and publish again
   - Verify callback is NOT invoked

3. **Test Save/Load**:
   - Play game and modify state (move player, change time)
   - Call `SaveManager.Instance.SaveGame()`
   - Check save file exists at `SaveManager.Instance.GetSavePath()`
   - Restart game
   - Call `SaveManager.Instance.LoadGame()`
   - Verify state was restored

### Automated Testing

Unit tests should be created for:
- EventSystem subscribe/unsubscribe/publish
- SaveManager save/load/validate
- DataTypes serialization/deserialization
- GameState cloning

## Integration with Other Agents

This core architecture provides the foundation for all other development agents:

- **Agent 2 (Input/Player)**: Uses EventSystem to publish movement events
- **Agent 3 (Time/Environment)**: Uses EventSystem for time/weather changes
- **Agent 5 (Fishing)**: Implements IFishable interface
- **Agent 6 (Inventory)**: Items implement IInventoryItem interface
- **Agent 7 (Sanity/Horror)**: Updates GameState.sanity, publishes events
- **Agent 8 (Fish AI)**: Uses Fish data structure
- **Agent 9 (Progression)**: Implements IUpgradeable for upgrades
- **All Agents**: Use EventSystem for communication and SaveManager for persistence

## Troubleshooting

### Common Issues

**Problem**: "NullReferenceException: GameManager.Instance is null"
- **Solution**: Ensure GameManager exists in scene or is created at runtime

**Problem**: "Event not firing"
- **Solution**: Check event name spelling, ensure subscription happened, verify publisher is calling Publish

**Problem**: "Memory leak / events firing after object destroyed"
- **Solution**: Always unsubscribe in OnDestroy()

**Problem**: "Save file corrupted"
- **Solution**: Delete save file or restore from backup (.backup extension)

**Problem**: "Multiple GameManagers in scene"
- **Solution**: Singleton pattern will destroy duplicates automatically

## Version History

- **v1.0** (Phase 1): Initial implementation
  - GameManager with basic state management
  - EventSystem with pub/sub
  - Core interfaces (IFishable, IInventoryItem, etc.)
  - DataTypes (Fish, GameState, enums)
  - SaveManager with JSON serialization

## Contact

For questions about core architecture, contact Agent 1 or post in the #core-systems channel.

---

**Remember**: This core architecture is designed to support parallel development. Keep interfaces stable, communicate breaking changes immediately, and use the EventSystem for all cross-system communication!

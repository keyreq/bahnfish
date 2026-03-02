# Save/Load System - Bahnfish

## Overview

The Save/Load System is a robust, production-ready implementation that handles all game persistence in Bahnfish. It provides JSON-based serialization, automatic backups, corruption detection, and cloud save support (stub).

## Architecture

### Components

1. **SaveManager.cs** - Main save/load logic and orchestration
2. **SaveData.cs** - Comprehensive data structure for all game state
3. **SaveValidator.cs** - Validation and corruption detection
4. **CloudSaveHandler.cs** - Cloud save integration (stub for future implementation)

## Features

### Core Features

- **JSON Serialization**: Uses Unity's JsonUtility for fast, compatible serialization
- **Singleton Pattern**: Easy access via `SaveManager.Instance`
- **Auto-Save**: Configurable interval (default: 5 minutes)
- **Backup System**: Keeps last 3 saves with timestamped filenames
- **Save Validation**: Comprehensive checks before save and load
- **Corruption Recovery**: Automatically falls back to valid backup if main save is corrupted
- **Event-Driven**: Loose coupling via EventSystem for easy integration
- **Safe Save Checking**: Won't save during critical operations or when paused

### SaveData Structure

The `SaveData` class contains all persistent game state:

#### Meta Information
- `saveVersion` - Save format version for migration
- `saveTimestamp` - When the save was created
- `gameVersion` - Game version for compatibility
- `totalPlayTime` - Accumulated playtime in seconds

#### Player State
- `playerPosition` - 3D position in world
- `playerRotation` - Player orientation

#### Time & Environment
- `currentTime` - 0-24 hour format
- `timeOfDay` - Day/Dusk/Night/Dawn enum
- `currentWeather` - Weather type enum

#### Resources
- `money` - Player currency
- `scrap` - Crafting resource
- `relics` - Special collectibles
- `fuel` - Boat fuel level
- `sanity` - Sanity meter (0-100)

#### Location
- `currentLocationID` - Current fishing location
- `unlockedLocations` - List of accessible locations

#### Inventory
- `inventoryItems` - Serialized inventory items
- `inventoryCapacity` - Maximum inventory size

#### Upgrades
- `purchasedUpgrades` - List of owned upgrades
- `upgradelevels` - Upgrade progression levels

#### Fish Collection
- `fishCaughtCount` - Count per species
- `discoveredFishSpecies` - Fish encyclopedia
- `fishRecordWeights` - Personal best weights

#### Quests
- `completedQuests` - Finished quests
- `activeQuests` - Current quests
- `questProgress` - Progress tracking

#### Statistics
- `totalFishCaught` - Lifetime fish count
- `totalMoneyEarned` - Lifetime earnings
- `nightsSurvived` - Nights at sea
- `lowestSanity` - Lowest sanity reached
- `deathCount` - Total deaths

## Usage

### Saving the Game

```csharp
// Manual save
SaveManager.Instance.SaveGame();

// Via event system
EventSystem.Publish("RequestSave", new SaveData());
```

### Loading the Game

```csharp
// Manual load
if (SaveManager.Instance.HasSaveData())
{
    SaveManager.Instance.LoadGame();
}

// Via event system
EventSystem.Subscribe("RequestLoad", () => {
    SaveManager.Instance.LoadGame();
});
```

### Auto-Save

Auto-save is enabled by default with a 5-minute interval. Configure in Unity Inspector:

- `enableAutoSave` - Toggle auto-save
- `autoSaveInterval` - Seconds between auto-saves

### Adding Your Data to Saves

Other systems can integrate with the save system via events:

```csharp
// Subscribe to save gathering event
void Start()
{
    EventSystem.Subscribe<SaveData>("GatheringSaveData", OnGatherSaveData);
    EventSystem.Subscribe<SaveData>("ApplyingSaveData", OnApplySaveData);
}

// Add your data when saving
void OnGatherSaveData(SaveData data)
{
    // Add your system's data to the SaveData object
    data.inventoryItems.Add(mySerializedItem);
}

// Load your data when loading
void OnApplySaveData(SaveData data)
{
    // Restore your system from SaveData
    foreach (var item in data.inventoryItems)
    {
        RestoreItem(item);
    }
}
```

## SaveValidator

The `SaveValidator` class performs comprehensive validation:

### Validation Checks

1. **Meta Data**: Version, timestamp, game version
2. **Player State**: Position/rotation validity, NaN checks
3. **Resources**: Range checks, negative value detection
4. **Location**: Valid location IDs
5. **Inventory**: Item integrity, quantity validation
6. **Fish Collection**: Count and weight validation
7. **Statistics**: Range and logic checks

### Using SaveValidator

```csharp
SaveValidator.ValidationResult result = SaveValidator.ValidateSaveData(data);

if (!result.IsValid)
{
    Debug.LogError($"Validation failed:\n{result.GetSummary()}");
    foreach (var error in result.Errors)
    {
        Debug.LogError($"  - {error}");
    }
}

if (result.Warnings.Count > 0)
{
    Debug.LogWarning("Validation warnings:");
    foreach (var warning in result.Warnings)
    {
        Debug.LogWarning($"  - {warning}");
    }
}
```

### Checksum Validation

Detect file tampering or corruption:

```csharp
string json = JsonUtility.ToJson(saveData);
string checksum = SaveValidator.CalculateChecksum(json);

// Later, verify integrity
bool isValid = SaveValidator.VerifyChecksum(json, checksum);
```

## Backup System

### How It Works

1. Before each save, the current save file is backed up
2. Backups are named with timestamps: `savegame_backup_20260301_143022.json`
3. Only the last 3 backups are kept (configurable via `maxBackups`)
4. Oldest backups are automatically deleted

### Backup Recovery

If the main save file is corrupted:

1. SaveValidator detects corruption
2. System tries each backup from newest to oldest
3. First valid backup is restored
4. Player is notified of recovery

### Manual Backup Management

```csharp
// Get backup information
FileInfo[] backups = SaveManager.Instance.GetBackupInfo();
foreach (var backup in backups)
{
    Debug.Log($"Backup: {backup.Name}, Created: {backup.CreationTime}");
}
```

## CloudSaveHandler (Stub)

The `CloudSaveHandler` provides a framework for future cloud save integration.

### Supported Platforms (Planned)

- Steam Cloud (via Steamworks.NET)
- Epic Games Cloud
- Xbox Live
- PlayStation Network
- Nintendo Network

### Current Status

All methods are stubbed and log to console. To implement:

1. Add platform SDK (e.g., Steamworks.NET)
2. Implement platform-specific methods in `InitializeSteamCloud()`, etc.
3. Add actual upload/download logic in `UploadToCloud()` and `DownloadFromCloud()`
4. Implement conflict resolution in `ResolveConflict()`

### Future Usage

```csharp
// Enable cloud saves
CloudSaveHandler.Instance.SetCloudSavesEnabled(true);

// Manual cloud sync
CloudSaveHandler.Instance.UploadToCloud(savePath);
CloudSaveHandler.Instance.DownloadFromCloud(savePath);

// Check for newer cloud save
if (CloudSaveHandler.Instance.HasNewerCloudSave())
{
    // Resolve conflict or download
}
```

## Events

The SaveSystem publishes and subscribes to these events:

### Published Events

| Event Name | Data Type | Description |
|------------|-----------|-------------|
| `SaveComplete` | `SaveData` | Fired after successful save |
| `OnSaveComplete` | `SaveData` | Alternative event name |
| `SaveFailed` | `string` | Fired when save fails (error message) |
| `LoadComplete` | `SaveData` | Fired after successful load |
| `OnLoadComplete` | `SaveData` | Alternative event name |
| `LoadFailed` | `string` | Fired when load fails (error message) |
| `SaveDataDeleted` | `bool` | Fired after save deletion (success/failure) |
| `GatheringSaveData` | `SaveData` | Request other systems to add data |
| `ApplyingSaveData` | `SaveData` | Request other systems to load data |
| `CheckSafeToSave` | `bool` | Query if it's safe to save |
| `CloudUploadComplete` | `SyncStatus` | Cloud upload finished |
| `CloudDownloadComplete` | `SyncStatus` | Cloud download finished |

### Subscribed Events

| Event Name | Data Type | Description |
|------------|-----------|-------------|
| `RequestSave` | `SaveData` | Trigger a save operation |
| `RequestLoad` | - | Trigger a load operation |

## Configuration

All settings are configurable in the Unity Inspector:

### Save Settings
- `saveFileName` - Name of save file (default: "savegame.json")
- `prettyPrint` - Format JSON for readability (default: true)
- `maxBackups` - Number of backups to keep (default: 3)

### Auto-Save
- `enableAutoSave` - Enable automatic saving (default: true)
- `autoSaveInterval` - Seconds between saves (default: 300)

### Validation
- `validateOnSave` - Validate before saving (default: true)
- `validateOnLoad` - Validate after loading (default: true)

### Cloud Integration
- `syncWithCloud` - Enable cloud sync (default: false)

### Debug
- `enableDebugLogging` - Log save/load operations (default: true)

## File Locations

### Save Files
- **Path**: `Application.persistentDataPath/savegame.json`
- **Windows**: `C:\Users\<username>\AppData\LocalLow\<company>\<game>\savegame.json`
- **Mac**: `~/Library/Application Support/<company>/<game>/savegame.json`
- **Linux**: `~/.config/unity3d/<company>/<game>/savegame.json`

### Backup Files
Same directory as main save, with pattern: `savegame_backup_YYYYMMDD_HHMMSS.json`

## SerializedItem Class

Represents items in inventory with grid-based placement:

```csharp
SerializedItem item = new SerializedItem("fish_bass", "Bass", "fish");
item.fishWeight = 2.5f;
item.fishSpecies = "largemouth_bass";
item.rarity = FishRarity.Common;
item.value = 15f;
item.gridX = 2;
item.gridY = 1;
item.width = 2;
item.height = 1;
```

## Error Handling

The system handles errors gracefully:

1. **Save Failures**: Logs error, publishes `SaveFailed` event
2. **Load Failures**: Attempts backup restoration, publishes `LoadFailed` if all fail
3. **Validation Failures**: Detailed error messages with line-by-line issues
4. **Corruption**: Automatic backup restoration with user notification

## Testing

### Manual Testing

1. Start game and play for a bit
2. Call `SaveManager.Instance.SaveGame()`
3. Verify save file exists at `GetSavePath()`
4. Check backup files were created
5. Quit game and restart
6. Call `SaveManager.Instance.LoadGame()`
7. Verify game state restored

### Corruption Testing

1. Save game normally
2. Manually corrupt save file (edit JSON, add garbage)
3. Try to load
4. System should restore from backup automatically

### Auto-Save Testing

1. Enable auto-save with short interval (e.g., 30 seconds)
2. Play game
3. Watch console for auto-save messages
4. Verify saves are being created

## Integration with Other Agents

### Agent Dependencies

- **Agent 1 (Core Architecture)**: Uses ISaveSystem interface, EventSystem, GameManager
- **Agent 6 (Inventory)**: Provides SerializedItem for inventory serialization
- **Agent 8 (Fish AI)**: Saves fish collection data
- **Agent 9 (Progression)**: Saves upgrades and economy data
- **Agent 10 (Quest/Narrative)**: Saves quest state

### Adding New Save Data

When other agents need to save data:

1. Add fields to `SaveData.cs`
2. Subscribe to `GatheringSaveData` event
3. Subscribe to `ApplyingSaveData` event
4. Update `SaveValidator.cs` with new validation rules

## Best Practices

1. **Always validate saves**: Keep validation enabled in production
2. **Keep backups**: Never set `maxBackups` to 0
3. **Test corruption recovery**: Manually corrupt saves during testing
4. **Use events for loose coupling**: Don't directly depend on SaveManager
5. **Log save operations**: Keep debug logging enabled during development
6. **Auto-save at safe points**: Don't save during combat or critical operations
7. **Version your saves**: Update `saveVersion` when format changes
8. **Test save migration**: When changing SaveData structure, test loading old saves

## Troubleshooting

### Save Not Working

1. Check Unity Console for errors
2. Verify `Application.persistentDataPath` is writable
3. Check if disk is full
4. Verify validation isn't rejecting data

### Load Failing

1. Check if save file exists at `GetSavePath()`
2. Verify JSON is valid (open in text editor)
3. Check validation errors in console
4. Try restoring from backup manually

### Auto-Save Not Triggering

1. Verify `enableAutoSave` is true
2. Check `autoSaveInterval` setting
3. Ensure game isn't paused (`Time.timeScale != 0`)
4. Check console for "Skipping auto-save" messages

## Future Enhancements

- [ ] Implement actual cloud save integrations
- [ ] Add save file encryption
- [ ] Implement save file compression
- [ ] Add multiple save slots
- [ ] Create save file browser UI
- [ ] Add save file export/import
- [ ] Implement automatic save migration system
- [ ] Add save statistics dashboard

## License

Part of the Bahnfish game project. All rights reserved.

---

**Agent 4: Save/Load System - Implementation Complete**

Created by: Agent 4 (Save/Load System Agent)
Last Updated: 2026-03-01
Status: Production Ready

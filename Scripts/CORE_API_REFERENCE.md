# Bahnfish Core API - Quick Reference

## GameManager API

```csharp
// Access singleton
GameManager.Instance

// Get current game state
GameState state = GameManager.Instance.CurrentGameState;

// Update game state
state.sanity -= 10f;
GameManager.Instance.UpdateGameState(state);

// Game control
GameManager.Instance.PauseGame();
GameManager.Instance.ResumeGame();
GameManager.Instance.QuitGame();
```

## EventSystem API

```csharp
// Subscribe with data
EventSystem.Subscribe<TimeOfDay>("TimeChanged", OnTimeChanged);

// Subscribe without data
EventSystem.Subscribe("PlayerDied", OnPlayerDied);

// Publish with data
EventSystem.Publish("TimeChanged", TimeOfDay.Night);

// Publish without data
EventSystem.Publish("PlayerDied");

// Unsubscribe (ALWAYS DO THIS IN OnDestroy!)
EventSystem.Unsubscribe<TimeOfDay>("TimeChanged", OnTimeChanged);
EventSystem.Unsubscribe("PlayerDied", OnPlayerDied);

// Utility
int count = EventSystem.GetSubscriberCount("TimeChanged");
EventSystem.SetDebugLogging(true);
```

## SaveManager API

```csharp
// Save/Load
SaveManager.Instance.SaveGame();
SaveManager.Instance.LoadGame();
SaveManager.Instance.AutoSave();

// Check save status
bool hasSave = SaveManager.Instance.HasSaveData();
string path = SaveManager.Instance.GetSavePath();

// Delete save
SaveManager.Instance.DeleteSaveData();

// Integrate your system with save/load
EventSystem.Subscribe<SaveData>("GatheringSaveData", OnGatherSave);
EventSystem.Subscribe<SaveData>("ApplyingSaveData", OnApplySave);

private void OnGatherSave(SaveData data)
{
    data.inventoryData = JsonUtility.ToJson(myData);
}

private void OnApplySave(SaveData data)
{
    myData = JsonUtility.FromJson<MyData>(data.inventoryData);
}
```

## Core Data Types

```csharp
// Enums
TimeOfDay.Day | Dusk | Night | Dawn
WeatherType.Clear | Rain | Storm | Fog
FishRarity.Common | Uncommon | Rare | Legendary | Aberrant

// Fish
Fish bass = new Fish
{
    id = "bass_001",
    name = "Largemouth Bass",
    rarity = FishRarity.Common,
    baseValue = 25f,
    inventorySize = new Vector2Int(2, 1),
    preferredTime = TimeOfDay.Day,
    weight = 2.5f,
    length = 45f
};
float sellPrice = bass.GetSellValue(1.0f);

// GameState
GameState state = new GameState();
state.currentTime = 14.5f;  // 2:30 PM
state.timeOfDay = TimeOfDay.Day;
state.weather = WeatherType.Clear;
state.sanity = 80f;
state.money = 500f;
state.fuel = 75f;
state.playerPosition = new Vector3(10, 0, 20);
state.currentLocationID = "deep_ocean";
GameState clone = state.Clone();

// Location
Location lake = new Location
{
    id = "starter_lake",
    name = "Peaceful Lake",
    description = "A calm lake perfect for beginners",
    isUnlocked = true,
    unlockCost = 0f,
    worldPosition = new Vector3(0, 0, 0),
    sceneName = "StarterLake"
};
```

## Interfaces

### IFishable
```csharp
public class FishingController : MonoBehaviour, IFishable
{
    public void StartFishing() { /* ... */ }
    public void StopFishing() { /* ... */ }
    public bool IsFishing() { return isFishing; }
    public object GetCurrentTool() { return currentRod; }
}
```

### IInventoryItem
```csharp
public class FishItem : IInventoryItem
{
    public string ItemID { get; private set; }
    public string ItemName { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }
    public Sprite Icon { get; private set; }

    public string GetDescription() { return "A fresh fish"; }
    public float GetValue() { return baseValue; }
    public bool CanRotate() { return true; }
    public bool CanStackWith(IInventoryItem other) { return false; }
}
```

### IUpgradeable
```csharp
public class BoatEngine : MonoBehaviour, IUpgradeable
{
    public string UpgradeID => "engine";
    public string UpgradeName => "Boat Engine";
    public int CurrentLevel { get; private set; }
    public int MaxLevel => 5;

    public bool CanUpgrade() { return CurrentLevel < MaxLevel; }
    public bool Upgrade() { /* ... */ }
    public float GetUpgradeCost() { return baseCost * CurrentLevel; }
    public string GetUpgradeRequirements() { return "500 coins"; }
    public string GetUpgradeDescription() { return "+10% speed"; }
}
```

### IInteractable
```csharp
public class Dock : MonoBehaviour, IInteractable
{
    public void Interact() { /* Open shop UI */ }
    public string GetInteractionPrompt() { return "Press E to dock"; }
    public bool CanInteract() { return !GameManager.Instance.CurrentGameState.isFishing; }
    public float GetInteractionRange() { return 5f; }
    public void OnPlayerEnterRange() { /* Show prompt */ }
    public void OnPlayerExitRange() { /* Hide prompt */ }
}
```

### ISaveSystem
```csharp
public class CustomSaveSystem : ISaveSystem
{
    public void SaveGame() { /* ... */ }
    public void LoadGame() { /* ... */ }
    public bool HasSaveData() { return File.Exists(savePath); }
    public void DeleteSaveData() { File.Delete(savePath); }
    public string GetSavePath() { return savePath; }
    public void AutoSave() { SaveGame(); }
}
```

## Common Events

### Core Events
```csharp
// Game lifecycle
EventSystem.Subscribe<GameManager>("GameInitialized", OnGameInit);
EventSystem.Subscribe<GameState>("GameStateUpdated", OnStateUpdate);
EventSystem.Subscribe<bool>("GamePaused", OnPause);

// Save/Load
EventSystem.Subscribe<SaveData>("SaveComplete", OnSaveComplete);
EventSystem.Subscribe<SaveData>("LoadComplete", OnLoadComplete);
EventSystem.Subscribe<string>("SaveFailed", OnSaveFailed);

// Save integration
EventSystem.Subscribe<SaveData>("GatheringSaveData", OnGatherSave);
EventSystem.Subscribe<SaveData>("ApplyingSaveData", OnApplySave);
```

### Agent-Specific Events (To Be Published)
```csharp
// Agent 2: Player
EventSystem.Publish("PlayerMoved", position);
EventSystem.Publish("PlayerInteracted", interactable);

// Agent 3: Time/Environment
EventSystem.Publish("TimeOfDayChanged", TimeOfDay.Night);
EventSystem.Publish("WeatherChanged", WeatherType.Storm);

// Agent 5: Fishing
EventSystem.Publish("FishCaught", fish);
EventSystem.Publish("LineBroken", null);

// Agent 6: Inventory
EventSystem.Publish("InventoryFull", null);
EventSystem.Publish("ItemAdded", item);

// Agent 7: Sanity
EventSystem.Publish("SanityChanged", 50f);
EventSystem.Publish("InsanityTriggered", null);

// Agent 8: Fish AI
EventSystem.Publish("RareFishSpawned", fish);

// Agent 9: Progression
EventSystem.Publish("UpgradePurchased", upgrade);
EventSystem.Publish("LocationUnlocked", location);

// Agent 10: Narrative
EventSystem.Publish("QuestStarted", quest);
EventSystem.Publish("QuestCompleted", quest);
```

## Best Practices

### System Template
```csharp
using UnityEngine;

public class YourSystem : MonoBehaviour
{
    // Singleton (if needed)
    private static YourSystem _instance;
    public static YourSystem Instance => _instance;

    private void Awake()
    {
        // Singleton setup
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
        EventSystem.Subscribe<SaveData>("GatheringSaveData", OnGatherSave);
        EventSystem.Subscribe<SaveData>("ApplyingSaveData", OnApplySave);
    }

    private void OnDestroy()
    {
        // CRITICAL: Always unsubscribe!
        EventSystem.Unsubscribe<TimeOfDay>("TimeOfDayChanged", OnTimeChanged);
        EventSystem.Unsubscribe<SaveData>("GatheringSaveData", OnGatherSave);
        EventSystem.Unsubscribe<SaveData>("ApplyingSaveData", OnApplySave);
    }

    // Event handlers
    private void OnTimeChanged(TimeOfDay newTime)
    {
        Debug.Log($"Time changed to: {newTime}");
    }

    private void OnGatherSave(SaveData data)
    {
        // Add your data to save
        MyData myData = new MyData();
        data.inventoryData = JsonUtility.ToJson(myData);
    }

    private void OnApplySave(SaveData data)
    {
        // Restore your data from save
        MyData myData = JsonUtility.FromJson<MyData>(data.inventoryData);
    }
}
```

### Common Patterns

**Update GameState**
```csharp
GameState state = GameManager.Instance.CurrentGameState;
state.sanity = Mathf.Clamp(state.sanity - 0.1f, 0f, 100f);
GameManager.Instance.UpdateGameState(state);
```

**Publish Event**
```csharp
EventSystem.Publish("FishCaught", caughtFish);
```

**Subscribe to Event**
```csharp
// In Start()
EventSystem.Subscribe<Fish>("FishCaught", OnFishCaught);

// Handler
private void OnFishCaught(Fish fish)
{
    Debug.Log($"Caught: {fish.name}");
}

// In OnDestroy()
EventSystem.Unsubscribe<Fish>("FishCaught", OnFishCaught);
```

**Save Integration**
```csharp
[System.Serializable]
private class MySystemData
{
    public int level;
    public float score;
}

private void OnGatherSave(SaveData data)
{
    MySystemData myData = new MySystemData
    {
        level = currentLevel,
        score = currentScore
    };
    data.inventoryData = JsonUtility.ToJson(myData);
}

private void OnApplySave(SaveData data)
{
    if (!string.IsNullOrEmpty(data.inventoryData))
    {
        MySystemData myData = JsonUtility.FromJson<MySystemData>(data.inventoryData);
        currentLevel = myData.level;
        currentScore = myData.score;
    }
}
```

## Troubleshooting

**Problem**: Event not firing
- Check event name spelling (case-sensitive)
- Verify subscription happened (put debug log in Subscribe call)
- Ensure publisher is calling Publish

**Problem**: NullReferenceException on GameManager.Instance
- GameManager might not exist in scene
- Make sure you're calling it after Awake

**Problem**: Memory leak
- Always unsubscribe in OnDestroy()
- Use EventSystem.GetSubscriberCount() to verify cleanup

**Problem**: Save data not persisting
- Check SaveManager.Instance.GetSavePath() - does directory exist?
- Look for errors in console during SaveGame()
- Try SaveManager.Instance.HasSaveData() to verify save exists

**Problem**: Multiple GameManagers
- Singleton pattern will auto-destroy duplicates
- Check console for warnings

## File Locations

All core files are in: `C:\Users\larry\bahnfish\Scripts\`

- Core: `Scripts/Core/`
- Interfaces: `Scripts/Interfaces/`
- Save System: `Scripts/SaveSystem/`
- Documentation: `Scripts/Core/README.md`

## Support

- Full documentation: `Scripts/Core/README.md`
- Implementation guide: `AGENT_1_IMPLEMENTATION_COMPLETE.md`
- File list: `Scripts/AGENT_1_FILES.txt`

---

**Quick Reference Version 1.0** | Agent 1: Core Architecture | 2026-03-01

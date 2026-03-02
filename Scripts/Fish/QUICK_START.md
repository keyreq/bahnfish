# Fish System - Quick Start Guide

## Setup (5 minutes)

### 1. Generate Fish Species (1 minute)
```
Unity Editor:
1. Create empty GameObject
2. Add Component → FishSpeciesGenerator
3. Right-click component → "Generate All Fish Species"
4. Wait for "Generation Complete!" message
5. Check Assets/Resources/FishSpecies/ folder (60 fish created)
```

### 2. Add Core Systems to Scene (2 minutes)
```
Unity Hierarchy:
1. Create empty GameObject "FishManager"
   - Add Component → FishManager
   - Check DontDestroyOnLoad

2. Create empty GameObject "FishDatabase"
   - Add Component → FishDatabase
   - Check DontDestroyOnLoad
```

### 3. Add Spawners (2 minutes)
```
At each fishing location:
1. Create empty GameObject "FishSpawner_[LocationName]"
2. Add Component → FishSpawner
3. Set locationID (e.g., "starter_lake")
4. Adjust spawn settings if needed
5. Spawner will auto-generate spawn points
```

### 4. Test
```
1. Enter Play Mode
2. Fish spawn automatically every 5-10 seconds
3. Use Context Menu for testing:
   - FishManager → Spawn Test Fish
   - FishManager → Print Fish Stats
   - FishDatabase → Print Database Stats
```

---

## Integration Snippets

### Agent 3 (Time/Environment)
```csharp
// When time changes
FishManager.Instance.UpdateTime(TimeOfDay.Night);

// When weather changes
FishManager.Instance.UpdateWeather(WeatherType.Storm);
```

### Agent 5 (Fishing)
```csharp
// Get nearby fish
List<GameObject> fish = FishManager.Instance.GetFishInArea(
    playerPosition,
    radius: 10f
);

// Try to hook a fish
FishAI fishAI = fish[0].GetComponent<FishAI>();
if (fishAI.TryBite(BaitType.Lures))
{
    fishAI.OnHooked();
    StartFishingMinigame(fishAI);
}
```

### Agent 6 (Inventory)
```csharp
// Get fish data
Fish fishData = fishAI.GetFishData();
Vector2Int size = fishData.inventorySize;
float value = fishData.baseValue;
```

---

## Common Tasks

### Spawn a specific fish
```csharp
FishSpeciesData bass = FishDatabase.Instance.GetFishByID("largemouth_bass");
GameObject fishObj = FishManager.Instance.SpawnFish(position, "starter_lake");
```

### Get all rare fish
```csharp
List<FishSpeciesData> rareFish = FishDatabase.Instance.GetFishByRarity(FishRarity.Rare);
```

### Check spawn rate
```csharp
float currentRate = FishManager.Instance.GetCurrentSpawnRate();
Debug.Log($"Current spawn rate: {currentRate}x");
```

### Subscribe to rare fish events
```csharp
FishManager.Instance.OnRareFishSpawned += (fish) =>
{
    Debug.Log($"RARE: {fish.name} spawned!");
    UIManager.ShowNotification($"Rare fish: {fish.name}");
};
```

---

## Debug Tools

### FishManager Context Menu
- **Print Fish Stats**: Shows active fish count and spawn rate
- **Spawn Test Fish**: Spawns fish at player position
- **Clear All Fish**: Removes all active fish

### FishDatabase Context Menu
- **Print Database Stats**: Shows fish counts by rarity
- **Reload Database**: Reloads fish from Resources

### Gizmos (Scene View)
- **FishSpawner**: Cyan = spawn radius, Yellow = player range, Green = spawn points
- **FishAI**: Yellow = bait detection, Red = player detection, Green = wander target

---

## Fish Species Quick Reference

### Common (20) - $5-20
Largemouth Bass, Bluegill, Rainbow Trout, Catfish, Perch, Crappie, Sunfish, Carp, etc.

### Uncommon (15) - $40-70
Northern Pike, Walleye, Muskie, Salmon, Steelhead, Striped Bass, Tarpon, Tuna, etc.

### Rare (10) - $145-220
Sturgeon, Blue Marlin, Swordfish, Bluefin Tuna, Sailfish, Arapaima, Giant Grouper, etc.

### Legendary (5) - $450-520
Ancient Leviathan, Crimson Titan, Abyssal Maw, Celestial Dragon, Void Emperor

### Aberrant (10) - $80-200
Aberrant Bass, Corrupted Pike, Phantom Trout, Void Carp, Twisted Salmon, etc.

---

## Spawn Rate Cheat Sheet

| Time + Weather | Spawn Rate | Rare Fish Chance |
|----------------|------------|------------------|
| Day + Clear    | 1.0×       | Normal           |
| Day + Rain     | 1.5×       | +50%             |
| Dusk + Rain    | 2.25×      | +125%            |
| Night + Clear  | 2.0×       | +100%            |
| Night + Storm  | 5.0×       | +1150% (!!)      |

---

## Troubleshooting

**No fish spawning?**
- Check FishManager is in scene
- Check FishDatabase is in scene
- Check FishSpawner has autoSpawn = true
- Check player is within playerProximityRange (50m)
- Check maxActiveFish not reached (50 default)

**Fish not loading?**
- Run FishSpeciesGenerator first
- Check Assets/Resources/FishSpecies/ exists
- Check 60 fish assets created
- Use FishDatabase context menu → "Reload Database"

**Performance issues?**
- Reduce maxActiveFish in FishManager
- Increase despawnDistance
- Disable schooling in NormalBehavior
- Reduce number of active FishSpawners

---

## File Locations

```
Scripts/Fish/
  - FishManager.cs
  - FishDatabase.cs
  - FishSpawner.cs
  - FishAI.cs
  - FishSpeciesData.cs
  - FishSpeciesGenerator.cs
  - FishBehavior/
    - NormalBehavior.cs
    - AberrantBehavior.cs
    - LegendaryBehavior.cs

Assets/Resources/FishSpecies/
  - (60 fish ScriptableObjects)
```

---

## Full Documentation

See **README_FISH_SYSTEM.md** for complete system documentation.
See **AGENT_8_COMPLETION_REPORT.md** for implementation details.

---

**Quick Start Complete!**

You now have a fully functional fish system with 60 species and dynamic spawning.

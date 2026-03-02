# Dynamic Events System - Quick Reference

Quick reference for integrating with the Events system (Agent 19).

## Getting Event Multipliers

```csharp
EventManager em = EventManager.Instance;

float spawnMult = em.GetSpawnMultiplier();      // Fish spawn rate
float rareMult = em.GetRareMultiplier();        // Rare fish chance
float hazardMult = em.GetHazardMultiplier();    // Hazard spawn rate
float sanityMult = em.GetSanityMultiplier();    // Sanity drain rate
```

## Checking Active Events

```csharp
// Check if any event is active
GameEvent current = EventManager.Instance.GetCurrentEvent();
if (current != null)
{
    string name = current.data.eventName;
    float timeLeft = current.GetRemainingTime();
}

// Check specific event
if (EventManager.Instance.IsEventActive("blood_moon"))
{
    // Blood Moon is active!
}

// Via specific event system
if (BloodMoonEvent.Instance?.IsActive() ?? false)
{
    // Blood Moon confirmed
}
```

## Subscribing to Events

```csharp
void Start()
{
    // Event lifecycle
    EventSystem.Subscribe<GameEvent>("EventStarted", OnEventStarted);
    EventSystem.Subscribe<GameEvent>("EventEnded", OnEventEnded);

    // Effect changes
    EventSystem.Subscribe<float>("EventSpawnMultiplierChanged", OnSpawnChanged);
    EventSystem.Subscribe<bool>("ForceAberrantFish", OnAberrantForced);
}

void OnDestroy()
{
    // ALWAYS UNSUBSCRIBE!
    EventSystem.Unsubscribe<GameEvent>("EventStarted", OnEventStarted);
    EventSystem.Unsubscribe<GameEvent>("EventEnded", OnEventEnded);
    EventSystem.Unsubscribe<float>("EventSpawnMultiplierChanged", OnSpawnChanged);
    EventSystem.Unsubscribe<bool>("ForceAberrantFish", OnAberrantForced);
}

void OnEventStarted(GameEvent evt)
{
    switch (evt.data.eventType)
    {
        case EventType.BloodMoon:
            // Handle blood moon
            break;
        case EventType.MeteorShower:
            // Handle meteor shower
            break;
        // etc...
    }
}
```

## Checking Fish Availability (Migrations)

```csharp
MigrationSystem ms = MigrationSystem.Instance;

// Is fish available this season?
bool available = ms.IsFishAvailable("salmon");

// Get seasonal spawn modifier
float seasonalMult = ms.GetSeasonalSpawnModifier("salmon");

// Current season
Season season = ms.GetCurrentSeason(); // Spring, Summer, Autumn, Winter

// Days until next migration
int days = ms.GetDaysUntilNextMigration();
```

## Getting Event Forecast

```csharp
EventCalendar calendar = EventCalendar.Instance;

// Upcoming events (next 3 days)
List<EventPrediction> upcoming = calendar.GetUpcomingEvents();

foreach (var prediction in upcoming)
{
    string eventName = prediction.eventData.eventName;
    int daysUntil = prediction.daysUntil;
    float confidence = prediction.confidence; // 0-1
}

// Today's events
List<EventPrediction> today = calendar.GetTodaysEvents();

// Event statistics
EventStatistics stats = calendar.GetEventStatistics("blood_moon");
int timesOccurred = stats.timesOccurred;
int fishCaught = stats.totalFishCaught;
```

## Festival Information

```csharp
FestivalSystem fs = FestivalSystem.Instance;

FestivalInfo info = fs.GetCurrentFestival();

if (info.isActive)
{
    FestivalType type = info.festivalType;
    float timeLeft = info.timeRemaining;

    if (type == FestivalType.FishingTournament)
    {
        Fish biggestCatch = info.biggestCatch;
        float weight = info.biggestCatchWeight;
    }
}
```

## Force Triggering (Testing)

```csharp
// Via EventManager
EventManager.Instance.ForceTriggerEvent("blood_moon");
EventManager.Instance.ForceTriggerEvent("meteor_shower");
EventManager.Instance.ForceTriggerEvent("fog_bank");

// Via specific systems
BloodMoonEvent.Instance.ForceTrigger();
MeteorShowerEvent.Instance.ForceTrigger();
FogBankEvent.Instance.ForceTrigger();
FestivalSystem.Instance.ForceTrigger(FestivalType.FishingTournament);
MigrationSystem.Instance.ForceSeasonChange(Season.Winter);
```

## Common Event System Messages

### To Publish (from your system):

```csharp
// Show notification to player
EventSystem.Publish("ShowNotification", "Your message here");

// Add money (festivals, tournaments)
EventSystem.Publish("AddMoney", 1000f);

// Spawn specific fish
EventSystem.Publish("ForceSpawnLegendary", position);
```

### To Subscribe (in your system):

```csharp
// Weather changes
EventSystem.Subscribe<WeatherType>("OverrideWeather", OnWeatherOverride);

// Shop modifiers
EventSystem.Subscribe<float>("ShopPriceMultiplier", OnShopPriceChanged);
EventSystem.Subscribe<float>("SellPriceMultiplier", OnSellPriceChanged);

// NPC updates
EventSystem.Subscribe<string>("UpdateNPCDialogues", OnDialogueUpdate);

// Day completion
EventSystem.Subscribe("DayCompleted", OnDayCompleted);
```

## Event Type Enum

```csharp
public enum EventType
{
    BloodMoon,
    MeteorShower,
    FogBank,
    Festival,
    Migration,
    Storm,
    SolarEclipse,
    FishSchool,
    Custom
}
```

## Season Enum

```csharp
public enum Season
{
    Spring,  // Salmon run
    Summer,  // Tropical influx
    Autumn,  // Larger fish
    Winter   // Deep dwellers
}
```

## Festival Type Enum

```csharp
public enum FestivalType
{
    FishingTournament,   // Biggest catch wins prize
    NightMarket,         // Shop discount + special items
    HarvestFestival,     // Sell price bonus
    DarkMoonFestival     // Free curse cleansing
}
```

## Common Integration Patterns

### Applying Event Multipliers to Spawning

```csharp
float baseRate = 1.0f;

// Event multipliers
baseRate *= EventManager.Instance.GetSpawnMultiplier();
baseRate *= EventManager.Instance.GetRareMultiplier();

// Seasonal multipliers
baseRate *= MigrationSystem.Instance.GetSeasonalSpawnModifier(fishID);

// Your final spawn rate
float finalRate = baseRate;
```

### Checking All Conditions for Fish Spawn

```csharp
bool CanSpawnFish(string fishID)
{
    // Check seasonal availability
    if (!MigrationSystem.Instance.IsFishAvailable(fishID))
        return false;

    // Check location requirements (from fish data)
    // Check time of day requirements (from fish data)

    return true;
}
```

### Reacting to Blood Moon

```csharp
void OnEventStarted(GameEvent evt)
{
    if (evt.data.eventType == EventType.BloodMoon)
    {
        // Apply red post-processing
        // Change music to ominous track
        // Show warning UI
        // ALL fish spawns become aberrant (handled by FishManager)
    }
}
```

### Handling Festival Tournaments

```csharp
void OnEventStarted(GameEvent evt)
{
    if (evt.data.eventType == EventType.Festival)
    {
        FestivalInfo info = FestivalSystem.Instance.GetCurrentFestival();

        if (info.festivalType == FestivalType.FishingTournament)
        {
            // Show tournament UI
            // Track catches
            // Subscribe to FishCaught event
        }
    }
}
```

## Save/Load Integration

Your system should implement:

```csharp
void Start()
{
    EventSystem.Subscribe<SaveData>("GatheringSaveData", OnSave);
    EventSystem.Subscribe<SaveData>("ApplyingSaveData", OnLoad);
}

void OnSave(SaveData data)
{
    MySystemSaveData myData = new MySystemSaveData
    {
        // Your data here
    };

    data.mySystemData = JsonUtility.ToJson(myData);
}

void OnLoad(SaveData data)
{
    if (string.IsNullOrEmpty(data.mySystemData)) return;

    MySystemSaveData myData = JsonUtility.FromJson<MySystemSaveData>(data.mySystemData);

    // Restore your state
}

void OnDestroy()
{
    EventSystem.Unsubscribe<SaveData>("GatheringSaveData", OnSave);
    EventSystem.Unsubscribe<SaveData>("ApplyingSaveData", OnLoad);
}
```

## Debugging

### Enable Debug Logging

```csharp
EventManager.Instance.enableDebugLogging = true;
BloodMoonEvent.Instance.enableDebugLogging = true;
MigrationSystem.Instance.enableDebugLogging = true;
// etc...
```

### Context Menu Commands

All event systems have right-click context menu commands in Unity:
- Force trigger events
- End active events
- Print status/statistics
- Change seasons
- Clear cooldowns

### Common Issues

**Events not triggering?**
- Check `RandomEventSpawner.enableRandomEvents` is true
- Check event cooldowns haven't blocked it
- Check max events per day limit (default 3)

**Multipliers not applying?**
- Subscribe to `EventSpawnMultiplierChanged` etc.
- Query `EventManager.Instance.GetSpawnMultiplier()`
- Check if event is actually active

**Fish not available?**
- Check `MigrationSystem.Instance.IsFishAvailable(fishID)`
- Fish might be seasonal, not available this season

## Performance Tips

- Events are lightweight - only check when needed
- Don't poll for active events every frame
- Use EventSystem subscriptions instead
- Event managers use singletons - cache the Instance

## Need More Info?

- Full documentation: `Scripts/Events/README.md`
- Integration examples: `Scripts/Events/INTEGRATION_EXAMPLE.cs`
- Summary: `Scripts/Events/AGENT_19_SUMMARY.md`
- Source files: `Scripts/Events/*.cs`

---

*Quick reference for Agent 19 Dynamic Events system*

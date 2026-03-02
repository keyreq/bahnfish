# Events System - Agent 19: Dynamic Events

## Overview

The Events system manages all dynamic occurrences in Bahnfish that keep gameplay fresh and exciting. From rare Blood Moons to seasonal fish migrations, this system creates a living world that changes over time.

## System Architecture

### Core Components

1. **EventManager.cs** - Central manager coordinating all events
2. **EventCalendar.cs** - Tracks history and predicts upcoming events
3. **RandomEventSpawner.cs** - Handles probability-based event triggering
4. **EventData.cs** - ScriptableObject definitions for events

### Specific Event Systems

1. **BloodMoonEvent.cs** - Rare monthly aberrant fish outbreak
2. **MeteorShowerEvent.cs** - Celestial display with fishing bonuses
3. **FogBankEvent.cs** - Dangerous fog encounters at sea
4. **FestivalSystem.cs** - Town celebrations and competitions
5. **MigrationSystem.cs** - Seasonal fish availability changes

## Event Types

### 1. Blood Moon (Rare, High Impact)

**Frequency**: Every 10 days (10% chance if 10+ days passed)
**Duration**: One full night (6 real-time minutes)

**Effects**:
- 100% aberrant fish spawns (all fish become aberrant)
- 3x hazard spawn rate
- 2x sanity drain
- 10x fish sell values
- Guaranteed legendary fish spawn after 1 minute
- Triple relic drops

**Visual**: Red sky, red moon, red water tint, eerie fog

**Integration**:
```csharp
// Check if Blood Moon is active
if (BloodMoonEvent.Instance.IsActive())
{
    // All your fish become aberrant!
}

// Subscribe to Blood Moon events
EventSystem.Subscribe<GameEvent>("EventStarted", (evt) => {
    if (evt.data.eventType == EventType.BloodMoon) {
        // Blood Moon started!
    }
});
```

### 2. Meteor Shower (Medium Frequency, Pure Bonus)

**Frequency**: Every 3 days (30% chance if 3+ days passed)
**Duration**: 30 real-time minutes

**Effects**:
- +200% rare fish spawn rate (3x multiplier)
- +50% legendary fish chance
- +25% catch success rate
- Meteor fragments spawn in water (dredgeable)
- No negative effects!

**Visual**: Shooting stars, meteor trails, celestial particles

**Integration**:
```csharp
// Meteors spawn periodically
EventSystem.Subscribe<Vector3>("MeteorImpact", (position) => {
    // Handle meteor impact at position
    // Maybe spawn a meteor fragment item
});
```

### 3. Fog Bank (Random, High Risk/Reward)

**Frequency**: 5% chance per hour at sea
**Duration**: 5-15 minutes (random)

**Effects**:
- Visibility drops to 20m
- +50% ghost ship spawn rate
- +100% aberrant fish
- Navigation challenging
- Fog-only fish variants appear

**Visual**: Thick fog, reduced camera distance, muffled sounds

**Integration**:
```csharp
// Check if in fog
if (FogBankEvent.Instance.IsActive())
{
    // Reduce UI visibility range
    // Enable fog fish variants
}

// Get remaining fog time
float timeLeft = FogBankEvent.Instance.GetTimeRemaining();
```

### 4. Festivals (Scheduled, Town Events)

**Frequency**: Every 7 days
**Duration**: Full day (12 real-time minutes)

**Types**:
- **Fishing Tournament**: Biggest catch wins $1000
- **Night Market**: All shop prices -20%, special items available
- **Harvest Festival**: Sell prices +50%
- **Dark Moon Festival**: Free curse cleansing

**Integration**:
```csharp
// Check current festival
FestivalInfo festival = FestivalSystem.Instance.GetCurrentFestival();
if (festival.isActive)
{
    switch (festival.festivalType)
    {
        case FestivalType.FishingTournament:
            // Track biggest catch
            break;
        case FestivalType.NightMarket:
            // Apply shop discount
            break;
    }
}
```

### 5. Fish Migration (Seasonal, Long-term)

**Frequency**: Every 14 days (season change)
**Duration**: Entire season (14 days)

**Seasons**:
- **Spring**: Salmon run in rivers (3x spawn rate)
- **Summer**: Tropical fish everywhere (+50% global spawn)
- **Autumn**: Fish feed heavily (1.3x size/weight)
- **Winter**: Deep sea fish surface (+100% deep fish, +20% rare)

**Integration**:
```csharp
// Check if fish is available this season
bool available = MigrationSystem.Instance.IsFishAvailable("salmon");

// Get seasonal spawn modifier
float modifier = MigrationSystem.Instance.GetSeasonalSpawnModifier("salmon");

// Get current season
Season season = MigrationSystem.Instance.GetCurrentSeason();

// Days until next migration
int daysLeft = MigrationSystem.Instance.GetDaysUntilNextMigration();
```

## Event Probability System

### Daily Rolls (at midnight)

Performed by `RandomEventSpawner` each game day:

```
Blood Moon:     10% if 10+ days since last
Meteor Shower:  30% if 3+ days since last
Festival:       Scheduled every 7 days
Storm:          15% chance
Solar Eclipse:  Story-triggered only (not random)
```

### Hourly Rolls (while at sea)

Performed every in-game hour:

```
Fog Bank:       5% per hour
Fish School:    20% per hour (minor spawn boost)
```

### Balancing

- Maximum 3 events per day
- Global cooldown of 60 seconds between events
- Event-specific cooldowns prevent spam
- Only 1 major event active at a time (configurable)

## Event System Architecture

### Event Flow

```
RandomEventSpawner
    ↓ (rolls for events)
EventManager
    ↓ (triggers event)
Specific Event System (BloodMoonEvent, etc.)
    ↓ (applies effects)
Game Systems (FishManager, SanityManager, etc.)
    ↓ (respond to effects)
Player Experience
```

### Communication

All events communicate via `EventSystem` (Agent 1):

**Published Events**:
- `"EventStarted"` (GameEvent)
- `"EventEnded"` (GameEvent)
- `"EventSpawnMultiplierChanged"` (float)
- `"EventRareMultiplierChanged"` (float)
- `"EventHazardMultiplierChanged"` (float)
- `"EventSanityMultiplierChanged"` (float)
- `"ForceAberrantFish"` (bool)
- `"ShowNotification"` (string)

**Subscribed Events**:
- `"DayCompleted"` - Trigger daily rolls
- `"TimeOfDayChanged"` - Check time-specific events
- `"FishCaught"` - Track catches during events
- `"GatheringSaveData"` - Save event state
- `"ApplyingSaveData"` - Restore event state

## Integration with Other Systems

### Agent 1: Core Architecture
- Uses `EventSystem` for all communication
- Integrates with `SaveManager` for persistence

### Agent 3: Time & Environment
- Subscribes to `TimeManager` for time-based triggers
- Can override `WeatherSystem` during events
- Modifies lighting and fog settings

### Agent 7: Horror System
- Modifies `SanityManager` drain rates
- Controls hazard spawn rates
- Ghost ships spawn more during fog

### Agent 8: Fish AI
- Modifies `FishManager` spawn rates
- Controls aberrant fish spawning
- Seasonal fish availability

### Agent 9: Progression
- Festival shop discounts
- Prize money for tournaments
- Sell price modifiers

### Agent 10: Quest/Narrative
- NPC dialogue changes during events
- Event warnings from NPCs
- Solar Eclipse story triggers

### Agent 11: UI/UX
- Display active event notifications
- Show event timers
- Forecast display in journal

## Creating Custom Events

### 1. Create EventData ScriptableObject

```csharp
// In Unity Editor: Right-click > Create > Bahnfish > Events > Event Data
// Configure in Inspector:
- Event ID: "my_custom_event"
- Event Type: Custom
- Probability: 0.2 (20%)
- Duration: 15 minutes
- Effects: Set spawn/rare/hazard multipliers
- Visual effects: Sky/water tints
- Audio: Event music
```

### 2. Register Event (Optional Script)

```csharp
public class MyCustomEvent : MonoBehaviour
{
    private void Start()
    {
        EventSystem.Subscribe<GameEvent>("EventStarted", OnEventStarted);
        EventSystem.Subscribe<GameEvent>("EventEnded", OnEventEnded);
    }

    private void OnEventStarted(GameEvent gameEvent)
    {
        if (gameEvent.data.eventID == "my_custom_event")
        {
            // Apply custom effects
            Debug.Log("My custom event started!");
        }
    }

    private void OnEventEnded(GameEvent gameEvent)
    {
        if (gameEvent.data.eventID == "my_custom_event")
        {
            // Remove custom effects
            Debug.Log("My custom event ended!");
        }
    }
}
```

### 3. Add to EventManager

```csharp
// In Unity Editor:
// - Find EventManager GameObject
// - Add EventData to "All Events" list
// - Event will now roll randomly based on probability
```

## Save/Load Integration

Event state is automatically saved:

```csharp
[Serializable]
public class SaveData
{
    public string eventManagerData;      // Active events
    public string eventCalendarData;     // Event history
    public string migrationData;         // Season/migration state
}
```

Each system handles its own save data:
- EventManager: Active event, current day
- EventCalendar: Event history, statistics
- MigrationSystem: Current season, days since migration

## Debugging and Testing

### Force Trigger Events

```csharp
// Via EventManager
EventManager.Instance.ForceTriggerEvent("blood_moon");

// Via Specific Event
BloodMoonEvent.Instance.ForceTrigger();
MeteorShowerEvent.Instance.ForceTrigger();
FogBankEvent.Instance.ForceTrigger();

// Festivals
FestivalSystem.Instance.ForceTrigger(FestivalType.FishingTournament);

// Seasons
MigrationSystem.Instance.ForceSeasonChange(Season.Winter);
```

### Context Menu Commands

All managers include Unity context menu debug commands:

- **EventManager**: Trigger Test Event, End Current Event, Print Status
- **BloodMoonEvent**: Force Blood Moon, End Blood Moon
- **MeteorShowerEvent**: (auto-managed)
- **FogBankEvent**: Force Fog Bank, End Fog Bank, Test Fog Roll
- **FestivalSystem**: Start [Festival Type], End Festival
- **MigrationSystem**: Change to [Season], Print Status
- **RandomEventSpawner**: Force Daily Roll, Force Hourly Roll, Print Statistics

### Enable Debug Logging

Each system has `enableDebugLogging` toggle:

```csharp
BloodMoonEvent.Instance.enableDebugLogging = true;
// Now logs all Blood Moon activity to console
```

## Performance Considerations

- Events are lightweight - only active event applies effects
- Visual effects use pooled particles where possible
- Event checks are infrequent (daily/hourly)
- No Update() loops when events inactive
- Save data is minimal JSON strings

## Best Practices

1. **Subscribe/Unsubscribe Properly**
   ```csharp
   void Start() => EventSystem.Subscribe<GameEvent>("EventStarted", OnEvent);
   void OnDestroy() => EventSystem.Unsubscribe<GameEvent>("EventStarted", OnEvent);
   ```

2. **Check Event Activity Before Acting**
   ```csharp
   if (BloodMoonEvent.Instance?.IsActive() ?? false)
   {
       // Do blood moon things
   }
   ```

3. **Use Event Multipliers, Don't Hardcode**
   ```csharp
   // Good
   float spawnRate = baseRate * EventManager.Instance.GetSpawnMultiplier();

   // Bad
   if (bloodMoonActive) spawnRate = baseRate * 3f;
   ```

4. **Restore State on Event End**
   - Always restore original values when event ends
   - Use fields to store pre-event state
   - Don't assume default values

## Future Enhancements

Potential additions:
- Solar Eclipse event implementation
- Storm event visuals and mechanics
- More festival types (fishing derby variants)
- Location-specific event variations
- Event chains (one event triggers another)
- Player-triggered events (items that summon events)
- Co-op event synchronization (if multiplayer added)

## Contact

For questions about the Events system:
- Agent 19 (Dynamic Events Agent)
- See `Scripts/Events/` for all source files
- Integration examples in `INTEGRATION_EXAMPLE.cs`

---

**Remember**: Events should feel impactful but not overwhelming. Balance risk with reward, and always give players agency in how they respond!

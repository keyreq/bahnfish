# Bahnfish Fishing System (Agent 5)

## Overview

The Fishing System implements active, engaging fishing gameplay inspired by Cast n Chill's combat mechanics. Players experience tension-based fishing with multiple tools and minigames, making every catch feel earned and exciting.

## Core Philosophy

**ACTIVE, NOT PASSIVE**: Fish fight back! Players must manage line tension, react to fish behavior, and time their actions. This is combat-style fishing, not idle clicking.

## Architecture

### State Machine Flow

```
Idle → Casting → Waiting → Hooked → Reeling → Caught/LineBroken → Idle
  ↑                                                                  ↓
  └──────────────────────────────────────────────────────────────────┘
```

### Components

1. **FishingController.cs** - Main state machine and coordination
2. **TensionSystem.cs** - Active combat mechanics with fish AI
3. **MiniGames/** - Different gameplay for different tools
4. **FishingTools/** - Distinct tool implementations

## Fishing State Machine

### States Explained

#### Idle
- Player can move boat freely
- Controls enabled
- Waiting for fishing input (E key)
- Must be in fishing zone to start

#### Casting
- Brief animation phase (1 second)
- Boat controls disabled
- Line being cast into water
- Publishes `OnFishingStarted` event

#### Waiting
- Line in water, waiting for bite
- Random delay (2-8 seconds)
- Fish spawn request sent to Agent 8
- Visual: bobber animation

#### Hooked
- Fish has bitten!
- Brief reaction window (0.5 seconds)
- "Fish On!" indicator
- Prepares minigame

#### Reeling
- Active minigame running
- Player must manage tension
- Fish fights back dynamically
- Success = Caught, Failure = LineBroken

#### Caught
- Success state (1.5 seconds)
- Fish added to inventory
- Money/XP rewards
- Play celebration effects

#### LineBroken
- Failure state (2 seconds)
- Line snapped, fish escaped
- Play failure effects
- Returns to Idle

## Tension System - THE CORE MECHANIC

### How It Works

The TensionSystem creates **active combat** during fishing:

1. **Fish Resistance**: Each fish has resistance based on:
   - Rarity (Common 1x → Legendary 2x → Aberrant 2.5x)
   - Weight (heavier = harder)
   - Tool power (reduces effective resistance)

2. **Fish Behavior**: Fish actively fight!
   - Random intervals (2-5 seconds)
   - Fight duration (1-3 seconds)
   - Legendary/Aberrant fish fight more often
   - High tension triggers more fights

3. **Player Actions**:
   - **SPACE**: Reel In (+25 tension/sec)
   - **SHIFT**: Let Out Line (-20 tension/sec)
   - **NOTHING**: Natural decay (-10 tension/sec)

4. **Tension Calculation**:
   ```
   Total Tension Change =
     + Fish Resistance (passive)
     + Fish Fighting (2x resistance when active!)
     + Player Reeling
     - Letting Out Line
     - Natural Decay
   ```

5. **Break Condition**:
   - Tension > 95% for 2 seconds = LINE BREAKS
   - Visual warning at 70%+
   - Audio cue at 90%+

6. **Catch Progress**:
   - Only increases when reeling at safe tension (<80%)
   - Decreases if fish fights and player doesn't manage tension
   - Reach 100% = Fish Caught!

### Making It Feel Like Cast n Chill

- Fish behavior is **unpredictable** but **fair**
- Clear visual/audio feedback
- Tension meter always visible
- Players can react to fish movements
- Skill-based, not luck-based

## Mini-Games

### ReelMinigame (Standard Fishing Rod)

**Gameplay**: Balance tension management
- Duration: 20-45 seconds (based on rarity)
- Success: Reach 100% progress
- Failure: Line breaks or timeout

**Difficulty Scaling**:
- Common: 20s, 1x difficulty
- Uncommon: 25s, 1.3x difficulty
- Rare: 30s, 1.6x difficulty
- Legendary: 40s, 2x difficulty
- Aberrant: 45s, 2.5x difficulty

**Controls**:
- SPACE: Reel in (builds tension and progress)
- SHIFT: Let out line (reduces tension, stops progress)

**Strategy**: Watch for fish fighting! Let out line during fights, reel during calm periods.

### HarpoonMinigame (Harpoon Tool)

**Gameplay**: Aim and throw at moving target
- Duration: 10 seconds
- Success: Hit target = instant catch
- Failure: Miss = fish scared away

**Controls**:
- WASD: Move cursor
- SPACE (hold): Charge throw
- SPACE (release): Throw

**Mechanics**:
- Target moves across screen
- Hit radius varies by fish rarity
- Perfect throw (full charge) = bonus
- Aberrant fish move erratically

**Difficulty**:
- Larger fish = bigger target, slower movement
- Rarer fish = faster movement, smaller hit radius

### DredgeMinigame (Dredge Crane)

**Gameplay**: Navigate crane through obstacles
- Duration: 15-30 seconds
- Success: Reach green target
- Failure: Too many collisions or timeout

**Controls**:
- WASD: Move crane

**Mechanics**:
- Obstacles block path
- Some obstacles move (rare+ fish)
- Collision = 1.5s stun
- Cable visualized from top of screen

**Strategy**: Plan path, avoid moving obstacles, be patient.

## Fishing Tools

### 1. FishingRod (Standard)

**Best For**: Most fish, active gameplay

**Stats**:
- Line Strength: 50-150 (upgradable)
- Reel Speed: 1-2x (upgradable)
- Durability: 100-200 (upgradable)

**Upgrades** (5 levels):
- Level 1: Basic rod
- Level 2: +20 strength, +0.2 speed ($100)
- Level 3: Can catch Legendary ($250)
- Level 4: Can catch large Aberrant ($500)
- Level 5: Master rod ($1000)

**Usage**:
- Durability decreases slowly per use
- Repair at dock
- Uses ReelMinigame

### 2. Harpoon (One-Shot)

**Best For**: Large, dangerous fish

**Stats**:
- Ammo: 10/20 (restockable)
- Accuracy: 1-2x (upgradable)
- Damage: 3x multiplier
- Minimum Weight: 5kg

**Economy**:
- Ammo cost: $5 per harpoon
- Full restock: (20 - current) × $5

**Advantages**:
- Instant catch on hit
- No tension management
- Effective on Legendary fish

**Disadvantages**:
- Limited ammo
- Can't catch small fish
- Miss = fish escapes

**Usage**: Use for high-value targets where risk of line break is too high.

### 3. CrabPot (Passive)

**Best For**: AFK income, shellfish

**Stats**:
- Capacity: 5-15 fish (upgradable)
- Catch Interval: 120-70 seconds
- Can catch Aberrant: Upgradable

**Deployment**:
1. Place at location
2. Return later (hours/days)
3. Collect catches
4. Durability decreases over time

**Catches**:
- Shellfish only (crabs, lobster, clams)
- Common/Uncommon mostly
- Small chance Aberrant (if upgraded)

**Strategy**: Deploy multiple pots in different locations for passive income.

### 4. DriftNet (While Sailing)

**Best For**: Traveling between locations

**Stats**:
- Capacity: 8-20 fish (upgradable)
- Catch Interval: 15s
- Speed Penalty: -10% to -5%

**Mechanics**:
- Auto-deploys when moving (if upgraded)
- Catches fish while sailing
- Faster speed = more catches
- Full net = stops catching

**Trade-off**: Boat speed reduced but passive income while traveling.

**Upgrades**:
- Capacity increase
- Faster catch rate
- Reduced speed penalty
- Auto-deploy feature

## Integration Points

### With Agent 2 (Player Controller)

```csharp
// Disable boat movement during fishing
boatController.SetMovementEnabled(false);

// Re-enable when fishing ends
boatController.SetMovementEnabled(true);
```

### With Agent 6 (Inventory)

```csharp
// Add caught fish to inventory
EventSystem.Subscribe<Fish>("FishCaught", (fish) => {
    inventoryManager.AddItem(fish);
});
```

### With Agent 8 (Fish AI)

```csharp
// Request fish spawn at location
EventSystem.Publish("RequestFishSpawn", new FishSpawnRequest {
    location = currentZone.locationID,
    time = TimeManager.Instance.GetCurrentTime(),
    weather = WeatherSystem.Instance.GetCurrentWeather()
});

// Receive spawned fish
EventSystem.Subscribe<Fish>("OnFishBite", OnFishBite);
```

### With Agent 11 (UI)

**Events to Display**:
- `OnTensionUpdated` - Show tension meter
- `OnFishingStarted` - Show fishing UI
- `OnFishingEnded` - Hide fishing UI
- `FishCaught` - Show catch notification
- `LineBroken` - Show failure message

**Required UI Elements**:
- Tension bar (0-100%, color-coded)
- Progress bar (catch progress)
- Fish name/rarity indicator
- Timer (for minigames)
- Controls hint

## Event Reference

### Published Events

| Event Name | Data Type | When | Description |
|------------|-----------|------|-------------|
| `OnFishingStarted` | none | Casting state entered | Player started fishing |
| `OnFishingEnded` | none | Return to Idle | Fishing session ended |
| `FishCaught` | `Fish` | Caught state | Successfully caught fish |
| `LineBroken` | none | LineBroken state | Line broke, fish escaped |
| `OnTensionUpdated` | `TensionUpdateData` | Every frame (Reeling) | Current tension status |
| `OnCrabPotDeployed` | `CrabPot` | Pot placed | Crab pot deployed |
| `OnCrabPotRetrieved` | `CrabPot` | Pot collected | Crab pot retrieved |
| `OnDriftNetDeployed` | `DriftNet` | Net deployed | Drift net started |
| `OnDriftNetRetrieved` | `DriftNet` | Net retrieved | Drift net collected |
| `OnToolBroken` | `BaseFishingTool` | Tool breaks | Tool needs repair |

### Subscribed Events

| Event Name | Data Type | When | Action |
|------------|-----------|------|--------|
| `OnFishBite` | `Fish` | Fish spawned and bites | Start Hooked state |
| `OnPlayerMoved` | `PlayerMovedEventData` | Player moves | Update drift net |

## Fishing Zones

**Component**: `FishingZone.cs`

Mark areas where fishing is allowed:

```csharp
public class FishingZone : MonoBehaviour
{
    public string zoneName = "Fishing Spot";
    public string locationID = "starter_lake";
}
```

**Setup**:
1. Add to trigger collider in water
2. Player enters = can fish
3. Player exits = stop fishing

## Controls

### Active Fishing (ReelMinigame)
- **E**: Start fishing (in zone)
- **SPACE**: Reel in (increase tension)
- **SHIFT**: Let out line (decrease tension)
- **ESC**: Cancel fishing

### Harpoon Minigame
- **WASD**: Move cursor
- **SPACE** (hold/release): Charge and throw

### Dredge Minigame
- **WASD**: Navigate crane

### Passive Tools
- **E**: Deploy/Retrieve crab pot
- **R**: Deploy/Retrieve drift net (or auto)

## Tutorial Sequence

**First 5 Minutes**:

1. **Approach Fishing Zone** (Starter Lake)
   - UI hint: "Press E to start fishing"

2. **First Cast** (Common Fish)
   - Brief tutorial overlay
   - Shows tension meter
   - "SPACE to reel, SHIFT to let out"

3. **Simple Catch**
   - Fish doesn't fight much
   - Easy success
   - Celebration!

4. **Sell Fish**
   - Return to dock
   - Earn money
   - Buy first upgrade?

5. **Second Cast** (Harder Fish)
   - Fish fights back!
   - Player must react
   - Learn tension management

## Debug Tools

### FishingController Debug

```csharp
// In Update()
if (Input.GetKeyDown(KeyCode.F1))
{
    Debug.Log($"State: {currentState}, Tool: {currentTool?.toolName}");
}
```

### TensionSystem Debug UI

Automatic debug display shows:
- Current tension percentage
- Fish fighting status
- Catch progress
- Control hints

**Toggle**: `Debug.isDebugBuild` must be true

### Gizmos

All components draw debug gizmos:
- **FishingController**: Fishing range, cast position
- **CrabPot**: Deployed location, buoy
- **DriftNet**: Net area, trail behind boat
- **FishingZone**: Zone boundaries

## Performance Considerations

### Optimization Tips

1. **Object Pooling**: Reuse minigame components instead of Create/Destroy
2. **Event Throttling**: Only publish OnTensionUpdated every 0.1s, not every frame
3. **LOD**: Disable gizmos in builds
4. **Passive Tools**: Update crab pots/nets on fixed interval, not every frame

### Memory Management

- Minigames auto-destroy when complete
- Fish data is lightweight (no meshes in data structure)
- Event system automatically handles null references

## Known Limitations

### Current Implementation

- **Mock Fish Data**: Using placeholder fish until Agent 8 integration
- **No UI**: Debug display only, waiting for Agent 11
- **No Inventory**: Fish added to money directly until Agent 6
- **No Save/Load**: Tool states not persisted yet

### Future Enhancements

- **Split-screen view**: Above/below water simultaneously
- **Photo mode**: Capture catch moments
- **Multiplayer**: Co-op fishing (post-launch)
- **Weather effects**: Rain/storms affect fishing
- **Bait system**: Different baits attract different fish

## Testing Checklist

### Manual Tests

- [ ] Cast in fishing zone
- [ ] Cancel fishing mid-cast
- [ ] Catch common fish
- [ ] Let line break intentionally
- [ ] Upgrade fishing rod
- [ ] Deploy/retrieve crab pot
- [ ] Deploy/retrieve drift net
- [ ] Use harpoon (hit and miss)
- [ ] Play all three minigames
- [ ] Exit fishing zone while fishing

### Integration Tests

- [ ] Boat controls disabled during fishing
- [ ] Boat controls re-enabled after
- [ ] Events published correctly
- [ ] Fish added to inventory (when Agent 6 ready)
- [ ] Money awarded for catches
- [ ] Tools persist durability

## FAQ

**Q: Why is fishing so hard?**
A: Fishing is meant to be engaging, not passive. Start with common fish to learn, then tackle harder rarities.

**Q: Can I fish from land?**
A: No, must be in a boat within a FishingZone.

**Q: Do I need the harpoon?**
A: No, but it's very effective for large fish that might break your rod line.

**Q: How many crab pots can I deploy?**
A: Currently unlimited, but will be tied to boat upgrades (Agent 9).

**Q: Does drift net work while idle?**
A: Only works while actively sailing (boat speed > 1).

**Q: What happens if I run out of durability?**
A: Tool still works but very weak. Repair at dock for best performance.

## Version History

- **v1.0** (Agent 5 Initial): Complete fishing system with all tools and minigames

## Contact

Agent 5 deliverables complete! For questions or integration help, see:
- Agent 2: Player control integration
- Agent 6: Inventory integration
- Agent 8: Fish AI integration
- Agent 11: UI integration

---

**Remember**: Fishing should feel ACTIVE and ENGAGING, not idle clicking. Every catch should feel earned!

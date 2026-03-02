# Fishing System - Quick Start Guide

## Want to Test Fishing Right Now?

Follow these steps to get fishing in under 5 minutes!

## Step 1: Scene Setup (2 minutes)

### Add Required Objects

1. **Create FishingController**
   ```
   - GameObject → Create Empty
   - Name: "FishingManager"
   - Add Component → FishingController
   - Add Component → TensionSystem (auto-adds if missing)
   ```

2. **Create Fishing Zone**
   ```
   - GameObject → 3D Object → Cube (or existing water collider)
   - Name: "FishingZone_StarterLake"
   - Add Component → Box Collider
   - Check "Is Trigger"
   - Add Component → FishingZone
   - Set Zone Name: "Starter Lake"
   - Set Location ID: "starter_lake"
   - Scale to cover fishable water area
   ```

3. **Add Cast Point** (optional, improves visuals)
   ```
   - In Player/Boat hierarchy
   - GameObject → Create Empty
   - Name: "CastPoint"
   - Position at front of boat
   - Assign to FishingController's "Cast Point" field
   ```

## Step 2: Equip a Fishing Rod (1 minute)

### Option A: Via Inspector

```
1. Select FishingManager object
2. Create a Fishing Rod:
   - GameObject → Create Empty
   - Name: "FishingRod_Basic"
   - Add Component → FishingRod
3. In FishingController:
   - Find "Current Tool" field
   - Assign your FishingRod object
```

### Option B: Via Code

Add this to your player setup script:

```csharp
private void Start()
{
    // Create fishing rod
    GameObject rodObj = new GameObject("FishingRod");
    FishingRod rod = rodObj.AddComponent<FishingRod>();

    // Equip it
    FishingController fc = FindObjectOfType<FishingController>();
    if (fc != null)
        fc.EquipTool(rod);
}
```

## Step 3: Test! (1 minute)

### Controls

1. **Enter the Fishing Zone** (drive boat into the trigger)
   - Console will show: "Entered fishing zone: Starter Lake"

2. **Press E** to start fishing
   - Boat controls will disable
   - State changes to "Casting"
   - After 1 second, changes to "Waiting"

3. **Wait for Bite** (2-8 seconds)
   - Console will show: "Largemouth Bass is fighting back!"
   - State changes to "Hooked" → "Reeling"

4. **Manage Tension!**
   - **SPACE**: Reel in (increases tension and progress)
   - **SHIFT**: Let out line (decreases tension)
   - Watch the debug UI at bottom of screen

5. **Catch the Fish!**
   - Get progress to 100% while keeping tension < 95%
   - Success: "Caught Largemouth Bass!"
   - Money increases automatically

### What You Should See

**Debug UI** (automatic in play mode):
```
Tension: 45.2% [FIGHTING!]
[======------] Green/Yellow/Red bar

Progress: 67.3%
[=========---] Progress bar

SPACE: Reel In | SHIFT: Let Out
```

**Console Logs**:
```
Fishing State Changed: Casting
Fishing State Changed: Waiting
Fishing State Changed: Hooked
Tension System initialized - Fish: Largemouth Bass, Resistance: 35.0
Fishing State Changed: Reeling
Largemouth Bass is fighting back!
Caught Largemouth Bass!
Adding Largemouth Bass to inventory (integration pending)
Fishing State Changed: Idle
```

## Step 4: Try Other Tools (optional)

### Harpoon (One-Shot Fishing)

```csharp
// Create harpoon
GameObject harpoonObj = new GameObject("Harpoon");
Harpoon harpoon = harpoonObj.AddComponent<Harpoon>();

// Equip
FishingController fc = FindObjectOfType<FishingController>();
fc.EquipTool(harpoon);

// Fish - Uses HarpoonMinigame instead!
// WASD to move cursor, SPACE to throw
```

### Crab Pot (Passive)

```csharp
// Create crab pot
GameObject potObj = new GameObject("CrabPot");
CrabPot pot = potObj.AddComponent<CrabPot>();

// Deploy
pot.Deploy(transform.position, "starter_lake");

// Wait (it catches fish over time)
// In editor: Advance time or wait
// Or use Time.timeScale = 10f to speed up

// Retrieve
pot.Retrieve();

// Collect
List<Fish> catches = pot.CollectCatch();
Debug.Log($"Caught {catches.Count} shellfish!");
```

### Drift Net (While Sailing)

```csharp
// Create drift net
GameObject netObj = new GameObject("DriftNet");
DriftNet net = netObj.AddComponent<DriftNet>();

// Deploy
net.Deploy();

// Sail around (boat must be moving!)
// Net catches fish automatically while traveling

// Retrieve
net.Retrieve();
List<Fish> catches = net.CollectCatch();
```

## Troubleshooting

### "Nothing happens when I press E"

**Check**:
- [ ] Are you in the fishing zone? (console should say "Entered fishing zone")
- [ ] Is a tool equipped? (FishingController.currentTool not null)
- [ ] Is state Idle? (check console for state changes)

**Fix**: Make sure fishing zone trigger is large enough and boat collider enters it.

---

### "Fish never bites"

**Check**:
- [ ] Wait 2-8 seconds after casting
- [ ] Check console for "State Changed: Waiting"

**Fix**: Mock fish spawn should happen automatically. If not, check console for errors.

---

### "Tension meter not visible"

**Check**:
- [ ] Is game in play mode?
- [ ] Is Debug.isDebugBuild true?
- [ ] Is TensionSystem component present?

**Fix**: TensionSystem auto-adds to FishingController. If missing, add manually.

---

### "Controls don't work"

**Correct Controls**:
- E = Start fishing (only in fishing zone)
- SPACE = Reel in (only while Reeling)
- SHIFT = Let out line (only while Reeling)
- ESC = Cancel fishing

**Note**: Boat controls disabled while fishing!

---

### "Line keeps breaking"

**Tips**:
- Let out line (SHIFT) when fish is fighting
- Don't reel (SPACE) constantly
- Watch tension meter - stop reeling at 70%+
- Fish behavior is dynamic - react to it!

**Remember**: This is skill-based! Practice makes perfect.

---

### "Fish escaped but I was doing well"

**Check**:
- Did you timeout? (20-45s limit per fish)
- Did tension exceed 95% for 2 seconds?

**Fix**: Reel more aggressively during calm periods, be patient during fights.

---

## Advanced Testing

### Test All Fish Rarities

Modify the mock fish in FishingController.cs:

```csharp
private void SimulateFishBite()
{
    Fish mockFish = new Fish
    {
        id = "test_fish",
        name = "Legendary Marlin",
        rarity = FishRarity.Legendary,  // Try: Common, Uncommon, Rare, Legendary, Aberrant
        baseValue = 100f,
        inventorySize = new Vector2Int(3, 2),
        weight = 20f,  // Heavier = harder
        length = 100f,
        isAberrant = false  // Set true for extra challenge!
    };

    EventSystem.Publish("OnFishBite", mockFish);
}
```

### Force Specific States

```csharp
// In Update() of a test script
if (Input.GetKeyDown(KeyCode.F2))
{
    // Force fish bite
    Fish testFish = new Fish {
        name = "Test Fish",
        rarity = FishRarity.Rare
    };
    EventSystem.Publish("OnFishBite", testFish);
}

if (Input.GetKeyDown(KeyCode.F3))
{
    // Force line break
    EventSystem.Publish("OnLineBroken");
}

if (Input.GetKeyDown(KeyCode.F4))
{
    // Force successful catch
    Fish testFish = new Fish { name = "Instant Catch" };
    EventSystem.Publish("OnFishCaught", testFish);
}
```

### Speed Up Testing

```csharp
// Make fishing faster for testing
private void Update()
{
    if (Input.GetKey(KeyCode.F5))
        Time.timeScale = 5f;  // 5x speed
    else
        Time.timeScale = 1f;
}
```

---

## What to Expect

### Common Fish (Easy)
- Duration: ~20 seconds
- Fights occasionally
- Low resistance
- Easy to catch

### Rare Fish (Medium)
- Duration: ~30 seconds
- Fights frequently
- Medium resistance
- Requires attention

### Legendary Fish (Hard)
- Duration: ~40 seconds
- Fights often
- High resistance
- Very challenging

### Aberrant Fish (Very Hard)
- Duration: ~45 seconds
- Fights constantly
- Very high resistance
- Erratic behavior
- Expert-level challenge

---

## Performance Check

### Expected Performance

In play mode:
- FPS: 60+ (minimal impact)
- Frame Time: <0.5ms for fishing system
- Memory: ~50KB per session
- GC: Minimal (events use structs)

### If Performance Issues

Check:
- [ ] Is TensionSystem publishing every frame? (throttle to 0.1s)
- [ ] Are minigames being destroyed after use? (should auto-destroy)
- [ ] Are there memory leaks? (check event unsubscribes)

---

## Quick Reference

### Fishing Flow
```
1. Enter Fishing Zone
2. Press E
3. Wait for Bite (2-8s)
4. Manage Tension (SPACE/SHIFT)
5. Catch Fish (100% progress)
6. Return to Idle
```

### State Machine
```
Idle → Casting (1s) → Waiting (2-8s) → Hooked (0.5s) → Reeling → Caught/Broken → Idle
```

### Controls
```
E      = Start fishing (in zone)
SPACE  = Reel in (while fishing)
SHIFT  = Let out line (while fishing)
ESC    = Cancel fishing
```

### Debug Keys (if you added them)
```
F1 = Show fishing state
F2 = Force fish bite
F3 = Force line break
F4 = Force catch
F5 = 5x speed
```

---

## Next Steps

Once basic fishing works:

1. **Test All Tools**: Rod, Harpoon, Crab Pot, Drift Net
2. **Test All Minigames**: Reel, Harpoon Aim, Dredge Navigation
3. **Test Edge Cases**: Leave zone while fishing, run out of ammo, break tool
4. **Test Integration**: When other agents ready
5. **Polish**: Add UI (Agent 11), SFX (Agent 12), VFX (Agent 13)

---

## Getting Help

**Documentation**:
- `README.md` - Full system documentation
- `INTEGRATION_GUIDE.md` - For other agents
- `AGENT_5_DELIVERABLES.md` - Complete feature list

**Check Console**: All state changes and important events are logged

**Enable Gizmos**: See fishing range, zones, deployed tools

**Use Debug UI**: Shows tension, progress, controls in play mode

---

**Happy Fishing! 🎣**

The system is designed to work out-of-the-box with minimal setup. If something doesn't work, check the console first - it will tell you what's wrong!

# Bahnfish Testing Framework

**Version**: 1.0.0
**Last Updated**: 2026-03-01
**Phase**: 6 - Testing & Launch Preparation

---

## Testing Overview

This document provides a comprehensive testing framework for Bahnfish, covering all systems across Phases 1-5.

---

## Test Categories

### 1. Unit Tests (Per-System)
### 2. Integration Tests (Cross-System)
### 3. Performance Tests (FPS, Memory, Load Times)
### 4. Balance Tests (Economy, Difficulty)
### 5. User Experience Tests (Playability, Fun Factor)
### 6. Platform Tests (Windows, Mac, Linux, Controllers)
### 7. Regression Tests (Re-test after bug fixes)

---

## 1. UNIT TESTS

Test each system in isolation.

### Phase 1: Foundation

#### GameManager (Core/GameManager.cs)
- [ ] **Singleton**: Only one instance exists
- [ ] **Initialization**: Initialize() called on Awake
- [ ] **DontDestroyOnLoad**: Persists across scenes
- [ ] **Game State**: GameState accessible and modifiable
- [ ] **Events**: Publishes GameInitialized event

**Test Steps**:
1. Create new scene
2. Add GameManager GameObject
3. Enter Play mode
4. Verify singleton instance exists
5. Load another scene
6. Verify GameManager still exists
7. Exit Play mode

#### EventSystem (Core/EventSystem.cs)
- [ ] **Subscribe**: Can subscribe to events
- [ ] **Publish**: Can publish events with data
- [ ] **Unsubscribe**: Can unsubscribe from events
- [ ] **Multiple Subscribers**: Multiple listeners receive same event
- [ ] **Type Safety**: Events are type-safe

**Test Steps**:
1. Subscribe to "TestEvent" with callback
2. Publish "TestEvent" with test data
3. Verify callback received data
4. Unsubscribe from "TestEvent"
5. Publish again
6. Verify callback NOT called

#### SaveManager (SaveSystem/SaveManager.cs)
- [ ] **Save Game**: SaveGame() creates save file
- [ ] **Load Game**: LoadGame() loads save file
- [ ] **Auto-Save**: Auto-save triggers every 5 minutes
- [ ] **Backup**: Creates up to 3 backup files
- [ ] **Validation**: Detects corrupted saves
- [ ] **Events**: Publishes SaveComplete, LoadComplete

**Test Steps**:
1. Modify GameState (set money = 1000)
2. Call SaveManager.Instance.SaveGame()
3. Verify save file exists
4. Modify GameState (set money = 0)
5. Call SaveManager.Instance.LoadGame()
6. Verify money = 1000 (loaded correctly)

#### TimeManager (Environment/TimeManager.cs)
- [ ] **Time Progression**: CurrentTime increases over time
- [ ] **Day Length**: 15-minute default (configurable)
- [ ] **Time of Day**: Correctly identifies Day/Dusk/Night/Dawn
- [ ] **Events**: Publishes TimeUpdated, TimeOfDayChanged
- [ ] **Pause**: Time stops when paused

**Test Steps**:
1. Set dayLengthMinutes = 1 (for faster testing)
2. Enter Play mode
3. Observe currentTime increase
4. Verify TimeOfDayChanged events fire
5. Verify transitions: Day→Dusk→Night→Dawn→Day

#### WeatherSystem (Environment/WeatherSystem.cs)
- [ ] **Weather Types**: 4 types (Clear, Rain, Storm, Fog)
- [ ] **Transitions**: Smooth weather changes
- [ ] **Forecast**: 3-day forecast available
- [ ] **Events**: Publishes WeatherChanged
- [ ] **Randomization**: Weather changes randomly

**Test Steps**:
1. Set weather to Clear
2. Call ChangeWeather(Rain)
3. Verify weather transitions to Rain
4. Verify WeatherChanged event fired
5. Check GetForecast() returns 3 days

---

### Phase 2: Core Gameplay

#### FishingController (Fishing/FishingController.cs)
- [ ] **State Machine**: 7 states work correctly
- [ ] **Casting**: Transitions Idle→Casting→Waiting
- [ ] **Hooking**: Fish bite triggers Hooked state
- [ ] **Reeling**: Player can reel in fish
- [ ] **Tension**: TensionSystem integrated
- [ ] **Catch**: Success transitions to Caught state
- [ ] **Line Break**: High tension breaks line

**Test Steps**:
1. Press Space (cast)
2. Verify state = Casting
3. Wait for cast complete
4. Verify state = Waiting
5. Simulate fish bite
6. Verify state = Hooked
7. Press Space (reel)
8. Verify tension increases
9. Reduce tension before break
10. Continue reeling until caught

#### InventoryManager (Inventory/InventoryManager.cs)
- [ ] **Grid System**: 10x10 grid initialized
- [ ] **Add Item**: Can add items to grid
- [ ] **Rotate Item**: R key rotates items
- [ ] **Collision**: Detects overlapping items
- [ ] **Remove Item**: Can remove items
- [ ] **Save/Load**: Inventory persists

**Test Steps**:
1. Add 1x1 item to grid (0,0)
2. Verify item placed
3. Add 2x2 item to grid (0,0)
4. Verify placement fails (collision)
5. Add 2x2 item to grid (2,2)
6. Verify item placed
7. Rotate item (press R)
8. Verify shape rotated

#### SanityManager (Horror/SanityManager.cs)
- [ ] **Sanity Drain**: Drains at night
- [ ] **Drain Rates**: Day=0, Dusk=0.2/s, Night=0.5/s
- [ ] **Regeneration**: Regenerates at shore
- [ ] **Effects**: Low sanity triggers hazards
- [ ] **Events**: Publishes SanityChanged

**Test Steps**:
1. Set time to Night
2. Set sanity = 100
3. Wait 10 seconds
4. Verify sanity decreased (should be ~95)
5. Set sanity = 25
6. Verify hazards spawn
7. Move to shore
8. Verify sanity regenerates

#### FishDatabase (Fish/FishDatabase.cs)
- [ ] **60 Species**: All 60 fish defined
- [ ] **Rarity Distribution**: Correct spawn rates
- [ ] **Bait Preferences**: Each fish has preferences
- [ ] **Aberrant Variants**: 10 aberrant fish
- [ ] **Data Integrity**: No missing fields

**Test Steps**:
1. Call LoadAllFish()
2. Verify allSpecies.Count = 60
3. Count fish by rarity
4. Verify: 20 Common, 15 Uncommon, 10 Rare, 5 Legendary, 10 Aberrant
5. Check random fish has valid baitPreferences

---

### Phase 3: Content

#### EconomySystem (Progression/EconomySystem.cs)
- [ ] **Currency Management**: Money, scrap, relics tracked
- [ ] **Add Money**: AddMoney() increases balance
- [ ] **Spend Money**: SpendMoney() decreases if sufficient
- [ ] **Can Afford**: CanAfford() checks correctly
- [ ] **Events**: Publishes MoneyChanged

**Test Steps**:
1. Set money = 100
2. Call AddMoney(50)
3. Verify money = 150
4. Call SpendMoney(200)
5. Verify transaction fails (insufficient)
6. Verify money still = 150
7. Call SpendMoney(100)
8. Verify money = 50

#### QuestManager (Narrative/QuestManager.cs)
- [ ] **Quest Tracking**: 30+ quests available
- [ ] **Active Quests**: Max 5 active quests
- [ ] **Quest Progress**: Auto-tracks via events
- [ ] **Quest Completion**: Rewards granted
- [ ] **Save/Load**: Quest progress persists

**Test Steps**:
1. Start quest "Catch 5 Common Fish"
2. Verify quest added to activeQuests
3. Catch 1 common fish
4. Verify progress: 1/5
5. Catch 4 more common fish
6. Verify quest completed
7. Verify reward granted (money, XP, etc.)

#### LocationManager (World/LocationManager.cs)
- [ ] **13 Locations**: All defined
- [ ] **Travel**: Can travel between locations
- [ ] **Fuel Cost**: Consumes fuel
- [ ] **Licenses**: Requires license to access
- [ ] **Fish Pools**: Each location has unique fish

**Test Steps**:
1. Start at Calm Lake (free)
2. Attempt travel to Deep Ocean (unlocked? fuel?)
3. If blocked, purchase license
4. Travel to Deep Ocean
5. Verify fuel consumed
6. Verify fish pool changed
7. Verify sanity drain rate changed

#### EventManager (Events/EventManager.cs)
- [ ] **Daily Roll**: Events roll each day
- [ ] **Blood Moon**: 10% chance if 10+ days
- [ ] **Meteor Shower**: 30% chance if 3+ days
- [ ] **Festivals**: 4 types scheduled
- [ ] **Event Effects**: Buffs/debuffs apply

**Test Steps**:
1. Set daysSinceBloodMoon = 15
2. Trigger DailyRoll()
3. Check if Blood Moon activated
4. If yes, verify effects (10× fish values, all aberrant)
5. Wait for event to end
6. Verify effects removed

---

### Phase 4: Feature Expansion

#### CookingSystem (Cooking/CookingSystem.cs)
- [ ] **30+ Recipes**: All recipes available
- [ ] **Cooking**: Can cook meals
- [ ] **Buff Application**: Buffs apply after eating
- [ ] **Buff Stacking**: Same type refreshes, different stack
- [ ] **Real-Time Cooking**: Uses real-time (not game time)

**Test Steps**:
1. Obtain ingredients (2 common fish)
2. Start cooking "Fish Stew"
3. Verify cooking timer starts
4. Wait for completion
5. Verify buff applied (+15% Line Strength, 8 min)
6. Cook another meal with different buff
7. Verify both buffs active
8. Cook same meal again
9. Verify buff refreshed (not doubled)

#### BreedingSystem (Aquarium/BreedingSystem.cs)
- [ ] **Compatibility**: Checks same species, maturity, happiness
- [ ] **Genetics**: 10 traits inherited correctly
- [ ] **Mutations**: 1-5% chance triggers
- [ ] **Incubation**: 24-hour real-time breeding
- [ ] **Offspring**: New fish created with traits

**Test Steps**:
1. Add 2 fish of same species to tank
2. Attempt breeding
3. Verify compatibility check passes
4. Start breeding (24h incubation)
5. Fast-forward time or wait
6. Verify offspring created
7. Check offspring traits (inherited from parents)
8. Check for mutation (rare)

#### CompanionManager (Companion/CompanionManager.cs)
- [ ] **Pet Selection**: 6 pets available
- [ ] **Active Pet**: One pet active at a time
- [ ] **Petting**: **THE PETTING MECHANIC WORKS!**
- [ ] **Loyalty**: Loyalty increases when petted
- [ ] **Abilities**: Pet abilities activate
- [ ] **Feeding**: Daily feeding requirement

**Test Steps**:
1. Set active pet = Dog
2. Position player near dog
3. Press E (pet interaction)
4. Verify:
   - Animation plays
   - Sound effect plays
   - Hearts spawn
   - Loyalty increases +5
   - Cooldown applies (30s)
5. Wait 30 seconds
6. Pet again, verify works

#### PhotoModeController (Photography/PhotoModeController.cs)
- [ ] **Enter/Exit**: P key toggles photo mode
- [ ] **Free Camera**: WASD + mouse movement
- [ ] **Game Pause**: Time.timeScale = 0 during photo mode
- [ ] **Filters**: 20+ filters apply
- [ ] **Capture**: Space takes photo
- [ ] **Quality Rating**: 1-5 stars calculated

**Test Steps**:
1. Press P (enter photo mode)
2. Verify game pauses
3. Move camera with WASD
4. Apply filter (Sepia)
5. Verify real-time preview
6. Press Space (capture)
7. Verify photo saved
8. Check quality rating
9. Press P (exit photo mode)
10. Verify game resumes

#### IdleManager (Idle/IdleManager.cs)
- [ ] **Offline Tracking**: Records logout time
- [ ] **Earnings Calculation**: Calculates offline rewards
- [ ] **24-Hour Cap**: Enforces max offline time
- [ ] **Welcome Back**: Displays earnings summary
- [ ] **Upgrades**: Idle upgrades apply correctly

**Test Steps**:
1. Set lastLogoutTime = 8 hours ago
2. Enter game
3. Verify CalculateOfflineRewards() called
4. Verify earnings displayed in WelcomeBackUI
5. Check money increased correctly
6. Verify 24-hour cap enforced (if >24h)

---

### Phase 5: Polish

#### AudioManager (Audio/AudioManager.cs)
- [ ] **Audio Pooling**: 32 AudioSources created
- [ ] **Volume Control**: 5 channels (Master, Music, SFX, Ambient, UI)
- [ ] **Audio Ducking**: Music lowers during dialog/SFX
- [ ] **Fade Operations**: Smooth volume transitions
- [ ] **Save/Load**: Volume settings persist

**Test Steps**:
1. Verify AudioManager initialized
2. Play SFX (fishing cast)
3. Verify sound plays
4. Adjust SFX volume to 50%
5. Verify sound quieter
6. Play 33 sounds simultaneously
7. Verify pooling (max 32 concurrent)
8. Save game
9. Load game
10. Verify volume settings restored

#### MusicSystem (Audio/MusicSystem.cs)
- [ ] **8+ Tracks**: All tracks available
- [ ] **Track Switching**: Transitions smoothly
- [ ] **Layer System**: Layers add/remove dynamically
- [ ] **Game State Integration**: Music responds to time, sanity, events
- [ ] **Crossfade**: 2-5 second transitions

**Test Steps**:
1. Set time to Day
2. Verify Day music plays
3. Set time to Night
4. Verify music transitions to Night theme
5. Set sanity = 20
6. Verify horror layer added
7. Set sanity = 80
8. Verify horror layer removed

#### VFXManager (VFX/VFXManager.cs)
- [ ] **Particle Pooling**: Particles reused
- [ ] **Quality LOD**: 4 levels (Low/Medium/High/Ultra)
- [ ] **Auto-Quality**: Adjusts based on FPS
- [ ] **Max Particles**: 10,000 cap enforced
- [ ] **Save/Load**: VFX quality persists

**Test Steps**:
1. Set quality = Ultra
2. Spawn water splash
3. Verify splash particles appear
4. Spawn 50 splashes rapidly
5. Verify pooling works (no instantiation lag)
6. Set quality = Low
7. Spawn splash
8. Verify particle count reduced (20%)

#### SettingsManager (Accessibility/SettingsManager.cs)
- [ ] **60+ Settings**: All settings functional
- [ ] **5 Categories**: Video, Audio, Controls, Gameplay, Accessibility
- [ ] **Real-Time Apply**: Settings apply without restart
- [ ] **Save/Load**: Settings persist via PlayerPrefs
- [ ] **Reset Defaults**: Can reset to defaults

**Test Steps**:
1. Open settings menu
2. Change resolution to 1920x1080
3. Verify screen resolution changes immediately
4. Change colorblind mode to Deuteranopia
5. Verify shader applied
6. Enable reduced motion
7. Verify camera shake disabled
8. Save and quit
9. Relaunch game
10. Verify all settings restored

---

## 2. INTEGRATION TESTS

Test systems working together.

### Test 1: Complete Fishing Loop
**Systems**: Fishing, Inventory, Economy, UI, Audio, VFX

1. Cast fishing rod (FishingController)
2. Verify audio plays (cast sound)
3. Verify VFX spawns (line arc, splash)
4. Wait for fish bite
5. Verify audio plays (bite sound)
6. Reel in fish (TensionSystem)
7. Verify audio plays (reel sound)
8. Verify VFX (water disturbance, tension sparkles)
9. Catch fish
10. Verify audio plays (success sound)
11. Verify VFX (celebration particles)
12. Fish added to inventory (InventoryManager)
13. Verify UI updates (inventory grid)
14. Sell fish (EconomySystem)
15. Verify money increases
16. Verify UI updates (resource display)

**Expected Result**: Entire loop works smoothly with audio and visual feedback at each step.

### Test 2: Night Horror Sequence
**Systems**: Time, Sanity, Horror, Audio, VFX, Events

1. Set time to Dusk (18:00)
2. Verify music transitions to dusk theme
3. Set time to Night (20:00)
4. Verify music transitions to night theme
5. Verify sanity drain starts (0.5/s)
6. Wait until sanity < 50%
7. Verify horror layer added to music
8. Verify VFX (vignette, desaturation)
9. Sanity drops below 30%
10. Verify hazards start spawning
11. Fish thief spawns
12. Verify audio (crow caw)
13. Verify VFX (dark mist, feathers)
14. Thief steals fish
15. Verify inventory fish removed
16. Verify audio (theft sound)

**Expected Result**: Progressive horror experience with all systems synchronized.

### Test 3: Breeding to Sale Loop
**Systems**: Aquarium, Breeding, Genetics, Economy, UI

1. Catch 2 fish of same species
2. Add both to aquarium tank
3. Verify fish displayed (DisplayController)
4. Feed both fish daily
5. Verify happiness increases
6. Start breeding pair
7. Verify compatibility check passes
8. Wait 24 hours (or fast-forward)
9. Offspring born
10. Verify offspring has inherited traits
11. Check for mutations (1-5% chance)
12. Offspring matures
13. Sell offspring
14. Verify money increased (2-5× base value)

**Expected Result**: Complete breeding loop functional with genetic inheritance.

### Test 4: Companion Interaction
**Systems**: Companion, Loyalty, Abilities, Audio, VFX, UI

1. Select Dog as active pet
2. Position player near dog
3. Press E (pet)
4. Verify animation plays
5. Verify audio (bark sound)
6. Verify VFX (**HEARTS FLOAT UP!**)
7. Verify loyalty increases (+5)
8. Verify UI updates (loyalty bar)
9. Pet cooldown active (30s)
10. Attempt pet again immediately
11. Verify blocked by cooldown
12. Wait 30 seconds
13. Pet again
14. Verify works
15. Activate pet ability (Dog: Fetch)
16. Verify ability activates
17. Verify audio/VFX for ability

**Expected Result**: Petting feels satisfying and rewarding, loyalty system works.

### Test 5: Photo Challenge Completion
**Systems**: Photography, Encyclopedia, Challenges, Economy, UI

1. Accept photo challenge "Photograph a Legendary fish"
2. Catch legendary fish
3. Enter photo mode (P)
4. Frame fish in shot (>30% of frame)
5. Apply filter (optional)
6. Take photo (Space)
7. Photo rated (should be 3+ stars with legendary bonus)
8. Encyclopedia updated (legendary fish entry)
9. Challenge completed
10. Reward granted ($3,000)
11. Verify UI updates (challenge tracker)
12. Verify UI updates (resource display)

**Expected Result**: Photography challenge loop seamless and rewarding.

### Test 6: Idle Offline Loop
**Systems**: Idle, Auto-Fishing, Economy, Aquarium, UI

1. Have idle upgrades: Auto-Fisher, Auto-Sell
2. Set location = Deep Ocean
3. Save and quit game
4. Wait 8 hours (or mock lastLogoutTime)
5. Relaunch game
6. Verify WelcomeBackSystem triggers
7. Verify offline earnings calculated
8. Check summary:
   - Fish caught and sold
   - Money earned (~$4,000-6,000)
   - Aquarium exhibition income
   - Materials gathered
9. Verify money added to balance
10. Verify UI shows updated balance

**Expected Result**: Idle system rewards time away from game appropriately.

### Test 7: Dynamic Event Impact
**Systems**: Events, Fishing, Economy, Audio, VFX

1. Trigger Blood Moon event
2. Verify audio (event music)
3. Verify VFX (red sky, moon, mist)
4. Go fishing
5. Catch fish
6. Verify fish is aberrant (100% during Blood Moon)
7. Verify fish value 10× normal
8. Sell fish
9. Verify money = baseValue × 10
10. Event ends
11. Verify VFX removed
12. Verify music returns to normal
13. Catch fish again
14. Verify normal rarity distribution

**Expected Result**: Events dramatically alter gameplay with full audio/visual support.

---

## 3. PERFORMANCE TESTS

### FPS Target: 60 FPS (mid-range PC)

**Test Conditions**:
- Resolution: 1920x1080
- Quality: High
- Location: Deep Ocean (medium complexity)
- Weather: Rain
- Time: Night (horror effects active)
- Active Systems: Fishing, inventory full, music playing, particles active

**Measurement**:
1. Enable FPS counter (PerformanceMonitor)
2. Play for 30 minutes
3. Record min/avg/max FPS
4. **Target**: Avg FPS ≥ 60, Min FPS ≥ 45

**If FPS < 60**:
- Profile with Unity Profiler
- Identify bottleneck (CPU/GPU/Memory)
- Optimize (reduce particles, LOD, culling)

### Memory Target: <2GB RAM

**Test Conditions**:
- Play for 4 hours continuously
- Catch 200+ fish
- Visit all 13 locations
- Trigger multiple events

**Measurement**:
1. Use Unity Profiler Memory module
2. Check memory usage every 30 minutes
3. **Target**: <2GB, no leaks

**If Memory > 2GB**:
- Check for memory leaks (pooling issues)
- Unload unused assets
- Compress textures/audio

### Load Time Target: <5 seconds

**Test Conditions**:
- Cold start (fresh launch)
- Load saved game with:
  - 100+ fish in inventory
  - 10 tanks with fish
  - All locations unlocked
  - 100+ photos in gallery

**Measurement**:
1. Time from clicking "Load Game" to gameplay
2. **Target**: <5 seconds

**If Load Time > 5 seconds**:
- Async loading
- Load screens
- Lazy loading (load on demand)

---

## 4. BALANCE TESTS

### Economy Balance

**Early Game** (0-2 hours):
- Earnings: $100-300/session
- Expenses: $50-150 (bait, fuel)
- Net: $50-150/session
- **Target**: Can afford first upgrade ($500) after ~4-5 sessions

**Mid Game** (5-10 hours):
- Earnings: $500-2,000/session
- Expenses: $200-500 (crew, licenses)
- Net: $300-1,500/session
- **Target**: Can afford mid-tier upgrades ($2,000-5,000)

**Late Game** (20+ hours):
- Earnings: $5,000-20,000/session (with night premium)
- Expenses: $1,000-3,000 (crew, maintenance)
- Net: $4,000-17,000/session
- **Target**: Can afford endgame content ($10,000 licenses, max upgrades)

### Difficulty Balance

**Fishing Difficulty**:
- Common fish: 5-10 seconds to catch
- Uncommon fish: 10-20 seconds
- Rare fish: 20-40 seconds
- Legendary fish: 40-60 seconds
- **Target**: Feels challenging but fair

**Sanity Management**:
- Day fishing: No sanity drain
- Night fishing: -0.5/s (-30/min)
- With lights upgrade: -0.375/s (-22.5/min)
- Shore regeneration: +0.2/s (+12/min)
- **Target**: Can fish at night for 5-10 minutes safely

**Progression Pace**:
- Tutorial: 15-30 minutes
- First location unlock: 1-2 hours
- All locations unlocked: 15-20 hours
- All upgrades maxed: 30-40 hours
- **Target**: Feels steady, not grindy

---

## 5. USER EXPERIENCE TESTS

### Playability Tests

**First-Time Player** (no tutorial yet):
1. Can they figure out controls within 5 minutes?
2. Do they understand fishing mechanics?
3. Do they understand inventory system?
4. Is the UI intuitive?

**Experienced Player**:
1. Are controls responsive?
2. Is progression satisfying?
3. Are systems deep enough?
4. Is there enough content?

### Fun Factor

**Questions**:
- Is fishing satisfying? (audio/VFX feedback)
- Are night horrors scary/tense?
- Is collecting fish engaging?
- Is breeding rewarding?
- Is photography fun?
- Do you want to keep playing?

**Metrics**:
- Average session length (target: 1-2 hours)
- Retention rate (% returning after 1 day/week)
- Quest completion rate (% finishing quests)
- Achievement rate (% unlocking achievements)

---

## 6. PLATFORM TESTS

### Windows
- [ ] Runs on Windows 10/11
- [ ] Keyboard + mouse controls work
- [ ] Xbox controller works
- [ ] PlayStation controller works
- [ ] Saves persist correctly
- [ ] No crashes over 4-hour session

### macOS (if applicable)
- [ ] Runs on macOS Monterey+
- [ ] All features work
- [ ] Performance acceptable

### Linux (if applicable)
- [ ] Runs on Ubuntu/SteamOS
- [ ] All features work
- [ ] Performance acceptable

### Resolution Tests
- [ ] 720p (1280x720)
- [ ] 1080p (1920x1080)
- [ ] 1440p (2560x1440)
- [ ] 4K (3840x2160)
- [ ] Ultrawide (3440x1440)

### Input Tests
- [ ] Keyboard + Mouse
- [ ] Xbox Controller
- [ ] PlayStation Controller
- [ ] Generic Controller
- [ ] Accessibility Controller

---

## 7. REGRESSION TESTS

After fixing bugs, re-test to ensure:
- Bug is actually fixed
- No new bugs introduced
- Related systems still work

**Regression Test Suite**:
1. Core gameplay loop (fishing → inventory → sell)
2. Save/load functionality
3. All 21 agents' primary features
4. Audio and VFX triggers
5. Settings and accessibility
6. Performance (FPS, memory)

---

## Test Reporting

### Bug Report Template

```markdown
**Bug ID**: BUG-001
**Severity**: Critical / High / Medium / Low
**Status**: Open / In Progress / Fixed / Won't Fix

**Description**:
Clear description of the bug.

**Steps to Reproduce**:
1. Step one
2. Step two
3. Step three

**Expected Behavior**:
What should happen.

**Actual Behavior**:
What actually happens.

**Systems Affected**:
- FishingController
- InventoryManager

**Screenshots/Logs**:
Attach if applicable.

**Notes**:
Additional context.
```

### Test Pass/Fail Criteria

**Pass**: ✅ Feature works as designed, no bugs
**Fail**: ❌ Feature broken, bug found
**Blocked**: ⚠️ Cannot test (missing dependency)

---

## Automated Testing (Optional)

For critical systems, consider Unity Test Framework:

```csharp
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class FishingControllerTests
{
    [Test]
    public void TestCastingTransition()
    {
        // Arrange
        var fishingController = new GameObject().AddComponent<FishingController>();

        // Act
        fishingController.StartFishing();

        // Assert
        Assert.AreEqual(FishingState.Casting, fishingController.CurrentState);
    }
}
```

---

## Testing Schedule

**Week 31**: Unit tests (Phase 1-2 systems)
**Week 32**: Unit tests (Phase 3-5 systems)
**Week 33**: Integration tests
**Week 34**: Performance, balance, UX tests
**Week 35**: Platform and regression tests
**Week 36**: Final smoke tests before launch

---

## Test Coverage Goal

**Target**: 80% of features tested
**Minimum**: 100% of critical features tested

**Critical Features**:
- Fishing mechanics
- Save/load
- Inventory
- Economy
- Audio/VFX triggers
- Settings persistence

---

**Let's ensure Bahnfish is bug-free and polished!** 🐛🔨

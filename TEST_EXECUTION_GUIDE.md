# Test Execution Guide

**For**: Bahnfish Phase 6 - Testing & QA
**Week**: 32-34 (Testing Phase)
**Status**: Ready to execute

---

## Overview

This guide walks you through executing all tests in Bahnfish, from automated unit tests to manual integration testing. Follow this systematically to ensure comprehensive coverage.

---

## Prerequisites

Before testing:
- [ ] Unity 2022 LTS installed
- [ ] Project opens without errors
- [ ] All Phase 1-5 code implemented
- [ ] Audio assets imported (or placeholders)
- [ ] VFX assets imported (or placeholders)
- [ ] BUG_TRACKER.md ready for logging bugs
- [ ] PERFORMANCE_REPORT.md ready for metrics

---

## Testing Schedule

### Week 32: Functional Testing
- Days 1-2: Automated tests (Unit + Build validation)
- Days 3-5: Manual functional testing (all 21 agents)
- Days 6-7: Bug logging and prioritization

### Week 33: Integration & Performance
- Days 1-3: Integration testing (7 scenarios)
- Days 4-7: Performance profiling and optimization

### Week 34: Bug Fixing
- Days 1-7: Fix critical and high-priority bugs

---

## Part 1: Automated Testing (Day 1-2)

### Step 1: Open Unity Test Runner

1. Open Unity Editor
2. Go to **Window → General → Test Runner**
3. You'll see two tabs: **EditMode** and **PlayMode**

### Step 2: Run Build Tests (EditMode)

These tests run in the editor without entering Play mode.

1. Click the **EditMode** tab
2. You should see **BuildTests** with 11 tests:
   - Test_ScenesExistInBuildSettings
   - Test_AllScenesEnabled
   - Test_PlayerSettingsConfigured
   - Test_ColorSpaceIsLinear
   - Test_MultiThreadedRenderingEnabled
   - Test_NoMissingScriptsInResources
   - Test_RequiredFoldersExist
   - Test_AudioSettingsConfigured
   - Test_QualitySettingsExist
   - Test_TargetFrameRateReasonable
   - Test_ScriptingDefineSymbolsValid

3. Click **Run All** (top right)
4. Wait for tests to complete (30 seconds to 1 minute)
5. Check results:
   - ✅ All green = Pass
   - ❌ Any red = Failed (check Console for details)

**Expected Result**: All 11 tests should pass.

**If Tests Fail**:
- Read error message in Console
- Fix the issue (missing scenes, wrong settings, etc.)
- Run tests again until all pass

**Log Results**:
```
Build Tests:
- Total: 11
- Passed: 11
- Failed: 0
- Status: ✅ ALL PASSED
```

### Step 3: Run Core System Tests (PlayMode)

These tests run in Play mode and test runtime behavior.

1. Click the **PlayMode** tab
2. You should see **CoreSystemTests** with 10 tests:
   - Test_GameManagerExists
   - Test_GameManagerSingleton
   - Test_EventSystemPublishSubscribe
   - Test_SaveManagerExists
   - Test_TimeManagerExists
   - Test_TimeManagerTimeProgression
   - Test_WeatherSystemExists
   - Test_WeatherSystemHasValidWeather
   - Test_GameStateInitialized
   - Test_DontDestroyOnLoad

3. Click **Run All**
4. Unity will enter Play mode
5. Tests will run (1-2 minutes)
6. Unity will exit Play mode
7. Check results in Test Runner

**Expected Result**: All 10 tests should pass.

**If Tests Fail**:
- Check which system failed
- Verify the system exists in the scene
- Check Console for null reference errors
- Fix and rerun

**Log Results**:
```
Core System Tests:
- Total: 10
- Passed: 10
- Failed: 0
- Status: ✅ ALL PASSED
```

### Step 4: Run Integration Tests (PlayMode)

These tests verify systems work together.

1. Still in **PlayMode** tab
2. You should see **IntegrationTests** with 7 tests:
   - Integration_CompleteFishingLoop
   - Integration_AtmosphereLoop
   - Integration_HorrorLoop
   - Integration_CompanionLoop
   - Integration_CookingBuffLoop
   - Integration_SaveLoadPersistence
   - Integration_EventLoop

3. Click **Run All**
4. Unity will enter Play mode
5. Tests will run (2-5 minutes)
6. Unity will exit Play mode
7. Check results

**Expected Result**: All 7 tests should pass.

**If Tests Fail**:
- Integration tests are complex
- Check Console for detailed error
- Verify all required systems exist
- Check that systems are properly initialized
- May need to adjust test parameters

**Note**: Some integration tests may fail if placeholder assets are used. Document these as "expected failures" until real assets are integrated.

**Log Results**:
```
Integration Tests:
- Total: 7
- Passed: 7 (or 5-6 with placeholders)
- Failed: 0 (or 1-2 expected with placeholders)
- Status: ✅ PASSED (or ⚠️ MOSTLY PASSED)
```

### Automated Test Summary

Total automated tests: **28 tests**
- Build validation: 11 tests
- Core systems: 10 tests
- Integration: 7 tests

Expected time: **30-60 minutes**

Document results in **TESTING_FRAMEWORK.md** or create **TEST_RESULTS.md**.

---

## Part 2: Manual Functional Testing (Day 3-5)

### Testing Approach

For each system, test:
1. **Happy Path**: Normal use case works
2. **Edge Cases**: Boundary conditions
3. **Error Cases**: Invalid inputs handled
4. **Integration**: Works with other systems

### Day 3: Core & Player Systems

#### Phase 1: Core Architecture

**GameManager**:
- [ ] Game starts without errors
- [ ] GameManager.Instance is accessible
- [ ] Game state initializes correctly
- [ ] DontDestroyOnLoad persists across scenes

**EventSystem**:
- [ ] Subscribe to event
- [ ] Publish event
- [ ] Event received correctly
- [ ] Unsubscribe works

**SaveManager**:
- [ ] Save game (press ESC → Save)
- [ ] Load game (main menu → Load Game)
- [ ] Save file created in correct location
- [ ] Backup saves created (3 total)
- [ ] Corrupted save handled gracefully

**TimeManager**:
- [ ] Time progresses automatically
- [ ] Day/night cycle visible (15 min = 24h)
- [ ] Time periods correct (Day, Dusk, Night, Dawn)
- [ ] Can manually change time (if debug enabled)

**WeatherSystem**:
- [ ] Weather changes occur
- [ ] 4 weather types exist (Clear, Rain, Storm, Fog)
- [ ] Weather forecast shows 3 days
- [ ] Weather affects visuals

**Bug Logging**: Log any issues in BUG_TRACKER.md with severity.

#### Phase 2: Player & Input

**BoatController**:
- [ ] WASD moves boat
- [ ] Mouse rotates camera
- [ ] Shift increases speed
- [ ] Space brakes boat
- [ ] Movement feels smooth
- [ ] Boat doesn't fall through water

**InputManager**:
- [ ] All keys respond
- [ ] Can rebind keys (Settings → Controls)
- [ ] Controller detected (if plugged in)
- [ ] Controller inputs work

**CameraController**:
- [ ] Camera follows boat smoothly
- [ ] Camera doesn't clip through objects
- [ ] Camera shake works (if triggered)

**WaterPhysics**:
- [ ] Boat floats on water
- [ ] Buoyancy feels realistic
- [ ] Boat rocks with waves (if waves enabled)

**PlayerInteraction**:
- [ ] Can interact with objects (press E)
- [ ] Interaction range is 5m
- [ ] Visual feedback shows interactable objects

**Bug Logging**: Document any movement or control issues.

### Day 4: Fishing & Inventory

#### Fishing Mechanics

**FishingController**:
- [ ] Press Left Click to cast
- [ ] Power meter fills (hold Left Click)
- [ ] Rod casts on release
- [ ] Bobber appears in water
- [ ] Wait for fish bite
- [ ] Bobber dips when fish bites
- [ ] Click to hook fish
- [ ] Reeling mechanic works (hold Left Click)
- [ ] Tension meter appears
- [ ] Fish resists (tension increases)
- [ ] Can let out line (release Left Click)
- [ ] Line breaks if tension > 95% too long
- [ ] Successfully catching fish adds to inventory
- [ ] Mini-games work (if applicable)

**Fishing Tools**:
- [ ] Switch tools with Q/E
- [ ] Rod, Net, Harpoon, Dredge all selectable
- [ ] Each tool works differently
- [ ] Visual feedback shows selected tool

**Bait System**:
- [ ] Can attach bait to rod
- [ ] Different bait types affect catch rate
- [ ] Bait is consumed on catch

**Bug Logging**: Fishing is core gameplay - log everything!

#### Inventory System

**InventoryGrid**:
- [ ] Open inventory (press Tab)
- [ ] 10×10 grid visible
- [ ] Fish have correct shapes (1×1, 2×1, etc.)
- [ ] Drag and drop works
- [ ] Can rotate items (press R)
- [ ] Collision detection (can't overlap items)
- [ ] Visual feedback (green=valid, red=invalid)
- [ ] Auto-sort works
- [ ] Quick-stack works

**Bug Logging**: Document any grid placement or rotation issues.

### Day 5: Sanity, Fish AI, UI

#### Sanity & Horror

**SanityManager**:
- [ ] Sanity meter visible in HUD
- [ ] Sanity drains at night (0.5/s)
- [ ] Sanity doesn't drain during day
- [ ] Sanity regenerates near shore
- [ ] Sleep restores sanity to 100

**Night Hazards**:
- [ ] Fish Thief appears at night
- [ ] Fish Thief steals from inventory
- [ ] Obstacles spawn at night
- [ ] Fog reduces visibility
- [ ] Ghost Ship appears (if implemented)

**InsanityEffects**:
- [ ] Screen effects at low sanity (<30%)
- [ ] Screen shake
- [ ] Color desaturation
- [ ] Chromatic aberration
- [ ] Audio distortions

**Bug Logging**: Horror effects are important for atmosphere.

#### Fish AI & Behavior

**FishDatabase**:
- [ ] 60 fish species exist
- [ ] 5 rarity tiers (Common, Uncommon, Rare, Legendary, Aberrant)
- [ ] Each fish has correct value
- [ ] Fish sizes vary

**FishSpawner**:
- [ ] Fish spawn in water
- [ ] Different species spawn at different times
- [ ] Rare fish are actually rare
- [ ] Aberrant fish spawn at night only

**FishBehavior**:
- [ ] Fish swim naturally
- [ ] Schooling behavior visible (for common fish)
- [ ] Aggressive fish more difficult to catch

**Bug Logging**: Document spawn rates and behavior issues.

#### UI/UX

**HUDManager**:
- [ ] Sanity meter visible
- [ ] Time display correct
- [ ] Money/scrap/relics displayed
- [ ] Tension meter during fishing
- [ ] Location name shown
- [ ] Weather icon visible
- [ ] Quest tracker shows active quests

**Menus**:
- [ ] Pause menu works (ESC)
- [ ] Settings accessible
- [ ] All settings persist after restart
- [ ] Controls menu shows key bindings
- [ ] Can rebind all keys

**Bug Logging**: UI bugs affect user experience.

---

## Part 3: Phase 3-5 Systems Testing (Week 32)

### Progression & Economy (Phase 3)

- [ ] Can earn money by selling fish
- [ ] 3 currencies work (money, scrap, relics)
- [ ] Can purchase upgrades
- [ ] Upgrades apply correctly (hull, engine, rod, etc.)
- [ ] Location licenses unlock new areas
- [ ] Dark abilities purchasable with relics
- [ ] Night premium (3-5× values) works

### Narrative & Quests (Phase 3)

- [ ] Quest tracker shows active quests
- [ ] Can accept quests from NPCs
- [ ] Quest objectives track correctly
- [ ] Quest completion works
- [ ] Rewards granted
- [ ] Story progresses through 5 acts
- [ ] NPCs have correct dialogue
- [ ] NPCs follow day/night schedules

### Locations & World (Phase 3)

- [ ] Can travel between 13 locations
- [ ] Fuel consumed during travel
- [ ] Each location has unique fish
- [ ] Secret areas discoverable
- [ ] Fast travel works (with relics)

### Dynamic Events (Phase 3)

- [ ] Blood Moon triggers (10% chance after 10 days)
- [ ] Meteor Shower triggers (30% chance after 3 days)
- [ ] Festivals occur
- [ ] Seasonal migrations happen
- [ ] Event forecast shows upcoming events
- [ ] Events affect gameplay (fish values, spawns, etc.)

### Cooking & Crafting (Phase 4)

- [ ] Can access cooking station
- [ ] 30+ recipes available
- [ ] Cooking works (ingredients consumed)
- [ ] Buffs apply after eating
- [ ] Buff effects work (fishing luck, speed, etc.)
- [ ] Multiple buffs stack correctly
- [ ] Crafting works (bait, tools, upgrades)
- [ ] Preservation methods work

### Aquarium & Breeding (Phase 4)

- [ ] Can purchase aquarium tanks
- [ ] Fish can be placed in tanks
- [ ] Breeding works (select 2 fish, pay cost, wait 24h)
- [ ] Offspring inherit traits
- [ ] Mutations occur (1-5% chance)
- [ ] Exhibition generates passive income
- [ ] Fish care affects happiness

### Companions & Crew (Phase 4)

- [ ] **PETTING MECHANIC WORKS** (press E near companion)
- [ ] Hearts spawn when petting (THIS IS CRITICAL!)
- [ ] Loyalty increases with petting
- [ ] Companion abilities work
- [ ] Can hire crew members
- [ ] Crew provides bonuses
- [ ] Salary paid daily
- [ ] Morale affects performance

### Photography Mode (Phase 4)

- [ ] Press P to enter photo mode
- [ ] Game pauses (time stops)
- [ ] Free camera movement (WASD)
- [ ] 20+ filters available
- [ ] Quality rating system works (1-5 stars)
- [ ] Photos added to encyclopedia
- [ ] Challenges track correctly
- [ ] Can export photos (PNG/JPG, up to 4K)

### Idle/AFK System (Phase 4)

- [ ] Auto-fishing works while offline
- [ ] Welcome back screen shows earnings
- [ ] Idle upgrades purchasable
- [ ] Offline earnings calculated correctly
- [ ] 24-hour cap enforced
- [ ] Comeback bonuses awarded

### Audio System (Phase 5)

- [ ] Music plays and changes with time/events
- [ ] 100+ sound effects play correctly
- [ ] 3D spatial audio works (sounds from correct direction)
- [ ] Volume controls work (5 channels)
- [ ] Audio ducking works (music lowers during important sounds)
- [ ] Audio pooling limits to 32 sources

### Visual Effects (Phase 5)

- [ ] Water splashes on fishing
- [ ] Weather particles (rain, snow, fog)
- [ ] **Petting hearts spawn** (CRITICAL FEATURE!)
- [ ] Horror effects at low sanity
- [ ] Event VFX (Blood Moon, meteors, aurora)
- [ ] Fishing VFX for all stages
- [ ] Particle pooling limits to 10,000

### Accessibility (Phase 5)

- [ ] 8 colorblind modes work
- [ ] UI scaling works (75%-200%)
- [ ] Font sizes adjustable
- [ ] Input remapping works
- [ ] Reduced motion disables shake
- [ ] Subtitles display
- [ ] All 60+ settings persist

---

## Part 4: Integration Testing (Week 33, Days 1-3)

Refer to **TESTING_FRAMEWORK.md** for detailed integration test scenarios.

### Test 1: Complete Fishing Loop
1. Cast rod
2. Wait for bite
3. Reel in fish
4. Fish added to inventory
5. Sell fish at dock
6. Money increases

**Expected**: Entire flow works seamlessly.

### Test 2: Breeding Loop
1. Catch 2 fish
2. Place in breeding tank
3. Start breeding
4. Wait 24 real hours (or fast-forward time for testing)
5. Offspring born
6. Sell offspring

**Expected**: Breeding produces new fish with inherited traits.

### Test 3: Companion Loop
1. Pet companion (press E)
2. Hearts spawn
3. Loyalty increases
4. Use companion ability
5. Ability cooldown starts

**Expected**: Petting increases loyalty, abilities work.

### Test 4: Photography Loop
1. Enter photo mode (P)
2. Take photo of rare fish
3. Photo rated 3-5 stars
4. Fish added to encyclopedia
5. Challenge completed (if applicable)
6. Reward granted

**Expected**: Photo system works end-to-end.

### Test 5: Idle Loop
1. Note current money
2. Purchase idle upgrades
3. Exit game (close Unity)
4. Wait 1 hour (or simulate)
5. Reopen game
6. Welcome back screen shows earnings
7. Money increased

**Expected**: Idle fishing generates income offline.

### Test 6: Atmosphere Loop
1. Wait for time to change to night
2. Music changes to night theme
3. Ambient sounds change
4. Lighting darkens
5. Sanity starts draining

**Expected**: All atmosphere systems sync with time.

### Test 7: Horror Loop
1. Let sanity drop to <30%
2. Visual effects activate
3. Horror audio intensifies
4. Night hazards may spawn
5. Restore sanity
6. Effects diminish

**Expected**: Horror systems respond to sanity level.

### Test 8: Event Loop
1. Trigger Blood Moon event (manually or wait)
2. Music changes to event music
3. Blood Moon VFX activates
4. All fish become aberrant
5. Fish values 10×
6. Event ends after full night
7. Systems return to normal

**Expected**: Events affect all related systems.

---

## Part 5: Performance Testing (Week 33, Days 4-7)

### Step 1: Open Performance Profiler

1. In Unity Editor: **Tools → Performance Profiler**
2. Set duration: 300 seconds (5 minutes)
3. Click **Start Profiling**

### Step 2: Play Game Normally

1. Enter Play mode
2. Play the game for 5 minutes:
   - Move around
   - Fish
   - Navigate menus
   - Trigger VFX
3. Profiler collects metrics automatically

### Step 3: Review Results

After 5 minutes:
- **Average FPS**: Should be 60+ in editor
- **Min FPS**: Should be >30
- **Memory**: Should be <2GB
- **Audio Sources**: Should be ≤32
- **Particles**: Should be ≤10,000

### Step 4: Profile Stress Test

Repeat profiling with maximum stress:
- Night time (sanity low)
- Storm weather
- Blood Moon event
- Multiple fish on screen
- All VFX active

Record if FPS drops below targets.

### Step 5: Memory Leak Test (4 Hours)

1. Start performance profiler (set to 4 hours, or multiple 1-hour sessions)
2. Enter Play mode
3. Let game run for 4 hours
4. Check memory graph

**Expected**: Memory should stabilize, not continuously increase.

If memory increases linearly, there's a memory leak. Check:
- AudioSource not being returned to pool
- Particle systems not being destroyed
- Event subscriptions not being unsubscribed

### Step 6: Document Results

Fill out **PERFORMANCE_REPORT.md** with actual metrics.

---

## Part 6: Balance Testing (Week 34)

### Progression Curve

1. Start new game
2. Play for 2-4 hours
3. Track:
   - Money earned per hour
   - Upgrades affordable
   - Time to unlock each location
   - Difficulty curve

**Expected**: Progression feels smooth, not too slow or fast.

### Economy Balance

- [ ] Can afford first upgrade within 30 minutes
- [ ] Can afford all upgrades within 10-15 hours
- [ ] Night fishing feels rewarding (3-5× values)
- [ ] Idle earnings not exploitable

### Difficulty Balance

- [ ] Fishing difficulty appropriate
- [ ] Rare fish are actually rare
- [ ] Legendary fish feel special
- [ ] Night risk/reward feels fair
- [ ] Sanity drain not too punishing

### Adjustments

If balance feels off, adjust values in:
- `EconomySystem.cs` (fish values, upgrade costs)
- `FishSpawner.cs` (spawn rates)
- `SanityManager.cs` (drain rates)

---

## Part 7: Bug Fixing (Week 34)

### Bug Triage

1. Review all bugs in **BUG_TRACKER.md**
2. Sort by severity:
   - **Critical**: Game crashes, data loss
   - **High**: Major features broken
   - **Medium**: Minor features affected
   - **Low**: Polish issues

### Fix Priority

1. **Week 34 Days 1-3**: Fix ALL critical bugs
2. **Week 34 Days 4-5**: Fix ALL high-priority bugs
3. **Week 34 Days 6-7**: Fix medium bugs if time permits

### Bug Fix Workflow

For each bug:
1. Reproduce bug reliably
2. Identify root cause
3. Implement fix
4. Test fix works
5. Verify fix doesn't break other systems
6. Mark bug as Fixed in BUG_TRACKER.md
7. Commit fix to Git

### Regression Testing

After fixing bugs:
- Run automated tests again (28 tests)
- Retest affected systems
- Verify other systems still work

---

## Testing Checklist Summary

### Week 32: Functional Testing
- [ ] Run 28 automated tests
- [ ] Test all Phase 1-2 systems manually
- [ ] Test all Phase 3-5 systems manually
- [ ] Log all bugs in BUG_TRACKER.md

### Week 33: Integration & Performance
- [ ] Execute 7 integration tests
- [ ] Profile performance (5 min, stress, 4 hour)
- [ ] Document performance metrics
- [ ] Identify optimization targets

### Week 34: Bug Fixing
- [ ] Fix all critical bugs
- [ ] Fix all high-priority bugs
- [ ] Fix medium bugs if time permits
- [ ] Run regression tests

---

## Success Criteria

Testing is complete when:
- [ ] Zero critical bugs
- [ ] Zero high-priority bugs
- [ ] All automated tests pass
- [ ] All integration tests pass
- [ ] Performance targets met (60 FPS, <2GB RAM)
- [ ] All 21 agents' systems functional
- [ ] **Petting mechanic works and hearts spawn!**

---

## Tips for Effective Testing

1. **Test systematically**: Follow checklist, don't skip
2. **Document everything**: Log bugs immediately
3. **Be thorough**: Test edge cases, not just happy path
4. **Test on target hardware**: Don't just test in editor
5. **Get fresh eyes**: Have someone else test too
6. **Take breaks**: Fatigue causes missed bugs
7. **Prioritize**: Fix game-breakers first
8. **Regression test**: Always retest after fixes

---

**Testing is the difference between a broken game and a polished experience. Be thorough!**

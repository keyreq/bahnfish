# Horror System - Agent 7

## Overview

The Horror System manages the sanity/panic mechanics, night hazards, and atmospheric horror elements in Bahnfish. It creates tension through visual/audio distortions, spawns dangerous creatures at night, and implements the fish-stealing mechanics inspired by Dredge.

## Architecture

```
/Horror
  - SanityManager.cs           // Core sanity system
  - InsanityEffects.cs          // Visual/audio distortions
  - CurseSystem.cs              // Persistent negative effects
  - VisualDistortion.cs         // Post-processing effects
  - FogSystem.cs                // Visibility reduction
  /NightHazards
    - FishThiefSpawner.cs       // Crows/phantoms steal fish
    - ObstacleSpawner.cs        // Spontaneous rocks/debris
    - ChaseCreature.cs          // Pursues player
    - Vortex.cs                 // Swirling water hazard
    - GhostShip.cs              // Steals catch
```

## Core Components

### 1. SanityManager.cs
**Central sanity tracking and management**

**Features:**
- Current sanity (0-100, starts at 100)
- Time-based drain rates:
  - Day: 0/sec (completely safe)
  - Dusk: 0.2/sec (transition)
  - Night: 0.5/sec (baseline)
- Lantern upgrades reduce drain (10% per level)
- Talisman temporarily halts drain
- Threshold triggers:
  - Low sanity (<30%): Hazards spawn
  - Critical (<10%): Major dangers
  - Insanity (0%): Curse trigger chance

**Public Methods:**
```csharp
float GetCurrentSanity()                    // Get current sanity value
float GetSanityPercentage()                 // Get as percentage (0-100)
void RestoreSanity(float amount)            // Restore sanity instantly
void FullRestoreSanity()                    // Full restore (docking)
void ActivateTalisman(float duration)       // Temporary protection
void UpgradeLantern()                       // Increase lantern level
bool IsLowSanity()                          // Check if below 30%
bool IsCriticalSanity()                     // Check if below 10%
bool IsInsane()                             // Check if at 0
float GetDrainRate()                        // Current drain rate
```

**Events Published:**
- `"SanityChanged"` - float sanity
- `"InsanityTriggered"` - void
- `"SanityFullyRestored"` - void
- `"TalismanActivated"` - float duration
- `"LanternUpgraded"` - int level

---

### 2. InsanityEffects.cs
**Visual and audio distortions at low sanity**

**Features:**
- **Visual Effects:**
  - Chromatic aberration (color splitting)
  - Vignette darkening (edge shadows)
  - Color desaturation (-50% at 0 sanity)
  - Screen shake (at <30% sanity)
  - Lens distortion

- **Audio Effects:**
  - Pitch shift (0.7x to 1.3x)
  - Reverb (cave preset)
  - Audio distortion

- **Hallucinations:**
  - False fish signals
  - Equipment malfunctions
  - Distorted compass
  - Check every 5 seconds at >30% effect intensity

**Public Methods:**
```csharp
float GetEffectIntensity()                  // Get current intensity (0-1)
void SetEffectIntensity(float intensity)    // Force intensity (testing)
void TriggerScreenShake(float intensity, float duration)
```

**Events Published:**
- `"HallucinationTriggered"` - int type
- `"FalseFishDetected"` - Vector3 position
- `"EquipmentMalfunction"` - float duration
- `"CompassDistorted"` - float angle

---

### 3. Night Hazards

#### FishThiefSpawner.cs
**KEY MECHANIC from Dredge - Makes night dangerous!**

**Features:**
- Spawns crows (anytime at night) and phantoms (low sanity)
- 5% base chance every 10 seconds
- 3x spawn chance at low sanity
- Flies toward player boat
- Steals 1 random fish from inventory
- Visual swooping animation
- Audio cues (crow caw, phantom wail)
- Max 3 active thieves

**Public Methods:**
```csharp
void ForceSpawnThief(bool phantom)          // Testing spawn
void OnFishStolen(string thiefType)         // Called by thief
void OnThiefDestroyed()                     // Cleanup
```

**Events Published:**
- `"FishThiefSpawned"` - string type
- `"FishStolenByThief"` - string type
- `"RemoveRandomFishFromInventory"` - int count

**Integration:**
Agent 6 (Inventory) should subscribe to `"RemoveRandomFishFromInventory"` to actually remove fish.

---

#### ObstacleSpawner.cs
**Spontaneous rocks/debris at low sanity**

**Features:**
- Only spawns when sanity < 30%
- Appears 15-40m ahead of player
- Within 60° cone ahead of boat
- Damages boat on collision (10 damage)
- Fades in over 2 seconds
- Despawns after 30 seconds
- Max 10 active obstacles

**Public Methods:**
```csharp
void ForceSpawnObstacle()                   // Testing spawn
void ClearAllObstacles()                    // Remove all
void OnObstacleCollision(GameObject obstacle)
void RemoveObstacle(GameObject obstacle)
```

**Events Published:**
- `"ObstacleSpawned"` - Vector3 position
- `"BoatDamaged"` - float damage

---

#### ChaseCreature.cs
**Massive creature pursues player at low sanity**

**Features:**
- Spawns when sanity < 20%
- Only at night
- 2 minute cooldown between spawns
- Accelerates from 8m/s to 15m/s
- Damages player when within 8m (5 damage/sec)
- Gives up if player gets 150m away
- Escapes at dock or when sanity > 40%
- Plays chase music during pursuit

**Public Methods:**
```csharp
void ForceSpawn()                           // Testing spawn
void OnCreatureCatch()                      // Called when catches player
void OnCreatureGiveUp()                     // Called when gives up
```

**Events Published:**
- `"ChaseCreatureSpawned"` - Vector3 position
- `"CreatureAttacking"` - void
- `"ChaseCreatureEscaped"` - void
- `"BoatDamaged"` - float damage

**Integration:**
Agent 2 (Player) should handle camera shake and disable controls during attacks.

---

#### Vortex.cs + VortexSpawner.cs
**Swirling water hazards with tentacles**

**Features:**
- Spawns randomly at night (3% chance every 15 seconds)
- Pulls player toward center (5 units/sec)
- Damages in inner radius (3 damage/sec)
- Outer radius: 15m (pull zone)
- Inner radius: 5m (damage zone)
- Spawns 3-6 tentacles around edge
- Lasts 30-60 seconds
- Max 3 active vortexes

**Public Methods:**
```csharp
// VortexSpawner
void ForceSpawn()                           // Testing spawn
```

**Events Published:**
- `"VortexSpawned"` - Vector3 position
- `"VortexDespawned"` - Vector3 position
- `"BoatDamaged"` - float damage

---

#### GhostShip.cs + GhostShipSpawner.cs
**Ethereal ship appears in fog and steals catch**

**Features:**
- Only spawns at night in fog weather
- 5% chance every 20 seconds
- Appears 60m from player
- Steals 25% of catch if player within 20m
- Drifts slowly across water
- Visible for 10 seconds, then disappears
- Ghostly visual effects (50% alpha, blue tint)
- Ghost light pulses

**Public Methods:**
```csharp
// GhostShipSpawner
void ForceSpawn()                           // Testing spawn
```

**Events Published:**
- `"GhostShipAppeared"` - Vector3 position
- `"GhostShipDisappeared"` - Vector3 position
- `"GhostShipStealCatch"` - float percentage

**Integration:**
Agent 6 (Inventory) should subscribe to `"GhostShipStealCatch"` to remove fish.

---

### 4. CurseSystem.cs
**Persistent negative effects from 0 sanity**

**Curse Types:**
1. **Rotting Catch**: Fish spoil 2x faster
2. **Broken Compass**: Navigation distorted by 45°
3. **Haunted Hull**: Random visual/audio glitches
4. **Leaking Boat**: 1 damage/second constant
5. **Tangled Nets**: Fishing speed reduced by 50%

**Features:**
- 30% chance to apply curse at 0 sanity
- Max 3 simultaneous curses
- Must be cleansed by mystic NPC
- Cost: 100 money per curse
- Curses persist across sessions

**Public Methods:**
```csharp
void ApplyCurse(CurseType type)             // Apply specific curse
bool RemoveCurse(CurseType type)            // Remove specific curse
bool CleanseAllCurses()                     // Remove all (costs money)
bool HasCurse(CurseType type)               // Check if cursed
List<CurseType> GetActiveCurses()           // Get all active curses
int GetCurseCount()                         // Number of curses
float GetCleansingCost()                    // Total cost to cleanse
```

**Events Published:**
- `"CurseApplied"` - CurseType
- `"CurseRemoved"` - CurseType
- `"AllCursesCleansed"` - int count
- `"FishSpoilRateModifier"` - float multiplier
- `"CompassDistorted"` - float angle
- `"HauntedGlitch"` - void
- `"BoatDamaged"` - float damage
- `"FishingSpeedModifier"` - float multiplier

---

### 5. VisualDistortion.cs
**Post-processing effects based on sanity**

**Effects:**
- **Chromatic Aberration**: Max 1.0 at 0 sanity
- **Vignette**: Max 0.5 intensity, black
- **Color Grading**: -50 saturation, blue tint
- **Lens Distortion**: -30 at 0 sanity
- **Grain**: 0.5 intensity at 0 sanity
- **Screen Shake**: 0.2 intensity at <30% sanity
- **Pulse Effect**: Breathing effect at >50% intensity

**Public Methods:**
```csharp
float GetEffectIntensity()                  // Current intensity (0-1)
void SetEffectIntensity(float intensity)    // Force intensity (testing)
void TriggerScreenShake(float intensity, float duration)
void SetEffectEnabled(string effectName, bool enabled)
```

**Usage:**
Automatically adjusts based on sanity. No manual management needed.

---

### 6. FogSystem.cs
**Dynamic fog based on time, weather, and sanity**

**Fog Density:**
- **Day**: 0.0 (clear)
- **Dusk**: 0.005
- **Night**: 0.01
- **Dawn**: 0.008

**Weather Multipliers:**
- Clear: 1x
- Rain: 2x
- Storm: 1.5x
- Fog: 5x (very heavy)

**Sanity Multiplier:**
- Above 30% sanity: 1x
- Below 30% sanity: Up to 2x (increases fog)

**Visibility Effects:**
- Reduces camera far clip plane
- Tints fog color (blue at night, purple at low sanity)
- Smooth transitions

**Public Methods:**
```csharp
float GetCurrentFogDensity()                // Current fog density
float GetVisibilityRange()                  // Approx visibility in meters
bool IsReducedVisibility()                  // True if fog > 0.005
void SetFogEnabled(bool enabled)            // Enable/disable fog
void SetFogDensity(float density)           // Manual control (testing)
```

---

## Setup Instructions

### 1. Scene Setup

Create empty GameObject named "Horror Manager":
1. Add `SanityManager`
2. Add `InsanityEffects`
3. Add `CurseSystem`
4. Add `VisualDistortion`
5. Add `FogSystem`

Create "Night Hazard Spawners" GameObject:
1. Add `FishThiefSpawner`
2. Add `ObstacleSpawner`
3. Add `ChaseCreature`
4. Add `VortexSpawner`
5. Add `GhostShipSpawner`

### 2. Post-Processing Setup

1. Install Post Processing Stack v2 from Package Manager
2. Create PostProcessVolume in scene
3. Set as Global
4. Assign to `InsanityEffects.postProcessVolume`
5. Assign to `VisualDistortion.postProcessVolume`

### 3. Prefab Requirements

Create and assign these prefabs:

**FishThiefSpawner:**
- `crowPrefab` - Crow model with wings
- `phantomPrefab` - Ghostly humanoid figure
- Add `FishThief` component to both

**ObstacleSpawner:**
- `rockPrefabs[]` - Various rock models
- `debrisPrefabs[]` - Floating debris models
- Add colliders to all

**ChaseCreature:**
- `creaturePrefab` - Large sea monster
- Add collider and rigidbody

**VortexSpawner:**
- `vortexPrefab` - Swirling water effect
- `tentaclePrefabs[]` - Tentacle models
- Add particle systems

**GhostShipSpawner:**
- `ghostShipPrefab` - Ship model
- Add particle system for fog
- Add light component

### 4. Audio Setup

Assign audio clips:

**SanityManager:**
- (None required)

**FishThiefSpawner:**
- `crowCawSound`
- `phantomWailSound`
- `fishStolenSound`

**ObstacleSpawner:**
- `obstacleAppearSound`
- `obstacleCollisionSound`

**ChaseCreature:**
- `creatureRoarSound`
- `creatureChaseMusic`
- `creatureAttackSound`

**Vortex:**
- `vortexAmbientSound`
- `tentacleAttackSound`

**GhostShip:**
- `ghostShipAmbient`
- `stealSound`
- `laughSound`

**CurseSystem:**
- `curseAppliedSound`
- `curseRemovedSound`

---

## Integration with Other Agents

### Agent 1 (Core)
- Updates `GameState.sanity` continuously
- Uses `EventSystem` for all communication

### Agent 2 (Player)
```csharp
// Subscribe to damage events
EventSystem.Subscribe<float>("BoatDamaged", OnBoatDamaged);

// Subscribe to camera shake
EventSystem.Subscribe("CreatureAttacking", OnCreatureAttack);

void OnCreatureAttack() {
    // Disable controls temporarily
    // Trigger camera shake
}
```

### Agent 3 (Time/Environment)
- Horror system automatically subscribes to:
  - `"TimeOfDayChanged"`
  - `"WeatherChanged"`
- No manual integration needed

### Agent 6 (Inventory)
```csharp
// Subscribe to fish stealing
EventSystem.Subscribe<int>("RemoveRandomFishFromInventory", RemoveRandomFish);
EventSystem.Subscribe<float>("GhostShipStealCatch", StealPercentage);
EventSystem.Subscribe<float>("FishSpoilRateModifier", UpdateSpoilRate);

void RemoveRandomFish(int count) {
    // Remove 'count' random fish from inventory
}

void StealPercentage(float percentage) {
    // Remove percentage of total fish
}
```

### Agent 11 (UI)
```csharp
// Subscribe to sanity changes
EventSystem.Subscribe<float>("SanityChanged", UpdateSanityMeter);

// Subscribe to curse events
EventSystem.Subscribe<CurseType>("CurseApplied", ShowCurseNotification);

// Display active curses
List<CurseType> curses = CurseSystem.Instance.GetActiveCurses();
```

### Agent 12 (Audio)
```csharp
// Subscribe to insanity events for audio distortion
EventSystem.Subscribe<float>("SanityChanged", UpdateAudioDistortion);

void UpdateAudioDistortion(float sanity) {
    float intensity = 1f - (sanity / 100f);
    AudioMixer.SetFloat("Pitch", 1f - (intensity * 0.3f));
}
```

---

## Event Reference

### Published Events

**Sanity:**
- `"SanityChanged"` (float) - Current sanity value
- `"InsanityTriggered"` (void) - Reached 0 sanity
- `"SanityFullyRestored"` (void) - Back to 100
- `"TalismanActivated"` (float) - Protection duration
- `"LanternUpgraded"` (int) - New level

**Hazards:**
- `"FishThiefSpawned"` (string) - Thief type
- `"FishStolenByThief"` (string) - Thief type
- `"ObstacleSpawned"` (Vector3) - Position
- `"ChaseCreatureSpawned"` (Vector3) - Position
- `"CreatureAttacking"` (void) - Player caught
- `"ChaseCreatureEscaped"` (void) - Creature retreated
- `"VortexSpawned"` (Vector3) - Position
- `"VortexDespawned"` (Vector3) - Position
- `"GhostShipAppeared"` (Vector3) - Position
- `"GhostShipDisappeared"` (Vector3) - Position
- `"BoatDamaged"` (float) - Damage amount

**Curses:**
- `"CurseApplied"` (CurseType) - New curse
- `"CurseRemoved"` (CurseType) - Removed curse
- `"AllCursesCleansed"` (int) - Count removed
- `"FishSpoilRateModifier"` (float) - Multiplier
- `"CompassDistorted"` (float) - Angle offset
- `"HauntedGlitch"` (void) - Visual glitch
- `"FishingSpeedModifier"` (float) - Speed multiplier

**Hallucinations:**
- `"HallucinationTriggered"` (int) - Type
- `"FalseFishDetected"` (Vector3) - Position
- `"EquipmentMalfunction"` (float) - Duration
- `"CompassDistorted"` (float) - Angle

**Inventory Integration:**
- `"RemoveRandomFishFromInventory"` (int) - Count
- `"GhostShipStealCatch"` (float) - Percentage

---

## Testing & Debug

### Debug Controls

Add to a test script:

```csharp
void Update() {
    // Sanity controls
    if (Input.GetKeyDown(KeyCode.Alpha1))
        SanityManager.Instance.SetSanity(100f);
    if (Input.GetKeyDown(KeyCode.Alpha2))
        SanityManager.Instance.SetSanity(50f);
    if (Input.GetKeyDown(KeyCode.Alpha3))
        SanityManager.Instance.SetSanity(0f);

    // Force spawn hazards
    if (Input.GetKeyDown(KeyCode.F))
        FishThiefSpawner.Instance.ForceSpawnThief(false);
    if (Input.GetKeyDown(KeyCode.G))
        ObstacleSpawner.Instance.ForceSpawnObstacle();
    if (Input.GetKeyDown(KeyCode.H))
        ChaseCreature.Instance.ForceSpawn();
    if (Input.GetKeyDown(KeyCode.J))
        VortexSpawner.Instance.ForceSpawn();
    if (Input.GetKeyDown(KeyCode.K))
        GhostShipSpawner.Instance.ForceSpawn();

    // Apply curse
    if (Input.GetKeyDown(KeyCode.C))
        CurseSystem.Instance.ApplyCurse(CurseType.RottingCatch);

    // Talisman
    if (Input.GetKeyDown(KeyCode.T))
        SanityManager.Instance.ActivateTalisman(30f);
}
```

### Debug Visualization

Enable these in Inspector:
- `ObstacleSpawner.visualizeSpawnRange` - Shows spawn cone
- `Vortex.visualizeRadius` - Shows vortex radii
- `GhostShip.visualizeRadius` - Shows steal radius
- `FogSystem.showFogDebug` - Shows fog info overlay

### Expected Behavior

**Day Fishing (Sanity 100%):**
- No sanity drain
- No hazards spawn
- Clear visibility
- No visual effects

**Night Fishing (Sanity 100% → 70%):**
- Gradual sanity drain (0.5/sec)
- Occasional crow spawns
- Slight fog increase
- Minimal visual effects

**Low Sanity (70% → 30%):**
- Faster hazard spawns
- Phantoms appear
- Obstacles may spawn
- Noticeable visual distortion
- Heavy fog

**Critical Sanity (30% → 0%):**
- Frequent hazards
- Chase creature may spawn
- Heavy visual distortion
- Very reduced visibility
- Hallucinations occur

**Insanity (0%):**
- Curse may be applied
- All hazards active
- Maximum visual distortion
- Extremely dangerous

**At Dock:**
- Full sanity restore
- All hazards despawn
- Safe zone

---

## Performance Considerations

### Optimization Tips:

1. **Hazard Pooling**: Consider object pooling for frequently spawned hazards
2. **Distance Culling**: Despawn hazards far from player
3. **Max Limits**: Enforced max counts prevent spawn spam
4. **Effect LOD**: Reduce effect quality at low frame rates
5. **Audio Limiting**: Max 1 of each audio type playing simultaneously

### Performance Targets:

- Max 3 fish thieves
- Max 10 obstacles
- Max 1 chase creature
- Max 3 vortexes
- Max 1 ghost ship
- Total: ~20 max active hazards

---

## Balancing Guidelines

### Sanity Drain Rates:
Current balanced rates (per VIDEO_ANALYSIS.md):
- Day: 0/sec - Completely safe
- Dusk: 0.2/sec - 5 minutes to drain from 100%
- Night: 0.5/sec - 3 minutes 20 seconds to drain
- With lantern L5: 0.25/sec night - 6 minutes 40 seconds

### Hazard Spawn Chances:
- Fish Thieves: 5% base, 15% at low sanity
- Obstacles: 10% at <30% sanity
- Chase Creature: Guaranteed at <20% sanity (if cooldown complete)
- Vortex: 3% at night
- Ghost Ship: 5% at night + fog

### Damage Values:
- Obstacle collision: 10 damage
- Vortex: 3 damage/sec
- Chase creature: 5 damage/sec when caught
- Leaking boat curse: 1 damage/sec

### Playtesting Notes:
- First night should feel tense but manageable
- Players should WANT to return before dark
- Risk/reward balanced (night fish worth 3-5x)
- Escape is always possible (not instant death)
- Curses are rare but impactful

---

## Common Issues & Solutions

**Issue**: Sanity not draining at night
- Check TimeManager is publishing events
- Verify SanityManager is subscribed
- Ensure time is not paused

**Issue**: No hazards spawning
- Check player has "Player" tag
- Verify prefabs are assigned
- Check sanity thresholds
- Ensure night time

**Issue**: Post-processing not working
- Install Post Processing Stack v2
- Create PostProcessVolume
- Assign to components
- Check camera has PostProcessLayer

**Issue**: Fish not being stolen
- Agent 6 must subscribe to events
- Check inventory system is active
- Verify event published

**Issue**: Fog too thick/thin
- Adjust density multipliers
- Check weather system integration
- Verify fog enabled in Quality Settings

---

## Future Enhancements

### Possible Additions:
1. More creature types (serpents, leviathans)
2. Weather-specific hazards (lightning in storms)
3. Sanity-restoring items (tea, meals)
4. Crew member buffs (mystic reduces drain)
5. Cursed fishing spots (high risk/reward)
6. Blood moon event (maximum hazards)
7. Eldritch fish that drain sanity
8. Asylum achievement (survive 0 sanity for 5 min)

---

## Credits

**Agent 7: Sanity & Horror System**
- Inspired by Dredge's panic mechanics
- Fish-stealing mechanic from VIDEO_ANALYSIS.md
- Balanced according to GAME_DESIGN.md
- Integrated with Agent 1, 2, 3, 6, 11, 12

**Key Mechanics:**
✅ Sanity drains at night
✅ Fish thieves steal catch (Dredge-inspired)
✅ Spontaneous obstacles
✅ Chase sequences
✅ Persistent curses
✅ Visual/audio horror
✅ Dynamic fog

**Status**: Complete
**Date**: 2026-03-01

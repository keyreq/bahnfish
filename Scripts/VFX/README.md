# Bahnfish VFX System - Complete Documentation

**Agent 13: Visual Effects & Particles Specialist**
**Implementation Date**: Week 25-26
**Version**: 1.0.0

## Table of Contents
1. [System Overview](#system-overview)
2. [Architecture](#architecture)
3. [VFX Components](#vfx-components)
4. [Quality System](#quality-system)
5. [Particle Pooling](#particle-pooling)
6. [Integration Guide](#integration-guide)
7. [Event Reference](#event-reference)
8. [Performance Optimization](#performance-optimization)
9. [Artist Guidelines](#artist-guidelines)

---

## System Overview

The Bahnfish VFX system provides a comprehensive visual effects framework covering:

- **Water Effects**: Splashes, wake trails, ripples, foam, caustics
- **Weather Particles**: Rain, snow, fog, lightning, wind
- **Fishing VFX**: Casting, reeling, catch success, line break
- **Horror Effects**: Sanity distortion, night hazards, hallucinations
- **Event VFX**: Blood Moon, meteor showers, aurora, festivals
- **Companion VFX**: Petting hearts (THE KEY FEATURE!), pet abilities
- **Fish AI Visuals**: Trails, school shimmer, rarity auras
- **Inventory VFX**: Pickup, drag, sell, craft effects
- **UI Particles**: Achievements, notifications, level ups
- **Post-Processing**: Bloom, vignette, chromatic aberration, DOF

### Key Features

- **Particle Pooling**: Efficient reuse of particle systems
- **Quality LOD**: 4 quality levels (Low/Medium/High/Ultra)
- **Auto-Quality**: Automatic adjustment based on FPS
- **Event-Driven**: Loose coupling via EventSystem
- **Save/Load**: Persistent VFX settings
- **Performance Tracking**: Real-time monitoring

---

## Architecture

### Component Hierarchy

```
VFXManager (Singleton)
├── WaterEffects
├── WeatherParticles
├── FishingVFX
├── HorrorVFX
├── EventVFX
├── FishAIVisuals
├── CompanionVFX
├── InventoryVFX
├── UIParticleEffects
└── PostProcessingManager
```

### Core Systems

#### VFXManager
Central hub that manages all VFX subsystems and particle pooling.

**Responsibilities**:
- Particle system pooling
- Quality management
- Performance tracking
- Subsystem coordination

**Key Methods**:
```csharp
ParticleSystem SpawnEffect(string particleID, Vector3 position)
void RegisterParticlePrefab(string particleID, GameObject prefab)
void SetQuality(VFXQuality quality)
void SetParticlesEnabled(bool enabled)
VFXData GetVFXData() // For saving
void LoadVFXData(VFXData data) // For loading
```

---

## VFX Components

### 1. WaterEffects

Manages water-related visual effects.

**Features**:
- Splashes (small, medium, large)
- Wake trails behind boats
- Interactive ripples
- Foam particles
- Underwater bubbles
- Water caustics (shader-based)

**Quality Levels**:
- **Low**: Simple splashes only
- **Medium**: Splashes + ripples + particle wake
- **High**: + Foam + enhanced ripples
- **Ultra**: Full mesh wake + enhanced foam

**Example Usage**:
```csharp
WaterEffects waterEffects = VFXManager.Instance.GetWaterEffects();

// Create splash
waterEffects.CreateSplash(position, SplashSize.Medium, intensity: 1f);

// Create ripple
waterEffects.CreateRipple(position, intensity: 0.5f);

// Create bubbles
waterEffects.CreateBubbles(underwaterPosition, count: 10);
```

### 2. WeatherParticles

Manages all weather particle effects.

**Features**:
- Rain (light, medium, heavy)
- Snow (gentle, blizzard)
- Fog (volumetric)
- Lightning flashes
- Wind debris
- Mist particles

**Weather Types**:
- `WeatherType.Clear`: No particles, minimal fog
- `WeatherType.Rain`: Rain particles + mist
- `WeatherType.Storm`: Heavy rain + lightning + wind
- `WeatherType.Fog`: Dense fog + mist particles

**Example Usage**:
```csharp
// Weather changes automatically via event system
EventSystem.Publish("WeatherChanged", WeatherType.Storm);

// Or manually
WeatherParticles weather = VFXManager.Instance.GetWeatherParticles();
// Weather will respond to event
```

### 3. FishingVFX

Manages fishing gameplay visual effects.

**Features**:
- Casting line trail and splash
- Bobber idle ripples
- Tension sparkles along line
- Warning particles at high tension
- Fish jump splashes and droplets
- Catch success effects (rarity-based)
- Line break effects

**Rarity Effects**:
- **Common**: Small sparkle
- **Uncommon**: Green glow
- **Rare**: Blue aura burst
- **Legendary**: Gold radial burst + rainbow spray
- **Aberrant**: Dark purple distortion

**Example Usage**:
```csharp
// Effects triggered automatically via events:
EventSystem.Publish("FishingCastStarted", new CastingData(...));
EventSystem.Publish("FishHooked", new FishHookedData(...));
EventSystem.Publish("FishCaught", new FishCaughtData(...));
```

### 4. HorrorVFX

Manages horror atmosphere and sanity effects.

**Features**:
- Sanity-based screen distortion (vignette, chromatic aberration)
- Screen shake at low sanity
- Hallucination particles (shadows, eyes)
- Night hazard effects (fish thief, fog, ghost ship, whisperer)
- Cursed fish auras

**Sanity Levels**:
- **80-100%**: Clean visuals, no effects
- **40-80%**: Slight vignette, 90% saturation
- **10-40%**: Heavy vignette, chromatic aberration, screen shake, hallucinations
- **<10%**: Extreme effects, tunnel vision, black & white

**Example Usage**:
```csharp
HorrorVFX horrorVFX = VFXManager.Instance.GetHorrorVFX();

// Sanity effects triggered automatically
EventSystem.Publish("SanityChanged", newSanityValue);

// Manual hazard effects
horrorVFX.CreateFishThiefEffect(position);
horrorVFX.CreateGhostShipEffect(ghostShipObject);
horrorVFX.CreateCursedAura(cursedFishObject);
```

### 5. EventVFX

Manages dynamic event visual effects.

**Features**:
- **Blood Moon**: Red sky, fog, mist, moon object
- **Meteor Shower**: Meteors, trails, impacts, splashes
- **Aurora Borealis**: Procedural aurora waves, reflection
- **Festival**: Fireworks, floating lanterns, confetti

**Example Usage**:
```csharp
// Events triggered automatically
EventSystem.Publish("DynamicEventStarted", "BloodMoon");
EventSystem.Publish("DynamicEventEnded", "BloodMoon");

// Or manually
EventVFX eventVFX = VFXManager.Instance.GetEventVFX();
eventVFX.StartBloodMoon();
eventVFX.StopBloodMoon();
```

### 6. CompanionVFX - THE KEY FEATURE!

Manages companion and pet visual effects, including THE PETTING HEARTS!

**Petting Effects** (Cast n Chill inspired):
- 3-5 hearts spawn and float upward
- Heart color matches pet type
- Sparkle burst around pet
- Warm glow effect
- Loyalty increase sparkles

**Pet Ability Effects**:
- **Dog Fetch**: Retrieving trail
- **Cat Stealth**: Fade to translucent
- **Seabird Scout**: Vision cone
- **Otter Dive**: Splash + bubbles
- **Hermit Crab Shell**: Protective dome
- **Ghost Phase**: Ethereal fade + trail

**Example Usage**:
```csharp
// Petting hearts triggered automatically
EventSystem.Publish("PetPetted", new PetPettedData(
    petID: "dog_1",
    petType: "dog",
    petPosition: dogPosition,
    currentLoyalty: 75f
));

// Pet abilities
EventSystem.Publish("PetAbilityActivated", new PetAbilityData(
    petID: "cat_1",
    petType: "cat",
    abilityName: "Stealth",
    position: catPosition,
    petObject: catGameObject
));
```

### 7. FishAIVisuals

Manages fish visual effects.

**Features**:
- Bioluminescent swimming trails
- School shimmer effects
- Rarity-based auras (uncommon, rare, legendary)
- Aberrant distortion effects

**Example Usage**:
```csharp
FishAIVisuals fishVisuals = VFXManager.Instance.GetFishAIVisuals();

// Create fish trail
ParticleSystem trail = fishVisuals.CreateFishTrail(fishObject, isBioluminescent: true);

// Create rarity aura
ParticleSystem aura = fishVisuals.CreateFishAura(fishObject, FishRarity.Legendary, isAberrant: false);

// School shimmer
fishVisuals.CreateSchoolShimmer(schoolCenterPosition, fishCount: 10);
```

### 8. InventoryVFX

Manages inventory interaction effects.

**Features**:
- Item pickup glow (rarity-colored)
- Valid/invalid placement feedback
- Sell coin particles
- Craft sparkles and success burst

**Example Usage**:
```csharp
InventoryVFX inventoryVFX = VFXManager.Instance.GetInventoryVFX();

// Item pickup
inventoryVFX.CreateItemPickupEffect(itemPosition, FishRarity.Rare);

// Placement feedback
inventoryVFX.CreatePlacementFeedback(gridPosition, isValid: true);

// Sell
inventoryVFX.CreateSellEffect(shopPosition, value: 150f);

// Craft
inventoryVFX.CreateCraftEffect(stationPosition, itemID: "upgraded_rod");
```

### 9. UIParticleEffects

Manages UI visual effects.

**Features**:
- Achievement fanfare + stars
- Notification glow (info, warning, error)
- Quest complete confetti
- Level up radial burst + numbers
- Button hover glow

**Example Usage**:
```csharp
UIParticleEffects uiVFX = VFXManager.Instance.GetUIParticleEffects();

// Achievement
uiVFX.CreateAchievementEffect(screenPosition);

// Notification
uiVFX.CreateNotificationEffect(screenPosition, NotificationType.Warning);

// Level up
uiVFX.CreateLevelUpEffect(screenPosition, newLevel: 5);
```

### 10. PostProcessingManager

Manages all post-processing effects.

**Features**:
- Bloom (water reflections, sunlight)
- Vignette (horror, focus)
- Chromatic aberration (sanity distortion)
- Color grading (time of day, mood)
- Depth of field (photography mode)
- Motion blur (high-speed movement)
- Screen space reflections (water)
- Ambient occlusion

**Quality Presets**:
- **Low**: Bloom OFF, SSR OFF, AO OFF, Motion Blur OFF
- **Medium**: Bloom 30%, AO 50%, SSR OFF
- **High**: Bloom 50%, AO 100%, SSR ON, Motion Blur 30%
- **Ultra**: All effects ON, maximum quality

**Example Usage**:
```csharp
PostProcessingManager postProcessing = VFXManager.Instance.GetPostProcessingManager();

// Manual control
postProcessing.SetVignetteIntensity(0.5f);
postProcessing.SetChromaticAberration(0.3f);
postProcessing.SetColorSaturation(0.8f);
postProcessing.SetBloomIntensity(0.7f);

// Special effects
postProcessing.FlashScreen(Color.white, duration: 0.2f);
postProcessing.ApplyDamageEffect();

// Photography mode DOF
postProcessing.SetDepthOfField(enabled: true, focusDistance: 10f, aperture: 2.8f);
```

---

## Quality System

### VFXQuality Enum

```csharp
public enum VFXQuality
{
    Low = 0,      // 20% density, essential effects only
    Medium = 1,   // 40% density, reduced particle count
    High = 2,     // 70% density, most effects enabled
    Ultra = 3     // 100% density, all effects maximum
}
```

### Setting Quality

```csharp
// Set quality
VFXManager.Instance.SetQuality(VFXQuality.High);

// Auto-quality adjustment (enabled by default)
// Automatically adjusts quality based on FPS performance
```

### Quality Impact

| Feature | Low | Medium | High | Ultra |
|---------|-----|--------|------|-------|
| Particle Density | 20% | 40% | 70% | 100% |
| Water Wake | None | Particle | Particle | Mesh |
| Foam | OFF | OFF | ON | ON (Enhanced) |
| Weather Particles | Basic | Standard | Full | Full + Wind |
| Lightning | OFF | ON | ON | ON |
| Hallucinations | OFF | ON | ON | ON |
| Post-Processing | Minimal | Medium | Full | Ultra |
| Bloom | OFF | 30% | 50% | 70% |
| SSR | OFF | OFF | ON | ON |

---

## Particle Pooling

### How It Works

The VFXManager uses object pooling to recycle particle systems, reducing instantiation overhead.

**Benefits**:
- Reduced garbage collection
- Improved performance
- Consistent frame rates
- Scalable particle counts

**Configuration**:
```csharp
[SerializeField] private int defaultPoolSize = 20;      // Initial pool size
[SerializeField] private int maxPoolSize = 100;         // Max cached particles
[SerializeField] private float poolCleanupInterval = 30f; // Cleanup frequency
```

### Registering Prefabs

```csharp
// In each VFX subsystem
vfxManager.RegisterParticlePrefab("particle_id", prefabReference);
```

### Spawning From Pool

```csharp
// Automatically pooled
ParticleSystem ps = vfxManager.SpawnEffect("particle_id", position);

// With rotation
ParticleSystem ps = vfxManager.SpawnEffect("particle_id", position, rotation);
```

### Pool Lifecycle

1. **First Spawn**: Instantiate new particle system
2. **On Stop**: Return to pool (inactive)
3. **Next Spawn**: Reuse from pool
4. **Cleanup**: Remove excess particles every 30 seconds

---

## Integration Guide

### Step 1: Setup VFX Manager

```csharp
// VFXManager is a singleton and initializes automatically
// Access it from anywhere:
VFXManager vfxManager = VFXManager.Instance;
```

### Step 2: Assign Particle Prefabs

In Unity Editor:
1. Select VFXManager in hierarchy
2. Expand each VFX subsystem
3. Assign particle prefabs to appropriate fields

### Step 3: Publish Events

```csharp
// Fishing example
EventSystem.Publish("FishingCastStarted", new CastingData(
    startPosition: rodTip.position,
    targetPosition: targetWaterPosition,
    castPower: 0.8f
));

// Petting example (THE KEY FEATURE!)
EventSystem.Publish("PetPetted", new PetPettedData(
    petID: currentPet.petID,
    petType: currentPet.petType,
    petPosition: currentPet.transform.position,
    currentLoyalty: currentPet.loyalty
));

// Sanity example
EventSystem.Publish("SanityChanged", newSanityValue);
```

### Step 4: Load/Save Settings

```csharp
// Save
VFXData vfxData = VFXManager.Instance.GetVFXData();
SaveData saveData = new SaveData();
saveData.vfxData = vfxData;
SaveSystem.Instance.SaveGame(saveData);

// Load
SaveData loadedData = SaveSystem.Instance.LoadGame();
VFXManager.Instance.LoadVFXData(loadedData.vfxData);
```

---

## Event Reference

### Events Published BY VFX System

| Event Name | Data Type | Description |
|------------|-----------|-------------|
| `VFXSpawned` | `string` | Particle effect spawned (particleID) |
| `VFXQualityChanged` | `VFXQuality` | Quality level changed |
| `ParticlesToggled` | `bool` | Particles enabled/disabled |
| `PostProcessingToggled` | `bool` | Post-processing enabled/disabled |

### Events Subscribed TO by VFX System

| Event Name | Data Type | VFX Component |
|------------|-----------|---------------|
| `PlayerMoved` | `PlayerMovedEventData` | WaterEffects (wake trails) |
| `FishingCastStarted` | `CastingData` | FishingVFX |
| `FishingBobberLanded` | `Vector3` | FishingVFX, WaterEffects |
| `FishHooked` | `FishHookedData` | FishingVFX |
| `FishingTensionChanged` | `float` | FishingVFX |
| `FishJumped` | `FishJumpData` | FishingVFX, WaterEffects |
| `FishCaught` | `FishCaughtData` | FishingVFX |
| `LineBroken` | — | FishingVFX |
| `FishingEnded` | — | FishingVFX |
| `WeatherChanged` | `WeatherType` | WeatherParticles, WaterEffects |
| `TimeChanged` | `TimeChangedEventData` | WeatherParticles, PostProcessingManager |
| `LightningFlash` | `Vector3` | Audio system (thunder) |
| `SanityChanged` | `float` | HorrorVFX, PostProcessingManager |
| `NightHazardSpawned` | `HazardSpawnData` | HorrorVFX |
| `FishThiefApproaching` | `string` | HorrorVFX |
| `WhispererActive` | `Vector3` | HorrorVFX |
| `DynamicEventStarted` | `string` | EventVFX, PostProcessingManager |
| `DynamicEventEnded` | `string` | EventVFX |
| `FireworkExploded` | `Vector3` | Audio system |
| `PetPetted` | `PetPettedData` | CompanionVFX (THE KEY FEATURE!) |
| `PetAbilityActivated` | `PetAbilityData` | CompanionVFX |
| `LoyaltyChanged` | `LoyaltyChangedEventData` | CompanionVFX |
| `ItemPickedUp` | `ItemPickupData` | InventoryVFX |
| `ItemSold` | `ItemSoldData` | InventoryVFX |
| `ItemCrafted` | `ItemCraftedData` | InventoryVFX |
| `AchievementUnlocked` | `string` | UIParticleEffects |
| `QuestCompleted` | `string` | UIParticleEffects |
| `PlayerLevelUp` | `int` | UIParticleEffects |
| `PhotographyModeToggled` | `bool` | PostProcessingManager |

---

## Performance Optimization

### Optimization Techniques

1. **Particle Pooling**: Reuse instead of instantiate
2. **Quality LOD**: Adjust based on performance
3. **Distance Culling**: Don't render distant particles
4. **Frustum Culling**: Only render visible particles
5. **Max Particle Cap**: Limit simultaneous particles (default: 10,000)
6. **Auto-Quality**: Automatically reduce quality if FPS drops

### Performance Settings

```csharp
[Header("Performance Settings")]
[SerializeField] private int maxSimultaneousParticles = 10000;
[SerializeField] private float cullingDistance = 100f;
[SerializeField] private bool autoQualityAdjust = true;
[SerializeField] private int targetFPS = 60;
```

### Monitoring Performance

```csharp
// Get performance stats
string stats = VFXManager.Instance.GetPerformanceStats();
// Output: "FPS: 58.3 | Particles: 4523/10000 | Quality: High"

// Debug display (only in debug builds)
// Stats automatically shown in top-left corner
```

### Optimization Tips

- Use lower quality on mobile platforms
- Reduce particle density in crowded scenes
- Disable post-processing on low-end hardware
- Use sprite-based particles instead of mesh particles for distant effects
- Combine multiple small emitters into one larger system
- Disable particles entirely in menus/UI screens

---

## Artist Guidelines

### Creating Particle Prefabs

**Requirements**:
- Must have a `ParticleSystem` component on root GameObject
- Use consistent naming: `VFX_{Category}_{Effect}` (e.g., `VFX_Water_Splash_Medium`)
- Set "Stop Action" to "Disable" for pooling compatibility
- Keep particle count reasonable (< 500 per system)
- Use appropriate culling mode
- Set initial color/size in particle system (code can override)

**Recommended Structure**:
```
VFX_Water_Splash_Medium/
├── ParticleSystem (main splash)
├── Droplets (child particle system)
└── Foam (child particle system)
```

### Color Palettes

**Rarity Colors**:
- Common: White/Grey
- Uncommon: Green (#00FF00)
- Rare: Blue (#0080FF)
- Legendary: Gold/Yellow (#FFD700)
- Aberrant: Dark Purple (#800080)

**Time of Day Colors**:
- Dawn: Warm orange (#FFA050)
- Day: Bright white (#FFFFFF)
- Dusk: Orange-red (#FF8040)
- Night: Cool blue (#5080C0)

**Horror Colors**:
- Sanity effects: Black/Red (#000000, #FF0000)
- Cursed: Dark purple (#400040)
- Ghost: Pale blue (#C0E0FF)

### Particle System Presets

**Splash Effects**:
- Duration: 0.5-1 second
- Emission: Burst (20-50 particles)
- Shape: Cone (15-30 degree angle)
- Speed: 2-5 m/s

**Trail Effects**:
- Duration: Looping
- Emission: 10-30 per second
- Shape: Sphere or Cone
- Speed: 0.5-2 m/s

**Ambient Effects**:
- Duration: Looping
- Emission: Constant rate
- Shape: Box or Sphere
- Speed: Low (0.2-1 m/s)

### Testing Your VFX

1. Test at all 4 quality levels
2. Verify pooling works (spawn effect 100 times rapidly)
3. Check performance impact (aim for < 1ms per effect)
4. Test color overrides work correctly
5. Ensure effects scale with intensity parameter
6. Test on target platform (PC, Console, Mobile)

---

## Example Integration Code

### Example 1: Spawning a Splash

```csharp
public class FishController : MonoBehaviour
{
    private void OnFishJump()
    {
        // Publish event (VFX system handles it automatically)
        EventSystem.Publish("FishJumped", new FishJumpData(
            position: transform.position,
            fishSize: size,
            airTime: 1.5f,
            rarity: FishRarity.Rare
        ));
    }
}
```

### Example 2: Petting a Pet (THE KEY FEATURE!)

```csharp
public class PetInteraction : MonoBehaviour
{
    private void OnPlayerClickedPet()
    {
        // Increase loyalty
        loyalty += 5f;

        // Spawn petting hearts!
        EventSystem.Publish("PetPetted", new PetPettedData(
            petID: petData.petID,
            petType: petData.petType,
            petPosition: transform.position,
            currentLoyalty: loyalty
        ));

        // Play happy animation
        animator.SetTrigger("Happy");
    }
}
```

### Example 3: Custom Weather Transition

```csharp
public class WeatherController : MonoBehaviour
{
    private void TransitionToStorm()
    {
        // Publish event
        EventSystem.Publish("WeatherChanged", WeatherType.Storm);

        // VFX system will:
        // - Start heavy rain particles
        // - Activate lightning flashes
        // - Spawn wind debris
        // - Increase fog density
    }
}
```

### Example 4: Dynamic Event

```csharp
public class DynamicEventController : MonoBehaviour
{
    private void StartBloodMoon()
    {
        // Publish event
        EventSystem.Publish("DynamicEventStarted", "BloodMoon");

        // VFX system will:
        // - Tint sky red
        // - Spawn blood moon object
        // - Create red mist
        // - Adjust post-processing (red filter)
        // - Increase fog (red color)
    }
}
```

### Example 5: Manual VFX Spawning

```csharp
public class CustomEffectSpawner : MonoBehaviour
{
    private void SpawnCustomEffect()
    {
        VFXManager vfxManager = VFXManager.Instance;

        // Spawn a specific effect
        ParticleSystem ps = vfxManager.SpawnEffect("water_splash", transform.position);

        // Customize the particle system
        if (ps != null)
        {
            var main = ps.main;
            main.startColor = Color.red;
            main.startSizeMultiplier = 2f;
        }
    }
}
```

---

## Troubleshooting

### Particles Not Spawning

**Problem**: `vfxManager.SpawnEffect()` returns null.

**Solutions**:
1. Check if particle prefab is registered: `vfxManager.RegisterParticlePrefab()`
2. Verify prefab has `ParticleSystem` component
3. Check if particles are enabled: `vfxManager.AreParticlesEnabled()`
4. Check if max particle limit reached

### Low Performance

**Problem**: Frame rate drops with many particles.

**Solutions**:
1. Lower quality: `vfxManager.SetQuality(VFXQuality.Low)`
2. Enable auto-quality adjustment
3. Reduce `maxSimultaneousParticles`
4. Check for particle leaks (not returning to pool)
5. Use simpler particle systems

### Effects Not Visible

**Problem**: Particle systems spawn but aren't visible.

**Solutions**:
1. Check camera rendering layer
2. Verify particle system is active: `ps.isPlaying`
3. Check particle size (might be too small)
4. Verify rendering mode (billboard vs mesh)
5. Check shader/material assignment

### Pool Memory Leak

**Problem**: Memory usage grows over time.

**Solutions**:
1. Ensure `ParticleSystem.stopAction` is set to `Disable`
2. Verify particles are returning to pool
3. Reduce `maxPoolSize` if too high
4. Check for manual `Destroy()` calls (use pool instead)

---

## Future Improvements

### Phase 2 Enhancements

1. **VFX Graph Integration**: Upgrade to Unity VFX Graph for GPU particles
2. **Shader Variations**: Dynamic shader swapping based on quality
3. **Procedural Effects**: Runtime-generated particle systems
4. **Texture Atlasing**: Combine particle textures for efficiency
5. **LOD Animations**: Simplified animations at distance

### Advanced Features

1. **Decal System**: Blood splats, water puddles, impact marks
2. **Ribbon Trails**: Smooth trails for fishing line, spells
3. **Dynamic Lighting**: Particles emit light (bioluminescence)
4. **Sound Integration**: Particles trigger sound effects
5. **Physics Interactions**: Particles affected by wind, gravity

---

## Credits

**Implemented by**: Agent 13 - Visual Effects & Particles Specialist
**Week**: 25-26
**Phase**: 4

**Inspiration**:
- Cast n Chill (Petting Hearts Feature)
- Dredge (Horror Atmosphere)
- Zelda: Breath of the Wild (Weather System)

**Tools Used**:
- Unity Particle System
- Unity Post-Processing Stack (URP/HDRP)
- Event-Driven Architecture

---

## Quick Reference

### Common Events

```csharp
// Fishing
EventSystem.Publish("FishingCastStarted", castData);
EventSystem.Publish("FishCaught", fishData);

// Petting (THE KEY FEATURE!)
EventSystem.Publish("PetPetted", petData);

// Sanity
EventSystem.Publish("SanityChanged", sanityValue);

// Weather
EventSystem.Publish("WeatherChanged", WeatherType.Storm);

// Events
EventSystem.Publish("DynamicEventStarted", "BloodMoon");
```

### Quick Access

```csharp
VFXManager vfx = VFXManager.Instance;
vfx.GetWaterEffects().CreateSplash(pos, size, intensity);
vfx.GetFishingVFX(); // Auto-handles fishing events
vfx.GetHorrorVFX(); // Auto-handles sanity effects
vfx.GetEventVFX().StartBloodMoon();
vfx.GetCompanionVFX(); // THE PETTING HEARTS!
vfx.GetPostProcessingManager().SetVignetteIntensity(0.5f);
```

---

**End of Documentation**

For questions or support, refer to the main Bahnfish documentation or contact the development team.

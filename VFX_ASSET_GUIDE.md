# VFX Asset Sourcing Guide

**For**: Bahnfish Phase 6 - Asset Integration
**Target**: 78 particle prefabs + shaders
**Status**: Asset sourcing in progress

---

## Overview

This guide helps you source, create, or purchase the VFX assets needed for Bahnfish. The VFX system framework is complete; it just needs the actual particle prefabs and shaders.

**Total Required**:
- 78 particle prefabs across 10 categories
- 3 custom shaders (water, aurora, blood moon)
- Post-processing profile configuration

---

## Option 1: Hire a VFX Artist (Recommended)

### What to Provide Them

Give them these documents:
1. **Scripts/VFX/README.md** - Technical specifications
2. **VFX_ASSET_GUIDE.md** (this file) - Asset list
3. **GAME_DESIGN.md** - Game overview and art direction
4. **VIDEO_ANALYSIS.md** - Visual reference

### Estimated Cost
- **Full VFX package**: $1,500-$4,000 (professional)
- **Particles only**: $800-$2,000
- **Shaders only**: $500-$1,000
- **Budget option**: $500-$1,500 (semi-professional)

### Timeline
- **Full VFX**: 3-6 weeks
- **Particles only**: 2-4 weeks
- **Shaders only**: 1-2 weeks

### Where to Find VFX Artists
- **ArtStation**: Professional portfolio site
- **Fiverr**: $50-$500 per effect
- **Upwork**: $1,000-$4,000 for full game VFX
- **Reddit**: r/VFXRequests, r/gameDevClassifieds
- **Twitter**: #gamedev #vfxartist

---

## Option 2: Use Unity Asset Store (Quick Solution)

### Recommended Asset Packs

#### Particle Effect Packs

1. **"Cartoon FX Pack" series** by Jean Moreno ($50-80 each)
   - Stylized particle effects
   - Water splashes, magic effects
   - Good for: General VFX, stylized look
   - Quality: Excellent
   - Includes: 100+ prefabs per pack

2. **"Epic Toon FX"** ($45)
   - Cartoon-style VFX
   - Water, fire, magic effects
   - Good for: Stylized fishing effects
   - Quality: High
   - Includes: 300+ prefabs

3. **"Realistic Effects Pack"** series ($30-60 each)
   - Realistic particle systems
   - Water, weather, explosions
   - Good for: Realistic water effects
   - Quality: High
   - Includes: 50-100 prefabs per pack

4. **"Magic Effects Pack"** ($25-50)
   - Magical particle effects
   - Good for: Dark abilities, aberrant effects
   - Quality: Good
   - Includes: 50+ prefabs

5. **"Weather Maker"** ($60-80)
   - Complete weather system
   - Rain, snow, fog, lightning
   - Good for: All weather particles
   - Quality: Professional
   - Includes: Full weather system

#### Water Shader Packs

1. **"Stylized Water Shader"** ($30-50)
   - Cartoon/stylized water
   - Good performance
   - Customizable colors and waves
   - Good for: Stylized look

2. **"Realistic Water Shader"** ($50-80)
   - Photorealistic water
   - Reflections, refractions
   - Good for: Realistic look

3. **"Toon Water Shader"** ($20-40)
   - Cel-shaded water
   - Simple and performant
   - Good for: Stylized/cartoon aesthetic

#### Horror Effect Packs

1. **"Horror VFX Pack"** ($35-60)
   - Dark particles, mist, spirits
   - Good for: Horror atmosphere
   - Quality: High
   - Includes: 40-60 prefabs

2. **"Spooky VFX"** ($25-45)
   - Ghost effects, dark magic
   - Good for: Night hazards
   - Quality: Good
   - Includes: 30+ prefabs

**Total Asset Store Cost**: $300-$700 for all needed packs

---

## Option 3: Free Resources (Budget Option)

### Free Particle Packs

1. **Unity Particle Pack** (Unity Asset Store, Free)
   - Basic particle effects
   - Good starting point
   - Quality: Basic
   - Includes: 50+ prefabs

2. **Free Cartoon FX** (Unity Asset Store, Free)
   - Cartoon-style effects
   - Limited selection
   - Quality: Good
   - Includes: 20+ prefabs

3. **OpenGameArt.org**
   - URL: https://opengameart.org/art-search?keys=particle
   - Various particle sprites
   - License: Various Creative Commons
   - Quality: Variable

### Free Shaders

1. **Unity Standard Shaders** (Built-in)
   - Use Unity's Standard shader
   - Customize for water
   - Free, performant

2. **Shader Graph** (Unity, Free)
   - Create custom shaders visually
   - No coding required
   - Includes: Unlimited possibilities

3. **GitHub Shader Libraries**
   - Search: "Unity water shader"
   - Many open-source options
   - Quality: Variable

**Total Free Cost**: $0 (with more setup time)

---

## VFX Prefab Specifications

### Water Effects (6 prefabs)

#### 1. VFX_Water_SplashSmall
- **Use**: Small water splash (casting, small fish)
- **Particles**: 50-100
- **Lifetime**: 1-2 seconds
- **Size**: 0.5-1.5m
- **Color**: White/blue water
- **Texture**: Water droplet sprite
- **Quality Scaling**: Yes (20%-100% density)

#### 2. VFX_Water_SplashMedium
- **Use**: Medium splash (jumping fish)
- **Particles**: 100-200
- **Lifetime**: 1.5-2.5 seconds
- **Size**: 1-2m
- **Color**: White/blue water
- **Texture**: Water droplet + spray
- **Quality Scaling**: Yes

#### 3. VFX_Water_SplashLarge
- **Use**: Large splash (legendary fish, meteor impact)
- **Particles**: 200-500
- **Lifetime**: 2-3 seconds
- **Size**: 2-4m
- **Color**: White/blue water with foam
- **Texture**: Multiple droplet sprites
- **Quality Scaling**: Yes

#### 4. VFX_Water_WakeTrail
- **Use**: Boat wake trail
- **Particles**: 10-30 per second (continuous)
- **Lifetime**: 3-5 seconds
- **Size**: 1-2m
- **Color**: White foam
- **Texture**: Foam sprite
- **Quality Scaling**: Yes (disable on Low)

#### 5. VFX_Water_Ripples
- **Use**: Expanding ripples (bobber, droplets)
- **Particles**: 3-10 rings
- **Lifetime**: 2-4 seconds
- **Size**: Start 0.5m, expand to 2-3m
- **Color**: White/transparent
- **Texture**: Ripple ring sprite
- **Quality Scaling**: No (always on, low cost)

#### 6. VFX_Water_Bubbles
- **Use**: Underwater bubbles
- **Particles**: 20-50
- **Lifetime**: 1-3 seconds
- **Size**: 0.1-0.3m
- **Color**: White/transparent
- **Texture**: Bubble sprite
- **Quality Scaling**: Yes

---

### Weather Particles (7 prefabs)

#### 1. VFX_Weather_RainLight
- **Use**: Light rain
- **Particles**: 500-1000 per second
- **Lifetime**: 1-2 seconds
- **Size**: 0.05-0.1m
- **Color**: Gray/transparent
- **Texture**: Raindrop streak
- **Quality Scaling**: Yes (50% on Low)

#### 2. VFX_Weather_RainHeavy
- **Use**: Heavy rain (storms)
- **Particles**: 1500-2000 per second
- **Lifetime**: 0.8-1.5 seconds
- **Size**: 0.08-0.15m
- **Color**: Gray/white
- **Texture**: Raindrop streak
- **Quality Scaling**: Yes (30% on Low)

#### 3. VFX_Weather_Snow
- **Use**: Snowfall
- **Particles**: 300-600 per second
- **Lifetime**: 5-10 seconds
- **Size**: 0.1-0.3m
- **Color**: White
- **Texture**: Snowflake sprite
- **Quality Scaling**: Yes

#### 4. VFX_Weather_Fog
- **Use**: Volumetric fog
- **Particles**: 50-100 (large particles)
- **Lifetime**: 10-20 seconds
- **Size**: 5-15m
- **Color**: Gray/white/transparent
- **Texture**: Smoke sprite
- **Quality Scaling**: Yes (disable on Low)

#### 5. VFX_Weather_Lightning
- **Use**: Lightning bolt
- **Particles**: 1-5 (flash + bolts)
- **Lifetime**: 0.1-0.3 seconds
- **Size**: 1-3m wide, 20-50m tall
- **Color**: White/blue
- **Texture**: Lightning bolt sprite or mesh
- **Quality Scaling**: No (always on)

#### 6. VFX_Weather_WindDebris
- **Use**: Wind-blown leaves/spray
- **Particles**: 50-100
- **Lifetime**: 3-5 seconds
- **Size**: 0.2-0.5m
- **Color**: Variable (leaves, spray)
- **Texture**: Leaf/debris sprites
- **Quality Scaling**: Yes

#### 7. VFX_Weather_Storm
- **Use**: Storm ambience (combine rain + wind + fog)
- **Particles**: Multiple sub-emitters
- **Lifetime**: Continuous
- **Quality Scaling**: Yes

---

### Fishing VFX (13 prefabs)

#### 1. VFX_Fishing_CastLineArc
- **Use**: Fishing line arc during cast
- **Particles**: 20-40 (trail)
- **Lifetime**: 0.5-1 second
- **Size**: 0.1m
- **Color**: White/transparent
- **Texture**: Line trail
- **Quality Scaling**: No

#### 2. VFX_Fishing_BobberRipples
- **Use**: Periodic ripples around bobber
- **Particles**: 3-5 rings every 2 seconds
- **Lifetime**: 2 seconds
- **Size**: 0.3-1m
- **Color**: White/transparent
- **Texture**: Ripple ring
- **Quality Scaling**: No

#### 3. VFX_Fishing_TensionSparkles
- **Use**: Sparkles during tension
- **Particles**: 10-30
- **Lifetime**: 0.5-1 second
- **Size**: 0.1-0.2m
- **Color**: White/yellow
- **Texture**: Star/sparkle
- **Quality Scaling**: Yes

#### 4. VFX_Fishing_TensionWarning
- **Use**: Red particles at high tension
- **Particles**: 20-40
- **Lifetime**: 0.3-0.6 seconds
- **Size**: 0.2-0.4m
- **Color**: Red/orange
- **Texture**: Exclamation or glow
- **Quality Scaling**: No

#### 5-8. VFX_Fishing_CatchCommon/Uncommon/Rare/Legendary
- **Use**: Celebration effects for each rarity
- **Particles**: 50-200 (more for higher rarities)
- **Lifetime**: 2-4 seconds
- **Size**: 1-3m
- **Color**:
  - Common: White
  - Uncommon: Green
  - Rare: Blue
  - Legendary: Gold/rainbow
- **Texture**: Stars, sparkles, confetti
- **Quality Scaling**: Yes

#### 9. VFX_Fishing_LineBreak
- **Use**: Line snapping visual
- **Particles**: 10-20
- **Lifetime**: 0.5-1 second
- **Size**: 0.2-0.5m
- **Color**: White/gray
- **Texture**: Line fragments
- **Quality Scaling**: No

#### 10. VFX_Fishing_FishJump
- **Use**: Fish jumping splash + droplets
- **Particles**: 100-200
- **Lifetime**: 1-2 seconds
- **Size**: 1-2m
- **Color**: White/blue water
- **Texture**: Water droplets
- **Quality Scaling**: Yes

#### 11. VFX_Fishing_FishJumpRainbow
- **Use**: Rainbow effect for legendary fish jumps
- **Particles**: 50-100
- **Lifetime**: 1-2 seconds
- **Size**: 2-4m
- **Color**: Rainbow gradient
- **Texture**: Glow/mist sprite
- **Quality Scaling**: Yes

#### 12. VFX_Fishing_NetCast
- **Use**: Net expanding in water
- **Particles**: 30-50
- **Lifetime**: 1-2 seconds
- **Size**: 2-3m
- **Color**: White/blue
- **Texture**: Net pattern or splash
- **Quality Scaling**: Yes

#### 13. VFX_Fishing_HarpoonTrail
- **Use**: Harpoon projectile trail
- **Particles**: 20-30
- **Lifetime**: 0.3-0.6 seconds
- **Size**: 0.2-0.4m
- **Color**: White/gray
- **Texture**: Smoke/trail
- **Quality Scaling**: Yes

---

### Horror VFX (9 prefabs)

#### 1. VFX_Horror_SanityDistortion
- **Use**: Screen-space distortion at low sanity
- **Particles**: 10-20 (subtle)
- **Lifetime**: Continuous (fade in/out)
- **Size**: Screen-space
- **Color**: Black/purple/red
- **Texture**: Distortion noise
- **Quality Scaling**: No

#### 2. VFX_Horror_Hallucination
- **Use**: Fleeting hallucination particles
- **Particles**: 5-15
- **Lifetime**: 0.5-1.5 seconds
- **Size**: 0.5-1m
- **Color**: Black/transparent
- **Texture**: Shadow figures
- **Quality Scaling**: Yes

#### 3. VFX_Horror_FishThiefMist
- **Use**: Mist when Fish Thief appears
- **Particles**: 30-60
- **Lifetime**: 2-4 seconds
- **Size**: 2-5m
- **Color**: Black/gray/transparent
- **Texture**: Smoke/mist
- **Quality Scaling**: Yes

#### 4. VFX_Horror_FogHazard
- **Use**: Dense fog hazard
- **Particles**: 50-100
- **Lifetime**: 10-15 seconds
- **Size**: 5-10m
- **Color**: Dark gray/transparent
- **Texture**: Dense fog
- **Quality Scaling**: Yes (reduce count on Low)

#### 5. VFX_Horror_GhostShipGlow
- **Use**: Ethereal glow around Ghost Ship
- **Particles**: 20-40
- **Lifetime**: 2-5 seconds (fade in/out)
- **Size**: 3-8m
- **Color**: Green/blue/transparent
- **Texture**: Glow/mist
- **Quality Scaling**: Yes

#### 6. VFX_Horror_WhispererAura
- **Use**: Dark aura from Whisperer
- **Particles**: 10-30
- **Lifetime**: 3-6 seconds
- **Size**: 2-4m
- **Color**: Purple/black/transparent
- **Texture**: Wispy smoke
- **Quality Scaling**: Yes

#### 7. VFX_Horror_CursedFishAura
- **Use**: Dark aura around cursed fish
- **Particles**: 10-20
- **Lifetime**: Continuous
- **Size**: 0.5-1m
- **Color**: Red/purple/black
- **Texture**: Dark mist
- **Quality Scaling**: Yes

#### 8. VFX_Horror_SanityPulse
- **Use**: Pulsing effect at very low sanity
- **Particles**: 5-10 waves
- **Lifetime**: 0.5-1 second
- **Size**: Screen-space
- **Color**: Red/black
- **Texture**: Vignette pulse
- **Quality Scaling**: No

#### 9. VFX_Horror_InsanityScreenEffect
- **Use**: Full-screen corruption effect
- **Particles**: Screen-space overlay
- **Lifetime**: Continuous (intensity varies)
- **Size**: Full screen
- **Color**: Black/red/distorted colors
- **Texture**: Noise/corruption
- **Quality Scaling**: No

---

### Companion VFX (10 prefabs)

#### 1-6. VFX_Petting_Hearts_[PetType]
One for each pet (Dog, Cat, Seabird, Otter, Crab, Ghost):
- **Use**: Hearts floating up when petting
- **Particles**: 3-5 hearts
- **Lifetime**: 1-2 seconds
- **Size**: 0.3-0.6m
- **Color**: Pet-specific:
  - Dog: Red hearts
  - Cat: Pink hearts
  - Seabird: Blue hearts
  - Otter: Green hearts
  - Crab: Orange hearts
  - Ghost: Purple hearts
- **Texture**: Heart sprite
- **Quality Scaling**: No (core feature!)

#### 7. VFX_Companion_PetSparkleBurst
- **Use**: Sparkle burst during petting
- **Particles**: 20-40
- **Lifetime**: 0.5-1 second
- **Size**: 0.5-1m
- **Color**: White/yellow
- **Texture**: Sparkle/star
- **Quality Scaling**: Yes

#### 8. VFX_Companion_AbilityActivate
- **Use**: Visual effect when companion ability activates
- **Particles**: 30-60
- **Lifetime**: 1-2 seconds
- **Size**: 1-2m
- **Color**: Varies by ability
- **Texture**: Glow/magic
- **Quality Scaling**: Yes

#### 9. VFX_Companion_LoyaltyIncrease
- **Use**: Small sparkles when loyalty increases
- **Particles**: 10-20
- **Lifetime**: 0.8-1.5 seconds
- **Size**: 0.3-0.6m
- **Color**: Yellow/gold
- **Texture**: Small sparkles
- **Quality Scaling**: Yes

#### 10. VFX_Companion_WarmGlow
- **Use**: Warm glow during petting interaction
- **Particles**: 5-10
- **Lifetime**: 1-3 seconds
- **Size**: 1-2m
- **Color**: Warm orange/yellow
- **Texture**: Soft glow
- **Quality Scaling**: Yes

---

### Event VFX (7 prefabs)

#### 1. VFX_Event_BloodMoon
- **Use**: Blood Moon sky effects
- **Particles**: 50-100
- **Lifetime**: Continuous
- **Size**: Sky-scale (100+m)
- **Color**: Red/crimson
- **Texture**: Mist, glow
- **Quality Scaling**: Yes

#### 2. VFX_Event_MeteorTrail
- **Use**: Meteor falling with trail
- **Particles**: 40-80 (trail)
- **Lifetime**: 2-4 seconds
- **Size**: 1-3m wide, 10-20m long
- **Color**: Orange/yellow/white
- **Texture**: Fire/trail sprite
- **Quality Scaling**: Yes

#### 3. VFX_Event_MeteorImpact
- **Use**: Meteor hitting water
- **Particles**: 200-400
- **Lifetime**: 2-3 seconds
- **Size**: 5-10m
- **Color**: Orange/white/blue water
- **Texture**: Explosion + splash
- **Quality Scaling**: Yes

#### 4. VFX_Event_Aurora
- **Use**: Aurora Borealis waves
- **Particles**: Use shader (procedural)
- **Lifetime**: Continuous
- **Size**: Sky-scale
- **Color**: Green/blue/purple gradient
- **Texture**: Shader-based
- **Quality Scaling**: Yes (disable on Low)

#### 5. VFX_Event_FestivalFireworks
- **Use**: Fireworks explosions
- **Particles**: 100-200 per burst
- **Lifetime**: 2-4 seconds
- **Size**: 5-10m
- **Color**: Rainbow colors
- **Texture**: Star burst
- **Quality Scaling**: Yes

#### 6. VFX_Event_FestivalLanterns
- **Use**: Floating lanterns
- **Particles**: 10-30
- **Lifetime**: 10-20 seconds
- **Size**: 0.5-1m
- **Color**: Warm orange/yellow
- **Texture**: Paper lantern sprite
- **Quality Scaling**: Yes

#### 7. VFX_Event_FestivalConfetti
- **Use**: Confetti celebration
- **Particles**: 100-200
- **Lifetime**: 3-5 seconds
- **Size**: 0.1-0.3m
- **Color**: Rainbow colors
- **Texture**: Confetti pieces
- **Quality Scaling**: Yes

---

### Fish AI Visuals (6 prefabs)

#### 1. VFX_Fish_Trail
- **Use**: Trail behind swimming fish
- **Particles**: 10-20
- **Lifetime**: 0.5-1 second
- **Size**: 0.2-0.5m
- **Color**: Transparent/water color
- **Texture**: Wake trail
- **Quality Scaling**: Yes

#### 2. VFX_Fish_SchoolShimmer
- **Use**: Shimmer effect for schooling fish
- **Particles**: 20-40
- **Lifetime**: 0.3-0.8 seconds
- **Size**: 0.1-0.3m
- **Color**: Silver/white
- **Texture**: Sparkle
- **Quality Scaling**: Yes

#### 3. VFX_Fish_AberrantAura
- **Use**: Aberrant fish glow
- **Particles**: 15-30
- **Lifetime**: Continuous
- **Size**: 0.5-1m
- **Color**: Purple/green mutant glow
- **Texture**: Glow/mist
- **Quality Scaling**: Yes

#### 4. VFX_Fish_LegendaryAura
- **Use**: Legendary fish aura
- **Particles**: 20-40
- **Lifetime**: Continuous
- **Size**: 1-2m
- **Color**: Gold/rainbow
- **Texture**: Sparkles/glow
- **Quality Scaling**: Yes

#### 5. VFX_Fish_BiolumGlow
- **Use**: Bioluminescent fish glow
- **Particles**: 10-20
- **Lifetime**: Continuous (pulse)
- **Size**: 0.3-0.8m
- **Color**: Blue/green
- **Texture**: Glow
- **Quality Scaling**: Yes

#### 6. VFX_Fish_Bubbles
- **Use**: Bubbles from fish
- **Particles**: 5-10
- **Lifetime**: 1-2 seconds
- **Size**: 0.05-0.15m
- **Color**: Transparent/white
- **Texture**: Bubble
- **Quality Scaling**: Yes

---

### Inventory VFX (6 prefabs)

#### 1. VFX_Inventory_ItemPickup
- **Use**: Item collected sparkle
- **Particles**: 20-40
- **Lifetime**: 0.8-1.5 seconds
- **Size**: 0.5-1m
- **Color**: White/yellow
- **Texture**: Sparkle
- **Quality Scaling**: No

#### 2. VFX_Inventory_DragGlow
- **Use**: Glow around dragged item
- **Particles**: 10-20
- **Lifetime**: Continuous while dragging
- **Size**: Item size + 0.2m
- **Color**: White/blue
- **Texture**: Glow
- **Quality Scaling**: No

#### 3. VFX_Inventory_ValidPlacement
- **Use**: Green glow for valid placement
- **Particles**: 5-10
- **Lifetime**: Continuous
- **Size**: Grid cell size
- **Color**: Green/transparent
- **Texture**: Grid highlight
- **Quality Scaling**: No

#### 4. VFX_Inventory_InvalidPlacement
- **Use**: Red glow for invalid placement
- **Particles**: 5-10
- **Lifetime**: Continuous
- **Size**: Grid cell size
- **Color**: Red/transparent
- **Texture**: Grid highlight
- **Quality Scaling**: No

#### 5. VFX_Inventory_SellFlash
- **Use**: Flash when selling item
- **Particles**: 15-30
- **Lifetime**: 0.5-1 second
- **Size**: 0.5-1m
- **Color**: Gold/yellow
- **Texture**: Coin/sparkle
- **Quality Scaling**: No

#### 6. VFX_Inventory_CraftSuccess
- **Use**: Crafting success burst
- **Particles**: 30-60
- **Lifetime**: 1-2 seconds
- **Size**: 1-2m
- **Color**: White/gold
- **Texture**: Sparkle/star
- **Quality Scaling**: Yes

---

### UI Particle Effects (9 prefabs)

#### 1. VFX_UI_Achievement
- **Use**: Achievement unlock fanfare
- **Particles**: 50-100
- **Lifetime**: 2-3 seconds
- **Size**: 2-4m (UI space)
- **Color**: Gold/rainbow
- **Texture**: Stars/confetti
- **Quality Scaling**: No

#### 2. VFX_UI_Notification
- **Use**: Notification popup sparkle
- **Particles**: 10-20
- **Lifetime**: 0.5-1 second
- **Size**: 1-2m (UI space)
- **Color**: White/blue
- **Texture**: Sparkle
- **Quality Scaling**: No

#### 3. VFX_UI_LevelUp
- **Use**: Level up burst
- **Particles**: 60-100
- **Lifetime**: 2-3 seconds
- **Size**: 2-3m (UI space)
- **Color**: Yellow/gold
- **Texture**: Stars/burst
- **Quality Scaling**: No

#### 4. VFX_UI_MoneyGain
- **Use**: Money gain coins
- **Particles**: 10-20
- **Lifetime**: 1-2 seconds
- **Size**: 0.5-1m
- **Color**: Gold
- **Texture**: Coin sprite
- **Quality Scaling**: No

#### 5. VFX_UI_ButtonHover
- **Use**: Button hover glow
- **Particles**: 5-10
- **Lifetime**: Continuous
- **Size**: Button size
- **Color**: White/blue
- **Texture**: Glow
- **Quality Scaling**: No

#### 6. VFX_UI_QuestComplete
- **Use**: Quest completion effect
- **Particles**: 40-80
- **Lifetime**: 2-3 seconds
- **Size**: 2-3m
- **Color**: Green/white
- **Texture**: Check marks/sparkles
- **Quality Scaling**: No

#### 7. VFX_UI_ErrorShake
- **Use**: Error indicator shake
- **Particles**: Screen shake + red flash
- **Lifetime**: 0.3-0.5 seconds
- **Size**: Full screen
- **Color**: Red
- **Texture**: Flash
- **Quality Scaling**: No

#### 8. VFX_UI_ScreenFlash
- **Use**: Screen flash (events, damage)
- **Particles**: Full screen overlay
- **Lifetime**: 0.1-0.3 seconds
- **Size**: Full screen
- **Color**: White/red/blue
- **Texture**: Full-screen quad
- **Quality Scaling**: No

#### 9. VFX_UI_DamagePulse
- **Use**: Damage vignette pulse
- **Particles**: Screen-space
- **Lifetime**: 0.5-1 second
- **Size**: Full screen
- **Color**: Red vignette
- **Texture**: Radial gradient
- **Quality Scaling**: No

---

## Custom Shader Specifications

### 1. Water Surface Shader

**Features Needed**:
- Animated waves (vertex displacement)
- Foam at edges (shore, boat)
- Reflections (skybox, sun)
- Transparency with depth fade
- Normal mapping for waves
- Color tinting (day/night)
- Caustics (underwater light patterns)

**Technical Requirements**:
- Shader Graph or HLSL
- Works with URP/Built-in pipeline
- Performance: < 5ms per frame
- Quality levels: Low (no reflections), High (all features)

**Reference Shaders**:
- Unity "Stylized Water Shader"
- "Toon Water Shader" asset store
- Built-in Water shader (modify)

### 2. Aurora Shader (Procedural)

**Features Needed**:
- Procedural wave patterns
- Color gradient (green/blue/purple)
- Animated movement
- Soft edges (fade out)
- Emissive glow

**Technical Requirements**:
- Shader Graph or HLSL
- Skybox shader or quad mesh
- Noise-based animation
- Performance: < 2ms per frame
- Can be disabled on Low quality

**Reference**:
- Northern lights shaders
- Noise-based sky effects

### 3. Blood Moon Overlay Shader

**Features Needed**:
- Red color tint overlay
- Gradient from center (moon)
- Pulsing intensity
- Blend with existing scene
- Vignette effect

**Technical Requirements**:
- Post-processing shader or screen-space
- Additive/multiply blending
- Simple and performant
- Performance: < 1ms per frame

**Reference**:
- Color grading shaders
- Vignette post-effects

---

## Particle Creation Tips

### General Guidelines

1. **Keep It Simple**: Start with basic shapes, add detail later
2. **Use Textures**: 64×64 to 256×256 for most particles
3. **Alpha Channels**: Use alpha for transparency/fade
4. **Color Over Lifetime**: Fade in/out for smooth appearance
5. **Size Over Lifetime**: Start small, grow, then shrink
6. **Rotation**: Add random rotation for variety
7. **Velocity**: Use curves for realistic motion
8. **Collision**: Enable for ground/water interactions
9. **Pooling**: All VFX use pooling (already implemented)
10. **Testing**: Test at Low quality (20% density)

### Performance Optimization

- **Max Particles**: 10,000 cap (system-wide)
- **Distance Culling**: 100m
- **Quality LOD**: Scale density (20%-100%)
- **Texture Atlasing**: Combine textures
- **GPU Particles**: Use when possible
- **Disable on Low**: Heavy effects like fog, aurora

---

## Unity Particle System Settings

### Example: Water Splash

```
Particle System:
- Duration: 1.5s
- Looping: No
- Start Lifetime: 1-2s
- Start Speed: 3-6 m/s
- Start Size: 0.5-1.5m
- Start Rotation: 0-360 degrees
- Start Color: White
- Gravity Modifier: 1.0
- Max Particles: 100

Emission:
- Rate over Time: 0
- Bursts: 1 burst of 50-100 particles at start

Shape:
- Shape: Sphere
- Radius: 0.5m
- Random Direction

Color over Lifetime:
- Start: Alpha 1.0
- End: Alpha 0.0

Size over Lifetime:
- Curve: Start 0.5, peak 1.0 at 30%, fade to 0.0

Texture Sheet Animation:
- Mode: Single Row
- Frame over Time: Linear

Renderer:
- Render Mode: Billboard
- Material: Particle/Alpha Blended
```

---

## Asset Delivery Format

### Folder Structure
```
Bahnfish_VFX/
├── Prefabs/
│   ├── Water/
│   │   ├── VFX_Water_SplashSmall.prefab
│   │   ├── VFX_Water_SplashMedium.prefab
│   │   └── ...
│   ├── Weather/
│   │   └── ...
│   ├── Fishing/
│   │   └── ...
│   ├── Horror/
│   │   └── ...
│   └── ...
├── Textures/
│   ├── water_droplet.png
│   ├── foam_sprite.png
│   ├── heart_sprite.png
│   └── ...
├── Materials/
│   ├── M_ParticleWater.mat
│   ├── M_ParticleFire.mat
│   └── ...
├── Shaders/
│   ├── SH_WaterSurface.shader
│   ├── SH_Aurora.shadergraph
│   └── SH_BloodMoon.shader
└── Documentation/
    └── VFX_List.xlsx
```

### Documentation
- **VFX List**: Spreadsheet with all prefabs, descriptions, specs
- **Shader Guide**: How to use and customize shaders
- **Quality Settings**: Recommended settings per quality level
- **Source Files**: Original project files (if applicable)

---

## Testing VFX Assets

### Checklist
- [ ] All 78 prefabs spawn correctly
- [ ] Particles respect quality settings (LOD)
- [ ] No z-fighting or sorting issues
- [ ] Colors match game aesthetic
- [ ] Performance: Max 10,000 particles
- [ ] Distance culling works (100m)
- [ ] Pooling works (no instantiate lag)
- [ ] Water shader animates smoothly
- [ ] Aurora shader can be disabled on Low
- [ ] Blood Moon overlay doesn't obscure gameplay

---

## Quick Start: Placeholder VFX

If you need to test the system before final VFX:

1. **Use Unity's built-in particles** (Edit → Effects → Particle System)
2. **Use basic textures** (white circles, simple sprites)
3. **Focus on functionality**, not visual quality
4. **Test pooling and LOD systems**

This allows you to:
- Test VFX system functionality
- Verify particle pooling works
- Test quality LOD scaling
- Verify spawn triggers
- Complete testing while final VFX are being created

---

## Budget Breakdown

### Professional VFX Artist ($2,500)
- Particles: $1,500 (78 prefabs)
- Shaders: $800 (3 custom shaders)
- Revisions: $200

### Asset Store Packs ($500)
- Particle packs: $300
- Shader packs: $150
- Weather system: $50

### Free + DIY ($0)
- Unity free particles
- Shader Graph (create custom)
- Free texture resources

---

## Timeline

### With VFX Artist
- **Week 1-2**: Core particles (water, weather, fishing)
- **Week 3-4**: Horror and event particles
- **Week 5**: Shaders
- **Week 6**: Polish and optimization

### With Asset Stores
- **Day 1**: Purchase and download
- **Day 2-3**: Import and organize
- **Day 4-5**: Configure and customize
- **Day 6-7**: Test and optimize

### With DIY Approach
- **Week 1**: Learn Unity Particle System
- **Week 2**: Create basic particles
- **Week 3**: Create advanced particles
- **Week 4**: Create shaders (Shader Graph)
- **Week 5**: Polish and optimize

---

**Next Step**: Choose an option and start creating/sourcing VFX assets. The VFX system is ready and waiting for prefabs!

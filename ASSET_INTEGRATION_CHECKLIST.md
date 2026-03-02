# Asset Integration Checklist

**Phase**: 6 - Testing & Launch
**Purpose**: Guide for integrating audio and VFX assets into Bahnfish

---

## Overview

Bahnfish has complete audio and VFX frameworks ready for asset integration. This document provides step-by-step instructions for sound designers and VFX artists to integrate their work.

---

## AUDIO ASSET INTEGRATION

### Prerequisites
- Unity 2022 LTS or later
- Bahnfish project opened
- Audio assets prepared (see specifications below)

### Audio Asset Specifications

#### Music Tracks (8+ tracks needed)
- **Format**: OGG or MP3 (OGG preferred for looping)
- **Sample Rate**: 44.1kHz
- **Bit Depth**: 16-bit
- **Length**: 2-5 minutes per track
- **Looping**: Seamless loops (no gaps)
- **Layers**: 2-4 layers per track (base + conditional layers)
- **File Naming**: `music_[trackname]_[layer].ogg`
  - Example: `music_night_base.ogg`, `music_night_horror.ogg`

**Required Tracks**:
1. **Menu** - Atmospheric title theme
2. **Day** - Calm, peaceful fishing music
3. **Dusk** - Melancholic transition music
4. **Night** - Tense, ominous music with horror undertones
5. **Dawn** - Hopeful, new beginning music
6. **Fishing** - Active, rhythmic music for catching fish
7. **Shop** - Upbeat, social music for docks/vendors
8. **Boss/Chase** - Intense, action music for horror sequences

**Optional Tracks**:
9. **Blood Moon** - Choral, intense event music
10. **Festival** - Celebratory, joyful music

#### Sound Effects (100+ sounds needed)

**Format**: WAV (uncompressed for quality)
**Sample Rate**: 44.1kHz or 48kHz
**Bit Depth**: 16-bit
**Length**: 0.1-5 seconds

**Required SFX by Category**:

**Fishing Sounds** (25 sounds):
- `sfx_cast_whoosh.wav` - Casting rod whoosh
- `sfx_cast_splash.wav` - Bobber hitting water
- `sfx_reel_slow.wav` - Slow reeling sound (looping)
- `sfx_reel_fast.wav` - Fast reeling sound (looping)
- `sfx_line_tension_01.wav` to `03.wav` - Line creaking (3 variations)
- `sfx_line_snap.wav` - Line breaking
- `sfx_fish_jump_01.wav` to `03.wav` - Fish splashing (3 variations)
- `sfx_fish_thrash.wav` - Fish fighting on line
- `sfx_catch_success.wav` - Success jingle
- `sfx_catch_fail.wav` - Disappointment sound
- `sfx_bite_alert.wav` - Fish bite notification
- (10 more fishing-related sounds)

**Boat Sounds** (15 sounds):
- `sfx_engine_start.wav`
- `sfx_engine_idle.wav` (looping)
- `sfx_engine_accel.wav`
- `sfx_engine_stop.wav`
- `sfx_water_splash_bow.wav`
- `sfx_water_splash_side.wav`
- `sfx_wood_creak_01.wav` to `03.wav`
- `sfx_anchor_drop.wav`
- `sfx_anchor_raise.wav`
- (6 more boat sounds)

**Horror Sounds** (25 sounds):
- `sfx_whisper_01.wav` to `05.wav` - Eerie whispers (5 variations)
- `sfx_scream_distant.wav`
- `sfx_crow_caw_01.wav` to `03.wav`
- `sfx_crow_wings.wav`
- `sfx_ghost_ship_horn.wav`
- `sfx_ghost_ship_creak.wav`
- `sfx_chains_rattle.wav`
- `sfx_sanity_drain.wav` - Tinnitus/ringing
- `sfx_distortion.wav` - Reality distortion sound
- (16 more horror sounds)

**Companion Sounds** (15 sounds):
**Dog**:
- `sfx_dog_bark_happy.wav`
- `sfx_dog_bark_alert.wav`
- `sfx_dog_whine.wav`
- `sfx_dog_pant.wav`
**Cat**:
- `sfx_cat_meow.wav`
- `sfx_cat_purr.wav`
- `sfx_cat_hiss.wav`
**Seabird**:
- `sfx_seabird_caw.wav`
- `sfx_seabird_chirp.wav`
**Otter**:
- `sfx_otter_squeak.wav`
- `sfx_otter_splash.wav`
**Hermit Crab**:
- `sfx_crab_scuttle.wav`
**Ghost**:
- `sfx_ghost_whisper.wav`
- `sfx_ghost_phase.wav`

**UI Sounds** (20 sounds):
- `sfx_ui_button_hover.wav`
- `sfx_ui_button_click.wav`
- `sfx_ui_button_disabled.wav`
- `sfx_ui_menu_open.wav`
- `sfx_ui_menu_close.wav`
- `sfx_ui_notification_info.wav`
- `sfx_ui_notification_warning.wav`
- `sfx_ui_notification_error.wav`
- `sfx_ui_achievement.wav` - Fanfare
- `sfx_ui_quest_complete.wav`
- (10 more UI sounds)

**Environment Sounds** (10 sounds):
- `sfx_thunder_distant.wav`
- `sfx_thunder_close.wav`
- `sfx_wind_light.wav` (looping)
- `sfx_wind_strong.wav` (looping)
- `sfx_whale_song.wav`
- `sfx_dolphin_clicks.wav`
- `sfx_seagull_01.wav` to `03.wav`
- (1 more)

**Item Sounds** (10 sounds):
- `sfx_item_pickup.wav`
- `sfx_item_drop.wav`
- `sfx_item_rotate.wav`
- `sfx_money_coin.wav`
- `sfx_sell_success.wav`
- `sfx_craft_success.wav`
- `sfx_cook_sizzle.wav` (looping)
- `sfx_cook_complete.wav`
- `sfx_camera_shutter.wav`
- `sfx_buff_apply.wav`

#### Ambient Layers (50+ layers needed)

**Format**: OGG (streaming)
**Sample Rate**: 44.1kHz
**Bit Depth**: 16-bit
**Length**: 30 seconds - 2 minutes (seamless loop)
**File Naming**: `ambient_[location]_[layer].ogg`

**Required Ambient Layers** (13 locations × 3-4 layers each):

**Calm Lake**:
- `ambient_calm_lake_base.ogg` - Gentle waves
- `ambient_calm_lake_birds.ogg` - Duck quacks, songbirds
- `ambient_calm_lake_wind.ogg` - Light breeze
- `ambient_calm_lake_splash.ogg` - Distant fish jumping

**Rocky Coastline**:
- `ambient_rocky_coast_waves.ogg` - Crashing waves
- `ambient_rocky_coast_seagulls.ogg` - Seagull cries
- `ambient_rocky_coast_wind.ogg` - Strong coastal wind
- `ambient_rocky_coast_foghorn.ogg` - Distant foghorn

... *(repeat for all 13 locations)*

**Weather Layers**:
- `ambient_rain_light.ogg`
- `ambient_rain_heavy.ogg`
- `ambient_storm_rain.ogg`
- `ambient_storm_thunder.ogg` (thunder cracks, not looping)
- `ambient_fog_mist.ogg`
- `ambient_snow_wind.ogg`

**Time Layers**:
- `ambient_night_crickets.ogg`
- `ambient_night_owls.ogg`
- `ambient_dawn_birds.ogg`

### Audio Integration Steps

#### Step 1: Import Audio Files
1. Open Unity project
2. Create folder structure:
   ```
   Assets/Audio/
   ├── Music/
   │   ├── Day/
   │   ├── Night/
   │   ├── etc.
   ├── SFX/
   │   ├── Fishing/
   │   ├── Boat/
   │   ├── Horror/
   │   ├── Companion/
   │   ├── UI/
   │   ├── Environment/
   │   ├── Items/
   └── Ambient/
       ├── Locations/
       ├── Weather/
       └── Time/
   ```
3. Drag audio files into appropriate folders
4. Wait for Unity to import (may take several minutes for 800MB)

#### Step 2: Configure Music Tracks
1. Locate `Scripts/Audio/MusicSystem.cs` in hierarchy
2. Select it in Inspector
3. Find "Music Tracks" array
4. Expand each track:
   - **Day Track**:
     - Track Type: Day
     - Base Clip: Drag `music_day_base.ogg`
     - Conditional Layers: (Add layers if multi-layer)
   - Repeat for all 8+ tracks
5. Configure:
   - Crossfade Duration: 3.0 (seconds)
   - Beat Match: true/false (if tracks have BPM)

#### Step 3: Configure Sound Effects
1. Locate `Scripts/Audio/SoundEffectManager.cs`
2. Select in Inspector
3. Find "Sound Effects Library" array
4. For each SFX category, add entries:
   - **ID**: `fishing_cast` (unique identifier)
   - **Display Name**: "Fishing Cast"
   - **Clip**: Drag `sfx_cast_whoosh.wav`
   - **Category**: SFX_Fishing
   - **Base Volume**: 1.0
   - **Pitch Variation**: 0.1 (±10% pitch)
   - **Loop**: false
5. Repeat for all 100+ sounds

**Tip**: Create a spreadsheet to track all sound IDs and files.

#### Step 4: Configure Ambient Soundscapes
1. Locate `Scripts/Audio/AmbientSoundscape.cs`
2. Select in Inspector
3. Find "Location Soundscapes" array (13 entries)
4. For each location:
   - **Location ID**: `calm_lake`
   - **Layers**: (Add 3-4 layers)
     - Layer 0 (Base):
       - Clip: `ambient_calm_lake_base.ogg`
       - Volume: 1.0
       - Loop: true
     - Layer 1 (Birds):
       - Clip: `ambient_calm_lake_birds.ogg`
       - Volume: 0.8
       - Loop: true
       - Condition: TimeOfDay.Day
   - Repeat for all layers

#### Step 5: Configure AudioMixer
1. Open `Assets/Audio/MainAudioMixer.mixer` (create if doesn't exist)
2. Create 5 groups:
   - Master (parent)
     - Music
     - SFX
     - Ambient
     - UI
3. For each group, expose volume parameter:
   - Right-click Volume → "Expose to script"
   - Name: "MusicVolume", "SFXVolume", etc.
4. Assign AudioMixer to AudioManager:
   - Select AudioManager in hierarchy
   - Drag MainAudioMixer to "Audio Mixer" field

#### Step 6: Place AudioZones
1. For each of 13 locations, create AudioZone:
   - Create empty GameObject: "AudioZone_CalmLake"
   - Add `AudioZone.cs` component
   - Configure:
     - Location ID: `calm_lake`
     - Reverb Preset: OpenSpace
     - Override Soundscape: true
     - Soundscape Override: Calm Lake soundscape
2. Position trigger collider to cover location area

#### Step 7: Test Audio Integration
- [ ] Enter Play mode
- [ ] Verify music starts (Menu or Day track)
- [ ] Cast fishing rod, verify cast sound plays
- [ ] Change time to Night, verify music transitions
- [ ] Move to different location, verify soundscape changes
- [ ] Adjust volume sliders, verify all channels work
- [ ] Save and load game, verify volumes persist

---

## VFX ASSET INTEGRATION

### Prerequisites
- Unity 2022 LTS or later
- Universal Render Pipeline (URP) or HDRP
- Particle systems knowledge

### VFX Asset Specifications

#### Particle Prefabs (78 needed)

**General Specs**:
- Use Unity Particle System (not VFX Graph, for compatibility)
- Emission: Bursts or continuous as appropriate
- Max Particles: 100-1000 depending on effect
- Simulation Space: World (for most), Local (for attached effects)
- Start Lifetime: 0.5-5 seconds
- Start Speed: Varies by effect
- Collision: Enable for water splashes
- Poolable: Yes (mark as static when possible)

**Naming Convention**: `VFX_[category]_[effect].prefab`
- Example: `VFX_Water_SplashSmall.prefab`

#### Required VFX by Category

**Water Effects** (6 prefabs):
1. **VFX_Water_SplashSmall** - Small water splash (casting, small fish)
   - Particles: 50-100
   - Lifetime: 1-2s
   - Sprite: Water droplet texture
2. **VFX_Water_SplashMedium** - Medium splash (jumping fish)
   - Particles: 100-200
   - Lifetime: 1.5-2.5s
3. **VFX_Water_SplashLarge** - Large splash (legendary fish, meteor impact)
   - Particles: 200-500
   - Lifetime: 2-3s
4. **VFX_Water_WakeTrail** - Boat wake trail
   - Particles: Continuous, 50/sec
   - Lifetime: 3-5s
   - Trail renderer or particle trail
5. **VFX_Water_Ripple** - Expanding ripple rings
   - Particles: 1-3 rings
   - Lifetime: 2-4s
   - Scale over lifetime (expand)
6. **VFX_Water_Foam** - Foam particles around rocks
   - Particles: Continuous, 20/sec
   - Lifetime: 2-3s

**Weather Particles** (7 prefabs):
1. **VFX_Weather_RainLight** - Light rain
   - Particles: 500/sec
   - Lifetime: 2s
   - Direction: Downward, slight angle
2. **VFX_Weather_RainHeavy** - Heavy rain
   - Particles: 2000/sec
   - Lifetime: 1.5s
3. **VFX_Weather_Snow** - Snowflakes
   - Particles: 200/sec
   - Lifetime: 10s
   - Slow fall, swaying motion
4. **VFX_Weather_Fog** - Fog mist (volumetric or particle)
   - Particles: 50/sec
   - Lifetime: 20s
   - Large, slow-moving
5. **VFX_Weather_Lightning** - Lightning flash (screen overlay)
   - Duration: 0.1s flash
   - White screen overlay
6. **VFX_Weather_WindDebris** - Wind-blown leaves/spray
   - Particles: 100/sec
   - Random directions
7. **VFX_Weather_Mist** - Ground mist
   - Particles: 30/sec
   - Lifetime: 15s
   - Stays low to ground

**Fishing VFX** (13 prefabs):
1. **VFX_Fishing_CastTrail** - Line trail in air
   - Line renderer or trail
   - Duration: 0.5s
2. **VFX_Fishing_BobberSplash** - Bobber impact
   - Reuse Water_SplashSmall
3. **VFX_Fishing_BobberRipple** - Periodic ripples around bobber
   - Ripple effect, every 2s
4. **VFX_Fishing_LineTension** - Sparkles along line
   - Particles: 20-50
   - Lifetime: 0.5s
   - Color: White→Red (tension increases)
5. **VFX_Fishing_TensionWarning** - Red sparks at high tension
   - Particles: 50
   - Color: Red
   - Urgent flashing
6. **VFX_Fishing_FishJump** - Fish jump splash
   - Reuse Water_SplashMedium
   - Add rainbow particles for legendary
7. **VFX_Fishing_CatchSuccess** - Success celebration
   - Burst: 50-100 particles
   - Colors: Gold, white sparkles
   - Radial explosion
8. **VFX_Fishing_CatchRainbow** - Rainbow for legendary
   - Arc particles, rainbow gradient
9. **VFX_Fishing_LineBreak** - Snap burst
   - Burst: 30 particles
   - Direction: Whip back toward player
10. **VFX_Fishing_FailPuff** - Disappointment puff
    - Burst: 20 particles
    - Color: Grey, smoke-like
11-13. (3 more fishing-related effects as needed)

**Horror VFX** (9 prefabs):
1. **VFX_Horror_DarkMist** - Fish thief mist trail
   - Particles: 50/sec
   - Color: Dark purple/black
   - Lifetime: 3s
2. **VFX_Horror_Feathers** - Crow feathers falling
   - Particles: 20
   - Lifetime: 2s
   - Falling motion
3. **VFX_Horror_GhostGlow** - Ghost ship spectral glow
   - Particles: 100/sec
   - Color: Green/blue ethereal
   - Lifetime: 5s
4. **VFX_Horror_Whisper** - Sound wave particles
   - Particles: Rings expanding
   - Translucent, distortion effect
5. **VFX_Horror_ShadowTendrils** - Screen edge shadows
   - Particles: Slow-moving at edges
   - Color: Black
6. **VFX_Horror_EyesGlow** - Glowing eyes in darkness
   - Particles: 2 (pair of eyes)
   - Color: Red
   - Blinking
7. **VFX_Horror_Hallucination** - Visual glitches
   - Particles: Random screen artifacts
8. **VFX_Horror_CurseAura** - Dark aura around cursed fish
   - Particles: 30/sec
   - Color: Purple/black
   - Orbital motion
9. **VFX_Horror_PortalSpawn** - Hazard spawn portal
   - Particles: Swirling
   - Color: Dark energy
   - Duration: 2s

**Companion VFX** (10 prefabs):
1. **VFX_Pet_HeartsRed** - Red hearts (Dog)
   - Particles: 3-5 hearts
   - Lifetime: 2s
   - Float upward, fade out
   - Shape: Heart sprite
2. **VFX_Pet_HeartsPink** - Pink hearts (Cat)
   - Similar to Red but pink
3. **VFX_Pet_HeartsBlue** - Blue hearts (Seabird)
4. **VFX_Pet_HeartsGreen** - Green hearts (Otter)
5. **VFX_Pet_HeartsOrange** - Orange hearts (Hermit Crab)
6. **VFX_Pet_HeartsGhost** - Ethereal hearts (Ghost)
7. **VFX_Pet_SparkleB urst** - Sparkle explosion when petting
   - Burst: 30 particles
   - Color: White/gold
8. **VFX_Pet_LoyaltyGlow** - Warm glow around pet
   - Particles: 50
   - Color: Warm yellow
   - Radial
9. **VFX_Pet_AbilityVisionCone** - Seabird scout vision cone
   - Cone-shaped particle emission
   - Color: Yellow/white
10. **VFX_Pet_PhaseFade** - Ghost phase effect
    - Particles: Ethereal trail
    - Color: Blue

**Event VFX** (7 prefabs):
1. **VFX_Event_MeteorTrail** - Meteor falling with trail
   - Trail renderer
   - Color: Orange→Red gradient
   - Glow effect
2. **VFX_Event_MeteorImpact** - Meteor water impact
   - Reuse Water_SplashLarge
   - Add shockwave ring
3. **VFX_Event_FireworksLaunch** - Firework launch trail
   - Trail upward
   - Color: Varies
4. **VFX_Event_FireworksBurst** - Firework explosion
   - Burst: 100-200 particles
   - Radial explosion
   - Sparkle trails
5. **VFX_Event_Confetti** - Celebration confetti
   - Particles: 100
   - Colorful paper pieces
   - Falling with tumbling
6. **VFX_Event_Lantern** - Floating lantern with glow
   - 1 particle (lantern)
   - Glow light
   - Slow upward drift
7. **VFX_Event_RedMist** - Blood Moon red mist
   - Particles: 50/sec
   - Color: Red
   - Ground fog

**Fish AI Visuals** (6 prefabs):
1. **VFX_Fish_BioTrail** - Bioluminescent fish trail
   - Trail renderer
   - Color: Cyan/green glow
2. **VFX_Fish_SchoolShimmer** - School shimmer effect
   - Particles: 20/sec
   - Color: Silver
3. **VFX_Fish_RareAura** - Rare fish glow
   - Particles: 30/sec
   - Color: Blue
4. **VFX_Fish_LegendaryAura** - Legendary fish glow
   - Particles: 50/sec
   - Color: Gold
5. **VFX_Fish_AberrantDistortion** - Aberrant reality warp
   - Particles: Distortion waves
   - Color: Purple
6. **VFX_Fish_FeedingBits** - Small food particles
   - Particles: 10
   - Lifetime: 1s

**Inventory VFX** (6 prefabs):
1. **VFX_Inventory_PickupGlow** - Item pickup glow
   - Particles: 20
   - Color: White/gold
2. **VFX_Inventory_DragTrail** - Drag trail
   - Trail renderer
   - Translucent
3. **VFX_Inventory_PlaceValid** - Green placement feedback
   - Glow outline
   - Color: Green
4. **VFX_Inventory_PlaceInvalid** - Red placement feedback
   - Glow outline
   - Color: Red
5. **VFX_Inventory_SellCoins** - Coin particles when selling
   - Particles: 10-20 coins
   - Lifetime: 1s
6. **VFX_Inventory_CraftSparkles** - Crafting sparkles
   - Particles: 30
   - Color: White/gold

**UI Particles** (9 prefabs):
1. **VFX_UI_AchievementBurst** - Achievement unlock
   - Burst: 50 particles
   - Color: Gold
   - Radial explosion
2. **VFX_UI_NotificationGlow** - Notification glow
   - Particles: 20
   - Color: Blue (info), Yellow (warning), Red (error)
3. **VFX_UI_QuestComplete** - Quest complete confetti
   - Particles: 30
   - Colorful
4. **VFX_UI_LevelUp** - Level up burst
   - Burst: 100 particles
   - Radial
5. **VFX_UI_ButtonHover** - Button hover glow (subtle)
   - Particles: 5
   - Color: White
6. **VFX_UI_Transition** - Screen transition particles
   - Particles: 50
   - Fade wipe
7-9. (3 more UI effects as needed)

#### Shaders

**Water Surface Shader**:
- Reflections (skybox, objects)
- Refraction (underwater distortion)
- Caustics (light patterns on seafloor)
- Foam (around rocks, boat)
- Time-of-day color (blue day, orange dusk, dark night)
- Normal maps for wave details

**Aurora Shader**:
- Procedural aurora waves
- Color gradient (green, blue, purple)
- Slow undulation animation
- Additive blending for glow
- Skybox integration

**Blood Moon Overlay Shader**:
- Red screen tint (10% opacity)
- Post-processing effect
- Adjustable intensity

**Distortion Shader**:
- Screen space distortion
- Used for low sanity effects
- Chromatic aberration

### VFX Integration Steps

#### Step 1: Import VFX Prefabs
1. Create folder structure:
   ```
   Assets/VFX/
   ├── Water/
   ├── Weather/
   ├── Fishing/
   ├── Horror/
   ├── Companion/
   ├── Events/
   ├── FishAI/
   ├── Inventory/
   ├── UI/
   └── Shaders/
   ```
2. Import all 78 particle prefabs into appropriate folders
3. Import all shaders into Shaders/ folder

#### Step 2: Configure VFXManager
1. Locate `Scripts/VFX/VFXManager.cs` in hierarchy
2. Select in Inspector
3. Assign components:
   - Water Effects: Drag `VFXManager/WaterEffects` component
   - Weather Particles: Drag `VFXManager/WeatherParticles` component
   - (Repeat for all 10 VFX subsystems)
4. For each subsystem, assign prefabs:
   - Example: WaterEffects
     - Splash Small: Drag `VFX_Water_SplashSmall.prefab`
     - Splash Medium: Drag `VFX_Water_SplashMedium.prefab`
     - (Assign all water prefabs)

#### Step 3: Configure Particle Pooling
For each prefab:
1. Open prefab
2. Set pool size:
   - Common effects (splashes, sparkles): 20
   - Rare effects (events, achievements): 5
3. Mark as static (if doesn't move)

#### Step 4: Configure Post-Processing
1. Create Post-Processing Volume:
   - GameObject → Volume → Global Volume
   - Name: "MainPostProcessing"
2. Add profile:
   - Create new Post-Processing Profile
   - Add effects:
     - Bloom
     - Vignette
     - Chromatic Aberration (disable by default)
     - Color Grading
     - Depth of Field (disable by default)
     - Motion Blur (disable by default)
3. Assign to PostProcessingManager:
   - Select `Scripts/VFX/PostProcessingManager.cs`
   - Drag Volume to "Post Processing Volume" field

#### Step 5: Apply Water Shader
1. Find water mesh in scene
2. Create new material: `WaterSurface_Mat`
3. Assign water shader to material
4. Configure shader properties:
   - Reflection: Enable
   - Refraction: Enable
   - Caustics: Enable
   - Foam: Enable
   - Color: (0.1, 0.3, 0.6) for day
5. Assign material to water mesh

#### Step 6: Create Aurora Object
1. Create empty GameObject: "Aurora"
2. Add quad mesh (or skybox layer)
3. Create material with aurora shader
4. Position above player (or in skybox)
5. Disable by default (EventVFX will enable during Aurora event)

#### Step 7: Test VFX Integration
- [ ] Enter Play mode
- [ ] Cast fishing rod, verify splash and trail VFX
- [ ] Catch fish, verify success particles
- [ ] Pet companion, verify hearts spawn
- [ ] Lower sanity, verify horror VFX
- [ ] Trigger Blood Moon event, verify red effects
- [ ] Test all quality levels (Low/Med/High/Ultra)
- [ ] Verify particle pooling (spawn many effects rapidly)

---

## Asset Alternative Sources

If you don't have dedicated sound designer or VFX artist:

### Audio Sources
**Free**:
- Freesound.org - Community sound effects
- OpenGameArt.org - Free game audio
- Incompetech - Royalty-free music by Kevin MacLeod
- Purple Planet - Free music
- Zapsplat - Free SFX library

**Paid** (Unity Asset Store):
- "Pro Sound Collection" by Glitchedtones
- "Fantasy SFX" by Little Robot Sound Factory
- "Dynamic Music" by Alchemy Audio Solutions

### VFX Sources
**Free**:
- Unity Particle Pack (free)
- Brackeys particle textures (free on GitHub)

**Paid** (Unity Asset Store):
- "Cartoon FX Pack" by Jean Moreno
- "Epic Toon FX" by Moonflower Carnivore
- "Realistic Effects Pack" by VIS Games

---

## Integration Timeline

**Day 1-2**: Import audio files, configure AudioManager
**Day 3-4**: Configure music tracks and SFX library
**Day 5-6**: Configure ambient soundscapes and AudioZones
**Day 7**: Test all audio, balance volumes

**Day 8-9**: Import VFX prefabs, configure VFXManager
**Day 10-11**: Configure particle pooling, assign all prefabs
**Day 12-13**: Configure post-processing, shaders
**Day 14**: Test all VFX at all quality levels

---

## Quality Checklist

### Audio Quality
- [ ] No audio clipping or distortion
- [ ] Seamless music loops (no gaps)
- [ ] Balanced volume levels across all SFX
- [ ] 3D audio positioned correctly
- [ ] Audio ducking works smoothly
- [ ] All 100+ sounds integrated

### VFX Quality
- [ ] Particles look good at all quality levels
- [ ] No excessive particle counts (performance)
- [ ] Satisfying visual feedback for all actions
- [ ] Colors match game art style
- [ ] Post-processing enhances (not distracts)
- [ ] All 78 prefabs integrated

---

**With assets integrated, Bahnfish will come to life!** 🎵✨

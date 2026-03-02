# Audio System Integration Checklist
## Agent 12: Audio System Specialist - Week 25-26

This checklist ensures proper integration of the audio system into Bahnfish.

---

## Phase 1: Unity Scene Setup

### Step 1.1: Create AudioManager GameObject
- [ ] Create empty GameObject named "AudioManager"
- [ ] Add `AudioManager` component
- [ ] Add `MusicSystem` component
- [ ] Add `SoundEffectManager` component
- [ ] Add `AmbientSoundscape` component
- [ ] Add `UIAudioController` component
- [ ] Mark as DontDestroyOnLoad (automatic via scripts)

### Step 1.2: Verify Singleton Setup
- [ ] Only ONE AudioManager should exist in scene
- [ ] AudioManager persists across scene loads
- [ ] All audio components attached to same GameObject

---

## Phase 2: Audio Asset Import

### Step 2.1: Create Audio Folders
```
Assets/Audio/
├── Music/
│   ├── Day/
│   ├── Night/
│   ├── Dusk/
│   ├── Dawn/
│   ├── Fishing/
│   ├── Shop/
│   ├── Menu/
│   └── Boss/
├── SFX/
│   ├── Fishing/
│   ├── Boat/
│   ├── Horror/
│   ├── Companion/
│   ├── UI/
│   ├── Environment/
│   └── Items/
└── Ambient/
    ├── Locations/
    ├── Weather/
    └── Time/
```

### Step 2.2: Configure Audio Import Settings

**Music Tracks:**
- [ ] Format: Vorbis (OGG) or MP3
- [ ] Load Type: **Streaming**
- [ ] Compression Format: Vorbis
- [ ] Quality: 70-100%
- [ ] Preload Audio Data: Unchecked
- [ ] Force to Mono: Unchecked (keep stereo)

**Sound Effects:**
- [ ] Format: WAV or OGG
- [ ] Load Type: **Decompress on Load**
- [ ] Compression Format: PCM (for WAV) or Vorbis (for OGG)
- [ ] Quality: 100%
- [ ] Preload Audio Data: Checked
- [ ] Force to Mono: Check for non-3D sounds

**Ambient Loops:**
- [ ] Format: Vorbis (OGG)
- [ ] Load Type: **Streaming**
- [ ] Compression Format: Vorbis
- [ ] Quality: 70%
- [ ] Preload Audio Data: Unchecked
- [ ] Force to Mono: Check if not using 3D

---

## Phase 3: Audio Clip Assignment

### Step 3.1: Assign Sound Effects
In Unity Inspector (AudioManager → SoundEffectManager):
- [ ] Expand `soundEffects` list (should show 100+ entries)
- [ ] Assign AudioClip to each `clip` field
- [ ] Match clip to ID (e.g., "fishing_cast" → fishing_cast.wav)
- [ ] Verify all 100+ sounds assigned

**TIP**: Use search in Project window to find clips quickly.

### Step 3.2: Create Music Tracks
For each music track type:
- [ ] Create `MusicTrack` ScriptableObject (Assets → Create → Audio → Music Track)
- [ ] Set `trackID`, `trackName`, `trackType`
- [ ] Define layers (base + conditional layers)
- [ ] Assign audio clips to each layer
- [ ] Set layer conditions (e.g., "sanity < 50")
- [ ] Configure crossfade duration

**Required Tracks:**
- [ ] Menu Theme
- [ ] Day Theme (3 layers)
- [ ] Night Theme (4 layers)
- [ ] Dusk Theme
- [ ] Dawn Theme
- [ ] Fishing Theme (2 modes)
- [ ] Shop Theme
- [ ] Boss/Chase Theme

### Step 3.3: Assign Music Tracks
In Unity Inspector (AudioManager → MusicSystem):
- [ ] Expand `musicTracks` list
- [ ] Assign all 8+ MusicTrack ScriptableObjects
- [ ] Verify each track has correct type

### Step 3.4: Assign Ambient Layers
In Unity Inspector (AudioManager → AmbientSoundscape):
- [ ] Expand `ambientLayers` list (should show 50+ entries)
- [ ] Assign AudioClip to each layer's `clip` field
- [ ] Verify layer conditions are correct
- [ ] Set 3D positions for spatial ambient sounds

---

## Phase 4: Audio Zones Setup

### Step 4.1: Create Location Audio Zones
For each of the 13 locations:
- [ ] Create GameObject with BoxCollider (trigger)
- [ ] Add `AudioZone` component
- [ ] Set `zoneID` (e.g., "calm_lake_zone")
- [ ] Set `locationID` (e.g., "calm_lake")
- [ ] Configure reverb preset if needed
- [ ] Position and size trigger collider
- [ ] Set trigger tag to "Player"

**Locations to cover:**
- [ ] Calm Lake
- [ ] Rocky Coastline
- [ ] Deep Ocean
- [ ] Bioluminescent Cavern
- [ ] Abyssal Trench
- [ ] Ghost Ship Graveyard
- [ ] Mangrove Swamp
- [ ] Frozen Waters
- [ ] Coral Reef
- [ ] Volcanic Vent
- [ ] Kelp Forest
- [ ] Shipwreck Cove
- [ ] River Delta

### Step 4.2: Add Reverb Zones
For special areas with reverb:
- [ ] Caves: ReverbPreset.Cave
- [ ] Underwater: ReverbPreset.Underwater
- [ ] Large open areas: ReverbPreset.Mountains

---

## Phase 5: Integration with Existing Systems

### Step 5.1: Fishing System Integration
In `FishingController.cs` (or equivalent):
```csharp
// When casting
EventSystem.Publish("FishingCast");

// When fish bites
EventSystem.Publish("FishHooked");

// When reeling
EventSystem.Publish("ReelStart");

// When caught
EventSystem.Publish("FishCaught");

// When line breaks
EventSystem.Publish("LineBroken");

// When fish jumps
EventSystem.Publish("FishJump");
```

**Checklist:**
- [ ] Cast sound plays when casting
- [ ] Hook sound + tension loop when fish hooked
- [ ] Music switches to fishing track
- [ ] Success jingle when caught
- [ ] Snap sound when line breaks
- [ ] Splash sound at fish position when jumping

### Step 5.2: Boat System Integration
In boat controller:
```csharp
EventSystem.Publish("BoatEngineStart");
EventSystem.Publish("BoatEngineStop");
EventSystem.Publish("BoatCollision");
EventSystem.Publish("AnchorDrop");
EventSystem.Publish("AnchorRaise");
```

**Checklist:**
- [ ] Engine sounds play on start/stop
- [ ] Collision sounds on impacts
- [ ] Anchor sounds play

### Step 5.3: Horror System Integration
In hazard/horror systems:
```csharp
EventSystem.Publish("CrowCaw");
EventSystem.Publish("PhantomScream");
EventSystem.Publish("GhostShipHorn");
EventSystem.Publish("WhisperTrigger");
```

**Checklist:**
- [ ] Horror sounds play at correct 3D positions
- [ ] Low sanity triggers whispers/tinnitus
- [ ] Music adds horror layers at low sanity
- [ ] Chase music plays during pursuit

### Step 5.4: Companion System Integration
In companion system:
```csharp
EventSystem.Publish("PetPetted", petType); // "dog", "cat", etc.
EventSystem.Publish("CompanionAbilityUsed", abilityName);
```

**Checklist:**
- [ ] Pet sounds play when interacting
- [ ] Correct pet sounds for each type (dog, cat, seabird, otter, crab)
- [ ] Ability activation sounds play

### Step 5.5: UI Integration
UI buttons auto-attach audio (if `autoAttachToButtons` enabled).

Manual integration:
```csharp
EventSystem.Publish("MenuOpened");
EventSystem.Publish("MenuClosed");
EventSystem.Publish("ShowNotification", "Message");
EventSystem.Publish("ShowWarning", "Warning");
EventSystem.Publish("ShowError", "Error");
EventSystem.Publish("AchievementUnlocked", "achievement_id");
```

**Checklist:**
- [ ] All buttons have hover/click sounds
- [ ] Menu transitions have sounds
- [ ] Notifications play appropriate sounds
- [ ] Achievement fanfare plays

### Step 5.6: Item/Inventory Integration
```csharp
EventSystem.Publish("ItemPickup");
EventSystem.Publish("ItemSold");
EventSystem.Publish("UpgradePurchased");
EventSystem.Publish("CookingComplete");
EventSystem.Publish("CraftingComplete");
EventSystem.Publish("PhotoTaken");
```

**Checklist:**
- [ ] Pickup sound when collecting items
- [ ] Sell sound + coin jingle when selling
- [ ] Upgrade fanfare when purchasing
- [ ] Completion sounds for cooking/crafting
- [ ] Camera shutter for photography

---

## Phase 6: Testing

### Step 6.1: Basic Audio Test
- [ ] Start game - menu music plays
- [ ] Adjust master volume - all audio changes
- [ ] Adjust music volume - only music changes
- [ ] Adjust SFX volume - only SFX changes
- [ ] Mute music - music stops, SFX continues
- [ ] Volume settings persist after reload

### Step 6.2: Music System Test
- [ ] Day music plays during daytime
- [ ] Night music plays at night
- [ ] Music transitions smoothly (crossfade)
- [ ] Fishing music plays when fishing starts
- [ ] Music layers activate based on sanity
- [ ] Shop music plays when entering shop

### Step 6.3: Sound Effects Test
- [ ] Fishing sounds play during fishing
- [ ] Boat sounds play when moving
- [ ] Horror sounds play at correct 3D positions
- [ ] Companion sounds play when interacting
- [ ] UI sounds play on button clicks
- [ ] All 100+ sounds assigned and working

### Step 6.4: Ambient Soundscape Test
- [ ] Ambient changes when entering new location
- [ ] Weather sounds layer correctly (rain, storm, fog)
- [ ] Time-based ambience (dawn birds, night crickets)
- [ ] 3D ambient sounds positioned correctly
- [ ] Smooth transitions between soundscapes

### Step 6.5: 3D Audio Test
- [ ] Sounds get quieter with distance
- [ ] Sounds positioned correctly in 3D space
- [ ] Doppler effect on moving objects
- [ ] Occlusion works (muffled through walls)
- [ ] Reverb applies in audio zones

### Step 6.6: Performance Test
- [ ] Audio pool not exhausted (check console)
- [ ] No more than 32 concurrent sounds
- [ ] Frame rate stable with full audio
- [ ] No audio crackling or popping
- [ ] Memory usage acceptable

---

## Phase 7: Polish & Optimization

### Step 7.1: Volume Balance
- [ ] Music not too loud (default 0.7)
- [ ] SFX audible but not overwhelming (default 0.8)
- [ ] Ambient subtle but present (default 0.6)
- [ ] UI clear and responsive (default 1.0)
- [ ] Horror sounds impactful but not jarring

### Step 7.2: Mixing & Ducking
- [ ] Music ducks during important sounds
- [ ] Dialog audible over music/ambient
- [ ] No frequency clashing
- [ ] Dynamic range appropriate

### Step 7.3: Audio Variation
- [ ] Splash sounds use random variations
- [ ] Horror sounds don't repeat too often
- [ ] Companion sounds have variety
- [ ] No repetitive loops noticeable

---

## Phase 8: Documentation & Handoff

### Step 8.1: Audio Asset List
- [ ] Create spreadsheet of all audio files
- [ ] Document missing/placeholder audio
- [ ] List priority sounds for sound designer
- [ ] Note any audio bugs or issues

### Step 8.2: Sound Designer Handoff
- [ ] Provide audio specifications (sample rate, format, etc.)
- [ ] Share naming conventions
- [ ] Provide integration examples
- [ ] Set up review process for new audio

---

## Common Issues & Solutions

### Issue: No audio playing
**Solutions:**
- Check AudioManager exists in scene
- Verify AudioListener on camera
- Check volume not muted
- Verify audio clips assigned

### Issue: Audio crackling/popping
**Solutions:**
- Increase audio buffer size (Project Settings → Audio)
- Reduce concurrent sounds
- Check sample rate consistency (44.1kHz or 48kHz)

### Issue: 3D audio not working
**Solutions:**
- Verify AudioClipData has `is3D = true`
- Check min/max distance settings
- Ensure AudioListener exists
- Verify spatial blend is 1.0

### Issue: Music layers not activating
**Solutions:**
- Check layer conditions (e.g., "sanity < 50")
- Verify sanity value is correct
- Enable debug logging in MusicSystem
- Check `autoManageLayers` is enabled

### Issue: Audio pool exhausted
**Solutions:**
- Increase pool size in AudioManager (default 32)
- Stop looping sounds when not needed
- Check for audio leaks (sounds not stopping)

---

## Performance Targets

- [ ] **Max Concurrent Sounds**: 32 or less
- [ ] **Memory Usage**: < 50MB for all audio
- [ ] **CPU Usage**: < 5% for audio processing
- [ ] **Frame Time**: < 1ms for audio system
- [ ] **Load Time**: < 2 seconds for audio initialization

---

## Sign-Off

### Audio System
- [ ] All core systems implemented
- [ ] 100+ sound effects defined
- [ ] 8+ music tracks with layers
- [ ] 13 location soundscapes
- [ ] 3D spatial audio working
- [ ] Save/load integration complete

### Integration
- [ ] Fishing system integrated
- [ ] Boat system integrated
- [ ] Horror system integrated
- [ ] Companion system integrated
- [ ] UI system integrated
- [ ] All events published correctly

### Quality
- [ ] All audio clips assigned
- [ ] Volume levels balanced
- [ ] No audio bugs
- [ ] Performance targets met
- [ ] Documentation complete

---

**Audio System Status**: ✅ Ready for Production
**Completion Date**: Week 25-26
**Agent**: Agent 12 - Audio System Specialist

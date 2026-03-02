# Bahnfish - Parallel Development Agent Architecture

## Overview

This document outlines the specialized development agents that will work in parallel to build Bahnfish. Each agent has clearly defined responsibilities, interfaces, and dependencies to ensure smooth parallel development.

---

## Agent Categories

### 🎯 Core Systems (Priority 1 - Start Immediately)
### 🔧 Gameplay Systems (Priority 2 - Depends on Core)
### 🎨 Content & Polish (Priority 3 - Parallel with Gameplay)
### 🧪 Testing & Integration (Priority 4 - Continuous)

---

## 🎯 CORE SYSTEMS AGENTS

### Agent 1: Core Architecture Agent
**Responsibility**: Foundation, project structure, and core engine setup

**Tasks**:
- Set up Unity/Unreal Engine project
- Establish folder structure and naming conventions
- Create core managers (GameManager, SceneManager, EventSystem)
- Implement design patterns (Singleton, Observer, Factory)
- Set up version control and build pipeline
- Create base classes and interfaces for all systems

**Deliverables**:
```
/Core
  - GameManager.cs
  - EventSystem.cs
  - SaveSystem.cs (interface only)
  - DataTypes.cs (common data structures)
/Interfaces
  - IFishable.cs
  - IInventoryItem.cs
  - IUpgradeable.cs
  - IInteractable.cs
```

**Dependencies**: None (starts first)

**Communication**: Provides base architecture for all other agents

---

### Agent 2: Input & Player Controller Agent
**Responsibility**: Player controls, boat movement, and camera system

**Tasks**:
- Implement boat movement (WASD/controller)
- Create camera system (above/below water split-screen)
- Handle input mapping and rebinding
- Implement boat physics (water resistance, momentum)
- Create interaction system (dock, NPCs, objects)

**Deliverables**:
```
/Player
  - BoatController.cs
  - InputManager.cs
  - CameraController.cs
  - PlayerInteraction.cs
/Physics
  - WaterPhysics.cs
```

**Dependencies**: Agent 1 (Core Architecture)

**Interface Contracts**:
- Exposes `OnPlayerMoved(Vector3 position)` event
- Exposes `OnInteractionTriggered(IInteractable target)` event

---

### Agent 3: Time & Environment Agent
**Responsibility**: Day/night cycle, weather, and time management

**Tasks**:
- Implement real-time day/night cycle (configurable speed)
- Create dynamic lighting system (sun position, ambient light)
- Build weather system (rain, storms, fog, clear)
- Develop 3-day weather forecasting
- Create environmental audio controller
- Implement time-based event triggers

**Deliverables**:
```
/Environment
  - TimeManager.cs
  - DayNightCycle.cs
  - WeatherSystem.cs
  - LightingController.cs
  - EnvironmentalAudio.cs
```

**Dependencies**: Agent 1 (Core Architecture)

**Interface Contracts**:
- Exposes `OnTimeChanged(float currentTime, TimeOfDay period)` event
- Exposes `OnWeatherChanged(WeatherType weather)` event
- Provides `GetCurrentTimeOfDay()` method
- Provides `GetCurrentWeather()` method

---

### Agent 4: Save/Load System Agent
**Responsibility**: Data persistence, cloud saves, and backup

**Tasks**:
- Design save data structure (JSON/binary)
- Implement save/load functionality
- Create auto-save system (configurable intervals)
- Build cloud save integration (Steam Cloud/platform-specific)
- Implement backup and corruption detection
- Create save file migration system for updates

**Deliverables**:
```
/SaveSystem
  - SaveManager.cs
  - SaveData.cs
  - CloudSaveHandler.cs
  - SaveValidator.cs
```

**Dependencies**: Agent 1 (Core Architecture)

**Interface Contracts**:
- Implements `ISaveSystem` interface
- Provides `SaveGame()`, `LoadGame()`, `AutoSave()` methods
- Exposes `OnSaveComplete()`, `OnLoadComplete()` events

---

## 🔧 GAMEPLAY SYSTEMS AGENTS

### Agent 5: Fishing Mechanics Agent
**Responsibility**: Core fishing gameplay loop and mini-games

**Tasks**:
- Implement two-button fishing controls (reel in/let out)
- Create tension system (line stress, breakage)
- Build mini-games (standard reel, harpoon, dredge)
- Develop bite detection and hook mechanics
- Implement different fishing tools (rod, harpoon, nets, crab pots)
- Create visual feedback (line tension indicator, fish struggle)

**Deliverables**:
```
/Fishing
  - FishingController.cs
  - TensionSystem.cs
  - MiniGames/
    - ReelMinigame.cs
    - HarpoonMinigame.cs
    - DredgeMinigame.cs
  - FishingTools/
    - FishingRod.cs
    - Harpoon.cs
    - CrabPot.cs
    - DriftNet.cs
```

**Dependencies**:
- Agent 2 (Input/Player Controller)
- Agent 8 (Fish AI) for fish behavior

**Interface Contracts**:
- Exposes `OnFishCaught(Fish fish)` event
- Exposes `OnLineBroken()` event
- Provides `StartFishing(FishingTool tool)` method

---

### Agent 6: Inventory System Agent
**Responsibility**: Tetris-style inventory, storage, and resource management

**Tasks**:
- Build grid-based inventory system
- Implement drag-and-drop with rotation
- Create different item shapes and sizes
- Develop cooler/premium storage slots
- Build equipment inventory (separate from cargo)
- Implement inventory optimization algorithms
- Create inventory UI with visual feedback

**Deliverables**:
```
/Inventory
  - InventoryManager.cs
  - InventoryGrid.cs
  - InventoryItem.cs
  - ItemShape.cs
  - StorageOptimizer.cs
/UI/Inventory
  - InventoryUI.cs
  - GridCell.cs
  - DragDropHandler.cs
```

**Dependencies**:
- Agent 1 (Core Architecture)
- Agent 5 (Fishing) for caught items

**Interface Contracts**:
- Implements `IInventorySystem` interface
- Provides `AddItem(InventoryItem item)` method
- Exposes `OnInventoryFull()`, `OnItemAdded()` events
- Provides `GetAvailableSpace()` method

---

### Agent 7: Sanity & Horror System Agent
**Responsibility**: Sanity mechanics, horror elements, and night dangers

**Tasks**:
- Implement sanity meter (drain rate, restoration)
- Create insanity effects (hallucinations, distortions)
- Build night hazards (vortexes, ghost ships, creatures)
- Develop curse system
- Implement visual distortions at low sanity
- Create audio distortion system
- Build fog and visibility system

**Deliverables**:
```
/Horror
  - SanityManager.cs
  - InsanityEffects.cs
  - NightHazards/
    - Vortex.cs
    - GhostShip.cs
    - EldritchCreature.cs
  - CurseSystem.cs
  - VisualDistortion.cs
  - FogSystem.cs
```

**Dependencies**:
- Agent 3 (Time/Environment) for day/night state
- Agent 2 (Player Controller) for player position

**Interface Contracts**:
- Provides `GetCurrentSanity()` method
- Exposes `OnSanityChanged(float sanity)` event
- Exposes `OnInsanityTrigger()` event
- Provides `RestoreSanity(float amount)` method

---

### Agent 8: Fish AI & Behavior Agent
**Responsibility**: Fish spawning, behavior, and AI

**Tasks**:
- Create fish behavior AI (movement patterns, depth preferences)
- Implement fish spawning system (location-based, time-based)
- Build fish species database (50+ species)
- Develop aberrant/mutated fish variants
- Create legendary boss fish with unique mechanics
- Implement bait preference system
- Build fish rarity and spawn rate algorithms

**Deliverables**:
```
/Fish
  - FishManager.cs
  - FishAI.cs
  - FishSpawner.cs
  - FishDatabase.cs
  - FishBehavior/
    - NormalBehavior.cs
    - AberrantBehavior.cs
    - LegendaryBehavior.cs
  - FishSpecies/
    - (50+ fish species ScriptableObjects)
```

**Dependencies**:
- Agent 3 (Time/Environment) for spawn conditions
- Agent 14 (Location/World) for spawn locations

**Interface Contracts**:
- Provides `SpawnFish(Location location, TimeOfDay time)` method
- Provides `GetFishInArea(Vector3 position, float radius)` method
- Exposes `OnRareFishSpawned(Fish fish)` event

---

### Agent 9: Progression & Economy Agent
**Responsibility**: Player progression, upgrades, and economy

**Tasks**:
- Implement currency system (money, scrap, relics)
- Create upgrade system (ship, equipment, tools)
- Build shop/vendor system
- Implement location licensing
- Create dark abilities unlock system
- Develop upgrade trees with dependencies
- Balance pricing and progression curve

**Deliverables**:
```
/Progression
  - ProgressionManager.cs
  - EconomySystem.cs
  - UpgradeSystem.cs
  - ShopManager.cs
  - LocationLicenses.cs
  - DarkAbilities.cs
  - UpgradeTree.cs
```

**Dependencies**:
- Agent 6 (Inventory) for currency storage
- Agent 4 (Save/Load) for progression persistence

**Interface Contracts**:
- Provides `PurchaseUpgrade(Upgrade upgrade)` method
- Provides `UnlockLocation(Location location)` method
- Exposes `OnUpgradePurchased(Upgrade upgrade)` event
- Provides `GetPlayerMoney()` method

---

### Agent 10: Quest & Narrative Agent
**Responsibility**: Quests, story, dialogue, and mysteries

**Tasks**:
- Design main mystery storyline
- Create NPC dialogue system
- Implement quest tracking
- Build objective system (catch specific fish, find items)
- Create environmental storytelling elements
- Develop message-in-bottle system
- Implement journal entry discovery

**Deliverables**:
```
/Narrative
  - QuestManager.cs
  - DialogueSystem.cs
  - NPCController.cs
  - StoryProgression.cs
  - EnvironmentalClues.cs
  - JournalSystem.cs
```

**Dependencies**:
- Agent 9 (Progression) for quest rewards
- Agent 4 (Save/Load) for quest state

**Interface Contracts**:
- Provides `StartQuest(Quest quest)` method
- Exposes `OnQuestComplete(Quest quest)` event
- Provides `GetActiveQuests()` method

---

## 🎨 CONTENT & POLISH AGENTS

### Agent 11: UI/UX Agent
**Responsibility**: All user interface elements and menus

**Tasks**:
- Design and implement main menu
- Create HUD (sanity meter, time, fuel, etc.)
- Build pause menu and settings
- Implement journal/encyclopedia UI
- Create shop/upgrade interface
- Design inventory UI (work with Agent 6)
- Build tutorial system
- Create notification system

**Deliverables**:
```
/UI
  - MainMenu.cs
  - HUDManager.cs
  - PauseMenu.cs
  - JournalUI.cs
  - ShopUI.cs
  - TutorialSystem.cs
  - NotificationManager.cs
  - SettingsMenu.cs
```

**Dependencies**: All gameplay agents (needs to display their data)

**Interface Contracts**:
- Provides `ShowNotification(string message)` method
- Provides `UpdateHUD(GameState state)` method
- Exposes UI events for all interactive elements

---

### Agent 12: Audio System Agent
**Responsibility**: Music, sound effects, and dynamic audio

**Tasks**:
- Implement music system (day/night transitions)
- Create audio distortion for low sanity
- Build environmental sound system
- Implement fish-specific audio cues
- Create UI sound effects
- Develop dynamic audio mixer
- Implement 3D spatial audio for world sounds

**Deliverables**:
```
/Audio
  - AudioManager.cs
  - MusicController.cs
  - SFXManager.cs
  - DynamicAudioMixer.cs
  - AudioDistortion.cs
  - SpatialAudio.cs
```

**Dependencies**:
- Agent 3 (Time/Environment) for day/night music
- Agent 7 (Sanity/Horror) for audio distortion

**Interface Contracts**:
- Provides `PlaySound(AudioClip clip, Vector3 position)` method
- Provides `TransitionMusic(MusicTrack track, float fadeTime)` method
- Exposes `OnMusicChanged(MusicTrack track)` event

---

### Agent 13: Visual Effects & Post-Processing Agent
**Responsibility**: Particle effects, shaders, and visual polish

**Tasks**:
- Create water rendering system
- Implement particle effects (splashes, rain, fog)
- Build post-processing effects (color grading, vignette)
- Create sanity-based visual distortions
- Implement underwater caustics and lighting
- Develop weather visual effects
- Create fish catching VFX

**Deliverables**:
```
/VFX
  - WaterRenderer.cs
  - ParticleManager.cs
  - PostProcessingController.cs
  - ShaderEffects.cs
  - UnderwaterEffects.cs
```

**Dependencies**:
- Agent 3 (Time/Environment) for lighting
- Agent 7 (Sanity/Horror) for distortion effects

**Interface Contracts**:
- Provides `ApplyPostProcessing(EffectType type, float intensity)` method
- Provides `SpawnParticleEffect(ParticleType type, Vector3 position)` method

---

### Agent 14: Location & World Generation Agent
**Responsibility**: Creating and managing all 13 fishing locations

**Tasks**:
- Design 13 distinct regions
- Create location-specific assets
- Implement location streaming/loading
- Build navigation system between locations
- Create underwater environments
- Implement hidden areas and secrets
- Design altars and mysterious structures

**Deliverables**:
```
/World
  - LocationManager.cs
  - LocationData/ (13 locations)
  - NavigationSystem.cs
  - SecretAreaManager.cs
  - LocationAssets/
  - UnderwaterScenes/
```

**Dependencies**:
- Agent 1 (Core Architecture)
- Agent 9 (Progression) for location unlocking

**Interface Contracts**:
- Provides `LoadLocation(Location location)` method
- Provides `GetCurrentLocation()` method
- Exposes `OnLocationChanged(Location location)` event

---

### Agent 15: Cooking & Crafting Agent
**Responsibility**: Cooking station and crafting systems

**Tasks**:
- Implement cooking system (recipes, buffs)
- Create bait crafting system
- Build equipment repair/crafting
- Design recipe discovery
- Implement buff/debuff system
- Create crafting UI

**Deliverables**:
```
/Crafting
  - CookingSystem.cs
  - CraftingManager.cs
  - RecipeDatabase.cs
  - BuffSystem.cs
  - CraftingUI.cs
```

**Dependencies**:
- Agent 6 (Inventory) for ingredients
- Agent 9 (Progression) for recipe unlocks

**Interface Contracts**:
- Provides `CookRecipe(Recipe recipe)` method
- Provides `CraftItem(Recipe recipe)` method
- Exposes `OnItemCrafted(Item item)` event

---

### Agent 16: Aquarium & Breeding Agent
**Responsibility**: Aquarium management and fish breeding

**Tasks**:
- Build aquarium display system
- Implement fish breeding mechanics
- Create genetics/mutation system
- Develop visitor system (passive income)
- Build aquarium UI and management
- Implement long-term breeding projects

**Deliverables**:
```
/Aquarium
  - AquariumManager.cs
  - BreedingSystem.cs
  - GeneticsSystem.cs
  - VisitorSystem.cs
  - AquariumUI.cs
  - FishDisplay.cs
```

**Dependencies**:
- Agent 8 (Fish AI) for fish data
- Agent 9 (Progression) for aquarium upgrades

**Interface Contracts**:
- Provides `AddFishToAquarium(Fish fish)` method
- Provides `BreedFish(Fish parent1, Fish parent2)` method
- Exposes `OnNewVariantBred(Fish variant)` event

---

### Agent 17: Crew & Companion System Agent
**Responsibility**: Hiring, managing, and interacting with crew members and pet companions

**Tasks**:
- Create crew member NPC system
- Implement crew abilities and buffs
- Build crew hiring/firing system
- Develop crew backstories and quests
- Create crew UI and management
- Implement crew cost/benefit balancing
- **Build pet companion system** (dog/cat/sea creature)
- **Implement petting interaction mechanic**
- Create pet following AI
- Design pet comfort buff system
- Build pet customization options

**Deliverables**:
```
/Crew
  - CrewManager.cs
  - CrewMember.cs
  - CrewAbilities.cs
  - CrewQuests.cs
  - CrewUI.cs
/Companions
  - PetCompanion.cs
  - PetAI.cs (follow behavior)
  - PetInteraction.cs (petting mechanic)
  - PetCustomization.cs
  - PetComfortSystem.cs
```

**Dependencies**:
- Agent 9 (Progression) for crew wages
- Agent 10 (Quest/Narrative) for crew stories

**Interface Contracts**:
- Provides `HireCrew(CrewMember crew)` method
- Provides `GetActiveCrewBuffs()` method
- Exposes `OnCrewHired(CrewMember crew)` event

---

### Agent 18: Photography & Collection Agent
**Responsibility**: Photo mode and photo challenges

**Tasks**:
- Implement camera/photo mode
- Create photo gallery system
- Build photo challenge system
- Implement photo quality/scoring
- Create photo sharing (optional online)
- Build photo rewards system

**Deliverables**:
```
/Photography
  - PhotoMode.cs
  - PhotoGallery.cs
  - PhotoChallenges.cs
  - PhotoScoring.cs
  - PhotoUI.cs
```

**Dependencies**:
- Agent 2 (Input/Camera) for camera control
- Agent 9 (Progression) for photo rewards

**Interface Contracts**:
- Provides `EnterPhotoMode()` method
- Provides `TakePhoto()` method
- Exposes `OnPhotoTaken(Photo photo)` event

---

### Agent 19: Dynamic Events Agent
**Responsibility**: Random events, festivals, and special occurrences

**Tasks**:
- Implement Blood Moon event
- Create meteor shower system
- Build festival events
- Develop fog bank encounters
- Create fish migration system
- Implement dynamic event scheduler

**Deliverables**:
```
/Events
  - EventManager.cs
  - BloodMoonEvent.cs
  - FestivalSystem.cs
  - MigrationSystem.cs
  - RandomEventSpawner.cs
```

**Dependencies**:
- Agent 3 (Time/Environment) for event timing
- Agent 8 (Fish AI) for migration effects

**Interface Contracts**:
- Exposes `OnEventStarted(GameEvent event)` event
- Exposes `OnEventEnded(GameEvent event)` event
- Provides `GetActiveEvents()` method

---

### Agent 20: Idle/AFK System Agent
**Responsibility**: Offline progress and auto-fishing

**Tasks**:
- Implement auto-fishing algorithm
- Create offline progress calculation
- Build AFK configuration UI
- Implement safety measures (day-only)
- Create notification system
- Balance offline vs active efficiency

**Deliverables**:
```
/Idle
  - IdleManager.cs
  - AutoFishingSystem.cs
  - OfflineProgressCalculator.cs
  - IdleConfigUI.cs
```

**Dependencies**:
- Agent 5 (Fishing) for fishing logic
- Agent 4 (Save/Load) for offline time calculation

**Interface Contracts**:
- Provides `EnableAutoFishing(AutoFishConfig config)` method
- Provides `CalculateOfflineProgress(float timeDelta)` method
- Exposes `OnInventoryFullOffline()` event

---

## 🧪 TESTING & INTEGRATION AGENTS

### Agent 21: Testing & QA Agent
**Responsibility**: Testing all systems and integration

**Tasks**:
- Create unit tests for all systems
- Perform integration testing
- Build automated test suite
- Conduct playtesting sessions
- Bug tracking and reporting
- Performance profiling
- Balance testing

**Deliverables**:
```
/Tests
  - UnitTests/
  - IntegrationTests/
  - PerformanceTests/
  - TestReports/
```

**Dependencies**: All other agents

**Interface Contracts**:
- Provides test reports for all systems
- Identifies integration issues
- Recommends performance optimizations

---

### Agent 22: Build & Deployment Agent
**Responsibility**: Building, packaging, and deploying the game

**Tasks**:
- Set up build pipeline (CI/CD)
- Configure platform-specific builds
- Implement version management
- Create installer/package scripts
- Set up Steam/platform integration
- Build release checklist

**Deliverables**:
```
/Build
  - BuildPipeline/
  - PlatformConfigs/
  - VersionManager.cs
  - ReleaseChecklist.md
```

**Dependencies**: All other agents

**Interface Contracts**:
- Provides automated builds
- Generates build reports
- Manages versioning

---

## Parallel Development Strategy

### Phase 1: Foundation (Weeks 1-4)
**Start in Parallel**:
- Agent 1: Core Architecture
- Agent 2: Input/Player Controller
- Agent 3: Time/Environment
- Agent 4: Save/Load System

**Goal**: Establish foundation with player movement, basic time system, and save/load

---

### Phase 2: Core Gameplay (Weeks 5-10)
**Start in Parallel** (after Phase 1):
- Agent 5: Fishing Mechanics
- Agent 6: Inventory System
- Agent 7: Sanity/Horror
- Agent 8: Fish AI & Behavior
- Agent 11: UI/UX (HUD basics)

**Goal**: Playable core loop - fish, store, manage sanity

---

### Phase 3: Progression & Content (Weeks 11-16)
**Start in Parallel** (after Phase 2):
- Agent 9: Progression/Economy
- Agent 10: Quest/Narrative
- Agent 14: Locations/World
- Agent 19: Dynamic Events

**Goal**: Full progression system with multiple locations and quests

---

### Phase 4: Feature Expansion (Weeks 17-24)
**Start in Parallel** (after Phase 3):
- Agent 15: Cooking/Crafting
- Agent 16: Aquarium/Breeding
- Agent 17: Crew System
- Agent 18: Photography
- Agent 20: Idle/AFK

**Goal**: All innovative features implemented

---

### Phase 5: Polish & Audio/Visual (Weeks 25-30)
**Start in Parallel** (after Phase 4):
- Agent 12: Audio System
- Agent 13: VFX/Post-Processing
- Agent 11: UI/UX (final polish)

**Goal**: Professional visual and audio quality

---

### Phase 6: Testing & Release (Weeks 31-36)
**Start in Parallel**:
- Agent 21: Testing/QA
- Agent 22: Build/Deployment

**Goal**: Bug-free, polished release build

---

## Communication Protocols

### Daily Sync
- Each agent posts progress updates
- Identifies blocking issues
- Requests interface clarifications

### Weekly Integration
- Merge all agent work into main branch
- Integration testing
- Resolve conflicts
- Update documentation

### Interface Changes
- Any interface change must be communicated immediately
- All dependent agents must acknowledge before implementation
- Version interfaces to prevent breaking changes

---

## Shared Resources

### Common Data Structures
```csharp
// Fish.cs - Used by multiple agents
public class Fish {
    public string id;
    public string name;
    public FishRarity rarity;
    public float baseValue;
    public Vector2Int inventorySize;
    public FishBehavior behavior;
    public bool isAberrant;
    public Sprite icon;
}

// GameState.cs - Shared state
public class GameState {
    public float currentTime;
    public TimeOfDay timeOfDay;
    public WeatherType weather;
    public float sanity;
    public Vector3 playerPosition;
    public Location currentLocation;
}
```

### Event System
All agents publish events to central EventSystem for loose coupling:
```csharp
EventSystem.Publish("FishCaught", fishData);
EventSystem.Subscribe("TimeChanged", OnTimeChanged);
```

---

## Success Metrics

### Per Agent
- All deliverables completed on time
- Unit test coverage > 80%
- No critical bugs in assigned systems
- Documentation complete
- Interfaces stable (no breaking changes in final phase)

### Overall Integration
- All systems work together seamlessly
- 60 FPS on target hardware
- Save/load works with all systems
- No game-breaking bugs
- Smooth player experience across all features

---

## Conclusion

This parallel agent architecture allows for efficient development of Bahnfish by dividing work into specialized, loosely-coupled systems. By clearly defining interfaces and dependencies, agents can work simultaneously with minimal conflicts, significantly reducing overall development time while maintaining code quality and system integrity.

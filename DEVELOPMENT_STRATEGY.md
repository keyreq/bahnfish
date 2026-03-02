# Bahnfish - Development Strategy & Implementation Guide

## Overview

This document outlines the practical strategy for executing parallel development of Bahnfish using the 22 specialized agents defined in AGENTS_DESIGN.md. It includes launch sequences, coordination methods, and best practices for efficient parallel development.

---

## Technology Stack

### Game Engine
- **Unity 2022 LTS** (recommended for indie scope)
  - Easier for 2D/3D hybrid
  - Large asset store
  - Better indie community support
  - Lighter than Unreal for this scope

**Alternative**: Unreal Engine 5 (if team has experience)

### Languages
- **C#** (Unity) or **C++/Blueprint** (Unreal)
- **JSON** for data files
- **YAML** for configuration

### Version Control
- **Git** with GitHub/GitLab
- **Git LFS** for large assets (textures, audio, models)
- **Branch strategy**: Feature branches per agent

### Project Management
- **Jira/Linear** for task tracking
- **Discord** for real-time communication
- **Confluence/Notion** for documentation

### CI/CD
- **GitHub Actions** or **Unity Cloud Build**
- Automated testing on commit
- Nightly builds for integration testing

---

## Development Phases

## PHASE 1: FOUNDATION (Weeks 1-4)

### Objective
Establish core architecture, player controls, time system, and persistence.

### Agent Launch Sequence

#### Week 1: Core Setup
```bash
# Launch these agents in parallel on Day 1
Agent 1: Core Architecture        [PRIORITY: CRITICAL]
Agent 4: Save/Load System         [PRIORITY: HIGH]
```

**Agent 1 Deliverables (Week 1)**:
- Unity project structure
- Core folder hierarchy
- GameManager singleton
- EventSystem implementation
- Base interfaces (IFishable, IInventoryItem, etc.)
- ScriptableObject templates

**Agent 4 Deliverables (Week 1)**:
- SaveData class structure (empty, to be populated)
- SaveManager with Save/Load methods
- JSON serialization setup
- Save file path management

#### Week 2-3: Player & Environment
```bash
# Launch after Agent 1 core is ready
Agent 2: Input/Player Controller  [PRIORITY: HIGH]
Agent 3: Time/Environment         [PRIORITY: HIGH]
```

**Agent 2 Deliverables (Week 2-3)**:
- Boat movement (WASD + mouse/controller)
- Basic boat physics (floating, momentum)
- Camera system (single view first, split-screen later)
- Input rebinding system

**Agent 3 Deliverables (Week 2-3)**:
- Day/night cycle (adjustable speed)
- Sun position and lighting
- TimeManager with events
- Basic skybox transitions

#### Week 4: Integration & Testing
- Integrate all Phase 1 agents
- Test: Player can move boat around
- Test: Time progresses and lighting changes
- Test: Game can save and load player position/time
- Fix integration bugs
- Prepare for Phase 2

**Phase 1 Milestone**: Playable prototype with boat movement, time cycle, and save/load.

---

## PHASE 2: CORE GAMEPLAY (Weeks 5-10)

### Objective
Implement fishing mechanics, inventory, sanity system, and fish AI to create the core gameplay loop.

### Agent Launch Sequence

#### Week 5-6: Fishing & Fish AI
```bash
# Launch in parallel (both need basic implementations)
Agent 5: Fishing Mechanics        [PRIORITY: CRITICAL]
Agent 8: Fish AI & Behavior       [PRIORITY: CRITICAL]
```

**Agent 5 Focus**:
- Two-button fishing control
- Tension system (line stress)
- Basic reel mini-game
- Hook/catch mechanics
- Visual feedback (tension bar)

**Agent 8 Focus**:
- Fish spawning system
- Basic fish AI (movement patterns)
- 15 fish species for prototype (scriptable objects)
- Depth preference system
- Fish database structure

**Integration Point**: Agent 5 and 8 must sync on fish catching interface by end of Week 6.

#### Week 7-8: Inventory & Sanity
```bash
# Launch in parallel
Agent 6: Inventory System         [PRIORITY: HIGH]
Agent 7: Sanity/Horror            [PRIORITY: HIGH]
```

**Agent 6 Focus**:
- Grid-based inventory (Tetris-style)
- Drag and drop functionality
- Item shapes (fish have different sizes)
- Inventory UI
- Add/remove item logic

**Agent 7 Focus**:
- Sanity meter implementation
- Sanity drain based on time of day
- Basic night hazards (1-2 types)
- Visual effects for low sanity
- Restore sanity at dock

#### Week 9: UI Foundation
```bash
Agent 11: UI/UX (Partial)         [PRIORITY: MEDIUM]
```

**Agent 11 Focus (Week 9)**:
- Basic HUD (sanity, time, money)
- Inventory UI integration
- Simple menu system
- Tutorial prompts

#### Week 10: Phase 2 Integration
- Integrate fishing, inventory, sanity, and UI
- Test core loop: catch fish → store in inventory → manage sanity → return to dock
- Balance fishing difficulty
- Fix integration issues
- Playtest and iterate

**Phase 2 Milestone**: Complete core gameplay loop functional.

---

## PHASE 3: PROGRESSION & CONTENT (Weeks 11-16)

### Objective
Add progression systems, multiple locations, quests, and economic systems.

### Agent Launch Sequence

#### Week 11-12: Economy & Progression
```bash
Agent 9: Progression/Economy      [PRIORITY: HIGH]
```

**Agent 9 Focus**:
- Currency system (money, scrap)
- Shop/vendor system
- Ship upgrades (hull, engine, lights)
- Upgrade UI
- Pricing and balance

#### Week 13-14: World & Locations
```bash
Agent 14: Locations/World         [PRIORITY: HIGH]
```

**Agent 14 Focus**:
- 5 distinct locations (to start)
- Location loading/streaming
- Navigation between locations
- Location-specific fish spawn lists (work with Agent 8)
- Travel system (fuel consumption)

**Coordination**: Agent 14 must work with Agent 8 to define which fish spawn in which locations.

#### Week 15-16: Quests & Events
```bash
# Launch in parallel
Agent 10: Quest/Narrative         [PRIORITY: MEDIUM]
Agent 19: Dynamic Events          [PRIORITY: MEDIUM]
```

**Agent 10 Focus**:
- Quest system (fetch quests, catch specific fish)
- 5-10 starter quests
- NPC dialogue system
- Quest tracking UI

**Agent 19 Focus**:
- Blood Moon event
- Random fog banks
- Fish migration system
- Event scheduler

#### Week 16: Phase 3 Integration
- Integrate economy, locations, quests
- Test progression: start → earn money → buy upgrades → unlock locations
- Balance economy
- Playtest progression curve

**Phase 3 Milestone**: Full progression loop with multiple locations and upgrade path.

---

## PHASE 4: FEATURE EXPANSION (Weeks 17-24)

### Objective
Implement advanced features: cooking, aquarium, crew, photography, idle systems.

### Agent Launch Sequence

#### Week 17-18: Cooking & Crafting
```bash
Agent 15: Cooking/Crafting        [PRIORITY: MEDIUM]
```

**Agent 15 Focus**:
- Cooking station
- 10-15 recipes
- Buff system
- Bait crafting
- Recipe UI

#### Week 19-20: Aquarium & Breeding
```bash
Agent 16: Aquarium/Breeding       [PRIORITY: MEDIUM]
```

**Agent 16 Focus**:
- Aquarium system
- Fish display
- Basic breeding mechanics
- Genetics system (simplified for MVP)
- Visitor/passive income

#### Week 21-22: Crew & Photography
```bash
# Launch in parallel
Agent 17: Crew System             [PRIORITY: LOW]
Agent 18: Photography             [PRIORITY: LOW]
```

**Agent 17 Focus**:
- 4-5 crew members
- Crew abilities
- Hiring system
- Crew UI

**Agent 18 Focus**:
- Photo mode
- Photo gallery
- 5-10 photo challenges
- Photo scoring

#### Week 23-24: Idle/AFK System
```bash
Agent 20: Idle/AFK                [PRIORITY: MEDIUM]
```

**Agent 20 Focus**:
- Auto-fishing algorithm
- Offline progress calculation
- Configuration UI
- Balance offline vs active play

#### Week 24: Phase 4 Integration
- Integrate all Phase 4 features
- Test each feature independently
- Test feature interactions
- Balance feature rewards

**Phase 4 Milestone**: All major features implemented.

---

## PHASE 5: POLISH (Weeks 25-30)

### Objective
Add professional audio, visual effects, and UI polish.

### Agent Launch Sequence

#### Week 25-26: Audio System
```bash
Agent 12: Audio System            [PRIORITY: HIGH]
```

**Agent 12 Focus**:
- Music system (day/night transitions)
- Sound effects for all actions
- Audio distortion (sanity)
- Ambient soundscapes
- Audio mixer

#### Week 27-28: Visual Effects
```bash
Agent 13: VFX/Post-Processing     [PRIORITY: HIGH]
```

**Agent 13 Focus**:
- Water rendering
- Particle effects (splashes, rain)
- Post-processing (color grading)
- Sanity visual distortions
- Weather VFX

#### Week 29-30: UI Polish
```bash
Agent 11: UI/UX (Final Polish)    [PRIORITY: MEDIUM]
```

**Agent 11 Focus (Final)**:
- UI animations
- Transitions and feedback
- Polish all menus
- Settings menu
- Accessibility options
- Tutorial refinement

**Phase 5 Milestone**: Game is visually and aurally polished.

---

## PHASE 6: TESTING & RELEASE (Weeks 31-36)

### Objective
Test, fix bugs, optimize performance, and prepare for launch.

### Agent Launch Sequence

#### Week 31-34: Testing & QA
```bash
Agent 21: Testing/QA              [PRIORITY: CRITICAL]
```

**Agent 21 Focus**:
- Comprehensive playthrough
- Bug tracking and fixing
- Balance testing
- Performance profiling
- Save/load stress testing
- Edge case testing

#### Week 35-36: Build & Deploy
```bash
Agent 22: Build/Deployment        [PRIORITY: CRITICAL]
```

**Agent 22 Focus**:
- Build pipeline setup
- Platform-specific builds (Windows, Mac, Linux)
- Steam integration
- Release builds
- Installer creation

**Phase 6 Milestone**: Game is ready for release.

---

## Parallel Development Best Practices

### 1. Interface-First Development

Each agent should define their public interfaces FIRST before implementation:

```csharp
// Example: Agent 5 (Fishing) defines interface first
public interface IFishingSystem {
    void StartFishing(FishingTool tool);
    void StopFishing();
    event Action<Fish> OnFishCaught;
    event Action OnLineBroken;
}

// Then implements
public class FishingController : IFishingSystem {
    // Implementation
}
```

**Benefit**: Other agents can work with interfaces immediately, even if implementation isn't complete.

### 2. Mock Data for Parallel Work

Agents should create mock/placeholder data to avoid blocking:

```csharp
// Agent 8 (Fish AI) creates mock fish immediately
public class FishDatabase {
    public static Fish GetMockFish() {
        return new Fish {
            id = "mock_bass",
            name = "Mock Bass",
            // ... other properties
        };
    }
}
```

**Benefit**: Agent 5 (Fishing) can test catching fish without waiting for full fish database.

### 3. Feature Flags

Use feature flags to enable/disable incomplete features:

```csharp
public class FeatureFlags {
    public static bool COOKING_ENABLED = false;
    public static bool AQUARIUM_ENABLED = false;
    public static bool PHOTOGRAPHY_ENABLED = false;
}

// In code
if (FeatureFlags.COOKING_ENABLED) {
    ShowCookingUI();
}
```

**Benefit**: Incomplete features don't break main build.

### 4. Daily Integration Builds

- Every agent commits to their feature branch daily
- Automated CI runs tests and creates integration build
- Team tests integration build each morning
- Integration issues identified early

### 5. Weekly Sync Meetings

**Agenda**:
- Each agent reports progress (5 min max)
- Identify blocking issues
- Discuss interface changes
- Plan next week's priorities
- Demo new features

### 6. Documentation as You Go

Each agent maintains their own README:

```
/Fishing/README.md
  - How to use FishingSystem
  - Public API documentation
  - Known issues
  - Future improvements

/Inventory/README.md
  - How to add items
  - Inventory API
  - Grid system explanation
```

### 7. Shared Data Format

All agents use consistent data formats:

```json
// Fish data format (agreed upon by all agents)
{
  "id": "bass_001",
  "name": "Largemouth Bass",
  "rarity": "common",
  "baseValue": 15,
  "inventorySize": { "x": 2, "y": 1 },
  "behavior": "cautious",
  "minDepth": 2,
  "maxDepth": 10,
  "preferredBait": ["worm", "lure_small"]
}
```

---

## Risk Management

### Risk 1: Integration Conflicts
**Mitigation**:
- Clear interface contracts defined upfront
- Weekly integration testing
- Feature branches with pull request reviews
- Automated merge conflict detection

### Risk 2: Agent Blocking
**Mitigation**:
- Mock data for dependencies
- Interface-first development
- Flexible sprint planning (agents can pivot if blocked)

### Risk 3: Scope Creep
**Mitigation**:
- Strict adherence to GAME_DESIGN.md
- Feature freeze after Phase 4
- "Nice to have" list for post-launch updates

### Risk 4: Performance Issues
**Mitigation**:
- Performance profiling throughout development (not just at end)
- Regular performance testing on target hardware
- Agent 21 (Testing) monitors performance metrics

### Risk 5: Save System Corruption
**Mitigation**:
- Robust save validation from Agent 4
- Versioned save format
- Automatic backups
- Migration system for updates

---

## Communication Channels

### Discord Channels
- `#general` - General discussion
- `#agent-[number]` - Per-agent channels (e.g., #agent-05-fishing)
- `#integration` - Integration issues and blockers
- `#design` - Design discussions and changes
- `#builds` - Build notifications and issues

### GitHub
- Feature branches: `agent-[number]-[feature-name]`
- Pull requests reviewed by at least 1 other agent
- Issues tagged by agent (e.g., `agent-05`, `bug`, `integration`)

### Documentation
- Living documents in `/Docs` folder
- Updated weekly
- Version controlled

---

## Success Metrics (Weekly Check)

### Phase 1 Success
- [ ] Player can move boat smoothly
- [ ] Time cycles through day/night
- [ ] Game saves and loads correctly
- [ ] No critical bugs

### Phase 2 Success
- [ ] Player can catch fish
- [ ] Fish appear in inventory
- [ ] Sanity drains at night
- [ ] Core loop is fun (subjective but important)

### Phase 3 Success
- [ ] Player can earn money and buy upgrades
- [ ] Multiple locations accessible
- [ ] Quests are completable
- [ ] Progression feels rewarding

### Phase 4 Success
- [ ] All features functional
- [ ] Features enhance core loop
- [ ] No game-breaking bugs

### Phase 5 Success
- [ ] Game looks polished
- [ ] Audio enhances experience
- [ ] UI is intuitive

### Phase 6 Success
- [ ] No critical bugs
- [ ] Performance hits 60 FPS target
- [ ] Save/load is reliable
- [ ] Game is ready to ship

---

## Launch Readiness Checklist

### Technical
- [ ] All 22 agents have completed deliverables
- [ ] All unit tests passing
- [ ] Integration tests passing
- [ ] Performance meets 60 FPS on target hardware
- [ ] Save/load tested extensively (no corruption)
- [ ] All platforms build successfully
- [ ] No critical or high-priority bugs

### Content
- [ ] 50+ fish species implemented
- [ ] 13 locations complete
- [ ] Main mystery questline complete
- [ ] All upgrade trees implemented
- [ ] Tutorial teaches all mechanics

### Polish
- [ ] All audio implemented
- [ ] All VFX implemented
- [ ] UI is polished and intuitive
- [ ] No placeholder assets
- [ ] Settings menu complete

### Business
- [ ] Steam page ready
- [ ] Marketing materials prepared
- [ ] Press kit available
- [ ] Trailer created
- [ ] Launch date set

---

## Post-Launch Plan

### Week 1-2: Hotfixes
- Monitor for critical bugs
- Quick patches for game-breaking issues
- Gather player feedback

### Month 1-3: Updates
- Balance patches based on player data
- QoL improvements
- Bug fixes

### Month 4-6: Content Updates
- New fish species
- Seasonal events
- Community-requested features

### Month 7-12: DLC Planning
- Plan expansion content
- New regions
- Story expansions

---

## Conclusion

By following this parallel development strategy with clearly defined agent responsibilities, interfaces, and communication protocols, Bahnfish can be developed efficiently in 36 weeks (9 months) with a team of 22 specialized developers/agents.

The key to success is:
1. **Clear interfaces** defined before implementation
2. **Frequent integration** to catch issues early
3. **Mock data** to prevent blocking
4. **Feature flags** to isolate incomplete work
5. **Consistent communication** through Discord and GitHub
6. **Strict adherence** to design documents
7. **Regular testing** throughout development

With this structure, Bahnfish will be a polished, feature-rich fishing game that successfully blends cozy mechanics with atmospheric horror, creating a unique and engaging experience for players.

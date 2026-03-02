# 🎣 Bahnfish

**A unique fishing game blending cozy mechanics with atmospheric horror**

Bahnfish combines the relaxing, meditative fishing of *Cast n Chill* with the tense, mysterious atmosphere of *Dredge*. Experience a dynamic day/night cycle where peaceful daytime fishing transforms into nerve-wracking nighttime expeditions filled with supernatural encounters and rare discoveries.

---

## 📚 Project Documentation

This repository contains comprehensive documentation for the parallel development of Bahnfish:

### Core Documents

1. **[GAME_DESIGN.md](GAME_DESIGN.md)** - Complete game design document
   - Core mechanics and systems
   - Progression and content structure
   - Visual and audio design
   - Technical considerations
   - Development roadmap

2. **[AGENTS_DESIGN.md](AGENTS_DESIGN.md)** - Parallel development agent architecture
   - 22 specialized development agents
   - Agent responsibilities and deliverables
   - Dependencies and interfaces
   - Communication protocols

3. **[DEVELOPMENT_STRATEGY.md](DEVELOPMENT_STRATEGY.md)** - Implementation strategy
   - Phase-by-phase development plan
   - Agent launch sequences
   - Best practices for parallel development
   - Risk management
   - Success metrics

---

## 🎮 Game Overview

### Core Pillars
- 🌊 **Relaxing Yet Engaging** - Simple controls with strategic depth
- ⚖️ **Risk vs Reward** - Push your luck at night for rare catches
- 🗺️ **Progressive Discovery** - Unlock secrets, locations, and mysteries
- 🌙 **Atmospheric Tension** - Dynamic mood shifts between calm and eerie
- 📈 **Meaningful Progression** - Every session advances your capabilities

### Key Features

#### Fishing Mechanics
- Two-button control system (reel in/let out)
- Split-screen view (above/below water)
- Multiple fishing tools (rod, harpoon, nets, crab pots, dredge)
- 50+ unique fish species with distinct behaviors

#### Day/Night Cycle
- **Day**: Peaceful fishing, safe exploration
- **Night**: Rare fish, supernatural hazards, sanity drain
- Dynamic lighting and atmosphere

#### Sanity System
- Depletes at night, restored at dock
- Low sanity triggers hallucinations and dangers
- Manageable through upgrades and consumables

#### Tetris-Style Inventory
- Grid-based storage puzzle
- Different fish shapes optimize packing
- Upgradeable cargo space

#### Progression
- 13 distinct fishing locations
- Ship upgrades (hull, engine, lights, armor)
- Dark abilities unlocked through relics
- Economy system (money, scrap, relics)

#### Innovative Features
- 🍳 **Cooking & Crafting** - Prepare buffs and custom bait
- 🐠 **Aquarium & Breeding** - Collect and breed fish variants
- 👥 **Crew System** - Hire NPCs with unique abilities
- 📷 **Photography Mode** - Document your adventures
- ⏰ **Idle/AFK System** - Offline progress
- 🌧️ **Dynamic Weather** - Affects gameplay and fish spawns
- 🔮 **Curse & Blessing System** - Risk/reward with supernatural elements

---

## 🏗️ Development Structure

### 22 Specialized Agents

The game is being developed using 22 specialized agents working in parallel:

#### 🎯 Core Systems (Priority 1)
1. **Core Architecture** - Foundation and project structure
2. **Input & Player Controller** - Boat movement and controls
3. **Time & Environment** - Day/night cycle and weather
4. **Save/Load System** - Data persistence

#### 🔧 Gameplay Systems (Priority 2)
5. **Fishing Mechanics** - Core fishing gameplay
6. **Inventory System** - Tetris-style storage
7. **Sanity & Horror** - Night dangers and effects
8. **Fish AI & Behavior** - Fish spawning and AI
9. **Progression & Economy** - Upgrades and currency
10. **Quest & Narrative** - Story and missions

#### 🎨 Content & Polish (Priority 3)
11. **UI/UX** - All interface elements
12. **Audio System** - Music and sound effects
13. **Visual Effects** - Particles and post-processing
14. **Location & World** - 13 fishing regions
15. **Cooking & Crafting** - Recipes and items
16. **Aquarium & Breeding** - Collection system
17. **Crew System** - NPC companions
18. **Photography** - Photo mode and challenges
19. **Dynamic Events** - Special occurrences
20. **Idle/AFK** - Offline progress

#### 🧪 Testing & Integration (Priority 4)
21. **Testing & QA** - Quality assurance
22. **Build & Deployment** - Release pipeline

### Development Phases

**Phase 1: Foundation** (Weeks 1-4)
- Core architecture, player controls, time system, save/load

**Phase 2: Core Gameplay** (Weeks 5-10)
- Fishing, inventory, sanity, fish AI, basic UI

**Phase 3: Progression & Content** (Weeks 11-16)
- Economy, locations, quests, events

**Phase 4: Feature Expansion** (Weeks 17-24)
- Cooking, aquarium, crew, photography, idle

**Phase 5: Polish** (Weeks 25-30)
- Audio, VFX, UI polish

**Phase 6: Testing & Release** (Weeks 31-36)
- QA, optimization, deployment

---

## 🚀 Getting Started

### Prerequisites
- Unity 2022 LTS (or Unreal Engine 5)
- Git with Git LFS
- C# development environment
- 16GB RAM minimum
- GPU with DirectX 11+ support

### Initial Setup

1. **Clone the repository**
   ```bash
   git clone https://github.com/yourusername/bahnfish.git
   cd bahnfish
   ```

2. **Review documentation**
   - Read [GAME_DESIGN.md](GAME_DESIGN.md) for game overview
   - Read [AGENTS_DESIGN.md](AGENTS_DESIGN.md) for architecture
   - Read [DEVELOPMENT_STRATEGY.md](DEVELOPMENT_STRATEGY.md) for implementation plan

3. **Set up development environment**
   - Install Unity 2022 LTS
   - Configure Git LFS for large assets
   - Join Discord for team communication

4. **Choose your agent**
   - Review agent responsibilities in AGENTS_DESIGN.md
   - Coordinate with team on agent assignment
   - Create feature branch: `agent-[number]-[feature-name]`

### Development Workflow

1. **Define interfaces first**
   ```csharp
   // Example: Define public interface before implementation
   public interface IFishingSystem {
       void StartFishing(FishingTool tool);
       event Action<Fish> OnFishCaught;
   }
   ```

2. **Create mock data for dependencies**
   ```csharp
   // Use mock data to avoid blocking
   public static Fish GetMockFish() {
       return new Fish { id = "mock", name = "Mock Fish" };
   }
   ```

3. **Use feature flags**
   ```csharp
   if (FeatureFlags.MY_FEATURE_ENABLED) {
       // Feature code
   }
   ```

4. **Commit daily to feature branch**
   ```bash
   git add .
   git commit -m "Agent-05: Implement fishing tension system"
   git push origin agent-05-fishing
   ```

5. **Create pull request when ready**
   - Request review from at least one other agent
   - Ensure tests pass
   - Update documentation

---

## 📋 Project Status

### Current Phase
**Planning Complete** ✅
- Game design finalized
- Agent architecture defined
- Development strategy outlined

### Next Steps
1. Set up Unity project (Agent 1)
2. Initialize Git repository with proper structure
3. Create base classes and interfaces
4. Begin Phase 1 development

### Milestones
- [ ] Phase 1: Foundation (Week 4)
- [ ] Phase 2: Core Gameplay (Week 10)
- [ ] Phase 3: Progression & Content (Week 16)
- [ ] Phase 4: Feature Expansion (Week 24)
- [ ] Phase 5: Polish (Week 30)
- [ ] Phase 6: Testing & Release (Week 36)

---

## 🤝 Contributing

### For Team Members

1. Choose an unassigned agent from AGENTS_DESIGN.md
2. Create feature branch following naming convention
3. Define public interfaces first
4. Implement according to agent deliverables
5. Write unit tests (80%+ coverage)
6. Document your code and APIs
7. Submit pull request with description

### Communication

- **Discord**: Real-time discussion and blockers
- **GitHub Issues**: Bug reports and feature requests
- **Pull Requests**: Code reviews and integration
- **Weekly Meetings**: Progress updates and planning

### Code Standards

- Follow C# naming conventions
- Comment complex logic
- Write self-documenting code
- Include XML documentation for public APIs
- Maintain 80%+ test coverage

---

## 📊 Technical Specifications

### Target Platforms
- **Primary**: PC (Windows, Mac, Linux)
- **Future**: Console (Switch, PlayStation, Xbox)
- **Mobile**: Simplified idle-focused version

### Performance Targets
- 60 FPS on mid-range hardware
- < 3 second load times
- < 4GB RAM usage
- Smooth gameplay at 1920x1080

### Minimum Requirements (PC)
- OS: Windows 10 / macOS 10.15 / Ubuntu 20.04
- Processor: Intel i5-6600K / AMD Ryzen 5 1600
- Memory: 8 GB RAM
- Graphics: GTX 960 / RX 570
- Storage: 5 GB available space

### Recommended Requirements (PC)
- OS: Windows 11 / macOS 13 / Ubuntu 22.04
- Processor: Intel i7-8700K / AMD Ryzen 7 3700X
- Memory: 16 GB RAM
- Graphics: RTX 2060 / RX 5700
- Storage: 5 GB SSD

---

## 🎨 Art & Audio Direction

### Visual Style
- Clean, stylized 3D art
- Cozy aesthetic during day
- Eerie, atmospheric horror at night
- Smooth transitions between moods

### Color Palette
- **Day**: Warm pastels, inviting
- **Dusk/Dawn**: Golden oranges, deep purples
- **Night**: Desaturated blues, eerie greens

### Audio
- Calm acoustic music (day)
- Distorted atmospheric music (night)
- Ambient soundscapes (water, wildlife)
- Distinct audio cues for fish and events

---

## 📈 Success Metrics

### Player Engagement
- Average session: 30-60 minutes
- Return rate: 70%+ within 48 hours
- Completion rate: 40%+ finish main story
- Collection rate: 60%+ catch all common fish

### Business Goals
- 100,000 units sold in Year 1
- 80+ Metacritic score
- 10,000+ Discord members
- Active content creator community

---

## 📝 License

[To be determined - suggest MIT or similar permissive license]

---

## 🙏 Acknowledgments

### Inspirations
- **Cast n Chill** - Cozy fishing mechanics and idle gameplay
- **Dredge** - Atmospheric horror and sanity system
- **Stardew Valley** - Progression and collection systems
- **Subnautica** - Underwater exploration and tension

### Tools & Resources
- Unity Technologies
- Git & GitHub
- Discord
- [Add asset store assets, libraries, etc.]

---

## 📞 Contact

- **Discord**: [Your Discord Server]
- **Email**: [Your Email]
- **Twitter**: [Your Twitter]
- **Website**: [Your Website]

---

## 🗺️ Roadmap

### Pre-Alpha (Months 1-3)
- Core mechanics implemented
- Basic gameplay loop functional
- Internal testing

### Alpha (Months 4-6)
- Horror elements integrated
- Multiple locations
- Closed alpha testing

### Beta (Months 7-9)
- All features implemented
- Content complete
- Open beta testing

### Release (Month 9-10)
- Polish and optimization
- Bug fixing
- Marketing preparation
- Launch!

### Post-Launch
- Hotfixes and patches
- Community feedback integration
- Seasonal events
- DLC planning

---

**Built with ❤️ by the Bahnfish Team**

*Dive into the depths... if you dare.*

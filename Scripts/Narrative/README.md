# Bahnfish Narrative System

## Overview

The Narrative System is the storytelling heart of Bahnfish, managing quests, NPCs, dialogue, environmental clues, and the main mystery storyline. It creates an atmospheric, slow-burn horror narrative that unfolds as players explore the cursed waters.

## Core Components

### Quest System
- **QuestManager.cs** - Central quest tracking and progression
- **Quest.cs** - Quest data structure
- **QuestObjective.cs** - Individual objective tracking
- **QuestReward.cs** - Reward handling
- **QuestDatabase.cs** - All 30+ quests

### NPC System
- **NPCController.cs** - Individual NPC behavior and interaction
- **NPCData.cs** - NPC definitions and personality traits
- **NPCDatabase.cs** - All 12 unique NPCs

### Dialogue System
- **DialogueSystem.cs** - Branching conversation management
- **DialogueTree.cs** - Node-based dialogue structure

### Story Progression
- **StoryProgression.cs** - Main mystery tracking (Acts 1-5)
- **EnvironmentalClues.cs** - Messages in bottles, carvings, etc.
- **JournalSystem.cs** - Quest log and lore entries (50+ entries)

## Main Mystery: "The Deep One"

### The Story

**Premise**: Players arrive as a new fisher at a mysterious coastal town. The previous fisher, Marcus, vanished months ago. As players explore, they discover:

1. The waters are cursed by an ancient entity
2. Marcus and other fishers attempted a ritual to control it
3. The ritual backfired, corrupting the fish and cursing the waters
4. The entity sleeps beneath, but stirs at night

### Story Acts

#### Act 0: Prologue (Tutorial)
- New fisher arrives at the dock
- Learn basic fishing mechanics
- Meet Old Fisher Tom
- First hints that something is wrong

#### Act 1: The Arrival
- Complete tutorial quests
- Experience first night at sea
- Notice strange behavior in fish
- NPCs mention Marcus, the missing fisher
- **Trigger**: Complete tutorial quests

#### Act 2: The Discovery
- Find first message in bottle (Marcus's log)
- Aberrant fish start appearing
- NPCs share legends of "The Deep One"
- Scientist requests samples
- **Trigger**: Find first message in bottle

#### Act 3: The Investigation
- Discover all 3 ancient altars
- Unlock Underground Cavern location
- Meet Ghost Fisher (at 0 sanity)
- Learn ritual was performed 3 years ago
- Piece together what happened
- **Trigger**: Find all altars OR meet ghost

#### Act 4: The Truth
- Collect all 5 messages in bottles
- Ghost Fisher reveals full story
- Learn about the entity's nature
- Understand the failed ritual
- Three paths become available
- **Trigger**: Find all messages + meet ghost + find all altars

#### Act 5: The Resolution (Endings)
- **Good Ending**: Seal the Entity
  - Perform ritual correctly with 10 relics
  - Removes night horror permanently
  - Waters return to normal
  - Town prospers

- **Dark Ending**: Embrace the Darkness
  - Use ritual to gain entity's power
  - Unlock ultimate dark ability
  - Become like Marcus
  - Waters remain cursed

- **Neutral Ending**: Ignore the Mystery
  - Just keep fishing
  - Status quo maintained
  - Night dangers persist
  - Life goes on

## Quest Types (30+ Total)

### Tutorial Quests (5)
1. **First Catch** - Catch any fish
2. **Market Day** - Sell fish worth $50
3. **Night Watch** - Survive one night
4. **Upgrade Time** - Buy first upgrade
5. **New Waters** - Unlock second location

### Story Quests (10)
1. **The Missing Fisher** - Investigate Marcus
2. **Messages from the Deep** - Find 5 bottles
3. **The Ancient Altars** - Discover 3 altars
4. **Aberrant Specimens** - Collect 5 aberrant fish
5. **The Mystic's Warning** - Survive 3 nights without curse
6. **Echoes of the Past** - Meet ghost fisher
7. **The Scientist's Request** - Bring research samples
8. **The Ritual** - Gather 10 relics
9. **Seal the Deep** - Good ending quest
10. **The Truth Revealed** - Complete all lore entries

### Fetch Quests (10)
- Catch specific fish species
- Retrieve items from locations
- Collect rare specimens
- Deliver items to NPCs

### Side Quests (10)
- Photography challenges
- Aquarium building
- Speed fishing
- Master fisher (catch all 60 species)
- Legendary hunter
- Treasure hunter
- Perfect storm challenge

### Repeatable Quests (5)
- Daily Catch ($500 in sales)
- Night Fishing (5 rare fish)
- Competition (biggest fish)
- Exploration (visit all locations)
- Helping Hand (deliveries)

## NPCs (12 Unique Characters)

### 1. Old Fisher Tom
- **Role**: Tutorial mentor
- **Personality**: Wise, helpful, harboring secrets
- **Availability**: Always
- **Quests**: Tutorial quests, story intro
- **Secret**: Knew Marcus, knows about the entity

### 2. Martha (Shopkeeper)
- **Role**: Vendor, gossip
- **Personality**: Cheerful, friendly
- **Availability**: Day only
- **Quests**: Market quests, daily catch
- **Service**: Shop for supplies

### 3. Madame Esther (Mystic)
- **Role**: Curse cleanser, spiritual guide
- **Personality**: Mysterious, cryptic
- **Availability**: Always
- **Quests**: Story quests, ritual quest
- **Service**: Cleanse curses for money

### 4. Captain Hargrave
- **Role**: Location unlocker
- **Personality**: Hostile, gruff
- **Availability**: Always
- **Quests**: Location licenses, legendary fish
- **Service**: Unlock new fishing areas

### 5. Dr. Cassandra Wells (Scientist)
- **Role**: Researcher, aquarium expert
- **Personality**: Eccentric, obsessed with data
- **Availability**: Day only
- **Quests**: Aberrant research, aquarium
- **Lore**: Studying mutations

### 6. Solomon (Lighthouse Keeper)
- **Role**: Night fishing expert
- **Personality**: Gloomy, watchful
- **Availability**: Night only
- **Quests**: Night fishing challenges
- **Lore**: Keeps the light burning

### 7. The Hermit
- **Role**: Dark abilities teacher
- **Personality**: Mysterious, corrupted
- **Availability**: Always
- **Location**: Fog Swamp
- **Lore**: Embraced the entity

### 8. Lily (Child)
- **Role**: Innocent contrast
- **Personality**: Cheerful, naive
- **Availability**: Day only
- **Quests**: Lost locket (emotional quest)
- **Lore**: Her father was Marcus

### 9. One-Eyed Jack (Drunk Sailor)
- **Role**: Exposition through "tall tales"
- **Personality**: Eccentric, drunk
- **Availability**: Night only
- **Location**: Tavern
- **Lore**: Saw the ritual, tells truth but no one believes

### 10. Marcus (Ghost Fisher)
- **Role**: Key story reveal
- **Personality**: Traumatized, regretful
- **Availability**: Sanity below 10
- **Location**: Underground Cavern
- **Quests**: Ghost encounter quest
- **Lore**: The previous fisher who failed

### 11. Barkeep Roderick
- **Role**: Information broker
- **Personality**: Greedy, knowledgeable
- **Availability**: Always
- **Location**: Tavern
- **Service**: Sells information

### 12. Rusty Pete (Mechanic)
- **Role**: Boat upgrades/repairs
- **Personality**: Friendly, pragmatic
- **Availability**: Day only
- **Service**: Ship modifications

## Environmental Storytelling

### Message in Bottles (5 Total)
Messages from Marcus's journey to madness:

1. **"Day 1"** - Excited about new fishing grounds
2. **"Week 3"** - Found strange altar, fish acting odd
3. **"Month 2"** - Aberrant fish appearing, profitable
4. **"Month 5"** - Attempted ritual with others, something went wrong
5. **"Final Entry"** - Regret, warning others to seal it

### Ancient Altars (3 Total)
1. **Altar of the Deep** - Starter Lake area
2. **Altar of Twilight** - Rocky Coastline
3. **Altar of the Abyss** - Underground Cavern

### Rock Carvings
- Ancient warnings in forgotten languages
- Symbols matching the entity
- Ritual instructions

### Sunken Journals
- Previous fishers' logs (via dredging)
- Town history
- Entity worship records

### Ghost Whispers
- Audio clues at low sanity
- Marcus's voice warning players
- Entity's whispers trying to seduce

## Journal System

### Categories (50+ Entries)

#### Fish (20 entries)
- One entry per common fish species
- Behavior, habitat, value info
- Unlocks when species first caught

#### Locations (13 entries)
- History of each fishing area
- What happened there
- Unlocks when location visited

#### NPCs (12 entries)
- Backstory of each character
- Relationships and secrets
- Unlocks when NPC first met

#### Aberrations (10 entries)
- Aberrant fish mutations
- Scientific analysis
- Entity's influence

#### Lore (20+ entries)
- Ancient entity history
- Ritual instructions
- Previous fisher's fate
- Town legends
- Story progression

#### Quests
- Active quest logs
- Completed quest records
- Objectives tracking

#### Clues
- Discovered environmental clues
- Messages in bottles
- Altar information

## Integration with Other Systems

### Agent 1 (Core)
- Uses EventSystem for all communication
- Implements IInteractable for NPCs
- Uses DataTypes for Fish, Location, etc.

### Agent 4 (Save/Load)
- Quest state saved in SaveData.activeQuests/completedQuests
- Quest progress in SaveData.questProgress
- NPC relationships tracked
- Journal entries saved
- Story progression saved

### Agent 6 (Inventory)
- Quest items tracked in inventory
- Quest rewards added to player
- Item collection objectives

### Agent 7 (Sanity)
- Ghost NPC appears at low sanity
- Ghost whispers at 0 sanity
- Story progression affected by sanity

### Agent 8 (Fish AI)
- Quest fish species
- Aberrant fish tracking
- Legendary fish objectives

### Agent 9 (Progression)
- Quest rewards (money, relics)
- Location unlocks from quests
- Dark abilities from story
- Upgrade unlocks

### Agent 11 (UI)
- Quest log display
- Dialogue boxes
- Journal UI
- NPC interaction prompts
- Objective tracking

### Agent 12 (Audio)
- Dialogue blip sounds
- Ghost whisper audio
- Quest complete jingles

### Agent 14 (Locations)
- Location-specific NPCs
- Environmental clue spawning
- Quest location requirements

## Events Published

### Quest Events
- `QuestStarted` (Quest)
- `QuestCompleted` (Quest)
- `QuestFailed` (Quest)
- `QuestAbandoned` (Quest)
- `QuestObjectiveCompleted` (QuestObjective)
- `QuestLimitReached` (int)
- `QuestPrerequisitesNotMet` (Quest)

### Story Events
- `StoryActChanged` (StoryAct)
- `EndingChosen` (StoryEnding)
- `AllAltarsDiscovered`
- `StoryProgressionChanged` (int)

### NPC Events
- `NPCDialogueStarted` (NPCDialogueData)
- `DialogueEnded`
- `NPCTalkedTo` (string npcID)
- `NPCFirstMet` (string npcID)
- `NPCRelationshipChanged` (object {npcID, relationship})
- `PlayerNearNPC` (string npcID)
- `PlayerLeftNPC` (string npcID)

### Discovery Events
- `ClueDiscovered` (string clueID)
- `MessageInBottleFound` (string messageID)
- `AltarDiscovered` (string altarID)
- `JournalEntryUnlocked` (JournalEntry)
- `UnlockJournalEntry` (string entryID)

### UI Events
- `DisplayDialogue` (DialogueUIData)
- `ShowClueDiscovery` (EnvironmentalClue)
- `ShowNotification` (string message)

## Events Subscribed To

### Gameplay Events (for quest tracking)
- `FishCaught` (Fish)
- `FishSold` (Fish)
- `LocationVisited` (string)
- `LocationUnlocked` (string)
- `ItemCollected` (string)
- `DarkAbilityUsed` (string)
- `NightSurvived`
- `ItemDredged` (string)
- `PhotoTaken` (string)
- `RecipeCooked` (string)

### State Events
- `TimeChanged` (TimeChangedEventData)
- `SanityChanged` (float)
- `GameInitialized`

### Save/Load Events
- `GatheringSaveData` (SaveData)
- `ApplyingSaveData` (SaveData)

## Usage Examples

### Starting a Quest
```csharp
// From NPC interaction
QuestManager.Instance.StartQuest("tutorial_first_catch");

// Quest auto-tracks objectives through events
EventSystem.Publish("FishCaught", caughtFish);

// Quest auto-completes when objectives done
```

### Unlocking Journal Entry
```csharp
// Manual unlock
JournalSystem.Instance.UnlockEntry("lore_entity_origin");

// Auto-unlock on fish caught
EventSystem.Publish("FishCaught", bass); // Unlocks "fish_bass" entry

// Auto-unlock on NPC met
EventSystem.Publish("NPCFirstMet", "old_fisher"); // Unlocks "npc_old_fisher" entry
```

### Discovering Environmental Clue
```csharp
// Player interacts with message bottle
// ClueInteraction component calls:
EnvironmentalClues.Instance.DiscoverClue(clueData);

// Automatically:
// - Unlocks journal entries
// - Grants rewards
// - Publishes events for quest tracking
// - Shows UI notification
```

### Progressing Story
```csharp
// Story progresses automatically based on discoveries
// When player finds first message:
EventSystem.Publish("MessageInBottleFound", "message_1");
// -> StoryProgression advances to Act 2

// When player finds all altars:
// -> StoryProgression advances to Act 3

// Manual advancement:
StoryProgression.Instance.AdvanceToAct(StoryAct.Act4_Truth);
```

### Choosing Ending
```csharp
// Good ending
StoryProgression.Instance.ChooseSealEntityEnding();

// Dark ending
StoryProgression.Instance.ChooseEmbraceDarknessEnding();

// Neutral ending
StoryProgression.Instance.ChooseIgnoreMysteryEnding();
```

## Configuration

### Quest Manager Settings
- **Max Active Quests**: 5 (player can only have 5 active at once)
- **Auto-Complete**: Some quests auto-complete when objectives done
- **Hidden Quests**: Some quests only appear through discovery

### NPC Availability
- **Time-Based**: Some NPCs only available day/night
- **Sanity-Based**: Ghost only appears at low sanity
- **Story-Based**: Some NPCs disappear after story acts

### Environmental Clue Spawning
- **Location-Based**: Clues spawn in specific locations
- **Random Spawning**: Some clues have random positions
- **Spawn Chance**: Random clues have probability (0-1)
- **One-Time**: Clues despawn after discovery

## Narrative Tone Guidelines

### Day
- Cozy, friendly atmosphere
- Hints of unease in NPC dialogue
- Environmental clues subtle
- Focus on fishing and progress

### Night
- Ominous, cryptic dialogue
- Building dread through audio
- Ghost whispers at low sanity
- Environmental storytelling intensifies

### Horror Style
- Atmospheric, not jump scares
- Slow-burn revelation
- Environmental storytelling > exposition
- Player discovers truth organically
- Lovecraftian cosmic horror theme

### NPC Voice
Each NPC should have distinct:
- Vocabulary level
- Speech patterns
- Topics they discuss
- Reactions to player
- Secrets they hold

## Testing

### Quest System
1. Start multiple quests simultaneously
2. Complete objectives in various orders
3. Fail quests and restart them
4. Test repeatable quest reset
5. Verify prerequisite blocking
6. Test quest limit (max 5 active)

### NPC System
1. Interact with all 12 NPCs
2. Test time-based availability
3. Test sanity-based appearance (ghost)
4. Verify quest giving
5. Test relationship changes
6. Check shop/mystic services

### Story Progression
1. Play through all 5 acts
2. Test each ending path
3. Verify clue discovery triggers
4. Check journal unlocks
5. Test save/load story state

### Environmental Clues
1. Find all messages in bottles
2. Discover all altars
3. Test random clue spawning
4. Verify location-based clues
5. Test ghost whisper triggering

## Performance Considerations

- Quest objectives update only when relevant events fire (not every frame)
- Environmental clues use object pooling
- Journal entries load on-demand
- Dialogue trees validate once at startup
- NPC visibility checks cached

## Future Enhancements

### Possible Additions
- Branching dialogue choices affect story
- NPC relationship system deeper integration
- More environmental clue types
- Seasonal story events
- Multiple language support
- Voice acting integration
- Animated NPC portraits
- Quest markers on map
- Achievement system integration

### DLC Story Content
- New story act (Act 6)
- Additional NPCs
- Side story mysteries
- New endings
- Expanded lore

## Troubleshooting

**Quest not starting**: Check prerequisites, story act requirement, active quest limit

**NPC not appearing**: Verify time of day, location, sanity level, story progression

**Objectives not tracking**: Ensure correct event is published with proper data

**Journal entry not unlocking**: Check event subscription, entry ID spelling

**Story not progressing**: Verify all trigger conditions met (messages, altars, ghost)

**Save/load issues**: Check SaveData quest fields populated correctly

## Credits

**Narrative Design**: Agent 10
**Integration**: Core architecture (Agent 1)
**Dependencies**: Save System (Agent 4), Inventory (Agent 6), Fish AI (Agent 8), Progression (Agent 9)

---

**"The waters hold secrets. Will you discover them, or will they discover you?"**

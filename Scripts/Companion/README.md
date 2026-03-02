# Agent 17: Crew & Companion Specialist

## Overview

The Crew & Companion Specialist system implements a comprehensive pet companion system (inspired by Cast n Chill's dog companion) and crew management mechanics for Bahnfish. This system adds strategic depth through pets with loyalty mechanics and crew members with morale management.

## Table of Contents

- [Features](#features)
- [Core Systems](#core-systems)
- [Pet Companion System](#pet-companion-system)
- [Crew Management System](#crew-management-system)
- [Integration Guide](#integration-guide)
- [Pet Specifications](#pet-specifications)
- [Crew Specifications](#crew-specifications)
- [Events Published](#events-published)
- [Example Usage](#example-usage)

---

## Features

### Pet Companions
- **6 unique pet types** with distinct abilities and personalities
- **Petting mechanic** (THE KEY FEATURE from Cast n Chill!)
- **Loyalty system** (0-100%) with progression and decay
- **Following AI** with smooth pathfinding
- **Passive + Active abilities** with cooldowns
- **Feeding and care** mechanics
- **Playing interactions** for loyalty boosts

### Crew Members
- **12 unique crew members** across 5 specializations
- **Morale system** (0-100%) affecting performance
- **Daily salary payments** and bonus system
- **Skill bonuses** based on morale levels
- **Crew synergies** and conflicts
- **Station assignments** (Fishing Deck, Helm, Engine Room, Galley, Deck Patrol)

---

## Core Systems

### 1. CompanionManager
**Location**: `Scripts/Companion/CompanionManager.cs`

Central manager coordinating all companion systems.

**Key Features**:
- Pet unlocking and switching
- Active companion tracking
- Integration with all subsystems
- Save/load management

### 2. LoyaltySystem
**Location**: `Scripts/Companion/LoyaltySystem.cs`

Manages pet loyalty progression, decay, and interactions.

**Key Features**:
- Petting cooldown management
- Feeding tracking
- Play session tracking
- Daily loyalty decay
- Low loyalty warnings

### 3. MoraleSystem
**Location**: `Scripts/Companion/MoraleSystem.cs`

Manages crew morale, payments, and relationships.

**Key Features**:
- Daily salary tracking
- Automatic payment system
- Morale decay and restoration
- Crew compatibility checking
- Quit warnings

### 4. CompanionAbilitySystem
**Location**: `Scripts/Companion/CompanionAbilitySystem.cs`

Manages pet and crew abilities, buffs, and cooldowns.

**Key Features**:
- Passive ability application
- Active ability activation
- Buff tracking and expiration
- Crew skill multipliers

### 5. CrewManager
**Location**: `Scripts/Companion/CrewManager.cs`

Manages crew hiring, firing, and assignments.

**Key Features**:
- Crew hiring and firing
- Salary management
- Station assignments
- Crew capacity upgrades

### 6. PetCompanion
**Location**: `Scripts/Companion/PetCompanion.cs`

Individual pet AI and behavior script.

**Key Features**:
- Following behavior
- Idle animations
- Petting interaction
- Feeding and playing
- Ability activation

---

## Pet Companion System

### The Petting Mechanic (Cast n Chill Inspiration!)

The petting interaction is the heart of the companion system:

```csharp
// Player presses E near pet
CompanionManager.Instance.PetActivePet();

// Triggers petting animation
// Plays pet-specific sound
// Spawns heart particles
// Increases loyalty (+3 to +10 based on pet type)
// Provides sanity boost if player sanity < 50%
// Applies 30-second cooldown
```

**What happens during petting:**
1. Pet stops moving and plays petting animation
2. Pet-specific sound effect plays
3. Heart particles spawn
4. Loyalty increases based on pet's affection level
5. Player receives notification
6. Sanity boost if player needs it
7. Cooldown prevents spam

### Pet Types

#### 1. Dog (Starter Pet - FREE)
- **Passive Ability**: Bark Warning (-20% hazard damage)
- **Active Ability**: Fetch (retrieve dropped items, 5-min cooldown)
- **Personality**: Loyal, energetic, friendly
- **Loyalty per Pet**: +5
- **Special**: Can fetch thrown items, retrieves lost fishing lures

#### 2. Cat ($1,000 from Marina)
- **Passive Ability**: Night Eyes (+30% night vision, -15% sanity drain)
- **Active Ability**: Stealth (hide from one hazard, 10-min cooldown)
- **Personality**: Independent, mysterious, calm
- **Loyalty per Pet**: +3 (prefers space)
- **Special**: Detects aberrant fish before they appear

#### 3. Seabird (Quest reward from Lighthouse Keeper)
- **Passive Ability**: Fish Spotter (+25% rare fish spawn rate)
- **Active Ability**: Scout (reveal nearby fish schools, 8-min cooldown)
- **Personality**: Curious, helpful, noisy
- **Loyalty per Pet**: +4
- **Special**: Flies ahead and marks fish schools on minimap

#### 4. Otter (Rare find in Coral Reef)
- **Passive Ability**: Dive Buddy (+10% catch rate, retrieves lost fish)
- **Active Ability**: Treasure Dive (find 1 relic, 24-hour cooldown)
- **Personality**: Playful, clever, mischievous
- **Loyalty per Pet**: +6
- **Special**: Can dive for relics (1 relic/day passive income)

#### 5. Hermit Crab (Catch rare hermit crab variant)
- **Passive Ability**: Shell Guard (+15% inventory space, protects 1 fish from thieves)
- **Active Ability**: Shell Shield (protect all fish from next theft, 15-min cooldown)
- **Personality**: Shy, protective, slow
- **Loyalty per Pet**: +4
- **Special**: Can carry extra item in shell

#### 6. Ghost Companion (Complete dark ending)
- **Passive Ability**: Void Sense (reveals secret areas, +50% aberrant detection)
- **Active Ability**: Ethereal Phase (immune to hazards for 30s, 20-min cooldown)
- **Personality**: Silent, eerie, mysterious
- **Loyalty per Pet**: +10 (hard to gain loyalty)
- **Special**: Immune to sanity drain effects

### Loyalty System

**Loyalty Scale**: 0-100%

**How to Increase Loyalty:**
- **Petting**: +3 to +10 (varies by pet, 30s cooldown)
- **Feeding**: +3 (once per 24 hours)
- **Playing**: +10 (2-minute cooldown)

**Loyalty Decay:**
- -1% per day if not petted
- -5% if not fed when hungry

**Loyalty Effects:**

| Loyalty Level | Effect |
|---------------|--------|
| 80-100% (High) | +50% ability effectiveness, pet is very responsive |
| 30-79% (Normal) | Normal ability effectiveness |
| <30% (Low) | -30% ability effectiveness, warning messages |
| 0% (Abandoned) | Pet runs away permanently |

### Feeding Requirements

- Pets need feeding **once per 24 hours** (real-time)
- Can feed with 1 common fish OR $10 to buy pet food
- Overfed pets have no bonus (max 1 feeding/day counts)
- Underfed pets lose loyalty and effectiveness

---

## Crew Management System

### Crew Specializations

#### 1. Fisherman (3 available)
- **Primary Skill**: +20% catch rate, +15% line strength
- **Secondary Skill**: Faster reeling speed
- **Salary**: $150/day
- **Station**: Fishing Deck
- **Example**: Old Salt Pete (Gruff, experienced, 8/10 skill)

#### 2. Navigator (2 available)
- **Primary Skill**: -30% fuel consumption, +20% boat speed
- **Secondary Skill**: Reveals map shortcuts
- **Salary**: $200/day
- **Station**: Helm
- **Example**: Captain Ada (Precise, professional, 9/10 skill)

#### 3. Maintenance Engineer (2 available)
- **Primary Skill**: +25% durability, auto-repair over time
- **Secondary Skill**: Prevents random breakdowns
- **Salary**: $100/day
- **Station**: Engine Room
- **Example**: Rusty Jim (Lazy but brilliant, 7/10 skill)

#### 4. Cook (2 available)
- **Primary Skill**: 2× cooking buff duration, +1 meal per recipe
- **Secondary Skill**: -50% food costs
- **Salary**: $120/day
- **Station**: Galley
- **Example**: Chef Marie (Perfectionist, cheerful, 10/10 skill)

#### 5. Defender (3 available)
- **Primary Skill**: -50% night hazard damage, scares away thieves
- **Secondary Skill**: +20% sanity during horror events
- **Salary**: $250/day
- **Station**: Deck Patrol
- **Example**: Scar the Harpooner (Intimidating, loyal, 8/10 skill)

### Morale System

**Morale Scale**: 0-100%

**Morale Factors:**

| Factor | Effect |
|--------|--------|
| On-time payment | +5% morale |
| Late payment | -10% to -20% morale |
| Bonus payment (+$50) | +15% morale |
| Working in storm | -10% morale |
| Base decay | -2% per day |
| Crew synergy | +5% morale |
| Ship comfort upgrades | +3% per upgrade |

**Morale Effects on Performance:**

| Morale Level | Skill Bonus |
|--------------|-------------|
| >70% (High) | Full bonuses (100%) |
| 40-70% (Medium) | Half bonuses (50%) |
| <40% (Low) | No bonuses, -10% penalty |
| <20% (Very Low) | Crew threatens to quit |
| <10% (Critical) | Crew quits immediately |

### Hiring Process

1. Visit location where crew is available (Marina, Harbor, etc.)
2. Check unlock requirements (level, quest completion)
3. Check compatibility with existing crew
4. Interview crew member (dialogue system)
5. Hire crew member
6. Assign to station
7. Pay daily salary

### Salary Management

**Payment Options:**
- **Auto-pay**: Automatically pays all crew at day completion
- **Manual pay**: Pay individual crew members manually
- **Bonus**: Give extra money to boost morale

**If unable to pay:**
- Morale decreases by 10-20%
- Consecutive late payments increase penalty
- Crew may quit at <10% morale

---

## Integration Guide

### Setting Up in Unity

1. **Create managers in scene:**
```csharp
// Add to GameManager GameObject or create new
CompanionManager
LoyaltySystem
MoraleSystem
CompanionAbilitySystem
CrewManager
```

2. **Create ScriptableObjects:**
```
Assets/Data/Companions/Pets/
  - Dog.asset (PetData)
  - Cat.asset (PetData)
  - Seabird.asset (PetData)
  ...

Assets/Data/Companions/Crew/
  - OldSaltPete.asset (CrewMemberData)
  - CaptainAda.asset (CrewMemberData)
  ...
```

3. **Assign references:**
- Assign all pet data to CompanionManager.allPets
- Assign all crew data to CrewManager.availableCrewMembers

4. **Create pet prefabs:**
- Each pet needs a prefab with PetCompanion component
- Add Rigidbody, Animator, AudioSource
- Assign to PetData.petPrefab

### Code Integration

**Accessing systems:**
```csharp
// Get managers
CompanionManager companionMgr = CompanionManager.Instance;
LoyaltySystem loyaltySystem = LoyaltySystem.Instance;
MoraleSystem moraleSystem = MoraleSystem.Instance;
CrewManager crewManager = CrewManager.Instance;
```

**Player interaction:**
```csharp
// In PlayerController.cs
void Update()
{
    if (Input.GetKeyDown(KeyCode.E))
    {
        // Check if near pet
        if (nearPet)
        {
            CompanionManager.Instance.PetActivePet();
        }
    }

    if (Input.GetKeyDown(KeyCode.F))
    {
        // Activate pet ability
        CompanionManager.Instance.ActivateActivePetAbility();
    }
}
```

### Event Subscriptions

**Subscribe to companion events:**
```csharp
void Start()
{
    // Pet events
    EventSystem.Subscribe<string>("PetPetted", OnPetPetted);
    EventSystem.Subscribe<string>("PetUnlocked", OnPetUnlocked);
    EventSystem.Subscribe<string>("CompanionSwitched", OnCompanionSwitched);
    EventSystem.Subscribe<LoyaltyChangedEventData>("LoyaltyChanged", OnLoyaltyChanged);

    // Crew events
    EventSystem.Subscribe<string>("CrewHired", OnCrewHired);
    EventSystem.Subscribe<string>("CrewFired", OnCrewFired);
    EventSystem.Subscribe<MoraleChangedEventData>("CrewMoraleChanged", OnMoraleChanged);
    EventSystem.Subscribe<string>("CrewWantsToQuit", OnCrewWantsToQuit);
}
```

### Using Pet Abilities

**Accessing passive bonuses:**
```csharp
// In FishingController.cs
float catchRate = baseCatchRate;

// Apply pet bonus
float fishingMultiplier = CompanionAbilitySystem.Instance.GetSkillMultiplier("Fishing");
catchRate *= fishingMultiplier;
```

**Activating active abilities:**
```csharp
// Activate pet ability
bool success = CompanionManager.Instance.ActivateActivePetAbility();

if (success)
{
    Debug.Log("Pet ability activated!");
}
```

### Using Crew Bonuses

**Accessing crew skill bonuses:**
```csharp
// In BoatController.cs
float fuelConsumption = baseFuelConsumption;

// Apply navigator bonus
float navBonus = CrewManager.Instance.GetSpecializationBonus(CrewSpecialization.Navigator);
fuelConsumption *= (1f - (navBonus / 100f));
```

---

## Events Published

### Pet Events
- `PetRegistered` (string petID) - Pet added to loyalty system
- `PetPetted` (string petID) - Pet was petted
- `PetFed` (string petID) - Pet was fed
- `PetPlayed` (string petID) - Played with pet
- `PetUnlocked` (string petID) - New pet unlocked
- `CompanionSwitched` (string petID) - Active pet changed
- `PetAbilityActivated` (AbilityActivatedEventData) - Pet ability used
- `PetLowLoyalty` (string petID) - Pet loyalty below 20%
- `PetRunAway` (string petID) - Pet ran away due to 0% loyalty
- `LoyaltyChanged` (LoyaltyChangedEventData) - Pet loyalty changed

### Crew Events
- `CrewRegistered` (string crewID) - Crew added to morale system
- `CrewHired` (string crewID) - Crew member hired
- `CrewFired` (string crewID) - Crew member fired
- `CrewRemoved` (string crewID) - Crew member removed from systems
- `SalaryPaid` (SalaryPaidEventData) - Salary paid to crew
- `BonusGiven` (SalaryPaidEventData) - Bonus given to crew
- `SalaryPaymentFailed` (string crewID) - Could not afford salary
- `CrewMoraleChanged` (MoraleChangedEventData) - Crew morale changed
- `CrewWantsToQuit` (string crewID) - Crew threatening to quit
- `CrewStationChanged` (CrewStationChangedEventData) - Crew reassigned
- `DailySalaryCheckRequired` (int count) - Time to pay salaries

### Ability Events
- `BuffApplied` (ActiveBuff) - Buff applied
- `BuffRemoved` (ActiveBuff) - Buff removed
- `AbilityCooldownComplete` (string abilityKey) - Ability ready

---

## Example Usage

### Example 1: Pet the Dog

```csharp
// Get active pet
PetData activePet = CompanionManager.Instance.GetActivePetData();

if (activePet != null)
{
    // Check if can pet
    if (LoyaltySystem.Instance.CanPetNow(activePet.petID, activePet))
    {
        // Pet the companion
        bool success = CompanionManager.Instance.PetActivePet();

        if (success)
        {
            Debug.Log($"Petted {activePet.petName}!");
        }
    }
}
```

### Example 2: Hire a Crew Member

```csharp
// Get available crew at marina
List<CrewMemberData> availableCrew = CrewManager.Instance.GetAvailableCrewAtLocation("marina");

if (availableCrew.Count > 0)
{
    CrewMemberData firstCrew = availableCrew[0];

    // Hire crew member
    bool hired = CrewManager.Instance.HireCrewMember(firstCrew);

    if (hired)
    {
        Debug.Log($"Hired {firstCrew.crewName} for ${firstCrew.dailySalary}/day");
    }
}
```

### Example 3: Feed Your Pet

```csharp
// Check if pet needs feeding
PetData pet = CompanionManager.Instance.GetActivePetData();

if (pet != null && LoyaltySystem.Instance.NeedsFeeding(pet.petID, pet))
{
    // Feed the pet
    bool fed = CompanionManager.Instance.FeedActivePet();

    if (fed)
    {
        Debug.Log($"{pet.petName} is full and happy!");
    }
}
```

### Example 4: Pay Crew Salaries

```csharp
// Get total salary cost
float totalCost = CrewManager.Instance.GetTotalDailySalary();

// Check if can afford
GameState state = GameManager.Instance.CurrentGameState;

if (state.money >= totalCost)
{
    // Pay all salaries
    int paid = CrewManager.Instance.PayAllSalaries();

    Debug.Log($"Paid {paid} crew members (${totalCost})");
}
else
{
    Debug.LogWarning("Cannot afford crew salaries!");
}
```

### Example 5: Switch Active Pet

```csharp
// Get all owned pets
List<PetData> ownedPets = CompanionManager.Instance.GetOwnedPets();

// Switch to second pet
if (ownedPets.Count > 1)
{
    PetData newPet = ownedPets[1];
    bool switched = CompanionManager.Instance.SwitchActivePet(newPet.petID);

    if (switched)
    {
        Debug.Log($"Now following: {newPet.petName}");
    }
}
```

### Example 6: Get Companion Summary

```csharp
// Get summary of all companions
CompanionSummary summary = CompanionManager.Instance.GetCompanionSummary();

Debug.Log($"=== Companion Summary ===");
Debug.Log($"Active Pet: {summary.activePetID}");
Debug.Log($"Pets Owned: {summary.totalPetsOwned}");
Debug.Log($"Average Pet Loyalty: {summary.averagePetLoyalty:F1}%");
Debug.Log($"Crew Hired: {summary.totalCrewHired}");
Debug.Log($"Average Crew Morale: {summary.averageCrewMorale:F1}%");
Debug.Log($"Daily Salary: ${summary.totalDailySalaryCost:F2}");
```

---

## File Structure

```
Scripts/Companion/
├── CompanionManager.cs          (Main manager, 480 lines)
├── LoyaltySystem.cs             (Pet loyalty, 420 lines)
├── MoraleSystem.cs              (Crew morale, 410 lines)
├── CompanionAbilitySystem.cs    (Abilities & buffs, 480 lines)
├── CrewManager.cs               (Crew management, 520 lines)
├── PetCompanion.cs              (Pet AI & behavior, 450 lines)
├── PetData.cs                   (Pet ScriptableObject, 280 lines)
├── CrewMemberData.cs            (Crew ScriptableObject, 240 lines)
└── README.md                    (This file)

Total: ~3,280 lines of production code
```

---

## Success Criteria

✅ **6 unique pet types** with distinct abilities and personalities
✅ **Petting interaction** with animation, loyalty gain, sound effects
✅ **Loyalty system** (0-100%) with decay and progression
✅ **Pet following AI** with smooth pathfinding
✅ **12 unique crew members** across 5 specializations
✅ **Crew morale system** (0-100%) affecting performance
✅ **Salary payment** and management system
✅ **Companion ability system** (passive + active)
✅ **Complete save/load integration** for all systems
✅ **Event-driven architecture** with 20+ events
✅ **100% XML documentation** throughout all files
✅ **Comprehensive README** with examples and integration guide

---

## Tips & Best Practices

1. **Pet the dog regularly!** - Keep loyalty above 80% for maximum ability effectiveness
2. **Pay crew on time** - Late payments severely damage morale
3. **Check crew compatibility** - Some crew don't work well together
4. **Use active abilities strategically** - They have long cooldowns
5. **Feed pets daily** - Prevents loyalty decay
6. **Give bonuses** - Dramatically improves crew morale
7. **Switch pets** - Different situations call for different abilities
8. **Monitor morale** - Crew can quit if morale drops too low

---

## Known Limitations

- Pet AI uses simple following behavior (not NavMesh pathfinding)
- Crew station prefabs need to be created in Unity
- Pet animations must be configured in Animator controllers
- Some crew interactions require dialogue system integration
- Pet sounds need to be assigned to PetData assets

---

## Future Enhancements

- Pet leveling system
- Crew personal quests
- Pet breeding/offspring
- Crew relationship dialogues
- Pet tricks and training
- Crew skill specialization trees
- Pet customization (collars, accessories)
- Crew promotions and ranks

---

**Agent 17: Crew & Companion Specialist - Complete**
Version 1.0 | March 2026
Production-ready code with full documentation

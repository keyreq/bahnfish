# Aquarium & Breeding System

**Agent 16: Aquarium & Breeding Specialist**
**Week 19-20 Deliverable**

## Overview

The Aquarium & Breeding System adds collection and long-term engagement mechanics to Bahnfish. Players can display caught fish in customizable tanks, breed them to create unique genetic variants, and earn passive income through exhibitions.

### Core Features

- **8 Tank Types** across 4 size categories (Small, Medium, Large, Massive)
- **Complete Breeding System** with 10 inheritable genetic traits
- **Mendelian Genetics** with dominant/recessive inheritance
- **Mutation System** with 1-5% chance for unique variants
- **Fish Care Mechanics** (feeding, health, happiness, disease prevention)
- **Exhibition Income** - passive revenue from displayed fish
- **Family Tree Tracking** - full lineage and breeding history
- **Collection Goals** - encyclopedia integration for completionists

---

## System Architecture

### File Structure

```
Scripts/Aquarium/
├── AquariumManager.cs          # Central manager for tank and fish management
├── FishTankData.cs             # ScriptableObject for tank definitions
├── BreedingSystem.cs           # Breeding mechanics and timing
├── GeneticsSystem.cs           # Trait inheritance and mutation logic
├── DisplayController.cs        # Visual fish display and animations
├── DisplayFish.cs              # Fish instance data structure
├── FishCareSystem.cs           # Health, happiness, feeding mechanics
└── README.md                   # This file
```

### Integration Points

The system integrates with existing Phase 1-3 systems:

- **FishDatabase** - Accesses 60 fish species data
- **EconomySystem** - Tank purchases, breeding costs, exhibition income
- **EventSystem** - Event-driven architecture for loose coupling
- **SaveManager** - Complete persistence of tanks, fish, and genetics
- **TimeManager** - Daily updates for fish care and income

---

## Tank System

### Tank Types

#### Small Tanks ($500, 5 fish capacity)
1. **Starter Tank** (Freshwater)
   - Basic freshwater tank for common fish
   - Perfect for beginners

2. **Desktop Tank** (Decorative)
   - Compact display tank
   - Mixed environment support

#### Medium Tanks ($2,000, 10 fish capacity)
3. **Community Tank** (Mixed species)
   - Can hold multiple species together
   - Improved environment quality

4. **Themed Tank** (Specific biome)
   - Optimized for specific fish types
   - Better happiness bonuses

#### Large Tanks ($5,000, 20 fish capacity)
5. **Exhibition Tank** (Rare species showcase)
   - Displays rare fish for maximum income
   - High environment quality

6. **Breeding Tank** (Genetics optimized)
   - Built-in breeding chamber
   - Faster breeding cycles

#### Massive Tanks ($15,000, 50 fish capacity)
7. **Museum Display** (All species collection)
   - Massive capacity for complete collections
   - Premium exhibition income

8. **Aberrant Containment** (Cursed fish)
   - Supports aberrant fish display
   - Special containment effects

### Tank Upgrades

Each tank can be upgraded in 5 categories:

1. **Capacity** (4 levels, +5 fish per level)
   - Cost: $100, $250, $500, $1,000

2. **Auto-Feeder** (3 levels)
   - Automatically feeds fish daily
   - Cost: $150, $300, $600

3. **Filtration** (3 levels, +7.5% environment quality per level)
   - Improves fish happiness
   - Cost: $250, $500, $1,000

4. **Lighting** (4 levels)
   - Unlocks rare species display
   - Increases exhibition value
   - Cost: $300, $600, $1,200, $2,400

5. **Breeding Chamber** (3 levels)
   - Faster breeding cycles
   - Cost: $500, $1,000, $2,000

6. **Genetics Lab** (3 levels, +0.5% mutation chance per level)
   - Increases mutation probability
   - Cost: $800, $1,600, $3,200

---

## Breeding Mechanics

### Requirements

To breed two fish, the following conditions must be met:

- **Same Species** - Fish must be the same species (or compatible variants)
- **Maturity** - Both fish must be mature (3-7 days in-game, modified by growth rate)
- **Happiness** - Both fish must be >60% happy
- **Environment** - Correct tank type for the species
- **No Cooldown** - 24-hour cooldown between breeding attempts per fish
- **Not Already Breeding** - Fish can't be in another breeding pair

### Breeding Process

1. **Select Compatible Fish** - Use `BreedingSystem.ValidateBreeding()` to check compatibility
2. **Pay Breeding Cost** - $50 (common), $100 (rare), $200 (legendary)
3. **Success Roll** - 30-80% chance based on compatibility and happiness
4. **Incubation Period** - 12-48 hours real-time (24h base)
5. **Offspring Birth** - Receives inherited traits from parents
6. **Add to Tank** - Offspring automatically added to breeding tank

### Breeding Success Calculation

```csharp
Base Success = 60%
+ Genetic Compatibility Bonus (0-20%)
+ Happiness Bonus (0-30% based on avg happiness above 60%)
- Rarity Penalty (-10% for Rare, -20% for Legendary)
```

### Compatibility Scoring

```csharp
Base Compatibility = 1.0
- Size Difference Penalty (-0.1 per 10% difference)
- High Aggression Penalty (-0.2 if both >70% aggressive)
+ Same Generation Bonus (+0.1)
- Inbreeding Penalty (-0.5 for siblings, -0.8 for parent-child)
```

---

## Genetics System

### 10 Inheritable Traits

1. **Size Multiplier** (0.5x to 1.5x)
   - Average of parents ± variance
   - Affects fish appearance and swimming speed

2. **Color Variant** (8 colors)
   - Natural, Golden, Albino, Melanistic, Blue, Red, Purple, Rainbow
   - 75% parent color, 20% recessive, 5% mutation

3. **Pattern Type** (3 patterns)
   - Solid, Spots, Stripes
   - Spots and Stripes are dominant over Solid

4. **Rarity Bonus** (0% to 15%)
   - Increases chance of rarity upgrade in offspring
   - Additive inheritance with potential improvement

5. **Value Modifier** (0.7x to 1.6x)
   - Affects sell price
   - Average with variance, mutations favor higher values

6. **Aggression** (0 to 1)
   - Affects breeding success
   - Average of parents with variance

7. **Growth Rate** (0.5x to 2x)
   - Time to maturity
   - Faster growth is dominant trait

8. **Lifespan** (20 to 180 days)
   - In-game days until death from old age
   - Average with variance, mutations favor longevity

9. **Bioluminescence** (boolean)
   - Recessive trait - both parents must have for high chance
   - 90% if both parents, 25% if one parent, 1% mutation

10. **Mutation Chance** (1% to 5%)
    - Each generation has slight improvement
    - Genetics Lab increases by +0.5% per level

### Mutation System

**Base Mutation Rate:** 1%
**Modified By:** Genetics Lab level (+0.5% per level)
**Max Mutation Rate:** 5%

Mutations can result in:
- Random color variants (including rare Rainbow)
- Extreme size variations
- Improved value modifiers
- Bioluminescence activation
- Enhanced traits

### Aberrant Breeding

Special rules for aberrant (cursed) fish:

- **Aberrant + Aberrant** = 80% chance aberrant offspring
- **Aberrant + Normal** = 10% chance aberrant offspring
- **Normal + Normal** = Cannot produce aberrant (unless mutation)

---

## Fish Care System

### Daily Care Requirements

1. **Feeding** ($1 per fish)
   - Must be fed daily
   - Unfed fish lose happiness and health
   - Auto-feeder upgrade available

2. **Maintenance** (varies by tank)
   - Automatic daily cost
   - Tank cleaning improves happiness

3. **Health Monitoring**
   - Fish can become diseased (2% daily chance if unhealthy)
   - Treatment costs $25 per fish
   - Auto-treatment available

### Happiness System

Happiness affects breeding success and exhibition value.

**Happiness Modifiers:**
- Fed Today: +5%
- Good Environment Quality: +0-20%
- Appropriate Tank Type: +5%
- Overpopulation: -15%
- Poor Health: -10%
- Not Fed: -10%

### Health & Disease

**Health Decay:**
- Not fed: -5% per day
- Old age (>80% lifespan): -2% per day
- Diseased: -20% initially, ongoing damage

**Disease Chance:**
- Base: 2% per day
- 2x if health <50%
- 1.5x if happiness <30%

**Death Causes:**
- Old age (lifespan exceeded)
- Health reaches 0%
- Extreme neglect (happiness <10%, 5% daily chance)

---

## Exhibition & Income

### Revenue Calculation

Daily income per fish:
```
Base Income = Tank Base Income Per Fish
× Rarity Multiplier
× Happiness Factor (50-100%)
```

**Rarity Multipliers:**
- Common: 1x ($2/day)
- Uncommon: 2x ($4/day)
- Rare: 5x ($10/day)
- Legendary: 15x ($30/day)
- Aberrant: 10x ($20/day)

### Passive Income Potential

**Example Setup:**
- 1 Large Exhibition Tank (20 capacity)
- 10 Rare fish + 10 Legendary fish
- 80% average happiness

**Daily Income:**
- Rare: 10 × $10 × 0.8 = $80
- Legendary: 10 × $30 × 0.8 = $240
- **Total: $320/day**

**Monthly (30 days): $9,600**

### Costs

**Daily Costs:**
- Maintenance: $5-20 per tank
- Feeding: $1 per fish (or auto-feeder: $10/tank)
- Occasional treatment: $25 per sick fish

**Net Income (above example):**
- Revenue: $320/day
- Costs: ~$40/day (maintenance + food)
- **Net Profit: $280/day ($8,400/month)**

---

## Collection Goals

### Encyclopedia Integration

The aquarium tracks:
- **Base Species Caught** - All 60 species (wild-caught)
- **Color Variants Bred** - 8 colors × 60 species = 480 variants
- **Pattern Variants** - 3 patterns × 60 species = 180 variants
- **Genetic Lineage** - Full family trees with parent tracking
- **Mutation Discoveries** - Unique trait combinations

### Achievement Milestones

1. **First Catch** - Add first fish to aquarium
2. **Starter Collection** - 10 species displayed
3. **Rare Breeder** - Breed first rare offspring
4. **Mutation Master** - Discover 10 mutations
5. **Color Collector** - Breed all 8 color variants of one species
6. **Full Spectrum** - Breed Rainbow variant
7. **Perfect Specimen** - Breed fish with max stats in all traits
8. **Aberrant Researcher** - Breed 10 aberrant offspring
9. **Complete Collection** - All 60 base species displayed
10. **Ultimate Aquarist** - All 480 color variants discovered

### Breeding Goals

**Short-term:**
- Breed first offspring (beginner)
- Create color variant (intermediate)
- Upgrade rarity tier (advanced)

**Long-term:**
- Complete color variant set for species
- Breed "perfect" fish (all maxed traits)
- Discover all 10 mutation types
- Create legendary offspring from rare parents

---

## Code Examples

### Adding Caught Fish to Aquarium

```csharp
// When fish is caught, offer to add to aquarium
EventSystem.Subscribe<Fish>("FishCaught", (Fish caughtFish) => {
    // Get species data
    FishSpeciesData speciesData = FishDatabase.Instance.GetFishByID(caughtFish.id);

    // Create display fish from caught fish
    DisplayFish displayFish = DisplayFish.FromWildCaught(speciesData, caughtFish);

    // Add to tank (player selects tank in UI)
    bool success = AquariumManager.Instance.AddFishToTank(displayFish, selectedTankID);

    if (success) {
        Debug.Log($"Added {displayFish.speciesName} to aquarium!");
    }
});
```

### Starting a Breeding Pair

```csharp
// Select two fish from tank
DisplayFish parent1 = AquariumManager.Instance.GetDisplayFish(fishID1);
DisplayFish parent2 = AquariumManager.Instance.GetDisplayFish(fishID2);

// Validate breeding
string error = BreedingSystem.Instance.ValidateBreeding(parent1, parent2);
if (!string.IsNullOrEmpty(error)) {
    Debug.LogWarning($"Cannot breed: {error}");
    return;
}

// Get genetics lab level from tank
TankInstance tank = AquariumManager.Instance.GetTank(tankID);
int geneticsLabLevel = tank.GetUpgradeLevel(TankUpgradeType.GeneticsLab);

// Start breeding
bool started = BreedingSystem.Instance.StartBreeding(
    parent1,
    parent2,
    tankID,
    geneticsLabLevel
);

if (started) {
    Debug.Log("Breeding started! Check back in 24 hours.");
}
```

### Generating Offspring with Genetics

```csharp
// When breeding completes, offspring is generated automatically
EventSystem.Subscribe<OffspringBornData>("OffspringBorn", (OffspringBornData data) => {
    DisplayFish offspring = data.offspring;
    GeneticTraits traits = offspring.traits;

    // Log genetic information
    Debug.Log($"Offspring born: {offspring.speciesName}");
    Debug.Log($"Generation: {traits.generation}");
    Debug.Log($"Color: {traits.colorVariant}");
    Debug.Log($"Size: {traits.sizeMultiplier:P0}");
    Debug.Log($"Value Modifier: {traits.valueModifier:P0}");

    // Check for mutations
    if (traits.hasBioluminescence) {
        Debug.Log("MUTATION: Bioluminescence discovered!");
    }

    if (traits.colorVariant == FishColor.Rainbow) {
        Debug.Log("RARE: Rainbow variant!");
    }

    // Offspring is automatically added to tank
});
```

### Calculating Exhibition Revenue

```csharp
// Get total daily income from all tanks
float dailyIncome = AquariumManager.Instance.CalculateTotalDailyIncome();
float dailyCost = AquariumManager.Instance.CalculateTotalMaintenanceCost();
float netIncome = dailyIncome - dailyCost;

Debug.Log($"Daily Exhibition Income: ${dailyIncome:F2}");
Debug.Log($"Daily Maintenance Cost: ${dailyCost:F2}");
Debug.Log($"Net Daily Profit: ${netIncome:F2}");
Debug.Log($"Monthly Projection: ${netIncome * 30:F2}");

// Get income breakdown by tank
List<TankInstance> tanks = AquariumManager.Instance.GetOwnedTanks();
foreach (var tank in tanks) {
    List<DisplayFish> fishList = AquariumManager.Instance.GetTankFish(tank.tankData.tankID);
    float tankIncome = 0f;

    foreach (var fish in fishList) {
        if (fish.isAlive) {
            tankIncome += tank.tankData.GetDailyIncome(fish.rarity, 1);
        }
    }

    Debug.Log($"{tank.tankData.tankName}: ${tankIncome:F2}/day");
}
```

### Upgrading a Tank

```csharp
// Upgrade tank capacity
string tankID = "starter_tank";
bool upgraded = AquariumManager.Instance.UpgradeTankCapacity(tankID);

if (upgraded) {
    TankInstance tank = AquariumManager.Instance.GetTank(tankID);
    int newCapacity = tank.GetMaxCapacity();
    Debug.Log($"Tank capacity upgraded to {newCapacity} fish!");
}

// Upgrade specific feature
bool geneticsUpgraded = AquariumManager.Instance.UpgradeTank(
    tankID,
    TankUpgradeType.GeneticsLab
);

if (geneticsUpgraded) {
    Debug.Log("Genetics Lab upgraded! Mutation chance increased.");
}

// Check upgrade costs before purchasing
TankInstance tank = AquariumManager.Instance.GetTank(tankID);
int currentLevel = tank.GetUpgradeLevel(TankUpgradeType.Filtration);
float upgradeCost = tank.tankData.GetCapacityUpgradeCost(currentLevel + 1);

if (EconomySystem.Instance.CanAffordMoney(upgradeCost)) {
    Debug.Log($"Can afford Filtration upgrade: ${upgradeCost:F2}");
}
```

---

## Event Reference

### Events Published

| Event Name | Data Type | Description |
|------------|-----------|-------------|
| `TankPurchased` | `TankInstance` | New tank purchased |
| `TankUpgraded` | `TankUpgradeData` | Tank upgrade completed |
| `FishAddedToTank` | `FishTankChangeData` | Fish placed in tank |
| `FishRemovedFromTank` | `FishTankChangeData` | Fish removed from tank |
| `BreedingStarted` | `BreedingPair` | Breeding pair created |
| `BreedingFailed` | `BreedingPair` | Breeding attempt failed |
| `OffspringBorn` | `OffspringBornData` | New fish born |
| `MutationDiscovered` | `DisplayFish` | Unique mutation found |
| `FishDied` | `FishDeathData` | Fish died |
| `FishBecameSick` | `DisplayFish` | Fish contracted disease |
| `FishTreated` | `DisplayFish` | Fish cured of disease |
| `ExhibitionIncomeEarned` | `float` | Daily income paid |
| `FishFocused` | `DisplayFish` | Fish selected in display |

### Events Subscribed To

| Event Name | Source System | Purpose |
|------------|---------------|---------|
| `FishCaught` | Fishing System | Offer to add fish to aquarium |
| `TimeChanged` | Time Manager | Daily updates and income |
| `MoneyChanged` | Economy System | Enable/disable purchases |
| `GatheringSaveData` | Save Manager | Save aquarium state |
| `ApplyingSaveData` | Save Manager | Load aquarium state |

---

## Performance Considerations

### Optimization Techniques

1. **LOD System** - DisplayController limits visible fish to 50 maximum
2. **Object Pooling** - Reuse fish display GameObjects when possible
3. **Update Batching** - Daily updates processed once per game day
4. **Lazy Loading** - Tank data loaded from Resources on demand
5. **Dictionary Lookups** - O(1) fish access by ID

### Memory Management

- Display fish only instantiated when tank is viewed
- Inactive tanks don't render fish
- Breeding calculations use lightweight structs
- Genetic traits stored as value types

### Recommended Limits

- **Max Tanks:** 20 (plenty for most players)
- **Max Fish Per Tank:** 50 (only Massive tanks)
- **Max Total Fish:** 500 (across all tanks)
- **Max Visible Fish:** 50 (LOD culling)
- **Max Active Breeding Pairs:** 10 (simultaneous)

---

## Testing Checklist

### Core Functionality

- [ ] Purchase all 8 tank types
- [ ] Upgrade tank capacity to max
- [ ] Upgrade all 5 upgrade types
- [ ] Add fish to tank (wild-caught)
- [ ] Remove fish from tank
- [ ] Feed individual fish
- [ ] Feed entire tank
- [ ] Feed all tanks

### Breeding System

- [ ] Start breeding with compatible fish
- [ ] Fail breeding with incompatible fish
- [ ] Wait for breeding to complete (24h or fast mode)
- [ ] Receive offspring notification
- [ ] Verify offspring has parent traits
- [ ] Breed multiple generations (test lineage)
- [ ] Test breeding cooldown enforcement
- [ ] Test aberrant × aberrant breeding

### Genetics System

- [ ] Verify size inheritance
- [ ] Verify color inheritance (all 8 colors)
- [ ] Verify pattern inheritance
- [ ] Test mutation occurrence (<5%)
- [ ] Breed for bioluminescence
- [ ] Create Rainbow variant
- [ ] Test inbreeding penalties
- [ ] Generate "perfect" specimen

### Care System

- [ ] Fish lose happiness when unfed
- [ ] Fish become sick when neglected
- [ ] Treat sick fish
- [ ] Fish die from old age
- [ ] Fish die from poor health
- [ ] Auto-feeder works correctly
- [ ] Auto-treatment works correctly
- [ ] Clean tank improves happiness

### Economy Integration

- [ ] Tank purchase deducts money
- [ ] Breeding costs money
- [ ] Feeding costs money
- [ ] Treatment costs money
- [ ] Exhibition income earned daily
- [ ] Maintenance costs deducted daily
- [ ] Sold fish add to economy

### Display System

- [ ] Fish appear in tank when added
- [ ] Fish swim with patrol behavior
- [ ] Genetic appearance applied (size, color)
- [ ] Bioluminescence glows
- [ ] Aberrant effects visible
- [ ] LOD system culls distant fish
- [ ] Focus mode works

### Save/Load

- [ ] Owned tanks persist
- [ ] Display fish persist
- [ ] Genetic traits persist
- [ ] Active breeding pairs persist
- [ ] Tank upgrades persist
- [ ] Statistics persist

---

## Future Enhancements

### Potential Additions

1. **Advanced Decorations** - Player-placed tank items
2. **Fish Behaviors** - Schooling, territorial, feeding animations
3. **Cross-Species Breeding** - Create hybrid species
4. **Genetic Engineering** - Direct trait manipulation (late-game)
5. **Aquarium Visitors** - NPC tourists with reactions
6. **Photography Mode** - Capture and share fish photos
7. **Competitive Breeding** - Breed for tournaments
8. **Rare Variants** - Shiny/chrome special appearances
9. **Environmental Effects** - Temperature, pH affecting fish
10. **Underwater Viewing** - First-person tank exploration

### Integration Opportunities

- **Quest System** - "Breed a Golden Tuna" objectives
- **NPC Collectors** - Buy specific genetic variants
- **Dark Abilities** - Cursed breeding for aberrants
- **Festival System** - Fish exhibition competitions
- **Journal System** - Detailed breeding notes
- **Achievement System** - Breeding milestones

---

## Troubleshooting

### Common Issues

**Fish not appearing in tank:**
- Verify DisplayController is active
- Check tank capacity not exceeded
- Ensure fish species compatible with tank type

**Breeding fails immediately:**
- Check validation error message
- Verify both fish are mature (3+ days)
- Check happiness >60%
- Ensure no breeding cooldown active

**No exhibition income:**
- Verify daily update is running (TimeManager integration)
- Check fish are alive and in tanks
- Ensure EconomySystem is active

**Mutations not occurring:**
- Base rate is 1% (rare by design)
- Upgrade Genetics Lab for better chances
- Breed many offspring to see mutations

**Performance issues:**
- Reduce maxVisibleFish in DisplayController
- Limit total fish count across tanks
- Disable debug gizmos in build

---

## Credits

**Agent 16: Aquarium & Breeding Specialist**
Implementation by Claude (Anthropic)
Week 19-20 Deliverable for Bahnfish

**Dependencies:**
- Phase 1: Core Architecture, Save System
- Phase 2: 60 Fish Species, Inventory System
- Phase 3: Progression, Economy, Locations

**Special Thanks:**
- Agent 8: Fish Database & Species Data
- Agent 9: Economy & Progression System
- Agent 5: Save/Load Infrastructure

---

## Contact & Support

For questions, bug reports, or feature requests related to the Aquarium & Breeding System:

1. Check this README for documentation
2. Review code comments (100% XML documented)
3. Test with Debug menu context actions
4. Review EventSystem integration

**Debug Commands:**
- Right-click AquariumManager → Print Aquarium Status
- Right-click BreedingSystem → Force Complete All Breeding
- Right-click FishCareSystem → Print Care Statistics

---

**End of Documentation**

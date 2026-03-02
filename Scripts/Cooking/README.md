# Cooking & Crafting System

**Agent 15 Implementation - Week 17-18**

Complete cooking and crafting system for Bahnfish that adds depth and utility to caught fish through meals, buffs, material extraction, and item crafting.

---

## Table of Contents

1. [System Overview](#system-overview)
2. [Core Components](#core-components)
3. [Recipe System](#recipe-system)
4. [Meal Buff System](#meal-buff-system)
5. [Crafting System](#crafting-system)
6. [Preservation System](#preservation-system)
7. [Integration Guide](#integration-guide)
8. [API Reference](#api-reference)
9. [Creating Recipes](#creating-recipes)
10. [Event System](#event-system)

---

## System Overview

The Cooking & Crafting System provides four interconnected subsystems:

### 1. **Cooking System** (`CookingSystem.cs`)
- Central recipe manager with 30+ recipes across 5 tiers
- Recipe unlocking through quests, fishing achievements, and progression
- Real-time cooking operations with success rates
- Integration with inventory and buff systems

### 2. **Meal Buff System** (`MealBuffSystem.cs`)
- 8 buff types with configurable durations (5-30 minutes)
- No stacking for same buff type (refreshes duration instead)
- Different buff types stack for combined effects
- Save/load support for active buffs

### 3. **Crafting System** (`CraftingSystem.cs`)
- Material extraction from caught fish (12 material types)
- 20+ crafting recipes for bait, tools, and upgrades
- Blueprint discovery system
- Material storage and tracking

### 4. **Preservation System** (`PreservationSystem.cs`)
- 4 preservation methods with different durations
- Fish decay mechanics with quality degradation
- Capacity-limited preservation stations
- Real-time decay tracking

---

## Core Components

### File Structure
```
Scripts/Cooking/
├── RecipeData.cs              # ScriptableObject for recipes
├── MealBuffSystem.cs          # Buff management
├── CookingSystem.cs           # Central cooking manager
├── CraftingSystem.cs          # Material & crafting system
├── PreservationSystem.cs      # Fish preservation & decay
├── CookingStationController.cs # Interactive cooking stations
└── README.md                  # This file
```

### Dependencies
- **InventoryManager**: Fish and item storage
- **EventSystem**: Event-driven communication
- **SaveManager**: Persistence
- **FishDatabase**: Fish species data
- **GameManager**: Game state access

---

## Recipe System

### Recipe Types

#### 1. Meals (Tier 1-5)
Provide temporary buffs when consumed.

**Tier 1 - Basic Meals** ($5-15, 10-15 min buffs)
- Grilled Bass: 1 Common Fish → +10% Fishing Luck (10 min)
- Fish Stew: 2 Common Fish → +15% Line Strength (8 min)
- Fried Catch: 1 Common + Bait → +10% Speed (5 min)
- Simple Soup: 3 Common Fish → +10% Sanity Shield (12 min)
- Fish Jerky: 2 Common Fish → +5% All Stats (6 min)

**Tier 2 - Improved Meals** ($30-60, 15-20 min buffs)
- Seafood Platter: 3 Uncommon Fish → +20% Coin Multiplier (15 min)
- Sailor's Feast: 2 Uncommon + 1 Common → +25% Sanity Shield (12 min)
- Fisherman's Delight: 2 Uncommon Fish → +15% Fishing Luck (18 min)
- Hearty Chowder: 1 Uncommon + 3 Common → +20% Line Strength (15 min)
- Energy Meal: 2 Uncommon + 1 Rare Material → +20% Speed (10 min)
- Night Fisher's Brew: 2 Uncommon + Fish Eyes → +15% Night Vision (20 min)
- Storm Stew: 3 Uncommon Fish → +15% Weather Resistance (15 min)
- XP Boost Meal: 2 Uncommon + Rare Pearl → +20% XP Gain (15 min)

**Tier 3 - Advanced Meals** ($100-180, 20-25 min buffs)
- Ocean Bounty: 2 Rare Fish → +30% Fishing Luck (20 min)
- Deep Sea Delight: 1 Rare + 2 Uncommon → +35% Night Vision (18 min)
- Treasure Hunter's Feast: 2 Rare + Sea Crystal → +40% Coin Multiplier (20 min)
- Master's Meal: 3 Rare Fish → +25% All Stats (22 min)
- Endurance Stew: 2 Rare + Ancient Scale → +35% Line Strength (25 min)
- Speed Sashimi: 1 Rare + 2 Uncommon → +30% Speed (15 min)
- Sanity Restoration: 2 Rare + Rare Pearl → +40% Sanity Shield (20 min)

**Tier 4 - Rare Delicacies** ($300-450, 25-30 min buffs)
- Legendary Sushi: 1 Legendary Fish → +40% All Stats (25 min)
- Aberrant Steak: 1 Aberrant Fish → +50% XP Boost + Side Effects (30 min)
- Ancient Feast: 1 Legendary + 2 Rare → +45% Fishing Luck (30 min)
- Void Delicacy: 1 Aberrant + Void Fragment → +45% Night Vision + Sanity Cost (25 min)
- Eternal Meal: 1 Legendary + Ancient Scale → +50% Weather Resistance (28 min)
- Fortune Feast: 2 Legendary Fish → +60% Coin Multiplier (25 min)

**Tier 5 - Mythic Feasts** ($600+, 40+ min buffs)
- Elder's Banquet: 2 Legendary + 3 Rare → +50% All Stats (40 min)
- Void Cuisine: 3 Aberrant Fish → Extreme Buffs + Dark Side Effects (45 min)
- God's Meal: 3 Legendary + 5 Rare Materials → +60% All Stats (60 min)
- Transcendent Feast: 5 Legendary Fish → +70% All Stats + Special Ability (50 min)

#### 2. Bait (8 Types)
Consumable bait for fishing with enhanced spawn rates.

- **Basic Worm**: 1 Fish Guts → 10 uses
- **Shrimp Bait**: 2 Common Fish → 8 uses, +20% uncommon spawn
- **Chum Bait**: 3 Fish Guts + 1 Fish Bone → 12 uses, +15% rare spawn
- **Specialized Lure**: 1 Uncommon + 2 Fish Teeth → 6 uses, +30% rare spawn
- **Rare Attractor**: 1 Rare Fish + Rare Pearl → 5 uses, +50% epic spawn
- **Deep Sea Lure**: 1 Epic Fish + Sea Crystal → 4 uses, +40% legendary spawn
- **Aberrant Bait**: 1 Aberrant Essence + Void Fragment → 5 uses, +60% aberrant spawn
- **Legendary Lure**: 1 Legendary Fish + Ancient Scale → 3 uses, +100% legendary spawn

#### 3. Tools (6 Types)
Consumable tools for repairs and utilities.

- **Repair Kit**: 3 Fish Bones + 2 Scrap → Repairs 50% line durability
- **Emergency Kit**: 5 Fish Bones + 3 Scrap → Full line repair
- **Fuel Can**: 2 Fish Oil + 1 Scrap → +20 fuel
- **Large Fuel Can**: 5 Fish Oil + 2 Scrap → +50 fuel
- **Relic Detector**: 1 Rare Fish + 3 Relics → Highlights secret areas (30 min)
- **Sonar Beacon**: 1 Epic Fish + Sea Crystal → Reveals fish locations (20 min)

#### 4. Upgrades (6 Types)
Permanent upgrade items.

- **Reinforced Line**: 5 Fish Scales + 1 Uncommon → Permanent +10% tension
- **Sturdy Hook**: 3 Fish Teeth + 2 Fish Bones → Permanent +5% catch rate
- **Engine Tuning Kit**: 3 Fish Oil + 2 Scrap → Permanent +5% boat speed
- **Advanced Engine**: 5 Fish Oil + 5 Scrap + 1 Rare Fish → Permanent +15% speed
- **Storage Expansion**: 10 Fish Scales + 3 Scrap → +2 inventory slots
- **Premium Cooler**: 1 Epic Fish + 5 Rare Pearls → +2 cooler slots

---

## Meal Buff System

### Buff Types

| Buff Type | Effect | Duration Range |
|-----------|--------|----------------|
| **Fishing Luck** | +10-50% rare fish spawn rate | 5-30 min |
| **Line Strength** | +15-40% tension tolerance | 5-30 min |
| **Speed Boost** | +10-30% boat speed | 5-30 min |
| **Sanity Shield** | -20-60% sanity drain rate | 5-30 min |
| **Night Vision** | +20-50% visibility at night | 5-30 min |
| **Coin Multiplier** | +15-50% fish sell value | 5-30 min |
| **XP Boost** | +20-40% progression speed | 5-30 min |
| **Weather Resistance** | Reduce storm penalties | 5-30 min |

### Buff Stacking Rules

- **Same Type**: Does NOT stack - refreshes duration and updates strength if higher
- **Different Types**: DO stack - can have all 8 buffs active simultaneously
- **Maximum Active**: 10 buffs total (configurable)

### Usage Example

```csharp
// Apply a buff from a meal
MealBuff buff = new MealBuff
{
    buffType = BuffType.FishingLuck,
    buffStrength = 30f,  // +30%
    duration = 600f,     // 10 minutes
    description = "Increased rare fish spawn rate"
};

MealBuffSystem.Instance.ApplyBuff(buff, "Grilled Bass");

// Check if player has a buff
bool hasLuck = MealBuffSystem.Instance.HasBuff(BuffType.FishingLuck);

// Get buff strength
float luckBonus = MealBuffSystem.Instance.GetBuffStrength(BuffType.FishingLuck);

// Extend buff duration
MealBuffSystem.Instance.ExtendBuff(BuffType.FishingLuck, 300f); // +5 min
```

---

## Crafting System

### Material Types

| Material | Source | Common Uses |
|----------|--------|-------------|
| **Fish Scales** | All fish (1-3) | Basic crafting, storage upgrades |
| **Fish Bones** | All fish (1-2) | Tools, line reinforcement |
| **Fish Oil** | All fish (1-1) | Fuel, engine upgrades |
| **Fish Guts** | Common+ (1-4) | Basic bait |
| **Fish Teeth** | Uncommon+ (1-3) | Advanced bait, hooks |
| **Fish Eyes** | Rare+ (1-3) | Night vision items |
| **Rare Pearl** | Rare+ (0-3) | Premium crafting |
| **Sea Crystal** | Epic+ (1-2) | High-tier tools |
| **Ancient Scale** | Legendary (1-2) | Legendary crafts |
| **Aberrant Essence** | Aberrant (2-5) | Dark items |
| **Void Fragment** | Aberrant (1-3) | Aberrant bait |
| **Scrap Metal** | Found debris | All tool crafting |

### Material Extraction

Materials are automatically extracted when fish are caught:

```csharp
// Automatically triggered on FishCaught event
Dictionary<string, int> materials = CraftingSystem.Instance.ExtractMaterialsFromFish(fishData);

// Check material counts
int scales = CraftingSystem.Instance.GetMaterialCount("fish_scales");
bool hasEnough = CraftingSystem.Instance.HasMaterial("fish_bones", 5);

// Add materials manually
CraftingSystem.Instance.AddMaterial("fish_oil", 10);
```

### Blueprint Discovery

Blueprints unlock crafting recipes through:

1. **Automatic Discovery** (5% chance after each fish caught)
2. **Quest Rewards**
3. **Fishing Achievements**
4. **Manual Unlock**

```csharp
// Discover a blueprint
CraftingSystem.Instance.DiscoverBlueprint("legendary_lure");

// Get craftable recipes
List<RecipeData> craftable = CraftingSystem.Instance.GetCraftableRecipes();
```

---

## Preservation System

### Preservation Methods

| Method | Duration | Capacity | Unlock Requirement |
|--------|----------|----------|-------------------|
| **None** | 48 hours | N/A | Default |
| **Ice Box** | 7 days | 4 fish | Default |
| **Salting** | 14 days | Unlimited | Quest/Upgrade |
| **Smoking** | 30 days | 6 fish | Quest/Upgrade |
| **Freezing** | Indefinite | 10 fish | Quest/Upgrade |

### Quality Degradation

Fish quality degrades over time, affecting sell value:

- **0-25% decay**: 100% quality (full value)
- **25-50% decay**: 75% quality
- **50-75% decay**: 50% quality (warning)
- **75-100% decay**: 25% quality (near spoiled)
- **100% decay**: Fish removed from inventory

### Usage Example

```csharp
// Preserve a fish
PreservationSystem.Instance.PreserveFish(fishID, PreservationMethod.IceBox);

// Check remaining time
float timeLeft = PreservationSystem.Instance.GetRemainingTime(fishID);

// Get quality multiplier
float quality = PreservationSystem.Instance.GetQualityMultiplier(fishID);
float sellValue = baseFishValue * quality;

// Unlock preservation methods
PreservationSystem.Instance.UnlockMethod(PreservationMethod.Freezing);
```

---

## Integration Guide

### Setup

1. **Add Manager GameObjects** to your scene:
   ```
   - CookingSystem (attach CookingSystem.cs)
   - MealBuffSystem (attach MealBuffSystem.cs)
   - CraftingSystem (attach CraftingSystem.cs)
   - PreservationSystem (attach PreservationSystem.cs)
   ```

2. **Create Recipe Assets**:
   - Right-click in Project → Create → Bahnfish → Cooking → Recipe
   - Place in `Resources/Recipes/` folder

3. **Add Cooking Stations**:
   - Add `CookingStationController.cs` to station GameObjects
   - Configure interaction distance and visual effects

### Event Integration

The system publishes these events:

```csharp
// Cooking Events
"CookingStarted"        → RecipeData
"CookingCompleted"      → CookingResult
"CookingFailed"         → RecipeData
"RecipeUnlocked"        → RecipeData

// Buff Events
"BuffApplied"           → ActiveBuff
"BuffExpired"           → ActiveBuff
"BuffRefreshed"         → ActiveBuff

// Crafting Events
"MaterialAdded"         → MaterialChangeData
"MaterialRemoved"       → MaterialChangeData
"BlueprintDiscovered"   → RecipeData
"ItemCrafted"           → ItemCraftedData

// Preservation Events
"FishPreserved"         → FishPreservationData
"FishDecayed"           → string (fishID)
"FishHalfwayDecayed"    → string (fishID)
"PreservationMethodUnlocked" → PreservationMethod
```

### Subscribing to Events

```csharp
// Subscribe to cooking completion
EventSystem.Subscribe<CookingResult>("CookingCompleted", (result) =>
{
    Debug.Log($"Cooked {result.recipe.recipeName}!");
    // Update UI, play effects, etc.
});

// Subscribe to material extraction
EventSystem.Subscribe<MaterialChangeData>("MaterialAdded", (data) =>
{
    Debug.Log($"Gained {data.amount}x {data.materialID}");
});

// Subscribe to fish decay warnings
EventSystem.Subscribe<string>("FishHalfwayDecayed", (fishID) =>
{
    NotificationManager.Instance.ShowWarning("Fish is halfway decayed!");
});
```

---

## API Reference

### CookingSystem

```csharp
// Start cooking a recipe
bool StartCooking(string recipeID)

// Cancel current cooking
void CancelCooking()

// Check ingredients
bool HasIngredients(RecipeData recipe)

// Recipe management
RecipeData GetRecipe(string recipeID)
List<RecipeData> GetRecipesByType(RecipeType type)
List<RecipeData> GetUnlockedRecipes()
bool UnlockRecipe(string recipeID, bool notify = true)
bool IsRecipeUnlocked(string recipeID)

// Properties
bool IsCooking { get; }
CookingOperation CurrentCooking { get; }
int UnlockedRecipeCount { get; }
```

### MealBuffSystem

```csharp
// Buff management
bool ApplyBuff(MealBuff buff, string sourceName)
bool RemoveBuff(BuffType buffType)
void ClearAllBuffs()

// Buff queries
bool HasBuff(BuffType buffType)
ActiveBuff GetBuff(BuffType buffType)
float GetBuffStrength(BuffType buffType)
float GetBuffRemainingTime(BuffType buffType)
Dictionary<BuffType, float> GetAllBuffStrengths()

// Buff manipulation
bool ExtendBuff(BuffType buffType, float additionalTime)

// Properties
List<ActiveBuff> ActiveBuffs { get; }
int ActiveBuffCount { get; }
```

### CraftingSystem

```csharp
// Material management
void AddMaterial(string materialID, int amount)
bool RemoveMaterial(string materialID, int amount)
int GetMaterialCount(string materialID)
bool HasMaterial(string materialID, int amount)
Dictionary<string, int> Materials { get; }

// Blueprint management
bool DiscoverBlueprint(string recipeID)
void AttemptBlueprintDiscovery()

// Crafting queries
List<RecipeData> GetCraftableRecipes()
MaterialInfo GetMaterialInfo(string materialID)

// Fish processing
Dictionary<string, int> ExtractMaterialsFromFish(FishCaughtEventData fishData)
```

### PreservationSystem

```csharp
// Preservation management
bool PreserveFish(string fishID, PreservationMethod method)
bool IsMethodUnlocked(PreservationMethod method)
void UnlockMethod(PreservationMethod method)

// Decay queries
float GetQualityMultiplier(string fishID)
float GetRemainingTime(string fishID)
PreservationMethod GetPreservationMethod(string fishID)
```

### CookingStationController

```csharp
// Interaction
void OpenCookingMenu()
bool StartCooking(RecipeData recipe)
void CancelCooking()

// Station queries
List<RecipeData> GetAvailableRecipes()
float GetCookingProgress()
float GetRemainingTime()

// Properties
StationType Type { get; }
bool IsActive { get; }
bool IsPlayerNearby { get; }
```

---

## Creating Recipes

### Step 1: Create Recipe Asset

1. Right-click in Project window
2. Create → Bahnfish → Cooking → Recipe
3. Name it descriptively (e.g., "GrilledBass_Recipe")

### Step 2: Configure Recipe

```
Basic Info:
- Recipe ID: grilled_bass
- Recipe Name: Grilled Bass
- Recipe Type: Meal
- Tier: 1
- Description: A simple grilled fish that boosts fishing luck

Ingredients:
- Ingredient 1:
  - Type: Fish
  - Rarity: Common
  - Quantity: 1

Crafting:
- Craft Time: 10 seconds
- Base Cost: $5
- Success Rate: 100%

Output:
- Quantity: 1 (meals are consumed immediately)
- Value: $15

Meal Buffs:
- Buff 1:
  - Type: Fishing Luck
  - Strength: 10%
  - Duration: 600 (10 minutes)

Unlock Conditions:
- Unlocked By Default: Yes
```

### Step 3: Place in Resources

Move recipe to `Assets/Resources/Recipes/` folder for automatic loading.

### Recipe Templates

#### Basic Meal Template
```
Type: Meal, Tier: 1-2
Ingredients: 1-3 Common/Uncommon Fish
Cost: $5-60
Buff: Single type, +10-25%
Duration: 5-15 minutes
```

#### Advanced Meal Template
```
Type: Meal, Tier: 3-4
Ingredients: 2-3 Rare/Epic Fish + Materials
Cost: $100-450
Buff: Multiple types or +30-50% single
Duration: 15-30 minutes
```

#### Bait Template
```
Type: Bait, Tier: 1-3
Ingredients: Fish + Materials
Output: 5-12 uses
Effect: Spawn rate bonus
```

#### Tool Template
```
Type: Tool, Tier: 1-2
Ingredients: Materials + Scrap
Output: 1 use (consumable)
Effect: Repair, fuel, or utility
```

---

## Event System

### Published Events

#### Cooking
```csharp
"CookingStarted"    → RecipeData
"CookingCompleted"  → CookingResult { recipe, success, outputQuantity }
"CookingFailed"     → RecipeData
"CookingCancelled"  → RecipeData
"RecipeUnlocked"    → RecipeData
"MealConsumed"      → RecipeData
"MissingIngredients" → RecipeData
"InsufficientFunds" → RecipeData
```

#### Buffs
```csharp
"BuffApplied"       → ActiveBuff { buffType, buffStrength, remainingTime, ... }
"BuffExpired"       → ActiveBuff
"BuffRefreshed"     → ActiveBuff
"BuffLimitReached"  → MealBuffSystem

// Stat modification events (for other systems to subscribe)
"ModifyFishingLuck"        → float (percentage)
"ModifyLineStrength"       → float
"ModifyBoatSpeed"          → float
"ModifySanityDrain"        → float
"ModifyNightVision"        → float
"ModifySellValue"          → float
"ModifyXPGain"             → float
"ModifyWeatherResistance"  → float
```

#### Crafting
```csharp
"MaterialAdded"      → MaterialChangeData { materialID, amount, newTotal }
"MaterialRemoved"    → MaterialChangeData
"ConsumeMaterial"    → MaterialConsumption { materialID, quantity }
"BlueprintDiscovered" → RecipeData
"ItemCrafted"        → ItemCraftedData { itemID, itemName, quantity, value }
```

#### Preservation
```csharp
"FishPreserved"        → FishPreservationData { fishID, method, preservationTime }
"FishDecayed"          → string (fishID)
"FishHalfwayDecayed"   → string (fishID)
"FishAlmostDecayed"    → string (fishID)
"PreservationMethodUnlocked" → PreservationMethod
```

#### Station Interaction
```csharp
"OpenCookingUI"     → CookingStationController
```

### Subscribing to Events

```csharp
void Start()
{
    // Subscribe to events
    EventSystem.Subscribe<RecipeData>("RecipeUnlocked", OnRecipeUnlocked);
    EventSystem.Subscribe<ActiveBuff>("BuffApplied", OnBuffApplied);
}

void OnDestroy()
{
    // Always unsubscribe
    EventSystem.Unsubscribe<RecipeData>("RecipeUnlocked", OnRecipeUnlocked);
    EventSystem.Unsubscribe<ActiveBuff>("BuffApplied", OnBuffApplied);
}

private void OnRecipeUnlocked(RecipeData recipe)
{
    Debug.Log($"New recipe unlocked: {recipe.recipeName}");
}

private void OnBuffApplied(ActiveBuff buff)
{
    Debug.Log($"Buff applied: {buff.buffType} +{buff.buffStrength}%");
}
```

---

## Save/Load Integration

All systems automatically integrate with `SaveManager`:

### Saved Data Structures

```csharp
// In SaveData.cs
public CookingData cookingData;          // Unlocked recipes
public CraftingData craftingData;        // Materials & blueprints
public PreservationData preservationData; // Decay timers & unlocks
public List<SerializedBuff> activeBuffs; // Active buffs with timers
```

### Manual Save/Load

```csharp
// Systems automatically handle save/load through EventSystem:
// - "GatheringSaveData" event
// - "ApplyingSaveData" event

// No manual intervention required!
```

---

## Performance Considerations

### Update Loops
- **MealBuffSystem**: Updates buff timers every frame (lightweight)
- **PreservationSystem**: Updates decay timers every frame (lightweight)
- **CookingSystem**: Only updates when cooking is active

### Memory
- Recipe data loaded once at startup
- Material dictionary optimized with string keys
- Active buff list typically < 10 items

### Optimization Tips
1. Load recipes from Resources only once
2. Cache recipe lookups in dictionaries
3. Use events instead of polling
4. Limit simultaneous buffs (default: 10)

---

## Troubleshooting

### Recipes Not Loading
- Ensure recipes are in `Resources/Recipes/` folder
- Check recipe IDs are unique
- Verify ScriptableObject asset is not corrupted

### Buffs Not Applying
- Check `MealBuffSystem.Instance` exists in scene
- Verify buff limit not exceeded (default: 10)
- Ensure buff type not already active (same type doesn't stack)

### Materials Not Extracting
- Verify `CraftingSystem.Instance` exists
- Check fish caught event is being published
- Ensure `FishCaughtEventData` structure matches

### Fish Not Decaying
- Confirm `PreservationSystem.Instance` exists
- Check if fish is frozen (indefinite preservation)
- Verify fish was added to inventory correctly

---

## Future Enhancements

Potential additions for future updates:

1. **Recipe Variants**: Seasonal or weather-specific recipe variations
2. **Cooking Minigames**: Interactive cooking challenges
3. **Material Quality**: Common/rare/legendary material variants
4. **Batch Cooking**: Cook multiple recipes simultaneously
5. **Recipe Books**: Collectible recipe books with set bonuses
6. **Fermentation**: Time-based food improvement system
7. **Trading**: Trade recipes and materials with NPCs
8. **Cooking Competitions**: Timed cooking challenges

---

## Credits

**Agent 15: Cooking & Crafting Specialist**
- Implementation: Week 17-18
- Systems: Cooking, Buffs, Crafting, Preservation
- Integration: Inventory, Events, Save System

---

## Version History

**v1.0.0** (Week 17-18)
- Initial implementation
- 30+ recipes across 5 tiers
- 8 buff types with stacking rules
- 20+ crafting recipes
- 4 preservation methods
- Complete save/load integration
- Event-driven architecture
- Interactive cooking stations

---

For questions or support, refer to the main Bahnfish documentation or contact the development team.

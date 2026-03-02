# Agent 14: Location & World Generation System

## Overview

The Location & World Generation system manages all 13 fishing locations in Bahnfish, including location loading, navigation, fast travel, and secret area discovery. Each location has unique biomes, fish pools, environmental characteristics, and special mechanics.

## System Components

### Core Files

#### LocationData.cs (ScriptableObject)
Defines the template for fishing locations with all configurable properties.

**Key Properties**:
- Basic Info: ID, name, description
- Progression: License cost, difficulty, biome type
- Fish Pool: Species IDs, spawn rate modifiers
- Environment: Weather, sanity drain, hazards
- World Position: Dock position, spawn bounds
- Special Features: Secret areas, NPCs, story significance

**Usage**:
```csharp
// Access location data
LocationData location = LocationManager.Instance.GetLocationByID("calm_lake");
Debug.Log($"License Cost: ${location.licenseCost}");
Debug.Log($"Fish Species: {location.fishSpeciesIDs.Count}");
```

#### LocationManager.cs
Singleton that manages all locations, handles loading, and tracks unlocked locations.

**Key Methods**:
- `GetLocationByID(string)` - Get location data
- `GetCurrentLocation()` - Get active location
- `LoadLocation(string)` - Load and activate a location
- `UnlockLocation(string)` - Unlock a location license
- `GetUnlockedLocations()` - Get all unlocked locations
- `GetFishPoolForLocation(string)` - Get fish species for a location

**Events Published**:
- `LocationUnlocked` - Location license purchased
- `LocationLoadingStarted` - Loading transition begins
- `LocationChanged` - New location activated
- `LocationLoadingComplete` - Loading finished

**Integration**:
- Agent 3 (Environment): Applies weather and lighting settings
- Agent 7 (Sanity): Sets location-specific drain modifiers
- Agent 8 (Fish): Updates fish spawn pools
- Agent 9 (Progression): Checks license ownership

**Usage**:
```csharp
// Check if location is unlocked
if (LocationManager.Instance.IsLocationUnlocked("arctic_waters"))
{
    // Load the location
    LocationManager.Instance.LoadLocation("arctic_waters");
}

// Get current location's fish pool
List<string> fishPool = LocationManager.Instance.GetCurrentLocationFishPool();
```

### Navigation System

#### NavigationSystem.cs
Handles travel between locations with fuel consumption and travel time.

**Key Methods**:
- `TravelToLocation(string)` - Start travel to destination
- `CalculateDistance(LocationData, LocationData)` - Get distance
- `CalculateFuelCost(float)` - Calculate fuel needed
- `CalculateTravelTime(float)` - Calculate travel duration
- `GetTravelRequirements(string)` - Get full travel info
- `CancelTravel()` - Abort current travel

**Events Published**:
- `TravelStarted` - Travel begins
- `TravelProgress` - Progress update (0-1)
- `TravelComplete` - Arrival at destination
- `TravelCancelled` - Travel aborted
- `TravelBlocked` - Cannot travel (fishing, no fuel, etc)

**Features**:
- Real-time fuel consumption during travel
- Travel time simulation with progress tracking
- Cannot travel while fishing
- Upgrade system integration for engine speed

**Usage**:
```csharp
// Check travel requirements
TravelRequirements req = NavigationSystem.Instance.GetTravelRequirements("coral_reef");
Debug.Log($"Distance: {req.distance} km, Fuel: {req.fuelCost}, Can Afford: {req.canAfford}");

// Start travel
if (req.canAfford)
{
    NavigationSystem.Instance.TravelToLocation("coral_reef");
}
```

#### FastTravelSystem.cs
Handles instant dock-to-dock teleportation using relics (Tidal Gate ability).

**Key Methods**:
- `FastTravel(string)` - Instant travel to location
- `UnlockFastTravel()` - Unlock fast travel ability
- `IsFastTravelUnlocked()` - Check if unlocked
- `GetRelicCost()` - Get relic cost per travel
- `CanFastTravelTo(string)` - Check if can fast travel

**Features**:
- Instant travel (no fuel cost)
- Requires 1 relic per use (configurable)
- Must be at dock to use
- Unlocked via Tidal Gate altar in Bioluminescent Bay

**Events Published**:
- `FastTravelUnlocked` - Ability unlocked
- `FastTravelStarted` - Teleportation begins
- `FastTravelComplete` - Arrived at destination
- `FastTravelBlocked` - Cannot fast travel

**Usage**:
```csharp
// Unlock fast travel (from progression system)
FastTravelSystem.Instance.UnlockFastTravel();

// Use fast travel
if (FastTravelSystem.Instance.CanFastTravelTo("abyssal_trench"))
{
    FastTravelSystem.Instance.FastTravel("abyssal_trench");
}
```

#### TravelCostCalculator.cs
Calculates fuel costs and efficiency with environmental modifiers.

**Key Methods**:
- `CalculateFuelCost(string)` - Get fuel cost to location
- `CanAffordTravel(string)` - Check if enough fuel
- `GetFuelWarningStatus()` - Get fuel warning level
- `GetMaximumRange()` - Max distance with current fuel
- `GetReachableLocations()` - All locations within range
- `SetEngineEfficiency(float)` - Apply upgrade modifier

**Features**:
- Weather affects fuel consumption (storms = +50% fuel)
- Night travel uses slightly more fuel (+10%)
- Engine upgrades improve efficiency
- Fuel warning system (Normal/Low/Critical)
- Distance caching for performance

**Usage**:
```csharp
// Check fuel status
FuelWarning warning = TravelCostCalculator.Instance.GetFuelWarningStatus();
if (warning.level == FuelWarningLevel.Critical)
{
    Debug.LogWarning("CRITICAL FUEL!");
}

// Get reachable locations
List<LocationData> reachable = TravelCostCalculator.Instance.GetReachableLocations();
Debug.Log($"Can reach {reachable.Count} locations");

// Get detailed breakdown
TravelCostBreakdown breakdown = TravelCostCalculator.Instance.GetTravelCostBreakdown("volcanic_vent");
Debug.Log($"Base: {breakdown.baseFuelCost}, Total: {breakdown.totalFuelCost}");
```

### Secret Areas

#### SecretAreaManager.cs
Manages hidden fishing spots and discovery mechanics.

**Key Methods**:
- `DiscoverSecretByID(string)` - Manually discover secret
- `IsSecretDiscovered(string)` - Check discovery status
- `GetSecretsInLocation(string)` - Get location's secrets
- `GetRareFishBonusAtPosition(Vector3)` - Check for bonus
- `GetLegendaryBonusAtPosition(Vector3)` - Check legendary boost

**Features**:
- Automatic discovery when player enters radius
- Some secrets require Eldritch Eye ability
- Rare fish bonus (2x) in discovered secret areas
- Legendary spawn chance boost (+5%)
- Relic rewards on discovery

**Events Published**:
- `SecretAreaDiscovered` - Secret found

**Usage**:
```csharp
// Check if player is in secret area
float rareFishBonus = SecretAreaManager.Instance.GetRareFishBonusAtPosition(playerPosition);
if (rareFishBonus > 1f)
{
    Debug.Log("Fishing in a secret area! 2x rare fish chance!");
}

// Get discovery stats
var stats = SecretAreaManager.Instance.GetSecretStats();
Debug.Log($"Discovered {stats.discovered} of {stats.total} secrets");
```

### Location Generation

#### LocationGenerator.cs
Editor tool that generates all 13 location ScriptableObject assets.

**Usage**:
```csharp
// In Unity Editor:
// 1. Create empty GameObject
// 2. Add LocationGenerator component
// 3. Right-click component
// 4. Select "Generate All 13 Locations"
// 5. All locations created in Assets/Resources/Locations/
```

**Generated Locations**:
1. Calm Lake - FREE starter (20 common fish)
2. Rocky Coastline - $500 (18 species)
3. Tidal Pools - $1500 (20 small fish + shellfish)
4. Deep Ocean Trenches - $1500 (20 rare deep fish)
5. Fog-Shrouded Swamp - $2000 (15 aberrant focus)
6. Mangrove Forest - $2000 (16 brackish species)
7. Coral Reef - $2500 (25 species - highest diversity)
8. Arctic Waters - $3000 (12 cold-water species)
9. Shipwreck Graveyard - $3500 (15 + dredging focus)
10. Underground Cavern - $4000 (10 unique cave species)
11. Bioluminescent Bay - $4500 (18 glowing species)
12. Volcanic Vent - $5000 (12 heat-adapted + legendaries)
13. Abyssal Trench - $10,000 (8 legendary/aberrant - ENDGAME)

#### All13Locations.cs
Centralized data definitions and reference for all locations.

**Static Data**:
- `LocationIDs` - All location IDs in order
- `ProgressionPath` - Recommended unlock order
- `StoryLocations` - Locations with story significance
- `SecretAreas` - All secret area definitions
- `LocationNPCs` - NPC assignments
- `SpecialMechanics` - Special features per location

**Usage**:
```csharp
// Check if story location
if (All13Locations.IsStoryLocation("underground_cavern"))
{
    Debug.Log("Story location!");
}

// Get progression info
LocationProgression prog = All13Locations.GetProgression("volcanic_vent");
Debug.Log($"Tier: {prog.tier}, Recommended Level: {prog.recommendedLevel}");

// Get secrets
List<SecretAreaDefinition> secrets = All13Locations.GetSecrets("coral_reef");
```

## The 13 Locations

### 1. Calm Lake (Starter - FREE)
- **Difficulty**: Beginner
- **Fish**: 20 common/uncommon freshwater species
- **Special**: No sanity drain, no night hazards (safe tutorial zone)
- **NPCs**: Old Fisher
- **Secret**: Hidden cove with rare trout

### 2. Rocky Coastline ($500)
- **Difficulty**: Easy-Medium
- **Fish**: 18 coastal species
- **Special**: Crab pot hotspot (150% catch rate)
- **NPCs**: Lighthouse Keeper
- **Secret**: Ancient tide pools

### 3. Tidal Pools ($1500)
- **Difficulty**: Easy
- **Fish**: 20 small fish + shellfish
- **Special**: Crab pot catch rate +200%, shallow safe water
- **NPCs**: Shopkeeper
- **Secret**: Pearl oyster bed (full moon only)

### 4. Deep Ocean Trenches ($1500)
- **Difficulty**: Hard
- **Fish**: 20 rare deep-sea species + aberrants
- **Special**: Extreme depth (200m+), harpoon required
- **Secret**: Underwater canyon with legendaries

### 5. Fog-Shrouded Swamp ($2000)
- **Difficulty**: Medium (High at night)
- **Fish**: 15 aberrant-focused species
- **Special**: HIGHEST aberrant spawn (50% at night), permanent fog
- **NPCs**: Mystic
- **Secret**: Ancient altar (story location)

### 6. Mangrove Forest ($2000)
- **Difficulty**: Medium
- **Fish**: 16 brackish + unique tree-dwelling species
- **Special**: Navigation maze, unique climbing fish
- **Secret**: Third altar in deepest grove

### 7. Coral Reef ($2500)
- **Difficulty**: Medium
- **Fish**: 25 tropical species (HIGHEST DIVERSITY)
- **Special**: Beautiful underwater view, research station
- **NPCs**: Scientist
- **Secret**: Coral cave breeding grounds

### 8. Arctic Waters ($3000)
- **Difficulty**: Hard
- **Fish**: 12 cold-water species
- **Special**: Cold damage (1.5x sanity drain), icebergs, aurora
- **NPCs**: Captain
- **Secret**: Frozen shipwreck with relics

### 9. Shipwreck Graveyard ($3500)
- **Difficulty**: Medium-Hard
- **Fish**: 15 species + dredging focus
- **Special**: BEST dredging location, high scrap yield
- **NPCs**: Drunk Sailor
- **Secret**: Captain's treasure (20 relics)

### 10. Underground Cavern ($4000)
- **Difficulty**: Extreme
- **Fish**: 10 unique cave species
- **Special**: PURE HORROR, requires upgraded lights, story climax
- **NPCs**: Hermit
- **Secrets**: Cavern altar, Entity's prison

### 11. Bioluminescent Bay ($4500)
- **Difficulty**: Medium
- **Fish**: 18 glowing species
- **Special**: INVERTED RISK (safer at night), lower sanity drain
- **NPCs**: Child
- **Secret**: Tidal Gate altar (fast travel unlock)

### 12. Volcanic Vent ($5000)
- **Difficulty**: Extreme
- **Fish**: 12 heat-adapted + legendaries
- **Special**: HIGHEST legendary spawn (10%), heat damage zones
- **Secret**: Ancient Leviathan lair

### 13. Abyssal Trench ($10,000 - ENDGAME)
- **Difficulty**: EXTREME
- **Fish**: 8 legendary/aberrant species only
- **Special**: Final location, extreme depth (500m), 2x sanity drain
- **Secret**: Entity's awakening site (true ending)

## Fish Distribution Strategy

### Common Fish (20 species)
- Appear in **6-8 locations**
- Focus: Calm Lake, Rocky Coastline, Tidal Pools
- Examples: Bass, Bluegill, Crappie, Catfish

### Uncommon Fish (15 species)
- Appear in **3-5 locations**
- Focus: Coastal, Freshwater, Tropical
- Examples: Striped Bass, Red Drum, Snook

### Rare Fish (10 species)
- Appear in **1-3 locations**
- Focus: Deep Ocean, Volcanic, Arctic
- Examples: Blue Marlin, Swordfish, Giant Trevally

### Legendary Fish (5 species)
- Appear in **1-2 specific locations**
- Most spawn in: Volcanic Vent, Abyssal Trench
- Examples: Void Emperor, Ancient Leviathan, Celestial Dragon

### Aberrant Fish (10 species)
- Appear in **3-6 locations** (night-only)
- Highest rates: Swamp (50%), Cavern (40%), Trench (50%)
- Examples: Aberrant Bass, Phantom Trout, Corrupted Gar

## Location Progression Path

```
START: Calm Lake (FREE)
  ↓
Early Game: Rocky Coast ($500) or Tidal Pools ($1500)
  ↓
Mid Game: Coral Reef ($2500), Mangrove ($2000), Swamp ($2000)
  ↓
Advanced: Arctic ($3000), Ocean Trench ($1500), Shipwreck ($3500), Bio Bay ($4500)
  ↓
Late Game: Underground Cavern ($4000), Volcanic Vent ($5000)
  ↓
ENDGAME: Abyssal Trench ($10,000)
```

## Integration with Other Agents

### Agent 3 (Environment)
- `WeatherSystem.SetAllowedWeather(list)` - Apply location weather
- `TimeManager` - Affects spawn rates and sanity drain

### Agent 7 (Sanity/Horror)
- `SanityManager.SetSanityDrainModifier(float)` - Location-specific drain
- `HazardSpawner.SetSpawnRateModifier(float)` - Location danger level

### Agent 8 (Fish AI)
- `FishSpawner.SetLocationFishPool(list)` - Update spawnable fish
- `FishSpawner.SetSpawnRateModifiers(...)` - Rare/legendary/aberrant rates

### Agent 9 (Progression)
- Check license ownership before allowing travel
- Unlock locations when license purchased
- Fast travel requires Tidal Gate ability

### Agent 10 (Quest/Narrative)
- Story locations trigger narrative events
- NPCs at specific locations give quests
- Secret areas contain quest objectives

### Agent 11 (UI)
- Display location info, fish counts, weather
- Show travel requirements and fuel costs
- Map interface with location markers

## Events Reference

### LocationManager Events
| Event | Data Type | Description |
|-------|-----------|-------------|
| `LocationUnlocked` | `string` | Location ID unlocked |
| `LocationLoadingStarted` | `string` | Loading begins |
| `LocationChanged` | `LocationData` | New location active |
| `LocationLoadingComplete` | `string` | Loading finished |

### NavigationSystem Events
| Event | Data Type | Description |
|-------|-----------|-------------|
| `TravelStarted` | `TravelStartedEventData` | Travel begins |
| `TravelProgress` | `float` | Progress (0-1) |
| `TravelComplete` | `string` | Arrived at destination |
| `TravelCancelled` | `string` | Travel aborted |
| `TravelBlocked` | `string` | Cannot travel (reason) |

### FastTravelSystem Events
| Event | Data Type | Description |
|-------|-----------|-------------|
| `FastTravelUnlocked` | `bool` | Ability unlocked |
| `FastTravelStarted` | `string` | Teleport begins |
| `FastTravelComplete` | `string` | Arrived |
| `FastTravelBlocked` | `string` | Cannot fast travel |

### SecretAreaManager Events
| Event | Data Type | Description |
|-------|-----------|-------------|
| `SecretAreaDiscovered` | `SecretAreaData` | Secret found |

## Save/Load Integration

All location systems integrate with the save system:

**LocationManager**:
- Current location ID
- Unlocked location IDs

**FastTravelSystem**:
- Fast travel unlock status

**SecretAreaManager**:
- Discovered secret IDs

**Usage**:
```csharp
// Subscribe to save/load events
EventSystem.Subscribe<SaveData>("GatheringSaveData", OnGatheringSave);
EventSystem.Subscribe<SaveData>("ApplyingSaveData", OnApplyingSave);

private void OnGatheringSave(SaveData data)
{
    // Add your location data to save
    data.locationData = JsonUtility.ToJson(myLocationData);
}

private void OnApplyingSave(SaveData data)
{
    // Restore from save
    myLocationData = JsonUtility.FromJson<LocationData>(data.locationData);
}
```

## Debug Commands

### LocationManager
- `Print Location Stats` - Show current location info
- `Unlock All Locations (Debug)` - Unlock everything
- `Reset Unlocked Locations` - Reset to starter only

### NavigationSystem
- `Simulate Quick Travel` - Instant travel for testing

### FastTravelSystem
- `Unlock Fast Travel (Debug)` - Unlock ability
- `Test Fast Travel` - Quick test travel

### TravelCostCalculator
- `Print Fuel Status` - Show fuel and range
- `Print Reachable Locations` - Show available destinations

### SecretAreaManager
- `Print Secret Stats` - Show discovery progress
- `Discover All Secrets (Debug)` - Reveal all
- `Reset All Secrets` - Clear discoveries

## Future Enhancements

### Phase 5 (Visual Assets)
- 3D location scenes with unique visuals
- Environmental props (icebergs, shipwrecks, etc)
- Weather particle effects per location
- Underwater rendering for split-screen

### Potential Features
- Dynamic weather affects available locations
- Seasonal location variants
- Location events (tournaments, migrations)
- Location-specific achievements
- Photo mode locations

## Performance Considerations

- Location distance calculations are cached
- Scene loading uses coroutines with delays
- Fish pool queries use dictionaries for O(1) lookup
- Secret area checks only on player movement
- All systems use singleton pattern (single instance)

## Common Patterns

### Checking if Player Can Access Location
```csharp
string targetID = "arctic_waters";

// Check unlocked
if (!LocationManager.Instance.IsLocationUnlocked(targetID))
{
    Debug.Log("Location is locked!");
    return;
}

// Check fuel
if (!TravelCostCalculator.Instance.CanAffordTravel(targetID))
{
    Debug.Log("Not enough fuel!");
    return;
}

// Check if fishing
if (FishingController.Instance.IsFishing())
{
    Debug.Log("Cannot travel while fishing!");
    return;
}

// All checks passed
NavigationSystem.Instance.TravelToLocation(targetID);
```

### Getting Location-Specific Bonuses
```csharp
Vector3 playerPos = player.transform.position;

// Check for secret area bonus
float rareFishBonus = SecretAreaManager.Instance.GetRareFishBonusAtPosition(playerPos);
float legendaryBonus = SecretAreaManager.Instance.GetLegendaryBonusAtPosition(playerPos);

// Apply to spawn rates
float finalRareChance = baseRareChance * rareFishBonus;
float finalLegendaryChance = baseLegendaryChance + legendaryBonus;
```

## Troubleshooting

**Problem**: "Location ID not found"
- **Solution**: Ensure LocationGenerator has created all locations in Resources/Locations

**Problem**: "Cannot travel - location is locked"
- **Solution**: Purchase license through ProgressionManager (Agent 9)

**Problem**: "Not enough fuel"
- **Solution**: Return to dock and purchase fuel, or use Fast Travel

**Problem**: "Fast travel not working"
- **Solution**: Ensure Tidal Gate ability is unlocked and player is at dock

**Problem**: "Secrets not discovering"
- **Solution**: Check discovery radius, ensure Eldritch Eye is unlocked for hidden secrets

---

**Agent 14 Status**: ✅ COMPLETE

All 13 locations implemented with full fish distributions, navigation systems, fast travel, and secret areas. Ready for integration with other agents and visual asset implementation in Phase 5.

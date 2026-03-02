# Agent 14: Location & World Generation - System Architecture

## Component Diagram

```
┌─────────────────────────────────────────────────────────────────────┐
│                         WORLD GENERATION SYSTEM                      │
│                            (Agent 14)                                │
└─────────────────────────────────────────────────────────────────────┘

┌──────────────────────┐
│   LocationData.cs    │ ◄─────── ScriptableObject Template
│  (ScriptableObject)  │          (13 instances created)
└──────────────────────┘
         │
         ▼
┌──────────────────────────────────────────────────────────────────────┐
│                      LocationManager.cs                              │
│                         (Singleton)                                  │
│  ┌────────────────────────────────────────────────────────────────┐ │
│  │ • Manages all 13 locations                                     │ │
│  │ • Tracks unlocked locations                                    │ │
│  │ • Loads/unloads locations                                      │ │
│  │ • Publishes location change events                             │ │
│  └────────────────────────────────────────────────────────────────┘ │
└──────────────────────────────────────────────────────────────────────┘
         │
         ├────────────────────────────────────────────────────────────┐
         │                                                             │
         ▼                                                             ▼
┌─────────────────────┐                                  ┌──────────────────────┐
│  NavigationSystem   │                                  │  SecretAreaManager   │
│    (Singleton)      │                                  │     (Singleton)      │
│ ┌─────────────────┐ │                                  │ ┌──────────────────┐ │
│ │ • Travel system │ │                                  │ │ • Secret spots   │ │
│ │ • Fuel consume  │ │                                  │ │ • Discovery      │ │
│ │ • Time calc     │ │                                  │ │ • Rare fish bonus│ │
│ └─────────────────┘ │                                  │ └──────────────────┘ │
└─────────────────────┘                                  └──────────────────────┘
         │                                                             │
         ├──────────────────┬──────────────────┐                      │
         ▼                  ▼                  ▼                      ▼
┌───────────────┐  ┌──────────────┐  ┌──────────────────┐  ┌──────────────────┐
│FastTravelSys  │  │TravelCost    │  │NavigationUI      │  │SecretAreaData    │
│  (Singleton)  │  │Calculator    │  │  (Agent 11)      │  │  (17 secrets)    │
│               │  │  (Singleton)  │  │                  │  │                  │
│• Tidal Gate   │  │• Fuel costs  │  │• Map interface   │  │• Position/radius │
│• Instant      │  │• Weather mod │  │• Travel UI       │  │• Eldritch Eye    │
│  travel       │  │• Engine eff  │  │• Fuel warnings   │  │• Hint text       │
│• Relic cost   │  │• Range calc  │  │                  │  │                  │
└───────────────┘  └──────────────┘  └──────────────────┘  └──────────────────┘
```

---

## Data Flow Diagram

### Location Loading Flow

```
Player selects location
         │
         ▼
┌─────────────────────────────────────────────┐
│ 1. LocationManager.LoadLocation(locationID) │
└─────────────────────────────────────────────┘
         │
         ├──────────────────────────────────────────┐
         │                                           │
         ▼                                           ▼
┌──────────────────────┐              ┌───────────────────────────┐
│ Check if unlocked    │              │ Check if already current  │
│ (License purchased)  │              │ (Skip if same location)   │
└──────────────────────┘              └───────────────────────────┘
         │                                           │
         ▼                                           ▼
         OK ──────────────────────────────────────► OK
         │
         ▼
┌─────────────────────────────────────────────┐
│ 2. Publish "LocationLoadingStarted" event   │
└─────────────────────────────────────────────┘
         │
         ▼
┌─────────────────────────────────────────────┐
│ 3. Coroutine: Wait for loading delay        │
│    (Simulates scene streaming)              │
└─────────────────────────────────────────────┘
         │
         ▼
┌─────────────────────────────────────────────┐
│ 4. Set currentLocation = targetLocation     │
└─────────────────────────────────────────────┘
         │
         ▼
┌─────────────────────────────────────────────┐
│ 5. Update GameState.currentLocationID       │
│    Update GameState.playerPosition (dock)   │
└─────────────────────────────────────────────┘
         │
         ▼
┌─────────────────────────────────────────────┐
│ 6. ApplyLocationSettings()                  │
│    ├─► WeatherSystem.SetAllowedWeather()    │
│    ├─► SanityManager.SetDrainModifier()     │
│    ├─► FishSpawner.SetLocationFishPool()    │
│    └─► FogSystem.EnablePermanentFog()       │
└─────────────────────────────────────────────┘
         │
         ▼
┌─────────────────────────────────────────────┐
│ 7. Publish "LocationChanged" event          │
└─────────────────────────────────────────────┘
         │
         ▼
┌─────────────────────────────────────────────┐
│ 8. Publish "LocationLoadingComplete" event  │
└─────────────────────────────────────────────┘
```

---

### Travel Flow (Normal Navigation)

```
Player initiates travel
         │
         ▼
┌───────────────────────────────────────────────┐
│ NavigationSystem.TravelToLocation(targetID)   │
└───────────────────────────────────────────────┘
         │
         ├────────────────┬───────────────┬──────────────────┐
         ▼                ▼               ▼                  ▼
    ┌─────────┐     ┌──────────┐    ┌─────────┐      ┌──────────┐
    │Already  │     │Currently │    │Location │      │Location  │
    │traveling│     │fishing?  │    │unlocked?│      │exists?   │
    └─────────┘     └──────────┘    └─────────┘      └──────────┘
         │               │                │                 │
         ▼               ▼                ▼                 ▼
        FAIL            FAIL              OK                OK
         │
         ▼
┌───────────────────────────────────────────────┐
│ TravelCostCalculator.GetTravelRequirements()  │
│  ├─► Calculate distance                       │
│  ├─► Calculate fuel cost                      │
│  └─► Calculate travel time                    │
└───────────────────────────────────────────────┘
         │
         ▼
┌───────────────────────────────────────────────┐
│ Check if player has enough fuel               │
└───────────────────────────────────────────────┘
         │
         ├───────────────────┐
         ▼                   ▼
       ENOUGH          NOT ENOUGH
         │                   │
         OK                 FAIL ──► Publish "TravelBlocked"
         │
         ▼
┌───────────────────────────────────────────────┐
│ StartCoroutine(TravelCoroutine)               │
│  1. Set isTraveling = true                    │
│  2. Publish "TravelStarted" event             │
│  3. While traveling:                          │
│     ├─► Consume fuel gradually                │
│     ├─► Update progress (0-1)                 │
│     └─► Publish "TravelProgress" event        │
│  4. Set isTraveling = false                   │
│  5. LocationManager.LoadLocation(target)      │
│  6. Publish "TravelComplete" event            │
└───────────────────────────────────────────────┘
```

---

### Fast Travel Flow

```
Player uses fast travel
         │
         ▼
┌───────────────────────────────────────────────┐
│ FastTravelSystem.FastTravel(targetID)         │
└───────────────────────────────────────────────┘
         │
         ├────────────┬──────────────┬──────────────┬─────────────┐
         ▼            ▼              ▼              ▼             ▼
    ┌─────────┐  ┌────────┐    ┌──────────┐  ┌─────────┐  ┌─────────┐
    │Fast     │  │At dock?│    │Currently │  │Location │  │Enough   │
    │travel   │  │        │    │fishing?  │  │unlocked?│  │relics?  │
    │unlocked?│  │        │    │          │  │         │  │         │
    └─────────┘  └────────┘    └──────────┘  └─────────┘  └─────────┘
         │            │              │             │             │
         ▼            ▼              ▼             ▼             ▼
        OK           OK             OK            OK           OK
         │
         ▼
┌───────────────────────────────────────────────┐
│ Publish "FastTravelStarted" event             │
└───────────────────────────────────────────────┘
         │
         ▼
┌───────────────────────────────────────────────┐
│ Play teleport effect (0.5s duration)          │
└───────────────────────────────────────────────┘
         │
         ▼
┌───────────────────────────────────────────────┐
│ LocationManager.LoadLocation(target)          │
│ (Instant - no fuel cost)                      │
└───────────────────────────────────────────────┘
         │
         ▼
┌───────────────────────────────────────────────┐
│ Publish "FastTravelComplete" event            │
└───────────────────────────────────────────────┘
```

---

### Secret Discovery Flow

```
Player moves around location
         │
         ▼
┌───────────────────────────────────────────────┐
│ EventSystem publishes "PlayerMoved" event     │
│ (from Agent 2 - BoatController)              │
└───────────────────────────────────────────────┘
         │
         ▼
┌───────────────────────────────────────────────┐
│ SecretAreaManager.OnPlayerMoved()             │
└───────────────────────────────────────────────┘
         │
         ▼
┌───────────────────────────────────────────────┐
│ Get secrets in current location               │
└───────────────────────────────────────────────┘
         │
         ▼
    For each secret:
         │
         ├────────────┬──────────────────────────┐
         ▼            ▼                          ▼
    ┌─────────┐  ┌──────────┐         ┌──────────────────┐
    │Already  │  │Within    │         │Requires          │
    │discover │  │discovery │         │Eldritch Eye?     │
    │-ed?     │  │radius?   │         │                  │
    └─────────┘  └──────────┘         └──────────────────┘
         │            │                         │
         ▼            ▼                         ▼
        SKIP         YES                    Has ability?
                      │                         │
                      OK ◄──────────────────── YES
                      │
                      ▼
         ┌────────────────────────────────────────┐
         │ DiscoverSecret()                       │
         │  ├─► Add to discoveredSecretIDs        │
         │  ├─► Award relics (1 relic)            │
         │  ├─► Publish "SecretAreaDiscovered"    │
         │  └─► Show notification                 │
         └────────────────────────────────────────┘
```

---

## Integration Architecture

```
┌─────────────────────────────────────────────────────────────────────┐
│                       BAHNFISH GAME SYSTEMS                          │
└─────────────────────────────────────────────────────────────────────┘

         ┌──────────────────────────────────────────────────┐
         │            Agent 14: World System                 │
         │                                                   │
         │  LocationManager    NavigationSystem              │
         │  SecretAreaManager  FastTravelSystem              │
         └──────────────────────────────────────────────────┘
                     │          │          │          │
        ┌────────────┼──────────┼──────────┼──────────┼────────────┐
        │            │          │          │          │            │
        ▼            ▼          ▼          ▼          ▼            ▼
┌─────────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────────┐
│   Agent 1   │ │ Agent 2  │ │ Agent 3  │ │ Agent 7  │ │   Agent 8    │
│    Core     │ │  Player  │ │ Environ  │ │  Sanity  │ │   Fish AI    │
│             │ │          │ │          │ │          │ │              │
│• EventSys   │ │• Boat    │ │• Weather │ │• Sanity  │ │• Fish Pool   │
│• GameMgr    │ │  Move    │ │  System  │ │  Drain   │ │  Update      │
│• SaveSys    │ │• Player  │ │• Lighting│ │  Mods    │ │• Spawn Rates │
│• GameState  │ │  Pos     │ │• Time    │ │• Hazards │ │• Species DB  │
└─────────────┘ └──────────┘ └──────────┘ └──────────┘ └──────────────┘
        ▲            ▲          ▲          ▲          ▲            ▲
        │            │          │          │          │            │
        └────────────┴──────────┴──────────┴──────────┴────────────┘
                         Location Change Events

        ┌────────────┬──────────────────────────────────────┐
        │            │                                      │
        ▼            ▼                                      ▼
┌──────────────┐ ┌────────────┐                    ┌──────────────┐
│   Agent 9    │ │  Agent 10  │                    │   Agent 11   │
│ Progression  │ │   Quest    │                    │     UI       │
│              │ │            │                    │              │
│• License Buy │ │• Story Loc │                    │• Location Map│
│• Abilities   │ │• NPCs      │                    │• Travel UI   │
│• Upgrades    │ │• Quests    │                    │• Fuel Warn   │
│• Relics      │ │• Altars    │                    │• Secret UI   │
└──────────────┘ └────────────┘                    └──────────────┘
```

---

## State Machine: Location States

```
┌──────────────────────────────────────────────────────────────┐
│                    LOCATION STATE MACHINE                     │
└──────────────────────────────────────────────────────────────┘

     ┌─────────────┐
     │   LOCKED    │ ◄───────────┐
     │             │              │
     │ • Cannot    │              │
     │   travel    │              │
     │ • Cannot    │         Reset/Debug
     │   load      │              │
     └─────────────┘              │
            │                     │
            │ Purchase            │
            │ License             │
            ▼                     │
     ┌─────────────┐              │
     │  UNLOCKED   │              │
     │             │              │
     │ • Can       │              │
     │   travel    │              │
     │ • Can load  │              │
     └─────────────┘              │
            │                     │
            │ Travel or           │
            │ Load Location       │
            ▼                     │
     ┌─────────────┐              │
     │   LOADING   │              │
     │             │              │
     │ • Scene     │              │
     │   stream    │              │
     │ • Loading   │              │
     │   screen    │              │
     └─────────────┘              │
            │                     │
            │ Load                │
            │ Complete            │
            ▼                     │
     ┌─────────────┐              │
     │   ACTIVE    │              │
     │  (CURRENT)  │              │
     │             │              │
     │ • Player at │              │
     │   location  │              │
     │ • Fish spawn│              │
     │ • Weather   │              │
     │   active    │              │
     └─────────────┘              │
            │                     │
            │ Travel to           │
            │ Different Loc       │
            └─────────────────────┘
```

---

## Event Flow Timeline

### Typical Play Session

```
TIME  │ EVENT                                    │ SYSTEM
──────┼──────────────────────────────────────────┼─────────────────────
00:00 │ Game Start                               │ GameManager
      │ ↓ Initialize                             │
00:01 │ LocationManager loads database           │ LocationManager
      │ ↓ Load locations from Resources          │
00:02 │ Set starter location: Calm Lake          │ LocationManager
      │ ↓ Unlock starter location                │
      │ ↓ Publish "LocationChanged"              │
00:03 │ Apply location settings                  │ LocationManager
      │ ├─► WeatherSystem.SetAllowedWeather      │ Agent 3
      │ ├─► SanityManager.SetDrainModifier       │ Agent 7
      │ └─► FishSpawner.SetLocationFishPool      │ Agent 8
      │                                           │
─────── PLAYER PLAYS AT CALM LAKE ────────────────────────────────────
      │                                           │
10:30 │ Player moves near hidden cove            │ BoatController
      │ ↓ Publish "PlayerMoved"                  │ Agent 2
10:31 │ SecretAreaManager detects proximity      │ SecretAreaManager
      │ ↓ Player within discovery radius         │
      │ ↓ Publish "SecretAreaDiscovered"         │
10:32 │ Show discovery notification              │ NotificationManager
      │ ↓ Award 1 relic                          │ ProgressionManager
      │                                           │
─────── PLAYER DECIDES TO TRAVEL ────────────────────────────────────
      │                                           │
15:00 │ Player opens map UI                      │ UI System
      │ ↓ Request unlocked locations             │
15:01 │ LocationManager returns list             │ LocationManager
      │ ↓ UI displays 2 locations                │
      │   (Calm Lake, Rocky Coastline)           │
15:02 │ Player selects Rocky Coastline           │ UI System
      │ ↓ Request travel requirements            │
15:03 │ TravelCostCalculator calculates          │ TravelCostCalculator
      │ ↓ Distance: 5 km                         │
      │ ↓ Fuel: 2.5 units                        │
      │ ↓ Time: 30 seconds                       │
15:04 │ Player confirms travel                   │ UI System
      │ ↓ NavigationSystem.TravelToLocation      │
15:05 │ Publish "TravelStarted"                  │ NavigationSystem
      │ ↓ Show loading screen                    │
      │ ↓ Begin fuel consumption                 │
15:15 │ Publish "TravelProgress" (50%)           │ NavigationSystem
15:35 │ Publish "TravelComplete"                 │ NavigationSystem
      │ ↓ LocationManager.LoadLocation           │
15:36 │ Publish "LocationChanged"                │ LocationManager
      │ ↓ Apply Rocky Coastline settings         │
15:37 │ Publish "LocationLoadingComplete"        │ LocationManager
      │ ↓ Hide loading screen                    │
      │ ↓ Player now at Rocky Coastline          │
      │                                           │
─────── PLAYER DISCOVERS TIDAL GATE ──────────────────────────────────
      │                                           │
45:00 │ Player finds Tidal Gate altar            │ SecretAreaManager
      │ ↓ Publish "SecretAreaDiscovered"         │
45:01 │ Quest system triggers unlock             │ QuestManager
      │ ↓ FastTravelSystem.UnlockFastTravel      │
45:02 │ Publish "FastTravelUnlocked"             │ FastTravelSystem
      │ ↓ Show notification                      │
      │                                           │
─────── PLAYER USES FAST TRAVEL ──────────────────────────────────────
      │                                           │
50:00 │ Player activates fast travel             │ UI System
      │ ↓ FastTravelSystem.FastTravel            │
50:01 │ Publish "FastTravelStarted"              │ FastTravelSystem
      │ ↓ Play teleport VFX                      │
50:02 │ LocationManager.LoadLocation (instant)   │ LocationManager
      │ ↓ Publish "FastTravelComplete"           │
50:03 │ Player at destination (no fuel cost)     │ FastTravelSystem
```

---

## Memory Management

### Singleton Instances (Persistent)
```
DontDestroyOnLoad:
  - LocationManager
  - NavigationSystem
  - FastTravelSystem
  - TravelCostCalculator
  - SecretAreaManager
```

### ScriptableObject Assets (Loaded Once)
```
Resources/Locations/:
  - 13 LocationData assets
  - Loaded at initialization
  - Cached in dictionary
  - Never unloaded (small footprint)
```

### Runtime Data (Per Session)
```
LocationManager:
  - unlockedLocationIDs (HashSet<string>)
  - currentLocation (LocationData reference)

NavigationSystem:
  - isTraveling (bool)
  - travelProgress (float)

SecretAreaManager:
  - discoveredSecretIDs (HashSet<string>)

TravelCostCalculator:
  - locationDistanceCache (Dictionary<string, float>)
```

### Save Data (Serialized)
```
SaveData:
  - locationData (JSON string ~200 bytes)
  - fastTravelData (JSON string ~50 bytes)
  - secretAreaData (JSON string ~100 bytes)

Total: ~350 bytes per save
```

---

## Performance Metrics

### Target Performance:
- Location loading: < 1 second (simulated)
- Travel system: 60 FPS during transit
- Secret area checks: < 0.1ms per frame
- Distance calculations: Cached, O(1) lookup
- Event publishing: < 0.01ms per event

### Optimization Strategies:
1. Dictionary-based lookups (O(1) instead of O(n))
2. Distance caching (calculate once, reuse)
3. Coroutine-based loading (non-blocking)
4. Event-driven architecture (loose coupling)
5. Minimal Update() calls (event-based only)

---

## Error Handling

### Common Error Cases:
```
1. Location Not Found
   ├─► Log warning
   ├─► Return null
   └─► Don't crash game

2. Travel While Fishing
   ├─► Publish "TravelBlocked"
   ├─► Show notification
   └─► Cancel travel

3. Insufficient Fuel
   ├─► Publish "TravelBlocked"
   ├─► Show fuel warning
   └─► Suggest refueling

4. Fast Travel Not Unlocked
   ├─► Publish "FastTravelBlocked"
   ├─► Show hint (find Tidal Gate)
   └─► Cancel fast travel

5. Secret Requires Eldritch Eye
   ├─► Show hint notification
   ├─► Don't discover yet
   └─► Re-check on next visit
```

---

This architecture provides a scalable, performant, and maintainable foundation for the World Generation system.

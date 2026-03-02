# Environment System - Agent 3

## Overview

The Environment System manages the day/night cycle, weather system, lighting, and environmental audio for Bahnfish. It creates dynamic atmospheric changes that affect gameplay and mood.

## Components

### 1. TimeManager.cs
**Core time management system**

**Features:**
- 24-hour time cycle with configurable speed (10, 15, or 20 minutes)
- Default: 15 minutes for full day/night cycle
- Tracks time periods: Day, Dusk, Night, Dawn
- Time progression with pause/speed controls
- Publishes time change events via EventSystem

**Public Methods:**
```csharp
TimeOfDay GetCurrentTimeOfDay()      // Returns current time period
float GetCurrentTime()                // Returns time in 24-hour format (0-24)
int GetCurrentHour()                  // Returns hour (0-23)
int GetCurrentMinute()                // Returns minute (0-59)
string GetTimeString()                // Returns "HH:MM" format
string GetTime12HourString()          // Returns "HH:MM AM/PM" format
bool IsDaytime()                      // True if Day or Dawn
bool IsNighttime()                    // True if Night
bool IsTransitionTime()               // True if Dusk or Dawn
float GetNormalizedTime()             // Returns 0.0-1.0 for current day
void SetTime(float time)              // Set time to specific hour
void AdvanceTime(float hours)         // Skip forward by hours
void SetTimePaused(bool paused)       // Pause/unpause time
void SetTimeSpeed(float multiplier)   // Adjust time speed
void SkipToTimeOfDay(TimeOfDay tod)   // Jump to specific period (testing)
```

**Events Published:**
- `"TimeUpdated"` - Every frame with current time
- `"TimeOfDayChanged"` - When period changes
- `"OnTimeChanged"` - With TimeChangedEventData
- `"DayCompleted"` - When 24-hour cycle completes

### 2. DayNightCycle.cs
**Visual lighting transitions**

**Features:**
- Smooth color transitions using VIDEO_ANALYSIS.md color palette
- Day: #87CEEB sky, warm tones
- Night: #1a1a2e sky, #0f3460 water
- Dusk/Dawn: Orange (#FF8C00) to purple (#4B0082) gradients
- Dynamic fog density
- Ambient light changes
- Sun rotation based on time

**Public Methods:**
```csharp
void ForceUpdate()                    // Immediately update visuals
float GetCurrentLightIntensity()      // Get current light level
Color GetCurrentAmbientColor()        // Get current ambient color
```

### 3. WeatherSystem.cs
**Dynamic weather with 3-day forecast**

**Features:**
- Four weather types: Clear, Rain, Storm, Fog
- Smooth transitions between weather states
- Configurable weather probabilities
- 3-day forecast system
- Weather affects fish spawns and visibility
- Random weather duration (2-6 hours default)

**Public Methods:**
```csharp
WeatherType GetCurrentWeather()       // Get current weather
float GetWeatherIntensity()           // Get transition intensity (0-1)
bool IsTransitioning()                // Check if transitioning
WeatherType GetTargetWeather()        // Get weather transitioning to
float GetFishSpawnMultiplier()        // Rain: 1.5x, Storm: 2.5x
float GetVisibilityRange()            // Get visibility distance
bool IsRaining()                      // Check if rain/storm
bool IsStormy()                       // Check if storm
bool IsFoggy()                        // Check if fog
bool IsClear()                        // Check if clear
List<WeatherForecast> GetForecast()   // Get 3-day forecast
WeatherForecast GetForecastForDay(int day) // Get specific day
void SetWeather(WeatherType, bool immediate) // Force weather
void ForceWeather(WeatherType)        // Testing method
```

**Events Published:**
- `"OnWeatherChanged"` - With WeatherChangedEventData
- `"WeatherChanged"` - With WeatherType

**Weather Effects:**
- **Rain**: +50% fish spawn rate, reduced visibility
- **Storm**: +150% rare fish spawn rate, high danger, low visibility
- **Fog**: 50m visibility range, reduced navigation
- **Clear**: Normal conditions, safest

### 4. LightingController.cs
**Advanced lighting with curves and gradients**

**Features:**
- Sun rotation and positioning
- Light intensity curves over 24 hours
- Color gradients for realistic transitions
- Dynamic shadow distance (performance optimization)
- Weather-based lighting adjustments
- Optional moon light
- Lens flare control
- Global illumination support

**Public Methods:**
```csharp
float GetCurrentIntensity()           // Get light intensity
Color GetCurrentLightColor()          // Get light color
Vector3 GetSunDirection()             // Get sun direction vector
void ForceUpdate()                    // Force lighting update
```

**Features:**
- Automatic sun rotation (0° sunrise, 180° sunset)
- Intensity peaks at midday (1.5), lowest at midnight (0.1)
- Color gradient from blue night to warm day
- Shadow distance: 200m day, 50m night

### 5. EnvironmentalAudio.cs
**Ambient soundscapes**

**Features:**
- Time-based ambient sounds (day, dusk, night)
- Weather-based audio layers
- Smooth crossfading between audio states
- Random ambient sound effects
- Audio effects: pitch shift, reverb, low-pass filter
- Night audio distortion (pitch lowered to 85%)
- Fog muffling with low-pass filter

**Public Methods:**
```csharp
void SetMasterVolume(float volume)    // Set overall volume
void SetAmbientVolume(float volume)   // Set ambient volume
void SetWeatherVolume(float volume)   // Set weather volume
void SetMuted(bool muted)             // Mute/unmute all
```

**Audio Layers:**
1. **Ambient Layer**: Continuous background (waves, wind)
2. **Weather Layer**: Rain, storm, fog sounds
3. **Effects Layer**: Random bird calls, eerie sounds

**Time-Based Sounds:**
- **Day**: Seagulls, gentle waves, calm atmosphere
- **Dusk/Dawn**: Evening birds, transitional sounds
- **Night**: Deep rumbles, eerie ambient, distorted pitch

## Setup Instructions

### 1. Scene Setup

1. Create an empty GameObject named "Environment Manager"
2. Add the following components:
   - TimeManager
   - DayNightCycle
   - WeatherSystem
   - LightingController
   - EnvironmentalAudio

3. The system will auto-create missing directional light if needed

### 2. Lighting Setup

1. Create a Directional Light named "Sun"
2. Assign it to LightingController's `sunLight` field
3. Optional: Create second Directional Light named "Moon" for moonlight

### 3. Audio Setup

1. Add Audio Sources to the Environment Manager GameObject
2. Import audio clips for:
   - Day ambient (calm waves, birds)
   - Night ambient (eerie atmosphere)
   - Rain/Storm sounds
   - Fog sounds
   - Random effect sounds (bird calls, etc.)
3. Assign clips to EnvironmentalAudio component

### 4. Time Configuration

In TimeManager:
- Set `dayLengthMinutes` to 15 (default from VIDEO_ANALYSIS.md)
- Or use presets: Fast (10min), Balanced (15min), Slow (20min)
- Set `startingTime` to 6.0 (dawn start)

### 5. Weather Configuration

In WeatherSystem:
- Adjust probabilities: Clear (50%), Rain (30%), Storm (10%), Fog (10%)
- Set transition duration (10 seconds default)
- Configure min/max weather duration (2-6 hours)

## Interface Contracts (for other agents)

### Events to Subscribe To:

```csharp
// Time events
EventSystem.Subscribe<float>("TimeUpdated", OnTimeUpdated);
EventSystem.Subscribe<TimeOfDay>("TimeOfDayChanged", OnTimeOfDayChanged);
EventSystem.Subscribe<TimeChangedEventData>("OnTimeChanged", OnTimeChanged);
EventSystem.Subscribe("DayCompleted", OnDayCompleted);

// Weather events
EventSystem.Subscribe<WeatherType>("WeatherChanged", OnWeatherChanged);
EventSystem.Subscribe<WeatherChangedEventData>("OnWeatherChanged", OnWeatherChangedDetailed);

// Pause events
EventSystem.Subscribe<bool>("TimePausedChanged", OnTimePausedChanged);
```

### Getting Current State:

```csharp
// Time
TimeManager timeManager = TimeManager.Instance;
TimeOfDay currentPeriod = timeManager.GetCurrentTimeOfDay();
float currentTime = timeManager.GetCurrentTime();
bool isDaytime = timeManager.IsDaytime();

// Weather
WeatherSystem weather = WeatherSystem.Instance;
WeatherType currentWeather = weather.GetCurrentWeather();
float fishMultiplier = weather.GetFishSpawnMultiplier();
float visibility = weather.GetVisibilityRange();

// Lighting
LightingController lighting = LightingController.Instance;
float lightIntensity = lighting.GetCurrentIntensity();
Vector3 sunDir = lighting.GetSunDirection();
```

## Dependencies

**Required:**
- `Scripts/Core/DataTypes.cs` - Defines TimeOfDay, WeatherType enums
- `Scripts/Core/EventSystem.cs` - Event communication system

**Optional:**
- Audio clips (game will work without audio)
- Directional light (will auto-create if missing)

## Integration with Other Agents

### Agent 7 (Sanity System)
```csharp
// Sanity drains faster at night
if (timeManager.IsNighttime()) {
    sanityDrainRate *= 2.0f;
}
```

### Agent 8 (Fish AI)
```csharp
// Weather affects fish spawns
float spawnRate = baseSpawnRate * weatherSystem.GetFishSpawnMultiplier();

// Time affects fish types
if (timeManager.IsNighttime()) {
    SpawnRareFish();
}
```

### Agent 12 (Audio System)
- EnvironmentalAudio already integrated
- Additional audio can subscribe to time/weather events

### Agent 13 (VFX)
```csharp
// Use weather to trigger particle effects
if (weatherSystem.IsRaining()) {
    EnableRainParticles();
}
```

## Testing

### Debug Controls

Add to a test script:

```csharp
void Update() {
    // Skip time with keyboard
    if (Input.GetKeyDown(KeyCode.Alpha1))
        TimeManager.Instance.SkipToTimeOfDay(TimeOfDay.Day);
    if (Input.GetKeyDown(KeyCode.Alpha2))
        TimeManager.Instance.SkipToTimeOfDay(TimeOfDay.Night);

    // Change weather
    if (Input.GetKeyDown(KeyCode.R))
        WeatherSystem.Instance.ForceWeather(WeatherType.Rain);
    if (Input.GetKeyDown(KeyCode.S))
        WeatherSystem.Instance.ForceWeather(WeatherType.Storm);
    if (Input.GetKeyDown(KeyCode.C))
        WeatherSystem.Instance.ForceWeather(WeatherType.Clear);

    // Speed up time
    if (Input.GetKey(KeyCode.LeftShift))
        TimeManager.Instance.SetTimeSpeed(5f);
    else
        TimeManager.Instance.SetTimeSpeed(1f);
}
```

### Expected Behavior

1. **Day Cycle**: 15 minutes real-time = 24 game hours
2. **Time Periods**:
   - Dawn: 5:00 - 8:00
   - Day: 8:00 - 18:00
   - Dusk: 18:00 - 20:00
   - Night: 20:00 - 5:00

3. **Weather Changes**: Every 2-6 game hours randomly
4. **Audio**: Smooth crossfades between ambient states
5. **Lighting**: Smooth sun rotation and color transitions

## Performance Notes

- Dynamic shadow distance optimization (200m day, 50m night)
- Audio crossfades prevent abrupt changes
- Singleton pattern ensures only one instance
- Events use EventSystem to prevent tight coupling
- All components use DontDestroyOnLoad for persistence

## Future Enhancements

- Cloud system with weather
- Lightning flashes during storms
- Underwater lighting effects
- Seasonal variations
- Moon phases affecting tides
- Aurora borealis at night in arctic locations

## Color Reference (VIDEO_ANALYSIS.md)

### Day Colors
- Sky: #87CEEB (Sky Blue)
- Horizon: #B0E0E6 (Light Blue)
- Water: #4682B4 (Steel Blue)

### Dusk/Dawn Colors
- Sky Top: #4B0082 (Indigo)
- Sky Bottom: #FF8C00 (Orange)

### Night Colors
- Sky: #1a1a2e (Very Dark Blue)
- Water: #0f3460 (Deep Blue)
- Fog: #16213e at 60% opacity

## Troubleshooting

**Issue**: Time not progressing
- Check `timePaused` is false in TimeManager
- Verify TimeManager is in scene

**Issue**: No lighting changes
- Ensure directional light is assigned
- Check DayNightCycle is attached to same GameObject as TimeManager

**Issue**: No audio
- Verify audio clips are assigned
- Check master volume is not 0
- Ensure AudioListener exists in scene

**Issue**: Weather not changing
- Check min/max duration settings
- Verify WeatherSystem is initialized

## Contact

Agent 3: Time & Environment System
Completed: 2026-03-01
Dependencies: Agent 1 (Core Architecture) ✓

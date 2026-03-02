# Photography Mode System

**Agent 18: Photography Mode Specialist**

A comprehensive photography system for Bahnfish that allows players to capture memorable moments, complete the fish encyclopedia with photos, and share their catches. Features 20+ Instagram-quality filters, composition tools, photo challenges, and a complete gallery system.

---

## Table of Contents

1. [Overview](#overview)
2. [Core Components](#core-components)
3. [Photo Mode Controls](#photo-mode-controls)
4. [Photo Filters](#photo-filters)
5. [Camera Settings](#camera-settings)
6. [Quality Rating System](#quality-rating-system)
7. [Fish Encyclopedia](#fish-encyclopedia)
8. [Photo Challenges](#photo-challenges)
9. [Sharing System](#sharing-system)
10. [Integration Guide](#integration-guide)
11. [Events & Architecture](#events--architecture)

---

## Overview

The Photography Mode system provides players with a professional-grade camera tool to:
- **Capture high-resolution photos** up to 4K resolution
- **Apply 20+ filters** with real-time preview
- **Complete the fish encyclopedia** by photographing all 60 species
- **Complete 30+ challenges** for rewards
- **Share photos** with watermarks and stat overlays

### Key Features

- Free camera movement with pause functionality
- Instagram-quality photo filters
- 1-5 star quality rating system
- Fish encyclopedia completion tracking
- 30+ photo challenges with monetary rewards
- High-resolution export (1080p, 1440p, 4K)
- Watermark and sharing tools
- Complete gallery management

---

## Core Components

### 1. PhotoModeController.cs

Main controller for photo mode functionality.

**Responsibilities:**
- Enter/exit photo mode (P key)
- Free camera movement (WASD + mouse)
- Pause game time during photo mode
- Camera settings management
- Photo capture coordination

**Key Methods:**
```csharp
PhotoModeController.Instance.EnterPhotoMode();
PhotoModeController.Instance.ExitPhotoMode();
PhotoModeController.Instance.SetFOV(float fov);
PhotoModeController.Instance.SetExposure(float exposure);
```

### 2. CameraEffects.cs

Manages photo filters and post-processing effects.

**Features:**
- 20+ photo filters organized by category
- Real-time filter preview
- Intensity controls (0-100%)
- Multiple filter stacking

**Key Methods:**
```csharp
CameraEffects.Instance.ApplyFilter(FilterType.Sepia, 0.8f);
CameraEffects.Instance.RemoveFilter(FilterType.Sepia);
CameraEffects.Instance.ClearAllFilters();
```

### 3. FramingTools.cs

Composition aids and framing tools.

**Features:**
- Rule of thirds grid
- Golden ratio spiral
- Center crosshair
- Safe frame guides
- Focus point indicator
- Composition analysis

**Key Methods:**
```csharp
FramingTools.Instance.ToggleRuleOfThirds();
FramingTools.Instance.AnalyzeComposition(Vector2 subjectPosition);
```

### 4. PhotoStorage.cs

Screenshot capture and photo management.

**Features:**
- High-resolution capture (up to 4K)
- Photo metadata storage
- Gallery management
- Disk export (PNG/JPG)

**Key Methods:**
```csharp
PhotoMetadata photo = PhotoStorage.Instance.CapturePhoto(camera);
List<PhotoMetadata> gallery = PhotoStorage.Instance.GetPhotoGallery();
PhotoStorage.Instance.DeletePhoto(photoID);
```

### 5. EncyclopediaPhoto.cs

Fish encyclopedia photo system with quality rating.

**Features:**
- 1-5 star quality rating
- Encyclopedia completion tracking
- Photo replacement (better quality)
- Collection rewards

**Key Methods:**
```csharp
bool hasPhoto = EncyclopediaPhoto.Instance.HasPhotoForSpecies(fishID);
float completion = EncyclopediaPhoto.Instance.GetCompletionPercentage();
PhotographerStats stats = EncyclopediaPhoto.Instance.GetStatistics();
```

### 6. ShareSystem.cs

Photo sharing and export features.

**Features:**
- Watermark application
- Stats overlay generation
- Copy to clipboard
- Permalink generation
- Custom export settings

**Key Methods:**
```csharp
string path = ShareSystem.Instance.PreparePhotoForSharing(photo, includeStats);
ShareSystem.Instance.CopyPhotoToClipboard(photo);
string permalink = ShareSystem.Instance.GeneratePermalink(photo);
```

### 7. PhotoChallenges.cs

30+ photography challenges with rewards.

**Challenge Categories:**
- **Species Challenges** (12): Photograph fish by rarity/location
- **Action Challenges** (8): Capture dynamic moments
- **Artistic Challenges** (6): Master composition and filters
- **Event Challenges** (4): Photograph special events

**Key Methods:**
```csharp
List<PhotoChallenge> active = PhotoChallenges.Instance.GetActiveChallenges();
PhotoChallenge challenge = PhotoChallenges.Instance.GetChallengeByID(id);
float completion = PhotoChallenges.Instance.GetCompletionPercentage();
```

### 8. PhotoUI.cs

Complete UI system for photo mode.

**UI Panels:**
- Photo mode HUD
- Camera settings panel
- Filter selection menu
- Photo gallery viewer
- Challenge tracker
- Encyclopedia browser

**Key Methods:**
```csharp
PhotoUI.Instance.ToggleGallery();
PhotoUI.Instance.ToggleChallenges();
PhotoUI.Instance.ShowPanel("encyclopedia");
```

---

## Photo Mode Controls

### Camera Movement

| Input | Action |
|-------|--------|
| **WASD** | Move camera horizontally |
| **Q** | Move camera down |
| **E** | Move camera up |
| **Shift** | Sprint (2x speed) |
| **Ctrl** | Slow (0.5x speed) |
| **Right Mouse** | Look around |
| **Scroll Wheel** | Zoom in/out (FOV 30-90°) |

### Photo Actions

| Input | Action |
|-------|--------|
| **Space** | Take photo |
| **R** | Reset camera position |
| **P** | Exit photo mode |
| **Tab** | Toggle settings panel |
| **G** | Toggle gallery |
| **H** | Toggle help |

### Distance Limit

Camera movement is limited to **100 meters** from the player to prevent getting lost.

---

## Photo Filters

### Classic Filters

1. **Sepia Tone** - Vintage brown tint
2. **Black & White** - Monochrome conversion
3. **Vintage** - Faded colors with grain
4. **Film Noir** - High contrast B&W
5. **Polaroid** - Instant camera look

### Artistic Filters

6. **Oil Paint** - Painterly effect
7. **Watercolor** - Soft, washed colors
8. **Sketch** - Pencil drawing look
9. **Cel Shading** - Comic book style
10. **Impressionist** - Monet-style blur

### Enhancement Filters

11. **HDR** - High dynamic range
12. **Bloom** - Light glow effect
13. **Vignette** - Darkened corners
14. **Sharpness** - Clarity boost
15. **Color Pop** - Selective color

### Creative Filters

16. **Fisheye** - Circular distortion
17. **Tilt-Shift** - Miniature effect
18. **Chromatic Aberration** - Color fringing
19. **Glitch** - Digital corruption
20. **Retrowave** - 80s aesthetic

### Using Filters

```csharp
// Apply single filter
CameraEffects.Instance.ApplyFilter(FilterType.Sepia, 0.8f);

// Stack multiple filters
CameraEffects.Instance.ApplyFilter(FilterType.Vintage, 0.5f);
CameraEffects.Instance.ApplyFilter(FilterType.Vignette, 0.6f);

// Adjust intensity
CameraEffects.Instance.ApplyFilter(FilterType.HDR, 0.3f);

// Remove filter
CameraEffects.Instance.RemoveFilter(FilterType.Sepia);
```

---

## Camera Settings

### Field of View (FOV)

- **Range:** 30° to 90°
- **Default:** 60°
- **Use:** Wide angle (90°) for landscapes, narrow (30°) for portraits

```csharp
PhotoModeController.Instance.SetFOV(45f);
```

### Exposure

- **Range:** -2 to +2 stops
- **Default:** 0
- **Use:** Brighten (+) or darken (-) the image

```csharp
PhotoModeController.Instance.SetExposure(1.5f);
```

### Contrast

- **Range:** 0% to 200%
- **Default:** 100%
- **Use:** Increase separation between light and dark

```csharp
PhotoModeController.Instance.SetContrast(1.5f);
```

### Saturation

- **Range:** 0% to 200%
- **Default:** 100%
- **Use:** 0% = grayscale, 200% = vibrant colors

```csharp
PhotoModeController.Instance.SetSaturation(1.3f);
```

### Brightness

- **Range:** -50 to +50
- **Default:** 0
- **Use:** Global brightness adjustment

```csharp
PhotoModeController.Instance.SetBrightness(10f);
```

### Tilt (Dutch Angle)

- **Range:** -45° to +45°
- **Default:** 0°
- **Use:** Dramatic angled compositions

```csharp
PhotoModeController.Instance.SetTilt(15f);
```

### Depth of Field

- **Focus Distance:** 0+ meters
- **Blur Amount:** 0 to 1

```csharp
PhotoModeController.Instance.SetFocusDistance(10f);
PhotoModeController.Instance.SetDepthOfFieldBlur(0.5f);
```

---

## Quality Rating System

Photos are rated on a **1-5 star scale** based on:

### Rating Factors

1. **Composition (0-1 stars)**
   - Rule of thirds alignment
   - Subject centering
   - Golden ratio positioning

2. **Focus (0-1 stars)**
   - Image sharpness
   - Subject clarity

3. **Lighting (0-1 stars)**
   - Exposure quality
   - Time of day
   - Weather conditions

4. **Visibility (0-1 stars)**
   - Fish coverage (>30% of frame required)
   - Subject obstruction
   - Clarity

5. **Rarity Bonus (0-1 stars)**
   - Common: +0.2
   - Uncommon: +0.4
   - Rare: +0.6
   - Aberrant: +0.8
   - Legendary: +1.0

### Star Ratings

- ⭐ **(1 star):** Poor quality - doesn't count for encyclopedia
- ⭐⭐ **(2 stars):** Below average - doesn't count
- ⭐⭐⭐ **(3 stars):** Good quality - **counts for encyclopedia**
- ⭐⭐⭐⭐ **(4 stars):** Great quality
- ⭐⭐⭐⭐⭐ **(5 stars):** Perfect shot - bonus rewards!

### Encyclopedia Requirements

To add a photo to the encyclopedia:
- **Minimum 3 stars** quality
- Fish occupies **>30% of frame**
- Fish **clearly identifiable** (not obscured)
- Fish **in focus** (not blurry)

---

## Fish Encyclopedia

### Overview

Photograph all **60 fish species** to complete the encyclopedia.

### Completion Rewards

| Species Count | Reward | Achievement |
|--------------|--------|-------------|
| **10 species** | $500 | Amateur Photographer |
| **25 species** | $2,000 + Polaroid Filter | Aspiring Photographer |
| **40 species** | $5,000 | Professional Photographer |
| **60 species** | $15,000 + Ghost Companion | Master Photographer |

### Photo Replacement

Better quality photos automatically replace lower quality ones:

```csharp
// Check if species has been photographed
bool hasPhoto = EncyclopediaPhoto.Instance.HasPhotoForSpecies("bluegill");

// Get current best photo
EncyclopediaPhotoEntry entry = EncyclopediaPhoto.Instance.GetEncyclopediaEntry("bluegill");
float currentQuality = entry.bestPhotoQuality;
```

### Encyclopedia Statistics

```csharp
PhotographerStats stats = EncyclopediaPhoto.Instance.GetStatistics();

Debug.Log($"Species Photographed: {stats.speciesPhotographed}/60");
Debug.Log($"Completion: {stats.completionPercentage}%");
Debug.Log($"Total Photos: {stats.totalPhotosTaken}");
Debug.Log($"Average Quality: {stats.averageQuality} stars");
Debug.Log($"Perfect Shots: {stats.perfectShots}");
```

---

## Photo Challenges

### Species Challenges (12)

1. **Photograph 5 Common Fish** - $200
2. **Photograph 3 Uncommon Fish** - $500
3. **Photograph 1 Rare Fish** - $1,000
4. **Photograph 1 Legendary Fish** - $3,000
5. **Photograph 1 Aberrant Fish** - $2,000
6. **Location Complete** (all fish in one location) - $1,500
7. **Rainbow Collection** (one of each rarity) - $4,000
8. **Photo Spree** (10 species in one session) - $2,500
9. **Night Hunter** (3 fish at night) - $800
10. **Deep Sea Explorer** (fish below 50m) - $1,200
11. **Storm Chaser** (fish during storm) - $900
12. **Around the Clock** (fish at all 4 times) - $1,500

### Action Challenges (8)

1. **Air Time** (fish jumping) - $800
2. **The Fight** (fish fighting on line) - $1,200
3. **Reeling In** (fish being reeled) - $600
4. **Caught in the Act** (theft by crow/phantom) - $2,000
5. **Phantom Voyage** (Ghost Ship in background) - $2,500
6. **Feeding Time** (feeding frenzy) - $1,500
7. **Triple Threat** (3 fish breaching) - $2,000
8. **Legendary Battle** (legendary fish fight) - $3,500

### Artistic Challenges (6)

1. **Vintage Vibes** (5 photos with Sepia) - $300
2. **Perfect Shot** (5-star photo) - $2,000
3. **Composition Master** (perfect rule of thirds) - $1,000
4. **Shadow Play** (silhouette at sunset) - $1,200
5. **Miniature World** (Tilt-Shift on landmark) - $800
6. **Filter Fanatic** (3+ filters on one photo) - $1,500

### Event Challenges (4)

1. **Blood Moon Rising** (Blood Moon event) - $5,000
2. **Shooting Stars** (Meteor Shower) - $3,000
3. **Northern Lights** (Aurora Borealis) - $2,500
4. **Weather Watcher** (all 4 weather types) - $2,000

### Category Completion Bonuses

Complete all challenges in a category for bonus rewards:

- **Species Expert:** $5,000
- **Action Photographer:** $4,000
- **Artist:** $3,000
- **Event Master:** $6,000

---

## Sharing System

### Watermark

Add game branding to photos:

```csharp
// Enable watermark
ShareSystem.Instance.SetWatermarkEnabled(true);

// Customize text
ShareSystem.Instance.SetWatermarkText("Bahnfish - MyUsername");

// Set position
ShareSystem.Instance.SetWatermarkPosition(WatermarkPosition.BottomRight);
```

### Stats Overlay

Add game information to photos:

```csharp
// Enable stats overlay
ShareSystem.Instance.SetStatsOverlayEnabled(true);
```

Stats overlay includes:
- Fish species name
- Rarity tier
- Location captured
- Time & weather
- Quality rating
- "Captured in Bahnfish" branding

### Export Options

```csharp
ShareExportSettings settings = new ShareExportSettings
{
    includeWatermark = true,
    includeStats = true,
    resolution = ExportResolution.UHD_4K,
    format = ExportFormat.PNG
};

string path = ShareSystem.Instance.ExportPhoto(photo, settings);
```

### Copy to Clipboard

```csharp
ShareSystem.Instance.CopyPhotoToClipboard(photo);
```

### Generate Permalink

```csharp
string permalink = ShareSystem.Instance.GeneratePermalink(photo);
// Returns: "https://bahnfish.com/photos/{guid}"
```

---

## Integration Guide

### Setup

1. **Add Photography Manager to scene:**
   ```
   Create empty GameObject named "PhotographyManager"
   Add: PhotoModeController
   Add: CameraEffects
   Add: FramingTools
   Add: PhotoStorage
   Add: EncyclopediaPhoto
   Add: ShareSystem
   Add: PhotoChallenges
   Add: PhotoUI
   ```

2. **Configure Camera References:**
   - Assign main camera to PhotoModeController
   - Assign camera to CameraEffects

3. **Setup UI:**
   - Create Canvas for photo mode UI
   - Assign UI references in PhotoUI component

### Example Integration

```csharp
using UnityEngine;

public class GameExample : MonoBehaviour
{
    private void Update()
    {
        // Player presses P to enter photo mode
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (PhotoModeController.Instance != null)
            {
                PhotoModeController.Instance.TogglePhotoMode();
            }
        }
    }

    // Called when fish is caught
    private void OnFishCaught(string fishID)
    {
        // Suggest taking a photo
        Debug.Log("Take a photo with P key!");
    }

    // Check encyclopedia completion
    private void CheckEncyclopediaProgress()
    {
        if (EncyclopediaPhoto.Instance != null)
        {
            float completion = EncyclopediaPhoto.Instance.GetCompletionPercentage();
            Debug.Log($"Encyclopedia: {completion}% complete");
        }
    }
}
```

### Save/Load Integration

Photography data is automatically saved in `SaveData.photographyData`:

```csharp
// Save
SaveData saveData = new SaveData();
saveData.photographyData = new PhotographyData
{
    totalPhotosTaken = EncyclopediaPhoto.Instance.GetTotalPhotosTaken(),
    averagePhotoQuality = EncyclopediaPhoto.Instance.GetAveragePhotoQuality(),
    perfectShots = EncyclopediaPhoto.Instance.GetPerfectShotCount()
    // ... etc
};

// Load
PhotographyData photoData = saveData.photographyData;
// Restore photography state
```

---

## Events & Architecture

### Published Events

| Event Name | Data Type | Description |
|------------|-----------|-------------|
| `PhotoModeEntered` | none | Photo mode activated |
| `PhotoModeExited` | none | Photo mode deactivated |
| `PhotoSaved` | PhotoMetadata | Photo captured and saved |
| `EncyclopediaPhotoAdded` | string (fishID) | New species photographed |
| `EncyclopediaPhotoUpdated` | string (fishID) | Better photo replaced old one |
| `PhotoChallengeProgress` | PhotoChallenge | Challenge progressed |
| `PhotoChallengeCompleted` | PhotoChallenge | Challenge completed |
| `HighQualityPhotoTaken` | PhotoMetadata | 5-star photo captured |
| `FilterUnlocked` | FilterType | New filter unlocked |
| `AchievementUnlocked` | string | Photography achievement |

### Subscribed Events

| Event Name | Handler | Purpose |
|------------|---------|---------|
| `FishCaught` | PhotoChallenges | Check species challenges |
| `DynamicEventStarted` | PhotoChallenges | Track event photography |
| `LocationChanged` | PhotoStorage | Update location metadata |

### Example Event Usage

```csharp
// Subscribe to photo events
EventSystem.Subscribe<PhotoMetadata>("PhotoSaved", OnPhotoSaved);

private void OnPhotoSaved(PhotoMetadata photo)
{
    if (photo.qualityRating >= 5f)
    {
        Debug.Log("Perfect shot!");
        // Trigger particle effects, sound, etc.
    }
}

// Publish custom event
EventSystem.Publish("CustomPhotoEvent", photoData);
```

---

## File Structure

```
Scripts/Photography/
├── PhotoModeController.cs      (462 lines) - Photo mode management
├── CameraEffects.cs            (447 lines) - Filter system
├── FramingTools.cs             (338 lines) - Composition tools
├── PhotoStorage.cs             (423 lines) - Photo capture & storage
├── EncyclopediaPhoto.cs        (370 lines) - Encyclopedia system
├── ShareSystem.cs              (351 lines) - Sharing features
├── PhotoChallenges.cs          (464 lines) - Challenge system
├── PhotoUI.cs                  (548 lines) - User interface
└── README.md                   (This file)

Total: 3,403+ lines of production code
```

---

## Technical Details

### Photo Capture Process

1. Player presses Space/Left Mouse in photo mode
2. PhotoModeController calls PhotoStorage.CapturePhoto()
3. Camera renders to RenderTexture at target resolution
4. Texture converted to Texture2D
5. CameraEffects applies active filters
6. PhotoStorage creates metadata
7. EncyclopediaPhoto rates quality (1-5 stars)
8. Photo saved to disk (PNG/JPG)
9. Metadata added to gallery
10. Events published (PhotoSaved, etc.)
11. PhotoChallenges checks progress
12. UI updated

### Photo Quality Calculation

```
Quality = Composition (0-1)
        + Focus (0-1)
        + Lighting (0-1)
        + Visibility (0-1)
        + Rarity Bonus (0-1)
        = Total (1-5 stars)
```

### Performance Considerations

- Photos rendered at chosen resolution (up to 4K)
- Filters applied on CPU (consider GPU optimization)
- Gallery loads thumbnails lazily
- Time.timeScale = 0 during photo mode (uses Time.unscaledDeltaTime)
- Photo files stored in persistent data path
- Maximum 500 photos in gallery (auto-cleanup oldest)

---

## Future Enhancements

Potential additions for future development:

1. **Advanced Filters**
   - Custom filter creation
   - LUT-based color grading
   - GPU-accelerated filters

2. **Social Features**
   - Online photo gallery
   - Community voting
   - Photo contests

3. **Camera Modes**
   - Timelapse
   - Burst mode (rapid fire)
   - Panorama stitching
   - 360° photos

4. **Advanced Composition**
   - Fibonacci spiral
   - Dynamic symmetry
   - Leading lines detection
   - AI-powered composition suggestions

5. **Photo Effects**
   - Depth of field with bokeh
   - Motion blur
   - Lens flares
   - Light leaks

---

## Credits

**Agent 18: Photography Mode Specialist**

Developed as part of the Bahnfish multi-agent development system.

Integration with:
- Agent 8: Fish AI & Behavior (FishDatabase, FishSpeciesData)
- Agent 19: Dynamic Events (Event photography)
- Agent 1: Core Systems (EventSystem, SaveData)
- Agent 7: Time & Environment (TimeManager, WeatherSystem)

---

## Version History

**v1.0.0** - Initial Release
- Photo mode with free camera
- 20+ photo filters
- 1-5 star quality rating
- Fish encyclopedia system (60 species)
- 30+ photo challenges
- Photo gallery & sharing
- Complete save/load integration

---

## Support

For integration help or bug reports, refer to the main Bahnfish documentation or contact the development team.

**Happy Photography!** 📷✨

# Photography Mode - Quick Reference Card

## 📷 Photo Mode Controls

| Key | Action |
|-----|--------|
| **P** | Enter/Exit Photo Mode |
| **WASD** | Move Camera |
| **Q/E** | Up/Down |
| **Shift** | Sprint (2x) |
| **Ctrl** | Slow (0.5x) |
| **Right Mouse** | Look Around |
| **Scroll** | Zoom (FOV) |
| **Space** | Take Photo |
| **R** | Reset Camera |
| **Tab** | Settings |
| **G** | Gallery |
| **H** | Help |

## 🎨 Photo Filters

### Classic (5)
Sepia • Black & White • Vintage • Film Noir • Polaroid

### Artistic (5)
Oil Paint • Watercolor • Sketch • Cel Shading • Impressionist

### Enhancement (5)
HDR • Bloom • Vignette • Sharpness • Color Pop

### Creative (5)
Fisheye • Tilt-Shift • Chromatic Aberration • Glitch • Retrowave

## ⭐ Quality Rating

| Stars | Quality | Encyclopedia |
|-------|---------|--------------|
| ⭐ | Poor | No |
| ⭐⭐ | Below Average | No |
| ⭐⭐⭐ | Good | **YES** |
| ⭐⭐⭐⭐ | Great | **YES** |
| ⭐⭐⭐⭐⭐ | Perfect | **YES + Bonus** |

**Encyclopedia Requirements:**
- Minimum 3 stars
- Fish >30% of frame
- Fish in focus
- Fish clearly visible

## 📖 Encyclopedia Rewards

| Species | Reward |
|---------|--------|
| **10** | $500 + Amateur Photographer |
| **25** | $2,000 + Polaroid Filter |
| **40** | $5,000 + Professional |
| **60** | $15,000 + Ghost Companion |

## 🏆 Challenge Categories

### Species (12 challenges)
Photograph fish by rarity, location, time

### Action (8 challenges)
Capture jumping, fighting, events

### Artistic (6 challenges)
Perfect shots, filters, composition

### Event (4 challenges)
Blood Moon, Meteor Shower, Aurora, Weather

**Total Rewards:** $50,000+ available

## ⚙️ Camera Settings

| Setting | Range | Default |
|---------|-------|---------|
| FOV | 30-90° | 60° |
| Exposure | -2 to +2 | 0 |
| Contrast | 0-200% | 100% |
| Saturation | 0-200% | 100% |
| Brightness | -50 to +50 | 0 |
| Tilt | -45° to +45° | 0° |

## 💾 Export Options

### Resolutions
- 1080p (1920x1080)
- 1440p (2560x1440)
- 4K (3840x2160)

### Formats
- PNG (lossless)
- JPG (compressed)

### Watermark Positions
Top-Left • Top-Right • Bottom-Left • Bottom-Right

## 🔧 Quick API

```csharp
// Enter photo mode
PhotoModeController.Instance.TogglePhotoMode();

// Apply filter
CameraEffects.Instance.ApplyFilter(FilterType.Sepia, 0.8f);

// Take photo
PhotoStorage.Instance.CapturePhoto(camera);

// Check encyclopedia
float progress = EncyclopediaPhoto.Instance.GetCompletionPercentage();

// Get challenges
var challenges = PhotoChallenges.Instance.GetActiveChallenges();

// Share photo
ShareSystem.Instance.PreparePhotoForSharing(photo, true);
```

## 📊 Statistics

Track your photography skills:
- Total Photos Taken
- Average Photo Quality
- Species Photographed (X/60)
- Perfect Shots (5 stars)
- Challenges Completed
- Encyclopedia Completion %

## 🎯 Pro Tips

1. **Golden Hour** - Best lighting at dawn/dusk
2. **Rule of Thirds** - Position fish at grid intersections
3. **Fill Frame** - Get close for >30% coverage
4. **Steady Camera** - Use slow mode for sharpness
5. **Filter Stack** - Combine 2-3 filters for unique looks
6. **Challenge First** - Check active challenges before shooting
7. **Encyclopedia Priority** - Focus on missing species
8. **Perfect Shots** - 5-star photos give bonus rewards
9. **Event Photography** - High-value challenges
10. **Quality Over Quantity** - Better photos replace old ones

## 📱 Gallery Management

- **Max Photos:** 500 (auto-cleanup oldest)
- **Storage:** Application.persistentDataPath/Photos/
- **Metadata:** Full EXIF-like data saved
- **Delete:** Via gallery UI
- **Filter:** By fish species, quality, date

## 🎨 Composition Tools

- **Rule of Thirds** - Classic composition grid
- **Golden Ratio** - Fibonacci spiral guide
- **Center Crosshair** - Perfect centering
- **Safe Frames** - TV-safe boundaries
- **Focus Point** - Dynamic focus indicator

## 📈 Progression Path

1. **Start:** Take first photo (any quality)
2. **Learn:** Experiment with filters and settings
3. **Practice:** Improve composition and quality
4. **Complete:** Photograph all 60 species
5. **Master:** Complete all challenges
6. **Perfect:** Achieve 5-star average quality

## 🎉 Achievements

- Amateur Photographer (10 species)
- Aspiring Photographer (25 species)
- Professional Photographer (40 species)
- Master Photographer (60 species)
- Species Expert (all species challenges)
- Action Photographer (all action challenges)
- Artist (all artistic challenges)
- Event Master (all event challenges)

---

**For detailed documentation, see README.md**
**For code examples, see INTEGRATION_EXAMPLE.cs**

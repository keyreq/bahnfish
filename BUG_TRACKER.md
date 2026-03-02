# Bahnfish Bug Tracker

**Last Updated**: 2026-03-01
**Phase**: 6 - Testing & Launch Preparation
**Status**: Active Development

---

## Bug Statistics

| Severity | Open | In Progress | Fixed | Won't Fix | Total |
|----------|------|-------------|-------|-----------|-------|
| Critical | 0    | 0           | 0     | 0         | 0     |
| High     | 0    | 0           | 0     | 0         | 0     |
| Medium   | 0    | 0           | 0     | 0         | 0     |
| Low      | 0    | 0           | 0     | 0         | 0     |
| **Total**| **0**| **0**       | **0** | **0**     | **0** |

---

## Active Bugs

### Critical (Game-Breaking)
> Bugs that prevent game from running, cause crashes, or result in data loss.

*No critical bugs reported.*

---

### High Priority (Major Features Broken)
> Bugs that significantly impact gameplay or break major features.

*No high priority bugs reported.*

---

### Medium Priority (Minor Features Broken)
> Bugs that affect gameplay but have workarounds.

*No medium priority bugs reported.*

---

### Low Priority (Polish Issues)
> Visual glitches, minor UI issues, or cosmetic problems.

*No low priority bugs reported.*

---

## Fixed Bugs

*No bugs fixed yet.*

---

## Won't Fix

*No bugs marked as won't fix.*

---

## Bug Report Template

```markdown
### BUG-XXX: [Short Description]

**Severity**: Critical / High / Medium / Low
**Status**: Open / In Progress / Fixed / Won't Fix
**Reporter**: [Name]
**Date Reported**: YYYY-MM-DD
**Date Fixed**: YYYY-MM-DD (if applicable)

**Systems Affected**:
- [System Name 1]
- [System Name 2]

**Description**:
Clear, detailed description of the bug.

**Steps to Reproduce**:
1. Step one
2. Step two
3. Step three
4. Bug occurs

**Expected Behavior**:
What should happen when following the steps above.

**Actual Behavior**:
What actually happens instead.

**Frequency**:
Always / Often / Sometimes / Rarely

**Environment**:
- Unity Version: 2022.x.x
- Platform: Windows 10/11 / macOS / Linux
- Build Type: Editor / Development / Release
- Hardware: [CPU, GPU, RAM]

**Screenshots/Videos**:
[Attach or link to visual evidence]

**Console Output**:
```
[Paste relevant console errors/warnings]
```

**Additional Context**:
Any other relevant information.

**Fix Notes** (if fixed):
Description of how the bug was fixed.

**Testing Notes** (if fixed):
How to verify the fix works.
```

---

## How to Report Bugs

### For Developers
1. Create a new bug entry using the template above
2. Assign a unique bug ID (BUG-001, BUG-002, etc.)
3. Set severity based on impact
4. Add to appropriate severity section
5. Update statistics table
6. Create GitHub issue (if using issue tracker)

### For Testers
1. Check if bug already exists
2. Fill out bug report template completely
3. Include screenshots/videos when possible
4. Attach console output for crashes/errors
5. Submit to development team

---

## Severity Guidelines

### Critical
- Game crashes on startup
- Save file corruption
- Infinite loops or freezes
- Cannot progress in main story
- Major memory leaks
- Security vulnerabilities

### High
- Major features completely broken
- Significant gameplay impact
- Multiplayer desyncs
- Major performance issues (< 15 FPS)
- Game-breaking exploits
- Data loss scenarios

### Medium
- Minor features not working correctly
- Workarounds exist
- Non-critical UI issues
- Minor performance issues (15-30 FPS)
- Incorrect values or calculations
- Audio/visual glitches that affect gameplay

### Low
- Cosmetic issues
- Minor visual glitches
- Typos in text
- Non-critical UI alignment
- Missing sound effects
- Performance issues at extreme settings

---

## Testing Phases

### Phase 1: Functional Testing (Week 32)
Test all game systems work as designed:
- [ ] Core Systems (Phase 1)
- [ ] Player & Input (Phase 2)
- [ ] Fishing Mechanics (Phase 2)
- [ ] Inventory System (Phase 2)
- [ ] Sanity & Horror (Phase 2)
- [ ] Fish AI (Phase 2)
- [ ] UI/UX (Phase 2)
- [ ] Progression & Economy (Phase 3)
- [ ] Quest & Narrative (Phase 3)
- [ ] Locations & World (Phase 3)
- [ ] Dynamic Events (Phase 3)
- [ ] Cooking & Crafting (Phase 4)
- [ ] Aquarium & Breeding (Phase 4)
- [ ] Companions & Crew (Phase 4)
- [ ] Photography Mode (Phase 4)
- [ ] Idle/AFK System (Phase 4)
- [ ] Audio System (Phase 5)
- [ ] Visual Effects (Phase 5)
- [ ] Accessibility (Phase 5)

### Phase 2: Integration Testing (Week 33)
Test systems working together:
- [ ] Complete fishing loop (catch → inventory → sell)
- [ ] Breeding loop (catch → breed → sell offspring)
- [ ] Companion loop (pet → ability → loyalty)
- [ ] Photography loop (photo → encyclopedia → challenge)
- [ ] Idle loop (offline → return → earnings → spend)
- [ ] Atmosphere loop (time → music → ambient → lighting)
- [ ] Horror loop (sanity → effects → hazards → audio)
- [ ] Event loop (start → music → VFX → rewards)

### Phase 3: Performance Testing (Week 33)
- [ ] Maintain 60 FPS on mid-range PC
- [ ] No memory leaks over 4-hour session
- [ ] Audio pooling working (max 32 concurrent)
- [ ] Particle pooling working (max 10,000)
- [ ] Save file size reasonable (<10MB)
- [ ] Load times under 5 seconds
- [ ] No stuttering during transitions

### Phase 4: Balance Testing (Week 34)
- [ ] Progression curve feels smooth
- [ ] Economy balanced (can afford upgrades)
- [ ] Night risk/reward feels fair
- [ ] Fishing difficulty appropriate
- [ ] Sanity drain not too punishing
- [ ] Idle earnings not exploitable

### Phase 5: Edge Cases (Week 34)
- [ ] Saving during critical operations
- [ ] Loading corrupted save files
- [ ] Max inventory (full grid)
- [ ] Max concurrent buffs (10)
- [ ] 0 sanity behavior
- [ ] Negative money scenarios
- [ ] Offline for 7+ days

### Phase 6: Platform Testing (Week 35)
- [ ] Windows 10/11
- [ ] macOS (if applicable)
- [ ] Linux (if applicable)
- [ ] Different resolutions (720p, 1080p, 1440p, 4K)
- [ ] Different aspect ratios (16:9, 21:9, 4:3)
- [ ] Controller support (Xbox, PlayStation, generic)
- [ ] Keyboard + mouse

---

## Common Bug Categories

### Crash/Stability
- Null reference exceptions
- Index out of bounds
- Stack overflow
- Out of memory
- Infinite loops

### Gameplay
- Incorrect values or calculations
- State machine stuck
- AI not responding
- Physics glitches
- Collision issues

### UI/UX
- Elements not updating
- Incorrect layout
- Text overflow
- Input not registering
- Menu navigation broken

### Audio
- Sounds not playing
- Audio clipping
- Volume issues
- Spatial audio incorrect
- Music not transitioning

### Visual
- Particles not spawning
- VFX incorrect colors
- Post-processing issues
- Lighting glitches
- Animation bugs

### Save/Load
- Data not saving
- Corruption on load
- Missing data after load
- Backwards compatibility issues

### Performance
- FPS drops
- Memory leaks
- CPU/GPU spikes
- Long load times
- Stuttering

---

## Known Issues

### Phase 5 Limitations (Not Bugs)
These are known limitations documented in Phase 5:

1. **Audio Assets Missing**: Framework complete, actual audio files need creation
2. **VFX Assets Missing**: Framework complete, particle prefabs need creation
3. **Colorblind Testing**: Needs testing with actual colorblind players
4. **Screen Reader**: Placeholder only (full implementation needs platform APIs)

---

## Testing Resources

### Tools
- Unity Profiler (CPU, GPU, Memory)
- Unity Test Framework (automated tests)
- Frame Debugger (rendering issues)
- Console (errors and warnings)

### Test Builds
- Development: Full debugging, console access
- Release: Optimized, no debug UI
- Platform: Windows, Mac, Linux builds

### Test Saves
- Fresh save (new game)
- Mid-game save (some progress)
- Late-game save (most content unlocked)
- Edge case saves (max values, empty inventory, etc.)

---

## Bug Fixing Priority

### Priority Order
1. **Critical**: Fix immediately, block release
2. **High**: Fix before launch, may delay if needed
3. **Medium**: Fix if time permits, can patch post-launch
4. **Low**: Polish pass, can defer to post-launch updates

### Fix Verification
For each fixed bug:
1. Verify fix in development build
2. Test that fix doesn't break other systems
3. Add regression test (if applicable)
4. Test in release build
5. Mark as fixed with notes
6. Update statistics

---

## Contact

**Bug Reports**: Submit via GitHub Issues or directly to development team
**Questions**: Contact project lead

---

*Keep this document updated as bugs are found and fixed. Good testing leads to a polished release!*

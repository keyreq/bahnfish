# Fishing System - Integration Guide for Other Agents

## Quick Start

The fishing system is **ready to integrate** with your agent's systems. All core mechanics are complete and functional.

## For Agent 6: Inventory System

### What You Need to Do

Subscribe to the `FishCaught` event and add fish to inventory:

```csharp
private void Start()
{
    EventSystem.Subscribe<Fish>("FishCaught", OnFishCaught);
}

private void OnDestroy()
{
    EventSystem.Unsubscribe<Fish>("FishCaught", OnFishCaught);
}

private void OnFishCaught(Fish fish)
{
    // Add fish to your grid inventory
    bool added = inventoryManager.AddItem(fish);

    if (!added)
    {
        Debug.LogWarning("Inventory full! Fish escaped.");
        // Could auto-sell or drop fish
    }
}
```

### Passive Tool Integration

When player retrieves crab pots or drift nets:

```csharp
// Crab Pot
CrabPot pot = playerController.GetCurrentTool() as CrabPot;
if (pot != null && pot.IsDeployed())
{
    List<Fish> catches = pot.CollectCatch();
    foreach (Fish fish in catches)
    {
        inventoryManager.AddItem(fish);
    }
}

// Drift Net (same pattern)
DriftNet net = playerController.GetCurrentTool() as DriftNet;
if (net != null)
{
    List<Fish> catches = net.CollectCatch();
    foreach (Fish fish in catches)
    {
        inventoryManager.AddItem(fish);
    }
}
```

### Fish Inventory Properties

Fish already implements the grid system you need:

```csharp
public class Fish
{
    public Vector2Int inventorySize;  // Grid size (e.g., 2x1, 3x2)
    public Sprite icon;               // Visual for inventory
    public string name;               // Display name
    public float baseValue;           // Sell price
    // ... other properties
}
```

---

## For Agent 8: Fish AI & Behavior

### What You Need to Do

Replace the mock fish spawning with your real system:

#### 1. Listen for Spawn Requests

```csharp
private void Start()
{
    EventSystem.Subscribe("RequestFishSpawn", OnFishSpawnRequested);
}

private void OnFishSpawnRequested()
{
    // Get context from FishingController
    FishingController fc = FindObjectOfType<FishingController>();
    if (fc == null) return;

    // Get location, time, weather
    string locationID = // Get from current fishing zone
    TimeOfDay timeOfDay = TimeManager.Instance.GetCurrentTime();
    WeatherType weather = WeatherSystem.Instance.GetCurrentWeather();

    // Spawn appropriate fish
    Fish spawnedFish = SpawnFishForConditions(locationID, timeOfDay, weather);

    // Simulate bite after delay
    float biteDelay = Random.Range(2f, 8f);
    StartCoroutine(SimulateBite(spawnedFish, biteDelay));
}

private IEnumerator SimulateBite(Fish fish, float delay)
{
    yield return new WaitForSeconds(delay);
    EventSystem.Publish("OnFishBite", fish);
}
```

#### 2. Provide Fish Based on Conditions

```csharp
private Fish SpawnFishForConditions(string locationID, TimeOfDay time, WeatherType weather)
{
    // Your fish spawning logic
    // Consider:
    // - Location-specific species
    // - Time of day (night fish vs day fish)
    // - Weather (rain attracts certain species)
    // - Player's current tool capabilities

    return fishDatabase.GetRandomFish(locationID, time, weather);
}
```

#### 3. Fish Behavior During Combat

The TensionSystem already handles fish fighting behavior, but you can customize:

```csharp
// In your Fish data
public class Fish
{
    public float aggressiveness = 0.5f;  // 0-1, how often fish fights
    public float strength = 1f;          // Multiplier for resistance
    public bool hasErraticMovement = false;  // For aberrant fish
}
```

TensionSystem will use these properties automatically.

---

## For Agent 11: UI/UX System

### What You Need to Display

#### 1. Tension Meter (CRITICAL!)

```csharp
private void Start()
{
    EventSystem.Subscribe<TensionUpdateData>("OnTensionUpdated", UpdateTensionUI);
}

private void UpdateTensionUI(TensionUpdateData data)
{
    // Update tension bar
    tensionSlider.value = data.tensionPercentage / 100f;

    // Color code based on danger
    if (data.isDanger)
        tensionSlider.fillRect.color = Color.red;
    else if (data.tensionPercentage > 70f)
        tensionSlider.fillRect.color = Color.yellow;
    else
        tensionSlider.fillRect.color = Color.green;

    // Show warning if fish is fighting
    if (data.fishIsFighting)
        fishFightingIndicator.SetActive(true);
    else
        fishFightingIndicator.SetActive(false);

    // Update progress bar
    progressSlider.value = data.catchProgress / 100f;
}
```

#### 2. Fishing State Indicators

```csharp
private void Start()
{
    EventSystem.Subscribe("OnFishingStarted", ShowFishingUI);
    EventSystem.Subscribe("OnFishingEnded", HideFishingUI);
    EventSystem.Subscribe<Fish>("FishCaught", ShowCatchNotification);
    EventSystem.Subscribe("LineBroken", ShowLineBrokenNotification);
}

private void ShowFishingUI()
{
    fishingPanel.SetActive(true);
    controlsHint.SetActive(true);
}

private void HideFishingUI()
{
    fishingPanel.SetActive(false);
    controlsHint.SetActive(false);
}

private void ShowCatchNotification(Fish fish)
{
    // Show "Caught!" popup with fish name/icon
    catchNotification.Show(fish.name, fish.icon, fish.rarity);
}

private void ShowLineBrokenNotification()
{
    // Show "Line Broke!" failure message
    failureNotification.Show("Line broke! The fish got away.");
}
```

#### 3. Minigame-Specific UI

**ReelMinigame**: Already has debug UI, replace with proper UI elements

**HarpoonMinigame**: Needs:
- Crosshair cursor
- Moving fish target
- Charge meter
- Timer

**DredgeMinigame**: Needs:
- Crane sprite
- Obstacles
- Target indicator
- Cable visualization

#### 4. Tool Status Display

```csharp
// Show current tool info
private void UpdateToolDisplay(BaseFishingTool tool)
{
    if (tool is FishingRod rod)
    {
        toolName.text = rod.toolName;
        durabilityBar.value = rod.GetDurabilityPercentage() / 100f;
        upgradeLevel.text = $"Level {rod.GetUpgradeLevel()}";
    }
    else if (tool is Harpoon harpoon)
    {
        toolName.text = "Harpoon";
        ammoCount.text = $"{harpoon.GetCurrentAmmo()}/{harpoon.GetMaxAmmo()}";
    }
    else if (tool is CrabPot pot)
    {
        toolName.text = "Crab Pot";
        if (pot.IsDeployed())
            status.text = $"Deployed - {pot.GetCatchCount()} fish";
        else
            status.text = "Not Deployed";
    }
    // etc.
}
```

#### 5. Passive Tool Notifications

```csharp
EventSystem.Subscribe<CrabPot>("OnCrabPotRetrieved", (pot) => {
    ShowNotification($"Retrieved crab pot with {pot.GetCatchCount()} fish!");
});

EventSystem.Subscribe<DriftNet>("OnDriftNetRetrieved", (net) => {
    ShowNotification($"Collected drift net with {net.GetCatchCount()} fish!");
});
```

### Required UI Elements

**Main Fishing HUD**:
- Tension bar (horizontal, color-coded)
- Progress bar (catch progress)
- Fish name/rarity
- Control hints ("SPACE: Reel" etc.)
- Timer (for minigames)
- Fish fighting indicator (pulsing "FIGHTING!" text)

**Tool Display** (bottom-right):
- Tool icon
- Durability bar / ammo count
- Upgrade level

**Notifications**:
- Catch success
- Line broken
- Tool broken
- Crab pot full
- Drift net full

---

## For Agent 12: Audio System

### Sound Effects Needed

```csharp
// Fishing actions
EventSystem.Subscribe("OnFishingStarted", () => PlaySFX("cast_line"));
EventSystem.Subscribe<Fish>("OnFishBite", (fish) => PlaySFX("fish_bite"));
EventSystem.Subscribe<Fish>("FishCaught", (fish) => PlaySFX("fish_caught"));
EventSystem.Subscribe("LineBroken", () => PlaySFX("line_snap"));

// Tension feedback
EventSystem.Subscribe<TensionUpdateData>("OnTensionUpdated", (data) => {
    if (data.isDanger && !tensionWarningPlaying)
    {
        PlayLoopingSFX("tension_warning");
        tensionWarningPlaying = true;
    }
    else if (!data.isDanger && tensionWarningPlaying)
    {
        StopLoopingSFX("tension_warning");
        tensionWarningPlaying = false;
    }
});

// Fish fighting
EventSystem.Subscribe<TensionUpdateData>("OnTensionUpdated", (data) => {
    if (data.fishIsFighting && !fightSoundPlaying)
    {
        PlaySFX("fish_fight");
        fightSoundPlaying = true;
    }
});

// Tools
EventSystem.Subscribe<CrabPot>("OnCrabPotDeployed", () => PlaySFX("deploy_pot"));
EventSystem.Subscribe<DriftNet>("OnDriftNetDeployed", () => PlaySFX("deploy_net"));
```

### Recommended Sounds

**Actions**:
- `cast_line` - Whoosh + splash
- `reel_in` - Mechanical clicking (loop)
- `let_out_line` - Unreeling sound (loop)

**Events**:
- `fish_bite` - Sharp tug + splash
- `fish_caught` - Success jingle + splash
- `line_snap` - Snap + failure sound
- `fish_fight` - Splashing + tension

**Tools**:
- `deploy_pot` - Splash + clunk
- `deploy_net` - Cloth unfurling
- `harpoon_throw` - Whoosh + impact

**UI**:
- `tension_warning` - Looping high-pitched pulse
- `progress_tick` - Subtle click as progress increases

---

## For Agent 13: Visual Effects

### Particle Effects Needed

```csharp
EventSystem.Subscribe("OnFishingStarted", () => {
    // Spawn splash where line enters water
    SpawnVFX("water_splash", castPosition);
});

EventSystem.Subscribe<Fish>("OnFishBite", (fish) => {
    // Spawn underwater disturbance
    SpawnVFX("fish_bite_ripple", castPosition);
});

EventSystem.Subscribe<Fish>("FishCaught", (fish) => {
    // Big splash + celebration particles
    SpawnVFX("catch_success", castPosition);
});

EventSystem.Subscribe("LineBroken", () => {
    // Line snapping effect
    SpawnVFX("line_break", castPosition);
});

EventSystem.Subscribe<TensionUpdateData>("OnTensionUpdated", (data) => {
    // Update line tension visual (line vibration)
    UpdateLineTension(data.tensionPercentage);
});
```

### Visual Effects Needed

**Fishing**:
- Water splash (cast entry)
- Bobber floating animation
- Ripples (fish movement)
- Line tension (line vibrates/bends)

**Success/Failure**:
- Big splash (fish caught)
- Celebration particles (stars, sparkles)
- Line snap (break effect)

**Tools**:
- Crab pot buoy (floating marker)
- Drift net trail (visible net in water)
- Harpoon trail (projectile motion)

**Minigames**:
- Dredge crane cable
- Harpoon crosshair glow
- Obstacle warning indicators

---

## Common Integration Patterns

### Pattern 1: Subscribe to Events

```csharp
private void Start()
{
    EventSystem.Subscribe<Fish>("FishCaught", OnFishCaught);
}

private void OnDestroy()
{
    EventSystem.Unsubscribe<Fish>("FishCaught", OnFishCaught);
}

private void OnFishCaught(Fish fish)
{
    // Your logic here
}
```

### Pattern 2: Check Fishing State

```csharp
FishingController fc = FindObjectOfType<FishingController>();
if (fc != null && fc.IsFishing())
{
    // Fishing is active
}
```

### Pattern 3: Get Current Tool

```csharp
FishingController fc = FindObjectOfType<FishingController>();
BaseFishingTool tool = fc.GetCurrentTool() as BaseFishingTool;

if (tool is FishingRod)
    // Rod logic
else if (tool is Harpoon)
    // Harpoon logic
```

---

## Event Reference Quick Lookup

| Event | Data | Use Case |
|-------|------|----------|
| `OnFishingStarted` | none | Show fishing UI |
| `OnFishingEnded` | none | Hide fishing UI |
| `FishCaught` | Fish | Add to inventory, show notification |
| `LineBroken` | none | Show failure message, play SFX |
| `OnTensionUpdated` | TensionUpdateData | Update tension meter, play warning |
| `OnCrabPotDeployed` | CrabPot | Show deployment confirmation |
| `OnCrabPotRetrieved` | CrabPot | Collect fish, show count |
| `OnDriftNetDeployed` | DriftNet | Show deployment, apply speed penalty |
| `OnDriftNetRetrieved` | DriftNet | Collect fish, remove speed penalty |
| `OnToolBroken` | BaseFishingTool | Show repair needed warning |

---

## Testing Your Integration

### Checklist

- [ ] Subscribe to fishing events in Start()
- [ ] Unsubscribe in OnDestroy()
- [ ] Test with all fish rarities (Common → Legendary → Aberrant)
- [ ] Test all four tools (Rod, Harpoon, CrabPot, DriftNet)
- [ ] Test success and failure cases
- [ ] Test edge cases (full inventory, no ammo, broken tool)
- [ ] Verify no memory leaks (unsubscribe works)
- [ ] Test performance (should be <1ms impact)

### Debug Commands

```csharp
// Force spawn specific fish
Fish testFish = new Fish {
    name = "Test Bass",
    rarity = FishRarity.Legendary
};
EventSystem.Publish("OnFishBite", testFish);

// Force line break
EventSystem.Publish("LineBroken");

// Force successful catch
EventSystem.Publish("OnFishCaught", testFish);
```

---

## Questions?

**Q: When do I need to integrate?**
A: Fishing system works standalone with mock data. Integrate when your system is ready.

**Q: Will fishing system break if I don't integrate?**
A: No! It uses fallbacks and mocks for missing systems.

**Q: Can I test fishing before integration?**
A: Yes! Just equip a FishingRod and press E in a FishingZone.

**Q: How do I create a FishingZone?**
A: Add `FishingZone` component to a trigger collider in water.

**Q: Where are the controls?**
A: E (start), SPACE (reel), SHIFT (let out), ESC (cancel)

---

## Support

For integration issues:
- Read `/Scripts/Fishing/README.md` for full documentation
- Check `/Scripts/Fishing/AGENT_5_DELIVERABLES.md` for implementation details
- Review existing event subscriptions in the core files
- Test with debug UI enabled (`Debug.isDebugBuild`)

**Integration should be straightforward - all hooks are ready!**

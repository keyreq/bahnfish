using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Example integration code showing how other agents should interact with the Horror System.
/// This demonstrates best practices for subscribing to events and using the Horror API.
/// Agent 7: Sanity & Horror System
/// </summary>
public class HorrorSystemIntegrationExample : MonoBehaviour
{
    [Header("Example: Player/Boat Agent Integration")]
    [SerializeField] private float currentHealth = 100f;
    [SerializeField] private bool controlsEnabled = true;

    [Header("Example: Inventory Agent Integration")]
    [SerializeField] private List<string> fishInventory = new List<string>();

    [Header("Example: UI Agent Integration")]
    [SerializeField] private float displayedSanity = 100f;
    [SerializeField] private string statusMessage = "";

    private void Start()
    {
        SubscribeToHorrorEvents();
        InitializeExampleInventory();
    }

    /// <summary>
    /// Example: Subscribe to all relevant Horror System events
    /// </summary>
    private void SubscribeToHorrorEvents()
    {
        // ==== SANITY EVENTS ====
        EventSystem.Subscribe<float>("SanityChanged", OnSanityChanged);
        EventSystem.Subscribe("InsanityTriggered", OnInsanityTriggered);
        EventSystem.Subscribe("SanityFullyRestored", OnSanityFullyRestored);

        // ==== DAMAGE EVENTS ====
        EventSystem.Subscribe<float>("BoatDamaged", OnBoatDamaged);

        // ==== HAZARD SPAWN EVENTS ====
        EventSystem.Subscribe<string>("FishThiefSpawned", OnFishThiefSpawned);
        EventSystem.Subscribe<Vector3>("ChaseCreatureSpawned", OnChaseCreatureSpawned);
        EventSystem.Subscribe<Vector3>("ObstacleSpawned", OnObstacleSpawned);

        // ==== FISH STEALING EVENTS (CRITICAL FOR INVENTORY AGENT) ====
        EventSystem.Subscribe<int>("RemoveRandomFishFromInventory", OnRemoveRandomFish);
        EventSystem.Subscribe<float>("GhostShipStealCatch", OnGhostShipStealCatch);

        // ==== CURSE EVENTS ====
        EventSystem.Subscribe<CurseType>("CurseApplied", OnCurseApplied);
        EventSystem.Subscribe<CurseType>("CurseRemoved", OnCurseRemoved);

        // ==== CURSE EFFECT EVENTS (MODIFY GAMEPLAY) ====
        EventSystem.Subscribe<float>("FishSpoilRateModifier", OnFishSpoilRateModifier);
        EventSystem.Subscribe<float>("FishingSpeedModifier", OnFishingSpeedModifier);
        EventSystem.Subscribe<float>("CompassDistorted", OnCompassDistorted);
        EventSystem.Subscribe("HauntedGlitch", OnHauntedGlitch);

        // ==== HALLUCINATION EVENTS ====
        EventSystem.Subscribe<Vector3>("FalseFishDetected", OnFalseFishDetected);
        EventSystem.Subscribe<float>("EquipmentMalfunction", OnEquipmentMalfunction);

        Debug.Log("[Integration Example] Subscribed to all Horror System events");
    }

    // ===================================================================
    // EXAMPLE: SANITY EVENT HANDLERS
    // ===================================================================

    private void OnSanityChanged(float newSanity)
    {
        displayedSanity = newSanity;

        // Example: Update UI meter
        Debug.Log($"[UI Example] Sanity: {newSanity:F1}%");

        // Example: Change background music intensity based on sanity
        float intensity = 1f - (newSanity / 100f);
        // AudioManager.SetMusicIntensity(intensity);

        // Example: Show warning at low sanity
        if (newSanity < 30f)
        {
            statusMessage = "WARNING: Sanity critically low!";
            // UIManager.ShowWarning("Low Sanity!");
        }
        else if (newSanity < 50f)
        {
            statusMessage = "Feeling uneasy...";
        }
        else
        {
            statusMessage = "Feeling normal";
        }
    }

    private void OnInsanityTriggered()
    {
        Debug.LogWarning("[Integration Example] INSANITY! Player at 0 sanity!");

        // Example: Trigger dramatic effect
        // CameraController.ShakeDramatically(2f);
        // AudioManager.PlayStinger("insanity_stinger");
        // UIManager.ShowFullscreenEffect("InsanityEffect");

        statusMessage = "LOSING YOUR MIND!";
    }

    private void OnSanityFullyRestored()
    {
        Debug.Log("[Integration Example] Sanity fully restored!");

        statusMessage = "Feeling refreshed";
        // UIManager.ShowNotification("Sanity Restored!");
    }

    // ===================================================================
    // EXAMPLE: DAMAGE EVENT HANDLERS (PLAYER/BOAT AGENT)
    // ===================================================================

    private void OnBoatDamaged(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(0f, currentHealth);

        Debug.LogWarning($"[Player Example] Boat damaged! -{damage} HP. Current: {currentHealth}");

        // Example: Update health UI
        // UIManager.UpdateHealthBar(currentHealth);

        // Example: Visual feedback
        // CameraController.Shake(0.3f);
        // AudioManager.PlaySound("boat_hit");

        // Example: Check for death
        if (currentHealth <= 0f)
        {
            OnBoatDestroyed();
        }
    }

    private void OnBoatDestroyed()
    {
        Debug.LogError("[Player Example] BOAT DESTROYED! Game Over!");
        controlsEnabled = false;

        // Example: Game over sequence
        // GameManager.TriggerGameOver("Boat destroyed");
        // UIManager.ShowGameOverScreen();
    }

    // ===================================================================
    // EXAMPLE: HAZARD SPAWN HANDLERS (AUDIO/UI AGENT)
    // ===================================================================

    private void OnFishThiefSpawned(string thiefType)
    {
        Debug.Log($"[Audio Example] {thiefType} spawned! Playing warning sound.");

        // Example: Play warning sound
        // AudioManager.PlaySound($"{thiefType.ToLower()}_spawn");

        // Example: Show UI warning
        // UIManager.ShowWarning($"{thiefType} approaching!");
    }

    private void OnChaseCreatureSpawned(Vector3 position)
    {
        Debug.LogWarning("[Integration Example] CHASE CREATURE SPAWNED! RUN!");

        // Example: Disable fishing during chase
        // FishingController.DisableFishing();

        // Example: Start chase music
        // AudioManager.TransitionToChaseMusic();

        // Example: Camera effect
        // CameraController.StartChaseEffects();

        // Example: UI alert
        // UIManager.ShowFullscreenAlert("CREATURE APPROACHING!");

        controlsEnabled = false; // Example: Temporarily disable some controls
        Invoke(nameof(RestoreControls), 1f); // Re-enable after 1 second
    }

    private void RestoreControls()
    {
        controlsEnabled = true;
    }

    private void OnObstacleSpawned(Vector3 position)
    {
        Debug.Log($"[Integration Example] Obstacle spawned at {position}");

        // Example: Add to minimap
        // MinimapController.AddHazardMarker(position, "obstacle");

        // Example: Play ambient sound
        // AudioManager.Play3DSound("obstacle_appear", position);
    }

    // ===================================================================
    // EXAMPLE: FISH STEALING HANDLERS (INVENTORY AGENT - CRITICAL!)
    // ===================================================================

    private void OnRemoveRandomFish(int count)
    {
        Debug.LogWarning($"[Inventory Example] Fish Thief stealing {count} fish!");

        // CRITICAL: Inventory Agent MUST implement this
        for (int i = 0; i < count && fishInventory.Count > 0; i++)
        {
            int randomIndex = Random.Range(0, fishInventory.Count);
            string stolenFish = fishInventory[randomIndex];
            fishInventory.RemoveAt(randomIndex);

            Debug.LogWarning($"[Inventory Example] {stolenFish} STOLEN!");

            // Example: Show UI notification
            // UIManager.ShowNotification($"{stolenFish} stolen!");

            // Example: Play steal sound
            // AudioManager.PlaySound("fish_stolen");
        }
    }

    private void OnGhostShipStealCatch(float percentage)
    {
        Debug.LogWarning($"[Inventory Example] Ghost Ship stealing {percentage * 100}% of catch!");

        // CRITICAL: Inventory Agent MUST implement this
        int fishToSteal = Mathf.FloorToInt(fishInventory.Count * percentage);

        for (int i = 0; i < fishToSteal && fishInventory.Count > 0; i++)
        {
            int randomIndex = Random.Range(0, fishInventory.Count);
            string stolenFish = fishInventory[randomIndex];
            fishInventory.RemoveAt(randomIndex);

            Debug.LogWarning($"[Inventory Example] {stolenFish} STOLEN BY GHOST SHIP!");
        }

        // Example: Show dramatic UI effect
        // UIManager.ShowFullscreenEffect("GhostShipSteal");
        // AudioManager.PlaySound("ghost_laugh");
    }

    // ===================================================================
    // EXAMPLE: CURSE HANDLERS (UI/GAMEPLAY AGENTS)
    // ===================================================================

    private void OnCurseApplied(CurseType curseType)
    {
        Debug.LogWarning($"[Integration Example] CURSE APPLIED: {curseType}!");

        // Example: Show curse notification
        string curseDescription = GetCurseDescription(curseType);
        // UIManager.ShowCurseNotification(curseType, curseDescription);

        // Example: Update curse display
        // UIManager.AddCurseIcon(curseType);

        // Example: Play curse sound
        // AudioManager.PlaySound("curse_applied");

        statusMessage = $"Cursed with {curseType}!";
    }

    private void OnCurseRemoved(CurseType curseType)
    {
        Debug.Log($"[Integration Example] Curse removed: {curseType}");

        // Example: Update UI
        // UIManager.RemoveCurseIcon(curseType);
        // UIManager.ShowNotification($"{curseType} curse cleansed!");

        statusMessage = $"{curseType} curse removed";
    }

    // ===================================================================
    // EXAMPLE: CURSE EFFECT HANDLERS (GAMEPLAY MODIFICATION)
    // ===================================================================

    private void OnFishSpoilRateModifier(float multiplier)
    {
        Debug.Log($"[Inventory Example] Fish spoil rate: {multiplier}x");

        // IMPORTANT: Inventory Agent should apply this multiplier
        // inventorySystem.fishSpoilRate *= multiplier;
    }

    private void OnFishingSpeedModifier(float multiplier)
    {
        Debug.Log($"[Fishing Example] Fishing speed: {multiplier}x");

        // IMPORTANT: Fishing Agent should apply this multiplier
        // fishingController.reelSpeed *= multiplier;
    }

    private void OnCompassDistorted(float angle)
    {
        Debug.Log($"[Navigation Example] Compass distorted by {angle} degrees");

        // IMPORTANT: Navigation/UI Agent should apply compass rotation
        // compassUI.AddDistortion(angle);
    }

    private void OnHauntedGlitch()
    {
        Debug.Log("[Visual Example] Haunted glitch effect!");

        // Example: Random visual glitch
        // UIManager.TriggerGlitchEffect(0.2f);
        // AudioManager.PlaySound("static_burst");

        // Example: Random screen effects
        int glitchType = Random.Range(0, 3);
        switch (glitchType)
        {
            case 0:
                // VisualDistortion.TriggerScreenShake(0.5f, 0.2f);
                break;
            case 1:
                // UIManager.FlashScreen(Color.red, 0.1f);
                break;
            case 2:
                // AudioManager.PlayReversedSound("ambient");
                break;
        }
    }

    // ===================================================================
    // EXAMPLE: HALLUCINATION HANDLERS
    // ===================================================================

    private void OnFalseFishDetected(Vector3 position)
    {
        Debug.Log($"[Fishing Example] FALSE FISH SIGNAL at {position}");

        // Example: Show fake fish on sonar/minimap
        // MinimapController.ShowFakeFish(position, 2f); // Disappears after 2 sec

        // Example: Play fish detection sound
        // AudioManager.PlaySound("fish_detected");

        // IMPORTANT: Don't actually spawn a fish - it's a hallucination!
    }

    private void OnEquipmentMalfunction(float duration)
    {
        Debug.LogWarning($"[Fishing Example] EQUIPMENT MALFUNCTION for {duration} seconds!");

        // Example: Disable fishing temporarily
        // FishingController.DisableFishing(duration);

        // Example: Visual effect on fishing rod
        // FishingRodVisual.ShowMalfunction(duration);

        // Example: UI feedback
        // UIManager.ShowWarning($"Equipment malfunction! {duration}s");
    }

    // ===================================================================
    // EXAMPLE: DIRECT API USAGE
    // ===================================================================

    /// <summary>
    /// Example: Check sanity state before allowing actions
    /// </summary>
    private void ExampleCheckSanityBeforeAction()
    {
        if (SanityManager.Instance == null) return;

        float currentSanity = SanityManager.Instance.GetCurrentSanity();

        if (SanityManager.Instance.IsCriticalSanity())
        {
            Debug.LogWarning("[Example] Action blocked - sanity too low!");
            // UIManager.ShowWarning("Too unstable to perform this action!");
            return;
        }

        // Proceed with action
        Debug.Log("[Example] Action performed successfully");
    }

    /// <summary>
    /// Example: Restore sanity when player docks
    /// </summary>
    private void ExampleRestoreSanityAtDock()
    {
        if (SanityManager.Instance == null) return;

        // Full restore at dock
        SanityManager.Instance.FullRestoreSanity();

        Debug.Log("[Example] Docked at port - sanity fully restored!");
        // UIManager.ShowNotification("Sanity restored!");
    }

    /// <summary>
    /// Example: Use talisman item
    /// </summary>
    private void ExampleUseTalisman()
    {
        if (SanityManager.Instance == null) return;

        // Activate talisman for 60 seconds
        SanityManager.Instance.ActivateTalisman(60f);

        Debug.Log("[Example] Talisman activated! 60 seconds of protection.");
        // UIManager.ShowNotification("Talisman activated!");
        // InventorySystem.RemoveItem("talisman");
    }

    /// <summary>
    /// Example: Check for curses and cleanse
    /// </summary>
    private void ExampleCleanseAllCurses()
    {
        if (CurseSystem.Instance == null) return;

        int curseCount = CurseSystem.Instance.GetCurseCount();

        if (curseCount == 0)
        {
            Debug.Log("[Example] No curses to cleanse");
            return;
        }

        float cost = CurseSystem.Instance.GetCleansingCost();

        if (GameManager.Instance.CurrentGameState.money < cost)
        {
            Debug.Log($"[Example] Cannot afford cleansing. Need ${cost}");
            // UIManager.ShowError($"Need ${cost} to cleanse curses");
            return;
        }

        bool success = CurseSystem.Instance.CleanseAllCurses();

        if (success)
        {
            Debug.Log($"[Example] All {curseCount} curses cleansed for ${cost}!");
            // UIManager.ShowNotification($"All curses cleansed!");
        }
    }

    /// <summary>
    /// Example: Display active curses in UI
    /// </summary>
    private void ExampleDisplayActiveCurses()
    {
        if (CurseSystem.Instance == null) return;

        List<CurseType> curses = CurseSystem.Instance.GetActiveCurses();

        if (curses.Count == 0)
        {
            Debug.Log("[Example] No active curses");
            return;
        }

        Debug.Log($"[Example] Active curses ({curses.Count}):");
        foreach (CurseType curse in curses)
        {
            string description = GetCurseDescription(curse);
            Debug.Log($"  - {curse}: {description}");
            // UIManager.AddCurseToList(curse, description);
        }
    }

    /// <summary>
    /// Example: Force spawn hazards for testing
    /// </summary>
    private void ExampleForceSpawnHazards()
    {
        Debug.Log("[Example] Force spawning all hazards for testing");

        if (FishThiefSpawner.Instance != null)
            FishThiefSpawner.Instance.ForceSpawnThief(false);

        if (ObstacleSpawner.Instance != null)
            ObstacleSpawner.Instance.ForceSpawnObstacle();

        if (ChaseCreature.Instance != null)
            ChaseCreature.Instance.ForceSpawn();

        if (VortexSpawner.Instance != null)
            VortexSpawner.Instance.ForceSpawn();

        if (GhostShipSpawner.Instance != null)
            GhostShipSpawner.Instance.ForceSpawn();
    }

    // ===================================================================
    // HELPER METHODS
    // ===================================================================

    private void InitializeExampleInventory()
    {
        // Example inventory for testing
        fishInventory.Add("Bass");
        fishInventory.Add("Trout");
        fishInventory.Add("Salmon");
        fishInventory.Add("Pike");
        fishInventory.Add("Cod");
    }

    private string GetCurseDescription(CurseType curseType)
    {
        switch (curseType)
        {
            case CurseType.RottingCatch:
                return "Fish spoil 2x faster";
            case CurseType.BrokenCompass:
                return "Navigation distorted";
            case CurseType.HauntedHull:
                return "Random glitches occur";
            case CurseType.LeakingBoat:
                return "Constant slow damage";
            case CurseType.TangledNets:
                return "Fishing speed reduced 50%";
            default:
                return "Unknown curse";
        }
    }

    // ===================================================================
    // DEBUG GUI
    // ===================================================================

    private void OnGUI()
    {
        if (!Application.isEditor) return;

        GUILayout.BeginArea(new Rect(10, 10, 400, 600));
        GUILayout.Box("=== HORROR SYSTEM INTEGRATION EXAMPLE ===");

        // Sanity info
        GUILayout.Label($"Sanity: {displayedSanity:F1}%");
        GUILayout.Label($"Status: {statusMessage}");
        GUILayout.Label($"Health: {currentHealth:F0}");
        GUILayout.Label($"Fish Count: {fishInventory.Count}");

        // Control buttons
        if (GUILayout.Button("Set Sanity 100%"))
            SanityManager.Instance?.SetSanity(100f);
        if (GUILayout.Button("Set Sanity 50%"))
            SanityManager.Instance?.SetSanity(50f);
        if (GUILayout.Button("Set Sanity 0%"))
            SanityManager.Instance?.SetSanity(0f);

        GUILayout.Space(10);

        if (GUILayout.Button("Restore Sanity (Dock)"))
            ExampleRestoreSanityAtDock();
        if (GUILayout.Button("Use Talisman"))
            ExampleUseTalisman();

        GUILayout.Space(10);

        if (GUILayout.Button("Force Spawn Fish Thief"))
            FishThiefSpawner.Instance?.ForceSpawnThief(false);
        if (GUILayout.Button("Force Spawn Phantom"))
            FishThiefSpawner.Instance?.ForceSpawnThief(true);
        if (GUILayout.Button("Force Spawn Obstacle"))
            ObstacleSpawner.Instance?.ForceSpawnObstacle();
        if (GUILayout.Button("Force Spawn Chase Creature"))
            ChaseCreature.Instance?.ForceSpawn();

        GUILayout.Space(10);

        if (GUILayout.Button("Apply Random Curse"))
            CurseSystem.Instance?.ApplyCurse((CurseType)Random.Range(0, 5));
        if (GUILayout.Button("Cleanse All Curses"))
            ExampleCleanseAllCurses();
        if (GUILayout.Button("Display Active Curses"))
            ExampleDisplayActiveCurses();

        GUILayout.EndArea();
    }

    private void OnDestroy()
    {
        // IMPORTANT: Unsubscribe from all events to prevent memory leaks
        EventSystem.Unsubscribe<float>("SanityChanged", OnSanityChanged);
        EventSystem.Unsubscribe("InsanityTriggered", OnInsanityTriggered);
        EventSystem.Unsubscribe("SanityFullyRestored", OnSanityFullyRestored);
        EventSystem.Unsubscribe<float>("BoatDamaged", OnBoatDamaged);
        EventSystem.Unsubscribe<string>("FishThiefSpawned", OnFishThiefSpawned);
        EventSystem.Unsubscribe<Vector3>("ChaseCreatureSpawned", OnChaseCreatureSpawned);
        EventSystem.Unsubscribe<Vector3>("ObstacleSpawned", OnObstacleSpawned);
        EventSystem.Unsubscribe<int>("RemoveRandomFishFromInventory", OnRemoveRandomFish);
        EventSystem.Unsubscribe<float>("GhostShipStealCatch", OnGhostShipStealCatch);
        EventSystem.Unsubscribe<CurseType>("CurseApplied", OnCurseApplied);
        EventSystem.Unsubscribe<CurseType>("CurseRemoved", OnCurseRemoved);
        EventSystem.Unsubscribe<float>("FishSpoilRateModifier", OnFishSpoilRateModifier);
        EventSystem.Unsubscribe<float>("FishingSpeedModifier", OnFishingSpeedModifier);
        EventSystem.Unsubscribe<float>("CompassDistorted", OnCompassDistorted);
        EventSystem.Unsubscribe("HauntedGlitch", OnHauntedGlitch);
        EventSystem.Unsubscribe<Vector3>("FalseFishDetected", OnFalseFishDetected);
        EventSystem.Unsubscribe<float>("EquipmentMalfunction", OnEquipmentMalfunction);
    }
}

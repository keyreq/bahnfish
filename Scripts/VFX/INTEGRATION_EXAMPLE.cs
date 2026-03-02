using UnityEngine;

/// <summary>
/// Agent 13: Visual Effects & Particles Specialist - INTEGRATION_EXAMPLE.cs
/// Example integration code showing how to use the VFX system in your gameplay code.
/// This file demonstrates best practices for triggering visual effects.
/// </summary>
public class VFXIntegrationExample : MonoBehaviour
{
    // ===== EXAMPLE 1: PETTING A PET (THE KEY FEATURE!) =====

    /// <summary>
    /// Example: How to trigger the petting hearts effect when player pets a companion.
    /// This is THE signature feature inspired by Cast n Chill!
    /// </summary>
    public void Example_PetACompanion()
    {
        // Your pet data
        string petID = "dog_companion_1";
        string petType = "dog"; // dog, cat, seabird, otter, hermitcrab, ghost
        Vector3 petPosition = transform.position;
        float currentLoyalty = 75f;

        // Trigger the petting hearts effect!
        EventSystem.Publish("PetPetted", new PetPettedData(
            petID: petID,
            petType: petType,
            petPosition: petPosition,
            currentLoyalty: currentLoyalty
        ));

        // The VFX system will automatically:
        // - Spawn 3-5 hearts that float upward
        // - Color hearts based on pet type (red for dog, pink for cat, etc.)
        // - Create sparkle burst around the pet
        // - Show warm glow effect
        // - Display loyalty increase sparkles (if significant gain)

        Debug.Log($"[VFX Example] Petted {petType}! Hearts should appear!");
    }

    // ===== EXAMPLE 2: FISHING SEQUENCE =====

    /// <summary>
    /// Example: Complete fishing sequence with visual effects.
    /// </summary>
    public void Example_FishingSequence()
    {
        // Step 1: Cast fishing line
        Vector3 rodTipPosition = transform.position + Vector3.up * 2f;
        Vector3 targetWaterPosition = transform.position + transform.forward * 20f;
        float castPower = 0.8f;

        EventSystem.Publish("FishingCastStarted", new CastingData(
            startPosition: rodTipPosition,
            targetPosition: targetWaterPosition,
            castPower: castPower
        ));

        // VFX: Line arc animation + splash on impact

        // Step 2: Bobber lands in water
        EventSystem.Publish("FishingBobberLanded", targetWaterPosition);
        // VFX: Impact splash + bobber idle ripples

        // Step 3: Fish gets hooked
        EventSystem.Publish("FishHooked", new FishHookedData(
            fishPosition: targetWaterPosition,
            fishID: "bass_1",
            rarity: FishRarity.Rare
        ));
        // VFX: Bobber submerges + tension sparkles + water disturbance

        // Step 4: Update tension during reeling
        float currentTension = 0.85f; // High tension!
        EventSystem.Publish("FishingTensionChanged", currentTension);
        // VFX: Warning particles at high tension

        // Step 5: Fish jumps
        EventSystem.Publish("FishJumped", new FishJumpData(
            position: targetWaterPosition,
            fishSize: 1.5f,
            airTime: 1.2f,
            rarity: FishRarity.Rare
        ));
        // VFX: Jump splash + water droplets

        // Step 6: Successfully catch fish
        EventSystem.Publish("FishCaught", new FishCaughtData(
            fishID: "bass_1",
            rarity: FishRarity.Rare,
            weight: 3.5f,
            isAberrant: false
        ));
        // VFX: Blue aura burst (Rare fish)

        Debug.Log("[VFX Example] Complete fishing sequence triggered!");
    }

    // ===== EXAMPLE 3: WATER EFFECTS =====

    /// <summary>
    /// Example: Creating water splashes and ripples manually.
    /// </summary>
    public void Example_WaterEffects()
    {
        WaterEffects waterEffects = VFXManager.Instance.GetWaterEffects();

        // Create a splash
        Vector3 splashPosition = transform.position;
        waterEffects.CreateSplash(splashPosition, SplashSize.Medium, intensity: 1f);

        // Create a ripple
        waterEffects.CreateRipple(splashPosition, intensity: 0.5f);

        // Create underwater bubbles
        waterEffects.CreateBubbles(splashPosition, count: 10);

        // Create boat wake (automatically done when PlayerMoved event fires)
        // But you can manually create it too:
        float boatSpeed = 15f;
        waterEffects.CreateBoatSplash(splashPosition, boatSpeed);

        Debug.Log("[VFX Example] Water effects created!");
    }

    // ===== EXAMPLE 4: SANITY EFFECTS =====

    /// <summary>
    /// Example: Triggering horror sanity effects.
    /// </summary>
    public void Example_SanityEffects()
    {
        // Update sanity (triggers automatic visual distortion)
        float newSanity = 25f; // Low sanity!

        EventSystem.Publish("SanityChanged", newSanity);

        // VFX system automatically applies:
        // - Heavy vignette
        // - Chromatic aberration
        // - Screen shake
        // - Hallucination particles (shadows, eyes)
        // - Color desaturation

        Debug.Log($"[VFX Example] Sanity changed to {newSanity}%. Horror effects applied!");
    }

    /// <summary>
    /// Example: Spawning a night hazard with effects.
    /// </summary>
    public void Example_NightHazard()
    {
        // Spawn fish thief hazard
        Vector3 hazardPosition = transform.position + transform.forward * 10f;

        EventSystem.Publish("NightHazardSpawned", new HazardSpawnData(
            hazardType: "FishThief",
            position: hazardPosition,
            hazardObject: null
        ));

        // VFX: Dark mist trail + feathers + ghostly effect

        Debug.Log("[VFX Example] Fish thief hazard spawned with effects!");
    }

    // ===== EXAMPLE 5: DYNAMIC EVENTS =====

    /// <summary>
    /// Example: Starting a Blood Moon event.
    /// </summary>
    public void Example_BloodMoonEvent()
    {
        // Start Blood Moon
        EventSystem.Publish("DynamicEventStarted", "BloodMoon");

        // VFX system automatically:
        // - Tints sky red
        // - Spawns blood moon object
        // - Creates red mist particles
        // - Adjusts post-processing (red filter)
        // - Increases fog density (red color)

        Debug.Log("[VFX Example] Blood Moon event started!");

        // Later, end the event:
        // EventSystem.Publish("DynamicEventEnded", "BloodMoon");
    }

    /// <summary>
    /// Example: Starting a meteor shower.
    /// </summary>
    public void Example_MeteorShower()
    {
        EventSystem.Publish("DynamicEventStarted", "MeteorShower");

        // VFX: Meteors fall from sky with trails and impact splashes

        Debug.Log("[VFX Example] Meteor shower started!");
    }

    // ===== EXAMPLE 6: WEATHER CHANGES =====

    /// <summary>
    /// Example: Changing weather to create atmospheric effects.
    /// </summary>
    public void Example_WeatherChange()
    {
        // Change to storm
        EventSystem.Publish("WeatherChanged", WeatherType.Storm);

        // VFX: Heavy rain + lightning + wind + increased fog

        Debug.Log("[VFX Example] Weather changed to storm!");

        // Other weather types:
        // EventSystem.Publish("WeatherChanged", WeatherType.Clear);
        // EventSystem.Publish("WeatherChanged", WeatherType.Rain);
        // EventSystem.Publish("WeatherChanged", WeatherType.Fog);
    }

    // ===== EXAMPLE 7: INVENTORY VFX =====

    /// <summary>
    /// Example: Item interactions with visual feedback.
    /// </summary>
    public void Example_InventoryEffects()
    {
        InventoryVFX inventoryVFX = VFXManager.Instance.GetInventoryVFX();

        // Item pickup
        Vector3 itemPosition = transform.position;
        inventoryVFX.CreateItemPickupEffect(itemPosition, FishRarity.Legendary);
        // VFX: Gold glow (legendary)

        // Valid placement feedback
        inventoryVFX.CreatePlacementFeedback(itemPosition, isValid: true);
        // VFX: Green highlight

        // Sell item
        inventoryVFX.CreateSellEffect(itemPosition, value: 250f);
        // VFX: Coin particles

        // Craft item
        inventoryVFX.CreateCraftEffect(itemPosition, itemID: "upgraded_rod");
        // VFX: Crafting sparkles + success burst

        Debug.Log("[VFX Example] Inventory effects demonstrated!");
    }

    // ===== EXAMPLE 8: UI EFFECTS =====

    /// <summary>
    /// Example: UI visual effects for achievements and notifications.
    /// </summary>
    public void Example_UIEffects()
    {
        UIParticleEffects uiVFX = VFXManager.Instance.GetUIParticleEffects();

        // Achievement unlock
        Vector3 screenCenter = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2f, Screen.height / 2f, 10f));
        uiVFX.CreateAchievementEffect(screenCenter);
        // VFX: Fanfare burst + stars

        // Notification
        uiVFX.CreateNotificationEffect(screenCenter, NotificationType.Warning);
        // VFX: Warning pulse

        // Level up
        uiVFX.CreateLevelUpEffect(screenCenter, newLevel: 5);
        // VFX: Radial burst + number particles

        Debug.Log("[VFX Example] UI effects demonstrated!");
    }

    // ===== EXAMPLE 9: QUALITY SETTINGS =====

    /// <summary>
    /// Example: Adjusting VFX quality for performance.
    /// </summary>
    public void Example_QualitySettings()
    {
        VFXManager vfxManager = VFXManager.Instance;

        // Set quality level
        vfxManager.SetQuality(VFXQuality.High);

        // Get current quality
        VFXQuality currentQuality = vfxManager.GetQuality();
        Debug.Log($"[VFX Example] Current quality: {currentQuality}");

        // Get particle density
        float density = vfxManager.GetParticleDensity();
        Debug.Log($"[VFX Example] Particle density: {density}");

        // Enable/disable particles
        vfxManager.SetParticlesEnabled(true);

        // Enable/disable post-processing
        vfxManager.SetPostProcessingEnabled(true);

        // Get performance stats
        string stats = vfxManager.GetPerformanceStats();
        Debug.Log($"[VFX Example] Performance: {stats}");
    }

    // ===== EXAMPLE 10: POST-PROCESSING =====

    /// <summary>
    /// Example: Manual post-processing control for special effects.
    /// </summary>
    public void Example_PostProcessing()
    {
        PostProcessingManager postProcessing = VFXManager.Instance.GetPostProcessingManager();

        // Vignette for focus
        postProcessing.SetVignetteIntensity(0.4f);

        // Chromatic aberration for distortion
        postProcessing.SetChromaticAberration(0.2f);

        // Color saturation (0 = black & white, 1 = normal, 2 = oversaturated)
        postProcessing.SetColorSaturation(0.8f);

        // Bloom intensity
        postProcessing.SetBloomIntensity(0.6f);

        // Exposure
        postProcessing.SetExposure(0.1f);

        // Color filter tint
        postProcessing.SetColorFilter(new Color(1f, 0.9f, 0.8f)); // Warm tint

        // Flash screen (camera flash effect)
        postProcessing.FlashScreen(Color.white, duration: 0.2f);

        // Damage effect (red vignette pulse)
        postProcessing.ApplyDamageEffect();

        // Depth of field (photography mode)
        postProcessing.SetDepthOfField(enabled: true, focusDistance: 10f, aperture: 2.8f);

        Debug.Log("[VFX Example] Post-processing effects applied!");
    }

    // ===== EXAMPLE 11: FISH VISUALS =====

    /// <summary>
    /// Example: Creating visual effects for fish AI.
    /// </summary>
    public void Example_FishVisuals()
    {
        FishAIVisuals fishVisuals = VFXManager.Instance.GetFishAIVisuals();

        GameObject fishObject = gameObject; // Your fish GameObject

        // Create swimming trail (for bioluminescent fish)
        ParticleSystem trail = fishVisuals.CreateFishTrail(fishObject, isBioluminescent: true);

        // Create rarity aura
        ParticleSystem aura = fishVisuals.CreateFishAura(fishObject, FishRarity.Legendary, isAberrant: false);
        // VFX: Gold legendary aura

        // School shimmer
        Vector3 schoolCenter = transform.position;
        int fishCount = 12;
        fishVisuals.CreateSchoolShimmer(schoolCenter, fishCount);

        Debug.Log("[VFX Example] Fish visual effects created!");
    }

    // ===== EXAMPLE 12: SAVE/LOAD =====

    /// <summary>
    /// Example: Saving and loading VFX settings.
    /// </summary>
    public void Example_SaveLoadVFX()
    {
        VFXManager vfxManager = VFXManager.Instance;

        // Save VFX settings
        VFXData vfxData = vfxManager.GetVFXData();

        // In your save system, add vfxData to SaveData:
        // saveData.vfxData = vfxData;

        Debug.Log($"[VFX Example] Saved VFX data: Quality={vfxData.quality}, Particles={vfxData.particlesEnabled}");

        // Load VFX settings
        // VFXData loadedData = saveData.vfxData;
        // vfxManager.LoadVFXData(loadedData);

        Debug.Log("[VFX Example] VFX settings saved/loaded!");
    }

    // ===== TESTING HELPER =====

    /// <summary>
    /// Test all VFX systems at once (for debugging).
    /// </summary>
    public void TestAllVFXSystems()
    {
        Debug.Log("===== TESTING ALL VFX SYSTEMS =====");

        Example_PetACompanion();
        Example_FishingSequence();
        Example_WaterEffects();
        Example_SanityEffects();
        Example_NightHazard();
        Example_BloodMoonEvent();
        Example_MeteorShower();
        Example_WeatherChange();
        Example_InventoryEffects();
        Example_UIEffects();
        Example_QualitySettings();
        Example_PostProcessing();
        Example_FishVisuals();
        Example_SaveLoadVFX();

        Debug.Log("===== ALL VFX SYSTEMS TESTED =====");
    }
}

// ===== HOW TO USE THIS FILE =====
//
// 1. Attach this script to any GameObject in your scene
// 2. Call the example methods from your gameplay code
// 3. Use the patterns shown here in your own systems
//
// For example, in your PetController.cs:
//   void OnPlayerInteract()
//   {
//       EventSystem.Publish("PetPetted", new PetPettedData(...));
//   }
//
// The VFX system will automatically handle the visual effects!

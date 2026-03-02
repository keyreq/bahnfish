using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Agent 12: Audio System Specialist - SoundEffectManager.cs
/// Manages 100+ sound effects with pooling, categorization, and easy playback.
/// Handles all game SFX: fishing, boat, horror, companion, environment, and items.
/// </summary>
public class SoundEffectManager : MonoBehaviour
{
    #region Inspector Settings
    [Header("Sound Effect Library")]
    [Tooltip("All registered sound effects in the game")]
    [SerializeField] private List<AudioClipData> soundEffects = new List<AudioClipData>();

    [Header("Auto-Play Settings")]
    [Tooltip("Automatically play sounds in response to game events")]
    [SerializeField] private bool autoPlayEnabled = true;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogging = true;
    #endregion

    #region Private Variables
    private AudioManager audioManager;
    private Dictionary<string, AudioClipData> sfxDictionary = new Dictionary<string, AudioClipData>();

    // Recently played sounds (for preventing spam)
    private Dictionary<string, float> recentlyPlayedSounds = new Dictionary<string, float>();
    #endregion

    #region Initialization
    private void Start()
    {
        audioManager = AudioManager.Instance;

        // Build SFX dictionary for fast lookup
        BuildSFXDictionary();

        // Subscribe to game events if auto-play enabled
        if (autoPlayEnabled)
        {
            SubscribeToEvents();
        }

        // Create default SFX library if empty
        if (soundEffects.Count == 0)
        {
            CreateDefaultSFXLibrary();
        }

        if (enableDebugLogging)
        {
            Debug.Log($"[SoundEffectManager] Initialized with {soundEffects.Count} sound effects");
        }
    }

    /// <summary>
    /// Builds a dictionary for fast sound effect lookup by ID.
    /// </summary>
    private void BuildSFXDictionary()
    {
        sfxDictionary.Clear();
        foreach (var sfx in soundEffects)
        {
            if (!string.IsNullOrEmpty(sfx.id) && !sfxDictionary.ContainsKey(sfx.id))
            {
                sfxDictionary[sfx.id] = sfx;
            }
        }
    }

    /// <summary>
    /// Subscribes to game events for automatic sound effect playback.
    /// </summary>
    private void SubscribeToEvents()
    {
        // Fishing events
        EventSystem.Subscribe("FishingCast", OnFishingCast);
        EventSystem.Subscribe("FishHooked", OnFishHooked);
        EventSystem.Subscribe("FishCaught", OnFishCaught);
        EventSystem.Subscribe("LineBroken", OnLineBroken);
        EventSystem.Subscribe("FishJump", OnFishJump);
        EventSystem.Subscribe("ReelStart", OnReelStart);
        EventSystem.Subscribe("ReelStop", OnReelStop);

        // Boat events
        EventSystem.Subscribe("BoatEngineStart", OnBoatEngineStart);
        EventSystem.Subscribe("BoatEngineStop", OnBoatEngineStop);
        EventSystem.Subscribe("BoatCollision", OnBoatCollision);
        EventSystem.Subscribe("AnchorDrop", OnAnchorDrop);
        EventSystem.Subscribe("AnchorRaise", OnAnchorRaise);

        // Horror events
        EventSystem.Subscribe("CrowCaw", OnCrowCaw);
        EventSystem.Subscribe("PhantomScream", OnPhantomScream);
        EventSystem.Subscribe("GhostShipHorn", OnGhostShipHorn);
        EventSystem.Subscribe("WhisperTrigger", OnWhisperTrigger);
        EventSystem.Subscribe<float>("SanityChanged", OnSanityChanged);

        // Companion events
        EventSystem.Subscribe<string>("PetPetted", OnPetPetted);
        EventSystem.Subscribe<string>("CompanionAbilityUsed", OnCompanionAbilityUsed);

        // Environment events
        EventSystem.Subscribe<string>("WeatherChanged", OnWeatherChanged);

        // Item/Inventory events
        EventSystem.Subscribe("ItemPickup", OnItemPickup);
        EventSystem.Subscribe("ItemSold", OnItemSold);
        EventSystem.Subscribe("UpgradePurchased", OnUpgradePurchased);
        EventSystem.Subscribe("CookingComplete", OnCookingComplete);
        EventSystem.Subscribe("CraftingComplete", OnCraftingComplete);

        // Photography events
        EventSystem.Subscribe("PhotoTaken", OnPhotoTaken);

        // Achievement events
        EventSystem.Subscribe("AchievementUnlocked", OnAchievementUnlocked);
    }
    #endregion

    #region Sound Playback
    /// <summary>
    /// Plays a sound effect by ID at a specific position.
    /// </summary>
    public AudioSource PlaySound(string soundID, Vector3 position = default, Transform parent = null)
    {
        if (!sfxDictionary.ContainsKey(soundID))
        {
            if (enableDebugLogging)
                Debug.LogWarning($"[SoundEffectManager] Sound '{soundID}' not found!");
            return null;
        }

        AudioClipData clipData = sfxDictionary[soundID];
        return audioManager.PlaySoundAtPosition(clipData, position, parent);
    }

    /// <summary>
    /// Plays a 2D sound effect (no position).
    /// </summary>
    public AudioSource PlaySound2D(string soundID)
    {
        return PlaySound(soundID, Vector3.zero, null);
    }

    /// <summary>
    /// Plays a random sound from a category.
    /// </summary>
    public AudioSource PlayRandomFromCategory(AudioCategory category, Vector3 position = default, Transform parent = null)
    {
        var categorySounds = soundEffects.Where(s => s.category == category).ToList();
        if (categorySounds.Count == 0)
        {
            if (enableDebugLogging)
                Debug.LogWarning($"[SoundEffectManager] No sounds found in category: {category}");
            return null;
        }

        AudioClipData randomClip = categorySounds[Random.Range(0, categorySounds.Count)];
        return audioManager.PlaySoundAtPosition(randomClip, position, parent);
    }

    /// <summary>
    /// Plays a sound with variation (randomized from similar sounds).
    /// For example, "splash" might play splash_01, splash_02, or splash_03.
    /// </summary>
    public AudioSource PlaySoundWithVariation(string baseSoundID, Vector3 position = default, Transform parent = null)
    {
        // Find all sounds starting with baseSoundID
        var variations = soundEffects.Where(s => s.id.StartsWith(baseSoundID)).ToList();

        if (variations.Count == 0)
        {
            // Fallback to exact match
            return PlaySound(baseSoundID, position, parent);
        }

        AudioClipData randomVariation = variations[Random.Range(0, variations.Count)];
        return audioManager.PlaySoundAtPosition(randomVariation, position, parent);
    }
    #endregion

    #region Event Handlers - Fishing
    private void OnFishingCast()
    {
        PlaySound2D("fishing_cast");
    }

    private void OnFishHooked()
    {
        PlaySound2D("fishing_hooked");
        PlaySound2D("line_tension");
    }

    private void OnFishCaught()
    {
        PlaySound2D("fish_caught");
        PlaySound2D("success_jingle");
    }

    private void OnLineBroken()
    {
        PlaySound2D("line_snap");
        PlaySound2D("fail_sound");
    }

    private void OnFishJump()
    {
        // 3D sound at fish position (would need position data)
        PlaySoundWithVariation("splash", Vector3.zero);
    }

    private void OnReelStart()
    {
        PlaySound2D("reel_start");
    }

    private void OnReelStop()
    {
        PlaySound2D("reel_stop");
    }
    #endregion

    #region Event Handlers - Boat
    private void OnBoatEngineStart()
    {
        PlaySound2D("boat_engine_start");
    }

    private void OnBoatEngineStop()
    {
        PlaySound2D("boat_engine_stop");
    }

    private void OnBoatCollision()
    {
        PlaySoundWithVariation("boat_collision", Vector3.zero);
    }

    private void OnAnchorDrop()
    {
        PlaySound2D("anchor_drop");
    }

    private void OnAnchorRaise()
    {
        PlaySound2D("anchor_raise");
    }
    #endregion

    #region Event Handlers - Horror
    private void OnCrowCaw()
    {
        PlaySoundWithVariation("crow_caw", Vector3.zero);
    }

    private void OnPhantomScream()
    {
        PlaySoundWithVariation("phantom_scream", Vector3.zero);
    }

    private void OnGhostShipHorn()
    {
        PlaySound("ghost_ship_horn", Vector3.zero);
    }

    private void OnWhisperTrigger()
    {
        PlaySoundWithVariation("whisper", Vector3.zero);
    }

    private void OnSanityChanged(float sanity)
    {
        // Play tinnitus/distortion at low sanity
        if (sanity < 30f && !recentlyPlayedSounds.ContainsKey("tinnitus"))
        {
            PlaySound2D("sanity_drain");
            recentlyPlayedSounds["tinnitus"] = Time.time;
        }
    }
    #endregion

    #region Event Handlers - Companion
    private void OnPetPetted(string petType)
    {
        // Play pet-specific sound based on type
        switch (petType.ToLower())
        {
            case "dog":
                PlaySound2D("dog_happy");
                break;
            case "cat":
                PlaySound2D("cat_purr");
                break;
            case "seabird":
                PlaySound2D("seabird_chirp");
                break;
            case "otter":
                PlaySound2D("otter_squeak");
                break;
            case "hermitcrab":
                PlaySound2D("crab_rattle");
                break;
        }
    }

    private void OnCompanionAbilityUsed(string abilityName)
    {
        PlaySound2D("ability_activate");
    }
    #endregion

    #region Event Handlers - Environment
    private void OnWeatherChanged(string weatherType)
    {
        // Weather sounds handled by AmbientSoundscape
        // Could add transition sounds here (thunder crack, wind gust, etc.)
        if (weatherType.ToLower() == "storm")
        {
            PlaySound2D("thunder_crack");
        }
    }
    #endregion

    #region Event Handlers - Items
    private void OnItemPickup()
    {
        PlaySound2D("item_pickup");
    }

    private void OnItemSold()
    {
        PlaySound2D("item_sell");
        PlaySound2D("coin_jingle");
    }

    private void OnUpgradePurchased()
    {
        PlaySound2D("upgrade_purchased");
    }

    private void OnCookingComplete()
    {
        PlaySound2D("cooking_complete");
    }

    private void OnCraftingComplete()
    {
        PlaySound2D("crafting_complete");
    }
    #endregion

    #region Event Handlers - Other
    private void OnPhotoTaken()
    {
        PlaySound2D("camera_shutter");
    }

    private void OnAchievementUnlocked()
    {
        PlaySound2D("achievement_fanfare");
    }
    #endregion

    #region SFX Library Creation
    /// <summary>
    /// Creates the default SFX library with 100+ sound effect definitions.
    /// Actual audio clips are assigned in Unity Inspector.
    /// </summary>
    private void CreateDefaultSFXLibrary()
    {
        // FISHING SOUNDS (25 sounds)
        AddSFX("fishing_cast", "Fishing Cast", AudioCategory.SFX_Fishing, 0.8f, AudioPriority.High);
        AddSFX("fishing_cast_whoosh", "Cast Whoosh", AudioCategory.SFX_Fishing, 0.7f, AudioPriority.Medium);
        AddSFX("line_release", "Line Release", AudioCategory.SFX_Fishing, 0.6f, AudioPriority.Medium);
        AddSFX("fishing_hooked", "Fish Hooked", AudioCategory.SFX_Fishing, 0.9f, AudioPriority.High);
        AddSFX("line_tension", "Line Tension", AudioCategory.SFX_Fishing, 0.7f, AudioPriority.High, loop: true);
        AddSFX("reel_start", "Reel Start", AudioCategory.SFX_Fishing, 0.8f, AudioPriority.High);
        AddSFX("reel_stop", "Reel Stop", AudioCategory.SFX_Fishing, 0.7f, AudioPriority.High);
        AddSFX("reel_fast", "Reel Fast", AudioCategory.SFX_Fishing, 0.8f, AudioPriority.High, loop: true);
        AddSFX("reel_slow", "Reel Slow", AudioCategory.SFX_Fishing, 0.7f, AudioPriority.Medium, loop: true);
        AddSFX("line_snap", "Line Snap", AudioCategory.SFX_Fishing, 0.9f, AudioPriority.High);
        AddSFX("fish_caught", "Fish Caught", AudioCategory.SFX_Fishing, 1.0f, AudioPriority.Critical);
        AddSFX("success_jingle", "Success Jingle", AudioCategory.SFX_Fishing, 0.8f, AudioPriority.High);
        AddSFX("fail_sound", "Fail Sound", AudioCategory.SFX_Fishing, 0.7f, AudioPriority.Medium);
        AddSFX("splash_01", "Splash 1", AudioCategory.SFX_Fishing, 0.7f, AudioPriority.Medium, is3D: true);
        AddSFX("splash_02", "Splash 2", AudioCategory.SFX_Fishing, 0.7f, AudioPriority.Medium, is3D: true);
        AddSFX("splash_03", "Splash 3", AudioCategory.SFX_Fishing, 0.7f, AudioPriority.Medium, is3D: true);
        AddSFX("splash_large", "Large Splash", AudioCategory.SFX_Fishing, 0.9f, AudioPriority.High, is3D: true);
        AddSFX("fish_jump", "Fish Jump", AudioCategory.SFX_Fishing, 0.8f, AudioPriority.Medium, is3D: true);
        AddSFX("fish_thrash", "Fish Thrashing", AudioCategory.SFX_Fishing, 0.7f, AudioPriority.Medium, is3D: true);
        AddSFX("underwater_splash", "Underwater Splash", AudioCategory.SFX_Fishing, 0.6f, AudioPriority.Low);
        AddSFX("bobber_splash", "Bobber Splash", AudioCategory.SFX_Fishing, 0.5f, AudioPriority.Low);
        AddSFX("line_creak", "Line Creak", AudioCategory.SFX_Fishing, 0.6f, AudioPriority.Medium);
        AddSFX("line_warning", "Line Warning", AudioCategory.SFX_Fishing, 0.8f, AudioPriority.High);
        AddSFX("fish_landed", "Fish Landed", AudioCategory.SFX_Fishing, 0.8f, AudioPriority.High);
        AddSFX("tackle_box_open", "Tackle Box Open", AudioCategory.SFX_Fishing, 0.6f, AudioPriority.Low);

        // BOAT SOUNDS (15 sounds)
        AddSFX("boat_engine_start", "Boat Engine Start", AudioCategory.SFX_Boat, 0.8f, AudioPriority.High);
        AddSFX("boat_engine_stop", "Boat Engine Stop", AudioCategory.SFX_Boat, 0.8f, AudioPriority.High);
        AddSFX("boat_engine_idle", "Boat Engine Idle", AudioCategory.SFX_Boat, 0.6f, AudioPriority.Medium, loop: true);
        AddSFX("boat_engine_running", "Boat Engine Running", AudioCategory.SFX_Boat, 0.7f, AudioPriority.Medium, loop: true);
        AddSFX("boat_acceleration", "Boat Acceleration", AudioCategory.SFX_Boat, 0.8f, AudioPriority.Medium);
        AddSFX("boat_bow_splash", "Bow Splash", AudioCategory.SFX_Boat, 0.6f, AudioPriority.Low, loop: true);
        AddSFX("boat_wake", "Boat Wake", AudioCategory.SFX_Boat, 0.5f, AudioPriority.Low, loop: true);
        AddSFX("boat_waves_hitting", "Waves Hitting Hull", AudioCategory.SFX_Boat, 0.6f, AudioPriority.Low, loop: true);
        AddSFX("boat_creak_wood", "Wood Creaking", AudioCategory.SFX_Boat, 0.5f, AudioPriority.Low);
        AddSFX("boat_creak_metal", "Metal Stress", AudioCategory.SFX_Boat, 0.5f, AudioPriority.Low);
        AddSFX("anchor_drop", "Anchor Drop", AudioCategory.SFX_Boat, 0.8f, AudioPriority.Medium);
        AddSFX("anchor_raise", "Anchor Raise", AudioCategory.SFX_Boat, 0.7f, AudioPriority.Medium);
        AddSFX("anchor_chain", "Anchor Chain", AudioCategory.SFX_Boat, 0.6f, AudioPriority.Low, loop: true);
        AddSFX("boat_collision_01", "Boat Collision 1", AudioCategory.SFX_Boat, 0.9f, AudioPriority.High);
        AddSFX("boat_collision_02", "Boat Collision 2", AudioCategory.SFX_Boat, 0.9f, AudioPriority.High);

        // HORROR SOUNDS (25 sounds)
        AddSFX("whisper_01", "Whisper 1", AudioCategory.SFX_Horror, 0.6f, AudioPriority.High, is3D: true);
        AddSFX("whisper_02", "Whisper 2", AudioCategory.SFX_Horror, 0.6f, AudioPriority.High, is3D: true);
        AddSFX("whisper_03", "Whisper 3", AudioCategory.SFX_Horror, 0.6f, AudioPriority.High, is3D: true);
        AddSFX("phantom_scream_01", "Phantom Scream 1", AudioCategory.SFX_Horror, 0.8f, AudioPriority.High, is3D: true);
        AddSFX("phantom_scream_02", "Phantom Scream 2", AudioCategory.SFX_Horror, 0.8f, AudioPriority.High, is3D: true);
        AddSFX("distant_scream", "Distant Scream", AudioCategory.SFX_Horror, 0.7f, AudioPriority.Medium, is3D: true);
        AddSFX("crow_caw_01", "Crow Caw 1", AudioCategory.SFX_Horror, 0.7f, AudioPriority.Medium, is3D: true);
        AddSFX("crow_caw_02", "Crow Caw 2", AudioCategory.SFX_Horror, 0.7f, AudioPriority.Medium, is3D: true);
        AddSFX("crow_caw_03", "Crow Caw 3", AudioCategory.SFX_Horror, 0.7f, AudioPriority.Medium, is3D: true);
        AddSFX("crow_wing_flap", "Crow Wing Flap", AudioCategory.SFX_Horror, 0.6f, AudioPriority.Low, is3D: true);
        AddSFX("phantom_wing_flap", "Phantom Wing Flap", AudioCategory.SFX_Horror, 0.7f, AudioPriority.Medium, is3D: true);
        AddSFX("ghost_ship_horn", "Ghost Ship Horn", AudioCategory.SFX_Horror, 0.9f, AudioPriority.High, is3D: true);
        AddSFX("ghost_ship_creak", "Ghost Ship Creak", AudioCategory.SFX_Horror, 0.6f, AudioPriority.Medium, is3D: true);
        AddSFX("ghost_ship_chains", "Ghost Ship Chains", AudioCategory.SFX_Horror, 0.7f, AudioPriority.Medium, is3D: true, loop: true);
        AddSFX("sanity_drain", "Sanity Drain (Tinnitus)", AudioCategory.SFX_Horror, 0.5f, AudioPriority.High);
        AddSFX("heartbeat_fast", "Fast Heartbeat", AudioCategory.SFX_Horror, 0.6f, AudioPriority.High, loop: true);
        AddSFX("breathing_heavy", "Heavy Breathing", AudioCategory.SFX_Horror, 0.5f, AudioPriority.Medium, loop: true);
        AddSFX("distortion_audio", "Audio Distortion", AudioCategory.SFX_Horror, 0.4f, AudioPriority.Medium);
        AddSFX("chase_stinger", "Chase Music Stinger", AudioCategory.SFX_Horror, 0.8f, AudioPriority.Critical);
        AddSFX("jumpscare", "Jumpscare", AudioCategory.SFX_Horror, 1.0f, AudioPriority.Critical);
        AddSFX("eerie_ambience", "Eerie Ambience", AudioCategory.SFX_Horror, 0.4f, AudioPriority.Low, loop: true);
        AddSFX("music_box", "Creepy Music Box", AudioCategory.SFX_Horror, 0.5f, AudioPriority.Medium, is3D: true);
        AddSFX("child_laugh", "Child Laugh (Creepy)", AudioCategory.SFX_Horror, 0.6f, AudioPriority.Medium, is3D: true);
        AddSFX("bell_toll", "Bell Toll", AudioCategory.SFX_Horror, 0.7f, AudioPriority.Medium, is3D: true);
        AddSFX("entity_presence", "Entity Presence", AudioCategory.SFX_Horror, 0.6f, AudioPriority.High, is3D: true);

        // COMPANION SOUNDS (15 sounds)
        AddSFX("dog_bark", "Dog Bark", AudioCategory.SFX_Companion, 0.7f, AudioPriority.Medium, is3D: true);
        AddSFX("dog_whine", "Dog Whine", AudioCategory.SFX_Companion, 0.6f, AudioPriority.Low, is3D: true);
        AddSFX("dog_growl", "Dog Growl", AudioCategory.SFX_Companion, 0.7f, AudioPriority.Medium, is3D: true);
        AddSFX("dog_pant", "Dog Panting", AudioCategory.SFX_Companion, 0.5f, AudioPriority.Low, is3D: true, loop: true);
        AddSFX("dog_happy", "Dog Happy", AudioCategory.SFX_Companion, 0.7f, AudioPriority.Medium, is3D: true);
        AddSFX("cat_meow", "Cat Meow", AudioCategory.SFX_Companion, 0.6f, AudioPriority.Medium, is3D: true);
        AddSFX("cat_purr", "Cat Purr", AudioCategory.SFX_Companion, 0.5f, AudioPriority.Low, is3D: true, loop: true);
        AddSFX("cat_hiss", "Cat Hiss", AudioCategory.SFX_Companion, 0.7f, AudioPriority.Medium, is3D: true);
        AddSFX("seabird_caw", "Seabird Caw", AudioCategory.SFX_Companion, 0.6f, AudioPriority.Low, is3D: true);
        AddSFX("seabird_chirp", "Seabird Chirp", AudioCategory.SFX_Companion, 0.5f, AudioPriority.Low, is3D: true);
        AddSFX("otter_squeak", "Otter Squeak", AudioCategory.SFX_Companion, 0.6f, AudioPriority.Medium, is3D: true);
        AddSFX("otter_splash", "Otter Splash", AudioCategory.SFX_Companion, 0.5f, AudioPriority.Low, is3D: true);
        AddSFX("crab_rattle", "Crab Shell Rattle", AudioCategory.SFX_Companion, 0.4f, AudioPriority.Low, is3D: true);
        AddSFX("crab_scuttle", "Crab Scuttle", AudioCategory.SFX_Companion, 0.3f, AudioPriority.VeryLow, is3D: true);
        AddSFX("ability_activate", "Ability Activate", AudioCategory.SFX_Companion, 0.8f, AudioPriority.High);

        // UI SOUNDS (20 sounds)
        AddSFX("button_hover", "Button Hover", AudioCategory.SFX_UI, 0.5f, AudioPriority.Medium);
        AddSFX("button_click", "Button Click", AudioCategory.SFX_UI, 0.7f, AudioPriority.High);
        AddSFX("button_disabled", "Button Disabled", AudioCategory.SFX_UI, 0.6f, AudioPriority.Medium);
        AddSFX("menu_open", "Menu Open", AudioCategory.SFX_UI, 0.7f, AudioPriority.High);
        AddSFX("menu_close", "Menu Close", AudioCategory.SFX_UI, 0.7f, AudioPriority.High);
        AddSFX("menu_transition", "Menu Transition", AudioCategory.SFX_UI, 0.6f, AudioPriority.Medium);
        AddSFX("notification_info", "Notification Info", AudioCategory.SFX_UI, 0.7f, AudioPriority.High);
        AddSFX("notification_warning", "Notification Warning", AudioCategory.SFX_UI, 0.8f, AudioPriority.High);
        AddSFX("notification_error", "Notification Error", AudioCategory.SFX_UI, 0.8f, AudioPriority.High);
        AddSFX("achievement_fanfare", "Achievement Unlocked", AudioCategory.SFX_UI, 0.8f, AudioPriority.Critical);
        AddSFX("quest_accept", "Quest Accepted", AudioCategory.SFX_UI, 0.7f, AudioPriority.High);
        AddSFX("quest_complete", "Quest Complete", AudioCategory.SFX_UI, 0.8f, AudioPriority.High);
        AddSFX("quest_fail", "Quest Failed", AudioCategory.SFX_UI, 0.7f, AudioPriority.Medium);
        AddSFX("level_up", "Level Up", AudioCategory.SFX_UI, 0.8f, AudioPriority.High);
        AddSFX("xp_gain", "XP Gain", AudioCategory.SFX_UI, 0.6f, AudioPriority.Low);
        AddSFX("slider_drag", "Slider Drag", AudioCategory.SFX_UI, 0.4f, AudioPriority.Low);
        AddSFX("tab_switch", "Tab Switch", AudioCategory.SFX_UI, 0.6f, AudioPriority.Medium);
        AddSFX("popup_open", "Popup Open", AudioCategory.SFX_UI, 0.7f, AudioPriority.High);
        AddSFX("popup_close", "Popup Close", AudioCategory.SFX_UI, 0.6f, AudioPriority.Medium);
        AddSFX("typing_sound", "Typing Sound", AudioCategory.SFX_UI, 0.5f, AudioPriority.Low);

        // ENVIRONMENT SOUNDS (10 sounds)
        AddSFX("thunder_crack", "Thunder Crack", AudioCategory.SFX_Environment, 0.9f, AudioPriority.High, is3D: true);
        AddSFX("lightning_strike", "Lightning Strike", AudioCategory.SFX_Environment, 0.8f, AudioPriority.High, is3D: true);
        AddSFX("wind_gust", "Wind Gust", AudioCategory.SFX_Environment, 0.6f, AudioPriority.Low);
        AddSFX("leaves_rustle", "Leaves Rustling", AudioCategory.SFX_Environment, 0.4f, AudioPriority.VeryLow, is3D: true);
        AddSFX("water_drip", "Water Drip", AudioCategory.SFX_Environment, 0.3f, AudioPriority.VeryLow, is3D: true);
        AddSFX("cave_drip", "Cave Drip", AudioCategory.SFX_Environment, 0.4f, AudioPriority.VeryLow, is3D: true);
        AddSFX("whale_song", "Whale Song", AudioCategory.SFX_Environment, 0.7f, AudioPriority.Medium, is3D: true);
        AddSFX("dolphin_clicks", "Dolphin Clicks", AudioCategory.SFX_Environment, 0.6f, AudioPriority.Low, is3D: true);
        AddSFX("seagull", "Seagull", AudioCategory.SFX_Environment, 0.5f, AudioPriority.Low, is3D: true);
        AddSFX("biolum_hum", "Bioluminescent Hum", AudioCategory.SFX_Environment, 0.4f, AudioPriority.VeryLow, is3D: true, loop: true);

        // ITEM/INVENTORY SOUNDS (10 sounds)
        AddSFX("item_pickup", "Item Pickup", AudioCategory.SFX_UI, 0.7f, AudioPriority.Medium);
        AddSFX("item_drop", "Item Drop", AudioCategory.SFX_UI, 0.6f, AudioPriority.Low);
        AddSFX("item_rotate", "Item Rotate", AudioCategory.SFX_UI, 0.4f, AudioPriority.Low);
        AddSFX("item_place", "Item Place", AudioCategory.SFX_UI, 0.6f, AudioPriority.Medium);
        AddSFX("item_sell", "Item Sell", AudioCategory.SFX_UI, 0.7f, AudioPriority.Medium);
        AddSFX("coin_jingle", "Coin Jingle", AudioCategory.SFX_UI, 0.7f, AudioPriority.Medium);
        AddSFX("upgrade_purchased", "Upgrade Purchased", AudioCategory.SFX_UI, 0.8f, AudioPriority.High);
        AddSFX("cooking_complete", "Cooking Complete", AudioCategory.SFX_UI, 0.7f, AudioPriority.Medium);
        AddSFX("crafting_complete", "Crafting Complete", AudioCategory.SFX_UI, 0.7f, AudioPriority.Medium);
        AddSFX("camera_shutter", "Camera Shutter", AudioCategory.SFX_UI, 0.8f, AudioPriority.High);

        if (enableDebugLogging)
        {
            Debug.Log($"[SoundEffectManager] Created default SFX library with {soundEffects.Count} sounds");
        }
    }

    /// <summary>
    /// Helper method to add a sound effect to the library.
    /// </summary>
    private void AddSFX(string id, string name, AudioCategory category, float volume,
                       AudioPriority priority, bool loop = false, bool is3D = false)
    {
        AudioClipData sfx = new AudioClipData
        {
            id = id,
            displayName = name,
            category = category,
            baseVolume = volume,
            priority = priority,
            loop = loop,
            is3D = is3D,
            pitchVariation = 0.1f,
            volumeVariation = 0.05f
        };

        if (is3D)
        {
            sfx.minDistance = 5f;
            sfx.maxDistance = 50f;
            sfx.dopplerLevel = 0.5f;
        }

        soundEffects.Add(sfx);
        sfxDictionary[id] = sfx;
    }
    #endregion

    #region Public API
    /// <summary>
    /// Registers a new sound effect.
    /// </summary>
    public void RegisterSoundEffect(AudioClipData clipData)
    {
        if (clipData == null || string.IsNullOrEmpty(clipData.id)) return;

        if (!sfxDictionary.ContainsKey(clipData.id))
        {
            soundEffects.Add(clipData);
            sfxDictionary[clipData.id] = clipData;
        }
    }

    /// <summary>
    /// Gets a sound effect by ID.
    /// </summary>
    public AudioClipData GetSoundEffect(string soundID)
    {
        return sfxDictionary.ContainsKey(soundID) ? sfxDictionary[soundID] : null;
    }

    /// <summary>
    /// Gets all sound effects in a category.
    /// </summary>
    public List<AudioClipData> GetSoundEffectsByCategory(AudioCategory category)
    {
        return soundEffects.Where(s => s.category == category).ToList();
    }
    #endregion

    #region Cleanup
    private void OnDestroy()
    {
        if (autoPlayEnabled)
        {
            // Unsubscribe from all events
            EventSystem.Unsubscribe("FishingCast", OnFishingCast);
            EventSystem.Unsubscribe("FishHooked", OnFishHooked);
            EventSystem.Unsubscribe("FishCaught", OnFishCaught);
            EventSystem.Unsubscribe("LineBroken", OnLineBroken);
            EventSystem.Unsubscribe("FishJump", OnFishJump);
            EventSystem.Unsubscribe("ReelStart", OnReelStart);
            EventSystem.Unsubscribe("ReelStop", OnReelStop);
            EventSystem.Unsubscribe("BoatEngineStart", OnBoatEngineStart);
            EventSystem.Unsubscribe("BoatEngineStop", OnBoatEngineStop);
            EventSystem.Unsubscribe("BoatCollision", OnBoatCollision);
            EventSystem.Unsubscribe("AnchorDrop", OnAnchorDrop);
            EventSystem.Unsubscribe("AnchorRaise", OnAnchorRaise);
            EventSystem.Unsubscribe("CrowCaw", OnCrowCaw);
            EventSystem.Unsubscribe("PhantomScream", OnPhantomScream);
            EventSystem.Unsubscribe("GhostShipHorn", OnGhostShipHorn);
            EventSystem.Unsubscribe("WhisperTrigger", OnWhisperTrigger);
            EventSystem.Unsubscribe<float>("SanityChanged", OnSanityChanged);
            EventSystem.Unsubscribe<string>("PetPetted", OnPetPetted);
            EventSystem.Unsubscribe<string>("CompanionAbilityUsed", OnCompanionAbilityUsed);
            EventSystem.Unsubscribe<string>("WeatherChanged", OnWeatherChanged);
            EventSystem.Unsubscribe("ItemPickup", OnItemPickup);
            EventSystem.Unsubscribe("ItemSold", OnItemSold);
            EventSystem.Unsubscribe("UpgradePurchased", OnUpgradePurchased);
            EventSystem.Unsubscribe("CookingComplete", OnCookingComplete);
            EventSystem.Unsubscribe("CraftingComplete", OnCraftingComplete);
            EventSystem.Unsubscribe("PhotoTaken", OnPhotoTaken);
            EventSystem.Unsubscribe("AchievementUnlocked", OnAchievementUnlocked);
        }
    }
    #endregion

    #region Editor Utilities
#if UNITY_EDITOR
    [ContextMenu("Print SFX Library")]
    private void PrintSFXLibrary()
    {
        Debug.Log("=== Sound Effect Library ===");
        Debug.Log($"Total Sounds: {soundEffects.Count}");

        foreach (AudioCategory category in System.Enum.GetValues(typeof(AudioCategory)))
        {
            int count = soundEffects.Count(s => s.category == category);
            if (count > 0)
                Debug.Log($"{category}: {count} sounds");
        }
    }

    [ContextMenu("Test Random Fishing Sound")]
    private void TestRandomFishingSound()
    {
        PlayRandomFromCategory(AudioCategory.SFX_Fishing);
    }
#endif
    #endregion
}

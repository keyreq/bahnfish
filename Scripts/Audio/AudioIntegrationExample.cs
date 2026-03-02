using UnityEngine;

/// <summary>
/// Agent 12: Audio System Specialist - Integration Example
/// Demonstrates how to use the audio system in various game scenarios.
/// This file serves as a reference and can be deleted in production.
/// </summary>
public class AudioIntegrationExample : MonoBehaviour
{
    #region Example 1: Basic Sound Effect Playback
    /// <summary>
    /// Example: Playing a simple 2D sound effect.
    /// </summary>
    private void Example_PlaySimpleSound()
    {
        SoundEffectManager sfxManager = FindObjectOfType<SoundEffectManager>();

        // Play a 2D sound (no position)
        sfxManager.PlaySound2D("button_click");

        // Or publish event (auto-plays via SoundEffectManager)
        EventSystem.Publish("ItemPickup");
    }
    #endregion

    #region Example 2: 3D Positional Sound
    /// <summary>
    /// Example: Playing a 3D sound at a specific world position.
    /// Use this for sounds that should have distance attenuation.
    /// </summary>
    private void Example_Play3DSound()
    {
        SoundEffectManager sfxManager = FindObjectOfType<SoundEffectManager>();

        // Play splash sound at fish position
        Vector3 fishPosition = new Vector3(10, 0, 5);
        sfxManager.PlaySound("splash_01", fishPosition);

        // Play random variation
        sfxManager.PlaySoundWithVariation("splash", fishPosition);

        // Play random from category
        sfxManager.PlayRandomFromCategory(AudioCategory.SFX_Horror, fishPosition);
    }
    #endregion

    #region Example 3: Dynamic Music Control
    /// <summary>
    /// Example: Controlling music based on game state.
    /// </summary>
    private void Example_MusicControl()
    {
        MusicSystem musicSystem = FindObjectOfType<MusicSystem>();

        // Switch music based on time of day
        TimeOfDay currentTime = TimeManager.Instance.GetCurrentTimeOfDay();

        switch (currentTime)
        {
            case TimeOfDay.Day:
                musicSystem.PlayTrack(MusicTrackType.Day);
                break;
            case TimeOfDay.Night:
                musicSystem.PlayTrack(MusicTrackType.Night);
                break;
            case TimeOfDay.Dusk:
                musicSystem.PlayTrack(MusicTrackType.Dusk);
                break;
            case TimeOfDay.Dawn:
                musicSystem.PlayTrack(MusicTrackType.Dawn);
                break;
        }

        // Or just publish event (MusicSystem listens automatically)
        EventSystem.Publish("FishingStarted"); // Switches to fishing music
        EventSystem.Publish("EnterShop"); // Switches to shop music
    }
    #endregion

    #region Example 4: Ambient Soundscape
    /// <summary>
    /// Example: Changing ambient soundscape when entering new location.
    /// </summary>
    private void Example_AmbientSoundscape()
    {
        AmbientSoundscape ambient = FindObjectOfType<AmbientSoundscape>();

        // Manually set location
        ambient.SetLocation("biolum_cavern");

        // Or publish event (AmbientSoundscape listens automatically)
        EventSystem.Publish("LocationChanged", "deep_ocean");

        // Weather and time changes are automatic via events
        EventSystem.Publish("WeatherChanged", "Storm");
    }
    #endregion

    #region Example 5: Fishing Audio Integration
    /// <summary>
    /// Example: Complete fishing audio sequence.
    /// </summary>
    private void Example_FishingAudio()
    {
        SoundEffectManager sfxManager = FindObjectOfType<SoundEffectManager>();
        MusicSystem musicSystem = FindObjectOfType<MusicSystem>();

        // Player casts line
        EventSystem.Publish("FishingCast");
        // Plays: fishing_cast sound + switches to fishing music

        // Fish bites (after delay)
        EventSystem.Publish("FishHooked");
        // Plays: fishing_hooked + line_tension (loops)

        // Player reeling
        sfxManager.PlaySound2D("reel_fast");

        // Fish jumps
        Vector3 fishJumpPosition = new Vector3(15, 2, 10);
        EventSystem.Publish("FishJump");
        // Plays: random splash sound at fish position

        // Success!
        EventSystem.Publish("FishCaught");
        // Plays: fish_caught + success_jingle

        // OR failure
        EventSystem.Publish("LineBroken");
        // Plays: line_snap + fail_sound
    }
    #endregion

    #region Example 6: Horror Audio
    /// <summary>
    /// Example: Horror audio for night hazards.
    /// </summary>
    private void Example_HorrorAudio()
    {
        SoundEffectManager sfxManager = FindObjectOfType<SoundEffectManager>();
        AudioManager audioManager = AudioManager.Instance;

        // Crow appears
        Vector3 crowPosition = new Vector3(20, 5, 15);
        EventSystem.Publish("CrowCaw");
        // Plays: random crow_caw sound at crow position

        // Phantom screams in distance
        Vector3 phantomPosition = new Vector3(50, 10, 30);
        EventSystem.Publish("PhantomScream");
        // Plays: phantom_scream (3D positional)

        // Ghost ship horn (very distant)
        Vector3 ghostShipPosition = new Vector3(200, 0, 100);
        EventSystem.Publish("GhostShipHorn");
        // Plays: ghost_ship_horn (3D, audible from far away)

        // Sanity drain sound (player going insane)
        float currentSanity = 25f;
        if (currentSanity < 30f)
        {
            sfxManager.PlaySound2D("sanity_drain");
            sfxManager.PlaySound2D("heartbeat_fast"); // Loops
        }

        // Jumpscare!
        sfxManager.PlaySound2D("jumpscare");
        audioManager.StartDucking(); // Lower music for dramatic effect
    }
    #endregion

    #region Example 7: Companion Audio
    /// <summary>
    /// Example: Companion pet audio.
    /// </summary>
    private void Example_CompanionAudio()
    {
        SoundEffectManager sfxManager = FindObjectOfType<SoundEffectManager>();

        // Player pets dog
        EventSystem.Publish("PetPetted", "dog");
        // Plays: dog_happy sound

        // Dog barks at hazard
        Vector3 dogPosition = transform.position;
        sfxManager.PlaySound("dog_bark", dogPosition);

        // Cat purrs (looping)
        AudioSource purring = sfxManager.PlaySound("cat_purr", dogPosition);

        // Stop purring after 5 seconds
        StartCoroutine(StopSoundAfterDelay(purring, 5f));
    }

    private System.Collections.IEnumerator StopSoundAfterDelay(AudioSource source, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (source != null)
        {
            AudioManager.Instance.StopAudioSource(source, 1f); // 1 second fade out
        }
    }
    #endregion

    #region Example 8: UI Audio
    /// <summary>
    /// Example: UI audio is mostly automatic, but can be triggered manually.
    /// </summary>
    private void Example_UIAudio()
    {
        UIAudioController uiAudio = FindObjectOfType<UIAudioController>();

        // Manual UI sounds
        uiAudio.PlayButtonClick();
        uiAudio.PlayMenuOpen();
        uiAudio.PlayNotification();

        // Or via events
        EventSystem.Publish("ShowNotification", "Quest completed!");
        // Plays: notification_info sound

        EventSystem.Publish("AchievementUnlocked", "first_catch");
        // Plays: achievement_fanfare sound

        EventSystem.Publish("ShowError", "Not enough money!");
        // Plays: notification_error sound
    }
    #endregion

    #region Example 9: Audio Ducking
    /// <summary>
    /// Example: Audio ducking for important sounds/dialog.
    /// </summary>
    private void Example_AudioDucking()
    {
        AudioManager audioManager = AudioManager.Instance;

        // Important dialog starting - lower music
        audioManager.StartDucking();

        // Play dialog audio here
        // ...

        // Dialog finished - restore music
        audioManager.StopDucking();
    }
    #endregion

    #region Example 10: Volume Control
    /// <summary>
    /// Example: Volume controls (typically in settings menu).
    /// </summary>
    private void Example_VolumeControl()
    {
        AudioManager audioManager = AudioManager.Instance;

        // Set volumes (0.0 - 1.0)
        audioManager.SetMasterVolume(0.8f);
        audioManager.SetMusicVolume(0.7f);
        audioManager.SetSFXVolume(0.9f);
        audioManager.SetAmbientVolume(0.6f);
        audioManager.SetUIVolume(1.0f);

        // Mute/unmute
        audioManager.ToggleMusicMute();
        audioManager.ToggleSFXMute();

        // Settings are automatically saved via SaveData integration
    }
    #endregion

    #region Example 11: Audio Zones
    /// <summary>
    /// Example: Setting up an audio zone in code.
    /// Usually done in Unity Editor, but can be created dynamically.
    /// </summary>
    private void Example_AudioZone()
    {
        // Create cave audio zone
        GameObject zoneObject = new GameObject("CaveAudioZone");
        AudioZone audioZone = zoneObject.AddComponent<AudioZone>();

        // Add trigger collider
        BoxCollider trigger = zoneObject.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.size = new Vector3(50, 30, 50);
        trigger.center = Vector3.zero;

        // Configure zone
        audioZone.GetType().GetField("zoneID", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(audioZone, "cave_zone_01");
        audioZone.GetType().GetField("locationID", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(audioZone, "biolum_cavern");
        audioZone.GetType().GetField("applyReverb", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(audioZone, true);
        audioZone.GetType().GetField("reverbPreset", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(audioZone, ReverbPreset.Cave);

        // Position zone
        zoneObject.transform.position = new Vector3(100, 0, 50);

        // When player enters zone:
        // - Location changes to "biolum_cavern"
        // - Ambient soundscape updates
        // - Cave reverb applies
    }
    #endregion

    #region Example 12: PositionalAudio Component
    /// <summary>
    /// Example: Adding PositionalAudio to a GameObject.
    /// </summary>
    private void Example_PositionalAudioComponent()
    {
        // Create a ghost ship with audio
        GameObject ghostShip = new GameObject("GhostShip");

        // Add positional audio for fog horn
        PositionalAudio fogHorn = ghostShip.AddComponent<PositionalAudio>();
        fogHorn.SetAudioClipByID("ghost_ship_horn");
        fogHorn.SetDistances(20f, 200f); // Audible from far away
        fogHorn.SetLoop(false);
        fogHorn.SetVolume(0.9f);

        // Play fog horn every 30 seconds
        InvokeRepeating(nameof(PlayGhostShipHorn), 5f, 30f);

        // Add creaking sound (looping)
        PositionalAudio creaking = ghostShip.AddComponent<PositionalAudio>();
        creaking.SetAudioClipByID("ghost_ship_creak");
        creaking.SetDistances(10f, 50f);
        creaking.SetLoop(true);
        creaking.SetVolume(0.6f);
        creaking.Play();
    }

    private void PlayGhostShipHorn()
    {
        PositionalAudio fogHorn = GetComponent<PositionalAudio>();
        if (fogHorn != null)
        {
            fogHorn.Play();
        }
    }
    #endregion

    #region Example 13: Custom Audio Events
    /// <summary>
    /// Example: Creating custom audio triggers for your game systems.
    /// </summary>
    private void Example_CustomAudioEvents()
    {
        // In your custom system, publish audio events

        // Example: Boss battle starts
        EventSystem.Publish("BossBattleStarted");

        // Example: Player discovers secret
        EventSystem.Publish("SecretDiscovered");

        // Example: Storm warning
        EventSystem.Publish("StormWarning");

        // Then in SoundEffectManager, subscribe to these events:
        // EventSystem.Subscribe("BossBattleStarted", OnBossBattleStarted);
        // private void OnBossBattleStarted() { PlaySound2D("boss_theme_stinger"); }
    }
    #endregion

    #region Example 14: Audio Fade Operations
    /// <summary>
    /// Example: Manual audio fade operations.
    /// </summary>
    private void Example_AudioFade()
    {
        AudioManager audioManager = AudioManager.Instance;
        SoundEffectManager sfxManager = FindObjectOfType<SoundEffectManager>();

        // Play looping sound
        AudioSource loopingSound = sfxManager.PlaySound2D("boat_engine_idle");

        // Fade it out after 5 seconds
        StartCoroutine(FadeOutAfterDelay(loopingSound, 5f));
    }

    private System.Collections.IEnumerator FadeOutAfterDelay(AudioSource source, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (source != null)
        {
            AudioManager.Instance.FadeAudioSource(source, 0f, 2f, () =>
            {
                source.Stop();
            });
        }
    }
    #endregion

    #region Example 15: Complete Scenario - Night Horror Sequence
    /// <summary>
    /// Example: Complete audio sequence for a night horror encounter.
    /// </summary>
    private void Example_CompleteHorrorSequence()
    {
        StartCoroutine(HorrorSequence());
    }

    private System.Collections.IEnumerator HorrorSequence()
    {
        SoundEffectManager sfxManager = FindObjectOfType<SoundEffectManager>();
        MusicSystem musicSystem = FindObjectOfType<MusicSystem>();
        AudioManager audioManager = AudioManager.Instance;

        // Night begins
        musicSystem.PlayTrack(MusicTrackType.Night);

        yield return new WaitForSeconds(5f);

        // Distant crow caw
        Vector3 crowPos = new Vector3(30, 5, 20);
        EventSystem.Publish("CrowCaw");

        yield return new WaitForSeconds(10f);

        // Whispers start (low sanity)
        sfxManager.PlaySound2D("whisper_01");

        yield return new WaitForSeconds(8f);

        // Ghost ship horn in distance
        Vector3 ghostShipPos = new Vector3(150, 0, 80);
        EventSystem.Publish("GhostShipHorn");

        yield return new WaitForSeconds(5f);

        // Phantom approaches
        Vector3 phantomPos = new Vector3(50, 10, 30);
        sfxManager.PlaySound("phantom_wing_flap", phantomPos);

        yield return new WaitForSeconds(3f);

        // Phantom screams!
        EventSystem.Publish("PhantomScream");
        audioManager.StartDucking(); // Duck music for dramatic effect

        yield return new WaitForSeconds(2f);

        // Chase begins
        musicSystem.PlayTrack(MusicTrackType.Boss);
        sfxManager.PlaySound2D("chase_stinger");
        sfxManager.PlaySound2D("heartbeat_fast");

        yield return new WaitForSeconds(10f);

        // Player escapes - restore calm
        audioManager.StopDucking();
        musicSystem.PlayTrack(MusicTrackType.Night);

        Debug.Log("[AudioIntegrationExample] Horror sequence complete!");
    }
    #endregion
}

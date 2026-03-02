using UnityEngine;
using System.Collections;

/// <summary>
/// Agent 13: Visual Effects & Particles Specialist - FishingVFX.cs
/// Manages all fishing-related visual effects including casting, reeling, fish jumps, and catch success.
/// Creates satisfying feedback for the fishing gameplay loop.
/// </summary>
public class FishingVFX : MonoBehaviour
{
    #region Configuration
    [Header("Casting Effects")]
    [SerializeField] private GameObject lineTrailPrefab;
    [SerializeField] private LineRenderer fishingLine;
    [SerializeField] private GameObject castImpactPrefab;
    [SerializeField] private float castTrailDuration = 0.5f;

    [Header("Bobber Effects")]
    [SerializeField] private GameObject bobberRipplePrefab;
    [SerializeField] private float bobberRippleInterval = 2f;

    [Header("Reeling Effects")]
    [SerializeField] private GameObject tensionSparklesPrefab;
    [SerializeField] private GameObject warningParticlesPrefab;
    [SerializeField] private float tensionEffectThreshold = 0.7f;

    [Header("Fish Jump Effects")]
    [SerializeField] private GameObject fishJumpSplashPrefab;
    [SerializeField] private GameObject waterDropletsPrefab;
    [SerializeField] private GameObject rainbowSprayPrefab;

    [Header("Catch Success Effects")]
    [SerializeField] private GameObject successSparklesPrefab;
    [SerializeField] private GameObject rareCatchBurstPrefab;
    [SerializeField] private GameObject legendaryCatchBurstPrefab;
    [SerializeField] private GameObject aberrantCatchEffectPrefab;

    [Header("Line Break Effects")]
    [SerializeField] private GameObject lineSnapPrefab;
    [SerializeField] private GameObject disappointmentPuffPrefab;

    [Header("Tension Meter Effects")]
    [SerializeField] private GameObject tensionGlowPrefab;
    [SerializeField] private GameObject dangerPulsePrefab;
    #endregion

    #region Private Fields
    private VFXQuality currentQuality = VFXQuality.High;
    private VFXManager vfxManager;
    private WaterEffects waterEffects;
    private bool isFishing = false;
    private float currentTension = 0f;
    private Vector3 bobberPosition;
    private Coroutine bobberRippleCoroutine;
    private ParticleSystem activeTensionEffect;
    #endregion

    #region Initialization
    private void Start()
    {
        vfxManager = VFXManager.Instance;
        waterEffects = vfxManager.GetWaterEffects();

        // Subscribe to events
        SubscribeToEvents();

        // Register particle prefabs
        RegisterParticlePrefabs();

        Debug.Log("[FishingVFX] Initialized.");
    }

    /// <summary>
    /// Subscribes to game events.
    /// </summary>
    private void SubscribeToEvents()
    {
        EventSystem.Subscribe<CastingData>("FishingCastStarted", OnCastStarted);
        EventSystem.Subscribe<Vector3>("FishingBobberLanded", OnBobberLanded);
        EventSystem.Subscribe<FishHookedData>("FishHooked", OnFishHooked);
        EventSystem.Subscribe<float>("FishingTensionChanged", OnTensionChanged);
        EventSystem.Subscribe<FishJumpData>("FishJumped", OnFishJumped);
        EventSystem.Subscribe<FishCaughtData>("FishCaught", OnFishCaught);
        EventSystem.Subscribe("LineBroken", OnLineBroken);
        EventSystem.Subscribe("FishingEnded", OnFishingEnded);
    }

    /// <summary>
    /// Registers particle prefabs with the VFX Manager.
    /// </summary>
    private void RegisterParticlePrefabs()
    {
        if (vfxManager == null) return;

        if (lineTrailPrefab != null) vfxManager.RegisterParticlePrefab("fishing_line_trail", lineTrailPrefab);
        if (castImpactPrefab != null) vfxManager.RegisterParticlePrefab("fishing_cast_impact", castImpactPrefab);
        if (bobberRipplePrefab != null) vfxManager.RegisterParticlePrefab("fishing_bobber_ripple", bobberRipplePrefab);
        if (tensionSparklesPrefab != null) vfxManager.RegisterParticlePrefab("fishing_tension_sparkles", tensionSparklesPrefab);
        if (warningParticlesPrefab != null) vfxManager.RegisterParticlePrefab("fishing_warning", warningParticlesPrefab);
        if (fishJumpSplashPrefab != null) vfxManager.RegisterParticlePrefab("fishing_jump_splash", fishJumpSplashPrefab);
        if (waterDropletsPrefab != null) vfxManager.RegisterParticlePrefab("fishing_water_droplets", waterDropletsPrefab);
        if (rainbowSprayPrefab != null) vfxManager.RegisterParticlePrefab("fishing_rainbow_spray", rainbowSprayPrefab);
        if (successSparklesPrefab != null) vfxManager.RegisterParticlePrefab("fishing_success_sparkles", successSparklesPrefab);
        if (rareCatchBurstPrefab != null) vfxManager.RegisterParticlePrefab("fishing_rare_burst", rareCatchBurstPrefab);
        if (legendaryCatchBurstPrefab != null) vfxManager.RegisterParticlePrefab("fishing_legendary_burst", legendaryCatchBurstPrefab);
        if (aberrantCatchEffectPrefab != null) vfxManager.RegisterParticlePrefab("fishing_aberrant_effect", aberrantCatchEffectPrefab);
        if (lineSnapPrefab != null) vfxManager.RegisterParticlePrefab("fishing_line_snap", lineSnapPrefab);
        if (disappointmentPuffPrefab != null) vfxManager.RegisterParticlePrefab("fishing_disappointment", disappointmentPuffPrefab);
    }

    private void OnDestroy()
    {
        EventSystem.Unsubscribe<CastingData>("FishingCastStarted", OnCastStarted);
        EventSystem.Unsubscribe<Vector3>("FishingBobberLanded", OnBobberLanded);
        EventSystem.Unsubscribe<FishHookedData>("FishHooked", OnFishHooked);
        EventSystem.Unsubscribe<float>("FishingTensionChanged", OnTensionChanged);
        EventSystem.Unsubscribe<FishJumpData>("FishJumped", OnFishJumped);
        EventSystem.Unsubscribe<FishCaughtData>("FishCaught", OnFishCaught);
        EventSystem.Unsubscribe("LineBroken", OnLineBroken);
        EventSystem.Unsubscribe("FishingEnded", OnFishingEnded);
    }
    #endregion

    #region Casting Effects
    /// <summary>
    /// Creates casting visual effects including line trail and impact splash.
    /// </summary>
    private void CreateCastingEffect(Vector3 startPos, Vector3 endPos, float castPower)
    {
        // Line trail effect
        if (currentQuality >= VFXQuality.Medium)
        {
            StartCoroutine(AnimateCastLine(startPos, endPos, castPower));
        }

        // Cast impact splash (handled by water effects)
        // Bobber landing will trigger splash
    }

    /// <summary>
    /// Animates the fishing line casting arc.
    /// </summary>
    private IEnumerator AnimateCastLine(Vector3 start, Vector3 end, float power)
    {
        if (fishingLine == null) yield break;

        fishingLine.enabled = true;
        fishingLine.positionCount = 20;

        float duration = castTrailDuration / Mathf.Clamp(power, 0.5f, 2f);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Create arc
            for (int i = 0; i < fishingLine.positionCount; i++)
            {
                float pointT = (float)i / (fishingLine.positionCount - 1);

                if (pointT <= t)
                {
                    Vector3 position = Vector3.Lerp(start, end, pointT);
                    float arcHeight = Mathf.Sin(pointT * Mathf.PI) * 5f * power;
                    position.y += arcHeight;
                    fishingLine.SetPosition(i, position);
                }
            }

            yield return null;
        }

        // Keep line visible after cast
        // It will be updated during fishing
    }
    #endregion

    #region Bobber Effects
    /// <summary>
    /// Starts bobber idle effects including periodic ripples.
    /// </summary>
    private void StartBobberEffects(Vector3 bobberPos)
    {
        bobberPosition = bobberPos;

        if (bobberRippleCoroutine != null)
        {
            StopCoroutine(bobberRippleCoroutine);
        }
        bobberRippleCoroutine = StartCoroutine(BobberRippleRoutine());
    }

    /// <summary>
    /// Stops bobber idle effects.
    /// </summary>
    private void StopBobberEffects()
    {
        if (bobberRippleCoroutine != null)
        {
            StopCoroutine(bobberRippleCoroutine);
            bobberRippleCoroutine = null;
        }
    }

    /// <summary>
    /// Generates periodic ripples around the bobber.
    /// </summary>
    private IEnumerator BobberRippleRoutine()
    {
        while (isFishing)
        {
            if (waterEffects != null)
            {
                waterEffects.CreateRipple(bobberPosition, 0.3f);
            }

            yield return new WaitForSeconds(bobberRippleInterval);
        }
    }
    #endregion

    #region Reeling & Tension Effects
    /// <summary>
    /// Updates tension visual effects based on current tension level.
    /// </summary>
    private void UpdateTensionEffects(float tension)
    {
        currentTension = tension;

        // Tension sparkles along line
        if (tension > tensionEffectThreshold && currentQuality >= VFXQuality.Medium)
        {
            if (activeTensionEffect == null || !activeTensionEffect.isPlaying)
            {
                Vector3 midPoint = (bobberPosition + Camera.main.transform.position) * 0.5f;
                activeTensionEffect = vfxManager.SpawnEffect("fishing_tension_sparkles", midPoint);
            }
        }
        else if (activeTensionEffect != null && activeTensionEffect.isPlaying)
        {
            activeTensionEffect.Stop();
        }

        // Warning particles at high tension
        if (tension > 0.9f && currentQuality >= VFXQuality.Low)
        {
            Vector3 warningPos = bobberPosition;
            vfxManager.SpawnEffect("fishing_warning", warningPos);
        }
    }

    /// <summary>
    /// Creates water disturbance where fish is fighting.
    /// </summary>
    private void CreateFishFightingEffect(Vector3 position)
    {
        if (waterEffects != null && currentQuality >= VFXQuality.Medium)
        {
            waterEffects.CreateRipple(position, 1f);
            waterEffects.CreateBubbles(position, 5);
        }
    }
    #endregion

    #region Fish Jump Effects
    /// <summary>
    /// Creates visual effects for a fish jumping out of water.
    /// </summary>
    private void CreateFishJumpEffect(Vector3 position, FishRarity rarity, float fishSize)
    {
        // Jump splash
        if (waterEffects != null)
        {
            SplashSize splashSize = fishSize > 2f ? SplashSize.Large : (fishSize < 0.5f ? SplashSize.Small : SplashSize.Medium);
            waterEffects.CreateSplash(position, splashSize, 1.2f);
        }

        // Water droplets
        if (currentQuality >= VFXQuality.Medium)
        {
            vfxManager.SpawnEffect("fishing_water_droplets", position);
        }

        // Rainbow spray for legendary fish
        if (rarity >= FishRarity.Legendary && currentQuality >= VFXQuality.High)
        {
            vfxManager.SpawnEffect("fishing_rainbow_spray", position);
        }

        Debug.Log($"[FishingVFX] Fish jumped at {position}");
    }
    #endregion

    #region Catch Success Effects
    /// <summary>
    /// Creates success effects based on fish rarity.
    /// </summary>
    private void CreateCatchSuccessEffect(Vector3 position, FishRarity rarity, bool isAberrant)
    {
        // Base success sparkles
        vfxManager.SpawnEffect("fishing_success_sparkles", position);

        // Rarity-specific effects
        switch (rarity)
        {
            case FishRarity.Common:
                // Small sparkle (already done)
                break;

            case FishRarity.Uncommon:
                if (currentQuality >= VFXQuality.Medium)
                {
                    // Green glow
                    ParticleSystem ps = vfxManager.SpawnEffect("fishing_success_sparkles", position);
                    if (ps != null)
                    {
                        var main = ps.main;
                        main.startColor = Color.green;
                    }
                }
                break;

            case FishRarity.Rare:
                if (currentQuality >= VFXQuality.Medium)
                {
                    vfxManager.SpawnEffect("fishing_rare_burst", position);
                }
                break;

            case FishRarity.Legendary:
                if (currentQuality >= VFXQuality.Low)
                {
                    vfxManager.SpawnEffect("fishing_legendary_burst", position);

                    if (currentQuality >= VFXQuality.High)
                    {
                        vfxManager.SpawnEffect("fishing_rainbow_spray", position);
                    }
                }
                break;
        }

        // Aberrant effect
        if (isAberrant && currentQuality >= VFXQuality.Medium)
        {
            vfxManager.SpawnEffect("fishing_aberrant_effect", position);
        }

        Debug.Log($"[FishingVFX] Catch success effect for {rarity} fish{(isAberrant ? " (Aberrant)" : "")}");
    }
    #endregion

    #region Line Break Effects
    /// <summary>
    /// Creates visual effects when the fishing line breaks.
    /// </summary>
    private void CreateLineBreakEffect(Vector3 breakPosition)
    {
        // Line snap burst
        vfxManager.SpawnEffect("fishing_line_snap", breakPosition);

        // Disappointment puff
        if (currentQuality >= VFXQuality.Medium)
        {
            vfxManager.SpawnEffect("fishing_disappointment", breakPosition);
        }

        // Hide fishing line
        if (fishingLine != null)
        {
            fishingLine.enabled = false;
        }

        Debug.Log("[FishingVFX] Line break effect");
    }
    #endregion

    #region Event Handlers
    /// <summary>
    /// Handles fishing cast start event.
    /// </summary>
    private void OnCastStarted(CastingData data)
    {
        CreateCastingEffect(data.startPosition, data.targetPosition, data.castPower);
        Debug.Log($"[FishingVFX] Cast started: Power={data.castPower}");
    }

    /// <summary>
    /// Handles bobber landing event.
    /// </summary>
    private void OnBobberLanded(Vector3 position)
    {
        bobberPosition = position;

        // Impact splash (handled by water effects through event)

        // Start bobber idle effects
        isFishing = true;
        StartBobberEffects(position);

        Debug.Log($"[FishingVFX] Bobber landed at {position}");
    }

    /// <summary>
    /// Handles fish hooked event.
    /// </summary>
    private void OnFishHooked(FishHookedData data)
    {
        // Stop bobber idle effects
        StopBobberEffects();

        // Bobber submerge splash
        if (waterEffects != null)
        {
            waterEffects.CreateSplash(bobberPosition, SplashSize.Medium, 0.8f);
        }

        // Start fighting effects
        CreateFishFightingEffect(data.fishPosition);

        Debug.Log("[FishingVFX] Fish hooked");
    }

    /// <summary>
    /// Handles tension change event.
    /// </summary>
    private void OnTensionChanged(float tension)
    {
        UpdateTensionEffects(tension);
    }

    /// <summary>
    /// Handles fish jump event.
    /// </summary>
    private void OnFishJumped(FishJumpData data)
    {
        CreateFishJumpEffect(data.position, data.rarity, data.fishSize);
    }

    /// <summary>
    /// Handles fish caught event.
    /// </summary>
    private void OnFishCaught(FishCaughtData data)
    {
        CreateCatchSuccessEffect(bobberPosition, data.rarity, data.isAberrant);

        // Hide fishing line
        if (fishingLine != null)
        {
            StartCoroutine(FadeOutLine());
        }
    }

    /// <summary>
    /// Handles line broken event.
    /// </summary>
    private void OnLineBroken()
    {
        CreateLineBreakEffect(bobberPosition);
        OnFishingEnded();
    }

    /// <summary>
    /// Handles fishing ended event.
    /// </summary>
    private void OnFishingEnded()
    {
        isFishing = false;
        StopBobberEffects();

        if (activeTensionEffect != null)
        {
            activeTensionEffect.Stop();
            activeTensionEffect = null;
        }

        if (fishingLine != null)
        {
            fishingLine.enabled = false;
        }

        Debug.Log("[FishingVFX] Fishing ended");
    }

    /// <summary>
    /// Fades out the fishing line after catching a fish.
    /// </summary>
    private IEnumerator FadeOutLine()
    {
        if (fishingLine == null) yield break;

        float duration = 0.5f;
        float elapsed = 0f;
        Color startColor = fishingLine.startColor;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = 1f - (elapsed / duration);

            Color color = startColor;
            color.a = alpha;
            fishingLine.startColor = color;
            fishingLine.endColor = color;

            yield return null;
        }

        fishingLine.enabled = false;
        fishingLine.startColor = startColor; // Reset for next time
        fishingLine.endColor = startColor;
    }
    #endregion

    #region Quality Settings
    /// <summary>
    /// Sets the VFX quality level for fishing effects.
    /// </summary>
    public void SetQuality(VFXQuality quality)
    {
        currentQuality = quality;
        Debug.Log($"[FishingVFX] Quality set to {quality}");
    }
    #endregion
}

#region Data Structures
/// <summary>
/// Fish hooked event data.
/// </summary>
[System.Serializable]
public struct FishHookedData
{
    public Vector3 fishPosition;
    public string fishID;
    public FishRarity rarity;

    public FishHookedData(Vector3 pos, string id, FishRarity rarity)
    {
        fishPosition = pos;
        fishID = id;
        this.rarity = rarity;
    }
}

/// <summary>
/// Fish caught event data.
/// </summary>
[System.Serializable]
public struct FishCaughtData
{
    public string fishID;
    public FishRarity rarity;
    public float weight;
    public bool isAberrant;

    public FishCaughtData(string id, FishRarity rarity, float weight, bool aberrant)
    {
        fishID = id;
        this.rarity = rarity;
        this.weight = weight;
        isAberrant = aberrant;
    }
}
#endregion

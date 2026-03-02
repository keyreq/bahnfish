using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;

/// <summary>
/// Agent 13: Visual Effects & Particles Specialist - HorrorVFX.cs
/// Manages all horror-related visual effects including sanity distortions and night hazard effects.
/// Creates atmospheric tension and dread for the night horror gameplay.
/// </summary>
public class HorrorVFX : MonoBehaviour
{
    #region Configuration
    [Header("Sanity Screen Effects")]
    [SerializeField] private Material screenEffectMaterial;
    [SerializeField] private float vignetteIntensity = 0f;
    [SerializeField] private float chromaticAberrationIntensity = 0f;
    [SerializeField] private float screenShakeIntensity = 0f;
    [SerializeField] private float colorSaturation = 1f;

    [Header("Night Hazard Effects")]
    [SerializeField] private GameObject fishThiefMistPrefab;
    [SerializeField] private GameObject fishThiefFeathersPrefab;
    [SerializeField] private GameObject obstaclePulsePrefab;
    [SerializeField] private GameObject fogHazardPrefab;
    [SerializeField] private GameObject ghostShipGlowPrefab;
    [SerializeField] private GameObject whispererRipplePrefab;

    [Header("Low Sanity Hallucinations")]
    [SerializeField] private GameObject shadowParticlesPrefab;
    [SerializeField] private GameObject eyesInDarknessPrefab;
    [SerializeField] private float hallucinationFrequency = 5f;

    [Header("Cursed Fish Effects")]
    [SerializeField] private GameObject cursedAuraPrefab;
    [SerializeField] private Color cursedAuraColor = new Color(0.5f, 0f, 0.5f);
    #endregion

    #region Private Fields
    private VFXQuality currentQuality = VFXQuality.High;
    private VFXManager vfxManager;
    private PostProcessingManager postProcessing;
    private Camera mainCamera;
    private float currentSanity = 100f;
    private Vector3 originalCameraPosition;
    private Coroutine screenShakeCoroutine;
    private Coroutine hallucinationCoroutine;
    private bool isHallucinatingActive = false;
    #endregion

    #region Initialization
    private void Start()
    {
        vfxManager = VFXManager.Instance;
        postProcessing = vfxManager.GetPostProcessingManager();
        mainCamera = Camera.main;

        if (mainCamera != null)
        {
            originalCameraPosition = mainCamera.transform.localPosition;
        }

        // Subscribe to events
        SubscribeToEvents();

        // Register particle prefabs
        RegisterParticlePrefabs();

        Debug.Log("[HorrorVFX] Initialized.");
    }

    /// <summary>
    /// Subscribes to game events.
    /// </summary>
    private void SubscribeToEvents()
    {
        EventSystem.Subscribe<float>("SanityChanged", OnSanityChanged);
        EventSystem.Subscribe<HazardSpawnData>("NightHazardSpawned", OnNightHazardSpawned);
        EventSystem.Subscribe<string>("FishThiefApproaching", OnFishThiefApproaching);
        EventSystem.Subscribe<Vector3>("WhispererActive", OnWhispererActive);
    }

    /// <summary>
    /// Registers particle prefabs with the VFX Manager.
    /// </summary>
    private void RegisterParticlePrefabs()
    {
        if (vfxManager == null) return;

        if (fishThiefMistPrefab != null) vfxManager.RegisterParticlePrefab("horror_fish_thief_mist", fishThiefMistPrefab);
        if (fishThiefFeathersPrefab != null) vfxManager.RegisterParticlePrefab("horror_fish_thief_feathers", fishThiefFeathersPrefab);
        if (obstaclePulsePrefab != null) vfxManager.RegisterParticlePrefab("horror_obstacle_pulse", obstaclePulsePrefab);
        if (fogHazardPrefab != null) vfxManager.RegisterParticlePrefab("horror_fog_hazard", fogHazardPrefab);
        if (ghostShipGlowPrefab != null) vfxManager.RegisterParticlePrefab("horror_ghost_ship_glow", ghostShipGlowPrefab);
        if (whispererRipplePrefab != null) vfxManager.RegisterParticlePrefab("horror_whisperer_ripple", whispererRipplePrefab);
        if (shadowParticlesPrefab != null) vfxManager.RegisterParticlePrefab("horror_shadow_particles", shadowParticlesPrefab);
        if (eyesInDarknessPrefab != null) vfxManager.RegisterParticlePrefab("horror_eyes_in_darkness", eyesInDarknessPrefab);
        if (cursedAuraPrefab != null) vfxManager.RegisterParticlePrefab("horror_cursed_aura", cursedAuraPrefab);
    }

    private void OnDestroy()
    {
        EventSystem.Unsubscribe<float>("SanityChanged", OnSanityChanged);
        EventSystem.Unsubscribe<HazardSpawnData>("NightHazardSpawned", OnNightHazardSpawned);
        EventSystem.Unsubscribe<string>("FishThiefApproaching", OnFishThiefApproaching);
        EventSystem.Unsubscribe<Vector3>("WhispererActive", OnWhispererActive);

        // Restore camera position
        if (mainCamera != null)
        {
            mainCamera.transform.localPosition = originalCameraPosition;
        }
    }
    #endregion

    #region Update Loop
    private void Update()
    {
        UpdateSanityEffects();
    }

    /// <summary>
    /// Updates sanity-based visual effects every frame.
    /// </summary>
    private void UpdateSanityEffects()
    {
        if (postProcessing == null) return;

        // Apply vignette
        postProcessing.SetVignetteIntensity(vignetteIntensity);

        // Apply chromatic aberration
        postProcessing.SetChromaticAberration(chromaticAberrationIntensity);

        // Apply color saturation
        postProcessing.SetColorSaturation(colorSaturation);

        // Screen shake (if active)
        if (screenShakeIntensity > 0f && mainCamera != null)
        {
            ApplyScreenShake();
        }
    }

    /// <summary>
    /// Applies screen shake effect.
    /// </summary>
    private void ApplyScreenShake()
    {
        float shakeX = Random.Range(-screenShakeIntensity, screenShakeIntensity);
        float shakeY = Random.Range(-screenShakeIntensity, screenShakeIntensity);

        mainCamera.transform.localPosition = originalCameraPosition + new Vector3(shakeX, shakeY, 0f);
    }
    #endregion

    #region Sanity Visual Effects
    /// <summary>
    /// Updates all sanity-based effects based on current sanity level.
    /// </summary>
    private void UpdateSanityVisuals(float sanity)
    {
        currentSanity = sanity;

        // High Sanity (80-100%)
        if (sanity >= 80f)
        {
            vignetteIntensity = 0f;
            chromaticAberrationIntensity = 0f;
            screenShakeIntensity = 0f;
            colorSaturation = 1f;
            StopHallucinations();
        }
        // Medium Sanity (40-80%)
        else if (sanity >= 40f)
        {
            float t = (sanity - 40f) / 40f; // 0 at 40%, 1 at 80%
            vignetteIntensity = Mathf.Lerp(0.3f, 0f, t);
            chromaticAberrationIntensity = 0f;
            screenShakeIntensity = 0f;
            colorSaturation = Mathf.Lerp(0.9f, 1f, t);
            StopHallucinations();
        }
        // Low Sanity (10-40%)
        else if (sanity >= 10f)
        {
            float t = (sanity - 10f) / 30f; // 0 at 10%, 1 at 40%
            vignetteIntensity = Mathf.Lerp(0.7f, 0.3f, t);
            chromaticAberrationIntensity = Mathf.Lerp(0.5f, 0f, t);
            screenShakeIntensity = Mathf.Lerp(0.02f, 0f, t);
            colorSaturation = Mathf.Lerp(0.6f, 0.9f, t);

            if (currentQuality >= VFXQuality.Medium && !isHallucinatingActive)
            {
                StartHallucinations();
            }
        }
        // Critical Sanity (<10%)
        else
        {
            float t = sanity / 10f; // 0 at 0%, 1 at 10%
            vignetteIntensity = Mathf.Lerp(0.9f, 0.7f, t);
            chromaticAberrationIntensity = Mathf.Lerp(1f, 0.5f, t);
            screenShakeIntensity = Mathf.Lerp(0.05f, 0.02f, t);
            colorSaturation = Mathf.Lerp(0.2f, 0.6f, t);

            if (currentQuality >= VFXQuality.Medium && !isHallucinatingActive)
            {
                StartHallucinations();
            }
        }
    }

    /// <summary>
    /// Starts hallucination particle effects at low sanity.
    /// </summary>
    private void StartHallucinations()
    {
        if (isHallucinatingActive) return;

        isHallucinatingActive = true;
        hallucinationCoroutine = StartCoroutine(HallucinationRoutine());
    }

    /// <summary>
    /// Stops hallucination effects.
    /// </summary>
    private void StopHallucinations()
    {
        if (!isHallucinatingActive) return;

        isHallucinatingActive = false;
        if (hallucinationCoroutine != null)
        {
            StopCoroutine(hallucinationCoroutine);
            hallucinationCoroutine = null;
        }
    }

    /// <summary>
    /// Routine that spawns hallucination particles periodically.
    /// </summary>
    private IEnumerator HallucinationRoutine()
    {
        while (isHallucinatingActive && currentSanity < 40f)
        {
            SpawnHallucination();

            float delay = hallucinationFrequency * (currentSanity / 40f); // More frequent at lower sanity
            yield return new WaitForSeconds(Mathf.Max(delay, 1f));
        }
    }

    /// <summary>
    /// Spawns a random hallucination particle effect.
    /// </summary>
    private void SpawnHallucination()
    {
        if (mainCamera == null) return;

        // Random position at edge of screen
        Vector3 screenEdge = GetRandomScreenEdgePosition();

        int hallucinationType = Random.Range(0, 2);
        string effectID = hallucinationType == 0 ? "horror_shadow_particles" : "horror_eyes_in_darkness";

        vfxManager.SpawnEffect(effectID, screenEdge);
    }

    /// <summary>
    /// Gets a random position at the edge of the screen.
    /// </summary>
    private Vector3 GetRandomScreenEdgePosition()
    {
        if (mainCamera == null) return Vector3.zero;

        int edge = Random.Range(0, 4);
        Vector3 viewportPos = Vector3.zero;

        switch (edge)
        {
            case 0: viewportPos = new Vector3(Random.Range(0f, 1f), 0f, 10f); break; // Bottom
            case 1: viewportPos = new Vector3(Random.Range(0f, 1f), 1f, 10f); break; // Top
            case 2: viewportPos = new Vector3(0f, Random.Range(0f, 1f), 10f); break; // Left
            case 3: viewportPos = new Vector3(1f, Random.Range(0f, 1f), 10f); break; // Right
        }

        return mainCamera.ViewportToWorldPoint(viewportPos);
    }
    #endregion

    #region Night Hazard Effects
    /// <summary>
    /// Creates visual effects for fish thief hazard.
    /// </summary>
    public void CreateFishThiefEffect(Vector3 position)
    {
        // Dark mist trail
        vfxManager.SpawnEffect("horror_fish_thief_mist", position);

        // Feather particles
        if (currentQuality >= VFXQuality.Medium)
        {
            vfxManager.SpawnEffect("horror_fish_thief_feathers", position);
        }

        Debug.Log($"[HorrorVFX] Fish thief effect at {position}");
    }

    /// <summary>
    /// Creates visual effects for obstacle hazard.
    /// </summary>
    public void CreateObstacleWarningEffect(Vector3 position)
    {
        // Warning pulse
        vfxManager.SpawnEffect("horror_obstacle_pulse", position);

        Debug.Log($"[HorrorVFX] Obstacle warning at {position}");
    }

    /// <summary>
    /// Creates visual effects for fog hazard.
    /// </summary>
    public void CreateFogHazardEffect(Vector3 position)
    {
        // Dense fog particles
        vfxManager.SpawnEffect("horror_fog_hazard", position);

        // Increase fog density temporarily
        if (postProcessing != null)
        {
            StartCoroutine(TemporaryFogIncrease());
        }

        Debug.Log($"[HorrorVFX] Fog hazard at {position}");
    }

    /// <summary>
    /// Temporarily increases fog density.
    /// </summary>
    private IEnumerator TemporaryFogIncrease()
    {
        float originalFogDensity = RenderSettings.fogDensity;
        float targetDensity = originalFogDensity * 5f;
        float duration = 10f;
        float elapsed = 0f;

        // Fade in
        while (elapsed < duration * 0.2f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (duration * 0.2f);
            RenderSettings.fogDensity = Mathf.Lerp(originalFogDensity, targetDensity, t);
            yield return null;
        }

        // Hold
        yield return new WaitForSeconds(duration * 0.6f);

        // Fade out
        elapsed = 0f;
        while (elapsed < duration * 0.2f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (duration * 0.2f);
            RenderSettings.fogDensity = Mathf.Lerp(targetDensity, originalFogDensity, t);
            yield return null;
        }

        RenderSettings.fogDensity = originalFogDensity;
    }

    /// <summary>
    /// Creates visual effects for ghost ship hazard.
    /// </summary>
    public void CreateGhostShipEffect(GameObject ghostShip)
    {
        if (ghostShip == null) return;

        // Spectral glow
        ParticleSystem glow = vfxManager.SpawnEffect("horror_ghost_ship_glow", ghostShip.transform.position);
        if (glow != null)
        {
            glow.transform.SetParent(ghostShip.transform);
        }

        // Fog around ship
        if (currentQuality >= VFXQuality.Medium)
        {
            ParticleSystem fog = vfxManager.SpawnEffect("horror_fog_hazard", ghostShip.transform.position);
            if (fog != null)
            {
                fog.transform.SetParent(ghostShip.transform);
            }
        }

        Debug.Log($"[HorrorVFX] Ghost ship effect");
    }

    /// <summary>
    /// Creates visual effects for whisperer hazard.
    /// </summary>
    public void CreateWhispererEffect(Vector3 position)
    {
        // Sound wave ripples
        vfxManager.SpawnEffect("horror_whisperer_ripple", position);

        // Screen distortion ripple
        if (postProcessing != null && currentQuality >= VFXQuality.Medium)
        {
            StartCoroutine(ScreenDistortionPulse());
        }

        Debug.Log($"[HorrorVFX] Whisperer effect at {position}");
    }

    /// <summary>
    /// Creates a screen distortion pulse effect.
    /// </summary>
    private IEnumerator ScreenDistortionPulse()
    {
        float originalAberration = chromaticAberrationIntensity;
        float pulseIntensity = 0.5f;
        float duration = 1f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Sin((elapsed / duration) * Mathf.PI);
            chromaticAberrationIntensity = originalAberration + (pulseIntensity * t);
            yield return null;
        }

        chromaticAberrationIntensity = originalAberration;
    }
    #endregion

    #region Cursed Fish Effects
    /// <summary>
    /// Creates a persistent cursed aura around a fish or object.
    /// </summary>
    public ParticleSystem CreateCursedAura(GameObject target)
    {
        if (target == null || currentQuality < VFXQuality.Medium) return null;

        ParticleSystem aura = vfxManager.SpawnEffect("horror_cursed_aura", target.transform.position);
        if (aura != null)
        {
            aura.transform.SetParent(target.transform);
            aura.transform.localPosition = Vector3.zero;

            // Set color
            var main = aura.main;
            main.startColor = cursedAuraColor;
        }

        Debug.Log($"[HorrorVFX] Cursed aura on {target.name}");
        return aura;
    }
    #endregion

    #region Event Handlers
    /// <summary>
    /// Handles sanity change events.
    /// </summary>
    private void OnSanityChanged(float newSanity)
    {
        UpdateSanityVisuals(newSanity);
    }

    /// <summary>
    /// Handles night hazard spawn events.
    /// </summary>
    private void OnNightHazardSpawned(HazardSpawnData data)
    {
        switch (data.hazardType)
        {
            case "FishThief":
                CreateFishThiefEffect(data.position);
                break;

            case "Obstacle":
                CreateObstacleWarningEffect(data.position);
                break;

            case "Fog":
                CreateFogHazardEffect(data.position);
                break;

            case "GhostShip":
                if (data.hazardObject != null)
                {
                    CreateGhostShipEffect(data.hazardObject);
                }
                break;

            case "Whisperer":
                CreateWhispererEffect(data.position);
                break;
        }
    }

    /// <summary>
    /// Handles fish thief approaching event.
    /// </summary>
    private void OnFishThiefApproaching(string direction)
    {
        // Create directional effect
        if (mainCamera == null) return;

        Vector3 directionVector = GetDirectionVector(direction);
        Vector3 effectPosition = mainCamera.transform.position + directionVector * 10f;

        CreateFishThiefEffect(effectPosition);
    }

    /// <summary>
    /// Handles whisperer active event.
    /// </summary>
    private void OnWhispererActive(Vector3 position)
    {
        CreateWhispererEffect(position);
    }

    /// <summary>
    /// Converts a direction string to a vector.
    /// </summary>
    private Vector3 GetDirectionVector(string direction)
    {
        switch (direction.ToLower())
        {
            case "north": return Vector3.forward;
            case "south": return Vector3.back;
            case "east": return Vector3.right;
            case "west": return Vector3.left;
            default: return Vector3.forward;
        }
    }
    #endregion

    #region Quality Settings
    /// <summary>
    /// Sets the VFX quality level for horror effects.
    /// </summary>
    public void SetQuality(VFXQuality quality)
    {
        currentQuality = quality;

        // Restart hallucinations if needed
        if (isHallucinatingActive)
        {
            StopHallucinations();
            if (currentSanity < 40f && quality >= VFXQuality.Medium)
            {
                StartHallucinations();
            }
        }

        Debug.Log($"[HorrorVFX] Quality set to {quality}");
    }
    #endregion
}

#region Data Structures
/// <summary>
/// Hazard spawn event data.
/// </summary>
[System.Serializable]
public struct HazardSpawnData
{
    public string hazardType;
    public Vector3 position;
    public GameObject hazardObject;

    public HazardSpawnData(string type, Vector3 pos, GameObject obj = null)
    {
        hazardType = type;
        position = pos;
        hazardObject = obj;
    }
}
#endregion

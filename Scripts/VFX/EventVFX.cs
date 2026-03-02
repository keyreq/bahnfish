using UnityEngine;
using System.Collections;

/// <summary>
/// Agent 13: Visual Effects & Particles Specialist - EventVFX.cs
/// Manages all dynamic event visual effects including Blood Moon, meteor showers, aurora borealis, and festivals.
/// Creates spectacular atmospheric events that transform the game world.
/// </summary>
public class EventVFX : MonoBehaviour
{
    #region Configuration
    [Header("Blood Moon Effects")]
    [SerializeField] private Material skyboxMaterial;
    [SerializeField] private GameObject bloodMoonPrefab;
    [SerializeField] private GameObject redMistPrefab;
    [SerializeField] private Color bloodMoonSkyColor = new Color(0.5f, 0.1f, 0.1f);
    [SerializeField] private Color bloodMoonFogColor = new Color(0.6f, 0.1f, 0.1f);

    [Header("Meteor Shower Effects")]
    [SerializeField] private GameObject meteorPrefab;
    [SerializeField] private GameObject meteorTrailPrefab;
    [SerializeField] private GameObject meteorImpactPrefab;
    [SerializeField] private float meteorSpawnRate = 2f;
    [SerializeField] private int meteorCount = 20;

    [Header("Aurora Borealis Effects")]
    [SerializeField] private Material auroraMaterial;
    [SerializeField] private GameObject auroraQuadPrefab;
    [SerializeField] private float auroraWaveSpeed = 0.5f;
    [SerializeField] private Color[] auroraColors = { Color.green, Color.cyan, new Color(0.5f, 0f, 1f) };

    [Header("Festival Effects")]
    [SerializeField] private GameObject fireworkPrefab;
    [SerializeField] private GameObject lanternPrefab;
    [SerializeField] private GameObject confettiPrefab;
    [SerializeField] private float fireworkInterval = 3f;
    #endregion

    #region Private Fields
    private VFXQuality currentQuality = VFXQuality.High;
    private VFXManager vfxManager;
    private bool bloodMoonActive = false;
    private bool meteorShowerActive = false;
    private bool auroraActive = false;
    private bool festivalActive = false;
    private GameObject bloodMoonObject;
    private GameObject auroraObject;
    private Coroutine meteorShowerCoroutine;
    private Coroutine fireworksCoroutine;
    private Color originalSkyColor;
    private Color originalFogColor;
    #endregion

    #region Initialization
    private void Start()
    {
        vfxManager = VFXManager.Instance;

        // Store original colors
        originalSkyColor = RenderSettings.ambientLight;
        originalFogColor = RenderSettings.fogColor;

        // Subscribe to events
        SubscribeToEvents();

        // Register particle prefabs
        RegisterParticlePrefabs();

        Debug.Log("[EventVFX] Initialized.");
    }

    /// <summary>
    /// Subscribes to game events.
    /// </summary>
    private void SubscribeToEvents()
    {
        EventSystem.Subscribe<string>("DynamicEventStarted", OnDynamicEventStarted);
        EventSystem.Subscribe<string>("DynamicEventEnded", OnDynamicEventEnded);
    }

    /// <summary>
    /// Registers particle prefabs with the VFX Manager.
    /// </summary>
    private void RegisterParticlePrefabs()
    {
        if (vfxManager == null) return;

        if (redMistPrefab != null) vfxManager.RegisterParticlePrefab("event_blood_moon_mist", redMistPrefab);
        if (meteorPrefab != null) vfxManager.RegisterParticlePrefab("event_meteor", meteorPrefab);
        if (meteorTrailPrefab != null) vfxManager.RegisterParticlePrefab("event_meteor_trail", meteorTrailPrefab);
        if (meteorImpactPrefab != null) vfxManager.RegisterParticlePrefab("event_meteor_impact", meteorImpactPrefab);
        if (fireworkPrefab != null) vfxManager.RegisterParticlePrefab("event_firework", fireworkPrefab);
        if (lanternPrefab != null) vfxManager.RegisterParticlePrefab("event_lantern", lanternPrefab);
        if (confettiPrefab != null) vfxManager.RegisterParticlePrefab("event_confetti", confettiPrefab);
    }

    private void OnDestroy()
    {
        EventSystem.Unsubscribe<string>("DynamicEventStarted", OnDynamicEventStarted);
        EventSystem.Unsubscribe<string>("DynamicEventEnded", OnDynamicEventEnded);

        // Cleanup active events
        StopAllEvents();
    }
    #endregion

    #region Update Loop
    private void Update()
    {
        if (auroraActive)
        {
            UpdateAurora();
        }
    }

    /// <summary>
    /// Updates aurora wave animation.
    /// </summary>
    private void UpdateAurora()
    {
        if (auroraMaterial != null)
        {
            float waveOffset = Time.time * auroraWaveSpeed;
            auroraMaterial.SetFloat("_WaveOffset", waveOffset);
        }
    }
    #endregion

    #region Blood Moon Effects
    /// <summary>
    /// Starts Blood Moon visual effects.
    /// </summary>
    public void StartBloodMoon()
    {
        if (bloodMoonActive) return;
        bloodMoonActive = true;

        StartCoroutine(BloodMoonTransition(true));

        Debug.Log("[EventVFX] Blood Moon started");
    }

    /// <summary>
    /// Stops Blood Moon visual effects.
    /// </summary>
    public void StopBloodMoon()
    {
        if (!bloodMoonActive) return;
        bloodMoonActive = false;

        StartCoroutine(BloodMoonTransition(false));

        Debug.Log("[EventVFX] Blood Moon ended");
    }

    /// <summary>
    /// Transitions Blood Moon effects in or out.
    /// </summary>
    private IEnumerator BloodMoonTransition(bool fadeIn)
    {
        float duration = 5f;
        float elapsed = 0f;

        // Create/destroy moon object
        if (fadeIn && bloodMoonPrefab != null)
        {
            bloodMoonObject = Instantiate(bloodMoonPrefab);
            bloodMoonObject.transform.position = new Vector3(0, 100, 0);
        }

        // Transition colors
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = fadeIn ? (elapsed / duration) : (1f - elapsed / duration);

            // Sky color
            RenderSettings.ambientLight = Color.Lerp(originalSkyColor, bloodMoonSkyColor, t);

            // Fog color
            RenderSettings.fogColor = Color.Lerp(originalFogColor, bloodMoonFogColor, t);

            yield return null;
        }

        // Red mist particles
        if (fadeIn && currentQuality >= VFXQuality.Medium)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                vfxManager.SpawnEffect("event_blood_moon_mist", mainCamera.transform.position);
            }
        }

        // Destroy moon object when fading out
        if (!fadeIn && bloodMoonObject != null)
        {
            Destroy(bloodMoonObject);
            bloodMoonObject = null;
        }
    }
    #endregion

    #region Meteor Shower Effects
    /// <summary>
    /// Starts meteor shower visual effects.
    /// </summary>
    public void StartMeteorShower()
    {
        if (meteorShowerActive) return;
        meteorShowerActive = true;

        meteorShowerCoroutine = StartCoroutine(MeteorShowerRoutine());

        Debug.Log("[EventVFX] Meteor shower started");
    }

    /// <summary>
    /// Stops meteor shower visual effects.
    /// </summary>
    public void StopMeteorShower()
    {
        if (!meteorShowerActive) return;
        meteorShowerActive = false;

        if (meteorShowerCoroutine != null)
        {
            StopCoroutine(meteorShowerCoroutine);
            meteorShowerCoroutine = null;
        }

        Debug.Log("[EventVFX] Meteor shower ended");
    }

    /// <summary>
    /// Routine that spawns meteors periodically.
    /// </summary>
    private IEnumerator MeteorShowerRoutine()
    {
        int meteorsSpawned = 0;

        while (meteorShowerActive && meteorsSpawned < meteorCount)
        {
            SpawnMeteor();
            meteorsSpawned++;

            yield return new WaitForSeconds(meteorSpawnRate);
        }

        meteorShowerActive = false;
    }

    /// <summary>
    /// Spawns a single meteor.
    /// </summary>
    private void SpawnMeteor()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null) return;

        // Random spawn position in sky
        Vector3 spawnPos = mainCamera.transform.position;
        spawnPos += new Vector3(Random.Range(-50f, 50f), 50f, Random.Range(-50f, 50f));

        // Random target position in water
        Vector3 targetPos = mainCamera.transform.position;
        targetPos += new Vector3(Random.Range(-100f, 100f), 0f, Random.Range(-100f, 100f));

        StartCoroutine(AnimateMeteor(spawnPos, targetPos));
    }

    /// <summary>
    /// Animates a meteor falling.
    /// </summary>
    private IEnumerator AnimateMeteor(Vector3 start, Vector3 end)
    {
        // Spawn meteor particle
        ParticleSystem meteor = vfxManager.SpawnEffect("event_meteor", start);
        if (meteor == null) yield break;

        // Spawn trail
        ParticleSystem trail = null;
        if (currentQuality >= VFXQuality.Medium)
        {
            trail = vfxManager.SpawnEffect("event_meteor_trail", start);
        }

        float duration = 2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            Vector3 currentPos = Vector3.Lerp(start, end, t);
            meteor.transform.position = currentPos;

            if (trail != null)
            {
                trail.transform.position = currentPos;
            }

            yield return null;
        }

        // Impact effect
        vfxManager.SpawnEffect("event_meteor_impact", end);

        // Water splash
        WaterEffects waterEffects = vfxManager.GetWaterEffects();
        if (waterEffects != null)
        {
            waterEffects.CreateSplash(end, SplashSize.Large, 2f);
        }

        // Cleanup
        if (meteor != null) Destroy(meteor.gameObject, 1f);
        if (trail != null) Destroy(trail.gameObject, 1f);
    }
    #endregion

    #region Aurora Borealis Effects
    /// <summary>
    /// Starts aurora borealis visual effects.
    /// </summary>
    public void StartAurora()
    {
        if (auroraActive) return;
        if (currentQuality < VFXQuality.Medium) return; // Aurora requires at least medium quality

        auroraActive = true;

        // Create aurora quad in sky
        if (auroraQuadPrefab != null)
        {
            auroraObject = Instantiate(auroraQuadPrefab);
            auroraObject.transform.position = new Vector3(0, 80, 0);
            auroraObject.transform.rotation = Quaternion.Euler(90, 0, 0);
            auroraObject.transform.localScale = new Vector3(200, 200, 1);

            // Assign material
            if (auroraMaterial != null)
            {
                Renderer renderer = auroraObject.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material = auroraMaterial;
                }
            }
        }

        Debug.Log("[EventVFX] Aurora started");
    }

    /// <summary>
    /// Stops aurora borealis visual effects.
    /// </summary>
    public void StopAurora()
    {
        if (!auroraActive) return;
        auroraActive = false;

        if (auroraObject != null)
        {
            StartCoroutine(FadeOutAurora());
        }

        Debug.Log("[EventVFX] Aurora ended");
    }

    /// <summary>
    /// Fades out the aurora.
    /// </summary>
    private IEnumerator FadeOutAurora()
    {
        Renderer renderer = auroraObject != null ? auroraObject.GetComponent<Renderer>() : null;
        if (renderer == null) yield break;

        float duration = 3f;
        float elapsed = 0f;
        Color startColor = renderer.material.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float alpha = 1f - t;

            Color color = startColor;
            color.a = alpha;
            renderer.material.color = color;

            yield return null;
        }

        if (auroraObject != null)
        {
            Destroy(auroraObject);
            auroraObject = null;
        }
    }
    #endregion

    #region Festival Effects
    /// <summary>
    /// Starts festival visual effects.
    /// </summary>
    public void StartFestival()
    {
        if (festivalActive) return;
        festivalActive = true;

        // Start fireworks
        fireworksCoroutine = StartCoroutine(FireworksRoutine());

        // Spawn lanterns
        if (currentQuality >= VFXQuality.Medium)
        {
            StartCoroutine(SpawnLanterns());
        }

        // Spawn confetti
        if (currentQuality >= VFXQuality.High)
        {
            SpawnConfetti();
        }

        Debug.Log("[EventVFX] Festival started");
    }

    /// <summary>
    /// Stops festival visual effects.
    /// </summary>
    public void StopFestival()
    {
        if (!festivalActive) return;
        festivalActive = false;

        if (fireworksCoroutine != null)
        {
            StopCoroutine(fireworksCoroutine);
            fireworksCoroutine = null;
        }

        Debug.Log("[EventVFX] Festival ended");
    }

    /// <summary>
    /// Routine that launches fireworks periodically.
    /// </summary>
    private IEnumerator FireworksRoutine()
    {
        while (festivalActive)
        {
            LaunchFirework();
            yield return new WaitForSeconds(fireworkInterval);
        }
    }

    /// <summary>
    /// Launches a single firework.
    /// </summary>
    private void LaunchFirework()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null) return;

        // Random launch position
        Vector3 launchPos = mainCamera.transform.position;
        launchPos += new Vector3(Random.Range(-30f, 30f), 0f, Random.Range(-30f, 30f));

        // Burst position in sky
        Vector3 burstPos = launchPos + new Vector3(0, Random.Range(30f, 50f), 0);

        StartCoroutine(AnimateFirework(launchPos, burstPos));
    }

    /// <summary>
    /// Animates a firework launch and burst.
    /// </summary>
    private IEnumerator AnimateFirework(Vector3 start, Vector3 burst)
    {
        // Launch trail
        float launchDuration = 1f;
        float elapsed = 0f;

        while (elapsed < launchDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / launchDuration;
            Vector3 currentPos = Vector3.Lerp(start, burst, t);

            // Trail particles
            if (currentQuality >= VFXQuality.Medium && elapsed % 0.1f < 0.05f)
            {
                vfxManager.SpawnEffect("event_firework", currentPos);
            }

            yield return null;
        }

        // Burst
        ParticleSystem firework = vfxManager.SpawnEffect("event_firework", burst);
        if (firework != null)
        {
            var main = firework.main;
            main.startColor = Random.ColorHSV(0f, 1f, 1f, 1f, 0.8f, 1f);
        }

        // Play sound
        EventSystem.Publish("FireworkExploded", burst);
    }

    /// <summary>
    /// Spawns floating lanterns.
    /// </summary>
    private IEnumerator SpawnLanterns()
    {
        int lanternCount = 10;

        for (int i = 0; i < lanternCount; i++)
        {
            if (!festivalActive) break;

            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                Vector3 spawnPos = mainCamera.transform.position;
                spawnPos += new Vector3(Random.Range(-20f, 20f), 0f, Random.Range(-20f, 20f));

                ParticleSystem lantern = vfxManager.SpawnEffect("event_lantern", spawnPos);
                if (lantern != null)
                {
                    // Lanterns float upward slowly
                    StartCoroutine(FloatLantern(lantern.gameObject));
                }
            }

            yield return new WaitForSeconds(2f);
        }
    }

    /// <summary>
    /// Floats a lantern upward.
    /// </summary>
    private IEnumerator FloatLantern(GameObject lantern)
    {
        float duration = 20f;
        float elapsed = 0f;
        Vector3 startPos = lantern.transform.position;

        while (elapsed < duration && lantern != null)
        {
            elapsed += Time.deltaTime;
            lantern.transform.position = startPos + new Vector3(0, elapsed * 2f, 0);
            yield return null;
        }

        if (lantern != null)
        {
            Destroy(lantern);
        }
    }

    /// <summary>
    /// Spawns confetti particles.
    /// </summary>
    private void SpawnConfetti()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null) return;

        Vector3 spawnPos = mainCamera.transform.position + new Vector3(0, 10f, 0);
        vfxManager.SpawnEffect("event_confetti", spawnPos);
    }
    #endregion

    #region Event Management
    /// <summary>
    /// Stops all active events.
    /// </summary>
    private void StopAllEvents()
    {
        if (bloodMoonActive) StopBloodMoon();
        if (meteorShowerActive) StopMeteorShower();
        if (auroraActive) StopAurora();
        if (festivalActive) StopFestival();
    }
    #endregion

    #region Event Handlers
    /// <summary>
    /// Handles dynamic event start.
    /// </summary>
    private void OnDynamicEventStarted(string eventName)
    {
        switch (eventName)
        {
            case "BloodMoon":
                StartBloodMoon();
                break;
            case "MeteorShower":
                StartMeteorShower();
                break;
            case "Aurora":
            case "AuroraBorealis":
                StartAurora();
                break;
            case "Festival":
                StartFestival();
                break;
        }
    }

    /// <summary>
    /// Handles dynamic event end.
    /// </summary>
    private void OnDynamicEventEnded(string eventName)
    {
        switch (eventName)
        {
            case "BloodMoon":
                StopBloodMoon();
                break;
            case "MeteorShower":
                StopMeteorShower();
                break;
            case "Aurora":
            case "AuroraBorealis":
                StopAurora();
                break;
            case "Festival":
                StopFestival();
                break;
        }
    }
    #endregion

    #region Quality Settings
    /// <summary>
    /// Sets the VFX quality level for event effects.
    /// </summary>
    public void SetQuality(VFXQuality quality)
    {
        currentQuality = quality;

        // Restart aurora if quality changed
        if (auroraActive && quality < VFXQuality.Medium)
        {
            StopAurora();
        }

        Debug.Log($"[EventVFX] Quality set to {quality}");
    }
    #endregion
}

using UnityEngine;
using System.Collections;

/// <summary>
/// Agent 13: Visual Effects & Particles Specialist - CompanionVFX.cs
/// Manages all companion and pet visual effects including THE PETTING HEARTS and pet ability effects.
/// The key feature inspired by Cast n Chill!
/// </summary>
public class CompanionVFX : MonoBehaviour
{
    #region Configuration
    [Header("Petting Effects - THE KEY FEATURE!")]
    [SerializeField] private GameObject heartParticlePrefab;
    [SerializeField] private GameObject petGlowPrefab;
    [SerializeField] private GameObject sparklesBurstPrefab;
    [SerializeField] private int heartsPerPet = 3;
    [SerializeField] private float heartSpawnInterval = 0.1f;
    [SerializeField] private float petGlowDuration = 1f;

    [Header("Pet Ability Effects")]
    [SerializeField] private GameObject dogFetchTrailPrefab;
    [SerializeField] private GameObject catStealthFadePrefab;
    [SerializeField] private GameObject seabirdVisionConePrefab;
    [SerializeField] private GameObject otterSplashPrefab;
    [SerializeField] private GameObject crabShieldPrefab;
    [SerializeField] private GameObject ghostPhasePrefab;

    [Header("Loyalty Effects")]
    [SerializeField] private GameObject loyaltyIncreaseSparklesPrefab;
    [SerializeField] private float loyaltyEffectThreshold = 10f;
    #endregion

    #region Private Fields
    private VFXQuality currentQuality = VFXQuality.High;
    private VFXManager vfxManager;
    #endregion

    #region Initialization
    private void Start()
    {
        vfxManager = VFXManager.Instance;

        // Subscribe to events
        SubscribeToEvents();

        // Register particle prefabs
        RegisterParticlePrefabs();

        Debug.Log("[CompanionVFX] Initialized - Petting hearts ready!");
    }

    /// <summary>
    /// Subscribes to game events.
    /// </summary>
    private void SubscribeToEvents()
    {
        EventSystem.Subscribe<PetPettedData>("PetPetted", OnPetPetted);
        EventSystem.Subscribe<PetAbilityData>("PetAbilityActivated", OnPetAbilityActivated);
        EventSystem.Subscribe<LoyaltyChangedEventData>("LoyaltyChanged", OnLoyaltyChanged);
    }

    /// <summary>
    /// Registers particle prefabs with the VFX Manager.
    /// </summary>
    private void RegisterParticlePrefabs()
    {
        if (vfxManager == null) return;

        if (heartParticlePrefab != null) vfxManager.RegisterParticlePrefab("companion_heart", heartParticlePrefab);
        if (petGlowPrefab != null) vfxManager.RegisterParticlePrefab("companion_pet_glow", petGlowPrefab);
        if (sparklesBurstPrefab != null) vfxManager.RegisterParticlePrefab("companion_sparkles_burst", sparklesBurstPrefab);
        if (dogFetchTrailPrefab != null) vfxManager.RegisterParticlePrefab("companion_dog_fetch_trail", dogFetchTrailPrefab);
        if (catStealthFadePrefab != null) vfxManager.RegisterParticlePrefab("companion_cat_stealth", catStealthFadePrefab);
        if (seabirdVisionConePrefab != null) vfxManager.RegisterParticlePrefab("companion_seabird_vision", seabirdVisionConePrefab);
        if (otterSplashPrefab != null) vfxManager.RegisterParticlePrefab("companion_otter_splash", otterSplashPrefab);
        if (crabShieldPrefab != null) vfxManager.RegisterParticlePrefab("companion_crab_shield", crabShieldPrefab);
        if (ghostPhasePrefab != null) vfxManager.RegisterParticlePrefab("companion_ghost_phase", ghostPhasePrefab);
        if (loyaltyIncreaseSparklesPrefab != null) vfxManager.RegisterParticlePrefab("companion_loyalty_sparkles", loyaltyIncreaseSparklesPrefab);
    }

    private void OnDestroy()
    {
        EventSystem.Unsubscribe<PetPettedData>("PetPetted", OnPetPetted);
        EventSystem.Unsubscribe<PetAbilityData>("PetAbilityActivated", OnPetAbilityActivated);
        EventSystem.Unsubscribe<LoyaltyChangedEventData>("LoyaltyChanged", OnLoyaltyChanged);
    }
    #endregion

    #region Petting Effects - THE KEY FEATURE!
    /// <summary>
    /// Creates the amazing petting hearts effect! This is THE feature from Cast n Chill.
    /// Hearts float up, sparkles burst, pet glows with happiness!
    /// </summary>
    public void CreatePettingEffect(Vector3 petPosition, string petType = "default")
    {
        if (currentQuality < VFXQuality.Low) return;

        // Spawn hearts floating upward
        StartCoroutine(SpawnPettingHearts(petPosition, petType));

        // Radial sparkle burst
        if (currentQuality >= VFXQuality.Medium)
        {
            vfxManager.SpawnEffect("companion_sparkles_burst", petPosition);
        }

        // Warm glow around pet
        if (currentQuality >= VFXQuality.High)
        {
            StartCoroutine(PetGlowEffect(petPosition));
        }

        Debug.Log($"[CompanionVFX] Petting hearts effect for {petType}!");
    }

    /// <summary>
    /// Spawns hearts that float upward when petting.
    /// </summary>
    private IEnumerator SpawnPettingHearts(Vector3 position, string petType)
    {
        Color heartColor = GetHeartColorForPetType(petType);
        int heartCount = Mathf.RoundToInt(heartsPerPet * GetDensityMultiplier());

        for (int i = 0; i < heartCount; i++)
        {
            // Spawn heart
            ParticleSystem heart = vfxManager.SpawnEffect("companion_heart", position + Vector3.up);
            if (heart != null)
            {
                // Set color
                var main = heart.main;
                main.startColor = heartColor;

                // Animate heart floating up
                StartCoroutine(AnimateHeart(heart.gameObject, position));
            }

            yield return new WaitForSeconds(heartSpawnInterval);
        }
    }

    /// <summary>
    /// Animates a heart floating upward and fading out.
    /// </summary>
    private IEnumerator AnimateHeart(GameObject heart, Vector3 startPos)
    {
        if (heart == null) yield break;

        float duration = 2f;
        float elapsed = 0f;
        Vector3 randomOffset = new Vector3(Random.Range(-0.5f, 0.5f), 0, Random.Range(-0.5f, 0.5f));

        while (elapsed < duration && heart != null)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Move up with slight randomness
            heart.transform.position = startPos + Vector3.up * (t * 3f) + randomOffset * t;

            // Scale and fade
            float scale = 1f + Mathf.Sin(t * Mathf.PI) * 0.5f;
            heart.transform.localScale = Vector3.one * scale;

            yield return null;
        }

        if (heart != null)
        {
            Destroy(heart);
        }
    }

    /// <summary>
    /// Creates a warm glow effect around the pet.
    /// </summary>
    private IEnumerator PetGlowEffect(Vector3 position)
    {
        ParticleSystem glow = vfxManager.SpawnEffect("companion_pet_glow", position);
        if (glow == null) yield break;

        yield return new WaitForSeconds(petGlowDuration);

        if (glow != null)
        {
            glow.Stop();
        }
    }

    /// <summary>
    /// Gets the heart color for different pet types.
    /// </summary>
    private Color GetHeartColorForPetType(string petType)
    {
        switch (petType.ToLower())
        {
            case "dog": return new Color(1f, 0.2f, 0.2f); // Red
            case "cat": return new Color(1f, 0.5f, 0.8f); // Pink
            case "seabird": return new Color(0.5f, 0.8f, 1f); // Light blue
            case "otter": return new Color(0.6f, 0.4f, 0.2f); // Brown
            case "hermitcrab": return new Color(1f, 0.6f, 0.2f); // Orange
            case "ghost": return new Color(0.8f, 0.8f, 1f); // Pale blue
            default: return new Color(1f, 0.3f, 0.5f); // Default pink
        }
    }
    #endregion

    #region Pet Ability Effects
    /// <summary>
    /// Creates visual effects for pet abilities.
    /// </summary>
    private void CreatePetAbilityEffect(string petType, string abilityName, Vector3 position, GameObject petObject)
    {
        switch (petType.ToLower())
        {
            case "dog":
                if (abilityName == "Fetch") CreateDogFetchEffect(petObject);
                break;

            case "cat":
                if (abilityName == "Stealth") CreateCatStealthEffect(petObject);
                break;

            case "seabird":
                if (abilityName == "Scout") CreateSeabirdScoutEffect(position);
                break;

            case "otter":
                if (abilityName == "Dive") CreateOtterDiveEffect(position);
                break;

            case "hermitcrab":
                if (abilityName == "Shell") CreateCrabShieldEffect(petObject);
                break;

            case "ghost":
                if (abilityName == "Phase") CreateGhostPhaseEffect(petObject);
                break;
        }
    }

    /// <summary>
    /// Creates dog fetch trail effect.
    /// </summary>
    private void CreateDogFetchEffect(GameObject dog)
    {
        if (dog == null || currentQuality < VFXQuality.Medium) return;

        ParticleSystem trail = vfxManager.SpawnEffect("companion_dog_fetch_trail", dog.transform.position);
        if (trail != null)
        {
            trail.transform.SetParent(dog.transform);
        }
    }

    /// <summary>
    /// Creates cat stealth fade effect.
    /// </summary>
    private void CreateCatStealthEffect(GameObject cat)
    {
        if (cat == null) return;

        StartCoroutine(FadeCatStealth(cat));
    }

    /// <summary>
    /// Fades cat to translucent for stealth.
    /// </summary>
    private IEnumerator FadeCatStealth(GameObject cat)
    {
        Renderer[] renderers = cat.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) yield break;

        float duration = 0.5f;
        float elapsed = 0f;

        // Fade to 30% opacity
        while (elapsed < duration && cat != null)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float alpha = Mathf.Lerp(1f, 0.3f, t);

            foreach (Renderer renderer in renderers)
            {
                if (renderer != null && renderer.material != null)
                {
                    Color color = renderer.material.color;
                    color.a = alpha;
                    renderer.material.color = color;
                }
            }

            yield return null;
        }

        // Stealth VFX
        if (currentQuality >= VFXQuality.Medium && cat != null)
        {
            vfxManager.SpawnEffect("companion_cat_stealth", cat.transform.position);
        }
    }

    /// <summary>
    /// Creates seabird scout vision cone effect.
    /// </summary>
    private void CreateSeabirdScoutEffect(Vector3 position)
    {
        if (currentQuality < VFXQuality.Medium) return;

        ParticleSystem visionCone = vfxManager.SpawnEffect("companion_seabird_vision", position);
        // Vision cone would also highlight fish in range (handled by gameplay systems)
    }

    /// <summary>
    /// Creates otter dive splash and bubbles effect.
    /// </summary>
    private void CreateOtterDiveEffect(Vector3 position)
    {
        // Splash
        WaterEffects waterEffects = vfxManager.GetWaterEffects();
        if (waterEffects != null)
        {
            waterEffects.CreateSplash(position, SplashSize.Medium, 1f);
            waterEffects.CreateBubbles(position, 20);
        }

        // Dive particles
        if (currentQuality >= VFXQuality.Medium)
        {
            vfxManager.SpawnEffect("companion_otter_splash", position);
        }
    }

    /// <summary>
    /// Creates hermit crab protective shield effect.
    /// </summary>
    private void CreateCrabShieldEffect(GameObject crab)
    {
        if (crab == null || currentQuality < VFXQuality.Medium) return;

        ParticleSystem shield = vfxManager.SpawnEffect("companion_crab_shield", crab.transform.position);
        if (shield != null)
        {
            shield.transform.SetParent(crab.transform);
        }
    }

    /// <summary>
    /// Creates ghost pet phase effect.
    /// </summary>
    private void CreateGhostPhaseEffect(GameObject ghost)
    {
        if (ghost == null) return;

        StartCoroutine(GhostPhaseRoutine(ghost));
    }

    /// <summary>
    /// Animates ghost phasing to ethereal state.
    /// </summary>
    private IEnumerator GhostPhaseRoutine(GameObject ghost)
    {
        // Fade to ethereal blue
        Renderer[] renderers = ghost.GetComponentsInChildren<Renderer>();
        float duration = 0.5f;
        float elapsed = 0f;

        Color etherealBlue = new Color(0.5f, 0.8f, 1f, 0.5f);

        while (elapsed < duration && ghost != null)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            foreach (Renderer renderer in renderers)
            {
                if (renderer != null && renderer.material != null)
                {
                    renderer.material.color = Color.Lerp(Color.white, etherealBlue, t);
                }
            }

            yield return null;
        }

        // Ghostly trail particles
        if (currentQuality >= VFXQuality.Medium && ghost != null)
        {
            ParticleSystem trail = vfxManager.SpawnEffect("companion_ghost_phase", ghost.transform.position);
            if (trail != null)
            {
                trail.transform.SetParent(ghost.transform);
            }
        }
    }
    #endregion

    #region Loyalty Effects
    /// <summary>
    /// Creates sparkle effect when loyalty increases significantly.
    /// </summary>
    private void CreateLoyaltyIncreaseEffect(Vector3 position, float loyaltyChange)
    {
        if (loyaltyChange < loyaltyEffectThreshold) return;
        if (currentQuality < VFXQuality.Medium) return;

        vfxManager.SpawnEffect("companion_loyalty_sparkles", position);
    }
    #endregion

    #region Event Handlers
    /// <summary>
    /// Handles pet petted event - THE KEY FEATURE!
    /// </summary>
    private void OnPetPetted(PetPettedData data)
    {
        CreatePettingEffect(data.petPosition, data.petType);
        Debug.Log($"[CompanionVFX] {data.petType} was petted! Hearts spawned!");
    }

    /// <summary>
    /// Handles pet ability activated event.
    /// </summary>
    private void OnPetAbilityActivated(PetAbilityData data)
    {
        CreatePetAbilityEffect(data.petType, data.abilityName, data.position, data.petObject);
    }

    /// <summary>
    /// Handles loyalty changed event.
    /// </summary>
    private void OnLoyaltyChanged(LoyaltyChangedEventData data)
    {
        if (data.loyaltyChange > 0)
        {
            // Get pet position (would be passed in real event data)
            CreateLoyaltyIncreaseEffect(Vector3.zero, data.loyaltyChange);
        }
    }
    #endregion

    #region Quality Settings
    /// <summary>
    /// Sets the VFX quality level for companion effects.
    /// </summary>
    public void SetQuality(VFXQuality quality)
    {
        currentQuality = quality;
        Debug.Log($"[CompanionVFX] Quality set to {quality}");
    }

    /// <summary>
    /// Gets density multiplier based on quality.
    /// </summary>
    private float GetDensityMultiplier()
    {
        return vfxManager != null ? vfxManager.GetParticleDensity() : 1f;
    }
    #endregion
}

#region Data Structures
/// <summary>
/// Pet petted event data - THE KEY FEATURE!
/// </summary>
[System.Serializable]
public struct PetPettedData
{
    public string petID;
    public string petType;
    public Vector3 petPosition;
    public float currentLoyalty;

    public PetPettedData(string id, string type, Vector3 pos, float loyalty)
    {
        petID = id;
        petType = type;
        petPosition = pos;
        currentLoyalty = loyalty;
    }
}

/// <summary>
/// Pet ability activated event data.
/// </summary>
[System.Serializable]
public struct PetAbilityData
{
    public string petID;
    public string petType;
    public string abilityName;
    public Vector3 position;
    public GameObject petObject;

    public PetAbilityData(string id, string type, string ability, Vector3 pos, GameObject obj)
    {
        petID = id;
        petType = type;
        abilityName = ability;
        position = pos;
        petObject = obj;
    }
}

/// <summary>
/// Loyalty changed event data.
/// </summary>
[System.Serializable]
public struct LoyaltyChangedEventData
{
    public string petID;
    public float oldLoyalty;
    public float newLoyalty;
    public float loyaltyChange;

    public LoyaltyChangedEventData(string id, float oldVal, float newVal)
    {
        petID = id;
        oldLoyalty = oldVal;
        newLoyalty = newVal;
        loyaltyChange = newVal - oldVal;
    }
}
#endregion

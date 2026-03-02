using UnityEngine;

/// <summary>
/// Agent 13: Visual Effects & Particles Specialist - FishAIVisuals.cs
/// Manages visual effects for fish AI including swimming trails, school shimmer, and rarity auras.
/// </summary>
public class FishAIVisuals : MonoBehaviour
{
    [Header("Fish Trail Effects")]
    [SerializeField] private GameObject bioluminescentTrailPrefab;
    [SerializeField] private bool enableTrails = true;

    [Header("School Effects")]
    [SerializeField] private GameObject schoolShimmerPrefab;

    [Header("Rarity Auras")]
    [SerializeField] private GameObject uncommonAuraPrefab;
    [SerializeField] private GameObject rareAuraPrefab;
    [SerializeField] private GameObject legendaryAuraPrefab;
    [SerializeField] private GameObject aberrantEffectPrefab;

    private VFXQuality currentQuality = VFXQuality.High;
    private VFXManager vfxManager;

    private void Start()
    {
        vfxManager = VFXManager.Instance;
        RegisterParticlePrefabs();
        Debug.Log("[FishAIVisuals] Initialized.");
    }

    private void RegisterParticlePrefabs()
    {
        if (vfxManager == null) return;
        if (bioluminescentTrailPrefab != null) vfxManager.RegisterParticlePrefab("fish_bioluminescent_trail", bioluminescentTrailPrefab);
        if (schoolShimmerPrefab != null) vfxManager.RegisterParticlePrefab("fish_school_shimmer", schoolShimmerPrefab);
        if (uncommonAuraPrefab != null) vfxManager.RegisterParticlePrefab("fish_uncommon_aura", uncommonAuraPrefab);
        if (rareAuraPrefab != null) vfxManager.RegisterParticlePrefab("fish_rare_aura", rareAuraPrefab);
        if (legendaryAuraPrefab != null) vfxManager.RegisterParticlePrefab("fish_legendary_aura", legendaryAuraPrefab);
        if (aberrantEffectPrefab != null) vfxManager.RegisterParticlePrefab("fish_aberrant_effect", aberrantEffectPrefab);
    }

    /// <summary>
    /// Creates a swimming trail for bioluminescent fish.
    /// </summary>
    public ParticleSystem CreateFishTrail(GameObject fish, bool isBioluminescent)
    {
        if (!enableTrails || !isBioluminescent || currentQuality < VFXQuality.Medium) return null;

        ParticleSystem trail = vfxManager.SpawnEffect("fish_bioluminescent_trail", fish.transform.position);
        if (trail != null)
        {
            trail.transform.SetParent(fish.transform);
            trail.transform.localPosition = Vector3.zero;
        }
        return trail;
    }

    /// <summary>
    /// Creates shimmer effect for a school of fish.
    /// </summary>
    public void CreateSchoolShimmer(Vector3 schoolCenter, int fishCount)
    {
        if (currentQuality < VFXQuality.High || fishCount < 5) return;

        ParticleSystem shimmer = vfxManager.SpawnEffect("fish_school_shimmer", schoolCenter);
        if (shimmer != null)
        {
            var main = shimmer.main;
            main.startSizeMultiplier = Mathf.Min(fishCount * 0.1f, 3f);
        }
    }

    /// <summary>
    /// Creates a rarity-based aura around a fish.
    /// </summary>
    public ParticleSystem CreateFishAura(GameObject fish, FishRarity rarity, bool isAberrant)
    {
        if (fish == null || currentQuality < VFXQuality.Medium) return null;

        string effectID = GetAuraEffectID(rarity, isAberrant);
        if (effectID == null) return null;

        ParticleSystem aura = vfxManager.SpawnEffect(effectID, fish.transform.position);
        if (aura != null)
        {
            aura.transform.SetParent(fish.transform);
            aura.transform.localPosition = Vector3.zero;
        }

        return aura;
    }

    private string GetAuraEffectID(FishRarity rarity, bool isAberrant)
    {
        if (isAberrant) return "fish_aberrant_effect";

        switch (rarity)
        {
            case FishRarity.Uncommon: return currentQuality >= VFXQuality.High ? "fish_uncommon_aura" : null;
            case FishRarity.Rare: return "fish_rare_aura";
            case FishRarity.Legendary: return "fish_legendary_aura";
            default: return null;
        }
    }

    public void SetQuality(VFXQuality quality)
    {
        currentQuality = quality;
        enableTrails = quality >= VFXQuality.Medium;
    }
}

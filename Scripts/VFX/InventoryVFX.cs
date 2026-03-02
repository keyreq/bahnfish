using UnityEngine;

/// <summary>
/// Agent 13: Visual Effects & Particles Specialist - InventoryVFX.cs
/// Manages visual effects for inventory interactions including pickup, drag, sell, and craft effects.
/// </summary>
public class InventoryVFX : MonoBehaviour
{
    [Header("Item Interaction")]
    [SerializeField] private GameObject itemPickupGlowPrefab;
    [SerializeField] private GameObject validPlacementPrefab;
    [SerializeField] private GameObject invalidPlacementPrefab;

    [Header("Transaction Effects")]
    [SerializeField] private GameObject sellCoinsPrefab;
    [SerializeField] private GameObject craftSparklesPrefab;
    [SerializeField] private GameObject craftSuccessBurstPrefab;

    private VFXQuality currentQuality = VFXQuality.High;
    private VFXManager vfxManager;

    private void Start()
    {
        vfxManager = VFXManager.Instance;
        SubscribeToEvents();
        RegisterParticlePrefabs();
        Debug.Log("[InventoryVFX] Initialized.");
    }

    private void SubscribeToEvents()
    {
        EventSystem.Subscribe<ItemPickupData>("ItemPickedUp", OnItemPickedUp);
        EventSystem.Subscribe<ItemSoldData>("ItemSold", OnItemSold);
        EventSystem.Subscribe<ItemCraftedData>("ItemCrafted", OnItemCrafted);
    }

    private void RegisterParticlePrefabs()
    {
        if (vfxManager == null) return;
        if (itemPickupGlowPrefab != null) vfxManager.RegisterParticlePrefab("inventory_pickup_glow", itemPickupGlowPrefab);
        if (validPlacementPrefab != null) vfxManager.RegisterParticlePrefab("inventory_valid_placement", validPlacementPrefab);
        if (invalidPlacementPrefab != null) vfxManager.RegisterParticlePrefab("inventory_invalid_placement", invalidPlacementPrefab);
        if (sellCoinsPrefab != null) vfxManager.RegisterParticlePrefab("inventory_sell_coins", sellCoinsPrefab);
        if (craftSparklesPrefab != null) vfxManager.RegisterParticlePrefab("inventory_craft_sparkles", craftSparklesPrefab);
        if (craftSuccessBurstPrefab != null) vfxManager.RegisterParticlePrefab("inventory_craft_success", craftSuccessBurstPrefab);
    }

    private void OnDestroy()
    {
        EventSystem.Unsubscribe<ItemPickupData>("ItemPickedUp", OnItemPickedUp);
        EventSystem.Unsubscribe<ItemSoldData>("ItemSold", OnItemSold);
        EventSystem.Unsubscribe<ItemCraftedData>("ItemCrafted", OnItemCrafted);
    }

    public void CreateItemPickupEffect(Vector3 position, FishRarity rarity)
    {
        if (currentQuality < VFXQuality.Low) return;

        ParticleSystem glow = vfxManager.SpawnEffect("inventory_pickup_glow", position);
        if (glow != null && rarity >= FishRarity.Rare)
        {
            var main = glow.main;
            main.startColor = GetRarityColor(rarity);
        }
    }

    public void CreatePlacementFeedback(Vector3 position, bool isValid)
    {
        if (currentQuality < VFXQuality.Medium) return;

        string effectID = isValid ? "inventory_valid_placement" : "inventory_invalid_placement";
        vfxManager.SpawnEffect(effectID, position);
    }

    public void CreateSellEffect(Vector3 position, float value)
    {
        if (currentQuality < VFXQuality.Low) return;

        ParticleSystem coins = vfxManager.SpawnEffect("inventory_sell_coins", position);
        if (coins != null && value > 100f)
        {
            var emission = coins.emission;
            emission.rateOverTime = Mathf.Min(value / 10f, 100f);
        }
    }

    public void CreateCraftEffect(Vector3 position, string itemID)
    {
        if (currentQuality < VFXQuality.Medium) return;

        vfxManager.SpawnEffect("inventory_craft_sparkles", position);

        if (currentQuality >= VFXQuality.High)
        {
            vfxManager.SpawnEffect("inventory_craft_success", position);
        }
    }

    private Color GetRarityColor(FishRarity rarity)
    {
        switch (rarity)
        {
            case FishRarity.Uncommon: return Color.green;
            case FishRarity.Rare: return Color.blue;
            case FishRarity.Legendary: return Color.yellow;
            case FishRarity.Aberrant: return new Color(0.5f, 0f, 0.5f);
            default: return Color.white;
        }
    }

    private void OnItemPickedUp(ItemPickupData data)
    {
        CreateItemPickupEffect(data.position, data.rarity);
    }

    private void OnItemSold(ItemSoldData data)
    {
        CreateSellEffect(data.position, data.value);
    }

    private void OnItemCrafted(ItemCraftedData data)
    {
        CreateCraftEffect(data.position, data.itemID);
    }

    public void SetQuality(VFXQuality quality)
    {
        currentQuality = quality;
    }
}

[System.Serializable]
public struct ItemPickupData
{
    public string itemID;
    public Vector3 position;
    public FishRarity rarity;
}

[System.Serializable]
public struct ItemSoldData
{
    public string itemID;
    public Vector3 position;
    public float value;
}

[System.Serializable]
public struct ItemCraftedData
{
    public string itemID;
    public Vector3 position;
}

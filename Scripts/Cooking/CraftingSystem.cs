using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages crafting of bait, tools, and upgrades from fish materials.
/// Handles material extraction from caught fish and blueprint discovery.
/// Works alongside CookingSystem for the complete production chain.
/// </summary>
public class CraftingSystem : MonoBehaviour
{
    public static CraftingSystem Instance { get; private set; }

    [Header("Material Storage")]
    [SerializeField] private Dictionary<string, int> materials = new Dictionary<string, int>();

    [Header("Blueprint Discovery")]
    [SerializeField] private List<string> discoveredBlueprints = new List<string>();

    [Header("Settings")]
    [Tooltip("Material drop rates by fish rarity")]
    [SerializeField] private MaterialDropRates dropRates = new MaterialDropRates();

    [Tooltip("Show debug logs")]
    [SerializeField] private bool debugMode = false;

    // Events
    public event System.Action<string, int> OnMaterialExtracted;
    public event System.Action<RecipeData> OnBlueprintDiscovered;
    public event System.Action<string, int, int> OnMaterialChanged; // materialID, oldAmount, newAmount

    // Properties
    public Dictionary<string, int> Materials => new Dictionary<string, int>(materials);
    public int DiscoveredBlueprintCount => discoveredBlueprints.Count;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeMaterials();
    }

    private void Start()
    {
        // Subscribe to events
        EventSystem.Subscribe<SaveData>("GatheringSaveData", OnGatheringSaveData);
        EventSystem.Subscribe<SaveData>("ApplyingSaveData", OnApplyingSaveData);
        EventSystem.Subscribe<FishCaughtEventData>("FishCaught", OnFishCaught);
        EventSystem.Subscribe<MaterialConsumption>("ConsumeMaterial", OnConsumeMaterial);
    }

    private void OnDestroy()
    {
        EventSystem.Unsubscribe<SaveData>("GatheringSaveData", OnGatheringSaveData);
        EventSystem.Unsubscribe<SaveData>("ApplyingSaveData", OnApplyingSaveData);
        EventSystem.Unsubscribe<FishCaughtEventData>("FishCaught", OnFishCaught);
        EventSystem.Unsubscribe<MaterialConsumption>("ConsumeMaterial", OnConsumeMaterial);
    }

    /// <summary>
    /// Initializes the material storage dictionary.
    /// </summary>
    private void InitializeMaterials()
    {
        materials.Clear();

        // Initialize all material types
        string[] materialTypes = new string[]
        {
            "fish_scales", "fish_bones", "fish_oil", "fish_guts",
            "fish_teeth", "fish_eyes", "aberrant_essence", "rare_pearl",
            "sea_crystal", "ancient_scale", "void_fragment", "scrap_metal"
        };

        foreach (var material in materialTypes)
        {
            materials[material] = 0;
        }
    }

    /// <summary>
    /// Extracts materials from a caught fish.
    /// </summary>
    public Dictionary<string, int> ExtractMaterialsFromFish(FishCaughtEventData fishData)
    {
        Dictionary<string, int> extracted = new Dictionary<string, int>();

        // Base materials (all fish drop these)
        extracted["fish_scales"] = Random.Range(1, 4);
        extracted["fish_bones"] = Random.Range(1, 3);
        extracted["fish_oil"] = Random.Range(1, 2);

        // Rarity-based materials
        switch (fishData.rarity)
        {
            case FishRarity.Common:
                extracted["fish_guts"] = Random.Range(1, 3);
                break;

            case FishRarity.Uncommon:
                extracted["fish_guts"] = Random.Range(2, 4);
                extracted["fish_teeth"] = Random.Range(1, 2);
                break;

            case FishRarity.Rare:
                extracted["fish_guts"] = Random.Range(2, 5);
                extracted["fish_teeth"] = Random.Range(1, 3);
                extracted["fish_eyes"] = Random.Range(1, 2);
                extracted["rare_pearl"] = Random.Range(0, 2); // Chance
                break;

            case FishRarity.Epic:
                extracted["fish_teeth"] = Random.Range(2, 4);
                extracted["fish_eyes"] = Random.Range(1, 3);
                extracted["rare_pearl"] = Random.Range(1, 3);
                extracted["sea_crystal"] = Random.Range(1, 2);
                break;

            case FishRarity.Legendary:
                extracted["fish_teeth"] = Random.Range(3, 6);
                extracted["fish_eyes"] = Random.Range(2, 4);
                extracted["rare_pearl"] = Random.Range(2, 4);
                extracted["sea_crystal"] = Random.Range(1, 3);
                extracted["ancient_scale"] = Random.Range(1, 2);
                break;

            case FishRarity.Aberrant:
                extracted["aberrant_essence"] = Random.Range(2, 5);
                extracted["void_fragment"] = Random.Range(1, 3);
                extracted["fish_eyes"] = Random.Range(1, 2);
                break;
        }

        // Add aberrant bonus if aberrant variant
        if (fishData.isAberrant)
        {
            extracted["aberrant_essence"] = Random.Range(1, 3);
        }

        // Add materials to storage
        foreach (var kvp in extracted)
        {
            AddMaterial(kvp.Key, kvp.Value);
        }

        if (debugMode)
        {
            Debug.Log($"[CraftingSystem] Extracted {extracted.Count} material types from {fishData.fishID}");
        }

        return extracted;
    }

    /// <summary>
    /// Adds material to storage.
    /// </summary>
    public void AddMaterial(string materialID, int amount)
    {
        if (!materials.ContainsKey(materialID))
        {
            materials[materialID] = 0;
        }

        int oldAmount = materials[materialID];
        materials[materialID] += amount;

        OnMaterialExtracted?.Invoke(materialID, amount);
        OnMaterialChanged?.Invoke(materialID, oldAmount, materials[materialID]);

        EventSystem.Publish("MaterialAdded", new MaterialChangeData
        {
            materialID = materialID,
            amount = amount,
            newTotal = materials[materialID]
        });
    }

    /// <summary>
    /// Removes material from storage.
    /// </summary>
    public bool RemoveMaterial(string materialID, int amount)
    {
        if (!materials.ContainsKey(materialID))
            return false;

        if (materials[materialID] < amount)
            return false;

        int oldAmount = materials[materialID];
        materials[materialID] -= amount;

        OnMaterialChanged?.Invoke(materialID, oldAmount, materials[materialID]);

        EventSystem.Publish("MaterialRemoved", new MaterialChangeData
        {
            materialID = materialID,
            amount = amount,
            newTotal = materials[materialID]
        });

        return true;
    }

    /// <summary>
    /// Gets the amount of a specific material.
    /// </summary>
    public int GetMaterialCount(string materialID)
    {
        return materials.TryGetValue(materialID, out int count) ? count : 0;
    }

    /// <summary>
    /// Checks if player has enough of a material.
    /// </summary>
    public bool HasMaterial(string materialID, int amount)
    {
        return GetMaterialCount(materialID) >= amount;
    }

    /// <summary>
    /// Discovers a crafting blueprint.
    /// </summary>
    public bool DiscoverBlueprint(string recipeID)
    {
        if (discoveredBlueprints.Contains(recipeID))
            return false;

        discoveredBlueprints.Add(recipeID);

        // Try to unlock in cooking system
        if (CookingSystem.Instance != null)
        {
            RecipeData recipe = CookingSystem.Instance.GetRecipe(recipeID);
            if (recipe != null)
            {
                CookingSystem.Instance.UnlockRecipe(recipeID);
                OnBlueprintDiscovered?.Invoke(recipe);
                EventSystem.Publish("BlueprintDiscovered", recipe);

                if (debugMode)
                    Debug.Log($"[CraftingSystem] Discovered blueprint: {recipe.recipeName}");
            }
        }

        return true;
    }

    /// <summary>
    /// Attempts to discover a random blueprint based on materials collected.
    /// </summary>
    public void AttemptBlueprintDiscovery()
    {
        if (CookingSystem.Instance == null)
            return;

        // Get all craftable recipes (non-meals)
        List<RecipeData> craftableRecipes = new List<RecipeData>();
        craftableRecipes.AddRange(CookingSystem.Instance.GetRecipesByType(RecipeType.Bait));
        craftableRecipes.AddRange(CookingSystem.Instance.GetRecipesByType(RecipeType.Tool));
        craftableRecipes.AddRange(CookingSystem.Instance.GetRecipesByType(RecipeType.Upgrade));

        // Filter to undiscovered recipes that can be discovered
        List<RecipeData> undiscovered = craftableRecipes
            .Where(r => !discoveredBlueprints.Contains(r.recipeID) && r.canBeDiscovered)
            .ToList();

        if (undiscovered.Count == 0)
            return;

        // 5% chance per update to discover a blueprint
        if (Random.Range(0f, 1f) < 0.05f)
        {
            RecipeData discovered = undiscovered[Random.Range(0, undiscovered.Count)];
            DiscoverBlueprint(discovered.recipeID);
        }
    }

    /// <summary>
    /// Gets all craftable recipes the player can currently make.
    /// </summary>
    public List<RecipeData> GetCraftableRecipes()
    {
        if (CookingSystem.Instance == null)
            return new List<RecipeData>();

        List<RecipeData> craftable = new List<RecipeData>();

        // Check all unlocked non-meal recipes
        foreach (var recipe in CookingSystem.Instance.GetUnlockedRecipes())
        {
            if (recipe.recipeType == RecipeType.Meal)
                continue;

            // Check if we have materials
            if (CookingSystem.Instance.HasIngredients(recipe))
            {
                craftable.Add(recipe);
            }
        }

        return craftable;
    }

    /// <summary>
    /// Gets material info for UI display.
    /// </summary>
    public MaterialInfo GetMaterialInfo(string materialID)
    {
        return new MaterialInfo
        {
            materialID = materialID,
            displayName = GetMaterialDisplayName(materialID),
            description = GetMaterialDescription(materialID),
            count = GetMaterialCount(materialID),
            icon = GetMaterialIcon(materialID)
        };
    }

    /// <summary>
    /// Gets display name for a material.
    /// </summary>
    private string GetMaterialDisplayName(string materialID)
    {
        Dictionary<string, string> names = new Dictionary<string, string>
        {
            { "fish_scales", "Fish Scales" },
            { "fish_bones", "Fish Bones" },
            { "fish_oil", "Fish Oil" },
            { "fish_guts", "Fish Guts" },
            { "fish_teeth", "Fish Teeth" },
            { "fish_eyes", "Fish Eyes" },
            { "aberrant_essence", "Aberrant Essence" },
            { "rare_pearl", "Rare Pearl" },
            { "sea_crystal", "Sea Crystal" },
            { "ancient_scale", "Ancient Scale" },
            { "void_fragment", "Void Fragment" },
            { "scrap_metal", "Scrap Metal" }
        };

        return names.TryGetValue(materialID, out string name) ? name : materialID;
    }

    /// <summary>
    /// Gets description for a material.
    /// </summary>
    private string GetMaterialDescription(string materialID)
    {
        Dictionary<string, string> descriptions = new Dictionary<string, string>
        {
            { "fish_scales", "Common scales from any fish. Used in basic crafting." },
            { "fish_bones", "Sturdy bones. Useful for tool reinforcement." },
            { "fish_oil", "Slippery oil. Can be refined into fuel." },
            { "fish_guts", "Unpleasant but useful for making bait." },
            { "fish_teeth", "Sharp teeth from predatory fish." },
            { "fish_eyes", "Peculiar eyes that seem to watch you." },
            { "aberrant_essence", "Glowing essence from mutated fish. Handle with care." },
            { "rare_pearl", "Beautiful pearl from rare fish. Highly valuable." },
            { "sea_crystal", "Crystallized sea minerals. Very rare." },
            { "ancient_scale", "Scale from an ancient legendary fish." },
            { "void_fragment", "Fragment of something from beyond. Unsettling." },
            { "scrap_metal", "Salvaged metal parts. Found in debris." }
        };

        return descriptions.TryGetValue(materialID, out string desc) ? desc : "Unknown material.";
    }

    /// <summary>
    /// Gets icon sprite for a material (would load from Resources).
    /// </summary>
    private Sprite GetMaterialIcon(string materialID)
    {
        // This would load the actual sprite from Resources
        // return Resources.Load<Sprite>($"Materials/{materialID}");
        return null;
    }

    // ===== Event Handlers =====

    private void OnFishCaught(FishCaughtEventData data)
    {
        // Extract materials from caught fish
        ExtractMaterialsFromFish(data);

        // Attempt blueprint discovery
        AttemptBlueprintDiscovery();
    }

    private void OnConsumeMaterial(MaterialConsumption consumption)
    {
        RemoveMaterial(consumption.materialID, consumption.quantity);
    }

    // ===== Save/Load Integration =====

    private void OnGatheringSaveData(SaveData saveData)
    {
        saveData.craftingData = new CraftingData
        {
            materials = new Dictionary<string, int>(materials),
            discoveredBlueprints = new List<string>(discoveredBlueprints)
        };

        if (debugMode)
            Debug.Log($"[CraftingSystem] Saved {materials.Count} material types and {discoveredBlueprints.Count} blueprints");
    }

    private void OnApplyingSaveData(SaveData saveData)
    {
        materials.Clear();
        discoveredBlueprints.Clear();

        if (saveData.craftingData != null)
        {
            // Load materials
            foreach (var kvp in saveData.craftingData.materials)
            {
                materials[kvp.Key] = kvp.Value;
            }

            // Load blueprints
            discoveredBlueprints.AddRange(saveData.craftingData.discoveredBlueprints);

            if (debugMode)
                Debug.Log($"[CraftingSystem] Loaded {materials.Count} material types and {discoveredBlueprints.Count} blueprints");
        }
        else
        {
            // Initialize with defaults
            InitializeMaterials();
        }
    }

    // ===== Debug Methods =====

    [ContextMenu("Debug: Add 100 of All Materials")]
    public void DebugAddAllMaterials()
    {
        foreach (var key in materials.Keys.ToList())
        {
            AddMaterial(key, 100);
        }

        Debug.Log("[CraftingSystem] Added 100 of all materials");
    }

    [ContextMenu("Debug: Print Material Inventory")]
    public void DebugPrintMaterials()
    {
        Debug.Log("=== MATERIAL INVENTORY ===");

        foreach (var kvp in materials.OrderByDescending(x => x.Value))
        {
            if (kvp.Value > 0)
            {
                Debug.Log($"{GetMaterialDisplayName(kvp.Key)}: {kvp.Value}");
            }
        }
    }

    [ContextMenu("Debug: Discover All Blueprints")]
    public void DebugDiscoverAllBlueprints()
    {
        if (CookingSystem.Instance == null)
            return;

        List<RecipeData> craftableRecipes = new List<RecipeData>();
        craftableRecipes.AddRange(CookingSystem.Instance.GetRecipesByType(RecipeType.Bait));
        craftableRecipes.AddRange(CookingSystem.Instance.GetRecipesByType(RecipeType.Tool));
        craftableRecipes.AddRange(CookingSystem.Instance.GetRecipesByType(RecipeType.Upgrade));

        foreach (var recipe in craftableRecipes)
        {
            DiscoverBlueprint(recipe.recipeID);
        }

        Debug.Log($"[CraftingSystem] Discovered all {discoveredBlueprints.Count} blueprints");
    }
}

/// <summary>
/// Material drop rate configuration.
/// </summary>
[System.Serializable]
public class MaterialDropRates
{
    [Range(0f, 1f)] public float commonDropChance = 1.0f;
    [Range(0f, 1f)] public float uncommonDropChance = 0.8f;
    [Range(0f, 1f)] public float rareDropChance = 0.6f;
    [Range(0f, 1f)] public float epicDropChance = 0.4f;
    [Range(0f, 1f)] public float legendaryDropChance = 0.3f;
    [Range(0f, 1f)] public float aberrantDropChance = 0.5f;
}

/// <summary>
/// Material information for UI display.
/// </summary>
[System.Serializable]
public class MaterialInfo
{
    public string materialID;
    public string displayName;
    public string description;
    public int count;
    public Sprite icon;
}

/// <summary>
/// Data for material change events.
/// </summary>
[System.Serializable]
public class MaterialChangeData
{
    public string materialID;
    public int amount;
    public int newTotal;
}

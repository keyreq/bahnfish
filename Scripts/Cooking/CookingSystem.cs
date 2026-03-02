using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Central cooking and recipe management system for Bahnfish.
/// Manages recipe database, cooking operations, and recipe unlocking.
/// Integrates with InventoryManager, MealBuffSystem, and save system.
/// </summary>
public class CookingSystem : MonoBehaviour
{
    public static CookingSystem Instance { get; private set; }

    [Header("Recipe Database")]
    [SerializeField] private List<RecipeData> allRecipes = new List<RecipeData>();

    [Header("Cooking State")]
    [SerializeField] private CookingOperation currentCooking = null;
    [SerializeField] private List<string> unlockedRecipes = new List<string>();

    [Header("Settings")]
    [Tooltip("Allow recipe discovery through experimentation")]
    [SerializeField] private bool allowExperimentation = true;

    [Tooltip("Show debug logs")]
    [SerializeField] private bool debugMode = false;

    // Cached lookups
    private Dictionary<string, RecipeData> recipeByID = new Dictionary<string, RecipeData>();
    private Dictionary<RecipeType, List<RecipeData>> recipesByType = new Dictionary<RecipeType, List<RecipeData>>();

    // Events
    public event System.Action<RecipeData> OnRecipeUnlocked;
    public event System.Action<RecipeData> OnCookingStarted;
    public event System.Action<RecipeData, int> OnCookingCompleted;
    public event System.Action<RecipeData> OnCookingFailed;

    // Properties
    public bool IsCooking => currentCooking != null;
    public CookingOperation CurrentCooking => currentCooking;
    public int UnlockedRecipeCount => unlockedRecipes.Count;
    public int TotalRecipeCount => allRecipes.Count;

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

        LoadAllRecipes();
        BuildRecipeLookups();
    }

    private void Start()
    {
        // Subscribe to events
        EventSystem.Subscribe<SaveData>("GatheringSaveData", OnGatheringSaveData);
        EventSystem.Subscribe<SaveData>("ApplyingSaveData", OnApplyingSaveData);
        EventSystem.Subscribe<FishCaughtEventData>("FishCaught", OnFishCaught);
        EventSystem.Subscribe<string>("QuestCompleted", OnQuestCompleted);

        UnlockDefaultRecipes();
    }

    private void Update()
    {
        UpdateCookingProgress();
    }

    private void OnDestroy()
    {
        EventSystem.Unsubscribe<SaveData>("GatheringSaveData", OnGatheringSaveData);
        EventSystem.Unsubscribe<SaveData>("ApplyingSaveData", OnApplyingSaveData);
        EventSystem.Unsubscribe<FishCaughtEventData>("FishCaught", OnFishCaught);
        EventSystem.Unsubscribe<string>("QuestCompleted", OnQuestCompleted);
    }

    /// <summary>
    /// Loads all recipe ScriptableObjects from Resources.
    /// </summary>
    private void LoadAllRecipes()
    {
        RecipeData[] loadedRecipes = Resources.LoadAll<RecipeData>("Recipes");

        if (loadedRecipes.Length == 0)
        {
            Debug.LogWarning("[CookingSystem] No recipes found in Resources/Recipes folder!");
            return;
        }

        allRecipes.Clear();
        allRecipes.AddRange(loadedRecipes);

        if (debugMode)
            Debug.Log($"[CookingSystem] Loaded {allRecipes.Count} recipes");
    }

    /// <summary>
    /// Builds lookup dictionaries for fast recipe queries.
    /// </summary>
    private void BuildRecipeLookups()
    {
        recipeByID.Clear();
        recipesByType.Clear();

        // Initialize type lists
        foreach (RecipeType type in System.Enum.GetValues(typeof(RecipeType)))
        {
            recipesByType[type] = new List<RecipeData>();
        }

        // Populate lookups
        foreach (var recipe in allRecipes)
        {
            if (recipe == null) continue;

            // ID lookup
            if (!recipeByID.ContainsKey(recipe.recipeID))
            {
                recipeByID[recipe.recipeID] = recipe;
            }
            else
            {
                Debug.LogWarning($"[CookingSystem] Duplicate recipe ID: {recipe.recipeID}");
            }

            // Type lookup
            recipesByType[recipe.recipeType].Add(recipe);
        }
    }

    /// <summary>
    /// Unlocks recipes that are marked as default.
    /// </summary>
    private void UnlockDefaultRecipes()
    {
        foreach (var recipe in allRecipes)
        {
            if (recipe.unlockedByDefault && !unlockedRecipes.Contains(recipe.recipeID))
            {
                UnlockRecipe(recipe.recipeID, false);
            }
        }
    }

    /// <summary>
    /// Starts cooking a recipe.
    /// </summary>
    public bool StartCooking(string recipeID)
    {
        if (IsCooking)
        {
            Debug.LogWarning("[CookingSystem] Already cooking something!");
            return false;
        }

        RecipeData recipe = GetRecipe(recipeID);

        if (recipe == null)
        {
            Debug.LogWarning($"[CookingSystem] Recipe not found: {recipeID}");
            return false;
        }

        if (!IsRecipeUnlocked(recipeID))
        {
            Debug.LogWarning($"[CookingSystem] Recipe not unlocked: {recipe.recipeName}");
            return false;
        }

        // Check ingredients
        if (!HasIngredients(recipe))
        {
            Debug.LogWarning($"[CookingSystem] Missing ingredients for {recipe.recipeName}");
            EventSystem.Publish("MissingIngredients", recipe);
            return false;
        }

        // Check money cost
        if (recipe.baseCost > 0)
        {
            SaveData saveData = SaveManager.Instance?.GetCurrentSaveData();
            if (saveData != null && saveData.money < recipe.baseCost)
            {
                Debug.LogWarning($"[CookingSystem] Not enough money to cook {recipe.recipeName}");
                EventSystem.Publish("InsufficientFunds", recipe);
                return false;
            }
        }

        // Consume ingredients
        if (!ConsumeIngredients(recipe))
        {
            Debug.LogWarning($"[CookingSystem] Failed to consume ingredients");
            return false;
        }

        // Deduct money cost
        if (recipe.baseCost > 0)
        {
            EventSystem.Publish("SpendMoney", recipe.baseCost);
        }

        // Start cooking operation
        currentCooking = new CookingOperation
        {
            recipe = recipe,
            startTime = Time.time,
            duration = recipe.craftTime,
            successRate = recipe.successRate
        };

        OnCookingStarted?.Invoke(recipe);
        EventSystem.Publish("CookingStarted", recipe);

        if (debugMode)
            Debug.Log($"[CookingSystem] Started cooking {recipe.recipeName} ({recipe.craftTime}s)");

        return true;
    }

    /// <summary>
    /// Updates the current cooking operation.
    /// </summary>
    private void UpdateCookingProgress()
    {
        if (!IsCooking) return;

        currentCooking.elapsedTime = Time.time - currentCooking.startTime;

        // Check if cooking is complete
        if (currentCooking.elapsedTime >= currentCooking.duration)
        {
            CompleteCooking();
        }
    }

    /// <summary>
    /// Completes the current cooking operation.
    /// </summary>
    private void CompleteCooking()
    {
        if (!IsCooking) return;

        RecipeData recipe = currentCooking.recipe;

        // Determine success
        bool success = Random.Range(0f, 100f) <= currentCooking.successRate;

        if (success)
        {
            // Create output items
            int outputQty = recipe.outputQuantity;

            if (recipe.recipeType == RecipeType.Meal)
            {
                // For meals, consume immediately and apply buffs
                ConsumeMeal(recipe);
            }
            else
            {
                // For other items, add to inventory
                AddCraftedItemsToInventory(recipe, outputQty);
            }

            OnCookingCompleted?.Invoke(recipe, outputQty);
            EventSystem.Publish("CookingCompleted", new CookingResult
            {
                recipe = recipe,
                success = true,
                outputQuantity = outputQty
            });

            if (debugMode)
                Debug.Log($"[CookingSystem] Successfully cooked {recipe.recipeName}!");
        }
        else
        {
            // Cooking failed
            OnCookingFailed?.Invoke(recipe);
            EventSystem.Publish("CookingFailed", recipe);

            if (debugMode)
                Debug.Log($"[CookingSystem] Failed to cook {recipe.recipeName}");
        }

        currentCooking = null;
    }

    /// <summary>
    /// Cancels the current cooking operation.
    /// </summary>
    public void CancelCooking()
    {
        if (!IsCooking) return;

        RecipeData recipe = currentCooking.recipe;
        currentCooking = null;

        EventSystem.Publish("CookingCancelled", recipe);

        if (debugMode)
            Debug.Log($"[CookingSystem] Cancelled cooking {recipe.recipeName}");
    }

    /// <summary>
    /// Consumes a meal and applies its buffs.
    /// </summary>
    private void ConsumeMeal(RecipeData recipe)
    {
        if (MealBuffSystem.Instance == null)
        {
            Debug.LogWarning("[CookingSystem] MealBuffSystem not found!");
            return;
        }

        foreach (var buff in recipe.buffs)
        {
            MealBuffSystem.Instance.ApplyBuff(buff, recipe.recipeName);
        }

        EventSystem.Publish("MealConsumed", recipe);
    }

    /// <summary>
    /// Adds crafted items to the player's inventory.
    /// </summary>
    private void AddCraftedItemsToInventory(RecipeData recipe, int quantity)
    {
        // This would integrate with your item system
        // For now, just publish an event
        EventSystem.Publish("ItemCrafted", new ItemCraftedData
        {
            itemID = recipe.recipeID,
            itemName = recipe.recipeName,
            quantity = quantity,
            value = recipe.outputValue
        });
    }

    /// <summary>
    /// Checks if the player has all required ingredients.
    /// </summary>
    public bool HasIngredients(RecipeData recipe)
    {
        if (InventoryManager.Instance == null)
            return false;

        Dictionary<string, int> materials = GetPlayerMaterials();
        List<InventoryItem> inventory = InventoryManager.Instance.GetAllItems();

        return recipe.HasAllIngredients(materials, inventory);
    }

    /// <summary>
    /// Consumes ingredients from player inventory.
    /// </summary>
    private bool ConsumeIngredients(RecipeData recipe)
    {
        if (InventoryManager.Instance == null)
            return false;

        foreach (var ingredient in recipe.ingredients)
        {
            if (ingredient.ingredientType == IngredientType.Fish)
            {
                // Remove fish from inventory
                if (!ConsumeFishIngredient(ingredient))
                    return false;
            }
            else if (ingredient.ingredientType == IngredientType.Material)
            {
                // Remove materials
                if (!ConsumeMaterialIngredient(ingredient))
                    return false;
            }
            else if (ingredient.ingredientType == IngredientType.Item)
            {
                // Remove items
                if (!ConsumeItemIngredient(ingredient))
                    return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Consumes fish ingredients from inventory.
    /// </summary>
    private bool ConsumeFishIngredient(RecipeIngredient ingredient)
    {
        List<InventoryItem> inventory = InventoryManager.Instance.GetAllItems();
        int neededCount = ingredient.quantity;
        int consumedCount = 0;

        for (int i = inventory.Count - 1; i >= 0 && consumedCount < neededCount; i--)
        {
            InventoryItem item = inventory[i];

            if (item.ItemData is FishInventoryItem fishItem)
            {
                bool matches = false;

                // Check species match
                if (!string.IsNullOrEmpty(ingredient.fishSpeciesID))
                {
                    matches = fishItem.FishSpecies == ingredient.fishSpeciesID;
                }
                // Check rarity match
                else if (ingredient.fishRarity.HasValue)
                {
                    matches = fishItem.Rarity == ingredient.fishRarity.Value;
                }

                if (matches)
                {
                    InventoryManager.Instance.RemoveItem(item);
                    consumedCount++;
                }
            }
        }

        return consumedCount >= neededCount;
    }

    /// <summary>
    /// Consumes material ingredients.
    /// </summary>
    private bool ConsumeMaterialIngredient(RecipeIngredient ingredient)
    {
        // This would integrate with a material storage system
        // For now, publish an event
        EventSystem.Publish("ConsumeMaterial", new MaterialConsumption
        {
            materialID = ingredient.materialID,
            quantity = ingredient.quantity
        });

        return true;
    }

    /// <summary>
    /// Consumes item ingredients from inventory.
    /// </summary>
    private bool ConsumeItemIngredient(RecipeIngredient ingredient)
    {
        List<InventoryItem> inventory = InventoryManager.Instance.GetAllItems();
        int neededCount = ingredient.quantity;
        int consumedCount = 0;

        for (int i = inventory.Count - 1; i >= 0 && consumedCount < neededCount; i--)
        {
            if (inventory[i].ItemID == ingredient.itemID)
            {
                InventoryManager.Instance.RemoveItem(inventory[i]);
                consumedCount++;
            }
        }

        return consumedCount >= neededCount;
    }

    /// <summary>
    /// Gets the player's material counts (would integrate with material system).
    /// </summary>
    private Dictionary<string, int> GetPlayerMaterials()
    {
        // This would integrate with a material storage system
        // For now, return empty dictionary
        return new Dictionary<string, int>();
    }

    /// <summary>
    /// Unlocks a recipe by ID.
    /// </summary>
    public bool UnlockRecipe(string recipeID, bool notify = true)
    {
        if (unlockedRecipes.Contains(recipeID))
            return false;

        RecipeData recipe = GetRecipe(recipeID);
        if (recipe == null)
            return false;

        unlockedRecipes.Add(recipeID);

        if (notify)
        {
            OnRecipeUnlocked?.Invoke(recipe);
            EventSystem.Publish("RecipeUnlocked", recipe);

            if (debugMode)
                Debug.Log($"[CookingSystem] Unlocked recipe: {recipe.recipeName}");
        }

        return true;
    }

    /// <summary>
    /// Checks if a recipe is unlocked.
    /// </summary>
    public bool IsRecipeUnlocked(string recipeID)
    {
        return unlockedRecipes.Contains(recipeID);
    }

    /// <summary>
    /// Gets a recipe by ID.
    /// </summary>
    public RecipeData GetRecipe(string recipeID)
    {
        return recipeByID.TryGetValue(recipeID, out RecipeData recipe) ? recipe : null;
    }

    /// <summary>
    /// Gets all recipes of a specific type.
    /// </summary>
    public List<RecipeData> GetRecipesByType(RecipeType type)
    {
        return recipesByType.TryGetValue(type, out List<RecipeData> recipes)
            ? new List<RecipeData>(recipes)
            : new List<RecipeData>();
    }

    /// <summary>
    /// Gets all unlocked recipes.
    /// </summary>
    public List<RecipeData> GetUnlockedRecipes()
    {
        return allRecipes.Where(r => unlockedRecipes.Contains(r.recipeID)).ToList();
    }

    /// <summary>
    /// Gets all unlocked recipes of a specific type.
    /// </summary>
    public List<RecipeData> GetUnlockedRecipesByType(RecipeType type)
    {
        return GetRecipesByType(type).Where(r => IsRecipeUnlocked(r.recipeID)).ToList();
    }

    /// <summary>
    /// Checks and unlocks recipes based on current game state.
    /// </summary>
    public void CheckRecipeUnlocks(SaveData saveData)
    {
        foreach (var recipe in allRecipes)
        {
            if (!IsRecipeUnlocked(recipe.recipeID) && recipe.IsUnlocked(saveData))
            {
                UnlockRecipe(recipe.recipeID);
            }
        }
    }

    // ===== Event Handlers =====

    private void OnFishCaught(FishCaughtEventData data)
    {
        // Check for recipe unlocks based on caught fish
        if (SaveManager.Instance != null)
        {
            CheckRecipeUnlocks(SaveManager.Instance.GetCurrentSaveData());
        }
    }

    private void OnQuestCompleted(string questID)
    {
        // Check for recipe unlocks based on completed quest
        if (SaveManager.Instance != null)
        {
            CheckRecipeUnlocks(SaveManager.Instance.GetCurrentSaveData());
        }
    }

    // ===== Save/Load Integration =====

    private void OnGatheringSaveData(SaveData saveData)
    {
        saveData.cookingData = new CookingData
        {
            unlockedRecipes = new List<string>(unlockedRecipes)
        };

        if (debugMode)
            Debug.Log($"[CookingSystem] Saved {unlockedRecipes.Count} unlocked recipes");
    }

    private void OnApplyingSaveData(SaveData saveData)
    {
        unlockedRecipes.Clear();

        if (saveData.cookingData != null)
        {
            unlockedRecipes.AddRange(saveData.cookingData.unlockedRecipes);

            if (debugMode)
                Debug.Log($"[CookingSystem] Loaded {unlockedRecipes.Count} unlocked recipes");
        }
        else
        {
            // No saved data, unlock defaults
            UnlockDefaultRecipes();
        }
    }

    // ===== Debug Methods =====

    [ContextMenu("Debug: Unlock All Recipes")]
    public void DebugUnlockAllRecipes()
    {
        foreach (var recipe in allRecipes)
        {
            UnlockRecipe(recipe.recipeID, false);
        }

        Debug.Log($"[CookingSystem] Unlocked all {allRecipes.Count} recipes");
    }

    [ContextMenu("Debug: Print Recipe Stats")]
    public void DebugPrintRecipeStats()
    {
        Debug.Log("=== COOKING SYSTEM STATS ===");
        Debug.Log($"Total Recipes: {allRecipes.Count}");
        Debug.Log($"Unlocked: {unlockedRecipes.Count}");

        foreach (RecipeType type in System.Enum.GetValues(typeof(RecipeType)))
        {
            int count = recipesByType[type].Count;
            int unlocked = recipesByType[type].Count(r => IsRecipeUnlocked(r.recipeID));
            Debug.Log($"{type}: {unlocked}/{count}");
        }
    }
}

/// <summary>
/// Represents an active cooking operation.
/// </summary>
[System.Serializable]
public class CookingOperation
{
    public RecipeData recipe;
    public float startTime;
    public float duration;
    public float elapsedTime;
    public float successRate;

    public float GetProgress()
    {
        return Mathf.Clamp01(elapsedTime / duration);
    }

    public float GetRemainingTime()
    {
        return Mathf.Max(0f, duration - elapsedTime);
    }
}

/// <summary>
/// Data for cooking completed event.
/// </summary>
[System.Serializable]
public class CookingResult
{
    public RecipeData recipe;
    public bool success;
    public int outputQuantity;
}

/// <summary>
/// Data for item crafted event.
/// </summary>
[System.Serializable]
public class ItemCraftedData
{
    public string itemID;
    public string itemName;
    public int quantity;
    public float value;
}

/// <summary>
/// Data for material consumption event.
/// </summary>
[System.Serializable]
public class MaterialConsumption
{
    public string materialID;
    public int quantity;
}

/// <summary>
/// Data for fish caught event (if not already defined).
/// </summary>
[System.Serializable]
public class FishCaughtEventData
{
    public string fishID;
    public FishRarity rarity;
    public float weight;
    public bool isAberrant;
}

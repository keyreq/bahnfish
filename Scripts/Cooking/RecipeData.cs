using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ScriptableObject that defines a cooking recipe or crafting blueprint.
/// Contains all data needed for cooking meals, crafting bait, tools, and upgrades.
/// </summary>
[CreateAssetMenu(fileName = "NewRecipe", menuName = "Bahnfish/Cooking/Recipe", order = 1)]
public class RecipeData : ScriptableObject
{
    [Header("Basic Info")]
    [Tooltip("Unique identifier for this recipe")]
    public string recipeID = "";

    [Tooltip("Display name of the recipe")]
    public string recipeName = "Unknown Recipe";

    [Tooltip("Type of recipe (meal, bait, tool, upgrade)")]
    public RecipeType recipeType = RecipeType.Meal;

    [Tooltip("Recipe tier/difficulty (1-5)")]
    [Range(1, 5)]
    public int tier = 1;

    [TextArea(3, 6)]
    [Tooltip("Description of the recipe and its effects")]
    public string description = "";

    [Header("Visual")]
    [Tooltip("Icon shown in recipe book and UI")]
    public Sprite icon;

    [Header("Ingredients")]
    [Tooltip("Required ingredients to craft this recipe")]
    public List<RecipeIngredient> ingredients = new List<RecipeIngredient>();

    [Header("Crafting")]
    [Tooltip("Time to cook/craft in real-time seconds")]
    [Range(1f, 120f)]
    public float craftTime = 10f;

    [Tooltip("Base cost to cook/craft (optional money cost)")]
    public float baseCost = 0f;

    [Tooltip("Success rate percentage (100 = always succeeds)")]
    [Range(50f, 100f)]
    public float successRate = 100f;

    [Header("Output")]
    [Tooltip("Number of items produced when crafting")]
    [Range(1, 100)]
    public int outputQuantity = 1;

    [Tooltip("Sell value of the crafted item")]
    public float outputValue = 50f;

    [Header("Meal Buffs (For Meals Only)")]
    [Tooltip("Buffs applied when this meal is consumed")]
    public List<MealBuff> buffs = new List<MealBuff>();

    [Header("Unlock Conditions")]
    [Tooltip("Is this recipe unlocked by default?")]
    public bool unlockedByDefault = false;

    [Tooltip("Quest IDs that unlock this recipe")]
    public List<string> unlockQuests = new List<string>();

    [Tooltip("Fish species that must be caught to unlock")]
    public List<string> unlockFishSpecies = new List<string>();

    [Tooltip("Total money spent required to unlock")]
    public float unlockMoneyRequired = 0f;

    [Tooltip("Total fish caught required to unlock")]
    public int unlockFishCountRequired = 0;

    [Tooltip("Player level required to unlock")]
    public int unlockLevelRequired = 0;

    [Header("Special Properties")]
    [Tooltip("Does this recipe require a special crafting station?")]
    public bool requiresSpecialStation = false;

    [Tooltip("Name of the special station required")]
    public string requiredStation = "";

    [Tooltip("Can this recipe be discovered randomly?")]
    public bool canBeDiscovered = true;

    [Tooltip("Tags for filtering and categorization")]
    public List<string> tags = new List<string>();

    /// <summary>
    /// Checks if the player has all required ingredients.
    /// </summary>
    public bool HasAllIngredients(Dictionary<string, int> playerMaterials, List<InventoryItem> inventory)
    {
        foreach (var ingredient in ingredients)
        {
            if (ingredient.ingredientType == IngredientType.Material)
            {
                // Check materials dictionary
                if (!playerMaterials.ContainsKey(ingredient.materialID) ||
                    playerMaterials[ingredient.materialID] < ingredient.quantity)
                {
                    return false;
                }
            }
            else if (ingredient.ingredientType == IngredientType.Fish)
            {
                // Count matching fish in inventory
                int fishCount = CountMatchingFish(inventory, ingredient);
                if (fishCount < ingredient.quantity)
                {
                    return false;
                }
            }
            else if (ingredient.ingredientType == IngredientType.Item)
            {
                // Check for specific items
                int itemCount = CountMatchingItems(inventory, ingredient.itemID);
                if (itemCount < ingredient.quantity)
                {
                    return false;
                }
            }
        }

        return true;
    }

    /// <summary>
    /// Counts fish matching the ingredient requirements.
    /// </summary>
    private int CountMatchingFish(List<InventoryItem> inventory, RecipeIngredient ingredient)
    {
        int count = 0;

        foreach (var item in inventory)
        {
            if (item.ItemData is FishInventoryItem fishItem)
            {
                // Check if fish matches species requirement
                if (!string.IsNullOrEmpty(ingredient.fishSpeciesID))
                {
                    if (fishItem.FishSpecies == ingredient.fishSpeciesID)
                    {
                        count++;
                    }
                }
                // Check if fish matches rarity requirement
                else if (ingredient.fishRarity.HasValue)
                {
                    if (fishItem.Rarity == ingredient.fishRarity.Value)
                    {
                        count++;
                    }
                }
            }
        }

        return count;
    }

    /// <summary>
    /// Counts items matching the item ID.
    /// </summary>
    private int CountMatchingItems(List<InventoryItem> inventory, string itemID)
    {
        int count = 0;

        foreach (var item in inventory)
        {
            if (item.ItemID == itemID)
            {
                count++;
            }
        }

        return count;
    }

    /// <summary>
    /// Gets the total cost to craft this recipe.
    /// </summary>
    public float GetTotalCost()
    {
        float total = baseCost;

        foreach (var ingredient in ingredients)
        {
            total += ingredient.estimatedValue * ingredient.quantity;
        }

        return total;
    }

    /// <summary>
    /// Checks if all unlock conditions are met.
    /// </summary>
    public bool IsUnlocked(SaveData saveData)
    {
        if (unlockedByDefault)
            return true;

        // Check quest requirements
        foreach (var questID in unlockQuests)
        {
            if (!saveData.completedQuests.Contains(questID))
                return false;
        }

        // Check fish species requirements
        foreach (var fishID in unlockFishSpecies)
        {
            if (!saveData.discoveredFishSpecies.Contains(fishID))
                return false;
        }

        // Check money spent
        if (saveData.totalMoneyEarned < unlockMoneyRequired)
            return false;

        // Check fish count
        if (saveData.totalFishCaught < unlockFishCountRequired)
            return false;

        return true;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Auto-generate ID from name if empty
        if (string.IsNullOrEmpty(recipeID) && !string.IsNullOrEmpty(recipeName))
        {
            recipeID = recipeName.ToLower().Replace(" ", "_");
        }

        // Ensure at least one ingredient
        if (ingredients.Count == 0)
        {
            Debug.LogWarning($"[{recipeName}] Recipe has no ingredients!");
        }

        // Meals should have buffs
        if (recipeType == RecipeType.Meal && buffs.Count == 0)
        {
            Debug.LogWarning($"[{recipeName}] Meal recipe has no buffs!");
        }

        // Validate craft time
        if (craftTime <= 0)
        {
            craftTime = 10f;
        }

        // Validate output quantity
        if (outputQuantity <= 0)
        {
            outputQuantity = 1;
        }
    }
#endif
}

/// <summary>
/// Types of recipes available.
/// </summary>
[System.Serializable]
public enum RecipeType
{
    Meal,       // Food that provides buffs
    Bait,       // Fishing bait items
    Tool,       // Consumable tools (repair kits, fuel)
    Upgrade     // Permanent upgrade items
}

/// <summary>
/// A single ingredient required for a recipe.
/// </summary>
[System.Serializable]
public class RecipeIngredient
{
    [Tooltip("Type of ingredient")]
    public IngredientType ingredientType = IngredientType.Fish;

    [Header("Fish Ingredient")]
    [Tooltip("Specific fish species ID (leave empty for any fish of rarity)")]
    public string fishSpeciesID = "";

    [Tooltip("Fish rarity required (if no specific species)")]
    public FishRarity? fishRarity = null;

    [Header("Material Ingredient")]
    [Tooltip("Material ID (scales, bones, oil, etc.)")]
    public string materialID = "";

    [Header("Item Ingredient")]
    [Tooltip("Specific item ID required")]
    public string itemID = "";

    [Header("Quantity")]
    [Tooltip("Number of this ingredient required")]
    [Range(1, 100)]
    public int quantity = 1;

    [Tooltip("Estimated value per unit (for cost calculation)")]
    public float estimatedValue = 10f;

    public override string ToString()
    {
        string ingredientName = "";

        switch (ingredientType)
        {
            case IngredientType.Fish:
                if (!string.IsNullOrEmpty(fishSpeciesID))
                    ingredientName = fishSpeciesID;
                else if (fishRarity.HasValue)
                    ingredientName = $"{fishRarity.Value} Fish";
                else
                    ingredientName = "Any Fish";
                break;

            case IngredientType.Material:
                ingredientName = materialID;
                break;

            case IngredientType.Item:
                ingredientName = itemID;
                break;
        }

        return $"{quantity}x {ingredientName}";
    }
}

/// <summary>
/// Types of ingredients that can be used in recipes.
/// </summary>
[System.Serializable]
public enum IngredientType
{
    Fish,       // Requires a fish from inventory
    Material,   // Requires crafting materials (extracted from fish)
    Item        // Requires a specific item
}

/// <summary>
/// A buff applied by consuming a meal.
/// </summary>
[System.Serializable]
public class MealBuff
{
    [Tooltip("Type of buff")]
    public BuffType buffType = BuffType.FishingLuck;

    [Tooltip("Buff strength (percentage or flat value)")]
    [Range(0f, 100f)]
    public float buffStrength = 10f;

    [Tooltip("Duration in real-time seconds")]
    [Range(60f, 3600f)]
    public float duration = 300f; // 5 minutes default

    [Tooltip("Description of what this buff does")]
    public string description = "";

    public override string ToString()
    {
        int minutes = Mathf.FloorToInt(duration / 60f);
        return $"{buffType}: +{buffStrength}% for {minutes}m";
    }
}

/// <summary>
/// Types of buffs that meals can provide.
/// </summary>
[System.Serializable]
public enum BuffType
{
    FishingLuck,        // Increases rare fish spawn rate
    LineStrength,       // Increases tension tolerance
    SpeedBoost,         // Increases boat speed
    SanityShield,       // Reduces sanity drain
    NightVision,        // Increases visibility at night
    CoinMultiplier,     // Increases fish sell values
    XPBoost,            // Increases progression speed
    WeatherResistance   // Reduces weather penalties
}

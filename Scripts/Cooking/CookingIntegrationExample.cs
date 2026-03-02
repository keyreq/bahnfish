using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Example integration script demonstrating how to use the Cooking & Crafting System.
/// This shows practical usage patterns for all major features.
/// Attach this to a GameObject for testing or reference the methods in your own code.
/// </summary>
public class CookingIntegrationExample : MonoBehaviour
{
    [Header("Testing")]
    [SerializeField] private bool runExamplesOnStart = false;

    private void Start()
    {
        if (runExamplesOnStart)
        {
            // Subscribe to events
            SubscribeToEvents();

            // Run examples
            Invoke(nameof(RunAllExamples), 2f); // Wait for systems to initialize
        }
    }

    private void OnDestroy()
    {
        // Always clean up event subscriptions
        UnsubscribeFromEvents();
    }

    /// <summary>
    /// Runs all integration examples.
    /// </summary>
    public void RunAllExamples()
    {
        Debug.Log("=== COOKING & CRAFTING INTEGRATION EXAMPLES ===");

        Example1_CookAMeal();
        Example2_CheckAndApplyBuffs();
        Example3_ExtractMaterials();
        Example4_CraftBait();
        Example5_PreserveFish();
        Example6_UnlockRecipes();
        Example7_InteractWithStation();
    }

    // ===== EXAMPLE 1: Cook a Meal =====

    /// <summary>
    /// Example 1: How to cook a recipe and consume it.
    /// </summary>
    public void Example1_CookAMeal()
    {
        Debug.Log("\n--- Example 1: Cook a Meal ---");

        // Check if cooking system exists
        if (CookingSystem.Instance == null)
        {
            Debug.LogError("CookingSystem not found!");
            return;
        }

        // Get a recipe by ID
        RecipeData recipe = CookingSystem.Instance.GetRecipe("grilled_bass");

        if (recipe == null)
        {
            Debug.LogWarning("Recipe 'grilled_bass' not found. Make sure to create it!");
            return;
        }

        // Check if we have ingredients
        bool hasIngredients = CookingSystem.Instance.HasIngredients(recipe);
        Debug.Log($"Has ingredients for {recipe.recipeName}: {hasIngredients}");

        if (!hasIngredients)
        {
            Debug.Log("Missing ingredients. For testing, you'd add fish to inventory here.");
            return;
        }

        // Start cooking
        bool started = CookingSystem.Instance.StartCooking(recipe.recipeID);

        if (started)
        {
            Debug.Log($"Started cooking {recipe.recipeName}!");
            Debug.Log($"Cooking time: {recipe.craftTime} seconds");

            // Check progress (you'd do this in Update)
            float progress = CookingSystem.Instance.CurrentCooking.GetProgress();
            Debug.Log($"Initial progress: {progress * 100f}%");
        }
        else
        {
            Debug.LogWarning("Failed to start cooking!");
        }
    }

    // ===== EXAMPLE 2: Check and Apply Buffs =====

    /// <summary>
    /// Example 2: How to check active buffs and manually apply buffs.
    /// </summary>
    public void Example2_CheckAndApplyBuffs()
    {
        Debug.Log("\n--- Example 2: Check and Apply Buffs ---");

        if (MealBuffSystem.Instance == null)
        {
            Debug.LogError("MealBuffSystem not found!");
            return;
        }

        // Check active buffs
        Debug.Log($"Active buffs: {MealBuffSystem.Instance.ActiveBuffCount}");

        // Check for specific buff
        bool hasLuck = MealBuffSystem.Instance.HasBuff(BuffType.FishingLuck);
        Debug.Log($"Has Fishing Luck buff: {hasLuck}");

        if (hasLuck)
        {
            float strength = MealBuffSystem.Instance.GetBuffStrength(BuffType.FishingLuck);
            float timeLeft = MealBuffSystem.Instance.GetBuffRemainingTime(BuffType.FishingLuck);
            Debug.Log($"Fishing Luck: +{strength}%, {timeLeft}s remaining");
        }

        // Manually apply a buff (normally done by cooking system)
        MealBuff testBuff = new MealBuff
        {
            buffType = BuffType.SpeedBoost,
            buffStrength = 20f,
            duration = 300f, // 5 minutes
            description = "Test speed boost"
        };

        bool applied = MealBuffSystem.Instance.ApplyBuff(testBuff, "Test Meal");
        Debug.Log($"Applied test buff: {applied}");

        // Get all active buff strengths
        var allBuffs = MealBuffSystem.Instance.GetAllBuffStrengths();
        foreach (var kvp in allBuffs)
        {
            Debug.Log($"{kvp.Key}: +{kvp.Value}%");
        }
    }

    // ===== EXAMPLE 3: Extract Materials from Fish =====

    /// <summary>
    /// Example 3: How to extract materials from a caught fish.
    /// </summary>
    public void Example3_ExtractMaterials()
    {
        Debug.Log("\n--- Example 3: Extract Materials ---");

        if (CraftingSystem.Instance == null)
        {
            Debug.LogError("CraftingSystem not found!");
            return;
        }

        // Simulate catching a rare fish
        FishCaughtEventData fishData = new FishCaughtEventData
        {
            fishID = "golden_trout",
            rarity = FishRarity.Rare,
            weight = 5.2f,
            isAberrant = false
        };

        // Extract materials (normally automatic on FishCaught event)
        Dictionary<string, int> materials = CraftingSystem.Instance.ExtractMaterialsFromFish(fishData);

        Debug.Log($"Extracted {materials.Count} material types:");
        foreach (var kvp in materials)
        {
            Debug.Log($"  {kvp.Key}: {kvp.Value}");
        }

        // Check total material counts
        Debug.Log("\nTotal Materials:");
        Debug.Log($"Fish Scales: {CraftingSystem.Instance.GetMaterialCount("fish_scales")}");
        Debug.Log($"Fish Bones: {CraftingSystem.Instance.GetMaterialCount("fish_bones")}");
        Debug.Log($"Fish Oil: {CraftingSystem.Instance.GetMaterialCount("fish_oil")}");
    }

    // ===== EXAMPLE 4: Craft Bait =====

    /// <summary>
    /// Example 4: How to craft bait from materials.
    /// </summary>
    public void Example4_CraftBait()
    {
        Debug.Log("\n--- Example 4: Craft Bait ---");

        if (CraftingSystem.Instance == null || CookingSystem.Instance == null)
        {
            Debug.LogError("Required systems not found!");
            return;
        }

        // Get all craftable recipes
        List<RecipeData> craftable = CraftingSystem.Instance.GetCraftableRecipes();
        Debug.Log($"Craftable recipes: {craftable.Count}");

        // Find bait recipes
        List<RecipeData> baitRecipes = CookingSystem.Instance.GetRecipesByType(RecipeType.Bait);
        Debug.Log($"Total bait recipes: {baitRecipes.Count}");

        // Try to craft basic worm bait
        RecipeData wormBait = CookingSystem.Instance.GetRecipe("basic_worm");

        if (wormBait != null)
        {
            bool hasIngredients = CookingSystem.Instance.HasIngredients(wormBait);
            Debug.Log($"Can craft {wormBait.recipeName}: {hasIngredients}");

            if (hasIngredients)
            {
                // Start crafting (same as cooking)
                CookingSystem.Instance.StartCooking(wormBait.recipeID);
                Debug.Log($"Started crafting {wormBait.recipeName}!");
            }
        }
        else
        {
            Debug.LogWarning("Basic worm recipe not found. Create it as a RecipeData asset.");
        }
    }

    // ===== EXAMPLE 5: Preserve Fish =====

    /// <summary>
    /// Example 5: How to preserve fish and check decay status.
    /// </summary>
    public void Example5_PreserveFish()
    {
        Debug.Log("\n--- Example 5: Preserve Fish ---");

        if (PreservationSystem.Instance == null)
        {
            Debug.LogError("PreservationSystem not found!");
            return;
        }

        string testFishID = "test_fish_001";

        // Check available preservation methods
        bool hasIceBox = PreservationSystem.Instance.IsMethodUnlocked(PreservationMethod.IceBox);
        bool hasSmoking = PreservationSystem.Instance.IsMethodUnlocked(PreservationMethod.Smoking);
        bool hasFreezing = PreservationSystem.Instance.IsMethodUnlocked(PreservationMethod.Freezing);

        Debug.Log($"Ice Box unlocked: {hasIceBox}");
        Debug.Log($"Smoking unlocked: {hasSmoking}");
        Debug.Log($"Freezing unlocked: {hasFreezing}");

        // Preserve a fish (assuming it's in inventory)
        if (hasIceBox)
        {
            bool preserved = PreservationSystem.Instance.PreserveFish(testFishID, PreservationMethod.IceBox);
            Debug.Log($"Preserved fish in ice box: {preserved}");

            if (preserved)
            {
                // Check preservation status
                float timeLeft = PreservationSystem.Instance.GetRemainingTime(testFishID);
                float quality = PreservationSystem.Instance.GetQualityMultiplier(testFishID);
                PreservationMethod method = PreservationSystem.Instance.GetPreservationMethod(testFishID);

                Debug.Log($"Time remaining: {timeLeft}s");
                Debug.Log($"Quality: {quality * 100f}%");
                Debug.Log($"Method: {method}");
            }
        }

        // Unlock a preservation method
        if (!hasFreezing)
        {
            PreservationSystem.Instance.UnlockMethod(PreservationMethod.Freezing);
            Debug.Log("Unlocked freezing method!");
        }
    }

    // ===== EXAMPLE 6: Unlock Recipes =====

    /// <summary>
    /// Example 6: How to unlock recipes through progression.
    /// </summary>
    public void Example6_UnlockRecipes()
    {
        Debug.Log("\n--- Example 6: Unlock Recipes ---");

        if (CookingSystem.Instance == null)
        {
            Debug.LogError("CookingSystem not found!");
            return;
        }

        // Check unlocked recipe count
        Debug.Log($"Unlocked recipes: {CookingSystem.Instance.UnlockedRecipeCount} / {CookingSystem.Instance.TotalRecipeCount}");

        // Get unlocked recipes by type
        List<RecipeData> unlockedMeals = CookingSystem.Instance.GetUnlockedRecipesByType(RecipeType.Meal);
        List<RecipeData> unlockedBait = CookingSystem.Instance.GetUnlockedRecipesByType(RecipeType.Bait);

        Debug.Log($"Unlocked meals: {unlockedMeals.Count}");
        Debug.Log($"Unlocked bait: {unlockedBait.Count}");

        // Manually unlock a recipe
        RecipeData advancedRecipe = CookingSystem.Instance.GetRecipe("legendary_sushi");

        if (advancedRecipe != null)
        {
            bool wasUnlocked = CookingSystem.Instance.IsRecipeUnlocked(advancedRecipe.recipeID);

            if (!wasUnlocked)
            {
                CookingSystem.Instance.UnlockRecipe(advancedRecipe.recipeID);
                Debug.Log($"Unlocked {advancedRecipe.recipeName}!");
            }
            else
            {
                Debug.Log($"{advancedRecipe.recipeName} already unlocked");
            }
        }

        // Check recipe unlock conditions (requires SaveManager)
        if (SaveManager.Instance != null)
        {
            SaveData saveData = SaveManager.Instance.GetCurrentSaveData();
            if (saveData != null)
            {
                CookingSystem.Instance.CheckRecipeUnlocks(saveData);
                Debug.Log("Checked for recipe unlocks based on progression");
            }
        }
    }

    // ===== EXAMPLE 7: Interact with Cooking Station =====

    /// <summary>
    /// Example 7: How to interact with a cooking station.
    /// </summary>
    public void Example7_InteractWithStation()
    {
        Debug.Log("\n--- Example 7: Interact with Station ---");

        // Find a cooking station in the scene
        CookingStationController station = FindObjectOfType<CookingStationController>();

        if (station == null)
        {
            Debug.LogWarning("No cooking station found in scene. Add one to test!");
            return;
        }

        Debug.Log($"Found station: {station.Type}");
        Debug.Log($"Station active: {station.IsActive}");

        // Get available recipes at this station
        List<RecipeData> available = station.GetAvailableRecipes();
        Debug.Log($"Recipes available at this station: {available.Count}");

        // Open cooking menu (normally triggered by player input)
        station.OpenCookingMenu();
        Debug.Log("Opened cooking menu");

        // Start cooking if a recipe is available
        if (available.Count > 0)
        {
            RecipeData firstRecipe = available[0];
            bool started = station.StartCooking(firstRecipe);

            if (started)
            {
                Debug.Log($"Started cooking {firstRecipe.recipeName} at station");

                // Check progress (you'd do this in Update loop)
                float progress = station.GetCookingProgress();
                float timeLeft = station.GetRemainingTime();
                Debug.Log($"Progress: {progress * 100f}%, Time left: {timeLeft}s");
            }
        }
    }

    // ===== EVENT SUBSCRIPTIONS =====

    /// <summary>
    /// Subscribe to all cooking & crafting events.
    /// </summary>
    private void SubscribeToEvents()
    {
        // Cooking events
        EventSystem.Subscribe<RecipeData>("CookingStarted", OnCookingStarted);
        EventSystem.Subscribe<CookingResult>("CookingCompleted", OnCookingCompleted);
        EventSystem.Subscribe<RecipeData>("CookingFailed", OnCookingFailed);
        EventSystem.Subscribe<RecipeData>("RecipeUnlocked", OnRecipeUnlocked);

        // Buff events
        EventSystem.Subscribe<ActiveBuff>("BuffApplied", OnBuffApplied);
        EventSystem.Subscribe<ActiveBuff>("BuffExpired", OnBuffExpired);

        // Crafting events
        EventSystem.Subscribe<MaterialChangeData>("MaterialAdded", OnMaterialAdded);
        EventSystem.Subscribe<RecipeData>("BlueprintDiscovered", OnBlueprintDiscovered);

        // Preservation events
        EventSystem.Subscribe<FishPreservationData>("FishPreserved", OnFishPreserved);
        EventSystem.Subscribe<string>("FishDecayed", OnFishDecayed);
    }

    /// <summary>
    /// Unsubscribe from all events.
    /// </summary>
    private void UnsubscribeFromEvents()
    {
        EventSystem.Unsubscribe<RecipeData>("CookingStarted", OnCookingStarted);
        EventSystem.Unsubscribe<CookingResult>("CookingCompleted", OnCookingCompleted);
        EventSystem.Unsubscribe<RecipeData>("CookingFailed", OnCookingFailed);
        EventSystem.Unsubscribe<RecipeData>("RecipeUnlocked", OnRecipeUnlocked);

        EventSystem.Unsubscribe<ActiveBuff>("BuffApplied", OnBuffApplied);
        EventSystem.Unsubscribe<ActiveBuff>("BuffExpired", OnBuffExpired);

        EventSystem.Unsubscribe<MaterialChangeData>("MaterialAdded", OnMaterialAdded);
        EventSystem.Unsubscribe<RecipeData>("BlueprintDiscovered", OnBlueprintDiscovered);

        EventSystem.Unsubscribe<FishPreservationData>("FishPreserved", OnFishPreserved);
        EventSystem.Unsubscribe<string>("FishDecayed", OnFishDecayed);
    }

    // ===== EVENT HANDLERS =====

    private void OnCookingStarted(RecipeData recipe)
    {
        Debug.Log($"[Event] Cooking started: {recipe.recipeName}");
    }

    private void OnCookingCompleted(CookingResult result)
    {
        Debug.Log($"[Event] Cooking completed: {result.recipe.recipeName} (x{result.outputQuantity})");
    }

    private void OnCookingFailed(RecipeData recipe)
    {
        Debug.LogWarning($"[Event] Cooking failed: {recipe.recipeName}");
    }

    private void OnRecipeUnlocked(RecipeData recipe)
    {
        Debug.Log($"[Event] Recipe unlocked: {recipe.recipeName}");
    }

    private void OnBuffApplied(ActiveBuff buff)
    {
        Debug.Log($"[Event] Buff applied: {buff.buffType} +{buff.buffStrength}% ({buff.GetFormattedTimeRemaining()})");
    }

    private void OnBuffExpired(ActiveBuff buff)
    {
        Debug.Log($"[Event] Buff expired: {buff.buffType}");
    }

    private void OnMaterialAdded(MaterialChangeData data)
    {
        Debug.Log($"[Event] Material added: {data.materialID} +{data.amount} (Total: {data.newTotal})");
    }

    private void OnBlueprintDiscovered(RecipeData recipe)
    {
        Debug.Log($"[Event] Blueprint discovered: {recipe.recipeName}");
    }

    private void OnFishPreserved(FishPreservationData data)
    {
        Debug.Log($"[Event] Fish preserved: {data.fishID} using {data.method}");
    }

    private void OnFishDecayed(string fishID)
    {
        Debug.LogWarning($"[Event] Fish decayed and removed: {fishID}");
    }
}

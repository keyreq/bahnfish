using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Agent 16: Aquarium & Breeding Specialist - INTEGRATION_EXAMPLE.cs
/// Demonstrates how to integrate the Aquarium & Breeding System with other game systems.
/// This file shows complete workflow examples for common use cases.
/// </summary>
public class AquariumIntegrationExample : MonoBehaviour
{
    // ============================================
    // EXAMPLE 1: Adding Caught Fish to Aquarium
    // ============================================

    /// <summary>
    /// When a fish is caught, offer the player the option to add it to their aquarium.
    /// This would typically be called from the fishing system or UI.
    /// </summary>
    private void Example_AddCaughtFishToAquarium()
    {
        // Subscribe to fish caught event
        EventSystem.Subscribe<Fish>("FishCaught", (Fish caughtFish) =>
        {
            // Get the species data
            FishSpeciesData speciesData = FishDatabase.Instance?.GetFishByID(caughtFish.id);

            if (speciesData == null)
            {
                Debug.LogWarning($"Species data not found for {caughtFish.id}");
                return;
            }

            // Create a DisplayFish from the caught fish
            DisplayFish displayFish = DisplayFish.FromWildCaught(speciesData, caughtFish);

            // In a real implementation, show UI to let player choose tank
            // For this example, we'll use the first available tank with space
            TankInstance availableTank = FindTankWithSpace(displayFish);

            if (availableTank != null)
            {
                // Add to tank
                bool success = AquariumManager.Instance.AddFishToTank(
                    displayFish,
                    availableTank.tankData.tankID
                );

                if (success)
                {
                    Debug.Log($"Added {displayFish.speciesName} to {availableTank.tankData.tankName}!");

                    // Update encyclopedia
                    EventSystem.Publish("EncyclopediaUpdated", caughtFish.id);
                }
                else
                {
                    Debug.LogWarning("Failed to add fish to tank");
                }
            }
            else
            {
                Debug.LogWarning("No available tank with space for this fish");
                // In real game, show "Purchase more tanks" prompt
            }
        });
    }

    /// <summary>
    /// Finds a tank with available space for a fish.
    /// </summary>
    private TankInstance FindTankWithSpace(DisplayFish fish)
    {
        if (AquariumManager.Instance == null)
        {
            return null;
        }

        List<TankInstance> tanks = AquariumManager.Instance.GetOwnedTanks();

        foreach (var tank in tanks)
        {
            // Check if tank has space
            if (tank.GetFishCount() >= tank.GetMaxCapacity())
            {
                continue;
            }

            // Check if tank can hold this species
            FishSpeciesData speciesData = FishDatabase.Instance?.GetFishByID(fish.speciesID);

            if (speciesData != null && tank.tankData.CanHoldFish(speciesData))
            {
                return tank;
            }
        }

        return null;
    }

    // ============================================
    // EXAMPLE 2: Starting a Breeding Pair
    // ============================================

    /// <summary>
    /// Complete workflow for breeding two fish.
    /// </summary>
    private void Example_BreedFish(string fishID1, string fishID2, string tankID)
    {
        // Get the fish
        DisplayFish parent1 = AquariumManager.Instance?.GetDisplayFish(fishID1);
        DisplayFish parent2 = AquariumManager.Instance?.GetDisplayFish(fishID2);

        if (parent1 == null || parent2 == null)
        {
            Debug.LogError("One or both fish not found");
            return;
        }

        // Validate breeding compatibility
        string validationError = BreedingSystem.Instance?.ValidateBreeding(parent1, parent2);

        if (!string.IsNullOrEmpty(validationError))
        {
            Debug.LogWarning($"Cannot breed: {validationError}");
            // Show error message to player
            return;
        }

        // Check genetic compatibility
        float compatibility = GeneticsSystem.CalculateCompatibility(parent1.traits, parent2.traits);
        Debug.Log($"Breeding compatibility: {compatibility:P0}");

        if (compatibility < 0.3f)
        {
            Debug.LogWarning("Low compatibility - breeding may fail or produce weak offspring");
            // Show warning to player, but allow them to proceed
        }

        // Get genetics lab level from tank (improves mutation chance)
        TankInstance tank = AquariumManager.Instance?.GetTank(tankID);
        int geneticsLabLevel = tank?.GetUpgradeLevel(TankUpgradeType.GeneticsLab) ?? 0;

        // Start breeding
        bool success = BreedingSystem.Instance?.StartBreeding(
            parent1,
            parent2,
            tankID,
            geneticsLabLevel
        ) ?? false;

        if (success)
        {
            Debug.Log($"Breeding started between {parent1.speciesName} and {parent2.speciesName}!");
            Debug.Log("Check back in 24 hours for offspring.");

            // Show breeding progress UI
            ShowBreedingProgressUI(parent1, parent2);
        }
        else
        {
            Debug.LogWarning("Breeding failed to start");
        }
    }

    /// <summary>
    /// Subscribe to breeding completion event.
    /// </summary>
    private void Example_ListenForOffspring()
    {
        EventSystem.Subscribe<OffspringBornData>("OffspringBorn", (OffspringBornData data) =>
        {
            DisplayFish offspring = data.offspring;
            GeneticTraits traits = offspring.traits;

            Debug.Log("=== NEW OFFSPRING BORN ===");
            Debug.Log($"Species: {offspring.speciesName}");
            Debug.Log($"Generation: {traits.generation}");
            Debug.Log($"Parents: {data.pair.parent1.speciesName} x {data.pair.parent2.speciesName}");
            Debug.Log($"Color: {traits.colorVariant}");
            Debug.Log($"Pattern: {traits.patternType}");
            Debug.Log($"Size: {traits.sizeMultiplier:P0}");
            Debug.Log($"Value Modifier: {traits.valueModifier:P0}");

            // Check for special traits
            if (traits.hasBioluminescence)
            {
                Debug.Log("SPECIAL: Bioluminescent!");
                ShowNotification("Bioluminescent fish discovered!");
            }

            if (traits.colorVariant == FishColor.Rainbow)
            {
                Debug.Log("RARE: Rainbow variant!");
                ShowNotification("Rainbow variant discovered!");
            }

            if (traits.isAberrant)
            {
                Debug.Log("WARNING: Aberrant offspring!");
            }

            // Offspring is automatically added to tank by AquariumManager
            Debug.Log($"Offspring added to tank: {data.pair.tankID}");

            // Update encyclopedia with new variant
            EventSystem.Publish("VariantDiscovered", new VariantDiscoveryData
            {
                speciesID = offspring.speciesID,
                colorVariant = traits.colorVariant,
                patternType = traits.patternType
            });
        });
    }

    // ============================================
    // EXAMPLE 3: Calculating Exhibition Revenue
    // ============================================

    /// <summary>
    /// Shows how to calculate and display exhibition income.
    /// </summary>
    private void Example_CalculateExhibitionIncome()
    {
        if (AquariumManager.Instance == null)
        {
            return;
        }

        // Get total daily income
        float totalDailyIncome = AquariumManager.Instance.CalculateTotalDailyIncome();
        float totalDailyCost = AquariumManager.Instance.CalculateTotalMaintenanceCost();
        float netDailyProfit = totalDailyIncome - totalDailyCost;

        Debug.Log("=== AQUARIUM ECONOMICS ===");
        Debug.Log($"Daily Exhibition Income: ${totalDailyIncome:F2}");
        Debug.Log($"Daily Maintenance Cost: ${totalDailyCost:F2}");
        Debug.Log($"Net Daily Profit: ${netDailyProfit:F2}");
        Debug.Log($"Monthly Projection: ${netDailyProfit * 30:F2}");
        Debug.Log($"Yearly Projection: ${netDailyProfit * 365:F2}");

        // Get breakdown by tank
        List<TankInstance> tanks = AquariumManager.Instance.GetOwnedTanks();

        Debug.Log("\n=== INCOME BY TANK ===");

        foreach (var tank in tanks)
        {
            List<DisplayFish> fishList = AquariumManager.Instance.GetTankFish(tank.tankData.tankID);
            float tankIncome = 0f;
            int rareFishCount = 0;

            foreach (var fish in fishList)
            {
                if (!fish.isAlive)
                {
                    continue;
                }

                float fishIncome = tank.tankData.GetDailyIncome(fish.rarity, 1) * fish.happiness;
                tankIncome += fishIncome;

                if (fish.rarity >= FishRarity.Rare)
                {
                    rareFishCount++;
                }
            }

            Debug.Log($"{tank.tankData.tankName}: ${tankIncome:F2}/day ({fishList.Count} fish, {rareFishCount} rare+)");
        }

        // Listen for daily income events
        EventSystem.Subscribe<float>("ExhibitionIncomeEarned", (float income) =>
        {
            Debug.Log($"Exhibition income earned: ${income:F2}");
            ShowNotification($"Exhibition earnings: ${income:F2}");
        });
    }

    // ============================================
    // EXAMPLE 4: Tank Upgrade System
    // ============================================

    /// <summary>
    /// Complete workflow for upgrading a tank.
    /// </summary>
    private void Example_UpgradeTank(string tankID)
    {
        if (AquariumManager.Instance == null)
        {
            return;
        }

        TankInstance tank = AquariumManager.Instance.GetTank(tankID);

        if (tank == null)
        {
            Debug.LogError($"Tank not found: {tankID}");
            return;
        }

        Debug.Log($"=== UPGRADE OPTIONS FOR {tank.tankData.tankName} ===");

        // Show all upgrade options
        ShowUpgradeOption(tank, TankUpgradeType.AutoFeeder, "Reduces daily feeding workload");
        ShowUpgradeOption(tank, TankUpgradeType.Filtration, "Improves water quality and fish happiness");
        ShowUpgradeOption(tank, TankUpgradeType.Lighting, "Unlocks rare species display");
        ShowUpgradeOption(tank, TankUpgradeType.BreedingChamber, "Speeds up breeding cycles");
        ShowUpgradeOption(tank, TankUpgradeType.GeneticsLab, "Increases mutation chances");

        // Example: Upgrade genetics lab
        int currentLabLevel = tank.GetUpgradeLevel(TankUpgradeType.GeneticsLab);

        if (currentLabLevel < 3)
        {
            bool upgraded = AquariumManager.Instance.UpgradeTank(tankID, TankUpgradeType.GeneticsLab);

            if (upgraded)
            {
                int newLevel = tank.GetUpgradeLevel(TankUpgradeType.GeneticsLab);
                float newMutationChance = 0.01f + (newLevel * 0.005f);

                Debug.Log($"Genetics Lab upgraded to level {newLevel}!");
                Debug.Log($"Mutation chance increased to {newMutationChance:P1}");
            }
            else
            {
                Debug.Log("Upgrade failed (insufficient funds or already maxed)");
            }
        }

        // Example: Upgrade capacity
        int currentCapacity = tank.GetMaxCapacity();
        bool capacityUpgraded = AquariumManager.Instance.UpgradeTankCapacity(tankID);

        if (capacityUpgraded)
        {
            int newCapacity = tank.GetMaxCapacity();
            Debug.Log($"Tank capacity upgraded: {currentCapacity} → {newCapacity}");
        }
    }

    /// <summary>
    /// Helper to show upgrade option details.
    /// </summary>
    private void ShowUpgradeOption(TankInstance tank, TankUpgradeType upgradeType, string description)
    {
        int currentLevel = tank.GetUpgradeLevel(upgradeType);
        int maxLevel = GetMaxLevel(upgradeType);

        if (currentLevel >= maxLevel)
        {
            Debug.Log($"{upgradeType}: MAX LEVEL ({currentLevel}/{maxLevel})");
            return;
        }

        float cost = GetUpgradeCost(upgradeType, currentLevel + 1);
        bool canAfford = EconomySystem.Instance?.CanAffordMoney(cost) ?? false;

        Debug.Log($"{upgradeType}: Level {currentLevel}/{maxLevel} | Cost: ${cost:F2} | {description} | " +
                 $"{(canAfford ? "CAN AFFORD" : "CANNOT AFFORD")}");
    }

    // ============================================
    // EXAMPLE 5: Fish Care Workflow
    // ============================================

    /// <summary>
    /// Complete fish care system integration.
    /// </summary>
    private void Example_FishCareSystem()
    {
        if (FishCareSystem.Instance == null || AquariumManager.Instance == null)
        {
            return;
        }

        // Get all fish that need attention
        List<DisplayFish> allFish = AquariumManager.Instance.GetAllDisplayFish();

        int unfedCount = 0;
        int sickCount = 0;
        int unhappyCount = 0;

        foreach (var fish in allFish)
        {
            if (!fish.isAlive)
            {
                continue;
            }

            if (!fish.isFed)
            {
                unfedCount++;
            }

            if (FishCareSystem.Instance.IsDiseased(fish))
            {
                sickCount++;
            }

            if (fish.happiness < 0.5f)
            {
                unhappyCount++;
            }
        }

        Debug.Log("=== FISH CARE STATUS ===");
        Debug.Log($"Total Fish: {allFish.Count}");
        Debug.Log($"Unfed: {unfedCount}");
        Debug.Log($"Sick: {sickCount}");
        Debug.Log($"Unhappy (<50%): {unhappyCount}");

        // Feed all unfed fish
        if (unfedCount > 0)
        {
            float feedingCost = unfedCount * 1f; // $1 per fish
            Debug.Log($"Feeding all fish would cost: ${feedingCost:F2}");

            bool fedAll = FishCareSystem.Instance.FeedAllTanks();

            if (fedAll)
            {
                Debug.Log("All fish fed successfully!");
            }
        }

        // Treat all sick fish
        if (sickCount > 0)
        {
            Debug.Log($"Treating {sickCount} sick fish...");

            foreach (var fish in allFish)
            {
                if (FishCareSystem.Instance.IsDiseased(fish))
                {
                    bool treated = FishCareSystem.Instance.TreatFish(fish);

                    if (treated)
                    {
                        Debug.Log($"Treated {fish.speciesName}");
                    }
                }
            }
        }

        // Show care statistics
        CareStatistics stats = FishCareSystem.Instance.GetStatistics();

        Debug.Log("\n=== CARE STATISTICS ===");
        Debug.Log($"Total Feedings: {stats.totalFeedings}");
        Debug.Log($"Total Treatments: {stats.totalTreatments}");
        Debug.Log($"Total Care Cost: ${stats.totalCareCost:F2}");
        Debug.Log($"Auto-Feeding: {(stats.autoFeedingEnabled ? "ON" : "OFF")}");
        Debug.Log($"Auto-Treatment: {(stats.autoTreatmentEnabled ? "ON" : "OFF")}");
    }

    /// <summary>
    /// Enable automated care systems.
    /// </summary>
    private void Example_EnableAutoCare()
    {
        if (FishCareSystem.Instance == null)
        {
            return;
        }

        // Enable auto-feeding (requires Auto-Feeder upgrade on tanks)
        FishCareSystem.Instance.SetAutoFeeding(true);
        Debug.Log("Auto-feeding enabled");

        // Enable auto-treatment
        FishCareSystem.Instance.SetAutoTreatment(true);
        Debug.Log("Auto-treatment enabled");

        // These will run automatically during daily updates
    }

    // ============================================
    // EXAMPLE 6: Display & Viewing System
    // ============================================

    /// <summary>
    /// Shows how to use the display system to view tanks.
    /// </summary>
    private void Example_DisplaySystem(string tankID)
    {
        if (DisplayController.Instance == null)
        {
            return;
        }

        // Display a specific tank
        DisplayController.Instance.DisplayTank(tankID);
        Debug.Log($"Now viewing tank: {tankID}");

        // Set viewing mode
        DisplayController.Instance.SetViewMode(ViewingMode.Overview);

        // Focus on a specific fish
        List<DisplayFish> tankFish = AquariumManager.Instance?.GetTankFish(tankID);

        if (tankFish != null && tankFish.Count > 0)
        {
            DisplayFish focusFish = tankFish[0];
            DisplayController.Instance.FocusFish(focusFish);
            Debug.Log($"Focused on: {focusFish.speciesName}");

            // Show fish details
            Debug.Log(focusFish.GetStatusString());
            Debug.Log(GeneticsSystem.GetTraitDescription(focusFish.traits));
        }

        // Listen for fish selection events
        EventSystem.Subscribe<DisplayFish>("FishFocused", (DisplayFish fish) =>
        {
            Debug.Log($"Player selected: {fish.speciesName}");
            // Show UI panel with fish details
        });
    }

    // ============================================
    // HELPER METHODS
    // ============================================

    private int GetMaxLevel(TankUpgradeType upgradeType)
    {
        switch (upgradeType)
        {
            case TankUpgradeType.AutoFeeder: return 3;
            case TankUpgradeType.Filtration: return 3;
            case TankUpgradeType.Lighting: return 4;
            case TankUpgradeType.BreedingChamber: return 3;
            case TankUpgradeType.GeneticsLab: return 3;
            default: return 0;
        }
    }

    private float GetUpgradeCost(TankUpgradeType upgradeType, int level)
    {
        float baseCost = 200f;

        switch (upgradeType)
        {
            case TankUpgradeType.AutoFeeder: baseCost = 150f; break;
            case TankUpgradeType.Filtration: baseCost = 250f; break;
            case TankUpgradeType.Lighting: baseCost = 300f; break;
            case TankUpgradeType.BreedingChamber: baseCost = 500f; break;
            case TankUpgradeType.GeneticsLab: baseCost = 800f; break;
        }

        return baseCost * Mathf.Pow(2f, level - 1);
    }

    private void ShowBreedingProgressUI(DisplayFish parent1, DisplayFish parent2)
    {
        // In real implementation, show UI panel
        Debug.Log("Showing breeding progress UI...");
    }

    private void ShowNotification(string message)
    {
        // In real implementation, show in-game notification
        Debug.Log($"[NOTIFICATION] {message}");
    }
}

/// <summary>
/// Data structure for variant discovery tracking.
/// </summary>
[System.Serializable]
public struct VariantDiscoveryData
{
    public string speciesID;
    public FishColor colorVariant;
    public PatternType patternType;
}

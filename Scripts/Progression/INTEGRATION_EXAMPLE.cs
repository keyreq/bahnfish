using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Agent 9: Progression & Economy Agent - Integration Example
/// Demonstrates how to use all progression and economy systems together.
/// Copy this pattern for your own game scripts.
/// </summary>
public class ProgressionIntegrationExample : MonoBehaviour
{
    /// <summary>
    /// Example: Complete fishing session with progression.
    /// </summary>
    public void CompleteGameplayLoop()
    {
        // ===== STEP 1: Catch Fish (Agent 5 + 8) =====
        List<Fish> caughtFish = new List<Fish>();

        // Simulate catching 10 fish
        for (int i = 0; i < 10; i++)
        {
            Fish fish = SimulateFishCatch();
            if (fish != null)
            {
                caughtFish.Add(fish);
                Debug.Log($"Caught {fish.name}!");
            }
        }

        // ===== STEP 2: Sell Fish (ShopManager + PricingSystem) =====
        bool caughtAtNight = (GameManager.Instance.CurrentGameState.timeOfDay == TimeOfDay.Night);

        // Preview total value before selling
        float totalValue = PricingSystem.Instance.CalculateBulkSellValue(caughtFish, caughtAtNight);
        Debug.Log($"Your catch is worth ${totalValue:F2}");

        // Sell all fish
        float earned = ShopManager.Instance.SellBulkFish(caughtFish, caughtAtNight);
        Debug.Log($"Sold {caughtFish.Count} fish for ${earned:F2}");

        // ===== STEP 3: Check Progression =====
        var stats = ProgressionManager.Instance.GetProgressionStats();
        Debug.Log($"Player Level: {stats.playerLevel}, Fish Caught: {stats.totalFishCaught}");

        // ===== STEP 4: Check Available Upgrades =====
        CheckAndPurchaseUpgrades();

        // ===== STEP 5: Check Location Unlocks =====
        CheckAndUnlockLocations();

        // ===== STEP 6: Check Dark Abilities =====
        CheckAndUnlockAbilities();
    }

    /// <summary>
    /// Example: Check and purchase available upgrades.
    /// </summary>
    private void CheckAndPurchaseUpgrades()
    {
        // Get all upgrades we can afford right now
        var availableUpgrades = UpgradeSystem.Instance.GetAvailableUpgrades();

        if (availableUpgrades.Count > 0)
        {
            Debug.Log($"You can purchase {availableUpgrades.Count} upgrades!");

            foreach (var upgrade in availableUpgrades)
            {
                int currentLevel = UpgradeSystem.Instance.GetUpgradeLevel(upgrade.upgradeID);
                float cost = upgrade.GetCostForLevel(currentLevel);

                Debug.Log($"- {upgrade.upgradeName} Level {currentLevel + 1}: ${cost:F2}");
                Debug.Log($"  {upgrade.description}");

                // Auto-purchase fishing rod upgrades (example)
                if (upgrade.upgradeType == UpgradeType.FishingRod)
                {
                    bool success = UpgradeSystem.Instance.PurchaseUpgrade(upgrade.upgradeID);
                    if (success)
                    {
                        Debug.Log($"Purchased {upgrade.upgradeName}!");
                    }
                }
            }
        }
    }

    /// <summary>
    /// Example: Check and unlock new locations.
    /// </summary>
    private void CheckAndUnlockLocations()
    {
        var availableLocations = LocationLicenses.Instance.GetAvailableLocations();

        if (availableLocations.Count > 0)
        {
            Debug.Log($"You can unlock {availableLocations.Count} new locations!");

            foreach (var location in availableLocations)
            {
                Debug.Log($"- {location.locationName}: ${location.cost:F2}");
                Debug.Log($"  {location.description}");

                // Example: Auto-unlock if we have enough money
                if (EconomySystem.Instance.CanAffordMoney(location.cost))
                {
                    bool success = LocationLicenses.Instance.PurchaseLicense(location.locationID);
                    if (success)
                    {
                        Debug.Log($"Unlocked {location.locationName}!");
                    }
                }
            }
        }
    }

    /// <summary>
    /// Example: Check and unlock dark abilities.
    /// </summary>
    private void CheckAndUnlockAbilities()
    {
        var availableAbilities = DarkAbilities.Instance.GetAvailableAbilities();

        if (availableAbilities.Count > 0)
        {
            Debug.Log($"You can unlock {availableAbilities.Count} dark abilities!");

            foreach (var ability in availableAbilities)
            {
                Debug.Log($"- {ability.abilityName}: {ability.relicCost} relics");
                Debug.Log($"  {ability.description}");

                // Example: Unlock if we have relics
                if (EconomySystem.Instance.CanAffordRelics(ability.relicCost))
                {
                    bool success = DarkAbilities.Instance.UnlockAbility(ability.abilityID);
                    if (success)
                    {
                        Debug.Log($"Unlocked {ability.abilityName}!");
                    }
                }
            }
        }
    }

    /// <summary>
    /// Example: Use a dark ability during gameplay.
    /// </summary>
    public void ActivateDarkAbilityExample()
    {
        // Check if Abyssal Sprint is ready
        if (DarkAbilities.Instance.IsAbilityUnlocked("abyssal_sprint") &&
            DarkAbilities.Instance.IsAbilityReady("abyssal_sprint"))
        {
            // Activate for escape from danger
            bool activated = DarkAbilities.Instance.ActivateAbility("abyssal_sprint");

            if (activated)
            {
                Debug.Log("Abyssal Sprint activated! 2x speed for 10 seconds!");
                // Speed boost is applied via event, BoatController listens
            }
        }
        else
        {
            float cooldown = DarkAbilities.Instance.GetRemainingCooldown("abyssal_sprint");
            Debug.Log($"Abyssal Sprint on cooldown: {cooldown:F1}s remaining");
        }
    }

    /// <summary>
    /// Example: Buy supplies from shop.
    /// </summary>
    public void BuySuppliesExample()
    {
        // Buy bait for fishing
        bool boughtBait = ShopManager.Instance.BuyBait("Worms", quantity: 20, pricePerUnit: 2f);
        if (boughtBait)
        {
            Debug.Log("Bought 20 worms for $40");
        }

        // Buy fuel for travel
        bool boughtFuel = ShopManager.Instance.BuyFuel(amount: 50f, pricePerUnit: 1f);
        if (boughtFuel)
        {
            Debug.Log("Bought 50 fuel for $50");
        }

        // Buy talisman for sanity protection
        bool boughtTalisman = ShopManager.Instance.BuyTalisman("Protection Charm", cost: 150f);
        if (boughtTalisman)
        {
            Debug.Log("Bought Protection Charm for $150");
        }
    }

    /// <summary>
    /// Example: Subscribe to progression events.
    /// </summary>
    private void Start()
    {
        // Subscribe to economy events
        EventSystem.Subscribe<CurrencyChangeData>("MoneyChanged", OnMoneyChanged);
        EventSystem.Subscribe<FishSoldData>("FishSold", OnFishSold);

        // Subscribe to upgrade events
        EventSystem.Subscribe<UpgradePurchasedData>("UpgradePurchased", OnUpgradePurchased);

        // Subscribe to location events
        EventSystem.Subscribe<LocationLicenseData>("LocationUnlocked", OnLocationUnlocked);

        // Subscribe to ability events
        EventSystem.Subscribe<DarkAbilityData>("DarkAbilityUnlocked", OnAbilityUnlocked);

        // Subscribe to milestone events
        EventSystem.Subscribe<MilestoneData>("MilestoneCompleted", OnMilestoneCompleted);
    }

    private void OnDestroy()
    {
        // Unsubscribe from all events
        EventSystem.Unsubscribe<CurrencyChangeData>("MoneyChanged", OnMoneyChanged);
        EventSystem.Unsubscribe<FishSoldData>("FishSold", OnFishSold);
        EventSystem.Unsubscribe<UpgradePurchasedData>("UpgradePurchased", OnUpgradePurchased);
        EventSystem.Unsubscribe<LocationLicenseData>("LocationUnlocked", OnLocationUnlocked);
        EventSystem.Unsubscribe<DarkAbilityData>("DarkAbilityUnlocked", OnAbilityUnlocked);
        EventSystem.Unsubscribe<MilestoneData>("MilestoneCompleted", OnMilestoneCompleted);
    }

    // ===== EVENT HANDLERS =====

    private void OnMoneyChanged(CurrencyChangeData data)
    {
        Debug.Log($"Money changed: ${data.oldValue:F2} → ${data.newValue:F2} ({data.reason})");

        // Update UI
        // UIManager.Instance.UpdateMoneyDisplay(data.newValue);
    }

    private void OnFishSold(FishSoldData data)
    {
        Debug.Log($"Sold {data.fish.name} for ${data.amountEarned:F2}");

        // Show notification
        // NotificationManager.Instance.Show($"Sold {data.fish.name}", NotificationType.Success);
    }

    private void OnUpgradePurchased(UpgradePurchasedData data)
    {
        Debug.Log($"Purchased {data.upgrade.upgradeName} Level {data.newLevel}!");

        // Show upgrade notification
        // NotificationManager.Instance.Show($"Upgrade: {data.upgrade.upgradeName}", NotificationType.Upgrade);
    }

    private void OnLocationUnlocked(LocationLicenseData location)
    {
        Debug.Log($"New location unlocked: {location.locationName}!");

        // Show big notification
        // NotificationManager.Instance.ShowBig($"New Location: {location.locationName}", location.description);
    }

    private void OnAbilityUnlocked(DarkAbilityData ability)
    {
        Debug.Log($"Dark ability unlocked: {ability.abilityName}!");

        // Show special notification
        // NotificationManager.Instance.ShowDarkAbility(ability);
    }

    private void OnMilestoneCompleted(MilestoneData milestone)
    {
        Debug.Log($"Milestone completed: {milestone.milestoneName}!");
        Debug.Log($"Reward: ${milestone.reward.money:F2} + {milestone.reward.scrap} scrap + {milestone.reward.relics} relics");

        // Show milestone popup
        // UIManager.Instance.ShowMilestonePopup(milestone);
    }

    // ===== HELPER METHODS =====

    /// <summary>
    /// Simulates catching a fish (placeholder for Agent 5 integration).
    /// </summary>
    private Fish SimulateFishCatch()
    {
        // In real game, this would be from FishingController
        return FishDatabase.Instance?.GetFishByID("largemouth_bass");
    }

    /// <summary>
    /// Example: Display progression UI.
    /// </summary>
    public void DisplayProgressionUI()
    {
        Debug.Log("=== Player Progression ===");

        // Currency
        Debug.Log($"Money: ${EconomySystem.Instance.GetMoney():F2}");
        Debug.Log($"Scrap: {EconomySystem.Instance.GetScrap()}");
        Debug.Log($"Relics: {EconomySystem.Instance.GetRelics()}");

        // Stats
        var stats = ProgressionManager.Instance.GetProgressionStats();
        Debug.Log($"Level: {stats.playerLevel}");
        Debug.Log($"Fish Caught: {stats.totalFishCaught}");
        Debug.Log($"Locations: {stats.locationsUnlocked}/13");
        Debug.Log($"Abilities: {stats.abilitiesUnlocked}/6");

        // Milestones
        Debug.Log($"Milestones: {stats.milestonesCompleted}/{stats.totalMilestones}");

        Debug.Log("========================");
    }

    /// <summary>
    /// Example: Context menu test function.
    /// </summary>
    [ContextMenu("Test Complete Gameplay Loop")]
    private void TestCompleteLoop()
    {
        CompleteGameplayLoop();
        DisplayProgressionUI();
    }
}

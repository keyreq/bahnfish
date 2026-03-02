using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Agent 14: Location & World Generation - Integration Example
/// Demonstrates how to integrate the World system with other game systems.
/// </summary>
public class WorldIntegrationExample : MonoBehaviour
{
    // Example 1: Basic Location Access
    void Example_AccessLocationData()
    {
        // Get current location
        LocationData currentLocation = LocationManager.Instance.GetCurrentLocation();
        Debug.Log($"Current Location: {currentLocation.locationName}");
        Debug.Log($"Biome: {currentLocation.biomeType}");
        Debug.Log($"Fish Species: {currentLocation.fishSpeciesIDs.Count}");

        // Get specific location
        LocationData arcticWaters = LocationManager.Instance.GetLocationByID("arctic_waters");
        if (arcticWaters != null)
        {
            Debug.Log($"Arctic Waters costs ${arcticWaters.licenseCost} to unlock");
        }

        // Check if location is unlocked
        bool isUnlocked = LocationManager.Instance.IsLocationUnlocked("volcanic_vent");
        Debug.Log($"Volcanic Vent unlocked: {isUnlocked}");

        // Get all unlocked locations
        List<LocationData> unlockedLocations = LocationManager.Instance.GetUnlockedLocations();
        Debug.Log($"Player has unlocked {unlockedLocations.Count} locations");
    }

    // Example 2: Travel Between Locations
    void Example_TravelToLocation()
    {
        string destinationID = "coral_reef";

        // Step 1: Check if location is unlocked
        if (!LocationManager.Instance.IsLocationUnlocked(destinationID))
        {
            Debug.Log("Location is locked! Purchase license first.");
            return;
        }

        // Step 2: Get travel requirements
        TravelRequirements req = NavigationSystem.Instance.GetTravelRequirements(destinationID);
        Debug.Log($"Travel to Coral Reef:");
        Debug.Log($"  Distance: {req.distance:F1} km");
        Debug.Log($"  Fuel Cost: {req.fuelCost:F1}");
        Debug.Log($"  Travel Time: {req.travelTime:F1} seconds");
        Debug.Log($"  Can Afford: {req.canAfford}");

        // Step 3: Check if player can afford travel
        if (!req.canAfford)
        {
            Debug.Log("Not enough fuel! Return to dock to refuel.");
            return;
        }

        // Step 4: Start travel
        bool travelStarted = NavigationSystem.Instance.TravelToLocation(destinationID);
        if (travelStarted)
        {
            Debug.Log("Travel started! Consuming fuel...");
        }
    }

    // Example 3: Fast Travel System
    void Example_FastTravel()
    {
        // Check if fast travel is unlocked
        if (!FastTravelSystem.Instance.IsFastTravelUnlocked())
        {
            Debug.Log("Fast travel not unlocked yet!");
            Debug.Log("Find the Tidal Gate altar in Bioluminescent Bay");
            return;
        }

        string destinationID = "abyssal_trench";

        // Check if can fast travel to destination
        if (!FastTravelSystem.Instance.CanFastTravelTo(destinationID))
        {
            Debug.Log("Cannot fast travel to this location");
            return;
        }

        // Get relic cost
        int relicCost = FastTravelSystem.Instance.GetRelicCost();
        Debug.Log($"Fast travel costs {relicCost} relic(s)");

        // TODO: Check if player has enough relics (via ProgressionManager)
        // if (ProgressionManager.Instance.GetRelicCount() < relicCost) return;

        // Perform fast travel
        bool success = FastTravelSystem.Instance.FastTravel(destinationID);
        if (success)
        {
            Debug.Log("Teleporting to destination!");
        }
    }

    // Example 4: Fuel Management
    void Example_FuelManagement()
    {
        // Get current fuel status
        FuelWarning warning = TravelCostCalculator.Instance.GetFuelWarningStatus();

        switch (warning.level)
        {
            case FuelWarningLevel.Normal:
                Debug.Log($"Fuel: {warning.fuelRemaining:F1} - All good");
                break;
            case FuelWarningLevel.Low:
                Debug.LogWarning($"Low Fuel: {warning.fuelRemaining:F1} - Consider refueling");
                break;
            case FuelWarningLevel.Critical:
                Debug.LogError($"CRITICAL FUEL: {warning.fuelRemaining:F1} - Return to dock NOW!");
                break;
        }

        // Get maximum range with current fuel
        float maxRange = TravelCostCalculator.Instance.GetMaximumRange();
        Debug.Log($"Maximum range: {maxRange:F1} km");

        // Get all reachable locations
        List<LocationData> reachable = TravelCostCalculator.Instance.GetReachableLocations();
        Debug.Log($"Can reach {reachable.Count} locations:");
        foreach (var location in reachable)
        {
            float fuelCost = TravelCostCalculator.Instance.CalculateFuelCost(location.locationID);
            Debug.Log($"  - {location.locationName}: {fuelCost:F1} fuel");
        }

        // Get detailed cost breakdown
        string targetID = "volcanic_vent";
        TravelCostBreakdown breakdown = TravelCostCalculator.Instance.GetTravelCostBreakdown(targetID);
        Debug.Log($"Travel Cost Breakdown to {targetID}:");
        Debug.Log($"  Distance: {breakdown.distance:F1} km");
        Debug.Log($"  Base Fuel Cost: {breakdown.baseFuelCost:F1}");
        Debug.Log($"  Engine Efficiency: {breakdown.engineEfficiency}x");
        Debug.Log($"  Weather Modifier: {breakdown.weatherModifier}x");
        Debug.Log($"  Night Modifier: {breakdown.nightModifier}x");
        Debug.Log($"  Total Fuel Cost: {breakdown.totalFuelCost:F1}");
        Debug.Log($"  Can Afford: {breakdown.canAfford}");
    }

    // Example 5: Secret Area Discovery
    void Example_SecretAreas()
    {
        // Get player position
        Vector3 playerPosition = transform.position;

        // Check if player is in a secret area
        float rareFishBonus = SecretAreaManager.Instance.GetRareFishBonusAtPosition(playerPosition);
        float legendaryBonus = SecretAreaManager.Instance.GetLegendaryBonusAtPosition(playerPosition);

        if (rareFishBonus > 1f)
        {
            Debug.Log($"Fishing in a secret area! Rare fish spawn rate: {rareFishBonus}x");
        }

        if (legendaryBonus > 0f)
        {
            Debug.Log($"Legendary fish spawn chance: +{legendaryBonus * 100}%");
        }

        // Get discovery statistics
        var stats = SecretAreaManager.Instance.GetSecretStats();
        Debug.Log($"Secrets: {stats.discovered} / {stats.total} discovered");

        // Get secrets in current location
        LocationData currentLocation = LocationManager.Instance.GetCurrentLocation();
        List<SecretAreaData> secrets = SecretAreaManager.Instance.GetSecretsInLocation(currentLocation.locationID);

        Debug.Log($"Secrets in {currentLocation.locationName}:");
        foreach (var secret in secrets)
        {
            bool discovered = SecretAreaManager.Instance.IsSecretDiscovered(secret.secretID);
            string status = discovered ? "[DISCOVERED]" : "[HIDDEN]";
            Debug.Log($"  {status} {secret.secretName}: {secret.description}");

            if (!discovered && secret.requiresElditchEye)
            {
                Debug.Log($"    Hint: {secret.unlockHint} (Requires Eldritch Eye)");
            }
        }
    }

    // Example 6: Fish Pool Integration with Agent 8
    void Example_GetLocationFishPool()
    {
        // Get fish species that can spawn at current location
        List<string> fishPool = LocationManager.Instance.GetCurrentLocationFishPool();
        Debug.Log($"Fish species at current location: {fishPool.Count}");

        // Get fish data from FishDatabase (Agent 8)
        foreach (string fishID in fishPool)
        {
            FishSpeciesData fishData = FishDatabase.Instance.GetFishByID(fishID);
            if (fishData != null)
            {
                Debug.Log($"  - {fishData.fishName} ({fishData.rarity})");
            }
        }

        // Filter fish by rarity and location
        LocationData location = LocationManager.Instance.GetCurrentLocation();
        List<FishSpeciesData> rareFish = FishDatabase.Instance.GetFilteredFish(
            locationID: location.locationID,
            rarity: FishRarity.Rare
        );

        Debug.Log($"Rare fish at {location.locationName}: {rareFish.Count}");
    }

    // Example 7: Subscribe to Location Events
    void Start()
    {
        // Subscribe to location change events
        EventSystem.Subscribe<LocationData>("LocationChanged", OnLocationChanged);
        EventSystem.Subscribe<TravelStartedEventData>("TravelStarted", OnTravelStarted);
        EventSystem.Subscribe<float>("TravelProgress", OnTravelProgress);
        EventSystem.Subscribe<string>("TravelComplete", OnTravelComplete);
        EventSystem.Subscribe<SecretAreaData>("SecretAreaDiscovered", OnSecretDiscovered);
    }

    void OnDestroy()
    {
        // Always unsubscribe!
        EventSystem.Unsubscribe<LocationData>("LocationChanged", OnLocationChanged);
        EventSystem.Unsubscribe<TravelStartedEventData>("TravelStarted", OnTravelStarted);
        EventSystem.Unsubscribe<float>("TravelProgress", OnTravelProgress);
        EventSystem.Unsubscribe<string>("TravelComplete", OnTravelComplete);
        EventSystem.Unsubscribe<SecretAreaData>("SecretAreaDiscovered", OnSecretDiscovered);
    }

    private void OnLocationChanged(LocationData newLocation)
    {
        Debug.Log($"[EVENT] Arrived at: {newLocation.locationName}");
        Debug.Log($"  Biome: {newLocation.biomeType}");
        Debug.Log($"  Difficulty: {newLocation.difficulty}");
        Debug.Log($"  Fish Species: {newLocation.fishSpeciesIDs.Count}");
        Debug.Log($"  Sanity Drain: {newLocation.sanityDrainModifier}x");

        // Update UI, spawn fish, apply weather, etc.
    }

    private void OnTravelStarted(TravelStartedEventData travelData)
    {
        Debug.Log($"[EVENT] Started traveling to: {travelData.destinationName}");
        Debug.Log($"  Distance: {travelData.distance:F1} km");
        Debug.Log($"  Fuel Cost: {travelData.fuelCost:F1}");
        Debug.Log($"  Estimated Time: {travelData.estimatedTime:F1}s");

        // Show loading screen, disable controls, etc.
    }

    private void OnTravelProgress(float progress)
    {
        // Update loading bar
        Debug.Log($"[EVENT] Travel progress: {progress * 100:F0}%");
    }

    private void OnTravelComplete(string locationID)
    {
        Debug.Log($"[EVENT] Travel complete: {locationID}");

        // Hide loading screen, enable controls, etc.
    }

    private void OnSecretDiscovered(SecretAreaData secret)
    {
        Debug.Log($"[EVENT] SECRET DISCOVERED: {secret.secretName}!");
        Debug.Log($"  {secret.description}");

        // Show discovery UI, play VFX, award relics, etc.
    }

    // Example 8: Integration with Progression System (Agent 9)
    void Example_UnlockLocation()
    {
        string locationID = "fog_shrouded_swamp";
        LocationData location = LocationManager.Instance.GetLocationByID(locationID);

        // Check if already unlocked
        if (LocationManager.Instance.IsLocationUnlocked(locationID))
        {
            Debug.Log($"{location.locationName} is already unlocked!");
            return;
        }

        // Check if player has enough money (via GameManager)
        GameState gameState = GameManager.Instance.CurrentGameState;
        if (gameState.money < location.licenseCost)
        {
            Debug.Log($"Not enough money! Need ${location.licenseCost}, have ${gameState.money}");
            return;
        }

        // Deduct money
        gameState.money -= location.licenseCost;
        GameManager.Instance.UpdateGameState(gameState);

        // Unlock location
        LocationManager.Instance.UnlockLocation(locationID);

        Debug.Log($"Purchased license for {location.locationName} (${location.licenseCost})");
        Debug.Log($"Remaining money: ${gameState.money}");
    }

    // Example 9: Using Location Reference Data
    void Example_LocationReferenceData()
    {
        // Get all location IDs
        List<string> allLocationIDs = All13Locations.LocationIDs;
        Debug.Log($"Total locations in game: {allLocationIDs.Count}");

        // Get progression info
        LocationProgression prog = All13Locations.GetProgression("volcanic_vent");
        Debug.Log($"Volcanic Vent:");
        Debug.Log($"  Tier: {prog.tier}");
        Debug.Log($"  Recommended Level: {prog.recommendedLevel}");
        Debug.Log($"  Unlock Order: {prog.unlockOrder}");

        // Check if story location
        bool isStory = All13Locations.IsStoryLocation("underground_cavern");
        Debug.Log($"Underground Cavern is story location: {isStory}");

        // Get NPCs at location
        List<string> npcs = All13Locations.GetNPCs("coral_reef");
        Debug.Log($"NPCs at Coral Reef: {string.Join(", ", npcs)}");

        // Get special mechanics
        string mechanics = All13Locations.GetSpecialMechanics("bioluminescent_bay");
        Debug.Log($"Special: {mechanics}");

        // Get secrets
        List<SecretAreaDefinition> secrets = All13Locations.GetSecrets("fog_shrouded_swamp");
        Debug.Log($"Secrets in Fog-Shrouded Swamp: {secrets.Count}");
        foreach (var secret in secrets)
        {
            Debug.Log($"  - {secret.secretName}: {secret.description}");
        }
    }

    // Example 10: Complete Travel UI Flow
    void Example_TravelUIFlow()
    {
        // 1. Player opens map
        Debug.Log("=== OPENING TRAVEL MAP ===");

        // 2. Show all unlocked locations
        List<LocationData> unlockedLocations = LocationManager.Instance.GetUnlockedLocations();
        Debug.Log($"Available destinations: {unlockedLocations.Count}");

        foreach (var location in unlockedLocations)
        {
            // Skip current location
            if (location == LocationManager.Instance.GetCurrentLocation())
                continue;

            // Get travel requirements
            TravelRequirements req = NavigationSystem.Instance.GetTravelRequirements(location.locationID);

            // Show location button with info
            string affordStatus = req.canAfford ? "[CAN TRAVEL]" : "[NOT ENOUGH FUEL]";
            Debug.Log($"{affordStatus} {location.locationName}");
            Debug.Log($"  Distance: {req.distance:F1} km");
            Debug.Log($"  Fuel: {req.fuelCost:F1}");
            Debug.Log($"  Time: {req.travelTime:F1}s");
        }

        // 3. Show locked locations (grayed out)
        List<LocationData> lockedLocations = LocationManager.Instance.GetLockedLocations();
        Debug.Log($"\nLocked locations: {lockedLocations.Count}");
        foreach (var location in lockedLocations)
        {
            Debug.Log($"[LOCKED] {location.locationName} - ${location.licenseCost} license required");
        }

        // 4. Player selects destination
        string selectedID = "arctic_waters";
        LocationData selectedLocation = LocationManager.Instance.GetLocationByID(selectedID);

        // 5. Show confirmation dialog
        TravelRequirements selectedReq = NavigationSystem.Instance.GetTravelRequirements(selectedID);
        Debug.Log($"\n=== TRAVEL TO {selectedLocation.locationName.ToUpper()}? ===");
        Debug.Log($"Fuel Cost: {selectedReq.fuelCost:F1}");
        Debug.Log($"Travel Time: {selectedReq.travelTime:F1} seconds");
        Debug.Log($"[CONFIRM] [CANCEL]");

        // 6. Player confirms - start travel
        bool started = NavigationSystem.Instance.TravelToLocation(selectedID);
        if (started)
        {
            Debug.Log("Travel started! Show loading screen...");
        }
    }
}

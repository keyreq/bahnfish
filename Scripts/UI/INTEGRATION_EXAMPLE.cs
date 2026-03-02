using UnityEngine;

/// <summary>
/// Example code demonstrating how to integrate with the UI system.
/// This file is for reference only and should not be included in builds.
/// Shows how other agents can trigger UI updates and notifications.
/// </summary>
public class UI_INTEGRATION_EXAMPLE : MonoBehaviour
{
    // ========================================
    // EXAMPLE 1: Updating Sanity (Agent 7)
    // ========================================

    public void ExampleUpdateSanity(float newSanity)
    {
        // Agent 7 (Sanity System) publishes this event
        EventSystem.Publish("OnSanityChanged", newSanity);

        // The SanityMeter automatically subscribes and updates
        // No direct reference to SanityMeter needed!
    }

    // ========================================
    // EXAMPLE 2: Showing Notifications
    // ========================================

    public void ExampleShowNotifications()
    {
        // Success notification (fish caught)
        NotificationManager.Instance.ShowNotification(
            "Caught Largemouth Bass!",
            NotificationManager.NotificationType.Success
        );

        // Warning notification (low sanity)
        NotificationManager.Instance.ShowNotification(
            "Sanity is getting low!",
            NotificationManager.NotificationType.Warning
        );

        // Error notification (line broke)
        NotificationManager.Instance.ShowNotification(
            "The line broke!",
            NotificationManager.NotificationType.Error
        );

        // Info notification with custom duration
        NotificationManager.Instance.ShowNotification(
            "New location unlocked: Deep Ocean",
            NotificationManager.NotificationType.Info,
            5f // Show for 5 seconds
        );
    }

    // ========================================
    // EXAMPLE 3: Fishing Tension (Agent 5)
    // ========================================

    public void ExampleFishingTension()
    {
        // When fishing starts
        EventSystem.Publish("FishingStarted");
        // TensionMeter automatically shows

        // During fishing, update tension
        float tension = 75f; // 0-100
        EventSystem.Publish("OnTensionChanged", tension);
        // TensionMeter updates in real-time

        // When fishing ends
        EventSystem.Publish("FishingEnded");
        // TensionMeter automatically hides
    }

    // ========================================
    // EXAMPLE 4: Updating Resources (Agent 9)
    // ========================================

    public void ExampleUpdateResources()
    {
        // Update money (triggers animation)
        EventSystem.Publish("OnMoneyChanged", 250f);

        // Update fuel
        EventSystem.Publish("OnFuelChanged", 65f);

        // Or update via GameState
        GameState state = GameManager.Instance.CurrentGameState.Clone();
        state.money = 300f;
        state.fuel = 70f;
        GameManager.Instance.UpdateGameState(state);
        // ResourceDisplay subscribes to GameStateUpdated
    }

    // ========================================
    // EXAMPLE 5: Fish Caught Event
    // ========================================

    public void ExampleFishCaught(Fish caughtFish)
    {
        // Agent 5 (Fishing) publishes this event
        EventSystem.Publish("FishCaught", caughtFish);

        // NotificationManager automatically shows:
        // "Caught [Fish Name]!"

        // You can also show custom notification:
        NotificationManager.Instance.ShowNotification(
            $"Caught {caughtFish.name} worth ${caughtFish.baseValue}!",
            NotificationManager.NotificationType.Success
        );
    }

    // ========================================
    // EXAMPLE 6: Tooltips
    // ========================================

    public void ExampleShowTooltips()
    {
        // Simple tooltip
        TooltipSystem.Instance.ShowTooltip("This is a fishing rod upgrade");

        // Tooltip with header
        TooltipSystem.Instance.ShowTooltip(
            "Increases casting distance by 20%",
            "Advanced Rod"
        );

        // Fish tooltip (auto-formatted)
        Fish bass = new Fish
        {
            name = "Largemouth Bass",
            baseValue = 25f,
            rarity = FishRarity.Common,
            inventorySize = new Vector2Int(2, 1),
            description = "A common freshwater fish."
        };
        TooltipSystem.Instance.ShowFishTooltip(bass);
    }

    // ========================================
    // EXAMPLE 7: Show/Hide HUD
    // ========================================

    public void ExampleToggleHUD()
    {
        // Hide HUD with animation
        HUDManager.Instance.HideHUD(true);

        // Show HUD with animation
        HUDManager.Instance.ShowHUD(true);

        // Toggle HUD
        HUDManager.Instance.ToggleHUD(true);

        // Hide specific element (e.g., tension meter when not fishing)
        HUDManager.Instance.SetElementVisibility("tension", false);
    }

    // ========================================
    // EXAMPLE 8: Complete Fishing Session
    // ========================================

    public void ExampleCompleteFishingSession()
    {
        // Player starts fishing
        EventSystem.Publish("FishingStarted");

        // Tension increases as fish fights
        for (float tension = 0f; tension <= 100f; tension += 10f)
        {
            EventSystem.Publish("OnTensionChanged", tension);

            // Show warning if high tension
            if (tension >= 90f)
            {
                NotificationManager.Instance.ShowNotification(
                    "Line is about to break!",
                    NotificationManager.NotificationType.Warning
                );
            }
        }

        // Fish caught successfully
        Fish caughtFish = new Fish
        {
            name = "Rainbow Trout",
            baseValue = 45f,
            rarity = FishRarity.Uncommon
        };
        EventSystem.Publish("FishCaught", caughtFish);

        // Fishing ends
        EventSystem.Publish("FishingEnded");

        // Update money
        GameState state = GameManager.Instance.CurrentGameState.Clone();
        state.money += caughtFish.baseValue;
        GameManager.Instance.UpdateGameState(state);

        // Show success notification
        NotificationManager.Instance.ShowNotification(
            $"Caught {caughtFish.name}!",
            NotificationManager.NotificationType.Success
        );
    }

    // ========================================
    // EXAMPLE 9: Sanity Drain Over Time
    // ========================================

    public void ExampleSanityDrainAtNight()
    {
        // Agent 7 would do this in Update() during nighttime
        GameState state = GameManager.Instance.CurrentGameState;

        if (TimeManager.Instance.IsNighttime())
        {
            // Drain sanity
            state.sanity -= 0.1f * Time.deltaTime; // 0.1 per second
            state.sanity = Mathf.Max(0f, state.sanity);

            // Update game state
            GameManager.Instance.UpdateGameState(state);

            // Publish sanity change event
            EventSystem.Publish("OnSanityChanged", state.sanity);

            // Warn player at low sanity
            if (state.sanity <= 25f && state.sanity > 24f) // Only once
            {
                NotificationManager.Instance.ShowNotification(
                    "Your sanity is dangerously low!",
                    NotificationManager.NotificationType.Warning
                );
            }
        }
    }

    // ========================================
    // EXAMPLE 10: UI Hover Events
    // ========================================

    // This would be attached to a UI button or image
    public void OnPointerEnter()
    {
        // Show tooltip when hovering over UI element
        TooltipSystem.Instance.ShowTooltip(
            "Click to upgrade your fishing rod",
            "Upgrade Rod - $200"
        );
    }

    public void OnPointerExit()
    {
        // Hide tooltip when leaving UI element
        TooltipSystem.Instance.HideTooltip();
    }

    // ========================================
    // EXAMPLE 11: Event Summary for Agent Integration
    // ========================================

    /*
     * EVENTS TO PUBLISH FROM YOUR AGENT:
     *
     * Agent 3 (Time/Environment):
     * - EventSystem.Publish("TimeUpdated", currentTime);
     * - EventSystem.Publish("TimeOfDayChanged", timeOfDay);
     *
     * Agent 5 (Fishing):
     * - EventSystem.Publish("FishingStarted");
     * - EventSystem.Publish("FishingEnded");
     * - EventSystem.Publish("OnTensionChanged", tension);
     * - EventSystem.Publish("FishCaught", fish);
     *
     * Agent 7 (Sanity):
     * - EventSystem.Publish("OnSanityChanged", sanity);
     *
     * Agent 9 (Economy):
     * - EventSystem.Publish("OnMoneyChanged", money);
     * - EventSystem.Publish("OnFuelChanged", fuel);
     *
     * You can also use:
     * - GameManager.Instance.UpdateGameState(state);
     *   (publishes "GameStateUpdated" event automatically)
     *
     * You can also directly call UI singletons:
     * - NotificationManager.Instance.ShowNotification(msg, type);
     * - TooltipSystem.Instance.ShowTooltip(text);
     * - HUDManager.Instance.ShowHUD(animated);
     */

    // ========================================
    // BEST PRACTICES
    // ========================================

    /*
     * 1. PREFER EVENTS OVER DIRECT CALLS
     *    - Use EventSystem.Publish() instead of direct UI calls
     *    - Keeps systems loosely coupled
     *    - UI can be disabled/removed without breaking your code
     *
     * 2. UPDATE GAMESTATE CENTRALLY
     *    - Always update GameState through GameManager
     *    - Don't modify GameState directly
     *    - UI subscribes to GameStateUpdated event
     *
     * 3. PUBLISH FINE-GRAINED EVENTS
     *    - Publish specific events (OnSanityChanged) for performance
     *    - Avoid publishing GameStateUpdated for every small change
     *
     * 4. USE NOTIFICATIONS WISELY
     *    - Don't spam notifications
     *    - Group similar events (e.g., "3 fish caught!" instead of 3 notifications)
     *    - Use appropriate types (Info, Success, Warning, Error)
     *
     * 5. CLEAN UP TOOLTIPS
     *    - Always hide tooltips when element is destroyed
     *    - Use OnPointerExit to hide on mouse leave
     *
     * 6. RESPECT HUD VISIBILITY
     *    - Check HUDManager.Instance.IsHUDVisible() if needed
     *    - Don't force show/hide HUD from your system
     */
}

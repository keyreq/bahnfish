using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// Agent 20: Idle/AFK Progression System - WelcomeBackSystem.cs
/// Displays welcome back UI when player returns after being offline.
/// Shows offline earnings summary, events that occurred, and comeback bonuses.
/// </summary>
public class WelcomeBackSystem : MonoBehaviour
{
    [Header("UI Configuration")]
    [Tooltip("Automatically show welcome UI when rewards are calculated")]
    [SerializeField] private bool autoShowWelcomeUI = true;

    [Tooltip("Minimum offline time (hours) to show welcome UI")]
    [SerializeField] private float minOfflineTimeToShow = 0.25f; // 15 minutes

    [Header("Display Settings")]
    [Tooltip("Show detailed breakdown of earnings")]
    [SerializeField] private bool showDetailedBreakdown = true;

    [Tooltip("Show events that occurred while offline")]
    [SerializeField] private bool showOfflineEvents = true;

    [Tooltip("Play welcome back sound effect")]
    [SerializeField] private bool playSoundEffect = true;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogging = true;

    // Current summary being displayed
    private OfflineRewardsSummary currentSummary;
    private bool isShowingUI = false;

    /// <summary>
    /// Shows the welcome back UI with offline rewards summary.
    /// </summary>
    /// <param name="summary">Offline rewards summary</param>
    public void ShowWelcomeBackUI(OfflineRewardsSummary summary)
    {
        // Check minimum offline time
        if (summary.offlineTimeHours < minOfflineTimeToShow)
        {
            if (enableDebugLogging)
            {
                Debug.Log($"[WelcomeBackSystem] Offline time too short to show UI ({summary.offlineTimeHours:F2}h)");
            }
            return;
        }

        currentSummary = summary;
        isShowingUI = true;

        // Play sound effect
        if (playSoundEffect)
        {
            EventSystem.Publish("PlayWelcomeBackSound", true);
        }

        // Generate UI content
        string uiContent = GenerateWelcomeBackContent(summary);

        // Publish event to show UI
        EventSystem.Publish("ShowWelcomeBackUI", uiContent);

        // Log to console for debugging
        if (enableDebugLogging)
        {
            Debug.Log($"[WelcomeBackSystem] Welcome Back UI Displayed:\n{uiContent}");
        }
    }

    /// <summary>
    /// Generates the welcome back content string for UI display.
    /// </summary>
    /// <param name="summary">Offline rewards summary</param>
    /// <returns>Formatted welcome back content</returns>
    private string GenerateWelcomeBackContent(OfflineRewardsSummary summary)
    {
        StringBuilder content = new StringBuilder();

        // Header
        content.AppendLine("=== WELCOME BACK! ===");
        content.AppendLine();

        // Time away
        content.AppendLine(FormatOfflineTime(summary.offlineTimeHours));
        if (summary.wasCapped)
        {
            content.AppendLine($"(Capped at {summary.cappedTimeHours:F1} hours for rewards)");
        }
        content.AppendLine();

        // Offline earnings section
        content.AppendLine("--- Offline Earnings ---");

        // Money
        if (summary.moneyEarned > 0f)
        {
            content.AppendLine($"Money: ${summary.moneyEarned:F2}");
        }

        // Fish caught
        if (summary.fishCaught > 0)
        {
            content.AppendLine($"Fish Caught & Sold: {summary.fishCaught} fish");
        }

        // Materials
        if (summary.materialsGathered != null && summary.materialsGathered.Count > 0)
        {
            content.AppendLine($"Materials Collected:");
            foreach (var material in summary.materialsGathered)
            {
                content.AppendLine($"  - {FormatMaterialName(material.Key)}: {material.Value}");
            }
        }

        content.AppendLine();

        // Events section
        if (showOfflineEvents && summary.eventsOccurred != null && summary.eventsOccurred.Count > 0)
        {
            content.AppendLine("--- Events While You Were Away ---");
            foreach (string eventName in summary.eventsOccurred)
            {
                content.AppendLine($"• {eventName}");
            }
            content.AppendLine();
        }

        // Comeback bonus
        if (summary.comebackBonus.moneyBonus > 0f || summary.comebackBonus.rareBaitCount > 0 || summary.comebackBonus.relicCount > 0)
        {
            content.AppendLine("--- Comeback Bonus ---");
            content.AppendLine(summary.comebackBonus.bonusDescription);

            if (summary.comebackBonus.moneyBonus > 0f)
            {
                content.AppendLine($"  +${summary.comebackBonus.moneyBonus:F2}");
            }
            if (summary.comebackBonus.rareBaitCount > 0)
            {
                content.AppendLine($"  +{summary.comebackBonus.rareBaitCount} Rare Bait");
            }
            if (summary.comebackBonus.relicCount > 0)
            {
                content.AppendLine($"  +{summary.comebackBonus.relicCount} Relic");
            }

            content.AppendLine();
        }

        // Total summary
        float totalMoney = summary.moneyEarned + summary.comebackBonus.moneyBonus;
        if (totalMoney > 0f)
        {
            content.AppendLine($"TOTAL REWARDS: ${totalMoney:F2}");
        }

        content.AppendLine();
        content.AppendLine("[Collect Rewards]");

        return content.ToString();
    }

    /// <summary>
    /// Formats offline time into readable string.
    /// </summary>
    /// <param name="hours">Hours offline</param>
    /// <returns>Formatted time string</returns>
    private string FormatOfflineTime(float hours)
    {
        if (hours < 1f)
        {
            int minutes = Mathf.FloorToInt(hours * 60f);
            return $"You were away for: {minutes} minutes";
        }
        else if (hours < 24f)
        {
            int wholeHours = Mathf.FloorToInt(hours);
            int minutes = Mathf.FloorToInt((hours - wholeHours) * 60f);
            if (minutes > 0)
            {
                return $"You were away for: {wholeHours} hours {minutes} minutes";
            }
            else
            {
                return $"You were away for: {wholeHours} hours";
            }
        }
        else
        {
            int days = Mathf.FloorToInt(hours / 24f);
            int remainingHours = Mathf.FloorToInt(hours % 24f);
            if (remainingHours > 0)
            {
                return $"You were away for: {days} days {remainingHours} hours";
            }
            else
            {
                return $"You were away for: {days} days";
            }
        }
    }

    /// <summary>
    /// Formats material ID into readable name.
    /// </summary>
    /// <param name="materialID">Material identifier</param>
    /// <returns>Formatted material name</returns>
    private string FormatMaterialName(string materialID)
    {
        // Convert snake_case or camelCase to readable format
        string formatted = materialID.Replace("_", " ");

        // Capitalize first letter of each word
        StringBuilder result = new StringBuilder();
        bool capitalizeNext = true;

        foreach (char c in formatted)
        {
            if (c == ' ')
            {
                capitalizeNext = true;
                result.Append(c);
            }
            else if (capitalizeNext)
            {
                result.Append(char.ToUpper(c));
                capitalizeNext = false;
            }
            else
            {
                result.Append(c);
            }
        }

        return result.ToString();
    }

    /// <summary>
    /// Hides the welcome back UI.
    /// </summary>
    public void HideWelcomeBackUI()
    {
        if (!isShowingUI) return;

        isShowingUI = false;
        EventSystem.Publish("HideWelcomeBackUI", true);

        if (enableDebugLogging)
        {
            Debug.Log("[WelcomeBackSystem] Welcome Back UI hidden");
        }
    }

    /// <summary>
    /// Called when player collects offline rewards.
    /// </summary>
    public void OnRewardsCollected()
    {
        // Trigger collection in IdleManager
        IdleManager idleManager = IdleManager.Instance;
        if (idleManager != null)
        {
            idleManager.CollectOfflineRewards();
        }

        // Hide UI
        HideWelcomeBackUI();

        if (enableDebugLogging)
        {
            Debug.Log("[WelcomeBackSystem] Rewards collected");
        }
    }

    /// <summary>
    /// Gets whether the welcome UI is currently showing.
    /// </summary>
    /// <returns>True if UI is showing</returns>
    public bool IsShowingUI()
    {
        return isShowingUI;
    }

    /// <summary>
    /// Gets the current rewards summary being displayed.
    /// </summary>
    /// <returns>Current summary</returns>
    public OfflineRewardsSummary GetCurrentSummary()
    {
        return currentSummary;
    }

#if UNITY_EDITOR
    [ContextMenu("Test Welcome UI - Short Absence (1 hour)")]
    private void TestShortAbsence()
    {
        OfflineRewardsSummary summary = new OfflineRewardsSummary
        {
            offlineTimeHours = 1f,
            cappedTimeHours = 1f,
            moneyEarned = 250f,
            fishCaught = 8,
            materialsGathered = new Dictionary<string, int>
            {
                { "fish_scales", 5 },
                { "fish_bones", 3 }
            },
            eventsOccurred = new List<string>(),
            comebackBonus = new ComebackBonus
            {
                moneyBonus = 0f,
                bonusDescription = ""
            },
            wasCapped = false
        };

        ShowWelcomeBackUI(summary);
    }

    [ContextMenu("Test Welcome UI - Medium Absence (8 hours)")]
    private void TestMediumAbsence()
    {
        OfflineRewardsSummary summary = new OfflineRewardsSummary
        {
            offlineTimeHours = 8.5f,
            cappedTimeHours = 8.5f,
            moneyEarned = 2100f,
            fishCaught = 64,
            materialsGathered = new Dictionary<string, int>
            {
                { "fish_scales", 42 },
                { "fish_bones", 28 },
                { "fish_oil", 15 },
                { "scrap", 8 }
            },
            eventsOccurred = new List<string>
            {
                "Meteor Shower occurred (3 hours ago)",
                "Fish migration event (2 hours ago)"
            },
            comebackBonus = new ComebackBonus
            {
                moneyBonus = 0f,
                bonusDescription = ""
            },
            wasCapped = false
        };

        ShowWelcomeBackUI(summary);
    }

    [ContextMenu("Test Welcome UI - Long Absence (24 hours, capped)")]
    private void TestLongAbsence()
    {
        OfflineRewardsSummary summary = new OfflineRewardsSummary
        {
            offlineTimeHours = 30f,
            cappedTimeHours = 24f,
            moneyEarned = 48000f,
            fishCaught = 192,
            materialsGathered = new Dictionary<string, int>
            {
                { "fish_scales", 120 },
                { "fish_bones", 85 },
                { "fish_oil", 60 },
                { "scrap", 25 }
            },
            eventsOccurred = new List<string>
            {
                "Blood Moon occurred (12 hours ago) - 10x fish value!",
                "Meteor Shower (8 hours ago)",
                "Festival started (2 hours remaining)",
                "2 fish breeding pairs completed"
            },
            comebackBonus = new ComebackBonus
            {
                moneyBonus = 500f,
                rareBaitCount = 0,
                relicCount = 0,
                bonusDescription = "Daily Comeback Bonus"
            },
            wasCapped = true
        };

        ShowWelcomeBackUI(summary);
    }

    [ContextMenu("Test Welcome UI - Extended Absence (3 days)")]
    private void TestExtendedAbsence()
    {
        OfflineRewardsSummary summary = new OfflineRewardsSummary
        {
            offlineTimeHours = 72f,
            cappedTimeHours = 24f,
            moneyEarned = 48000f,
            fishCaught = 192,
            materialsGathered = new Dictionary<string, int>
            {
                { "fish_scales", 120 },
                { "fish_bones", 85 },
                { "fish_oil", 60 },
                { "scrap", 25 }
            },
            eventsOccurred = new List<string>
            {
                "Multiple events occurred during your absence",
                "Check event log for details"
            },
            comebackBonus = new ComebackBonus
            {
                moneyBonus = 2000f,
                rareBaitCount = 1,
                relicCount = 1,
                bonusDescription = "3-Day Comeback Bonus"
            },
            wasCapped = true
        };

        ShowWelcomeBackUI(summary);
    }
#endif
}

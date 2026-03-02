using UnityEngine;

/// <summary>
/// Agent 20: Idle/AFK Progression System - IdleProgressionCalculator.cs
/// Calculates offline earnings with diminishing returns and upgrade bonuses.
/// Factors in time away, upgrades owned, location efficiency, and idle boost items.
/// </summary>
public class IdleProgressionCalculator : MonoBehaviour
{
    [Header("Progression Curves")]
    [Tooltip("Diminishing returns curve for extended offline periods")]
    [SerializeField] private AnimationCurve diminishingReturnsCurve;

    [Header("Base Income Rates")]
    [Tooltip("Base income per hour from idle fishing (without upgrades)")]
    [SerializeField] private float baseIdleIncomePerHour = 200f;

    [Tooltip("Maximum income per hour with all upgrades")]
    [SerializeField] private float maxIdleIncomePerHour = 2000f;

    [Header("Configuration")]
    [Tooltip("Apply diminishing returns after this many hours")]
    [SerializeField] private float diminishingReturnsThreshold = 12f;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogging = false;

    // Component references
    private IdleUpgradeSystem upgradeSystem;

    private void Awake()
    {
        // Initialize diminishing returns curve if not set
        if (diminishingReturnsCurve == null || diminishingReturnsCurve.length == 0)
        {
            InitializeDefaultCurve();
        }

        upgradeSystem = GetComponent<IdleUpgradeSystem>();
        if (upgradeSystem == null)
        {
            Debug.LogWarning("[IdleProgressionCalculator] IdleUpgradeSystem not found!");
        }
    }

    /// <summary>
    /// Initializes a default diminishing returns curve.
    /// Linear until threshold, then gradually flattens.
    /// </summary>
    private void InitializeDefaultCurve()
    {
        diminishingReturnsCurve = new AnimationCurve();

        // 0-12 hours: Linear (100% efficiency)
        diminishingReturnsCurve.AddKey(0f, 0f);
        diminishingReturnsCurve.AddKey(diminishingReturnsThreshold, diminishingReturnsThreshold);

        // 12-18 hours: 80% efficiency
        diminishingReturnsCurve.AddKey(18f, diminishingReturnsThreshold + ((18f - diminishingReturnsThreshold) * 0.8f));

        // 18-24 hours: 60% efficiency
        diminishingReturnsCurve.AddKey(24f, diminishingReturnsThreshold + ((18f - diminishingReturnsThreshold) * 0.8f) + ((24f - 18f) * 0.6f));

        // 24+ hours: Capped (handled elsewhere)

        // Set all tangents to linear for smooth transitions
        for (int i = 0; i < diminishingReturnsCurve.length; i++)
        {
            AnimationUtility.SetKeyLeftTangentMode(diminishingReturnsCurve, i, AnimationUtility.TangentMode.Linear);
            AnimationUtility.SetKeyRightTangentMode(diminishingReturnsCurve, i, AnimationUtility.TangentMode.Linear);
        }
    }

    /// <summary>
    /// Calculates total offline earnings based on time and upgrades.
    /// </summary>
    /// <param name="hoursOffline">Number of hours player was offline</param>
    /// <returns>Total earnings from offline time</returns>
    public float CalculateOfflineEarnings(float hoursOffline)
    {
        if (hoursOffline <= 0f) return 0f;

        // Check if Auto-Fisher is owned
        if (upgradeSystem == null || !upgradeSystem.HasUpgrade("auto_fisher"))
        {
            return 0f;
        }

        // Apply time compression
        float compressedTime = hoursOffline * upgradeSystem.GetTimeCompressionMultiplier();

        // Apply diminishing returns
        float effectiveTime = ApplyDiminishingReturns(compressedTime);

        // Calculate base income
        float incomePerHour = CalculateIncomePerHour();

        // Total earnings
        float totalEarnings = effectiveTime * incomePerHour;

        if (enableDebugLogging)
        {
            Debug.Log($"[IdleProgressionCalculator] Offline Earnings Calculation:");
            Debug.Log($"  Hours Offline: {hoursOffline:F2}");
            Debug.Log($"  Time Compression: {upgradeSystem.GetTimeCompressionMultiplier():F2}x");
            Debug.Log($"  Compressed Time: {compressedTime:F2} hours");
            Debug.Log($"  Effective Time (after diminishing returns): {effectiveTime:F2} hours");
            Debug.Log($"  Income Per Hour: ${incomePerHour:F2}");
            Debug.Log($"  Total Earnings: ${totalEarnings:F2}");
        }

        return totalEarnings;
    }

    /// <summary>
    /// Calculates income per hour based on upgrades and location.
    /// </summary>
    /// <returns>Income per hour</returns>
    private float CalculateIncomePerHour()
    {
        float incomePerHour = baseIdleIncomePerHour;

        if (upgradeSystem == null) return incomePerHour;

        // Apply Efficiency Boost (+10% per level, max 10 levels)
        int efficiencyLevel = upgradeSystem.GetUpgradeLevel("efficiency_boost");
        float efficiencyMultiplier = 1f + (efficiencyLevel * 0.1f);
        incomePerHour *= efficiencyMultiplier;

        // Apply Quality Rod Holder (+20% per level, max 5 levels)
        int rodHolderLevel = upgradeSystem.GetUpgradeLevel("quality_rod_holder");
        float rodHolderMultiplier = 1f + (rodHolderLevel * 0.2f);
        incomePerHour *= rodHolderMultiplier;

        // Apply location efficiency
        float locationEfficiency = GetLocationEfficiency();
        incomePerHour *= locationEfficiency;

        // Apply crew bonus if Crew Autonomy is owned
        if (upgradeSystem.HasUpgrade("crew_autonomy"))
        {
            float crewBonus = GetCrewIdleBonus();
            incomePerHour *= crewBonus;
        }

        // Cap at maximum
        incomePerHour = Mathf.Min(incomePerHour, maxIdleIncomePerHour);

        return incomePerHour;
    }

    /// <summary>
    /// Applies diminishing returns to extended offline periods.
    /// </summary>
    /// <param name="hours">Hours offline (after time compression)</param>
    /// <returns>Effective hours for earnings calculation</returns>
    private float ApplyDiminishingReturns(float hours)
    {
        if (hours <= diminishingReturnsThreshold)
        {
            return hours; // Full efficiency
        }

        // Use curve for diminishing returns
        return diminishingReturnsCurve.Evaluate(hours);
    }

    /// <summary>
    /// Gets the location efficiency multiplier.
    /// </summary>
    /// <returns>Multiplier between 0.8 and 1.5</returns>
    private float GetLocationEfficiency()
    {
        // TODO: Integrate with location system (Agent 14)
        // Different locations have different idle efficiency:
        // - Starter Lake: 1.0x
        // - River Delta: 1.1x
        // - Deep Ocean: 1.5x
        // - Swamp: 0.9x
        // - Cursed Waters: 0.8x (high risk, but active play has better rewards)

        return 1.0f; // Default for now
    }

    /// <summary>
    /// Gets the crew idle bonus multiplier.
    /// </summary>
    /// <returns>Multiplier based on crew members</returns>
    private float GetCrewIdleBonus()
    {
        // TODO: Integrate with crew system (Agent 17)
        // Crew members provide bonuses to idle income:
        // - Fisherman: +20% idle catch rate
        // - Navigator: Can work in multiple locations
        // - Cook: Increases fish value by 15%
        // - Engineer: +10% overall efficiency

        return 1.0f; // Default for now
    }

    /// <summary>
    /// Estimates total earnings for a given time period.
    /// Useful for UI display and player planning.
    /// </summary>
    /// <param name="hours">Number of hours to estimate</param>
    /// <returns>Estimated total earnings</returns>
    public float EstimateTotalEarnings(float hours)
    {
        return CalculateOfflineEarnings(hours);
    }

    /// <summary>
    /// Gets the current income per hour rate.
    /// </summary>
    /// <returns>Current income per hour</returns>
    public float GetCurrentIncomePerHour()
    {
        return CalculateIncomePerHour();
    }

    /// <summary>
    /// Gets the maximum possible income per hour.
    /// </summary>
    /// <returns>Maximum income per hour</returns>
    public float GetMaxIncomePerHour()
    {
        return maxIdleIncomePerHour;
    }

    /// <summary>
    /// Calculates the progress toward maximum idle income (0-1).
    /// </summary>
    /// <returns>Progress percentage (0-1)</returns>
    public float GetIdleIncomeProgress()
    {
        float currentIncome = CalculateIncomePerHour();
        return Mathf.Clamp01(currentIncome / maxIdleIncomePerHour);
    }

#if UNITY_EDITOR
    [ContextMenu("Test Earnings - 1 Hour")]
    private void TestOneHour()
    {
        float earnings = CalculateOfflineEarnings(1f);
        Debug.Log($"[IdleProgressionCalculator] 1 Hour Earnings: ${earnings:F2}");
    }

    [ContextMenu("Test Earnings - 8 Hours")]
    private void TestEightHours()
    {
        float earnings = CalculateOfflineEarnings(8f);
        Debug.Log($"[IdleProgressionCalculator] 8 Hours Earnings: ${earnings:F2}");
    }

    [ContextMenu("Test Earnings - 24 Hours")]
    private void TestTwentyFourHours()
    {
        float earnings = CalculateOfflineEarnings(24f);
        Debug.Log($"[IdleProgressionCalculator] 24 Hours Earnings: ${earnings:F2}");
    }

    [ContextMenu("Test Earnings - 48 Hours (With Diminishing Returns)")]
    private void TestFortyEightHours()
    {
        float earnings = CalculateOfflineEarnings(48f);
        Debug.Log($"[IdleProgressionCalculator] 48 Hours Earnings: ${earnings:F2}");
        Debug.Log($"Note: Capped at 24 hours in actual gameplay");
    }

    [ContextMenu("Print Income Breakdown")]
    private void PrintIncomeBreakdown()
    {
        Debug.Log($"=== Idle Income Breakdown ===");
        Debug.Log($"Base Income Per Hour: ${baseIdleIncomePerHour:F2}");
        Debug.Log($"Current Income Per Hour: ${GetCurrentIncomePerHour():F2}");
        Debug.Log($"Max Income Per Hour: ${maxIdleIncomePerHour:F2}");
        Debug.Log($"Progress: {GetIdleIncomeProgress() * 100f:F1}%");
        Debug.Log($"");
        Debug.Log($"Estimated Earnings:");
        Debug.Log($"  1 Hour: ${EstimateTotalEarnings(1f):F2}");
        Debug.Log($"  4 Hours: ${EstimateTotalEarnings(4f):F2}");
        Debug.Log($"  8 Hours: ${EstimateTotalEarnings(8f):F2}");
        Debug.Log($"  12 Hours: ${EstimateTotalEarnings(12f):F2}");
        Debug.Log($"  24 Hours: ${EstimateTotalEarnings(24f):F2}");
    }

    [ContextMenu("Test Diminishing Returns Curve")]
    private void TestDiminishingReturnsCurve()
    {
        Debug.Log($"=== Diminishing Returns Curve Test ===");

        float[] testHours = { 1f, 4f, 8f, 12f, 16f, 20f, 24f, 30f, 48f };

        foreach (float hours in testHours)
        {
            float effectiveHours = ApplyDiminishingReturns(hours);
            float efficiency = (effectiveHours / hours) * 100f;
            Debug.Log($"{hours:F0}h → {effectiveHours:F2}h effective ({efficiency:F1}% efficiency)");
        }
    }
#endif
}

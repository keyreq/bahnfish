using UnityEngine;

/// <summary>
/// Standard fishing minigame with timing-based tension management.
/// Player must balance reeling in vs letting out line based on fish behavior.
/// Inspired by Cast n Chill's active combat system.
/// </summary>
public class ReelMinigame : BaseMinigame
{
    [Header("Minigame Settings")]
    private float minigameDuration = 30f; // Max time to catch fish
    private float timeElapsed = 0f;

    [Header("Difficulty Scaling")]
    private float difficultyMultiplier = 1f;

    private bool isActive = false;

    public override void Initialize(Fish fish, BaseFishingTool tool, TensionSystem tensionSystem)
    {
        base.Initialize(fish, tool, tensionSystem);

        // Scale difficulty based on fish rarity
        switch (fish.rarity)
        {
            case FishRarity.Common:
                difficultyMultiplier = 1f;
                minigameDuration = 20f;
                break;
            case FishRarity.Uncommon:
                difficultyMultiplier = 1.3f;
                minigameDuration = 25f;
                break;
            case FishRarity.Rare:
                difficultyMultiplier = 1.6f;
                minigameDuration = 30f;
                break;
            case FishRarity.Legendary:
                difficultyMultiplier = 2f;
                minigameDuration = 40f;
                break;
            case FishRarity.Aberrant:
                difficultyMultiplier = 2.5f;
                minigameDuration = 45f;
                break;
        }

        Debug.Log($"Reel Minigame initialized - Difficulty: {difficultyMultiplier}x, Duration: {minigameDuration}s");
    }

    public override void OnMinigameStart()
    {
        isActive = true;
        timeElapsed = 0f;

        // Show minigame UI
        ShowMinigameUI();

        // Play start sound
        Debug.Log("Reel Minigame Started! Balance tension to reel in the fish.");
    }

    public override void UpdateMinigame(float deltaTime)
    {
        if (!isActive)
            return;

        timeElapsed += deltaTime;

        // Check for timeout
        if (timeElapsed >= minigameDuration)
        {
            // Fish escaped due to timeout
            Debug.Log("Fish escaped - took too long!");
            EventSystem.Publish("OnLineBroken");
            OnMinigameEnd();
            return;
        }

        // Tension system handles all the input and mechanics
        // We just need to check for completion conditions

        // Success condition is handled by TensionSystem when progress reaches 100%
        // Failure condition is handled by TensionSystem when line breaks

        // Update UI
        UpdateMinigameUI();
    }

    public override void OnMinigameEnd()
    {
        isActive = false;

        // Hide minigame UI
        HideMinigameUI();

        // Clean up
        Destroy(this);
    }

    #region UI Management

    private void ShowMinigameUI()
    {
        // Will integrate with Agent 11 (UI/UX)
        // For now, tension system shows debug UI
        Debug.Log("Show Reel Minigame UI");
    }

    private void UpdateMinigameUI()
    {
        // Update tension bar, progress bar, fish behavior indicators
        // Handled by TensionSystem debug UI for now
    }

    private void HideMinigameUI()
    {
        Debug.Log("Hide Reel Minigame UI");
    }

    #endregion

    #region Visual Feedback

    private void OnGUI()
    {
        if (!isActive)
            return;

        if (Debug.isDebugBuild)
        {
            GUIStyle style = new GUIStyle();
            style.fontSize = 24;
            style.normal.textColor = Color.white;
            style.alignment = TextAnchor.UpperCenter;

            float remainingTime = minigameDuration - timeElapsed;
            string timeText = $"Time Remaining: {remainingTime:F1}s";

            GUI.Label(new Rect(0, 50, Screen.width, 50), timeText, style);

            // Instructions
            style.fontSize = 18;
            style.normal.textColor = Color.yellow;
            GUI.Label(new Rect(0, 90, Screen.width, 50), "Watch the tension! Let out line when fish fights!", style);
        }
    }

    #endregion
}

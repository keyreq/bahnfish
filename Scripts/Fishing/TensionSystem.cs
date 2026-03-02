using UnityEngine;

/// <summary>
/// Manages line tension during fishing encounters.
/// Creates active combat-style gameplay inspired by Cast n Chill.
/// Fish fight back, creating tension spikes that players must manage.
/// </summary>
public class TensionSystem : MonoBehaviour
{
    [Header("Tension Settings")]
    [SerializeField] private float maxTension = 100f;
    [SerializeField] private float breakThreshold = 95f;
    [SerializeField] private float breakThresholdDuration = 2f; // How long at 95%+ before break
    [SerializeField] private float tensionDecayRate = 10f; // Tension decreases when letting out line

    [Header("Fish Resistance")]
    [SerializeField] private float baseResistance = 15f;
    [SerializeField] private float rarityMultiplier = 1.5f;
    [SerializeField] private float weightMultiplier = 2f;

    [Header("Player Control")]
    [SerializeField] private float reelInTensionIncrease = 25f;
    [SerializeField] private float letOutTensionDecrease = 20f;

    // Current state
    private float currentTension = 0f;
    private float timeAtDangerLevel = 0f;
    private bool isInitialized = false;

    // Fish data
    private Fish currentFish;
    private BaseFishingTool currentTool;
    private float fishResistance;

    // Fish behavior patterns
    private float fishBehaviorTimer = 0f;
    private float fishBehaviorInterval = 0f;
    private bool fishIsFighting = false;
    private float fightDuration = 0f;

    // Input tracking
    private bool isReelingIn = false;
    private bool isLettingOut = false;

    #region Initialization

    public void Initialize(Fish fish, BaseFishingTool tool)
    {
        currentFish = fish;
        currentTool = tool;
        currentTension = 0f;
        timeAtDangerLevel = 0f;
        fishBehaviorTimer = 0f;
        isInitialized = true;

        // Calculate fish resistance based on properties
        CalculateFishResistance();

        // Set initial behavior interval
        fishBehaviorInterval = Random.Range(1f, 3f);

        Debug.Log($"Tension System initialized - Fish: {fish.name}, Resistance: {fishResistance}");
    }

    public void Reset()
    {
        isInitialized = false;
        currentTension = 0f;
        currentFish = null;
        currentTool = null;
        fishIsFighting = false;
        isReelingIn = false;
        isLettingOut = false;
    }

    #endregion

    #region Update Loop

    private void Update()
    {
        if (!isInitialized)
            return;

        // Get player input
        HandleInput();

        // Update fish behavior
        UpdateFishBehavior();

        // Update tension based on actions
        UpdateTension();

        // Check for line break
        CheckLineBreak();

        // Publish tension update event
        PublishTensionUpdate();
    }

    #endregion

    #region Input Handling

    private void HandleInput()
    {
        // Reel In: Space bar or gamepad A button
        isReelingIn = Input.GetKey(KeyCode.Space) || Input.GetButton("Fire1");

        // Let Out Line: Left Shift or gamepad B button
        isLettingOut = Input.GetKey(KeyCode.LeftShift) || Input.GetButton("Fire2");

        // Can't do both at once
        if (isReelingIn && isLettingOut)
        {
            isReelingIn = false;
            isLettingOut = false;
        }
    }

    #endregion

    #region Fish Behavior

    private void CalculateFishResistance()
    {
        if (currentFish == null)
            return;

        // Base resistance
        float resistance = baseResistance;

        // Rarity modifier
        switch (currentFish.rarity)
        {
            case FishRarity.Common:
                resistance *= 1f;
                break;
            case FishRarity.Uncommon:
                resistance *= 1.3f;
                break;
            case FishRarity.Rare:
                resistance *= 1.6f;
                break;
            case FishRarity.Legendary:
                resistance *= 2f;
                break;
            case FishRarity.Aberrant:
                resistance *= 2.5f; // Aberrant fish are unpredictable
                break;
        }

        // Weight modifier (heavier fish fight harder)
        resistance += currentFish.weight * weightMultiplier;

        // Tool power reduces effective resistance
        if (currentTool != null)
        {
            resistance /= currentTool.power;
        }

        fishResistance = resistance;
    }

    private void UpdateFishBehavior()
    {
        fishBehaviorTimer += Time.deltaTime;

        if (fishBehaviorTimer >= fishBehaviorInterval)
        {
            // Fish changes behavior
            fishBehaviorTimer = 0f;

            // Determine if fish will fight back
            float fightChance = GetFightChance();

            if (Random.value < fightChance)
            {
                // Fish starts fighting!
                fishIsFighting = true;
                fightDuration = Random.Range(1f, 3f);

                // Aberrant fish have longer, more intense fights
                if (currentFish.isAberrant)
                {
                    fightDuration *= 1.5f;
                }

                Debug.Log($"{currentFish.name} is fighting back!");
            }
            else
            {
                // Fish is calm
                fishIsFighting = false;
            }

            // Set next behavior interval
            fishBehaviorInterval = Random.Range(2f, 5f);
        }

        // Update fight duration
        if (fishIsFighting)
        {
            fightDuration -= Time.deltaTime;
            if (fightDuration <= 0f)
            {
                fishIsFighting = false;
                Debug.Log($"{currentFish.name} stopped fighting");
            }
        }
    }

    private float GetFightChance()
    {
        // Base chance
        float chance = 0.4f;

        // Rarity increases fight frequency
        switch (currentFish.rarity)
        {
            case FishRarity.Common:
                chance = 0.3f;
                break;
            case FishRarity.Uncommon:
                chance = 0.4f;
                break;
            case FishRarity.Rare:
                chance = 0.5f;
                break;
            case FishRarity.Legendary:
                chance = 0.7f;
                break;
            case FishRarity.Aberrant:
                chance = 0.8f; // Very aggressive
                break;
        }

        // If tension is high, fish fights more
        if (currentTension > 60f)
        {
            chance += 0.2f;
        }

        // If player is reeling aggressively, fish fights back
        if (isReelingIn)
        {
            chance += 0.15f;
        }

        return Mathf.Clamp01(chance);
    }

    #endregion

    #region Tension Management

    private void UpdateTension()
    {
        float tensionChange = 0f;

        // Fish resistance always adds some tension
        if (fishIsFighting)
        {
            // Fighting fish creates MASSIVE tension spike
            tensionChange += fishResistance * 2f * Time.deltaTime;
        }
        else
        {
            // Passive resistance (fish naturally pulls)
            tensionChange += fishResistance * 0.3f * Time.deltaTime;
        }

        // Player actions
        if (isReelingIn)
        {
            // Reeling in increases tension
            tensionChange += reelInTensionIncrease * Time.deltaTime;
        }
        else if (isLettingOut)
        {
            // Letting out decreases tension
            tensionChange -= letOutTensionDecrease * Time.deltaTime;
        }
        else
        {
            // Natural decay when doing nothing
            tensionChange -= tensionDecayRate * Time.deltaTime;
        }

        // Apply change
        currentTension += tensionChange;

        // Clamp tension
        currentTension = Mathf.Clamp(currentTension, 0f, maxTension);
    }

    private void CheckLineBreak()
    {
        if (currentTension >= breakThreshold)
        {
            timeAtDangerLevel += Time.deltaTime;

            if (timeAtDangerLevel >= breakThresholdDuration)
            {
                // Line breaks!
                BreakLine();
            }
        }
        else
        {
            // Reset danger timer
            timeAtDangerLevel = 0f;
        }
    }

    private void BreakLine()
    {
        Debug.Log("Line broke!");
        EventSystem.Publish("OnLineBroken");
        Reset();
    }

    #endregion

    #region Progress Tracking

    /// <summary>
    /// Calculate catch progress (0-100%)
    /// Progress increases when reeling at safe tension levels
    /// </summary>
    private float catchProgress = 0f;

    private void UpdateCatchProgress()
    {
        if (!isInitialized)
            return;

        if (isReelingIn && currentTension < 80f) // Can make progress when tension is manageable
        {
            // Progress based on tool power and fish difficulty
            float progressGain = (currentTool.power / fishResistance) * 5f * Time.deltaTime;
            catchProgress += progressGain;

            catchProgress = Mathf.Clamp(catchProgress, 0f, 100f);

            if (catchProgress >= 100f)
            {
                // Fish caught!
                EventSystem.Publish("OnFishCaught", currentFish);
                Reset();
            }
        }
        else if (fishIsFighting && !isLettingOut)
        {
            // Lose progress if fish is fighting and you're not managing tension
            catchProgress -= 3f * Time.deltaTime;
            catchProgress = Mathf.Max(0f, catchProgress);
        }
    }

    #endregion

    #region Events & UI

    private void PublishTensionUpdate()
    {
        TensionUpdateData data = new TensionUpdateData
        {
            currentTension = currentTension,
            maxTension = maxTension,
            tensionPercentage = (currentTension / maxTension) * 100f,
            isDanger = currentTension >= breakThreshold,
            fishIsFighting = fishIsFighting,
            catchProgress = catchProgress
        };

        EventSystem.Publish("OnTensionUpdated", data);
    }

    #endregion

    #region Public API

    public float GetCurrentTension() => currentTension;
    public float GetTensionPercentage() => (currentTension / maxTension) * 100f;
    public bool IsDangerLevel() => currentTension >= breakThreshold;
    public bool IsFishFighting() => fishIsFighting;
    public float GetCatchProgress() => catchProgress;

    #endregion

    #region Debug Visualization

    private void OnGUI()
    {
        if (!isInitialized)
            return;

        // Debug tension meter
        if (Debug.isDebugBuild)
        {
            GUIStyle style = new GUIStyle();
            style.fontSize = 20;
            style.normal.textColor = Color.white;

            float barWidth = 300f;
            float barHeight = 30f;
            float x = Screen.width / 2 - barWidth / 2;
            float y = Screen.height - 150f;

            // Background
            GUI.color = Color.black;
            GUI.DrawTexture(new Rect(x - 2, y - 2, barWidth + 4, barHeight + 4), Texture2D.whiteTexture);

            // Tension bar
            float tensionWidth = (currentTension / maxTension) * barWidth;
            GUI.color = GetTensionColor();
            GUI.DrawTexture(new Rect(x, y, tensionWidth, barHeight), Texture2D.whiteTexture);

            // Text
            GUI.color = Color.white;
            GUI.Label(new Rect(x, y - 30, barWidth, 30), $"Tension: {currentTension:F1}% {(fishIsFighting ? "[FIGHTING!]" : "")}", style);
            GUI.Label(new Rect(x, y + barHeight + 5, barWidth, 30), $"Progress: {catchProgress:F1}%", style);

            // Controls hint
            GUI.Label(new Rect(x, y + barHeight + 35, barWidth, 30), "SPACE: Reel In | SHIFT: Let Out", style);
        }
    }

    private Color GetTensionColor()
    {
        float percentage = GetTensionPercentage();

        if (percentage >= 90f)
            return Color.red; // Danger!
        else if (percentage >= 70f)
            return Color.yellow; // Warning
        else
            return Color.green; // Safe
    }

    #endregion
}

/// <summary>
/// Data structure for tension update events
/// </summary>
[System.Serializable]
public struct TensionUpdateData
{
    public float currentTension;
    public float maxTension;
    public float tensionPercentage;
    public bool isDanger;
    public bool fishIsFighting;
    public float catchProgress;
}

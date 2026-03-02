using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Displays player resources (money, fuel, scrap, relics) on the HUD.
/// Animates value changes with count up/down effect.
/// Position: Top-right corner of screen.
/// </summary>
public class ResourceDisplay : MonoBehaviour
{
    #region Inspector References
    [Header("Money Display")]
    [Tooltip("Text component for money amount")]
    public TextMeshProUGUI moneyText;

    [Tooltip("Icon for money")]
    public Image moneyIcon;

    [Tooltip("Currency symbol")]
    public string currencySymbol = "$";

    [Header("Fuel Display")]
    [Tooltip("Text component for fuel amount")]
    public TextMeshProUGUI fuelText;

    [Tooltip("Icon for fuel")]
    public Image fuelIcon;

    [Tooltip("Fuel fill bar (optional)")]
    public Image fuelFillBar;

    [Tooltip("Show fuel as percentage")]
    public bool showFuelAsPercentage = false;

    [Header("Scrap Display")]
    [Tooltip("Text component for scrap amount")]
    public TextMeshProUGUI scrapText;

    [Tooltip("Icon for scrap")]
    public Image scrapIcon;

    [Header("Relics Display")]
    [Tooltip("Text component for relics amount")]
    public TextMeshProUGUI relicsText;

    [Tooltip("Icon for relics")]
    public Image relicsIcon;

    [Header("Display Settings")]
    [Tooltip("Show/hide individual resource displays")]
    public bool showMoney = true;
    public bool showFuel = true;
    public bool showScrap = true;
    public bool showRelics = true;

    [Header("Animation")]
    [Tooltip("Animate value changes")]
    public bool animateValueChanges = true;

    [Tooltip("Duration of count animation")]
    [Range(0.1f, 2f)]
    public float animationDuration = 0.5f;

    [Tooltip("Flash on value increase")]
    public bool flashOnIncrease = true;

    [Tooltip("Flash color (green for gain)")]
    public Color increaseFlashColor = new Color(0.2f, 1f, 0.2f);

    [Tooltip("Flash on value decrease")]
    public bool flashOnDecrease = true;

    [Tooltip("Flash color (red for loss)")]
    public Color decreaseFlashColor = new Color(1f, 0.2f, 0.2f);

    [Tooltip("Flash duration")]
    [Range(0.1f, 1f)]
    public float flashDuration = 0.3f;

    [Header("Warnings")]
    [Tooltip("Warn when fuel is low")]
    public bool warnLowFuel = true;

    [Tooltip("Fuel percentage to trigger warning")]
    [Range(0f, 50f)]
    public float lowFuelThreshold = 20f;

    [Tooltip("Low fuel warning color")]
    public Color lowFuelColor = new Color(1f, 0.5f, 0f);

    [Header("Debug")]
    [Tooltip("Enable debug logging")]
    public bool enableDebugLogging = false;
    #endregion

    #region Private Variables
    private float currentMoney = 0f;
    private float currentFuel = 100f;
    private int currentScrap = 0;
    private int currentRelics = 0;

    private float displayMoney = 0f;
    private float displayFuel = 100f;
    private int displayScrap = 0;
    private int displayRelics = 0;

    private Coroutine moneyAnimCoroutine;
    private Coroutine fuelAnimCoroutine;
    private Coroutine scrapAnimCoroutine;
    private Coroutine relicsAnimCoroutine;

    private Color originalMoneyColor;
    private Color originalFuelColor;
    private Color originalScrapColor;
    private Color originalRelicsColor;
    #endregion

    #region Unity Lifecycle

    private void Start()
    {
        // Store original colors
        StoreOriginalColors();

        // Subscribe to events
        EventSystem.Subscribe<GameState>("GameStateUpdated", OnGameStateUpdated);
        EventSystem.Subscribe<float>("OnMoneyChanged", OnMoneyChanged);
        EventSystem.Subscribe<float>("OnFuelChanged", OnFuelChanged);

        // Initialize with current game state
        if (GameManager.Instance != null)
        {
            GameState state = GameManager.Instance.CurrentGameState;
            InitializeValues(state.money, state.fuel);
        }

        // Update visibility
        UpdateVisibility();

        if (enableDebugLogging)
        {
            Debug.Log("[ResourceDisplay] Initialized");
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        EventSystem.Unsubscribe<GameState>("GameStateUpdated", OnGameStateUpdated);
        EventSystem.Unsubscribe<float>("OnMoneyChanged", OnMoneyChanged);
        EventSystem.Unsubscribe<float>("OnFuelChanged", OnFuelChanged);
    }

    #endregion

    #region Initialization

    /// <summary>
    /// Store original text colors for flash effects
    /// </summary>
    private void StoreOriginalColors()
    {
        if (moneyText != null)
            originalMoneyColor = moneyText.color;
        if (fuelText != null)
            originalFuelColor = fuelText.color;
        if (scrapText != null)
            originalScrapColor = scrapText.color;
        if (relicsText != null)
            originalRelicsColor = relicsText.color;
    }

    /// <summary>
    /// Initialize values without animation
    /// </summary>
    private void InitializeValues(float money, float fuel)
    {
        currentMoney = money;
        currentFuel = fuel;
        displayMoney = money;
        displayFuel = fuel;

        UpdateMoneyDisplay();
        UpdateFuelDisplay();
        UpdateScrapDisplay();
        UpdateRelicsDisplay();
    }

    #endregion

    #region Event Handlers

    /// <summary>
    /// Called when game state updates
    /// </summary>
    private void OnGameStateUpdated(GameState state)
    {
        if (state == null)
            return;

        SetMoney(state.money);
        SetFuel(state.fuel);
    }

    /// <summary>
    /// Called when money changes (from Agent 9)
    /// </summary>
    private void OnMoneyChanged(float newMoney)
    {
        SetMoney(newMoney);
    }

    /// <summary>
    /// Called when fuel changes
    /// </summary>
    private void OnFuelChanged(float newFuel)
    {
        SetFuel(newFuel);
    }

    #endregion

    #region Resource Setters

    /// <summary>
    /// Set money value with optional animation
    /// </summary>
    public void SetMoney(float newMoney)
    {
        float oldMoney = currentMoney;
        currentMoney = Mathf.Max(0f, newMoney);

        if (animateValueChanges)
        {
            if (moneyAnimCoroutine != null)
                StopCoroutine(moneyAnimCoroutine);
            moneyAnimCoroutine = StartCoroutine(AnimateMoney(displayMoney, currentMoney));

            // Flash on change
            if (newMoney > oldMoney && flashOnIncrease)
                StartCoroutine(FlashText(moneyText, increaseFlashColor));
            else if (newMoney < oldMoney && flashOnDecrease)
                StartCoroutine(FlashText(moneyText, decreaseFlashColor));
        }
        else
        {
            displayMoney = currentMoney;
            UpdateMoneyDisplay();
        }

        if (enableDebugLogging)
        {
            Debug.Log($"[ResourceDisplay] Money: {oldMoney:F0} → {currentMoney:F0}");
        }
    }

    /// <summary>
    /// Set fuel value with optional animation
    /// </summary>
    public void SetFuel(float newFuel)
    {
        float oldFuel = currentFuel;
        currentFuel = Mathf.Clamp(newFuel, 0f, 100f);

        if (animateValueChanges)
        {
            if (fuelAnimCoroutine != null)
                StopCoroutine(fuelAnimCoroutine);
            fuelAnimCoroutine = StartCoroutine(AnimateFuel(displayFuel, currentFuel));

            // Flash on change
            if (newFuel < oldFuel && flashOnDecrease)
                StartCoroutine(FlashText(fuelText, decreaseFlashColor));
        }
        else
        {
            displayFuel = currentFuel;
            UpdateFuelDisplay();
        }

        if (enableDebugLogging)
        {
            Debug.Log($"[ResourceDisplay] Fuel: {oldFuel:F1}% → {currentFuel:F1}%");
        }
    }

    /// <summary>
    /// Set scrap value with optional animation
    /// </summary>
    public void SetScrap(int newScrap)
    {
        int oldScrap = currentScrap;
        currentScrap = Mathf.Max(0, newScrap);

        if (animateValueChanges)
        {
            if (scrapAnimCoroutine != null)
                StopCoroutine(scrapAnimCoroutine);
            scrapAnimCoroutine = StartCoroutine(AnimateScrap(displayScrap, currentScrap));

            // Flash on change
            if (newScrap > oldScrap && flashOnIncrease)
                StartCoroutine(FlashText(scrapText, increaseFlashColor));
            else if (newScrap < oldScrap && flashOnDecrease)
                StartCoroutine(FlashText(scrapText, decreaseFlashColor));
        }
        else
        {
            displayScrap = currentScrap;
            UpdateScrapDisplay();
        }
    }

    /// <summary>
    /// Set relics value with optional animation
    /// </summary>
    public void SetRelics(int newRelics)
    {
        int oldRelics = currentRelics;
        currentRelics = Mathf.Max(0, newRelics);

        if (animateValueChanges)
        {
            if (relicsAnimCoroutine != null)
                StopCoroutine(relicsAnimCoroutine);
            relicsAnimCoroutine = StartCoroutine(AnimateRelics(displayRelics, currentRelics));

            // Flash on change
            if (newRelics > oldRelics && flashOnIncrease)
                StartCoroutine(FlashText(relicsText, increaseFlashColor));
        }
        else
        {
            displayRelics = currentRelics;
            UpdateRelicsDisplay();
        }
    }

    #endregion

    #region Display Updates

    /// <summary>
    /// Update money text display
    /// </summary>
    private void UpdateMoneyDisplay()
    {
        if (moneyText != null)
        {
            moneyText.text = $"{currencySymbol}{displayMoney:F0}";
        }
    }

    /// <summary>
    /// Update fuel text and bar display
    /// </summary>
    private void UpdateFuelDisplay()
    {
        if (fuelText != null)
        {
            if (showFuelAsPercentage)
            {
                fuelText.text = $"{displayFuel:F0}%";
            }
            else
            {
                fuelText.text = $"{displayFuel:F1}";
            }

            // Apply low fuel warning color
            if (warnLowFuel && displayFuel <= lowFuelThreshold)
            {
                fuelText.color = lowFuelColor;
            }
            else
            {
                fuelText.color = originalFuelColor;
            }
        }

        if (fuelFillBar != null)
        {
            fuelFillBar.fillAmount = displayFuel / 100f;

            // Color the fuel bar based on level
            if (displayFuel <= lowFuelThreshold)
            {
                fuelFillBar.color = lowFuelColor;
            }
            else
            {
                fuelFillBar.color = Color.white;
            }
        }
    }

    /// <summary>
    /// Update scrap text display
    /// </summary>
    private void UpdateScrapDisplay()
    {
        if (scrapText != null)
        {
            scrapText.text = displayScrap.ToString();
        }
    }

    /// <summary>
    /// Update relics text display
    /// </summary>
    private void UpdateRelicsDisplay()
    {
        if (relicsText != null)
        {
            relicsText.text = displayRelics.ToString();
        }
    }

    /// <summary>
    /// Update visibility of resource displays
    /// </summary>
    private void UpdateVisibility()
    {
        SetResourceVisible("money", showMoney);
        SetResourceVisible("fuel", showFuel);
        SetResourceVisible("scrap", showScrap);
        SetResourceVisible("relics", showRelics);
    }

    #endregion

    #region Animations

    /// <summary>
    /// Animate money value change
    /// </summary>
    private IEnumerator AnimateMoney(float from, float to)
    {
        float elapsed = 0f;
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            displayMoney = Mathf.Lerp(from, to, elapsed / animationDuration);
            UpdateMoneyDisplay();
            yield return null;
        }
        displayMoney = to;
        UpdateMoneyDisplay();
    }

    /// <summary>
    /// Animate fuel value change
    /// </summary>
    private IEnumerator AnimateFuel(float from, float to)
    {
        float elapsed = 0f;
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            displayFuel = Mathf.Lerp(from, to, elapsed / animationDuration);
            UpdateFuelDisplay();
            yield return null;
        }
        displayFuel = to;
        UpdateFuelDisplay();
    }

    /// <summary>
    /// Animate scrap value change
    /// </summary>
    private IEnumerator AnimateScrap(int from, int to)
    {
        float elapsed = 0f;
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            displayScrap = Mathf.RoundToInt(Mathf.Lerp(from, to, elapsed / animationDuration));
            UpdateScrapDisplay();
            yield return null;
        }
        displayScrap = to;
        UpdateScrapDisplay();
    }

    /// <summary>
    /// Animate relics value change
    /// </summary>
    private IEnumerator AnimateRelics(int from, int to)
    {
        float elapsed = 0f;
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            displayRelics = Mathf.RoundToInt(Mathf.Lerp(from, to, elapsed / animationDuration));
            UpdateRelicsDisplay();
            yield return null;
        }
        displayRelics = to;
        UpdateRelicsDisplay();
    }

    /// <summary>
    /// Flash text color
    /// </summary>
    private IEnumerator FlashText(TextMeshProUGUI text, Color flashColor)
    {
        if (text == null)
            yield break;

        Color originalColor = text.color;
        text.color = flashColor;

        yield return new WaitForSeconds(flashDuration);

        text.color = originalColor;
    }

    #endregion

    #region Public Interface

    /// <summary>
    /// Get current money value
    /// </summary>
    public float GetMoney()
    {
        return currentMoney;
    }

    /// <summary>
    /// Get current fuel value
    /// </summary>
    public float GetFuel()
    {
        return currentFuel;
    }

    /// <summary>
    /// Get current scrap value
    /// </summary>
    public int GetScrap()
    {
        return currentScrap;
    }

    /// <summary>
    /// Get current relics value
    /// </summary>
    public int GetRelics()
    {
        return currentRelics;
    }

    /// <summary>
    /// Toggle visibility of specific resource display
    /// </summary>
    public void SetResourceVisible(string resourceName, bool visible)
    {
        switch (resourceName.ToLower())
        {
            case "money":
                showMoney = visible;
                if (moneyText != null) moneyText.gameObject.SetActive(visible);
                if (moneyIcon != null) moneyIcon.gameObject.SetActive(visible);
                break;

            case "fuel":
                showFuel = visible;
                if (fuelText != null) fuelText.gameObject.SetActive(visible);
                if (fuelIcon != null) fuelIcon.gameObject.SetActive(visible);
                if (fuelFillBar != null) fuelFillBar.gameObject.SetActive(visible);
                break;

            case "scrap":
                showScrap = visible;
                if (scrapText != null) scrapText.gameObject.SetActive(visible);
                if (scrapIcon != null) scrapIcon.gameObject.SetActive(visible);
                break;

            case "relics":
                showRelics = visible;
                if (relicsText != null) relicsText.gameObject.SetActive(visible);
                if (relicsIcon != null) relicsIcon.gameObject.SetActive(visible);
                break;
        }
    }

    #endregion

    #region Debug Methods

    /// <summary>
    /// Test money increase
    /// </summary>
    [ContextMenu("Test Add Money (+100)")]
    private void TestAddMoney()
    {
        SetMoney(currentMoney + 100f);
    }

    /// <summary>
    /// Test money decrease
    /// </summary>
    [ContextMenu("Test Spend Money (-50)")]
    private void TestSpendMoney()
    {
        SetMoney(currentMoney - 50f);
    }

    /// <summary>
    /// Test fuel decrease
    /// </summary>
    [ContextMenu("Test Use Fuel (-20%)")]
    private void TestUseFuel()
    {
        SetFuel(currentFuel - 20f);
    }

    /// <summary>
    /// Test low fuel warning
    /// </summary>
    [ContextMenu("Test Low Fuel")]
    private void TestLowFuel()
    {
        SetFuel(15f);
    }

    #endregion
}

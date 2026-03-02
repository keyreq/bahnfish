using UnityEngine;

/// <summary>
/// Agent 9: Progression & Economy Agent - EconomySystem.cs
/// Core economy management system for Bahnfish.
/// Manages three currency types: Money, Scrap, and Relics.
/// Integrates with SaveManager for automatic persistence.
/// </summary>
public class EconomySystem : MonoBehaviour
{
    // Singleton instance
    private static EconomySystem _instance;
    public static EconomySystem Instance => _instance;

    [Header("Currency Balances")]
    [SerializeField] private float _money = 100f;
    [SerializeField] private float _scrap = 0f;
    [SerializeField] private int _relics = 0;

    [Header("Debug")]
    [SerializeField] private bool _enableDebugLogs = true;
    [SerializeField] private bool _allowNegativeBalance = false;

    // Events
    public event System.Action<float, float> OnMoneyChanged; // oldValue, newValue
    public event System.Action<float, float> OnScrapChanged; // oldValue, newValue
    public event System.Action<int, int> OnRelicsChanged; // oldValue, newValue
    public event System.Action<string, float> OnTransactionCompleted; // transactionType, amount

    // Properties
    public float Money => _money;
    public float Scrap => _scrap;
    public int Relics => _relics;

    #region Unity Lifecycle

    private void Awake()
    {
        // Singleton pattern
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Subscribe to save/load events
        EventSystem.Subscribe<SaveData>("GatheringSaveData", OnGatheringSaveData);
        EventSystem.Subscribe<SaveData>("ApplyingSaveData", OnApplyingSaveData);

        // Subscribe to game events that affect economy
        EventSystem.Subscribe<Fish>("FishCaught", OnFishCaught);

        if (_enableDebugLogs)
        {
            Debug.Log($"[EconomySystem] Initialized with Money: ${_money:F2}, Scrap: {_scrap}, Relics: {_relics}");
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        EventSystem.Unsubscribe<SaveData>("GatheringSaveData", OnGatheringSaveData);
        EventSystem.Unsubscribe<SaveData>("ApplyingSaveData", OnApplyingSaveData);
        EventSystem.Unsubscribe<Fish>("FishCaught", OnFishCaught);
    }

    #endregion

    #region Money Management

    /// <summary>
    /// Gets the current money balance.
    /// </summary>
    public float GetMoney()
    {
        return _money;
    }

    /// <summary>
    /// Adds money to the player's balance.
    /// </summary>
    /// <param name="amount">Amount to add (must be positive)</param>
    /// <param name="reason">Optional reason for tracking</param>
    /// <returns>True if successful</returns>
    public bool AddMoney(float amount, string reason = "")
    {
        if (amount < 0)
        {
            Debug.LogWarning($"[EconomySystem] Cannot add negative money: {amount}");
            return false;
        }

        float oldValue = _money;
        _money += amount;

        if (_enableDebugLogs)
        {
            string reasonText = string.IsNullOrEmpty(reason) ? "" : $" (Reason: {reason})";
            Debug.Log($"[EconomySystem] Added ${amount:F2}{reasonText}. New balance: ${_money:F2}");
        }

        // Fire events
        OnMoneyChanged?.Invoke(oldValue, _money);
        EventSystem.Publish("MoneyChanged", new CurrencyChangeData(_money, oldValue, amount, reason));
        OnTransactionCompleted?.Invoke("MoneyAdded", amount);

        // Update GameState
        UpdateGameState();

        return true;
    }

    /// <summary>
    /// Spends money from the player's balance.
    /// </summary>
    /// <param name="amount">Amount to spend (must be positive)</param>
    /// <param name="reason">Reason for spending (e.g., "Fishing Rod Upgrade")</param>
    /// <returns>True if successful, false if insufficient funds</returns>
    public bool SpendMoney(float amount, string reason = "Unknown")
    {
        if (amount < 0)
        {
            Debug.LogWarning($"[EconomySystem] Cannot spend negative money: {amount}");
            return false;
        }

        if (!CanAffordMoney(amount))
        {
            if (_enableDebugLogs)
            {
                Debug.Log($"[EconomySystem] Insufficient funds for {reason}. Need: ${amount:F2}, Have: ${_money:F2}");
            }
            EventSystem.Publish("InsufficientFunds", new InsufficientFundsData("money", amount, _money, reason));
            return false;
        }

        float oldValue = _money;
        _money -= amount;

        if (_enableDebugLogs)
        {
            Debug.Log($"[EconomySystem] Spent ${amount:F2} on {reason}. New balance: ${_money:F2}");
        }

        // Fire events
        OnMoneyChanged?.Invoke(oldValue, _money);
        EventSystem.Publish("MoneyChanged", new CurrencyChangeData(_money, oldValue, -amount, reason));
        OnTransactionCompleted?.Invoke("MoneySpent", amount);

        // Update GameState
        UpdateGameState();

        return true;
    }

    /// <summary>
    /// Checks if the player can afford a specific amount of money.
    /// </summary>
    public bool CanAffordMoney(float amount)
    {
        if (_allowNegativeBalance) return true;
        return _money >= amount;
    }

    /// <summary>
    /// Sets money to a specific value (use sparingly, mainly for debugging).
    /// </summary>
    public void SetMoney(float amount, string reason = "Manual Set")
    {
        float oldValue = _money;
        _money = Mathf.Max(0, amount);

        if (_enableDebugLogs)
        {
            Debug.Log($"[EconomySystem] Money set to ${_money:F2}. Reason: {reason}");
        }

        OnMoneyChanged?.Invoke(oldValue, _money);
        EventSystem.Publish("MoneyChanged", new CurrencyChangeData(_money, oldValue, _money - oldValue, reason));
        UpdateGameState();
    }

    #endregion

    #region Scrap Management

    /// <summary>
    /// Gets the current scrap balance.
    /// </summary>
    public float GetScrap()
    {
        return _scrap;
    }

    /// <summary>
    /// Adds scrap to the player's balance.
    /// </summary>
    public bool AddScrap(float amount, string reason = "")
    {
        if (amount < 0)
        {
            Debug.LogWarning($"[EconomySystem] Cannot add negative scrap: {amount}");
            return false;
        }

        float oldValue = _scrap;
        _scrap += amount;

        if (_enableDebugLogs)
        {
            string reasonText = string.IsNullOrEmpty(reason) ? "" : $" (Reason: {reason})";
            Debug.Log($"[EconomySystem] Added {amount} scrap{reasonText}. New balance: {_scrap}");
        }

        OnScrapChanged?.Invoke(oldValue, _scrap);
        EventSystem.Publish("ScrapChanged", new CurrencyChangeData(_scrap, oldValue, amount, reason));
        OnTransactionCompleted?.Invoke("ScrapAdded", amount);
        UpdateGameState();

        return true;
    }

    /// <summary>
    /// Spends scrap from the player's balance.
    /// </summary>
    public bool SpendScrap(float amount, string reason = "Unknown")
    {
        if (amount < 0)
        {
            Debug.LogWarning($"[EconomySystem] Cannot spend negative scrap: {amount}");
            return false;
        }

        if (!CanAffordScrap(amount))
        {
            if (_enableDebugLogs)
            {
                Debug.Log($"[EconomySystem] Insufficient scrap for {reason}. Need: {amount}, Have: {_scrap}");
            }
            EventSystem.Publish("InsufficientFunds", new InsufficientFundsData("scrap", amount, _scrap, reason));
            return false;
        }

        float oldValue = _scrap;
        _scrap -= amount;

        if (_enableDebugLogs)
        {
            Debug.Log($"[EconomySystem] Spent {amount} scrap on {reason}. New balance: {_scrap}");
        }

        OnScrapChanged?.Invoke(oldValue, _scrap);
        EventSystem.Publish("ScrapChanged", new CurrencyChangeData(_scrap, oldValue, -amount, reason));
        OnTransactionCompleted?.Invoke("ScrapSpent", amount);
        UpdateGameState();

        return true;
    }

    /// <summary>
    /// Checks if the player can afford a specific amount of scrap.
    /// </summary>
    public bool CanAffordScrap(float amount)
    {
        if (_allowNegativeBalance) return true;
        return _scrap >= amount;
    }

    /// <summary>
    /// Sets scrap to a specific value.
    /// </summary>
    public void SetScrap(float amount, string reason = "Manual Set")
    {
        float oldValue = _scrap;
        _scrap = Mathf.Max(0, amount);

        if (_enableDebugLogs)
        {
            Debug.Log($"[EconomySystem] Scrap set to {_scrap}. Reason: {reason}");
        }

        OnScrapChanged?.Invoke(oldValue, _scrap);
        EventSystem.Publish("ScrapChanged", new CurrencyChangeData(_scrap, oldValue, _scrap - oldValue, reason));
        UpdateGameState();
    }

    #endregion

    #region Relics Management

    /// <summary>
    /// Gets the current relics count.
    /// </summary>
    public int GetRelics()
    {
        return _relics;
    }

    /// <summary>
    /// Adds relics to the player's balance.
    /// </summary>
    public bool AddRelics(int amount, string reason = "")
    {
        if (amount < 0)
        {
            Debug.LogWarning($"[EconomySystem] Cannot add negative relics: {amount}");
            return false;
        }

        int oldValue = _relics;
        _relics += amount;

        if (_enableDebugLogs)
        {
            string reasonText = string.IsNullOrEmpty(reason) ? "" : $" (Reason: {reason})";
            Debug.Log($"[EconomySystem] Added {amount} relic(s){reasonText}. New count: {_relics}");
        }

        OnRelicsChanged?.Invoke(oldValue, _relics);
        EventSystem.Publish("RelicsChanged", new RelicChangeData(_relics, oldValue, amount, reason));
        OnTransactionCompleted?.Invoke("RelicsAdded", amount);
        UpdateGameState();

        return true;
    }

    /// <summary>
    /// Spends relics from the player's balance.
    /// </summary>
    public bool SpendRelics(int amount, string reason = "Unknown")
    {
        if (amount < 0)
        {
            Debug.LogWarning($"[EconomySystem] Cannot spend negative relics: {amount}");
            return false;
        }

        if (!CanAffordRelics(amount))
        {
            if (_enableDebugLogs)
            {
                Debug.Log($"[EconomySystem] Insufficient relics for {reason}. Need: {amount}, Have: {_relics}");
            }
            EventSystem.Publish("InsufficientFunds", new InsufficientFundsData("relics", amount, _relics, reason));
            return false;
        }

        int oldValue = _relics;
        _relics -= amount;

        if (_enableDebugLogs)
        {
            Debug.Log($"[EconomySystem] Spent {amount} relic(s) on {reason}. New count: {_relics}");
        }

        OnRelicsChanged?.Invoke(oldValue, _relics);
        EventSystem.Publish("RelicsChanged", new RelicChangeData(_relics, oldValue, -amount, reason));
        OnTransactionCompleted?.Invoke("RelicsSpent", amount);
        UpdateGameState();

        return true;
    }

    /// <summary>
    /// Checks if the player can afford a specific amount of relics.
    /// </summary>
    public bool CanAffordRelics(int amount)
    {
        if (_allowNegativeBalance) return true;
        return _relics >= amount;
    }

    /// <summary>
    /// Sets relics to a specific value.
    /// </summary>
    public void SetRelics(int amount, string reason = "Manual Set")
    {
        int oldValue = _relics;
        _relics = Mathf.Max(0, amount);

        if (_enableDebugLogs)
        {
            Debug.Log($"[EconomySystem] Relics set to {_relics}. Reason: {reason}");
        }

        OnRelicsChanged?.Invoke(oldValue, _relics);
        EventSystem.Publish("RelicsChanged", new RelicChangeData(_relics, oldValue, _relics - oldValue, reason));
        UpdateGameState();
    }

    #endregion

    #region Save/Load Integration

    /// <summary>
    /// Saves economy data to SaveData.
    /// </summary>
    private void OnGatheringSaveData(SaveData data)
    {
        data.money = _money;
        data.scrap = _scrap;
        data.relics = _relics;

        if (_enableDebugLogs)
        {
            Debug.Log($"[EconomySystem] Saved economy data: ${_money:F2}, {_scrap} scrap, {_relics} relics");
        }
    }

    /// <summary>
    /// Loads economy data from SaveData.
    /// </summary>
    private void OnApplyingSaveData(SaveData data)
    {
        SetMoney(data.money, "Load Save");
        SetScrap(data.scrap, "Load Save");
        SetRelics(data.relics, "Load Save");

        if (_enableDebugLogs)
        {
            Debug.Log($"[EconomySystem] Loaded economy data: ${_money:F2}, {_scrap} scrap, {_relics} relics");
        }
    }

    #endregion

    #region GameState Integration

    /// <summary>
    /// Updates the central GameState with current currency values.
    /// </summary>
    private void UpdateGameState()
    {
        if (GameManager.Instance != null)
        {
            GameState state = GameManager.Instance.CurrentGameState;
            state.money = _money;
            // Note: scrap and relics would need to be added to GameState if needed for real-time access
            GameManager.Instance.UpdateGameState(state);
        }
    }

    #endregion

    #region Event Handlers

    /// <summary>
    /// Handles fish caught events (optional: auto-sell or tracking).
    /// </summary>
    private void OnFishCaught(Fish fish)
    {
        // This could be used for auto-sell in idle mode or tracking
        // For now, just log for potential future use
        if (_enableDebugLogs)
        {
            Debug.Log($"[EconomySystem] Fish caught: {fish.name} (Value: ${fish.baseValue:F2})");
        }
    }

    #endregion

    #region Debug Methods

    /// <summary>
    /// Prints current economy status to console.
    /// </summary>
    [ContextMenu("Print Economy Status")]
    public void PrintStatus()
    {
        Debug.Log($"=== Economy Status ===\n" +
                  $"Money: ${_money:F2}\n" +
                  $"Scrap: {_scrap}\n" +
                  $"Relics: {_relics}\n" +
                  $"=====================");
    }

    /// <summary>
    /// Adds test money (for debugging).
    /// </summary>
    [ContextMenu("Add $1000 (Debug)")]
    private void DebugAddMoney()
    {
        AddMoney(1000f, "Debug Test");
    }

    /// <summary>
    /// Adds test scrap (for debugging).
    /// </summary>
    [ContextMenu("Add 100 Scrap (Debug)")]
    private void DebugAddScrap()
    {
        AddScrap(100f, "Debug Test");
    }

    /// <summary>
    /// Adds test relics (for debugging).
    /// </summary>
    [ContextMenu("Add 10 Relics (Debug)")]
    private void DebugAddRelics()
    {
        AddRelics(10, "Debug Test");
    }

    /// <summary>
    /// Resets economy to starting values.
    /// </summary>
    [ContextMenu("Reset Economy")]
    private void DebugReset()
    {
        SetMoney(100f, "Debug Reset");
        SetScrap(0f, "Debug Reset");
        SetRelics(0, "Debug Reset");
        Debug.Log("[EconomySystem] Economy reset to starting values");
    }

    #endregion
}

#region Event Data Structures

/// <summary>
/// Data structure for currency change events.
/// </summary>
[System.Serializable]
public struct CurrencyChangeData
{
    public float newValue;
    public float oldValue;
    public float changeAmount;
    public string reason;

    public CurrencyChangeData(float newValue, float oldValue, float changeAmount, string reason)
    {
        this.newValue = newValue;
        this.oldValue = oldValue;
        this.changeAmount = changeAmount;
        this.reason = reason;
    }
}

/// <summary>
/// Data structure for relic change events.
/// </summary>
[System.Serializable]
public struct RelicChangeData
{
    public int newValue;
    public int oldValue;
    public int changeAmount;
    public string reason;

    public RelicChangeData(int newValue, int oldValue, int changeAmount, string reason)
    {
        this.newValue = newValue;
        this.oldValue = oldValue;
        this.changeAmount = changeAmount;
        this.reason = reason;
    }
}

/// <summary>
/// Data structure for insufficient funds events.
/// </summary>
[System.Serializable]
public struct InsufficientFundsData
{
    public string currencyType; // "money", "scrap", "relics"
    public float requiredAmount;
    public float currentAmount;
    public string attemptedPurchase;

    public InsufficientFundsData(string currencyType, float requiredAmount, float currentAmount, string attemptedPurchase)
    {
        this.currencyType = currencyType;
        this.requiredAmount = requiredAmount;
        this.currentAmount = currentAmount;
        this.attemptedPurchase = attemptedPurchase;
    }
}

#endregion

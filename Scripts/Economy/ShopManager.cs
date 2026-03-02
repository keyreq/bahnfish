using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Agent 9: Progression & Economy Agent - ShopManager.cs
/// Handles all marketplace and vendor interactions.
/// Sell fish, buy upgrades, purchase supplies, interact with NPCs.
/// Inspired by Cast n Chill's trading system.
/// </summary>
public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    [Header("Vendor Configuration")]
    [SerializeField] private List<VendorData> _vendors = new List<VendorData>();

    [Header("Shop Settings")]
    [SerializeField] private bool _enableDebugLogs = true;
    [SerializeField] private float _sellPriceMultiplier = 1.0f; // Adjust global sell prices
    [SerializeField] private float _buyPriceMultiplier = 1.0f; // Adjust global buy prices

    [Header("Transaction History")]
    [SerializeField] private int _maxTransactionHistory = 50;
    private List<Transaction> _transactionHistory = new List<Transaction>();

    // Events
    public event System.Action<Fish, float> OnFishSold;
    public event System.Action<string, float> OnItemPurchased;
    public event System.Action<List<Fish>, float> OnBulkSale;
    public event System.Action<Transaction> OnTransactionCompleted;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        InitializeVendors();

        if (_enableDebugLogs)
        {
            Debug.Log($"[ShopManager] Initialized with {_vendors.Count} vendors");
        }
    }

    #region Initialization

    /// <summary>
    /// Initializes default vendors.
    /// </summary>
    private void InitializeVendors()
    {
        if (_vendors.Count == 0)
        {
            // Create default vendors
            _vendors.Add(new VendorData
            {
                vendorID = "general_store",
                vendorName = "Marina General Store",
                vendorType = VendorType.GeneralStore,
                sellsFish = true,
                sellsUpgrades = true,
                sellsSupplies = true,
                priceMultiplier = 1.0f
            });

            _vendors.Add(new VendorData
            {
                vendorID = "fish_market",
                vendorName = "Fish Market",
                vendorType = VendorType.FishMarket,
                sellsFish = true,
                sellsUpgrades = false,
                sellsSupplies = false,
                priceMultiplier = 1.1f // Pays 10% more for fish
            });

            _vendors.Add(new VendorData
            {
                vendorID = "boat_mechanic",
                vendorName = "Boat Mechanic",
                vendorType = VendorType.Mechanic,
                sellsFish = false,
                sellsUpgrades = true,
                sellsSupplies = true,
                priceMultiplier = 1.0f
            });

            _vendors.Add(new VendorData
            {
                vendorID = "mystic_vendor",
                vendorName = "Mystic's Curio Shop",
                vendorType = VendorType.Mystic,
                sellsFish = false,
                sellsUpgrades = false,
                sellsSupplies = true,
                acceptsRelics = true,
                priceMultiplier = 1.5f // Expensive mystical items
            });
        }
    }

    #endregion

    #region Sell Fish

    /// <summary>
    /// Sells a single fish to the marketplace.
    /// </summary>
    /// <param name="fish">Fish to sell</param>
    /// <param name="isFresh">Is the fish fresh?</param>
    /// <param name="caughtAtNight">Was it caught at night?</param>
    /// <param name="vendorID">Optional vendor (uses best price if null)</param>
    /// <returns>Amount earned</returns>
    public float SellFish(Fish fish, bool isFresh = true, bool caughtAtNight = false, string vendorID = null)
    {
        if (fish == null)
        {
            Debug.LogWarning("[ShopManager] Cannot sell null fish");
            return 0f;
        }

        // Calculate sell price
        float basePrice = PricingSystem.Instance.GetFishSellValue(fish, isFresh, caughtAtNight);
        float finalPrice = ApplyVendorMultiplier(basePrice, vendorID);

        // Add money to player
        if (EconomySystem.Instance.AddMoney(finalPrice, $"Sold {fish.name}"))
        {
            // Record transaction
            RecordTransaction(new Transaction
            {
                transactionType = TransactionType.SellFish,
                itemName = fish.name,
                amount = finalPrice,
                vendorID = vendorID ?? "best_price",
                timestamp = System.DateTime.Now
            });

            // Track for market dynamics
            PricingSystem.Instance.RecordFishSale(fish, 1);

            // Fire events
            OnFishSold?.Invoke(fish, finalPrice);
            EventSystem.Publish("FishSold", new FishSoldData(fish, finalPrice, caughtAtNight));

            if (_enableDebugLogs)
            {
                Debug.Log($"[ShopManager] Sold {fish.name} for ${finalPrice:F2}");
            }

            return finalPrice;
        }

        return 0f;
    }

    /// <summary>
    /// Sells all fish from inventory in bulk.
    /// Uses Agent 6's inventory system to get all fish.
    /// </summary>
    /// <param name="caughtAtNight">Were these caught at night?</param>
    /// <returns>Total amount earned</returns>
    public float SellAllFish(bool caughtAtNight = false)
    {
        if (InventoryManager.Instance == null)
        {
            Debug.LogWarning("[ShopManager] InventoryManager not found");
            return 0f;
        }

        // Get all fish from inventory
        List<Fish> allFish = GetAllFishFromInventory();

        if (allFish.Count == 0)
        {
            if (_enableDebugLogs)
            {
                Debug.Log("[ShopManager] No fish to sell");
            }
            return 0f;
        }

        return SellBulkFish(allFish, caughtAtNight);
    }

    /// <summary>
    /// Sells a list of fish in bulk with bonuses.
    /// </summary>
    public float SellBulkFish(List<Fish> fishList, bool caughtAtNight = false, string vendorID = null)
    {
        if (fishList == null || fishList.Count == 0)
        {
            return 0f;
        }

        // Calculate bulk price with bonuses
        float bulkPrice = PricingSystem.Instance.CalculateBulkSellValue(fishList, caughtAtNight);
        float finalPrice = ApplyVendorMultiplier(bulkPrice, vendorID);

        // Add money
        if (EconomySystem.Instance.AddMoney(finalPrice, $"Sold {fishList.Count} fish (bulk)"))
        {
            // Record transaction
            RecordTransaction(new Transaction
            {
                transactionType = TransactionType.SellBulk,
                itemName = $"{fishList.Count} fish (bulk)",
                amount = finalPrice,
                quantity = fishList.Count,
                vendorID = vendorID ?? "bulk_sale",
                timestamp = System.DateTime.Now
            });

            // Track all fish sales
            foreach (var fish in fishList)
            {
                PricingSystem.Instance.RecordFishSale(fish, 1);
            }

            // Remove fish from inventory
            RemoveFishFromInventory(fishList);

            // Fire events
            OnBulkSale?.Invoke(fishList, finalPrice);
            EventSystem.Publish("BulkFishSold", new BulkSaleData(fishList.Count, finalPrice, caughtAtNight));

            if (_enableDebugLogs)
            {
                Debug.Log($"[ShopManager] Sold {fishList.Count} fish for ${finalPrice:F2} (${finalPrice / fishList.Count:F2} per fish avg)");
            }

            return finalPrice;
        }

        return 0f;
    }

    /// <summary>
    /// Gets total value of all fish in inventory (for UI display).
    /// </summary>
    public float GetTotalFishValue(bool caughtAtNight = false)
    {
        List<Fish> allFish = GetAllFishFromInventory();
        if (allFish.Count == 0) return 0f;

        return PricingSystem.Instance.CalculateBulkSellValue(allFish, caughtAtNight);
    }

    #endregion

    #region Buy Items

    /// <summary>
    /// Purchases bait from a vendor.
    /// </summary>
    public bool BuyBait(string baitType, int quantity, float pricePerUnit)
    {
        float totalCost = pricePerUnit * quantity;

        if (EconomySystem.Instance.SpendMoney(totalCost, $"Buy {quantity}x {baitType}"))
        {
            // TODO: Add bait to inventory when bait system is implemented
            RecordTransaction(new Transaction
            {
                transactionType = TransactionType.BuySupplies,
                itemName = baitType,
                amount = totalCost,
                quantity = quantity,
                timestamp = System.DateTime.Now
            });

            OnItemPurchased?.Invoke(baitType, totalCost);
            EventSystem.Publish("ItemPurchased", new ItemPurchasedData(baitType, quantity, totalCost));

            if (_enableDebugLogs)
            {
                Debug.Log($"[ShopManager] Bought {quantity}x {baitType} for ${totalCost:F2}");
            }

            return true;
        }

        return false;
    }

    /// <summary>
    /// Purchases fuel.
    /// </summary>
    public bool BuyFuel(float amount, float pricePerUnit = 1f)
    {
        float totalCost = amount * pricePerUnit;

        if (EconomySystem.Instance.SpendMoney(totalCost, $"Buy {amount} fuel"))
        {
            // Update fuel in GameState
            if (GameManager.Instance != null)
            {
                GameState state = GameManager.Instance.CurrentGameState;
                state.fuel = Mathf.Min(state.fuel + amount, 200f); // Max 200 fuel
                GameManager.Instance.UpdateGameState(state);
            }

            RecordTransaction(new Transaction
            {
                transactionType = TransactionType.BuySupplies,
                itemName = "Fuel",
                amount = totalCost,
                quantity = (int)amount,
                timestamp = System.DateTime.Now
            });

            if (_enableDebugLogs)
            {
                Debug.Log($"[ShopManager] Bought {amount} fuel for ${totalCost:F2}");
            }

            return true;
        }

        return false;
    }

    /// <summary>
    /// Purchases a talisman (sanity protection item).
    /// </summary>
    public bool BuyTalisman(string talismanType, float cost)
    {
        if (EconomySystem.Instance.SpendMoney(cost, $"Buy {talismanType}"))
        {
            // TODO: Add talisman to inventory
            RecordTransaction(new Transaction
            {
                transactionType = TransactionType.BuySupplies,
                itemName = talismanType,
                amount = cost,
                timestamp = System.DateTime.Now
            });

            if (_enableDebugLogs)
            {
                Debug.Log($"[ShopManager] Bought {talismanType} for ${cost:F2}");
            }

            return true;
        }

        return false;
    }

    #endregion

    #region Vendor System

    /// <summary>
    /// Gets a vendor by ID.
    /// </summary>
    public VendorData GetVendor(string vendorID)
    {
        return _vendors.FirstOrDefault(v => v.vendorID == vendorID);
    }

    /// <summary>
    /// Gets all vendors of a specific type.
    /// </summary>
    public List<VendorData> GetVendorsByType(VendorType type)
    {
        return _vendors.Where(v => v.vendorType == type).ToList();
    }

    /// <summary>
    /// Applies vendor-specific price multiplier.
    /// </summary>
    private float ApplyVendorMultiplier(float basePrice, string vendorID)
    {
        if (string.IsNullOrEmpty(vendorID))
        {
            // Find best price vendor for fish
            var bestVendor = _vendors.Where(v => v.sellsFish).OrderByDescending(v => v.priceMultiplier).FirstOrDefault();
            if (bestVendor != null)
            {
                return basePrice * bestVendor.priceMultiplier * _sellPriceMultiplier;
            }
            return basePrice * _sellPriceMultiplier;
        }

        var vendor = GetVendor(vendorID);
        if (vendor != null)
        {
            return basePrice * vendor.priceMultiplier * _sellPriceMultiplier;
        }

        return basePrice * _sellPriceMultiplier;
    }

    #endregion

    #region Inventory Integration

    /// <summary>
    /// Gets all fish from inventory manager (Agent 6 integration).
    /// </summary>
    private List<Fish> GetAllFishFromInventory()
    {
        List<Fish> fishList = new List<Fish>();

        if (InventoryManager.Instance == null) return fishList;

        // Get all items from inventory grid
        var allItems = InventoryManager.Instance.MainGrid.GetAllItems();

        foreach (var item in allItems)
        {
            // Check if item is a fish
            if (item is FishInventoryItem fishItem)
            {
                fishList.Add(fishItem.FishData);
            }
        }

        return fishList;
    }

    /// <summary>
    /// Removes sold fish from inventory.
    /// </summary>
    private void RemoveFishFromInventory(List<Fish> fishToRemove)
    {
        if (InventoryManager.Instance == null) return;

        foreach (var fish in fishToRemove)
        {
            // Find and remove fish items from inventory
            var allItems = InventoryManager.Instance.MainGrid.GetAllItems();
            var fishItem = allItems.FirstOrDefault(item =>
                item is FishInventoryItem fi && fi.FishData.id == fish.id);

            if (fishItem != null)
            {
                InventoryManager.Instance.RemoveItem(fishItem);
            }
        }
    }

    #endregion

    #region Transaction History

    /// <summary>
    /// Records a transaction in history.
    /// </summary>
    private void RecordTransaction(Transaction transaction)
    {
        _transactionHistory.Insert(0, transaction);

        // Keep history limited
        if (_transactionHistory.Count > _maxTransactionHistory)
        {
            _transactionHistory.RemoveAt(_transactionHistory.Count - 1);
        }

        OnTransactionCompleted?.Invoke(transaction);
    }

    /// <summary>
    /// Gets recent transaction history.
    /// </summary>
    public List<Transaction> GetTransactionHistory(int count = 10)
    {
        return _transactionHistory.Take(count).ToList();
    }

    /// <summary>
    /// Gets total money earned from selling fish.
    /// </summary>
    public float GetTotalFishSales()
    {
        return _transactionHistory
            .Where(t => t.transactionType == TransactionType.SellFish || t.transactionType == TransactionType.SellBulk)
            .Sum(t => t.amount);
    }

    /// <summary>
    /// Clears transaction history.
    /// </summary>
    public void ClearHistory()
    {
        _transactionHistory.Clear();
        if (_enableDebugLogs)
        {
            Debug.Log("[ShopManager] Transaction history cleared");
        }
    }

    #endregion

    #region Debug Methods

    [ContextMenu("Print Shop Status")]
    public void PrintShopStatus()
    {
        Debug.Log($"=== Shop Manager Status ===");
        Debug.Log($"Vendors: {_vendors.Count}");
        Debug.Log($"Recent Transactions: {_transactionHistory.Count}");
        Debug.Log($"Total Fish Sales: ${GetTotalFishSales():F2}");
        Debug.Log($"===========================");
    }

    [ContextMenu("Sell Test Fish")]
    private void DebugSellTestFish()
    {
        Fish testFish = new Fish
        {
            id = "test_fish",
            name = "Test Fish",
            baseValue = 50f,
            rarity = FishRarity.Common
        };

        float earned = SellFish(testFish, true, false);
        Debug.Log($"Sold test fish for ${earned:F2}");
    }

    #endregion
}

#region Data Structures

/// <summary>
/// Vendor data configuration.
/// </summary>
[System.Serializable]
public class VendorData
{
    public string vendorID;
    public string vendorName;
    public VendorType vendorType;
    public bool sellsFish = true;
    public bool sellsUpgrades = true;
    public bool sellsSupplies = true;
    public bool acceptsRelics = false;
    public float priceMultiplier = 1.0f;
    public string description;
    public Vector3 worldPosition;
}

/// <summary>
/// Vendor types.
/// </summary>
public enum VendorType
{
    GeneralStore,
    FishMarket,
    Mechanic,
    Mystic,
    BlackMarket
}

/// <summary>
/// Transaction types.
/// </summary>
public enum TransactionType
{
    SellFish,
    SellBulk,
    BuyUpgrade,
    BuySupplies,
    UnlockLocation,
    UnlockAbility
}

/// <summary>
/// Transaction record.
/// </summary>
[System.Serializable]
public class Transaction
{
    public TransactionType transactionType;
    public string itemName;
    public float amount;
    public int quantity = 1;
    public string vendorID;
    public System.DateTime timestamp;
}

/// <summary>
/// Fish sold event data.
/// </summary>
[System.Serializable]
public struct FishSoldData
{
    public Fish fish;
    public float amountEarned;
    public bool caughtAtNight;

    public FishSoldData(Fish fish, float amountEarned, bool caughtAtNight)
    {
        this.fish = fish;
        this.amountEarned = amountEarned;
        this.caughtAtNight = caughtAtNight;
    }
}

/// <summary>
/// Bulk sale event data.
/// </summary>
[System.Serializable]
public struct BulkSaleData
{
    public int fishCount;
    public float totalEarned;
    public bool caughtAtNight;

    public BulkSaleData(int fishCount, float totalEarned, bool caughtAtNight)
    {
        this.fishCount = fishCount;
        this.totalEarned = totalEarned;
        this.caughtAtNight = caughtAtNight;
    }
}

/// <summary>
/// Item purchased event data.
/// </summary>
[System.Serializable]
public struct ItemPurchasedData
{
    public string itemName;
    public int quantity;
    public float cost;

    public ItemPurchasedData(string itemName, int quantity, float cost)
    {
        this.itemName = itemName;
        this.quantity = quantity;
        this.cost = cost;
    }
}

#endregion

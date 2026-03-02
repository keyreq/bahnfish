using UnityEngine;

/// <summary>
/// Harpoon tool for instantly catching large or dangerous fish.
/// Uses aim-and-throw minigame instead of tension system.
/// Limited ammo, must be restocked.
/// </summary>
public class Harpoon : BaseFishingTool
{
    [Header("Harpoon Stats")]
    [SerializeField] private int currentAmmo = 10;
    [SerializeField] private int maxAmmo = 20;
    [SerializeField] private float damageMultiplier = 3f; // For large fish
    [SerializeField] private float accuracy = 1f; // Affects hit detection

    [Header("Ammo Costs")]
    [SerializeField] private float ammoPrice = 5f; // Cost per harpoon

    [Header("Restrictions")]
    [SerializeField] private float minimumFishWeight = 5f; // Only works on larger fish

    private void Awake()
    {
        toolName = "Harpoon";
        durability = 100f;
        power = 2.5f; // Strong tool
    }

    public override bool CanCatchFish(Fish fish)
    {
        // Harpoon only works on larger fish
        if (fish.weight < minimumFishWeight)
        {
            Debug.Log("Fish too small for harpoon!");
            return false;
        }

        // Need ammo
        if (currentAmmo <= 0)
        {
            Debug.Log("Out of harpoons!");
            return false;
        }

        // Especially effective on legendary and large aberrant fish
        return true;
    }

    #region Ammo Management

    public bool HasAmmo()
    {
        return currentAmmo > 0;
    }

    public void UseAmmo()
    {
        if (currentAmmo > 0)
        {
            currentAmmo--;
            Debug.Log($"Harpoon used. Remaining: {currentAmmo}/{maxAmmo}");

            if (currentAmmo == 0)
            {
                Debug.LogWarning("Out of harpoons! Restock at dock.");
                EventSystem.Publish("OnHarpoonDepleted", this);
            }
        }
    }

    public bool TryRestock(int amount, float playerMoney)
    {
        float cost = amount * ammoPrice;

        if (playerMoney < cost)
        {
            Debug.Log($"Not enough money to restock! Need ${cost}");
            return false;
        }

        int spaceAvailable = maxAmmo - currentAmmo;
        int amountToAdd = Mathf.Min(amount, spaceAvailable);

        if (amountToAdd <= 0)
        {
            Debug.Log("Harpoon ammo already full!");
            return false;
        }

        currentAmmo += amountToAdd;
        Debug.Log($"Restocked {amountToAdd} harpoons. Total: {currentAmmo}/{maxAmmo}");

        return true;
    }

    public void RestockFull(float playerMoney)
    {
        int needed = maxAmmo - currentAmmo;
        TryRestock(needed, playerMoney);
    }

    #endregion

    #region Tool Usage

    public void StartHarpoonThrow()
    {
        if (!HasAmmo())
        {
            Debug.LogWarning("No harpoons to throw!");
            return;
        }

        // Harpoon minigame will handle the actual throw
        // Just track usage
    }

    public void OnThrowComplete(bool hit)
    {
        UseAmmo();

        if (hit)
        {
            // Decrease durability less on successful hits
            durability -= 0.3f;
        }
        else
        {
            // Missed throws don't damage the harpoon as much
            durability -= 0.1f;
        }

        if (durability <= 0f)
        {
            OnToolBroken();
        }
    }

    public void Repair(float amount = 100f)
    {
        durability = Mathf.Min(durability + amount, 100f);
        Debug.Log($"Harpoon repaired to {durability:F1} durability");
    }

    private void OnToolBroken()
    {
        Debug.LogWarning("Harpoon broke! Need to repair.");
        EventSystem.Publish("OnToolBroken", this);

        // Can still throw but less accurate
        accuracy *= 0.5f;
        durability = 1f;
    }

    #endregion

    #region Upgrades

    public void UpgradeMaxAmmo(int increase)
    {
        maxAmmo += increase;
        Debug.Log($"Harpoon max ammo increased to {maxAmmo}");
    }

    public void UpgradeAccuracy(float increase)
    {
        accuracy += increase;
        accuracy = Mathf.Min(accuracy, 2f); // Cap at 2x
        Debug.Log($"Harpoon accuracy improved to {accuracy}x");
    }

    #endregion

    #region Public API

    public int GetCurrentAmmo() => currentAmmo;
    public int GetMaxAmmo() => maxAmmo;
    public float GetAccuracy() => accuracy;
    public float GetDamageMultiplier() => damageMultiplier;
    public float GetAmmoPrice() => ammoPrice;
    public float GetRestockCost() => (maxAmmo - currentAmmo) * ammoPrice;

    #endregion
}

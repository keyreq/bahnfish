using UnityEngine;
using System;

/// <summary>
/// Agent 16: Aquarium & Breeding Specialist - DisplayFish.cs
/// Represents a fish displayed in an aquarium tank.
/// Contains genetic traits, care status, and display properties.
/// </summary>
[Serializable]
public class DisplayFish
{
    // Identity
    public string uniqueID;
    public string speciesID;
    public string speciesName;
    public FishRarity rarity;

    // Genetics
    public GeneticTraits traits;

    // Status
    public float age; // In game days
    public float happiness; // 0 to 1
    public float health; // 0 to 1
    public bool isFed; // Fed today?
    public DateTime lastFedTime;
    public DateTime lastBreedingTime;

    // Tank assignment
    public string tankID;

    // Life cycle
    public DateTime birthDate;
    public bool isBred; // True if bred, false if wild-caught
    public bool isAlive;

    // Value
    public float currentValue;

    // Display properties
    public Vector3 position; // Position in tank
    public bool isSelected;

    public DisplayFish()
    {
        uniqueID = Guid.NewGuid().ToString();
        happiness = 0.7f;
        health = 1f;
        age = 0f;
        isAlive = true;
        isFed = false;
        lastFedTime = DateTime.MinValue;
        lastBreedingTime = DateTime.MinValue;
        birthDate = DateTime.Now;
        isBred = false;
        traits = GeneticsSystem.GenerateRandomTraits();
    }

    /// <summary>
    /// Creates a DisplayFish from wild-caught fish species.
    /// </summary>
    public static DisplayFish FromWildCaught(FishSpeciesData speciesData, Fish caughtFish)
    {
        DisplayFish displayFish = new DisplayFish
        {
            uniqueID = Guid.NewGuid().ToString(),
            speciesID = speciesData.fishID,
            speciesName = speciesData.fishName,
            rarity = caughtFish.rarity,
            traits = GeneticsSystem.GenerateRandomTraits(caughtFish.isAberrant),
            age = 0f,
            happiness = 0.7f,
            health = 1f,
            isFed = false,
            birthDate = DateTime.Now,
            isBred = false,
            isAlive = true,
            currentValue = caughtFish.baseValue
        };

        // Apply genetic modifiers to value
        displayFish.currentValue *= displayFish.traits.valueModifier;

        return displayFish;
    }

    /// <summary>
    /// Updates fish age and status (called daily).
    /// </summary>
    public void UpdateDaily(float environmentQuality)
    {
        if (!isAlive)
        {
            return;
        }

        // Age the fish
        age += 1f;

        // Update happiness based on care
        UpdateHappiness(environmentQuality);

        // Update health
        UpdateHealth();

        // Check for death (old age or poor health)
        CheckLifespan();

        // Reset daily feeding status
        if (DateTime.Now.Date > lastFedTime.Date)
        {
            isFed = false;
        }
    }

    /// <summary>
    /// Feeds the fish, improving happiness.
    /// </summary>
    public void Feed()
    {
        if (!isAlive)
        {
            return;
        }

        isFed = true;
        lastFedTime = DateTime.Now;
        happiness = Mathf.Min(happiness + 0.1f, 1f);
        health = Mathf.Min(health + 0.05f, 1f);
    }

    /// <summary>
    /// Updates fish happiness based on environment and care.
    /// </summary>
    private void UpdateHappiness(float environmentQuality)
    {
        // Environment quality affects happiness
        float environmentEffect = (environmentQuality - 0.5f) * 0.2f;

        // Being fed improves happiness
        float feedingEffect = isFed ? 0.05f : -0.1f;

        // Health affects happiness
        float healthEffect = (health - 0.5f) * 0.1f;

        happiness += environmentEffect + feedingEffect + healthEffect;
        happiness = Mathf.Clamp01(happiness);
    }

    /// <summary>
    /// Updates fish health based on care and age.
    /// </summary>
    private void UpdateHealth()
    {
        // Not being fed damages health
        if (!isFed)
        {
            health -= 0.05f;
        }

        // Old age slowly reduces health
        if (traits != null && age > traits.lifespanDays * 0.8f)
        {
            health -= 0.02f;
        }

        health = Mathf.Clamp01(health);
    }

    /// <summary>
    /// Checks if fish has died from old age or poor health.
    /// </summary>
    private void CheckLifespan()
    {
        if (traits == null)
        {
            return;
        }

        // Death from old age
        if (age >= traits.lifespanDays)
        {
            Die("Old Age");
            return;
        }

        // Death from poor health
        if (health <= 0f)
        {
            Die("Poor Health");
            return;
        }

        // Death from extreme unhappiness
        if (happiness <= 0.1f && UnityEngine.Random.value < 0.05f)
        {
            Die("Neglect");
        }
    }

    /// <summary>
    /// Marks the fish as dead.
    /// </summary>
    private void Die(string cause)
    {
        isAlive = false;
        health = 0f;

        Debug.Log($"[DisplayFish] {speciesName} (ID: {uniqueID}) died from {cause} at age {age} days");

        // Fire death event
        EventSystem.Publish("FishDied", new FishDeathData(this, cause));
    }

    /// <summary>
    /// Checks if the fish is mature enough to breed.
    /// </summary>
    public bool IsMature()
    {
        if (traits == null)
        {
            return age >= 3f; // Default 3 days
        }

        // Maturity based on growth rate
        float maturityAge = 3f / traits.growthRate; // Faster growth = earlier maturity
        return age >= maturityAge;
    }

    /// <summary>
    /// Gets days until mature.
    /// </summary>
    public float GetDaysUntilMature()
    {
        if (IsMature())
        {
            return 0f;
        }

        float maturityAge = traits != null ? 3f / traits.growthRate : 3f;
        return maturityAge - age;
    }

    /// <summary>
    /// Gets remaining lifespan in days.
    /// </summary>
    public float GetRemainingLifespan()
    {
        if (traits == null)
        {
            return 60f - age;
        }

        return Mathf.Max(0, traits.lifespanDays - age);
    }

    /// <summary>
    /// Calculates current sell value based on age, health, and traits.
    /// </summary>
    public float GetSellValue()
    {
        float value = currentValue;

        // Bred fish are worth more
        if (isBred)
        {
            value *= 2f; // 2x base value for bred fish
        }

        // Genetic value modifier
        if (traits != null)
        {
            value *= traits.valueModifier;
        }

        // Age penalties/bonuses
        if (IsMature())
        {
            value *= 1.2f; // Mature fish worth more
        }
        else
        {
            value *= 0.8f; // Juvenile fish worth less
        }

        // Health and happiness affect value
        value *= (health * 0.5f + 0.5f); // 50%-100% based on health
        value *= (happiness * 0.3f + 0.7f); // 70%-100% based on happiness

        // Rarity multiplier
        switch (rarity)
        {
            case FishRarity.Uncommon:
                value *= 2f;
                break;
            case FishRarity.Rare:
                value *= 5f;
                break;
            case FishRarity.Legendary:
                value *= 15f;
                break;
            case FishRarity.Aberrant:
                value *= 10f;
                break;
        }

        return Mathf.Max(1f, value);
    }

    /// <summary>
    /// Gets a formatted status string for UI display.
    /// </summary>
    public string GetStatusString()
    {
        if (!isAlive)
        {
            return "Deceased";
        }

        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        sb.AppendLine($"<b>{speciesName}</b>");
        sb.AppendLine($"Age: {age:F1} days" + (IsMature() ? " (Mature)" : $" (Mature in {GetDaysUntilMature():F1} days)"));
        sb.AppendLine($"Health: {health:P0}");
        sb.AppendLine($"Happiness: {happiness:P0}");
        sb.AppendLine($"Value: ${GetSellValue():F2}");
        sb.AppendLine($"Lifespan: {GetRemainingLifespan():F0} days remaining");

        if (isBred)
        {
            sb.AppendLine($"Generation: {traits.generation}");
        }

        if (!isFed)
        {
            sb.AppendLine("<color=red>Needs Feeding!</color>");
        }

        return sb.ToString();
    }
}

/// <summary>
/// Event data for fish death.
/// </summary>
[Serializable]
public struct FishDeathData
{
    public DisplayFish fish;
    public string cause;

    public FishDeathData(DisplayFish fish, string cause)
    {
        this.fish = fish;
        this.cause = cause;
    }
}

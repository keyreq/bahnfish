using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// Agent 16: Aquarium & Breeding Specialist - GeneticsSystem.cs
/// Handles fish genetic inheritance, mutations, and trait management.
/// Implements Mendelian genetics with dominant/recessive traits.
/// </summary>
public class GeneticsSystem
{
    private const float BASE_MUTATION_CHANCE = 0.01f; // 1% base mutation chance
    private const float MUTATION_CHANCE_PER_LAB_LEVEL = 0.005f; // +0.5% per lab level

    /// <summary>
    /// Generates offspring genetic traits from two parent fish.
    /// </summary>
    /// <param name="parent1">First parent's genetic traits</param>
    /// <param name="parent2">Second parent's genetic traits</param>
    /// <param name="geneticsLabLevel">Level of genetics lab (0-3, increases mutation chance)</param>
    /// <returns>Genetic traits for offspring</returns>
    public static GeneticTraits GenerateOffspring(GeneticTraits parent1, GeneticTraits parent2, int geneticsLabLevel = 0)
    {
        if (parent1 == null || parent2 == null)
        {
            Debug.LogError("[GeneticsSystem] Cannot generate offspring from null parents!");
            return GenerateRandomTraits();
        }

        GeneticTraits offspring = new GeneticTraits
        {
            uniqueID = System.Guid.NewGuid().ToString(),
            generation = Mathf.Max(parent1.generation, parent2.generation) + 1,
            parent1ID = parent1.uniqueID,
            parent2ID = parent2.uniqueID
        };

        // Calculate mutation chance
        float mutationChance = BASE_MUTATION_CHANCE + (geneticsLabLevel * MUTATION_CHANCE_PER_LAB_LEVEL);

        // Size: Average of parents ± variance
        offspring.sizeMultiplier = InheritSize(parent1.sizeMultiplier, parent2.sizeMultiplier, mutationChance);

        // Color: Mendelian inheritance with mutation
        offspring.colorVariant = InheritColor(parent1.colorVariant, parent2.colorVariant, mutationChance);

        // Pattern: Dominant/recessive inheritance
        offspring.patternType = InheritPattern(parent1.patternType, parent2.patternType, mutationChance);

        // Rarity chance: Additive with potential upgrade
        offspring.rarityBonus = InheritRarityBonus(parent1.rarityBonus, parent2.rarityBonus, mutationChance);

        // Value modifier: Average with variance
        offspring.valueModifier = InheritValueModifier(parent1.valueModifier, parent2.valueModifier, mutationChance);

        // Aggression: Average of parents
        offspring.aggression = InheritAggression(parent1.aggression, parent2.aggression, mutationChance);

        // Growth rate: Faster of two parents (dominant trait)
        offspring.growthRate = InheritGrowthRate(parent1.growthRate, parent2.growthRate, mutationChance);

        // Lifespan: Average of parents
        offspring.lifespanDays = InheritLifespan(parent1.lifespanDays, parent2.lifespanDays, mutationChance);

        // Bioluminescence: Recessive trait (both parents must have it)
        offspring.hasBioluminescence = InheritBioluminescence(parent1.hasBioluminescence, parent2.hasBioluminescence, mutationChance);

        // Mutation chance: Average with slight improvement
        offspring.mutationChance = InheritMutationChance(parent1.mutationChance, parent2.mutationChance, mutationChance);

        // Check for aberrant breeding
        if (parent1.isAberrant && parent2.isAberrant)
        {
            // 80% chance offspring is aberrant
            offspring.isAberrant = UnityEngine.Random.value < 0.8f;
        }
        else if (parent1.isAberrant || parent2.isAberrant)
        {
            // 10% chance if only one parent is aberrant
            offspring.isAberrant = UnityEngine.Random.value < 0.1f;
        }

        // Record breeding timestamp
        offspring.birthTimestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        return offspring;
    }

    /// <summary>
    /// Generates random genetic traits for wild-caught fish.
    /// </summary>
    public static GeneticTraits GenerateRandomTraits(bool isAberrant = false)
    {
        GeneticTraits traits = new GeneticTraits
        {
            uniqueID = System.Guid.NewGuid().ToString(),
            generation = 0,
            sizeMultiplier = UnityEngine.Random.Range(0.8f, 1.2f),
            colorVariant = (FishColor)UnityEngine.Random.Range(0, Enum.GetValues(typeof(FishColor)).Length),
            patternType = (PatternType)UnityEngine.Random.Range(0, Enum.GetValues(typeof(PatternType)).Length),
            rarityBonus = UnityEngine.Random.Range(0f, 0.05f),
            valueModifier = UnityEngine.Random.Range(0.7f, 1.3f),
            aggression = UnityEngine.Random.Range(0.3f, 0.8f),
            growthRate = UnityEngine.Random.Range(0.8f, 1.2f),
            lifespanDays = UnityEngine.Random.Range(30, 90),
            hasBioluminescence = UnityEngine.Random.value < 0.05f,
            mutationChance = BASE_MUTATION_CHANCE,
            isAberrant = isAberrant,
            birthTimestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        };

        return traits;
    }

    #region Inheritance Methods

    private static float InheritSize(float p1Size, float p2Size, float mutationChance)
    {
        // Average of parents with random variance
        float baseSize = (p1Size + p2Size) / 2f;
        float variance = UnityEngine.Random.Range(-0.1f, 0.1f);

        // Mutation: larger variance
        if (UnityEngine.Random.value < mutationChance)
        {
            variance += UnityEngine.Random.Range(-0.3f, 0.3f);
        }

        return Mathf.Clamp(baseSize + variance, 0.5f, 1.5f);
    }

    private static FishColor InheritColor(FishColor p1Color, FishColor p2Color, float mutationChance)
    {
        // 75% chance of parent color, 20% recessive, 5% mutation
        float roll = UnityEngine.Random.value;

        // Mutation: random color
        if (roll < mutationChance * 5f) // 5% at base mutation rate
        {
            return (FishColor)UnityEngine.Random.Range(0, Enum.GetValues(typeof(FishColor)).Length);
        }

        // 75% chance of inheriting from either parent
        if (roll < 0.75f)
        {
            return UnityEngine.Random.value < 0.5f ? p1Color : p2Color;
        }

        // 20% chance of recessive color (one step away from parents)
        int colorCount = Enum.GetValues(typeof(FishColor)).Length;
        int p1Index = (int)p1Color;
        int recessiveIndex = (p1Index + UnityEngine.Random.Range(1, 3)) % colorCount;
        return (FishColor)recessiveIndex;
    }

    private static PatternType InheritPattern(PatternType p1Pattern, PatternType p2Pattern, float mutationChance)
    {
        // Spots and Stripes are dominant over Solid
        if (UnityEngine.Random.value < mutationChance)
        {
            return (PatternType)UnityEngine.Random.Range(0, Enum.GetValues(typeof(PatternType)).Length);
        }

        // If either parent has spots/stripes, 75% chance to inherit
        if ((p1Pattern != PatternType.Solid || p2Pattern != PatternType.Solid) && UnityEngine.Random.value < 0.75f)
        {
            return p1Pattern != PatternType.Solid ? p1Pattern : p2Pattern;
        }

        // Otherwise inherit randomly
        return UnityEngine.Random.value < 0.5f ? p1Pattern : p2Pattern;
    }

    private static float InheritRarityBonus(float p1Bonus, float p2Bonus, float mutationChance)
    {
        // Average of parents with potential improvement
        float baseBonus = (p1Bonus + p2Bonus) / 2f;

        // 10% chance to improve rarity bonus
        if (UnityEngine.Random.value < 0.1f || UnityEngine.Random.value < mutationChance)
        {
            baseBonus += UnityEngine.Random.Range(0.01f, 0.05f);
        }

        return Mathf.Clamp(baseBonus, 0f, 0.15f);
    }

    private static float InheritValueModifier(float p1Value, float p2Value, float mutationChance)
    {
        float baseValue = (p1Value + p2Value) / 2f;
        float variance = UnityEngine.Random.Range(-0.1f, 0.1f);

        if (UnityEngine.Random.value < mutationChance)
        {
            variance += UnityEngine.Random.Range(-0.2f, 0.3f); // Mutations favor positive value
        }

        return Mathf.Clamp(baseValue + variance, 0.7f, 1.6f);
    }

    private static float InheritAggression(float p1Aggression, float p2Aggression, float mutationChance)
    {
        float baseAggression = (p1Aggression + p2Aggression) / 2f;
        float variance = UnityEngine.Random.Range(-0.1f, 0.1f);

        if (UnityEngine.Random.value < mutationChance)
        {
            variance += UnityEngine.Random.Range(-0.2f, 0.2f);
        }

        return Mathf.Clamp(baseAggression + variance, 0f, 1f);
    }

    private static float InheritGrowthRate(float p1Rate, float p2Rate, float mutationChance)
    {
        // Faster growth is dominant
        float baseRate = Mathf.Max(p1Rate, p2Rate);

        if (UnityEngine.Random.value < mutationChance)
        {
            baseRate += UnityEngine.Random.Range(-0.2f, 0.3f); // Favor faster growth
        }

        return Mathf.Clamp(baseRate, 0.5f, 2f);
    }

    private static int InheritLifespan(int p1Lifespan, int p2Lifespan, float mutationChance)
    {
        int baseLifespan = (p1Lifespan + p2Lifespan) / 2;
        int variance = UnityEngine.Random.Range(-10, 10);

        if (UnityEngine.Random.value < mutationChance)
        {
            variance += UnityEngine.Random.Range(-20, 30); // Favor longer lifespan
        }

        return Mathf.Clamp(baseLifespan + variance, 20, 180);
    }

    private static bool InheritBioluminescence(bool p1Biolum, bool p2Biolum, float mutationChance)
    {
        // Recessive trait - both parents must have it for high chance
        if (p1Biolum && p2Biolum)
        {
            return UnityEngine.Random.value < 0.9f; // 90% chance if both have it
        }
        else if (p1Biolum || p2Biolum)
        {
            return UnityEngine.Random.value < 0.25f; // 25% chance if one has it
        }
        else
        {
            // Mutation chance
            return UnityEngine.Random.value < mutationChance;
        }
    }

    private static float InheritMutationChance(float p1Chance, float p2Chance, float mutationChance)
    {
        float baseChance = (p1Chance + p2Chance) / 2f;

        // Slight improvement with each generation
        baseChance += UnityEngine.Random.Range(0f, 0.005f);

        return Mathf.Clamp(baseChance, BASE_MUTATION_CHANCE, 0.05f);
    }

    #endregion

    /// <summary>
    /// Calculates compatibility score between two fish for breeding.
    /// </summary>
    /// <returns>Compatibility score 0-1 (higher is better)</returns>
    public static float CalculateCompatibility(GeneticTraits parent1, GeneticTraits parent2)
    {
        if (parent1 == null || parent2 == null)
        {
            return 0f;
        }

        float compatibility = 1f;

        // Similar size is better
        float sizeDiff = Mathf.Abs(parent1.sizeMultiplier - parent2.sizeMultiplier);
        compatibility -= sizeDiff * 0.1f;

        // Too high aggression in both reduces compatibility
        if (parent1.aggression > 0.7f && parent2.aggression > 0.7f)
        {
            compatibility -= 0.2f;
        }

        // Same generation improves compatibility
        if (parent1.generation == parent2.generation)
        {
            compatibility += 0.1f;
        }

        // Prevent inbreeding
        if (parent1.parent1ID == parent2.parent1ID || parent1.parent2ID == parent2.parent2ID)
        {
            compatibility -= 0.5f;
        }

        // Direct parent-child breeding is heavily penalized
        if (parent1.uniqueID == parent2.parent1ID || parent1.uniqueID == parent2.parent2ID ||
            parent2.uniqueID == parent1.parent1ID || parent2.uniqueID == parent1.parent2ID)
        {
            compatibility -= 0.8f;
        }

        return Mathf.Clamp01(compatibility);
    }

    /// <summary>
    /// Gets a human-readable description of genetic traits.
    /// </summary>
    public static string GetTraitDescription(GeneticTraits traits)
    {
        if (traits == null)
        {
            return "No genetic data available.";
        }

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine($"<b>Generation:</b> {traits.generation}");
        sb.AppendLine($"<b>Size:</b> {traits.sizeMultiplier:P0} of normal");
        sb.AppendLine($"<b>Color:</b> {traits.colorVariant}");
        sb.AppendLine($"<b>Pattern:</b> {traits.patternType}");
        sb.AppendLine($"<b>Value:</b> {traits.valueModifier:P0} of base");
        sb.AppendLine($"<b>Aggression:</b> {traits.aggression:P0}");
        sb.AppendLine($"<b>Growth Rate:</b> {traits.growthRate:P0}");
        sb.AppendLine($"<b>Lifespan:</b> {traits.lifespanDays} days");

        if (traits.hasBioluminescence)
        {
            sb.AppendLine("<b>Special:</b> Bioluminescent");
        }

        if (traits.isAberrant)
        {
            sb.AppendLine("<b>Type:</b> ABERRANT");
        }

        if (traits.generation > 0)
        {
            sb.AppendLine($"<b>Born:</b> {traits.birthTimestamp}");
        }

        return sb.ToString();
    }
}

/// <summary>
/// Represents the genetic traits of a fish.
/// </summary>
[Serializable]
public class GeneticTraits
{
    // Identity
    public string uniqueID;
    public int generation; // 0 = wild-caught, 1+ = bred
    public string parent1ID;
    public string parent2ID;
    public string birthTimestamp;

    // Trait 1: Size variation (±50% from base)
    public float sizeMultiplier = 1f; // 0.5 to 1.5

    // Trait 2: Color mutations (8 color variants)
    public FishColor colorVariant = FishColor.Natural;

    // Trait 3: Pattern changes
    public PatternType patternType = PatternType.Solid;

    // Trait 4: Rarity chance bonus
    public float rarityBonus = 0f; // 0% to 15%

    // Trait 5: Value modifier
    public float valueModifier = 1f; // 0.7x to 1.6x sell price

    // Trait 6: Aggression level
    public float aggression = 0.5f; // 0 to 1

    // Trait 7: Growth rate
    public float growthRate = 1f; // 0.5x to 2x time to maturity

    // Trait 8: Lifespan
    public int lifespanDays = 60; // 20 to 180 in-game days

    // Trait 9: Bioluminescence
    public bool hasBioluminescence = false;

    // Trait 10: Mutation chance
    public float mutationChance = 0.01f; // 1% to 5%

    // Special properties
    public bool isAberrant = false;

    public GeneticTraits()
    {
        uniqueID = System.Guid.NewGuid().ToString();
        birthTimestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }
}

/// <summary>
/// Available color variants for fish.
/// </summary>
[Serializable]
public enum FishColor
{
    Natural,    // Original species color
    Golden,     // Gold/yellow tint
    Albino,     // White/pale
    Melanistic, // Black/dark
    Blue,       // Blue tint
    Red,        // Red tint
    Purple,     // Purple/violet
    Rainbow     // Multi-colored (rare)
}

/// <summary>
/// Pattern types for fish appearance.
/// </summary>
[Serializable]
public enum PatternType
{
    Solid,      // No pattern
    Spots,      // Spotted pattern
    Stripes     // Striped pattern
}

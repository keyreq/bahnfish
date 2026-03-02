using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Agent 14: Location & World Generation Agent - All13Locations.cs
/// Centralized definition of all 13 fishing locations with their complete configurations.
/// This serves as both documentation and reference for the location system.
/// </summary>
public static class All13Locations
{
    /// <summary>
    /// Gets the complete list of all location IDs in progression order.
    /// </summary>
    public static readonly List<string> LocationIDs = new List<string>
    {
        "calm_lake",              // 1. FREE - Starter
        "rocky_coastline",        // 2. $500 - Easy
        "tidal_pools",            // 3. $1500 - Easy
        "deep_ocean_trenches",    // 4. $1500 - Hard
        "fog_shrouded_swamp",     // 5. $2000 - Medium (High at night)
        "mangrove_forest",        // 6. $2000 - Medium
        "coral_reef",             // 7. $2500 - Medium
        "arctic_waters",          // 8. $3000 - Hard
        "shipwreck_graveyard",    // 9. $3500 - Hard
        "underground_cavern",     // 10. $4000 - Extreme
        "bioluminescent_bay",     // 11. $4500 - Medium (inverted risk)
        "volcanic_vent",          // 12. $5000 - Extreme
        "abyssal_trench"          // 13. $10,000 - ENDGAME
    };

    /// <summary>
    /// Location progression path for recommended order.
    /// </summary>
    public static readonly Dictionary<string, LocationProgression> ProgressionPath = new Dictionary<string, LocationProgression>
    {
        { "calm_lake", new LocationProgression { tier = 0, recommendedLevel = 1, unlockOrder = 1 } },
        { "rocky_coastline", new LocationProgression { tier = 1, recommendedLevel = 3, unlockOrder = 2 } },
        { "tidal_pools", new LocationProgression { tier = 1, recommendedLevel = 3, unlockOrder = 3 } },
        { "deep_ocean_trenches", new LocationProgression { tier = 2, recommendedLevel = 5, unlockOrder = 4 } },
        { "fog_shrouded_swamp", new LocationProgression { tier = 2, recommendedLevel = 6, unlockOrder = 5 } },
        { "mangrove_forest", new LocationProgression { tier = 2, recommendedLevel = 6, unlockOrder = 6 } },
        { "coral_reef", new LocationProgression { tier = 3, recommendedLevel = 8, unlockOrder = 7 } },
        { "arctic_waters", new LocationProgression { tier = 3, recommendedLevel = 10, unlockOrder = 8 } },
        { "shipwreck_graveyard", new LocationProgression { tier = 3, recommendedLevel = 12, unlockOrder = 9 } },
        { "underground_cavern", new LocationProgression { tier = 4, recommendedLevel = 15, unlockOrder = 10 } },
        { "bioluminescent_bay", new LocationProgression { tier = 4, recommendedLevel = 16, unlockOrder = 11 } },
        { "volcanic_vent", new LocationProgression { tier = 5, recommendedLevel = 20, unlockOrder = 12 } },
        { "abyssal_trench", new LocationProgression { tier = 6, recommendedLevel = 30, unlockOrder = 13 } }
    };

    /// <summary>
    /// Story-significant locations that contain major plot revelations.
    /// </summary>
    public static readonly List<string> StoryLocations = new List<string>
    {
        "fog_shrouded_swamp",      // Ancient altar, Mystic NPC
        "underground_cavern",       // Entity's prison, story climax
        "bioluminescent_bay",       // Tidal Gate altar, fast travel unlock
        "abyssal_trench"            // Entity's awakening, true ending
    };

    /// <summary>
    /// Secret areas within each location.
    /// </summary>
    public static readonly Dictionary<string, List<SecretAreaDefinition>> SecretAreas = new Dictionary<string, List<SecretAreaDefinition>>
    {
        {
            "calm_lake",
            new List<SecretAreaDefinition>
            {
                new SecretAreaDefinition
                {
                    secretID = "calm_lake_hidden_cove",
                    secretName = "Hidden Cove",
                    description = "A secluded cove with rare trout",
                    position = new Vector3(150, -5, 150),
                    radius = 20f,
                    requiresElditchEye = false,
                    unlockHint = "Follow the lilypads north"
                }
            }
        },
        {
            "rocky_coastline",
            new List<SecretAreaDefinition>
            {
                new SecretAreaDefinition
                {
                    secretID = "tide_pools_secret",
                    secretName = "Ancient Tide Pools",
                    description = "Tide pools with unique shellfish",
                    position = new Vector3(700, -2, 100),
                    radius = 15f,
                    requiresElditchEye = false,
                    unlockHint = "Search the rocks at low tide"
                }
            }
        },
        {
            "deep_ocean_trenches",
            new List<SecretAreaDefinition>
            {
                new SecretAreaDefinition
                {
                    secretID = "underwater_canyon",
                    secretName = "Underwater Canyon",
                    description = "A massive canyon with legendary fish",
                    position = new Vector3(2200, -150, 0),
                    radius = 30f,
                    requiresElditchEye = false,
                    unlockHint = "Dive to extreme depths"
                }
            }
        },
        {
            "fog_shrouded_swamp",
            new List<SecretAreaDefinition>
            {
                new SecretAreaDefinition
                {
                    secretID = "ancient_altar_swamp",
                    secretName = "Ancient Altar",
                    description = "A mysterious altar in the swamp's center",
                    position = new Vector3(0, -5, 900),
                    radius = 25f,
                    requiresElditchEye = true,
                    unlockHint = "The fog conceals something powerful..."
                }
            }
        },
        {
            "arctic_waters",
            new List<SecretAreaDefinition>
            {
                new SecretAreaDefinition
                {
                    secretID = "frozen_shipwreck",
                    secretName = "Frozen Shipwreck",
                    description = "A ship frozen in ice with ancient relics",
                    position = new Vector3(-1600, -20, 0),
                    radius = 20f,
                    requiresElditchEye = false,
                    unlockHint = "Navigate between the icebergs"
                }
            }
        },
        {
            "coral_reef",
            new List<SecretAreaDefinition>
            {
                new SecretAreaDefinition
                {
                    secretID = "coral_cave_system",
                    secretName = "Coral Cave System",
                    description = "Hidden caves where rare fish breed",
                    position = new Vector3(1100, -30, 1100),
                    radius = 25f,
                    requiresElditchEye = false,
                    unlockHint = "Look for the largest coral formation"
                }
            }
        },
        {
            "underground_cavern",
            new List<SecretAreaDefinition>
            {
                new SecretAreaDefinition
                {
                    secretID = "ancient_altar_cavern",
                    secretName = "Cavern Altar",
                    description = "Second altar of the ancient ones",
                    position = new Vector3(-500, -120, -500),
                    radius = 20f,
                    requiresElditchEye = true,
                    unlockHint = "Where light has never touched"
                },
                new SecretAreaDefinition
                {
                    secretID = "entity_prison",
                    secretName = "The Entity's Prison",
                    description = "Where the Entity was first bound",
                    position = new Vector3(-600, -140, -600),
                    radius = 30f,
                    requiresElditchEye = true,
                    unlockHint = "The deepest darkness holds the truth"
                }
            }
        },
        {
            "shipwreck_graveyard",
            new List<SecretAreaDefinition>
            {
                new SecretAreaDefinition
                {
                    secretID = "captains_treasure",
                    secretName = "Captain's Treasure",
                    description = "Legendary captain's chest with 20 relics",
                    position = new Vector3(1600, -30, -800),
                    radius = 15f,
                    requiresElditchEye = false,
                    unlockHint = "Find the flagship wreck"
                }
            }
        },
        {
            "volcanic_vent",
            new List<SecretAreaDefinition>
            {
                new SecretAreaDefinition
                {
                    secretID = "leviathan_lair",
                    secretName = "Leviathan's Lair",
                    description = "Home of the Ancient Leviathan boss",
                    position = new Vector3(-1000, -80, 1500),
                    radius = 40f,
                    requiresElditchEye = false,
                    unlockHint = "The largest vent hides the greatest terror"
                }
            }
        },
        {
            "bioluminescent_bay",
            new List<SecretAreaDefinition>
            {
                new SecretAreaDefinition
                {
                    secretID = "tidal_gate_altar",
                    secretName = "Tidal Gate Altar",
                    description = "Unlocks fast travel ability",
                    position = new Vector3(500, -15, -1200),
                    radius = 20f,
                    requiresElditchEye = true,
                    unlockHint = "Where the lights dance brightest"
                }
            }
        },
        {
            "tidal_pools",
            new List<SecretAreaDefinition>
            {
                new SecretAreaDefinition
                {
                    secretID = "pearl_oyster_bed",
                    secretName = "Pearl Oyster Bed",
                    description = "Rare pearl oysters spawn during full moon",
                    position = new Vector3(-800, -5, -800),
                    radius = 15f,
                    requiresElditchEye = false,
                    unlockHint = "Wait for the full moon"
                }
            }
        },
        {
            "mangrove_forest",
            new List<SecretAreaDefinition>
            {
                new SecretAreaDefinition
                {
                    secretID = "ancient_altar_mangrove",
                    secretName = "Mangrove Altar",
                    description = "Third altar hidden in deepest grove",
                    position = new Vector3(900, -10, -1500),
                    radius = 20f,
                    requiresElditchEye = true,
                    unlockHint = "Follow the twisted roots to their source"
                }
            }
        },
        {
            "abyssal_trench",
            new List<SecretAreaDefinition>
            {
                new SecretAreaDefinition
                {
                    secretID = "entity_awakening_site",
                    secretName = "The Entity's Awakening",
                    description = "Where the Entity will emerge",
                    position = new Vector3(-2000, -300, -2000),
                    radius = 50f,
                    requiresElditchEye = true,
                    unlockHint = "The abyss itself calls to you"
                }
            }
        }
    };

    /// <summary>
    /// NPC assignments for each location.
    /// </summary>
    public static readonly Dictionary<string, List<string>> LocationNPCs = new Dictionary<string, List<string>>
    {
        { "calm_lake", new List<string> { "old_fisher" } },
        { "rocky_coastline", new List<string> { "lighthouse_keeper" } },
        { "fog_shrouded_swamp", new List<string> { "mystic" } },
        { "arctic_waters", new List<string> { "captain" } },
        { "coral_reef", new List<string> { "scientist" } },
        { "underground_cavern", new List<string> { "hermit" } },
        { "shipwreck_graveyard", new List<string> { "drunk_sailor" } },
        { "bioluminescent_bay", new List<string> { "child" } },
        { "tidal_pools", new List<string> { "shopkeeper" } }
    };

    /// <summary>
    /// Special mechanics description for each location.
    /// </summary>
    public static readonly Dictionary<string, string> SpecialMechanics = new Dictionary<string, string>
    {
        { "calm_lake", "Tutorial zone - Safe at all times, no night hazards" },
        { "rocky_coastline", "Crab pot hotspot - 150% catch rate for crab pots" },
        { "deep_ocean_trenches", "Extreme depth (200m+) - Harpoon required for large fish" },
        { "fog_shrouded_swamp", "HIGHEST aberrant spawn rate (50% at night) - Ghost Ship spawns frequently" },
        { "arctic_waters", "Cold damage - Sanity drains 1.5x faster, icebergs as obstacles" },
        { "coral_reef", "HIGHEST fish diversity (25 species) - Perfect split-screen underwater view" },
        { "underground_cavern", "PURE MYSTERY/HORROR - Requires upgraded lights, story climax location" },
        { "shipwreck_graveyard", "BEST dredging location - High scrap yield, frequent Ghost Ship" },
        { "volcanic_vent", "Heat damage zones - HIGHEST legendary spawn rate (10%), vortex spawns" },
        { "bioluminescent_bay", "Most beautiful at night - INVERTED RISK (safer at night), no fish thieves" },
        { "tidal_pools", "Crab pot catch rate +200% - Safe shallow water, perfect for passive fishing" },
        { "mangrove_forest", "Navigation challenge - Maze-like roots, unique 'tree-dweller' fish" },
        { "abyssal_trench", "FINAL LOCATION - Extreme depth (500m+), requires all upgrades, story conclusion" }
    };

    /// <summary>
    /// Gets location progression data.
    /// </summary>
    public static LocationProgression GetProgression(string locationID)
    {
        if (ProgressionPath.TryGetValue(locationID, out LocationProgression progression))
        {
            return progression;
        }
        return new LocationProgression { tier = 0, recommendedLevel = 1, unlockOrder = 0 };
    }

    /// <summary>
    /// Checks if a location is story-significant.
    /// </summary>
    public static bool IsStoryLocation(string locationID)
    {
        return StoryLocations.Contains(locationID);
    }

    /// <summary>
    /// Gets secrets for a location.
    /// </summary>
    public static List<SecretAreaDefinition> GetSecrets(string locationID)
    {
        if (SecretAreas.TryGetValue(locationID, out List<SecretAreaDefinition> secrets))
        {
            return secrets;
        }
        return new List<SecretAreaDefinition>();
    }

    /// <summary>
    /// Gets NPCs for a location.
    /// </summary>
    public static List<string> GetNPCs(string locationID)
    {
        if (LocationNPCs.TryGetValue(locationID, out List<string> npcs))
        {
            return npcs;
        }
        return new List<string>();
    }

    /// <summary>
    /// Gets special mechanics description.
    /// </summary>
    public static string GetSpecialMechanics(string locationID)
    {
        if (SpecialMechanics.TryGetValue(locationID, out string mechanics))
        {
            return mechanics;
        }
        return "";
    }
}

/// <summary>
/// Location progression data structure.
/// </summary>
[System.Serializable]
public struct LocationProgression
{
    public int tier;              // 0-6 (difficulty tier)
    public int recommendedLevel;  // Suggested player level
    public int unlockOrder;       // Order players should unlock (1-13)
}

/// <summary>
/// Secret area definition structure.
/// </summary>
[System.Serializable]
public class SecretAreaDefinition
{
    public string secretID;
    public string secretName;
    public string locationID;
    public Vector3 position;
    public float radius;
    public string description;
    public bool requiresElditchEye;
    public string unlockHint;
}

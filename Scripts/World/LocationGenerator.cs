using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Agent 14: Location & World Generation Agent - LocationGenerator.cs
/// Editor tool that generates all 13 fishing location ScriptableObjects with
/// complete fish distributions and configuration.
/// </summary>
public class LocationGenerator : MonoBehaviour
{
#if UNITY_EDITOR
    [ContextMenu("Generate All 13 Locations")]
    public void GenerateAllLocations()
    {
        Debug.Log("[LocationGenerator] Starting generation of all 13 locations...");

        GenerateCalmLake();
        GenerateRockyCoastline();
        GenerateDeepOceanTrenches();
        GenerateFogShroudedSwamp();
        GenerateArcticWaters();
        GenerateCoralReef();
        GenerateUndergroundCavern();
        GenerateShipwreckGraveyard();
        GenerateVolcanicVent();
        GenerateBioluminescentBay();
        GenerateTidalPools();
        GenerateMangroveForest();
        GenerateAbyssalTrench();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[LocationGenerator] All 13 locations generated successfully!");
    }

    #region Location 1: Calm Lake (Starter)

    private void GenerateCalmLake()
    {
        LocationData location = CreateLocationAsset("calm_lake", "Calm Lake");

        location.description = "A peaceful freshwater lake perfect for beginners. Calm waters, sunny skies, and abundant fish make this the ideal starting point for any angler.";
        location.licenseCost = 0; // FREE - starter location
        location.difficulty = LocationDifficulty.Beginner;
        location.biomeType = BiomeType.Freshwater;

        // Fish pool - 20 common/uncommon species
        location.fishSpeciesIDs = new List<string>
        {
            "largemouth_bass", "bluegill", "crappie", "sunfish", "rock_bass",
            "yellow_perch", "brook_trout", "channel_catfish", "bullhead",
            "carp", "pickerel", "shad", "sucker", "dace", "drum",
            "pumpkinseed", "warmouth", "white_perch", "redfin_pickerel", "smallmouth_bass"
        };

        location.allowedWeather = new List<WeatherType> { WeatherType.Clear, WeatherType.Rain };
        location.sanityDrainModifier = 0f; // No sanity drain - protected area
        location.hazardSpawnRateModifier = 0f; // No night hazards
        location.rareFishSpawnMultiplier = 0.8f;
        location.legendaryFishSpawnMultiplier = 0.1f;
        location.aberrantSpawnMultiplier = 0f; // No aberrants here

        location.dockPosition = new Vector3(0, 0, 0);
        location.spawnBoundsMin = new Vector3(-200, -20, -200);
        location.spawnBoundsMax = new Vector3(200, 0, 200);

        location.npcIDs = new List<string> { "old_fisher" };
        location.secretAreaIDs = new List<string> { "calm_lake_hidden_cove" };
        location.specialMechanics = "Tutorial zone - safe at all times";

        EditorUtility.SetDirty(location);
        Debug.Log("[LocationGenerator] Created: Calm Lake");
    }

    #endregion

    #region Location 2: Rocky Coastline

    private void GenerateRockyCoastline()
    {
        LocationData location = CreateLocationAsset("rocky_coastline", "Rocky Coastline");

        location.description = "Coastal rocks and moderate waves characterize this saltwater fishing spot. Seagulls circle overhead as you cast for larger catches.";
        location.licenseCost = 500;
        location.difficulty = LocationDifficulty.Easy;
        location.biomeType = BiomeType.Coastal;

        // 18 species - mix of coastal and carryover common fish
        location.fishSpeciesIDs = new List<string>
        {
            "striped_bass", "red_drum", "snook", "yelloweye_rockfish",
            "white_bass", "ling_cod", "king_mackerel",
            "largemouth_bass", "bluegill", "crappie", "yellow_perch",
            "channel_catfish", "carp", "drum", "sunfish",
            "flounder", "sea_bass", "spot"
        };

        location.allowedWeather = new List<WeatherType>
        {
            WeatherType.Clear, WeatherType.Rain, WeatherType.Fog
        };
        location.sanityDrainModifier = 0.8f;
        location.hazardSpawnRateModifier = 0.5f; // Moderate night hazards
        location.rareFishSpawnMultiplier = 1.2f;

        location.dockPosition = new Vector3(500, 0, 0);
        location.spawnBoundsMin = new Vector3(400, -30, -200);
        location.spawnBoundsMax = new Vector3(800, 0, 200);

        location.npcIDs = new List<string> { "lighthouse_keeper" };
        location.secretAreaIDs = new List<string> { "tide_pools_secret" };
        location.specialMechanics = "Crab pot hotspot - 150% catch rate";

        EditorUtility.SetDirty(location);
        Debug.Log("[LocationGenerator] Created: Rocky Coastline");
    }

    #endregion

    #region Location 3: Deep Ocean Trenches

    private void GenerateDeepOceanTrenches()
    {
        LocationData location = CreateLocationAsset("deep_ocean_trenches", "Deep Ocean Trenches");

        location.description = "The open ocean stretches endlessly in all directions. Deep blue waters hide massive predators and rare catches. Storms are common.";
        location.licenseCost = 1500;
        location.difficulty = LocationDifficulty.Hard;
        location.biomeType = BiomeType.Ocean;

        // 20 species - rare focus
        location.fishSpeciesIDs = new List<string>
        {
            "bluefin_tuna", "albacore_tuna", "blue_marlin", "swordfish",
            "pacific_halibut", "giant_barracuda", "giant_trevally",
            "white_sturgeon", "sailfish", "yellowfin_tuna",
            "aberrant_deep_lurker", "aberrant_trench_eel", "aberrant_void_ray",
            "giant_squid", "mahi_mahi", "wahoo", "cobia",
            "deep_sea_grouper", "oarfish", "lancetfish"
        };

        location.allowedWeather = new List<WeatherType>
        {
            WeatherType.Clear, WeatherType.Rain, WeatherType.Storm, WeatherType.Fog
        };
        location.sanityDrainModifier = 1.2f;
        location.hazardSpawnRateModifier = 1.5f;
        location.rareFishSpawnMultiplier = 2.0f;
        location.legendaryFishSpawnMultiplier = 1.5f;
        location.aberrantSpawnMultiplier = 1.5f;

        location.dockPosition = new Vector3(2000, 0, 0);
        location.spawnBoundsMin = new Vector3(1800, -200, -300);
        location.spawnBoundsMax = new Vector3(2500, -50, 300);

        location.secretAreaIDs = new List<string> { "underwater_canyon" };
        location.specialMechanics = "Extreme depth (200m+) - Harpoon required for large fish";

        EditorUtility.SetDirty(location);
        Debug.Log("[LocationGenerator] Created: Deep Ocean Trenches");
    }

    #endregion

    #region Location 4: Fog-Shrouded Swamp

    private void GenerateFogShroudedSwamp()
    {
        LocationData location = CreateLocationAsset("fog_shrouded_swamp", "Fog-Shrouded Swamp");

        location.description = "Murky water and constant fog create an eerie atmosphere. Twisted trees loom overhead. This swamp has the highest aberrant spawn rate in the game.";
        location.licenseCost = 2000;
        location.difficulty = LocationDifficulty.Medium;
        location.biomeType = BiomeType.Swamp;

        // 15 species - aberrant focus
        location.fishSpeciesIDs = new List<string>
        {
            "alligator_gar", "bowfin", "gar", "corrupted_gar",
            "aberrant_catfish", "aberrant_bass", "phantom_trout",
            "swamp_mutant_1", "swamp_mutant_2", "swamp_mutant_3",
            "ghost_pike", "cursed_carp", "blood_eel",
            "channel_catfish", "bullhead"
        };

        location.allowedWeather = new List<WeatherType>
        {
            WeatherType.Fog, WeatherType.Rain
        };
        location.sanityDrainModifier = 1.5f;
        location.hazardSpawnRateModifier = 2.0f;
        location.hasPermanentFog = true;
        location.aberrantSpawnMultiplier = 3.0f; // 50% at night with modifier

        location.dockPosition = new Vector3(0, 0, 800);
        location.spawnBoundsMin = new Vector3(-200, -15, 700);
        location.spawnBoundsMax = new Vector3(200, 0, 1000);

        location.npcIDs = new List<string> { "mystic" };
        location.secretAreaIDs = new List<string> { "ancient_altar_swamp" };
        location.isStoryLocation = true;
        location.specialMechanics = "HIGHEST aberrant spawn rate - Ghost Ship spawns frequently";

        EditorUtility.SetDirty(location);
        Debug.Log("[LocationGenerator] Created: Fog-Shrouded Swamp");
    }

    #endregion

    #region Location 5: Arctic Waters

    private void GenerateArcticWaters()
    {
        LocationData location = CreateLocationAsset("arctic_waters", "Arctic Waters");

        location.description = "Icy waters surrounded by icebergs. The aurora borealis illuminates the night sky. Cold damage drains sanity faster, but the beauty is worth it.";
        location.licenseCost = 3000;
        location.difficulty = LocationDifficulty.Hard;
        location.biomeType = BiomeType.Arctic;

        // 12 species - cold-water
        location.fishSpeciesIDs = new List<string>
        {
            "arctic_char", "lake_trout", "coho_salmon", "steelhead",
            "king_salmon", "rainbow_trout",
            "ice_variant_pike", "ice_variant_cod", "ice_variant_halibut",
            "frozen_aberrant", "glacier_fish", "aurora_trout"
        };

        location.allowedWeather = new List<WeatherType>
        {
            WeatherType.Clear, WeatherType.Storm, WeatherType.Fog
        };
        location.sanityDrainModifier = 1.5f; // Cold damage
        location.hazardSpawnRateModifier = 0.3f; // Too cold for most creatures
        location.temperatureModifier = -30f;

        location.dockPosition = new Vector3(-1500, 0, 0);
        location.spawnBoundsMin = new Vector3(-1800, -40, -250);
        location.spawnBoundsMax = new Vector3(-1200, 0, 250);

        location.npcIDs = new List<string> { "captain" };
        location.secretAreaIDs = new List<string> { "frozen_shipwreck" };
        location.specialMechanics = "Cold damage - sanity drains 1.5x faster. Icebergs as obstacles.";

        EditorUtility.SetDirty(location);
        Debug.Log("[LocationGenerator] Created: Arctic Waters");
    }

    #endregion

    #region Location 6: Coral Reef

    private void GenerateCoralReef()
    {
        LocationData location = CreateLocationAsset("coral_reef", "Coral Reef");

        location.description = "Crystal clear tropical waters teem with colorful fish. The coral reef is the most biodiverse location in the game.";
        location.licenseCost = 2500;
        location.difficulty = LocationDifficulty.Medium;
        location.biomeType = BiomeType.Tropical;

        // 25 species - HIGHEST diversity
        location.fishSpeciesIDs = new List<string>
        {
            "tarpon", "snook", "red_drum", "king_mackerel",
            "giant_grouper", "giant_trevally",
            "parrotfish", "angelfish", "butterflyfish", "triggerfish",
            "barracuda", "bonefish", "permit", "jack_crevalle",
            "reef_shark", "manta_ray", "lionfish", "snapper",
            "grouper", "hogfish", "amberjack", "pompano",
            "tropical_variant_1", "tropical_variant_2", "rainbow_fish"
        };

        location.allowedWeather = new List<WeatherType>
        {
            WeatherType.Clear, WeatherType.Rain
        };
        location.sanityDrainModifier = 0.9f;
        location.hazardSpawnRateModifier = 0.8f;
        location.rareFishSpawnMultiplier = 1.5f;
        location.temperatureModifier = 25f;

        location.dockPosition = new Vector3(1000, 0, 1000);
        location.spawnBoundsMin = new Vector3(800, -50, 800);
        location.spawnBoundsMax = new Vector3(1400, 0, 1400);

        location.npcIDs = new List<string> { "scientist" };
        location.secretAreaIDs = new List<string> { "coral_cave_system" };
        location.specialMechanics = "HIGHEST fish diversity (25 species) - Perfect for split-screen underwater view";

        EditorUtility.SetDirty(location);
        Debug.Log("[LocationGenerator] Created: Coral Reef");
    }

    #endregion

    #region Location 7: Underground Cavern

    private void GenerateUndergroundCavern()
    {
        LocationData location = CreateLocationAsset("underground_cavern", "Underground Cavern");

        location.description = "Dark caves lit only by glowing mushrooms. An underground river flows through ancient chambers. This is where major story revelations occur.";
        location.licenseCost = 4000;
        location.difficulty = LocationDifficulty.Extreme;
        location.biomeType = BiomeType.Underground;

        // 10 species - all unique
        location.fishSpeciesIDs = new List<string>
        {
            "blind_cave_fish_1", "blind_cave_fish_2", "blind_cave_fish_3",
            "blind_cave_fish_4", "blind_cave_fish_5",
            "ancient_coelacanth", "bioluminescent_eel", "cave_aberrant",
            "primordial_salamander", "entity_fragment"
        };

        location.allowedWeather = new List<WeatherType>(); // No weather underground
        location.sanityDrainModifier = 2.0f;
        location.hazardSpawnRateModifier = 2.5f;
        location.hasPermanentFog = true; // Darkness
        location.aberrantSpawnMultiplier = 2.0f;

        location.dockPosition = new Vector3(-500, -100, -500);
        location.spawnBoundsMin = new Vector3(-700, -150, -700);
        location.spawnBoundsMax = new Vector3(-300, -50, -300);

        location.npcIDs = new List<string> { "hermit" };
        location.secretAreaIDs = new List<string> { "ancient_altar_cavern", "entity_prison" };
        location.isStoryLocation = true;
        location.specialMechanics = "PURE MYSTERY/HORROR - Requires upgraded lights. Story climax location.";

        EditorUtility.SetDirty(location);
        Debug.Log("[LocationGenerator] Created: Underground Cavern");
    }

    #endregion

    #region Location 8: Shipwreck Graveyard

    private void GenerateShipwreckGraveyard()
    {
        LocationData location = CreateLocationAsset("shipwreck_graveyard", "Shipwreck Graveyard");

        location.description = "Shallow waters filled with sunken ships from decades past. The best location for dredging treasures and artifacts.";
        location.licenseCost = 3500;
        location.difficulty = LocationDifficulty.Hard;
        location.biomeType = BiomeType.Ocean;

        // 15 species + focus on dredging
        location.fishSpeciesIDs = new List<string>
        {
            "striped_bass", "red_drum", "flounder", "sea_bass",
            "ling_cod", "yelloweye_rockfish", "giant_grouper",
            "ghost_fish", "wreck_lurker", "barnacle_beast",
            "cursed_sailor_fish", "treasure_guardian",
            "octopus", "moray_eel", "conger_eel"
        };

        location.allowedWeather = new List<WeatherType>
        {
            WeatherType.Clear, WeatherType.Rain, WeatherType.Fog, WeatherType.Storm
        };
        location.sanityDrainModifier = 1.3f;
        location.hazardSpawnRateModifier = 1.8f; // Ghost Ship frequent

        location.dockPosition = new Vector3(1500, 0, -800);
        location.spawnBoundsMin = new Vector3(1300, -40, -1000);
        location.spawnBoundsMax = new Vector3(1800, 0, -600);

        location.npcIDs = new List<string> { "drunk_sailor" };
        location.secretAreaIDs = new List<string> { "captains_treasure" };
        location.specialMechanics = "BEST dredging location - High scrap yield. Ghost Ship spawns frequently.";

        EditorUtility.SetDirty(location);
        Debug.Log("[LocationGenerator] Created: Shipwreck Graveyard");
    }

    #endregion

    #region Location 9: Volcanic Vent

    private void GenerateVolcanicVent()
    {
        LocationData location = CreateLocationAsset("volcanic_vent", "Volcanic Vent");

        location.description = "Geothermal vents bubble beneath the surface, casting an eerie orange glow. Heat-adapted fish and legendary creatures thrive here.";
        location.licenseCost = 5000;
        location.difficulty = LocationDifficulty.Extreme;
        location.biomeType = BiomeType.Volcanic;

        // 12 species - heat-adapted + legendaries
        location.fishSpeciesIDs = new List<string>
        {
            "lava_eel", "thermal_bass", "magma_grouper",
            "fire_aberrant_1", "fire_aberrant_2", "fire_aberrant_3",
            "volcanic_dragon", "infernal_sturgeon", "ember_trout",
            "ancient_leviathan", // Legendary boss
            "phoenix_fish", "sulfur_swimmer"
        };

        location.allowedWeather = new List<WeatherType>
        {
            WeatherType.Clear, WeatherType.Storm
        };
        location.sanityDrainModifier = 1.8f;
        location.hazardSpawnRateModifier = 2.5f; // Extreme hazards
        location.legendaryFishSpawnMultiplier = 10f; // 10% legendary rate!
        location.aberrantSpawnMultiplier = 2.0f;
        location.temperatureModifier = 80f;

        location.dockPosition = new Vector3(-1000, 0, 1500);
        location.spawnBoundsMin = new Vector3(-1300, -100, 1300);
        location.spawnBoundsMax = new Vector3(-700, -20, 1700);

        location.secretAreaIDs = new List<string> { "leviathan_lair" };
        location.specialMechanics = "Heat damage zones - HIGHEST legendary spawn rate (10%). Vortexes spawn frequently.";

        EditorUtility.SetDirty(location);
        Debug.Log("[LocationGenerator] Created: Volcanic Vent");
    }

    #endregion

    #region Location 10: Bioluminescent Bay

    private void GenerateBioluminescentBay()
    {
        LocationData location = CreateLocationAsset("bioluminescent_bay", "Bioluminescent Bay");

        location.description = "A magical bay where glowing plankton illuminate the water at night. The most beautiful location in the game, and surprisingly safe.";
        location.licenseCost = 4500;
        location.difficulty = LocationDifficulty.Medium;
        location.biomeType = BiomeType.Bioluminescent;

        // 18 species - glowing variants
        location.fishSpeciesIDs = new List<string>
        {
            "glow_bass", "luminous_trout", "starlight_salmon",
            "crystal_perch", "moonbeam_snapper", "aurora_angelfish",
            "neon_tetra_large", "phosphor_pike", "radiant_ray",
            "celestial_dragon_fish", // Legendary
            "glimmer_grouper", "shine_squid", "gleam_eel",
            "sparkle_shad", "twinkle_tuna", "shimmer_sunfish",
            "bright_bream", "luster_ling"
        };

        location.allowedWeather = new List<WeatherType>
        {
            WeatherType.Clear, WeatherType.Fog
        };
        location.sanityDrainModifier = 0.5f; // LOWER sanity drain!
        location.hazardSpawnRateModifier = 0.2f; // Almost no hazards
        location.rareFishSpawnMultiplier = 1.8f;

        location.dockPosition = new Vector3(500, 0, -1200);
        location.spawnBoundsMin = new Vector3(300, -30, -1400);
        location.spawnBoundsMax = new Vector3(700, 0, -1000);

        location.npcIDs = new List<string> { "child" };
        location.secretAreaIDs = new List<string> { "tidal_gate_altar" };
        location.isStoryLocation = true;
        location.specialMechanics = "Most beautiful at night - INVERTED RISK (safer at night). No fish thieves. Tidal Gate unlock location.";

        EditorUtility.SetDirty(location);
        Debug.Log("[LocationGenerator] Created: Bioluminescent Bay");
    }

    #endregion

    #region Location 11: Tidal Pools

    private void GenerateTidalPools()
    {
        LocationData location = CreateLocationAsset("tidal_pools", "Tidal Pools");

        location.description = "Shallow rocky pools revealed at low tide. Perfect for crab pots and small fish. Safe and relaxing.";
        location.licenseCost = 1500;
        location.difficulty = LocationDifficulty.Easy;
        location.biomeType = BiomeType.Coastal;

        // 20 species - small fish + shellfish focus
        location.fishSpeciesIDs = new List<string>
        {
            "rock_bass", "sunfish", "bluegill", "perch_small",
            "minnow_school", "silverside", "killifish", "sculpin",
            "sand_dab", "goby", "blenny", "shrimp_large",
            "crab_blue", "crab_rock", "lobster_small",
            "clam_harvester", "oyster_pearl", "mussel_cluster",
            "sea_urchin", "starfish_rare"
        };

        location.allowedWeather = new List<WeatherType>
        {
            WeatherType.Clear, WeatherType.Rain, WeatherType.Fog
        };
        location.sanityDrainModifier = 0.3f;
        location.hazardSpawnRateModifier = 0.1f; // Very safe

        location.dockPosition = new Vector3(-800, 0, -800);
        location.spawnBoundsMin = new Vector3(-1000, -10, -1000);
        location.spawnBoundsMax = new Vector3(-600, 0, -600);

        location.npcIDs = new List<string> { "shopkeeper" };
        location.secretAreaIDs = new List<string> { "pearl_oyster_bed" };
        location.specialMechanics = "Crab pot catch rate +200%. Safe shallow water. Perfect for passive fishing.";

        EditorUtility.SetDirty(location);
        Debug.Log("[LocationGenerator] Created: Tidal Pools");
    }

    #endregion

    #region Location 12: Mangrove Forest

    private void GenerateMangroveForest()
    {
        LocationData location = CreateLocationAsset("mangrove_forest", "Mangrove Forest");

        location.description = "Tangled roots create a maze-like environment in brackish water. Unique fish species dwell among the roots, some even climbing them!";
        location.licenseCost = 2000;
        location.difficulty = LocationDifficulty.Medium;
        location.biomeType = BiomeType.Mangrove;

        // 16 species - brackish + tree-dwellers
        location.fishSpeciesIDs = new List<string>
        {
            "tarpon", "snook", "striped_bass",
            "mangrove_snapper", "root_climber_1", "root_climber_2",
            "brackish_barracuda", "mudskipper_large", "archerfish",
            "aberrant_root_dweller", "tree_lurker", "canopy_eel",
            "red_drum", "sheepshead", "black_drum", "jack_crevalle"
        };

        location.allowedWeather = new List<WeatherType>
        {
            WeatherType.Clear, WeatherType.Rain, WeatherType.Fog
        };
        location.sanityDrainModifier = 1.1f;
        location.hazardSpawnRateModifier = 1.0f;
        location.hasPermanentFog = false; // Only morning fog

        location.dockPosition = new Vector3(800, 0, -1500);
        location.spawnBoundsMin = new Vector3(600, -25, -1700);
        location.spawnBoundsMax = new Vector3(1000, 0, -1300);

        location.secretAreaIDs = new List<string> { "ancient_altar_mangrove" };
        location.specialMechanics = "Navigation challenge - maze-like roots. Unique 'tree-dweller' fish. Dense fog in mornings.";

        EditorUtility.SetDirty(location);
        Debug.Log("[LocationGenerator] Created: Mangrove Forest");
    }

    #endregion

    #region Location 13: Abyssal Trench (ENDGAME)

    private void GenerateAbyssalTrench()
    {
        LocationData location = CreateLocationAsset("abyssal_trench", "Abyssal Trench");

        location.description = "The deepest, darkest place in the world. Crushing depths and eldritch horrors await. Only the most prepared anglers dare venture here.";
        location.licenseCost = 10000; // EXPENSIVE
        location.difficulty = LocationDifficulty.Extreme;
        location.biomeType = BiomeType.Abyssal;

        // 8 species - ALL legendary/aberrant
        location.fishSpeciesIDs = new List<string>
        {
            "void_emperor", // Final boss fish
            "ancient_leviathan", // All 5 legendaries can spawn
            "celestial_dragon_fish",
            "volcanic_dragon",
            "abyssal_aberrant_1", "abyssal_aberrant_2",
            "entity_spawn", "primordial_terror"
        };

        location.allowedWeather = new List<WeatherType>(); // No weather - too deep
        location.sanityDrainModifier = 2.0f; // Double drain!
        location.hazardSpawnRateModifier = 5.0f; // ALL hazards at max
        location.legendaryFishSpawnMultiplier = 5.0f;
        location.aberrantSpawnMultiplier = 5.0f;
        location.hasPermanentFog = true; // Pitch black

        location.dockPosition = new Vector3(-2000, 0, -2000);
        location.spawnBoundsMin = new Vector3(-2500, -500, -2500);
        location.spawnBoundsMax = new Vector3(-1500, -100, -1500);

        location.npcIDs = new List<string>(); // No NPCs - completely isolated
        location.secretAreaIDs = new List<string> { "entity_awakening_site" };
        location.isStoryLocation = true;
        location.specialMechanics = "FINAL LOCATION - Extreme depth (500m+). Requires all upgrades. Story conclusion.";

        EditorUtility.SetDirty(location);
        Debug.Log("[LocationGenerator] Created: Abyssal Trench (ENDGAME)");
    }

    #endregion

    #region Helper Methods

    private LocationData CreateLocationAsset(string id, string name)
    {
        string path = $"Assets/Resources/Locations/{id}.asset";

        // Create directory if it doesn't exist
        string directory = System.IO.Path.GetDirectoryName(path);
        if (!System.IO.Directory.Exists(directory))
        {
            System.IO.Directory.CreateDirectory(directory);
        }

        // Check if asset already exists
        LocationData existingAsset = AssetDatabase.LoadAssetAtPath<LocationData>(path);
        if (existingAsset != null)
        {
            Debug.Log($"[LocationGenerator] Updating existing asset: {name}");
            return existingAsset;
        }

        // Create new asset
        LocationData newAsset = ScriptableObject.CreateInstance<LocationData>();
        newAsset.locationID = id;
        newAsset.locationName = name;
        newAsset.sceneName = $"Location_{id}";

        AssetDatabase.CreateAsset(newAsset, path);
        Debug.Log($"[LocationGenerator] Created new asset: {name} at {path}");

        return newAsset;
    }

    #endregion
#endif
}

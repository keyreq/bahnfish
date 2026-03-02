using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Agent 8: Fish AI & Behavior Agent - FishSpeciesGenerator.cs
/// Generates all 60 fish species as ScriptableObjects.
/// 20 Common, 15 Uncommon, 10 Rare, 5 Legendary, 10 Aberrant variants.
/// </summary>
public class FishSpeciesGenerator : MonoBehaviour
{
    // Fish species definitions
    public static class FishSpeciesDefinitions
    {
        // Common Fish (20 species)
        public static readonly List<FishData> CommonFish = new List<FishData>
        {
            new FishData("largemouth_bass", "Largemouth Bass", FishRarity.Common, 15f, new Vector2Int(2, 1), 0.5f, 3f, 1f, 3f, 20f, 40f, TimeOfDay.Day, new[] { WeatherType.Clear, WeatherType.Rain }, new[] { "starter_lake", "river_bend" }, FishBehaviorType.Normal, new[] { BaitType.Worms, BaitType.Lures }, "A common freshwater fish found in lakes and rivers. Popular among beginner anglers."),
            new FishData("bluegill", "Bluegill", FishRarity.Common, 8f, new Vector2Int(1, 1), 0f, 2f, 0.3f, 1f, 10f, 20f, TimeOfDay.Day, new[] { WeatherType.Clear }, new[] { "starter_lake" }, FishBehaviorType.Normal, new[] { BaitType.Worms }, "Small panfish with distinctive blue coloring on its gills."),
            new FishData("rainbow_trout", "Rainbow Trout", FishRarity.Common, 20f, new Vector2Int(2, 1), 2f, 8f, 1.5f, 4f, 25f, 50f, TimeOfDay.Dawn, new[] { WeatherType.Clear, WeatherType.Rain }, new[] { "mountain_stream", "river_bend" }, FishBehaviorType.Normal, new[] { BaitType.Lures, BaitType.Specialized }, "Beautiful trout with rainbow-colored stripe. Prefers cold, clear water."),
            new FishData("channel_catfish", "Channel Catfish", FishRarity.Common, 18f, new Vector2Int(2, 2), 3f, 10f, 1f, 4f, 30f, 60f, TimeOfDay.Night, new[] { WeatherType.Clear, WeatherType.Rain }, new[] { "starter_lake", "river_bend", "muddy_delta" }, FishBehaviorType.Normal, new[] { BaitType.Chum, BaitType.Worms }, "Bottom-feeding catfish active at night. Strong fighters."),
            new FishData("yellow_perch", "Yellow Perch", FishRarity.Common, 12f, new Vector2Int(1, 2), 1f, 5f, 0.8f, 2.5f, 15f, 30f, TimeOfDay.Day, new[] { WeatherType.Clear }, new[] { "starter_lake", "coastal_waters" }, FishBehaviorType.Normal, new[] { BaitType.Worms, BaitType.Lures }, "Striped yellow fish that often schools in large groups."),
            new FishData("crappie", "Crappie", FishRarity.Common, 10f, new Vector2Int(1, 1), 1f, 4f, 0.5f, 2f, 12f, 25f, TimeOfDay.Dusk, new[] { WeatherType.Clear }, new[] { "starter_lake", "swamp" }, FishBehaviorType.Normal, new[] { BaitType.Lures }, "Popular panfish with delicate, sweet meat."),
            new FishData("sunfish", "Pumpkinseed Sunfish", FishRarity.Common, 7f, new Vector2Int(1, 1), 0f, 2f, 0.3f, 0.8f, 8f, 15f, TimeOfDay.Day, new[] { WeatherType.Clear }, new[] { "starter_lake" }, FishBehaviorType.Normal, new[] { BaitType.Worms }, "Colorful small fish with red and orange spots."),
            new FishData("rock_bass", "Rock Bass", FishRarity.Common, 11f, new Vector2Int(1, 1), 1f, 5f, 0.6f, 2f, 12f, 22f, TimeOfDay.Day, new[] { WeatherType.Clear }, new[] { "rocky_coast", "river_bend" }, FishBehaviorType.Normal, new[] { BaitType.Worms, BaitType.Lures }, "Hardy bass that lives near rocky structures."),
            new FishData("white_bass", "White Bass", FishRarity.Common, 13f, new Vector2Int(2, 1), 1f, 4f, 1f, 3f, 20f, 35f, TimeOfDay.Day, new[] { WeatherType.Clear, WeatherType.Rain }, new[] { "starter_lake", "river_bend" }, FishBehaviorType.Normal, new[] { BaitType.Lures, BaitType.Chum }, "Schooling bass with silver-white coloration."),
            new FishData("brook_trout", "Brook Trout", FishRarity.Common, 16f, new Vector2Int(2, 1), 2f, 6f, 1f, 3f, 18f, 35f, TimeOfDay.Dawn, new[] { WeatherType.Clear }, new[] { "mountain_stream" }, FishBehaviorType.Normal, new[] { BaitType.Lures }, "Native trout with beautiful spotted pattern."),
            new FishData("carp", "Common Carp", FishRarity.Common, 14f, new Vector2Int(3, 2), 2f, 8f, 2f, 8f, 40f, 80f, TimeOfDay.Day, new[] { WeatherType.Clear, WeatherType.Rain }, new[] { "starter_lake", "muddy_delta" }, FishBehaviorType.Normal, new[] { BaitType.Chum, BaitType.Worms }, "Large bottom-feeder known for strong fights."),
            new FishData("bullhead", "Brown Bullhead", FishRarity.Common, 9f, new Vector2Int(1, 2), 2f, 6f, 0.8f, 2.5f, 15f, 30f, TimeOfDay.Night, new[] { WeatherType.Clear }, new[] { "starter_lake", "swamp" }, FishBehaviorType.Normal, new[] { BaitType.Worms, BaitType.Chum }, "Small catfish species, active at night."),
            new FishData("pickerel", "Chain Pickerel", FishRarity.Common, 17f, new Vector2Int(3, 1), 1f, 5f, 1.5f, 4f, 30f, 60f, TimeOfDay.Day, new[] { WeatherType.Clear }, new[] { "swamp", "starter_lake" }, FishBehaviorType.Normal, new[] { BaitType.Lures }, "Smaller pike relative with chain-like markings."),
            new FishData("shad", "American Shad", FishRarity.Common, 12f, new Vector2Int(2, 1), 3f, 10f, 1f, 3f, 25f, 50f, TimeOfDay.Dawn, new[] { WeatherType.Clear }, new[] { "river_bend", "coastal_waters" }, FishBehaviorType.Normal, new[] { BaitType.Lures }, "Migratory fish that travels between salt and fresh water."),
            new FishData("sucker", "White Sucker", FishRarity.Common, 8f, new Vector2Int(2, 1), 2f, 8f, 1f, 3f, 20f, 40f, TimeOfDay.Day, new[] { WeatherType.Clear }, new[] { "river_bend", "mountain_stream" }, FishBehaviorType.Normal, new[] { BaitType.Worms }, "Bottom-feeding fish with downturned mouth."),
            new FishData("dace", "Longnose Dace", FishRarity.Common, 5f, new Vector2Int(1, 1), 1f, 3f, 0.2f, 0.5f, 5f, 10f, TimeOfDay.Day, new[] { WeatherType.Clear }, new[] { "mountain_stream" }, FishBehaviorType.Normal, new[] { BaitType.Worms }, "Small minnow commonly used as bait."),
            new FishData("smelt", "Rainbow Smelt", FishRarity.Common, 6f, new Vector2Int(1, 1), 5f, 15f, 0.3f, 1f, 8f, 15f, TimeOfDay.Night, new[] { WeatherType.Clear }, new[] { "coastal_waters", "deep_ocean" }, FishBehaviorType.Normal, new[] { BaitType.Lures }, "Small silvery fish that schools in large numbers."),
            new FishData("drum", "Freshwater Drum", FishRarity.Common, 13f, new Vector2Int(2, 2), 3f, 12f, 2f, 6f, 35f, 70f, TimeOfDay.Day, new[] { WeatherType.Clear, WeatherType.Rain }, new[] { "river_bend", "starter_lake" }, FishBehaviorType.Normal, new[] { BaitType.Worms, BaitType.Chum }, "Makes drumming sound using swim bladder."),
            new FishData("gar", "Longnose Gar", FishRarity.Common, 19f, new Vector2Int(4, 1), 1f, 6f, 2f, 6f, 50f, 100f, TimeOfDay.Day, new[] { WeatherType.Clear }, new[] { "swamp", "muddy_delta" }, FishBehaviorType.Normal, new[] { BaitType.Lures }, "Prehistoric-looking fish with long snout."),
            new FishData("bowfin", "Bowfin", FishRarity.Common, 16f, new Vector2Int(2, 2), 2f, 8f, 2f, 6f, 40f, 80f, TimeOfDay.Dusk, new[] { WeatherType.Clear, WeatherType.Rain }, new[] { "swamp", "starter_lake" }, FishBehaviorType.Normal, new[] { BaitType.Worms, BaitType.Lures }, "Ancient species with remarkable hardiness.")
        };

        // Uncommon Fish (15 species)
        public static readonly List<FishData> UncommonFish = new List<FishData>
        {
            new FishData("northern_pike", "Northern Pike", FishRarity.Uncommon, 45f, new Vector2Int(4, 2), 2f, 10f, 3f, 10f, 60f, 120f, TimeOfDay.Day, new[] { WeatherType.Clear, WeatherType.Rain }, new[] { "starter_lake", "swamp", "northern_lake" }, FishBehaviorType.Normal, new[] { BaitType.Lures, BaitType.Specialized }, "Aggressive predator with sharp teeth. Excellent fighter."),
            new FishData("walleye", "Walleye", FishRarity.Uncommon, 50f, new Vector2Int(3, 2), 5f, 15f, 2.5f, 8f, 50f, 100f, TimeOfDay.Dusk, new[] { WeatherType.Clear, WeatherType.Fog }, new[] { "northern_lake", "river_bend" }, FishBehaviorType.Normal, new[] { BaitType.Lures, BaitType.Specialized }, "Prized gamefish with excellent night vision."),
            new FishData("muskie", "Muskellunge", FishRarity.Uncommon, 65f, new Vector2Int(5, 2), 3f, 12f, 5f, 15f, 80f, 150f, TimeOfDay.Day, new[] { WeatherType.Clear }, new[] { "northern_lake", "river_bend" }, FishBehaviorType.Normal, new[] { BaitType.Lures, BaitType.Specialized }, "Legendary freshwater predator. The fish of 10,000 casts."),
            new FishData("lake_trout", "Lake Trout", FishRarity.Uncommon, 42f, new Vector2Int(3, 2), 15f, 40f, 3f, 9f, 60f, 120f, TimeOfDay.Day, new[] { WeatherType.Clear }, new[] { "northern_lake", "deep_lake" }, FishBehaviorType.Normal, new[] { BaitType.Lures, BaitType.Specialized }, "Deep water trout preferring cold temperatures."),
            new FishData("salmon", "Coho Salmon", FishRarity.Uncommon, 55f, new Vector2Int(3, 2), 8f, 25f, 3f, 10f, 70f, 140f, TimeOfDay.Dawn, new[] { WeatherType.Clear, WeatherType.Rain }, new[] { "coastal_waters", "river_bend", "northern_waters" }, FishBehaviorType.Normal, new[] { BaitType.Lures, BaitType.Specialized }, "Powerful fighter known for acrobatic jumps."),
            new FishData("steelhead", "Steelhead Trout", FishRarity.Uncommon, 48f, new Vector2Int(3, 2), 5f, 20f, 3f, 9f, 65f, 130f, TimeOfDay.Dawn, new[] { WeatherType.Rain, WeatherType.Clear }, new[] { "river_bend", "coastal_waters" }, FishBehaviorType.Normal, new[] { BaitType.Lures, BaitType.Specialized }, "Sea-run rainbow trout, exceptional fighter."),
            new FishData("striped_bass", "Striped Bass", FishRarity.Uncommon, 52f, new Vector2Int(3, 2), 5f, 20f, 4f, 12f, 75f, 150f, TimeOfDay.Day, new[] { WeatherType.Clear, WeatherType.Storm }, new[] { "coastal_waters", "river_bend" }, FishBehaviorType.Normal, new[] { BaitType.Lures, BaitType.Chum }, "Powerful coastal predator with distinctive stripes."),
            new FishData("redfish", "Red Drum", FishRarity.Uncommon, 46f, new Vector2Int(3, 2), 3f, 15f, 3f, 10f, 60f, 120f, TimeOfDay.Dusk, new[] { WeatherType.Clear }, new[] { "coastal_waters", "muddy_delta" }, FishBehaviorType.Normal, new[] { BaitType.Chum, BaitType.Specialized }, "Copper-colored drum with distinctive spot on tail."),
            new FishData("snook", "Common Snook", FishRarity.Uncommon, 49f, new Vector2Int(4, 1), 2f, 12f, 3f, 10f, 70f, 140f, TimeOfDay.Dusk, new[] { WeatherType.Clear, WeatherType.Rain }, new[] { "coastal_waters", "mangrove" }, FishBehaviorType.Normal, new[] { BaitType.Lures, BaitType.Specialized }, "Elusive predator found near mangroves."),
            new FishData("tarpon", "Tarpon", FishRarity.Uncommon, 70f, new Vector2Int(5, 3), 5f, 20f, 8f, 25f, 100f, 200f, TimeOfDay.Day, new[] { WeatherType.Clear, WeatherType.Storm }, new[] { "coastal_waters", "tropical_reef" }, FishBehaviorType.Normal, new[] { BaitType.Lures, BaitType.Chum }, "Silver king - famous for spectacular jumping ability."),
            new FishData("albacore", "Albacore Tuna", FishRarity.Uncommon, 60f, new Vector2Int(3, 2), 20f, 50f, 5f, 15f, 80f, 160f, TimeOfDay.Day, new[] { WeatherType.Clear }, new[] { "deep_ocean", "coastal_waters" }, FishBehaviorType.Normal, new[] { BaitType.Lures, BaitType.Chum }, "Fast-swimming tuna with excellent stamina."),
            new FishData("halibut", "Pacific Halibut", FishRarity.Uncommon, 58f, new Vector2Int(4, 3), 30f, 80f, 10f, 40f, 120f, 250f, TimeOfDay.Day, new[] { WeatherType.Clear, WeatherType.Fog }, new[] { "deep_ocean", "northern_waters" }, FishBehaviorType.Normal, new[] { BaitType.Chum, BaitType.Specialized }, "Large flatfish dwelling on ocean floor."),
            new FishData("lingcod", "Lingcod", FishRarity.Uncommon, 44f, new Vector2Int(3, 2), 15f, 40f, 4f, 12f, 65f, 130f, TimeOfDay.Day, new[] { WeatherType.Clear }, new[] { "rocky_coast", "deep_ocean" }, FishBehaviorType.Normal, new[] { BaitType.Lures, BaitType.Chum }, "Aggressive predator with massive jaw."),
            new FishData("rockfish", "Yelloweye Rockfish", FishRarity.Uncommon, 40f, new Vector2Int(2, 2), 25f, 70f, 3f, 9f, 55f, 110f, TimeOfDay.Day, new[] { WeatherType.Clear }, new[] { "rocky_coast", "deep_ocean" }, FishBehaviorType.Normal, new[] { BaitType.Chum, BaitType.Specialized }, "Deep-dwelling rockfish with distinctive yellow eyes."),
            new FishData("king_mackerel", "King Mackerel", FishRarity.Uncommon, 51f, new Vector2Int(4, 2), 10f, 30f, 4f, 13f, 75f, 150f, TimeOfDay.Day, new[] { WeatherType.Clear, WeatherType.Storm }, new[] { "coastal_waters", "deep_ocean" }, FishBehaviorType.Normal, new[] { BaitType.Lures, BaitType.Chum }, "Fast predator with razor-sharp teeth.")
        };

        // Rare Fish (10 species)
        public static readonly List<FishData> RareFish = new List<FishData>
        {
            new FishData("sturgeon", "White Sturgeon", FishRarity.Rare, 150f, new Vector2Int(6, 3), 40f, 100f, 20f, 80f, 200f, 500f, TimeOfDay.Day, new[] { WeatherType.Clear, WeatherType.Rain }, new[] { "deep_lake", "river_bend", "northern_waters" }, FishBehaviorType.Normal, new[] { BaitType.Chum, BaitType.Rare }, "Ancient giant living 100+ years. Critically endangered."),
            new FishData("marlin", "Blue Marlin", FishRarity.Rare, 200f, new Vector2Int(7, 3), 50f, 150f, 50f, 200f, 300f, 800f, TimeOfDay.Day, new[] { WeatherType.Clear, WeatherType.Storm }, new[] { "deep_ocean", "tropical_waters" }, FishBehaviorType.Normal, new[] { BaitType.Lures, BaitType.Rare }, "Apex predator of the ocean. Trophy of a lifetime."),
            new FishData("swordfish", "Swordfish", FishRarity.Rare, 180f, new Vector2Int(6, 3), 60f, 180f, 40f, 180f, 280f, 700f, TimeOfDay.Night, new[] { WeatherType.Clear }, new[] { "deep_ocean" }, FishBehaviorType.Normal, new[] { BaitType.Lures, BaitType.Rare }, "Powerful deep-sea predator with sword-like bill."),
            new FishData("tuna", "Bluefin Tuna", FishRarity.Rare, 220f, new Vector2Int(5, 4), 40f, 120f, 60f, 250f, 350f, 900f, TimeOfDay.Day, new[] { WeatherType.Clear }, new[] { "deep_ocean", "northern_waters" }, FishBehaviorType.Normal, new[] { BaitType.Lures, BaitType.Rare }, "King of tuna. Incredible speed and power."),
            new FishData("sailfish", "Sailfish", FishRarity.Rare, 190f, new Vector2Int(7, 3), 30f, 100f, 35f, 140f, 250f, 600f, TimeOfDay.Day, new[] { WeatherType.Clear, WeatherType.Storm }, new[] { "tropical_waters", "deep_ocean" }, FishBehaviorType.Normal, new[] { BaitType.Lures, BaitType.Rare }, "Fastest fish in the ocean. Spectacular jumper."),
            new FishData("arapaima", "Arapaima", FishRarity.Rare, 175f, new Vector2Int(6, 4), 5f, 20f, 50f, 180f, 280f, 650f, TimeOfDay.Dusk, new[] { WeatherType.Rain }, new[] { "tropical_river", "swamp" }, FishBehaviorType.Normal, new[] { BaitType.Chum, BaitType.Rare }, "Massive air-breathing fish from the Amazon."),
            new FishData("giant_grouper", "Giant Grouper", FishRarity.Rare, 165f, new Vector2Int(5, 4), 50f, 150f, 60f, 220f, 300f, 700f, TimeOfDay.Day, new[] { WeatherType.Clear }, new[] { "tropical_reef", "deep_ocean" }, FishBehaviorType.Normal, new[] { BaitType.Chum, BaitType.Rare }, "Huge reef dweller. Powerful and stubborn."),
            new FishData("alligator_gar", "Alligator Gar", FishRarity.Rare, 160f, new Vector2Int(7, 2), 3f, 15f, 40f, 150f, 250f, 600f, TimeOfDay.Night, new[] { WeatherType.Clear, WeatherType.Storm }, new[] { "swamp", "muddy_delta", "river_bend" }, FishBehaviorType.Normal, new[] { BaitType.Chum, BaitType.Rare }, "Prehistoric monster with armored scales."),
            new FishData("giant_trevally", "Giant Trevally", FishRarity.Rare, 155f, new Vector2Int(4, 3), 15f, 50f, 30f, 120f, 200f, 500f, TimeOfDay.Day, new[] { WeatherType.Clear, WeatherType.Storm }, new[] { "tropical_reef", "coastal_waters" }, FishBehaviorType.Normal, new[] { BaitType.Lures, BaitType.Rare }, "Powerful reef predator known for explosive strikes."),
            new FishData("giant_barracuda", "Giant Barracuda", FishRarity.Rare, 145f, new Vector2Int(5, 2), 10f, 40f, 25f, 100f, 180f, 450f, TimeOfDay.Day, new[] { WeatherType.Clear }, new[] { "tropical_reef", "coastal_waters" }, FishBehaviorType.Normal, new[] { BaitType.Lures, BaitType.Rare }, "Lightning-fast predator with razor teeth.")
        };

        // Legendary Fish (5 species)
        public static readonly List<FishData> LegendaryFish = new List<FishData>
        {
            new FishData("ancient_leviathan", "Ancient Leviathan", FishRarity.Legendary, 500f, new Vector2Int(10, 5), 100f, 300f, 200f, 800f, 600f, 1500f, TimeOfDay.Night, new[] { WeatherType.Storm }, new[] { "abyss", "deep_ocean" }, FishBehaviorType.Legendary, new[] { BaitType.Legendary }, "Mythical sea serpent from the deepest trenches. Boss fight.", true),
            new FishData("crimson_titan", "Crimson Titan", FishRarity.Legendary, 450f, new Vector2Int(9, 5), 80f, 250f, 180f, 700f, 550f, 1400f, TimeOfDay.Day, new[] { WeatherType.Clear, WeatherType.Storm }, new[] { "volcanic_reef", "deep_ocean" }, FishBehaviorType.Legendary, new[] { BaitType.Legendary }, "Massive red-scaled predator that controls fire. Boss fight.", true),
            new FishData("abyssal_maw", "Abyssal Maw", FishRarity.Legendary, 480f, new Vector2Int(9, 6), 150f, 400f, 220f, 850f, 580f, 1450f, TimeOfDay.Night, new[] { WeatherType.Fog }, new[] { "abyss", "haunted_waters" }, FishBehaviorType.Legendary, new[] { BaitType.Legendary }, "Nightmare creature from the void. Creates vortexes. Boss fight.", true),
            new FishData("celestial_dragon", "Celestial Dragon Fish", FishRarity.Legendary, 460f, new Vector2Int(10, 4), 60f, 200f, 160f, 650f, 520f, 1350f, TimeOfDay.Dawn, new[] { WeatherType.Clear }, new[] { "mountain_lake", "sacred_waters" }, FishBehaviorType.Legendary, new[] { BaitType.Legendary }, "Sacred dragon that brings good fortune. Boss fight.", true),
            new FishData("void_emperor", "Void Emperor", FishRarity.Legendary, 520f, new Vector2Int(11, 6), 180f, 500f, 250f, 900f, 650f, 1600f, TimeOfDay.Night, new[] { WeatherType.Storm, WeatherType.Fog }, new[] { "abyss", "cursed_depths" }, FishBehaviorType.Legendary, new[] { BaitType.Legendary }, "Ruler of the dark depths. Ultimate challenge. Boss fight.", true)
        };

        // Aberrant Variants (10+ mutated fish)
        public static readonly List<FishData> AberrantFish = new List<FishData>
        {
            new FishData("aberrant_bass", "Aberrant Bass", FishRarity.Aberrant, 80f, new Vector2Int(3, 2), 2f, 10f, 3f, 8f, 35f, 70f, TimeOfDay.Night, new[] { WeatherType.Fog, WeatherType.Storm }, new[] { "starter_lake", "haunted_waters" }, FishBehaviorType.Aberrant, new[] { BaitType.Specialized, BaitType.Rare }, "Mutated bass with glowing green eyes. Erratic movements.", false, true, Color.green),
            new FishData("aberrant_pike", "Corrupted Pike", FishRarity.Aberrant, 120f, new Vector2Int(5, 3), 5f, 20f, 8f, 18f, 90f, 180f, TimeOfDay.Night, new[] { WeatherType.Storm }, new[] { "haunted_waters", "cursed_lake" }, FishBehaviorType.Aberrant, new[] { BaitType.Rare }, "Twisted pike with extra fins. Phases through obstacles.", false, true, Color.cyan),
            new FishData("aberrant_catfish", "Aberrant Catfish", FishRarity.Aberrant, 95f, new Vector2Int(4, 3), 8f, 25f, 6f, 15f, 60f, 120f, TimeOfDay.Night, new[] { WeatherType.Fog }, new[] { "swamp", "haunted_waters" }, FishBehaviorType.Aberrant, new[] { BaitType.Chum, BaitType.Rare }, "Bottom-dweller with tentacle-like whiskers.", false, true, Color.magenta),
            new FishData("aberrant_trout", "Phantom Trout", FishRarity.Aberrant, 85f, new Vector2Int(3, 2), 5f, 18f, 4f, 11f, 45f, 90f, TimeOfDay.Night, new[] { WeatherType.Fog }, new[] { "mountain_stream", "haunted_waters" }, FishBehaviorType.Aberrant, new[] { BaitType.Specialized, BaitType.Rare }, "Ghostly trout that can become invisible.", false, true, Color.white),
            new FishData("aberrant_gar", "Corrupted Gar", FishRarity.Aberrant, 110f, new Vector2Int(6, 2), 3f, 15f, 10f, 22f, 85f, 170f, TimeOfDay.Night, new[] { WeatherType.Storm }, new[] { "swamp", "cursed_lake" }, FishBehaviorType.Aberrant, new[] { BaitType.Rare }, "Ancient gar mutated by dark energies.", false, true, Color.red),
            new FishData("aberrant_salmon", "Twisted Salmon", FishRarity.Aberrant, 100f, new Vector2Int(4, 2), 10f, 30f, 7f, 16f, 75f, 150f, TimeOfDay.Night, new[] { WeatherType.Storm }, new[] { "coastal_waters", "haunted_waters" }, FishBehaviorType.Aberrant, new[] { BaitType.Rare }, "Deformed salmon with multiple eyes.", false, true, Color.yellow),
            new FishData("aberrant_carp", "Void Carp", FishRarity.Aberrant, 90f, new Vector2Int(4, 3), 5f, 20f, 5f, 14f, 55f, 110f, TimeOfDay.Night, new[] { WeatherType.Fog }, new[] { "cursed_lake", "haunted_waters" }, FishBehaviorType.Aberrant, new[] { BaitType.Specialized, BaitType.Rare }, "Carp touched by the void. Leaves dark trail.", false, true, Color.black),
            new FishData("aberrant_tuna", "Aberrant Tuna", FishRarity.Aberrant, 150f, new Vector2Int(6, 4), 40f, 120f, 15f, 35f, 180f, 360f, TimeOfDay.Night, new[] { WeatherType.Storm }, new[] { "deep_ocean", "abyss" }, FishBehaviorType.Aberrant, new[] { BaitType.Rare, BaitType.Legendary }, "Mutated tuna with bioluminescent patterns.", false, true, Color.blue),
            new FishData("aberrant_grouper", "Corrupted Grouper", FishRarity.Aberrant, 135f, new Vector2Int(5, 4), 45f, 140f, 12f, 28f, 160f, 320f, TimeOfDay.Night, new[] { WeatherType.Fog, WeatherType.Storm }, new[] { "haunted_reef", "abyss" }, FishBehaviorType.Aberrant, new[] { BaitType.Rare }, "Massive grouper with parasitic growths.", false, true, Color.green),
            new FishData("aberrant_sturgeon", "Ancient Aberrant", FishRarity.Aberrant, 200f, new Vector2Int(8, 4), 50f, 150f, 25f, 100f, 280f, 600f, TimeOfDay.Night, new[] { WeatherType.Storm }, new[] { "abyss", "cursed_depths" }, FishBehaviorType.Aberrant, new[] { BaitType.Rare, BaitType.Legendary }, "Prehistoric sturgeon corrupted by eldritch power.", false, true, new Color(0.5f, 0f, 0.5f))
        };
    }

    // Fish data structure
    public class FishData
    {
        public string id;
        public string name;
        public FishRarity rarity;
        public float baseValue;
        public Vector2Int inventorySize;
        public float minDepth;
        public float maxDepth;
        public float minWeight;
        public float maxWeight;
        public float minLength;
        public float maxLength;
        public TimeOfDay preferredTime;
        public WeatherType[] preferredWeather;
        public string[] allowedLocations;
        public FishBehaviorType behaviorType;
        public BaitType[] preferredBait;
        public string description;
        public bool isLegendary;
        public bool isAberrant;
        public Color aberrantGlowColor;

        public FishData(string id, string name, FishRarity rarity, float baseValue, Vector2Int inventorySize,
            float minDepth, float maxDepth, float minWeight, float maxWeight, float minLength, float maxLength,
            TimeOfDay preferredTime, WeatherType[] preferredWeather, string[] allowedLocations,
            FishBehaviorType behaviorType, BaitType[] preferredBait, string description,
            bool isLegendary = false, bool isAberrant = false, Color aberrantGlowColor = default)
        {
            this.id = id;
            this.name = name;
            this.rarity = rarity;
            this.baseValue = baseValue;
            this.inventorySize = inventorySize;
            this.minDepth = minDepth;
            this.maxDepth = maxDepth;
            this.minWeight = minWeight;
            this.maxWeight = maxWeight;
            this.minLength = minLength;
            this.maxLength = maxLength;
            this.preferredTime = preferredTime;
            this.preferredWeather = preferredWeather;
            this.allowedLocations = allowedLocations;
            this.behaviorType = behaviorType;
            this.preferredBait = preferredBait;
            this.description = description;
            this.isLegendary = isLegendary;
            this.isAberrant = isAberrant;
            this.aberrantGlowColor = aberrantGlowColor == default ? Color.green : aberrantGlowColor;
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Generate All Fish Species")]
    public void GenerateAllFishSpecies()
    {
        Debug.Log("=== Starting Fish Species Generation ===");

        int totalGenerated = 0;
        totalGenerated += GenerateFishList(FishSpeciesDefinitions.CommonFish, "Common");
        totalGenerated += GenerateFishList(FishSpeciesDefinitions.UncommonFish, "Uncommon");
        totalGenerated += GenerateFishList(FishSpeciesDefinitions.RareFish, "Rare");
        totalGenerated += GenerateFishList(FishSpeciesDefinitions.LegendaryFish, "Legendary");
        totalGenerated += GenerateFishList(FishSpeciesDefinitions.AberrantFish, "Aberrant");

        Debug.Log($"=== Fish Generation Complete! Generated {totalGenerated} species ===");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private int GenerateFishList(List<FishData> fishList, string category)
    {
        int count = 0;
        foreach (FishData data in fishList)
        {
            CreateFishSpeciesAsset(data);
            count++;
        }
        Debug.Log($"Generated {count} {category} fish species");
        return count;
    }

    private void CreateFishSpeciesAsset(FishData data)
    {
        string path = $"Assets/Resources/FishSpecies/{data.id}.asset";
        string directory = "Assets/Resources/FishSpecies";

        // Create directory if it doesn't exist
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
        {
            AssetDatabase.CreateFolder("Assets", "Resources");
        }
        if (!AssetDatabase.IsValidFolder(directory))
        {
            AssetDatabase.CreateFolder("Assets/Resources", "FishSpecies");
        }

        // Create or load existing asset
        FishSpeciesData asset = AssetDatabase.LoadAssetAtPath<FishSpeciesData>(path);
        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<FishSpeciesData>();
            AssetDatabase.CreateAsset(asset, path);
        }

        // Set all properties
        asset.fishID = data.id;
        asset.fishName = data.name;
        asset.rarity = data.rarity;
        asset.baseValue = data.baseValue;
        asset.inventorySize = data.inventorySize;
        asset.minDepth = data.minDepth;
        asset.maxDepth = data.maxDepth;
        asset.weightRange = new Vector2(data.minWeight, data.maxWeight);
        asset.lengthRange = new Vector2(data.minLength, data.maxLength);
        asset.preferredTime = data.preferredTime;
        asset.preferredWeather = new List<WeatherType>(data.preferredWeather);
        asset.allowedLocations = new List<string>(data.allowedLocations);
        asset.behaviorType = data.behaviorType;
        asset.preferredBait = new List<BaitType>(data.preferredBait);
        asset.description = data.description;
        asset.isLegendary = data.isLegendary;
        asset.isAberrant = data.isAberrant;
        asset.aberrantGlowColor = data.aberrantGlowColor;

        // Set speed and aggression based on rarity
        asset.speedMultiplier = data.rarity switch
        {
            FishRarity.Common => 1.0f,
            FishRarity.Uncommon => 1.2f,
            FishRarity.Rare => 1.5f,
            FishRarity.Legendary => 2.0f,
            FishRarity.Aberrant => 1.8f,
            _ => 1.0f
        };

        asset.aggression = data.rarity switch
        {
            FishRarity.Common => 0.3f,
            FishRarity.Uncommon => 0.5f,
            FishRarity.Rare => 0.7f,
            FishRarity.Legendary => 0.9f,
            FishRarity.Aberrant => 0.8f,
            _ => 0.5f
        };

        asset.staminaDuration = data.rarity switch
        {
            FishRarity.Common => 20f,
            FishRarity.Uncommon => 40f,
            FishRarity.Rare => 60f,
            FishRarity.Legendary => 120f,
            FishRarity.Aberrant => 50f,
            _ => 30f
        };

        asset.baitPreferenceMultiplier = data.rarity switch
        {
            FishRarity.Common => 2.0f,
            FishRarity.Uncommon => 2.5f,
            FishRarity.Rare => 3.0f,
            FishRarity.Legendary => 4.0f,
            FishRarity.Aberrant => 3.5f,
            _ => 2.0f
        };

        EditorUtility.SetDirty(asset);
    }
#endif
}

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Database of all NPCs in the game with their complete data.
/// Contains the 10+ unique characters with distinct personalities.
/// </summary>
[CreateAssetMenu(fileName = "NPCDatabase", menuName = "Bahnfish/NPC Database")]
public class NPCDatabase : ScriptableObject
{
    [Header("All NPCs")]
    public List<NPCData> allNPCs = new List<NPCData>();

    /// <summary>
    /// Get NPC by ID
    /// </summary>
    public NPCData GetNPCByID(string npcID)
    {
        return allNPCs.Find(npc => npc.npcID == npcID);
    }

    /// <summary>
    /// Initialize default NPCs programmatically
    /// </summary>
    public void InitializeDefaultNPCs()
    {
        allNPCs.Clear();

        // 1. OLD FISHER - Tutorial Mentor
        var oldFisher = ScriptableObject.CreateInstance<NPCData>();
        oldFisher.npcID = "old_fisher";
        oldFisher.npcName = "Old Fisher Tom";
        oldFisher.description = "A weathered veteran who's seen it all. He teaches newcomers the basics but harbors dark secrets about the waters.";
        oldFisher.personality = NPCPersonality.Wise;
        oldFisher.availability = NPCAvailability.Always;
        oldFisher.friendliness = 0.8f;
        oldFisher.homeLocationID = "starter_lake";
        oldFisher.greetings = new List<string>
        {
            "Ah, a new face at the docks. Let me teach you the ropes.",
            "Welcome to the water, friend. It's calmer during the day...",
            "You've got the look of someone who's come seeking answers. I hope you're ready for what you'll find."
        };
        oldFisher.idleDialogue = new List<string>
        {
            "The fish aren't biting like they used to. Something's changed in these waters.",
            "I've been fishing here for forty years. Seen things no one would believe.",
            "If you go out at night, keep your wits about you. The darkness here... it's alive."
        };
        oldFisher.farewells = new List<string>
        {
            "Stay safe out there.",
            "Come back before nightfall if you value your sanity.",
            "Good luck, and may the waters be kind to you."
        };
        oldFisher.questsToGive = new List<string> { "tutorial_first_catch", "tutorial_night_watch", "story_missing_fisher" };
        oldFisher.baseRelationship = 20;
        allNPCs.Add(oldFisher);

        // 2. SHOPKEEPER - Vendor & Gossip
        var shopkeeper = ScriptableObject.CreateInstance<NPCData>();
        shopkeeper.npcID = "shopkeeper";
        shopkeeper.npcName = "Martha";
        shopkeeper.description = "The cheerful shopkeeper who sells supplies and loves to gossip about the town's mysteries.";
        shopkeeper.personality = NPCPersonality.Cheerful;
        shopkeeper.availability = NPCAvailability.DayOnly;
        shopkeeper.friendliness = 0.9f;
        shopkeeper.homeLocationID = "town_market";
        shopkeeper.greetings = new List<string>
        {
            "Welcome to my shop! What can I get you today?",
            "Oh, a new customer! Always happy to see fresh faces.",
            "Business is slow today. Care to hear some local stories while you shop?"
        };
        shopkeeper.idleDialogue = new List<string>
        {
            "Did you hear about the strange lights over the ocean last week?",
            "Marcus used to shop here every morning. I miss him.",
            "I've got the best bait in town, and the freshest supplies!"
        };
        shopkeeper.questsToGive = new List<string> { "tutorial_market_day", "tutorial_upgrade", "fetch_bass", "daily_catch" };
        shopkeeper.isShopkeeper = true;
        shopkeeper.baseRelationship = 15;
        allNPCs.Add(shopkeeper);

        // 3. MYSTIC - Curse Cleanser
        var mystic = ScriptableObject.CreateInstance<NPCData>();
        mystic.npcID = "mystic";
        mystic.npcName = "Madame Esther";
        mystic.description = "A mysterious woman with spiritual powers. She can cleanse curses and provides cryptic warnings about the entity.";
        mystic.personality = NPCPersonality.Mysterious;
        mystic.availability = NPCAvailability.Always;
        mystic.friendliness = 0.5f;
        mystic.homeLocationID = "mystic_hut";
        mystic.greetings = new List<string>
        {
            "I've been expecting you. The spirits whisper your name.",
            "You carry darkness with you. Come, let me help.",
            "The veil is thin here. I can see what others cannot."
        };
        mystic.idleDialogue = new List<string>
        {
            "The entity beneath grows restless. I can feel its hunger.",
            "Your sanity is a precious thing. Guard it well.",
            "The altars call to you. You must find them all."
        };
        mystic.questsToGive = new List<string> { "story_messages", "story_final_ritual" };
        mystic.isMystic = true;
        mystic.baseRelationship = 0;
        allNPCs.Add(mystic);

        // 4. CAPTAIN - Location Unlocker
        var captain = ScriptableObject.CreateInstance<NPCData>();
        captain.npcID = "captain";
        captain.npcName = "Captain Hargrave";
        captain.description = "A gruff sea captain who controls access to dangerous fishing locations. He knows the legends of these waters.";
        captain.personality = NPCPersonality.Hostile;
        captain.availability = NPCAvailability.Always;
        captain.friendliness = 0.3f;
        captain.homeLocationID = "docks";
        captain.greetings = new List<string>
        {
            "What do you want? I'm busy.",
            "You looking for passage to the deeper waters? That'll cost you.",
            "Another fool seeking the mysteries of the deep. Fine."
        };
        captain.idleDialogue = new List<string>
        {
            "I've sailed these waters for thirty years. Lost good men to them.",
            "The deep ocean isn't for amateurs. You'll need better equipment.",
            "Some places out there... they're not meant for the living."
        };
        captain.questsToGive = new List<string> { "tutorial_new_location", "fetch_legendary", "side_master_fisher" };
        captain.baseRelationship = -10;
        allNPCs.Add(captain);

        // 5. SCIENTIST - Research & Analysis
        var scientist = ScriptableObject.CreateInstance<NPCData>();
        scientist.npcID = "scientist";
        scientist.npcName = "Dr. Cassandra Wells";
        scientist.description = "A marine biologist studying the aberrant fish phenomenon. She's desperate for samples and data.";
        scientist.personality = NPCPersonality.Eccentric;
        scientist.availability = NPCAvailability.DayOnly;
        scientist.friendliness = 0.7f;
        scientist.homeLocationID = "research_station";
        scientist.greetings = new List<string>
        {
            "Perfect timing! I need someone with your... skills.",
            "The mutations I've documented are unlike anything in scientific literature!",
            "You fish at night? Fascinating. The specimens you could bring me..."
        };
        scientist.idleDialogue = new List<string>
        {
            "The aberrant fish don't follow normal evolutionary patterns. It's as if something is actively changing them.",
            "My research indicates these mutations started exactly three years ago. What happened then?",
            "I need more samples. Always more samples."
        };
        scientist.questsToGive = new List<string> { "story_aberrant_samples", "story_altars", "side_aquarium", "side_photography" };
        scientist.baseRelationship = 10;
        allNPCs.Add(scientist);

        // 6. LIGHTHOUSE KEEPER - Night Fishing Expert
        var keeper = ScriptableObject.CreateInstance<NPCData>();
        keeper.npcID = "lighthouse_keeper";
        keeper.npcName = "Solomon";
        keeper.description = "The lonely lighthouse keeper who watches over the night waters. He offers tips for night fishing and dark warnings.";
        keeper.personality = NPCPersonality.Gloomy;
        keeper.availability = NPCAvailability.NightOnly;
        keeper.friendliness = 0.5f;
        keeper.homeLocationID = "lighthouse";
        keeper.greetings = new List<string>
        {
            "You're brave to be out at night. Or foolish.",
            "The light keeps some things at bay. But not all.",
            "I keep the light burning. It's all that stands between us and... them."
        };
        keeper.idleDialogue = new List<string>
        {
            "I've seen things from this lighthouse. Shapes in the water that shouldn't exist.",
            "The night fish are valuable, but they come with a price.",
            "Every night I wonder if the light will be enough."
        };
        keeper.questsToGive = new List<string> { "night_fishing_repeatable" };
        keeper.baseRelationship = 5;
        allNPCs.Add(keeper);

        // 7. HERMIT - Dark Abilities Teacher
        var hermit = ScriptableObject.CreateInstance<NPCData>();
        hermit.npcID = "hermit";
        hermit.npcName = "The Hermit";
        hermit.description = "A reclusive figure who lives in the swamp. He knows forbidden knowledge and teaches dark abilities to those who seek them.";
        hermit.personality = NPCPersonality.Mysterious;
        hermit.availability = NPCAvailability.Always;
        hermit.friendliness = 0.2f;
        hermit.homeLocationID = "fog_swamp";
        hermit.greetings = new List<string>
        {
            "You seek power. I can sense it.",
            "The entity offers gifts to those willing to embrace the darkness.",
            "Come closer. Let me show you what the depths can offer."
        };
        hermit.idleDialogue = new List<string>
        {
            "Power has a price. Are you willing to pay?",
            "The entity is not evil. It simply IS. Ancient. Hungry. Eternal.",
            "Your sanity is a cage. Break free."
        };
        hermit.baseRelationship = -20;
        hermit.canMove = false;
        allNPCs.Add(hermit);

        // 8. CHILD - Innocent Quests
        var child = ScriptableObject.CreateInstance<NPCData>();
        child.npcID = "child";
        child.npcName = "Lily";
        child.description = "A young girl who lost her family's locket in the ocean. Represents innocence contrasting with the horror.";
        child.personality = NPCPersonality.Cheerful;
        child.availability = NPCAvailability.DayOnly;
        child.friendliness = 1.0f;
        child.homeLocationID = "town_square";
        child.greetings = new List<string>
        {
            "Hi! Are you a fisher? My daddy was a fisher!",
            "I lost something important in the water. Can you help me find it?",
            "The old people say scary things about the ocean, but I think it's beautiful."
        };
        child.idleDialogue = new List<string>
        {
            "Daddy used to bring me fish every day. I miss him.",
            "Do you have any pretty shells? I collect them!",
            "Mommy says not to go near the water at night. But sometimes I hear singing..."
        };
        child.questsToGive = new List<string> { "fetch_locket" };
        child.baseRelationship = 30;
        allNPCs.Add(child);

        // 9. DRUNK SAILOR - Tall Tales with Truth
        var sailor = ScriptableObject.CreateInstance<NPCData>();
        sailor.npcID = "drunk_sailor";
        sailor.npcName = "One-Eyed Jack";
        sailor.description = "A drunk sailor at the tavern who tells exaggerated stories... that turn out to be disturbingly accurate.";
        sailor.personality = NPCPersonality.Eccentric;
        sailor.availability = NPCAvailability.NightOnly;
        sailor.friendliness = 0.6f;
        sailor.homeLocationID = "tavern";
        sailor.greetings = new List<string>
        {
            "*hic* Sit down, friend! Let me tell you about the REAL monsters in these waters!",
            "You don't believe me? HA! You will. You all do, eventually.",
            "Another drink and I'll tell you about the time I saw the Deep One with my own eye!"
        };
        sailor.idleDialogue = new List<string>
        {
            "They say I'm crazy. But I've SEEN it! Tentacles as thick as tree trunks!",
            "Marcus? Ha! He got greedy. Wanted to control it. Look how that worked out.",
            "The ritual... *hic*... it was supposed to bring prosperity. Brought hell instead."
        };
        sailor.baseRelationship = 0;
        allNPCs.Add(sailor);

        // 10. GHOST FISHER - Low Sanity Only
        var ghost = ScriptableObject.CreateInstance<NPCData>();
        ghost.npcID = "ghost_fisher";
        ghost.npcName = "Marcus (Ghost)";
        ghost.description = "The spectral remnant of the previous fisher. Only appears when your sanity drops to 0. Reveals the truth.";
        ghost.personality = NPCPersonality.Traumatized;
        ghost.availability = NPCAvailability.Always;
        ghost.friendliness = 0.4f;
        ghost.homeLocationID = "underground_cavern";
        ghost.isGhost = true;
        ghost.requiresSanityLevel = 10f; // Only appears below 10 sanity
        ghost.greetings = new List<string>
        {
            "You... you can see me? Then it's already begun for you too.",
            "I tried to warn the others. They didn't listen. Will you?",
            "The entity showed me such wonders... and such horrors."
        };
        ghost.idleDialogue = new List<string>
        {
            "We found the altars. Performed the ritual. Thought we could control it.",
            "The fish were just the beginning. The mutations spread. To the water. To us.",
            "Don't make my mistake. Seal it away before it's too late.",
            "Or... embrace it. Become one with the deep. That's what it wants."
        };
        ghost.questsToGive = new List<string> { "story_ghost_encounter" };
        ghost.baseRelationship = 0;
        ghost.disappearsAfterStoryAct = 5; // Disappears after story ends
        allNPCs.Add(ghost);

        // 11. TAVERN KEEPER - Information Broker
        var tavernKeeper = ScriptableObject.CreateInstance<NPCData>();
        tavernKeeper.npcID = "tavern_keeper";
        tavernKeeper.npcName = "Barkeep Roderick";
        tavernKeeper.description = "The tavern keeper who hears all the gossip and rumors. He can provide valuable information... for a price.";
        tavernKeeper.personality = NPCPersonality.Greedy;
        tavernKeeper.availability = NPCAvailability.Always;
        tavernKeeper.friendliness = 0.5f;
        tavernKeeper.homeLocationID = "tavern";
        tavernKeeper.greetings = new List<string>
        {
            "Welcome to The Drowning Man. What'll it be?",
            "Information? I've got plenty. Question is, can you afford it?",
            "Ah, you're the new fisher. Everyone's talking about you."
        };
        tavernKeeper.idleDialogue = new List<string>
        {
            "I hear things. Secrets. Sometimes I sell them.",
            "The fishers who come back from the deep... they're never quite the same.",
            "Business is good when people need to forget. And everyone here has something to forget."
        };
        tavernKeeper.baseRelationship = 0;
        allNPCs.Add(tavernKeeper);

        // 12. BOAT MECHANIC - Upgrades & Repairs
        var mechanic = ScriptableObject.CreateInstance<NPCData>();
        mechanic.npcID = "mechanic";
        mechanic.npcName = "Rusty Pete";
        mechanic.description = "The boat mechanic who handles all upgrades and repairs. Pragmatic and no-nonsense.";
        mechanic.personality = NPCPersonality.Friendly;
        mechanic.availability = NPCAvailability.DayOnly;
        mechanic.friendliness = 0.7f;
        mechanic.homeLocationID = "workshop";
        mechanic.greetings = new List<string>
        {
            "Need work done on your boat? I'm your man.",
            "I don't ask questions about the dents and scratches. Better that way.",
            "Seen a lot of boats come through here. Yours is still in one piece, so that's good."
        };
        mechanic.idleDialogue = new List<string>
        {
            "Some of the damage I've seen... no natural fish could do that.",
            "Upgrade your hull if you're going out at night. Trust me on that.",
            "Marcus's boat washed up in pieces. Never did figure out what happened to it."
        };
        mechanic.baseRelationship = 10;
        allNPCs.Add(mechanic);

        Debug.Log($"Initialized {allNPCs.Count} NPCs");
    }
}

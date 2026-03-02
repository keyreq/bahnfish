using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Database of all quests in the game.
/// Contains tutorial, story, fetch, side, and repeatable quests.
/// </summary>
[CreateAssetMenu(fileName = "QuestDatabase", menuName = "Bahnfish/Quest Database")]
public class QuestDatabase : ScriptableObject
{
    [Header("Quest Collections")]
    public List<Quest> tutorialQuests = new List<Quest>();
    public List<Quest> storyQuests = new List<Quest>();
    public List<Quest> fetchQuests = new List<Quest>();
    public List<Quest> sideQuests = new List<Quest>();
    public List<Quest> repeatableQuests = new List<Quest>();

    /// <summary>
    /// Get all quests in database
    /// </summary>
    public List<Quest> GetAllQuests()
    {
        List<Quest> allQuests = new List<Quest>();
        allQuests.AddRange(tutorialQuests);
        allQuests.AddRange(storyQuests);
        allQuests.AddRange(fetchQuests);
        allQuests.AddRange(sideQuests);
        allQuests.AddRange(repeatableQuests);
        return allQuests;
    }

    /// <summary>
    /// Get quest by ID
    /// </summary>
    public Quest GetQuestByID(string questID)
    {
        var allQuests = GetAllQuests();
        return allQuests.Find(q => q.questID == questID);
    }

    /// <summary>
    /// Initialize default quests programmatically
    /// </summary>
    public void InitializeDefaultQuests()
    {
        InitializeTutorialQuests();
        InitializeStoryQuests();
        InitializeFetchQuests();
        InitializeSideQuests();
        InitializeRepeatableQuests();
    }

    #region Tutorial Quests

    private void InitializeTutorialQuests()
    {
        tutorialQuests.Clear();

        // Quest 1: First Catch
        tutorialQuests.Add(new Quest
        {
            questID = "tutorial_first_catch",
            title = "First Catch",
            description = "Catch your first fish to get started. Head to the dock and cast your line!",
            questType = QuestType.Tutorial,
            objectives = new List<QuestObjective>
            {
                new QuestObjective
                {
                    objectiveID = "catch_any_fish",
                    description = "Catch any fish",
                    objectiveType = ObjectiveType.CatchAnyFish,
                    targetCount = 1
                }
            },
            reward = new QuestReward
            {
                money = 25f
            },
            autoComplete = true,
            questGiverNPCID = "old_fisher",
            startDialogue = "Welcome, newcomer! Let's see if you can handle a fishing rod. Catch me any fish and I'll show you the ropes.",
            completeDialogue = "Not bad for a first timer! You've got potential."
        });

        // Quest 2: Market Day
        tutorialQuests.Add(new Quest
        {
            questID = "tutorial_market_day",
            title = "Market Day",
            description = "Sell your caught fish at the market to earn money.",
            questType = QuestType.Tutorial,
            objectives = new List<QuestObjective>
            {
                new QuestObjective
                {
                    objectiveID = "sell_fish",
                    description = "Sell fish worth $50",
                    objectiveType = ObjectiveType.SellFish,
                    targetValue = 50f
                }
            },
            prerequisiteQuestIDs = new List<string> { "tutorial_first_catch" },
            reward = new QuestReward
            {
                money = 50f
            },
            autoComplete = true,
            questGiverNPCID = "shopkeeper"
        });

        // Quest 3: Night Watch
        tutorialQuests.Add(new Quest
        {
            questID = "tutorial_night_watch",
            title = "Night Watch",
            description = "Experience fishing at night. Be careful - strange things happen after dark...",
            questType = QuestType.Tutorial,
            objectives = new List<QuestObjective>
            {
                new QuestObjective
                {
                    objectiveID = "survive_night",
                    description = "Survive one night at sea",
                    objectiveType = ObjectiveType.SurviveNight,
                    targetCount = 1
                }
            },
            prerequisiteQuestIDs = new List<string> { "tutorial_market_day" },
            reward = new QuestReward
            {
                money = 100f,
                relics = 1
            },
            autoComplete = true,
            questGiverNPCID = "old_fisher",
            startDialogue = "You've done well during the day, but the night... that's when this place shows its true face. Stay alert."
        });

        // Quest 4: Upgrade Time
        tutorialQuests.Add(new Quest
        {
            questID = "tutorial_upgrade",
            title = "Upgrade Time",
            description = "Purchase your first boat upgrade to improve your capabilities.",
            questType = QuestType.Tutorial,
            objectives = new List<QuestObjective>
            {
                new QuestObjective
                {
                    objectiveID = "buy_upgrade",
                    description = "Purchase any upgrade",
                    objectiveType = ObjectiveType.UpgradeItem,
                    targetCount = 1
                }
            },
            prerequisiteQuestIDs = new List<string> { "tutorial_market_day" },
            reward = new QuestReward
            {
                money = 75f
            },
            autoComplete = true,
            questGiverNPCID = "shopkeeper"
        });

        // Quest 5: New Waters
        tutorialQuests.Add(new Quest
        {
            questID = "tutorial_new_location",
            title = "New Waters",
            description = "Save up and unlock a new fishing location.",
            questType = QuestType.Tutorial,
            objectives = new List<QuestObjective>
            {
                new QuestObjective
                {
                    objectiveID = "unlock_location",
                    description = "Unlock any new location",
                    objectiveType = ObjectiveType.UnlockLocation,
                    targetCount = 1
                }
            },
            prerequisiteQuestIDs = new List<string> { "tutorial_upgrade" },
            reward = new QuestReward
            {
                money = 200f,
                relics = 2
            },
            autoComplete = true,
            questGiverNPCID = "captain",
            startDialogue = "These starter waters won't satisfy you forever. I can grant you access to deeper, more mysterious places... for a price."
        });
    }

    #endregion

    #region Story Quests

    private void InitializeStoryQuests()
    {
        storyQuests.Clear();

        // Story Quest 1: The Missing Fisher
        storyQuests.Add(new Quest
        {
            questID = "story_missing_fisher",
            title = "The Missing Fisher",
            description = "The locals speak of a fisher who vanished months ago. Investigate what happened.",
            questType = QuestType.Story,
            storyProgressionRequired = 1,
            objectives = new List<QuestObjective>
            {
                new QuestObjective
                {
                    objectiveID = "talk_to_npcs",
                    description = "Talk to 3 different NPCs about the missing fisher",
                    objectiveType = ObjectiveType.TalkToNPC,
                    targetCount = 3
                }
            },
            reward = new QuestReward
            {
                money = 150f,
                unlockedJournalEntries = new List<string> { "missing_fisher_clue_1" },
                storyProgressionIncrease = 0
            },
            questGiverNPCID = "old_fisher",
            startDialogue = "You've been asking questions. Good. Someone should know what happened to Marcus. He was a good man... before the waters changed him."
        });

        // Story Quest 2: Messages in Bottles
        storyQuests.Add(new Quest
        {
            questID = "story_messages",
            title = "Messages from the Deep",
            description = "Find messages in bottles scattered across the waters. They contain the previous fisher's logs.",
            questType = QuestType.Story,
            storyProgressionRequired = 1,
            objectives = new List<QuestObjective>
            {
                new QuestObjective
                {
                    objectiveID = "find_messages",
                    description = "Find 5 messages in bottles",
                    objectiveType = ObjectiveType.FindClue,
                    targetID = "message_bottle",
                    targetCount = 5
                }
            },
            reward = new QuestReward
            {
                money = 300f,
                relics = 3,
                storyProgressionIncrease = 1,
                unlockedJournalEntries = new List<string> { "previous_fisher_logs", "the_entity_first_mention" }
            },
            questGiverNPCID = "mystic",
            startDialogue = "The waters hold secrets, written by those who came before. Find their messages. Learn from their mistakes."
        });

        // Story Quest 3: Altar Discovery
        storyQuests.Add(new Quest
        {
            questID = "story_altars",
            title = "The Ancient Altars",
            description = "Discover three ancient altars hidden in different fishing locations.",
            questType = QuestType.Story,
            storyProgressionRequired = 2,
            objectives = new List<QuestObjective>
            {
                new QuestObjective
                {
                    objectiveID = "find_altars",
                    description = "Discover 3 ancient altars",
                    objectiveType = ObjectiveType.FindClue,
                    targetID = "altar",
                    targetCount = 3
                }
            },
            reward = new QuestReward
            {
                relics = 5,
                storyProgressionIncrease = 1,
                unlockedLocations = new List<string> { "underground_cavern" },
                unlockedJournalEntries = new List<string> { "altar_purpose", "ritual_instructions" }
            },
            questGiverNPCID = "scientist",
            startDialogue = "My research indicates three ancient structures exist in these waters. Finding them could unlock the truth about the aberrations."
        });

        // Story Quest 4: The Scientist's Request
        storyQuests.Add(new Quest
        {
            questID = "story_aberrant_samples",
            title = "Aberrant Specimens",
            description = "The scientist needs aberrant fish samples for research.",
            questType = QuestType.Story,
            storyProgressionRequired = 2,
            objectives = new List<QuestObjective>
            {
                new QuestObjective
                {
                    objectiveID = "catch_aberrant",
                    description = "Catch 5 aberrant fish",
                    objectiveType = ObjectiveType.CatchRarity,
                    targetID = "Aberrant",
                    targetCount = 5
                }
            },
            reward = new QuestReward
            {
                money = 500f,
                relics = 4,
                unlockedJournalEntries = new List<string> { "aberrant_analysis", "mutation_causes" }
            },
            questGiverNPCID = "scientist"
        });

        // Story Quest 5: Ghost Stories
        storyQuests.Add(new Quest
        {
            questID = "story_ghost_encounter",
            title = "Echoes of the Past",
            description = "Encounter the ghost of the previous fisher. He only appears when your sanity is low...",
            questType = QuestType.Story,
            storyProgressionRequired = 2,
            objectives = new List<QuestObjective>
            {
                new QuestObjective
                {
                    objectiveID = "meet_ghost",
                    description = "Encounter the ghost fisher",
                    objectiveType = ObjectiveType.TalkToNPC,
                    targetID = "ghost_fisher",
                    targetCount = 1
                }
            },
            reward = new QuestReward
            {
                relics = 5,
                storyProgressionIncrease = 1,
                unlockedJournalEntries = new List<string> { "ghost_warning", "ritual_gone_wrong" }
            },
            isHidden = true,
            startDialogue = "You... you can see me? Then you're already too deep. Listen well - do not repeat our mistakes."
        });

        // Story Quest 6: The Ritual (Final Choice)
        storyQuests.Add(new Quest
        {
            questID = "story_final_ritual",
            title = "The Choice",
            description = "You've learned the truth. Now you must decide: seal the entity, embrace its power, or walk away.",
            questType = QuestType.Story,
            storyProgressionRequired = 4,
            objectives = new List<QuestObjective>
            {
                new QuestObjective
                {
                    objectiveID = "gather_relics",
                    description = "Gather 10 relics for the ritual",
                    objectiveType = ObjectiveType.CollectItem,
                    targetID = "relic",
                    targetCount = 10
                }
            },
            prerequisiteQuestIDs = new List<string> { "story_messages", "story_altars", "story_ghost_encounter" },
            reward = new QuestReward
            {
                storyProgressionIncrease = 1
            },
            questGiverNPCID = "mystic",
            startDialogue = "The time has come. You know what dwells beneath. You have the power to change everything... or nothing. Choose wisely."
        });
    }

    #endregion

    #region Fetch Quests

    private void InitializeFetchQuests()
    {
        fetchQuests.Clear();

        // Fetch Quest 1: Bass Collection
        fetchQuests.Add(new Quest
        {
            questID = "fetch_bass",
            title = "Bass Bonanza",
            description = "The shopkeeper needs bass for the market.",
            questType = QuestType.Fetch,
            objectives = new List<QuestObjective>
            {
                new QuestObjective
                {
                    objectiveID = "catch_bass",
                    description = "Catch 10 bass",
                    objectiveType = ObjectiveType.CatchFish,
                    targetID = "bass",
                    targetCount = 10
                }
            },
            reward = new QuestReward
            {
                money = 200f
            },
            questGiverNPCID = "shopkeeper"
        });

        // Fetch Quest 2: Legendary Fish
        fetchQuests.Add(new Quest
        {
            questID = "fetch_legendary",
            title = "Trophy Catch",
            description = "Catch a legendary fish to prove your skill.",
            questType = QuestType.Fetch,
            objectives = new List<QuestObjective>
            {
                new QuestObjective
                {
                    objectiveID = "catch_legendary",
                    description = "Catch 1 legendary fish",
                    objectiveType = ObjectiveType.CatchRarity,
                    targetID = "Legendary",
                    targetCount = 1
                }
            },
            reward = new QuestReward
            {
                money = 1000f,
                relics = 5
            },
            questGiverNPCID = "captain"
        });

        // Fetch Quest 3: Deep Ocean Retrieval
        fetchQuests.Add(new Quest
        {
            questID = "fetch_locket",
            title = "Lost Locket",
            description = "A child lost their family locket in the deep ocean. Retrieve it.",
            questType = QuestType.Fetch,
            objectives = new List<QuestObjective>
            {
                new QuestObjective
                {
                    objectiveID = "dredge_locket",
                    description = "Dredge up the lost locket from Deep Ocean",
                    objectiveType = ObjectiveType.DredgeItem,
                    targetID = "family_locket",
                    targetCount = 1
                }
            },
            reward = new QuestReward
            {
                money = 150f,
                unlockedJournalEntries = new List<string> { "child_gratitude" }
            },
            questGiverNPCID = "child"
        });
    }

    #endregion

    #region Side Quests

    private void InitializeSideQuests()
    {
        sideQuests.Clear();

        // Side Quest 1: Aquarium Builder
        sideQuests.Add(new Quest
        {
            questID = "side_aquarium",
            title = "Aquarium Collection",
            description = "Catch one of every common fish species for your aquarium.",
            questType = QuestType.Side,
            objectives = new List<QuestObjective>
            {
                new QuestObjective
                {
                    objectiveID = "collect_commons",
                    description = "Catch one of each common fish species (20 species)",
                    objectiveType = ObjectiveType.CatchRarity,
                    targetID = "Common",
                    targetCount = 20
                }
            },
            reward = new QuestReward
            {
                money = 500f,
                unlockedUpgrades = new List<string> { "aquarium_expansion" }
            },
            questGiverNPCID = "scientist"
        });

        // Side Quest 2: Photography Challenge
        sideQuests.Add(new Quest
        {
            questID = "side_photography",
            title = "Rare Sightings",
            description = "Take photos of rare fish species.",
            questType = QuestType.Side,
            objectives = new List<QuestObjective>
            {
                new QuestObjective
                {
                    objectiveID = "photo_rare",
                    description = "Photograph 5 rare fish",
                    objectiveType = ObjectiveType.TakePhoto,
                    targetCount = 5
                }
            },
            reward = new QuestReward
            {
                money = 300f,
                relics = 2
            },
            questGiverNPCID = "scientist"
        });

        // Side Quest 3: Master Fisher
        sideQuests.Add(new Quest
        {
            questID = "side_master_fisher",
            title = "Master Fisher",
            description = "The ultimate challenge - catch all 60 fish species in the game.",
            questType = QuestType.Side,
            objectives = new List<QuestObjective>
            {
                new QuestObjective
                {
                    objectiveID = "catch_all",
                    description = "Catch all 60 fish species",
                    objectiveType = ObjectiveType.CatchAnyFish,
                    targetCount = 60
                }
            },
            reward = new QuestReward
            {
                money = 5000f,
                relics = 20,
                unlockedAbilities = new List<string> { "master_fisher_aura" }
            },
            questGiverNPCID = "captain"
        });
    }

    #endregion

    #region Repeatable Quests

    private void InitializeRepeatableQuests()
    {
        repeatableQuests.Clear();

        // Repeatable Quest 1: Daily Catch
        repeatableQuests.Add(new Quest
        {
            questID = "daily_catch",
            title = "Daily Catch",
            description = "Sell $500 worth of fish to earn a bonus.",
            questType = QuestType.Repeatable,
            isRepeatable = true,
            objectives = new List<QuestObjective>
            {
                new QuestObjective
                {
                    objectiveID = "sell_daily",
                    description = "Sell fish worth $500",
                    objectiveType = ObjectiveType.SellFish,
                    targetValue = 500f
                }
            },
            reward = new QuestReward
            {
                money = 50f
            },
            autoComplete = true,
            questGiverNPCID = "shopkeeper"
        });

        // Repeatable Quest 2: Night Fishing
        repeatableQuests.Add(new Quest
        {
            questID = "night_fishing_repeatable",
            title = "Night Stalker",
            description = "Catch 5 rare fish at night to earn relics.",
            questType = QuestType.Repeatable,
            isRepeatable = true,
            availableTimeOfDay = TimeOfDay.Night,
            objectives = new List<QuestObjective>
            {
                new QuestObjective
                {
                    objectiveID = "catch_rare_night",
                    description = "Catch 5 rare fish at night",
                    objectiveType = ObjectiveType.CatchRarity,
                    targetID = "Rare",
                    targetCount = 5
                }
            },
            reward = new QuestReward
            {
                relics = 1
            },
            autoComplete = true,
            questGiverNPCID = "lighthouse_keeper"
        });

        // Repeatable Quest 3: Exploration
        repeatableQuests.Add(new Quest
        {
            questID = "exploration_repeatable",
            title = "World Traveler",
            description = "Visit all unlocked locations in one day.",
            questType = QuestType.Repeatable,
            isRepeatable = true,
            objectives = new List<QuestObjective>
            {
                new QuestObjective
                {
                    objectiveID = "visit_all",
                    description = "Visit all unlocked locations",
                    objectiveType = ObjectiveType.VisitLocation,
                    targetCount = 13
                }
            },
            reward = new QuestReward
            {
                relics = 2,
                money = 100f
            },
            autoComplete = true,
            questGiverNPCID = "captain"
        });
    }

    #endregion
}

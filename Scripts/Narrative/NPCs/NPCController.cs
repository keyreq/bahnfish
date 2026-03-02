using UnityEngine;

/// <summary>
/// Controls individual NPC behavior, interaction, and quest giving.
/// Implements IInteractable interface for player interaction.
/// </summary>
public class NPCController : MonoBehaviour, IInteractable
{
    [Header("NPC Configuration")]
    [SerializeField] private NPCData npcData;
    [SerializeField] private float interactionRange = 3f;

    [Header("Visual Feedback")]
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private SpriteRenderer npcSprite;

    private bool isPlayerInRange = false;
    private int currentRelationship = 0;

    private void Start()
    {
        if (npcData == null)
        {
            Debug.LogError($"NPCController on {gameObject.name} has no NPCData assigned!");
            return;
        }

        // Hide interaction prompt by default
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);

        // Initialize relationship
        currentRelationship = npcData.baseRelationship;

        // Subscribe to time changes to check availability
        EventSystem.Subscribe<TimeChangedEventData>("TimeChanged", OnTimeChanged);
        EventSystem.Subscribe<float>("SanityChanged", OnSanityChanged);

        // Check initial visibility
        UpdateVisibility();
    }

    private void OnDestroy()
    {
        EventSystem.Unsubscribe<TimeChangedEventData>("TimeChanged", OnTimeChanged);
        EventSystem.Unsubscribe<float>("SanityChanged", OnSanityChanged);
    }

    private void Update()
    {
        // Check for player proximity
        if (npcData != null && GameObject.FindGameObjectWithTag("Player") != null)
        {
            float distance = Vector3.Distance(transform.position, GameObject.FindGameObjectWithTag("Player").transform.position);
            bool wasInRange = isPlayerInRange;
            isPlayerInRange = distance <= interactionRange;

            if (isPlayerInRange && !wasInRange)
            {
                OnPlayerEnterRange();
            }
            else if (!isPlayerInRange && wasInRange)
            {
                OnPlayerExitRange();
            }
        }
    }

    #region IInteractable Implementation

    public void Interact()
    {
        if (!CanInteract())
            return;

        Debug.Log($"Interacting with {npcData.npcName}");

        // Publish interaction event for quest tracking
        EventSystem.Publish("NPCTalkedTo", npcData.npcID);

        // Start dialogue
        StartDialogue();
    }

    public string GetInteractionPrompt()
    {
        if (npcData == null)
            return "Talk";

        // Check if NPC has quests
        bool hasQuest = HasAvailableQuest();
        if (hasQuest)
            return $"[E] {npcData.npcName} (!)";

        return $"[E] {npcData.npcName}";
    }

    public bool CanInteract()
    {
        if (npcData == null)
            return false;

        // Check if player is in range
        if (!isPlayerInRange)
            return false;

        // Check time availability
        GameState gameState = GameManager.Instance?.CurrentGameState;
        if (gameState != null && !npcData.IsAvailableAtTime(gameState.timeOfDay))
            return false;

        return true;
    }

    public float GetInteractionRange()
    {
        return interactionRange;
    }

    private void OnPlayerEnterRange()
    {
        if (interactionPrompt != null && CanInteract())
            interactionPrompt.SetActive(true);

        EventSystem.Publish("PlayerNearNPC", npcData.npcID);
    }

    private void OnPlayerExitRange()
    {
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);

        EventSystem.Publish("PlayerLeftNPC", npcData.npcID);
    }

    #endregion

    #region Dialogue

    private void StartDialogue()
    {
        // Check if this is first meeting
        bool firstMeeting = !HasMetBefore();

        string dialogue = firstMeeting ? npcData.GetRandomGreeting() : npcData.GetRandomIdleDialogue();

        // Create dialogue data and publish event
        var dialogueData = new NPCDialogueData
        {
            npcID = npcData.npcID,
            npcName = npcData.npcName,
            dialogue = dialogue,
            portrait = npcData.portrait,
            personality = npcData.personality,
            isShopkeeper = npcData.isShopkeeper,
            isMystic = npcData.isMystic,
            availableQuests = GetAvailableQuests()
        };

        EventSystem.Publish("NPCDialogueStarted", dialogueData);

        // Mark as met
        MarkAsMet();
    }

    private bool HasMetBefore()
    {
        // TODO: Check save data for NPC relationship
        return false;
    }

    private void MarkAsMet()
    {
        EventSystem.Publish("NPCFirstMet", npcData.npcID);
    }

    #endregion

    #region Quests

    private bool HasAvailableQuest()
    {
        if (npcData.questsToGive == null || npcData.questsToGive.Count == 0)
            return false;

        foreach (var questID in npcData.questsToGive)
        {
            if (!QuestManager.Instance.IsQuestCompleted(questID) &&
                !QuestManager.Instance.IsQuestActive(questID))
            {
                return true;
            }
        }

        return false;
    }

    private string[] GetAvailableQuests()
    {
        if (npcData.questsToGive == null || npcData.questsToGive.Count == 0)
            return new string[0];

        System.Collections.Generic.List<string> availableQuests = new System.Collections.Generic.List<string>();

        foreach (var questID in npcData.questsToGive)
        {
            if (!QuestManager.Instance.IsQuestCompleted(questID) &&
                !QuestManager.Instance.IsQuestActive(questID))
            {
                availableQuests.Add(questID);
            }
        }

        return availableQuests.ToArray();
    }

    #endregion

    #region Visibility & Availability

    private void OnTimeChanged(TimeChangedEventData data)
    {
        UpdateVisibility();
    }

    private void OnSanityChanged(float sanity)
    {
        UpdateVisibility();
    }

    private void UpdateVisibility()
    {
        if (npcData == null)
            return;

        GameState gameState = GameManager.Instance?.CurrentGameState;
        if (gameState == null)
            return;

        // Check time availability
        bool availableAtTime = npcData.IsAvailableAtTime(gameState.timeOfDay);

        // Check sanity requirement (for ghost NPCs)
        bool availableAtSanity = npcData.IsAvailableAtSanity(gameState.sanity);

        // Check story progression
        bool availableInStory = npcData.disappearsAfterStoryAct < 0 ||
                                QuestManager.Instance.GetCurrentStoryAct() <= npcData.disappearsAfterStoryAct;

        bool shouldBeVisible = availableAtTime && availableAtSanity && availableInStory;

        // Update visibility
        gameObject.SetActive(shouldBeVisible);

        // Ghost effect for low-sanity NPCs
        if (npcData.isGhost && npcSprite != null)
        {
            Color color = npcSprite.color;
            color.a = shouldBeVisible ? 0.7f : 0f;
            npcSprite.color = color;
        }
    }

    #endregion

    #region Relationship

    public void ModifyRelationship(int amount)
    {
        currentRelationship = Mathf.Clamp(currentRelationship + amount, -100, 100);
        EventSystem.Publish("NPCRelationshipChanged", new { npcID = npcData.npcID, relationship = currentRelationship });
    }

    public int GetCurrentRelationship()
    {
        return currentRelationship;
    }

    #endregion

    #region Shop

    public void OpenShop()
    {
        if (!npcData.isShopkeeper)
        {
            Debug.LogWarning($"{npcData.npcName} is not a shopkeeper!");
            return;
        }

        EventSystem.Publish("ShopOpened", npcData);
    }

    #endregion

    #region Mystic Services

    public void CleanseCurse()
    {
        if (!npcData.isMystic)
        {
            Debug.LogWarning($"{npcData.npcName} cannot cleanse curses!");
            return;
        }

        EventSystem.Publish("CurseCleanseRequested", npcData.npcID);
    }

    #endregion

    #region Debug

    private void OnDrawGizmosSelected()
    {
        // Draw interaction range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }

    #endregion
}

/// <summary>
/// Data structure for NPC dialogue events
/// </summary>
[System.Serializable]
public struct NPCDialogueData
{
    public string npcID;
    public string npcName;
    public string dialogue;
    public Sprite portrait;
    public NPCPersonality personality;
    public bool isShopkeeper;
    public bool isMystic;
    public string[] availableQuests;
}

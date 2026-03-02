using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages dialogue flow, branching conversations, and NPC interactions.
/// Handles dialogue display, choices, and progression through dialogue trees.
/// </summary>
public class DialogueSystem : MonoBehaviour
{
    private static DialogueSystem _instance;
    public static DialogueSystem Instance => _instance;

    [Header("Dialogue State")]
    private bool isDialogueActive = false;
    private DialogueTree currentTree;
    private DialogueNode currentNode;
    private string currentNPCID;
    private int currentNPCRelationship = 0;

    [Header("Settings")]
    [SerializeField] private float textSpeed = 0.05f; // Time per character
    [SerializeField] private bool allowSkip = true;
    [SerializeField] private bool allowFastForward = true;

    [Header("Audio")]
    [SerializeField] private AudioClip dialogueBlipSound;
    [SerializeField] private AudioClip choiceSelectSound;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Subscribe to NPC dialogue events
        EventSystem.Subscribe<NPCDialogueData>("NPCDialogueStarted", OnNPCDialogueStarted);
    }

    private void OnDestroy()
    {
        EventSystem.Unsubscribe<NPCDialogueData>("NPCDialogueStarted", OnNPCDialogueStarted);
    }

    #region Dialogue Management

    /// <summary>
    /// Start dialogue with an NPC
    /// </summary>
    private void OnNPCDialogueStarted(NPCDialogueData data)
    {
        // For now, just display simple dialogue
        // TODO: Integrate with full dialogue tree system
        DisplaySimpleDialogue(data);
    }

    /// <summary>
    /// Display simple dialogue (non-branching)
    /// </summary>
    private void DisplaySimpleDialogue(NPCDialogueData data)
    {
        currentNPCID = data.npcID;
        isDialogueActive = true;

        // Create UI data and publish
        var uiData = new DialogueUIData
        {
            npcName = data.npcName,
            dialogueText = data.dialogue,
            portrait = data.portrait,
            choices = new string[0],
            canSkip = allowSkip,
            hasShop = data.isShopkeeper,
            hasMystic = data.isMystic,
            availableQuests = data.availableQuests
        };

        EventSystem.Publish("DisplayDialogue", uiData);
    }

    /// <summary>
    /// Start branching dialogue tree
    /// </summary>
    public void StartDialogueTree(DialogueTree tree, string npcID, int npcRelationship = 0)
    {
        if (tree == null)
        {
            Debug.LogError("Cannot start null dialogue tree");
            return;
        }

        if (!tree.Validate())
        {
            Debug.LogError($"Dialogue tree {tree.treeID} failed validation");
            return;
        }

        currentTree = tree;
        currentNPCID = npcID;
        currentNPCRelationship = npcRelationship;
        isDialogueActive = true;

        // Start at root node
        currentNode = tree.GetRootNode();
        DisplayCurrentNode();
    }

    /// <summary>
    /// Display the current dialogue node
    /// </summary>
    private void DisplayCurrentNode()
    {
        if (currentNode == null)
        {
            EndDialogue();
            return;
        }

        // Get available choices
        int currentStoryAct = QuestManager.Instance.GetCurrentStoryAct();
        List<string> completedQuests = QuestManager.Instance.GetCompletedQuestIDs();
        List<DialogueChoice> availableChoices = currentTree.GetAvailableChoices(
            currentNode,
            currentNPCRelationship,
            currentStoryAct,
            completedQuests
        );

        // Convert choices to string array
        string[] choiceTexts = new string[availableChoices.Count];
        for (int i = 0; i < availableChoices.Count; i++)
        {
            choiceTexts[i] = availableChoices[i].choiceText;
        }

        // Create UI data
        var uiData = new DialogueUIData
        {
            npcName = currentNode.speakerName,
            dialogueText = currentNode.dialogueText,
            portrait = currentNode.portrait,
            choices = choiceTexts,
            canSkip = allowSkip,
            hasShop = false,
            hasMystic = false,
            availableQuests = new string[0]
        };

        EventSystem.Publish("DisplayDialogue", uiData);

        // Apply node effects
        ApplyNodeEffects(currentNode);
    }

    /// <summary>
    /// Select a dialogue choice
    /// </summary>
    public void SelectChoice(int choiceIndex)
    {
        if (currentNode == null || currentTree == null)
        {
            Debug.LogWarning("No active dialogue to select choice from");
            return;
        }

        // Get available choices
        int currentStoryAct = QuestManager.Instance.GetCurrentStoryAct();
        List<string> completedQuests = QuestManager.Instance.GetCompletedQuestIDs();
        List<DialogueChoice> availableChoices = currentTree.GetAvailableChoices(
            currentNode,
            currentNPCRelationship,
            currentStoryAct,
            completedQuests
        );

        if (choiceIndex < 0 || choiceIndex >= availableChoices.Count)
        {
            Debug.LogWarning($"Invalid choice index: {choiceIndex}");
            return;
        }

        DialogueChoice selectedChoice = availableChoices[choiceIndex];

        // Play sound effect
        if (choiceSelectSound != null)
        {
            EventSystem.Publish("PlaySoundEffect", choiceSelectSound);
        }

        // Apply choice effects
        if (selectedChoice.relationshipChange != 0)
        {
            currentNPCRelationship += selectedChoice.relationshipChange;
            EventSystem.Publish("NPCRelationshipChanged", new { npcID = currentNPCID, relationship = currentNPCRelationship });
        }

        // Move to next node
        currentNode = currentTree.GetNodeByID(selectedChoice.nextNodeID);

        // Check if dialogue should end
        if (currentNode == null || currentNode.endsDialogue)
        {
            EndDialogue();
        }
        else
        {
            DisplayCurrentNode();
        }
    }

    /// <summary>
    /// Apply effects from current dialogue node
    /// </summary>
    private void ApplyNodeEffects(DialogueNode node)
    {
        // Relationship change
        if (node.relationshipChange != 0)
        {
            currentNPCRelationship += node.relationshipChange;
            EventSystem.Publish("NPCRelationshipChanged", new { npcID = currentNPCID, relationship = currentNPCRelationship });
        }

        // Start quest
        if (!string.IsNullOrEmpty(node.questToStart))
        {
            QuestManager.Instance.StartQuest(node.questToStart);
        }

        // Trigger custom event
        if (!string.IsNullOrEmpty(node.eventToTrigger))
        {
            EventSystem.Publish(node.eventToTrigger);
        }
    }

    /// <summary>
    /// End current dialogue
    /// </summary>
    public void EndDialogue()
    {
        isDialogueActive = false;
        currentTree = null;
        currentNode = null;
        currentNPCID = null;

        EventSystem.Publish("DialogueEnded");
    }

    /// <summary>
    /// Skip current dialogue text (instant display)
    /// </summary>
    public void SkipDialogue()
    {
        if (!allowSkip || !isDialogueActive)
            return;

        EventSystem.Publish("SkipDialogueText");
    }

    /// <summary>
    /// Fast forward dialogue text (faster display)
    /// </summary>
    public void FastForwardDialogue()
    {
        if (!allowFastForward || !isDialogueActive)
            return;

        EventSystem.Publish("FastForwardDialogueText");
    }

    #endregion

    #region Query Methods

    /// <summary>
    /// Check if dialogue is currently active
    /// </summary>
    public bool IsDialogueActive()
    {
        return isDialogueActive;
    }

    /// <summary>
    /// Get current NPC ID
    /// </summary>
    public string GetCurrentNPCID()
    {
        return currentNPCID;
    }

    #endregion

    #region Settings

    /// <summary>
    /// Set text display speed
    /// </summary>
    public void SetTextSpeed(float speed)
    {
        textSpeed = Mathf.Clamp(speed, 0.01f, 0.2f);
    }

    /// <summary>
    /// Get current text speed
    /// </summary>
    public float GetTextSpeed()
    {
        return textSpeed;
    }

    #endregion
}

/// <summary>
/// Data structure for dialogue UI display
/// </summary>
[System.Serializable]
public struct DialogueUIData
{
    public string npcName;
    public string dialogueText;
    public Sprite portrait;
    public string[] choices;
    public bool canSkip;
    public bool hasShop;
    public bool hasMystic;
    public string[] availableQuests;
}

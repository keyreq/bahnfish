using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A node in the dialogue tree representing a single dialogue choice or response
/// </summary>
[System.Serializable]
public class DialogueNode
{
    [Header("Node Identity")]
    public string nodeID;
    public string speakerName;

    [Header("Dialogue Content")]
    [TextArea(3, 6)]
    public string dialogueText;
    public Sprite portrait;

    [Header("Choices")]
    public List<DialogueChoice> choices = new List<DialogueChoice>();

    [Header("Requirements")]
    public int minRelationship = -100; // Minimum relationship to see this node
    public int minStoryAct = 0; // Minimum story progression
    public List<string> requiredQuestIDs = new List<string>(); // Quests that must be completed

    [Header("Effects")]
    public int relationshipChange = 0; // How this dialogue affects relationship
    public bool endsDialogue = false; // If true, dialogue ends after this node
    public string questToStart = ""; // Quest to start after this node
    public string eventToTrigger = ""; // Custom event to trigger

    /// <summary>
    /// Check if this node can be displayed
    /// </summary>
    public bool CanDisplay(int currentRelationship, int currentStoryAct, List<string> completedQuests)
    {
        if (currentRelationship < minRelationship)
            return false;

        if (currentStoryAct < minStoryAct)
            return false;

        foreach (var requiredQuest in requiredQuestIDs)
        {
            if (!completedQuests.Contains(requiredQuest))
                return false;
        }

        return true;
    }
}

/// <summary>
/// A dialogue choice that leads to another node
/// </summary>
[System.Serializable]
public class DialogueChoice
{
    [TextArea(1, 3)]
    public string choiceText;
    public string nextNodeID; // ID of the next dialogue node
    public int relationshipChange = 0; // How this choice affects relationship
    public bool requiresItem = false; // If true, choice requires an item
    public string requiredItemID = ""; // Item required to select this choice
    public int requiredMoney = 0; // Money required to select this choice
}

/// <summary>
/// Complete dialogue tree for branching conversations.
/// Supports choices, requirements, and relationship changes.
/// </summary>
[CreateAssetMenu(fileName = "DialogueTree_", menuName = "Bahnfish/Dialogue Tree")]
public class DialogueTree : ScriptableObject
{
    [Header("Dialogue Configuration")]
    public string treeID;
    public string treeName;
    [TextArea(2, 4)]
    public string description;

    [Header("Dialogue Nodes")]
    public string rootNodeID = "start"; // Starting node
    public List<DialogueNode> nodes = new List<DialogueNode>();

    [Header("Loop Behavior")]
    public bool loopToRoot = false; // If true, dialogue can be restarted
    public string loopNodeID = ""; // Specific node to loop to

    /// <summary>
    /// Get the root/starting node
    /// </summary>
    public DialogueNode GetRootNode()
    {
        return GetNodeByID(rootNodeID);
    }

    /// <summary>
    /// Get a node by its ID
    /// </summary>
    public DialogueNode GetNodeByID(string nodeID)
    {
        foreach (var node in nodes)
        {
            if (node.nodeID == nodeID)
                return node;
        }

        Debug.LogWarning($"Dialogue node not found: {nodeID} in tree {treeID}");
        return null;
    }

    /// <summary>
    /// Get available choices for a node, filtered by requirements
    /// </summary>
    public List<DialogueChoice> GetAvailableChoices(DialogueNode node, int currentRelationship, int currentStoryAct, List<string> completedQuests)
    {
        List<DialogueChoice> availableChoices = new List<DialogueChoice>();

        foreach (var choice in node.choices)
        {
            // Check if next node is accessible
            DialogueNode nextNode = GetNodeByID(choice.nextNodeID);
            if (nextNode != null && nextNode.CanDisplay(currentRelationship, currentStoryAct, completedQuests))
            {
                availableChoices.Add(choice);
            }
        }

        return availableChoices;
    }

    /// <summary>
    /// Validate dialogue tree (checks for broken links)
    /// </summary>
    public bool Validate()
    {
        bool isValid = true;

        // Check root node exists
        if (GetRootNode() == null)
        {
            Debug.LogError($"Dialogue tree {treeID} has invalid root node: {rootNodeID}");
            isValid = false;
        }

        // Check all node references
        foreach (var node in nodes)
        {
            foreach (var choice in node.choices)
            {
                if (GetNodeByID(choice.nextNodeID) == null)
                {
                    Debug.LogError($"Dialogue tree {treeID}: Node {node.nodeID} has invalid choice reference: {choice.nextNodeID}");
                    isValid = false;
                }
            }
        }

        return isValid;
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Quests
{
    public enum QuestState
    {
        Available,
        Active,
        Completed
    }

    // Simplified reward class (same as before)
    [Serializable]
    public class QuestReward
    {
        public string description;
        public RewardType rewardType;
        public int amount;
        public string itemID; // Only needed for item rewards
    }

    public enum RewardType
    {
        Experience,
        Currency,
        Item
    }

    // Condition types for quest completion
    public enum QuestConditionType
    {
        ItemDelivery,   // Deliver item(s) to NPC
        ItemCollection, // Collect a certain number of items
        DialogueChoice, // Make a specific dialogue choice
        LocationVisit   // Visit a specific location
    }

    // Simple condition class for quest completion
    [Serializable]
    public class QuestCondition
    {
        public QuestConditionType conditionType;
        public string targetID;    // NPC ID, location ID, etc.
        public string itemID;      // Required item ID (if applicable)
        public int quantity = 1;   // Required quantity (if applicable)
    }

    // Streamlined quest definition
    [CreateAssetMenu(fileName = "NewQuest", menuName = "Courier/Quests/Quest Definition")]
    public class QuestDefinition : ScriptableObject
    {
        [Header("Basic Info")]
        public string questID;
        public string title;
        [TextArea(3, 10)]
        public string description;
        
        [Header("Requirements")]
        public List<string> requiredQuestIDs = new();
        
        [Header("Completion Condition")]
        public QuestCondition completionCondition;
    
        [Header("Display Objectives")]
        public List<string> objectiveDescriptions = new();
        
        [Header("Rewards")]
        public List<QuestReward> rewards = new();
        
        [Header("Properties")]
        public bool isRepeatable;
    }

    // Simplified quest instance
    [Serializable]
    public class QuestInstance
    {
        public string questID;
        public QuestState state;

        public QuestInstance(QuestDefinition definition)
        {
            questID = definition.questID;
            state = QuestState.Active;
        }
    }
}
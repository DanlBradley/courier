using System;
using System.Collections.Generic;
using System.Linq;
using EditorTools;
using Interfaces;
using Quests;
using UnityEngine;
using Utils;

namespace GameServices
{
    public class QuestService : Service
    {
        // ID Registry
        private GameIDRegistry _registry;
        
        // Dictionary for fast lookup of quest definitions
        private Dictionary<string, QuestDefinition> questDefinitionDict = new();
        
        // Active quest instances
        private Dictionary<string, QuestInstance> activeQuests = new();
        
        // Quest states for tracking
        private HashSet<string> availableQuests = new();
        private HashSet<string> completedQuests = new();
        
        // Event callbacks
        private event Action<string, QuestState, QuestState> OnQuestStateChanged;

        public override void Initialize()
        {
            _registry = GameManager.Instance.GetIdRegistry();
            InitializeQuestLibrary();
            RefreshAvailableQuests();
            Debug.Log("Trying to log quest service");
            Logs.Log("Quest Service Initialized.", "GameServices");
        }
        
        private void InitializeQuestLibrary()
        {
            questDefinitionDict.Clear();
    
            // Get all quest definitions from the registry
            QuestDefinition[] loadedQuests = _registry.GetAllQuestDefinitions();
    
            foreach (var quest in loadedQuests)
            {
                if (quest != null && !string.IsNullOrEmpty(quest.questID))
                {
                    questDefinitionDict[quest.questID] = quest;
                    Debug.Log($"Loaded quest definition: {quest.questID}");
                }
                else
                {
                    Debug.LogWarning("Invalid quest definition found in Resources/Quests");
                }
            }
    
            Debug.Log($"Initialized quest library with {questDefinitionDict.Count} quests");
        }
        
        private void RefreshAvailableQuests()
        {
            availableQuests.Clear();

            foreach (var questID in questDefinitionDict.Keys.Where(CanAcceptQuest))
            {
                Debug.Log($"Adding {questID} to active quests");
                availableQuests.Add(questID);
            }
        }
        
        #region IQuestService Implementation
        
        public bool CanAcceptQuest(string questID)
        {
            // Check if quest is in registry
            if (!_registry.IsValidQuestID(questID))
            {
                // Debug.Log("Cannot accept quest: Quest ID is invalid: " + questID);
                return false;
            }
            
            // Check if quest exists
            if (!questDefinitionDict.TryGetValue(questID, out QuestDefinition questDef))
            {
                // Debug.Log("Cannot accept quest: Failed to get quest from definition dict.");
                return false;
            }
                
            // Check if quest is already active
            if (activeQuests.ContainsKey(questID))
            {
                // Debug.Log("Cannot accept quest: Quest already active.");
                return false;
            }
                
            // Check if quest is completed and not repeatable
            if (completedQuests.Contains(questID) && !questDef.isRepeatable)
            {
                // Debug.Log("Cannot accept quest: Quest is completed and not repeatable.");
                return false;
            }
                
            // Check if all required quests are completed
            foreach (var requiredQuestID in questDef.requiredQuestIDs)
            {
                if (!completedQuests.Contains(requiredQuestID))
                {
                    // Debug.Log($"Cannot accept quest: Required quest not completed: {requiredQuestID}");
                    return false;
                }
            }
            
            // Debug.Log("Can accept quest!");
            return true;
        }
        
        public void AcceptQuest(string questID)
        {
            if (!CanAcceptQuest(questID))
            {
                Debug.LogWarning($"Cannot accept quest: {questID}");
                return;
            }
            
            QuestDefinition questDef = questDefinitionDict[questID];
            QuestInstance quest = new QuestInstance(questDef);
            
            // Add to active quests
            activeQuests[questID] = quest;
            availableQuests.Remove(questID);
            
            // Trigger quest accepted event
            NotifyQuestStateChanged(questID, QuestState.Available, QuestState.Active);
            
            Debug.Log($"Quest accepted: {questID}");
        }
        
        public bool CanCompleteQuest(string questID)
        {
            // Check if quest is active
            if (!activeQuests.TryGetValue(questID, out QuestInstance quest))
            {
                Debug.LogWarning($"Cannot complete quest - not active: {questID}");
                return false;
            }
                
            // Get the quest definition
            if (!questDefinitionDict.TryGetValue(questID, out QuestDefinition questDef))
            {
                Debug.LogWarning($"Cannot complete quest - definition not found: {questID}");
                return false;
            }
            
            // Check completion condition
            QuestCondition condition = questDef.completionCondition;
            
            switch (condition.conditionType)
            {
                case QuestConditionType.ItemDelivery:
                    // Check if player has required item(s) and is near the target NPC
                    bool hasItems = HasItemInInventory(condition.itemID, condition.quantity);
                    bool isNearTarget = IsNearEntity(condition.targetID);
                    return hasItems && isNearTarget;
                    
                case QuestConditionType.ItemCollection:
                    // Check if player has collected required items
                    return HasItemInInventory(condition.itemID, condition.quantity);
                    
                case QuestConditionType.DialogueChoice:
                    // Check if player made the required dialogue choice
                    return HasMadeDialogueChoice(condition.targetID);
                    
                case QuestConditionType.LocationVisit:
                    // Check if player has visited the required location
                    return HasVisitedLocation(condition.targetID);
                    
                default:
                    Debug.LogWarning($"Unknown condition type for quest: {questID}");
                    return false;
            }
        }
        
        public void CompleteQuest(string questID)
        {
            Debug.Log("Attempting to complete quest: " + questID);
            if (!CanCompleteQuest(questID))
            {
                Debug.LogWarning($"Cannot complete quest: {questID}");
                return;
            }
            
            // Get instance and definition
            QuestInstance quest = activeQuests[questID];
            QuestDefinition questDef = questDefinitionDict[questID];
            
            // Handle item delivery (remove items from inventory)
            if (questDef.completionCondition.conditionType == QuestConditionType.ItemDelivery)
            {
                RemoveItemsFromInventory(
                    questDef.completionCondition.itemID, 
                    questDef.completionCondition.quantity
                );
            }
            
            // Update state
            QuestState oldState = quest.state;
            quest.state = QuestState.Completed;
            
            // Grant rewards
            GrantQuestRewards(questDef);
            
            // Move from active to completed
            activeQuests.Remove(questID);
            completedQuests.Add(questID);
            
            // If repeatable, make available again
            if (questDef.isRepeatable)
            {
                availableQuests.Add(questID);
            }
            
            // Notify listeners
            NotifyQuestStateChanged(questID, oldState, QuestState.Completed);
            
            // Refresh available quests (some quests might now be available)
            RefreshAvailableQuests();
            
            Debug.Log($"Quest completed: {questID}");
        }
        
        public bool IsQuestAvailable(string questID)
        {
            return availableQuests.Contains(questID);
        }
        
        public bool IsQuestActive(string questID)
        {
            return activeQuests.ContainsKey(questID);
        }
        
        public bool IsQuestCompleted(string questID)
        {
            return completedQuests.Contains(questID);
        }
        
        public QuestState GetQuestState(string questID)
        {
            if (activeQuests.TryGetValue(questID, out QuestInstance quest))
                return quest.state;
                
            if (completedQuests.Contains(questID))
                return QuestState.Completed;
                
            if (availableQuests.Contains(questID))
                return QuestState.Available;
                
            return QuestState.Available; // Default, since we removed Unavailable
        }
        
        public QuestInstance GetQuestInstance(string questID)
        {
            return activeQuests.GetValueOrDefault(questID);
        }
        
        public QuestDefinition GetQuestDefinition(string questID)
        {
            if (questDefinitionDict.TryGetValue(questID, out QuestDefinition questDef))
                return questDef;
                
            return null;
        }
        
        public List<string> GetAvailableQuestIDs()
        {
            return availableQuests.ToList();
        }
        
        public List<string> GetActiveQuestIDs()
        {
            return activeQuests.Keys.ToList();
        }
        
        public List<string> GetCompletedQuestIDs()
        {
            return completedQuests.ToList();
        }
        
        public void SubscribeToQuestStateChange(Action<string, QuestState, QuestState> callback)
        {
            OnQuestStateChanged += callback;
        }
        
        public void UnsubscribeFromQuestStateChange(Action<string, QuestState, QuestState> callback)
        {
            OnQuestStateChanged -= callback;
        }
        
        #endregion
        
        #region Helper Methods
        
        private void GrantQuestRewards(QuestDefinition questDef)
        {
            // This would integrate with your game's item/inventory/experience systems
            foreach (var reward in questDef.rewards)
            {
                switch (reward.rewardType)
                {
                    case RewardType.Experience:
                        // AddExperienceToPlayer(reward.amount);
                        Debug.Log($"Granting experience: {reward.amount}");
                        break;
                        
                    case RewardType.Currency:
                        // AddCurrencyToPlayer(reward.amount);
                        Debug.Log($"Granting currency: {reward.amount}");
                        break;
                        
                    case RewardType.Item:
                        // AddItemToInventory(reward.itemID, reward.amount);
                        Debug.Log($"Granting item: {reward.itemID} x{reward.amount}");
                        break;
                }
            }
        }
        
        private void NotifyQuestStateChanged(string questID, QuestState oldState, QuestState newState)
        {
            OnQuestStateChanged?.Invoke(questID, oldState, newState);
        }
        
        // These methods would integrate with other systems in your game
        private bool HasItemInInventory(string itemID, int quantity)
        {
            // TODO: Implement integration with inventory system
            Debug.Log($"Checking if player has {quantity} of {itemID}");
            return true; // Default for now
        }
        
        private void RemoveItemsFromInventory(string itemID, int quantity)
        {
            // TODO: Implement integration with inventory system
            Debug.Log($"Removing {quantity} of {itemID} from inventory");
        }
        
        private bool IsNearEntity(string entityID)
        {
            // TODO: Implement integration with NPC/entity proximity system
            Debug.Log($"Checking if player is near entity {entityID}");
            return true; // Default for now
        }
        
        private bool HasMadeDialogueChoice(string choiceID)
        {
            // TODO: Implement integration with dialogue system
            Debug.Log($"Checking if player has made dialogue choice {choiceID}");
            return true; // Default for now
        }
        
        private bool HasVisitedLocation(string locationID)
        {
            // TODO: Implement integration with location/discovery system
            Debug.Log($"Checking if player has visited location {locationID}");
            return true; // Default for now
        }
        
        #endregion
        
        #region Save/Load System Integration
        
        // This would integrate with your game's save/load system
        public void SaveQuestData()
        {
            // Create serializable data structure
            var saveData = new QuestSaveData
            {
                activeQuests = activeQuests,
                completedQuestIDs = completedQuests.ToList()
            };
            
            // In a real implementation, you would serialize this to JSON or another format
            Debug.Log("Quest data saved");
        }
        
        public void LoadQuestData(QuestSaveData saveData)
        {
            if (saveData == null)
                return;
                
            // Clear current state
            activeQuests.Clear();
            completedQuests.Clear();
            
            // Load active quests
            foreach (var pair in saveData.activeQuests)
            {
                activeQuests[pair.Key] = pair.Value;
            }
            
            // Load completed quests
            foreach (var id in saveData.completedQuestIDs)
            {
                completedQuests.Add(id);
            }
            
            // Refresh available quests based on loaded state
            RefreshAvailableQuests();
            
            Debug.Log("Quest data loaded");
        }
        
        [Serializable]
        public class QuestSaveData
        {
            public Dictionary<string, QuestInstance> activeQuests = new Dictionary<string, QuestInstance>();
            public List<string> completedQuestIDs = new List<string>();
        }
        
        #endregion
    }
}
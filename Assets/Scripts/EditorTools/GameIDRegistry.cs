using System.Collections.Generic;
using System.Linq;
using Dialogue;
using Items;
using Quests;
using UnityEngine;

namespace EditorTools
{
    [CreateAssetMenu(fileName = "GameIDRegistry", menuName = "Courier/System/ID Registry")]
    public class GameIDRegistry : ScriptableObject
    {
        // Collections for different ID types
        [SerializeField] private List<string> questIDs = new();
        [SerializeField] private List<string> dialogueIDs = new();
        [SerializeField] private List<string> locationIDs = new();
        [SerializeField] private List<string> itemIDs = new();
        [SerializeField] private List<string> eventIDs = new();
    
        // Public read-only access to ID lists
        public IReadOnlyList<string> QuestIDs => questIDs;
        public IReadOnlyList<string> DialogueIDs => dialogueIDs;
        public IReadOnlyList<string> LocationIDs => locationIDs;
        public IReadOnlyList<string> ItemIDs => itemIDs;
        public IReadOnlyList<string> EventIDs => eventIDs;
    
        private bool isPopulated;
    
        public void RefreshIDsFromResources()
        {
            // Clear existing IDs
            questIDs.Clear();
            dialogueIDs.Clear();
            locationIDs.Clear();
            itemIDs.Clear();
            eventIDs.Clear();
        
            QuestDefinition[] questDefs = Resources.LoadAll<QuestDefinition>("Quests");
            foreach (var quest in questDefs)
            {
                if (string.IsNullOrEmpty(quest.questID)) continue;
                questIDs.Add(quest.questID);
            }
        
            DialogueAsset[] dialogueAssets = Resources.LoadAll<DialogueAsset>("Dialogues");
            foreach (var dialogue in dialogueAssets)
            { if (!string.IsNullOrEmpty(dialogue.dialogueId)) { dialogueIDs.Add(dialogue.dialogueId); } }
        
            ItemDefinition[] items = Resources.LoadAll<ItemDefinition>("Items");
            foreach (var item in items)
            {
                bool idAlreadyInRegistry = false;
                foreach (var itemId in itemIDs.Where(itemId => item.id == itemId))
                {
                    Debug.LogError($"ItemID {itemId} already in registry! All IDs must be unique."); 
                    idAlreadyInRegistry = true;
                }

                if (!string.IsNullOrEmpty(item.id) && !idAlreadyInRegistry) { itemIDs.Add(item.id); }
            }
        
            isPopulated = true;
            Debug.Log($"ID Registry refreshed: {questIDs.Count} quests, " +
                      $"{dialogueIDs.Count} dialogues, {itemIDs.Count} items.");
        }

        private void EnsurePopulated() { if (!isPopulated) { RefreshIDsFromResources(); } }

        public bool IsValidQuestID(string id)
        {
            EnsurePopulated();
            return questIDs.Contains(id);
        }
    
        public bool IsValidDialogueID(string id)
        {
            EnsurePopulated();
            return dialogueIDs.Contains(id);
        }
    
        public bool IsValidLocationID(string id)
        {
            EnsurePopulated();
            return locationIDs.Contains(id);
        }
    
        public bool IsValidItemID(string id)
        {
            EnsurePopulated();
            return itemIDs.Contains(id);
        }
    
        public bool IsValidEventID(string id)
        {
            EnsurePopulated();
            return eventIDs.Contains(id);
        }
        
        // Get all loaded quest definitions
        public QuestDefinition[] GetAllQuestDefinitions()
        {
            EnsurePopulated();
            return Resources.LoadAll<QuestDefinition>("Quests");
        }

        // Get quest definition by ID
        public QuestDefinition GetQuestDefinition(string questID)
        {
            EnsurePopulated();
    
            // Try to find directly first
            QuestDefinition quest = Resources.Load<QuestDefinition>($"Quests/{questID}");
    
            // If not found by direct name, search all quests
            if (quest != null) return quest;
            QuestDefinition[] allQuests = Resources.LoadAll<QuestDefinition>("Quests");
            quest = allQuests.FirstOrDefault(q => q.questID == questID);

            return quest;
        }

        public ItemDefinition GetItemDefinition(string itemID)
        {
            EnsurePopulated();
            ItemDefinition item = Resources.Load<ItemDefinition>($"Items/{itemID}");
            if (item != null) return item;
            ItemDefinition[] allItems = Resources.LoadAll<ItemDefinition>("Items");
            item = allItems.FirstOrDefault(q => q.id == itemID);
            return item;
        }
    }
}
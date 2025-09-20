using System.Collections.Generic;
using GameServices;
using TMPro;
using UnityEngine;

namespace Quests
{
    public class QuestLogUI : MonoBehaviour
    {
        private GameObject questEntryPrefab;
        private GameObject questObjectivePrefab;
        private Transform questListContainer;
        private GameObject questLogPanel;
        
        private QuestService questService;
        private Dictionary<string, GameObject> questEntries = new();
        private bool isInitialized;
        
        private void Start()
        {
            // Get reference to QuestManager via ServiceLocator
            questService = ServiceLocator.GetService<QuestService>();
            
            if (questService == null)
            {
                Debug.LogError("QuestManager not found!");
                return;
            }
            
            // Subscribe to events
            questService.SubscribeToQuestStateChange(OnQuestStateChanged);
            
            isInitialized = true;
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events when destroyed
            questService?.UnsubscribeFromQuestStateChange(OnQuestStateChanged);
        }
        
        // Method for UIManager to set references
        public void SetReferences(GameObject entryPrefab, GameObject objectivePrefab, 
            Transform listContainer, GameObject panel)
        {
            questEntryPrefab = entryPrefab;
            questObjectivePrefab = objectivePrefab;
            questListContainer = listContainer;
            questLogPanel = panel;
        }
        
        public void ShowQuestLog()
        {
            if (!isInitialized) return;
            RefreshQuestLog();
        }
        
        private void RefreshQuestLog()
        {
            ClearQuestEntries();
            List<string> activeQuestIDs = questService.GetActiveQuestIDs();
            foreach (var questID in activeQuestIDs) { CreateQuestEntry(questID); }
        }
        
        private void ClearQuestEntries()
        {
            foreach (var entry in questEntries.Values) { Destroy(entry); }
            questEntries.Clear();
        }
        
        private void CreateQuestEntry(string questID)
        {
            if (questEntryPrefab == null || questListContainer == null)
            {
                Debug.LogError($"Quest entry prefab or container not assigned for {questID}");
                return;
            }
            
            QuestInstance quest = questService.GetQuestInstance(questID);
            QuestDefinition questDef = questService.GetQuestDefinition(questID);

            if (quest == null || questDef == null)
            {
                Debug.LogError($"Quest {questID} is not active or does not have a valid definition.");
                return;
            }
                
            // Instantiate prefab
            GameObject entryObj = Instantiate(questEntryPrefab, questListContainer);
            
            // Set up UI elements - adjust these to match your actual prefab hierarchy
            TMP_Text titleText = entryObj.transform.Find("QuestEntryTitle").
                Find("TitleText").GetComponent<TMP_Text>();
            TMP_Text descriptionText = entryObj.transform.Find("QuestDescription").
                Find("DescriptionText").GetComponent<TMP_Text>();
            Transform objectivesContainer = entryObj.transform.Find("ObjectivesContainer");
            
            if (titleText == null || descriptionText == null || objectivesContainer == null)
            {
                Debug.LogError("Quest entry prefab is missing required child objects!");
                Destroy(entryObj);
                return;
            }
            
            // Set text values
            titleText.text = questDef.title;
            descriptionText.text = questDef.description;
            
            // Set up objectives
            foreach (var objective in questDef.objectiveDescriptions)
            {
                GameObject objectiveEntry = Instantiate(questObjectivePrefab, objectivesContainer);

                TMP_Text objectiveText = objectiveEntry.transform.Find("Text").GetComponent<TMP_Text>();
                
                objectiveText.text = $"{objective}";
            }
            
            // Add to dictionary
            questEntries[questID] = entryObj;
        }
        
        #region Event Handlers
        
        private void OnQuestStateChanged(string questID, QuestState oldState, QuestState newState)
        {
            // Only update UI if the quest log is active
            if (questLogPanel == null || !questLogPanel.activeSelf) return;
            
            // If quest became active, add it
            if (newState == QuestState.Active)
            { CreateQuestEntry(questID); }
            
            else if (oldState == QuestState.Active)
            {
                if (!questEntries.TryGetValue(questID, out GameObject entryObj)) return;
                Destroy(entryObj);
                questEntries.Remove(questID);
            }
        }
        
        #endregion
    }
}
using System.Collections.Generic;
using Dialogue;
using EnvironmentTools;
using GameServices;
using Interfaces;
using Quests;
using UnityEngine;

namespace Character
{
    [RequireComponent(typeof(TimedInteraction))]
    public class NpcDialogueController : MonoBehaviour, ITimedInteractionHandler
    {
        [SerializeField] private NpcDefinition npcDefinition;
        private QuestService questService;
        private DialogueService dialogueService;
        private string currentQuestId;
        private bool canCompleteCurrentQuest;
    
        private void Awake()
        {
            if (npcDefinition == null) { Debug.LogError($"Must include a NPC definition!"); return; }
        
            questService = ServiceLocator.GetService<QuestService>();
            dialogueService = ServiceLocator.GetService<DialogueService>();
            if (dialogueService != null) { dialogueService.OnDialogueStateChanged += HandleDialogueStateChanged; }
        }
        private void OnDestroy()
        {
            if (dialogueService != null) { dialogueService.OnDialogueStateChanged -= HandleDialogueStateChanged; }
        }
    
        private void HandleDialogueStateChanged()
        {
            if (!canCompleteCurrentQuest || string.IsNullOrEmpty(currentQuestId)) return;
        
            bool dialogueEnded = string.IsNullOrEmpty(dialogueService.GetCurrentDialogueText()) && 
                                 !dialogueService.HasChoices();
        
            if (!dialogueEnded || !questService.CanCompleteQuest(currentQuestId)) return;
            questService.CompleteQuest(currentQuestId);
            
            canCompleteCurrentQuest = false;
            currentQuestId = null;
            
            Debug.Log($"Completed quest: {currentQuestId}");
        }
    
        private string FindCompletableQuestID()
        {
            if (questService == null) return null;
        
            // Check active quests to see if any can be completed with this NPC
            foreach (string questID in questService.GetActiveQuestIDs())
            {
                QuestDefinition quest = questService.GetQuestDefinition(questID);
                if (quest == null) continue;
            
                // Check if this NPC is part of the quest completion and the conditions are met
                if (IsNpcInvolvedInQuest(quest) && questService.CanCompleteQuest(questID))
                {
                    return questID;
                }
            }
        
            return null;
        }
    
        private bool IsNpcInvolvedInQuest(QuestDefinition quest)
        {
            return quest.completionCondition.conditionType == QuestConditionType.ItemDelivery &&
                   quest.completionCondition.targetID == npcDefinition.npcID;
        }

        public void OnInteractionComplete(GameObject interactor)
        {
            if (npcDefinition == null || questService == null || dialogueService == null) return;
        
            List<string> activeQuests = questService.GetActiveQuestIDs();
            List<string> completedQuests = questService.GetCompletedQuestIDs();
            List<string> availableQuests = questService.GetAvailableQuestIDs();
        
            // Check if there's a quest that can be completed with this NPC
            string completableQuestID = FindCompletableQuestID();
            currentQuestId = completableQuestID;
            canCompleteCurrentQuest = !string.IsNullOrEmpty(completableQuestID);
        
            // Get the appropriate dialogue based on quest state
            DialogueAsset dialogueToUse = npcDefinition.GetAppropriateDialogue(
                activeQuests, 
                completedQuests, 
                availableQuests,
                questService
            );
        
            if (dialogueToUse != null) { dialogueService.StartStory(dialogueToUse, npcDefinition); }
            else { Debug.LogWarning($"No dialogue asset found for NPC: {npcDefinition.displayName}"); }
        }

        public bool CanPerformInteraction(GameObject interactor)
        {
            return true;
        }
    }
}
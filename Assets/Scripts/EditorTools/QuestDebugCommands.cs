using GameServices;
using Interfaces;
using UnityEngine;

namespace EditorTools
{
    public class QuestDebugCommands : MonoBehaviour
    {
        private QuestService questService;
    
        private void Start()
        {
            // Get quest manager
            questService = ServiceLocator.GetService<QuestService>();
        
            if (questService == null)
            {
                Debug.LogError("QuestManager not found!");
                return;
            }
        
            // Register quest commands
            DebugConsole.RegisterCommand("quest.list", ListQuests, 
                "List all quests in the game", "quest.list [available/active/completed]");
            
            DebugConsole.RegisterCommand("quest.accept", AcceptQuest, 
                "Accept a quest by ID", "quest.accept <questID>");
            
            DebugConsole.RegisterCommand("quest.complete", CompleteQuest, 
                "Force complete a quest by ID", "quest.complete <questID>");
        }
    
        #region Command Handlers
    
        private string ListQuests(string[] args)
        {
            string filter = args.Length > 0 ? args[0].ToLower() : "all";
        
            string result = "";
        
            if (filter is "all" or "available")
            {
                var availableQuestIDs = questService.GetAvailableQuestIDs();
                result += "AVAILABLE QUESTS:\n";
            
                if (availableQuestIDs.Count == 0)
                {
                    result += "  None\n";
                }
                else
                {
                    foreach (var id in availableQuestIDs)
                    {
                        var def = questService.GetQuestDefinition(id);
                        result += $"  - {id}: {def.title}\n";
                    }
                }
            }
        
            if (filter is "all" or "active")
            {
                var activeQuestIDs = questService.GetActiveQuestIDs();
                result += "\nACTIVE QUESTS:\n";
            
                if (activeQuestIDs.Count == 0)
                {
                    result += "  None\n";
                }
                else
                {
                    foreach (var id in activeQuestIDs)
                    {
                        var def = questService.GetQuestDefinition(id);
                        result += $"  - {id}: {def.title}\n";
                    }
                }
            }

            if (filter is not ("all" or "completed")) return result;
            {
                var completedQuestIDs = questService.GetCompletedQuestIDs();
                result += "\nCOMPLETED QUESTS:\n";
            
                if (completedQuestIDs.Count == 0)
                {
                    result += "  None\n";
                }
                else
                {
                    foreach (var id in completedQuestIDs)
                    {
                        var def = questService.GetQuestDefinition(id);
                        result += $"  - {id}: {def.title}\n";
                    }
                }
            }

            return result;
        }
    
        private string AcceptQuest(string[] args)
        {
            if (args.Length < 1)
                return "Usage: quest.accept <questID>";
            
            string questID = args[0];
            
            // Check if we can accept the quest
            if (!questService.CanAcceptQuest(questID))
                return $"Failed to accept quest '{questID}'. Check requirements or if already accepted.";
                
            // Accept the quest (passing null for extraParams)
            questService.AcceptQuest(questID);
            return $"Quest '{questID}' accepted!";
        }
    
        private string CompleteQuest(string[] args)
        {
            if (args.Length < 1)
                return "Usage: quest.complete <questID>";
            
            string questID = args[0];
            var activeQuest = questService.GetQuestInstance(questID);
        
            if (activeQuest == null) return $"Quest '{questID}' is not active. Accept it first.";
        
            // Check if we can complete the quest now
            if (!questService.CanCompleteQuest(questID))
                return $"Failed to complete quest '{questID}'. There might be some issue with completion criteria.";
                
            // Complete the quest
            questService.CompleteQuest(questID);
            return $"Quest '{questID}' completed!";
        }
    
        #endregion
    }
}
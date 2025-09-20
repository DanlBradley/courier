using System;
using System.Collections.Generic;
using Dialogue;
using GameServices;
using Quests;
using UnityEngine;

namespace Character
{
    [Serializable]
    public class QuestDialogueMapping
    {
        public string questID;
        public QuestState requiredState;
        public bool canComplete; // Whether this dialogue should be used when the quest can be completed
        public DialogueAsset dialogueAsset;
    }

    [CreateAssetMenu(fileName = "NewNPC", menuName = "Courier/Character/NPC Definition")]
    public class NpcDefinition : ScriptableObject
    {
        [Header("Basic Info")]
        public string npcID;          // Unique identifier for the NPC
        public string displayName;    // Name shown in dialogue and UI
        [TextArea(3, 8)]
        public string description;    // Brief description of the NPC
        
        [Header("Appearance")]
        public Sprite portrait;       // Character portrait for dialogue UI

        [Header("Base Dialogue")]
        public DialogueAsset defaultGreeting;  // Default dialogue when no special conditions are met
        public List<DialogueAsset> greetingVariations = new(); // Multiple greetings for variety

        [Header("Quest-Specific Dialogue")]
        [Tooltip("Dialogue assets to use based on quest state")]
        public List<QuestDialogueMapping> questDialogues = new();

        [Header("Trading")]
        public bool isVendor;             // Whether this NPC can trade with the player
        public string shopID;             // Reference to shop inventory if this is a vendor

        /// <summary>
        /// Gets the appropriate dialogue asset based on quest state
        /// </summary>
        /// <param name="activeQuests">Currently active quest IDs</param>
        /// <param name="completedQuests">Completed quest IDs</param>
        /// <param name="availableQuests">Available quest IDs</param>
        /// <param name="questService">Reference to quest manager for checking completion possibility</param>
        /// <returns>The most appropriate dialogue asset to use</returns>
        public DialogueAsset GetAppropriateDialogue(
            List<string> activeQuests, 
            List<string> completedQuests,
            List<string> availableQuests,
            QuestService questService)
        {
            if (questService == null || questDialogues.Count == 0)
            {
                return GetDefaultDialogue();
            }

            // First priority: Check for quests that can be completed
            foreach (var mapping in questDialogues)
            {
                if (mapping.requiredState == QuestState.Active &&
                    mapping.canComplete && 
                    activeQuests.Contains(mapping.questID) && 
                    questService.CanCompleteQuest(mapping.questID))
                {
                    return mapping.dialogueAsset;
                }
            }

            // Second priority: Check for active quests
            foreach (var mapping in questDialogues)
            {
                if (mapping.requiredState == QuestState.Active && 
                    activeQuests.Contains(mapping.questID))
                {
                    return mapping.dialogueAsset;
                }
            }

            // Third priority: Check for completed quests
            foreach (var mapping in questDialogues)
            {
                if (mapping.requiredState == QuestState.Completed && 
                    completedQuests.Contains(mapping.questID))
                {
                    return mapping.dialogueAsset;
                }
            }
            
            // Fourth priority: Check for available quests
            foreach (var mapping in questDialogues)
            {
                if (mapping.requiredState == QuestState.Available && 
                    availableQuests.Contains(mapping.questID))
                { return mapping.dialogueAsset; }
            }

            // Fallback to default dialogue
            return GetDefaultDialogue();
        }

        /// <summary>
        /// Returns the default dialogue, possibly selecting a random variation
        /// </summary>
        private DialogueAsset GetDefaultDialogue()
        {
            if (greetingVariations.Count > 0 && UnityEngine.Random.value > 0.5f)
            {
                int randomIndex = UnityEngine.Random.Range(0, greetingVariations.Count);
                return greetingVariations[randomIndex];
            }
            
            return defaultGreeting;
        }
    }
}
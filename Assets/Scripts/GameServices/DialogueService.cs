using System;
using Character;
using Dialogue;
using Ink.Runtime;
using Interfaces;
using UnityEngine;
using Utils;

namespace GameServices
{
    public class DialogueService : Service
    {
        private Story story;
        private string currentDialogueId;
        private Sprite defaultPortrait;
        private string defaultSpeakerName;
        private DialogueActionRegistry actionRegistry;
        public event Action OnDialogueStateChanged;
        
        public override void Initialize()
        {
            if (actionRegistry == null) { actionRegistry = GetComponent<DialogueActionRegistry>(); }
            Logs.Log("Dialogue service initialized.", "GameServices");
        }

        public bool StartStory(DialogueAsset dialogueAsset, NpcDefinition npcDefinition)
        {
            currentDialogueId = dialogueAsset.dialogueId;
            defaultPortrait = npcDefinition.portrait;
            defaultSpeakerName = npcDefinition.name;
            story = new Story(dialogueAsset.inkAsset.text);
            ServiceLocator.GetService<UIService>().ToggleDialogueView();
            GameStateManager.Instance.ChangeState(GameState.Dialogue);
            ContinueStory();
            return true;
        }
        
        public void ContinueDialogue() { if (story.currentChoices.Count > 0) return; ContinueStory(); }
        
        public void SelectChoice(int choiceIndex)
        {
            if (story == null || choiceIndex < 0 || choiceIndex >= story.currentChoices.Count)
            { Debug.LogWarning($"Invalid choice index: {choiceIndex}"); return; }
            
            string choiceText = story.currentChoices[choiceIndex].text.Trim();
            story.ChooseChoiceIndex(choiceIndex);
            actionRegistry.ProcessChoice(currentDialogueId, choiceText);
            ContinueStory();
        }
        
        private void ContinueStory()
        {
            if (story.canContinue) { story.Continue(); OnDialogueStateChanged?.Invoke(); }
            else { EndDialogue(); }
        }
        
        private void EndDialogue()
        {
            Debug.Log("No more choices, closing dialogue");
            ServiceLocator.GetService<UIService>().ToggleDialogueView();
            GameStateManager.Instance.ChangeState(GameState.Exploration);
        }

        #region Interface Helpers
        public bool HasChoices() { return story != null && story.currentChoices.Count > 0; }
        public int GetChoiceCount() { return story?.currentChoices.Count ?? 0; }
        public string GetCurrentDialogueText() { return story?.currentText?.Trim() ?? string.Empty; }
        public string[] GetCurrentChoices()
        {
            if (story == null || story.currentChoices.Count == 0)
                return Array.Empty<string>();
                
            string[] choices = new string[story.currentChoices.Count];
            for (int i = 0; i < story.currentChoices.Count; i++)
            {
                choices[i] = story.currentChoices[i].text.Trim();
            }
            return choices;
        }
        
        public Sprite GetCurrentPortrait() { return defaultPortrait; }
        public string GetCurrentSpeakerName() { return defaultSpeakerName; }
        public string GetCurrentDialogueId() { return currentDialogueId; }
        #endregion
    }
}
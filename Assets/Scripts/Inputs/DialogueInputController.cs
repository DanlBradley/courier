using GameServices;
using UnityEngine;

namespace Inputs
{
    public class DialogueInputController : InputConsumer
    {
        private DialogueService dialogueService;
        private InputManager inputManager;
        
        private void Start()
        {
            // Get dialogue manager from service locator
            dialogueService = ServiceLocator.GetService<DialogueService>();
            inputManager = ServiceLocator.GetService<InputManager>();
            if (dialogueService != null) return;
            Debug.LogError("DialogueInputController: DialogueService not found in ServiceLocator");
            if (inputManager != null) return;
            Debug.LogError("DialogueInputController: InputService not found in ServiceLocator");
            enabled = false;
        }
        
        protected override void SubscribeToEvents()
        {
            
            inputManager.OnDialogueChoiceInput += HandleDialogueChoice;
            inputManager.OnContinueDialogueInput += HandleContinueDialogue;
        }
        
        protected override void UnsubscribeFromEvents()
        {
            inputManager.OnDialogueChoiceInput -= HandleDialogueChoice;
            inputManager.OnContinueDialogueInput -= HandleContinueDialogue;
        }
        
        private void HandleDialogueChoice(int choiceIndex)
        {
            if (!ShouldProcessInput(GameState.Dialogue)) return;
            
            // Make sure the choice index is valid
            if (dialogueService.HasChoices() 
                && choiceIndex >= 0 
                && choiceIndex < dialogueService.GetChoiceCount())
            {
                dialogueService.SelectChoice(choiceIndex);
            }
            else
            {
                Debug.LogWarning($"Invalid choice index {choiceIndex}. " +
                                 $"Available choices: {dialogueService.GetChoiceCount()}");
            }
        }
        
        private void HandleContinueDialogue()
        {
            if (!ShouldProcessInput(GameState.Dialogue)) return;
            if (dialogueService.HasChoices()) return;
            dialogueService.ContinueDialogue();
        }
    }
}
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using GameServices;

namespace Dialogue
{
    public class DialogueViewController : MonoBehaviour
    {
        [SerializeField] private TMP_Text dialogueText;
        [SerializeField] private Button choicePrefab;
        [SerializeField] private Transform choicePanel;
        [SerializeField] private Image portraitPf;
        [SerializeField] private TMP_Text speakerName;
        [SerializeField] private Button continueBtn;
        
        private DialogueService dialogueService;
        
        private void Awake()
        { continueBtn.onClick.AddListener(OnContinueButtonClicked); }
        
        private void OnEnable()
        {
            // Get dialogue manager reference when view is enabled
            dialogueService = ServiceLocator.GetService<DialogueService>();
            if (dialogueService == null)
            {
                Debug.LogError("DialogueViewController: IDialogueManager not found in ServiceLocator");
                return;
            }
            
            // Subscribe to dialogue state changes
            dialogueService.OnDialogueStateChanged += UpdateView;
            
            // Initial view update
            UpdateView();
        }
        
        private void OnDisable()
        {
            // Unsubscribe when view is disabled
            if (dialogueService != null)
            {
                dialogueService.OnDialogueStateChanged -= UpdateView;
            }
        }
        
        private void UpdateView()
        {
            if (dialogueService == null) return;
            
            // Update dialogue text
            dialogueText.text = dialogueService.GetCurrentDialogueText();
            
            // Update portrait and speaker name
            portraitPf.sprite = dialogueService.GetCurrentPortrait();
            speakerName.text = dialogueService.GetCurrentSpeakerName();
            
            // Update choice buttons
            ResetDialogue();
            string[] choices = dialogueService.GetCurrentChoices();
            for (int i = 0; i < choices.Length; i++)
            {
                int choiceIndex = i; // Capture for lambda
                Button button = AddChoice(choices[i]);
                button.onClick.AddListener(() => OnChoiceButtonClicked(choiceIndex));
            }
            
            // Show/hide continue button based on whether there are choices
            continueBtn.gameObject.SetActive(!dialogueService.HasChoices());
        }
        
        private void OnContinueButtonClicked() { dialogueService?.ContinueDialogue(); }
        
        private void OnChoiceButtonClicked(int choiceIndex) { dialogueService?.SelectChoice(choiceIndex); }
        
        // UI utility methods
        private void ResetDialogue() { DestroyChildren(choicePanel); }
        
        private Button AddChoice(string choiceText)
        {
            Button button = Instantiate(choicePrefab, choicePanel);
            button.transform.GetChild(0).GetComponent<TMP_Text>().text = choiceText;
            return button;
        }
        
        private static void DestroyChildren(Transform transformToClear)
        {
            int childCount = transformToClear.childCount;
            for (int i = childCount - 1; i >= 0; --i)
            {
                Destroy(transformToClear.GetChild(i).gameObject);
            }
        }
    }
}
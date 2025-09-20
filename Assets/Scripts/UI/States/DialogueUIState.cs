using GameServices;
using UnityEngine;

namespace UI.States
{
    public class DialogueUIState: UIState
    {
        public override string StateName => "Dialogue";
        private GameObject dialoguePanel;

        public override void Initialize(UIService service)
        {
            base.Initialize(service);
            dialoguePanel = service.GetDialoguePanel();
        }

        public override void OnEnter() { dialoguePanel.SetActive(true); }

        public override void OnExit() { dialoguePanel.SetActive(false); }
    }
}
using GameServices;
using UnityEngine;

namespace UI.States
{
    public class PauseMenuUIState : UIState
    {
        public override string StateName { get; }
        private GameObject pauseMenuPanel;

        public override void OnEnter()
        {
            Debug.Log("Opened the pause menu.");
            pauseMenuPanel = uiService.GetPauseMenuPanel().gameObject;
            pauseMenuPanel.SetActive(true);
            GameStateManager.Instance.ChangeState(GameState.UI);
        }

        public override void OnExit()
        {
            Debug.Log("Closed the pause menu.");
            pauseMenuPanel.SetActive(false);
        }
        
        public override bool HandleInput(string inputAction)
        {
            switch (inputAction)
            {
                case "Cancel":
                    var uiStateManager = uiService.uiStateManager;
                    uiStateManager.TransitionToState<ExplorationUIState>();
                    return true;
                default: return false;
            }
        }
    }
}
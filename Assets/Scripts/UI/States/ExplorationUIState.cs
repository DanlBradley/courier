using GameServices;
using UnityEngine;
using Utils;

namespace UI.States
{
    public class ExplorationUIState : UIState
    {
        public override string StateName => "Exploration";

        public override void OnEnter()
        {
            Logs.Log("Entering exploration UI state, switching to game state exploration...");
            GameStateManager.Instance.ChangeState(GameState.Exploration);
            Logs.Log("Entered exploration state." + GameStateManager.Instance.CurrentState);
        }

        public override bool HandleInput(string inputAction)
        {
            switch (inputAction)
            {
                case "ToggleInventory":
                    // Transition to inventory state instead of just toggling
                    var uiStateManager = uiService.GetComponent<UIStateManager>();
                    uiStateManager.TransitionToState<CharacterScreenUIState>();
                    return true;

                case "ToggleQuestLog":
                    var questOverlay = UIOverlayFlags.QuestLog;
                    uiService.GetComponent<UIStateManager>().ToggleOverlay(questOverlay);
                    return true;

                default:
                    return false;
            }
        }
    }
}
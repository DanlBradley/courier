using GameServices;
using UnityEngine;

namespace UI.States
{
    public class WorldMapMenuState : UIState
    {
        public override string StateName { get; }
        private GameObject worldMapMenuPanel;

        public override void OnEnter()
        {
            Debug.Log("Opened the world map menu.");
            worldMapMenuPanel = uiService.GetWorldMapMenuPanel().gameObject;
            worldMapMenuPanel.SetActive(true);
            GameStateManager.Instance.ChangeState(GameState.UI);
        }

        public override void OnExit()
        {
            Debug.Log("Closed the world map menu.");
            worldMapMenuPanel.SetActive(false);
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
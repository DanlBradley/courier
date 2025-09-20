using System.Collections.Generic;
using EnvironmentTools;
using GameServices;
using Interfaces;
using Items;
using UnityEngine;

namespace UI.States
{
    public class BackpackConfigurationUIState : UIState, IParameterizedState
    {
        public override string StateName => "BackpackConfiguration";
        
        private BackpackFrame currentFrame;
        private ContainerItem moduleStorage;
        private BackpackWorkbenchHandler workbench;
        
        // UI References
        private GameObject configPanel;
        private Transform frameGridContainer;
        private Transform moduleStorageContainer;
        private InventoryUI frameGridUI;
        private InventoryUI storageUI;
        
        public override void Initialize(UIService service)
        {
            base.Initialize(service);
            configPanel = uiService.GetBackpackConfigPanel();
            
            if (configPanel != null)
            {
                frameGridContainer = configPanel.transform.Find("FrameGrid");
                moduleStorageContainer = configPanel.transform.Find("ModuleStorage");
                
                if (frameGridContainer == null || moduleStorageContainer == null)
                {
                    Debug.LogError("BackpackConfigurationUIState: Missing UI containers in config panel!");
                }
            }
        }
        
        public void SetParameters(object parameters)
        {
            if (parameters is not BackpackConfigParams configParams) return;
            currentFrame = configParams.frame;
            moduleStorage = configParams.moduleStorage;
            workbench = configParams.workbench;
        }
        
        public override void OnEnter()
        {
            if (currentFrame == null || moduleStorage == null)
            {
                Debug.LogError("BackpackConfigurationUIState: Missing required parameters!");
                uiService.uiStateManager.TransitionToState<ExplorationUIState>();
                return;
            }
            
            GameStateManager.Instance.ChangeState(GameState.UI);
            ShowConfigurationPanel();
        }
        
        public override void OnExit() { HideConfigurationPanel(); }
        
        private void ShowConfigurationPanel()
        {
            if (configPanel == null) return;
            configPanel.SetActive(true);
            
            ClearExistingUI();
            
            // Create UI for the frame's module grid
            var uiPrefab = uiService.GetInventoryUIPrefab();
            
            if (frameGridContainer != null && uiPrefab != null)
            {
                // Create frame grid UI
                var frameGridInstance = Object.Instantiate(uiPrefab, frameGridContainer);
                frameGridUI = frameGridInstance.GetComponent<InventoryUI>();
                
                // Create a temporary container wrapper for the frame's module grid
                var frameContainerWrapper = CreateFrameContainerWrapper(currentFrame);
                frameGridUI.OpenContainer(frameContainerWrapper);
            }

            if (moduleStorageContainer != null && uiPrefab != null)
            {
                // Create module storage UI
                var storageInstance = Object.Instantiate(uiPrefab, moduleStorageContainer);
                storageUI = storageInstance.GetComponent<InventoryUI>();
                storageUI.OpenContainer(moduleStorage);
            }
        }
        
        private void HideConfigurationPanel()
        {
            ClearExistingUI();
            if (configPanel != null) configPanel.SetActive(false);
        }
        
        private void ClearExistingUI()
        {
            if (frameGridUI != null)
            {
                frameGridUI.CloseContainer();
                Object.Destroy(frameGridUI.gameObject);
                frameGridUI = null;
            }

            if (storageUI == null) return;
            storageUI.CloseContainer();
            Object.Destroy(storageUI.gameObject);
            storageUI = null;
        }
        
        private ContainerItem CreateFrameContainerWrapper(BackpackFrame frame)
        {
            // Create a dummy definition that matches the frame grid
            var wrapperDef = ScriptableObject.CreateInstance<ContainerDefinition>();
            wrapperDef.containerSize = frame.definition.containerSize;
            wrapperDef.name = "Frame Module Grid";
            
            // Create wrapper that uses the frame's module grid
            var wrapper = new FrameGridContainerWrapper(wrapperDef, frame);
            return wrapper;
        }
        
        public override bool HandleInput(string inputAction)
        {
            switch (inputAction)
            {
                case "Cancel":
                case "ToggleInventory":
                    uiService.uiStateManager.TransitionToState<ExplorationUIState>();
                    return true;
                default:
                    return false;
            }
        }
    }
    
    public class BackpackConfigParams
    {
        public BackpackFrame frame { get; set; }
        public ContainerItem moduleStorage { get; set; }
        public BackpackWorkbenchHandler workbench { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Character.Player;
using GameServices;
using Interfaces;
using Items;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI.States
{
    /// <summary>
    /// The character screen is a comprehensive UI state that contains tabs for multiple interfaces:
    /// 1. Character inventory / equipment (hot key "I")
    /// 2. Character status and RPG stats (hot key "C")
    /// 3. Quest log and quest state info (hot key "J")
    /// 4. Other stuff
    ///
    /// To navigate between them, the player can either press the associated hot key,
    /// or click on the corresponding tab in the character screen menu.
    /// </summary>
    public class CharacterScreenUIState : UIState, IParameterizedState
    {
        public override string StateName => "CharacterScreen";
        
        //default tab
        public CharacterTab currentTab { get; private set; } = CharacterTab.Inventory;
        private GameObject characterScreenPanel;
        private List<ContainerItem> playerContainers;
        private List<InventoryUI> inventoryUIs = new();
        private GameObject inventoryTab;
        private GameObject questLogTab;
        private GameObject currentFrameLayout;
        
        //Loot info
        private Transform lootPanel;
        private IContainerOwner lootOwner;
        private List<ContainerItem> lootContainers;
        private List<InventoryUI> lootInventoryUIs = new();
        
        public override void Initialize(UIService service)
        {
            base.Initialize(service);

            

            characterScreenPanel = uiService.GetCharacterScreenPanel();
            inventoryTab = uiService.GetInventoryTab();
            questLogTab = uiService.GetQuestTab();
            
            //Initialize loot panel
            lootPanel = uiService.GetLootViewPanel().transform;
            Debug.Log("Character screen UI state initialized");
        }
        
        public override void OnEnter()
        {
            //get vars
            playerContainers = ServiceLocator.GetService<InventoryService>().GetContainersOwnedBy(
                GameManager.Instance.GetPlayer().GetComponent<PlayerInventory>());
            Logs.Log("UI state: Player containers: " + playerContainers);
            //Handle UI
            SwitchToTab(currentTab);
            GameStateManager.Instance.ChangeState(GameState.UI);
        }

        public override void OnExit() { HideCharacterScreen(); lootOwner = null; }
        
        private void SwitchToTab(CharacterTab tab)
        {
            if (characterScreenPanel != null) characterScreenPanel.SetActive(true);
    
            HideAllTabs();
            
            // Debug.Log($"Showing character screen - {tab} tab");
            switch (tab)
            {
                case CharacterTab.Inventory:
                    ShowInventoryPanel(); 
                    break;
                case CharacterTab.Status: break;
                case CharacterTab.QuestLog: ShowQuestLogPanel(); break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(tab), tab, null);
            }
        }
        
        private void HideCharacterScreen()
        {
            // Debug.Log("Character Screen State - Hiding character screen...");
            if (characterScreenPanel != null) characterScreenPanel.SetActive(false);
    
            switch (currentTab)
            {
                case CharacterTab.Inventory: HideInventoryPanel(); break;
                case CharacterTab.QuestLog: HideQuestLogPanel(); break;
                case CharacterTab.Status: break;
                default: throw new ArgumentOutOfRangeException();
            }
        }
        
        public override bool HandleInput(string inputAction)
        {
            // Debug.Log("Character Screen UI State - Handling input for: " + inputAction);
            switch (inputAction)
            {
                case "ToggleInventory":
                    SwitchToTab(CharacterTab.Inventory);
                    return true;
                    
                case "ToggleQuestLog":
                    SwitchToTab(CharacterTab.QuestLog);
                    return true;
                    
                case "Cancel":
                    var uiStateManager = uiService.uiStateManager;
                    uiStateManager.TransitionToState<ExplorationUIState>();
                    return true;
                default:
                    return false;
            }
        }
        
        private void ShowInventoryPanel()
        {
            Debug.Log("lil webhook test");
            if (inventoryTab == null) return;
            inventoryTab.SetActive(true);
            foreach (var ui in inventoryUIs.Where(ui => ui != null)) { UnityEngine.Object.Destroy(ui.gameObject); }
            inventoryUIs.Clear();
            
            var containerParent = uiService.GetInventoryContainer();
    
            var player = GameManager.Instance.GetPlayer();
            var playerInventory = player.GetComponent<PlayerInventory>();
            var equippedFrame = ServiceLocator.GetService<InventoryService>().GetEquippedFrame(playerInventory);
            if (equippedFrame != null) ShowFrameBasedLayout(equippedFrame, containerParent);

            if (lootOwner != null) { ShowLootPanel(); }
        }
        
        private void ShowFrameBasedLayout(BackpackFrame frame, Transform parent)
        {
            // Get prefabs from UIService
            var frameLayoutPrefab = uiService.GetFrameLayoutContainerPrefab();
            var emptySlotPrefab = uiService.GetEmptyFrameSlotPrefab();
            var spacerPrefab = uiService.GetFrameSpacerPrefab();
            var inventoryUIPrefab = uiService.GetInventoryUIPrefab();
            
            // Instantiate the pre-configured frame layout container
            var frameLayoutInstance = UnityEngine.Object.Instantiate(frameLayoutPrefab, parent);
            currentFrameLayout = frameLayoutInstance;
            var gridLayout = frameLayoutInstance.GetComponent<GridLayoutGroup>();
            
            // Configure grid dimensions based on frame
            var frameSize = frame.definition.containerSize;
            gridLayout.constraintCount = frameSize.x;
            
            // Create a 2D array to track which grid positions have modules
            var moduleMap = new ModuleItem[frameSize.x][];
            for (int index = 0; index < frameSize.x; index++)
            {
                moduleMap[index] = new ModuleItem[frameSize.y];
            }

            var processedModules = new HashSet<ModuleItem>();
            
            // Map modules to their positions using existing Container system
            var frameGridItems = frame.moduleGridItem.storage.GetAllGridItems();
            foreach (var gridItem in frameGridItems)
            {
                if (gridItem.GetItem() is not ModuleItem moduleAsGridItem) continue;
                var module = moduleAsGridItem;
                var position = gridItem.GetRootPosition();
                var moduleSize = moduleAsGridItem.GetItemSize();
                    
                // Mark all positions this module occupies
                for (int x = 0; x < moduleSize.x; x++)
                {
                    for (int y = 0; y < moduleSize.y; y++)
                    {
                        var pos = new Vector2Int(position.x + x, position.y + y);
                        if (pos.x < frameSize.x && pos.y < frameSize.y)
                        {
                            moduleMap[pos.x][pos.y] = module;
                        }
                    }
                }
            }
            
            // Create UI elements for each grid position
            var layoutParent = frameLayoutInstance.transform;
            for (int y = 0; y < frameSize.y; y++)
            {
                for (int x = 0; x < frameSize.x; x++)
                {
                    var module = moduleMap[x][y];
                    
                    if (module != null && !processedModules.Contains(module))
                    {
                        // Create inventory UI for this module using prefab
                        var moduleUI = UnityEngine.Object.Instantiate(inventoryUIPrefab, layoutParent);
                        var inventoryUI = moduleUI.GetComponent<InventoryUI>();
                        inventoryUI.OpenContainer(module);
                        inventoryUIs.Add(inventoryUI);
                        processedModules.Add(module);
                    }
                    else if (module == null)
                    {
                        // Create empty slot using prefab
                        UnityEngine.Object.Instantiate(emptySlotPrefab, layoutParent);
                    }
                    else
                    {
                        // Create spacer for multi-cell modules using prefab
                        UnityEngine.Object.Instantiate(spacerPrefab, layoutParent);
                    }
                }
            }
        }
        
        private void ShowLootPanel()
        {
            if (lootPanel == null || lootOwner == null) return;
            lootContainers = ServiceLocator.GetService<InventoryService>().GetContainersOwnedBy(lootOwner);
            if (lootContainers.Count == 0) return;
            lootPanel.gameObject.SetActive(true);
            LoadContainers(lootContainers, lootPanel);
        }

        private void LoadContainers(List<ContainerItem> containersToLoad, Transform parent)
        {
            var uiPrefab = uiService.GetInventoryUIPrefab();
            foreach (var container in containersToLoad)
            {
                Debug.Log($"Container being loaded for inv UI: {container}");
                var uiInstance = UnityEngine.Object.Instantiate(uiPrefab, parent);
                var inventoryUI = uiInstance.GetComponent<InventoryUI>();
                inventoryUI.OpenContainer(container);
                inventoryUIs.Add(inventoryUI);
            }
        }
        
        private void HideInventoryPanel()
        {
            foreach (var ui in inventoryUIs.Where(ui => ui != null)) { ui.CloseContainer(); }
            
            if (currentFrameLayout != null)
            {
                UnityEngine.Object.Destroy(currentFrameLayout);
                currentFrameLayout = null;
            }

            foreach (var ui in lootInventoryUIs.Where(ui => ui != null))
            { ui.CloseContainer(); UnityEngine.Object.Destroy(ui.gameObject); }
            lootInventoryUIs.Clear();
    
            if (lootPanel != null) lootPanel.gameObject.SetActive(false);
            if (inventoryTab != null) inventoryTab.SetActive(false);
        }

        private void ShowQuestLogPanel()
        {
            if (questLogTab != null) { questLogTab.SetActive(true); }
            else { Debug.LogError("CharacterScreenUIState: Missing UI components for quest log");}
        }
        
        private void HideQuestLogPanel() 
        { if (questLogTab != null) questLogTab.SetActive(false); }
        
        private void HideAllTabs()
        {
            if (inventoryTab != null) inventoryTab.SetActive(false);
            if (questLogTab != null) questLogTab.SetActive(false);
        }

        public void SetParameters(object parameters)
        {
            switch (parameters)
            {
                case CharacterTab tab:
                    currentTab = tab;
                    break;
                case IContainerOwner lootTarget:
                    lootOwner = lootTarget;
                    currentTab = CharacterTab.Inventory;
                    break;
            }
        }
    }
    
    public enum CharacterTab
    {
        Inventory,
        Status,
        QuestLog
    }
}
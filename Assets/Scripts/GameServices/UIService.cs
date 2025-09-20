using System.Globalization;
using Character;
using EnvironmentTools;
using Interfaces;
using Items;
using Quests;
using TMPro;
using UI;
using UI.States;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace GameServices
{
    public class UIService : Service
    {
        [Header("Status Fields")]
        [SerializeField] private Slider healthSlider;
        [SerializeField] private Slider manaSlider;
        [SerializeField] private CircularProgressBar satietyBar;
        [SerializeField] private CircularProgressBar hydrationBar;
        [SerializeField] private CircularProgressBar energyBar;
        [SerializeField] private TMP_Text heartRateIndicator;
        [Header("UI Panels - Dialogue")]
        [SerializeField] private Transform dialogueView;
        
        [Header("UI Panels - Questing")]
        [SerializeField] private GameObject questTab;
        [SerializeField] private GameObject questEntryPrefab;
        [SerializeField] private GameObject questObjectivePrefab;
        [SerializeField] private Transform questContentContainer;
        
        [Header("UI Panels - Character Screen")]
        [SerializeField] private GameObject characterScreenPanel;
        
        [Header("Inventory")]
        [SerializeField] private GameObject inventoryAndGearTab;
        [SerializeField] private Transform inventoryPanel;
        [SerializeField] private GameObject inventoryUIPrefab;
        [SerializeField] private GameObject frameLayoutContainerPrefab;
        [SerializeField] private GameObject emptyFrameSlotPrefab;
        [SerializeField] private GameObject frameSpacerPrefab;
        [SerializeField] private GameObject backpackConfigPanel;
        [SerializeField] private Transform lootPanel;
        
        [Header("UI Panels - Misc.")]
        [SerializeField] private Transform dep_worldMapPanel; // DEPRECATED
        [SerializeField] private GameObject regionBtnPf;
        [SerializeField] private InteractableProgressBar interactionProgressBar;
        [SerializeField] private Transform pauseMenuPanel;
        [SerializeField] private Transform worldMapMenuPanel;
        
        private CharacterStatus playerStatus;
        private QuestLogUI questLogUI;
        private InventoryUI inventoryUI;
        private InventoryUI playerInventoryUI;
        private InventoryUI lootInventoryUI;
        [HideInInspector] public UIStateManager uiStateManager;

        public GameObject GetInventoryUIPrefab() => inventoryUIPrefab;
        public GameObject GetInventoryTab() => inventoryAndGearTab;
        public Transform GetInventoryContainer() => inventoryPanel;
        public Transform GetLootViewPanel() => lootPanel;
        public Transform GetRoutePlannerPanel() => dep_worldMapPanel;
        public GameObject GetRegionBtnPf() => regionBtnPf;
        public GameObject GetDialoguePanel() => dialogueView.gameObject;
        public GameObject GetQuestTab() => questTab;
        public GameObject GetCharacterScreenPanel() => characterScreenPanel;
        public GameObject GetBackpackConfigPanel() => backpackConfigPanel;
        public GameObject GetFrameLayoutContainerPrefab() => frameLayoutContainerPrefab;
        public GameObject GetEmptyFrameSlotPrefab() => emptyFrameSlotPrefab;
        public GameObject GetFrameSpacerPrefab() => frameSpacerPrefab;
        public Transform GetPauseMenuPanel() => pauseMenuPanel;
        public Transform GetWorldMapMenuPanel() => worldMapMenuPanel;

        public override void Initialize()
        {
            InitializeStatusTracking();
            InitializeUIComponents();
            InitializeStateManager();
            Logs.Log("UI Service initialized.", "GameServices");
        }

        private void OnDestroy() { CleanupSubscriptions(); }

        private void InitializeStatusTracking()
        {
            playerStatus = GameManager.Instance.GetPlayer().GetComponent<CharacterStatus>();
            playerStatus.OnStatusUpdate += UpdateAllSliders;
        }

        private void InitializeUIComponents()
        {
            InitializeQuestLogUI();
            InitializeInventoryUI();
        }

        private void InitializeStateManager()
        {
            uiStateManager = GetComponent<UIStateManager>();
            if (uiStateManager == null) { uiStateManager = gameObject.AddComponent<UIStateManager>(); }
            uiStateManager.Initialize(this);
        }

        private void InitializeQuestLogUI()
        {
            if (questTab != null)
            {
                questLogUI = questTab.GetComponent<QuestLogUI>();
                
                questLogUI.SetReferences(
                    questEntryPrefab, 
                    questObjectivePrefab,
                    questContentContainer, 
                    questTab);
                
                questTab.SetActive(false);
            }
            else { Debug.LogWarning("Quest Log View not assigned in UIManager!"); }
        }

        private void InitializeInventoryUI()
        {
            if (inventoryAndGearTab != null) { inventoryAndGearTab.SetActive(false); }
            else { Debug.LogWarning("Inventory panel not assigned in UIManager!"); }
        }

        private void CleanupSubscriptions()
        { if (playerStatus != null) playerStatus.OnStatusUpdate -= UpdateAllSliders; }

        private void UpdateAllSliders()
        {
            healthSlider.value = playerStatus.Health / playerStatus.MaxHealth;
            manaSlider.value = playerStatus.Mana / playerStatus.MaxMana;
            energyBar.SetFill(playerStatus.Energy / playerStatus.MaxEnergy);
            satietyBar.SetFill(playerStatus.Satiety / playerStatus.MaxSatiety);
            hydrationBar.SetFill(playerStatus.Hydration / playerStatus.MaxHydration);
            heartRateIndicator.text = playerStatus.HeartRate.ToString("F1");
        }
        public void ToggleDialogueView()
        {
            if (uiStateManager.CurrentState is DialogueUIState) 
                uiStateManager.TransitionToState<ExplorationUIState>();
            else uiStateManager.TransitionToState<DialogueUIState>();
        }
        
        public void ToggleLootView(IContainerOwner lootOwner)
        {
            if (uiStateManager.CurrentState is CharacterScreenUIState { currentTab: CharacterTab.Inventory }) 
                uiStateManager.TransitionToState<ExplorationUIState>();
            else uiStateManager.TransitionToState<CharacterScreenUIState>(lootOwner);
        }

        public void ToggleRoutePlanner()
        {
            Debug.Log("Attempting to toggle the route planner in UI Service. Current UI State: " + uiStateManager.CurrentState);
            if (uiStateManager.CurrentState is RoutePlannerUIState)
                uiStateManager.TransitionToState<ExplorationUIState>();
            else uiStateManager.TransitionToState<RoutePlannerUIState>();
        }

        public void ToggleInventoryTab()
        {
            if (uiStateManager.CurrentState is CharacterScreenUIState { currentTab: CharacterTab.Inventory })
                uiStateManager.TransitionToState<ExplorationUIState>();
            else uiStateManager.TransitionToState<CharacterScreenUIState>(CharacterTab.Inventory);
        }

        public void ToggleQuestTab()
        {
            if (uiStateManager.CurrentState is CharacterScreenUIState { currentTab: CharacterTab.QuestLog }) 
                uiStateManager.TransitionToState<ExplorationUIState>();
            else uiStateManager.TransitionToState<CharacterScreenUIState>(CharacterTab.QuestLog);
        }

        public void TogglePauseMenu()
        {
            if (uiStateManager.CurrentState is PauseMenuUIState) uiStateManager.TransitionToState<ExplorationUIState>();
            else uiStateManager.TransitionToState<PauseMenuUIState>();
        }
        public void ToggleWorldMapMenu()
        {
            if (uiStateManager.CurrentState is WorldMapMenuState) uiStateManager.TransitionToState<ExplorationUIState>();
            else uiStateManager.TransitionToState<WorldMapMenuState>();
        }

        public void RefreshInventoryDisplay() { inventoryUI?.RefreshInventoryDisplay(); }

        public bool HandleEscapeInput() { return uiStateManager.HandleEscapeInput(); }
        public void StartTimedProgress(TimedInteraction interaction) { interactionProgressBar?.StartTracking(interaction); }
        public void StopTimedProgress() { interactionProgressBar?.StopTracking(); }

        public void OpenBackpackConfiguration(BackpackFrame frame, ContainerItem moduleStorage, BackpackWorkbenchHandler workbench)
        {
            var configParams = new BackpackConfigParams
            {
                frame = frame,
                moduleStorage = moduleStorage,
                workbench = workbench
            };
    
            uiStateManager.TransitionToState<BackpackConfigurationUIState>(configParams);
        }
    }
}
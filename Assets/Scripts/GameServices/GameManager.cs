using System;
using Dialogue;
using EditorTools;
using Gaia;
using Inputs;
using Systems.EventRadio;
using UnityEngine;
using Utils;

namespace GameServices
{
    [DefaultExecutionOrder(-1000)]
    public class GameManager : MonoBehaviour {
        public static GameManager Instance { get; private set; }
        private GameIDRegistry registry;

        [Header("Game Config")] 
        [SerializeField] private bool enableWorldGen;
        [SerializeField] private GameObject player;
        [SerializeField] private float gaiaOverrideDetailDensity;
    
        [Header("Global Events")]
        [SerializeField] private GameEvent onNoiseSourceCreated;
        
        [Header("Game Services")]
        [SerializeField] private TickService tickService;
        [SerializeField] private DialogueService dialogueService;
        [SerializeField] private InputManager inputManager;
        [SerializeField] private GameStateManager gameStateManager;
        [SerializeField] private InventoryService inventoryService;
        [SerializeField] private TooltipService tooltipService;
        [SerializeField] private UIService uiService;
        [SerializeField] private ShopService shopService;
        [SerializeField] private QuestService questService;
        [SerializeField] private ClockService clockService;
        [SerializeField] private WorldManagerService worldManagerService;
        [SerializeField] private StatusEffectService statusEffectService;
        [SerializeField] private CombatService combatService;
        [SerializeField] private SaveService saveService;
    
        // Event for other systems to know when managers are ready
        public event Action OnManagersInitialized;
        public GameEvent OnNoiseSourceCreated => onNoiseSourceCreated;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); }
            else { Instance = this; player = GetPlayer(); }

            registry = Resources.Load<GameIDRegistry>("System/GameIDRegistry");
            if (registry == null) { Debug.LogError("Failed to load GameIDRegistry from Resources!"); }
            InitializeDialogueActionRegistry();
            InitializeSystems();
        }

        private void Start()
        {
            // scene swap is just to check if the player is swapping scenes and needs to load temp data (i.e., temp
            // in load game). Probably a better way to do this
            string tempLoaded = PlayerPrefs.GetString("SceneSwap");
            if (tempLoaded != "false" && tempLoaded != "")
            {
                saveService.LoadGame("temp");
                PlayerPrefs.SetString("SceneSwap", "false");
                return;
            }
            
            string loaded = PlayerPrefs.GetString("GameLoaded");
            if (loaded != "false")
            {
                saveService.LoadGame("TestSave1");
            }
            
            //Override gaia terrain override
            foreach (var tdo in FindObjectsByType<TerrainDetailOverwrite>(FindObjectsSortMode.None))
            {
                tdo.m_unityDetailDensity = gaiaOverrideDetailDensity;
            }
        }

        public GameObject GetPlayer()
        {
            if (!player)
            {
                player = GameObject.FindGameObjectWithTag("Player");
                Logs.Log("Player go: " + player.name);
            }
            return player;
        }

        public GameIDRegistry GetIdRegistry() { return registry; }
        private void InitializeSystems()
        {
            ServiceLocator.Initialize();
            if (gameStateManager == null) { gameStateManager = gameObject.AddComponent<GameStateManager>(); }
            if (inputManager == null) { inputManager = gameObject.AddComponent<InputManager>(); }
            if (questService == null) { questService = gameObject.AddComponent<QuestService>(); }
            if (inventoryService == null) { inventoryService = gameObject.AddComponent<InventoryService>(); }
            if (tooltipService == null) { tooltipService = gameObject.AddComponent<TooltipService>(); }
            if (clockService == null) { clockService = gameObject.AddComponent<ClockService>(); }
            if (worldManagerService != null) { ServiceLocator.RegisterAndInitialize(worldManagerService); }
            if (shopService == null) { shopService = gameObject.AddComponent<ShopService>(); }
            if (statusEffectService == null) { statusEffectService = gameObject.AddComponent<StatusEffectService>(); }
            if (combatService == null) { combatService = gameObject.AddComponent<CombatService>(); }
            if (saveService == null) { saveService = gameObject.AddComponent<SaveService>(); }
            


            // inputManager.InitializeInputManager();

            //High priority services
            ServiceLocator.RegisterAndInitialize(tickService);
            ServiceLocator.RegisterAndInitialize(inputManager);
            ServiceLocator.RegisterAndInitialize(gameStateManager);
            
            ServiceLocator.RegisterAndInitialize(dialogueService);
            ServiceLocator.RegisterAndInitialize(questService);
            ServiceLocator.RegisterAndInitialize(inventoryService);
            ServiceLocator.RegisterAndInitialize(tooltipService);
            ServiceLocator.RegisterAndInitialize(clockService);
            ServiceLocator.RegisterAndInitialize(shopService);
            ServiceLocator.RegisterAndInitialize(statusEffectService);
            ServiceLocator.RegisterAndInitialize(combatService);
            ServiceLocator.RegisterAndInitialize(saveService);
            
            //UI service depends on other services - must initialize late
            ServiceLocator.RegisterAndInitialize(uiService);
            
            OnManagersInitialized?.Invoke();
        }

        /// <summary>
        /// Initialize the actions taken by specific kinds of dialogue. These can be triggered via dialogue tags.
        /// TODO: This is pretty weird - maybe we use built-in inky tools instead
        /// </summary>
        private void InitializeDialogueActionRegistry()
        {
            // Get the action registry
            var actionRegistry = GetComponent<DialogueActionRegistry>();
            
            actionRegistry.RegisterActionHandler(ActionCategory.Shop, shopID => {
                shopService.OpenShop(shopID);
            });
            actionRegistry.RegisterActionHandler(ActionCategory.AcceptQuest, questID => {
                questService.AcceptQuest(questID);
            });
            actionRegistry.RegisterActionHandler(ActionCategory.CompleteQuest, questID => {
                questService.CompleteQuest(questID);
            });
        }
        
        public void QuitToMainMenu()
        {
            // Clean up persistent objects
            var tempPlayer = GetPlayer();
            if (tempPlayer != null) { Destroy(tempPlayer); }
            Instance = null;
            PlayerPrefs.SetString("GameLoaded", "false");
            PlayerPrefs.SetString("SceneSwap", "false");
            Destroy(gameObject);
        }
    }
}
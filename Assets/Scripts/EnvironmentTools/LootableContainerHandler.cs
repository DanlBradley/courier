using System;
using GameServices;
using Interfaces;
using Items;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EnvironmentTools
{
    /// <summary>
    /// This object contains an inventory that the player can view and take items from upon completing a timed interaction.
    /// </summary>
    [RequireComponent(typeof(TimedInteraction))]
    public class LootableContainerHandler : MonoBehaviour, ITimedInteractionHandler, IContainerOwner, ISaveable
    {
        [Header("Container Properties")]
        [SerializeField] private ContainerDefinition lootableContainerDef;
        [SerializeField] private ItemDefinition[] startingItems;
        [SerializeField] private bool hasBeenLooted = false;
        [SerializeField] private string containerName = "lootableChest";
        
        private InventoryService inventoryService;
        private UIService uiService;
        private SaveService saveService;
        private ContainerItem lootableContainer;
        
        //save info
        private bool shouldRestoreState;
        private ContainerSaveData containerSaveData;
        
        public ContainerItem GetLootContainer() => lootableContainer;
        public string GetOwnerID() => $"{gameObject.name}_{Guid.NewGuid()}";

        private void Start()
        {
            // Get services
            inventoryService = ServiceLocator.GetService<InventoryService>();
            uiService = ServiceLocator.GetService<UIService>();
            saveService = ServiceLocator.GetService<SaveService>();
            
            if (inventoryService == null || uiService == null || saveService == null)
            { 
                Debug.LogError("Required services not found!"); 
                return; 
            }

            if (lootableContainerDef == null) return;
            
            lootableContainer = inventoryService.CreateContainer(lootableContainerDef, this);
            AddItemsToContainer(startingItems);
            
            var timedInteraction = GetComponent<TimedInteraction>();
            if (timedInteraction != null)
            { timedInteraction.SetInteractionText(hasBeenLooted ? "Search container" : "Open container"); }
        }

        public void OnInteractionComplete(GameObject interactor)
        {
            if (shouldRestoreState) { RestoreLazyState(); shouldRestoreState = false; }
            if (inventoryService == null || uiService == null) return;
            uiService.ToggleLootView(this);

            if (hasBeenLooted) return;
            
            hasBeenLooted = true;
            var timedInteraction = GetComponent<TimedInteraction>();
            if (timedInteraction == null) return;
            
            timedInteraction.SetInteractionText("Search container");
            timedInteraction.SetHoldDuration(0.5f);
        }

        public bool CanPerformInteraction(GameObject interactor)
        {
            return inventoryService != null && uiService != null;
        }
        
        private void AddItemsToContainer(ItemDefinition[] itemsToLoot)
        {
            if (itemsToLoot == null || itemsToLoot.Length == 0) return;
            
            foreach (var itemDef in itemsToLoot)
            {
                if (itemDef == null) continue;
                Item item = new Item(itemDef);
                lootableContainer.storage.TryAddItem(item);
            }
        }

        public string SaveID => $"{containerName}_{SceneManager.GetActiveScene().name}_{transform.position.ToString()}";

        public object CaptureState() { return new ContainerSaveData(lootableContainer); }

        public void RestoreState(object saveData)
        {
            containerSaveData = saveData as ContainerSaveData;
            shouldRestoreState = true;
        }

        private void RestoreLazyState()
        {
            lootableContainer = inventoryService.CreateContainer(lootableContainerDef, this);
            var registry = GameManager.Instance.GetIdRegistry();
            foreach (var savedItem in containerSaveData.storageSaveData)
            {
                lootableContainer.storage.TryAddItemAtPosition(
                    new Item(registry.GetItemDefinition(savedItem.id)),
                    new Vector2Int(savedItem.gridX, savedItem.gridY));
            }
        }
    }
}
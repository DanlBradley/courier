using System;
using GameServices;
using Interfaces;
using Items;
using UnityEngine;

namespace EnvironmentTools
{
    /// <summary>
    /// This object contains an inventory that the player can view and take items from upon interacting with.
    /// </summary>
    public class LootableContainer : MonoBehaviour, IInteractable, IContainerOwner
    {
        [Header("Container Properties")]
        [SerializeField] private ContainerDefinition lootableContainerDef;
        [SerializeField] private ItemDefinition[] itemsToLoot;
        
        private InventoryService inventoryService;
        private UIService uiService;
        private ContainerItem lootableContainer;
        public ContainerItem GetLootContainer() => lootableContainer;
        
        public string GetOwnerID() => $"Lootable_Container_{gameObject.name}";

        private void Start()
        {
            // Get services
            inventoryService = ServiceLocator.GetService<InventoryService>();
            uiService = ServiceLocator.GetService<UIService>();
            
            if (inventoryService == null || uiService == null)
            { Debug.LogError("Required services not found!"); return; }

            if (lootableContainerDef == null) return;
            lootableContainer = inventoryService.CreateContainer(lootableContainerDef, this);
            AddTestItems();
        }

        public bool CanInteract(GameObject interactor) { return inventoryService != null && uiService != null; }
        public void StartInteraction(GameObject interactor, Action onComplete, Action onCancel)
        {
            uiService.ToggleLootView(this);
            onComplete?.Invoke();
        }

        public void CancelInteraction() { }

        public GameObject GetGameObject() { return gameObject; }
        
        private void AddTestItems()
        {
            if (itemsToLoot == null || itemsToLoot.Length == 0) return;
            
            foreach (var itemDef in itemsToLoot)
            {
                if (itemDef == null) continue;
                Item item = new Item(itemDef);
                lootableContainer.storage.TryAddItem(item);
            }
        }
    }
}

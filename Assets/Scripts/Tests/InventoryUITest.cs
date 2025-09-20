using GameServices;
using UnityEngine;
using Items;
using Interfaces;

namespace Tests
{
    /// <summary>
    /// Simple test script to demonstrate opening/closing inventory UI
    /// Attach this to the Player GameObject alongside PlayerInventoryTest
    /// </summary>
    public class InventoryUITest : MonoBehaviour, IContainerOwner
    {
        [Header("Test Configuration")]
        [SerializeField] private ContainerDefinition playerBackpackDef;
        [SerializeField] private ItemDefinition[] testItems;
        
        private InventoryService inventoryService;
        private UIService iuiService;
        private ContainerItem playerBackpack;
        
        public string GetOwnerID() => $"Player_{gameObject.name}";
        
        private void Start()
        {
            // Get services
            inventoryService = ServiceLocator.GetService<InventoryService>();
            iuiService = ServiceLocator.GetService<UIService>();
            
            if (inventoryService == null || iuiService == null)
            {
                Debug.LogError("Required services not found!");
                return;
            }
            
            // Create player backpack
            if (playerBackpackDef != null)
            {
                playerBackpack = inventoryService.CreateContainer(playerBackpackDef, this);
                // Debug.Log($"Created player backpack: {playerBackpack.ContainerID}");
                
                // Add some test items
                AddTestItems();
            }
        }
        
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.I))
            {
                if (playerBackpack != null) { iuiService.ToggleInventoryTab(); }
                else { Debug.LogError("Inventory not found"); }
            }
            
            // Test adding items
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                AddRandomItem();
                iuiService.RefreshInventoryDisplay();
            }
            
            // Test removing items
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                RemoveRandomItem();
                iuiService.RefreshInventoryDisplay();
            }
        }
        
        private void AddTestItems()
        {
            if (testItems == null || testItems.Length == 0) return;
            
            foreach (var itemDef in testItems)
            {
                if (itemDef != null)
                {
                    Item item = new Item(itemDef);
                    bool added = playerBackpack.storage.TryAddItem(item);
                    // Debug.Log($"Added {itemDef.name} to backpack: {added}");
                }
            }
        }
        
        private void AddRandomItem()
        {
            if (testItems == null || testItems.Length == 0 || playerBackpack == null) return;
            
            ItemDefinition randomDef = testItems[Random.Range(0, testItems.Length)];
            if (randomDef != null)
            {
                Item item = new Item(randomDef);
                bool added = playerBackpack.storage.TryAddItem(item);
                // Debug.Log($"Added random item {randomDef.name}: {added}");
            }
        }
        
        private void RemoveRandomItem()
        {
            if (playerBackpack == null) return;
            
            Item[] items = playerBackpack.storage.GetAllItems();
            if (items.Length > 0)
            {
                Item randomItem = items[Random.Range(0, items.Length)];
                bool removed = playerBackpack.storage.TryRemoveItem(randomItem);
                // Debug.Log($"Removed random item: {removed}");
            }
        }
    }
}
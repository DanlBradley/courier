using System;
using GameServices;
using Inputs;
using Interfaces;
using Items;
using UnityEngine;


namespace Character.Player
{
    /// <summary>
    /// Responsibility: Handle Quick Slot items. Intended functionality: Player hovers over "usable" items in the UI menu
    /// and can press 1, 2, 3, or 4 on the keyboard. These items become "QuickSlot" items where the player sort of "equips"
    /// the items on key press like in stardew/minecraft. For example, assign the canteen to "1" and then pressing 1 puts the
    /// canteen in the player's hand, where they can either drink from it (maybe hold interact key) or fill it up (hold
    /// interact key at a water source or a different button?) And also it could be cute if they're running around with a
    /// canteen in their hand or a mushroom for example.
    /// </summary>
    public class PlayerQuickSlots : MonoBehaviour
    {
        [SerializeField] private int activeSlot = -1; //-1 flag to show no slot equipped
        private InputManager inputManager;
        private InventoryService inventoryService;
        private Item[] quickSlots = new Item[4];
        
        // Events for UI/visual updates
        public event Action<int, Item> OnQuickSlotChanged;
        public event Action<int> OnActiveSlotChanged;
        
        private void Start()
        {
            inputManager = ServiceLocator.GetService<InputManager>();
            inventoryService = ServiceLocator.GetService<InventoryService>();
            inputManager.OnHotkeyInput += ActivateQuickSlot;
            inventoryService.OnItemRemoved += OnItemRemovedFromInventory;
        }

        private void OnDestroy()
        {
            inputManager.OnHotkeyInput -= ActivateQuickSlot;
            inventoryService.OnItemRemoved -= OnItemRemovedFromInventory;
        }
        
        public void AssignToQuickSlot(Item item, int slotNumber)
        {
            if (slotNumber is < 1 or > 4) { Debug.LogWarning($"Invalid quick slot number: {slotNumber}"); return; }
            if (!inventoryService.IsItemInPlayerInventory(item)) return;
            
            for (int i = 0; i < quickSlots.Length; i++)
            {
                if (quickSlots[i] != item) continue;
                quickSlots[i] = null;
                OnQuickSlotChanged?.Invoke(i, null);
                break;
            }
            
            int slotIndex = slotNumber - 1; // Convert to 0-based index
            quickSlots[slotIndex] = item;
            
            OnQuickSlotChanged?.Invoke(slotIndex, item);
        }
        
        private void ActivateQuickSlot(int slotNumber)
        {
            if (GameStateManager.Instance.CurrentState != GameState.Exploration) return;
            if (slotNumber is < 1 or > 4) { Debug.LogWarning($"Invalid quick slot number: {slotNumber}"); return; }
            
            int slotIndex = slotNumber - 1;
            
            if (activeSlot == slotIndex) { DeactivateCurrentSlot(); }
            else
            {
                activeSlot = slotIndex;
                OnActiveSlotChanged?.Invoke(activeSlot);
                
            }
        }
        
        private void OnItemRemovedFromInventory(ContainerItem container, Item item, IContainerOwner owner)
        {
            IContainerOwner playerAsOwner = GetComponent<PlayerInventory>();
            if (owner != playerAsOwner) return;
        
            // Check if this item is in any quick slot
            for (int i = 0; i < quickSlots.Length; i++)
            {
                if (quickSlots[i] != item) continue;
                ClearQuickSlot(i + 1);
                break;
            }
        }
        
        private  void ClearQuickSlot(int slotNumber)
        {
            Debug.Log($"Clearing quick slot {slotNumber}");
            if (slotNumber is < 1 or > 4) return;
        
            int slotIndex = slotNumber - 1;
            quickSlots[slotIndex] = null;
        
            if (activeSlot == slotIndex) { DeactivateCurrentSlot(); }
        
            OnQuickSlotChanged?.Invoke(slotIndex, null);
        }
        
        private void DeactivateCurrentSlot()
        {
            if (activeSlot == -1) return;
            
            Debug.Log($"Deactivated quick slot {activeSlot + 1}");
            activeSlot = -1;
            OnActiveSlotChanged?.Invoke(activeSlot);
        }
        
        public Item GetActiveItem() => activeSlot == -1 ? null : quickSlots[activeSlot];
        public Item GetQuickSlotItem(int slotNumber) => quickSlots[slotNumber - 1];
        public int GetActiveSlotNumber() => activeSlot == -1 ? -1 : activeSlot + 1;
        

        private void OnValidate()
        { if (quickSlots == null || quickSlots.Length != 4) { quickSlots = new Item[4]; } }
    }
}

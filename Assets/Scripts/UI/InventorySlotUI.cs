using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using GameServices;
using Interfaces;
using Items;

namespace UI
{
    // Component for individual inventory slots
    public class InventorySlotUI : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
    {
        private Vector2Int position;
        private InventoryUI inventoryUI;
        private Image slotImage;
        private Color originalColor;
        private bool isDragTarget;
        
        public Vector2Int GetPosition() => position;
        
        public void Initialize(Vector2Int pos, InventoryUI ui)
        {
            position = pos;
            inventoryUI = ui;
            slotImage = GetComponent<Image>();
            if (slotImage != null) originalColor = slotImage.color;
        }
        
        public void OnDrop(PointerEventData eventData)
        {
            // Get the dragged item UI
            InventoryItemUI draggedItemUI = eventData.pointerDrag?.GetComponent<InventoryItemUI>();
            
            if (draggedItemUI != null)
            {
                if (draggedItemUI.GetInventoryUI() != inventoryUI) 
                { inventoryUI.HandleItemDrop(draggedItemUI, position); }
                else { inventoryUI.OnItemDropped(position); }
            }
            
            if (slotImage != null) slotImage.color = originalColor;
            isDragTarget = false;
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (slotImage == null || !eventData.dragging) return;
            slotImage.color = inventoryUI.highlightColor;
            isDragTarget = true;
        }
        
        public void OnPointerExit(PointerEventData eventData)
        {
            if (slotImage == null || !isDragTarget) return;
            slotImage.color = originalColor;
            isDragTarget = false;
        }
    }
}
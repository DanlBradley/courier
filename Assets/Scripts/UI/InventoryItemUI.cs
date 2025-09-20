using Character.Player;
using GameServices;
using Inputs;
using Interfaces;
using Items;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
    {
        // Component for draggable items
        public class InventoryItemUI : MonoBehaviour, IBeginDragHandler, IDragHandler, 
            IEndDragHandler, IPointerDownHandler, ITooltippable, IPointerEnterHandler, IPointerExitHandler
        {
            private Item item;
            private Vector2Int position;
            private InventoryUI inventoryUI;
            private RectTransform rectTransform;
            private Canvas canvas;
            private CanvasGroup canvasGroup;
            private Vector2Int clickOffset;
        
            public Vector2Int GetClickOffset() => clickOffset;
            
            private bool isHovered = false;
            private InputManager inputManager;
    
            private void Start()
            {
                inputManager = ServiceLocator.GetService<InputManager>();
            }
            
            public void Initialize(Item itemRef, Vector2Int pos, InventoryUI ui)
            {
                item = itemRef;
                position = pos;
                inventoryUI = ui;
                rectTransform = GetComponent<RectTransform>();
            
                // Find canvas for proper dragging
                canvas = GetComponentInParent<Canvas>();
                if (canvas == null)
                {
                    // Find root canvas
                    Canvas[] allCanvas = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
                    foreach (var c in allCanvas)
                    {
                        if (!c.isRootCanvas) continue;
                        canvas = c;
                        break;
                    }
                }
            
                canvasGroup = GetComponent<CanvasGroup>();
                
                if (GetComponent<TooltipTrigger>() == null) { gameObject.AddComponent<TooltipTrigger>(); }
            }
    
            public void OnPointerEnter(PointerEventData eventData)
            {
                isHovered = true;
                if (inputManager == null) return;
                inputManager.OnHotkeyInput += HandleQuickSlotAssignment;
            }
    
            public void OnPointerExit(PointerEventData eventData)
            {
                isHovered = false;
                if (inputManager == null) return;
                inputManager.OnHotkeyInput -= HandleQuickSlotAssignment;
            }
            
            private void HandleQuickSlotAssignment(int slotNumber)
            {
                if (!isHovered) return;
                if (GameStateManager.Instance.CurrentState != GameState.UI) return;
                GameManager.Instance.GetPlayer().GetComponent<PlayerQuickSlots>().AssignToQuickSlot(item, slotNumber);
            }
            public Item GetItem() => item;
            public Vector2Int GetPosition() => position;
            
            // Getter for the InventoryUI this item belongs to
            public InventoryUI GetInventoryUI() => inventoryUI;
    
            // Method to reset visual state after cross-container drag
            public void ResetDragVisuals()
            {
                CanvasGroup group = GetComponent<CanvasGroup>();
                if (group != null)
                {
                    group.alpha = 1f;
                    group.blocksRaycasts = true;
                }
            }
        
            public void OnBeginDrag(PointerEventData eventData)
            {
                var tooltipSystem = ServiceLocator.GetService<TooltipService>();
                tooltipSystem?.HideTooltip();
                
                inventoryUI.OnItemDragStart(this, clickOffset);
                transform.SetAsLastSibling();
            }
        
            public void OnDrag(PointerEventData eventData)
            {
                if (rectTransform == null) return;
    
                // Move by delta
                Vector2 newPosition = rectTransform.anchoredPosition + eventData.delta / canvas.scaleFactor;
    
                // Optional: Clamp to screen bounds
                RectTransform canvasRect = canvas.transform as RectTransform;
                float halfWidth = rectTransform.rect.width * 0.5f;
                float halfHeight = rectTransform.rect.height * 0.5f;
    
                newPosition.x = Mathf.Clamp(newPosition.x, -canvasRect.rect.width * 0.5f + halfWidth, 
                    canvasRect.rect.width * 0.5f - halfWidth);
                newPosition.y = Mathf.Clamp(newPosition.y, -canvasRect.rect.height * 0.5f + halfHeight, 
                    canvasRect.rect.height * 0.5f - halfHeight);
    
                rectTransform.anchoredPosition = newPosition;
            }
        
            public void OnEndDrag(PointerEventData eventData) { inventoryUI.OnItemDragEnd(); }
        
            public void OnPointerDown(PointerEventData eventData)
            {
                if (rectTransform == null) return;
        
                // Calculate which grid cell within the item was clicked
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    rectTransform, 
                    eventData.position, 
                    eventData.pressEventCamera, 
                    out var localPoint);
        
                // Convert local point to grid offset
                // Since pivot is at top-left (0,1), positive X is right, negative Y is down
                float slotSize = inventoryUI.slotSize;
                float slotSpacing = inventoryUI.slotSpacing;
        
                int xOffset = Mathf.FloorToInt(localPoint.x / (slotSize + slotSpacing));
                int yOffset = Mathf.FloorToInt(-localPoint.y / (slotSize + slotSpacing));
        
                clickOffset = new Vector2Int(xOffset, yOffset);
            }
            
            public string GetTooltipTitle() => item?.itemDef?.name ?? "Unknown Item";
    
            public string GetTooltipDescription() => item?.itemDef?.description ?? "";
    
            public string GetTooltipDetails()
            {
                return item?.itemDef == null ? "" : $"Size: {item.itemDef.size.x}x{item.itemDef.size.y}";
            }
    
            public float GetTooltipDelay() => 0.5f;
        }
    }
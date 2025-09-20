using System.Collections.Generic;
using System.Linq;
using GameServices;
using Items;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace UI
{
    public class InventoryUI : MonoBehaviour
    {
        [Header("UI References")] [SerializeField]
        private GameObject gridSlotPrefab;

        [SerializeField] private Transform gridContainer;
        [SerializeField] private TMP_Text containerTitleText;

        [Header("Visual Settings")] [SerializeField]
        public float slotSize = 50f;

        [SerializeField] public float slotSpacing = 2f;
        [SerializeField] private Color normalSlotColor = new(0.3f, 0.3f, 0.3f, 0.8f);
        [SerializeField] private Color occupiedSlotColor = new(0.2f, 0.2f, 0.2f, 0.9f);
        [SerializeField] public Color highlightColor = new(0.5f, 0.5f, 0.7f, 0.8f);

        // Core references
        private InventoryService inventoryService;
        private ContainerItem currentContainer;
        private Dictionary<Vector2Int, GameObject> gridSlots = new();
        private Dictionary<Item, InventoryItemUI> itemUIElements = new();

        // State
        private bool isInitialized;
        private InventoryItemUI draggedItem;
        private Vector2 dragStartPosition;
        private Vector2Int dragOffset;

        public ContainerItem GetCurrentContainer() => currentContainer;


        private void Start()
        {
            if (isInitialized) return;
            inventoryService = ServiceLocator.GetService<InventoryService>();
            if (inventoryService == null)
            {
                Debug.LogError("InventoryManager not found!");
                return;
            }

            isInitialized = true;
            inventoryService.OnInventoryChanged += RefreshInventoryDisplay;
        }

        private void OnEnable() { if (isInitialized) inventoryService.OnInventoryChanged += RefreshInventoryDisplay; }
        private void OnDisable() { inventoryService.OnInventoryChanged -= RefreshInventoryDisplay; }

        // Main method to open a container
        public void OpenContainer(ContainerItem container)
        {
            if (!isInitialized)
            {
                inventoryService = ServiceLocator.GetService<InventoryService>();
                if (inventoryService == null)
                {
                    Debug.LogError("InventoryManager not found!");
                    return;
                }

                isInitialized = true;
            }

            if (container == null)
            {
                Debug.LogError("Container is null.");
                return;
            }

            if (containerTitleText != null)
            {
                containerTitleText.text = container.itemDef.name;
            }

            currentContainer = container;

            BuildInventoryGrid();
            RefreshInventoryDisplay();
        }

        public void CloseContainer()
        {
            ClearInventoryDisplay();
            currentContainer = null;
        }

        private void BuildInventoryGrid()
        {
            if (currentContainer == null) return;
            ClearInventoryDisplay();
            Vector2Int containerSize = currentContainer.storage.GetSize();

            // Set up grid layout
            GridLayoutGroup gridLayout = gridContainer.GetComponent<GridLayoutGroup>();

            gridLayout.cellSize = new Vector2(slotSize, slotSize);
            gridLayout.spacing = new Vector2(slotSpacing, slotSpacing);
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = containerSize.x;

            // GridLayoutGroup specific settings:
            gridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
            gridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
            gridLayout.childAlignment = TextAnchor.UpperLeft;

            // Create grid slots
            for (int y = 0; y < containerSize.y; y++)
            {
                for (int x = 0; x < containerSize.x; x++)
                {
                    GameObject slot = Instantiate(gridSlotPrefab, gridContainer);
                    Vector2Int pos = new Vector2Int(x, y);

                    // Set up slot visuals
                    Image slotImage = slot.GetComponent<Image>();
                    if (slotImage == null) slotImage = slot.AddComponent<Image>();
                    slotImage.color = normalSlotColor;

                    // Add slot component
                    InventorySlotUI slotUI = slot.AddComponent<InventorySlotUI>();
                    slotUI.Initialize(pos, this);

                    gridSlots[pos] = slot;
                }
            }

            // Adjust content size for scrolling
            RectTransform contentRect = gameObject.GetComponent<RectTransform>();
            if (contentRect == null) return;
            float totalHeight =
                (containerSize.y * slotSize) + ((containerSize.y - 1) * slotSpacing) + 40; //40 for title bar height
            float totalWidth = (containerSize.x * slotSize) + ((containerSize.x - 1) * slotSpacing);
            contentRect.sizeDelta = new Vector2(totalWidth, totalHeight);
        }

        public void RefreshInventoryDisplay()
        {
            if (currentContainer == null) return;

            // Clear existing item UI elements
            foreach (var itemUI in itemUIElements.Values.Where(itemUI => itemUI != null))
            {
                Destroy(itemUI.gameObject);
            }

            itemUIElements.Clear();

            // Reset all slot colors
            foreach (var slot in gridSlots.Values)
            {
                Image slotImage = slot.GetComponent<Image>();
                if (slotImage != null) slotImage.color = normalSlotColor;
            }

            // Create UI elements for each item using GridItems directly
            GridItem[] gridItems = currentContainer.storage.GetAllGridItems();
            foreach (GridItem gridItem in gridItems)
            {
                CreateItemUI(gridItem);
            }
        }

        private void CreateItemUI(GridItem gridItem)
        {
            Item item = gridItem.GetItem();
            Vector2Int itemPosition = gridItem.GetRootPosition();

            // Get the slot at that position
            if (!gridSlots.TryGetValue(itemPosition, out _))
            {
                Debug.LogError($"No slot found at position {itemPosition} for item {item.itemDef.name}");
                return;
            }

            // Create item UI directly under grid container
            GameObject itemGo = new GameObject($"Item_{item.itemDef.name}");
            itemGo.transform.SetParent(gridContainer, false);

            // Add components
            RectTransform itemRect = itemGo.AddComponent<RectTransform>();
            Image itemImage = itemGo.AddComponent<Image>();
            itemGo.AddComponent<CanvasGroup>();

            // CRITICAL: Add LayoutElement to ignore grid layout
            LayoutElement layoutElement = itemGo.AddComponent<LayoutElement>();
            layoutElement.ignoreLayout = true;

            // Set up visuals
            if (item.itemDef.sprite != null) itemImage.sprite = item.itemDef.sprite;
            else itemImage.color = Color.gray; // Placeholder color

            // Calculate size based on item dimensions
            Vector2Int itemSize = item.GetItemSize();
            float width = (itemSize.x * slotSize) + ((itemSize.x - 1) * slotSpacing);
            float height = (itemSize.y * slotSize) + ((itemSize.y - 1) * slotSpacing);
            itemRect.sizeDelta = new Vector2(width, height);

            // Set anchors and pivot
            itemRect.anchorMin = Vector2.up; // Top-left anchor (0, 1)
            itemRect.anchorMax = Vector2.up; // Top-left anchor (0, 1)
            itemRect.pivot = Vector2.up; // Top-left pivot (0, 1)

            // Calculate position based on grid position
            // X position is straightforward
            float xPos = itemPosition.x * (slotSize + slotSpacing);

            // Y position: Since we're using top-left anchor/pivot, we position from the top
            // Grid row 0 should be at the top, row 1 should be one slot down, etc.
            float yPos = -(itemPosition.y * (slotSize + slotSpacing));

            itemRect.anchoredPosition = new Vector2(xPos, yPos);

            // Ensure item renders on top of slots
            itemGo.transform.SetAsLastSibling();

            // Add item UI component
            InventoryItemUI itemUI = itemGo.AddComponent<InventoryItemUI>();
            itemUI.Initialize(item, itemPosition, this);

            itemUIElements[item] = itemUI;

            MarkOccupiedSlots(itemPosition, itemSize);
        }

        private void MarkOccupiedSlots(Vector2Int origin, Vector2Int size)
        {
            for (int y = 0; y < size.y; y++)
            {
                for (int x = 0; x < size.x; x++)
                {
                    Vector2Int slotPos = new Vector2Int(origin.x + x, origin.y + y);
                    if (!gridSlots.TryGetValue(slotPos, out GameObject slot)) continue;
                    Image slotImage = slot.GetComponent<Image>();
                    if (slotImage != null) slotImage.color = occupiedSlotColor;
                }
            }
        }

        private void ClearInventoryDisplay()
        {
            // Clear item UI elements
            foreach (var itemUI in itemUIElements.Values.Where(itemUI => itemUI != null))
            {
                Destroy(itemUI.gameObject);
            }

            itemUIElements.Clear();

            // Clear grid slots
            foreach (var slot in gridSlots.Values.Where(slot => slot != null))
            {
                Destroy(slot);
            }

            gridSlots.Clear();
        }

        // Called by InventoryItemUI when drag starts
        public void OnItemDragStart(InventoryItemUI itemUI, Vector2Int clickOffset)
        {
            draggedItem = itemUI;
            dragOffset = clickOffset;

            // Store the original position in case we need to snap back
            RectTransform draggedRect = itemUI.GetComponent<RectTransform>();
            dragStartPosition = draggedRect.anchoredPosition;

            // Make item semi-transparent during drag
            CanvasGroup group = itemUI.GetComponent<CanvasGroup>();
            if (group != null)
            {
                group.alpha = 0.6f;
                group.blocksRaycasts = false;
            }
        }

        // Called by InventorySlotUI when item is dropped
        public void OnItemDropped(Vector2Int targetPosition)
        {
            if (draggedItem == null || currentContainer == null) return;

            Item item = draggedItem.GetItem();
            Vector2Int currentPos = draggedItem.GetPosition();

            Vector2Int adjustedTargetPosition = targetPosition - dragOffset;

            Debug.Log(
                $"Dropping item at slot {targetPosition}, adjusted to {adjustedTargetPosition} (offset was {dragOffset})");

            // Try to move item in the container
            bool success = false;
            
            if (inventoryService.RemoveItemFromContainer(currentContainer, item))
            {
                if (currentContainer.storage.TryAddItemAtPosition(item, adjustedTargetPosition))
                {
                    // Success - refresh display
                    success = true;
                    RefreshInventoryDisplay();
                }
                else
                {
                    // Failed to add at new position, add back at original
                    currentContainer.storage.TryAddItemAtPosition(item, currentPos);
                }
            }

            // If the move failed, snap back to original position
            if (!success && draggedItem != null)
            {
                RectTransform draggedRect = draggedItem.GetComponent<RectTransform>();
                draggedRect.anchoredPosition = dragStartPosition;

                // Reset visual
                CanvasGroup group = draggedItem.GetComponent<CanvasGroup>();
                if (group != null)
                {
                    group.alpha = 1f;
                    group.blocksRaycasts = true;
                }
            }

            draggedItem = null;
        }

        // Called when drag is cancelled
        public void OnItemDragEnd()
        {
            if (draggedItem == null) return;

            // Check if we're over a valid slot
            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            bool droppedOnValidSlot = false;
            Vector2Int? targetSlotPosition = null;

            foreach (var result in results)
            {
                InventorySlotUI slot = result.gameObject.GetComponent<InventorySlotUI>();
                if (slot != null)
                {
                    // We found a slot, but we need to check if the drop would be valid
                    // Get the slot position from the slot component
                    targetSlotPosition = slot.GetPosition(); // Need to add this method to InventorySlotUI
                    droppedOnValidSlot = true;
                    break;
                }
            }

            // If we didn't drop on a slot, or if OnItemDropped wasn't called
            // (which happens when dropping on an occupied slot), snap back
            bool shouldSnapBack = !droppedOnValidSlot || draggedItem != null;

            // Also check if the item is still being dragged (OnItemDropped sets draggedItem to null on success)

            if (!shouldSnapBack || draggedItem == null) return;
            RectTransform draggedRect = draggedItem.GetComponent<RectTransform>();
            draggedRect.anchoredPosition = dragStartPosition;

            // Reset visual state
            CanvasGroup group = draggedItem.GetComponent<CanvasGroup>();
            if (group != null)
            {
                group.alpha = 1f;
                group.blocksRaycasts = true;
            }

            draggedItem = null;
        }

        public void HandleItemDrop(InventoryItemUI draggedItemUI, Vector2Int targetPosition)
        {
            if (draggedItemUI.GetInventoryUI() != this)
            { HandleCrossContainerDrop(draggedItemUI, targetPosition); }
            else { OnItemDropped(targetPosition); }
        }

        private void HandleCrossContainerDrop(InventoryItemUI draggedItemUI, Vector2Int targetPosition)
        {
            Item item = draggedItemUI.GetItem();
            ContainerItem sourceContainer = draggedItemUI.GetInventoryUI().GetCurrentContainer();
            Vector2Int clickOffset = draggedItemUI.GetClickOffset();
            Vector2Int adjustedTargetPosition = targetPosition - clickOffset;
            
            bool success = inventoryService.TransferItem(
                sourceContainer, currentContainer,
                item, adjustedTargetPosition);
            if (success)
            {
                draggedItemUI.GetInventoryUI().RefreshInventoryDisplay();
                RefreshInventoryDisplay();
            }
            draggedItemUI.ResetDragVisuals();
        }
    }
}
using System;
using Character.Player;
using GameServices;
using Interfaces;
using Items;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class QuickSlotUI : MonoBehaviour, ITooltippable
    {
        [Header("UI References")]
        [SerializeField] private int quickSlotNumber = 1;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image itemImage;
        [SerializeField] private Sprite emptySlotSprite;
        
        [Header("Visual States")]
        [SerializeField] private Color normalBackgroundColor = new Color(0.3f, 0.3f, 0.3f, 0.8f);
        [SerializeField] private Color activeBackgroundColor = new Color(0.8f, 0.8f, 0.2f, 0.9f);
        [SerializeField] private Color emptyBackgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
        
        private ItemDefinition itemRef;
        private PlayerQuickSlots playerQuickSlots;
        private bool isActiveSlot;

        private void Start()
        {
            playerQuickSlots = GameManager.Instance.GetPlayer().GetComponent<PlayerQuickSlots>();
            playerQuickSlots.OnQuickSlotChanged += UpdateSlot;
            playerQuickSlots.OnActiveSlotChanged += UpdateActiveState;
            if (GetComponent<TooltipTrigger>() == null) { gameObject.AddComponent<TooltipTrigger>(); }
            UpdateSlotDisplay();
        }

        private void UpdateSlot(int slotIndex, Item item)
        {
            if (slotIndex + 1 != quickSlotNumber) return;
            itemRef = item?.itemDef;
            UpdateSlotDisplay();
        }
        
        private void UpdateActiveState(int activeSlotIndex)
        {
            // Check if this slot is the active one (convert from 0-based to 1-based)
            isActiveSlot = (activeSlotIndex + 1 == quickSlotNumber);
            UpdateSlotDisplay();
        }
        
        private void UpdateSlotDisplay()
        {
            if (backgroundImage != null)
            {
                if (itemRef == null) { backgroundImage.color = emptyBackgroundColor; }
                else { backgroundImage.color = isActiveSlot ? activeBackgroundColor : normalBackgroundColor; }
            }
            if (itemImage == null) return;
            if (itemRef != null && itemRef.sprite != null)
            {
                itemImage.sprite = itemRef.sprite;
                itemImage.color = Color.white; // Always normal
            }
            else
            {
                itemImage.sprite = emptySlotSprite;
                itemImage.color = emptySlotSprite == null ? Color.clear : Color.white;
            }
        }

        private void OnDestroy()
        {
            if (playerQuickSlots == null) return;
            playerQuickSlots.OnQuickSlotChanged -= UpdateSlot;
            playerQuickSlots.OnActiveSlotChanged -= UpdateActiveState;
        }
        
        public string GetTooltipTitle() => itemRef?.name ?? $"Quick Slot {quickSlotNumber}";
        public string GetTooltipDescription() => itemRef?.description ?? "Empty";
        public string GetTooltipDetails() 
        { 
            return itemRef == null ? "Press a number key while hovering over an item to assign it here" 
                                   : $"Size: {itemRef.size.x}x{itemRef.size.y}"; 
        }
        public float GetTooltipDelay() => 0.5f;
    }
}
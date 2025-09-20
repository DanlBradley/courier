using Character.Player;
using GameServices;
using Interfaces;
using Items;
using UnityEngine;

namespace Inputs
{
    public class PlayerActionController : InputConsumer
    {
        [SerializeField] private float interactionRange = 2f;
        [SerializeField] private LayerMask interactableLayers;
        
        private UIService uiService;
        private InputManager inputManager;
        
        private IInteractable currentInteractable;
        private bool isInteracting;
        
        private void Awake()
        {
            inputManager = ServiceLocator.GetService<InputManager>();
            uiService = ServiceLocator.GetService<UIService>();
        }
        
        protected override void SubscribeToEvents()
        {
            inputManager.OnInteractInput += HandleInteract;
            inputManager.OnInteractInputCanceled += HandleInteractRelease;
            inputManager.OnQuestLogToggleInput += HandleQuestLogToggle;
            inputManager.OnRoutePlannerToggleInput += HandleRoutePlanner;
            inputManager.OnInventoryToggleInput += HandleInventoryToggle;
            inputManager.OnBagToggleInput += HandleBagToggle;
            inputManager.OnEscapeInput += HandleEscape;
            inputManager.OnQuickUseInput += HandleQuickSlot;
            inputManager.OnPauseMenuToggleInput += HandlePauseMenuToggle;
        }
        
        protected override void UnsubscribeFromEvents()
        {
            inputManager.OnInteractInput -= HandleInteract;
            inputManager.OnInteractInputCanceled -= OnInteractionCancelled;
            inputManager.OnQuestLogToggleInput -= HandleQuestLogToggle;
            inputManager.OnRoutePlannerToggleInput -= HandleRoutePlanner;
            inputManager.OnInventoryToggleInput -= HandleInventoryToggle;
            inputManager.OnBagToggleInput -= HandleBagToggle;
            inputManager.OnEscapeInput -= HandleEscape;
            inputManager.OnQuickUseInput -= HandleQuickSlot;
            inputManager.OnPauseMenuToggleInput -= HandlePauseMenuToggle;
        }
        
        private void HandleInteract(string context)
        {
            if (!ShouldProcessInput() || isInteracting) return;
            var interactable = FindClosestInteractable(context);
            if (interactable == null) return;
            currentInteractable = interactable;
            Debug.Log("Interactable: " + currentInteractable);
            isInteracting = true;
            interactable.StartInteraction(gameObject, OnInteractionComplete, OnInteractionCancelled);
        }

        private void HandleQuickSlot()
        {
            var activeItem = GetComponent<PlayerQuickSlots>().GetActiveItem();
            ProcessItemBehaviors(activeItem, UseType.Hotkey);
        }

        private void ProcessItemBehaviors(Item item, UseType useType, GameObject target = null)
        {
            if (item == null) return;
            var behaviors = item.itemDef.behaviors;
            var context = new UseContext(useType, gameObject, target);
            foreach (var behavior in behaviors)
            { if(behavior.CanUse(item, context)) behavior.Use(item, context); }
        }
    
        private IInteractable FindClosestInteractable(string context)
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, interactionRange);

            IInteractable closestInteractable = null;
            float closestDistance = float.MaxValue;
        
            foreach (var coll in colliders)
            {
                IInteractable interactable = coll.GetComponent<IInteractable>();
                if (interactable == null || !interactable.CanInteract(gameObject)) continue;
                float distance = Vector2.Distance(transform.position, coll.transform.position);
                if (!(distance < closestDistance)) continue;
                if (coll.GetComponent<BackpackFrameObject>() != null) { if (context != "bag") { continue; } }
                closestDistance = distance;
                closestInteractable = interactable;
            }
        
            return closestInteractable;
        }
        
        private void HandleQuestLogToggle()
        {
            if (!ShouldProcessUIInput()) return;
            uiService?.ToggleQuestTab();
        }

        private void HandlePauseMenuToggle()
        {
            if (!ShouldProcessUIInput()) return;
            uiService?.TogglePauseMenu();
        }

        private void HandleRoutePlanner()
        {
            if (!ShouldProcessUIInput()) return;
            uiService?.ToggleRoutePlanner();
        }

        private void HandleInventoryToggle()
        {
            if (!ShouldProcessUIInput()) return;
            uiService?.ToggleInventoryTab();
        }

        private void HandleBagToggle() { HandleInteract("bag"); }

        private void HandleEscape()
        {
            if (!ShouldProcessUIInput()) return;
            bool handled = uiService.HandleEscapeInput();
        }
        
        private bool ShouldProcessUIInput()
        {
            if (!IsSubscribed) return false;
            
            var currentState = GameStateManager.Instance.CurrentState;
            return currentState switch
            {
                GameState.Exploration => true,
                GameState.UI => true,
                GameState.Dialogue => false,  // Don't interrupt dialogue
                GameState.Cutscene => false,  // Don't interrupt cutscenes
                _ => false
            };
        }

        private void HandleInteractRelease() { if (isInteracting) { currentInteractable?.CancelInteraction(); } }
    
        private void OnInteractionComplete()
        {
            var activeItem = GetComponent<PlayerQuickSlots>().GetActiveItem();
            Debug.Log("Active item: " + activeItem);
            Debug.Log("Current interactable: " + currentInteractable.GetGameObject());
            ProcessItemBehaviors(activeItem, UseType.Interact, currentInteractable.GetGameObject());
            ResetInteractionState();
        }

        private void OnInteractionCancelled()
        {
            ResetInteractionState();
        }

        private void ResetInteractionState()
        {
            isInteracting = false; currentInteractable = null;
            Debug.Log("Interaction state reset.");
        }
    }
}

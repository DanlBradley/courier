using System;
using System.Collections;
using GameServices;
using Interfaces;
using UnityEngine;

namespace EnvironmentTools
{
    public class TimedInteraction : MonoBehaviour, IInteractable
    {
        [SerializeField] private float holdDuration = 1f;
        [SerializeField] private string interactionText = "Hold to interact";
        [SerializeField] private bool canInteract = true;
        
        private Coroutine currentInteraction;
        private Action onCompleteCallback;
        private Action onCancelCallback;
        private UIService uiService;
        
        // Single event that handles everything the UI needs
        public event Action<InteractionEvent> OnInteractionUpdate;
        
        private void Awake() 
        { 
            uiService = ServiceLocator.GetService<UIService>(); 
        }

        public bool CanInteract(GameObject interactor) 
        {
            var handler = GetComponent<ITimedInteractionHandler>();
            return canInteract && handler?.CanPerformInteraction(interactor) != false;
        }

        public void StartInteraction(GameObject interactor, Action onComplete, Action onCancel)
        {
            if (!CanInteract(interactor)) return;
            
            onCompleteCallback = onComplete;
            onCancelCallback = onCancel;
            
            uiService?.StartTimedProgress(this);
            currentInteraction = StartCoroutine(HoldInteractionCoroutine(interactor));
        }

        public void CancelInteraction()
        {
            if (currentInteraction == null) return;
            
            StopCoroutine(currentInteraction);
            currentInteraction = null;
            
            // Tell UI we're cancelled
            OnInteractionUpdate?.Invoke(new InteractionEvent(InteractionState.Cancelled));
            uiService?.StopTimedProgress();
            onCancelCallback?.Invoke();
        }
        
        private IEnumerator HoldInteractionCoroutine(GameObject interactor)
        {
            float elapsed = 0f;
            while (elapsed < holdDuration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / holdDuration;
                
                // Single event with progress info
                OnInteractionUpdate?.Invoke(new InteractionEvent(InteractionState.InProgress, progress));
                yield return null;
            }
            
            CompleteInteraction(interactor);
        }
        
        private void CompleteInteraction(GameObject interactor)
        {
            // Tell UI we're completed
            OnInteractionUpdate?.Invoke(new InteractionEvent(InteractionState.Completed));
            
            uiService?.StopTimedProgress();
            onCompleteCallback?.Invoke();
            
            var handler = GetComponent<ITimedInteractionHandler>();
            handler?.OnInteractionComplete(interactor);
        }

        public GameObject GetGameObject() { return gameObject; }
        
        public void SetHoldDuration(float duration) { holdDuration = duration; }
        public void SetInteractionText(string text) { interactionText = text; }
        public void SetCanInteract(bool canInteractValue) { canInteract = canInteractValue; }
    }
    
    public enum InteractionState
    {
        InProgress,
        Completed,
        Cancelled
    }

    public struct InteractionEvent
    {
        public InteractionState state;
        public float progress; // 0.0 to 1.0, only relevant for InProgress
        
        public InteractionEvent(InteractionState state, float progress = 0f)
        {
            this.state = state;
            this.progress = progress;
        }
    }
}
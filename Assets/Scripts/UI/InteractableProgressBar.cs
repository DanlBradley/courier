using System;
using UnityEngine;
using EnvironmentTools;

namespace UI
{
    /// <summary>
    /// This manages a circular progress bar that updates based on events from TimedInteraction components
    /// </summary>
    [RequireComponent(typeof(CircularProgressBar))]
    public class InteractableProgressBar : MonoBehaviour
    {
        private CircularProgressBar progressBar;
        private TimedInteraction currentInteraction;
        
        private void Awake() 
        { 
            progressBar = GetComponent<CircularProgressBar>(); 
            gameObject.SetActive(false); 
        }
        
        public void StartTracking(TimedInteraction interaction)
        {
            StopTracking();
            currentInteraction = interaction;
            currentInteraction.OnInteractionUpdate += HandleInteractionUpdate;
            ShowProgressBar();
        }
        
        public void StopTracking()
        {
            if (currentInteraction != null)
            {
                currentInteraction.OnInteractionUpdate -= HandleInteractionUpdate;
                currentInteraction = null;
            }
            HideProgressBar();
        }
        
        private void HandleInteractionUpdate(InteractionEvent evt)
        {
            switch (evt.state)
            {
                case InteractionState.InProgress:
                    UpdateProgress(evt.progress);
                    break;
                case InteractionState.Completed:
                case InteractionState.Cancelled:
                    HideProgressBar();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        private void ShowProgressBar() 
        { 
            gameObject.SetActive(true); 
            progressBar.SetFill(0f); 
        }
        
        private void UpdateProgress(float progress) { progressBar.SetFill(progress); }
        
        private void HideProgressBar() { gameObject.SetActive(false); }
    }
}
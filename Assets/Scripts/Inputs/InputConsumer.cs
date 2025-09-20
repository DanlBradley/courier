using GameServices;
using UnityEngine;

namespace Inputs
{
    /// <summary>
    /// Base class for all components that consume input events from the InputManager.
    /// Handles subscribing/unsubscribing in a consistent way and manages initialization timing.
    /// </summary>
    public abstract class InputConsumer : MonoBehaviour
    {
        protected bool IsSubscribed { get; private set; }
        
        protected virtual void OnEnable()
        {
            GameManager.Instance.OnManagersInitialized += SubscribeToInputs;
            SubscribeToInputs();
        }
        
        protected virtual void OnDisable()
        {
            GameManager.Instance.OnManagersInitialized -= SubscribeToInputs;
            UnsubscribeFromInputs();
        }
        
        private void SubscribeToInputs()
        {
            if (IsSubscribed || ServiceLocator.GetService<InputManager>() == null) return;
            SubscribeToEvents();
            IsSubscribed = true;
        }
        
        private void UnsubscribeFromInputs()
        {
            if (!IsSubscribed || ServiceLocator.GetService<InputManager>() == null) return;
            UnsubscribeFromEvents();
            IsSubscribed = false;
        }

        protected abstract void SubscribeToEvents();
        protected abstract void UnsubscribeFromEvents();

        protected bool ShouldProcessInput(GameState requiredState = GameState.Exploration)
        {
            Debug.Log("Should process input? ");
            Debug.Log(IsSubscribed);
            Debug.Log(GameStateManager.Instance.CurrentState);
            return IsSubscribed && GameStateManager.Instance.CurrentState == requiredState;
        }
    }
}
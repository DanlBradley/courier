using System;
using System.Collections.Generic;
using System.Linq;
using GameServices;
using UnityEngine;

namespace UI.States
{
    public class UIStateManager : MonoBehaviour
    {
        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = true;
        
        // Current state management
        private UIState _currentState;
        private Dictionary<Type, UIState> _stateInstances;
        private UIOverlayFlags _activeOverlays = UIOverlayFlags.None;
        
        // Events
        public event Action<UIState, UIState> OnStateChanged;
        public event Action<UIOverlayFlags> OnOverlaysChanged;
        
        // References
        private UIService _uiService;
        
        public void Initialize(UIService uiService)
        {
            _uiService = uiService;
            InitializeStates();
            TransitionToState<ExplorationUIState>();
        }
        
        private void Update() { _currentState?.OnUpdate(); }
        
        private void InitializeStates()
        {
            _stateInstances = new Dictionary<Type, UIState>();
            
            // Find all UIState classes and instantiate them
            var stateTypes = System.Reflection.Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => t.IsSubclassOf(typeof(UIState)) && !t.IsAbstract);
            
            foreach (var stateType in stateTypes)
            {
                var stateInstance = (UIState)Activator.CreateInstance(stateType);
                stateInstance.Initialize(_uiService);
                _stateInstances[stateType] = stateInstance;
            }
        }


        public void TransitionToState<T>(object parameters = null) where T : UIState, new()
        {
            var stateType = typeof(T);
            // Debug.Log($"Transitioning to state {stateType.Name}...");
            if (!_stateInstances.ContainsKey(stateType))
            { Debug.LogError($"State {stateType.Name} not found in state instances!"); return; }
            
            var newState = _stateInstances[stateType];
            
            // Check if transition is allowed
            if (_currentState != null && !_currentState.CanTransitionTo(newState))
            { Debug.LogWarning($"Transition from " +
                               $"{_currentState.StateName} to {newState.StateName} not allowed"); return; }
            
            if (parameters != null && newState is IParameterizedState paramState) 
            { paramState.SetParameters(parameters); }
            
            // Handle state switch
            var oldState = _currentState;
            _currentState?.OnExit();
            _currentState = newState;
            _currentState.OnEnter();
            
            OnStateChanged?.Invoke(oldState, _currentState);
            
            if (showDebugInfo) { Debug.Log($"UI State: {oldState?.StateName ?? "None"} → {_currentState.StateName}"); }
        }
        
        // Overlay management
        private void SetOverlay(UIOverlayFlags overlay, bool active)
        {
            var oldOverlays = _activeOverlays;
            
            if (active) _activeOverlays |= overlay;
            else _activeOverlays &= ~overlay;

            if (oldOverlays == _activeOverlays) return;
            OnOverlaysChanged?.Invoke(_activeOverlays);
                
            if (showDebugInfo)
            { Debug.Log($"UI Overlays: {oldOverlays} → {_activeOverlays}"); }
        }
        
        public void ToggleOverlay(UIOverlayFlags overlay) { SetOverlay(overlay, !HasOverlay(overlay)); }
        
        public bool HasOverlay(UIOverlayFlags overlay) { return (_activeOverlays & overlay) != 0; }
        
        public bool HandleEscapeInput()
        {
            Debug.Log("UI State Manager - Attempting to handle escape. Current state: " + _currentState.GetType());
            // First, try to close any active overlays
            if (_activeOverlays != UIOverlayFlags.None)
            {
                ClearAllOverlays();
                return true;
            }
        
            // If no overlays, let the current state handle escape
            return _currentState?.HandleInput("Cancel") ?? false;
        }
    
        private void ClearAllOverlays()
        {
            if (_activeOverlays == UIOverlayFlags.None) return;
            SetOverlay(UIOverlayFlags.QuestLog | UIOverlayFlags.Inventory, false);
        }
        public bool HandleInput(string inputAction) { return _currentState?.HandleInput(inputAction) ?? false; }
        
        public UIState CurrentState => _currentState;
        public UIOverlayFlags ActiveOverlays => _activeOverlays;
        public T GetState<T>() where T : UIState
        { return _stateInstances.TryGetValue(typeof(T), out var state) ? (T)state : null; }
    }
}
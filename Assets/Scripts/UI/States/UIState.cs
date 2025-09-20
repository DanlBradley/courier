using System;
using GameServices;

namespace UI.States
{
    public abstract class UIState
    {
        public abstract string StateName { get; }
        protected UIService uiService;
        
        public virtual void Initialize(UIService service)
        {
            uiService = service;
        }
        
        public virtual bool CanTransitionTo(UIState newState) => true;
        public virtual void OnEnter() { }
        public virtual void OnExit() { }
        public virtual void OnUpdate() { }
        
        // Handle input for this state
        public virtual bool HandleInput(string inputAction) => false;
    }
    
    [Flags]
    public enum UIOverlayFlags
    {
        None = 0,
        QuestLog = 1 << 0,
        Inventory = 1 << 1,
    }
    
    public interface IParameterizedState { void SetParameters(object parameters); }
}
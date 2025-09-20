using System;
using Inputs;
using UnityEngine;

namespace GameServices
{
    public enum GameState
    {
        Exploration,
        Dialogue,
        UI,
        Cutscene
    }

    public class GameStateManager : Service
    {
        public static GameStateManager Instance { get; private set; }
        public GameState CurrentState { get; private set; } = GameState.Exploration;
    
        // Event that other systems can subscribe to
        public event Action<GameState, GameState> OnGameStateChanged;
        
        public override void Initialize()
        {
            Cursor.lockState = CursorLockMode.Locked;
            if (Instance == null) { Instance = this; }
            else { Destroy(gameObject); }
        }
    
        public void ChangeState(GameState newState)
        {
            if (CurrentState == newState) return;
        
            GameState oldState = CurrentState;
            CurrentState = newState;
            ServiceLocator.GetService<InputManager>().SwitchActionMap(newState);
            OnGameStateChanged?.Invoke(oldState, newState);
            SetCursorLock(CurrentState == GameState.Exploration);
            Debug.Log($"Game State changed from {oldState} to {newState}");
        }
        
        private void SetCursorLock(bool locked)
        {
            if (locked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
        
                // Force focus in editor
                #if UNITY_EDITOR
                System.Reflection.Assembly assembly = typeof(UnityEditor.EditorWindow).Assembly;
                Type type = assembly.GetType("UnityEditor.GameView");
                UnityEditor.EditorWindow gameview = UnityEditor.EditorWindow.GetWindow(type);
                gameview.Focus();
                #endif
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }
}
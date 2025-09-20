using System;
using GameServices;
using UnityEngine;
using UnityEngine.InputSystem;
using Utils;

namespace Inputs
{
    public class InputManager : Service
    {
        private InputActions inputActions;
        
        public event Action<Vector2> OnMoveInput;
        public event Action<string> OnInteractInput;
        public event Action OnInteractInputCanceled;
        public event Action<int> OnDialogueChoiceInput;
        public event Action OnContinueDialogueInput;
        public event Action OnInventoryToggleInput;
        public event Action OnBagToggleInput;
        public event Action OnQuestLogToggleInput;
        public event Action OnRoutePlannerToggleInput;
        public event Action OnEscapeInput;
        public event Action<int> OnHotkeyInput;
        public event Action OnQuickUseInput;
        public event Action<Vector2> OnMouseDelta;
        public event Action<bool> OnSprintInput;
        public event Action<bool> OnCrouchInput;
        public event Action OnJumpInput;
        
        // Combat events
        public event Action OnPrimaryAttackInput;
        public event Action OnPauseMenuToggleInput;
        public event Action OnSecondaryAttackInput;
        public event Action<bool> OnBlockInput;
        public event Action OnWeaponSwitchInput;
        
        public override void Initialize()
        {
            inputActions = new InputActions();
            
            inputActions.Exploration.Move.performed += OnMovePerformed;
            inputActions.Exploration.Move.canceled += OnMoveCanceled;
            inputActions.Exploration.Interact.performed += OnInteractPerformed;
            inputActions.Exploration.Interact.canceled += OnInteractCanceled;
            inputActions.Exploration.ToggleQuestLog.performed += OnToggleQuestLogPerformed;
            inputActions.Exploration.ToggleRoutePlanner.performed += OnToggleRoutePlannerPerformed;
            inputActions.Exploration.Inventory.performed += OnInventoryTogglePerformed;
            inputActions.Exploration.Bag.performed += OnToggleBagPerformed;
            inputActions.Exploration.Escape.performed += OnEscapePerformed;
            inputActions.Exploration.Hotkey1.performed += OnHotkey1Performed;
            inputActions.Exploration.Hotkey2.performed += OnHotkey2Performed;
            inputActions.Exploration.Hotkey3.performed += OnHotkey3Performed;
            inputActions.Exploration.Hotkey4.performed += OnHotkey4Performed;
            inputActions.Exploration.QuickUse.performed += OnQuickUsePerformed;
            inputActions.Exploration.MouseDelta.performed += OnMouseDeltaPerformed;
            inputActions.Exploration.Sprint.performed += OnSprintPerformed;
            inputActions.Exploration.Sprint.canceled += OnSprintCanceled;
            inputActions.Exploration.Crouch.performed += OnCrouchPerformed;
            inputActions.Exploration.Crouch.canceled += OnCrouchCanceled;
            inputActions.Exploration.Jump.performed += OnJumpPerformed;
            inputActions.Exploration.TogglePauseMenu.performed += OnPauseMenuTogglePerformed;
            
            inputActions.Exploration.PrimaryAttack.performed += OnPrimaryAttackPerformed;
            inputActions.Exploration.SecondaryAttack.performed += OnSecondaryAttackPerformed;
            inputActions.Exploration.Block.performed += OnBlockPerformed;
            inputActions.Exploration.Block.canceled += OnBlockCanceled;
            inputActions.Exploration.WeaponSwitch.performed += OnWeaponSwitchPerformed;
            
            inputActions.Dialogue.Choice1.performed += OnDialogueChoice1Performed;
            inputActions.Dialogue.Choice2.performed += OnDialogueChoice2Performed;
            inputActions.Dialogue.Choice3.performed += OnDialogueChoice3Performed;
            inputActions.Dialogue.Continue.performed += OnContinueDialoguePerformed;
            
            inputActions.CharacterScreen.ToggleInventory.performed += OnInventoryTogglePerformed;
            inputActions.CharacterScreen.Bag.performed += OnToggleBagPerformed;
            inputActions.CharacterScreen.ToggleQuestLog.performed += OnToggleQuestLogPerformed;
            inputActions.CharacterScreen.ToggleRoutePlanner.performed += OnToggleRoutePlannerPerformed;
            inputActions.CharacterScreen.Escape.performed += OnEscapePerformed;
            inputActions.CharacterScreen.Hotkey1.performed += OnHotkey1Performed;
            inputActions.CharacterScreen.Hotkey2.performed += OnHotkey2Performed;
            inputActions.CharacterScreen.Hotkey3.performed += OnHotkey3Performed;
            inputActions.CharacterScreen.Hotkey4.performed += OnHotkey4Performed;
            
            inputActions.Enable();
            SwitchActionMap(GameState.Exploration);
            Logs.Log("Input manager initialized.", "GameServices");
        }

        private void OnDestroy()
        {
            inputActions.Exploration.Move.performed -= OnMovePerformed;
            inputActions.Exploration.Move.canceled -= OnMoveCanceled;
            inputActions.Exploration.Interact.performed -= OnInteractPerformed;
            inputActions.Exploration.Interact.canceled -= OnInteractCanceled;
            inputActions.Exploration.Escape.performed -= OnEscapePerformed;
            inputActions.Exploration.ToggleQuestLog.performed -= OnToggleQuestLogPerformed;
            inputActions.Exploration.ToggleRoutePlanner.performed -= OnToggleRoutePlannerPerformed;
            inputActions.Exploration.Inventory.performed -= OnInventoryTogglePerformed;
            inputActions.Exploration.Bag.performed -= OnToggleBagPerformed;
            inputActions.Exploration.Hotkey1.performed -= OnHotkey1Performed;
            inputActions.Exploration.Hotkey2.performed -= OnHotkey2Performed;
            inputActions.Exploration.Hotkey3.performed -= OnHotkey3Performed;
            inputActions.Exploration.Hotkey4.performed -= OnHotkey4Performed;
            inputActions.Exploration.QuickUse.performed -= OnQuickUsePerformed;
            inputActions.Exploration.MouseDelta.performed -= OnMouseDeltaPerformed;
            inputActions.Exploration.Sprint.performed -= OnSprintPerformed;
            inputActions.Exploration.Sprint.canceled -= OnSprintCanceled;
            inputActions.Exploration.Crouch.performed -= OnCrouchPerformed;
            inputActions.Exploration.Crouch.canceled -= OnCrouchCanceled;
            inputActions.Exploration.Jump.performed -= OnJumpPerformed;

            inputActions.Exploration.PrimaryAttack.performed -= OnPrimaryAttackPerformed;
            inputActions.Exploration.SecondaryAttack.performed -= OnSecondaryAttackPerformed;
            inputActions.Exploration.Block.performed -= OnBlockPerformed;
            inputActions.Exploration.Block.canceled -= OnBlockCanceled;
            inputActions.Exploration.WeaponSwitch.performed -= OnWeaponSwitchPerformed;
            
            inputActions.Dialogue.Choice1.performed -= OnDialogueChoice1Performed;
            inputActions.Dialogue.Choice2.performed -= OnDialogueChoice2Performed;
            inputActions.Dialogue.Choice3.performed -= OnDialogueChoice3Performed;
            inputActions.Dialogue.Continue.performed -= OnContinueDialoguePerformed;
            
            inputActions.CharacterScreen.ToggleInventory.performed -= OnInventoryTogglePerformed;
            inputActions.CharacterScreen.Bag.performed += OnToggleBagPerformed;
            inputActions.CharacterScreen.ToggleQuestLog.performed -= OnToggleQuestLogPerformed;
            inputActions.CharacterScreen.ToggleRoutePlanner.performed -= OnToggleRoutePlannerPerformed;
            inputActions.CharacterScreen.Escape.performed -= OnEscapePerformed;
            inputActions.CharacterScreen.Hotkey1.performed -= OnHotkey1Performed;
            inputActions.CharacterScreen.Hotkey2.performed -= OnHotkey2Performed;
            inputActions.CharacterScreen.Hotkey3.performed -= OnHotkey3Performed;
            inputActions.CharacterScreen.Hotkey4.performed -= OnHotkey4Performed;
            
            // Disable and dispose
            inputActions.Disable();
            inputActions.Dispose();
        }
        
        public void SwitchActionMap(GameState state)
        {
            inputActions.Exploration.Disable();
            inputActions.Dialogue.Disable();
            inputActions.CharacterScreen.Disable();
    
            switch (state)
            {
                case GameState.Exploration:
                    inputActions.Exploration.Enable();
                    break;
                case GameState.Dialogue:
                    inputActions.Dialogue.Enable();
                    break;
                case GameState.UI:
                    inputActions.CharacterScreen.Enable();
                    break;
                case GameState.Cutscene:
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }
        
        #region Input Callbacks
        
        private void OnMovePerformed(InputAction.CallbackContext context)
        { OnMoveInput?.Invoke(context.ReadValue<Vector2>()); }
        private void OnMouseDeltaPerformed(InputAction.CallbackContext context)
        { OnMouseDelta?.Invoke(context.ReadValue<Vector2>()); }
        private void OnMoveCanceled(InputAction.CallbackContext context) { OnMoveInput?.Invoke(Vector2.zero); }
        private void OnToggleQuestLogPerformed(InputAction.CallbackContext obj) { OnQuestLogToggleInput?.Invoke(); }
        private void OnToggleRoutePlannerPerformed(InputAction.CallbackContext obj)
        { OnRoutePlannerToggleInput?.Invoke(); }
        private void OnInteractPerformed(InputAction.CallbackContext context) { OnInteractInput?.Invoke("na"); }
        private void OnInteractCanceled(InputAction.CallbackContext obj) { OnInteractInputCanceled?.Invoke(); }
        private void OnQuickUsePerformed(InputAction.CallbackContext context) { OnQuickUseInput?.Invoke(); }
        private void OnHotkey1Performed(InputAction.CallbackContext context) { OnHotkeyInput?.Invoke(1); }
        private void OnHotkey2Performed(InputAction.CallbackContext context) { OnHotkeyInput?.Invoke(2); }
        private void OnHotkey3Performed(InputAction.CallbackContext context) { OnHotkeyInput?.Invoke(3); }
        private void OnHotkey4Performed(InputAction.CallbackContext context) { OnHotkeyInput?.Invoke(4); }
        
        private void OnDialogueChoice1Performed(InputAction.CallbackContext context)
        { OnDialogueChoiceInput?.Invoke(0); }        
        private void OnDialogueChoice2Performed(InputAction.CallbackContext context)
        { OnDialogueChoiceInput?.Invoke(1); }        
        private void OnDialogueChoice3Performed(InputAction.CallbackContext context)
        { OnDialogueChoiceInput?.Invoke(2); }
        private void OnContinueDialoguePerformed(InputAction.CallbackContext context)
        { OnContinueDialogueInput?.Invoke(); }
        private void OnInventoryTogglePerformed(InputAction.CallbackContext context)
        { OnInventoryToggleInput?.Invoke(); }

        private void OnToggleBagPerformed(InputAction.CallbackContext context)
        { OnBagToggleInput?.Invoke(); }

        private void OnEscapePerformed(InputAction.CallbackContext context)
        { OnEscapeInput?.Invoke(); }
        
        private void OnSprintPerformed(InputAction.CallbackContext context)
        { OnSprintInput?.Invoke(true); }
        private void OnSprintCanceled(InputAction.CallbackContext context)
        { OnSprintInput?.Invoke(false); }
        
        private void OnCrouchPerformed(InputAction.CallbackContext context)
        { 
            OnCrouchInput?.Invoke(true); 
        }
        
        private void OnCrouchCanceled(InputAction.CallbackContext context)
        { 
            OnCrouchInput?.Invoke(false); 
        }
        
        private void OnJumpPerformed(InputAction.CallbackContext context) { OnJumpInput?.Invoke(); }

        private void OnPauseMenuTogglePerformed(InputAction.CallbackContext context) { OnPauseMenuToggleInput?.Invoke(); }
        
        private void OnPrimaryAttackPerformed(InputAction.CallbackContext context)
        { 
            OnPrimaryAttackInput?.Invoke(); 
        }
        
        private void OnSecondaryAttackPerformed(InputAction.CallbackContext context)
        { 
            OnSecondaryAttackInput?.Invoke(); 
        }
        
        private void OnBlockPerformed(InputAction.CallbackContext context)
        { OnBlockInput?.Invoke(true); }
        
        private void OnBlockCanceled(InputAction.CallbackContext context)
        { OnBlockInput?.Invoke(false); }
        
        
        private void OnWeaponSwitchPerformed(InputAction.CallbackContext context)
        { OnWeaponSwitchInput?.Invoke(); }
        
        #endregion

        
    }
}
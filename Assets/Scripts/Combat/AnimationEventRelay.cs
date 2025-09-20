using Combat.Weapons;
using UnityEngine;

namespace Combat
{
    /// <summary>
    /// Relays animation events from weapon rig animator to parent CombatController.
    /// Place this on the GameObject with the Animator (e.g., RightHand).
    /// </summary>
    public class AnimationEventRelay : MonoBehaviour
    {
        private CombatController combatController;
        private WeaponController weaponController;
        
        private void Awake()
        {
            // Find CombatController on parent hierarchy
            combatController = GetComponentInParent<CombatController>();
            
            if (combatController == null)
            {
                Debug.LogError($"AnimationEventRelay on {gameObject.name} couldn't find CombatController in parent hierarchy!");
            }
            
            // Cache weapon controller if present
            weaponController = GetComponentInChildren<WeaponController>();
        }
        
        // Relay animation events to CombatController
        
        public void OnAttackHitboxActive()
        {
            var animator = GetComponent<Animator>();
            if (animator != null)
            {
                var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                Debug.Log($"[AnimationEventRelay] OnAttackHitboxActive - Layer 0 normalized time: {stateInfo.normalizedTime}");
                Debug.Log($"[AnimationEventRelay] Is in 'Attack' state: {stateInfo.IsName("Attack")}");
                Debug.Log($"[AnimationEventRelay] Is in 'FP_Attack1' state: {stateInfo.IsName("FP_Attack1")}");
            }
            if (combatController != null)
            {
                combatController.OnAttackHitboxActive();
            }
        }
        
        public void OnAttackHitboxInactive()
        {
            if (combatController != null)
            {
                combatController.OnAttackHitboxInactive();
            }
        }
        
        public void OnAttackComplete()
        {
            if (combatController != null)
            {
                combatController.OnAttackComplete();
            }
        }
        
        // Optional: Relay block events if needed
        
        public void OnBlockStart()
        {
            if (combatController != null)
            {
                // combatController.StartBlocking();
            }
        }
        
        public void OnBlockEnd()
        {
            if (combatController != null)
            {
                // combatController.StopBlocking();
            }
        }
        
        // Optional: Add any weapon-specific events
        
        public void OnWeaponTrailStart()
        {
            // Could enable weapon trail renderer here
            if (weaponController != null)
            {
                // weaponController.EnableTrail();
            }
        }
        
        public void OnWeaponTrailEnd()
        {
            // Could disable weapon trail renderer here
            if (weaponController != null)
            {
                // weaponController.DisableTrail();
            }
        }
        
        // Debug helper
        public void TestEvent(string message)
        {
            Debug.Log($"Animation Event Received: {message}");
            
            if (combatController != null)
            {
                Debug.Log($"CombatController found on: {combatController.gameObject.name}");
            }
        }
    }
}
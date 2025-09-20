using System;
using System.Collections.Generic;
using Character;
using Combat;
using Combat.Weapons;
using GameServices;
using Interfaces;
using StatusEffects;
using UnityEngine;
using Inputs;

namespace Combat
{
    [RequireComponent(typeof(CharacterStatus))]
    public class CombatController : MonoBehaviour, IDamageable, IDamageDealer
    {
        [Header("Combat Configuration")]
        [SerializeField] private CombatConfig combatConfig;
        [SerializeField] private Team team = Team.Enemy;
        
        [Header("Weapon Slots")]
        [SerializeField] private Transform mainHandSlot;
        [SerializeField] private Transform offHandSlot;
        
        [Header("Combat State")]
        [SerializeField] private bool isBlocking = false;
        [SerializeField] private bool isAttacking = false;
        [SerializeField] private float lastAttackTime = 0f;
        
        private CharacterStatus characterStatus;
        private StatusEffectManager statusEffectManager;
        private Animator animator;
        private IWeapon currentWeapon;
        private CombatService combatService;
        private InputManager inputManager;
        private Rigidbody cachedRigidbody;
        private int currentComboIndex = 0;
        private float comboResetTimer = 0f;
        private const float COMBO_RESET_TIME = 1.5f;
        private bool isPlayer = false;
        
        // IDamageable Implementation
        public Team Team => team;
        public bool IsAlive => characterStatus != null && characterStatus.Health > 0;
        public bool IsBlocking => isBlocking;
        
        // Events
        public event Action<DamageInfo> OnDamageTaken;
        public event Action<float> OnHealed;
        public event Action OnDeath;
        public event Action<DamageInfo> OnDamageDealt;
        
        private void Awake()
        {
            characterStatus = GetComponent<CharacterStatus>();
            statusEffectManager = GetComponent<StatusEffectManager>();
            cachedRigidbody = GetComponent<Rigidbody>();
            
            // Try to get animator on this object first, then check children (for weapon rigs)
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }
            
            // Determine if this is the player based on tag or team
            isPlayer = gameObject.CompareTag("Player") || team == Team.Player;
            
            
            if (combatConfig == null)
            {
                Debug.LogWarning($"CombatController on {gameObject.name} is missing CombatConfig!");
            }
        }
        
        private void Start()
        {
            combatService = ServiceLocator.GetService<CombatService>();
            
            // Get input manager for player characters
            if (isPlayer)
            {
                inputManager = ServiceLocator.GetService<InputManager>();
            }
            
            // Auto-detect weapon in children if not already set
            if (currentWeapon == null)
            {
                currentWeapon = GetComponentInChildren<IWeapon>();
                if (currentWeapon != null)
                {
                }
            }
        }
        
        private void OnEnable()
        {
            // Try to get InputManager again in case it wasn't available in Start
            if (isPlayer && inputManager == null)
            {
                inputManager = ServiceLocator.GetService<InputManager>();
                Debug.Log($"[CombatController] OnEnable - Retry getting InputManager: {inputManager != null}");
            }
            
            if (isPlayer && inputManager != null)
            {
                Debug.Log($"[CombatController] OnEnable - Subscribing to input events");
                inputManager.OnPrimaryAttackInput += HandlePrimaryAttack;
                inputManager.OnSecondaryAttackInput += HandleSecondaryAttack;
                inputManager.OnBlockInput += HandleBlock;
                inputManager.OnWeaponSwitchInput += HandleWeaponSwitch;
            }
            else
            {
                Debug.Log($"[CombatController] OnEnable - NOT subscribing (isPlayer: {isPlayer}, inputManager: {inputManager})");
            }
        }
        
        private void OnDisable()
        {
            if (isPlayer && inputManager != null)
            {
                inputManager.OnPrimaryAttackInput -= HandlePrimaryAttack;
                inputManager.OnSecondaryAttackInput -= HandleSecondaryAttack;
                inputManager.OnBlockInput -= HandleBlock;
                inputManager.OnWeaponSwitchInput -= HandleWeaponSwitch;
            }
        }
        
        private void OnDestroy()
        {
            // No cleanup needed since we don't register with CombatService anymore
        }
        
        private void Update()
        {
            UpdateComboTimer();
        }
        
        private void UpdateComboTimer()
        {
            if (comboResetTimer > 0f)
            {
                comboResetTimer -= Time.deltaTime;
                if (comboResetTimer <= 0f)
                {
                    currentComboIndex = 0;
                }
            }
        }
        
        // IDamageable and IDamageDealer Implementation
        public Vector3 GetBlockDirection()
        {
            return transform.forward;
        }
        
        public float GetDefense()
        {
            float baseDefense = combatConfig != null ? combatConfig.baseDefense : 5f;
            
            if (statusEffectManager != null)
            {
                var (additive, multiplicative, overrideValue) = statusEffectManager.GetModifierValues("Defense");
                if (overrideValue.HasValue)
                    return overrideValue.Value;
                return (baseDefense + additive) * multiplicative;
            }
            
            return baseDefense;
        }
        
        public float GetAttackPower()
        {
            float basePower = combatConfig != null ? combatConfig.baseAttackPower : 10f;
            
            if (statusEffectManager != null)
            {
                var (additive, multiplicative, overrideValue) = statusEffectManager.GetModifierValues("AttackPower");
                if (overrideValue.HasValue)
                    return overrideValue.Value;
                return (basePower + additive) * multiplicative;
            }
            
            return basePower;
        }
        
        
        public IWeapon GetCurrentWeapon()
        {
            return currentWeapon;
        }
        
        private float GetAttackSpeed()
        {
            float baseSpeed = combatConfig != null ? combatConfig.baseAttackSpeed : 1f;
            
            if (statusEffectManager != null)
            {
                var (additive, multiplicative, overrideValue) = statusEffectManager.GetModifierValues("AttackSpeed");
                if (overrideValue.HasValue)
                    return overrideValue.Value;
                return (baseSpeed + additive) * multiplicative;
            }
            
            return baseSpeed;
        }
        
        private bool CanAttack()
        {
            if (!IsAlive || isAttacking || isBlocking)
                return false;
                
            float attackCooldown = 1f / GetAttackSpeed();
            if (Time.time - lastAttackTime < attackCooldown)
                return false;
                
            float energyCost = combatConfig != null ? combatConfig.lightAttackEnergy : 10f;
            if (currentWeapon != null)
                energyCost *= currentWeapon.EnergyCost;
                
            return characterStatus.Energy >= energyCost;
        }
        
        // Combat Actions
        public void ExecuteAttack(int attackIndex = 0)
        {
            if (!CanAttack())
            {
                // Attack blocked - could be due to energy, already attacking, or blocking
                return;
            }
                
            isAttacking = true;
            lastAttackTime = Time.time;
            
            float energyCost = combatConfig != null ? combatConfig.lightAttackEnergy : 10f;
            if (currentWeapon != null)
            {
                energyCost *= currentWeapon.EnergyCost;
                currentWeapon.StartAttack(attackIndex);
            }
            
            ConsumeEnergy(energyCost);
            
            if (animator != null)
            {
                Debug.Log($"[CombatController] Setting Attack trigger on {animator.name} (GameObject: {animator.gameObject.name})");
                animator.ResetTrigger("Attack"); // Clear any pending triggers first
                animator.SetTrigger("Attack");
                animator.SetInteger("AttackIndex", attackIndex);
                Debug.Log($"[CombatController] Attack trigger set. Current state: {animator.GetCurrentAnimatorStateInfo(0).IsName("Attack")}");
            }
            else
            {
                Debug.LogWarning("[CombatController] No animator found!");
            }
            
            RaiseCombatEvent(new CombatEvent(
                CombatEventType.AttackStarted,
                gameObject,
                null,
                GetAttackPower(),
                transform.position
            ));
        }
        
        public void ExecuteCombo()
        {
            if (!CanAttack())
                return;
                
            ExecuteAttack(currentComboIndex);
            currentComboIndex++;
            comboResetTimer = COMBO_RESET_TIME;
            
            if (currentWeapon != null && currentWeapon is WeaponController weaponController)
            {
                // Check if we've exceeded max combo length
                // This would need to be implemented based on weapon's combo pattern
                currentComboIndex = currentComboIndex % 3; // Default 3-hit combo
            }
        }
        
        public void BlockStart()
        {
            if (!IsAlive || isAttacking)
                return;
                
            float blockEnergyCost = combatConfig != null ? combatConfig.blockEnergy : 5f;
            if (characterStatus.Energy < blockEnergyCost)
                return;
                
            isBlocking = true;
            ConsumeEnergy(blockEnergyCost);
            
            if (animator != null)
            {
                animator.SetBool("IsBlocking", true);
            }
        }
        
        public void BlockEnd()
        {
            isBlocking = false;
            
            if (animator != null)
            {
                animator.SetBool("IsBlocking", false);
            }
        }
        
        // IDamageable Methods
        public void TakeDamage(DamageInfo damageInfo)
        {
            if (!IsAlive)
                return;
            
            float finalDamage = damageInfo.damage;
            
            // Apply defense
            if (!damageInfo.wasBlocked)
            {
                float defense = GetDefense();
                finalDamage = Mathf.Max(0, finalDamage - defense);
            }
            else
            {
                // Blocked attacks deal reduced damage
                finalDamage *= 0.2f;
            }
            
            // Apply damage to health
            characterStatus.UpdateVital(-finalDamage, Vitals.Health);
            
            // Apply knockback if not blocked
            if (!damageInfo.wasBlocked && damageInfo.knockbackForce > 0)
            {
                ApplyKnockback(damageInfo.hitDirection, damageInfo.knockbackForce);
            }
            
            // Apply status effects
            if (damageInfo.statusEffectIds != null && statusEffectManager != null)
            {
                var statusEffectService = ServiceLocator.GetService<StatusEffectService>();
                if (statusEffectService != null)
                {
                    foreach (string effectId in damageInfo.statusEffectIds)
                    {
                        statusEffectService.ApplyEffectToTarget(gameObject, effectId);
                    }
                }
            }
            
            OnDamageTaken?.Invoke(damageInfo);
            
            RaiseCombatEvent(new CombatEvent(
                CombatEventType.DamageTaken,
                damageInfo.source,
                gameObject,
                finalDamage,
                damageInfo.hitPoint
            ));
            
            // Check for death
            if (characterStatus.Health <= 0)
            {
                Die();
            }
            else if (animator != null && !damageInfo.wasBlocked)
            {
                // Play hit reaction
                animator.SetTrigger("Hit");
            }
        }
        
        public void Heal(float amount)
        {
            if (!IsAlive)
                return;
                
            characterStatus.UpdateVital(amount, Vitals.Health);
            OnHealed?.Invoke(amount);
        }
        
        // IDamageDealer Methods
        public void DealDamage(IDamageable target, DamageInfo damageInfo)
        {
            if (target == null)
                return;
                
            damageInfo.source = gameObject;
            target.TakeDamage(damageInfo);
            
            OnDamageDealt?.Invoke(damageInfo);
            
            RaiseCombatEvent(new CombatEvent(
                CombatEventType.DamageDealt,
                gameObject,
                (target as Component)?.gameObject,
                damageInfo.damage,
                damageInfo.hitPoint
            ));
        }
        
        public float CalculateDamage(AttackInfo attackInfo)
        {
            float baseDamage = GetAttackPower();
            
            if (attackInfo.weapon != null)
            {
                baseDamage += attackInfo.weapon.BaseDamage;
            }
            
            // Apply charge multiplier if applicable
            if (attackInfo.chargeAmount > 0 && combatConfig != null)
            {
                float chargeMultiplier = combatConfig.damageMultiplierByCharge.Evaluate(attackInfo.chargeAmount);
                baseDamage *= chargeMultiplier;
            }
            
            return baseDamage;
        }
        
        // Energy Management
        private bool ConsumeEnergy(float amount)
        {
            if (characterStatus.Energy < amount)
                return false;
                
            characterStatus.UpdateVital(-amount, Vitals.Energy);
            return true;
        }
        
        
        // Animation Events (called from animation clips)
        public void OnAttackHitboxActive()
        {
            if (currentWeapon != null && currentWeapon is WeaponController weaponController)
            {
                weaponController.ActivateHitbox();
            }
        }
        
        public void OnAttackHitboxInactive()
        {
            if (currentWeapon != null && currentWeapon is WeaponController weaponController)
            {
                weaponController.DeactivateHitbox();
            }
        }
        
        public void OnAttackComplete()
        {
            isAttacking = false;
            
            if (currentWeapon != null)
            {
                currentWeapon.CancelAttack();
            }
        }
        
        // Utility Methods
        private void ApplyKnockback(Vector3 direction, float force)
        {
            var rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(direction * force, ForceMode.Impulse);
            }
        }
        
        private void Die()
        {
            OnDeath?.Invoke();
            
            RaiseCombatEvent(new CombatEvent(
                CombatEventType.Death,
                gameObject,
                null,
                0,
                transform.position
            ));
            
            if (animator != null)
            {
                animator.SetTrigger("Death");
            }
            
            // Disable combat functionality
            enabled = false;
        }
        
        private void RaiseCombatEvent(CombatEvent combatEvent)
        {
            if (combatService != null)
            {
                combatService.RaiseCombatEvent(combatEvent);
            }
        }
        
        // Public utility methods
        public void SetWeapon(IWeapon weapon)
        {
            currentWeapon = weapon;
        }
        
        public void SetCombatConfig(CombatConfig config)
        {
            combatConfig = config;
        }
        
        public void SetTeam(Team newTeam)
        {
            team = newTeam;
        }
        
        #region Input Handlers
        
        private void HandlePrimaryAttack()
        {
            if (!isPlayer || !IsAlive)
                return;
            
            // Use combo system for primary attacks
            ExecuteCombo();
        }
        
        private void HandleSecondaryAttack()
        {
            if (!isPlayer || !IsAlive)
                return;
                
            // Heavy attack - could be index 1 or special attack pattern
            float heavyAttackEnergy = combatConfig != null ? combatConfig.heavyAttackEnergy : 25f;
            if (characterStatus.Energy >= heavyAttackEnergy)
            {
                ExecuteAttack(3); // Index 3 for heavy attacks
            }
        }
        
        private void HandleBlock(bool isBlocking)
        {
            if (!isPlayer || !IsAlive)
                return;
                
            if (isBlocking)
            {
                BlockStart();
            }
            else
            {
                BlockEnd();
            }
        }
        
        
        private void HandleWeaponSwitch()
        {
            if (!isPlayer || !IsAlive || isAttacking)
                return;
                
            // Weapon switching implementation will be added when inventory integration is complete
            Debug.Log("Weapon switch input received - implementation pending");
        }
        
        #endregion
    }
}
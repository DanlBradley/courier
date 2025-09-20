using System;
using System.Collections;
using System.Collections.Generic;
using Combat;
using GameServices;
using Interfaces;
using Items;
using UnityEngine;

namespace Combat.Weapons
{
    public abstract class WeaponController : MonoBehaviour, IWeapon
    {
        [Header("Weapon Configuration")]
        [SerializeField] protected WeaponDefinition weaponDef;
        
        [Header("Hitbox Settings")]
        [SerializeField] protected Collider[] hitboxes;
        [SerializeField] protected LayerMask hitLayers = -1;
        [SerializeField] protected float minHitVelocity = 1f;
        
        [Header("Visual/Audio")]
        [SerializeField] protected Transform weaponModel;
        [SerializeField] protected AudioSource audioSource;
        
        protected Animator weaponAnimator;
        protected CombatController ownerCombatController;
        protected CombatService combatService;
        protected bool isAttacking;
        protected int currentComboIndex;
        protected HashSet<Collider> hitThisSwing;
        protected Vector3 lastPosition;
        protected float swingVelocity;
        
        // IWeapon Implementation
        public WeaponType WeaponType => weaponDef != null ? weaponDef.weaponType : WeaponType.Fist;
        public float BaseDamage => weaponDef != null ? weaponDef.baseDamage : 5f;
        public float AttackSpeed => weaponDef != null ? weaponDef.attackSpeed : 1f;
        public float Range => weaponDef != null ? weaponDef.range : 1f;
        public float EnergyCost => weaponDef != null ? weaponDef.energyCostMultiplier : 1f;
        public bool IsAttacking => isAttacking;
        
        public event Action<AttackInfo> OnAttackExecuted;
        
        protected virtual void Awake()
        {
            hitThisSwing = new HashSet<Collider>();
            weaponAnimator = GetComponent<Animator>();
            
            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();
                
            // Get owner's combat controller
            Transform current = transform;
            while (current != null && ownerCombatController == null)
            {
                ownerCombatController = current.GetComponent<CombatController>();
                current = current.parent;
            }
            
            // Ensure hitboxes are disabled initially
            DeactivateHitbox();
        }
        
        protected virtual void Start()
        {
            combatService = ServiceLocator.GetService<CombatService>();
            lastPosition = transform.position;
        }
        
        protected virtual void FixedUpdate()
        {
            // Calculate swing velocity for physics-based damage
            if (isAttacking && hitboxes != null && hitboxes.Length > 0)
            {
                Vector3 currentPosition = hitboxes[0].transform.position;
                swingVelocity = (currentPosition - lastPosition).magnitude / Time.fixedDeltaTime;
                lastPosition = currentPosition;
            }
        }
        
        public virtual void StartAttack(int comboIndex)
        {
            if (isAttacking)
                return;
                
            isAttacking = true;
            currentComboIndex = comboIndex;
            hitThisSwing.Clear();
            
            // Play swing sound
            if (audioSource != null && weaponDef != null && weaponDef.swingSounds.Length > 0)
            {
                var sound = weaponDef.swingSounds[UnityEngine.Random.Range(0, weaponDef.swingSounds.Length)];
                audioSource.PlayOneShot(sound);
            }
            
            // Trigger weapon animation if available
            if (weaponAnimator != null)
            {
                Debug.Log($"[WeaponController] Setting Attack trigger on {weaponAnimator.name} (GameObject: {weaponAnimator.gameObject.name})");
                weaponAnimator.SetTrigger("Attack");
                weaponAnimator.SetInteger("ComboIndex", comboIndex);
                Debug.Log($"[WeaponController] Attack trigger set. Is in Attack state: {weaponAnimator.GetCurrentAnimatorStateInfo(0).IsName("Attack")}");
            }
            else
            {
                Debug.Log("[WeaponController] No weaponAnimator found on weapon");
            }
            
            // Get attack pattern
            AttackPattern attackPattern = GetAttackPattern(comboIndex);
            
            // Raise attack event
            OnAttackExecuted?.Invoke(new AttackInfo(this, transform, hitLayers)
            {
                comboIndex = comboIndex,
                chargeAmount = 0f
            });
        }
        
        public virtual void CancelAttack()
        {
            isAttacking = false;
            DeactivateHitbox();
            hitThisSwing.Clear();
            
            if (weaponAnimator != null)
            {
                weaponAnimator.ResetTrigger("Attack");
            }
        }
        
        public virtual void ActivateHitbox()
        {
            if (hitboxes == null)
                return;
                
            foreach (var hitbox in hitboxes)
            {
                if (hitbox != null)
                {
                    hitbox.enabled = true;
                }
            }
        }
        
        public virtual void DeactivateHitbox()
        {
            if (hitboxes == null)
                return;
                
            foreach (var hitbox in hitboxes)
            {
                if (hitbox != null)
                {
                    hitbox.enabled = false;
                }
            }
            
            hitThisSwing.Clear();
        }
        
        protected virtual void OnTriggerEnter(Collider other)
        {
            if (!isAttacking)
                return;
                
            ProcessHit(other);
        }
        
        protected virtual void OnCollisionEnter(Collision collision)
        {
            if (!isAttacking)
                return;
                
            ProcessHit(collision.collider, collision);
        }
        
        protected virtual void ProcessHit(Collider other, Collision collision = null)
        {
            // Check if we've already hit this collider in this swing
            if (hitThisSwing.Contains(other))
                return;
                
            // Check if the collider is on a valid hit layer
            if ((hitLayers.value & (1 << other.gameObject.layer)) == 0)
                return;
                
            // Check minimum velocity threshold for physics-based hits
            if (swingVelocity < minHitVelocity)
                return;
                
            // Don't hit ourselves or our owner
            if (other.transform.IsChildOf(transform) || 
                (ownerCombatController != null && other.transform.IsChildOf(ownerCombatController.transform)))
                return;
                
            // Mark this collider as hit
            hitThisSwing.Add(other);
            
            // Get damageable component
            var damageable = other.GetComponent<IDamageable>();
            if (damageable == null)
            {
                // Check parent objects
                damageable = other.GetComponentInParent<IDamageable>();
            }
            
            if (damageable != null)
            {
                // Calculate hit info
                Vector3 hitPoint = collision != null ? 
                    collision.GetContact(0).point : 
                    other.ClosestPoint(transform.position);
                    
                Vector3 hitNormal = collision != null ? 
                    collision.GetContact(0).normal : 
                    (other.transform.position - transform.position).normalized;
                    
                Vector3 hitDirection = transform.forward;
                
                // Check for blocking
                bool wasBlocked = false;
                bool wasDeflected = false;
                
                if (damageable != null)
                {
                    // Check if target is blocking
                    if (damageable.IsBlocking)
                    {
                        Vector3 blockDirection = damageable.GetBlockDirection();
                        float blockAngle = Vector3.Angle(-hitDirection, blockDirection);
                        
                        // Block is successful if within 90 degree cone
                        if (blockAngle < 90f)
                        {
                            wasBlocked = true;
                            PlayBlockEffect(hitPoint, hitNormal);
                            
                            // Trigger attacker's weapon deflection
                            TriggerWeaponDeflection(other);
                        }
                    }
                    
                    // Check for deflection (hitting armor at wrong angle)
                    if (!wasBlocked && IsDeflected(hitNormal, hitDirection))
                    {
                        wasDeflected = true;
                        PlayDeflectEffect(hitPoint, hitNormal);
                    }
                }
                
                // Calculate damage
                float damage = CalculateDamage();
                
                // Apply physics-based damage multiplier
                if (weaponDef != null)
                {
                    float velocityMultiplier = CalculateVelocityDamageMultiplier(swingVelocity, weaponDef.weight);
                    damage *= velocityMultiplier;
                }
                
                // Create damage info
                DamageInfo damageInfo = new DamageInfo(
                    ownerCombatController != null ? ownerCombatController.gameObject : gameObject,
                    other.gameObject,
                    damage,
                    GetDamageType(),
                    hitPoint
                )
                {
                    hitNormal = hitNormal,
                    hitDirection = hitDirection,
                    impactForce = swingVelocity * (weaponDef != null ? weaponDef.weight : 1f),
                    knockbackForce = CalculateKnockback(),
                    statusEffectIds = GetStatusEffects(),
                    wasBlocked = wasBlocked,
                    wasDeflected = wasDeflected
                };
                
                // Deal damage through owner if available
                if (ownerCombatController != null)
                {
                    ownerCombatController.DealDamage(damageable, damageInfo);
                }
                else
                {
                    damageable.TakeDamage(damageInfo);
                }
                
                // Play hit effects
                if (!wasBlocked && !wasDeflected)
                {
                    PlayHitEffect(hitPoint, hitNormal);
                    PlayHitSound();
                }
                
                // Notify combat service
                if (combatService != null && ownerCombatController != null)
                {
                    combatService.ProcessWeaponHit(collision, this, ownerCombatController);
                }
            }
            else
            {
                // Hit something non-damageable (wall, ground, etc.)
                PlayEnvironmentHitEffect(other);
                
                // Trigger weapon deflection
                TriggerWeaponDeflection(other);
            }
        }
        
        protected virtual float CalculateDamage()
        {
            float damage = BaseDamage;
            
            if (ownerCombatController != null)
            {
                damage += ownerCombatController.GetAttackPower();
            }
            
            // Apply combo multiplier
            AttackPattern pattern = GetAttackPattern(currentComboIndex);
            if (pattern != null)
            {
                damage *= pattern.damageMultiplier;
            }
            
            return damage;
        }
        
        protected virtual float CalculateKnockback()
        {
            float knockback = 5f; // Base knockback
            
            if (weaponDef != null)
            {
                knockback *= weaponDef.weight;
            }
            
            AttackPattern pattern = GetAttackPattern(currentComboIndex);
            if (pattern != null)
            {
                knockback = pattern.knockbackForce;
            }
            
            return knockback;
        }
        
        protected virtual DamageType GetDamageType()
        {
            if (weaponDef != null)
            {
                switch (weaponDef.weaponType)
                {
                    case WeaponType.Sword:
                    case WeaponType.Axe:
                        return DamageType.Slash;
                    case WeaponType.Mace:
                    case WeaponType.Hammer:
                        return DamageType.Blunt;
                    case WeaponType.Spear:
                    case WeaponType.Dagger:
                        return DamageType.Pierce;
                    default:
                        return DamageType.Physical;
                }
            }
            
            return DamageType.Physical;
        }
        
        protected virtual string[] GetStatusEffects()
        {
            AttackPattern pattern = GetAttackPattern(currentComboIndex);
            if (pattern != null && pattern.statusEffectIds != null)
            {
                return pattern.statusEffectIds;
            }
            
            return null;
        }
        
        protected virtual AttackPattern GetAttackPattern(int comboIndex)
        {
            if (weaponDef == null)
                return null;
                
            if (weaponDef.comboAttacks != null && comboIndex < weaponDef.comboAttacks.Length)
            {
                return weaponDef.comboAttacks[comboIndex];
            }
            
            return null;
        }
        
        protected virtual float CalculateVelocityDamageMultiplier(float velocity, float weaponWeight)
        {
            // Higher velocity and weight = more damage
            float normalizedVelocity = Mathf.Clamp01(velocity / 10f);
            float weightFactor = Mathf.Lerp(0.8f, 1.5f, weaponWeight / 5f);
            return Mathf.Lerp(0.5f, 1.5f, normalizedVelocity) * weightFactor;
        }
        
        protected virtual bool IsDeflected(Vector3 hitNormal, Vector3 hitDirection)
        {
            // Check if hit angle would cause deflection (glancing blow)
            float hitAngle = Vector3.Angle(hitDirection, -hitNormal);
            return hitAngle > 60f; // Deflect if hit at more than 60 degree angle
        }
        
        protected virtual void PlayHitEffect(Vector3 position, Vector3 normal)
        {
            if (weaponDef != null && weaponDef.hitEffectPrefab != null)
            {
                var effect = Instantiate(weaponDef.hitEffectPrefab, position, Quaternion.LookRotation(normal));
                Destroy(effect, 2f);
            }
        }
        
        protected virtual void PlayHitSound()
        {
            if (audioSource != null && weaponDef != null && weaponDef.hitSounds.Length > 0)
            {
                var sound = weaponDef.hitSounds[UnityEngine.Random.Range(0, weaponDef.hitSounds.Length)];
                audioSource.PlayOneShot(sound);
            }
        }
        
        protected virtual void PlayBlockEffect(Vector3 position, Vector3 normal)
        {
            // Override in derived classes for specific block effects
        }
        
        protected virtual void PlayDeflectEffect(Vector3 position, Vector3 normal)
        {
            // Override in derived classes for specific deflect effects
        }
        
        protected virtual void PlayEnvironmentHitEffect(Collider surface)
        {
            // Override in derived classes for hitting walls, ground, etc.
        }
        
        protected virtual void TriggerWeaponDeflection(Collider surface)
        {
            // Get animator from parent (should be on RightHand)
            if (ownerCombatController != null)
            {
                var animator = ownerCombatController.GetComponentInChildren<Animator>();
                if (animator != null)
                {
                    // Don't cancel attack - we want to reverse it instead
                    isAttacking = false; // Stop damage dealing
                    DeactivateHitbox(); // Stop hitting things
                    
                    // Get current animation state
                    var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                    float currentProgress = stateInfo.normalizedTime;
                    
                    Debug.Log($"[WeaponController] Deflection at {currentProgress * 100:F0}% through attack");
                    
                    // Play animation in reverse
                    animator.SetFloat("AttackSpeed", -1.5f); // Negative speed = reverse, 1.5x for snappy feel
                    
                    // Start recovery coroutine
                    StartCoroutine(RecoverFromReverseDeflection(animator, currentProgress));
                }
            }
        }
        
        private IEnumerator RecoverFromReverseDeflection(Animator animator, float startProgress)
        {
            // Wait for reverse animation to complete
            // Calculate how long the reverse will take
            float reverseTime = startProgress / 1.5f; // Divided by our reverse speed multiplier
            
            Debug.Log($"[WeaponController] Reversing for {reverseTime:F2} seconds");
            
            yield return new WaitForSeconds(reverseTime);
            
            // Reset animation speed
            animator.SetFloat("AttackSpeed", 1f);
            
            // Let animation naturally return to idle state
            // The attack animation will complete its reverse and transition normally
            
            // Notify combat controller that deflection is complete
            if (ownerCombatController != null)
            {
                ownerCombatController.OnAttackComplete();
            }
        }
        
        
        public WeaponDefinition GetWeaponDefinition()
        {
            return weaponDef;
        }
        
        public void SetWeaponDefinition(WeaponDefinition definition)
        {
            weaponDef = definition;
            
            // Spawn weapon model if provided
            if (definition != null && definition.weaponPrefab != null && weaponModel == null)
            {
                var model = Instantiate(definition.weaponPrefab, transform);
                model.transform.localPosition = definition.gripOffset;
                model.transform.localEulerAngles = definition.gripRotation;
                weaponModel = model.transform;
                
                // Get hitboxes from the model if not already set
                if (hitboxes == null || hitboxes.Length == 0)
                {
                    hitboxes = model.GetComponentsInChildren<Collider>();
                }
            }
        }
    }
}
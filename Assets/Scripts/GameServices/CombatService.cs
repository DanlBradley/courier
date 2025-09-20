using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Interfaces;
using Combat;
using Utils;

namespace GameServices
{
    public class CombatService : Service
    {
        [Header("Damage Calculation")]
        [SerializeField] private AnimationCurve damageByVelocity;
        [SerializeField] private float baseDamageMultiplier = 1f;
        
        public event Action<CombatEvent> OnCombatEvent;
        
        public override void Initialize()
        {
            Logs.Log("Combat service initialized.", "GameServices");
        }
        
        #region Combat Processing
        
        public void ProcessWeaponHit(Collision collision, IWeapon weapon, IDamageDealer attacker)
        {
            if (collision == null || weapon == null || attacker == null) return;
            
            var targetGO = collision.gameObject;
            var damageable = targetGO.GetComponent<IDamageable>();
            
            if (damageable == null) return;
            
            if (!ValidateHit(collision, attacker as IDamageable, damageable))
            {
                var attackerGO = (attacker as Component)?.gameObject;
                BroadcastEvent(new CombatEvent(CombatEventType.AttackBlocked, attackerGO, targetGO, 0f, collision.contacts[0].point));
                return;
            }
            
            var damageInfo = CalculateDamageFromCollision(collision, weapon, attacker, damageable);
            damageable.TakeDamage(damageInfo);
            
            var attackerGameObject = (attacker as Component)?.gameObject;
            BroadcastEvent(new CombatEvent(CombatEventType.AttackConnected, attackerGameObject, targetGO, damageInfo.damage, damageInfo.hitPoint));
        }
        
        public DamageInfo CalculateDamageFromCollision(Collision collision, IWeapon weapon, IDamageDealer attacker, IDamageable target)
        {
            var contact = collision.contacts[0];
            var relativeVelocity = collision.relativeVelocity.magnitude;
            
            float baseDamage = weapon.BaseDamage * attacker.GetAttackPower();
            float velocityMultiplier = CalculateDamageMultiplier(relativeVelocity, 1f); // TODO: Get weapon weight
            float finalDamage = baseDamage * velocityMultiplier * baseDamageMultiplier;
            
            // Minimum damage threshold for testing
            if (finalDamage < 1f && finalDamage > 0f)
            {
                Debug.LogWarning($"[CombatService] Low damage detected: {finalDamage:F4} (velocity: {relativeVelocity:F2}, baseDamage: {baseDamage}, velocityMult: {velocityMultiplier:F4})");
                finalDamage = Mathf.Max(finalDamage, 5f); // Minimum 5 damage for testing
            }
            
            var attackerComponent = attacker as Component;
            var attackerGO = attackerComponent != null ? attackerComponent.gameObject : null;
            
            var damageInfo = new DamageInfo(
                attackerGO,
                collision.gameObject,
                finalDamage,
                DamageType.Physical,
                contact.point
            )
            {
                hitNormal = contact.normal,
                hitDirection = collision.relativeVelocity.normalized,
                impactForce = collision.impulse.magnitude,
                knockbackForce = collision.impulse.magnitude * 0.1f
            };
            
            return damageInfo;
        }
        
        public bool ValidateHit(Collision collision, IDamageable attacker, IDamageable target)
        {
            if (attacker == null || target == null) return true;
            
            // Can't hit yourself
            if (attacker == target) return false;
            
            // Can't hit teammates (unless friendly fire is on)
            if (attacker.Team == target.Team && attacker.Team != Team.Neutral) return false;
            
            // Check if target is blocking
            if (target.IsBlocking)
            {
                var targetTransform = (target as Component)?.transform;
                if (targetTransform != null)
                {
                    Vector3 attackDirection = (collision.contacts[0].point - targetTransform.position).normalized;
                    Vector3 blockDirection = target.GetBlockDirection();
                    float angle = Vector3.Angle(attackDirection, blockDirection);
                    
                    // Block successful if within 90 degree cone
                    if (angle < 90f) return false;
                }
            }
            
            return true;
        }
        
        public float CalculateDamageMultiplier(float relativeVelocity, float weaponWeight)
        {
            // If no curve is set, use a simple linear calculation
            float velocityFactor = 1f;
            if (damageByVelocity != null && damageByVelocity.keys.Length > 0)
            {
                velocityFactor = damageByVelocity.Evaluate(relativeVelocity);
            }
            else
            {
                // Default: linear scaling from 0 to 2x damage based on velocity (0-10 m/s)
                velocityFactor = Mathf.Clamp(relativeVelocity / 10f, 0.1f, 2f);
            }
            
            float weightFactor = Mathf.Sqrt(weaponWeight); // Heavier weapons do more damage
            return velocityFactor * weightFactor;
        }
        
        #endregion
        
        #region Event Broadcasting
        
        public void RaiseCombatEvent(CombatEvent combatEvent)
        {
            BroadcastEvent(combatEvent);
        }
        
        private void BroadcastEvent(CombatEvent combatEvent)
        {
            OnCombatEvent?.Invoke(combatEvent);
            
            // Events on interfaces can't be invoked directly, only subscribed to
            // Individual combatants should subscribe to the main CombatService event if interested
        }
        
        #endregion
    }
}
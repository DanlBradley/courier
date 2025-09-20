using System;
using Combat;
using UnityEngine;

namespace Interfaces
{
    public interface IDamageable
    {
        // Properties needed for combat validation
        Team Team { get; }
        bool IsAlive { get; }
        bool IsBlocking { get; }
        Vector3 GetBlockDirection();
        float GetDefense();
        
        // Core damage functionality
        void TakeDamage(DamageInfo damageInfo);
        void Heal(float amount);
        
        // Events
        event Action<DamageInfo> OnDamageTaken;
        event Action<float> OnHealed;
        event Action OnDeath;
    }
}
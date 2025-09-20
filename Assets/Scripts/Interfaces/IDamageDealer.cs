using System;
using Combat;

namespace Interfaces
{
    public interface IDamageDealer
    {
        // Properties needed for damage calculation
        Team Team { get; }
        float GetAttackPower();
        IWeapon GetCurrentWeapon();
        
        // Core damage functionality
        void DealDamage(IDamageable target, DamageInfo damageInfo);
        float CalculateDamage(AttackInfo attackInfo);
        
        // Events
        event Action<DamageInfo> OnDamageDealt;
    }
}
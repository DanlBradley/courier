using System;
using Combat;

namespace Interfaces
{
    public enum WeaponType
    {
        None,
        Sword,
        Axe,
        Mace,
        Dagger,
        Spear,
        Hammer,
        Staff,
        Bow,
        Crossbow,
        Shield,
        Fist
    }

    public interface IWeapon
    {
        WeaponType WeaponType { get; }
        float BaseDamage { get; }
        float AttackSpeed { get; }
        float Range { get; }
        float EnergyCost { get; }
        
        void StartAttack(int comboIndex);
        void CancelAttack();
        bool IsAttacking { get; }
        
        event Action<AttackInfo> OnAttackExecuted;
    }
}
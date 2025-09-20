using System;
using UnityEngine;
using Combat;
using Interfaces;

namespace Items
{
    [CreateAssetMenu(fileName = "WeaponDefinition", menuName = "Courier/Items/Weapon Definition")]
    public class WeaponDefinition : EquipmentDefinition
    {
        [Header("Weapon Type")]
        public WeaponType weaponType;
        
        [Header("Combat Stats")]
        public float baseDamage = 10f;
        public float attackSpeed = 1f;
        public float range = 2f;
        public float weight = 1f;
        
        [Header("Energy")]
        public float energyCostMultiplier = 1f;
        
        [Header("Attack Patterns")]
        public AttackPattern[] comboAttacks;
        public AttackPattern heavyAttack;
        
        [Header("Audio/Visual")]
        public AudioClip[] swingSounds;
        public AudioClip[] hitSounds;
        public GameObject hitEffectPrefab;
        
        [Header("Weapon Model")]
        public GameObject weaponPrefab;
        public Vector3 gripOffset;
        public Vector3 gripRotation;
    }
    
    public class WeaponItem : EquipmentItem
    {
        public WeaponItem(WeaponDefinition weaponDefinition) : base(weaponDefinition)
        {
            if (weaponDefinition == null) 
                throw new ArgumentNullException(nameof(weaponDefinition));
        }
        
        public WeaponDefinition GetWeaponDefinition() 
        { 
            return (WeaponDefinition)itemDef; 
        }
    }
}
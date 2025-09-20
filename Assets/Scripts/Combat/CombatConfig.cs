using UnityEngine;

namespace Combat
{
    [CreateAssetMenu(fileName = "CombatConfig", menuName = "Courier/Combat/Combat Config")]
    public class CombatConfig : ScriptableObject
    {
        [Header("Base Stats")]
        public float baseAttackPower = 10f;
        public float baseDefense = 5f;
        public float baseAttackSpeed = 1f;
        
        [Header("Energy Costs")]
        public float lightAttackEnergy = 10f;
        public float heavyAttackEnergy = 25f;
        public float blockEnergy = 5f;
        
        [Header("Combat Behavior")]
        public float combatEngageDistance = 15f;
        public float combatDisengageTime = 5f;
        public AnimationCurve damageMultiplierByCharge;
        
        [Header("Hit Settings")]
        public LayerMask targetLayers;
        public float hitStopDuration = 0.1f;
        
        [Header("Physics")]
        public AnimationCurve damageByVelocity;
        public float minVelocityForDamage = 2f;
        public float maxVelocityForDamage = 20f;
    }
}
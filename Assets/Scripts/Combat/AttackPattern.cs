using UnityEngine;

namespace Combat
{
    [System.Serializable]
    public class AttackPattern
    {
        public string attackName = "Attack";
        public AnimationClip animation;
        public float damageMultiplier = 1f;
        public float energyCost = 10f;
        public float range = 2f;
        
        [Header("Timing")]
        public float windupTime = 0.2f;
        public float activeTime = 0.3f;
        public float recoveryTime = 0.5f;
        
        [Header("Combo")]
        public bool canCombo = true;
        public float comboWindowStart = 0.5f;
        public float comboWindowDuration = 0.3f;
        
        [Header("Hit Properties")]
        public float knockbackForce = 5f;
        public string[] statusEffectIds;
        
        public float TotalTime => windupTime + activeTime + recoveryTime;
    }
}
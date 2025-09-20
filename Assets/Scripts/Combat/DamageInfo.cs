using UnityEngine;

namespace Combat
{
    public enum DamageType
    {
        Physical,
        Blunt,
        Slash,
        Pierce,
        Fire,
        Ice,
        Poison,
        Bleed,
        Magic
    }

    [System.Serializable]
    public struct DamageInfo
    {
        public GameObject source;
        public GameObject target;
        public float damage;
        public DamageType damageType;
        public Vector3 hitPoint;
        public Vector3 hitNormal;
        public Vector3 hitDirection;
        public float impactForce;
        public float knockbackForce;
        public string[] statusEffectIds;
        public bool wasBlocked;
        public bool wasDeflected;

        public DamageInfo(GameObject source, GameObject target, float damage, DamageType damageType, Vector3 hitPoint)
        {
            this.source = source;
            this.target = target;
            this.damage = damage;
            this.damageType = damageType;
            this.hitPoint = hitPoint;
            this.hitNormal = Vector3.zero;
            this.hitDirection = Vector3.zero;
            this.impactForce = 0f;
            this.knockbackForce = 0f;
            this.statusEffectIds = null;
            this.wasBlocked = false;
            this.wasDeflected = false;
        }
    }
}
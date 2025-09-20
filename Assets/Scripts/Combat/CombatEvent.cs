using UnityEngine;

namespace Combat
{
    public enum CombatEventType
    {
        AttackStarted,
        AttackConnected,
        AttackBlocked,
        AttackDeflected,
        DamageTaken,
        DamageDealt,
        Death,
        Stagger,
        WeaponClash
    }

    [System.Serializable]
    public struct CombatEvent
    {
        public CombatEventType eventType;
        public GameObject source;
        public GameObject target;
        public float value;
        public Vector3 position;
        public float timestamp;

        public CombatEvent(CombatEventType eventType, GameObject source, GameObject target = null, float value = 0f, Vector3 position = default)
        {
            this.eventType = eventType;
            this.source = source;
            this.target = target;
            this.value = value;
            this.position = position;
            this.timestamp = Time.time;
        }
    }
}
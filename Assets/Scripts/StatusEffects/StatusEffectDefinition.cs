using UnityEngine;
using System.Collections.Generic;

namespace StatusEffects
{
    [CreateAssetMenu(fileName = "StatusEffect", menuName = "Courier/Status Effects/Status Effect Definition")]
    public class StatusEffectDefinition : ScriptableObject
    {
        [Header("Basic Info")]
        public string id;
        public new string name;
        public string description;
        public Sprite icon;
        public bool isDebuff = true;

        [Header("Duration")]
        public EffectDurationType durationType = EffectDurationType.Timed;
        public float duration = 10f; // Only used if durationType is Timed
        public StackingRule stackingRule = StackingRule.Replace;

        [Header("Effects")]
        public List<StatusEffectModifier> modifiers = new();
        public List<StatusEffectAction> actions = new();
    }

    [System.Serializable]
    public class StatusEffectModifier
    {
        public string targetStat; // e.g., "StaminaRegenRate", "HealthDecayRate"
        public ModifierType modifierType;
        public float value;
    }

    [System.Serializable]
    public class StatusEffectAction
    {
        public ActionTrigger trigger;
        public string actionType; // e.g., "DamageHealth", "RestoreStamina"
        public float value;
        public float interval = 1f; // For periodic actions
    }
    
    public enum EffectDurationType
    {
        Permanent,      // Until explicitly removed
        Timed,         // Expires after X seconds
        Conditional    // Expires when condition is met (e.g., sleep, full health)
    }

    public enum StackingRule
    {
        Replace,       // New effect replaces old
        Stack,         // Effects accumulate
        ExtendDuration, // Refresh duration of existing
        Ignore         // Can't apply if already present
    }

    public enum ModifierType
    {
        Additive,      // +5 to stat
        Multiplicative, // *1.5 to stat
        Override       // Set stat to specific value
    }

    public enum ActionTrigger
    {
        OnApply,       // When effect is first applied
        OnRemove,      // When effect is removed
        Periodic,      // Every X seconds
        OnTick         // Every game tick
    }
}
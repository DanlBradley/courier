using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using GameServices;
using Interfaces;

namespace StatusEffects
{
    public class StatusEffectManager : MonoBehaviour
    {
        private List<StatusEffect> activeEffects = new();
        public event Action<EffectChangeEvent> OnEffectChanged;

        private void OnEnable() { if (TickService.Instance != null) { TickService.Instance.OnTick += OnTick; } }
        private void OnDisable() { if (TickService.Instance != null) { TickService.Instance.OnTick -= OnTick; } }
        private void Update() { UpdateEffects(Time.deltaTime); }

        private void Start()
        {
            // ServiceLocator.GetService<IStatusEffectService>().ApplyEffectToTarget(gameObject, "bleeding");
            // ServiceLocator.GetService<IStatusEffectService>().ApplyEffectToTarget(gameObject, "sprained_leg");
        }

        private void OnTick() { foreach (var effect in activeEffects) { effect.OnTick(); } }

        public bool ApplyStatusEffect(StatusEffectDefinition effectDef, float intensity = 1f)
        {
            var existingEffect = GetActiveEffect(effectDef.id);
            
            if (existingEffect != null)
            {
                switch (effectDef.stackingRule)
                {
                    case StackingRule.Replace:
                        RemoveStatusEffect(effectDef.id);
                        break;
                    case StackingRule.ExtendDuration:
                        existingEffect.ExtendDuration(effectDef.duration);
                        return true;
                    case StackingRule.Ignore:
                        return false;
                    case StackingRule.Stack:
                        // Allow multiple instances
                        break;
                }
            }

            var newEffect = new StatusEffect(effectDef, gameObject, intensity);
            activeEffects.Add(newEffect);
            
            NotifyEffectChange(EffectChangeType.Applied, newEffect);
            
            return true;
        }

        public bool RemoveStatusEffect(string effectId)
        {
            var effect = GetActiveEffect(effectId);
            if (effect == null) return false;

            effect.RemoveEffect();
            activeEffects.Remove(effect);
            
            NotifyEffectChange(EffectChangeType.Removed, effect);
            
            return true;
        }

        public StatusEffect GetActiveEffect(string effectId)
        {
            return activeEffects.FirstOrDefault(e => e.definition.id == effectId && e.isActive);
        }

        public List<StatusEffect> GetActiveEffects() { return activeEffects.Where(e => e.isActive).ToList(); }

        public (float additive, float multiplicative, float? overrideValue) GetModifierValues(string statName)
        {
            float additiveTotal = 0f;
            float multiplicativeTotal = 1f;
            float? overrideValue = null;

            foreach (var effect in activeEffects.Where(e => e.isActive))
            {
                foreach (var modifier in effect.definition.modifiers)
                {
                    if (modifier.targetStat != statName) continue;

                    switch (modifier.modifierType)
                    {
                        case ModifierType.Additive:
                            additiveTotal += modifier.value * effect.intensity;
                            break;
                        case ModifierType.Multiplicative:
                            multiplicativeTotal *= modifier.value;
                            break;
                        case ModifierType.Override:
                            overrideValue = modifier.value;
                            break;
                    }
                }
            }

            return (additiveTotal, multiplicativeTotal, overrideValue);
        }

        public bool HasEffect(string effectId) { return GetActiveEffect(effectId) != null; }

        private void UpdateEffects(float deltaTime)
        {
            var effectsToRemove = new List<StatusEffect>();

            foreach (var effect in activeEffects)
            {
                effect.UpdateEffect(deltaTime);
                if (!effect.isActive)
                {
                    effectsToRemove.Add(effect);
                }
            }

            foreach (var effect in effectsToRemove)
            {
                activeEffects.Remove(effect);
                NotifyEffectChange(EffectChangeType.Removed, effect);
            }
        }
        
        private void NotifyEffectChange(EffectChangeType changeType, StatusEffect effect)
        {
            var evt = new EffectChangeEvent(changeType, effect, GetActiveEffects());
            OnEffectChanged?.Invoke(evt);
        }
    }
    
    public enum EffectChangeType
    {
        Applied,
        Removed,
        Updated  // For duration changes, intensity changes, etc.
    }

    public struct EffectChangeEvent
    {
        public EffectChangeType changeType;
        public StatusEffect effect;
        public List<StatusEffect> allActiveEffects; // For UI that needs the full list
        
        public EffectChangeEvent(EffectChangeType changeType, StatusEffect effect, List<StatusEffect> allActiveEffects)
        {
            this.changeType = changeType;
            this.effect = effect;
            this.allActiveEffects = allActiveEffects;
        }
    }
}
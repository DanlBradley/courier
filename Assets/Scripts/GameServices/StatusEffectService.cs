using System;
using System.Collections.Generic;
using Character;
using StatusEffects;
using UnityEngine;
using Utils;

namespace GameServices
{
    public class StatusEffectService : Service
    {
        
        [SerializeField] private List<StatusEffectDefinition> availableEffects = new();
        private Dictionary<string, StatusEffectDefinition> effectLookup = new();
        
        public override void Initialize()
        {
            InitializeEffectLookup();
            Logs.Log("Status effect Service initialized.", "GameServices");
        }

        private void InitializeEffectLookup()
        {
            effectLookup.Clear();
            foreach (var effect in availableEffects)
            {
                if (!string.IsNullOrEmpty(effect.id))
                {
                    effectLookup[effect.id] = effect;
                }
            }
        }

        public StatusEffectDefinition GetEffectDefinition(string effectId)
        {
            effectLookup.TryGetValue(effectId, out var effect);
            return effect;
        }

        public void ExecuteAction(StatusEffectAction action, GameObject target, float intensity)
        {
            switch (action.actionType)
            {
                case "DamageHealth":
                    var characterStatus = target.GetComponent<CharacterStatus>();
                    if (characterStatus != null)
                    {
                        characterStatus.UpdateVital(-action.value * intensity, Vitals.Health);
                    }
                    break;
                case "RestoreSatiety":
                    var satietyStatus = target.GetComponent<CharacterStatus>();
                    if (satietyStatus != null)
                    {
                        satietyStatus.UpdateVital(action.value * intensity, Vitals.Satiety);
                    }
                    break;
                // Add more action types as needed
                default:
                    Debug.LogWarning($"Unknown status effect action: {action.actionType}");
                    break;
            }
        }

        public bool ApplyEffectToTarget(GameObject target, string effectId, float intensity = 1f)
        {
            var effectDef = GetEffectDefinition(effectId);
            if (effectDef == null)
            {
                Debug.LogError($"Status effect not found: {effectId}");
                return false;
            }

            var statusManager = target.GetComponent<StatusEffectManager>();
            if (statusManager == null)
            {
                Debug.LogError($"Target {target.name} doesn't have a StatusEffectManager component");
                return false;
            }

            return statusManager.ApplyStatusEffect(effectDef, intensity);
        }
        
        public event Action<EffectChangeEvent, GameObject> OnGlobalEffectChanged;
        public void BroadcastEffectChange(EffectChangeEvent evt, GameObject entity)
        {
            //TODO: Not implemented
            //something like this to notify audio system etc. etc.
            OnGlobalEffectChanged?.Invoke(evt, entity);
            throw new NotImplementedException();
        }
    }
}
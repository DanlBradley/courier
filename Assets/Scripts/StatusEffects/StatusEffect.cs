using UnityEngine;
using GameServices;

namespace StatusEffects
{
    public class StatusEffect
    {
        public StatusEffectDefinition definition { get; private set; }
        public float remainingDuration { get; private set; }
        public bool isActive { get; private set; }
        public GameObject target { get; private set; }
        public float intensity { get; private set; }
        
        private float lastPeriodicActionTime;

        public StatusEffect(StatusEffectDefinition def, GameObject targetEntity, float effectIntensity = 1f)
        {
            definition = def;
            target = targetEntity;
            intensity = effectIntensity;
            remainingDuration = def.duration;
            isActive = true;
            lastPeriodicActionTime = 0f;

            ExecuteActions(ActionTrigger.OnApply);
        }

        public void UpdateEffect(float deltaTime)
        {
            if (!isActive) return;

            if (definition.durationType == EffectDurationType.Timed)
            {
                remainingDuration -= deltaTime;
                if (remainingDuration <= 0)
                {
                    RemoveEffect();
                    return;
                }
            }

            // Handle periodic actions
            ExecutePeriodicActions(deltaTime);
        }

        public void OnTick()
        {
            if (!isActive) return;
            ExecuteActions(ActionTrigger.OnTick);
        }

        public void ExtendDuration(float additionalTime)
        {
            if (definition.durationType == EffectDurationType.Timed)
            {
                remainingDuration += additionalTime;
            }
        }

        public void RemoveEffect()
        {
            if (!isActive) return;
            
            ExecuteActions(ActionTrigger.OnRemove);
            isActive = false;
        }

        private void ExecuteActions(ActionTrigger trigger)
        {
            foreach (var action in definition.actions)
            {
                if (action.trigger != trigger) continue;
                Debug.Log("Activating effect " + definition.name + " with trigger " + trigger);
                ServiceLocator.GetService<StatusEffectService>().ExecuteAction(action, target, intensity);
            }
        }

        private void ExecutePeriodicActions(float deltaTime)
        {
            foreach (var action in definition.actions)
            {
                if (action.trigger != ActionTrigger.Periodic) continue;
                lastPeriodicActionTime += deltaTime;
                if (!(lastPeriodicActionTime >= action.interval)) continue;
                ServiceLocator.GetService<StatusEffectService>().ExecuteAction(action, target, intensity);
                lastPeriodicActionTime = 0f;
            }
        }
    }
}
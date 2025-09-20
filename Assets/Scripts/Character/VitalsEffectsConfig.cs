using System;
using System.Collections.Generic;
using UnityEngine;

namespace Character
{
    [CreateAssetMenu(fileName = "New VitalsEffectsConfig", menuName = "Courier/Character/Vitals Effects Config")]
    public class VitalsEffectsConfig : ScriptableObject
    {
        [SerializeField] private List<VitalsEffect> vitalsEffects;
        public List<VitalsEffect> VitalsEffects => vitalsEffects;
    }
    
    [Serializable]
    public class VitalsEffect
    {
        public Vitals vital;
        public float thresholdValue;
        public float thresholdValueRange; //only applies to threshold types of "between" to add to reg value
        public ThresholdType type;
        public string effectId;
        public bool isCurrentlyActive;

        public bool CheckThreshold(float value)
        {
            return type switch
            {
                ThresholdType.Below => value < thresholdValue,
                ThresholdType.Above => value > thresholdValue,
                ThresholdType.Between => value > thresholdValue && value < thresholdValue + thresholdValueRange,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        
        public enum ThresholdType{ Below, Above, Between }
    }
}
using Interfaces;
using UnityEngine;

namespace EnvironmentTools
{
    /// <summary>
    /// A water source that requires time to collect water from (filling containers)
    /// </summary>
    [RequireComponent(typeof(TimedInteraction))]
    public class WaterSourceHandler : MonoBehaviour, ITimedInteractionHandler
    {
        [Header("Water Source Properties")] [SerializeField]
        private bool isContaminated;

        [SerializeField] private bool hasUnlimitedWater = true;
        [SerializeField] private int remainingWaterUnits = 100;

        private void Start()
        {
            // Configure the timed interaction
            var timedInteraction = GetComponent<TimedInteraction>();
            if (timedInteraction == null) return;
            timedInteraction.SetInteractionText(GetInteractionText());
            timedInteraction.SetHoldDuration(3f);
        }

        public void OnInteractionComplete(GameObject interactor)
        {
            if (!CanPerformInteraction(interactor)) return;
            if (hasUnlimitedWater) return;
            
            remainingWaterUnits--;
            UpdateInteractionText();
            if (remainingWaterUnits > 0) return;
            
            var timedInteraction = GetComponent<TimedInteraction>();
            if (timedInteraction == null) return;
            
            timedInteraction.SetCanInteract(false);
            timedInteraction.SetInteractionText("Dry");
        }

        public bool CanPerformInteraction(GameObject interactor) 
        { return hasUnlimitedWater || remainingWaterUnits > 0; }

        private string GetInteractionText()
        {
            if (!hasUnlimitedWater && remainingWaterUnits <= 0) return "Dry";
            return "Fill container";
        }

        private void UpdateInteractionText()
        {
            var timedInteraction = GetComponent<TimedInteraction>();
            if (timedInteraction != null) { timedInteraction.SetInteractionText(GetInteractionText()); }
        }

        public bool IsContaminated => isContaminated;
        public bool HasWater => hasUnlimitedWater || remainingWaterUnits > 0;

        public void SetContaminated(bool contaminated)
        {
            isContaminated = contaminated;
            UpdateInteractionText();
        }

        public void AddWater(int units)
        {
            if (hasUnlimitedWater) return;
            remainingWaterUnits += units;
            var timedInteraction = GetComponent<TimedInteraction>();
            if (timedInteraction != null && remainingWaterUnits > 0) { timedInteraction.SetCanInteract(true); }
            UpdateInteractionText();
        }
    }
}
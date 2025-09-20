using UnityEngine;
using GameServices;
using Inputs;
using Interfaces;

namespace Character
{
    public class HeartRateManager : MonoBehaviour
    {
        [Header("Heart Rate Settings")]
        [SerializeField] private float recoveryRate = 1f;
        [SerializeField] private float stressedRecoveryRate = 0.5f;
        [SerializeField] private float hrRecoveryScale = 2.5f;
        
        [Header("Activity Targets")]
        [SerializeField] private float walkingTargetHr = 90f;
        [SerializeField] private float sprintingTargetHr = 170f;
        [SerializeField] private float climbingTargetHr = 150f;
        
        [Header("Activity Rates")]
        [SerializeField] private float walkingHrRate = 0.5f;
        [SerializeField] private float sprintingHrRate = 3f;
        [SerializeField] private float climbingHrRate = 2f;
        
        
        private CharacterStatus characterStatus;
        private IPlayerMovementController movementController;
        private float currentTargetHr;
        private bool isStressed;
        
        public enum HeartRateZone
        {
            Resting,
            Moderate,
            High,
            RedZone
        }
        
        private void Start()
        {
            characterStatus = GetComponent<CharacterStatus>();
            movementController = GetComponent<PlayerMovementController>();
            if (movementController == null)
            {
                Debug.LogError($"No movement controller found on {gameObject.name}");
            }
        }
        
        private void OnEnable() { TickService.Instance.OnTick += UpdateHeartRate; }
        private void OnDisable() { TickService.Instance.OnTick -= UpdateHeartRate; }
        
        private void UpdateHeartRate()
        {
            CalculateTargetHeartRate();
            
            float currentHr = characterStatus.HeartRate;
            float targetHr = currentTargetHr + GetEnvironmentalModifiers();
            
            float hrChange = 0f;
            
            if (currentHr < targetHr)
            {
                float increaseRate = GetActivityIncreaseRate();
                hrChange = increaseRate * hrRecoveryScale;
                hrChange = Mathf.Min(hrChange, targetHr - currentHr);
            }
            else if (currentHr > targetHr)
            {
                float decreaseRate = isStressed ? stressedRecoveryRate : recoveryRate;
                
                hrChange = -decreaseRate * hrRecoveryScale;
                hrChange = Mathf.Max(hrChange, targetHr - currentHr);
            }
            
            if (Mathf.Abs(hrChange) > 0.01f)
            {
                characterStatus.UpdateVital(hrChange, Vitals.HeartRate);
            }
        }
        
        private void CalculateTargetHeartRate()
        {
            currentTargetHr = characterStatus.RestingHeartRate;
            
            if (movementController == null) return;

            if (movementController.IsSprinting) { currentTargetHr = sprintingTargetHr; }
            else if (movementController.IsClimbing()) currentTargetHr = climbingTargetHr;
            else if (movementController.IsWalking()) { currentTargetHr = walkingTargetHr; }
        }
        
        private float GetActivityIncreaseRate()
        {
            if (movementController == null) return 0f;
            
            if (movementController.IsSprinting) { return sprintingHrRate; }
            if (movementController.IsClimbing()) { return climbingHrRate; }
            if (movementController.IsWalking()) { return walkingHrRate; }
            return 0f;
        }
        
        private float GetEnvironmentalModifiers()
        {
            float modifier = 0f;
            
            float bodyTemp = characterStatus.BodyTemp;
            if (bodyTemp > 100f) { modifier += (bodyTemp - 100f) * 0.3f; }
            
            return modifier;
        }
        
        public HeartRateZone GetCurrentZone()
        {
            float currentHr = characterStatus.HeartRate;

            return currentHr switch
            {
                >= 170f => HeartRateZone.RedZone,
                >= 150f => HeartRateZone.High,
                >= 120f => HeartRateZone.Moderate,
                _ => HeartRateZone.Resting
            };
        }
    }
}
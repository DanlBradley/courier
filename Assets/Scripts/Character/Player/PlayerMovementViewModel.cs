using UnityEngine;
using Interfaces;
using GameServices;
using Inputs;

namespace Character.Player
{
    [RequireComponent(typeof(Camera))]
    public class PlayerMovementViewModel : MonoBehaviour
    {
        [Header("Head Bob Settings")]
        [SerializeField] private bool enableHeadBob = true;
        [SerializeField] private float walkBobSpeed = 10f;
        [SerializeField] private float walkBobAmplitude = 0.05f;
        [SerializeField] private float sprintBobSpeed = 15f;
        [SerializeField] private float sprintBobAmplitude = 0.075f;
        [SerializeField] private float crouchBobSpeed = 6f;
        [SerializeField] private float crouchBobAmplitude = 0.025f;
        
        [Header("Bob Curve Settings")]
        [SerializeField] private AnimationCurve horizontalBobCurve = AnimationCurve.Linear(0, 0, 1, 0);
        [SerializeField] private AnimationCurve verticalBobCurve = AnimationCurve.Linear(0, 0, 1, 0);
        [SerializeField] private float horizontalMultiplier = 1f;
        [SerializeField] private float verticalMultiplier = 2f;
        
        [Header("Landing Impact")]
        [SerializeField] private float landingImpactDuration = 0.3f;
        [SerializeField] private float landingImpactIntensity = 0.2f;
        [SerializeField] private AnimationCurve landingCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
        [SerializeField] private float fallThreshold = -5f;
        
        [Header("Movement Inertia")]
        [SerializeField] private float movementLag = 5f;
        [SerializeField] private float rotationLag = 8f;
        [SerializeField] private float maxInertiaOffset = 0.05f;
        [SerializeField] private float maxInertiaRotation = 2f;
        
        [Header("Weapon Sway")]
        [SerializeField] private bool enableWeaponSway = true;
        [SerializeField] private float swayAmount = 0.02f;
        [SerializeField] private float swaySpeed = 5f;
        [SerializeField] private float maxSwayAmount = 0.06f;
        
        [Header("Breathing Effect")]
        [SerializeField] private bool enableBreathing = true;
        [SerializeField] private float breathingSpeed = 0.3f;
        [SerializeField] private float breathingIntensity = 0.002f;
        [SerializeField] private float exhaustedBreathingSpeed = 0.6f;
        [SerializeField] private float exhaustedBreathingIntensity = 0.008f;
        
        [Header("Stance Transitions")]
        [SerializeField] private float stanceTransitionSpeed = 10f;
        [SerializeField] private float crouchOffset = -0.5f;
        
        private IPlayerMovementController movementController;
        private Camera playerCamera;
        private Transform cameraTransform;
        
        private float bobTimer;
        private Vector3 originalCameraPosition;
        private Quaternion originalCameraRotation;
        
        private float landingTimer;
        private float lastFallVelocity;
        private bool wasGrounded = true;
        
        private Vector3 targetInertiaOffset;
        private Vector3 currentInertiaOffset;
        private Vector3 targetInertiaRotation;
        private Vector3 currentInertiaRotation;
        
        private Vector2 lastMouseDelta;
        private float breathingTimer;
        
        private float currentStanceOffset;
        private float targetStanceOffset;
        
        private Vector3 weaponSwayOffset;
        private Vector3 lastCameraForward;

        private void Awake()
        {
            playerCamera = GetComponent<Camera>();
            cameraTransform = transform;
            
            var playerObject = GetComponentInParent<IPlayerMovementController>();
            if (playerObject != null)
            {
                movementController = playerObject;
            }
            
            originalCameraPosition = cameraTransform.localPosition;
            originalCameraRotation = cameraTransform.localRotation;
            lastCameraForward = cameraTransform.forward;
        }

        private void LateUpdate()
        {
            if (movementController == null) return;
            
            HandleGroundImpact();
            HandleStanceTransition();
            
            Vector3 finalPosition = originalCameraPosition;
            Quaternion finalRotation = originalCameraRotation;
            
            if (enableHeadBob && movementController.IsMoving && movementController.IsGrounded)
            {
                ApplyHeadBob(ref finalPosition);
            }
            
            ApplyBreathing(ref finalPosition);
            ApplyMovementInertia(ref finalPosition, ref finalRotation);
            ApplyLandingImpact(ref finalPosition);
            ApplyStanceOffset(ref finalPosition);
            
            if (enableWeaponSway)
            {
                ApplyWeaponSway(ref finalPosition);
            }
            
            cameraTransform.localPosition = finalPosition;
            cameraTransform.localRotation = finalRotation;
            
            lastCameraForward = cameraTransform.forward;
        }

        private void ApplyHeadBob(ref Vector3 position)
        {
            float bobSpeed = walkBobSpeed;
            float bobAmplitude = walkBobAmplitude;
            
            if (movementController.IsSprinting)
            {
                bobSpeed = sprintBobSpeed;
                bobAmplitude = sprintBobAmplitude;
            }
            else if (movementController.IsCrouching)
            {
                bobSpeed = crouchBobSpeed;
                bobAmplitude = crouchBobAmplitude;
            }
            
            float speedMultiplier = Mathf.Clamp01(movementController.CurrentSpeed / 6f);
            bobTimer += Time.deltaTime * bobSpeed * speedMultiplier;
            
            float bobCycle = bobTimer % 1f;
            
            float horizontalBob = horizontalBobCurve.Evaluate(bobCycle) * bobAmplitude * horizontalMultiplier;
            float verticalBob = verticalBobCurve.Evaluate(bobCycle) * bobAmplitude * verticalMultiplier;
            
            position.x += horizontalBob * speedMultiplier;
            position.y += verticalBob * speedMultiplier;
        }

        private void ApplyBreathing(ref Vector3 position)
        {
            if (!enableBreathing) return;
            
            float speed = breathingSpeed;
            float intensity = breathingIntensity;
            
            if (movementController.IsSprinting && movementController.CurrentSpeed > 8f)
            {
                speed = exhaustedBreathingSpeed;
                intensity = exhaustedBreathingIntensity;
            }
            
            breathingTimer += Time.deltaTime * speed;
            
            float breathY = Mathf.Sin(breathingTimer * 2 * Mathf.PI) * intensity;
            float breathX = Mathf.Sin(breathingTimer * 2 * Mathf.PI * 0.5f) * intensity * 0.3f;
            
            position.x += breathX;
            position.y += breathY;
        }

        private void ApplyMovementInertia(ref Vector3 position, ref Quaternion rotation)
        {
            Vector3 velocity = movementController.GetVelocity();
            Vector3 localVelocity = transform.InverseTransformDirection(velocity);
            
            targetInertiaOffset = new Vector3(
                -localVelocity.x * 0.001f,
                -localVelocity.y * 0.0005f,
                -localVelocity.z * 0.001f
            );
            
            targetInertiaOffset = Vector3.ClampMagnitude(targetInertiaOffset, maxInertiaOffset);
            
            currentInertiaOffset = Vector3.Lerp(currentInertiaOffset, targetInertiaOffset, Time.deltaTime * movementLag);
            position += currentInertiaOffset;
            
            Vector3 angularVelocity = Vector3.Cross(lastCameraForward, cameraTransform.forward) / Time.deltaTime;
            targetInertiaRotation = new Vector3(
                angularVelocity.y * 0.5f,
                -angularVelocity.x * 0.5f,
                -angularVelocity.y * 0.2f
            );
            
            targetInertiaRotation = Vector3.ClampMagnitude(targetInertiaRotation, maxInertiaRotation);
            currentInertiaRotation = Vector3.Lerp(currentInertiaRotation, targetInertiaRotation, Time.deltaTime * rotationLag);
            
            rotation *= Quaternion.Euler(currentInertiaRotation);
        }

        private void HandleGroundImpact()
        {
            if (!wasGrounded && movementController.IsGrounded)
            {
                float fallVelocity = movementController.GetVerticalVelocity();
                if (fallVelocity < fallThreshold)
                {
                    landingTimer = landingImpactDuration;
                    lastFallVelocity = fallVelocity;
                }
            }
            
            wasGrounded = movementController.IsGrounded;
        }

        private void ApplyLandingImpact(ref Vector3 position)
        {
            if (landingTimer <= 0) return;
            
            landingTimer -= Time.deltaTime;
            float progress = 1f - (landingTimer / landingImpactDuration);
            float impact = landingCurve.Evaluate(progress);
            
            float impactStrength = Mathf.Clamp01(Mathf.Abs(lastFallVelocity) / 20f) * landingImpactIntensity;
            position.y -= impact * impactStrength;
        }

        private void HandleStanceTransition()
        {
            targetStanceOffset = movementController.IsCrouching ? crouchOffset : 0f;
            currentStanceOffset = Mathf.Lerp(currentStanceOffset, targetStanceOffset, Time.deltaTime * stanceTransitionSpeed);
        }

        private void ApplyStanceOffset(ref Vector3 position)
        {
            position.y += currentStanceOffset;
        }

        private void ApplyWeaponSway(ref Vector3 position)
        {
            Vector2 mouseInput = GetMouseDelta();
            
            float swayX = -mouseInput.x * swayAmount;
            float swayY = -mouseInput.y * swayAmount;
            
            swayX = Mathf.Clamp(swayX, -maxSwayAmount, maxSwayAmount);
            swayY = Mathf.Clamp(swayY, -maxSwayAmount, maxSwayAmount);
            
            Vector3 targetSway = new Vector3(swayX, swayY, 0);
            weaponSwayOffset = Vector3.Lerp(weaponSwayOffset, targetSway, Time.deltaTime * swaySpeed);
            
            position += weaponSwayOffset;
        }

        private Vector2 GetMouseDelta()
        {
            return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        }

        public void ResetViewModel()
        {
            bobTimer = 0f;
            landingTimer = 0f;
            breathingTimer = 0f;
            currentInertiaOffset = Vector3.zero;
            currentInertiaRotation = Vector3.zero;
            weaponSwayOffset = Vector3.zero;
            currentStanceOffset = 0f;
        }

        public void SetHeadBobEnabled(bool enabled)
        {
            enableHeadBob = enabled;
        }

        public void SetWeaponSwayEnabled(bool enabled)
        {
            enableWeaponSway = enabled;
        }

        public void SetBreathingEnabled(bool enabled)
        {
            enableBreathing = enabled;
        }
    }
}
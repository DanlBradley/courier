using UnityEngine;
using GameServices;
using Interfaces;
using StatusEffects;
using Systems;
using Utils;

namespace Inputs
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    [RequireComponent(typeof(StatusEffectManager))]
    public class PlayerMovementController : InputConsumer, IPlayerMovementController
    {
        [Header("Movement Settings")]
        [SerializeField] private float walkSpeed = 2f;
        [SerializeField] private float sprintSpeed = 3f;
        [SerializeField] private float crouchSpeed = 1f;
        [SerializeField] private float moveForce = 50f;
        [SerializeField] private float maxSpeed = 5.2f;
        [SerializeField] private float groundDrag = 8f;
        [SerializeField] private float airDrag = 2f;
        [SerializeField] private float slopeForceMultiplier = 1.5f;
        
        [Header("Sprint Dynamics")]
        [SerializeField] private float sprintAccelerationTime = 1.25f;
        [SerializeField] private float sprintDecelerationTime = 0.25f;
        [SerializeField] private float sprintExitDelayTime = 0.25f;
        [SerializeField] private AnimationCurve sprintAccelerationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] private AnimationCurve sprintDecelerationCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

        [Header("Jump Settings")]
        [SerializeField] private float jumpForce = 4f;
        [SerializeField] private float gravityMultiplier = 2.1f;
        [SerializeField] private float maxFallSpeed = 40f;

        [Header("Ground Detection")]
        [SerializeField] private float groundCheckDistance = 0.2f;
        [SerializeField] private LayerMask groundLayers = -1;
        [SerializeField] private float maxSlopeAngle = 45f;
        [SerializeField] private int groundCheckRays = 5;

        [Header("Crouch Settings")]
        [SerializeField] private float crouchHeight = 1f;
        [SerializeField] private float standingHeight = 2f;
        [SerializeField] private float crouchTransitionSpeed = 10f;
        [SerializeField] private bool crouchToggleMode = true;

        private Rigidbody rb;
        private CapsuleCollider capsuleCollider;
        private InputManager inputManager;
        private StatusEffectManager statusManager;

        private Vector2 moveInput;
        
        private bool isGrounded;
        private bool canJump;
        private bool isSprinting;
        private bool isCrouching;
        private bool crouchButtonPressed;
        
        private Vector3 groundNormal = Vector3.up;
        private float currentGroundAngle;
        
        private Vector3 moveDirection;
        private NoiseSource noiseSource;
        private float currentSpeed;
        
        // Sprint dynamics
        private bool sprintKeyPressed;
        private float sprintTransitionTimer;
        private float currentSprintMultiplier = 1f;
        private float sprintExitTimer;
        private bool sprintExitDelayActive;
        private float transitionStartMultiplier = 1f;

        public bool IsMoving => moveInput.magnitude > 0.1f;
        public bool IsSprinting => isSprinting && IsMoving && !isCrouching;
        public bool IsCrouching => isCrouching;
        public bool IsGrounded => isGrounded;
        public float CurrentSpeed => currentSpeed;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            capsuleCollider = GetComponent<CapsuleCollider>();
            statusManager = GetComponent<StatusEffectManager>();
            inputManager = ServiceLocator.GetService<InputManager>();
            noiseSource = gameObject.GetOrAddComponent<NoiseSource>();

            // DontDestroyOnLoad(gameObject);
            ConfigureRigidbody();
            InitializeSettings();
        }

        private void ConfigureRigidbody()
        {
            rb.freezeRotation = true;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.mass = 1f;
            rb.useGravity = true;
        }

        private void InitializeSettings()
        {
            standingHeight = capsuleCollider.height;
        }

        private void Update()
        {
            if (!ShouldProcessInput()) return;

            UpdateCrouchState();
            UpdateSprintTransition();
            if (IsMoving) { GenerateMovementNoise(); }
        }

        private void FixedUpdate()
        {
            if (!ShouldProcessInput()) return;

            CheckGroundState();
            HandleMovement();
            HandleJump();
            ApplyDrag();
            LimitFallSpeed();
        }

        private void CheckGroundState()
        {
            float checkDistance = capsuleCollider.height * 0.5f + groundCheckDistance;
            Vector3 origin = transform.position;
            
            isGrounded = false;
            groundNormal = Vector3.up;
            currentGroundAngle = 0f;
            
            float capsuleRadius = capsuleCollider.radius;
            
            for (int i = 0; i < groundCheckRays; i++)
            {
                float angle = (360f / groundCheckRays) * i;
                Vector3 rayOrigin = origin + Quaternion.Euler(0, angle, 0) * Vector3.forward * (capsuleRadius * 0.9f);
                
                if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, checkDistance, groundLayers, QueryTriggerInteraction.Ignore))
                {
                    isGrounded = true;
                    groundNormal = Vector3.Slerp(groundNormal, hit.normal, 0.5f);
                }
            }
            
            if (isGrounded)
            {
                groundNormal.Normalize();
                currentGroundAngle = Vector3.Angle(Vector3.up, groundNormal);
                
                if (currentGroundAngle > maxSlopeAngle)
                {
                    isGrounded = false;
                }
            }
        }

        private void HandleMovement()
        {
            Debug.Log("Player input detected");
            if (moveInput.magnitude < 0.1f)
            {
                moveDirection = Vector3.zero;
                return;
            }
            
            Vector3 forward = transform.forward;
            Vector3 right = transform.right;
            
            moveDirection = (forward * moveInput.y + right * moveInput.x).normalized;
            
            if (isGrounded && currentGroundAngle > 0.1f)
            {
                moveDirection = Vector3.ProjectOnPlane(moveDirection, groundNormal).normalized;
            }
            
            float targetSpeed = GetTargetSpeed();
            currentSpeed = Vector3.Project(rb.linearVelocity, moveDirection).magnitude;
            
            if (currentSpeed < targetSpeed)
            {
                float forceMultiplier = isGrounded ? moveForce : moveForce * 0.3f;
                
                if (isGrounded && currentGroundAngle > 15f)
                {
                    forceMultiplier *= slopeForceMultiplier;
                }
                
                Vector3 force = moveDirection * forceMultiplier;
                rb.AddForce(force, ForceMode.Force);
            }
            
            Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            if (horizontalVelocity.magnitude > maxSpeed)
            {
                horizontalVelocity = horizontalVelocity.normalized * maxSpeed;
                rb.linearVelocity = new Vector3(horizontalVelocity.x, rb.linearVelocity.y, horizontalVelocity.z);
            }
        }

        private void UpdateSprintTransition()
        {
            // STEP 1: Determine Sprint State (Input + Exit Delay)
            bool wantsToSprint = sprintKeyPressed && IsMoving && !isCrouching && isGrounded;
            UpdateSprintState(wantsToSprint);
            
            // STEP 2: Calculate Target Speed Multiplier
            float targetMultiplier = isSprinting ? (sprintSpeed / walkSpeed) : 1f;
            
            // STEP 3: Apply Smooth Speed Transitions
            UpdateSpeedMultiplier(targetMultiplier);
        }
        
        private void UpdateSprintState(bool wantsToSprint)
        {
            if (wantsToSprint)
            {
                isSprinting = true;
                sprintExitDelayActive = false; // Cancel any exit delay
            }
            else if (isSprinting && !sprintExitDelayActive)
            {
                // Start exit delay when stopping sprint
                sprintExitDelayActive = true;
                sprintExitTimer = 0f;
            }
            
            // Handle exit delay countdown
            if (sprintExitDelayActive)
            {
                sprintExitTimer += Time.deltaTime;
                if (sprintExitTimer >= sprintExitDelayTime)
                {
                    isSprinting = false;
                    sprintExitDelayActive = false;
                }
            }
        }
        
        private void UpdateSpeedMultiplier(float targetMultiplier)
        {
            const float SNAP_THRESHOLD = 0.01f;
            
            // Snap to target if close enough
            if (Mathf.Abs(currentSprintMultiplier - targetMultiplier) <= SNAP_THRESHOLD)
            {
                currentSprintMultiplier = targetMultiplier;
                sprintTransitionTimer = 0f;
                return;
            }
            
            // Determine if we're starting a new transition
            bool isAccelerating = targetMultiplier > currentSprintMultiplier;
            bool wasAccelerating = transitionStartMultiplier < targetMultiplier;
            bool directionChanged = isAccelerating != wasAccelerating;
            
            // Reset transition when direction changes
            if (directionChanged || sprintTransitionTimer <= 0f)
            {
                sprintTransitionTimer = 0f;
                transitionStartMultiplier = currentSprintMultiplier;
            }
            
            // Get transition parameters
            float transitionTime = isAccelerating ? sprintAccelerationTime : sprintDecelerationTime;
            AnimationCurve transitionCurve = isAccelerating ? sprintAccelerationCurve : sprintDecelerationCurve;
            
            // Apply smooth transition
            sprintTransitionTimer += Time.deltaTime;
            float progress = Mathf.Clamp01(sprintTransitionTimer / transitionTime);
            float curveValue = transitionCurve.Evaluate(progress);
            
            currentSprintMultiplier = Mathf.Lerp(transitionStartMultiplier, targetMultiplier, curveValue);
            
            // Complete transition if finished
            if (progress >= 1f)
            {
                currentSprintMultiplier = targetMultiplier;
                sprintTransitionTimer = 0f;
            }
        }

        private float GetTargetSpeed()
        {
            if (isCrouching) return crouchSpeed;
            return walkSpeed * currentSprintMultiplier;
        }

        private void ApplyDrag()
        {
            float currentDrag = isGrounded ? groundDrag : airDrag;
            
            Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            Vector3 dragForce = -horizontalVelocity * currentDrag;
            
            rb.AddForce(dragForce, ForceMode.Force);
        }

        private void HandleJump()
        {
            if (canJump && isGrounded)
            {
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                canJump = false;
            }
            
            if (rb.linearVelocity.y < 0)
            {
                rb.AddForce(Vector3.down * (gravityMultiplier * 9.81f), ForceMode.Force);
            }
        }

        private void LimitFallSpeed()
        {
            if (rb.linearVelocity.y < -maxFallSpeed)
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, -maxFallSpeed, rb.linearVelocity.z);
            }
        }

        private void UpdateCrouchState()
        {
            if (!capsuleCollider) return;

            float targetHeight = isCrouching ? crouchHeight : standingHeight;
            float currentHeight = capsuleCollider.height;

            if (!isCrouching && currentHeight < standingHeight - 0.01f)
            {
                if (!CanStandUp())
                {
                    isCrouching = true;
                    targetHeight = crouchHeight;
                }
            }

            capsuleCollider.height = Mathf.Lerp(currentHeight, targetHeight, Time.deltaTime * crouchTransitionSpeed);
        }

        private bool CanStandUp()
        {
            float checkDistance = (standingHeight - crouchHeight) * 0.9f;
            Vector3 origin = transform.position + Vector3.up * (crouchHeight * 0.5f);
            int layerMask = ~(1 << gameObject.layer);

            return !Physics.CheckCapsule(
                origin,
                origin + Vector3.up * checkDistance,
                capsuleCollider.radius * 0.9f,
                layerMask,
                QueryTriggerInteraction.Ignore
            );
        }

        protected override void SubscribeToEvents()
        {
            inputManager.OnMoveInput += HandleMoveInput;
            inputManager.OnSprintInput += HandleSprintInput;
            inputManager.OnCrouchInput += HandleCrouchInput;
            inputManager.OnJumpInput += HandleJumpInput;
        }

        protected override void UnsubscribeFromEvents()
        {
            inputManager.OnMoveInput -= HandleMoveInput;
            inputManager.OnSprintInput -= HandleSprintInput;
            inputManager.OnCrouchInput -= HandleCrouchInput;
            inputManager.OnJumpInput -= HandleJumpInput;
        }

        private void HandleMoveInput(Vector2 input) => moveInput = input;
        private void HandleSprintInput(bool sprint)
        {
            if (isCrouching && sprint)
            {
                isCrouching = false;
            }
            sprintKeyPressed = sprint;
        }

        private void HandleCrouchInput(bool pressed)
        {
            if (crouchToggleMode)
            {
                if (pressed && !crouchButtonPressed)
                {
                    if (isSprinting)
                    {
                        isSprinting = false;
                    }

                    if (isCrouching)
                    {
                        if (CanStandUp())
                        {
                            isCrouching = false;
                        }
                    }
                    else
                    {
                        isCrouching = true;
                    }
                }
                crouchButtonPressed = pressed;
            }
            else
            {
                if (isSprinting && pressed)
                {
                    isSprinting = false;
                }
                isCrouching = pressed;
                crouchButtonPressed = pressed;
            }
        }

        private void HandleJumpInput()
        {
            if (isGrounded && !isCrouching)
            {
                canJump = true;
            }
        }

        public void Teleport(Vector3 position)
        {
            rb.position = position;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        public void ResetMovement()
        {
            moveInput = Vector2.zero;
            moveDirection = Vector3.zero;
            currentSpeed = 0;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            isSprinting = false;
            isCrouching = false;
            sprintKeyPressed = false;
            sprintTransitionTimer = 0f;
            currentSprintMultiplier = 1f;
            sprintExitTimer = 0f;
            sprintExitDelayActive = false;
            transitionStartMultiplier = 1f;
        }

        public bool IsWalking() => IsMoving && !IsSprinting && !isCrouching;
        public bool IsRunning() => IsSprinting;
        public bool IsClimbing() => !isGrounded && rb && rb.linearVelocity.y > 0;
        public bool IsFalling() => !isGrounded && rb && rb.linearVelocity.y < -2f;
        
        public Vector3 GetMovementDirection() => moveDirection;
        public float GetVerticalVelocity() => rb ? rb.linearVelocity.y : 0f;
        public Vector3 GetVelocity() => rb ? rb.linearVelocity : Vector3.zero;

        public void ForceJump(float force)
        {
            if (rb)
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, force, rb.linearVelocity.z);
            }
        }

        public void SetCrouchMode(bool toggle)
        {
            crouchToggleMode = toggle;
            if (!toggle && isCrouching)
            {
                isCrouching = false;
            }
        }
        
        public void ForceCrouch(bool crouch)
        {
            isCrouching = crouch;
            if (crouch || CanStandUp()) return;
            isCrouching = true;
        }
        private void GenerateMovementNoise()
        {
            float movementVolume = 55f;
            if (isCrouching) { movementVolume = 30f; }
            else if (isSprinting) { movementVolume = 65f; }
            noiseSource.CreateNoise(movementVolume);
        }
    }
}
# PlayerMovementController Analysis & Fixes

## Critical Issues to Fix:

### 1. Jump Input Handling
```csharp
// Add a jump request flag
private bool jumpRequested;

private void HandleJumpInput()
{
    if (isGrounded && !isCrouching)
    {
        jumpRequested = true;  // Request jump
    }
}

private void HandleJump()
{
    if (jumpRequested && isGrounded && !isCrouching)
    {
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        jumpRequested = false;
    }
}
```

### 2. Fix CurrentSpeed Calculation
```csharp
private void HandleMovement()
{
    if (moveInput.magnitude < 0.1f)
    {
        moveDirection = Vector3.zero;
        currentSpeed = rb.linearVelocity.magnitude; // Still track speed when not inputting
        return;
    }
    
    // ... rest of movement logic
    
    // Safe speed calculation
    if (moveDirection.magnitude > 0.01f)
    {
        currentSpeed = Vector3.Project(rb.linearVelocity, moveDirection).magnitude;
    }
    else
    {
        currentSpeed = 0f;
    }
}
```

### 3. Fix ForceCrouch Logic
```csharp
public void ForceCrouch(bool crouch)
{
    if (crouch)
    {
        isCrouching = true;
        isSprinting = false;
    }
    else if (CanStandUp())
    {
        isCrouching = false;
    }
    // If can't stand up, remain crouched
}
```

### 4. Align Speed System
```csharp
[Header("Movement Settings")]
[SerializeField] private float walkSpeed = 6f;
[SerializeField] private float sprintSpeed = 10f;  // Lower than maxSpeed
[SerializeField] private float crouchSpeed = 3f;
[SerializeField] private float maxSpeed = 12f;      // Absolute limit
```

### 5. Add Noise Generation Cooldown
```csharp
private float lastNoiseTime;
private float noiseCooldown = 0.5f;

private void GenerateMovementNoise()
{
    if (Time.time - lastNoiseTime < noiseCooldown) return;
    
    float movementVolume = 55f;
    if (isCrouching) movementVolume = 30f;
    else if (isSprinting) movementVolume = 65f;
    
    noiseSource.CreateNoise(movementVolume);
    lastNoiseTime = Time.time;
}
```

### 6. Simplify Sprint Property
```csharp
public bool IsSprinting => isSprinting && IsMoving;  // Remove redundant crouch check
```

### 7. Add Null Safety
```csharp
public Vector3 GetVelocity() => rb ? rb.linearVelocity : Vector3.zero;
public float GetVerticalVelocity() => rb ? rb.linearVelocity.y : 0f;

private void ResetMovement()
{
    moveInput = Vector2.zero;
    moveDirection = Vector3.zero;
    currentSpeed = 0;
    if (rb)
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }
    isSprinting = false;
    isCrouching = false;
    jumpRequested = false;  // Clear jump request
}
```

## Additional Recommendations:

1. **Status Effect Integration**: GetTargetSpeed() should check StatusEffectManager for speed modifiers
2. **Ground Check Optimization**: Cache ground check results if called multiple times per frame
3. **State Validation**: Add state validation in Awake/Start to ensure all settings are valid
4. **Debug Visualization**: Add debug rays for ground checking in editor
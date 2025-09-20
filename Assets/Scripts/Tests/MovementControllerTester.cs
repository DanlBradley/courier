using UnityEngine;
using Inputs;

namespace Tests
{
    public class MovementControllerTester : MonoBehaviour
    {
        [Header("Testing Options")]
        [SerializeField] private bool showDebugInfo = true;
        [SerializeField] private bool visualizeGroundCheck = true;
        [SerializeField] private bool showMovementVectors = true;
        
        private PlayerMovementController controller;
        private Rigidbody rb;
        
        private float smoothedSpeed;
        private float maxRecordedSpeed;
        private Vector3 lastPosition;
        private float distanceTraveled;
        
        private void Start()
        {
            controller = GetComponent<PlayerMovementController>();
            rb = GetComponent<Rigidbody>();
            lastPosition = transform.position;
        }
        
        private void Update()
        {
            if (!showDebugInfo || !controller) return;
            
            float currentSpeed = rb.linearVelocity.magnitude;
            smoothedSpeed = Mathf.Lerp(smoothedSpeed, currentSpeed, Time.deltaTime * 5f);
            maxRecordedSpeed = Mathf.Max(maxRecordedSpeed, currentSpeed);
            
            float frameDistance = Vector3.Distance(transform.position, lastPosition);
            distanceTraveled += frameDistance;
            lastPosition = transform.position;
        }
        
        private void OnGUI()
        {
            if (!showDebugInfo || !controller) return;
            
            int lineHeight = 20;
            int labelWidth = 400;
            int buttonHeight = 25;
            int padding = 10;
            
            // Calculate starting Y position from bottom
            int totalHeight = (lineHeight * 9) + buttonHeight + padding;
            int yOffset = Screen.height - totalHeight - padding;
            int xOffset = padding; // Bottom-left corner
            
            GUI.Label(new Rect(xOffset, yOffset, labelWidth, lineHeight), $"Speed: {smoothedSpeed:F2} m/s (Max: {maxRecordedSpeed:F2})");
            yOffset += lineHeight;
            
            GUI.Label(new Rect(xOffset, yOffset, labelWidth, lineHeight), $"Current Speed: {controller.CurrentSpeed:F2} m/s");
            yOffset += lineHeight;
            
            // Access private fields through reflection
            var type = typeof(PlayerMovementController);
            var sprintMultiplierField = type.GetField("currentSprintMultiplier", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var walkSpeedField = type.GetField("walkSpeed", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var sprintSpeedField = type.GetField("sprintSpeed", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var crouchSpeedField = type.GetField("crouchSpeed", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            float sprintMultiplier = sprintMultiplierField != null ? (float)sprintMultiplierField.GetValue(controller) : 1f;
            float walkSpeed = walkSpeedField != null ? (float)walkSpeedField.GetValue(controller) : 1f;
            float sprintSpeed = sprintSpeedField != null ? (float)sprintSpeedField.GetValue(controller) : 8f;
            float crouchSpeed = crouchSpeedField != null ? (float)crouchSpeedField.GetValue(controller) : 1f;
            
            GUI.Label(new Rect(xOffset, yOffset, labelWidth, lineHeight), $"Sprint Multiplier: {sprintMultiplier:F2}x");
            yOffset += lineHeight;
            
            // Display base speed based on current state
            float baseSpeed = controller.IsCrouching ? crouchSpeed : walkSpeed;
            GUI.Label(new Rect(xOffset, yOffset, labelWidth, lineHeight), $"Base Speed: {baseSpeed:F2} m/s (Walk: {walkSpeed:F1}, Sprint: {sprintSpeed:F1})");
            yOffset += lineHeight;
            
            GUI.Label(new Rect(xOffset, yOffset, labelWidth, lineHeight), $"Velocity: {rb.linearVelocity:F2}");
            yOffset += lineHeight;
            
            GUI.Label(new Rect(xOffset, yOffset, labelWidth, lineHeight), $"Grounded: {controller.IsGrounded}");
            yOffset += lineHeight;

            GUI.Label(new Rect(xOffset, yOffset, labelWidth, lineHeight), $"Moving: {controller.IsMoving} | Sprinting: {controller.IsSprinting} | Crouching: {controller.IsCrouching}");
            yOffset += lineHeight;
            
            GUI.Label(new Rect(xOffset, yOffset, labelWidth, lineHeight), $"Distance Traveled: {distanceTraveled:F2} m");
            yOffset += lineHeight;
            
            GUI.Label(new Rect(xOffset, yOffset, labelWidth, lineHeight), $"FPS: {1f / Time.deltaTime:F0} | Fixed Delta: {Time.fixedDeltaTime * 1000:F1}ms");
            yOffset += lineHeight;
            
            if (GUI.Button(new Rect(xOffset, yOffset, 100, buttonHeight), "Reset Stats"))
            {
                maxRecordedSpeed = 0;
                distanceTraveled = 0;
            }
        }
        
        private void OnDrawGizmos()
        {
            if (!controller) return;
            
            if (visualizeGroundCheck)
            {
                Gizmos.color = controller.IsGrounded ? Color.green : Color.red;
                Gizmos.DrawWireSphere(transform.position, 0.2f);
                
                CapsuleCollider capsule = GetComponent<CapsuleCollider>();
                if (capsule)
                {
                    float checkDistance = capsule.height * 0.5f + 0.2f;
                    Gizmos.DrawLine(transform.position, transform.position + Vector3.down * checkDistance);
                    
                    for (int i = 0; i < 5; i++)
                    {
                        float angle = (360f / 5) * i;
                        Vector3 rayOrigin = transform.position + Quaternion.Euler(0, angle, 0) * Vector3.forward * (capsule.radius * 0.9f);
                        Gizmos.DrawLine(rayOrigin, rayOrigin + Vector3.down * checkDistance);
                    }
                }
            }
            
            if (showMovementVectors && rb)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawRay(transform.position, rb.linearVelocity);
                
                Gizmos.color = Color.yellow;
                Gizmos.DrawRay(transform.position + Vector3.up * 0.1f, transform.forward * 2f);
            }
        }
    }
}
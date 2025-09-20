using GameServices;
using UnityEngine;

namespace Inputs
{
    public class FPSCamera : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform playerBody;
        [SerializeField] private Transform cameraTransform;
        
        [Header("Settings")]
        [SerializeField] private float mouseSensitivity = 2f;
        [SerializeField] private float maxLookAngle = 80f;
        [SerializeField] private bool invertYAxis = false;
        
        private float xRotation = 0f;
        private bool isActive = true;
        private float accumulatedRotation = 0;
        
        private void Start()
        {
            // Lock cursor to center of screen
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            
            // If references aren't set, try to find them
            if (!playerBody) playerBody = transform.root;
            
            if (!cameraTransform) cameraTransform = transform;
            
            // Initialize rotation to current camera rotation
            if (!cameraTransform) return;
            xRotation = cameraTransform.localEulerAngles.x;
            if (xRotation > 180f) xRotation -= 360f;
        }
        
        private void FixedUpdate()
        {
            if (!isActive) return;
            if (!playerBody) return;
            if (GameStateManager.Instance.CurrentState != GameState.Exploration) return;
            
            // Apply horizontal rotation through Rigidbody in FixedUpdate to work with physics
            Rigidbody rb = playerBody.GetComponent<Rigidbody>();
            if (rb && accumulatedRotation != 0)
            {
                Quaternion deltaRotation = Quaternion.Euler(0, accumulatedRotation, 0);
                rb.MoveRotation(rb.rotation * deltaRotation);
                accumulatedRotation = 0;
            }
        }
        
        private void Update()
        {
            if (!isActive) return;
            if (GameStateManager.Instance.CurrentState != GameState.Exploration) return;
            
            // Get raw mouse input - Unity's Input System handles frame-rate independence internally
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
            
            // Accumulate horizontal rotation to apply in FixedUpdate
            if (Mathf.Abs(mouseX) > 0.01f)
            {
                accumulatedRotation += mouseX;
            }
            
            // Rotate the camera vertically (this can stay in Update as it's not physics-related)
            if (cameraTransform)
            {
                float rotationAmount = invertYAxis ? mouseY : -mouseY;
                xRotation += rotationAmount;
                xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);
                cameraTransform.localRotation = Quaternion.Euler(xRotation, 0, 0);
            }
        }
        
        public void SetActive(bool active)
        {
            isActive = active;
            if (active)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
        
        public void SetSensitivity(float sensitivity)
        {
            mouseSensitivity = sensitivity;
        }
        
        public void SetInvertY(bool invert)
        {
            invertYAxis = invert;
        }
    }
}
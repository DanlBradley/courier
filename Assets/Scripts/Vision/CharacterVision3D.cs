using System;
using System.Collections.Generic;
using GameServices;
using Inputs;
using UnityEngine;

namespace Vision
{
    /// <summary>
    /// Logic for character vision which is used for several systems to identify what a character can "see"
    /// through a cone of variable size. 3D version with improved height handling and fog of war.
    /// </summary>
    public class CharacterVision3D : MonoBehaviour
    {
        [Header("Vision Settings")]
        [SerializeField] private float visionRange = 10f;
        [SerializeField] private float visionAngle = 60f;
        [SerializeField] private LayerMask entityLayerMask = -1;
        [SerializeField] private LayerMask obstacleLayerMask = -1;
        
        [Header("Fog of War Settings")]
        [SerializeField] private bool enableFogOfWar = true;
        [SerializeField] private LayerMask affectedLayers = -1; // What layers get fog of war effect
        [SerializeField] private int hiddenLayer = 31; // Layer to move hidden objects to
        [SerializeField] private int visibleLayer = 0; // Default layer for visible objects
        
        [Header("3D Vision Settings")]
        [SerializeField] private float visionHeight = 2f; // How tall the vision cone is
        [SerializeField] private bool ignoreHeightDifferences = true; // For top-down games
        [SerializeField] private float maxHeightDifference = 3f; // Max Y difference to detect
        
        [Header("Debug")]
        [SerializeField] private bool showDebugGizmos = true;
        [SerializeField] private Color visionConeColor = Color.yellow;
        
        private List<GameObject> allEntitiesInRange = new();
        private List<GameObject> visibleEntities = new();
        private List<GameObject> previouslyVisible = new(); // Track what was visible last frame
        private PlayerMovementController pmc;
        private Vector3 eyePosition; // Player's eye level
        private Vector3 _heading;

        private void Start()
        {
            pmc = GameManager.Instance.GetPlayer().GetComponent<PlayerMovementController>();
            UpdateEyePosition();
        }

        private void Update()
        {
            UpdateEyePosition();
            UpdateHeading();
            
            if (Time.frameCount % 5 == 0) { UpdateVision(); }
        }

        private void UpdateEyePosition()
        {
            eyePosition = transform.position + Vector3.up * (visionHeight * 0.8f);
        }

        private void UpdateHeading()
        {
            Vector3 movementDirection = pmc.GetMovementDirection();
            if (movementDirection.magnitude > 0.1f) { _heading = movementDirection.normalized; }
        }

        private void UpdateVision()
        {
            visibleEntities.Clear();
            allEntitiesInRange.Clear();

            // Find all entities in range using 3D overlap sphere
            LayerMask searchMask = entityLayerMask | (1 << hiddenLayer);
            Collider[] observableColliders = Physics.OverlapSphere(
                eyePosition, 
                visionRange, 
                searchMask
            );

            foreach (var observableCollider in observableColliders)
            {
                if (observableCollider.gameObject == gameObject) continue;
                
                // Check height difference if not ignoring
                if (!ignoreHeightDifferences)
                {
                    float heightDiff = Mathf.Abs(observableCollider.transform.position.y - transform.position.y);
                    if (heightDiff > maxHeightDifference) continue;
                }
                
                allEntitiesInRange.Add(observableCollider.gameObject);

                if (CanSee(observableCollider.transform.position))
                {
                    visibleEntities.Add(observableCollider.gameObject);
                    Debug.Log($"Can see {observableCollider.name}!");
                }
            }
            
            // Apply fog of war effect
            if (enableFogOfWar)
            {
                UpdateFogOfWar();
            }
            
            Debug.Log($"Vision Update: {allEntitiesInRange.Count} in range, {visibleEntities.Count} visible");
        }

        public bool CanSee(Vector3 targetPosition)
        {
            return IsInVisionCone(targetPosition) && HasLineOfSight(targetPosition);
        }

        private bool IsInVisionCone(Vector3 targetPosition)
        {
            Vector3 directionToTarget;
            Vector3 facingDirection;
            
            if (ignoreHeightDifferences)
            {
                // For top-down games - project to horizontal plane
                Vector3 flatTarget = new Vector3(targetPosition.x, transform.position.y, targetPosition.z);
                Vector3 flatEye = new Vector3(eyePosition.x, transform.position.y, eyePosition.z);
                
                directionToTarget = (flatTarget - flatEye).normalized;
                facingDirection = GetFacingDirection3D();
                // Project facing direction to horizontal plane too
                facingDirection = new Vector3(facingDirection.x, 0, facingDirection.z).normalized;
            }
            else
            {
                // Full 3D vision cone
                directionToTarget = (targetPosition - eyePosition).normalized;
                facingDirection = GetFacingDirection3D();
            }
            
            float angleToTarget = Vector3.Angle(facingDirection, directionToTarget);
            return angleToTarget <= visionAngle * 0.5f;
        }
        
        private bool HasLineOfSight(Vector3 targetPosition)
        {
            Vector3 startPos = eyePosition;
            Vector3 endPos = targetPosition;
            
            // Adjust target position to be at a reasonable height (center of target)
            if (ignoreHeightDifferences)
            {
                endPos.y = startPos.y; // Same height for top-down
            }
            
            float distance = Vector3.Distance(startPos, endPos);
            if (distance > visionRange) return false;

            Vector3 direction = (endPos - startPos).normalized;
            
            // Use 3D raycast
            bool hasObstacle = Physics.Raycast(startPos, direction, distance, obstacleLayerMask);
            
            // Debug ray
            if (showDebugGizmos)
            {
                Color rayColor = hasObstacle ? Color.red : Color.green;
                Debug.DrawRay(startPos, direction * distance, rayColor, 0.1f);
            }
            
            return !hasObstacle;
        }

        public Vector3 GetFacingDirection3D()
        {
            // Use movement direction instead of mouse heading
            Vector3 movementDirection = pmc.GetMovementDirection();
            
            // If moving, use movement direction
            if (movementDirection.magnitude > 0.1f)
            {
                return movementDirection.normalized;
            }
            
            // If not moving, keep last heading direction or default forward
            if (_heading.magnitude > 0.1f)
            {
                return _heading.normalized;
            }
            
            // Fallback to transform forward
            return transform.forward;
        }

        // Legacy 2D compatibility method
        public Vector2 GetFacingDirection()
        {
            Vector3 heading3D = GetFacingDirection3D();
            return new Vector2(heading3D.x, heading3D.z);
        }
        
        // ======= PUBLIC METHODS =======
        public List<GameObject> GetVisibleEntities() { return new List<GameObject>(visibleEntities); }
        public List<GameObject> GetAllEntitiesInRange() { return new List<GameObject>(allEntitiesInRange); }

        public float VisionRange => visionRange;
        public float VisionAngle => visionAngle;
        public Vector3 EyePosition => eyePosition;

        // Additional utility methods for 3D movement compatibility
        public bool IsGrounded()
        {
            // Simple ground check - you might want to make this more sophisticated
            return Physics.Raycast(transform.position, Vector3.down, 1.1f);
        }

        public Vector3 GetMovementDirection()
        {
            return pmc.GetMovementDirection();
        }
        
        public bool IsMoving()
        {
            return pmc.IsWalking();
        }
        
        // ======= FOG OF WAR SYSTEM =======
        private void UpdateFogOfWar()
        {
            // First, find all objects that need fog of war applied
            List<GameObject> allAffectedObjects = new List<GameObject>();
            
            // Get all objects in a larger radius for fog of war
            LayerMask searchLayers = affectedLayers | (1 << hiddenLayer);
            Collider[] fogColliders = Physics.OverlapSphere(
                eyePosition, 
                visionRange * 2f, // Larger radius for fog effect
                searchLayers
            );
            
            foreach (var collider in fogColliders)
            {
                if (collider.gameObject != gameObject) // Don't affect self
                {
                    allAffectedObjects.Add(collider.gameObject);
                }
            }
            
            // Apply visibility states
            foreach (var obj in allAffectedObjects)
            {
                bool isVisible = visibleEntities.Contains(obj);
                SetObjectVisibility(obj, isVisible);
            }
        }
        
        private void SetObjectVisibility(GameObject obj, bool isVisible)
        {
            if (isVisible)
            {
                // Make object visible
                SetObjectLayer(obj, visibleLayer);
            }
            else
            {
                // Hide object
                SetObjectLayer(obj, hiddenLayer);
            }
        }
        
        private void SetObjectLayer(GameObject obj, int layer)
        {
            // Set layer for the object and all its children
            obj.layer = layer;
            
            // Also set layer for all child objects
            foreach (Transform child in obj.transform)
            {
                SetObjectLayer(child.gameObject, layer);
            }
        }
        
        // Optional: Method to restore all objects (for cleanup)
        public void RestoreAllObjectsVisibility()
        {
            if (!enableFogOfWar) return;
            
            Collider[] allColliders = Physics.OverlapSphere(
                eyePosition, 
                visionRange * 3f, 
                affectedLayers
            );
            
            foreach (var collider in allColliders)
            {
                if (collider.gameObject != gameObject)
                {
                    SetObjectLayer(collider.gameObject, visibleLayer);
                }
            }
        }

        // ======= DEBUG VISUALIZATION =======
        private void OnDrawGizmos()
        {
            if (!showDebugGizmos) return;
            
            UpdateEyePosition();
            
            // Draw vision range sphere
            Gizmos.color = Color.white * 0.3f;
            Gizmos.DrawWireSphere(eyePosition, visionRange);
            
            // Draw vision cone
            Vector3 facingDirection = GetFacingDirection3D();
            if (facingDirection.magnitude > 0.1f)
            {
                Gizmos.color = visionConeColor * 0.5f;
                
                // Calculate cone edges
                float halfAngle = visionAngle * 0.5f;
                Vector3 leftBoundary = Quaternion.AngleAxis(-halfAngle, Vector3.up) * facingDirection;
                Vector3 rightBoundary = Quaternion.AngleAxis(halfAngle, Vector3.up) * facingDirection;
                
                // Draw cone lines
                Gizmos.DrawLine(eyePosition, eyePosition + leftBoundary * visionRange);
                Gizmos.DrawLine(eyePosition, eyePosition + rightBoundary * visionRange);
                Gizmos.DrawLine(eyePosition, eyePosition + facingDirection * visionRange);
                
                // Draw arc (simplified)
                Vector3 prevPoint = eyePosition + leftBoundary * visionRange;
                for (int i = 1; i <= 10; i++)
                {
                    float angle = Mathf.Lerp(-halfAngle, halfAngle, i / 10f);
                    Vector3 point = eyePosition + Quaternion.AngleAxis(angle, Vector3.up) * facingDirection * visionRange;
                    Gizmos.DrawLine(prevPoint, point);
                    prevPoint = point;
                }
            }
            
            // Draw eye position
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(eyePosition, 0.2f);
        }

        private void OnDrawGizmosSelected()
        {
            // Draw visible entities
            Gizmos.color = Color.green;
            foreach (var entity in visibleEntities)
            {
                if (entity != null)
                {
                    Gizmos.DrawWireSphere(entity.transform.position, 0.5f);
                    Gizmos.DrawLine(eyePosition, entity.transform.position);
                }
            }
            
            // Draw entities in range but not visible
            Gizmos.color = Color.yellow;
            foreach (var entity in allEntitiesInRange)
            {
                if (entity != null && !visibleEntities.Contains(entity))
                {
                    Gizmos.DrawWireCube(entity.transform.position, Vector3.one * 0.5f);
                }
            }
        }
    }
}
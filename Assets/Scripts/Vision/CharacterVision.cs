using System;
using System.Collections.Generic;
using GameServices;
using Inputs;
using UnityEngine;

namespace Vision
{
    /// <summary>
    /// Logic for character vision which is used for several systems to identify what a character can "see"
    /// through a cone of variable size.
    /// </summary>
    public class CharacterVision : MonoBehaviour
    {
        [SerializeField] private float visionRange;
        [SerializeField] private LayerMask entityLayerMask;
        [SerializeField] private LayerMask obstacleLayerMask;
        [SerializeField] private float visionAngle;
        
        private List<GameObject> allEntitiesInRange = new();
        private List<GameObject> visibleEntities = new();
        private PlayerMovementController pmc;

        private void Start()
        {
            pmc = GameManager.Instance.GetPlayer().GetComponent<PlayerMovementController>();
        }

        private void Update()
        {
            //TEST METHOD TO CHECK VISION TEMPORARILY:
            if (Input.GetKeyDown(KeyCode.Space)) { UpdateVision(); }
        }

        private void UpdateVision()
        {
            visibleEntities.Clear();
            allEntitiesInRange.Clear();

            // Find all entities in range
            Collider2D[] observableColliders = Physics2D.OverlapCircleAll(
                transform.position, 
                visionRange, 
                entityLayerMask
            );

            foreach (var observableCollider in observableColliders)
            {
                if (observableCollider.gameObject == gameObject) continue;
                allEntitiesInRange.Add(observableCollider.gameObject);

                if (CanSee(observableCollider.transform.position))
                {
                    // Debug.Log($"Can see {observableCollider.name}!");
                    visibleEntities.Add(observableCollider.gameObject);
                }
            }
        }

        public bool CanSee(Vector3 targetPosition)
        {
            return IsInVisionCone(targetPosition) && HasLineOfSight(targetPosition);
        }

        private bool IsInVisionCone(Vector3 targetPosition)
        {
            Vector2 directionToTarget = ((Vector2)targetPosition - (Vector2)transform.position).normalized;
            Vector2 facingDirection = GetFacingDirection();
            
            float angleToTarget = Vector2.Angle(facingDirection, directionToTarget);
            return angleToTarget <= visionAngle * 0.5f;
        }
        
        private bool HasLineOfSight(Vector3 targetPosition)
        {
            float distance = Vector2.Distance(transform.position, targetPosition);
            if (distance > visionRange) return false;

            Vector2 direction = ((Vector2)targetPosition - (Vector2)transform.position).normalized;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, distance, obstacleLayerMask);
            
            return hit.collider == null;
        }

        public Vector2 GetFacingDirection()
        {
            // Debug.Log($"Returning facing direction: {pmc.GetHeading()}");
            // return pmc.GetHeading();
            return Vector2.zero;
        }
        
        // ======= PUBLIC METHODS =======
        public List<GameObject> GetVisibleEntities() { return new List<GameObject>(visibleEntities); }

        public float VisionRange => visionRange;
        public float VisionAngle => visionAngle;
    }
}

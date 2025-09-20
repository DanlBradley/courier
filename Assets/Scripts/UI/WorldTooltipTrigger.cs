using UnityEngine;
using Interfaces;
using GameServices;
using System.Collections;

namespace UI
{
    /// <summary>
    /// Tooltip trigger for 2D world objects.
    /// Uses 2D raycasting from mouse position.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(ITooltippable))]
    public class WorldTooltipTrigger : MonoBehaviour
    {
        private ITooltippable tooltippable;
        private TooltipService tooltipService;
        private Coroutine showTooltipCoroutine;
        private bool isMouseOver;
        
        [Header("Settings")]
        [SerializeField] private LayerMask raycastLayers = -1; // All layers by default
        [SerializeField] private Vector3 worldPositionOffset = Vector3.up * 2f; // Offset above object
        
        private Camera mainCamera;
        private Collider2D objectCollider;
        
        private void Awake()
        {
            tooltippable = GetComponent<ITooltippable>();
            if (tooltippable == null)
            {
                Debug.LogError($"WorldTooltipTrigger2D on {gameObject.name} requires a component implementing ITooltippable!");
                enabled = false;
                return;
            }
            
            objectCollider = GetComponent<Collider2D>();
            if (objectCollider == null)
            {
                Debug.LogError($"WorldTooltipTrigger2D on {gameObject.name} requires a Collider2D!");
                enabled = false;
                return;
            }
            
            mainCamera = Camera.main;
        }
        
        private void Start()
        {
            tooltipService = ServiceLocator.GetService<TooltipService>();
            if (tooltipService == null)
            {
                Debug.LogError("TooltipService not found in ServiceLocator!");
                enabled = false;
            }
        }
        
        private void Update()
        {
            if (!enabled || tooltipService == null || mainCamera == null) return;
            
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0f;
            Collider2D hit = Physics2D.OverlapPoint(mouseWorldPos, raycastLayers);
            
            bool hitThisObject = false;
            
            if (hit != null && hit == objectCollider)
            {
                hitThisObject = true;
                if (!isMouseOver) { OnMouseEnterObject(); }
            }
            
            if (!hitThisObject && isMouseOver) { OnMouseExitObject(); }
            isMouseOver = hitThisObject;
        }
        
        private void OnMouseEnterObject()
        {
            if (showTooltipCoroutine != null)
                StopCoroutine(showTooltipCoroutine);
                
            showTooltipCoroutine = StartCoroutine(ShowTooltipAfterDelay());
        }
        
        private void OnMouseExitObject()
        {
            if (showTooltipCoroutine != null)
            {
                StopCoroutine(showTooltipCoroutine);
                showTooltipCoroutine = null;
            }
            
            tooltipService?.HideTooltip();
        }
        
        private IEnumerator ShowTooltipAfterDelay()
        {
            float delay = tooltippable.GetTooltipDelay();
            if (delay > 0f)
                yield return new WaitForSeconds(delay);
            
            if (isMouseOver && tooltipService != null)
            {
                // Show at world position with offset
                Vector3 tooltipPosition = transform.position + worldPositionOffset;
                tooltipService.ShowTooltip(tooltippable, tooltipPosition);
                
                // Update position while showing
                while (isMouseOver && tooltipService.IsTooltipActive)
                {
                    tooltipPosition = transform.position + worldPositionOffset;
                    tooltipService.UpdatePosition(tooltipPosition);
                    yield return null;
                }
            }
        }
        
        private void OnDisable()
        {
            if (showTooltipCoroutine != null)
            {
                StopCoroutine(showTooltipCoroutine);
                showTooltipCoroutine = null;
            }
            
            tooltipService?.HideTooltip();
            isMouseOver = false;
        }
        
        private void OnDestroy()
        {
            OnDisable();
        }
        
        // Visual debugging
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position + worldPositionOffset, 0.1f);
        }
    }
}
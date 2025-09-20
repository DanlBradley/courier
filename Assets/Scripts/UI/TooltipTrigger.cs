using UnityEngine;
using UnityEngine.EventSystems;
using Interfaces;
using GameServices;
using System.Collections;

namespace UI
{
    [RequireComponent(typeof(ITooltippable))]
    public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private ITooltippable tooltippable;
        private TooltipService tooltipService;
        private Coroutine showTooltipCoroutine;
        
        [Header("Settings")]
        public bool useWorldPosition;
        public Transform worldPositionTransform;
        
        private void Awake()
        {
            tooltippable = GetComponent<ITooltippable>();
            if (tooltippable != null) return;
            Debug.LogError($"TooltipTrigger on {gameObject.name} requires a component " +
                           $"implementing ITooltippable!");
            enabled = false;
        }
        
        private void Start()
        {
            tooltipService = ServiceLocator.GetService<TooltipService>();
            if (tooltipService != null) return;
            Debug.LogError("TooltipSystem not found in ServiceLocator!");
            enabled = false;
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (tooltippable == null || tooltipService == null) return;
            
            // Don't show tooltips while dragging
            if (eventData.dragging) return;
            
            showTooltipCoroutine = StartCoroutine(ShowTooltipAfterDelay());
        }
        
        public void OnPointerExit(PointerEventData eventData)
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
            if (delay > 0f) yield return new WaitForSeconds(delay);
            
            if (useWorldPosition && worldPositionTransform != null)
            {
                tooltipService.ShowTooltip(tooltippable, worldPositionTransform.position);
                
                // Update position while showing
                while (tooltipService.IsTooltipActive)
                {
                    tooltipService.UpdatePosition(worldPositionTransform.position);
                    yield return null;
                }
            }
            else
            {
                tooltipService.ShowTooltipAtCursor(tooltippable);
                
                // Update position while showing
                while (tooltipService.IsTooltipActive)
                {
                    tooltipService.UpdatePositionAtCursor();
                    yield return null;
                }
            }
        }
        
        private void OnDestroy() { OnDisable(); }
        private void OnDisable()
        {
            if (showTooltipCoroutine != null)
            {
                StopCoroutine(showTooltipCoroutine);
                showTooltipCoroutine = null;
            }
            tooltipService?.HideTooltip();
        }
    }
}
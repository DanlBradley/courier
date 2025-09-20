using Interfaces;
using TMPro;
using UnityEngine;
using Utils;

namespace GameServices
{
    public class TooltipService : Service
    {
        [Header("UI References")]
        [SerializeField] private GameObject tooltipPanel;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI detailsText;
        
        [Header("Settings")]
        [SerializeField] private Vector2 cursorOffset = new Vector2(10f, -10f);
        [SerializeField] private Vector2 worldOffset = new Vector2(0f, 50f);
        
        private RectTransform tooltipRect;
        private Canvas canvas;
        private Camera mainCamera;
        
        public bool IsTooltipActive => tooltipPanel != null && tooltipPanel.activeSelf;

        public override void Initialize()
        {
            if (tooltipPanel != null)
            {
                tooltipRect = tooltipPanel.GetComponent<RectTransform>();
                tooltipPanel.SetActive(false);
            }
    
            canvas = GetComponentInParent<Canvas>();
            if (canvas == null) { canvas = FindFirstObjectByType<Canvas>(); }
            else
            {
                Debug.Log($"TooltipService: Found canvas with render mode {canvas.renderMode}");
                if (canvas.worldCamera != null) Debug.Log($"Canvas camera: {canvas.worldCamera.name}");
            }
            mainCamera = Camera.main;
            Logs.Log("Tooltip Service initialized.", "GameServices");
        }
        
        public void ShowTooltip(ITooltippable tooltippable, Vector3 worldPosition)
        {
            if (tooltippable == null || tooltipPanel == null) return;
            
            SetTooltipContent(tooltippable);
            tooltipPanel.SetActive(true);
            UpdatePosition(worldPosition);
        }
        
        public void ShowTooltipAtCursor(ITooltippable tooltippable)
        {
            if (tooltippable == null || tooltipPanel == null) return;
            
            SetTooltipContent(tooltippable);
            tooltipPanel.SetActive(true);
            UpdatePositionAtCursor();
        }
        
        public void HideTooltip()
        {
            if (tooltipPanel != null)
                tooltipPanel.SetActive(false);
        }
        
        public void UpdatePosition(Vector3 worldPosition)
        {
            if (!IsTooltipActive || canvas == null || mainCamera == null) return;
            
            // Convert world position to screen position
            Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPosition);
            
            // Convert to canvas position
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                screenPos,
                canvas.worldCamera,
                out Vector2 localPoint);
            
            // Apply offset and set position
            tooltipRect.anchoredPosition = localPoint + worldOffset;
            ClampToScreen();
        }
        
        public void UpdatePositionAtCursor()
        {
            if (!IsTooltipActive || tooltipRect == null) return;
    
            // Simple approach - set position directly relative to screen
            Vector2 position = Input.mousePosition;
    
            // If canvas is overlay mode
            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                tooltipRect.position = position + (Vector2)tooltipRect.TransformVector(cursorOffset);
            }
            else
            {
                // For camera-based canvases
                RectTransformUtility.ScreenPointToWorldPointInRectangle(
                    tooltipRect,
                    position,
                    canvas.worldCamera,
                    out Vector3 worldPoint);
                tooltipRect.position = worldPoint + (Vector3)cursorOffset;
            }
    
            ClampToScreen();
        }
        
        private void SetTooltipContent(ITooltippable tooltippable)
        {
            if (titleText != null)
                titleText.text = tooltippable.GetTooltipTitle();
            
            if (descriptionText != null)
            {
                string desc = tooltippable.GetTooltipDescription();
                descriptionText.text = desc;
                descriptionText.gameObject.SetActive(!string.IsNullOrEmpty(desc));
            }
            
            if (detailsText != null)
            {
                string details = tooltippable.GetTooltipDetails();
                detailsText.text = details;
                detailsText.gameObject.SetActive(!string.IsNullOrEmpty(details));
            }
        }
        
        private void ClampToScreen()
        {
            if (tooltipRect == null || canvas == null) return;
            
            // Get canvas bounds
            RectTransform canvasRect = canvas.transform as RectTransform;
            Vector2 canvasSize = canvasRect.rect.size * canvas.scaleFactor;
            
            // Calculate current tooltip bounds
            Vector3[] corners = new Vector3[4];
            tooltipRect.GetWorldCorners(corners);
            
            // Convert to local space
            for (int i = 0; i < 4; i++)
                corners[i] = canvasRect.InverseTransformPoint(corners[i]);
            
            // Calculate adjustments
            float leftEdge = corners[0].x;
            float rightEdge = corners[2].x;
            float bottomEdge = corners[0].y;
            float topEdge = corners[2].y;
            
            Vector2 adjustment = Vector2.zero;
            
            float halfWidth = canvasSize.x / 2f;
            float halfHeight = canvasSize.y / 2f;
            
            if (rightEdge > halfWidth)
                adjustment.x = halfWidth - rightEdge - 10f;
            else if (leftEdge < -halfWidth)
                adjustment.x = -halfWidth - leftEdge + 10f;
            
            if (topEdge > halfHeight)
                adjustment.y = halfHeight - topEdge - 10f;
            else if (bottomEdge < -halfHeight)
                adjustment.y = -halfHeight - bottomEdge + 10f;
            
            tooltipRect.anchoredPosition += adjustment;
        }
    }
}
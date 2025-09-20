using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Vision
{
    [RequireComponent(typeof(CharacterVision))]
    public class PlayerVisionRenderer : MonoBehaviour
    {
        [Header("Visual Settings")]
        [SerializeField] private float dimmedAlpha = 0.4f;
        [SerializeField] private float transitionSpeed = 3f;
        [SerializeField] private LayerMask renderingLayerMask = -1; // What objects to apply dimming to
        
        [Header("Light Cone Effect")]
        [SerializeField] private bool enableLightCone = true;
        [SerializeField] private Color lightConeColor = new Color(1f, 1f, 0.8f, 0.15f);
        [SerializeField] private float lightIntensity = 0.6f;
        
        // Components
        private CharacterVision characterVision;
        private Light2D visionLight;
        
        // Runtime tracking
        private Dictionary<Renderer, VisibilityState> trackedRenderers = new();
        
        private class VisibilityState
        {
            public float currentAlpha = 1f;
            public Color originalColor;
            public Material originalMaterial;
            public Material dimmingMaterial;
            public bool isVisible;
        }

        private void Awake()
        {
            characterVision = GetComponent<CharacterVision>();
            if (enableLightCone) { SetupVisionLight(); }
        }

        private void Start() { RefreshTrackedObjects(); }

        private void Update()
        {
            if (enableLightCone) { UpdateVisionDirection(); }
            UpdateObjectVisibility();
        }

        private void SetupVisionLight()
        {
            // Create or get Light2D component for the visual cone effect
            visionLight = GetComponent<Light2D>();
            if (visionLight == null)
            { visionLight = gameObject.AddComponent<Light2D>(); }
            
            visionLight.lightType = Light2D.LightType.Point;
            visionLight.pointLightOuterRadius = characterVision.VisionRange;
            visionLight.pointLightInnerRadius = characterVision.VisionRange * 0.3f;
            visionLight.pointLightOuterAngle = characterVision.VisionAngle;
            visionLight.pointLightInnerAngle = characterVision.VisionAngle * 0.8f;
            visionLight.color = lightConeColor;
            visionLight.intensity = lightIntensity;
            visionLight.falloffIntensity = 0.4f;
        }

        private void UpdateVisionDirection()
        {
            if (visionLight == null) return;
            
            // Update light to match character vision
            visionLight.pointLightOuterRadius = characterVision.VisionRange; 
            visionLight.pointLightInnerRadius = characterVision.VisionRange * 0.3f;
            visionLight.pointLightOuterAngle = characterVision.VisionAngle;
            visionLight.pointLightInnerAngle = characterVision.VisionAngle * 0.8f;
            
            // Update light rotation to match character facing direction
            Vector2 facingDirection = characterVision.GetFacingDirection();
            float angle = Mathf.Atan2(facingDirection.y, facingDirection.x) * Mathf.Rad2Deg;
            visionLight.transform.rotation = Quaternion.AngleAxis(angle-90, transform.forward);
        }

        private void UpdateObjectVisibility()
        {
            RefreshTrackedObjects();

            foreach (var kvp in trackedRenderers)
            {
                var renderer = kvp.Key;
                var state = kvp.Value;

                if (renderer == null) continue;

                bool shouldBeVisible = characterVision.CanSee(renderer.transform.position);

                float targetAlpha = shouldBeVisible ? 1f : dimmedAlpha;
                state.currentAlpha = Mathf.Lerp(state.currentAlpha, targetAlpha, Time.deltaTime * transitionSpeed);

                ApplyAlphaToRenderer(renderer, state);
            }
        }

        private void RefreshTrackedObjects()
        {
            // Find all renderers in extended range that should be affected by dimming
            Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(
                transform.position, 
                characterVision.VisionRange * 1.5f,
                renderingLayerMask
            );

            // Add new renderers to tracking
            foreach (var collider1 in nearbyColliders)
            {
                var colliderRenderer = collider1.GetComponent<Renderer>();
                if (colliderRenderer == null || trackedRenderers.ContainsKey(colliderRenderer)) continue;
                var material = colliderRenderer.material;
                var state = new VisibilityState
                {
                    originalMaterial = material,
                    originalColor = material.color,
                    dimmingMaterial = CreateDimmingMaterial(material)
                };
                trackedRenderers[colliderRenderer] = state;
            }

            // Remove distant or destroyed renderers
            var toRemove = new List<Renderer>();
            foreach (var kvp in trackedRenderers)
            {
                if (kvp.Key != null && !(Vector2.Distance(transform.position, kvp.Key.transform.position) > 
                                         characterVision.VisionRange * 2f)) continue;
                // Restore original material before removing
                if (kvp.Key != null) { kvp.Key.material = kvp.Value.originalMaterial; }
                    
                // Cleanup dimming material
                if (kvp.Value.dimmingMaterial != null) { DestroyImmediate(kvp.Value.dimmingMaterial); }
                
                toRemove.Add(kvp.Key);
            }
            foreach (var key in toRemove) { trackedRenderers.Remove(key); }
        }

        private bool DoesNeedTransparency(Material material)
        {
            // Check if material already supports transparency
            return material.HasProperty("_Mode") || 
                   material.renderQueue >= 3000 ||
                   material.shader.name.Contains("Transparent") ||
                   material.shader.name.Contains("Alpha");
        }

        private Material CreateDimmingMaterial(Material originalMaterial)
        {
            // Create a copy of the original material for dimming effects
            Material dimmingMat = new Material(originalMaterial) { name = originalMaterial.name + "_Dimmed" };

            // Ensure the material can handle transparency
            if (DoesNeedTransparency(originalMaterial)) return dimmingMat;
            
            // Convert material to support transparency
            if (dimmingMat.HasProperty("_Mode")) { dimmingMat.SetFloat("_Mode", 2); }
                
            dimmingMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            dimmingMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            dimmingMat.SetInt("_ZWrite", 0);
            dimmingMat.DisableKeyword("_ALPHATEST_ON");
            dimmingMat.EnableKeyword("_ALPHABLEND_ON");
            dimmingMat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            dimmingMat.renderQueue = 3000;

            return dimmingMat;
        }

        private void ApplyAlphaToRenderer(Renderer rndr, VisibilityState state)
        {
            //TODO: also apply shadow with appropriate shadow length
            if (state.currentAlpha >= 0.99f)
            {
                // Fully visible - use original material
                if (rndr.material != state.originalMaterial)
                {
                    rndr.material = state.originalMaterial;
                }
            }
            else
            {
                // Dimmed - use dimming material with modified alpha
                if (rndr.material != state.dimmingMaterial)
                {
                    rndr.material = state.dimmingMaterial;
                }
                
                // Apply the alpha
                Color currentColor = state.originalColor;
                currentColor.a = currentColor.a * state.currentAlpha; // Preserve original alpha
                state.dimmingMaterial.color = currentColor;
            }
        }

        private void OnDestroy()
        {
            // Cleanup: restore all materials and destroy dimming materials
            foreach (var kvp in trackedRenderers)
            {
                if (kvp.Key != null) { kvp.Key.material = kvp.Value.originalMaterial; }
                if (kvp.Value.dimmingMaterial != null) { DestroyImmediate(kvp.Value.dimmingMaterial); }
            }
            trackedRenderers.Clear();
        }

        private void OnValidate()
        {
            // Update light cone when values change in editor
            if (visionLight == null || characterVision == null) return;
            visionLight.pointLightOuterRadius = characterVision.VisionRange;
            visionLight.pointLightInnerRadius = characterVision.VisionRange * 0.3f;
            visionLight.pointLightOuterAngle = characterVision.VisionAngle;
            visionLight.pointLightInnerAngle = characterVision.VisionAngle * 0.8f;
            visionLight.color = lightConeColor;
            visionLight.intensity = lightIntensity;
        }
    }
}
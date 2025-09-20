using System.Collections.Generic;
using UnityEngine;
using Vision;

namespace Vision
{
    [RequireComponent(typeof(CharacterVision3D))]
    public class SimpleVisionRenderer3D : MonoBehaviour
    {
        [Header("Ground Cone Settings")]
        [SerializeField] private bool enableGroundCone = true;
        [SerializeField] private Color coneColor = new Color(1f, 1f, 0.8f, 0.3f);
        [SerializeField] private Material coneMaterial; // Assign a transparent material
        
        [Header("Cone Positioning")]
        [SerializeField] private float groundOffset = 0.1f; // Slightly above ground to avoid z-fighting
        [SerializeField] private bool smoothRotation = true;
        [SerializeField] private float rotationSpeed = 5f;
        
        // Components
        private CharacterVision3D characterVision;
        private GameObject coneObject;
        private MeshRenderer coneMeshRenderer;
        private Vector3 lastMovementDirection;

        private void Awake()
        {
            characterVision = GetComponent<CharacterVision3D>();
            if (enableGroundCone)
            {
                SetupGroundCone();
            }
        }

        private void Update()
        {
            if (enableGroundCone && coneObject != null)
            {
                UpdateConeDirection();
            }
        }

        private void SetupGroundCone()
        {
            // Create a child GameObject for the ground cone
            coneObject = new GameObject("VisionGroundCone");
            coneObject.transform.SetParent(transform);
            coneObject.transform.localPosition = Vector3.up * groundOffset;
            
            // Create cone mesh
            MeshFilter meshFilter = coneObject.AddComponent<MeshFilter>();
            coneMeshRenderer = coneObject.AddComponent<MeshRenderer>();
            
            // Generate cone mesh
            Mesh coneMesh = CreateConeMesh();
            meshFilter.mesh = coneMesh;
            
            // Debug mesh info
            Debug.Log($"Cone mesh created: {coneMesh.vertices.Length} vertices, {coneMesh.triangles.Length} triangles");
            Debug.Log($"Mesh bounds: {coneMesh.bounds}");
            
            // Setup material
            if (coneMaterial != null)
            {
                coneMeshRenderer.material = coneMaterial;
                Debug.Log($"Using assigned material: {coneMaterial.name}");
            }
            else
            {
                // Create default transparent material
                Material defaultMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                defaultMat.SetFloat("_Surface", 1); // Transparent
                defaultMat.SetFloat("_Blend", 0); // Alpha
                defaultMat.color = coneColor;
                coneMeshRenderer.material = defaultMat;
                Debug.Log("Created default material");
            }
            
            // Force enable everything
            coneObject.SetActive(true);
            coneMeshRenderer.enabled = true;
            
            // Set initial rotation
            coneObject.transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
            
            Debug.Log($"Ground cone created with range={characterVision.VisionRange}, angle={characterVision.VisionAngle}");
            Debug.Log($"Cone object active: {coneObject.activeInHierarchy}, renderer enabled: {coneMeshRenderer.enabled}");
            Debug.Log($"Cone position: {coneObject.transform.position}, scale: {coneObject.transform.localScale}");
        }

        private Mesh CreateConeMesh()
        {
            Mesh mesh = new Mesh();
            mesh.name = "VisionCone";
            
            float range = characterVision.VisionRange;
            float angle = characterVision.VisionAngle;
            int segments = 20; // Number of segments for smooth cone
            
            Debug.Log($"Creating mesh with range={range}, angle={angle}, segments={segments}");
            
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            List<Vector2> uvs = new List<Vector2>();
            
            // Center vertex at origin
            vertices.Add(Vector3.zero);
            uvs.Add(new Vector2(0.5f, 0));
            
            // Create cone vertices in a fan pattern
            float halfAngle = angle * 0.5f * Mathf.Deg2Rad;
            for (int i = 0; i <= segments; i++)
            {
                float currentAngle = Mathf.Lerp(-halfAngle, halfAngle, (float)i / segments);
                Vector3 direction = new Vector3(Mathf.Sin(currentAngle), 0, Mathf.Cos(currentAngle));
                Vector3 vertex = direction * range;
                
                vertices.Add(vertex);
                uvs.Add(new Vector2((float)i / segments, 1));
            }
            
            // Create triangles (fan from center)
            for (int i = 0; i < segments; i++)
            {
                // Triangle: center, current vertex, next vertex
                triangles.Add(0); // Center
                triangles.Add(i + 1); // Current vertex  
                triangles.Add(i + 2); // Next vertex
            }
            
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.uv = uvs.ToArray();
            
            // Important: Calculate bounds and normals
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            
            Debug.Log($"Mesh created: {vertices.Count} vertices, {triangles.Count/3} triangles");
            Debug.Log($"First few vertices: {vertices[0]}, {vertices[1]}, {vertices[2]}");
            
            return mesh;
        }

        private void UpdateConeDirection()
        {
            Vector3 movementDirection = characterVision.GetFacingDirection3D();
            
            // Only update if we have a valid direction
            if (movementDirection.magnitude < 0.1f)
            {
                return; // Keep current rotation
            }
            
            // Store this direction for when player stops moving
            lastMovementDirection = movementDirection;
            
            // Calculate target rotation - cone points in movement direction
            Quaternion targetRotation = Quaternion.LookRotation(movementDirection, Vector3.up);
            
            // Apply rotation (smooth or instant)
            if (smoothRotation)
            {
                coneObject.transform.rotation = Quaternion.Slerp(
                    coneObject.transform.rotation, 
                    targetRotation, 
                    Time.deltaTime * rotationSpeed
                );
            }
            else
            {
                coneObject.transform.rotation = targetRotation;
            }
            
            // Update cone size if vision parameters changed
            if (ShouldRegenerateMesh())
            {
                MeshFilter meshFilter = coneObject.GetComponent<MeshFilter>();
                if (meshFilter != null)
                {
                    meshFilter.mesh = CreateConeMesh();
                }
            }
        }

        private bool ShouldRegenerateMesh()
        {
            // Check if vision parameters have changed significantly
            MeshFilter meshFilter = coneObject.GetComponent<MeshFilter>();
            if (meshFilter == null || meshFilter.mesh == null) return true;
            
            // Simple check - you could store last values and compare
            return false; // For now, don't regenerate during runtime
        }

        // Public methods for runtime control
        public void SetConeEnabled(bool enabled)
        {
            if (coneObject != null)
            {
                coneObject.SetActive(enabled);
            }
        }
        
        public void SetConeColor(Color color)
        {
            coneColor = color;
            if (coneMeshRenderer != null && coneMeshRenderer.material != null)
            {
                coneMeshRenderer.material.color = color;
            }
        }

        private void OnValidate()
        {
            // Update settings when changed in inspector
            if (coneMeshRenderer != null && coneMeshRenderer.material != null)
            {
                coneMeshRenderer.material.color = coneColor;
            }
            
            if (coneObject != null)
            {
                coneObject.transform.localPosition = Vector3.up * groundOffset;
            }
        }

        private void OnDestroy()
        {
            // Cleanup
            if (coneObject != null)
            {
                DestroyImmediate(coneObject);
            }
        }
    }
}
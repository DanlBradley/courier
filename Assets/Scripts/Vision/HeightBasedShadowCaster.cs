using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using GameServices;
using UnityEngine.Rendering.Universal;

namespace Vision
{
    [RequireComponent(typeof(ShadowCaster2D))]
    public class HeightBasedShadowCaster : MonoBehaviour
    {
        [Header("Height Settings")]
        [SerializeField] private float objectHeight = 1.0f;
        [SerializeField] private float playerEyeHeight = 1.5f;
        
        [Header("Shadow Settings")]
        [SerializeField] private float maxShadowDistance = 50f;
        [SerializeField] private float updateFrequency = 0.1f;
        [SerializeField] private float minShadowThreshold = 2f;
        
        private ShadowCaster2D shadowCaster;
        private Transform playerTransform;
        private float lastUpdateTime;
        
        private void Start()
        {
            shadowCaster = GetComponent<ShadowCaster2D>();
            var gameManager = GameManager.Instance;
            if (gameManager != null) playerTransform = gameManager.GetPlayer()?.transform;
            lastUpdateTime = Time.time;
        }
        
        private void Update()
        {
            if (playerTransform == null) return;
            if (Time.time - lastUpdateTime < updateFrequency) return;
            lastUpdateTime = Time.time;
            UpdateShadowLength();
        }
        
        private void UpdateShadowLength()
        {
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
            float shadowLength = CalculateShadowLength(distanceToPlayer);
            shadowCaster.enabled = shadowLength > minShadowThreshold;
        }
        
        private float CalculateShadowLength(float distanceToPlayer)
        {
            if (objectHeight >= playerEyeHeight) { return maxShadowDistance; }
            if (objectHeight <= 0.01f) { return 0f; }
            float shadowLength = (objectHeight * distanceToPlayer) / playerEyeHeight;
            return Mathf.Clamp(shadowLength, 0f, maxShadowDistance);
        }
        private void OnValidate()
        {
            objectHeight = Mathf.Max(0f, objectHeight);
            playerEyeHeight = Mathf.Max(0.1f, playerEyeHeight);
            maxShadowDistance = Mathf.Max(1f, maxShadowDistance);
            updateFrequency = Mathf.Max(0.01f, updateFrequency);
        }
    }
}
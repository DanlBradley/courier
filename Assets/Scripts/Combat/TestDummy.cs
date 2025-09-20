using UnityEngine;
using Combat;
using Interfaces;

namespace Combat
{
    /// <summary>
    /// Simple test dummy for combat system testing.
    /// Logs all damage received and provides visual/audio feedback.
    /// </summary>
    public class TestDummy : MonoBehaviour, IDamageable
    {
        [Header("Dummy Settings")]
        [SerializeField] private Team dummyTeam = Team.Enemy;
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float defense = 5f;
        [SerializeField] private bool isInvulnerable = false;
        
        [Header("Visual Feedback")]
        [SerializeField] private Renderer meshRenderer;
        [SerializeField] private Color hitColor = Color.red;
        [SerializeField] private float hitFlashDuration = 0.2f;
        
        [Header("Debug")]
        [SerializeField] private bool verboseLogging = true;
        
        private float currentHealth;
        private Color originalColor;
        private float hitFlashTimer;
        
        // IDamageable Implementation
        public Team Team => dummyTeam;
        public bool IsAlive => currentHealth > 0;
        public bool IsBlocking => false; // Dummies don't block
        public float MaxHealth => maxHealth;
        public float CurrentHealth => currentHealth;
        
        public event System.Action<DamageInfo> OnDamageTaken;
        public event System.Action<float> OnHealed;
        public event System.Action OnDeath;
        
        private void Awake()
        {
            currentHealth = maxHealth;
            
            if (meshRenderer == null)
                meshRenderer = GetComponentInChildren<Renderer>();
                
            if (meshRenderer != null)
                originalColor = meshRenderer.material.color;
        }
        
        private void Update()
        {
            // Handle hit flash effect
            if (hitFlashTimer > 0)
            {
                hitFlashTimer -= Time.deltaTime;
                if (hitFlashTimer <= 0 && meshRenderer != null)
                {
                    meshRenderer.material.color = originalColor;
                }
            }
        }
        
        public void TakeDamage(DamageInfo damageInfo)
        {
            if (isInvulnerable)
            {
                Debug.Log($"[TestDummy] {name} is invulnerable - no damage taken");
                return;
            }
            
            // Log detailed damage info
            if (verboseLogging)
            {
                Debug.Log($"[TestDummy] === DAMAGE RECEIVED ===");
                Debug.Log($"[TestDummy] Target: {name}");
                Debug.Log($"[TestDummy] Source: {damageInfo.source?.name ?? "Unknown"}");
                Debug.Log($"[TestDummy] Raw Damage: {damageInfo.damage:F2}");
                Debug.Log($"[TestDummy] Damage Type: {damageInfo.damageType}");
                Debug.Log($"[TestDummy] Hit Point: {damageInfo.hitPoint}");
                Debug.Log($"[TestDummy] Impact Force: {damageInfo.impactForce:F2}");
                Debug.Log($"[TestDummy] Knockback: {damageInfo.knockbackForce:F2}");
                Debug.Log($"[TestDummy] Was Blocked: {damageInfo.wasBlocked}");
                Debug.Log($"[TestDummy] Was Deflected: {damageInfo.wasDeflected}");
            }
            
            // Apply defense
            float finalDamage = Mathf.Max(0, damageInfo.damage - defense);
            currentHealth = Mathf.Max(0, currentHealth - finalDamage);
            
            Debug.Log($"[TestDummy] {name} took {finalDamage:F2} damage (after {defense} defense). Health: {currentHealth:F2}/{maxHealth:F2}");
            
            // Visual feedback
            ShowHitEffect();
            
            // Raise events
            OnDamageTaken?.Invoke(damageInfo);
            
            if (currentHealth <= 0 && verboseLogging)
            {
                Debug.Log($"[TestDummy] {name} has been destroyed!");
                OnDeath?.Invoke();
            }
        }
        
        public void Heal(float amount)
        {
            float healAmount = Mathf.Min(amount, maxHealth - currentHealth);
            currentHealth += healAmount;
            
            if (verboseLogging)
                Debug.Log($"[TestDummy] {name} healed for {healAmount:F2}. Health: {currentHealth:F2}/{maxHealth:F2}");
                
            OnHealed?.Invoke(healAmount);
        }
        
        public Vector3 GetBlockDirection()
        {
            return transform.forward; // Not used since dummy doesn't block
        }
        
        public float GetDefense()
        {
            return defense;
        }
        
        private void ShowHitEffect()
        {
            if (meshRenderer != null)
            {
                meshRenderer.material.color = hitColor;
                hitFlashTimer = hitFlashDuration;
            }
        }
        
        // Debug helper
        public void ResetHealth()
        {
            currentHealth = maxHealth;
            Debug.Log($"[TestDummy] {name} health reset to {maxHealth}");
        }
        
        private void OnTriggerEnter(Collider other)
        {
            // Log any trigger enters for debugging
            if (verboseLogging)
            {
                Debug.Log($"[TestDummy] {name} detected trigger enter from: {other.name} (Layer: {LayerMask.LayerToName(other.gameObject.layer)})");
            }
        }
        
        private void OnCollisionEnter(Collision collision)
        {
            // Log any collisions for debugging
            if (verboseLogging)
            {
                Debug.Log($"[TestDummy] {name} detected collision from: {collision.gameObject.name} (Layer: {LayerMask.LayerToName(collision.gameObject.layer)})");
            }
        }
    }
}
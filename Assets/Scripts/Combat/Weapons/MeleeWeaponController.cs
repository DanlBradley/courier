using UnityEngine;

namespace Combat.Weapons
{
    /// <summary>
    /// Concrete implementation of WeaponController for melee weapons (swords, axes, maces, etc.)
    /// Most functionality is already in WeaponController base class.
    /// This adds melee-specific features like trail effects and environmental hit sounds.
    /// </summary>
    public class MeleeWeaponController : WeaponController
    {
        [Header("Melee Specific")]
        [SerializeField] private TrailRenderer weaponTrail;
        [SerializeField] private bool debugHits = false;
        
        [Header("Environmental Hit Effects")]
        [SerializeField] private AudioClip[] metalHitSounds;
        [SerializeField] private AudioClip[] stoneHitSounds;
        [SerializeField] private AudioClip[] woodHitSounds;
        [SerializeField] private GameObject sparkEffectPrefab;
        
        public override void ActivateHitbox()
        {
            base.ActivateHitbox();
            
            // Enable trail effect if present
            if (weaponTrail != null)
            {
                weaponTrail.enabled = true;
                weaponTrail.Clear();
            }
            
            if (debugHits)
            {
                Debug.Log($"[MeleeWeapon] Hitbox activated for {gameObject.name}, isAttacking: {isAttacking}");
                if (hitboxes != null)
                {
                    foreach (var hitbox in hitboxes)
                    {
                        if (hitbox != null)
                            Debug.Log($"[MeleeWeapon] - Hitbox {hitbox.name} enabled: {hitbox.enabled}");
                    }
                }
                else
                {
                    Debug.LogWarning($"[MeleeWeapon] No hitboxes assigned!");
                }
            }
        }
        
        public override void DeactivateHitbox()
        {
            base.DeactivateHitbox();
            
            // Disable trail effect if present
            if (weaponTrail != null)
                weaponTrail.enabled = false;
            
            if (debugHits)
                Debug.Log($"[MeleeWeapon] Hitbox deactivated for {gameObject.name}");
        }
        
        protected override void OnTriggerEnter(Collider other)
        {
            if (debugHits)
            {
                Debug.Log($"[MeleeWeapon] OnTriggerEnter: {other.name}, isAttacking: {isAttacking}, velocity: {swingVelocity:F2}");
            }
            base.OnTriggerEnter(other);
        }
        
        protected override void ProcessHit(Collider other, Collision collision = null)
        {
            if (debugHits)
            {
                Debug.Log($"[MeleeWeapon] Processing hit on {other.name}, Velocity: {swingVelocity:F2}");
            }
            
            // Call base implementation which handles all the damage logic
            base.ProcessHit(other, collision);
        }
        
        protected override void PlayBlockEffect(Vector3 position, Vector3 normal)
        {
            // Sparks when weapon hits another weapon/shield
            if (sparkEffectPrefab != null)
            {
                var sparks = Instantiate(sparkEffectPrefab, position, Quaternion.LookRotation(normal));
                Destroy(sparks, 1f);
            }
            
            // Metal clang sound
            if (audioSource != null && metalHitSounds != null && metalHitSounds.Length > 0)
            {
                var clip = metalHitSounds[Random.Range(0, metalHitSounds.Length)];
                audioSource.PlayOneShot(clip, 0.8f);
            }
        }
        
        protected override void PlayDeflectEffect(Vector3 position, Vector3 normal)
        {
            // Similar to block but quieter
            if (sparkEffectPrefab != null)
            {
                var sparks = Instantiate(sparkEffectPrefab, position, Quaternion.LookRotation(normal));
                sparks.transform.localScale = Vector3.one * 0.5f; // Smaller sparks for deflection
                Destroy(sparks, 0.5f);
            }
            
            if (audioSource != null && metalHitSounds != null && metalHitSounds.Length > 0)
            {
                var clip = metalHitSounds[Random.Range(0, metalHitSounds.Length)];
                audioSource.PlayOneShot(clip, 0.4f); // Quieter for deflection
            }
        }
        
        protected override void PlayEnvironmentHitEffect(Collider surface)
        {
            // Determine surface type by tag or layer
            string surfaceTag = surface.tag.ToLower();
            AudioClip[] soundsToPlay = null;
            
            if (surfaceTag.Contains("metal") || surface.gameObject.layer == LayerMask.NameToLayer("Metal"))
            {
                soundsToPlay = metalHitSounds;
                
                // Create sparks for metal hits
                if (sparkEffectPrefab != null)
                {
                    var hitPoint = surface.ClosestPoint(transform.position);
                    var sparks = Instantiate(sparkEffectPrefab, hitPoint, Quaternion.identity);
                    Destroy(sparks, 0.5f);
                }
            }
            else if (surfaceTag.Contains("stone") || surface.gameObject.layer == LayerMask.NameToLayer("Stone"))
            {
                soundsToPlay = stoneHitSounds;
            }
            else if (surfaceTag.Contains("wood") || surface.gameObject.layer == LayerMask.NameToLayer("Wood"))
            {
                soundsToPlay = woodHitSounds;
            }
            
            // Play appropriate sound
            if (audioSource != null && soundsToPlay != null && soundsToPlay.Length > 0)
            {
                var clip = soundsToPlay[Random.Range(0, soundsToPlay.Length)];
                audioSource.PlayOneShot(clip, 0.6f);
            }
            
            if (debugHits)
                Debug.Log($"[MeleeWeapon] Hit environment: {surface.name} (Tag: {surface.tag}, Layer: {LayerMask.LayerToName(surface.gameObject.layer)})");
        }
        
        // Optional: Override velocity damage calculation for specific melee behavior
        protected override float CalculateVelocityDamageMultiplier(float velocity, float weaponWeight)
        {
            // Melee weapons benefit more from velocity
            float normalizedVelocity = Mathf.Clamp01(velocity / 8f); // Slightly lower threshold than base
            float weightFactor = Mathf.Lerp(0.7f, 1.6f, weaponWeight / 5f);
            
            float multiplier = Mathf.Lerp(0.4f, 1.8f, normalizedVelocity) * weightFactor;
            
            if (debugHits)
                Debug.Log($"[MeleeWeapon] Velocity multiplier: {multiplier:F2} (Vel: {velocity:F2}, Weight: {weaponWeight:F2})");
                
            return multiplier;
        }
    }
}
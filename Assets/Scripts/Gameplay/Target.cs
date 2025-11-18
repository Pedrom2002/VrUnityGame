using System;
using System.Collections;
using UnityEngine;
using VRAimLab.Core;

namespace VRAimLab.Gameplay
{
    /// <summary>
    /// Target types with different behaviors and point values
    /// </summary>
    public enum TargetType
    {
        Basic,      // Standard target
        Speed,      // Quick spawn/despawn
        Precision,  // Small, high points
        Moving,     // Moving target
        Combo       // Requires multiple hits
    }

    /// <summary>
    /// Hit zones for precision scoring
    /// </summary>
    public enum HitZone
    {
        Outer,      // Base points
        Inner,      // 1.5x points
        Bullseye    // 2x points + perfect hit bonus
    }

    /// <summary>
    /// Base Target class - handles hit detection, scoring, lifetime, and effects
    /// Can be extended for specialized target behaviors
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class Target : MonoBehaviour
    {
        #region Events
        public static event Action<Target> OnTargetHit;
        public static event Action<Target> OnTargetExpired;
        #endregion

        #region Inspector Fields
        [Header("Target Settings")]
        [SerializeField] protected TargetType targetType = TargetType.Basic;
        [SerializeField] protected int basePoints = 100;
        [SerializeField] protected bool hasLifetime = true; // Se false, alvo nunca expira
        [SerializeField] protected float lifetime = 5f; // Time before auto-despawn
        [SerializeField] protected int hitsRequired = 1; // For combo targets

        [Header("Hit Zones (Optional)")]
        [SerializeField] protected bool useHitZones = false;
        [SerializeField] protected Transform bullseyeZone; // Smallest zone
        [SerializeField] protected Transform innerZone;    // Middle zone
        [SerializeField] protected float bullseyeRadius = 0.1f;
        [SerializeField] protected float innerRadius = 0.2f;

        [Header("Visual Feedback")]
        [SerializeField] protected GameObject hitEffectPrefab;
        [SerializeField] protected GameObject destroyEffectPrefab;
        [SerializeField] protected Color normalColor = Color.red;
        [SerializeField] protected Color hitColor = Color.yellow;

        [Header("Audio")]
        [SerializeField] protected bool playHitSound = true;
        [SerializeField] protected bool playSpawnSound = false;
        #endregion

        #region Protected Fields
        protected Renderer targetRenderer;
        protected Collider targetCollider;
        protected float currentLifetime;
        protected int currentHits = 0;
        protected bool isActive = true;
        protected MaterialPropertyBlock propBlock;
        #endregion

        #region Properties
        public TargetType Type => targetType;
        public int BasePoints => basePoints;
        public bool IsActive => isActive;
        public float RemainingLifetime => currentLifetime;
        #endregion

        #region Unity Lifecycle
        protected virtual void Awake()
        {
            targetRenderer = GetComponent<Renderer>();
            targetCollider = GetComponent<Collider>();
            propBlock = new MaterialPropertyBlock();

            // Preserve original material color if normalColor is still default red
            if (targetRenderer != null && targetRenderer.sharedMaterial != null)
            {
                // Check if normalColor is still the default red (not customized in Inspector)
                if (normalColor == Color.red)
                {
                    // Use the material's original color
                    normalColor = targetRenderer.sharedMaterial.color;
                }

                SetTargetColor(normalColor);
            }
        }

        protected virtual void OnEnable()
        {
            ResetTarget();

            if (playSpawnSound && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayTargetSpawnSound(transform.position);
            }
        }

        protected virtual void Update()
        {
            if (!isActive) return;

            UpdateLifetime();
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Reset target to initial state (for object pooling)
        /// </summary>
        public virtual void ResetTarget()
        {
            currentLifetime = lifetime;
            currentHits = 0;
            isActive = true;

            if (targetCollider != null)
                targetCollider.enabled = true;

            if (targetRenderer != null)
                SetTargetColor(normalColor);
        }

        /// <summary>
        /// Initialize target with custom settings
        /// </summary>
        public virtual void Initialize(TargetType type, int points, float life)
        {
            targetType = type;
            basePoints = points;
            lifetime = life;
            ResetTarget();
        }
        #endregion

        #region Lifetime Management
        /// <summary>
        /// Update target lifetime and handle expiration
        /// </summary>
        protected virtual void UpdateLifetime()
        {
            // Se hasLifetime está false, alvo nunca expira
            if (!hasLifetime) return;

            currentLifetime -= Time.deltaTime;

            if (currentLifetime <= 0f)
            {
                OnLifetimeExpired();
            }
        }

        /// <summary>
        /// Called when target expires without being hit
        /// </summary>
        protected virtual void OnLifetimeExpired()
        {
            isActive = false;
            OnTargetExpired?.Invoke(this);

            // Notify game manager to break streak
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnTargetExpired();
            }

            DestroyTarget(false);
        }
        #endregion

        #region Hit Detection
        /// <summary>
        /// Called when bullet hits this target
        /// </summary>
        /// <param name="hitPoint">World position of hit</param>
        /// <param name="hitNormal">Normal of hit surface</param>
        public virtual void OnHit(Vector3 hitPoint, Vector3 hitNormal)
        {
            if (!isActive) return;

            currentHits++;

            // Determine hit zone and calculate points
            HitZone zone = DetermineHitZone(hitPoint);
            int points = CalculatePoints(zone);
            bool isPerfect = (zone == HitZone.Bullseye);

            // Visual feedback
            SpawnHitEffect(hitPoint, hitNormal);
            StartCoroutine(FlashHitColor());

            // Audio feedback
            if (playHitSound && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayTargetHitSound(transform.position, isPerfect);
            }

            // Notify game manager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnTargetHit(points, isPerfect);
            }

            // Emit event
            OnTargetHit?.Invoke(this);

            // Check if target should be destroyed
            if (currentHits >= hitsRequired)
            {
                DestroyTarget(true);
            }

            Debug.Log($"[Target] Hit! Zone: {zone}, Points: {points}");
        }

        /// <summary>
        /// Determine which hit zone was hit
        /// </summary>
        protected virtual HitZone DetermineHitZone(Vector3 hitPoint)
        {
            if (!useHitZones) return HitZone.Outer;

            float distanceFromCenter = Vector3.Distance(hitPoint, transform.position);

            if (distanceFromCenter <= bullseyeRadius)
                return HitZone.Bullseye;
            else if (distanceFromCenter <= innerRadius)
                return HitZone.Inner;
            else
                return HitZone.Outer;
        }

        /// <summary>
        /// Calculate points based on hit zone
        /// </summary>
        protected virtual int CalculatePoints(HitZone zone)
        {
            switch (zone)
            {
                case HitZone.Bullseye:
                    return Mathf.RoundToInt(basePoints * 2f);
                case HitZone.Inner:
                    return Mathf.RoundToInt(basePoints * 1.5f);
                case HitZone.Outer:
                default:
                    return basePoints;
            }
        }
        #endregion

        #region Visual Effects
        /// <summary>
        /// Spawn hit effect particle system
        /// </summary>
        protected virtual void SpawnHitEffect(Vector3 position, Vector3 normal)
        {
            if (hitEffectPrefab != null)
            {
                GameObject effect = Instantiate(hitEffectPrefab, position, Quaternion.LookRotation(normal));
                Destroy(effect, 2f);
            }
        }

        /// <summary>
        /// Flash target color on hit
        /// </summary>
        protected virtual IEnumerator FlashHitColor()
        {
            SetTargetColor(hitColor);
            yield return new WaitForSeconds(0.1f);
            SetTargetColor(normalColor);
        }

        /// <summary>
        /// Set target color using MaterialPropertyBlock (efficient)
        /// </summary>
        protected virtual void SetTargetColor(Color color)
        {
            if (targetRenderer == null) return;

            targetRenderer.GetPropertyBlock(propBlock);
            propBlock.SetColor("_Color", color);
            propBlock.SetColor("_BaseColor", color); // For URP
            targetRenderer.SetPropertyBlock(propBlock);
        }

        /// <summary>
        /// Spawn destroy effect and return to pool or destroy
        /// </summary>
        protected virtual void DestroyTarget(bool wasHit)
        {
            isActive = false;

            if (targetCollider != null)
                targetCollider.enabled = false;

            // Spawn destroy effect
            if (destroyEffectPrefab != null && wasHit)
            {
                GameObject effect = Instantiate(destroyEffectPrefab, transform.position, Quaternion.identity);
                Destroy(effect, 2f);
            }

            // Return to pool or destroy
            StartCoroutine(DeactivateAfterDelay(0.1f));
        }

        protected virtual IEnumerator DeactivateAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            gameObject.SetActive(false);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Force destroy target (e.g., when game ends)
        /// </summary>
        public virtual void ForceDestroy()
        {
            isActive = false;
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Add bonus time to target lifetime
        /// </summary>
        public virtual void AddLifetime(float seconds)
        {
            currentLifetime += seconds;
        }

        /// <summary>
        /// Get target info string for debugging
        /// </summary>
        public virtual string GetTargetInfo()
        {
            return $"Type: {targetType}, Points: {basePoints}, Lifetime: {currentLifetime:F1}s";
        }
        #endregion

        #region Editor Helpers
#if UNITY_EDITOR
        protected virtual void OnDrawGizmosSelected()
        {
            if (!useHitZones) return;

            // Draw hit zones in editor
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, bullseyeRadius);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, innerRadius);

            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(transform.position, innerRadius * 1.5f);
        }
#endif
        #endregion
    }
}

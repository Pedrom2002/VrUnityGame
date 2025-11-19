using System;
using System.Collections;
using UnityEngine;
using VRAimLab.Core;

namespace VRAimLab.Gameplay
{
    /// Target types with different behaviors and point values
    public enum TargetType
    {
        Basic,      // Standard target
        Speed,      // Quick spawn/despawn
        Precision,  // Small, high points
        Moving,     // Moving target
        Combo       // Requires multiple hits
    }

    /// Hit zones for precision scoring
    public enum HitZone
    {
        Outer,      // Base points
        Inner,      // 1.5x points
        Bullseye    // 2x points + perfect hit bonus
    }

    /// Base Target class - handles hit detection, scoring, lifetime, and effects
    /// Can be extended for specialized target behaviors
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

            // Preservar a cor original do material se não foi customizada no Inspector
            if (targetRenderer != null && targetRenderer.sharedMaterial != null)
            {
                // Se a cor ainda é o vermelho default, usar a cor do material
                if (normalColor == Color.red)
                {
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
        /// Reset target to initial state (for object pooling)
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

        /// Initialize target with custom settings
        public virtual void Initialize(TargetType type, int points, float life)
        {
            targetType = type;
            basePoints = points;
            lifetime = life;
            ResetTarget();
        }
        #endregion

        #region Lifetime Management
        /// Update target lifetime and handle expiration
        protected virtual void UpdateLifetime()
        {
            // Modo training não tem lifetime, alvos ficam para sempre
            if (!hasLifetime) return;

            currentLifetime -= Time.deltaTime;

            // Alvo expirou sem ser acertado
            if (currentLifetime <= 0f)
            {
                OnLifetimeExpired();
            }
        }

        /// Called when target expires without being hit
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
        /// Called when bullet hits this target
        public virtual void OnHit(Vector3 hitPoint, Vector3 hitNormal)
        {
            if (!isActive) return;

            currentHits++;

            // Verificar em que zona acertámos (outer, inner, bullseye)
            HitZone zone = DetermineHitZone(hitPoint);
            int points = CalculatePoints(zone);
            bool isPerfect = (zone == HitZone.Bullseye);

            // Spawn do efeito visual de impacto
            SpawnHitEffect(hitPoint, hitNormal);
            StartCoroutine(FlashHitColor());

            // Tocar som de acerto (diferente se for bullseye)
            if (playHitSound && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayTargetHitSound(transform.position, isPerfect);
            }

            // Informar o GameManager para atualizar score
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnTargetHit(points, isPerfect);
            }

            // Emitir evento para outros sistemas
            OnTargetHit?.Invoke(this);

            // Destruir alvo se já levou hits suficientes (combo targets precisam de múltiplos)
            if (currentHits >= hitsRequired)
            {
                DestroyTarget(true);
            }

            Debug.Log($"[Target] Hit! Zone: {zone}, Points: {points}");
        }

        /// Determine which hit zone was hit
        protected virtual HitZone DetermineHitZone(Vector3 hitPoint)
        {
            // Se não usa zonas, retornar sempre outer (pontos base)
            if (!useHitZones) return HitZone.Outer;

            // Calcular distância do ponto de impacto ao centro do alvo
            float distanceFromCenter = Vector3.Distance(hitPoint, transform.position);

            // Verificar em que zona caiu o tiro
            if (distanceFromCenter <= bullseyeRadius)
                return HitZone.Bullseye;  // Centro (x2 pontos)
            else if (distanceFromCenter <= innerRadius)
                return HitZone.Inner;      // Anel interior (x1.5 pontos)
            else
                return HitZone.Outer;      // Anel exterior (pontos base)
        }

        /// Calculate points based on hit zone
        protected virtual int CalculatePoints(HitZone zone)
        {
            // Multiplicadores por zona de acerto
            switch (zone)
            {
                case HitZone.Bullseye:
                    return Mathf.RoundToInt(basePoints * 2f);   // x2 (ex: 200 pts)
                case HitZone.Inner:
                    return Mathf.RoundToInt(basePoints * 1.5f); // x1.5 (ex: 150 pts)
                case HitZone.Outer:
                default:
                    return basePoints;                           // x1 (ex: 100 pts)
            }
        }
        #endregion

        #region Visual Effects
        /// Spawn hit effect particle system
        protected virtual void SpawnHitEffect(Vector3 position, Vector3 normal)
        {
            if (hitEffectPrefab != null)
            {
                GameObject effect = Instantiate(hitEffectPrefab, position, Quaternion.LookRotation(normal));
                Destroy(effect, 2f);
            }
        }

        /// Flash target color on hit
        protected virtual IEnumerator FlashHitColor()
        {
            SetTargetColor(hitColor);
            yield return new WaitForSeconds(0.1f);
            SetTargetColor(normalColor);
        }

        /// Set target color using MaterialPropertyBlock (efficient)
        protected virtual void SetTargetColor(Color color)
        {
            if (targetRenderer == null) return;

            targetRenderer.GetPropertyBlock(propBlock);
            propBlock.SetColor("_Color", color);
            propBlock.SetColor("_BaseColor", color); // For URP
            targetRenderer.SetPropertyBlock(propBlock);
        }

        /// Spawn destroy effect and return to pool or destroy
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
        /// Force destroy target (e.g., when game ends)
        public virtual void ForceDestroy()
        {
            isActive = false;
            gameObject.SetActive(false);
        }

        /// Add bonus time to target lifetime
        public virtual void AddLifetime(float seconds)
        {
            currentLifetime += seconds;
        }

        /// Get target info string for debugging
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

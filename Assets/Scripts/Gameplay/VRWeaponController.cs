using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using VRAimLab.Core;

namespace VRAimLab.Gameplay
{
    /// VR Weapon Controller - handles shooting, reloading, haptics for VR
    /// Compatible with XR Interaction Toolkit
    /// Uses raycast for shooting with visual feedback
    [RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable))]
    public class VRWeaponController : MonoBehaviour
    {
        #region Inspector Fields
        [Header("Weapon Stats")]
        [SerializeField] private int maxAmmo = 30;
        [SerializeField] private float fireRate = 0.2f; // Time between shots
        [SerializeField] private float reloadTime = 2f;
        [SerializeField] private float range = 100f;
        #pragma warning disable 0414 // Field assigned but never used - reserved for future damage system
        [SerializeField] private int damage = 100;
        #pragma warning restore 0414

        [Header("Shooting")]
        [SerializeField] private Transform shootPoint; // Where raycast starts
        [SerializeField] private LayerMask targetLayer; // What can be hit
        [SerializeField] private bool autoAim = false; // Optional aim assist
        [SerializeField] private float autoAimAngle = 5f;

        [Header("Visual Feedback")]
        [SerializeField] private LineRenderer laserSight; // Legacy laser (kept for backwards compatibility)
        [SerializeField] private LaserSight enhancedLaserSight; // New enhanced laser sight
        [SerializeField] private GameObject muzzleFlashPrefab;
        [SerializeField] private GameObject bulletTracerPrefab;
        [SerializeField] private GameObject bulletImpactPrefab;
        [SerializeField] private float tracerSpeed = 100f;
        [SerializeField] private bool showLaserSight = true;

        [Header("Haptic Feedback")]
        [SerializeField] private float shootHapticIntensity = 0.5f;
        [SerializeField] private float shootHapticDuration = 0.1f;
        [SerializeField] private float reloadHapticIntensity = 0.3f;
        [SerializeField] private float reloadHapticDuration = 0.2f;

        [Header("Recoil")]
        [SerializeField] private bool enableRecoil = true;
        [SerializeField] private float recoilAmount = 0.05f;
        [SerializeField] private float recoilRecoverySpeed = 5f;
        #endregion

        #region Private Fields
        private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
        #pragma warning disable CS0618 // Type or member is obsolete - XRBaseController used for haptic feedback, will migrate to new system in future
        private XRBaseController controller;
        #pragma warning restore CS0618
        private int currentAmmo;
        private float nextFireTime;
        private bool isReloading = false;
        private bool isGrabbed = false;
        private bool wasGripPressed = false;
        private bool wasPrimaryPressed = false;

        private Vector3 originalPosition;
        private Quaternion originalRotation;
        private float recoilTimer;
        #endregion

        #region Properties
        public int CurrentAmmo => currentAmmo;
        public int MaxAmmo => maxAmmo;
        public bool IsReloading => isReloading;
        public bool CanShoot => !isReloading && currentAmmo > 0 && Time.time >= nextFireTime && isGrabbed;
        public float AmmoPercentage => (float)currentAmmo / maxAmmo;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();

            // Setup shoot point if not set
            if (shootPoint == null)
            {
                GameObject sp = new GameObject("ShootPoint");
                sp.transform.SetParent(transform);
                sp.transform.localPosition = Vector3.forward * 0.3f;
                shootPoint = sp.transform;
            }

            // Store original transform
            originalPosition = transform.localPosition;
            originalRotation = transform.localRotation;
        }

        private void OnEnable()
        {
            // Setup laser sight (done in OnEnable to ensure GameObject is active)
            if (laserSight == null)
            {
                // First check if a LineRenderer already exists
                laserSight = GetComponent<LineRenderer>();

                if (laserSight == null)
                {
                    laserSight = gameObject.AddComponent<LineRenderer>();
                }

                if (laserSight != null)
                {
                    laserSight.startWidth = 0.002f;
                    laserSight.endWidth = 0.002f;
                    laserSight.material = new Material(Shader.Find("Sprites/Default"));
                    laserSight.startColor = Color.red;
                    laserSight.endColor = Color.red;
                    laserSight.enabled = false;
                }
                else
                {
                    Debug.LogError($"[VRWeapon] Failed to create LineRenderer on {gameObject.name}. GameObject active: {gameObject.activeInHierarchy}");
                }
            }

            // Subscribe to XR events (remove first to prevent duplicates)
            if (grabInteractable != null)
            {
                grabInteractable.selectEntered.RemoveListener(OnGrab);
                grabInteractable.selectExited.RemoveListener(OnRelease);
                grabInteractable.activated.RemoveListener(OnTriggerPressed);

                grabInteractable.selectEntered.AddListener(OnGrab);
                grabInteractable.selectExited.AddListener(OnRelease);
                grabInteractable.activated.AddListener(OnTriggerPressed);
            }
        }

        private void OnDisable()
        {
            // Unsubscribe from XR events
            grabInteractable.selectEntered.RemoveListener(OnGrab);
            grabInteractable.selectExited.RemoveListener(OnRelease);
            grabInteractable.activated.RemoveListener(OnTriggerPressed);
        }

        private void Start()
        {
            currentAmmo = maxAmmo;
        }

        private void Update()
        {
            if (isGrabbed)
            {
                UpdateLaserSight();
                UpdateRecoilRecovery();
                CheckReloadInput();
            }
        }
        #endregion

        #region XR Interaction Events
        /// Called when weapon is grabbed
        private void OnGrab(SelectEnterEventArgs args)
        {
            isGrabbed = true;
            #pragma warning disable CS0618 // Type or member is obsolete
            controller = args.interactorObject.transform.GetComponent<XRBaseController>();
            #pragma warning restore CS0618

            if (showLaserSight)
            {
                // Use enhanced laser if available, otherwise fallback to legacy
                if (enhancedLaserSight != null)
                {
                    enhancedLaserSight.SetEnabled(true);
                }
                else if (laserSight != null)
                {
                    laserSight.enabled = true;
                }
            }

            Debug.Log("[VRWeapon] Weapon grabbed");
        }

        /// Called when weapon is released
        private void OnRelease(SelectExitEventArgs args)
        {
            isGrabbed = false;
            controller = null;

            // Disable both laser types
            if (enhancedLaserSight != null)
            {
                enhancedLaserSight.SetEnabled(false);
            }
            if (laserSight != null)
            {
                laserSight.enabled = false;
            }

            Debug.Log("[VRWeapon] Weapon released");
        }

        /// Called when trigger is pressed (XR Activated event)
        private void OnTriggerPressed(ActivateEventArgs args)
        {
            TryShoot();
        }
        #endregion

        #region Shooting
        /// Attempt to shoot the weapon
        public void TryShoot()
        {
            if (!CanShoot)
            {
                // Only show warning when out of ammo
                if (currentAmmo == 0 && !isReloading)
                {
                    Debug.LogWarning($"[VRWeapon] Cannot shoot - Out of ammo! (Ammo: {currentAmmo})");
                    PlayEmptySound();
                    SendHapticFeedback(0.2f, 0.05f);
                }
                // Silently ignore if in cooldown (fire rate) or other conditions
                return;
            }

            Shoot();
        }

        /// Shoot the weapon
        private void Shoot()
        {
            // Consume ammo
            currentAmmo--;
            nextFireTime = Time.time + fireRate;

            Debug.Log($"[VRWeapon] BANG! Ammo remaining: {currentAmmo}/{maxAmmo}");

            // Register shot for accuracy
            if (GameManager.Instance != null)
            {
                GameManager.Instance.Score.RegisterShot();
            }

            // Perform raycast
            Vector3 shootDirection = GetShootDirection();
            Vector3 shootOrigin = shootPoint.position;

            // Always use shootPoint position in VR
            // Only use camera position if explicitly in desktop mode (no XR active)

            RaycastHit hit;
            bool didHit;

            // Raycast with layer mask if configured, otherwise raycast everything
            if (targetLayer.value != 0)
            {
                didHit = Physics.Raycast(shootOrigin, shootDirection, out hit, range, targetLayer);
            }
            else
            {
                // No layer mask configured - raycast everything
                didHit = Physics.Raycast(shootOrigin, shootDirection, out hit, range);
                Debug.LogWarning("[VRWeapon] targetLayer not configured! Shooting at all layers. Configure targetLayer in Inspector for better performance.");
            }

            // Debug visualization
            Vector3 rayEndPoint = didHit ? hit.point : shootOrigin + shootDirection * range;
            Debug.DrawLine(shootOrigin, rayEndPoint, didHit ? Color.green : Color.red, 2f);

            if (didHit)
            {
                Debug.Log($"[VRWeapon] HIT! Object: {hit.collider.gameObject.name}, Layer: {LayerMask.LayerToName(hit.collider.gameObject.layer)}, Distance: {hit.distance:F2}m");
            }
            else
            {
                Debug.Log($"[VRWeapon] MISS! Direction: {shootDirection}, Origin: {shootOrigin}");
            }

            // Visual feedback
            SpawnMuzzleFlash();
            SpawnBulletTracer(shootOrigin, rayEndPoint);

            // Audio feedback
            PlayShootSound();

            // Haptic feedback
            SendHapticFeedback(shootHapticIntensity, shootHapticDuration);

            // Recoil
            if (enableRecoil)
            {
                ApplyRecoil();
            }

            // Check if we hit a target
            if (didHit)
            {
                OnHitTarget(hit);
            }
            else
            {
                // Missed shot
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.OnShotMissed();
                }
            }

            Debug.Log($"[VRWeapon] Shot fired! Ammo: {currentAmmo}/{maxAmmo}");
        }

        /// Get shoot direction (with optional auto-aim)
        private Vector3 GetShootDirection()
        {
            // Always use shootPoint direction (where barrel is pointing)
            Vector3 direction = shootPoint.forward;

            // VR MODE: Use auto-aim if enabled
            if (isGrabbed && autoAim)
            {
                // Simple auto-aim: check for targets in cone
                Collider[] nearbyTargets = Physics.OverlapSphere(shootPoint.position, range, targetLayer);

                float closestAngle = autoAimAngle;
                Transform closestTarget = null;

                foreach (Collider targetCollider in nearbyTargets)
                {
                    Vector3 directionToTarget = (targetCollider.transform.position - shootPoint.position).normalized;
                    float angle = Vector3.Angle(shootPoint.forward, directionToTarget);

                    if (angle < closestAngle)
                    {
                        closestAngle = angle;
                        closestTarget = targetCollider.transform;
                    }
                }

                if (closestTarget != null)
                {
                    direction = (closestTarget.position - shootPoint.position).normalized;
                }
            }

            return direction;
        }

        /// Handle hitting a target
        private void OnHitTarget(RaycastHit hit)
        {
            // Spawn impact effect
            SpawnImpactEffect(hit.point, hit.normal);

            // Check if we hit a Target component (procura no próprio objeto e nos pais)
            Target target = hit.collider.GetComponent<Target>();

            // Se não encontrou no collider, procura no parent
            if (target == null)
            {
                target = hit.collider.GetComponentInParent<Target>();
            }

            if (target != null)
            {
                Debug.Log($"[VRWeapon] ★ TARGET HIT! ★ Name: {hit.collider.gameObject.name}, Type: {target.Type}, Active: {target.IsActive}");
                target.OnHit(hit.point, hit.normal);
            }
            else
            {
                Debug.LogWarning($"[VRWeapon] Hit object '{hit.collider.gameObject.name}' has no Target component! Add Target script to make it destroyable.");
            }
        }
        #endregion

        #region Reloading
        /// Check for reload input (grip button or Y/B button)
        private void CheckReloadInput()
        {
            if (!isGrabbed) return;

            // Get all hand devices
            var handDevices = new System.Collections.Generic.List<UnityEngine.XR.InputDevice>();
            UnityEngine.XR.InputDevices.GetDevicesAtXRNode(UnityEngine.XR.XRNode.LeftHand, handDevices);
            UnityEngine.XR.InputDevices.GetDevicesAtXRNode(UnityEngine.XR.XRNode.RightHand, handDevices);

            foreach (var device in handDevices)
            {
                if (!device.isValid) continue;

                // Check Grip Button (Botão Lateral) - Pressionar uma vez
                bool gripPressed = false;
                if (device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.gripButton, out gripPressed))
                {
                    if (gripPressed && !wasGripPressed && !isReloading)
                    {
                        StartReload();
                        wasGripPressed = true;
                        return;
                    }
                    else if (!gripPressed)
                    {
                        wasGripPressed = false;
                    }
                }

                // Check Primary Button (Botão Y/B) - Pressionar uma vez
                bool primaryPressed = false;
                if (device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out primaryPressed))
                {
                    if (primaryPressed && !wasPrimaryPressed && !isReloading)
                    {
                        StartReload();
                        wasPrimaryPressed = true;
                        return;
                    }
                    else if (!primaryPressed)
                    {
                        wasPrimaryPressed = false;
                    }
                }
            }

            // Automatic reload when ammo is empty (fallback)
            if (currentAmmo == 0 && !isReloading)
            {
                StartReload();
            }
        }

        /// Start reload process
        public void StartReload()
        {
            if (isReloading || currentAmmo == maxAmmo) return;

            StartCoroutine(ReloadCoroutine());
        }

        /// Reload coroutine
        private IEnumerator ReloadCoroutine()
        {
            isReloading = true;

            // Play reload sound
            PlayReloadSound();

            // Haptic feedback
            SendHapticFeedback(reloadHapticIntensity, reloadHapticDuration);

            Debug.Log("[VRWeapon] Reloading...");

            yield return new WaitForSeconds(reloadTime);

            currentAmmo = maxAmmo;
            isReloading = false;

            // Second haptic pulse when reload complete
            SendHapticFeedback(reloadHapticIntensity * 0.5f, reloadHapticDuration * 0.5f);

            Debug.Log("[VRWeapon] Reload complete");
        }
        #endregion

        #region Visual Feedback
        /// Update laser sight line renderer
        private void UpdateLaserSight()
        {
            if (!laserSight.enabled) return;

            Vector3 endPosition;
            RaycastHit hit;

            if (Physics.Raycast(shootPoint.position, shootPoint.forward, out hit, range, targetLayer))
            {
                endPosition = hit.point;
                laserSight.startColor = Color.red;
                laserSight.endColor = Color.red;
            }
            else
            {
                endPosition = shootPoint.position + shootPoint.forward * range;
                laserSight.startColor = new Color(1f, 0f, 0f, 0.3f);
                laserSight.endColor = new Color(1f, 0f, 0f, 0.1f);
            }

            laserSight.SetPosition(0, shootPoint.position);
            laserSight.SetPosition(1, endPosition);
        }

        /// Spawn muzzle flash effect
        private void SpawnMuzzleFlash()
        {
            if (muzzleFlashPrefab != null)
            {
                GameObject flash = Instantiate(muzzleFlashPrefab, shootPoint.position, shootPoint.rotation);
                flash.transform.SetParent(shootPoint);

                // Inicializar efeito melhorado se tiver o componente
                MuzzleFlashEffect effect = flash.GetComponent<MuzzleFlashEffect>();
                if (effect == null)
                {
                    // Fallback: destruir após tempo padrão
                    Destroy(flash, 0.1f);
                }
            }
        }

        /// Spawn bullet tracer effect
        private void SpawnBulletTracer(Vector3 start, Vector3 end)
        {
            if (bulletTracerPrefab != null)
            {
                GameObject tracer = Instantiate(bulletTracerPrefab, start, Quaternion.identity);

                // Tentar usar novo sistema de tracer
                BulletTracerEffect tracerEffect = tracer.GetComponent<BulletTracerEffect>();
                if (tracerEffect != null)
                {
                    tracerEffect.Initialize(start, end, tracerSpeed);
                }
                else
                {
                    // Fallback: sistema antigo
                    StartCoroutine(MoveTracer(tracer, start, end));
                }
            }
        }

        /// Move tracer from start to end (sistema antigo - fallback)
        private IEnumerator MoveTracer(GameObject tracer, Vector3 start, Vector3 end)
        {
            float distance = Vector3.Distance(start, end);
            float duration = distance / tracerSpeed;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                tracer.transform.position = Vector3.Lerp(start, end, t);
                yield return null;
            }

            Destroy(tracer);
        }

        /// Spawn bullet impact effect
        private void SpawnImpactEffect(Vector3 position, Vector3 normal)
        {
            if (bulletImpactPrefab != null)
            {
                GameObject impact = Instantiate(bulletImpactPrefab, position, Quaternion.LookRotation(normal));

                // Inicializar efeito melhorado se tiver o componente
                BulletImpactEffect impactEffect = impact.GetComponent<BulletImpactEffect>();
                if (impactEffect != null)
                {
                    impactEffect.Initialize(normal);
                }
                else
                {
                    // Fallback: destruir após tempo padrão
                    Destroy(impact, 2f);
                }
            }
        }
        #endregion

        #region Recoil
        /// Apply recoil effect to weapon
        private void ApplyRecoil()
        {
            // Simple recoil: push back and up slightly
            Vector3 recoilOffset = new Vector3(
                Random.Range(-recoilAmount, recoilAmount),
                recoilAmount * 0.5f,
                -recoilAmount
            );

            transform.localPosition = originalPosition + recoilOffset;
            recoilTimer = 0f;
        }

        /// Recover from recoil smoothly
        private void UpdateRecoilRecovery()
        {
            if (!enableRecoil) return;

            recoilTimer += Time.deltaTime * recoilRecoverySpeed;
            transform.localPosition = Vector3.Lerp(transform.localPosition, originalPosition, recoilTimer);
        }
        #endregion

        #region Haptic Feedback
        /// Send haptic impulse to controller
        private void SendHapticFeedback(float intensity, float duration)
        {
            if (controller != null)
            {
                controller.SendHapticImpulse(intensity, duration);
            }
        }
        #endregion

        #region Audio
        /// Play shoot sound
        private void PlayShootSound()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayShootSound(shootPoint.position);
            }
        }

        /// Play reload sound
        private void PlayReloadSound()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayReloadSound();
            }
        }

        /// Play empty gun sound
        private void PlayEmptySound()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayEmptyGunSound();
            }
        }
        #endregion

        #region Public Methods
        /// Add ammo to weapon
        public void AddAmmo(int amount)
        {
            currentAmmo = Mathf.Min(currentAmmo + amount, maxAmmo);
        }

        /// Set laser sight visibility
        public void SetLaserSightEnabled(bool enabled)
        {
            showLaserSight = enabled;
            if (!enabled && laserSight != null)
            {
                laserSight.enabled = false;
            }
        }

        /// Get weapon info string
        public string GetWeaponInfo()
        {
            return $"Ammo: {currentAmmo}/{maxAmmo}, Fire Rate: {fireRate:F2}s, Range: {range}m";
        }
        #endregion

        #region Debug
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (shootPoint != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawRay(shootPoint.position, shootPoint.forward * 2f);
                Gizmos.DrawWireSphere(shootPoint.position, 0.05f);
            }
        }
#endif
        #endregion
    }
}

using UnityEngine;

namespace VRAimLab.Gameplay
{
    /// Enhanced Laser Sight with visual effects
    /// Shows where weapon is aiming with customizable laser beam
    public class LaserSight : MonoBehaviour
    {
        [Header("Laser Settings")]
        [SerializeField] private bool isEnabled = true;
        [SerializeField] private float maxDistance = 100f;
        [SerializeField] private LayerMask hitLayers = ~0; // Hit everything by default
        [SerializeField] private Transform laserOrigin; // Where laser starts (usually barrel)

        [Header("Visual Settings")]
        [SerializeField] private Color laserColor = Color.red;
        [SerializeField] private float laserWidth = 0.002f;
        [SerializeField] private float endDotSize = 0.05f;
        [SerializeField] private bool useBrightMaterial = true;
        [SerializeField] private float brightness = 2f;

        [Header("End Dot Settings")]
        [SerializeField] private bool showEndDot = true;
        [SerializeField] private Color endDotColor = Color.red;
        [SerializeField] private bool pulseEndDot = true;
        [SerializeField] private float pulseSpeed = 3f;
        [SerializeField] private float pulseIntensity = 0.3f;

        [Header("Advanced Effects")]
        [SerializeField] private bool addNoise = false;
        [SerializeField] private float noiseAmount = 0.001f;
        [SerializeField] private float noiseSpeed = 10f;

        private LineRenderer lineRenderer;
        private GameObject endDot;
        private Renderer endDotRenderer;
        private Material laserMaterial;
        private Material endDotMaterial;
        private RaycastHit hitInfo;
        private float pulseTimer;

        void Awake()
        {
            SetupLaser();
            SetupEndDot();
        }

        void OnEnable()
        {
            if (lineRenderer != null)
            {
                lineRenderer.enabled = isEnabled;
            }
            if (endDot != null)
            {
                endDot.SetActive(isEnabled && showEndDot);
            }
        }

        void Update()
        {
            if (!isEnabled) return;

            UpdateLaser();

            if (showEndDot && pulseEndDot)
            {
                UpdatePulse();
            }
        }

        /// Setup line renderer for laser beam
        private void SetupLaser()
        {
            // Create or get LineRenderer
            lineRenderer = GetComponent<LineRenderer>();
            if (lineRenderer == null)
            {
                lineRenderer = gameObject.AddComponent<LineRenderer>();
            }

            // Configure line renderer
            lineRenderer.positionCount = 2;
            lineRenderer.startWidth = laserWidth;
            lineRenderer.endWidth = laserWidth;
            lineRenderer.useWorldSpace = true;

            // Create material
            if (useBrightMaterial)
            {
                laserMaterial = new Material(Shader.Find("Sprites/Default"));
                laserMaterial.color = laserColor * brightness;
            }
            else
            {
                laserMaterial = new Material(Shader.Find("Standard"));
                laserMaterial.color = laserColor;
            }

            lineRenderer.material = laserMaterial;
            lineRenderer.startColor = laserColor;
            lineRenderer.endColor = laserColor;

            // Disable shadows for performance
            lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lineRenderer.receiveShadows = false;
        }

        /// Setup end dot (point where laser hits)
        private void SetupEndDot()
        {
            if (!showEndDot) return;

            // Create sphere for end dot
            endDot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            endDot.name = "Laser End Dot";
            endDot.transform.SetParent(transform);
            endDot.transform.localScale = Vector3.one * endDotSize;

            // Remove collider
            Destroy(endDot.GetComponent<Collider>());

            // Setup material
            endDotRenderer = endDot.GetComponent<Renderer>();
            if (useBrightMaterial)
            {
                endDotMaterial = new Material(Shader.Find("Sprites/Default"));
                endDotMaterial.color = endDotColor * brightness;
            }
            else
            {
                endDotMaterial = new Material(Shader.Find("Standard"));
                endDotMaterial.SetFloat("_Mode", 3); // Transparent mode
                endDotMaterial.color = endDotColor;
            }

            endDotRenderer.material = endDotMaterial;
            endDotRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            endDotRenderer.receiveShadows = false;
        }

        /// Update laser position and hit detection
        private void UpdateLaser()
        {
            // Determine laser origin
            Vector3 origin = laserOrigin != null ? laserOrigin.position : transform.position;
            Vector3 direction = laserOrigin != null ? laserOrigin.forward : transform.forward;

            // Add noise if enabled
            if (addNoise)
            {
                Vector3 noise = new Vector3(
                    Mathf.PerlinNoise(Time.time * noiseSpeed, 0) - 0.5f,
                    Mathf.PerlinNoise(0, Time.time * noiseSpeed) - 0.5f,
                    0
                ) * noiseAmount;
                direction += noise;
                direction.Normalize();
            }

            Vector3 endPosition;

            // Raycast to find hit point
            if (Physics.Raycast(origin, direction, out hitInfo, maxDistance, hitLayers))
            {
                endPosition = hitInfo.point;
            }
            else
            {
                endPosition = origin + direction * maxDistance;
            }

            // Update line renderer
            lineRenderer.SetPosition(0, origin);
            lineRenderer.SetPosition(1, endPosition);

            // Update end dot
            if (showEndDot && endDot != null)
            {
                endDot.transform.position = endPosition;
                endDot.SetActive(true);
            }
        }

        /// Update pulse effect for end dot
        private void UpdatePulse()
        {
            pulseTimer += Time.deltaTime * pulseSpeed;
            float pulse = 1f + Mathf.Sin(pulseTimer) * pulseIntensity;

            if (endDot != null)
            {
                endDot.transform.localScale = Vector3.one * endDotSize * pulse;
            }

            if (endDotMaterial != null)
            {
                Color pulsedColor = endDotColor * (useBrightMaterial ? brightness * pulse : pulse);
                endDotMaterial.color = pulsedColor;
            }
        }

        /// Enable or disable laser sight
        public void SetEnabled(bool enabled)
        {
            isEnabled = enabled;

            if (lineRenderer != null)
            {
                lineRenderer.enabled = enabled;
            }

            if (endDot != null)
            {
                endDot.SetActive(enabled && showEndDot);
            }
        }

        /// Change laser color at runtime
        public void SetLaserColor(Color color)
        {
            laserColor = color;

            if (laserMaterial != null)
            {
                laserMaterial.color = useBrightMaterial ? color * brightness : color;
            }

            if (lineRenderer != null)
            {
                lineRenderer.startColor = color;
                lineRenderer.endColor = color;
            }
        }

        /// Change laser width at runtime
        public void SetLaserWidth(float width)
        {
            laserWidth = width;

            if (lineRenderer != null)
            {
                lineRenderer.startWidth = width;
                lineRenderer.endWidth = width;
            }
        }

        /// Get current hit information
        public RaycastHit GetHitInfo()
        {
            return hitInfo;
        }

        /// Check if laser is currently hitting something
        public bool IsHitting()
        {
            return hitInfo.collider != null;
        }

        void OnDisable()
        {
            if (lineRenderer != null)
            {
                lineRenderer.enabled = false;
            }

            if (endDot != null)
            {
                endDot.SetActive(false);
            }
        }

        void OnDestroy()
        {
            if (endDot != null)
            {
                Destroy(endDot);
            }

            if (laserMaterial != null)
            {
                Destroy(laserMaterial);
            }

            if (endDotMaterial != null)
            {
                Destroy(endDotMaterial);
            }
        }

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (!isEnabled) return;

            Vector3 origin = laserOrigin != null ? laserOrigin.position : transform.position;
            Vector3 direction = laserOrigin != null ? laserOrigin.forward : transform.forward;

            Gizmos.color = laserColor;
            Gizmos.DrawRay(origin, direction * maxDistance);

            if (showEndDot && Physics.Raycast(origin, direction, out RaycastHit hit, maxDistance, hitLayers))
            {
                Gizmos.DrawWireSphere(hit.point, endDotSize);
            }
        }
#endif
    }
}

using UnityEngine;

namespace VRAimLab.Gameplay
{
    /// <summary>
    /// Bullet Tracer Effect melhorado com trail renderer
    /// Anexe este script ao prefab de tracer
    /// </summary>
    public class BulletTracerEffect : MonoBehaviour
    {
        [Header("Tracer Settings")]
        [SerializeField] private float speed = 100f;
        [SerializeField] private float lifetime = 1f;
        [SerializeField] private bool useTrail = true;

        [Header("Trail Settings")]
        [SerializeField] private Color trailStartColor = new Color(1f, 0.9f, 0.5f, 1f); // Amarelo brilhante
        [SerializeField] private Color trailEndColor = new Color(1f, 0.5f, 0.2f, 0f); // Orange fade
        [SerializeField] private float trailWidth = 0.05f;
        [SerializeField] private float trailTime = 0.2f;

        [Header("Glow Settings")]
        [SerializeField] private bool useGlow = true;
        [SerializeField] private Color glowColor = new Color(1f, 0.8f, 0.4f);
        [SerializeField] private float glowIntensity = 2f;

        private Vector3 targetPosition;
        private Vector3 startPosition;
        private float startTime;
        private TrailRenderer trail;
        private Light glowLight;

        public void Initialize(Vector3 start, Vector3 end, float customSpeed = -1f)
        {
            startPosition = start;
            targetPosition = end;
            transform.position = start;
            startTime = Time.time;

            if (customSpeed > 0)
                speed = customSpeed;

            // Olhar na direção do alvo
            Vector3 direction = (end - start).normalized;
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }

            // Criar trail
            if (useTrail)
            {
                CreateTrail();
            }

            // Criar glow light
            if (useGlow)
            {
                CreateGlowLight();
            }

            // Auto-destruir
            Destroy(gameObject, lifetime);
        }

        void Update()
        {
            float distance = Vector3.Distance(startPosition, targetPosition);
            float duration = distance / speed;
            float elapsed = Time.time - startTime;

            if (elapsed < duration)
            {
                float t = elapsed / duration;
                transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            }
            else
            {
                // Chegou ao destino
                transform.position = targetPosition;

                // Desabilitar trail para fade out natural
                if (trail != null)
                {
                    trail.emitting = false;
                }

                // Fade out da luz
                if (glowLight != null)
                {
                    glowLight.enabled = false;
                }
            }
        }

        private void CreateTrail()
        {
            trail = gameObject.AddComponent<TrailRenderer>();

            trail.time = trailTime;
            trail.startWidth = trailWidth;
            trail.endWidth = trailWidth * 0.3f;
            trail.numCornerVertices = 2;
            trail.numCapVertices = 2;

            // Gradient de cor
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(trailStartColor, 0.0f),
                    new GradientColorKey(trailEndColor, 1.0f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(1.0f, 0.0f),
                    new GradientAlphaKey(0.0f, 1.0f)
                }
            );
            trail.colorGradient = gradient;

            // Material básico emissor
            trail.material = new Material(Shader.Find("Sprites/Default"));
            trail.material.SetColor("_Color", trailStartColor);
        }

        private void CreateGlowLight()
        {
            GameObject lightObj = new GameObject("Tracer Glow");
            lightObj.transform.SetParent(transform);
            lightObj.transform.localPosition = Vector3.zero;

            glowLight = lightObj.AddComponent<Light>();
            glowLight.type = LightType.Point;
            glowLight.color = glowColor;
            glowLight.intensity = glowIntensity;
            glowLight.range = 2f;
            glowLight.shadows = LightShadows.None;
        }
    }
}

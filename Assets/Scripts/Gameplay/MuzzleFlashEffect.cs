using UnityEngine;

namespace VRAimLab.Gameplay
{
    /// Muzzle Flash Effect melhorado com luz e partículas
    /// Anexe este script ao prefab de muzzle flash
    public class MuzzleFlashEffect : MonoBehaviour
    {
        [Header("Flash Settings")]
        [SerializeField] private float lifetime = 0.1f;
        [SerializeField] private bool randomRotation = true;
        [SerializeField] private float scalePulse = 1.2f;

        [Header("Light Settings")]
        [SerializeField] private bool useLight = true;
        [SerializeField] private Color lightColor = new Color(1f, 0.6f, 0.2f); // Orange flash
        [SerializeField] private float lightIntensity = 3f;
        [SerializeField] private float lightRange = 5f;

        [Header("Particle Settings")]
        [SerializeField] private bool useParticles = true;
        [SerializeField] private int particleCount = 20;
        [SerializeField] private float particleSpeed = 5f;

        private Light flashLight;
        private ParticleSystem particles;
        private float spawnTime;
        private Vector3 originalScale;

        void Start()
        {
            spawnTime = Time.time;
            originalScale = transform.localScale;

            // Rotação aleatória para variedade visual
            if (randomRotation)
            {
                transform.Rotate(0, 0, Random.Range(0f, 360f));
            }

            // Criar luz dinâmica
            if (useLight)
            {
                CreateFlashLight();
            }

            // Criar partículas
            if (useParticles)
            {
                CreateParticles();
            }

            // Auto-destruir
            Destroy(gameObject, lifetime);
        }

        void Update()
        {
            float age = Time.time - spawnTime;
            float normalizedAge = age / lifetime;

            // Fade out da luz
            if (flashLight != null)
            {
                flashLight.intensity = Mathf.Lerp(lightIntensity, 0f, normalizedAge);
            }

            // Pulse da escala
            float scale = Mathf.Lerp(scalePulse, 1f, normalizedAge);
            transform.localScale = originalScale * scale;
        }

        private void CreateFlashLight()
        {
            GameObject lightObj = new GameObject("Flash Light");
            lightObj.transform.SetParent(transform);
            lightObj.transform.localPosition = Vector3.zero;

            flashLight = lightObj.AddComponent<Light>();
            flashLight.type = LightType.Point;
            flashLight.color = lightColor;
            flashLight.intensity = lightIntensity;
            flashLight.range = lightRange;
            flashLight.shadows = LightShadows.None; // Sem sombras para performance
        }

        private void CreateParticles()
        {
            GameObject particleObj = new GameObject("Muzzle Particles");
            particleObj.transform.SetParent(transform);
            particleObj.transform.localPosition = Vector3.zero;

            particles = particleObj.AddComponent<ParticleSystem>();

            var main = particles.main;
            main.startLifetime = 0.3f;
            main.startSpeed = particleSpeed;
            main.startSize = 0.1f;
            main.startColor = new Color(1f, 0.8f, 0.5f, 0.5f);
            main.maxParticles = particleCount;

            var emission = particles.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] {
                new ParticleSystem.Burst(0f, particleCount)
            });

            var shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 15f;

            var colorOverLifetime = particles.colorOverLifetime;
            colorOverLifetime.enabled = true;

            // Criar material para as partículas (evita cor roxa)
            var renderer = particles.GetComponent<ParticleSystemRenderer>();
            if (renderer != null)
            {
                renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
                renderer.material.SetColor("_Color", new Color(1f, 0.8f, 0.5f));
            }

            particles.Play();
        }
    }
}

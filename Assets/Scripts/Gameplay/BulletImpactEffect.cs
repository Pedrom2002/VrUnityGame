using UnityEngine;

namespace VRAimLab.Gameplay
{
    /// Bullet Impact Effect melhorado com partículas, marca de bala e som
    /// Anexe este script ao prefab de impacto
    public class BulletImpactEffect : MonoBehaviour
    {
        [Header("Impact Settings")]
        [SerializeField] private float lifetime = 3f;
        [SerializeField] private bool createBulletHole = true;
        [SerializeField] private float bulletHoleSize = 0.1f;

        [Header("Particle Settings")]
        [SerializeField] private bool useParticles = true;
        [SerializeField] private int sparkCount = 10;
        [SerializeField] private int smokeCount = 5;
        [SerializeField] private Color sparkColor = new Color(1f, 0.8f, 0.3f);
        [SerializeField] private Color smokeColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

        [Header("Impact Light")]
        [SerializeField] private bool useImpactLight = true;
        [SerializeField] private Color lightColor = new Color(1f, 0.8f, 0.5f);
        [SerializeField] private float lightIntensity = 2f;
        [SerializeField] private float lightDuration = 0.1f;

        private Light impactLight;
        private GameObject bulletHole;

        void Start()
        {
            // Criar efeitos
            if (useParticles)
            {
                CreateSparkParticles();
                CreateSmokeParticles();
            }

            if (useImpactLight)
            {
                CreateImpactLight();
            }

            if (createBulletHole)
            {
                CreateBulletHole();
            }

            // Auto-destruir
            Destroy(gameObject, lifetime);
        }

        void Update()
        {
            // Fade out da luz
            if (impactLight != null)
            {
                impactLight.intensity = Mathf.Max(0, impactLight.intensity - Time.deltaTime * (lightIntensity / lightDuration));

                if (impactLight.intensity <= 0)
                {
                    Destroy(impactLight.gameObject);
                    impactLight = null;
                }
            }
        }

        private void CreateSparkParticles()
        {
            GameObject sparkObj = new GameObject("Sparks");
            sparkObj.transform.SetParent(transform);
            sparkObj.transform.localPosition = Vector3.zero;
            sparkObj.transform.localRotation = Quaternion.identity;

            ParticleSystem sparks = sparkObj.AddComponent<ParticleSystem>();

            var main = sparks.main;
            main.startLifetime = 0.3f;
            main.startSpeed = 5f;
            main.startSize = 0.05f;
            main.startColor = sparkColor;
            main.maxParticles = sparkCount;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = sparks.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] {
                new ParticleSystem.Burst(0f, sparkCount)
            });

            var shape = sparks.shape;
            shape.shapeType = ParticleSystemShapeType.Hemisphere;
            shape.radius = 0.1f;

            var velocityOverLifetime = sparks.velocityOverLifetime;
            velocityOverLifetime.enabled = true;
            velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(-9.81f); // Gravidade

            var colorOverLifetime = sparks.colorOverLifetime;
            colorOverLifetime.enabled = true;

            // Criar material para as partículas (evita cor roxa)
            var sparkRenderer = sparks.GetComponent<ParticleSystemRenderer>();
            if (sparkRenderer != null)
            {
                sparkRenderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
                sparkRenderer.material.SetColor("_Color", sparkColor);
            }

            sparks.Play();
        }

        private void CreateSmokeParticles()
        {
            GameObject smokeObj = new GameObject("Smoke");
            smokeObj.transform.SetParent(transform);
            smokeObj.transform.localPosition = Vector3.zero;
            smokeObj.transform.localRotation = Quaternion.identity;

            ParticleSystem smoke = smokeObj.AddComponent<ParticleSystem>();

            var main = smoke.main;
            main.startLifetime = 1f;
            main.startSpeed = 1f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.3f);
            main.startColor = smokeColor;
            main.maxParticles = smokeCount;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = smoke.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] {
                new ParticleSystem.Burst(0f, smokeCount)
            });

            var shape = smoke.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 30f;

            var sizeOverLifetime = smoke.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 2f));

            var colorOverLifetime = smoke.colorOverLifetime;
            colorOverLifetime.enabled = true;

            // Criar material para as partículas (evita cor roxa)
            var smokeRenderer = smoke.GetComponent<ParticleSystemRenderer>();
            if (smokeRenderer != null)
            {
                smokeRenderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
                smokeRenderer.material.SetColor("_Color", smokeColor);
            }

            smoke.Play();
        }

        private void CreateImpactLight()
        {
            GameObject lightObj = new GameObject("Impact Light");
            lightObj.transform.SetParent(transform);
            lightObj.transform.localPosition = Vector3.forward * 0.1f;

            impactLight = lightObj.AddComponent<Light>();
            impactLight.type = LightType.Point;
            impactLight.color = lightColor;
            impactLight.intensity = lightIntensity;
            impactLight.range = 3f;
            impactLight.shadows = LightShadows.None;
        }

        private void CreateBulletHole()
        {
            bulletHole = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bulletHole.name = "Bullet Hole";
            bulletHole.transform.SetParent(transform);
            bulletHole.transform.localPosition = Vector3.zero;
            bulletHole.transform.localScale = Vector3.one * bulletHoleSize;

            // Remover collider
            Destroy(bulletHole.GetComponent<Collider>());

            // Material escuro
            Renderer renderer = bulletHole.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = new Material(Shader.Find("Standard"));
                renderer.material.color = new Color(0.1f, 0.1f, 0.1f);
            }

            // Afundar um pouco na superfície
            bulletHole.transform.localPosition = -Vector3.forward * 0.01f;
        }

        /// Inicializar com normal da superfície para orientar efeitos
        public void Initialize(Vector3 normal)
        {
            // Orientar para a normal da superfície
            transform.rotation = Quaternion.LookRotation(normal);
        }
    }
}

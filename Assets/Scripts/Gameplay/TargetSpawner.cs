using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRAimLab.Core;

namespace VRAimLab.Gameplay
{
    /// Spawn configuration for different game modes
    [System.Serializable]
    public class SpawnConfig
    {
        public GameMode gameMode;
        public float spawnInterval = 1f;
        public int maxActiveTargets = 5;
        public TargetType[] allowedTargetTypes;
        public float targetLifetime = 5f;
        public int basePoints = 100;
    }

    /// Target Spawner - handles target spawning with object pooling
    /// Configurable per game mode with difficulty progression
    /// Uses wave system for increasing challenge
    public class TargetSpawner : MonoBehaviour
    {
        #region Inspector Fields
        [Header("Prefabs")]
        [SerializeField] private GameObject basicTargetPrefab;
        [SerializeField] private GameObject movingTargetPrefab;
        [SerializeField] private GameObject precisionTargetPrefab;

        [Header("Spawn Points")]
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField] private bool useRandomPositions = true;
        [SerializeField] private Vector3 spawnAreaMin = new Vector3(-5f, 1f, 5f);
        [SerializeField] private Vector3 spawnAreaMax = new Vector3(5f, 3f, 10f);

        [Header("Spawn Settings")]
        [SerializeField] private SpawnConfig[] gameModeConfigs;

        #pragma warning disable 0414 // Field assigned but never used - reserved for future use
        [SerializeField] private int poolSize = 20;
        #pragma warning restore 0414

        [Header("Difficulty Progression")]
        [SerializeField] private bool enableProgression = true;
        [SerializeField] private float intervalDecreasePerLevel = 0.1f;
        [SerializeField] private int maxTargetIncreasePerLevel = 1;
        [SerializeField] private float minSpawnInterval = 0.3f;
        #endregion

        #region Private Fields
        private SpawnConfig currentConfig;
        private float currentSpawnInterval;
        private int currentMaxTargets;
        private bool isSpawning = false;
        private float spawnTimer;
        private List<Target> activeTargets = new List<Target>();
        private ObjectPooler objectPooler;
        private int waveNumber = 0;
        #endregion

        #region Properties
        public int ActiveTargetCount => activeTargets.Count;
        public int WaveNumber => waveNumber;
        public bool IsSpawning => isSpawning;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            // Setup object pooler if not exists
            objectPooler = GetComponent<ObjectPooler>();
            if (objectPooler == null)
            {
                objectPooler = gameObject.AddComponent<ObjectPooler>();
            }

            // Validate spawn points
            if (spawnPoints == null || spawnPoints.Length == 0)
            {
                Debug.LogWarning("[TargetSpawner] No spawn points set! Will use random positions.");
                useRandomPositions = true;
            }
        }

        private void Start()
        {
            InitializePools();
        }

        private void Update()
        {
            if (isSpawning)
            {
                UpdateSpawning();
            }
        }
        #endregion

        #region Initialization
        /// Initialize object pools for targets
        private void InitializePools()
        {
            // Note: If using ObjectPooler component, configure pools in inspector
            // This is a fallback if pools aren't configured

            if (ObjectPooler.Instance == null)
            {
                Debug.LogWarning("[TargetSpawner] No ObjectPooler instance found!");
                return;
            }

            Debug.Log("[TargetSpawner] Pools initialized");
        }
        #endregion

        #region Spawning Control
        /// Start spawning targets for a game mode
        public void StartSpawning(GameMode gameMode)
        {
            // Find config for this game mode
            currentConfig = GetConfigForMode(gameMode);

            if (currentConfig == null)
            {
                Debug.LogError($"[TargetSpawner] No spawn config for {gameMode}!");
                return;
            }

            // Initialize spawn parameters
            currentSpawnInterval = currentConfig.spawnInterval;
            currentMaxTargets = currentConfig.maxActiveTargets;
            spawnTimer = 0f;
            waveNumber = 0;
            isSpawning = true;

            // Clear any existing targets
            ClearAllTargets();

            Debug.Log($"[TargetSpawner] Started spawning for {gameMode} mode");
        }

        /// Stop spawning targets
        public void StopSpawning()
        {
            isSpawning = false;
            ClearAllTargets();
            Debug.Log("[TargetSpawner] Stopped spawning");
        }

        /// Pause spawning (keeps current targets active)
        public void PauseSpawning()
        {
            isSpawning = false;
        }

        /// Resume spawning
        public void ResumeSpawning()
        {
            isSpawning = true;
        }
        #endregion

        #region Spawn Update
        /// Update spawning logic each frame
        private void UpdateSpawning()
        {
            spawnTimer += Time.deltaTime;

            // Remove inactive targets from list
            activeTargets.RemoveAll(t => t == null || !t.gameObject.activeInHierarchy);

            // Check if we should spawn new target
            if (spawnTimer >= currentSpawnInterval && activeTargets.Count < currentMaxTargets)
            {
                SpawnTarget();
                spawnTimer = 0f;
            }
        }

        /// Spawn a single target
        private void SpawnTarget()
        {
            if (currentConfig == null) return;

            // Choose random target type from allowed types
            TargetType targetType = GetRandomTargetType();

            // Get spawn position
            Vector3 spawnPosition = GetSpawnPosition();
            Quaternion spawnRotation = Quaternion.identity;

            // Spawn target based on type
            GameObject targetObj = null;

            if (ObjectPooler.Instance != null)
            {
                string poolTag = GetPoolTagForType(targetType);
                targetObj = ObjectPooler.Instance.SpawnFromPool(poolTag, spawnPosition, spawnRotation);
            }
            else
            {
                // Fallback: instantiate directly
                GameObject prefab = GetPrefabForType(targetType);
                if (prefab != null)
                {
                    targetObj = Instantiate(prefab, spawnPosition, spawnRotation);
                }
            }

            if (targetObj == null)
            {
                Debug.LogWarning($"[TargetSpawner] Failed to spawn target of type {targetType}");
                return;
            }

            // Setup target
            Target target = targetObj.GetComponent<Target>();
            if (target != null)
            {
                target.Initialize(targetType, currentConfig.basePoints, currentConfig.targetLifetime);
                activeTargets.Add(target);

                // Debug log to verify configuration
                Debug.Log($"[TargetSpawner] Spawned {targetType} - Lifetime: {currentConfig.targetLifetime}s, Points: {currentConfig.basePoints}");
            }

            waveNumber++;
        }
        #endregion

        #region Difficulty Progression
        /// Increase difficulty level
        public void IncreaseDifficulty(int level)
        {
            if (!enableProgression) return;

            // Decrease spawn interval
            currentSpawnInterval = Mathf.Max(
                minSpawnInterval,
                currentConfig.spawnInterval - (intervalDecreasePerLevel * (level - 1))
            );

            // Increase max targets
            currentMaxTargets = currentConfig.maxActiveTargets + (maxTargetIncreasePerLevel * (level - 1));

            Debug.Log($"[TargetSpawner] Difficulty level {level}: Interval={currentSpawnInterval:F2}s, MaxTargets={currentMaxTargets}");
        }
        #endregion

        #region Target Management
        /// Clear all active targets
        public void ClearAllTargets()
        {
            foreach (Target target in activeTargets)
            {
                if (target != null && target.gameObject != null)
                {
                    target.ForceDestroy();
                }
            }

            activeTargets.Clear();
        }

        /// Get count of active targets of a specific type
        public int GetActiveTargetCount(TargetType type)
        {
            int count = 0;
            foreach (Target target in activeTargets)
            {
                if (target != null && target.Type == type)
                    count++;
            }
            return count;
        }
        #endregion

        #region Spawn Configuration
        /// Get spawn config for game mode
        private SpawnConfig GetConfigForMode(GameMode mode)
        {
            foreach (SpawnConfig config in gameModeConfigs)
            {
                if (config.gameMode == mode)
                    return config;
            }

            // Return default config if not found
            return CreateDefaultConfig(mode);
        }

        /// Create default spawn config for a game mode
        private SpawnConfig CreateDefaultConfig(GameMode mode)
        {
            SpawnConfig config = new SpawnConfig
            {
                gameMode = mode,
                basePoints = 100
            };

            switch (mode)
            {
                case GameMode.Training:
                    config.spawnInterval = 2f;
                    config.maxActiveTargets = 3;
                    config.allowedTargetTypes = new[] { TargetType.Basic };
                    config.targetLifetime = 10f;
                    break;

                case GameMode.Precision:
                    config.spawnInterval = 2f;
                    config.maxActiveTargets = 4;
                    config.allowedTargetTypes = new[] { TargetType.Precision, TargetType.Basic };
                    config.targetLifetime = 6f;
                    break;

                case GameMode.Speed:
                    config.spawnInterval = 0.8f;
                    config.maxActiveTargets = 6;
                    config.allowedTargetTypes = new[] { TargetType.Speed, TargetType.Basic };
                    config.targetLifetime = 3f;
                    break;

                case GameMode.Tracking:
                    config.spawnInterval = 1.5f;
                    config.maxActiveTargets = 4;
                    config.allowedTargetTypes = new[] { TargetType.Moving };
                    config.targetLifetime = 8f;
                    break;

                case GameMode.Challenge:
                    config.spawnInterval = 1f;
                    config.maxActiveTargets = 8;
                    config.allowedTargetTypes = new[] { TargetType.Basic, TargetType.Speed, TargetType.Precision, TargetType.Moving, TargetType.Combo };
                    config.targetLifetime = 5f;
                    break;
            }

            return config;
        }

        /// Get random target type from allowed types
        private TargetType GetRandomTargetType()
        {
            if (currentConfig.allowedTargetTypes == null || currentConfig.allowedTargetTypes.Length == 0)
            {
                return TargetType.Basic;
            }

            return currentConfig.allowedTargetTypes[Random.Range(0, currentConfig.allowedTargetTypes.Length)];
        }

        /// Get pool tag for target type
        private string GetPoolTagForType(TargetType type)
        {
            switch (type)
            {
                case TargetType.Moving:
                    return "MovingTarget";
                case TargetType.Precision:
                    return "PrecisionTarget";
                default:
                    return "BasicTarget";
            }
        }

        /// Get prefab for target type (fallback if no pooler)
        private GameObject GetPrefabForType(TargetType type)
        {
            switch (type)
            {
                case TargetType.Moving:
                    return movingTargetPrefab;
                case TargetType.Precision:
                    return precisionTargetPrefab;
                default:
                    return basicTargetPrefab;
            }
        }
        #endregion

        #region Spawn Position
        /// Get spawn position (either fixed point or random)
        private Vector3 GetSpawnPosition()
        {
            if (useRandomPositions || spawnPoints == null || spawnPoints.Length == 0)
            {
                return GetRandomSpawnPosition();
            }
            else
            {
                return GetFixedSpawnPosition();
            }
        }

        /// Get random position within spawn area (relative to camera)
        /// Uses same logic as manual spawn (T, Y, U keys) - spawns in front of camera
        private Vector3 GetRandomSpawnPosition()
        {
            // Get camera reference
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                // Fallback to world space if no camera
                return new Vector3(
                    Random.Range(spawnAreaMin.x, spawnAreaMax.x),
                    Random.Range(spawnAreaMin.y, spawnAreaMax.y),
                    Random.Range(spawnAreaMin.z, spawnAreaMax.z)
                );
            }

            // Spawn in front of camera (same as manual spawn with T, Y, U)
            Vector3 spawnPosition = mainCamera.transform.position + mainCamera.transform.forward * 5f;

            return spawnPosition;
        }

        /// Get fixed spawn point position
        private Vector3 GetFixedSpawnPosition()
        {
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            return spawnPoint.position;
        }
        #endregion

        #region Debug Helpers
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            // Draw spawn area
            if (useRandomPositions)
            {
                Gizmos.color = Color.green;
                Vector3 center = (spawnAreaMin + spawnAreaMax) / 2f;
                Vector3 size = spawnAreaMax - spawnAreaMin;
                Gizmos.DrawWireCube(center, size);
            }

            // Draw spawn points
            if (spawnPoints != null)
            {
                Gizmos.color = Color.cyan;
                foreach (Transform spawnPoint in spawnPoints)
                {
                    if (spawnPoint != null)
                    {
                        Gizmos.DrawWireSphere(spawnPoint.position, 0.3f);
                    }
                }
            }
        }
#endif

        /// Print spawner statistics
        public void PrintStats()
        {
            Debug.Log($"[TargetSpawner] Stats: Active={activeTargets.Count}/{currentMaxTargets}, " +
                      $"Interval={currentSpawnInterval:F2}s, Wave={waveNumber}");
        }
        #endregion
    }
}

using UnityEngine;
using VRAimLab.Core;
using VRAimLab.Gameplay;

// Enable legacy Input for keyboard shortcuts
// This allows the old Input system to work alongside the new Input System
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
#define USE_INPUT_SYSTEM_ONLY
#endif

namespace VRAimLab.Utilities
{
    /// <summary>
    /// Debug Helper - provides keyboard shortcuts and testing utilities
    /// Allows testing without VR headset using keyboard/mouse
    /// </summary>
    public class DebugHelper : MonoBehaviour
    {
        #region Inspector Fields
        [Header("Debug Settings")]
        [SerializeField] private bool enableDebugMode = true;
        [SerializeField] private bool showDebugUI = true;
        [SerializeField] private bool allowKeyboardControls = true;

        [Header("Testing")]
        #pragma warning disable 0414 // Field assigned but never used - reserved for future VR testing options
        [SerializeField] private bool skipVRRequirement = false;
        #pragma warning restore 0414
        [SerializeField] private GameObject debugWeapon; // Non-VR weapon for testing

        [Header("Performance Monitoring")]
        [SerializeField] private bool showFPS = true;
        [SerializeField] private bool showMemoryUsage = false;
        [SerializeField] private bool logPerformanceWarnings = true;
        #endregion

        #region Private Fields
        private float deltaTime = 0f;
        private float fpsUpdateInterval = 0.5f;
        private float fpsTimer = 0f;
        private float currentFPS = 0f;

        private GUIStyle guiStyle;
        private Rect fpsRect;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            if (!enableDebugMode) return;

            // Setup GUI style
            guiStyle = new GUIStyle();
            guiStyle.fontSize = 16;
            guiStyle.normal.textColor = Color.white;
            guiStyle.alignment = TextAnchor.UpperLeft;

            fpsRect = new Rect(10, 10, 200, 100);
        }

        private void Update()
        {
            if (!enableDebugMode) return;

#if !USE_INPUT_SYSTEM_ONLY
            HandleKeyboardShortcuts();
#endif
            UpdatePerformanceMonitoring();
        }

        private void OnGUI()
        {
            if (!enableDebugMode || !showDebugUI) return;

            DrawDebugInfo();
        }
        #endregion

        #region Keyboard Shortcuts
        /// <summary>
        /// Handle keyboard shortcuts for testing
        /// NOTE: Requires Legacy Input enabled in Player Settings
        /// Edit → Project Settings → Player → Active Input Handling → set to "Both"
        /// </summary>
        private void HandleKeyboardShortcuts()
        {
            if (!allowKeyboardControls) return;

#if USE_INPUT_SYSTEM_ONLY
            Debug.LogWarning("[DebugHelper] Keyboard shortcuts require Legacy Input. Enable 'Both' in Player Settings → Active Input Handling");
            return;
#endif

            // Game Mode Selection (Number Keys)
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                StartGameMode(GameMode.Training);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                StartGameMode(GameMode.Precision);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                StartGameMode(GameMode.Speed);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                StartGameMode(GameMode.Tracking);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                StartGameMode(GameMode.Challenge);
            }

            // Game Control
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                TogglePause();
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                RestartGame();
            }

            if (Input.GetKeyDown(KeyCode.M))
            {
                ReturnToMenu();
            }

            // Debug Actions - Spawn Targets
            if (Input.GetKeyDown(KeyCode.T))
            {
                SpawnDebugTarget("BasicTarget");
            }

            if (Input.GetKeyDown(KeyCode.Y))
            {
                SpawnDebugTarget("MovingTarget");
            }

            if (Input.GetKeyDown(KeyCode.U))
            {
                SpawnDebugTarget("PrecisionTarget");
            }

            if (Input.GetKeyDown(KeyCode.K))
            {
                KillAllTargets();
            }

            if (Input.GetKeyDown(KeyCode.P))
            {
                PrintStats();
            }

            if (Input.GetKeyDown(KeyCode.F1))
            {
                ToggleDebugUI();
            }

            // Score Testing
            if (Input.GetKeyDown(KeyCode.Plus) || Input.GetKeyDown(KeyCode.KeypadPlus))
            {
                AddDebugScore(100);
            }

            if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus))
            {
                AddDebugScore(-100);
            }

            // Time Control
            if (Input.GetKeyDown(KeyCode.PageUp))
            {
                Time.timeScale = Mathf.Min(Time.timeScale + 0.5f, 3f);
                Debug.Log($"[Debug] Time Scale: {Time.timeScale}x");
            }

            if (Input.GetKeyDown(KeyCode.PageDown))
            {
                Time.timeScale = Mathf.Max(Time.timeScale - 0.5f, 0.1f);
                Debug.Log($"[Debug] Time Scale: {Time.timeScale}x");
            }

            // Mouse shooting (for testing without VR) - DISABLED
            // DISABLED: This bypasses the weapon system and allows shooting without ammo
            /*
            if (Input.GetMouseButtonDown(0))
            {
                TestMouseShoot();
            }
            */
        }

        /// <summary>
        /// Start game with specified mode
        /// </summary>
        private void StartGameMode(GameMode mode)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartGame(mode);
                Debug.Log($"[Debug] Started {mode} mode");
            }
        }

        /// <summary>
        /// Toggle pause
        /// </summary>
        private void TogglePause()
        {
            if (GameManager.Instance != null)
            {
                if (GameManager.Instance.CurrentGameState == GameState.Playing)
                {
                    GameManager.Instance.PauseGame();
                    Debug.Log("[Debug] Game Paused");
                }
                else if (GameManager.Instance.CurrentGameState == GameState.Paused)
                {
                    GameManager.Instance.ResumeGame();
                    Debug.Log("[Debug] Game Resumed");
                }
            }
        }

        /// <summary>
        /// Restart current game
        /// </summary>
        private void RestartGame()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartGame(GameManager.Instance.CurrentGameMode);
                Debug.Log("[Debug] Game Restarted");
            }
        }

        /// <summary>
        /// Return to menu
        /// </summary>
        private void ReturnToMenu()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ReturnToMenu();
                Debug.Log("[Debug] Returned to Menu");
            }
        }
        #endregion

        #region Debug Actions
        /// <summary>
        /// Spawn a debug target at camera position
        /// </summary>
        /// <param name="targetType">Type of target to spawn (BasicTarget, MovingTarget, PrecisionTarget)</param>
        private void SpawnDebugTarget(string targetType = "BasicTarget")
        {
            TargetSpawner spawner = FindFirstObjectByType<TargetSpawner>();
            if (spawner != null)
            {
                // Spawn target in front of camera
                Vector3 spawnPos = Camera.main.transform.position + Camera.main.transform.forward * 5f;

                if (ObjectPooler.Instance != null)
                {
                    GameObject spawnedTarget = ObjectPooler.Instance.SpawnFromPool(targetType, spawnPos, Quaternion.identity);
                    if (spawnedTarget != null)
                    {
                        Debug.Log($"[Debug] Spawned {targetType} at {spawnPos}");
                    }
                    else
                    {
                        Debug.LogWarning($"[Debug] Failed to spawn {targetType}. Make sure the pool is configured!");
                    }
                }
            }
        }

        /// <summary>
        /// Kill all active targets
        /// </summary>
        private void KillAllTargets()
        {
            Target[] targets = FindObjectsByType<Target>(FindObjectsSortMode.None);
            foreach (Target target in targets)
            {
                target.ForceDestroy();
            }
            Debug.Log($"[Debug] Killed {targets.Length} targets");
        }

        /// <summary>
        /// Add debug score
        /// </summary>
        private void AddDebugScore(int points)
        {
            if (GameManager.Instance != null && GameManager.Instance.Score != null)
            {
                GameManager.Instance.Score.AddBonusPoints(points, "Debug");
                Debug.Log($"[Debug] Added {points} points");
            }
        }

        /// <summary>
        /// Test mouse shooting (raycast from camera)
        /// </summary>
        private void TestMouseShoot()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100f))
            {
                Target target = hit.collider.GetComponent<Target>();
                if (target != null)
                {
                    target.OnHit(hit.point, hit.normal);
                    Debug.Log($"[Debug] Hit target: {target.name}");
                }
                else
                {
                    Debug.Log($"[Debug] Hit: {hit.collider.name}");
                }

                // Visual feedback
                Debug.DrawLine(ray.origin, hit.point, Color.red, 1f);
            }
        }

        /// <summary>
        /// Print game statistics
        /// </summary>
        private void PrintStats()
        {
            Debug.Log("========================================");
            Debug.Log("VR AIM LAB - DEBUG STATS");
            Debug.Log("========================================");

            if (GameManager.Instance != null)
            {
                Debug.Log($"Game State: {GameManager.Instance.CurrentGameState}");
                Debug.Log($"Game Mode: {GameManager.Instance.CurrentGameMode}");
                Debug.Log($"Time Remaining: {GameManager.Instance.GetFormattedTime()}");
                Debug.Log($"Score: {GameManager.Instance.CurrentScore}");
                Debug.Log($"Accuracy: {GameManager.Instance.Accuracy:F1}%");

                if (GameManager.Instance.Score != null)
                {
                    Debug.Log($"Streak: {GameManager.Instance.Score.CurrentStreak}");
                    Debug.Log($"Max Streak: {GameManager.Instance.Score.MaxStreak}");
                    Debug.Log($"Combo: x{GameManager.Instance.Score.ComboMultiplier}");
                    Debug.Log($"Rank: {GameManager.Instance.Score.CurrentRank}");
                }
            }

            TargetSpawner spawner = FindFirstObjectByType<TargetSpawner>();
            if (spawner != null)
            {
                Debug.Log($"Active Targets: {spawner.ActiveTargetCount}");
                Debug.Log($"Wave: {spawner.WaveNumber}");
            }

            if (ObjectPooler.Instance != null)
            {
                ObjectPooler.Instance.PrintPoolStats();
            }

            Debug.Log($"FPS: {currentFPS:F1}");
            Debug.Log($"Time Scale: {Time.timeScale}x");
            Debug.Log("========================================");
        }

        /// <summary>
        /// Toggle debug UI
        /// </summary>
        private void ToggleDebugUI()
        {
            showDebugUI = !showDebugUI;
            Debug.Log($"[Debug] Debug UI: {(showDebugUI ? "ON" : "OFF")}");
        }
        #endregion

        #region Performance Monitoring
        /// <summary>
        /// Update performance metrics
        /// </summary>
        private void UpdatePerformanceMonitoring()
        {
            if (!showFPS) return;

            deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
            fpsTimer += Time.unscaledDeltaTime;

            if (fpsTimer >= fpsUpdateInterval)
            {
                currentFPS = 1.0f / deltaTime;
                fpsTimer = 0f;

                // Log performance warnings (disabled by default to reduce console spam)
                // Uncomment if you need performance warnings:
                // if (logPerformanceWarnings && currentFPS < 60f)
                // {
                //     Debug.LogWarning($"[Performance] Low FPS: {currentFPS:F1}");
                // }
            }
        }

        /// <summary>
        /// Draw debug information on screen
        /// </summary>
        private void DrawDebugInfo()
        {
            if (!showDebugUI) return;

            string debugText = "";

            // FPS
            if (showFPS)
            {
                Color fpsColor = currentFPS >= 90f ? Color.green :
                                 currentFPS >= 60f ? Color.yellow :
                                 Color.red;

                string fpsText = $"<color=#{ColorUtility.ToHtmlStringRGB(fpsColor)}>FPS: {currentFPS:F1}</color>\n";
                debugText += fpsText;
            }

            // Memory
            if (showMemoryUsage)
            {
                float memoryMB = System.GC.GetTotalMemory(false) / (1024f * 1024f);
                debugText += $"Memory: {memoryMB:F1} MB\n";
            }

            // Game info
            if (GameManager.Instance != null)
            {
                debugText += $"\nMode: {GameManager.Instance.CurrentGameMode}\n";
                debugText += $"State: {GameManager.Instance.CurrentGameState}\n";
                debugText += $"Score: {GameManager.Instance.CurrentScore}\n";
                debugText += $"Time: {GameManager.Instance.GetFormattedTime()}\n";
            }

            // Controls
            debugText += "\n--- CONTROLS ---\n";
            debugText += "1-5: Start Game Mode\n";
            debugText += "ESC: Pause\n";
            debugText += "R: Restart\n";
            debugText += "M: Menu\n";
            debugText += "T: Spawn Basic\n";
            debugText += "Y: Spawn Moving\n";
            debugText += "U: Spawn Precision\n";
            debugText += "K: Kill All\n";
            debugText += "P: Print Stats\n";
            debugText += "F1: Toggle UI\n";
            debugText += "Mouse: Shoot\n";

            // Draw
            GUI.Label(fpsRect, debugText, guiStyle);
        }
        #endregion

        #region Context Menu Items
        /// <summary>
        /// Print all keyboard shortcuts
        /// </summary>
        [ContextMenu("Print Keyboard Shortcuts")]
        public void PrintKeyboardShortcuts()
        {
            Debug.Log("========================================");
            Debug.Log("VR AIM LAB - KEYBOARD SHORTCUTS");
            Debug.Log("========================================");
            Debug.Log("GAME MODES:");
            Debug.Log("  1 - Training Mode");
            Debug.Log("  2 - Precision Mode");
            Debug.Log("  3 - Speed Mode");
            Debug.Log("  4 - Tracking Mode");
            Debug.Log("  5 - Challenge Mode");
            Debug.Log("\nGAME CONTROL:");
            Debug.Log("  ESC - Pause/Resume");
            Debug.Log("  R - Restart Game");
            Debug.Log("  M - Return to Menu");
            Debug.Log("\nDEBUG:");
            Debug.Log("  T - Spawn Basic Target");
            Debug.Log("  Y - Spawn Moving Target");
            Debug.Log("  U - Spawn Precision Target");
            Debug.Log("  K - Kill All Targets");
            Debug.Log("  P - Print Stats");
            Debug.Log("  F1 - Toggle Debug UI");
            Debug.Log("  +/- - Add/Remove Score");
            Debug.Log("  PgUp/PgDn - Time Scale");
            Debug.Log("\nTESTING:");
            Debug.Log("  Left Mouse - Shoot (raycast from camera)");
            Debug.Log("========================================");
        }

        /// <summary>
        /// Enable debug mode
        /// </summary>
        [ContextMenu("Enable Debug Mode")]
        public void EnableDebugMode()
        {
            enableDebugMode = true;
            showDebugUI = true;
            allowKeyboardControls = true;
            Debug.Log("[Debug] Debug mode enabled");
        }

        /// <summary>
        /// Disable debug mode
        /// </summary>
        [ContextMenu("Disable Debug Mode")]
        public void DisableDebugMode()
        {
            enableDebugMode = false;
            showDebugUI = false;
            Debug.Log("[Debug] Debug mode disabled");
        }
        #endregion
    }
}

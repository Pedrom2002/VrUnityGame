using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRAimLab.UI;
using VRAimLab.Gameplay;

namespace VRAimLab.Core
{
    #region Enums
    /// <summary>
    /// Available game modes in VR Aim Lab
    /// </summary>
    public enum GameMode
    {
        Training,   // No timer, practice mode
        Precision,  // Small targets, accuracy focused
        Speed,      // Fast spawning, reaction time
        Tracking,   // Moving targets only
        Challenge   // Mix of all, hardest difficulty
    }

    /// <summary>
    /// Current state of the game
    /// </summary>
    public enum GameState
    {
        Menu,
        Playing,
        Paused,
        GameOver
    }
    #endregion

    /// <summary>
    /// Main game manager - handles game flow, scoring, modes, and state
    /// Singleton pattern for easy access from any script
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        #region Singleton
        public static GameManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        #endregion

        #region Events
        public static event Action<int> OnScoreChanged;
        public static event Action<GameState> OnGameStateChanged;
        public static event Action<float> OnTimerUpdated;
        public static event Action<GameMode> OnGameModeChanged;
        #endregion

        #region Inspector Fields
        [Header("Game Settings")]
        [SerializeField] private GameMode currentGameMode = GameMode.Training;
        [SerializeField] private float gameDuration = 60f; // Duration in seconds (0 = infinite for Training)

        [Header("Difficulty Settings")]
        [SerializeField] private float difficultyIncreaseInterval = 30f; // Increase difficulty every 30s
        [SerializeField] private bool adaptiveDifficulty = true;

        [Header("References")]
        [SerializeField] private TargetSpawner targetSpawner;
        [SerializeField] private UIManager uiManager;
        #endregion

        #region Private Fields
        private GameState currentGameState = GameState.Menu;
        private ScoreSystem scoreSystem;
        private float currentTime;
        private float difficultyTimer;
        private int currentDifficultyLevel = 1;
        private bool isGameActive = false;
        #endregion

        #region Properties
        public GameMode CurrentGameMode => currentGameMode;
        public GameState CurrentGameState => currentGameState;
        public float CurrentTime => currentTime;
        public int CurrentScore => scoreSystem?.CurrentScore ?? 0;
        public float Accuracy => scoreSystem?.Accuracy ?? 0f;
        public ScoreSystem Score => scoreSystem;
        #endregion

        #region Unity Lifecycle
        private void Start()
        {
            InitializeGame();
        }

        private void Update()
        {
            if (isGameActive && currentGameState == GameState.Playing)
            {
                UpdateGameTimer();
                UpdateDifficulty();
            }
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize game systems
        /// </summary>
        private void InitializeGame()
        {
            scoreSystem = GetComponent<ScoreSystem>();
            if (scoreSystem == null)
            {
                scoreSystem = gameObject.AddComponent<ScoreSystem>();
            }

            // Find references if not set
            if (targetSpawner == null)
                targetSpawner = FindFirstObjectByType<TargetSpawner>();

            if (uiManager == null)
                uiManager = FindFirstObjectByType<UIManager>();

            ChangeGameState(GameState.Menu);
        }
        #endregion

        #region Game Flow
        /// <summary>
        /// Start a new game with specified mode
        /// </summary>
        /// <param name="mode">Game mode to play</param>
        public void StartGame(GameMode mode)
        {
            currentGameMode = mode;
            ConfigureGameMode(mode);

            scoreSystem.ResetScore();
            currentTime = gameDuration;
            currentDifficultyLevel = 1;
            difficultyTimer = 0f;
            isGameActive = true;

            OnGameModeChanged?.Invoke(mode);
            ChangeGameState(GameState.Playing);

            // Start spawning targets
            if (targetSpawner != null)
            {
                targetSpawner.StartSpawning(mode);
            }

            Debug.Log($"[GameManager] Started {mode} mode");
        }
        public void StartTrainingMode()
        {
            StartGame(GameMode.Training);
        }

        public void StartPrecisionMode()
        {
            StartGame(GameMode.Precision);
        }

        public void StartSpeedMode()
        {
            StartGame(GameMode.Speed);
        }

        public void StartTrackingMode()
        {
            StartGame(GameMode.Tracking);
        }

        public void StartChallengeMode()
        {
            StartGame(GameMode.Challenge);
        }

        public void PauseGame()
        {
            if (currentGameState == GameState.Playing)
            {
                Time.timeScale = 0f;
                ChangeGameState(GameState.Paused);
            }
        }

        /// <summary>
        /// Resume from pause
        /// </summary>
        public void ResumeGame()
        {
            if (currentGameState == GameState.Paused)
            {
                Time.timeScale = 1f;
                ChangeGameState(GameState.Playing);
            }
        }

        /// <summary>
        /// End the current game
        /// </summary>
        public void EndGame()
        {
            isGameActive = false;
            Time.timeScale = 1f;

            // Stop spawning
            if (targetSpawner != null)
            {
                targetSpawner.StopSpawning();
            }

            // Save high score
            SaveHighScore();

            ChangeGameState(GameState.GameOver);
            Debug.Log($"[GameManager] Game Over - Final Score: {CurrentScore}");
        }

        /// <summary>
        /// Return to main menu
        /// </summary>
        public void ReturnToMenu()
        {
            Time.timeScale = 1f;
            isGameActive = false;

            if (targetSpawner != null)
            {
                targetSpawner.StopSpawning();
            }

            ChangeGameState(GameState.Menu);
        }
        #endregion

        #region Game Mode Configuration
        /// <summary>
        /// Configure game parameters based on selected mode
        /// </summary>
        private void ConfigureGameMode(GameMode mode)
        {
            switch (mode)
            {
                case GameMode.Training:
                    gameDuration = 0f; // Infinite time
                    adaptiveDifficulty = false;
                    break;

                case GameMode.Precision:
                    gameDuration = 90f; // 1.5 minutes
                    adaptiveDifficulty = true;
                    break;

                case GameMode.Speed:
                    gameDuration = 60f; // 1 minute
                    adaptiveDifficulty = true;
                    break;

                case GameMode.Tracking:
                    gameDuration = 120f; // 2 minutes
                    adaptiveDifficulty = true;
                    break;

                case GameMode.Challenge:
                    gameDuration = 180f; // 3 minutes
                    adaptiveDifficulty = true;
                    break;
            }
        }
        #endregion

        #region Timer System
        /// <summary>
        /// Update game timer each frame
        /// </summary>
        private void UpdateGameTimer()
        {
            // Training mode has no timer
            if (currentGameMode == GameMode.Training)
            {
                OnTimerUpdated?.Invoke(-1f);
                return;
            }

            currentTime -= Time.deltaTime;
            OnTimerUpdated?.Invoke(currentTime);

            if (currentTime <= 0f)
            {
                EndGame();
            }
        }
        #endregion

        #region Difficulty System
        /// <summary>
        /// Update adaptive difficulty during gameplay
        /// </summary>
        private void UpdateDifficulty()
        {
            if (!adaptiveDifficulty) return;

            difficultyTimer += Time.deltaTime;

            if (difficultyTimer >= difficultyIncreaseInterval)
            {
                difficultyTimer = 0f;
                currentDifficultyLevel++;

                // Notify spawner to increase difficulty
                if (targetSpawner != null)
                {
                    targetSpawner.IncreaseDifficulty(currentDifficultyLevel);
                }

                Debug.Log($"[GameManager] Difficulty increased to level {currentDifficultyLevel}");
            }
        }
        #endregion

        #region State Management
        /// <summary>
        /// Change the current game state
        /// </summary>
        private void ChangeGameState(GameState newState)
        {
            if (currentGameState == newState) return;

            currentGameState = newState;
            OnGameStateChanged?.Invoke(newState);

            // Update UI
            if (uiManager != null)
            {
                uiManager.OnGameStateChanged(newState);
            }

            Debug.Log($"[GameManager] Game state changed to: {newState}");
        }
        #endregion

        #region Scoring
        /// <summary>
        /// Called when player hits a target
        /// </summary>
        public void OnTargetHit(int points, bool isPerfectHit)
        {
            if (scoreSystem != null)
            {
                scoreSystem.AddScore(points, isPerfectHit);
                OnScoreChanged?.Invoke(scoreSystem.CurrentScore);
            }

            // Play audio feedback
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayHitSound(isPerfectHit);
            }
        }

        /// <summary>
        /// Called when player misses a shot
        /// </summary>
        public void OnShotMissed()
        {
            if (scoreSystem != null)
            {
                scoreSystem.RegisterMiss();
            }
        }

        /// <summary>
        /// Called when a target expires without being hit
        /// </summary>
        public void OnTargetExpired()
        {
            if (scoreSystem != null)
            {
                scoreSystem.BreakStreak();
            }
        }
        #endregion

        #region Save/Load
        /// <summary>
        /// Save high score for current game mode
        /// </summary>
        private void SaveHighScore()
        {
            string key = $"HighScore_{currentGameMode}";
            int currentHighScore = PlayerPrefs.GetInt(key, 0);

            if (CurrentScore > currentHighScore)
            {
                PlayerPrefs.SetInt(key, CurrentScore);
                PlayerPrefs.SetFloat($"HighScoreAccuracy_{currentGameMode}", Accuracy);
                PlayerPrefs.SetInt($"HighScoreStreak_{currentGameMode}", scoreSystem.MaxStreak);
                PlayerPrefs.Save();

                Debug.Log($"[GameManager] New high score for {currentGameMode}: {CurrentScore}");
            }
        }

        /// <summary>
        /// Get high score for a specific game mode
        /// </summary>
        public int GetHighScore(GameMode mode)
        {
            return PlayerPrefs.GetInt($"HighScore_{mode}", 0);
        }

        /// <summary>
        /// Get high score accuracy for a specific game mode
        /// </summary>
        public float GetHighScoreAccuracy(GameMode mode)
        {
            return PlayerPrefs.GetFloat($"HighScoreAccuracy_{mode}", 0f);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Add time to the current game (bonus time pickup)
        /// </summary>
        public void AddBonusTime(float seconds)
        {
            if (currentGameMode != GameMode.Training)
            {
                currentTime += seconds;
                Debug.Log($"[GameManager] Added {seconds}s bonus time");
            }
        }

        /// <summary>
        /// Get formatted time string
        /// </summary>
        public string GetFormattedTime()
        {
            if (currentGameMode == GameMode.Training)
                return "∞";

            int minutes = Mathf.FloorToInt(currentTime / 60f);
            int seconds = Mathf.FloorToInt(currentTime % 60f);
            return $"{minutes:00}:{seconds:00}";
        }
        #endregion
    }
}

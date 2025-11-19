using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRAimLab.UI;
using VRAimLab.Gameplay;

namespace VRAimLab.Core
{
    #region Enums
    /// Available game modes in VR Aim Lab
    public enum GameMode
    {
        Training,   // No timer, practice mode
        Precision,  // Small targets, accuracy focused
        Speed,      // Fast spawning, reaction time
        Tracking,   // Moving targets only
        Challenge   // Mix of all, hardest difficulty
    }

    /// Current state of the game
    public enum GameState
    {
        Menu,
        Playing,
        Paused,
        GameOver
    }
    #endregion

    /// Main game manager - handles game flow, scoring, modes, and state
    /// Singleton pattern for easy access from any script
    public class GameManager : MonoBehaviour
    {
        #region Singleton
        public static GameManager Instance { get; private set; }

        private void Awake()
        {
            // Garantir que só existe uma instância do GameManager (singleton)
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);  // Persistir entre cenas
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
        /// Initialize game systems
        private void InitializeGame()
        {
            // Obter ou criar o ScoreSystem
            scoreSystem = GetComponent<ScoreSystem>();
            if (scoreSystem == null)
            {
                scoreSystem = gameObject.AddComponent<ScoreSystem>();
            }

            // Encontrar referências se não foram atribuídas no inspector
            if (targetSpawner == null)
                targetSpawner = FindFirstObjectByType<TargetSpawner>();

            if (uiManager == null)
                uiManager = FindFirstObjectByType<UIManager>();

            // Começar no menu principal
            ChangeGameState(GameState.Menu);
        }
        #endregion

        #region Game Flow
        /// Start a new game with specified mode
        public void StartGame(GameMode mode)
        {
            currentGameMode = mode;
            ConfigureGameMode(mode);  // Define duração e dificuldade baseado no modo

            // Resetar estatísticas da sessão anterior
            scoreSystem.ResetScore();
            currentTime = gameDuration;
            currentDifficultyLevel = 1;
            difficultyTimer = 0f;
            isGameActive = true;

            // Notificar sistemas sobre o novo modo
            OnGameModeChanged?.Invoke(mode);
            ChangeGameState(GameState.Playing);

            // Começar a spawnar alvos
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
                Time.timeScale = 0f;  // Congelar o tempo
                ChangeGameState(GameState.Paused);
            }
        }

        /// Resume from pause
        public void ResumeGame()
        {
            if (currentGameState == GameState.Paused)
            {
                Time.timeScale = 1f;  // Retomar tempo normal
                ChangeGameState(GameState.Playing);
            }
        }

        /// End the current game
        public void EndGame()
        {
            isGameActive = false;
            Time.timeScale = 1f;  // Garantir que o tempo volta ao normal

            // Parar de spawnar alvos
            if (targetSpawner != null)
            {
                targetSpawner.StopSpawning();
            }

            // Guardar high score se batemos o anterior
            SaveHighScore();

            ChangeGameState(GameState.GameOver);
            Debug.Log($"[GameManager] Game Over - Final Score: {CurrentScore}");
        }

        /// Return to main menu
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
        /// Configure game parameters based on selected mode
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
        /// Update game timer each frame
        private void UpdateGameTimer()
        {
            // Training mode não tem timer (tempo infinito)
            if (currentGameMode == GameMode.Training)
            {
                OnTimerUpdated?.Invoke(-1f);  // -1 indica infinito para a UI
                return;
            }

            // Decrementar o tempo restante
            currentTime -= Time.deltaTime;
            OnTimerUpdated?.Invoke(currentTime);

            // Acabou o tempo, terminar o jogo
            if (currentTime <= 0f)
            {
                EndGame();
            }
        }
        #endregion

        #region Difficulty System
        /// Update adaptive difficulty during gameplay
        private void UpdateDifficulty()
        {
            // Training mode não tem progressão de dificuldade
            if (!adaptiveDifficulty) return;

            difficultyTimer += Time.deltaTime;

            // A cada 30 segundos aumentar a dificuldade
            if (difficultyTimer >= difficultyIncreaseInterval)
            {
                difficultyTimer = 0f;
                currentDifficultyLevel++;

                // Informar o spawner para aumentar dificuldade (spawn mais rápido, mais alvos)
                if (targetSpawner != null)
                {
                    targetSpawner.IncreaseDifficulty(currentDifficultyLevel);
                }

                Debug.Log($"[GameManager] Difficulty increased to level {currentDifficultyLevel}");
            }
        }
        #endregion

        #region State Management
        /// Change the current game state
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
        /// Called when player hits a target
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

        /// Called when player misses a shot
        public void OnShotMissed()
        {
            if (scoreSystem != null)
            {
                scoreSystem.RegisterMiss();
            }
        }

        /// Called when a target expires without being hit
        public void OnTargetExpired()
        {
            if (scoreSystem != null)
            {
                scoreSystem.BreakStreak();
            }
        }
        #endregion

        #region Save/Load
        /// Save high score for current game mode
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

        /// Get high score for a specific game mode
        public int GetHighScore(GameMode mode)
        {
            return PlayerPrefs.GetInt($"HighScore_{mode}", 0);
        }

        /// Get high score accuracy for a specific game mode
        public float GetHighScoreAccuracy(GameMode mode)
        {
            return PlayerPrefs.GetFloat($"HighScoreAccuracy_{mode}", 0f);
        }
        #endregion

        #region Public Methods
        /// Add time to the current game (bonus time pickup)
        public void AddBonusTime(float seconds)
        {
            if (currentGameMode != GameMode.Training)
            {
                currentTime += seconds;
                Debug.Log($"[GameManager] Added {seconds}s bonus time");
            }
        }

        /// Get formatted time string
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

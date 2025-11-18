using UnityEngine;
using TMPro;
using VRAimLab.Core;

namespace VRAimLab.UI
{
    /// <summary>
    /// UI Manager - manages all UI panels and updates for VR
    /// Handles world space canvas UI for VR compatibility
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        #region Inspector Fields
        [Header("UI Panels")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject gameplayHUDPanel;
        [SerializeField] private GameObject pauseMenuPanel;
        [SerializeField] private GameObject gameOverPanel;

        [Header("Components")]
        [SerializeField] private MenuController menuController;
        [SerializeField] private HUDController hudController;

        [Header("Canvas Settings")]
        [SerializeField] private Canvas mainCanvas;
        [SerializeField] private float canvasDistanceFromCamera = 2f;
        #endregion

        #region Private Fields
        private GameState currentState;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            // Find components if not set
            if (menuController == null)
                menuController = GetComponentInChildren<MenuController>();

            if (hudController == null)
                hudController = GetComponentInChildren<HUDController>();

            if (mainCanvas == null)
                mainCanvas = GetComponent<Canvas>();

            // Configure canvas for VR
            ConfigureCanvasForVR();
        }

        private void OnEnable()
        {
            // Subscribe to game events
            GameManager.OnScoreChanged += OnScoreChanged;
            GameManager.OnGameStateChanged += OnGameStateChanged;
            GameManager.OnTimerUpdated += OnTimerUpdated;

            // Subscribe to score system events (static)
            ScoreSystem.OnStreakChanged += OnStreakChanged;
            ScoreSystem.OnComboMultiplierChanged += OnComboChanged;
            ScoreSystem.OnRankChanged += OnRankChanged;
        }

        private void OnDisable()
        {
            // Unsubscribe from events
            GameManager.OnScoreChanged -= OnScoreChanged;
            GameManager.OnGameStateChanged -= OnGameStateChanged;
            GameManager.OnTimerUpdated -= OnTimerUpdated;

            ScoreSystem.OnStreakChanged -= OnStreakChanged;
            ScoreSystem.OnComboMultiplierChanged -= OnComboChanged;
            ScoreSystem.OnRankChanged -= OnRankChanged;
        }

        private void Start()
        {
            // Show main menu initially
            ShowPanel(mainMenuPanel);
        }
        #endregion

        #region Canvas Configuration
        /// <summary>
        /// Configure canvas for VR (World Space)
        /// </summary>
        private void ConfigureCanvasForVR()
        {
            if (mainCanvas == null) return;

            // Set to World Space for VR
            mainCanvas.renderMode = RenderMode.WorldSpace;

            // Position canvas in front of player
            Vector3 cameraForward = Camera.main != null ? Camera.main.transform.forward : Vector3.forward;
            cameraForward.y = 0; // Keep at eye level
            transform.position = Camera.main.transform.position + cameraForward.normalized * canvasDistanceFromCamera;
            transform.LookAt(Camera.main.transform);
            transform.Rotate(0, 180, 0);

            // Scale appropriately for VR
            transform.localScale = Vector3.one * 0.001f;
        }

        /// <summary>
        /// Update canvas position to face camera (call if camera moves)
        /// </summary>
        public void UpdateCanvasPosition()
        {
            if (Camera.main != null)
            {
                transform.LookAt(Camera.main.transform);
                transform.Rotate(0, 180, 0);
            }
        }
        #endregion

        #region Panel Management
        /// <summary>
        /// Show specific panel, hide others
        /// </summary>
        private void ShowPanel(GameObject panelToShow)
        {
            if (mainMenuPanel != null)
                mainMenuPanel.SetActive(panelToShow == mainMenuPanel);

            if (gameplayHUDPanel != null)
                gameplayHUDPanel.SetActive(panelToShow == gameplayHUDPanel);

            if (pauseMenuPanel != null)
                pauseMenuPanel.SetActive(panelToShow == pauseMenuPanel);

            if (gameOverPanel != null)
                gameOverPanel.SetActive(panelToShow == gameOverPanel);
        }

        /// <summary>
        /// Show main menu
        /// </summary>
        public void ShowMainMenu()
        {
            ShowPanel(mainMenuPanel);

            if (menuController != null)
                menuController.RefreshUI();
        }

        /// <summary>
        /// Show gameplay HUD
        /// </summary>
        public void ShowGameplayHUD()
        {
            ShowPanel(gameplayHUDPanel);

            if (hudController != null)
                hudController.ResetHUD();
        }

        /// <summary>
        /// Show pause menu
        /// </summary>
        public void ShowPauseMenu()
        {
            ShowPanel(pauseMenuPanel);
        }

        /// <summary>
        /// Show game over screen
        /// </summary>
        public void ShowGameOver()
        {
            ShowPanel(gameOverPanel);
            DisplayGameOverStats();
        }
        #endregion

        #region Game Event Handlers
        /// <summary>
        /// Handle game state changes
        /// </summary>
        public void OnGameStateChanged(GameState newState)
        {
            currentState = newState;

            switch (newState)
            {
                case GameState.Menu:
                    ShowMainMenu();
                    break;

                case GameState.Playing:
                    ShowGameplayHUD();
                    break;

                case GameState.Paused:
                    ShowPauseMenu();
                    break;

                case GameState.GameOver:
                    ShowGameOver();
                    break;
            }
        }

        /// <summary>
        /// Handle score changes
        /// </summary>
        private void OnScoreChanged(int newScore)
        {
            if (hudController != null)
            {
                hudController.UpdateScore(newScore);
            }
        }

        /// <summary>
        /// Handle timer updates
        /// </summary>
        private void OnTimerUpdated(float timeRemaining)
        {
            if (hudController != null)
            {
                hudController.UpdateTimer(timeRemaining);
            }
        }

        /// <summary>
        /// Handle streak changes
        /// </summary>
        private void OnStreakChanged(int streak)
        {
            if (hudController != null)
            {
                hudController.UpdateStreak(streak);
            }
        }

        /// <summary>
        /// Handle combo multiplier changes
        /// </summary>
        private void OnComboChanged(int multiplier)
        {
            if (hudController != null)
            {
                hudController.UpdateCombo(multiplier);
            }
        }

        /// <summary>
        /// Handle rank changes
        /// </summary>
        private void OnRankChanged(Rank rank)
        {
            if (hudController != null)
            {
                hudController.UpdateRank(rank);
            }
        }
        #endregion

        #region Game Over Display
        /// <summary>
        /// Display final stats on game over screen
        /// </summary>
        private void DisplayGameOverStats()
        {
            if (GameManager.Instance == null || gameOverPanel == null) return;

            ScoreSystem scoreSystem = GameManager.Instance.Score;

            // Find text elements in game over panel
            // TODO: Connect these in Unity Inspector or find by name
            TextMeshProUGUI finalScoreText = gameOverPanel.transform.Find("FinalScoreText")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI accuracyText = gameOverPanel.transform.Find("AccuracyText")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI streakText = gameOverPanel.transform.Find("MaxStreakText")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI rankText = gameOverPanel.transform.Find("RankText")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI highScoreText = gameOverPanel.transform.Find("HighScoreText")?.GetComponent<TextMeshProUGUI>();

            if (finalScoreText != null)
                finalScoreText.text = $"Score: {scoreSystem.CurrentScore}";

            if (accuracyText != null)
                accuracyText.text = $"Accuracy: {scoreSystem.GetAccuracyString()}";

            if (streakText != null)
                streakText.text = $"Max Streak: {scoreSystem.MaxStreak}";

            if (rankText != null)
            {
                rankText.text = $"Rank: {scoreSystem.GetRankString()}";
                rankText.color = scoreSystem.GetRankColor();
            }

            // Check if new high score
            int previousHighScore = GameManager.Instance.GetHighScore(GameManager.Instance.CurrentGameMode);
            if (highScoreText != null)
            {
                if (scoreSystem.IsNewHighScore(previousHighScore))
                {
                    highScoreText.text = "NEW HIGH SCORE!";
                    highScoreText.color = Color.yellow;
                }
                else
                {
                    highScoreText.text = $"High Score: {previousHighScore}";
                    highScoreText.color = Color.white;
                }
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Update HUD elements (manual update if needed)
        /// </summary>
        public void UpdateHUD()
        {
            if (GameManager.Instance == null || hudController == null) return;

            ScoreSystem score = GameManager.Instance.Score;

            hudController.UpdateScore(score.CurrentScore);
            hudController.UpdateAccuracy(score.Accuracy);
            hudController.UpdateStreak(score.CurrentStreak);
            hudController.UpdateCombo(score.ComboMultiplier);
            hudController.UpdateRank(score.CurrentRank);
            hudController.UpdateTimer(GameManager.Instance.CurrentTime);
        }

        /// <summary>
        /// Toggle pause menu
        /// </summary>
        public void TogglePause()
        {
            if (GameManager.Instance == null) return;

            if (currentState == GameState.Playing)
            {
                GameManager.Instance.PauseGame();
            }
            else if (currentState == GameState.Paused)
            {
                GameManager.Instance.ResumeGame();
            }
        }

        /// <summary>
        /// Refresh all UI elements
        /// </summary>
        public void RefreshAllUI()
        {
            if (menuController != null)
                menuController.RefreshUI();

            if (hudController != null)
                hudController.ResetHUD();

            UpdateHUD();
        }
        #endregion

        #region Button Callbacks (for Inspector)
        /// <summary>
        /// Resume game button callback
        /// </summary>
        public void OnResumeButtonPressed()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ResumeGame();
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayButtonClick();
            }
        }

        /// <summary>
        /// Return to menu button callback
        /// </summary>
        public void OnMenuButtonPressed()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ReturnToMenu();
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayButtonClick();
            }
        }

        /// <summary>
        /// Restart game button callback
        /// </summary>
        public void OnRestartButtonPressed()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartGame(GameManager.Instance.CurrentGameMode);
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayButtonClick();
            }
        }
        #endregion
    }
}

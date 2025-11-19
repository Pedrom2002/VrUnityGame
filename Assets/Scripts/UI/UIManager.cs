using UnityEngine;
using TMPro;
using VRAimLab.Core;

namespace VRAimLab.UI
{
    /// UI Manager - manages all UI panels and updates for VR
    /// Handles world space canvas UI for VR compatibility
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
        /// Configure canvas for VR (World Space)
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

        /// Update canvas position to face camera (call if camera moves)
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
        /// Show specific panel, hide others
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

        /// Show main menu
        public void ShowMainMenu()
        {
            ShowPanel(mainMenuPanel);

            if (menuController != null)
                menuController.RefreshUI();
        }

        /// Show gameplay HUD
        public void ShowGameplayHUD()
        {
            ShowPanel(gameplayHUDPanel);

            if (hudController != null)
                hudController.ResetHUD();
        }

        /// Show pause menu
        public void ShowPauseMenu()
        {
            ShowPanel(pauseMenuPanel);
        }

        /// Show game over screen
        public void ShowGameOver()
        {
            ShowPanel(gameOverPanel);
            DisplayGameOverStats();
        }
        #endregion

        #region Game Event Handlers
        /// Handle game state changes
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

        /// Handle score changes
        private void OnScoreChanged(int newScore)
        {
            if (hudController != null)
            {
                hudController.UpdateScore(newScore);
            }
        }

        /// Handle timer updates
        private void OnTimerUpdated(float timeRemaining)
        {
            if (hudController != null)
            {
                hudController.UpdateTimer(timeRemaining);
            }
        }

        /// Handle streak changes
        private void OnStreakChanged(int streak)
        {
            if (hudController != null)
            {
                hudController.UpdateStreak(streak);
            }
        }

        /// Handle combo multiplier changes
        private void OnComboChanged(int multiplier)
        {
            if (hudController != null)
            {
                hudController.UpdateCombo(multiplier);
            }
        }

        /// Handle rank changes
        private void OnRankChanged(Rank rank)
        {
            if (hudController != null)
            {
                hudController.UpdateRank(rank);
            }
        }
        #endregion

        #region Game Over Display
        /// Display final stats on game over screen
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
        /// Update HUD elements (manual update if needed)
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

        /// Toggle pause menu
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

        /// Refresh all UI elements
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
        /// Resume game button callback
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

        /// Return to menu button callback
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

        /// Restart game button callback
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

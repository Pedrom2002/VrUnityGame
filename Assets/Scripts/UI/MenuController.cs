using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VRAimLab.Core;

namespace VRAimLab.UI
{
    /// <summary>
    /// Menu Controller - handles main menu UI and game mode selection
    /// Displays high scores and settings
    /// </summary>
    public class MenuController : MonoBehaviour
    {
        #region Inspector Fields
        [Header("Mode Selection Buttons")]
        [SerializeField] private Button trainingModeButton;
        [SerializeField] private Button precisionModeButton;
        [SerializeField] private Button speedModeButton;
        [SerializeField] private Button trackingModeButton;
        [SerializeField] private Button challengeModeButton;

        [Header("Mode Info Display")]
        [SerializeField] private TextMeshProUGUI modeNameText;
        [SerializeField] private TextMeshProUGUI modeDescriptionText;
        [SerializeField] private TextMeshProUGUI highScoreText;
        [SerializeField] private TextMeshProUGUI highScoreAccuracyText;

        [Header("Settings Panel")]
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private Toggle laserSightToggle;

        [Header("Other UI")]
        [SerializeField] private Button startButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private TextMeshProUGUI versionText;
        #endregion

        #region Private Fields
        private GameMode selectedGameMode = GameMode.Training;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            SetupButtonListeners();
            SetupVolumeSliders();
        }

        private void Start()
        {
            RefreshUI();
            SelectGameMode(GameMode.Training); // Default selection

            if (versionText != null)
            {
                versionText.text = "v1.0.0 - APEX Training System";
            }
        }
        #endregion

        #region Setup
        /// <summary>
        /// Setup button click listeners
        /// </summary>
        private void SetupButtonListeners()
        {
            // Mode selection buttons
            if (trainingModeButton != null)
                trainingModeButton.onClick.AddListener(() => SelectGameMode(GameMode.Training));

            if (precisionModeButton != null)
                precisionModeButton.onClick.AddListener(() => SelectGameMode(GameMode.Precision));

            if (speedModeButton != null)
                speedModeButton.onClick.AddListener(() => SelectGameMode(GameMode.Speed));

            if (trackingModeButton != null)
                trackingModeButton.onClick.AddListener(() => SelectGameMode(GameMode.Tracking));

            if (challengeModeButton != null)
                challengeModeButton.onClick.AddListener(() => SelectGameMode(GameMode.Challenge));

            // Action buttons
            if (startButton != null)
                startButton.onClick.AddListener(OnStartButtonPressed);

            if (settingsButton != null)
                settingsButton.onClick.AddListener(OnSettingsButtonPressed);

            if (quitButton != null)
                quitButton.onClick.AddListener(OnQuitButtonPressed);
        }

        /// <summary>
        /// Setup volume sliders
        /// </summary>
        private void SetupVolumeSliders()
        {
            if (AudioManager.Instance == null) return;

            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.value = AudioManager.Instance.MasterVolume;
                masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            }

            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.value = AudioManager.Instance.MusicVolume;
                musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            }

            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.value = AudioManager.Instance.SFXVolume;
                sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
            }
        }
        #endregion

        #region Game Mode Selection
        /// <summary>
        /// Select a game mode and update UI
        /// </summary>
        public void SelectGameMode(GameMode mode)
        {
            selectedGameMode = mode;
            UpdateModeInfo(mode);
            UpdateButtonHighlights(mode);

            // Play UI sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayModeSelect();
            }
        }

        /// <summary>
        /// Update mode information display
        /// </summary>
        private void UpdateModeInfo(GameMode mode)
        {
            string modeName = "";
            string modeDescription = "";

            switch (mode)
            {
                case GameMode.Training:
                    modeName = "TRAINING MODE";
                    modeDescription = "Practice mode with no time limit.\nPerfect for warming up and improving accuracy.";
                    break;

                case GameMode.Precision:
                    modeName = "PRECISION MODE";
                    modeDescription = "Small targets, accuracy is everything.\nTest your precision and aim for the bullseye.";
                    break;

                case GameMode.Speed:
                    modeName = "SPEED MODE";
                    modeDescription = "Fast-paced action, quick reflexes required.\nHow many targets can you hit in 60 seconds?";
                    break;

                case GameMode.Tracking:
                    modeName = "TRACKING MODE";
                    modeDescription = "Moving targets only.\nPractice tracking and leading your shots.";
                    break;

                case GameMode.Challenge:
                    modeName = "CHALLENGE MODE";
                    modeDescription = "Ultimate test - all target types, increasing difficulty.\nProve you're ready for APEX status.";
                    break;
            }

            if (modeNameText != null)
                modeNameText.text = modeName;

            if (modeDescriptionText != null)
                modeDescriptionText.text = modeDescription;

            // Update high score display
            UpdateHighScoreDisplay(mode);
        }

        /// <summary>
        /// Update high score display for selected mode
        /// </summary>
        private void UpdateHighScoreDisplay(GameMode mode)
        {
            if (GameManager.Instance == null) return;

            int highScore = GameManager.Instance.GetHighScore(mode);
            float highAccuracy = GameManager.Instance.GetHighScoreAccuracy(mode);

            if (highScoreText != null)
            {
                if (highScore > 0)
                {
                    highScoreText.text = $"High Score: {highScore}";
                }
                else
                {
                    highScoreText.text = "High Score: ---";
                }
            }

            if (highScoreAccuracyText != null)
            {
                if (highScore > 0)
                {
                    highScoreAccuracyText.text = $"Best Accuracy: {highAccuracy:F1}%";
                }
                else
                {
                    highScoreAccuracyText.text = "Best Accuracy: ---";
                }
            }
        }

        /// <summary>
        /// Highlight selected mode button
        /// </summary>
        private void UpdateButtonHighlights(GameMode mode)
        {
            // Reset all buttons to normal color
            ResetButtonColor(trainingModeButton);
            ResetButtonColor(precisionModeButton);
            ResetButtonColor(speedModeButton);
            ResetButtonColor(trackingModeButton);
            ResetButtonColor(challengeModeButton);

            // Highlight selected button
            Button selectedButton = GetButtonForMode(mode);
            if (selectedButton != null)
            {
                HighlightButton(selectedButton);
            }
        }

        /// <summary>
        /// Get button for a game mode
        /// </summary>
        private Button GetButtonForMode(GameMode mode)
        {
            switch (mode)
            {
                case GameMode.Training: return trainingModeButton;
                case GameMode.Precision: return precisionModeButton;
                case GameMode.Speed: return speedModeButton;
                case GameMode.Tracking: return trackingModeButton;
                case GameMode.Challenge: return challengeModeButton;
                default: return null;
            }
        }

        /// <summary>
        /// Highlight button
        /// </summary>
        private void HighlightButton(Button button)
        {
            if (button == null) return;

            ColorBlock colors = button.colors;
            colors.normalColor = new Color(0.3f, 0.8f, 1f); // Cyan highlight
            button.colors = colors;
        }

        /// <summary>
        /// Reset button to normal color
        /// </summary>
        private void ResetButtonColor(Button button)
        {
            if (button == null) return;

            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            button.colors = colors;
        }
        #endregion

        #region Button Callbacks
        /// <summary>
        /// Start button pressed - begin selected game mode
        /// </summary>
        private void OnStartButtonPressed()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartGame(selectedGameMode);
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayButtonClick();
                AudioManager.Instance.PlayGameplayMusic();
            }
        }

        /// <summary>
        /// Settings button pressed - toggle settings panel
        /// </summary>
        private void OnSettingsButtonPressed()
        {
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(!settingsPanel.activeSelf);
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayButtonClick();
            }
        }

        /// <summary>
        /// Quit button pressed - exit application
        /// </summary>
        private void OnQuitButtonPressed()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayButtonClick();
            }

            Debug.Log("[MenuController] Quit button pressed");

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        #endregion

        #region Settings Callbacks
        /// <summary>
        /// Master volume slider changed
        /// </summary>
        private void OnMasterVolumeChanged(float value)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.MasterVolume = value;
            }
        }

        /// <summary>
        /// Music volume slider changed
        /// </summary>
        private void OnMusicVolumeChanged(float value)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.MusicVolume = value;
            }
        }

        /// <summary>
        /// SFX volume slider changed
        /// </summary>
        private void OnSFXVolumeChanged(float value)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SFXVolume = value;
            }
        }

        /// <summary>
        /// Laser sight toggle changed
        /// </summary>
        public void OnLaserSightToggled(bool enabled)
        {
            // TODO: Update all weapon controllers
            PlayerPrefs.SetInt("LaserSight", enabled ? 1 : 0);
            PlayerPrefs.Save();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Refresh UI elements
        /// </summary>
        public void RefreshUI()
        {
            UpdateModeInfo(selectedGameMode);
            UpdateButtonHighlights(selectedGameMode);

            // Play menu music
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayMenuMusic();
            }
        }

        /// <summary>
        /// Get currently selected game mode
        /// </summary>
        public GameMode GetSelectedGameMode()
        {
            return selectedGameMode;
        }
        #endregion
    }
}

using UnityEngine;
using TMPro;
using UnityEngine.UI;
using VRAimLab.Core;

namespace VRAimLab.UI
{
    /// <summary>
    /// HUD Controller - manages in-game HUD display
    /// Shows score, timer, accuracy, streak, combo, ammo, rank
    /// </summary>
    public class HUDController : MonoBehaviour
    {
        #region Inspector Fields
        [Header("Score Display")]
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI accuracyText;

        [Header("Timer Display")]
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private Image timerFillImage;

        [Header("Streak & Combo")]
        [SerializeField] private TextMeshProUGUI streakText;
        [SerializeField] private TextMeshProUGUI comboText;
        [SerializeField] private GameObject streakBanner; // Shows on milestones
        [SerializeField] private TextMeshProUGUI streakBannerText;

        [Header("Ammo Display")]
        [SerializeField] private TextMeshProUGUI ammoText;
        [SerializeField] private Image ammoFillImage;

        [Header("Rank Display")]
        [SerializeField] private TextMeshProUGUI rankText;
        [SerializeField] private Image rankIcon;

        [Header("Game Mode Display")]
        [SerializeField] private TextMeshProUGUI gameModeText;

        [Header("Animation Settings")]
        [SerializeField] private bool enableAnimations = true;
        [SerializeField] private float scorePopDuration = 0.3f;
        [SerializeField] private float streakBannerDuration = 2f;
        #endregion

        #region Private Fields
        private int currentDisplayedScore = 0;
        private float scoreAnimationTimer = 0f;
        private int targetScore = 0;

        private Coroutine streakBannerCoroutine;
        #endregion

        #region Unity Lifecycle
        private void Update()
        {
            if (enableAnimations)
            {
                AnimateScoreUpdate();
            }
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Reset HUD to initial state
        /// </summary>
        public void ResetHUD()
        {
            UpdateScore(0);
            UpdateAccuracy(0f);
            UpdateTimer(0f);
            UpdateStreak(0);
            UpdateCombo(1);
            UpdateRank(Rank.Bronze);

            currentDisplayedScore = 0;
            targetScore = 0;

            if (streakBanner != null)
            {
                streakBanner.SetActive(false);
            }

            // Update game mode display
            if (GameManager.Instance != null && gameModeText != null)
            {
                gameModeText.text = GameManager.Instance.CurrentGameMode.ToString().ToUpper();
            }
        }
        #endregion

        #region Score Updates
        /// <summary>
        /// Update score display
        /// </summary>
        public void UpdateScore(int score)
        {
            if (enableAnimations)
            {
                targetScore = score;
                scoreAnimationTimer = 0f;
            }
            else
            {
                currentDisplayedScore = score;
                UpdateScoreText(score);
            }
        }

        /// <summary>
        /// Animate score counting up
        /// </summary>
        private void AnimateScoreUpdate()
        {
            if (currentDisplayedScore == targetScore) return;

            scoreAnimationTimer += Time.deltaTime;
            float t = Mathf.Clamp01(scoreAnimationTimer / scorePopDuration);

            currentDisplayedScore = Mathf.RoundToInt(Mathf.Lerp(currentDisplayedScore, targetScore, t));
            UpdateScoreText(currentDisplayedScore);

            if (t >= 1f)
            {
                currentDisplayedScore = targetScore;
            }
        }

        /// <summary>
        /// Update score text element
        /// </summary>
        private void UpdateScoreText(int score)
        {
            if (scoreText != null)
            {
                scoreText.text = $"{score:N0}";
            }
        }

        /// <summary>
        /// Update accuracy display
        /// </summary>
        public void UpdateAccuracy(float accuracy)
        {
            if (accuracyText != null)
            {
                accuracyText.text = $"{accuracy:F1}%";

                // Color code accuracy
                if (accuracy >= 90f)
                    accuracyText.color = Color.green;
                else if (accuracy >= 70f)
                    accuracyText.color = Color.yellow;
                else if (accuracy >= 50f)
                    accuracyText.color = new Color(1f, 0.6f, 0f); // Orange
                else
                    accuracyText.color = Color.red;
            }
        }
        #endregion

        #region Timer Updates
        /// <summary>
        /// Update timer display
        /// </summary>
        public void UpdateTimer(float timeRemaining)
        {
            if (timerText != null)
            {
                // Training mode shows infinity
                if (timeRemaining < 0f)
                {
                    timerText.text = "∞";
                    if (timerFillImage != null)
                        timerFillImage.fillAmount = 1f;
                    return;
                }

                int minutes = Mathf.FloorToInt(timeRemaining / 60f);
                int seconds = Mathf.FloorToInt(timeRemaining % 60f);
                timerText.text = $"{minutes:00}:{seconds:00}";

                // Color warning when time is low
                if (timeRemaining <= 10f)
                {
                    timerText.color = Color.red;
                }
                else if (timeRemaining <= 30f)
                {
                    timerText.color = Color.yellow;
                }
                else
                {
                    timerText.color = Color.white;
                }
            }

            // Update timer fill (if using circular timer)
            if (timerFillImage != null && GameManager.Instance != null)
            {
                float maxTime = 180f; // Adjust based on game mode
                switch (GameManager.Instance.CurrentGameMode)
                {
                    case GameMode.Speed:
                        maxTime = 60f;
                        break;
                    case GameMode.Precision:
                        maxTime = 90f;
                        break;
                    case GameMode.Tracking:
                        maxTime = 120f;
                        break;
                    case GameMode.Challenge:
                        maxTime = 180f;
                        break;
                }

                timerFillImage.fillAmount = timeRemaining / maxTime;
            }
        }
        #endregion

        #region Streak & Combo Updates
        /// <summary>
        /// Update streak display
        /// </summary>
        public void UpdateStreak(int streak)
        {
            if (streakText != null)
            {
                streakText.text = $"Streak: {streak}";

                // Show banner on milestone streaks
                if (streak > 0 && streak % 10 == 0)
                {
                    ShowStreakBanner(streak);
                }
            }
        }

        /// <summary>
        /// Update combo multiplier display
        /// </summary>
        public void UpdateCombo(int multiplier)
        {
            if (comboText != null)
            {
                if (multiplier > 1)
                {
                    comboText.text = $"x{multiplier} COMBO!";
                    comboText.color = GetComboColor(multiplier);
                    comboText.gameObject.SetActive(true);
                }
                else
                {
                    comboText.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Get color for combo multiplier
        /// </summary>
        private Color GetComboColor(int multiplier)
        {
            switch (multiplier)
            {
                case 2: return Color.yellow;
                case 3: return new Color(1f, 0.6f, 0f); // Orange
                case 4: return new Color(1f, 0.3f, 0f); // Red-orange
                case 5: return Color.red;
                default: return Color.white;
            }
        }

        /// <summary>
        /// Show streak milestone banner
        /// </summary>
        private void ShowStreakBanner(int streak)
        {
            if (streakBanner == null) return;

            // Stop previous banner animation
            if (streakBannerCoroutine != null)
            {
                StopCoroutine(streakBannerCoroutine);
            }

            streakBannerCoroutine = StartCoroutine(ShowStreakBannerCoroutine(streak));
        }

        /// <summary>
        /// Streak banner animation coroutine
        /// </summary>
        private System.Collections.IEnumerator ShowStreakBannerCoroutine(int streak)
        {
            if (streakBannerText != null)
            {
                streakBannerText.text = $"{streak} HIT STREAK!";
            }

            streakBanner.SetActive(true);

            // Scale animation
            if (enableAnimations)
            {
                streakBanner.transform.localScale = Vector3.zero;
                float elapsed = 0f;
                float animDuration = 0.3f;

                while (elapsed < animDuration)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / animDuration;
                    streakBanner.transform.localScale = Vector3.one * Mathf.Lerp(0f, 1.2f, t);
                    yield return null;
                }

                streakBanner.transform.localScale = Vector3.one;
            }

            yield return new WaitForSeconds(streakBannerDuration);

            // Fade out
            if (enableAnimations)
            {
                CanvasGroup cg = streakBanner.GetComponent<CanvasGroup>();
                if (cg == null) cg = streakBanner.AddComponent<CanvasGroup>();

                float elapsed = 0f;
                float fadeDuration = 0.5f;

                while (elapsed < fadeDuration)
                {
                    elapsed += Time.deltaTime;
                    cg.alpha = 1f - (elapsed / fadeDuration);
                    yield return null;
                }

                cg.alpha = 1f;
            }

            streakBanner.SetActive(false);
        }
        #endregion

        #region Ammo Updates
        /// <summary>
        /// Update ammo display
        /// </summary>
        public void UpdateAmmo(int current, int max)
        {
            if (ammoText != null)
            {
                ammoText.text = $"{current}/{max}";

                // Warning color when low
                if (current == 0)
                    ammoText.color = Color.red;
                else if (current <= max * 0.3f)
                    ammoText.color = Color.yellow;
                else
                    ammoText.color = Color.white;
            }

            if (ammoFillImage != null)
            {
                ammoFillImage.fillAmount = (float)current / max;

                // Change fill color based on amount
                if (current == 0)
                    ammoFillImage.color = Color.red;
                else if (current <= max * 0.3f)
                    ammoFillImage.color = Color.yellow;
                else
                    ammoFillImage.color = Color.cyan;
            }
        }

        /// <summary>
        /// Update ammo with percentage
        /// </summary>
        public void UpdateAmmoPercentage(float percentage)
        {
            if (ammoFillImage != null)
            {
                ammoFillImage.fillAmount = percentage;
            }
        }
        #endregion

        #region Rank Updates
        /// <summary>
        /// Update rank display
        /// </summary>
        public void UpdateRank(Rank rank)
        {
            if (rankText != null)
            {
                rankText.text = rank.ToString().ToUpper();

                // Get rank color from score system
                if (GameManager.Instance != null && GameManager.Instance.Score != null)
                {
                    rankText.color = GameManager.Instance.Score.GetRankColor();
                }
            }

            // TODO: Update rank icon sprite based on rank
            if (rankIcon != null)
            {
                // Set appropriate sprite for rank
            }
        }
        #endregion

        #region Public Utility Methods
        /// <summary>
        /// Show/hide HUD
        /// </summary>
        public void SetHUDVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        /// <summary>
        /// Set HUD opacity
        /// </summary>
        public void SetHUDOpacity(float alpha)
        {
            CanvasGroup cg = GetComponent<CanvasGroup>();
            if (cg == null) cg = gameObject.AddComponent<CanvasGroup>();
            cg.alpha = Mathf.Clamp01(alpha);
        }
        #endregion
    }
}

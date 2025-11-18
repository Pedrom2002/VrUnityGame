using System;
using UnityEngine;

namespace VRAimLab.Core
{
    /// <summary>
    /// Ranking system based on performance
    /// </summary>
    public enum Rank
    {
        Bronze,
        Silver,
        Gold,
        Platinum,
        Diamond
    }

    /// <summary>
    /// Score system - handles scoring, accuracy, streaks, combos, and rankings
    /// Attached to GameManager
    /// </summary>
    public class ScoreSystem : MonoBehaviour
    {
        #region Events
        public static event Action<int> OnStreakChanged;
        public static event Action<int> OnComboMultiplierChanged;
        public static event Action<Rank> OnRankChanged;
        #endregion

        #region Score Data
        [Header("Current Session Stats")]
        [SerializeField] private int currentScore = 0;
        [SerializeField] private int totalShots = 0;
        [SerializeField] private int successfulHits = 0;
        [SerializeField] private int currentStreak = 0;
        [SerializeField] private int maxStreak = 0;
        [SerializeField] private int comboMultiplier = 1;
        [SerializeField] private Rank currentRank = Rank.Bronze;

        [Header("Scoring Settings")]
        [SerializeField] private int basePoints = 100;
        [SerializeField] private int perfectHitBonus = 50;
        [SerializeField] private int streakThreshold = 5; // Hits needed for combo
        [SerializeField] private int maxComboMultiplier = 5;
        #endregion

        #region Properties
        public int CurrentScore => currentScore;
        public int TotalShots => totalShots;
        public int SuccessfulHits => successfulHits;
        public int CurrentStreak => currentStreak;
        public int MaxStreak => maxStreak;
        public int ComboMultiplier => comboMultiplier;
        public Rank CurrentRank => currentRank;

        /// <summary>
        /// Calculate accuracy percentage (0-100)
        /// </summary>
        public float Accuracy
        {
            get
            {
                if (totalShots == 0) return 0f;
                return ((float)successfulHits / totalShots) * 100f;
            }
        }

        /// <summary>
        /// Get accuracy as ratio (0-1)
        /// </summary>
        public float AccuracyRatio
        {
            get
            {
                if (totalShots == 0) return 0f;
                return (float)successfulHits / totalShots;
            }
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Reset all score data for new game
        /// </summary>
        public void ResetScore()
        {
            currentScore = 0;
            totalShots = 0;
            successfulHits = 0;
            currentStreak = 0;
            maxStreak = 0;
            comboMultiplier = 1;
            currentRank = Rank.Bronze;

            OnStreakChanged?.Invoke(0);
            OnComboMultiplierChanged?.Invoke(1);
            OnRankChanged?.Invoke(Rank.Bronze);

            Debug.Log("[ScoreSystem] Score reset");
        }
        #endregion

        #region Scoring Methods
        /// <summary>
        /// Add score when target is hit
        /// </summary>
        /// <param name="baseScore">Base points for this target</param>
        /// <param name="isPerfectHit">Was this a perfect/headshot hit?</param>
        public void AddScore(int baseScore, bool isPerfectHit = false)
        {
            successfulHits++;
            currentStreak++;

            // Update max streak
            if (currentStreak > maxStreak)
            {
                maxStreak = currentStreak;
            }

            // Calculate points
            int points = baseScore;

            // Perfect hit bonus
            if (isPerfectHit)
            {
                points += perfectHitBonus;
            }

            // Update combo multiplier based on streak
            UpdateComboMultiplier();

            // Apply combo multiplier
            points *= comboMultiplier;

            // Add to total score
            currentScore += points;

            // Update rank
            UpdateRank();

            // Emit events
            OnStreakChanged?.Invoke(currentStreak);

            Debug.Log($"[ScoreSystem] +{points} points (Streak: {currentStreak}, Combo: x{comboMultiplier})");

            // Play combo sound for milestones
            if (currentStreak % streakThreshold == 0 && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayComboSound();
            }
        }

        /// <summary>
        /// Register a missed shot (affects accuracy)
        /// </summary>
        public void RegisterMiss()
        {
            totalShots++;
            // Don't break streak on miss, only on target expiry
        }

        /// <summary>
        /// Register a shot fired (for accuracy tracking)
        /// </summary>
        public void RegisterShot()
        {
            totalShots++;
        }

        /// <summary>
        /// Break current streak (when target expires)
        /// </summary>
        public void BreakStreak()
        {
            if (currentStreak > 0)
            {
                Debug.Log($"[ScoreSystem] Streak broken at {currentStreak}");

                // Play streak lost sound
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayStreakLostSound();
                }
            }

            currentStreak = 0;
            comboMultiplier = 1;

            OnStreakChanged?.Invoke(0);
            OnComboMultiplierChanged?.Invoke(1);
        }
        #endregion

        #region Combo System
        /// <summary>
        /// Update combo multiplier based on current streak
        /// </summary>
        private void UpdateComboMultiplier()
        {
            int newMultiplier = 1 + (currentStreak / streakThreshold);
            newMultiplier = Mathf.Min(newMultiplier, maxComboMultiplier);

            if (newMultiplier != comboMultiplier)
            {
                comboMultiplier = newMultiplier;
                OnComboMultiplierChanged?.Invoke(comboMultiplier);

                Debug.Log($"[ScoreSystem] Combo multiplier: x{comboMultiplier}");
            }
        }
        #endregion

        #region Ranking System
        /// <summary>
        /// Update player rank based on current score and accuracy
        /// </summary>
        private void UpdateRank()
        {
            Rank newRank = CalculateRank();

            if (newRank != currentRank)
            {
                currentRank = newRank;
                OnRankChanged?.Invoke(newRank);
                Debug.Log($"[ScoreSystem] Rank up! Now {newRank}");
            }
        }

        /// <summary>
        /// Calculate rank based on score and accuracy
        /// </summary>
        private Rank CalculateRank()
        {
            float acc = AccuracyRatio;

            // Diamond: 5000+ points, 90%+ accuracy
            if (currentScore >= 5000 && acc >= 0.9f)
                return Rank.Diamond;

            // Platinum: 3000+ points, 80%+ accuracy
            if (currentScore >= 3000 && acc >= 0.8f)
                return Rank.Platinum;

            // Gold: 1500+ points, 70%+ accuracy
            if (currentScore >= 1500 && acc >= 0.7f)
                return Rank.Gold;

            // Silver: 500+ points, 60%+ accuracy
            if (currentScore >= 500 && acc >= 0.6f)
                return Rank.Silver;

            // Bronze: default
            return Rank.Bronze;
        }

        /// <summary>
        /// Get rank as string
        /// </summary>
        public string GetRankString()
        {
            return currentRank.ToString();
        }

        /// <summary>
        /// Get rank color for UI
        /// </summary>
        public Color GetRankColor()
        {
            switch (currentRank)
            {
                case Rank.Bronze:
                    return new Color(0.8f, 0.5f, 0.2f); // Bronze
                case Rank.Silver:
                    return new Color(0.75f, 0.75f, 0.75f); // Silver
                case Rank.Gold:
                    return new Color(1f, 0.84f, 0f); // Gold
                case Rank.Platinum:
                    return new Color(0.9f, 1f, 1f); // Platinum
                case Rank.Diamond:
                    return new Color(0.7f, 0.9f, 1f); // Diamond
                default:
                    return Color.white;
            }
        }
        #endregion

        #region Bonus Points
        /// <summary>
        /// Add bonus points (e.g., from time bonus, special targets)
        /// </summary>
        public void AddBonusPoints(int points, string reason = "")
        {
            currentScore += points;
            UpdateRank();

            if (!string.IsNullOrEmpty(reason))
            {
                Debug.Log($"[ScoreSystem] Bonus +{points} points: {reason}");
            }
        }
        #endregion

        #region Statistics
        /// <summary>
        /// Get formatted accuracy string
        /// </summary>
        public string GetAccuracyString()
        {
            return $"{Accuracy:F1}%";
        }

        /// <summary>
        /// Get score statistics as formatted string
        /// </summary>
        public string GetStatsString()
        {
            return $"Score: {currentScore}\n" +
                   $"Accuracy: {GetAccuracyString()}\n" +
                   $"Max Streak: {maxStreak}\n" +
                   $"Rank: {currentRank}";
        }

        /// <summary>
        /// Check if current performance beats a high score
        /// </summary>
        public bool IsNewHighScore(int previousHighScore)
        {
            return currentScore > previousHighScore;
        }
        #endregion

        #region Debug
        private void OnValidate()
        {
            // Clamp values in editor
            basePoints = Mathf.Max(1, basePoints);
            perfectHitBonus = Mathf.Max(0, perfectHitBonus);
            streakThreshold = Mathf.Max(1, streakThreshold);
            maxComboMultiplier = Mathf.Max(1, maxComboMultiplier);
        }
        #endregion
    }
}

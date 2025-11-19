using System;
using UnityEngine;

namespace VRAimLab.Core
{
    /// Ranking system based on performance
    public enum Rank
    {
        Bronze,
        Silver,
        Gold,
        Platinum,
        Diamond
    }

    /// Score system - handles scoring, accuracy, streaks, combos, and rankings
    /// Attached to GameManager
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

        /// Calculate accuracy percentage (0-100)
        public float Accuracy
        {
            get
            {
                if (totalShots == 0) return 0f;
                return ((float)successfulHits / totalShots) * 100f;
            }
        }

        /// Get accuracy as ratio (0-1)
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
        /// Reset all score data for new game
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
        /// Add score when target is hit
        public void AddScore(int baseScore, bool isPerfectHit = false)
        {
            // Incrementar contadores de acertos
            successfulHits++;
            currentStreak++;

            // Guardar a melhor streak da sessão
            if (currentStreak > maxStreak)
            {
                maxStreak = currentStreak;
            }

            // Começar com os pontos base do alvo
            int points = baseScore;

            // Adicionar bónus se foi bullseye
            if (isPerfectHit)
            {
                points += perfectHitBonus;
            }

            // Calcular multiplicador de combo baseado na streak atual
            UpdateComboMultiplier();

            // Multiplicar pontos pelo combo (x1, x2, x3, x4, x5)
            points *= comboMultiplier;

            // Adicionar ao score total
            currentScore += points;

            // Verificar se subimos de rank
            UpdateRank();

            // Notificar UI da mudança de streak
            OnStreakChanged?.Invoke(currentStreak);

            Debug.Log($"[ScoreSystem] +{points} points (Streak: {currentStreak}, Combo: x{comboMultiplier})");

            // Tocar som de combo a cada 5 hits (milestone)
            if (currentStreak % streakThreshold == 0 && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayComboSound();
            }
        }

        /// Register a missed shot (affects accuracy)
        public void RegisterMiss()
        {
            totalShots++;
            // Nota: falhar um tiro não quebra a streak, só expirar alvo
        }

        /// Register a shot fired (for accuracy tracking)
        public void RegisterShot()
        {
            totalShots++;
        }

        /// Break current streak (when target expires)
        public void BreakStreak()
        {
            if (currentStreak > 0)
            {
                Debug.Log($"[ScoreSystem] Streak broken at {currentStreak}");

                // Tocar som de perda de streak
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayStreakLostSound();
                }
            }

            // Resetar streak e combo para valores base
            currentStreak = 0;
            comboMultiplier = 1;

            // Atualizar UI
            OnStreakChanged?.Invoke(0);
            OnComboMultiplierChanged?.Invoke(1);
        }
        #endregion

        #region Combo System
        /// Update combo multiplier based on current streak
        private void UpdateComboMultiplier()
        {
            // Fórmula: 1 + (streak / 5)
            // Exemplo: 7 hits = 1 + (7/5) = 1 + 1 = x2
            int newMultiplier = 1 + (currentStreak / streakThreshold);

            // Limitar ao máximo de x5
            newMultiplier = Mathf.Min(newMultiplier, maxComboMultiplier);

            // Só notificar se o multiplicador mudou
            if (newMultiplier != comboMultiplier)
            {
                comboMultiplier = newMultiplier;
                OnComboMultiplierChanged?.Invoke(comboMultiplier);

                Debug.Log($"[ScoreSystem] Combo multiplier: x{comboMultiplier}");
            }
        }
        #endregion

        #region Ranking System
        /// Update player rank based on current score and accuracy
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

        /// Calculate rank based on score and accuracy
        private Rank CalculateRank()
        {
            float acc = AccuracyRatio;

            // Diamond: 5000+ pontos E 90%+ accuracy (elite)
            if (currentScore >= 5000 && acc >= 0.9f)
                return Rank.Diamond;

            // Platinum: 3000+ pontos E 80%+ accuracy
            if (currentScore >= 3000 && acc >= 0.8f)
                return Rank.Platinum;

            // Gold: 1500+ pontos E 70%+ accuracy
            if (currentScore >= 1500 && acc >= 0.7f)
                return Rank.Gold;

            // Silver: 500+ pontos E 60%+ accuracy
            if (currentScore >= 500 && acc >= 0.6f)
                return Rank.Silver;

            // Bronze: rank inicial (qualquer score/accuracy)
            return Rank.Bronze;
        }

        /// Get rank as string
        public string GetRankString()
        {
            return currentRank.ToString();
        }

        /// Get rank color for UI
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
        /// Add bonus points (e.g., from time bonus, special targets)
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
        /// Get formatted accuracy string
        public string GetAccuracyString()
        {
            return $"{Accuracy:F1}%";
        }

        /// Get score statistics as formatted string
        public string GetStatsString()
        {
            return $"Score: {currentScore}\n" +
                   $"Accuracy: {GetAccuracyString()}\n" +
                   $"Max Streak: {maxStreak}\n" +
                   $"Rank: {currentRank}";
        }

        /// Check if current performance beats a high score
        public bool IsNewHighScore(int previousHighScore)
        {
            return currentScore > previousHighScore;
        }
        #endregion

        #region Debug
        private void OnValidate()
        {
            // Validar valores no inspector para evitar bugs
            basePoints = Mathf.Max(1, basePoints);
            perfectHitBonus = Mathf.Max(0, perfectHitBonus);
            streakThreshold = Mathf.Max(1, streakThreshold);
            maxComboMultiplier = Mathf.Max(1, maxComboMultiplier);
        }
        #endregion
    }
}

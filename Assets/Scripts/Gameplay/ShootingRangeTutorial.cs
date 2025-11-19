using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using VRAimLab.Core;

namespace VRAimLab.Gameplay
{
    /// Shooting Range Tutorial - Step-by-step introduction to VR Aim Lab
    /// Guides player through weapon pickup, shooting, and basic mechanics
    public class ShootingRangeTutorial : MonoBehaviour
    {
        #region Tutorial Steps
        public enum TutorialStep
        {
            Welcome,            // Welcome message
            LookAround,         // Get familiar with VR
            PickupWeapon,       // Grab the weapon
            AimWithLaser,       // Learn to aim with laser sight
            ShootFirstTarget,   // Shoot first target
            ReloadWeapon,       // Learn to reload
            HitMultipleTargets, // Hit 5 targets
            PerfectShots,       // Get 3 bullseye hits
            MovingTargets,      // Hit 3 moving targets
            Completed           // Tutorial complete
        }
        #endregion

        #region Inspector Fields
        [Header("Tutorial Settings")]
        [SerializeField] private bool autoStart = true;
        [SerializeField] private float stepDelay = 1f; // Delay entre steps

        [Header("Tutorial Targets")]
        [SerializeField] private Transform[] staticTargetPositions;
        [SerializeField] private Transform[] movingTargetPositions;
        [SerializeField] private GameObject basicTargetPrefab;
        [SerializeField] private GameObject movingTargetPrefab;
        [SerializeField] private GameObject precisionTargetPrefab;

        [Header("Weapon Setup")]
        [SerializeField] private Transform weaponSpawnPoint;
        [SerializeField] private GameObject weaponPrefab;
        [SerializeField] private float weaponRespawnDelay = 3f;

        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI instructionText;
        [SerializeField] private TextMeshProUGUI objectiveText;
        [SerializeField] private TextMeshProUGUI progressText;
        [SerializeField] private GameObject tutorialPanel;
        [SerializeField] private GameObject completionPanel;

        [Header("Audio")]
        [SerializeField] private bool playVoiceLines = true;
        [SerializeField] private AudioClip stepCompleteSound;
        [SerializeField] private AudioClip tutorialCompleteSound;
        #endregion

        #region Private Fields
        private TutorialStep currentStep = TutorialStep.Welcome;
        private bool stepCompleted = false;
        private int targetHits = 0;
        private int perfectHits = 0;
        private int movingTargetHits = 0;
        private GameObject currentWeapon;
        private VRWeaponController weaponController;
        private List<GameObject> activeTargets = new List<GameObject>();
        private bool tutorialActive = false;
        #endregion

        #region Properties
        public TutorialStep CurrentStep => currentStep;
        public bool IsActive => tutorialActive;
        public bool IsCompleted => currentStep == TutorialStep.Completed;
        #endregion

        #region Unity Lifecycle
        private void Start()
        {
            if (autoStart)
            {
                StartTutorial();
            }
            else
            {
                if (tutorialPanel != null)
                    tutorialPanel.SetActive(false);
            }

            if (completionPanel != null)
                completionPanel.SetActive(false);

            // Subscribe to target events
            Target.OnTargetHit += OnTargetHit;
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            Target.OnTargetHit -= OnTargetHit;
        }

        private void Update()
        {
            if (!tutorialActive) return;

            // Check step completion conditions
            CheckStepCompletion();

            // Update progress display
            UpdateProgressDisplay();
        }
        #endregion

        #region Tutorial Control
        /// Start the tutorial
        public void StartTutorial()
        {
            tutorialActive = true;
            currentStep = TutorialStep.Welcome;
            stepCompleted = false;

            if (tutorialPanel != null)
                tutorialPanel.SetActive(true);

            Debug.Log("[Tutorial] Starting Shooting Range Tutorial");

            // Start with welcome message
            StartCoroutine(BeginTutorialSequence());
        }

        /// Begin tutorial sequence
        private IEnumerator BeginTutorialSequence()
        {
            yield return new WaitForSeconds(1f);

            // Show welcome message
            ShowStep(TutorialStep.Welcome);

            // ARIA welcome voice
            if (AudioManager.Instance != null && playVoiceLines)
            {
                AudioManager.Instance.PlayWelcomeVoice();
            }
        }

        /// Move to next tutorial step
        private void NextStep()
        {
            // Play step complete sound
            if (stepCompleteSound != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayButtonClick();
            }

            // Mark current step as completed
            stepCompleted = false;

            // Move to next step
            TutorialStep nextStep = currentStep + 1;

            if (nextStep > TutorialStep.Completed)
            {
                nextStep = TutorialStep.Completed;
            }

            currentStep = nextStep;

            Debug.Log($"[Tutorial] Moving to step: {currentStep}");

            // Delay before showing next step
            StartCoroutine(ShowNextStepDelayed(stepDelay));
        }

        /// Show next step with delay
        private IEnumerator ShowNextStepDelayed(float delay)
        {
            yield return new WaitForSeconds(delay);
            ShowStep(currentStep);
        }

        /// Show specific tutorial step
        private void ShowStep(TutorialStep step)
        {
            currentStep = step;
            stepCompleted = false;

            // Update UI
            UpdateInstructionText();

            // Setup step-specific elements
            SetupStepElements();

            Debug.Log($"[Tutorial] Showing step: {step}");
        }

        /// Complete current step and move to next
        private void CompleteStep()
        {
            if (stepCompleted) return;

            stepCompleted = true;
            Debug.Log($"[Tutorial] Step completed: {currentStep}");

            // Clear active targets
            ClearActiveTargets();

            // Check if tutorial is complete
            if (currentStep == TutorialStep.Completed)
            {
                CompleteTutorial();
            }
            else
            {
                NextStep();
            }
        }
        #endregion

        #region Step Management
        /// Update instruction text based on current step
        private void UpdateInstructionText()
        {
            if (instructionText == null) return;

            string instruction = GetInstructionForStep(currentStep);
            instructionText.text = instruction;
        }

        /// Get instruction text for step
        private string GetInstructionForStep(TutorialStep step)
        {
            switch (step)
            {
                case TutorialStep.Welcome:
                    return "WELCOME TO APEX TRAINING SYSTEM\n\nPrepare for neural precision training.";

                case TutorialStep.LookAround:
                    return "ORIENTATION\n\nLook around to familiarize yourself with the Training Hall.\nMove your head naturally.";

                case TutorialStep.PickupWeapon:
                    return "WEAPON ACQUISITION\n\nLocate the Pulse Pistol and grab it.\nVR: Hold GRIP button while touching weapon\nDesktop: Press E when near weapon";

                case TutorialStep.AimWithLaser:
                    return "TARGETING SYSTEMS\n\nNotice the red laser sight.\nPoint the weapon at targets to aim.\nThe laser shows exactly where you'll hit.";

                case TutorialStep.ShootFirstTarget:
                    return "ENGAGE TARGET\n\nPull the TRIGGER to fire.\nDesktop: Left Mouse Click\nHit the target in front of you.";

                case TutorialStep.ReloadWeapon:
                    return "RELOAD PROCEDURE\n\nYour weapon needs ammunition.\nVR: Press GRIP button to reload\nDesktop: Press R to reload";

                case TutorialStep.HitMultipleTargets:
                    return "SUSTAINED FIRE\n\nHit 5 targets to continue.\nFocus on accuracy.";

                case TutorialStep.PerfectShots:
                    return "PRECISION TRAINING\n\nHit the BULLSEYE (center) on 3 targets.\nTake your time, aim carefully.";

                case TutorialStep.MovingTargets:
                    return "TRACKING DRILL\n\nHit 3 moving targets.\nLead your shots - aim where the target will be.";

                case TutorialStep.Completed:
                    return "TUTORIAL COMPLETE\n\nYou're ready for APEX training.\nProceeding to main menu...";

                default:
                    return "APEX TRAINING SYSTEM";
            }
        }

        /// Setup elements specific to current step
        private void SetupStepElements()
        {
            switch (currentStep)
            {
                case TutorialStep.Welcome:
                    // Just show message
                    StartCoroutine(AutoCompleteAfterDelay(3f));
                    break;

                case TutorialStep.LookAround:
                    // Auto-complete after 5 seconds
                    StartCoroutine(AutoCompleteAfterDelay(5f));
                    break;

                case TutorialStep.PickupWeapon:
                    // Spawn weapon if not present
                    SpawnWeapon();
                    break;

                case TutorialStep.AimWithLaser:
                    // Auto-complete after showing laser
                    StartCoroutine(AutoCompleteAfterDelay(4f));
                    break;

                case TutorialStep.ShootFirstTarget:
                    // Spawn one static target
                    SpawnStaticTarget(GetTargetPosition(0));
                    break;

                case TutorialStep.ReloadWeapon:
                    // Weapon should be empty, wait for reload
                    if (weaponController != null)
                    {
                        // Empty the weapon (reflection)
                        var ammoField = typeof(VRWeaponController).GetField("currentAmmo",
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        if (ammoField != null)
                        {
                            ammoField.SetValue(weaponController, 0);
                        }
                    }
                    break;

                case TutorialStep.HitMultipleTargets:
                    // Reset counter
                    targetHits = 0;
                    // Spawn 5 targets
                    for (int i = 0; i < 5; i++)
                    {
                        SpawnStaticTarget(GetTargetPosition(i));
                    }
                    break;

                case TutorialStep.PerfectShots:
                    // Reset counter
                    perfectHits = 0;
                    // Spawn 3 precision targets
                    for (int i = 0; i < 3; i++)
                    {
                        SpawnPrecisionTarget(GetTargetPosition(i));
                    }
                    break;

                case TutorialStep.MovingTargets:
                    // Reset counter
                    movingTargetHits = 0;
                    // Spawn 3 moving targets
                    for (int i = 0; i < 3; i++)
                    {
                        SpawnMovingTarget(GetMovingTargetPosition(i));
                    }
                    break;

                case TutorialStep.Completed:
                    // Show completion screen
                    ShowCompletionScreen();
                    break;
            }
        }

        /// Auto-complete step after delay
        private IEnumerator AutoCompleteAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            CompleteStep();
        }

        /// Check if current step is completed
        private void CheckStepCompletion()
        {
            if (stepCompleted) return;

            switch (currentStep)
            {
                case TutorialStep.PickupWeapon:
                    // Check if weapon is grabbed
                    if (weaponController != null)
                    {
                        // Use reflection to check isGrabbed
                        var isGrabbedField = typeof(VRWeaponController).GetField("isGrabbed",
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        if (isGrabbedField != null)
                        {
                            bool isGrabbed = (bool)isGrabbedField.GetValue(weaponController);
                            if (isGrabbed)
                            {
                                CompleteStep();
                            }
                        }
                    }
                    break;

                case TutorialStep.ShootFirstTarget:
                    // Completed via OnTargetHit event
                    break;

                case TutorialStep.ReloadWeapon:
                    // Check if weapon was reloaded
                    if (weaponController != null && weaponController.CurrentAmmo > 0)
                    {
                        CompleteStep();
                    }
                    break;

                case TutorialStep.HitMultipleTargets:
                    // Check if hit 5 targets
                    if (targetHits >= 10)
                    {
                        CompleteStep();
                    }
                    break;

                case TutorialStep.PerfectShots:
                    // Check if got 3 perfect hits
                    if (perfectHits >= 3)
                    {
                        CompleteStep();
                    }
                    break;

                case TutorialStep.MovingTargets:
                    // Check if hit 3 moving targets
                    if (movingTargetHits >= 3)
                    {
                        CompleteStep();
                    }
                    break;
            }
        }

        /// Update progress display
        private void UpdateProgressDisplay()
        {
            if (progressText == null) return;

            string progress = "";

            switch (currentStep)
            {
                case TutorialStep.HitMultipleTargets:
                    progress = $"Targets Hit: {targetHits}/10";
                    break;

                case TutorialStep.PerfectShots:
                    progress = $"Perfect Hits: {perfectHits}/3";
                    break;

                case TutorialStep.MovingTargets:
                    progress = $"Moving Targets Hit: {movingTargetHits}/3";
                    break;

                default:
                    progress = "";
                    break;
            }

            progressText.text = progress;
        }
        #endregion

        #region Target Spawning
        /// Spawn static target
        private void SpawnStaticTarget(Vector3 position)
        {
            if (basicTargetPrefab == null)
            {
                Debug.LogWarning("[Tutorial] Basic target prefab not assigned!");
                return;
            }

            GameObject target = Instantiate(basicTargetPrefab, position, Quaternion.identity);
            activeTargets.Add(target);

            // Configure target to not auto-despawn
            Target targetScript = target.GetComponent<Target>();
            if (targetScript != null)
            {
                targetScript.Initialize(TargetType.Basic, 100, 999f); // Very long lifetime
            }

            Debug.Log($"[Tutorial] Spawned static target at {position}");
        }

        /// Spawn precision target (with bullseye)
        private void SpawnPrecisionTarget(Vector3 position)
        {
            GameObject prefab = precisionTargetPrefab != null ? precisionTargetPrefab : basicTargetPrefab;

            if (prefab == null)
            {
                Debug.LogWarning("[Tutorial] Precision target prefab not assigned!");
                return;
            }

            GameObject target = Instantiate(prefab, position, Quaternion.identity);
            activeTargets.Add(target);

            Target targetScript = target.GetComponent<Target>();
            if (targetScript != null)
            {
                targetScript.Initialize(TargetType.Precision, 200, 999f);
            }
        }

        /// Spawn moving target
        private void SpawnMovingTarget(Vector3 position)
        {
            GameObject prefab = movingTargetPrefab != null ? movingTargetPrefab : basicTargetPrefab;

            if (prefab == null)
            {
                Debug.LogWarning("[Tutorial] Moving target prefab not assigned!");
                return;
            }

            GameObject target = Instantiate(prefab, position, Quaternion.identity);
            activeTargets.Add(target);

            MovingTarget movingScript = target.GetComponent<MovingTarget>();
            if (movingScript != null)
            {
                movingScript.Initialize(TargetType.Moving, 150, 999f);
            }
        }

        /// Clear all active targets
        private void ClearActiveTargets()
        {
            foreach (GameObject target in activeTargets)
            {
                if (target != null)
                {
                    Destroy(target);
                }
            }
            activeTargets.Clear();
        }

        /// Get target position for index
        private Vector3 GetTargetPosition(int index)
        {
            if (staticTargetPositions != null && staticTargetPositions.Length > 0)
            {
                int idx = index % staticTargetPositions.Length;
                return staticTargetPositions[idx].position;
            }

            // Fallback: spawn in front of camera
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                Vector3 forward = mainCam.transform.forward;
                forward.y = 0; // Keep at eye level
                return mainCam.transform.position + forward.normalized * (5f + index * 2f);
            }

            return Vector3.forward * (5f + index * 2f);
        }

        /// Get moving target position
        private Vector3 GetMovingTargetPosition(int index)
        {
            if (movingTargetPositions != null && movingTargetPositions.Length > 0)
            {
                int idx = index % movingTargetPositions.Length;
                return movingTargetPositions[idx].position;
            }

            return GetTargetPosition(index);
        }
        #endregion

        #region Weapon Management
        /// Spawn weapon at spawn point
        private void SpawnWeapon()
        {
            if (weaponPrefab == null)
            {
                Debug.LogWarning("[Tutorial] Weapon prefab not assigned!");
                return;
            }

            Vector3 spawnPos = weaponSpawnPoint != null ? weaponSpawnPoint.position : Vector3.forward * 2f;
            Quaternion spawnRot = weaponSpawnPoint != null ? weaponSpawnPoint.rotation : Quaternion.identity;

            currentWeapon = Instantiate(weaponPrefab, spawnPos, spawnRot);
            weaponController = currentWeapon.GetComponent<VRWeaponController>();

            if (weaponController != null)
            {
                weaponController.SetLaserSightEnabled(true);
            }

            Debug.Log($"[Tutorial] Spawned weapon at {spawnPos}");
        }

        /// Respawn weapon if lost
        private IEnumerator RespawnWeaponAfterDelay()
        {
            yield return new WaitForSeconds(weaponRespawnDelay);

            if (currentWeapon == null && currentStep >= TutorialStep.PickupWeapon && currentStep < TutorialStep.Completed)
            {
                SpawnWeapon();
            }
        }
        #endregion

        #region Event Handlers
        /// Handle target hit event
        private void OnTargetHit(Target target)
        {
            if (!tutorialActive) return;
            if (target == null) return;

            Debug.Log($"[Tutorial] Target hit: {target.Type}, Step: {currentStep}");

            switch (currentStep)
            {
                case TutorialStep.ShootFirstTarget:
                    // First target hit - conta QUALQUER alvo
                    CompleteStep();
                    break;

                case TutorialStep.HitMultipleTargets:
                    // Conta qualquer alvo atingido
                    targetHits++;
                    Debug.Log($"[Tutorial] Multiple targets: {targetHits}/10");
                    break;

                case TutorialStep.PerfectShots:
                    // Conta qualquer alvo como perfeito (simplificado)
                    perfectHits++;
                    Debug.Log($"[Tutorial] Perfect shots: {perfectHits}/3");
                    break;

                case TutorialStep.MovingTargets:
                    // Conta alvos móveis (MovingTarget component)
                    MovingTarget movingTarget = target.GetComponent<MovingTarget>();
                    if (movingTarget != null)
                    {
                        movingTargetHits++;
                        Debug.Log($"[Tutorial] Moving targets: {movingTargetHits}/3");
                    }
                    break;
            }
        }
        #endregion

        #region Tutorial Completion
        /// Complete tutorial
        private void CompleteTutorial()
        {
            tutorialActive = false;

            Debug.Log("[Tutorial] Tutorial completed!");

            // Play completion sound
            if (tutorialCompleteSound != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayButtonClick();
            }

            // ARIA congratulations
            if (AudioManager.Instance != null && playVoiceLines)
            {
                AudioManager.Instance.PlayEncouragementVoice(1.0f);
            }

            // Mark tutorial as completed (PlayerPrefs)
            PlayerPrefs.SetInt("TutorialCompleted", 1);
            PlayerPrefs.Save();

            // Auto-transition to main menu after delay
            StartCoroutine(ReturnToMenuAfterDelay(5f));
        }

        /// Show completion screen
        private void ShowCompletionScreen()
        {
            if (completionPanel != null)
            {
                completionPanel.SetActive(true);
            }

            if (tutorialPanel != null)
            {
                tutorialPanel.SetActive(false);
            }

            // Show stats
            if (objectiveText != null)
            {
                objectiveText.text = "TRAINING COMPLETE\n\n" +
                    "You've mastered the fundamentals:\n" +
                    "✓ Weapon handling\n" +
                    "✓ Target engagement\n" +
                    "✓ Precision shooting\n" +
                    "✓ Moving target tracking\n\n" +
                    "Welcome to APEX, Recruit.";
            }

            // Cancelar auto-return para dar tempo de clicar no botão
            StopCoroutine("ReturnToMenuAfterDelay");
        }

        /// Return to main menu after delay
        private IEnumerator ReturnToMenuAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);

            // Load main menu scene
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ReturnToMenu();
            }
            else
            {
                // Fallback: load scene directly
                UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
            }
        }
        #endregion

        #region Public Methods
        /// Skip current step (for testing)
        [ContextMenu("Skip Current Step")]
        public void SkipCurrentStep()
        {
            CompleteStep();
        }

        /// Reset tutorial
        [ContextMenu("Reset Tutorial")]
        public void ResetTutorial()
        {
            StopAllCoroutines();
            ClearActiveTargets();

            if (currentWeapon != null)
            {
                Destroy(currentWeapon);
            }

            targetHits = 0;
            perfectHits = 0;
            movingTargetHits = 0;

            StartTutorial();
        }

        /// Check if player has completed tutorial before
        public static bool HasCompletedTutorial()
        {
            return PlayerPrefs.GetInt("TutorialCompleted", 0) == 1;
        }

        /// Get tutorial progress (0-1)
        public float GetProgress()
        {
            return (float)currentStep / (float)TutorialStep.Completed;
        }

        /// Called by UI button - Repeat tutorial
        public void OnRepeatButtonClicked()
        {
            Debug.Log("[Tutorial] Repeat button clicked - restarting tutorial");

            // Hide completion panel
            if (completionPanel != null)
            {
                completionPanel.SetActive(false);
            }

            // Reset and restart
            ResetTutorial();
        }

        /// Called by UI button - Return to menu
        public void OnReturnToMenuButtonClicked()
        {
            Debug.Log("[Tutorial] Return to menu button clicked");

            // Load main menu scene
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ReturnToMenu();
            }
            else
            {
                // Fallback: load scene directly
                UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
            }
        }
        #endregion

        #region Debug Gizmos
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            // Draw weapon spawn point
            if (weaponSpawnPoint != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(weaponSpawnPoint.position, 0.2f);
                Gizmos.DrawLine(weaponSpawnPoint.position, weaponSpawnPoint.position + weaponSpawnPoint.forward * 0.5f);
            }

            // Draw static target positions
            if (staticTargetPositions != null)
            {
                Gizmos.color = Color.red;
                foreach (Transform t in staticTargetPositions)
                {
                    if (t != null)
                    {
                        Gizmos.DrawWireSphere(t.position, 0.3f);
                    }
                }
            }

            // Draw moving target positions
            if (movingTargetPositions != null)
            {
                Gizmos.color = Color.green;
                foreach (Transform t in movingTargetPositions)
                {
                    if (t != null)
                    {
                        Gizmos.DrawWireSphere(t.position, 0.3f);
                    }
                }
            }
        }
#endif
        #endregion
    }
}

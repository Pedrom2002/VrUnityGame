using UnityEngine;
using VRAimLab.Core;
using VRAimLab.Gameplay;
using VRAimLab.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VRAimLab.Utilities
{
    /// Quick Start Setup Helper - automates scene setup and verification
    /// Helps developers quickly set up the game with minimal configuration
    /// Use context menu items to auto-create and connect managers
    public class QuickStartSetup : MonoBehaviour
    {
        #region Inspector Fields
        [Header("Auto-Find References")]
        [SerializeField] private bool autoFindOnStart = true;

        [Header("Manager References")]
        [SerializeField] private GameManager gameManager;
        [SerializeField] private AudioManager audioManager;
        [SerializeField] private UIManager uiManager;
        [SerializeField] private TargetSpawner targetSpawner;
        [SerializeField] private ObjectPooler objectPooler;

        [Header("Scene References")]
        [SerializeField] private Camera vrCamera;
        [SerializeField] private Transform xrOrigin;
        #endregion

        #region Unity Lifecycle
        private void Start()
        {
            if (autoFindOnStart)
            {
                FindAllReferences();
                VerifySetup();
            }
        }
        #endregion

        #region Auto-Find References
        /// Automatically find all manager references in scene
        [ContextMenu("Find All References")]
        public void FindAllReferences()
        {
            Debug.Log("[QuickStartSetup] Finding all references...");

            // Find managers
            if (gameManager == null)
            {
                gameManager = FindFirstObjectByType<GameManager>();
                if (gameManager != null)
                    Debug.Log("[QuickStartSetup] ✓ Found GameManager");
            }

            if (audioManager == null)
            {
                audioManager = FindFirstObjectByType<AudioManager>();
                if (audioManager != null)
                    Debug.Log("[QuickStartSetup] ✓ Found AudioManager");
            }

            if (uiManager == null)
            {
                uiManager = FindFirstObjectByType<UIManager>();
                if (uiManager != null)
                    Debug.Log("[QuickStartSetup] ✓ Found UIManager");
            }

            if (targetSpawner == null)
            {
                targetSpawner = FindFirstObjectByType<TargetSpawner>();
                if (targetSpawner != null)
                    Debug.Log("[QuickStartSetup] ✓ Found TargetSpawner");
            }

            if (objectPooler == null)
            {
                objectPooler = FindFirstObjectByType<ObjectPooler>();
                if (objectPooler != null)
                    Debug.Log("[QuickStartSetup] ✓ Found ObjectPooler");
            }

            // Find VR components
            if (vrCamera == null)
            {
                vrCamera = Camera.main;
                if (vrCamera != null)
                    Debug.Log("[QuickStartSetup] ✓ Found Main Camera");
            }

            if (xrOrigin == null)
            {
                GameObject xrOriginObj = GameObject.Find("XR Origin") ?? GameObject.Find("XR Rig");
                if (xrOriginObj != null)
                {
                    xrOrigin = xrOriginObj.transform;
                    Debug.Log("[QuickStartSetup] ✓ Found XR Origin");
                }
            }

            Debug.Log("[QuickStartSetup] Reference search complete!");
        }
        #endregion

        #region Create Missing Managers
        /// Create missing manager GameObjects
        [ContextMenu("Create Missing Managers")]
        public void CreateMissingManagers()
        {
            Debug.Log("[QuickStartSetup] Creating missing managers...");

            // Create GameManager
            if (gameManager == null && FindFirstObjectByType<GameManager>() == null)
            {
                GameObject gmObj = new GameObject("GameManager");
                gameManager = gmObj.AddComponent<GameManager>();
                gmObj.AddComponent<ScoreSystem>();
                Debug.Log("[QuickStartSetup] ✓ Created GameManager");
            }

            // Create AudioManager
            if (audioManager == null && FindFirstObjectByType<AudioManager>() == null)
            {
                GameObject amObj = new GameObject("AudioManager");
                audioManager = amObj.AddComponent<AudioManager>();
                Debug.Log("[QuickStartSetup] ✓ Created AudioManager");
            }

            // Create TargetSpawner with ObjectPooler
            if (targetSpawner == null && FindFirstObjectByType<TargetSpawner>() == null)
            {
                GameObject tsObj = new GameObject("TargetSpawner");
                targetSpawner = tsObj.AddComponent<TargetSpawner>();
                objectPooler = tsObj.AddComponent<ObjectPooler>();
                Debug.Log("[QuickStartSetup] ✓ Created TargetSpawner + ObjectPooler");
            }

            // Create UIManager
            if (uiManager == null && FindFirstObjectByType<UIManager>() == null)
            {
                GameObject uiObj = new GameObject("UIManager");
                Canvas canvas = uiObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.WorldSpace;
                uiObj.AddComponent<UnityEngine.UI.CanvasScaler>();
                uiObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
                uiManager = uiObj.AddComponent<UIManager>();
                Debug.Log("[QuickStartSetup] ✓ Created UIManager with Canvas");
            }

            Debug.Log("[QuickStartSetup] Manager creation complete!");

#if UNITY_EDITOR
            // Mark scene as dirty
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
            );
#endif
        }
        #endregion

        #region Setup Verification
        /// Verify XR and scene setup
        [ContextMenu("Verify Setup")]
        public void VerifySetup()
        {
            Debug.Log("========================================");
            Debug.Log("VR AIM LAB - SETUP VERIFICATION");
            Debug.Log("========================================");

            bool allGood = true;

            // Check managers
            allGood &= CheckComponent("GameManager", gameManager);
            allGood &= CheckComponent("AudioManager", audioManager);
            allGood &= CheckComponent("UIManager", uiManager);
            allGood &= CheckComponent("TargetSpawner", targetSpawner);
            allGood &= CheckComponent("ObjectPooler", objectPooler);

            // Check VR components
            allGood &= CheckComponent("VR Camera", vrCamera);
            allGood &= CheckComponent("XR Origin", xrOrigin);

            // Check XR settings
            VerifyXRSetup();

            Debug.Log("========================================");
            if (allGood)
            {
                Debug.Log("✓ ALL CHECKS PASSED!");
                Debug.Log("Your scene is ready for VR Aim Lab.");
            }
            else
            {
                Debug.LogWarning("⚠ SOME CHECKS FAILED!");
                Debug.LogWarning("Use 'Create Missing Managers' to auto-create missing components.");
            }
            Debug.Log("========================================");
        }

        /// Check if component exists and log result
        private bool CheckComponent<T>(string name, T component) where T : Object
        {
            if (component != null)
            {
                Debug.Log($"✓ {name} found");
                return true;
            }
            else
            {
                Debug.LogWarning($"✗ {name} MISSING!");
                return false;
            }
        }

        /// Verify XR Toolkit setup
        private void VerifyXRSetup()
        {
            Debug.Log("\n--- XR SETUP ---");

            // Check for XR Interaction Manager
            var xrInteractionManager = FindFirstObjectByType<UnityEngine.XR.Interaction.Toolkit.XRInteractionManager>();
            if (xrInteractionManager != null)
            {
                Debug.Log("✓ XR Interaction Manager found");
            }
            else
            {
                Debug.LogWarning("✗ XR Interaction Manager MISSING!");
                Debug.LogWarning("  Create: GameObject → XR → Interaction Manager");
            }

            // Check for XR Origin
            if (xrOrigin != null)
            {
                Debug.Log("✓ XR Origin found");

                // Check for controllers
                #pragma warning disable CS0618 // Type or member is obsolete - keeping for backward compatibility check
                var controllers = xrOrigin.GetComponentsInChildren<UnityEngine.XR.Interaction.Toolkit.XRBaseController>();
                #pragma warning restore CS0618
                if (controllers.Length > 0)
                {
                    Debug.Log($"✓ Found {controllers.Length} XR Controller(s)");
                }
                else
                {
                    Debug.LogWarning("✗ No XR Controllers found in XR Origin!");
                }
            }
            else
            {
                Debug.LogWarning("✗ XR Origin MISSING!");
                Debug.LogWarning("  Create: GameObject → XR → XR Origin (VR)");
            }
        }
        #endregion

        #region Setup Checklist
        /// Print setup checklist for manual verification
        [ContextMenu("Print Setup Checklist")]
        public void PrintSetupChecklist()
        {
            Debug.Log("========================================");
            Debug.Log("VR AIM LAB - SETUP CHECKLIST");
            Debug.Log("========================================");
            Debug.Log("\n□ Import XR Interaction Toolkit from Package Manager");
            Debug.Log("□ Import TextMeshPro essentials");
            Debug.Log("□ Create XR Origin (GameObject → XR → XR Origin)");
            Debug.Log("□ Create XR Interaction Manager (GameObject → XR → Interaction Manager)");
            Debug.Log("□ Create GameManager (or use QuickStartSetup → Create Missing Managers)");
            Debug.Log("□ Create AudioManager");
            Debug.Log("□ Create TargetSpawner + ObjectPooler");
            Debug.Log("□ Create UIManager with Canvas");
            Debug.Log("□ Create Target prefabs (BasicTarget, MovingTarget, PrecisionTarget)");
            Debug.Log("□ Create Weapon prefab with VRWeaponController");
            Debug.Log("□ Assign references in Inspector");
            Debug.Log("□ Configure spawn points or spawn area");
            Debug.Log("□ Test in VR headset");
            Debug.Log("\n========================================");
        }
        #endregion

        #region Quick Actions
        /// Print useful Unity shortcuts
        [ContextMenu("Show Useful Shortcuts")]
        public void ShowUsefulShortcuts()
        {
            Debug.Log("========================================");
            Debug.Log("USEFUL UNITY SHORTCUTS");
            Debug.Log("========================================");
            Debug.Log("F - Frame selected object in Scene view");
            Debug.Log("Ctrl+Shift+N - Create new empty GameObject");
            Debug.Log("Ctrl+D - Duplicate selected object");
            Debug.Log("Ctrl+P - Play/Stop game");
            Debug.Log("Ctrl+Shift+F - Move camera to selected object");
            Debug.Log("Alt+Click - Open object in Project window");
            Debug.Log("========================================");
        }

        /// Print project structure guide
        [ContextMenu("Show Project Structure")]
        public void ShowProjectStructure()
        {
            Debug.Log("========================================");
            Debug.Log("VR AIM LAB - PROJECT STRUCTURE");
            Debug.Log("========================================");
            Debug.Log("\nAssets/");
            Debug.Log("├── Scripts/");
            Debug.Log("│   ├── Core/");
            Debug.Log("│   │   ├── GameManager.cs");
            Debug.Log("│   │   ├── AudioManager.cs");
            Debug.Log("│   │   └── ScoreSystem.cs");
            Debug.Log("│   ├── Gameplay/");
            Debug.Log("│   │   ├── Target.cs");
            Debug.Log("│   │   ├── MovingTarget.cs");
            Debug.Log("│   │   ├── TargetSpawner.cs");
            Debug.Log("│   │   ├── VRWeaponController.cs");
            Debug.Log("│   │   └── ObjectPooler.cs");
            Debug.Log("│   ├── UI/");
            Debug.Log("│   │   ├── UIManager.cs");
            Debug.Log("│   │   ├── MenuController.cs");
            Debug.Log("│   │   └── HUDController.cs");
            Debug.Log("│   └── Utilities/");
            Debug.Log("│       ├── QuickStartSetup.cs");
            Debug.Log("│       └── DebugHelper.cs");
            Debug.Log("├── Prefabs/");
            Debug.Log("│   ├── Targets/");
            Debug.Log("│   └── Weapons/");
            Debug.Log("├── Materials/");
            Debug.Log("├── Audio/");
            Debug.Log("└── Scenes/");
            Debug.Log("========================================");
        }
        #endregion

        #region Editor Utilities
#if UNITY_EDITOR
        /// Create complete scene setup (Editor only)
        [MenuItem("VR Aim Lab/Complete Scene Setup")]
        public static void CompleteSceneSetup()
        {
            QuickStartSetup setup = FindFirstObjectByType<QuickStartSetup>();

            if (setup == null)
            {
                GameObject setupObj = new GameObject("QuickStartSetup");
                setup = setupObj.AddComponent<QuickStartSetup>();
            }

            setup.CreateMissingManagers();
            setup.FindAllReferences();
            setup.VerifySetup();

            EditorUtility.DisplayDialog(
                "Setup Complete",
                "Scene setup complete! Check Console for details.\n\nNext steps:\n" +
                "1. Create target prefabs\n" +
                "2. Create weapon prefab\n" +
                "3. Assign references in Inspector\n" +
                "4. Test in VR",
                "OK"
            );
        }

        /// Show setup wizard
        [MenuItem("VR Aim Lab/Setup Wizard")]
        public static void ShowSetupWizard()
        {
            QuickStartSetup setup = FindFirstObjectByType<QuickStartSetup>();

            if (setup == null)
            {
                GameObject setupObj = new GameObject("QuickStartSetup");
                setup = setupObj.AddComponent<QuickStartSetup>();
            }

            setup.PrintSetupChecklist();

            bool proceed = EditorUtility.DisplayDialog(
                "VR Aim Lab Setup Wizard",
                "Welcome to VR Aim Lab!\n\n" +
                "This wizard will help you set up your scene.\n\n" +
                "Prerequisites:\n" +
                "✓ XR Interaction Toolkit installed\n" +
                "✓ TextMeshPro imported\n\n" +
                "Continue with automatic setup?",
                "Yes, Auto Setup",
                "No, Manual Setup"
            );

            if (proceed)
            {
                CompleteSceneSetup();
            }
            else
            {
                Debug.Log("[Setup Wizard] Manual setup selected. Use context menu items for step-by-step setup.");
            }
        }

        /// Open documentation folder
        [MenuItem("VR Aim Lab/Open Documentation")]
        public static void OpenDocumentation()
        {
            string docPath = Application.dataPath + "/../Documentation";
            if (System.IO.Directory.Exists(docPath))
            {
                EditorUtility.RevealInFinder(docPath);
            }
            else
            {
                EditorUtility.DisplayDialog(
                    "Documentation Not Found",
                    "Documentation folder not found at:\n" + docPath,
                    "OK"
                );
            }
        }
#endif
        #endregion
    }
}

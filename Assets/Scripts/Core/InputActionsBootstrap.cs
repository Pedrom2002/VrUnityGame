using UnityEngine;
using UnityEngine.InputSystem;

namespace VRAimLab.Core
{
    /// Automatically enables all Input Actions when the game starts
    /// This script ensures all Input Actions are enabled without manual intervention
    /// Add this to a GameObject in your first scene or use [RuntimeInitializeOnLoadMethod]
    public class InputActionsBootstrap : MonoBehaviour
    {
        [Header("Input Action Assets")]
        [Tooltip("List all Input Action Assets that should be enabled at startup")]
        [SerializeField] private InputActionAsset[] inputActionAssets;

        [Header("Settings")]
        [SerializeField] private bool enableOnAwake = true;
        [SerializeField] private bool enableOnEnable = true;
        [SerializeField] private bool autoFindInputActionAssets = true;

        private void Awake()
        {
            if (enableOnAwake)
            {
                EnableAllInputActions();
            }
        }

        private void OnEnable()
        {
            if (enableOnEnable)
            {
                EnableAllInputActions();
            }
        }

        /// Enable all assigned Input Action Assets
        public void EnableAllInputActions()
        {
            int enabledCount = 0;

            // If auto-find is enabled and no assets are assigned, try to find them
            if (autoFindInputActionAssets && (inputActionAssets == null || inputActionAssets.Length == 0))
            {
                FindAndEnableAllInputActionAssets();
                return;
            }

            // Enable manually assigned Input Action Assets
            if (inputActionAssets != null && inputActionAssets.Length > 0)
            {
                foreach (var asset in inputActionAssets)
                {
                    if (asset != null)
                    {
                        if (!asset.enabled)
                        {
                            asset.Enable();
                            enabledCount++;
                            Debug.Log($"[InputBootstrap] Enabled Input Actions: {asset.name}");
                        }
                        else
                        {
                            Debug.Log($"[InputBootstrap] Input Actions already enabled: {asset.name}");
                        }
                    }
                }

                Debug.Log($"[InputBootstrap] Successfully enabled {enabledCount} Input Action Asset(s)");
            }
            else
            {
                Debug.LogWarning("[InputBootstrap] No Input Action Assets assigned! Please assign them in the Inspector or enable 'Auto Find'.");
            }
        }

        /// Find and enable all Input Action Assets in the project
        private void FindAndEnableAllInputActionAssets()
        {
            // Load all Input Action Assets from Resources
            InputActionAsset[] foundAssets = Resources.LoadAll<InputActionAsset>("");

            if (foundAssets.Length == 0)
            {
                Debug.LogWarning("[InputBootstrap] Auto-find enabled but no Input Action Assets found in Resources folders.");
                return;
            }

            int enabledCount = 0;

            foreach (var asset in foundAssets)
            {
                if (asset != null && !asset.enabled)
                {
                    asset.Enable();
                    enabledCount++;
                    Debug.Log($"[InputBootstrap] Auto-enabled Input Actions: {asset.name}");
                }
            }

            Debug.Log($"[InputBootstrap] Auto-enabled {enabledCount} Input Action Asset(s)");
        }

        /// Disable all assigned Input Action Assets
        public void DisableAllInputActions()
        {
            if (inputActionAssets != null && inputActionAssets.Length > 0)
            {
                foreach (var asset in inputActionAssets)
                {
                    if (asset != null && asset.enabled)
                    {
                        asset.Disable();
                        Debug.Log($"[InputBootstrap] Disabled Input Actions: {asset.name}");
                    }
                }
            }
        }

        private void OnDisable()
        {
            // Optionally disable on disable (commented out to prevent issues)
            // DisableAllInputActions();
        }

        private void OnDestroy()
        {
            // Optionally disable on destroy (commented out to prevent issues)
            // DisableAllInputActions();
        }

#if UNITY_EDITOR
        [ContextMenu("Find All Input Action Assets")]
        private void FindAllInputActionAssetsInEditor()
        {
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:InputActionAsset");
            inputActionAssets = new InputActionAsset[guids.Length];

            for (int i = 0; i < guids.Length; i++)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[i]);

                // Skip assets in PackageCache
                if (path.Contains("PackageCache"))
                    continue;

                inputActionAssets[i] = UnityEditor.AssetDatabase.LoadAssetAtPath<InputActionAsset>(path);
            }

            // Remove null entries
            inputActionAssets = System.Array.FindAll(inputActionAssets, asset => asset != null);

            Debug.Log($"[InputBootstrap] Found {inputActionAssets.Length} Input Action Asset(s) in project");
        }
#endif
    }

    /// Alternative: Use RuntimeInitializeOnLoadMethod to enable Input Actions automatically
    /// This runs before any scene loads
    public static class InputActionsAutoEnable
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void EnableInputActionsOnStartup()
        {
            // You can manually specify Input Action Assets to enable here
            // Example:
            // var asset = Resources.Load<InputActionAsset>("XRI Default Input Actions");
            // if (asset != null && !asset.enabled)
            // {
            //     asset.Enable();
            //     Debug.Log("[InputBootstrap] Auto-enabled XRI Default Input Actions");
            // }

            Debug.Log("[InputBootstrap] RuntimeInitializeOnLoadMethod executed");
        }
    }
}

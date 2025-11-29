using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public class AutoLoadSampleScene
{
    private const string SCENE_PATH = "Assets/Scenes/SampleScene.unity";
    private const string PREF_KEY = "AutoLoadSampleScene_HasLoaded";

    static AutoLoadSampleScene()
    {
        EditorApplication.delayCall += () =>
        {
            // Check if we should load the scene
            if (!EditorApplication.isPlayingOrWillChangePlaymode &&
                !EditorApplication.isCompiling)
            {
                // Get the current active scene
                var currentScene = EditorSceneManager.GetActiveScene();

                // If no scene is loaded or it's an untitled scene, load SampleScene
                if (string.IsNullOrEmpty(currentScene.path) ||
                    currentScene.path == "Temp/__Backupscenes/0.backup")
                {
                    LoadSampleScene();
                }
            }
        };
    }

    private static void LoadSampleScene()
    {
        if (System.IO.File.Exists(SCENE_PATH))
        {
            EditorSceneManager.OpenScene(SCENE_PATH);
            Debug.Log("Auto-loaded SampleScene");
        }
        else
        {
            Debug.LogWarning($"SampleScene not found at path: {SCENE_PATH}");
        }
    }
}

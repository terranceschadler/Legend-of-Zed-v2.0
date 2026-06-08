using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

namespace LegendOfZed.Editor
{
    public static class ZedV03CleanControllerTestSceneLockdown
    {
        private const string SourceScenePath = "Assets/Scenes/SampleScene.unity";
        private const string TargetFolder = "Assets/LegendOfZed/Scenes";
        private const string TargetScenePath = "Assets/LegendOfZed/Scenes/Zed_Controller_Test.unity";
        private const string MarkerName = "Zed_Controller_Test_Root";

        [MenuItem("Legend of Zed/Setup/v0.3 Lock Clean Controller Test Scene")]
        public static void LockCleanControllerTestScene()
        {
            if (!File.Exists(SourceScenePath))
            {
                Debug.LogWarning("v0.3 scene lockdown could not find source scene: " + SourceScenePath);
                return;
            }

            EnsureFolder("Assets/LegendOfZed");
            EnsureFolder(TargetFolder);

            if (File.Exists(TargetScenePath))
            {
                AssetDatabase.DeleteAsset(TargetScenePath);
            }

            if (!AssetDatabase.CopyAsset(SourceScenePath, TargetScenePath))
            {
                Debug.LogWarning("v0.3 scene lockdown failed to copy " + SourceScenePath + " to " + TargetScenePath);
                return;
            }

            AssetDatabase.Refresh();

            Scene scene = EditorSceneManager.OpenScene(TargetScenePath, OpenSceneMode.Single);
            EnsureMarker(scene);
            EnsureInputSystemUiModules();
            SetOnlyBuildScene(TargetScenePath);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, TargetScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("v0.3 clean controller test scene locked at " + TargetScenePath + ". Source scene was copied unchanged except for Input System UI module cleanup, root marker, and Build Settings scene path. No weapon data, ammo, projectiles, prefabs, controller logic, Synty, or materials were changed.");
        }

        private static void EnsureMarker(Scene scene)
        {
            foreach (GameObject rootObject in scene.GetRootGameObjects())
            {
                if (rootObject.name == MarkerName)
                {
                    return;
                }
            }

            GameObject marker = new GameObject(MarkerName);
            EditorSceneManager.MoveGameObjectToScene(marker, scene);
        }

        private static void EnsureInputSystemUiModules()
        {
            EventSystem[] eventSystems = Object.FindObjectsByType<EventSystem>(FindObjectsInactive.Include);
            foreach (EventSystem eventSystem in eventSystems)
            {
                if (eventSystem == null)
                {
                    continue;
                }

                StandaloneInputModule oldModule = eventSystem.GetComponent<StandaloneInputModule>();
                if (oldModule != null)
                {
                    Object.DestroyImmediate(oldModule, true);
                }

                if (eventSystem.GetComponent<InputSystemUIInputModule>() == null)
                {
                    eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
                }

                EditorUtility.SetDirty(eventSystem.gameObject);
            }
        }

        private static void SetOnlyBuildScene(string scenePath)
        {
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(scenePath, true)
            };
        }

        private static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder))
            {
                return;
            }

            string normalized = folder.Replace("\\", "/");
            string parent = Path.GetDirectoryName(normalized)?.Replace("\\", "/");
            string child = Path.GetFileName(normalized);

            if (string.IsNullOrEmpty(parent) || string.IsNullOrEmpty(child))
            {
                return;
            }

            if (!AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(parent, child);
        }
    }
}

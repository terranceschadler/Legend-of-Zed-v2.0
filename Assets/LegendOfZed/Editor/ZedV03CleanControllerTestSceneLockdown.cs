using System.IO;
using TopDownShooter;
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
        private const string TargetFolder = "Assets/LegendOfZed/Scenes";
        private const string TargetScenePath = "Assets/LegendOfZed/Scenes/Zed_Controller_Test.unity";
        private const string MarkerName = "Zed_Controller_Test_Root";

        [MenuItem("Legend of Zed/Setup/v0.3 Lock Clean Controller Test Scene")]
        public static void LockCleanControllerTestScene()
        {
            string sourceScenePath = FindBestControllerDemoScene();
            if (string.IsNullOrEmpty(sourceScenePath))
            {
                Debug.LogWarning("v0.3 scene lockdown could not find a TopDownShooterController scene containing PlayerController/ShooterController. Open the working package demo scene once, then rerun this menu.");
                return;
            }

            EnsureFolder("Assets/LegendOfZed");
            EnsureFolder(TargetFolder);

            if (File.Exists(TargetScenePath))
            {
                AssetDatabase.DeleteAsset(TargetScenePath);
            }

            if (!AssetDatabase.CopyAsset(sourceScenePath, TargetScenePath))
            {
                Debug.LogWarning("v0.3 scene lockdown failed to copy " + sourceScenePath + " to " + TargetScenePath);
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

            Debug.Log("v0.3 clean controller test scene locked at " + TargetScenePath + ". Source scene copied from " + sourceScenePath + ". No weapon data, ammo, projectiles, prefabs, controller logic, Synty, or materials were changed.");
        }

        private static string FindBestControllerDemoScene()
        {
            string[] sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/TopDownShooterController" });
            string bestPath = string.Empty;
            int bestScore = int.MinValue;

            foreach (string guid in sceneGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrEmpty(path) || !path.EndsWith(".unity"))
                {
                    continue;
                }

                if (path == TargetScenePath)
                {
                    continue;
                }

                int score = ScoreScenePath(path);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestPath = path;
                }
            }

            if (!string.IsNullOrEmpty(bestPath) && bestScore > int.MinValue)
            {
                Debug.Log("v0.3 selected controller demo source scene: " + bestPath);
                return bestPath;
            }

            return string.Empty;
        }

        private static int ScoreScenePath(string path)
        {
            string lowerPath = path.ToLowerInvariant();
            int score = 0;

            if (lowerPath.Contains("topdownshootercontroller")) score += 100;
            if (lowerPath.Contains("demo")) score += 50;
            if (lowerPath.Contains("1_demoscenepc")) score += 1000;
            if (lowerPath.Contains("mainscene")) score += 700;
            if (lowerPath.Contains("shooterenemy")) score += 500;
            if (lowerPath.Contains("powers")) score -= 100;
            if (lowerPath.Contains("movie")) score -= 200;
            if (lowerPath.Contains("settings")) score -= 500;

            return score;
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

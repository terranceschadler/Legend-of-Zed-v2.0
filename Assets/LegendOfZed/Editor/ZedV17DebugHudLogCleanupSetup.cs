using System.IO;
using LegendOfZed.Player;
using TopDownShooter;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LegendOfZed.Editor
{
    public static class ZedV17DebugHudLogCleanupSetup
    {
        private const string ScenePath = "Assets/LegendOfZed/Scenes/Zed_Controller_Test.unity";

        [MenuItem("Legend of Zed/Setup/v1.7 Clean Debug HUD And Logs")]
        public static void CleanDebugHudAndLogs()
        {
            ApplyDebugSettings(false, false, true);
        }

        [MenuItem("Legend of Zed/Setup/v1.7 Enable Player Debug HUD")]
        public static void EnablePlayerDebugHud()
        {
            ApplyDebugSettings(true, true, true);
        }

        private static void ApplyDebugSettings(bool showDebugHud, bool logDamage, bool showDamageFlash)
        {
            if (!File.Exists(ScenePath))
            {
                Debug.LogWarning("Could not find scene: " + ScenePath);
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            PlayerController playerController = Object.FindFirstObjectByType<PlayerController>();
            GameObject playerObject = playerController != null ? playerController.gameObject : GameObject.FindGameObjectWithTag("Player");
            if (playerObject == null)
            {
                Debug.LogWarning("v1.7 cleanup could not find PlayerController or Player-tagged object.");
                return;
            }

            ZedPlayerHealth health = playerObject.GetComponent<ZedPlayerHealth>();
            if (health == null)
            {
                health = playerObject.AddComponent<ZedPlayerHealth>();
                health.MaxHealth = 100f;
                health.CurrentHealth = 100f;
            }

            health.ShowDebugHud = showDebugHud;
            health.LogDamageForTesting = logDamage;
            health.ShowDamageFlash = showDamageFlash;

            EditorUtility.SetDirty(playerObject);
            EditorUtility.SetDirty(health);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log(
                "v1.7 debug cleanup applied. ShowDebugHud=" + showDebugHud +
                " LogDamageForTesting=" + logDamage +
                " ShowDamageFlash=" + showDamageFlash +
                ". Gameplay behavior was not changed.");
        }
    }
}

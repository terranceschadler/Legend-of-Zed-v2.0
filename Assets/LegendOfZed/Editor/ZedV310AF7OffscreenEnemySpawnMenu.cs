#if UNITY_EDITOR
using LegendOfZed.Enemies;
using LegendOfZed.LegacyMapGenerator;
using TopDownShooter;
using UnityEditor;
using UnityEngine;

namespace LegendOfZed.EditorTools
{
    public static class ZedV310AF7OffscreenEnemySpawnMenu
    {
        private const string ZombiePrefabPath = "Assets/LegendOfZed/Prefabs/Enemies/Zed_Zombie_Enemy_Baseline.prefab";
        private const string DirectorName = "Zed_Offscreen_Enemy_Spawn_Director";

        [MenuItem("Legend Of Zed/Enemies/Setup Offscreen Enemy Spawn Director")]
        public static void SetupOffscreenEnemySpawnDirector()
        {
            ZedOffscreenEnemySpawnDirector director = Object.FindAnyObjectByType<ZedOffscreenEnemySpawnDirector>(FindObjectsInactive.Include);
            GameObject directorObject;

            if (director == null)
            {
                directorObject = new GameObject(DirectorName);
                Undo.RegisterCreatedObjectUndo(directorObject, "Create offscreen enemy spawn director");
                director = directorObject.AddComponent<ZedOffscreenEnemySpawnDirector>();
            }
            else
            {
                directorObject = director.gameObject;
                Undo.RecordObject(director, "Setup offscreen enemy spawn director");
            }

            director.MapGenerator = Object.FindAnyObjectByType<ZedLegacyRandomMapGenerator>(FindObjectsInactive.Include);
            director.GameplayCamera = Camera.main;

            PlayerController playerController = Object.FindAnyObjectByType<PlayerController>(FindObjectsInactive.Include);
            if (playerController != null)
            {
                director.Player = playerController.transform;
            }
            else
            {
                GameObject player = FindGameObjectWithTagSafe("Player");
                if (player != null)
                {
                    director.Player = player.transform;
                }
            }

            if (director.ZombiePrefab == null)
            {
                director.ZombiePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ZombiePrefabPath);
            }

            director.SpawnAfterMapGeneration = true;
            director.AutoStart = true;
            director.InitialGraceSeconds = 5f;
            director.StartTileSafeRadius = 14f;
            director.MinPlayerDistance = 22f;
            director.MaxPlayerDistance = 65f;
            director.MinGroupSize = 1;
            director.MaxGroupSize = 3;
            director.MinSpawnInterval = 3f;
            director.MaxSpawnInterval = 6f;
            director.MaxAliveZombies = 10;
            director.MaxSpawnedPerMinute = 24;
            director.DespawnFarInvisibleZombies = true;
            director.FarDespawnDistance = 90f;
            director.UsePooling = true;
            director.PrewarmPoolCount = 0;
            director.VerboseLogs = false;
            director.DrawDebugGizmos = false;

            EditorUtility.SetDirty(director);
            Selection.activeGameObject = directorObject;

            Debug.Log("v3.10AF7 offscreen enemy spawn director setup complete. Assign the real zombie prefab if the default baseline prefab is missing.", director);
        }

        [MenuItem("Legend Of Zed/Enemies/Select Offscreen Enemy Spawn Director")]
        public static void SelectOffscreenEnemySpawnDirector()
        {
            ZedOffscreenEnemySpawnDirector director = Object.FindAnyObjectByType<ZedOffscreenEnemySpawnDirector>(FindObjectsInactive.Include);
            if (director == null)
            {
                Debug.LogWarning("No ZedOffscreenEnemySpawnDirector exists in the open scene. Run Setup Offscreen Enemy Spawn Director first.");
                return;
            }

            Selection.activeGameObject = director.gameObject;
            EditorGUIUtility.PingObject(director.gameObject);
        }

        private static GameObject FindGameObjectWithTagSafe(string tagName)
        {
            if (string.IsNullOrEmpty(tagName))
            {
                return null;
            }

            try
            {
                return GameObject.FindGameObjectWithTag(tagName);
            }
            catch (UnityException)
            {
                return null;
            }
        }
    }
}
#endif

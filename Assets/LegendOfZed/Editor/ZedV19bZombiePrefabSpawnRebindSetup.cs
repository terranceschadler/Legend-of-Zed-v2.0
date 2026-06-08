using System.IO;
using LegendOfZed.Enemies;
using TopDownShooter;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LegendOfZed.Editor
{
    public static class ZedV19bZombiePrefabSpawnRebindSetup
    {
        private const string ScenePath = "Assets/LegendOfZed/Scenes/Zed_Controller_Test.unity";
        private const string SpawnerName = "Zed_Zombie_Spawner";
        private const string PrefabPath = "Assets/LegendOfZed/Prefabs/Enemies/Zed_Zombie_Enemy_Baseline.prefab";

        [MenuItem("Legend of Zed/Setup/v1.9b Reassign Prefab Spawner And Rebind Rules")]
        public static void ReassignPrefabSpawnerAndRebindRules()
        {
            if (!File.Exists(ScenePath))
            {
                Debug.LogWarning("Could not find scene: " + ScenePath);
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (prefab == null)
            {
                Debug.LogWarning("Could not find zombie prefab baseline at: " + PrefabPath);
                return;
            }

            GameObject spawnerObject = GameObject.Find(SpawnerName);
            if (spawnerObject == null)
            {
                Debug.LogWarning("Could not find spawner object: " + SpawnerName);
                return;
            }

            ZedZombieSpawner spawner = spawnerObject.GetComponent<ZedZombieSpawner>();
            if (spawner == null)
            {
                spawner = spawnerObject.AddComponent<ZedZombieSpawner>();
            }

            PlayerController player = Object.FindFirstObjectByType<PlayerController>();

            spawner.ZombiePrototype = prefab;
            spawner.PlayerTarget = player != null ? player.transform : null;
            spawner.HidePrototypeOnStart = false;
            spawner.SpawnOnStart = true;
            spawner.RagdollOnDeath = true;
            spawner.DeathDisableDelay = Mathf.Max(spawner.DeathDisableDelay, 999f);

            EditorUtility.SetDirty(spawner);
            EditorUtility.SetDirty(spawnerObject);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("v1.9b spawner now points at prefab asset and spawned zombies will rebind their own Animator/root-motion/audio references after instantiation.");
        }
    }
}

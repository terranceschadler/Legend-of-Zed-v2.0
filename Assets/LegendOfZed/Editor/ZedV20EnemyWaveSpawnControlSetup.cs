using System.IO;
using LegendOfZed.Enemies;
using TopDownShooter;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LegendOfZed.Editor
{
    public static class ZedV20EnemyWaveSpawnControlSetup
    {
        private const string ScenePath = "Assets/LegendOfZed/Scenes/Zed_Controller_Test.unity";
        private const string SpawnerName = "Zed_Zombie_Spawner";
        private const string PrefabPath = "Assets/LegendOfZed/Prefabs/Enemies/Zed_Zombie_Enemy_Baseline.prefab";

        [MenuItem("Legend of Zed/Setup/v2.0 Apply Enemy Wave Spawn Defaults")]
        public static void ApplyEnemyWaveSpawnDefaults()
        {
            if (!File.Exists(ScenePath))
            {
                Debug.LogWarning("Could not find scene: " + ScenePath);
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

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

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (prefab != null)
            {
                spawner.ZombiePrototype = prefab;
                spawner.HidePrototypeOnStart = false;
            }

            PlayerController player = Object.FindFirstObjectByType<PlayerController>();
            spawner.PlayerTarget = player != null ? player.transform : null;

            spawner.SpawnOnStart = true;
            spawner.LoopWaves = false;
            spawner.ZombiesPerWave = 5;
            spawner.MaxAliveZombies = 5;
            spawner.InitialSpawnDelay = 0.25f;
            spawner.DelayBetweenSpawns = 0.45f;
            spawner.DelayBetweenWaves = 4f;

            spawner.ZombieHealth = 60f;
            spawner.DeathDisableDelay = 999f;
            spawner.RagdollOnDeath = true;
            spawner.LogWaveEvents = false;

            EditorUtility.SetDirty(spawner);
            EditorUtility.SetDirty(spawnerObject);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("v2.0 enemy wave spawn defaults applied. Spawner uses prefab baseline when available.");
        }

        [MenuItem("Legend of Zed/Setup/v2.0 Validate Enemy Wave Spawner")]
        public static void ValidateEnemyWaveSpawner()
        {
            if (!File.Exists(ScenePath))
            {
                Debug.LogWarning("Could not find scene: " + ScenePath);
                return;
            }

            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            int warnings = 0;
            GameObject spawnerObject = GameObject.Find(SpawnerName);
            if (spawnerObject == null)
            {
                Debug.LogWarning("Missing spawner object: " + SpawnerName);
                return;
            }

            ZedZombieSpawner spawner = spawnerObject.GetComponent<ZedZombieSpawner>();
            if (spawner == null)
            {
                Debug.LogWarning("Spawner object is missing ZedZombieSpawner.");
                return;
            }

            if (spawner.ZombiePrototype == null)
            {
                Debug.LogWarning("Spawner has no ZombiePrototype assigned.");
                warnings++;
            }

            if (spawner.SpawnPoints == null || spawner.SpawnPoints.Count == 0)
            {
                Debug.LogWarning("Spawner has no spawn points.");
                warnings++;
            }

            if (spawner.ZombiesPerWave <= 0)
            {
                Debug.LogWarning("ZombiesPerWave should be greater than 0.");
                warnings++;
            }

            if (spawner.MaxAliveZombies <= 0)
            {
                Debug.LogWarning("MaxAliveZombies should be greater than 0.");
                warnings++;
            }

            if (spawner.MaxAliveZombies < spawner.ZombiesPerWave && !spawner.LoopWaves)
            {
                Debug.LogWarning("MaxAliveZombies is lower than ZombiesPerWave. This is allowed, but the wave will spawn gradually as zombies die.");
                warnings++;
            }

            if (warnings == 0)
            {
                Debug.Log("v2.0 enemy wave spawner validation passed.");
            }
            else
            {
                Debug.LogWarning("v2.0 enemy wave spawner validation finished with " + warnings + " warning(s).");
            }
        }
    }
}

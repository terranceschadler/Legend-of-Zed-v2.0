using System.Collections.Generic;
using System.IO;
using LegendOfZed.Overworld;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LegendOfZed.Editor
{
    public static class ZedV28OverworldZombieSpawnPathTestSetup
    {
        private const string OverworldScenePath = "Assets/LegendOfZed/Scenes/Zed_Overworld.unity";
        private const string TestRootName = "Zed_Overworld_ZombiePathTest";
        private const string SpawnRootName = "Zed_Overworld_ZombiePathTest_SpawnPoints";
        private const string SpawnerName = "Zed_Overworld_ZombiePathTestSpawner";
        private const string PlayerName = "Zed_Portal_Test_Player";

        [MenuItem("Legend of Zed/Setup/Overworld/One Click Build Zombie Spawn Path Test")]
        public static void OneClickBuildZombieSpawnPathTest()
        {
            if (!File.Exists(OverworldScenePath))
            {
                Debug.LogWarning("Could not find overworld scene. Run immediate portal test first: " + OverworldScenePath);
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(OverworldScenePath, OpenSceneMode.Single);

            GameObject root = GetOrCreateRoot(TestRootName, scene);
            Transform[] spawnPoints = EnsureSpawnPoints(root.transform);
            ZedOverworldZombiePathTestSpawner spawner = EnsureSpawner(root.transform, spawnPoints);

            GameObject prefab = FindBestZombiePrefab();
            if (prefab != null)
            {
                spawner.ZombiePrefab = prefab;
                spawner.AllowFallbackCapsuleZombie = false;
                Debug.Log("v2.8b assigned zombie prefab: " + AssetDatabase.GetAssetPath(prefab));
            }
            else
            {
                spawner.ZombiePrefab = null;
                spawner.AllowFallbackCapsuleZombie = true;
                Debug.LogWarning("v2.8b could not find an existing zombie prefab. Fallback capsule zombies will be used for path testing.");
            }

            Transform target = FindTarget();
            spawner.Target = target;
            spawner.TargetObjectName = PlayerName;
            spawner.SpawnOnStart = true;
            spawner.SpawnCount = 3;
            spawner.SpawnRadius = 1.25f;
            spawner.NavMeshSampleDistance = 8f;
            spawner.AddNavMeshAgentIfMissing = true;
            spawner.AgentSpeed = 2.4f;
            spawner.AgentStoppingDistance = 1.35f;
            spawner.SpawnPoints = spawnPoints;

            EditorUtility.SetDirty(spawner.gameObject);
            EditorUtility.SetDirty(spawner);
            EditorUtility.SetDirty(root);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("v2.8b overworld zombie spawn/path test setup complete. SpawnPoints=" + spawnPoints.Length + " Target=" + (target != null ? target.name : "none") + " Prefab=" + (spawner.ZombiePrefab != null ? spawner.ZombiePrefab.name : "fallback capsules") + ".");
        }

        [MenuItem("Legend of Zed/Setup/Overworld/Rebuild Zombie Spawn Path Test Clean")]
        public static void RebuildZombieSpawnPathTestClean()
        {
            if (!File.Exists(OverworldScenePath))
            {
                Debug.LogWarning("Could not find overworld scene. Run immediate portal test first: " + OverworldScenePath);
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(OverworldScenePath, OpenSceneMode.Single);

            GameObject oldRoot = GameObject.Find(TestRootName);
            if (oldRoot != null)
            {
                Object.DestroyImmediate(oldRoot);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            OneClickBuildZombieSpawnPathTest();
        }

        [MenuItem("Legend of Zed/Setup/Overworld/Validate Zombie Spawn Path Test")]
        public static void ValidateZombieSpawnPathTest()
        {
            int warnings = 0;

            ZedOverworldRuntimeNavMeshBuilder builder = Object.FindAnyObjectByType<ZedOverworldRuntimeNavMeshBuilder>();
            if (builder == null)
            {
                Debug.LogWarning("Missing ZedOverworldRuntimeNavMeshBuilder. Run v2.7 NavMesh prototype setup first.");
                warnings++;
            }

            ZedOverworldZombiePathTestSpawner spawner = Object.FindAnyObjectByType<ZedOverworldZombiePathTestSpawner>();
            if (spawner == null)
            {
                Debug.LogWarning("Missing ZedOverworldZombiePathTestSpawner.");
                warnings++;
            }
            else
            {
                if (spawner.SpawnPoints == null || spawner.SpawnPoints.Length == 0)
                {
                    Debug.LogWarning("Zombie path test spawner has no spawn points.");
                    warnings++;
                }

                if (spawner.Target == null && GameObject.Find(PlayerName) == null)
                {
                    Debug.LogWarning("Zombie path test target player not found.");
                    warnings++;
                }

                if (spawner.ZombiePrefab == null && !spawner.AllowFallbackCapsuleZombie)
                {
                    Debug.LogWarning("Zombie path test has no prefab and fallback is disabled.");
                    warnings++;
                }
            }

            if (warnings == 0)
            {
                Debug.Log("v2.8b overworld zombie spawn/path test validation passed.");
            }
            else
            {
                Debug.LogWarning("v2.8b overworld zombie spawn/path test validation finished with " + warnings + " warning(s).");
            }
        }

        private static GameObject GetOrCreateRoot(string name, Scene scene)
        {
            GameObject root = GameObject.Find(name);
            if (root == null)
            {
                root = new GameObject(name);
                EditorSceneManager.MoveGameObjectToScene(root, scene);
            }

            root.transform.position = Vector3.zero;
            root.transform.rotation = Quaternion.identity;
            root.transform.localScale = Vector3.one;
            return root;
        }

        private static Transform[] EnsureSpawnPoints(Transform root)
        {
            Transform spawnRoot = root.Find(SpawnRootName);
            if (spawnRoot == null)
            {
                GameObject spawnRootObject = new GameObject(SpawnRootName);
                spawnRootObject.transform.SetParent(root, false);
                spawnRoot = spawnRootObject.transform;
            }

            Vector3[] positions =
            {
                new Vector3(6f, 0.4f, -18f),
                new Vector3(-6f, 0.4f, -18f),
                new Vector3(10f, 0.4f, -12f),
                new Vector3(-10f, 0.4f, -12f)
            };

            List<Transform> points = new List<Transform>();
            for (int i = 0; i < positions.Length; i++)
            {
                string name = "ZombiePathTestSpawn_" + (i + 1);
                Transform point = spawnRoot.Find(name);
                if (point == null)
                {
                    GameObject pointObject = new GameObject(name);
                    pointObject.transform.SetParent(spawnRoot, false);
                    point = pointObject.transform;
                }

                point.position = positions[i];

                Vector3 look = new Vector3(0f, 0f, -8f) - point.position;
                look.y = 0f;
                if (look.sqrMagnitude > 0.001f)
                {
                    point.rotation = Quaternion.LookRotation(look.normalized, Vector3.up);
                }

                points.Add(point);
                EditorUtility.SetDirty(point.gameObject);
            }

            EditorUtility.SetDirty(spawnRoot.gameObject);
            return points.ToArray();
        }

        private static ZedOverworldZombiePathTestSpawner EnsureSpawner(Transform root, Transform[] spawnPoints)
        {
            Transform existing = root.Find(SpawnerName);
            GameObject spawnerObject;
            if (existing == null)
            {
                spawnerObject = new GameObject(SpawnerName);
                spawnerObject.transform.SetParent(root, false);
            }
            else
            {
                spawnerObject = existing.gameObject;
            }

            ZedOverworldZombiePathTestSpawner spawner = spawnerObject.GetComponent<ZedOverworldZombiePathTestSpawner>();
            if (spawner == null)
            {
                spawner = spawnerObject.AddComponent<ZedOverworldZombiePathTestSpawner>();
            }

            spawner.SpawnPoints = spawnPoints;
            return spawner;
        }

        private static Transform FindTarget()
        {
            GameObject player = GameObject.Find(PlayerName);
            if (player != null)
            {
                return player.transform;
            }

            GameObject tagged = GameObject.FindGameObjectWithTag("Player");
            if (tagged != null)
            {
                return tagged.transform;
            }

            CharacterController controller = Object.FindAnyObjectByType<CharacterController>();
            return controller != null ? controller.transform : null;
        }

        private static GameObject FindBestZombiePrefab()
        {
            string[] preferredNames =
            {
                "Zed_Zombie_Enemy_Baseline",
                "Zed_Zombie_Enemy",
                "Zed_Prototype_Zombie",
                "Zombie_Enemy",
                "Zombie"
            };

            for (int i = 0; i < preferredNames.Length; i++)
            {
                string[] guids = AssetDatabase.FindAssets(preferredNames[i] + " t:Prefab", new[] { "Assets" });
                GameObject prefab = FirstValidZombiePrefab(guids);
                if (prefab != null)
                {
                    return prefab;
                }
            }

            string[] allPrefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });
            return FirstValidZombiePrefab(allPrefabGuids);
        }

        private static GameObject FirstValidZombiePrefab(string[] guids)
        {
            if (guids == null)
            {
                return null;
            }

            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (string.IsNullOrEmpty(path) || path.Contains("/Editor/") || path.Contains("/ReadMeDocs/"))
                {
                    continue;
                }

                string lowerPath = path.ToLowerInvariant();
                if (!lowerPath.Contains("zombie") && !lowerPath.Contains("zed"))
                {
                    continue;
                }

                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null)
                {
                    continue;
                }

                if (PrefabLooksLikeZombie(prefab))
                {
                    return prefab;
                }
            }

            return null;
        }

        private static bool PrefabLooksLikeZombie(GameObject prefab)
        {
            if (prefab == null)
            {
                return false;
            }

            string name = prefab.name.ToLowerInvariant();
            if (name.Contains("zombie"))
            {
                return true;
            }

            Component[] components = prefab.GetComponentsInChildren<Component>(true);
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] == null)
                {
                    continue;
                }

                string typeName = components[i].GetType().Name.ToLowerInvariant();
                if (typeName.Contains("zombie") || typeName.Contains("enemy"))
                {
                    return true;
                }
            }

            return false;
        }
    }
}

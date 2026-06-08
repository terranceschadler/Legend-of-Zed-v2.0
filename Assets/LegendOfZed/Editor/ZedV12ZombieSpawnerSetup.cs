using System.IO;
using LegendOfZed.Enemies;
using TopDownShooter;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LegendOfZed.Editor
{
    public static class ZedV12ZombieSpawnerSetup
    {
        private const string ScenePath = "Assets/LegendOfZed/Scenes/Zed_Controller_Test.unity";
        private const string PrototypeName = "Zed_Prototype_Zombie_Enemy";
        private const string SpawnerName = "Zed_Zombie_Spawner";
        private const string SpawnRootName = "Zed_Zombie_SpawnPoints";

        [MenuItem("Legend of Zed/Setup/v1.2 Add Zombie Spawner")]
        public static void AddZombieSpawner()
        {
            if (!File.Exists(ScenePath))
            {
                Debug.LogWarning("Could not find scene: " + ScenePath);
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            GameObject prototype = GameObject.Find(PrototypeName);
            if (prototype == null)
            {
                Debug.LogWarning("Could not find " + PrototypeName + ". Run the zombie prototype/ragdoll setup first.");
                return;
            }

            RemoveDemoHitPoints(prototype);
            RemoveNavMeshAgentIfPresent(prototype);

            PlayerController player = Object.FindFirstObjectByType<PlayerController>();
            Vector3 center = player != null ? player.transform.position : Vector3.zero;

            GameObject spawnRoot = GameObject.Find(SpawnRootName);
            if (spawnRoot == null)
            {
                spawnRoot = new GameObject(SpawnRootName);
                EditorSceneManager.MoveGameObjectToScene(spawnRoot, scene);
            }

            Transform[] points = BuildSpawnPoints(spawnRoot.transform, center);

            GameObject spawnerObject = GameObject.Find(SpawnerName);
            if (spawnerObject == null)
            {
                spawnerObject = new GameObject(SpawnerName);
                EditorSceneManager.MoveGameObjectToScene(spawnerObject, scene);
            }

            ZedZombieSpawner spawner = spawnerObject.GetComponent<ZedZombieSpawner>();
            if (spawner == null)
            {
                spawner = spawnerObject.AddComponent<ZedZombieSpawner>();
            }

            spawner.ZombiePrototype = prototype;
            spawner.PlayerTarget = player != null ? player.transform : null;
            spawner.SpawnPoints.Clear();
            spawner.SpawnPoints.AddRange(points);
            spawner.SpawnOnStart = true;
            spawner.MaxAliveZombies = 5;
            spawner.HidePrototypeOnStart = true;
            spawner.ZombieHealth = 60f;
            spawner.DeathDisableDelay = 999f;
            spawner.RagdollOnDeath = true;

            prototype.SetActive(true);

            EditorUtility.SetDirty(prototype);
            EditorUtility.SetDirty(spawnRoot);
            EditorUtility.SetDirty(spawnerObject);
            EditorUtility.SetDirty(spawner);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("v1.2 zombie spawner added. It will hide the prototype on Play and spawn 5 zombies from scene spawn points.");
        }

        private static Transform[] BuildSpawnPoints(Transform root, Vector3 center)
        {
            Vector3[] offsets =
            {
                new Vector3(5f, 0f, 5f),
                new Vector3(-5f, 0f, 5f),
                new Vector3(7f, 0f, 0f),
                new Vector3(-7f, 0f, 0f),
                new Vector3(0f, 0f, 7f)
            };

            Transform[] points = new Transform[offsets.Length];

            for (int i = 0; i < offsets.Length; i++)
            {
                string pointName = "ZombieSpawnPoint_" + (i + 1);
                Transform point = root.Find(pointName);
                if (point == null)
                {
                    GameObject pointObject = new GameObject(pointName);
                    pointObject.transform.SetParent(root, false);
                    point = pointObject.transform;
                }

                point.position = SnapToGround(center + offsets[i]);
                Vector3 lookDirection = center - point.position;
                lookDirection.y = 0f;
                if (lookDirection.sqrMagnitude > 0.001f)
                {
                    point.rotation = Quaternion.LookRotation(lookDirection.normalized, Vector3.up);
                }

                points[i] = point;
            }

            return points;
        }

        private static Vector3 SnapToGround(Vector3 position)
        {
            Vector3 origin = position + Vector3.up * 6f;
            RaycastHit hit;
            if (Physics.Raycast(origin, Vector3.down, out hit, 20f, ~0, QueryTriggerInteraction.Ignore))
            {
                position.y = hit.point.y;
            }

            return position;
        }

        private static void RemoveDemoHitPoints(GameObject root)
        {
            HitPoint[] hitPoints = root.GetComponentsInChildren<HitPoint>(true);
            for (int i = 0; i < hitPoints.Length; i++)
            {
                Object.DestroyImmediate(hitPoints[i], true);
            }
        }

        private static void RemoveNavMeshAgentIfPresent(GameObject root)
        {
            UnityEngine.AI.NavMeshAgent agent = root.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null)
            {
                Object.DestroyImmediate(agent, true);
            }
        }
    }
}

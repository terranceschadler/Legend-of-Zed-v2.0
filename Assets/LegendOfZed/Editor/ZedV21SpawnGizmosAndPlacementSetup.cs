using System.Collections.Generic;
using System.IO;
using LegendOfZed.Enemies;
using LegendOfZed.Spawning;
using TopDownShooter;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LegendOfZed.Editor
{
    public static class ZedV21SpawnGizmosAndPlacementSetup
    {
        private const string ScenePath = "Assets/LegendOfZed/Scenes/Zed_Controller_Test.unity";
        private const string SpawnerName = "Zed_Zombie_Spawner";
        private const string SpawnRootName = "Zed_Zombie_SpawnPoints";

        [MenuItem("Legend of Zed/Setup/v2.1 Add Spawn Point Gizmos")]
        public static void AddSpawnPointGizmos()
        {
            if (!OpenScene())
            {
                return;
            }

            ZedZombieSpawner spawner = FindSpawner();
            if (spawner == null)
            {
                Debug.LogWarning("Could not find ZedZombieSpawner. Run v2.0 Apply Enemy Wave Spawn Defaults first.");
                return;
            }

            int updated = 0;
            if (spawner.SpawnPoints != null)
            {
                for (int i = 0; i < spawner.SpawnPoints.Count; i++)
                {
                    Transform point = spawner.SpawnPoints[i];
                    if (point == null)
                    {
                        continue;
                    }

                    ZedZombieSpawnPointMarker marker = point.GetComponent<ZedZombieSpawnPointMarker>();
                    if (marker == null)
                    {
                        marker = point.gameObject.AddComponent<ZedZombieSpawnPointMarker>();
                    }

                    marker.SpawnLabel = "Zombie Spawn " + (i + 1);
                    marker.SpawnRadius = 0.75f;
                    marker.DrawGizmos = true;
                    marker.DrawLabel = true;

                    EditorUtility.SetDirty(marker);
                    EditorUtility.SetDirty(point.gameObject);
                    updated++;
                }
            }

            SaveScene();
            Debug.Log("v2.1 spawn point gizmos added/updated. Spawn points updated=" + updated + ".");
        }

        [MenuItem("Legend of Zed/Setup/v2.1 Rebuild 8 Arena Spawn Points")]
        public static void RebuildEightArenaSpawnPoints()
        {
            if (!OpenScene())
            {
                return;
            }

            ZedZombieSpawner spawner = FindSpawner();
            if (spawner == null)
            {
                Debug.LogWarning("Could not find ZedZombieSpawner. Run v2.0 Apply Enemy Wave Spawn Defaults first.");
                return;
            }

            PlayerController player = Object.FindFirstObjectByType<PlayerController>();
            Vector3 center = player != null ? player.transform.position : Vector3.zero;

            GameObject root = GameObject.Find(SpawnRootName);
            if (root == null)
            {
                root = new GameObject(SpawnRootName);
                EditorSceneManager.MoveGameObjectToScene(root, SceneManager.GetActiveScene());
            }

            Vector3[] offsets =
            {
                new Vector3(6f, 0f, 6f),
                new Vector3(-6f, 0f, 6f),
                new Vector3(6f, 0f, -6f),
                new Vector3(-6f, 0f, -6f),
                new Vector3(9f, 0f, 0f),
                new Vector3(-9f, 0f, 0f),
                new Vector3(0f, 0f, 9f),
                new Vector3(0f, 0f, -9f)
            };

            List<Transform> points = new List<Transform>();
            for (int i = 0; i < offsets.Length; i++)
            {
                string pointName = "ZombieSpawnPoint_" + (i + 1);
                Transform point = root.transform.Find(pointName);
                if (point == null)
                {
                    GameObject pointObject = new GameObject(pointName);
                    pointObject.transform.SetParent(root.transform, false);
                    point = pointObject.transform;
                }

                point.position = SnapToGround(center + offsets[i]);

                Vector3 look = center - point.position;
                look.y = 0f;
                if (look.sqrMagnitude > 0.001f)
                {
                    point.rotation = Quaternion.LookRotation(look.normalized, Vector3.up);
                }

                ZedZombieSpawnPointMarker marker = point.GetComponent<ZedZombieSpawnPointMarker>();
                if (marker == null)
                {
                    marker = point.gameObject.AddComponent<ZedZombieSpawnPointMarker>();
                }

                marker.SpawnLabel = "Zombie Spawn " + (i + 1);
                marker.SpawnRadius = 0.75f;
                marker.DrawGizmos = true;
                marker.DrawLabel = true;

                points.Add(point);
                EditorUtility.SetDirty(marker);
                EditorUtility.SetDirty(point.gameObject);
            }

            spawner.SpawnPoints.Clear();
            spawner.SpawnPoints.AddRange(points);
            spawner.ZombiesPerWave = Mathf.Min(Mathf.Max(spawner.ZombiesPerWave, 5), points.Count);
            spawner.MaxAliveZombies = Mathf.Max(spawner.MaxAliveZombies, spawner.ZombiesPerWave);

            EditorUtility.SetDirty(root);
            EditorUtility.SetDirty(spawner);
            SaveScene();

            Debug.Log("v2.1 rebuilt 8 arena zombie spawn points and assigned them to ZedZombieSpawner.");
        }

        [MenuItem("Legend of Zed/Setup/v2.1 Select Spawn Point Root")]
        public static void SelectSpawnPointRoot()
        {
            GameObject root = GameObject.Find(SpawnRootName);
            if (root == null)
            {
                Debug.LogWarning("Could not find spawn point root: " + SpawnRootName);
                return;
            }

            Selection.activeGameObject = root;
            EditorGUIUtility.PingObject(root);
        }

        private static bool OpenScene()
        {
            if (!File.Exists(ScenePath))
            {
                Debug.LogWarning("Could not find scene: " + ScenePath);
                return false;
            }

            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            return true;
        }

        private static ZedZombieSpawner FindSpawner()
        {
            GameObject spawnerObject = GameObject.Find(SpawnerName);
            if (spawnerObject == null)
            {
                return Object.FindFirstObjectByType<ZedZombieSpawner>();
            }

            return spawnerObject.GetComponent<ZedZombieSpawner>();
        }

        private static Vector3 SnapToGround(Vector3 position)
        {
            Vector3 origin = position + Vector3.up * 8f;
            RaycastHit hit;
            if (Physics.Raycast(origin, Vector3.down, out hit, 30f, ~0, QueryTriggerInteraction.Ignore))
            {
                position.y = hit.point.y;
            }

            return position;
        }

        private static void SaveScene()
        {
            Scene scene = SceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}

using System.IO;
using System.Reflection;
using LegendOfZed.Enemies;
using LegendOfZed.Overworld;
using LegendOfZed.Spawning;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LegendOfZed.Editor
{
    public static class ZedV27OverworldNavMeshBakePrototypeSetup
    {
        private const string OverworldScenePath = "Assets/LegendOfZed/Scenes/Zed_Overworld.unity";
        private const string NavRootName = "Zed_Overworld_NavMesh_Prototype";
        private const string SpawnRootName = "Zed_Overworld_ZombieSpawnPoints";
        private const string WalkableFloorName = "Zed_Overworld_Test_Walkable_Floor";

        [MenuItem("Legend of Zed/Setup/Overworld/One Click Build NavMesh Prototype")]
        public static void OneClickBuildNavMeshPrototype()
        {
            if (!File.Exists(OverworldScenePath))
            {
                Debug.LogWarning("Could not find overworld scene. Run the immediate portal test setup first: " + OverworldScenePath);
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(OverworldScenePath, OpenSceneMode.Single);

            EnsureWalkableFloor(scene);
            ZedOverworldRuntimeNavMeshBuilder builder = EnsureNavMeshBuilder(scene);
            EnsureZombieSpawnPoints(scene);
            ConnectZombieSpawnerSafely();

            EditorUtility.SetDirty(builder);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("v2.7 overworld NavMesh prototype setup complete. Press Play to build the runtime NavMesh, then test zombie spawning/pathing.");
        }

        [MenuItem("Legend of Zed/Setup/Overworld/Validate NavMesh Prototype")]
        public static void ValidateNavMeshPrototype()
        {
            int warnings = 0;

            ZedOverworldRuntimeNavMeshBuilder builder = Object.FindAnyObjectByType<ZedOverworldRuntimeNavMeshBuilder>();
            if (builder == null)
            {
                Debug.LogWarning("Missing ZedOverworldRuntimeNavMeshBuilder.");
                warnings++;
            }

            GameObject floor = GameObject.Find(WalkableFloorName);
            if (floor == null)
            {
                Debug.LogWarning("Missing guaranteed walkable floor.");
                warnings++;
            }

            GameObject spawnRoot = GameObject.Find(SpawnRootName);
            if (spawnRoot == null || spawnRoot.transform.childCount == 0)
            {
                Debug.LogWarning("Missing overworld zombie spawn points.");
                warnings++;
            }

            ZedZombieSpawner spawner = Object.FindAnyObjectByType<ZedZombieSpawner>();
            if (spawner == null)
            {
                Debug.LogWarning("No ZedZombieSpawner found in scene. NavMesh can still build, but zombie pathing cannot be tested from this scene yet.");
                warnings++;
            }
            else if (spawner.SpawnPoints == null || spawner.SpawnPoints.Count == 0)
            {
                Debug.LogWarning("ZedZombieSpawner has no spawn points assigned.");
                warnings++;
            }

            if (warnings == 0)
            {
                Debug.Log("v2.7 overworld NavMesh prototype validation passed.");
            }
            else
            {
                Debug.LogWarning("v2.7 overworld NavMesh prototype validation finished with " + warnings + " warning(s).");
            }
        }

        private static void EnsureWalkableFloor(Scene scene)
        {
            GameObject floor = GameObject.Find(WalkableFloorName);
            if (floor == null)
            {
                floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
                floor.name = WalkableFloorName;
                EditorSceneManager.MoveGameObjectToScene(floor, scene);
            }

            floor.SetActive(true);
            floor.transform.position = new Vector3(0f, -0.08f, 0f);
            floor.transform.rotation = Quaternion.identity;
            floor.transform.localScale = new Vector3(210f, 0.12f, 210f);

            Collider collider = floor.GetComponent<Collider>();
            if (collider == null)
            {
                collider = floor.AddComponent<BoxCollider>();
            }

            collider.isTrigger = false;

            Renderer renderer = floor.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.enabled = false;
                EditorUtility.SetDirty(renderer);
            }

            EditorUtility.SetDirty(floor);
            EditorUtility.SetDirty(collider);
        }

        private static ZedOverworldRuntimeNavMeshBuilder EnsureNavMeshBuilder(Scene scene)
        {
            GameObject root = GameObject.Find(NavRootName);
            if (root == null)
            {
                root = new GameObject(NavRootName);
                EditorSceneManager.MoveGameObjectToScene(root, scene);
            }

            ZedOverworldRuntimeNavMeshBuilder builder = root.GetComponent<ZedOverworldRuntimeNavMeshBuilder>();
            if (builder == null)
            {
                builder = root.AddComponent<ZedOverworldRuntimeNavMeshBuilder>();
            }

            builder.BuildOnStart = true;
            builder.BoundsCenter = Vector3.zero;
            builder.BoundsSize = new Vector3(230f, 24f, 230f);
            builder.IncludeAllSceneMeshes = false;
            builder.WalkableTag = "";
            builder.IncludedLayers = ~0;
            builder.LogBuild = true;

            return builder;
        }

        private static void EnsureZombieSpawnPoints(Scene scene)
        {
            GameObject root = GameObject.Find(SpawnRootName);
            if (root == null)
            {
                root = new GameObject(SpawnRootName);
                EditorSceneManager.MoveGameObjectToScene(root, scene);
            }

            Vector3[] positions =
            {
                new Vector3(10f, 0.25f, -18f),
                new Vector3(-10f, 0.25f, -18f),
                new Vector3(18f, 0.25f, 0f),
                new Vector3(-18f, 0.25f, 0f),
                new Vector3(12f, 0.25f, 16f),
                new Vector3(-12f, 0.25f, 16f)
            };

            for (int i = 0; i < positions.Length; i++)
            {
                string name = "OverworldZombieSpawn_" + (i + 1);
                Transform existing = root.transform.Find(name);
                GameObject point = existing != null ? existing.gameObject : new GameObject(name);

                point.transform.SetParent(root.transform, false);
                point.transform.position = positions[i];

                Vector3 look = Vector3.zero - point.transform.position;
                look.y = 0f;
                if (look.sqrMagnitude > 0.001f)
                {
                    point.transform.rotation = Quaternion.LookRotation(look.normalized, Vector3.up);
                }

                ZedZombieSpawnPointMarker marker = point.GetComponent<ZedZombieSpawnPointMarker>();
                if (marker == null)
                {
                    marker = point.AddComponent<ZedZombieSpawnPointMarker>();
                }

                marker.SpawnLabel = "Overworld Spawn " + (i + 1);
                marker.SpawnRadius = 1.25f;

                EditorUtility.SetDirty(point);
                EditorUtility.SetDirty(marker);
            }
        }

        private static void ConnectZombieSpawnerSafely()
        {
            ZedZombieSpawner spawner = Object.FindAnyObjectByType<ZedZombieSpawner>();
            if (spawner == null)
            {
                Debug.Log("v2.7 did not find a ZedZombieSpawner in Zed_Overworld. Spawn points were created, NavMesh can still be tested.");
                return;
            }

            GameObject spawnRoot = GameObject.Find(SpawnRootName);
            if (spawnRoot == null)
            {
                return;
            }

            if (spawner.SpawnPoints != null)
            {
                spawner.SpawnPoints.Clear();
                for (int i = 0; i < spawnRoot.transform.childCount; i++)
                {
                    Transform child = spawnRoot.transform.GetChild(i);
                    if (child != null)
                    {
                        spawner.SpawnPoints.Add(child);
                    }
                }
            }

            SetBoolIfExists(spawner, "UseNavMeshAgentWhenAvailable", true);
            SetBoolIfExists(spawner, "UseNavMeshAgents", true);
            SetFloatIfExists(spawner, "NavMeshSampleDistance", 6f);
            SetBoolIfExists(spawner, "SpawnOnStart", true);
            SetIntMinIfExists(spawner, "MaxAliveZombies", 5);
            SetIntMinIfExists(spawner, "ZombiesPerWave", 5);

            EditorUtility.SetDirty(spawner);
            Debug.Log("v2.7 connected existing ZedZombieSpawner to overworld spawn points using compatibility-safe field assignment.");
        }

        private static void SetBoolIfExists(object target, string memberName, bool value)
        {
            FieldInfo field = target.GetType().GetField(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field != null && field.FieldType == typeof(bool))
            {
                field.SetValue(target, value);
                return;
            }

            PropertyInfo property = target.GetType().GetProperty(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (property != null && property.PropertyType == typeof(bool) && property.CanWrite)
            {
                property.SetValue(target, value, null);
            }
        }

        private static void SetFloatIfExists(object target, string memberName, float value)
        {
            FieldInfo field = target.GetType().GetField(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field != null && field.FieldType == typeof(float))
            {
                field.SetValue(target, value);
                return;
            }

            PropertyInfo property = target.GetType().GetProperty(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (property != null && property.PropertyType == typeof(float) && property.CanWrite)
            {
                property.SetValue(target, value, null);
            }
        }

        private static void SetIntMinIfExists(object target, string memberName, int minimum)
        {
            FieldInfo field = target.GetType().GetField(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field != null && field.FieldType == typeof(int))
            {
                int current = (int)field.GetValue(target);
                field.SetValue(target, Mathf.Max(current, minimum));
                return;
            }

            PropertyInfo property = target.GetType().GetProperty(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (property != null && property.PropertyType == typeof(int) && property.CanRead && property.CanWrite)
            {
                int current = (int)property.GetValue(target, null);
                property.SetValue(target, Mathf.Max(current, minimum), null);
            }
        }
    }
}

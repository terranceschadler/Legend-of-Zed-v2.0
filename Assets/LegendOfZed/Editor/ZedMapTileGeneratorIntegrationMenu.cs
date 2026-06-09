#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using LegendOfZed.LegacyMapGenerator;
using LegendOfZed.MapIntegration;

namespace LegendOfZed.Editor
{
    public static class ZedMapTileGeneratorIntegrationMenu
    {
        private const string RoomTilesFolder = "Assets/LegendOfZed/MapGeneratorImport/Prefabs/RoomTiles";

        [MenuItem("Legend of Zed/Map Integration/Add Runtime Bridge To Scene")]
        public static void AddRuntimeBridgeToScene()
        {
            EnsureTagExists("MapGenerator");
            EnsureTagExists("TileSpawn");
            EnsureTagExists("RoomTile");

            ZedLegacyRandomMapGenerator generator = UnityEngine.Object.FindObjectOfType<ZedLegacyRandomMapGenerator>();
            if (generator == null)
            {
                GameObject generatorObject = new GameObject("Zed_Legacy_MapTile_Generator");
                Undo.RegisterCreatedObjectUndo(generatorObject, "Create legacy map tile generator");
                generatorObject.transform.position = Vector3.zero;
                generatorObject.tag = "MapGenerator";
                generator = generatorObject.AddComponent<ZedLegacyRandomMapGenerator>();
            }

            RepairGeneratorReferences(generator);

            ZedMapTileGeneratorRuntimeBridge bridge = UnityEngine.Object.FindObjectOfType<ZedMapTileGeneratorRuntimeBridge>();
            if (bridge == null)
            {
                GameObject bridgeObject = new GameObject("Zed_MapTileGenerator_RuntimeBridge");
                Undo.RegisterCreatedObjectUndo(bridgeObject, "Create Zed map tile runtime bridge");
                bridge = bridgeObject.AddComponent<ZedMapTileGeneratorRuntimeBridge>();
            }

            Selection.activeObject = bridge.gameObject;
            EditorGUIUtility.PingObject(bridge.gameObject);
            EditorSceneManagerHelper.MarkActiveSceneDirty();

            Debug.Log("Map tile generator scene setup complete. Generator prefab references repaired from imported RoomTiles folder. Save the scene, then press Play.", bridge.gameObject);
        }

        [MenuItem("Legend of Zed/Map Integration/Repair Legacy Generator Tile References")]
        public static void RepairSelectedOrSceneGenerator()
        {
            ZedLegacyRandomMapGenerator generator = null;
            if (Selection.activeGameObject != null)
            {
                generator = Selection.activeGameObject.GetComponent<ZedLegacyRandomMapGenerator>();
            }

            if (generator == null)
            {
                generator = UnityEngine.Object.FindObjectOfType<ZedLegacyRandomMapGenerator>();
            }

            if (generator == null)
            {
                Debug.LogWarning("No ZedLegacyRandomMapGenerator found in the current scene.");
                return;
            }

            RepairGeneratorReferences(generator);
            EditorUtility.SetDirty(generator);
            EditorSceneManagerHelper.MarkActiveSceneDirty();
        }

        private static void RepairGeneratorReferences(ZedLegacyRandomMapGenerator generator)
        {
            if (generator == null)
            {
                return;
            }

            generator.tileCount = Mathf.Max(1, generator.tileCount <= 0 ? 22 : generator.tileCount);
            generator.startingTile = LoadRoomTileExact("RoomTile-4Way");
            generator.deadEndTile = LoadRoomTileExact("RoomTile-DeadEnd");
            generator.tilePrefabs = LoadRandomRoomTiles(generator.startingTile, generator.deadEndTile);

            EditorUtility.SetDirty(generator);

            Debug.Log("Legacy map tile generator references repaired. " +
                      "StartingTile=" + NameOrMissing(generator.startingTile) + ", " +
                      "DeadEndTile=" + NameOrMissing(generator.deadEndTile) + ", " +
                      "RandomTiles=" + (generator.tilePrefabs == null ? 0 : generator.tilePrefabs.Length) + ".", generator);
        }

        private static GameObject LoadRoomTileExact(string prefabName)
        {
            string[] guids = AssetDatabase.FindAssets(prefabName + " t:Prefab", new[] { RoomTilesFolder });
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null && prefab.name == prefabName)
                {
                    return prefab;
                }
            }

            Debug.LogWarning("Missing required room tile prefab: " + prefabName + " under " + RoomTilesFolder);
            return null;
        }

        private static GameObject[] LoadRandomRoomTiles(GameObject startingTile, GameObject deadEndTile)
        {
            string[] guids = AssetDatabase.FindAssets("RoomTile t:Prefab", new[] { RoomTilesFolder });
            List<GameObject> prefabs = new List<GameObject>();

            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null)
                {
                    continue;
                }

                if (!prefab.name.StartsWith("RoomTile-", StringComparison.Ordinal))
                {
                    continue;
                }

                if (deadEndTile != null && prefab == deadEndTile)
                {
                    continue;
                }

                if (!prefabs.Contains(prefab))
                {
                    prefabs.Add(prefab);
                }
            }

            // If AssetDatabase search is stale, fall back to the known imported prefab names visible in the package.
            if (prefabs.Count == 0)
            {
                AddIfFound(prefabs, "RoomTile-2Way-Left");
                AddIfFound(prefabs, "RoomTile-2Way-Right");
                AddIfFound(prefabs, "RoomTile-2Way-Straight");
                AddIfFound(prefabs, "RoomTile-3Way-Left");
                AddIfFound(prefabs, "RoomTile-3Way-Right");
                AddIfFound(prefabs, "RoomTile-3Way-Straight");
                AddIfFound(prefabs, "RoomTile-4Way");
            }

            if (prefabs.Count == 0 && startingTile != null)
            {
                prefabs.Add(startingTile);
            }

            return prefabs.ToArray();
        }

        private static void AddIfFound(List<GameObject> prefabs, string prefabName)
        {
            GameObject prefab = LoadRoomTileExact(prefabName);
            if (prefab != null && !prefabs.Contains(prefab))
            {
                prefabs.Add(prefab);
            }
        }

        private static string NameOrMissing(GameObject prefab)
        {
            return prefab == null ? "MISSING" : prefab.name;
        }

        private static void EnsureTagExists(string tag)
        {
            UnityEngine.Object[] tagAssets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            if (tagAssets == null || tagAssets.Length == 0)
            {
                return;
            }

            SerializedObject tagManager = new SerializedObject(tagAssets[0]);
            SerializedProperty tags = tagManager.FindProperty("tags");
            if (tags == null)
            {
                return;
            }

            for (int i = 0; i < tags.arraySize; i++)
            {
                if (tags.GetArrayElementAtIndex(i).stringValue == tag)
                {
                    return;
                }
            }

            tags.InsertArrayElementAtIndex(tags.arraySize);
            tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tag;
            tagManager.ApplyModifiedProperties();
        }

        private static class EditorSceneManagerHelper
        {
            public static void MarkActiveSceneDirty()
            {
                Type sceneManagerType = Type.GetType("UnityEditor.SceneManagement.EditorSceneManager, UnityEditor");
                if (sceneManagerType == null) return;

                MethodInfo getActiveScene = typeof(UnityEngine.SceneManagement.SceneManager).GetMethod("GetActiveScene", BindingFlags.Public | BindingFlags.Static);
                MethodInfo markSceneDirty = sceneManagerType.GetMethod("MarkSceneDirty", BindingFlags.Public | BindingFlags.Static);
                if (getActiveScene == null || markSceneDirty == null) return;

                object activeScene = getActiveScene.Invoke(null, null);
                markSceneDirty.Invoke(null, new[] { activeScene });
            }
        }
    }
}
#endif

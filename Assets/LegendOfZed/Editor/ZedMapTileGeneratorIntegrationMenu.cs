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
        private const string ParkTilesFolder = "Assets/LegendOfZed/MapGeneratorImport/Prefabs/ParkTiles";

        private const int DefaultCityWeight = 10;
        private const int DefaultParkWeight = 2;

        [MenuItem("Legend of Zed/Map Integration/Add Runtime Bridge To Scene")]
        public static void AddRuntimeBridgeToScene()
        {
            EnsureTagExists("MapGenerator");
            EnsureTagExists("TileSpawn");
            EnsureTagExists("RoomTile");
            EnsureTagExists("Player");

            ZedLegacyRandomMapGenerator generator = UnityEngine.Object.FindAnyObjectByType<ZedLegacyRandomMapGenerator>();
            if (generator == null)
            {
                GameObject generatorObject = new GameObject("Zed_Legacy_MapTile_Generator");
                Undo.RegisterCreatedObjectUndo(generatorObject, "Create legacy map tile generator");
                generatorObject.transform.position = Vector3.zero;
                generatorObject.tag = "MapGenerator";
                generator = generatorObject.AddComponent<ZedLegacyRandomMapGenerator>();
            }

            RepairGeneratorReferences(generator);

            ZedMapTileGeneratorRuntimeBridge bridge = UnityEngine.Object.FindAnyObjectByType<ZedMapTileGeneratorRuntimeBridge>();
            if (bridge == null)
            {
                GameObject bridgeObject = new GameObject("Zed_MapTileGenerator_RuntimeBridge");
                Undo.RegisterCreatedObjectUndo(bridgeObject, "Create Zed map tile runtime bridge");
                bridge = bridgeObject.AddComponent<ZedMapTileGeneratorRuntimeBridge>();
            }

            AssignBridgeReferences(bridge, generator);

            Selection.activeObject = bridge.gameObject;
            EditorGUIUtility.PingObject(bridge.gameObject);
            EditorSceneManagerHelper.MarkActiveSceneDirty();

            Debug.Log("Map tile generator scene setup complete. Generator now uses weighted city/park tile selection. Assign Player Prefab on the bridge if no Player exists in the scene.", bridge.gameObject);
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
                generator = UnityEngine.Object.FindAnyObjectByType<ZedLegacyRandomMapGenerator>();
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
            generator.startingTile = LoadPrefabExact(RoomTilesFolder, "RoomTile-4Way");
            generator.deadEndTile = LoadPrefabExact(RoomTilesFolder, "RoomTile-DeadEnd");
            generator.tilePrefabs = LoadRandomRoomTiles(generator.startingTile, generator.deadEndTile);
            generator.useWeightedTilePrefabs = true;
            generator.weightedTilePrefabs = BuildWeightedTileList(generator.tilePrefabs);

            EditorUtility.SetDirty(generator);

            int parkCount = CountWeightedPrefabsWithPrefix(generator.weightedTilePrefabs, "ParkTile-");
            int cityCount = CountWeightedPrefabsWithPrefix(generator.weightedTilePrefabs, "RoomTile-");

            Debug.Log("Legacy map tile generator references repaired. " +
                      "StartingTile=" + NameOrMissing(generator.startingTile) + ", " +
                      "DeadEndTile=" + NameOrMissing(generator.deadEndTile) + ", " +
                      "LegacyRandomTiles=" + (generator.tilePrefabs == null ? 0 : generator.tilePrefabs.Length) + ", " +
                      "WeightedCityTiles=" + cityCount + ", " +
                      "WeightedParkTiles=" + parkCount + ".", generator);
        }

        private static ZedLegacyRandomMapGenerator.WeightedTilePrefab[] BuildWeightedTileList(GameObject[] cityTiles)
        {
            List<ZedLegacyRandomMapGenerator.WeightedTilePrefab> weighted = new List<ZedLegacyRandomMapGenerator.WeightedTilePrefab>();

            if (cityTiles != null)
            {
                for (int i = 0; i < cityTiles.Length; i++)
                {
                    GameObject prefab = cityTiles[i];
                    if (prefab == null || prefab.name == "RoomTile-DeadEnd")
                    {
                        continue;
                    }

                    AddWeighted(weighted, prefab, DefaultCityWeight);
                }
            }

            AddParkTiles(weighted);

            return weighted.ToArray();
        }

        private static void AddParkTiles(List<ZedLegacyRandomMapGenerator.WeightedTilePrefab> weighted)
        {
            if (!AssetDatabase.IsValidFolder(ParkTilesFolder))
            {
                Debug.Log("No ParkTiles folder found yet. Weighted generation will use city tiles only until park prefabs exist at " + ParkTilesFolder + ".");
                return;
            }

            string[] guids = AssetDatabase.FindAssets("ParkTile t:Prefab", new[] { ParkTilesFolder });
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null || !prefab.name.StartsWith("ParkTile-", StringComparison.Ordinal))
                {
                    continue;
                }

                if (prefab.name == "ParkTile-DeadEnd")
                {
                    continue;
                }

                AddWeighted(weighted, prefab, DefaultParkWeight);
            }
        }

        private static void AddWeighted(List<ZedLegacyRandomMapGenerator.WeightedTilePrefab> weighted, GameObject prefab, int weight)
        {
            if (prefab == null)
            {
                return;
            }

            for (int i = 0; i < weighted.Count; i++)
            {
                if (weighted[i] != null && weighted[i].prefab == prefab)
                {
                    return;
                }
            }

            weighted.Add(new ZedLegacyRandomMapGenerator.WeightedTilePrefab
            {
                prefab = prefab,
                weight = Mathf.Max(0, weight)
            });
        }

        private static GameObject LoadPrefabExact(string folder, string prefabName)
        {
            string[] guids = AssetDatabase.FindAssets(prefabName + " t:Prefab", new[] { folder });
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null && prefab.name == prefabName)
                {
                    return prefab;
                }
            }

            Debug.LogWarning("Missing required prefab: " + prefabName + " under " + folder);
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
            GameObject prefab = LoadPrefabExact(RoomTilesFolder, prefabName);
            if (prefab != null && !prefabs.Contains(prefab))
            {
                prefabs.Add(prefab);
            }
        }

        private static void AssignBridgeReferences(ZedMapTileGeneratorRuntimeBridge bridge, ZedLegacyRandomMapGenerator generator)
        {
            if (bridge == null)
            {
                return;
            }

            SerializedObject serializedBridge = new SerializedObject(bridge);

            SerializedProperty mapGeneratorProp = serializedBridge.FindProperty("mapGenerator");
            if (mapGeneratorProp != null)
            {
                mapGeneratorProp.objectReferenceValue = generator;
            }

            SerializedProperty cameraProp = serializedBridge.FindProperty("gameplayCamera");
            if (cameraProp != null && cameraProp.objectReferenceValue == null)
            {
                cameraProp.objectReferenceValue = Camera.main;
            }

            SerializedProperty playerProp = serializedBridge.FindProperty("player");
            if (playerProp != null && playerProp.objectReferenceValue == null)
            {
                GameObject taggedPlayer = FindGameObjectWithTagSafe("Player");
                if (taggedPlayer != null)
                {
                    playerProp.objectReferenceValue = taggedPlayer.transform;
                }
            }

            serializedBridge.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(bridge);
        }

        private static int CountWeightedPrefabsWithPrefix(ZedLegacyRandomMapGenerator.WeightedTilePrefab[] prefabs, string prefix)
        {
            if (prefabs == null)
            {
                return 0;
            }

            int count = 0;
            for (int i = 0; i < prefabs.Length; i++)
            {
                if (prefabs[i] != null && prefabs[i].prefab != null && prefabs[i].prefab.name.StartsWith(prefix, StringComparison.Ordinal))
                {
                    count++;
                }
            }

            return count;
        }

        private static GameObject FindGameObjectWithTagSafe(string tag)
        {
            try
            {
                return GameObject.FindGameObjectWithTag(tag);
            }
            catch (UnityException)
            {
                return null;
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

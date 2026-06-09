using System.Collections.Generic;
using System.IO;
using LegendOfZed.LegacyMapGenerator;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LegendOfZed.Editor
{
    public static class ZedV30MapTileGeneratorImportSetup
    {
        private const string ImportRoot = "Assets/LegendOfZed/MapGeneratorImport";
        private const string RoomTilesFolder = ImportRoot + "/Prefabs/RoomTiles";
        private const string BuildingsFolder = ImportRoot + "/Prefabs/Buildings";
        private const string TestScenePath = ImportRoot + "/Scenes/Zed_LegacyMapTileGenerator_Test.unity";

        [MenuItem("Legend of Zed/Setup/Map Tile Import/v3.0 Create Required Legacy Tags")]
        public static void CreateRequiredLegacyTags()
        {
            EnsureTag("MapGenerator");
            EnsureTag("RoomTile");
            EnsureTag("TileSpawn");
            EnsureTag("Walkable");
            Debug.Log("v3.0 map tile import required tags are present: MapGenerator, RoomTile, TileSpawn, Walkable.");
        }

        [MenuItem("Legend of Zed/Setup/Map Tile Import/v3.0 Create Legacy Map Tile Test Scene")]
        public static void CreateLegacyMapTileTestScene()
        {
            CreateRequiredLegacyTags();

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject generatorObject = new GameObject("Zed_Legacy_MapTile_Generator");
            generatorObject.tag = "MapGenerator";
            generatorObject.transform.position = Vector3.zero;

            ZedLegacyRandomMapGenerator generator = generatorObject.AddComponent<ZedLegacyRandomMapGenerator>();
            generator.tileCount = 22;
            generator.startingTile = LoadTile("RoomTile-4Way");
            generator.deadEndTile = LoadTile("RoomTile-DeadEnd");
            generator.tilePrefabs = LoadRandomTileSet();

            GameObject cameraObject = new GameObject("Main Camera");
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.transform.position = new Vector3(0f, 80f, -80f);
            camera.transform.rotation = Quaternion.Euler(60f, 0f, 0f);

            GameObject lightObject = new GameObject("Directional Light");
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1f;
            light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            EditorUtility.SetDirty(generatorObject);
            EditorUtility.SetDirty(generator);

            Directory.CreateDirectory(Path.GetDirectoryName(TestScenePath));
            EditorSceneManager.SaveScene(scene, TestScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("v3.0 legacy map tile test scene created: " + TestScenePath + ". Press Play to let the imported legacy generator spawn room tiles.");
        }

        [MenuItem("Legend of Zed/Setup/Map Tile Import/v3.0 Validate Map Tile Import")]
        public static void ValidateMapTileImport()
        {
            int warnings = 0;

            if (!AssetDatabase.IsValidFolder(RoomTilesFolder))
            {
                Debug.LogWarning("Missing room tiles folder: " + RoomTilesFolder);
                warnings++;
            }

            string[] roomTileGuids = AssetDatabase.FindAssets("t:Prefab", new[] { RoomTilesFolder });
            if (roomTileGuids.Length < 8)
            {
                Debug.LogWarning("Expected at least 8 room tile prefabs. Found=" + roomTileGuids.Length);
                warnings++;
            }

            if (LoadTile("RoomTile-4Way") == null)
            {
                Debug.LogWarning("Missing starting tile RoomTile-4Way.");
                warnings++;
            }

            if (LoadTile("RoomTile-DeadEnd") == null)
            {
                Debug.LogWarning("Missing dead-end tile RoomTile-DeadEnd.");
                warnings++;
            }

            if (!AssetDatabase.IsValidFolder(BuildingsFolder))
            {
                Debug.LogWarning("Missing buildings folder: " + BuildingsFolder);
                warnings++;
            }

            string[] buildingGuids = AssetDatabase.FindAssets("t:Prefab", new[] { BuildingsFolder });
            if (buildingGuids.Length == 0)
            {
                Debug.LogWarning("No imported building prefabs found.");
                warnings++;
            }

            ValidateScriptGuid("ZedLegacyRandomMapGenerator", "285eb3c3152703a46a718a7184c85e9e", ref warnings);
            ValidateScriptGuid("ZedLegacyRoomTile", "8619fdb66f2c1204581dc0a656d6a90c", ref warnings);
            ValidateScriptGuid("ZedLegacyGateway", "a45dc05f7c8428b48ac572d4630193be", ref warnings);
            ValidateScriptGuid("ZedLegacySpawnCollisionDetector", "274ed2ccca4c879499202d00e7e4abe0", ref warnings);

            if (warnings == 0)
            {
                Debug.Log("v3.0 map tile generator import validation passed. RoomTiles=" + roomTileGuids.Length + " Buildings=" + buildingGuids.Length + ".");
            }
            else
            {
                Debug.LogWarning("v3.0 map tile generator import validation finished with " + warnings + " warning(s). Review before continuing.");
            }
        }

        private static GameObject LoadTile(string name)
        {
            string[] guids = AssetDatabase.FindAssets(name + " t:Prefab", new[] { RoomTilesFolder });
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null && prefab.name == name)
                {
                    return prefab;
                }
            }

            return null;
        }

        private static GameObject[] LoadRandomTileSet()
        {
            List<GameObject> tiles = new List<GameObject>();
            string[] names =
            {
                "RoomTile-2Way-Left",
                "RoomTile-2Way-Right",
                "RoomTile-2Way-Straight",
                "RoomTile-3Way-Left",
                "RoomTile-3Way-Right",
                "RoomTile-3Way-Straight",
                "RoomTile-4Way"
            };

            for (int i = 0; i < names.Length; i++)
            {
                GameObject tile = LoadTile(names[i]);
                if (tile != null)
                {
                    tiles.Add(tile);
                }
            }

            return tiles.ToArray();
        }

        private static void EnsureTag(string tagName)
        {
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            if (assets == null || assets.Length == 0)
            {
                Debug.LogWarning("Could not load TagManager.asset to add tag: " + tagName);
                return;
            }

            SerializedObject tagManager = new SerializedObject(assets[0]);
            SerializedProperty tags = tagManager.FindProperty("tags");

            for (int i = 0; i < tags.arraySize; i++)
            {
                SerializedProperty tag = tags.GetArrayElementAtIndex(i);
                if (tag.stringValue == tagName)
                {
                    return;
                }
            }

            tags.InsertArrayElementAtIndex(tags.arraySize);
            SerializedProperty newTag = tags.GetArrayElementAtIndex(tags.arraySize - 1);
            newTag.stringValue = tagName;
            tagManager.ApplyModifiedProperties();
        }

        private static void ValidateScriptGuid(string label, string expectedGuid, ref int warnings)
        {
            string[] guids = AssetDatabase.FindAssets(label + " t:Script", new[] { ImportRoot + "/Scripts" });
            bool found = false;
            for (int i = 0; i < guids.Length; i++)
            {
                if (guids[i] == expectedGuid)
                {
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                Debug.LogWarning("Script GUID check failed for " + label + ". Prefab references may not bind correctly.");
                warnings++;
            }
        }
    }
}

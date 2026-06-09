using System.Collections.Generic;
using LegendOfZed.MapIntegration;
using UnityEditor;
using UnityEngine;

namespace LegendOfZed.Editor
{
    public static class ZedParkTileSetupMenu
    {
        private const string MenuRoot = "Legend of Zed/Map Integration/Parks/";
        private const string TreeSpawnTag = "TreeSpawn";
        private const string SyntyPolygonCityRoot = "Assets/Synty/PolygonCity";
        private const string MapGeneratorImportRoot = "Assets/LegendOfZed/MapGeneratorImport";

        [MenuItem(MenuRoot + "1 Create Required Park Tags")]
        public static void CreateRequiredParkTags()
        {
            EnsureTag(TreeSpawnTag);
            Debug.Log("Park tile required tags verified.");
        }

        [MenuItem(MenuRoot + "2 Setup Selected Park Tile Tree Spawner")]
        public static void SetupSelectedParkTileTreeSpawner()
        {
            CreateRequiredParkTags();

            GameObject selected = Selection.activeGameObject;
            if (selected == null)
            {
                Debug.LogWarning("Select your ParkTile prefab root or a ParkTile prefab instance first.");
                return;
            }

            ZedParkTilePropSpawner spawner = selected.GetComponent<ZedParkTilePropSpawner>();
            if (spawner == null)
            {
                spawner = selected.AddComponent<ZedParkTilePropSpawner>();
            }

            spawner.treeSpawnTag = TreeSpawnTag;
            spawner.spawnOnStart = true;
            spawner.destroyMarkersAfterSpawn = false;
            spawner.randomYaw = true;
            spawner.uniformScaleRange = new Vector2(0.9f, 1.15f);
            spawner.yOffset = 0f;
            spawner.logDetails = true;
            spawner.treePrefabs = FindTreePrefabs();

            EditorUtility.SetDirty(selected);
            PrefabUtility.RecordPrefabInstancePropertyModifications(spawner);

            Debug.Log("Park tile tree spawner set up on " + selected.name + ". Tree prefab refs assigned=" + (spawner.treePrefabs == null ? 0 : spawner.treePrefabs.Length) + ".", selected);
        }

        [MenuItem(MenuRoot + "3 Validate Selected Park Tile")]
        public static void ValidateSelectedParkTile()
        {
            GameObject selected = Selection.activeGameObject;
            if (selected == null)
            {
                Debug.LogWarning("Select your ParkTile prefab root or a ParkTile prefab instance first.");
                return;
            }

            int tileSpawns = CountTaggedChildren(selected.transform, "TileSpawn");
            int treeSpawns = CountTaggedChildren(selected.transform, TreeSpawnTag);
            ZedParkTilePropSpawner spawner = selected.GetComponent<ZedParkTilePropSpawner>();

            Debug.Log(
                "Park tile validation for " + selected.name +
                ": TileSpawn markers=" + tileSpawns +
                ", TreeSpawn markers=" + treeSpawns +
                ", has ZedParkTilePropSpawner=" + (spawner != null) +
                ", tree prefab refs=" + (spawner == null || spawner.treePrefabs == null ? 0 : spawner.treePrefabs.Length) + ".",
                selected);
        }

        private static int CountTaggedChildren(Transform root, string tag)
        {
            if (root == null)
            {
                return 0;
            }

            int count = 0;
            Transform[] children = root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < children.Length; i++)
            {
                Transform child = children[i];
                if (child != null && child.CompareTag(tag))
                {
                    count++;
                }
            }

            return count;
        }

        private static GameObject[] FindTreePrefabs()
        {
            List<GameObject> result = new List<GameObject>();
            string[] roots = { SyntyPolygonCityRoot, MapGeneratorImportRoot };
            string[] guids = AssetDatabase.FindAssets("Tree t:Prefab", roots);

            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                string lower = path.ToLowerInvariant();

                if (!lower.Contains("tree"))
                {
                    continue;
                }

                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null && !result.Contains(prefab))
                {
                    result.Add(prefab);
                }
            }

            if (result.Count == 0)
            {
                Debug.LogWarning("No tree prefabs were auto-found. Assign tree prefabs manually on ZedParkTilePropSpawner.");
            }

            return result.ToArray();
        }

        private static void EnsureTag(string tag)
        {
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty tags = tagManager.FindProperty("tags");

            for (int i = 0; i < tags.arraySize; i++)
            {
                SerializedProperty existingTag = tags.GetArrayElementAtIndex(i);
                if (existingTag.stringValue == tag)
                {
                    return;
                }
            }

            tags.InsertArrayElementAtIndex(tags.arraySize);
            tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tag;
            tagManager.ApplyModifiedProperties();
        }
    }
}

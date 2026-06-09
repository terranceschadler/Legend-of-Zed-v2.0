using LegendOfZed.LegacyMapGenerator;
using UnityEditor;
using UnityEngine;

namespace LegendOfZed.Editor
{
    public static class ZedMapTileMissingScriptRepairMenu
    {
        private const string MenuRoot = "Legend of Zed/Map Integration/";
        private const string RoomTilesFolder = "Assets/LegendOfZed/MapGeneratorImport/Prefabs/RoomTiles";

        [MenuItem(MenuRoot + "Repair Room Tile Missing Scripts")]
        public static void RepairRoomTileMissingScripts()
        {
            string[] guids = AssetDatabase.FindAssets("RoomTile t:Prefab", new[] { RoomTilesFolder });

            int prefabsScanned = 0;
            int prefabsModified = 0;
            int missingScriptsRemoved = 0;
            int detectorsAdded = 0;
            int collidersFixed = 0;

            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                GameObject root = PrefabUtility.LoadPrefabContents(path);
                if (root == null)
                {
                    continue;
                }

                prefabsScanned++;
                bool changed = false;

                Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
                for (int t = 0; t < transforms.Length; t++)
                {
                    GameObject go = transforms[t].gameObject;
                    int removed = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
                    if (removed > 0)
                    {
                        missingScriptsRemoved += removed;
                        changed = true;
                    }
                }

                transforms = root.GetComponentsInChildren<Transform>(true);
                for (int t = 0; t < transforms.Length; t++)
                {
                    Transform child = transforms[t];
                    if (child == null || !child.CompareTag("TileSpawn"))
                    {
                        continue;
                    }

                    ZedLegacySpawnCollisionDetector detector = child.GetComponent<ZedLegacySpawnCollisionDetector>();
                    if (detector == null)
                    {
                        child.gameObject.AddComponent<ZedLegacySpawnCollisionDetector>();
                        detectorsAdded++;
                        changed = true;
                    }

                    Collider collider = child.GetComponent<Collider>();
                    if (collider != null && !collider.isTrigger)
                    {
                        collider.isTrigger = true;
                        collidersFixed++;
                        changed = true;
                    }
                }

                if (changed)
                {
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                    prefabsModified++;
                }

                PrefabUtility.UnloadPrefabContents(root);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log(
                "Room tile missing script repair complete. " +
                "Prefabs scanned=" + prefabsScanned +
                ", modified=" + prefabsModified +
                ", missing scripts removed=" + missingScriptsRemoved +
                ", spawn detectors added=" + detectorsAdded +
                ", trigger colliders fixed=" + collidersFixed + ".");
        }

        [MenuItem(MenuRoot + "Report Room Tile Missing Scripts")]
        public static void ReportRoomTileMissingScripts()
        {
            string[] guids = AssetDatabase.FindAssets("RoomTile t:Prefab", new[] { RoomTilesFolder });

            int prefabsScanned = 0;
            int missingScriptObjects = 0;
            int tileSpawnsWithoutDetector = 0;

            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                GameObject root = PrefabUtility.LoadPrefabContents(path);
                if (root == null)
                {
                    continue;
                }

                prefabsScanned++;

                Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
                for (int t = 0; t < transforms.Length; t++)
                {
                    GameObject go = transforms[t].gameObject;
                    if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go) > 0)
                    {
                        missingScriptObjects++;
                        Debug.LogWarning("Missing script found in prefab " + path + " on object " + GetHierarchyPath(go.transform));
                    }

                    if (go.CompareTag("TileSpawn") && go.GetComponent<ZedLegacySpawnCollisionDetector>() == null)
                    {
                        tileSpawnsWithoutDetector++;
                        Debug.LogWarning("TileSpawn without ZedLegacySpawnCollisionDetector found in prefab " + path + " on object " + GetHierarchyPath(go.transform));
                    }
                }

                PrefabUtility.UnloadPrefabContents(root);
            }

            Debug.Log(
                "Room tile missing script report complete. " +
                "Prefabs scanned=" + prefabsScanned +
                ", objects with missing scripts=" + missingScriptObjects +
                ", TileSpawns without detector=" + tileSpawnsWithoutDetector + ".");
        }

        private static string GetHierarchyPath(Transform transform)
        {
            if (transform == null)
            {
                return string.Empty;
            }

            string path = transform.name;
            Transform parent = transform.parent;
            while (parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }

            return path;
        }
    }
}

using LegendOfZed.LegacyMapGenerator;
using LegendOfZed.MapIntegration;
using UnityEditor;
using UnityEngine;

namespace LegendOfZed.Editor
{
    public static class ZedMapTileSpawnRegistryRepairMenu
    {
        private const string MenuRoot = "Legend of Zed/Map Integration/";

        [MenuItem(MenuRoot + "Add Spawn Registry Hooks To Room Tile Prefabs")]
        public static void AddSpawnRegistryHooksToRoomTilePrefabs()
        {
            string[] guids = AssetDatabase.FindAssets("RoomTile t:Prefab", new[] { "Assets/LegendOfZed/MapGeneratorImport/Prefabs/RoomTiles" });
            int modified = 0;

            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                GameObject root = PrefabUtility.LoadPrefabContents(path);
                if (root == null)
                {
                    continue;
                }

                bool changed = false;
                if (root.GetComponent<ZedLegacyRoomTileSpawnRegistryHook>() == null)
                {
                    root.AddComponent<ZedLegacyRoomTileSpawnRegistryHook>();
                    changed = true;
                }

                ZedLegacySpawnCollisionDetector[] detectors = root.GetComponentsInChildren<ZedLegacySpawnCollisionDetector>(true);
                for (int d = 0; d < detectors.Length; d++)
                {
                    if (detectors[d] == null)
                    {
                        continue;
                    }

                    Collider collider = detectors[d].GetComponent<Collider>();
                    if (collider != null && !collider.isTrigger)
                    {
                        collider.isTrigger = true;
                        changed = true;
                    }
                }

                if (changed)
                {
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                    modified++;
                }

                PrefabUtility.UnloadPrefabContents(root);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("Map tile spawn registry hooks added/verified on " + modified + " room tile prefab(s).");
        }

        [MenuItem(MenuRoot + "Reset Runtime Spawn Registry")]
        public static void ResetRuntimeSpawnRegistry()
        {
            ZedMapTileSpawnRegistry.ResetForNewPlaySession();
            Debug.Log("Map tile spawn registry reset.");
        }
    }
}

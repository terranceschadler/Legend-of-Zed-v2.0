#if UNITY_EDITOR
using System.Collections.Generic;
using LegendOfZed.LegacyMapGenerator;
using LegendOfZed.MapIntegration;
using UnityEditor;
using UnityEngine;

namespace LegendOfZed.Editor
{
    public static class ZedCityBlockBuildingFillerMenu
    {
        private const string MenuPath = "Legend of Zed/Map Integration/Add Post-Generation City Block Building Filler";

        [MenuItem(MenuPath)]
        public static void AddCityBlockFiller()
        {
            ZedPostGenerationCityBlockBuildingFiller filler = Object.FindAnyObjectByType<ZedPostGenerationCityBlockBuildingFiller>();
            if (filler == null)
            {
                GameObject go = new GameObject("Zed_PostGeneration_CityBlockBuildingFiller");
                Undo.RegisterCreatedObjectUndo(go, "Create post-generation city block building filler");
                filler = go.AddComponent<ZedPostGenerationCityBlockBuildingFiller>();
            }

            ZedLegacyRandomMapGenerator generator = Object.FindAnyObjectByType<ZedLegacyRandomMapGenerator>();
            if (generator != null)
            {
                filler.mapGenerator = generator;
            }

            if (filler.buildingPrefabs == null || filler.buildingPrefabs.Length == 0)
            {
                filler.buildingPrefabs = CollectBuildingPrefabsFromGeneratorScene();
            }

            EditorUtility.SetDirty(filler);
            Selection.activeGameObject = filler.gameObject;
            EditorGUIUtility.PingObject(filler.gameObject);

            Debug.Log("Post-generation city block building filler added. It suppresses legacy per-tile building spawning while active.", filler);
        }

        private static GameObject[] CollectBuildingPrefabsFromGeneratorScene()
        {
            List<GameObject> prefabs = new List<GameObject>();

            ZedLegacyRoomTile[] tiles = Object.FindObjectsByType<ZedLegacyRoomTile>(FindObjectsInactive.Include);
            for (int i = 0; i < tiles.Length; i++)
            {
                ZedLegacyRoomTile tile = tiles[i];
                if (tile == null || tile.buildingPrefabs == null)
                {
                    continue;
                }

                for (int p = 0; p < tile.buildingPrefabs.Count; p++)
                {
                    GameObject prefab = tile.buildingPrefabs[p];
                    if (prefab != null && !prefabs.Contains(prefab))
                    {
                        prefabs.Add(prefab);
                    }
                }
            }

            return prefabs.ToArray();
        }
    }
}
#endif

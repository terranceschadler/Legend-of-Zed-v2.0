#if UNITY_EDITOR
using System.Collections.Generic;
using LegendOfZed.LegacyMapGenerator;
using LegendOfZed.MapIntegration;
using UnityEditor;
using UnityEngine;

namespace LegendOfZed.Editor
{
    public static class ZedRoadFrontageBuildingSpawnerMenu
    {
        [MenuItem("Legend of Zed/Map Integration/Add Road Frontage Building Spawner")]
        public static void AddRoadFrontageBuildingSpawner()
        {
            ZedPostGenerationCityBlockBuildingFiller oldFiller = Object.FindAnyObjectByType<ZedPostGenerationCityBlockBuildingFiller>();
            if (oldFiller != null)
            {
                oldFiller.runOnStart = false;
                EditorUtility.SetDirty(oldFiller);
            }

            ZedRoadFrontageDebugLines debugLines = Object.FindAnyObjectByType<ZedRoadFrontageDebugLines>();
            if (debugLines != null)
            {
                debugLines.runOnStart = false;
                EditorUtility.SetDirty(debugLines);
            }

            ZedRoadFrontageBuildingSpawner spawner = Object.FindAnyObjectByType<ZedRoadFrontageBuildingSpawner>();
            if (spawner == null)
            {
                GameObject go = new GameObject("Zed_RoadFrontage_BuildingSpawner");
                Undo.RegisterCreatedObjectUndo(go, "Create road frontage building spawner");
                spawner = go.AddComponent<ZedRoadFrontageBuildingSpawner>();
            }

            ZedLegacyRandomMapGenerator generator = Object.FindAnyObjectByType<ZedLegacyRandomMapGenerator>();
            if (generator != null)
            {
                spawner.mapGenerator = generator;
            }

            GameObject generatedMapRoot = GameObject.Find("Generated_Map_Root");
            if (generatedMapRoot == null)
            {
                generatedMapRoot = GameObject.Find("Generated");
            }

            if (generatedMapRoot != null)
            {
                spawner.scanRoot = generatedMapRoot.transform;
            }

            if (spawner.buildingPrefabs == null || spawner.buildingPrefabs.Length == 0)
            {
                spawner.buildingPrefabs = CollectBuildingPrefabs();
            }

            spawner.runOnStart = true;
            spawner.useRoundRobinCoverage = true;
            spawner.maxTotalBuildings = Mathf.Max(spawner.maxTotalBuildings, 800);
            spawner.maxFrontageSegments = Mathf.Max(spawner.maxFrontageSegments, 1024);

            EditorUtility.SetDirty(spawner);
            Selection.activeGameObject = spawner.gameObject;
            EditorGUIUtility.PingObject(spawner.gameObject);

            Debug.Log("Road frontage building spawner added. It uses approved road frontage lines and spawns buildings facing the road.", spawner);
        }

        private static GameObject[] CollectBuildingPrefabs()
        {
            List<GameObject> prefabs = new List<GameObject>();

            ZedPostGenerationCityBlockBuildingFiller oldFiller = Object.FindAnyObjectByType<ZedPostGenerationCityBlockBuildingFiller>();
            if (oldFiller != null && oldFiller.buildingPrefabs != null)
            {
                for (int i = 0; i < oldFiller.buildingPrefabs.Length; i++)
                {
                    GameObject prefab = oldFiller.buildingPrefabs[i];
                    if (prefab != null && !prefabs.Contains(prefab))
                    {
                        prefabs.Add(prefab);
                    }
                }
            }

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

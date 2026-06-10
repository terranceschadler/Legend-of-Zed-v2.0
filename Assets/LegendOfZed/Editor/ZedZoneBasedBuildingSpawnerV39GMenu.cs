#if UNITY_EDITOR
using LegendOfZed.MapIntegration;
using UnityEditor;
using UnityEngine;

namespace LegendOfZed.Editor
{
    public static class ZedZoneBasedBuildingSpawnerV39GMenu
    {
        [MenuItem("Legend of Zed/Map Integration/Add V3.9G1 Integrated Zone Building Spawner")]
        public static void AddSpawner()
        {
            ZedZoneBasedBuildingSpawnerV39G spawner = Object.FindAnyObjectByType<ZedZoneBasedBuildingSpawnerV39G>();
            if (spawner == null)
            {
                GameObject go = new GameObject("Zed_V39G1_IntegratedZoneBuildingSpawner");
                Undo.RegisterCreatedObjectUndo(go, "Create V3.9G1 integrated zone building spawner");
                spawner = go.AddComponent<ZedZoneBasedBuildingSpawnerV39G>();
            }

            TryAssignScanRoot(spawner);
            TryCopyBuildingPrefabs(spawner);

            spawner.buildOnStart = true;
            spawner.startDelay = 0.65f;
            spawner.generatedRootName = "Generated_RoadFrontage_Buildings";
            spawner.clearPreviousGeneratedBuildings = true;
            spawner.autoGenerateBuildableZones = true;
            spawner.autoZoneMinimumDesiredZones = 40;
            spawner.autoZoneRoadClearance = 0.25f;
            spawner.buildingGap = 0.25f;
            spawner.maxBuildingsPerZone = 12;
            spawner.minRemainingWidthToContinue = 1.25f;

            EditorUtility.SetDirty(spawner);
            Selection.activeGameObject = spawner.gameObject;
            EditorGUIUtility.PingObject(spawner.gameObject);

            Debug.Log("Added V3.9G1 integrated zone building spawner. Disable old zone/road building spawners before testing.", spawner);
        }

        [MenuItem("Legend of Zed/Map Integration/Disable Old Building Spawners For V3.9G1")]
        public static void DisableOldBuildingSpawners()
        {
            int disabled = 0;

            ZedRoadFrontageBuildingSpawner[] roadSpawners = Object.FindObjectsByType<ZedRoadFrontageBuildingSpawner>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < roadSpawners.Length; i++)
            {
                if (roadSpawners[i] != null && roadSpawners[i].enabled)
                {
                    Undo.RecordObject(roadSpawners[i], "Disable old road-frontage building spawner");
                    roadSpawners[i].enabled = false;
                    EditorUtility.SetDirty(roadSpawners[i]);
                    disabled++;
                }
            }

            ZedZoneBasedBuildingSpawner[] oldZoneSpawners = Object.FindObjectsByType<ZedZoneBasedBuildingSpawner>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < oldZoneSpawners.Length; i++)
            {
                if (oldZoneSpawners[i] != null && oldZoneSpawners[i].enabled)
                {
                    Undo.RecordObject(oldZoneSpawners[i], "Disable old zone building spawner");
                    oldZoneSpawners[i].enabled = false;
                    EditorUtility.SetDirty(oldZoneSpawners[i]);
                    disabled++;
                }
            }

            Debug.Log("Disabled old building spawners for V3.9G1. Count=" + disabled + ".");
        }

        [MenuItem("Legend of Zed/Map Integration/Run V3.9G1 Integrated Zone Building Spawner Now")]
        public static void RunNow()
        {
            ZedZoneBasedBuildingSpawnerV39G spawner = Object.FindAnyObjectByType<ZedZoneBasedBuildingSpawnerV39G>();
            if (spawner == null)
            {
                Debug.LogWarning("No V3.9G1 integrated zone building spawner found. Run Add V3.9G1 Integrated Zone Building Spawner first.");
                return;
            }

            TryAssignScanRoot(spawner);
            TryCopyBuildingPrefabs(spawner);
            spawner.BuildNow();
            EditorUtility.SetDirty(spawner);
        }

        private static void TryAssignScanRoot(ZedZoneBasedBuildingSpawnerV39G spawner)
        {
            if (spawner == null || spawner.scanRoot != null)
            {
                return;
            }

            GameObject mapRoot =
                GameObject.Find("Generated_Map_Tiles") ??
                GameObject.Find("Generated_Map_Root") ??
                GameObject.Find("Generated_Map") ??
                GameObject.Find("GeneratedMap");

            if (mapRoot != null)
            {
                spawner.scanRoot = mapRoot.transform;
            }
        }

        private static void TryCopyBuildingPrefabs(ZedZoneBasedBuildingSpawnerV39G spawner)
        {
            if (spawner == null || spawner.buildingPrefabs != null && spawner.buildingPrefabs.Length > 0)
            {
                return;
            }

            ZedZoneBasedBuildingSpawner oldZone = Object.FindAnyObjectByType<ZedZoneBasedBuildingSpawner>();
            if (oldZone != null && oldZone.buildingPrefabs != null && oldZone.buildingPrefabs.Length > 0)
            {
                spawner.buildingPrefabs = oldZone.buildingPrefabs;
                return;
            }

            ZedRoadFrontageBuildingSpawner oldRoad = Object.FindAnyObjectByType<ZedRoadFrontageBuildingSpawner>();
            if (oldRoad != null && oldRoad.buildingPrefabs != null && oldRoad.buildingPrefabs.Length > 0)
            {
                spawner.buildingPrefabs = oldRoad.buildingPrefabs;
            }
        }
    }
}
#endif

#if UNITY_EDITOR
using LegendOfZed.MapIntegration;
using UnityEditor;
using UnityEngine;

namespace LegendOfZed.Editor
{
    public static class ZedZoneBasedBuildingSpawnerMenu
    {
        [MenuItem("Legend of Zed/Map Integration/Add Zone-Based Building Spawner")]
        public static void AddZoneBasedBuildingSpawner()
        {
            ZedZoneBasedBuildingSpawner spawner = Object.FindAnyObjectByType<ZedZoneBasedBuildingSpawner>();
            if (spawner == null)
            {
                GameObject go = new GameObject("Zed_ZoneBasedBuildingSpawner");
                Undo.RegisterCreatedObjectUndo(go, "Create zone-based building spawner");
                spawner = go.AddComponent<ZedZoneBasedBuildingSpawner>();
            }

            TryAssignScanRoot(spawner);
            TryCopyBuildingPrefabsFromRoadSpawner(spawner);

            spawner.buildOnStart = true;
            spawner.startDelay = 0.45f;
            spawner.generatedRootName = "Generated_RoadFrontage_Buildings";
            spawner.clearPreviousGeneratedBuildings = true;
            spawner.rejectAgainstDetectedRoadRenderers = true;

            EditorUtility.SetDirty(spawner);
            Selection.activeGameObject = spawner.gameObject;
            EditorGUIUtility.PingObject(spawner.gameObject);

            Debug.Log("Zone-based building spawner added. Disable the old ZedRoadFrontageBuildingSpawner before testing v3.9a.", spawner);
        }

        [MenuItem("Legend of Zed/Map Integration/Disable Old Road Frontage Building Spawner")]
        public static void DisableOldRoadFrontageBuildingSpawner()
        {
            ZedRoadFrontageBuildingSpawner[] oldSpawners = Object.FindObjectsByType<ZedRoadFrontageBuildingSpawner>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            int disabled = 0;

            for (int i = 0; i < oldSpawners.Length; i++)
            {
                ZedRoadFrontageBuildingSpawner old = oldSpawners[i];
                if (old == null || !old.enabled)
                {
                    continue;
                }

                Undo.RecordObject(old, "Disable old road frontage building spawner");
                old.enabled = false;
                EditorUtility.SetDirty(old);
                disabled++;
            }

            Debug.Log("Disabled old road-frontage building spawner count: " + disabled + ".");
        }

        [MenuItem("Legend of Zed/Map Integration/Run Zone-Based Building Spawner Now")]
        public static void RunZoneBasedBuildingSpawnerNow()
        {
            ZedZoneBasedBuildingSpawner spawner = Object.FindAnyObjectByType<ZedZoneBasedBuildingSpawner>();
            if (spawner == null)
            {
                Debug.LogWarning("No ZedZoneBasedBuildingSpawner found. Run Add Zone-Based Building Spawner first.");
                return;
            }

            TryAssignScanRoot(spawner);
            TryCopyBuildingPrefabsFromRoadSpawner(spawner);

            spawner.BuildNow();
            EditorUtility.SetDirty(spawner);
        }

        private static void TryAssignScanRoot(ZedZoneBasedBuildingSpawner spawner)
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

        private static void TryCopyBuildingPrefabsFromRoadSpawner(ZedZoneBasedBuildingSpawner spawner)
        {
            if (spawner == null || spawner.buildingPrefabs != null && spawner.buildingPrefabs.Length > 0)
            {
                return;
            }

            ZedRoadFrontageBuildingSpawner old = Object.FindAnyObjectByType<ZedRoadFrontageBuildingSpawner>();
            if (old != null && old.buildingPrefabs != null && old.buildingPrefabs.Length > 0)
            {
                spawner.buildingPrefabs = old.buildingPrefabs;
            }
        }
    }
}
#endif

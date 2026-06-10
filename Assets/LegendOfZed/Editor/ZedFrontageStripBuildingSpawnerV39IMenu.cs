#if UNITY_EDITOR
using System.Collections.Generic;
using System.Reflection;
using LegendOfZed.MapIntegration;
using UnityEditor;
using UnityEngine;

namespace LegendOfZed.Editor
{
    public static class ZedFrontageStripBuildingSpawnerV39IMenu
    {
        [MenuItem("Legend of Zed/Map Integration/CLEAN INSTALL V3.9I Frontage Strip Building System")]
        public static void CleanInstall()
        {
            DisableOldBuildingSpawners();
            ClearGeneratedBuildingRoot();

            ZedFrontageStripBuildingSpawnerV39I spawner = Object.FindAnyObjectByType<ZedFrontageStripBuildingSpawnerV39I>();
            if (spawner == null)
            {
                GameObject go = new GameObject("Zed_V39I_FrontageStripBuildingSpawner");
                Undo.RegisterCreatedObjectUndo(go, "Create V3.9I frontage strip building spawner");
                spawner = go.AddComponent<ZedFrontageStripBuildingSpawnerV39I>();
            }

            TryAssignScanRoot(spawner);
            CopyAllBuildingPrefabs(spawner);

            spawner.buildOnStart = true;
            spawner.startDelay = 0.85f;
            spawner.generatedBuildingRootName = "Generated_RoadFrontage_Buildings";
            spawner.clearPreviousGeneratedBuildings = true;

            EditorUtility.SetDirty(spawner);
            Selection.activeGameObject = spawner.gameObject;
            EditorGUIUtility.PingObject(spawner.gameObject);

            Debug.Log("CLEAN INSTALL V3.9I complete. Regenerate and look for: V3.9I FRONTAGE STRIP spawner complete.", spawner);
        }

        [MenuItem("Legend of Zed/Map Integration/Run V3.9I Frontage Strip Building Spawner Now")]
        public static void RunNow()
        {
            ZedFrontageStripBuildingSpawnerV39I spawner = Object.FindAnyObjectByType<ZedFrontageStripBuildingSpawnerV39I>();
            if (spawner == null)
            {
                Debug.LogWarning("No V3.9I frontage strip spawner found. Run CLEAN INSTALL V3.9I first.");
                return;
            }

            TryAssignScanRoot(spawner);
            CopyAllBuildingPrefabs(spawner);
            spawner.BuildNow();
            EditorUtility.SetDirty(spawner);
        }

        private static void DisableOldBuildingSpawners()
        {
            string[] oldTypeNames =
            {
                "ZedRoadFrontageBuildingSpawner",
                "ZedZoneBasedBuildingSpawner",
                "ZedZoneBasedBuildingSpawnerV39G",
                "ZedForcedZoneBuildingSpawnerV39H",
                "ZedAutoBuildableZoneCoverage"
            };

            MonoBehaviour[] behaviours = Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            int disabled = 0;

            for (int i = 0; i < behaviours.Length; i++)
            {
                MonoBehaviour mb = behaviours[i];
                if (mb == null)
                {
                    continue;
                }

                string typeName = mb.GetType().Name;
                bool match = false;

                for (int t = 0; t < oldTypeNames.Length; t++)
                {
                    if (typeName == oldTypeNames[t])
                    {
                        match = true;
                        break;
                    }
                }

                if (!match || !mb.enabled)
                {
                    continue;
                }

                Undo.RecordObject(mb, "Disable obsolete building spawner");
                mb.enabled = false;
                EditorUtility.SetDirty(mb);
                disabled++;
            }

            Debug.Log("V3.9I cleanup disabled obsolete building spawner components: " + disabled + ".");
        }

        private static void ClearGeneratedBuildingRoot()
        {
            GameObject root = GameObject.Find("Generated_RoadFrontage_Buildings");
            if (root == null)
            {
                return;
            }

            int cleared = 0;
            for (int i = root.transform.childCount - 1; i >= 0; i--)
            {
                Transform child = root.transform.GetChild(i);
                if (child != null)
                {
                    Undo.DestroyObjectImmediate(child.gameObject);
                    cleared++;
                }
            }

            Debug.Log("V3.9I cleanup cleared generated building children: " + cleared + ".");
        }

        private static void TryAssignScanRoot(ZedFrontageStripBuildingSpawnerV39I spawner)
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

        private static void CopyAllBuildingPrefabs(ZedFrontageStripBuildingSpawnerV39I spawner)
        {
            if (spawner == null)
            {
                return;
            }

            List<GameObject> prefabs = new List<GameObject>();

            if (spawner.buildingPrefabs != null)
            {
                AddRangeDistinct(prefabs, spawner.buildingPrefabs);
            }

            MonoBehaviour[] behaviours = Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < behaviours.Length; i++)
            {
                MonoBehaviour mb = behaviours[i];
                if (mb == null || mb == spawner)
                {
                    continue;
                }

                FieldInfo field = mb.GetType().GetField("buildingPrefabs", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (field == null)
                {
                    continue;
                }

                object value = field.GetValue(mb);
                GameObject[] array = value as GameObject[];
                if (array != null)
                {
                    AddRangeDistinct(prefabs, array);
                    continue;
                }

                List<GameObject> list = value as List<GameObject>;
                if (list != null)
                {
                    AddRangeDistinct(prefabs, list.ToArray());
                }
            }

            spawner.buildingPrefabs = prefabs.ToArray();
            EditorUtility.SetDirty(spawner);

            if (spawner.buildingPrefabs.Length == 0)
            {
                Debug.LogWarning("V3.9I could not find building prefabs to copy. Assign Building Prefabs on Zed_V39I_FrontageStripBuildingSpawner.", spawner);
            }
            else
            {
                Debug.Log("V3.9I building prefab pool assigned. Count=" + spawner.buildingPrefabs.Length + ".", spawner);
            }
        }

        private static void AddRangeDistinct(List<GameObject> target, GameObject[] source)
        {
            if (target == null || source == null)
            {
                return;
            }

            for (int i = 0; i < source.Length; i++)
            {
                GameObject prefab = source[i];
                if (prefab == null || target.Contains(prefab))
                {
                    continue;
                }

                target.Add(prefab);
            }
        }
    }
}
#endif

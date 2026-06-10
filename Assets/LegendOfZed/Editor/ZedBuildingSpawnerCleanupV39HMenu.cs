#if UNITY_EDITOR
using System;
using System.Reflection;
using LegendOfZed.MapIntegration;
using UnityEditor;
using UnityEngine;

namespace LegendOfZed.Editor
{
    public static class ZedBuildingSpawnerCleanupV39HMenu
    {
        [MenuItem("Legend of Zed/Map Integration/CLEAN INSTALL V3.9H Building System")]
        public static void CleanInstall()
        {
            DisableOldSpawnerComponents();
            ClearGeneratedRoots();

            ZedForcedZoneBuildingSpawnerV39H spawner = UnityEngine.Object.FindAnyObjectByType<ZedForcedZoneBuildingSpawnerV39H>();
            if (spawner == null)
            {
                GameObject go = new GameObject("Zed_V39H_ForcedZoneBuildingSpawner");
                Undo.RegisterCreatedObjectUndo(go, "Create V3.9H forced zone building spawner");
                spawner = go.AddComponent<ZedForcedZoneBuildingSpawnerV39H>();
            }

            TryCopyBuildingPrefabs(spawner);

            spawner.buildOnStart = true;
            spawner.startDelay = 0.8f;
            spawner.generateAutoZones = true;
            spawner.autoZoneRoadClearance = 0.15f;
            spawner.generatedBuildingRootName = "Generated_RoadFrontage_Buildings";
            spawner.clearPreviousGeneratedBuildings = true;
            spawner.clearPreviousAutoZones = true;

            EditorUtility.SetDirty(spawner);
            Selection.activeGameObject = spawner.gameObject;
            EditorGUIUtility.PingObject(spawner.gameObject);

            Debug.Log("CLEAN INSTALL V3.9H complete. Regenerate and look for: V3.9H CLEAN forced zone building spawner complete.", spawner);
        }

        [MenuItem("Legend of Zed/Map Integration/Run V3.9H Building Spawner Now")]
        public static void RunNow()
        {
            ZedForcedZoneBuildingSpawnerV39H spawner = UnityEngine.Object.FindAnyObjectByType<ZedForcedZoneBuildingSpawnerV39H>();
            if (spawner == null)
            {
                Debug.LogWarning("No V3.9H spawner found. Run CLEAN INSTALL V3.9H Building System first.");
                return;
            }

            TryCopyBuildingPrefabs(spawner);
            spawner.BuildNow();
            EditorUtility.SetDirty(spawner);
        }

        private static void DisableOldSpawnerComponents()
        {
            string[] oldTypeNames =
            {
                "ZedRoadFrontageBuildingSpawner",
                "ZedZoneBasedBuildingSpawner",
                "ZedZoneBasedBuildingSpawnerV39G",
                "ZedAutoBuildableZoneCoverage"
            };

            MonoBehaviour[] behaviours = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
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

            Debug.Log("V3.9H cleanup disabled obsolete building spawner components: " + disabled + ".");
        }

        private static void ClearGeneratedRoots()
        {
            string[] roots =
            {
                "Generated_RoadFrontage_Buildings",
                "Generated_AutoBuildable_Zones"
            };

            int cleared = 0;
            for (int i = 0; i < roots.Length; i++)
            {
                GameObject root = GameObject.Find(roots[i]);
                if (root == null)
                {
                    continue;
                }

                for (int c = root.transform.childCount - 1; c >= 0; c--)
                {
                    Transform child = root.transform.GetChild(c);
                    if (child != null)
                    {
                        Undo.DestroyObjectImmediate(child.gameObject);
                        cleared++;
                    }
                }
            }

            Debug.Log("V3.9H cleanup cleared generated building/zone children: " + cleared + ".");
        }

        private static void TryCopyBuildingPrefabs(ZedForcedZoneBuildingSpawnerV39H spawner)
        {
            if (spawner == null || spawner.buildingPrefabs != null && spawner.buildingPrefabs.Length > 0)
            {
                return;
            }

            MonoBehaviour[] behaviours = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < behaviours.Length; i++)
            {
                MonoBehaviour mb = behaviours[i];
                if (mb == null)
                {
                    continue;
                }

                FieldInfo field = mb.GetType().GetField("buildingPrefabs", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (field == null)
                {
                    continue;
                }

                GameObject[] prefabs = field.GetValue(mb) as GameObject[];
                if (prefabs != null && prefabs.Length > 0)
                {
                    spawner.buildingPrefabs = prefabs;
                    Debug.Log("V3.9H copied building prefabs from " + mb.GetType().Name + ". Count=" + prefabs.Length + ".", spawner);
                    return;
                }
            }

            Debug.LogWarning("V3.9H could not find building prefabs to copy. Assign Building Prefabs on Zed_V39H_ForcedZoneBuildingSpawner.", spawner);
        }
    }
}
#endif

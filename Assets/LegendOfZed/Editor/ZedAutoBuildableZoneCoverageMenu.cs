#if UNITY_EDITOR
using LegendOfZed.MapIntegration;
using UnityEditor;
using UnityEngine;

namespace LegendOfZed.Editor
{
    public static class ZedAutoBuildableZoneCoverageMenu
    {
        [MenuItem("Legend of Zed/Map Integration/Add Auto Buildable Zone Coverage")]
        public static void AddAutoBuildableZoneCoverage()
        {
            ZedAutoBuildableZoneCoverage coverage = Object.FindAnyObjectByType<ZedAutoBuildableZoneCoverage>();
            if (coverage == null)
            {
                GameObject go = new GameObject("Zed_AutoBuildableZoneCoverage");
                Undo.RegisterCreatedObjectUndo(go, "Create auto buildable zone coverage");
                coverage = go.AddComponent<ZedAutoBuildableZoneCoverage>();
            }

            TryAssignScanRoot(coverage);

            coverage.buildOnStart = true;
            coverage.startDelay = 0.2f;
            coverage.clearPreviousAutoZones = true;

            EditorUtility.SetDirty(coverage);
            Selection.activeGameObject = coverage.gameObject;
            EditorGUIUtility.PingObject(coverage.gameObject);

            Debug.Log("Auto buildable zone coverage added. It creates more ZedBuildableZone strips before the zone-based building spawner runs.", coverage);
        }

        [MenuItem("Legend of Zed/Map Integration/Run Auto Buildable Zone Coverage Now")]
        public static void RunAutoBuildableZoneCoverageNow()
        {
            ZedAutoBuildableZoneCoverage coverage = Object.FindAnyObjectByType<ZedAutoBuildableZoneCoverage>();
            if (coverage == null)
            {
                Debug.LogWarning("No ZedAutoBuildableZoneCoverage found. Run Add Auto Buildable Zone Coverage first.");
                return;
            }

            TryAssignScanRoot(coverage);
            coverage.BuildNow();
            EditorUtility.SetDirty(coverage);
        }

        private static void TryAssignScanRoot(ZedAutoBuildableZoneCoverage coverage)
        {
            if (coverage == null || coverage.scanRoot != null)
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
                coverage.scanRoot = mapRoot.transform;
            }
        }
    }
}
#endif

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace LegendOfZed.MapIntegration.EditorTools
{
    /// <summary>
    /// Deletes obsolete/experimental scripts and removes obsolete open-scene components.
    /// Kept as one stable cleanup menu; old versioned menus are deleted by this pass.
    /// </summary>
    [InitializeOnLoad]
    public static class ZedV310ACProjectCleanupMenu
    {
        private const string AutoRunKey = "LegendOfZed.V310AC.ProjectCleanup.AutoRunComplete";
        private const string MenuRoot = "Legend Of Zed/Cleanup/v3.10AC/";

        private static readonly string[] ObsoleteTypeNames =
        {
            "ZedPostGenerationCityBlockBuildingFiller",
            "ZedRoadFrontageBuildingSpawner",
            "ZedZoneBasedBuildingSpawner",
            "ZedZoneBasedBuildingSpawnerV39G",
            "ZedForcedZoneBuildingSpawnerV39H",
            "ZedFrontageStripBuildingSpawnerV39I",
            "ZedBuildingLotFiller",
            "ZedAutoBuildableZoneCoverage",
            "ZedBuildableZone",
            "ZedRoadFrontageDebugLines",
            "ZedRoadFrontageDiagnosticOverlay",
            "ZedRoadFrontageSocket",
            "ZedTileRoadMaskAudit",
            "ZedTileRoadMaskOverride",
            "ZedMapTileGapResolver"
        };

        private static readonly string[] ObsoleteScriptFileNames =
        {
            "ZedAuthoredBuildingCornerSideMenu.cs",
            "ZedAuthoredBuildingLotDiagnosticsMenu.cs",
            "ZedAuthoredBuildingLotSpawnerMenu.cs",
            "ZedAuthoredLotBuildingCategoryMenu.cs",
            "ZedAuthoredLotRotationAuthoringMenu.cs",
            "ZedAutoAggressiveEditorFolderCleanup.cs",
            "ZedAutoBuildableZoneCoverageMenu.cs",
            "ZedBuildingSpawnerCleanupV39HMenu.cs",
            "ZedCityBlockBuildingFillerMenu.cs",
            "ZedCityBlockTagEnsure.cs",
            "ZedExplicitTileFrontageSocketSetupMenu.cs",
            "ZedFrontageStripBuildingSpawnerV39IMenu.cs",
            "ZedMapImportMissingScriptLocatorMenu.cs",
            "ZedMapTileGapResolverMenu.cs",
            "ZedMapTileMissingScriptRepairMenu.cs",
            "ZedMapTileSpawnRegistryRepairMenu.cs",
            "ZedOneTimeAggressiveEditorFolderCleanup.cs",
            "ZedOneTimeObsoleteEditorSetupCleanup.cs",
            "ZedOneTimeRemoveBlockSurfaceHelper.cs",
            "ZedOneTimeRemoveZoneAuthoringScriptsOnly.cs",
            "ZedOneTimeRoomTile4WayBuildableZoneAuthoring.cs",
            "ZedOneTimeRoomTile4WayZoneRotationAndFillTuning.cs",
            "ZedOneTimeRoomTile4WayZoneTuningPass.cs",
            "ZedRoadFrontageBuildingSpawnerMenu.cs",
            "ZedRoadFrontageBuildingSpawnerSettingsResetMenu.cs",
            "ZedRoadFrontageDebugMenu.cs",
            "ZedRoadFrontageDiagnosticOverlayMenu.cs",
            "ZedRoadFrontageSocketEditor.cs",
            "ZedTileRoadMaskAuditMenu.cs",
            "ZedTileRoadMaskOverrideMenu.cs",
            "ZedV310a2RemoveLeftoverExperimentMenus.cs",
            "ZedV310a3RemoveOldTopLevelMapMenus.cs",
            "ZedV310aRemoveFailedExperimentScriptsMenu.cs",
            "ZedV310cSceneHierarchyCleanupMenu.cs",
            "ZedZoneBasedBuildingSpawnerMenu.cs",
            "ZedZoneBasedBuildingSpawnerV39GMenu.cs",
            "ZedAutoBuildableZoneCoverage.cs",
            "ZedBuildableZone.cs",
            "ZedBuildingCornerSide.cs",
            "ZedBuildingLotFiller.cs",
            "ZedForcedZoneBuildingSpawnerV39H.cs",
            "ZedFrontageStripBuildingSpawnerV39I.cs",
            "ZedPostGenerationCityBlockBuildingFiller.cs",
            "ZedRoadFrontageBuildingSpawner.cs",
            "ZedRoadFrontageDebugLines.cs",
            "ZedRoadFrontageDiagnosticOverlay.cs",
            "ZedRoadFrontageSocket.cs",
            "ZedTileRoadMaskAudit.cs",
            "ZedTileRoadMaskOverride.cs",
            "ZedZoneBasedBuildingSpawner.cs",
            "ZedZoneBasedBuildingSpawnerV39G.cs"
        };

        static ZedV310ACProjectCleanupMenu()
        {
            EditorApplication.delayCall += AutoRunOnce;
        }

        private static void AutoRunOnce()
        {
            if (EditorPrefs.GetBool(AutoRunKey, false))
            {
                return;
            }

            EditorPrefs.SetBool(AutoRunKey, true);
            RunCleanup();
        }

        [MenuItem(MenuRoot + "Run Project Cleanup Now")]
        public static void RunCleanup()
        {
            int componentsRemoved = RemoveObsoleteOpenSceneComponents();
            int scriptsDeleted = DeleteObsoleteScripts();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("V3.10AC project cleanup complete. ComponentsRemoved=" + componentsRemoved + ", ScriptsDeleted=" + scriptsDeleted + ".");
        }

        [MenuItem(MenuRoot + "Reset Cleanup Auto-Run Flag")]
        public static void ResetAutoRunFlag()
        {
            EditorPrefs.DeleteKey(AutoRunKey);
            Debug.Log("V3.10AC cleanup auto-run flag reset.");
        }

        private static int RemoveObsoleteOpenSceneComponents()
        {
            int removed = 0;
            HashSet<string> obsoleteNames = new HashSet<string>(ObsoleteTypeNames);

            MonoBehaviour[] behaviours = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include);
            for (int i = 0; i < behaviours.Length; i++)
            {
                MonoBehaviour behaviour = behaviours[i];
                if (behaviour == null)
                {
                    continue;
                }

                Type type = behaviour.GetType();
                if (type == null || !obsoleteNames.Contains(type.Name))
                {
                    continue;
                }

                Undo.DestroyObjectImmediate(behaviour);
                removed++;
            }

            return removed;
        }

        private static int DeleteObsoleteScripts()
        {
            int deleted = 0;
            HashSet<string> targets = new HashSet<string>(ObsoleteScriptFileNames);

            string[] guids = AssetDatabase.FindAssets("t:MonoScript", new[] { "Assets/LegendOfZed" });
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (string.IsNullOrEmpty(path))
                {
                    continue;
                }

                string fileName = Path.GetFileName(path);
                if (!targets.Contains(fileName))
                {
                    continue;
                }

                if (AssetDatabase.DeleteAsset(path))
                {
                    deleted++;
                }
            }

            return deleted;
        }
    }
}
#endif

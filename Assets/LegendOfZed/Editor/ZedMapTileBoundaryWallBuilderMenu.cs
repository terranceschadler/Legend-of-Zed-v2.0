using LegendOfZed.LegacyMapGenerator;
using LegendOfZed.MapIntegration;
using UnityEditor;
using UnityEngine;

namespace LegendOfZed.Editor
{
    public static class ZedMapTileBoundaryWallBuilderMenu
    {
        private const string MenuRoot = "Legend of Zed/Map Integration/";

        [MenuItem(MenuRoot + "Add Boundary Wall Builder To Scene")]
        public static void AddBoundaryWallBuilderToScene()
        {
            ZedMapTileBoundaryWallBuilder builder = Object.FindAnyObjectByType<ZedMapTileBoundaryWallBuilder>();
            if (builder == null)
            {
                GameObject builderObject = new GameObject("Zed_MapTile_BoundaryWallBuilder");
                builder = builderObject.AddComponent<ZedMapTileBoundaryWallBuilder>();
                Undo.RegisterCreatedObjectUndo(builderObject, "Add Map Tile Boundary Wall Builder");
            }

            builder.generator = Object.FindAnyObjectByType<ZedLegacyRandomMapGenerator>();
            builder.wallRootName = "Generated_Map_Boundary_Walls";
            builder.wallHeight = 5f;
            builder.wallThickness = 1.5f;
            builder.yCenter = 2.5f;
            builder.addVisibleWallMesh = true;
            builder.rebuildAutomatically = true;
            builder.maxFootprintCenterY = 1.25f;
            builder.minFootprintSize = 8f;
            builder.coordinateTolerance = 0.05f;
            builder.logDetails = false;

            EditorUtility.SetDirty(builder);
            Selection.activeObject = builder.gameObject;

            Debug.Log("Floor-footprint perimeter boundary wall builder added/updated in scene.", builder);
        }

        [MenuItem(MenuRoot + "Remove Boundary Walls From Scene")]
        public static void RemoveBoundaryWallsFromScene()
        {
            GameObject wallRoot = GameObject.Find("Generated_Map_Boundary_Walls");
            if (wallRoot != null)
            {
                Undo.DestroyObjectImmediate(wallRoot);
                Debug.Log("Removed generated map boundary walls from scene.");
            }
            else
            {
                Debug.Log("No generated map boundary wall root found in scene.");
            }
        }
    }
}

#if UNITY_EDITOR
using LegendOfZed.MapIntegration;
using UnityEditor;
using UnityEngine;

namespace LegendOfZed.Editor
{
    public static class ZedRoadFrontageDebugMenu
    {
        [MenuItem("Legend of Zed/Map Integration/Add Road Frontage Debug Lines")]
        public static void AddRoadFrontageDebugLines()
        {
            ZedPostGenerationCityBlockBuildingFiller oldFiller = Object.FindAnyObjectByType<ZedPostGenerationCityBlockBuildingFiller>();
            if (oldFiller != null)
            {
                oldFiller.runOnStart = false;
                EditorUtility.SetDirty(oldFiller);
                Debug.Log("Disabled old CityBlock building filler runOnStart so road frontage debug can be reviewed without spawned buildings.", oldFiller);
            }

            ZedRoadFrontageDebugLines debug = Object.FindAnyObjectByType<ZedRoadFrontageDebugLines>();
            if (debug == null)
            {
                GameObject go = new GameObject("Zed_RoadFrontage_DebugLines");
                Undo.RegisterCreatedObjectUndo(go, "Create road frontage debug lines");
                debug = go.AddComponent<ZedRoadFrontageDebugLines>();
            }

            GameObject generatedMapRoot = GameObject.Find("Generated_Map_Root");
            if (generatedMapRoot == null)
            {
                generatedMapRoot = GameObject.Find("Generated");
            }

            if (generatedMapRoot != null)
            {
                debug.scanRoot = generatedMapRoot.transform;
            }

            debug.runOnStart = true;
            debug.drawGizmos = true;
            debug.logDetails = true;

            EditorUtility.SetDirty(debug);
            Selection.activeGameObject = debug.gameObject;
            EditorGUIUtility.PingObject(debug.gameObject);

            Debug.Log("Road frontage debug lines added. Press Play and review green frontage lines/yellow road-facing arrows before spawning buildings.", debug);
        }
    }
}
#endif

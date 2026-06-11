#if UNITY_EDITOR
using LegendOfZed.MapIntegration;
using LegendOfZed.Overworld;
using UnityEditor;
using UnityEngine;

namespace LegendOfZed.EditorTools
{
    public static class ZedV310AF8RuntimeNavMeshMenu
    {
        [MenuItem("Legend Of Zed/Navigation/Rebuild Runtime NavMesh Now")]
        public static void RebuildRuntimeNavMeshNow()
        {
            ZedMapTileGeneratorRuntimeBridge bridge = Object.FindAnyObjectByType<ZedMapTileGeneratorRuntimeBridge>();
            if (bridge != null)
            {
                bridge.RebuildRuntimeNavMeshNow();
                Debug.Log("Requested runtime NavMesh rebuild through ZedMapTileGeneratorRuntimeBridge.", bridge);
                return;
            }

            ZedOverworldRuntimeNavMeshBuilder builder = Object.FindAnyObjectByType<ZedOverworldRuntimeNavMeshBuilder>();
            if (builder == null)
            {
                GameObject builderObject = new GameObject("Zed_Runtime_NavMesh_Builder");
                Undo.RegisterCreatedObjectUndo(builderObject, "Create runtime NavMesh builder");
                builder = builderObject.AddComponent<ZedOverworldRuntimeNavMeshBuilder>();
            }

            builder.BuildOnStart = false;
            builder.LogBuild = true;
            builder.BuildNow();
            EditorUtility.SetDirty(builder);
            Debug.Log("Requested runtime NavMesh rebuild through ZedOverworldRuntimeNavMeshBuilder.", builder);
        }

        [MenuItem("Legend Of Zed/Navigation/Select Runtime NavMesh Builder")]
        public static void SelectRuntimeNavMeshBuilder()
        {
            ZedOverworldRuntimeNavMeshBuilder builder = Object.FindAnyObjectByType<ZedOverworldRuntimeNavMeshBuilder>();
            if (builder == null)
            {
                GameObject builderObject = new GameObject("Zed_Runtime_NavMesh_Builder");
                Undo.RegisterCreatedObjectUndo(builderObject, "Create runtime NavMesh builder");
                builder = builderObject.AddComponent<ZedOverworldRuntimeNavMeshBuilder>();
            }

            Selection.activeObject = builder.gameObject;
            EditorGUIUtility.PingObject(builder.gameObject);
        }
    }
}
#endif

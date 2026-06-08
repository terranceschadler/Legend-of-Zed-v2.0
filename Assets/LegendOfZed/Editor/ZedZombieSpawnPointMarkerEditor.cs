using LegendOfZed.Spawning;
using UnityEditor;
using UnityEngine;

namespace LegendOfZed.Editor
{
    [CustomEditor(typeof(ZedZombieSpawnPointMarker))]
    public class ZedZombieSpawnPointMarkerEditor : UnityEditor.Editor
    {
        private void OnSceneGUI()
        {
            ZedZombieSpawnPointMarker marker = (ZedZombieSpawnPointMarker)target;
            if (marker == null || !marker.DrawLabel)
            {
                return;
            }

            GUIStyle style = new GUIStyle(EditorStyles.boldLabel);
            style.normal.textColor = Color.white;
            style.alignment = TextAnchor.MiddleCenter;

            Vector3 labelPosition = marker.transform.position + Vector3.up * 1.8f;
            Handles.Label(labelPosition, marker.SpawnLabel, style);
        }
    }
}

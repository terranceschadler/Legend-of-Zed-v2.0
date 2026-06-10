using System.Collections.Generic;
using UnityEngine;

namespace LegendOfZed.MapIntegration
{
    /// <summary>
    /// Explicit authored buildable strip.
    /// The lot filler only spawns buildings that fully fit inside this rectangle.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ZedBuildableZone : MonoBehaviour
    {
        public enum DepthAlignment
        {
            Center,
            FrontEdge,
            BackEdge
        }

        [Header("Zone")]
        [Min(1f)] public float width = 8f;
        [Min(1f)] public float depth = 6f;

        [Tooltip("Inset from all zone edges before footprint fit checks. Keeps buildings off roads/sidewalk edges.")]
        [Min(0f)] public float edgeInset = 0.25f;

        [Header("Depth Placement")]
        [Tooltip("Use BackEdge for buildings pulled away from the road/sidewalk. Use FrontEdge to line storefronts along a sidewalk.")]
        public DepthAlignment depthAlignment = DepthAlignment.BackEdge;

        [Tooltip("Extra local Z offset after edge alignment. Positive moves along the zone forward axis.")]
        public float depthOffset = 0f;

        [Header("Rotation")]
        [Tooltip("Yaw correction applied after the zone rotation. Use this when building prefabs face backward/sideways relative to the zone.")]
        public float buildingYawOffset = 180f;

        [Header("Fill")]
        public bool allowAlleys = true;
        [Range(0f, 1f)] public float alleyChance = 0.05f;
        [Min(0f)] public float alleyWidthMin = 1.5f;
        [Min(0f)] public float alleyWidthMax = 2.5f;
        [Min(0f)] public float sidePadding = 0.2f;

        [Header("Optional Override Prefabs")]
        [Tooltip("If populated, only these prefabs are used for this zone. This is the preferred way to keep tight zones from picking oversized buildings.")]
        public List<GameObject> buildingPrefabs = new List<GameObject>();

        [Header("Debug")]
        public Color gizmoColor = new Color(0.2f, 1f, 0.4f, 0.85f);

        public Quaternion GetBuildingRotation()
        {
            return transform.rotation * Quaternion.Euler(0f, buildingYawOffset, 0f);
        }

        public Vector3 GetDepthAlignedLocalCenter(float buildingDepth)
        {
            float safeDepth = Mathf.Max(0.1f, buildingDepth);
            float halfZone = Mathf.Max(0f, depth * 0.5f - edgeInset);
            float halfBuilding = safeDepth * 0.5f;

            float z = 0f;
            switch (depthAlignment)
            {
                case DepthAlignment.FrontEdge:
                    z = -halfZone + halfBuilding;
                    break;
                case DepthAlignment.BackEdge:
                    z = halfZone - halfBuilding;
                    break;
                default:
                    z = 0f;
                    break;
            }

            z += depthOffset;
            return new Vector3(0f, 0f, z);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = gizmoColor;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(width, 0.25f, depth));

            Gizmos.color = Color.white;
            float insetWidth = Mathf.Max(0.01f, width - edgeInset * 2f - sidePadding * 2f);
            float insetDepth = Mathf.Max(0.01f, depth - edgeInset * 2f);
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(insetWidth, 0.35f, insetDepth));

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(Vector3.zero, Vector3.forward * Mathf.Max(0.5f, depth * 0.5f));

            // Yellow line shows final building forward after yaw offset.
            Gizmos.color = Color.yellow;
            Quaternion buildingRotation = Quaternion.Euler(0f, buildingYawOffset, 0f);
            Gizmos.DrawLine(Vector3.zero, buildingRotation * Vector3.forward * Mathf.Max(0.5f, depth * 0.4f));

            Gizmos.matrix = Matrix4x4.identity;
        }
    }
}

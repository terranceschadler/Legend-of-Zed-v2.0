using UnityEngine;

namespace LegendOfZed.MapIntegration
{
    public enum ZedAuthoredBuildingLotCategory
    {
        Any = 0,
        Corner = 1,
        SmallShop = 2,
        Apartment = 3,
        Office = 4,
        Filler = 5,
        Alley = 6
    }

    /// <summary>
    /// Explicit building lot marker.
    /// One approved lot spawns at most one building.
    /// Local +Z should point toward the street/building front direction.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ZedAuthoredBuildingLot : MonoBehaviour
    {
        [Header("Runtime")]
        public bool approvedForRuntimeSpawning = true;
        public bool canSpawnBuilding = true;

        [Header("Building Selection")]
        [Tooltip("Limits which building prefabs may spawn on this authored lot. Any allows the whole pool.")]
        public ZedAuthoredBuildingLotCategory buildingCategory = ZedAuthoredBuildingLotCategory.Any;


        [Header("Footprint")]
        [Min(0.5f)] public float width = 8f;
        [Min(0.5f)] public float depth = 8f;

        [Tooltip("Optional prefab for this exact lot. If empty, the spawner chooses from its building prefab list.")]
        public GameObject overrideBuildingPrefab;

        [Header("Debug")]
        public bool drawGizmos = true;
        public Color gizmoColor = new Color(0.35f, 1f, 0.35f, 0.9f);

        public Quaternion BuildingRotation
        {
            get { return Quaternion.LookRotation(transform.forward.FlattenedSafe(Vector3.forward), Vector3.up); }
        }

        public Bounds WorldBounds
        {
            get
            {
                Vector3 size = new Vector3(width, 8f, depth);
                return new Bounds(transform.position + Vector3.up * 4f, size);
            }
        }

        private void OnDrawGizmos()
        {
            if (!drawGizmos)
            {
                return;
            }

            Vector3 right = transform.right.FlattenedSafe(Vector3.right) * (width * 0.5f);
            Vector3 forward = transform.forward.FlattenedSafe(Vector3.forward) * (depth * 0.5f);
            Vector3 center = transform.position;

            Vector3 a = center - right - forward;
            Vector3 b = center + right - forward;
            Vector3 c = center + right + forward;
            Vector3 d = center - right + forward;

            Gizmos.color = approvedForRuntimeSpawning && canSpawnBuilding ? gizmoColor : Color.yellow;
            Gizmos.DrawLine(a, b);
            Gizmos.DrawLine(b, c);
            Gizmos.DrawLine(c, d);
            Gizmos.DrawLine(d, a);

            Gizmos.DrawLine(center, center + transform.forward.FlattenedSafe(Vector3.forward) * 2f);
            Gizmos.DrawSphere(center + transform.forward.FlattenedSafe(Vector3.forward) * 2f, 0.15f);
        }
    }

    internal static class ZedVectorExtensions
    {
        public static Vector3 FlattenedSafe(this Vector3 value, Vector3 fallback)
        {
            value.y = 0f;
            if (value.sqrMagnitude < 0.0001f)
            {
                value = fallback;
                value.y = 0f;
            }

            return value.normalized;
        }
    }
}

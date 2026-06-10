using UnityEngine;

namespace LegendOfZed.MapIntegration
{
    /// <summary>
    /// Footprint metadata for building prefabs used by the lot filler.
    /// Manual footprint sizes are preferred because art bounds can include signs, awnings, pivots, or decorative meshes.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ZedBuildingFootprint : MonoBehaviour
    {
        public enum BuildingSizeClass
        {
            Small,
            Medium,
            Wide,
            Filler,
            Corner
        }

        [Header("Manual Footprint")]
        public bool useManualFootprint = false;
        [Min(0.1f)] public float width = 4f;
        [Min(0.1f)] public float depth = 4f;

        [Header("Catalog")]
        public BuildingSizeClass sizeClass = BuildingSizeClass.Medium;

        [Tooltip("Useful for future zone filtering. Current prototype only requires footprint fit.")]
        public bool allowInTightZones = true;

        [Header("Placement")]
        [Tooltip("Extra width reserved beside this building when placed in a row.")]
        [Min(0f)] public float sideClearance = 0f;

        [Tooltip("Extra depth reserved behind this building.")]
        [Min(0f)] public float rearClearance = 0f;

        public Vector2 GetFootprintSize()
        {
            if (useManualFootprint)
            {
                return new Vector2(Mathf.Max(0.1f, width + sideClearance), Mathf.Max(0.1f, depth + rearClearance));
            }

            return EstimateFootprintSize(gameObject);
        }

        public void SetManualFootprint(float newWidth, float newDepth, BuildingSizeClass newSizeClass)
        {
            useManualFootprint = true;
            width = Mathf.Max(0.1f, newWidth);
            depth = Mathf.Max(0.1f, newDepth);
            sizeClass = newSizeClass;
        }

        public static Vector2 EstimateFootprintSize(GameObject prefabOrInstance)
        {
            if (prefabOrInstance == null)
            {
                return new Vector2(4f, 4f);
            }

            ZedBuildingFootprint footprint = prefabOrInstance.GetComponent<ZedBuildingFootprint>();
            if (footprint != null && footprint.useManualFootprint)
            {
                return footprint.GetFootprintSize();
            }

            Renderer[] renderers = prefabOrInstance.GetComponentsInChildren<Renderer>(true);
            if (renderers == null || renderers.Length == 0)
            {
                return new Vector2(4f, 4f);
            }

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                {
                    bounds.Encapsulate(renderers[i].bounds);
                }
            }

            return new Vector2(Mathf.Max(0.1f, bounds.size.x), Mathf.Max(0.1f, bounds.size.z));
        }

        public static BuildingSizeClass Classify(float footprintWidth)
        {
            if (footprintWidth <= 4.5f)
            {
                return BuildingSizeClass.Small;
            }

            if (footprintWidth <= 7f)
            {
                return BuildingSizeClass.Medium;
            }

            if (footprintWidth <= 10f)
            {
                return BuildingSizeClass.Wide;
            }

            return BuildingSizeClass.Corner;
        }
    }
}

using System.Collections;
using LegendOfZed.LegacyMapGenerator;
using UnityEngine;

namespace LegendOfZed.MapIntegration
{
    public class ZedMapTileGapResolver : MonoBehaviour
    {
        [Header("Generator")]
        public ZedLegacyRandomMapGenerator generator;

        [Header("Safety")]
        [Tooltip("Disabled by default. The resolver was experimental and can place incorrect tiles. Keep off unless debugging.")]
        public bool runAutomatically = false;

        [Header("Legacy Fields Kept For Inspector Compatibility")]
        public GameObject twoWayStraightTile;
        public GameObject twoWayLeftTile;
        public GameObject twoWayRightTile;
        public GameObject threeWayStraightTile;
        public GameObject threeWayLeftTile;
        public GameObject threeWayRightTile;
        public GameObject fourWayTile;
        public float gridSize = 20f;
        public float waitTimeoutSeconds = 10f;
        public bool logDebugDetails;

        private IEnumerator Start()
        {
            if (!runAutomatically)
            {
                Debug.Log("Map tile gap resolver is disabled. Using TileSpawn collision fix instead.", this);
                yield break;
            }

            if (generator == null)
            {
                generator = FindAnyObjectByType<ZedLegacyRandomMapGenerator>();
            }

            float start = Time.realtimeSinceStartup;
            while (generator != null && !generator.bakingNavMeshCompleted)
            {
                if (Time.realtimeSinceStartup - start > waitTimeoutSeconds)
                {
                    break;
                }

                yield return null;
            }

            Debug.LogWarning("Map tile gap resolver automatic fill is disabled for production. Do not use this old resolver for map repair.", this);
        }

        [ContextMenu("Resolve Map Tile Gaps Once")]
        public void ResolveGapsOnce()
        {
            Debug.LogWarning("Map tile gap resolver is intentionally disabled. Fix should come from TileSpawn collision handling, not post-generation guessing.", this);
        }
    }
}

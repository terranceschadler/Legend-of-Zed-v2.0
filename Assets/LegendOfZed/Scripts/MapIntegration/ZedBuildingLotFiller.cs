using System.Collections.Generic;
using LegendOfZed.LegacyMapGenerator;
using UnityEngine;

namespace LegendOfZed.MapIntegration
{
    /// <summary>
    /// Explicit-zone-only building lot filler.
    /// Buildings must fit inside authored ZedBuildableZone strips.
    /// Packing now prefers multiple smaller buildings per zone instead of one max-width building.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ZedBuildingLotFiller : MonoBehaviour
    {
        [Header("Safety")]
        [Tooltip("Only explicit ZedBuildableZone children are supported. Old marker inference is disabled.")]
        public bool requireExplicitBuildableZones = true;

        [Tooltip("Reject buildings that do not fully fit inside the zone width/depth.")]
        public bool requireFullFootprintInsideZone = true;

        [Tooltip("Fallback inset used when a zone has edgeInset set to zero.")]
        [Min(0f)] public float zoneEdgeInset = 0.25f;

        [Header("Building Packing")]
        [Tooltip("Small gap between adjacent buildings. Keep near zero for dense city blocks.")]
        [Min(0f)] public float buildingGap = 0.05f;

        [Tooltip("Prefer placing multiple buildings in each zone instead of one large building.")]
        public bool preferMultipleBuildingsPerZone = true;

        [Tooltip("Target minimum number of buildings per zone when enough fitting prefabs exist.")]
        [Min(1)] public int targetBuildingsPerZone = 2;

        [Tooltip("Avoid picking one building that consumes more than this fraction of the zone width while trying to hit the target count.")]
        [Range(0.25f, 1f)] public float maxPreferredWidthFraction = 0.55f;

        [Tooltip("When true, if a zone cannot fit a row, it will still try to place the single largest prefab that fully fits.")]
        public bool trySingleBestFitIfRowEmpty = true;

        [Tooltip("Prevents runaway loops if a bad prefab footprint is reported.")]
        [Min(1)] public int maxBuildingsPerRow = 24;

        [Header("Diagnostics")]
        public bool logFillResults = false;
        public bool logRejectedBuildings = false;

        private bool filled;

        public bool Fill(ZedLegacyRoomTile roomTile)
        {
            if (filled || roomTile == null)
            {
                return filled;
            }

            ZedBuildableZone[] explicitZones = GetComponentsInChildren<ZedBuildableZone>(true);
            if (explicitZones == null || explicitZones.Length == 0)
            {
                if (logFillResults)
                {
                    Debug.Log("Building lot filler skipped " + gameObject.name + " because it has no explicit ZedBuildableZone children.", this);
                }

                return false;
            }

            List<GameObject> prefabs = GetValidPrefabs(roomTile.buildingPrefabs);
            if (prefabs.Count == 0)
            {
                return false;
            }

            Transform generatedRoot = GetOrCreateGeneratedRoot().transform;
            int placed = 0;

            for (int i = 0; i < explicitZones.Length; i++)
            {
                ZedBuildableZone zone = explicitZones[i];
                if (zone == null)
                {
                    continue;
                }

                List<GameObject> zonePrefabs = zone.buildingPrefabs != null && zone.buildingPrefabs.Count > 0
                    ? GetValidPrefabs(zone.buildingPrefabs)
                    : prefabs;

                placed += FillExplicitZone(zone, zonePrefabs, generatedRoot);
            }

            filled = placed > 0;

            if (logFillResults)
            {
                Debug.Log("Explicit building lot filler placed " + placed + " buildings for " + gameObject.name + ".", this);
            }

            return filled;
        }

        private int FillExplicitZone(ZedBuildableZone zone, List<GameObject> prefabs, Transform parent)
        {
            if (zone == null || prefabs == null || prefabs.Count == 0)
            {
                return 0;
            }

            float localInset = zone.edgeInset > 0f ? zone.edgeInset : Mathf.Max(0f, zoneEdgeInset);
            float usableWidth = Mathf.Max(0f, zone.width - zone.sidePadding * 2f - localInset * 2f);
            float usableDepth = Mathf.Max(0f, zone.depth - localInset * 2f);

            if (usableWidth <= 0.1f || usableDepth <= 0.1f)
            {
                return 0;
            }

            float cursor = -usableWidth * 0.5f;
            int placed = 0;
            int safety = Mathf.Max(1, maxBuildingsPerRow);
            float zoneAlleyChance = zone.allowAlleys ? zone.alleyChance : 0f;

            float smallestFittingWidth = GetSmallestFittingWidth(prefabs, usableDepth);
            if (smallestFittingWidth <= 0f)
            {
                return 0;
            }

            while (cursor < usableWidth * 0.5f && safety-- > 0)
            {
                float remaining = usableWidth * 0.5f - cursor;
                GameObject prefab = PickBuildingThatFits(prefabs, remaining, usableDepth, usableWidth, smallestFittingWidth, placed);
                if (prefab == null)
                {
                    break;
                }

                Vector2 footprint = ZedBuildingFootprint.EstimateFootprintSize(prefab);
                float buildingWidth = Mathf.Max(0.1f, footprint.x);
                float buildingDepth = Mathf.Max(0.1f, footprint.y);

                if (requireFullFootprintInsideZone && (buildingWidth > remaining || buildingDepth > usableDepth))
                {
                    break;
                }

                float centerOffset = cursor + buildingWidth * 0.5f;
                Vector3 localDepthOffset = zone.GetDepthAlignedLocalCenter(buildingDepth);
                Vector3 position = zone.transform.position + zone.transform.right * centerOffset + zone.transform.forward * localDepthOffset.z;

                if (!IsFootprintInsideZone(zone, position, buildingWidth, buildingDepth, localInset))
                {
                    if (logRejectedBuildings)
                    {
                        Debug.Log("Rejected building " + prefab.name + " for zone " + zone.name + " because final placement would leave the zone.", this);
                    }

                    cursor += Mathf.Max(0.25f, buildingWidth + buildingGap);
                    continue;
                }

                SpawnBuilding(prefab, position, zone.GetBuildingRotation(), parent);
                placed++;
                cursor += buildingWidth + buildingGap;

                // Do not insert an alley until we have reached the target count. Otherwise alleys can prevent a second building.
                if (placed >= Mathf.Max(1, targetBuildingsPerZone) && Random.value < zoneAlleyChance)
                {
                    float min = Mathf.Min(zone.alleyWidthMin, zone.alleyWidthMax);
                    float max = Mathf.Max(zone.alleyWidthMin, zone.alleyWidthMax);
                    cursor += Random.Range(min, max);
                }
            }

            if (placed == 0 && trySingleBestFitIfRowEmpty)
            {
                return TryPlaceSingleBestFit(zone, prefabs, parent, localInset, usableWidth, usableDepth);
            }

            return placed;
        }

        private GameObject PickBuildingThatFits(
            List<GameObject> prefabs,
            float remainingWidth,
            float usableDepth,
            float totalUsableWidth,
            float smallestFittingWidth,
            int alreadyPlaced)
        {
            if (prefabs == null || prefabs.Count == 0)
            {
                return null;
            }

            List<GameObject> candidates = new List<GameObject>();
            int targetCount = Mathf.Max(1, targetBuildingsPerZone);
            int remainingTargetSlots = Mathf.Max(0, targetCount - alreadyPlaced - 1);

            float reservedForFuture = 0f;
            if (preferMultipleBuildingsPerZone && remainingTargetSlots > 0)
            {
                reservedForFuture = remainingTargetSlots * smallestFittingWidth + remainingTargetSlots * buildingGap;
            }

            float maxAllowedWidth = remainingWidth - reservedForFuture;

            if (preferMultipleBuildingsPerZone && alreadyPlaced < targetCount - 1)
            {
                maxAllowedWidth = Mathf.Min(maxAllowedWidth, totalUsableWidth * maxPreferredWidthFraction);
            }

            if (maxAllowedWidth <= 0.1f)
            {
                maxAllowedWidth = remainingWidth;
            }

            for (int i = 0; i < prefabs.Count; i++)
            {
                GameObject prefab = prefabs[i];
                if (prefab == null)
                {
                    continue;
                }

                Vector2 footprint = ZedBuildingFootprint.EstimateFootprintSize(prefab);
                float width = Mathf.Max(0.1f, footprint.x);
                float depth = Mathf.Max(0.1f, footprint.y);

                if (requireFullFootprintInsideZone)
                {
                    if (width <= remainingWidth && depth <= usableDepth && width <= maxAllowedWidth)
                    {
                        candidates.Add(prefab);
                    }
                }
                else if (depth <= usableDepth && width <= maxAllowedWidth)
                {
                    candidates.Add(prefab);
                }
            }

            // If the multi-building preference filtered out everything, relax the maxPreferredWidth rule but keep strict zone containment.
            if (candidates.Count == 0)
            {
                for (int i = 0; i < prefabs.Count; i++)
                {
                    GameObject prefab = prefabs[i];
                    if (prefab == null)
                    {
                        continue;
                    }

                    Vector2 footprint = ZedBuildingFootprint.EstimateFootprintSize(prefab);
                    float width = Mathf.Max(0.1f, footprint.x);
                    float depth = Mathf.Max(0.1f, footprint.y);

                    if (width <= remainingWidth && depth <= usableDepth)
                    {
                        candidates.Add(prefab);
                    }
                }
            }

            if (candidates.Count == 0)
            {
                return null;
            }

            // Prefer medium/small candidates while still allowing variety.
            candidates.Sort((a, b) =>
            {
                float aw = ZedBuildingFootprint.EstimateFootprintSize(a).x;
                float bw = ZedBuildingFootprint.EstimateFootprintSize(b).x;

                float preferred = preferMultipleBuildingsPerZone && alreadyPlaced < targetCount
                    ? totalUsableWidth / targetCount
                    : totalUsableWidth;

                float da = Mathf.Abs(aw - preferred);
                float db = Mathf.Abs(bw - preferred);
                return da.CompareTo(db);
            });

            int topCount = Mathf.Clamp(Mathf.CeilToInt(candidates.Count * 0.5f), 1, candidates.Count);
            return candidates[Random.Range(0, topCount)];
        }

        private float GetSmallestFittingWidth(List<GameObject> prefabs, float usableDepth)
        {
            float smallest = float.MaxValue;

            if (prefabs == null)
            {
                return 0f;
            }

            for (int i = 0; i < prefabs.Count; i++)
            {
                GameObject prefab = prefabs[i];
                if (prefab == null)
                {
                    continue;
                }

                Vector2 footprint = ZedBuildingFootprint.EstimateFootprintSize(prefab);
                float width = Mathf.Max(0.1f, footprint.x);
                float depth = Mathf.Max(0.1f, footprint.y);

                if (depth <= usableDepth && width < smallest)
                {
                    smallest = width;
                }
            }

            return smallest == float.MaxValue ? 0f : smallest;
        }

        private int TryPlaceSingleBestFit(ZedBuildableZone zone, List<GameObject> prefabs, Transform parent, float inset, float usableWidth, float usableDepth)
        {
            GameObject prefab = PickSingleLargestFit(prefabs, usableWidth, usableDepth);
            if (prefab == null)
            {
                return 0;
            }

            Vector2 footprint = ZedBuildingFootprint.EstimateFootprintSize(prefab);
            float width = Mathf.Max(0.1f, footprint.x);
            float depth = Mathf.Max(0.1f, footprint.y);

            Vector3 localDepthOffset = zone.GetDepthAlignedLocalCenter(depth);
            Vector3 position = zone.transform.position + zone.transform.forward * localDepthOffset.z;

            if (!IsFootprintInsideZone(zone, position, width, depth, inset))
            {
                return 0;
            }

            SpawnBuilding(prefab, position, zone.GetBuildingRotation(), parent);
            return 1;
        }

        private GameObject PickSingleLargestFit(List<GameObject> prefabs, float usableWidth, float usableDepth)
        {
            GameObject best = null;
            float bestWidth = -1f;

            if (prefabs == null)
            {
                return null;
            }

            for (int i = 0; i < prefabs.Count; i++)
            {
                GameObject prefab = prefabs[i];
                if (prefab == null)
                {
                    continue;
                }

                Vector2 footprint = ZedBuildingFootprint.EstimateFootprintSize(prefab);
                float width = Mathf.Max(0.1f, footprint.x);
                float depth = Mathf.Max(0.1f, footprint.y);

                if (width <= usableWidth && depth <= usableDepth && width > bestWidth)
                {
                    best = prefab;
                    bestWidth = width;
                }
            }

            return best;
        }

        private void SpawnBuilding(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent)
        {
            GameObject instance = Instantiate(prefab, position, rotation, parent);
            instance.name = prefab.name;
        }

        private bool IsFootprintInsideZone(ZedBuildableZone zone, Vector3 worldCenter, float width, float depth, float inset)
        {
            if (zone == null)
            {
                return false;
            }

            Vector3 local = zone.transform.InverseTransformPoint(worldCenter);
            float halfAllowedWidth = Mathf.Max(0f, zone.width * 0.5f - zone.sidePadding - inset);
            float halfAllowedDepth = Mathf.Max(0f, zone.depth * 0.5f - inset);
            float halfBuildingWidth = width * 0.5f;
            float halfBuildingDepth = depth * 0.5f;

            if (Mathf.Abs(local.x) + halfBuildingWidth > halfAllowedWidth)
            {
                return false;
            }

            if (Mathf.Abs(local.z) + halfBuildingDepth > halfAllowedDepth)
            {
                return false;
            }

            return true;
        }

        private GameObject GetOrCreateGeneratedRoot()
        {
            Transform existing = transform.Find("Generated_Building_Lots");
            if (existing != null)
            {
                return existing.gameObject;
            }

            GameObject root = new GameObject("Generated_Building_Lots");
            root.transform.SetParent(transform, false);
            return root;
        }

        private static List<GameObject> GetValidPrefabs(List<GameObject> source)
        {
            List<GameObject> valid = new List<GameObject>();
            if (source == null)
            {
                return valid;
            }

            for (int i = 0; i < source.Count; i++)
            {
                if (source[i] != null && !valid.Contains(source[i]))
                {
                    valid.Add(source[i]);
                }
            }

            return valid;
        }
    }
}

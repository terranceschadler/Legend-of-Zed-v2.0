using System.Collections;
using System.Collections.Generic;
using LegendOfZed.LegacyMapGenerator;
using UnityEngine;

namespace LegendOfZed.MapIntegration
{
    /// <summary>
    /// v3.9i frontage-strip building spawner.
    ///
    /// Buildings are spawned from detected road edges instead of zone centers.
    /// Rule: road edge -> frontage strip -> building front near road -> building faces road.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ZedFrontageStripBuildingSpawnerV39I : MonoBehaviour
    {
        [Header("References")]
        public Transform scanRoot;
        public GameObject[] buildingPrefabs;

        [Header("Generated Root")]
        public string generatedBuildingRootName = "Generated_RoadFrontage_Buildings";
        public bool clearPreviousGeneratedBuildings = true;

        [Header("Timing")]
        public bool buildOnStart = true;
        [Min(0f)] public float startDelay = 0.85f;

        [Header("V3.9I3 Runtime Fix")]
        [Tooltip("Force facing/density/sidewalk setback values at runtime so older serialized Inspector values do not keep overriding this patch.")]
        public bool applyV39I3RuntimeFix = true;

        [Header("Frontage Strip Rules")]
        [Tooltip("Maximum number of buildings for the whole map.")]
        [Min(1)] public int maxBuildingsTotal = 280;

        [Tooltip("Try to keep filling frontage strips until at least this many buildings are placed, if valid frontage remains.")]
        [Min(1)] public int targetBuildingsTotal = 230;

        [Tooltip("Maximum number of buildings on a single frontage strip.")]
        [Min(1)] public int maxBuildingsPerStrip = 7;

        [Header("Dead-End Tile Coverage")]
        [Tooltip("Dead-end road pieces often do not merge into long road runs. Add extra tile-edge frontage strips for those tiles.")]
        public bool boostDeadEndTiles = true;

        [Tooltip("Tile name terms that count as dead-end tiles.")]
        public string[] deadEndTileNameContains =
        {
            "deadend",
            "dead-end",
            "dead end"
        };

        [Tooltip("Maximum total extra strips to add per dead-end tile.")]
        [Min(1)] public int maxDeadEndExtraStripsPerTile = 3;

        [Tooltip("Inset from dead-end tile bounds when adding supplemental frontage strips.")]
        [Min(0f)] public float deadEndTileEdgeInset = 3.5f;

        [Tooltip("Minimum strip length for supplemental dead-end tile strips.")]
        [Min(2f)] public float minDeadEndStripLength = 7f;

        [Tooltip("Extra buildings allowed per dead-end supplemental strip.")]
        [Min(1)] public int maxBuildingsPerDeadEndStrip = 4;

        [Tooltip("Prefer creating strips on tile edges that are not already close to a merged road strip.")]
        [Min(0f)] public float deadEndExistingStripRejectDistance = 4f;

        [Tooltip("Minimum distance from a supplemental dead-end strip to detected roads. Prevents placing directly over the road cap.")]
        [Min(0f)] public float deadEndRoadClearance = 1.0f;

        [Tooltip("Flip supplemental dead-end strip normals so building local +Z faces back toward the dead-end street instead of away from it.")]
        public bool flipDeadEndSupplementFacing = true;

        [Tooltip("Minimum distance from a supplemental dead-end strip to map boundary wall.")]
        [Min(0f)] public float deadEndBoundaryClearance = 3.0f;

        [Tooltip("Minimum usable road-edge length needed before a strip can spawn buildings.")]
        [Min(2f)] public float minStripLength = 8f;

        [Tooltip("Trim this much from both strip ends so buildings do not sit directly on intersections/corners.")]
        [Min(0f)] public float stripEndInset = 2.0f;

        [Tooltip("Distance from the road edge to the building front face. This is the consistent sidewalk/curb space.")]
        [Min(0f)] public float frontSetbackFromRoad = 3.5f;

        [Tooltip("Maximum allowed final visual front-edge error after bounds anchoring.")]
        [Min(0f)] public float maxFrontSetbackError = 0.2f;

        [Tooltip("Reject a building if its final center is too far behind the intended frontage row.")]
        public bool rejectBackRowDrift = true;

        [Tooltip("Extra allowed distance behind the expected building depth before treating the placement as a back-row drift.")]
        [Min(0f)] public float backRowDriftTolerance = 0.75f;

        [Tooltip("After visual-bounds front anchoring, run overlap/road checks again. This prevents final shifted buildings from intersecting.")]
        public bool finalBoundsSafetyChecks = true;

        [Tooltip("Only allow one building side per frontage sample point. Helps prevent duplicate parallel road chunk runs from creating back rows.")]
        public bool rejectIfBehindExistingFrontageBuilding = true;

        [Header("Frontage Density")]
        [Tooltip("Minimum distance along a similar frontage side before another building can be placed.")]
        [Min(0f)] public float sameFrontageAlongSpacing = 0.75f;

        [Tooltip("Maximum distance between frontage lines for them to count as the same row.")]
        [Min(0f)] public float sameFrontageLineTolerance = 3.0f;

        [Tooltip("Gap between neighboring buildings on the same frontage.")]
        [Min(0f)] public float buildingGap = 0.8f;

        [Tooltip("Small random side-to-side offset along the frontage row.")]
        [Min(0f)] public float alongStripJitter = 0.15f;

        [Tooltip("Yaw correction for the current building art. Base value before per-prefab road-facing correction.")]
        public float buildingYawOffset = 0f;

        [Header("Per-Prefab Road Facing Correction")]
        [Tooltip("Disabled for this project because all active building prefabs are authored with +Z as the front/facade direction.")]
        public bool autoCorrectPrefabRoadFacing = false;

        [Tooltip("Force each spawned building's local +Z to point toward the road. This is the correct rule for the active building art.")]
        public bool forceLocalZForwardTowardRoad = true;

        [Tooltip("Yaw variants tested after the base road-facing rotation. Covers prefabs authored on +Z, -Z, +X, and -X front axes.")]
        public float[] roadFacingYawVariants = { 0f, 180f, 90f, -90f };

        [Tooltip("How strongly the orientation scorer prefers the long building side along the road.")]
        [Min(0f)] public float frontageWidthScoreWeight = 2f;

        [Tooltip("How strongly the orientation scorer penalizes depth pushing away from the road.")]
        [Min(0f)] public float frontageDepthPenaltyWeight = 1f;

        [Header("Road Shape Filtering")]
        [Tooltip("A road rect must be at least this long in its dominant direction.")]
        [Min(1f)] public float minRoadLongAxis = 7f;

        [Tooltip("A road rect must be no wider than this in its short direction.")]
        [Min(1f)] public float maxRoadShortAxis = 12f;

        [Tooltip("Dominant axis must be this much longer than the short axis to count as a road segment instead of an intersection.")]
        [Min(1f)] public float roadAxisRatio = 1.25f;

        [Header("Merged Road Chunk Frontage")]
        [Tooltip("Merge small road renderer chunks into longer street rows/columns before building frontage strips.")]
        public bool mergeRoadChunksIntoStreetRuns = true;

        [Tooltip("Maximum center-line distance for road chunks to be considered part of the same row or column.")]
        [Min(0.1f)] public float roadRunLineTolerance = 2.5f;

        [Tooltip("Maximum gap between adjacent road chunks that still merges into one street run.")]
        [Min(0f)] public float roadRunGapTolerance = 2.5f;

        [Tooltip("Minimum merged road run length before it can produce frontage.")]
        [Min(2f)] public float minMergedRoadRunLength = 10f;

        [Header("Prefab Variety")]
        public bool randomizePrefabChoice = true;
        public bool avoidImmediatePrefabRepeat = true;
        [Min(1)] public int maxPrefabPickAttempts = 16;

        [Header("Safety")]
        public bool rejectAgainstDetectedRoadRenderers = true;
        [Min(0f)] public float roadRejectPadding = 0.05f;

        [Tooltip("Reject buildings outside the detected generated map bounds.")]
        public bool rejectOutsideMapBounds = true;

        [Min(0f)] public float mapBoundsPadding = 1f;

        [Header("Road Detection")]
        public string[] roadNameContains = { "road", "street", "asphalt" };

        public string[] rejectRoadNameContains =
        {
            "sidewalk", "pavement", "curb", "kerb", "line", "marking", "crosswalk",
            "spawn", "trigger", "wall", "building", "prop", "tree", "grass", "park"
        };

        public float maxRoadCenterY = 2f;
        [Min(0.01f)] public float maxRoadRendererHeight = 1.5f;
        [Min(0.01f)] public float minRoadRendererSize = 0.25f;

        [Header("Logging")]
        public bool logDetails = true;

        [Header("Debug")]
        public bool drawDebugGizmos = true;
        public bool drawRejectedGizmos = false;
        public bool drawPlacedGizmos = true;
        public Color stripGizmoColor = new Color(0f, 1f, 1f, 0.85f);
        public Color placedGizmoColor = new Color(0.2f, 1f, 0.2f, 0.75f);
        public Color rejectedGizmoColor = new Color(1f, 0.2f, 0.2f, 0.55f);

        private readonly List<Rect2> roadRects = new List<Rect2>();
        private readonly List<FrontageStrip> frontageStrips = new List<FrontageStrip>();
        private readonly List<Rect2> placedRects = new List<Rect2>();
        private readonly List<PlacedFrontage> placedFrontages = new List<PlacedFrontage>();
        private readonly List<Rect2> rejectedRects = new List<Rect2>();
        private readonly Dictionary<string, int> prefabUsage = new Dictionary<string, int>();

        private Bounds mapBounds;
        private bool hasMapBounds;

        private int roadRenderers;
        private int roadRectsSkippedAsIntersections;
        private int mergedRoadRuns;
        private int stripsBuilt;
        private int deadEndTilesScanned;
        private int deadEndStripsAdded;
        private int stripsUsed;
        private int prefabsAvailable;
        private int uniquePrefabsUsed;
        private int buildingsSpawned;
        private int fitRejects;
        private int roadRejects;
        private int overlapRejects;
        private int boundsRejects;
        private int emptyPrefabRejects;
        private int frontAnchorsAdjusted;
        private int finalOverlapRejects;
        private int finalRoadRejects;
        private int backRowRejects;
        private int frontSetbackRejects;
        private int duplicateFrontageRejects;
        private int orientationCorrections;

        private enum StripSide
        {
            NorthOfRoad,
            SouthOfRoad,
            EastOfRoad,
            WestOfRoad
        }

        private struct Rect2
        {
            public float minX;
            public float maxX;
            public float minZ;
            public float maxZ;

            public float Width { get { return maxX - minX; } }
            public float Depth { get { return maxZ - minZ; } }
            public Vector3 Center { get { return new Vector3((minX + maxX) * 0.5f, 0f, (minZ + maxZ) * 0.5f); } }
        }

        private struct FrontageStrip
        {
            public Vector3 start;
            public Vector3 end;
            public Vector3 along;
            public Vector3 awayFromRoad;
            public float length;
            public StripSide side;
            public bool deadEndSupplement;
        }

        private struct RoadRun
        {
            public bool horizontal;
            public float minAlong;
            public float maxAlong;
            public float minAcross;
            public float maxAcross;
        }

        private struct PlacedFrontage
        {
            public Vector3 roadEdgePoint;
            public Vector3 along;
            public Vector3 awayFromRoad;
            public float halfWidth;
        }

        private sealed class Candidate
        {
            public GameObject prefab;
            public Vector2 size;
        }

        private IEnumerator Start()
        {
            if (!buildOnStart)
            {
                yield break;
            }

            if (startDelay > 0f)
            {
                yield return new WaitForSeconds(startDelay);
            }

            BuildNow();
        }

        private void OnValidate()
        {
            startDelay = Mathf.Max(0f, startDelay);
            maxBuildingsTotal = Mathf.Max(1, maxBuildingsTotal);
            targetBuildingsTotal = Mathf.Clamp(targetBuildingsTotal, 1, maxBuildingsTotal);
            maxBuildingsPerStrip = Mathf.Max(1, maxBuildingsPerStrip);
            maxDeadEndExtraStripsPerTile = Mathf.Max(1, maxDeadEndExtraStripsPerTile);
            deadEndTileEdgeInset = Mathf.Max(0f, deadEndTileEdgeInset);
            minDeadEndStripLength = Mathf.Max(2f, minDeadEndStripLength);
            maxBuildingsPerDeadEndStrip = Mathf.Max(1, maxBuildingsPerDeadEndStrip);
            deadEndExistingStripRejectDistance = Mathf.Max(0f, deadEndExistingStripRejectDistance);
            deadEndRoadClearance = Mathf.Max(0f, deadEndRoadClearance);
            deadEndBoundaryClearance = Mathf.Max(0f, deadEndBoundaryClearance);
            minStripLength = Mathf.Max(2f, minStripLength);
            stripEndInset = Mathf.Max(0f, stripEndInset);
            frontSetbackFromRoad = Mathf.Max(0f, frontSetbackFromRoad);
            maxFrontSetbackError = Mathf.Max(0f, maxFrontSetbackError);
            backRowDriftTolerance = Mathf.Max(0f, backRowDriftTolerance);
            sameFrontageAlongSpacing = Mathf.Max(0f, sameFrontageAlongSpacing);
            sameFrontageLineTolerance = Mathf.Max(0f, sameFrontageLineTolerance);
            buildingGap = Mathf.Max(0f, buildingGap);
            alongStripJitter = Mathf.Max(0f, alongStripJitter);
            minRoadLongAxis = Mathf.Max(1f, minRoadLongAxis);
            maxRoadShortAxis = Mathf.Max(1f, maxRoadShortAxis);
            roadAxisRatio = Mathf.Max(1f, roadAxisRatio);
            frontageWidthScoreWeight = Mathf.Max(0f, frontageWidthScoreWeight);
            frontageDepthPenaltyWeight = Mathf.Max(0f, frontageDepthPenaltyWeight);
            if (roadFacingYawVariants == null || roadFacingYawVariants.Length == 0)
            {
                roadFacingYawVariants = new float[] { 0f, 180f, 90f, -90f };
            }
            roadRunLineTolerance = Mathf.Max(0.1f, roadRunLineTolerance);
            roadRunGapTolerance = Mathf.Max(0f, roadRunGapTolerance);
            minMergedRoadRunLength = Mathf.Max(2f, minMergedRoadRunLength);
            maxPrefabPickAttempts = Mathf.Max(1, maxPrefabPickAttempts);
            roadRejectPadding = Mathf.Max(0f, roadRejectPadding);
            mapBoundsPadding = Mathf.Max(0f, mapBoundsPadding);
            maxRoadRendererHeight = Mathf.Max(0.01f, maxRoadRendererHeight);
            minRoadRendererSize = Mathf.Max(0.01f, minRoadRendererSize);
        }

        [ContextMenu("Build V3.9I Frontage Strip Buildings Now")]
        public void BuildNow()
        {
            OnValidate();
            ApplyV39I3RuntimeFixIfEnabled();
            ResetState();

            ResolveScanRoot();
            CalculateMapBounds();
            DetectRoadRects();
            BuildFrontageStrips();

            if (boostDeadEndTiles)
            {
                AddDeadEndTileFrontageStrips();
            }

            Transform buildingRoot = GetOrCreateRoot(generatedBuildingRootName);
            if (clearPreviousGeneratedBuildings)
            {
                ClearChildren(buildingRoot);
            }

            List<Candidate> candidates = BuildCandidateList();
            prefabsAvailable = candidates.Count;

            if (candidates.Count == 0)
            {
                emptyPrefabRejects++;
                LogSummary();
                return;
            }

            for (int i = 0; i < frontageStrips.Count && buildingsSpawned < maxBuildingsTotal; i++)
            {
                SpawnAlongStrip(frontageStrips[i], candidates, buildingRoot);
            }

            if (buildingsSpawned < targetBuildingsTotal)
            {
                RunCoverageFillPass(candidates, buildingRoot);
            }

            uniquePrefabsUsed = prefabUsage.Count;
            LogSummary();
        }

        private void ApplyV39I3RuntimeFixIfEnabled()
        {
            if (!applyV39I3RuntimeFix)
            {
                return;
            }

            // v3.9i5: use yaw 0 as base, then auto-correct per prefab.
            buildingYawOffset = 0f;
            autoCorrectPrefabRoadFacing = false;
            forceLocalZForwardTowardRoad = true;
            roadFacingYawVariants = new float[] { 0f, 180f, 90f, -90f };

            // Fill the city, but keep frontage rows disciplined.
            maxBuildingsTotal = 280;
            targetBuildingsTotal = 230;
            maxBuildingsPerStrip = 7;

            // Keep buildings off intersections/corners, but do not starve long frontage rows.
            stripEndInset = 2.0f;

            // Leave room for sidewalk/curb space between the road edge and building front.
            frontSetbackFromRoad = 3.5f;
            maxFrontSetbackError = 0.2f;
            rejectBackRowDrift = true;
            backRowDriftTolerance = 0.75f;
            finalBoundsSafetyChecks = true;
            rejectIfBehindExistingFrontageBuilding = true;
            sameFrontageAlongSpacing = 0.75f;
            sameFrontageLineTolerance = 3.0f;

            // Keep storefront rows reasonably dense.
            buildingGap = 0.55f;

            // Keep rejected debug clutter hidden.
            drawRejectedGizmos = false;
        }

        private void ResetState()
        {
            roadRects.Clear();
            frontageStrips.Clear();
            placedRects.Clear();
            placedFrontages.Clear();
            rejectedRects.Clear();
            prefabUsage.Clear();

            hasMapBounds = false;
            mapBounds = default;

            roadRenderers = 0;
            roadRectsSkippedAsIntersections = 0;
            mergedRoadRuns = 0;
            stripsBuilt = 0;
            deadEndTilesScanned = 0;
            deadEndStripsAdded = 0;
            stripsUsed = 0;
            prefabsAvailable = 0;
            uniquePrefabsUsed = 0;
            buildingsSpawned = 0;
            fitRejects = 0;
            roadRejects = 0;
            overlapRejects = 0;
            boundsRejects = 0;
            emptyPrefabRejects = 0;
            frontAnchorsAdjusted = 0;
            finalOverlapRejects = 0;
            finalRoadRejects = 0;
            backRowRejects = 0;
            frontSetbackRejects = 0;
            duplicateFrontageRejects = 0;
            orientationCorrections = 0;
        }

        private void LogSummary()
        {
            if (!logDetails)
            {
                return;
            }

            Debug.Log(
                "V3.9I10 SIDEWALK SETBACK INCREASE complete. RoadRenderers=" + roadRenderers +
                ", RoadRects=" + roadRects.Count +
                ", IntersectionsSkipped=" + roadRectsSkippedAsIntersections +
                ", MergedRoadRuns=" + mergedRoadRuns +
                ", StripsBuilt=" + stripsBuilt +
                ", DeadEndTilesScanned=" + deadEndTilesScanned +
                ", DeadEndStripsAdded=" + deadEndStripsAdded +
                ", StripsUsed=" + stripsUsed +
                ", PrefabsAvailable=" + prefabsAvailable +
                ", UniquePrefabsUsed=" + uniquePrefabsUsed +
                ", Buildings=" + buildingsSpawned +
                ", TargetBuildings=" + targetBuildingsTotal +
                ", FitRejects=" + fitRejects +
                ", RoadRejects=" + roadRejects +
                ", OverlapRejects=" + overlapRejects +
                ", BoundsRejects=" + boundsRejects +
                ", EmptyPrefabRejects=" + emptyPrefabRejects +
                ", FrontAnchorsAdjusted=" + frontAnchorsAdjusted +
                ", FinalOverlapRejects=" + finalOverlapRejects +
                ", FinalRoadRejects=" + finalRoadRejects +
                ", BackRowRejects=" + backRowRejects +
                ", FrontSetbackRejects=" + frontSetbackRejects +
                ", DuplicateFrontageRejects=" + duplicateFrontageRejects +
                ", OrientationCorrections=" + orientationCorrections +
                ", RuntimeFix=" + applyV39I3RuntimeFix +
                ", ForceZForward=" + forceLocalZForwardTowardRoad +
                ", FlipDeadEndFacing=" + flipDeadEndSupplementFacing +
                ", YawOffset=" + buildingYawOffset +
                ", FrontSetback=" + frontSetbackFromRoad + ".",
                this);
        }

        private void AddDeadEndTileFrontageStrips()
        {
            Transform root = scanRoot;
            if (root == null)
            {
                ResolveScanRoot();
                root = scanRoot;
            }

            ZedLegacyRoomTile[] tiles = null;
            if (root != null)
            {
                tiles = root.GetComponentsInChildren<ZedLegacyRoomTile>(true);
            }

            if (tiles == null || tiles.Length == 0)
            {
                tiles = FindObjectsByType<ZedLegacyRoomTile>(FindObjectsInactive.Exclude);
            }

            if (tiles == null)
            {
                return;
            }

            for (int i = 0; i < tiles.Length; i++)
            {
                ZedLegacyRoomTile tile = tiles[i];
                if (tile == null || !IsDeadEndTile(tile.name))
                {
                    continue;
                }

                deadEndTilesScanned++;
                AddDeadEndStripsForTile(tile);
            }

            stripsBuilt = frontageStrips.Count;
        }

        private bool IsDeadEndTile(string tileName)
        {
            if (string.IsNullOrEmpty(tileName) || deadEndTileNameContains == null)
            {
                return false;
            }

            string lower = tileName.ToLowerInvariant();
            for (int i = 0; i < deadEndTileNameContains.Length; i++)
            {
                string term = deadEndTileNameContains[i];
                if (!string.IsNullOrEmpty(term) && lower.Contains(term.ToLowerInvariant()))
                {
                    return true;
                }
            }

            return false;
        }

        private void AddDeadEndStripsForTile(ZedLegacyRoomTile tile)
        {
            Bounds bounds;
            if (!TryCalculateRendererBounds(tile.gameObject, out bounds))
            {
                return;
            }

            List<FrontageStrip> candidates = new List<FrontageStrip>();
            TryAddDeadEndCandidate(candidates, MakeDeadEndNorthStrip(bounds));
            TryAddDeadEndCandidate(candidates, MakeDeadEndSouthStrip(bounds));
            TryAddDeadEndCandidate(candidates, MakeDeadEndEastStrip(bounds));
            TryAddDeadEndCandidate(candidates, MakeDeadEndWestStrip(bounds));

            candidates.Sort((a, b) =>
            {
                float da = DistanceToNearestExistingStrip(a);
                float db = DistanceToNearestExistingStrip(b);
                return db.CompareTo(da);
            });

            int added = 0;
            for (int i = 0; i < candidates.Count && added < maxDeadEndExtraStripsPerTile; i++)
            {
                FrontageStrip strip = candidates[i];

                if (DistanceToNearestExistingStrip(strip) < deadEndExistingStripRejectDistance)
                {
                    continue;
                }

                frontageStrips.Add(strip);
                deadEndStripsAdded++;
                added++;
            }
        }

        private FrontageStrip MakeDeadEndNorthStrip(Bounds b)
        {
            float z = b.max.z - deadEndTileEdgeInset;
            Vector3 start = new Vector3(b.min.x + deadEndTileEdgeInset, 0f, z);
            Vector3 end = new Vector3(b.max.x - deadEndTileEdgeInset, 0f, z);
            return MakeDeadEndStrip(start, end, Vector3.right, flipDeadEndSupplementFacing ? Vector3.forward : Vector3.back, StripSide.SouthOfRoad);
        }

        private FrontageStrip MakeDeadEndSouthStrip(Bounds b)
        {
            float z = b.min.z + deadEndTileEdgeInset;
            Vector3 start = new Vector3(b.min.x + deadEndTileEdgeInset, 0f, z);
            Vector3 end = new Vector3(b.max.x - deadEndTileEdgeInset, 0f, z);
            return MakeDeadEndStrip(start, end, Vector3.right, flipDeadEndSupplementFacing ? Vector3.back : Vector3.forward, StripSide.NorthOfRoad);
        }

        private FrontageStrip MakeDeadEndEastStrip(Bounds b)
        {
            float x = b.max.x - deadEndTileEdgeInset;
            Vector3 start = new Vector3(x, 0f, b.min.z + deadEndTileEdgeInset);
            Vector3 end = new Vector3(x, 0f, b.max.z - deadEndTileEdgeInset);
            return MakeDeadEndStrip(start, end, Vector3.forward, flipDeadEndSupplementFacing ? Vector3.right : Vector3.left, StripSide.WestOfRoad);
        }

        private FrontageStrip MakeDeadEndWestStrip(Bounds b)
        {
            float x = b.min.x + deadEndTileEdgeInset;
            Vector3 start = new Vector3(x, 0f, b.min.z + deadEndTileEdgeInset);
            Vector3 end = new Vector3(x, 0f, b.max.z - deadEndTileEdgeInset);
            return MakeDeadEndStrip(start, end, Vector3.forward, flipDeadEndSupplementFacing ? Vector3.left : Vector3.right, StripSide.EastOfRoad);
        }

        private FrontageStrip MakeDeadEndStrip(Vector3 start, Vector3 end, Vector3 along, Vector3 awayFromRoad, StripSide side)
        {
            return new FrontageStrip
            {
                start = start,
                end = end,
                along = along.normalized,
                awayFromRoad = awayFromRoad.normalized,
                length = Vector3.Distance(start, end),
                side = side,
                deadEndSupplement = true
            };
        }

        private void TryAddDeadEndCandidate(List<FrontageStrip> candidates, FrontageStrip strip)
        {
            if (candidates == null || strip.length < minDeadEndStripLength)
            {
                return;
            }

            Rect2 probe = BuildDeadEndStripProbe(strip);
            if (rejectOutsideMapBounds && !RectInsideMapBounds(probe))
            {
                return;
            }

            if (RectNearMapBoundary(probe, deadEndBoundaryClearance))
            {
                return;
            }

            if (RectOverlapsAnyRoad(probe, deadEndRoadClearance))
            {
                return;
            }

            candidates.Add(strip);
        }

        private Rect2 BuildDeadEndStripProbe(FrontageStrip strip)
        {
            float probeDepth = Mathf.Max(frontSetbackFromRoad + 4f, 5f);
            Vector3 center = (strip.start + strip.end) * 0.5f + strip.awayFromRoad.normalized * (frontSetbackFromRoad + probeDepth * 0.5f);
            Quaternion rotation = Quaternion.LookRotation((-strip.awayFromRoad).normalized, Vector3.up);
            return BuildWorldAabbFromOrientedFootprint(center, rotation, strip.length, probeDepth);
        }

        private bool RectNearMapBoundary(Rect2 rect, float clearance)
        {
            if (!hasMapBounds)
            {
                return false;
            }

            return rect.minX < mapBounds.min.x + clearance ||
                   rect.maxX > mapBounds.max.x - clearance ||
                   rect.minZ < mapBounds.min.z + clearance ||
                   rect.maxZ > mapBounds.max.z - clearance;
        }

        private float DistanceToNearestExistingStrip(FrontageStrip strip)
        {
            if (frontageStrips.Count == 0)
            {
                return float.MaxValue;
            }

            Vector3 center = (strip.start + strip.end) * 0.5f;
            float best = float.MaxValue;

            for (int i = 0; i < frontageStrips.Count; i++)
            {
                Vector3 other = (frontageStrips[i].start + frontageStrips[i].end) * 0.5f;
                float distance = Vector3.Distance(center, other);
                if (distance < best)
                {
                    best = distance;
                }
            }

            return best;
        }

        private void RunCoverageFillPass(List<Candidate> candidates, Transform buildingRoot)
        {
            if (candidates == null || buildingRoot == null)
            {
                return;
            }

            float oldSameLineTolerance = sameFrontageLineTolerance;
            float oldSameAlongSpacing = sameFrontageAlongSpacing;
            int oldMaxPerStrip = maxBuildingsPerStrip;

            // Fill obvious gaps but do not fully remove safety.
            sameFrontageLineTolerance = Mathf.Min(sameFrontageLineTolerance, 2.0f);
            sameFrontageAlongSpacing = Mathf.Min(sameFrontageAlongSpacing, 0.5f);
            maxBuildingsPerStrip = Mathf.Max(maxBuildingsPerStrip, 8);

            for (int i = 0; i < frontageStrips.Count && buildingsSpawned < targetBuildingsTotal && buildingsSpawned < maxBuildingsTotal; i++)
            {
                SpawnAlongStrip(frontageStrips[i], candidates, buildingRoot);
            }

            sameFrontageLineTolerance = oldSameLineTolerance;
            sameFrontageAlongSpacing = oldSameAlongSpacing;
            maxBuildingsPerStrip = oldMaxPerStrip;
        }

        private void SpawnAlongStrip(FrontageStrip strip, List<Candidate> candidates, Transform buildingRoot)
        {
            if (strip.length < minStripLength || buildingRoot == null)
            {
                return;
            }

            float cursor = stripEndInset;
            int placedOnStrip = 0;
            GameObject lastPrefab = null;

            while (cursor < strip.length - stripEndInset &&
                   placedOnStrip < (strip.deadEndSupplement ? maxBuildingsPerDeadEndStrip : maxBuildingsPerStrip) &&
                   buildingsSpawned < maxBuildingsTotal)
            {
                float remaining = strip.length - stripEndInset - cursor;
                Candidate selected = PickCandidate(candidates, remaining, lastPrefab);

                if (selected == null)
                {
                    fitRejects++;
                    break;
                }

                float width = selected.size.x;
                float depth = selected.size.y;
                float halfWidth = width * 0.5f;

                float alongDistance = cursor + halfWidth;
                if (alongStripJitter > 0f)
                {
                    alongDistance += Random.Range(-alongStripJitter, alongStripJitter);
                }

                Vector3 edgePoint = strip.start + strip.along * alongDistance;
                Vector3 center = edgePoint + strip.awayFromRoad.normalized * (frontSetbackFromRoad + depth * 0.5f);
                Vector3 faceRoad = -strip.awayFromRoad.normalized;
                Quaternion rotation = forceLocalZForwardTowardRoad
                    ? Quaternion.LookRotation(faceRoad.normalized, Vector3.up)
                    : Quaternion.LookRotation(faceRoad.normalized, Vector3.up) * Quaternion.Euler(0f, buildingYawOffset, 0f);

                Rect2 footprint = BuildWorldAabbFromOrientedFootprint(center, rotation, width, depth);

                if (rejectOutsideMapBounds && !RectInsideMapBounds(footprint))
                {
                    boundsRejects++;
                    rejectedRects.Add(footprint);
                    cursor += Mathf.Max(0.5f, width * 0.25f);
                    continue;
                }

                if (RectOverlapsAnyPlaced(footprint))
                {
                    overlapRejects++;
                    rejectedRects.Add(footprint);
                    cursor += Mathf.Max(0.5f, width * 0.25f);
                    continue;
                }

                if (rejectAgainstDetectedRoadRenderers && RectOverlapsAnyRoad(footprint, roadRejectPadding))
                {
                    roadRejects++;
                    rejectedRects.Add(footprint);
                    cursor += Mathf.Max(0.5f, width * 0.25f);
                    continue;
                }

                GameObject instance = Instantiate(selected.prefab, center, rotation, buildingRoot);
                instance.name = selected.prefab.name + "_FrontageBuilding";

                if (autoCorrectPrefabRoadFacing && !forceLocalZForwardTowardRoad)
                {
                    Quaternion correctedRotation;
                    if (TryFindBestRoadFacingRotation(instance, center, faceRoad, strip.along.normalized, strip.awayFromRoad.normalized, out correctedRotation))
                    {
                        if (Quaternion.Angle(instance.transform.rotation, correctedRotation) > 0.1f)
                        {
                            instance.transform.rotation = correctedRotation;
                            orientationCorrections++;
                        }
                    }
                }

                Bounds visualBounds;
                if (TryCalculateRendererBounds(instance, out visualBounds))
                {
                    Vector3 anchoredPosition = CalculateFrontAnchoredPosition(
                        instance.transform.position,
                        visualBounds,
                        edgePoint,
                        strip.awayFromRoad.normalized,
                        frontSetbackFromRoad);

                    Vector3 delta = anchoredPosition - instance.transform.position;
                    delta.y = 0f;

                    if (delta.sqrMagnitude > 0.0001f)
                    {
                        instance.transform.position += delta;
                        frontAnchorsAdjusted++;
                    }

                    if (TryCalculateRendererBounds(instance, out visualBounds))
                    {
                        footprint = new Rect2
                        {
                            minX = visualBounds.min.x,
                            maxX = visualBounds.max.x,
                            minZ = visualBounds.min.z,
                            maxZ = visualBounds.max.z
                        };
                    }
                }

                float finalFrontDistance = CalculateVisualFrontDistance(footprint, edgePoint, strip.awayFromRoad.normalized);
                if (Mathf.Abs(finalFrontDistance - frontSetbackFromRoad) > maxFrontSetbackError)
                {
                    frontSetbackRejects++;
                    rejectedRects.Add(footprint);
                    Destroy(instance);
                    cursor += Mathf.Max(0.5f, width * 0.25f);
                    continue;
                }

                if (rejectBackRowDrift && IsBackRowDrift(edgePoint, center, strip.awayFromRoad.normalized, depth))
                {
                    backRowRejects++;
                    rejectedRects.Add(footprint);
                    Destroy(instance);
                    cursor += Mathf.Max(0.5f, width * 0.25f);
                    continue;
                }

                if (rejectIfBehindExistingFrontageBuilding && OverlapsExistingFrontageSlot(edgePoint, strip.along, strip.awayFromRoad.normalized, width * 0.5f))
                {
                    duplicateFrontageRejects++;
                    rejectedRects.Add(footprint);
                    Destroy(instance);
                    cursor += Mathf.Max(0.5f, width * 0.25f);
                    continue;
                }

                if (rejectOutsideMapBounds && !RectInsideMapBounds(footprint))
                {
                    boundsRejects++;
                    rejectedRects.Add(footprint);
                    Destroy(instance);
                    cursor += Mathf.Max(0.5f, width * 0.25f);
                    continue;
                }

                if (finalBoundsSafetyChecks && RectOverlapsAnyPlaced(footprint))
                {
                    finalOverlapRejects++;
                    rejectedRects.Add(footprint);
                    Destroy(instance);
                    cursor += Mathf.Max(0.5f, width * 0.25f);
                    continue;
                }

                if (rejectAgainstDetectedRoadRenderers && RectOverlapsAnyRoad(footprint, roadRejectPadding))
                {
                    finalRoadRejects++;
                    rejectedRects.Add(footprint);
                    Destroy(instance);
                    cursor += Mathf.Max(0.5f, width * 0.25f);
                    continue;
                }

                placedRects.Add(footprint);
                placedFrontages.Add(new PlacedFrontage
                {
                    roadEdgePoint = edgePoint,
                    along = strip.along.normalized,
                    awayFromRoad = strip.awayFromRoad.normalized,
                    halfWidth = width * 0.5f
                });

                buildingsSpawned++;
                placedOnStrip++;
                lastPrefab = selected.prefab;
                RegisterPrefabUse(selected.prefab);

                cursor += width + buildingGap;
            }

            if (placedOnStrip > 0)
            {
                stripsUsed++;
            }
        }

        private bool TryFindBestRoadFacingRotation(
            GameObject instance,
            Vector3 center,
            Vector3 faceRoad,
            Vector3 alongRoad,
            Vector3 awayFromRoad,
            out Quaternion bestRotation)
        {
            bestRotation = instance != null ? instance.transform.rotation : Quaternion.identity;

            if (instance == null || faceRoad.sqrMagnitude < 0.001f || alongRoad.sqrMagnitude < 0.001f || awayFromRoad.sqrMagnitude < 0.001f)
            {
                return false;
            }

            float[] variants = roadFacingYawVariants;
            if (variants == null || variants.Length == 0)
            {
                variants = new float[] { 0f, 180f, 90f, -90f };
            }

            Quaternion originalRotation = instance.transform.rotation;
            Vector3 originalPosition = instance.transform.position;

            float bestScore = float.NegativeInfinity;
            bool found = false;

            for (int i = 0; i < variants.Length; i++)
            {
                Quaternion trialRotation = Quaternion.LookRotation(faceRoad.normalized, Vector3.up) * Quaternion.Euler(0f, buildingYawOffset + variants[i], 0f);
                instance.transform.SetPositionAndRotation(center, trialRotation);

                Bounds bounds;
                if (!TryCalculateRendererBounds(instance, out bounds))
                {
                    continue;
                }

                float alongSize;
                float depthSize;
                CalculateProjectedBoundsSize(bounds, alongRoad.normalized, awayFromRoad.normalized, out alongSize, out depthSize);

                // Prefer storefront width along the road and lower depth away from the road.
                float score = alongSize * frontageWidthScoreWeight - depthSize * frontageDepthPenaltyWeight;

                // Strongly prefer rotations whose transform forward faces the road, but still allow side-axis prefabs if the footprint proves it.
                float forwardScore = Vector3.Dot(trialRotation * Vector3.forward, faceRoad.normalized);
                score += Mathf.Max(-1f, forwardScore) * 0.25f;

                if (!found || score > bestScore)
                {
                    bestScore = score;
                    bestRotation = trialRotation;
                    found = true;
                }
            }

            instance.transform.SetPositionAndRotation(originalPosition, originalRotation);
            return found;
        }

        private void CalculateProjectedBoundsSize(Bounds bounds, Vector3 alongRoad, Vector3 awayFromRoad, out float alongSize, out float depthSize)
        {
            Vector3[] corners =
            {
                new Vector3(bounds.min.x, bounds.center.y, bounds.min.z),
                new Vector3(bounds.min.x, bounds.center.y, bounds.max.z),
                new Vector3(bounds.max.x, bounds.center.y, bounds.min.z),
                new Vector3(bounds.max.x, bounds.center.y, bounds.max.z)
            };

            float minAlong = float.MaxValue;
            float maxAlong = float.MinValue;
            float minDepth = float.MaxValue;
            float maxDepth = float.MinValue;

            for (int i = 0; i < corners.Length; i++)
            {
                float along = Vector3.Dot(corners[i], alongRoad);
                float depth = Vector3.Dot(corners[i], awayFromRoad);

                minAlong = Mathf.Min(minAlong, along);
                maxAlong = Mathf.Max(maxAlong, along);
                minDepth = Mathf.Min(minDepth, depth);
                maxDepth = Mathf.Max(maxDepth, depth);
            }

            alongSize = Mathf.Max(0f, maxAlong - minAlong);
            depthSize = Mathf.Max(0f, maxDepth - minDepth);
        }

        private float CalculateVisualFrontDistance(Rect2 footprint, Vector3 roadEdgePoint, Vector3 awayFromRoad)
        {
            if (awayFromRoad.sqrMagnitude < 0.001f)
            {
                return frontSetbackFromRoad;
            }

            Vector3 away = awayFromRoad.normalized;

            Vector3[] corners =
            {
                new Vector3(footprint.minX, roadEdgePoint.y, footprint.minZ),
                new Vector3(footprint.minX, roadEdgePoint.y, footprint.maxZ),
                new Vector3(footprint.maxX, roadEdgePoint.y, footprint.minZ),
                new Vector3(footprint.maxX, roadEdgePoint.y, footprint.maxZ)
            };

            float nearest = float.MaxValue;
            for (int i = 0; i < corners.Length; i++)
            {
                float alongAway = Vector3.Dot(corners[i] - roadEdgePoint, away);
                if (alongAway < nearest)
                {
                    nearest = alongAway;
                }
            }

            return nearest;
        }

        private bool IsBackRowDrift(Vector3 roadEdgePoint, Vector3 intendedCenter, Vector3 awayFromRoad, float expectedDepth)
        {
            if (awayFromRoad.sqrMagnitude < 0.001f)
            {
                return false;
            }

            float distance = Vector3.Dot(intendedCenter - roadEdgePoint, awayFromRoad.normalized);
            float maxAllowed = frontSetbackFromRoad + expectedDepth * 0.5f + backRowDriftTolerance;
            return distance > maxAllowed;
        }

        private bool OverlapsExistingFrontageSlot(Vector3 roadEdgePoint, Vector3 along, Vector3 awayFromRoad, float halfWidth)
        {
            if (along.sqrMagnitude < 0.001f || awayFromRoad.sqrMagnitude < 0.001f)
            {
                return false;
            }

            Vector3 a = along.normalized;
            Vector3 n = awayFromRoad.normalized;

            for (int i = 0; i < placedFrontages.Count; i++)
            {
                PlacedFrontage existing = placedFrontages[i];

                // Same side frontage rows: prevents duplicate parallel chunks from placing a second row behind the first.
                if (Vector3.Dot(existing.awayFromRoad, n) < 0.95f)
                {
                    continue;
                }

                float lineDistance = Mathf.Abs(Vector3.Dot(existing.roadEdgePoint - roadEdgePoint, n));
                if (lineDistance > sameFrontageLineTolerance)
                {
                    continue;
                }

                float alongDistance = Mathf.Abs(Vector3.Dot(existing.roadEdgePoint - roadEdgePoint, a));
                float minAllowed = existing.halfWidth + halfWidth + sameFrontageAlongSpacing;
                if (alongDistance < minAllowed)
                {
                    return true;
                }
            }

            return false;
        }

        private Vector3 CalculateFrontAnchoredPosition(
            Vector3 currentPosition,
            Bounds visualBounds,
            Vector3 roadEdgePoint,
            Vector3 awayFromRoad,
            float setback)
        {
            if (awayFromRoad.sqrMagnitude < 0.001f)
            {
                return currentPosition;
            }

            Vector3 away = awayFromRoad.normalized;

            Vector3[] corners =
            {
                new Vector3(visualBounds.min.x, visualBounds.center.y, visualBounds.min.z),
                new Vector3(visualBounds.min.x, visualBounds.center.y, visualBounds.max.z),
                new Vector3(visualBounds.max.x, visualBounds.center.y, visualBounds.min.z),
                new Vector3(visualBounds.max.x, visualBounds.center.y, visualBounds.max.z)
            };

            float nearestAlongAway = float.MaxValue;
            for (int i = 0; i < corners.Length; i++)
            {
                float along = Vector3.Dot(corners[i] - roadEdgePoint, away);
                if (along < nearestAlongAway)
                {
                    nearestAlongAway = along;
                }
            }

            float desiredFrontDistance = setback;
            float move = desiredFrontDistance - nearestAlongAway;
            return currentPosition + away * move;
        }

        private Candidate PickCandidate(List<Candidate> candidates, float remainingWidth, GameObject lastPrefab)
        {
            if (candidates == null || candidates.Count == 0)
            {
                return null;
            }

            if (randomizePrefabChoice)
            {
                for (int attempt = 0; attempt < maxPrefabPickAttempts; attempt++)
                {
                    Candidate candidate = candidates[Random.Range(0, candidates.Count)];
                    if (candidate == null || candidate.prefab == null)
                    {
                        continue;
                    }

                    if (avoidImmediatePrefabRepeat && lastPrefab != null && candidates.Count > 1 && candidate.prefab == lastPrefab)
                    {
                        continue;
                    }

                    if (candidate.size.x <= remainingWidth)
                    {
                        return candidate;
                    }
                }
            }

            for (int i = 0; i < candidates.Count; i++)
            {
                Candidate candidate = candidates[i];
                if (candidate == null || candidate.prefab == null)
                {
                    continue;
                }

                if (avoidImmediatePrefabRepeat && lastPrefab != null && candidates.Count > 1 && candidate.prefab == lastPrefab)
                {
                    continue;
                }

                if (candidate.size.x <= remainingWidth)
                {
                    return candidate;
                }
            }

            for (int i = 0; i < candidates.Count; i++)
            {
                Candidate candidate = candidates[i];
                if (candidate != null && candidate.prefab != null && candidate.size.x <= remainingWidth)
                {
                    return candidate;
                }
            }

            return null;
        }

        private List<Candidate> BuildCandidateList()
        {
            List<Candidate> candidates = new List<Candidate>();

            if (buildingPrefabs == null)
            {
                return candidates;
            }

            HashSet<GameObject> seen = new HashSet<GameObject>();

            for (int i = 0; i < buildingPrefabs.Length; i++)
            {
                GameObject prefab = buildingPrefabs[i];
                if (prefab == null || seen.Contains(prefab))
                {
                    continue;
                }

                seen.Add(prefab);
                Vector2 size = ZedBuildingFootprint.EstimateFootprintSize(prefab);

                if (size.x <= 0.1f || size.y <= 0.1f)
                {
                    continue;
                }

                candidates.Add(new Candidate
                {
                    prefab = prefab,
                    size = size
                });
            }

            return candidates;
        }

        private void RegisterPrefabUse(GameObject prefab)
        {
            if (prefab == null)
            {
                return;
            }

            string key = prefab.name;
            int count;
            prefabUsage.TryGetValue(key, out count);
            prefabUsage[key] = count + 1;
        }

        private void BuildFrontageStrips()
        {
            frontageStrips.Clear();

            if (mergeRoadChunksIntoStreetRuns)
            {
                List<RoadRun> runs = BuildMergedRoadRuns();
                mergedRoadRuns = runs.Count;

                for (int i = 0; i < runs.Count; i++)
                {
                    RoadRun run = runs[i];
                    if (run.horizontal)
                    {
                        AddHorizontalMergedRun(run);
                    }
                    else
                    {
                        AddVerticalMergedRun(run);
                    }
                }

                stripsBuilt = frontageStrips.Count;
                ShuffleStrips();
                return;
            }

            for (int i = 0; i < roadRects.Count; i++)
            {
                Rect2 road = roadRects[i];

                bool horizontal = road.Width >= minRoadLongAxis &&
                                  road.Depth <= maxRoadShortAxis &&
                                  road.Width >= road.Depth * roadAxisRatio;

                bool vertical = road.Depth >= minRoadLongAxis &&
                                road.Width <= maxRoadShortAxis &&
                                road.Depth >= road.Width * roadAxisRatio;

                if (!horizontal && !vertical)
                {
                    roadRectsSkippedAsIntersections++;
                    continue;
                }

                if (horizontal)
                {
                    AddHorizontalStrip(road, true);
                    AddHorizontalStrip(road, false);
                }

                if (vertical)
                {
                    AddVerticalStrip(road, true);
                    AddVerticalStrip(road, false);
                }
            }

            stripsBuilt = frontageStrips.Count;
            ShuffleStrips();
        }

        private List<RoadRun> BuildMergedRoadRuns()
        {
            List<RoadRun> runs = new List<RoadRun>();
            BuildMergedRunsForAxis(true, runs);
            BuildMergedRunsForAxis(false, runs);
            return runs;
        }

        private void BuildMergedRunsForAxis(bool horizontal, List<RoadRun> runs)
        {
            if (runs == null)
            {
                return;
            }

            List<Rect2> sorted = new List<Rect2>(roadRects);
            sorted.Sort((a, b) =>
            {
                float ca = horizontal ? (a.minZ + a.maxZ) * 0.5f : (a.minX + a.maxX) * 0.5f;
                float cb = horizontal ? (b.minZ + b.maxZ) * 0.5f : (b.minX + b.maxX) * 0.5f;
                int lineCompare = ca.CompareTo(cb);
                if (lineCompare != 0)
                {
                    return lineCompare;
                }

                float aa = horizontal ? a.minX : a.minZ;
                float ab = horizontal ? b.minX : b.minZ;
                return aa.CompareTo(ab);
            });

            List<Rect2> line = new List<Rect2>();
            float currentLine = 0f;
            bool hasLine = false;

            for (int i = 0; i < sorted.Count; i++)
            {
                Rect2 rect = sorted[i];
                float lineCenter = horizontal ? (rect.minZ + rect.maxZ) * 0.5f : (rect.minX + rect.maxX) * 0.5f;

                if (!hasLine)
                {
                    line.Clear();
                    line.Add(rect);
                    currentLine = lineCenter;
                    hasLine = true;
                    continue;
                }

                if (Mathf.Abs(lineCenter - currentLine) <= roadRunLineTolerance)
                {
                    line.Add(rect);
                    currentLine = (currentLine * (line.Count - 1) + lineCenter) / line.Count;
                }
                else
                {
                    FlushMergedLine(horizontal, line, runs);
                    line.Clear();
                    line.Add(rect);
                    currentLine = lineCenter;
                }
            }

            if (line.Count > 0)
            {
                FlushMergedLine(horizontal, line, runs);
            }
        }

        private void FlushMergedLine(bool horizontal, List<Rect2> line, List<RoadRun> runs)
        {
            if (line == null || line.Count == 0 || runs == null)
            {
                return;
            }

            line.Sort((a, b) =>
            {
                float aa = horizontal ? a.minX : a.minZ;
                float ab = horizontal ? b.minX : b.minZ;
                return aa.CompareTo(ab);
            });

            bool hasRun = false;
            float minAlong = 0f;
            float maxAlong = 0f;
            float minAcross = 0f;
            float maxAcross = 0f;

            for (int i = 0; i < line.Count; i++)
            {
                Rect2 rect = line[i];

                float rMinAlong = horizontal ? rect.minX : rect.minZ;
                float rMaxAlong = horizontal ? rect.maxX : rect.maxZ;
                float rMinAcross = horizontal ? rect.minZ : rect.minX;
                float rMaxAcross = horizontal ? rect.maxZ : rect.maxX;

                if (!hasRun)
                {
                    minAlong = rMinAlong;
                    maxAlong = rMaxAlong;
                    minAcross = rMinAcross;
                    maxAcross = rMaxAcross;
                    hasRun = true;
                    continue;
                }

                if (rMinAlong <= maxAlong + roadRunGapTolerance)
                {
                    maxAlong = Mathf.Max(maxAlong, rMaxAlong);
                    minAcross = Mathf.Min(minAcross, rMinAcross);
                    maxAcross = Mathf.Max(maxAcross, rMaxAcross);
                }
                else
                {
                    AddMergedRun(horizontal, minAlong, maxAlong, minAcross, maxAcross, runs);
                    minAlong = rMinAlong;
                    maxAlong = rMaxAlong;
                    minAcross = rMinAcross;
                    maxAcross = rMaxAcross;
                }
            }

            if (hasRun)
            {
                AddMergedRun(horizontal, minAlong, maxAlong, minAcross, maxAcross, runs);
            }
        }

        private void AddMergedRun(bool horizontal, float minAlong, float maxAlong, float minAcross, float maxAcross, List<RoadRun> runs)
        {
            if (maxAlong - minAlong < minMergedRoadRunLength)
            {
                roadRectsSkippedAsIntersections++;
                return;
            }

            runs.Add(new RoadRun
            {
                horizontal = horizontal,
                minAlong = minAlong,
                maxAlong = maxAlong,
                minAcross = minAcross,
                maxAcross = maxAcross
            });
        }

        private void AddHorizontalMergedRun(RoadRun run)
        {
            Rect2 road = new Rect2
            {
                minX = run.minAlong,
                maxX = run.maxAlong,
                minZ = run.minAcross,
                maxZ = run.maxAcross
            };

            AddHorizontalStrip(road, true);
            AddHorizontalStrip(road, false);
        }

        private void AddVerticalMergedRun(RoadRun run)
        {
            Rect2 road = new Rect2
            {
                minX = run.minAcross,
                maxX = run.maxAcross,
                minZ = run.minAlong,
                maxZ = run.maxAlong
            };

            AddVerticalStrip(road, true);
            AddVerticalStrip(road, false);
        }

        private void AddHorizontalStrip(Rect2 road, bool northSide)
        {
            float z = northSide ? road.maxZ : road.minZ;
            Vector3 start = new Vector3(road.minX + stripEndInset, 0f, z);
            Vector3 end = new Vector3(road.maxX - stripEndInset, 0f, z);
            float length = Vector3.Distance(start, end);

            if (length < minStripLength)
            {
                return;
            }

            frontageStrips.Add(new FrontageStrip
            {
                start = start,
                end = end,
                along = Vector3.right,
                awayFromRoad = northSide ? Vector3.forward : Vector3.back,
                length = length,
                side = northSide ? StripSide.NorthOfRoad : StripSide.SouthOfRoad
            });
        }

        private void AddVerticalStrip(Rect2 road, bool eastSide)
        {
            float x = eastSide ? road.maxX : road.minX;
            Vector3 start = new Vector3(x, 0f, road.minZ + stripEndInset);
            Vector3 end = new Vector3(x, 0f, road.maxZ - stripEndInset);
            float length = Vector3.Distance(start, end);

            if (length < minStripLength)
            {
                return;
            }

            frontageStrips.Add(new FrontageStrip
            {
                start = start,
                end = end,
                along = Vector3.forward,
                awayFromRoad = eastSide ? Vector3.right : Vector3.left,
                length = length,
                side = eastSide ? StripSide.EastOfRoad : StripSide.WestOfRoad
            });
        }

        private void ShuffleStrips()
        {
            for (int i = 0; i < frontageStrips.Count; i++)
            {
                int swap = Random.Range(i, frontageStrips.Count);
                FrontageStrip temp = frontageStrips[i];
                frontageStrips[i] = frontageStrips[swap];
                frontageStrips[swap] = temp;
            }
        }

        private void DetectRoadRects()
        {
            roadRects.Clear();

            Renderer[] renderers = scanRoot != null
                ? scanRoot.GetComponentsInChildren<Renderer>(true)
                : FindObjectsByType<Renderer>(FindObjectsInactive.Exclude);

            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null || !renderer.enabled || !IsRoadRenderer(renderer))
                {
                    continue;
                }

                roadRenderers++;

                Bounds b = renderer.bounds;
                Rect2 rect = new Rect2
                {
                    minX = b.min.x,
                    maxX = b.max.x,
                    minZ = b.min.z,
                    maxZ = b.max.z
                };

                if (rect.Width <= 0f || rect.Depth <= 0f)
                {
                    continue;
                }

                roadRects.Add(rect);
            }
        }

        private bool IsRoadRenderer(Renderer renderer)
        {
            Bounds b = renderer.bounds;

            if (b.center.y > maxRoadCenterY ||
                b.size.y > maxRoadRendererHeight ||
                b.size.x < minRoadRendererSize ||
                b.size.z < minRoadRendererSize)
            {
                return false;
            }

            string text = BuildSearchText(renderer);

            for (int i = 0; i < rejectRoadNameContains.Length; i++)
            {
                string reject = rejectRoadNameContains[i];
                if (!string.IsNullOrEmpty(reject) && text.Contains(reject.ToLowerInvariant()))
                {
                    return false;
                }
            }

            for (int i = 0; i < roadNameContains.Length; i++)
            {
                string term = roadNameContains[i];
                if (!string.IsNullOrEmpty(term) && text.Contains(term.ToLowerInvariant()))
                {
                    return true;
                }
            }

            return false;
        }

        private string BuildSearchText(Renderer renderer)
        {
            string text = string.Empty;

            Transform current = renderer.transform;
            int safety = 8;
            while (current != null && safety-- > 0)
            {
                text += " " + current.name.ToLowerInvariant();
                current = current.parent;
            }

            Material material = renderer.sharedMaterial;
            if (material != null)
            {
                text += " " + material.name.ToLowerInvariant();
            }

            return text;
        }

        private void ResolveScanRoot()
        {
            if (scanRoot != null)
            {
                return;
            }

            GameObject root =
                GameObject.Find("Generated_Map_Tiles") ??
                GameObject.Find("Generated_Map_Root") ??
                GameObject.Find("Generated_Map") ??
                GameObject.Find("GeneratedMap");

            if (root != null)
            {
                scanRoot = root.transform;
            }
        }

        private void CalculateMapBounds()
        {
            GameObject rootObject = scanRoot != null ? scanRoot.gameObject : null;
            if (rootObject == null)
            {
                return;
            }

            Bounds bounds;
            if (TryCalculateRendererBounds(rootObject, out bounds))
            {
                mapBounds = bounds;
                hasMapBounds = true;
            }
        }

        private bool RectInsideMapBounds(Rect2 rect)
        {
            if (!hasMapBounds)
            {
                return true;
            }

            return rect.minX >= mapBounds.min.x - mapBoundsPadding &&
                   rect.maxX <= mapBounds.max.x + mapBoundsPadding &&
                   rect.minZ >= mapBounds.min.z - mapBoundsPadding &&
                   rect.maxZ <= mapBounds.max.z + mapBoundsPadding;
        }

        private bool RectOverlapsAnyPlaced(Rect2 rect)
        {
            for (int i = 0; i < placedRects.Count; i++)
            {
                if (RectsOverlap(rect, placedRects[i]))
                {
                    return true;
                }
            }

            return false;
        }

        private bool RectOverlapsAnyRoad(Rect2 rect, float padding)
        {
            for (int i = 0; i < roadRects.Count; i++)
            {
                Rect2 road = roadRects[i];
                road.minX -= padding;
                road.maxX += padding;
                road.minZ -= padding;
                road.maxZ += padding;

                if (RectsOverlap(rect, road))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool RectsOverlap(Rect2 a, Rect2 b)
        {
            return a.minX < b.maxX &&
                   a.maxX > b.minX &&
                   a.minZ < b.maxZ &&
                   a.maxZ > b.minZ;
        }

        private Rect2 BuildWorldAabbFromOrientedFootprint(Vector3 center, Quaternion rotation, float width, float depth)
        {
            Vector3 right = rotation * Vector3.right;
            Vector3 forward = rotation * Vector3.forward;

            Vector3 p0 = center + right * (-width * 0.5f) + forward * (-depth * 0.5f);
            Vector3 p1 = center + right * (width * 0.5f) + forward * (-depth * 0.5f);
            Vector3 p2 = center + right * (-width * 0.5f) + forward * (depth * 0.5f);
            Vector3 p3 = center + right * (width * 0.5f) + forward * (depth * 0.5f);

            return new Rect2
            {
                minX = Mathf.Min(p0.x, p1.x, p2.x, p3.x),
                maxX = Mathf.Max(p0.x, p1.x, p2.x, p3.x),
                minZ = Mathf.Min(p0.z, p1.z, p2.z, p3.z),
                maxZ = Mathf.Max(p0.z, p1.z, p2.z, p3.z)
            };
        }

        private bool TryCalculateRendererBounds(GameObject root, out Bounds bounds)
        {
            bounds = default;

            if (root == null)
            {
                return false;
            }

            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            bool hasBounds = false;

            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null || !renderer.enabled)
                {
                    continue;
                }

                if (!hasBounds)
                {
                    bounds = renderer.bounds;
                    hasBounds = true;
                }
                else
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }

            return hasBounds;
        }

        private Transform GetOrCreateRoot(string rootName)
        {
            GameObject root = GameObject.Find(rootName);
            if (root == null)
            {
                root = new GameObject(rootName);
            }

            return root.transform;
        }

        private static void ClearChildren(Transform root)
        {
            if (root == null)
            {
                return;
            }

            for (int i = root.childCount - 1; i >= 0; i--)
            {
                Transform child = root.GetChild(i);
                if (child != null)
                {
                    Destroy(child.gameObject);
                }
            }
        }

        private void OnDrawGizmos()
        {
            if (!drawDebugGizmos)
            {
                return;
            }

            Gizmos.color = stripGizmoColor;
            for (int i = 0; i < frontageStrips.Count; i++)
            {
                Gizmos.DrawLine(frontageStrips[i].start + Vector3.up * 0.25f, frontageStrips[i].end + Vector3.up * 0.25f);
            }

            if (drawPlacedGizmos)
            {
                Gizmos.color = placedGizmoColor;
                for (int i = 0; i < placedRects.Count; i++)
                {
                    DrawRect(placedRects[i]);
                }
            }

            if (drawRejectedGizmos)
            {
                Gizmos.color = rejectedGizmoColor;
                for (int i = 0; i < rejectedRects.Count; i++)
                {
                    DrawRect(rejectedRects[i]);
                }
            }
        }

        private static void DrawRect(Rect2 rect)
        {
            Vector3 center = rect.Center + Vector3.up * 0.15f;
            Gizmos.DrawWireCube(center, new Vector3(rect.Width, 0.1f, rect.Depth));
        }
    }
}

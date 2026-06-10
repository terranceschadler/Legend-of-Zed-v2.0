using System.Collections;
using System.Collections.Generic;
using LegendOfZed.LegacyMapGenerator;
using UnityEngine;

namespace LegendOfZed.MapIntegration
{
    /// <summary>
    /// v3.6e road-frontage building spawner.
    ///
    /// Uses approved road frontage detection and actual placed renderer bounds:
    /// road cells -> frontage segments -> district-aware varied building placements facing road -> actual renderer footprint reservations.
    /// Does not use CityBlock placement or tile-footprint fallback.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ZedRoadFrontageBuildingSpawner : MonoBehaviour
    {
        [Header("References")]
        public ZedLegacyRandomMapGenerator mapGenerator;
        public GameObject[] buildingPrefabs;

        [Header("Detection")]
        public Transform scanRoot;

        public string[] roadNameContains =
        {
            "road",
            "street",
            "asphalt"
        };

        public string[] rejectNameContains =
        {
            "sidewalk",
            "pavement",
            "curb",
            "kerb",
            "line",
            "marking",
            "crosswalk",
            "spawn",
            "trigger",
            "wall",
            "building",
            "prop",
            "tree",
            "grass",
            "park"
        };

        public float maxRoadCenterY = 2f;
        [Min(0.01f)] public float maxRoadRendererHeight = 1.5f;
        [Min(0.01f)] public float minRoadRendererSize = 0.25f;

        [Header("Grid")]
        [Min(0.25f)] public float roadCellSize = 2.5f;
        [Min(0f)] public float roadPieceInset = 0.02f;

        [Header("Frontage")]
        [Tooltip("Distance from road edge to the building frontage line. This should match the approved debug line offset.")]
        [Min(0f)] public float frontageOffsetFromRoad = 4.5f;

        [Min(0.1f)] public float minFrontageSegmentLength = 6f;
        [Min(1)] public int maxFrontageSegments = 1024;

        [Header("Building Placement")]
        [Tooltip("Extra gap behind the frontage line. Building center also moves back by half the building depth.")]
        [Min(0f)] public float buildingSetback = 0.75f;

        [Tooltip("Keep buildings away from hard segment ends/intersections/corners.")]
        [Min(0f)] public float segmentEndMargin = 2.5f;

        [Tooltip("Extra spacing between buildings along a street.")]
        [Min(0f)] public float buildingGap = 0.45f;

        [Tooltip("Small random spacing variation between buildings.")]
        [Min(0f)] public float buildingGapJitter = 0.25f;

        [Tooltip("Do not place another building when the remaining segment space is smaller than this.")]
        [Min(0f)] public float minRemainingSegmentSpace = 3f;

        [Tooltip("Prefer larger buildings that fit the current frontage slot. This makes streets look lined instead of dotted.")]
        public bool preferLargerBuildings = true;

        [Tooltip("How much of the fitting building list may be randomly selected. Lower values keep buildings larger/more consistent.")]
        [Range(0.1f, 1f)] public float buildingSelectionTopPercent = 0.35f;

        [Tooltip("Generated building footprint padding for overlap checks.")]
        [Min(0f)] public float footprintPadding = 0.35f;

        [Tooltip("After spawning, align the visible renderer bounds center to the intended frontage placement center. Fixes off-center prefab pivots.")]
        public bool alignVisualBoundsToPlacementCenter = true;

        [Tooltip("Use actual renderer bounds after spawning for footprint checks and magenta debug boxes.")]
        public bool useActualRendererBoundsForFootprints = true;


        [Header("Variety + District Polish")]
        [Tooltip("Avoid placing the same prefab repeatedly on a segment.")]
        public bool avoidImmediatePrefabRepeats = true;

        [Tooltip("How many recent prefabs are discouraged per frontage segment.")]
        [Min(0)] public int recentPrefabMemory = 3;

        [Tooltip("Use simple position-based district style buckets so areas feel less uniformly random.")]
        public bool useDistrictStyleBuckets = true;

        [Tooltip("World-space size of district buckets.")]
        [Min(10f)] public float districtCellSize = 80f;

        [Tooltip("Chance to prioritize a larger anchor building at the start of a long frontage segment.")]
        [Range(0f, 1f)] public float anchorBuildingChance = 0.35f;

        [Tooltip("Minimum frontage length before anchor building preference is considered.")]
        [Min(1f)] public float minAnchorSegmentLength = 45f;

        [Tooltip("How strongly district style limits the candidate set. Lower is more distinct, higher is more variety.")]
        [Range(0.1f, 1f)] public float districtCandidatePercent = 0.65f;

        [Tooltip("Optional random seed. Use 0 for normal Unity Random behavior.")]
        public int varietySeed = 0;

        [Tooltip("Maximum total generated buildings.")]
        [Min(1)] public int maxTotalBuildings = 800;

        [Tooltip("Maximum buildings on one frontage segment.")]
        [Min(1)] public int maxBuildingsPerSegment = 96;

        [Tooltip("When enabled, each pass places one building per frontage before extra filling, so every street gets coverage.")]
        public bool useRoundRobinCoverage = true;

        [Header("Alleys")]
        [Tooltip("Guaranteed alley after this many buildings on a segment. 0 disables guaranteed alleys.")]
        [Min(0)] public int guaranteedAlleyEveryNBuildings = 7;

        [Range(0f, 1f)] public float randomAlleyChance = 0.04f;
        [Min(0f)] public float alleyWidthMin = 4f;
        [Min(0f)] public float alleyWidthMax = 7f;

        [Header("Generated Root")]
        public string generatedRootName = "Generated_RoadFrontage_Buildings";

        [Header("Run")]
        public bool runOnStart = true;
        public bool waitForMapGenerator = true;
        public bool clearPreviousGeneratedBuildings = true;
        public bool logDetails = true;
        public bool drawGizmos = true;

        private struct Rect2
        {
            public float minX;
            public float maxX;
            public float minZ;
            public float maxZ;
        }

        private struct FrontageSegment
        {
            public bool horizontal;
            public float fixedCoord;
            public float start;
            public float end;
            public Vector3 towardRoad;

            public float Length
            {
                get { return Mathf.Abs(end - start); }
            }
        }

        private struct SegmentFillState
        {
            public FrontageSegment segment;
            public float cursor;
            public int placedOnSegment;
            public int failedAttempts;
            public bool finished;
            public List<GameObject> recentPrefabs;
        }

        private struct Footprint2D
        {
            public float minX;
            public float maxX;
            public float minZ;
            public float maxZ;
        }

        private readonly HashSet<Vector2Int> roadCells = new HashSet<Vector2Int>();
        private readonly List<Rect2> roadRects = new List<Rect2>();
        private readonly List<FrontageSegment> frontageSegments = new List<FrontageSegment>();
        private readonly List<Footprint2D> generatedFootprints = new List<Footprint2D>();

        private float gridOriginX;
        private float gridOriginZ;
        private int spawnedTotal;

        private void OnValidate()
        {
            roadCellSize = Mathf.Max(0.25f, roadCellSize);
            roadPieceInset = Mathf.Max(0f, roadPieceInset);
            frontageOffsetFromRoad = Mathf.Max(0f, frontageOffsetFromRoad);
            minFrontageSegmentLength = Mathf.Max(0.1f, minFrontageSegmentLength);
            maxFrontageSegments = Mathf.Max(1, maxFrontageSegments);
            buildingSetback = Mathf.Max(0f, buildingSetback);
            segmentEndMargin = Mathf.Max(0f, segmentEndMargin);
            buildingGap = Mathf.Max(0f, buildingGap);
            buildingGapJitter = Mathf.Max(0f, buildingGapJitter);
            minRemainingSegmentSpace = Mathf.Max(0f, minRemainingSegmentSpace);
            buildingSelectionTopPercent = Mathf.Clamp(buildingSelectionTopPercent, 0.1f, 1f);
            recentPrefabMemory = Mathf.Max(0, recentPrefabMemory);
            districtCellSize = Mathf.Max(10f, districtCellSize);
            minAnchorSegmentLength = Mathf.Max(1f, minAnchorSegmentLength);
            districtCandidatePercent = Mathf.Clamp(districtCandidatePercent, 0.1f, 1f);
            maxTotalBuildings = Mathf.Max(1, maxTotalBuildings);
            maxBuildingsPerSegment = Mathf.Max(1, maxBuildingsPerSegment);
            guaranteedAlleyEveryNBuildings = Mathf.Max(0, guaranteedAlleyEveryNBuildings);
            alleyWidthMin = Mathf.Max(0f, alleyWidthMin);
            alleyWidthMax = Mathf.Max(alleyWidthMin, alleyWidthMax);
        }

        private IEnumerator Start()
        {
            if (!runOnStart)
            {
                yield break;
            }

            if (mapGenerator == null)
            {
                mapGenerator = FindAnyObjectByType<ZedLegacyRandomMapGenerator>();
            }

            if (waitForMapGenerator && mapGenerator != null)
            {
                while (!mapGenerator.bakingNavMeshCompleted)
                {
                    yield return null;
                }

                yield return null;
            }
            else
            {
                yield return null;
            }

            BuildNow();
        }

        [ContextMenu("Build Road Frontage Buildings Now")]
        public void BuildNow()
        {
            OnValidate();

            if (varietySeed != 0)
            {
                Random.InitState(varietySeed);
            }

            spawnedTotal = 0;
            roadCells.Clear();
            roadRects.Clear();
            frontageSegments.Clear();
            generatedFootprints.Clear();

            Transform generatedRoot = GetOrCreateGeneratedRoot();
            if (clearPreviousGeneratedBuildings)
            {
                ClearChildren(generatedRoot);
            }

            DetectRoadRects();
            RasterizeRoadRects();

            List<FrontageSegment> raw = BuildRoadBoundaryFrontageSegments();
            List<FrontageSegment> merged = MergeCollinearSegments(raw);
            FilterFrontageSegments(merged);

            int spawned = useRoundRobinCoverage
                ? SpawnRoundRobin(generatedRoot)
                : SpawnSequential(generatedRoot);

            if (logDetails)
            {
                Debug.Log(
                    "Road frontage building spawner complete. RoadRects=" + roadRects.Count +
                    ", RoadCells=" + roadCells.Count +
                    ", FrontageSegments=" + frontageSegments.Count +
                    ", Buildings=" + spawned +
                    ", RoundRobin=" + useRoundRobinCoverage +
                    ", Setback=" + buildingSetback.ToString("0.##") +
                    ", EndMargin=" + segmentEndMargin.ToString("0.##") +
                    ", Districts=" + useDistrictStyleBuckets +
                    ", AntiRepeat=" + avoidImmediatePrefabRepeats + ".",
                    this);
            }
        }

        private void DetectRoadRects()
        {
            Renderer[] renderers;

            if (scanRoot != null)
            {
                renderers = scanRoot.GetComponentsInChildren<Renderer>(true);
            }
            else
            {
                renderers = FindObjectsByType<Renderer>(FindObjectsInactive.Exclude);
            }

            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null || !renderer.enabled)
                {
                    continue;
                }

                if (!IsRoadRenderer(renderer))
                {
                    continue;
                }

                Bounds b = FlattenBounds(renderer.bounds);

                Rect2 rect = new Rect2
                {
                    minX = b.min.x + roadPieceInset,
                    maxX = b.max.x - roadPieceInset,
                    minZ = b.min.z + roadPieceInset,
                    maxZ = b.max.z - roadPieceInset
                };

                if (rect.maxX <= rect.minX || rect.maxZ <= rect.minZ)
                {
                    continue;
                }

                roadRects.Add(rect);
            }
        }

        private bool IsRoadRenderer(Renderer renderer)
        {
            Bounds b = FlattenBounds(renderer.bounds);

            if (b.center.y > maxRoadCenterY)
            {
                return false;
            }

            if (b.size.y > maxRoadRendererHeight)
            {
                return false;
            }

            if (b.size.x < minRoadRendererSize || b.size.z < minRoadRendererSize)
            {
                return false;
            }

            string text = BuildSearchText(renderer);

            for (int i = 0; i < rejectNameContains.Length; i++)
            {
                string reject = rejectNameContains[i];
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
            int safety = 6;
            while (current != null && safety-- > 0)
            {
                text += " " + current.name.ToLowerInvariant();
                current = current.parent;
            }

            Material mat = renderer.sharedMaterial;
            if (mat != null)
            {
                text += " " + mat.name.ToLowerInvariant();
            }

            return text;
        }

        private void RasterizeRoadRects()
        {
            roadCells.Clear();

            if (roadRects.Count == 0)
            {
                return;
            }

            float minX = roadRects[0].minX;
            float minZ = roadRects[0].minZ;

            for (int i = 1; i < roadRects.Count; i++)
            {
                minX = Mathf.Min(minX, roadRects[i].minX);
                minZ = Mathf.Min(minZ, roadRects[i].minZ);
            }

            gridOriginX = Mathf.Floor(minX / roadCellSize) * roadCellSize;
            gridOriginZ = Mathf.Floor(minZ / roadCellSize) * roadCellSize;

            for (int i = 0; i < roadRects.Count; i++)
            {
                Rect2 r = roadRects[i];

                int minCellX = Mathf.FloorToInt((r.minX - gridOriginX) / roadCellSize);
                int maxCellX = Mathf.FloorToInt((r.maxX - gridOriginX) / roadCellSize);
                int minCellZ = Mathf.FloorToInt((r.minZ - gridOriginZ) / roadCellSize);
                int maxCellZ = Mathf.FloorToInt((r.maxZ - gridOriginZ) / roadCellSize);

                for (int x = minCellX; x <= maxCellX; x++)
                {
                    for (int z = minCellZ; z <= maxCellZ; z++)
                    {
                        float centerX = gridOriginX + (x + 0.5f) * roadCellSize;
                        float centerZ = gridOriginZ + (z + 0.5f) * roadCellSize;

                        if (centerX >= r.minX && centerX <= r.maxX && centerZ >= r.minZ && centerZ <= r.maxZ)
                        {
                            roadCells.Add(new Vector2Int(x, z));
                        }
                    }
                }
            }
        }

        private List<FrontageSegment> BuildRoadBoundaryFrontageSegments()
        {
            List<FrontageSegment> edges = new List<FrontageSegment>();

            foreach (Vector2Int cell in roadCells)
            {
                float minX = gridOriginX + cell.x * roadCellSize;
                float maxX = minX + roadCellSize;
                float minZ = gridOriginZ + cell.y * roadCellSize;
                float maxZ = minZ + roadCellSize;

                if (!roadCells.Contains(new Vector2Int(cell.x, cell.y + 1)))
                {
                    edges.Add(new FrontageSegment
                    {
                        horizontal = true,
                        fixedCoord = maxZ + frontageOffsetFromRoad,
                        start = minX,
                        end = maxX,
                        towardRoad = Vector3.back
                    });
                }

                if (!roadCells.Contains(new Vector2Int(cell.x, cell.y - 1)))
                {
                    edges.Add(new FrontageSegment
                    {
                        horizontal = true,
                        fixedCoord = minZ - frontageOffsetFromRoad,
                        start = minX,
                        end = maxX,
                        towardRoad = Vector3.forward
                    });
                }

                if (!roadCells.Contains(new Vector2Int(cell.x + 1, cell.y)))
                {
                    edges.Add(new FrontageSegment
                    {
                        horizontal = false,
                        fixedCoord = maxX + frontageOffsetFromRoad,
                        start = minZ,
                        end = maxZ,
                        towardRoad = Vector3.left
                    });
                }

                if (!roadCells.Contains(new Vector2Int(cell.x - 1, cell.y)))
                {
                    edges.Add(new FrontageSegment
                    {
                        horizontal = false,
                        fixedCoord = minX - frontageOffsetFromRoad,
                        start = minZ,
                        end = maxZ,
                        towardRoad = Vector3.right
                    });
                }
            }

            return edges;
        }

        private List<FrontageSegment> MergeCollinearSegments(List<FrontageSegment> segments)
        {
            segments.Sort((a, b) =>
            {
                int axisCompare = a.horizontal.CompareTo(b.horizontal);
                if (axisCompare != 0) return axisCompare;

                int fixedCompare = a.fixedCoord.CompareTo(b.fixedCoord);
                if (fixedCompare != 0) return fixedCompare;

                int directionCompare = a.towardRoad.x.CompareTo(b.towardRoad.x);
                if (directionCompare != 0) return directionCompare;

                directionCompare = a.towardRoad.z.CompareTo(b.towardRoad.z);
                if (directionCompare != 0) return directionCompare;

                return a.start.CompareTo(b.start);
            });

            List<FrontageSegment> merged = new List<FrontageSegment>();

            for (int i = 0; i < segments.Count; i++)
            {
                FrontageSegment s = segments[i];

                if (merged.Count == 0)
                {
                    merged.Add(s);
                    continue;
                }

                FrontageSegment last = merged[merged.Count - 1];

                bool sameLine = last.horizontal == s.horizontal &&
                                Mathf.Abs(last.fixedCoord - s.fixedCoord) <= 0.01f &&
                                Vector3.Dot(last.towardRoad.normalized, s.towardRoad.normalized) > 0.99f &&
                                Mathf.Abs(last.end - s.start) <= 0.01f;

                if (sameLine)
                {
                    last.end = Mathf.Max(last.end, s.end);
                    merged[merged.Count - 1] = last;
                }
                else
                {
                    merged.Add(s);
                }
            }

            return merged;
        }

        private void FilterFrontageSegments(List<FrontageSegment> source)
        {
            source.Sort((a, b) => b.Length.CompareTo(a.Length));

            for (int i = 0; i < source.Count; i++)
            {
                FrontageSegment s = source[i];

                if (s.Length < minFrontageSegmentLength)
                {
                    continue;
                }

                frontageSegments.Add(s);

                if (frontageSegments.Count >= maxFrontageSegments)
                {
                    break;
                }
            }
        }

        private int SpawnSequential(Transform root)
        {
            int spawned = 0;

            for (int i = 0; i < frontageSegments.Count; i++)
            {
                SegmentFillState state = CreateFillState(frontageSegments[i]);

                while (!state.finished && spawnedTotal < maxTotalBuildings)
                {
                    if (TrySpawnOne(ref state, root))
                    {
                        spawned++;
                    }
                    else if (state.finished)
                    {
                        break;
                    }
                }

                if (spawnedTotal >= maxTotalBuildings)
                {
                    break;
                }
            }

            return spawned;
        }

        private int SpawnRoundRobin(Transform root)
        {
            List<SegmentFillState> states = new List<SegmentFillState>();
            for (int i = 0; i < frontageSegments.Count; i++)
            {
                states.Add(CreateFillState(frontageSegments[i]));
            }

            int spawned = 0;
            bool anyActive = true;
            int safetyPasses = Mathf.Max(2, maxBuildingsPerSegment + 8);

            while (anyActive && spawnedTotal < maxTotalBuildings && safetyPasses-- > 0)
            {
                anyActive = false;

                for (int i = 0; i < states.Count; i++)
                {
                    SegmentFillState state = states[i];

                    if (state.finished)
                    {
                        continue;
                    }

                    anyActive = true;

                    if (TrySpawnOne(ref state, root))
                    {
                        spawned++;
                    }

                    states[i] = state;

                    if (spawnedTotal >= maxTotalBuildings)
                    {
                        break;
                    }
                }
            }

            return spawned;
        }

        private SegmentFillState CreateFillState(FrontageSegment segment)
        {
            float halfLength = segment.Length * 0.5f;
            float startCursor = -halfLength + segmentEndMargin;
            bool tooShort = segment.Length < minFrontageSegmentLength || segment.Length <= segmentEndMargin * 2f + minRemainingSegmentSpace;

            return new SegmentFillState
            {
                segment = segment,
                cursor = startCursor,
                placedOnSegment = 0,
                failedAttempts = 0,
                finished = tooShort,
                recentPrefabs = new List<GameObject>()
            };
        }

        private bool TrySpawnOne(ref SegmentFillState state, Transform root)
        {
            if (state.finished || spawnedTotal >= maxTotalBuildings || state.placedOnSegment >= maxBuildingsPerSegment)
            {
                state.finished = true;
                return false;
            }

            FrontageSegment segment = state.segment;
            float length = segment.Length;

            float usableEnd = length * 0.5f - segmentEndMargin;
            if (state.cursor >= usableEnd)
            {
                state.finished = true;
                return false;
            }

            float remaining = usableEnd - state.cursor;
            if (remaining < minRemainingSegmentSpace)
            {
                state.finished = true;
                return false;
            }

            GameObject prefab = PickBuildingThatFits(remaining, state.segment, state.placedOnSegment, state.recentPrefabs);

            if (prefab == null)
            {
                state.finished = true;
                return false;
            }

            Vector2 estimatedFootprint = ZedBuildingFootprint.EstimateFootprintSize(prefab);
            float buildingWidth = Mathf.Max(0.1f, estimatedFootprint.x);
            float buildingDepth = Mathf.Max(0.1f, estimatedFootprint.y);

            Vector3 axis = segment.horizontal ? Vector3.right : Vector3.forward;
            Vector3 towardRoad = segment.towardRoad.normalized;
            Vector3 awayFromRoad = -towardRoad;

            Vector3 frontageCenter = segment.horizontal
                ? new Vector3((segment.start + segment.end) * 0.5f, 0f, segment.fixedCoord)
                : new Vector3(segment.fixedCoord, 0f, (segment.start + segment.end) * 0.5f);

            float alongOffset = state.cursor + buildingWidth * 0.5f;
            Vector3 desiredVisualCenter = frontageCenter + axis.normalized * alongOffset;
            desiredVisualCenter += awayFromRoad * (buildingDepth * 0.5f + buildingSetback);

            Quaternion rotation = Quaternion.LookRotation(towardRoad, Vector3.up);
            GameObject instance = Instantiate(prefab, desiredVisualCenter, rotation, root);
            instance.name = prefab.name + "_RoadFrontageBuilding";

            Bounds visualBounds;
            if (TryCalculateRendererBounds(instance, out visualBounds))
            {
                if (alignVisualBoundsToPlacementCenter)
                {
                    Vector3 delta = desiredVisualCenter - visualBounds.center;
                    delta.y = 0f;
                    instance.transform.position += delta;

                    TryCalculateRendererBounds(instance, out visualBounds);
                }
            }
            else
            {
                visualBounds = new Bounds(desiredVisualCenter, new Vector3(buildingWidth, 1f, buildingDepth));
            }

            Footprint2D realFootprint = useActualRendererBoundsForFootprints
                ? BuildFootprintFromBounds(visualBounds, footprintPadding)
                : BuildFootprint(desiredVisualCenter, segment.horizontal, buildingWidth, buildingDepth, footprintPadding);

            if (FootprintOverlapsRoad(realFootprint) || FootprintOverlapsGenerated(realFootprint))
            {
                Destroy(instance);

                state.cursor += Mathf.Max(1f, buildingWidth * 0.5f);
                state.failedAttempts++;

                if (state.failedAttempts > maxBuildingsPerSegment * 3)
                {
                    state.finished = true;
                }

                return false;
            }

            generatedFootprints.Add(realFootprint);
            RememberPrefab(state.recentPrefabs, prefab);

            spawnedTotal++;
            state.placedOnSegment++;
            state.failedAttempts = 0;

            float spacing = buildingGap;
            if (buildingGapJitter > 0f)
            {
                spacing += Random.Range(-buildingGapJitter, buildingGapJitter);
                spacing = Mathf.Max(0f, spacing);
            }

            state.cursor += buildingWidth + spacing;

            bool guaranteedAlley = guaranteedAlleyEveryNBuildings > 0 &&
                                   state.placedOnSegment > 0 &&
                                   state.placedOnSegment % guaranteedAlleyEveryNBuildings == 0;

            bool randomAlley = state.placedOnSegment >= 2 && Random.value < randomAlleyChance;

            if (guaranteedAlley || randomAlley)
            {
                state.cursor += Random.Range(Mathf.Min(alleyWidthMin, alleyWidthMax), Mathf.Max(alleyWidthMin, alleyWidthMax));
            }

            if (state.cursor >= usableEnd || state.placedOnSegment >= maxBuildingsPerSegment)
            {
                state.finished = true;
            }

            return true;
        }

        private bool TryCalculateRendererBounds(GameObject instance, out Bounds bounds)
        {
            bounds = new Bounds(instance != null ? instance.transform.position : Vector3.zero, Vector3.zero);

            if (instance == null)
            {
                return false;
            }

            Renderer[] renderers = instance.GetComponentsInChildren<Renderer>(true);
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

        private Footprint2D BuildFootprintFromBounds(Bounds bounds, float padding)
        {
            return new Footprint2D
            {
                minX = bounds.min.x - padding,
                maxX = bounds.max.x + padding,
                minZ = bounds.min.z - padding,
                maxZ = bounds.max.z + padding
            };
        }

        private GameObject PickBuildingThatFits(float remainingWidth, FrontageSegment segment, int placedOnSegment, List<GameObject> recentPrefabs)
        {
            if (buildingPrefabs == null || buildingPrefabs.Length == 0)
            {
                return null;
            }

            List<GameObject> candidates = new List<GameObject>();

            for (int i = 0; i < buildingPrefabs.Length; i++)
            {
                GameObject prefab = buildingPrefabs[i];
                if (prefab == null)
                {
                    continue;
                }

                Vector2 size = ZedBuildingFootprint.EstimateFootprintSize(prefab);
                if (size.x <= remainingWidth)
                {
                    candidates.Add(prefab);
                }
            }

            if (candidates.Count == 0)
            {
                return null;
            }

            if (useDistrictStyleBuckets && candidates.Count > 2)
            {
                int bucket = GetDistrictBucket(segment);
                candidates.Sort((a, b) =>
                {
                    int ah = Mathf.Abs(GetStablePrefabHash(a) - bucket);
                    int bh = Mathf.Abs(GetStablePrefabHash(b) - bucket);
                    return ah.CompareTo(bh);
                });

                int districtCount = Mathf.Clamp(Mathf.CeilToInt(candidates.Count * districtCandidatePercent), 1, candidates.Count);
                candidates.RemoveRange(districtCount, candidates.Count - districtCount);
            }

            bool preferAnchor = placedOnSegment == 0 &&
                                segment.Length >= minAnchorSegmentLength &&
                                Random.value < anchorBuildingChance;

            candidates.Sort((a, b) =>
            {
                float aw = ZedBuildingFootprint.EstimateFootprintSize(a).x;
                float bw = ZedBuildingFootprint.EstimateFootprintSize(b).x;

                if (preferAnchor || preferLargerBuildings)
                {
                    return bw.CompareTo(aw);
                }

                return aw.CompareTo(bw);
            });

            if (avoidImmediatePrefabRepeats && recentPrefabs != null && recentPrefabs.Count > 0 && candidates.Count > 1)
            {
                List<GameObject> nonRecent = new List<GameObject>();

                for (int i = 0; i < candidates.Count; i++)
                {
                    if (!recentPrefabs.Contains(candidates[i]))
                    {
                        nonRecent.Add(candidates[i]);
                    }
                }

                if (nonRecent.Count > 0)
                {
                    candidates = nonRecent;
                }
            }

            int topCount = preferAnchor
                ? Mathf.Clamp(Mathf.CeilToInt(candidates.Count * 0.2f), 1, candidates.Count)
                : Mathf.Clamp(Mathf.CeilToInt(candidates.Count * buildingSelectionTopPercent), 1, candidates.Count);

            return candidates[Random.Range(0, topCount)];
        }

        private int GetDistrictBucket(FrontageSegment segment)
        {
            Vector3 center = segment.horizontal
                ? new Vector3((segment.start + segment.end) * 0.5f, 0f, segment.fixedCoord)
                : new Vector3(segment.fixedCoord, 0f, (segment.start + segment.end) * 0.5f);

            int x = Mathf.FloorToInt(center.x / districtCellSize);
            int z = Mathf.FloorToInt(center.z / districtCellSize);
            return Mathf.Abs((x * 73856093) ^ (z * 19349663));
        }

        private int GetStablePrefabHash(GameObject prefab)
        {
            if (prefab == null)
            {
                return 0;
            }

            unchecked
            {
                int hash = 17;
                string name = prefab.name;
                for (int i = 0; i < name.Length; i++)
                {
                    hash = hash * 31 + name[i];
                }

                return Mathf.Abs(hash);
            }
        }

        private void RememberPrefab(List<GameObject> recentPrefabs, GameObject prefab)
        {
            if (recentPrefabs == null || prefab == null || recentPrefabMemory <= 0)
            {
                return;
            }

            recentPrefabs.Add(prefab);
            while (recentPrefabs.Count > recentPrefabMemory)
            {
                recentPrefabs.RemoveAt(0);
            }
        }

        private Footprint2D BuildFootprint(Vector3 center, bool segmentHorizontal, float width, float depth, float padding)
        {
            float halfX;
            float halfZ;

            if (segmentHorizontal)
            {
                halfX = width * 0.5f + padding;
                halfZ = depth * 0.5f + padding;
            }
            else
            {
                halfX = depth * 0.5f + padding;
                halfZ = width * 0.5f + padding;
            }

            return new Footprint2D
            {
                minX = center.x - halfX,
                maxX = center.x + halfX,
                minZ = center.z - halfZ,
                maxZ = center.z + halfZ
            };
        }

        private bool FootprintOverlapsGenerated(Footprint2D candidate)
        {
            for (int i = 0; i < generatedFootprints.Count; i++)
            {
                if (FootprintsOverlap(candidate, generatedFootprints[i]))
                {
                    return true;
                }
            }

            return false;
        }

        private bool FootprintsOverlap(Footprint2D a, Footprint2D b)
        {
            return a.minX < b.maxX &&
                   a.maxX > b.minX &&
                   a.minZ < b.maxZ &&
                   a.maxZ > b.minZ;
        }

        private bool FootprintOverlapsRoad(Footprint2D footprint)
        {
            int minX = Mathf.FloorToInt((footprint.minX - gridOriginX) / roadCellSize);
            int maxX = Mathf.FloorToInt((footprint.maxX - gridOriginX) / roadCellSize);
            int minZ = Mathf.FloorToInt((footprint.minZ - gridOriginZ) / roadCellSize);
            int maxZ = Mathf.FloorToInt((footprint.maxZ - gridOriginZ) / roadCellSize);

            for (int x = minX; x <= maxX; x++)
            {
                for (int z = minZ; z <= maxZ; z++)
                {
                    if (roadCells.Contains(new Vector2Int(x, z)))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private Transform GetOrCreateGeneratedRoot()
        {
            GameObject root = GameObject.Find(generatedRootName);
            if (root == null)
            {
                root = new GameObject(generatedRootName);
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

        private static Bounds FlattenBounds(Bounds b)
        {
            b.size = new Vector3(b.size.x, Mathf.Max(0.05f, b.size.y), b.size.z);
            return b;
        }

        private void OnDrawGizmos()
        {
            if (!drawGizmos)
            {
                return;
            }

            Gizmos.color = Color.green;
            for (int i = 0; i < frontageSegments.Count; i++)
            {
                FrontageSegment s = frontageSegments[i];
                Vector3 a = s.horizontal
                    ? new Vector3(s.start, 0.25f, s.fixedCoord)
                    : new Vector3(s.fixedCoord, 0.25f, s.start);

                Vector3 b = s.horizontal
                    ? new Vector3(s.end, 0.25f, s.fixedCoord)
                    : new Vector3(s.fixedCoord, 0.25f, s.end);

                Gizmos.DrawLine(a, b);
            }

            Gizmos.color = Color.magenta;
            for (int i = 0; i < generatedFootprints.Count; i++)
            {
                Footprint2D f = generatedFootprints[i];
                Vector3 center = new Vector3((f.minX + f.maxX) * 0.5f, 0.35f, (f.minZ + f.maxZ) * 0.5f);
                Vector3 size = new Vector3(f.maxX - f.minX, 0.1f, f.maxZ - f.minZ);
                Gizmos.DrawWireCube(center, size);
            }
        }
    }
}

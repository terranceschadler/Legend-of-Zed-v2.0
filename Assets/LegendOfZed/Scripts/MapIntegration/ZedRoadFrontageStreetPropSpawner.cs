using System.Collections;
using System.Collections.Generic;
using LegendOfZed.LegacyMapGenerator;
using UnityEngine;

namespace LegendOfZed.MapIntegration
{
    /// <summary>
    /// v3.7c4 heavier skip trash clusters.
    ///
    /// Adds sidewalk / curb props, building-side dumpster clusters, and dense paper debris using the approved road-frontage source of truth.
    /// Does not modify building placement.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ZedRoadFrontageStreetPropSpawner : MonoBehaviour
    {
        [Header("References")]
        public ZedLegacyRandomMapGenerator mapGenerator;
        public Transform scanRoot;

        [Header("Prop Prefabs")]
        public GameObject[] streetLightPrefabs;
        public GameObject[] benchPrefabs;
        public GameObject[] trashCanPrefabs;
        public GameObject[] hydrantPrefabs;
        public GameObject[] mailboxPrefabs;
        public GameObject[] paperDebrisPrefabs;
        public GameObject[] dumpsterPrefabs;
        public GameObject[] trashPilePrefabs;

        [Header("Road Detection")]
        public string[] roadNameContains =
        {
            "road",
            "street",
            "asphalt"
        };

        public string[] rejectRoadNameContains =
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

        [Header("Placement")]
        [Tooltip("Distance from road edge outward where curb/sidewalk props are placed.")]
        [Min(0f)] public float curbPropOffsetFromRoad = 2.25f;

        [Tooltip("Farther sidewalk/backline offset for benches/trash/mailboxes.")]
        [Min(0f)] public float sidewalkPropOffsetFromRoad = 5.25f;

        [Tooltip("Keep props away from hard road segment ends and intersections.")]
        [Min(0f)] public float segmentEndMargin = 4f;

        [Tooltip("No prop may reserve space closer than this to another prop.")]
        [Min(0f)] public float propFootprintPadding = 0.4f;

        [Tooltip("Generated props stay this far away from generated building visual bounds.")]
        [Min(0f)] public float buildingClearance = 0.75f;

        [Header("Spacing")]
        [Min(1f)] public float streetLightSpacing = 18f;
        [Min(1f)] public float benchSpacing = 32f;
        [Min(1f)] public float trashCanSpacing = 22f;
        [Min(1f)] public float hydrantSpacing = 38f;
        [Min(1f)] public float mailboxSpacing = 48f;
        [Min(1f)] public float paperDebrisSpacing = 3.5f;

        [Header("Chance")]
        [Range(0f, 1f)] public float streetLightChance = 0.8f;
        [Range(0f, 1f)] public float benchChance = 0.35f;
        [Range(0f, 1f)] public float trashCanChance = 0.45f;
        [Range(0f, 1f)] public float hydrantChance = 0.28f;
        [Range(0f, 1f)] public float mailboxChance = 0.18f;
        [Range(0f, 1f)] public float paperDebrisChance = 0.92f;

        [Header("Dumpster / Trash Piles")]
        [Tooltip("Chance per building blocker to try placing a side/back dumpster.")]
        [Range(0f, 1f)] public float dumpsterChancePerBuilding = 0.45f;

        [Tooltip("Keep trying until at least this many dumpsters/skips are spawned when prefabs are assigned.")]
        [Min(0)] public int minimumDumpsterCount = 10;

        [Tooltip("Maximum total dumpster placement attempts before giving up.")]
        [Min(1)] public int maxDumpsterAttempts = 400;

        [Tooltip("Minimum spacing between generated dumpsters.")]
        [Min(2f)] public float dumpsterSpacing = 18f;

        [Tooltip("Dumpster footprint width used before real renderer bounds are known.")]
        [Min(0.5f)] public float dumpsterFootprintWidth = 3.2f;

        [Tooltip("Dumpster footprint depth used before real renderer bounds are known.")]
        [Min(0.5f)] public float dumpsterFootprintDepth = 2.2f;

        [Tooltip("Trash piles spawned around each accepted dumpster.")]
        [Min(0)] public int trashPilesPerDumpster = 10;

        [Tooltip("Trash pile scatter radius around accepted dumpsters.")]
        [Min(0.25f)] public float trashPileScatterRadius = 3.6f;

        [Tooltip("Extra paper/debris pieces spawned around each accepted skip/dumpster.")]
        [Min(0)] public int paperDebrisPerDumpster = 18;

        [Tooltip("Extra outer debris pieces spawned around each accepted skip/dumpster.")]
        [Min(0)] public int outerDebrisPerDumpster = 8;

        [Tooltip("Outer debris scatter radius around accepted dumpsters.")]
        [Min(0.25f)] public float outerDebrisScatterRadius = 5.25f;

        [Tooltip("How far dumpsters sit away from building bounds.")]
        [Min(0f)] public float dumpsterBuildingOffset = 0.35f;

        [Header("Rotation")]
        public bool benchesFaceRoad = true;
        public bool mailboxesFaceRoad = true;
        public bool trashCansRandomYaw = true;
        public bool paperDebrisRandomYaw = true;

        [Header("Generated Root")]
        public string generatedRootName = "Generated_RoadFrontage_StreetProps";
        public string buildingRootName = "Generated_RoadFrontage_Buildings";

        [Header("Building Wait")]
        [Tooltip("Wait for generated road-frontage buildings before placing side/back dumpsters.")]
        public bool waitForGeneratedBuildings = true;

        [Tooltip("Maximum frames to wait for generated buildings before running anyway.")]
        [Min(0)] public int maxGeneratedBuildingWaitFrames = 180;

        [Header("Run")]
        public bool runOnStart = true;
        public bool waitForMapGenerator = true;
        public bool clearPreviousGeneratedProps = true;
        public bool drawGizmos = true;
        public bool logDetails = true;
        public int randomSeed = 0;

        private struct Rect2
        {
            public float minX;
            public float maxX;
            public float minZ;
            public float maxZ;
        }

        private struct BuildingBlocker
        {
            public Rect2 rect;
            public Vector3 center;
            public string name;
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

        private readonly HashSet<Vector2Int> roadCells = new HashSet<Vector2Int>();
        private readonly List<Rect2> roadRects = new List<Rect2>();
        private readonly List<FrontageSegment> frontageSegments = new List<FrontageSegment>();
        private readonly List<BuildingBlocker> buildingBlockers = new List<BuildingBlocker>();
        private readonly List<Rect2> propRects = new List<Rect2>();

        private float gridOriginX;
        private float gridOriginZ;
        private int spawnedProps;
        private int dumpsterAttempts;
        private int spawnedDumpsters;
        private int spawnedTrashPiles;
        private int dumpsterRoadRejects;
        private int dumpsterBuildingRejects;
        private int dumpsterPropRejects;
        private int spawnedSkipAreaPaperDebris;

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

            if (waitForGeneratedBuildings)
            {
                int waitFrames = 0;
                while (!GeneratedBuildingRootReady() && waitFrames < maxGeneratedBuildingWaitFrames)
                {
                    waitFrames++;
                    yield return null;
                }
            }

            BuildNow();
        }

        private bool GeneratedBuildingRootReady()
        {
            GameObject buildingRoot = GameObject.Find(buildingRootName);
            return buildingRoot != null && buildingRoot.transform.childCount > 0;
        }

        private void OnValidate()
        {
            roadCellSize = Mathf.Max(0.25f, roadCellSize);
            roadPieceInset = Mathf.Max(0f, roadPieceInset);
            curbPropOffsetFromRoad = Mathf.Max(0f, curbPropOffsetFromRoad);
            sidewalkPropOffsetFromRoad = Mathf.Max(curbPropOffsetFromRoad, sidewalkPropOffsetFromRoad);
            segmentEndMargin = Mathf.Max(0f, segmentEndMargin);
            propFootprintPadding = Mathf.Max(0f, propFootprintPadding);
            buildingClearance = Mathf.Max(0f, buildingClearance);

            streetLightSpacing = Mathf.Max(1f, streetLightSpacing);
            benchSpacing = Mathf.Max(1f, benchSpacing);
            trashCanSpacing = Mathf.Max(1f, trashCanSpacing);
            hydrantSpacing = Mathf.Max(1f, hydrantSpacing);
            mailboxSpacing = Mathf.Max(1f, mailboxSpacing);
            paperDebrisSpacing = Mathf.Max(1f, paperDebrisSpacing);
            minimumDumpsterCount = Mathf.Max(0, minimumDumpsterCount);
            maxDumpsterAttempts = Mathf.Max(1, maxDumpsterAttempts);
            maxGeneratedBuildingWaitFrames = Mathf.Max(0, maxGeneratedBuildingWaitFrames);
            dumpsterSpacing = Mathf.Max(2f, dumpsterSpacing);
            dumpsterFootprintWidth = Mathf.Max(0.5f, dumpsterFootprintWidth);
            dumpsterFootprintDepth = Mathf.Max(0.5f, dumpsterFootprintDepth);
            trashPilesPerDumpster = Mathf.Max(0, trashPilesPerDumpster);
            trashPileScatterRadius = Mathf.Max(0.25f, trashPileScatterRadius);
            paperDebrisPerDumpster = Mathf.Max(0, paperDebrisPerDumpster);
            outerDebrisPerDumpster = Mathf.Max(0, outerDebrisPerDumpster);
            outerDebrisScatterRadius = Mathf.Max(0.25f, outerDebrisScatterRadius);
            dumpsterBuildingOffset = Mathf.Max(0f, dumpsterBuildingOffset);
        }

        [ContextMenu("Build Road Frontage Street Props Now")]
        public void BuildNow()
        {
            OnValidate();

            if (randomSeed != 0)
            {
                Random.InitState(randomSeed);
            }

            spawnedProps = 0;
            dumpsterAttempts = 0;
            spawnedDumpsters = 0;
            spawnedTrashPiles = 0;
            dumpsterRoadRejects = 0;
            dumpsterBuildingRejects = 0;
            dumpsterPropRejects = 0;
            spawnedSkipAreaPaperDebris = 0;
            roadCells.Clear();
            roadRects.Clear();
            frontageSegments.Clear();
            buildingBlockers.Clear();
            propRects.Clear();

            Transform root = GetOrCreateGeneratedRoot();
            if (clearPreviousGeneratedProps)
            {
                ClearChildren(root);
            }

            DetectRoadRects();
            RasterizeRoadRects();

            List<FrontageSegment> raw = BuildRoadBoundaryFrontageSegments();
            List<FrontageSegment> merged = MergeCollinearSegments(raw);
            FilterFrontageSegments(merged);

            CollectBuildingBlockers();

            // Skips/dumpsters need building anchors and should claim their spots before small street props.
            SpawnDumpsterClusters(root);

            for (int i = 0; i < frontageSegments.Count; i++)
            {
                SpawnPropsOnSegment(frontageSegments[i], root);
            }

            if (logDetails)
            {
                Debug.Log(
                    "Road frontage street prop pass complete. RoadRects=" + roadRects.Count +
                    ", RoadCells=" + roadCells.Count +
                    ", FrontageSegments=" + frontageSegments.Count +
                    ", BuildingBlockers=" + buildingBlockers.Count +
                    ", BuildingRootReady=" + GeneratedBuildingRootReady() +
                    ", Props=" + spawnedProps +
                    ", DumpsterPrefabs=" + CountPrefabs(dumpsterPrefabs) +
                    ", DumpsterAttempts=" + dumpsterAttempts +
                    ", Dumpsters=" + spawnedDumpsters +
                    ", TrashPiles=" + spawnedTrashPiles +
                    ", SkipAreaPaperDebris=" + spawnedSkipAreaPaperDebris +
                    ", DumpsterRoadRejects=" + dumpsterRoadRejects +
                    ", DumpsterBuildingRejects=" + dumpsterBuildingRejects +
                    ", DumpsterPropRejects=" + dumpsterPropRejects + ".",
                    this);
            }
        }

        private void DetectRoadRects()
        {
            Renderer[] renderers = scanRoot != null
                ? scanRoot.GetComponentsInChildren<Renderer>(true)
                : FindObjectsByType<Renderer>(FindObjectsInactive.Exclude);

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
                    edges.Add(new FrontageSegment { horizontal = true, fixedCoord = maxZ, start = minX, end = maxX, towardRoad = Vector3.back });
                }

                if (!roadCells.Contains(new Vector2Int(cell.x, cell.y - 1)))
                {
                    edges.Add(new FrontageSegment { horizontal = true, fixedCoord = minZ, start = minX, end = maxX, towardRoad = Vector3.forward });
                }

                if (!roadCells.Contains(new Vector2Int(cell.x + 1, cell.y)))
                {
                    edges.Add(new FrontageSegment { horizontal = false, fixedCoord = maxX, start = minZ, end = maxZ, towardRoad = Vector3.left });
                }

                if (!roadCells.Contains(new Vector2Int(cell.x - 1, cell.y)))
                {
                    edges.Add(new FrontageSegment { horizontal = false, fixedCoord = minX, start = minZ, end = maxZ, towardRoad = Vector3.right });
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
            for (int i = 0; i < source.Count; i++)
            {
                FrontageSegment s = source[i];

                if (s.Length <= segmentEndMargin * 2f + 2f)
                {
                    continue;
                }

                frontageSegments.Add(s);
            }
        }

        private void CollectBuildingBlockers()
        {
            GameObject buildingRoot = GameObject.Find(buildingRootName);
            if (buildingRoot == null)
            {
                return;
            }

            for (int i = 0; i < buildingRoot.transform.childCount; i++)
            {
                Transform child = buildingRoot.transform.GetChild(i);
                if (child == null)
                {
                    continue;
                }

                Bounds bounds;
                if (!TryCalculateRendererBounds(child.gameObject, out bounds))
                {
                    continue;
                }

                Rect2 rect = new Rect2
                {
                    minX = bounds.min.x - buildingClearance,
                    maxX = bounds.max.x + buildingClearance,
                    minZ = bounds.min.z - buildingClearance,
                    maxZ = bounds.max.z + buildingClearance
                };

                if (rect.maxX <= rect.minX || rect.maxZ <= rect.minZ)
                {
                    continue;
                }

                buildingBlockers.Add(new BuildingBlocker
                {
                    rect = rect,
                    center = new Vector3((rect.minX + rect.maxX) * 0.5f, 0f, (rect.minZ + rect.maxZ) * 0.5f),
                    name = child.name
                });
            }
        }

        private void SpawnPropsOnSegment(FrontageSegment segment, Transform root)
        {
            TrySpawnRepeated(segment, root, streetLightPrefabs, streetLightSpacing, streetLightChance, curbPropOffsetFromRoad, 0.8f, 0.8f, false, true, false);
            TrySpawnRepeated(segment, root, hydrantPrefabs, hydrantSpacing, hydrantChance, curbPropOffsetFromRoad, 0.7f, 0.7f, false, false, false);
            TrySpawnRepeated(segment, root, trashCanPrefabs, trashCanSpacing, trashCanChance, sidewalkPropOffsetFromRoad, 0.8f, 0.8f, trashCansRandomYaw, false, false);
            TrySpawnRepeated(segment, root, mailboxPrefabs, mailboxSpacing, mailboxChance, sidewalkPropOffsetFromRoad, 0.9f, 0.9f, false, mailboxesFaceRoad, false);
            TrySpawnRepeated(segment, root, benchPrefabs, benchSpacing, benchChance, sidewalkPropOffsetFromRoad, 1.8f, 0.9f, false, benchesFaceRoad, true);
            TrySpawnRepeated(segment, root, paperDebrisPrefabs, paperDebrisSpacing, paperDebrisChance, sidewalkPropOffsetFromRoad, 0.7f, 0.7f, paperDebrisRandomYaw, false, false);
            TrySpawnRepeated(segment, root, paperDebrisPrefabs, paperDebrisSpacing * 1.15f, paperDebrisChance * 0.75f, sidewalkPropOffsetFromRoad + 1.25f, 0.7f, 0.7f, paperDebrisRandomYaw, false, false);
        }

        private void TrySpawnRepeated(
            FrontageSegment segment,
            Transform root,
            GameObject[] prefabs,
            float spacing,
            float chance,
            float offsetFromRoad,
            float footprintWidth,
            float footprintDepth,
            bool randomYaw,
            bool faceRoad,
            bool alignAlongStreet)
        {
            if (prefabs == null || prefabs.Length == 0 || chance <= 0f)
            {
                return;
            }

            Vector3 axis = segment.horizontal ? Vector3.right : Vector3.forward;
            Vector3 awayFromRoad = -segment.towardRoad.normalized;
            Vector3 towardRoad = segment.towardRoad.normalized;

            float start = segment.start + segmentEndMargin;
            float end = segment.end - segmentEndMargin;

            if (end <= start)
            {
                return;
            }

            float cursor = start + Random.Range(0f, spacing * 0.35f);
            while (cursor <= end)
            {
                if (Random.value <= chance)
                {
                    Vector3 position = segment.horizontal
                        ? new Vector3(cursor, 0f, segment.fixedCoord)
                        : new Vector3(segment.fixedCoord, 0f, cursor);

                    position += awayFromRoad * offsetFromRoad;

                    Quaternion rotation = Quaternion.identity;
                    if (randomYaw)
                    {
                        rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                    }
                    else if (faceRoad)
                    {
                        rotation = Quaternion.LookRotation(towardRoad, Vector3.up);
                    }
                    else if (alignAlongStreet)
                    {
                        rotation = Quaternion.LookRotation(axis, Vector3.up);
                    }

                    TrySpawnProp(root, prefabs, position, rotation, segment.horizontal, footprintWidth, footprintDepth);
                }

                cursor += spacing + Random.Range(-spacing * 0.15f, spacing * 0.15f);
            }
        }

        private bool TrySpawnProp(
            Transform root,
            GameObject[] prefabs,
            Vector3 position,
            Quaternion rotation,
            bool horizontalSegment,
            float footprintWidth,
            float footprintDepth)
        {
            Rect2 estimated = BuildPropRect(position, horizontalSegment, footprintWidth, footprintDepth, propFootprintPadding);

            if (RectOverlapsRoad(estimated) || RectOverlapsAnyBuilding(estimated) || RectOverlapsAny(estimated, propRects))
            {
                return false;
            }

            GameObject prefab = prefabs[Random.Range(0, prefabs.Length)];
            if (prefab == null)
            {
                return false;
            }

            GameObject instance = Instantiate(prefab, position, rotation, root);
            instance.name = prefab.name + "_StreetProp";

            AlignToGround(instance);

            Rect2 actual = estimated;
            Bounds bounds;
            if (TryCalculateRendererBounds(instance, out bounds))
            {
                actual = BuildRectFromBounds(bounds, propFootprintPadding);

                if (RectOverlapsRoad(actual) || RectOverlapsAnyBuilding(actual) || RectOverlapsAny(actual, propRects))
                {
                    Destroy(instance);
                    return false;
                }
            }

            propRects.Add(actual);
            spawnedProps++;
            return true;
        }

        private void SpawnDumpsterClusters(Transform root)
        {
            if ((dumpsterPrefabs == null || dumpsterPrefabs.Length == 0) &&
                (trashPilePrefabs == null || trashPilePrefabs.Length == 0))
            {
                return;
            }

            if (buildingBlockers.Count == 0)
            {
                if (logDetails)
                {
                    Debug.LogWarning("Street prop spawner found zero generated building blockers. Skips need Generated_RoadFrontage_Buildings to exist before they can be placed.", this);
                }

                return;
            }

            List<Rect2> dumpsterRects = new List<Rect2>();

            for (int i = 0; i < buildingBlockers.Count; i++)
            {
                if (dumpsterAttempts >= maxDumpsterAttempts)
                {
                    break;
                }

                if (Random.value > dumpsterChancePerBuilding)
                {
                    continue;
                }

                TryPlaceDumpsterNearBuilding(root, i, dumpsterRects);
            }

            int safety = 0;
            while (spawnedDumpsters < minimumDumpsterCount &&
                   dumpsterAttempts < maxDumpsterAttempts &&
                   buildingBlockers.Count > 0 &&
                   safety++ < maxDumpsterAttempts)
            {
                int index = Random.Range(0, buildingBlockers.Count);
                TryPlaceDumpsterNearBuilding(root, index, dumpsterRects);
            }
        }

        private bool TryPlaceDumpsterNearBuilding(Transform root, int buildingIndex, List<Rect2> dumpsterRects)
        {
            if (buildingIndex < 0 || buildingIndex >= buildingBlockers.Count)
            {
                return false;
            }

            BuildingBlocker blocker = buildingBlockers[buildingIndex];
            Rect2 building = blocker.rect;
            Vector3 center = blocker.center;

            Vector3 nearestRoadDirection;
            if (!TryGetNearestRoadDirection(center, out nearestRoadDirection))
            {
                return false;
            }

            Vector3 roadDir = nearestRoadDirection.normalized;
            Vector3[] candidateDirs =
            {
                -roadDir,
                Quaternion.Euler(0f, 90f, 0f) * roadDir,
                Quaternion.Euler(0f, -90f, 0f) * roadDir,
                roadDir
            };

            for (int d = 0; d < candidateDirs.Length; d++)
            {
                if (dumpsterAttempts >= maxDumpsterAttempts)
                {
                    return false;
                }

                Vector3 dir = candidateDirs[d].normalized;
                float pushDistance = Mathf.Max(dumpsterFootprintWidth, dumpsterFootprintDepth) * 0.5f + dumpsterBuildingOffset + buildingClearance;
                Vector3 pos = GetPointJustOutsideRect(building, dir, pushDistance);
                Quaternion rot = Quaternion.LookRotation(-dir, Vector3.up);

                Rect2 candidate = BuildPropRect(pos, Mathf.Abs(dir.z) > Mathf.Abs(dir.x), dumpsterFootprintWidth, dumpsterFootprintDepth, propFootprintPadding);
                dumpsterAttempts++;

                if (RectOverlapsRoad(candidate))
                {
                    dumpsterRoadRejects++;
                    continue;
                }

                if (RectOverlapsAnyBuildingExcept(candidate, buildingIndex))
                {
                    dumpsterBuildingRejects++;
                    continue;
                }

                if (RectOverlapsAny(candidate, dumpsterRects) ||
                    IsTooCloseToAny(candidate, dumpsterRects, dumpsterSpacing))
                {
                    dumpsterPropRejects++;
                    continue;
                }

                bool placedDumpster = false;
                if (dumpsterPrefabs != null && dumpsterPrefabs.Length > 0)
                {
                    placedDumpster = TrySpawnDumpsterProp(root, dumpsterPrefabs, pos, rot, Mathf.Abs(dir.z) > Mathf.Abs(dir.x), dumpsterFootprintWidth, dumpsterFootprintDepth, buildingIndex);
                }

                if (!placedDumpster && (trashPilePrefabs == null || trashPilePrefabs.Length == 0))
                {
                    continue;
                }

                dumpsterRects.Add(candidate);
                if (placedDumpster)
                {
                    spawnedDumpsters++;
                }

                SpawnTrashPilesNear(root, pos, dir);
                return true;
            }

            return false;
        }

        private bool TrySpawnDumpsterProp(
            Transform root,
            GameObject[] prefabs,
            Vector3 position,
            Quaternion rotation,
            bool horizontalSegment,
            float footprintWidth,
            float footprintDepth,
            int sourceBuildingIndex)
        {
            Rect2 estimated = BuildPropRect(position, horizontalSegment, footprintWidth, footprintDepth, propFootprintPadding);

            if (RectOverlapsRoad(estimated) ||
                RectOverlapsAnyBuildingExcept(estimated, sourceBuildingIndex))
            {
                return false;
            }

            GameObject prefab = prefabs[Random.Range(0, prefabs.Length)];
            if (prefab == null)
            {
                return false;
            }

            GameObject instance = Instantiate(prefab, position, rotation, root);
            instance.name = prefab.name + "_Dumpster";

            AlignToGround(instance);

            Rect2 actual = estimated;
            Bounds bounds;
            if (TryCalculateRendererBounds(instance, out bounds))
            {
                actual = BuildRectFromBounds(bounds, propFootprintPadding);

                if (RectOverlapsRoad(actual) ||
                    RectOverlapsAnyBuildingExcept(actual, sourceBuildingIndex))
                {
                    Destroy(instance);
                    return false;
                }
            }

            propRects.Add(actual);
            spawnedProps++;
            return true;
        }

        private void SpawnTrashPilesNear(Transform root, Vector3 dumpsterPosition, Vector3 awayFromBuilding)
        {
            Vector3 away = awayFromBuilding.sqrMagnitude > 0.01f ? awayFromBuilding.normalized : Vector3.forward;

            if (trashPilePrefabs != null && trashPilePrefabs.Length > 0)
            {
                for (int i = 0; i < trashPilesPerDumpster; i++)
                {
                    Vector2 scatter = Random.insideUnitCircle * trashPileScatterRadius;
                    Vector3 pos = dumpsterPosition + new Vector3(scatter.x, 0f, scatter.y);
                    pos += away * Random.Range(0.15f, 1.75f);

                    Quaternion rot = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                    if (TrySpawnProp(root, trashPilePrefabs, pos, rot, true, 0.9f, 0.9f))
                    {
                        spawnedTrashPiles++;
                    }
                }
            }

            SpawnPaperDebrisCluster(root, dumpsterPosition, away, paperDebrisPerDumpster, trashPileScatterRadius * 0.9f, 0.65f);
            SpawnPaperDebrisCluster(root, dumpsterPosition, away, outerDebrisPerDumpster, outerDebrisScatterRadius, 0.55f);
        }

        private void SpawnPaperDebrisCluster(Transform root, Vector3 origin, Vector3 awayFromBuilding, int count, float radius, float footprintSize)
        {
            if (paperDebrisPrefabs == null || paperDebrisPrefabs.Length == 0 || count <= 0)
            {
                return;
            }

            Vector3 away = awayFromBuilding.sqrMagnitude > 0.01f ? awayFromBuilding.normalized : Vector3.forward;

            for (int i = 0; i < count; i++)
            {
                Vector2 scatter = Random.insideUnitCircle * radius;
                Vector3 pos = origin + new Vector3(scatter.x, 0f, scatter.y);
                pos += away * Random.Range(0.05f, 1.5f);

                Quaternion rot = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                if (TrySpawnProp(root, paperDebrisPrefabs, pos, rot, true, footprintSize, footprintSize))
                {
                    spawnedSkipAreaPaperDebris++;
                }
            }
        }

        private bool TryGetNearestRoadDirection(Vector3 from, out Vector3 directionToRoad)
        {
            directionToRoad = Vector3.zero;
            float bestDistanceSq = float.MaxValue;

            foreach (Vector2Int cell in roadCells)
            {
                Vector3 roadCenter = new Vector3(
                    gridOriginX + (cell.x + 0.5f) * roadCellSize,
                    0f,
                    gridOriginZ + (cell.y + 0.5f) * roadCellSize);

                float distSq = (roadCenter - from).sqrMagnitude;
                if (distSq < bestDistanceSq)
                {
                    bestDistanceSq = distSq;
                    directionToRoad = roadCenter - from;
                    directionToRoad.y = 0f;
                }
            }

            return directionToRoad.sqrMagnitude > 0.01f;
        }

        private Vector3 GetPointJustOutsideRect(Rect2 rect, Vector3 dir, float offset)
        {
            Vector3 center = new Vector3((rect.minX + rect.maxX) * 0.5f, 0f, (rect.minZ + rect.maxZ) * 0.5f);

            if (Mathf.Abs(dir.x) > Mathf.Abs(dir.z))
            {
                center.x = dir.x > 0f ? rect.maxX + offset : rect.minX - offset;
            }
            else
            {
                center.z = dir.z > 0f ? rect.maxZ + offset : rect.minZ - offset;
            }

            return center;
        }

        private bool IsTooCloseToAny(Rect2 candidate, List<Rect2> list, float minDistance)
        {
            Vector2 c = new Vector2((candidate.minX + candidate.maxX) * 0.5f, (candidate.minZ + candidate.maxZ) * 0.5f);

            for (int i = 0; i < list.Count; i++)
            {
                Rect2 other = list[i];
                Vector2 o = new Vector2((other.minX + other.maxX) * 0.5f, (other.minZ + other.maxZ) * 0.5f);
                if (Vector2.Distance(c, o) < minDistance)
                {
                    return true;
                }
            }

            return false;
        }

        private Rect2 BuildRectFromBounds(Bounds bounds, float padding)
        {
            return new Rect2
            {
                minX = bounds.min.x - padding,
                maxX = bounds.max.x + padding,
                minZ = bounds.min.z - padding,
                maxZ = bounds.max.z + padding
            };
        }

        private Rect2 BuildPropRect(Vector3 center, bool horizontalSegment, float width, float depth, float padding)
        {
            float halfX = (horizontalSegment ? width : depth) * 0.5f + padding;
            float halfZ = (horizontalSegment ? depth : width) * 0.5f + padding;

            return new Rect2
            {
                minX = center.x - halfX,
                maxX = center.x + halfX,
                minZ = center.z - halfZ,
                maxZ = center.z + halfZ
            };
        }

        private bool RectOverlapsAny(Rect2 candidate, List<Rect2> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (RectsOverlap(candidate, list[i]))
                {
                    return true;
                }
            }

            return false;
        }



        private bool RectOverlapsAnyBuilding(Rect2 candidate)
        {
            for (int i = 0; i < buildingBlockers.Count; i++)
            {
                if (RectsOverlap(candidate, buildingBlockers[i].rect))
                {
                    return true;
                }
            }

            return false;
        }

        private bool RectOverlapsAnyBuildingExcept(Rect2 candidate, int skipBuildingIndex)
        {
            for (int i = 0; i < buildingBlockers.Count; i++)
            {
                if (i == skipBuildingIndex)
                {
                    continue;
                }

                if (RectsOverlap(candidate, buildingBlockers[i].rect))
                {
                    return true;
                }
            }

            return false;
        }

        private int CountPrefabs(GameObject[] prefabs)
        {
            return prefabs == null ? 0 : prefabs.Length;
        }

        private static bool RectsOverlap(Rect2 a, Rect2 b)
        {
            return a.minX < b.maxX &&
                   a.maxX > b.minX &&
                   a.minZ < b.maxZ &&
                   a.maxZ > b.minZ;
        }

        private bool RectOverlapsRoad(Rect2 rect)
        {
            int minX = Mathf.FloorToInt((rect.minX - gridOriginX) / roadCellSize);
            int maxX = Mathf.FloorToInt((rect.maxX - gridOriginX) / roadCellSize);
            int minZ = Mathf.FloorToInt((rect.minZ - gridOriginZ) / roadCellSize);
            int maxZ = Mathf.FloorToInt((rect.maxZ - gridOriginZ) / roadCellSize);

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

        private void AlignToGround(GameObject instance)
        {
            if (instance == null)
            {
                return;
            }

            Bounds bounds;
            if (!TryCalculateRendererBounds(instance, out bounds))
            {
                return;
            }

            Vector3 position = instance.transform.position;
            position.y -= bounds.min.y;
            instance.transform.position = position;
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

            Gizmos.color = Color.cyan;
            for (int i = 0; i < propRects.Count; i++)
            {
                Rect2 r = propRects[i];
                Vector3 center = new Vector3((r.minX + r.maxX) * 0.5f, 0.2f, (r.minZ + r.maxZ) * 0.5f);
                Vector3 size = new Vector3(r.maxX - r.minX, 0.1f, r.maxZ - r.minZ);
                Gizmos.DrawWireCube(center, size);
            }
        }
    }
}

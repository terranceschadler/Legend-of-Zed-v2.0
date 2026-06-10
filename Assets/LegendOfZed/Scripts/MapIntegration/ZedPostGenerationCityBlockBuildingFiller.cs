using System.Collections;
using System.Collections.Generic;
using LegendOfZed.LegacyMapGenerator;
using UnityEngine;

namespace LegendOfZed.MapIntegration
{
    /// <summary>
    /// Post-generation city block building filler.
    ///
    /// v3.5k rules:
    /// - CityBlock tag only.
    /// - No tile-footprint fallback.
    /// - No name/material fallback.
    /// - Rasterizes CityBlock renderer pieces into a 2D occupancy grid.
    /// - Builds placement edges where occupied CityBlock cells touch empty cells.
    /// - Uses round-robin placement so all streets get coverage before any one edge is overfilled.
    /// - Uses building depth when offsetting from roads so buildings do not protrude into streets.
    /// - Tracks generated building footprints to prevent generated buildings from overlapping each other.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ZedPostGenerationCityBlockBuildingFiller : MonoBehaviour
    {
        public static bool SuppressLegacyTileBuildingSpawns { get; private set; }

        [Header("References")]
        public ZedLegacyRandomMapGenerator mapGenerator;
        public GameObject[] buildingPrefabs;

        [Header("Tile Building Suppression")]
        public bool suppressLegacyTileBuildingSpawns = true;

        [Header("CityBlock Detection")]
        public string cityBlockTag = "CityBlock";
        public bool scanChildRenderersOfTaggedRoots = true;
        [Min(0.05f)] public float minRendererPieceSize = 0.25f;
        [Min(0.05f)] public float maxRendererPieceHeight = 2f;
        public float maxSurfaceCenterY = 2f;

        [Header("Grid Contour")]
        [Min(0.25f)] public float contourCellSize = 2.5f;
        [Min(0f)] public float pieceInset = 0.02f;
        [Min(0.1f)] public float minContourEdgeLength = 5f;
        [Min(1)] public int maxPlacementEdges = 512;

        [Header("Street Coverage")]
        public bool useRoundRobinStreetCoverage = true;
        [Min(0.1f)] public float minStreetCoverageEdgeLength = 8f;

        [Header("Alleys")]
        [Min(0)] public int guaranteedAlleyEveryNBuildings = 5;

        [Header("Fallbacks Disabled")]
        public bool trySurfaceRendererDetection = false;
        public bool fallbackToGeneratedTileFootprints = false;

        [Header("Building Placement")]
        [Tooltip("Extra setback behind the sidewalk/road edge after accounting for half of the building depth.")]
        [Min(0f)] public float edgeInset = 0.75f;

        [Tooltip("Padding used by generated building footprint overlap checks.")]
        [Min(0f)] public float generatedBuildingPadding = 0.35f;

        [Tooltip("When true, generated buildings cannot overlap each other in 2D footprint space.")]
        public bool preventGeneratedBuildingOverlap = true;

        [Tooltip("When true, generated building centers must remain inside CityBlock occupied cells.")]
        public bool requireBuildingCenterOnCityBlock = true;

        [Min(0f)] public float buildingGap = 0.1f;
        [Range(0f, 1f)] public float alleyChance = 0.08f;
        [Min(0f)] public float alleyWidthMin = 2f;
        [Min(0f)] public float alleyWidthMax = 4f;
        [Min(1f)] public float minEdgeLength = 8f;
        [Min(1)] public int maxBuildingsPerEdge = 64;
        [Min(1)] public int maxTotalBuildings = 600;

        [Header("Safety")]
        [Min(0f)] public float overlapCheckHalfHeight = 4f;
        public LayerMask blockingMask = ~0;
        public bool useOverlapCheck = false;

        [Header("Generated Root")]
        public string generatedRootName = "Generated_City_Block_Buildings";

        [Header("Debug")]
        public bool runOnStart = true;
        public bool logDetails = true;
        public bool drawDebugGizmos = true;

        private struct Rect2
        {
            public float minX;
            public float maxX;
            public float minZ;
            public float maxZ;
        }

        private struct Edge
        {
            public bool horizontal;
            public float fixedCoord;
            public float start;
            public float end;
            public int outwardSign;
            public Vector3 inward;

            public float Length
            {
                get { return Mathf.Abs(end - start); }
            }
        }

        private struct EdgeFillState
        {
            public Edge edge;
            public float cursor;
            public int placedOnEdge;
            public int failedAttempts;
            public bool finished;
        }

        private struct Footprint2D
        {
            public float minX;
            public float maxX;
            public float minZ;
            public float maxZ;
        }

        private readonly HashSet<Vector2Int> occupiedCells = new HashSet<Vector2Int>();
        private readonly List<Edge> debugEdges = new List<Edge>();
        private readonly List<Bounds> debugPieces = new List<Bounds>();
        private readonly List<Footprint2D> spawnedFootprints = new List<Footprint2D>();

        private float gridOriginX;
        private float gridOriginZ;
        private int spawnedTotal;
        private bool built;

        private void OnValidate()
        {
            ForceSafeDefaults();
        }

        private void Awake()
        {
            ForceSafeDefaults();

            if (suppressLegacyTileBuildingSpawns)
            {
                SuppressLegacyTileBuildingSpawns = true;
            }
        }

        private void ForceSafeDefaults()
        {
            if (string.IsNullOrEmpty(cityBlockTag))
            {
                cityBlockTag = "CityBlock";
            }

            trySurfaceRendererDetection = false;
            fallbackToGeneratedTileFootprints = false;
            scanChildRenderersOfTaggedRoots = true;
            contourCellSize = Mathf.Max(0.25f, contourCellSize);
            maxPlacementEdges = Mathf.Max(1, maxPlacementEdges);
            maxBuildingsPerEdge = Mathf.Max(1, maxBuildingsPerEdge);
            maxTotalBuildings = Mathf.Max(1, maxTotalBuildings);
            minStreetCoverageEdgeLength = Mathf.Max(0.1f, minStreetCoverageEdgeLength);
            guaranteedAlleyEveryNBuildings = Mathf.Max(0, guaranteedAlleyEveryNBuildings);
            generatedBuildingPadding = Mathf.Max(0f, generatedBuildingPadding);
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

            if (mapGenerator != null)
            {
                while (!mapGenerator.bakingNavMeshCompleted)
                {
                    yield return null;
                }
            }
            else
            {
                yield return null;
                yield return null;
            }

            yield return null;
            BuildNow();
        }

        [ContextMenu("Build City Block Buildings Now")]
        public void BuildNow()
        {
            ForceSafeDefaults();

            if (built)
            {
                return;
            }

            built = true;
            spawnedTotal = 0;
            occupiedCells.Clear();
            debugEdges.Clear();
            debugPieces.Clear();
            spawnedFootprints.Clear();

            Transform root = GetOrCreateGeneratedRoot();
            ClearChildren(root);

            List<Rect2> pieces = DetectCityBlockPieceRects();
            RasterizePieces(pieces);

            List<Edge> edges = BuildGridContourEdges();
            edges = MergeCollinearEdges(edges);
            edges = FilterPlacementEdges(edges);

            debugEdges.AddRange(edges);

            int spawned = useRoundRobinStreetCoverage
                ? FillContourEdgesRoundRobin(edges, root)
                : FillContourEdgesSequential(edges, root);

            if (logDetails)
            {
                Debug.Log(
                    "CityBlock street-coverage filler complete. TaggedPieces=" + pieces.Count +
                    ", occupiedCells=" + occupiedCells.Count +
                    ", placementEdges=" + edges.Count +
                    ", Buildings=" + spawned +
                    ", cellSize=" + contourCellSize.ToString("0.##") +
                    ", roundRobin=" + useRoundRobinStreetCoverage +
                    ", fallbacks=OFF.",
                    this);
            }
        }

        private List<Rect2> DetectCityBlockPieceRects()
        {
            List<Transform> taggedRoots = FindTaggedCityBlockRoots();
            List<Rect2> rects = new List<Rect2>();

            for (int i = 0; i < taggedRoots.Count; i++)
            {
                Transform root = taggedRoots[i];
                if (root == null)
                {
                    continue;
                }

                Renderer[] renderers = scanChildRenderersOfTaggedRoots
                    ? root.GetComponentsInChildren<Renderer>(true)
                    : new Renderer[] { root.GetComponent<Renderer>() };

                for (int r = 0; r < renderers.Length; r++)
                {
                    Renderer renderer = renderers[r];
                    if (renderer == null || !renderer.enabled)
                    {
                        continue;
                    }

                    Bounds b = FlattenBounds(renderer.bounds);

                    if (b.center.y > maxSurfaceCenterY)
                    {
                        continue;
                    }

                    if (b.size.y > maxRendererPieceHeight)
                    {
                        continue;
                    }

                    if (b.size.x < minRendererPieceSize || b.size.z < minRendererPieceSize)
                    {
                        continue;
                    }

                    Rect2 rect = new Rect2
                    {
                        minX = b.min.x + pieceInset,
                        maxX = b.max.x - pieceInset,
                        minZ = b.min.z + pieceInset,
                        maxZ = b.max.z - pieceInset
                    };

                    if (rect.maxX <= rect.minX || rect.maxZ <= rect.minZ)
                    {
                        continue;
                    }

                    rects.Add(rect);
                    debugPieces.Add(b);
                }
            }

            if (logDetails)
            {
                Debug.Log("CityBlock grid contour detection found TaggedRoots=" + taggedRoots.Count + ", rendererPieces=" + rects.Count + ".", this);

                if (rects.Count == 0)
                {
                    Debug.LogWarning("CityBlock grid contour filler found 0 tagged renderer pieces. Tag the sidewalk/block surface object or parent as CityBlock.", this);
                }
            }

            return rects;
        }

        private List<Transform> FindTaggedCityBlockRoots()
        {
            List<Transform> roots = new List<Transform>();
            Transform[] allTransforms = FindObjectsByType<Transform>(FindObjectsInactive.Exclude);

            for (int i = 0; i < allTransforms.Length; i++)
            {
                Transform t = allTransforms[i];
                if (t == null || !SafeTagEquals(t.gameObject, cityBlockTag))
                {
                    continue;
                }

                Transform owner = FindTopmostTaggedParent(t);
                if (owner != null && !roots.Contains(owner))
                {
                    roots.Add(owner);
                }
            }

            return roots;
        }

        private Transform FindTopmostTaggedParent(Transform t)
        {
            if (t == null)
            {
                return null;
            }

            Transform top = t;
            Transform current = t.parent;

            while (current != null)
            {
                if (SafeTagEquals(current.gameObject, cityBlockTag))
                {
                    top = current;
                }

                current = current.parent;
            }

            return top;
        }

        private void RasterizePieces(List<Rect2> pieces)
        {
            occupiedCells.Clear();

            if (pieces.Count == 0)
            {
                return;
            }

            float minX = pieces[0].minX;
            float minZ = pieces[0].minZ;

            for (int i = 1; i < pieces.Count; i++)
            {
                minX = Mathf.Min(minX, pieces[i].minX);
                minZ = Mathf.Min(minZ, pieces[i].minZ);
            }

            gridOriginX = Mathf.Floor(minX / contourCellSize) * contourCellSize;
            gridOriginZ = Mathf.Floor(minZ / contourCellSize) * contourCellSize;

            for (int i = 0; i < pieces.Count; i++)
            {
                Rect2 r = pieces[i];

                int minCellX = Mathf.FloorToInt((r.minX - gridOriginX) / contourCellSize);
                int maxCellX = Mathf.FloorToInt((r.maxX - gridOriginX) / contourCellSize);
                int minCellZ = Mathf.FloorToInt((r.minZ - gridOriginZ) / contourCellSize);
                int maxCellZ = Mathf.FloorToInt((r.maxZ - gridOriginZ) / contourCellSize);

                for (int x = minCellX; x <= maxCellX; x++)
                {
                    for (int z = minCellZ; z <= maxCellZ; z++)
                    {
                        float centerX = gridOriginX + (x + 0.5f) * contourCellSize;
                        float centerZ = gridOriginZ + (z + 0.5f) * contourCellSize;

                        if (centerX >= r.minX && centerX <= r.maxX && centerZ >= r.minZ && centerZ <= r.maxZ)
                        {
                            occupiedCells.Add(new Vector2Int(x, z));
                        }
                    }
                }
            }
        }

        private List<Edge> BuildGridContourEdges()
        {
            List<Edge> edges = new List<Edge>();

            foreach (Vector2Int cell in occupiedCells)
            {
                float minX = gridOriginX + cell.x * contourCellSize;
                float maxX = minX + contourCellSize;
                float minZ = gridOriginZ + cell.y * contourCellSize;
                float maxZ = minZ + contourCellSize;

                if (!occupiedCells.Contains(new Vector2Int(cell.x, cell.y + 1)))
                {
                    edges.Add(new Edge
                    {
                        horizontal = true,
                        fixedCoord = maxZ,
                        start = minX,
                        end = maxX,
                        outwardSign = 1,
                        inward = Vector3.back
                    });
                }

                if (!occupiedCells.Contains(new Vector2Int(cell.x, cell.y - 1)))
                {
                    edges.Add(new Edge
                    {
                        horizontal = true,
                        fixedCoord = minZ,
                        start = minX,
                        end = maxX,
                        outwardSign = -1,
                        inward = Vector3.forward
                    });
                }

                if (!occupiedCells.Contains(new Vector2Int(cell.x + 1, cell.y)))
                {
                    edges.Add(new Edge
                    {
                        horizontal = false,
                        fixedCoord = maxX,
                        start = minZ,
                        end = maxZ,
                        outwardSign = 1,
                        inward = Vector3.left
                    });
                }

                if (!occupiedCells.Contains(new Vector2Int(cell.x - 1, cell.y)))
                {
                    edges.Add(new Edge
                    {
                        horizontal = false,
                        fixedCoord = minX,
                        start = minZ,
                        end = maxZ,
                        outwardSign = -1,
                        inward = Vector3.right
                    });
                }
            }

            return edges;
        }

        private List<Edge> MergeCollinearEdges(List<Edge> edges)
        {
            edges.Sort((a, b) =>
            {
                int axisCompare = a.horizontal.CompareTo(b.horizontal);
                if (axisCompare != 0) return axisCompare;

                int fixedCompare = a.fixedCoord.CompareTo(b.fixedCoord);
                if (fixedCompare != 0) return fixedCompare;

                int signCompare = a.outwardSign.CompareTo(b.outwardSign);
                if (signCompare != 0) return signCompare;

                return a.start.CompareTo(b.start);
            });

            List<Edge> merged = new List<Edge>();

            for (int i = 0; i < edges.Count; i++)
            {
                Edge e = edges[i];

                if (merged.Count == 0)
                {
                    merged.Add(e);
                    continue;
                }

                Edge last = merged[merged.Count - 1];

                bool sameLine = last.horizontal == e.horizontal &&
                                Mathf.Abs(last.fixedCoord - e.fixedCoord) <= 0.01f &&
                                last.outwardSign == e.outwardSign &&
                                Mathf.Abs(last.end - e.start) <= 0.01f;

                if (sameLine)
                {
                    last.end = Mathf.Max(last.end, e.end);
                    merged[merged.Count - 1] = last;
                }
                else
                {
                    merged.Add(e);
                }
            }

            return merged;
        }

        private List<Edge> FilterPlacementEdges(List<Edge> source)
        {
            List<Edge> result = new List<Edge>();

            source.Sort((a, b) => b.Length.CompareTo(a.Length));

            for (int i = 0; i < source.Count; i++)
            {
                Edge e = source[i];
                if (e.Length < minContourEdgeLength || e.Length < minEdgeLength || e.Length < minStreetCoverageEdgeLength)
                {
                    continue;
                }

                result.Add(e);
                if (result.Count >= maxPlacementEdges)
                {
                    break;
                }
            }

            return result;
        }

        private int FillContourEdgesSequential(List<Edge> edges, Transform parent)
        {
            int spawned = 0;

            for (int i = 0; i < edges.Count; i++)
            {
                if (spawnedTotal >= maxTotalBuildings)
                {
                    break;
                }

                EdgeFillState state = CreateEdgeFillState(edges[i]);
                while (!state.finished && spawnedTotal < maxTotalBuildings)
                {
                    if (TryPlaceOneOnEdge(ref state, parent))
                    {
                        spawned++;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return spawned;
        }

        private int FillContourEdgesRoundRobin(List<Edge> edges, Transform parent)
        {
            if (edges == null || edges.Count == 0)
            {
                return 0;
            }

            List<EdgeFillState> states = new List<EdgeFillState>();
            for (int i = 0; i < edges.Count; i++)
            {
                states.Add(CreateEdgeFillState(edges[i]));
            }

            int spawned = 0;
            bool placedThisPass = true;
            int safetyPasses = Mathf.Max(1, maxBuildingsPerEdge + 4);

            while (placedThisPass && spawnedTotal < maxTotalBuildings && safetyPasses-- > 0)
            {
                placedThisPass = false;

                for (int i = 0; i < states.Count; i++)
                {
                    if (spawnedTotal >= maxTotalBuildings)
                    {
                        break;
                    }

                    EdgeFillState state = states[i];
                    if (state.finished)
                    {
                        continue;
                    }

                    if (TryPlaceOneOnEdge(ref state, parent))
                    {
                        spawned++;
                        placedThisPass = true;
                    }

                    states[i] = state;
                }
            }

            return spawned;
        }

        private EdgeFillState CreateEdgeFillState(Edge edge)
        {
            return new EdgeFillState
            {
                edge = edge,
                cursor = -edge.Length * 0.5f,
                placedOnEdge = 0,
                failedAttempts = 0,
                finished = edge.Length < minEdgeLength
            };
        }

        private bool TryPlaceOneOnEdge(ref EdgeFillState state, Transform parent)
        {
            if (state.finished || spawnedTotal >= maxTotalBuildings || state.placedOnEdge >= maxBuildingsPerEdge)
            {
                state.finished = true;
                return false;
            }

            Edge edge = state.edge;
            float length = edge.Length;

            if (state.cursor >= length * 0.5f)
            {
                state.finished = true;
                return false;
            }

            float remaining = length * 0.5f - state.cursor;
            GameObject prefab = PickBuildingThatFits(remaining);
            if (prefab == null)
            {
                state.finished = true;
                return false;
            }

            Vector2 footprint = ZedBuildingFootprint.EstimateFootprintSize(prefab);
            float buildingWidth = Mathf.Max(0.1f, footprint.x);
            float buildingDepth = Mathf.Max(0.1f, footprint.y);

            Vector3 axis = edge.horizontal ? Vector3.right : Vector3.forward;
            Vector3 inward = edge.inward.normalized;
            Quaternion rotation = RotationForInward(inward);

            // Important:
            // The building center must be moved inward by half of its depth plus a small setback.
            // A fixed edgeInset alone lets deep buildings stick out into the road.
            float perpendicularInset = buildingDepth * 0.5f + edgeInset;

            Vector3 edgeCenter = edge.horizontal
                ? new Vector3((edge.start + edge.end) * 0.5f, 0f, edge.fixedCoord)
                : new Vector3(edge.fixedCoord, 0f, (edge.start + edge.end) * 0.5f);

            Vector3 center = edgeCenter + inward * perpendicularInset;

            float offset = state.cursor + buildingWidth * 0.5f;
            Vector3 position = center + axis.normalized * offset;

            Footprint2D placedFootprint = BuildAxisAlignedFootprint(position, edge.horizontal, buildingWidth, buildingDepth, generatedBuildingPadding);

            if (requireBuildingCenterOnCityBlock && !IsWorldPointInsideOccupiedCityBlock(position.x, position.z))
            {
                state.cursor += Mathf.Max(1f, buildingWidth * 0.5f);
                state.failedAttempts++;
                if (state.failedAttempts > maxBuildingsPerEdge * 3)
                {
                    state.finished = true;
                }
                return false;
            }

            if (preventGeneratedBuildingOverlap && OverlapsSpawnedFootprints(placedFootprint))
            {
                state.cursor += Mathf.Max(1f, buildingWidth * 0.5f);
                state.failedAttempts++;
                if (state.failedAttempts > maxBuildingsPerEdge * 3)
                {
                    state.finished = true;
                }
                return false;
            }

            if (useOverlapCheck && IsBlocked(position, buildingWidth, buildingDepth, rotation))
            {
                state.cursor += Mathf.Max(1f, buildingWidth * 0.5f);
                state.failedAttempts++;
                if (state.failedAttempts > maxBuildingsPerEdge * 3)
                {
                    state.finished = true;
                }
                return false;
            }

            GameObject instance = Instantiate(prefab, position, rotation, parent);
            instance.name = prefab.name + "_CityBlockStreetBuilding";

            spawnedFootprints.Add(placedFootprint);

            state.placedOnEdge++;
            state.failedAttempts = 0;
            spawnedTotal++;

            state.cursor += buildingWidth + buildingGap;

            bool guaranteedAlley = guaranteedAlleyEveryNBuildings > 0 &&
                                   state.placedOnEdge > 0 &&
                                   state.placedOnEdge % guaranteedAlleyEveryNBuildings == 0;

            bool randomAlley = state.placedOnEdge >= 2 && Random.value < alleyChance;

            if (guaranteedAlley || randomAlley)
            {
                state.cursor += Random.Range(Mathf.Min(alleyWidthMin, alleyWidthMax), Mathf.Max(alleyWidthMin, alleyWidthMax));
            }

            if (state.cursor >= length * 0.5f || state.placedOnEdge >= maxBuildingsPerEdge)
            {
                state.finished = true;
            }

            return true;
        }

        private Footprint2D BuildAxisAlignedFootprint(Vector3 center, bool edgeHorizontal, float buildingWidth, float buildingDepth, float padding)
        {
            float halfX;
            float halfZ;

            if (edgeHorizontal)
            {
                halfX = buildingWidth * 0.5f + padding;
                halfZ = buildingDepth * 0.5f + padding;
            }
            else
            {
                halfX = buildingDepth * 0.5f + padding;
                halfZ = buildingWidth * 0.5f + padding;
            }

            return new Footprint2D
            {
                minX = center.x - halfX,
                maxX = center.x + halfX,
                minZ = center.z - halfZ,
                maxZ = center.z + halfZ
            };
        }

        private bool OverlapsSpawnedFootprints(Footprint2D candidate)
        {
            for (int i = 0; i < spawnedFootprints.Count; i++)
            {
                Footprint2D other = spawnedFootprints[i];

                bool overlaps = candidate.minX < other.maxX &&
                                candidate.maxX > other.minX &&
                                candidate.minZ < other.maxZ &&
                                candidate.maxZ > other.minZ;

                if (overlaps)
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsWorldPointInsideOccupiedCityBlock(float worldX, float worldZ)
        {
            int x = Mathf.FloorToInt((worldX - gridOriginX) / contourCellSize);
            int z = Mathf.FloorToInt((worldZ - gridOriginZ) / contourCellSize);
            return occupiedCells.Contains(new Vector2Int(x, z));
        }

        private Quaternion RotationForInward(Vector3 inward)
        {
            if (inward.sqrMagnitude < 0.001f)
            {
                return Quaternion.identity;
            }

            return Quaternion.LookRotation(-inward.normalized, Vector3.up);
        }

        private GameObject PickBuildingThatFits(float remainingWidth)
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

                Vector2 footprint = ZedBuildingFootprint.EstimateFootprintSize(prefab);
                if (footprint.x <= remainingWidth)
                {
                    candidates.Add(prefab);
                }
            }

            if (candidates.Count == 0)
            {
                return null;
            }

            candidates.Sort((a, b) => ZedBuildingFootprint.EstimateFootprintSize(a).x.CompareTo(ZedBuildingFootprint.EstimateFootprintSize(b).x));

            int topCount = Mathf.Clamp(Mathf.CeilToInt(candidates.Count * 0.65f), 1, candidates.Count);
            return candidates[Random.Range(0, topCount)];
        }

        private bool IsBlocked(Vector3 center, float width, float depth, Quaternion rotation)
        {
            Vector3 halfExtents = new Vector3(width * 0.45f, overlapCheckHalfHeight, depth * 0.45f);
            Collider[] hits = Physics.OverlapBox(center + Vector3.up * overlapCheckHalfHeight, halfExtents, rotation, blockingMask, QueryTriggerInteraction.Ignore);
            if (hits == null || hits.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < hits.Length; i++)
            {
                Collider hit = hits[i];
                if (hit == null)
                {
                    continue;
                }

                string n = hit.name.ToLowerInvariant();
                if (n.Contains("floor") || n.Contains("sidewalk") || n.Contains("pavement") || SafeTagEquals(hit.gameObject, "Walkable") || SafeTagEquals(hit.gameObject, cityBlockTag))
                {
                    continue;
                }

                return true;
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

        private static bool SafeTagEquals(GameObject go, string tagName)
        {
            if (go == null || string.IsNullOrEmpty(tagName))
            {
                return false;
            }

            try
            {
                return go.tag == tagName;
            }
            catch (UnityException)
            {
                return false;
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!drawDebugGizmos)
            {
                return;
            }

            Gizmos.color = Color.cyan;
            for (int i = 0; i < debugPieces.Count; i++)
            {
                Bounds b = debugPieces[i];
                Gizmos.DrawWireCube(b.center + Vector3.up * 0.15f, new Vector3(b.size.x, 0.1f, b.size.z));
            }

            Gizmos.color = Color.yellow;
            for (int i = 0; i < debugEdges.Count; i++)
            {
                Edge e = debugEdges[i];
                Vector3 a = e.horizontal
                    ? new Vector3(e.start, 0.25f, e.fixedCoord)
                    : new Vector3(e.fixedCoord, 0.25f, e.start);

                Vector3 b = e.horizontal
                    ? new Vector3(e.end, 0.25f, e.fixedCoord)
                    : new Vector3(e.fixedCoord, 0.25f, e.end);

                Gizmos.DrawLine(a, b);
            }
        }
    }
}

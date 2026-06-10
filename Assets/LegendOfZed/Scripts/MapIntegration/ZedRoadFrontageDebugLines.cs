using System.Collections;
using System.Collections.Generic;
using LegendOfZed.LegacyMapGenerator;
using UnityEngine;

namespace LegendOfZed.MapIntegration
{
    /// <summary>
    /// v3.6a diagnostic only.
    ///
    /// Detects road surface cells and draws road-facing frontage lines.
    /// Does not spawn buildings.
    ///
    /// Green lines = proposed building frontage lines.
    /// Yellow arrows = direction buildings should face, toward the road.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ZedRoadFrontageDebugLines : MonoBehaviour
    {
        [Header("References")]
        public ZedLegacyRandomMapGenerator mapGenerator;

        [Header("Detection")]
        [Tooltip("Only renderers under this root are scanned when assigned. Leave null to scan the whole scene.")]
        public Transform scanRoot;

        [Tooltip("Renderer/object/material names containing any of these terms are treated as road surfaces.")]
        public string[] roadNameContains =
        {
            "road",
            "street",
            "asphalt"
        };

        [Tooltip("Renderer/object/material names containing any of these terms are rejected even if they also match road terms.")]
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

        [Tooltip("Maximum road renderer center Y allowed.")]
        public float maxRoadCenterY = 2f;

        [Tooltip("Maximum road renderer height allowed.")]
        [Min(0.01f)] public float maxRoadRendererHeight = 1.5f;

        [Tooltip("Minimum X/Z size for road renderer bounds.")]
        [Min(0.01f)] public float minRoadRendererSize = 0.25f;

        [Header("Grid")]
        [Tooltip("Raster cell size used to convert road surfaces into road occupancy. Use 2.5 for current map scale.")]
        [Min(0.25f)] public float roadCellSize = 2.5f;

        [Tooltip("Inset road bounds slightly before rasterizing.")]
        [Min(0f)] public float roadPieceInset = 0.02f;

        [Header("Frontage Lines")]
        [Tooltip("Distance from road edge outward to proposed building frontage line.")]
        [Min(0f)] public float frontageOffsetFromRoad = 4.5f;

        [Tooltip("Ignore frontage segments shorter than this.")]
        [Min(0.1f)] public float minFrontageSegmentLength = 6f;

        [Tooltip("Maximum frontage segments kept for debug drawing.")]
        [Min(1)] public int maxFrontageSegments = 512;

        [Tooltip("Show yellow direction arrows pointing toward the road.")]
        public bool drawRoadFacingArrows = true;

        [Tooltip("Arrow spacing along frontage lines.")]
        [Min(1f)] public float arrowSpacing = 12f;

        [Tooltip("Arrow length.")]
        [Min(0.5f)] public float arrowLength = 3f;

        [Header("Run")]
        public bool runOnStart = true;
        public bool waitForMapGenerator = true;
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

        private readonly HashSet<Vector2Int> roadCells = new HashSet<Vector2Int>();
        private readonly List<Rect2> roadRects = new List<Rect2>();
        private readonly List<FrontageSegment> frontageSegments = new List<FrontageSegment>();

        private float gridOriginX;
        private float gridOriginZ;

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

        [ContextMenu("Build Road Frontage Debug Lines Now")]
        public void BuildNow()
        {
            roadCells.Clear();
            roadRects.Clear();
            frontageSegments.Clear();

            DetectRoadRects();
            RasterizeRoadRects();
            List<FrontageSegment> rawEdges = BuildRoadBoundaryFrontageSegments();
            List<FrontageSegment> merged = MergeCollinearSegments(rawEdges);
            FilterFrontageSegments(merged);

            if (logDetails)
            {
                Debug.Log(
                    "Road frontage debug complete. RoadRects=" + roadRects.Count +
                    ", RoadCells=" + roadCells.Count +
                    ", FrontageSegments=" + frontageSegments.Count +
                    ", SpawnedBuildings=0.",
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
            if (renderer == null)
            {
                return false;
            }

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

            string combined = BuildSearchText(renderer);

            for (int i = 0; i < rejectNameContains.Length; i++)
            {
                string reject = rejectNameContains[i];
                if (!string.IsNullOrEmpty(reject) && combined.Contains(reject.ToLowerInvariant()))
                {
                    return false;
                }
            }

            for (int i = 0; i < roadNameContains.Length; i++)
            {
                string term = roadNameContains[i];
                if (!string.IsNullOrEmpty(term) && combined.Contains(term.ToLowerInvariant()))
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

                // North road edge. Empty side is north. Frontage line sits north of road, arrow points south.
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

                // South road edge.
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

                // East road edge.
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

                // West road edge.
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

            if (!drawRoadFacingArrows)
            {
                return;
            }

            Gizmos.color = Color.yellow;
            for (int i = 0; i < frontageSegments.Count; i++)
            {
                DrawArrows(frontageSegments[i]);
            }
        }

        private void DrawArrows(FrontageSegment segment)
        {
            float length = segment.Length;
            int count = Mathf.Max(1, Mathf.FloorToInt(length / arrowSpacing));

            for (int i = 0; i < count; i++)
            {
                float t = (i + 0.5f) / count;
                float along = Mathf.Lerp(segment.start, segment.end, t);

                Vector3 start = segment.horizontal
                    ? new Vector3(along, 0.35f, segment.fixedCoord)
                    : new Vector3(segment.fixedCoord, 0.35f, along);

                Vector3 end = start + segment.towardRoad.normalized * arrowLength;

                Gizmos.DrawLine(start, end);
                Vector3 right = Quaternion.Euler(0f, 35f, 0f) * (-segment.towardRoad.normalized);
                Vector3 left = Quaternion.Euler(0f, -35f, 0f) * (-segment.towardRoad.normalized);
                Gizmos.DrawLine(end, end + right * (arrowLength * 0.35f));
                Gizmos.DrawLine(end, end + left * (arrowLength * 0.35f));
            }
        }
    }
}

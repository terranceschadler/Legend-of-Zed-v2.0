using System.Collections;
using System.Collections.Generic;
using LegendOfZed.LegacyMapGenerator;
using UnityEngine;

namespace LegendOfZed.MapIntegration
{
    /// <summary>
    /// v3.9f automatic buildable zone coverage.
    ///
    /// Creates additional ZedBuildableZone strips on generated map tiles so the
    /// zone-based building spawner has enough legal street-facing areas to fill.
    ///
    /// This does not spawn buildings directly.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ZedAutoBuildableZoneCoverage : MonoBehaviour
    {
        [Header("Roots")]
        public Transform scanRoot;
        public string generatedMapRootName = "Generated_Map_Tiles";
        public string generatedZoneRootName = "Generated_AutoBuildable_Zones";
        public bool clearPreviousAutoZones = true;

        [Header("Timing")]
        public bool buildOnStart = true;
        [Min(0f)] public float startDelay = 0.25f;

        [Header("Tile Filtering")]
        public string[] skipTileNameContains =
        {
            "park",
            "plaza",
            "water"
        };

        [Tooltip("Do not add zones to tiles with many road renderers/intersection complexity, unless Add Zones To Intersections is enabled.")]
        public bool skipComplexIntersectionTiles = false;

        public bool addZonesToIntersections = true;

        [Min(0)] public int complexRoadRendererThreshold = 34;

        [Header("Zone Shape")]
        [Tooltip("Depth of each generated buildable strip.")]
        [Min(2f)] public float zoneDepth = 7.5f;

        [Tooltip("Minimum width needed to create a strip.")]
        [Min(2f)] public float minZoneWidth = 8f;

        [Tooltip("Inset from the outer tile edge.")]
        [Min(0f)] public float outerEdgeInset = 1.25f;

        [Tooltip("Extra gap kept away from detected road rectangles.")]
        [Min(0f)] public float roadClearance = 1.25f;

        [Tooltip("Side padding used by generated ZedBuildableZone components.")]
        [Min(0f)] public float generatedZoneSidePadding = 0.2f;

        [Tooltip("Edge inset used by generated ZedBuildableZone components.")]
        [Min(0f)] public float generatedZoneEdgeInset = 0.25f;

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

        [Header("Generated Zone Defaults")]
        public ZedBuildableZone.DepthAlignment depthAlignment = ZedBuildableZone.DepthAlignment.BackEdge;
        public float buildingYawOffset = 180f;
        public bool allowAlleys = true;

        [Header("Logging")]
        public bool logDetails = true;

        [Header("Debug")]
        public bool drawCandidateGizmos = true;
        public Color acceptedZoneColor = new Color(0.2f, 1f, 0.4f, 0.75f);
        public Color rejectedZoneColor = new Color(1f, 0.2f, 0.2f, 0.45f);

        private readonly List<Rect2> roadRects = new List<Rect2>();
        private readonly List<CandidateZone> acceptedCandidates = new List<CandidateZone>();
        private readonly List<CandidateZone> rejectedCandidates = new List<CandidateZone>();

        private int tilesFound;
        private int tilesUsed;
        private int zonesCreated;
        private int roadRejects;
        private int smallRejects;
        private int skippedTiles;

        private enum EdgeSide
        {
            North,
            South,
            East,
            West
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

        private struct CandidateZone
        {
            public Rect2 rect;
            public EdgeSide side;
            public string tileName;
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

        [ContextMenu("Build Auto Buildable Zones Now")]
        public void BuildNow()
        {
            OnValidate();

            ResolveScanRoot();

            tilesFound = 0;
            tilesUsed = 0;
            zonesCreated = 0;
            roadRejects = 0;
            smallRejects = 0;
            skippedTiles = 0;

            roadRects.Clear();
            acceptedCandidates.Clear();
            rejectedCandidates.Clear();

            if (scanRoot == null)
            {
                Debug.LogWarning("Auto buildable zone coverage could not find generated map root.", this);
                return;
            }

            Transform zoneRoot = GetOrCreateZoneRoot();
            if (clearPreviousAutoZones)
            {
                ClearChildren(zoneRoot);
            }

            DetectRoadRects();

            ZedLegacyRoomTile[] tiles = scanRoot.GetComponentsInChildren<ZedLegacyRoomTile>(true);
            tilesFound = tiles.Length;

            for (int i = 0; i < tiles.Length; i++)
            {
                ProcessTile(tiles[i], zoneRoot);
            }

            if (logDetails)
            {
                Debug.Log(
                    "Auto buildable zone coverage complete. Tiles=" + tilesFound +
                    ", TilesUsed=" + tilesUsed +
                    ", ZonesCreated=" + zonesCreated +
                    ", RoadRejects=" + roadRejects +
                    ", SmallRejects=" + smallRejects +
                    ", SkippedTiles=" + skippedTiles +
                    ", RoadRects=" + roadRects.Count + ".",
                    this);
            }
        }

        private void OnValidate()
        {
            startDelay = Mathf.Max(0f, startDelay);
            zoneDepth = Mathf.Max(2f, zoneDepth);
            minZoneWidth = Mathf.Max(2f, minZoneWidth);
            outerEdgeInset = Mathf.Max(0f, outerEdgeInset);
            roadClearance = Mathf.Max(0f, roadClearance);
            generatedZoneSidePadding = Mathf.Max(0f, generatedZoneSidePadding);
            generatedZoneEdgeInset = Mathf.Max(0f, generatedZoneEdgeInset);
            complexRoadRendererThreshold = Mathf.Max(0, complexRoadRendererThreshold);
            maxRoadRendererHeight = Mathf.Max(0.01f, maxRoadRendererHeight);
            minRoadRendererSize = Mathf.Max(0.01f, minRoadRendererSize);
        }

        private void ResolveScanRoot()
        {
            if (scanRoot != null)
            {
                return;
            }

            GameObject root =
                GameObject.Find(generatedMapRootName) ??
                GameObject.Find("Generated_Map_Root") ??
                GameObject.Find("Generated_Map") ??
                GameObject.Find("GeneratedMap");

            if (root != null)
            {
                scanRoot = root.transform;
            }
        }

        private void ProcessTile(ZedLegacyRoomTile tile, Transform zoneRoot)
        {
            if (tile == null)
            {
                return;
            }

            if (ShouldSkipTile(tile.gameObject))
            {
                skippedTiles++;
                return;
            }

            Bounds bounds;
            if (!TryCalculateRendererBounds(tile.gameObject, out bounds))
            {
                skippedTiles++;
                return;
            }

            int tileRoadRenderers = CountRoadRenderers(tile.transform);
            if (skipComplexIntersectionTiles && !addZonesToIntersections && tileRoadRenderers >= complexRoadRendererThreshold)
            {
                skippedTiles++;
                return;
            }

            int createdBefore = zonesCreated;

            TryCreateZone(tile.name, zoneRoot, MakeNorthRect(bounds), EdgeSide.North);
            TryCreateZone(tile.name, zoneRoot, MakeSouthRect(bounds), EdgeSide.South);
            TryCreateZone(tile.name, zoneRoot, MakeEastRect(bounds), EdgeSide.East);
            TryCreateZone(tile.name, zoneRoot, MakeWestRect(bounds), EdgeSide.West);

            if (zonesCreated > createdBefore)
            {
                tilesUsed++;
            }
        }

        private bool ShouldSkipTile(GameObject tile)
        {
            if (tile == null)
            {
                return true;
            }

            string lower = tile.name.ToLowerInvariant();
            for (int i = 0; i < skipTileNameContains.Length; i++)
            {
                string term = skipTileNameContains[i];
                if (!string.IsNullOrEmpty(term) && lower.Contains(term.ToLowerInvariant()))
                {
                    return true;
                }
            }

            return false;
        }

        private Rect2 MakeNorthRect(Bounds b)
        {
            return new Rect2
            {
                minX = b.min.x + outerEdgeInset,
                maxX = b.max.x - outerEdgeInset,
                minZ = b.max.z - outerEdgeInset - zoneDepth,
                maxZ = b.max.z - outerEdgeInset
            };
        }

        private Rect2 MakeSouthRect(Bounds b)
        {
            return new Rect2
            {
                minX = b.min.x + outerEdgeInset,
                maxX = b.max.x - outerEdgeInset,
                minZ = b.min.z + outerEdgeInset,
                maxZ = b.min.z + outerEdgeInset + zoneDepth
            };
        }

        private Rect2 MakeEastRect(Bounds b)
        {
            return new Rect2
            {
                minX = b.max.x - outerEdgeInset - zoneDepth,
                maxX = b.max.x - outerEdgeInset,
                minZ = b.min.z + outerEdgeInset,
                maxZ = b.max.z - outerEdgeInset
            };
        }

        private Rect2 MakeWestRect(Bounds b)
        {
            return new Rect2
            {
                minX = b.min.x + outerEdgeInset,
                maxX = b.min.x + outerEdgeInset + zoneDepth,
                minZ = b.min.z + outerEdgeInset,
                maxZ = b.max.z - outerEdgeInset
            };
        }

        private void TryCreateZone(string tileName, Transform zoneRoot, Rect2 rect, EdgeSide side)
        {
            float width = side == EdgeSide.North || side == EdgeSide.South ? rect.Width : rect.Depth;
            float depth = side == EdgeSide.North || side == EdgeSide.South ? rect.Depth : rect.Width;

            if (width < minZoneWidth || depth < 2f)
            {
                smallRejects++;
                rejectedCandidates.Add(new CandidateZone { rect = rect, side = side, tileName = tileName });
                return;
            }

            if (RectOverlapsAnyRoad(rect, roadClearance))
            {
                roadRejects++;
                rejectedCandidates.Add(new CandidateZone { rect = rect, side = side, tileName = tileName });
                return;
            }

            GameObject go = new GameObject("AutoBuildableZone_" + tileName + "_" + side);
            go.transform.SetParent(zoneRoot, false);
            go.transform.position = rect.Center;
            go.transform.rotation = RotationForSide(side);

            ZedBuildableZone zone = go.AddComponent<ZedBuildableZone>();
            zone.width = width;
            zone.depth = depth;
            zone.edgeInset = generatedZoneEdgeInset;
            zone.sidePadding = generatedZoneSidePadding;
            zone.depthAlignment = depthAlignment;
            zone.buildingYawOffset = buildingYawOffset;
            zone.allowAlleys = allowAlleys;

            acceptedCandidates.Add(new CandidateZone { rect = rect, side = side, tileName = tileName });
            zonesCreated++;
        }

        private Quaternion RotationForSide(EdgeSide side)
        {
            switch (side)
            {
                case EdgeSide.North:
                    return Quaternion.Euler(0f, 180f, 0f);
                case EdgeSide.South:
                    return Quaternion.Euler(0f, 0f, 0f);
                case EdgeSide.East:
                    return Quaternion.Euler(0f, 270f, 0f);
                case EdgeSide.West:
                    return Quaternion.Euler(0f, 90f, 0f);
            }

            return Quaternion.identity;
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

                Bounds b = renderer.bounds;
                Rect2 rect = new Rect2
                {
                    minX = b.min.x,
                    maxX = b.max.x,
                    minZ = b.min.z,
                    maxZ = b.max.z
                };

                if (rect.maxX <= rect.minX || rect.maxZ <= rect.minZ)
                {
                    continue;
                }

                roadRects.Add(rect);
            }
        }

        private int CountRoadRenderers(Transform root)
        {
            int count = 0;
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);

            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null && renderers[i].enabled && IsRoadRenderer(renderers[i]))
                {
                    count++;
                }
            }

            return count;
        }

        private bool IsRoadRenderer(Renderer renderer)
        {
            Bounds b = renderer.bounds;

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
            int safety = 8;
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

        private Transform GetOrCreateZoneRoot()
        {
            GameObject root = GameObject.Find(generatedZoneRootName);
            if (root == null)
            {
                root = new GameObject(generatedZoneRootName);
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
            if (!drawCandidateGizmos)
            {
                return;
            }

            Gizmos.color = acceptedZoneColor;
            for (int i = 0; i < acceptedCandidates.Count; i++)
            {
                DrawRect(acceptedCandidates[i].rect);
            }

            Gizmos.color = rejectedZoneColor;
            for (int i = 0; i < rejectedCandidates.Count; i++)
            {
                DrawRect(rejectedCandidates[i].rect);
            }
        }

        private static void DrawRect(Rect2 rect)
        {
            Vector3 center = rect.Center;
            center.y = 0.2f;
            Gizmos.DrawWireCube(center, new Vector3(rect.Width, 0.1f, rect.Depth));
        }
    }
}

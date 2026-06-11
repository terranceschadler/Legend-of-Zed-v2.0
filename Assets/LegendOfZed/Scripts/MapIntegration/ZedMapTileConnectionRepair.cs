using System.Collections;
using System.Collections.Generic;
using LegendOfZed.LegacyMapGenerator;
using UnityEngine;

namespace LegendOfZed.MapIntegration
{
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-25)]
    public sealed class ZedMapTileConnectionRepair : MonoBehaviour
    {
        [Header("Timing")]
        public bool repairOnStart = true;
        [Min(0f)] public float startTimeout = 10f;
        [Min(0)] public int stableFramesRequired = 12;

        [Tooltip("Do not run repair until at least this many generated tiles exist. Prevents repair from firing while the legacy generator has only spawned a few starter tiles.")]
        [Min(1)] public int minimumTilesBeforeRepair = 20;

        [Tooltip("Small delay after the tile count first becomes stable before repair executes.")]
        [Min(0f)] public float postStableDelay = 0.2f;

        [Header("Scan")]
        public Transform generatedTilesRoot;
        [Min(1f)] public float tileSize = 50f;
        [Min(0.1f)] public float roadOffsetThreshold = 8f;
        [Min(0.1f)] public float positionTolerance = 4f;

        [Header("Road Edge Audit")]
        [Tooltip("Use global road renderer geometry crossing tile edges to decide connections. This is the strongest repair mode.")]
        public bool useGlobalRoadGeometryConnections = true;

        [Tooltip("Use actual road children near tile edges to decide tile connections instead of only neighbor tile presence.")]
        public bool useActualRoadEdgeOpenings = true;

        [Tooltip("How close a road child must be to a tile edge to count as an open connection.")]
        [Min(0.5f)] public float roadEdgeOpenThreshold = 18f;

        [Tooltip("Require the neighbor tile to have a road opening back toward this tile before counting a connection.")]
        public bool requireNeighborRoadBackLink = false;

        [Tooltip("Use neighbor road openings pointing back into this tile to add missing connections. This catches 3-way tiles that should be 4-way.")]
        public bool useNeighborBacklinksToAddMissingConnections = true;

        [Tooltip("If actual road edge detection finds no openings, fall back to tile-neighbor presence.")]
        public bool fallbackToNeighborPresence = true;

        [Tooltip("Thickness of the edge probe used to detect road geometry crossing a tile edge.")]
        [Min(0.5f)] public float globalRoadEdgeProbeThickness = 8f;

        [Tooltip("Inset from tile corners for edge probes, preventing corner/intersection bleed.")]
        [Min(0f)] public float globalRoadEdgeProbeCornerInset = 6f;

        [Tooltip("Minimum road renderer overlap area with an edge probe to count as a real connection.")]
        [Min(0.01f)] public float minGlobalRoadEdgeOverlapArea = 4f;

        [Header("Tile Prefabs")]
        public List<GameObject> roomTilePrefabs = new List<GameObject>();

        [Header("Rules")]
        public bool replaceWrongConnectionTiles = true;
        public bool keepOriginalName = false;

        [Header("Debug")]
        public bool logSummary = true;

        private readonly Dictionary<Vector2Int, TileInfo> tilesByGrid = new Dictionary<Vector2Int, TileInfo>();
        private readonly List<TileInfo> tiles = new List<TileInfo>();
        private readonly List<PrefabOption> prefabOptions = new List<PrefabOption>();
        private readonly List<RoadRect> globalRoadRects = new List<RoadRect>();

        private int tilesScanned;
        private int tilesReplaced;
        private int noPrefabMatches;
        private int alreadyCorrect;
        private int emptyNeighborRejects;
        private int roadEdgeMasksUsed;
        private int neighborPresenceMasksUsed;
        private int backlinkRejects;
        private int backlinkAddedConnections;
        private int globalRoadRectsScanned;
        private int globalRoadMasksUsed;

        private enum Direction
        {
            North,
            East,
            South,
            West
        }

        private struct TileInfo
        {
            public ZedLegacyRoomTile tile;
            public Vector2Int grid;
            public int currentMask;
            public int desiredMask;
        }

        private struct PrefabOption
        {
            public GameObject prefab;
            public int baseMask;
        }

        private struct RoadRect
        {
            public float minX;
            public float maxX;
            public float minZ;
            public float maxZ;

            public float Width
            {
                get { return Mathf.Max(0f, maxX - minX); }
            }

            public float Depth
            {
                get { return Mathf.Max(0f, maxZ - minZ); }
            }
        }

        private void OnValidate()
        {
            tileSize = Mathf.Max(1f, tileSize);
            roadOffsetThreshold = Mathf.Max(0.1f, roadOffsetThreshold);
            positionTolerance = Mathf.Max(0.1f, positionTolerance);
            roadEdgeOpenThreshold = Mathf.Max(0.5f, roadEdgeOpenThreshold);
            globalRoadEdgeProbeThickness = Mathf.Max(0.5f, globalRoadEdgeProbeThickness);
            globalRoadEdgeProbeCornerInset = Mathf.Max(0f, globalRoadEdgeProbeCornerInset);
            minGlobalRoadEdgeOverlapArea = Mathf.Max(0.01f, minGlobalRoadEdgeOverlapArea);
            startTimeout = Mathf.Max(0f, startTimeout);
            stableFramesRequired = Mathf.Max(0, stableFramesRequired);
            minimumTilesBeforeRepair = Mathf.Max(1, minimumTilesBeforeRepair);
            postStableDelay = Mathf.Max(0f, postStableDelay);
        }

        private void Start()
        {
            if (repairOnStart)
            {
                StartCoroutine(RepairWhenGenerated());
            }
        }

        private IEnumerator RepairWhenGenerated()
        {
            float startTime = Time.realtimeSinceStartup;
            int previousCount = -1;
            int stableFrames = 0;

            while (Time.realtimeSinceStartup - startTime <= startTimeout)
            {
                int count = CountGeneratedTiles();

                if (count >= minimumTilesBeforeRepair && count == previousCount)
                {
                    stableFrames++;
                    if (stableFrames >= stableFramesRequired)
                    {
                        break;
                    }
                }
                else
                {
                    stableFrames = 0;
                    previousCount = count;
                }

                yield return null;
            }

            if (postStableDelay > 0f)
            {
                yield return new WaitForSeconds(postStableDelay);
            }

            RepairNow();
        }

        [ContextMenu("Repair Map Tile Connections Now")]
        public void RepairNow()
        {
            ResetStats();

            ResolveGeneratedTilesRoot();
            BuildPrefabOptions();
            BuildTileIndex();
            BuildGlobalRoadRects();

            if (tiles.Count == 0 || prefabOptions.Count == 0)
            {
                LogSummary();
                return;
            }

            for (int i = 0; i < tiles.Count; i++)
            {
                TileInfo info = tiles[i];
                if (info.tile == null)
                {
                    continue;
                }

                int desiredMask = CalculateDesiredMask(info);
                if (desiredMask == 0)
                {
                    emptyNeighborRejects++;
                    continue;
                }

                info.desiredMask = desiredMask;

                if (info.currentMask == desiredMask)
                {
                    alreadyCorrect++;
                    continue;
                }

                if (!replaceWrongConnectionTiles)
                {
                    continue;
                }

                GameObject prefab;
                Quaternion rotation;
                if (!TryFindPrefabForMask(desiredMask, out prefab, out rotation))
                {
                    noPrefabMatches++;
                    continue;
                }

                ReplaceTile(info.tile, prefab, rotation, desiredMask);
            }

            LogSummary();
        }

        private void ResetStats()
        {
            tilesScanned = 0;
            tilesReplaced = 0;
            noPrefabMatches = 0;
            alreadyCorrect = 0;
            emptyNeighborRejects = 0;
            roadEdgeMasksUsed = 0;
            neighborPresenceMasksUsed = 0;
            backlinkRejects = 0;
            backlinkAddedConnections = 0;
            globalRoadRectsScanned = 0;
            globalRoadMasksUsed = 0;
            globalRoadRects.Clear();
            tiles.Clear();
            tilesByGrid.Clear();
            prefabOptions.Clear();
        }

        private int CountGeneratedTiles()
        {
            ResolveGeneratedTilesRoot();
            if (generatedTilesRoot != null)
            {
                return generatedTilesRoot.GetComponentsInChildren<ZedLegacyRoomTile>(true).Length;
            }

            return FindObjectsByType<ZedLegacyRoomTile>(FindObjectsInactive.Exclude).Length;
        }

        private void ResolveGeneratedTilesRoot()
        {
            if (generatedTilesRoot != null)
            {
                return;
            }

            GameObject namedRoot = GameObject.Find("Generated_Map_Tiles");
            if (namedRoot != null)
            {
                generatedTilesRoot = namedRoot.transform;
                return;
            }

            GameObject generatedRoot = GameObject.Find("Generated");
            if (generatedRoot != null)
            {
                generatedTilesRoot = generatedRoot.transform;
            }
        }

        private void BuildPrefabOptions()
        {
            prefabOptions.Clear();

            for (int i = 0; i < roomTilePrefabs.Count; i++)
            {
                GameObject prefab = roomTilePrefabs[i];
                if (prefab == null)
                {
                    continue;
                }

                int mask = DetectLocalRoadMask(prefab.transform);
                if (mask == 0)
                {
                    mask = InferMaskFromName(prefab.name);
                }

                if (mask == 0)
                {
                    continue;
                }

                prefabOptions.Add(new PrefabOption
                {
                    prefab = prefab,
                    baseMask = mask
                });
            }
        }

        private void BuildTileIndex()
        {
            ZedLegacyRoomTile[] found;
            if (generatedTilesRoot != null)
            {
                found = generatedTilesRoot.GetComponentsInChildren<ZedLegacyRoomTile>(true);
            }
            else
            {
                found = FindObjectsByType<ZedLegacyRoomTile>(FindObjectsInactive.Exclude);
            }

            for (int i = 0; i < found.Length; i++)
            {
                ZedLegacyRoomTile tile = found[i];
                if (tile == null || IsPrefabAssetLike(tile.gameObject))
                {
                    continue;
                }

                Vector2Int grid = WorldToGrid(tile.transform.position);
                int currentMask = DetectWorldRoadMask(tile.transform);

                TileInfo info = new TileInfo
                {
                    tile = tile,
                    grid = grid,
                    currentMask = currentMask,
                    desiredMask = 0
                };

                tiles.Add(info);
                tilesByGrid[grid] = info;
                tilesScanned++;
            }
        }

        private bool IsPrefabAssetLike(GameObject go)
        {
            return go == null || !go.scene.IsValid();
        }

        private Vector2Int WorldToGrid(Vector3 position)
        {
            return new Vector2Int(
                Mathf.RoundToInt(position.x / Mathf.Max(1f, tileSize)),
                Mathf.RoundToInt(position.z / Mathf.Max(1f, tileSize)));
        }

        private int CalculateDesiredMask(TileInfo info)
        {
            if (useGlobalRoadGeometryConnections)
            {
                int globalMask = CalculateGlobalRoadGeometryMask(info);
                if (globalMask != 0)
                {
                    globalRoadMasksUsed++;
                    return globalMask;
                }
            }

            if (useActualRoadEdgeOpenings)
            {
                int roadMask = CalculateRoadEdgeBacklinkedMask(info);
                if (roadMask != 0)
                {
                    roadEdgeMasksUsed++;
                    return roadMask;
                }
            }

            if (fallbackToNeighborPresence)
            {
                neighborPresenceMasksUsed++;
                return CalculateNeighborPresenceMask(info.grid);
            }

            return 0;
        }

        private void BuildGlobalRoadRects()
        {
            globalRoadRects.Clear();

            Transform searchRoot = generatedTilesRoot;
            if (searchRoot == null)
            {
                ResolveGeneratedTilesRoot();
                searchRoot = generatedTilesRoot;
            }

            Renderer[] renderers = searchRoot != null
                ? searchRoot.GetComponentsInChildren<Renderer>(true)
                : FindObjectsByType<Renderer>(FindObjectsInactive.Exclude);

            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null || !IsRoadRenderer(renderer))
                {
                    continue;
                }

                Bounds b = renderer.bounds;
                RoadRect rect = new RoadRect
                {
                    minX = b.min.x,
                    maxX = b.max.x,
                    minZ = b.min.z,
                    maxZ = b.max.z
                };

                if (rect.Width <= 0.1f || rect.Depth <= 0.1f)
                {
                    continue;
                }

                globalRoadRects.Add(rect);
            }

            globalRoadRectsScanned = globalRoadRects.Count;
        }

        private bool IsRoadRenderer(Renderer renderer)
        {
            if (renderer == null)
            {
                return false;
            }

            string hierarchyName = GetHierarchyName(renderer.transform).ToLowerInvariant();
            if (!hierarchyName.Contains("road"))
            {
                return false;
            }

            if (hierarchyName.Contains("sidewalk") ||
                hierarchyName.Contains("walk") ||
                hierarchyName.Contains("curb") ||
                hierarchyName.Contains("kerb") ||
                hierarchyName.Contains("wall") ||
                hierarchyName.Contains("building") ||
                hierarchyName.Contains("prop") ||
                hierarchyName.Contains("tree") ||
                hierarchyName.Contains("grass") ||
                hierarchyName.Contains("park"))
            {
                return false;
            }

            return true;
        }

        private string GetHierarchyName(Transform transform)
        {
            if (transform == null)
            {
                return string.Empty;
            }

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            Transform current = transform;
            int guard = 0;
            while (current != null && guard < 12)
            {
                if (sb.Length > 0)
                {
                    sb.Insert(0, "/");
                }

                sb.Insert(0, current.name);
                current = current.parent;
                guard++;
            }

            return sb.ToString();
        }

        private int CalculateGlobalRoadGeometryMask(TileInfo info)
        {
            Vector3 center = info.tile != null ? info.tile.transform.position : new Vector3(info.grid.x * tileSize, 0f, info.grid.y * tileSize);
            float half = Mathf.Max(1f, tileSize * 0.5f);
            float thickness = Mathf.Max(0.5f, globalRoadEdgeProbeThickness);
            float cornerInset = Mathf.Clamp(globalRoadEdgeProbeCornerInset, 0f, half - 0.5f);

            int mask = 0;

            RoadRect northProbe = new RoadRect
            {
                minX = center.x - half + cornerInset,
                maxX = center.x + half - cornerInset,
                minZ = center.z + half - thickness * 0.5f,
                maxZ = center.z + half + thickness * 0.5f
            };

            RoadRect eastProbe = new RoadRect
            {
                minX = center.x + half - thickness * 0.5f,
                maxX = center.x + half + thickness * 0.5f,
                minZ = center.z - half + cornerInset,
                maxZ = center.z + half - cornerInset
            };

            RoadRect southProbe = new RoadRect
            {
                minX = center.x - half + cornerInset,
                maxX = center.x + half - cornerInset,
                minZ = center.z - half - thickness * 0.5f,
                maxZ = center.z - half + thickness * 0.5f
            };

            RoadRect westProbe = new RoadRect
            {
                minX = center.x - half - thickness * 0.5f,
                maxX = center.x - half + thickness * 0.5f,
                minZ = center.z - half + cornerInset,
                maxZ = center.z + half - cornerInset
            };

            if (AnyRoadOverlapsProbe(northProbe)) mask |= Mask(Direction.North);
            if (AnyRoadOverlapsProbe(eastProbe)) mask |= Mask(Direction.East);
            if (AnyRoadOverlapsProbe(southProbe)) mask |= Mask(Direction.South);
            if (AnyRoadOverlapsProbe(westProbe)) mask |= Mask(Direction.West);

            return mask;
        }

        private bool AnyRoadOverlapsProbe(RoadRect probe)
        {
            for (int i = 0; i < globalRoadRects.Count; i++)
            {
                if (OverlapArea(globalRoadRects[i], probe) >= minGlobalRoadEdgeOverlapArea)
                {
                    return true;
                }
            }

            return false;
        }

        private float OverlapArea(RoadRect a, RoadRect b)
        {
            float overlapX = Mathf.Min(a.maxX, b.maxX) - Mathf.Max(a.minX, b.minX);
            float overlapZ = Mathf.Min(a.maxZ, b.maxZ) - Mathf.Max(a.minZ, b.minZ);

            if (overlapX <= 0f || overlapZ <= 0f)
            {
                return 0f;
            }

            return overlapX * overlapZ;
        }

        private int CalculateRoadEdgeBacklinkedMask(TileInfo info)
        {
            int mask = info.currentMask;

            if (useNeighborBacklinksToAddMissingConnections)
            {
                int addedMask = CalculateNeighborBacklinkMask(info.grid);
                int before = mask;
                mask |= addedMask;
                backlinkAddedConnections += CountBits(mask & ~before);
            }

            if (mask == 0)
            {
                return 0;
            }

            if (!requireNeighborRoadBackLink)
            {
                return mask;
            }

            int linkedMask = 0;

            if ((mask & Mask(Direction.North)) != 0 && NeighborHasBackRoad(info.grid, Direction.North))
            {
                linkedMask |= Mask(Direction.North);
            }
            else if ((mask & Mask(Direction.North)) != 0)
            {
                backlinkRejects++;
            }

            if ((mask & Mask(Direction.East)) != 0 && NeighborHasBackRoad(info.grid, Direction.East))
            {
                linkedMask |= Mask(Direction.East);
            }
            else if ((mask & Mask(Direction.East)) != 0)
            {
                backlinkRejects++;
            }

            if ((mask & Mask(Direction.South)) != 0 && NeighborHasBackRoad(info.grid, Direction.South))
            {
                linkedMask |= Mask(Direction.South);
            }
            else if ((mask & Mask(Direction.South)) != 0)
            {
                backlinkRejects++;
            }

            if ((mask & Mask(Direction.West)) != 0 && NeighborHasBackRoad(info.grid, Direction.West))
            {
                linkedMask |= Mask(Direction.West);
            }
            else if ((mask & Mask(Direction.West)) != 0)
            {
                backlinkRejects++;
            }

            return linkedMask;
        }

        private int CalculateNeighborBacklinkMask(Vector2Int grid)
        {
            int mask = 0;

            TileInfo north;
            if (tilesByGrid.TryGetValue(grid + Vector2Int.up, out north) &&
                (north.currentMask & Mask(Direction.South)) != 0)
            {
                mask |= Mask(Direction.North);
            }

            TileInfo east;
            if (tilesByGrid.TryGetValue(grid + Vector2Int.right, out east) &&
                (east.currentMask & Mask(Direction.West)) != 0)
            {
                mask |= Mask(Direction.East);
            }

            TileInfo south;
            if (tilesByGrid.TryGetValue(grid + Vector2Int.down, out south) &&
                (south.currentMask & Mask(Direction.North)) != 0)
            {
                mask |= Mask(Direction.South);
            }

            TileInfo west;
            if (tilesByGrid.TryGetValue(grid + Vector2Int.left, out west) &&
                (west.currentMask & Mask(Direction.East)) != 0)
            {
                mask |= Mask(Direction.West);
            }

            return mask;
        }

        private int CountBits(int mask)
        {
            int count = 0;
            while (mask != 0)
            {
                count += mask & 1;
                mask >>= 1;
            }

            return count;
        }

        private bool NeighborHasBackRoad(Vector2Int grid, Direction direction)
        {
            Vector2Int neighborGrid = grid + DirectionOffset(direction);
            TileInfo neighbor;
            if (!tilesByGrid.TryGetValue(neighborGrid, out neighbor))
            {
                return false;
            }

            Direction opposite = Opposite(direction);
            return (neighbor.currentMask & Mask(opposite)) != 0;
        }

        private Vector2Int DirectionOffset(Direction direction)
        {
            switch (direction)
            {
                case Direction.North:
                    return Vector2Int.up;
                case Direction.East:
                    return Vector2Int.right;
                case Direction.South:
                    return Vector2Int.down;
                case Direction.West:
                    return Vector2Int.left;
                default:
                    return Vector2Int.zero;
            }
        }

        private Direction Opposite(Direction direction)
        {
            switch (direction)
            {
                case Direction.North:
                    return Direction.South;
                case Direction.East:
                    return Direction.West;
                case Direction.South:
                    return Direction.North;
                case Direction.West:
                    return Direction.East;
                default:
                    return Direction.North;
            }
        }

        private int CalculateNeighborPresenceMask(Vector2Int grid)
        {
            int mask = 0;

            if (tilesByGrid.ContainsKey(grid + Vector2Int.up))
            {
                mask |= Mask(Direction.North);
            }

            if (tilesByGrid.ContainsKey(grid + Vector2Int.right))
            {
                mask |= Mask(Direction.East);
            }

            if (tilesByGrid.ContainsKey(grid + Vector2Int.down))
            {
                mask |= Mask(Direction.South);
            }

            if (tilesByGrid.ContainsKey(grid + Vector2Int.left))
            {
                mask |= Mask(Direction.West);
            }

            return mask;
        }

        private void AddRoadEdgeMaskFromOffset(Vector3 offset, ref int mask)
        {
            float half = Mathf.Max(1f, tileSize * 0.5f);
            float threshold = Mathf.Clamp(roadEdgeOpenThreshold, 0.5f, half);

            if (offset.z >= half - threshold)
            {
                mask |= Mask(Direction.North);
            }

            if (offset.x >= half - threshold)
            {
                mask |= Mask(Direction.East);
            }

            if (offset.z <= -half + threshold)
            {
                mask |= Mask(Direction.South);
            }

            if (offset.x <= -half + threshold)
            {
                mask |= Mask(Direction.West);
            }
        }

        private int DetectWorldRoadMask(Transform root)
        {
            if (root == null)
            {
                return 0;
            }

            int mask = 0;
            Transform[] children = root.GetComponentsInChildren<Transform>(true);
            Vector3 center = root.position;

            for (int i = 0; i < children.Length; i++)
            {
                Transform child = children[i];
                if (child == null || child == root)
                {
                    continue;
                }

                string n = child.name.ToLowerInvariant();
                if (!n.Contains("road") || n.Contains("center"))
                {
                    continue;
                }

                Vector3 delta = child.position - center;
                AddRoadEdgeMaskFromOffset(delta, ref mask);
            }

            if (mask == 0)
            {
                mask = InferMaskFromName(root.name);
            }

            return mask;
        }

        private int DetectLocalRoadMask(Transform root)
        {
            if (root == null)
            {
                return 0;
            }

            int mask = 0;
            Transform[] children = root.GetComponentsInChildren<Transform>(true);

            for (int i = 0; i < children.Length; i++)
            {
                Transform child = children[i];
                if (child == null || child == root)
                {
                    continue;
                }

                string n = child.name.ToLowerInvariant();
                if (!n.Contains("road") || n.Contains("center"))
                {
                    continue;
                }

                Vector3 local = root.InverseTransformPoint(child.position);
                AddRoadEdgeMaskFromOffset(local, ref mask);
            }

            return mask;
        }

        private int InferMaskFromName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return 0;
            }

            string lower = name.ToLowerInvariant();

            if (lower.Contains("4way") || lower.Contains("4-way"))
            {
                return Mask(Direction.North) | Mask(Direction.East) | Mask(Direction.South) | Mask(Direction.West);
            }

            if (lower.Contains("deadend") || lower.Contains("dead-end"))
            {
                return Mask(Direction.North);
            }

            if (lower.Contains("straight"))
            {
                return Mask(Direction.North) | Mask(Direction.South);
            }

            if (lower.Contains("left") || lower.Contains("right") || lower.Contains("corner"))
            {
                return Mask(Direction.North) | Mask(Direction.East);
            }

            return 0;
        }

        private bool TryFindPrefabForMask(int desiredMask, out GameObject prefab, out Quaternion rotation)
        {
            prefab = null;
            rotation = Quaternion.identity;

            for (int i = 0; i < prefabOptions.Count; i++)
            {
                PrefabOption option = prefabOptions[i];

                for (int turn = 0; turn < 4; turn++)
                {
                    int rotated = RotateMaskClockwise(option.baseMask, turn);
                    if (rotated == desiredMask)
                    {
                        prefab = option.prefab;
                        rotation = Quaternion.Euler(0f, turn * 90f, 0f);
                        return true;
                    }
                }
            }

            return false;
        }

        private void ReplaceTile(ZedLegacyRoomTile oldTile, GameObject prefab, Quaternion rotation, int desiredMask)
        {
            if (oldTile == null || prefab == null)
            {
                return;
            }

            Transform oldTransform = oldTile.transform;
            Transform parent = oldTransform.parent;
            Vector3 position = oldTransform.position;
            string oldName = oldTransform.name;
            int siblingIndex = oldTransform.GetSiblingIndex();

            GameObject instance = Instantiate(prefab, position, rotation, parent);
            instance.name = keepOriginalName ? oldName : prefab.name + "(ConnectionRepaired)";
            instance.transform.SetSiblingIndex(siblingIndex);

            Destroy(oldTile.gameObject);
            tilesReplaced++;
        }

        private int RotateMaskClockwise(int mask, int quarterTurns)
        {
            quarterTurns = ((quarterTurns % 4) + 4) % 4;

            int result = mask;
            for (int i = 0; i < quarterTurns; i++)
            {
                int next = 0;

                if ((result & Mask(Direction.North)) != 0) next |= Mask(Direction.East);
                if ((result & Mask(Direction.East)) != 0) next |= Mask(Direction.South);
                if ((result & Mask(Direction.South)) != 0) next |= Mask(Direction.West);
                if ((result & Mask(Direction.West)) != 0) next |= Mask(Direction.North);

                result = next;
            }

            return result;
        }

        private int Mask(Direction direction)
        {
            switch (direction)
            {
                case Direction.North:
                    return 1;
                case Direction.East:
                    return 2;
                case Direction.South:
                    return 4;
                case Direction.West:
                    return 8;
                default:
                    return 0;
            }
        }

        private string MaskToString(int mask)
        {
            List<string> parts = new List<string>();

            if ((mask & Mask(Direction.North)) != 0) parts.Add("N");
            if ((mask & Mask(Direction.East)) != 0) parts.Add("E");
            if ((mask & Mask(Direction.South)) != 0) parts.Add("S");
            if ((mask & Mask(Direction.West)) != 0) parts.Add("W");

            return parts.Count == 0 ? "None" : string.Join("", parts);
        }

        private void LogSummary()
        {
            if (!logSummary)
            {
                return;
            }

            Debug.Log(
                "V3.9J4 TIMED ROAD GEOMETRY CONNECTION REPAIR complete. " +
                "TilesScanned=" + tilesScanned +
                ", MinimumTilesBeforeRepair=" + minimumTilesBeforeRepair +
                ", PrefabOptions=" + prefabOptions.Count +
                ", AlreadyCorrect=" + alreadyCorrect +
                ", TilesReplaced=" + tilesReplaced +
                ", NoPrefabMatches=" + noPrefabMatches +
                ", EmptyNeighborRejects=" + emptyNeighborRejects +
                ", GlobalRoadRects=" + globalRoadRectsScanned +
                ", GlobalRoadMasksUsed=" + globalRoadMasksUsed +
                ", RoadEdgeMasksUsed=" + roadEdgeMasksUsed +
                ", NeighborPresenceMasksUsed=" + neighborPresenceMasksUsed +
                ", BacklinkRejects=" + backlinkRejects +
                ", BacklinkAddedConnections=" + backlinkAddedConnections + ".",
                this);
        }
    }
}

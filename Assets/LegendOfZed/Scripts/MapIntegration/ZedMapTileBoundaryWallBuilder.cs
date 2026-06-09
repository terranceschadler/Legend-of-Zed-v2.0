using System.Collections;
using System.Collections.Generic;
using LegendOfZed.LegacyMapGenerator;
using UnityEngine;

namespace LegendOfZed.MapIntegration
{
    public class ZedMapTileBoundaryWallBuilder : MonoBehaviour
    {
        [Header("Generator")]
        public ZedLegacyRandomMapGenerator generator;

        [Header("Footprint Perimeter Wall")]
        public string wallRootName = "Generated_Map_Boundary_Walls";
        public float wallHeight = 5f;
        public float wallThickness = 1.5f;
        public float yCenter = 2.5f;
        public bool addVisibleWallMesh = true;
        public bool rebuildAutomatically = true;

        [Header("Tile Footprint Detection")]
        [Tooltip("Only low renderers can be selected as a tile footprint. This prevents buildings/props from expanding the perimeter.")]
        public float maxFootprintCenterY = 1.25f;

        [Tooltip("Ignore small props/deco when selecting the tile footprint renderer.")]
        public float minFootprintSize = 8f;

        [Tooltip("Small coordinate tolerance used when merging rectangle edges.")]
        public float coordinateTolerance = 0.05f;

        [Header("Debug")]
        public bool logDetails;

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
            public int outsideSign;
        }

        private IEnumerator Start()
        {
            if (!rebuildAutomatically)
            {
                yield break;
            }

            if (generator == null)
            {
                generator = FindAnyObjectByType<ZedLegacyRandomMapGenerator>();
            }

            float timeoutAt = Time.realtimeSinceStartup + 10f;
            while (generator != null && !generator.bakingNavMeshCompleted)
            {
                if (Time.realtimeSinceStartup > timeoutAt)
                {
                    break;
                }

                yield return null;
            }

            yield return null;

            RebuildBoundaryWalls();
        }

        [ContextMenu("Rebuild Boundary Walls")]
        public void RebuildBoundaryWalls()
        {
            List<Rect2> footprints = CollectTileFootprints();
            if (footprints.Count == 0)
            {
                Debug.LogWarning("Boundary wall builder could not find generated tile floor footprints.", this);
                return;
            }

            List<Edge> perimeterEdges = BuildUnionPerimeterEdges(footprints);
            perimeterEdges = MergeCollinearEdges(perimeterEdges);

            GameObject oldRoot = GameObject.Find(wallRootName);
            if (oldRoot != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(oldRoot);
                }
                else
                {
                    DestroyImmediate(oldRoot);
                }
            }

            GameObject root = new GameObject(wallRootName);

            for (int i = 0; i < perimeterEdges.Count; i++)
            {
                EmitWall(root.transform, perimeterEdges[i], i);
            }

            Debug.Log("Generated floor-footprint perimeter boundary walls built. Segments=" + perimeterEdges.Count + ". Tile footprints=" + footprints.Count + ".", root);

            if (logDetails)
            {
                Debug.Log("Footprint perimeter builder used " + footprints.Count + " tile footprint rectangle(s).", root);
            }
        }

        private List<Rect2> CollectTileFootprints()
        {
            List<GameObject> tileRoots = new List<GameObject>();

            ZedLegacyRoomTile[] roomTiles = FindObjectsByType<ZedLegacyRoomTile>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            for (int i = 0; i < roomTiles.Length; i++)
            {
                if (roomTiles[i] != null && !tileRoots.Contains(roomTiles[i].gameObject))
                {
                    tileRoots.Add(roomTiles[i].gameObject);
                }
            }

            GameObject[] taggedTiles = GameObject.FindGameObjectsWithTag("RoomTile");
            for (int i = 0; i < taggedTiles.Length; i++)
            {
                if (taggedTiles[i] != null && !tileRoots.Contains(taggedTiles[i]))
                {
                    tileRoots.Add(taggedTiles[i]);
                }
            }

            List<Rect2> footprints = new List<Rect2>();

            for (int i = 0; i < tileRoots.Count; i++)
            {
                Renderer best = FindBestFootprintRenderer(tileRoots[i]);
                if (best == null)
                {
                    continue;
                }

                Bounds b = best.bounds;
                footprints.Add(new Rect2
                {
                    minX = b.min.x,
                    maxX = b.max.x,
                    minZ = b.min.z,
                    maxZ = b.max.z
                });

                if (logDetails)
                {
                    Debug.Log("Boundary footprint renderer for " + tileRoots[i].name + ": " + GetHierarchyPath(best.transform) + " bounds=" + b, best);
                }
            }

            return footprints;
        }

        private Renderer FindBestFootprintRenderer(GameObject tileRoot)
        {
            if (tileRoot == null)
            {
                return null;
            }

            Renderer[] renderers = tileRoot.GetComponentsInChildren<Renderer>(true);
            Renderer best = null;
            float bestScore = 0f;

            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null || !renderer.enabled)
                {
                    continue;
                }

                Bounds b = renderer.bounds;

                if (b.center.y > maxFootprintCenterY)
                {
                    continue;
                }

                float sizeX = b.size.x;
                float sizeZ = b.size.z;

                if (sizeX < minFootprintSize || sizeZ < minFootprintSize)
                {
                    continue;
                }

                float area = sizeX * sizeZ;
                float flatnessBonus = b.size.y <= 1.5f ? 100000f : 0f;
                float nameBonus = LooksLikeFootprintName(renderer.transform) ? 50000f : 0f;
                float score = area + flatnessBonus + nameBonus;

                if (score > bestScore)
                {
                    bestScore = score;
                    best = renderer;
                }
            }

            return best;
        }

        private bool LooksLikeFootprintName(Transform transform)
        {
            if (transform == null)
            {
                return false;
            }

            string name = transform.name.ToLowerInvariant();
            string parent = transform.parent != null ? transform.parent.name.ToLowerInvariant() : string.Empty;

            return name.Contains("floor") || parent.Contains("floor") ||
                   name.Contains("ground") || parent.Contains("ground") ||
                   name.Contains("sidewalk") || parent.Contains("sidewalk") ||
                   name.Contains("road") || parent.Contains("road") ||
                   name.Contains("tile") || parent.Contains("tile") ||
                   name.Contains("pavement") || parent.Contains("pavement");
        }

        private List<Edge> BuildUnionPerimeterEdges(List<Rect2> rectangles)
        {
            List<float> xs = new List<float>();
            List<float> zs = new List<float>();

            for (int i = 0; i < rectangles.Count; i++)
            {
                AddUnique(xs, rectangles[i].minX);
                AddUnique(xs, rectangles[i].maxX);
                AddUnique(zs, rectangles[i].minZ);
                AddUnique(zs, rectangles[i].maxZ);
            }

            xs.Sort();
            zs.Sort();

            int width = xs.Count - 1;
            int height = zs.Count - 1;
            bool[,] filled = new bool[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < height; z++)
                {
                    float cx = (xs[x] + xs[x + 1]) * 0.5f;
                    float cz = (zs[z] + zs[z + 1]) * 0.5f;

                    for (int r = 0; r < rectangles.Count; r++)
                    {
                        Rect2 rect = rectangles[r];
                        if (cx >= rect.minX - coordinateTolerance && cx <= rect.maxX + coordinateTolerance &&
                            cz >= rect.minZ - coordinateTolerance && cz <= rect.maxZ + coordinateTolerance)
                        {
                            filled[x, z] = true;
                            break;
                        }
                    }
                }
            }

            bool[,] outside = FloodOutside(filled, width, height);

            List<Edge> edges = new List<Edge>();

            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < height; z++)
                {
                    if (!filled[x, z])
                    {
                        continue;
                    }

                    // North edge.
                    if (z + 1 >= height || outside[x, z + 1])
                    {
                        edges.Add(new Edge
                        {
                            horizontal = true,
                            fixedCoord = zs[z + 1],
                            start = xs[x],
                            end = xs[x + 1],
                            outsideSign = 1
                        });
                    }

                    // South edge.
                    if (z - 1 < 0 || outside[x, z - 1])
                    {
                        edges.Add(new Edge
                        {
                            horizontal = true,
                            fixedCoord = zs[z],
                            start = xs[x],
                            end = xs[x + 1],
                            outsideSign = -1
                        });
                    }

                    // East edge.
                    if (x + 1 >= width || outside[x + 1, z])
                    {
                        edges.Add(new Edge
                        {
                            horizontal = false,
                            fixedCoord = xs[x + 1],
                            start = zs[z],
                            end = zs[z + 1],
                            outsideSign = 1
                        });
                    }

                    // West edge.
                    if (x - 1 < 0 || outside[x - 1, z])
                    {
                        edges.Add(new Edge
                        {
                            horizontal = false,
                            fixedCoord = xs[x],
                            start = zs[z],
                            end = zs[z + 1],
                            outsideSign = -1
                        });
                    }
                }
            }

            return edges;
        }

        private bool[,] FloodOutside(bool[,] filled, int width, int height)
        {
            bool[,] outside = new bool[width, height];
            Queue<Vector2Int> queue = new Queue<Vector2Int>();

            for (int x = 0; x < width; x++)
            {
                EnqueueOutsideCell(x, 0, filled, outside, queue, width, height);
                EnqueueOutsideCell(x, height - 1, filled, outside, queue, width, height);
            }

            for (int z = 0; z < height; z++)
            {
                EnqueueOutsideCell(0, z, filled, outside, queue, width, height);
                EnqueueOutsideCell(width - 1, z, filled, outside, queue, width, height);
            }

            Vector2Int[] dirs =
            {
                new Vector2Int(0, 1),
                new Vector2Int(1, 0),
                new Vector2Int(0, -1),
                new Vector2Int(-1, 0)
            };

            while (queue.Count > 0)
            {
                Vector2Int current = queue.Dequeue();

                for (int i = 0; i < dirs.Length; i++)
                {
                    Vector2Int next = current + dirs[i];
                    EnqueueOutsideCell(next.x, next.y, filled, outside, queue, width, height);
                }
            }

            return outside;
        }

        private void EnqueueOutsideCell(int x, int z, bool[,] filled, bool[,] outside, Queue<Vector2Int> queue, int width, int height)
        {
            if (x < 0 || x >= width || z < 0 || z >= height)
            {
                return;
            }

            if (filled[x, z] || outside[x, z])
            {
                return;
            }

            outside[x, z] = true;
            queue.Enqueue(new Vector2Int(x, z));
        }

        private List<Edge> MergeCollinearEdges(List<Edge> edges)
        {
            edges.Sort((a, b) =>
            {
                int horizontalCompare = a.horizontal.CompareTo(b.horizontal);
                if (horizontalCompare != 0)
                {
                    return horizontalCompare;
                }

                int fixedCompare = a.fixedCoord.CompareTo(b.fixedCoord);
                if (fixedCompare != 0)
                {
                    return fixedCompare;
                }

                int signCompare = a.outsideSign.CompareTo(b.outsideSign);
                if (signCompare != 0)
                {
                    return signCompare;
                }

                return a.start.CompareTo(b.start);
            });

            List<Edge> merged = new List<Edge>();

            for (int i = 0; i < edges.Count; i++)
            {
                Edge current = edges[i];

                if (merged.Count == 0)
                {
                    merged.Add(current);
                    continue;
                }

                Edge last = merged[merged.Count - 1];
                if (last.horizontal == current.horizontal &&
                    Mathf.Abs(last.fixedCoord - current.fixedCoord) <= coordinateTolerance &&
                    last.outsideSign == current.outsideSign &&
                    Mathf.Abs(last.end - current.start) <= coordinateTolerance)
                {
                    last.end = current.end;
                    merged[merged.Count - 1] = last;
                }
                else
                {
                    merged.Add(current);
                }
            }

            return merged;
        }

        private void EmitWall(Transform parent, Edge edge, int index)
        {
            float length = Mathf.Abs(edge.end - edge.start);
            if (length <= coordinateTolerance)
            {
                return;
            }

            Vector3 center;
            Vector3 size;

            if (edge.horizontal)
            {
                float z = edge.fixedCoord + edge.outsideSign * wallThickness * 0.5f;
                center = new Vector3((edge.start + edge.end) * 0.5f, yCenter, z);
                size = new Vector3(length + wallThickness, wallHeight, wallThickness);
            }
            else
            {
                float x = edge.fixedCoord + edge.outsideSign * wallThickness * 0.5f;
                center = new Vector3(x, yCenter, (edge.start + edge.end) * 0.5f);
                size = new Vector3(wallThickness, wallHeight, length + wallThickness);
            }

            CreateWall(parent, "BoundaryWall_" + index, center, size);
        }

        private void CreateWall(Transform parent, string name, Vector3 center, Vector3 size)
        {
            GameObject wall;

            if (addVisibleWallMesh)
            {
                wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                wall.name = name;
                wall.transform.SetParent(parent, true);
                wall.transform.position = center;
                wall.transform.localScale = size;

                Collider collider = wall.GetComponent<Collider>();
                if (collider == null)
                {
                    collider = wall.AddComponent<BoxCollider>();
                }

                collider.isTrigger = false;
            }
            else
            {
                wall = new GameObject(name);
                wall.transform.SetParent(parent, true);
                wall.transform.position = center;

                BoxCollider collider = wall.AddComponent<BoxCollider>();
                collider.size = size;
                collider.isTrigger = false;
            }

            wall.layer = LayerMask.NameToLayer("Default");
        }

        private void AddUnique(List<float> values, float value)
        {
            for (int i = 0; i < values.Count; i++)
            {
                if (Mathf.Abs(values[i] - value) <= coordinateTolerance)
                {
                    return;
                }
            }

            values.Add(value);
        }

        private string GetHierarchyPath(Transform transform)
        {
            if (transform == null)
            {
                return string.Empty;
            }

            string path = transform.name;
            Transform parent = transform.parent;
            while (parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }

            return path;
        }
    }
}

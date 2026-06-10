using System.Collections;
using System.Collections.Generic;
using LegendOfZed.LegacyMapGenerator;
using UnityEngine;

namespace LegendOfZed.MapIntegration
{
    [DisallowMultipleComponent]
    public sealed class ZedForcedZoneBuildingSpawnerV39H : MonoBehaviour
    {
        [Header("References")]
        public Transform scanRoot;
        public GameObject[] buildingPrefabs;

        [Header("Generated Roots")]
        public string generatedBuildingRootName = "Generated_RoadFrontage_Buildings";
        public string generatedAutoZoneRootName = "Generated_AutoBuildable_Zones";
        public bool clearPreviousGeneratedBuildings = true;
        public bool clearPreviousAutoZones = true;

        [Header("Timing")]
        public bool buildOnStart = true;
        [Min(0f)] public float startDelay = 0.8f;

        [Header("Auto Zones")]
        public bool generateAutoZones = true;
        [Min(2f)] public float autoZoneDepth = 7.5f;
        [Min(2f)] public float autoZoneMinWidth = 8f;
        [Min(0f)] public float autoZoneOuterInset = 1.25f;
        [Min(0f)] public float autoZoneRoadClearance = 0.15f;

        public string[] autoZoneSkipTileNameContains = { "park", "plaza", "water" };

        [Header("Building Packing")]
        [Min(0f)] public float buildingGap = 0.25f;
        [Min(1)] public int maxBuildingsPerZone = 12;
        [Min(0f)] public float minRemainingWidthToContinue = 1.25f;
        public bool preferSmallBuildings = true;
        public bool avoidImmediatePrefabRepeat = true;

        [Header("Road Safety")]
        public bool rejectAgainstDetectedRoadRenderers = true;
        [Min(0f)] public float roadRejectPadding = 0.05f;

        public string[] roadNameContains = { "road", "street", "asphalt" };

        public string[] rejectRoadNameContains =
        {
            "sidewalk", "pavement", "curb", "kerb", "line", "marking", "crosswalk",
            "spawn", "trigger", "wall", "building", "prop", "tree", "grass", "park"
        };

        public float maxRoadCenterY = 2f;
        [Min(0.01f)] public float maxRoadRendererHeight = 1.5f;
        [Min(0.01f)] public float minRoadRendererSize = 0.25f;

        [Header("Generated Zone Defaults")]
        public ZedBuildableZone.DepthAlignment depthAlignment = ZedBuildableZone.DepthAlignment.BackEdge;
        public float buildingYawOffset = 180f;
        public float generatedZoneEdgeInset = 0.25f;
        public float generatedZoneSidePadding = 0.2f;

        [Header("Logging")]
        public bool logDetails = true;

        private readonly List<Rect2> roadRects = new List<Rect2>();
        private readonly List<Rect2> placedRects = new List<Rect2>();

        private int tilesScanned;
        private int existingZonesFound;
        private int autoZonesCreated;
        private int autoZoneRoadRejects;
        private int autoZoneSmallRejects;
        private int zonesFound;
        private int zonesUsed;
        private int buildingsSpawned;
        private int fitRejects;
        private int roadRejects;
        private int overlapRejects;
        private int emptyZoneRejects;

        private enum EdgeSide { North, South, East, West }

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
            autoZoneDepth = Mathf.Max(2f, autoZoneDepth);
            autoZoneMinWidth = Mathf.Max(2f, autoZoneMinWidth);
            autoZoneOuterInset = Mathf.Max(0f, autoZoneOuterInset);
            autoZoneRoadClearance = Mathf.Max(0f, autoZoneRoadClearance);
            buildingGap = Mathf.Max(0f, buildingGap);
            maxBuildingsPerZone = Mathf.Max(1, maxBuildingsPerZone);
            minRemainingWidthToContinue = Mathf.Max(0f, minRemainingWidthToContinue);
            roadRejectPadding = Mathf.Max(0f, roadRejectPadding);
            generatedZoneEdgeInset = Mathf.Max(0f, generatedZoneEdgeInset);
            generatedZoneSidePadding = Mathf.Max(0f, generatedZoneSidePadding);
        }

        [ContextMenu("Build V3.9H Zone Buildings Now")]
        public void BuildNow()
        {
            OnValidate();
            ResetCounters();

            ResolveScanRoot();
            DetectRoadRects();

            Transform autoZoneRoot = GetOrCreateRoot(generatedAutoZoneRootName);
            if (clearPreviousAutoZones)
            {
                ClearChildren(autoZoneRoot);
            }

            if (generateAutoZones)
            {
                GenerateAutoZones(autoZoneRoot);
            }

            Transform buildingRoot = GetOrCreateRoot(generatedBuildingRootName);
            if (clearPreviousGeneratedBuildings)
            {
                ClearChildren(buildingRoot);
            }

            ZedBuildableZone[] zones = FindObjectsByType<ZedBuildableZone>(FindObjectsInactive.Exclude);
            zonesFound = zones != null ? zones.Length : 0;

            if (zones != null)
            {
                for (int i = 0; i < zones.Length; i++)
                {
                    SpawnIntoZone(zones[i], buildingRoot);
                }
            }

            if (logDetails)
            {
                Debug.Log(
                    "V3.9H CLEAN forced zone building spawner complete. TilesScanned=" + tilesScanned +
                    ", ExistingZones=" + existingZonesFound +
                    ", AutoZonesCreated=" + autoZonesCreated +
                    ", Zones=" + zonesFound +
                    ", ZonesUsed=" + zonesUsed +
                    ", Buildings=" + buildingsSpawned +
                    ", FitRejects=" + fitRejects +
                    ", RoadRejects=" + roadRejects +
                    ", OverlapRejects=" + overlapRejects +
                    ", EmptyZoneRejects=" + emptyZoneRejects +
                    ", AutoZoneRoadRejects=" + autoZoneRoadRejects +
                    ", AutoZoneSmallRejects=" + autoZoneSmallRejects +
                    ", RoadRects=" + roadRects.Count + ".",
                    this);
            }
        }

        private void ResetCounters()
        {
            tilesScanned = 0;
            existingZonesFound = 0;
            autoZonesCreated = 0;
            autoZoneRoadRejects = 0;
            autoZoneSmallRejects = 0;
            zonesFound = 0;
            zonesUsed = 0;
            buildingsSpawned = 0;
            fitRejects = 0;
            roadRejects = 0;
            overlapRejects = 0;
            emptyZoneRejects = 0;
            roadRects.Clear();
            placedRects.Clear();
        }

        private void GenerateAutoZones(Transform autoZoneRoot)
        {
            ZedBuildableZone[] existing = FindObjectsByType<ZedBuildableZone>(FindObjectsInactive.Exclude);
            existingZonesFound = existing != null ? existing.Length : 0;

            ZedLegacyRoomTile[] tiles = null;
            if (scanRoot != null)
            {
                tiles = scanRoot.GetComponentsInChildren<ZedLegacyRoomTile>(true);
            }

            if (tiles == null || tiles.Length == 0)
            {
                tiles = FindObjectsByType<ZedLegacyRoomTile>(FindObjectsInactive.Exclude);
            }

            tilesScanned = tiles != null ? tiles.Length : 0;
            if (tiles == null)
            {
                return;
            }

            for (int i = 0; i < tiles.Length; i++)
            {
                GenerateZonesForTile(tiles[i], autoZoneRoot);
            }
        }

        private void GenerateZonesForTile(ZedLegacyRoomTile tile, Transform autoZoneRoot)
        {
            if (tile == null || autoZoneRoot == null || ShouldSkipTile(tile.gameObject))
            {
                return;
            }

            Bounds bounds;
            if (!TryCalculateRendererBounds(tile.gameObject, out bounds))
            {
                return;
            }

            TryCreateAutoZone(tile.name, autoZoneRoot, MakeNorthRect(bounds), EdgeSide.North);
            TryCreateAutoZone(tile.name, autoZoneRoot, MakeSouthRect(bounds), EdgeSide.South);
            TryCreateAutoZone(tile.name, autoZoneRoot, MakeEastRect(bounds), EdgeSide.East);
            TryCreateAutoZone(tile.name, autoZoneRoot, MakeWestRect(bounds), EdgeSide.West);
        }

        private bool ShouldSkipTile(GameObject tile)
        {
            if (tile == null || autoZoneSkipTileNameContains == null)
            {
                return false;
            }

            string lower = tile.name.ToLowerInvariant();
            for (int i = 0; i < autoZoneSkipTileNameContains.Length; i++)
            {
                string term = autoZoneSkipTileNameContains[i];
                if (!string.IsNullOrEmpty(term) && lower.Contains(term.ToLowerInvariant()))
                {
                    return true;
                }
            }

            return false;
        }

        private Rect2 MakeNorthRect(Bounds b)
        {
            return new Rect2 { minX = b.min.x + autoZoneOuterInset, maxX = b.max.x - autoZoneOuterInset, minZ = b.max.z - autoZoneOuterInset - autoZoneDepth, maxZ = b.max.z - autoZoneOuterInset };
        }

        private Rect2 MakeSouthRect(Bounds b)
        {
            return new Rect2 { minX = b.min.x + autoZoneOuterInset, maxX = b.max.x - autoZoneOuterInset, minZ = b.min.z + autoZoneOuterInset, maxZ = b.min.z + autoZoneOuterInset + autoZoneDepth };
        }

        private Rect2 MakeEastRect(Bounds b)
        {
            return new Rect2 { minX = b.max.x - autoZoneOuterInset - autoZoneDepth, maxX = b.max.x - autoZoneOuterInset, minZ = b.min.z + autoZoneOuterInset, maxZ = b.max.z - autoZoneOuterInset };
        }

        private Rect2 MakeWestRect(Bounds b)
        {
            return new Rect2 { minX = b.min.x + autoZoneOuterInset, maxX = b.min.x + autoZoneOuterInset + autoZoneDepth, minZ = b.min.z + autoZoneOuterInset, maxZ = b.max.z - autoZoneOuterInset };
        }

        private void TryCreateAutoZone(string tileName, Transform autoZoneRoot, Rect2 rect, EdgeSide side)
        {
            float width = side == EdgeSide.North || side == EdgeSide.South ? rect.Width : rect.Depth;
            float depth = side == EdgeSide.North || side == EdgeSide.South ? rect.Depth : rect.Width;

            if (width < autoZoneMinWidth || depth < 2f)
            {
                autoZoneSmallRejects++;
                return;
            }

            if (RectOverlapsAnyRoad(rect, autoZoneRoadClearance))
            {
                autoZoneRoadRejects++;
                return;
            }

            GameObject go = new GameObject("AutoBuildableZone_" + tileName + "_" + side);
            go.transform.SetParent(autoZoneRoot, false);
            go.transform.position = rect.Center;
            go.transform.rotation = RotationForSide(side);

            ZedBuildableZone zone = go.AddComponent<ZedBuildableZone>();
            zone.width = width;
            zone.depth = depth;
            zone.edgeInset = generatedZoneEdgeInset;
            zone.sidePadding = generatedZoneSidePadding;
            zone.depthAlignment = depthAlignment;
            zone.buildingYawOffset = buildingYawOffset;
            zone.allowAlleys = true;

            autoZonesCreated++;
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

        private void SpawnIntoZone(ZedBuildableZone zone, Transform buildingRoot)
        {
            if (zone == null || buildingRoot == null)
            {
                return;
            }

            List<Candidate> candidates = BuildCandidateList(zone);
            if (candidates.Count == 0)
            {
                emptyZoneRejects++;
                return;
            }

            if (preferSmallBuildings)
            {
                candidates.Sort((a, b) => a.size.x.CompareTo(b.size.x));
            }

            float usableWidth = Mathf.Max(0f, zone.width - zone.edgeInset * 2f - zone.sidePadding * 2f);
            float usableDepth = Mathf.Max(0f, zone.depth - zone.edgeInset * 2f);

            if (usableWidth <= 0f || usableDepth <= 0f)
            {
                emptyZoneRejects++;
                return;
            }

            float cursor = -usableWidth * 0.5f;
            int placedInZone = 0;
            int guard = 0;
            GameObject lastPrefab = null;

            while (placedInZone < maxBuildingsPerZone && guard++ < maxBuildingsPerZone * 12)
            {
                float remaining = usableWidth * 0.5f - cursor;
                if (remaining < minRemainingWidthToContinue)
                {
                    break;
                }

                Candidate selected = FindCandidateThatFits(candidates, remaining, usableDepth, lastPrefab);
                if (selected == null)
                {
                    fitRejects++;
                    break;
                }

                float width = selected.size.x;
                float depth = selected.size.y;

                float localX = cursor + width * 0.5f;
                Vector3 localDepth = zone.GetDepthAlignedLocalCenter(depth);
                Vector3 worldCenter = zone.transform.TransformPoint(new Vector3(localX, 0f, localDepth.z));
                Quaternion rotation = zone.GetBuildingRotation();

                Rect2 footprint = BuildWorldAabbFromOrientedFootprint(worldCenter, rotation, width, depth);

                if (RectOverlapsAnyPlaced(footprint))
                {
                    overlapRejects++;
                    cursor += Mathf.Max(0.5f, width * 0.25f);
                    continue;
                }

                if (rejectAgainstDetectedRoadRenderers && RectOverlapsAnyRoad(footprint, roadRejectPadding))
                {
                    roadRejects++;
                    cursor += Mathf.Max(0.5f, width * 0.25f);
                    continue;
                }

                GameObject instance = Instantiate(selected.prefab, worldCenter, rotation, buildingRoot);
                instance.name = selected.prefab.name + "_ZoneBuilding";

                Bounds visualBounds;
                if (TryCalculateRendererBounds(instance, out visualBounds))
                {
                    Vector3 delta = worldCenter - visualBounds.center;
                    delta.y = 0f;
                    instance.transform.position += delta;

                    if (TryCalculateRendererBounds(instance, out visualBounds))
                    {
                        footprint = new Rect2 { minX = visualBounds.min.x, maxX = visualBounds.max.x, minZ = visualBounds.min.z, maxZ = visualBounds.max.z };
                    }
                }

                if (rejectAgainstDetectedRoadRenderers && RectOverlapsAnyRoad(footprint, roadRejectPadding))
                {
                    roadRejects++;
                    Destroy(instance);
                    cursor += Mathf.Max(0.5f, width * 0.25f);
                    continue;
                }

                placedRects.Add(footprint);
                buildingsSpawned++;
                placedInZone++;
                lastPrefab = selected.prefab;
                cursor += width + buildingGap;
            }

            if (placedInZone > 0)
            {
                zonesUsed++;
            }
        }

        private List<Candidate> BuildCandidateList(ZedBuildableZone zone)
        {
            List<Candidate> candidates = new List<Candidate>();

            if (zone != null && zone.buildingPrefabs != null && zone.buildingPrefabs.Count > 0)
            {
                for (int i = 0; i < zone.buildingPrefabs.Count; i++)
                {
                    AddCandidate(candidates, zone.buildingPrefabs[i]);
                }

                return candidates;
            }

            if (buildingPrefabs != null)
            {
                for (int i = 0; i < buildingPrefabs.Length; i++)
                {
                    AddCandidate(candidates, buildingPrefabs[i]);
                }
            }

            return candidates;
        }

        private void AddCandidate(List<Candidate> candidates, GameObject prefab)
        {
            if (prefab == null)
            {
                return;
            }

            candidates.Add(new Candidate { prefab = prefab, size = ZedBuildingFootprint.EstimateFootprintSize(prefab) });
        }

        private Candidate FindCandidateThatFits(List<Candidate> candidates, float remainingWidth, float usableDepth, GameObject lastPrefab)
        {
            for (int i = 0; i < candidates.Count; i++)
            {
                Candidate c = candidates[i];
                if (c == null || c.prefab == null)
                {
                    continue;
                }

                if (avoidImmediatePrefabRepeat && lastPrefab != null && candidates.Count > 1 && c.prefab == lastPrefab)
                {
                    continue;
                }

                if (c.size.x <= remainingWidth && c.size.y <= usableDepth)
                {
                    return c;
                }
            }

            for (int i = 0; i < candidates.Count; i++)
            {
                Candidate c = candidates[i];
                if (c != null && c.prefab != null && c.size.x <= remainingWidth && c.size.y <= usableDepth)
                {
                    return c;
                }
            }

            return null;
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
            return a.minX < b.maxX && a.maxX > b.minX && a.minZ < b.maxZ && a.maxZ > b.minZ;
        }

        private void ResolveScanRoot()
        {
            if (scanRoot != null)
            {
                return;
            }

            GameObject root = GameObject.Find("Generated_Map_Tiles") ?? GameObject.Find("Generated_Map_Root") ?? GameObject.Find("Generated_Map") ?? GameObject.Find("GeneratedMap");
            if (root != null)
            {
                scanRoot = root.transform;
            }
        }

        private void DetectRoadRects()
        {
            roadRects.Clear();

            Renderer[] renderers = scanRoot != null ? scanRoot.GetComponentsInChildren<Renderer>(true) : FindObjectsByType<Renderer>(FindObjectsInactive.Exclude);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer r = renderers[i];
                if (r == null || !r.enabled || !IsRoadRenderer(r))
                {
                    continue;
                }

                Bounds b = r.bounds;
                roadRects.Add(new Rect2 { minX = b.min.x, maxX = b.max.x, minZ = b.min.z, maxZ = b.max.z });
            }
        }

        private bool IsRoadRenderer(Renderer renderer)
        {
            Bounds b = renderer.bounds;
            if (b.center.y > maxRoadCenterY || b.size.y > maxRoadRendererHeight || b.size.x < minRoadRendererSize || b.size.z < minRoadRendererSize)
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

            if (renderer.sharedMaterial != null)
            {
                text += " " + renderer.sharedMaterial.name.ToLowerInvariant();
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
                Renderer r = renderers[i];
                if (r == null || !r.enabled)
                {
                    continue;
                }

                if (!hasBounds)
                {
                    bounds = r.bounds;
                    hasBounds = true;
                }
                else
                {
                    bounds.Encapsulate(r.bounds);
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
    }
}

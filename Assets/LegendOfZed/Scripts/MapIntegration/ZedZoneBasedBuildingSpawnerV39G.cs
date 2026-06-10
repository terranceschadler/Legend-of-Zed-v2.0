using System.Collections;
using System.Collections.Generic;
using LegendOfZed.LegacyMapGenerator;
using UnityEngine;

namespace LegendOfZed.MapIntegration
{
    /// <summary>
    /// v3.9g1 unique integrated auto-zone building spawner.
    ///
    /// This replaces road-frontage guessing with explicit authored buildable zones.
    /// A building may spawn only when its full footprint fits inside a ZedBuildableZone.
    ///
    /// Keeps the generated building root name used by the existing street prop blocker pass:
    /// Generated_RoadFrontage_Buildings
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ZedZoneBasedBuildingSpawnerV39G : MonoBehaviour
    {
        [Header("References")]
        public Transform scanRoot;

        [Tooltip("Fallback building catalog used when a zone does not provide its own prefab list.")]
        public GameObject[] buildingPrefabs;

        [Header("Generated Root")]
        public string generatedRootName = "Generated_RoadFrontage_Buildings";
        public bool clearPreviousGeneratedBuildings = true;

        [Header("Timing")]
        public bool buildOnStart = true;
        [Min(0f)] public float startDelay = 0.35f;

        [Header("Packing")]
        [Tooltip("Minimum distance between generated buildings inside a zone.")]
        [Min(0f)] public float buildingGap = 0.25f;

        [Tooltip("Try smaller buildings first when zones are tight.")]
        public bool preferSmallerBuildingsInTightZones = true;

        [Tooltip("Shuffle equal-size candidate buildings for variety.")]
        public bool randomizeCandidateOrder = true;

        [Tooltip("If non-zero, gives repeatable random candidate order.")]
        public int randomSeed = 0;

        [Tooltip("Stop filling a zone after this many buildings, even if room remains.")]
        [Min(1)] public int maxBuildingsPerZone = 12;

        [Tooltip("Minimum leftover width required to continue trying another building.")]
        [Min(0f)] public float minRemainingWidthToContinue = 1.25f;

        [Header("Safe Zone Front Row")]
        [Tooltip("Use each zone's authored front row. No street-adjacency gate is used, so valid zones are not skipped.")]
        public bool useAuthoredZoneFrontRowOnly = true;

        [Header("Integrated Auto Zone Coverage")]
        [Tooltip("Automatically add buildable zones to generated tiles before building placement, so this no longer depends on a separate coverage component running first.")]
        public bool autoGenerateBuildableZones = true;

        [Tooltip("Generated auto-zone root name.")]
        public string generatedAutoZoneRootName = "Generated_AutoBuildable_Zones";

        [Tooltip("Clear previously generated auto zones before creating new ones.")]
        public bool clearPreviousAutoZones = true;

        [Tooltip("Only auto-generate zones if fewer than this many zones already exist.")]
        [Min(0)] public int autoZoneMinimumDesiredZones = 40;

        [Tooltip("Depth of each generated buildable strip.")]
        [Min(2f)] public float autoZoneDepth = 7.5f;

        [Tooltip("Minimum width required for an auto zone.")]
        [Min(2f)] public float autoZoneMinWidth = 8f;

        [Tooltip("Inset from the outer tile edge.")]
        [Min(0f)] public float autoZoneOuterInset = 1.25f;

        [Tooltip("Road clearance used when validating auto zones.")]
        [Min(0f)] public float autoZoneRoadClearance = 0.75f;

        [Tooltip("Skip auto zones on tiles with these name terms.")]
        public string[] autoZoneSkipTileNameContains =
        {
            "park",
            "plaza",
            "water"
        };

        [Header("Placement Safety")]
        [Tooltip("Final road safety check. This should usually stay enabled.")]
        public bool rejectAgainstDetectedRoadRenderers = true;

        [Tooltip("Road renderer padding for the final road safety check.")]
        [Min(0f)] public float roadRejectPadding = 0.1f;

        [Tooltip("Names/material terms treated as road renderers.")]
        public string[] roadNameContains =
        {
            "road",
            "street",
            "asphalt"
        };

        [Tooltip("Names/material terms rejected from road renderer detection.")]
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

        [Header("Logging")]
        public bool logDetails = true;

        [Header("Debug")]
        public bool drawPlacedFootprints = true;
        public Color placedFootprintColor = new Color(1f, 0f, 1f, 1f);

        private readonly List<Rect2> roadRects = new List<Rect2>();
        private readonly List<PlacedFootprint> placedFootprints = new List<PlacedFootprint>();

        private int zonesFound;
        private int zonesUsed;
        private int buildingsSpawned;
        private int fitRejects;
        private int roadRejects;
        private int overlapRejects;
        private int emptyZoneRejects;
        private int autoZonesCreated;
        private int autoZoneRoadRejects;
        private int autoZoneSmallRejects;

        private struct Rect2
        {
            public float minX;
            public float maxX;
            public float minZ;
            public float maxZ;
        }

        private struct PlacedFootprint
        {
            public Vector3 center;
            public Vector3 size;
            public Quaternion rotation;
            public Rect2 rect;
        }

        private enum EdgeSide
        {
            North,
            South,
            East,
            West
        }

        private sealed class Candidate
        {
            public GameObject prefab;
            public Vector2 size;
        }

        private void OnValidate()
        {
            startDelay = Mathf.Max(0f, startDelay);
            buildingGap = Mathf.Max(0f, buildingGap);
            maxBuildingsPerZone = Mathf.Max(1, maxBuildingsPerZone);
            minRemainingWidthToContinue = Mathf.Max(0f, minRemainingWidthToContinue);
            autoZoneMinimumDesiredZones = Mathf.Max(0, autoZoneMinimumDesiredZones);
            autoZoneDepth = Mathf.Max(2f, autoZoneDepth);
            autoZoneMinWidth = Mathf.Max(2f, autoZoneMinWidth);
            autoZoneOuterInset = Mathf.Max(0f, autoZoneOuterInset);
            autoZoneRoadClearance = Mathf.Max(0f, autoZoneRoadClearance);
            roadRejectPadding = Mathf.Max(0f, roadRejectPadding);
            maxRoadRendererHeight = Mathf.Max(0.01f, maxRoadRendererHeight);
            minRoadRendererSize = Mathf.Max(0.01f, minRoadRendererSize);
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

        [ContextMenu("Build Zone-Based Buildings Now")]
        public void BuildNow()
        {
            OnValidate();

            if (randomSeed != 0)
            {
                Random.InitState(randomSeed);
            }

            zonesFound = 0;
            zonesUsed = 0;
            buildingsSpawned = 0;
            fitRejects = 0;
            roadRejects = 0;
            overlapRejects = 0;
            emptyZoneRejects = 0;
            autoZonesCreated = 0;
            autoZoneRoadRejects = 0;
            autoZoneSmallRejects = 0;

            roadRects.Clear();
            placedFootprints.Clear();

            Transform generatedRoot = GetOrCreateGeneratedRoot();
            if (clearPreviousGeneratedBuildings)
            {
                ClearChildren(generatedRoot);
            }

            DetectRoadRects();

            if (autoGenerateBuildableZones)
            {
                AutoGenerateBuildableZonesIfNeeded();
            }

            ZedBuildableZone[] zones = CollectZones();
            zonesFound = zones.Length;

            for (int i = 0; i < zones.Length; i++)
            {
                SpawnIntoZone(zones[i], generatedRoot);
            }

            if (logDetails)
            {
                Debug.Log(
                    "V3.9G1 integrated zone building spawner complete. Zones=" + zonesFound +
                    ", ZonesUsed=" + zonesUsed +
                    ", Buildings=" + buildingsSpawned +
                    ", FitRejects=" + fitRejects +
                    ", RoadRejects=" + roadRejects +
                    ", OverlapRejects=" + overlapRejects +
                    ", EmptyZoneRejects=" + emptyZoneRejects +
                    ", AuthoredFrontRows=True" +
                    ", AutoZonesCreated=" + autoZonesCreated +
                    ", AutoZoneRoadRejects=" + autoZoneRoadRejects +
                    ", AutoZoneSmallRejects=" + autoZoneSmallRejects +
                    ", RoadRects=" + roadRects.Count + ".",
                    this);
            }
        }

        private void AutoGenerateBuildableZonesIfNeeded()
        {
            ZedBuildableZone[] existing = CollectZones();
            if (existing.Length >= autoZoneMinimumDesiredZones)
            {
                return;
            }

            Transform root = ResolveScanRoot();
            if (root == null)
            {
                return;
            }

            Transform zoneRoot = GetOrCreateAutoZoneRoot();
            if (clearPreviousAutoZones)
            {
                ClearChildren(zoneRoot);
            }

            ZedLegacyRoomTile[] tiles = root.GetComponentsInChildren<ZedLegacyRoomTile>(true);
            for (int i = 0; i < tiles.Length; i++)
            {
                AutoGenerateZonesForTile(tiles[i], zoneRoot);
            }
        }

        private Transform ResolveScanRoot()
        {
            if (scanRoot != null)
            {
                return scanRoot;
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

            return scanRoot;
        }

        private void AutoGenerateZonesForTile(ZedLegacyRoomTile tile, Transform zoneRoot)
        {
            if (tile == null || zoneRoot == null)
            {
                return;
            }

            if (ShouldSkipAutoZoneTile(tile.gameObject))
            {
                return;
            }

            Bounds bounds;
            if (!TryCalculateRendererBounds(tile.gameObject, out bounds))
            {
                return;
            }

            TryCreateAutoZone(tile.name, zoneRoot, MakeAutoNorthRect(bounds), EdgeSide.North);
            TryCreateAutoZone(tile.name, zoneRoot, MakeAutoSouthRect(bounds), EdgeSide.South);
            TryCreateAutoZone(tile.name, zoneRoot, MakeAutoEastRect(bounds), EdgeSide.East);
            TryCreateAutoZone(tile.name, zoneRoot, MakeAutoWestRect(bounds), EdgeSide.West);
        }

        private bool ShouldSkipAutoZoneTile(GameObject tile)
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

        private Rect2 MakeAutoNorthRect(Bounds b)
        {
            return new Rect2
            {
                minX = b.min.x + autoZoneOuterInset,
                maxX = b.max.x - autoZoneOuterInset,
                minZ = b.max.z - autoZoneOuterInset - autoZoneDepth,
                maxZ = b.max.z - autoZoneOuterInset
            };
        }

        private Rect2 MakeAutoSouthRect(Bounds b)
        {
            return new Rect2
            {
                minX = b.min.x + autoZoneOuterInset,
                maxX = b.max.x - autoZoneOuterInset,
                minZ = b.min.z + autoZoneOuterInset,
                maxZ = b.min.z + autoZoneOuterInset + autoZoneDepth
            };
        }

        private Rect2 MakeAutoEastRect(Bounds b)
        {
            return new Rect2
            {
                minX = b.max.x - autoZoneOuterInset - autoZoneDepth,
                maxX = b.max.x - autoZoneOuterInset,
                minZ = b.min.z + autoZoneOuterInset,
                maxZ = b.max.z - autoZoneOuterInset
            };
        }

        private Rect2 MakeAutoWestRect(Bounds b)
        {
            return new Rect2
            {
                minX = b.min.x + autoZoneOuterInset,
                maxX = b.min.x + autoZoneOuterInset + autoZoneDepth,
                minZ = b.min.z + autoZoneOuterInset,
                maxZ = b.max.z - autoZoneOuterInset
            };
        }

        private void TryCreateAutoZone(string tileName, Transform zoneRoot, Rect2 rect, EdgeSide side)
        {
            float width = side == EdgeSide.North || side == EdgeSide.South ? rect.maxX - rect.minX : rect.maxZ - rect.minZ;
            float depth = side == EdgeSide.North || side == EdgeSide.South ? rect.maxZ - rect.minZ : rect.maxX - rect.minX;

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
            go.transform.SetParent(zoneRoot, false);
            go.transform.position = new Vector3((rect.minX + rect.maxX) * 0.5f, 0f, (rect.minZ + rect.maxZ) * 0.5f);
            go.transform.rotation = RotationForAutoZoneSide(side);

            ZedBuildableZone zone = go.AddComponent<ZedBuildableZone>();
            zone.width = width;
            zone.depth = depth;
            zone.edgeInset = 0.25f;
            zone.sidePadding = 0.2f;
            zone.depthAlignment = ZedBuildableZone.DepthAlignment.BackEdge;
            zone.buildingYawOffset = 180f;
            zone.allowAlleys = true;

            autoZonesCreated++;
        }

        private Quaternion RotationForAutoZoneSide(EdgeSide side)
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

        private Transform GetOrCreateAutoZoneRoot()
        {
            GameObject root = GameObject.Find(generatedAutoZoneRootName);
            if (root == null)
            {
                root = new GameObject(generatedAutoZoneRootName);
            }

            return root.transform;
        }

        private ZedBuildableZone[] CollectZones()
        {
            ZedBuildableZone[] zones = scanRoot != null
                ? scanRoot.GetComponentsInChildren<ZedBuildableZone>(true)
                : FindObjectsByType<ZedBuildableZone>(FindObjectsInactive.Exclude);

            List<ZedBuildableZone> valid = new List<ZedBuildableZone>();
            for (int i = 0; i < zones.Length; i++)
            {
                ZedBuildableZone zone = zones[i];
                if (zone == null || !zone.enabled)
                {
                    continue;
                }

                valid.Add(zone);
            }

            return valid.ToArray();
        }

        private void SpawnIntoZone(ZedBuildableZone zone, Transform generatedRoot)
        {
            if (zone == null || generatedRoot == null)
            {
                return;
            }

            List<Candidate> candidates = BuildCandidateList(zone);
            if (candidates.Count == 0)
            {
                emptyZoneRejects++;
                return;
            }

            float usableWidth = Mathf.Max(0f, zone.width - zone.edgeInset * 2f - zone.sidePadding * 2f);
            float usableDepth = Mathf.Max(0f, zone.depth - zone.edgeInset * 2f);

            if (usableWidth <= 0f || usableDepth <= 0f)
            {
                emptyZoneRejects++;
                return;
            }

            if (preferSmallerBuildingsInTightZones)
            {
                candidates.Sort((a, b) => a.size.x.CompareTo(b.size.x));
            }

            if (randomizeCandidateOrder)
            {
                ShuffleEqualEnough(candidates);
            }

            float cursor = -usableWidth * 0.5f;
            int placedInZone = 0;
            int guard = 0;

            while (placedInZone < maxBuildingsPerZone && guard++ < maxBuildingsPerZone * 12)
            {
                float remaining = usableWidth * 0.5f - cursor;
                if (remaining < minRemainingWidthToContinue)
                {
                    break;
                }

                Candidate selected = FindCandidateThatFits(candidates, remaining, usableDepth);
                if (selected == null)
                {
                    fitRejects++;
                    break;
                }

                float width = selected.size.x;
                float depth = selected.size.y;

                float localX = cursor + width * 0.5f;
                Vector3 localDepthCenter = zone.GetDepthAlignedLocalCenter(depth);
                Vector3 worldCenter = zone.transform.TransformPoint(new Vector3(localX, 0f, localDepthCenter.z));
                Quaternion rotation = zone.GetBuildingRotation();

                Rect2 worldRect = BuildWorldAabbFromOrientedFootprint(worldCenter, rotation, width, depth);

                if (RectOverlapsAnyPlaced(worldRect))
                {
                    overlapRejects++;
                    cursor += Mathf.Max(0.5f, width * 0.25f);
                    continue;
                }

                if (rejectAgainstDetectedRoadRenderers && RectOverlapsAnyRoad(worldRect, roadRejectPadding))
                {
                    roadRejects++;
                    cursor += Mathf.Max(0.5f, width * 0.25f);
                    continue;
                }

                GameObject instance = Instantiate(selected.prefab, worldCenter, rotation, generatedRoot);
                instance.name = selected.prefab.name + "_ZoneBuilding";

                // Align visual/footprint center after instantiate in case the model pivot is not centered.
                Bounds visualBounds;
                if (TryCalculateRendererBounds(instance, out visualBounds))
                {
                    Vector3 delta = worldCenter - visualBounds.center;
                    delta.y = 0f;
                    instance.transform.position += delta;

                    if (TryCalculateRendererBounds(instance, out visualBounds))
                    {
                        worldRect = new Rect2
                        {
                            minX = visualBounds.min.x,
                            maxX = visualBounds.max.x,
                            minZ = visualBounds.min.z,
                            maxZ = visualBounds.max.z
                        };
                    }
                }

                // Final strict safety after transform/pivot alignment.
                if (rejectAgainstDetectedRoadRenderers && RectOverlapsAnyRoad(worldRect, roadRejectPadding))
                {
                    roadRejects++;
                    Destroy(instance);
                    cursor += Mathf.Max(0.5f, width * 0.25f);
                    continue;
                }

                placedFootprints.Add(new PlacedFootprint
                {
                    center = worldCenter,
                    size = new Vector3(width, 0.1f, depth),
                    rotation = rotation,
                    rect = worldRect
                });

                buildingsSpawned++;
                placedInZone++;

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

            List<GameObject> source = zone != null && zone.buildingPrefabs != null && zone.buildingPrefabs.Count > 0
                ? zone.buildingPrefabs
                : null;

            if (source != null)
            {
                for (int i = 0; i < source.Count; i++)
                {
                    AddCandidate(candidates, source[i]);
                }
            }
            else if (buildingPrefabs != null)
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

            Vector2 size = ZedBuildingFootprint.EstimateFootprintSize(prefab);
            if (size.x <= 0f || size.y <= 0f)
            {
                return;
            }

            candidates.Add(new Candidate
            {
                prefab = prefab,
                size = size
            });
        }

        private Candidate FindCandidateThatFits(List<Candidate> candidates, float remainingWidth, float usableDepth)
        {
            Candidate best = null;

            for (int i = 0; i < candidates.Count; i++)
            {
                Candidate candidate = candidates[i];
                if (candidate == null || candidate.prefab == null)
                {
                    continue;
                }

                if (candidate.size.x <= remainingWidth && candidate.size.y <= usableDepth)
                {
                    best = candidate;
                    break;
                }
            }

            return best;
        }

        private void ShuffleEqualEnough(List<Candidate> candidates)
        {
            // Keep broad size order but avoid the exact same prefab sequence every run.
            for (int i = 0; i < candidates.Count; i++)
            {
                int j = Random.Range(i, candidates.Count);
                Candidate a = candidates[i];
                Candidate b = candidates[j];

                if (a == null || b == null)
                {
                    continue;
                }

                if (Mathf.Abs(a.size.x - b.size.x) <= 2f)
                {
                    candidates[i] = b;
                    candidates[j] = a;
                }
            }
        }

        private Rect2 BuildWorldAabbFromOrientedFootprint(Vector3 center, Quaternion rotation, float width, float depth)
        {
            Vector3 right = rotation * Vector3.right;
            Vector3 forward = rotation * Vector3.forward;

            Vector3 p0 = center + right * (-width * 0.5f) + forward * (-depth * 0.5f);
            Vector3 p1 = center + right * (width * 0.5f) + forward * (-depth * 0.5f);
            Vector3 p2 = center + right * (-width * 0.5f) + forward * (depth * 0.5f);
            Vector3 p3 = center + right * (width * 0.5f) + forward * (depth * 0.5f);

            Rect2 rect = new Rect2
            {
                minX = Mathf.Min(p0.x, p1.x, p2.x, p3.x),
                maxX = Mathf.Max(p0.x, p1.x, p2.x, p3.x),
                minZ = Mathf.Min(p0.z, p1.z, p2.z, p3.z),
                maxZ = Mathf.Max(p0.z, p1.z, p2.z, p3.z)
            };

            return rect;
        }

        private bool RectOverlapsAnyPlaced(Rect2 rect)
        {
            for (int i = 0; i < placedFootprints.Count; i++)
            {
                if (RectsOverlap(rect, placedFootprints[i].rect))
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

        private void OnDrawGizmos()
        {
            if (!drawPlacedFootprints)
            {
                return;
            }

            Gizmos.color = placedFootprintColor;

            for (int i = 0; i < placedFootprints.Count; i++)
            {
                PlacedFootprint footprint = placedFootprints[i];
                Gizmos.matrix = Matrix4x4.TRS(footprint.center, footprint.rotation, Vector3.one);
                Gizmos.DrawWireCube(Vector3.zero, footprint.size);
            }

            Gizmos.matrix = Matrix4x4.identity;
        }
    }
}

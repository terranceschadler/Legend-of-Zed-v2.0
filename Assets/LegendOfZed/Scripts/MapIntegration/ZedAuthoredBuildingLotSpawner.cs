using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LegendOfZed.MapIntegration
{
    /// <summary>
    /// v3.10AC clean authored lot building spawner.
    ///
    /// Source of truth:
    /// - ZedAuthoredBuildingLot position
    /// - ZedAuthoredBuildingLot rotation/local +Z
    /// - ZedAuthoredBuildingLot buildingCategory
    /// - ZedAuthoredBuildingLot footprint width/depth
    ///
    /// No road-based category detection.
    /// No footprint-category guessing.
    /// No corner auto-promotion.
    /// No socket/frontage/buildable-zone experiments.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ZedAuthoredBuildingLotSpawner : MonoBehaviour
    {
        [Header("Roots")]
        public Transform generatedMapRoot;
        public string generatedMapRootName = "Generated_Map_Tiles";
        public string buildingRootName = "Generated_RoadFrontage_Buildings";

        [Header("Building Prefabs")]
        public GameObject[] buildingPrefabs;

        [Tooltip("Editor/runtime fallback: if no building prefabs are assigned, find imported city building prefabs by name.")]
        public bool autoFindBuildingPrefabsIfEmpty = true;

        [Tooltip("Search root used by the editor prefab finder.")]
        public string runtimeBuildingPrefabSearchRoot = "Assets/LegendOfZed";

        [Header("Lot Marker Category Rules")]
        public bool filterPrefabsByAuthoredLotCategory = true;
        public bool lotMarkerCategoryIsOnlyTruth = true;
        public bool allowAnyPrefabFallbackForCategory = false;

        [Tooltip("Category=Corner only accepts prefabs whose names match the Corner keyword list.")]
        public bool strictCornerCategoryRequiresCornerPrefab = true;

        [Tooltip("Corner prefab name keywords.")]
        public string[] cornerCategoryKeywords = new[] { "Corner" };

        [Tooltip("Small shop prefab name keywords.")]
        public string[] smallShopCategoryKeywords = new[] { "Shop", "Store", "Cafe", "Market", "Restaurant" };

        [Tooltip("Apartment prefab name keywords.")]
        public string[] apartmentCategoryKeywords = new[] { "Apartment", "Apt", "Residential" };

        [Tooltip("Office prefab name keywords.")]
        public string[] officeCategoryKeywords = new[] { "Office", "Business", "Commercial" };

        [Tooltip("Generic/block filler prefab name keywords.")]
        public string[] fillerCategoryKeywords = new[] { "House", "Building", "Bld", "Block" };

        [Tooltip("Back-lot/service structure prefab name keywords.")]
        public string[] alleyCategoryKeywords = new[] { "Alley", "Garage", "Shed", "Back", "Small" };

        [Header("Timing")]
        [Min(0f)] public float startDelay = 1.0f;
        [Min(0f)] public float extraBuildDelaySeconds = 0.75f;

        [Header("Safety")]
        public bool clearPreviousBuildings = true;
        public bool suppressLegacyTileBuildingSpawns = true;
        public bool disableOldExperimentalBuildingSpawners = true;
        public bool rejectOverlappingLots = true;
        public bool rejectRoadOverlapsByName = true;
        [Min(0f)] public float overlapPadding = 0.35f;
        [Min(0f)] public float roadRejectClearance = 1.5f;

        [Tooltip("After spawning, move the prefab so its renderer-bounds center sits on the authored lot marker position.")]
        public bool centerSpawnedPrefabBoundsOnLot = true;

        [Tooltip("Moves accepted buildings inward into the block after centering. Lot local +Z should face the street, so this moves along -forward.")]
        [Min(0f)] public float blockInteriorSetback = 1.25f;

        [Header("Dense Packing / Variety")]
        public bool tryMultipleBuildingPrefabsPerLot = true;
        public bool preferLargeBuildingsFirst = true;
        public bool avoidRepeatingSamePrefabOnBlock = true;
        [Min(0)] public int recentPrefabMemory = 10;
        [Min(0f)] public float prefabRepeatPenalty = 1000f;
        [Tooltip("Maximum prefab attempts per lot. Zero or negative means try all filtered prefabs.")]
        public int maxPrefabAttemptsPerLot = 0;

        [Header("Legacy Marker Support")]
        [Tooltip("Disabled by default. The stable path is authored ZedAuthoredBuildingLot markers.")]
        public bool useLegacyBlgSpawnMarkersIfNoLots = false;
        [Min(0.5f)] public float legacyLotWidth = 8f;
        [Min(0.5f)] public float legacyLotDepth = 8f;

        [Header("Debug")]
        public bool stampDebugInfoInGeneratedNames = true;

        private readonly List<Rect2> acceptedRects = new List<Rect2>();
        private readonly List<Rect2> roadRects = new List<Rect2>();
        private readonly Dictionary<GameObject, int> prefabUsageCounts = new Dictionary<GameObject, int>();
        private readonly List<GameObject> recentPrefabUses = new List<GameObject>();

        private int lotsFound;
        private int lotsApproved;
        private int buildingsSpawned;
        private int noPrefabRejects;
        private int overlapRejects;
        private int roadRejects;
        private int categoryFilteredLots;
        private int categoryFallbackLots;
        private int categoryRejectedPrefabs;
        private int cornerOverridePrefabRejected;
        private int cornerFinalPrefabRejected;
        private int strictCornerCategoryRejects;
        private int prefabRetryAttempts;
        private int lotsFilledAfterPrefabRetry;
        private int lotsFailedAllPrefabAttempts;
        private int lotMarkerCategoryAnyCount;
        private int lotMarkerCategoryCornerCount;
        private int oldSpawnersDisabled;
        private int legacyMarkersUsed;

        private struct Rect2
        {
            public float minX;
            public float maxX;
            public float minZ;
            public float maxZ;
        }

        private sealed class LotData
        {
            public Transform transform;
            public float width;
            public float depth;
            public GameObject overridePrefab;
            public bool approved;
            public ZedAuthoredBuildingLotCategory category;
        }

        private IEnumerator Start()
        {
            yield return new WaitForSeconds(startDelay);

            if (extraBuildDelaySeconds > 0f)
            {
                yield return new WaitForSeconds(extraBuildDelaySeconds);
            }

            BuildNow();
        }

        [ContextMenu("Build Authored Lot Buildings Now")]
        public void BuildNow()
        {
            ResetCounters();

            if (suppressLegacyTileBuildingSpawns)
            {
                ApplyLegacyTileBuildingSpawnSuppression();
            }

            if (disableOldExperimentalBuildingSpawners)
            {
                DisableOldExperimentalBuildingSpawners();
            }

            AutoFindBuildingPrefabsIfNeeded();

            Transform mapRoot = ResolveGeneratedMapRoot();
            if (mapRoot == null)
            {
                Debug.LogWarning("V3.10AC Authored Lot Spawner could not find generated map root '" + generatedMapRootName + "'.");
                return;
            }

            Transform buildingRoot = PrepareBuildingRoot(mapRoot);
            BuildRoadRects(mapRoot);

            List<LotData> lots = CollectLots(mapRoot);
            lotsFound = lots.Count;

            for (int i = 0; i < lots.Count; i++)
            {
                TrySpawnLot(lots[i], buildingRoot);
            }

            LogSummary();
        }

        private void ResetCounters()
        {
            acceptedRects.Clear();
            roadRects.Clear();
            prefabUsageCounts.Clear();
            recentPrefabUses.Clear();

            lotsFound = 0;
            lotsApproved = 0;
            buildingsSpawned = 0;
            noPrefabRejects = 0;
            overlapRejects = 0;
            roadRejects = 0;
            categoryFilteredLots = 0;
            categoryFallbackLots = 0;
            categoryRejectedPrefabs = 0;
            cornerOverridePrefabRejected = 0;
            cornerFinalPrefabRejected = 0;
            strictCornerCategoryRejects = 0;
            prefabRetryAttempts = 0;
            lotsFilledAfterPrefabRetry = 0;
            lotsFailedAllPrefabAttempts = 0;
            lotMarkerCategoryAnyCount = 0;
            lotMarkerCategoryCornerCount = 0;
            oldSpawnersDisabled = 0;
            legacyMarkersUsed = 0;
        }

        private Transform ResolveGeneratedMapRoot()
        {
            if (generatedMapRoot != null)
            {
                return generatedMapRoot;
            }

            GameObject found = GameObject.Find(generatedMapRootName);
            return found != null ? found.transform : null;
        }

        private Transform PrepareBuildingRoot(Transform mapRoot)
        {
            Transform existing = mapRoot.Find(buildingRootName);

            if (existing != null && clearPreviousBuildings)
            {
                DestroyUnityObject(existing.gameObject);
                existing = null;
            }

            if (existing == null)
            {
                GameObject root = new GameObject(buildingRootName);
                root.transform.SetParent(mapRoot, false);
                existing = root.transform;
            }

            return existing;
        }

        private List<LotData> CollectLots(Transform mapRoot)
        {
            List<LotData> lots = new List<LotData>();

            ZedAuthoredBuildingLot[] markers = mapRoot.GetComponentsInChildren<ZedAuthoredBuildingLot>(true);
            for (int i = 0; i < markers.Length; i++)
            {
                ZedAuthoredBuildingLot marker = markers[i];
                if (marker == null)
                {
                    continue;
                }

                ZedAuthoredBuildingLotCategory category = marker.buildingCategory;
                if (category == ZedAuthoredBuildingLotCategory.Corner)
                {
                    lotMarkerCategoryCornerCount++;
                }
                else if (category == ZedAuthoredBuildingLotCategory.Any)
                {
                    lotMarkerCategoryAnyCount++;
                }

                lots.Add(new LotData
                {
                    transform = marker.transform,
                    width = Mathf.Max(0.5f, marker.width),
                    depth = Mathf.Max(0.5f, marker.depth),
                    overridePrefab = marker.overrideBuildingPrefab,
                    category = category,
                    approved = marker.enabled && marker.approvedForRuntimeSpawning && marker.canSpawnBuilding
                });
            }

            if (lots.Count == 0 && useLegacyBlgSpawnMarkersIfNoLots)
            {
                CollectLegacyBlgSpawnLots(mapRoot, lots);
            }

            return lots;
        }

        private void CollectLegacyBlgSpawnLots(Transform mapRoot, List<LotData> lots)
        {
            Transform[] transforms = mapRoot.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < transforms.Length; i++)
            {
                Transform t = transforms[i];
                if (t == null || string.IsNullOrEmpty(t.name))
                {
                    continue;
                }

                if (t.name.IndexOf("blgSpawn", StringComparison.OrdinalIgnoreCase) < 0 &&
                    t.name.IndexOf("BuildingSpawn", StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }

                lots.Add(new LotData
                {
                    transform = t,
                    width = legacyLotWidth,
                    depth = legacyLotDepth,
                    overridePrefab = null,
                    category = ZedAuthoredBuildingLotCategory.Any,
                    approved = true
                });

                legacyMarkersUsed++;
            }
        }

        private void TrySpawnLot(LotData lot, Transform buildingRoot)
        {
            if (lot == null || !lot.approved || lot.transform == null)
            {
                return;
            }

            lotsApproved++;

            GameObject[] candidates = BuildPrefabCandidateList(lot);
            if (candidates == null || candidates.Length == 0)
            {
                noPrefabRejects++;
                return;
            }

            int attemptsAllowed = maxPrefabAttemptsPerLot <= 0 ? candidates.Length : Mathf.Min(maxPrefabAttemptsPerLot, candidates.Length);
            for (int i = 0; i < attemptsAllowed; i++)
            {
                GameObject prefab = candidates[i];
                if (prefab == null)
                {
                    continue;
                }

                prefabRetryAttempts++;

                if (!IsPrefabAllowedForCategory(prefab, lot.category))
                {
                    if (lot.category == ZedAuthoredBuildingLotCategory.Corner)
                    {
                        cornerFinalPrefabRejected++;
                        strictCornerCategoryRejects++;
                    }

                    continue;
                }

                if (TrySpawnLotWithPrefab(lot, buildingRoot, prefab, i > 0))
                {
                    if (i > 0)
                    {
                        lotsFilledAfterPrefabRetry++;
                    }

                    RegisterPrefabUse(prefab);
                    return;
                }
            }

            lotsFailedAllPrefabAttempts++;
        }

        private GameObject[] BuildPrefabCandidateList(LotData lot)
        {
            List<GameObject> candidates = new List<GameObject>();

            if (lot.overridePrefab != null)
            {
                if (IsPrefabAllowedForCategory(lot.overridePrefab, lot.category))
                {
                    candidates.Add(lot.overridePrefab);
                    return candidates.ToArray();
                }

                if (lot.category == ZedAuthoredBuildingLotCategory.Corner)
                {
                    cornerOverridePrefabRejected++;
                    strictCornerCategoryRejects++;
                }
            }

            if (buildingPrefabs != null)
            {
                for (int i = 0; i < buildingPrefabs.Length; i++)
                {
                    if (buildingPrefabs[i] != null)
                    {
                        candidates.Add(buildingPrefabs[i]);
                    }
                }
            }

            if (filterPrefabsByAuthoredLotCategory && lot.category != ZedAuthoredBuildingLotCategory.Any)
            {
                int before = candidates.Count;
                List<GameObject> filtered = new List<GameObject>();

                for (int i = 0; i < candidates.Count; i++)
                {
                    GameObject prefab = candidates[i];
                    if (IsPrefabAllowedForCategory(prefab, lot.category))
                    {
                        filtered.Add(prefab);
                    }
                }

                if (filtered.Count > 0)
                {
                    candidates = filtered;
                    categoryFilteredLots++;
                    categoryRejectedPrefabs += Mathf.Max(0, before - filtered.Count);
                }
                else if (allowAnyPrefabFallbackForCategory && !(strictCornerCategoryRequiresCornerPrefab && lot.category == ZedAuthoredBuildingLotCategory.Corner))
                {
                    categoryFallbackLots++;
                }
                else
                {
                    categoryRejectedPrefabs += before;
                    if (lot.category == ZedAuthoredBuildingLotCategory.Corner)
                    {
                        strictCornerCategoryRejects++;
                    }
                    candidates.Clear();
                }
            }

            candidates.Sort((a, b) => GetPrefabSelectionScore(a, lot).CompareTo(GetPrefabSelectionScore(b, lot)));
            return candidates.ToArray();
        }

        private float GetPrefabSelectionScore(GameObject prefab, LotData lot)
        {
            if (prefab == null)
            {
                return float.MaxValue;
            }

            float score = 0f;

            if (avoidRepeatingSamePrefabOnBlock)
            {
                int usage;
                prefabUsageCounts.TryGetValue(prefab, out usage);
                score += usage * prefabRepeatPenalty;

                if (recentPrefabUses.Contains(prefab))
                {
                    score += prefabRepeatPenalty * 0.5f;
                }
            }

            if (preferLargeBuildingsFirst)
            {
                score -= EstimatePrefabFootprintArea(prefab);
            }

            return score;
        }

        private bool TrySpawnLotWithPrefab(LotData lot, Transform buildingRoot, GameObject prefab, bool isRetry)
        {
            if (!IsPrefabAllowedForCategory(prefab, lot.category))
            {
                if (lot.category == ZedAuthoredBuildingLotCategory.Corner)
                {
                    cornerFinalPrefabRejected++;
                    strictCornerCategoryRejects++;
                }

                return false;
            }

            Vector3 forward = SnapToCardinal(FlattenedSafe(lot.transform.forward, Vector3.forward));
            Quaternion rotation = Quaternion.LookRotation(forward, Vector3.up);
            GameObject instance = Instantiate(prefab, lot.transform.position, rotation, buildingRoot);

            if (stampDebugInfoInGeneratedNames)
            {
                instance.name = prefab.name + "_AuthoredLotBuilding_Category_" + lot.category + "_Facing_" + CardinalForwardToName(forward) + "_Yaw_" + Mathf.RoundToInt(rotation.eulerAngles.y);
            }
            else
            {
                instance.name = prefab.name + "_AuthoredLotBuilding";
            }

            Bounds bounds;
            Rect2 rect;

            if (TryCalculateRendererBounds(instance, out bounds))
            {
                if (centerSpawnedPrefabBoundsOnLot)
                {
                    Vector3 offset = lot.transform.position - bounds.center;
                    offset.y = 0f;
                    instance.transform.position += offset;
                    TryCalculateRendererBounds(instance, out bounds);
                }

                if (blockInteriorSetback > 0f)
                {
                    instance.transform.position += -forward * blockInteriorSetback;
                    TryCalculateRendererBounds(instance, out bounds);
                }

                rect = BoundsToRect(bounds, overlapPadding);
            }
            else
            {
                rect = LotToRect(lot, overlapPadding);
            }

            if (rejectRoadOverlapsByName && OverlapsAnyRoad(rect))
            {
                roadRejects++;
                DestroyUnityObject(instance);
                return false;
            }

            if (rejectOverlappingLots && OverlapsAccepted(rect))
            {
                overlapRejects++;
                DestroyUnityObject(instance);
                return false;
            }

            acceptedRects.Add(rect);
            buildingsSpawned++;
            return true;
        }

        private bool IsPrefabAllowedForCategory(GameObject prefab, ZedAuthoredBuildingLotCategory category)
        {
            if (prefab == null)
            {
                return false;
            }

            if (!filterPrefabsByAuthoredLotCategory || category == ZedAuthoredBuildingLotCategory.Any)
            {
                return true;
            }

            switch (category)
            {
                case ZedAuthoredBuildingLotCategory.Corner:
                    return MatchesAnyKeyword(prefab.name, cornerCategoryKeywords);
                case ZedAuthoredBuildingLotCategory.SmallShop:
                    return MatchesAnyKeyword(prefab.name, smallShopCategoryKeywords);
                case ZedAuthoredBuildingLotCategory.Apartment:
                    return MatchesAnyKeyword(prefab.name, apartmentCategoryKeywords);
                case ZedAuthoredBuildingLotCategory.Office:
                    return MatchesAnyKeyword(prefab.name, officeCategoryKeywords);
                case ZedAuthoredBuildingLotCategory.Filler:
                    return MatchesAnyKeyword(prefab.name, fillerCategoryKeywords);
                case ZedAuthoredBuildingLotCategory.Alley:
                    return MatchesAnyKeyword(prefab.name, alleyCategoryKeywords);
                default:
                    return true;
            }
        }

        private bool MatchesAnyKeyword(string name, string[] keywords)
        {
            if (string.IsNullOrEmpty(name) || keywords == null || keywords.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < keywords.Length; i++)
            {
                string keyword = keywords[i];
                if (string.IsNullOrEmpty(keyword))
                {
                    continue;
                }

                if (name.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        private void RegisterPrefabUse(GameObject prefab)
        {
            if (prefab == null)
            {
                return;
            }

            int usage;
            prefabUsageCounts.TryGetValue(prefab, out usage);
            prefabUsageCounts[prefab] = usage + 1;

            recentPrefabUses.Add(prefab);
            while (recentPrefabUses.Count > Mathf.Max(0, recentPrefabMemory))
            {
                recentPrefabUses.RemoveAt(0);
            }
        }

        private float EstimatePrefabFootprintArea(GameObject prefab)
        {
            if (prefab == null)
            {
                return 0f;
            }

            ZedBuildingFootprint footprint = prefab.GetComponent<ZedBuildingFootprint>();
            if (footprint != null)
            {
                return Mathf.Max(0.1f, footprint.width * footprint.depth);
            }

            Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>(true);
            if (renderers == null || renderers.Length == 0)
            {
                return 1f;
            }

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            return Mathf.Max(0.1f, bounds.size.x * bounds.size.z);
        }

        private void BuildRoadRects(Transform mapRoot)
        {
            roadRects.Clear();

            Renderer[] renderers = mapRoot.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null || !LooksLikeRoadObject(renderer.gameObject))
                {
                    continue;
                }

                roadRects.Add(BoundsToRect(renderer.bounds, roadRejectClearance));
            }

            Collider[] colliders = mapRoot.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++)
            {
                Collider collider = colliders[i];
                if (collider == null || !LooksLikeRoadObject(collider.gameObject))
                {
                    continue;
                }

                roadRects.Add(BoundsToRect(collider.bounds, roadRejectClearance));
            }
        }

        private bool LooksLikeRoadObject(GameObject go)
        {
            if (go == null)
            {
                return false;
            }

            string name = go.name;
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }

            return name.IndexOf("road", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   name.IndexOf("street", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   name.IndexOf("asphalt", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   name.IndexOf("lane", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   name.IndexOf("intersection", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   name.IndexOf("crosswalk", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private bool TryCalculateRendererBounds(GameObject instance, out Bounds bounds)
        {
            bounds = default;

            if (instance == null)
            {
                return false;
            }

            Renderer[] renderers = instance.GetComponentsInChildren<Renderer>(true);
            if (renderers == null || renderers.Length == 0)
            {
                return false;
            }

            bool initialized = false;
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null)
                {
                    continue;
                }

                if (!initialized)
                {
                    bounds = renderer.bounds;
                    initialized = true;
                }
                else
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }

            return initialized;
        }

        private Rect2 BoundsToRect(Bounds bounds, float padding)
        {
            return new Rect2
            {
                minX = bounds.min.x - padding,
                maxX = bounds.max.x + padding,
                minZ = bounds.min.z - padding,
                maxZ = bounds.max.z + padding
            };
        }

        private Rect2 LotToRect(LotData lot, float padding)
        {
            Vector3 center = lot.transform != null ? lot.transform.position : Vector3.zero;
            float halfWidth = Mathf.Max(0.5f, lot.width) * 0.5f;
            float halfDepth = Mathf.Max(0.5f, lot.depth) * 0.5f;

            return new Rect2
            {
                minX = center.x - halfWidth - padding,
                maxX = center.x + halfWidth + padding,
                minZ = center.z - halfDepth - padding,
                maxZ = center.z + halfDepth + padding
            };
        }

        private bool OverlapsAnyRoad(Rect2 rect)
        {
            for (int i = 0; i < roadRects.Count; i++)
            {
                if (Overlaps(rect, roadRects[i]))
                {
                    return true;
                }
            }

            return false;
        }

        private bool OverlapsAccepted(Rect2 rect)
        {
            for (int i = 0; i < acceptedRects.Count; i++)
            {
                if (Overlaps(rect, acceptedRects[i]))
                {
                    return true;
                }
            }

            return false;
        }

        private bool Overlaps(Rect2 a, Rect2 b)
        {
            return a.minX <= b.maxX &&
                   a.maxX >= b.minX &&
                   a.minZ <= b.maxZ &&
                   a.maxZ >= b.minZ;
        }

        private Vector3 SnapToCardinal(Vector3 forward)
        {
            forward = FlattenedSafe(forward, Vector3.forward);

            if (Mathf.Abs(forward.x) > Mathf.Abs(forward.z))
            {
                return forward.x >= 0f ? Vector3.right : Vector3.left;
            }

            return forward.z >= 0f ? Vector3.forward : Vector3.back;
        }

        private Vector3 FlattenedSafe(Vector3 value, Vector3 fallback)
        {
            value.y = 0f;
            if (value.sqrMagnitude <= 0.0001f)
            {
                value = fallback;
                value.y = 0f;
            }

            return value.normalized;
        }

        private string CardinalForwardToName(Vector3 forward)
        {
            forward = SnapToCardinal(forward);

            if (forward == Vector3.right)
            {
                return "East";
            }

            if (forward == Vector3.back)
            {
                return "South";
            }

            if (forward == Vector3.left)
            {
                return "West";
            }

            return "North";
        }

        private void ApplyLegacyTileBuildingSpawnSuppression()
        {
            MonoBehaviour[] behaviours = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include);
            for (int i = 0; i < behaviours.Length; i++)
            {
                MonoBehaviour behaviour = behaviours[i];
                if (behaviour == null)
                {
                    continue;
                }

                Type type = behaviour.GetType();
                if (type == null || type.Name != "ZedLegacyRoomTile")
                {
                    continue;
                }

                TrySetBool(type, behaviour, "spawnBuildings", false);
                TrySetBool(type, behaviour, "enableBuildingSpawns", false);
                TrySetBool(type, behaviour, "spawnAuthoredBuildings", false);
            }
        }

        private void DisableOldExperimentalBuildingSpawners()
        {
            string[] obsoleteNames =
            {
                "ZedPostGenerationCityBlockBuildingFiller",
                "ZedRoadFrontageBuildingSpawner",
                "ZedZoneBasedBuildingSpawner",
                "ZedZoneBasedBuildingSpawnerV39G",
                "ZedForcedZoneBuildingSpawnerV39H",
                "ZedFrontageStripBuildingSpawnerV39I",
                "ZedBuildingLotFiller",
                "ZedAutoBuildableZoneCoverage",
                "ZedMapTileGapResolver"
            };

            MonoBehaviour[] behaviours = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include);
            for (int i = 0; i < behaviours.Length; i++)
            {
                MonoBehaviour behaviour = behaviours[i];
                if (behaviour == null)
                {
                    continue;
                }

                string typeName = behaviour.GetType().Name;
                for (int n = 0; n < obsoleteNames.Length; n++)
                {
                    if (typeName == obsoleteNames[n])
                    {
                        behaviour.enabled = false;
                        oldSpawnersDisabled++;
                        break;
                    }
                }
            }
        }

        private void TrySetBool(Type type, object instance, string fieldOrPropertyName, bool value)
        {
            System.Reflection.FieldInfo field = type.GetField(fieldOrPropertyName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            if (field != null && field.FieldType == typeof(bool))
            {
                field.SetValue(instance, value);
                return;
            }

            System.Reflection.PropertyInfo property = type.GetProperty(fieldOrPropertyName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            if (property != null && property.PropertyType == typeof(bool) && property.CanWrite)
            {
                property.SetValue(instance, value, null);
            }
        }

        private void AutoFindBuildingPrefabsIfNeeded()
        {
            if (!autoFindBuildingPrefabsIfEmpty || HasAnyAssignedPrefab())
            {
                return;
            }

#if UNITY_EDITOR
            string[] searchRoots = !string.IsNullOrEmpty(runtimeBuildingPrefabSearchRoot)
                ? new[] { runtimeBuildingPrefabSearchRoot }
                : new[] { "Assets" };

            List<GameObject> found = new List<GameObject>();
            string[] guids = AssetDatabase.FindAssets("t:Prefab", searchRoots);
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (string.IsNullOrEmpty(path))
                {
                    continue;
                }

                string fileName = System.IO.Path.GetFileNameWithoutExtension(path);
                if (string.IsNullOrEmpty(fileName))
                {
                    continue;
                }

                if (fileName.IndexOf("Bld", StringComparison.OrdinalIgnoreCase) < 0 &&
                    fileName.IndexOf("Building", StringComparison.OrdinalIgnoreCase) < 0 &&
                    fileName.IndexOf("Shop", StringComparison.OrdinalIgnoreCase) < 0 &&
                    fileName.IndexOf("Apartment", StringComparison.OrdinalIgnoreCase) < 0 &&
                    fileName.IndexOf("Office", StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }

                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null && !found.Contains(prefab))
                {
                    found.Add(prefab);
                }
            }

            if (found.Count > 0)
            {
                buildingPrefabs = found.ToArray();
                Debug.Log("V3.10AC auto-filled authored building prefab pool. Count=" + buildingPrefabs.Length + ".");
            }
#endif
        }

        private bool HasAnyAssignedPrefab()
        {
            if (buildingPrefabs == null)
            {
                return false;
            }

            for (int i = 0; i < buildingPrefabs.Length; i++)
            {
                if (buildingPrefabs[i] != null)
                {
                    return true;
                }
            }

            return false;
        }

        private void DestroyUnityObject(UnityEngine.Object target)
        {
            if (target == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(target);
            }
            else
            {
                DestroyImmediate(target);
            }
        }

        private void LogSummary()
        {
            Debug.Log(
                "V3.10AC3 CLEAN AUTHORED LOT SPAWNER OBSOLETE API CLEANUP complete." +
                " LotsFound=" + lotsFound +
                ", LotsApproved=" + lotsApproved +
                ", BuildingsSpawned=" + buildingsSpawned +
                ", LotMarkerCategoryIsOnlyTruth=" + lotMarkerCategoryIsOnlyTruth +
                ", FilterPrefabsByAuthoredLotCategory=" + filterPrefabsByAuthoredLotCategory +
                ", AllowAnyPrefabFallbackForCategory=" + allowAnyPrefabFallbackForCategory +
                ", LotMarkerCategoryAnyCount=" + lotMarkerCategoryAnyCount +
                ", LotMarkerCategoryCornerCount=" + lotMarkerCategoryCornerCount +
                ", CategoryFilteredLots=" + categoryFilteredLots +
                ", CategoryFallbackLots=" + categoryFallbackLots +
                ", CategoryRejectedPrefabs=" + categoryRejectedPrefabs +
                ", CornerOverridePrefabRejected=" + cornerOverridePrefabRejected +
                ", CornerFinalPrefabRejected=" + cornerFinalPrefabRejected +
                ", StrictCornerCategoryRejects=" + strictCornerCategoryRejects +
                ", PrefabRetryAttempts=" + prefabRetryAttempts +
                ", LotsFilledAfterPrefabRetry=" + lotsFilledAfterPrefabRetry +
                ", LotsFailedAllPrefabAttempts=" + lotsFailedAllPrefabAttempts +
                ", OverlapRejects=" + overlapRejects +
                ", RoadRejects=" + roadRejects +
                ", RoadRects=" + roadRects.Count +
                ", LegacyMarkersUsed=" + legacyMarkersUsed +
                ", OldSpawnersDisabled=" + oldSpawnersDisabled +
                "."
            );
        }
    }
}

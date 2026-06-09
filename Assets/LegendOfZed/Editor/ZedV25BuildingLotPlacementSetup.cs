using System.Collections.Generic;
using LegendOfZed.Overworld;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LegendOfZed.Editor
{
    public static class ZedV25BuildingLotPlacementSetup
    {
        private const string BuildingPolishRootName = "Zed_BuildingLotPolishRoot";
        private const string EnterableMarkerName = "EnterablePortalMarker";
        private const string DoorMarkerName = "DoorFacingRoadMarker";

        [MenuItem("Legend of Zed/Setup/v2.5 Apply Building Lot Placement And Enterable Markers")]
        public static void ApplyBuildingLotPlacement()
        {
            ZedOverworldGenerationManager manager = Object.FindAnyObjectByType<ZedOverworldGenerationManager>();
            if (manager == null)
            {
                Debug.LogWarning("No ZedOverworldGenerationManager found. Open Zed_Overworld and run v2.3 rebuild first.");
                return;
            }

            RebuildManagerBlockList(manager);

            int lots = 0;
            int buildingsAligned = 0;
            int buildingsCreated = 0;
            int enterableMarkers = 0;

            for (int i = 0; i < manager.GeneratedBlocks.Count; i++)
            {
                ZedOverworldGeneratedBlock block = manager.GeneratedBlocks[i];
                if (block == null || block.BlockType != ZedOverworldBlockType.BuildingLot)
                {
                    continue;
                }

                lots++;

                ClearExistingBuildingPolish(block.transform);

                DirectionInfo roadDirection = GetNearestRoadDirection(manager, block.GridCoordinate);
                Transform building = FindExistingBuildingArt(block.transform);

                if (building == null)
                {
                    building = CreateBuildingFromPrefab(manager, block);
                    if (building != null)
                    {
                        buildingsCreated++;
                    }
                }

                if (building != null)
                {
                    AlignBuildingToRoad(manager, building, roadDirection);
                    buildingsAligned++;
                }

                GameObject polishRoot = new GameObject(BuildingPolishRootName);
                polishRoot.transform.SetParent(block.transform, false);
                polishRoot.transform.localPosition = Vector3.zero;
                polishRoot.transform.localRotation = Quaternion.identity;

                CreateLotPath(manager, polishRoot.transform, roadDirection);
                CreateDoorMarker(manager, block, polishRoot.transform, roadDirection);

                if (block.EnterableBuildingCandidate)
                {
                    CreateEnterableMarker(manager, block, polishRoot.transform, roadDirection);
                    enterableMarkers++;
                }

                EditorUtility.SetDirty(block);
                EditorUtility.SetDirty(block.gameObject);
            }

            SaveScene();

            Debug.Log(
                "v2.5 building lot placement applied. " +
                "Lots=" + lots +
                " BuildingsCreated=" + buildingsCreated +
                " BuildingsAligned=" + buildingsAligned +
                " EnterableMarkers=" + enterableMarkers + ".");
        }

        [MenuItem("Legend of Zed/Setup/v2.5 Clear Building Lot Placement Markers")]
        public static void ClearBuildingLotPlacement()
        {
            ZedOverworldGeneratedBlock[] blocks = Object.FindObjectsByType<ZedOverworldGeneratedBlock>(FindObjectsInactive.Include);

            int cleared = 0;
            for (int i = 0; i < blocks.Length; i++)
            {
                if (blocks[i] == null || blocks[i].BlockType != ZedOverworldBlockType.BuildingLot)
                {
                    continue;
                }

                Transform root = blocks[i].transform.Find(BuildingPolishRootName);
                if (root != null)
                {
                    Object.DestroyImmediate(root.gameObject);
                    cleared++;
                }
            }

            SaveScene();
            Debug.Log("v2.5 cleared building lot polish roots. Cleared=" + cleared + ".");
        }

        [MenuItem("Legend of Zed/Setup/v2.5 Validate Building Lot Placement")]
        public static void ValidateBuildingLotPlacement()
        {
            ZedOverworldGeneratedBlock[] blocks = Object.FindObjectsByType<ZedOverworldGeneratedBlock>(FindObjectsInactive.Include);

            int lots = 0;
            int lotsWithBuilding = 0;
            int enterableCandidates = 0;
            int enterableMarkers = 0;
            int missingPolish = 0;

            for (int i = 0; i < blocks.Length; i++)
            {
                ZedOverworldGeneratedBlock block = blocks[i];
                if (block == null || block.BlockType != ZedOverworldBlockType.BuildingLot)
                {
                    continue;
                }

                lots++;

                if (FindExistingBuildingArt(block.transform) != null)
                {
                    lotsWithBuilding++;
                }

                if (block.EnterableBuildingCandidate)
                {
                    enterableCandidates++;

                    if (block.GetComponentInChildren<ZedOverworldEnterableBuildingMarker>(true) != null)
                    {
                        enterableMarkers++;
                    }
                }

                if (block.transform.Find(BuildingPolishRootName) == null)
                {
                    missingPolish++;
                }
            }

            if (lots == 0)
            {
                Debug.LogWarning("v2.5 validation: no building lots found.");
                return;
            }

            if (missingPolish == 0 && enterableCandidates == enterableMarkers)
            {
                Debug.Log(
                    "v2.5 building lot placement validation passed. " +
                    "Lots=" + lots +
                    " LotsWithBuilding=" + lotsWithBuilding +
                    " EnterableCandidates=" + enterableCandidates +
                    " EnterableMarkers=" + enterableMarkers + ".");
            }
            else
            {
                Debug.LogWarning(
                    "v2.5 building lot placement validation warning. " +
                    "Lots=" + lots +
                    " LotsWithBuilding=" + lotsWithBuilding +
                    " MissingPolish=" + missingPolish +
                    " EnterableCandidates=" + enterableCandidates +
                    " EnterableMarkers=" + enterableMarkers + ".");
            }
        }

        private static Transform FindExistingBuildingArt(Transform block)
        {
            for (int i = 0; i < block.childCount; i++)
            {
                Transform child = block.GetChild(i);
                if (child == null)
                {
                    continue;
                }

                if (child.name.StartsWith("BuildingArt_") ||
                    child.name.StartsWith("EnterableBuildingArt_") ||
                    child.name.StartsWith("Building_") ||
                    child.name.Contains("Bld") ||
                    child.name.Contains("Building"))
                {
                    if (child.name == BuildingPolishRootName)
                    {
                        continue;
                    }

                    return child;
                }
            }

            return null;
        }

        private static Transform CreateBuildingFromPrefab(ZedOverworldGenerationManager manager, ZedOverworldGeneratedBlock block)
        {
            GameObject prefab = GetDeterministicPrefab(manager.BuildingPrefabs, block.GridCoordinate, 25001);
            if (prefab == null)
            {
                return null;
            }

            GameObject building = PrefabUtility.InstantiatePrefab(prefab, block.transform) as GameObject;
            if (building == null)
            {
                building = Object.Instantiate(prefab, block.transform);
            }

            building.name = block.EnterableBuildingCandidate ? "EnterableBuildingArt_" + prefab.name : "BuildingArt_" + prefab.name;
            building.transform.localPosition = manager.BuildingLocalOffset;
            building.transform.localScale = manager.BuildingLocalScale;
            return building.transform;
        }

        private static void AlignBuildingToRoad(ZedOverworldGenerationManager manager, Transform building, DirectionInfo roadDirection)
        {
            float size = Mathf.Max(8f, manager.BlockSize);
            Vector3 offsetFromRoad = -roadDirection.LocalDirection * size * 0.12f;

            building.localPosition = manager.BuildingLocalOffset + offsetFromRoad;
            building.localRotation = Quaternion.LookRotation(roadDirection.LocalDirection, Vector3.up);
        }

        private static void CreateLotPath(ZedOverworldGenerationManager manager, Transform root, DirectionInfo roadDirection)
        {
            float size = Mathf.Max(8f, manager.BlockSize);
            float pathLength = size * 0.34f;
            float pathWidth = size * 0.09f;
            Vector3 center = roadDirection.LocalDirection * size * 0.28f;

            GameObject path = CreateBox("BuildingLotPath_ToRoad", root, center, new Vector3(pathWidth, 0.08f, pathLength), manager.SidewalkDebugMaterial);
            path.transform.localRotation = Quaternion.LookRotation(roadDirection.LocalDirection, Vector3.up);
        }

        private static void CreateDoorMarker(ZedOverworldGenerationManager manager, ZedOverworldGeneratedBlock block, Transform root, DirectionInfo roadDirection)
        {
            float size = Mathf.Max(8f, manager.BlockSize);

            GameObject marker = CreateBox(
                DoorMarkerName,
                root,
                roadDirection.LocalDirection * size * 0.20f + Vector3.up * 1.0f,
                new Vector3(size * 0.14f, 1.8f, size * 0.06f),
                manager.EnterableDebugMaterial);

            marker.transform.localRotation = Quaternion.LookRotation(roadDirection.LocalDirection, Vector3.up);
        }

        private static void CreateEnterableMarker(ZedOverworldGenerationManager manager, ZedOverworldGeneratedBlock block, Transform root, DirectionInfo roadDirection)
        {
            float size = Mathf.Max(8f, manager.BlockSize);

            GameObject marker = CreateBox(
                EnterableMarkerName,
                root,
                roadDirection.LocalDirection * size * 0.36f + Vector3.up * 0.55f,
                new Vector3(size * 0.12f, 1.1f, size * 0.12f),
                manager.EnterableDebugMaterial);

            marker.transform.localRotation = Quaternion.LookRotation(roadDirection.LocalDirection, Vector3.up);

            ZedOverworldEnterableBuildingMarker enterable = marker.AddComponent<ZedOverworldEnterableBuildingMarker>();
            enterable.GridCoordinate = block.GridCoordinate;
            enterable.Seed = block.Seed;
            enterable.IsPortalCandidate = true;
        }

        private static DirectionInfo GetNearestRoadDirection(ZedOverworldGenerationManager manager, Vector2Int coord)
        {
            int roadEvery = Mathf.Max(1, manager.RoadEveryNBlocks);

            List<DirectionInfo> directions = new List<DirectionInfo>
            {
                new DirectionInfo(Vector3.forward, Mathf.Abs(NextRoadDistance(coord.y, roadEvery, 1))),
                new DirectionInfo(Vector3.back, Mathf.Abs(NextRoadDistance(coord.y, roadEvery, -1))),
                new DirectionInfo(Vector3.right, Mathf.Abs(NextRoadDistance(coord.x, roadEvery, 1))),
                new DirectionInfo(Vector3.left, Mathf.Abs(NextRoadDistance(coord.x, roadEvery, -1)))
            };

            directions.Sort((a, b) => a.Distance.CompareTo(b.Distance));
            return directions[0];
        }

        private static int NextRoadDistance(int coordinate, int roadEvery, int direction)
        {
            int distance = 0;
            int current = coordinate;

            while (distance < 100)
            {
                current += direction;
                distance++;

                if (current % roadEvery == 0)
                {
                    return distance;
                }
            }

            return 100;
        }

        private static GameObject CreateBox(string name, Transform parent, Vector3 localPosition, Vector3 scale, Material material)
        {
            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = name;
            box.transform.SetParent(parent, false);
            box.transform.localPosition = localPosition;
            box.transform.localRotation = Quaternion.identity;
            box.transform.localScale = scale;

            Renderer renderer = box.GetComponent<Renderer>();
            if (renderer != null && material != null)
            {
                renderer.sharedMaterial = material;
            }

            return box;
        }

        private static GameObject GetDeterministicPrefab(List<GameObject> prefabs, Vector2Int coord, int salt)
        {
            if (prefabs == null || prefabs.Count == 0)
            {
                return null;
            }

            int index = DeterministicInt(0, coord, salt, 0, prefabs.Count);
            for (int i = 0; i < prefabs.Count; i++)
            {
                GameObject prefab = prefabs[(index + i) % prefabs.Count];
                if (prefab != null)
                {
                    return prefab;
                }
            }

            return null;
        }

        private static int DeterministicInt(int seed, Vector2Int coord, int salt, int minInclusive, int maxExclusive)
        {
            if (maxExclusive <= minInclusive)
            {
                return minInclusive;
            }

            return minInclusive + Mathf.FloorToInt(RandomValue(seed, coord, salt) * (maxExclusive - minInclusive));
        }

        private static float RandomValue(int seed, Vector2Int coord, int salt)
        {
            unchecked
            {
                int hash = seed;
                hash = (hash * 397) ^ coord.x;
                hash = (hash * 397) ^ coord.y;
                hash = (hash * 397) ^ salt;
                hash ^= hash << 13;
                hash ^= hash >> 17;
                hash ^= hash << 5;
                uint value = (uint)hash;
                return (value % 1000000) / 1000000f;
            }
        }

        private static void ClearExistingBuildingPolish(Transform buildingLot)
        {
            Transform existing = buildingLot.Find(BuildingPolishRootName);
            if (existing != null)
            {
                Object.DestroyImmediate(existing.gameObject);
            }
        }

        private static void RebuildManagerBlockList(ZedOverworldGenerationManager manager)
        {
            manager.GeneratedBlocks.Clear();

            if (manager.GeneratedRoot == null)
            {
                GameObject root = GameObject.Find(ZedOverworldGenerationManager.GeneratedRootName);
                if (root != null)
                {
                    manager.GeneratedRoot = root.transform;
                }
            }

            if (manager.GeneratedRoot != null)
            {
                manager.GeneratedBlocks.AddRange(manager.GeneratedRoot.GetComponentsInChildren<ZedOverworldGeneratedBlock>(true));
            }
        }

        private static void SaveScene()
        {
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private struct DirectionInfo
        {
            public Vector3 LocalDirection;
            public int Distance;

            public DirectionInfo(Vector3 localDirection, int distance)
            {
                LocalDirection = localDirection;
                Distance = distance;
            }
        }
    }
}

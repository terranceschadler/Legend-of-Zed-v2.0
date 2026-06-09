using System.Collections.Generic;
using System.IO;
using LegendOfZed.Overworld;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LegendOfZed.Editor
{
    public static class ZedOverworldOneClickSetup
    {
        private const string OverworldScenePath = "Assets/LegendOfZed/Scenes/Zed_Overworld.unity";
        private const string InteriorScenePath = "Assets/LegendOfZed/Scenes/Zed_Interior_Test.unity";
        private const string ManagerName = "Zed_Overworld_Generation_Manager";
        private const string ParkPolishRootName = "Zed_ParkPolishRoot";
        private const string BuildingPolishRootName = "Zed_BuildingLotPolishRoot";
        private const string EnterableMarkerName = "EnterablePortalMarker";
        private const string DoorMarkerName = "DoorFacingRoadMarker";
        private const int MinimumEnterableBuildings = 2;

        [MenuItem("Legend of Zed/Setup/Overworld/One Click Build Current Overworld Test")]
        public static void OneClickBuildCurrentOverworldTest()
        {
            EnsureSceneFolder();

            Scene overworldScene = OpenOrCreateScene(OverworldScenePath);
            ZedOverworldGenerationManager manager = EnsureOverworldManager(overworldScene);

            AssignArt(manager);
            manager.GenerateNew();

            ApplyParkPolish(manager);
            GuaranteeEnterableCandidates(manager);
            ApplyBuildingLotPlacement(manager);
            AddReturnApplier(overworldScene);
            SaveCurrentScene();

            CreateInteriorTestScene();
            EditorSceneManager.OpenScene(OverworldScenePath, OpenSceneMode.Single);
            AddPortalTriggers();
            AddScenesToBuildSettings();

            SaveCurrentScene();

            Debug.Log("Overworld one-click setup complete. Open Zed_Overworld, press Play, walk to an enterable marker, press E, then return from Zed_Interior_Test.");
        }

        private static Scene OpenOrCreateScene(string path)
        {
            if (File.Exists(path))
            {
                return EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            }

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorSceneManager.SaveScene(scene, path);
            return scene;
        }

        private static ZedOverworldGenerationManager EnsureOverworldManager(Scene scene)
        {
            GameObject managerObject = GameObject.Find(ManagerName);
            if (managerObject == null)
            {
                managerObject = new GameObject(ManagerName);
                EditorSceneManager.MoveGameObjectToScene(managerObject, scene);
            }

            ZedOverworldGenerationManager manager = managerObject.GetComponent<ZedOverworldGenerationManager>();
            if (manager == null)
            {
                manager = managerObject.AddComponent<ZedOverworldGenerationManager>();
            }

            manager.OverworldSeed = 2200;
            manager.RandomizeSeedOnNewGame = false;
            manager.Width = 7;
            manager.Height = 7;
            manager.BlockSize = 24f;
            manager.ParkChance = 0.14f;
            manager.EnterableBuildingChance = 0.18f;
            manager.RoadEveryNBlocks = 2;
            manager.GenerateOnStart = true;
            manager.ReuseExistingGeneratedRoot = true;
            manager.PersistGeneratedRootAcrossScenes = true;
            manager.ClearBeforeGenerate = true;
            manager.BlockVisualMode = ZedOverworldGenerationManager.VisualMode.ArtPrefabsWithDebugFallback;
            manager.CreateDebugFallbackVisuals = true;
            manager.ParkTreeCount = 5;
            manager.ParkBushCount = 8;

            EnsureCamera(new Vector3(0f, 70f, -70f), Quaternion.Euler(55f, 0f, 0f));
            EnsureLight();

            EditorUtility.SetDirty(managerObject);
            EditorUtility.SetDirty(manager);
            return manager;
        }

        private static void AssignArt(ZedOverworldGenerationManager manager)
        {
            manager.RoadIntersectionPrefabs = FindPrefabs(
                new[] { "SM_Env_Road_Intersection", "Road_Intersection", "Road_Cross", "SM_Env_Road_Cross", "SM_Road_Intersection" },
                8);

            manager.RoadStraightPrefabs = FindPrefabs(
                new[] { "SM_Env_Road_Straight", "Road_Straight", "SM_Road_Straight", "Road" },
                12);

            manager.SidewalkPrefabs = FindPrefabs(
                new[] { "SM_Env_Sidewalk", "Sidewalk", "Pavement" },
                12);

            manager.BuildingPrefabs = FindPrefabs(
                new[] { "SM_Bld", "Building", "Shop", "House", "Store" },
                24);

            manager.ParkGroundPrefabs = FindPrefabs(
                new[] { "Grass", "Park", "Ground_Grass", "SM_Env_Grass" },
                8);

            manager.ParkTreePrefabs = FindPrefabs(
                new[] { "SM_Env_Tree", "Tree", "SM_Prop_Tree" },
                12);

            manager.ParkBushPrefabs = FindPrefabs(
                new[] { "SM_Env_Bush", "Bush", "Shrub", "SM_Prop_Bush" },
                12);

            EditorUtility.SetDirty(manager);
        }

        private static void ApplyParkPolish(ZedOverworldGenerationManager manager)
        {
            RebuildManagerBlockList(manager);

            for (int i = 0; i < manager.GeneratedBlocks.Count; i++)
            {
                ZedOverworldGeneratedBlock block = manager.GeneratedBlocks[i];
                if (block == null || block.BlockType != ZedOverworldBlockType.Park)
                {
                    continue;
                }

                ClearChild(block.transform, ParkPolishRootName);

                GameObject rootObject = new GameObject(ParkPolishRootName);
                rootObject.transform.SetParent(block.transform, false);
                rootObject.transform.localPosition = Vector3.zero;
                rootObject.transform.localRotation = Quaternion.identity;

                CreateParkPaths(manager, block, rootObject.transform);
                ScatterParkProps(manager, block, rootObject.transform, true);
                ScatterParkProps(manager, block, rootObject.transform, false);
            }
        }

        private static void CreateParkPaths(ZedOverworldGenerationManager manager, ZedOverworldGeneratedBlock block, Transform root)
        {
            float size = Mathf.Max(8f, manager.BlockSize);
            float pathWidth = Mathf.Max(1.2f, size * 0.16f);

            GameObject pathNS = CreateBox("ParkPath_NS_Debug", root, Vector3.up * 0.14f, new Vector3(pathWidth, 0.08f, size * 0.84f), manager.SidewalkDebugMaterial);
            pathNS.transform.localRotation = Quaternion.identity;

            GameObject pathEW = CreateBox("ParkPath_EW_Debug", root, Vector3.up * 0.15f, new Vector3(size * 0.84f, 0.08f, pathWidth), manager.SidewalkDebugMaterial);
            pathEW.transform.localRotation = Quaternion.identity;
        }

        private static void ScatterParkProps(ZedOverworldGenerationManager manager, ZedOverworldGeneratedBlock block, Transform root, bool trees)
        {
            List<GameObject> prefabs = trees ? manager.ParkTreePrefabs : manager.ParkBushPrefabs;
            int count = trees ? Mathf.Clamp(manager.ParkTreeCount, 0, 12) : Mathf.Clamp(manager.ParkBushCount, 0, 20);
            string prefix = trees ? "ParkTreePolished" : "ParkBushPolished";

            for (int i = 0; i < count; i++)
            {
                Vector3 pos = GetParkScatterPosition(manager, block.GridCoordinate, (trees ? 4000 : 5000) + i);
                GameObject prefab = GetDeterministicPrefab(prefabs, block.GridCoordinate, (trees ? 6000 : 7000) + i);

                if (prefab != null)
                {
                    GameObject instance = InstantiatePrefab(prefab, root, prefix);
                    instance.transform.localPosition = pos;
                    instance.transform.localRotation = Quaternion.Euler(0f, DeterministicInt(manager.OverworldSeed, block.GridCoordinate, 8000 + i, 0, 360), 0f);
                }
                else
                {
                    CreateBox(prefix + "_Fallback", root, pos + Vector3.up * 0.35f, trees ? new Vector3(1.2f, 1.2f, 1.2f) : new Vector3(1.0f, 0.5f, 1.0f), manager.ParkDebugMaterial);
                }
            }
        }

        private static Vector3 GetParkScatterPosition(ZedOverworldGenerationManager manager, Vector2Int coord, int salt)
        {
            float size = Mathf.Max(8f, manager.BlockSize);
            float max = size * 0.38f;
            float safe = size * 0.15f;

            for (int attempt = 0; attempt < 12; attempt++)
            {
                float x = DeterministicFloat(manager.OverworldSeed, coord, salt + attempt * 17, -max, max);
                float z = DeterministicFloat(manager.OverworldSeed, coord, salt + attempt * 17 + 1, -max, max);

                if (Mathf.Abs(x) > safe && Mathf.Abs(z) > safe)
                {
                    return new Vector3(x, 0f, z);
                }
            }

            return new Vector3(safe, 0f, safe);
        }

        private static void GuaranteeEnterableCandidates(ZedOverworldGenerationManager manager)
        {
            RebuildManagerBlockList(manager);

            List<ZedOverworldGeneratedBlock> lots = GetBuildingLots(manager);
            int current = CountEnterable(lots);
            if (current >= MinimumEnterableBuildings)
            {
                return;
            }

            lots.Sort((a, b) => DeterministicScore(manager.OverworldSeed, a.GridCoordinate).CompareTo(DeterministicScore(manager.OverworldSeed, b.GridCoordinate)));

            for (int i = 0; i < lots.Count && CountEnterable(lots) < MinimumEnterableBuildings; i++)
            {
                if (lots[i] == null || lots[i].EnterableBuildingCandidate)
                {
                    continue;
                }

                lots[i].EnterableBuildingCandidate = true;
                EditorUtility.SetDirty(lots[i]);
            }
        }

        private static void ApplyBuildingLotPlacement(ZedOverworldGenerationManager manager)
        {
            RebuildManagerBlockList(manager);

            for (int i = 0; i < manager.GeneratedBlocks.Count; i++)
            {
                ZedOverworldGeneratedBlock block = manager.GeneratedBlocks[i];
                if (block == null || block.BlockType != ZedOverworldBlockType.BuildingLot)
                {
                    continue;
                }

                ClearChild(block.transform, BuildingPolishRootName);
                DirectionInfo direction = GetNearestRoadDirection(manager, block.GridCoordinate);

                Transform building = FindExistingBuildingArt(block.transform);
                if (building != null)
                {
                    AlignBuildingToRoad(manager, building, direction);
                }

                GameObject rootObject = new GameObject(BuildingPolishRootName);
                rootObject.transform.SetParent(block.transform, false);
                rootObject.transform.localPosition = Vector3.zero;
                rootObject.transform.localRotation = Quaternion.identity;

                CreateLotPath(manager, rootObject.transform, direction);
                CreateDoorMarker(manager, rootObject.transform, direction);

                if (block.EnterableBuildingCandidate)
                {
                    CreateEnterableMarker(manager, block, rootObject.transform, direction);
                }
            }
        }

        private static Transform FindExistingBuildingArt(Transform block)
        {
            for (int i = 0; i < block.childCount; i++)
            {
                Transform child = block.GetChild(i);
                if (child == null || child.name == BuildingPolishRootName)
                {
                    continue;
                }

                if (child.name.StartsWith("BuildingArt_") ||
                    child.name.StartsWith("EnterableBuildingArt_") ||
                    child.name.Contains("Bld") ||
                    child.name.Contains("Building"))
                {
                    return child;
                }
            }

            return null;
        }

        private static void AlignBuildingToRoad(ZedOverworldGenerationManager manager, Transform building, DirectionInfo direction)
        {
            float size = Mathf.Max(8f, manager.BlockSize);
            building.localPosition = manager.BuildingLocalOffset - direction.LocalDirection * size * 0.12f;
            building.localRotation = Quaternion.LookRotation(direction.LocalDirection, Vector3.up);
        }

        private static void CreateLotPath(ZedOverworldGenerationManager manager, Transform root, DirectionInfo direction)
        {
            float size = Mathf.Max(8f, manager.BlockSize);
            GameObject path = CreateBox("BuildingLotPath_ToRoad", root, direction.LocalDirection * size * 0.28f, new Vector3(size * 0.09f, 0.08f, size * 0.34f), manager.SidewalkDebugMaterial);
            path.transform.localRotation = Quaternion.LookRotation(direction.LocalDirection, Vector3.up);
        }

        private static void CreateDoorMarker(ZedOverworldGenerationManager manager, Transform root, DirectionInfo direction)
        {
            float size = Mathf.Max(8f, manager.BlockSize);
            GameObject marker = CreateBox(DoorMarkerName, root, direction.LocalDirection * size * 0.20f + Vector3.up, new Vector3(size * 0.14f, 1.8f, size * 0.06f), manager.EnterableDebugMaterial);
            marker.transform.localRotation = Quaternion.LookRotation(direction.LocalDirection, Vector3.up);
        }

        private static void CreateEnterableMarker(ZedOverworldGenerationManager manager, ZedOverworldGeneratedBlock block, Transform root, DirectionInfo direction)
        {
            float size = Mathf.Max(8f, manager.BlockSize);
            GameObject marker = CreateBox(EnterableMarkerName, root, direction.LocalDirection * size * 0.36f + Vector3.up * 0.55f, new Vector3(size * 0.12f, 1.1f, size * 0.12f), manager.EnterableDebugMaterial);
            marker.transform.localRotation = Quaternion.LookRotation(direction.LocalDirection, Vector3.up);

            ZedOverworldEnterableBuildingMarker enterable = marker.AddComponent<ZedOverworldEnterableBuildingMarker>();
            enterable.GridCoordinate = block.GridCoordinate;
            enterable.Seed = block.Seed;
            enterable.IsPortalCandidate = true;
            enterable.InteriorSceneName = "Zed_Interior_Test";
        }

        private static void AddReturnApplier(Scene scene)
        {
            if (Object.FindAnyObjectByType<ZedOverworldReturnApplier>() != null)
            {
                return;
            }

            GameObject applierObject = new GameObject("Zed_Overworld_Return_Applier");
            applierObject.AddComponent<ZedOverworldReturnApplier>();
            EditorSceneManager.MoveGameObjectToScene(applierObject, scene);
        }

        private static void CreateInteriorTestScene()
        {
            Scene scene = OpenOrCreateScene(InteriorScenePath);

            EnsureCamera(new Vector3(0f, 14f, -12f), Quaternion.Euler(55f, 0f, 0f));
            EnsureLight();

            GameObject floor = GameObject.Find("Interior_Test_Floor");
            if (floor == null)
            {
                floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
                floor.name = "Interior_Test_Floor";
                floor.transform.position = Vector3.zero;
                floor.transform.localScale = new Vector3(12f, 0.2f, 12f);
            }

            GameObject returnPortal = GameObject.Find("Interior_Return_Portal");
            if (returnPortal == null)
            {
                returnPortal = GameObject.CreatePrimitive(PrimitiveType.Cube);
                returnPortal.name = "Interior_Return_Portal";
                returnPortal.transform.position = new Vector3(0f, 0.8f, -4f);
                returnPortal.transform.localScale = new Vector3(2f, 1.5f, 0.6f);
            }

            Collider collider = returnPortal.GetComponent<Collider>();
            collider.isTrigger = true;

            ZedInteriorReturnPortal portal = returnPortal.GetComponent<ZedInteriorReturnPortal>();
            if (portal == null)
            {
                portal = returnPortal.AddComponent<ZedInteriorReturnPortal>();
            }

            portal.OverworldSceneName = "Zed_Overworld";

            EditorUtility.SetDirty(returnPortal);
            EditorUtility.SetDirty(portal);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void AddPortalTriggers()
        {
            ZedOverworldGenerationManager manager = Object.FindAnyObjectByType<ZedOverworldGenerationManager>();
            ZedOverworldEnterableBuildingMarker[] markers = Object.FindObjectsByType<ZedOverworldEnterableBuildingMarker>(FindObjectsInactive.Include);

            for (int i = 0; i < markers.Length; i++)
            {
                ZedOverworldEnterableBuildingMarker marker = markers[i];
                if (marker == null)
                {
                    continue;
                }

                BoxCollider collider = marker.GetComponent<BoxCollider>();
                if (collider == null)
                {
                    collider = marker.gameObject.AddComponent<BoxCollider>();
                }

                collider.isTrigger = true;
                collider.size = new Vector3(2.5f, 2.2f, 2.5f);
                collider.center = Vector3.zero;

                ZedOverworldPortalTrigger portal = marker.GetComponent<ZedOverworldPortalTrigger>();
                if (portal == null)
                {
                    portal = marker.gameObject.AddComponent<ZedOverworldPortalTrigger>();
                }

                portal.PortalId = "Portal_" + marker.GridCoordinate.x + "_" + marker.GridCoordinate.y;
                portal.InteriorSceneName = "Zed_Interior_Test";
                portal.OverworldManager = manager;
                portal.PromptText = "Press E to enter building";

                EditorUtility.SetDirty(marker);
                EditorUtility.SetDirty(collider);
                EditorUtility.SetDirty(portal);
            }
        }

        private static List<ZedOverworldGeneratedBlock> GetBuildingLots(ZedOverworldGenerationManager manager)
        {
            List<ZedOverworldGeneratedBlock> lots = new List<ZedOverworldGeneratedBlock>();
            for (int i = 0; i < manager.GeneratedBlocks.Count; i++)
            {
                ZedOverworldGeneratedBlock block = manager.GeneratedBlocks[i];
                if (block != null && block.BlockType == ZedOverworldBlockType.BuildingLot)
                {
                    lots.Add(block);
                }
            }

            return lots;
        }

        private static int CountEnterable(List<ZedOverworldGeneratedBlock> lots)
        {
            int count = 0;
            for (int i = 0; i < lots.Count; i++)
            {
                if (lots[i] != null && lots[i].EnterableBuildingCandidate)
                {
                    count++;
                }
            }

            return count;
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

        private static List<GameObject> FindPrefabs(string[] searchTerms, int maxCount)
        {
            List<GameObject> results = new List<GameObject>();
            HashSet<string> seen = new HashSet<string>();

            for (int i = 0; i < searchTerms.Length; i++)
            {
                string[] guids = AssetDatabase.FindAssets(searchTerms[i] + " t:Prefab", new[] { "Assets" });

                for (int j = 0; j < guids.Length; j++)
                {
                    if (results.Count >= maxCount)
                    {
                        return results;
                    }

                    string path = AssetDatabase.GUIDToAssetPath(guids[j]);
                    if (string.IsNullOrEmpty(path) || seen.Contains(path) || path.Contains("/Editor/") || path.Contains("/Examples/"))
                    {
                        continue;
                    }

                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    if (prefab == null)
                    {
                        continue;
                    }

                    seen.Add(path);
                    results.Add(prefab);
                }
            }

            return results;
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

        private static GameObject InstantiatePrefab(GameObject prefab, Transform parent, string namePrefix)
        {
            GameObject instance = PrefabUtility.InstantiatePrefab(prefab, parent) as GameObject;
            if (instance == null)
            {
                instance = Object.Instantiate(prefab, parent);
            }

            instance.name = namePrefix + "_" + prefab.name;
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            return instance;
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

        private static float DeterministicFloat(int seed, Vector2Int coord, int salt, float min, float max)
        {
            return Mathf.Lerp(min, max, RandomValue(seed, coord, salt));
        }

        private static int DeterministicInt(int seed, Vector2Int coord, int salt, int minInclusive, int maxExclusive)
        {
            if (maxExclusive <= minInclusive)
            {
                return minInclusive;
            }

            return minInclusive + Mathf.FloorToInt(RandomValue(seed, coord, salt) * (maxExclusive - minInclusive));
        }

        private static int DeterministicScore(int seed, Vector2Int coord)
        {
            unchecked
            {
                int hash = seed;
                hash = (hash * 397) ^ coord.x;
                hash = (hash * 397) ^ coord.y;
                hash = (hash * 397) ^ 25050;
                hash ^= hash << 13;
                hash ^= hash >> 17;
                hash ^= hash << 5;
                return hash & int.MaxValue;
            }
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

        private static void ClearChild(Transform parent, string childName)
        {
            Transform existing = parent.Find(childName);
            if (existing != null)
            {
                Object.DestroyImmediate(existing.gameObject);
            }
        }

        private static void EnsureSceneFolder()
        {
            if (!AssetDatabase.IsValidFolder("Assets/LegendOfZed/Scenes"))
            {
                if (!AssetDatabase.IsValidFolder("Assets/LegendOfZed"))
                {
                    AssetDatabase.CreateFolder("Assets", "LegendOfZed");
                }

                AssetDatabase.CreateFolder("Assets/LegendOfZed", "Scenes");
            }
        }

        private static void EnsureCamera(Vector3 position, Quaternion rotation)
        {
            Camera camera = Object.FindAnyObjectByType<Camera>();
            if (camera == null)
            {
                GameObject cameraObject = new GameObject("Main Camera");
                camera = cameraObject.AddComponent<Camera>();
                camera.tag = "MainCamera";
            }

            camera.transform.position = position;
            camera.transform.rotation = rotation;
        }

        private static void EnsureLight()
        {
            Light light = Object.FindAnyObjectByType<Light>();
            if (light != null)
            {
                return;
            }

            GameObject lightObject = new GameObject("Directional Light");
            light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1f;
            light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        private static void AddScenesToBuildSettings()
        {
            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            AddSceneIfMissing(scenes, OverworldScenePath);
            AddSceneIfMissing(scenes, InteriorScenePath);
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private static void AddSceneIfMissing(List<EditorBuildSettingsScene> scenes, string path)
        {
            for (int i = 0; i < scenes.Count; i++)
            {
                if (scenes[i].path == path)
                {
                    scenes[i].enabled = true;
                    return;
                }
            }

            scenes.Add(new EditorBuildSettingsScene(path, true));
        }

        private static void SaveCurrentScene()
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

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LegendOfZed.Overworld
{
    [DisallowMultipleComponent]
    public class ZedOverworldGenerationManager : MonoBehaviour
    {
        public const string GeneratedRootName = "Zed_Seeded_Overworld_Root";

        public enum VisualMode
        {
            DebugOnly = 0,
            ArtPrefabsWithDebugFallback = 1
        }

        [Header("Seed")]
        public int OverworldSeed = 2200;
        public bool RandomizeSeedOnNewGame = false;

        [Header("Grid")]
        public int Width = 7;
        public int Height = 7;
        public float BlockSize = 24f;

        [Header("Block Rules")]
        [Range(0f, 1f)] public float ParkChance = 0.14f;
        [Range(0f, 1f)] public float EnterableBuildingChance = 0.18f;
        public int RoadEveryNBlocks = 2;

        [Header("Generation")]
        public bool GenerateOnStart = true;
        public bool ReuseExistingGeneratedRoot = true;
        public bool PersistGeneratedRootAcrossScenes = true;
        public bool ClearBeforeGenerate = true;

        [Header("Visual Mode")]
        public VisualMode BlockVisualMode = VisualMode.ArtPrefabsWithDebugFallback;
        public bool CreateDebugFallbackVisuals = true;

        [Header("Road / Sidewalk Art")]
        public List<GameObject> RoadIntersectionPrefabs = new List<GameObject>();
        public List<GameObject> RoadStraightPrefabs = new List<GameObject>();
        public List<GameObject> SidewalkPrefabs = new List<GameObject>();

        [Header("Building Art")]
        public List<GameObject> BuildingPrefabs = new List<GameObject>();
        public Vector3 BuildingLocalOffset = Vector3.zero;
        public Vector3 BuildingLocalScale = Vector3.one;

        [Header("Park Art")]
        public List<GameObject> ParkGroundPrefabs = new List<GameObject>();
        public List<GameObject> ParkTreePrefabs = new List<GameObject>();
        public List<GameObject> ParkBushPrefabs = new List<GameObject>();
        public int ParkTreeCount = 5;
        public int ParkBushCount = 8;

        [Header("Debug Materials")]
        public Material RoadDebugMaterial;
        public Material SidewalkDebugMaterial;
        public Material BuildingLotDebugMaterial;
        public Material ParkDebugMaterial;
        public Material EnterableDebugMaterial;

        [Header("Runtime State")]
        public Transform GeneratedRoot;
        public List<ZedOverworldGeneratedBlock> GeneratedBlocks = new List<ZedOverworldGeneratedBlock>();

        private static Transform _persistentGeneratedRoot;
        private static int _persistentSeed;
        private static bool _hasPersistentSeed;

        private void Start()
        {
            if (GenerateOnStart)
            {
                GenerateOrReuse();
            }
        }

        [ContextMenu("Generate Or Reuse")]
        public void GenerateOrReuse()
        {
            if (ReuseExistingGeneratedRoot && TryReuseExistingRoot())
            {
                return;
            }

            GenerateNew();
        }

        [ContextMenu("Generate New")]
        public void GenerateNew()
        {
            if (RandomizeSeedOnNewGame)
            {
                OverworldSeed = Random.Range(int.MinValue, int.MaxValue);
            }

            _persistentSeed = OverworldSeed;
            _hasPersistentSeed = true;

            if (ClearBeforeGenerate)
            {
                ClearGenerated();
            }

            GameObject rootObject = new GameObject(GeneratedRootName);
            rootObject.transform.position = Vector3.zero;
            GeneratedRoot = rootObject.transform;
            _persistentGeneratedRoot = GeneratedRoot;

            if (PersistGeneratedRootAcrossScenes && Application.isPlaying)
            {
                DontDestroyOnLoad(rootObject);
            }

            GeneratedBlocks.Clear();
            GenerateLayout();

            Debug.Log("Zed seeded overworld generated. Seed=" + OverworldSeed + " Blocks=" + GeneratedBlocks.Count + " Scene=" + SceneManager.GetActiveScene().name, this);
        }

        [ContextMenu("Clear Generated")]
        public void ClearGenerated()
        {
            GeneratedBlocks.Clear();

            if (GeneratedRoot != null)
            {
                DestroySafely(GeneratedRoot.gameObject);
            }

            if (_persistentGeneratedRoot != null)
            {
                DestroySafely(_persistentGeneratedRoot.gameObject);
            }

            GameObject existing = GameObject.Find(GeneratedRootName);
            if (existing != null)
            {
                DestroySafely(existing);
            }

            GeneratedRoot = null;
            _persistentGeneratedRoot = null;
        }

        public bool TryReuseExistingRoot()
        {
            if (_persistentGeneratedRoot != null)
            {
                GeneratedRoot = _persistentGeneratedRoot;
                if (_hasPersistentSeed)
                {
                    OverworldSeed = _persistentSeed;
                }

                RebuildGeneratedBlockList();
                if (GeneratedBlocks.Count > 0)
                {
                    Debug.Log("Reused persistent seeded overworld root. Seed=" + OverworldSeed + " Blocks=" + GeneratedBlocks.Count, this);
                    return true;
                }

                ClearGenerated();
                return false;
            }

            GameObject existing = GameObject.Find(GeneratedRootName);
            if (existing != null)
            {
                GeneratedRoot = existing.transform;
                _persistentGeneratedRoot = GeneratedRoot;
                if (_hasPersistentSeed)
                {
                    OverworldSeed = _persistentSeed;
                }

                RebuildGeneratedBlockList();
                if (GeneratedBlocks.Count > 0)
                {
                    Debug.Log("Reused existing seeded overworld root. Seed=" + OverworldSeed + " Blocks=" + GeneratedBlocks.Count, this);
                    return true;
                }

                ClearGenerated();
                return false;
            }

            return false;
        }

        private void GenerateLayout()
        {
            int width = Mathf.Max(3, Width);
            int height = Mathf.Max(3, Height);
            float size = Mathf.Max(8f, BlockSize);
            int roadEvery = Mathf.Max(1, RoadEveryNBlocks);

            Vector2 offset = new Vector2((width - 1) * size * 0.5f, (height - 1) * size * 0.5f);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Vector2Int coord = new Vector2Int(x, y);
                    Vector3 world = new Vector3(x * size - offset.x, 0f, y * size - offset.y);
                    ZedOverworldBlockType type = ResolveBlockType(coord, roadEvery);
                    bool enterable = type == ZedOverworldBlockType.BuildingLot && RandomValue(coord, 31) < EnterableBuildingChance;
                    CreateBlock(coord, world, type, enterable);
                }
            }
        }

        private ZedOverworldBlockType ResolveBlockType(Vector2Int coord, int roadEvery)
        {
            if (coord.x % roadEvery == 0 || coord.y % roadEvery == 0)
            {
                return ZedOverworldBlockType.Road;
            }

            if (RandomValue(coord, 11) < ParkChance)
            {
                return ZedOverworldBlockType.Park;
            }

            return ZedOverworldBlockType.BuildingLot;
        }

        private void CreateBlock(Vector2Int coord, Vector3 position, ZedOverworldBlockType type, bool enterable)
        {
            GameObject blockObject = new GameObject("Block_" + coord.x + "_" + coord.y + "_" + type);
            blockObject.transform.SetParent(GeneratedRoot, false);
            blockObject.transform.position = position;

            ZedOverworldGeneratedBlock block = blockObject.AddComponent<ZedOverworldGeneratedBlock>();
            block.GridCoordinate = coord;
            block.BlockType = type;
            block.Seed = OverworldSeed;
            block.EnterableBuildingCandidate = enterable;
            GeneratedBlocks.Add(block);

            bool usedArt = false;
            if (BlockVisualMode == VisualMode.ArtPrefabsWithDebugFallback)
            {
                usedArt = CreateArtVisual(blockObject.transform, coord, type, enterable);
            }

            if (!usedArt && CreateDebugFallbackVisuals)
            {
                CreateDebugVisual(blockObject.transform, type, enterable);
            }
        }

        private bool CreateArtVisual(Transform parent, Vector2Int coord, ZedOverworldBlockType type, bool enterable)
        {
            if (type == ZedOverworldBlockType.Road)
            {
                return CreateRoadArt(parent, coord);
            }

            if (type == ZedOverworldBlockType.Park)
            {
                return CreateParkArt(parent, coord);
            }

            return CreateBuildingLotArt(parent, coord, enterable);
        }

        private bool CreateRoadArt(Transform parent, Vector2Int coord)
        {
            bool usedAny = false;
            GameObject roadPrefab = GetDeterministicPrefab(RoadIntersectionPrefabs.Count > 0 ? RoadIntersectionPrefabs : RoadStraightPrefabs, coord, 101);
            if (roadPrefab != null)
            {
                GameObject road = InstantiatePrefab(roadPrefab, parent, "RoadArt");
                road.transform.localPosition = Vector3.zero;
                usedAny = true;
            }

            if (SidewalkPrefabs.Count > 0)
            {
                float size = Mathf.Max(8f, BlockSize);
                Vector3[] positions =
                {
                    new Vector3(0f, 0f, size * 0.46f),
                    new Vector3(0f, 0f, -size * 0.46f),
                    new Vector3(size * 0.46f, 0f, 0f),
                    new Vector3(-size * 0.46f, 0f, 0f)
                };

                Vector3[] rotations =
                {
                    Vector3.zero,
                    new Vector3(0f, 180f, 0f),
                    new Vector3(0f, 90f, 0f),
                    new Vector3(0f, -90f, 0f)
                };

                for (int i = 0; i < positions.Length; i++)
                {
                    GameObject sidewalkPrefab = GetDeterministicPrefab(SidewalkPrefabs, coord, 200 + i);
                    if (sidewalkPrefab == null)
                    {
                        continue;
                    }

                    GameObject sidewalk = InstantiatePrefab(sidewalkPrefab, parent, "SidewalkArt");
                    sidewalk.transform.localPosition = positions[i];
                    sidewalk.transform.localRotation = Quaternion.Euler(rotations[i]);
                    usedAny = true;
                }
            }

            return usedAny;
        }

        private bool CreateBuildingLotArt(Transform parent, Vector2Int coord, bool enterable)
        {
            GameObject buildingPrefab = GetDeterministicPrefab(BuildingPrefabs, coord, 301);
            if (buildingPrefab == null)
            {
                return false;
            }

            GameObject building = InstantiatePrefab(buildingPrefab, parent, enterable ? "EnterableBuildingArt" : "BuildingArt");
            building.transform.localPosition = BuildingLocalOffset;
            building.transform.localRotation = Quaternion.Euler(0f, DeterministicInt(coord, 302, 0, 4) * 90f, 0f);
            building.transform.localScale = BuildingLocalScale;

            if (enterable)
            {
                GameObject marker = new GameObject("EnterablePortalCandidate");
                marker.transform.SetParent(parent, false);
                marker.transform.localPosition = new Vector3(0f, 0f, -Mathf.Max(8f, BlockSize) * 0.38f);
            }

            return true;
        }

        private bool CreateParkArt(Transform parent, Vector2Int coord)
        {
            bool usedAny = false;
            GameObject groundPrefab = GetDeterministicPrefab(ParkGroundPrefabs, coord, 401);
            if (groundPrefab != null)
            {
                GameObject ground = InstantiatePrefab(groundPrefab, parent, "ParkGroundArt");
                ground.transform.localPosition = Vector3.zero;
                usedAny = true;
            }

            float size = Mathf.Max(8f, BlockSize);
            int treeCount = Mathf.Max(0, ParkTreeCount);
            for (int i = 0; i < treeCount; i++)
            {
                GameObject treePrefab = GetDeterministicPrefab(ParkTreePrefabs, coord, 500 + i);
                if (treePrefab == null)
                {
                    continue;
                }

                Vector3 pos = DeterministicLocalParkPosition(coord, 600 + i, size);
                GameObject tree = InstantiatePrefab(treePrefab, parent, "ParkTreeArt");
                tree.transform.localPosition = pos;
                tree.transform.localRotation = Quaternion.Euler(0f, DeterministicInt(coord, 700 + i, 0, 360), 0f);
                usedAny = true;
            }

            int bushCount = Mathf.Max(0, ParkBushCount);
            for (int i = 0; i < bushCount; i++)
            {
                GameObject bushPrefab = GetDeterministicPrefab(ParkBushPrefabs, coord, 800 + i);
                if (bushPrefab == null)
                {
                    continue;
                }

                Vector3 pos = DeterministicLocalParkPosition(coord, 900 + i, size);
                GameObject bush = InstantiatePrefab(bushPrefab, parent, "ParkBushArt");
                bush.transform.localPosition = pos;
                bush.transform.localRotation = Quaternion.Euler(0f, DeterministicInt(coord, 1000 + i, 0, 360), 0f);
                usedAny = true;
            }

            return usedAny;
        }

        private void CreateDebugVisual(Transform parent, ZedOverworldBlockType type, bool enterable)
        {
            float size = Mathf.Max(8f, BlockSize);
            float roadWidth = size * 0.42f;
            float sidewalkWidth = size * 0.16f;

            if (type == ZedOverworldBlockType.Road)
            {
                CreateBox("Road_DebugSurface", parent, Vector3.up * 0.02f, new Vector3(size, 0.08f, roadWidth), RoadDebugMaterial);
                CreateBox("Road_Cross_DebugSurface", parent, Vector3.up * 0.02f, new Vector3(roadWidth, 0.08f, size), RoadDebugMaterial);
                CreateBox("Sidewalk_Debug_N", parent, new Vector3(0f, 0.08f, size * 0.5f - sidewalkWidth * 0.5f), new Vector3(size, 0.08f, sidewalkWidth), SidewalkDebugMaterial);
                CreateBox("Sidewalk_Debug_S", parent, new Vector3(0f, 0.08f, -size * 0.5f + sidewalkWidth * 0.5f), new Vector3(size, 0.08f, sidewalkWidth), SidewalkDebugMaterial);
                CreateBox("Sidewalk_Debug_E", parent, new Vector3(size * 0.5f - sidewalkWidth * 0.5f, 0.08f, 0f), new Vector3(sidewalkWidth, 0.08f, size), SidewalkDebugMaterial);
                CreateBox("Sidewalk_Debug_W", parent, new Vector3(-size * 0.5f + sidewalkWidth * 0.5f, 0.08f, 0f), new Vector3(sidewalkWidth, 0.08f, size), SidewalkDebugMaterial);
            }
            else if (type == ZedOverworldBlockType.Park)
            {
                CreateBox("Park_DebugSurface", parent, Vector3.up * 0.04f, new Vector3(size * 0.92f, 0.08f, size * 0.92f), ParkDebugMaterial);
                CreateBox("Park_Path_Debug_NS", parent, Vector3.up * 0.09f, new Vector3(size * 0.16f, 0.08f, size * 0.92f), SidewalkDebugMaterial);
                CreateBox("Park_Path_Debug_EW", parent, Vector3.up * 0.10f, new Vector3(size * 0.92f, 0.08f, size * 0.16f), SidewalkDebugMaterial);
            }
            else
            {
                Material material = enterable && EnterableDebugMaterial != null ? EnterableDebugMaterial : BuildingLotDebugMaterial;
                CreateBox("BuildingLot_DebugSurface", parent, Vector3.up * 0.05f, new Vector3(size * 0.86f, 0.08f, size * 0.86f), material);

                if (enterable)
                {
                    CreateBox("EnterableBuildingMarker_Debug", parent, new Vector3(0f, 1.1f, -size * 0.25f), new Vector3(size * 0.22f, 2f, size * 0.12f), EnterableDebugMaterial);
                }
            }
        }

        private GameObject InstantiatePrefab(GameObject prefab, Transform parent, string namePrefix)
        {
            GameObject instance = Instantiate(prefab, parent);
            instance.name = namePrefix + "_" + prefab.name;
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            return instance;
        }

        private GameObject CreateBox(string name, Transform parent, Vector3 localPosition, Vector3 scale, Material material)
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

        private GameObject GetDeterministicPrefab(List<GameObject> prefabs, Vector2Int coord, int salt)
        {
            if (prefabs == null || prefabs.Count == 0)
            {
                return null;
            }

            int index = DeterministicInt(coord, salt, 0, prefabs.Count);
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

        private Vector3 DeterministicLocalParkPosition(Vector2Int coord, int salt, float size)
        {
            float range = size * 0.34f;
            float x = DeterministicFloat(coord, salt, -range, range);
            float z = DeterministicFloat(coord, salt + 1, -range, range);
            return new Vector3(x, 0f, z);
        }

        private int DeterministicInt(Vector2Int coord, int salt, int minInclusive, int maxExclusive)
        {
            if (maxExclusive <= minInclusive)
            {
                return minInclusive;
            }

            float value = RandomValue(coord, salt);
            return minInclusive + Mathf.FloorToInt(value * (maxExclusive - minInclusive));
        }

        private float DeterministicFloat(Vector2Int coord, int salt, float min, float max)
        {
            return Mathf.Lerp(min, max, RandomValue(coord, salt));
        }

        private float RandomValue(Vector2Int coord, int salt)
        {
            unchecked
            {
                int hash = OverworldSeed;
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

        private void RebuildGeneratedBlockList()
        {
            GeneratedBlocks.Clear();
            if (GeneratedRoot == null)
            {
                return;
            }

            GeneratedBlocks.AddRange(GeneratedRoot.GetComponentsInChildren<ZedOverworldGeneratedBlock>(true));
        }

        private static void DestroySafely(GameObject target)
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
    }
}

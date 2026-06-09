using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LegendOfZed.Overworld
{
    [DisallowMultipleComponent]
    public class ZedOverworldGenerationManager : MonoBehaviour
    {
        public const string GeneratedRootName = "Zed_Seeded_Overworld_Root";

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

        [Header("Debug Foundation Visuals")]
        public bool CreateDebugBlockVisuals = true;
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

            if (CreateDebugBlockVisuals)
            {
                CreateDebugVisual(blockObject.transform, type, enterable);
            }
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

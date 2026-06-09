using System.Collections.Generic;
using LegendOfZed.Overworld;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LegendOfZed.Editor
{
    public static class ZedV24ParkBlockPolishSetup
    {
        private const string ParkPolishRootName = "Zed_ParkPolishRoot";
        private const float PathWidthRatio = 0.16f;
        private const float SafePathHalfWidthRatio = 0.15f;

        [MenuItem("Legend of Zed/Setup/v2.4 Apply Park Block Polish")]
        public static void ApplyParkBlockPolish()
        {
            ZedOverworldGenerationManager manager = Object.FindAnyObjectByType<ZedOverworldGenerationManager>();
            if (manager == null)
            {
                Debug.LogWarning("No ZedOverworldGenerationManager found. Open Zed_Overworld and run v2.3 rebuild first.");
                return;
            }

            if (manager.GeneratedBlocks == null || manager.GeneratedBlocks.Count == 0)
            {
                RebuildManagerBlockList(manager);
            }

            int parkCount = 0;
            int pathCount = 0;
            int treeCount = 0;
            int bushCount = 0;

            for (int i = 0; i < manager.GeneratedBlocks.Count; i++)
            {
                ZedOverworldGeneratedBlock block = manager.GeneratedBlocks[i];
                if (block == null || block.BlockType != ZedOverworldBlockType.Park)
                {
                    continue;
                }

                parkCount++;
                ClearExistingParkPolish(block.transform);

                GameObject polishRoot = new GameObject(ParkPolishRootName);
                polishRoot.transform.SetParent(block.transform, false);
                polishRoot.transform.localPosition = Vector3.zero;
                polishRoot.transform.localRotation = Quaternion.identity;

                pathCount += CreateParkPaths(manager, block, polishRoot.transform);
                treeCount += ScatterParkTrees(manager, block, polishRoot.transform);
                bushCount += ScatterParkBushes(manager, block, polishRoot.transform);

                EditorUtility.SetDirty(block);
                EditorUtility.SetDirty(polishRoot);
            }

            SaveScene();

            Debug.Log(
                "v2.4 park block polish applied. Parks=" + parkCount +
                " Paths=" + pathCount +
                " Trees=" + treeCount +
                " Bushes=" + bushCount + ".");
        }

        [MenuItem("Legend of Zed/Setup/v2.4 Clear Park Block Polish")]
        public static void ClearParkBlockPolish()
        {
            ZedOverworldGeneratedBlock[] blocks = Object.FindObjectsByType<ZedOverworldGeneratedBlock>(FindObjectsInactive.Include);

            int cleared = 0;
            for (int i = 0; i < blocks.Length; i++)
            {
                if (blocks[i] == null || blocks[i].BlockType != ZedOverworldBlockType.Park)
                {
                    continue;
                }

                Transform existing = blocks[i].transform.Find(ParkPolishRootName);
                if (existing != null)
                {
                    Object.DestroyImmediate(existing.gameObject);
                    cleared++;
                }
            }

            SaveScene();
            Debug.Log("v2.4 cleared park block polish roots. Cleared=" + cleared + ".");
        }

        [MenuItem("Legend of Zed/Setup/v2.4 Validate Park Block Polish")]
        public static void ValidateParkBlockPolish()
        {
            ZedOverworldGeneratedBlock[] blocks = Object.FindObjectsByType<ZedOverworldGeneratedBlock>(FindObjectsInactive.Include);

            int parks = 0;
            int polished = 0;
            int missing = 0;

            for (int i = 0; i < blocks.Length; i++)
            {
                if (blocks[i] == null || blocks[i].BlockType != ZedOverworldBlockType.Park)
                {
                    continue;
                }

                parks++;
                if (blocks[i].transform.Find(ParkPolishRootName) != null)
                {
                    polished++;
                }
                else
                {
                    missing++;
                }
            }

            if (parks == 0)
            {
                Debug.LogWarning("v2.4 validation: no park blocks found in the generated overworld.");
                return;
            }

            if (missing == 0)
            {
                Debug.Log("v2.4 park block polish validation passed. Parks=" + parks + " Polished=" + polished + ".");
            }
            else
            {
                Debug.LogWarning("v2.4 park block polish validation found unpolished parks. Parks=" + parks + " Polished=" + polished + " Missing=" + missing + ".");
            }
        }

        private static int CreateParkPaths(ZedOverworldGenerationManager manager, ZedOverworldGeneratedBlock block, Transform root)
        {
            float size = Mathf.Max(8f, manager.BlockSize);
            float pathWidth = Mathf.Max(1.2f, size * PathWidthRatio);

            int created = 0;

            GameObject northSouth = TryCreatePathPrefab(manager, block, root, "ParkPath_NS", Vector3.zero, Quaternion.identity);
            if (northSouth == null)
            {
                northSouth = CreateBox("ParkPath_NS_Debug", root, Vector3.up * 0.14f, new Vector3(pathWidth, 0.08f, size * 0.84f), manager.SidewalkDebugMaterial);
            }
            else
            {
                northSouth.transform.localScale = Vector3.one;
            }

            created++;

            GameObject eastWest = TryCreatePathPrefab(manager, block, root, "ParkPath_EW", Vector3.zero, Quaternion.Euler(0f, 90f, 0f));
            if (eastWest == null)
            {
                eastWest = CreateBox("ParkPath_EW_Debug", root, Vector3.up * 0.15f, new Vector3(size * 0.84f, 0.08f, pathWidth), manager.SidewalkDebugMaterial);
            }
            else
            {
                eastWest.transform.localScale = Vector3.one;
            }

            created++;
            return created;
        }

        private static int ScatterParkTrees(ZedOverworldGenerationManager manager, ZedOverworldGeneratedBlock block, Transform root)
        {
            int requested = Mathf.Clamp(manager.ParkTreeCount, 0, 12);
            int created = 0;

            for (int i = 0; i < requested; i++)
            {
                GameObject prefab = GetDeterministicPrefab(manager.ParkTreePrefabs, block.GridCoordinate, 2400 + i);
                if (prefab == null)
                {
                    GameObject marker = CreateTreeFallback(root, block, manager, i);
                    if (marker != null)
                    {
                        created++;
                    }

                    continue;
                }

                GameObject tree = InstantiatePrefab(prefab, root, "ParkTreePolished");
                tree.transform.localPosition = GetParkScatterPosition(manager, block.GridCoordinate, 2500 + i);
                tree.transform.localRotation = Quaternion.Euler(0f, DeterministicInt(manager.OverworldSeed, block.GridCoordinate, 2600 + i, 0, 360), 0f);
                created++;
            }

            return created;
        }

        private static int ScatterParkBushes(ZedOverworldGenerationManager manager, ZedOverworldGeneratedBlock block, Transform root)
        {
            int requested = Mathf.Clamp(manager.ParkBushCount, 0, 20);
            int created = 0;

            for (int i = 0; i < requested; i++)
            {
                GameObject prefab = GetDeterministicPrefab(manager.ParkBushPrefabs, block.GridCoordinate, 2700 + i);
                if (prefab == null)
                {
                    GameObject marker = CreateBushFallback(root, block, manager, i);
                    if (marker != null)
                    {
                        created++;
                    }

                    continue;
                }

                GameObject bush = InstantiatePrefab(prefab, root, "ParkBushPolished");
                bush.transform.localPosition = GetParkScatterPosition(manager, block.GridCoordinate, 2800 + i);
                bush.transform.localRotation = Quaternion.Euler(0f, DeterministicInt(manager.OverworldSeed, block.GridCoordinate, 2900 + i, 0, 360), 0f);
                created++;
            }

            return created;
        }

        private static GameObject TryCreatePathPrefab(ZedOverworldGenerationManager manager, ZedOverworldGeneratedBlock block, Transform root, string name, Vector3 localPosition, Quaternion localRotation)
        {
            GameObject prefab = GetDeterministicPrefab(manager.SidewalkPrefabs, block.GridCoordinate, name.GetHashCode());
            if (prefab == null)
            {
                return null;
            }

            GameObject instance = InstantiatePrefab(prefab, root, name);
            instance.transform.localPosition = localPosition;
            instance.transform.localRotation = localRotation;
            return instance;
        }

        private static Vector3 GetParkScatterPosition(ZedOverworldGenerationManager manager, Vector2Int coord, int salt)
        {
            float size = Mathf.Max(8f, manager.BlockSize);
            float max = size * 0.38f;
            float safePathHalfWidth = size * SafePathHalfWidthRatio;

            for (int attempt = 0; attempt < 12; attempt++)
            {
                float x = DeterministicFloat(manager.OverworldSeed, coord, salt + attempt * 17, -max, max);
                float z = DeterministicFloat(manager.OverworldSeed, coord, salt + attempt * 17 + 1, -max, max);

                bool clearOfVerticalPath = Mathf.Abs(x) > safePathHalfWidth;
                bool clearOfHorizontalPath = Mathf.Abs(z) > safePathHalfWidth;

                if (clearOfVerticalPath && clearOfHorizontalPath)
                {
                    return new Vector3(x, 0f, z);
                }
            }

            // Fallback into one quadrant if deterministic retries land near the cross path.
            float fallbackX = DeterministicFloat(manager.OverworldSeed, coord, salt + 300, safePathHalfWidth, max);
            float fallbackZ = DeterministicFloat(manager.OverworldSeed, coord, salt + 301, safePathHalfWidth, max);
            if (DeterministicInt(manager.OverworldSeed, coord, salt + 302, 0, 2) == 0)
            {
                fallbackX *= -1f;
            }

            if (DeterministicInt(manager.OverworldSeed, coord, salt + 303, 0, 2) == 0)
            {
                fallbackZ *= -1f;
            }

            return new Vector3(fallbackX, 0f, fallbackZ);
        }

        private static GameObject CreateTreeFallback(Transform root, ZedOverworldGeneratedBlock block, ZedOverworldGenerationManager manager, int index)
        {
            GameObject trunk = CreateBox("ParkTreeFallback_Trunk", root, Vector3.zero, new Vector3(0.35f, 1.4f, 0.35f), null);
            trunk.transform.localPosition = GetParkScatterPosition(manager, block.GridCoordinate, 3100 + index);

            GameObject top = CreateBox("ParkTreeFallback_Top", trunk.transform, new Vector3(0f, 0.95f, 0f), new Vector3(1.3f, 1.3f, 1.3f), manager.ParkDebugMaterial);
            top.transform.localRotation = Quaternion.identity;
            return trunk;
        }

        private static GameObject CreateBushFallback(Transform root, ZedOverworldGeneratedBlock block, ZedOverworldGenerationManager manager, int index)
        {
            GameObject bush = CreateBox("ParkBushFallback", root, Vector3.zero, new Vector3(1.1f, 0.55f, 1.1f), manager.ParkDebugMaterial);
            bush.transform.localPosition = GetParkScatterPosition(manager, block.GridCoordinate, 3300 + index);
            return bush;
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

        private static void ClearExistingParkPolish(Transform parkBlock)
        {
            Transform existing = parkBlock.Find(ParkPolishRootName);
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
    }
}

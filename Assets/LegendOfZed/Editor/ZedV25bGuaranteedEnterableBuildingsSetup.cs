using System.Collections.Generic;
using LegendOfZed.Overworld;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LegendOfZed.Editor
{
    public static class ZedV25bGuaranteedEnterableBuildingsSetup
    {
        private const int MinimumEnterableBuildings = 2;

        [MenuItem("Legend of Zed/Setup/v2.5b Guarantee Enterable Building Candidates")]
        public static void GuaranteeEnterableBuildingCandidates()
        {
            ZedOverworldGenerationManager manager = Object.FindAnyObjectByType<ZedOverworldGenerationManager>();
            if (manager == null)
            {
                Debug.LogWarning("No ZedOverworldGenerationManager found. Open Zed_Overworld and run v2.3 rebuild first.");
                return;
            }

            List<ZedOverworldGeneratedBlock> buildingLots = GetBuildingLots(manager);
            if (buildingLots.Count == 0)
            {
                Debug.LogWarning("v2.5b could not find any building lots to promote.");
                return;
            }

            int before = CountEnterableCandidates(buildingLots);
            int promoted = 0;

            if (before < MinimumEnterableBuildings)
            {
                buildingLots.Sort((a, b) =>
                {
                    int aScore = DeterministicScore(manager.OverworldSeed, a.GridCoordinate);
                    int bScore = DeterministicScore(manager.OverworldSeed, b.GridCoordinate);
                    return aScore.CompareTo(bScore);
                });

                for (int i = 0; i < buildingLots.Count && CountEnterableCandidates(buildingLots) < MinimumEnterableBuildings; i++)
                {
                    ZedOverworldGeneratedBlock block = buildingLots[i];
                    if (block == null || block.EnterableBuildingCandidate)
                    {
                        continue;
                    }

                    block.EnterableBuildingCandidate = true;
                    EditorUtility.SetDirty(block);
                    promoted++;
                }
            }

            SaveScene();

            Debug.Log(
                "v2.5b guaranteed enterable building candidates. " +
                "Lots=" + buildingLots.Count +
                " Before=" + before +
                " Promoted=" + promoted +
                " After=" + CountEnterableCandidates(buildingLots) +
                ". Now run v2.5 Apply Building Lot Placement And Enterable Markers.");
        }

        [MenuItem("Legend of Zed/Setup/v2.5b Guarantee And Reapply Building Markers")]
        public static void GuaranteeAndReapplyBuildingMarkers()
        {
            GuaranteeEnterableBuildingCandidates();
            ZedV25BuildingLotPlacementSetup.ApplyBuildingLotPlacement();
            ZedV25BuildingLotPlacementSetup.ValidateBuildingLotPlacement();
        }

        [MenuItem("Legend of Zed/Setup/v2.5b Validate Enterable Building Candidates")]
        public static void ValidateEnterableBuildingCandidates()
        {
            ZedOverworldGenerationManager manager = Object.FindAnyObjectByType<ZedOverworldGenerationManager>();
            if (manager == null)
            {
                Debug.LogWarning("No ZedOverworldGenerationManager found.");
                return;
            }

            List<ZedOverworldGeneratedBlock> buildingLots = GetBuildingLots(manager);
            int candidates = CountEnterableCandidates(buildingLots);
            int markers = Object.FindObjectsByType<ZedOverworldEnterableBuildingMarker>(FindObjectsInactive.Include).Length;

            if (candidates >= MinimumEnterableBuildings && markers >= MinimumEnterableBuildings)
            {
                Debug.Log(
                    "v2.5b enterable building validation passed. " +
                    "Lots=" + buildingLots.Count +
                    " Candidates=" + candidates +
                    " Markers=" + markers + ".");
            }
            else
            {
                Debug.LogWarning(
                    "v2.5b enterable building validation warning. " +
                    "Lots=" + buildingLots.Count +
                    " Candidates=" + candidates +
                    " Markers=" + markers +
                    ". Run v2.5b Guarantee And Reapply Building Markers.");
            }
        }

        private static List<ZedOverworldGeneratedBlock> GetBuildingLots(ZedOverworldGenerationManager manager)
        {
            if (manager.GeneratedBlocks == null)
            {
                manager.GeneratedBlocks = new List<ZedOverworldGeneratedBlock>();
            }

            if (manager.GeneratedBlocks.Count == 0)
            {
                RebuildManagerBlockList(manager);
            }

            List<ZedOverworldGeneratedBlock> result = new List<ZedOverworldGeneratedBlock>();
            for (int i = 0; i < manager.GeneratedBlocks.Count; i++)
            {
                ZedOverworldGeneratedBlock block = manager.GeneratedBlocks[i];
                if (block != null && block.BlockType == ZedOverworldBlockType.BuildingLot)
                {
                    result.Add(block);
                }
            }

            return result;
        }

        private static int CountEnterableCandidates(List<ZedOverworldGeneratedBlock> buildingLots)
        {
            int count = 0;
            for (int i = 0; i < buildingLots.Count; i++)
            {
                if (buildingLots[i] != null && buildingLots[i].EnterableBuildingCandidate)
                {
                    count++;
                }
            }

            return count;
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

        private static void SaveScene()
        {
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}

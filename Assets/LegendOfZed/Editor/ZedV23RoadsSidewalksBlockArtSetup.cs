using System.Collections.Generic;
using System.IO;
using LegendOfZed.Overworld;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LegendOfZed.Editor
{
    public static class ZedV23RoadsSidewalksBlockArtSetup
    {
        [MenuItem("Legend of Zed/Setup/v2.3 Auto Assign Overworld Block Art")]
        public static void AutoAssignOverworldBlockArt()
        {
            ZedOverworldGenerationManager manager = Object.FindAnyObjectByType<ZedOverworldGenerationManager>();
            if (manager == null)
            {
                Debug.LogWarning("No ZedOverworldGenerationManager found. Run v2.2 Create Seeded Overworld Scene first.");
                return;
            }

            manager.BlockVisualMode = ZedOverworldGenerationManager.VisualMode.ArtPrefabsWithDebugFallback;
            manager.CreateDebugFallbackVisuals = true;

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
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log(
                "v2.3 auto assigned overworld block art. " +
                "RoadIntersection=" + manager.RoadIntersectionPrefabs.Count +
                " RoadStraight=" + manager.RoadStraightPrefabs.Count +
                " Sidewalk=" + manager.SidewalkPrefabs.Count +
                " Buildings=" + manager.BuildingPrefabs.Count +
                " ParkGround=" + manager.ParkGroundPrefabs.Count +
                " Trees=" + manager.ParkTreePrefabs.Count +
                " Bushes=" + manager.ParkBushPrefabs.Count + ".");
        }

        [MenuItem("Legend of Zed/Setup/v2.3 Rebuild Overworld With Block Art")]
        public static void RebuildOverworldWithBlockArt()
        {
            ZedOverworldGenerationManager manager = Object.FindAnyObjectByType<ZedOverworldGenerationManager>();
            if (manager == null)
            {
                Debug.LogWarning("No ZedOverworldGenerationManager found. Run v2.2 Create Seeded Overworld Scene first.");
                return;
            }

            manager.BlockVisualMode = ZedOverworldGenerationManager.VisualMode.ArtPrefabsWithDebugFallback;
            manager.CreateDebugFallbackVisuals = true;
            manager.GenerateNew();

            EditorUtility.SetDirty(manager);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("v2.3 rebuilt seeded overworld with block art/fallback visuals.");
        }

        [MenuItem("Legend of Zed/Setup/v2.3 Validate Overworld Block Art Assignment")]
        public static void ValidateOverworldBlockArtAssignment()
        {
            ZedOverworldGenerationManager manager = Object.FindAnyObjectByType<ZedOverworldGenerationManager>();
            if (manager == null)
            {
                Debug.LogWarning("Missing ZedOverworldGenerationManager.");
                return;
            }

            int warnings = 0;

            if (manager.RoadIntersectionPrefabs.Count == 0 && manager.RoadStraightPrefabs.Count == 0)
            {
                Debug.LogWarning("No road prefabs assigned. Road blocks will use debug fallback visuals.");
                warnings++;
            }

            if (manager.SidewalkPrefabs.Count == 0)
            {
                Debug.LogWarning("No sidewalk prefabs assigned. Sidewalks will use debug fallback visuals.");
                warnings++;
            }

            if (manager.BuildingPrefabs.Count == 0)
            {
                Debug.LogWarning("No building prefabs assigned. Building lots will use debug fallback visuals.");
                warnings++;
            }

            if (manager.GeneratedBlocks == null || manager.GeneratedBlocks.Count == 0)
            {
                Debug.LogWarning("No generated blocks currently registered. Run v2.3 Rebuild Overworld With Block Art.");
                warnings++;
            }

            if (warnings == 0)
            {
                Debug.Log("v2.3 overworld block art validation passed.");
            }
            else
            {
                Debug.LogWarning("v2.3 overworld block art validation finished with " + warnings + " warning(s).");
            }
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
                    if (string.IsNullOrEmpty(path) || seen.Contains(path))
                    {
                        continue;
                    }

                    if (path.Contains("/Editor/") || path.Contains("/Examples/"))
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
    }
}

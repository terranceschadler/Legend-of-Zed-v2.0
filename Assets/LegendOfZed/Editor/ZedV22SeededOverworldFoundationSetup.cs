using System.IO;
using LegendOfZed.Overworld;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LegendOfZed.Editor
{
    public static class ZedV22SeededOverworldFoundationSetup
    {
        private const string ScenePath = "Assets/LegendOfZed/Scenes/Zed_Overworld.unity";
        private const string ManagerName = "Zed_Overworld_Generation_Manager";

        [MenuItem("Legend of Zed/Setup/v2.2 Create Seeded Overworld Scene")]
        public static void CreateSeededOverworldScene()
        {
            EnsureSceneFolder();

            Scene scene;
            if (File.Exists(ScenePath))
            {
                scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }
            else
            {
                scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                EditorSceneManager.SaveScene(scene, ScenePath);
            }

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

            EnsureCamera();
            EnsureLight();

            EditorUtility.SetDirty(managerObject);
            EditorUtility.SetDirty(manager);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("v2.2 seeded overworld scene created at " + ScenePath + ". Open it and press Play or run v2.3 rebuild to verify seeded persistence foundation.");
        }

        [MenuItem("Legend of Zed/Setup/v2.2 Generate Seeded Overworld Now")]
        public static void GenerateSeededOverworldNow()
        {
            ZedOverworldGenerationManager manager = Object.FindAnyObjectByType<ZedOverworldGenerationManager>();
            if (manager == null)
            {
                Debug.LogWarning("No ZedOverworldGenerationManager found in the open scene. Run v2.2 Create Seeded Overworld Scene first.");
                return;
            }

            manager.GenerateNew();
            EditorUtility.SetDirty(manager);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("v2.2 seeded overworld generated in editor.");
        }

        [MenuItem("Legend of Zed/Setup/v2.2 Validate Seeded Overworld Foundation")]
        public static void ValidateSeededOverworldFoundation()
        {
            ZedOverworldGenerationManager manager = Object.FindAnyObjectByType<ZedOverworldGenerationManager>();
            if (manager == null)
            {
                Debug.LogWarning("Missing ZedOverworldGenerationManager.");
                return;
            }

            int warnings = 0;

            if (manager.Width < 3 || manager.Height < 3)
            {
                Debug.LogWarning("Overworld grid is very small. Expected at least 3x3.");
                warnings++;
            }

            if (manager.BlockSize < 8f)
            {
                Debug.LogWarning("Overworld BlockSize is very small.");
                warnings++;
            }

            if (manager.GeneratedRoot == null && GameObject.Find(ZedOverworldGenerationManager.GeneratedRootName) == null)
            {
                Debug.LogWarning("No generated overworld root found yet. Press Play or run Generate Seeded Overworld Now.");
                warnings++;
            }

            if (warnings == 0)
            {
                Debug.Log("v2.2 seeded overworld foundation validation passed.");
            }
            else
            {
                Debug.LogWarning("v2.2 seeded overworld foundation validation finished with " + warnings + " warning(s).");
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

        private static void EnsureCamera()
        {
            if (Object.FindAnyObjectByType<Camera>() != null)
            {
                return;
            }

            GameObject cameraObject = new GameObject("Main Camera");
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.transform.position = new Vector3(0f, 55f, -55f);
            camera.transform.rotation = Quaternion.Euler(55f, 0f, 0f);
            camera.orthographic = false;
        }

        private static void EnsureLight()
        {
            if (Object.FindAnyObjectByType<Light>() != null)
            {
                return;
            }

            GameObject lightObject = new GameObject("Directional Light");
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1f;
            light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }
    }
}

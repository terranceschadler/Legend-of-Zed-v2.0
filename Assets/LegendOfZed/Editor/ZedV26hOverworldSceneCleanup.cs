using System.IO;
using LegendOfZed.Overworld;
using TopDownShooter;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LegendOfZed.Editor
{
    public static class ZedV26hOverworldSceneCleanup
    {
        private const string OverworldScenePath = "Assets/LegendOfZed/Scenes/Zed_Overworld.unity";
        private const string InteriorScenePath = "Assets/LegendOfZed/Scenes/Zed_Interior_Test.unity";

        private const string TestRootName = "Zed_Test_RuntimeHelpers";
        private const string PortalTestPlayerName = "Zed_Portal_Test_Player";
        private const string OverworldFloorName = "Zed_Overworld_Test_Walkable_Floor";
        private const string NearPortalName = "Zed_Test_Enter_Portal_Near_Player";

        private const string InteriorTestRootName = "Zed_Interior_Test_RuntimeHelpers";
        private const string InteriorFloorName = "Interior_Test_Floor";
        private const string ReturnPortalName = "Interior_Return_Portal";

        [MenuItem("Legend of Zed/Setup/Overworld/Clean Current Portal Test Scene")]
        public static void CleanCurrentPortalTestScene()
        {
            if (!File.Exists(OverworldScenePath))
            {
                Debug.LogWarning("Could not find overworld scene: " + OverworldScenePath);
                return;
            }

            Scene overworld = EditorSceneManager.OpenScene(OverworldScenePath, OpenSceneMode.Single);
            CleanOverworld(overworld);
            SaveScene(overworld);

            if (File.Exists(InteriorScenePath))
            {
                Scene interior = EditorSceneManager.OpenScene(InteriorScenePath, OpenSceneMode.Single);
                CleanInterior(interior);
                SaveScene(interior);
            }

            EditorSceneManager.OpenScene(OverworldScenePath, OpenSceneMode.Single);
            Debug.Log("v2.6h cleaned overworld/interior portal test scenes. Immediate portal loop remains intact.");
        }

        private static void CleanOverworld(Scene scene)
        {
            GameObject root = GetOrCreateRoot(TestRootName, scene);

            RemoveBrokenGameplayPlayers();

            GameObject player = GameObject.Find(PortalTestPlayerName);
            if (player != null)
            {
                player.SetActive(true);
                player.tag = "Player";
                player.transform.SetParent(root.transform, true);
                player.transform.position = new Vector3(0f, 2f, -8f);
                EditorUtility.SetDirty(player);
            }

            GameObject floor = GameObject.Find(OverworldFloorName);
            if (floor != null)
            {
                floor.transform.SetParent(root.transform, true);
                floor.transform.position = new Vector3(0f, -0.08f, 0f);
                floor.transform.localScale = new Vector3(210f, 0.12f, 210f);

                Collider collider = floor.GetComponent<Collider>();
                if (collider != null)
                {
                    collider.isTrigger = false;
                    EditorUtility.SetDirty(collider);
                }

                Renderer renderer = floor.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.enabled = false;
                    EditorUtility.SetDirty(renderer);
                }

                EditorUtility.SetDirty(floor);
            }

            GameObject portal = GameObject.Find(NearPortalName);
            if (portal != null)
            {
                portal.transform.SetParent(root.transform, true);
                portal.transform.position = new Vector3(0f, 0.75f, -13f);
                portal.transform.localScale = new Vector3(2.5f, 1.5f, 2.5f);
                portal.name = NearPortalName + "_PRESS_E";

                BoxCollider collider = portal.GetComponent<BoxCollider>();
                if (collider != null)
                {
                    collider.isTrigger = true;
                    collider.size = Vector3.one;
                    collider.center = Vector3.zero;
                    EditorUtility.SetDirty(collider);
                }

                ZedOverworldPortalTrigger trigger = portal.GetComponent<ZedOverworldPortalTrigger>();
                if (trigger != null)
                {
                    trigger.PromptText = "Press E to enter test interior";
                    trigger.ShowDebugPrompt = true;
                    EditorUtility.SetDirty(trigger);
                }

                EditorUtility.SetDirty(portal);
            }

            PositionOverworldCamera();
            EditorUtility.SetDirty(root);
        }

        private static void CleanInterior(Scene scene)
        {
            GameObject root = GetOrCreateRoot(InteriorTestRootName, scene);

            GameObject player = GameObject.Find(PortalTestPlayerName);
            if (player != null)
            {
                player.SetActive(true);
                player.tag = "Player";
                player.transform.SetParent(root.transform, true);
                player.transform.position = new Vector3(0f, 2f, 2f);
                EditorUtility.SetDirty(player);
            }

            GameObject floor = GameObject.Find(InteriorFloorName);
            if (floor != null)
            {
                floor.transform.SetParent(root.transform, true);
                floor.transform.position = Vector3.zero;
                floor.transform.localScale = new Vector3(12f, 0.2f, 12f);

                Collider collider = floor.GetComponent<Collider>();
                if (collider != null)
                {
                    collider.isTrigger = false;
                    EditorUtility.SetDirty(collider);
                }

                EditorUtility.SetDirty(floor);
            }

            GameObject portal = GameObject.Find(ReturnPortalName);
            if (portal != null)
            {
                portal.transform.SetParent(root.transform, true);
                portal.transform.position = new Vector3(0f, 0.8f, -4f);
                portal.transform.localScale = new Vector3(2.5f, 1.5f, 2.5f);
                portal.name = ReturnPortalName + "_PRESS_E";

                BoxCollider collider = portal.GetComponent<BoxCollider>();
                if (collider != null)
                {
                    collider.isTrigger = true;
                    collider.size = Vector3.one;
                    collider.center = Vector3.zero;
                    EditorUtility.SetDirty(collider);
                }

                ZedInteriorReturnPortal trigger = portal.GetComponent<ZedInteriorReturnPortal>();
                if (trigger != null)
                {
                    trigger.PromptText = "Press E to return to overworld";
                    trigger.ShowDebugPrompt = true;
                    EditorUtility.SetDirty(trigger);
                }

                EditorUtility.SetDirty(portal);
            }

            PositionInteriorCamera();
            EditorUtility.SetDirty(root);
        }

        private static void RemoveBrokenGameplayPlayers()
        {
            PlayerController[] playerControllers = Object.FindObjectsByType<PlayerController>(FindObjectsInactive.Include);
            for (int i = 0; i < playerControllers.Length; i++)
            {
                if (playerControllers[i] == null)
                {
                    continue;
                }

                GameObject go = playerControllers[i].gameObject;
                if (go.name == PortalTestPlayerName)
                {
                    continue;
                }

                Object.DestroyImmediate(go);
            }

            MovementCharacterController[] movementControllers = Object.FindObjectsByType<MovementCharacterController>(FindObjectsInactive.Include);
            for (int i = 0; i < movementControllers.Length; i++)
            {
                if (movementControllers[i] == null)
                {
                    continue;
                }

                GameObject go = movementControllers[i].gameObject;
                if (go.name == PortalTestPlayerName)
                {
                    continue;
                }

                Object.DestroyImmediate(go);
            }
        }

        private static GameObject GetOrCreateRoot(string name, Scene scene)
        {
            GameObject root = GameObject.Find(name);
            if (root == null)
            {
                root = new GameObject(name);
                EditorSceneManager.MoveGameObjectToScene(root, scene);
            }

            root.transform.position = Vector3.zero;
            root.transform.rotation = Quaternion.identity;
            root.transform.localScale = Vector3.one;
            return root;
        }

        private static void PositionOverworldCamera()
        {
            Camera camera = Object.FindAnyObjectByType<Camera>();
            if (camera == null)
            {
                GameObject cameraObject = new GameObject("Main Camera");
                camera = cameraObject.AddComponent<Camera>();
                camera.tag = "MainCamera";
            }

            camera.transform.position = new Vector3(0f, 38f, -38f);
            camera.transform.rotation = Quaternion.Euler(55f, 0f, 0f);
            EditorUtility.SetDirty(camera);
        }

        private static void PositionInteriorCamera()
        {
            Camera camera = Object.FindAnyObjectByType<Camera>();
            if (camera == null)
            {
                GameObject cameraObject = new GameObject("Main Camera");
                camera = cameraObject.AddComponent<Camera>();
                camera.tag = "MainCamera";
            }

            camera.transform.position = new Vector3(0f, 14f, -12f);
            camera.transform.rotation = Quaternion.Euler(55f, 0f, 0f);
            EditorUtility.SetDirty(camera);
        }

        private static void SaveScene(Scene scene)
        {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}

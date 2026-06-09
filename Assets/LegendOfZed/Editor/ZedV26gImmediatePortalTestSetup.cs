using System.IO;
using LegendOfZed.Overworld;
using TopDownShooter;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LegendOfZed.Editor
{
    public static class ZedV26gImmediatePortalTestSetup
    {
        private const string OverworldScenePath = "Assets/LegendOfZed/Scenes/Zed_Overworld.unity";
        private const string InteriorScenePath = "Assets/LegendOfZed/Scenes/Zed_Interior_Test.unity";
        private const string PortalTestPlayerName = "Zed_Portal_Test_Player";
        private const string OverworldFloorName = "Zed_Overworld_Test_Walkable_Floor";
        private const string InteriorFloorName = "Interior_Test_Floor";
        private const string NearPortalName = "Zed_Test_Enter_Portal_Near_Player";
        private const string ReturnPortalName = "Interior_Return_Portal";

        [MenuItem("Legend of Zed/Setup/Overworld/One Click Build Immediate Portal Test")]
        public static void OneClickBuildImmediatePortalTest()
        {
            ZedOverworldOneClickSetup.OneClickBuildCurrentOverworldTest();

            Scene overworld = EditorSceneManager.OpenScene(OverworldScenePath, OpenSceneMode.Single);
            DisableGameplayPlayers();
            EnsureOverworldFloor(overworld);
            GameObject player = EnsureSafePlayer(overworld, new Vector3(0f, 2f, -8f));
            EnsureCamera(new Vector3(0f, 38f, -38f), Quaternion.Euler(55f, 0f, 0f));
            EnsureNearEnterPortal(overworld, new Vector3(0f, 0.75f, -13f));
            SaveScene(overworld);

            Scene interior = EditorSceneManager.OpenScene(InteriorScenePath, OpenSceneMode.Single);
            EnsureInteriorFloor(interior);
            EnsureSafePlayer(interior, new Vector3(0f, 2f, 2f));
            EnsureCamera(new Vector3(0f, 14f, -12f), Quaternion.Euler(55f, 0f, 0f));
            EnsureInteriorReturnPortal(interior, new Vector3(0f, 0.8f, -4f));
            SaveScene(interior);

            EditorSceneManager.OpenScene(OverworldScenePath, OpenSceneMode.Single);
            AddScenesToBuildSettings();

            Debug.Log("v2.6g immediate portal test ready. Player starts near Zed_Test_Enter_Portal_Near_Player. Press Play, walk forward, press E.");
        }

        private static void DisableGameplayPlayers()
        {
            PlayerController[] playerControllers = Object.FindObjectsByType<PlayerController>(FindObjectsInactive.Include);
            for (int i = 0; i < playerControllers.Length; i++)
            {
                if (playerControllers[i] != null)
                {
                    playerControllers[i].gameObject.SetActive(false);
                    EditorUtility.SetDirty(playerControllers[i].gameObject);
                }
            }

            MovementCharacterController[] movementControllers = Object.FindObjectsByType<MovementCharacterController>(FindObjectsInactive.Include);
            for (int i = 0; i < movementControllers.Length; i++)
            {
                if (movementControllers[i] != null)
                {
                    movementControllers[i].gameObject.SetActive(false);
                    EditorUtility.SetDirty(movementControllers[i].gameObject);
                }
            }
        }

        private static void EnsureOverworldFloor(Scene scene)
        {
            GameObject floor = GameObject.Find(OverworldFloorName);
            if (floor == null)
            {
                floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
                floor.name = OverworldFloorName;
                EditorSceneManager.MoveGameObjectToScene(floor, scene);
            }

            floor.SetActive(true);
            floor.transform.position = new Vector3(0f, -0.08f, 0f);
            floor.transform.rotation = Quaternion.identity;
            floor.transform.localScale = new Vector3(210f, 0.12f, 210f);

            Collider collider = floor.GetComponent<Collider>();
            if (collider == null)
            {
                collider = floor.AddComponent<BoxCollider>();
            }

            collider.isTrigger = false;
            EditorUtility.SetDirty(floor);
        }

        private static void EnsureInteriorFloor(Scene scene)
        {
            GameObject floor = GameObject.Find(InteriorFloorName);
            if (floor == null)
            {
                floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
                floor.name = InteriorFloorName;
                EditorSceneManager.MoveGameObjectToScene(floor, scene);
            }

            floor.SetActive(true);
            floor.transform.position = Vector3.zero;
            floor.transform.rotation = Quaternion.identity;
            floor.transform.localScale = new Vector3(12f, 0.2f, 12f);

            Collider collider = floor.GetComponent<Collider>();
            if (collider == null)
            {
                collider = floor.AddComponent<BoxCollider>();
            }

            collider.isTrigger = false;
            EditorUtility.SetDirty(floor);
        }

        private static GameObject EnsureSafePlayer(Scene scene, Vector3 position)
        {
            GameObject existing = GameObject.Find(PortalTestPlayerName);
            if (existing == null)
            {
                existing = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                existing.name = PortalTestPlayerName;
                EditorSceneManager.MoveGameObjectToScene(existing, scene);
            }

            existing.SetActive(true);
            existing.tag = "Player";
            existing.transform.position = position;
            existing.transform.rotation = Quaternion.identity;

            Collider primitiveCollider = existing.GetComponent<Collider>();
            if (primitiveCollider != null && primitiveCollider.GetType() != typeof(CharacterController))
            {
                Object.DestroyImmediate(primitiveCollider);
            }

            CharacterController controller = existing.GetComponent<CharacterController>();
            if (controller == null)
            {
                controller = existing.AddComponent<CharacterController>();
            }

            controller.height = 2f;
            controller.radius = 0.35f;
            controller.center = new Vector3(0f, 1f, 0f);

            ZedPortalTestPlayerMover mover = existing.GetComponent<ZedPortalTestPlayerMover>();
            if (mover == null)
            {
                mover = existing.AddComponent<ZedPortalTestPlayerMover>();
            }

            // Remove older Editor-only test mover if it exists as a missing/old component cannot be referenced safely.
            EditorUtility.SetDirty(existing);
            EditorUtility.SetDirty(controller);
            EditorUtility.SetDirty(mover);

            return existing;
        }

        private static void EnsureNearEnterPortal(Scene scene, Vector3 position)
        {
            GameObject portalObject = GameObject.Find(NearPortalName);
            if (portalObject == null)
            {
                portalObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                portalObject.name = NearPortalName;
                EditorSceneManager.MoveGameObjectToScene(portalObject, scene);
            }

            portalObject.SetActive(true);
            portalObject.transform.position = position;
            portalObject.transform.rotation = Quaternion.identity;
            portalObject.transform.localScale = new Vector3(2.5f, 1.5f, 2.5f);

            BoxCollider collider = portalObject.GetComponent<BoxCollider>();
            if (collider == null)
            {
                collider = portalObject.AddComponent<BoxCollider>();
            }

            collider.isTrigger = true;
            collider.size = Vector3.one;
            collider.center = Vector3.zero;

            ZedOverworldEnterableBuildingMarker marker = portalObject.GetComponent<ZedOverworldEnterableBuildingMarker>();
            if (marker == null)
            {
                marker = portalObject.AddComponent<ZedOverworldEnterableBuildingMarker>();
            }

            marker.GridCoordinate = new Vector2Int(0, 0);
            marker.Seed = 2200;
            marker.InteriorSceneName = "Zed_Interior_Test";
            marker.IsPortalCandidate = true;

            ZedOverworldPortalTrigger portal = portalObject.GetComponent<ZedOverworldPortalTrigger>();
            if (portal == null)
            {
                portal = portalObject.AddComponent<ZedOverworldPortalTrigger>();
            }

            portal.PortalId = "Immediate_Test_Portal";
            portal.InteriorSceneName = "Zed_Interior_Test";
            portal.PromptText = "Press E to enter test interior";
            portal.ShowDebugPrompt = true;
            portal.OverworldManager = Object.FindAnyObjectByType<ZedOverworldGenerationManager>();

            EditorUtility.SetDirty(portalObject);
            EditorUtility.SetDirty(collider);
            EditorUtility.SetDirty(marker);
            EditorUtility.SetDirty(portal);
        }

        private static void EnsureInteriorReturnPortal(Scene scene, Vector3 position)
        {
            GameObject portalObject = GameObject.Find(ReturnPortalName);
            if (portalObject == null)
            {
                portalObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                portalObject.name = ReturnPortalName;
                EditorSceneManager.MoveGameObjectToScene(portalObject, scene);
            }

            portalObject.SetActive(true);
            portalObject.transform.position = position;
            portalObject.transform.rotation = Quaternion.identity;
            portalObject.transform.localScale = new Vector3(2.5f, 1.5f, 2.5f);

            BoxCollider collider = portalObject.GetComponent<BoxCollider>();
            if (collider == null)
            {
                collider = portalObject.AddComponent<BoxCollider>();
            }

            collider.isTrigger = true;
            collider.size = Vector3.one;
            collider.center = Vector3.zero;

            ZedInteriorReturnPortal portal = portalObject.GetComponent<ZedInteriorReturnPortal>();
            if (portal == null)
            {
                portal = portalObject.AddComponent<ZedInteriorReturnPortal>();
            }

            portal.OverworldSceneName = "Zed_Overworld";
            portal.PromptText = "Press E to return to overworld";
            portal.ShowDebugPrompt = true;

            EditorUtility.SetDirty(portalObject);
            EditorUtility.SetDirty(collider);
            EditorUtility.SetDirty(portal);
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
            EditorUtility.SetDirty(camera);
        }

        private static void AddScenesToBuildSettings()
        {
            System.Collections.Generic.List<EditorBuildSettingsScene> scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            AddSceneIfMissing(scenes, OverworldScenePath);
            AddSceneIfMissing(scenes, InteriorScenePath);
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private static void AddSceneIfMissing(System.Collections.Generic.List<EditorBuildSettingsScene> scenes, string path)
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

        private static void SaveScene(Scene scene)
        {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}

using System.IO;
using TopDownShooter;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LegendOfZed.Editor
{
    public static class ZedV26ePortalTestPlayerSafeMode
    {
        private const string OverworldScenePath = "Assets/LegendOfZed/Scenes/Zed_Overworld.unity";
        private const string PortalTestPlayerName = "Zed_Portal_Test_Player";
        private const string TestFloorName = "Zed_Overworld_Test_Walkable_Floor";

        [MenuItem("Legend of Zed/Setup/Overworld/One Click Build Current Overworld Test + Safe Portal Player")]
        public static void OneClickBuildCurrentOverworldTestWithSafePlayer()
        {
            ZedOverworldOneClickSetup.OneClickBuildCurrentOverworldTest();
            UseSafePortalTestPlayer();
            Debug.Log("v2.6f one-click overworld test with safe portal player and walkable floor complete.");
        }

        [MenuItem("Legend of Zed/Setup/Overworld/Use Safe Portal Test Player")]
        public static void UseSafePortalTestPlayer()
        {
            if (!File.Exists(OverworldScenePath))
            {
                Debug.LogWarning("Could not find overworld scene: " + OverworldScenePath);
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(OverworldScenePath, OpenSceneMode.Single);

            DisableBrokenGameplayPlayers();
            EnsureWalkableTestFloor(scene);

            GameObject player = FindOrCreatePortalTestPlayer(scene);
            PlacePlayer(player.transform);
            EnsureCamera();

            EditorUtility.SetDirty(player);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("v2.6f safe portal test player ready with walkable test floor. Press Play, move with WASD, press E at enterable marker.");
        }

        [MenuItem("Legend of Zed/Setup/Overworld/Add/Repair Walkable Test Floor")]
        public static void AddOrRepairWalkableTestFloor()
        {
            if (!File.Exists(OverworldScenePath))
            {
                Debug.LogWarning("Could not find overworld scene: " + OverworldScenePath);
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(OverworldScenePath, OpenSceneMode.Single);
            EnsureWalkableTestFloor(scene);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("v2.6f walkable overworld test floor created/repaired.");
        }

        private static void EnsureWalkableTestFloor(Scene scene)
        {
            GameObject floor = GameObject.Find(TestFloorName);
            if (floor == null)
            {
                floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
                floor.name = TestFloorName;
                EditorSceneManager.MoveGameObjectToScene(floor, scene);
            }

            floor.SetActive(true);
            floor.transform.position = new Vector3(0f, -0.08f, 0f);
            floor.transform.rotation = Quaternion.identity;
            floor.transform.localScale = new Vector3(210f, 0.12f, 210f);
            floor.layer = 0;

            Collider collider = floor.GetComponent<Collider>();
            if (collider == null)
            {
                collider = floor.AddComponent<BoxCollider>();
            }

            collider.isTrigger = false;

            Renderer renderer = floor.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.enabled = true;
            }

            EditorUtility.SetDirty(floor);
            EditorUtility.SetDirty(collider);
        }

        private static void DisableBrokenGameplayPlayers()
        {
            PlayerController[] playerControllers = Object.FindObjectsByType<PlayerController>(FindObjectsInactive.Include);
            for (int i = 0; i < playerControllers.Length; i++)
            {
                if (playerControllers[i] == null)
                {
                    continue;
                }

                if (playerControllers[i].gameObject.name == PortalTestPlayerName)
                {
                    continue;
                }

                playerControllers[i].gameObject.SetActive(false);
                EditorUtility.SetDirty(playerControllers[i].gameObject);
            }

            MovementCharacterController[] movementControllers = Object.FindObjectsByType<MovementCharacterController>(FindObjectsInactive.Include);
            for (int i = 0; i < movementControllers.Length; i++)
            {
                if (movementControllers[i] == null)
                {
                    continue;
                }

                if (movementControllers[i].gameObject.name == PortalTestPlayerName)
                {
                    continue;
                }

                movementControllers[i].gameObject.SetActive(false);
                EditorUtility.SetDirty(movementControllers[i].gameObject);
            }

            GameObject tagged = GameObject.FindGameObjectWithTag("Player");
            if (tagged != null && tagged.name != PortalTestPlayerName)
            {
                tagged.tag = "Untagged";
                tagged.SetActive(false);
                EditorUtility.SetDirty(tagged);
            }
        }

        private static GameObject FindOrCreatePortalTestPlayer(Scene scene)
        {
            GameObject existing = GameObject.Find(PortalTestPlayerName);
            if (existing != null)
            {
                existing.SetActive(true);
                existing.tag = "Player";
                EnsurePortalTestMover(existing);
                return existing;
            }

            GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = PortalTestPlayerName;
            player.tag = "Player";

            Collider primitiveCollider = player.GetComponent<Collider>();
            if (primitiveCollider != null)
            {
                Object.DestroyImmediate(primitiveCollider);
            }

            CharacterController controller = player.AddComponent<CharacterController>();
            controller.height = 2f;
            controller.radius = 0.35f;
            controller.center = new Vector3(0f, 1f, 0f);

            EnsurePortalTestMover(player);
            EditorSceneManager.MoveGameObjectToScene(player, scene);
            return player;
        }

        private static void EnsurePortalTestMover(GameObject player)
        {
            if (player.GetComponent<ZedSafePortalTestMover>() == null)
            {
                player.AddComponent<ZedSafePortalTestMover>();
            }

            CharacterController controller = player.GetComponent<CharacterController>();
            if (controller == null)
            {
                controller = player.AddComponent<CharacterController>();
            }

            controller.height = 2f;
            controller.radius = 0.35f;
            controller.center = new Vector3(0f, 1f, 0f);
        }

        private static void PlacePlayer(Transform player)
        {
            player.position = new Vector3(0f, 2.0f, -8f);
            player.rotation = Quaternion.identity;
        }

        private static void EnsureCamera()
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
        }
    }

    public class ZedSafePortalTestMover : MonoBehaviour
    {
        public float MoveSpeed = 7f;
        public float Gravity = -18f;

        private CharacterController _controller;
        private float _verticalVelocity;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
        }

        private void Update()
        {
            if (_controller == null)
            {
                return;
            }

            UnityEngine.InputSystem.Keyboard keyboard = UnityEngine.InputSystem.Keyboard.current;

            Vector3 move = Vector3.zero;
            if (keyboard != null)
            {
                if (keyboard.wKey.isPressed) move.z += 1f;
                if (keyboard.sKey.isPressed) move.z -= 1f;
                if (keyboard.dKey.isPressed) move.x += 1f;
                if (keyboard.aKey.isPressed) move.x -= 1f;
            }

            move = Vector3.ClampMagnitude(move, 1f);

            if (_controller.isGrounded && _verticalVelocity < 0f)
            {
                _verticalVelocity = -1f;
            }

            _verticalVelocity += Gravity * Time.deltaTime;

            Vector3 velocity = move * MoveSpeed;
            velocity.y = _verticalVelocity;

            _controller.Move(velocity * Time.deltaTime);

            if (move.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(move.normalized, Vector3.up);
            }
        }
    }
}

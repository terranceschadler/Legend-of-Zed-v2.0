using System.IO;
using System.Reflection;
using TopDownShooter;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LegendOfZed.Editor
{
    public static class ZedV26cOverworldPlayerSetupFix
    {
        private const string OverworldScenePath = "Assets/LegendOfZed/Scenes/Zed_Overworld.unity";
        private const string ControllerTestScenePath = "Assets/LegendOfZed/Scenes/Zed_Controller_Test.unity";

        [MenuItem("Legend of Zed/Setup/Overworld/One Click Build Current Overworld Test + Player")]
        public static void OneClickBuildCurrentOverworldTestWithPlayer()
        {
            ZedOverworldOneClickSetup.OneClickBuildCurrentOverworldTest();
            AddPlayerToOverworldTest();
            RepairOverworldPlayerReferences();
            Debug.Log("v2.6d one-click overworld test with repaired player complete.");
        }

        [MenuItem("Legend of Zed/Setup/Overworld/Add Player To Overworld Test")]
        public static void AddPlayerToOverworldTest()
        {
            if (!File.Exists(OverworldScenePath))
            {
                Debug.LogWarning("Could not find overworld scene: " + OverworldScenePath);
                return;
            }

            Scene overworldScene = EditorSceneManager.OpenScene(OverworldScenePath, OpenSceneMode.Single);

            Transform existingPlayer = FindExistingPlayer();
            if (existingPlayer != null)
            {
                PlacePlayer(existingPlayer);
                RepairPlayer(existingPlayer.gameObject);
                SaveScene(overworldScene);
                Debug.Log("v2.6d found existing player in overworld, moved it to the test spawn, and repaired references.");
                return;
            }

            GameObject player = TryCopyPlayerFromControllerTestScene(overworldScene);
            if (player == null)
            {
                player = TryInstantiatePlayerPrefab();
            }

            if (player == null)
            {
                player = CreateFallbackPortalTestPlayer(overworldScene);
                Debug.LogWarning("v2.6d could not find the gameplay player prefab or controller test player. Created a temporary fallback portal-test player.");
            }

            if (player == null)
            {
                Debug.LogWarning("v2.6d failed to create any player.");
                return;
            }

            player.name = "Player";
            player.tag = "Player";
            EditorSceneManager.MoveGameObjectToScene(player, overworldScene);
            PlacePlayer(player.transform);
            RepairPlayer(player);

            EditorUtility.SetDirty(player);
            SaveScene(overworldScene);

            Debug.Log("v2.6d player added/repaired in Zed_Overworld. Press Play and walk to an enterable marker.");
        }

        [MenuItem("Legend of Zed/Setup/Overworld/Repair Overworld Player References")]
        public static void RepairOverworldPlayerReferences()
        {
            if (File.Exists(OverworldScenePath))
            {
                EditorSceneManager.OpenScene(OverworldScenePath, OpenSceneMode.Single);
            }

            Transform player = FindExistingPlayer();
            if (player == null)
            {
                Debug.LogWarning("No player found in Zed_Overworld to repair. Run Add Player To Overworld Test.");
                return;
            }

            int repaired = RepairPlayer(player.gameObject);
            SaveScene(SceneManager.GetActiveScene());

            Debug.Log("v2.6d overworld player reference repair finished. Repairs=" + repaired + ".");
        }

        private static Transform FindExistingPlayer()
        {
            PlayerController playerController = Object.FindAnyObjectByType<PlayerController>();
            if (playerController != null)
            {
                return playerController.transform;
            }

            GameObject tagged = GameObject.FindGameObjectWithTag("Player");
            if (tagged != null)
            {
                return tagged.transform;
            }

            CharacterController characterController = Object.FindAnyObjectByType<CharacterController>();
            return characterController != null ? characterController.transform : null;
        }

        private static GameObject TryCopyPlayerFromControllerTestScene(Scene overworldScene)
        {
            if (!File.Exists(ControllerTestScenePath))
            {
                return null;
            }

            Scene testScene = EditorSceneManager.OpenScene(ControllerTestScenePath, OpenSceneMode.Additive);
            GameObject source = null;

            GameObject[] roots = testScene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                if (roots[i] == null)
                {
                    continue;
                }

                PlayerController candidate = roots[i].GetComponentInChildren<PlayerController>(true);
                if (candidate != null)
                {
                    source = candidate.gameObject;
                    break;
                }

                MovementCharacterController movement = roots[i].GetComponentInChildren<MovementCharacterController>(true);
                if (movement != null)
                {
                    source = movement.gameObject;
                    break;
                }

                if (roots[i].CompareTag("Player"))
                {
                    source = roots[i];
                    break;
                }
            }

            GameObject copy = null;
            if (source != null)
            {
                copy = Object.Instantiate(source);
                copy.name = "Player";
                copy.SetActive(true);
                EditorSceneManager.MoveGameObjectToScene(copy, overworldScene);
                Debug.Log("v2.6d copied player from Zed_Controller_Test scene.");
            }

            EditorSceneManager.CloseScene(testScene, true);
            EditorSceneManager.SetActiveScene(overworldScene);
            return copy;
        }

        private static GameObject TryInstantiatePlayerPrefab()
        {
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (string.IsNullOrEmpty(path) || path.Contains("/Editor/") || path.Contains("/Examples/"))
                {
                    continue;
                }

                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null)
                {
                    continue;
                }

                if (prefab.GetComponentInChildren<PlayerController>(true) == null)
                {
                    continue;
                }

                GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                if (instance != null)
                {
                    Debug.Log("v2.6d instantiated player prefab: " + path);
                    return instance;
                }
            }

            return null;
        }

        private static GameObject CreateFallbackPortalTestPlayer(Scene overworldScene)
        {
            GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Player";
            player.tag = "Player";
            player.transform.localScale = Vector3.one;

            Collider collider = player.GetComponent<Collider>();
            if (collider != null)
            {
                Object.DestroyImmediate(collider);
            }

            CharacterController controller = player.AddComponent<CharacterController>();
            controller.height = 2f;
            controller.radius = 0.35f;
            controller.center = new Vector3(0f, 1f, 0f);

            player.AddComponent<ZedFallbackPortalTestMover>();

            EditorSceneManager.MoveGameObjectToScene(player, overworldScene);
            return player;
        }

        private static int RepairPlayer(GameObject player)
        {
            if (player == null)
            {
                return 0;
            }

            int repairs = 0;

            MovementCharacterController movement = player.GetComponentInChildren<MovementCharacterController>(true);
            if (movement != null)
            {
                SerializedObject serializedMovement = new SerializedObject(movement);

                SerializedProperty animatorProperty = serializedMovement.FindProperty("PlayerAnimator");
                if (animatorProperty != null && animatorProperty.objectReferenceValue == null)
                {
                    Animator animator = player.GetComponentInChildren<Animator>(true);
                    if (animator != null)
                    {
                        animatorProperty.objectReferenceValue = animator;
                        repairs++;
                    }
                }

                repairs += EnsureTransformReference(serializedMovement, movement.transform, "LowZonePosition", new Vector3(0f, 0.1f, 0f));
                repairs += EnsureTransformReference(serializedMovement, movement.transform, "HighZonePosition", new Vector3(0f, 1.8f, 0f));

                serializedMovement.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(movement);
            }

            PlayerController playerController = player.GetComponentInChildren<PlayerController>(true);
            if (playerController != null)
            {
                EditorUtility.SetDirty(playerController);
            }

            // Reflection fallback for differently serialized animator fields.
            Component[] components = player.GetComponentsInChildren<Component>(true);
            for (int i = 0; i < components.Length; i++)
            {
                Component component = components[i];
                if (component == null)
                {
                    continue;
                }

                FieldInfo field = component.GetType().GetField("PlayerAnimator", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (field != null && field.FieldType == typeof(Animator) && field.GetValue(component) == null)
                {
                    Animator animator = player.GetComponentInChildren<Animator>(true);
                    if (animator != null)
                    {
                        field.SetValue(component, animator);
                        EditorUtility.SetDirty(component);
                        repairs++;
                    }
                }
            }

            return repairs;
        }

        private static int EnsureTransformReference(SerializedObject serializedObject, Transform parent, string propertyName, Vector3 localPosition)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null || property.objectReferenceValue != null)
            {
                return 0;
            }

            Transform child = parent.Find(propertyName);
            if (child == null)
            {
                GameObject childObject = new GameObject(propertyName);
                childObject.transform.SetParent(parent, false);
                childObject.transform.localPosition = localPosition;
                childObject.transform.localRotation = Quaternion.identity;
                child = childObject.transform;
                EditorUtility.SetDirty(childObject);
            }

            property.objectReferenceValue = child;
            return 1;
        }

        private static void PlacePlayer(Transform player)
        {
            if (player == null)
            {
                return;
            }

            player.position = new Vector3(0f, 1.0f, -8f);
            player.rotation = Quaternion.identity;

            Camera camera = Object.FindAnyObjectByType<Camera>();
            if (camera != null)
            {
                camera.transform.position = new Vector3(0f, 38f, -38f);
                camera.transform.rotation = Quaternion.Euler(55f, 0f, 0f);
            }
        }

        private static void SaveScene(Scene scene)
        {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }

    public class ZedFallbackPortalTestMover : MonoBehaviour
    {
        public float MoveSpeed = 6f;
        private CharacterController _controller;

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

            Vector3 move = Vector3.zero;

            UnityEngine.InputSystem.Keyboard keyboard = UnityEngine.InputSystem.Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.wKey.isPressed) move.z += 1f;
                if (keyboard.sKey.isPressed) move.z -= 1f;
                if (keyboard.dKey.isPressed) move.x += 1f;
                if (keyboard.aKey.isPressed) move.x -= 1f;
            }

            move = Vector3.ClampMagnitude(move, 1f);
            _controller.SimpleMove(move * MoveSpeed);
        }
    }
}

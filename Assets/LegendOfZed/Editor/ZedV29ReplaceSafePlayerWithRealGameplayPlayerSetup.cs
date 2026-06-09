using System.Collections.Generic;
using System.IO;
using System.Reflection;
using TopDownShooter;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LegendOfZed.Editor
{
    public static class ZedV29ReplaceSafePlayerWithRealGameplayPlayerSetup
    {
        private const string OverworldScenePath = "Assets/LegendOfZed/Scenes/Zed_Overworld.unity";
        private const string SafePlayerName = "Zed_Portal_Test_Player";
        private const string RealPlayerName = "Zed_Real_Gameplay_Player";
        private const string PlayerSourceScenePreferred = "Assets/LegendOfZed/Scenes/Zed_Controller_Test.unity";

        [MenuItem("Legend of Zed/Setup/Overworld/v2.9 Replace Safe Player With Real Gameplay Player")]
        public static void ReplaceSafePlayerWithRealGameplayPlayer()
        {
            if (!File.Exists(OverworldScenePath))
            {
                Debug.LogWarning("v2.9 could not find Zed_Overworld. Run the immediate portal test setup first.");
                return;
            }

            string sourceScenePath = ResolveControllerTestScenePath();
            if (string.IsNullOrEmpty(sourceScenePath))
            {
                Debug.LogWarning("v2.9 could not find Zed_Controller_Test scene. Safe portal test player was left untouched.");
                return;
            }

            Scene overworldScene = EditorSceneManager.OpenScene(OverworldScenePath, OpenSceneMode.Single);

            GameObject oldRealPlayer = GameObject.Find(RealPlayerName);
            if (oldRealPlayer != null)
            {
                Object.DestroyImmediate(oldRealPlayer);
            }

            GameObject copiedPlayerRoot = CopyWorkingPlayerRootFromControllerTest(overworldScene, sourceScenePath);
            if (copiedPlayerRoot == null)
            {
                Debug.LogWarning("v2.9 could not copy a working player from " + sourceScenePath + ". Safe portal test player was left untouched.");
                return;
            }

            copiedPlayerRoot.name = RealPlayerName;
            copiedPlayerRoot.SetActive(true);
            SetTagRecursivelySafe(copiedPlayerRoot, "Player");
            PlaceRealPlayer(copiedPlayerRoot.transform);

            int repairs = RepairGameplayPlayerReferences(copiedPlayerRoot);
            DisableSafePortalPlayer();

            EnsureCameraFramesRealPlayer(copiedPlayerRoot.transform);
            EnsurePortalTriggersStillTargetPlayer();

            EditorUtility.SetDirty(copiedPlayerRoot);
            EditorSceneManager.MarkSceneDirty(overworldScene);
            EditorSceneManager.SaveScene(overworldScene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("v2.9 replaced safe portal test player with real gameplay player from " + sourceScenePath + ". Repairs=" + repairs + ". Test movement, weapon, zombies, and portal loop.");
        }

        [MenuItem("Legend of Zed/Setup/Overworld/v2.9 Validate Real Gameplay Player")]
        public static void ValidateRealGameplayPlayer()
        {
            int warnings = 0;

            GameObject realPlayer = GameObject.Find(RealPlayerName);
            if (realPlayer == null)
            {
                Debug.LogWarning("v2.9 validation: missing " + RealPlayerName + ".");
                warnings++;
            }
            else
            {
                if (realPlayer.GetComponentInChildren<PlayerController>(true) == null)
                {
                    Debug.LogWarning("v2.9 validation: real player has no PlayerController.");
                    warnings++;
                }

                MovementCharacterController movement = realPlayer.GetComponentInChildren<MovementCharacterController>(true);
                if (movement == null)
                {
                    Debug.LogWarning("v2.9 validation: real player has no MovementCharacterController.");
                    warnings++;
                }
                else
                {
                    Animator animator = GetObjectReferenceFromSerializedProperty<Animator>(movement, "PlayerAnimator");
                    if (animator == null)
                    {
                        Debug.LogWarning("v2.9 validation: MovementCharacterController.PlayerAnimator is still missing.");
                        warnings++;
                    }
                }

                if (realPlayer.GetComponentInChildren<CharacterController>(true) == null)
                {
                    Debug.LogWarning("v2.9 validation: real player has no CharacterController.");
                    warnings++;
                }
            }

            GameObject safePlayer = GameObject.Find(SafePlayerName);
            if (safePlayer != null && safePlayer.activeInHierarchy)
            {
                Debug.LogWarning("v2.9 validation: safe portal test player is still active. It should be disabled after real player placement.");
                warnings++;
            }

            if (warnings == 0)
            {
                Debug.Log("v2.9 real gameplay player validation passed.");
            }
            else
            {
                Debug.LogWarning("v2.9 real gameplay player validation finished with " + warnings + " warning(s).");
            }
        }

        [MenuItem("Legend of Zed/Setup/Overworld/v2.9 Restore Safe Portal Test Player")]
        public static void RestoreSafePortalTestPlayer()
        {
            Scene scene = EditorSceneManager.OpenScene(OverworldScenePath, OpenSceneMode.Single);

            GameObject realPlayer = GameObject.Find(RealPlayerName);
            if (realPlayer != null)
            {
                realPlayer.SetActive(false);
                EditorUtility.SetDirty(realPlayer);
            }

            GameObject safePlayer = GameObject.Find(SafePlayerName);
            if (safePlayer != null)
            {
                safePlayer.SetActive(true);
                safePlayer.tag = "Player";
                safePlayer.transform.position = new Vector3(0f, 2f, -8f);
                safePlayer.transform.rotation = Quaternion.identity;
                EditorUtility.SetDirty(safePlayer);
            }
            else
            {
                Debug.LogWarning("v2.9 restore could not find safe portal test player. Run v2.6g immediate portal test setup if you need to recreate it.");
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("v2.9 restored safe portal test player mode.");
        }

        private static string ResolveControllerTestScenePath()
        {
            if (File.Exists(PlayerSourceScenePreferred))
            {
                return PlayerSourceScenePreferred;
            }

            string[] sceneGuids = AssetDatabase.FindAssets("Zed_Controller_Test t:Scene", new[] { "Assets" });
            for (int i = 0; i < sceneGuids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(sceneGuids[i]);
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    return path;
                }
            }

            string[] allSceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets" });
            for (int i = 0; i < allSceneGuids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(allSceneGuids[i]);
                if (string.IsNullOrEmpty(path))
                {
                    continue;
                }

                string fileName = Path.GetFileNameWithoutExtension(path).ToLowerInvariant();
                if (fileName.Contains("controller") && fileName.Contains("test"))
                {
                    return path;
                }
            }

            return string.Empty;
        }

        private static GameObject CopyWorkingPlayerRootFromControllerTest(Scene overworldScene, string sourceScenePath)
        {
            Scene sourceScene = EditorSceneManager.OpenScene(sourceScenePath, OpenSceneMode.Additive);

            GameObject sourcePlayerRoot = FindBestSourcePlayerRoot(sourceScene);
            GameObject copy = null;

            if (sourcePlayerRoot != null)
            {
                copy = Object.Instantiate(sourcePlayerRoot);
                copy.name = RealPlayerName;
                copy.SetActive(true);
                EditorSceneManager.MoveGameObjectToScene(copy, overworldScene);
            }

            EditorSceneManager.CloseScene(sourceScene, true);
            EditorSceneManager.SetActiveScene(overworldScene);

            return copy;
        }

        private static GameObject FindBestSourcePlayerRoot(Scene sourceScene)
        {
            GameObject[] roots = sourceScene.GetRootGameObjects();

            GameObject best = null;
            int bestScore = int.MinValue;

            for (int i = 0; i < roots.Length; i++)
            {
                GameObject root = roots[i];
                if (root == null)
                {
                    continue;
                }

                Component[] all = root.GetComponentsInChildren<Component>(true);
                int score = 0;

                if (root.CompareTag("Player"))
                {
                    score += 50;
                }

                if (root.GetComponentInChildren<PlayerController>(true) != null)
                {
                    score += 100;
                }

                if (root.GetComponentInChildren<MovementCharacterController>(true) != null)
                {
                    score += 100;
                }

                if (root.GetComponentInChildren<CharacterController>(true) != null)
                {
                    score += 40;
                }

                if (root.GetComponentInChildren<Animator>(true) != null)
                {
                    score += 40;
                }

                if (root.GetComponentInChildren<Camera>(true) != null)
                {
                    score -= 50;
                }

                string lowerName = root.name.ToLowerInvariant();
                if (lowerName.Contains("manager") || lowerName.Contains("canvas") || lowerName.Contains("eventsystem") || lowerName.Contains("light"))
                {
                    score -= 100;
                }

                if (all.Length > 0 && score > bestScore)
                {
                    bestScore = score;
                    best = root;
                }
            }

            if (best == null || bestScore < 100)
            {
                // Fallback: find the object with PlayerController and copy its outermost source-scene root.
                for (int i = 0; i < roots.Length; i++)
                {
                    PlayerController controller = roots[i].GetComponentInChildren<PlayerController>(true);
                    if (controller != null)
                    {
                        return roots[i];
                    }
                }
            }

            return best;
        }

        private static void PlaceRealPlayer(Transform player)
        {
            player.position = new Vector3(0f, 1.1f, -8f);
            player.rotation = Quaternion.identity;

            CharacterController characterController = player.GetComponentInChildren<CharacterController>(true);
            if (characterController != null)
            {
                characterController.enabled = false;
                characterController.height = Mathf.Max(characterController.height, 1.8f);
                characterController.radius = Mathf.Clamp(characterController.radius, 0.25f, 0.65f);
                characterController.center = new Vector3(characterController.center.x, characterController.height * 0.5f, characterController.center.z);
                characterController.enabled = true;
                EditorUtility.SetDirty(characterController);
            }
        }

        private static void DisableSafePortalPlayer()
        {
            GameObject safePlayer = GameObject.Find(SafePlayerName);
            if (safePlayer == null)
            {
                return;
            }

            safePlayer.SetActive(false);
            if (safePlayer.CompareTag("Player"))
            {
                safePlayer.tag = "Untagged";
            }

            EditorUtility.SetDirty(safePlayer);
        }

        private static int RepairGameplayPlayerReferences(GameObject player)
        {
            int repairs = 0;

            Animator bestAnimator = FindBestAnimator(player);
            MovementCharacterController movement = player.GetComponentInChildren<MovementCharacterController>(true);
            if (movement != null && bestAnimator != null)
            {
                SerializedObject serializedMovement = new SerializedObject(movement);

                SerializedProperty animatorProperty = serializedMovement.FindProperty("PlayerAnimator");
                if (animatorProperty != null && animatorProperty.objectReferenceValue == null)
                {
                    animatorProperty.objectReferenceValue = bestAnimator;
                    repairs++;
                }

                repairs += EnsureTransformReference(serializedMovement, movement.transform, "LowZonePosition", new Vector3(0f, 0.1f, 0f));
                repairs += EnsureTransformReference(serializedMovement, movement.transform, "HighZonePosition", new Vector3(0f, 1.8f, 0f));

                serializedMovement.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(movement);
            }

            Component[] components = player.GetComponentsInChildren<Component>(true);
            for (int i = 0; i < components.Length; i++)
            {
                Component component = components[i];
                if (component == null || bestAnimator == null)
                {
                    continue;
                }

                FieldInfo field = component.GetType().GetField("PlayerAnimator", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (field != null && field.FieldType == typeof(Animator) && field.GetValue(component) == null)
                {
                    field.SetValue(component, bestAnimator);
                    EditorUtility.SetDirty(component);
                    repairs++;
                }
            }

            return repairs;
        }

        private static Animator FindBestAnimator(GameObject player)
        {
            Animator[] animators = player.GetComponentsInChildren<Animator>(true);
            if (animators == null || animators.Length == 0)
            {
                return null;
            }

            for (int i = 0; i < animators.Length; i++)
            {
                if (animators[i] != null && animators[i].runtimeAnimatorController != null)
                {
                    return animators[i];
                }
            }

            return animators[0];
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

        private static T GetObjectReferenceFromSerializedProperty<T>(Object target, string propertyName) where T : Object
        {
            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            return property != null ? property.objectReferenceValue as T : null;
        }

        private static void EnsureCameraFramesRealPlayer(Transform player)
        {
            Camera camera = Object.FindAnyObjectByType<Camera>();
            if (camera == null)
            {
                GameObject cameraObject = new GameObject("Main Camera");
                camera = cameraObject.AddComponent<Camera>();
                camera.tag = "MainCamera";
            }

            camera.transform.position = player.position + new Vector3(0f, 38f, -38f);
            camera.transform.rotation = Quaternion.Euler(55f, 0f, 0f);
            EditorUtility.SetDirty(camera);
        }

        private static void EnsurePortalTriggersStillTargetPlayer()
        {
            // The portal triggers detect PlayerController, Player tag, or CharacterController.
            // This method intentionally does not edit portal scene names or flow.
            GameObject realPlayer = GameObject.Find(RealPlayerName);
            if (realPlayer != null)
            {
                SetTagRecursivelySafe(realPlayer, "Player");
            }
        }

        private static void SetTagRecursivelySafe(GameObject root, string tag)
        {
            try
            {
                root.tag = tag;
            }
            catch
            {
                // Ignore missing custom tags. Player is a built-in tag in normal Unity projects.
            }

            Transform[] children = root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < children.Length; i++)
            {
                if (children[i] == null)
                {
                    continue;
                }

                try
                {
                    if (children[i].name.ToLowerInvariant().Contains("player"))
                    {
                        children[i].gameObject.tag = tag;
                    }
                }
                catch
                {
                    // Ignore tag assignment issues.
                }
            }
        }
    }
}

using System.IO;
using TopDownShooter;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LegendOfZed.Editor
{
    public static class ZedV06SyntyCharacterVisualSwap
    {
        private const string ScenePath = "Assets/LegendOfZed/Scenes/Zed_Controller_Test.unity";
        private const string CharacterPrefabName = "Character_MercenaryMale_01";
        private const string NewVisualName = "Zed_Synty_Character_MercenaryMale_01_Visual";
        private const string HiddenRendererPrefix = "ZedHiddenOriginalRenderer_";
        private const float SyntyVisualScaleMultiplier = 1.25f;

        [MenuItem("Legend of Zed/Setup/v0.6 Apply Synty Character Visual Only")]
        public static void ApplySyntyCharacterVisualOnly()
        {
            if (!File.Exists(ScenePath))
            {
                Debug.LogWarning("v0.6 Synty visual swap could not find scene: " + ScenePath);
                return;
            }

            GameObject characterPrefab = FindExactPrefab(CharacterPrefabName);
            if (characterPrefab == null)
            {
                Debug.LogWarning("v0.6 Synty visual swap could not find prefab named " + CharacterPrefabName + ". Confirm the Synty assets are imported.");
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            PlayerController playerController = Object.FindFirstObjectByType<PlayerController>();
            if (playerController == null || playerController.MovCharController == null || playerController.MovCharController.PlayerAnimator == null)
            {
                Debug.LogWarning("v0.6 Synty visual swap needs the package PlayerController, MovementCharacterController, and PlayerAnimator in the locked test scene.");
                return;
            }

            GameObject existingVisualObject = FindExistingSyntyVisualInScene();
            if (existingVisualObject != null)
            {
                existingVisualObject.transform.localScale = Vector3.one * SyntyVisualScaleMultiplier;
                EditorUtility.SetDirty(existingVisualObject);
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene, ScenePath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log("v0.6 resized existing Synty character visual root " + existingVisualObject.name + " to scale multiplier " + SyntyVisualScaleMultiplier + ". No weapon data, ammo, projectiles, prefabs, or controller logic were changed.");
                return;
            }

            Animator originalAnimator = playerController.MovCharController.PlayerAnimator;
            Transform originalVisual = originalAnimator.transform;
            Transform visualParent = originalVisual.parent != null ? originalVisual.parent : playerController.transform;

            RemoveExistingNewVisual(visualParent);
            RestoreRenderers(originalVisual);

            GameObject newVisual = PrefabUtility.InstantiatePrefab(characterPrefab, scene) as GameObject;
            if (newVisual == null)
            {
                Debug.LogWarning("v0.6 Synty visual swap failed to instantiate prefab: " + CharacterPrefabName);
                return;
            }

            newVisual.name = NewVisualName;
            newVisual.transform.SetParent(visualParent, false);
            newVisual.transform.localPosition = originalVisual.localPosition;
            newVisual.transform.localRotation = originalVisual.localRotation;
            newVisual.transform.localScale = Vector3.one * SyntyVisualScaleMultiplier;

            Animator newAnimator = newVisual.GetComponentInChildren<Animator>(true);
            if (newAnimator == null)
            {
                Object.DestroyImmediate(newVisual);
                Debug.LogWarning("v0.6 Synty visual swap found no Animator on " + CharacterPrefabName + ".");
                return;
            }

            newAnimator.runtimeAnimatorController = originalAnimator.runtimeAnimatorController;
            newAnimator.applyRootMotion = false;
            playerController.MovCharController.PlayerAnimator = newAnimator;

            HideSkinnedRenderersOnly(originalVisual);

            EditorUtility.SetDirty(playerController.MovCharController);
            EditorUtility.SetDirty(newAnimator);
            EditorUtility.SetDirty(newVisual);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("v0.6 Synty visual-only character swap complete at scale multiplier " + SyntyVisualScaleMultiplier + ". Only the scene character visual renderers and MovementCharacterController.PlayerAnimator were changed. WeaponData, ShooterController, ammo, projectiles, prefabs, and weapon scripts were not changed.");
        }

        private static GameObject FindExistingSyntyVisualInScene()
        {
            GameObject[] allObjects = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (GameObject sceneObject in allObjects)
            {
                if (sceneObject != null && sceneObject.name == NewVisualName)
                {
                    return sceneObject;
                }
            }

            return null;
        }

        private static GameObject FindExactPrefab(string prefabName)
        {
            string[] guids = AssetDatabase.FindAssets(prefabName + " t:Prefab", new[] { "Assets/Synty" });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetFileNameWithoutExtension(path) != prefabName)
                {
                    continue;
                }

                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null)
                {
                    return prefab;
                }
            }

            return null;
        }

        private static void RemoveExistingNewVisual(Transform visualParent)
        {
            Transform existing = visualParent.Find(NewVisualName);
            if (existing != null)
            {
                Object.DestroyImmediate(existing.gameObject);
            }
        }

        private static void HideSkinnedRenderersOnly(Transform root)
        {
            SkinnedMeshRenderer[] renderers = root.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            foreach (SkinnedMeshRenderer renderer in renderers)
            {
                if (renderer == null)
                {
                    continue;
                }

                renderer.enabled = false;
                if (!renderer.gameObject.name.StartsWith(HiddenRendererPrefix))
                {
                    renderer.gameObject.name = HiddenRendererPrefix + renderer.gameObject.name;
                }

                EditorUtility.SetDirty(renderer);
            }
        }

        private static void RestoreRenderers(Transform root)
        {
            SkinnedMeshRenderer[] renderers = root.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            foreach (SkinnedMeshRenderer renderer in renderers)
            {
                if (renderer == null)
                {
                    continue;
                }

                if (renderer.gameObject.name.StartsWith(HiddenRendererPrefix))
                {
                    renderer.gameObject.name = renderer.gameObject.name.Substring(HiddenRendererPrefix.Length);
                }

                renderer.enabled = true;
                EditorUtility.SetDirty(renderer);
            }
        }
    }
}

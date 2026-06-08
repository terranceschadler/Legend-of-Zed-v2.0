using System.IO;
using TopDownShooter;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LegendOfZed.Editor
{
    public static class ZedV07SyntyWeaponVisualAlignment
    {
        private const string ScenePath = "Assets/LegendOfZed/Scenes/Zed_Controller_Test.unity";
        private const string AnchorName = "Zed_Synty_RightHand_WeaponPosition";

        private static readonly Vector3 AnchorLocalPosition = new Vector3(0.055f, 0.015f, 0.02f);
        private static readonly Vector3 AnchorLocalEulerAngles = new Vector3(82f, 0f, 92f);

        [MenuItem("Legend of Zed/Setup/v0.7 Align Weapon Visual To Synty Hand")]
        public static void AlignWeaponVisualToSyntyHand()
        {
            if (!File.Exists(ScenePath))
            {
                Debug.LogWarning("v0.7 weapon visual alignment could not find scene: " + ScenePath);
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            ShooterController shooterController = Object.FindFirstObjectByType<ShooterController>();
            MovementCharacterController movementController = Object.FindFirstObjectByType<MovementCharacterController>();

            if (shooterController == null || movementController == null || movementController.PlayerAnimator == null)
            {
                Debug.LogWarning("v0.7 weapon visual alignment needs ShooterController, MovementCharacterController, and PlayerAnimator in the locked test scene.");
                return;
            }

            Animator animator = movementController.PlayerAnimator;
            Transform rightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);
            if (rightHand == null)
            {
                Debug.LogWarning("v0.7 weapon visual alignment could not find HumanBodyBones.RightHand on the current Synty PlayerAnimator.");
                return;
            }

            Transform anchor = rightHand.Find(AnchorName);
            if (anchor == null)
            {
                GameObject anchorObject = new GameObject(AnchorName);
                anchor = anchorObject.transform;
                anchor.SetParent(rightHand, false);
            }

            anchor.localPosition = AnchorLocalPosition;
            anchor.localEulerAngles = AnchorLocalEulerAngles;
            anchor.localScale = CalculateInverseWorldScale(rightHand.lossyScale);

            shooterController.WeaponPosition = anchor;

            EditorUtility.SetDirty(anchor.gameObject);
            EditorUtility.SetDirty(shooterController);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("v0.7 weapon visual alignment applied with hand-scale compensation. RightHand lossyScale=" + rightHand.lossyScale + ", Anchor localScale=" + anchor.localScale + ". No WeaponData, BulletId, ammo, projectile prefab, weapon database entry, ShooterController code, or controller/root transform was changed.");
        }

        private static Vector3 CalculateInverseWorldScale(Vector3 parentLossyScale)
        {
            return new Vector3(
                SafeInverse(parentLossyScale.x),
                SafeInverse(parentLossyScale.y),
                SafeInverse(parentLossyScale.z));
        }

        private static float SafeInverse(float value)
        {
            if (Mathf.Abs(value) < 0.0001f)
            {
                return 1f;
            }

            return 1f / value;
        }
    }
}

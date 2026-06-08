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
        private const string WeaponAnchorName = "Zed_Synty_RightHand_WeaponPosition";
        private const string BulletPointName = "Zed_Synty_Muzzle_BulletPoint";

        private static readonly Vector3 WeaponAnchorLocalPosition = new Vector3(0.067f, 0.003f, 0.015f);
        private static readonly Vector3 WeaponAnchorLocalEulerAngles = new Vector3(-22.004f, -181.868f, 102.104f);
        private static readonly Vector3 WeaponAnchorLocalScale = new Vector3(60.57f, 60.57f, 60.57f);

        // Child of the weapon socket. Adjust this in the scene if the visible muzzle needs final tuning.
        private static readonly Vector3 BulletPointLocalPosition = new Vector3(0.00155f, 0.00005f, 0.00015f);
        private static readonly Vector3 BulletPointLocalEulerAngles = Vector3.zero;
        private static readonly Vector3 BulletPointLocalScale = Vector3.one;

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

            Transform weaponAnchor = rightHand.Find(WeaponAnchorName);
            if (weaponAnchor == null)
            {
                GameObject anchorObject = new GameObject(WeaponAnchorName);
                weaponAnchor = anchorObject.transform;
                weaponAnchor.SetParent(rightHand, false);
            }

            weaponAnchor.localPosition = WeaponAnchorLocalPosition;
            weaponAnchor.localEulerAngles = WeaponAnchorLocalEulerAngles;
            weaponAnchor.localScale = WeaponAnchorLocalScale;

            Transform bulletPoint = weaponAnchor.Find(BulletPointName);
            if (bulletPoint == null)
            {
                GameObject bulletPointObject = new GameObject(BulletPointName);
                bulletPoint = bulletPointObject.transform;
                bulletPoint.SetParent(weaponAnchor, false);
            }

            bulletPoint.localPosition = BulletPointLocalPosition;
            bulletPoint.localEulerAngles = BulletPointLocalEulerAngles;
            bulletPoint.localScale = BulletPointLocalScale;

            shooterController.WeaponPosition = weaponAnchor;
            shooterController.BulletPoint = bulletPoint;

            EditorUtility.SetDirty(weaponAnchor.gameObject);
            EditorUtility.SetDirty(bulletPoint.gameObject);
            EditorUtility.SetDirty(shooterController);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("v0.7 weapon and bullet point alignment applied. WeaponPosition=" + WeaponAnchorName + ", BulletPoint=" + BulletPointName + ". No WeaponData, BulletId, ammo, projectile prefab, weapon database entry, ShooterController code, or controller/root transform was changed.");
        }
    }
}

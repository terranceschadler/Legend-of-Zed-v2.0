using System.IO;
using LegendOfZed.Feedback;
using TopDownShooter;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LegendOfZed.Editor
{
    public static class ZedV09ShellEjectSetupTool
    {
        private const string ScenePath = "Assets/LegendOfZed/Scenes/Zed_Controller_Test.unity";
        private const string ShellPrefabName = "Bullet_Shell_FX";
        private const string SyntyFxFolder = "Assets/Synty/PolygonBattleRoyale/Prefabs/FX";
        private const string EjectionPointName = "Zed_V09_Shell_Ejection_Point";

        [MenuItem("Legend of Zed/Setup/v0.9 Setup Shell Eject Feedback")]
        public static void SetupShellEjectFeedback()
        {
            if (!File.Exists(ScenePath))
            {
                Debug.LogWarning("v0.9 shell eject setup could not find scene: " + ScenePath);
                return;
            }

            GameObject shellPrefab = FindShellPrefab();
            if (shellPrefab == null)
            {
                Debug.LogWarning("v0.9 shell eject setup could not find " + ShellPrefabName + " under " + SyntyFxFolder + ".");
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            ShooterController shooterController = Object.FindFirstObjectByType<ShooterController>();
            if (shooterController == null)
            {
                Debug.LogWarning("v0.9 shell eject setup could not find ShooterController in the locked test scene.");
                return;
            }

            ZedShellEjectOnShot shellEject = shooterController.GetComponent<ZedShellEjectOnShot>();
            if (shellEject == null)
            {
                shellEject = shooterController.gameObject.AddComponent<ZedShellEjectOnShot>();
            }

            Transform ejectionPoint = FindOrCreateEjectionPoint(shooterController);

            shellEject.ShooterController = shooterController;
            shellEject.ShellFxPrefab = shellPrefab;
            shellEject.EjectionPoint = ejectionPoint;
            shellEject.PositionOffset = Vector3.zero;
            shellEject.RotationOffsetEuler = Vector3.zero;
            shellEject.SpawnVelocity = new Vector3(0.35f, 0.25f, -0.18f);
            shellEject.ShellScale = 1f;
            shellEject.ShellLifetime = 4f;

            EditorUtility.SetDirty(ejectionPoint.gameObject);
            EditorUtility.SetDirty(shellEject);
            EditorUtility.SetDirty(shooterController.gameObject);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("v0.9 shell eject feedback setup complete. Shells spawn from " + EjectionPointName + " when magazine count drops. No ShooterController code, WeaponData, BulletId, ammo, projectiles, or weapon prefabs were changed.");
        }

        private static GameObject FindShellPrefab()
        {
            string[] guids = AssetDatabase.FindAssets(ShellPrefabName + " t:Prefab", new[] { SyntyFxFolder });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetFileNameWithoutExtension(path) != ShellPrefabName)
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

        private static Transform FindOrCreateEjectionPoint(ShooterController shooterController)
        {
            Transform parent = shooterController.WeaponPosition != null ? shooterController.WeaponPosition : shooterController.transform;
            Transform existing = parent.Find(EjectionPointName);
            if (existing != null)
            {
                return existing;
            }

            GameObject ejectionPointObject = new GameObject(EjectionPointName);
            Transform ejectionPoint = ejectionPointObject.transform;
            ejectionPoint.SetParent(parent, false);
            ejectionPoint.localPosition = new Vector3(-0.015f, 0.006f, -0.006f);
            ejectionPoint.localEulerAngles = Vector3.zero;
            ejectionPoint.localScale = Vector3.one;
            return ejectionPoint;
        }
    }
}

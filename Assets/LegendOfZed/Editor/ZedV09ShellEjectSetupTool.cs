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
        private const string ShellAssetName = "SM_Wep_Bullet_Ammo_01";
        private const string SyntyRootFolder = "Assets/Synty";
        private const string EjectionPointName = "Zed_V09_Shell_Ejection_Point";

        [MenuItem("Legend of Zed/Setup/v0.9 Setup Single Shell Mesh Eject Feedback")]
        public static void SetupSingleShellMeshEjectFeedback()
        {
            if (!File.Exists(ScenePath))
            {
                Debug.LogWarning("v0.9 shell eject setup could not find scene: " + ScenePath);
                return;
            }

            GameObject shellMesh = FindShellMeshAsset();
            if (shellMesh == null)
            {
                Debug.LogWarning("v0.9 shell eject setup could not find " + ShellAssetName + " under " + SyntyRootFolder + ". Search Project for the asset and confirm its exact name.");
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            ShooterController shooterController = Object.FindFirstObjectByType<ShooterController>();
            if (shooterController == null)
            {
                Debug.LogWarning("v0.9 shell eject setup could not find ShooterController in the locked test scene.");
                return;
            }

            ZedSingleShellMeshEjectOnShot shellEject = shooterController.GetComponent<ZedSingleShellMeshEjectOnShot>();
            if (shellEject == null)
            {
                shellEject = shooterController.gameObject.AddComponent<ZedSingleShellMeshEjectOnShot>();
            }

            Transform ejectionPoint = FindOrCreateEjectionPoint(shooterController);

            shellEject.ShooterController = shooterController;
            shellEject.ShellMeshPrefab = shellMesh;
            shellEject.EjectionPoint = ejectionPoint;
            shellEject.PositionOffset = Vector3.zero;
            shellEject.RotationOffsetEuler = Vector3.zero;
            shellEject.TossVelocity = new Vector3(0.3f, 0.22f, -0.16f);
            shellEject.RandomAngularVelocity = new Vector3(180f, 360f, 180f);
            shellEject.ShellScale = 1f;
            shellEject.ShellLifetime = 4f;

            EditorUtility.SetDirty(ejectionPoint.gameObject);
            EditorUtility.SetDirty(shellEject);
            EditorUtility.SetDirty(shooterController.gameObject);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("v0.9 single shell mesh eject setup complete using " + ShellAssetName + ". One shell mesh spawns when magazine count drops. No ShooterController code, WeaponData, BulletId, ammo, projectiles, or weapon prefabs were changed.");
        }

        private static GameObject FindShellMeshAsset()
        {
            string[] guids = AssetDatabase.FindAssets(ShellAssetName + " t:Prefab", new[] { SyntyRootFolder });
            GameObject shell = FindExactAssetFromGuids(guids);
            if (shell != null)
            {
                return shell;
            }

            guids = AssetDatabase.FindAssets(ShellAssetName + " t:Model", new[] { SyntyRootFolder });
            return FindExactAssetFromGuids(guids);
        }

        private static GameObject FindExactAssetFromGuids(string[] guids)
        {
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetFileNameWithoutExtension(path) != ShellAssetName)
                {
                    continue;
                }

                GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (asset != null)
                {
                    return asset;
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

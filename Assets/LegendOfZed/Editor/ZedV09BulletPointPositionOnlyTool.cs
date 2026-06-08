using System.IO;
using TopDownShooter;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LegendOfZed.Editor
{
    public static class ZedV09BulletPointPositionOnlyTool
    {
        private const string ScenePath = "Assets/LegendOfZed/Scenes/Zed_Controller_Test.unity";
        private const string MarkerName = "Zed_V09_BulletPoint_Position_Only_Marker";

        [MenuItem("Legend of Zed/Setup/v0.9 Create BulletPoint Position Marker")]
        public static void CreateBulletPointPositionMarker()
        {
            if (!GetControllerTestScene(out Scene scene))
            {
                return;
            }

            ShooterController shooterController = Object.FindFirstObjectByType<ShooterController>();
            if (!ValidateShooterController(shooterController))
            {
                return;
            }

            Transform bulletPoint = shooterController.BulletPoint;
            GameObject marker = FindOrCreateMarker(scene);

            marker.transform.position = bulletPoint.position;
            marker.transform.rotation = bulletPoint.rotation;
            marker.transform.localScale = Vector3.one * 0.25f;

            Selection.activeGameObject = marker;

            SaveIfNotPlaying(scene, marker, null, null);

            Debug.Log("v0.9 BulletPoint position marker created at the existing BulletPoint. Move only this marker's position to the visible muzzle, then run 'Apply Marker Position To BulletPoint Only'.");
        }

        [MenuItem("Legend of Zed/Setup/v0.9 Apply Marker Position To BulletPoint Only")]
        public static void ApplyMarkerPositionToBulletPointOnly()
        {
            if (!GetControllerTestScene(out Scene scene))
            {
                return;
            }

            ShooterController shooterController = Object.FindFirstObjectByType<ShooterController>();
            if (!ValidateShooterController(shooterController))
            {
                return;
            }

            GameObject marker = GameObject.Find(MarkerName);
            if (marker == null)
            {
                Debug.LogWarning("v0.9 BulletPoint marker was not found. Run 'Create BulletPoint Position Marker' first.");
                return;
            }

            Transform bulletPoint = shooterController.BulletPoint;
            Quaternion preservedWorldRotation = bulletPoint.rotation;
            Vector3 preservedLocalEulerAngles = bulletPoint.localEulerAngles;
            Transform preservedParent = bulletPoint.parent;

            bulletPoint.position = marker.transform.position;
            bulletPoint.rotation = preservedWorldRotation;
            bulletPoint.localEulerAngles = preservedLocalEulerAngles;

            if (bulletPoint.parent != preservedParent)
            {
                bulletPoint.SetParent(preservedParent, true);
            }

            SaveIfNotPlaying(scene, bulletPoint.gameObject, shooterController, null);

            Debug.Log("v0.9 applied marker position to ShooterController.BulletPoint only. BulletPoint rotation and parent were preserved. No WeaponData, BulletId, ammo, projectile, ShooterController code, or weapon prefab was changed." +
                      (EditorApplication.isPlaying ? " NOTE: this was applied during Play Mode, so copy the final BulletPoint position before exiting Play Mode and paste it again in Edit Mode if you want it saved." : string.Empty));
        }

        [MenuItem("Legend of Zed/Setup/v0.9 Delete BulletPoint Position Marker")]
        public static void DeleteBulletPointPositionMarker()
        {
            if (!GetControllerTestScene(out Scene scene))
            {
                return;
            }

            GameObject marker = GameObject.Find(MarkerName);
            if (marker != null)
            {
                Object.DestroyImmediate(marker);
                SaveIfNotPlaying(scene, null, null, null);
                Debug.Log("v0.9 BulletPoint position marker deleted.");
            }
        }

        private static bool GetControllerTestScene(out Scene scene)
        {
            scene = default;

            if (EditorApplication.isPlaying)
            {
                scene = SceneManager.GetActiveScene();
                if (!scene.IsValid())
                {
                    Debug.LogWarning("v0.9 BulletPoint tool could not get the active Play Mode scene.");
                    return false;
                }

                return true;
            }

            if (!File.Exists(ScenePath))
            {
                Debug.LogWarning("v0.9 BulletPoint tool could not find scene: " + ScenePath);
                return false;
            }

            scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            return scene.IsValid();
        }

        private static void SaveIfNotPlaying(Scene scene, Object primaryDirtyObject, Object secondaryDirtyObject, Object tertiaryDirtyObject)
        {
            if (primaryDirtyObject != null)
            {
                EditorUtility.SetDirty(primaryDirtyObject);
            }

            if (secondaryDirtyObject != null)
            {
                EditorUtility.SetDirty(secondaryDirtyObject);
            }

            if (tertiaryDirtyObject != null)
            {
                EditorUtility.SetDirty(tertiaryDirtyObject);
            }

            if (EditorApplication.isPlaying)
            {
                return;
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static bool ValidateShooterController(ShooterController shooterController)
        {
            if (shooterController == null)
            {
                Debug.LogWarning("v0.9 BulletPoint tool could not find ShooterController in the locked test scene.");
                return false;
            }

            if (shooterController.BulletPoint == null)
            {
                Debug.LogWarning("v0.9 BulletPoint tool found ShooterController, but BulletPoint is unassigned. Restore the original package BulletPoint reference before using this tool.");
                return false;
            }

            return true;
        }

        private static GameObject FindOrCreateMarker(Scene scene)
        {
            GameObject marker = GameObject.Find(MarkerName);
            if (marker != null)
            {
                return marker;
            }

            marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            marker.name = MarkerName;
            marker.hideFlags = HideFlags.None;
            Object.DestroyImmediate(marker.GetComponent<Collider>());
            EditorSceneManager.MoveGameObjectToScene(marker, scene);
            return marker;
        }
    }
}

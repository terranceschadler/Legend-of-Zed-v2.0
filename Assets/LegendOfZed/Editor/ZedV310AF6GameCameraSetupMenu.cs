#if UNITY_EDITOR
using TopDownShooter;
using UnityEditor;
using UnityEngine;

namespace LegendOfZed.EditorTools
{
    public static class ZedV310AF6GameCameraSetupMenu
    {
        private const string MenuRoot = "Legend Of Zed/Camera/";

        [MenuItem(MenuRoot + "Setup Top Down Follow Camera")]
        public static void SetupTopDownFollowCamera()
        {
            Transform player = ResolveSelectedOrScenePlayer();
            Camera camera = Camera.main;

            if (camera == null)
            {
                GameObject cameraObject = new GameObject("Main Camera");
                Undo.RegisterCreatedObjectUndo(cameraObject, "Create top down game camera");
                camera = cameraObject.AddComponent<Camera>();
                TrySetTag(cameraObject, "MainCamera");
            }
            else
            {
                Undo.RecordObject(camera.gameObject, "Setup top down game camera");
                Undo.RecordObject(camera.transform, "Setup top down game camera");
                Undo.RecordObject(camera, "Setup top down game camera");
            }

            TopDownCamera followCamera = camera.GetComponent<TopDownCamera>();
            if (followCamera == null)
            {
                followCamera = Undo.AddComponent<TopDownCamera>(camera.gameObject);
            }
            else
            {
                Undo.RecordObject(followCamera, "Setup top down follow camera");
            }

            followCamera.ApplyDefaultTopDownPerspective();
            followCamera.Target = player;
            followCamera.PlayerTag = "Player";
            followCamera.AutoFindPlayer = true;
            followCamera.SnapToTarget();

            camera.orthographic = false;
            camera.fieldOfView = followCamera.FieldOfView;
            TrySetTag(camera.gameObject, "MainCamera");

            EditorUtility.SetDirty(camera);
            EditorUtility.SetDirty(camera.transform);
            EditorUtility.SetDirty(followCamera);
            Selection.activeGameObject = camera.gameObject;

            Debug.Log("Top down follow camera setup complete. Target=" + (player != null ? player.name : "auto-find Player at runtime") + ".", camera);
        }

        [MenuItem(MenuRoot + "Snap Camera To Player Now")]
        public static void SnapCameraToPlayerNow()
        {
            Camera camera = Camera.main;
            if (camera == null)
            {
                Debug.LogWarning("No Main Camera found. Run Setup Top Down Follow Camera first.");
                return;
            }

            TopDownCamera followCamera = camera.GetComponent<TopDownCamera>();
            if (followCamera == null)
            {
                Debug.LogWarning("Main Camera has no TopDownCamera component. Run Setup Top Down Follow Camera first.", camera);
                return;
            }

            Undo.RecordObject(camera.transform, "Snap top down camera to player");
            followCamera.Target = followCamera.Target != null ? followCamera.Target : ResolveSelectedOrScenePlayer();
            followCamera.SnapToTarget();
            EditorUtility.SetDirty(camera.transform);
        }

        private static Transform ResolveSelectedOrScenePlayer()
        {
            if (Selection.activeTransform != null)
            {
                PlayerController selectedPlayer = Selection.activeTransform.GetComponentInParent<PlayerController>();
                if (selectedPlayer != null)
                {
                    return selectedPlayer.transform;
                }

                if (Selection.activeTransform.CompareTag("Player"))
                {
                    return Selection.activeTransform;
                }
            }

            GameObject taggedPlayer = FindGameObjectWithTagSafe("Player");
            if (taggedPlayer != null)
            {
                return taggedPlayer.transform;
            }

            PlayerController scenePlayer = Object.FindFirstObjectByType<PlayerController>();
            return scenePlayer != null ? scenePlayer.transform : null;
        }

        private static GameObject FindGameObjectWithTagSafe(string tagName)
        {
            try
            {
                return GameObject.FindGameObjectWithTag(tagName);
            }
            catch (UnityException)
            {
                return null;
            }
        }

        private static void TrySetTag(GameObject target, string tagName)
        {
            if (target == null || string.IsNullOrEmpty(tagName))
            {
                return;
            }

            try
            {
                target.tag = tagName;
            }
            catch (UnityException)
            {
                // MainCamera normally exists, but keep this safe for damaged tag setups.
            }
        }
    }
}
#endif

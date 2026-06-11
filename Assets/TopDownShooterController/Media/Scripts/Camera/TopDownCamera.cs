using LegendOfZed.Input;
using UnityEngine;

namespace TopDownShooter
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    public class TopDownCamera : MonoBehaviour
    {
        [Header("Target")]
        public Transform Target;
        public string PlayerTag = "Player";
        public bool AutoFindPlayer = true;

        [Header("Top Down Perspective")]
        [Tooltip("Vertical height above the player.")]
        public float Height = 18f;

        [Tooltip("Backward distance from the player. Keep this above zero for angled top-down perspective.")]
        public float Distance = 12f;

        [Tooltip("How far ahead of the player forward direction the camera looks.")]
        public float SeeForward = 0f;

        [Tooltip("Height above the player's pivot to look at.")]
        public float LookAtHeight = 1.2f;

        [Tooltip("World yaw of the camera orbit. 0 keeps the camera behind world north/south depending on Distance.")]
        public float Yaw = 0f;

        [Tooltip("Force this camera to use perspective projection.")]
        public bool ForcePerspective = true;

        [Tooltip("Perspective field of view.")]
        public float FieldOfView = 45f;

        [Header("Follow")]
        [Tooltip("The speed with which the camera follows the player.")]
        public float Smoothing = 8f;

        public float RotationSmoothing = 12f;
        public bool SnapOnStart = true;

        [Header("Optional Rotation Input")]
        public bool AllowRotationInput = false;
        public float RotationSpeed = 90f;

        [Header("Bounds")]
        public bool ClampToBounds = false;
        public Vector2 MinBounds = new Vector2(-500f, -500f);
        public Vector2 MaxBounds = new Vector2(500f, 500f);

        private Camera _camera;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
            ApplyCameraSettings();
        }

        private void Start()
        {
            ResolveTarget();

            if (SnapOnStart)
            {
                SnapToTarget();
            }
        }

        private void LateUpdate()
        {
            if (Target == null && AutoFindPlayer)
            {
                ResolveTarget();
            }

            if (Target == null)
            {
                return;
            }

            ApplyCameraSettings();
            ReadRotationInput();

            Vector3 desiredPosition = GetDesiredPosition();
            Quaternion desiredRotation = GetDesiredRotation(desiredPosition);

            float followT = 1f - Mathf.Exp(-Mathf.Max(0.01f, Smoothing) * Time.deltaTime);
            float rotationT = 1f - Mathf.Exp(-Mathf.Max(0.01f, RotationSmoothing) * Time.deltaTime);

            transform.position = Vector3.Lerp(transform.position, desiredPosition, followT);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationT);
        }

        public void SetTarget(Transform newTarget, bool snapNow)
        {
            Target = newTarget;

            if (snapNow)
            {
                SnapToTarget();
            }
        }

        [ContextMenu("Snap To Target")]
        public void SnapToTarget()
        {
            if (Target == null)
            {
                ResolveTarget();
            }

            if (Target == null)
            {
                return;
            }

            ApplyCameraSettings();
            Vector3 desiredPosition = GetDesiredPosition();
            transform.position = desiredPosition;
            transform.rotation = GetDesiredRotation(desiredPosition);
        }

        public void ApplyDefaultTopDownPerspective()
        {
            Height = 18f;
            Distance = 12f;
            SeeForward = 0f;
            LookAtHeight = 1.2f;
            Yaw = 0f;
            ForcePerspective = true;
            FieldOfView = 45f;
            Smoothing = 8f;
            RotationSmoothing = 12f;
            AllowRotationInput = false;
        }

        private void ResolveTarget()
        {
            if (Target != null || !AutoFindPlayer)
            {
                return;
            }

            GameObject taggedPlayer = FindGameObjectWithTagSafe(PlayerTag);
            if (taggedPlayer != null)
            {
                Target = taggedPlayer.transform;
                return;
            }

            PlayerController playerController = FindAnyObjectByType<PlayerController>();
            if (playerController != null)
            {
                Target = playerController.transform;
            }
        }

        private void ApplyCameraSettings()
        {
            if (_camera == null)
            {
                _camera = GetComponent<Camera>();
            }

            if (_camera == null)
            {
                return;
            }

            if (ForcePerspective)
            {
                _camera.orthographic = false;
            }

            _camera.fieldOfView = Mathf.Clamp(FieldOfView, 20f, 80f);
        }

        private void ReadRotationInput()
        {
            if (!AllowRotationInput)
            {
                return;
            }

            ZedInputReader input = ZedInputReader.Instance;
            if (input == null)
            {
                return;
            }

            if (input.RotateCameraLeftHeld)
            {
                Yaw -= RotationSpeed * Time.deltaTime;
            }

            if (input.RotateCameraRightHeld)
            {
                Yaw += RotationSpeed * Time.deltaTime;
            }

            Yaw = Mathf.Repeat(Yaw, 360f);
        }

        private Vector3 GetDesiredPosition()
        {
            Vector3 lookTarget = GetLookTarget();
            Quaternion yawRotation = Quaternion.Euler(0f, Yaw, 0f);
            Vector3 offset = yawRotation * new Vector3(0f, Mathf.Max(2f, Height), -Mathf.Max(0f, Distance));
            Vector3 desired = lookTarget + offset;

            if (ClampToBounds)
            {
                desired.x = Mathf.Clamp(desired.x, MinBounds.x, MaxBounds.x);
                desired.z = Mathf.Clamp(desired.z, MinBounds.y, MaxBounds.y);
            }

            return desired;
        }

        private Quaternion GetDesiredRotation(Vector3 fromPosition)
        {
            Vector3 direction = GetLookTarget() - fromPosition;
            if (direction.sqrMagnitude <= 0.0001f)
            {
                return transform.rotation;
            }

            return Quaternion.LookRotation(direction.normalized, Vector3.up);
        }

        private Vector3 GetLookTarget()
        {
            if (Target == null)
            {
                return transform.position + transform.forward;
            }

            Vector3 forward = Target.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude <= 0.0001f)
            {
                forward = Vector3.forward;
            }

            return Target.position + Vector3.up * LookAtHeight + forward.normalized * SeeForward;
        }

        private static GameObject FindGameObjectWithTagSafe(string tagName)
        {
            if (string.IsNullOrEmpty(tagName))
            {
                return null;
            }

            try
            {
                return GameObject.FindGameObjectWithTag(tagName);
            }
            catch (UnityException)
            {
                return null;
            }
        }
    }
}

using TopDownShooter;
using UnityEngine;

namespace LegendOfZed.Feedback
{
    public class ZedSingleShellMeshEjectOnShot : MonoBehaviour
    {
        [Header("References")]
        public ShooterController ShooterController;
        public GameObject ShellMeshPrefab;
        public Transform EjectionPoint;

        [Header("Visual Tuning")]
        public Vector3 PositionOffset = Vector3.zero;
        public Vector3 RotationOffsetEuler = Vector3.zero;
        public Vector3 TossVelocity = new Vector3(0.02f, 0.28f, -0.55f);
        public Vector3 RandomAngularVelocity = new Vector3(240f, 540f, 240f);
        public float ShellScale = 1.6f;
        public float ShellLifetime = 6f;
        public float ShellMass = 0.025f;
        public PhysicsMaterial ShellBounceMaterial;

        private int _lastWeaponIndex = int.MinValue;
        private int _lastMagazineCount = -1;
        private bool _hasBaseline;

        private void Reset()
        {
            ShooterController = GetComponent<ShooterController>();
        }

        private void Awake()
        {
            if (ShooterController == null)
            {
                ShooterController = GetComponent<ShooterController>();
            }
        }

        private void LateUpdate()
        {
            if (ShooterController == null || ShellMeshPrefab == null)
            {
                return;
            }

            int weaponIndex = ShooterController.CurrentDbWeaponIndex;
            if (weaponIndex < 0)
            {
                ClearBaseline();
                return;
            }

            int magazineCount;
            if (!TryGetCurrentMagazineCount(weaponIndex, out magazineCount))
            {
                ClearBaseline();
                return;
            }

            if (!_hasBaseline || _lastWeaponIndex != weaponIndex)
            {
                _lastWeaponIndex = weaponIndex;
                _lastMagazineCount = magazineCount;
                _hasBaseline = true;
                return;
            }

            if (magazineCount < _lastMagazineCount)
            {
                int shotsFired = Mathf.Clamp(_lastMagazineCount - magazineCount, 1, 3);
                for (int i = 0; i < shotsFired; i++)
                {
                    SpawnSingleShell();
                }
            }

            _lastMagazineCount = magazineCount;
        }

        private bool TryGetCurrentMagazineCount(int weaponIndex, out int magazineCount)
        {
            magazineCount = 0;

            if (ShooterController.WeaponsBullets == null)
            {
                return false;
            }

            for (int i = 0; i < ShooterController.WeaponsBullets.Count; i++)
            {
                WeaponsBullets weaponBullets = ShooterController.WeaponsBullets[i];
                if (weaponBullets != null && weaponBullets.WeaponLoadIndex == weaponIndex)
                {
                    magazineCount = weaponBullets.WeaponCurrentBullets;
                    return true;
                }
            }

            return false;
        }

        private void SpawnSingleShell()
        {
            Transform source = EjectionPoint != null ? EjectionPoint : ShooterController.BulletPoint;
            if (source == null)
            {
                return;
            }

            Quaternion rotation = source.rotation * Quaternion.Euler(RotationOffsetEuler);
            Vector3 position = source.TransformPoint(PositionOffset);

            GameObject shell = Instantiate(ShellMeshPrefab, position, rotation);
            shell.transform.localScale = shell.transform.localScale * ShellScale;

            Rigidbody shellRigidbody = shell.GetComponent<Rigidbody>();
            if (shellRigidbody == null)
            {
                shellRigidbody = shell.AddComponent<Rigidbody>();
            }

            Collider shellCollider = shell.GetComponent<Collider>();
            if (shellCollider == null)
            {
                shellCollider = shell.AddComponent<BoxCollider>();
            }

            if (ShellBounceMaterial != null)
            {
                shellCollider.sharedMaterial = ShellBounceMaterial;
            }

            shellRigidbody.mass = ShellMass;
            shellRigidbody.useGravity = true;
            shellRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            shellRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            shellRigidbody.linearDamping = 0.08f;
            shellRigidbody.angularDamping = 0.05f;
            shellRigidbody.linearVelocity = source.TransformDirection(TossVelocity);
            shellRigidbody.angularVelocity = new Vector3(
                Random.Range(-RandomAngularVelocity.x, RandomAngularVelocity.x),
                Random.Range(-RandomAngularVelocity.y, RandomAngularVelocity.y),
                Random.Range(-RandomAngularVelocity.z, RandomAngularVelocity.z)) * Mathf.Deg2Rad;

            if (ShellLifetime > 0f)
            {
                Destroy(shell, ShellLifetime);
            }
        }

        private void ClearBaseline()
        {
            _lastWeaponIndex = int.MinValue;
            _lastMagazineCount = -1;
            _hasBaseline = false;
        }
    }
}

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
        public Vector3 TossVelocity = new Vector3(0.3f, 0.22f, -0.16f);
        public Vector3 RandomAngularVelocity = new Vector3(180f, 360f, 180f);
        public float ShellScale = 1f;
        public float ShellLifetime = 4f;

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

            shellRigidbody.mass = 0.02f;
            shellRigidbody.linearDamping = 0.1f;
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

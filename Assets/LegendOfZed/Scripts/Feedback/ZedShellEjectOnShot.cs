using TopDownShooter;
using UnityEngine;

namespace LegendOfZed.Feedback
{
    public class ZedShellEjectOnShot : MonoBehaviour
    {
        [Header("References")]
        public ShooterController ShooterController;
        public GameObject ShellFxPrefab;
        public Transform EjectionPoint;

        [Header("Visual Tuning")]
        public Vector3 PositionOffset = Vector3.zero;
        public Vector3 RotationOffsetEuler = Vector3.zero;
        public Vector3 SpawnVelocity = new Vector3(0.4f, 0.25f, -0.2f);
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
            if (ShooterController == null || ShellFxPrefab == null)
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
                int shotsFired = Mathf.Clamp(_lastMagazineCount - magazineCount, 1, 5);
                for (int i = 0; i < shotsFired; i++)
                {
                    SpawnShell();
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

        private void SpawnShell()
        {
            Transform source = EjectionPoint != null ? EjectionPoint : ShooterController.BulletPoint;
            if (source == null)
            {
                return;
            }

            Quaternion rotation = source.rotation * Quaternion.Euler(RotationOffsetEuler);
            Vector3 position = source.TransformPoint(PositionOffset);

            GameObject shell = Instantiate(ShellFxPrefab, position, rotation);
            shell.transform.localScale = shell.transform.localScale * ShellScale;

            Rigidbody shellRigidbody = shell.GetComponentInChildren<Rigidbody>();
            if (shellRigidbody != null)
            {
                shellRigidbody.linearVelocity = source.TransformDirection(SpawnVelocity);
            }

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

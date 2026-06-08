using LegendOfZed.Enemies;
using LegendOfZed.Runtime;
using UnityEngine;

namespace TopDownShooter
{
    public class Damage : MonoBehaviour
    {
        public float DamagePower;
        public GameObject BulletImpact;
        public GameObject EnemyBulletImpact;

        [Header("Impact Cleanup")]
        public float BulletImpactLifetime = 2f;
        public float EnemyBulletImpactLifetime = 3f;

        [Header("DamageOnExplode")]
        public bool ExplodeDamageBullet;
        public float DamageRadius;
        public float ExplosionForcePower;
        public LayerMask RayCasterLayer;

        public Rigidbody RigidBodyComponent;
        private bool _impact;

        private void Awake()
        {
            if (!RigidBodyComponent)
            {
                RigidBodyComponent = GetComponent<Rigidbody>();
            }
        }

        public void SetupBullet(Vector3 launchForce, float bulletDamage)
        {
            DamagePower = bulletDamage;
            RigidBodyComponent.AddForce(launchForce);
        }

        public void SetupMeleeAttack(float damage)
        {
            DamagePower = damage;
        }

        private void ExplodeDamage()
        {
            Collider[] hitCollider = Physics.OverlapSphere(transform.position, DamageRadius, RayCasterLayer);
            foreach (Collider hit in hitCollider)
            {
                Rigidbody rigidBody = hit.GetComponent<Rigidbody>();

                ZedPrototypeZombieEnemy zedEnemy = hit.GetComponentInParent<ZedPrototypeZombieEnemy>();
                if (zedEnemy != null)
                {
                    zedEnemy.ApplyDamage(DamagePower);

                    ZedZombieHitReactionMotor hitReaction = zedEnemy.GetComponent<ZedZombieHitReactionMotor>();
                    if (hitReaction != null)
                    {
                        hitReaction.PlayBulletHitReaction();
                    }
                }
                else
                {
                    HitPoint hitPoint = hit.GetComponent<HitPoint>();
                    if (hitPoint != null)
                    {
                        hitPoint.ApplyDamage(DamagePower);
                    }
                }

                if (rigidBody)
                {
                    rigidBody.AddExplosionForce(ExplosionForcePower, transform.position, DamageRadius);
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_impact)
            {
                return;
            }

            _impact = true;

            if (!ExplodeDamageBullet)
            {
                ApplyDirectHitDamage(other);
            }

            SpawnImpactFeedback(other, other.ClosestPointOnBounds(transform.position));

            if (ExplodeDamageBullet)
            {
                ExplodeDamage();
            }

            gameObject.SetActive(false);
            Destroy(gameObject, 0.1f);
        }

        private void OnCollisionEnter(Collision other)
        {
            if (_impact)
            {
                return;
            }

            _impact = true;

            if (!ExplodeDamageBullet)
            {
                ApplyDirectHitDamage(other.collider);
            }

            Vector3 contactPoint = other.contacts.Length > 0 ? other.contacts[0].point : transform.position;
            SpawnImpactFeedback(other.collider, contactPoint);

            if (ExplodeDamageBullet)
            {
                ExplodeDamage();
            }

            gameObject.SetActive(false);
            Destroy(gameObject, 0.1f);
        }

        private void ApplyDirectHitDamage(Collider hitCollider)
        {
            if (hitCollider == null)
            {
                return;
            }

            ZedPrototypeZombieEnemy zedEnemy = hitCollider.GetComponentInParent<ZedPrototypeZombieEnemy>();
            if (zedEnemy != null)
            {
                zedEnemy.ApplyDamage(DamagePower);

                ZedZombieHitReactionMotor hitReaction = zedEnemy.GetComponent<ZedZombieHitReactionMotor>();
                if (hitReaction != null)
                {
                    hitReaction.PlayBulletHitReaction();
                }

                return;
            }

            HitPoint hitPoint = hitCollider.GetComponent<HitPoint>();
            if (hitPoint == null)
            {
                hitPoint = hitCollider.GetComponentInParent<HitPoint>();
            }

            if (hitPoint != null)
            {
                hitPoint.ApplyDamage(DamagePower);
            }
        }

        private void SpawnImpactFeedback(Collider hitCollider, Vector3 hitPoint)
        {
            GameObject impactPrefab = GetImpactPrefab(hitCollider);
            if (!impactPrefab)
            {
                return;
            }

            GameObject spawnedImpact = Instantiate(impactPrefab, hitPoint, impactPrefab.transform.rotation);
            ZedAutoDestroyAfterSeconds autoDestroy = spawnedImpact.GetComponent<ZedAutoDestroyAfterSeconds>();
            if (autoDestroy == null)
            {
                autoDestroy = spawnedImpact.AddComponent<ZedAutoDestroyAfterSeconds>();
            }

            autoDestroy.Lifetime = IsEnemyHit(hitCollider) ? EnemyBulletImpactLifetime : BulletImpactLifetime;
        }

        private GameObject GetImpactPrefab(Collider hitCollider)
        {
            if (hitCollider != null && EnemyBulletImpact != null && IsEnemyHit(hitCollider))
            {
                return EnemyBulletImpact;
            }

            return BulletImpact;
        }

        private static bool IsEnemyHit(Collider hitCollider)
        {
            return hitCollider != null &&
                   (hitCollider.GetComponentInParent<ZedPrototypeZombieEnemy>() != null ||
                    hitCollider.GetComponent<HitPoint>() != null ||
                    hitCollider.GetComponentInParent<HitPoint>() != null);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(transform.position, DamageRadius);
        }
    }
}

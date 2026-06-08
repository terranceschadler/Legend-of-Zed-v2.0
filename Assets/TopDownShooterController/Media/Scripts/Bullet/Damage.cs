using System;
using LegendOfZed.Enemies;
using UnityEngine;

namespace TopDownShooter
{
    public class Damage : MonoBehaviour
    {
        public float DamagePower;
        public GameObject BulletImpact;
        public GameObject EnemyBulletImpact;

        [Header("DamageOnExplode")] public bool ExplodeDamageBullet;
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
            RigidBodyComponent
                .AddForce(launchForce);
        }

        public void SetupMeleeAttack(float damage)
        {
            DamagePower = damage;
        }

        private void ExplodeDamage()
        {
            var hitCollider = Physics.OverlapSphere(transform.position, DamageRadius, RayCasterLayer);
            foreach (var hit in hitCollider)
            {
                var rigidBody = hit.GetComponent<Rigidbody>();

                if (hit.GetComponent<HitPoint>())
                {
                    hit.GetComponent<HitPoint>().ApplyDamage(DamagePower);
                }

                ZedPrototypeZombieEnemy zedEnemy = hit.GetComponentInParent<ZedPrototypeZombieEnemy>();
                if (zedEnemy != null)
                {
                    zedEnemy.ApplyDamage(DamagePower);
                }

                if (rigidBody)
                {
                    rigidBody.AddExplosionForce(ExplosionForcePower, transform.position, DamageRadius);
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_impact) return;
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
            if (_impact) return;
            _impact = true;
            if (!ExplodeDamageBullet)
            {
                ApplyDirectHitDamage(other.collider);
            }

            SpawnImpactFeedback(other.collider, other.contacts[0].point);
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

            HitPoint hitPoint = hitCollider.GetComponent<HitPoint>();
            if (hitPoint == null)
            {
                hitPoint = hitCollider.GetComponentInParent<HitPoint>();
            }

            if (hitPoint != null)
            {
                hitPoint.ApplyDamage(DamagePower);
            }

            ZedPrototypeZombieEnemy zedEnemy = hitCollider.GetComponentInParent<ZedPrototypeZombieEnemy>();
            if (zedEnemy != null)
            {
                zedEnemy.ApplyDamage(DamagePower);
            }
        }

        private void SpawnImpactFeedback(Collider hitCollider, Vector3 hitPoint)
        {
            GameObject impactPrefab = GetImpactPrefab(hitCollider);
            if (!impactPrefab)
            {
                return;
            }

            Instantiate(impactPrefab, hitPoint, impactPrefab.transform.rotation);
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
            return hitCollider.GetComponent<HitPoint>() != null || hitCollider.GetComponentInParent<HitPoint>() != null || hitCollider.GetComponentInParent<ZedPrototypeZombieEnemy>() != null;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(transform.position, DamageRadius);
        }
    }
}
using LegendOfZed.Enemies;
using TopDownShooter;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace LegendOfZed.Editor
{
    public static class ZedV13ZombieSetupCleanupMenu
    {
        [MenuItem("Legend of Zed/Setup/v1.3 Cleanup Zombie Scene Components")]
        public static void CleanupZombieSceneComponents()
        {
            ZedPrototypeZombieEnemy[] zombies = Object.FindObjectsByType<ZedPrototypeZombieEnemy>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

            int zombieCount = 0;
            int hitPointCount = 0;
            int navMeshAgentCount = 0;
            int relayCount = 0;

            for (int i = 0; i < zombies.Length; i++)
            {
                ZedPrototypeZombieEnemy zombie = zombies[i];
                if (zombie == null)
                {
                    continue;
                }

                zombieCount++;
                GameObject root = zombie.gameObject;

                hitPointCount += RemoveDemoHitPoints(root);
                navMeshAgentCount += RemoveUnneededNavMeshAgent(root);

                Animator animator = zombie.Animator != null ? zombie.Animator : root.GetComponentInChildren<Animator>(true);
                if (animator != null)
                {
                    zombie.Animator = animator;
                    animator.applyRootMotion = zombie.UseRootMotionLocomotion;

                    ZedZombieRootMotionRelay relay = animator.GetComponent<ZedZombieRootMotionRelay>();
                    if (relay == null)
                    {
                        relay = animator.gameObject.AddComponent<ZedZombieRootMotionRelay>();
                        relayCount++;
                    }

                    relay.Owner = zombie;
                    EditorUtility.SetDirty(animator);
                    EditorUtility.SetDirty(relay);
                }

                zombie.NavMeshAgent = root.GetComponent<NavMeshAgent>();
                zombie.RagdollOnDeath = true;
                zombie.DestroyOnDeath = false;

                if (zombie.DeathDisableDelay < 10f)
                {
                    zombie.DeathDisableDelay = 999f;
                }

                EditorUtility.SetDirty(zombie);
                EditorUtility.SetDirty(root);
            }

            Debug.Log(
                "v1.3 zombie cleanup complete. Zombies=" + zombieCount +
                " Removed demo HitPoint=" + hitPointCount +
                " Removed bad NavMeshAgent=" + navMeshAgentCount +
                " Added root motion relays=" + relayCount +
                ". No player/weapons/bullets/ammo/projectile IDs changed.");
        }

        [MenuItem("Legend of Zed/Setup/v1.3 Print Zombie Workflow")]
        public static void PrintZombieWorkflow()
        {
            Debug.Log(
                "Current zombie workflow:\\n" +
                "1. Use ZedPrototypeZombieEnemy for zombie health/death.\\n" +
                "2. Do not add TopDownShooter.HitPoint to zombies.\\n" +
                "3. Do not require NavMeshAgent until a valid NavMesh exists.\\n" +
                "4. Animator child needs ZedZombieRootMotionRelay.\\n" +
                "5. Ragdoll is built with v1.1 Rebuild Tuned Zombie Ragdoll.\\n" +
                "6. Spawning is handled by ZedZombieSpawner / v1.2 Add Zombie Spawner.\\n" +
                "7. Blood splat cleanup is handled by ZedAutoDestroyAfterSeconds added by Damage.cs.");
        }

        private static int RemoveDemoHitPoints(GameObject root)
        {
            int removed = 0;
            HitPoint[] hitPoints = root.GetComponentsInChildren<HitPoint>(true);
            for (int i = 0; i < hitPoints.Length; i++)
            {
                if (hitPoints[i] == null)
                {
                    continue;
                }

                Object.DestroyImmediate(hitPoints[i], true);
                removed++;
            }

            return removed;
        }

        private static int RemoveUnneededNavMeshAgent(GameObject root)
        {
            NavMeshAgent agent = root.GetComponent<NavMeshAgent>();
            if (agent == null)
            {
                return 0;
            }

            NavMeshHit hit;
            bool hasUsableNavMesh = NavMesh.SamplePosition(root.transform.position, out hit, 25f, NavMesh.AllAreas);
            if (hasUsableNavMesh && agent.isOnNavMesh)
            {
                return 0;
            }

            Object.DestroyImmediate(agent, true);
            return 1;
        }
    }
}

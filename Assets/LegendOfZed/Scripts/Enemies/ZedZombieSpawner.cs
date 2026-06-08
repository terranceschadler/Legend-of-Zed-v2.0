using System.Collections.Generic;
using LegendOfZed.Runtime;
using TopDownShooter;
using UnityEngine;

namespace LegendOfZed.Enemies
{
    [DisallowMultipleComponent]
    public class ZedZombieSpawner : MonoBehaviour
    {
        [Header("Prototype")]
        public GameObject ZombiePrototype;
        public Transform PlayerTarget;

        [Header("Spawn Points")]
        public List<Transform> SpawnPoints = new List<Transform>();
        public bool SpawnOnStart = true;
        public int MaxAliveZombies = 5;
        public bool HidePrototypeOnStart = true;

        [Header("Spawned Zombie Defaults")]
        public float ZombieHealth = 60f;
        public float DeathDisableDelay = 999f;
        public bool RagdollOnDeath = true;

        private readonly List<ZedPrototypeZombieEnemy> _aliveZombies = new List<ZedPrototypeZombieEnemy>();

        private void Start()
        {
            if (HidePrototypeOnStart && ZombiePrototype != null && IsSceneObject(ZombiePrototype))
            {
                ZombiePrototype.SetActive(false);
            }

            ResolvePlayerTarget();

            if (SpawnOnStart)
            {
                SpawnInitialWave();
            }
        }

        [ContextMenu("Spawn Initial Wave")]
        public void SpawnInitialWave()
        {
            CleanupDeadReferences();

            if (ZombiePrototype == null)
            {
                Debug.LogWarning("ZedZombieSpawner has no ZombiePrototype assigned.", this);
                return;
            }

            if (SpawnPoints == null || SpawnPoints.Count == 0)
            {
                Debug.LogWarning("ZedZombieSpawner has no spawn points.", this);
                return;
            }

            int spawnCount = Mathf.Min(MaxAliveZombies, SpawnPoints.Count);
            for (int i = 0; i < spawnCount; i++)
            {
                SpawnZombieAt(SpawnPoints[i]);
            }
        }

        [ContextMenu("Clear Spawned Zombies")]
        public void ClearSpawnedZombies()
        {
            for (int i = _aliveZombies.Count - 1; i >= 0; i--)
            {
                if (_aliveZombies[i] != null)
                {
                    DestroySafely(_aliveZombies[i].gameObject);
                }
            }

            _aliveZombies.Clear();
        }

        public ZedPrototypeZombieEnemy SpawnZombieAt(Transform spawnPoint)
        {
            if (ZombiePrototype == null || spawnPoint == null)
            {
                return null;
            }

            CleanupDeadReferences();

            if (_aliveZombies.Count >= MaxAliveZombies)
            {
                return null;
            }

            ResolvePlayerTarget();

            GameObject zombieObject = Instantiate(ZombiePrototype, spawnPoint.position, spawnPoint.rotation);
            zombieObject.name = "Zed_Spawned_Zombie";
            zombieObject.SetActive(true);

            ZedPrototypeZombieEnemy zombie = RebindSpawnedZombie(zombieObject);
            _aliveZombies.Add(zombie);
            return zombie;
        }

        private ZedPrototypeZombieEnemy RebindSpawnedZombie(GameObject zombieObject)
        {
            RemoveDemoHitPoints(zombieObject);
            RemoveNavMeshAgentIfPresent(zombieObject);

            ZedPrototypeZombieEnemy zombie = zombieObject.GetComponent<ZedPrototypeZombieEnemy>();
            if (zombie == null)
            {
                zombie = zombieObject.AddComponent<ZedPrototypeZombieEnemy>();
            }

            Animator animator = zombieObject.GetComponentInChildren<Animator>(true);
            zombie.Animator = animator;
            zombie.Target = PlayerTarget;
            zombie.NavMeshAgent = null;

            zombie.MaxHealth = ZombieHealth;
            zombie.CurrentHealth = ZombieHealth;
            zombie.DestroyOnDeath = false;
            zombie.DeathDisableDelay = DeathDisableDelay;
            zombie.RagdollOnDeath = RagdollOnDeath;

            zombie.StaggerOnBulletHit = true;
            zombie.HitStaggerSeconds = Mathf.Max(zombie.HitStaggerSeconds, 0.45f);
            zombie.RequireTargetInRangeAtHitMoment = true;

            if (animator != null)
            {
                animator.applyRootMotion = zombie.UseRootMotionLocomotion;

                ZedZombieRootMotionRelay relay = animator.GetComponent<ZedZombieRootMotionRelay>();
                if (relay == null)
                {
                    relay = animator.gameObject.AddComponent<ZedZombieRootMotionRelay>();
                }

                relay.Owner = zombie;
            }

            ZedZombieHitReactionMotor hitReaction = zombieObject.GetComponent<ZedZombieHitReactionMotor>();
            if (hitReaction == null)
            {
                hitReaction = zombieObject.AddComponent<ZedZombieHitReactionMotor>();
            }

            ZedAudioFeedback audioFeedback = zombieObject.GetComponent<ZedAudioFeedback>();
            if (audioFeedback == null)
            {
                audioFeedback = zombieObject.AddComponent<ZedAudioFeedback>();
            }

            ZedZombieAudioBridge audioBridge = zombieObject.GetComponent<ZedZombieAudioBridge>();
            if (audioBridge == null)
            {
                audioBridge = zombieObject.AddComponent<ZedZombieAudioBridge>();
            }

            audioBridge.AudioFeedback = audioFeedback;

            return zombie;
        }

        private void ResolvePlayerTarget()
        {
            if (PlayerTarget != null)
            {
                return;
            }

            PlayerController player = FindFirstObjectByType<PlayerController>();
            if (player != null)
            {
                PlayerTarget = player.transform;
            }
        }

        private void CleanupDeadReferences()
        {
            for (int i = _aliveZombies.Count - 1; i >= 0; i--)
            {
                if (_aliveZombies[i] == null || !_aliveZombies[i].gameObject.activeInHierarchy)
                {
                    _aliveZombies.RemoveAt(i);
                }
            }
        }

        private static void RemoveDemoHitPoints(GameObject root)
        {
            HitPoint[] hitPoints = root.GetComponentsInChildren<HitPoint>(true);
            for (int i = 0; i < hitPoints.Length; i++)
            {
                DestroyComponentSafely(hitPoints[i]);
            }
        }

        private static void RemoveNavMeshAgentIfPresent(GameObject root)
        {
            UnityEngine.AI.NavMeshAgent agent = root.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null)
            {
                DestroyComponentSafely(agent);
            }
        }

        private static bool IsSceneObject(GameObject target)
        {
            return target != null && target.scene.IsValid();
        }

        private static void DestroyComponentSafely(Component component)
        {
            if (component == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(component);
            }
            else
            {
                DestroyImmediate(component);
            }
        }

        private static void DestroySafely(GameObject target)
        {
            if (target == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(target);
            }
            else
            {
                DestroyImmediate(target);
            }
        }
    }
}

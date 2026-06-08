using System.Collections.Generic;
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
            if (HidePrototypeOnStart && ZombiePrototype != null)
            {
                ZombiePrototype.SetActive(false);
            }

            if (PlayerTarget == null)
            {
                PlayerController player = FindFirstObjectByType<PlayerController>();
                if (player != null)
                {
                    PlayerTarget = player.transform;
                }
            }

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
                    DestroyImmediate(_aliveZombies[i].gameObject);
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

            GameObject zombieObject = Instantiate(ZombiePrototype, spawnPoint.position, spawnPoint.rotation);
            zombieObject.name = "Zed_Spawned_Zombie";
            zombieObject.SetActive(true);

            ZedPrototypeZombieEnemy zombie = zombieObject.GetComponent<ZedPrototypeZombieEnemy>();
            if (zombie == null)
            {
                zombie = zombieObject.AddComponent<ZedPrototypeZombieEnemy>();
            }

            zombie.Target = PlayerTarget;
            zombie.MaxHealth = ZombieHealth;
            zombie.CurrentHealth = ZombieHealth;
            zombie.DestroyOnDeath = false;
            zombie.DeathDisableDelay = DeathDisableDelay;
            zombie.RagdollOnDeath = RagdollOnDeath;

            RemoveDemoHitPoints(zombieObject);
            RemoveNavMeshAgentIfPresent(zombieObject);

            _aliveZombies.Add(zombie);
            return zombie;
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
                DestroyImmediate(hitPoints[i]);
            }
        }

        private static void RemoveNavMeshAgentIfPresent(GameObject root)
        {
            UnityEngine.AI.NavMeshAgent agent = root.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null)
            {
                DestroyImmediate(agent);
            }
        }
    }
}

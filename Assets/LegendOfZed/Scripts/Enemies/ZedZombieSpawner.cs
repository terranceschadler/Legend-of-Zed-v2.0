using System.Collections;
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
        public bool HidePrototypeOnStart = true;

        [Header("Wave Control")]
        public bool SpawnOnStart = true;
        public bool LoopWaves = false;
        public int ZombiesPerWave = 5;
        public int MaxAliveZombies = 5;
        public float InitialSpawnDelay = 0.25f;
        public float DelayBetweenSpawns = 0.45f;
        public float DelayBetweenWaves = 4f;

        [Header("Spawned Zombie Defaults")]
        public float ZombieHealth = 60f;
        public float DeathDisableDelay = 999f;
        public bool RagdollOnDeath = true;

        [Header("Debug")]
        public bool LogWaveEvents = false;

        private readonly List<ZedPrototypeZombieEnemy> _aliveZombies = new List<ZedPrototypeZombieEnemy>();
        private Coroutine _waveRoutine;
        private int _waveNumber;
        private int _nextSpawnPointIndex;

        public int AliveCount
        {
            get
            {
                CleanupDeadReferences();
                return _aliveZombies.Count;
            }
        }

        public bool IsSpawning => _waveRoutine != null;

        private void Start()
        {
            if (HidePrototypeOnStart && ZombiePrototype != null && IsSceneObject(ZombiePrototype))
            {
                ZombiePrototype.SetActive(false);
            }

            ResolvePlayerTarget();

            if (SpawnOnStart)
            {
                StartWaves();
            }
        }

        [ContextMenu("Start Waves")]
        public void StartWaves()
        {
            if (_waveRoutine != null)
            {
                return;
            }

            _waveRoutine = StartCoroutine(WaveRoutine());
        }

        [ContextMenu("Stop Waves")]
        public void StopWaves()
        {
            if (_waveRoutine != null)
            {
                StopCoroutine(_waveRoutine);
                _waveRoutine = null;
            }
        }

        [ContextMenu("Spawn One Wave Now")]
        public void SpawnOneWaveNow()
        {
            if (_waveRoutine != null)
            {
                return;
            }

            _waveRoutine = StartCoroutine(SpawnSingleWaveRoutine());
        }

        [ContextMenu("Clear Spawned Zombies")]
        public void ClearSpawnedZombies()
        {
            StopWaves();

            for (int i = _aliveZombies.Count - 1; i >= 0; i--)
            {
                if (_aliveZombies[i] != null)
                {
                    DestroySafely(_aliveZombies[i].gameObject);
                }
            }

            _aliveZombies.Clear();
        }

        private IEnumerator WaveRoutine()
        {
            if (InitialSpawnDelay > 0f)
            {
                yield return new WaitForSeconds(InitialSpawnDelay);
            }

            do
            {
                yield return SpawnWaveRoutine();

                if (!LoopWaves)
                {
                    break;
                }

                while (AliveCount > 0)
                {
                    yield return null;
                }

                if (DelayBetweenWaves > 0f)
                {
                    yield return new WaitForSeconds(DelayBetweenWaves);
                }
            }
            while (LoopWaves);

            _waveRoutine = null;
        }

        private IEnumerator SpawnSingleWaveRoutine()
        {
            yield return SpawnWaveRoutine();
            _waveRoutine = null;
        }

        private IEnumerator SpawnWaveRoutine()
        {
            if (!CanSpawn())
            {
                _waveRoutine = null;
                yield break;
            }

            _waveNumber++;

            if (LogWaveEvents)
            {
                Debug.Log("Starting zombie wave " + _waveNumber + " with " + ZombiesPerWave + " requested zombie(s).", this);
            }

            int spawnedThisWave = 0;
            int attempts = 0;
            int maxAttempts = Mathf.Max(ZombiesPerWave * 4, SpawnPoints.Count * 2);

            while (spawnedThisWave < ZombiesPerWave && attempts < maxAttempts)
            {
                attempts++;
                CleanupDeadReferences();

                if (_aliveZombies.Count >= MaxAliveZombies)
                {
                    yield return null;
                    continue;
                }

                Transform spawnPoint = GetNextSpawnPoint();
                if (spawnPoint == null)
                {
                    yield break;
                }

                ZedPrototypeZombieEnemy spawned = SpawnZombieAt(spawnPoint);
                if (spawned != null)
                {
                    spawnedThisWave++;
                }

                if (DelayBetweenSpawns > 0f)
                {
                    yield return new WaitForSeconds(DelayBetweenSpawns);
                }
                else
                {
                    yield return null;
                }
            }

            if (LogWaveEvents)
            {
                Debug.Log("Finished zombie wave " + _waveNumber + ". Spawned=" + spawnedThisWave + ".", this);
            }
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

        private bool CanSpawn()
        {
            if (ZombiePrototype == null)
            {
                Debug.LogWarning("ZedZombieSpawner has no ZombiePrototype assigned.", this);
                return false;
            }

            if (SpawnPoints == null || SpawnPoints.Count == 0)
            {
                Debug.LogWarning("ZedZombieSpawner has no spawn points.", this);
                return false;
            }

            return true;
        }

        private Transform GetNextSpawnPoint()
        {
            if (SpawnPoints == null || SpawnPoints.Count == 0)
            {
                return null;
            }

            int checkedCount = 0;
            while (checkedCount < SpawnPoints.Count)
            {
                int index = _nextSpawnPointIndex % SpawnPoints.Count;
                _nextSpawnPointIndex++;
                checkedCount++;

                if (SpawnPoints[index] != null)
                {
                    return SpawnPoints[index];
                }
            }

            return null;
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

            PlayerController player = FindAnyObjectByType<PlayerController>();
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

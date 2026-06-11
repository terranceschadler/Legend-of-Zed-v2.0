using System.Collections;
using System.Collections.Generic;
using LegendOfZed.LegacyMapGenerator;
using LegendOfZed.Overworld;
using LegendOfZed.Runtime;
using TopDownShooter;
using UnityEngine;
using UnityEngine.AI;

namespace LegendOfZed.Enemies
{
    /// <summary>
    /// Runtime director for spawning existing Legend of Zed zombies outside the active game camera view.
    /// Uses the current generated map, player, camera, NavMesh, and ZedPrototypeZombieEnemy prefab.
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(250)]
    public sealed class ZedOffscreenEnemySpawnDirector : MonoBehaviour
    {
        [Header("References")]
        public ZedLegacyRandomMapGenerator MapGenerator;
        public Transform Player;
        public Camera GameplayCamera;
        public GameObject ZombiePrefab;

        [Header("Startup")]
        public bool SpawnAfterMapGeneration = true;
        public bool AutoStart = true;
        public float InitialGraceSeconds = 5f;
        public float StartTileSafeRadius = 14f;
        public bool PreferRuntimeNavMeshBuilderReady = true;
        public float NavMeshWarningDelaySeconds = 12f;

        [Header("Spawn Distance")]
        public float MinPlayerDistance = 22f;
        public float MaxPlayerDistance = 65f;
        public float CameraViewportPadding = 0.12f;
        public float SpawnGroundOffset = 0.02f;

        [Header("Trickle Spawn")]
        public int MinGroupSize = 1;
        public int MaxGroupSize = 3;
        public float MinSpawnInterval = 3f;
        public float MaxSpawnInterval = 6f;
        public int MaxAliveZombies = 10;
        public int MaxSpawnedPerMinute = 24;

        [Header("Candidate Validation")]
        public int CandidateAttemptsPerZombie = 36;
        public float NavMeshSampleRadius = 8f;
        public int NavMeshAreaMask = NavMesh.AllAreas;
        public bool RequireCompletePathToPlayer = true;
        public LayerMask GroundRayMask = ~0;
        public float GroundRayHeight = 18f;

        [Header("Despawn / Pooling")]
        public bool UsePooling = true;
        public int PrewarmPoolCount = 0;
        public bool DespawnFarInvisibleZombies = true;
        public float FarDespawnDistance = 90f;
        public float DeadBodyPoolReturnDelay = 18f;

        [Header("Spawned Zombie Defaults")]
        public float ZombieHealth = 60f;
        public bool RagdollOnDeath = true;
        public bool DestroyOnDeath = false;

        [Header("Debug")]
        public bool VerboseLogs = false;
        public bool DrawDebugGizmos = false;

        private readonly List<ZedPrototypeZombieEnemy> _trackedZombies = new List<ZedPrototypeZombieEnemy>();
        private readonly Queue<ZedPrototypeZombieEnemy> _pool = new Queue<ZedPrototypeZombieEnemy>();
        private readonly Queue<float> _recentSpawnTimes = new Queue<float>();
        private readonly List<Vector3> _recentRejectedCandidates = new List<Vector3>();
        private readonly List<Vector3> _recentAcceptedCandidates = new List<Vector3>();

        private Coroutine _spawnRoutine;
        private Vector3 _startPosition;
        private bool _hasStartPosition;
        private bool _warnedMissingPrefab;
        private bool _warnedMissingPlayer;
        private bool _warnedMissingCamera;
        private bool _warnedMissingNavMesh;
        private float _navMeshWaitStartedTime = -1f;

        public int AliveCount
        {
            get
            {
                CleanupTrackedZombies();
                int count = 0;
                for (int i = 0; i < _trackedZombies.Count; i++)
                {
                    ZedPrototypeZombieEnemy zombie = _trackedZombies[i];
                    if (zombie != null && zombie.gameObject.activeInHierarchy && !zombie.IsDead)
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        public int PooledCount => _pool.Count;
        public bool IsSpawning => _spawnRoutine != null;

        private void Awake()
        {
            ResolveReferences();
            PrewarmPool();
        }

        private void OnEnable()
        {
            if (AutoStart)
            {
                StartSpawning();
            }
        }

        private void OnDisable()
        {
            StopSpawning();
        }

        private void Update()
        {
            CleanupTrackedZombies();

            if (DespawnFarInvisibleZombies)
            {
                DespawnFarInvisibleTrackedZombies();
            }
        }

        [ContextMenu("Start Offscreen Spawning")]
        public void StartSpawning()
        {
            if (_spawnRoutine != null)
            {
                return;
            }

            _spawnRoutine = StartCoroutine(SpawnRoutine());
        }

        [ContextMenu("Stop Offscreen Spawning")]
        public void StopSpawning()
        {
            if (_spawnRoutine == null)
            {
                return;
            }

            StopCoroutine(_spawnRoutine);
            _spawnRoutine = null;
        }

        [ContextMenu("Spawn Test Group Now")]
        public void SpawnTestGroupNow()
        {
            ResolveReferences();
            TrySpawnGroup(Random.Range(Mathf.Max(1, MinGroupSize), Mathf.Max(MinGroupSize, MaxGroupSize) + 1));
        }

        [ContextMenu("Clear Spawned Zombies")]
        public void ClearSpawnedZombies()
        {
            for (int i = _trackedZombies.Count - 1; i >= 0; i--)
            {
                ZedPrototypeZombieEnemy zombie = _trackedZombies[i];
                if (zombie == null)
                {
                    _trackedZombies.RemoveAt(i);
                    continue;
                }

                if (Application.isPlaying)
                {
                    zombie.gameObject.SetActive(false);
                    if (UsePooling)
                    {
                        _pool.Enqueue(zombie);
                    }
                    else
                    {
                        Destroy(zombie.gameObject);
                    }
                }
                else
                {
                    DestroyImmediate(zombie.gameObject);
                }

                _trackedZombies.RemoveAt(i);
            }
        }

        private IEnumerator SpawnRoutine()
        {
            ResolveReferences();

            if (SpawnAfterMapGeneration && MapGenerator != null)
            {
                while (!MapGenerator.bakingNavMeshCompleted)
                {
                    yield return null;
                }
            }

            float grace = Mathf.Max(0f, InitialGraceSeconds);
            if (grace > 0f)
            {
                yield return new WaitForSeconds(grace);
            }

            ResolveReferences();
            CaptureStartPosition();

            while (enabled)
            {
                ResolveReferences();

                if (CanAttemptSpawning())
                {
                    int groupSize = Random.Range(Mathf.Max(1, MinGroupSize), Mathf.Max(MinGroupSize, MaxGroupSize) + 1);
                    TrySpawnGroup(groupSize);
                }

                float wait = Random.Range(Mathf.Max(0.1f, MinSpawnInterval), Mathf.Max(MinSpawnInterval, MaxSpawnInterval));
                yield return new WaitForSeconds(wait);
            }

            _spawnRoutine = null;
        }

        private void ResolveReferences()
        {
            if (MapGenerator == null)
            {
                MapGenerator = FindAnyObjectByType<ZedLegacyRandomMapGenerator>();
            }

            if (Player == null)
            {
                PlayerController playerController = FindAnyObjectByType<PlayerController>();
                if (playerController != null)
                {
                    Player = playerController.transform;
                }
            }

            if (Player == null)
            {
                GameObject taggedPlayer = FindGameObjectWithTagSafe("Player");
                if (taggedPlayer != null)
                {
                    Player = taggedPlayer.transform;
                }
            }

            if (GameplayCamera == null)
            {
                GameplayCamera = Camera.main;
            }
        }

        private void CaptureStartPosition()
        {
            if (_hasStartPosition || Player == null)
            {
                return;
            }

            _startPosition = Player.position;
            _hasStartPosition = true;
        }

        private void PrewarmPool()
        {
            if (!Application.isPlaying || !UsePooling || ZombiePrefab == null)
            {
                return;
            }

            int count = Mathf.Max(0, PrewarmPoolCount);
            for (int i = 0; i < count; i++)
            {
                ZedPrototypeZombieEnemy zombie = CreatePooledZombie();
                if (zombie != null)
                {
                    _pool.Enqueue(zombie);
                }
            }
        }

        private bool CanAttemptSpawning()
        {
            if (ZombiePrefab == null)
            {
                WarnOnce(ref _warnedMissingPrefab, "Offscreen enemy spawn director has no ZombiePrefab assigned.");
                return false;
            }

            if (Player == null)
            {
                WarnOnce(ref _warnedMissingPlayer, "Offscreen enemy spawn director cannot find the player yet.");
                return false;
            }

            if (GameplayCamera == null)
            {
                WarnOnce(ref _warnedMissingCamera, "Offscreen enemy spawn director cannot find the gameplay camera yet.");
                return false;
            }

            if (!HasAnyNavMeshNearPlayer())
            {
                WarnMissingNavMeshAfterDelay();
                return false;
            }

            _navMeshWaitStartedTime = -1f;

            if (AliveCount >= Mathf.Max(0, MaxAliveZombies))
            {
                return false;
            }

            TrimRecentSpawnTimes();
            return _recentSpawnTimes.Count < Mathf.Max(1, MaxSpawnedPerMinute);
        }

        private bool TrySpawnGroup(int requestedCount)
        {
            int spawned = 0;
            int maxAllowedNow = Mathf.Max(0, MaxAliveZombies) - AliveCount;
            int maxByMinute = Mathf.Max(1, MaxSpawnedPerMinute) - CountRecentSpawns();
            int targetCount = Mathf.Min(Mathf.Max(0, requestedCount), maxAllowedNow, maxByMinute);

            for (int i = 0; i < targetCount; i++)
            {
                if (TryFindSpawnPosition(out Vector3 position))
                {
                    ZedPrototypeZombieEnemy zombie = SpawnZombie(position);
                    if (zombie != null)
                    {
                        spawned++;
                        _recentSpawnTimes.Enqueue(Time.time);
                    }
                }
            }

            if (VerboseLogs && spawned > 0)
            {
                Debug.Log("Offscreen zombie spawn group complete. Spawned=" + spawned + ", Alive=" + AliveCount + ", Pool=" + _pool.Count + ".", this);
            }

            return spawned > 0;
        }

        private bool TryFindSpawnPosition(out Vector3 spawnPosition)
        {
            spawnPosition = Vector3.zero;
            _recentAcceptedCandidates.Clear();
            _recentRejectedCandidates.Clear();

            if (Player == null || GameplayCamera == null)
            {
                return false;
            }

            int attempts = Mathf.Max(1, CandidateAttemptsPerZombie);
            float minDistance = Mathf.Max(0f, MinPlayerDistance);
            float maxDistance = Mathf.Max(minDistance + 1f, MaxPlayerDistance);

            for (int i = 0; i < attempts; i++)
            {
                float angle = Random.Range(0f, Mathf.PI * 2f);
                float distance = Random.Range(minDistance, maxDistance);
                Vector3 candidate = Player.position + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * distance;

                if (!TryProjectCandidateToNavMesh(candidate, out Vector3 navPosition))
                {
                    RememberRejected(candidate);
                    continue;
                }

                navPosition = ProjectToGround(navPosition);

                if (!IsOutsideCameraView(navPosition))
                {
                    RememberRejected(navPosition);
                    continue;
                }

                if (!IsFarEnoughFromStart(navPosition))
                {
                    RememberRejected(navPosition);
                    continue;
                }

                if (RequireCompletePathToPlayer && !HasCompletePathToPlayer(navPosition))
                {
                    RememberRejected(navPosition);
                    continue;
                }

                spawnPosition = navPosition;
                _recentAcceptedCandidates.Add(spawnPosition);
                return true;
            }

            return false;
        }

        private bool TryProjectCandidateToNavMesh(Vector3 candidate, out Vector3 navPosition)
        {
            navPosition = candidate;
            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, Mathf.Max(0.1f, NavMeshSampleRadius), NavMeshAreaMask))
            {
                navPosition = hit.position;
                return true;
            }

            return false;
        }

        private Vector3 ProjectToGround(Vector3 origin)
        {
            Vector3 rayStart = origin + Vector3.up * Mathf.Max(0.1f, GroundRayHeight);
            float rayDistance = Mathf.Max(0.1f, GroundRayHeight) * 2f;

            if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, rayDistance, GroundRayMask, QueryTriggerInteraction.Ignore))
            {
                return hit.point + Vector3.up * SpawnGroundOffset;
            }

            origin.y += SpawnGroundOffset;
            return origin;
        }

        private bool IsOutsideCameraView(Vector3 worldPosition)
        {
            if (GameplayCamera == null)
            {
                return false;
            }

            Vector3 viewport = GameplayCamera.WorldToViewportPoint(worldPosition);
            float padding = Mathf.Max(0f, CameraViewportPadding);

            if (viewport.z <= 0f)
            {
                return true;
            }

            return viewport.x < -padding ||
                   viewport.x > 1f + padding ||
                   viewport.y < -padding ||
                   viewport.y > 1f + padding;
        }

        private bool IsFarEnoughFromStart(Vector3 worldPosition)
        {
            if (!_hasStartPosition || StartTileSafeRadius <= 0f)
            {
                return true;
            }

            Vector3 delta = worldPosition - _startPosition;
            delta.y = 0f;
            return delta.sqrMagnitude >= StartTileSafeRadius * StartTileSafeRadius;
        }

        private bool HasCompletePathToPlayer(Vector3 spawnPosition)
        {
            if (Player == null)
            {
                return false;
            }

            NavMeshPath path = new NavMeshPath();
            if (!NavMesh.CalculatePath(spawnPosition, Player.position, NavMeshAreaMask, path))
            {
                return false;
            }

            return path.status == NavMeshPathStatus.PathComplete;
        }

        private bool HasAnyNavMeshNearPlayer()
        {
            if (Player == null)
            {
                return false;
            }

            if (PreferRuntimeNavMeshBuilderReady)
            {
                ZedOverworldRuntimeNavMeshBuilder builder = FindAnyObjectByType<ZedOverworldRuntimeNavMeshBuilder>();
                if (builder != null && !builder.HasBuiltValidNavMesh && !ZedOverworldRuntimeNavMeshBuilder.HasSuccessfulRuntimeBuild)
                {
                    return false;
                }
            }

            return NavMesh.SamplePosition(Player.position, out _, Mathf.Max(3f, NavMeshSampleRadius), NavMeshAreaMask);
        }

        private void WarnMissingNavMeshAfterDelay()
        {
            if (_warnedMissingNavMesh)
            {
                return;
            }

            if (_navMeshWaitStartedTime < 0f)
            {
                _navMeshWaitStartedTime = Time.time;
                return;
            }

            if (Time.time - _navMeshWaitStartedTime < Mathf.Max(0f, NavMeshWarningDelaySeconds))
            {
                return;
            }

            WarnOnce(ref _warnedMissingNavMesh, "Offscreen enemy spawn director is waiting for the post-generation runtime NavMesh build near the player.");
        }

        private ZedPrototypeZombieEnemy SpawnZombie(Vector3 position)
        {
            ZedPrototypeZombieEnemy zombie = UsePooling ? GetFromPoolOrCreate() : CreatePooledZombie();
            if (zombie == null)
            {
                return null;
            }

            ConfigureZombieForSpawn(zombie, position);
            if (!_trackedZombies.Contains(zombie))
            {
                _trackedZombies.Add(zombie);
            }

            return zombie;
        }

        private ZedPrototypeZombieEnemy GetFromPoolOrCreate()
        {
            while (_pool.Count > 0)
            {
                ZedPrototypeZombieEnemy pooled = _pool.Dequeue();
                if (pooled != null)
                {
                    return pooled;
                }
            }

            return CreatePooledZombie();
        }

        private ZedPrototypeZombieEnemy CreatePooledZombie()
        {
            if (ZombiePrefab == null)
            {
                return null;
            }

            GameObject zombieObject = Instantiate(ZombiePrefab);
            zombieObject.name = "Zed_Offscreen_Spawned_Zombie";
            zombieObject.SetActive(false);

            ZedPrototypeZombieEnemy zombie = RebindZombieObject(zombieObject);
            return zombie;
        }

        private void ConfigureZombieForSpawn(ZedPrototypeZombieEnemy zombie, Vector3 position)
        {
            if (zombie == null)
            {
                return;
            }

            GameObject zombieObject = zombie.gameObject;
            zombieObject.transform.position = position;
            zombieObject.transform.rotation = Quaternion.identity;

            zombie.Target = Player;
            zombie.MaxHealth = ZombieHealth;
            zombie.CurrentHealth = ZombieHealth;
            zombie.DestroyOnDeath = DestroyOnDeath;
            zombie.DeathDisableDelay = Mathf.Max(0.1f, DeadBodyPoolReturnDelay);
            zombie.RagdollOnDeath = RagdollOnDeath;
            zombie.StaggerOnBulletHit = true;
            zombie.RequireTargetInRangeAtHitMoment = true;

            Animator animator = zombie.Animator != null ? zombie.Animator : zombieObject.GetComponentInChildren<Animator>(true);
            zombie.Animator = animator;
            if (animator != null)
            {
                animator.applyRootMotion = zombie.UseRootMotionLocomotion;
                animator.Rebind();
                animator.Update(0f);

                ZedZombieRootMotionRelay relay = animator.GetComponent<ZedZombieRootMotionRelay>();
                if (relay == null)
                {
                    relay = animator.gameObject.AddComponent<ZedZombieRootMotionRelay>();
                }

                relay.Owner = zombie;
            }

            zombieObject.SetActive(true);
        }

        private ZedPrototypeZombieEnemy RebindZombieObject(GameObject zombieObject)
        {
            RemoveDemoHitPoints(zombieObject);

            ZedPrototypeZombieEnemy zombie = zombieObject.GetComponent<ZedPrototypeZombieEnemy>();
            if (zombie == null)
            {
                zombie = zombieObject.AddComponent<ZedPrototypeZombieEnemy>();
            }

            zombie.Target = Player;
            zombie.Animator = zombieObject.GetComponentInChildren<Animator>(true);

            if (zombie.Animator != null)
            {
                ZedZombieRootMotionRelay relay = zombie.Animator.GetComponent<ZedZombieRootMotionRelay>();
                if (relay == null)
                {
                    relay = zombie.Animator.gameObject.AddComponent<ZedZombieRootMotionRelay>();
                }

                relay.Owner = zombie;
            }

            if (zombieObject.GetComponent<ZedZombieHitReactionMotor>() == null)
            {
                zombieObject.AddComponent<ZedZombieHitReactionMotor>();
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

        private void CleanupTrackedZombies()
        {
            for (int i = _trackedZombies.Count - 1; i >= 0; i--)
            {
                ZedPrototypeZombieEnemy zombie = _trackedZombies[i];
                if (zombie == null)
                {
                    _trackedZombies.RemoveAt(i);
                    continue;
                }

                if (!zombie.gameObject.activeInHierarchy)
                {
                    _trackedZombies.RemoveAt(i);
                    if (UsePooling)
                    {
                        _pool.Enqueue(zombie);
                    }
                }
            }
        }

        private void DespawnFarInvisibleTrackedZombies()
        {
            if (Player == null || GameplayCamera == null)
            {
                return;
            }

            float farSqr = Mathf.Max(1f, FarDespawnDistance) * Mathf.Max(1f, FarDespawnDistance);
            for (int i = _trackedZombies.Count - 1; i >= 0; i--)
            {
                ZedPrototypeZombieEnemy zombie = _trackedZombies[i];
                if (zombie == null || !zombie.gameObject.activeInHierarchy || zombie.IsDead)
                {
                    continue;
                }

                Vector3 delta = zombie.transform.position - Player.position;
                delta.y = 0f;
                if (delta.sqrMagnitude < farSqr)
                {
                    continue;
                }

                if (!IsOutsideCameraView(zombie.transform.position))
                {
                    continue;
                }

                zombie.gameObject.SetActive(false);
            }
        }

        private int CountRecentSpawns()
        {
            TrimRecentSpawnTimes();
            return _recentSpawnTimes.Count;
        }

        private void TrimRecentSpawnTimes()
        {
            float cutoff = Time.time - 60f;
            while (_recentSpawnTimes.Count > 0 && _recentSpawnTimes.Peek() < cutoff)
            {
                _recentSpawnTimes.Dequeue();
            }
        }

        private void RememberRejected(Vector3 position)
        {
            if (!DrawDebugGizmos)
            {
                return;
            }

            if (_recentRejectedCandidates.Count > 24)
            {
                _recentRejectedCandidates.RemoveAt(0);
            }

            _recentRejectedCandidates.Add(position);
        }

        private void WarnOnce(ref bool flag, string message)
        {
            if (flag)
            {
                return;
            }

            flag = true;
            Debug.LogWarning(message, this);
        }

        private static void RemoveDemoHitPoints(GameObject root)
        {
            if (root == null)
            {
                return;
            }

            HitPoint[] hitPoints = root.GetComponentsInChildren<HitPoint>(true);
            for (int i = 0; i < hitPoints.Length; i++)
            {
                Component component = hitPoints[i];
                if (component == null)
                {
                    continue;
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

        private void OnDrawGizmosSelected()
        {
            if (!DrawDebugGizmos)
            {
                return;
            }

            if (Player != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(Player.position, MinPlayerDistance);
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(Player.position, MaxPlayerDistance);
            }

            if (_hasStartPosition)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(_startPosition, StartTileSafeRadius);
            }

            Gizmos.color = Color.red;
            for (int i = 0; i < _recentRejectedCandidates.Count; i++)
            {
                Gizmos.DrawSphere(_recentRejectedCandidates[i], 0.35f);
            }

            Gizmos.color = Color.magenta;
            for (int i = 0; i < _recentAcceptedCandidates.Count; i++)
            {
                Gizmos.DrawSphere(_recentAcceptedCandidates[i], 0.55f);
            }
        }
    }
}

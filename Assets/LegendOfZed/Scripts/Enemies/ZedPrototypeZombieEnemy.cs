using TopDownShooter;
using UnityEngine;
using UnityEngine.AI;

namespace LegendOfZed.Enemies
{
    [DisallowMultipleComponent]
    public class ZedPrototypeZombieEnemy : MonoBehaviour
    {
        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int MovingHash = Animator.StringToHash("IsMoving");
        private static readonly int AttackingHash = Animator.StringToHash("IsAttacking");
        private static readonly int AttackIndexHash = Animator.StringToHash("AttackIndex");
        private static readonly int AttackBiteHash = Animator.StringToHash("AttackBite");
        private static readonly int AttackLeftHash = Animator.StringToHash("AttackLeft");
        private static readonly int AttackRightHash = Animator.StringToHash("AttackRight");
        private static readonly int AttackRight2Hash = Animator.StringToHash("AttackRight2");
        private static readonly int AttackTwoHandHash = Animator.StringToHash("AttackTwoHand");

        [Header("References")]
        public Transform Target;
        public NavMeshAgent NavMeshAgent;
        public Animator Animator;

        [Header("Detection")]
        public float DetectionRange = 11f;
        public float LoseTargetRange = 15f;
        public float AttackRange = 1.35f;

        [Header("Movement")]
        public float WalkSpeed = 1.2f;
        public float ChaseSpeed = 2.15f;
        public float TurnSpeed = 540f;
        public float StoppingDistance = 1.05f;
        public float SeparationRadius = 0.85f;
        public float SeparationStrength = 1.25f;

        [Header("Wander")]
        public float WanderRadius = 6f;
        public float WanderPointTolerance = 0.45f;
        public float MinWanderWait = 0.75f;
        public float MaxWanderWait = 2.0f;
        public float WanderDestinationRefreshSeconds = 6f;

        [Header("Attack")]
        public float AttackDamage = 10f;
        public float AttackCooldown = 1.25f;
        public bool SendDamageMessages = true;
        public bool RandomizeAttackAnimations = true;

        private Vector3 _spawnPosition;
        private Vector3 _wanderDestination;
        private float _nextWanderPickTime;
        private float _wanderDestinationExpireTime;
        private float _nextAttackTime;
        private bool _hasWanderDestination;
        private bool _isChasing;

        private void Reset()
        {
            NavMeshAgent = GetComponent<NavMeshAgent>();
            Animator = GetComponentInChildren<Animator>();
        }

        private void Awake()
        {
            _spawnPosition = transform.position;

            if (NavMeshAgent == null)
            {
                NavMeshAgent = GetComponent<NavMeshAgent>();
            }

            if (Animator == null)
            {
                Animator = GetComponentInChildren<Animator>();
            }

            ConfigureAgentForPathOnly();
        }

        private void Start()
        {
            if (Target == null)
            {
                Target = FindPlayerTarget();
            }

            PickWanderDestination(true);
        }

        private void OnEnable()
        {
            ConfigureAgentForPathOnly();
        }

        private void Update()
        {
            if (Target == null)
            {
                Target = FindPlayerTarget();
            }

            float distanceToTarget = Target != null ? Vector3.Distance(transform.position, Target.position) : float.PositiveInfinity;
            UpdateTargetState(distanceToTarget);

            if (_isChasing && Target != null)
            {
                ChaseTarget(distanceToTarget);
            }
            else
            {
                Wander();
            }
        }

        private void ConfigureAgentForPathOnly()
        {
            if (NavMeshAgent == null)
            {
                return;
            }

            NavMeshAgent.updatePosition = false;
            NavMeshAgent.updateRotation = false;
            NavMeshAgent.speed = ChaseSpeed;
            NavMeshAgent.angularSpeed = TurnSpeed;
            NavMeshAgent.stoppingDistance = StoppingDistance;
            NavMeshAgent.autoBraking = true;
        }

        private Transform FindPlayerTarget()
        {
            PlayerController playerController = FindFirstObjectByType<PlayerController>();
            if (playerController != null)
            {
                return playerController.transform;
            }

            GameObject taggedPlayer = GameObject.FindGameObjectWithTag("Player");
            return taggedPlayer != null ? taggedPlayer.transform : null;
        }

        private void UpdateTargetState(float distanceToTarget)
        {
            if (Target == null)
            {
                _isChasing = false;
                return;
            }

            if (_isChasing)
            {
                if (distanceToTarget > LoseTargetRange)
                {
                    _isChasing = false;
                    PickWanderDestination(true);
                }
            }
            else if (distanceToTarget <= DetectionRange)
            {
                _isChasing = true;
                _hasWanderDestination = false;
            }
        }

        private void ChaseTarget(float distanceToTarget)
        {
            if (distanceToTarget <= AttackRange)
            {
                SetMovingAnimation(0f);
                FaceTowards(Target.position);
                TryAttack();
                return;
            }

            Vector3 desiredMove = GetPathMoveDirection(Target.position, ChaseSpeed);
            desiredMove += GetSeparationOffset();
            MoveManually(desiredMove, ChaseSpeed);
        }

        private void Wander()
        {
            if (!_hasWanderDestination || Time.time >= _wanderDestinationExpireTime || Vector3.Distance(transform.position, _wanderDestination) <= WanderPointTolerance)
            {
                if (Time.time >= _nextWanderPickTime)
                {
                    PickWanderDestination(false);
                }

                SetMovingAnimation(0f);
                return;
            }

            Vector3 desiredMove = GetPathMoveDirection(_wanderDestination, WalkSpeed);
            desiredMove += GetSeparationOffset() * 0.5f;
            MoveManually(desiredMove, WalkSpeed);
        }

        private void PickWanderDestination(bool immediate)
        {
            if (!immediate)
            {
                _nextWanderPickTime = Time.time + Random.Range(MinWanderWait, MaxWanderWait);
            }

            Vector2 randomCircle = Random.insideUnitCircle * WanderRadius;
            Vector3 candidate = _spawnPosition + new Vector3(randomCircle.x, 0f, randomCircle.y);

            NavMeshHit navHit;
            if (NavMesh.SamplePosition(candidate, out navHit, WanderRadius, NavMesh.AllAreas))
            {
                candidate = navHit.position;
            }

            _wanderDestination = candidate;
            _hasWanderDestination = true;
            _wanderDestinationExpireTime = Time.time + WanderDestinationRefreshSeconds;
            SetAgentDestination(candidate);
        }

        private Vector3 GetPathMoveDirection(Vector3 destination, float speed)
        {
            SetAgentDestination(destination);

            if (NavMeshAgent != null && NavMeshAgent.enabled && NavMeshAgent.isOnNavMesh)
            {
                NavMeshAgent.speed = speed;
                Vector3 desiredVelocity = NavMeshAgent.desiredVelocity;
                desiredVelocity.y = 0f;
                if (desiredVelocity.sqrMagnitude > 0.0001f)
                {
                    return desiredVelocity.normalized;
                }
            }

            Vector3 direct = destination - transform.position;
            direct.y = 0f;
            return direct.sqrMagnitude > 0.0001f ? direct.normalized : Vector3.zero;
        }

        private void SetAgentDestination(Vector3 destination)
        {
            if (NavMeshAgent == null || !NavMeshAgent.enabled || !NavMeshAgent.isOnNavMesh)
            {
                return;
            }

            NavMeshAgent.nextPosition = transform.position;
            NavMeshAgent.SetDestination(destination);
        }

        private Vector3 GetSeparationOffset()
        {
            Collider[] nearby = Physics.OverlapSphere(transform.position, SeparationRadius);
            Vector3 separation = Vector3.zero;

            for (int i = 0; i < nearby.Length; i++)
            {
                ZedPrototypeZombieEnemy otherZombie = nearby[i].GetComponentInParent<ZedPrototypeZombieEnemy>();
                if (otherZombie == null || otherZombie == this)
                {
                    continue;
                }

                Vector3 away = transform.position - otherZombie.transform.position;
                away.y = 0f;
                float sqrMagnitude = away.sqrMagnitude;
                if (sqrMagnitude > 0.0001f)
                {
                    separation += away.normalized / Mathf.Max(0.1f, sqrMagnitude);
                }
            }

            return separation * SeparationStrength;
        }

        private void MoveManually(Vector3 desiredDirection, float speed)
        {
            desiredDirection.y = 0f;

            if (desiredDirection.sqrMagnitude <= 0.0001f)
            {
                SetMovingAnimation(0f);
                return;
            }

            Vector3 direction = desiredDirection.normalized;
            transform.position += direction * speed * Time.deltaTime;
            FaceDirection(direction);
            SetMovingAnimation(speed);

            if (NavMeshAgent != null && NavMeshAgent.enabled && NavMeshAgent.isOnNavMesh)
            {
                NavMeshAgent.nextPosition = transform.position;
            }
        }

        private void FaceTowards(Vector3 worldPosition)
        {
            Vector3 direction = worldPosition - transform.position;
            direction.y = 0f;
            FaceDirection(direction);
        }

        private void FaceDirection(Vector3 direction)
        {
            direction.y = 0f;
            if (direction.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, TurnSpeed * Time.deltaTime);
        }

        private void TryAttack()
        {
            if (Time.time < _nextAttackTime)
            {
                return;
            }

            _nextAttackTime = Time.time + AttackCooldown;
            TriggerAttackAnimation();

            if (SendDamageMessages && Target != null)
            {
                Target.SendMessage("ApplyDamage", AttackDamage, SendMessageOptions.DontRequireReceiver);
                Target.SendMessage("TakeDamage", AttackDamage, SendMessageOptions.DontRequireReceiver);
                Target.SendMessage("Damage", AttackDamage, SendMessageOptions.DontRequireReceiver);
            }
        }

        private void SetMovingAnimation(float speed)
        {
            if (Animator == null)
            {
                return;
            }

            Animator.SetFloat(SpeedHash, speed);
            Animator.SetBool(MovingHash, speed > 0.01f);
            Animator.SetBool(AttackingHash, false);
        }

        private void TriggerAttackAnimation()
        {
            if (Animator == null)
            {
                return;
            }

            int attackIndex = RandomizeAttackAnimations ? Random.Range(0, 5) : 0;
            Animator.SetInteger(AttackIndexHash, attackIndex);
            Animator.SetBool(AttackingHash, true);

            switch (attackIndex)
            {
                case 0:
                    Animator.SetTrigger(AttackBiteHash);
                    break;
                case 1:
                    Animator.SetTrigger(AttackLeftHash);
                    break;
                case 2:
                    Animator.SetTrigger(AttackRightHash);
                    break;
                case 3:
                    Animator.SetTrigger(AttackRight2Hash);
                    break;
                default:
                    Animator.SetTrigger(AttackTwoHandHash);
                    break;
            }
        }
    }
}
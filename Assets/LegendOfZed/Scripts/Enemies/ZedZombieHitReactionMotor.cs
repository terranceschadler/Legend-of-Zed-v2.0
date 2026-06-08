using System.Collections;
using TopDownShooter;
using UnityEngine;

namespace LegendOfZed.Enemies
{
    [DisallowMultipleComponent]
    public class ZedZombieHitReactionMotor : MonoBehaviour
    {
        public bool EnableHitShove = true;
        public float ShoveDistance = 0.55f;
        public float ShoveSeconds = 0.14f;
        public float ExtraStaggerSeconds = 0.45f;
        public bool PlayHitAudio = true;

        private ZedPrototypeZombieEnemy _zombie;
        private ZedZombieAudioBridge _audioBridge;
        private Transform _playerTarget;
        private Coroutine _shoveRoutine;

        private void Awake()
        {
            _zombie = GetComponent<ZedPrototypeZombieEnemy>();
            _audioBridge = GetComponent<ZedZombieAudioBridge>();
            FindPlayerTarget();
        }

        public void PlayBulletHitReaction()
        {
            if (_zombie == null)
            {
                _zombie = GetComponent<ZedPrototypeZombieEnemy>();
            }

            if (_zombie != null && _zombie.IsDead)
            {
                return;
            }

            if (PlayHitAudio)
            {
                if (_audioBridge == null)
                {
                    _audioBridge = GetComponent<ZedZombieAudioBridge>();
                }

                if (_audioBridge != null)
                {
                    _audioBridge.PlayHitAudio();
                }
            }

            if (!EnableHitShove)
            {
                return;
            }

            if (_zombie != null)
            {
                _zombie.StaggerOnBulletHit = true;
                _zombie.HitStaggerSeconds = Mathf.Max(_zombie.HitStaggerSeconds, ExtraStaggerSeconds);
            }

            if (_playerTarget == null)
            {
                FindPlayerTarget();
            }

            Vector3 direction = Vector3.zero;
            if (_playerTarget != null)
            {
                direction = transform.position - _playerTarget.position;
                direction.y = 0f;
            }

            if (direction.sqrMagnitude <= 0.0001f)
            {
                direction = -transform.forward;
            }

            direction.Normalize();

            if (_shoveRoutine != null)
            {
                StopCoroutine(_shoveRoutine);
            }

            _shoveRoutine = StartCoroutine(ShoveRoutine(direction));
        }

        private IEnumerator ShoveRoutine(Vector3 direction)
        {
            float duration = Mathf.Max(0.03f, ShoveSeconds);
            float elapsed = 0f;
            float speed = ShoveDistance / duration;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                transform.position += direction * speed * Time.deltaTime;
                yield return null;
            }

            _shoveRoutine = null;
        }

        private void FindPlayerTarget()
        {
            PlayerController player = FindFirstObjectByType<PlayerController>();
            if (player != null)
            {
                _playerTarget = player.transform;
                return;
            }

            GameObject taggedPlayer = GameObject.FindGameObjectWithTag("Player");
            if (taggedPlayer != null)
            {
                _playerTarget = taggedPlayer.transform;
            }
        }
    }
}

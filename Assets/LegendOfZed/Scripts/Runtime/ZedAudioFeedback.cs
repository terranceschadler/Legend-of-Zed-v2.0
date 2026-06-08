using UnityEngine;

namespace LegendOfZed.Runtime
{
    [DisallowMultipleComponent]
    public class ZedAudioFeedback : MonoBehaviour
    {
        [Header("Source")]
        public AudioSource AudioSource;
        public bool CreateSourceIfMissing = true;
        public bool Spatialize = true;
        public float SpatialBlend = 1f;
        public float MinDistance = 2f;
        public float MaxDistance = 18f;

        [Header("Clips")]
        public AudioClip[] IdleGroans;
        public AudioClip[] AttackClips;
        public AudioClip[] HitClips;
        public AudioClip[] DeathClips;
        public AudioClip[] HurtClips;

        [Header("Volumes")]
        [Range(0f, 1f)] public float IdleVolume = 0.55f;
        [Range(0f, 1f)] public float AttackVolume = 0.75f;
        [Range(0f, 1f)] public float HitVolume = 0.7f;
        [Range(0f, 1f)] public float DeathVolume = 0.85f;
        [Range(0f, 1f)] public float HurtVolume = 0.75f;

        [Header("Idle Groans")]
        public bool PlayIdleGroans = true;
        public float MinIdleGroanDelay = 3.5f;
        public float MaxIdleGroanDelay = 8f;

        private float _nextIdleGroanTime;

        private void Awake()
        {
            EnsureAudioSource();
            ScheduleNextIdleGroan();
        }

        private void OnEnable()
        {
            EnsureAudioSource();
            ScheduleNextIdleGroan();
        }

        private void Update()
        {
            if (!PlayIdleGroans || IdleGroans == null || IdleGroans.Length == 0)
            {
                return;
            }

            if (Time.time >= _nextIdleGroanTime)
            {
                PlayIdleGroan();
                ScheduleNextIdleGroan();
            }
        }

        public void PlayIdleGroan()
        {
            PlayRandomOneShot(IdleGroans, IdleVolume);
        }

        public void PlayAttack()
        {
            PlayRandomOneShot(AttackClips, AttackVolume);
        }

        public void PlayHit()
        {
            PlayRandomOneShot(HitClips, HitVolume);
        }

        public void PlayDeath()
        {
            PlayRandomOneShot(DeathClips, DeathVolume);
        }

        public void PlayHurt()
        {
            PlayRandomOneShot(HurtClips, HurtVolume);
        }

        public void PlayOneShot(AudioClip clip, float volume)
        {
            if (clip == null)
            {
                return;
            }

            EnsureAudioSource();
            if (AudioSource == null)
            {
                return;
            }

            AudioSource.PlayOneShot(clip, Mathf.Clamp01(volume));
        }

        private void PlayRandomOneShot(AudioClip[] clips, float volume)
        {
            if (clips == null || clips.Length == 0)
            {
                return;
            }

            AudioClip clip = null;
            for (int i = 0; i < 8 && clip == null; i++)
            {
                clip = clips[Random.Range(0, clips.Length)];
            }

            PlayOneShot(clip, volume);
        }

        private void EnsureAudioSource()
        {
            if (AudioSource != null || !CreateSourceIfMissing)
            {
                ConfigureSource();
                return;
            }

            AudioSource = GetComponent<AudioSource>();
            if (AudioSource == null)
            {
                AudioSource = gameObject.AddComponent<AudioSource>();
            }

            ConfigureSource();
        }

        private void ConfigureSource()
        {
            if (AudioSource == null)
            {
                return;
            }

            AudioSource.playOnAwake = false;
            AudioSource.loop = false;
            AudioSource.spatialBlend = Spatialize ? SpatialBlend : 0f;
            AudioSource.minDistance = MinDistance;
            AudioSource.maxDistance = MaxDistance;
        }

        private void ScheduleNextIdleGroan()
        {
            _nextIdleGroanTime = Time.time + Random.Range(MinIdleGroanDelay, MaxIdleGroanDelay);
        }
    }
}

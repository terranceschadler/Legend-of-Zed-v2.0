using LegendOfZed.Runtime;
using UnityEngine;

namespace LegendOfZed.Enemies
{
    [DisallowMultipleComponent]
    public class ZedZombieAudioBridge : MonoBehaviour
    {
        public ZedAudioFeedback AudioFeedback;

        private void Awake()
        {
            if (AudioFeedback == null)
            {
                AudioFeedback = GetComponent<ZedAudioFeedback>();
            }

            if (AudioFeedback == null)
            {
                AudioFeedback = gameObject.AddComponent<ZedAudioFeedback>();
            }
        }

        public void PlayAttackAudio()
        {
            if (AudioFeedback != null)
            {
                AudioFeedback.PlayAttack();
            }
        }

        public void PlayHitAudio()
        {
            if (AudioFeedback != null)
            {
                AudioFeedback.PlayHit();
            }
        }

        public void PlayDeathAudio()
        {
            if (AudioFeedback != null)
            {
                AudioFeedback.PlayDeath();
            }
        }
    }
}

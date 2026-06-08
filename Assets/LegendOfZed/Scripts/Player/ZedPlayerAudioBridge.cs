using LegendOfZed.Runtime;
using UnityEngine;

namespace LegendOfZed.Player
{
    [DisallowMultipleComponent]
    public class ZedPlayerAudioBridge : MonoBehaviour
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

            if (AudioFeedback != null)
            {
                AudioFeedback.Spatialize = false;
                AudioFeedback.PlayIdleGroans = false;
            }
        }

        public void PlayHurtAudio()
        {
            if (AudioFeedback != null)
            {
                AudioFeedback.PlayHurt();
            }
        }
    }
}

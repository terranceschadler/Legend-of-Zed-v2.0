using UnityEngine;

namespace LegendOfZed.Enemies
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Animator))]
    public class ZedZombieRootMotionRelay : MonoBehaviour
    {
        public ZedPrototypeZombieEnemy Owner;

        private Animator _animator;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            if (Owner == null)
            {
                Owner = GetComponentInParent<ZedPrototypeZombieEnemy>();
            }
        }

        private void OnAnimatorMove()
        {
            if (_animator == null)
            {
                _animator = GetComponent<Animator>();
            }

            if (Owner != null && _animator != null)
            {
                Owner.ApplyRootMotionDelta(_animator);
            }
        }
    }
}

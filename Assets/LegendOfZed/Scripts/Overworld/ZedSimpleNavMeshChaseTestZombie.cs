using UnityEngine;
using UnityEngine.AI;

namespace LegendOfZed.Overworld
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(NavMeshAgent))]
    public class ZedSimpleNavMeshChaseTestZombie : MonoBehaviour
    {
        public Transform Target;
        public float Speed = 2.4f;
        public float StoppingDistance = 1.35f;
        public float RepathInterval = 0.2f;

        private NavMeshAgent _agent;
        private float _nextRepathTime;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _agent.speed = Speed;
            _agent.stoppingDistance = StoppingDistance;
            _agent.angularSpeed = 540f;
            _agent.acceleration = 12f;
        }

        private void Update()
        {
            if (Target == null)
            {
                GameObject tagged = GameObject.FindGameObjectWithTag("Player");
                if (tagged != null)
                {
                    Target = tagged.transform;
                }
            }

            if (Target == null || _agent == null || !_agent.isOnNavMesh)
            {
                return;
            }

            if (Time.time < _nextRepathTime)
            {
                return;
            }

            _nextRepathTime = Time.time + RepathInterval;
            _agent.SetDestination(Target.position);
        }
    }
}

using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.AI;

namespace LegendOfZed.Overworld
{
    [DisallowMultipleComponent]
    public class ZedOverworldZombiePathTestSpawner : MonoBehaviour
    {
        [Header("Prefab")]
        public GameObject ZombiePrefab;
        public bool AllowFallbackCapsuleZombie = true;

        [Header("Target")]
        public Transform Target;
        public string TargetObjectName = "Zed_Portal_Test_Player";

        [Header("Spawn")]
        public bool SpawnOnStart = true;
        public int SpawnCount = 3;
        public float SpawnRadius = 1.25f;
        public float NavMeshSampleDistance = 8f;
        public Transform[] SpawnPoints;

        [Header("Navigation")]
        public bool AddNavMeshAgentIfMissing = true;
        public float AgentSpeed = 2.4f;
        public float AgentAngularSpeed = 540f;
        public float AgentAcceleration = 12f;
        public float AgentStoppingDistance = 1.35f;

        [Header("Debug")]
        public bool LogSpawns = true;

        private bool _spawned;

        private IEnumerator Start()
        {
            if (!SpawnOnStart)
            {
                yield break;
            }

            // Let the overworld runtime NavMesh builder run first.
            yield return null;
            yield return null;

            SpawnNow();
        }

        [ContextMenu("Spawn Now")]
        public void SpawnNow()
        {
            if (_spawned)
            {
                return;
            }

            _spawned = true;

            EnsureTarget();

            if (SpawnPoints == null || SpawnPoints.Length == 0)
            {
                Debug.LogWarning("ZedOverworldZombiePathTestSpawner has no spawn points.", this);
                return;
            }

            int spawned = 0;
            for (int i = 0; i < SpawnCount; i++)
            {
                Transform point = SpawnPoints[i % SpawnPoints.Length];
                if (point == null)
                {
                    continue;
                }

                Vector3 desired = point.position + Random.insideUnitSphere * SpawnRadius;
                desired.y = point.position.y;

                if (!NavMesh.SamplePosition(desired, out NavMeshHit hit, NavMeshSampleDistance, NavMesh.AllAreas))
                {
                    if (!NavMesh.SamplePosition(point.position, out hit, NavMeshSampleDistance, NavMesh.AllAreas))
                    {
                        Debug.LogWarning("Could not find NavMesh position near spawn point " + point.name + ".", point);
                        continue;
                    }
                }

                GameObject zombie = CreateZombie(hit.position, point.rotation, i + 1);
                if (zombie == null)
                {
                    continue;
                }

                ConfigureZombie(zombie);
                spawned++;
            }

            if (LogSpawns)
            {
                Debug.Log("v2.8 overworld zombie path test spawned " + spawned + " zombie(s).", this);
            }
        }

        private GameObject CreateZombie(Vector3 position, Quaternion rotation, int index)
        {
            GameObject zombie = null;

            if (ZombiePrefab != null)
            {
                zombie = Instantiate(ZombiePrefab, position, rotation);
                zombie.name = "Overworld_PathTest_Zombie_" + index + "_" + ZombiePrefab.name;
                zombie.SetActive(true);
                return zombie;
            }

            if (!AllowFallbackCapsuleZombie)
            {
                Debug.LogWarning("No ZombiePrefab assigned and fallback capsule zombie is disabled.", this);
                return null;
            }

            zombie = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            zombie.name = "Overworld_PathTest_FallbackZombie_" + index;
            zombie.transform.position = position;
            zombie.transform.rotation = rotation;
            zombie.transform.localScale = new Vector3(1f, 1.1f, 1f);

            ZedSimpleNavMeshChaseTestZombie chase = zombie.AddComponent<ZedSimpleNavMeshChaseTestZombie>();
            chase.Target = Target;
            chase.Speed = AgentSpeed;
            chase.StoppingDistance = AgentStoppingDistance;

            return zombie;
        }

        private void ConfigureZombie(GameObject zombie)
        {
            if (zombie == null)
            {
                return;
            }

            NavMeshAgent agent = zombie.GetComponent<NavMeshAgent>();
            if (agent == null && AddNavMeshAgentIfMissing)
            {
                agent = zombie.AddComponent<NavMeshAgent>();
            }

            if (agent != null)
            {
                agent.speed = AgentSpeed;
                agent.angularSpeed = AgentAngularSpeed;
                agent.acceleration = AgentAcceleration;
                agent.stoppingDistance = AgentStoppingDistance;
                agent.updateRotation = true;
                agent.updatePosition = true;

                if (NavMesh.SamplePosition(zombie.transform.position, out NavMeshHit hit, NavMeshSampleDistance, NavMesh.AllAreas))
                {
                    agent.Warp(hit.position);
                }

                if (Target != null)
                {
                    agent.SetDestination(Target.position);
                }
            }

            if (Target != null)
            {
                AssignTargetByReflection(zombie, Target);
            }

            ZedSimpleNavMeshChaseTestZombie fallbackChase = zombie.GetComponent<ZedSimpleNavMeshChaseTestZombie>();
            if (fallbackChase == null && ZombiePrefab == null)
            {
                fallbackChase = zombie.AddComponent<ZedSimpleNavMeshChaseTestZombie>();
            }

            if (fallbackChase != null)
            {
                fallbackChase.Target = Target;
                fallbackChase.Speed = AgentSpeed;
                fallbackChase.StoppingDistance = AgentStoppingDistance;
            }
        }

        private void EnsureTarget()
        {
            if (Target != null)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(TargetObjectName))
            {
                GameObject named = GameObject.Find(TargetObjectName);
                if (named != null)
                {
                    Target = named.transform;
                    return;
                }
            }

            GameObject tagged = GameObject.FindGameObjectWithTag("Player");
            if (tagged != null)
            {
                Target = tagged.transform;
                return;
            }

            CharacterController controller = FindAnyObjectByType<CharacterController>();
            if (controller != null)
            {
                Target = controller.transform;
            }
        }

        private static void AssignTargetByReflection(GameObject zombie, Transform target)
        {
            Component[] components = zombie.GetComponentsInChildren<Component>(true);
            for (int i = 0; i < components.Length; i++)
            {
                Component component = components[i];
                if (component == null)
                {
                    continue;
                }

                System.Type type = component.GetType();
                TryAssignMember(component, type, "Target", target);
                TryAssignMember(component, type, "target", target);
                TryAssignMember(component, type, "PlayerTarget", target);
                TryAssignMember(component, type, "playerTarget", target);
                TryAssignMember(component, type, "ChaseTarget", target);
                TryAssignMember(component, type, "chaseTarget", target);
            }
        }

        private static void TryAssignMember(Component component, System.Type type, string memberName, Transform target)
        {
            FieldInfo field = type.GetField(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field != null)
            {
                if (field.FieldType == typeof(Transform))
                {
                    field.SetValue(component, target);
                    return;
                }

                if (field.FieldType == typeof(GameObject))
                {
                    field.SetValue(component, target.gameObject);
                    return;
                }
            }

            PropertyInfo property = type.GetProperty(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (property != null && property.CanWrite)
            {
                if (property.PropertyType == typeof(Transform))
                {
                    property.SetValue(component, target, null);
                    return;
                }

                if (property.PropertyType == typeof(GameObject))
                {
                    property.SetValue(component, target.gameObject, null);
                }
            }
        }
    }
}

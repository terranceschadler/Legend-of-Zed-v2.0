using System.IO;
using LegendOfZed.Enemies;
using TopDownShooter;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace LegendOfZed.Editor
{
    public static class ZedV10ZombieEnemyPrototypeSetup
    {
        private const string ScenePath = "Assets/LegendOfZed/Scenes/Zed_Controller_Test.unity";
        private const string ZombieRootName = "Zed_Prototype_Zombie_Enemy";
        private const string ZombieVisualName = "Zed_Prototype_Zombie_Visual";
        private const string SyntyRoot = "Assets/Synty";

        [MenuItem("Legend of Zed/Setup/v1.0 Add Prototype Zombie Enemy")]
        public static void AddPrototypeZombieEnemy()
        {
            if (!File.Exists(ScenePath))
            {
                Debug.LogWarning("v1.0 zombie setup could not find scene: " + ScenePath);
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            PlayerController player = Object.FindFirstObjectByType<PlayerController>();
            Vector3 spawnPosition = ResolveSpawnPosition(player);

            GameObject zombieRoot = GameObject.Find(ZombieRootName);
            if (zombieRoot == null)
            {
                zombieRoot = new GameObject(ZombieRootName);
                EditorSceneManager.MoveGameObjectToScene(zombieRoot, scene);
            }

            zombieRoot.transform.position = spawnPosition;
            zombieRoot.transform.rotation = Quaternion.identity;
            zombieRoot.transform.localScale = Vector3.one;

            CapsuleCollider capsule = zombieRoot.GetComponent<CapsuleCollider>();
            if (capsule == null)
            {
                capsule = zombieRoot.AddComponent<CapsuleCollider>();
            }

            capsule.center = new Vector3(0f, 0.9f, 0f);
            capsule.height = 1.8f;
            capsule.radius = 0.32f;
            capsule.isTrigger = false;

            Rigidbody rigidbody = zombieRoot.GetComponent<Rigidbody>();
            if (rigidbody == null)
            {
                rigidbody = zombieRoot.AddComponent<Rigidbody>();
            }

            rigidbody.isKinematic = true;
            rigidbody.useGravity = false;

            NavMeshAgent agent = zombieRoot.GetComponent<NavMeshAgent>();
            if (agent == null)
            {
                agent = zombieRoot.AddComponent<NavMeshAgent>();
            }

            agent.updatePosition = false;
            agent.updateRotation = false;
            agent.radius = 0.32f;
            agent.height = 1.8f;
            agent.speed = 2.15f;
            agent.angularSpeed = 540f;
            agent.stoppingDistance = 1.05f;
            agent.autoBraking = true;

            GameObject visual = EnsureZombieVisual(zombieRoot.transform);
            Animator animator = visual.GetComponentInChildren<Animator>();

            ZedPrototypeZombieEnemy zombieBrain = zombieRoot.GetComponent<ZedPrototypeZombieEnemy>();
            if (zombieBrain == null)
            {
                zombieBrain = zombieRoot.AddComponent<ZedPrototypeZombieEnemy>();
            }

            zombieBrain.Target = player != null ? player.transform : null;
            zombieBrain.NavMeshAgent = agent;
            zombieBrain.Animator = animator;
            zombieBrain.DetectionRange = 11f;
            zombieBrain.LoseTargetRange = 15f;
            zombieBrain.AttackRange = 1.35f;
            zombieBrain.WalkSpeed = 1.2f;
            zombieBrain.ChaseSpeed = 2.15f;
            zombieBrain.StoppingDistance = 1.05f;
            zombieBrain.AttackDamage = 10f;
            zombieBrain.AttackCooldown = 1.25f;

            EnsureHitPointIfAvailable(zombieRoot);

            EditorUtility.SetDirty(zombieRoot);
            EditorUtility.SetDirty(zombieBrain);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("v1.0 prototype zombie enemy added to " + ScenePath + ". NavMeshAgent is configured for pathing only; visible movement is driven manually. Player controller, ShooterController, WeaponData, bullets, ammo, projectile IDs, and v0.9 feedback code were not changed.");
        }

        private static Vector3 ResolveSpawnPosition(PlayerController player)
        {
            Vector3 basePosition = player != null ? player.transform.position : Vector3.zero;
            Vector3 candidate = basePosition + new Vector3(0f, 0f, 7f);

            NavMeshHit hit;
            if (NavMesh.SamplePosition(candidate, out hit, 6f, NavMesh.AllAreas))
            {
                return hit.position;
            }

            return candidate;
        }

        private static GameObject EnsureZombieVisual(Transform root)
        {
            Transform existing = root.Find(ZombieVisualName);
            if (existing != null)
            {
                return existing.gameObject;
            }

            GameObject source = FindZombieVisualAsset();
            GameObject visual;
            if (source != null)
            {
                visual = (GameObject)PrefabUtility.InstantiatePrefab(source);
                visual.name = ZombieVisualName;
            }
            else
            {
                visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                visual.name = ZombieVisualName;
                Object.DestroyImmediate(visual.GetComponent<Collider>());
            }

            visual.transform.SetParent(root, false);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.identity;
            visual.transform.localScale = Vector3.one;
            return visual;
        }

        private static GameObject FindZombieVisualAsset()
        {
            string[] searchTerms =
            {
                "Zombie t:Prefab",
                "Character_Zombie t:Prefab",
                "Zombie t:Model",
                "Character_Zombie t:Model"
            };

            foreach (string searchTerm in searchTerms)
            {
                string[] guids = AssetDatabase.FindAssets(searchTerm, new[] { SyntyRoot });
                foreach (string guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    if (string.IsNullOrEmpty(path))
                    {
                        continue;
                    }

                    string fileName = Path.GetFileNameWithoutExtension(path).ToLowerInvariant();
                    if (!fileName.Contains("zombie"))
                    {
                        continue;
                    }

                    GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    if (asset != null)
                    {
                        return asset;
                    }
                }
            }

            return null;
        }

        private static void EnsureHitPointIfAvailable(GameObject zombieRoot)
        {
            if (zombieRoot.GetComponent("HitPoint") != null)
            {
                return;
            }

            System.Type hitPointType = FindTypeByName("TopDownShooter.HitPoint") ?? FindTypeByName("HitPoint");
            if (hitPointType == null || !typeof(Component).IsAssignableFrom(hitPointType))
            {
                Debug.LogWarning("v1.0 zombie setup could not find a HitPoint component type. Zombie was created, but bullets may not damage it until a project health/HitPoint component is added.");
                return;
            }

            Component hitPoint = zombieRoot.AddComponent(hitPointType);
            EditorUtility.SetDirty(hitPoint);
        }

        private static System.Type FindTypeByName(string typeName)
        {
            System.Reflection.Assembly[] assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; i++)
            {
                System.Type type = assemblies[i].GetType(typeName);
                if (type != null)
                {
                    return type;
                }
            }

            return null;
        }
    }
}

using System.IO;
using LegendOfZed.Enemies;
using LegendOfZed.Runtime;
using TopDownShooter;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace LegendOfZed.Editor
{
    public static class ZedV19ZombiePrefabBaselineSetup
    {
        private const string ScenePath = "Assets/LegendOfZed/Scenes/Zed_Controller_Test.unity";
        private const string ScenePrototypeName = "Zed_Prototype_Zombie_Enemy";
        private const string SpawnerName = "Zed_Zombie_Spawner";
        private const string PrefabFolder = "Assets/LegendOfZed/Prefabs/Enemies";
        private const string PrefabPath = PrefabFolder + "/Zed_Zombie_Enemy_Baseline.prefab";

        [MenuItem("Legend of Zed/Setup/v1.9 Create Zombie Prefab Baseline")]
        public static void CreateZombiePrefabBaseline()
        {
            if (!OpenTestScene())
            {
                return;
            }

            GameObject scenePrototype = GameObject.Find(ScenePrototypeName);
            if (scenePrototype == null)
            {
                Debug.LogWarning("Could not find scene zombie prototype: " + ScenePrototypeName);
                return;
            }

            Directory.CreateDirectory(PrefabFolder);

            GameObject workingCopy = Object.Instantiate(scenePrototype);
            workingCopy.name = "Zed_Zombie_Enemy_Baseline";

            CleanZombieObject(workingCopy);
            EnsureCoreComponents(workingCopy);

            PrefabUtility.SaveAsPrefabAsset(workingCopy, PrefabPath);
            Object.DestroyImmediate(workingCopy);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (prefab == null)
            {
                Debug.LogWarning("Failed to load created zombie prefab at: " + PrefabPath);
                return;
            }

            AssignPrefabToSpawner(prefab);

            Scene scene = SceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("v1.9 zombie prefab baseline created at " + PrefabPath + " and assigned to the scene spawner.");
        }

        [MenuItem("Legend of Zed/Setup/v1.9 Validate Zombie Prefab Baseline")]
        public static void ValidateZombiePrefabBaseline()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (prefab == null)
            {
                Debug.LogWarning("Missing zombie prefab baseline: " + PrefabPath);
                return;
            }

            int warnings = 0;

            if (prefab.GetComponent<ZedPrototypeZombieEnemy>() == null)
            {
                Debug.LogWarning("Zombie prefab missing ZedPrototypeZombieEnemy.");
                warnings++;
            }

            Animator animator = prefab.GetComponentInChildren<Animator>(true);
            if (animator == null)
            {
                Debug.LogWarning("Zombie prefab missing Animator.");
                warnings++;
            }
            else if (animator.GetComponent<ZedZombieRootMotionRelay>() == null)
            {
                Debug.LogWarning("Zombie prefab Animator missing ZedZombieRootMotionRelay.");
                warnings++;
            }

            if (prefab.GetComponent<ZedZombieHitReactionMotor>() == null)
            {
                Debug.LogWarning("Zombie prefab missing ZedZombieHitReactionMotor.");
                warnings++;
            }

            if (prefab.GetComponent<ZedZombieAudioBridge>() == null)
            {
                Debug.LogWarning("Zombie prefab missing ZedZombieAudioBridge.");
                warnings++;
            }

            if (prefab.GetComponent<ZedAudioFeedback>() == null)
            {
                Debug.LogWarning("Zombie prefab missing ZedAudioFeedback.");
                warnings++;
            }

            HitPoint[] hitPoints = prefab.GetComponentsInChildren<HitPoint>(true);
            if (hitPoints.Length > 0)
            {
                Debug.LogWarning("Zombie prefab has demo TopDownShooter.HitPoint component(s). Remove them.");
                warnings++;
            }

            NavMeshAgent navMeshAgent = prefab.GetComponent<NavMeshAgent>();
            if (navMeshAgent != null)
            {
                Debug.LogWarning("Zombie prefab has NavMeshAgent. Current baseline should not require NavMeshAgent.");
                warnings++;
            }

            Rigidbody[] bodies = prefab.GetComponentsInChildren<Rigidbody>(true);
            if (bodies.Length <= 1)
            {
                Debug.LogWarning("Zombie prefab appears to have no ragdoll rigidbodies. Rebuild tuned ragdoll before creating prefab.");
                warnings++;
            }

            if (warnings == 0)
            {
                Debug.Log("v1.9 zombie prefab baseline validation passed: " + PrefabPath);
            }
            else
            {
                Debug.LogWarning("v1.9 zombie prefab baseline validation finished with " + warnings + " warning(s).");
            }
        }

        [MenuItem("Legend of Zed/Setup/v1.9 Assign Zombie Prefab To Spawner")]
        public static void AssignZombiePrefabToSpawner()
        {
            if (!OpenTestScene())
            {
                return;
            }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (prefab == null)
            {
                Debug.LogWarning("Missing zombie prefab baseline: " + PrefabPath);
                return;
            }

            AssignPrefabToSpawner(prefab);

            Scene scene = SceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("Assigned zombie prefab baseline to scene spawner: " + PrefabPath);
        }

        private static bool OpenTestScene()
        {
            if (!File.Exists(ScenePath))
            {
                Debug.LogWarning("Could not find scene: " + ScenePath);
                return false;
            }

            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            return true;
        }

        private static void AssignPrefabToSpawner(GameObject prefab)
        {
            GameObject spawnerObject = GameObject.Find(SpawnerName);
            if (spawnerObject == null)
            {
                Debug.LogWarning("Could not find spawner object: " + SpawnerName);
                return;
            }

            ZedZombieSpawner spawner = spawnerObject.GetComponent<ZedZombieSpawner>();
            if (spawner == null)
            {
                Debug.LogWarning("Spawner object is missing ZedZombieSpawner.");
                return;
            }

            spawner.ZombiePrototype = prefab;
            spawner.HidePrototypeOnStart = false;
            spawner.SpawnOnStart = true;
            spawner.RagdollOnDeath = true;
            spawner.DeathDisableDelay = Mathf.Max(spawner.DeathDisableDelay, 999f);

            EditorUtility.SetDirty(spawner);
            EditorUtility.SetDirty(spawnerObject);
        }

        private static void EnsureCoreComponents(GameObject root)
        {
            ZedPrototypeZombieEnemy brain = root.GetComponent<ZedPrototypeZombieEnemy>();
            if (brain == null)
            {
                brain = root.AddComponent<ZedPrototypeZombieEnemy>();
            }

            brain.RagdollOnDeath = true;
            brain.DestroyOnDeath = false;
            brain.DeathDisableDelay = Mathf.Max(brain.DeathDisableDelay, 999f);
            brain.StaggerOnBulletHit = true;
            brain.HitStaggerSeconds = Mathf.Max(brain.HitStaggerSeconds, 0.45f);
            brain.RequireTargetInRangeAtHitMoment = true;

            Animator animator = root.GetComponentInChildren<Animator>(true);
            if (animator != null)
            {
                brain.Animator = animator;

                ZedZombieRootMotionRelay relay = animator.GetComponent<ZedZombieRootMotionRelay>();
                if (relay == null)
                {
                    relay = animator.gameObject.AddComponent<ZedZombieRootMotionRelay>();
                }

                relay.Owner = brain;
                animator.applyRootMotion = true;
            }

            if (root.GetComponent<ZedZombieHitReactionMotor>() == null)
            {
                root.AddComponent<ZedZombieHitReactionMotor>();
            }

            if (root.GetComponent<ZedZombieAudioBridge>() == null)
            {
                root.AddComponent<ZedZombieAudioBridge>();
            }

            if (root.GetComponent<ZedAudioFeedback>() == null)
            {
                ZedAudioFeedback feedback = root.AddComponent<ZedAudioFeedback>();
                feedback.Spatialize = true;
                feedback.PlayIdleGroans = true;
            }
        }

        private static void CleanZombieObject(GameObject root)
        {
            HitPoint[] hitPoints = root.GetComponentsInChildren<HitPoint>(true);
            for (int i = 0; i < hitPoints.Length; i++)
            {
                Object.DestroyImmediate(hitPoints[i], true);
            }

            NavMeshAgent agent = root.GetComponent<NavMeshAgent>();
            if (agent != null)
            {
                Object.DestroyImmediate(agent, true);
            }

            ZedPrototypeZombieEnemy brain = root.GetComponent<ZedPrototypeZombieEnemy>();
            if (brain != null)
            {
                brain.NavMeshAgent = null;
                brain.Target = null;
                brain.CurrentHealth = brain.MaxHealth;
            }
        }
    }
}

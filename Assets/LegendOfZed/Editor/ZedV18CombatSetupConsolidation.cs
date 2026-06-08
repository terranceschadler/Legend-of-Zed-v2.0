using System.IO;
using LegendOfZed.Enemies;
using LegendOfZed.Player;
using LegendOfZed.Runtime;
using TopDownShooter;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace LegendOfZed.Editor
{
    public static class ZedV18CombatSetupConsolidation
    {
        private const string ScenePath = "Assets/LegendOfZed/Scenes/Zed_Controller_Test.unity";
        private const string PrototypeName = "Zed_Prototype_Zombie_Enemy";
        private const string SpawnerName = "Zed_Zombie_Spawner";

        [MenuItem("Legend of Zed/Setup/v1.8 Validate Current Combat Setup")]
        public static void ValidateCurrentCombatSetup()
        {
            if (!OpenTestScene())
            {
                return;
            }

            int warnings = 0;

            warnings += ValidatePlayer();
            warnings += ValidateZombiePrototype();
            warnings += ValidateSpawner();
            warnings += ValidateZombies();

            if (warnings == 0)
            {
                Debug.Log("v1.8 combat setup validation passed. Player health, zombie prototype, spawner, ragdoll/audio/hit reaction components look present.");
            }
            else
            {
                Debug.LogWarning("v1.8 combat setup validation finished with " + warnings + " warning(s). Review the console messages above.");
            }
        }

        [MenuItem("Legend of Zed/Setup/v1.8 Apply Stable Combat Defaults")]
        public static void ApplyStableCombatDefaults()
        {
            if (!OpenTestScene())
            {
                return;
            }

            ApplyPlayerDefaults();
            ApplyZombieDefaults();
            ApplySpawnerDefaults();

            Scene scene = SceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("v1.8 stable combat defaults applied. Gameplay systems unchanged; only current component defaults were normalized.");
        }

        [MenuItem("Legend of Zed/Setup/v1.8 Print Stable Setup Workflow")]
        public static void PrintStableSetupWorkflow()
        {
            Debug.Log(
                "Stable combat setup workflow as of v1.8:\\n" +
                "1. v1.1 Rebuild Tuned Zombie Ragdoll\\n" +
                "2. v1.2 Add Zombie Spawner\\n" +
                "3. v1.3 Cleanup Zombie Scene Components\\n" +
                "4. v1.4 Add Player Health\\n" +
                "5. v1.5 Apply Zombie Attack Feel Defaults\\n" +
                "6. v1.5b Apply Stronger Zombie Hit Reaction\\n" +
                "7. v1.6 Add Combat Audio Feedback Components\\n" +
                "8. v1.7 Clean Debug HUD And Logs\\n" +
                "9. v1.8 Validate Current Combat Setup\\n\\n" +
                "Rules: zombies use ZedPrototypeZombieEnemy, not demo HitPoint. NavMeshAgent is optional. Current branch uses zip-delivered assets, then local Unity test, then commit/push.");
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

        private static int ValidatePlayer()
        {
            int warnings = 0;
            PlayerController playerController = Object.FindFirstObjectByType<PlayerController>();
            GameObject playerObject = playerController != null ? playerController.gameObject : GameObject.FindGameObjectWithTag("Player");

            if (playerObject == null)
            {
                Debug.LogWarning("v1.8 validation: Could not find PlayerController or Player-tagged object.");
                return 1;
            }

            if (playerObject.GetComponent<ZedPlayerHealth>() == null)
            {
                Debug.LogWarning("v1.8 validation: Player is missing ZedPlayerHealth.");
                warnings++;
            }

            if (playerObject.GetComponent<ZedPlayerAudioBridge>() == null)
            {
                Debug.LogWarning("v1.8 validation: Player is missing ZedPlayerAudioBridge. Audio clips are optional, but the bridge should exist after v1.6.");
                warnings++;
            }

            if (playerObject.GetComponent<ZedAudioFeedback>() == null)
            {
                Debug.LogWarning("v1.8 validation: Player is missing ZedAudioFeedback.");
                warnings++;
            }

            return warnings;
        }

        private static int ValidateZombiePrototype()
        {
            int warnings = 0;
            GameObject prototype = GameObject.Find(PrototypeName);
            if (prototype == null)
            {
                Debug.LogWarning("v1.8 validation: Missing zombie prototype object: " + PrototypeName);
                return 1;
            }

            ZedPrototypeZombieEnemy brain = prototype.GetComponent<ZedPrototypeZombieEnemy>();
            if (brain == null)
            {
                Debug.LogWarning("v1.8 validation: Zombie prototype is missing ZedPrototypeZombieEnemy.");
                warnings++;
            }

            Animator animator = prototype.GetComponentInChildren<Animator>(true);
            if (animator == null)
            {
                Debug.LogWarning("v1.8 validation: Zombie prototype is missing child Animator.");
                warnings++;
            }
            else if (animator.GetComponent<ZedZombieRootMotionRelay>() == null)
            {
                Debug.LogWarning("v1.8 validation: Zombie Animator is missing ZedZombieRootMotionRelay.");
                warnings++;
            }

            if (prototype.GetComponent<ZedZombieHitReactionMotor>() == null)
            {
                Debug.LogWarning("v1.8 validation: Zombie prototype is missing ZedZombieHitReactionMotor.");
                warnings++;
            }

            if (prototype.GetComponent<ZedZombieAudioBridge>() == null)
            {
                Debug.LogWarning("v1.8 validation: Zombie prototype is missing ZedZombieAudioBridge.");
                warnings++;
            }

            if (prototype.GetComponent<ZedAudioFeedback>() == null)
            {
                Debug.LogWarning("v1.8 validation: Zombie prototype is missing ZedAudioFeedback.");
                warnings++;
            }

            HitPoint[] hitPoints = prototype.GetComponentsInChildren<HitPoint>(true);
            if (hitPoints.Length > 0)
            {
                Debug.LogWarning("v1.8 validation: Zombie prototype still has demo TopDownShooter.HitPoint component(s). Remove them from zombie hierarchy.");
                warnings++;
            }

            NavMeshAgent agent = prototype.GetComponent<NavMeshAgent>();
            if (agent != null && !HasUsableNavMeshAt(prototype.transform.position))
            {
                Debug.LogWarning("v1.8 validation: Zombie prototype has NavMeshAgent but no usable NavMesh. Current workflow should not require it.");
                warnings++;
            }

            return warnings;
        }

        private static int ValidateSpawner()
        {
            int warnings = 0;
            GameObject spawnerObject = GameObject.Find(SpawnerName);
            if (spawnerObject == null)
            {
                Debug.LogWarning("v1.8 validation: Missing zombie spawner object: " + SpawnerName);
                return 1;
            }

            ZedZombieSpawner spawner = spawnerObject.GetComponent<ZedZombieSpawner>();
            if (spawner == null)
            {
                Debug.LogWarning("v1.8 validation: Spawner object is missing ZedZombieSpawner.");
                return 1;
            }

            if (spawner.ZombiePrototype == null)
            {
                Debug.LogWarning("v1.8 validation: ZedZombieSpawner has no ZombiePrototype assigned.");
                warnings++;
            }

            if (spawner.SpawnPoints == null || spawner.SpawnPoints.Count == 0)
            {
                Debug.LogWarning("v1.8 validation: ZedZombieSpawner has no spawn points.");
                warnings++;
            }

            return warnings;
        }

        private static int ValidateZombies()
        {
            int warnings = 0;
            ZedPrototypeZombieEnemy[] zombies = Object.FindObjectsByType<ZedPrototypeZombieEnemy>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

            if (zombies.Length == 0)
            {
                Debug.LogWarning("v1.8 validation: No ZedPrototypeZombieEnemy objects found in scene.");
                return 1;
            }

            for (int i = 0; i < zombies.Length; i++)
            {
                ZedPrototypeZombieEnemy zombie = zombies[i];
                if (zombie == null)
                {
                    continue;
                }

                HitPoint[] hitPoints = zombie.GetComponentsInChildren<HitPoint>(true);
                if (hitPoints.Length > 0)
                {
                    Debug.LogWarning("v1.8 validation: " + zombie.name + " has demo HitPoint component(s).");
                    warnings++;
                }
            }

            return warnings;
        }

        private static void ApplyPlayerDefaults()
        {
            PlayerController playerController = Object.FindFirstObjectByType<PlayerController>();
            GameObject playerObject = playerController != null ? playerController.gameObject : GameObject.FindGameObjectWithTag("Player");
            if (playerObject == null)
            {
                return;
            }

            ZedPlayerHealth health = playerObject.GetComponent<ZedPlayerHealth>();
            if (health != null)
            {
                health.ShowDebugHud = false;
                health.LogDamageForTesting = false;
                health.ShowDamageFlash = true;
                health.PlayHurtAudio = true;
                EditorUtility.SetDirty(health);
            }

            ZedAudioFeedback feedback = playerObject.GetComponent<ZedAudioFeedback>();
            if (feedback != null)
            {
                feedback.Spatialize = false;
                feedback.PlayIdleGroans = false;
                EditorUtility.SetDirty(feedback);
            }

            EditorUtility.SetDirty(playerObject);
        }

        private static void ApplyZombieDefaults()
        {
            ZedPrototypeZombieEnemy[] zombies = Object.FindObjectsByType<ZedPrototypeZombieEnemy>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

            for (int i = 0; i < zombies.Length; i++)
            {
                ZedPrototypeZombieEnemy zombie = zombies[i];
                if (zombie == null)
                {
                    continue;
                }

                RemoveDemoHitPoints(zombie.gameObject);
                RemoveBadNavMeshAgent(zombie.gameObject);

                zombie.RagdollOnDeath = true;
                zombie.DestroyOnDeath = false;
                zombie.DeathDisableDelay = Mathf.Max(zombie.DeathDisableDelay, 999f);
                zombie.StaggerOnBulletHit = true;
                zombie.HitStaggerSeconds = Mathf.Max(zombie.HitStaggerSeconds, 0.45f);
                zombie.AttackCooldown = Mathf.Max(zombie.AttackCooldown, 1.45f);
                zombie.AttackWindupSeconds = Mathf.Max(zombie.AttackWindupSeconds, 0.35f);
                zombie.RequireTargetInRangeAtHitMoment = true;

                Animator animator = zombie.Animator != null ? zombie.Animator : zombie.GetComponentInChildren<Animator>(true);
                if (animator != null)
                {
                    zombie.Animator = animator;
                    ZedZombieRootMotionRelay relay = animator.GetComponent<ZedZombieRootMotionRelay>();
                    if (relay == null)
                    {
                        relay = animator.gameObject.AddComponent<ZedZombieRootMotionRelay>();
                    }

                    relay.Owner = zombie;
                    EditorUtility.SetDirty(relay);
                    EditorUtility.SetDirty(animator);
                }

                if (zombie.GetComponent<ZedZombieHitReactionMotor>() == null)
                {
                    zombie.gameObject.AddComponent<ZedZombieHitReactionMotor>();
                }

                if (zombie.GetComponent<ZedZombieAudioBridge>() == null)
                {
                    zombie.gameObject.AddComponent<ZedZombieAudioBridge>();
                }

                if (zombie.GetComponent<ZedAudioFeedback>() == null)
                {
                    zombie.gameObject.AddComponent<ZedAudioFeedback>();
                }

                EditorUtility.SetDirty(zombie);
                EditorUtility.SetDirty(zombie.gameObject);
            }
        }

        private static void ApplySpawnerDefaults()
        {
            ZedZombieSpawner[] spawners = Object.FindObjectsByType<ZedZombieSpawner>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

            for (int i = 0; i < spawners.Length; i++)
            {
                ZedZombieSpawner spawner = spawners[i];
                if (spawner == null)
                {
                    continue;
                }

                spawner.SpawnOnStart = true;
                spawner.HidePrototypeOnStart = true;
                spawner.MaxAliveZombies = Mathf.Max(1, spawner.MaxAliveZombies);
                spawner.DeathDisableDelay = Mathf.Max(spawner.DeathDisableDelay, 999f);
                spawner.RagdollOnDeath = true;

                EditorUtility.SetDirty(spawner);
            }
        }

        private static void RemoveDemoHitPoints(GameObject root)
        {
            HitPoint[] hitPoints = root.GetComponentsInChildren<HitPoint>(true);
            for (int i = 0; i < hitPoints.Length; i++)
            {
                Object.DestroyImmediate(hitPoints[i], true);
            }
        }

        private static void RemoveBadNavMeshAgent(GameObject root)
        {
            NavMeshAgent agent = root.GetComponent<NavMeshAgent>();
            if (agent == null)
            {
                return;
            }

            if (!HasUsableNavMeshAt(root.transform.position))
            {
                Object.DestroyImmediate(agent, true);
            }
        }

        private static bool HasUsableNavMeshAt(Vector3 position)
        {
            NavMeshHit hit;
            return NavMesh.SamplePosition(position, out hit, 25f, NavMesh.AllAreas);
        }
    }
}

using System.IO;
using LegendOfZed.Enemies;
using LegendOfZed.Player;
using LegendOfZed.Runtime;
using TopDownShooter;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LegendOfZed.Editor
{
    public static class ZedV16CombatAudioSetup
    {
        private const string ScenePath = "Assets/LegendOfZed/Scenes/Zed_Controller_Test.unity";

        [MenuItem("Legend of Zed/Setup/v1.6 Add Combat Audio Feedback Components")]
        public static void AddCombatAudioFeedbackComponents()
        {
            if (!File.Exists(ScenePath))
            {
                Debug.LogWarning("Could not find scene: " + ScenePath);
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            int zombies = SetupZombieAudio();
            bool player = SetupPlayerAudio();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("v1.6 combat audio feedback components added. Zombies updated=" + zombies + " Player updated=" + player + ". Assign AudioClips in ZedAudioFeedback components to hear sounds.");
        }

        private static int SetupZombieAudio()
        {
            ZedPrototypeZombieEnemy[] zombieBrains = Object.FindObjectsByType<ZedPrototypeZombieEnemy>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

            int count = 0;
            for (int i = 0; i < zombieBrains.Length; i++)
            {
                ZedPrototypeZombieEnemy zombie = zombieBrains[i];
                if (zombie == null)
                {
                    continue;
                }

                ZedAudioFeedback feedback = zombie.GetComponent<ZedAudioFeedback>();
                if (feedback == null)
                {
                    feedback = zombie.gameObject.AddComponent<ZedAudioFeedback>();
                }

                feedback.Spatialize = true;
                feedback.PlayIdleGroans = true;

                ZedZombieAudioBridge bridge = zombie.GetComponent<ZedZombieAudioBridge>();
                if (bridge == null)
                {
                    bridge = zombie.gameObject.AddComponent<ZedZombieAudioBridge>();
                }

                bridge.AudioFeedback = feedback;

                ZedZombieHitReactionMotor motor = zombie.GetComponent<ZedZombieHitReactionMotor>();
                if (motor == null)
                {
                    motor = zombie.gameObject.AddComponent<ZedZombieHitReactionMotor>();
                }

                motor.PlayHitAudio = true;

                EditorUtility.SetDirty(feedback);
                EditorUtility.SetDirty(bridge);
                EditorUtility.SetDirty(motor);
                EditorUtility.SetDirty(zombie);
                count++;
            }

            ZedZombieSpawner[] spawners = Object.FindObjectsByType<ZedZombieSpawner>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

            for (int i = 0; i < spawners.Length; i++)
            {
                if (spawners[i] != null && spawners[i].ZombiePrototype != null)
                {
                    GameObject prototype = spawners[i].ZombiePrototype;

                    ZedAudioFeedback feedback = prototype.GetComponent<ZedAudioFeedback>();
                    if (feedback == null)
                    {
                        feedback = prototype.AddComponent<ZedAudioFeedback>();
                    }

                    ZedZombieAudioBridge bridge = prototype.GetComponent<ZedZombieAudioBridge>();
                    if (bridge == null)
                    {
                        bridge = prototype.AddComponent<ZedZombieAudioBridge>();
                    }

                    bridge.AudioFeedback = feedback;

                    ZedZombieHitReactionMotor motor = prototype.GetComponent<ZedZombieHitReactionMotor>();
                    if (motor == null)
                    {
                        motor = prototype.AddComponent<ZedZombieHitReactionMotor>();
                    }

                    motor.PlayHitAudio = true;

                    EditorUtility.SetDirty(feedback);
                    EditorUtility.SetDirty(bridge);
                    EditorUtility.SetDirty(motor);
                }
            }

            return count;
        }

        private static bool SetupPlayerAudio()
        {
            PlayerController playerController = Object.FindFirstObjectByType<PlayerController>();
            GameObject playerObject = playerController != null ? playerController.gameObject : GameObject.FindGameObjectWithTag("Player");
            if (playerObject == null)
            {
                return false;
            }

            ZedAudioFeedback feedback = playerObject.GetComponent<ZedAudioFeedback>();
            if (feedback == null)
            {
                feedback = playerObject.AddComponent<ZedAudioFeedback>();
            }

            feedback.Spatialize = false;
            feedback.PlayIdleGroans = false;

            ZedPlayerAudioBridge bridge = playerObject.GetComponent<ZedPlayerAudioBridge>();
            if (bridge == null)
            {
                bridge = playerObject.AddComponent<ZedPlayerAudioBridge>();
            }

            bridge.AudioFeedback = feedback;

            ZedPlayerHealth health = playerObject.GetComponent<ZedPlayerHealth>();
            if (health != null)
            {
                health.AudioBridge = bridge;
                health.PlayHurtAudio = true;
                EditorUtility.SetDirty(health);
            }

            EditorUtility.SetDirty(feedback);
            EditorUtility.SetDirty(bridge);
            return true;
        }
    }
}

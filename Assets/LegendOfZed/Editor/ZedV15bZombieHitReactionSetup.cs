using System.IO;
using LegendOfZed.Enemies;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LegendOfZed.Editor
{
    public static class ZedV15bZombieHitReactionSetup
    {
        private const string ScenePath = "Assets/LegendOfZed/Scenes/Zed_Controller_Test.unity";

        [MenuItem("Legend of Zed/Setup/v1.5b Apply Stronger Zombie Hit Reaction")]
        public static void ApplyStrongerZombieHitReaction()
        {
            if (!File.Exists(ScenePath))
            {
                Debug.LogWarning("Could not find scene: " + ScenePath);
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            ZedPrototypeZombieEnemy[] zombies = Object.FindObjectsByType<ZedPrototypeZombieEnemy>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

            int count = 0;
            for (int i = 0; i < zombies.Length; i++)
            {
                ZedPrototypeZombieEnemy zombie = zombies[i];
                if (zombie == null)
                {
                    continue;
                }

                zombie.StaggerOnBulletHit = true;
                zombie.HitStaggerSeconds = 0.45f;

                ZedZombieHitReactionMotor motor = zombie.GetComponent<ZedZombieHitReactionMotor>();
                if (motor == null)
                {
                    motor = zombie.gameObject.AddComponent<ZedZombieHitReactionMotor>();
                }

                motor.EnableHitShove = true;
                motor.ShoveDistance = 0.55f;
                motor.ShoveSeconds = 0.14f;
                motor.ExtraStaggerSeconds = 0.45f;

                EditorUtility.SetDirty(zombie);
                EditorUtility.SetDirty(motor);
                count++;
            }

            ZedZombieSpawner[] spawners = Object.FindObjectsByType<ZedZombieSpawner>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

            for (int i = 0; i < spawners.Length; i++)
            {
                if (spawners[i] != null && spawners[i].ZombiePrototype != null)
                {
                    ZedZombieHitReactionMotor motor = spawners[i].ZombiePrototype.GetComponent<ZedZombieHitReactionMotor>();
                    if (motor == null)
                    {
                        motor = spawners[i].ZombiePrototype.AddComponent<ZedZombieHitReactionMotor>();
                    }

                    motor.EnableHitShove = true;
                    motor.ShoveDistance = 0.55f;
                    motor.ShoveSeconds = 0.14f;
                    motor.ExtraStaggerSeconds = 0.45f;
                    EditorUtility.SetDirty(motor);
                }
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("v1.5b stronger zombie hit reaction applied. Zombies updated=" + count + ". Player/weapons/bullets/ammo IDs were not changed.");
        }
    }
}

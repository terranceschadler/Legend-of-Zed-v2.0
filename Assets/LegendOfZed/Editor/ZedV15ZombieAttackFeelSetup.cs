using System.IO;
using LegendOfZed.Enemies;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LegendOfZed.Editor
{
    public static class ZedV15ZombieAttackFeelSetup
    {
        private const string ScenePath = "Assets/LegendOfZed/Scenes/Zed_Controller_Test.unity";

        [MenuItem("Legend of Zed/Setup/v1.5 Apply Zombie Attack Feel Defaults")]
        public static void ApplyZombieAttackFeelDefaults()
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

                zombie.AttackDamage = 10f;
                zombie.AttackCooldown = 1.45f;
                zombie.AttackWindupSeconds = 0.35f;
                zombie.AttackRangeGrace = 0.35f;
                zombie.RequireTargetInRangeAtHitMoment = true;

                zombie.StaggerOnBulletHit = true;
                zombie.HitStaggerSeconds = 0.16f;
                zombie.FaceAttackerOnHit = false;

                zombie.RagdollOnDeath = true;
                zombie.DestroyOnDeath = false;
                if (zombie.DeathDisableDelay < 10f)
                {
                    zombie.DeathDisableDelay = 999f;
                }

                EditorUtility.SetDirty(zombie);
                count++;
            }

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

                spawner.ZombieHealth = 60f;
                spawner.DeathDisableDelay = 999f;
                spawner.RagdollOnDeath = true;
                EditorUtility.SetDirty(spawner);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("v1.5 zombie attack feel defaults applied. Zombies updated=" + count + ". Player/weapons/bullets/ammo IDs were not changed.");
        }
    }
}

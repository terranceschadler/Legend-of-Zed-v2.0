using System.IO;
using LegendOfZed.Player;
using TopDownShooter;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LegendOfZed.Editor
{
    public static class ZedV14PlayerHealthSetup
    {
        private const string ScenePath = "Assets/LegendOfZed/Scenes/Zed_Controller_Test.unity";

        [MenuItem("Legend of Zed/Setup/v1.4 Add Player Health")]
        public static void AddPlayerHealth()
        {
            if (!File.Exists(ScenePath))
            {
                Debug.LogWarning("Could not find scene: " + ScenePath);
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            PlayerController playerController = Object.FindFirstObjectByType<PlayerController>();

            GameObject playerObject = null;
            if (playerController != null)
            {
                playerObject = playerController.gameObject;
            }
            else
            {
                GameObject taggedPlayer = GameObject.FindGameObjectWithTag("Player");
                if (taggedPlayer != null)
                {
                    playerObject = taggedPlayer;
                }
            }

            if (playerObject == null)
            {
                Debug.LogWarning("v1.4 player health setup could not find PlayerController or Player-tagged object.");
                return;
            }

            ZedPlayerHealth health = playerObject.GetComponent<ZedPlayerHealth>();
            if (health == null)
            {
                health = playerObject.AddComponent<ZedPlayerHealth>();
            }

            health.MaxHealth = 100f;
            health.CurrentHealth = 100f;
            health.IsDead = false;
            health.DamageInvulnerabilitySeconds = 0.65f;
            health.DisableMovementOnDeath = true;
            health.DisableShootingOnDeath = true;
            health.ShowDebugHud = true;
            health.ShowDamageFlash = true;
            health.DamageFlashSeconds = 0.22f;
            health.LogDamageForTesting = true;
            health.DeathMessage = "PLAYER DOWN";

            EditorUtility.SetDirty(playerObject);
            EditorUtility.SetDirty(health);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("v1.4 player health added to " + playerObject.name + ". Zombie attacks can now damage the player through ApplyDamage/TakeDamage/Damage. Player/weapons/bullets/ammo IDs were not changed.");
        }

        [MenuItem("Legend of Zed/Setup/v1.4 Print Player Damage Test Steps")]
        public static void PrintPlayerDamageTestSteps()
        {
            Debug.Log(
                "v1.4 Player Damage Test Steps:\\n" +
                "1. Run Legend of Zed/Setup/v1.4 Add Player Health.\\n" +
                "2. Press Play.\\n" +
                "3. Let a zombie reach the player.\\n" +
                "4. Confirm health decreases once per attack cooldown, not every frame.\\n" +
                "5. Confirm red damage flash appears.\\n" +
                "6. Confirm PLAYER DOWN appears at 0 health and movement/shooting stop.\\n" +
                "7. Confirm no TopDownShooter.HitPoint health bar errors.");
        }
    }
}

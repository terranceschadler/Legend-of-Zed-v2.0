using LegendOfZed.Enemies;
using LegendOfZed.Player;
using LegendOfZed.Runtime;
using TopDownShooter;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace LegendOfZed.EditorTools
{
    public static class ZedV310AF1ExistingPlayerEnemyRebindMenu
    {
        private const string ZombiePrefabPath = "Assets/LegendOfZed/Prefabs/Enemies/Zed_Zombie_Enemy_Baseline.prefab";
        private const string MenuRoot = "Legend Of Zed/v3.10AF1 Player Enemy Integration/";

        [MenuItem(MenuRoot + "Rebind Current Scene To Selected Or Existing Player")]
        public static void RebindCurrentScene()
        {
            PlayerController player = ResolveScenePlayer();
            if (player == null)
            {
                Debug.LogWarning(
                    "No safe scene PlayerController was resolved. Select the correct player object in the Hierarchy and run this command again. " +
                    "v3.10AF3 no longer creates or loads the TopDownShooter demo Player prefab automatically.");
                return;
            }

            Undo.RecordObject(player.gameObject, "Rebind existing player enemy integration");
            RebindPlayer(player.gameObject, true);

            ZedPrototypeZombieEnemy[] zombies = Object.FindObjectsByType<ZedPrototypeZombieEnemy>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < zombies.Length; i++)
            {
                if (zombies[i] == null)
                {
                    continue;
                }

                RebindZombie(zombies[i].gameObject, player.transform, true);
            }

            ZedZombieSpawner[] spawners = Object.FindObjectsByType<ZedZombieSpawner>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            GameObject zombiePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ZombiePrefabPath);

            for (int i = 0; i < spawners.Length; i++)
            {
                if (spawners[i] == null)
                {
                    continue;
                }

                Undo.RecordObject(spawners[i], "Rebind existing zombie spawner");
                spawners[i].PlayerTarget = player.transform;

                if (spawners[i].ZombiePrototype == null && zombiePrefab != null)
                {
                    spawners[i].ZombiePrototype = zombiePrefab;
                }

                EditorUtility.SetDirty(spawners[i]);
            }

            MarkSceneDirty();
            Debug.Log("v3.10AF3 scene rebind complete. Player=" + GetPath(player.transform) + ", Zombies=" + zombies.Length + ", Spawners=" + spawners.Length + ".");
        }

        [MenuItem(MenuRoot + "Rebind Selected Player Object Or Prefab")]
        public static void RebindSelectedPlayerObjectOrPrefab()
        {
            GameObject selected = Selection.activeGameObject;
            if (selected == null)
            {
                Debug.LogWarning("Select the correct player GameObject in the Hierarchy, or the correct player prefab asset in the Project window, then run this command again.");
                return;
            }

            string assetPath = AssetDatabase.GetAssetPath(selected);
            if (!string.IsNullOrEmpty(assetPath) && assetPath.EndsWith(".prefab", System.StringComparison.OrdinalIgnoreCase))
            {
                RebindPrefab(assetPath, RebindPlayerPrefabRoot);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log("v3.10AF3 selected player prefab rebind complete: " + assetPath);
                return;
            }

            PlayerController player = ResolvePlayerFromGameObject(selected);
            if (player == null)
            {
                Debug.LogWarning("Selected object is not the player and does not contain a PlayerController: " + selected.name, selected);
                return;
            }

            RebindPlayer(player.gameObject, true);
            MarkSceneDirty();
            Debug.Log("v3.10AF3 selected player scene object rebind complete: " + GetPath(player.transform), player);
        }

        [MenuItem(MenuRoot + "Rebind Existing Zombie Prefab")]
        public static void RebindExistingZombiePrefab()
        {
            bool changed = RebindPrefab(ZombiePrefabPath, RebindZombiePrefabRoot);
            if (changed)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            Debug.Log("v3.10AF3 zombie prefab rebind complete. Changed=" + changed + ".");
        }

        [MenuItem(MenuRoot + "Validate Current Scene Integration")]
        public static void ValidateCurrentScene()
        {
            PlayerController[] players = Object.FindObjectsByType<PlayerController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            PlayerController selectedPlayer = ResolvePlayerFromGameObject(Selection.activeGameObject);
            PlayerController resolvedPlayer = ResolveScenePlayer(false);
            ZedPlayerHealth playerHealth = resolvedPlayer != null ? resolvedPlayer.GetComponent<ZedPlayerHealth>() : null;
            ZedPrototypeZombieEnemy[] zombies = Object.FindObjectsByType<ZedPrototypeZombieEnemy>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            ZedZombieSpawner[] spawners = Object.FindObjectsByType<ZedZombieSpawner>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            int zombiesWithoutTarget = 0;
            int zombiesWithoutAnimator = 0;
            int zombiesWithoutRelay = 0;
            int spawnersWithoutTarget = 0;

            for (int i = 0; i < zombies.Length; i++)
            {
                if (zombies[i] == null)
                {
                    continue;
                }

                if (zombies[i].Target == null) zombiesWithoutTarget++;
                if (zombies[i].Animator == null && zombies[i].GetComponentInChildren<Animator>(true) == null) zombiesWithoutAnimator++;
                Animator animator = zombies[i].Animator != null ? zombies[i].Animator : zombies[i].GetComponentInChildren<Animator>(true);
                if (animator != null && animator.GetComponent<ZedZombieRootMotionRelay>() == null) zombiesWithoutRelay++;
            }

            for (int i = 0; i < spawners.Length; i++)
            {
                if (spawners[i] != null && spawners[i].PlayerTarget == null) spawnersWithoutTarget++;
            }

            string playerList = BuildPlayerList(players);
            string resolvedSource = resolvedPlayer != null ? GetPrefabSourcePath(resolvedPlayer.gameObject) : "";

            Debug.Log(
                "v3.10AF3 Integration Validation\n" +
                "Scene PlayerControllers found: " + players.Length + "\n" +
                "Selected player: " + (selectedPlayer != null ? GetPath(selectedPlayer.transform) : "none") + "\n" +
                "Resolved player: " + (resolvedPlayer != null ? GetPath(resolvedPlayer.transform) : "none") + "\n" +
                "Resolved player prefab source: " + (string.IsNullOrEmpty(resolvedSource) ? "scene object or unknown" : resolvedSource) + "\n" +
                "ZedPlayerHealth found on resolved player: " + (playerHealth != null) + "\n" +
                "Zombies found: " + zombies.Length + "\n" +
                "Zombies missing Target: " + zombiesWithoutTarget + "\n" +
                "Zombies missing Animator: " + zombiesWithoutAnimator + "\n" +
                "Zombies missing RootMotionRelay: " + zombiesWithoutRelay + "\n" +
                "Spawners found: " + spawners.Length + "\n" +
                "Spawners missing PlayerTarget: " + spawnersWithoutTarget + "\n" +
                "Player asset rule: selected or existing scene player only; no hard-coded demo player prefab.\n" +
                "Expected zombie prefab: " + ZombiePrefabPath + "\n" +
                playerList);
        }

        private static PlayerController ResolveScenePlayer(bool warnOnAmbiguous = true)
        {
            PlayerController selectedPlayer = ResolvePlayerFromGameObject(Selection.activeGameObject);
            if (selectedPlayer != null && selectedPlayer.gameObject.scene.IsValid())
            {
                return selectedPlayer;
            }

            GameObject taggedPlayer = FindTaggedPlayerSafely();
            PlayerController taggedController = ResolvePlayerFromGameObject(taggedPlayer);
            if (taggedController != null && taggedController.gameObject.scene.IsValid())
            {
                return taggedController;
            }

            PlayerController[] players = Object.FindObjectsByType<PlayerController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (players.Length == 1)
            {
                return players[0];
            }

            PlayerController namedZedPlayer = null;
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i] == null)
                {
                    continue;
                }

                string objectName = players[i].gameObject.name;
                if (objectName.IndexOf("Zed", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                    objectName.IndexOf("Gameplay", System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    if (namedZedPlayer != null)
                    {
                        namedZedPlayer = null;
                        break;
                    }

                    namedZedPlayer = players[i];
                }
            }

            if (namedZedPlayer != null)
            {
                return namedZedPlayer;
            }

            if (warnOnAmbiguous && players.Length > 1)
            {
                Debug.LogWarning("Multiple PlayerController objects were found. Select the correct player in the Hierarchy and run the rebind again.\n" + BuildPlayerList(players));
            }

            return null;
        }

        private static PlayerController ResolvePlayerFromGameObject(GameObject gameObject)
        {
            if (gameObject == null)
            {
                return null;
            }

            PlayerController player = gameObject.GetComponent<PlayerController>();
            if (player != null)
            {
                return player;
            }

            player = gameObject.GetComponentInParent<PlayerController>(true);
            if (player != null)
            {
                return player;
            }

            return gameObject.GetComponentInChildren<PlayerController>(true);
        }

        private static GameObject FindTaggedPlayerSafely()
        {
            try
            {
                return GameObject.FindGameObjectWithTag("Player");
            }
            catch (UnityException)
            {
                return null;
            }
        }

        private static bool RebindPrefab(string prefabPath, System.Action<GameObject> rebindAction)
        {
            GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefabAsset == null)
            {
                Debug.LogWarning("Prefab not found: " + prefabPath);
                return false;
            }

            GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                rebindAction(root);
                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
                Debug.Log("Rebound existing prefab: " + prefabPath);
                return true;
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void RebindPlayerPrefabRoot(GameObject root)
        {
            PlayerController player = ResolvePlayerFromGameObject(root);
            RebindPlayer(player != null ? player.gameObject : root, false);
        }

        private static void RebindZombiePrefabRoot(GameObject root)
        {
            RebindZombie(root, null, false);
        }

        private static void RebindPlayer(GameObject root, bool useUndo)
        {
            if (root == null) return;

            TrySetPlayerTag(root);

            PlayerController playerController = root.GetComponent<PlayerController>();
            if (playerController == null)
            {
                playerController = root.GetComponentInChildren<PlayerController>(true);
            }

            if (playerController == null)
            {
                Debug.LogWarning("Cannot rebind player because no PlayerController was found under: " + root.name, root);
                return;
            }

            GameObject playerRoot = playerController.gameObject;
            MovementCharacterController movement = playerRoot.GetComponent<MovementCharacterController>();
            ShooterController shooter = playerRoot.GetComponent<ShooterController>();
            SwimmingController swimming = playerRoot.GetComponent<SwimmingController>();

            if (useUndo) Undo.RecordObject(playerController, "Rebind existing player controller");

            if (playerController.MovCharController == null) playerController.MovCharController = movement;
            if (playerController.ShooterController == null) playerController.ShooterController = shooter;
            if (playerController.SwimmingController == null) playerController.SwimmingController = swimming;
            playerController.UseMouseToRotate = true;
            EditorUtility.SetDirty(playerController);

            if (movement != null)
            {
                if (useUndo) Undo.RecordObject(movement, "Rebind existing player movement");
                if (movement.PlayerController == null) movement.PlayerController = playerController;
                EditorUtility.SetDirty(movement);
            }

            if (shooter != null)
            {
                if (useUndo) Undo.RecordObject(shooter, "Rebind existing player shooter");
                if (shooter.PlayerController == null) shooter.PlayerController = playerController;
                EditorUtility.SetDirty(shooter);
            }

            ZedPlayerHealth health = playerRoot.GetComponent<ZedPlayerHealth>();
            if (health == null)
            {
                health = useUndo ? Undo.AddComponent<ZedPlayerHealth>(playerRoot) : playerRoot.AddComponent<ZedPlayerHealth>();
            }

            ZedPlayerAudioBridge audioBridge = playerRoot.GetComponent<ZedPlayerAudioBridge>();
            if (audioBridge == null)
            {
                audioBridge = useUndo ? Undo.AddComponent<ZedPlayerAudioBridge>(playerRoot) : playerRoot.AddComponent<ZedPlayerAudioBridge>();
            }

            if (useUndo) Undo.RecordObject(health, "Rebind existing player health");
            health.AudioBridge = audioBridge;
            EditorUtility.SetDirty(health);
            EditorUtility.SetDirty(audioBridge);
            EditorUtility.SetDirty(playerRoot);
        }

        private static void RebindZombie(GameObject root, Transform playerTarget, bool useUndo)
        {
            if (root == null) return;

            ZedPrototypeZombieEnemy zombie = root.GetComponent<ZedPrototypeZombieEnemy>();
            if (zombie == null)
            {
                zombie = useUndo ? Undo.AddComponent<ZedPrototypeZombieEnemy>(root) : root.AddComponent<ZedPrototypeZombieEnemy>();
            }

            Animator animator = root.GetComponentInChildren<Animator>(true);
            NavMeshAgent agent = root.GetComponent<NavMeshAgent>();

            if (useUndo) Undo.RecordObject(zombie, "Rebind existing zombie enemy");

            if (zombie.Animator == null) zombie.Animator = animator;
            if (zombie.NavMeshAgent == null) zombie.NavMeshAgent = agent;
            if (playerTarget != null) zombie.Target = playerTarget;

            if (agent != null)
            {
                if (useUndo) Undo.RecordObject(agent, "Configure zombie NavMeshAgent for path only");
                agent.updatePosition = false;
                agent.updateRotation = false;
                agent.stoppingDistance = zombie.StoppingDistance;
                EditorUtility.SetDirty(agent);
            }

            if (animator != null)
            {
                if (useUndo) Undo.RecordObject(animator, "Configure zombie animator root motion");
                animator.applyRootMotion = zombie.UseRootMotionLocomotion;

                ZedZombieRootMotionRelay relay = animator.GetComponent<ZedZombieRootMotionRelay>();
                if (relay == null)
                {
                    relay = useUndo ? Undo.AddComponent<ZedZombieRootMotionRelay>(animator.gameObject) : animator.gameObject.AddComponent<ZedZombieRootMotionRelay>();
                }

                if (useUndo) Undo.RecordObject(relay, "Rebind zombie root motion relay");
                relay.Owner = zombie;
                EditorUtility.SetDirty(relay);
                EditorUtility.SetDirty(animator);
            }

            ZedZombieHitReactionMotor hitReaction = root.GetComponent<ZedZombieHitReactionMotor>();
            if (hitReaction == null)
            {
                hitReaction = useUndo ? Undo.AddComponent<ZedZombieHitReactionMotor>(root) : root.AddComponent<ZedZombieHitReactionMotor>();
            }

            ZedAudioFeedback audioFeedback = root.GetComponent<ZedAudioFeedback>();
            if (audioFeedback == null)
            {
                audioFeedback = useUndo ? Undo.AddComponent<ZedAudioFeedback>(root) : root.AddComponent<ZedAudioFeedback>();
            }

            ZedZombieAudioBridge audioBridge = root.GetComponent<ZedZombieAudioBridge>();
            if (audioBridge == null)
            {
                audioBridge = useUndo ? Undo.AddComponent<ZedZombieAudioBridge>(root) : root.AddComponent<ZedZombieAudioBridge>();
            }

            if (useUndo) Undo.RecordObject(audioBridge, "Rebind zombie audio bridge");
            audioBridge.AudioFeedback = audioFeedback;

            EditorUtility.SetDirty(zombie);
            EditorUtility.SetDirty(hitReaction);
            EditorUtility.SetDirty(audioFeedback);
            EditorUtility.SetDirty(audioBridge);
            EditorUtility.SetDirty(root);
        }

        private static void TrySetPlayerTag(GameObject root)
        {
            if (root == null) return;

            try
            {
                root.tag = "Player";
            }
            catch (UnityException)
            {
                Debug.LogWarning("Could not set Player tag. Add a Player tag in Tags and Layers, then run the rebind again.", root);
            }
        }

        private static string BuildPlayerList(PlayerController[] players)
        {
            if (players == null || players.Length == 0)
            {
                return "Player list: none";
            }

            string result = "Player list:";
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i] == null)
                {
                    continue;
                }

                string prefabSource = GetPrefabSourcePath(players[i].gameObject);
                result += "\n- " + GetPath(players[i].transform) + (string.IsNullOrEmpty(prefabSource) ? "" : " | prefab source: " + prefabSource);
            }

            return result;
        }

        private static string GetPrefabSourcePath(GameObject gameObject)
        {
            if (gameObject == null)
            {
                return "";
            }

            GameObject source = PrefabUtility.GetCorrespondingObjectFromSource(gameObject);
            return source != null ? AssetDatabase.GetAssetPath(source) : "";
        }

        private static string GetPath(Transform transform)
        {
            if (transform == null)
            {
                return "";
            }

            string path = transform.name;
            while (transform.parent != null)
            {
                transform = transform.parent;
                path = transform.name + "/" + path;
            }

            return path;
        }

        private static void MarkSceneDirty()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            if (activeScene.IsValid())
            {
                EditorSceneManager.MarkSceneDirty(activeScene);
            }
        }
    }
}

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LegendOfZed.Editor
{
    public static class ZedV29bOverworldWeaponPickupTestPlacement
    {
        private const string OverworldScenePath = "Assets/LegendOfZed/Scenes/Zed_Overworld.unity";
        private const string WeaponDropFolder = "Assets/TopDownShooterController/Media/Prefabs/Items/WeaponDrop";
        private const string RootName = "Zed_Overworld_WeaponPickupTest";
        private const string RealPlayerName = "Zed_Real_Gameplay_Player";
        private const string SafePlayerName = "Zed_Portal_Test_Player";

        [MenuItem("Legend of Zed/Setup/Overworld/v2.9b Add Weapon Pickups Near Player")]
        public static void AddWeaponPickupsNearPlayer()
        {
            if (!File.Exists(OverworldScenePath))
            {
                Debug.LogWarning("v2.9b could not find Zed_Overworld.");
                return;
            }

            Scene overworldScene = EditorSceneManager.OpenScene(OverworldScenePath, OpenSceneMode.Single);

            GameObject root = GameObject.Find(RootName);
            if (root == null)
            {
                root = new GameObject(RootName);
                EditorSceneManager.MoveGameObjectToScene(root, overworldScene);
            }

            ClearChildren(root.transform);

            Transform targetPlayer = FindPlayerTransform();
            Vector3 center = targetPlayer != null ? targetPlayer.position : new Vector3(0f, 1f, -8f);

            List<GameObject> pickupPrefabs = LoadWeaponDropPrefabs();
            List<GameObject> pickupInstances = InstantiatePickups(pickupPrefabs);

            if (pickupInstances.Count == 0)
            {
                Debug.LogWarning("v2.9c found no weapon pickup prefabs in: " + WeaponDropFolder);
            }
            else
            {
                PlacePickups(root.transform, pickupInstances, center);
                Debug.Log("v2.9c placed " + pickupInstances.Count + " WeaponDrop pickup prefab(s) near the player from " + WeaponDropFolder + ".");
            }

            EditorUtility.SetDirty(root);
            EditorSceneManager.MarkSceneDirty(overworldScene);
            EditorSceneManager.SaveScene(overworldScene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [MenuItem("Legend of Zed/Setup/Overworld/v2.9b Validate Weapon Pickups")]
        public static void ValidateWeaponPickups()
        {
            int warnings = 0;

            if (!AssetDatabase.IsValidFolder(WeaponDropFolder))
            {
                Debug.LogWarning("v2.9c validation: WeaponDrop folder missing: " + WeaponDropFolder);
                warnings++;
            }

            GameObject root = GameObject.Find(RootName);
            if (root == null)
            {
                Debug.LogWarning("v2.9c validation: missing " + RootName + ".");
                warnings++;
            }
            else if (root.transform.childCount == 0)
            {
                Debug.LogWarning("v2.9c validation: weapon pickup test root has no children.");
                warnings++;
            }
            else
            {
                int pickupCandidates = 0;
                for (int i = 0; i < root.transform.childCount; i++)
                {
                    Transform child = root.transform.GetChild(i);
                    if (child == null)
                    {
                        continue;
                    }

                    if (LooksLikeWeaponDropPickup(child.gameObject))
                    {
                        pickupCandidates++;
                    }
                }

                if (pickupCandidates == 0)
                {
                    Debug.LogWarning("v2.9c validation: pickup objects exist, but none look like WeaponDrop pickups.");
                    warnings++;
                }
            }

            Transform player = FindPlayerTransform();
            if (player == null)
            {
                Debug.LogWarning("v2.9c validation: no real/safe player found near pickups.");
                warnings++;
            }

            if (warnings == 0)
            {
                Debug.Log("v2.9c WeaponDrop pickup validation passed.");
            }
            else
            {
                Debug.LogWarning("v2.9c WeaponDrop pickup validation finished with " + warnings + " warning(s).");
            }
        }

        [MenuItem("Legend of Zed/Setup/Overworld/v2.9b Clear Weapon Pickup Test")]
        public static void ClearWeaponPickupTest()
        {
            if (!File.Exists(OverworldScenePath))
            {
                Debug.LogWarning("v2.9b could not find Zed_Overworld.");
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(OverworldScenePath, OpenSceneMode.Single);

            GameObject root = GameObject.Find(RootName);
            if (root != null)
            {
                Object.DestroyImmediate(root);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("v2.9c cleared overworld WeaponDrop pickup test objects.");
        }

        private static List<GameObject> LoadWeaponDropPrefabs()
        {
            List<GameObject> results = new List<GameObject>();

            if (!AssetDatabase.IsValidFolder(WeaponDropFolder))
            {
                Debug.LogWarning("WeaponDrop folder does not exist: " + WeaponDropFolder);
                return results;
            }

            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { WeaponDropFolder });
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (string.IsNullOrEmpty(path))
                {
                    continue;
                }

                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null)
                {
                    continue;
                }

                results.Add(prefab);
            }

            results.Sort((a, b) => string.CompareOrdinal(a.name, b.name));
            return results;
        }

        private static List<GameObject> InstantiatePickups(List<GameObject> prefabs)
        {
            List<GameObject> instances = new List<GameObject>();

            for (int i = 0; i < prefabs.Count && instances.Count < 8; i++)
            {
                GameObject prefab = prefabs[i];
                if (prefab == null)
                {
                    continue;
                }

                GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                if (instance == null)
                {
                    instance = Object.Instantiate(prefab);
                }

                if (instance == null)
                {
                    continue;
                }

                instance.name = "OverworldWeaponDropTest_" + prefab.name;
                instance.SetActive(true);
                EnsurePickupCollider(instance);
                instances.Add(instance);
            }

            return instances;
        }

        private static bool LooksLikeWeaponDropPickup(GameObject go)
        {
            if (go == null)
            {
                return false;
            }

            string lowerName = go.name.ToLowerInvariant();

            if (lowerName.Contains("weapondrop") ||
                lowerName.Contains("weapon_drop") ||
                lowerName.Contains("weapon") ||
                lowerName.Contains("pickup"))
            {
                return true;
            }

            Component[] components = go.GetComponentsInChildren<Component>(true);
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] == null)
                {
                    continue;
                }

                string typeName = components[i].GetType().Name.ToLowerInvariant();
                if (typeName.Contains("weapondrop") ||
                    typeName.Contains("weaponpickup") ||
                    typeName.Contains("pickup"))
                {
                    return true;
                }
            }

            return false;
        }

        private static void PlacePickups(Transform root, List<GameObject> pickups, Vector3 center)
        {
            Vector3[] offsets =
            {
                new Vector3(-4f, 0.7f, -4f),
                new Vector3(-2f, 0.7f, -4f),
                new Vector3(0f, 0.7f, -4f),
                new Vector3(2f, 0.7f, -4f),
                new Vector3(4f, 0.7f, -4f),
                new Vector3(-3f, 0.7f, -7f),
                new Vector3(0f, 0.7f, -7f),
                new Vector3(3f, 0.7f, -7f)
            };

            for (int i = 0; i < pickups.Count; i++)
            {
                GameObject pickup = pickups[i];
                if (pickup == null)
                {
                    continue;
                }

                pickup.transform.SetParent(root, true);
                pickup.transform.position = center + offsets[i % offsets.Length];
                pickup.transform.rotation = Quaternion.Euler(0f, i * 30f, 0f);
                pickup.SetActive(true);
                EnsurePickupCollider(pickup);
                EditorUtility.SetDirty(pickup);
            }
        }

        private static void EnsurePickupCollider(GameObject pickup)
        {
            if (pickup == null)
            {
                return;
            }

            Collider existingCollider = pickup.GetComponentInChildren<Collider>(true);
            if (existingCollider != null)
            {
                existingCollider.enabled = true;
                EditorUtility.SetDirty(existingCollider);
                return;
            }

            BoxCollider collider = pickup.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.size = new Vector3(1.25f, 1.25f, 1.25f);
            collider.center = new Vector3(0f, 0.6f, 0f);
            EditorUtility.SetDirty(collider);
        }

        private static Transform FindPlayerTransform()
        {
            GameObject realPlayer = GameObject.Find(RealPlayerName);
            if (realPlayer != null && realPlayer.activeInHierarchy)
            {
                return realPlayer.transform;
            }

            GameObject safePlayer = GameObject.Find(SafePlayerName);
            if (safePlayer != null && safePlayer.activeInHierarchy)
            {
                return safePlayer.transform;
            }

            GameObject tagged = GameObject.FindGameObjectWithTag("Player");
            if (tagged != null)
            {
                return tagged.transform;
            }

            CharacterController controller = Object.FindAnyObjectByType<CharacterController>();
            return controller != null ? controller.transform : null;
        }

        private static void ClearChildren(Transform root)
        {
            for (int i = root.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(root.GetChild(i).gameObject);
            }
        }
    }
}

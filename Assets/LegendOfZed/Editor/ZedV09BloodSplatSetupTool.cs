using System.IO;
using TopDownShooter;
using UnityEditor;
using UnityEngine;

namespace LegendOfZed.Editor
{
    public static class ZedV09BloodSplatSetupTool
    {
        private const string BloodSplatAssetName = "BloodSplat_FX";
        private const string SearchRoot = "Assets";

        [MenuItem("Legend of Zed/Setup/v0.9 Assign BloodSplat Enemy Hit Feedback")]
        public static void AssignBloodSplatEnemyHitFeedback()
        {
            GameObject bloodSplatPrefab = FindExactPrefab(BloodSplatAssetName);
            if (bloodSplatPrefab == null)
            {
                Debug.LogWarning("v0.9 BloodSplat setup could not find prefab named " + BloodSplatAssetName + " under " + SearchRoot + ".");
                return;
            }

            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { SearchRoot });
            int updatedCount = 0;

            foreach (string guid in prefabGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null)
                {
                    continue;
                }

                Damage[] damageComponents = prefab.GetComponentsInChildren<Damage>(true);
                if (damageComponents == null || damageComponents.Length == 0)
                {
                    continue;
                }

                bool prefabChanged = false;
                foreach (Damage damage in damageComponents)
                {
                    if (damage != null && damage.EnemyBulletImpact != bloodSplatPrefab)
                    {
                        damage.EnemyBulletImpact = bloodSplatPrefab;
                        EditorUtility.SetDirty(damage);
                        prefabChanged = true;
                    }
                }

                if (prefabChanged)
                {
                    EditorUtility.SetDirty(prefab);
                    updatedCount++;
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("v0.9 assigned " + BloodSplatAssetName + " to EnemyBulletImpact on " + updatedCount + " projectile prefab(s). Damage, WeaponData, BulletId, ammo, and projectile IDs were not changed.");
        }

        private static GameObject FindExactPrefab(string prefabName)
        {
            string[] guids = AssetDatabase.FindAssets(prefabName + " t:Prefab", new[] { SearchRoot });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetFileNameWithoutExtension(path) != prefabName)
                {
                    continue;
                }

                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null)
                {
                    return prefab;
                }
            }

            return null;
        }
    }
}

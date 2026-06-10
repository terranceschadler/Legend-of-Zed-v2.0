#if UNITY_EDITOR
using System.Collections.Generic;
using LegendOfZed.LegacyMapGenerator;
using LegendOfZed.MapIntegration;
using UnityEditor;
using UnityEngine;

namespace LegendOfZed.Editor
{
    public static class ZedRoadFrontageStreetPropSpawnerMenu
    {
        [MenuItem("Legend of Zed/Map Integration/Add Road Frontage Street Prop Spawner")]
        public static void AddStreetPropSpawner()
        {
            ZedRoadFrontageStreetPropSpawner spawner = Object.FindAnyObjectByType<ZedRoadFrontageStreetPropSpawner>();
            if (spawner == null)
            {
                GameObject go = new GameObject("Zed_RoadFrontage_StreetPropSpawner");
                Undo.RegisterCreatedObjectUndo(go, "Create road frontage street prop spawner");
                spawner = go.AddComponent<ZedRoadFrontageStreetPropSpawner>();
            }

            ZedLegacyRandomMapGenerator generator = Object.FindAnyObjectByType<ZedLegacyRandomMapGenerator>();
            if (generator != null)
            {
                spawner.mapGenerator = generator;
            }

            GameObject generatedMapRoot = GameObject.Find("Generated_Map_Root");
            if (generatedMapRoot == null)
            {
                generatedMapRoot = GameObject.Find("Generated");
            }

            if (generatedMapRoot != null)
            {
                spawner.scanRoot = generatedMapRoot.transform;
            }

            PreserveOrAutoFill(ref spawner.streetLightPrefabs, "streetLightPrefabs", "street light", "streetlight", "lamp", "light");
            PreserveOrAutoFill(ref spawner.benchPrefabs, "benchPrefabs", "bench");
            PreserveOrAutoFill(ref spawner.trashCanPrefabs, "trashCanPrefabs", "trashcan", "trash can", "bin", "rubbish");
            PreserveOrAutoFill(ref spawner.hydrantPrefabs, "hydrantPrefabs", "hydrant");
            PreserveOrAutoFill(ref spawner.mailboxPrefabs, "mailboxPrefabs", "mailbox", "mail box");
            PreserveOrAutoFill(ref spawner.paperDebrisPrefabs, "paperDebrisPrefabs", "paper", "debris", "trash");
            PreserveOrAutoFill(ref spawner.dumpsterPrefabs, "dumpsterPrefabs", "dumpster", "skip", "sm prop skip", "prop skip", "container", "large bin");
            PreserveOrAutoFill(ref spawner.trashPilePrefabs, "trashPilePrefabs", "trash pile", "rubbish pile", "garbage", "trash heap", "debris", "paper", "trash bag", "rubbish");

            spawner.runOnStart = true;
            spawner.clearPreviousGeneratedProps = true;
            spawner.logDetails = true;
            spawner.drawGizmos = true;

            EditorUtility.SetDirty(spawner);
            Selection.activeGameObject = spawner.gameObject;
            EditorGUIUtility.PingObject(spawner.gameObject);

            Debug.Log(
                "Street prop spawner added. Prefabs found: lights=" + Count(spawner.streetLightPrefabs) +
                ", benches=" + Count(spawner.benchPrefabs) +
                ", trash=" + Count(spawner.trashCanPrefabs) +
                ", hydrants=" + Count(spawner.hydrantPrefabs) +
                ", mailboxes=" + Count(spawner.mailboxPrefabs) +
                ", paper=" + Count(spawner.paperDebrisPrefabs) +
                ", dumpsters=" + Count(spawner.dumpsterPrefabs) +
                ", trashPiles=" + Count(spawner.trashPilePrefabs) + ".",
                spawner);
        }

        private static void PreserveOrAutoFill(ref GameObject[] prefabs, string fieldName, params string[] terms)
        {
            if (prefabs != null && prefabs.Length > 0)
            {
                Debug.Log("Street prop spawner preserved tuned " + fieldName + " array. Count=" + prefabs.Length + ".");
                return;
            }

            prefabs = FindPrefabs(terms);
            Debug.Log("Street prop spawner auto-filled empty " + fieldName + " array. Count=" + Count(prefabs) + ".");
        }

        private static GameObject[] FindPrefabs(params string[] terms)
        {
            List<GameObject> results = new List<GameObject>();
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });

            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                string lower = path.ToLowerInvariant().Replace("_", " ").Replace("-", " ");

                bool match = false;
                for (int t = 0; t < terms.Length; t++)
                {
                    string term = terms[t];
                    if (!string.IsNullOrEmpty(term) && lower.Contains(term.ToLowerInvariant()))
                    {
                        match = true;
                        break;
                    }
                }

                if (!match)
                {
                    continue;
                }

                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null && !results.Contains(prefab))
                {
                    results.Add(prefab);
                }
            }

            return results.ToArray();
        }

        private static int Count(GameObject[] prefabs)
        {
            return prefabs == null ? 0 : prefabs.Length;
        }
    }
}
#endif

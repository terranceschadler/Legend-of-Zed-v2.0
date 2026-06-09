using UnityEngine;

namespace LegendOfZed.MapIntegration
{
    public class ZedParkTilePropSpawner : MonoBehaviour
    {
        [Header("Tree Spawns")]
        public GameObject[] treePrefabs;
        public string treeSpawnTag = "TreeSpawn";
        public Transform treeSpawnParent;
        public bool spawnOnStart = true;
        public bool destroyMarkersAfterSpawn = false;

        [Header("Randomization")]
        public bool randomYaw = true;
        public Vector2 uniformScaleRange = new Vector2(0.9f, 1.15f);
        public float yOffset = 0f;

        [Header("Debug")]
        public bool logDetails;

        private bool _spawned;

        private void Start()
        {
            if (spawnOnStart)
            {
                SpawnAll();
            }
        }

        [ContextMenu("Spawn Park Props")]
        public void SpawnAll()
        {
            if (_spawned)
            {
                return;
            }

            _spawned = true;

            Transform root = treeSpawnParent != null ? treeSpawnParent : transform;
            Transform[] children = root.GetComponentsInChildren<Transform>(true);

            int markerCount = 0;
            int spawnedCount = 0;

            for (int i = 0; i < children.Length; i++)
            {
                Transform marker = children[i];
                if (marker == null || !marker.CompareTag(treeSpawnTag))
                {
                    continue;
                }

                markerCount++;

                GameObject prefab = GetRandomValidTreePrefab();
                if (prefab == null)
                {
                    continue;
                }

                Vector3 position = marker.position + new Vector3(0f, yOffset, 0f);
                Quaternion rotation = marker.rotation;

                if (randomYaw)
                {
                    rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                }

                GameObject spawned = Instantiate(prefab, position, rotation, transform);
                spawned.name = prefab.name + "_ParkTree";

                float scale = Random.Range(uniformScaleRange.x, uniformScaleRange.y);
                spawned.transform.localScale = spawned.transform.localScale * scale;

                spawnedCount++;

                if (destroyMarkersAfterSpawn)
                {
                    Destroy(marker.gameObject);
                }
                else
                {
                    HideMarkerVisuals(marker);
                }
            }

            if (logDetails)
            {
                Debug.Log("Park tile prop spawner found " + markerCount + " TreeSpawn marker(s) and spawned " + spawnedCount + " tree(s).", this);
            }
        }

        private GameObject GetRandomValidTreePrefab()
        {
            if (treePrefabs == null || treePrefabs.Length == 0)
            {
                return null;
            }

            int safety = 32;
            while (safety-- > 0)
            {
                GameObject candidate = treePrefabs[Random.Range(0, treePrefabs.Length)];
                if (candidate != null)
                {
                    return candidate;
                }
            }

            for (int i = 0; i < treePrefabs.Length; i++)
            {
                if (treePrefabs[i] != null)
                {
                    return treePrefabs[i];
                }
            }

            return null;
        }

        private void HideMarkerVisuals(Transform marker)
        {
            Renderer[] renderers = marker.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].enabled = false;
            }
        }
    }
}

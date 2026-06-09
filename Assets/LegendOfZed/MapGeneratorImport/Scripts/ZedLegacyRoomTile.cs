using System.Collections.Generic;
using UnityEngine;

namespace LegendOfZed.LegacyMapGenerator
{
    public class ZedLegacyRoomTile : MonoBehaviour
    {
        public GameObject[] buildingSpawns;
        public List<GameObject> buildingPrefabs = new List<GameObject>();
        public GameObject[] gateWays;
        public GameObject floor;
        public ZedLegacyRandomMapGenerator _randomMapGenerator;
        public bool spawningBuildings = false;

        private void Awake()
        {
            GameObject generatorObject = FindGameObjectWithTagSafe("MapGenerator");
            if (generatorObject != null)
            {
                _randomMapGenerator = generatorObject.GetComponent<ZedLegacyRandomMapGenerator>();
            }

            InitWalkableFloorReference();
            SpawnBuildings();
        }

        private void InitWalkableFloorReference()
        {
            Collider[] colliders = gameObject.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++)
            {
                Collider col = colliders[i];
                if (col == null || col.gameObject == null)
                {
                    continue;
                }

                if (SafeCompareTag(col.gameObject, "Walkable"))
                {
                    floor = col.gameObject;
                    return;
                }
            }
        }

        private void SpawnBuildings()
        {
            if (buildingSpawns == null || buildingSpawns.Length == 0 || buildingPrefabs == null || buildingPrefabs.Count == 0)
            {
                return;
            }

            spawningBuildings = true;

            for (int i = 0; i < buildingSpawns.Length; i++)
            {
                GameObject spawn = buildingSpawns[i];
                if (spawn == null)
                {
                    continue;
                }

                GameObject prefab = buildingPrefabs[Random.Range(0, buildingPrefabs.Count)];
                if (prefab != null)
                {
                    Instantiate(prefab, spawn.transform);
                }
            }

            spawningBuildings = false;
        }

        private static bool SafeCompareTag(GameObject go, string tagName)
        {
            try
            {
                return go != null && go.CompareTag(tagName);
            }
            catch (UnityException)
            {
                return false;
            }
        }

        private static GameObject FindGameObjectWithTagSafe(string tagName)
        {
            try
            {
                return GameObject.FindGameObjectWithTag(tagName);
            }
            catch (UnityException)
            {
                return null;
            }
        }
    }
}

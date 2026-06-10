using System.Collections.Generic;
using LegendOfZed.MapIntegration;
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

        [Header("Building Lot Filler")]
        [Tooltip("Disabled by default. Tile-local lot filling is not the preferred path anymore; use post-generation city block filling.")]
        public bool useBuildingLotFiller = false;

        [Tooltip("If the lot filler cannot place anything, the old fixed marker building spawn system is used as fallback.")]
        public bool fallbackToFixedSpawnsIfLotFillerCannotFill = true;

        [Header("Park Tile Safety")]
        [Tooltip("When true, this room tile will not spawn random buildings.")]
        public bool disableBuildingSpawns = false;

        [Tooltip("Tiles whose root name starts with this value are treated as park tiles and will not spawn buildings.")]
        public string parkTileNamePrefix = "ParkTile";

        [Tooltip("Building spawn markers with this tag are treated as park/tree markers and will never spawn buildings.")]
        public string treeSpawnTag = "TreeSpawn";

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

        private bool ShouldSuppressBuildingSpawns()
        {
            if (ZedPostGenerationCityBlockBuildingFiller.SuppressLegacyTileBuildingSpawns)
            {
                return true;
            }

            if (disableBuildingSpawns)
            {
                return true;
            }

            if (!string.IsNullOrEmpty(parkTileNamePrefix) && gameObject.name.StartsWith(parkTileNamePrefix))
            {
                return true;
            }

            if (GetComponent<ZedParkTilePropSpawner>() != null)
            {
                return true;
            }

            if (HasAnyTaggedChild(treeSpawnTag))
            {
                return true;
            }

            if (buildingSpawns != null)
            {
                for (int i = 0; i < buildingSpawns.Length; i++)
                {
                    GameObject spawn = buildingSpawns[i];
                    if (spawn != null && SafeCompareTag(spawn, treeSpawnTag))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void SpawnBuildings()
        {
            if (ShouldSuppressBuildingSpawns())
            {
                return;
            }

            if (buildingSpawns == null || buildingSpawns.Length == 0 || buildingPrefabs == null || buildingPrefabs.Count == 0)
            {
                return;
            }

            if (useBuildingLotFiller)
            {
                ZedBuildingLotFiller existingFiller = GetComponent<ZedBuildingLotFiller>();
                if (existingFiller != null && existingFiller.Fill(this))
                {
                    return;
                }

                if (!fallbackToFixedSpawnsIfLotFillerCannotFill)
                {
                    return;
                }
            }

            SpawnBuildingsFromFixedMarkers();
        }

        private void SpawnBuildingsFromFixedMarkers()
        {
            spawningBuildings = true;

            for (int i = 0; i < buildingSpawns.Length; i++)
            {
                GameObject spawn = buildingSpawns[i];
                if (spawn == null)
                {
                    continue;
                }

                if (SafeCompareTag(spawn, treeSpawnTag))
                {
                    continue;
                }

                GameObject prefab = GetRandomValidBuildingPrefab();
                if (prefab != null)
                {
                    Instantiate(prefab, spawn.transform);
                }
            }

            spawningBuildings = false;
        }

        private GameObject GetRandomValidBuildingPrefab()
        {
            if (buildingPrefabs == null || buildingPrefabs.Count == 0)
            {
                return null;
            }

            int safety = 64;
            while (safety-- > 0)
            {
                GameObject candidate = buildingPrefabs[Random.Range(0, buildingPrefabs.Count)];
                if (candidate != null)
                {
                    return candidate;
                }
            }

            for (int i = 0; i < buildingPrefabs.Count; i++)
            {
                if (buildingPrefabs[i] != null)
                {
                    return buildingPrefabs[i];
                }
            }

            return null;
        }

        private bool HasAnyTaggedChild(string tagName)
        {
            if (string.IsNullOrEmpty(tagName))
            {
                return false;
            }

            Transform[] children = GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < children.Length; i++)
            {
                Transform child = children[i];
                if (child != null && child.gameObject != gameObject && SafeCompareTag(child.gameObject, tagName))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool SafeCompareTag(GameObject go, string tagName)
        {
            if (go == null || string.IsNullOrEmpty(tagName))
            {
                return false;
            }

            try
            {
                return go.CompareTag(tagName);
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

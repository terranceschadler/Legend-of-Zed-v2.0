using System.Collections.Generic;
using UnityEngine;

namespace LegendOfZed.LegacyMapGenerator
{
    public class ZedLegacyRandomMapGenerator : MonoBehaviour
    {
        [Header("Legacy Tile Prefabs")]
        public GameObject startingTile;
        public GameObject deadEndTile;
        public GameObject[] tilePrefabs;

        [Header("Runtime State")]
        public List<Transform> tileSpawns = new List<Transform>();
        public List<Vector3> tilePositions = new List<Vector3>();
        public bool bakingNavMeshCompleted = false;
        public int tileCount = 20;

        private int _tileCountTotal;
        private bool _initializing;
        private bool _spawningTiles;
        private bool _deadEndsCompleted;
        private bool _mapCompleted;

        private void Awake()
        {
            _tileCountTotal = tileCount;

            if (startingTile != null)
            {
                Instantiate(startingTile, transform.position, transform.rotation);
                AddTilePosition(transform);
            }
        }

        private void Start()
        {
            if (!_initializing)
            {
                InitTileSpawns();
            }
        }

        private void FixedUpdate()
        {
            if (!_initializing && !_spawningTiles && !_mapCompleted && !_deadEndsCompleted)
            {
                if (tileSpawns.Count == 0 && tileCount >= 1)
                {
                    InitTileSpawns();
                    return;
                }

                if (tileSpawns.Count > 0 && tileCount <= 0)
                {
                    for (int i = tileSpawns.Count - 1; i >= 0; i--)
                    {
                        if (tileSpawns.Count > 0)
                        {
                            SpawnTile(deadEndTile, tileSpawns[i]);
                        }
                    }

                    InitTileSpawns();
                    if (tileSpawns.Count == 0)
                    {
                        _deadEndsCompleted = true;
                    }
                }

                if (tileSpawns.Count > 0 && tileCount >= 1)
                {
                    for (int i = tileSpawns.Count - 1; i >= 0; i--)
                    {
                        if (tileCount < 1)
                        {
                            return;
                        }

                        SpawnTile(GetRandomTile(), tileSpawns[i]);
                    }
                }

                if (tileSpawns.Count == 0 && tileCount < 1)
                {
                    InitTileSpawns();
                    if (tileSpawns.Count == 0)
                    {
                        _mapCompleted = true;
                    }
                }
            }

            if (_mapCompleted && !bakingNavMeshCompleted)
            {
                MarkMapCompleteForProjectNavMesh();
            }
        }

        private void MarkMapCompleteForProjectNavMesh()
        {
            bakingNavMeshCompleted = true;
            Debug.Log("Legacy map tile generation completed. Use the current project NavMesh pass after tile placement.", this);
        }

        private void InitTileSpawns()
        {
            _initializing = true;
            tileSpawns.Clear();

            GameObject[] spawnPoints = FindGameObjectsWithTagSafe("TileSpawn");
            for (int i = 0; i < spawnPoints.Length; i++)
            {
                if (spawnPoints[i] != null && !tileSpawns.Contains(spawnPoints[i].transform))
                {
                    tileSpawns.Add(spawnPoints[i].transform);
                }
            }

            _initializing = false;
        }

        private GameObject GetRandomTile()
        {
            if (tilePrefabs == null || tilePrefabs.Length == 0)
            {
                return deadEndTile;
            }

            return tilePrefabs[Random.Range(0, tilePrefabs.Length)];
        }

        private void SpawnTile(GameObject prefab, Transform spawnPoint)
        {
            if (prefab == null || spawnPoint == null)
            {
                InitTileSpawns();
                return;
            }

            if (!tilePositions.Contains(spawnPoint.position))
            {
                _spawningTiles = true;
                tileSpawns.Remove(spawnPoint);
                tilePositions.Add(spawnPoint.position);
                Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
                tileCount--;
                _spawningTiles = false;
            }
        }

        private void AddTilePosition(Transform tile)
        {
            if (tile != null && !tilePositions.Contains(tile.position))
            {
                tilePositions.Add(tile.position);
            }
        }

        public void RegenerateMap()
        {
            GameObject[] allTiles = FindGameObjectsWithTagSafe("RoomTile");
            for (int i = 0; i < allTiles.Length; i++)
            {
                if (allTiles[i] != null)
                {
                    Destroy(allTiles[i]);
                }
            }

            tilePositions.Clear();
            tileSpawns.Clear();
            tileCount = _tileCountTotal;
            bakingNavMeshCompleted = false;
            _initializing = false;
            _mapCompleted = false;
            _spawningTiles = false;
            _deadEndsCompleted = false;

            if (startingTile != null)
            {
                Instantiate(startingTile, transform.position, transform.rotation);
                AddTilePosition(transform);
            }
        }

        private static GameObject[] FindGameObjectsWithTagSafe(string tagName)
        {
            try
            {
                return GameObject.FindGameObjectsWithTag(tagName);
            }
            catch (UnityException)
            {
                Debug.LogWarning("Missing required legacy map generator tag: " + tagName + ". Run the v3.0 Map Tile Import setup to create tags.");
                return new GameObject[0];
            }
        }
    }
}

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

        [Header("Generation")]
        public int tileCount = 22;
        public float tileGridSize = 20f;
        public bool bakingNavMeshCompleted;

        [Header("Runtime State")]
        public List<Transform> tileSpawns = new List<Transform>();
        public List<Vector3> tilePositions = new List<Vector3>();

        private readonly HashSet<Vector2Int> _occupiedCells = new HashSet<Vector2Int>();
        private bool _completed;

        private void Start()
        {
            ResetRuntimeState();

            if (startingTile != null)
            {
                SpawnTile(startingTile, transform, false);
            }
            else
            {
                Debug.LogWarning("Legacy map generator has no starting tile.", this);
            }
        }

        private void FixedUpdate()
        {
            if (_completed)
            {
                return;
            }

            RefreshTileSpawns();

            if (tileSpawns.Count == 0)
            {
                MarkMapCompleteForProjectNavMesh();
                return;
            }

            Transform spawnPoint = GetNextValidSpawnPoint();
            if (spawnPoint == null)
            {
                MarkMapCompleteForProjectNavMesh();
                return;
            }

            GameObject prefabToSpawn = tileCount > 0 ? GetRandomTilePrefab() : deadEndTile;
            bool consumesBudget = tileCount > 0;

            if (prefabToSpawn == null)
            {
                Debug.LogWarning("Legacy map generator skipped a tile spawn because the prefab reference is missing.", this);
                RemoveSpawnPointCluster(spawnPoint.position);
                return;
            }

            SpawnTile(prefabToSpawn, spawnPoint, consumesBudget);
        }

        private void ResetRuntimeState()
        {
            _completed = false;
            bakingNavMeshCompleted = false;
            tileSpawns.Clear();
            tilePositions.Clear();
            _occupiedCells.Clear();
        }

        private void RefreshTileSpawns()
        {
            tileSpawns.Clear();

            GameObject[] spawns = GameObject.FindGameObjectsWithTag("TileSpawn");
            for (int i = 0; i < spawns.Length; i++)
            {
                GameObject spawn = spawns[i];
                if (spawn == null)
                {
                    continue;
                }

                Vector2Int cell = WorldToCell(spawn.transform.position);
                if (_occupiedCells.Contains(cell))
                {
                    Destroy(spawn);
                    continue;
                }

                tileSpawns.Add(spawn.transform);
            }
        }

        private Transform GetNextValidSpawnPoint()
        {
            while (tileSpawns.Count > 0)
            {
                Transform spawnPoint = tileSpawns[0];
                tileSpawns.RemoveAt(0);

                if (spawnPoint == null)
                {
                    continue;
                }

                Vector2Int cell = WorldToCell(spawnPoint.position);
                if (_occupiedCells.Contains(cell))
                {
                    Destroy(spawnPoint.gameObject);
                    continue;
                }

                return spawnPoint;
            }

            return null;
        }

        private GameObject GetRandomTilePrefab()
        {
            if (tilePrefabs == null || tilePrefabs.Length == 0)
            {
                return null;
            }

            List<GameObject> validPrefabs = new List<GameObject>();
            for (int i = 0; i < tilePrefabs.Length; i++)
            {
                if (tilePrefabs[i] != null)
                {
                    validPrefabs.Add(tilePrefabs[i]);
                }
            }

            if (validPrefabs.Count == 0)
            {
                return null;
            }

            return validPrefabs[Random.Range(0, validPrefabs.Count)];
        }

        private void SpawnTile(GameObject prefab, Transform spawnPoint, bool consumesBudget)
        {
            if (prefab == null || spawnPoint == null)
            {
                return;
            }

            Vector3 spawnPosition = spawnPoint.position;
            Vector2Int cell = WorldToCell(spawnPosition);

            if (_occupiedCells.Contains(cell))
            {
                RemoveSpawnPointCluster(spawnPosition);
                return;
            }

            _occupiedCells.Add(cell);
            tilePositions.Add(spawnPosition);

            // Important:
            // Occupancy is grid/cell based, but placement must use the original
            // TileSpawn transform position and rotation. Snapping the placement
            // position creates visible gaps because the imported prefabs already
            // contain their own correct offsets.
            Instantiate(prefab, spawnPosition, spawnPoint.rotation);

            if (consumesBudget)
            {
                tileCount = Mathf.Max(0, tileCount - 1);
            }

            RemoveSpawnPointCluster(spawnPosition);
        }

        private void RemoveSpawnPointCluster(Vector3 worldPosition)
        {
            Vector2Int targetCell = WorldToCell(worldPosition);

            GameObject[] spawns = GameObject.FindGameObjectsWithTag("TileSpawn");
            for (int i = 0; i < spawns.Length; i++)
            {
                GameObject spawn = spawns[i];
                if (spawn == null)
                {
                    continue;
                }

                if (WorldToCell(spawn.transform.position) == targetCell)
                {
                    Destroy(spawn);
                }
            }

            for (int i = tileSpawns.Count - 1; i >= 0; i--)
            {
                Transform spawn = tileSpawns[i];
                if (spawn == null || WorldToCell(spawn.position) == targetCell)
                {
                    tileSpawns.RemoveAt(i);
                }
            }
        }

        private Vector2Int WorldToCell(Vector3 position)
        {
            float grid = Mathf.Max(0.01f, tileGridSize);
            return new Vector2Int(
                Mathf.RoundToInt(position.x / grid),
                Mathf.RoundToInt(position.z / grid));
        }

        private void MarkMapCompleteForProjectNavMesh()
        {
            if (_completed)
            {
                return;
            }

            _completed = true;
            bakingNavMeshCompleted = true;
            Debug.Log("Legacy map tile generation completed. Use the current project NavMesh pass after tile placement.", this);
        }
    }
}

using System.Collections;
using LegendOfZed.LegacyMapGenerator;
using UnityEngine;

namespace LegendOfZed.MapIntegration
{
    /// <summary>
    /// Small bridge between the imported legacy map tile generator and the current top-down controller scene.
    /// It does not modify the legacy generator. It waits for generation to finish, finds a safe spawn point,
    /// places the player, and aims the camera at the playable map center.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ZedMapTileGeneratorRuntimeBridge : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ZedLegacyRandomMapGenerator mapGenerator;
        [SerializeField] private Transform player;
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private Camera gameplayCamera;

        [Header("Spawn")]
        [SerializeField] private float spawnHeight = 0.25f;
        [SerializeField] private float groundRayHeight = 12f;
        [SerializeField] private LayerMask groundMask = ~0;

        [Header("Camera")]
        [SerializeField] private Vector3 cameraOffset = new Vector3(0f, 18f, -14f);
        [SerializeField] private Vector3 cameraEuler = new Vector3(55f, 0f, 0f);
        [SerializeField] private bool positionCameraOnStart = true;

        [Header("Generated Tile Cleanup")]
        [SerializeField] private string generatedParentName = "Generated_Map_Tiles";
        [SerializeField] private bool parentGeneratedTiles = true;

        private bool integrated;

        private IEnumerator Start()
        {
            ResolveReferences();

            if (mapGenerator == null)
            {
                Debug.LogWarning("Map tile bridge could not find a ZedLegacyRandomMapGenerator.", this);
                yield break;
            }

            while (!mapGenerator.bakingNavMeshCompleted)
            {
                yield return null;
            }

            IntegrateGeneratedMap();
        }

        [ContextMenu("Integrate Generated Map Now")]
        public void IntegrateGeneratedMap()
        {
            if (integrated)
            {
                return;
            }

            ResolveReferences();

            Vector3 spawnPosition = FindSpawnPosition();
            PlacePlayer(spawnPosition);

            if (parentGeneratedTiles)
            {
                ParentGeneratedTiles();
            }

            if (positionCameraOnStart)
            {
                PositionCamera(spawnPosition);
            }

            integrated = true;
            Debug.Log("Map tile generator bridge integrated generated map.", this);
        }

        private void ResolveReferences()
        {
            if (mapGenerator == null)
            {
                mapGenerator = FindObjectOfType<ZedLegacyRandomMapGenerator>();
            }

            if (player == null)
            {
                GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
                if (playerObject != null)
                {
                    player = playerObject.transform;
                }
            }

            if (gameplayCamera == null)
            {
                gameplayCamera = Camera.main;
            }
        }

        private Vector3 FindSpawnPosition()
        {
            if (mapGenerator != null && mapGenerator.startingTile != null)
            {
                GameObject startingTileInstance = GameObject.Find(mapGenerator.startingTile.name + "(Clone)");
                if (startingTileInstance != null)
                {
                    return ProjectToGround(startingTileInstance.transform.position);
                }
            }

            ZedLegacyRoomTile firstTile = FindObjectOfType<ZedLegacyRoomTile>();
            if (firstTile != null)
            {
                return ProjectToGround(firstTile.transform.position);
            }

            return ProjectToGround(Vector3.zero);
        }

        private Vector3 ProjectToGround(Vector3 origin)
        {
            Vector3 rayStart = origin + Vector3.up * groundRayHeight;
            if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, groundRayHeight * 2f, groundMask, QueryTriggerInteraction.Ignore))
            {
                return hit.point + Vector3.up * spawnHeight;
            }

            origin.y = spawnHeight;
            return origin;
        }

        private void PlacePlayer(Vector3 spawnPosition)
        {
            if (player == null && playerPrefab != null)
            {
                GameObject spawnedPlayer = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
                player = spawnedPlayer.transform;
            }

            if (player == null)
            {
                Debug.LogWarning("Map tile bridge has no player or playerPrefab to place.", this);
                return;
            }

            player.position = spawnPosition;
            player.rotation = Quaternion.identity;
        }

        private void PositionCamera(Vector3 focusPosition)
        {
            if (gameplayCamera == null)
            {
                return;
            }

            gameplayCamera.transform.position = focusPosition + cameraOffset;
            gameplayCamera.transform.rotation = Quaternion.Euler(cameraEuler);
        }

        private void ParentGeneratedTiles()
        {
            GameObject parent = GameObject.Find(generatedParentName);
            if (parent == null)
            {
                parent = new GameObject(generatedParentName);
            }

            ZedLegacyRoomTile[] tiles = FindObjectsOfType<ZedLegacyRoomTile>();
            for (int i = 0; i < tiles.Length; i++)
            {
                if (tiles[i] != null && tiles[i].transform.parent == null)
                {
                    tiles[i].transform.SetParent(parent.transform, true);
                }
            }
        }
    }
}

using System.Collections;
using LegendOfZed.LegacyMapGenerator;
using LegendOfZed.Overworld;
using TopDownShooter;
using UnityEngine;

namespace LegendOfZed.MapIntegration
{
    /// <summary>
    /// Small bridge between the imported legacy map tile generator and the current top-down controller scene.
    /// It waits for generation to finish, finds the generated start tile, places/spawns the player there, and binds the top-down follow camera.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ZedMapTileGeneratorRuntimeBridge : MonoBehaviour
    {
        private static readonly string[] StartTileSpawnMarkerNames =
        {
            "PlayerSpawn",
            "Player Spawn",
            "PlayerStart",
            "Player Start",
            "StartSpawn",
            "Start Spawn",
            "StartTilePlayerSpawn",
            "SpawnPoint",
            "Spawn Point"
        };

        [Header("References")]
        [SerializeField] private ZedLegacyRandomMapGenerator mapGenerator;
        [SerializeField] private Transform player;
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private Camera gameplayCamera;

        [Header("Player Spawn Prep")]
        [SerializeField] private string playerTag = "Player";
        [SerializeField] private bool autoFindTaggedPlayer = true;
        [SerializeField] private bool spawnPlayerPrefabIfNoPlayer = true;
        [SerializeField] private string spawnedPlayerName = "Zed_Runtime_Player";
        [SerializeField] private bool disableCharacterControllerDuringWarp = true;

        [Header("Start Tile Spawn")]
        [SerializeField] private bool preferGeneratedStartTile = true;
        [SerializeField] private Vector3 startTileLocalSpawnOffset = Vector3.zero;
        [SerializeField] private bool facePlayerAlongStartTileForward = true;

        [Header("Spawn")]
        [SerializeField] private float spawnHeight = 0.25f;
        [SerializeField] private float groundRayHeight = 12f;
        [SerializeField] private LayerMask groundMask = ~0;

        [Header("Camera")]
        [SerializeField] private Vector3 cameraOffset = new Vector3(0f, 18f, -12f);
        [SerializeField] private Vector3 cameraEuler = new Vector3(58f, 0f, 0f);
        [SerializeField] private bool positionCameraOnStart = true;
        [SerializeField] private bool bindTopDownFollowCamera = true;
        [SerializeField] private bool addTopDownCameraIfMissing = true;
        [SerializeField] private float cameraFieldOfView = 45f;
        [SerializeField] private float cameraLookAtHeight = 1.2f;
        [SerializeField] private float cameraFollowSmoothing = 8f;

        [Header("Generated Tile Cleanup")]
        [SerializeField] private string generatedParentName = "Generated_Map_Tiles";
        [SerializeField] private bool parentGeneratedTiles = true;

        [Header("Runtime NavMesh")]
        [SerializeField] private bool rebuildRuntimeNavMeshAfterMapIntegration = true;
        [SerializeField] private bool createRuntimeNavMeshBuilderIfMissing = true;
        [SerializeField] private int runtimeNavMeshRebuildDelayFrames = 5;
        [SerializeField] private float runtimeNavMeshRebuildDelaySeconds = 2.0f;
        [SerializeField] private Vector3 runtimeNavMeshMinimumBoundsSize = new Vector3(230f, 24f, 230f);
        [SerializeField] private float runtimeNavMeshTileBoundsPadding = 40f;
        [SerializeField] private bool runtimeNavMeshIncludeAllSceneMeshes = false;
        [SerializeField] private bool runtimeNavMeshUseColliderBoundsOnly = true;
        [SerializeField] private bool runtimeNavMeshLogBuild = false;

        private bool integrated;
        private Quaternion resolvedSpawnRotation = Quaternion.identity;
        private Coroutine runtimeNavMeshRebuildRoutine;

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

            ConfigureGameplayCamera(player != null ? player.position : spawnPosition);
            QueueRuntimeNavMeshRebuild();

            integrated = true;
            Debug.Log("Map tile generator bridge integrated generated map. PlayerSpawn=" + spawnPosition + ".", this);
        }

        private void ResolveReferences()
        {
            if (mapGenerator == null)
            {
                mapGenerator = FindAnyObjectByType<ZedLegacyRandomMapGenerator>();
            }

            if (player == null && autoFindTaggedPlayer)
            {
                GameObject playerObject = FindGameObjectWithTagSafe(playerTag);
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
            resolvedSpawnRotation = Quaternion.identity;

            if (preferGeneratedStartTile)
            {
                Transform startTileTransform = FindGeneratedStartTileTransform();
                if (startTileTransform != null)
                {
                    Transform marker = FindSpawnMarker(startTileTransform);
                    if (marker != null)
                    {
                        resolvedSpawnRotation = facePlayerAlongStartTileForward ? FlattenRotation(marker.rotation) : Quaternion.identity;
                        return ProjectToGround(marker.position);
                    }

                    resolvedSpawnRotation = facePlayerAlongStartTileForward ? FlattenRotation(startTileTransform.rotation) : Quaternion.identity;
                    return ProjectToGround(startTileTransform.TransformPoint(startTileLocalSpawnOffset));
                }
            }

            if (mapGenerator != null && mapGenerator.tilePositions != null && mapGenerator.tilePositions.Count > 0)
            {
                return ProjectToGround(mapGenerator.tilePositions[0] + startTileLocalSpawnOffset);
            }

            ZedLegacyRoomTile firstTile = FindAnyObjectByType<ZedLegacyRoomTile>();
            if (firstTile != null)
            {
                resolvedSpawnRotation = facePlayerAlongStartTileForward ? FlattenRotation(firstTile.transform.rotation) : Quaternion.identity;
                return ProjectToGround(firstTile.transform.position + startTileLocalSpawnOffset);
            }

            return ProjectToGround(Vector3.zero);
        }

        private Transform FindGeneratedStartTileTransform()
        {
            if (mapGenerator == null || mapGenerator.startingTile == null)
            {
                return null;
            }

            string prefabName = mapGenerator.startingTile.name;
            string cloneName = prefabName + "(Clone)";

            GameObject activeClone = GameObject.Find(cloneName);
            if (activeClone != null)
            {
                return activeClone.transform;
            }

            Transform[] transforms = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            Transform best = null;
            float bestDistance = float.PositiveInfinity;
            Vector3 expectedPosition = mapGenerator.tilePositions != null && mapGenerator.tilePositions.Count > 0
                ? mapGenerator.tilePositions[0]
                : mapGenerator.transform.position;

            for (int i = 0; i < transforms.Length; i++)
            {
                Transform candidate = transforms[i];
                if (candidate == null)
                {
                    continue;
                }

                string candidateName = candidate.name;
                bool nameMatches = candidateName == cloneName ||
                                   candidateName == prefabName ||
                                   candidateName.StartsWith(prefabName + "(", System.StringComparison.Ordinal);

                if (!nameMatches)
                {
                    continue;
                }

                float distance = Vector3.SqrMagnitude(candidate.position - expectedPosition);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = candidate;
                }
            }

            return best;
        }

        private static Transform FindSpawnMarker(Transform startTileTransform)
        {
            if (startTileTransform == null)
            {
                return null;
            }

            Transform[] children = startTileTransform.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < children.Length; i++)
            {
                Transform child = children[i];
                if (child == null || child == startTileTransform)
                {
                    continue;
                }

                for (int nameIndex = 0; nameIndex < StartTileSpawnMarkerNames.Length; nameIndex++)
                {
                    if (child.name.Equals(StartTileSpawnMarkerNames[nameIndex], System.StringComparison.OrdinalIgnoreCase))
                    {
                        return child;
                    }
                }
            }

            return null;
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
            if (player == null && playerPrefab != null && spawnPlayerPrefabIfNoPlayer)
            {
                GameObject spawnedPlayer = Instantiate(playerPrefab, spawnPosition, resolvedSpawnRotation);
                spawnedPlayer.name = spawnedPlayerName;

                if (!string.IsNullOrEmpty(playerTag) && TagExists(playerTag))
                {
                    spawnedPlayer.tag = playerTag;
                }

                player = spawnedPlayer.transform;
            }

            if (player == null)
            {
                Debug.LogWarning("Map tile bridge has no player or playerPrefab assigned yet. Assign the real Legend of Zed player object/prefab to the bridge; do not use the imported demo player prefab.", this);
                return;
            }

            CharacterController characterController = disableCharacterControllerDuringWarp ? player.GetComponent<CharacterController>() : null;
            bool restoreController = characterController != null && characterController.enabled;
            if (restoreController)
            {
                characterController.enabled = false;
            }

            player.SetPositionAndRotation(spawnPosition, resolvedSpawnRotation);

            if (!string.IsNullOrEmpty(playerTag) && TagExists(playerTag) && player.CompareTag("Untagged"))
            {
                player.tag = playerTag;
            }

            if (restoreController)
            {
                characterController.enabled = true;
            }
        }

        private static Quaternion FlattenRotation(Quaternion rotation)
        {
            Vector3 forward = rotation * Vector3.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude <= 0.0001f)
            {
                return Quaternion.identity;
            }

            return Quaternion.LookRotation(forward.normalized, Vector3.up);
        }

        private void ConfigureGameplayCamera(Vector3 focusPosition)
        {
            if (gameplayCamera == null)
            {
                return;
            }

            TopDownCamera topDownCamera = gameplayCamera.GetComponent<TopDownCamera>();
            if (topDownCamera == null && bindTopDownFollowCamera && addTopDownCameraIfMissing)
            {
                topDownCamera = gameplayCamera.gameObject.AddComponent<TopDownCamera>();
            }

            if (topDownCamera != null && bindTopDownFollowCamera)
            {
                Transform target = player != null ? player : FindGameObjectWithTagSafe(playerTag)?.transform;
                if (target != null)
                {
                    topDownCamera.Target = target;
                    topDownCamera.PlayerTag = playerTag;
                    topDownCamera.AutoFindPlayer = true;
                    topDownCamera.ForcePerspective = true;
                    topDownCamera.FieldOfView = cameraFieldOfView;
                    topDownCamera.Height = Mathf.Max(2f, cameraOffset.y);
                    topDownCamera.Distance = Mathf.Max(0f, Mathf.Abs(cameraOffset.z));
                    topDownCamera.LookAtHeight = cameraLookAtHeight;
                    topDownCamera.Smoothing = cameraFollowSmoothing;
                    topDownCamera.RotationSmoothing = Mathf.Max(4f, cameraFollowSmoothing);
                    topDownCamera.SeeForward = 0f;
                    topDownCamera.AllowRotationInput = false;
                    topDownCamera.Yaw = cameraEuler.y;

                    topDownCamera.SnapToTarget();
                    return;
                }
            }

            if (positionCameraOnStart)
            {
                PositionCameraOnce(focusPosition);
            }
        }

        private void PositionCameraOnce(Vector3 focusPosition)
        {
            if (gameplayCamera == null)
            {
                return;
            }

            gameplayCamera.orthographic = false;
            gameplayCamera.fieldOfView = cameraFieldOfView;
            gameplayCamera.transform.position = focusPosition + cameraOffset;
            gameplayCamera.transform.rotation = Quaternion.Euler(cameraEuler);
        }


        private void QueueRuntimeNavMeshRebuild()
        {
            if (!rebuildRuntimeNavMeshAfterMapIntegration)
            {
                return;
            }

            if (runtimeNavMeshRebuildRoutine != null)
            {
                StopCoroutine(runtimeNavMeshRebuildRoutine);
                runtimeNavMeshRebuildRoutine = null;
            }

            if (Application.isPlaying)
            {
                runtimeNavMeshRebuildRoutine = StartCoroutine(RebuildRuntimeNavMeshAfterGeneratedSceneSettles());
            }
            else
            {
                RebuildRuntimeNavMeshNow();
            }
        }

        private IEnumerator RebuildRuntimeNavMeshAfterGeneratedSceneSettles()
        {
            int frames = Mathf.Max(0, runtimeNavMeshRebuildDelayFrames);
            for (int i = 0; i < frames; i++)
            {
                yield return null;
            }

            float delaySeconds = Mathf.Max(0f, runtimeNavMeshRebuildDelaySeconds);
            if (delaySeconds > 0f)
            {
                yield return new WaitForSeconds(delaySeconds);
            }

            RebuildRuntimeNavMeshNow();
            runtimeNavMeshRebuildRoutine = null;
        }

        [ContextMenu("Rebuild Runtime NavMesh Now")]
        public void RebuildRuntimeNavMeshNow()
        {
            ZedOverworldRuntimeNavMeshBuilder builder = FindAnyObjectByType<ZedOverworldRuntimeNavMeshBuilder>();
            if (builder == null && createRuntimeNavMeshBuilderIfMissing)
            {
                GameObject builderObject = new GameObject("Zed_Runtime_NavMesh_Builder");
                builder = builderObject.AddComponent<ZedOverworldRuntimeNavMeshBuilder>();
            }

            if (builder == null)
            {
                Debug.LogWarning("Map tile bridge could not rebuild the runtime NavMesh because no ZedOverworldRuntimeNavMeshBuilder exists.", this);
                return;
            }

            Bounds navBounds = ResolveRuntimeNavMeshBounds();
            builder.BuildOnStart = false;
            builder.BoundsCenter = navBounds.center;
            builder.BoundsSize = navBounds.size;
            builder.IncludeAllSceneMeshes = runtimeNavMeshIncludeAllSceneMeshes;
            builder.UseColliderBoundsOnly = runtimeNavMeshUseColliderBoundsOnly;
            builder.LogBuild = runtimeNavMeshLogBuild;
            builder.BuildNow();
        }

        private Bounds ResolveRuntimeNavMeshBounds()
        {
            bool hasBounds = false;
            Bounds bounds = new Bounds(Vector3.zero, runtimeNavMeshMinimumBoundsSize);

            ZedLegacyRoomTile[] tiles = FindObjectsByType<ZedLegacyRoomTile>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            for (int i = 0; i < tiles.Length; i++)
            {
                ZedLegacyRoomTile tile = tiles[i];
                if (tile == null)
                {
                    continue;
                }

                if (!hasBounds)
                {
                    bounds = new Bounds(tile.transform.position, Vector3.zero);
                    hasBounds = true;
                }
                else
                {
                    bounds.Encapsulate(tile.transform.position);
                }
            }

            if (!hasBounds && mapGenerator != null && mapGenerator.tilePositions != null)
            {
                for (int i = 0; i < mapGenerator.tilePositions.Count; i++)
                {
                    Vector3 tilePosition = mapGenerator.tilePositions[i];
                    if (!hasBounds)
                    {
                        bounds = new Bounds(tilePosition, Vector3.zero);
                        hasBounds = true;
                    }
                    else
                    {
                        bounds.Encapsulate(tilePosition);
                    }
                }
            }

            if (hasBounds)
            {
                float padding = Mathf.Max(0f, runtimeNavMeshTileBoundsPadding);
                Vector3 size = bounds.size + new Vector3(padding * 2f, 0f, padding * 2f);
                Vector3 minimum = runtimeNavMeshMinimumBoundsSize;
                size.x = Mathf.Max(size.x, minimum.x);
                size.y = Mathf.Max(minimum.y, 8f);
                size.z = Mathf.Max(size.z, minimum.z);

                Vector3 center = bounds.center;
                center.y = 0f;
                return new Bounds(center, size);
            }

            return new Bounds(Vector3.zero, runtimeNavMeshMinimumBoundsSize);
        }

        private void ParentGeneratedTiles()
        {
            GameObject parent = GameObject.Find(generatedParentName);
            if (parent == null)
            {
                parent = new GameObject(generatedParentName);
            }

            ZedLegacyRoomTile[] tiles = FindObjectsByType<ZedLegacyRoomTile>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            for (int i = 0; i < tiles.Length; i++)
            {
                if (tiles[i] != null && tiles[i].transform.parent == null)
                {
                    tiles[i].transform.SetParent(parent.transform, true);
                }
            }
        }

        private static GameObject FindGameObjectWithTagSafe(string tagName)
        {
            if (string.IsNullOrEmpty(tagName))
            {
                return null;
            }

            try
            {
                return GameObject.FindGameObjectWithTag(tagName);
            }
            catch (UnityException)
            {
                return null;
            }
        }

        private static bool TagExists(string tagName)
        {
            if (string.IsNullOrEmpty(tagName))
            {
                return false;
            }

            GameObject temp = null;
            try
            {
                temp = new GameObject("__tag_check__");
                temp.tag = tagName;
                return true;
            }
            catch (UnityException)
            {
                return false;
            }
            finally
            {
                if (temp != null)
                {
                    if (Application.isPlaying)
                    {
                        Destroy(temp);
                    }
                    else
                    {
                        DestroyImmediate(temp);
                    }
                }
            }
        }
    }
}

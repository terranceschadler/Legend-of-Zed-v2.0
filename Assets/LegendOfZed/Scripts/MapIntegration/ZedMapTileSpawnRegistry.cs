using System.Collections.Generic;
using UnityEngine;

namespace LegendOfZed.MapIntegration
{
    /// <summary>
    /// Runtime-only evidence collector for the imported legacy tile generator.
    /// TileSpawn marker GameObjects can be destroyed by collision cleanup before the
    /// generator/gap resolver sees them, so this registry records their position,
    /// source tile center, and facing as soon as a tile exists and again just before
    /// a marker is destroyed.
    /// </summary>
    public static class ZedMapTileSpawnRegistry
    {
        public struct SpawnRecord
        {
            public Vector3 position;
            public Vector3 forward;
            public Vector3 sourceTilePosition;
            public string sourceTileName;
            public bool destroyedByCollision;
        }

        private static readonly List<SpawnRecord> Records = new List<SpawnRecord>();
        private static int _lastFrameCleared = -1;

        public static IReadOnlyList<SpawnRecord> SpawnRecords => Records;

        public static void ResetForNewPlaySession()
        {
            Records.Clear();
            _lastFrameCleared = Time.frameCount;
        }

        public static void RegisterSpawn(Transform spawn, bool destroyedByCollision)
        {
            if (spawn == null)
            {
                return;
            }

            string sourceTileName = string.Empty;
            Vector3 sourceTilePosition = Vector3.zero;

            Transform parent = spawn.parent;
            while (parent != null)
            {
                if (parent.name.StartsWith("RoomTile"))
                {
                    sourceTileName = parent.name;
                    sourceTilePosition = Snap(parent.position);
                    break;
                }

                parent = parent.parent;
            }

            RegisterSpawn(spawn.position, spawn.forward, sourceTilePosition, sourceTileName, destroyedByCollision);
        }

        public static void RegisterSpawn(Vector3 position, Vector3 forward, Vector3 sourceTilePosition, string sourceTileName, bool destroyedByCollision)
        {
            Vector3 snappedPosition = Snap(position);
            Vector3 snappedForward = SnapDirection(forward);
            Vector3 snappedSource = Snap(sourceTilePosition);

            for (int i = 0; i < Records.Count; i++)
            {
                SpawnRecord existing = Records[i];
                if ((existing.position - snappedPosition).sqrMagnitude < 0.01f &&
                    (existing.sourceTilePosition - snappedSource).sqrMagnitude < 0.01f)
                {
                    if (destroyedByCollision && !existing.destroyedByCollision)
                    {
                        existing.destroyedByCollision = true;
                        Records[i] = existing;
                    }

                    return;
                }
            }

            Records.Add(new SpawnRecord
            {
                position = snappedPosition,
                forward = snappedForward,
                sourceTilePosition = snappedSource,
                sourceTileName = sourceTileName ?? string.Empty,
                destroyedByCollision = destroyedByCollision
            });
        }

        public static Vector3 Snap(Vector3 position)
        {
            const float grid = 20f;
            return new Vector3(
                Mathf.Round(position.x / grid) * grid,
                0f,
                Mathf.Round(position.z / grid) * grid);
        }

        public static Vector3 SnapDirection(Vector3 direction)
        {
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.0001f)
            {
                return Vector3.forward;
            }

            direction.Normalize();

            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.z))
            {
                return direction.x >= 0f ? Vector3.right : Vector3.left;
            }

            return direction.z >= 0f ? Vector3.forward : Vector3.back;
        }

        public static bool WasClearedThisFrame()
        {
            return _lastFrameCleared == Time.frameCount;
        }
    }
}

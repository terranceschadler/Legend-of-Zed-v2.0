using LegendOfZed.MapIntegration;
using UnityEngine;

namespace LegendOfZed.LegacyMapGenerator
{
    public class ZedLegacySpawnCollisionDetector : MonoBehaviour
    {
        private bool _recordedAndDestroyed;

        private void Awake()
        {
            ZedMapTileSpawnRegistry.RegisterSpawn(transform, false);
        }

        private void OnTriggerStay(Collider other)
        {
            if (_recordedAndDestroyed || other == null)
            {
                return;
            }

            // Critical fix:
            // Multiple branch endpoints can produce TileSpawn markers at the same
            // empty cell. The old imported detector destroyed itself on ANY trigger,
            // so TileSpawn-vs-TileSpawn overlap deleted the evidence before the
            // generator could place the correct tile.
            //
            // Ignore other TileSpawn markers. The generator already prevents double
            // placement with tilePositions.Contains(spawnPoint.position), so one
            // surviving marker is enough and preserves the correct placement rotation.
            if (other.CompareTag("TileSpawn"))
            {
                return;
            }

            _recordedAndDestroyed = true;
            ZedMapTileSpawnRegistry.RegisterSpawn(transform, true);
            Destroy(gameObject);
        }
    }
}

using LegendOfZed.MapIntegration;
using UnityEngine;

namespace LegendOfZed.LegacyMapGenerator
{
    /// <summary>
    /// Small companion component for imported legacy room tiles.
    /// It does not change tile behavior. It only records child TileSpawn markers before
    /// collision cleanup can delete them.
    /// </summary>
    public class ZedLegacyRoomTileSpawnRegistryHook : MonoBehaviour
    {
        private void Awake()
        {
            RegisterChildSpawns();
        }

        private void Start()
        {
            RegisterChildSpawns();
        }

        private void RegisterChildSpawns()
        {
            Transform[] children = GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < children.Length; i++)
            {
                Transform child = children[i];
                if (child != null && child.CompareTag("TileSpawn"))
                {
                    ZedMapTileSpawnRegistry.RegisterSpawn(child, false);
                }
            }
        }
    }
}

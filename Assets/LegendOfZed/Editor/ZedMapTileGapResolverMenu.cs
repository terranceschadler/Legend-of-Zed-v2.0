using LegendOfZed.LegacyMapGenerator;
using LegendOfZed.MapIntegration;
using UnityEditor;
using UnityEngine;

namespace LegendOfZed.Editor
{
    public static class ZedMapTileGapResolverMenu
    {
        private const string MenuRoot = "Legend of Zed/Map Integration/";

        [MenuItem(MenuRoot + "Add Gap Resolver To Scene")]
        public static void AddGapResolverToScene()
        {
            ZedMapTileGapResolver resolver = Object.FindAnyObjectByType<ZedMapTileGapResolver>();
            if (resolver == null)
            {
                GameObject resolverObject = new GameObject("Zed_MapTile_GapResolver");
                resolver = resolverObject.AddComponent<ZedMapTileGapResolver>();
                Undo.RegisterCreatedObjectUndo(resolverObject, "Add Map Tile Gap Resolver");
            }

            resolver.generator = Object.FindAnyObjectByType<ZedLegacyRandomMapGenerator>();
            resolver.runAutomatically = false;
            resolver.gridSize = 20f;
            resolver.waitTimeoutSeconds = 10f;
            resolver.logDebugDetails = false;

            EditorUtility.SetDirty(resolver);
            Selection.activeObject = resolver.gameObject;

            Debug.Log("Map tile gap resolver added/updated in scene but left disabled. Current fix uses TileSpawn collision handling instead.", resolver);
        }

        [MenuItem(MenuRoot + "Remove Gap Resolver From Scene")]
        public static void RemoveGapResolverFromScene()
        {
            ZedMapTileGapResolver[] resolvers = Object.FindObjectsByType<ZedMapTileGapResolver>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            int removed = 0;

            for (int i = 0; i < resolvers.Length; i++)
            {
                if (resolvers[i] == null)
                {
                    continue;
                }

                Undo.DestroyObjectImmediate(resolvers[i].gameObject);
                removed++;
            }

            Debug.Log("Removed " + removed + " map tile gap resolver object(s) from scene.");
        }
    }
}

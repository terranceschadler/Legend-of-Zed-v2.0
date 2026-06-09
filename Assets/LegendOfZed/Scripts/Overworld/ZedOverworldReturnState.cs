using UnityEngine;

namespace LegendOfZed.Overworld
{
    public static class ZedOverworldReturnState
    {
        public static bool HasReturnState;
        public static Vector3 ReturnPosition;
        public static Quaternion ReturnRotation;
        public static int OverworldSeed;
        public static string LastPortalId;

        public static void StoreReturn(Vector3 position, Quaternion rotation, int seed, string portalId)
        {
            HasReturnState = true;
            ReturnPosition = position;
            ReturnRotation = rotation;
            OverworldSeed = seed;
            LastPortalId = portalId;
        }

        public static void Clear()
        {
            HasReturnState = false;
            ReturnPosition = Vector3.zero;
            ReturnRotation = Quaternion.identity;
            OverworldSeed = 0;
            LastPortalId = string.Empty;
        }
    }
}

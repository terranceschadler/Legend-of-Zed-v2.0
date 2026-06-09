using UnityEngine;

namespace LegendOfZed.Overworld
{
    [DisallowMultipleComponent]
    public class ZedOverworldEnterableBuildingMarker : MonoBehaviour
    {
        public Vector2Int GridCoordinate;
        public int Seed;
        public string InteriorSceneName = "";
        public bool IsPortalCandidate = true;
    }
}

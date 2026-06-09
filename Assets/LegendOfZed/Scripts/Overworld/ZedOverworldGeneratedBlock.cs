using UnityEngine;

namespace LegendOfZed.Overworld
{
    [DisallowMultipleComponent]
    public class ZedOverworldGeneratedBlock : MonoBehaviour
    {
        public Vector2Int GridCoordinate;
        public ZedOverworldBlockType BlockType;
        public int Seed;
        public bool EnterableBuildingCandidate;
    }
}

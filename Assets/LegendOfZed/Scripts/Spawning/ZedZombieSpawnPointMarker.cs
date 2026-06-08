using UnityEngine;

namespace LegendOfZed.Spawning
{
    [DisallowMultipleComponent]
    public class ZedZombieSpawnPointMarker : MonoBehaviour
    {
        [Header("Spawn Point")]
        public string SpawnLabel = "Zombie Spawn";
        public float SpawnRadius = 0.75f;
        public bool FacePlayerOnSetup = true;

        [Header("Gizmos")]
        public bool DrawGizmos = true;
        public bool DrawLabel = true;
        public Color GizmoColor = new Color(1f, 0.2f, 0.1f, 0.8f);
        public Color DirectionColor = new Color(1f, 1f, 0.2f, 0.9f);
        public float DirectionLength = 1.25f;

        private void OnDrawGizmos()
        {
            if (!DrawGizmos)
            {
                return;
            }

            DrawMarkerGizmos();
        }

        private void OnDrawGizmosSelected()
        {
            DrawMarkerGizmos();
        }

        private void DrawMarkerGizmos()
        {
            Color oldColor = Gizmos.color;

            Gizmos.color = GizmoColor;
            Gizmos.DrawWireSphere(transform.position, Mathf.Max(0.05f, SpawnRadius));
            Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 1.5f);

            Gizmos.color = DirectionColor;
            Vector3 start = transform.position + Vector3.up * 0.15f;
            Vector3 end = start + transform.forward * Mathf.Max(0.1f, DirectionLength);
            Gizmos.DrawLine(start, end);
            Gizmos.DrawSphere(end, 0.08f);

            Gizmos.color = oldColor;
        }
    }
}

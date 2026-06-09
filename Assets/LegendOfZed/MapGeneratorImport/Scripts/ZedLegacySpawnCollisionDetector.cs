using UnityEngine;

namespace LegendOfZed.LegacyMapGenerator
{
    public class ZedLegacySpawnCollisionDetector : MonoBehaviour
    {
        private void OnTriggerStay(Collider other)
        {
            Destroy(gameObject);
        }
    }
}

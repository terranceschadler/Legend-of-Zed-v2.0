using UnityEngine;

namespace LegendOfZed.LegacyMapGenerator
{
    public class ZedLegacyGateway : MonoBehaviour
    {
        private Collider _myTrigger;

        private void Start()
        {
            _myTrigger = GetComponent<Collider>();
            if (_myTrigger != null)
            {
                _myTrigger.isTrigger = true;
            }

            MeshRenderer renderer = GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.enabled = false;
            }
        }
    }
}

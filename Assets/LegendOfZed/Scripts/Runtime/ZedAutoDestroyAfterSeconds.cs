using UnityEngine;

namespace LegendOfZed.Runtime
{
    [DisallowMultipleComponent]
    public class ZedAutoDestroyAfterSeconds : MonoBehaviour
    {
        public float Lifetime = 3f;

        private void OnEnable()
        {
            Destroy(gameObject, Mathf.Max(0.05f, Lifetime));
        }
    }
}

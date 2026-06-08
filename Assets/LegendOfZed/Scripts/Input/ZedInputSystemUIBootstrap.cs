using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace LegendOfZed.Input
{
    public sealed class ZedInputSystemUIBootstrap : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void BootstrapSceneEventSystems()
        {
            EventSystem[] eventSystems = Object.FindObjectsByType<EventSystem>(FindObjectsInactive.Include);
            foreach (EventSystem eventSystem in eventSystems)
            {
                if (eventSystem == null)
                {
                    continue;
                }

                StandaloneInputModule standaloneInputModule = eventSystem.GetComponent<StandaloneInputModule>();
                if (standaloneInputModule != null)
                {
                    Object.Destroy(standaloneInputModule);
                }

                if (eventSystem.GetComponent<InputSystemUIInputModule>() == null)
                {
                    eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
                }
            }
        }
    }
}

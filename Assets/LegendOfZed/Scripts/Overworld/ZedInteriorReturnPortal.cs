using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace LegendOfZed.Overworld
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public class ZedInteriorReturnPortal : MonoBehaviour
    {
        public string OverworldSceneName = "Zed_Overworld";
        public bool RequireInteractKey = true;
        public string PromptText = "Press E to return";
        public bool ShowDebugPrompt = true;

        private bool _playerInside;

        private void Reset()
        {
            Collider trigger = GetComponent<Collider>();
            trigger.isTrigger = true;
        }

        private void Awake()
        {
            Collider trigger = GetComponent<Collider>();
            trigger.isTrigger = true;
        }

        private void Update()
        {
            if (!_playerInside)
            {
                return;
            }

            if (!RequireInteractKey || WasInteractPressed())
            {
                SceneManager.LoadScene(OverworldSceneName, LoadSceneMode.Single);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") || other.GetComponentInParent<CharacterController>() != null)
            {
                _playerInside = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player") || other.GetComponentInParent<CharacterController>() != null)
            {
                _playerInside = false;
            }
        }

        private void OnGUI()
        {
            if (!ShowDebugPrompt || !_playerInside)
            {
                return;
            }

            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.alignment = TextAnchor.MiddleCenter;
            style.fontSize = 24;
            style.fontStyle = FontStyle.Bold;

            GUI.Label(new Rect(0f, Screen.height - 90f, Screen.width, 40f), PromptText, style);
        }

        private static bool WasInteractPressed()
        {
            Keyboard keyboard = Keyboard.current;
            return keyboard != null && keyboard.eKey.wasPressedThisFrame;
        }
    }
}

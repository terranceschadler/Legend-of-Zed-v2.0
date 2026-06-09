using TopDownShooter;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace LegendOfZed.Overworld
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public class ZedOverworldPortalTrigger : MonoBehaviour
    {
        public string PortalId = "OverworldPortal";
        public string InteriorSceneName = "Zed_Interior_Test";
        public bool RequireInteractKey = true;
        public string PromptText = "Press E to enter";
        public bool ShowDebugPrompt = true;
        public ZedOverworldGenerationManager OverworldManager;

        private bool _playerInside;
        private Transform _player;

        private void Reset()
        {
            Collider trigger = GetComponent<Collider>();
            trigger.isTrigger = true;
        }

        private void Awake()
        {
            Collider trigger = GetComponent<Collider>();
            trigger.isTrigger = true;

            if (OverworldManager == null)
            {
                OverworldManager = FindAnyObjectByType<ZedOverworldGenerationManager>();
            }
        }

        private void Update()
        {
            if (!_playerInside || _player == null)
            {
                return;
            }

            if (!RequireInteractKey || WasInteractPressed())
            {
                EnterInterior();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!IsPlayer(other))
            {
                return;
            }

            _playerInside = true;
            _player = other.transform;
        }

        private void OnTriggerExit(Collider other)
        {
            if (_player == null || other.transform != _player)
            {
                return;
            }

            _playerInside = false;
            _player = null;
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

        private void EnterInterior()
        {
            if (_player == null)
            {
                return;
            }

            int seed = OverworldManager != null ? OverworldManager.OverworldSeed : 0;
            ZedOverworldReturnState.StoreReturn(_player.position, _player.rotation, seed, PortalId);

            if (string.IsNullOrWhiteSpace(InteriorSceneName))
            {
                Debug.LogWarning("Portal has no InteriorSceneName assigned.", this);
                return;
            }

            SceneManager.LoadScene(InteriorSceneName, LoadSceneMode.Single);
        }

        private static bool IsPlayer(Collider other)
        {
            if (other == null)
            {
                return false;
            }

            if (other.GetComponentInParent<PlayerController>() != null)
            {
                return true;
            }

            return other.CompareTag("Player") || other.GetComponentInParent<CharacterController>() != null;
        }
    }
}

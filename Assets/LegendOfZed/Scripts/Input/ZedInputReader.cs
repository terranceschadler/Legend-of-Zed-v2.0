using UnityEngine;
using UnityEngine.InputSystem;

namespace LegendOfZed.Input
{
    public sealed class ZedInputReader : MonoBehaviour
    {
        public static ZedInputReader Instance { get; private set; }

        public Vector2 Move { get; private set; }
        public Vector2 AimScreenPosition { get; private set; }

        public bool FireHeld { get; private set; }
        public bool JumpPressedThisFrame { get; private set; }
        public bool DashPressedThisFrame { get; private set; }
        public bool JetPackHeld { get; private set; }
        public bool SlowFallPressedThisFrame { get; private set; }
        public bool DropWeaponPressedThisFrame { get; private set; }
        public bool ReloadPressedThisFrame { get; private set; }
        public bool PausePressedThisFrame { get; private set; }
        public bool RotateCameraLeftHeld { get; private set; }
        public bool RotateCameraRightHeld { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void EnsureInstanceBeforeSceneLoad()
        {
            if (Instance != null)
            {
                return;
            }

            GameObject inputObject = new GameObject("Zed Input Reader");
            DontDestroyOnLoad(inputObject);
            inputObject.AddComponent<ZedInputReader>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            Mouse mouse = Mouse.current;
            Gamepad gamepad = Gamepad.current;

            Move = ReadMove(keyboard, gamepad);
            AimScreenPosition = mouse != null ? mouse.position.ReadValue() : Vector2.zero;

            FireHeld = mouse != null && mouse.leftButton.isPressed;
            JumpPressedThisFrame = (keyboard != null && keyboard.spaceKey.wasPressedThisFrame) || (gamepad != null && gamepad.buttonSouth.wasPressedThisFrame);
            DashPressedThisFrame = keyboard != null && keyboard.fKey.wasPressedThisFrame;
            JetPackHeld = keyboard != null && keyboard.xKey.isPressed;
            SlowFallPressedThisFrame = keyboard != null && keyboard.vKey.wasPressedThisFrame;
            DropWeaponPressedThisFrame = keyboard != null && keyboard.gKey.wasPressedThisFrame;
            ReloadPressedThisFrame = keyboard != null && keyboard.rKey.wasPressedThisFrame;
            PausePressedThisFrame = (keyboard != null && keyboard.escapeKey.wasPressedThisFrame) || (gamepad != null && gamepad.startButton.wasPressedThisFrame);
            RotateCameraLeftHeld = keyboard != null && keyboard.eKey.isPressed;
            RotateCameraRightHeld = keyboard != null && keyboard.qKey.isPressed;
        }

        private static Vector2 ReadMove(Keyboard keyboard, Gamepad gamepad)
        {
            Vector2 move = Vector2.zero;

            if (keyboard != null)
            {
                if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
                {
                    move.x -= 1f;
                }

                if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
                {
                    move.x += 1f;
                }

                if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
                {
                    move.y -= 1f;
                }

                if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
                {
                    move.y += 1f;
                }
            }

            if (gamepad != null)
            {
                Vector2 stick = gamepad.leftStick.ReadValue();
                if (stick.sqrMagnitude > move.sqrMagnitude)
                {
                    move = stick;
                }
            }

            return Vector2.ClampMagnitude(move, 1f);
        }
    }
}

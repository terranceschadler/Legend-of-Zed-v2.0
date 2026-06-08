using LegendOfZed.Input;
using UnityEngine;

/* Script to easy setup your own input configurations.
 * You can use the virtual joystick solution in this pack or use another solution.
 * Note: if you go to use joystick like a Xbox controller you need add this two
 * new axis to the input manager.
 * */

namespace TopDownShooter
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Scripts reference")] public MovementCharacterController MovCharController;
        public ShooterController ShooterController;
        public SwimmingController SwimmingController;

        [Header("Use mouse to shoot and rotate player")]
        public bool UseMouseToRotate = true;

        [Tooltip("This is the layer for the ground.")]
        public LayerMask GroundLayer;

        [Header("Use virtualJoystick to control the player")]
        public bool UseVirtualJoyStick;

        public Joystick JoystickControllerLeft;
        public Joystick JoystickControllerRight;

        private bool _activeJetPack;
        private bool _activeSlowFall;

        private void Awake()
        {
            //avoid use more than one control at the same time
            if (UseMouseToRotate)
            {
                UseVirtualJoyStick = false;
            }

            CheckForVirtualJoystick();
        }

        public float GetHorizontalValue()
        {
            if (UseVirtualJoyStick)
            {
                return JoystickControllerLeft.Horizontal;
            }

            ZedInputReader input = ZedInputReader.Instance;
            return input != null ? input.Move.x : 0f;
        }

        public float GetVerticalValue()
        {
            if (UseVirtualJoyStick)
            {
                return JoystickControllerLeft.Vertical;
            }

            ZedInputReader input = ZedInputReader.Instance;
            return input != null ? input.Move.y : 0f;
        }

        public float GetHorizontal2Value()
        {
            if (UseMouseToRotate)
            {
                return GetMouseDirection().x;
            }

            if (UseVirtualJoyStick)
            {
                return JoystickControllerRight.Horizontal;
            }

            ZedInputReader input = ZedInputReader.Instance;
            return input != null ? input.Move.x : 0f;
        }

        public float GetVertical2Value()
        {
            if (UseMouseToRotate)
            {
                return GetMouseDirection().z;
            }

            if (UseVirtualJoyStick)
            {
                return JoystickControllerRight.Vertical;
            }

            ZedInputReader input = ZedInputReader.Instance;
            return input != null ? input.Move.y : 0f;
        }

        public bool GetJumpValue()
        {
            ZedInputReader input = ZedInputReader.Instance;
            return input != null && input.JumpPressedThisFrame;
        }

        public bool GetDashValue()
        {
            ZedInputReader input = ZedInputReader.Instance;
            return input != null && input.DashPressedThisFrame;
        }

        public bool GetJetPackValue()
        {
            if (UseVirtualJoyStick)
            {
                return _activeJetPack;
            }

            ZedInputReader input = ZedInputReader.Instance;
            return input != null && input.JetPackHeld;
        }

        public bool GetSlowFallValue()
        {
            if (UseVirtualJoyStick)
            {
                if (!_activeSlowFall)
                {
                    return false;
                }

                _activeSlowFall = false;
                return true;
            }

            ZedInputReader input = ZedInputReader.Instance;
            return input != null && input.SlowFallPressedThisFrame;
        }

        public bool GetDropWeaponValue()
        {
            ZedInputReader input = ZedInputReader.Instance;
            return input != null && input.DropWeaponPressedThisFrame;
        }

        public bool GetReloadWeaponValue()
        {
            ZedInputReader input = ZedInputReader.Instance;
            return input != null && input.ReloadPressedThisFrame;
        }

        public void ActivateJetPack(bool active)
        {
            _activeJetPack = active;
        }

        public void ActivateSlowFall()
        {
            _activeSlowFall = true;
        }
        public void DeActivateSlowFall()
        {
            _activeSlowFall = false;
        }

        public Vector3 GetMouseDirection()
        {
            ZedInputReader input = ZedInputReader.Instance;
            if (input == null || !input.FireHeld || Camera.main == null)
            {
                return Vector3.zero;
            }

            Ray newRay = Camera.main.ScreenPointToRay(input.AimScreenPosition);

            //check if the player press mouse button and the ray hit the ground
            if (Physics.Raycast(newRay, out RaycastHit groundHit, 1000, GroundLayer))
            {
                Vector3 playerToMouse = groundHit.point - transform.position;

                playerToMouse.y = 0f;

                return playerToMouse;
            }

            return Vector3.zero;
        }

        //hide or show virtualJoystick if exist
        private void CheckForVirtualJoystick()
        {
            if (UseVirtualJoyStick)
            {
                if (JoystickControllerLeft)
                {
                    JoystickControllerLeft.gameObject.SetActive(true);
                }

                if (JoystickControllerRight)
                {
                    JoystickControllerRight.gameObject.SetActive(true);
                }
            }
            else
            {
                if (JoystickControllerLeft)
                {
                    JoystickControllerLeft.gameObject.SetActive(false);
                }

                if (JoystickControllerRight)
                {
                    JoystickControllerRight.gameObject.SetActive(false);
                }
            }
        }
    }
}
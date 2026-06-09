using UnityEngine;
using UnityEngine.InputSystem;

namespace LegendOfZed.Overworld
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterController))]
    public class ZedPortalTestPlayerMover : MonoBehaviour
    {
        public float MoveSpeed = 7f;
        public float Gravity = -18f;

        private CharacterController _controller;
        private float _verticalVelocity;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
        }

        private void Update()
        {
            if (_controller == null)
            {
                return;
            }

            Keyboard keyboard = Keyboard.current;

            Vector3 move = Vector3.zero;
            if (keyboard != null)
            {
                if (keyboard.wKey.isPressed) move.z += 1f;
                if (keyboard.sKey.isPressed) move.z -= 1f;
                if (keyboard.dKey.isPressed) move.x += 1f;
                if (keyboard.aKey.isPressed) move.x -= 1f;
            }

            move = Vector3.ClampMagnitude(move, 1f);

            if (_controller.isGrounded && _verticalVelocity < 0f)
            {
                _verticalVelocity = -1f;
            }

            _verticalVelocity += Gravity * Time.deltaTime;

            Vector3 velocity = move * MoveSpeed;
            velocity.y = _verticalVelocity;

            _controller.Move(velocity * Time.deltaTime);

            if (move.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(move.normalized, Vector3.up);
            }
        }
    }
}

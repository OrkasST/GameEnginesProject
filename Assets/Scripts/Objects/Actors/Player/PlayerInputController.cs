using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.Scripts.Objects.Actors.Player
{
    [RequireComponent(typeof(ActorController))]
    public class PlayerInputController : MonoBehaviour
    {
        public InputActionReference move;
        public InputActionReference jump;

        private ActorController _player;

        private void Start()
        {
            _player = GetComponent<ActorController>();
        }

        private void Update()
        {
            if (move.action.ReadValue<Vector2>().x > 0)
            {
                _player.StateMachine.RegisterStateChange(ActorState.Running);
                _player.StateMachine.RegisterStateChange(LookDirection.Right);
            }
            else if (move.action.ReadValue<Vector2>().x < 0)
            {
                _player.StateMachine.RegisterStateChange(ActorState.Running);
                _player.StateMachine.RegisterStateChange(LookDirection.Left);
            }
            else if (move.action.ReadValue<Vector2>().x == 0)
            {
                _player.StateMachine.RegisterStateChange(ActorState.Standing);
            }
        }

        public void OnEnable()
        {
            jump.action.started += Jump;
            jump.action.canceled += StopJump;
        }
        public void OnDisable()
        {
            jump.action.started -= Jump;
            jump.action.canceled -= StopJump;
        }

        private void StopJump(InputAction.CallbackContext context)
        {
            _player.StateMachine.RegisterStateChange(ActorState.StoppingJump);
        }

        private void Jump(InputAction.CallbackContext context)
        {
            _player.StateMachine.RegisterStateChange(ActorState.Jumping);
        }
    }
}

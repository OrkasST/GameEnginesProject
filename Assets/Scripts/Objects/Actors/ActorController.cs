using Assets.Scripts.Business.Lists;
using System;
using UnityEngine;

namespace Assets.Scripts.Objects.Actors
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class ActorController : MonoBehaviour
    {
        public ActorStateMachine StateMachine { get; private set; } = new();


        public Rigidbody2D RB { get; private set; }
        public float Speed { get; private set; }
        public float JumpForce { get; private set; }


        private bool wasInitialised = false;

        public void Init()
        {
            wasInitialised = true;
            RB = GetComponent<Rigidbody2D>();
            Speed = 7f;
            JumpForce = 10f;
        }

        private void Update()
        {
            if (!wasInitialised) return;
            if (StateMachine.CurrentState == ActorState.Running) ActionMachine.Move(this);
            if (StateMachine.CurrentState == ActorState.Standing) ActionMachine.Stop(this);
            if (StateMachine.CurrentState == ActorState.Jumping) ActionMachine.Jump(this);
            if (StateMachine.CurrentState == ActorState.StoppingJump) ActionMachine.StopJump(this);

            CheckOnFall();
        }

        private void CheckOnFall()
        {
            if (RB.linearVelocity.y <= -0.1f)
            {
                StateMachine.RegisterStateChange(ActorState.Falling);
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision == null) return;
            if (collision.gameObject.layer == Layers.Ground && StateMachine.CurrentState == ActorState.Falling)
            {
                StateMachine.RegisterStateChange(ActorState.Landing);
            }
        }
    }
}

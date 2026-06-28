using Assets.Scripts.Business.Lists;
using Assets.Scripts.Objects.Actors.Player;
using Assets.Scripts.Objects.InteractibleObjects;
using System;
using UnityEngine;

namespace Assets.Scripts.Objects.Actors
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Animator))]
    public class ActorController : MonoBehaviour
    {
        public ActorStateMachine StateMachine { get; private set; } = new();
        public Inventory ActorInventory { get; private set; } = new();

        public Rigidbody2D RB { get; private set; }
        public float Speed { get; private set; }
        public float JumpForce { get; private set; }

        #region AttackData
        public Vector2 AttackInitialPoint { get; private set; }
        public int TicksToStart { get; private set; }
        public Vector2[] AttackMovementVectors { get; private set; }
        public int[] AttackMovementTicks { get; private set; }
        public float[] AttackSpeed { get; private set; }
        public float LastAttackTime = 0;
        public float AttackInterval { get; private set; }
        #endregion

        public float MaxHp { get; private set; }
        public float CurrentDamage { get; private set; }
        public float DealingDamage { get; private set; }


        private bool wasInitialised = false;
        public Vector3 SpawnPosition { get; private set; }
        public Action OnDeath { get; private set; }

        private Vector3 _leftScale = new Vector3(-1, 1, 1);
        private Vector3 _rightScale = new Vector3(1, 1, 1);

        public InteractibleObject Interactible { get; private set; }

        public Animator ActorAnimator { get; private set; }

        public void Init(Vector3 position)
        {
            Debug.Log(gameObject.name + " Was Initialized");
            wasInitialised = true;
            SpawnPosition = position;

            RB = GetComponent<Rigidbody2D>();
            Speed = 7f;
            JumpForce = 10f;

            AttackInitialPoint = new Vector2(0.93f, 0.92f);
            TicksToStart = 10;
            AttackMovementVectors = new Vector2[] { new Vector2(0, -1f), new Vector2(1f, 1f) };
            AttackMovementTicks = new int[] { 10, 5 };
            AttackSpeed = new float[] { 12f, 16f };
            AttackInterval = 3f;

            MaxHp = 100;
            CurrentDamage = 0;
            DealingDamage = 10;

            transform.GetChild(0).gameObject.GetComponent<AttackColliderLogic>().Init(OnAttack);

            StateMachine.name = gameObject.name;
            ActorAnimator = GetComponent<Animator>();
        }

        private void Update()
        {
            if (!wasInitialised) return;

            UpdateDirection();

            if (StateMachine.CurrentState == ActorState.Running) ActionMachine.Move(this);
            else if (StateMachine.CurrentState == ActorState.Standing) ActionMachine.Stop(this);
            else if (StateMachine.CurrentState == ActorState.Jumping) ActionMachine.Jump(this);
            else if (StateMachine.CurrentState == ActorState.StoppingJump) ActionMachine.StopJump(this);
            else if (StateMachine.CurrentState == ActorState.StartingAttack) ActionMachine.Attack(this);
            else if (StateMachine.CurrentState == ActorState.Dieing)
            {
                ActionMachine.Stop(this);
                OnDeath();
                Destroy(gameObject);
            }
            else if (StateMachine.CurrentState == ActorState.Interacting) { ActionMachine.InteractWithObject(this); }

            CheckOnFall();
        }

        private void UpdateDirection()
        {
            //if (StateMachine.CurrentDirection == StateMachine.LastDirection) return;
            if (StateMachine.CurrentDirection == LookDirection.Left) gameObject.transform.localScale = _leftScale;
            else if (StateMachine.CurrentDirection == LookDirection.Right) gameObject.transform.localScale = _rightScale;
        }

        private void CheckOnFall()
        {
            if (RB.linearVelocity.y <= -0.1f)
            {
                StateMachine.RegisterStateChange(ActorState.Falling);
                ActorAnimator.SetBool("IsJumping", false);
                ActorAnimator.SetBool("IsFalling", true);
            }
        }

        public void SetDamage(float damage)
        {
            CurrentDamage = damage;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision == null) return;
            if (collision.gameObject.layer == Layers.Ground && StateMachine.CurrentState == ActorState.Falling)
            {
                StateMachine.RegisterStateChange(ActorState.Landing);
                ActorAnimator.SetBool("IsFalling", false);
                StateMachine.RegisterStateChange(ActorState.Standing);
            }
        }

        private void OnAttack(Collider2D collision)
        {
            if (collision.gameObject.GetEntityId() == transform.GetChild(0).gameObject.GetEntityId() || collision.gameObject.tag == Tags.AttackCollider) return;

            Debug.Log("Collision Layer: " + collision.gameObject.layer);
            Debug.Log(gameObject.name + ":::" + collision.gameObject.name);
            Debug.Log("PushBack: " + (int)StateMachine.CurrentDirection);
            ActionMachine.TakeDamage(collision.gameObject.GetComponent<ActorController>(), DealingDamage, new Vector2((int)StateMachine.CurrentDirection * 2f, 2f));
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision == null) return;
            var interactible = collision.gameObject.GetComponent<InteractibleObject>();
            if (interactible != null)
            {
                interactible.ShowHint();
                Interactible = interactible;
            }
        }
        private void OnTriggerExit2D(Collider2D collision)
        {
            var interactible = collision.gameObject.GetComponent<InteractibleObject>();
            if (interactible != null)
            {
                interactible.HideHint();
                Interactible = null;
            }
        }
        public void ResetInteractible() => Interactible = null;

        public void Respawn()
        {
            transform.position = SpawnPosition;
        }
        public void SetDeathAction(Action action)
        {
            OnDeath = action;
        }
    }
}

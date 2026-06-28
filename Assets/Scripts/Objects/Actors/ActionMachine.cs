using Assets.Scripts.Business.Lists;
using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEditor.U2D.Animation;
using UnityEngine;

namespace Assets.Scripts.Objects.Actors
{
    public static class ActionMachine
    {
        public static void Move(ActorController actor)
        {
            actor.ActorAnimator.SetBool("IsRunning", true);
            actor.RB.linearVelocity = new Vector2 ((int)actor.StateMachine.CurrentDirection * actor.Speed, actor.RB.linearVelocity.y);
        }

        public static void Stop(ActorController actor)
        {
            actor.ActorAnimator.SetBool("IsRunning", false);
            actor.RB.linearVelocity = Vector2.zero;
        }

        public static void Jump(ActorController actor)
        {
            actor.StateMachine.RegisterStateChange(ActorState.Accelerating);
            actor.ActorAnimator.SetBool("IsJumping", true);

            int sleepTime = (int)(Time.fixedDeltaTime * 1000);
            actor.RB.linearVelocity = new Vector2(actor.RB.linearVelocity.x, actor.JumpForce);
        }

        public static void StopJump(ActorController actor)
        {
            actor.StateMachine.RegisterStateChange(ActorState.Decellerating);
            actor.RB.linearVelocity = new Vector2(actor.RB.linearVelocity.x, actor.RB.linearVelocity.y * 0.3f);
        }

        public static void Attack(ActorController actor)
        {
            if (Time.time - actor.LastAttackTime < actor.AttackInterval) return;
            actor.LastAttackTime = Time.time;
            Stop(actor);
            actor.StateMachine.RegisterStateChange(ActorState.Attacking);
            actor.ActorAnimator.SetBool("IsAttacking", true);
            GameObject attackCollider = actor.gameObject.transform.GetChild(0).gameObject;
            actor.StartCoroutine(AttackMovement(
                (int)actor.StateMachine.CurrentDirection,
                actor.AttackInitialPoint, actor.TicksToStart,
                actor.AttackMovementVectors, actor.AttackMovementTicks,
                actor.AttackSpeed, attackCollider,
                () => {
                    actor.StateMachine.RegisterStateChange(ActorState.FinishedAttacking);
                    actor.ActorAnimator.SetBool("IsAttacking", false);
                }
                ));
        }
        private static IEnumerator AttackMovement(
            int direction,
            Vector2 startPoint, int ticksToStart,
            Vector2[] movementVectors, int[] ticks,
            float[] attackSpeed, GameObject attackCollider, 
            Action onEnd
            )
        {
            yield return new WaitForFixedUpdate();
            while (ticksToStart > 0)
            {
                ticksToStart--;
                yield return new WaitForFixedUpdate();
            }

            attackCollider.transform.localPosition = startPoint;
            attackCollider.SetActive(true);

            for (int i = 0; i < ticks.Length; i++)
            {
                int tick = ticks[i];
                Vector3 distance = (Vector3)(movementVectors[i] * attackSpeed[i] * Time.fixedDeltaTime);
                distance = new Vector3(distance.x * direction, distance.y, 1);

                while (tick > 0)
                {
                    tick--;
                    attackCollider.transform.position += distance;
                    yield return new WaitForFixedUpdate();
                }
            }

            attackCollider.SetActive(false);
            onEnd.Invoke();
            yield return null;
        }

        public static async void TakeDamage(ActorController actor, float damage, Vector2 pushBack)
        {
            actor.StateMachine.RegisterStateChange(ActorState.TakingDamage);
            actor.SetDamage(actor.CurrentDamage + damage);
            actor.RB.linearVelocity = pushBack;

            Debug.Log(actor.name + ": " + actor.CurrentDamage + ", state: " + actor.StateMachine.CurrentState);

            if (actor.CurrentDamage < actor.MaxHp)
            {

                await Task.Delay(1000);

                actor.StateMachine.RegisterStateChange(ActorState.TookDamage);
                actor.StateMachine.RegisterStateChange(ActorState.Standing);
            }
            else
            {
                actor.StateMachine.RegisterStateChange(ActorState.Dieing);
            }
        }

        public static async void InteractWithObject(ActorController actor)
        {
            if (actor.ActorAnimator.GetBool("IsInteracting")) return;
            if (actor.Interactible == null)
            {
                actor.StateMachine.RegisterStateChange(ActorState.FinishedInteracting);
                return;
            }
            actor.ActorAnimator.SetBool("IsInteracting", true);
            await Task.Delay(600);  
            var interatible = actor.Interactible;

            Debug.Log("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            Debug.Log("interatible: " + interatible);

            actor.ActorInventory.StoreItem(interatible.ObjectID);
            Debug.Log("actor.ActorInventory: " + actor.ActorInventory);

            actor.ResetInteractible();
            interatible.OnBeingPicked();
            actor.StateMachine.RegisterStateChange(ActorState.FinishedInteracting);
            actor.ActorAnimator.SetBool("IsInteracting", false);

            Debug.Log("Storage:");
            Debug.Log(actor.ActorInventory.Storage[0]);
            Debug.Log(actor.ActorInventory.Storage[1]);
            Debug.Log(actor.ActorInventory.Storage[2]);
            Debug.Log(actor.ActorInventory.Storage[3]);
            Debug.Log("StorageEnd");
        }
    }
}

using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor.U2D.Animation;
using UnityEngine;

namespace Assets.Scripts.Objects.Actors
{
    public static class ActionMachine
    {
        public static void Move(ActorController actor)
        {
            actor.RB.linearVelocity = new Vector2 ((int)actor.StateMachine.CurrentDirection * actor.Speed, actor.RB.linearVelocity.y);
        }

        public static void Stop(ActorController actor)
        {
            actor.RB.linearVelocity = Vector2.zero;
        }

        public static void Jump(ActorController actor)
        {
            actor.StateMachine.RegisterStateChange(ActorState.Accelerating);

            int sleepTime = (int)(Time.fixedDeltaTime * 1000);
            actor.RB.linearVelocity = new Vector2(actor.RB.linearVelocity.x, actor.JumpForce);
        }

        private static async Task Accelerate(ActorController actor, int sleepTime, int repeatTime, Vector2 speed)
        {
            if (actor.StateMachine.CurrentState != ActorState.Decellerating)
            {
                Debug.Log("Jumping in state: " + actor.StateMachine.CurrentState);
                actor.RB.linearVelocity = speed;

                await Task.Delay(sleepTime);
                repeatTime -= sleepTime;
                if (repeatTime > 0) await Accelerate(actor, sleepTime, repeatTime, speed);
            }
        }

        public static void StopJump(ActorController actor)
        {
            actor.StateMachine.RegisterStateChange(ActorState.Decellerating);
            actor.RB.linearVelocity = new Vector2(actor.RB.linearVelocity.x, actor.RB.linearVelocity.y * 0.3f);
        }
    }
}

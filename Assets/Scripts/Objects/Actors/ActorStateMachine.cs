using System;
using System.Collections.Generic;

namespace Assets.Scripts.Objects.Actors
{
    public enum ActorState { Running, Standing, Jumping, Falling, Attacking, TakingDamage, Interacting, Dashing }
    public enum LookDirection { Left, Right, None }

    public class ActorStateMachine
    {
        private List<ActorState> _futureStates = new();
        private LookDirection _registeredDirection;

        public ActorState CurrentState { get; private set; }
        public ActorState LastState { get; private set; }

        public LookDirection CurrentDirection { get; private set; }
        public LookDirection LastDirection { get; private set; }


        public void RegisterStateChange(ActorState state)
        {
            _futureStates.Add(state);
        }
        public void RegisterStateChange(LookDirection lookDirection)
        {
            _registeredDirection = lookDirection;
        }
        public void UpdateState()
        {
            for (int i = _futureStates.Count - 1; i >= 0; i--)
            {
                if (CanBeApplied(_futureStates[i])) ChangeState(_futureStates[i]);
                if (CanBeApplied(_registeredDirection)) ChangeDirection(_registeredDirection);
            }
        }

        private bool CanBeApplied(ActorState actorState)
        {
            if (CurrentState == ActorState.TakingDamage && actorState != ActorState.Standing) return false;
            if (CurrentState == ActorState.Jumping && actorState != ActorState.Falling) return false;
            if (CurrentState == ActorState.Falling)
            {
                if (actorState != ActorState.Standing || actorState != ActorState.Running) return false;
            }
            return true;
        }
        private bool CanBeApplied(LookDirection direction)
        {
            if (CurrentState == ActorState.Dashing) return false;
            return true;
        }

        private void ChangeState(ActorState state)
        {
            LastState = CurrentState;
            CurrentState = state;
        }
        private void ChangeDirection(LookDirection lookDirection)
        {
            LastDirection = lookDirection;
            CurrentDirection = lookDirection;
            _registeredDirection = LookDirection.None;
        }
    }
}

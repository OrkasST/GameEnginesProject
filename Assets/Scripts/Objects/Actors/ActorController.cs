using UnityEngine;

namespace Assets.Scripts.Objects.Actors
{
    public class ActorController : MonoBehaviour
    {
        public ActorStateMachine StateMachine { get; private set; } = new();


    }
}

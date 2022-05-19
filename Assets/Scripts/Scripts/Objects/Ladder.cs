using UnityEngine;
using Gameplay.Interaction;

namespace GameWorld
{
    public class Ladder : MonoBehaviour
    {
        public Transform start;
        public Transform end;
        public Transform enter;
        public Transform exit;

        private Interactable interactable;

        private void Start()
        {
            interactable = GetComponent<Interactable>();
        }

        public void Exit()
        {
            interactable.active = true;
        }

        public void Climb()
        {
            Gameplay.Movement.UseLadder(this);
        }
    }
}
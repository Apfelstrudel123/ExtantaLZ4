using UnityEngine;
namespace GameWorld
{
    public class Ladder : MonoBehaviour
    {
        public Transform start;
        public Transform end;
        public Transform enter;
        public Transform exit;

        private Interactable interact;

        private void Start()
        {
            interact = GetComponent<Interactable>();
        }

        public void Exit()
        {
            interact.active = true;
        }

        public void Climb()
        {
            Gameplay.Movement.UseLadder(this);
        }
    }
}
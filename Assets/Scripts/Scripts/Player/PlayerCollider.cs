using UnityEngine;
namespace Gameplay
{
    public class PlayerCollider : MonoBehaviour
    {
        public static PlayerCollider instance;
        [HideInInspector] public Vector3 lastPos;
        [SerializeField] private LayerMask layer = new LayerMask();

        private void Awake()
        {
            instance = this;
        }
        private void Setup()
        {
            lastPos = transform.position;
        }

        private void OnTriggerEnter(Collider other)
        {
            Player.active.Collide(other);
        }

        public void CheckCol()
        {
            Vector3 dir = lastPos - transform.position;
            if (Physics.Raycast(transform.position, dir, out RaycastHit hit, dir.magnitude, layer, QueryTriggerInteraction.Collide))
            {
                Player.active.Collide(hit.collider);
            }
        }
    }
}
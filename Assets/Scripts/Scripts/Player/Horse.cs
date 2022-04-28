using UnityEngine;
namespace Gameplay
{
    public class Horse : MonoBehaviour
    {
        [HideInInspector] public bool isUsed;

        public Transform playerPos;
        public Transform exitPos;

        [SerializeField] private float maxHealth = 100f;

        private float health;

        [HideInInspector] public Rigidbody rig;
        private Interactable interaction;

        void Start()
        {
            rig = GetComponent<Rigidbody>();
            interaction = GetComponent<Interactable>();
            rig.centerOfMass += new Vector3(0, -1, 0);

            health = maxHealth;
        }

        private void Update()
        {
            if (isUsed)
            {

            }
        }

        public void TakeDamage(float amt)
        {
            health -= amt;

            if (health < 0)
            {
                //GetComponentInChildren<Interactable>().interactName = null;
                //StartCoroutine(InteractableManager.instance.CreateExplosion(transform, InteractableManager.ExplosionType.Vehicle));
            }
        }

        public void Mount()
        {
            Movement.MountHorse(this);
        }

        public void OnLeave()
        {
            interaction.active = true;
        }
    }
}
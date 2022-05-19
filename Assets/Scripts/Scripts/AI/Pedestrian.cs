using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.HighDefinition;
using System.Collections;
using System.Collections.Generic;
using Gameplay.Interaction;

namespace AI
{
    public class Pedestrian : Human
    {
        #region General

        private NPCState state;
        private NavMeshAgent nav;
        private AudioSource audioSource;
        private CapsuleCollider col = null;
        private Animator animator = null;
        [Header("General")]
        public AIType type;

        public override NPC Init(AIWaypoint firstWaypoint, bool mission, bool isLoc, Transform loc)
        {
            audioSource = GetComponent<AudioSource>();
            nav = GetComponent<NavMeshAgent>();
            col = GetComponent<CapsuleCollider>();
            health = GetComponent<ObjectHealth>();

            Collider[] cols = GetComponentsInChildren<Collider>();
            foreach (Collider c in cols)
            {
                if (c.gameObject.layer == 19)
                {
                    c.GetComponent<AIHitbox>().parent = this;
                    c.attachedRigidbody.constraints = RigidbodyConstraints.FreezeAll;
                    ragdollParts.Add(c);
                }
            }

            float r = Random.Range(speedMinMultiplier, speedMaxMultiplier);
            walkSpeed *= r;
            runSpeed *= r;
            nav.speed = walkSpeed;
            animator.SetFloat("WalkSpeed", r);

            currentWaypoint = firstWaypoint;
            positiveDirection = Random.value > 0.5f;
            SetDestination(firstWaypoint.GetPosition());

            EnterState(typeof(NPCWalk), null, null);

            health.ResetHealth();
            health.onDamage += TakeDamage;

            return this;
        }

        public void CreateBody(GameObject prefab)
        {
            PedestrianBody b = Instantiate(prefab, transform).GetComponent<PedestrianBody>();
            animator = b.animator;
            sight = b.sight;
            b.CreateRandom();
        }

        public override void Update()
        {
            if (Core.Game.GameState == GameState.Active)
            {
                state.OnStateUpdate();
            }

            if ((Gameplay.Player.Transform.position - transform.position).sqrMagnitude > AI.settings.typeSettings[(int)type].removeDistance)
            {
                AI.RemoveInstance(this);
                CleanUp();
                return;
            }
        }

        public override void PlayAudio(AudioType audioClip)
        {
            throw new System.NotImplementedException();
        }

        #endregion

        #region Moving

        [HideInInspector] public AIWaypoint currentWaypoint;
        private bool positiveDirection = true;
        private AIWaypoint lastWaypoint;
        [HideInInspector] public float currentWaitTime;
        [Space()]
        [Header("Movement")]
        public float walkSpeed = 1f;
        public float runSpeed = 1f;
        public float speedMinMultiplier;
        public float speedMaxMultiplier;

        public override void SetDestination(Vector3 destination)
        {
            if (destination == null)
            {
                Debug.Log(gameObject.name);
                return;
            }
            nav.SetDestination(destination);
        }
        public override void NewWaypoint()
        {
            if (currentWaypoint == null)
            {
                return;
            }
            if (currentWaypoint.minWaitTime > 0 && !currentWaypoint.isUsed)
            {
                currentWaypoint.isUsed = true;
                currentWaitTime = UnityEngine.Random.Range(currentWaypoint.minWaitTime, currentWaypoint.maxWaitTime);
                nav.isStopped = true;
                animator.SetBool("Walking", false);
                return;
            }

            if (currentWaitTime > 0)
            {
                currentWaitTime = 0;
                currentWaypoint.isUsed = false;
                animator.SetBool("Walking", true);
                nav.isStopped = false;
            }

            AIWaypoint w = GetNewWaypoint(currentWaypoint, lastWaypoint, positiveDirection, out bool newDirection);
            if (w != null)
            {
                positiveDirection = newDirection;
                lastWaypoint = currentWaypoint;
                SetDestination(currentWaypoint.GetPosition());
            }
            else { Debug.Log("Couldn´t get new Waypoint!"); }
        }

        #endregion

        #region Detection

        [Space()]
        [Header("Detection")]
        public float sightRange = 20f;
        [SerializeField] private GameObject sight;

        public override SightResult CheckSight(out GameObject detected)
        {
            detected = null;
            return SightResult.None;
        }

        #endregion

        #region States

        public override void EnterState(System.Type _state, NPCTask firstTask, object arg)
        {
            if (state != null)
            {
                state.OnStateLeave();
            }
            state = (NPCState)System.Activator.CreateInstance(_state);
            state.OnStateEnter(this, firstTask, arg);
        }

        #endregion

        #region Health

        [HideInInspector] public ObjectHealth health;
        private readonly List<Collider> ragdollParts = new List<Collider>();
        [HideInInspector] public Transform lastHitbox;
        private DecalProjector blood;
        [Space()]
        [Header("Health & Economy")]
        public int maxHealth;
        [SerializeField] private LayerMask bloodSurf;
        [SerializeField] private GameObject bloodSplatter;
        [SerializeField] private int[] lootIDs;
        [SerializeField] private int[] count;

        public override void Loot()
        {
            for (int i = 0; i < lootIDs.Length; i++)
            {
                Gameplay.Inventory.AddItem(lootIDs[i], count[i]);
            }
            Gameplay.Inventory.Transaction(25);
            Core.Game.PlayerData.abilityPoints++;
        }
        public override void TakeDamage(ObjectHealth.DamageType origin)
        {
            if (state.GetType() == typeof(NPCDead))
            { return; }
        }
        
        public override void OnDie()
        {
            if (state.GetType() == typeof(NPCDead))
            { return; }
            StartCoroutine(Die());
        }

        public override IEnumerator Die()
        {
            GetComponent<Interactable>().active = true;

            col.isTrigger = true;
            nav.enabled = false;
            animator.enabled = false;
            AI.RemoveInstance(this);
            EnterState(typeof(NPCDead), null, null);

            foreach (Collider c in ragdollParts)
            {
                c.attachedRigidbody.constraints = RigidbodyConstraints.None;
                c.attachedRigidbody.useGravity = true;
            }

            yield return new WaitForSeconds(3);

            if (Physics.Raycast(lastHitbox.position, Vector3.down, out RaycastHit groundHit, bloodSurf))
            {
                blood = Instantiate(bloodSplatter, groundHit.point + new Vector3(0, -1.2f, 0), Quaternion.LookRotation(groundHit.normal), transform).GetComponent<DecalProjector>();
                blood.fadeFactor = 0;
                while (blood.fadeFactor < 1f)
                {
                    blood.fadeFactor += Time.deltaTime / 2;
                }
            }
        }


        #endregion

        #region Abstract Variables

        public override NavMeshAgent NavAgent()
        {
            return nav;
        }
        public override AIWaypoint CurrentWaypoint()
        {
            return currentWaypoint;
        }
        public override AIType Type()
        {
            return type;
        }
        public override Animator Animator()
        {
            return animator;
        }

        public override ObjectHealth Health()
        {
            return health;
        }

        #endregion
    }
}
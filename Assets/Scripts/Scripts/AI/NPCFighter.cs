using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.HighDefinition;
using SensorToolkit;
using System;
using System.Collections;
using System.Collections.Generic;
using ScriptableObjects;
using Gameplay;
using Gameplay.Interaction;

namespace AI
{
    public class NPCFighter : Human
    {
        #region General

        [HideInInspector] public NPCState state;
        private bool resumed = true;
        private NavMeshAgent nav;
        [HideInInspector] public bool isMission;
        [HideInInspector] public bool isLocation;
        [HideInInspector] public Transform location;
        private CapsuleCollider col = null;
        [Header("General")]
        public AIType type;
        [SerializeField] private Animator animator;
        [HideInInspector] public GameObject target;

        public override NPC Init(AIWaypoint firstWaypoint, bool mission, bool isLoc, Transform loc)
        {
            isMission = mission;
            isLocation = isLoc;
            location = loc;

            audioSource = GetComponent<AudioSource>();
            nav = GetComponent<NavMeshAgent>();
            col = GetComponent<CapsuleCollider>();
            health = GetComponent<ObjectHealth>();

            Collider[] cols = GetComponentsInChildren<Collider>();
            foreach (Collider c in cols)
            {
                if (c.TryGetComponent<AIHitbox>(out AIHitbox h))
                {
                    h.parent = this;
                    c.attachedRigidbody.constraints = RigidbodyConstraints.FreezeAll;
                    ragdollParts.Add(c);
                }
            }

            float r = UnityEngine.Random.Range(speedMinMultiplier, speedMaxMultiplier);
            walkSpeed *= r;
            runSpeed *= r;
            nav.speed = walkSpeed;

            currentWaypoint = firstWaypoint;
            positiveDirection = UnityEngine.Random.value > 0.5f;
            EnterState(typeof(NPCPatrol), null, currentWaypoint);
            ammo = weaponData.magSize;

            health.ResetHealth();
            health.onDamage += TakeDamage;

            return this;
        }

        public override void Update()
        {
            if (Core.Game.GameState == GameState.Active)
            {
                if(!resumed)
                { if (nav.enabled) { nav.velocity = lastVelocity; nav.isStopped = lastStopped; } resumed = true; }
                state.OnStateUpdate();
            }
            else
            {
                if(resumed)
                {
                    resumed = false;
                    if (nav.enabled)
                    {
                        lastVelocity = nav.velocity;
                        nav.velocity = Vector3.zero;
                        lastStopped = nav.isStopped;
                        nav.isStopped = true;
                    }
                }
            }

            if ((Player.Transform.position - transform.position).sqrMagnitude > AI.settings.typeSettings[(int)type].removeDistance && !isLocation && !isMission)
            {
                AI.RemoveInstance(this);
                CleanUp();
                return;
            }
        }

        #endregion

        #region Moving

        [HideInInspector] public AIWaypoint currentWaypoint;
        private bool positiveDirection = true;
        private AIWaypoint lastWaypoint;
        private Vector3 lastVelocity;
        private bool lastStopped;
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
            if(currentWaypoint.minWaitTime > 0 && !currentWaypoint.isUsed)
            {
                currentWaypoint.isUsed = true;
                currentWaitTime = UnityEngine.Random.Range(currentWaypoint.minWaitTime, currentWaypoint.maxWaitTime);
                nav.isStopped = true;
                animator.SetBool("Walking", false);
                return;
            }

            if(currentWaitTime > 0)
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
        [SerializeField] private GameObject sight = null;
        [SerializeField] private LayerMask sightLayers;
        [SerializeField] private float minVis = 0.05f;

        public override SightResult CheckSight(out GameObject detected)
        {
            float angle = Vector3.Angle(transform.forward, Player.Transform.position - transform.position);

            if (angle < 50)
            {
                float playerVis = 0;

                LOSTargets los = Player.active.GetComponent<LOSTargets>();

                for(int i = 0; i < los.Targets.Length; i++)
                {
                    if(Physics.Raycast(sight.transform.position, los.Targets[i].position - sight.transform.position, out RaycastHit hit, sightRange, sightLayers, QueryTriggerInteraction.Ignore))
                    {
                        if (hit.transform.TryGetComponent<Player>(out _))
                        {
                            playerVis++;
                        }
                    }             
                }

                playerVis /= los.Targets.Length;

                if (playerVis >= minVis)
                {
                    detected = Player.active.gameObject;
                    return SightResult.Player;
                }
            }

            /*foreach (GameObject g in sensor.DetectedObjectsOrderedByDistance)
            {
                if (g.CompareTag("Player"))
                {
                    detected = g;
                    return SightResult.Player;
                }
                else if (g.CompareTag("Friendly") || g.CompareTag("Enemy"))
                {
                    NPCFighter e = g.transform.GetComponentInParent<NPCFighter>();
                    if (e.state.GetType() == typeof(NPCDead))
                    {
                        if (e.faction == faction)
                        {
                            firstBody = g;
                        }
                    }
                    else
                    {
                        if (e.faction != faction)
                        {
                            detected = g;
                            return SightResult.Enemy;
                        }
                    }
                }
            }
            if (firstBody != null)
            {
                detected = firstBody;
                return SightResult.Body;
            }*/
            detected = null;
            return SightResult.None;
        }

        #endregion

        #region States

        public int searchTime;
        public override void EnterState(Type _state, NPCTask firstTask, object arg)
        {
            if (state != null)
            {
                state.OnStateLeave();
            }
            state = (NPCState)Activator.CreateInstance(_state);
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
                Inventory.AddItem(lootIDs[i], count[i]);
            }
            Inventory.Transaction(25);
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
            //Rigidbody r = Instantiate(Player.instance.weaponOptics[weaponData.weaponID], weapon.transform.position, weapon.transform.rotation).GetComponent<Rigidbody>();
            //r.velocity = weapon.GetComponent<Rigidbody>().velocity;
            Destroy(weapon);

            /*if (isMission)
            {
                GameManager.instance.currentMission.checkpoints[GameManager.instance.currentMission.currentCheckpoint].Kill();
            }*/
            if (isLocation)
            {
                location.GetComponent<GameWorld.Location>().Kill();
            }

            GetComponent<Interactable>().active = true;

            col.enabled = false;
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
            /*
            if (Physics.Raycast(lastHitbox.position, Vector3.down, out RaycastHit groundHit, bloodSurf))
            {
                blood = Instantiate(bloodSplatter, groundHit.point + new Vector3(0, -1.2f, 0), Quaternion.LookRotation(groundHit.normal), transform).GetComponent<DecalProjector>();
                blood.fadeFactor = 0;
                while (blood.fadeFactor < 1f)
                {
                    blood.fadeFactor += Time.deltaTime / 2;
                }
            }*/
        }


        #endregion

        #region Combat

        [HideInInspector] public int ammo;
        [Space()]
        [Header("Shooting")]
        public WeaponData weaponData;
        public GameObject weapon;
        public GameObject ammoDrop;
        public float shootDistance;
        public float minShootTime = 0.4f;
        public float maxShootTime = 1f;
        public float damageMultiplier = 0.5f;
        [SerializeField] private Transform bulletSpawnPoint = null;


        public void Shoot()
        {
            animator.SetTrigger("Shoot");
            ammo--;
            PlayAudio(AudioType.Shoot);
            Vector3 origin = bulletSpawnPoint.position;
            Vector3 dir = Player.Transform.position - bulletSpawnPoint.position;
            float dist = dir.magnitude / 20;
            dir += new Vector3(UnityEngine.Random.Range(-dist, dist), UnityEngine.Random.Range(-dist / 2, dist / 2), UnityEngine.Random.Range(-dist, dist));
            if (Physics.Raycast(origin, dir, out RaycastHit hit, AI.settings.bulletDistance, AI.settings.shootMask, QueryTriggerInteraction.Collide))
            {
                if (hit.collider.transform.CompareTag("Player"))
                {
                    Player.health.AddHealth(-weaponData.damage * damageMultiplier, ObjectHealth.DamageType.Bullet);
                    return;
                }
                else if (hit.collider.TryGetComponent<AIHitbox>(out AIHitbox h))
                {
                    h.TakeDamage((int)(weaponData.damage * damageMultiplier));
                    Core.ObjectPool.Request("ImpactBlood", hit.point, Quaternion.LookRotation(hit.normal), null);
                    return;
                }

                if (hit.collider.transform.TryGetComponent<ObjectMaterial>(out ObjectMaterial mat))
                {
                    Vector3 normal = hit.normal;
                    Vector3 point = hit.point;
                    string s = mat.materialType;
                    if (s == "Terrain")
                    {
                        s = "ImpactConcrete";
                    }
                    else
                    {
                        string t = s;
                        s = "Impact" + t;
                    }
                    Core.ObjectPool.Request(s, point, Quaternion.LookRotation(normal), null);
                }
            }
        }

        public void Reload()
        {
            EnterState(typeof(NPCReload), null, null);
            animator.SetTrigger("Reload");
            PlayAudio(AudioType.Reload);
        }

        public void OnReloadFinished()
        {
            ammo = weaponData.magSize;
        }

        #endregion

        #region Audio

        private AudioSource audioSource;
        public override void PlayAudio(AudioType audioType)
        {
            switch (audioType)
            {
                case AudioType.Reload:
                    audioSource.PlayOneShot(AI.settings.reloadClip);
                    break;
                case AudioType.Shoot:
                    audioSource.PlayOneShot(AI.settings.shootClip);
                    break;
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
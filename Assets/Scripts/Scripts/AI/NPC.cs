using UnityEngine;
using UnityEngine.AI;
using System.Collections;
namespace AI
{
    #region NPC Base

    public class NPC : MonoBehaviour
    {
        #region General

        public virtual NPC Init(AIWaypoint firstWaypoint, bool mission, bool isLoc, Transform loc) { return null; }
        public virtual void Update() { }
        public void CleanUp()
        {
            if (this != null)
            {
                Destroy(gameObject);
            }
        }

        #endregion

        #region Inherited

        public virtual void SetDestination(Vector3 destination) { }
        public virtual void NewWaypoint() { }

        public virtual SightResult CheckSight(out GameObject detected) { detected = null; return (SightResult)1000; }
        public virtual void EnterState(System.Type _state, NPCTask firstTask, object arg) { }

        public virtual void Loot() { }
        public virtual void TakeDamage(ObjectHealth.DamageType origin) { }
        public virtual void OnDie() { }
        public virtual IEnumerator Die() { yield return null; }
        public virtual void PlayAudio(AudioType audioType) { }

        public virtual NavMeshAgent NavAgent() { return null; }
        public virtual AIWaypoint CurrentWaypoint() { return null; }
        public virtual Animator Animator() { return null; }
        public virtual AIType Type() { return (AIType)1000; }
        public virtual ObjectHealth Health() { return null; }

        #endregion

        #region Movement

        public static AIWaypoint GetNewWaypoint(AIWaypoint currentWaypoint, AIWaypoint lastWaypoint, bool positiveDirection, out bool newDirection)
        {
            float r = Random.value;
            float add = 0f;

            for (int i = 0; i < currentWaypoint.branches.Count; ++i)
            {
                add += currentWaypoint.branches[i].ratio;
                if (r <= add)
                {
                    if (currentWaypoint.branches[i].waypoint == lastWaypoint && !currentWaypoint.branches[i].allowBranchingBack)
                    {
                        break;
                    }
                    newDirection = true;
                    return currentWaypoint.branches[i].waypoint;
                }
            }

            if ((positiveDirection && currentWaypoint.nextWaypoint != null) || (!positiveDirection && currentWaypoint.previousWaypoint == null))
            {
                newDirection = true;
                return currentWaypoint.nextWaypoint;
            }
            else
            {
                newDirection = false;
                return currentWaypoint.previousWaypoint;

            }
        }

        #endregion
    }

    #endregion

    #region Human

    public class Human : NPC
    {

    }

    #endregion

    #region Animal

    public class Animal : NPC
    {

    }

    #endregion

    // States & Tasks

    #region State & Task Base || NPCDead State

    public abstract class NPCState
    {
        public abstract void OnStateEnter(NPC _npc, NPCTask firstTask, object arg);
        public abstract void OnStateUpdate();
        public virtual void OnStateLeave() { }
    }
    public abstract class NPCTask
    {
        public abstract void StartTask(NPCState _state);
        public abstract bool UpdateTask(out NPCTask nextTask, out System.Type nextState);
        public virtual void OnTaskDone() { }
    }

    public class NPCDead : NPCState
    {
        private NPC npc;
        private float destroyDist;
        public override void OnStateEnter(NPC _npc, NPCTask firstTask, object nullArg)
        {
            npc = _npc;
            destroyDist = (float)AI.settings.typeSettings[(int)npc.Type()].deadRemoveDistance;
        }
        public override void OnStateUpdate()
        {
            if ((npc.transform.position - Gameplay.Player.Transform.position).sqrMagnitude > destroyDist)
            {
                npc.CleanUp();
            }
        }
    }

    #endregion

    #region Pedestrian

    public class NPCWalk : NPCState
    {
        private Pedestrian npc;
        private float resetTime;

        override public void OnStateEnter(NPC _npc, NPCTask firstTask, object obj)
        {
            npc = (Pedestrian)_npc;
            npc.NewWaypoint();

            npc.NavAgent().isStopped = false;
            npc.NavAgent().speed = npc.walkSpeed;

            npc.Animator().SetBool("Walking", true);
            npc.Animator().SetBool("Running", false);
        }

        override public void OnStateUpdate()
        {
            SightResult sight = npc.CheckSight(out GameObject detected);

            if (sight == SightResult.Player)
            {

            }
            else if (sight == SightResult.Enemy)
            {

            }
            else if (sight == SightResult.Body)
            {
                if (npc.Type() != AIType.Pedestrian)
                {
                    //npc.EnterState(typeof(NPCInspect), detected, npc.GetComponent<NPCFighter>().walkSpeed);
                }
            }

            resetTime += Time.deltaTime;

            if (npc.NavAgent().remainingDistance <= 0.8f && resetTime > 1f)
            {
                resetTime = 0f;
                npc.NewWaypoint();
            }
        }
    }

    #endregion

    #region Combat

    public class NPCCombat : NPCState
    {
        public NPCFighter npc;
        private NPCTask currentTask;

        override public void OnStateEnter(NPC _npc, NPCTask firstTask, object obj)
        {
            npc = (NPCFighter)_npc;

            if (firstTask != null)
            {
                currentTask = firstTask;
                currentTask.StartTask(this);
            }
            else
            {
                if ((npc.target.transform.position - npc.transform.position).sqrMagnitude <= npc.shootDistance)
                {
                    if (npc.ammo > 0)
                    {
                        currentTask = new NPCShoot();
                        currentTask.StartTask(this);
                    }
                    else
                    {
                        currentTask = new NPCReload();
                        currentTask.StartTask(this);
                    }
                }
                else
                {
                    currentTask = new NPCChase();
                    currentTask.StartTask(this);
                }
            }
        }

        override public void OnStateUpdate()
        {
            if (Core.Game.GameState != GameState.Active || Core.Game.PlayerStatus != PlayerStatus.Alive)
            { return; }

            if (currentTask != null)
            {
                if (currentTask.UpdateTask(out NPCTask nextTask, out System.Type nextState))
                {
                    currentTask.OnTaskDone();
                    if (nextState != null)
                    {
                        npc.EnterState(nextState, nextTask, null);
                    }
                    else if (nextTask != null)
                    {
                        currentTask = nextTask;
                        nextTask.StartTask(this);
                    }
                    else
                    {
                        currentTask = null;
                    }
                }
            }

            if (currentTask == null)
            {
                SightResult sight = npc.CheckSight(out GameObject detected);

                if (sight == SightResult.None || sight == SightResult.Body)
                {
                    currentTask = new NPCSearch();
                    currentTask.StartTask(this);
                    return;
                }

                if ((npc.target.transform.position - npc.transform.position).sqrMagnitude <= npc.shootDistance)
                {
                    if (npc.ammo > 0)
                    {
                        currentTask = new NPCShoot();
                        currentTask.StartTask(this);
                    }
                    else
                    {
                        currentTask = new NPCReload();
                        currentTask.StartTask(this);
                    }
                }
                else
                {
                    currentTask = new NPCChase();
                    currentTask.StartTask(this);
                }
            }
        }
    }

    public class NPCReload : NPCTask
    {
        private float reloadTimer;
        private NPCCombat state;

        override public void StartTask(NPCState _state)
        {
            state = (NPCCombat)_state;
            reloadTimer = state.npc.weaponData.reloadTime;

            state.npc.NavAgent().isStopped = true;
            state.npc.Animator().SetBool("Running", false);
            state.npc.Animator().SetBool("Walking", false);
            state.npc.Animator().SetBool("Aiming", false);
        }
        override public bool UpdateTask(out NPCTask nullTask, out System.Type nullState)
        {
            nullTask = null;
            nullState = null;
            reloadTimer -= Time.deltaTime;
            if (reloadTimer > 0)
            {
                return false;
            }
            state.npc.OnReloadFinished();
            return true;
        }
    }
    public class NPCShoot : NPCTask
    {
        private NPCCombat state;

        private float shootTimer = 0f;
        private float nextShootTimer = 0f;

        override public void StartTask(NPCState _state)
        {
            state = (NPCCombat)_state;
            shootTimer = 0f;
            nextShootTimer = state.npc.minShootTime;

            state.npc.NavAgent().isStopped = true;
            state.npc.Animator().SetBool("Running", false);
            state.npc.Animator().SetBool("Walking", false);
            state.npc.Animator().SetBool("Aiming", true);
        }
        override public bool UpdateTask(out NPCTask nextTask, out System.Type nextState)
        {
            nextTask = null;
            nextState = null;

            /* Face Player
            Vector3 forward = state.npc.transform.forward;
            forward.y = 0;

            Vector3 playerPos = Player.position - state.npc.transform.position;
            playerPos.y = 0;

            double dot = Vector3.Dot(forward, playerPos);
            float angle = (float)Math.Acos(dot / (forward.magnitude * playerPos.magnitude));
            npc.transform.rotation = Quaternion.Euler(0f, angle, 0f);*/

            SightResult sight = state.npc.CheckSight(out GameObject detected);

            if (sight != SightResult.Player && sight != SightResult.Enemy)
            {
                nextState = typeof(NPCSearch);
                return true;
            }
            else if ((state.npc.target.transform.position - state.npc.transform.position).sqrMagnitude <= state.npc.shootDistance)
            {
                state.npc.target = detected;
                shootTimer += Time.deltaTime;
                if (shootTimer > nextShootTimer)
                {
                    if (state.npc.ammo > 0)
                    {
                        shootTimer = 0;
                        nextShootTimer = Random.Range(state.npc.minShootTime, state.npc.maxShootTime);
                        state.npc.Shoot();
                    }
                    else
                    {
                        nextTask = new NPCReload();
                        return true;
                    }
                }
                return false;
            }

            state.npc.target = detected;
            nextTask = new NPCChase();
            return true;
        }
    }
    public class NPCChase : NPCTask
    {
        private NPCCombat state;
        private float timer;

        override public void StartTask(NPCState _state)
        {
            state = (NPCCombat)_state;
            state.npc.NavAgent().isStopped = false;
            state.npc.NavAgent().speed = state.npc.runSpeed;
            state.npc.NavAgent().SetDestination(state.npc.target.transform.position);

            state.npc.Animator().SetBool("Running", true);
            state.npc.Animator().SetBool("Walking", false);
            state.npc.Animator().SetBool("Aiming", false);

            timer = 0;
        }

        override public bool UpdateTask(out NPCTask nextTask, out System.Type nextState)
        {
            nextTask = null;
            nextState = null;

            SightResult sight = state.npc.CheckSight(out GameObject detected);

            if (sight == SightResult.None)
            {
                nextTask = new NPCSearch();
                return true;
            }
            else if (sight == SightResult.Player || sight == SightResult.Enemy)
            {
                float dist = (detected.transform.position - state.npc.transform.position).sqrMagnitude;
                if (dist < state.npc.shootDistance)
                {
                    nextTask = new NPCShoot();
                    return true;
                }
            }

            timer++;
            if (timer > 150)
            {
                timer = 0;
                state.npc.NavAgent().SetDestination(state.npc.target.transform.position);
            }
            return false;
        }
    }
    public class NPCSearch : NPCTask
    {
        private NPCCombat state;

        private float searchedTime;

        override public void StartTask(NPCState _state)
        {
            state = (NPCCombat)_state;

            state.npc.NavAgent().isStopped = false;
            searchedTime = 0;
            state.npc.NavAgent().speed = state.npc.walkSpeed;
            state.npc.NavAgent().SetDestination(state.npc.target.transform.position);

            state.npc.Animator().SetBool("Walking", true);
            state.npc.Animator().SetBool("Running", false);
            state.npc.Animator().SetBool("Aiming", false);
        }

        override public bool UpdateTask(out NPCTask nextTask, out System.Type nextState)
        {
            nextState = null;
            SightResult sight = state.npc.CheckSight(out GameObject detected);

            if (sight == SightResult.Player || sight == SightResult.Enemy)
            {
                nextTask = new NPCChase();
                state.npc.target = detected;
                return true;
            }

            searchedTime += Time.deltaTime;
            if (searchedTime > state.npc.searchTime)
            {
                nextState = typeof(NPCPatrol);
                nextTask = null;
                return true;
            }

            nextTask = null;
            return false;
        }
    }

    #endregion

    #region Non-Alert

    public class NPCPatrol : NPCState
    {
        public NPCFighter npc;
        private NPCTask currentTask;
        private float waitTime;

        override public void OnStateEnter(NPC _npcFighter, NPCTask firstTask, object waypoint)
        {
            npc = (NPCFighter)_npcFighter;

            npc.NavAgent().isStopped = false;
            npc.Animator().SetBool("Walking", true);
            npc.Animator().SetBool("Running", false);
            npc.Animator().SetBool("Aiming", false);
            npc.SetDestination(((AIWaypoint)waypoint).GetPosition());

            if (firstTask != null)
            {
                currentTask = firstTask;
                currentTask.StartTask(this);
            }
        }

        override public void OnStateUpdate()
        {
            if (currentTask != null)
            {
                if (currentTask.UpdateTask(out NPCTask nextTask, out System.Type nextState))
                {
                    currentTask.OnTaskDone();
                    if (nextState != null)
                    {
                        npc.EnterState(nextState, nextTask, null);
                    }
                    else if (nextTask != null)
                    {
                        currentTask = nextTask;
                        nextTask.StartTask(this);
                    }
                    else
                    {
                        currentTask = null;
                    }
                }
            }
            else
            {
                SightResult sight = npc.CheckSight(out GameObject detected);

                if (sight == SightResult.Player || sight == SightResult.Enemy)
                {
                    npc.target = detected;
                    npc.EnterState(typeof(NPCCombat), null, null);
                    return;
                }
                else if (sight == SightResult.Body)
                {
                    npc.target = detected;
                    currentTask = new NPCInspect();
                    currentTask.StartTask(this);
                    return;
                }

                waitTime += Time.deltaTime;

                if (npc.NavAgent().remainingDistance <= 0.8f && waitTime > 1f && waitTime >= npc.currentWaitTime)
                {
                    waitTime = 0f;
                    npc.NewWaypoint();
                }
            }
        }
    }

    public class NPCInspect : NPCTask
    {
        private NPCPatrol state;
        private float timeInspected;

        public override void StartTask(NPCState _state)
        {
            state = (NPCPatrol)_state;

            Debug.Log(state.npc.name + "is inspecting body" + (state.npc.target).name);
            state.npc.NavAgent().isStopped = false;
            state.npc.NavAgent().speed = state.npc.walkSpeed;
            state.npc.Animator().SetBool("Running", false);
            state.npc.Animator().SetBool("Walking", true);
            state.npc.Animator().SetBool("Aiming", false);

            state.npc.SetDestination(state.npc.target.transform.position);
            timeInspected = 0;
        }

        public override bool UpdateTask(out NPCTask nextTask, out System.Type nextState)
        {
            timeInspected += Time.deltaTime;
            SightResult sight = state.npc.CheckSight(out GameObject detected);

            if (sight == SightResult.Player || sight == SightResult.Enemy)
            {
                nextState = typeof(NPCCombat);
                nextTask = new NPCChase();
                state.npc.target = detected;
                return true;
            }
            else if (sight == SightResult.Body)
            {
                //npc.EnterState(typeof(NPCInspect), detected, nf.walkSpeed);
                //return;
            }

            nextState = null;
            nextTask = null;

            if (timeInspected > 10f)
            {
                return true;
            }

            return false;
        }
    }

    #endregion
}
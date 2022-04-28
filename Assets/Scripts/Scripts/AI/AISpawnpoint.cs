using UnityEngine;
namespace AI
{
    public class AISpawnpoint : MonoBehaviour
    {
        public AIType type;

        [SerializeField] private bool isMission = false;
        [SerializeField] private bool isLocation = false;
        public bool blockSpawn = false;
        public float minDistance;
        public float maxDistance;
        public AIWaypoint firstWaypoint;

        [HideInInspector] public Transform location = null;
        [HideInInspector] public bool added = false;
        private float timer = 0f;

        private void Start()
        {
            minDistance = minDistance * minDistance;
            maxDistance = maxDistance * maxDistance;
            if (isLocation)
            {
                blockSpawn = true;
            }
        }

        public void CheckSpawn(bool useMinDist)
        {
            if (blockSpawn)
            {
                return;
            }      
            float dist = (transform.position - Gameplay.Player.Transform.position).sqrMagnitude;
            if (dist < maxDistance)
            {
                if (!useMinDist)
                {
                    added = true;
                    AI.spawnable.Add(this);
                }
                else
                {
                    if (dist > minDistance && !added)
                    {
                        added = true;
                        AI.spawnable.Add(this);
                    }
                    else if (added && dist < minDistance)
                    {
                        added = false;
                        AI.spawnable.Remove(this);
                    }
                }
            }
            else if (added)
            {
                added = false;
                AI.spawnable.Remove(this);
            }
        }

        private void Update()
        {
            if(Core.Game.GameState != GameState.Active || Core.Game.PlayerStatus != PlayerStatus.Alive || isMission || isLocation)
            {
                return;
            }

            if (timer < 5f)
            {
                timer += Time.deltaTime;
            }
            else
            {
                timer = 0;
                CheckSpawn(true);
            }
        }

        public NPC Spawn()
        {
            return AI.SpawnInstance(type, transform).Init(firstWaypoint, isMission, isLocation, location);
        }

        private void OnDrawGizmos()
        { }

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawCube(transform.position, new Vector3(0.4f, 1.8f, 0.4f));
        }
    }
}
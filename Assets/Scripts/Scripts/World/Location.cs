using System.Collections.Generic;
using UnityEngine;
using AI;
namespace GameWorld
{
    public class Location : MonoBehaviour
    {
        public enum LocationType
        {
            Outpost,
            Residence,
        }
        public enum LocationStatus
        {
            Locked,
            Free,
        }

        private int m_locationIndex;
        private bool active = false;
        [Header("General")]
        [SerializeField] private LocationType type;
        [SerializeField] private GameObject localMissions = null;
        [SerializeField] private float activeDist = 250f;

        private readonly List<NPCFighter> spawnedAiList = new();
        private AISpawnpoint[] spawnpoints;
        private int enemyCount;
        private int remaining;
        [Header("Outpost")]
        [SerializeField] private Transform enemies = null;

        private void Start()
        {
            activeDist = activeDist * activeDist;
            if (type == LocationType.Outpost)
            {
                spawnpoints = enemies.GetComponentsInChildren<AISpawnpoint>();
                enemyCount = spawnpoints.Length;
            }
        }
        private void Setup()
        {
            m_locationIndex = transform.GetSiblingIndex();
            if ((LocationStatus)Core.Game.PlayerData.locationStatus[m_locationIndex] != LocationStatus.Free)
            {
                localMissions.SetActive(false);
                if (spawnpoints != null)
                {
                    remaining = enemyCount;
                    foreach (AISpawnpoint ai in spawnpoints)
                    {
                        if (ai.type == AIType.Soldier)
                        {
                            ai.location = transform;
                        }
                    }
                }
            }
            else
            {
                foreach (AISpawnpoint ai in spawnpoints)
                {
                    if (ai.type == AIType.Soldier)
                    {
                        ai.gameObject.SetActive(false);
                    }
                }

            }
        }

        private void Update()
        {
            if (Core.Game.GameState != GameState.Active || Core.Game.PlayerStatus != PlayerStatus.Alive) { return; }
            if ((LocationStatus)Core.Game.PlayerData.locationStatus[m_locationIndex] == LocationStatus.Locked && enemyCount > 0)
            {
                CheckSpawn();
            }
        }

        public void Free()
        {
            Core.Game.PlayerData.locationStatus[m_locationIndex] = (int)LocationStatus.Free;
            localMissions.SetActive(true);
            active = false;
            foreach (AISpawnpoint ai in spawnpoints)
            {
                if (ai.type == AI.AIType.Soldier)
                {
                    ai.gameObject.SetActive(false);
                }
            }
            Core.GameManager.FreeOutpost(gameObject.name);
        }

        public void CheckSpawn()
        {
            float dist = (Gameplay.Player.Transform.position - transform.position).sqrMagnitude;
            if (dist < activeDist && !active)
            {
                Spawn();
            }
            else if (dist > activeDist && active)
            {
                Clear();
            }
        }

        public void Kill()
        {
            remaining--;
            if (remaining < 1 && active)
            {
                Free();
            }
        }

        private void Spawn()
        {
            active = true;
            remaining = enemyCount;
            foreach (AISpawnpoint ai in spawnpoints)
            {
                spawnedAiList.Add(ai.Spawn().GetComponent<NPCFighter>());
            }
        }

        private void Clear()
        {
            active = false;
            foreach (NPCFighter ai in spawnedAiList)
            {
                ai.CleanUp();
            }
            spawnedAiList.Clear();
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawIcon(transform.position, "Location", false);
        }
    }
}
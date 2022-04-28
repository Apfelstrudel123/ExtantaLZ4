using System.Collections.Generic;
using UnityEngine;
using ScriptableObjects;
namespace AI
{
    #region Enums

    public enum AIType
    {
        Soldier,
        Rebel,
        Pedestrian,
        PeacefulAnimal,
        AggressiveAnimal,
    }
    public enum AnimalType
    {
        Pig,
        Cow,
        Sheep,
        Goat,
        Bird,
        Eagle,
        Deer,
        Wolf,
        Fox,
    }
    public enum SightResult
    {
        None,
        Player,
        Enemy,
        Body,
        Animal,
    }
    public enum AudioType
    {
        Shoot,
        Reload,
    }

    #endregion

    public static class AI
    {
        public static AISettings settings;

        private static List<NPC>[] spawnedNPCs;

        public static List<AISpawnpoint> spawnable = new List<AISpawnpoint>();
        private static readonly List<GameObject> waypoints = new List<GameObject>();
        private static float timer = 0;
        private const float refreshTime = 15f;

        private static List<AISpawnpoint> nextSpawns = new List<AISpawnpoint>();
        private static int spawnIndex = 0;

        public static void Init(AISettings setts)
        {
            settings = setts;
            spawnedNPCs = new List<NPC>[setts.typeSettings.Length];

            for(int i = 0; i < spawnedNPCs.Length; ++i)
            {
                spawnedNPCs[i] = new List<NPC>();
                settings.typeSettings[i].deadRemoveDistance *= settings.typeSettings[i].deadRemoveDistance;
                settings.typeSettings[i].removeDistance *= settings.typeSettings[i].removeDistance;
            }

            timer = refreshTime;
        }

        private const int maxNewSpawns = 16;
        public static void Update()
        {
            timer += Time.deltaTime;

            if (timer >= refreshTime)
            {
                timer = 0f;
                nextSpawns.Clear();
                spawnIndex = 0;

                int newSpawns = 16;
                for (int i = 0; i < spawnable.Count && newSpawns < maxNewSpawns; ++i)
                {
                    if (spawnedNPCs[(int)spawnable[i].type].Count < settings.typeSettings[(int)spawnable[i].type].maxSpawnedLimit)
                    {
                        nextSpawns.Add(spawnable[i]);
                        newSpawns++;
                    }
                }

                if (nextSpawns.Count > 0)
                {
                    Core.GameManager.instance.InvokeRepeating(nameof(SpawnDelayed), 0f, 10f / nextSpawns.Count);
                }

            }
        }

        public static void SpawnDelayed()
        {
            nextSpawns[spawnIndex].Spawn();
            spawnIndex++;
            if (spawnIndex > nextSpawns.Count -1)
            { Core.GameManager.instance.CancelInvoke(nameof(SpawnDelayed)); }
        }

        public static void Clear()
        {
            spawnable.Clear();
            spawnIndex = 0;
            nextSpawns.Clear();
            Core.GameManager.instance.CancelInvoke(nameof(SpawnDelayed));

            for (int i = 0; i < spawnedNPCs.Length; ++i)
            {
                foreach(NPC n in spawnedNPCs[i])
                {
                    if (n != null)
                    {
                        n.CleanUp();
                    }
                }
                spawnedNPCs[i].Clear();
            }
        }

        public static void RemoveInstance(NPC npc)
        {
            spawnedNPCs[(int)npc.Type()].Remove(npc);
        }

        public static void SpawnIteration()
        {

            //Check all Spawnpoints
            spawnable.Clear();
            GameObject[] g = GameObject.FindGameObjectsWithTag("AISpawnpoint");
            foreach (GameObject go in g)
            {
                AISpawnpoint s = go.GetComponent<AISpawnpoint>();
                if (!s.blockSpawn)
                {
                    s.added = false;
                    if (s.type != AIType.Soldier)
                    {
                        s.CheckSpawn(false);
                    }
                    else
                    {
                        s.CheckSpawn(true);
                    }
                }
            }

            for (int i = 0; i < spawnable.Count; ++i)
            {
                if (spawnedNPCs[(int)spawnable[i].type].Count < settings.typeSettings[(int)spawnable[i].type].maxSpawnedLimit)
                {
                    spawnable[i].Spawn();
                }
            }
        }

        public static NPC SpawnInstance(AIType type, Transform pos)
        {
            
            if(type == AIType.Pedestrian)
            {
                Pedestrian p = Object.Instantiate(settings.neutralBase, pos.position, pos.rotation).GetComponent<Pedestrian>();
                p.CreateBody(settings.typeSettings[(int)AIType.Pedestrian].npcPrefabs
                    [Random.Range(0, settings.typeSettings[(int)AIType.Pedestrian].npcPrefabs.Length)]);
                spawnedNPCs[(int)AIType.Pedestrian].Add(p.GetComponent<NPC>());
                return p;
            }

            NPC n = Object.Instantiate(settings.typeSettings[(int)type].npcPrefabs
                [Random.Range(0, settings.typeSettings[(int)type].npcPrefabs.Length)], pos.position, pos.rotation).GetComponent<NPC>();
            spawnedNPCs[(int)type].Add(n);
            return n;
        }
    }
}
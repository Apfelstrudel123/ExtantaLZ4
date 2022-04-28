using UnityEngine;
using JBooth.MicroSplat;
using UnityEngine.VFX;
using Gameplay;
public static class FireSystem
{
    private enum FireState
    {
        Healthy,
        Burning,
        Burned,
    }

    private static int metersUntilShift;
    private static Vector2 lastPos;

    private static int lastX = 0;

    private static readonly FireCell[,] cells = new FireCell[128, 128];

    private static float maxFlammable;
    private static float maxEnergy;
    private static float spreadMultiplier;

    private static LayerMask fireLayers;
    private static Vector3 shift = Vector3.zero;

    public static void Init(float _fireSpread, float _maxFlammable, float _maxEnergy, int _metersUntilShift, LayerMask layers)
    {
        fireLayers = layers;
        metersUntilShift = _metersUntilShift;
        maxFlammable = _maxFlammable;
        maxEnergy = _maxEnergy;
        spreadMultiplier = _fireSpread;

        Rebuild((int)Player.Transform.position.x, (int)Player.Transform.position.z);
        //ShiftToPlayer();
    }

    public static void Rebuild(int xShift, int zShift)
    {
        shift = new Vector3(xShift, 0f, zShift);

        for (int x = 0; x < cells.GetLength(0); x++)
        {
            for (int z = 0; z < cells.GetLength(1); z++)
            {
                cells[x, z] = new FireCell();
                Vector3 pos = CellToWorldSpace(x, z);
                pos.y = 605f;
                cells[x, z].Init(pos);
            }
        }
    }

    public static void ShiftToPlayer()
    {
        Vector3 roundedPos = new Vector3((int)Player.Transform.position.x, 0f, (int)Player.Transform.position.z);
        Vector3 dif = roundedPos - shift;
        shift = roundedPos;

        for (int x = 0; x < cells.GetLength(0); x++)
        {
            for (int z = 0; z < cells.GetLength(1); z++)
            {
                if(x + dif.x < cells.GetLength(0) && x + dif.x >= 0 && z + dif.z < cells.GetLength(1) && z + dif.z >= 0)
                {
                    //cells[x + (int)dif.x, z + (int)dif.z].CopyTo(cells[x, z]);
                    cells[x, z].AssignNew(cells[x + (int)dif.x, z + (int)dif.z], CellToWorldSpace(x,z));
                }
                else
                {
                    cells[x, z].Init(CellToWorldSpace(x,z));
                }
            }
        }
    }

    public static void Update()
    {
        Vector3 playerPos = Player.Transform.position;
        if ((lastPos - new Vector2(playerPos.x, playerPos.z)).sqrMagnitude > metersUntilShift)
        {
            lastPos = new Vector2(playerPos.x, playerPos.z);
            ShiftToPlayer();
        }

        float time = Time.deltaTime * cells.GetLength(0);

        for (int z = 0; z < cells.GetLength(1); z++)
        {
            FireCell c = cells[lastX, z];
            if (c.state == FireState.Burning)
            {
                c.energy -= time;
                c.flammable -= time;
                if (c.flammable <= 0)
                {
                    c.Extinguish();
                }
                else
                {
                    float addEnergy = time * (c.energy / maxEnergy) * spreadMultiplier;
                    if(lastX < cells.GetLength(0) - 1)
                    {
                        UpdateCell(lastX + 1, z, addEnergy);
                    }
                    if (lastX > 0)
                    {
                        UpdateCell(lastX - 1, z, addEnergy);  
                    }
                    if (z < cells.GetLength(1) - 1)
                    {
                        UpdateCell(lastX, z+1, addEnergy);
                    }
                    if (z > 0)
                    {
                        UpdateCell(lastX, z-1, addEnergy);
                    }
                }

                if (c.terrain != null)
                {
                    float b = 0.4f + (c.flammable / maxFlammable);
                    Vector3 pos = CellToWorldSpace(lastX, z);
                    pos -= c.terrain.transform.position;
                    //c.terrain.tintMapOverride.SetPixel((int)pos.x, (int)pos.z, new Color(b, b, b));
                    //c.terrain.tintMapOverride.Apply();
                }
            }

            if (c.fireInstance != null)
            {
                c.fireInstance.Recalculate();
                c.fireInstance.GetComponent<AudioSource>().volume = c.energy / maxEnergy;
                if(c.fireInstance.vfx != null)
                {
                    c.fireInstance.vfx.GetComponent<VisualEffect>().SetFloat("Intensity", c.energy);
                }         
            }
        }

        lastX++;
        if (lastX >= cells.GetLength(0))
        {
            lastX = 0;
        }
    }

    private static void UpdateCell(int x, int z, float addEnergy)
    {
        FireCell cell = cells[x, z];
        if (cell.state != FireState.Burned && cell.flammable > 0)
        {
            cell.energy += addEnergy;
        }
        if (cell.state == FireState.Healthy && cell.energy > 4)
        {
            cell.StartBurning();
            SpreadFire(x, z);
        }
    }

    private static void SpreadFire(int x, int z)
    {
        Core.ObjectPool.Request("Fire", CellToWorldSpace(x, z), Quaternion.Euler(0f, 0f, 0f), OnFireCreated);
    }

    private static void OnFireCreated(GameObject obj)
    {
        Vector2 cell = WorldSpaceToCell(obj.transform.position);

        if(cell.x > 127 || cell.y > 127)
        { Debug.Log("FireOutsideBounds"); return; }

        cells[(int)cell.x, (int)cell.y].fireInstance = obj.GetComponent<FireInstance>();
    }

    public static void Clear()
    {
        foreach(FireCell cell in cells)
        {
            if (cell.fireInstance != null)
            {
                if (cell.fireInstance.vfx != null)
                {
                    cell.fireInstance.vfx.GetComponent<PoolObjectRuntime>().Hide();
                    cell.fireInstance.vfx = null;
                }

                cell.fireInstance.GetComponent<PoolObjectRuntime>().Hide();
                cell.fireInstance = null;
            }

            cell.state = FireState.Healthy;
            cell.flammable = maxFlammable;
            cell.energy = 0;
            cell.fireInstance = null;
        }
    }

    private class FireCell
    {
        public float y;

        public FireState state;

        public float flammable;
        public float energy;

        public FireInstance fireInstance;
        public MicroSplatTerrain terrain;

        public void Init(Vector3 pos)
        {
            pos.y = 605f;
            if (Physics.Raycast(pos, Vector3.down, out RaycastHit hit, 615f, fireLayers, QueryTriggerInteraction.Ignore))
            {
                y = hit.point.y;
                hit.transform.TryGetComponent<MicroSplatTerrain>(out terrain);
            }
            else { Debug.Log("Fire Failed "/* + pos.x + " " + pos.z*/); }

            energy = 0f;
            flammable = maxFlammable;
            state = FireState.Healthy;
        }

        public void StartBurning()
        {
            if(state != FireState.Healthy)
            { return; }
            state = FireState.Burning;
        }

        public void Extinguish()
        {
            if(fireInstance != null)
            {
                if(fireInstance.vfx != null)
                {
                    fireInstance.vfx.GetComponent<PoolObjectRuntime>().Hide();
                    fireInstance.vfx = null;
                }

                fireInstance.GetComponent<PoolObjectRuntime>().Hide();
                fireInstance = null;
            }
            state = FireState.Burned;
            energy = 0;
        }

        public void AssignNew(FireCell cell, Vector3 pos)
        {
            state = cell.state;
            flammable = cell.flammable;
            energy = cell.energy;
            y = cell.y;

            if (cell.fireInstance != null)
            {
                fireInstance = cell.fireInstance;
            }

            pos.y = 605f;
            if (Physics.Raycast(pos, Vector3.down, out RaycastHit hit, 615f, fireLayers, QueryTriggerInteraction.Ignore))
            {
                y = hit.point.y;
                hit.transform.TryGetComponent<MicroSplatTerrain>(out terrain);
            }
            else { Debug.Log("Fire Failed " + pos.x + " " + pos.z); }
        }
    }

    public static void CreateFire(Vector3 position)
    {
        Vector2 cellIndex = WorldSpaceToCell(position);

        if(cellIndex.x > 127 || cellIndex.x < 0 || cellIndex.y > 127 || cellIndex.y < 0)
        { return; }

        float heightDif = cells[(int)cellIndex.x, (int)cellIndex.y].y - position.y;

        if (cells[(int)cellIndex.x, (int)cellIndex.y].state == FireState.Healthy && heightDif > -0.5f && heightDif < 0.5f)
        {
            cells[(int)cellIndex.x, (int)cellIndex.y].energy = maxEnergy;
            cells[(int)cellIndex.x, (int)cellIndex.y].StartBurning();
            SpreadFire((int)cellIndex.x, (int)cellIndex.y);
        }
    }

    private static Vector2 WorldSpaceToCell(Vector3 pos)
    {
        return new Vector2((int)(pos.x + 63.5 - shift.x), (int)(pos.z + 63.5 - shift.z));
    }

    private static Vector3 CellToWorldSpace(int x, int z)
    {
        return new Vector3(x - 63.5f, cells[x, z].y, z - 63.5f) + shift;
    }
}
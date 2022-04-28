using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using ScriptableObjects;
public class LevelDesignTools : EditorWindow
{
    [MenuItem("Window/Design Tools/Level Design Tools")]
    public static void Open()
    {
        GetWindow<LevelDesignTools>();
    }

    private enum Tab
    {
        Terrain,
        Stairs,
        Screenshotter,
    }

    public float loadDistance;
    private int zoneLODflags;

    private Tab currentTab;

    public Transform terrains;

    private void OnGUI()
    {
        SerializedObject obj = new SerializedObject(this);    

        EditorGUILayout.BeginHorizontal("box");

        EditorGUILayout.PropertyField(obj.FindProperty("loadDistance"), GUILayout.ExpandWidth(false));

        string[] s = { "LOD0", "LOD1", "LOD2" };
        zoneLODflags = EditorGUILayout.MaskField("Zone load", zoneLODflags, s, GUILayout.ExpandWidth(false));

        if (GUILayout.Button("Load Zones", GUILayout.ExpandWidth(false)))
        {
            Load();
        }

        if (GUILayout.Button("Load Square", GUILayout.ExpandWidth(false)))
        {
            LoadSquare();
        }

        if (GUILayout.Button("Selected Obj To Scenes", GUILayout.ExpandWidth(false)))
        {
            MoveSelectedObjectsToScenes();
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal("box");

        if (GUILayout.Button("Terrain"))
        {
            currentTab = Tab.Terrain;
        }
        if (GUILayout.Button("Stairs"))
        {
            currentTab = Tab.Stairs;
        }
        if (GUILayout.Button("Screenshotter"))
        {
            currentTab = Tab.Screenshotter;
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        if (currentTab == Tab.Terrain)
        {
            EditorGUILayout.BeginHorizontal("box");

            EditorGUILayout.PropertyField(obj.FindProperty("collisionLayerMask"), GUILayout.ExpandWidth(false));

            EditorGUILayout.PropertyField(obj.FindProperty("terrains"), GUILayout.ExpandWidth(false));

            if (GUILayout.Button("Show/Hide Terrains", GUILayout.ExpandWidth(false)))
            {
                ShowTerrains();
            }

            if (GUILayout.Button("Show/Hide Foliage", GUILayout.ExpandWidth(false)))
            {
                ShowFoliage();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal("box");

            if (GUILayout.Button("UpdateAll", GUILayout.ExpandWidth(false)))
            {
                LoadRessources();
                UpdateAll();
            }
         
            if (GUILayout.Button("Set Trees and Details", GUILayout.ExpandWidth(false)))
            {
                SetPrototypes();
            }          

            if (GUILayout.Button("Paint", GUILayout.ExpandWidth(false)))
            {
                LoadRessources();
                Paint();
            }

            if (GUILayout.Button("Place Trees", GUILayout.ExpandWidth(false)))
            {
                LoadRessources();
                PlaceTrees();
            }
            if (GUILayout.Button("Place Grass", GUILayout.ExpandWidth(false)))
            {
                LoadRessources();
                PlaceGrass();
            }

            EditorGUILayout.EndHorizontal();
        }
        else if (currentTab == Tab.Stairs)
        {
            EditorGUILayout.BeginHorizontal("box");

            EditorGUILayout.PropertyField(obj.FindProperty("stepCount"), GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(obj.FindProperty("customGroundLevel"), GUILayout.ExpandWidth(false));
            if (customGroundLevel)
            {
                EditorGUILayout.PropertyField(obj.FindProperty("groundLevel"), GUILayout.ExpandWidth(false));
            }
            EditorGUILayout.PropertyField(obj.FindProperty("stairsProfile"), GUILayout.ExpandWidth(false));

            if (GUILayout.Button("Generate", GUILayout.ExpandWidth(false)))
            {
                GenerateStairs();
            }

            EditorGUILayout.EndHorizontal();
        }
        else if(currentTab == Tab.Screenshotter)
        {
            EditorGUILayout.BeginHorizontal("box");

            EditorGUILayout.PropertyField(obj.FindProperty("screenshotCam"), GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(obj.FindProperty("screenshotPath"), GUILayout.ExpandWidth(false));

            if (GUILayout.Button("Screenshot", GUILayout.ExpandWidth(false)))
            {
                Screenshot();
            }

            EditorGUILayout.EndHorizontal();
        }

        obj.ApplyModifiedProperties();
    }

    private BiomeCollection biomes;
    private Texture2D biomeImg;
    private Texture2D noiseImg;
    private void LoadRessources()
    {
        noiseImg = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Graphics/Miscellanous/EditorResources/SplatNoise.png");
        biomeImg = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Graphics/Miscellanous/EditorResources/Biomes.png");
        biomes = AssetDatabase.LoadAssetAtPath<BiomeCollection>("Assets/World/Biomes/Biomes.asset");
    }

    private void UpdateAll()
    {
        if(terrains == null)
        {
            Debug.Log("Assign the terrains parent.");
            return;
        }
        TerrainData td;
        Terrain terrain;

        for (int i = 0; i < 4; i++)
        {
            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayProgressBar("Updating World", "Terrain " + i + " out of " + 4, i / 4f);

            terrain = terrains.GetChild(i).GetComponent<Terrain>();
            td = terrain.terrainData;

            SetPrototypesOnTerrain(td);

            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayProgressBar("Updating World", "Painting Terrain " + i + " out of " + 4, i / 4f);

            PaintTerrain(td, terrain.transform.position);

            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayProgressBar("Updating World", "Spawning Trees on Terrain " + i + " out of " + 4, i / 4f);

            SpawnTreesOnTerrain(terrain);

            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayProgressBar("Updating World", "Spawning Grass on Terrain " + (i) + " out of " + 4, i / 4f);

            int[,] map = new int[td.detailWidth, td.detailHeight];
            for (int r = 0; r < td.detailPrototypes.Length; r++)
            {
                td.SetDetailLayer(0, 0, r, map);
            }
            SpawnGrassOnTerrain(terrain);

            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayProgressBar("Updating World", "Saving Terrain " + (i) + " out of " + 4, i / 4f);

            terrain.Flush();
            EditorUtility.SetDirty(terrain);
            EditorUtility.SetDirty(td);
            AssetDatabase.SaveAssets();

            EditorUtility.ClearProgressBar();
        }
    }

    private void ShowTerrains()
    {
        if (!terrains.GetChild(0).GetComponent<Terrain>().drawHeightmap)
        {
            for (int i = 0; i < terrains.childCount; ++i)
            {
                terrains.GetChild(i).GetComponent<Terrain>().drawHeightmap = true;
            }
        }
        else
        {
            for (int i = 0; i < terrains.childCount; ++i)
            {
                terrains.GetChild(i).GetComponent<Terrain>().drawHeightmap = false;
            }
        }
    }

    private void ShowFoliage()
    {
        if (!terrains.GetChild(0).GetComponent<Terrain>().drawTreesAndFoliage)
        {
            for (int i = 0; i < terrains.childCount; ++i)
            {
                terrains.GetChild(i).GetComponent<Terrain>().drawTreesAndFoliage = true;
            }
        }
        else
        {
            for (int i = 0; i < terrains.childCount; ++i)
            {
                terrains.GetChild(i).GetComponent<Terrain>().drawTreesAndFoliage = false;
            }
        }
    }
    #region ObjectSceneMoving

    private void MoveSelectedObjectsToScenes()
    {
        foreach (GameObject g in Selection.GetFiltered<GameObject>(SelectionMode.Editable))
        {
            int index;
            string name = (string)g.name.Split('_').GetValue(0);

            index = int.Parse(name);

            if (index >= 0 && index <= 2)
            {
                int x = ((int)g.transform.position.x + 2048) / 128;
                int z = ((int)g.transform.position.z + 2048) / 128;
                string s = "Assets/World/Map/Row " + x + "/" + z + "/Zone " + x + " " + z + " Level " + index + ".unity";
                if (s != null)
                {
                    EditorSceneManager.OpenScene(s, OpenSceneMode.Additive);
                    SceneManager.MoveGameObjectToScene(g, SceneManager.GetSceneByPath(s));
                    EditorSceneManager.MarkSceneDirty(SceneManager.GetSceneByPath(s));
                    EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                }
            }
        }
    }

    #endregion

    #region Loading
    private void Load()
    {
        Vector3 pos = ((SceneView)SceneView.sceneViews[0]).camera.transform.position;
        pos.y = 0f;

        int y = 0;
        int x = 0;
        float dist = loadDistance * loadDistance;
        for (int i = 0; i < 1024; i++)
        {
            if ((pos - GetPosition(x, y)).sqrMagnitude < dist)
            {
                Scene sc;
                if (zoneLODflags == -1 || zoneLODflags == 1 || zoneLODflags == 3 || zoneLODflags == 5)
                {
                    sc = EditorSceneManager.GetSceneByName("Zone " + x + " " + y + " Level " + 0);
                    if (!sc.IsValid())
                    {
                        EditorSceneManager.OpenScene("Assets/World/Map/Row " + x + "/" + y + "/Zone " + x + " " + y + " Level " + 0 + ".unity", OpenSceneMode.Additive);
                    }
                }
                if (zoneLODflags == -1 || zoneLODflags == 2 || zoneLODflags == 3 || zoneLODflags == 6)
                {
                    sc = EditorSceneManager.GetSceneByName("Zone " + x + " " + y + " Level " + 1);
                    if (!sc.IsValid())
                    {
                        EditorSceneManager.OpenScene("Assets/World/Map/Row " + x + "/" + y + "/Zone " + x + " " + y + " Level " + 1 + ".unity", OpenSceneMode.Additive);
                    }
                }
                if (zoneLODflags == -1 || zoneLODflags == 4 || zoneLODflags == 5 || zoneLODflags == 6)
                {
                    sc = EditorSceneManager.GetSceneByName("Zone " + x + " " + y + " Level " + 2);
                    if (!sc.IsValid())
                    {
                        EditorSceneManager.OpenScene("Assets/World/Map/Row " + x + "/" + y + "/Zone " + x + " " + y + " Level " + 2 + ".unity", OpenSceneMode.Additive);
                    }
                }
            }
            else
            {
                Scene sc = EditorSceneManager.GetSceneByName("Zone " + x + " " + y + " Level " + 0);
                if (sc.IsValid())
                {
                    EditorSceneManager.CloseScene(sc, true);
                }
                sc = EditorSceneManager.GetSceneByName("Zone " + x + " " + y + " Level " + 1);
                if (sc.IsValid())
                {
                    EditorSceneManager.CloseScene(sc, true);
                }
                sc = EditorSceneManager.GetSceneByName("Zone " + x + " " + y + " Level " + 2);
                if (sc.IsValid())
                {
                    EditorSceneManager.CloseScene(sc, true);
                }
            }

            y++;
            if (y == 32)
            { y = 0; x++; }
        }
    }

    private void LoadSquare()
    {
        foreach (GameObject g in Selection.GetFiltered<GameObject>(SelectionMode.ExcludePrefab))
        {
            if (g.TryGetComponent<Terrain>(out Terrain t))
            {
                Vector3 pos = t.GetPosition();
                LoadSquareAboveTerrain(pos);
                return;
            }
        }
    }

    private void LoadSquareAboveTerrain(Vector3 position)
    {
        int minX, minY, maxX, maxY;
        if (position.x >= 1000) { minX = 16; maxX = 31; }
        else { minX = 0; maxX = 15; }

        if (position.y >= 1000) { minY = 16; maxY = 31; }
        else { minY = 0; maxY = 15; }

        Scene scene;
        for(int y = minY; y <= maxY; ++y)
        {
            for (int x = minX; x <= maxX; ++x)
            {
                for (int level = 0; level < 3; ++level)
                {
                    scene = EditorSceneManager.GetSceneByName("Zone " + x + " " + y + " Level " + level);
                    if (!scene.IsValid())
                    {
                        EditorSceneManager.OpenScene("Assets/World/Map/Row " + x + "/" + y + "/Zone " + x + " " + y + " Level " + level + ".unity", OpenSceneMode.Additive);
                    }
                }
            }
        }
    }

    private Vector3 GetPosition(int x, int z)
    {
        return new Vector3((x - 15.5f) * 128f, 0f, (z - 15.5f) * 128f);
    }

    #endregion

    #region Terrain Prototypes

    private void SetPrototypes()
    {
        LoadRessources();
        foreach (GameObject g in Selection.objects)
        {
            if (g.TryGetComponent<GameWorld.TerrainInstance>(out GameWorld.TerrainInstance t))
            {
                SetPrototypesOnTerrain(t.GetComponent<Terrain>().terrainData);
            }
        }
    }
    private void SetPrototypesOnTerrain(TerrainData data)
    {
        List<TreePrototype> trees = new List<TreePrototype>();
        List<DetailPrototype> details = new List<DetailPrototype>();

        foreach (TreeType p in biomes.allTrees)
        {      
            TreePrototype tree = new() { prefab = p.prefab };
            trees.Add(tree);
        }
        for (int i = 0; i < biomes.allDetails.Length; ++i)
        {
            DetailPrototype detail = new()
            {
                prototype = biomes.allDetails[i].prefab,
                usePrototypeMesh = true,
                dryColor = biomes.allDetails[i].mainColor,
                healthyColor = biomes.allDetails[i].secondaryColor,
                noiseSpread = biomes.allDetails[i].noiseSize,
                minHeight = biomes.allDetails[i].minMaxHeight.x,
                maxHeight = biomes.allDetails[i].minMaxHeight.y,
                minWidth = biomes.allDetails[i].minMaxWidth.x,
                maxWidth = biomes.allDetails[i].minMaxWidth.y,
            };
            details.Add(detail);
        }

        data.detailPrototypes = details.ToArray();
        data.treePrototypes = trees.ToArray();
    }

    #endregion

    #region Procedural Texturing

    private void Paint()
    {
        foreach (GameObject g in Selection.GetFiltered<GameObject>(SelectionMode.ExcludePrefab))
        {
            if(g.TryGetComponent<Terrain>(out Terrain t))
            {
                PaintTerrain(t.terrainData, t.transform.position);

                t.Flush();
                EditorUtility.SetDirty(t.terrainData);
                EditorUtility.SetDirty(t);
                AssetDatabase.SaveAssets();

                
                Debug.Log("Memory used before collection: " + System.GC.GetTotalMemory(false));
                System.GC.Collect();
                Debug.Log("Memory used after full collection: " + System.GC.GetTotalMemory(true));
            }
        }
    }

    private void PaintTerrain(TerrainData data, Vector3 pos)
    {
        float[,,] oldSplat = data.GetAlphamaps(0, 0, data.alphamapWidth, data.alphamapHeight);
        float[,,] splatData = new float[data.alphamapWidth, data.alphamapHeight, 20];

        float steepnessDiv = data.alphamapHeight - 1;

        for (int y = 0; y < data.alphamapHeight; y++)
        {
            for (int x = 0; x < data.alphamapWidth; x++)
            {
                float xb = 2112 - pos.x - (y / 2f);
                float yb = 2112 - pos.z - (x / 2f);

                Color c = biomeImg.GetPixel((int)xb, (int)yb);

                float height = data.GetHeight(y / 2, x / 2);
                float slope = data.GetSteepness(y / steepnessDiv, x / steepnessDiv);

                float[] splat = new float[biomes.terrainMaterials.Length];
                float[] hand = new float[biomes.terrainMaterials.Length];
                for (int i = 0; i < biomes.terrainMaterials.Length; i++)
                {
                    bool isBiome = false;

                    if (c.r > 0 && biomes.terrainMaterials[i].biomes.Contains(BiomeType.Rocks))
                    { isBiome = true; }
                    else if (c.g > 0 && biomes.terrainMaterials[i].biomes.Contains(BiomeType.Field))
                    { isBiome = true; }
                    else if (c.b > 0 && biomes.terrainMaterials[i].biomes.Contains(BiomeType.Forest))
                    { isBiome = true; }

                    if (!biomes.terrainMaterials[i].procedural)
                    {
                        splat[i] = 0;
                        hand[i] = oldSplat[x, y, i];
                    }
                    else if (!isBiome)
                    {
                        splat[i] = 0;
                        hand[i] = oldSplat[x, y, i];
                    }
                    else
                    {
                        float h = CheckHeight(biomes.terrainMaterials[i], height);
                        float s = CheckSlope(biomes.terrainMaterials[i], slope);

                        splat[i] = h * s;
                        hand[i] = 0;
                    }
                }

                float noise = noiseImg.GetPixel(Mathf.RoundToInt((pos.z + (x * 0.5f) + biomes.seed) * biomes.noiseScale), Mathf.RoundToInt((pos.x + (y * 0.5f) + biomes.seed) * biomes.noiseScale)).r;
                noise = Mathf.Clamp(noise, 0.001f, 0.999f);
                splat = Normalize(splat, noise);

                float handSum = 0;
                for (int i = 0; i < biomes.terrainMaterials.Length; i++)
                {
                    handSum += hand[i];
                }

                for (int i = 0; i < biomes.terrainMaterials.Length; i++)
                {
                    splatData[x, y, i] = (1 - handSum) * splat[i] + hand[i];
                }
            }
        }
        data.SetAlphamaps(0, 0, splatData);
    }

    private float CheckHeight(TerrainMaterial mat, float height)
    {
        float maxHeight = mat.maxHeight;
        float minHeight = mat.minHeight;

        if (height >= minHeight && height <= maxHeight)
        { return 1; }
        /*else if (height >= minHeight - heightFalloff && height < minHeight)
        {
            return 1 - ((minHeight - height) / heightFalloff);
        }
        else if (height <= maxHeight + heightFalloff && height > maxHeight)
        {
            return 1 - ((height - maxHeight) / heightFalloff);
        }*/
        else
        {
            return 0;
        }
    }

    private float CheckSlope(TerrainMaterial mat, float slope)
    {
        float maxSlope = mat.maxSlope;
        float minSlope = mat.minSlope;

        if (slope >= minSlope && slope <= maxSlope)
        { return 1; }
        /*else if (slope >= minSlope - slopeFalloff && slope < minSlope)
        {
            return 1 - ((minSlope - slope) / slopeFalloff);
        }
        else if (slope <= maxSlope + slopeFalloff && slope > maxSlope)
        {
            return 1 - ((slope - maxSlope) / slopeFalloff);
        }*/
        else
        {
            return 0;
        }
    }

    public static float[] Normalize(float[] f, float noise)
    {
        List<int> indexes = new List<int>();
        for (int i = 0; i < f.Length; i++)
        {
            if (f[i] > 0)
            {
                indexes.Add(i);
            }
            else
            {
                f[i] = 0;
            }
        }

        if (indexes.Count == 0)
        {
            for (int i = 0; i < f.Length; i++)
            {
                f[i] = 0;
            }
            return f;
        }

        float b = 1f / indexes.Count;
        float a;

        for (int i = 0; i < indexes.Count; i++)
        {
            if (noise < (i + 1.25f) * b && noise >= i * b)
            {
                a = (i + 1.15f) * b - noise;
            }
            else if (noise > (i - 1.15f) * b && noise <= (i + 1) * b && i != 0)
            {
                a = noise - (i - 1.15f) * b;
            }
            else
            {
                a = 0;
            }

            f[indexes[i]] = a;
        }

        float total = 0f;

        for (int i = 0; i < f.Length; i++)
        {
            total += f[i];
        }

        for (int i = 0; i < f.Length; i++)
        {
            f[i] /= total;
        }

        return f;
    }

    #endregion

    #region Vegetation Placement

    private readonly List<TreeInstance> treeInstanceCollection = new List<TreeInstance>();
    private void PlaceTrees()
    {
        foreach (GameObject g in Selection.GetFiltered<GameObject>(SelectionMode.ExcludePrefab))
        {
            if (g.TryGetComponent<Terrain>(out Terrain t))
            {
                SpawnTreesOnTerrain(t);

                t.Flush();
                EditorUtility.SetDirty(t.terrainData);
                EditorUtility.SetDirty(t);
                AssetDatabase.SaveAssets();

                System.GC.Collect();
            }
        }
    }
    private void SpawnTreesOnTerrain(Terrain terrain)
    {
        terrain.terrainData.treeInstances = new TreeInstance[0];
        treeInstanceCollection.Clear();
        foreach (Biome biome in biomes.biomes)
        {
            SpawnTree(biome, terrain);
        }

        terrain.terrainData.SetTreeInstances(treeInstanceCollection.ToArray(), false);
    }

    private Dictionary<Terrain, Cell[,]> terrainCells = new Dictionary<Terrain, Cell[,]>();
    private int cellSize = 512;
    private int cellDivisions = 512;
    public LayerMask collisionLayerMask;
    private int recursionCounter = 0;

    private void SpawnTree(Biome biome, Terrain terrain)
    {      
        BuildCollisionData(terrain);

        Vector3[] spawnPoints = PoissonDisc.GetSpawnpoints(terrain, biome.treeDistance, biomes.seed);

        foreach (Vector3 pos in spawnPoints)
        {
            float xb = 2112 - pos.x;
            float yb = 2112 - pos.z;
            Color c = biomeImg.GetPixel((int)xb, (int)yb);

            float spawnChance = 0f;
            float value = 0;

            if (biome.index == 0)
            {
                value = c.r;
            }
            else if (biome.index == 1)
            {
                value = c.g;
            }
            else if (biome.index == 2)
            {
                value = c.b;
            }

            if (value > 0)
            {
                value = Mathf.Clamp01(value);
                spawnChance += value;
            }

            if (Random.value > spawnChance)
            { continue; }

            Vector2 normalizedPos = GetNormalizedPosition(terrain, pos);
        
            if (((Random.value * 100f) <= biome.treeProbability) == false)
            {
                continue;
            }

            if (InsideOccupiedCell(terrain, pos, normalizedPos))
            {
                continue;
            }

            recursionCounter = 0;
            TreeType tree = PickTreeRecursive(biome);
            if (tree == null) continue;

            SampleHeight(terrain, normalizedPos, out float height, out float worldHeight, out float normalizedHeight);

            if (worldHeight < tree.heightRange.x || worldHeight > tree.heightRange.y)
            { continue; }

            if (tree.slopeRange.x > 0 || tree.slopeRange.y < 90f)
            {
                float slope = GetSlope(terrain, normalizedPos, false);

                if (!(slope >= (tree.slopeRange.x) && slope <= (tree.slopeRange.y)))
                { continue; }
            }

            if (tree.curvatureRange.x > 0 || tree.curvatureRange.y < 1f)
            {
                float curvature = SampleConvexity(terrain, normalizedPos);
                //0=concave, 0.5=flat, 1=convex
                curvature = (curvature - (1 - curvature)) * 0.5f + 0.5f;
                if (curvature < tree.curvatureRange.x || curvature > tree.curvatureRange.y)
                { continue; }
            }

            Vector3 terrainPosition = pos - terrain.transform.position;

            Vector3 mapPosition = new Vector3
            (terrainPosition.x / terrain.terrainData.size.x, 0,
            terrainPosition.z / terrain.terrainData.size.z);

            float xCoord = mapPosition.x * terrain.terrainData.alphamapWidth;
            float zCoord = mapPosition.z * terrain.terrainData.alphamapHeight;

            int posX;
            int posZ;
            posX = (int)xCoord;
            posZ = (int)zCoord;

            float[,,] aMap = terrain.terrainData.GetAlphamaps(posX, posZ, 1, 1);

            List<int> indexes = new List<int>();

            for (int i = 0; i < aMap.GetLength(2);i++)
            {
                if(aMap[0,0,i] > 0.6f)
                {
                    indexes.Add(i);
                }
            }

            if (indexes.Count > 0)
            {
                if (!HasValidTreeSplat(indexes))
                { continue; }
            }
            else
            {
                float h = 0;
                int index = -1;

                for(int i = 0; i < aMap.GetLength(2); i++)
                {
                    if(aMap[0,0,i] > h)
                    {
                        h = aMap[0, 0, i];
                        index = i;
                    }
                }
                if(index < 0)
                { continue; }
                if (!IsValidTreeSplat(index))
                { continue; }
            }

            TreeInstance treeInstance = new();
            treeInstance.prototypeIndex = tree.index;

            treeInstance.position = new Vector3(normalizedPos.x, normalizedHeight - (tree.sinkAmount / (terrain.terrainData.size.y + 0.01f)), normalizedPos.y);
            treeInstance.rotation = Random.Range(0f, 359f) * Mathf.Deg2Rad;

            float scale = Random.Range(tree.scaleRange.x, tree.scaleRange.y);
            treeInstance.heightScale = scale;
            treeInstance.widthScale = scale;

            treeInstance.color = Color.white;
            treeInstance.lightmapColor = Color.white;

            treeInstanceCollection.Add(treeInstance);
        }        
    }

    private bool IsValidTreeSplat(int index)
    {
        if (biomes.terrainMaterials[index].trees)
        { return true; }       

        return false;
    }
    private bool IsValidGrassSplat(int index)
    {
        if (biomes.terrainMaterials[index].gras)
        { return true; }    

        return false;
    }
    private bool HasValidTreeSplat(List<int> indexes)
    {
        for(int i = 0; i < indexes.Count; i++)
        {
            if(biomes.terrainMaterials[indexes[i]].trees)
            { return true; }
        }

        return false;
    }
    private bool HasValidGrassSplat(List<int> indexes)
    {
        for (int i = 0; i < indexes.Count; i++)
        {
            if (biomes.terrainMaterials[indexes[i]].gras)
            { return true; }
        }

        return false;
    }

    private void BuildCollisionData(Terrain terrain)
    {
        RaycastHit hit;

        terrainCells.Clear();

        int xCount = Mathf.CeilToInt(terrain.terrainData.size.x / cellSize);
        int zCount = Mathf.CeilToInt(terrain.terrainData.size.z / cellSize);

        Cell[,] cellGrid = new Cell[xCount, zCount];

        for (int x = 0; x < xCount; x++)
        {
            for (int z = 0; z < zCount; z++)
            {
                Vector3 wPos = new Vector3(terrain.GetPosition().x + (x * cellSize) + (cellSize * 0.5f), 0f, terrain.GetPosition().z + (z * cellSize) + (cellSize * 0.5f));

                Vector2 normalizeTerrainPos = GetNormalizedPosition(terrain, wPos);

                SampleHeight(terrain, normalizeTerrainPos, out _, out wPos.y, out _);

                Cell cell = Cell.New(wPos, cellSize);
                cell.Subdivide(cellDivisions);

                cellGrid[x, z] = cell;

                for (int sX = 0; sX < cellDivisions; sX++)
                {
                    for (int sZ = 0; sZ < cellDivisions; sZ++)
                    {
                        Bounds b = cell.subCells[sX, sZ].bounds;

                        Vector3[] corners = new Vector3[]
                        {
                                //BL corner
                                new Vector3(b.min.x, b.center.y, b.min.z),
                                //TL corner
                                new Vector3(b.min.x, b.center.y, b.min.z + b.size.z),
                                //BR corner
                                new Vector3(b.max.x, b.center.y, b.min.z),
                                //TR corner
                                new Vector3(b.max.x, b.center.y, b.max.z),
                        };

                        int hitCount = corners.Length;
                        for (int i = 0; i < corners.Length; i++)
                        {
                            if (Physics.Raycast(corners[i] + (Vector3.up * 100f), -Vector3.up, out hit, 150f, collisionLayerMask))
                            {
                                //Require to check for type, since its possible to hit a neighboring terrains
                                if (hit.collider.GetType() == typeof(TerrainCollider))
                                {
                                    hitCount--;
                                }
                            }
                            else
                            {
                                hitCount--;
                            }
                        }

                        //Remove cell when all rays missed
                        if (hitCount == 0) cell.subCells[sX, sZ] = null;
                    }
                }
            }

        }
        terrainCells.Add(terrain, cellGrid);
    }

    public bool InsideOccupiedCell(Terrain terrain, Vector3 worldPos, Vector2 normalizedPos)
    {
        if (terrainCells == null) return false;

        //No collision cells baked for terrain, user will probably notice
        if (terrainCells.ContainsKey(terrain) == false) return false;

        Cell[,] cells = terrainCells[terrain];

        Vector2Int cellIndex = Cell.PositionToCellIndex(terrain, normalizedPos, cellSize);
        Cell mainCell = cells[cellIndex.x, cellIndex.y];

        if (mainCell != null)
        {
            Cell subCell = mainCell.GetSubcell(worldPos, cellSize, cellDivisions);

            if (subCell != null)
            {
                return true;
            }
            else
            {
                //Cell doesn't exist
                return false;
            }
        }
        else
        {
            Debug.LogErrorFormat("Position {0} falls outside of the cell grid", worldPos);
        }

        return false;
    }

    public static void SampleHeight(Terrain terrain, Vector2 position, out float height, out float worldHeight, out float normalizedHeight)
    {
        height = terrain.terrainData.GetHeight(
            Mathf.CeilToInt(position.x * terrain.terrainData.heightmapTexture.width),
            Mathf.CeilToInt(position.y * terrain.terrainData.heightmapTexture.height)
            );

        worldHeight = height + terrain.transform.position.y;
        //Normalized height value (0-1)
        normalizedHeight = height / terrain.terrainData.size.y;
    }

    public float GetSlope(Terrain terrain, Vector2 position, bool average = false)
    {
        if (!average)
        {
            return terrain.terrainData.GetSteepness(position.x, position.y);
        }

        return GetAverageSlope(terrain, position);
    }

    public float GetAverageSlope(Terrain terrain, Vector2 position)
    {
        float texelSize = (1f / terrain.terrainData.heightmapTexture.width) * 2f;

        float slope = 0f;

        //Center
        slope += terrain.terrainData.GetSteepness(position.x, position.y);
        //Right
        slope += terrain.terrainData.GetSteepness(position.x + texelSize, position.y);
        //Left
        slope += terrain.terrainData.GetSteepness(position.x - texelSize, position.y);
        //Up
        slope += terrain.terrainData.GetSteepness(position.x, position.y + texelSize);
        //Down
        slope += terrain.terrainData.GetSteepness(position.x, position.y - texelSize);

        return slope / 5f;
    }

    public float SampleConvexity(Terrain terrain, Vector2 position, float radius = 3f)
    {
        float texelSize = (1f / terrain.terrainData.heightmapResolution) * radius;

        float posX = terrain.terrainData.GetInterpolatedNormal(position.x + texelSize, position.y).x;
        float negX = terrain.terrainData.GetInterpolatedNormal(position.x - texelSize, position.y).x;

        float x = (posX - negX) + 0.5f;

        float posY = terrain.terrainData.GetInterpolatedNormal(position.x, position.y + texelSize).z;
        float NegY = terrain.terrainData.GetInterpolatedNormal(position.x, position.y - texelSize).z;

        float y = (posY - NegY) + 0.5f;

        //Blend overlay
        return (y < 0.5f) ? 2.0f * x * y : 1.0f - 2.0f * (1.0f - x) * (1.0f - y);
    }

    public static Vector2Int SplatmapTexelIndex(Terrain terrain, Vector2 position)
    {
        return new Vector2Int(Mathf.CeilToInt(position.x * terrain.terrainData.alphamapWidth),Mathf.CeilToInt(position.y * terrain.terrainData.alphamapHeight));
    }

    private int GetSplatmapID(int layerID)
    {
        if (layerID > 11) return 3;
        if (layerID > 7) return 2;
        if (layerID > 3) return 1;

        return 0;
    }

    private  float SampleChannel(Color color, int channel)
    {
        float value = 0;

        switch (channel)
        {
            case 0:
                value = color.r;
                break;
            case 1:
                value = color.g;
                break;
            case 2:
                value = color.b;
                break;
            case 3:
                value = color.a;
                break;
        }

        return value;
    }

    private Vector2 GetNormalizedPosition(Terrain terrain, Vector3 worldPosition)
    {
        Vector3 localPos = terrain.transform.InverseTransformPoint(worldPosition);

        //Position relative to terrain as 0-1 value
        return new Vector2(
            localPos.x / terrain.terrainData.size.x,
            localPos.z / terrain.terrainData.size.z);
    }

    private TreeType PickTreeRecursive(Biome biome)
    {
        if (biome.trees.Length == 0) { return null; }

        int r = Random.Range(0, biome.trees.Length);
        TreeType t = biome.trees[r];

        //Give up after 4 attempts
        if (recursionCounter >= 4) { return null; }

        if ((Random.value * 100f) <= t.probability[biome.index]) { return t; }

        recursionCounter++;
        return PickTreeRecursive(biome);
    }
    private GrassType PickGrassRecursive(Biome biome)
    {
        if (biome.details.Length == 0) return null;

        GrassType g = biome.details[Random.Range(0, biome.details.Length)];

        if (recursionCounter >= 4) return null;
        if ((Random.value * 100f) <= g.probability[biome.index])
        {
            return g;
        }
        recursionCounter++;

        return PickGrassRecursive(biome);
    }

    private Vector3 DetailToWorld(Terrain terrain, int x, int y)
    {
        //XZ world position
        return new Vector3(
            terrain.GetPosition().x + ((float)x / terrain.terrainData.detailWidth * terrain.terrainData.size.x),
            0f,
            terrain.GetPosition().z + ((float)y / terrain.terrainData.detailHeight * terrain.terrainData.size.z));
    }

    private void PlaceGrass()
    {
        foreach (GameObject g in Selection.GetFiltered<GameObject>(SelectionMode.ExcludePrefab))
        {
            if (g.TryGetComponent<Terrain>(out Terrain t))
            {
                TerrainData data = t.terrainData;

                int[,] map = new int[data.detailWidth, data.detailHeight];
                for (int i = 0; i < data.detailPrototypes.Length; i++)
                {
                    data.SetDetailLayer(0, 0, i, map);
                }
                SpawnGrassOnTerrain(t);

                t.Flush();
                EditorUtility.SetDirty(data);
                EditorUtility.SetDirty(t);
                AssetDatabase.SaveAssets();
            }
        }
    }

    private void SpawnGrassOnTerrain(Terrain terrain)
    {
        foreach (Biome biome in biomes.biomes)
        {
            SpawnGrass(biome, terrain);
        }
    }

    private void SpawnGrass(Biome biome, Terrain terrain)
    {
        BuildCollisionData(terrain);

        for (int i = 0; i < biome.details.Length; i++)
        {
            int[,] map = new int[terrain.terrainData.detailWidth, terrain.terrainData.detailHeight];

            for (int x = 0; x < terrain.terrainData.detailWidth; x++)
            {
                for (int y = 0; y < terrain.terrainData.detailHeight; y++)
                {
                    if (((Random.value * 100f) <= biome.grassProbability) == false)
                    { continue; }

                    Vector3 wPos = DetailToWorld(terrain, y, x);
                    Vector2 normalizedPos = GetNormalizedPosition(terrain, wPos);

                    float xb = 2112 - wPos.x;
                    float yb = 2112 - wPos.z;
                    Color c = biomeImg.GetPixel((int)xb, (int)yb);
                    float spawnChance = 0f;
                    float value = 0;

                    if (biome.index == 0)
                    {
                        value = c.r;
                    }
                    else if (biome.index == 1)
                    {
                        value = c.g;
                    }
                    else if (biome.index == 2)
                    {
                        value = c.b;
                    }
                    if (value > 0)
                    {
                        value = Mathf.Clamp01(value);
                        spawnChance += value;
                    }
                    if (Random.value > spawnChance)
                    { continue; }

                    if (InsideOccupiedCell(terrain, wPos, normalizedPos))
                    { continue; }

                    SampleHeight(terrain, normalizedPos, out _, out wPos.y, out _);

                    recursionCounter = 0;
                    GrassType grass = PickGrassRecursive(biome);
                    if (grass == null) continue;


                    if (wPos.y < grass.heightRange.x || wPos.y > grass.heightRange.y)
                    { continue; }

                    if (grass.slopeRange.x > 0 || grass.slopeRange.y < 90)
                    {
                        float slope = GetSlope(terrain, normalizedPos);
                        //Reject if slope check fails
                        if (slope < grass.slopeRange.x || slope > grass.slopeRange.y)
                        { continue; }
                    }

                    if (grass.curvatureRange.x > 0 || grass.curvatureRange.y < 1f)
                    {
                        float curvature = SampleConvexity(terrain, normalizedPos);
                        //0=concave, 0.5=flat, 1=convex
                        curvature = (curvature - (1 - curvature)) * 0.5f + 0.5f;
                        if (curvature < grass.curvatureRange.x || curvature > grass.curvatureRange.y)
                        { continue; }
                    }

                    Vector3 terrainPosition = wPos - terrain.transform.position;

                    Vector3 mapPosition = new Vector3
                    (terrainPosition.x / terrain.terrainData.size.x, 0,
                    terrainPosition.z / terrain.terrainData.size.z);

                    float xCoord = mapPosition.x * terrain.terrainData.alphamapWidth;
                    float zCoord = mapPosition.z * terrain.terrainData.alphamapHeight;

                    int posX;
                    int posZ;
                    posX = (int)xCoord;
                    posZ = (int)zCoord;

                    float[,,] alphaMap = terrain.terrainData.GetAlphamaps(posX, posZ, 1, 1);

                    List<int> indexes = new();

                    for (int j = 0; j < alphaMap.GetLength(2); j++)
                    {
                        if (alphaMap[0, 0, j] > 0.6f)
                        {
                            indexes.Add(j);
                        }
                    }

                    if (indexes.Count > 0)
                    {
                        if (!HasValidGrassSplat(indexes))
                        {
                            continue;
                        }
                    }
                    else
                    {
                        float h = 0;
                        int index = -1;

                        for (int j = 0; j < alphaMap.GetLength(2); j++)
                        {
                            if (alphaMap[0, 0, j] > h)
                            {
                                h = alphaMap[0, 0, j];
                                index = j;
                            }
                        }

                        if (index == -1 || !IsValidGrassSplat(index))
                        {
                            continue;
                        }
                    }

                    map[x, y] = 1;
                }
            }

            int[,] old = terrain.terrainData.GetDetailLayer(0, 0, terrain.terrainData.detailWidth, terrain.terrainData.detailHeight, biome.details[i].index);

            for(int x = 0; x < terrain.terrainData.detailWidth; x++)
            {
                for (int y = 0; y < terrain.terrainData.detailHeight; y++)
                {
                    map[x, y] += old[x, y];
                }
            }

            terrain.terrainData.SetDetailLayer(0, 0, biome.details[i].index, map);
        }                         
    }

    #endregion

    #region Stairs

    public int stepCount;
    public bool customGroundLevel;
    public float groundLevel;
    private float _groundLevel;
    public StairsProfile stairsProfile;

    private void GenerateStairs()
    {
        List<Transform> childrenToDestroy = new List<Transform>();
        foreach (GameObject g in Selection.GetFiltered<GameObject>(SelectionMode.Editable))
        {
            if (customGroundLevel)
            {
                _groundLevel = groundLevel;
            }
            else
            {
                _groundLevel = 0;
            }

            for (int i = 0; i < g.transform.childCount; i++)
            {
                if (g.transform.GetChild(i).name.Contains("StairsElement"))
                {
                    childrenToDestroy.Add(g.transform.GetChild(i));
                }
            }

            foreach (Transform t in childrenToDestroy)
            {
                DestroyImmediate(t.gameObject);
            }

            StairsElement last = null;
            StairsElement newElement;

            Vector3 pos = Vector3.zero;
            for (int i = 0; i < stepCount; i++)
            {
                newElement = stairsProfile.elements[Random.Range(0, stairsProfile.elements.Length)];
                if (last != null)
                {
                    pos += new Vector3(0f, last.height, last.depth);
                }

                GameObject st = Instantiate(newElement.model, g.transform.position, g.transform.rotation * newElement.rotation, g.transform);
                st.transform.localPosition += pos;

                last = newElement;
            }

            if (stairsProfile.wallType != StairsProfile.WallType.NoWall)
            {
                Transform wall = Instantiate(stairsProfile.wall, g.transform.position + new Vector3(pos.z, _groundLevel, 0f), g.transform.rotation * Quaternion.Euler(0f, 180f, 270f), g.transform).transform;

                wall.localScale = new Vector3(pos.y, 1f, pos.z);
            }
        }
    }

    #endregion

    #region Screenshot

    public Camera screenshotCam;
    public string screenshotPath;
    private void Screenshot()
    {
        RenderTexture.active = screenshotCam.targetTexture;
        RenderTexture currentRT = RenderTexture.active;

        screenshotCam.Render();

        Texture2D img = new(screenshotCam.targetTexture.width, screenshotCam.targetTexture.height);
        img.ReadPixels(new Rect(0, 0, screenshotCam.targetTexture.width, screenshotCam.targetTexture.height), 0, 0);
        img.Apply();
        RenderTexture.active = currentRT;

        byte[] bytes = img.EncodeToPNG();
        DestroyImmediate(img);

        System.IO.File.WriteAllBytes(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop) + "/UnityScreenshots/" + screenshotPath + Random.Range(0, int.MaxValue) + ".png", bytes);
    }

    #endregion
}

public class Cell
{
    public Bounds bounds = new Bounds();
    public Cell[,] subCells;

    public void Subdivide(int divisions)
    {
        subCells = new Cell[divisions, divisions];

        int subCount = divisions;
        int cellSize = ((int)bounds.size.x / divisions) * 2;

        for (int x = 0; x < subCount; x++)
        {
            for (int z = 0; z < subCount; z++)
            {
                Vector3 subCellPos = new Vector3(
                    bounds.min.x + (x * (cellSize * 0.5f)),
                    bounds.center.y,
                    bounds.min.z + (z * (cellSize * 0.5f))
                    );
                Cell subCell = Cell.New(subCellPos, cellSize * 0.5f);

                subCells[x, z] = subCell;
            }
        }
    }
    public static Cell New(Vector3 wPos, float size)
    {
        Cell cell = new Cell();

        cell.bounds.size = new Vector3(1f, 1f, 1f) * size;
        cell.bounds.center = wPos;

        return cell;
    }

    public bool InsideXZ(Vector3 wPos)
    {
        return (wPos.x >= bounds.min.x && wPos.x <= bounds.max.x && wPos.z >= bounds.min.z && wPos.z <= bounds.max.z);
    }

    public static Vector2Int PositionToCellIndex(Terrain terrain, Vector2 normalizedPos, int cellSize)
    {
        int x = Mathf.FloorToInt((terrain.terrainData.size.x / cellSize) * normalizedPos.x);
        int y = Mathf.FloorToInt((terrain.terrainData.size.z / cellSize) * normalizedPos.y);

        return new Vector2Int(x, y);
    }

    public Cell GetSubcell(Vector3 worldPos, float cellSize, int subDivisions)
    {
        if (subCells == null) return null;

        Vector2 localCellPos = new Vector2(
            (worldPos.x - bounds.min.x) / cellSize,
            (worldPos.z - bounds.min.z) / cellSize);

        Vector2Int subCellIndex = new Vector2Int(
            Mathf.FloorToInt(subDivisions * localCellPos.x),
            Mathf.FloorToInt(subDivisions * localCellPos.y));

        return subCells[subCellIndex.x, subCellIndex.y];
    }
}

public static class PoissonDisc
{
    //Max attempts
    private const int MAX_ATTEMPTS = 10;
    private const int dimensions = 2; //2D

    private static readonly List<Vector2> samples = new List<Vector2>();
    private static readonly List<Vector2> points = new List<Vector2>();
    private static List<Vector3> spawnPoints = new List<Vector3>();

    private static int[,] grid;
    private static float cellSize;
    private static float radius;
    private static Bounds bounds;

    public static Vector3[] GetSpawnpoints(Terrain terrain, float _radius, int seed)
    {
        radius = _radius;
        bounds = terrain.terrainData.bounds;

        cellSize = radius / Mathf.Sqrt(dimensions);

        //Initialize terrain background grid
        int xCells = Mathf.CeilToInt(bounds.size.x / cellSize);
        int zCells = Mathf.CeilToInt(bounds.size.z / cellSize);
        grid = new int[xCells, zCells];

        samples.Clear();
        points.Clear();
        spawnPoints = new List<Vector3>();

        Random.InitState(seed);

        //Random starting point
        Vector2 randomPos = new Vector2(Random.value * bounds.size.x, Random.value * bounds.size.z);
        samples.Add(randomPos);

        while (samples.Count > 0)
        {
            int i = Random.Range(0, samples.Count);
            Vector2 sampleCenter = samples[i];

            bool valid = false;

            //Find next available position randomly outside radius^2
            for (int s = 0; s < MAX_ATTEMPTS; s++)
            {
                Random.InitState(seed + s + i);

                Vector2 sample = RandomPointOnAnnulus(sampleCenter);

                if (ValidSample(sample))
                {
                    Vector3 spawnPoint = CreateSpawnPoint(terrain, sample);

                    spawnPoints.Add(spawnPoint);

                    points.Add(sample);
                    samples.Add(sample);

                    Vector2Int gridPos = PositionToGridCoord(sample);
                    grid[gridPos.x, gridPos.y] = points.Count;

                    valid = true;
                    break;
                }

            }
            if (!valid)
            {
                samples.RemoveAt(i);
            }

        }

        return spawnPoints.ToArray();
    }

    private static Vector2Int PositionToGridCoord(Vector2 pos)
    {
        return new Vector2Int((int)(pos.x / cellSize), (int)(pos.y / cellSize));
    }

    private static bool ValidSample(Vector2 sample)
    {
        //Reject any samples outside of the terrain bounds
        bool valid = InsideBounds(sample);

        if (valid)
        {
            Vector2Int gridPos = PositionToGridCoord(sample);

            int xmin = Mathf.Max(gridPos.x - 2, 0);
            int xmax = Mathf.Min(gridPos.x + 2, grid.GetLength(0) - 1);
            int ymin = Mathf.Max(gridPos.y - 2, 0);
            int ymax = Mathf.Min(gridPos.y + 2, grid.GetLength(1) - 1);

            //Check cells around current grid cell (3x3)
            for (int y = ymin; y <= ymax; y++)
            {
                for (int x = xmin; x <= xmax; x++)
                {
                    int i = grid[x, y] - 1;

                    if (i != -1)
                    {
                        if (OutsideRadius(sample, points[i])) return false;
                    }
                }
            }
        }
        return valid;
    }

    private static bool InsideBounds(Vector2 pos)
    {
        return (pos.x >= 0 && pos.x < bounds.size.x && pos.y >= 0 && pos.y < bounds.size.z);
    }

    //Check if position falls within annulus
    private static bool OutsideRadius(Vector2 center, Vector2 position)
    {
        return ((center - position).sqrMagnitude < (radius * radius));
    }

    private static Vector2 RandomPointOnAnnulus(Vector2 center)
    {
        float angle = 2f * Mathf.PI * Random.value;
        Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        float dist = Random.Range(radius, radius * 2);

        return center + (dir * dist);
    }

    //Creates a world-space spawn point, relative to the terrain bounds
    private static Vector3 CreateSpawnPoint(Terrain t, Vector2 position)
    {
        return new Vector3((position.x + (t.GetPosition().x + bounds.center.x)) - bounds.extents.x, 0f, (position.y + (t.GetPosition().z + bounds.center.z)) - bounds.extents.z);
    }
}
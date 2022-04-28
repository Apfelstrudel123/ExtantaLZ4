using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System.IO;

public class WorldStreamEditor : EditorWindow
{
    [MenuItem("Window/Design Tools/Sector Tools")]
    public static void Open()
    {
        GetWindow<WorldStreamEditor>();
    }

    public GameObject[] terrain;
    public TerrainData[] terrainData;

    public bool generierScene;
    public bool generierOrdner;
    public string path;

    public int startRow;
    public int lastRow;

    private void OnGUI()
    {
        SerializedObject obj = new SerializedObject(this);

        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.PropertyField(obj.FindProperty("path"));

        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.PropertyField(obj.FindProperty("terrain"));
        EditorGUILayout.PropertyField(obj.FindProperty("terrainData"));

        EditorGUILayout.PropertyField(obj.FindProperty("startRow"));
        EditorGUILayout.PropertyField(obj.FindProperty("lastRow"));

        EditorGUILayout.PropertyField(obj.FindProperty("generierScene"));
        EditorGUILayout.PropertyField(obj.FindProperty("generierOrdner"));


        if (GUILayout.Button("Generier Welt"))
        {
            if (generierOrdner)
            {
                GenerateFolder();
            }
            if (generierScene)
            {
                GenerateScene();
            }
        }

        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        EditorGUILayout.Space();

        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.HelpBox("Near = 0, Far = 4", MessageType.Info);

        EditorGUILayout.EndVertical();

        obj.ApplyModifiedProperties();
    }

    #region Generation

    private void GenerateFolder()
    {
        for (int i = startRow; i <= lastRow; i++)
        {
            Directory.CreateDirectory(path + "/Row " + i);

            for (int u = 0; u < 32; u++)
            {
                Directory.CreateDirectory(path + "/Row " + i + "/" + u);
            }
        }
    }

    private void GenerateScene()
    {
        Quaternion q = Quaternion.Euler(0f, 0f, 0f);
        Scene s;
        GameObject g;
        TerrainData td;
        Mesh mesh;
        List<Scene> scenesToSave = new List<Scene>();
        Vector3 v;

        for (int i = startRow; i <= lastRow; i++)
        {
            for (int u = 0; u < 32; u++)
            {
                v = GetPosition(i, u);

                for (int level = 0; level < 4; level++)
                {
                    s = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
                    s.name = "Zone " + i + " " + u + " Level " + level;
                    scenesToSave.Add(s);
                    EditorSceneManager.SaveScene(s, path + "/Row " + i + "/" + u + "/Zone " + i + " " + u + " Level " + level + ".unity");

                    s = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);

                    if(level == 3)
                    {
                        g = Instantiate(terrain[level], v, q);
                        mesh = new Mesh();
                        g.GetComponent<MeshFilter>().sharedMesh = mesh;
                        AssetDatabase.CreateAsset(mesh, path + "/Row " + i + "/" + u + "/TerrainMesh " + i + " " + u + ".fbx");
                    }
                    else
                    {
                        g = Instantiate(terrain[level], v - new Vector3(64f,0f,64f), q);
                        td = CreateTerrainData(level);
                        g.GetComponent<Terrain>().terrainData = td;
                        g.GetComponent<TerrainCollider>().terrainData = td;
                        AssetDatabase.CreateAsset(td, path + "/Row " + i + "/" + u + "/TerrainData " + i + " " + u + " Level " + level + ".asset");
                    }
                    AssetDatabase.SaveAssets();
                    EditorSceneManager.MoveGameObjectToScene(g, s);
                    s.name = "Terrain " + i + " " + u + " Level " + level;
                    scenesToSave.Add(s);
                    EditorSceneManager.SaveScene(s, path + "/Row " + i + "/" + u + "/Terrain " + i + " " + u + " Level " + level + ".unity");
                }
            }

            for(int save = 0; save < scenesToSave.Count; save++)
            {
                EditorSceneManager.UnloadSceneAsync(scenesToSave[save]);
            }
            scenesToSave.Clear();
        }
    }

    private TerrainData CreateTerrainData(int index)
    {
        TerrainData d = new TerrainData
        {
            alphamapResolution = terrainData[index].alphamapResolution,
            baseMapResolution = terrainData[index].baseMapResolution
        };

        d.SetDetailResolution(terrainData[index].detailResolution, terrainData[index].detailResolutionPerPatch);

        d.heightmapResolution = terrainData[index].heightmapResolution;
        d.size = terrainData[index].size;

        d.wavingGrassAmount = terrainData[index].wavingGrassAmount;
        d.wavingGrassSpeed = terrainData[index].wavingGrassSpeed;
        d.wavingGrassStrength = terrainData[index].wavingGrassStrength;
        d.wavingGrassTint = terrainData[index].wavingGrassTint;

        return d;
    }

    #endregion

    private Vector3 GetPosition(int x, int z)
    {      
        return new Vector3((x - 15.5f) * 128f, 0f, (z - 15.5f) * 128f);
    }
}
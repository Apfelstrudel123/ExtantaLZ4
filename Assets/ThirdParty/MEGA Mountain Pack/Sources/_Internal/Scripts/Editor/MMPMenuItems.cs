using UnityEngine;
using UnityEditor;
using System.IO;

public class MMPMenuItems : EditorWindow
{
    static MMPMenuItems window;


    static string[] fileNamesMain = new string[7]
    {
        "MMP Bake Colormap",
        "MMP Colormap Detail Snow",
        "MMP Colormap Detail",
        "MMP Colormap Snow",
        "MMP Colormap",
        "MMP Complete Detail",
        "MMP Complete"
    };
    static string[] fileNamesDemo = new string[2]
    {
        "MMP Demo Base",
        "MMP Demo Rim"
    };


    [MenuItem("Window/MEGA Mountain Pack/Convert Shaders to Renderpipeline/Standard (Built-In RP)")]
    public static void Builtin() => UpdateShaders("Standard");

    [MenuItem("Window/MEGA Mountain Pack/Convert Shaders to Renderpipeline/URP2019")]
    public static void URP2019() => UpdateShaders("URP2019");

    [MenuItem("Window/MEGA Mountain Pack/Convert Shaders to Renderpipeline/URP2020")]
    public static void URP2020() => UpdateShaders("URP2020");

    [MenuItem("Window/MEGA Mountain Pack/Convert Shaders to Renderpipeline/HDRP2019")]
    public static void HDRP2019() => UpdateShaders("HDRP2019");

    [MenuItem("Window/MEGA Mountain Pack/Convert Shaders to Renderpipeline/HDRP2020")]
    public static void HDRP2020() => UpdateShaders("HDRP2020");


    static void UpdateShaders(string rp)
    {
        Shader shader = Shader.Find("Hidden/MMP/Bake Colormap");
        if (shader == null)
        {
            Debug.LogError("Shader conversion failed, please contact support at: becoming.at@gmail.com\nError: Shader could not be found");
            return;
        }
        string path = AssetDatabase.GetAssetPath(shader);
        path = path.Remove(path.IndexOf(fileNamesMain[0] + ".shader"));

        string[] pathsTarget = new string[7];
        for (int i = 0; i < fileNamesMain.Length; i++) pathsTarget[i] = path + fileNamesMain[i] + ".shader";

        string[] pathsSource = new string[7];
        for (int i = 0; i < fileNamesMain.Length; i++) pathsSource[i] = path + "RP Sources/" + fileNamesMain[i] + "_" + rp + ".txt";

        for (int i = 0; i < pathsSource.Length; i++)
        {
            StreamReader reader = new StreamReader(pathsSource[i]);
            string s = reader.ReadToEnd();
            reader.Close();

            s = RemoveWarnings(s);

            StreamWriter writer = new StreamWriter(pathsTarget[i], false);
            writer.Write(s);
            writer.Close();
        }

        // Demo Shaders
        shader = Shader.Find("MMP/Demo Scene/Base");
        if (shader != null)
        {
            path = AssetDatabase.GetAssetPath(shader);
            path = path.Remove(path.IndexOf(fileNamesDemo[0] + ".shader"));

            pathsTarget = new string[2];
            for (int i = 0; i < fileNamesDemo.Length; i++) pathsTarget[i] = path + fileNamesDemo[i] + ".shader";

            pathsSource = new string[2];
            for (int i = 0; i < fileNamesDemo.Length; i++) pathsSource[i] = path + "RP Sources/" + fileNamesDemo[i] + "_" + rp + ".txt";

            for (int i = 0; i < pathsSource.Length; i++)
            {
                StreamReader reader = new StreamReader(pathsSource[i]);
                string s = reader.ReadToEnd();
                reader.Close();

                s = RemoveWarnings(s);

                StreamWriter writer = new StreamWriter(pathsTarget[i], false);
                writer.Write(s);
                writer.Close();
            }
        }
        AssetDatabase.Refresh();
    }

    static string RemoveWarnings(string s)
    {
        s = s.Replace("#undef UNITY_MATRIX_MVP", "");
        s = s.Replace("#define UNITY_MATRIX_MVP   mul(UNITY_MATRIX_VP, UNITY_MATRIX_M)", "");
        s = s.Replace("#undef UNITY_MATRIX_MV", "");
        s = s.Replace("#define UNITY_MATRIX_MV    mul(UNITY_MATRIX_V, UNITY_MATRIX_M)", "");
        s = s.Replace("#define UNITY_MATRIX_T_MV  transpose(UNITY_MATRIX_MV)", "");
        return s;
    }





    //[MenuItem("Window/MEGA Mountain Pack/Apply a Material to all MeshRenderers in the Selection")]
    //public static void Init()
    //{
    //    window = GetWindow(typeof(MMPMenuItems)) as MMPMenuItems;
    //    window.titleContent = new GUIContent("Set LODs");
    //    //window.minSize = new Vector2(771, 566);
    //    //window.minSize = new Vector2(66, 110);
    //    window.Show();
    //}

    //void OnEnable() { count = 0; }

    //List<LODGroup> LODGroups = new List<LODGroup>();
    //SerializedObject masterLODGroup;
    //SerializedProperty distances;
    //static float[] LODscreensizes = { 1f, 0.857143f, 0.714286f, 0.571429f, 0.428572f, 0.285715f, 0.142858f, 0.01f };
    //static Material mat;

    //float count;

    //public void OnGUI()
    //{
    //    mat = EditorGUILayout.ObjectField("Material", mat, typeof(Material), false) as Material;

    //    if (Selection.gameObjects.Length == 0 || mat == null) GUI.color = new Color(1,1,1,0.5f);

    //    if (GUILayout.Button("Apply Material to all MeshRenderers in the Selection"))
    //    {
    //        count = 0;
    //        if (Selection.gameObjects.Length == 0 || mat == null) return;
    //        foreach (GameObject go in Selection.gameObjects)
    //        {
    //            MeshRenderer[] mr = go.GetComponentsInChildren<MeshRenderer>(true);
    //            foreach (MeshRenderer m in mr) { m.sharedMaterial = mat; count++; }
    //        }
    //    }
    //    GUI.color = Color.white;
    //    if (count != 0)
    //    {
    //        EditorGUILayout.HelpBox("The Material \"" + mat.name + "\" has been applied to " + count.ToString() + " Mesh Renderers.", MessageType.Info);
    //    }
    //    if (Selection.gameObjects.Length == 0 || mat == null)
    //    {
    //        EditorGUILayout.HelpBox("Assign a material in the slot above and select some Gameobjects which have Mesh Renderers in the hierarchy.", MessageType.Info);
    //    }

    //    if (GUILayout.Button("Get all LODGroups in Selection"))
    //    {
    //        LODGroups.Clear();
    //        foreach (GameObject go in Selection.gameObjects)
    //        {
    //            LODGroup lg = go.GetComponent<LODGroup>();
    //            if (lg != null)
    //            {
    //                LODGroups.Add(lg);
    //            }
    //        }
    //    }

    //    if (LODGroups.Count > 0)
    //    {
    //        if (GUILayout.Button("SetLODDistances"))
    //        {
    //            for (int i = 0; i < LODGroups.Count; i++)
    //            {
    //                LODGroup lg = LODGroups[i];
    //                SerializedObject so = new SerializedObject(LODGroups[i]);

    //                for (int j = 0; j < lg.lodCount; j++)
    //                {
    //                    SerializedProperty prop = so.FindProperty("m_LODs.Array.data[" + j.ToString() + "].screenRelativeHeight");
    //                    prop.doubleValue = LODscreensizes[j];
    //                }
    //                so.ApplyModifiedProperties();
    //            }
    //        }
    //    }
    //}
}

using UnityEngine;
using UnityEditor;
using MMP;

namespace MMP
{
    [CustomEditor(typeof(BakeColormap))]
    public class BakeColormapEditor : Editor
    {
        BakeColormap bcm;
        GUIContent[] resolutions = new GUIContent[] { new GUIContent("128"), new GUIContent("256"), new GUIContent("512"), new GUIContent("1024"), new GUIContent("2048"), new GUIContent("4096"), new GUIContent("8192"), /*new GUIContent("16384")*/ };
        int selectedIndex = 4;
        string installPath;
        float contextWidth;
        Rect r = new Rect();
        SerializedProperty sourceMat;

        // Material Properties
        SerializedProperty _RockColorBright;
        SerializedProperty _RockColorDark;
        SerializedProperty _DirtColorBright;
        SerializedProperty _DirtColorDark;
        SerializedProperty _GrassColorBright;
        SerializedProperty _GrassColorDark;
        SerializedProperty _FlowColor;
        SerializedProperty _SnowColor;
        SerializedProperty _RockScale;
        SerializedProperty _DirtScale;
        SerializedProperty _GrassScale;
        SerializedProperty _SedimentScale;
        SerializedProperty _OcclusionToAlbedo;
        SerializedProperty _OcclusionIntensity;
        SerializedProperty _SedimentIntensity;
        SerializedProperty _SedimentDistortion;
        SerializedProperty _DirtSlopeMin;
        SerializedProperty _DirtSlopeMax;
        SerializedProperty _GrassSlopeMin;
        SerializedProperty _GrassSlopeMax;
        SerializedProperty _GrassHeightMin;
        SerializedProperty _GrassHeightMax;
        SerializedProperty _GrassHeightMod;
        SerializedProperty _FlowIntensity;
        SerializedProperty _SnowSlopeMin;
        SerializedProperty _SnowSlopeMax;
        SerializedProperty _SnowHeightMin;
        SerializedProperty _SnowHeightMax;
        SerializedProperty _SnowHeightMod;
        //==================================================================================================================================================================================
        //==================================================================================================================================================================================
        //==================================================================================================================================================================================

        void SetupSerializedProperties()
        {
            _RockColorBright = serializedObject.FindProperty("_RockColorBright");
            _RockColorDark = serializedObject.FindProperty("_RockColorDark");
            _DirtColorBright = serializedObject.FindProperty("_DirtColorBright");
            _DirtColorDark = serializedObject.FindProperty("_DirtColorDark");
            _GrassColorBright = serializedObject.FindProperty("_GrassColorBright");
            _GrassColorDark = serializedObject.FindProperty("_GrassColorDark");
            _FlowColor = serializedObject.FindProperty("_FlowColor");
            _SnowColor = serializedObject.FindProperty("_SnowColor");

            _RockScale = serializedObject.FindProperty("_RockScale");
            _DirtScale = serializedObject.FindProperty("_DirtScale");
            _GrassScale = serializedObject.FindProperty("_GrassScale");
            _SedimentScale = serializedObject.FindProperty("_SedimentScale");

            _OcclusionToAlbedo = serializedObject.FindProperty("_OcclusionToAlbedo");
            _OcclusionIntensity = serializedObject.FindProperty("_OcclusionIntensity");
            _SedimentIntensity = serializedObject.FindProperty("_SedimentIntensity");
            _SedimentDistortion = serializedObject.FindProperty("_SedimentDistortion");
            _DirtSlopeMin = serializedObject.FindProperty("_DirtSlopeMin");
            _DirtSlopeMax = serializedObject.FindProperty("_DirtSlopeMax");
            _GrassSlopeMin = serializedObject.FindProperty("_GrassSlopeMin");
            _GrassSlopeMax = serializedObject.FindProperty("_GrassSlopeMax");
            _GrassHeightMin = serializedObject.FindProperty("_GrassHeightMin");
            _GrassHeightMax = serializedObject.FindProperty("_GrassHeightMax");
            _GrassHeightMod = serializedObject.FindProperty("_GrassHeightMod");
            _FlowIntensity = serializedObject.FindProperty("_FlowIntensity");
            _SnowSlopeMin = serializedObject.FindProperty("_SnowSlopeMin");
            _SnowSlopeMax = serializedObject.FindProperty("_SnowSlopeMax");
            _SnowHeightMin = serializedObject.FindProperty("_SnowHeightMin");
            _SnowHeightMax = serializedObject.FindProperty("_SnowHeightMax");
            _SnowHeightMod = serializedObject.FindProperty("_SnowHeightMod");
        } //==================================================================================================================================================================================


        void OnEnable()
        {
            bcm = (BakeColormap)target;
            string scriptLocation = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this));
            installPath = scriptLocation.Replace("/Sources/_Internal/Scripts/Editor/BakeColormapEditor.cs", "");

            SetupSerializedProperties();

            bcm.GetMaterials();
            bcm.UpdateMaterials();
            bcm.GetValuesFromFirstMaterial();
            bcm.transform.hideFlags = HideFlags.HideInInspector;
            foreach (Transform t in bcm.transform)
            {
                if (t == bcm.transform) continue;
                t.hideFlags = HideFlags.HideInInspector; //HideFlags.None;
                t.hideFlags = HideFlags.HideInHierarchy; //HideFlags.None;
                SceneVisibilityManager.instance.DisablePicking(t.gameObject, false); //SceneVisibilityManager.instance.EnablePicking(t.gameObject, false);
            }
            SceneVisibilityManager.instance.EnablePicking(bcm.transform.gameObject, false);

            SceneView.RepaintAll();

            Undo.undoRedoPerformed += UndoCallback;
        } //==================================================================================================================================================================================


        void UndoCallback()
        {
            bcm.UpdateMaterials();
        } //==================================================================================================================================================================================


        public override void OnInspectorGUI()
        {
            //Tools.current = Tool.None;

            if (bcm.materials.Count == 0) return;
            contextWidth = EditorGUIUtility.currentViewWidth;

            EditorGUIUtility.labelWidth = contextWidth - 200;

            serializedObject.Update();

            selectedIndex = EditorGUILayout.Popup(MMPGUI.textureResolution, selectedIndex, resolutions);
            bcm.includeSnow = EditorGUILayout.Toggle(MMPGUI.includeSnow, bcm.includeSnow);


            UnityEngine.GUI.color = new Color32(124, 168, 56, 255);
            if (GUILayout.Button("Bake Colormap and save to PNG", GUILayout.Height(32)))
            {
                switch (selectedIndex)
                {
                    case 0: bcm.resolution = 128; break;
                    case 1: bcm.resolution = 256; break;
                    case 2: bcm.resolution = 512; break;
                    case 3: bcm.resolution = 1024; break;
                    case 4: bcm.resolution = 2048; break;
                    case 5: bcm.resolution = 4096; break;
                    case 6: bcm.resolution = 8192; break;
                    case 7: bcm.resolution = 16384; break;
                }
                bcm.textureName = bcm.gameObject.name + "_ColorMap";
                bcm.defaultDirectory = installPath + "/Colormap Baker/Baked Colormaps";
                bcm.Bake();
            }
            UnityEngine.GUI.color = Color.white;

            GUILayout.Space(2);
            EditorGUI.DrawRect(EditorGUILayout.GetControlRect(GUILayout.Height(1)), Color.black);
            EditorGUI.DrawRect(EditorGUILayout.GetControlRect(GUILayout.Height(1)), Color.black);
            GUILayout.Space(2);

            CopyMaterialsGUI();

            //==================================================================================
            MMPGUI.StartSection(MMPGUI.colorMainSettings);
            EditorGUILayout.PropertyField(_OcclusionIntensity, MMPGUI.occlusionIntensity);
            EditorGUILayout.PropertyField(_OcclusionToAlbedo, MMPGUI.occlusionToAlbedo);
            MMPGUI.EndSection();
            //==================================================================================
            MMPGUI.StartSection(MMPGUI.colorRockSettings);
            EditorGUILayout.PropertyField(_RockScale, MMPGUI.rockScale);
            MMPGUI.DoubleField(_RockColorBright, _RockColorDark, MMPGUI.rockColors);
            EditorGUILayout.PropertyField(_SedimentScale, MMPGUI.sedimentScale);
            EditorGUILayout.PropertyField(_SedimentIntensity, MMPGUI.sedimentIntensity);
            EditorGUILayout.PropertyField(_SedimentDistortion, MMPGUI.sedimentDistoriton);
            MMPGUI.EndSection();
            //==================================================================================
            MMPGUI.StartSection(MMPGUI.colorDirtSetings);
            EditorGUILayout.PropertyField(_DirtScale, MMPGUI.dirtScale);
            MMPGUI.DoubleField(_DirtColorBright, _DirtColorDark, MMPGUI.dirtColors);
            MMPGUI.MinMaxSlider(_DirtSlopeMin, _DirtSlopeMax, MMPGUI.dirtSlopeRange);
            MMPGUI.EndSection();
            //==================================================================================
            MMPGUI.StartSection(MMPGUI.colorGrassSettings);
            EditorGUILayout.PropertyField(_GrassScale, MMPGUI.grassScale);
            MMPGUI.DoubleField(_GrassColorBright, _GrassColorDark, MMPGUI.grassColors);
            MMPGUI.MinMaxSlider(_GrassSlopeMin, _GrassSlopeMax, MMPGUI.grassSlopeRange);
            MMPGUI.DoubleField(_GrassHeightMin, _GrassHeightMax, MMPGUI.grassHeightRange);
            EditorGUILayout.PropertyField(_GrassHeightMod, MMPGUI.grassHeightModulation);
            MMPGUI.EndSection();
            //==================================================================================
            MMPGUI.StartSection(MMPGUI.colorFlowSettings);
            EditorGUILayout.PropertyField(_FlowColor, MMPGUI.flowColor);
            EditorGUILayout.PropertyField(_FlowIntensity, MMPGUI.flowIntensity);
            MMPGUI.EndSection();
            //==================================================================================
            MMPGUI.StartSection(MMPGUI.colorSnowSettings);
            EditorGUILayout.PropertyField(_SnowColor, MMPGUI.snowColor);
            MMPGUI.MinMaxSlider(_SnowSlopeMin, _SnowSlopeMax, MMPGUI.snowSlopeRange);
            MMPGUI.DoubleField(_SnowHeightMin, _SnowHeightMax, MMPGUI.snowHeightRange);
            EditorGUILayout.PropertyField(_SnowHeightMod, MMPGUI.snowHeightModulation);
            MMPGUI.EndSection();
            //==================================================================================
            serializedObject.ApplyModifiedProperties();
            if (UnityEngine.GUI.changed) bcm.UpdateMaterials();

            // Load / Save
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(MMPGUI.loadSettings, GUILayout.Width(contextWidth - 196)))
            {
                bcm.LoadPreset(EditorUtility.OpenFilePanel("Load Preset", installPath + "/Colormap Baker/Presets", "prefab"));
                return;
            }
            if (GUILayout.Button(MMPGUI.saveSettings))
                bcm.SavePreset(EditorUtility.SaveFilePanel("Save Preset", installPath + "/Colormap Baker/Presets", bcm.gameObject.name + " Preset 01", "prefab"));
            EditorGUILayout.EndHorizontal();
        } //========================================================================================









        void CopyMaterialsGUI()
        {
            if (bcm.sourceMat == null || !(bcm.sourceMat.shader == Shader.Find("MMP/Complete") || bcm.sourceMat.shader == Shader.Find("MMP/Complete Detail"))) UnityEngine.GUI.color = new Color(1, 1, 1, 0.4f);

            r = EditorGUILayout.GetControlRect();
            r.width = contextWidth - 198;
            if (UnityEngine.GUI.Button(r, "Copy Values from:"))
            {
                if (bcm.sourceMat != null)
                {
                    if (bcm.sourceMat.shader == Shader.Find("MMP/Complete") || bcm.sourceMat.shader == Shader.Find("MMP/Complete Detail")) bcm.CopyValuesFromMaterial();
                }
            }
            UnityEngine.GUI.color = Color.white;
            r.x = r.width + 20; r.width = 150;
            bcm.sourceMat = EditorGUI.ObjectField(r, "", bcm.sourceMat, typeof(Material), false) as Material;
            if (bcm.sourceMat != null)
            {
                if (!(bcm.sourceMat.shader == Shader.Find("MMP/Complete") || bcm.sourceMat.shader == Shader.Find("MMP/Complete Detail")))
                {
                    EditorGUILayout.HelpBox("The material you assigned uses a different shader, you can only copy values from a material which uses the following shaders:\nMMP / Complete\nMMP / Complete Detail", MessageType.Warning);
                }
            }
        } //========================================================================================
    }
}

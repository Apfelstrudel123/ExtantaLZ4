using UnityEngine;
using UnityEditor;
using MMP;

public class MMPMaterialInspector : ShaderGUI
{
    Material m;
    MaterialEditor mEditor;
    static Material source;
    Rect r = new Rect();
    static bool showAssets;
    static float contextWidth;
    string shaderPath;

    MaterialProperty _BaseMap;
    MaterialProperty _SplatMap;
    MaterialProperty _OcclusionToAlbedo;
    MaterialProperty _OcclusionIntensity;
    MaterialProperty _ColorMap;
    MaterialProperty _Detail;
    MaterialProperty _DetailScale;
    MaterialProperty _DetailColor;
    MaterialProperty _DetailNormal;
    MaterialProperty _DetailAO;
    MaterialProperty _Rock;
    MaterialProperty _RockScale;
    MaterialProperty _RockColorBright;
    MaterialProperty _RockColorDark;
    MaterialProperty _Sediment;
    MaterialProperty _SedimentScale;
    MaterialProperty _SedimentIntensity;
    MaterialProperty _SedimentDistortion;
    MaterialProperty _Dirt;
    MaterialProperty _DirtScale;
    MaterialProperty _DirtColorBright;
    MaterialProperty _DirtColorDark;
    MaterialProperty _DirtSlopeMin;
    MaterialProperty _DirtSlopeMax;
    MaterialProperty _Grass;
    MaterialProperty _GrassScale;
    MaterialProperty _GrassColorBright;
    MaterialProperty _GrassColorDark;
    MaterialProperty _GrassSlopeMin;
    MaterialProperty _GrassSlopeMax;
    MaterialProperty _GrassHeightMin;
    MaterialProperty _GrassHeightMax;
    MaterialProperty _GrassHeightMod;
    MaterialProperty _FlowColor;
    MaterialProperty _FlowIntensity;
    MaterialProperty _SnowColor;
    MaterialProperty _SnowSlopeMin;
    MaterialProperty _SnowSlopeMax;
    MaterialProperty _SnowHeightMin;
    MaterialProperty _SnowHeightMax;
    MaterialProperty _SnowHeightMod;



    void FindProperties(MaterialProperty[] properties)
    {
        if (m.HasProperty("_BaseMap")) _BaseMap = FindProperty("_BaseMap", properties);
        if (m.HasProperty("_SplatMap")) _SplatMap = FindProperty("_SplatMap", properties);
        if (m.HasProperty("_Rock")) _Rock = FindProperty("_Rock", properties);
        if (m.HasProperty("_Dirt")) _Dirt = FindProperty("_Dirt", properties);
        if (m.HasProperty("_Grass")) _Grass = FindProperty("_Grass", properties);
        if (m.HasProperty("_Sediment")) _Sediment = FindProperty("_Sediment", properties);
        if (m.HasProperty("_ColorMap")) _ColorMap = FindProperty("_ColorMap", properties);
        if (m.HasProperty("_Detail")) _Detail = FindProperty("_Detail", properties);
        if (m.HasProperty("_DetailScale")) _DetailScale = FindProperty("_DetailScale", properties);
        if (m.HasProperty("_DetailColor")) _DetailColor = FindProperty("_DetailColor", properties);
        if (m.HasProperty("_DetailNormal")) _DetailNormal = FindProperty("_DetailNormal", properties);
        if (m.HasProperty("_DetailAO")) _DetailAO = FindProperty("_DetailAO", properties);
        if (m.HasProperty("_RockColorBright")) _RockColorBright = FindProperty("_RockColorBright", properties);
        if (m.HasProperty("_RockColorDark")) _RockColorDark = FindProperty("_RockColorDark", properties);
        if (m.HasProperty("_DirtColorBright")) _DirtColorBright = FindProperty("_DirtColorBright", properties);
        if (m.HasProperty("_DirtColorDark")) _DirtColorDark = FindProperty("_DirtColorDark", properties);
        if (m.HasProperty("_GrassColorBright")) _GrassColorBright = FindProperty("_GrassColorBright", properties);
        if (m.HasProperty("_GrassColorDark")) _GrassColorDark = FindProperty("_GrassColorDark", properties);
        if (m.HasProperty("_FlowColor")) _FlowColor = FindProperty("_FlowColor", properties);
        if (m.HasProperty("_SnowColor")) _SnowColor = FindProperty("_SnowColor", properties);
        if (m.HasProperty("_RockScale")) _RockScale = FindProperty("_RockScale", properties);
        if (m.HasProperty("_DirtScale")) _DirtScale = FindProperty("_DirtScale", properties);
        if (m.HasProperty("_GrassScale")) _GrassScale = FindProperty("_GrassScale", properties);
        if (m.HasProperty("_SedimentScale")) _SedimentScale = FindProperty("_SedimentScale", properties);
        if (m.HasProperty("_OcclusionToAlbedo")) _OcclusionToAlbedo = FindProperty("_OcclusionToAlbedo", properties);
        if (m.HasProperty("_OcclusionIntensity")) _OcclusionIntensity = FindProperty("_OcclusionIntensity", properties);
        if (m.HasProperty("_SedimentIntensity")) _SedimentIntensity = FindProperty("_SedimentIntensity", properties);
        if (m.HasProperty("_SedimentDistortion")) _SedimentDistortion = FindProperty("_SedimentDistortion", properties);
        if (m.HasProperty("_DirtSlopeMin")) _DirtSlopeMin = FindProperty("_DirtSlopeMin", properties);
        if (m.HasProperty("_DirtSlopeMax")) _DirtSlopeMax = FindProperty("_DirtSlopeMax", properties);
        if (m.HasProperty("_GrassSlopeMin")) _GrassSlopeMin = FindProperty("_GrassSlopeMin", properties);
        if (m.HasProperty("_GrassSlopeMax")) _GrassSlopeMax = FindProperty("_GrassSlopeMax", properties);
        if (m.HasProperty("_GrassHeightMin")) _GrassHeightMin = FindProperty("_GrassHeightMin", properties);
        if (m.HasProperty("_GrassHeightMax")) _GrassHeightMax = FindProperty("_GrassHeightMax", properties);
        if (m.HasProperty("_GrassHeightMod")) _GrassHeightMod = FindProperty("_GrassHeightMod", properties);
        if (m.HasProperty("_FlowIntensity")) _FlowIntensity = FindProperty("_FlowIntensity", properties);
        if (m.HasProperty("_SnowSlopeMin")) _SnowSlopeMin = FindProperty("_SnowSlopeMin", properties);
        if (m.HasProperty("_SnowSlopeMax")) _SnowSlopeMax = FindProperty("_SnowSlopeMax", properties);
        if (m.HasProperty("_SnowHeightMin")) _SnowHeightMin = FindProperty("_SnowHeightMin", properties);
        if (m.HasProperty("_SnowHeightMax")) _SnowHeightMax = FindProperty("_SnowHeightMax", properties);
        if (m.HasProperty("_SnowHeightMod")) _SnowHeightMod = FindProperty("_SnowHeightMod", properties);
    } //========================================================================================


    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        mEditor = materialEditor;
        m = materialEditor.target as Material;
        FindProperties(properties);

        contextWidth = EditorGUIUtility.currentViewWidth;
        EditorGUIUtility.labelWidth = contextWidth - 200;

        CopyMaterialsGUI();
        //==================================================================================
        if (m.HasProperty("_OcclusionIntensity"))
        {
            MMPGUI.StartSection(MMPGUI.colorMainSettings);
            MMPGUI.Slider(_OcclusionIntensity, MMPGUI.occlusionIntensity, 0, 1);
            MMPGUI.Slider(_OcclusionToAlbedo, MMPGUI.occlusionToAlbedo, 0, 1);
            MMPGUI.EndSection();
        }//==================================================================================
        if (m.HasProperty("_Rock"))
        {
            MMPGUI.StartSection(MMPGUI.colorRockSettings);
            MMPGUI.FloatField(_RockScale, MMPGUI.rockScale);
            MMPGUI.DoubleColorField(_RockColorBright, _RockColorDark, MMPGUI.rockColors);
            MMPGUI.FloatField(_SedimentScale, MMPGUI.sedimentScale);
            MMPGUI.Slider(_SedimentIntensity, MMPGUI.sedimentIntensity, 0, 1);
            MMPGUI.FloatField(_SedimentDistortion, MMPGUI.sedimentDistoriton);
            MMPGUI.EndSection();
        }//==================================================================================
        if (m.HasProperty("_Dirt"))
        {
            MMPGUI.StartSection(MMPGUI.colorDirtSetings);
            MMPGUI.FloatField(_DirtScale, MMPGUI.dirtScale);
            MMPGUI.DoubleColorField(_DirtColorBright, _DirtColorDark, MMPGUI.dirtColors);
            MMPGUI.MinMaxSlider(_DirtSlopeMin, _DirtSlopeMax, MMPGUI.dirtSlopeRange);
            MMPGUI.EndSection();
        }//==================================================================================
        if (m.HasProperty("_Grass"))
        {
            MMPGUI.StartSection(MMPGUI.colorGrassSettings);
            MMPGUI.FloatField(_GrassScale, MMPGUI.grassScale);
            MMPGUI.DoubleColorField(_GrassColorBright, _GrassColorDark, MMPGUI.grassColors);
            MMPGUI.MinMaxSlider(_GrassSlopeMin, _GrassSlopeMax, MMPGUI.grassSlopeRange);
            MMPGUI.DoubleFloatField(mEditor, _GrassHeightMin, _GrassHeightMax, MMPGUI.grassHeightRange);
            MMPGUI.FloatField(_GrassHeightMod, MMPGUI.grassHeightModulation);
            MMPGUI.EndSection();
        }//==================================================================================
        if (m.HasProperty("_FlowColor"))
        {
            MMPGUI.StartSection(MMPGUI.colorFlowSettings);
            MMPGUI.ColorField(_FlowColor, MMPGUI.flowColor);
            MMPGUI.Slider(_FlowIntensity, MMPGUI.flowIntensity, 0, 1);
            MMPGUI.EndSection();
        }//==================================================================================
        if (m.HasProperty("_SnowColor"))
        {
            MMPGUI.StartSection(MMPGUI.colorSnowSettings);
            MMPGUI.ColorField(_SnowColor, MMPGUI.snowColor);
            MMPGUI.MinMaxSlider(_SnowSlopeMin, _SnowSlopeMax, MMPGUI.snowSlopeRange);
            MMPGUI.DoubleFloatField(mEditor, _SnowHeightMin, _SnowHeightMax, MMPGUI.snowHeightRange);
            MMPGUI.FloatField(_SnowHeightMod, MMPGUI.snowHeightModulation);
            MMPGUI.EndSection();
        }//==================================================================================
        if (m.HasProperty("_DetailScale"))
        {
            MMPGUI.StartSection(MMPGUI.colorDetailSettings);
            MMPGUI.FloatField(_DetailScale, MMPGUI.detailScale);
            MMPGUI.Slider(_DetailColor, MMPGUI.detailColorIntensity, 0, 1);
            MMPGUI.Slider(_DetailNormal, MMPGUI.detailNormalIntensity, 0, 1);
            MMPGUI.Slider(_DetailAO, MMPGUI.detailOcclusionIntensity, 0, 1);
            MMPGUI.EndSection();
        }//==================================================================================
        if (showAssets = MMPGUI.StartSectionFoldout("Textures", showAssets, MMPGUI.colorAssetReferences))
        {
            r = EditorGUILayout.GetControlRect(false, 0);
            r.width = (r.width / 6) - 3; r.height = 16;

            if (m.HasProperty("_BaseMap"))  { EditorGUI.LabelField(r, "Normal");   r.x += r.width + 3.5f; }
            if (m.HasProperty("_SplatMap")) { EditorGUI.LabelField(r, "Splat");    r.x += r.width + 3.5f; }
            if (m.HasProperty("_Rock"))     { EditorGUI.LabelField(r, "Rock");     r.x += r.width + 3.5f; }
            if (m.HasProperty("_Dirt"))     { EditorGUI.LabelField(r, "Dirt");     r.x += r.width + 3.5f; }
            if (m.HasProperty("_Grass"))    { EditorGUI.LabelField(r, "Grass");    r.x += r.width + 3.5f; }
            if (m.HasProperty("_Sediment")) { EditorGUI.LabelField(r, "Sediment"); r.x += r.width + 3.5f; }
            if (m.HasProperty("_ColorMap")) { EditorGUI.LabelField(r, "Color");    r.x += r.width + 3.5f; }
            if (m.HasProperty("_Detail"))   { EditorGUI.LabelField(r, "Detail");   r.x += r.width + 3.5f; }


            r = EditorGUILayout.GetControlRect(false, 0, GUILayout.Height(contextWidth / 6 + 8));
            r.width = (r.width / 6) - 3; r.height = r.width;
            r.y += 16;

            if (m.HasProperty("_BaseMap"))  MMPGUI.TextureField(ref r, _BaseMap);
            if (m.HasProperty("_SplatMap")) MMPGUI.TextureField(ref r, _SplatMap);
            if (m.HasProperty("_Rock"))     MMPGUI.TextureField(ref r, _Rock);
            if (m.HasProperty("_Dirt"))     MMPGUI.TextureField(ref r, _Dirt);
            if (m.HasProperty("_Grass"))    MMPGUI.TextureField(ref r, _Grass);
            if (m.HasProperty("_Sediment")) MMPGUI.TextureField(ref r, _Sediment);
            if (m.HasProperty("_ColorMap")) MMPGUI.TextureField(ref r, _ColorMap);
            if (m.HasProperty("_Detail"))   MMPGUI.TextureField(ref r, _Detail);
        }
        MMPGUI.EndSection();
        //===================================================================================

        m.enableInstancing = EditorGUILayout.ToggleLeft("Enable Instancing", m.enableInstancing);

        GUILayout.Space(5);
    } //========================================================================================




    

    void CopyMaterialsGUI()
    {
        Shader[] compatibleShaders = new Shader[] 
        {
            Shader.Find("MMP/Complete"),
            Shader.Find("MMP/Complete Detail"),
            Shader.Find("MMP/Colormap"),
            Shader.Find("MMP/Colormap Detail"),
            Shader.Find("MMP/Colormap Snow"),
            Shader.Find("MMP/Colormap Detail Snow")
        };


        bool isCompatible = false;
        if (source != null) for (int i = 0; i < compatibleShaders.Length; i++) if (source.shader == compatibleShaders[i]) isCompatible = true;


        if (!isCompatible || source == m) UnityEngine.GUI.color = new Color(1, 1, 1, 0.4f);

        r = EditorGUILayout.GetControlRect();
        r.width -= 154;
        if (UnityEngine.GUI.Button(r, new GUIContent("Copy Values from:", "This can be used to copy the settings from another material. Note: This is obviously only compatible with MMP materials.")))
        {
            if (source != null)
            {
                //if (source.shader == m.shader)
                CopyValuesFromMaterial();
            }
        }
        UnityEngine.GUI.color = Color.white;
        r.x = r.width + 20; r.width = 150;
        source = EditorGUI.ObjectField(r, "", source, typeof(Material), false) as Material;
        if (source != null)
        {
            if (!isCompatible)
            {
                EditorGUILayout.HelpBox("The material you assigned uses a different shader, you can only copy values from a material which uses the same shader (" + m.shader.name + ") as this material.", MessageType.Warning);
            }
            if (source == m) EditorGUILayout.HelpBox("Copying values from itself does nothing ;)", MessageType.Info);
        }

        //r.x += r.width + 2; r.width = 26;
        //if (GUI.Button(r, "RP"))
        //{
        //    shaderPath = AssetDatabase.GetAssetPath(m.shader);
        //    shaderPath = shaderPath.Remove(shaderPath.IndexOf("MMP "));

        //    var menu = new GenericMenu();

        //    var function = new GenericMenu.MenuFunction2((filename) => { Debug.Log(filename); });

        //    menu.AddDisabledItem(new GUIContent("Select Renderpipeline"));
        //    menu.AddSeparator("");
        //    menu.AddItem(new GUIContent("Built-In", "Tooltip"), false, function, shaderPath);
        //    menu.AddItem(new GUIContent("URP 2019"), false, function, "Hey 2");
        //    menu.AddItem(new GUIContent("URP 2020"), true, function, "Hey 3");
        //    menu.AddItem(new GUIContent("HDRP 2019"), false, function, "Hey 4");
        //    menu.AddItem(new GUIContent("HDRP 2020"), false, function, "Hey 5");

        //    menu.ShowAsContext();
        //}
    } //========================================================================================


    void CopyValuesFromMaterial()
    {
        if (source.HasProperty("_RockColorBright"))    m.SetColor("_RockColorBright",    source.GetColor("_RockColorBright"));
        if (source.HasProperty("_RockColorDark"))      m.SetColor("_RockColorDark",      source.GetColor("_RockColorDark"));
        if (source.HasProperty("_DirtColorBright"))    m.SetColor("_DirtColorBright",    source.GetColor("_DirtColorBright"));
        if (source.HasProperty("_DirtColorDark"))      m.SetColor("_DirtColorDark",      source.GetColor("_DirtColorDark"));
        if (source.HasProperty("_GrassColorBright"))   m.SetColor("_GrassColorBright",   source.GetColor("_GrassColorBright"));
        if (source.HasProperty("_GrassColorDark"))     m.SetColor("_GrassColorDark",     source.GetColor("_GrassColorDark"));
        if (source.HasProperty("_FlowColor"))          m.SetColor("_FlowColor",          source.GetColor("_FlowColor"));
        if (source.HasProperty("_SnowColor"))          m.SetColor("_SnowColor",          source.GetColor("_SnowColor"));

        if (source.HasProperty("_RockScale"))          m.SetFloat("_RockScale",          source.GetFloat("_RockScale"));
        if (source.HasProperty("_DirtScale"))          m.SetFloat("_DirtScale",          source.GetFloat("_DirtScale"));
        if (source.HasProperty("_GrassScale"))         m.SetFloat("_GrassScale",         source.GetFloat("_GrassScale"));
        if (source.HasProperty("_SedimentScale"))      m.SetFloat("_SedimentScale",      source.GetFloat("_SedimentScale"));

        if (source.HasProperty("_OcclusionToAlbedo"))  m.SetFloat("_OcclusionToAlbedo",  source.GetFloat("_OcclusionToAlbedo"));
        if (source.HasProperty("_OcclusionIntensity")) m.SetFloat("_OcclusionIntensity", source.GetFloat("_OcclusionIntensity"));
        if (source.HasProperty("_SedimentIntensity"))  m.SetFloat("_SedimentIntensity",  source.GetFloat("_SedimentIntensity"));
        if (source.HasProperty("_SedimentDistortion")) m.SetFloat("_SedimentDistortion", source.GetFloat("_SedimentDistortion"));
        if (source.HasProperty("_DirtSlopeMin"))       m.SetFloat("_DirtSlopeMin",       source.GetFloat("_DirtSlopeMin"));
        if (source.HasProperty("_DirtSlopeMax"))       m.SetFloat("_DirtSlopeMax",       source.GetFloat("_DirtSlopeMax"));
        if (source.HasProperty("_GrassSlopeMin"))      m.SetFloat("_GrassSlopeMin",      source.GetFloat("_GrassSlopeMin"));
        if (source.HasProperty("_GrassSlopeMax"))      m.SetFloat("_GrassSlopeMax",      source.GetFloat("_GrassSlopeMax"));
        if (source.HasProperty("_GrassHeightMin"))     m.SetFloat("_GrassHeightMin",     source.GetFloat("_GrassHeightMin"));
        if (source.HasProperty("_GrassHeightMax"))     m.SetFloat("_GrassHeightMax",     source.GetFloat("_GrassHeightMax"));
        if (source.HasProperty("_GrassHeightMod"))     m.SetFloat("_GrassHeightMod",     source.GetFloat("_GrassHeightMod"));
        if (source.HasProperty("_FlowIntensity"))      m.SetFloat("_FlowIntensity",      source.GetFloat("_FlowIntensity"));
        if (source.HasProperty("_SnowSlopeMin"))       m.SetFloat("_SnowSlopeMin",       source.GetFloat("_SnowSlopeMin"));
        if (source.HasProperty("_SnowSlopeMax"))       m.SetFloat("_SnowSlopeMax",       source.GetFloat("_SnowSlopeMax"));
        if (source.HasProperty("_SnowHeightMin"))      m.SetFloat("_SnowHeightMin",      source.GetFloat("_SnowHeightMin"));
        if (source.HasProperty("_SnowHeightMax"))      m.SetFloat("_SnowHeightMax",      source.GetFloat("_SnowHeightMax"));
        if (source.HasProperty("_SnowHeightMod"))      m.SetFloat("_SnowHeightMod",      source.GetFloat("_SnowHeightMod"));
    } //========================================================================================
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MMP
{
    public static class MMPGUI
    {
        public static GUIContent textureResolution = new GUIContent(
            "Texture Resolution",
            "The resolution that the baked texture is going to have(2048 is default)."
            );
        public static GUIContent includeSnow = new GUIContent(
            "Include Snow",
            "Should snow be baked into the colormap or not?"
            );
        public static GUIContent loadSettings = new GUIContent(
            "Load Settings",
            "Here you can load previously saved settings from a file."
            );
        public static GUIContent saveSettings = new GUIContent(
            "Save Settings",
            "Here you can save the current settings to a file."
            );
        public static GUIContent occlusionIntensity = new GUIContent(
            "Occlusion Intensity",
            "Controls how much ambient occlusion will be applied."
            );
        public static GUIContent occlusionToAlbedo = new GUIContent(
            "Occlusion To Albedo",
            "Controls how much of the ambient occlusion should be copied to the albedo color."
            );
        public static GUIContent rockScale = new GUIContent(
            "Rock Scale",
            "Controls the tiling size of the rock layer in worldspace."
            );
        public static GUIContent rockColors = new GUIContent(
            "Rock Colors",
            "The colors of the rock layer. This is a gradient, the left color maps to the darkest parts and the right color maps to the brightest parts of the grayscale rock texture."
            );
        public static GUIContent sedimentScale = new GUIContent(
            "Sediment Scale",
            "Controls the scale of the sediment layers in the rock. These are horizontal stripes overlayed on the rock layer."
            );
        public static GUIContent sedimentIntensity = new GUIContent(
            "Sediment Intensity",
            "Controls the intensity of the sediment layers."
            );
        public static GUIContent sedimentDistoriton = new GUIContent(
            "Sediment Distortion",
            "This controls how much distortion is applied to the sediment layers."
            );
        public static GUIContent dirtScale = new GUIContent(
            "Dirt Scale",
            "Controls the tiling size of the dirt layer in worldspace."
            );
        public static GUIContent dirtColors = new GUIContent(
            "Dirt Colors",
            "The colors of the dirt layer. This is a gradient, the left color maps to the darkest parts and the right color maps to the brightest parts of the grayscale dirt texture."
            );
        public static GUIContent dirtSlopeRange = new GUIContent(
            "Dirt Slope Range",
            "This controls at which steepness range the dirt is applied. The further the handles are apart the smoother the transition. Note: This layer is drawn on top of the rock layer but below of the grass and snow layers."
            );
        public static GUIContent grassScale = new GUIContent(
            "Grass Scale",
            "Controls the tiling size of the grass layer in worldspace."
            );
        public static GUIContent grassColors = new GUIContent(
            "Grass Colors",
            "The colors of the grass layer. This is a gradient, the left color maps to the darkest parts and the right color maps to the brightest parts of the grayscale grass texture."
            );
        public static GUIContent grassSlopeRange = new GUIContent(
            "Grass Slope Range",
            "This controls at which steepness range the grass is applied. The further the handles are apart the smoother the transition. Note: This layer is drawn on top of the rock and dirt layers but below of the snow layer."
            );
        public static GUIContent grassHeightRange = new GUIContent(
            "Grass Height Range",
            "This controls at which height grass will be drawn. Everything below the left value will have full grass contribution and everything above the right value will have zero grass contribution."
            );
        public static GUIContent grassHeightModulation = new GUIContent(
            "Grass Height Modulation",
            "This controls how much the shape of the mountain affects(offsets) the grass height range."
            );
        public static GUIContent flowColor = new GUIContent(
            "Flow Color",
            "This is the color of the sediment flow, a result of hydraulic erosion."
            );
        public static GUIContent flowIntensity = new GUIContent(
            "Flow Intensity",
            "The intensity of the sediment flow."
            );
        public static GUIContent snowColor = new GUIContent(
            "Snow Color",
            "The color of the snow layer. This can alternatively be used as a second grass layer or as sand/dust. Be creative with it."
            );
        public static GUIContent snowSlopeRange = new GUIContent(
            "Snow Slope Range",
            "This controls at which steepness range the snow is applied. The further the handles are apart the smoother the transition. Note: This layer is drawn on top of all the other layers."
            );
        public static GUIContent snowHeightRange = new GUIContent(
            "Snow Height Range",
            "This controls at which height snow will be drawn. Everything below the left value will have zero snow contribution and everything above the right value will have full snow contribution."
            );
        public static GUIContent snowHeightModulation = new GUIContent(
            "Snow Height Modulation",
            "This controls how much the shape of the mountain affects(offsets) the snow height range."
            );
        public static GUIContent detailScale = new GUIContent(
            "Detail Scale",
            "Controls the tiling size of the detail layer in worldspace."
            );
        public static GUIContent detailColorIntensity = new GUIContent(
            "Detail Color Intensity",
            "Controls how much the detail layer affects the overall color."
            );
        public static GUIContent detailNormalIntensity = new GUIContent(
            "Detail Normal Intensity",
            "Controls how much the detail layer affects the normals."
            );
        public static GUIContent detailOcclusionIntensity = new GUIContent(
            "Detail Occlusion Intensity",
            "Controls how much the detail layer affects the overall ambient occlusion."
            );


        public static Color colorMainSettings    = new Color(0.5f, 0.0f, 1.0f, 0.5f);
        public static Color colorRockSettings    = new Color(0.0f, 0.5f, 1.0f, 0.5f);
        public static Color colorDirtSetings     = new Color(0.0f, 1.0f, 0.5f, 0.5f);
        public static Color colorGrassSettings   = new Color(0.5f, 1.0f, 0.0f, 0.5f);
        public static Color colorFlowSettings    = new Color(1.0f, 0.8f, 0.0f, 0.5f);
        public static Color colorSnowSettings    = new Color(1.0f, 0.5f, 0.0f, 0.5f);
        public static Color colorDetailSettings  = new Color(1.0f, 0.0f, 0.0f, 0.5f);
        public static Color colorAssetReferences = new Color(0.5f, 0.5f, 0.5f, 0.5f);










        public static void StartSection(Color color)
        {
            UnityEngine.GUI.backgroundColor = color;
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.BeginVertical("Box");
            UnityEngine.GUI.backgroundColor = Color.white;
        } //========================================================================================

        public static bool StartSectionFoldout(string title, bool show, Color color)
        {
            UnityEngine.GUI.backgroundColor = color;
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.BeginVertical("Box");
            UnityEngine.GUI.backgroundColor = Color.white;

            string showLabel = "| Show";
            if (show) showLabel = "| Hide";

            Rect r = EditorGUILayout.GetControlRect();
            EditorGUI.LabelField(r, title, EditorStyles.boldLabel);
            r.x = r.width - 40; r.width = 50;
            EditorGUI.LabelField(r, showLabel);
            r.x += 56; r.width = 15;
            show = EditorGUI.Foldout(r, show, "");

            EditorGUI.DrawRect(EditorGUILayout.GetControlRect(GUILayout.MaxHeight(1)), Color.black);
            EditorGUI.DrawRect(EditorGUILayout.GetControlRect(GUILayout.MaxHeight(1)), Color.black);
            GUILayout.Space(1);
            return show;
        } //========================================================================================

        public static void EndSection()
        {
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndVertical();
        } //========================================================================================

        public static void MinMaxSlider(MaterialProperty p1, MaterialProperty p2, GUIContent gc)
        {
            EditorGUI.BeginChangeCheck();
            float min = p1.floatValue, max = p2.floatValue;
            EditorGUILayout.MinMaxSlider(gc, ref min, ref max, 0, 1);
            if (EditorGUI.EndChangeCheck())
            {
                p1.floatValue = min;
                p2.floatValue = max;
            }
        } //========================================================================================

        public static void MinMaxSlider(SerializedProperty p1, SerializedProperty p2, GUIContent gc)
        {
            EditorGUI.BeginChangeCheck();
            float min = p1.floatValue, max = p2.floatValue;
            EditorGUILayout.MinMaxSlider(gc, ref min, ref max, 0, 1);
            if (EditorGUI.EndChangeCheck())
            {
                p1.floatValue = min;
                p2.floatValue = max;
            }
        } //========================================================================================

        public static void Slider(MaterialProperty p, GUIContent gc, float min, float max)
        {
            EditorGUI.BeginChangeCheck();
            float val = EditorGUILayout.Slider(gc, p.floatValue, min, max);
            if (EditorGUI.EndChangeCheck()) p.floatValue = val;
        } //========================================================================================

        public static void FloatField(MaterialProperty p, GUIContent gc)
        {
            EditorGUI.BeginChangeCheck();
            float val = EditorGUILayout.FloatField(gc, p.floatValue);
            if (EditorGUI.EndChangeCheck()) p.floatValue = val;
        } //========================================================================================

        public static void DoubleFloatField(MaterialEditor mEditor, MaterialProperty p1, MaterialProperty p2, GUIContent gc)
        {
            Rect r = EditorGUILayout.GetControlRect();
            EditorGUI.LabelField(r, gc);

            r.x = EditorGUIUtility.labelWidth + 22 + 5;
            r.width = (r.width - EditorGUIUtility.labelWidth - 3) / 2;
            mEditor.FloatProperty(r, p1, "");
            r.x += r.width + 2;
            mEditor.FloatProperty(r, p2, "");
        } //========================================================================================

        public static void DoubleField(SerializedProperty p1, SerializedProperty p2, GUIContent label)
        {
            Rect r = EditorGUILayout.GetControlRect();
            EditorGUI.LabelField(r, label);
            r.x = EditorGUIUtility.labelWidth + 22 + 5;
            r.width = (r.width - EditorGUIUtility.labelWidth - 3) / 2;
            EditorGUI.PropertyField(r, p1, GUIContent.none);
            r.x += r.width + 2;
            EditorGUI.PropertyField(r, p2, GUIContent.none);
        } //========================================================================================

        public static void ColorField(MaterialProperty p, GUIContent gc)
        {
            EditorGUI.BeginChangeCheck();
            Color c = EditorGUILayout.ColorField(gc, p.colorValue, true, false, false);
            if (EditorGUI.EndChangeCheck()) p.colorValue = c;
        } //========================================================================================

        public static void DoubleColorField(MaterialProperty p1, MaterialProperty p2, GUIContent gc)
        {
            Rect r = EditorGUILayout.GetControlRect();
            EditorGUI.LabelField(r, gc);

            r.x = EditorGUIUtility.labelWidth + 22 + 5;
            r.width = (r.width - EditorGUIUtility.labelWidth - 3) / 2;

            EditorGUI.BeginChangeCheck();
            Color c1 = EditorGUI.ColorField(r, GUIContent.none, p1.colorValue, true, false, false);
            if (EditorGUI.EndChangeCheck()) p1.colorValue = c1;

            r.x += r.width + 2;

            EditorGUI.BeginChangeCheck();
            Color c2 = EditorGUI.ColorField(r, GUIContent.none, p2.colorValue, true, false, false);
            if (EditorGUI.EndChangeCheck()) p2.colorValue = c2;
        } //========================================================================================

        public static void TextureField(ref Rect r, MaterialProperty p)
        {
            EditorGUI.BeginChangeCheck();
            Texture t = EditorGUI.ObjectField(r, "", p.textureValue, typeof(Texture), false) as Texture;
            if (EditorGUI.EndChangeCheck()) p.textureValue = t;
            r.x += r.width + 3.5f;
        } //========================================================================================
    }
}

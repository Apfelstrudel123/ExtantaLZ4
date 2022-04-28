#if UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;

namespace MMP
{
    [SelectionBase]
    public class BakeColormap : MonoBehaviour
    {

        // Baking Options
        public string textureName = "Colormap";
        public string defaultDirectory;
        public int resolution = 2048;
        public bool includeSnow;

        // Object Variables
        public List<Material> materials = new List<Material>();
        public bool isPreset;
        public Material sourceMat;

        // Shader Properties
        [ColorUsage(false)] public Color _RockColorBright;
        [ColorUsage(false)] public Color _RockColorDark;
        [ColorUsage(false)] public Color _DirtColorBright;
        [ColorUsage(false)] public Color _DirtColorDark;
        [ColorUsage(false)] public Color _GrassColorBright;
        [ColorUsage(false)] public Color _GrassColorDark;
        [ColorUsage(false)] public Color _FlowColor;
        [ColorUsage(false)] public Color _SnowColor;
        public float _RockScale;
        public float _DirtScale;
        public float _GrassScale;
        public float _SedimentScale;
        [Range(0, 1)] public float _OcclusionToAlbedo;
        [Range(0, 1)] public float _OcclusionIntensity;
        [Range(0, 1)] public float _SedimentIntensity;
        public float _SedimentDistortion;
        [Range(0, 1)] public float _DirtSlopeMin;
        [Range(0, 1)] public float _DirtSlopeMax;
        [Range(0, 1)] public float _GrassSlopeMin;
        [Range(0, 1)] public float _GrassSlopeMax;
        public float _GrassHeightMin;
        public float _GrassHeightMax;
        public float _GrassHeightMod;
        [Range(0, 1)] public float _FlowIntensity;
        [Range(0, 1)] public float _SnowSlopeMin;
        [Range(0, 1)] public float _SnowSlopeMax;
        public float _SnowHeightMin;
        public float _SnowHeightMax;
        public float _SnowHeightMod;
        //==================================================================================================================================================================================
        //==================================================================================================================================================================================
        //==================================================================================================================================================================================


        void OnEnable()
        {
            GetMaterials();
            //UpdateMaterials();
        } // =====================================================================================================================


        public void GetMaterials()
        {
            materials.Clear();

            MeshRenderer[] meshRenderers = transform.GetComponentsInChildren<MeshRenderer>();
            for (int i = 0; i < meshRenderers.Length; i++)
            {
                Material[] mats = meshRenderers[i].sharedMaterials;
                for (int j = 0; j < mats.Length; j++)
                {
                    if (!materials.Contains(mats[j])) materials.Add(mats[j]);
                }
            }
        } // =====================================================================================================================


        public void UpdateMaterials()
        {
            foreach (Material m in materials)
            {
                if (m != null)
                {
                    if (m.HasProperty("_RockColorBright")) m.SetColor("_RockColorBright", _RockColorBright);
                    if (m.HasProperty("_RockColorDark")) m.SetColor("_RockColorDark", _RockColorDark);
                    if (m.HasProperty("_DirtColorBright")) m.SetColor("_DirtColorBright", _DirtColorBright);
                    if (m.HasProperty("_DirtColorDark")) m.SetColor("_DirtColorDark", _DirtColorDark);
                    if (m.HasProperty("_GrassColorBright")) m.SetColor("_GrassColorBright", _GrassColorBright);
                    if (m.HasProperty("_GrassColorDark")) m.SetColor("_GrassColorDark", _GrassColorDark);
                    if (m.HasProperty("_FlowColor")) m.SetColor("_FlowColor", _FlowColor);
                    if (m.HasProperty("_SnowColor")) m.SetColor("_SnowColor", _SnowColor);

                    if (m.HasProperty("_RockScale")) m.SetFloat("_RockScale", _RockScale);
                    if (m.HasProperty("_DirtScale")) m.SetFloat("_DirtScale", _DirtScale);
                    if (m.HasProperty("_GrassScale")) m.SetFloat("_GrassScale", _GrassScale);
                    if (m.HasProperty("_SedimentScale")) m.SetFloat("_SedimentScale", _SedimentScale);

                    if (m.HasProperty("_OcclusionToAlbedo")) m.SetFloat("_OcclusionToAlbedo", _OcclusionToAlbedo);
                    if (m.HasProperty("_OcclusionIntensity")) m.SetFloat("_OcclusionIntensity", _OcclusionIntensity);
                    if (m.HasProperty("_SedimentIntensity")) m.SetFloat("_SedimentIntensity", _SedimentIntensity);
                    if (m.HasProperty("_SedimentDistortion")) m.SetFloat("_SedimentDistortion", _SedimentDistortion);
                    if (m.HasProperty("_DirtSlopeMin")) m.SetFloat("_DirtSlopeMin", _DirtSlopeMin);
                    if (m.HasProperty("_DirtSlopeMax")) m.SetFloat("_DirtSlopeMax", _DirtSlopeMax);
                    if (m.HasProperty("_GrassSlopeMin")) m.SetFloat("_GrassSlopeMin", _GrassSlopeMin);
                    if (m.HasProperty("_GrassSlopeMax")) m.SetFloat("_GrassSlopeMax", _GrassSlopeMax);
                    if (m.HasProperty("_GrassHeightMin")) m.SetFloat("_GrassHeightMin", _GrassHeightMin);
                    if (m.HasProperty("_GrassHeightMax")) m.SetFloat("_GrassHeightMax", _GrassHeightMax);
                    if (m.HasProperty("_GrassHeightMod")) m.SetFloat("_GrassHeightMod", _GrassHeightMod);
                    if (m.HasProperty("_FlowIntensity")) m.SetFloat("_FlowIntensity", _FlowIntensity);
                    if (m.HasProperty("_SnowSlopeMin")) m.SetFloat("_SnowSlopeMin", _SnowSlopeMin);
                    if (m.HasProperty("_SnowSlopeMax")) m.SetFloat("_SnowSlopeMax", _SnowSlopeMax);
                    if (m.HasProperty("_SnowHeightMin")) m.SetFloat("_SnowHeightMin", _SnowHeightMin);
                    if (m.HasProperty("_SnowHeightMax")) m.SetFloat("_SnowHeightMax", _SnowHeightMax);
                    if (m.HasProperty("_SnowHeightMod")) m.SetFloat("_SnowHeightMod", _SnowHeightMod);
                }
            }
        } // =====================================================================================================================

        public void GetValuesFromFirstMaterial()
        {
            if (isPreset) return;
            if (materials.Count == 0) GetMaterials();
            Material m = materials[0];

            if (m.HasProperty("_RockColorBright")) _RockColorBright = m.GetColor("_RockColorBright");
            if (m.HasProperty("_RockColorDark")) _RockColorDark = m.GetColor("_RockColorDark");
            if (m.HasProperty("_DirtColorBright")) _DirtColorBright = m.GetColor("_DirtColorBright");
            if (m.HasProperty("_DirtColorDark")) _DirtColorDark = m.GetColor("_DirtColorDark");
            if (m.HasProperty("_GrassColorBright")) _GrassColorBright = m.GetColor("_GrassColorBright");
            if (m.HasProperty("_GrassColorDark")) _GrassColorDark = m.GetColor("_GrassColorDark");
            if (m.HasProperty("_FlowColor")) _FlowColor = m.GetColor("_FlowColor");
            if (m.HasProperty("_SnowColor")) _SnowColor = m.GetColor("_SnowColor");

            if (m.HasProperty("_RockScale")) _RockScale = m.GetFloat("_RockScale");
            if (m.HasProperty("_DirtScale")) _DirtScale = m.GetFloat("_DirtScale");
            if (m.HasProperty("_GrassScale")) _GrassScale = m.GetFloat("_GrassScale");
            if (m.HasProperty("_SedimentScale")) _SedimentScale = m.GetFloat("_SedimentScale");

            if (m.HasProperty("_OcclusionToAlbedo")) _OcclusionToAlbedo = m.GetFloat("_OcclusionToAlbedo");
            if (m.HasProperty("_OcclusionIntensity")) _OcclusionIntensity = m.GetFloat("_OcclusionIntensity");
            if (m.HasProperty("_SedimentIntensity")) _SedimentIntensity = m.GetFloat("_SedimentIntensity");
            if (m.HasProperty("_SedimentDistortion")) _SedimentDistortion = m.GetFloat("_SedimentDistortion");
            if (m.HasProperty("_DirtSlopeMin")) _DirtSlopeMin = m.GetFloat("_DirtSlopeMin");
            if (m.HasProperty("_DirtSlopeMax")) _DirtSlopeMax = m.GetFloat("_DirtSlopeMax");
            if (m.HasProperty("_GrassSlopeMin")) _GrassSlopeMin = m.GetFloat("_GrassSlopeMin");
            if (m.HasProperty("_GrassSlopeMax")) _GrassSlopeMax = m.GetFloat("_GrassSlopeMax");
            if (m.HasProperty("_GrassHeightMin")) _GrassHeightMin = m.GetFloat("_GrassHeightMin");
            if (m.HasProperty("_GrassHeightMax")) _GrassHeightMax = m.GetFloat("_GrassHeightMax");
            if (m.HasProperty("_GrassHeightMod")) _GrassHeightMod = m.GetFloat("_GrassHeightMod");
            if (m.HasProperty("_FlowIntensity")) _FlowIntensity = m.GetFloat("_FlowIntensity");
            if (m.HasProperty("_SnowSlopeMin")) _SnowSlopeMin = m.GetFloat("_SnowSlopeMin");
            if (m.HasProperty("_SnowSlopeMax")) _SnowSlopeMax = m.GetFloat("_SnowSlopeMax");
            if (m.HasProperty("_SnowHeightMin")) _SnowHeightMin = m.GetFloat("_SnowHeightMin");
            if (m.HasProperty("_SnowHeightMax")) _SnowHeightMax = m.GetFloat("_SnowHeightMax");
            if (m.HasProperty("_SnowHeightMod")) _SnowHeightMod = m.GetFloat("_SnowHeightMod");
        }

        public void CopyValuesFromMaterial()
        {
            Material m = sourceMat;
            if (m == null) return;

            if (m.HasProperty("_RockColorBright")) _RockColorBright = m.GetColor("_RockColorBright");
            if (m.HasProperty("_RockColorDark")) _RockColorDark = m.GetColor("_RockColorDark");
            if (m.HasProperty("_DirtColorBright")) _DirtColorBright = m.GetColor("_DirtColorBright");
            if (m.HasProperty("_DirtColorDark")) _DirtColorDark = m.GetColor("_DirtColorDark");
            if (m.HasProperty("_GrassColorBright")) _GrassColorBright = m.GetColor("_GrassColorBright");
            if (m.HasProperty("_GrassColorDark")) _GrassColorDark = m.GetColor("_GrassColorDark");
            if (m.HasProperty("_FlowColor")) _FlowColor = m.GetColor("_FlowColor");
            if (m.HasProperty("_SnowColor")) _SnowColor = m.GetColor("_SnowColor");

            if (m.HasProperty("_RockScale")) _RockScale = m.GetFloat("_RockScale");
            if (m.HasProperty("_DirtScale")) _DirtScale = m.GetFloat("_DirtScale");
            if (m.HasProperty("_GrassScale")) _GrassScale = m.GetFloat("_GrassScale");
            if (m.HasProperty("_SedimentScale")) _SedimentScale = m.GetFloat("_SedimentScale");

            if (m.HasProperty("_OcclusionToAlbedo")) _OcclusionToAlbedo = m.GetFloat("_OcclusionToAlbedo");
            if (m.HasProperty("_OcclusionIntensity")) _OcclusionIntensity = m.GetFloat("_OcclusionIntensity");
            if (m.HasProperty("_SedimentIntensity")) _SedimentIntensity = m.GetFloat("_SedimentIntensity");
            if (m.HasProperty("_SedimentDistortion")) _SedimentDistortion = m.GetFloat("_SedimentDistortion");
            if (m.HasProperty("_DirtSlopeMin")) _DirtSlopeMin = m.GetFloat("_DirtSlopeMin");
            if (m.HasProperty("_DirtSlopeMax")) _DirtSlopeMax = m.GetFloat("_DirtSlopeMax");
            if (m.HasProperty("_GrassSlopeMin")) _GrassSlopeMin = m.GetFloat("_GrassSlopeMin");
            if (m.HasProperty("_GrassSlopeMax")) _GrassSlopeMax = m.GetFloat("_GrassSlopeMax");
            if (m.HasProperty("_GrassHeightMin")) _GrassHeightMin = m.GetFloat("_GrassHeightMin");
            if (m.HasProperty("_GrassHeightMax")) _GrassHeightMax = m.GetFloat("_GrassHeightMax");
            if (m.HasProperty("_GrassHeightMod")) _GrassHeightMod = m.GetFloat("_GrassHeightMod");
            if (m.HasProperty("_FlowIntensity")) _FlowIntensity = m.GetFloat("_FlowIntensity");
            if (m.HasProperty("_SnowSlopeMin")) _SnowSlopeMin = m.GetFloat("_SnowSlopeMin");
            if (m.HasProperty("_SnowSlopeMax")) _SnowSlopeMax = m.GetFloat("_SnowSlopeMax");
            if (m.HasProperty("_SnowHeightMin")) _SnowHeightMin = m.GetFloat("_SnowHeightMin");
            if (m.HasProperty("_SnowHeightMax")) _SnowHeightMax = m.GetFloat("_SnowHeightMax");
            if (m.HasProperty("_SnowHeightMod")) _SnowHeightMod = m.GetFloat("_SnowHeightMod");
        }


        public void SavePreset(string path)
        {
            if (path.Length == 0) return;
            path = path.Replace(Application.dataPath + "/", "Assets/");
            GameObject tempParent = new GameObject();
            int childCount = this.transform.childCount;
            for (int i = 0; i < childCount; ++i) this.transform.GetChild(0).parent = tempParent.transform;
            isPreset = true;
            GameObject save = Instantiate(this.gameObject) as GameObject;
            UnityEditor.AssetDatabase.DeleteAsset(path);
            //UnityEngine.Object prefab = UnityEditor.PrefabUtility.CreateEmptyPrefab(path);
            //UnityEditor.PrefabUtility.ReplacePrefab(save, prefab, UnityEditor.ReplacePrefabOptions.ReplaceNameBased);
            UnityEditor.PrefabUtility.SaveAsPrefabAsset(save, path);
            DestroyImmediate(save);
            for (int i = 0; i < childCount; ++i) tempParent.transform.GetChild(0).parent = this.transform;
            DestroyImmediate(tempParent);
            isPreset = false;
        }// =====================================================================================================================


        public void LoadPreset(string path)
        {
            if (path.Length == 0) return;
            path = path.Replace(Application.dataPath + "/", "Assets/");
            int index = this.transform.GetSiblingIndex();
            GameObject newMMP = (GameObject)Instantiate(UnityEditor.AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)));
            newMMP.name = this.gameObject.name;
            int childCount = this.transform.childCount;
            for (int i = 0; i < childCount; ++i) this.transform.GetChild(0).parent = newMMP.transform;
            DestroyImmediate(this.gameObject);
            newMMP.transform.SetSiblingIndex(index);
            newMMP.transform.hideFlags = HideFlags.HideInInspector;
            UnityEditor.Selection.activeGameObject = newMMP;
            newMMP.GetComponent<BakeColormap>().isPreset = false;
        }// =====================================================================================================================


        public void Bake()
        {
            string path = UnityEditor.EditorUtility.SaveFilePanel("Save texture as PNG", defaultDirectory, textureName + ".png", "png");

            if (path.Length != 0)
            {
                UnityEditor.EditorUtility.DisplayProgressBar("Please Wait", "Rendering Mountains...", 0.0f);

                GameObject camGO = new GameObject("Capture Camera", typeof(Camera));
                camGO.hideFlags = HideFlags.HideAndDontSave;
                Transform camT = camGO.transform;
                camT.parent = transform;
                camT.localPosition = new Vector3(0, 2500, 0);
                camT.localEulerAngles = new Vector3(90, 180, 0);
                Camera cam = camGO.GetComponent<Camera>();
                cam.aspect = 1;
                cam.nearClipPlane = 0;
                cam.farClipPlane = 5000;
                cam.orthographicSize = 800;
                cam.orthographic = true;
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = Color.black;
                cam.useOcclusionCulling = false;
                cam.allowHDR = false;
                cam.allowMSAA = false;
                cam.cullingMask = -1;

                Material mat = transform.GetChild(0).GetComponent<MeshRenderer>().sharedMaterial;
                Shader originalShader = mat.shader;
                mat.shader = Shader.Find("Hidden/MMP/Bake Colormap");
                if (includeSnow) mat.SetFloat("_IncludeSnow", 1);
                else mat.SetFloat("_IncludeSnow", 0);

                RenderTexture rt = new RenderTexture(resolution, resolution, 24);
                rt.name = "renderTextureColormap";
                rt.Create();
                cam.targetTexture = rt;
                cam.Render();

                RenderTexture.active = rt;

                Texture2D tex = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);
                tex.ReadPixels(new Rect(0, 0, resolution, resolution), 0, 0);
                tex.Apply();

                RenderTexture.active = null;
                cam.targetTexture = null;
                rt.Release();
                DestroyImmediate(rt);
                mat.shader = originalShader;
                DestroyImmediate(camGO);


                Texture2D texAO = mat.GetTexture("_SplatMap") as Texture2D;
                int count = 0;
                int total = tex.height;
                for (int y = 0; y < tex.height; y++)
                {
                    for (int x = 0; x < tex.width; x++)
                    {
                        Color c = tex.GetPixel(x, y);
                        float occlusion = texAO.GetPixelBilinear(x / (float)tex.width, y / (float)tex.height).b;
                        c.a = occlusion;
                        tex.SetPixel(x, y, c);
                    }
                    UnityEditor.EditorUtility.DisplayProgressBar("Please Wait", "Writing Pixels...", (count / (float)total) * 0.95f + 0.025f);
                    count++;
                }
                UnityEditor.EditorUtility.DisplayProgressBar("Please Wait", "Saving Texture...", 0.975f);

                byte[] bytes;
                bytes = tex.EncodeToPNG();
                if (bytes != null)
                {
                    System.IO.File.WriteAllBytes(path, bytes);
                    //if (System.IO.File.Exists(path)) Debug.Log("Succesfully saved file:" + path);
                    UnityEditor.AssetDatabase.Refresh();
                }

                DestroyImmediate(tex);
            }
            UnityEditor.EditorUtility.ClearProgressBar();
        } // =====================================================================================================================
    }
}
#endif

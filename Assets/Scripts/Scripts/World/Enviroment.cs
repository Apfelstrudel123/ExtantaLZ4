using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.VFX;
using TMPro;
using JBooth.MicroSplat;
using Core.GameSettings;

namespace GameWorld
{
    public class Enviroment : MonoBehaviour
    {
        public static Enviroment instance;

        #region Editor

        [SerializeField] private bool updateTimeOnValidate = true;

        private void OnValidate()
        {
            sunLight.GetComponent<HDAdditionalLightData>().shadowUpdateMode = ShadowUpdateMode.EveryFrame;
            if (updateTimeOnValidate)
            {
                sunLastPos = 0;
                moonLastPos = 0;

                sunLight.transform.rotation = Quaternion.Euler(0, sunAngleY, 0);
                moonLight.transform.rotation = Quaternion.Euler(0, moonAngleY, 0);

                sunLight.transform.Rotate(transform.right, sunPos.Evaluate(curTime));
                sunLight.intensity = sunIntensity.Evaluate(curTime);
                sunLight.GetComponent<HDAdditionalLightData>().intensity = sunIntensity.Evaluate(curTime);

                moonLight.transform.Rotate(transform.right, moonPos.Evaluate(curTime));
                moonLight.intensity = moonIntensity.Evaluate(curTime);
                moonLight.GetComponent<HDAdditionalLightData>().intensity = moonIntensity.Evaluate(curTime);

                if (volume.sharedProfile.TryGet(out sky))
                {
                    sky.spaceEmissionMultiplier.value = starsVisibilty.Evaluate(curTime) * maxStarsVisibilty;
                }
                if (volume.sharedProfile.TryGet(out exp))
                {
                    exp.limitMax.value = exposureMax.Evaluate(curTime);
                    exp.limitMin.value = exposureMin.Evaluate(curTime);
                }
                if (volume.sharedProfile.TryGet(out ambLight))
                {
                    ambLight.indirectDiffuseLightingMultiplier.value = ambientLight.Evaluate(curTime);
                }

                if (curTime > 6 && curTime < 21.4)
                {
                    sunLight.shadows = LightShadows.Soft;
                    moonLight.shadows = LightShadows.None;
                }
                else if (curTime > 21.4 || curTime < 6)
                {
                    sunLight.shadows = LightShadows.None;
                    moonLight.shadows = LightShadows.Soft;
                }

                sunLight.GetComponent<HDAdditionalLightData>().RequestShadowMapRendering();
                moonLight.GetComponent<HDAdditionalLightData>().RequestShadowMapRendering();
            }
        }

        #endregion

        #region General
        private void Awake()
        {
            instance = this;

            GetVolumes();

            sunData = sunLight.GetComponent<HDAdditionalLightData>();
            moonData = moonLight.GetComponent<HDAdditionalLightData>();
            sunData.shadowUpdateMode = ShadowUpdateMode.OnDemand;

            Color grey = new (1f, 1f, 1f);
            defaultTerrainTint = new Texture2D(128, 128);
            for (int x = 0; x < 128; x++)
            {
                for (int z = 0; z < 128; z++)
                {
                    defaultTerrainTint.SetPixel(x, z, grey);
                }
            }

            Init();
        }

        public void Init()
        {
            lastTime = curTime;
            time = curTime;

            InitSky();
            InitWheater();
        }

        private void Update()
        {
            if (Core.Game.GameState != GameState.Active)
            { return; }

            time += speed * Time.deltaTime;

            if (time >= 24f)
            {
                time = 0f;
                Debug.Log("NewDay");
            }

            display.text = "TIME:" + System.Math.Round(time, 2);

            if (Core.Game.PlayerStatus == PlayerStatus.Dead)
            {
                if (color.saturation.value > -100f)
                {
                    color.saturation.SetValue(new FloatParameter(color.saturation.value - Time.deltaTime * 50));
                }
                if (color.postExposure.value > -5f)
                {
                    color.postExposure.SetValue(new FloatParameter(color.postExposure.value - Time.deltaTime));
                }
            }

            updateTimer++;
            if (updateTimer >= updateFrequency)
            {
                updateTimer = 0;
                UpdateLighting();
            }
            else if (Settings.Get<int>(Settings.visuals.shadows) > 0)
            {
                if (time > 6 && time < 21.4) { sunData.RequestShadowMapRendering(); }
                else { moonData.RequestShadowMapRendering(); }
            }

            UpdateWheater();
        }

        public void LoadingScreenUpdate()
        {
            UpdateLighting();
            if (Settings.visuals.shadows.Get() > 0)
            {
                if (time > 6 && time < 21.4) { sunData.RequestShadowMapRendering(); }
                else { moonData.RequestShadowMapRendering(); }
            }
        }

        #endregion

        #region Time & Day

        private float time;
        private float lastTime;
        private int updateTimer = 0;
        [Header("Time")]
        [SerializeField] [Range(0, 10)] private float speed = 1;
        [SerializeField] [Range(0, 24)] private float curTime = 1;
        [SerializeField] private TMP_Text display = null;

        private void UpdateLighting()
        {
            if (Settings.visuals.shadows.Get() > 0)
            {
                if (time > 6 && lastTime < 6)
                {
                    sunData.EnableShadows(true);
                    moonData.EnableShadows(false);
                }
                else if (time > 21.4 && lastTime < 21.4)
                {
                    sunData.EnableShadows(false);
                    moonData.EnableShadows(true);
                }
            }

            if (time > 23 && lastTime < 23)
            {
                sunLight.enabled = false;
            }
            if (time > 4 && lastTime < 4)
            {
                sunLight.enabled = true;
            }
            if (time > 7 && lastTime < 7)
            {
                moonLight.enabled = false;
            }
            if (time > 18 && lastTime < 18)
            {
                moonLight.enabled = true;
            }

            exp.limitMax.value = exposureMax.Evaluate(time);
            exp.limitMin.value = exposureMin.Evaluate(time);
            sky.spaceEmissionMultiplier.value = starsVisibilty.Evaluate(time) * maxStarsVisibilty;
            ambLight.indirectDiffuseLightingMultiplier.value = ambientLight.Evaluate(time);

            sunLight.transform.Rotate(transform.right, sunPos.Evaluate(time) - sunLastPos);
            sunLight.intensity = sunIntensity.Evaluate(time);
            sunData.intensity = sunIntensity.Evaluate(time);
            sunLastPos = sunPos.Evaluate(time);

            moonLight.transform.Rotate(transform.right, moonPos.Evaluate(time) - moonLastPos);
            moonLight.intensity = moonIntensity.Evaluate(time);
            moonData.intensity = moonIntensity.Evaluate(time);
            moonLastPos = moonPos.Evaluate(time);

            lastTime = time;
        }

        public void SetTime(int t)
        {
            time = t / 100f;
        }

        public void SkipHours(int amt)
        {
            time += amt;
        }

        #endregion

        #region Sky & Sun & Moon

        [Header("Sky")]
        private PhysicallyBasedSky sky = null;
        [SerializeField] private AnimationCurve starsVisibilty = null;
        [SerializeField] private float maxStarsVisibilty = 10f;

        private HDAdditionalLightData sunData;
        private float sunLastPos;
        [Header("Sun")]
        [SerializeField] private Light sunLight = null;
        [SerializeField] private float sunAngleY = 10f;
        [SerializeField] private AnimationCurve sunPos = null;
        [SerializeField] private AnimationCurve sunIntensity = null;
        [SerializeField] private int updateFrequency = 1;

        private HDAdditionalLightData moonData;
        private float moonLastPos;
        [Header("Moon")]
        [SerializeField] private Light moonLight = null;
        [SerializeField] private float moonAngleY = 10f;
        [SerializeField] private AnimationCurve moonPos = null;
        [SerializeField] private AnimationCurve moonIntensity = null;

        private void InitSky()
        {
            if (Settings.visuals.shadows.Get() > 0)
            {
                if (time > 6 && time < 21.4)
                {
                    sunData.EnableShadows(true);
                    moonData.EnableShadows(false);
                }
                else
                {
                    sunData.EnableShadows(false);
                    moonData.EnableShadows(true);
                }
            }
            else
            {
                sunData.EnableShadows(false);
                moonData.EnableShadows(false);
            }

            if (time < 23 && time > 4)
            {
                sunLight.enabled = true;
            }
            else
            {
                sunLight.enabled = false;
            }
            if (time > 18 && time < 7)
            {
                moonLight.enabled = true;
            }
            else
            {
                moonLight.enabled = false;
            }
            sunLastPos = 0;
            sunLight.transform.rotation = Quaternion.Euler(0, sunAngleY, 0);
            sunLight.transform.Rotate(transform.right, sunPos.Evaluate(time));
            sunLight.intensity = sunIntensity.Evaluate(time);
            sunData.intensity = sunIntensity.Evaluate(time);
            sunLastPos = sunPos.Evaluate(time);

            moonLastPos = 0;
            moonLight.transform.rotation = Quaternion.Euler(0, moonAngleY, 0);
            moonLight.transform.Rotate(transform.right, moonPos.Evaluate(time));
            moonLight.intensity = moonIntensity.Evaluate(time);
            moonData.intensity = moonIntensity.Evaluate(time);
            moonLastPos = moonPos.Evaluate(time);

            exp.limitMax.value = exposureMax.Evaluate(time);
            exp.limitMin.value = exposureMin.Evaluate(time);
            sky.spaceEmissionMultiplier.value = starsVisibilty.Evaluate(time) * maxStarsVisibilty;
            ambLight.indirectDiffuseLightingMultiplier.value = ambientLight.Evaluate(time);
        }

        #endregion

        #region Wheater

        public enum Wheater
        {
            Clear,
            Rain,
        }
        private Wheater currentWheater = Wheater.Clear;
        private Wheater lastWheater = Wheater.Clear;
        private int transitionTime;
        private float spentTime;
        private int targetRain;
        [Header("Wheater")]
        [SerializeField] private Material terrainMaterial;
        [SerializeField] private int minTransitionTime = 30;
        [SerializeField] private int maxTransitionTime = 60;
        [SerializeField] private WheaterProgramm clearWheater;
        [SerializeField] private WheaterProgramm rainWheater;
        [SerializeField] private VisualEffect rainVFX;
        [SerializeField] private int minRain;
        [SerializeField] private int maxRain;
        [SerializeField] private AudioSource rainAudio;
        [SerializeField] private THOR.THOR_Thunderstorm lightning;

        private void InitWheater()
        {
            currentWheater = Wheater.Clear;
            transitionTime = 20;
            rainVFX.SetInt("Strength", 0);
            lightning.probability = 0f;
            rainVFX.enabled = false;
            rainAudio.volume = 0;
            rainAudio.Stop();
            terrainMaterial.SetVector("_WetnessParams", new Vector4(0, 1, 0, 0));
        }
        private void UpdateWheater()
        {
            spentTime += Time.deltaTime * speed * 1000f;

            if (spentTime >= transitionTime)
            {
                spentTime = 0;
                if (lastWheater == currentWheater)
                {
                    int c = UnityEngine.Random.Range(0, 2);
                    currentWheater = (Wheater)c;
                    transitionTime = UnityEngine.Random.Range(minTransitionTime, maxTransitionTime);

                    if (currentWheater == Wheater.Rain)
                    {
                        rainVFX.enabled = true;
                        rainAudio.volume = 0f;
                        rainAudio.Play();
                        targetRain = UnityEngine.Random.Range(minRain, maxRain);
                    }
                    else //if (currentWheater == Wheater.Clear)
                    {
                        rainVFX.enabled = false;
                    }
                }
                else
                {
                    WheaterProgramm wp;
                    if (currentWheater == Wheater.Rain)
                    {
                        wp = rainWheater;
                        rainVFX.SetInt("Strength", targetRain);
                        terrainMaterial.SetVector("_WetnessParams", new Vector4(0.6f, 1, 0, 0));
                        lightning.probability = 0.8f;
                        rainAudio.volume = 1f;
                    }
                    else //if (currentWheater == Wheater.Clear)
                    {
                        wp = clearWheater;
                        rainVFX.SetInt("Strength", 0);
                        rainAudio.volume = 0f;
                        terrainMaterial.SetVector("_WetnessParams", new Vector4(0, 1, 0, 0));
                        lightning.probability = 0;
                    }
                    transitionTime = UnityEngine.Random.Range(wp.minTime, wp.maxTime);
                    lastWheater = currentWheater;
                }
            }

            if (lastWheater != currentWheater)
            {
                if (lastWheater == Wheater.Clear && currentWheater == Wheater.Rain)
                {
                    rainVFX.SetInt("Strength", (int)(targetRain * spentTime / transitionTime));
                    terrainMaterial.SetVector("_WetnessParams", new Vector4(0.6f * spentTime / transitionTime, 1f, 0, 0));
                    lightning.probability = 0.8f * spentTime / transitionTime;
                    rainAudio.volume = spentTime / transitionTime;

                    MicroSplatTerrain.SyncAll();
                }
                if (lastWheater == Wheater.Rain && currentWheater == Wheater.Clear)
                {
                    rainVFX.SetInt("Strength", targetRain - (int)(targetRain * spentTime / transitionTime));
                    terrainMaterial.SetVector("_WetnessParams", new Vector4(0.6f - 0.6f * spentTime / transitionTime, 1f, 0, 0));
                    lightning.probability = 0.8f - 0.8f * spentTime / transitionTime;
                    rainAudio.volume = 1 - spentTime / transitionTime;

                    MicroSplatTerrain.SyncAll();
                }
            }

            if (currentWheater == Wheater.Rain)
            {
                rainVFX.transform.position = Gameplay.Player.Transform.position;
            }
        }

        #endregion

        #region Terrain

        public static bool showTrees;
        public static Texture2D defaultTerrainTint;
        [Header("Terrain")]
        [HideInInspector] public Transform terrains;

        public static void ShowTerrains(bool on)
        {
            if (on)
            {
                for (int i = 0; i < instance.terrains.childCount; ++i)
                {
                    instance.terrains.GetChild(i).GetComponent<Terrain>().drawHeightmap = true;
                }
            }
            else
            {
                for (int i = 0; i < instance.terrains.childCount; ++i)
                {
                    instance.terrains.GetChild(i).GetComponent<Terrain>().drawHeightmap = false;
                }
            }
        }
        public static void ShowTrees(bool on)
        {
            if (on)
            {
                showTrees = true;
                for (int i = 0; i < instance.terrains.childCount; ++i)
                {
                    instance.terrains.GetChild(i).GetComponent<Terrain>().drawTreesAndFoliage = true;
                }
            }
            else
            {

                showTrees = false;
                for (int i = 0; i < instance.terrains.childCount; ++i)
                {
                    instance.terrains.GetChild(i).GetComponent<Terrain>().drawTreesAndFoliage = false;
                }
            }
        }

        #endregion

        #region Volumes & Settings

        private Exposure exp = null;
        private IndirectLightingController ambLight = null;
        private ColorAdjustments color;
        private ChromaticAberration chrom;
        private Fog fog;
        private AmbientOcclusion ambientOcclusion;
        [Header("Volumes")]
        [SerializeField] private Volume volume = null;
        [SerializeField] private AnimationCurve chromCurve = null;
        [SerializeField] private AnimationCurve satCurve = null;
        [SerializeField] private AnimationCurve exposureMax = null;
        [SerializeField] private AnimationCurve exposureMin = null;
        [SerializeField] private AnimationCurve ambientLight = null;

        private void GetVolumes()
        {
            if (!volume.profile.TryGet<PhysicallyBasedSky>(out sky))
            {
                Debug.Log("no PhysicallyBasedSky");
            }
            if (!volume.profile.TryGet<Exposure>(out exp))
            {
                Debug.Log("no exposure");
            }
            if (!volume.profile.TryGet<IndirectLightingController>(out ambLight))
            {
                Debug.Log("no IndirectLightingController");
            }
            if (!volume.profile.TryGet<Fog>(out fog))
            {
                Debug.Log("no Fog");
            }
            if (!volume.profile.TryGet<ColorAdjustments>(out color))
            {
                Debug.Log("no ColorAdjustments");
            }
            if (!volume.profile.TryGet<ChromaticAberration>(out chrom))
            {
                Debug.Log("no ChromaticAberration");
            }
            if (!volume.profile.TryGet<AmbientOcclusion>(out ambientOcclusion))
            {
                Debug.Log("no AmbientOcclusion");
            }
        }

        public void UpdateHealth(float health)
        {
            chrom.intensity.value = chromCurve.Evaluate(health);
            color.saturation.value = satCurve.Evaluate(health);
        }

        public void UpdateQuality()
        {
            int l = Settings.visuals.shadows.Get();

            if (l == 0)
            {
                sunData.EnableShadows(false);
                moonData.EnableShadows(false);
            }
            else
            {
                sunData.SetShadowResolutionLevel(l - 1);
                moonData.SetShadowResolutionLevel(l - 1);
                if (time > 6 && time < 21.4)
                {
                    sunData.EnableShadows(true);
                }
                else
                {
                    moonData.EnableShadows(true);
                }
            }

            fog.quality.value = Settings.visuals.fog.Get();
            ambientOcclusion.quality.value = Settings.visuals.ambientOcclusion.Get();
        }

        #endregion
    }

    [System.Serializable]
    public class WheaterProgramm
    {
        public int minTime;
        public int maxTime;
    }
}
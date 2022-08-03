using UnityEngine;

namespace Core 
{
    namespace GameSettings 
    {
        public class Setting {
            public string Name { get; set; }
        }

        public class Setting<T> : Setting {
            public T Value { get; set; }
            private Setting(string name) {
                Name = name;
            }

            public static Setting<T> Create(string name) {
                Setting<T> s = new (name);
                Settings.AddSetting(s);
                return s;
            }

            public static Setting<T> Create(string name, T value) {
                Setting<T> s = new(name);
                s.Value = value;
                Settings.AddSetting(s);
                return s;
            }

            public T Get() {
                return Value;
            }
        }

        public static class Settings {
            public static GameProfile gameplay;
            public static VisualProfile visuals;
            public static ControlProfile controls;
            public static AudioProfile audio;

            public static void Init() {
                InitAudio();
            }

            private readonly static System.Collections.Generic.List<Setting> settings = new ();
            public static void AddSetting(Setting s) {
                settings.Add(s);
            }

            public static Setting GetSetting(string code) {
                return settings.Find(s => s.Name == code);
            }

            public static T Get<T>(Setting<T> s) {
                return s.Value;
            }

            private static T Get<T>(string code) {
                return Get<T>((Setting<T>)settings.Find(s => s.Name == code));
            }

            public static void Set<T>(Setting<T> s, T value) {
                s.Value = value;
            }
            private static void Set<T>(string code, T value) {
                Set<T>((Setting<T>)settings.Find(s => s.Name == code), value);
            }

            public static void InitControls() {
                Gameplay.Input.LoadJSON(controls.rebinds.Get());
            }

            public static void InitVisuals() {
                Screen.SetResolution(Screen.width, Screen.height, (FullScreenMode)visuals.windowMode.Get(), visuals.refreshRate.Get());
                Application.targetFrameRate = visuals.fpsLimit.Get();
                QualitySettings.anisotropicFiltering = (AnisotropicFiltering)visuals.textureFiltering.Get();
                QualitySettings.masterTextureLimit = visuals.textureQuality.Get();
                GameWorld.Enviroment.instance.UpdateQuality();

                Gameplay.Cam.active.UpdateSettings();
            }

            public static void InitAudio() {
                AudioListener.volume = audio.mainVolume.Get() / 100f;
                Audio.mixer.SetFloat("DialogVol", Mathf.Log10(audio.dialogVolume.Get() / 100f) * 20);
                Audio.mixer.SetFloat("IngameMusicVol", Mathf.Log10(audio.ingameMusicVolume.Get() / 100f) * 20);
                Audio.mixer.SetFloat("MenuMusicVol", Mathf.Log10(audio.menuMusicVolume.Get() / 100f) * 20);
                Audio.mixer.SetFloat("MenuSoundsVol", Mathf.Log10(audio.menuSoundsVolume.Get() / 100f) * 20);
            }

            public static void InitFXAudioSettings() {
                Audio.mixer.SetFloat("FXVol", Mathf.Log10(audio.sfxVolume.Get() / 100f) * 20);
            }
        }

        public class GameProfile {
            public Setting<bool> showFPS = Setting<bool>.Create("show_fps", true);
            public Setting<int> language = Setting<int>.Create("language", 0);
            public Setting<bool> loadLastLOD = Setting<bool>.Create("load_last_lod", false);
        }

        public class VisualProfile {
            public Setting<int> fpsLimit = Setting<int>.Create("fps_limit", -1);
            public Setting<int> refreshRate = Setting<int>.Create("refresh_rate", 60);
            public Setting<int> vSync = Setting<int>.Create("v_sync");
            public Setting<int> fov = Setting<int>.Create("fov", 60);
            public Setting<bool> dynamicRes = Setting<bool>.Create("dynamic_resolution", false);

            public Setting<int> windowMode = Setting<int>.Create("window_mode", 0);
            public Setting<int> windowHeight = Setting<int>.Create("window_height");
            public Setting<int> windowWidth = Setting<int>.Create("window_width");

            public Setting<int> textureQuality = Setting<int>.Create("textures", 0);
            public Setting<int> textureFiltering = Setting<int>.Create("texture_filtering", 1);
            public Setting<int> lod = Setting<int>.Create("lod", 10);
            public Setting<int> fog = Setting<int>.Create("fog", 1);
            public Setting<int> shadows = Setting<int>.Create("shadows", 2);
            public Setting<int> reflections = Setting<int>.Create("reflections", 2);

            public Setting<int> aaMethod = Setting<int>.Create("aa_method", 0);
            public Setting<int> aaQuality = Setting<int>.Create("aa_quality", 1);
            public Setting<float> taaSharpen = Setting<float>.Create("taa_sharpen", 1f);
            public Setting<int> ambientOcclusion = Setting<int>.Create("ambient_occlusion", 1);
            public Setting<int> postProcess = Setting<int>.Create("post_processing", 2);

            public static bool operator ==(VisualProfile lhs, VisualProfile rhs) {
                return lhs.Equals(rhs);
            }
            public static bool operator !=(VisualProfile lhs, VisualProfile rhs) {
                return !lhs.Equals(rhs);
            }
        }

        public class ControlProfile {
            public Setting<string> rebinds = Setting<string>.Create("rebinds");
            public Setting<float> verticalSensitivity = Setting<float>.Create("sensitivity_y", 1);
            public Setting<float> horizontalSensitivity = Setting<float>.Create("sensitivity_x", 1);
        }

        public class AudioProfile {
            public Setting<int> mainVolume = Setting<int>.Create("main_volume", 50);
            public Setting<int> sfxVolume = Setting<int>.Create("sfx_volume", 50);
            public Setting<int> dialogVolume = Setting<int>.Create("dialog_volume", 10);
            public Setting<int> ingameMusicVolume = Setting<int>.Create("game_music_volume", 10);
            public Setting<int> menuMusicVolume = Setting<int>.Create("menu_music_volume", 10);
            public Setting<int> menuSoundsVolume = Setting<int>.Create("menu_sounds_volume", 10);
        }
    }
}

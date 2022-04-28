using System.IO;
using Newtonsoft.Json;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Core.GameSettings;
namespace Core
{
    public static class IO
    {
        public static readonly string PATH = UnityEngine.Application.persistentDataPath + "/saves/";
        /*public static void CheckIfSavesExist(out bool[] saveExists)
        {
            saveExists = new bool[5];
            for (int i = 0; i < 5; ++i)
            {
                saveExists[i] = File.Exists(path + "progress" + i + ".save");
            }
        }*/

        public static void LoadData(out ApplicationData appData, out Save[] saves)
        {
            if(!Directory.Exists(PATH))
            {
                Directory.CreateDirectory(PATH);
            }
            appData = LoadAppData();
            saves = LoadSaves();
            LoadSettings();

            if (appData.firstStart)
            {
                appData.firstStart = false;
                for (int i = 0; i < saves.Length; ++i)
                {
                    saves[i].empty = true;
                }

                SaveAppData();
                SaveSettings();
            }
        }

        public static void LoadGame(int index)
        {
            Game.PlayerData = ReadFile<PlayerData>(PATH + "game_" + index + ".json", out bool success);
            if (!success)
            {
                Game.PlayerData = DefaultFile<PlayerData>("default_player_data");
            }
        }

        /// <summary>
        /// Read a file into an object
        /// </summary>
        /// <param name="key">Addressable Key</param>
        /// <returns>Default Object of Type T</returns>
        public static T DefaultFile<T>(string key)
        {
            AsyncOperationHandle<UnityEngine.TextAsset> handle = Addressables.LoadAssetAsync<UnityEngine.TextAsset>(key);
            handle.WaitForCompletion();
            return JsonConvert.DeserializeObject<T>(handle.Result.ToString());
        }
        /// <summary>
        /// Read a file into an object
        /// </summary>
        /// <param name="filename">Name of the file</param>
        /// <returns>Object of type T</returns>
        public static T ReadFile<T>(string filename, out bool success)
        {
            try
            {
                string json = File.ReadAllText(PATH + filename);
                success = true;
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch
            {
                success = false;
                return default;
            }
        }
        public static ApplicationData LoadAppData()
        {
            ApplicationData appData;
            if (File.Exists(PATH + "appData.json"))
            {
                appData = ReadFile<ApplicationData>(PATH + "appData.json", out bool success);
                if (!success)
                {
                    appData.firstStart = true;
                    appData.lastPlayed = -1;
                }
            }
            else
            {
                appData.firstStart = true;
                appData.lastPlayed = -1;
            }

            return appData;
        }
        public static Save[] LoadSaves()
        {
            Save[] saves = new Save[5];
            for (int i = 0; i < saves.Length; ++i)
            {
                saves[i] = ReadFile<Save>(PATH + "save_" + i + ".json", out bool success);
                if (!success)
                {
                    saves[i].empty = true;
                }
            }
            return saves;
        }
        public static void LoadSettings()
        {
            bool success = true;
            Settings.gameplay = new GameProfile();
            //Settings.gameplay = ReadFile<GameProfile>(PATH + "gameplay.json", out bool success);
            if (!success)
            {
                Settings.gameplay = new GameProfile();
                //Settings.gameplay = DefaultFile<GameProfile>("default_gameplay");
            }
            Settings.visuals = new VisualProfile();
            //Settings.visuals = ReadFile<VisualProfile>(PATH + "visuals.json", out success);
            if (!success)
            {
                Settings.visuals = DefaultFile<VisualProfile>("default_visuals");
                
            }
            Settings.controls = new ControlProfile();
            //Settings.controls = ReadFile<ControlProfile>(PATH + "controls.json", out success);
            if (!success)
            {
                Settings.controls = DefaultFile<ControlProfile>("default_controls");
            }
            Settings.audio = new AudioProfile();
            //Settings.audio = ReadFile<AudioProfile>(PATH + "audio.json", out success);
            if (!success)
            {
                Settings.audio = DefaultFile<AudioProfile>("default_audio");
            }
        }

        /// <summary>
        /// Save an object into a file (Settings, Game state)
        /// </summary>
        /// <param name="data">Object to save</param>
        /// <param name="filename">Name of the file</param>
        public static async void SaveFile(object data, string filename)
        {
            string json = JsonConvert.SerializeObject(data);
            await File.WriteAllTextAsync(PATH + filename, json);
        }
        public static void SaveAppData()
        {
            SaveFile(Game.AppData, "appData.json");
            for (int i = 0; i < Game.Saves.Length; ++i)
            {
                SaveFile(Game.Saves[i], "save_" + i + ".json");
            }
        }
        public static void SaveSettings()
        {
            SaveFile(Settings.gameplay, "gameplay.json");
            SaveFile(Settings.visuals, "visuals.json");
            SaveFile(Settings.controls, "controls.json");
            SaveFile(Settings.audio, "audio.json");
        }
    }
}

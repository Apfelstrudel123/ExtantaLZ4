using Core.GameSettings;
namespace Core
{
    public static class Game
    {
        private static GameState gameState = GameState.MainMenu;
        public static GameState GameState
        {
            get { return gameState; }
            set { gameState = value; }
        }
        private static PlayerStatus playerStatus = PlayerStatus.Alive;
        public static PlayerStatus PlayerStatus
        {
            get { return playerStatus; }
            set { playerStatus = value; }
        }
        private static int saveIndex = 5;
        public static int SaveIndex
        {
            get { return saveIndex; }
            set { saveIndex = value; }
        }
        private static ApplicationData appData;
        public static ApplicationData AppData
        {
            get { return appData; }
        }
        public static Save[] Saves = new Save[5];

        public static PlayerData PlayerData;

        public static void Init()
        {
            IO.LoadData(out appData, out Saves);

            if (!string.IsNullOrEmpty(Settings.controls.rebinds.Get()))
            {
                Gameplay.Input.LoadJSON(Settings.controls.rebinds.Get());
            }

            Settings.Init();
        }

        public static ScriptableObjects.DifficultyProfile GetDifficulty()
        {
            return null;
        }

        public static Save GetActiveSave()
        {
            return Saves[SaveIndex];
        }

        public static void SetDifficulty(Difficulty value)
        {
            Saves[SaveIndex].difficulty = value;
        }

        public static void ResetSaveToDefault(int index)
        {
            Saves[index].lastPlay = string.Empty;
            Saves[index].lastMission = 0;
            Saves[index].percentage = 0;
            Saves[index].empty = true;
            IO.SaveAppData();
        }
        public static void SetLastPlayed(int index)
        {
            appData.lastPlayed = index;
        }
        public static void SaveGame()
        {
            //if (instance.testType != TestType.None) { return; }

            if (GameState != GameState.MainMenu)
            {
                PlayerData.lastLocation = "Village_1";
                ++Saves[SaveIndex].percentage;
            }
            Saves[SaveIndex].lastPlay = System.DateTime.Now.ToString();

            IO.SaveFile(PlayerData, "game_" + SaveIndex + ".json");
            IO.SaveAppData();
        }
    }
}
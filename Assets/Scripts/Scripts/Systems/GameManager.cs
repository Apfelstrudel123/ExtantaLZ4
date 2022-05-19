using UnityEngine;
using UnityEngine.InputSystem;
using ScriptableObjects;
using UnityEngine.Localization.Settings;
using GameWorld;
using UI;
using Core.GameSettings;
namespace Core
{
    public class GameManager : MonoBehaviour
    {
        #region Testing

        [Header("Testing")]
        public TestType testType;
        [SerializeField] private bool streamingEnabled;
        [SerializeField] private bool menuEnabled;
        [SerializeField] private Vector3 playerTestSpawn = Vector3.zero;

        #endregion

        #region General

        public static GameManager instance;
        private Transform cam;
        public static InputActionAsset Input { get { return instance.input; } }
        [Header("General")]
        [SerializeField] private InputActionAsset input = null;
        [SerializeField] private DifficultyProfile[] difficultyProfiles = null;
        [SerializeField] private UI.GUI GUI;

        private void Awake()
        {
            instance = this;
            cam = Camera.main.transform;
            StartAudio();
            StartStreaming();
            Game.Init();

            ObjectPool.Init();
            Gameplay.Abilities.AbilitySystem.Init();
            AI.AI.Init(aiSettings);

            GUI.StartUI(menuEnabled);
            {
                UISetting[] s = UI.GUI.instance.GetComponentsInChildren<UISetting>(true);
                foreach (UISetting se in s)
                    se.Init();
            }
        }
        private void Start()
        {
            StopRendering();
            Physics.gravity = Vector3.zero;
            Application.backgroundLoadingPriority = ThreadPriority.High;
            
            if (testType == TestType.None || testType == TestType.UI)
            {
                UI.GUI.OpenTitleScreen();
            }     

            if (testType == TestType.None && !UnityEngine.SceneManagement.SceneManager.GetSceneByName("World").IsValid())
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(1, UnityEngine.SceneManagement.LoadSceneMode.Additive);
            }

            if (!menuEnabled)
            {
                LoadGameAt(0);
            }
        }
        private bool localizationInitalized = false;
        private void Update()
        {
            if (!localizationInitalized && LocalizationSettings.AvailableLocales.Locales.Count > 0)
            {
                localizationInitalized = true;
                var locale = LocalizationSettings.AvailableLocales.Locales[Settings.gameplay.language.Get()];
                LocalizationSettings.SelectedLocale = locale;
            }
            Extensions.Update();
            UI.GUI.instance.UpdateUI();

            if (Game.GameState == GameState.Active && Game.PlayerStatus == PlayerStatus.Alive)
            {
                WorldStreaming.Update();
                AI.AI.Update();
                FireSystem.Update();
            }
        }
        private void OnEscape()
        {
            if (!menuEnabled) { return; }
            switch (Game.GameState)
            {
                case GameState.MainMenu:
                case GameState.PauseMenu:
                    UI.GUI.Escape();
                    break;
                case GameState.Active:
                    if (Game.PlayerStatus == PlayerStatus.UI)
                        UI.GUI.Escape();
                    else
                        Pause();
                    break;
            }
        }
        public void QuitApplication()
        {
            Debug.Log("Application Quit");
            Application.Quit();
        }

        #endregion

        #region Save & Load

        [Header("Save & Load")]
        [SerializeField] private PlayerData defaultPlayerData;
        [SerializeField] private PlayerData testData;

        public static void SaveGame()
        {
           
        }

        #endregion

        #region Streaming

        [Header("Streaming")]
        [SerializeField] private int zonesCheckedPerFrame = 64;
        [SerializeField] private GameObject loadingScreenObjects;
        public int[] loadDistances;

        private void StartStreaming()
        {
            if(!streamingEnabled)
            { return; }
            WorldStreaming.Init(zonesCheckedPerFrame, loadDistances);
        }

        #endregion

        #region Game

        [HideInInspector] public Vector3 startPos;
        [HideInInspector] public Quaternion startRot;
        [HideInInspector] public bool freeRoam = true;
        [HideInInspector] public bool inOutpost;
        [HideInInspector] public GameObject locations;
        [Header("Game")]
        [SerializeField] private GameObject[] activateOnGameLoad;
        [SerializeField] private Vector3 firstPlayerSpawn;
        [SerializeField] private Vector3 defaultGravity = new Vector3(0f, -9.81f, 0f);
        [SerializeField] private AISettings aiSettings = null;
        [SerializeField] private float fireEnergy;
        [SerializeField] private float fireDuration;
        [SerializeField] private float fireSpreadMultiplier;
        [SerializeField] private LayerMask fireLayers;
        [SerializeField] private int fireReshifMeters;
        [SerializeField] private LayerMask uiMask;
        [SerializeField] private LayerMask allMask;

        private void Pause()
        {
            Game.GameState = GameState.PauseMenu;
            SaveGame();
            Time.timeScale = 0;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            StopRendering();
            UI.GUI.instance.Pause();

            audioMixer.SetFloat("FXVol", -80f);
            Audio.PlayMenuSound(pauseSound);
            Music.PlayMenuMusic(Music.MenuClips.PauseTheme);
        }
        public void Unpause()
        {
            if (Game.GameState == GameState.PauseMenu)
            {
                Game.GameState = GameState.Active;
                Time.timeScale = 1;
                Gameplay.Input.Activate();

                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;

                StartRendering();
                UI.GUI.instance.Unpause();

                Music.StopMenuMusic();
                Settings.InitFXAudioSettings();
            }
            else if (Game.GameState == GameState.MainMenu)
            {
                //Quit
            }
        }
        private void OpenLoadingScreen()
        {
            UI.GUI.instance.OpenLoadingScreen();
            loadingScreenObjects.SetActive(true);
            StopRendering();
            Gameplay.Combat.Weapons.SetActive(false);
            Gameplay.Cam.active.SetLoadingScreenPosition();
            cam.GetComponent<Camera>().fieldOfView = 60;
            Enviroment.instance.LoadingScreenUpdate();
        }
        private void CloseLoadingScreen()
        {
            UI.GUI.instance.CloseLoadingScreen();
            loadingScreenObjects.SetActive(false);
            StartRendering();

            Gameplay.Combat.Weapons.SetActive(true);
            cam.SetPositionAndRotation(Gameplay.Player.Transform.position + new Vector3(0f, 0.6f, 0f), Quaternion.Euler(0f, Gameplay.Player.Transform.rotation.y, 0f));
            cam.GetComponent<Camera>().fieldOfView = Settings.visuals.fov.Get();
        }
        private void StartRendering()
        {
            cam.GetComponent<Camera>().cullingMask = allMask.value;
        }
        private void StopRendering()
        {
            cam.GetComponent<Camera>().cullingMask = uiMask.value;
        }
        public void FastTravel(int index)
        {
            Location destination = locations.transform.GetChild(index).GetComponent<Location>();
            Teleport(destination.transform.position, destination.transform.rotation);
        }
        public void PlayerDie()
        {
            Teleport(Gameplay.Player.Transform.position, Gameplay.Player.Transform.rotation);
        }
        private void Teleport(Vector3 pos, Quaternion rot)
        {
            Game.GameState = GameState.LoadingScreen;
            Gameplay.Player.active.transform.SetPositionAndRotation(pos, rot);
            OpenLoadingScreen();

            Physics.gravity = Vector3.zero;
            Time.timeScale = 0f;

            Music.PlayMenuMusic(Music.MenuClips.LoadingScreen);
            audioMixer.SetFloat("FXVol", -80f);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            AI.AI.Clear();
            FireSystem.Clear();
            ObjectPool.TidyUp();
            SaveGame();

            Application.backgroundLoadingPriority = ThreadPriority.High;

            WorldStreaming.ClearLoading();
            WorldStreaming.Teleport(pos);

            newSpawnPosition = new Vector2(pos.x, pos.z);

            onSceneLoadCompleted = new UnityEngine.Events.UnityEvent();
            onSceneLoadCompleted.AddListener(Respawn);
            StartCoroutine(GetLoadProgress(2f));
        }
        private static Vector2 newSpawnPosition;
        private void Respawn()
        {
            FireSystem.Rebuild((int)newSpawnPosition.x, (int)newSpawnPosition.y);
            Game.GameState = GameState.Active;
            Time.timeScale = 1f;
            Music.StopMenuMusic();
            Settings.InitFXAudioSettings();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Physics.gravity = defaultGravity;
            CloseLoadingScreen();
            Gameplay.Player.active.Respawn();
        }
        public static void FreeOutpost(string name)
        {
            SaveGame();
            UI.GUI.instance.FreeOutpost(name);
        }
        public static void LoadGame()
        {
            instance.LoadGameAt(Game.SaveIndex);
        }
        private void LoadGameAt(int index)
        {
            AI.AI.Clear();
            Gameplay.Player.active.CompleteReset();
            Game.SetLastPlayed(index);
            Game.SaveIndex = index;

            if (testType == TestType.None)
            {
                if (!Game.Saves[index].empty)
                {
                    IO.LoadGame(index);
                    startPos = firstPlayerSpawn;
                }
                else
                {
                    Game.Saves[index].empty = false;
                    Game.Saves[index].lastPlay = System.DateTime.Now.ToString();
                    Game.PlayerData = IO.DefaultFile<PlayerData>("default_playerdata");
                    SaveGame();
                    startPos = firstPlayerSpawn;
                }
            }
            else
            {
                Game.PlayerData = testData;
                startPos = playerTestSpawn;
            }

            startRot = Quaternion.Euler(0f, 0f, 0f);

            if (Game.GameState == GameState.PauseMenu && streamingEnabled)
            {
                WorldStreaming.ClearAll();
            }

            if (menuEnabled)
            {
                Audio.PlayMenuSound(enterGame);
                OpenLoadingScreen();
                Game.GameState = GameState.LoadingScreen;
            }

            World.SetDifficulty(Game.GetActiveSave().difficulty);
            
            Time.timeScale = 1;
            if (streamingEnabled)
            {
                onSceneLoadCompleted = new UnityEngine.Events.UnityEvent();
                onSceneLoadCompleted.AddListener(OnGameLoaded);
                Invoke(nameof(LoadStartScenes), 0.1f);
            }
            else
            {
                OnGameLoaded();
            }
        }
        private void LoadStartScenes()
        {
            Time.timeScale = 0;
            if(streamingEnabled)
                WorldStreaming.Load(new Vector3(startPos.x, 0f, startPos.z));        
        }
        private UnityEngine.Events.UnityEvent onSceneLoadCompleted;
        public System.Collections.IEnumerator GetLoadProgress(float delay)
        {
            while (WorldStreaming.Loading())
            {
                float progress = WorldStreaming.GetProgress();
                UI.GUI.instance.loadBar.fillAmount = progress;
                yield return null;
            }

            WorldStreaming.Finish();
            Application.backgroundLoadingPriority = ThreadPriority.Low;
            AI.AI.SpawnIteration();

            yield return new WaitForSecondsRealtime(delay);

            onSceneLoadCompleted.Invoke();
        }
        public void OnGameLoaded()
        {
            Gameplay.Player.active.transform.SetPositionAndRotation(startPos, startRot);
            Gameplay.Player.active.Setup();

            FireSystem.Init(fireSpreadMultiplier, fireDuration, fireEnergy, fireReshifMeters, fireLayers);

            if (testType == TestType.None || testType == TestType.UI)
            {
                locations.BroadcastMessage("Setup", SendMessageOptions.RequireReceiver);
            }

            foreach (GameObject g in activateOnGameLoad)
            {
                g.SetActive(true);
            }

            Music.StopMenuMusic();
            Settings.InitFXAudioSettings();

            Game.GameState = GameState.Active;
            Time.timeScale = 1;
            Physics.gravity = defaultGravity;
            Gameplay.Player.active.Init();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Inventory.Recompute();
            CloseLoadingScreen();
        }

        #endregion

        #region Audio

        [Header("Audio")]
        [SerializeField] private UnityEngine.Audio.AudioMixer audioMixer;
        [SerializeField] private AudioSource menuSoundSource;
        [SerializeField] private AudioSource menuMusicSource;
        [SerializeField] private AudioSource gameMusicSource;
        [SerializeField] private AudioClip pauseSound = null;
        public AudioClip hover;
        [SerializeField] private AudioClip enterGame = null;

        private void StartAudio()
        {
            Audio.Init(audioMixer, menuSoundSource);
            Music.Init(menuMusicSource, gameMusicSource, null);
        }

        #endregion
    }
}
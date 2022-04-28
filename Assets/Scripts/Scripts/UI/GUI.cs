using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace UI
{
    public class GUI : MonoBehaviour
    {
        #region General

        public static GUI instance;
        private static UIMenu currentMenu;
        private TabGroup mainMenuTabGroup;
        [Header("General")]
        public GameObject mainMenu = null;

        [Space()]
        [Header("Debug")]
        [SerializeField] private GameObject debugStats = null;
        [SerializeField] private TMPro.TMP_Text fpsText = null;
        [SerializeField] private TMPro.TMP_Text saveText = null;

        public void StartUI(bool menuEnabled)
        {
            if (instance != null)
            { Debug.LogError("Two UIs"); }
            instance = this;

            mainMenuTabGroup = mainMenu.GetComponent<TabGroup>();

            gameObject.SetActive(true);
            if (menuEnabled)
            {
                mainMenu.SetActive(true);
                UpdateGameButtons(false);

                anyButton = new InputAction(binding: "/*/<button>");
                anyButton.performed += CloseTitleScreen;
                anyButton.Enable();
            }
            else
            {
                mainMenu.SetActive(true);
            }

            waypoint.SetActive(false);

            actionTexts = new TMPro.TMP_Text[3];
            controlTexts = new TMPro.TMP_Text[3];
            for (int i = 0; i < 3; i++)
            {
                actionTexts[i] = controlOfferObjects[i].GetComponent<TMPro.TMP_Text>();
                controlTexts[i] = controlOfferObjects[i].GetChild(0).GetComponent<TMPro.TMP_Text>();
            }
        }

        public void UpdateUI()
        {
            if (Core.GameSettings.Settings.gameplay.showFPS.Get())
            {
                UpdateFPS();
            }

            if (Time.timeScale != targetTimeScale && lerpTime)
            {
                UpdateTime();
            }

            if (Core.Game.GameState == GameState.LoadingScreen)
            {
                if (tipTimer < 5f)
                {
                    tipTimer += Time.unscaledDeltaTime;
                }
                else
                {
                    tipTimer = 0f;
                    tipText1.text = tips[Random.Range(0, tips.Length)];
                    tipText2.text = tips[Random.Range(0, tips.Length)];
                }
            }
            else if (Core.Game.GameState == GameState.Active)
            {
                UpdateWorldIcons();
            }
        }

        private float checkFrames;
        private int frames;
        private void UpdateFPS()
        {
            frames++;
            checkFrames += Time.unscaledDeltaTime;
            if (checkFrames > 1f)
            {
                fpsText.text = "" + Mathf.Round(frames * (1 / checkFrames));
                frames = 0;
                checkFrames = 0;
            }
        }

        private float targetTimeScale = 1f;
        private float timeScaleLerpAmt = 1f;
        private bool lerpTime = false;
        private void UpdateTime()
        {
            float t = timeScaleLerpAmt * Time.unscaledDeltaTime;
            float d = Mathf.Abs(Time.timeScale - targetTimeScale);
            if (d > Mathf.Abs(t))
            {
                Time.timeScale += t;
            }
            else
            {
                lerpTime = false;
                Time.timeScale = targetTimeScale;
            }
        }

        public static void Escape()
        {
            if (currentMenu.onEscapeConnection.active)
            {
                OpenMenu(currentMenu.onEscapeConnection);
            }
            else if (currentMenu.onEscape != null)
            {
                currentMenu.onEscape.Invoke();
            }
        }

        #endregion

        #region HUD

        [Space()]
        [Header("HUD")]
        [SerializeField] private GameObject hud;
        [SerializeField] private TMPro.TMP_Text ammoText;
        [SerializeField] private TMPro.TMP_Text throwableText;
        [SerializeField] private Image throwableImage;
        [SerializeField] private Sprite[] throwableIcons;
        [SerializeField] private GameObject ammoImg;
        [SerializeField] private GameObject pickUp;
        public Transform[] controlOfferObjects;

        public static void HideHUD()
        {
            instance.hud.SetActive(false);
        }
        public static void UnhideHUD()
        {
            instance.hud.SetActive(true);
        }
        public static void UpdateAmmoBar()
        {
            instance.UpdateAmmo();
        }
        private void UpdateAmmo()
        {
            ammoText.text = Core.Game.PlayerData.ammo.ToString();
            throwableText.text = Core.Game.PlayerData.gadgets[Gameplay.Player.currentThrowable].ToString();

            int ammo = Core.Game.PlayerData.ammo;
            int magSize = Gameplay.Combat.Weapons.Active.wpnData.magSize;

            if (ammo < 1)
            {
                ammoImg.transform.localPosition = new Vector3(120, 0, 0);
            }
            else
            {
                ammoImg.transform.localPosition = new Vector3(120f * (1f - ((float)ammo / (float)magSize)), 0, 0);
            }

            throwableImage.sprite = throwableIcons[Gameplay.Player.currentThrowable];
        }

        public static void UpdateControlPanel(Gameplay.ControlType[] controls, string[] controlOffers)
        {
            instance.UpdateActions(controls, controlOffers);
        }

        private TMPro.TMP_Text[] actionTexts = null;
        private TMPro.TMP_Text[] controlTexts = null;
        private void UpdateActions(Gameplay.ControlType[] controls, string[] controlOffers)
        {
            int count;
            if (controlOffers.Length > 3)
            {
                count = 3;
            }
            else
            {
                count = controlOffers.Length;
            }

            for (int i = 0; i < count; i++)
            {
                controlOfferObjects[i].gameObject.SetActive(true);
                actionTexts[i].text = controlOffers[i];
                if (controls[i] == Gameplay.ControlType.Interact)
                {
                    controlTexts[i].text = "[" + Core.GameManager.Input.FindAction("Interact").GetBindingDisplayString(InputBinding.DisplayStringOptions.DontIncludeInteractions) + "]";
                }
                else if (controls[i] == Gameplay.ControlType.SecInteract)
                {
                    controlTexts[i].text = "[" + Core.GameManager.Input.FindAction("SecInteract").GetBindingDisplayString(InputBinding.DisplayStringOptions.DontIncludeInteractions) + "]";
                }
                else if (controls[i] == Gameplay.ControlType.Parkour)
                {
                    controlTexts[i].text = "[" + Core.GameManager.Input.FindAction("Jump").GetBindingDisplayString(InputBinding.DisplayStringOptions.DontIncludeInteractions) + "]";
                }
                else if (controls[i] == Gameplay.ControlType.Individual)
                {
                    controlTexts[i].text = "[" + Core.GameManager.Input.FindAction(controlOffers[i]).GetBindingDisplayString(InputBinding.DisplayStringOptions.DontIncludeInteractions) + "]";
                }
            }
            for (int d = count; d < controlOfferObjects.Length; d++)
            {
                controlOfferObjects[d].gameObject.SetActive(false);
            }
        }

        #endregion

        #region Title Screen

        private InputAction anyButton;
        [Space()]
        [Header("Title Screen")]
        [SerializeField] private GameObject titleScreen;
        [SerializeField] private AudioClip closeTitleClip;

        private void CloseTitleScreen(InputAction.CallbackContext obj)
        {
            anyButton.performed -= CloseTitleScreen;
            anyButton.Disable();
            Core.Audio.PlayMenuSound(closeTitleClip);
            titleScreen.GetComponent<Animation>().Play("TitleScreenClose");
            Invoke(nameof(DeactivateTitleScreen), animTime);
        }
        public static void OpenTitleScreen()
        {
            instance.titleScreen.SetActive(true);
        }

        private void DeactivateTitleScreen()
        {
            titleScreen.SetActive(false);
        }

        #endregion

        #region Menu

        public static void OpenMenu(UIMenu menu, AudioClip audioClip)
        {
            if (audioClip != null)
            {
                Core.Audio.PlayMenuSound(audioClip);
            }
            menu.OnEnter(currentMenu);
            currentMenu = menu;
        }
        public static void OpenMenu(UIMenuConnection menu)
        {
            if (currentMenu.OnLeave(menu.targetMenu))
            {
                Core.Audio.PlayMenuSound(menu.audioClip);
                menu.targetMenu.OnEnter(currentMenu);
                currentMenu = menu.targetMenu;
            }
        }
        private void OpenNewGameMenu()
        {
            OpenMenu(currentMenu.connections[0]);
        }
        public static void CloseMenu(AudioClip audioClip)
        {
            currentMenu.OnLeave(null);
            if (audioClip != null)
            {
                Core.Audio.PlayMenuSound(audioClip);
            }
        }
        public static void SwitchMenu(UIMenu menu)
        {
            currentMenu = menu;
        }

        #endregion

        #region ActivateElements

        public static void ShowDebugStats()
        {
            instance.debugStats.SetActive(!instance.debugStats.activeSelf);
        }

        #endregion

        #region Buttons

        public void HoverGame(int index)
        {
            Core.Game.SaveIndex = index;
        }
        public void OpenGame()
        {
            if (Core.Game.GetActiveSave().empty)
            {
                OpenNewGameMenu();
            }
            else
            {
                Core.GameManager.LoadGame();
            }
        }
        public void OpenNewGame()
        {
            Core.Game.SetDifficulty(new Difficulty());// Saves.newDifficulty;
            Core.GameManager.LoadGame();
        }

        public void DeleteGame()
        {
            Core.Game.ResetSaveToDefault(Core.Game.SaveIndex);
            UpdateGameButtons(false);
        }

        #endregion

        #region Loading Screen

        private float tipTimer = 0f;
        [Space()]
        [Header("Loading Screen")]
        [SerializeField] private GameObject loadingScreen = null;
        public Image loadBar = null;
        [SerializeField] private string[] tips = null;
        [SerializeField] private TMPro.TMP_Text tipText1 = null;
        [SerializeField] private TMPro.TMP_Text tipText2 = null;

        public void OpenLoadingScreen()
        {
            hud.SetActive(false);
            mainMenu.SetActive(false);
            loadingScreen.SetActive(true);
            mainMenuTabGroup.firstSelected = 1;
            tipText1.text = tips[Random.Range(0, tips.Length)];
            tipText2.text = tips[Random.Range(0, tips.Length)];
            saveText.text = Core.Game.SaveIndex + string.Empty;
            UpdateGameButtons();
        }
        public void CloseLoadingScreen()
        {
            hud.SetActive(true);
            loadingScreen.SetActive(false);
        }

        #endregion

        #region Main Menu

        [Space()]
        [Header("Main Menu")]
        [SerializeField] private Button[] openGameButtons = null;
        [SerializeField] private Button[] newGameButtons = null;
        [SerializeField] private float animTime = 0.25f;
        [SerializeField] private TMPro.TMP_Text moneyText;

        public void UpdateGameButtons(bool changeInteractable = true)
        {
            for (int i = 0; i < 5; ++i)
            {
                if (Core.Game.Saves[i].empty)
                {
                    newGameButtons[i].gameObject.SetActive(true);
                    openGameButtons[i].gameObject.SetActive(false);
                }
                else
                {
                    newGameButtons[i].gameObject.SetActive(false);
                    openGameButtons[i].gameObject.SetActive(true);
                    openGameButtons[i].transform.GetChild(1).GetComponent<TMPro.TMP_Text>().text = Core.Game.Saves[i].lastPlay.ToString();
                    openGameButtons[i].transform.GetChild(2).GetComponent<TMPro.TMP_Text>().text = Core.Game.Saves[i].percentage + "%";
                    openGameButtons[i].transform.GetChild(0).GetComponent<TMPro.TMP_Text>().text = "EXPLORING: Oberhausen";
                }
            }

            if (changeInteractable)
            {
                foreach (Button b in instance.openGameButtons)
                {
                    b.interactable = true;
                }
                instance.openGameButtons[Core.Game.SaveIndex].interactable = false;
            }
        }

        public void Pause()
        {
            mainMenu.SetActive(true);
            Map.instance.playerIcon.localPosition.Set(Gameplay.Player.Transform.position.x / 4f, Gameplay.Player.Transform.position.z / 4f, 0);
            moneyText.text = Core.Game.PlayerData.shillings + "$";
        }
        public void Unpause()
        {
            CloseMenu(null);
            mainMenu.SetActive(false);
        }

        #endregion

        #region Outpost

        [Space()]
        [Header("Outpost")]
        [SerializeField] private GameObject outpostWinPanel = null;
        [SerializeField] private TMPro.TMP_Text outpostWinName = null;

        public void FreeOutpost(string name)
        {
            lerpTime = true;
            targetTimeScale = 0f;
            timeScaleLerpAmt = -0.5f;
            hud.SetActive(false);
            outpostWinName.text = name;
            StartCoroutine(instance.OutpostPanel());
        }
        private IEnumerator OutpostPanel()
        {
            yield return new WaitForSecondsRealtime(1f);

            Core.Music.OutpostWin();
            Core.Game.PlayerStatus = PlayerStatus.UI;
            outpostWinPanel.SetActive(true);

            yield return new WaitForSecondsRealtime(5);

            lerpTime = true;
            targetTimeScale = 1f;
            timeScaleLerpAmt = 2f;
            outpostWinPanel.SetActive(false);
            Core.Game.PlayerStatus = PlayerStatus.Alive;
            hud.SetActive(true);
        }

        #endregion

        #region Shop

        //[Header("Shop")]
        //[SerializeField] private GameObject shopPanel;

        #endregion

        #region 3D

        [Space()]
        [Header("3D UI")]
        [SerializeField] private Transform worldSpaceIcons;
        [SerializeField] private GameObject waypoint;

        private void UpdateWorldIcons()
        {
            if (waypoint.activeInHierarchy)
            {
                if ((waypoint.transform.position - Gameplay.Player.Transform.position).sqrMagnitude <= 10f)
                {
                    waypoint.SetActive(false);
                    Map.instance.DeactivateWaypoint();
                }
            }

            for (int i = 0; i < worldSpaceIcons.childCount; i++)
            {
                Transform t = worldSpaceIcons.GetChild(i);
                t.LookAt(Gameplay.Cam.active.transform.position, Vector3.up);
                Icons ic = t.GetComponent<Icons>();

                float f = (t.position - Gameplay.Cam.active.transform.position).magnitude;
                if (f > ic.maxDistance || f < ic.minDistance)
                {
                    if (ic.icon.activeSelf)
                    {
                        ic.icon.SetActive(false);
                        ic.text.SetActive(false);
                    }
                }
                else
                {
                    t.localScale = new Vector3(f, f, f);
                    ic.text.GetComponent<TMPro.TMP_Text>().text = (int)f + ic.ending;
                    if (!ic.icon.activeSelf)
                    {
                        ic.icon.SetActive(true);
                        ic.text.SetActive(true);
                    }
                }
            }
        }

        #endregion
    }
}
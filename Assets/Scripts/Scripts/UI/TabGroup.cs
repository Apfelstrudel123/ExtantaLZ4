using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace UI
{
    public class TabGroup : MonoBehaviour
    {
        [SerializeField] private List<TabGroupButton> tabButtons = new List<TabGroupButton>();
        public List<UIMenu> tabMenus = new List<UIMenu>();

        [SerializeField] private RectTransform lerpGraphic = null;
        [SerializeField] private float lerpTime = 0.25f;
        private float lerped = 0f;
        private float lerpedScale = 0f;
        [SerializeField] private AudioClip switchClip;

        public int firstSelected = -1;
        [HideInInspector] public int selectedTab = -1;
        [HideInInspector] public int lastMenu = -1;
        private bool canSwitch = true;
        private TabGroupButton openAfterPopup;

        private CanvasGroup currentCanvas;
        private CanvasGroup lastCanvas;

        private void OnEnable()
        {
            if (selectedTab > -1 && selectedTab != firstSelected)
            {
                if (lastMenu >= 0)
                {
                    tabMenus[lastMenu].OnLeave(null);
                }
            }

            if (firstSelected > -1)
            {
                selectedTab = firstSelected;
                lastMenu = firstSelected;
                GUI.OpenMenu(tabMenus[firstSelected], null);
            }
            else
            {
                lerpGraphic.gameObject.SetActive(false);
            }
            canSwitch = true;
        }

        private void Update()
        {
            if (lerpGraphic != null && selectedTab > -1)
            {
                Vector3 p = new Vector3(tabButtons[selectedTab].transform.position.x + tabButtons[selectedTab].lerpPositionOffset.x, tabButtons[selectedTab].transform.position.y + tabButtons[selectedTab].lerpPositionOffset.y);
                Vector3 sc = new Vector3(tabButtons[selectedTab].lerpScale.x, tabButtons[selectedTab].lerpScale.y);
                if (lerpGraphic.position != p)
                {
                    lerpGraphic.position = Vector3.Lerp(lerpGraphic.position, p, lerped);
                    lerped += Time.unscaledDeltaTime / lerpTime;
                }
                else
                {
                    lerped = 0f;
                }

                if (lerpGraphic.localScale != sc)
                {
                    lerpGraphic.localScale = Vector3.Lerp(lerpGraphic.localScale, sc, lerpedScale);
                    lerpedScale += Time.unscaledDeltaTime / lerpTime;
                }
                else
                {
                    lerpedScale = 0f;
                }
            }
            if (lastCanvas != null)
            {
                if (lastCanvas.alpha > 0)
                {
                    lastCanvas.alpha -= Time.unscaledDeltaTime / lerpTime;
                }
            }
            if (currentCanvas != null)
            {
                if (currentCanvas.alpha < 1)
                {
                    currentCanvas.alpha += Time.unscaledDeltaTime / lerpTime;
                }
            }
        }

        public void OnTabEnter(TabGroupButton button)
        {

        }
        public void OnTabExit(TabGroupButton button)
        {

        }

        public void OnTabSelected(TabGroupButton button)
        {
            if (!canSwitch)
            {
                return;
            }

            if (button.index == selectedTab)
            {
                return;
            }

            if (selectedTab > -1)
            {
                if (tabMenus[selectedTab].beforeCloseActivated)
                {
                    tabMenus[selectedTab].beforeClose.Invoke();
                    tabMenus[selectedTab].onLeavePopup.AddListener(GetPopupResult);
                    openAfterPopup = button;
                    return;
                }
                tabButtons[selectedTab].Deselect();
            }
            else if (firstSelected > -1)
            {
                lerpGraphic.position = button.transform.position + button.lerpPositionOffset;
            }

            lerpGraphic.gameObject.SetActive(true);
            lastCanvas = currentCanvas;
            lastMenu = selectedTab;
            selectedTab = button.index;

            currentCanvas = tabMenus[selectedTab].GetComponent<CanvasGroup>();

            Core.Audio.PlayMenuSound(switchClip);

            tabButtons[selectedTab].Select();
            tabMenus[button.index].OnEnter(null);
            StartCoroutine(DisableTab(button.index));
            canSwitch = false;
        }

        public void CloseAll()
        {
            for (int i = 0; i < tabButtons.Count; i++)
            {
                tabButtons[i].Deselect();
                tabMenus[i].OnLeave(null);
            }
            selectedTab = -1;
            canSwitch = true;
            lastMenu = -1;
        }
        private void GetPopupResult()
        {
            OnTabSelected(openAfterPopup);
        }
        private IEnumerator DisableTab(int index)
        {
            yield return new WaitForSecondsRealtime(lerpTime);
            if (lastMenu >= 0)
            {
                tabMenus[lastMenu].OnLeave(tabMenus[lastMenu]);
            }
            canSwitch = true;
        }
    }
}
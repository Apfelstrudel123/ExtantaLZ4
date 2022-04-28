using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Events;
namespace UI
{
    public class TabGroupButton : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler
    {
        [SerializeField] private TabGroup tabGroup;
        public Graphic[] targetGraphics;
        public int index;
        [SerializeField] private UnityEvent OnTabSelected = null;
        [SerializeField] private UnityEvent OnTabDeselected = null;
        [SerializeField] private Color tabIdle = Color.white;
        [SerializeField] private Color tabHover = Color.gray;
        [SerializeField] private Color tabActive = Color.black;
        public Vector3 lerpPositionOffset;
        public Vector3 lerpScale;

        public void OnPointerClick(PointerEventData eventData)
        {
            foreach (Graphic g in targetGraphics)
            {
                g.color = tabActive;
            }
            tabGroup.OnTabSelected(this);
        }
        public void OnPointerEnter(PointerEventData eventData)
        {
            tabGroup.OnTabEnter(this);
            GUI.SwitchMenu(tabGroup.tabMenus[index]);
            foreach (Graphic g in targetGraphics)
            {
                g.color = tabHover;
            }
        }
        public void OnPointerExit(PointerEventData eventData)
        {
            tabGroup.OnTabExit(this);
            foreach (Graphic g in targetGraphics)
            {
                g.color = tabIdle;
            }
        }

        public void Select()
        {
            if (OnTabSelected != null)
            {
                OnTabSelected.Invoke();
            }
        }
        public void Deselect()
        {
            OnTabDeselected.Invoke();
        }
    }
}
using UnityEngine;
using UnityEngine.Events;
namespace UI
{
    public class UIMenu : MonoBehaviour
    {
        public UIMenuConnection[] connections;
        [HideInInspector] public UIMenuConnection onEscapeConnection = null;
        [SerializeField] private GameObject content;

        public UnityEvent onOpen;
        public UnityEvent onClose;
        public UnityEvent onEscape;
        public UnityEvent beforeClose;
        public bool beforeCloseActivated;

        [HideInInspector] public UnityEvent onLeavePopup;

        private void Start()
        {
            UpdateConnections();
        }
        public void UpdateConnections()
        {
            onEscapeConnection = new UIMenuConnection
            {
                active = false
            };
            for (int i = 0; i < connections.Length; i++)
            {
                if (connections[i].active && connections[i].useOnEscape)
                {
                    onEscapeConnection = connections[i];
                    return;
                }
            }
        }
        public void OnEnter(UIMenu lastMenu)
        {
            content.SetActive(true);
            if (onOpen != null)
            {
                onOpen.Invoke();
            }
        }
        public bool OnLeave(UIMenu newMenu)
        {
            if (beforeCloseActivated && newMenu != null)
            {
                beforeClose.Invoke();
                return false;
            }
            if (onClose != null)
            {
                onClose.Invoke();
            }
            content.SetActive(false);
            return true;
        }
        public void OpenConnection(int index)
        {
            GUI.OpenMenu(connections[index]);
        }
        public void LeavePopup()
        {
            beforeCloseActivated = false;
            onLeavePopup.Invoke();
        }
    }
    [System.Serializable]
    public class UIMenuConnection
    {
        public bool active = true;
        public UIMenu targetMenu;
        public bool useOnEscape = false;
        public float transitionTime;
        public AudioClip audioClip;
    }
}
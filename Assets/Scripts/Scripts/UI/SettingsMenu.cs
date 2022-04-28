using UnityEngine;
using Core.GameSettings;

namespace UI
{
    public class SettingsMenu : MonoBehaviour
    {
        private VisualProfile visualSettings;
        [SerializeField] private UIMenu visualMenu = null;
        [SerializeField] private UnityEngine.UI.Button applyButton = null;
        
        public void OnOpenVisuals()
        {
            visualMenu.beforeCloseActivated = false;
            applyButton.interactable = false;
            visualSettings = Settings.visuals;
        }

        public void OnGameplayChange()
        {
            
        }
        public void OnVisualChange()
        {
            if (visualSettings == Settings.visuals)
            {
                visualMenu.beforeCloseActivated = false;
                applyButton.interactable = false;
            }
            else
            {
                visualMenu.beforeCloseActivated = true;
                applyButton.interactable = true;
            }
        }
        public void OnAudioChange()
        {
            Settings.InitAudio();
        }

        public void ResetVisuals()
        {
            Settings.visuals = visualSettings;
        }
        public void ApplyVisuals()
        {
            visualMenu.beforeCloseActivated = false;
            applyButton.interactable = false;
            //Settings.visuals = visualSettings;
            Settings.InitVisuals();
        }
    }
}

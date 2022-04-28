using UnityEngine;
using UnityEngine.UI;
using Gameplay.Abilities;
namespace UI
{
    public class AbilityMenu : MonoBehaviour
    {
        public static AbilityMenu instance;
        [SerializeField] private Image enduranceImg = null;
        [SerializeField] private Image shootingImg = null;
        [SerializeField] private Image drivingImg = null;
        [SerializeField] private Image stealthImg = null;

        [SerializeField] private TMPro.TMP_Text points;

        public Color lockedColor;
        public Color unlockedColor;

        private void Awake()
        {
            instance = this;
        }

        public void Init()
        {
            points.text = Core.Game.PlayerData.abilityPoints.ToString();
            enduranceImg.fillAmount = Core.Game.PlayerData.strengths[(int)AbilityCategory.Endurance] / 100;
            shootingImg.fillAmount = Core.Game.PlayerData.strengths[(int)AbilityCategory.Shooting] / 100;
            drivingImg.fillAmount = Core.Game.PlayerData.strengths[(int)AbilityCategory.Riding] / 100;
            stealthImg.fillAmount = Core.Game.PlayerData.strengths[(int)AbilityCategory.Stealth] / 100;
        }

        public void OnUnlock()
        {
            points.text = Core.Game.PlayerData.abilityPoints.ToString();
        }
    }
}
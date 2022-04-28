using UnityEngine;
using UnityEngine.UI;
using ScriptableObjects;
using Gameplay.Abilities;
namespace UI
{
    public class AbilityUI : MonoBehaviour
    {
        [SerializeField] private string _name;
        [SerializeField] private AbilityUI[] unlocksAbilities;
        [SerializeField] private Image icon;
        private Ability ability;
        private bool unlocked;

        private void Start()
        {
            ability = AbilitySystem.GetAbility(_name, out unlocked);
            if (unlocked)
            {
                GetComponent<Image>().color = AbilityMenu.instance.unlockedColor;
                icon.color = AbilityMenu.instance.unlockedColor;
            }
        }

        public void Unlock()
        {
            if (Core.Game.PlayerData.abilityPoints < ability.cost || unlocked)
            { return; }

            unlocked = true;
            AbilitySystem.Unlock(_name);

            GetComponent<Image>().color = AbilityMenu.instance.unlockedColor;
            icon.color = AbilityMenu.instance.unlockedColor;
        }
    }
}
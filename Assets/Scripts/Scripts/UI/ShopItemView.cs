using UnityEngine;
using TMPro;
using UnityEngine.UI;
using ScriptableObjects;
namespace UI
{
    public class ShopItemView : MonoBehaviour
    {
        [SerializeField] private TMP_Text nameText = null;
        [SerializeField] private TMP_Text priceText = null;
        [SerializeField] private Image icon = null;

        public void Init(PlayerItem item)
        {
            nameText.text = item.name;
            icon.sprite = item.icon;
            priceText.text = item.price + Shop.instance.shillingChar;
            if (item.price > Core.Game.PlayerData.shillings)
            {
                priceText.color = Shop.instance.inaffordable;
            }
            else
            {
                priceText.color = Shop.instance.affordable;
            }
        }
    }
}
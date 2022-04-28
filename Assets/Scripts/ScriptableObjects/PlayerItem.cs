using UnityEngine;
namespace ScriptableObjects
{
    [CreateAssetMenu(menuName = "ScriptableObjects/Player Item")]
    public class PlayerItem : ScriptableObject
    {
        public enum ShopCategory
        {
            Weapons,
            Ammo,
            Gadgets,
            Resources,
            Miscellaneous,
            None,
        }

        public int itemID;
        public Sprite icon;
        public string description;
    
        public ShopCategory category;
        public int price;
        public int amount;

        public int width;
        public int height;
    }
}

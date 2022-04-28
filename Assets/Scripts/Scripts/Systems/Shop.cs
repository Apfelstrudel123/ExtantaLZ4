using UnityEngine;
using ScriptableObjects;
using Gameplay;
public class Shop : MonoBehaviour
{
    public static Shop instance;

    private int currentIndex;

    private PlayerItem.ShopCategory category = PlayerItem.ShopCategory.None;

    [SerializeField] private GameObject shopItemPrefab = null;
    private Transform shopItemParent;
    [SerializeField] private int longestList = 30;

    public string shillingChar = "$";
    public Color affordable;
    public Color inaffordable;

    private void Awake()
    {
        instance = this;
        shopItemParent = transform.GetChild(0).GetChild(0);
        for (int i = 0; i < longestList; i++)
        {
            Instantiate(shopItemPrefab, shopItemParent);
        }
    }

    private void OnEnable()
    {
        category = PlayerItem.ShopCategory.None;
        ChangeMenu(PlayerItem.ShopCategory.Weapons);
    }

    public void Buy()
    {
        if(Inventory.items[currentIndex].price > Core.Game.PlayerData.shillings)
        {
            return;
        }
        Inventory.AddItem(Inventory.items[currentIndex].itemID, Inventory.items[currentIndex].amount);
        Inventory.Transaction(-Inventory.items[currentIndex].price);
    }

    public void ChangeMenu(PlayerItem.ShopCategory cat)
    {      
        shopItemParent.GetComponent<RectTransform>().sizeDelta = new Vector2(405, 83 * Inventory.items.Length + 10);

        for(int i = 0; i < Inventory.items.Length; i++)
        {
            shopItemParent.GetChild(i).gameObject.SetActive(true);
            shopItemParent.GetChild(i).GetComponent<UI.ShopItemView>().Init(Inventory.items[i]);
        }

        for (int i = Inventory.items.Length; i < longestList; i++)
        {
            shopItemParent.GetChild(i).gameObject.SetActive(false);
        }
        category = cat;
    }
}
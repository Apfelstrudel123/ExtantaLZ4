using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace UI
{
    public class Inventory : MonoBehaviour
    {
        public static Inventory instance;
        [SerializeField] private Image weaponImg;
        [SerializeField] private Image sidearmImg;

        [HideInInspector] public static bool[,] slotsUsed = new bool[25, 30];

        [SerializeField] private GameObject itemPrefab;
        private List<GameObject> itemPrefs = new List<GameObject>();
        [SerializeField] private Transform itemParent;

        private void Awake()
        {
            instance = this;
        }

        public static void Recompute()
        {
            for (int x = 0; x < 25; x++)
            {
                for (int y = 0; y < 30; y++)
                {
                    slotsUsed[x, y] = false;
                }
            }

            int w;
            int h;
            Vector2 pos;
            for (int i = 0; i < Core.Game.PlayerData.itemIDs.Length; i++)
            {
                w = Gameplay.Inventory.items[Core.Game.PlayerData.itemIDs[i]].width;
                h = Gameplay.Inventory.items[Core.Game.PlayerData.itemIDs[i]].height;
                pos.x = Core.Game.PlayerData.itemLocsX[i];
                pos.y = Core.Game.PlayerData.itemLocsY[i];

                for (int x = (int)pos.x; x < pos.x + w; x++)
                {
                    for (int y = (int)pos.y; y < pos.y + h; y++)
                    {
                        slotsUsed[x, y] = false;
                    }
                }
            }
        }

        private void OnEnable()
        {
            for (int i = 0; i < itemPrefs.Count; i++)
            {
                Destroy(itemPrefs[i]);
            }
            itemPrefs.Clear();
            int w;
            int h;
            for (int i = 0; i < Core.Game.PlayerData.itemIDs.Length; i++)
            {
                w = Gameplay.Inventory.items[Core.Game.PlayerData.itemIDs[i]].width;
                h = Gameplay.Inventory.items[Core.Game.PlayerData.itemIDs[i]].height;
                itemPrefs.Add(Instantiate(itemPrefab, itemParent));
                itemPrefs[i].transform.localScale = new Vector3(w, h, 1);
                itemPrefs[i].transform.localPosition = Vector3.zero;
                itemPrefs[i].transform.position += new Vector3(Core.Game.PlayerData.itemLocsX[i] * 26 + (w - 1) * 13, -Core.Game.PlayerData.itemLocsY[i] * 26 - (h - 1) * 13, 0);
            }
        }
    }
}
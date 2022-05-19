using System.Collections.Generic;
using UnityEngine;
using Core;
using ScriptableObjects;

namespace Gameplay
{
    public static class Inventory
    {
		public static PlayerItem[] items;
		private static int invSizeX = 25;
		private static int invSizeY = 30;

		public static void Init(string itemPath)
        {
			PlayerItem[] p = Resources.LoadAll<PlayerItem>(itemPath);
			items = new PlayerItem[p.Length];
			for (int i = 0; i < p.Length; i++)
			{
				items[p[i].itemID] = p[i];
			}
		}

		public static bool CanAddItem(int id, out Vector2 loc)
		{
			int w = items[id].width;
			int h = items[id].height;

			for (int y = 0; y < invSizeY-h; y++)
			{
				int c = 0;
				for (int x = 0; x <= invSizeX-w+c; ++x)
				{
					if (UI.Inventory.slotsUsed[x, y])
						c = 0;
					else
					{
						++c;
						if (c == w)
						{
							if (CheckHeightChain(w, h, x, y))
							{
								loc.x = x - w + 1;
								loc.y = y;
								return true;
							}
							else
							{
								c = 0;
								//x -= widthChain - 1;
							}
						}
					}
				}
			}
			loc = Vector2.zero;
			return false;
		}

		private static bool CheckHeightChain(int w, int h, int x, int y)
		{
			for (int _y = y + 1; _y < y + h; ++_y)
			{
				for (int _x = x + 1 - w; _x <= x; ++_x)
				{
					if (UI.Inventory.slotsUsed[_x, _y])
					{
						return false;
					}
				}
			}
			return true;
		}

		public static void AddItem(int id, int count)
		{
			if (items[id].category == PlayerItem.ShopCategory.Ammo)
			{
				Game.PlayerData.Ammo[0] += count;
				if (Game.PlayerData.Ammo[0] > Game.PlayerData.ammoSize)
					Game.PlayerData.Ammo[0] = Game.PlayerData.ammoSize;
			}
			else if (items[id].category == PlayerItem.ShopCategory.Gadgets)
            {
				Game.PlayerData.gadgets[0] += count;
				if (Game.PlayerData.gadgets[0] > Game.PlayerData.gadgetsSize[0])
					Game.PlayerData.gadgets[0] = Game.PlayerData.gadgetsSize[0];
			}
			else
			{
				for (int i = 0; i < count; i++)
				{
					if (CanAddItem(id, out Vector2 loc))
					{
						List<int> ids = new List<int>();
						ids.AddRange(Game.PlayerData.itemIDs);
						ids.Add(id);
						Game.PlayerData.itemIDs = ids.ToArray();

						List<int> xLoc = new List<int>();
						xLoc.AddRange(Game.PlayerData.itemLocsX);
						xLoc.Add((int)loc.x);
						Game.PlayerData.itemLocsX = xLoc.ToArray();

						List<int> yLoc = new List<int>();
						yLoc.AddRange(Game.PlayerData.itemLocsY);
						yLoc.Add((int)loc.y);
						Game.PlayerData.itemLocsY = yLoc.ToArray();
						for (int x = 0; x < items[id].width; x++)
						{
							for (int y = 0; y < items[id].height; y++)
							{
								UI.Inventory.slotsUsed[x + (int)loc.x, y + (int)loc.y] = true;
							}
						}

						//PlayerData.Current.itemCounts[id] = count;
						//Debug.Log("Pickup Succ");
					}
					else
					{
						//Debug.Log("Pickup failed");
					}
				}
			}

			UI.GUI.UpdateAmmoBar();
		}

		public static void RemoveItem(int id, int count)
		{
			if (System.Array.Exists<int>(Game.PlayerData.itemIDs, i => i == id))
				return;

			Game.PlayerData.itemCounts[id] -= count;
			if (Game.PlayerData.itemCounts[id] <= 0)
			{
				Game.PlayerData.itemCounts[id] = 0;
			}
		}
		public static void Transaction(int amt)
		{
			Game.PlayerData.shillings += amt;
		}
	}
}

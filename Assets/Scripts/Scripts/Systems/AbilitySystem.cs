using UnityEngine;
using System.Collections.Generic;
using ScriptableObjects;
using GameWorld;
namespace Gameplay
{
    namespace Abilities
    {
        public enum AbilityCategory
        {
            Endurance = 0,
            Shooting = 1,
            Riding = 2,
            Stealth = 3,
        }

        public static class AbilitySystem
        {
            private static Ability[] abilities;

            public static void Init()
            {
                abilities = Resources.LoadAll<Ability>("/Abilities");
            }

            public static Ability GetAbility(string name, out bool unlocked)
            {
                unlocked = false;
                foreach (Ability a in abilities)
                {
                    if (a.name == name)
                    {
                        if (Contains(name))
                        {
                            unlocked = true;
                        }
                        return a;
                    }
                }
                Debug.Log("Ability not found" + name);
                return null;
            }
            public static Ability GetAbility(string name)
            {
                foreach (Ability a in abilities)
                {
                    if (a.name == name)
                    {
                        return a;
                    }
                }
                Debug.Log("Ability not found" + name);
                return null;
            }

            public static void UpdateValue(AbilityCategory type, float amt)
            {
                Core.Game.PlayerData.strengths[(int)type] += amt *= World.Difficulty.abilityProgressMultiplier;
            }

            public static void Unlock(string name)
            {
                List<string> s = new();
                s.AddRange(Core.Game.PlayerData.abilities);
                s.Add(name);
                Core.Game.PlayerData.abilities = s.ToArray();
                Core.Game.PlayerData.abilityPoints -= GetAbility(name).cost;
                UI.AbilityMenu.instance.OnUnlock();
            }

            private static bool Contains(string s)
            {
                foreach (string n in Core.Game.PlayerData.abilities)
                {
                    if (s == n)
                    {
                        return true;
                    }
                }
                return false;
            }
        }
    }    
}
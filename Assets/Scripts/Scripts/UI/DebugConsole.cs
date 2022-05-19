using System.Collections.Generic;
using UnityEngine;
using GameWorld;
using Gameplay;
namespace UI
{
    public class DebugConsole : MonoBehaviour
    {
        public static DebugConsole instance;
        private bool show = false;
        public bool IsActive { get { return show; } }
        private bool cursorWasLocked;

        private string input = string.Empty;
        private string lastInput = string.Empty;
        private string lastHelp = string.Empty;

        private DebugCommand KILL_PLAYER;
        private DebugCommand<int> ADD_HEALTH;
        private DebugCommand<int> SUB_HEALTH;
        private DebugCommand<int> ADD_AMMO;
        private DebugCommand<int> ADD_NADE;
        private DebugCommand<int> ADD_MOLOTOV;
        private DebugCommand<int> ADD_MONEY;
        private DebugCommand<int> SET_TIME;
        private DebugCommand<int> PASS_HOURS;
        private DebugCommand SHOW_DEBUGSTATS;
        private DebugCommand RENDER_TREES;
        private DebugCommand AI_SPAWN_ITERATION;

        public List<object> commands;
        public List<object> mostUsefulCommands;

        private void Awake()
        {
            instance = this;
            KILL_PLAYER = new DebugCommand("kill_player", "Subtracts the players current health", "kill_player", () =>
            {
                Player.health.AddHealth(-Player.health.currentHealth, ObjectHealth.DamageType.Script);
            });
            ADD_HEALTH = new DebugCommand<int>("add_health", "Adds player health (int amount)", "add_health", (x) =>
            {
                Player.health.AddHealth(x, ObjectHealth.DamageType.Script);
            });
            SUB_HEALTH = new DebugCommand<int>("sub_health", "Subtracts player health (int amount)", "sub_health", (x) =>
            {
                Player.health.SubtractHealth(x, ObjectHealth.DamageType.Script);
            });

            ADD_AMMO = new DebugCommand<int>("add_ammo", "Adds ammo (int amount)", "add_ammo", (x) =>
            {
                Core.Game.PlayerData.Ammo[0] += x;
                GUI.UpdateAmmoBar();
            });
            ADD_NADE = new DebugCommand<int>("add_nade", "Adds grenades (int amount)", "add_nade", (x) =>
            {
                Core.Game.PlayerData.gadgets[0] += x;
                GUI.UpdateAmmoBar();
            });
            ADD_MOLOTOV = new DebugCommand<int>("add_molotov", "Adds molotovs (int amount)", "add_molotov", (x) =>
            {
                Core.Game.PlayerData.gadgets[1] += x;
                GUI.UpdateAmmoBar();
            });

            ADD_MONEY = new DebugCommand<int>("add_money", "Adds money (int amount)", "add_money", (x) =>
            {
                Core.Game.PlayerData.shillings += x;
            });

            SET_TIME = new DebugCommand<int>("set_time", "Sets the time (int time) 0 - 2400", "set_time", (x) =>
            {
                Enviroment.instance.SetTime(x);
            });
            PASS_HOURS = new DebugCommand<int>("pass_hours", "Skip hours (int amount)", "pass_hours", (x) =>
            {
                Enviroment.instance.SkipHours(x);
            });

            SHOW_DEBUGSTATS = new DebugCommand("show_debugstats", "Show debugging statistics in the top left corner", "show_debugstats", () =>
            {
                GUI.ShowDebugStats();
            });

            RENDER_TREES = new DebugCommand("render_trees", "Switch On/off the rendering of Trees", "render_trees", () =>
            {
                Enviroment.ShowTrees(!Enviroment.showTrees);
            });

            AI_SPAWN_ITERATION = new DebugCommand("ai_spawn_iter", "Check all spawnpoints and spawn valid.", "ai_spawn_iter", () =>
            {
                AI.AI.SpawnIteration();
            });

            commands = new List<object>
        {
            KILL_PLAYER,
            ADD_AMMO,
            ADD_NADE,
            ADD_MOLOTOV,
            ADD_MONEY,
            SET_TIME,
            PASS_HOURS,
            SHOW_DEBUGSTATS,
            ADD_HEALTH,
            SUB_HEALTH,
            RENDER_TREES,
            AI_SPAWN_ITERATION,
        };

            mostUsefulCommands = new List<object>
        {
            KILL_PLAYER,
            ADD_HEALTH,
            SUB_HEALTH,
            ADD_AMMO,
            ADD_NADE,
            ADD_MOLOTOV,
            ADD_MONEY,
            SET_TIME,
            RENDER_TREES,
            SHOW_DEBUGSTATS,
        };
        }

        private void OnConsole()
        {
            show = !show;
            if (show)
            {
                cursorWasLocked = !Cursor.visible;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                if (cursorWasLocked)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
            }
        }

        private void OnGUI()
        {
            if (!show) { return; }

            float y = Screen.height * 0.2f;
            lastHelp = GetHelpOptions();
            UnityEngine.GUI.Box(new Rect(0f, 0f, Screen.width * 0.25f, y), lastHelp);
            //GUI.backgroundColor = new Color(0f, 0f, 0f, 0f);
            lastInput = input;
            input = UnityEngine.GUI.TextField(new Rect(Screen.width * 0.01f, y, Screen.width * 0.24f, Screen.height * 0.025f), input);
        }

        private void HandleInput()
        {
            string[] properties = input.Split(' ');


            for (int i = 0; i < commands.Count; ++i)
            {
                DebugCommandBase commandBase = commands[i] as DebugCommandBase;
                if (input.Contains(commandBase.CommandID))
                {
                    if (commands[i] as DebugCommand != null)
                    {
                        (commands[i] as DebugCommand).Invoke();
                    }
                    else if (commands[i] as DebugCommand<int> != null)
                    {
                        (commands[i] as DebugCommand<int>).Invoke(int.Parse(properties[1]));
                    }
                }
            }
        }

        private void OnReturn()
        {
            if (show)
            {
                HandleInput();
                input = string.Empty;
            }
        }

        private string GetHelpOptions()
        {
            if (input == lastInput) { return lastHelp; }

            string s = string.Empty;
            if (input == string.Empty)
            {
                for (int i = 0; i < 5; ++i)
                {
                    if (mostUsefulCommands[i] as DebugCommand != null)
                    {
                        s += (mostUsefulCommands[i] as DebugCommand).CommandID + " | " + (mostUsefulCommands[i] as DebugCommand).CommandDescription;
                    }
                    else if (mostUsefulCommands[i] as DebugCommand<int> != null)
                    {
                        s += (mostUsefulCommands[i] as DebugCommand<int>).CommandID + " | " + (mostUsefulCommands[i] as DebugCommand<int>).CommandDescription;
                    }
                    s += "\n";
                }

                return s;
            }


            int count = 0;

            for (int i = 0; i < commands.Count; ++i)
            {
                if (commands[i] as DebugCommand != null)
                {
                    if (input.Length > (commands[i] as DebugCommand).CommandID.Length)
                    { continue; }

                    if ((commands[i] as DebugCommand).CommandID.Contains(input))
                    {
                        s += (commands[i] as DebugCommand).CommandID + " | " + (commands[i] as DebugCommand).CommandDescription + "\n";
                        ++count;
                        if (count == 10)
                        {
                            break;
                        }
                    }
                }
                else if (commands[i] as DebugCommand<int> != null)
                {
                    if (input.Length > (commands[i] as DebugCommand<int>).CommandID.Length)
                    { continue; }

                    if ((commands[i] as DebugCommand<int>).CommandID.Contains(input))
                    {
                        s += (commands[i] as DebugCommand<int>).CommandID + " | " + (commands[i] as DebugCommand<int>).CommandDescription + "\n";

                        ++count;
                        if (count == 10)
                        {
                            break;
                        }
                    }
                }
            }
            return s;
        }
    }
}
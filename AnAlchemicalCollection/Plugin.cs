using System;
using System.Collections;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using FastTravelEnum;
using GlobalEnum;
using HarmonyLib;
using UnityEngine;

namespace AnAlchemicalCollection
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        private const string PluginGuid = "p1xel8ted.potionpermit.alchemical_collection";
        private const string PluginName = "An Alchemical Collection";
        private const string PluginVersion = "0.1.3";

        private static readonly Harmony Harmony = new(PluginGuid);

        private static ManualLogSource _logger;
        public static ConfigEntry<float> RunSpeedMultiplier;
        public static ConfigEntry<bool> EnableRunSpeedMultiplier;
        public static ConfigEntry<float> LeftRightRunSpeedMultiplier;
        public static ConfigEntry<bool> SpeedUpMenuIntro;
        public static ConfigEntry<bool> AutoChangeTool;
        public static ConfigEntry<bool> HalveToolStaminaUsage;
        public static ConfigEntry<bool> SkipLogos;
        private static ConfigEntry<bool> _saveOnExitWithF11;

        public static ConfigEntry<bool> ModifyResolutions;
        private static ConfigEntry<int> _width;
        private static ConfigEntry<int> _height;
        private static ConfigEntry<int> _refresh;
        public static ConfigEntry<int> FrameRate;

        private static ConfigEntry<string> _exitKeybind;
        private static ConfigEntry<string> _fastTravelKeybind;
        private static ConfigEntry<string> _quickSaveKeybind;
        private static ConfigEntry<string> _newsBoardKeybind;
        private static ConfigEntry<string> _toggleHudKeybind;

        public static Resolution Resolution = new()
        {
            width = 3440,
            height = 1440,
            refreshRate = 120
        };

        private void Awake()
        {
            _logger = Logger;

            //resolution
            ModifyResolutions = Config.Bind("Resolution", "ModifyResolutions", false, "Enable/Disable modifying the resolution list. Intended for use with a custom resolution.");
            _width = Config.Bind("Resolution", "Width", Display.main.systemWidth, "The width of the resolution to add to the list.");
            _height = Config.Bind("Resolution", "Height", Display.main.systemHeight, "The height of the resolution to add to the list.");
            _refresh = Config.Bind("Resolution", "Refresh", 60, "The refresh rate of the resolution to add to the list.");
            FrameRate = Config.Bind("Resolution", "TargetFrameRate", 60, "Don't know if this actually does anything, but the game sets it to 60 by default.");
            Resolution.width = _width.Value;
            Resolution.height = _height.Value;
            Resolution.refreshRate = _refresh.Value;

            //Tools
            AutoChangeTool = Config.Bind("Tools", "AutoChangeTool", true, "Tools will automatically change to the required time when near plants or hitting rocks/trees.");
            HalveToolStaminaUsage = Config.Bind("Tools", "HalveToolStaminaUsage", true, "Energy is taken every 2nd hit instead of every hit. Effectively halving the stamina usage.");

            //Logos-MainMenu
            SkipLogos = Config.Bind("Logos", "SkipLogos", true, "Enable/disable intro logos.");
            SpeedUpMenuIntro = Config.Bind("Logos", "SpeedUpMenuIntro", true, "Makes the menu appear instantly instead of the scroll down animation.");

            //saving
            _saveOnExitWithF11 = Config.Bind("Saving", "SaveOnExitWithQuickExit", true, "When using Quick Exit to immediately exit, save the game before exiting.");

            //player speed
            EnableRunSpeedMultiplier = Config.Bind("Player Speed", "ModifyRunSpeed", true, "Enable/disable modification of player run speed.");
            RunSpeedMultiplier = Config.Bind("Player Speed", "RunSpeedMultiplier", 1.5f, "Player run speed multiplier. Default is 1.5 or 50% faster.");
            LeftRightRunSpeedMultiplier = Config.Bind("Player Speed", "LeftRightRunSpeedMultiplier", 1.25f, "You shouldn't need to touch this value. But I included it just in case. It's used to make running left/right roughly the same speed as up/down.");

            //keybinds
            _exitKeybind = Config.Bind("Keybinds", "QuickExitKeybind", "F11", "Keybind to exit the game. Default is F11.");
            _fastTravelKeybind = Config.Bind("Keybinds", "FastTravelKeybind", "F4", "Fast travel keybind. Default is F4.");
            _quickSaveKeybind = Config.Bind("Keybinds", "QuickSaveKeybind", "F5", "Quick save keybind. Default is F5.");
            _newsBoardKeybind = Config.Bind("Keybinds", "NewsBoardKeybind", "F6", "News board toggle keybind. Default is F6.");
            _toggleHudKeybind = Config.Bind("Keybinds", "ToggleHudKeybind", "F7", "Hud toggle keybind keybind. Default is F7.");
        }

        private void OnEnable()
        {
            Harmony.PatchAll(Assembly.GetExecutingAssembly());
            L($"Plugin {PluginName} is loaded!");
        }

        private void OnDisable()
        {
            Harmony.UnpatchSelf();
            L($"Plugin {PluginName} is unloaded!");
        }


        private void Update()
        {
            if (UIManager.MAIN_MENU is not null && UIManager.MAIN_MENU.isActive) return;

            if (Input.GetKeyDown(Enum.TryParse<KeyCode>(_fastTravelKeybind.Value, out var ftKey) ? ftKey : KeyCode.F4))
            {
                FastTravelPatches.DoFastTravel = true;
                StartCoroutine(FastTravelIE());
            }

            if (Input.GetKeyDown(Enum.TryParse<KeyCode>(_quickSaveKeybind.Value, out var qsKey) ? qsKey : KeyCode.F5))
            {
                SaveSystemManager.SAVE();
                Helper.ShowNotification("Game Saved!", "Done!");
            }

            if (Input.GetKeyDown(Enum.TryParse<KeyCode>(_newsBoardKeybind.Value, out var nbKey) ? nbKey : KeyCode.F6))
            {
                if (UIManager.NEWS_BOARD_UI is null) return;
                if (UIManager.NEWS_BOARD_UI.isActive)
                {
                    UIManager.NEWS_BOARD_UI.OnRightClick();
                }
                else
                {
                    UIManager.NEWS_BOARD_UI.Call();
                    UIManager.NEWS_BOARD_UI.RefreshNewsBoard();
                }
            }

            if (Input.GetKeyDown(Enum.TryParse<KeyCode>(_toggleHudKeybind.Value, out var hudKey) ? hudKey : KeyCode.F7))
            {
                if (UIManager.GAME_HUD is null) return;
                if (UIManager.GAME_HUD.active)
                {
                    UIManager.GAME_HUD.Hide();
                }
                else
                {
                    UIManager.GAME_HUD.Show();
                }

                UIManager.GAME_HUD.topBlackBar.SetActive(false);
                UIManager.GAME_HUD.botBlackBar.SetActive(false);
            }

            if (Input.GetKeyDown(Enum.TryParse<KeyCode>(_exitKeybind.Value, out var exitKey) ? exitKey : KeyCode.F11))
            {
                StartCoroutine(SaveAndExitIE());
            }
        }

        private static IEnumerator FastTravelIE()
        {
            Helper.ShowNotification("Going home...", "Home!");
            yield return new WaitForSeconds(3f);
            Helper.Teleport(FastTravelID.MC_HOUSE, MapRegion.CITY);
        }

        private static IEnumerator SaveAndExitIE()
        {
            if (_saveOnExitWithF11.Value)
            {
                SaveSystemManager.SAVE();
                Helper.ShowNotification("Game Saved! Exiting...", "Bye!");
            }
            else
            {
                Helper.ShowNotification("Exiting...", "Bye!");
            }

            yield return new WaitForSeconds(2f);
            Application.Quit();
        }

        internal static void L(string message)
        {
            _logger.LogWarning(message);
        }
    }
}
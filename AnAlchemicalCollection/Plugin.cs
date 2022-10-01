using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Cinemachine;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AnAlchemicalCollection
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        private const string PluginGuid = "p1xel8ted.potionpermit.alchemical_collection";
        private const string PluginName = "An Alchemical Collection";
        private const string PluginVersion = "0.1.1";

        private static readonly Harmony Harmony = new(PluginGuid);
        private static ManualLogSource _logger;
        public static ConfigEntry<float> RunSpeedMultiplier;
        public static ConfigEntry<bool> EnableRunSpeedMultiplier;
        public static ConfigEntry<float> LeftRightRunSpeedMultiplier;
        public static ConfigEntry<bool> SkipLogos;
        

        private void Awake()
        {
            _logger = Logger;
            SkipLogos = Config.Bind("Logos", "SkipLogos", true, "Enable/disable intro logos.");
            EnableRunSpeedMultiplier = Config.Bind("Player Speed", "ModifyRunSpeed", true, "Enable/disable modification of player run speed.");
            RunSpeedMultiplier = Config.Bind("Player Speed", "RunSpeedMultiplier", 1.5f, "Player run speed multiplier. Default is 1.5 or 50% faster.");
            LeftRightRunSpeedMultiplier = Config.Bind("Player Speed", "LeftRightRunSpeedMultiplier", 1.25f, "You shouldn't need to touch this value. But I included it just in case. It's used to make running left/right roughly the same speed as up/down/diagonal.");
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
            if (Input.GetKeyDown(KeyCode.F6))
            {
                UIManager.NEWS_BOARD_UI.Call();
            }
            
            if (Input.GetKeyDown(KeyCode.F7))
            {
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
        }

        internal static void L(string message)
        {
            _logger.LogWarning(message);
        }
    }
}
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace AnAlchemicalCollection
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        private const string PluginGuid = "p1xel8ted.potionpermit.qol";
        private const string PluginName = "An Alchemical Collection";
        private const string PluginVersion = "0.1.0";

        private static readonly Harmony Harmony = new(PluginGuid);
        private static ManualLogSource _logger;

        private void Awake()
        {
            _logger = Logger;
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
            if (Input.GetKeyDown(KeyCode.F6))
            {
                UIManager.NEWS_BOARD_UI.Call();
            }
        }

        private static void L(string message)
        {
            _logger.LogWarning(message);
        }
    }
}
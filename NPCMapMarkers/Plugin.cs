using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using NotificationEnum;
using UnityEngine;

namespace NPCMapMarkers
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        private const string PluginGuid = "p1xel8ted.potionpermit.NPCMapMarkers";
        private const string PluginName = "Potion Permit NPCMapMarkers";
        private const string PluginVersion = "0.1.0";

        private static readonly Harmony Harmony = new(PluginGuid);
        private static ManualLogSource _logger;
        private static string PluginPath { get; set; }
        public static Sprite NpcSprite { get; private set; }

        private void Awake()
        {
            PluginPath = Path.GetDirectoryName(Info.Location);
            NpcSprite = SpriteGen.CreateSpriteFromPath(Path.Combine(PluginPath!, "assets", "npc.png"));
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

        public static void L(string message)
        {
            _logger.LogWarning(message);
        }

    }
}
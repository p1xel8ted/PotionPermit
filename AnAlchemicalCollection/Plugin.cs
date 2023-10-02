using System.Collections;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using FastTravelEnum;
using GlobalEnum;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AnAlchemicalCollection;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public class Plugin : BaseUnityPlugin
{
    private const string PluginGuid = "p1xel8ted.potionpermit.alchemical_collection";
    private const string PluginName = "An Alchemical Collection";
    private const string PluginVersion = "0.1.5";

    private static readonly Harmony Harmony = new(PluginGuid);
    private static ManualLogSource Log { get; set; }
    public static ConfigEntry<float> RunSpeedMultiplier { get; private set; }
    public static ConfigEntry<bool> EnableRunSpeedMultiplier { get; private set; }
    public static ConfigEntry<float> LeftRightRunSpeedMultiplier { get; private set; }
    public static ConfigEntry<bool> SpeedUpMenuIntro { get; private set; }
    public static ConfigEntry<bool> AutoChangeTool { get; private set; }
    public static ConfigEntry<bool> HalveToolStaminaUsage { get; private set; }
    public static ConfigEntry<bool> SkipLogos { get; private set; }
    private static ConfigEntry<bool> SaveOnExitWithF11 { get; set; }

    public static ConfigEntry<bool> ModifyResolutions { get; private set; }
    public static ConfigEntry<bool> CustomTargetFramerate { get; private set; }
    private static ConfigEntry<int> Width { get; set; }
    private static ConfigEntry<int> Height { get; set; }
    private static ConfigEntry<int> Refresh { get; set; }
    public static ConfigEntry<int> FrameRate { get; private set; }
    private static ConfigEntry<KeyboardShortcut> ExitKeybind { get; set; }
    private static ConfigEntry<KeyboardShortcut> FastTravelKeybind { get; set; }
    private static ConfigEntry<KeyboardShortcut> QuickSaveKeybind { get; set; }
    private static ConfigEntry<KeyboardShortcut> NewsBoardKeybind { get; set; }
    private static ConfigEntry<KeyboardShortcut> ToggleHudKeybind { get; set; }

    internal static ConfigEntry<int> CameraZoom { get; private set; }
    internal static int CameraZoomBacking { get; set; }
    private static ConfigEntry<bool> TimeManipulation { get; set; }
    internal static ConfigEntry<float> TimeMultiplier { get; private set; }

    private static ConfigEntry<float> IncreaseUpdateRate { get; set; }
    private static TimePatches TimeInstance { get; set; }

    private static int MaxRefreshRate => Screen.resolutions.Max(a => a.refreshRate);

    public static Resolution Resolution = new()
    {
        width = Display.main.systemWidth,
        height = Display.main.systemHeight,
        refreshRate = MaxRefreshRate
    };

    private void Awake()
    {
        Log = Logger;
        SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        TimeInstance = gameObject.AddComponent<TimePatches>();

        // Display Resolution Configuration
        ModifyResolutions = Config.Bind("1. Display Settings", "Enable Custom Resolution", false,
            new ConfigDescription("Toggle the usage of custom resolution settings.", null,
                new ConfigurationManagerAttributes {Order = 101}));
        Width = Config.Bind("1. Display Settings", "Custom Width", Display.main.systemWidth,
            new ConfigDescription("Define the custom display width.", null,
                new ConfigurationManagerAttributes {Order = 100}));
        Height = Config.Bind("1. Display Settings", "Custom Height", Display.main.systemHeight,
            new ConfigDescription("Define the custom display height.", null,
                new ConfigurationManagerAttributes {Order = 99}));
        Refresh = Config.Bind("1. Display Settings", "Custom Refresh Rate", Screen.resolutions.Max(a => a.refreshRate),
            new ConfigDescription("Define the custom display refresh rate.", null,
                new ConfigurationManagerAttributes {Order = 98}));
        CustomTargetFramerate = Config.Bind("1. Display Settings", "Enable Custom Target Frame Rate", false,
            new ConfigDescription("Toggle the usage of custom target frame rate settings. May or may not do anything.",
                null, new ConfigurationManagerAttributes {Order = 97}));
        FrameRate = Config.Bind("1. Display Settings", "Target Frame Rate", Screen.resolutions.Max(a => a.refreshRate),
            new ConfigDescription("Set the target frame rate.", null, new ConfigurationManagerAttributes {Order = 96}));
        Resolution.width = Width.Value;
        Resolution.height = Height.Value;
        Resolution.refreshRate = Refresh.Value;

        // Tool Usage Configuration
        AutoChangeTool = Config.Bind("2. Tools Settings", "Automatic Tool Switching", true,
            new ConfigDescription("Enable the automatic tool switching based on context.", null,
                new ConfigurationManagerAttributes {Order = 90}));
        HalveToolStaminaUsage = Config.Bind("2. Tools Settings", "Halve Stamina Usage", true,
            new ConfigDescription("Enable the halving of stamina usage for tools.", null,
                new ConfigurationManagerAttributes {Order = 89}));

        // Game Intro Configuration
        SkipLogos = Config.Bind("3. Intro Settings", "Skip Intro Logos", true,
            new ConfigDescription("Enable or disable the intro logos.", null,
                new ConfigurationManagerAttributes {Order = 80}));
        SpeedUpMenuIntro = Config.Bind("3. Intro Settings", "Accelerate Menu Intro", true,
            new ConfigDescription("Enable or disable the acceleration of menu intro.", null,
                new ConfigurationManagerAttributes {Order = 79}));

        // Saving Configuration
        SaveOnExitWithF11 = Config.Bind("4. Saving Settings", "Save On Quick Exit", true,
            new ConfigDescription("Enable saving the game on quick exit.", null,
                new ConfigurationManagerAttributes {Order = 70}));

        // Player Speed Configuration
        EnableRunSpeedMultiplier = Config.Bind("5. Player Speed", "Modify Run Speed", true,
            new ConfigDescription("Enable the modification of player run speed.", null,
                new ConfigurationManagerAttributes {Order = 60}));
        RunSpeedMultiplier = Config.Bind("5. Player Speed", "Run Speed Multiplier", 1.5f,
            new ConfigDescription("Set the player run speed multiplier.", null,
                new ConfigurationManagerAttributes {Order = 59}));
        LeftRightRunSpeedMultiplier = Config.Bind("5. Player Speed", "Lateral Run Speed Multiplier", 1.25f,
            new ConfigDescription("Set the lateral run speed multiplier.", null,
                new ConfigurationManagerAttributes {Order = 58}));

        // Keybinds Configuration
        ExitKeybind = Config.Bind("6. Keybinds", "Quick Exit Key", new KeyboardShortcut(KeyCode.F11),
            new ConfigDescription("Set the key for quick exit.", null,
                new ConfigurationManagerAttributes {Order = 50}));
        FastTravelKeybind = Config.Bind("6. Keybinds", "Fast Travel Key", new KeyboardShortcut(KeyCode.F4),
            new ConfigDescription("Set the key for fast travel.", null,
                new ConfigurationManagerAttributes {Order = 49}));
        QuickSaveKeybind = Config.Bind("6. Keybinds", "Quick Save Key", new KeyboardShortcut(KeyCode.F5),
            new ConfigDescription("Set the key for quick save.", null,
                new ConfigurationManagerAttributes {Order = 48}));
        NewsBoardKeybind = Config.Bind("6. Keybinds", "News Board Toggle Key", new KeyboardShortcut(KeyCode.F6),
            new ConfigDescription("Set the key to toggle the news board.", null,
                new ConfigurationManagerAttributes {Order = 47}));
        ToggleHudKeybind = Config.Bind("6. Keybinds", "HUD Toggle Key", new KeyboardShortcut(KeyCode.F7),
            new ConfigDescription("Set the key to toggle the HUD.", null,
                new ConfigurationManagerAttributes {Order = 46}));

        //Time manipulation
        TimeManipulation = Config.Bind("7. Time Manipulation", "Enable Time Manipulation", true,
            new ConfigDescription("Enable time manipulation.", null, new ConfigurationManagerAttributes {Order = 45}));
        TimeManipulation.SettingChanged += (_, _) => { TimeInstance.enabled = TimeManipulation.Value; };
        TimeMultiplier = Config.Bind("7. Time Manipulation", "Time Multiplier", 1.0f,
            new ConfigDescription("Set the time multiplier.", new AcceptableValueRange<float>(1, 10),
                new ConfigurationManagerAttributes {ShowRangeAsPercent = false, Order = 44}));
        TimeMultiplier.SettingChanged += (_, _) =>
        {
            if (!TimeManipulation.Value) return;
            TimeInstance.UpdateValues();
        };

        //Misc
        IncreaseUpdateRate = Config.Bind("8. Misc", "Increase Update Rate",
            Helper.CalculateLowestMultiplierAbove50(MaxRefreshRate),
            new ConfigDescription(
                "Sets the rate the camera and physics update. Can resolve camera judder, but setting too high can cause performance issues. Game default is 50fps. Ideally it should be a multiple of your refresh rate. You may/may not notice a difference.",
                new AcceptableValueRange<float>(50f, 360f),
                new ConfigurationManagerAttributes {ShowRangeAsPercent = false, Order = 43}));
        IncreaseUpdateRate.SettingChanged += (_, _) =>
        {
            Time.fixedDeltaTime = 1f / IncreaseUpdateRate.Value;
            Log.LogInfo($"FixedDeltaTime set to {Time.fixedDeltaTime} ({IncreaseUpdateRate.Value}fps)");
        };
    }

    private static void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        if (CustomTargetFramerate.Value)
        {
            Application.targetFrameRate = FrameRate.Value;
        }

        if (ModifyResolutions.Value)
        {
            Screen.SetResolution(Resolution.width, Resolution.height, Screen.fullScreen, Resolution.refreshRate);
        }
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

        if (FastTravelKeybind.Value.IsUp())
        {
            FastTravelPatches.DoFastTravel = true;
            StartCoroutine(FastTravelIE());
        }

        if (QuickSaveKeybind.Value.IsUp())
        {
            SaveSystemManager.SAVE();
            Helper.ShowNotification("Game Saved!", "Done!");
        }

        if (NewsBoardKeybind.Value.IsUp())
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

        if (ToggleHudKeybind.Value.IsUp())
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

        if (ExitKeybind.Value.IsUp())
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
        if (SaveOnExitWithF11.Value)
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

    internal static void L(string message, bool info = false)
    {
        if (info)
        {
            Log.LogInfo(message);
            return;
        }

        Log.LogWarning(message);
    }
}
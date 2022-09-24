using HarmonyLib;

namespace PotionPermit;

[HarmonyPatch]
public class Patches
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(SettingsManager), nameof(SettingsManager.LOAD))]
    public static void Load()
    {
        SettingsManager.resolutionList.Add(Plugin.Resolution);
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(GraphicSettingUI), nameof(GraphicSettingUI.SetGraphicLayout))]
    public static void SetGraphicLayout(ref GraphicSettingUI __instance)
    {
        __instance.resolutionAr.AddItem(Plugin.Resolution);
    }
}
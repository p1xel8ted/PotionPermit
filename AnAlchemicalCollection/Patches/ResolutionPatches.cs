using HarmonyLib;
using UnityEngine;

namespace AnAlchemicalCollection;

[HarmonyPatch]
public static class ResolutionPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(SettingsManager), nameof(SettingsManager.LOAD))]
    public static void Load()
    {
        if (!Plugin.ModifyResolutions.Value) return;
        SettingsManager.resolutionList.Add(Plugin.Resolution);
        Application.targetFrameRate = Plugin.FrameRate.Value;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(GraphicSettingUI), nameof(GraphicSettingUI.SetGraphicLayout))]
    public static void SetGraphicLayout(ref GraphicSettingUI __instance)
    {
        if (!Plugin.ModifyResolutions.Value) return;
        __instance.resolutionAr.AddItem(Plugin.Resolution);
        Application.targetFrameRate = Plugin.FrameRate.Value;
    } 
}
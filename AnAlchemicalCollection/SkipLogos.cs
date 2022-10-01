using HarmonyLib;
using UnityEngine;

namespace AnAlchemicalCollection;

[HarmonyPatch]
public static class SkipLogos
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(MainMenu), nameof(MainMenu.PlayMasshiveLogo))]
    public static bool MainMenu_PlayMasshiveLogo(ref MainMenu __instance)
    {
        if (!Plugin.SkipLogos.Value) return true;
        __instance.PlayMainMenuAnim();
        return false;
    }
}
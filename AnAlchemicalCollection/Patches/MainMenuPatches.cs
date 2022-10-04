using HarmonyLib;
using UnityEngine;

namespace AnAlchemicalCollection;

[HarmonyPatch]
public static class MainMenuPatches
{
    //make menu buttons appear instantly
    [HarmonyPrefix]
    [HarmonyPatch(typeof(LeanTween), nameof(LeanTween.value), typeof(float), typeof(float), typeof(float))]
    public static void LeanTween_value(LeanTween __instance, float from, float to, ref float time)
    {
        if (!Plugin.SpeedUpMenuIntro.Value) return;
        if (from == 0f && to == 1f && time == 3f)
        {
            time = 0f;
        }
    } 
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(LeanTween), nameof(LeanTween.moveLocalY), typeof(GameObject), typeof(float), typeof(float))]
    public static void LeanTween_moveLocalY(LeanTween __instance, GameObject gameObject, float to, ref float time)
    {
        if (!Plugin.SpeedUpMenuIntro.Value) return;
        if (gameObject.name == "MAIN_MENU")
        {
            time = 0f;
        }
    }
}
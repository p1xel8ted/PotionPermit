using HarmonyLib;

namespace AnAlchemicalCollection;

[HarmonyPatch]
public static class FastTravelPatches
{
    public static bool DoFastTravel;

    [HarmonyPatch(typeof(WorldMapUI), nameof(WorldMapUI.GoToDestination))]
    [HarmonyPrefix]
    public static void WorldMapUI_GoToDestination(ref WorldMapUI __instance)
    {
        if (!DoFastTravel) return;
        
        var marker = __instance.fastTravelPinList.Find(a => a.GetMarkerID == "MC_HOME");
        __instance.currentSelectedPin.Add(marker);
        __instance.currentSelectedPin[0] = marker;
        DoFastTravel = false;
    }
}
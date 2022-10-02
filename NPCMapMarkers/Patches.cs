using HarmonyLib;
using UnityEngine;

namespace NPCMapMarkers;

[HarmonyPatch]
public static class Patches
{
    private static MapMarker _mapMarker;

    [HarmonyPatch(typeof(WorldMapUI), nameof(WorldMapUI.Call))]
    [HarmonyPostfix]
    public static void WorldMapUI_Call(ref WorldMapUI __instance)
    {
        // __instance.activePinList.RemoveAll(a =>
        // {
        //     if (a.markerInfo != "NPC") return false;
        //     Object.Destroy(a);
        //     return true;
        //
        // });

        if (_mapMarker == null)
        {
            _mapMarker = __instance.gameObject.AddComponent<MapMarker>();
        }

        foreach (var npc in NPCManager.npcList)
        {
            var newMarker = Object.Instantiate(_mapMarker);
            newMarker.markerName = npc.name;

            var npcPos = new Vector3
            {
                x = npc.LOCAL_POSITION.x / 10f,
                y = npc.LOCAL_POSITION.y / 10f,
                z = -10f
            };
            newMarker.transform.localPosition = npcPos;

            newMarker.isActive = true;
            newMarker.enabled = true;
          //  newMarker.markerSprite.sprite = Plugin.NpcSprite;
            newMarker.markerInfo = "NPC";
            newMarker.Show();
            //  newMarker.markerSprite.sprite = Plugin.NpcSprite;
            __instance.activePinList.Add(newMarker);
        }
    }
}
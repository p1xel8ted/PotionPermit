using System;
using System.Collections.Generic;
using System.Linq;
using GlobalEnum;
using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AnAlchemicalCollection;

[HarmonyPatch]
public static class Patches
{
    private const float OriginalPlayerSpeed = 150f;
    private const float OriginalDogSpeed = 300f;

    //player run speed
    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerCharacter), nameof(PlayerCharacter.Start))]
    [HarmonyPatch(typeof(PlayerCharacter), nameof(PlayerCharacter.Move))]
    public static void PlayerCharacter_Move()
    {
        if (!Plugin.EnableRunSpeedMultiplier.Value) return;
        if (PlayerCharacter.Instance.CurrentDirection is Direction.Left or Direction.Right)
        {
            PlayerCharacter.Instance.MoveSpeed = (OriginalPlayerSpeed * Plugin.RunSpeedMultiplier.Value) *
                                                 Plugin.LeftRightRunSpeedMultiplier.Value;
        }
        else
        {
            PlayerCharacter.Instance.MoveSpeed = OriginalPlayerSpeed * Plugin.RunSpeedMultiplier.Value;
        }
    }

    //sets dog speed to 75% of the modified player speed
    [HarmonyPostfix]
    [HarmonyPatch(typeof(DogieAI), nameof(DogieAI.Start))]
    [HarmonyPatch(typeof(DogieAI), nameof(DogieAI.FIXED_UPDATE))]
    public static void DogieAI_Patches(ref DogieAI __instance)
    {
        if (!Plugin.EnableRunSpeedMultiplier.Value) return;
        __instance.speed = PlayerCharacter.Instance.MoveSpeed * 0.75f;
    }


    //stops ridiculous log spam
    [HarmonyPrefix]
    [HarmonyPatch(typeof(tk2dBaseSprite), nameof(tk2dBaseSprite.SetSprite), typeof(string))]
    public static bool tk2dBaseSprite_SetSprite(string spriteName, ref tk2dBaseSprite __instance, ref bool __result)
    {
        var spriteIdByName = __instance.collection.GetSpriteIdByName(spriteName, -1);
        if (spriteIdByName != -1)
        {
            __instance.SetSprite(spriteIdByName);
        }

        __result = spriteIdByName != -1;
        return false;
    }

    //stops ridiculous log spam
    [HarmonyPrefix]
    [HarmonyPatch(typeof(tk2dBaseSprite), nameof(tk2dBaseSprite.SetSprite), typeof(tk2dSpriteCollectionData),
        typeof(string))]
    public static bool tk2dBaseSprite_SetSprite(tk2dSpriteCollectionData newCollection, string spriteName,
        ref tk2dBaseSprite __instance, ref bool __result)
    {
        var spriteIdByName = newCollection.GetSpriteIdByName(spriteName, -1);
        if (spriteIdByName != -1)
        {
            __instance.SetSprite(newCollection, spriteIdByName);
        }

        __result = spriteIdByName != -1;
        return false;
    }


    [HarmonyPrefix]
    [HarmonyPatch(typeof(LeanTween), nameof(LeanTween.moveLocalY), typeof(GameObject), typeof(float), typeof(float))]
    public static void LeanTween_moveLocalY(LeanTween __instance, GameObject gameObject, float to, ref float time)
    {
        if (gameObject.name == "MAIN_MENU")
        {
            time = 0f;
        }
    }

    //UI_CAMERA/JOURNAL_UI/LAYOUT_SETTINGS */Anchor (MIddleLeft)/JOURNAL_LEFT_SIDE/LIST_CONTAINER/
    //make menu buttons appear instantly
    [HarmonyPrefix]
    [HarmonyPatch(typeof(LeanTween), nameof(LeanTween.value), typeof(float), typeof(float), typeof(float))]
    public static void LeanTween_value(LeanTween __instance, float from, float to, ref float time)
    {
        if (from == 0f && to == 1f && time == 3f)
        {
            time = 0f;
        }
    }

    // private static JournalListButton newButton;
    //
    // [HarmonyPrefix]
    // [HarmonyPatch(typeof(JournalUI), nameof(JournalUI.Navigate), typeof(int), typeof(int))]
    // public static void JournalUI_SHOW_UI(ref JournalUI __instance)
    // {
    //     if (__instance == null) return;
    //     if (newButton != null) return;
    //     var listContainer =
    //         GameObject.Find(
    //             "UI_CAMERA/JOURNAL_UI/LAYOUT_SETTINGS */Anchor (MIddleLeft)/JOURNAL_LEFT_SIDE/LIST_CONTAINER");
    //     var list = new List<JournalListButton>();
    //     listContainer.GetComponentsInChildren(list);
    //     var originalButton = list[3];
    //     newButton = Object.Instantiate(originalButton, listContainer.transform);
    //     newButton.buttonTxt.text = "Exit to Desktop";
    //     newButton.transform.localPosition = new Vector3(0, -100, -10);
    //     newButton.OnButtonExecuted = new EventHandler(__instance.CallSettingsUI);
    //     newButton.OnButtonHovered = new EventHandler(__instance.DisplayInfo);
    //     newButton.OnMouseClick = new EventHandler(__instance.CallSettingsUI);
    //     newButton.AssignButtonListPosition();
    //     newButton.enabled = true;
    //     newButton.buttonActive = true;
    // }
    //
    // [HarmonyPostfix]
    // [HarmonyPatch(typeof(JournalUI), nameof(JournalUI.GetAllButtonList), MethodType.Getter)]
    // public static void JournalUI_GetAllButtonList(ref List<ChemistCustomButton> __result)
    // {
    //     var button = __result.Last();
    //     button.name = "EXIT_TO_DESKTOP_MOUSE_DUMMY";
    //     __result.Add(button);
    // }
}
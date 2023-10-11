using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CharacterChemist;
using CharacterIDEnum;
using GlobalEnum;
using HarmonyLib;
using TutorialEnum;
using UnityEngine;

namespace AnAlchemicalCollection;

[HarmonyPatch]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public static class ToolPatches
{
    private const string Plant = "PLANT";
    private const string Tree = "TREE";
    private const string Stone = "STONE";
    private const string Rock = "ROCK";
    private static List<ToolsData> ToolsDataList { get; set; }
    private static ToolsHUDUI ToolsHud { get; set; }
    private static int StaminaUsageCounter { get; set; }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(CharacterStatus), nameof(CharacterStatus.SetStatus))]
    public static void CharacterStatus_SetStatus(ref CharacterStatus __instance, ref BaseStatus _baseStatus,
        ref BaseStatus _curStatus, CharacterType charType)
    {
        if (!Plugin.HalveToolStaminaUsage.Value) return;
        if (charType != CharacterType.PLAYER) return;
        StaminaUsageCounter = 0;
        Plugin.L($"ResetStatus Called. Resetting _hitCounter to 0.");
    }


    private static void SetTool(WeaponTypeEnum type)
    {
        var tool = ToolsDataList.Find(a => a.WeaponType == type);
        PlayerCharacter.Instance.SetSelectedTools(tool);
        ToolsHud.toolIcon.SetSprite(tool.IconName);
        ToolsHud.ToolsHUDUpdate();
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(CharacterCollider), nameof(CharacterCollider.OnTriggerEnter2D))]
    public static void CharacterCollider_OnTriggerEnter2D(Collider2D col)
    {
        if (!Plugin.AutoChangeTool.Value) return;


        ResourcesObject resource = null;
        if (col.gameObject.GetComponent<ResourcesObject>() != null)
        {
            resource = col.gameObject.GetComponent<ResourcesObject>();
        }

        if (resource == null) return;
        

        if (resource.RESOURCES_ID.ToString().Contains(Plant))
        {
            SetTool(WeaponTypeEnum.SICKLE);
        }


        if (resource.RESOURCES_ID.ToString().Contains(Tree))
        {
            SetTool(WeaponTypeEnum.AXE);
        }


        if (resource.RESOURCES_ID.ToString().Contains(Stone) || resource.RESOURCES_ID.ToString().Contains(Rock))
        {
            SetTool(WeaponTypeEnum.HAMMER);
        }
    }


    [HarmonyPostfix]
    [HarmonyPatch(typeof(ToolsHUDUI), nameof(ToolsHUDUI.Init))]
    public static void ToolsHUDUI_Init(ref ToolsHUDUI __instance)
    {
        ToolsDataList = __instance.toolsDataList;
        ToolsHud = __instance;
    }

    //half energy use if greater than 1
    [HarmonyPrefix]
    [HarmonyPatch(typeof(CharacterStatus), nameof(CharacterStatus.UseTools))]
    public static bool CharacterStatus_UseTools_Prefix(ref CharacterStatus __instance)
    {
        return !Plugin.HalveToolStaminaUsage.Value;
    }


    [HarmonyPostfix]
    [HarmonyPatch(typeof(CharacterStatus), nameof(CharacterStatus.UseTools))]
    public static void CharacterStatus_UseTools_Postfix(ref CharacterStatus __instance)
    {
        if (!Plugin.HalveToolStaminaUsage.Value) return;
        StaminaUsageCounter++;
        if (StaminaUsageCounter != 2) return;
        StaminaUsageCounter = 0;
        Plugin.L($"Take Energy! Counter: {StaminaUsageCounter}");
        var staminaLoss = -__instance.GetStatusTools().Stamina;
        var newStamina = __instance.currentstatus.Stamina + staminaLoss;

        if (newStamina > __instance.GetBaseStatus.Stamina)
        {
            var isStaminaLossNegative = staminaLoss < 0;
            staminaLoss = newStamina - __instance.GetBaseStatus.Stamina;
            staminaLoss = (isStaminaLossNegative ? (staminaLoss + staminaLoss) : (staminaLoss - staminaLoss));
            newStamina = __instance.GetBaseStatus.Stamina;
        }

        __instance.currentstatus.Stamina = newStamina;
        __instance.player.EnergySpeed = 100f;
        if (__instance.GetStaminaPercent <= 30f)
        {
            UIManager.TUTORIAL_UI.Call(TutorialID.STAMINA_SYSTEM);
        }

        if (UIManager.GAME_HUD != null)
        {
            UIManager.GAME_HUD.GetStaminaBarHUD.OnValueChange(staminaLoss);
        }
    }
}
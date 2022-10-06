using System.Collections.Generic;
using CharacterIDEnum;
using GlobalEnum;
using HarmonyLib;
using TutorialEnum;

namespace AnAlchemicalCollection;

[HarmonyPatch]
public static class ToolPatches
{
    private static List<ToolsData> _toolsDataList;
    private static ToolsHUDUI _toolsHud;

    private static int _hitCounter;

    [HarmonyPrefix]
    [HarmonyPatch(typeof(CharacterStatus), nameof(CharacterStatus.SetStatus))]
    public static void CharacterStatus_SetStatus(ref CharacterStatus __instance, ref BaseStatus _baseStatus, ref BaseStatus _curStatus, CharacterType charType)
    {
        if (!Plugin.HalveToolStaminaUsage.Value) return;
        if (charType != CharacterType.PLAYER) return;
        _hitCounter = 0;
        Plugin.L($"ResetStatus Called. Resetting _hitCounter to 0.");
    }


    private static void SetTool(WeaponTypeEnum type)
    {
        var tool = _toolsDataList.Find(a => a.WeaponType == type);
        PlayerCharacter.Instance.SetSelectedTools(tool);
        _toolsHud.toolIcon.SetSprite(tool.IconName);
        _toolsHud.ToolsHUDUpdate();
    }


   
    [HarmonyPrefix]
    [HarmonyPatch(typeof(BattleCalculator), nameof(BattleCalculator.Calculator))]
    public static void BattleCalculator_Calculator(CharacterType typeC, PlayerCharacter player, UnityEngine.Object obj)
    {
        if (!Plugin.AutoChangeTool.Value) return;
        if (typeC == CharacterType.RESOURCES)
        {
            var resource = (ResourcesObject) obj;
            Plugin.L($"BattleCalculator: ResourceID {resource.RESOURCES_ID}");
    
    
            if (resource.RESOURCES_ID.ToString().Contains("PLANT"))
            {
                SetTool(WeaponTypeEnum.SICKLE);
            }
    
    
            if (resource.RESOURCES_ID.ToString().Contains("TREE"))
            {
                SetTool(WeaponTypeEnum.AXE);
            }
    
    
            if (resource.RESOURCES_ID.ToString().Contains("STONE") || resource.RESOURCES_ID.ToString().Contains("ROCK"))
            {
                SetTool(WeaponTypeEnum.HAMMER);
            }
        }
    }
    

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ToolsHUDUI), nameof(ToolsHUDUI.Init))]
    public static void ToolsHUDUI_Init(ref ToolsHUDUI __instance)
    {
        _toolsDataList = __instance.toolsDataList;
        _toolsHud = __instance;
    }

    //half energy use if greater than 1
    [HarmonyPrefix]
    [HarmonyPatch(typeof(CharacterStatus), nameof(CharacterStatus.UseTools))]
    public static bool CharacterStatus_UseTools_Prefix(ref CharacterStatus __instance)
    {
        if (!Plugin.HalveToolStaminaUsage.Value) return true;
        return false;
    }


    [HarmonyPostfix]
    [HarmonyPatch(typeof(CharacterStatus), nameof(CharacterStatus.UseTools))]
    public static void CharacterStatus_UseTools_Postfix(ref CharacterStatus __instance)
    {
        if (!Plugin.HalveToolStaminaUsage.Value) return;
        _hitCounter++;
        if (_hitCounter != 2) return;
        _hitCounter = 0;
        Plugin.L($"Take Energy! Counter: {_hitCounter}");
        var num = -__instance.GetStatusTools().Stamina;
        var num2 = num;
        var num3 = __instance.currentstatus.Stamina + num;
        if (num3 > __instance.GetBaseStatus.Stamina)
        {
            var flag = num < 0;
            num2 = num3 - __instance.GetBaseStatus.Stamina;
            num2 = (flag ? (num + num2) : (num - num2));
            num3 = __instance.GetBaseStatus.Stamina;
        }

        __instance.currentstatus.Stamina = num3;
        __instance.player.EnergySpeed = 100f;
        if (__instance.GetStaminaPercent <= 30f)
        {
            UIManager.TUTORIAL_UI.Call(TutorialID.STAMINA_SYSTEM);
        }

        if (UIManager.GAME_HUD != null)
        {
            UIManager.GAME_HUD.GetStaminaBarHUD.OnValueChange(num2);
        }
    }
}
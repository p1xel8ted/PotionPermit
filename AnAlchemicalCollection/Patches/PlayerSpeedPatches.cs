using GlobalEnum;
using HarmonyLib;

namespace AnAlchemicalCollection;

[HarmonyPatch]
public static class PlayerSpeedPatches
{
    private const float OriginalPlayerSpeed = 150f;
    //private const float OriginalDogSpeed = 300f;

    //player run speed
    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerCharacter), nameof(PlayerCharacter.Start))]
    [HarmonyPatch(typeof(PlayerCharacter), nameof(PlayerCharacter.Move))]
    public static void PlayerCharacter_Move()
    {
        if (!Plugin.EnableRunSpeedMultiplier.Value) return;

        if (PlayerCharacter.Instance.CurrentDirection is Direction.None) return;

        if (PlayerCharacter.Instance.CurrentDirection is Direction.Top or Direction.Bottom)
        {
            PlayerCharacter.Instance.MoveSpeed = OriginalPlayerSpeed * Plugin.RunSpeedMultiplier.Value;
            // Plugin.L($"Running Up/Down: Speed: {PlayerCharacter.Instance.MoveSpeed}");
            return;
        }

        if (PlayerCharacter.Instance.CurrentDirection is Direction.Left or Direction.Right)
        {
            PlayerCharacter.Instance.MoveSpeed = (OriginalPlayerSpeed * Plugin.RunSpeedMultiplier.Value) *
                                                 Plugin.LeftRightRunSpeedMultiplier.Value;
            // Plugin.L($"Running Left/Right: Speed: {PlayerCharacter.Instance.MoveSpeed}");
            return;
        }

        Plugin.L($"Unknown direction: {PlayerCharacter.Instance.CurrentDirection},  Speed: {PlayerCharacter.Instance.MoveSpeed}");
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

    

   






}
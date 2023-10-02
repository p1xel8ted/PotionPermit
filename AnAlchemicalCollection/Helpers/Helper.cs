﻿using FastTravelEnum;
using GlobalEnum;
using NotificationEnum;

namespace AnAlchemicalCollection;

public static class Helper
{
    public static float CalculateLowestMultiplierAbove50(float refreshRate)
    {
        var fps = refreshRate;
        while (fps > 50)
        {
            fps /= 2;
        }
        return fps * 2;
    }

    public static void ShowNotification(string mainText, string smallText)
    {
        var comp = UIManager.NOTIFICATION_UI_MANAGER;
        var getInactiveEtcNotificationUI = comp.GetInactiveEtcNotificationUI;
        getInactiveEtcNotificationUI.Set(NotificationID.RESEARCH, comp.GetNotificationLayer);
        getInactiveEtcNotificationUI.notificationText.text = mainText;
        getInactiveEtcNotificationUI.newText.text = smallText;
        getInactiveEtcNotificationUI.Call();
        comp.notificationOnQueueList.Add(getInactiveEtcNotificationUI);
    }

    public static void Teleport(FastTravelID id, MapRegion region)
    {
        var vector = WorldMapManager.GET_FAST_TRAVEL_POSITION(id);
        PlayerCharacter.Instance.SetPosition(vector);
        RegionDataManager.SET_NEW_REGION(region, true);

        WeatherManager.CHANGE_WEATHER_REGION(RegionDataManager.SelectedMapRegion, region);
        Chemist_SFX.PLAY_SFX_UI(UIManager.WORLD_MAP_UI.teleportSFX);
        RegionDataManager.RESET_CHUNK();
        RegionDataManager.REFRESH_FIELD_ID(vector);
        DogieManager.Instance.SHOW_DOGIE(vector);
    }
}
using NotificationEnum;

namespace AnAlchemicalCollection;

public static class Helper
{
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
}
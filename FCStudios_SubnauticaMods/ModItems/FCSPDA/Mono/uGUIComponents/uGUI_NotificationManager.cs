using System.Collections.Generic;
using FCS_AlterraHub.ModItems.Buildables.BaseManager.Mono;
using UnityEngine;

namespace FCS_AlterraHub.ModItems.FCSPDA.Mono.uGUIComponents;
internal class uGUI_NotificationManager : MonoBehaviour
{
    public static uGUI_NotificationManager Instance;
    [SerializeField] private Transform notificationTemplate;
    [SerializeField] private Transform grid;
    [SerializeField] private int notificationMessageMax = 3;

    private Queue<string> pendingMessages = new();
    private int _currentMessagesCount;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        uGUI_BaseManager.Instance.OnUGUIBaseManagerOpened += UguiBaseManager_OnUGUIBaseManagerOpened;
    }

    private void UguiBaseManager_OnUGUIBaseManagerOpened(object sender, System.EventArgs e)
    {

    }

    public void AddNotification(string message)
    {
        if (_currentMessagesCount == notificationMessageMax)
        {
            pendingMessages.Enqueue(message);
            return;
        }

        CreateNotification(message);
    }

    private void CreateNotification(string message)
    {
        var template = Instantiate(notificationTemplate, grid);
        var notification = template.GetComponent<uGUI_Notification>();
        notification.SetMessage(message);
        notification.OnDeleted += Notification_OnDeleted;
        _currentMessagesCount++;
    }

    private void Notification_OnDeleted(object sender, uGUI_Notification.OnDeletedArg e)
    {
        _currentMessagesCount--;

        Destroy(e.notification.gameObject);

        if (pendingMessages.Count == 0) return;

        if (_currentMessagesCount < notificationMessageMax)
        {
            var amountToAdd = notificationMessageMax - _currentMessagesCount;

            for (int i = 0; i < amountToAdd; i++)
            {
                var message = pendingMessages.Dequeue();
                AddNotification(message);
            }
        }
    }
}

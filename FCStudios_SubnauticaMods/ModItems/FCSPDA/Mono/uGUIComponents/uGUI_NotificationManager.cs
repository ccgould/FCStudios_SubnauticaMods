using System;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Models;
using FCS_AlterraHub.Models.Interfaces;
using FCS_AlterraHub.ModItems.Buildables.BaseManager.Mono;
using FCS_AlterraHub.ModItems.FCSPDA.ScriptableObjects;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_AlterraHub.ModItems.FCSPDA.Mono.uGUIComponents;
internal class uGUI_NotificationManager : MonoBehaviour
{
    public static uGUI_NotificationManager Instance;
    [SerializeField] private Transform notificationTemplate;
    [SerializeField] private Transform grid;
    [SerializeField] private int notificationMessageMax = 3;

    private Queue<string> pendingMessages = new();
    private List<GameObject> currentMessagesGO = new();
    private int _currentMessagesCount;

    private void Awake()
    {
        Instance = this;
    }

    public void AddNotification(string message = null)
    {
        if (pendingMessages.Any(x => x == message)) return;

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
        pendingMessages.Enqueue(message);
        currentMessagesGO.Add(template.gameObject);
    }

    private void Notification_OnDeleted(object sender, uGUI_Notification.OnDeletedArg e)
    {
        _currentMessagesCount--;

        currentMessagesGO.Remove(e.notification.gameObject);

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
        
    internal void PurgeData()
    {
        foreach (var go in currentMessagesGO)
        {
            Destroy(go);
        }

        currentMessagesGO.Clear();
        pendingMessages.Clear();
        _currentMessagesCount = 0;
    }
}

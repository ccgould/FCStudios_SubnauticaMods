using FCS_AlterraHub.Core.Helpers;
using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.Models.Interfaces;
using FCS_AlterraHub.ModItems.FCSPDA.Enums;
using FCSCommon.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FCS_AlterraHub.Models.Mono.Handlers;

internal class MessageBoxHandler : MonoBehaviour
{
    private Action<FCSMessageResult> _result;
    [SerializeField]
    private FCSMessageBox _messageBox;
    [SerializeField]
    private IFCSAlterraHubGUI _gui;
    private readonly Queue<MessageBoxData> _messageQueue = new Queue<MessageBoxData>();
    private static Dictionary<FCSAlterraHubGUISender, IFCSAlterraHubGUI> _registeredGUI = new();

    private void TestMessagQueue()
    {
        _messageQueue.Enqueue(new MessageBoxData("Test", FCSMessageButton.OK, null));
    }

    public void Initialize(GameObject go, FCSAlterraHubGUISender sender)
    {       
        if (_gui is not null && !_registeredGUI.ContainsValue(_gui))
        {
            _registeredGUI.Add(sender, _gui);
        }
    }

    internal void Show(string message, FCSMessageButton button, Action<FCSMessageResult> result = null)
    {
        if (_messageBox == null)
        {
            QuickLogger.ModMessage(LanguageService.ErrorHasOccurred("0x0003"));
            QuickLogger.Error("MessageBox returned null");
            return;
        }

        if (_messageBox.IsVisible())
        {
            _messageQueue.Enqueue(new MessageBoxData(message, button, result));
            return;
        }

        _result = result;
        _messageBox.Show(message, button, OnMessageResult);
    }

    private void OnMessageResult(FCSMessageResult result)
    {
        if (_messageQueue.Any())
        {
            var data = _messageQueue.Dequeue();
            _result = data.Result;
            _messageBox.Show(data.Message, data.Button, OnMessageResult);
        }
    }

    private struct MessageBoxData
    {
        public MessageBoxData(string message, FCSMessageButton button, Action<FCSMessageResult> result)
        {
            Message = message;
            Button = button;
            Result = result;
        }

        public Action<FCSMessageResult> Result { get; }

        public FCSMessageButton Button { get; }

        public string Message { get; }
    }

    public static void ShowMessage(string message, FCSAlterraHubGUISender sender)
    {
        var guis = _registeredGUI.Where(x => x.Key == sender);
        foreach (var gui in guis)
        {
            gui.Value.ShowMessage(message);
        }
    }
}

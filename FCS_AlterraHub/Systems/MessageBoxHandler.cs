using System;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Managers.FCSAlterraHub;
using FCS_AlterraHub.Model;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_AlterraHub.Systems
{
    internal class MessageBoxHandler : MonoBehaviour
    {
        private Action<FCSMessageResult> _result;
        private FCSMessageBox _messageBox;
        private GameObject _objectRoot;
        private readonly Queue<MessageBoxData> _messageQueue = new Queue<MessageBoxData>();
        private static Dictionary<FCSAlterraHubGUISender,FCSAlterraHubGUI> _registeredGUI = new();

        private void TestMessagQueue()
        {
            _messageQueue.Enqueue(new MessageBoxData("Test",FCSMessageButton.OK,null));
        }

        public GameObject ObjectRoot
        {
            get => _objectRoot;
            set
            {
                _objectRoot = value;
                if (_messageBox == null)
                {
                    _messageBox = GameObjectHelpers.FindGameObject(ObjectRoot, "MessageBox").AddComponent<FCSMessageBox>();
                }
            }
        }

        public void Initialize(GameObject go, FCSAlterraHubGUISender sender)
        {
            var gui = ObjectRoot.GetComponentInChildren<FCSAlterraHubGUI>();
            if (gui is not null && !_registeredGUI.ContainsValue(gui))
            {
                _registeredGUI.Add(sender,gui);
            }
        }

        internal void Show(string message,FCSMessageButton button, Action<FCSMessageResult> result = null)
        {
            if (_messageBox == null)
            {
                QuickLogger.ModMessage(AlterraHub.ErrorHasOccurred("0x0003"));
                QuickLogger.Error("MessageBox returned null");
                return;
            }

            if (_messageBox.IsVisible())
            {
                _messageQueue.Enqueue(new MessageBoxData(message, button, result));
                return;
            }

            _result = result;
            _messageBox.Show(message,button,OnMessageResult);
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
           var guis =  _registeredGUI.Where(x => x.Key == sender);
           foreach (var gui in guis)
           {
               gui.Value.ShowMessage(message);
           }
        }
    }
}

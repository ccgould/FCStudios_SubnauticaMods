using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.Models.Abstract;
using FCS_AlterraHub.Models.Structs;
using FCS_AlterraHub.ModItems.FCSPDA.Enums;
using FCS_AlterraHub.ModItems.FCSPDA.Mono.Model;
using Steamworks;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UWE.Utils;

namespace FCS_AlterraHub.ModItems.FCSPDA.Mono.Dialogs
{
    internal class PDADeviceSettingsPage : Page
    {
        private Toggle _visiblityToggle;
        private TMP_InputField _friendlyNameInput;
        private FCSAlterraHubGUI _gui;
        private FCSDevice _device;

        private void Awake()
        {
            _gui = FCSPDAController.Main.GetGUI();
            _visiblityToggle = gameObject.GetComponentInChildren<Toggle>();
            _visiblityToggle.onValueChanged.AddListener((b) =>
            {
                _device.IsVisibleInPDA = b;
            });

            _friendlyNameInput = gameObject.GetComponentInChildren<TMP_InputField>();
            _friendlyNameInput.onValueChanged.AddListener((t) =>
            {
                _device.FriendlyName = t;
            });

            var backButton = gameObject.GetComponentInChildren<Button>();
            backButton.onClick.AddListener(() =>
            {
                _gui.GoToPage(PDAPages.None);
            });
        }

        public override void Enter(object arg = null)
        {
            base.Enter(arg);
            _device = (FCSDevice)arg;
            _visiblityToggle?.SetIsOnWithoutNotify(_device.IsVisibleInPDA);
            _friendlyNameInput.SetTextWithoutNotify(_device.GetDeviceName());
        }
    }
}
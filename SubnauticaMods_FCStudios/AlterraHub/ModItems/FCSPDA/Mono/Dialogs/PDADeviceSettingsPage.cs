using FCS_AlterraHub.Models.Abstract;
using FCS_AlterraHub.Models.Structs;
using FCS_AlterraHub.ModItems.FCSPDA.Enums;
using Steamworks;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_AlterraHub.ModItems.FCSPDA.Mono.Dialogs
{
    internal class PDADeviceSettingsPage : MonoBehaviour
    {
        private Toggle _visiblityToggle;
        private TMP_InputField _friendlyNameInput;
        private string _uiID;
        private FCSAlterraHubGUI _gui;
        private FCSDevice _device;

        private void Awake()
        {
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
                _gui.GoToPage(PDAPages.DevicePage,new Tuple<string,FCSDevice>(_uiID,_device));
            });
        }

        internal void Show(FCSAlterraHubGUI gui,string uiID, FCSDevice args)
        {
            _uiID = uiID;
            _gui = gui;
            _device = args;
            gameObject.SetActive(true);
            _visiblityToggle?.SetIsOnWithoutNotify(args.IsVisibleInPDA);
            _friendlyNameInput.SetTextWithoutNotify(args.GetDeviceName());
        }

        internal void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
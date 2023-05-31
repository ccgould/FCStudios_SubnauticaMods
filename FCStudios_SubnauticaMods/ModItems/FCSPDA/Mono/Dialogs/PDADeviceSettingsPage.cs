﻿using FCS_AlterraHub.Models.Abstract;
using FCS_AlterraHub.ModItems.FCSPDA.Enums;
using FCS_AlterraHub.ModItems.FCSPDA.Mono.Model;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_AlterraHub.ModItems.FCSPDA.Mono.Dialogs;

internal class PDADeviceSettingsPage : Page
{
    [SerializeField]
    private Toggle _visiblityToggle;
    [SerializeField]
    private uGUI_InputField _friendlyNameInput;
    [SerializeField]
    private FCSAlterraHubGUI _gui;
    private FCSDevice _device;

    public override PDAPages PageType => PDAPages.DeviceSettings;

    public void OnFriendlyNameChanged(string t)
    {
        _device.FriendlyName = t;
    }

    public void OnVisiblityToggleChanged(bool b)
    {
        _device.IsVisibleInPDA = b;
    }

    public override void Enter(object arg = null)
    {
        base.Enter(arg);

        if (arg is null) return;
        
        _device = (FCSDevice)arg;

        if (_device is null) return;

        _visiblityToggle?.SetIsOnWithoutNotify(_device.IsVisibleInPDA);
        _friendlyNameInput.SetTextWithoutNotify(_device.GetDeviceName());
    }

    public override void OnBackButtonClicked()
    {
        _gui.GoBackAPage();
    }
}
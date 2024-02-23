using FCS_AlterraHub.Core.Navigation;
using FCS_AlterraHub.Models.Interfaces;
using FCS_AlterraHub.ModItems.FCSPDA.Interfaces;
using FCS_EnergySolutions.ModItems.Buildables.TelepowerPylon.Enumerators;
using FCSCommon.Utilities;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_EnergySolutions.ModItems.Buildables.TelepowerPylon.Mono.UGUI;

internal class uGUI_TelepowerPylonHomePage : Page, IuGUIAdditionalPage
{
    [SerializeField] private Button _doneBTN;
    [SerializeField] private Toggle _pullToggle;
    [SerializeField] private Toggle _pushToggle;
    //[SerializeField] private GameObject _pushGrid;
    //[SerializeField] private GameObject _pullGridContent;
    //[SerializeField] private GameObject _pushGridContent;
    [SerializeField] private ToggleGroup toggleGroup;
    private TelepowerPylonController _sender;


    public override void Enter(object arg = null)
    {
        base.Enter(arg);

        var sender = arg as TelepowerPylonController;

        if (sender is not null)
        {
            _sender = sender;
            ResetUI();
        }
    }

    private void ResetUI()
    {
        if (!_sender.GetTelepowerBaseManager()?.GetHasBeenSet() ?? false)
        {
            _pullToggle.SetIsOnWithoutNotify(false);
            _pushToggle.SetIsOnWithoutNotify(false);
            _doneBTN.interactable = false;
        }
    }

    public void SetSendToMode(bool v)
    {
        QuickLogger.Debug($"SetSendToMode: ({v})", true);
        if (v)
        {
            _sender.SetMode(TelepowerPylonMode.PUSH);
        }

        SetDoneButtonState();
    }

    private void SetDoneButtonState()
    {
        _doneBTN.interactable = toggleGroup.AnyTogglesOn();

        if (!toggleGroup.AnyTogglesOn())
        {
            _sender.SetMode(TelepowerPylonMode.NONE);
        }

        //if(!_pullToggle.isOn && !_pushToggle.isOn)
        //{
        //    _doneBTN.interactable = false;
        //    _sender.SetMode(TelepowerPylonMode.NONE);

        //}
        //else
        //{
        //    _doneBTN.interactable = true;
        //}
    }

    public void SetRecieveFromMode(bool v)
    {
        QuickLogger.Debug($"SetRecieveFromMode: ({v})", true);

        if (v)
        {
            _sender.SetMode(TelepowerPylonMode.PULL);
        }

        SetDoneButtonState();
    }

    public IFCSObject GetController()
    {
        return _sender;
    }



    //private void InitializeIPC()
    //{
    //    //IPCMessage += message =>
    //    //{
    //    //    if (message.Equals("UpdateEffects"))
    //    //    {
    //    //        ChangeTrailColor();
    //    //    }
    //    //};

    //    //IPCMessage += message =>
    //    //{
    //    //    if (message.Equals("PylonBuilt") || message.Equals("PylonDestroy") || message.Equals("PylonModeSwitch"))
    //    //    {
    //    //        RefreshUI();
    //    //        GoToPage(_baseTelepowerPylonManager.GetCurrentMode());
    //    //    }
    //    //};
    //}

}
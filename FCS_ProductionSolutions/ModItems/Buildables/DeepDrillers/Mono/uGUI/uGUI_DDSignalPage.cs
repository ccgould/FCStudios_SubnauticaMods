using FCS_AlterraHub.Core.Navigation;
using FCS_AlterraHub.Core.Services;
using FCS_ProductionSolutions.ModItems.Buildables.DeepDrillers.Mono.Base;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_ProductionSolutions.ModItems.Buildables.DeepDrillers.Mono.uGUI;
internal class uGUI_DDSignalPage : Page
{
    private DrillSystem _sender;
    [SerializeField] private Text _beaconName;
    [SerializeField] private Toggle _beaconToggle;

    public override void Enter(object arg = null)
    {
        base.Enter(arg);

        if (arg is not null)
        {
            _sender = arg as DrillSystem;
            SetBeaconName(_sender.GetPingName());
            _beaconToggle.SetIsOnWithoutNotify(_sender.GetBeaconState());
        }
    }

    private void SetBeaconName(string v)
    {
        _beaconName.text = v;
    }

    public void OnSignalStateChanged(bool value)
    {
        _sender.SetBeaconState(value);
    }

    public void OnRenameDrillSignal()
    {
        global::uGUI.main.userInput.RequestString(LanguageService.ChangeBaseName(), LanguageService.Submit(), _sender.GetPingName(), 20, (s) =>
        {
            _sender.SetPingName(s);
            SetBeaconName(s);
        });
    }
    
}
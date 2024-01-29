using FCS_AlterraHub.Core.Navigation;
using FCS_AlterraHub.Models.Interfaces;
using FCS_AlterraHub.ModItems.FCSPDA.Interfaces;
using FCS_ProductionSolutions.Configuration;
using FCSCommon.Utilities;
using System;
using UnityEngine;
using UnityEngine.UI;


namespace FCS_ProductionSolutions.ModItems.Buildables.HydroponicHarvester.Mono.uGUI;
internal class uGUI_HydroponicHarvester : Page, IuGUIAdditionalPage
{
    private HydroponicHarvesterController _sender;
    private MenuController _menuController;
    [SerializeField] private uGUI_HarvesterSpeedSwitch _harvesterSpeedSwitch;
    [SerializeField] private Text _titleText;
    [SerializeField] private Text _powerUsagePerSecondText;
    [SerializeField] private Text _unitPerSecondText;
    [SerializeField] private uGUI_HarvesterSlot[] _uGUIHarvesterSlots;
    private Toggle _lightToggle;

    public override void Awake()
    {
        base.Awake();
        _lightToggle = GetComponentInChildren<Toggle>();
        _titleText.text = Language.main.Get("PS_CurrentSamples");
    }

    public IFCSObject GetController()
    {
        return _sender;
    }

    public override void Enter(object obj)
    {
        base.Enter(obj);

        if (_menuController is null)
        {
            _menuController = FCSPDAController.Main.GetGUI().GetMenuController();
        }

        if (obj is not null)
        {
            _sender = obj as HydroponicHarvesterController;
        }


        if (_sender is not null)
        {
            FCSPDAController.Main.GetGUI().GetNavigationController().SetErrorButtonDevice(_sender);
            _harvesterSpeedSwitch.SetSpeedMode(_sender.GetCurrentSpeedMode());
            _lightToggle.SetIsOnWithoutNotify(_sender.GetLightState());
            LinkSlots();
            UpdateUI();
        }
    }

    private void LinkSlots()
    {
        QuickLogger.Debug("LinkSlots");
        for (int i = 0; i < _uGUIHarvesterSlots.Length; i++)
        {
            QuickLogger.Debug($"Slot {i}");

            var slot = _sender.GetPlantSlot(i);

            QuickLogger.Debug($"IS Slot Null {slot is null}");

            if (slot is null) continue;

            _uGUIHarvesterSlots[i].SetSlot(slot);
        }
    }

    internal void UpdateUI()
    {
        _powerUsagePerSecondText.text = AuxPatchers.PowerUsagePerSecondFormat(_sender.GetPowerUsage());
        _unitPerSecondText.text = AuxPatchers.GenerationTimeFormat(Convert.ToSingle(_sender.GetCurrentSpeedMode()));
        //_unitID.text = $"UNIT ID: {_mono.UnitID}";
    }

    public void PushPage(Page page)
    {
        _menuController.PushPage(page, _sender);
    }

    public void PopPage()
    {
        _menuController.PopAndPeek();
    }

    public override void Exit()
    {
        base.Exit();
        Purge();
    }

    private void Purge()
    {
        for (int i = 0; i < _uGUIHarvesterSlots.Length; i++)
        {
            _uGUIHarvesterSlots[i].Purge();
        }
    }

    public void OnChangeSpeed()
    {
        _sender.SetHarvesterSpeed(_harvesterSpeedSwitch.GetMode());
        UpdateUI();
    }

    public void ToggleLight(bool isLightOn)
    {
        _sender.SetLightState(isLightOn);
    }
}

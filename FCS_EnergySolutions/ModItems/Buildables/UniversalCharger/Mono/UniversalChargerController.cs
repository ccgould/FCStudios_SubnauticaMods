using FCS_AlterraHub.Models.Abstract;
using FCS_AlterraHub.Models.Interfaces;
using FCS_AlterraHub.Models.Mono;
using FCS_EnergySolutions.Configuration;
using FCS_EnergySolutions.ModItems.Buildables.UniversalCharger.Enumerators;
using FCSCommon.Utilities;
using System;
using UnityEngine;
using UnityEngine.UI;
using static FCS_EnergySolutions.Configuration.SaveData;

namespace FCS_EnergySolutions.ModItems.Buildables.UniversalCharger.Mono;
internal class UniversalChargerController : FCSDevice, IFCSSave<SaveData>
{
    [SerializeField] private HoverInteraction hoverInteraction;
    [SerializeField] private Toggle[] toggles;
    [SerializeField] private Text clickToAddBatteriesLbl;
    [SerializeField] private Text currentModeLbl;
    [SerializeField] private PowercellCharger charger;


    public override void Awake()
    {
        base.Awake();
        //charger.Initialize();
        clickToAddBatteriesLbl.text = Language.main.Get("AES_ClickToAddBatteries");
        currentModeLbl.text = Language.main.Get("AES_CurrentMode");
        hoverInteraction.onSettingsWDataKeyPressed += HoverInteraction_onSettingsKeyPressed;
        hoverInteraction.OnKeyDown += HoverInteraction_onKeyPressed;
    }

    private void HoverInteraction_onKeyPressed()
    {
        if(Input.GetKeyDown(Plugin.Configuration.UniversalChargeModeKey))
        {
            if (charger.HasChargables())
            {
                QuickLogger.ModMessage(Language.main.Get("AES_CannotSwitchModes"));
                return;
            }
            charger.ToggleMode();
        }
    }

    internal void UpdateUIIToggles(PowerChargerMode mode)
    {
        switch (mode)
        {
            case PowerChargerMode.Powercell:
                toggles[0].isOn = true;
                break;
            case PowerChargerMode.Battery:
                toggles[1].isOn = true;
                break;
        }
    }

    public override void OnEnable()
    {
        if (_runStartUpOnEnable)
        {
            if (!IsInitialized)
            {
                Initialize();
            }

            if (IsFromSave)
            {
                if (_savedData == null)
                {
                    ReadySaveData();
                }

                //_colorManager.LoadTemplate(_savedData.ColorTemplate);
                LoadChargerFromSave();
                
                _runStartUpOnEnable = false;
            }
        }
    }

    private void LoadChargerFromSave()
    {
        var save = _savedData as UniversalChargerDataEntry;

        if (save.Mode == PowerChargerMode.Powercell)
        {
            toggles[0].SetIsOnWithoutNotify(true);
            toggles[1].SetIsOnWithoutNotify(false);
        }
        else
        {
            toggles[0].SetIsOnWithoutNotify(false);
            toggles[1].SetIsOnWithoutNotify(true);
        }

        charger.SetMode(save.Mode);
        charger.Load(save.ChargerData);
    }

    public override void Start()
    {
        base.Start();

        //for (int i = 0; i < uiBatteries.Length; i++)
        //{
        //    var meter = uiBatteries[i];
        //    meter.UpdateStateByPercentage(i/uiBatteries.Length);
        //}
    }

    private void HoverInteraction_onSettingsKeyPressed(TechType obj, HandTargetEventData eventData)
    {
        charger.OnHandClick(eventData);
    }

    public override void ReadySaveData()
    {
        _savedData = ModSaveManager.GetSaveData<UniversalChargerDataEntry>(GetPrefabID());
    }

    public void Save(SaveData newSaveData, ProtobufSerializer serializer)
    {
        QuickLogger.Debug("Saving Server Rack", true);

        if (!IsInitialized || !IsConstructed) return;

        if (_savedData == null)
        {
            _savedData = new UniversalChargerDataEntry();
        }

        var save = _savedData as UniversalChargerDataEntry;

        save.Id = GetPrefabID();

        QuickLogger.Debug($"Saving ID {save.Id}", true);
        //save.ColorTemplate = _colorManager.SaveTemplate();
        save.ChargerData = charger.Save();
        save.Mode = charger.GetMode();
        newSaveData.Data.Add(_savedData);
    }

    public override string[] GetDeviceStats()
    {
        string[] stats = new string[]
        {
            $"{Language.main.GetFormat("AES_UCPressToChangeMode",Plugin.Configuration.UniversalChargeModeKey)}",
            $"Current Mode: {charger.GetMode()}",
            $"EPM: {charger.chargeSpeed}"
        };
        return stats;
    }
}

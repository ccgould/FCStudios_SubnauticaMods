using FCS_AlterraHub.Core.Components;
using FCS_AlterraHub.Models.Interfaces;
using FCS_ProductionSolutions.Configuration;
using FCS_ProductionSolutions.ModItems.Buildables.DeepDrillers.Mono.Base;
using FCSCommon.Utilities;
using System;
using UnityEngine;
using static FCS_ProductionSolutions.Configuration.SaveData;

namespace FCS_ProductionSolutions.ModItems.Buildables.DeepDrillers.Mono;

internal class DeepDrillerLightDuty : DrillSystem, IFCSSave<SaveData>
{
    internal override bool UseOnScreenUi => throw new NotImplementedException();
    
    [SerializeField] private DeepDrillerLightDutyPowerManager powerManager;

    public override void Awake()
    {
        base.Awake();

        var powerRelay = gameObject.GetComponent<PowerRelay>();
        PowerManager = powerManager;
        powerManager.SetPowerRelay(powerRelay);
        powerManager.Initialize(this);
    }

    public override void ReadySaveData()
    {
        string id = (base.GetComponentInParent<PrefabIdentifier>() ?? base.GetComponent<PrefabIdentifier>()).Id;
        _savedData = ModSaveManager.GetSaveData<DeepDrillerLightDutySaveDataEntry>(id);
        QuickLogger.Debug($"Prefab Id : {GetPrefabID()} || SaveData Is Null: {_savedData is null}");
    }

    internal override void LoadSave()
    {
        QuickLogger.Debug("Loading light duty save");

        if (IsFromSave && _savedData != null)
        {
            QuickLogger.Debug($"Is From Save: {GetPrefabID()}");

            if (_savedData == null)
            {
                ReadySaveData();
            }

            QuickLogger.Debug($"Is Save Data Present: {_savedData is not null}");

            if (_savedData is not null)
            {
                QuickLogger.Debug($"Setting Data");

                var save = _savedData as DeepDrillerLightDutySaveDataEntry;

                //_colorManager?.LoadTemplate(((ISaveDataEntry)_savedData).ColorTemplatepowerManager.LoadData(save);

                GetDDContainer().LoadData(save.Items);

                if (save.IsFocused)
                {
                    GetOreGenerator().SetIsFocus(save.IsFocused);
                    GetOreGenerator().Load(save.FocusOres);
                }

                GetOreGenerator().SetBlackListMode(save.IsBlackListMode);

                _colorManager.LoadTemplate(save.ColorTemplate);
                GetOilHandler().SetOilTimeLeft(save.OilTimeLeft);
                _isBreakSet = save.IsBrakeSet;
                if (!string.IsNullOrWhiteSpace(save.BeaconName))
                {
                    SetPingName(save.BeaconName);
                }
                ToggleVisibility(save.IsPingVisible);
            }
                      
            UpdateEmission();
        }
        QuickLogger.Debug("Loaded light duty save");

    }

    public override void Save(SaveData saveDataList, ProtobufSerializer serializer = null)
    {
        QuickLogger.Info("================================ Saving Drill ============================");
        
        if (!IsInitialized || !IsConstructed) return;

        if (_savedData == null)
        {
            _savedData = new DeepDrillerLightDutySaveDataEntry();
        }

        var save = _savedData as DeepDrillerLightDutySaveDataEntry;

        save.Id = GetPrefabID();
        save.ColorTemplate = _colorManager.SaveTemplate();

        save.PowerState = PowerManager.GetPowerState();
        save.PowerData = powerManager.SaveData();

        save.Items = GetDDContainer().SaveData();
        save.FocusOres = GetOreGenerator().GetFocusedOres();
        save.IsFocused = GetOreGenerator().GetIsFocused();
        save.IsBlackListMode = GetOreGenerator().GetInBlackListMode();
        save.IsBrakeSet = _isBreakSet;

        save.OilTimeLeft = GetOilHandler().GetOilTimeLeft();
        save.BeaconName = _ping.GetLabel();
        save.IsPingVisible = _ping.visible;
        saveDataList.Data.Add(save);

        QuickLogger.Info("================================ Saved Drill ============================");
    }
}
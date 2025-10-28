using FCS_AlterraHub.API;
using FCS_AlterraHub.Core.Interfaces;
using FCS_AlterraHub.Models.Abstract;
using FCS_AlterraHub.Models.Interfaces;
using FCS_LifeSupportSolutions.Configuration;
using FCSCommon.Utilities;
using System;
using UnityEngine;


namespace FCS_LifeSupportSolutions.ModItems.Buildables.MiniMedBay.Mono;
internal class MiniMedBayController : FCSDevice, IFCSSave
{
    private MiniMedBayBedManager _bedManager;
    private MiniMedBayContainer _container;

    [SerializeField] private MiniMedBayTrigger trigger;
    [SerializeField] private MiniMedBayDisplay _display;


    public override void Awake()
    {
        base.Awake();
        _bedManager = gameObject.GetComponent<MiniMedBayBedManager>();
        _container = gameObject.GetComponent<MiniMedBayContainer>();
    }

    public override void OnEnable()
    {
        _container.OnKitAmountChanged += MiniMedBayContainer_OnKitAmountChanged;

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

                var save = _savedData as MiniMedBayDataEntry;

                if (save != null)
                {
                    //_colorManager.LoadTemplate(_savedData.ColorTemplate);
                    _container.NumberOfFirstAids = save.FirstAidCount;
                    _container.SetTimeToSpawn(save.TimeToSpawn);
                }
            }

            _runStartUpOnEnable = false;
        }
    }

    private void OnDisable()
    {
        _container.OnKitAmountChanged -= MiniMedBayContainer_OnKitAmountChanged;
    }

    private void Update()
    {
        _display.UpdatePlayerHealthPercent(trigger.IsPlayerInTrigger() ? Mathf.CeilToInt(Player.main.liveMixin.health) : 0);
    }



    private void MiniMedBayContainer_OnKitAmountChanged(int value)
    {
        _display.UpdateDispenserCount(value);
    }

    public override void ReadySaveData()
    {
        _savedData = FCSModsAPI.PublicAPI.GetSaveData<MiniMedBayDataEntry>(GetPrefabID()); //ModSaveManager.GetSaveData<MiniMedBayDataEntry>(GetPrefabID());
    }

    public void SaveDevice()
    {
        QuickLogger.Debug("Saving Server Rack", true);

        if (!IsInitialized || !IsConstructed) return;

        if (_savedData == null)
        {
            _savedData = new MiniMedBayDataEntry();
        }

        var save = _savedData as MiniMedBayDataEntry;

        save.Id = GetPrefabID();

        QuickLogger.Debug($"Saving ID {save.Id}", true);
        //save.ColorTemplate = _colorManager.SaveTemplate();
        save.FirstAidCount = _container.NumberOfFirstAids;
        save.TimeToSpawn = _container.GetTimeToSpawn();
        //newSaveData.Data.Add(save);
        FCSModsAPI.PublicAPI.PushSaveData(save); 
    }
    public override float GetPowerUsage()
    {
        return _bedManager != null && _bedManager.IsHealing ? energyPerSecond : 0f; // 8.0f 
    }

    public override bool IsOperational()
    {
        if (CachedHabitatManager is not null)
        {
            return IsConstructed && CachedHabitatManager.GetPower() >= energyPerSecond;
        }
        return false;
    }
}

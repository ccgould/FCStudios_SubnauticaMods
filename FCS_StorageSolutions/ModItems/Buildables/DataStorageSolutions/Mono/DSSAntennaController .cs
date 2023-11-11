using FCS_AlterraHub.API;
using FCS_AlterraHub.Configuation;
using FCS_AlterraHub.Core.Components;
using FCS_AlterraHub.Models.Abstract;
using FCS_AlterraHub.Models.Interfaces;
using FCS_StorageSolutions.Configuation;
using FCS_StorageSolutions.Models;
using FCS_StorageSolutions.Services;
using FCSCommon.Utilities;
using System.Collections;
using UnityEngine;

namespace FCS_StorageSolutions.ModItems.Buildables.DataStorageSolutions.Mono;
internal class DSSAntennaController : FCSDevice, IFCSSave<SaveData>
{
    [SerializeField] private MotorHandler motorHandler;
    private DSSManager _dssManager;

    public override void Start()
    {
        base.Start();

        if (_runStartUpOnEnable)
        {
            if (!IsInitialized)
            {
                Initialize();
            }

            if (IsFromSave)
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

                    var savedData = _savedData as BaseSaveData;

                    _colorManager?.LoadTemplate(((ISaveDataEntry)_savedData).ColorTemplate);
                }
            }
            _runStartUpOnEnable = false;
        }

        StartCoroutine(AttemptConnection());

        motorHandler.StartMotor();
    }

    public override void ReadySaveData()
    {
        string id = (base.GetComponentInParent<PrefabIdentifier>() ?? base.GetComponent<PrefabIdentifier>()).Id;
        _savedData = ModSaveManager.GetSaveData<BaseSaveData>(id);
        QuickLogger.Debug($"Prefab Id : {GetPrefabID()} || SaveData Is Null: {_savedData is null}");
    }

    public void Save(SaveData newSaveData, ProtobufSerializer serializer = null)
    {
        QuickLogger.Debug("Saving DSS Antenna", true);

        if (!IsInitialized || !IsConstructed) return;

        if (_savedData == null)
        {
            _savedData = new BaseSaveData();
        }

        var save = _savedData as BaseSaveData;

        save.Id = GetPrefabID();
        save.BaseId = FCSModsAPI.PublicAPI.GetHabitat(this)?.GetBasePrefabID();
        save.ColorTemplate = _colorManager.SaveTemplate();

        newSaveData.Data.Add(save);
        QuickLogger.Debug($"Saves DSS Antenna {newSaveData.Data.Count}", true);
    }

    public override string[] GetDeviceStats()
    {
        return new string[]
        {
            $"[EPM: {energyPerSecond * 60:F2}] [Is Connected: {IsRegisteredToBaseManager()}]",
        };
    }

    private IEnumerator AttemptConnection()
    {
        while (CachedHabitatManager is null)
        {
            yield return new WaitForSeconds(1);
            Start();
            yield return null;
        }

        while (_dssManager is null)
        {
            yield return new WaitForSeconds(1);
            _dssManager = DSSService.main.GetDSSManager(CachedHabitatManager?.GetBasePrefabID());
            yield return null;
        }

        _dssManager.RegisterAntenna(this);
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        if(_dssManager is not null)
        {
            _dssManager.UnRegisterAntenna(this);
        }
    }

    public override bool IsOperational()
    {
        return IsConstructed;
    }
}

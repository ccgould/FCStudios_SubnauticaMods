using FCS_AlterraHub.API;
using FCS_AlterraHub.Configuation;
using FCS_AlterraHub.Models.Abstract;
using FCS_AlterraHub.Models.Interfaces;
using FCS_StorageSolutions.Configuation;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_StorageSolutions.ModItems.Buildables.DataStorageSolutions.Mono;
internal class DSSItemDisplayController : FCSDevice, IFCSSave<SaveData>
{

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
}

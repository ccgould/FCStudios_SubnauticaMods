using FCS_AlterraHub.Models.Interfaces;
using FCS_StorageSolutions.Configuration;
using FCS_StorageSolutions.ModItems.Buildables.DataStorageSolutions.Mono.Base;
using FCSCommon.Utilities;

namespace FCS_StorageSolutions.ModItems.Buildables.DataStorageSolutions.Mono;
internal class DSSFloorRackController : RackBase
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

                    var savedData = _savedData as DSSRackSaveData;

                    _colorManager?.LoadTemplate(((ISaveDataEntry)_savedData).ColorTemplate);
                }
            }
            _runStartUpOnEnable = false;
        }
    }

}

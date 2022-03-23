using FCS_AlterraHub.Extensions;
using FCS_StorageSolutions.Configuration;
using FCSCommon.Utilities;

namespace FCS_StorageSolutions.Mods.DataStorageSolutions.Mono.Rack
{
    internal class DSSWallServerRackController : DSSRackBase
    {
        protected override int StorageWidth { get; } = 3;
        protected override int StorageHeight { get; } = 3;
        protected override string ModClassName { get; } = Mod.DSSWallServerRackClassName;
        protected override DSSServerRackDataEntry SavedData { get; set; }

        public override void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            base.OnProtoDeserialize(serializer);
            IsFromSave = true;
        }

        public override void Save(SaveData newSaveData, ProtobufSerializer serializer = null)
        {
            base.Save(newSaveData, serializer);

            SavedData.ID = GetPrefabID();
            SavedData.ColorTemplate = _colorManager.SaveTemplate();
            newSaveData.DSSWallServerRackDataEntries.Add(SavedData);
            QuickLogger.Debug($"Saving ID {SavedData.ID}", true);
        }

        protected override void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            SavedData = Mod.GetDSSWallServerRackSaveData(GetPrefabID());
        }
    }
}

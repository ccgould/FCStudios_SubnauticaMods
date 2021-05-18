using System.Collections.Generic;
using FCS_AlterraHub.Extensions;
using FCS_StorageSolutions.Configuration;
using FCSCommon.Utilities;

namespace FCS_StorageSolutions.Mods.DataStorageSolutions.Mono.Rack
{
    internal class DSSFloorServerRackController : DSSRackBase
    {
        protected override int StorageWidth { get; } = 4;
        protected override int StorageHeight { get; } = 5;
        protected override string ModClassName { get; } = Mod.DSSFloorServerRackClassName;
        protected override DSSServerRackDataEntry SavedData { get; set; }
        
        protected override void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            SavedData = Mod.GetDSSFloorServerRackSaveData(GetPrefabID());
        }
        
        public override void Save(SaveData newSaveData, ProtobufSerializer serializer = null)
        {
            base.Save(newSaveData, serializer);

            SavedData.ID = GetPrefabID();
            SavedData.BodyColor = _colorManager.GetColor().ColorToVector4();
            SavedData.SecondaryColor = _colorManager.GetSecondaryColor().ColorToVector4();
            //SavedData.Slot1 = Save(serializer);
            newSaveData.DSSFloorServerRackDataEntries.Add(SavedData);
            QuickLogger.Debug($"Saving ID {SavedData.ID}", true);
        }

        public override void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            base.OnProtoDeserialize(serializer);
            if (SavedData.SaveVersion.Equals("1.0"))
            {
                RestoreItems(serializer, new List<byte[]>
                {
                    SavedData.Slot1,
                    SavedData.Slot2,
                    SavedData.Slot3,
                    SavedData.Slot4,
                    SavedData.Slot5,
                    SavedData.Slot6,
                    SavedData.Slot7,
                    SavedData.Slot8,
                    SavedData.Slot9,
                    SavedData.Slot10,
                    SavedData.Slot11,
                    SavedData.Slot12,
                    SavedData.Slot13,
                    SavedData.Slot14,
                    SavedData.Slot15,
                    SavedData.Slot16,
                    SavedData.Slot17,
                    SavedData.Slot18,
                    SavedData.Slot19,
                    SavedData.Slot20,
                });
            }
            else
            {
                RestoreItems(serializer, SavedData.Slot1);
            }

            IsFromSave = true;
        }
    }
}
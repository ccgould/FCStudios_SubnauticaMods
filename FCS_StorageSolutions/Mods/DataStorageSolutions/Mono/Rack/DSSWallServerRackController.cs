using System;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHomeSolutions.Mono.PaintTool;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Interfaces;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_StorageSolutions.Configuration;
using FCS_StorageSolutions.Helpers;
using FCS_StorageSolutions.Mods.AlterraStorage.Buildable;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_StorageSolutions.Mods.DataStorageSolutions.Mono.Rack
{
    internal class DSSWallServerRackController : DSSRackBase
    {
        protected override DSSServerRackDataEntry SavedData { get; set; }

        public override void Initialize()
        {
            //SlotsLocation = GameObjectHelpers.FindGameObject(gameObject, "rack_door_mesh").transform;
            base.Initialize();
        }

        public override void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            base.OnProtoDeserialize(serializer);
            Slots.ElementAtOrDefault(0).Value.RestoreItems(serializer, SavedData.Slot1);
            Slots.ElementAtOrDefault(1).Value.RestoreItems(serializer, SavedData.Slot2);
            Slots.ElementAtOrDefault(2).Value.RestoreItems(serializer, SavedData.Slot3);
            Slots.ElementAtOrDefault(3).Value.RestoreItems(serializer, SavedData.Slot4);
            Slots.ElementAtOrDefault(4).Value.RestoreItems(serializer, SavedData.Slot5);
            Slots.ElementAtOrDefault(5).Value.RestoreItems(serializer, SavedData.Slot6);
            _isFromSave = true;
        }

        public override void Save(SaveData newSaveData, ProtobufSerializer serializer = null)
        {
            base.Save(newSaveData, serializer);

            SavedData.ID = GetPrefabID();
            SavedData.BodyColor = _colorManager.GetColor().ColorToVector4();
            SavedData.SecondaryColor = _colorManager.GetSecondaryColor().ColorToVector4();
            SavedData.Slot1 = Slots.ElementAtOrDefault(0).Value.Save(serializer);
            SavedData.Slot2 = Slots.ElementAtOrDefault(1).Value.Save(serializer);
            SavedData.Slot3 = Slots.ElementAtOrDefault(2).Value.Save(serializer);
            SavedData.Slot4 = Slots.ElementAtOrDefault(3).Value.Save(serializer);
            SavedData.Slot5 = Slots.ElementAtOrDefault(4).Value.Save(serializer);
            SavedData.Slot6 = Slots.ElementAtOrDefault(5).Value.Save(serializer);
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

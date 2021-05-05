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
    internal class DSSFloorServerRackController : DSSRackBase
    {
        protected override DSSServerRackDataEntry SavedData { get; set; }

        public override void Initialize()
        {
            //SlotsLocation = GameObjectHelpers.FindGameObject(gameObject, "Slots").transform;
            base.Initialize();
        }

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
            SavedData.Slot1 = Slots.ElementAtOrDefault(0).Value.Save(serializer);
            SavedData.Slot2 = Slots.ElementAtOrDefault(1).Value.Save(serializer);
            SavedData.Slot3 = Slots.ElementAtOrDefault(2).Value.Save(serializer);
            SavedData.Slot4 = Slots.ElementAtOrDefault(3).Value.Save(serializer);
            SavedData.Slot5 = Slots.ElementAtOrDefault(4).Value.Save(serializer);
            SavedData.Slot6 = Slots.ElementAtOrDefault(5).Value.Save(serializer);
            SavedData.Slot7 = Slots.ElementAtOrDefault(6).Value.Save(serializer);
            SavedData.Slot8 = Slots.ElementAtOrDefault(7).Value.Save(serializer);
            SavedData.Slot9 = Slots.ElementAtOrDefault(8).Value.Save(serializer);
            SavedData.Slot10 = Slots.ElementAtOrDefault(9).Value.Save(serializer);
            SavedData.Slot11 = Slots.ElementAtOrDefault(10).Value.Save(serializer);
            SavedData.Slot12 = Slots.ElementAtOrDefault(11).Value.Save(serializer);
            SavedData.Slot13 = Slots.ElementAtOrDefault(12).Value.Save(serializer);
            SavedData.Slot14 = Slots.ElementAtOrDefault(13).Value.Save(serializer);
            SavedData.Slot15 = Slots.ElementAtOrDefault(14).Value.Save(serializer);
            SavedData.Slot16 = Slots.ElementAtOrDefault(15).Value.Save(serializer);
            SavedData.Slot17 = Slots.ElementAtOrDefault(16).Value.Save(serializer);
            SavedData.Slot18 = Slots.ElementAtOrDefault(17).Value.Save(serializer);
            SavedData.Slot19 = Slots.ElementAtOrDefault(18).Value.Save(serializer);
            SavedData.Slot20 = Slots.ElementAtOrDefault(19).Value.Save(serializer);
            newSaveData.DSSFloorServerRackDataEntries.Add(SavedData);
            QuickLogger.Debug($"Saving ID {SavedData.ID}", true);
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
            Slots.ElementAtOrDefault(6).Value.RestoreItems(serializer, SavedData.Slot7);
            Slots.ElementAtOrDefault(7).Value.RestoreItems(serializer, SavedData.Slot8);
            Slots.ElementAtOrDefault(8).Value.RestoreItems(serializer, SavedData.Slot9);
            Slots.ElementAtOrDefault(9).Value.RestoreItems(serializer, SavedData.Slot10);
            Slots.ElementAtOrDefault(10).Value.RestoreItems(serializer, SavedData.Slot11);
            Slots.ElementAtOrDefault(11).Value.RestoreItems(serializer, SavedData.Slot12);
            Slots.ElementAtOrDefault(12).Value.RestoreItems(serializer, SavedData.Slot13);
            Slots.ElementAtOrDefault(13).Value.RestoreItems(serializer, SavedData.Slot14);
            Slots.ElementAtOrDefault(14).Value.RestoreItems(serializer, SavedData.Slot15);
            Slots.ElementAtOrDefault(15).Value.RestoreItems(serializer, SavedData.Slot16);
            Slots.ElementAtOrDefault(16).Value.RestoreItems(serializer, SavedData.Slot17);
            Slots.ElementAtOrDefault(17).Value.RestoreItems(serializer, SavedData.Slot18);
            Slots.ElementAtOrDefault(18).Value.RestoreItems(serializer, SavedData.Slot19);
            Slots.ElementAtOrDefault(19).Value.RestoreItems(serializer, SavedData.Slot20);
            _isFromSave = true;
        }
    }
}
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
        protected override float OpenPos { get; } = 0.207f;
        protected override float ClosePos { get; } = -0.1668553f;

        public override void Initialize()
        {
            Tray = GameObjectHelpers.FindGameObject(gameObject, "SildingDoor_glass");
            SlotsLocation = GameObjectHelpers.FindGameObject(gameObject, "Slots").transform;
            base.Initialize();
        }

        protected override void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            SavedData = Mod.GetDSSFloorServerRackSaveData(GetPrefabID());
        }

        protected override void MoveTray()
        {
            if (Tray == null) return;

            if (IsOpen)
            {
                if (Tray.transform.localPosition.x < OpenPos)
                {
                    Tray.transform.Translate(Vector3.right * Speed * DayNightCycle.main.deltaTime);
                }

                if (Tray.transform.localPosition.x > OpenPos)
                {
                    Tray.transform.localPosition = new Vector3(OpenPos, Tray.transform.localPosition.y, Tray.transform.localPosition.z);
                }
            }
            else
            {
                if (Tray.transform.localPosition.x > ClosePos)
                {
                    Tray.transform.Translate(-Vector3.right * Speed * DayNightCycle.main.deltaTime);
                }

                if (Tray.transform.localPosition.x < ClosePos)
                {
                    Tray.transform.localPosition = new Vector3(ClosePos, Tray.transform.localPosition.y, Tray.transform.localPosition.z);
                }
            }
        }

        public override void Save(SaveData newSaveData, ProtobufSerializer serializer = null)
        {
            base.Save(newSaveData, serializer);

            SavedData.ID = GetPrefabID();
            SavedData.BodyColor = _colorManager.GetColor().ColorToVector4();
            SavedData.SecondaryColor = _colorManager.GetSecondaryColor().ColorToVector4();
            SavedData.IsTrayOpen = IsOpen;
            SavedData.Slot1 = Slots.ElementAt(0).Value.Save(serializer);
            SavedData.Slot2 = Slots.ElementAt(1).Value.Save(serializer);
            SavedData.Slot3 = Slots.ElementAt(2).Value.Save(serializer);
            SavedData.Slot4 = Slots.ElementAt(3).Value.Save(serializer);
            SavedData.Slot5 = Slots.ElementAt(4).Value.Save(serializer);
            SavedData.Slot6 = Slots.ElementAt(5).Value.Save(serializer);
            SavedData.Slot7 = Slots.ElementAt(6).Value.Save(serializer);
            SavedData.Slot8 = Slots.ElementAt(7).Value.Save(serializer);
            SavedData.Slot9 = Slots.ElementAt(8).Value.Save(serializer);
            SavedData.Slot10 = Slots.ElementAt(9).Value.Save(serializer);
            SavedData.Slot11 = Slots.ElementAt(10).Value.Save(serializer);
            SavedData.Slot12 = Slots.ElementAt(11).Value.Save(serializer);
            SavedData.Slot13 = Slots.ElementAt(12).Value.Save(serializer);
            SavedData.Slot14 = Slots.ElementAt(13).Value.Save(serializer);
            SavedData.Slot15 = Slots.ElementAt(14).Value.Save(serializer);
            SavedData.Slot16 = Slots.ElementAt(15).Value.Save(serializer);
            SavedData.Slot17 = Slots.ElementAt(16).Value.Save(serializer);
            newSaveData.DSSFloorServerRackDataEntries.Add(SavedData);
            QuickLogger.Debug($"Saving ID {SavedData.ID}", true);
        }

        public override void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            base.OnProtoDeserialize(serializer);
            Slots.ElementAt(0).Value.RestoreItems(serializer, SavedData.Slot1);
            Slots.ElementAt(1).Value.RestoreItems(serializer, SavedData.Slot2);
            Slots.ElementAt(2).Value.RestoreItems(serializer, SavedData.Slot3);
            Slots.ElementAt(3).Value.RestoreItems(serializer, SavedData.Slot4);
            Slots.ElementAt(4).Value.RestoreItems(serializer, SavedData.Slot5);
            Slots.ElementAt(5).Value.RestoreItems(serializer, SavedData.Slot6);
            Slots.ElementAt(6).Value.RestoreItems(serializer, SavedData.Slot7);
            Slots.ElementAt(7).Value.RestoreItems(serializer, SavedData.Slot8);
            Slots.ElementAt(8).Value.RestoreItems(serializer, SavedData.Slot9);
            Slots.ElementAt(9).Value.RestoreItems(serializer, SavedData.Slot10);
            Slots.ElementAt(10).Value.RestoreItems(serializer, SavedData.Slot11);
            Slots.ElementAt(11).Value.RestoreItems(serializer, SavedData.Slot12);
            Slots.ElementAt(12).Value.RestoreItems(serializer, SavedData.Slot13);
            Slots.ElementAt(13).Value.RestoreItems(serializer, SavedData.Slot14);
            Slots.ElementAt(14).Value.RestoreItems(serializer, SavedData.Slot15);
            Slots.ElementAt(15).Value.RestoreItems(serializer, SavedData.Slot16);
            Slots.ElementAt(16).Value.RestoreItems(serializer, SavedData.Slot17);
            _isFromSave = true;
        }
    }
}
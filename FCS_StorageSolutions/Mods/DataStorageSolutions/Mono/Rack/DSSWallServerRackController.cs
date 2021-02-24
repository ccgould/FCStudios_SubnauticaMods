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
        protected override float ClosePos { get; } = 0f;
        protected override float OpenPos { get; } = 0.382f;

        public override void Initialize()
        {
            Tray = GameObjectHelpers.FindGameObject(gameObject, "anim_rack_door");
            SlotsLocation = GameObjectHelpers.FindGameObject(gameObject, "rack_door_mesh").transform;
            base.Initialize();
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
            _isFromSave = true;
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
            newSaveData.DSSWallServerRackDataEntries.Add(SavedData);
            QuickLogger.Debug($"Saving ID {SavedData.ID}", true);
        }

        protected override void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            SavedData = Mod.GetDSSWallServerRackSaveData(GetPrefabID());
        }

        protected override void MoveTray()
        {
            if (Tray == null) return;

            if (IsOpen)
            {
                if (Tray.transform.localPosition.z < OpenPos)
                {
                    Tray.transform.Translate(Vector3.forward * Speed * DayNightCycle.main.deltaTime);
                }

                if (Tray.transform.localPosition.z > OpenPos)
                {
                    Tray.transform.localPosition = new Vector3(Tray.transform.localPosition.x, Tray.transform.localPosition.y, OpenPos);
                }
            }
            else
            {
                if (Tray.transform.localPosition.z > ClosePos)
                {
                    Tray.transform.Translate(-Vector3.forward * Speed * DayNightCycle.main.deltaTime);
                }

                if (Tray.transform.localPosition.z < ClosePos)
                {
                    Tray.transform.localPosition = new Vector3(Tray.transform.localPosition.x, Tray.transform.localPosition.y, ClosePos);
                }
            }
        }
    }
}

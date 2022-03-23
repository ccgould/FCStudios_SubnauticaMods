using System.Collections.Generic;
using FCS_AlterraHomeSolutions.Mono.PaintTool;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Model;
using FCS_StorageSolutions.Configuration;
using FCSCommon.Utilities;
using UnityEngine;

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
            SavedData.ColorTemplate = _colorManager.SaveTemplate();
            newSaveData.DSSFloorServerRackDataEntries.Add(SavedData);
            QuickLogger.Debug($"Saving ID {SavedData.ID}", true);
        }

        public override void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            base.OnProtoDeserialize(serializer);
            IsFromSave = true;
        }

        public override void Initialize()
        {
            base.Initialize();

            _colorManager?.ChangeColor(new ColorTemplate{SecondaryColor = Color.gray});
        }
    }
}
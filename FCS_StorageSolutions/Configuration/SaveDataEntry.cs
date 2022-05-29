using System;
using System.Collections.Generic;
using FCS_AlterraHub.Model;
#if SUBNAUTICA_STABLE
using Oculus.Newtonsoft.Json;
#else
using Newtonsoft.Json;
#endif

namespace FCS_StorageSolutions.Configuration
{
    [Serializable]
    internal class AlterraStorageDataEntry
    {
        [JsonProperty] internal string ID { get; set; }
        [JsonProperty] internal string SaveVersion { get; set; } = "1.0";
        [JsonProperty] internal ColorTemplateSave ColorTemplate { get; set; }
        [JsonProperty] internal byte[] Data { get; set; }
        [JsonProperty] internal string StorageName { get; set; }
    }

    [Serializable]
    internal class DSSServerDataEntry
    {
        [JsonProperty] internal string ID { get; set; }
        [JsonProperty] internal string SaveVersion { get; set; } = "1.0";
        [JsonProperty] internal byte[] Data { get; set; }
        [JsonProperty] internal string RackSlot { get; set; }
        [JsonProperty] internal string RackSlotUnitID { get; set; }
        [JsonProperty] internal string CurrentBase { get; set; }
        [JsonProperty] internal bool IsBeingFormatted { get; set; }
        [JsonProperty] internal HashSet<Filter> ServerFilters { get; set; }
    }    
    
    internal class DSSServerRackDataEntry
    {
        [JsonProperty] internal string ID { get; set; }
        [JsonProperty] internal string SaveVersion { get; set; } = "1.0";
        [JsonProperty] internal ColorTemplateSave ColorTemplate { get; set; }
        [JsonProperty] internal byte[] Slot1 { get; set; }
        public byte[] Slot2 {  internal get; set; }
        public byte[] Slot3 {  internal get; set; }
        public byte[] Slot4 {  internal get; set; }
        public byte[] Slot5 {  internal get; set; }
        public byte[] Slot6 {  internal get; set; }
        public byte[] Slot7 {  internal get; set; }
        public byte[] Slot8 {  internal get; set; }
        public byte[] Slot9 {  internal get; set; }
        public byte[] Slot10 { internal get; set; }
        public byte[] Slot11 { internal get; set; }
        public byte[] Slot12 { internal get; set; }
        public byte[] Slot13 { internal get; set; }
        public byte[] Slot14 { internal get; set; }
        public byte[] Slot15 { internal get; set; }
        public byte[] Slot16 { internal get; set; }
        public byte[] Slot17 { internal get; set; }
        public byte[] Slot18 { internal get; set; }
        public byte[] Slot19 { internal get; set; }
        public byte[] Slot20 { internal get; set; }
    }

    [Serializable]
    internal class DSSItemDisplayDataEntry
    {
        [JsonProperty] internal string ID { get; set; }
        [JsonProperty] internal string SaveVersion { get; set; } = "1.0";
        [JsonProperty] internal ColorTemplateSave ColorTemplate { get; set; }
        [JsonProperty] internal TechType CurrentItem { get; set; }
    }

    [Serializable]
    internal class DSSAntennaDataEntry
    {
        [JsonProperty] internal string ID { get; set; }
        [JsonProperty] internal string SaveVersion { get; set; } = "1.0";
        [JsonProperty] internal ColorTemplateSave ColorTemplate { get; set; }
    }
    
    [Serializable]
    internal class DSSTerminalDataEntry
    {
        [JsonProperty] internal string ID { get; set; }
        [JsonProperty] internal string SaveVersion { get; set; } = "1.0";
        [JsonProperty] internal ColorTemplateSave ColorTemplate { get; set; }
    }

    [Serializable]
    internal class SaveData
    {
        [JsonProperty] internal List<AlterraStorageDataEntry> AlterraStorageDataEntries = new();
        [JsonProperty] internal List<DSSServerDataEntry> DSSServerDataEntries = new();
        [JsonProperty] internal List<DSSItemDisplayDataEntry> DSSItemDisplayDataEntries = new();
        [JsonProperty] internal List<DSSAntennaDataEntry> DSSAntennaDataEntries = new();
        [JsonProperty] internal List<DSSTerminalDataEntry> DSSTerminalDataEntries = new();
        [JsonProperty] internal List<DSSServerRackDataEntry> DSSWallServerRackDataEntries = new();
        [JsonProperty] internal List<DSSServerRackDataEntry> DSSFloorServerRackDataEntries = new();
    }
}

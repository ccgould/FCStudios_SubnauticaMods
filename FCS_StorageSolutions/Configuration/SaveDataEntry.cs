using System;
using System.Collections.Generic;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Objects;
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
        [JsonProperty] internal Vec4 Body { get; set; }
        [JsonProperty] internal bool IsVisible { get; set; }
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
        [JsonProperty] internal Vec4 BodyColor { get; set; }
        [JsonProperty] internal Vec4 SecondaryColor { get; set; }
        [JsonProperty] internal bool IsTrayOpen { get; set; }
        [JsonProperty] internal byte[] Slot1 { get; set; }
        [JsonProperty] internal byte[] Slot2 { get; set; }
        [JsonProperty] internal byte[] Slot3 { get; set; }
        [JsonProperty] internal byte[] Slot4 { get; set; }
        [JsonProperty] internal byte[] Slot5 { get; set; }
        [JsonProperty] internal byte[] Slot6 { get; set; }
        [JsonProperty] internal byte[] Slot7 { get; set; }
        [JsonProperty] internal byte[] Slot8 { get; set; }
        [JsonProperty] internal byte[] Slot9 { get; set; }
        [JsonProperty] internal byte[] Slot10 { get; set; }
        [JsonProperty] internal byte[] Slot11 { get; set; }
        [JsonProperty] internal byte[] Slot12 { get; set; }
        [JsonProperty] internal byte[] Slot13 { get; set; }
        [JsonProperty] internal byte[] Slot14 { get; set; }
        [JsonProperty] internal byte[] Slot15 { get; set; }
        [JsonProperty] internal byte[] Slot16 { get; set; }
        [JsonProperty] internal byte[] Slot17 { get; set; }
        [JsonProperty] internal byte[] Slot18 { get; set; }
        [JsonProperty] internal byte[] Slot19 { get; set; }
        [JsonProperty] internal byte[] Slot20 { get; set; }
    }

    [Serializable]
    internal class DSSFormattingStationDataEntry
    {
        [JsonProperty] internal string ID { get; set; }
        [JsonProperty] internal string SaveVersion { get; set; } = "1.0";
        [JsonProperty] internal Vec4 Body { get; set; }
        [JsonProperty] internal byte[] Data { get; set; }
        [JsonProperty] internal Vec4 SecondaryBody { get; set; }
        [JsonProperty] internal byte[] Bytes { get; set; }
    }

    [Serializable]
    internal class DSSItemDisplayDataEntry
    {
        [JsonProperty] internal string ID { get; set; }
        [JsonProperty] internal string SaveVersion { get; set; } = "1.0";
        [JsonProperty] internal Vec4 Body { get; set; }
        [JsonProperty] internal Vec4 SecondaryBody { get; set; }
        [JsonProperty] internal TechType CurrentItem { get; set; }
    }

    [Serializable]
    internal class DSSAntennaDataEntry
    {
        [JsonProperty] internal string ID { get; set; }
        [JsonProperty] internal string SaveVersion { get; set; } = "1.0";
        [JsonProperty] internal Vec4 Body { get; set; }
        [JsonProperty] internal Vec4 SecondaryBody { get; set; }
    }
    
    [Serializable]
    internal class DSSTerminalDataEntry
    {
        [JsonProperty] internal string ID { get; set; }
        [JsonProperty] internal string SaveVersion { get; set; } = "1.0";
        [JsonProperty] internal Vec4 Body { get; set; }
        [JsonProperty] internal Vec4 SecondaryBody { get; set; }
    }

    [Serializable]
    internal class SaveData
    {
        [JsonProperty] internal List<AlterraStorageDataEntry> AlterraStorageDataEntries = new List<AlterraStorageDataEntry>();
        [JsonProperty] internal List<DSSServerDataEntry> DSSServerDataEntries = new List<DSSServerDataEntry>();
        [JsonProperty] internal List<DSSFormattingStationDataEntry> DSSFormattingStationDataEntries = new List<DSSFormattingStationDataEntry>();
        [JsonProperty] internal List<DSSItemDisplayDataEntry> DSSItemDisplayDataEntries = new List<DSSItemDisplayDataEntry>();
        [JsonProperty] internal List<DSSAntennaDataEntry> DSSAntennaDataEntries = new List<DSSAntennaDataEntry>();
        [JsonProperty] internal List<DSSTerminalDataEntry> DSSTerminalDataEntries = new List<DSSTerminalDataEntry>();
        [JsonProperty] internal List<DSSServerRackDataEntry> DSSWallServerRackDataEntries = new List<DSSServerRackDataEntry>();
        [JsonProperty] internal List<DSSServerRackDataEntry> DSSFloorServerRackDataEntries = new List<DSSServerRackDataEntry>();
    }
}

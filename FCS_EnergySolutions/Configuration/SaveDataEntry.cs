using System;
using System.Collections.Generic;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Objects;
using FCS_EnergySolutions.Mods.TelepowerPylon.Model;
using FCS_EnergySolutions.Mods.TelepowerPylon.Mono;
using FCS_EnergySolutions.Mods.WindSurfer.Mono;
using FCS_EnergySolutions.Mods.WindSurfer.Structs;
using FCS_EnergySolutions.PowerStorage.Enums;
using FCS_EnergySolutions.PowerStorage.Mono;
using FCSCommon.Interfaces;
using UnityEngine;
#if SUBNAUTICA_STABLE
using Oculus.Newtonsoft.Json;
#else
using Newtonsoft.Json;
#endif

namespace FCS_EnergySolutions.Configuration
{
    [Serializable]
    internal class AlterraGenDataEntry : ISaveDataEntry
    {
        public string Id { get; set; }
        [JsonProperty] internal string SaveVersion { get; set; } = "1.0";
        [JsonProperty] internal Vec4 Body { get; set; }
        [JsonProperty] internal Dictionary<TechType, int> Storage { get; set; }
        [JsonProperty] internal float ToConsume { get; set; }
        [JsonProperty] internal FCSPowerStates PowerState { get; set; }
        [JsonProperty] internal float Power { get; set; }
        [JsonProperty] internal float StoredPower { get; set; }
        public string BaseId { get; set; }
        [JsonProperty] internal bool IsVisible { get; set; }
    }    
    
    internal class AlterraSolarClusterDataEntry : ISaveDataEntry
    {
        public string Id { get; set; }
        public string BaseId { get; set; }
        [JsonProperty] internal string SaveVersion { get; set; } = "1.0";
        [JsonProperty] internal Vec4 Body { get; set; }
        [JsonProperty] internal float StoredPower { get; set; }
    }

    internal class JetStreamT242DataEntry : ISaveDataEntry
    {
        public string Id { get; set; }
        public string BaseId { get; set; }
        [JsonProperty] internal string SaveVersion { get; set; } = "1.0";
        [JsonProperty] internal Vec4 Body { get; set; }
        [JsonProperty] internal Vec4 SecondaryBody { get; set; }
        [JsonProperty] internal FCSPowerStates PowerState { get; set; }
        [JsonProperty] internal float StoredPower { get; set; }
        [JsonProperty] internal string CurrentBiome { get; set; }
        [JsonProperty] internal bool IsIncreasing { get; set; }
        [JsonProperty] internal float CurrentSpeed { get; set; }
        [JsonProperty] internal float TargetRPM { get; set; }
        [JsonProperty] internal float TilterDeg { get; set; }
        [JsonProperty] internal bool TilterUseGlobal { get; set; }
        [JsonProperty] internal TargetAxis TilterAxis { get; set; }
        [JsonProperty] internal bool AxisTilterSet { get; set; }
        [JsonProperty] internal bool TilterMove { get; set; }
    }


    internal class PowerStorageDataEntry : ISaveDataEntry
    {
        public string Id { get; set; }
        public string BaseId { get; set; }
        [JsonProperty] internal string SaveVersion { get; set; } = "1.0";
        [JsonProperty] internal Vec4 Body { get; set; }
        [JsonProperty] internal Vec4 SecondaryBody { get; set; }
        [JsonProperty] internal FCSPowerStates PowerState { get; set; }
        [JsonProperty] internal byte[] Data { get; set; }
        [JsonProperty] internal PowerChargerMode Mode { get; set; }
    }

    internal class TelepowerPylonDataEntry : ISaveDataEntry
    {
        public string Id { get; set; }
        public string BaseId { get; set; }
        [JsonProperty] internal string SaveVersion { get; set; } = "1.0";
        [JsonProperty] internal Vec4 Body { get; set; }
        [JsonProperty] internal Vec4 SecondaryBody { get; set; }
        [JsonProperty] internal FCSPowerStates PowerState { get; set; }
        [JsonProperty] internal TelepowerPylonMode PylonMode { get; set; }
        [JsonProperty] internal List<string> CurrentConnections { get; set; }
        [JsonProperty] internal TelepowerPylonUpgrade Upgrade { get; set; }
    }    
    internal class WindSurferOperatorDataEntry : ISaveDataEntry
    {
        public string Id { get; set; }
        public string BaseId { get; set; }
        [JsonProperty] internal string SaveVersion { get; set; } = "1.0";
        [JsonProperty] internal Vec4 Body { get; set; }
        [JsonProperty] internal Vec4 SecondaryBody { get; set; }
        [JsonProperty] internal FCSPowerStates PowerState { get; set; }
        [JsonProperty] internal TelepowerPylonMode PylonMode { get; set; }
        [JsonProperty] internal SortedDictionary<string, ConnectedTurbineData> CurrentConnections { get; set; }
        [JsonProperty] internal TelepowerPylonUpgrade Upgrade { get; set; }
        [JsonProperty] internal float StoredPower { get; set; }
    }

    internal class WindSurferDataEntry : ISaveDataEntry
    {
        public string Id { get; set; }
        public string BaseId { get; set; }
        [JsonProperty] internal string SaveVersion { get; set; } = "1.0";
        [JsonProperty] internal Vec4 Body { get; set; }
        [JsonProperty] internal Vec4 SecondaryBody { get; set; }
        [JsonProperty] internal FCSPowerStates PowerState { get; set; }
        [JsonProperty] internal float StoredPower { get; set; }
        [JsonProperty] internal Vec3 Position { get; set; }
        [JsonProperty] internal Vec4 Rotation { get; set; }
        [JsonProperty] internal Vec3 PoleState { get; set; }
        [JsonProperty] internal float Speed { get; set; }
    }

    [Serializable]
    internal class SaveData
    {
        [JsonProperty] internal List<AlterraGenDataEntry> AlterraGenEntries = new List<AlterraGenDataEntry>();
        [JsonProperty] internal List<JetStreamT242DataEntry> MarineTurbineEntries = new List<JetStreamT242DataEntry>();
        [JsonProperty] internal List<PowerStorageDataEntry> PowerStorageEntries = new List<PowerStorageDataEntry>();
        [JsonProperty] internal List<AlterraSolarClusterDataEntry> AlterraSolarClusterEntries = new List<AlterraSolarClusterDataEntry>();
        [JsonProperty] internal List<TelepowerPylonDataEntry> TelepowerPylonEntries = new List<TelepowerPylonDataEntry>();
        [JsonProperty] internal List<WindSurferOperatorDataEntry> WindSurferOperatorEntries = new List<WindSurferOperatorDataEntry>();
        [JsonProperty] internal List<WindSurferDataEntry> WindSurferEntries = new List<WindSurferDataEntry>();
    }
}

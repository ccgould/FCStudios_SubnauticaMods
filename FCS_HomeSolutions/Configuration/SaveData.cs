using System;
using System.Collections.Generic;
using FCS_AlterraHomeSolutions.Mono.PaintTool;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Objects;
using FCS_HomeSolutions.Mods.QuantumTeleporter.Enumerators;
using FCS_HomeSolutions.Mods.TrashRecycler.Model;
using FCS_HomeSolutions.Mono.OutDoorPlanters;
using UnityEngine;
#if SUBNAUTICA_STABLE
using Oculus.Newtonsoft.Json;
#else
using Newtonsoft.Json;
#endif

namespace FCS_HomeSolutions.Configuration
{
    [Serializable]
    internal class DecorationDataEntry
    {
        [JsonProperty] internal string Id { get; set; }
        [JsonProperty(PropertyName = "COL")] internal Vec4 Fcs { get; set; }
        [JsonProperty(PropertyName = "SCOL")] internal Vec4 Secondary { get; set; }
        [JsonProperty(PropertyName = "ECOL")] internal Vec4 Emission { get; set; }
        [JsonProperty(PropertyName = "VOL")] internal float Volume { get; set; }
        [JsonProperty(PropertyName = "CHAN")] internal int Channel { get; set; }
        [JsonProperty(PropertyName = "IsOn")] internal bool IsOn { get; set; }
    }

    [Serializable]
    internal class CurtainDataEntry
    {
        [JsonProperty] internal string Id { get; set; }
        [JsonProperty(PropertyName = "COL")] internal Vec4 Fcs { get; set; }
        [JsonProperty(PropertyName = "SCOL")] internal Vec4 Secondary { get; set; }
        public string SelectedTexturePath { get; set; }
        [JsonProperty] internal bool IsOpen { get; set; }
    }


    [Serializable]
    internal class PaintToolDataEntry
    {
        [JsonProperty] internal string Id { get; set; }
        [JsonProperty(PropertyName = "COL")] internal Vec4 Fcs { get; set; }
        [JsonProperty(PropertyName = "A")] internal int Amount { get; set; }
        [JsonProperty(PropertyName = "PTM")] internal ColorTargetMode ColorTargetMode { get; set; }
    }
    
    internal class PlanterDataEntry
    {
        [JsonProperty] internal string Id { get; set; }
        [JsonProperty(PropertyName = "COL")] internal Vec4 Fcs { get; set; }
        [JsonProperty(PropertyName = "SCOL")] internal Vec4 Secondary { get; set; }
        [JsonProperty(PropertyName = "LUMCOL")] internal Vec4 Lum { get; set; }
        [JsonProperty]  internal byte[] Bytes { get; set; }
        [JsonProperty(PropertyName = "BID")] internal string BaseID { get; set; }
        public IEnumerable<PlantData> PlantAges { get; set; }
    }

    internal class MiniFountainFilterDataEntry
    {
        [JsonProperty] internal string Id { get; set; }
        [JsonProperty(PropertyName = "COL")] internal Vec4 Body { get; set; }
        [JsonProperty] internal float TankLevel { get; set; }
        [JsonProperty] internal int ContainerAmount { get; set; }
        [JsonProperty] internal bool IsInSub { get; set; }
        [JsonProperty(PropertyName = "BID")] internal string BaseID { get; set; }

    }

    internal class SeaBreezeDataEntry
    {
        [JsonProperty] internal string Id { get; set; }
        [JsonProperty] internal List<EatableEntities> FridgeContainer { get; set; }
        [JsonProperty] internal bool HasBreakerTripped { get; set; }
        [JsonProperty] internal string UnitName { get; set; }
        [JsonProperty] internal Vec4 Body { get; set; }
        [JsonProperty] internal PowercellData PowercellData { get; set; }
        [JsonProperty(PropertyName = "BID")] internal string BaseID { get; set; }

    }

    internal class TrashRecyclerDataEntry
    {
        [JsonProperty] internal string Id { get; set; }
        [JsonProperty(PropertyName = "COL")] internal Vec4 Fcs { get; set; }
        [JsonProperty(PropertyName = "SCOL")] internal Vec4 Secondary { get; set; }
        [JsonProperty(PropertyName = "BID")] internal string BaseID { get; set; }
        public bool IsRecycling { get; set; }
        public float CurrentTime { get; set; }
        public Queue<Waste> QueuedItems { get; set; }
        [JsonProperty(PropertyName = "BMC")] internal int BioMaterialsCount { get; set; }
    }

    internal class QuantumTeleporterDataEntry
    {
        [JsonProperty] internal string Id { get; set; }
        [JsonProperty(PropertyName = "COL")] internal Vec4 Fcs { get; set; }
        [JsonProperty(PropertyName = "SCOL")] internal Vec4 Secondary { get; set; }
        [JsonProperty(PropertyName = "BID")] internal string BaseID { get; set; }
        [JsonProperty] internal string UnitName { get; set; }
        [JsonProperty] internal bool IsGlobal { get; set; }
        [JsonProperty] internal QTTeleportTypes SelectedTab { get; set; }
        [JsonProperty] internal string LinkedPortal { get; set; }
        [JsonProperty] internal bool IsLinked { get; set; }
    }

    internal class BaseOperatorDataEntry
    {
        [JsonProperty] internal string Id { get; set; }
        [JsonProperty(PropertyName = "COL")] internal Vec4 Fcs { get; set; }
        [JsonProperty(PropertyName = "SCOL")] internal Vec4 Secondary { get; set; }
        [JsonProperty(PropertyName = "BID")] internal string BaseID { get; set; }
    }       
    
    internal class AlterraMiniBathroomDataEntry
    {
        [JsonProperty] internal string Id { get; set; }
        [JsonProperty(PropertyName = "COL")] internal Vec4 Fcs { get; set; }
        [JsonProperty(PropertyName = "SCOL")] internal Vec4 Secondary { get; set; }
        public bool ShowerDoorState { get; set; }
        public bool ToiletDoorState { get; set; }
        public bool IsShowerOn { get; set; }
    }       
    
    internal class ObservationTankDataEntry
    {
        [JsonProperty] internal string Id { get; set; }
        [JsonProperty(PropertyName = "COL")] internal Vec4 Body { get; set; }
        [JsonProperty(PropertyName = "SCOL")] internal Vec4 Secondary { get; set; }
        [JsonProperty(PropertyName = "ECOL")] internal Vec4 Emission { get; set; }
        [JsonProperty(PropertyName = "BID")] internal string BaseID { get; set; }
    }


    internal class SignDataEntry
    {
        [JsonProperty] internal string Id { get; set; }
        [JsonProperty(PropertyName = "COL")] internal Vec4 Body { get; set; }
        [JsonProperty(PropertyName = "ADir")] internal Vec4 ArrowDirection { get; set; }
        [JsonProperty(PropertyName = "SignName")] internal string SignName { get; set; }
    }


    internal class CabinetDataEntry
    {
        [JsonProperty] internal string Id { get; set; }
        [JsonProperty(PropertyName = "COL")] internal Vec4 Fcs { get; set; }
        [JsonProperty(PropertyName = "SCOL")] internal Vec4 Secondary { get; set; }
    }
    
    internal class AlienChiefDataEntry
    {
        [JsonProperty] internal string Id { get; set; }
        [JsonProperty(PropertyName = "COL")] internal Vec4 Fcs { get; set; }
        [JsonProperty(PropertyName = "SCOL")] internal Vec4 Secondary { get; set; }
        [JsonProperty(PropertyName = "BID")] internal string BaseID { get; set; }
        [JsonProperty(PropertyName = "Data")] internal byte[] Bytes { get; set; }
    }    
    
    internal class FEXRDataEntry
    {
        [JsonProperty] internal string Id { get; set; }
        [JsonProperty(PropertyName = "COL")] internal Vec4 Body { get; set; }
        [JsonProperty(PropertyName = "ECOL")] internal Vec4 Emission { get; set; }
        [JsonProperty(PropertyName = "Data")] internal byte[] Data { get; set; }
    }

    internal class LedLightDataEntry
    {
        [JsonProperty] internal string Id { get; set; }
        [JsonProperty(PropertyName = "LUM")] internal Vec4 Lum { get; set; }
        [JsonProperty(PropertyName = "ROT")] internal Vec4 Rotation { get; set; }
        [JsonProperty] internal float Intensity { get; set; }
        [JsonProperty] internal bool NightSensor { get; set; }
        [JsonProperty] internal bool LightState { get; set; }
    }

    [Serializable]
    internal class SaveData
    {
        [JsonProperty] internal float SaveVersion { get; set; } = 1.1f;

        [JsonProperty] internal List<SeaBreezeDataEntry> SeaBreezeDataEntries = new List<SeaBreezeDataEntry>();
        [JsonProperty] internal List<SignDataEntry> SignDataEntries = new List<SignDataEntry>();
        [JsonProperty] internal List<DecorationDataEntry> DecorationEntries = new List<DecorationDataEntry>();
        [JsonProperty] internal List<PaintToolDataEntry> PaintToolEntries = new List<PaintToolDataEntry>();
        [JsonProperty] internal List<PlanterDataEntry> PlanterEntries = new List<PlanterDataEntry>();
        [JsonProperty] internal List<MiniFountainFilterDataEntry> MiniFountainFilterEntries = new List<MiniFountainFilterDataEntry>();
        [JsonProperty] internal List<TrashRecyclerDataEntry> TrashRecyclerEntries = new List<TrashRecyclerDataEntry>();
        [JsonProperty] internal List<QuantumTeleporterDataEntry> QuantumTeleporterEntries = new List<QuantumTeleporterDataEntry>();
        [JsonProperty] internal List<CurtainDataEntry> CurtainEntries = new List<CurtainDataEntry>();
        [JsonProperty] internal List<BaseOperatorDataEntry> BaseOperatorEntries = new List<BaseOperatorDataEntry>();
        [JsonProperty] internal List<AlienChiefDataEntry> AlienChiefDataEntries = new List<AlienChiefDataEntry>();
        [JsonProperty] internal List<CabinetDataEntry> CabinetDataEntries = new List<CabinetDataEntry>();
        [JsonProperty] internal List<LedLightDataEntry> LedLightDataEntries = new List<LedLightDataEntry>();
        [JsonProperty] internal List<ObservationTankDataEntry> ObservationTankDataEntries = new List<ObservationTankDataEntry>();
        [JsonProperty] internal List<FEXRDataEntry> FEXRDataEntries = new List<FEXRDataEntry>();
        [JsonProperty] internal List<AlterraMiniBathroomDataEntry> AlterraMiniBathroomEntries = new List<AlterraMiniBathroomDataEntry>();
    }
}

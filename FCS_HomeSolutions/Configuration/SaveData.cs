using System;
using System.Collections.Generic;
using FCS_AlterraHub.Interfaces;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Objects;
using FCS_HomeSolutions.Mods.DisplayBoard.Mono;
using FCS_HomeSolutions.Mods.Elevator.Mono;
using FCS_HomeSolutions.Mods.NeonPlanter.Mono;
using FCS_HomeSolutions.Mods.QuantumTeleporter.Enumerators;
using FCS_HomeSolutions.Mods.Stove.Mono;
using FCS_HomeSolutions.Mods.TrashRecycler.Model;
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
        public ColorTemplateSave ColorTemplate { get; set; }
        [JsonProperty(PropertyName = "VOL")] internal float Volume { get; set; }
        [JsonProperty(PropertyName = "Path")] internal string Video { get; set; }
        [JsonProperty(PropertyName = "IsOn")] internal bool IsOn { get; set; }
    }

    internal class StairsDataEntry : DecorationDataEntry
    {
        public int ChildCount { get; set; }
        public float FloorDistance { get; set; } = -1;
        public float FloorDistance2 { get; set; } = -1;
    }

    [Serializable]
    internal class CurtainDataEntry
    {
        [JsonProperty] internal string Id { get; set; }
        public ColorTemplateSave ColorTemplate { get; set; }
        public string SelectedTexturePath { get; set; }
        [JsonProperty] internal bool IsOpen { get; set; }
    }


    [Serializable]
    internal class PaintToolDataEntry
    {
        [JsonProperty] internal string Id { get; set; }
        public ColorTemplateSave ColorTemplate { get; set; }
        [JsonProperty(PropertyName = "A")] internal int Amount { get; set; }
        public List<ColorTemplateSave> ColorTemplates { get; set; }
        public int CurrentTemplateIndex { get; set; }
    }
    
    internal class PlanterDataEntry
    {
        [JsonProperty] internal string Id { get; set; }
        public ColorTemplateSave ColorTemplate { get; set; }
        [JsonProperty]  internal byte[] Bytes { get; set; }
        [JsonProperty(PropertyName = "BID")] internal string BaseID { get; set; }
        public IEnumerable<PlantData> PlantAges { get; set; }
    }

    internal class MiniFountainFilterDataEntry
    {
        [JsonProperty] internal string Id { get; set; }
        public ColorTemplateSave ColorTemplate { get; set; }
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
        public ColorTemplateSave ColorTemplate { get; set; }
        [JsonProperty] internal PowercellData PowercellData { get; set; }
        [JsonProperty(PropertyName = "BID")] internal string BaseID { get; set; }

    }

    internal class TrashRecyclerDataEntry
    {
        [JsonProperty] internal string Id { get; set; }
        public ColorTemplateSave ColorTemplate { get; set; }
        [JsonProperty(PropertyName = "BID")] internal string BaseID { get; set; }
        public bool IsRecycling { get; set; }
        public float CurrentTime { get; set; }
        public Queue<Waste> QueuedItems { get; set; }
        [JsonProperty(PropertyName = "BMC")] internal int BioMaterialsCount { get; set; }
    }

    internal class QuantumTeleporterDataEntry
    {
        [JsonProperty] internal string Id { get; set; }
        public ColorTemplateSave ColorTemplate { get; set; }
        [JsonProperty] internal string UnitName { get; set; }
        [JsonProperty] internal bool IsGlobal { get; set; }
        [JsonProperty] internal QTTeleportTypes SelectedTab { get; set; }
        [JsonProperty] internal string LinkedPortal { get; set; }
        [JsonProperty] internal bool IsLinked { get; set; }
    }

    internal class QuantumTeleporterVehiclePadDataEntry
    {
        [JsonProperty] internal string Id { get; set; }
        public ColorTemplateSave ColorTemplate { get; set; }
        [JsonProperty] internal string OccupiedVehicleID  { get; set; }
    }

    internal class BaseOperatorDataEntry
    {
        [JsonProperty] internal string Id { get; set; }
        public ColorTemplateSave ColorTemplate { get; set; }
    }      
    
    internal class DisplayBoardDataEntry :ISaveDataEntry
    {
        public string Id { get; set; }
        public string BaseId { get; set; }
        public ColorTemplateSave ColorTemplate { get; set; }
        public Tuple<string, DisplayMode> Display1 { get; set; }
        public Tuple<string, DisplayMode> Display2 { get; set; }
        public Tuple<string, DisplayMode> Display3 { get; set; }
        public Tuple<string, DisplayMode> Display4 { get; set; }
    }       
    
    internal class ShowerDataEntry
    {
        [JsonProperty] internal string Id { get; set; }
        public ColorTemplateSave ColorTemplate { get; set; }
    }       
    
    internal class ObservationTankDataEntry
    {
        [JsonProperty] internal string Id { get; set; }
        public ColorTemplateSave ColorTemplate { get; set; }
    }


    internal class SignDataEntry
    {
        [JsonProperty] internal string Id { get; set; }
        public ColorTemplateSave ColorTemplate { get; set; }
        [JsonProperty(PropertyName = "ADir")] internal Vec4 ArrowDirection { get; set; }
        [JsonProperty(PropertyName = "SignName")] internal string SignName { get; set; }
    }


    internal class CabinetDataEntry
    {
        [JsonProperty] internal string Id { get; set; }
        public ColorTemplateSave ColorTemplate { get; set; }
        [JsonProperty]  internal string Label { get; set; }
    }
    
    internal class AlienChiefDataEntry
    {
        [JsonProperty] internal string Id { get; set; }
        public ColorTemplateSave ColorTemplate { get; set; }
    }    
    
    internal class FEXRDataEntry
    {
        [JsonProperty] internal string Id { get; set; }
        public ColorTemplateSave ColorTemplate { get; set; }
    }

    internal class PeeperLoungeBarEntry
    {
        [JsonProperty] internal string Id { get; set; }
        public ColorTemplate ColorTemplate { get; set; }
    }

    internal class TrashReceptacleDataEntry
    {
        [JsonProperty] internal string Id { get; set; }
        public string BaseId { get; set; }
        public ColorTemplateSave ColorTemplate { get; set; }
    }

    internal class LedLightDataEntry
    {
        [JsonProperty] internal string Id { get; set; }
        public ColorTemplateSave ColorTemplate { get; set; }
        [JsonProperty(PropertyName = "ROT")] internal Vec4 Rotation { get; set; }
        [JsonProperty] internal float Intensity { get; set; }
        [JsonProperty] internal bool NightSensor { get; set; }
        [JsonProperty] internal bool LightState { get; set; }
        [JsonProperty] internal Vec3 RotorRot { get; set; }
        [JsonProperty] internal Vec3 TilerRot { get; set; }
    }

    [Serializable]
    internal class HologramDataEntry
    {
        [JsonProperty] internal string Id { get; set; }
        public ColorTemplateSave ColorTemplate { get; set; }
        public string SelectedTexturePath { get; set; }
    }

    [Serializable]
    internal class ElevatorDataEntry
    {
        [JsonProperty] internal string Id { get; set; }
        public ColorTemplateSave ColorTemplate { get; set; }
        [JsonProperty] internal Dictionary<string, ElevatorFloorData> FloorData { get; set; }
        [JsonProperty] internal string CurrentFloorId { get; set; }
        [JsonProperty] internal float PlateformPosition { get; set; }
        [JsonProperty] internal bool IsRailingsVisible { get; set; }
        [JsonProperty] internal float Speed { get; set; }
    }

    [Serializable]
    internal class JukeBoxDataEntry
    {
        [JsonProperty] internal string Id { get; set; }
        public ColorTemplateSave ColorTemplate { get; set; }
        [JsonProperty] internal float Volume { get; set; }
    }

    [Serializable]
    internal class StoveDataEntry
    {
        [JsonProperty] internal string Id { get; set; }
        public ColorTemplateSave ColorTemplate { get; set; }
        [JsonProperty] internal Tuple<Queue<Cooker.CookingQueue>, float> QueuedItems { get; set; }
    }    
    
    [Serializable]
    internal class QuantumPowerBankDataEntry
    {
        [JsonProperty] internal string Id { get; set; }
        [JsonProperty] internal float Charge { get; set; }
    }    
    
    [Serializable]
    internal class QuantumPowerBankChargerDataEntry
    {
        [JsonProperty] internal string Id { get; set; }
        [JsonProperty] internal float Charge { get; set; }
        [JsonProperty] internal ColorTemplateSave ColorTemplate { get; set; }
        [JsonProperty] internal bool IsLocked { get; set; }
        [JsonProperty] internal float PartialPower { get; set; }
    }

    [Serializable]
    internal class SaveData
    {
        [JsonProperty] internal float SaveVersion { get; set; } = 1.1f;
        [JsonProperty] internal List<QuantumTeleporterVehiclePadDataEntry> QuantumTeleporterVehiclePadDataEntries = new();
        [JsonProperty] internal List<QuantumPowerBankChargerDataEntry> QuantumPowerBankChargerDataEntries = new();
        [JsonProperty] internal List<QuantumPowerBankDataEntry> QuantumPowerBankEntries = new();
        [JsonProperty] internal List<StoveDataEntry> StoveDataEntries = new();
        [JsonProperty] internal List<JukeBoxDataEntry> JukeBoxDataEntries = new();
        [JsonProperty] internal List<ElevatorDataEntry> ElevatorDataEntries = new();
        [JsonProperty] internal List<HologramDataEntry> HologramDataEntries = new();
        [JsonProperty] internal List<TrashReceptacleDataEntry> TrashReceptacleEntries = new();
        [JsonProperty] internal List<PeeperLoungeBarEntry> PeeperLoungeBarEntries = new();
        [JsonProperty] internal List<SeaBreezeDataEntry> SeaBreezeDataEntries = new();
        [JsonProperty] internal List<DisplayBoardDataEntry> DisplayBoardDataEntries = new();
        [JsonProperty] internal List<SignDataEntry> SignDataEntries = new();
        [JsonProperty] internal List<DecorationDataEntry> DecorationEntries = new();
        [JsonProperty] internal List<PaintToolDataEntry> PaintToolEntries = new();
        [JsonProperty] internal List<PlanterDataEntry> PlanterEntries = new();
        [JsonProperty] internal List<MiniFountainFilterDataEntry> MiniFountainFilterEntries = new();
        [JsonProperty] internal List<TrashRecyclerDataEntry> TrashRecyclerEntries = new();
        [JsonProperty] internal List<QuantumTeleporterDataEntry> QuantumTeleporterEntries = new();
        [JsonProperty] internal List<CurtainDataEntry> CurtainEntries = new();
        [JsonProperty] internal List<BaseOperatorDataEntry> BaseOperatorEntries = new();
        [JsonProperty] internal List<AlienChiefDataEntry> AlienChiefDataEntries = new();
        [JsonProperty] internal List<CabinetDataEntry> CabinetDataEntries = new();
        [JsonProperty] internal List<LedLightDataEntry> LedLightDataEntries = new();
        [JsonProperty] internal List<ObservationTankDataEntry> ObservationTankDataEntries = new();
        [JsonProperty] internal List<FEXRDataEntry> FEXRDataEntries = new();
        [JsonProperty] internal List<ShowerDataEntry> ShowerEntries = new();
        [JsonProperty] internal List<StairsDataEntry> StairsEntries = new();
    }
}

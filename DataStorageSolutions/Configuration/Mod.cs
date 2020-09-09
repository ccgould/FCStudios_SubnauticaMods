using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DataStorageSolutions.Abstract;
using DataStorageSolutions.Buildables;
using DataStorageSolutions.Display;
using DataStorageSolutions.Model;
using DataStorageSolutions.Mono;
using FCSCommon.Extensions;
using FCSCommon.Helpers;
using FCSCommon.Objects;
using FCSCommon.Utilities;
using FCSTechFabricator.Objects;
using Oculus.Newtonsoft.Json;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Options;
using SMLHelper.V2.Utility;
using UnityEngine;
using UnityEngine.UI;
using UWE;

namespace DataStorageSolutions.Configuration
{
    internal static class Mod
    {
        #region Private Members

        private static ModSaver _saveObject;
        private static SaveData _saveData;
        private const string ConfigFileName = "config.json";

        #endregion

        #region Internal Properties
        internal const string ModName = "DataStorageSolutions";
        internal static string ModFolderName => $"FCS_{ModName}";
        internal const string BundleName = "datastoragesolutionsbundle";
        internal const string DSSTabID = "DSS";
        internal const string ModFriendlyName = "Data Storage Solutions";

        internal static string SaveDataFilename => $"{ModName}SaveData.json";
        internal static string MODFOLDERLOCATION => GetModPath();
        internal static string AssetFolder => Path.Combine(ModName, "Assets");

        internal const string TerminalFriendlyName = "Terminal C48";
        internal const string TerminalDescription = "Terminal C48 bridges the connection between your Data Storage Solutions Server Racks and your finger tips";
        internal const string TerminalClassID = "DSSTerminal";
        internal const string TerminalKitClassID = "TerminalC48_Kit";
        internal const string TerminalPrefabName = "TerminalMontor";

        internal const string FloorMountedRackFriendlyName = "NetShelter C22 Floor Mounted Rack";
        internal const string FloorMountedRackDescription = "The NetShelter C22 is the floor mounted IT enclosure with basic functionality and features provided as a cost-effective solution. The NetShelter FCS maintains a strong focus on cooling, power distribution, and cable management to provide a reliable rack-mounting environment for mission-critical equipment but with optional features such as side panels or even an unassembled option that reduces the cost of the base enclosure.";
        internal const string FloorMountedRackClassID = "DSSFloorMountedRack";
        internal const string FloorMountedRackKitClassID = "FloorMountedRack_Kit";
        internal const string FloorMountedRackPrefabName = "FloorServerRack";

        internal const string WallMountedRackFriendlyName = "NetShelter C23 Wall Mounted Rack";
        internal const string WallMountedRackDescription = "The NetShelter C23 is the wall mounted IT enclosure with basic functionality and features provided as a cost-effective solution. The NetShelter FCS maintains a strong focus on cooling, power distribution, and cable management to provide a reliable rack-mounting environment for mission-critical equipment but with optional features such as side panels or even an unassembled option that reduces the cost of the base enclosure.";
        internal const string WallMountedRackClassID = "DSSWallMountedRack";
        internal const string WallMountedRackKitClassID = "WallMountedRack_Kit";
        internal const string WallMountedRackPrefabName = "WallServerRack";

        internal const string AntennaFriendlyName = "NetShelter Antenna C66";
        internal const string AntennaDescription = "The NetShelter Antenna C66 provides a reliable connection to all your terminals around the planet";
        internal const string AntennaClassID = "DSSAntenna";
        internal const string AntennaKitClassID = "NetShelterAntennaC66_Kit";
        internal const string AntennaPrefabName = "Antenna";

        internal const string ServerFormattingStationFriendlyName = "Server Format Station";
        internal const string ServerFormattingStationDescription = "Use this machine to filter your servers so they only store what you want them to accept.";
        internal const string ServerFormattingStationClassID = "DSSFormatStation";
        internal const string ServerFormattingStationKitClassID = "FormatStation_Kit";
        internal const string ServerFormattingStationPrefabName = "ServerFormatMachine";

        internal const string ServerFriendlyName = "NetShelter Server 100";
        internal const string ServerDescription = "The NetShelter Server 100 provides you with 48 free slots to securely access and store your items";
        internal const string ServerClassID = "DSSServer";
        internal const string ServerPrefabName = "Server";

        internal const string OperatorFriendlyName = "DSS Operator";
        internal const string OperatorDescription = "The DSS Operator allows your DSS system to connect to other devices to perform repeated task and performs other crafting operations";
        internal const string OperatorClassID = "DSSOperator";
        internal const string OperatorPrefabName = "DSSOperator";
        internal const string OperatorKitClassID = "DSSOperator_Kit";

        internal const string ItemDisplayFriendlyName = "DSS Item Display";
        internal const string ItemDisplayDescription = "The DSS Item display allows you to view how much of one item you have in your system.";
        internal const string ItemDisplayClassID = "DSSItemDisplay";
        internal const string ItemDisplayPrefabName = "ItemDisplay";
        internal const string ItemDisplayKitID = "ItemDisplay_Kit";

        public static List<TechType> BlackList = new List<TechType>
        {
           TechType.None,
TechType.LimestoneChunk,
TechType.CalciteOld,
TechType.DolomiteOld,
TechType.FlintOld,
TechType.EmeryOld,
TechType.MercuryOre,
TechType.CalciumChunk,
TechType.Placeholder,
TechType.CarbonOld,
TechType.EthanolOld,
TechType.EthyleneOld,
TechType.Gold,
TechType.Magnesium,
TechType.HydrogenOld,
TechType.Lodestone,
TechType.SandLoot,
TechType.BatteryAcidOld,
TechType.SandstoneChunk,
TechType.BasaltChunk,
TechType.ShaleChunk,
TechType.ObsidianChunk,
TechType.Fiber,
TechType.Enamel,
TechType.AcidOld,
TechType.VesselOld,
TechType.CombustibleOld,
TechType.OpalGem,
TechType.AminoAcids,
TechType.Graphene,
TechType.Nanowires,
TechType.PrecursorIonCrystalMatrix,
TechType.DrillableSalt,
TechType.DrillableQuartz,
TechType.DrillableCopper,
TechType.DrillableTitanium,
TechType.DrillableLead,
TechType.DrillableSilver,
TechType.DrillableDiamond,
TechType.DrillableGold,
TechType.DrillableMagnetite,
TechType.DrillableLithium,
TechType.DrillableMercury,
TechType.DrillableUranium,
TechType.DrillableAluminiumOxide,
TechType.DrillableNickel,
TechType.DrillableSulphur,
TechType.DrillableKyanite,
TechType.DiveSuit,
TechType.ShipComputerOld,
TechType.Drill,
TechType.PDA,
TechType.EscapePod,
TechType.Terraformer,
TechType.Transfuser,
TechType.BuildBot,
TechType.StasisSphere,
TechType.PowerGlide,
TechType.CompostCreepvine,
TechType.ProcessUranium,
TechType.PrecursorIonEnergyBlueprint,
TechType.FabricatorBlueprintOld,
TechType.ConstructorBlueprint,
TechType.CyclopsBlueprint,
TechType.FragmentAnalyzerBlueprintOld,
TechType.LockerBlueprint,
TechType.SpecialHullPlateBlueprintOld,
TechType.BikemanHullPlateBlueprintOld,
TechType.EatMyDictionHullPlateBlueprintOld,
TechType.DevTestItemBlueprintOld,
TechType.SeamothBlueprint,
TechType.StasisRifleBlueprint,
TechType.ExosuitBlueprint,
TechType.TransfuserBlueprint,
TechType.TerraformerBlueprint,
TechType.ReinforceHullBlueprint,
TechType.WorkbenchBlueprint,
TechType.PropulsionCannonBlueprint,
TechType.SpecimenAnalyzerBlueprint,
TechType.BioreactorBlueprint,
TechType.ThermalPlantBlueprint,
TechType.NuclearReactorBlueprint,
TechType.MoonpoolBlueprint,
TechType.FiltrationMachineBlueprint,
TechType.TechlightBlueprint,
TechType.LEDLightBlueprint,
TechType.CyclopsHullBlueprint,
TechType.CyclopsBridgeBlueprint,
TechType.CyclopsEngineBlueprint,
TechType.CyclopsDockingBayBlueprint,
TechType.SpotlightBlueprint,
TechType.RadioBlueprint,
TechType.StarshipCargoCrateBlueprint,
TechType.StarshipCircuitBoxBlueprint,
TechType.StarshipDeskBlueprint,
TechType.StarshipChairBlueprint,
TechType.StarshipMonitorBlueprint,
TechType.SolarPanelBlueprint,
TechType.PowerTransmitterBlueprint,
TechType.BaseUpgradeConsoleBlueprint,
TechType.BaseObservatoryBlueprint,
TechType.BaseWaterParkBlueprint,
TechType.PictureFrameBlueprint,
TechType.BaseRoomBlueprint,
TechType.BaseBulkheadBlueprint,
TechType.SeaglideBlueprint,
TechType.BatteryChargerBlueprint,
TechType.PowerCellChargerBlueprint,
TechType.FarmingTrayBlueprint,
TechType.SignBlueprint,
TechType.BenchBlueprint,
TechType.PlanterPotBlueprint,
TechType.PlanterBoxBlueprint,
TechType.PlanterShelfBlueprint,
TechType.AquariumBlueprint,
TechType.ReinforcedDiveSuitBlueprint,
TechType.RadiationSuitBlueprint,
TechType.StillsuitBlueprint,
TechType.ScannerRoomBlueprint,
TechType.BasePlanterBlueprint,
TechType.PlanterPot2Blueprint,
TechType.PlanterPot3Blueprint,
TechType.MedicalCabinetBlueprint,
TechType.BaseMapRoomBlueprint,
TechType.SeamothFragment,
TechType.StasisRifleFragment,
TechType.ExosuitFragment,
TechType.TransfuserFragment,
TechType.TerraformerFragment,
TechType.ReinforceHullFragment,
TechType.WorkbenchFragment,
TechType.PropulsionCannonFragment,
TechType.BioreactorFragment,
TechType.ThermalPlantFragment,
TechType.NuclearReactorFragment,
TechType.MoonpoolFragment,
TechType.BaseFiltrationMachineFragment,
TechType.CyclopsHullFragment,
TechType.CyclopsBridgeFragment,
TechType.CyclopsEngineFragment,
TechType.CyclopsDockingBayFragment,
TechType.SeaglideFragment,
TechType.ConstructorFragment,
TechType.SolarPanelFragment,
TechType.PowerTransmitterFragment,
TechType.BaseUpgradeConsoleFragment,
TechType.BaseObservatoryFragment,
TechType.BaseWaterParkFragment,
TechType.RadioFragment,
TechType.BaseRoomFragment,
TechType.BaseBulkheadFragment,
TechType.BatteryChargerFragment,
TechType.PowerCellChargerFragment,
TechType.ScannerRoomFragment,
TechType.SpecimenAnalyzerFragment,
TechType.FarmingTrayFragment,
TechType.SignFragment,
TechType.PictureFrameFragment,
TechType.BenchFragment,
TechType.PlanterPotFragment,
TechType.PlanterBoxFragment,
TechType.PlanterShelfFragment,
TechType.AquariumFragment,
TechType.ReinforcedDiveSuitFragment,
TechType.RadiationSuitFragment,
TechType.StillsuitFragment,
TechType.BuilderFragment,
TechType.LEDLightFragment,
TechType.TechlightFragment,
TechType.SpotlightFragment,
TechType.BaseMapRoomFragment,
TechType.BaseBioReactorFragment,
TechType.BaseNuclearReactorFragment,
TechType.LaserCutterFragment,
TechType.BeaconFragment,
TechType.GravSphereFragment,
TechType.SafeShallowsEgg,
TechType.KelpForestEgg,
TechType.GrassyPlateausEgg,
TechType.GrandReefsEgg,
TechType.MushroomForestEgg,
TechType.KooshZoneEgg,
TechType.TwistyBridgesEgg,
TechType.LavaZoneEgg,
TechType.ReefbackEgg,
TechType.RabbitrayEggUndiscovered,
TechType.JellyrayEggUndiscovered,
TechType.StalkerEggUndiscovered,
TechType.ReefbackEggUndiscovered,
TechType.JumperEggUndiscovered,
TechType.BonesharkEggUndiscovered,
TechType.GasopodEggUndiscovered,
TechType.MesmerEggUndiscovered,
TechType.SandsharkEggUndiscovered,
TechType.ShockerEggUndiscovered,
TechType.CrashEggUndiscovered,
TechType.CrabsquidEggUndiscovered,
TechType.CutefishEggUndiscovered,
TechType.LavaLizardEggUndiscovered,
TechType.CrabsnakeEggUndiscovered,
TechType.SpadefishEggUndiscovered,
TechType.ReefbackShell,
TechType.ReefbackTissue,
TechType.ReefbackAdvancedStructure,
TechType.ReefbackDNA,
TechType.Workbench,
TechType.HullReinforcementModule,
TechType.Fabricator,
TechType.Aquarium,
TechType.Locker,
TechType.Spotlight,
TechType.DiveHatch,
TechType.CurrentGenerator,
TechType.FragmentAnalyzer,
TechType.SpecialHullPlate,
TechType.BikemanHullPlate,
TechType.EatMyDictionHullPlate,
TechType.DevTestItem,
TechType.SpecimenAnalyzer,
TechType.HullReinforcementModule2,
TechType.HullReinforcementModule3,
TechType.SolarPanel,
TechType.Sign,
TechType.PowerTransmitter,
TechType.Accumulator,
TechType.Bioreactor,
TechType.ThermalPlant,
TechType.NuclearReactor,
TechType.SmallLocker,
TechType.Bench,
TechType.PictureFrame,
TechType.PlanterPot,
TechType.PlanterBox,
TechType.PlanterShelf,
TechType.FarmingTray,
TechType.FiltrationMachine,
TechType.Techlight,
TechType.Radio,
TechType.PlanterPot2,
TechType.PlanterPot3,
TechType.MedicalCabinet,
TechType.SingleWallShelf,
TechType.WallShelves,
TechType.Bed1,
TechType.Bed2,
TechType.NarrowBed,
TechType.BatteryCharger,
TechType.PowerCellCharger,
TechType.Incubator,
TechType.EnyzmeCloud,
TechType.EnzymeCureBall,
TechType.Centrifuge,
TechType.CyclopsFabricator,
TechType.StarshipCargoCrate,
TechType.StarshipCircuitBox,
TechType.StarshipDesk,
TechType.StarshipChair,
TechType.StarshipMonitor,
TechType.StarshipChair2,
TechType.StarshipChair3,
TechType.CoffeeVendingMachine,
TechType.BarTable,
TechType.Trashcans,
TechType.LabTrashcan,
TechType.VendingMachine,
TechType.LabCounter,
TechType.Seamoth,
TechType.Exosuit,
TechType.CrashedShip,
TechType.Cyclops,
TechType.Audiolog,
TechType.Signal,
TechType.SeamothReinforcementModule,
TechType.LootSensorMetal,
TechType.LootSensorLithium,
TechType.LootSensorFragment,
TechType.ExosuitDrillArmFragment,
TechType.ExosuitPropulsionArmFragment,
TechType.ExosuitGrapplingArmFragment,
TechType.ExosuitTorpedoArmFragment,
TechType.ExosuitClawArmFragment,
TechType.Creepvine,
TechType.Slime,
TechType.LavaLarva,
TechType.Bloom,
TechType.Reefback,
TechType.Grabcrab,
TechType.Player,
TechType.Bleeder,
TechType.Rockgrub,
TechType.CrashHome,
TechType.HoopfishSchool,
TechType.RockPuncher,
TechType.SeaTreader,
TechType.SeaEmperor,
TechType.ReaperLeviathan,
TechType.CaveCrawler,
TechType.Skyray,
TechType.Biter,
TechType.SkyrayNonRoosting,
TechType.Shuttlebug,
TechType.Blighter,
TechType.Warper,
TechType.SpineEel,
TechType.SeaDragon,
TechType.SeaEmperorBaby,
TechType.WarperSpawner,
TechType.GhostRayBlue,
TechType.GhostRayRed,
TechType.ReefbackBaby,
TechType.PrecursorDroid,
TechType.GhostLeviathan,
TechType.SeaEmperorLeviathan,
TechType.SeaEmperorJuvenile,
TechType.GhostLeviathanJuvenile,
TechType.HoleFishAnalysis,
TechType.PeeperAnalysis,
TechType.BladderfishAnalysis,
TechType.GarryFishAnalysis,
TechType.HoverfishAnalysis,
TechType.ReginaldAnalysis,
TechType.SpadefishAnalysis,
TechType.BoomerangAnalysis,
TechType.EyeyeAnalysis,
TechType.OculusAnalysis,
TechType.HoopfishAnalysis,
TechType.AnalysisTreeOld,
TechType.SpinefishAnalysis,
TechType.PlantPlaceholder,
TechType.BallClusters,
TechType.BarnacleSuckers,
TechType.BlueBarnacle,
TechType.BlueBarnacleCluster,
TechType.BlueCoralTubes,
TechType.RedGrass,
TechType.GreenGrass,
TechType.Mohawk,
TechType.GreenReeds,
TechType.BlueJeweledDisk,
TechType.GreenJeweledDisk,
TechType.PurpleJeweledDisk,
TechType.RedJeweledDisk,
TechType.SmallKoosh,
TechType.MediumKoosh,
TechType.LargeKoosh,
TechType.HugeKoosh,
TechType.MembrainTree,
TechType.PurpleFan,
TechType.PurpleTentacle,
TechType.RedSeaweed,
TechType.CoralOldPlaceholder,
TechType.CoralShellPlate,
TechType.SmallFan,
TechType.SmallFanCluster,
TechType.BigCoralTubes,
TechType.TreeMushroom,
TechType.BlueCluster,
TechType.BrownTubes,
TechType.BloodGrass,
TechType.HeatArea,
TechType.BloodRoot,
TechType.BloodVine,
TechType.PinkFlower,
TechType.PinkMushroom,
TechType.PurpleRattle,
TechType.BulboTree,
TechType.PurpleVasePlant,
TechType.OrangeMushroom,
TechType.FernPalm,
TechType.HangingFruitTree,
TechType.BluePalm,
TechType.GabeSFeather,
TechType.SeaCrown,
TechType.OrangePetalsPlant,
TechType.EyesPlant,
TechType.RedGreenTentacle,
TechType.PurpleStalk,
TechType.RedBasketPlant,
TechType.RedBush,
TechType.RedConePlant,
TechType.ShellGrass,
TechType.SpottedLeavesPlant,
TechType.RedRollPlant,
TechType.SnakeMushroom,
TechType.GenericJeweledDisk,
TechType.FloatingStone,
TechType.BlueAmoeba,
TechType.RedTipRockThings,
TechType.BlueTipLostRiverPlant,
TechType.BlueLostRiverLilly,
TechType.LargeFloater,
TechType.PiecePlaceholder,
TechType.CoralChunk,
TechType.KooshChunk,
TechType.PurpleVegetable,
TechType.EnvironmentPlaceholder,
TechType.Boulder,
TechType.PurpleBrainCoral,
TechType.HangingStinger,
TechType.SpikePlant,
TechType.BrainCoral,
TechType.CoveTree,
TechType.MonsterSkeleton,
TechType.SeaDragonSkeleton,
TechType.ReaperSkeleton,
TechType.CaveSkeleton,
TechType.HugeSkeleton,
TechType.PrecursorKey_PurpleFragment,
TechType.PrecursorKeyTerminal,
TechType.PrecursorTeleporter,
TechType.PrecursorEnergyCore,
TechType.PrecursorThermalPlant,
TechType.PrecursorWarper,
TechType.PrecursorFishSkeleton,
TechType.PrecursorScanner,
TechType.PrecursorLabCacheContainer1,
TechType.PrecursorLabCacheContainer2,
TechType.PrecursorLabTable,
TechType.PrecursorSeaDragonSkeleton,
TechType.PrecursorSensor,
TechType.PrecursorPrisonArtifact1,
TechType.PrecursorPrisonArtifact2,
TechType.PrecursorPrisonArtifact3,
TechType.PrecursorPrisonArtifact4,
TechType.PrecursorPrisonArtifact5,
TechType.PrecursorPrisonArtifact6,
TechType.PrecursorPrisonArtifact7,
TechType.PrecursorPrisonArtifact8,
TechType.PrecursorPrisonArtifact9,
TechType.PrecursorPrisonArtifact10,
TechType.PrecursorPrisonArtifact11,
TechType.PrecursorPrisonArtifact12,
TechType.PrecursorPipeRoomIncomingPipe,
TechType.PrecursorPipeRoomOutgoingPipe,
TechType.PrecursorPrisonLabEmperorFetus,
TechType.PrecursorPrisonLabEmperorEgg,
TechType.PrecursorPrisonAquariumPipe,
TechType.PrecursorPrisonAquariumFinalTeleporter,
TechType.PrecursorPrisonAquariumIncubatorEggs,
TechType.PrecursorPrisonAquariumIncubator,
TechType.PrecursorSurfacePipe,
TechType.PrecursorPrisonArtifact13,
TechType.PrecursorPrisonIonGenerator,
TechType.PrecursorPrisonOutposts,
TechType.ObservatoryOld,
TechType.PrecursorLostRiverBrokenAnchor,
TechType.PrecursorLostRiverLabRays,
TechType.PrecursorLostRiverLabBones,
TechType.PrecursorLostRiverLabEgg,
TechType.PrecursorLostRiverProductionLine,
TechType.PrecursorLostRiverWarperParts,
TechType.MembraneOld,
TechType.Unobtanium,
TechType.BaseRoom,
TechType.BaseHatch,
TechType.BaseWall,
TechType.BaseDoor,
TechType.BaseLadder,
TechType.BaseWindow,
TechType.PowerGeneratorOld,
TechType.UnusedOld,
TechType.BaseCorridor,
TechType.BaseFoundation,
TechType.BaseCorridorI,
TechType.BaseCorridorL,
TechType.BaseCorridorT,
TechType.BaseCorridorX,
TechType.BaseReinforcement,
TechType.BaseBulkhead,
TechType.BaseCorridorGlassI,
TechType.BaseCorridorGlassL,
TechType.BaseObservatory,
TechType.BaseConnector,
TechType.BaseMoonpool,
TechType.BaseCorridorGlass,
TechType.BaseUpgradeConsole,
TechType.BasePlanter,
TechType.BaseFiltrationMachine,
TechType.BaseWaterPark,
TechType.BaseMapRoom,
TechType.BaseBioReactor,
TechType.BaseNuclearReactor,
TechType.BasePipeConnector,
TechType.RocketBase,
TechType.RocketBaseLadder,
TechType.RocketStage1,
TechType.RocketStage2,
TechType.RocketStage3,
TechType.TimeCapsule,
TechType.DioramaHullPlate,
TechType.MarkiplierHullPlate,
TechType.MuyskermHullPlate,
TechType.LordMinionHullPlate,
TechType.JackSepticEyeHullPlate,
TechType.IGPHullPlate,
TechType.GilathissHullPlate,
TechType.Marki1,
TechType.Marki2,
TechType.JackSepticEye,
TechType.EatMyDiction,
TechType.RadiationLeakPoint,
TechType.SomethingPlaceholder,
TechType.Fragment,
TechType.CountOld,
TechType.Databox
        };

        internal static Action<bool> OnAntennaBuilt;
        private static TechType _seaBreezeTechType;

        internal static Dictionary<string, ServerData> Servers { get; set; } = new Dictionary<string, ServerData>();


        public static List<string> TrackedServers { get; set; } = new List<string>();
        #endregion

        #region Ingredients

#if SUBNAUTICA
        internal static TechData FloorMountedRackIngredients => new TechData
#elif BELOWZERO
                internal static RecipeData FloorMountedRackIngredients => new RecipeData
#endif
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(TechType.Glass, 1),
                new Ingredient(TechType.Titanium, 1),
                new Ingredient(TechType.WiringKit, 1),
                new Ingredient(TechType.Silicone, 1)
            }
        };

#if SUBNAUTICA
        internal static TechData WallMountedRackIngredients => new TechData
#elif BELOWZERO
                internal static RecipeData WallMountedRackHarvIngredients => new RecipeData
#endif
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(TechType.Glass, 1),
                new Ingredient(TechType.Titanium, 1),
                new Ingredient(TechType.WiringKit, 1),
                new Ingredient(TechType.Silicone, 1)
            }
        };

#if SUBNAUTICA
        internal static TechData TerminalIngredients => new TechData
#elif BELOWZERO
                internal static RecipeData TerminalIngredients => new RecipeData
#endif
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(TechType.Glass , 4),
                new Ingredient(TechType.AdvancedWiringKit, 1),
                new Ingredient(TechType.Titanium, 2),
                new Ingredient(TechType.MapRoomHUDChip, 1)
            }
        };

#if SUBNAUTICA
        internal static TechData AntennaIngredients => new TechData
#elif BELOWZERO
                internal static RecipeData AntennaIngredients => new RecipeData
#endif
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(TechType.Glass, 1),
                new Ingredient(TechType.AdvancedWiringKit, 1),
                new Ingredient(TechType.TitaniumIngot, 2),
                new Ingredient(TechType.MapRoomUpgradeScanRange, 1)
            }
        };

#if SUBNAUTICA
        internal static TechData ServerIngredients => new TechData
#elif BELOWZERO
                internal static RecipeData ServerIngredients => new RecipeData
#endif
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(TechType.WiringKit, 1),
                new Ingredient(TechType.ComputerChip, 1),
                new Ingredient(TechType.Aerogel, 1),
                new Ingredient(TechType.Titanium, 1)
            }
        };

#if SUBNAUTICA
        internal static TechData ServerFormattingStationIngredients => new TechData
#elif BELOWZERO
                internal static RecipeData ServerFormattingStationIngredients => new RecipeData
#endif
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(TechType.Glass, 1),
                new Ingredient(TechType.AdvancedWiringKit, 1),
                new Ingredient(TechType.Aerogel, 1),
                new Ingredient(TechType.Titanium, 1)
            }
        };

#if SUBNAUTICA
        internal static TechData DSSOperatorIngredients => new TechData
#elif BELOWZERO
                internal static RecipeData DSSOperatorIngredients => new RecipeData
#endif
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(TechType.Glass, 1),
                new Ingredient(TechType.AdvancedWiringKit, 1),
                new Ingredient(TechType.Titanium, 3)
            }
        };

#if SUBNAUTICA
        internal static TechData ItemDisplayIngredients => new TechData
#elif BELOWZERO
                internal static RecipeData ItemDisplayIngredients => new RecipeData
#endif
        {
            craftAmount = 1,
            Ingredients =
            {
                new Ingredient(TechType.Glass, 1),
                new Ingredient(TechType.WiringKit, 1),
                new Ingredient(TechType.Titanium, 2)
            }
        };

        internal static Action OnBaseUpdate { get; set; }
        internal static Action<DSSRackController> OnContainerUpdate { get; set; }
        internal static List<TechType> AllTechTypes = new List<TechType>();
        public static LootDistributionData LootDistributionData { get; set; }

        internal static event Action<SaveData> OnDataLoaded;
        #endregion

        #region Internal Methods
        
        internal static void Save()
        {
            if (!IsSaving())
            {
                _saveObject = new GameObject().AddComponent<ModSaver>();

                SaveData newSaveData = new SaveData();
                //GameObject.FindObjectsOfType<MonoBehaviour>().OfType<IDataSoluationsSave>();
                var controllers = GameObject.FindObjectsOfType<DataStorageSolutionsController>();

                foreach (var controller in controllers)
                {
                    controller.Save(newSaveData);
                }

                CleanServers();

                newSaveData.Servers = Servers;
                newSaveData.Bases = BaseManager.GetSaveData().ToList();
                _saveData = newSaveData;

                if (_saveData == null)
                {
                    QuickLogger.Error($"Save Failed for mod: {ModFriendlyName}");
                    return;
                }

                ModUtils.Save<SaveData>(_saveData, SaveDataFilename, GetSaveFileDirectory(), OnSaveComplete);
            }
        }

        private static void CleanServers()
        {
            var keysToRemove = Servers.Keys.Except(TrackedServers).ToList();

            foreach (var key in keysToRemove)
                Servers.Remove(key);
        }

        internal static bool IsSaving()
        {
            return _saveObject != null;
        }

        internal static void OnSaveComplete()
        {
            _saveObject.StartCoroutine(SaveCoroutine());
        }

        internal static string GetSaveFileDirectory()
        {
            return Path.Combine(SaveUtils.GetCurrentSaveDataDir(), ModName);
        }

        internal static SaveDataEntry GetSaveData(string id)
        {
            LoadData();

            var saveData = GetSaveData();

            foreach (var entry in saveData.Entries)
            {
                if (string.IsNullOrEmpty(entry.ID)) continue;

                if (entry.ID == id)
                {
                    return entry;
                }
            }

            return new SaveDataEntry() { ID = id };
        }

        internal static SaveData GetSaveData()
        {
            return _saveData ?? new SaveData();
        }

        internal static BaseSaveData GetBaseSaveData(string instanceId)
        {
            LoadData();

            var saveData = GetSaveData();

            if (saveData.Bases == null) return null;

            foreach (var entry in saveData.Bases)
            {
                if (string.IsNullOrEmpty(entry.InstanceID)) continue;

                if (entry.InstanceID == instanceId)
                {
                    return new BaseSaveData { BaseName = entry.BaseName, InstanceID = entry.InstanceID, AllowDocking = entry.AllowDocking, HasBreakerTripped = entry.HasBreakerTripped };
                }
            }

            return null;
        }

        internal static void LoadData()
        {
            if (_saveData != null) return;
            QuickLogger.Info("Loading Save Data...");
            ModUtils.LoadSaveData<SaveData>(SaveDataFilename, GetSaveFileDirectory(), (data) =>
            {
                _saveData = data;
                Servers = data.Servers;

                foreach (KeyValuePair<string, ServerData> objServer in data.Servers)
                {
                    QuickLogger.Debug($"Server Data: S={objServer.Value.Server?.Count} || F={objServer.Value.ServerFilters?.Count}");
                }

                QuickLogger.Info("Save Data Loaded");
                OnDataLoaded?.Invoke(_saveData);
            });
        }

        internal static string ConfigurationFile()
        {
            return Path.Combine(MODFOLDERLOCATION, ConfigFileName);
        }

        internal static bool IsConfigAvailable()
        {
            return File.Exists(ConfigurationFile());
        }

        internal static void CreateAllowedTechTypes()
        {
            if (!(AllTechTypes?.Count <= 0)) return;
            var items = Enum.GetValues(typeof(TechType));

            foreach (TechType techType in items)
            {
                if (BlackList.Contains(techType) || CraftData.IsBuildableTech(techType)) continue;
                AllTechTypes.Add(techType);
            }
        }
        #endregion

        #region Private Methods

        private static IEnumerator SaveCoroutine()
        {
            while (SaveLoadManager.main != null && SaveLoadManager.main.isSaving)
            {
                yield return null;
            }
            GameObject.DestroyImmediate(_saveObject.gameObject);
            _saveObject = null;
        }

        public static string GetAssetFolder()
        {
            return Path.Combine(GetModPath(), "Assets");
        }

        private static string GetModPath()
        {
            return Path.Combine(GetQModsPath(), ModFolderName);
        }

        private static string GetQModsPath()
        {
            return Path.Combine(Environment.CurrentDirectory, "QMods");
        }

        #endregion
        
        private static void CreateModConfiguration()
        {
            try
            {
                var config = new ConfigFile { Config = new Config() };

                var saveDataJson = JsonConvert.SerializeObject(config, Formatting.Indented);

                File.WriteAllText(Path.Combine(MODFOLDERLOCATION, ConfigFileName), saveDataJson);
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.StackTrace);
            }
        }

        private static ConfigFile LoadConfigurationData()
        {
            try
            {
                // == Load Configuration == //
                string configJson = File.ReadAllText(ConfigurationFile().Trim());

                JsonSerializerSettings settings = new JsonSerializerSettings();
                settings.MissingMemberHandling = MissingMemberHandling.Ignore;

                // == LoadData == //
                return JsonConvert.DeserializeObject<ConfigFile>(configJson, settings);
            }
            catch (Exception e)
            {
                QuickLogger.Error("Failed to load configuration. Using Default");
                QuickLogger.Error(e.StackTrace);
                return new ConfigFile();
            }
        }

        internal static ConfigFile LoadConfiguration()
        {
            if (!IsConfigAvailable())
            {
                CreateModConfiguration();
            }

            return LoadConfigurationData();
        }

        public static string GetAssetPath(string fileName)
        {
            return Path.Combine(GetAssetFolder(), fileName);
        }

        internal static void AddBlackListFilter(Filter filter)
        {
            var dockingList = QPatch.Configuration.Config.DockingBlackList;

            var result = dockingList.Any(x => x.IsSame(filter));

            if (!result)
            {
                dockingList.Add(filter);
            }

            if (!dockingList.Contains(filter))
            {

            }

            SaveModConfiguration();
        }

        internal static void RemoveBlackListFilter(Filter filter)
        {
            var dockingList = QPatch.Configuration.Config.DockingBlackList;

            foreach (Filter enabledFilter in dockingList)
            {
                if (enabledFilter.IsSame(filter))
                {
                    dockingList.Remove(enabledFilter);
                }
            }
            SaveModConfiguration();
        }

        internal static bool IsFilterAdded(Filter compareFilter)
        {
            foreach (Filter filter in QPatch.Configuration.Config.DockingBlackList)
            {
                if (filter.IsSame(compareFilter))
                {
                    QuickLogger.Debug("Filter is in the list", true);
                    return true;
                }
            }
            QuickLogger.Debug("Filter is not in the list", true);

            return false;
        }

        internal static bool IsFilterAddedWithType(TechType techType)
        {
            foreach (Filter filter in QPatch.Configuration.Config.DockingBlackList)
            {
                if (filter.IsTechTypeAllowed(techType))
                {
                    return true;
                }
            }

            return false;
        }

        internal static void SaveModConfiguration()
        {
            try
            {
                var saveDataJson = JsonConvert.SerializeObject(QPatch.Configuration, Formatting.Indented);

                File.WriteAllText(Path.Combine(MODFOLDERLOCATION, ConfigurationFile()), saveDataJson);
            }
            catch (Exception e)
            {
                QuickLogger.Error($"{e.Message}\n{e.StackTrace}");
            }
        }

        public static TechType GetSeaBreezeTechType()
        {
            if (_seaBreezeTechType == TechType.None)
            {
                _seaBreezeTechType = "ARSSeaBreezeFCS32".ToTechType();
            }

            return _seaBreezeTechType;
        }
    }

    internal class Config
    {
        [JsonProperty] internal int ServerStorageLimit { get; set; } = 48;
        [JsonProperty] internal float AntennaPowerUsage { get; set; } = 0.1f;
        [JsonProperty] internal float ScreenPowerUsage { get; set; } = 0.1f;
        [JsonProperty] internal bool ShowAllItems { get; set; }
        [JsonProperty] internal float RackPowerUsage { get; set; } = 0.1f;
        [JsonProperty] internal float ServerPowerUsage { get; set; } = 0.05f;
        [JsonProperty] internal bool PullFromDockedVehicles { get; set; } = true;
        [JsonProperty] internal float CheckVehiclesInterval { get; set; } = 2.0f;
        [JsonProperty] internal int ExtractMultiplier { get; set; }

        [JsonProperty] internal float ExtractInterval = 0.25f;
        [JsonProperty] internal bool AllowFood { get; set; }

        private HashSet<Filter> _dockingBlackList = new HashSet<Filter>();

        [JsonProperty]
        internal HashSet<Filter> DockingBlackList
        {
            get => _dockingBlackList;
            set
            {
                _dockingBlackList = FilterList.GetNewVersion(value);
                Mod.SaveModConfiguration();
            }
        }
    }

    internal class ConfigFile
    {
        [JsonProperty] internal Config Config { get; set; }
    }

    internal class Options : ModOptions
    {
        //private ModModes _modMode;
        private const string ExtractMultiplierID = "DSSEMulti";
        //private const string AllowFoodToggle = "DSSAllowFood";


        public Options() : base("Data Storage Solutions Settings")
        {
            ChoiceChanged += Options_ChoiceChanged;
            //ToggleChanged += Options_ToggleChanged;
        }

        //private void Options_ToggleChanged(object sender, ToggleChangedEventArgs e)
        //{
        //    switch (e.Id)
        //    {
        //        case ExtractMultiplierID:
        //            QPatch.Configuration.Config.AllowFood = e.Value;
        //            break;
        //    }

        //    Mod.SaveModConfiguration();
        //}

        private void Options_ChoiceChanged(object sender, ChoiceChangedEventArgs e)
        {
            switch (e.Id)
            {
                case ExtractMultiplierID:
                    QPatch.Configuration.Config.ExtractMultiplier = e.Index;
                    break;
            }

            Mod.SaveModConfiguration();
        }

        public override void BuildModOptions()
        {
            AddChoiceOption(ExtractMultiplierID, "Extract Multiplier", new[]
            {
                "x0",
                "x5",
                "x10",
                "x20"
            }, QPatch.Configuration.Config.ExtractMultiplier);

            //AddToggleOption(AllowFoodToggle, "Allow Food", QPatch.Configuration.Config.AllowFood);
        }
    }
}

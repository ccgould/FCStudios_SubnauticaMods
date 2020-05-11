using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using DataStorageSolutions.Configuration;
using Oculus.Newtonsoft.Json;

namespace DataStorageSolutions.Model
{
    [Serializable]
    public enum Category
    {
        None,
        Food,
        Water,
        PlantsAndSeeds,
        Metals,
        Electronics,
        Batteries,
        NaturalMaterials,
        SyntheticMaterials,
        CrystalMaterials,
        Fish,
        Eggs,
        Tools,
        Equipment,
        MysteriousTablets,
        ScannerRoomUpgrades,
        GeneralUpgrades,
        SeamothUpgrades,
        PrawnSuitUpgrades,
        CyclopsUpgrades,
        Torpedoes,
        AlterraStuff,
    }

    public static class CategoryData
    {
        public static List<TechType> Fish = new List<TechType> {
            TechType.Bladderfish,
            TechType.Boomerang,
            TechType.LavaBoomerang,
            TechType.Eyeye,
            TechType.LavaEyeye,
            TechType.GarryFish,
            TechType.HoleFish,
            TechType.Hoopfish,
            TechType.Spinefish,
            TechType.Hoverfish,
            TechType.Oculus,
            TechType.Peeper,
            TechType.Reginald,
            TechType.Spadefish,
        };

        public static List<TechType> AlterraArtifacts = new List<TechType> {
            TechType.LabContainer,
            TechType.LabContainer2,
            TechType.LabContainer3,
            TechType.ArcadeGorgetoy,
            TechType.Cap1,
            TechType.Cap2,
            TechType.LabEquipment1,
            TechType.LabEquipment2,
            TechType.LabEquipment3,
            TechType.LEDLightFragment,
            TechType.StarshipSouvenir,
            TechType.Poster,
            TechType.PosterAurora,
            TechType.PosterExoSuit1,
            TechType.PosterExoSuit2,
            TechType.PosterKitty,
        };

        public static List<TechType> MysteriousTablets = new List<TechType> {
            TechType.PrecursorKey_Blue,
            TechType.PrecursorKey_Orange,
            TechType.PrecursorKey_Purple,
        };

        public static List<TechType> CreatureEggs = new List<TechType> {
            TechType.BonesharkEgg,
            TechType.CrabsnakeEgg,
            TechType.CrabsquidEgg,
            TechType.CrashEgg,
            TechType.CutefishEgg,
            TechType.GasopodEgg,
            TechType.JellyrayEgg,
            TechType.JumperEgg,
            TechType.LavaLizardEgg,
            TechType.MesmerEgg,
            TechType.RabbitrayEgg,
            TechType.ReefbackEgg,
            TechType.SandsharkEgg,
            TechType.ShockerEgg,
            TechType.SpadefishEgg,
            TechType.StalkerEgg,
            TechType.GrandReefsEgg,
            TechType.GrassyPlateausEgg,
            TechType.KelpForestEgg,
            TechType.KooshZoneEgg,
            TechType.LavaZoneEgg,
            TechType.MushroomForestEgg,
            TechType.SafeShallowsEgg,
            TechType.TwistyBridgesEgg,
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
            TechType.GenericEgg,
            TechType.CrashEgg,
            TechType.CrashEggUndiscovered,
            TechType.CrabsquidEgg,
            TechType.CrabsquidEggUndiscovered,
            TechType.CutefishEgg,
            TechType.CutefishEggUndiscovered,
            TechType.LavaLizardEgg,
            TechType.LavaLizardEggUndiscovered,
            TechType.CrabsnakeEggUndiscovered,
            TechType.SpadefishEggUndiscovered
        };

        public static List<TechType> Food = new List<TechType> {
            TechType.CookedBladderfish,
            TechType.CookedBoomerang,
            TechType.CookedEyeye,
            TechType.CookedGarryFish,
            TechType.CookedHoleFish,
            TechType.CookedHoopfish,
            TechType.CookedHoverfish,
            TechType.CookedLavaBoomerang,
            TechType.CookedLavaEyeye,
            TechType.CookedOculus,
            TechType.CookedPeeper,
            TechType.CookedReginald,
            TechType.CookedSpadefish,
            TechType.CookedSpinefish,
            TechType.CuredBladderfish,
            TechType.CuredBoomerang,
            TechType.CuredEyeye,
            TechType.CuredGarryFish,
            TechType.CuredHoleFish,
            TechType.CuredHoopfish,
            TechType.CuredHoverfish,
            TechType.CuredLavaBoomerang,
            TechType.CuredLavaEyeye,
            TechType.CuredOculus,
            TechType.CuredPeeper,
            TechType.CuredReginald,
            TechType.CuredSpadefish,
            TechType.CuredSpinefish,
            TechType.NutrientBlock,
            TechType.Snack1,
            TechType.Snack2,
            TechType.Snack3,
            TechType.BulboTreePiece,
            TechType.HangingFruit,
            TechType.Melon,
            TechType.PurpleVegetable,
        };

        public static List<TechType> Water = new List<TechType> {
            TechType.BigFilteredWater,
            TechType.Coffee,
            TechType.DisinfectedWater,
            TechType.FilteredWater,
            TechType.StillsuitWater,
        };

        public static List<TechType> ScannerRoomUpgrades = new List<TechType> {
            TechType.MapRoomUpgradeScanRange,
            TechType.MapRoomUpgradeScanSpeed,
            TechType.MapRoomCamera,
        };

        public static List<TechType> CyclopsUpgrades = new List<TechType> {
            TechType.CyclopsDecoyModule,
            TechType.CyclopsFireSuppressionModule,
            TechType.CyclopsHullModule1,
            TechType.CyclopsHullModule2,
            TechType.CyclopsHullModule3,
            TechType.CyclopsSeamothRepairModule,
            TechType.CyclopsShieldModule,
            TechType.CyclopsSonarModule,
            TechType.CyclopsThermalReactorModule,
        };

        public static List<TechType> PrawnSuitUpgrades = new List<TechType> {
            TechType.ExoHullModule1,
            TechType.ExoHullModule2,
            TechType.ExosuitDrillArmModule,
            TechType.ExosuitGrapplingArmModule,
            TechType.ExosuitJetUpgradeModule,
            TechType.ExosuitPropulsionArmModule,
            TechType.ExosuitThermalReactorModule,
            TechType.ExosuitTorpedoArmModule,
        };

        public static List<TechType> SeamothUpgrades = new List<TechType> {
            TechType.SeamothElectricalDefense,
            TechType.SeamothReinforcementModule,
            TechType.SeamothSolarCharge,
            TechType.SeamothSonarModule,
            TechType.SeamothTorpedoModule,
        };

        public static List<TechType> GeneralUpgrades = new List<TechType> {
            TechType.HullReinforcementModule,
            TechType.PowerUpgradeModule,
            TechType.VehicleArmorPlating,
            TechType.VehicleHullModule1,
            TechType.VehicleHullModule2,
            TechType.VehicleHullModule3,
            TechType.VehiclePowerUpgradeModule,
            TechType.VehicleStorageModule,
        };

        public static List<TechType> Equipment = new List<TechType> {
            TechType.MapRoomHUDChip,
            TechType.Rebreather,
            TechType.Compass,
            TechType.Fins,
            TechType.HighCapacityTank,
            TechType.PlasteelTank,
            TechType.RadiationGloves,
            TechType.RadiationHelmet,
            TechType.RadiationSuit,
            TechType.ReinforcedDiveSuit,
            TechType.ReinforcedGloves,
            TechType.Stillsuit,
            TechType.SwimChargeFins,
            TechType.Tank,
            TechType.UltraGlideFins,
        };

        public static List<TechType> Tools = new List<TechType> {
            TechType.AirBladder,
            TechType.Beacon,
            TechType.Builder,
            TechType.CyclopsDecoy,
            TechType.DiamondBlade,
            TechType.DiveReel,
            TechType.DoubleTank,
            TechType.FireExtinguisher,
            TechType.Flare,
            TechType.Flashlight,
            TechType.Gravsphere,
            TechType.HeatBlade,
            TechType.Knife,
            TechType.LaserCutter,
            TechType.LEDLight,
            TechType.Pipe,
            TechType.PipeSurfaceFloater,
            TechType.PropulsionCannon,
            TechType.RepulsionCannon,
            TechType.Scanner,
            TechType.Seaglide,
            TechType.SmallStorage,
            TechType.StasisRifle,
            TechType.Welder,
            TechType.LuggageBag,
        };

        public static List<TechType> Torpedoes = new List<TechType> {
            TechType.GasTorpedo,
            TechType.WhirlpoolTorpedo
        };

        public static List<TechType> PlantsAndSeeds = new List<TechType> {
            TechType.AcidMushroomSpore,
            TechType.BluePalmSeed,
            TechType.BulboTreePiece,
            TechType.CreepvinePiece,
            TechType.CreepvineSeedCluster,
            TechType.EyesPlantSeed,
            TechType.FernPalmSeed,
            TechType.GabeSFeatherSeed,
            TechType.HangingFruit,
            TechType.JellyPlantSeed,
            TechType.KooshChunk,
            TechType.Melon,
            TechType.MelonSeed,
            TechType.MembrainTreeSeed,
            TechType.OrangeMushroomSpore,
            TechType.OrangePetalsPlantSeed,
            TechType.PinkFlowerSeed,
            TechType.PinkMushroomSpore,
            TechType.PurpleBrainCoralPiece,
            TechType.PurpleBranchesSeed,
            TechType.PurpleFanSeed,
            TechType.PurpleRattleSpore,
            TechType.PurpleStalkSeed,
            TechType.PurpleTentacleSeed,
            TechType.PurpleVasePlantSeed,
            TechType.PurpleVegetable,
            TechType.RedBasketPlantSeed,
            TechType.RedBushSeed,
            TechType.RedConePlantSeed,
            TechType.RedGreenTentacleSeed,
            TechType.RedRollPlantSeed,
            TechType.SeaCrownSeed,
            TechType.ShellGrassSeed,
            TechType.SmallFanSeed,
            TechType.SmallMelon,
            TechType.SnakeMushroomSpore,
            TechType.SpikePlantSeed,
            TechType.SpottedLeavesPlantSeed,
            TechType.WhiteMushroomSpore,
        };

        public static List<TechType> Metals = new List<TechType> {
            TechType.Copper,
            TechType.Gold,
            TechType.Lead,
            TechType.Lithium,
            TechType.Magnetite,
            TechType.ScrapMetal,
            TechType.Nickel,
            TechType.PlasteelIngot,
            TechType.Silver,
            TechType.Titanium,
            TechType.TitaniumIngot,
        };

        public static List<TechType> NaturalMaterials = new List<TechType> {
            TechType.GasPod,
            TechType.CoralChunk,
            TechType.WhiteMushroom,
            TechType.AcidMushroom,
            TechType.JeweledDiskPiece,
            TechType.BloodOil,
            TechType.CrashPowder,
            TechType.Salt,
            TechType.SeaTreaderPoop,
            TechType.StalkerTooth,
            TechType.JellyPlant,
            TechType.Glass,
            TechType.EnameledGlass,
        };

        public static List<TechType> Electronics = new List<TechType> {
            TechType.AdvancedWiringKit,
            TechType.ComputerChip,
            TechType.CopperWire,
            TechType.DepletedReactorRod,
            TechType.ReactorRod,
            TechType.WiringKit,
        };

        public static List<TechType> SyntheticMaterials = new List<TechType> {
            TechType.Aerogel,
            TechType.AramidFibers,
            TechType.Benzene,
            TechType.Bleach,
            TechType.FiberMesh,
            TechType.HatchingEnzymes,
            TechType.HydrochloricAcid,
            TechType.Lubricant,
            TechType.Polyaniline,
            TechType.PrecursorIonCrystal,
            TechType.Silicone,
        };

        public static List<TechType> CrystalMaterials = new List<TechType> {
            TechType.AluminumOxide,
            TechType.Diamond,
            TechType.Kyanite,
            TechType.Quartz,
            TechType.Sulphur,
            TechType.UraniniteCrystal,
        };

        public static List<TechType> Batteries = new List<TechType> {
            TechType.Battery,
            TechType.PowerCell,
            TechType.PrecursorIonBattery,
            TechType.PrecursorIonPowerCell,
        };

        public static List<TechType> IndividualItems = new List<TechType> {
            TechType.GasPod,
            TechType.CoralChunk,
            TechType.WhiteMushroom,
            TechType.AcidMushroom,
            TechType.JeweledDiskPiece,
            TechType.AdvancedWiringKit,
            TechType.Aerogel,
            TechType.AluminumOxide,
            TechType.AramidFibers,
            TechType.Benzene,
            TechType.Bleach,
            TechType.BloodOil,
            TechType.ComputerChip,
            TechType.Copper,
            TechType.CopperWire,
            TechType.CrashPowder,
            TechType.DepletedReactorRod,
            TechType.Diamond,
            TechType.EnameledGlass,
            TechType.FiberMesh,
            TechType.FirstAidKit,
            TechType.Glass,
            TechType.Gold,
            TechType.HatchingEnzymes,
            TechType.HydrochloricAcid,
            TechType.JellyPlant,
            TechType.Kyanite,
            TechType.Lead,
            TechType.Lithium,
            TechType.Lubricant,
            TechType.Magnetite,
            TechType.ScrapMetal,
            TechType.Nickel,
            TechType.PlasteelIngot,
            TechType.Polyaniline,
            TechType.PrecursorIonCrystal,
            TechType.Quartz,
            TechType.ReactorRod,
            TechType.Salt,
            TechType.SeaTreaderPoop,
            TechType.Silicone,
            TechType.Silver,
            TechType.StalkerTooth,
            TechType.Sulphur,
            TechType.Titanium,
            TechType.TitaniumIngot,
            TechType.UraniniteCrystal,
            TechType.WiringKit,
            TechType.Battery,
            TechType.PowerCell,
            TechType.PrecursorIonBattery,
            TechType.PrecursorIonPowerCell,
        };

        public static List<TechType> AllItems = new List<TechType> {
            TechType.GasPod,
            TechType.CoralChunk,
            TechType.WhiteMushroom,
            TechType.AcidMushroom,
            TechType.JeweledDiskPiece,
            TechType.AdvancedWiringKit,
            TechType.Aerogel,
            TechType.AluminumOxide,
            TechType.AramidFibers,
            TechType.Benzene,
            TechType.Bleach,
            TechType.BloodOil,
            TechType.ComputerChip,
            TechType.Copper,
            TechType.CopperWire,
            TechType.CrashPowder,
            TechType.DepletedReactorRod,
            TechType.Diamond,
            TechType.EnameledGlass,
            TechType.FiberMesh,
            TechType.FirstAidKit,
            TechType.Glass,
            TechType.Gold,
            TechType.HatchingEnzymes,
            TechType.HydrochloricAcid,
            TechType.JellyPlant,
            TechType.Kyanite,
            TechType.Lead,
            TechType.Lithium,
            TechType.Lubricant,
            TechType.Magnetite,
            TechType.ScrapMetal,
            TechType.Nickel,
            TechType.PlasteelIngot,
            TechType.Polyaniline,
            TechType.PrecursorIonCrystal,
            TechType.Quartz,
            TechType.ReactorRod,
            TechType.Salt,
            TechType.SeaTreaderPoop,
            TechType.Silicone,
            TechType.Silver,
            TechType.StalkerTooth,
            TechType.Sulphur,
            TechType.Titanium,
            TechType.TitaniumIngot,
            TechType.UraniniteCrystal,
            TechType.WiringKit,
            TechType.Battery,
            TechType.PowerCell,
            TechType.PrecursorIonBattery,
            TechType.PrecursorIonPowerCell,
            TechType.AcidMushroomSpore,
            TechType.BluePalmSeed,
            TechType.BulboTreePiece,
            TechType.CreepvinePiece,
            TechType.CreepvineSeedCluster,
            TechType.EyesPlantSeed,
            TechType.FernPalmSeed,
            TechType.GabeSFeatherSeed,
            TechType.HangingFruit,
            TechType.JellyPlantSeed,
            TechType.KooshChunk,
            TechType.Melon,
            TechType.MelonSeed,
            TechType.MembrainTreeSeed,
            TechType.OrangeMushroomSpore,
            TechType.OrangePetalsPlantSeed,
            TechType.PinkFlowerSeed,
            TechType.PinkMushroomSpore,
            TechType.PurpleBrainCoralPiece,
            TechType.PurpleBranchesSeed,
            TechType.PurpleFanSeed,
            TechType.PurpleRattleSpore,
            TechType.PurpleStalkSeed,
            TechType.PurpleTentacleSeed,
            TechType.PurpleVasePlantSeed,
            TechType.PurpleVegetable,
            TechType.RedBasketPlantSeed,
            TechType.RedBushSeed,
            TechType.RedConePlantSeed,
            TechType.RedGreenTentacleSeed,
            TechType.RedRollPlantSeed,
            TechType.SeaCrownSeed,
            TechType.ShellGrassSeed,
            TechType.SmallFanSeed,
            TechType.SmallMelon,
            TechType.SnakeMushroomSpore,
            TechType.SpikePlantSeed,
            TechType.SpottedLeavesPlantSeed,
            TechType.WhiteMushroomSpore,
            TechType.GasTorpedo,
            TechType.WhirlpoolTorpedo,
            TechType.AirBladder,
            TechType.Beacon,
            TechType.Builder,
            TechType.CyclopsDecoy,
            TechType.DiamondBlade,
            TechType.DiveReel,
            TechType.DoubleTank,
            TechType.FireExtinguisher,
            TechType.Flare,
            TechType.Flashlight,
            TechType.Gravsphere,
            TechType.HeatBlade,
            TechType.Knife,
            TechType.LaserCutter,
            TechType.LEDLight,
            TechType.Pipe,
            TechType.PipeSurfaceFloater,
            TechType.PropulsionCannon,
            TechType.RepulsionCannon,
            TechType.Scanner,
            TechType.Seaglide,
            TechType.SmallStorage,
            TechType.StasisRifle,
            TechType.Welder,
            TechType.LuggageBag,
            TechType.MapRoomHUDChip,
            TechType.Rebreather,
            TechType.Compass,
            TechType.Fins,
            TechType.HighCapacityTank,
            TechType.PlasteelTank,
            TechType.RadiationGloves,
            TechType.RadiationHelmet,
            TechType.RadiationSuit,
            TechType.ReinforcedDiveSuit,
            TechType.ReinforcedGloves,
            TechType.Stillsuit,
            TechType.SwimChargeFins,
            TechType.Tank,
            TechType.UltraGlideFins,
            TechType.HullReinforcementModule,
            TechType.PowerUpgradeModule,
            TechType.VehicleArmorPlating,
            TechType.VehicleHullModule1,
            TechType.VehicleHullModule2,
            TechType.VehicleHullModule3,
            TechType.VehiclePowerUpgradeModule,
            TechType.VehicleStorageModule,
            TechType.SeamothElectricalDefense,
            TechType.SeamothReinforcementModule,
            TechType.SeamothSolarCharge,
            TechType.SeamothSonarModule,
            TechType.SeamothTorpedoModule,
            TechType.ExoHullModule1,
            TechType.ExoHullModule2,
            TechType.ExosuitDrillArmModule,
            TechType.ExosuitGrapplingArmModule,
            TechType.ExosuitJetUpgradeModule,
            TechType.ExosuitPropulsionArmModule,
            TechType.ExosuitThermalReactorModule,
            TechType.ExosuitTorpedoArmModule,
            TechType.CyclopsDecoyModule,
            TechType.CyclopsFireSuppressionModule,
            TechType.CyclopsHullModule1,
            TechType.CyclopsHullModule2,
            TechType.CyclopsHullModule3,
            TechType.CyclopsSeamothRepairModule,
            TechType.CyclopsShieldModule,
            TechType.CyclopsSonarModule,
            TechType.CyclopsThermalReactorModule,
            TechType.MapRoomUpgradeScanRange,
            TechType.MapRoomUpgradeScanSpeed,
            TechType.MapRoomCamera,
            TechType.BigFilteredWater,
            TechType.Coffee,
            TechType.DisinfectedWater,
            TechType.FilteredWater,
            TechType.StillsuitWater,
            TechType.CookedBladderfish,
            TechType.CookedBoomerang,
            TechType.CookedEyeye,
            TechType.CookedGarryFish,
            TechType.CookedHoleFish,
            TechType.CookedHoopfish,
            TechType.CookedHoverfish,
            TechType.CookedLavaBoomerang,
            TechType.CookedLavaEyeye,
            TechType.CookedOculus,
            TechType.CookedPeeper,
            TechType.CookedReginald,
            TechType.CookedSpadefish,
            TechType.CookedSpinefish,
            TechType.CuredBladderfish,
            TechType.CuredBoomerang,
            TechType.CuredEyeye,
            TechType.CuredGarryFish,
            TechType.CuredHoleFish,
            TechType.CuredHoopfish,
            TechType.CuredHoverfish,
            TechType.CuredLavaBoomerang,
            TechType.CuredLavaEyeye,
            TechType.CuredOculus,
            TechType.CuredPeeper,
            TechType.CuredReginald,
            TechType.CuredSpadefish,
            TechType.CuredSpinefish,
            TechType.NutrientBlock,
            TechType.Snack1,
            TechType.Snack2,
            TechType.Snack3,
            TechType.BulboTreePiece,
            TechType.HangingFruit,
            TechType.Melon,
            TechType.PurpleVegetable,
            TechType.BonesharkEgg,
            TechType.CrabsnakeEgg,
            TechType.CrabsquidEgg,
            TechType.CrashEgg,
            TechType.CutefishEgg,
            TechType.GasopodEgg,
            TechType.JellyrayEgg,
            TechType.JumperEgg,
            TechType.LavaLizardEgg,
            TechType.MesmerEgg,
            TechType.RabbitrayEgg,
            TechType.ReefbackEgg,
            TechType.SandsharkEgg,
            TechType.ShockerEgg,
            TechType.SpadefishEgg,
            TechType.StalkerEgg,
            TechType.GrandReefsEgg,
            TechType.GrassyPlateausEgg,
            TechType.KelpForestEgg,
            TechType.KooshZoneEgg,
            TechType.LavaZoneEgg,
            TechType.MushroomForestEgg,
            TechType.SafeShallowsEgg,
            TechType.TwistyBridgesEgg,
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
            TechType.GenericEgg,
            TechType.CrashEgg,
            TechType.CrashEggUndiscovered,
            TechType.CrabsquidEgg,
            TechType.CrabsquidEggUndiscovered,
            TechType.CutefishEgg,
            TechType.CutefishEggUndiscovered,
            TechType.LavaLizardEgg,
            TechType.LavaLizardEggUndiscovered,
            TechType.CrabsnakeEggUndiscovered,
            TechType.SpadefishEggUndiscovered,
            TechType.PrecursorKey_Blue,
            TechType.PrecursorKey_Orange,
            TechType.PrecursorKey_Purple,
            TechType.LabContainer,
            TechType.LabContainer2,
            TechType.LabContainer3,
            TechType.ArcadeGorgetoy,
            TechType.Cap1,
            TechType.Cap2,
            TechType.LabEquipment1,
            TechType.LabEquipment2,
            TechType.LabEquipment3,
            TechType.LEDLightFragment,
            TechType.StarshipSouvenir,
            TechType.Poster,
            TechType.PosterAurora,
            TechType.PosterExoSuit1,
            TechType.PosterExoSuit2,
            TechType.PosterKitty,
            TechType.Bladderfish,
            TechType.Boomerang,
            TechType.LavaBoomerang,
            TechType.Eyeye,
            TechType.LavaEyeye,
            TechType.GarryFish,
            TechType.HoleFish,
            TechType.Hoopfish,
            TechType.Spinefish,
            TechType.Hoverfish,
            TechType.Oculus,
            TechType.Peeper,
            TechType.Reginald,
            TechType.Spadefish,
        };
    }

    [Serializable]
    public class Filter
    {
        public string Category;

        public List<TechType> Types = new List<TechType>();

        public bool IsCategory() => !string.IsNullOrEmpty(Category);

        public string GetString()
        {
            if (IsCategory())
            {
                return Category;
            }

            var textInfo = (new CultureInfo("en-US", false)).TextInfo;
            return textInfo.ToTitleCase(Language.main.Get(Types[0]));
        }

        public bool IsTechTypeAllowed(TechType techType)
        {
            return Types.Contains(techType);
        }

        public bool IsSame(Filter other)
        {
            return Category == other.Category && Types.Count > 0 && Types.Count == other.Types.Count && Types[0] == other.Types[0];
        }
    }

    [Serializable]
    public static class FilterList
    {
        public static List<Filter> Filters;

        public static List<Filter> GetFilters()
        {
            if (Filters == null)
            {
                InitializeFilters();
            }
            return Filters;
        }

        public static List<TechType> GetOldFilter(string oldCategory, out bool success, out string newCategory)
        {
            var category = Category.None;
            if (!int.TryParse(oldCategory, out int oldCategoryInt))
            {
                newCategory = "";
                success = false;
                return new List<TechType>();
            }
            category = (Category)oldCategoryInt;
            newCategory = category.ToString();

            success = true;
            switch (category)
            {
                default:
                    return CategoryData.IndividualItems;

                case Category.Food: return CategoryData.Food;
                case Category.Water: return CategoryData.Water;
                case Category.PlantsAndSeeds: return CategoryData.PlantsAndSeeds;
                case Category.Metals: return CategoryData.Metals;
                case Category.NaturalMaterials: return CategoryData.NaturalMaterials;
                case Category.SyntheticMaterials: return CategoryData.SyntheticMaterials;
                case Category.Electronics: return CategoryData.Electronics;
                case Category.CrystalMaterials: return CategoryData.CrystalMaterials;
                case Category.Batteries: return CategoryData.Batteries;
                case Category.Fish: return CategoryData.Fish;
                case Category.Eggs: return CategoryData.CreatureEggs;
                case Category.Tools: return CategoryData.Tools;
                case Category.Equipment: return CategoryData.Equipment;
                case Category.MysteriousTablets: return CategoryData.MysteriousTablets;
                case Category.ScannerRoomUpgrades: return CategoryData.ScannerRoomUpgrades;
                case Category.GeneralUpgrades: return CategoryData.GeneralUpgrades;
                case Category.SeamothUpgrades: return CategoryData.SeamothUpgrades;
                case Category.PrawnSuitUpgrades: return CategoryData.PrawnSuitUpgrades;
                case Category.CyclopsUpgrades: return CategoryData.CyclopsUpgrades;
                case Category.Torpedoes: return CategoryData.Torpedoes;
                case Category.AlterraStuff: return CategoryData.AlterraArtifacts;
            }
        }

        public static List<Filter> GetNewVersion(List<Filter> filterData)
        {
            Dictionary<string, Filter> validCategories = new Dictionary<string, Filter>();

            var filterList = GetFilters();

            foreach (var filter in filterList)
            {
                if (filter.IsCategory())
                {
                    validCategories[filter.Category] = filter;
                }
            }

            var newData = new List<Filter>();

            foreach (var filter in filterData)
            {
                if (validCategories.ContainsKey(filter.Category))
                {
                    newData.Add(validCategories[filter.Category]);
                    continue;
                }

                if (filter.Category == "")
                {
                    newData.Add(filter);
                    continue;
                }

                if (filter.Category == "0")
                {
                    filter.Category = "";
                    newData.Add(filter);
                    continue;
                }

                var newTypes = GetOldFilter(filter.Category, out bool success, out string newCategory);
                
                if (success)
                {
                    newData.Add(new Filter{ Category = newCategory, Types = newTypes });
                    continue;
                }

                newData.Add(filter);
            }
            return newData;
        }

        [Serializable]
        private class TypeReference
        {
            public string Name = "";
            public TechType Value = TechType.None;
        }

        private static void InitializeFilters()
        {
            var path = Mod.GetAssetPath("filters.json");
            var file = JsonConvert.DeserializeObject<List<Filter>>(File.ReadAllText(path));
            Filters = file.Where((f) => f.IsCategory()).ToList();

            if (QPatch.Configuration.Config.ShowAllItems)
            {
                var typeRefPath = Mod.GetAssetPath("type_reference.json");
                List<TypeReference> typeReferences =
                    JsonConvert.DeserializeObject<List<TypeReference>>(File.ReadAllText(typeRefPath));
                typeReferences.Sort((TypeReference a, TypeReference b) =>
                {
                    string aName = Language.main.Get(a.Value);
                    string bName = Language.main.Get(b.Value);
                    return string.Compare(aName.ToLowerInvariant(), bName.ToLowerInvariant(), StringComparison.Ordinal);
                });

                foreach (var typeRef in typeReferences)
                {
                    Filters.Add(new Filter() { Category = "", Types = new List<TechType> { typeRef.Value } });
                }
                return;
            }
            var sorted = file.Where(f => !f.IsCategory()).ToList();
            sorted.Sort((x, y) =>
            {
                string xName = Language.main.Get(x.Types.First());
                string yName = Language.main.Get(y.Types.First());
                return string.Compare(xName.ToLowerInvariant(), yName.ToLowerInvariant(), StringComparison.Ordinal);
            });
            foreach (var filter in sorted)
            {
                Filters.Add(filter);
            }
        }

        private static void AddEntry(string category, List<TechType> types)
        {
            Filters.Add(new Filter
            {
                Category = category,
                Types = types
            });
        }

        private static void AddEntry(TechType type)
        {
            Filters.Add(new Filter
            {
                Category = "",
                Types = new List<TechType> { type }
            });
        }
    }
}
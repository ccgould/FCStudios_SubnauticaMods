﻿using System.Collections.Generic;

namespace FCS_EnergySolutions.ModItems.Buildables.AlterraGen.Enumerators;
internal static class FuelTypes
{
    internal static readonly Dictionary<TechType, float> Charge = new Dictionary<TechType, float>(TechTypeExtensions.sTechTypeComparer)
    {
        {
            TechType.Melon,
            420f
        },
        {
            TechType.PurpleVegetable,
            140f
        },
        {
            TechType.HangingFruit,
            210f
        },
        {
            TechType.SmallMelon,
            280f
        },
        {
            TechType.JellyPlant,
            245f
        },
        {
            TechType.BulboTreePiece,
            420f
        },
        {
            TechType.KooshChunk,
            420f
        },
        {
            TechType.CreepvinePiece,
            210f
        },
        {
            TechType.WhiteMushroom,
            210f
        },
        {
            TechType.AcidMushroom,
            210f
        },
        {
            TechType.SmallFan,
            70f
        },
        {
            TechType.PurpleRattle,
            140f
        },
        {
            TechType.PinkMushroom,
            105f
        },
        {
            TechType.BloodVine,
            70f
        },
        {
            TechType.BluePalm,
            70f
        },
        {
            TechType.BulboTree,
            70f
        },
        {
            TechType.Creepvine,
            70f
        },
        {
            TechType.EyesPlant,
            70f
        },
        {
            TechType.FernPalm,
            70f
        },
        {
            TechType.GabeSFeather,
            70f
        },
        {
            TechType.HangingFruitTree,
            70f
        },
        {
            TechType.SmallKoosh,
            70f
        },
        {
            TechType.MembrainTree,
            70f
        },
        {
            TechType.OrangeMushroom,
            70f
        },
        {
            TechType.OrangePetalsPlant,
            140f
        },
        {
            TechType.PinkFlower,
            70f
        },
        {
            TechType.PurpleBrainCoral,
            70f
        },
        {
            TechType.PurpleBranches,
            70f
        },
        {
            TechType.PurpleFan,
            70f
        },
        {
            TechType.PurpleStalk,
            70f
        },
        {
            TechType.PurpleTentacle,
            70f
        },
        {
            TechType.PurpleVasePlant,
            70f
        },
        {
            TechType.PurpleVegetablePlant,
            70f
        },
        {
            TechType.RedBasketPlant,
            70f
        },
        {
            TechType.RedBush,
            70f
        },
        {
            TechType.RedConePlant,
            70f
        },
        {
            TechType.RedGreenTentacle,
            70f
        },
        {
            TechType.RedRollPlant,
            70f
        },
        {
            TechType.SeaCrown,
            70f
        },
        {
            TechType.ShellGrass,
            70f
        },
        {
            TechType.SnakeMushroom,
            70f
        },
        {
            TechType.SpikePlant,
            70f
        },
        {
            TechType.SpottedLeavesPlant,
            70f
        },
        {
            TechType.JeweledDiskPiece,
            70f
        },
        {
            TechType.CoralChunk,
            70f
        },
        {
            TechType.StalkerTooth,
            70f
        },
        {
            TechType.TreeMushroomPiece,
            70f
        },
        {
            TechType.OrangeMushroomSpore,
            140f
        },
        {
            TechType.PurpleVasePlantSeed,
            140f
        },
        {
            TechType.AcidMushroomSpore,
            21f
        },
        {
            TechType.WhiteMushroomSpore,
            21f
        },
        {
            TechType.PinkMushroomSpore,
            14f
        },
        {
            TechType.PurpleRattleSpore,
            14f
        },
        {
            TechType.MelonSeed,
            70f
        },
        {
            TechType.PurpleBrainCoralPiece,
            70f
        },
        {
            TechType.SpikePlantSeed,
            70f
        },
        {
            TechType.BluePalmSeed,
            70f
        },
        {
            TechType.PurpleFanSeed,
            21f
        },
        {
            TechType.SmallFanSeed,
            28f
        },
        {
            TechType.PurpleTentacleSeed,
            70f
        },
        {
            TechType.JellyPlantSeed,
            7f
        },
        {
            TechType.GabeSFeatherSeed,
            70f
        },
        {
            TechType.SeaCrownSeed,
            70f
        },
        {
            TechType.MembrainTreeSeed,
            70f
        },
        {
            TechType.PinkFlowerSeed,
            28f
        },
        {
            TechType.FernPalmSeed,
            70f
        },
        {
            TechType.OrangePetalsPlantSeed,
            105f
        },
        {
            TechType.EyesPlantSeed,
            70f
        },
        {
            TechType.RedGreenTentacleSeed,
            70f
        },
        {
            TechType.PurpleStalkSeed,
            70f
        },
        {
            TechType.RedBasketPlantSeed,
            70f
        },
        {
            TechType.RedBushSeed,
            70f
        },
        {
            TechType.RedConePlantSeed,
            70f
        },
        {
            TechType.SpottedLeavesPlantSeed,
            70f
        },
        {
            TechType.RedRollPlantSeed,
            70f
        },
        {
            TechType.PurpleBranchesSeed,
            70f
        },
        {
            TechType.SnakeMushroomSpore,
            140f
        },
        {
            TechType.CreepvineSeedCluster,
            70f
        },
        {
            TechType.BloodOil,
            420f
        },
        {
            TechType.Bladderfish,
            210f
        },
        {
            TechType.Boomerang,
            280f
        },
        {
            TechType.LavaBoomerang,
            280f
        },
        {
            TechType.Eyeye,
            420f
        },
        {
            TechType.LavaEyeye,
            420f
        },
        {
            TechType.GarryFish,
            420f
        },
        {
            TechType.HoleFish,
            280f
        },
        {
            TechType.Hoopfish,
            210f
        },
        {
            TechType.Spinefish,
            210f
        },
        {
            TechType.Hoverfish,
            350f
        },
        {
            TechType.Oculus,
            630f
        },
        {
            TechType.Peeper,
            420f
        },
        {
            TechType.Reginald,
            490f
        },
        {
            TechType.Spadefish,
            420f
        },
        {
            TechType.CookedBladderfish,
            157.5f
        },
        {
            TechType.CookedBoomerang,
            210f
        },
        {
            TechType.CookedLavaBoomerang,
            210f
        },
        {
            TechType.CookedEyeye,
            315f
        },
        {
            TechType.CookedLavaEyeye,
            315f
        },
        {
            TechType.CookedGarryFish,
            245f
        },
        {
            TechType.CookedHoleFish,
            210f
        },
        {
            TechType.CookedHoopfish,
            157.5f
        },
        {
            TechType.CookedSpinefish,
            157.5f
        },
        {
            TechType.CookedHoverfish,
            262.5f
        },
        {
            TechType.CookedOculus,
            472.5f
        },
        {
            TechType.CookedPeeper,
            315f
        },
        {
            TechType.CookedReginald,
            367.5f
        },
        {
            TechType.CookedSpadefish,
            315f
        },
        {
            TechType.CuredBladderfish,
            119f
        },
        {
            TechType.CuredBoomerang,
            157.5f
        },
        {
            TechType.CuredLavaBoomerang,
            157.5f
        },
        {
            TechType.CuredEyeye,
            245f
        },
        {
            TechType.CuredLavaEyeye,
            245f
        },
        {
            TechType.CuredGarryFish,
            182f
        },
        {
            TechType.CuredHoleFish,
            157.5f
        },
        {
            TechType.CuredHoopfish,
            119f
        },
        {
            TechType.CuredSpinefish,
            119f
        },
        {
            TechType.CuredHoverfish,
            196f
        },
        {
            TechType.CuredOculus,
            353.5f
        },
        {
            TechType.CuredPeeper,
            238f
        },
        {
            TechType.CuredReginald,
            273f
        },
        {
            TechType.CuredSpadefish,
            238f
        },
        {
            TechType.Jumper,
            280f
        },
        {
            TechType.RabbitRay,
            420f
        },
        {
            TechType.Stalker,
            560f
        },
        {
            TechType.Jellyray,
            350f
        },
        {
            TechType.Gasopod,
            700f
        },
        {
            TechType.Sandshark,
            630f
        },
        {
            TechType.BoneShark,
            630f
        },
        {
            TechType.CrabSquid,
            770f
        },
        {
            TechType.Mesmer,
            560f
        },
        {
            TechType.Biter,
            140f
        },
        {
            TechType.Crabsnake,
            700f
        },
        {
            TechType.Shocker,
            770f
        },
        {
            TechType.Shuttlebug,
            280f
        },
        {
            TechType.LavaLizard,
            560f
        },
        {
            TechType.Reefback,
            840f
        },
        {
            TechType.Crash,
            560f
        },
        {
            TechType.SafeShallowsEgg,
            350f
        },
        {
            TechType.KelpForestEgg,
            350f
        },
        {
            TechType.GrassyPlateausEgg,
            350f
        },
        {
            TechType.GrandReefsEgg,
            350f
        },
        {
            TechType.MushroomForestEgg,
            350f
        },
        {
            TechType.KooshZoneEgg,
            350f
        },
        {
            TechType.TwistyBridgesEgg,
            350f
        },
        {
            TechType.StalkerEgg,
            105f
        },
        {
            TechType.StalkerEggUndiscovered,
            105f
        },
        {
            TechType.ReefbackEgg,
            280f
        },
        {
            TechType.ReefbackEggUndiscovered,
            280f
        },
        {
            TechType.SpadefishEgg,
            140f
        },
        {
            TechType.SpadefishEggUndiscovered,
            140f
        },
        {
            TechType.RabbitrayEgg,
            140f
        },
        {
            TechType.RabbitrayEggUndiscovered,
            140f
        },
        {
            TechType.MesmerEgg,
            175f
        },
        {
            TechType.MesmerEggUndiscovered,
            175f
        },
        {
            TechType.JumperEgg,
            105f
        },
        {
            TechType.JumperEggUndiscovered,
            105f
        },
        {
            TechType.SandsharkEgg,
            210f
        },
        {
            TechType.SandsharkEggUndiscovered,
            210f
        },
        {
            TechType.JellyrayEgg,
            119f
        },
        {
            TechType.JellyrayEggUndiscovered,
            119f
        },
        {
            TechType.BonesharkEgg,
            210f
        },
        {
            TechType.BonesharkEggUndiscovered,
            210f
        },
        {
            TechType.CrabsnakeEgg,
            231f
        },
        {
            TechType.CrabsnakeEggUndiscovered,
            231f
        },
        {
            TechType.ShockerEgg,
            259f
        },
        {
            TechType.ShockerEggUndiscovered,
            259f
        },
        {
            TechType.GasopodEgg,
            231f
        },
        {
            TechType.GasopodEggUndiscovered,
            231f
        },
        {
            TechType.CrashEgg,
            189f
        },
        {
            TechType.CrashEggUndiscovered,
            189f
        },
        {
            TechType.CutefishEgg,
            210f
        },
        {
            TechType.CutefishEggUndiscovered,
            210f
        },
        {
            TechType.CrabsquidEgg,
            259f
        },
        {
            TechType.CrabsquidEggUndiscovered,
            259f
        },
        {
            TechType.LavaLizardEgg,
            189f
        },
        {
            TechType.LavaLizardEggUndiscovered,
            189f
        },
        {
            TechType.Floater,
            50f
        },
        {
            TechType.Lubricant,
            20f
        },
        {
            TechType.SeaTreaderPoop,
            300f
        }
    };
}

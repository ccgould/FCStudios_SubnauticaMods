using System.Reflection;
using FCS_LifeSupportSolutions.Buildable;
using FCS_LifeSupportSolutions.Configuration;
using FCS_LifeSupportSolutions.Mods.BaseOxygenTank.Buildable;
using FCS_LifeSupportSolutions.Mods.BaseUtilityUnit.Buildable;
using FCS_LifeSupportSolutions.Mods.EnergyPillVendingMachine.Buildable;
using FCS_LifeSupportSolutions.Mods.MiniMedBay.Buildable;
using FCS_LifeSupportSolutions.Spawnables;
using FCSCommon.Utilities;
using HarmonyLib;
using QModManager.API.ModLoading;
using SMLHelper.V2.Handlers;

namespace FCS_LifeSupportSolutions
{

    /*
     * If you are trying to build the project after changing it for subnautica check the build settings
     * make sure all build settings line up with the correct engine.
     */

    [QModCore]
    public class QPatch
    {
        public static bool IsRefillableOxygenTanksInstalled { get; } =
            TechTypeHandler.ModdedTechTypeExists("HighCapacityTankRefill");


        internal string Version { get; private set; } 
        internal static Config Configuration { get; } = OptionsPanelHandler.Main.RegisterModOptions<Config>();
        internal static BaseUtilityUnitConfig BaseUtilityUnitConfiguration { get; } = OptionsPanelHandler.Main.RegisterModOptions<BaseUtilityUnitConfig>();
        public static bool IsEnabled { get; set; }

        [QModPatch]
        public void Patch()
        {
            Version = QuickLogger.GetAssemblyVersion(Assembly.GetExecutingAssembly());
            QuickLogger.Info($"Started patching. Version: {Version}");
            QuickLogger.ModName = Mod.ModName;
            AuxPatchers.AdditionalPatching();
            ModelPrefab.Initialize();
            
            var energyPillVendingMachine = new EnergyPillVendingMachinePatcher();
            energyPillVendingMachine.Patch();

            var redEnergyPill = new PillPatch("RedEnergyPill", "Red Adrenaline Pill",
                "The red adrenaline pill refills you adrenaline bar to give you 2 minutes of speed when thrusty or hungry",
                ModelPrefab.RedEnergyPillPrefab);
            redEnergyPill.Patch();
            Mod.RedEnergyPillTechType = redEnergyPill.TechType;

            var greenEnergyPill = new PillPatch("GreenEnergyPill", "Green Adrenaline Pill",
                "The red adrenaline pill adds to your adrenaline bar to give you an additional minute of speed when thrusty or hungry",
                ModelPrefab.RedEnergyPillPrefab);
            greenEnergyPill.Patch();
            Mod.GreenEnergyPillTechType = greenEnergyPill.TechType;

            var blueEnergyPill = new PillPatch("BlueEnergyPill", "Blue Adrenaline Pill",
                "The red adrenaline pill adds to your adrenaline bar to give you an additional 30 seconds of speed when thrusty or hungry",
                ModelPrefab.RedEnergyPillPrefab);
            blueEnergyPill.Patch();
            Mod.BlueEnergyPillTechType = blueEnergyPill.TechType;

            var miniMedBay = new MiniMedBayPatcher();
            miniMedBay.Patch();

            if (BaseUtilityUnitConfiguration.IsModEnabled)
            {
                var baseUtilityUnit = new BaseUtilityUnitPatch();
                baseUtilityUnit.Patch();
                var baseOxygenTankPatch = new BaseOxygenTankPatch();
                baseOxygenTankPatch.Patch();
            }


            //Register debug commands
            ConsoleCommandsHandler.Main.RegisterConsoleCommands(typeof(DebugCommands));

            var harmony = new Harmony("com.lifesupportsolutions.fstudios");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}

//BarsPanel
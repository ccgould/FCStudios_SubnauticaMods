using System.Reflection;
using FCS_AlterraHub.Registration;
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

        [QModPatch]
        public void Patch()
        {
            FCSAlterraHubService.PublicAPI.RegisterModPack(Mod.ModPackID, Mod.ModBundleName, Assembly.GetExecutingAssembly());
            FCSAlterraHubService.PublicAPI.RegisterEncyclopediaEntry(Mod.ModPackID);

            AuxPatchers.AdditionalPatching();
            ModelPrefab.Initialize();

            if (Configuration.IsEnergyPillVendingMachineEnabled)
            {
                var energyPillVendingMachine = new EnergyPillVendingMachinePatcher();
                energyPillVendingMachine.Patch();

                var redEnergyPill = new PillPatch("RedEnergyPill", "Red Adrenaline Pill",
                    "The red adrenaline pill refills your adrenaline bar to give you a 2 minute boost, returning your speed to normal when you are extremely hungry or thirsty.",
                    ModelPrefab.RedEnergyPillPrefab);
                redEnergyPill.Patch();
                Mod.RedEnergyPillTechType = redEnergyPill.TechType;

                var greenEnergyPill = new PillPatch("GreenEnergyPill", "Green Adrenaline Pill",
                    "The green adrenaline pill refills your adrenaline bar to give you a 1 minute boost, returning your speed to normal when you are extremely hungry or thirsty.",
                    ModelPrefab.GreenEnergyPillPrefab);
                greenEnergyPill.Patch();
                Mod.GreenEnergyPillTechType = greenEnergyPill.TechType;

                var blueEnergyPill = new PillPatch("BlueEnergyPill", "Blue Adrenaline Pill",
                    "The blue adrenaline pill refills your adrenaline bar to give you a 30 second boost, returning your speed to normal when you are extremely hungry or thirsty.",
                    ModelPrefab.BlueEnergyPillPrefab);
                blueEnergyPill.Patch();
                Mod.BlueEnergyPillTechType = blueEnergyPill.TechType;
            }

            if (Configuration.IsMiniMedBayEnabled)
            {
                var miniMedBay = new MiniMedBayPatcher();
                miniMedBay.Patch();
            }

            if (Configuration.BaseUtilityUnitIsModEnabled)
            {
                var baseUtilityUnit = new BaseUtilityUnitPatch();
                baseUtilityUnit.Patch();
                var baseOxygenTankPatchKit = new BaseOxygenTankPatch($"{Mod.BaseOxygenTankClassID}KitType", $"{Mod.BaseOxygenTankClassID}KitType", true);
                baseOxygenTankPatchKit.Patch();                
                
                var baseOxygenTankPatchResources = new BaseOxygenTankPatch(Mod.BaseOxygenTankClassID, Mod.BaseOxygenTankClassID);
                baseOxygenTankPatchResources.Patch();
            }
            
            //Register debug commands
            ConsoleCommandsHandler.Main.RegisterConsoleCommands(typeof(DebugCommands));

            var harmony = new Harmony("com.lifesupportsolutions.fstudios");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
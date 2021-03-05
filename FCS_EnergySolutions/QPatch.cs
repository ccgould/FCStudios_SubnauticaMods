using System;
using System.Collections.Generic;
using System.Reflection;
using FCS_EnergySolutions.AlterraGen.Buildables;
using FCS_EnergySolutions.AlterraSolarCluster.Buildables;
using FCS_EnergySolutions.Buildable;
using FCS_EnergySolutions.Configuration;
using FCS_EnergySolutions.JetStreamT242.Buildables;
using FCS_EnergySolutions.Mods.TelepowerPylon.Mono;
using FCS_EnergySolutions.Spawnables;
using FCS_EnergySolutions.TelepowerPylon.Buildables;
using FCSCommon.Utilities;
using HarmonyLib;
using QModManager.API.ModLoading;
using SMLHelper.V2.Handlers;
using UnityEngine;

namespace FCS_EnergySolutions
{

    /*
     * If you are trying to build the project after changing it for subnautica check the build settings
     * make sure all build settings line up with the correct engine.
     */

    [QModCore]
    public class QPatch
    {
        internal string Version { get; private set; }
        internal static Config Configuration { get; } = OptionsPanelHandler.Main.RegisterModOptions<Config>();

        [QModPatch]
        public void Patch()
        {
            Version = QuickLogger.GetAssemblyVersion(Assembly.GetExecutingAssembly());
            QuickLogger.Info($"Started patching. Version: {Version}");
            QuickLogger.ModName = Mod.ModName;
            AuxPatchers.AdditionalPatching();
            ModelPrefab.Initialize();

            if (Configuration.IsAlterraGenEnabled)
            {
                var alterraGen = new AlterraGenBuildable();
                alterraGen.Patch();
            }

            if (Configuration.IsAlterraSolarPanelClusterEnabled)
            {
                var alterraSolarCluster = new AlterraSolarClusterBuildable();
                alterraSolarCluster.Patch();
            }

            if (Configuration.IsJetStreamT242Enabled)
            {
                var jetStreamT242 = new JetStreamT242Patcher();
                jetStreamT242.Patch();
            }

            if (Configuration.IsTelepowerPylonEnabled)
            {
                var telepowerPylon = new TelepowerPylonBuildable();
                telepowerPylon.Patch();

                var mk2PylonUpgrade = new TelepowerUpgradeSpawnable("TelepowerMk2Upgrade", "Telepower MK2 Upgrade",
                    "Allows you to upgrade your Telepower Pylon to the MK2 level which allows you to connect to 8 devices",
                    1000000, Color.cyan);
                mk2PylonUpgrade.Patch();

                var mk3PylonUpgrade = new TelepowerUpgradeSpawnable("TelepowerMk3Upgrade", "Telepower MK3 Upgrade",
                    "Allows you to upgrade your Telepower Pylon to the MK3 level which allows you to connect to 10 devices",
                    2000000, Color.green);
                mk3PylonUpgrade.Patch();
            }


            if (Configuration.IsPowerStorageEnabled)
            {
                var powerStorage = new PowerStoragePatcher();
                powerStorage.Patch();
            }
            
            //Register debug commands
            ConsoleCommandsHandler.Main.RegisterConsoleCommands(typeof(DebugCommands));
        }
    }
}

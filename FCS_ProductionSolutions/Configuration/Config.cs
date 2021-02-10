using System;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Model;
using FCSCommon.Extensions;
using SMLHelper.V2.Json;
using SMLHelper.V2.Options.Attributes;
using FCSCommon.Utilities;
using Oculus.Newtonsoft.Json;
using SMLHelper.V2.Options;
using UnityEngine;

namespace FCS_ProductionSolutions.Configuration
{

    [Menu("FCS Production Solutions Menu")]
    public class Config : ConfigFile
    {
        public Config() : base("productionSolutions-config", "Configurations") { }

        [Toggle("[Production Solutions] Enable Debugs",Order = 0), OnChange(nameof(EnableDebugsToggleEvent)),Tooltip("Enables debug logs set in code by FCStudios (Maybe asked to be enabled for bug reports)")]
        public bool EnableDebugLogs = false;

        #region Matter Analyzer

        [Toggle("[Matter Analyzer] Is Mod Enabled"),
         Tooltip("Enables/Disables Matter Analyzer from your game (*Note: Game must be restarted for changes to take effect. Its best to destroy all objects before disabling a mod)")]
        public bool IsMatterAnalyzerEnabled = true;

        [Toggle("[Matter Analyzer] Toggle Fx")]
        public bool MatterAnalyzerPlaySFX = true;

        #endregion

        #region Replicator

        [Toggle("[Replicator] Is Mod Enabled"),
         Tooltip("Enables/Disables Replicator from your game (*Note: Game must be restarted for changes to take effect. Its best to destroy all objects before disabling a mod)")]
        public bool IsReplicatorEnabled = true;

        #endregion

        #region Harvester

        [Toggle("[Hydroponic Harvester]] Is Mod Enabled"),
         Tooltip("Enables/Disables Hydroponic Harvester from your game (*Note: Game must be restarted for changes to take effect. Its best to destroy all objects before disabling a mod)")]
        public bool IsHydroponicHarvesterEnabled = true;

        [Toggle("[Hydroponic Harvester] Enable/Disable Light Trigger")]
        public bool HHIsLightTriggerEnabled = true;

        [Slider("[Hydroponic Harvester] Light Trigger Range", 0, 20)]
        public int HHLightTriggerRange = 4;

        #endregion

        #region Deep Driller

        [Toggle("[Deep Driller MK3] Is Mod Enabled"),
         Tooltip("Enables/Disables Deep Driller MK3 from your game (*Note: Game must be restarted for changes to take effect. Its best to destroy all objects before disabling a mod)")]
        public bool IsDeepDrillerEnabled = true;

        public int DDStorageSize  = 300;
        public float DDPowerDraw  = 0.7f;
        public float DDChargePullAmount  = 1.5f;
        public float DDSolarCapacity  = 125;

        [Toggle("[Deep Driller MK3] Is HardCore Mode")]
        public bool DDHardCoreMode  = true;
        public float DDOilTimePeriodInDays  = 30.0f;
        public float DDOilRestoresInDays  = 5.0f;
        public Dictionary<string, List<string>> DDAdditionalBiomeOres  = new Dictionary<string, List<string>>();
        public float DDMaxOreCountUpgradePowerUsage  = 0.2f;
        public float DDOrePerDayUpgradePowerUsage  = 1.0f;
        public float DDInternalBatteryCapacity  = 1000f;
        public float DDDrillAlterraStorageRange  = 30f;
        [JsonIgnore] internal float DDOreReductionValue => 0.08f;
        [JsonIgnore] internal Dictionary<string, List<TechType>> DDBiomeOresTechType  = new Dictionary<string, List<TechType>>();

        internal void Convert()
        {
            try
            {
                foreach (KeyValuePair<string, List<string>> biomeOre in DDAdditionalBiomeOres)
                {
                    var types = new List<TechType>();

                    foreach (string sTechType in biomeOre.Value)
                    {
                        types.Add(sTechType.ToTechType());
                    }
                    QuickLogger.Debug($"Added {biomeOre.Key} to BiomeOresTechType");
                    if (!DDBiomeOresTechType.ContainsKey(biomeOre.Key))
                    {
                        DDBiomeOresTechType.Add(biomeOre.Key, types);
                    }
                    else
                    {
                        DDBiomeOresTechType[biomeOre.Key] = DDBiomeOresTechType[biomeOre.Key].Union(types).ToList();
                    }

                }
            }
            catch (Exception e)
            {
                QuickLogger.Error($"Error: {e.Message} || Stack Trace: {e.StackTrace}");
            }
        }

        #endregion

        private void EnableDebugsToggleEvent(ToggleChangedEventArgs e)
        {
            if (e.Value)
            {
                QuickLogger.DebugLogsEnabled = true;
                QuickLogger.Debug("Debug logs enabled");
            }
            else
            {
                QuickLogger.DebugLogsEnabled = false;
                QuickLogger.Info("Debug logs disabled");
            }
        }
    }
}

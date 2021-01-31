using System;
using System.Collections.Generic;
using System.Linq;
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

        [Toggle("[Production Solutions] Enable Debugs"), OnChange(nameof(EnableDebugsToggleEvent))]
        public bool EnableDebugLogs = false;

        #region Matter Analyzer

        [Toggle("[Matter Analyzer] Toggle Fx")]
        public bool MatterAnalyzerPlaySFX { get; set; } = true;

        #endregion

        #region Harvester

        [Toggle("[Hydroponic Harvester] Enable/Disable Light Trigger")]
        public bool HHIsLightTriggerEnabled = true;

        [Slider("[Hydroponic Harvester] Light Trigger Range", 0, 20)]
        public int HHLightTriggerRange = 4;

        #endregion

        #region Deep Driller

        public int DDStorageSize { get; set; } = 300;
        public float DDPowerDraw { get; set; } = 0.7f;
        public float DDChargePullAmount { get; set; } = 1.5f;
        public float DDSolarCapacity { get; set; } = 125;

        [Toggle("[Deep Driller MK3] Is HardCore Mode")]
        public bool DDHardCoreMode { get; set; } = true;
        public float DDOilTimePeriodInDays { get; set; } = 30.0f;
        public float DDOilRestoresInDays { get; set; } = 5.0f;
        public Dictionary<string, List<string>> DDAdditionalBiomeOres { get; set; } = new Dictionary<string, List<string>>();
        public float DDMaxOreCountUpgradePowerUsage { get; set; } = 0.2f;
        public float DDOrePerDayUpgradePowerUsage { get; set; } = 1.0f;
        public float DDInternalBatteryCapacity { get; set; } = 1000f;
        public float DDDrillAlterraStorageRange { get; set; } = 30f;
        [JsonIgnore] internal float DDOreReductionValue => 0.08f;
        [JsonIgnore] internal Dictionary<string, List<TechType>> DDBiomeOresTechType { get; set; } = new Dictionary<string, List<TechType>>();

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

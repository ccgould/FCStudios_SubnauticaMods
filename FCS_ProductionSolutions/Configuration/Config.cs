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

        [Toggle("Enable Debugs"), OnChange(nameof(EnableDebugsToggleEvent))]
        public bool EnableDebugLogs = false;

        public float EnergyConsumpion { get; set; } = 15000f;
        
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

    [Menu("Hydroponic Harvester Menu")]
    public class HarvesterConfig : ConfigFile
    {
        public HarvesterConfig() : base("harvester-config", "Configurations") { }

        [Toggle("Enable/Disable Light Trigger")]
        public bool IsLightTriggerEnabled = true;

        [Slider("Light Trigger Range", 0, 20)]
        public int LightTriggerRange = 4;
    }

    [Menu("Deep Driller Mk2 Menu")]
    public class DeepDrillerMk2Config : ConfigFile
    {
        public DeepDrillerMk2Config() : base("deepdrillermk2-config", "Configurations") { }

        public int StorageSize { get; set; } = 300;
        public float PowerDraw { get; set; } = 0.7f;
        public float ChargePullAmount { get; set; } = 1.5f;
        public float SolarCapacity { get; set; } = 125;

        [Toggle("Is HardCore Mode")]
        public bool HardCoreMode { get; set; } = true;
        public float OilTimePeriodInDays { get; set; } = 30.0f;
        public float OilRestoresInDays { get; set; } = 5.0f;
        public Dictionary<string, List<string>> AdditionalBiomeOres { get; set; } = new Dictionary<string, List<string>>();
        public float MaxOreCountUpgradePowerUsage { get; set; } = 0.2f;
        public float OrePerDayUpgradePowerUsage { get; set; } = 1.0f;
        public float InternalBatteryCapacity { get; set; } = 1000f;
        public float DrillAlterraStorageRange { get; set; } = 30f;
        [JsonIgnore] internal float OreReductionValue => 0.08f;
        [JsonIgnore] internal Dictionary<string, List<TechType>> BiomeOresTechType { get; set; } = new Dictionary<string, List<TechType>>();

        internal void Convert()
        {
            try
            {
                foreach (KeyValuePair<string, List<string>> biomeOre in AdditionalBiomeOres)
                {
                    var types = new List<TechType>();

                    foreach (string sTechType in biomeOre.Value)
                    {
                        types.Add(sTechType.ToTechType());
                    }
                    QuickLogger.Debug($"Added {biomeOre.Key} to BiomeOresTechType");
                    if (!BiomeOresTechType.ContainsKey(biomeOre.Key))
                    {
                        BiomeOresTechType.Add(biomeOre.Key, types);
                    }
                    else
                    {
                        BiomeOresTechType[biomeOre.Key] = BiomeOresTechType[biomeOre.Key].Union(types).ToList();
                    }

                }
            }
            catch (Exception e)
            {
                QuickLogger.Error($"Error: {e.Message} || Stack Trace: {e.StackTrace}");
            }
        }
    }
}

using System;
using FCSCommon.Extensions;
using FCSCommon.Utilities;
using Oculus.Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace FCS_DeepDriller.Configuration
{
    public class DeepDrillerCfg
    {
        #region Public declarations

        public int StorageSize { get; set; } = 100;
        public float PowerDraw { get; set; } = 0.7f;
        public float ChargePullAmount { get; set; } = 0.3f;
        public float SolarCapacity { get; set; } = 125;
        public bool HardCoreMode { get; set; } = true;
        public float OilTimePeriodInDays { get; set; } = 30.0f;
        public float OilRestoresInDays { get; set; } = 5.0f;
        public Dictionary<string, List<string>> AdditionalBiomeOres { get; set; } = new Dictionary<string, List<string>>();
        public float MaxOreCountUpgradePowerUsage { get; set; } = 0.2f;
        public float OrePerDayUpgradePowerUsage { get; set; } = 1.0f;
        public float InternalBatteryCapacity { get; set; } = 1000f;
        public float DrillExStorageRange { get; set; } = 30f;
        [JsonIgnore] internal float OreReductionValue => 0.08f;
        [JsonIgnore] internal Dictionary<string, List<TechType>> BiomeOresTechType { get; set; } = new Dictionary<string, List<TechType>>();
        [JsonIgnore] internal float DrillExStorageMaxRange { get; set; } = 100f;


        #endregion
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

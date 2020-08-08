using FCSCommon.Extensions;
using FCSCommon.Utilities;
using Oculus.Newtonsoft.Json;
using System.Collections.Generic;

namespace FCS_DeepDriller.Configuration
{
    public class DeepDrillerCfg
    {
        #region Public declarations

        public int StorageSize { get; set; } = 100;
        public int CrafterStorageSize { get; set; } = 100;

        public float PowerDraw { get; set; } = 0.1f;
        public float SolarCapacity { get; set; } = 125;
        public bool AllowDamage { get; set; } = true;
        public float OilTimePeriodInDays { get; set; } = 30.0f;
        public float OilRestoresInDays { get; set; } = 5.0f;
        public Dictionary<string, List<string>> AdditionalBiomeOres { get; set; } = new Dictionary<string, List<string>>();


        [JsonIgnore] internal Dictionary<string, List<TechType>> BiomeOresTechType { get; set; } = new Dictionary<string, List<TechType>>();


        #endregion
        internal void Convert()
        {
            foreach (KeyValuePair<string, List<string>> biomeOre in AdditionalBiomeOres)
            {
                var types = new List<TechType>();

                foreach (string sTechType in biomeOre.Value)
                {
                    types.Add(sTechType.ToTechType());
                }
                QuickLogger.Debug($"Added {biomeOre.Key} to BiomeOresTechType");
                BiomeOresTechType.Add(biomeOre.Key, types);
            }
        }
    }
}

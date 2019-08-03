using FCSCommon.Extensions;
using FCSCommon.Objects;
using FCSCommon.Utilities;
using Oculus.Newtonsoft.Json;
using System.Collections.Generic;

namespace FCS_DeepDriller.Configuration
{
    public class DeepDrillerCfg
    {
        #region Public declarations
        public StorageConfig StorageSize { get; set; } = new StorageConfig { Height = 8, Width = 6 };

        public float PowerDraw { get; set; } = 0.1f;
        public float SolarCapacity { get; set; } = 125;

        public Dictionary<string, List<string>> BiomeOres { get; set; } = new Dictionary<string, List<string>>();

        [JsonIgnore] internal Dictionary<string, List<TechType>> BiomeOresTechType { get; set; } = new Dictionary<string, List<TechType>>();

        #endregion
        internal void Convert()
        {
            foreach (KeyValuePair<string, List<string>> biomeOre in BiomeOres)
            {
                var types = new List<TechType>();

                foreach (string sTechType in biomeOre.Value)
                {
                    types.Add(sTechType.ToTechType());
                }
                QuickLogger.Debug($"Added {biomeOre.Key} to BiomeOresTechType");
                BiomeOresTechType.Add(biomeOre.Key, types);
            }

            BiomeOres = null;
        }
    }
}

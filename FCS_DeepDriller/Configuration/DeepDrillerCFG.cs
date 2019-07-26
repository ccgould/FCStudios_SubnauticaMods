using FCSCommon.Extensions;
using FCSCommon.Objects;
using Oculus.Newtonsoft.Json;
using System.Collections.Generic;

namespace FCS_DeepDriller.Configuration
{
    public class DeepDrillerCfg
    {
        #region Public declarations
        public static bool _debug = false;

        public StorageConfig StorageSize { get; set; } = new StorageConfig { Height = 10, Width = 8 };

        public float PowerDraw { get; set; } = 0.1f;

        public bool EnableWear { get; set; } = true;
        public float SolarCapacity { get; set; } = 3000;

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

                BiomeOresTechType.Add(biomeOre.Key, types);
            }

            BiomeOres = null;
        }
    }
}

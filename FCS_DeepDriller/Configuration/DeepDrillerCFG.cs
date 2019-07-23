using FCSCommon.Objects;
using System.Collections.Generic;

namespace FCS_DeepDriller.Configuration
{
    public class DeepDrillerCfg
    {
        #region Public declarations
        public static bool _debug = false;

        public StorageConfig StorageSize { get; set; } = new StorageConfig { Height = 10, Width = 8 };

        public float PowerDraw { get; set; } = 1.666666666666667f;

        public bool EnableWear { get; set; } = true;
        public float SolarCapacity { get; set; } = 3000;

        public Dictionary<string, List<string>> BiomeOres = new Dictionary<string, List<string>>();
        #endregion
    }
}

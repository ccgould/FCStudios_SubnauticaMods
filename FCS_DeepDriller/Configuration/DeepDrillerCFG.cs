using FCSCommon.Objects;
using System.Collections.Generic;

namespace FCS_DeepDriller.Configuration
{
    public class DeepDrillerCfg
    {
        #region Public declarations
        public static bool _debug = false;

        public StorageConfig StorageSize { get; set; } = new StorageConfig { Height = 10, Width = 8 };

        public bool EnableWear { get; set; } = true;

        public Dictionary<string, List<string>> BiomeOres = new Dictionary<string, List<string>>();
        #endregion
    }
}

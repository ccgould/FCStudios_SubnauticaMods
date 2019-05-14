using FCSAlterraIndustrialSolutions.Data;
using System.Collections.Generic;

namespace FCSAlterraIndustrialSolutions.Configuration
{
    public class Cfg
    {
        #region Public declarations

        [DoNotSerialize]
        public static bool Debug = false;

        public int DamageCycleInSec { get; set; } = 1200;

        public float MaxCapacity { get; set; } = 200;

        public bool EnableWear { get; set; } = true;

        public Dictionary<string, AISolutionsData.BiomeItem> BiomeSpeeds { get; set; } =
            new Dictionary<string, AISolutionsData.BiomeItem>();


        #endregion
    }
}

using FCS_AIMarineTurbine.Model;
using System.Collections.Generic;

namespace FCS_AIMarineTurbine.Configuration
{
    internal class JetStreamT242Config
    {
        #region Public declarations

        [DoNotSerialize]
        public static bool Debug = false;

        public int RotationCycleInSec { get; set; } = 1200;

        public float MaxCapacity { get; set; } = 300;

        public bool EnableWear { get; set; } = true;

        public Dictionary<string, AISolutionsData.BiomeItem> BiomeSpeeds { get; set; } =
            new Dictionary<string, AISolutionsData.BiomeItem>();


        #endregion
    }
}

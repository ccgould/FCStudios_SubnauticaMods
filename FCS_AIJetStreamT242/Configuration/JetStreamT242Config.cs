using FCS_AIJetStreamT242.Model;
using System.Collections.Generic;

namespace FCS_AIJetStreamT242.Configuration
{
    internal class JetStreamT242Config
    {
        #region Public declarations

        [DoNotSerialize]
        public static bool Debug = false;

        public int RotationCycleInSec { get; set; } = 1200;

        public float MaxCapacity { get; set; } = 200;

        public bool EnableWear { get; set; } = true;

        public Dictionary<string, AISolutionsData.BiomeItem> BiomeSpeeds { get; set; } =
            new Dictionary<string, AISolutionsData.BiomeItem>();


        #endregion
    }
}

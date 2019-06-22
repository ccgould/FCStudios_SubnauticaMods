using FCSCommon.Objects;
using System.Collections.Generic;

namespace ARS_SeaBreezeFCS32.Model
{
    internal class SaveData
    {
        public float RemainingTime { get; set; }

        public int FreonCount { get; set; }

        public IEnumerable<EatableEntities> FridgeContainer { get; set; }

        public bool HasBreakerTripped { get; set; }
    }
}

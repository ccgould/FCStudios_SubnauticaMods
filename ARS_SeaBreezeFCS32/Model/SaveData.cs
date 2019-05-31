using FCSCommon.Objects;
using System.Collections.Generic;

namespace ARS_SeaBreezeFCS32.Model
{
    internal class SaveData
    {
        public float FilterHealth { get; set; }

        public List<EatableEntities> FridgeContainer { get; set; }

        public bool HasBreakerTripped { get; set; }
    }
}

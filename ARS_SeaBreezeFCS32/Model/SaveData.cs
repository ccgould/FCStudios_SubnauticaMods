using FCSCommon.Enums;
using FCSCommon.Objects;
using System.Collections.Generic;

namespace ARS_SeaBreezeFCS32.Model
{
    internal class SaveData
    {
        public float RemaingTime { get; set; }

        public FilterTypes FilterType { get; set; }

        public FilterState FilterState { get; set; }

        public TechType FilterTechType { get; set; }

        public IEnumerable<EatableEntities> FridgeContainer { get; set; }

        public bool HasBreakerTripped { get; set; }
    }
}
